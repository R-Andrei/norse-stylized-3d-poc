using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal enum GroundPaintedAccentGeneratedAssetAuditStatus
    {
        ActiveAndReferenced = 0,
        ReferencedButNotRequired = 1,
        OwnershipMismatch = 2,
        SharedIncorrectly = 3,
        ConfirmedOrphan = 4,
        UnknownUnsafe = 5
    }

    internal readonly struct GroundPaintedAccentGeneratedAssetAuditEntry
    {
        public GroundPaintedAccentGeneratedAssetAuditEntry(
            string assetPath,
            GroundPaintedAccentGeneratedAssetAuditStatus status,
            string detail)
        {
            AssetPath = assetPath ?? string.Empty;
            Status = status;
            Detail = detail ?? string.Empty;
        }

        public string AssetPath { get; }
        public GroundPaintedAccentGeneratedAssetAuditStatus Status { get; }
        public string Detail { get; }
        public bool IsConfirmedOrphan =>
            Status ==
            GroundPaintedAccentGeneratedAssetAuditStatus.ConfirmedOrphan;
    }

    internal sealed class GroundPaintedAccentGeneratedAssetAuditReport
    {
        private readonly List<GroundPaintedAccentGeneratedAssetAuditEntry>
            entries =
                new List<GroundPaintedAccentGeneratedAssetAuditEntry>(32);
        private readonly List<string> auditFailures =
            new List<string>(4);
        private readonly List<string> deletionBlockers =
            new List<string>(4);

        public IReadOnlyList<GroundPaintedAccentGeneratedAssetAuditEntry>
            Entries => entries;
        public IReadOnlyList<string> AuditFailures => auditFailures;
        public IReadOnlyList<string> DeletionBlockers => deletionBlockers;
        public bool Completed { get; internal set; }
        public bool Cancelled { get; internal set; }
        public bool CanDeleteConfirmedOrphans =>
            Completed &&
            !Cancelled &&
            auditFailures.Count == 0 &&
            deletionBlockers.Count == 0 &&
            ConfirmedOrphanCount > 0;

        public int ActiveCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus.ActiveAndReferenced);
        public int ReferencedNotRequiredCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus
                .ReferencedButNotRequired);
        public int OwnershipMismatchCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus.OwnershipMismatch);
        public int SharedIncorrectlyCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus.SharedIncorrectly);
        public int ConfirmedOrphanCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus.ConfirmedOrphan);
        public int UnknownUnsafeCount => CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus.UnknownUnsafe);

        internal void Add(
            GroundPaintedAccentGeneratedAssetAuditEntry entry)
        {
            entries.Add(entry);
        }

        internal void AddFailure(string failure)
        {
            if (!string.IsNullOrWhiteSpace(failure))
            {
                auditFailures.Add(failure);
            }
        }

        internal void AddDeletionBlocker(string blocker)
        {
            if (!string.IsNullOrWhiteSpace(blocker))
            {
                deletionBlockers.Add(blocker);
            }
        }

        public List<string> GetConfirmedOrphanPaths()
        {
            List<string> paths = new List<string>(ConfirmedOrphanCount);
            for (int index = 0; index < entries.Count; index++)
            {
                if (entries[index].IsConfirmedOrphan)
                {
                    paths.Add(entries[index].AssetPath);
                }
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        public string BuildReport()
        {
            StringBuilder builder = new StringBuilder(4096);
            builder.Append("Generated Painted Accent asset audit\n")
                .Append("Root: ")
                .Append(
                    GroundPaintedAccentProductionBaker.GeneratedRootPath)
                .Append("\nStatus: ")
                .Append(
                    Cancelled
                        ? "Cancelled"
                        : Completed
                            ? "Complete"
                            : "Incomplete")
                .Append("\nAssets scanned: ")
                .Append(entries.Count)
                .Append("\nActive and referenced: ")
                .Append(ActiveCount)
                .Append("\nReferenced but no longer required: ")
                .Append(ReferencedNotRequiredCount)
                .Append("\nOwnership mismatch: ")
                .Append(OwnershipMismatchCount)
                .Append("\nShared incorrectly: ")
                .Append(SharedIncorrectlyCount)
                .Append("\nConfirmed orphans: ")
                .Append(ConfirmedOrphanCount)
                .Append("\nUnknown / unsafe: ")
                .Append(UnknownUnsafeCount)
                .Append("\nDeletion readiness: ")
                .Append(
                    CanDeleteConfirmedOrphans
                        ? "Ready"
                        : ConfirmedOrphanCount == 0
                            ? "No confirmed orphans"
                            : "Blocked");

            if (auditFailures.Count > 0)
            {
                builder.Append("\n\nAudit failures");
                for (int index = 0; index < auditFailures.Count; index++)
                {
                    builder.Append("\n- ")
                        .Append(auditFailures[index]);
                }
            }

            if (deletionBlockers.Count > 0)
            {
                builder.Append("\n\nDeletion blockers");
                for (int index = 0;
                     index < deletionBlockers.Count;
                     index++)
                {
                    builder.Append("\n- ")
                        .Append(deletionBlockers[index]);
                }
            }

            AppendEntries(
                builder,
                "Active and referenced",
                GroundPaintedAccentGeneratedAssetAuditStatus
                    .ActiveAndReferenced);
            AppendEntries(
                builder,
                "Referenced but no longer required",
                GroundPaintedAccentGeneratedAssetAuditStatus
                    .ReferencedButNotRequired);
            AppendEntries(
                builder,
                "Ownership mismatch",
                GroundPaintedAccentGeneratedAssetAuditStatus
                    .OwnershipMismatch);
            AppendEntries(
                builder,
                "Shared incorrectly",
                GroundPaintedAccentGeneratedAssetAuditStatus
                    .SharedIncorrectly);
            AppendEntries(
                builder,
                "Confirmed orphans",
                GroundPaintedAccentGeneratedAssetAuditStatus
                    .ConfirmedOrphan);
            AppendEntries(
                builder,
                "Unknown / unsafe",
                GroundPaintedAccentGeneratedAssetAuditStatus.UnknownUnsafe);
            return builder.ToString();
        }

        private int CountStatus(
            GroundPaintedAccentGeneratedAssetAuditStatus status)
        {
            int count = 0;
            for (int index = 0; index < entries.Count; index++)
            {
                if (entries[index].Status == status)
                {
                    count++;
                }
            }

            return count;
        }

        private void AppendEntries(
            StringBuilder builder,
            string heading,
            GroundPaintedAccentGeneratedAssetAuditStatus status)
        {
            bool headingWritten = false;
            for (int index = 0; index < entries.Count; index++)
            {
                GroundPaintedAccentGeneratedAssetAuditEntry entry =
                    entries[index];
                if (entry.Status != status)
                {
                    continue;
                }

                if (!headingWritten)
                {
                    builder.Append("\n\n")
                        .Append(heading);
                    headingWritten = true;
                }

                builder.Append("\n- ")
                    .Append(entry.AssetPath);
                if (!string.IsNullOrWhiteSpace(entry.Detail))
                {
                    builder.Append("\n  ")
                        .Append(entry.Detail.Replace("\n", "\n  "));
                }
            }
        }
    }

}
