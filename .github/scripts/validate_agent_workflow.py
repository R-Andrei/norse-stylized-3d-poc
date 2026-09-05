#!/usr/bin/env python3
"""Validate repository invariants and issue/thread/branch identity for agent PRs."""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
import urllib.error
import urllib.request
from pathlib import Path
from typing import Any

EXPECTED_UNITY_VERSION = "6000.5.0f1"
THREAD_PATTERN = r"t-\d{8}-\d{4}-[a-z0-9]{6}"
BRANCH_RE = re.compile(
    rf"^issue-(?P<issue>[1-9]\d*)/(?P<thread>{THREAD_PATTERN})-"
    r"(?P<slug>[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)$"
)
THREAD_RE = re.compile(rf"^{THREAD_PATTERN}$")
CLAIM_MARKER = "<!-- agent-thread-claim:v1 -->"
ALLOWED_STATUSES = {"ACTIVE", "INTEGRATED", "RELEASED", "COMPLETED"}
FORBIDDEN_TRACKED_PREFIXES = (
    "Library/",
    "Temp/",
    "Logs/",
    "obj/",
    "Build/",
    "Builds/",
    "UserSettings/",
)


class PolicyFailure(RuntimeError):
    pass


def require(condition: bool, message: str) -> None:
    if not condition:
        raise PolicyFailure(message)


def read_event(path: str) -> dict[str, Any]:
    with open(path, "r", encoding="utf-8") as handle:
        return json.load(handle)


def field_from_markdown(body: str, label: str) -> str | None:
    match = re.search(
        rf"(?mi)^\s*(?:[-*]\s*)?{re.escape(label)}\s*:\s*(.+?)\s*$",
        body or "",
    )
    return match.group(1).strip() if match else None


def parse_claim(body: str, comment_id: int) -> dict[str, Any] | None:
    if CLAIM_MARKER not in (body or ""):
        return None

    thread_id = field_from_markdown(body, "Thread-ID")
    branch = field_from_markdown(body, "Branch")
    base = field_from_markdown(body, "Base")
    status = field_from_markdown(body, "Status")
    scope = field_from_markdown(body, "Scope")
    supersedes_raw = field_from_markdown(body, "Supersedes") or ""
    supersedes = [
        value.strip()
        for value in supersedes_raw.split(",")
        if value.strip()
    ]

    require(thread_id is not None and THREAD_RE.fullmatch(thread_id) is not None,
            f"Claim comment {comment_id} has an invalid Thread-ID.")
    require(branch is not None and BRANCH_RE.fullmatch(branch) is not None,
            f"Claim comment {comment_id} has an invalid Branch.")
    require(base is not None and re.fullmatch(r"fufu@[0-9a-f]{7,40}", base) is not None,
            f"Claim comment {comment_id} has an invalid Base; expected fufu@<commit>.")
    require(status is not None and status.upper() in ALLOWED_STATUSES,
            f"Claim comment {comment_id} has an invalid Status.")
    require(scope is not None and bool(scope.strip()),
            f"Claim comment {comment_id} must include a non-empty Scope.")
    for superseded in supersedes:
        require(THREAD_RE.fullmatch(superseded) is not None,
                f"Claim comment {comment_id} has invalid Supersedes Thread-ID {superseded!r}.")

    return {
        "comment_id": comment_id,
        "thread_id": thread_id,
        "branch": branch,
        "base": base,
        "status": status.upper(),
        "scope": scope,
        "supersedes": supersedes,
    }


def resolve_active_claims(claims: list[dict[str, Any]]) -> list[dict[str, Any]]:
    latest_by_thread: dict[str, dict[str, Any]] = {}
    permanently_superseded: set[str] = set()
    for claim in claims:
        latest_by_thread[claim["thread_id"]] = claim
        permanently_superseded.update(claim["supersedes"])

    return [
        claim
        for thread_id, claim in latest_by_thread.items()
        if claim["status"] == "ACTIVE" and thread_id not in permanently_superseded
    ]


def github_get_json(url: str, token: str) -> tuple[Any, str | None]:
    request = urllib.request.Request(
        url,
        headers={
            "Accept": "application/vnd.github+json",
            "Authorization": f"Bearer {token}",
            "X-GitHub-Api-Version": "2022-11-28",
            "User-Agent": "norse-stylized-3d-poc-repository-policy",
        },
    )
    try:
        with urllib.request.urlopen(request, timeout=30) as response:
            data = json.loads(response.read().decode("utf-8"))
            return data, response.headers.get("Link")
    except urllib.error.HTTPError as exc:
        detail = exc.read().decode("utf-8", errors="replace")
        raise PolicyFailure(f"GitHub API GET failed ({exc.code}) for {url}: {detail}") from exc


def next_link(link_header: str | None) -> str | None:
    if not link_header:
        return None
    for part in link_header.split(","):
        match = re.match(r'\s*<([^>]+)>;\s*rel="([^"]+)"', part)
        if match and match.group(2) == "next":
            return match.group(1)
    return None


def github_get_all_pages(url: str, token: str) -> list[Any]:
    items: list[Any] = []
    current: str | None = url
    while current:
        page, link = github_get_json(current, token)
        require(isinstance(page, list), f"Expected list response from {current}.")
        items.extend(page)
        current = next_link(link)
    return items


def validate_repository_invariants(repo_root: Path) -> None:
    root_agents = repo_root / "AGENTS.md"
    assets_agents = repo_root / "Assets" / "AGENTS.md"
    require(root_agents.is_file(), "Root AGENTS.md is missing.")
    require(assets_agents.is_file(), "Assets/AGENTS.md is missing.")
    require(
        root_agents.read_bytes() == assets_agents.read_bytes(),
        "AGENTS.md and Assets/AGENTS.md must remain byte-identical mirrors.",
    )

    project_version = repo_root / "ProjectSettings" / "ProjectVersion.txt"
    require(project_version.is_file(), "ProjectSettings/ProjectVersion.txt is missing.")
    version_text = project_version.read_text(encoding="utf-8", errors="strict")
    version_match = re.search(r"(?m)^m_EditorVersion:\s*(\S+)\s*$", version_text)
    require(version_match is not None, "Could not read m_EditorVersion from ProjectVersion.txt.")
    require(
        version_match.group(1) == EXPECTED_UNITY_VERSION,
        f"Unity version drift: expected {EXPECTED_UNITY_VERSION}, found {version_match.group(1)}.",
    )

    result = subprocess.run(
        ["git", "ls-files"],
        cwd=repo_root,
        check=True,
        text=True,
        stdout=subprocess.PIPE,
    )
    tracked = [line.strip() for line in result.stdout.splitlines() if line.strip()]
    forbidden = [
        path
        for path in tracked
        if any(path.startswith(prefix) for prefix in FORBIDDEN_TRACKED_PREFIXES)
    ]
    require(
        not forbidden,
        "Generated/local-only paths are tracked: " + ", ".join(forbidden[:20]),
    )


def validate_branch_freshness(repo_root: Path) -> None:
    result = subprocess.run(
        ["git", "merge-base", "--is-ancestor", "origin/fufu", "HEAD"],
        cwd=repo_root,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
    )
    require(
        result.returncode == 0,
        "PR branch does not contain the latest fetched origin/fufu. Reconcile with fufu and rerun CI.",
    )


def validate_pull_request(
    event: dict[str, Any],
    repo: str,
    token: str,
) -> None:
    pr = event.get("pull_request")
    require(isinstance(pr, dict), "pull_request event payload is missing pull_request data.")

    base_ref = pr.get("base", {}).get("ref")
    head_ref = pr.get("head", {}).get("ref")
    head_repo = pr.get("head", {}).get("repo", {}).get("full_name")
    body = pr.get("body") or ""

    require(base_ref == "fufu", f"Agent implementation PRs must target fufu, not {base_ref!r}.")
    require(head_repo == repo, "Agent implementation PR branches must live in the canonical repository, not a fork.")
    require(isinstance(head_ref, str), "PR head branch is missing.")

    branch_match = BRANCH_RE.fullmatch(head_ref)
    require(
        branch_match is not None,
        "Branch must match issue-<number>/t-YYYYMMDD-HHMM-<6chars>-<slug>.",
    )
    branch_issue = int(branch_match.group("issue"))
    branch_thread = branch_match.group("thread")

    issue_value = field_from_markdown(body, "Issue")
    thread_value = field_from_markdown(body, "Thread-ID")
    branch_value = field_from_markdown(body, "Branch")
    require(issue_value is not None and re.fullmatch(r"#[1-9]\d*", issue_value) is not None,
            "PR body must contain `Issue: #<number>`.")
    require(thread_value is not None and THREAD_RE.fullmatch(thread_value) is not None,
            "PR body must contain a valid `Thread-ID:` field.")
    require(branch_value is not None, "PR body must contain a `Branch:` field.")

    metadata_issue = int(issue_value[1:])
    require(metadata_issue == branch_issue,
            f"PR Issue metadata #{metadata_issue} does not match branch issue #{branch_issue}.")
    require(thread_value == branch_thread,
            "PR Thread-ID metadata does not match the branch Thread-ID.")
    require(branch_value == head_ref,
            "PR Branch metadata does not exactly match the PR head branch.")

    issue_url = f"https://api.github.com/repos/{repo}/issues/{branch_issue}"
    issue, _ = github_get_json(issue_url, token)
    require(isinstance(issue, dict), "Task issue API response was not an object.")
    require("pull_request" not in issue, f"#{branch_issue} is a pull request, not a task issue.")
    require(issue.get("state") == "open", f"Task issue #{branch_issue} must remain open during implementation.")

    comments_url = f"{issue_url}/comments?per_page=100"
    comments = github_get_all_pages(comments_url, token)
    claims: list[dict[str, Any]] = []
    for comment in comments:
        if not isinstance(comment, dict):
            continue
        parsed = parse_claim(comment.get("body") or "", int(comment.get("id", 0)))
        if parsed is not None:
            claims.append(parsed)

    require(claims, f"Task issue #{branch_issue} has no structured agent-thread claim comments.")
    active = resolve_active_claims(claims)
    require(
        len(active) == 1,
        f"Task issue #{branch_issue} must resolve to exactly one ACTIVE thread claim; found {len(active)}.",
    )
    claim = active[0]
    require(claim["thread_id"] == branch_thread,
            f"ACTIVE claim belongs to {claim['thread_id']}, not this PR thread {branch_thread}.")
    require(claim["branch"] == head_ref,
            f"ACTIVE claim branch {claim['branch']!r} does not match PR branch {head_ref!r}.")


def run_self_test() -> None:
    sample_branch = "issue-37/t-20260905-2230-a7c4f2-agent-workflow"
    require(BRANCH_RE.fullmatch(sample_branch) is not None, "Self-test branch regex failed.")
    sample_body = "Issue: #37\nThread-ID: t-20260905-2230-a7c4f2\nBranch: " + sample_branch
    require(field_from_markdown(sample_body, "Issue") == "#37", "Self-test PR metadata failed.")

    first = parse_claim(
        CLAIM_MARKER + "\nThread-ID: t-20260905-2230-a7c4f2\nBranch: " + sample_branch +
        "\nBase: fufu@1234567\nStatus: ACTIVE\nScope: Bootstrap policy.",
        1,
    )
    second_branch = "issue-37/t-20260906-0915-b3d91e-agent-workflow"
    second = parse_claim(
        CLAIM_MARKER + "\nThread-ID: t-20260906-0915-b3d91e\nBranch: " + second_branch +
        "\nBase: fufu@89abcde\nStatus: ACTIVE\nScope: Authorized takeover.\nSupersedes: t-20260905-2230-a7c4f2",
        2,
    )
    require(first is not None and second is not None, "Self-test claim parsing failed.")
    active = resolve_active_claims([first, second])
    require(len(active) == 1 and active[0]["thread_id"] == "t-20260906-0915-b3d91e",
            "Self-test claim supersession failed.")
    print("Agent workflow policy self-test: PASS")


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--event", default=os.environ.get("GITHUB_EVENT_PATH"))
    parser.add_argument("--repo", default=os.environ.get("GITHUB_REPOSITORY"))
    parser.add_argument("--repo-root", default=".")
    parser.add_argument("--self-test", action="store_true")
    args = parser.parse_args()

    try:
        if args.self_test:
            run_self_test()
            return 0

        require(args.event is not None, "No GitHub event payload path was provided.")
        require(args.repo is not None and "/" in args.repo, "No valid repository name was provided.")
        repo_root = Path(args.repo_root).resolve()
        validate_repository_invariants(repo_root)

        event = read_event(args.event)
        event_name = os.environ.get("GITHUB_EVENT_NAME", "")
        if event_name == "pull_request":
            token = os.environ.get("GITHUB_TOKEN", "")
            require(bool(token), "GITHUB_TOKEN is required for pull_request policy validation.")
            validate_pull_request(event, args.repo, token)
            validate_branch_freshness(repo_root)
            print("Agent PR identity and branch freshness: PASS")
        else:
            print(f"Repository invariants: PASS ({event_name or 'manual event'})")
        return 0
    except PolicyFailure as exc:
        print(f"POLICY FAILURE: {exc}", file=sys.stderr)
        return 1
    except subprocess.CalledProcessError as exc:
        print(f"POLICY FAILURE: command failed: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
