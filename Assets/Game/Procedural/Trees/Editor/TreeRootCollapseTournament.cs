using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeRootCollapseTournament
    {
        private const string OutputDirectory = "Library/PS3D/Trees/WychElmRootFrameTournament";
        private const string ReportFileName = "TreeWychElmRootFrameTournamentReport.txt";
        private const string CsvFileName = "TreeWychElmRootFrameTournament.csv";
        private const string PreviewObjectName = "TREE-CONTROLS.4H11H4 Unsafe Wych Preview";

        private static readonly float[] RootHeights = { 0.030f, 0.050f, 0.100f };
        private static readonly TreeRootCollapseTournamentStrategy[] Strategies =
        {
            TreeRootCollapseTournamentStrategy.Production,
            TreeRootCollapseTournamentStrategy.ImmediateFrameRelease,
            TreeRootCollapseTournamentStrategy.BoundedFrameRelease,
            TreeRootCollapseTournamentStrategy.DenseFrameAdoptionResampling,
            TreeRootCollapseTournamentStrategy.TransportedContourBlend
        };

        private sealed class Profile
        {
            internal string Name;
            internal bool OverrideReach;
            internal float Reach;
            internal bool OverrideThickness;
            internal float Thickness;
        }

        private static readonly Profile[] Profiles =
        {
            new Profile { Name = "RecipeBaseline" },
            new Profile { Name = "ReachHigh", OverrideReach = true, Reach = 2f },
            new Profile { Name = "ThicknessHigh", OverrideThickness = true, Thickness = 1f },
            new Profile
            {
                Name = "ReachAndThicknessHigh",
                OverrideReach = true,
                Reach = 2f,
                OverrideThickness = true,
                Thickness = 1f
            }
        };

        private sealed class Representative
        {
            internal string Name;
            internal TreeFamily Family;
            internal int Seed;
            internal string SourceIdentity;
            internal TreeResolvedControls Controls;
            internal ProceduralTreeInstance Instance;
        }

        private sealed class CaseResult
        {
            internal string Profile;
            internal TreeRootCollapseTournamentStrategy Strategy;
            internal float RootHeight;
            internal float RootReach;
            internal float RootThickness;
            internal bool Passed;
            internal string Failure;
            internal string FailureStage;
            internal int Vertices;
            internal int Triangles;
            internal int RootIntervals;
            internal int AuditDegenerate;
            internal int AuditOrientation;
            internal int AuditTangents;
            internal int AuditBoundaries;
            internal int AuditNonManifold;
            internal bool CandidateActivated;
            internal int MorphRequested;
            internal int MorphEmitted;
            internal string GeometryFingerprint;
        }

        private sealed class Job
        {
            internal Representative Representative;
            internal readonly List<CaseResult> Results = new List<CaseResult>(60);
            internal int StrategyIndex;
            internal int ProfileIndex;
            internal int HeightIndex;
            internal int Completed;
            internal bool CancelRequested;
            internal DateTime StartedUtc;
            internal string ReportPath;
            internal string CsvPath;
            internal StreamWriter CsvWriter;
        }

        private static Job activeJob;
        private static string lastReportPath = string.Empty;
        private static string currentDetail = "Not running";
        private static string currentEta = string.Empty;
        private static float currentProgress;

        internal static bool IsRunning => activeJob != null;
        internal static string LastReportPath => lastReportPath;
        internal static string CurrentDetail => currentDetail;
        internal static string CurrentEta => currentEta;
        internal static float CurrentProgress => currentProgress;
        internal static string ProgressLabel => activeJob == null
            ? "Not running"
            : activeJob.Completed + " / " + TotalCases;
        private static int TotalCases => Strategies.Length * Profiles.Length * RootHeights.Length;

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null || TreeControlResponseSuite.IsRunning) return false;
            Representative representative = CollectRepresentative(selected);
            if (representative == null)
            {
                Debug.LogError("[TREE-CONTROLS.4H11H4] Curated Wych Elm representative was not found.");
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string reportPath = Path.Combine(OutputDirectory, ReportFileName);
            string csvPath = Path.Combine(OutputDirectory, CsvFileName);
            var writer = new StreamWriter(csvPath, false, Encoding.UTF8);
            writer.WriteLine(
                "Strategy,Profile,RootHeight,RootReach,RootThickness,Status," +
                "Vertices,Triangles,RootIntervals,FailureStage,AuditDegenerate," +
                "AuditOrientation,AuditTangents,AuditBoundaries,AuditNonManifold," +
                "CandidateActivated,MorphRequested,MorphEmitted,GeometryFingerprint,Failure");
            writer.Flush();

            activeJob = new Job
            {
                Representative = representative,
                StartedUtc = DateTime.UtcNow,
                ReportPath = reportPath,
                CsvPath = csvPath,
                CsvWriter = writer
            };
            lastReportPath = reportPath;
            currentDetail = "Preparing first case";
            currentEta = "ETA calculating";
            currentProgress = 0f;
            WriteReport(activeJob, "RUNNING", null);
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            Debug.Log("[TREE-CONTROLS.4H11H4] Wych Elm root-frame tournament started. Cases=" + TotalCases + ".");
            return true;
        }

        internal static void RequestCancel()
        {
            if (activeJob != null) activeJob.CancelRequested = true;
        }

        internal static void CopyLastReport()
        {
            if (!string.IsNullOrEmpty(lastReportPath) && File.Exists(lastReportPath))
                EditorGUIUtility.systemCopyBuffer = File.ReadAllText(lastReportPath);
        }

        internal static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputDirectory));
        }

        internal static bool BuildUnsafeVisualPreview(ProceduralTreeInstance selected)
        {
            Representative representative = CollectRepresentative(selected);
            if (representative == null) return false;

            GameObject existing = GameObject.Find(PreviewObjectName);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing);

            TreeResolvedControls controls = BuildControls(
                representative, Profiles[0], 0.050f);
            TreeGenerationResult generation = TreeGenerator.GenerateExactForValidation(
                controls, representative.Seed, representative.SourceIdentity, representative.Family);
            if (generation == null || !generation.Passed || generation.Definition == null)
            {
                Debug.LogError("[TREE-CONTROLS.4H11H4] Unsafe preview tree generation failed.");
                return false;
            }

            var mesh = new Mesh { name = PreviewObjectName + " Mesh", hideFlags = HideFlags.DontSaveInEditor };
            TreeBarkMeshBuildResult bark = TreeBarkMeshGenerator.BuildUnsafeVisualPreview(
                generation.Definition, TreeBarkMeshSettings.CreateRecipeOnlyDefaults(), mesh);
            if (mesh.vertexCount == 0)
            {
                UnityEngine.Object.DestroyImmediate(mesh);
                Debug.LogError("[TREE-CONTROLS.4H11H4] Unsafe preview produced no mesh. " + (bark != null ? bark.Failure : string.Empty));
                return false;
            }

            var preview = new GameObject(PreviewObjectName)
            {
                hideFlags = HideFlags.DontSaveInEditor
            };
            var filter = preview.AddComponent<MeshFilter>();
            var renderer = preview.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            MeshRenderer sourceRenderer = representative.Instance != null
                ? representative.Instance.GetComponentInChildren<MeshRenderer>(true)
                : null;
            if (sourceRenderer != null) renderer.sharedMaterials = sourceRenderer.sharedMaterials;

            Vector3 offset = Vector3.right * Mathf.Max(4f, mesh.bounds.size.x * 1.5f);
            preview.transform.SetPositionAndRotation(
                representative.Instance.transform.position + offset,
                representative.Instance.transform.rotation);
            preview.transform.localScale = representative.Instance.transform.lossyScale;
            Selection.activeGameObject = preview;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log(
                "[TREE-CONTROLS.4H11H4] Built temporary unsafe Wych preview at Root Height 0.050. " +
                "This object is not saved. Bark result=" + (bark != null && bark.Passed ? "PASS" : "REJECTED") +
                ". Failure=" + (bark != null ? FirstFailureLine(bark.Failure) : "null"));
            return true;
        }

        private static void Tick()
        {
            Job job = activeJob;
            if (job == null) return;
            try
            {
                if (job.CancelRequested) { Finish(job, "CANCELLED", null); return; }
                TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
                double secondsPerCase = job.Completed > 0 ? elapsed.TotalSeconds / job.Completed : 0.0;
                double eta = secondsPerCase * (TotalCases - job.Completed);
                TreeRootCollapseTournamentStrategy strategy = Strategies[job.StrategyIndex];
                Profile profile = Profiles[job.ProfileIndex];
                float height = RootHeights[job.HeightIndex];
                currentProgress = job.Completed / (float)TotalCases;
                currentDetail = strategy + " — " + profile.Name + " — Root Height " + height.ToString("0.000", CultureInfo.InvariantCulture);
                currentEta = "Elapsed " + FormatDuration(elapsed.TotalSeconds) + " | ETA " + FormatDuration(eta);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                CaseResult result = RunCase(job.Representative, strategy, profile, height);
                job.Results.Add(result);
                WriteCsv(job.CsvWriter, result);
                job.CsvWriter.Flush();
                job.Completed++;
                WriteReport(job, "RUNNING", null);
                if (!Advance(job)) Finish(job, "COMPLETE", null);
            }
            catch (Exception exception)
            {
                Finish(job, "FAILED", exception.ToString());
            }
        }

        private static CaseResult RunCase(
            Representative representative,
            TreeRootCollapseTournamentStrategy strategy,
            Profile profile,
            float rootHeight)
        {
            TreeResolvedControls controls = BuildControls(representative, profile, rootHeight);
            var result = new CaseResult
            {
                Strategy = strategy,
                Profile = profile.Name,
                RootHeight = rootHeight,
                RootReach = controls.RootReach,
                RootThickness = controls.RootThickness,
                FailureStage = "TREE_GENERATION"
            };
            TreeGenerationResult generation = TreeGenerator.GenerateExactForValidation(
                controls, representative.Seed, representative.SourceIdentity, representative.Family);
            if (generation == null || !generation.Passed || generation.Definition == null || !generation.Definition.IsValid)
            {
                result.Failure = generation != null ? FirstFailureLine(generation.Report) : "Tree generation returned null.";
                return result;
            }

            var mesh = new Mesh { name = "TREE-CONTROLS.4H11H4 Case" };
            TreeBarkMeshBuildResult bark;
            try
            {
                bark = TreeBarkMeshGenerator.BuildForRootCollapseTournament(
                    generation.Definition, TreeBarkMeshSettings.CreateRecipeOnlyDefaults(), mesh, strategy);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(mesh);
            }
            if (bark == null)
            {
                result.FailureStage = "BARK_BUILD";
                result.Failure = "Bark generation returned null.";
                return result;
            }
            result.FailureStage = bark.FailureStage;
            result.Vertices = bark.VertexCount;
            result.Triangles = bark.TriangleCount;
            result.RootIntervals = bark.RootZoneLongitudinalIntervals;
            result.CandidateActivated = bark.RootTrunkBoundaryCandidateActivated;
            result.MorphRequested = bark.RootTrunkBoundaryMorphRingsRequested;
            result.MorphEmitted = bark.RootTrunkBoundaryMorphRingsUsed;
            result.GeometryFingerprint = bark.GeometryFingerprint;
            TreeBarkMeshTopologyAuditResult audit = bark.TopologyAudit;
            if (audit != null)
            {
                result.AuditDegenerate = audit.DegenerateTriangleCount;
                result.AuditOrientation = audit.SideOrientationFailureCount;
                result.AuditTangents = audit.TangentBasisFailureCount;
                result.AuditBoundaries = audit.UnexpectedExposedBoundaryLoopCount;
                result.AuditNonManifold = audit.NonManifoldEdgeCount;
            }
            result.Passed = bark.Passed;
            result.Failure = bark.Failure;
            return result;
        }

        private static TreeResolvedControls BuildControls(Representative representative, Profile profile, float rootHeight)
        {
            TreeResolvedControls controls = CloneControls(representative.Controls);
            SetFloat(controls, "missingBranchChance", 0f);
            SetFloat(controls, "brokenBranchChance", 0f);
            SetFloat(controls, "rootHeight", rootHeight);
            if (profile.OverrideReach) SetFloat(controls, "rootReach", profile.Reach);
            if (profile.OverrideThickness) SetFloat(controls, "rootThickness", profile.Thickness);
            controls.ValidateAndClamp();
            return controls;
        }

        private static bool Advance(Job job)
        {
            job.HeightIndex++;
            if (job.HeightIndex < RootHeights.Length) return true;
            job.HeightIndex = 0;
            job.ProfileIndex++;
            if (job.ProfileIndex < Profiles.Length) return true;
            job.ProfileIndex = 0;
            job.StrategyIndex++;
            return job.StrategyIndex < Strategies.Length;
        }

        private static void Finish(Job job, string outcome, string failure)
        {
            if (job == null) return;
            EditorApplication.update -= Tick;
            AssemblyReloadEvents.beforeAssemblyReload -= AbortForReload;
            EditorApplication.quitting -= AbortForQuit;
            job.CsvWriter?.Flush();
            job.CsvWriter?.Dispose();
            WriteReport(job, outcome, failure);
            activeJob = null;
            currentProgress = outcome == "COMPLETE" ? 1f : currentProgress;
            currentDetail = outcome;
            currentEta = string.Empty;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            Debug.Log("[TREE-CONTROLS.4H11H4] Wych Elm root-frame tournament " + outcome + ". Report=" + job.ReportPath);
        }

        private static void AbortForReload()
        {
            if (activeJob != null) Finish(activeJob, "CANCELLED", "Assembly reload interrupted the tournament; partial reports were preserved.");
        }

        private static void AbortForQuit()
        {
            if (activeJob != null) Finish(activeJob, "CANCELLED", "Unity quit interrupted the tournament; partial reports were preserved.");
        }

        private static void WriteReport(Job job, string outcome, string failure)
        {
            var report = new StringBuilder(32768);
            report.AppendLine("[TREE-CONTROLS.4H11H4 Wych Elm Root-Frame Strategy Tournament]");
            report.Append("Generated UTC: ").AppendLine(DateTime.UtcNow.ToString("O"));
            report.Append("Outcome: ").AppendLine(outcome);
            report.Append("Generator / bark: ").Append(TreeGenerator.CurrentGeneratorVersion).Append(" / ").AppendLine(TreeBarkMeshGenerator.BarkAlgorithmVersion.ToString());
            report.Append("Completed / total: ").Append(job.Completed).Append(" / ").AppendLine(TotalCases.ToString());
            report.AppendLine("Root Heights: 0.030, 0.050, 0.100");
            report.AppendLine("Profiles: baseline; Reach High; Thickness High; both extremes.");
            if (!string.IsNullOrEmpty(failure)) report.Append("Runner failure: ").AppendLine(failure);

            report.AppendLine();
            report.AppendLine("[Strategy Results]");
            for (int s = 0; s < Strategies.Length; s++)
            {
                int completed = 0, passed = 0;
                for (int i = 0; i < job.Results.Count; i++)
                {
                    if (job.Results[i].Strategy != Strategies[s]) continue;
                    completed++;
                    if (job.Results[i].Passed) passed++;
                }
                report.Append(Strategies[s]).Append(" | passed=").Append(passed).Append("/").Append(completed);
                if (completed == Profiles.Length * RootHeights.Length && passed == completed) report.Append(" | COMPLETE WINNER");
                report.AppendLine();
            }

            report.AppendLine();
            report.AppendLine("[Completed Cases]");
            for (int i = 0; i < job.Results.Count; i++)
            {
                CaseResult item = job.Results[i];
                report.Append(item.Passed ? "PASS" : "FAIL")
                    .Append(" | ").Append(item.Strategy)
                    .Append(" | ").Append(item.Profile)
                    .Append(" | rootHeight=").Append(item.RootHeight.ToString("F3", CultureInfo.InvariantCulture))
                    .Append(" | rootReach=").Append(item.RootReach.ToString("F3", CultureInfo.InvariantCulture))
                    .Append(" | rootThickness=").Append(item.RootThickness.ToString("F3", CultureInfo.InvariantCulture))
                    .Append(" | intervals=").Append(item.RootIntervals)
                    .Append(" | candidate=").Append(item.CandidateActivated ? "Yes" : "No")
                    .Append(" | morphRequested=").Append(item.MorphRequested)
                    .Append(" | morphEmitted=").Append(item.MorphEmitted)
                    .Append(" | failureStage=").Append(item.FailureStage)
                    .Append(" | auditDegenerate=").Append(item.AuditDegenerate)
                    .Append(" | auditOrientation=").Append(item.AuditOrientation)
                    .Append(" | auditTangents=").Append(item.AuditTangents)
                    .Append(" | auditBoundaries=").Append(item.AuditBoundaries)
                    .Append(" | auditNonManifold=").Append(item.AuditNonManifold)
                    .Append(" | vertices=").Append(item.Vertices);
                if (!item.Passed) report.Append(" | ").Append(FirstFailureLine(item.Failure));
                report.AppendLine();
            }
            report.Append("CSV: ").AppendLine(job.CsvPath);
            File.WriteAllText(job.ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static void WriteCsv(StreamWriter writer, CaseResult result)
        {
            writer.Write(result.Strategy); writer.Write(',');
            writer.Write(Escape(result.Profile)); writer.Write(',');
            writer.Write(result.RootHeight.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.RootReach.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.RootThickness.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.Passed ? "PASS" : "FAIL"); writer.Write(',');
            writer.Write(result.Vertices); writer.Write(',');
            writer.Write(result.Triangles); writer.Write(',');
            writer.Write(result.RootIntervals); writer.Write(',');
            writer.Write(Escape(result.FailureStage)); writer.Write(',');
            writer.Write(result.AuditDegenerate); writer.Write(',');
            writer.Write(result.AuditOrientation); writer.Write(',');
            writer.Write(result.AuditTangents); writer.Write(',');
            writer.Write(result.AuditBoundaries); writer.Write(',');
            writer.Write(result.AuditNonManifold); writer.Write(',');
            writer.Write(result.CandidateActivated ? "Yes" : "No"); writer.Write(',');
            writer.Write(result.MorphRequested); writer.Write(',');
            writer.Write(result.MorphEmitted); writer.Write(',');
            writer.Write(Escape(result.GeometryFingerprint)); writer.Write(',');
            writer.WriteLine(Escape(result.Failure));
        }

        private static Representative CollectRepresentative(ProceduralTreeInstance selected)
        {
            const string recipe = "tree-recipe-curated-wych-elm-leaning";
            string slot = TreeGenerationLibraryVariant.BuildStableKey(TreeFamily.Twisted, 1);
            ProceduralTreeInstance[] instances = UnityEngine.Object.FindObjectsByType<ProceduralTreeInstance>(FindObjectsInactive.Include);
            ProceduralTreeInstance match = null;
            for (int i = 0; i < instances.Length; i++)
            {
                ProceduralTreeInstance candidate = instances[i];
                if (candidate == null || !candidate.HasExactControls) continue;
                string identity = candidate.Recipe != null ? candidate.Recipe.StableIdentity : candidate.ExactControlsSourceRecipeIdentity;
                if (identity == recipe && candidate.StableSlotIdentity == slot) { match = candidate; break; }
            }
            if (match == null && selected != null && selected.HasExactControls) match = selected;
            if (match == null) return null;
            TreeResolvedControls controls;
            string sourceIdentity;
            if (match.Recipe != null && match.Recipe.ControlRanges != null)
            {
                controls = new TreeResolvedControls();
                controls.ResolveFrom(match.Recipe.ControlRanges, match.MasterSeed);
                sourceIdentity = match.Recipe.StableIdentity;
            }
            else
            {
                controls = CloneControls(match.ExactControls);
                sourceIdentity = match.ExactControlsSourceRecipeIdentity;
            }
            return new Representative
            {
                Name = "Wych Elm",
                Family = match.Family,
                Seed = match.MasterSeed,
                SourceIdentity = sourceIdentity,
                Controls = controls,
                Instance = match
            };
        }

        private static TreeResolvedControls CloneControls(TreeResolvedControls source)
        {
            return source == null ? null : JsonUtility.FromJson<TreeResolvedControls>(JsonUtility.ToJson(source));
        }

        private static void SetFloat(TreeResolvedControls controls, string fieldName, float value)
        {
            FieldInfo field = typeof(TreeResolvedControls).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new MissingFieldException(typeof(TreeResolvedControls).FullName, fieldName);
            field.SetValue(controls, value);
        }

        private static string FirstFailureLine(string report)
        {
            if (string.IsNullOrWhiteSpace(report)) return string.Empty;
            int newline = report.IndexOf('\n');
            return (newline >= 0 ? report.Substring(0, newline) : report).Trim();
        }

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) || seconds < 0.0) return "unknown";
            TimeSpan span = TimeSpan.FromSeconds(seconds);
            return span.TotalHours >= 1.0 ? span.ToString(@"h\:mm\:ss") : span.ToString(@"m\:ss");
        }
    }
}
