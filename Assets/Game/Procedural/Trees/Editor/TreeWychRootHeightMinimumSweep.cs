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
    internal static class TreeWychRootHeightMinimumSweep
    {
        private const string OutputDirectory =
            "Library/PS3D/Trees/WychRootHeightMinimumSweep";
        private const string ReportFileName =
            "TreeWychRootHeightMinimumSweepReport.txt";
        private const string CsvFileName =
            "TreeWychRootHeightMinimumSweep.csv";

        private static readonly float[] ButtressPersistences =
        {
            0.000f,
            0.100f,
            0.200f,
            0.300f,
            0.400f,
            0.500f,
            0.600f,
            0.700f
        };

        private static readonly float[] RootHeights =
        {
            0.050f,
            0.075f,
            0.100f,
            0.125f,
            0.150f,
            0.175f,
            0.200f,
            0.225f
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
            new Profile
            {
                Name = "ReachHigh",
                OverrideReach = true,
                Reach = 2f
            },
            new Profile
            {
                Name = "ThicknessHigh",
                OverrideThickness = true,
                Thickness = 1f
            },
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
        }

        private sealed class CaseResult
        {
            internal string Profile;
            internal float ButtressPersistence;
            internal float RootHeight;
            internal float RootReach;
            internal float RootThickness;
            internal bool Passed;
            internal string FailureStage;
            internal string Failure;
            internal int Vertices;
            internal int Triangles;
            internal int RootIntervals;
            internal int OrientationFailures;
            internal int DegenerateTriangles;
            internal int TangentFailures;
            internal int UnexpectedBoundaryLoops;
            internal int NonManifoldEdges;
            internal string GeometryFingerprint;
        }

        private sealed class Job
        {
            internal Representative Representative;
            internal readonly List<CaseResult> Results =
                new List<CaseResult>(256);
            internal int PersistenceIndex;
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

        private static int TotalCases =>
            ButtressPersistences.Length * Profiles.Length * RootHeights.Length;

        internal static bool Start(ProceduralTreeInstance selected)
        {
            if (activeJob != null ||
                TreeControlResponseSuite.IsRunning ||
                TreeRootCollapseTournament.IsRunning)
            {
                return false;
            }

            Representative representative = BuildRepresentative(selected);
            if (representative == null)
            {
                Debug.LogError(
                    "[TREE-CONTROLS.4H13] Select a generated Wych Elm ProceduralTreeInstance with exact controls before running the Wych Root Height Minimum Sweep.");
                return false;
            }

            Directory.CreateDirectory(OutputDirectory);
            string reportPath = Path.Combine(OutputDirectory, ReportFileName);
            string csvPath = Path.Combine(OutputDirectory, CsvFileName);
            var writer = new StreamWriter(csvPath, false, Encoding.UTF8);
            writer.WriteLine(
                "Profile,RootHeight,RootReach,RootThickness,ButtressPersistence," +
                "Status,FailureStage,Vertices,Triangles,RootIntervals," +
                "OrientationFailures,DegenerateTriangles,TangentFailures," +
                "UnexpectedBoundaryLoops,NonManifoldEdges,GeometryFingerprint,Failure");
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
            currentProgress = 0f;
            currentDetail = "Preparing first case";
            currentEta = "ETA calculating";
            WriteReport(activeJob, "RUNNING", null);
            EditorApplication.update += Tick;
            AssemblyReloadEvents.beforeAssemblyReload += AbortForReload;
            EditorApplication.quitting += AbortForQuit;
            Debug.Log(
                "[TREE-CONTROLS.4H13] Wych Root Height Minimum Sweep started. Cases=" +
                TotalCases + ". Output=" + reportPath);
            return true;
        }

        internal static void RequestCancel()
        {
            if (activeJob != null)
            {
                activeJob.CancelRequested = true;
            }
        }

        internal static void CopyLastReport()
        {
            if (!string.IsNullOrEmpty(lastReportPath) &&
                File.Exists(lastReportPath))
            {
                EditorGUIUtility.systemCopyBuffer =
                    File.ReadAllText(lastReportPath);
            }
        }

        internal static void OpenOutputFolder()
        {
            Directory.CreateDirectory(OutputDirectory);
            EditorUtility.RevealInFinder(Path.GetFullPath(OutputDirectory));
        }

        private static void Tick()
        {
            Job job = activeJob;
            if (job == null)
            {
                return;
            }

            try
            {
                if (job.CancelRequested)
                {
                    Finish(job, "CANCELLED", null);
                    return;
                }

                float buttressPersistence =
                    ButtressPersistences[job.PersistenceIndex];
                Profile profile = Profiles[job.ProfileIndex];
                float rootHeight = RootHeights[job.HeightIndex];
                TimeSpan elapsed = DateTime.UtcNow - job.StartedUtc;
                double secondsPerCase = job.Completed > 0
                    ? elapsed.TotalSeconds / job.Completed
                    : 0.0;
                double eta = secondsPerCase * (TotalCases - job.Completed);

                currentProgress = job.Completed / (float)TotalCases;
                currentDetail = "Persistence " +
                    buttressPersistence.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture) +
                    " — " + profile.Name +
                    " — Root Height " +
                    rootHeight.ToString("0.000", CultureInfo.InvariantCulture);
                currentEta = "Elapsed " + FormatDuration(elapsed.TotalSeconds) +
                    " | ETA " + FormatDuration(eta);
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();

                CaseResult result = RunCase(
                    job.Representative,
                    profile,
                    rootHeight,
                    buttressPersistence);
                job.Results.Add(result);
                WriteCsv(job.CsvWriter, result);
                job.CsvWriter.Flush();
                job.Completed++;
                WriteReport(job, "RUNNING", null);

                if (!Advance(job))
                {
                    Finish(job, "COMPLETE", null);
                }
            }
            catch (Exception exception)
            {
                Finish(job, "FAILED", exception.ToString());
            }
        }

        private static CaseResult RunCase(
            Representative representative,
            Profile profile,
            float rootHeight,
            float buttressPersistence)
        {
            TreeResolvedControls controls = CloneControls(
                representative.Controls);
            SetFloat(controls, "missingBranchChance", 0f);
            SetFloat(controls, "brokenBranchChance", 0f);
            SetFloat(controls, "rootHeight", rootHeight);
            SetFloat(
                controls,
                "buttressTransition",
                buttressPersistence);
            if (profile.OverrideReach)
            {
                SetFloat(controls, "rootReach", profile.Reach);
            }
            if (profile.OverrideThickness)
            {
                SetFloat(controls, "rootThickness", profile.Thickness);
            }
            controls.ValidateAndClamp();

            var result = new CaseResult
            {
                Profile = profile.Name,
                ButtressPersistence = controls.ButtressTransition,
                RootHeight = rootHeight,
                RootReach = controls.RootReach,
                RootThickness = controls.RootThickness,
                FailureStage = "TREE_GENERATION"
            };

            TreeGenerationResult generation =
                TreeGenerator.GenerateExactForValidation(
                    controls,
                    representative.Seed,
                    representative.SourceIdentity,
                    representative.Family);
            if (generation == null || !generation.Passed ||
                generation.Definition == null ||
                !generation.Definition.IsValid)
            {
                result.Failure = generation != null
                    ? FirstFailureLine(generation.Report)
                    : "Tree generation returned null.";
                return result;
            }

            var mesh = new Mesh
            {
                name = "TREE-CONTROLS.4H13 Wych Root Height Case"
            };
            TreeBarkMeshBuildResult bark;
            try
            {
                bark = TreeBarkMeshGenerator.BuildForRootCollapseTournament(
                    generation.Definition,
                    TreeBarkMeshSettings.CreateRecipeOnlyDefaults(),
                    mesh,
                    TreeRootCollapseTournamentStrategy.Production);
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
            result.GeometryFingerprint = bark.GeometryFingerprint;
            TreeBarkMeshTopologyAuditResult audit = bark.TopologyAudit;
            if (audit != null)
            {
                result.OrientationFailures =
                    audit.SideOrientationFailureCount;
                result.DegenerateTriangles =
                    audit.DegenerateTriangleCount;
                result.TangentFailures =
                    audit.TangentBasisFailureCount;
                result.UnexpectedBoundaryLoops =
                    audit.UnexpectedExposedBoundaryLoopCount;
                result.NonManifoldEdges = audit.NonManifoldEdgeCount;
            }
            result.Passed = bark.Passed;
            result.Failure = bark.Failure;
            return result;
        }

        private static bool Advance(Job job)
        {
            job.HeightIndex++;
            if (job.HeightIndex < RootHeights.Length)
            {
                return true;
            }

            job.HeightIndex = 0;
            job.ProfileIndex++;
            if (job.ProfileIndex < Profiles.Length)
            {
                return true;
            }

            job.ProfileIndex = 0;
            job.PersistenceIndex++;
            return job.PersistenceIndex < ButtressPersistences.Length;
        }

        private static void Finish(
            Job job,
            string outcome,
            string failure)
        {
            if (job == null)
            {
                return;
            }

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
            Debug.Log(
                "[TREE-CONTROLS.4H13] Wych Root Height Minimum Sweep " +
                outcome + ". Report=" + job.ReportPath);
        }

        private static void AbortForReload()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Assembly reload interrupted the sweep; partial TXT and CSV were preserved.");
            }
        }

        private static void AbortForQuit()
        {
            if (activeJob != null)
            {
                Finish(
                    activeJob,
                    "CANCELLED",
                    "Editor shutdown interrupted the sweep; partial TXT and CSV were preserved.");
            }
        }

        private static void WriteReport(
            Job job,
            string outcome,
            string failure)
        {
            var report = new StringBuilder(24576);
            report.AppendLine(
                "[TREE-CONTROLS.4H13 Wych Root Height Minimum Sweep]");
            report.Append("Generated UTC: ")
                .AppendLine(DateTime.UtcNow.ToString("O"));
            report.Append("Outcome: ").AppendLine(outcome);
            report.Append("Selected representative: ")
                .AppendLine(job.Representative.Name);
            report.Append("Source identity: ")
                .AppendLine(job.Representative.SourceIdentity);
            report.Append("Generator / bark: ")
                .Append(TreeGenerator.CurrentGeneratorVersion)
                .Append(" / ")
                .AppendLine(TreeBarkMeshGenerator.BarkAlgorithmVersion.ToString());
            report.Append("Completed / total: ")
                .Append(job.Completed).Append(" / ")
                .AppendLine(TotalCases.ToString());
            report.AppendLine(
                "Buttress Persistences: 0.000, 0.100, 0.200, 0.300, 0.400, 0.500, 0.600, 0.700");
            report.AppendLine(
                "Root Heights: 0.050, 0.075, 0.100, 0.125, 0.150, 0.175, 0.200, 0.225");
            report.AppendLine(
                "Profiles: RecipeBaseline; ReachHigh=2.000; ThicknessHigh=1.000; ReachAndThicknessHigh.");
            if (!string.IsNullOrEmpty(failure))
            {
                report.AppendLine(failure);
            }

            report.AppendLine();
            report.AppendLine("[Common Minimum Across All Persistences]");
            float allPersistenceMinimum = FindCommonMinimum(
                job.Results,
                null);
            if (allPersistenceMinimum >= 0f)
            {
                report.Append(
                        "PASS — first Root Height passing all profiles at every tested persistence: ")
                    .AppendLine(allPersistenceMinimum.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture));
            }
            else if (job.Completed == TotalCases)
            {
                report.AppendLine(
                    "NONE — no Root Height passed all profiles at every tested persistence.");
            }
            else
            {
                report.AppendLine("PENDING — sweep is incomplete.");
            }

            report.AppendLine();
            report.AppendLine("[Minimum By Buttress Persistence]");
            for (int persistenceIndex = 0;
                persistenceIndex < ButtressPersistences.Length;
                persistenceIndex++)
            {
                float persistence = ButtressPersistences[persistenceIndex];
                float minimum = FindCommonMinimum(job.Results, persistence);
                report.Append("Buttress Persistence ")
                    .Append(persistence.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append(" | ");
                if (minimum >= 0f)
                {
                    report.Append("first common Root Height=")
                        .AppendLine(minimum.ToString(
                            "0.000",
                            CultureInfo.InvariantCulture));
                }
                else
                {
                    report.AppendLine(
                        job.Completed == TotalCases
                            ? "no common passing Root Height"
                            : "pending");
                }
            }

            report.AppendLine();
            report.AppendLine("[Persistence / Height Summary]");
            for (int persistenceIndex = 0;
                persistenceIndex < ButtressPersistences.Length;
                persistenceIndex++)
            {
                float persistence = ButtressPersistences[persistenceIndex];
                for (int heightIndex = 0;
                    heightIndex < RootHeights.Length;
                    heightIndex++)
                {
                    float height = RootHeights[heightIndex];
                    int completed = 0;
                    int passed = 0;
                    for (int resultIndex = 0;
                        resultIndex < job.Results.Count;
                        resultIndex++)
                    {
                        CaseResult item = job.Results[resultIndex];
                        if (!Mathf.Approximately(
                                item.ButtressPersistence,
                                persistence) ||
                            !Mathf.Approximately(item.RootHeight, height))
                        {
                            continue;
                        }
                        completed++;
                        if (item.Passed)
                        {
                            passed++;
                        }
                    }
                    report.Append("Persistence ")
                        .Append(persistence.ToString(
                            "0.000",
                            CultureInfo.InvariantCulture))
                        .Append(" | Root Height ")
                        .Append(height.ToString(
                            "0.000",
                            CultureInfo.InvariantCulture))
                        .Append(" | passed=").Append(passed).Append("/")
                        .Append(completed).AppendLine();
                }
            }

            report.AppendLine();
            report.AppendLine("[Completed Cases]");
            for (int index = 0; index < job.Results.Count; index++)
            {
                CaseResult item = job.Results[index];
                report.Append(item.Passed ? "PASS | " : "FAIL | ")
                    .Append(item.Profile)
                    .Append(" | persistence=")
                    .Append(item.ButtressPersistence.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append(" | rootHeight=")
                    .Append(item.RootHeight.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append(" | reach=")
                    .Append(item.RootReach.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append(" | thickness=")
                    .Append(item.RootThickness.ToString(
                        "0.000",
                        CultureInfo.InvariantCulture))
                    .Append(" | intervals=").Append(item.RootIntervals)
                    .Append(" | vertices=").Append(item.Vertices)
                    .Append(" | orientation=")
                    .Append(item.OrientationFailures)
                    .Append(" | degenerate=")
                    .Append(item.DegenerateTriangles)
                    .Append(" | tangents=").Append(item.TangentFailures)
                    .Append(" | unexpectedLoops=")
                    .Append(item.UnexpectedBoundaryLoops)
                    .Append(" | nonManifold=")
                    .Append(item.NonManifoldEdges);
                if (!item.Passed)
                {
                    report.Append(" | stage=")
                        .Append(item.FailureStage)
                        .Append(" | ")
                        .Append(FirstFailureLine(item.Failure));
                }
                report.AppendLine();
            }
            report.Append("CSV: ").AppendLine(job.CsvPath);
            File.WriteAllText(job.ReportPath, report.ToString(), Encoding.UTF8);
        }

        private static float FindCommonMinimum(
            List<CaseResult> results,
            float? buttressPersistence)
        {
            for (int heightIndex = 0;
                heightIndex < RootHeights.Length;
                heightIndex++)
            {
                float height = RootHeights[heightIndex];
                int completed = 0;
                int passed = 0;
                for (int resultIndex = 0;
                    resultIndex < results.Count;
                    resultIndex++)
                {
                    CaseResult item = results[resultIndex];
                    if (!Mathf.Approximately(item.RootHeight, height) ||
                        (buttressPersistence.HasValue &&
                         !Mathf.Approximately(
                             item.ButtressPersistence,
                             buttressPersistence.Value)))
                    {
                        continue;
                    }
                    completed++;
                    if (item.Passed)
                    {
                        passed++;
                    }
                }

                int expected = buttressPersistence.HasValue
                    ? Profiles.Length
                    : Profiles.Length * ButtressPersistences.Length;
                if (completed == expected && passed == expected)
                {
                    return height;
                }
            }
            return -1f;
        }

        private static Representative BuildRepresentative(
            ProceduralTreeInstance selected)
        {
            if (selected == null || !selected.HasExactControls)
            {
                return null;
            }

            string identity = selected.Recipe != null
                ? selected.Recipe.StableIdentity
                : selected.ExactControlsSourceRecipeIdentity;
            if (string.IsNullOrEmpty(identity) ||
                identity.IndexOf(
                    "wych-elm",
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                return null;
            }

            TreeResolvedControls controls;
            if (selected.Recipe != null &&
                selected.Recipe.ControlRanges != null)
            {
                controls = new TreeResolvedControls();
                controls.ResolveFrom(
                    selected.Recipe.ControlRanges,
                    selected.MasterSeed);
            }
            else
            {
                controls = CloneControls(selected.ExactControls);
            }

            return new Representative
            {
                Name = selected.Recipe != null
                    ? selected.Recipe.RecipeDisplayName
                    : selected.name,
                Family = selected.Family,
                Seed = selected.MasterSeed,
                SourceIdentity = identity,
                Controls = controls
            };
        }

        private static TreeResolvedControls CloneControls(
            TreeResolvedControls source)
        {
            var clone = new TreeResolvedControls();
            if (source == null)
            {
                return clone;
            }

            FieldInfo[] fields = typeof(TreeResolvedControls).GetFields(
                BindingFlags.Instance |
                BindingFlags.NonPublic |
                BindingFlags.Public);
            for (int index = 0; index < fields.Length; index++)
            {
                FieldInfo field = fields[index];
                if (!field.IsInitOnly)
                {
                    field.SetValue(clone, field.GetValue(source));
                }
            }
            return clone;
        }

        private static void SetFloat(
            TreeResolvedControls controls,
            string fieldName,
            float value)
        {
            FieldInfo field = typeof(TreeResolvedControls).GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new MissingFieldException(
                    typeof(TreeResolvedControls).FullName,
                    fieldName);
            }
            field.SetValue(controls, value);
        }

        private static void WriteCsv(StreamWriter writer, CaseResult result)
        {
            writer.Write(Escape(result.Profile)); writer.Write(',');
            writer.Write(result.RootHeight.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.RootReach.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.RootThickness.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.ButtressPersistence.ToString("R", CultureInfo.InvariantCulture)); writer.Write(',');
            writer.Write(result.Passed ? "PASS" : "FAIL"); writer.Write(',');
            writer.Write(Escape(result.FailureStage)); writer.Write(',');
            writer.Write(result.Vertices); writer.Write(',');
            writer.Write(result.Triangles); writer.Write(',');
            writer.Write(result.RootIntervals); writer.Write(',');
            writer.Write(result.OrientationFailures); writer.Write(',');
            writer.Write(result.DegenerateTriangles); writer.Write(',');
            writer.Write(result.TangentFailures); writer.Write(',');
            writer.Write(result.UnexpectedBoundaryLoops); writer.Write(',');
            writer.Write(result.NonManifoldEdges); writer.Write(',');
            writer.Write(Escape(result.GeometryFingerprint)); writer.Write(',');
            writer.WriteLine(Escape(result.Failure));
        }

        private static string FirstFailureLine(string failure)
        {
            if (string.IsNullOrEmpty(failure))
            {
                return string.Empty;
            }
            int newline = failure.IndexOf('\n');
            return newline >= 0
                ? failure.Substring(0, newline).Trim()
                : failure.Trim();
        }

        private static string Escape(string value)
        {
            string safe = value ?? string.Empty;
            return "\"" + safe.Replace("\"", "\"\"") + "\"";
        }

        private static string FormatDuration(double seconds)
        {
            if (double.IsNaN(seconds) || double.IsInfinity(seconds) ||
                seconds < 0.0)
            {
                return "calculating";
            }
            TimeSpan duration = TimeSpan.FromSeconds(seconds);
            return duration.TotalHours >= 1.0
                ? duration.ToString(@"h\:mm\:ss")
                : duration.ToString(@"m\:ss");
        }
    }
}
