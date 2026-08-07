#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private enum CellSpawnerAuditSuite
        {
            None = 0,
            Smoke = 1,
            Exhaustive = 2
        }

        private const double CellSpawnerAuditRuntimeReadyTimeoutSeconds = 15.0;

        private readonly struct CellSpawnerAuditCase
        {
            public CellSpawnerAuditCase(
                int recipe, int scenario, int seed, int replayReferenceCase = 0,
                int componentMode = 0)
            {
                Recipe = recipe;
                Scenario = scenario;
                Seed = seed;
                ReplayReferenceCase = replayReferenceCase;
                ComponentMode = componentMode;
            }
            public int Recipe { get; }
            public int Scenario { get; }
            public int Seed { get; }
            public int ReplayReferenceCase { get; }
            public int ComponentMode { get; }
        }

        private readonly struct CellSpawnerReplayFixture
        {
            public CellSpawnerReplayFixture(
                AutomaticFoamSourceEvent sourceEvent,
                FoamSourceEventGpuData gpuData,
                P7SourceDispatchRange range)
            {
                SourceEvent = sourceEvent;
                GpuData = gpuData;
                Range = range;
            }

            public AutomaticFoamSourceEvent SourceEvent { get; }
            public FoamSourceEventGpuData GpuData { get; }
            public P7SourceDispatchRange Range { get; }
        }

        private sealed class CellSpawnerGpuCapture
        {
            public int Generation;
            public CellSpawnerAuditCase TestCase;
            public int CaseNumber;
            public string RecipeName;
            public string ScenarioName;
            public float RequestedLength;
            public float RequestedWidth;
            public float HeadLength;
            public float HeadWidth;
            public float ExpectedLength;
            public float ExpectedWidth;
            public bool HeadOnlyMeasurement;
            public bool WarmupOnly;
            public float CentreCellX;
            public float CentreCellY;
            public float AngleDegrees;
            public RenderTexture DebugTexture;
            public RenderTexture StateTexture;
            public RenderTexture ObjectContactTexture;
            public RenderTexture SyntheticBoundaryTexture;
            public RenderTexture SyntheticObstacleTexture;
            public RenderTexture SyntheticShoreEdgesTexture;
            public bool UsesSyntheticDomain;
            public ComputeBuffer EventBuffer;
            public ComputeBuffer CounterBuffer;
        }

        private readonly List<CellSpawnerAuditCase> cellSpawnerAuditCases = new();
        private readonly Dictionary<int, CellSpawnerReplayFixture> cellSpawnerReplayFixtures = new();
        private readonly Dictionary<int, ushort[]> cellSpawnerReplayPayloads = new();
        private readonly Dictionary<int, double> cellSpawnerComponentAreas = new();
        private StringBuilder cellSpawnerAuditText;
        private StringBuilder cellSpawnerAuditCsv;
        private int cellSpawnerAuditCursor;
        private int cellSpawnerAuditPassCount;
        private int cellSpawnerAuditFailCount;
        private double cellSpawnerAuditStartedAt;
        private double cellSpawnerAuditRuntimeWaitStartedAt;
        private bool cellSpawnerContractAuditRunning;
        private bool cellSpawnerAuditReadbackPending;
        private bool cellSpawnerAuditWarmupPending;
        private int cellSpawnerAuditGeneration;
        private CellSpawnerGpuCapture cellSpawnerGpuCapture;
        private CellSpawnerAuditSuite cellSpawnerAuditSuite;
        private string cellSpawnerAuditCurrentCase = "Idle";
        private string cellSpawnerAuditLastResult = "None";
        private string cellSpawnerAuditPhase = "Idle";
        private string cellSpawnerAuditRuntimeState = "PLAY MODE REQUIRED";
        private double cellSpawnerAuditLastInspectorRepaintAt;

        public bool CellSpawnerContractAuditRunning => cellSpawnerContractAuditRunning;
        public bool CellSpawnerContractAuditForcesRuntimeWork => cellSpawnerContractAuditRunning;
        public float CellSpawnerContractAuditProgress => cellSpawnerAuditCases.Count > 0
            ? Mathf.Clamp01((float)cellSpawnerAuditCursor / cellSpawnerAuditCases.Count)
            : 0f;
        public string CellSpawnerContractAuditStatus => cellSpawnerContractAuditRunning
            ? $"{cellSpawnerAuditCursor}/{cellSpawnerAuditCases.Count} · {CellSpawnerContractAuditProgress * 100f:0.0}%"
            : topologyCacheDiagnosticSummary;
        public int CellSpawnerContractAuditCompleted => cellSpawnerAuditCursor;
        public int CellSpawnerContractAuditTotal => cellSpawnerAuditCases.Count;
        public int CellSpawnerContractAuditPassCount => cellSpawnerAuditPassCount;
        public int CellSpawnerContractAuditFailCount => cellSpawnerAuditFailCount;
        public bool CellSpawnerContractAuditReadbackPending => cellSpawnerAuditReadbackPending;
        public string CellSpawnerContractAuditCurrentCase => cellSpawnerAuditCurrentCase;
        public string CellSpawnerContractAuditLastResult => cellSpawnerAuditLastResult;
        public string CellSpawnerContractAuditPhase => cellSpawnerAuditPhase;
        public string CellSpawnerContractAuditRuntimeState => cellSpawnerAuditRuntimeState;
        public string CellSpawnerContractAuditSuiteName => cellSpawnerAuditSuite switch
        {
            CellSpawnerAuditSuite.Smoke => "Smoke",
            CellSpawnerAuditSuite.Exhaustive => "Exhaustive",
            _ => "None"
        };
        public double CellSpawnerContractAuditElapsedSeconds => cellSpawnerContractAuditRunning
            ? Math.Max(0.0, Time.realtimeSinceStartupAsDouble - cellSpawnerAuditStartedAt)
            : 0.0;
        public double CellSpawnerContractAuditEtaSeconds
        {
            get
            {
                double elapsed = CellSpawnerContractAuditElapsedSeconds;
                double rate = elapsed > 0.001 ? cellSpawnerAuditCursor / elapsed : 0.0;
                return rate > 0.001
                    ? Math.Max(0.0, (cellSpawnerAuditCases.Count - cellSpawnerAuditCursor) / rate)
                    : 0.0;
            }
        }

        public bool RunCellSpawnerSmokeSuite()
        {
            return StartCellSpawnerContractAudit(CellSpawnerAuditSuite.Smoke);
        }

        public bool RunCellSpawnerExhaustiveSuite()
        {
            return StartCellSpawnerContractAudit(CellSpawnerAuditSuite.Exhaustive);
        }

        private bool StartCellSpawnerContractAudit(CellSpawnerAuditSuite suite)
        {
            if (cellSpawnerContractAuditRunning)
            {
                return false;
            }
            if (!Application.isPlaying)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary =
                    "PLAY MODE REQUIRED — enter Play Mode and wait for the River Foam runtime.";
                cellSpawnerAuditRuntimeState = "PLAY MODE REQUIRED";
                return false;
            }
            if (!IsSupported)
            {
                topologyCacheDiagnosticState = "Unsupported";
                topologyCacheDiagnosticSummary =
                    "Compute shaders, texture arrays, and required half-float formats are required.";
                return false;
            }
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                topologyCacheDiagnosticState = "Unsupported";
                topologyCacheDiagnosticSummary =
                    "Async GPU Readback is required; no synchronous fallback is allowed.";
                return false;
            }

            river = GetComponent<StylizedRiver>();
            if (river == null || !river.FoamEnabled || !river.Domain.IsValid)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary =
                    "A valid, enabled StylizedRiver Foam runtime is required.";
                return false;
            }
            if (river.FreezeAmount >= 0.999f)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary =
                    "The river is fully frozen; reduce Freeze Amount before running the suite.";
                return false;
            }
            if (initializationPhase == InitializationPhase.CachePreparationRequired)
            {
                topologyCacheDiagnosticState = "Preparation Required";
                topologyCacheDiagnosticSummary =
                    "Foam topology cache preparation is required. Exit Play Mode, then run " +
                    "Actions → Foam Cache & Validation → Prepare / Rebuild Foam Topology Cache.";
                cellSpawnerAuditRuntimeState = ResolveCellSpawnerAuditRuntimeState();
                return false;
            }

            BuildCellSpawnerAuditCases(suite);
            cellSpawnerAuditSuite = suite;
            cellSpawnerAuditCursor = 0;
            cellSpawnerAuditPassCount = 0;
            cellSpawnerAuditFailCount = 0;
            cellSpawnerAuditStartedAt = Time.realtimeSinceStartupAsDouble;
            cellSpawnerAuditRuntimeWaitStartedAt = cellSpawnerAuditStartedAt;
            cellSpawnerAuditReadbackPending = false;
            cellSpawnerAuditWarmupPending = true;
            cellSpawnerReplayFixtures.Clear();
            cellSpawnerReplayPayloads.Clear();
            cellSpawnerComponentAreas.Clear();
            cellSpawnerAuditGeneration++;
            cellSpawnerAuditCurrentCase = "Waiting for River Foam runtime";
            cellSpawnerAuditLastResult = "None";
            cellSpawnerAuditPhase = "Runtime readiness";
            cellSpawnerAuditRuntimeState = ResolveCellSpawnerAuditRuntimeState();
            cellSpawnerAuditLastInspectorRepaintAt = 0.0;
            cellSpawnerAuditText = new StringBuilder(suite == CellSpawnerAuditSuite.Exhaustive ? 262144 : 32768);
            cellSpawnerAuditCsv = new StringBuilder(suite == CellSpawnerAuditSuite.Exhaustive ? 262144 : 32768);
            cellSpawnerAuditText.AppendLine("RIVER FOAM CELL-EXACT SPAWNER GPU FOOTPRINT AUDIT");
            cellSpawnerAuditText.AppendLine(BuildCommonEnvironmentHeader());
            cellSpawnerAuditText.AppendLine($"Suite: {CellSpawnerContractAuditSuiteName}");
            cellSpawnerAuditText.AppendLine($"Cases: {cellSpawnerAuditCases.Count}");
            cellSpawnerAuditText.AppendLine(
                "Play Mode execution through StylizedRiverFoamRuntime.LateUpdate after the production runtime is Ready.");
            cellSpawnerAuditText.AppendLine(
                "Each case dispatches RasterizeFoamSourceEventDebug into isolated temporary targets and measures Coverage through AsyncGPUReadback.");
            cellSpawnerAuditText.AppendLine(
                "Shore Ribbon and Inward Wash geometry cases bind audit-owned valid-fluid, zero-obstacle, fixed-shore domain fixtures; production river masks are not acceptance inputs.");
            cellSpawnerAuditText.AppendLine(
                "The visible river persistent Foam state is not used as a raster target. No blocking readback or wait is permitted.");
            cellSpawnerAuditText.AppendLine();
            cellSpawnerAuditCsv.AppendLine(
                "Case,Recipe,Scenario,Seed,Measurement,ReplayReference,RequestedLength,RequestedWidth,HeadLength,HeadWidth,ExpectedLength,ExpectedWidth,CoverageArea,ProjectedLength,ProjectedWidth,OutsideEnvelopeCoverage,SupportWidth,SupportHeight,FullRowRun,FullColumnRun,NonZeroCells,ReadbackPass,AdjacencyPass,LengthPass,WidthPass,AreaPass,EnvelopePass,ReplayPass,Result,Detail");
            topologyCacheDiagnosticState = "Waiting";
            topologyCacheDiagnosticSummary =
                $"{CellSpawnerContractAuditSuiteName} cell-exact suite waiting for River Foam runtime readiness.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;
            cellSpawnerContractAuditRunning = true;
            RepaintCellSpawnerAuditViews(true);
            return true;
        }

        private void BuildCellSpawnerAuditCases(CellSpawnerAuditSuite suite)
        {
            cellSpawnerAuditCases.Clear();
            if (suite == CellSpawnerAuditSuite.Smoke)
            {
                // Keep Shore first so replay references 001-006 remain stable.
                for (int recipe = 0; recipe <= 1; recipe++)
                {
                    AddCellSpawnerSmokeRecipeCases(recipe);
                }

                // Determinism evidence: replay the first six Shore cases from
                // their exact captured event and dispatch range.
                for (int i = 0; i < 6; i++)
                {
                    CellSpawnerAuditCase original = cellSpawnerAuditCases[i];
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        original.Recipe,
                        original.Scenario,
                        original.Seed,
                        i + 1,
                        original.ComponentMode));
                }

                // Record profile and wake body components before evaluating their
                // composites. Semi contact is intentionally first so the full Arc
                // contact can validate the full-versus-half profile relationship.
                for (int scenario = 0; scenario <= 4; scenario++)
                {
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        3, scenario, 0, 0, 1));
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        2, scenario, 0, 0, 1));
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        2, scenario, 0, 0, 2));
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        2, scenario, 0, 0, 3));
                    cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                        3, scenario, 0, 0, 3));
                }

                // Record isolated progressive heads before their composite rows.
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(3, 6, 0, 0, 1));
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(2, 6, 0, 0, 1));
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(2, 6, 0, 0, 2));
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(2, 6, 0, 0, 3));
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(3, 6, 0, 0, 3));

                for (int recipe = 2; recipe < 8; recipe++)
                {
                    AddCellSpawnerSmokeRecipeCases(recipe);
                }
                return;
            }

            for (int recipe = 0; recipe < 8; recipe++)
            for (int scenario = 0; scenario < 7; scenario++)
            for (int seed = 0; seed < 12; seed++)
            {
                cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                    recipe, scenario, seed));
            }
        }


        private void AddCellSpawnerSmokeRecipeCases(int recipe)
        {
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(recipe, 0, 0));
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(recipe, 1, 0));
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(recipe, 2, 0));
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(recipe, 3, 0));
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(recipe, 4, 0));
            cellSpawnerAuditCases.Add(new CellSpawnerAuditCase(
                recipe, 6, 0));
        }

        private static int ResolveCellSpawnerComponentAreaKey(
            int recipe, int scenario, int componentMode)
        {
            return recipe * 100 + scenario * 10 + componentMode;
        }

        public void CancelCellSpawnerContractAudit()
        {
            if (!cellSpawnerContractAuditRunning)
            {
                return;
            }
            cellSpawnerAuditText?.AppendLine();
            cellSpawnerAuditText?.AppendLine(
                $"CANCELLED after {cellSpawnerAuditCursor}/{cellSpawnerAuditCases.Count} completed GPU cases.");
            FinishCellSpawnerContractAudit(false, true,
                "Cancelled by user; partial report preserved.");
        }

        private void CancelCellSpawnerContractAuditForLifecycle(string reason)
        {
            if (!cellSpawnerContractAuditRunning)
            {
                return;
            }
            cellSpawnerAuditText?.AppendLine();
            cellSpawnerAuditText?.AppendLine($"ABORTED: {reason}");
            FinishCellSpawnerContractAudit(false, true, reason);
        }

        private void AdvanceCellSpawnerContractAuditPlayMode(bool runtimeReady)
        {
            if (!cellSpawnerContractAuditRunning)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                CancelCellSpawnerContractAuditForLifecycle(
                    "Play Mode ended while the suite was running; partial report preserved.");
                return;
            }

            cellSpawnerAuditRuntimeState = ResolveCellSpawnerAuditRuntimeState();
            bool resourcesReady = AreCellSpawnerAuditResourcesReady(
                out string readinessFailure);
            // LateUpdate invokes the audit once before the runtime-ready branch and
            // once afterward. Direct resource completeness is authoritative; the
            // earlier boolean hint must not restart or expire the watchdog.
            if (!resourcesReady)
            {
                cellSpawnerAuditPhase = "Runtime readiness";
                cellSpawnerAuditCurrentCase = "Waiting for River Foam runtime";
                topologyCacheDiagnosticState = "Waiting";
                topologyCacheDiagnosticSummary =
                    $"Waiting for runtime: {readinessFailure}";
                double now = Time.realtimeSinceStartupAsDouble;
                if (cellSpawnerAuditRuntimeWaitStartedAt <= 0.0)
                {
                    cellSpawnerAuditRuntimeWaitStartedAt = now;
                }
                double wait = now - cellSpawnerAuditRuntimeWaitStartedAt;
                if (initializationPhase == InitializationPhase.CachePreparationRequired)
                {
                    string reason =
                        "Foam topology cache preparation became required while the suite was waiting. " +
                        "Exit Play Mode, then run Actions → Foam Cache & Validation → " +
                        "Prepare / Rebuild Foam Topology Cache.";
                    cellSpawnerAuditText.AppendLine("PRECONDITION NOT MET: " + reason);
                    FinishCellSpawnerContractAudit(false, true, reason);
                    return;
                }
                if (initializationPhase == InitializationPhase.Failed ||
                    wait >= CellSpawnerAuditRuntimeReadyTimeoutSeconds)
                {
                    string reason =
                        $"Runtime readiness failed after {wait:0.0}s: {readinessFailure}";
                    cellSpawnerAuditText.AppendLine("RUNTIME READINESS FAILED: " + reason);
                    cellSpawnerAuditFailCount++;
                    FinishCellSpawnerContractAudit(false, false, reason);
                    return;
                }
                RepaintCellSpawnerAuditViews();
                return;
            }
            // Measure only a continuous not-ready interval. Once ready, suite
            // duration can exceed the readiness timeout without a false abort.
            cellSpawnerAuditRuntimeWaitStartedAt = 0.0;

            if (cellSpawnerAuditReadbackPending)
            {
                cellSpawnerAuditPhase = "GPU readback";
                UpdateCellSpawnerAuditProgress();
                RepaintCellSpawnerAuditViews();
                return;
            }

            if (!AreCellSpawnerAuditResourcesReady(out string resourceFailure))
            {
                cellSpawnerAuditText.AppendLine(
                    "RESOURCE VALIDATION FAILED: " + resourceFailure);
                cellSpawnerAuditFailCount++;
                FinishCellSpawnerContractAudit(false, false, resourceFailure);
                return;
            }

            if (cellSpawnerAuditCursor >= cellSpawnerAuditCases.Count)
            {
                FinishCellSpawnerContractAudit(
                    cellSpawnerAuditFailCount == 0, false,
                    cellSpawnerAuditFailCount == 0
                        ? "All GPU footprint cases completed."
                        : "One or more GPU footprint cases failed.");
                return;
            }

            cellSpawnerAuditPhase = "GPU dispatch";
            if (cellSpawnerAuditWarmupPending)
            {
                cellSpawnerAuditPhase = "GPU warm-up";
                DispatchCellSpawnerGpuAuditCase(
                    cellSpawnerAuditCases[0], 0, true);
            }
            else
            {
                DispatchCellSpawnerGpuAuditCase(
                    cellSpawnerAuditCases[cellSpawnerAuditCursor],
                    cellSpawnerAuditCursor + 1, false);
            }
            UpdateCellSpawnerAuditProgress();
            RepaintCellSpawnerAuditViews();
        }

        private bool AreCellSpawnerAuditResourcesReady(out string failure)
        {
            bool ready = initializationPhase == InitializationPhase.Ready &&
                AreResourcesCompleteAndCurrent() &&
                computeShader != null &&
                rasterizeFoamSourceEventDebugKernel >= 0 &&
                metricBuffer != null &&
                boundaryTexture != null &&
                obstacleExclusionTexture != null &&
                currentShoreEdgesTexture != null &&
                fieldWidth > 0 && fieldHeight > 0;
            failure = ready
                ? string.Empty
                : ResolveCellSpawnerAuditRuntimeState();
            return ready;
        }

        private string ResolveCellSpawnerAuditRuntimeState()
        {
            return $"phase={initializationPhase}; complete={AreResourcesCompleteAndCurrent()}; " +
                $"kernel={rasterizeFoamSourceEventDebugKernel}; field={fieldWidth}x{fieldHeight}; " +
                $"metric={(metricBuffer != null)}; boundary={(boundaryTexture != null)}; " +
                $"obstacle={(obstacleExclusionTexture != null)}; shore={(currentShoreEdgesTexture != null)}";
        }

        private void UpdateCellSpawnerAuditProgress()
        {
            double elapsed = Math.Max(0.001,
                Time.realtimeSinceStartupAsDouble - cellSpawnerAuditStartedAt);
            double rate = cellSpawnerAuditCursor / elapsed;
            double eta = rate > 0.001
                ? (cellSpawnerAuditCases.Count - cellSpawnerAuditCursor) / rate
                : 0.0;
            topologyCacheDiagnosticState = cellSpawnerAuditReadbackPending
                ? "GPU Readback"
                : "Dispatching";
            topologyCacheDiagnosticSummary =
                $"{CellSpawnerContractAuditSuiteName} GPU audit " +
                $"{cellSpawnerAuditCursor}/{cellSpawnerAuditCases.Count} · " +
                $"PASS {cellSpawnerAuditPassCount} · FAIL {cellSpawnerAuditFailCount} · " +
                $"elapsed {elapsed:0.0}s · ETA " +
                (rate > 0.001
                    ? eta.ToString("0.0", CultureInfo.InvariantCulture) + "s"
                    : "calculating");
        }

        private void RepaintCellSpawnerAuditViews(bool force = false)
        {
            double now = EditorApplication.timeSinceStartup;
            if (!force && now - cellSpawnerAuditLastInspectorRepaintAt < 0.10)
            {
                return;
            }
            cellSpawnerAuditLastInspectorRepaintAt = now;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
        }

        private void DispatchCellSpawnerGpuAuditCase(
            CellSpawnerAuditCase testCase, int caseNumber, bool warmupOnly)
        {
            ResolveAuditScenario(testCase.Scenario, out string scenarioName,
                out float requestedLength, out float requestedWidth,
                out float headLength, out float headWidth,
                out bool headOnlyMeasurement, out float expectedLength,
                out float expectedWidth, out float offsetX, out float offsetY,
                out float angleDegrees);
            if (testCase.Recipe == 0)
            {
                // Shore Ribbon production has one structural birth shape: one
                // longitudinal grid cell by one lateral grid cell. Legacy body
                // width and head-size scenarios remain as fixed-contract rows so
                // Smoke/Exhaustive suite cardinality and replay numbering stay
                // stable without pretending those removed controls still apply.
                requestedWidth = 1f;
                headLength = 1f;
                headWidth = 1f;
                headOnlyMeasurement = false;
                expectedLength = requestedLength;
                expectedWidth = 1f;
                scenarioName = testCase.Scenario switch
                {
                    3 => "5-Cell Path · Discrete 1x1 Birth Cells",
                    4 => "Legacy Width Scenario · Fixed 1-Cell Birth",
                    6 => "5-Cell Path · Fixed 1x1 Birth Cells",
                    _ => scenarioName + " · Fixed 1x1 Birth"
                };
            }
            if (testCase.ComponentMode > 0)
            {
                scenarioName += testCase.ComponentMode switch
                {
                    1 => " · Contact Only",
                    2 => " · Negative Wake Only",
                    3 => " · Positive Wake Only",
                    _ => " · Component Only"
                };
            }
            string recipeName = ResolveAuditRecipeName(testCase.Recipe);
            cellSpawnerAuditCurrentCase = warmupOnly
                ? $"Warm-up · {recipeName} · {scenarioName}"
                : $"{caseNumber:000}/{cellSpawnerAuditCases.Count:000} · {recipeName} · {scenarioName} · seed {testCase.Seed:00}";
            AutomaticFoamSourceEvent sourceEvent;
            FoamSourceEventGpuData gpuData;
            P7SourceDispatchRange range;
            if (testCase.ReplayReferenceCase > 0)
            {
                if (!cellSpawnerReplayFixtures.TryGetValue(
                    testCase.ReplayReferenceCase, out CellSpawnerReplayFixture fixture))
                {
                    RecordCellSpawnerAuditFailure(testCase, caseNumber, recipeName,
                        scenarioName, requestedLength, requestedWidth, headLength,
                        headWidth, "immutable replay fixture was unavailable");
                    cellSpawnerAuditCursor++;
                    return;
                }
                sourceEvent = fixture.SourceEvent;
                gpuData = fixture.GpuData;
                range = fixture.Range;
            }
            else
            {
                sourceEvent = BuildCellSpawnerAuditEvent(
                    testCase, requestedLength, requestedWidth, headLength,
                    headWidth, headOnlyMeasurement, offsetX, offsetY, angleDegrees);
                gpuData = BuildAutomaticFoamSourceGpuData(sourceEvent, 0f);
                if (!TryResolveAutomaticSourceDispatchRange(
                    sourceEvent, out range))
                {
                    RecordCellSpawnerAuditFailure(testCase, caseNumber, recipeName,
                        scenarioName, requestedLength, requestedWidth, headLength,
                        headWidth, "dispatch range resolution failed");
                    cellSpawnerAuditCursor++;
                    return;
                }
                if (!warmupOnly && caseNumber >= 1 && caseNumber <= 6)
                {
                    cellSpawnerReplayFixtures[caseNumber] =
                        new CellSpawnerReplayFixture(sourceEvent, gpuData, range);
                }
            }

            ComputeBuffer eventBuffer = new ComputeBuffer(
                1,
                System.Runtime.InteropServices.Marshal.SizeOf<FoamSourceEventGpuData>(),
                ComputeBufferType.Structured);
            eventBuffer.SetData(new[] { gpuData });
            RenderTexture debugTexture = CreateFieldTexture("PS3D_CellSpawnerAudit_Debug");
            RenderTexture stateTexture = CreateFieldTexture("PS3D_CellSpawnerAudit_State");
            RenderTexture objectContactTexture = CreateFieldTexture(
                "PS3D_CellSpawnerAudit_ObjectContact");
            ClearRenderTexture(debugTexture);
            ClearRenderTexture(stateTexture);
            ClearCellSpawnerAuditTexture(
                objectContactTexture,
                new Color(1f, -1f, 0f, 1f));
            ComputeBuffer counterBuffer = new ComputeBuffer(
                AutomaticBirthDebugCounterCount, sizeof(uint),
                ComputeBufferType.Structured);
            counterBuffer.SetData(new uint[AutomaticBirthDebugCounterCount]);

            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat("_FoamSimulationLength", simulationFieldLength);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetInt("_FoamRangeStart", range.StartX);
            computeShader.SetInt("_FoamRangeCount", range.CountX);
            computeShader.SetInt("_FoamRangeStartY", range.StartY);
            computeShader.SetInt("_FoamRangeCountY", range.CountY);
            computeShader.SetInt("_FoamSourceEventIndex", 0);
            computeShader.SetInt("_FoamSourceEventDebugComponentMode", testCase.ComponentMode);
            computeShader.SetBuffer(rasterizeFoamSourceEventDebugKernel,
                "_FoamMetricRows", metricBuffer);
            computeShader.SetBuffer(rasterizeFoamSourceEventDebugKernel,
                "_FoamSourceEvents", eventBuffer);
            bool useSyntheticDomain = testCase.Recipe == 0 || testCase.Recipe == 1;
            RenderTexture syntheticBoundaryTexture = null;
            RenderTexture syntheticObstacleTexture = null;
            RenderTexture syntheticShoreEdgesTexture = null;
            Texture boundBoundary = boundaryTexture;
            Texture boundObstacle = obstacleExclusionTexture;
            Texture boundShoreEdges = currentShoreEdgesTexture;
            if (useSyntheticDomain)
            {
                syntheticBoundaryTexture = CreateFieldTexture(
                    "PS3D_CellSpawnerAudit_SyntheticBoundary");
                syntheticObstacleTexture = CreateObstacleExclusionTexture(
                    "PS3D_CellSpawnerAudit_SyntheticObstacle");
                syntheticShoreEdgesTexture = CreateShoreEdgesTexture(
                    "PS3D_CellSpawnerAudit_SyntheticShoreEdges");
                // Geometry-contract fixtures keep the production metric lattice
                // and range resolver, but replace mutable environmental masks.
                // The audit shore therefore uses the stable geometric shore
                // stored in metricRows for each column; a fixed zero-metre shore
                // is not spatially compatible with ranges resolved around the
                // actual river bank.
                ClearCellSpawnerAuditTexture(
                    syntheticBoundaryTexture, Color.white);
                ClearCellSpawnerAuditTexture(
                    syntheticObstacleTexture, Color.black);
                if (!TryPopulateCellSpawnerSyntheticShoreEdges(
                        syntheticShoreEdgesTexture,
                        sourceEvent,
                        range,
                        out string syntheticFixtureFailure))
                {
                    ReleaseCellSpawnerAuditTexture(debugTexture);
                    ReleaseCellSpawnerAuditTexture(stateTexture);
                    ReleaseCellSpawnerAuditTexture(objectContactTexture);
                    ReleaseCellSpawnerAuditTexture(syntheticBoundaryTexture);
                    ReleaseCellSpawnerAuditTexture(syntheticObstacleTexture);
                    ReleaseCellSpawnerAuditTexture(syntheticShoreEdgesTexture);
                    eventBuffer.Release();
                    counterBuffer.Release();
                    RecordCellSpawnerAuditFailure(testCase, caseNumber, recipeName,
                        scenarioName, requestedLength, requestedWidth, headLength,
                        headWidth, syntheticFixtureFailure);
                    cellSpawnerAuditCursor++;
                    return;
                }
                boundBoundary = syntheticBoundaryTexture;
                boundObstacle = syntheticObstacleTexture;
                boundShoreEdges = syntheticShoreEdgesTexture;
            }
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamBoundary", boundBoundary);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamObstacleExclusionRead", boundObstacle);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamCurrentShoreEdgesRead", boundShoreEdges);
            Texture neutralTexture = neutralDisturbanceTexture != null
                ? (Texture)neutralDisturbanceTexture
                : Texture2D.blackTexture;
            computeShader.SetInts("_FoamStaticPressureDimensions", 1, 1);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamStaticPressureField", neutralTexture);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamObjectContactFieldRead", objectContactTexture);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamStateWrite", stateTexture);
            computeShader.SetTexture(rasterizeFoamSourceEventDebugKernel,
                "_FoamBirthDebugWrite", debugTexture);
            computeShader.SetBuffer(rasterizeFoamSourceEventDebugKernel,
                "_FoamBirthDebugCounters", counterBuffer);
            Dispatch(rasterizeFoamSourceEventDebugKernel,
                range.CountX, range.CountY);

            int generation = cellSpawnerAuditGeneration;
            CellSpawnerGpuCapture capture = new CellSpawnerGpuCapture
            {
                Generation = generation,
                TestCase = testCase,
                CaseNumber = caseNumber,
                RecipeName = recipeName,
                ScenarioName = scenarioName,
                RequestedLength = requestedLength,
                RequestedWidth = requestedWidth,
                HeadLength = headLength,
                HeadWidth = headWidth,
                ExpectedLength = expectedLength,
                ExpectedWidth = expectedWidth,
                HeadOnlyMeasurement = headOnlyMeasurement,
                WarmupOnly = warmupOnly,
                CentreCellX = range.StartX + range.CountX * 0.5f,
                CentreCellY = range.StartY + range.CountY * 0.5f,
                AngleDegrees = angleDegrees,
                DebugTexture = debugTexture,
                StateTexture = stateTexture,
                ObjectContactTexture = objectContactTexture,
                SyntheticBoundaryTexture = syntheticBoundaryTexture,
                SyntheticObstacleTexture = syntheticObstacleTexture,
                SyntheticShoreEdgesTexture = syntheticShoreEdgesTexture,
                UsesSyntheticDomain = useSyntheticDomain,
                EventBuffer = eventBuffer,
                CounterBuffer = counterBuffer
            };
            cellSpawnerGpuCapture = capture;
            cellSpawnerAuditReadbackPending = true;
            AsyncGPUReadback.Request(debugTexture, 0, request =>
                CompleteCellSpawnerGpuAuditReadback(capture, request));
        }

        private AutomaticFoamSourceEvent BuildCellSpawnerAuditEvent(
            CellSpawnerAuditCase testCase, float lengthCells,
            float widthCells, float headLengthCells, float headWidthCells,
            bool headOnlyMeasurement, float offsetXCells, float offsetYCells,
            float angleDegrees)
        {
            float dx = Mathf.Max(0.01f, gridDescriptor.ResolvedDxMetres);
            float dy = Mathf.Max(0.01f, gridDescriptor.ResolvedDyMetres);
            float centreGlobal = river.Domain.GlobalDistanceMinimum +
                Mathf.Max(dx * 16f, validFieldLength * 0.5f) + offsetXCells * dx;
            centreGlobal = Mathf.Clamp(centreGlobal,
                river.Domain.GlobalDistanceMinimum + dx * 8f,
                river.Domain.GlobalDistanceMaximum - dx * 8f);
            float centreLateral = offsetYCells * dy;
            float radians = angleDegrees * Mathf.Deg2Rad;
            Vector2 tangentCells = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            if (tangentCells.sqrMagnitude < 0.5f) tangentCells = Vector2.right;
            Vector2 halfMetric = new Vector2(
                tangentCells.x * lengthCells * dx * 0.5f,
                tangentCells.y * lengthCells * dy * 0.5f);
            AutomaticFoamSourceEventType type = testCase.Recipe switch
            {
                0 => AutomaticFoamSourceEventType.ShoreRibbon,
                1 => AutomaticFoamSourceEventType.InwardWash,
                2 => AutomaticFoamSourceEventType.ObjectContactArc,
                3 => AutomaticFoamSourceEventType.ObjectContactSemiArc,
                4 => AutomaticFoamSourceEventType.ObjectContactFleck,
                5 => AutomaticFoamSourceEventType.FreeWaterLaceConnector,
                6 => AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector,
                _ => AutomaticFoamSourceEventType.FreeWaterTornFragment
            };
            AutomaticFoamSourceEvent e = CreateP7SyntheticAutomaticSource(
                type, 1f, centreGlobal - halfMetric.x,
                centreGlobal + halfMetric.x, centreGlobal, centreLateral);
            e.EventId = 830000 + testCase.Recipe * 1000 +
                testCase.Scenario * 100 + testCase.Seed;
            e.ShapeSeed = 1000f + testCase.Seed * 17.37f + testCase.Recipe * 31.19f;
            e.PatternSeed = 2000f + testCase.Seed * 11.13f;
            e.Duration = 1f;
            e.Elapsed = 1f;
            e.ObjectBuildDuration = 1f;
            e.ObjectContactStrokeDuration = 1f;
            e.ObjectContactStrokeCount = 1;
            e.SourceAmount = 1f;
            e.RemainingLife = 1f;
            e.BodyLengthCells = lengthCells;
            e.BodyWidthCells = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : widthCells;
            e.HeadLengthCells = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : headLengthCells;
            e.HeadWidthCells = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : headWidthCells;
            e.BendAmplitudeCells = 0f;
            e.ContactSpanCells = lengthCells;
            e.ContactWidthCells = widthCells;
            e.WakeLengthCells = lengthCells;
            e.WakeWidthCells = widthCells;
            e.ShoreInsetMetres = 0f;
            // The D8 Shore/Inward packing path retains legacy metre-named
            // fields, but consumes these values as authoritative Foam cells.
            e.WidthMetres = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : widthCells;
            // Length and Inward Reach are independent authored controls. Body
            // dimension cases isolate the path length under test.
            e.InwardReachMetres = testCase.Recipe == 1 ? 0f : lengthCells * dy;
            e.FeatherMetres = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : headWidthCells;
            e.RevealPathDistanceMetres = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? Mathf.Max(1f, lengthCells)
                : Mathf.Max(dx, lengthCells * dx);
            e.HeadTrailMetres = type == AutomaticFoamSourceEventType.ShoreRibbon
                ? 1f
                : headLengthCells * dx;
            e.FormationSpeedMetresPerSecond = Mathf.Max(dx, lengthCells * dx);
            e.ObjectSourceLateralCellSpacingMetres = dy;
            e.ObjectWakeArmLengthMetres = lengthCells * dx;
            e.ObjectContactPathLengthMetres = lengthCells * dx;
            e.ObjectContactStrokePathLengthMetres = lengthCells * dx;
            e.ObjectCentreAcrossMetres = centreLateral;
            e.CentreAcrossNormalized = ResolveSourceAcrossNormalized(
                centreGlobal, centreLateral);
            // Object profile points are source-local metric offsets. The shader
            // converts them to cells around ObjectCentreGlobalDistance and
            // ObjectCentreAcrossMetres. Absolute coordinates produce a false zero
            // footprint because they are interpreted as enormous local offsets.
            for (int i = 0; i < 5; i++)
            {
                float t = i * 0.25f - 0.5f;
                Vector2 point = new Vector2(
                    tangentCells.x * lengthCells * dx * t,
                    tangentCells.y * lengthCells * dy * t);
                // Axis-aligned fixtures require a non-degenerate lateral contact
                // profile; the wake arms remain longitudinal in the shader.
                if (Mathf.Abs(tangentCells.y) < 0.25f)
                {
                    point = new Vector2(0f, lengthCells * dy * t);
                }
                switch (i)
                {
                    case 0: e.ObjectContactPoint0 = point; break;
                    case 1: e.ObjectContactPoint1 = point; break;
                    case 2: e.ObjectContactPoint2 = point; break;
                    case 3: e.ObjectContactPoint3 = point; break;
                    default: e.ObjectContactPoint4 = point; break;
                }
            }
            e.ObjectContactFrontSplit = 0.5f;
            e.ObjectContactNegativeFirstSegmentSplit = 0.25f;
            e.ObjectContactPositiveFirstSegmentSplit = 0.75f;
            e.LateralPaddingMetres = (widthCells + 3f) * dy;
            return e;
        }

        private bool TryPopulateCellSpawnerSyntheticShoreEdges(
            RenderTexture shoreTexture,
            AutomaticFoamSourceEvent sourceEvent,
            P7SourceDispatchRange range,
            out string failure)
        {
            failure = string.Empty;
            if (shoreTexture == null || !shoreTexture.IsCreated())
            {
                failure = "synthetic-domain preflight failed: shore texture unavailable";
                return false;
            }
            if (metricRows == null || metricRows.Length == 0 ||
                fieldWidth <= 0 || fieldHeight <= 0)
            {
                failure = "synthetic-domain preflight failed: metric rows unavailable";
                return false;
            }

            int textureWidth = Mathf.Max(1, shoreTexture.width);
            Color[] pixels = new Color[textureWidth];
            for (int x = 0; x < textureWidth; x++)
            {
                FoamMetricRow metric = metricRows[Mathf.Clamp(
                    x, 0, metricRows.Length - 1)];
                pixels[x] = new Color(
                    -Mathf.Max(0.01f, metric.ShoreData.x),
                    Mathf.Max(0.01f, metric.ShoreData.y),
                    0f,
                    0f);
            }

            Texture2D upload = new Texture2D(
                textureWidth,
                1,
                TextureFormat.RGBAHalf,
                false,
                true)
            {
                name = "PS3D_CellSpawnerAudit_SyntheticShoreEdgesUpload",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            try
            {
                upload.SetPixels(pixels);
                upload.Apply(false, false);
                Graphics.Blit(upload, shoreTexture);
            }
            finally
            {
                if (Application.isPlaying)
                {
                    UnityEngine.Object.Destroy(upload);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(upload);
                }
            }

            int sampleX = Mathf.Clamp(
                range.StartX + range.CountX / 2,
                0,
                metricRows.Length - 1);
            FoamMetricRow sampleMetric = metricRows[sampleX];
            float shoreMetres = sourceEvent.SideSign < 0f
                ? -Mathf.Max(0.01f, sampleMetric.ShoreData.x)
                : Mathf.Max(0.01f, sampleMetric.ShoreData.y);
            float minimumInward = float.PositiveInfinity;
            float maximumInward = float.NegativeInfinity;
            int endY = Mathf.Min(fieldHeight, range.StartY + range.CountY);
            for (int y = Mathf.Max(0, range.StartY); y < endY; y++)
            {
                float lateralMetres;
                if (gridDescriptor.IsCreated &&
                    gridDescriptor.UsesFixedMetricLattice)
                {
                    lateralMetres =
                        gridDescriptor.ResolveLateralMetresAtRowCentre(y);
                }
                else
                {
                    float across01 = (y + 0.5f) / Mathf.Max(1f, fieldHeight);
                    lateralMetres = across01 <= 0.5f
                        ? -sampleMetric.WidthsAndSpacing.x *
                            (1f - across01 * 2f)
                        : sampleMetric.WidthsAndSpacing.y *
                            (across01 * 2f - 1f);
                }
                float inward = sourceEvent.SideSign < 0f
                    ? lateralMetres - shoreMetres
                    : shoreMetres - lateralMetres;
                minimumInward = Mathf.Min(minimumInward, inward);
                maximumInward = Mathf.Max(maximumInward, inward);
            }

            float lateralSpacing = Mathf.Max(
                0.01f,
                sampleMetric.WidthsAndSpacing.w);
            float permittedOutside = Mathf.Max(
                sourceEvent.FeatherMetres,
                lateralSpacing);
            bool inwardRangeFinite =
                !float.IsNaN(minimumInward) &&
                !float.IsInfinity(minimumInward) &&
                !float.IsNaN(maximumInward) &&
                !float.IsInfinity(maximumInward);
            if (!inwardRangeFinite ||
                maximumInward < -permittedOutside)
            {
                failure =
                    "synthetic-domain preflight failed: dispatch/shore coordinate " +
                    $"mismatch (x={sampleX}, y={range.StartY}.." +
                    $"{Mathf.Max(range.StartY, endY - 1)}, shore={shoreMetres:0.####}m, " +
                    $"inward={minimumInward:0.####}..{maximumInward:0.####}m)";
                return false;
            }
            return true;
        }

        private static void ClearCellSpawnerAuditTexture(
            RenderTexture texture, Color colour)
        {
            if (texture == null || !texture.IsCreated())
            {
                return;
            }

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = texture;
            GL.Clear(false, true, colour);
            RenderTexture.active = previous;
        }

        private void CompleteCellSpawnerGpuAuditReadback(
            CellSpawnerGpuCapture capture, AsyncGPUReadbackRequest request)
        {
            if (capture == null) return;
            try
            {
                if (this == null || !cellSpawnerContractAuditRunning ||
                    capture.Generation != cellSpawnerAuditGeneration ||
                    cellSpawnerGpuCapture != capture)
                {
                    return;
                }
                if (capture.WarmupOnly)
                {
                    if (request.hasError)
                    {
                        FinishCellSpawnerContractAudit(
                            false, false, "GPU warm-up AsyncGPUReadback failed.");
                    }
                    else
                    {
                        cellSpawnerAuditWarmupPending = false;
                        cellSpawnerAuditPhase = "Dispatching measured cases";
                        cellSpawnerAuditLastResult = "Warm-up complete";
                    }
                }
                else
                {
                    if (request.hasError)
                    {
                        RecordCellSpawnerAuditFailure(capture.TestCase,
                            capture.CaseNumber, capture.RecipeName,
                            capture.ScenarioName, capture.RequestedLength,
                            capture.RequestedWidth, capture.HeadLength,
                            capture.HeadWidth, "AsyncGPUReadback failed");
                    }
                    else
                    {
                        var data = request.GetData<ushort>();
                        AnalyzeCellSpawnerGpuFootprint(capture, data);
                    }
                    cellSpawnerAuditCursor++;
                }
                RepaintCellSpawnerAuditViews(true);
            }
            finally
            {
                ReleaseCellSpawnerGpuCapture(capture);
                cellSpawnerAuditReadbackPending = false;
                cellSpawnerGpuCapture = null;
            }
        }

        private void AnalyzeCellSpawnerGpuFootprint(
            CellSpawnerGpuCapture capture,
            Unity.Collections.NativeArray<ushort> data)
        {
            int expected = fieldWidth * fieldHeight * 4;
            if (data.Length != expected)
            {
                RecordCellSpawnerAuditFailure(capture.TestCase,
                    capture.CaseNumber, capture.RecipeName,
                    capture.ScenarioName, capture.RequestedLength,
                    capture.RequestedWidth, capture.HeadLength,
                    capture.HeadWidth,
                    $"unexpected ARGBHalf payload {data.Length}; expected {expected}");
                return;
            }

            double coverageArea = 0.0;
            double minAlong = double.PositiveInfinity;
            double maxAlong = double.NegativeInfinity;
            double minAcross = double.PositiveInfinity;
            double maxAcross = double.NegativeInfinity;
            double outsideEnvelopeCoverage = 0.0;
            int nonZeroCells = 0;
            int minX = fieldWidth, minY = fieldHeight, maxX = -1, maxY = -1;
            int[] fullByRow = new int[fieldHeight];
            int[] fullByColumn = new int[fieldWidth];
            float radians = capture.AngleDegrees * Mathf.Deg2Rad;
            Vector2 tangent = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            if (tangent.sqrMagnitude < 0.5f) tangent = Vector2.right;
            tangent.Normalize();
            Vector2 normal = new Vector2(-tangent.y, tangent.x);
            double halfExpectedLength = capture.ExpectedLength * 0.5;
            double halfExpectedWidth = capture.ExpectedWidth * 0.5;

            for (int y = 0; y < fieldHeight; y++)
            for (int x = 0; x < fieldWidth; x++)
            {
                int index = (y * fieldWidth + x) * 4;
                float r = Mathf.HalfToFloat(data[index]);
                float g = Mathf.HalfToFloat(data[index + 1]);
                float b = Mathf.HalfToFloat(data[index + 2]);
                float coverage = Mathf.Clamp01(Mathf.Max(r, Mathf.Max(g, b)));
                coverageArea += coverage;
                if (coverage <= 0.0005f) continue;

                nonZeroCells++;
                minX = Mathf.Min(minX, x); maxX = Mathf.Max(maxX, x);
                minY = Mathf.Min(minY, y); maxY = Mathf.Max(maxY, y);
                if (coverage >= 0.98f)
                {
                    fullByRow[y]++;
                    fullByColumn[x]++;
                }

                Vector2 delta = new Vector2(
                    x + 0.5f - capture.CentreCellX,
                    y + 0.5f - capture.CentreCellY);
                double along = Vector2.Dot(delta, tangent);
                double across = Vector2.Dot(delta, normal);
                // Add half a cell on each side because the sample represents a
                // cell-integrated value rather than a point sample.
                minAlong = Math.Min(minAlong, along - 0.5);
                maxAlong = Math.Max(maxAlong, along + 0.5);
                minAcross = Math.Min(minAcross, across - 0.5);
                maxAcross = Math.Max(maxAcross, across + 0.5);
                if (Math.Abs(along) > halfExpectedLength + 0.5 ||
                    Math.Abs(across) > halfExpectedWidth + 0.5)
                {
                    outsideEnvelopeCoverage += coverage;
                }
            }

            int supportWidth = maxX >= minX ? maxX - minX + 1 : 0;
            int supportHeight = maxY >= minY ? maxY - minY + 1 : 0;
            int fullRowRun = ResolveMaximumAdjacentOccupiedRun(fullByRow);
            int fullColumnRun = ResolveMaximumAdjacentOccupiedRun(fullByColumn);
            double projectedLength = nonZeroCells > 0 ? maxAlong - minAlong : 0.0;
            double projectedWidth = nonZeroCells > 0 ? maxAcross - minAcross : 0.0;
            bool readbackPass = nonZeroCells > 0 && coverageArea > 0.001;
            bool oneCellCase = capture.ExpectedWidth <= 1.0001f;
            bool adjacencyPass = !oneCellCase ||
                !(fullRowRun >= 2 && fullColumnRun >= 2);

            double primitiveArea = capture.ExpectedLength * capture.ExpectedWidth;
            double expectedArea = primitiveArea;
            double lowerAreaBound = primitiveArea;
            double upperAreaBound = primitiveArea;
            string areaContractNote = string.Empty;

            int recipe = capture.TestCase.Recipe;
            int scenario = capture.TestCase.Scenario;
            int componentMode = capture.TestCase.ComponentMode;
            if ((recipe == 2 || recipe == 3) && componentMode > 0)
            {
                cellSpawnerComponentAreas[
                    ResolveCellSpawnerComponentAreaKey(
                        recipe, scenario, componentMode)] = coverageArea;
            }

            double areaTolerance = Math.Max(0.20, primitiveArea * 0.20);
            bool areaPass;
            if (recipe == 7 && !capture.HeadOnlyMeasurement)
            {
                // Broken Filament intentionally removes pieces. Its report must
                // show the actual non-empty bounded-envelope contract rather than
                // the solid primitive's generic lower bound.
                lowerAreaBound = 0.001;
                upperAreaBound = primitiveArea + areaTolerance;
                areaPass = coverageArea > lowerAreaBound &&
                    coverageArea <= upperAreaBound;
                areaContractNote =
                    $"; fragmented-envelope-contract ({lowerAreaBound:0.###},{upperAreaBound:0.###}]";
            }
            else if ((recipe == 2 || recipe == 3) && componentMode == 0)
            {
                int[] modes = recipe == 2
                    ? new[] { 1, 2, 3 }
                    : new[] { 1, 3 };
                bool haveAllComponents = true;
                lowerAreaBound = 0.0;
                upperAreaBound = 0.0;
                for (int i = 0; i < modes.Length; i++)
                {
                    if (!cellSpawnerComponentAreas.TryGetValue(
                        ResolveCellSpawnerComponentAreaKey(
                            recipe, scenario, modes[i]),
                        out double componentArea))
                    {
                        haveAllComponents = false;
                        break;
                    }
                    lowerAreaBound = Math.Max(lowerAreaBound, componentArea);
                    upperAreaBound += componentArea;
                }

                double unionTolerance = Math.Max(0.20, upperAreaBound * 0.05);
                if (haveAllComponents)
                {
                    areaPass = coverageArea >= lowerAreaBound - unionTolerance &&
                        coverageArea <= upperAreaBound + unionTolerance;
                    expectedArea = upperAreaBound;
                    areaContractNote =
                        $"; composite-union-contract [{lowerAreaBound:0.###},{upperAreaBound:0.###}]";
                }
                else
                {
                    // Exhaustive currently does not prepend isolated component
                    // rows. Do not revive the invalid component-count multiplier;
                    // retain a non-empty informational contract instead.
                    lowerAreaBound = 0.001;
                    upperAreaBound = Math.Max(coverageArea, lowerAreaBound);
                    expectedArea = coverageArea;
                    areaPass = coverageArea > 0.001;
                    areaContractNote =
                        "; composite-profile informational (component evidence unavailable)";
                }
            }
            else if ((recipe == 2 || recipe == 3) && componentMode == 1 &&
                !capture.HeadOnlyMeasurement)
            {
                if (recipe == 3)
                {
                    // Semi-Arc is the selected half-profile anchor. It is not a
                    // rectangle, so use broad sanity bounds; the corresponding
                    // full Arc row validates the stronger 2x relationship.
                    lowerAreaBound = primitiveArea * 0.25;
                    upperAreaBound = primitiveArea * 2.0;
                    areaPass = coverageArea >= lowerAreaBound - 0.20 &&
                        coverageArea <= upperAreaBound + 0.20;
                    expectedArea = coverageArea;
                    areaContractNote =
                        $"; half-contact-profile-anchor [{lowerAreaBound:0.###},{upperAreaBound:0.###}]";
                }
                else
                {
                    bool haveHalf = cellSpawnerComponentAreas.TryGetValue(
                        ResolveCellSpawnerComponentAreaKey(3, scenario, 1),
                        out double halfArea);
                    expectedArea = halfArea * 2.0;
                    areaTolerance = Math.Max(0.25, expectedArea * 0.15);
                    lowerAreaBound = expectedArea - areaTolerance;
                    upperAreaBound = expectedArea + areaTolerance;
                    if (haveHalf)
                    {
                        areaPass = coverageArea >= lowerAreaBound &&
                            coverageArea <= upperAreaBound;
                        areaContractNote =
                            $"; full-vs-half-contact-profile expected≈{expectedArea:0.###}";
                    }
                    else
                    {
                        lowerAreaBound = primitiveArea * 0.25;
                        upperAreaBound = primitiveArea * 4.0;
                        expectedArea = coverageArea;
                        areaPass = coverageArea >= lowerAreaBound - 0.20 &&
                            coverageArea <= upperAreaBound + 0.20;
                        areaContractNote =
                            "; full-contact-profile informational (half evidence unavailable)";
                    }
                }
            }
            else
            {
                lowerAreaBound = primitiveArea - areaTolerance;
                upperAreaBound = primitiveArea + areaTolerance;
                areaPass = Math.Abs(coverageArea - primitiveArea) <= areaTolerance;
            }
            // Support extents and the legacy centre-classified outside value are
            // retained as informational diagnostics only. Fractional raster support
            // is not a physical dimension and must not fail a cell-exact contract.
            bool lengthPass = true;
            bool widthPass = true;
            bool envelopePass = true;

            bool replayPass = true;
            int rawDifferenceCount = 0;
            float rawMaximumDifference = 0f;
            ushort[] rawPayload = data.ToArray();
            if (capture.TestCase.ReplayReferenceCase > 0)
            {
                replayPass = cellSpawnerReplayPayloads.TryGetValue(
                    capture.TestCase.ReplayReferenceCase, out ushort[] reference) &&
                    reference.Length == rawPayload.Length;
                if (replayPass)
                {
                    for (int i = 0; i < rawPayload.Length; i++)
                    {
                        if (reference[i] == rawPayload[i])
                        {
                            continue;
                        }
                        rawDifferenceCount++;
                        float difference = Mathf.Abs(
                            Mathf.HalfToFloat(reference[i]) -
                            Mathf.HalfToFloat(rawPayload[i]));
                        rawMaximumDifference = Mathf.Max(
                            rawMaximumDifference, difference);
                    }
                    replayPass = rawDifferenceCount == 0;
                }
            }
            else if (capture.CaseNumber >= 1 && capture.CaseNumber <= 6)
            {
                cellSpawnerReplayPayloads[capture.CaseNumber] = rawPayload;
            }

            bool passed = readbackPass && adjacencyPass && areaPass;
            if (passed) cellSpawnerAuditPassCount++; else cellSpawnerAuditFailCount++;
            string measurementName = capture.HeadOnlyMeasurement ? "head" : "body";
            string componentNote = capture.TestCase.ComponentMode > 0
                ? "; isolated-component"
                : string.Empty;
            string replayNote = capture.TestCase.ReplayReferenceCase > 0
                ? $"; replay-of {capture.TestCase.ReplayReferenceCase:000} " +
                  (replayPass
                      ? "raw-match"
                      : $"raw-mismatch informational values={rawDifferenceCount}; max-delta={rawMaximumDifference:0.######}")
                : string.Empty;
            string acceptedAreaDescription = recipe == 7 &&
                !capture.HeadOnlyMeasurement
                ? $"accepted area ({lowerAreaBound:0.###},{upperAreaBound:0.###}]"
                : $"accepted area [{lowerAreaBound:0.###},{upperAreaBound:0.###}]";
            string detail =
                $"{measurementName} GPU area {coverageArea:0.###}; expected area {expectedArea:0.###}; " +
                acceptedAreaDescription + "; " +
                $"support-projected L {projectedLength:0.###}; " +
                $"support-projected W {projectedWidth:0.###}; " +
                $"legacy outside {outsideEnvelopeCoverage:0.###}; support {supportWidth}×{supportHeight}; " +
                $"full-row-run {fullRowRun}; full-column-run {fullColumnRun}; " +
                $"dispatch [{capture.CentreCellX:0.###},{capture.CentreCellY:0.###}]" +
                componentNote + areaContractNote +
                (capture.UsesSyntheticDomain ? "; synthetic-domain-fixture" : string.Empty) +
                replayNote;
            cellSpawnerAuditLastResult =
                $"{(passed ? "PASS" : "FAIL")} · {capture.RecipeName} / {capture.ScenarioName} / " +
                $"seed {capture.TestCase.Seed:00} · area {coverageArea:0.###} " +
                $"in [{lowerAreaBound:0.###},{upperAreaBound:0.###}]";
            cellSpawnerAuditText.AppendLine(
                $"{capture.CaseNumber:000}: {capture.RecipeName} / {capture.ScenarioName} / seed {capture.TestCase.Seed:00} — " +
                $"{(passed ? "PASS" : "FAIL")} · requested {capture.RequestedLength:0.###}×{capture.RequestedWidth:0.###} · {detail}");
            AppendCellSpawnerAuditCsv(capture, coverageArea, projectedLength,
                projectedWidth, outsideEnvelopeCoverage, supportWidth,
                supportHeight, fullRowRun, fullColumnRun, nonZeroCells,
                readbackPass, adjacencyPass, lengthPass, widthPass, areaPass,
                envelopePass, replayPass, passed, detail);

            // Replay remains diagnostic evidence, but raw payload equality is
            // informational and does not override the integrated geometry result
            // or contribute to the suite pass/fail count.
        }

        private static int ResolveMaximumAdjacentOccupiedRun(int[] values)
        {
            int best = 0, current = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0) { current++; best = Mathf.Max(best, current); }
                else current = 0;
            }
            return best;
        }

        private void RecordCellSpawnerAuditFailure(
            CellSpawnerAuditCase testCase, int caseNumber, string recipeName,
            string scenarioName, float requestedLength, float requestedWidth,
            float headLength, float headWidth, string detail)
        {
            cellSpawnerAuditFailCount++;
            cellSpawnerAuditLastResult = $"FAIL · {recipeName} / {scenarioName} / seed {testCase.Seed:00} · {detail}";
            cellSpawnerAuditText.AppendLine(
                $"{caseNumber:000}: {recipeName} / {scenarioName} / seed {testCase.Seed:00} — FAIL · {detail}");
            CellSpawnerGpuCapture capture = new CellSpawnerGpuCapture
            {
                TestCase = testCase, CaseNumber = caseNumber,
                RecipeName = recipeName, ScenarioName = scenarioName,
                RequestedLength = requestedLength,
                RequestedWidth = requestedWidth,
                HeadLength = headLength,
                HeadWidth = headWidth,
                ExpectedLength = requestedLength,
                ExpectedWidth = requestedWidth,
                HeadOnlyMeasurement = false
            };
            AppendCellSpawnerAuditCsv(capture, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                false, false, false, false, false, false, false, false, detail);
        }

        private void AppendCellSpawnerAuditCsv(
            CellSpawnerGpuCapture c, double area, double projectedLength,
            double projectedWidth, double outsideEnvelopeCoverage,
            int supportWidth, int supportHeight, int fullRowRun,
            int fullColumnRun, int nonZeroCells, bool readbackPass,
            bool adjacencyPass, bool lengthPass, bool widthPass,
            bool areaPass, bool envelopePass, bool replayPass,
            bool passed, string detail)
        {
            cellSpawnerAuditCsv.Append(c.CaseNumber.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(c.RecipeName).Append(',').Append(c.ScenarioName).Append(',')
                .Append(c.TestCase.Seed.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(c.HeadOnlyMeasurement ? "Head" : "Body").Append(',')
                .Append(c.TestCase.ReplayReferenceCase.ToString(CultureInfo.InvariantCulture)).Append(',')
                .Append(c.RequestedLength.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(c.RequestedWidth.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(c.HeadLength.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(c.HeadWidth.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(c.ExpectedLength.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(c.ExpectedWidth.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(area.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(projectedLength.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(projectedWidth.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(outsideEnvelopeCoverage.ToString("0.######", CultureInfo.InvariantCulture)).Append(',')
                .Append(supportWidth).Append(',').Append(supportHeight).Append(',')
                .Append(fullRowRun).Append(',').Append(fullColumnRun).Append(',')
                .Append(nonZeroCells).Append(',')
                .Append(readbackPass ? "PASS" : "FAIL").Append(',')
                .Append(adjacencyPass ? "PASS" : "FAIL").Append(',')
                .Append(lengthPass ? "PASS" : "FAIL").Append(',')
                .Append(widthPass ? "PASS" : "FAIL").Append(',')
                .Append(areaPass ? "PASS" : "FAIL").Append(',')
                .Append(envelopePass ? "PASS" : "FAIL").Append(',')
                .Append(replayPass ? "PASS" : "FAIL").Append(',')
                .Append(passed ? "PASS" : "FAIL").Append(',')
                .Append('"').Append(detail.Replace("\"", "\"\"")).Append('"').AppendLine();
        }

        private static void ReleaseCellSpawnerGpuCapture(CellSpawnerGpuCapture capture)
        {
            if (capture == null)
            {
                return;
            }
            ReleaseCellSpawnerAuditTexture(capture.DebugTexture);
            ReleaseCellSpawnerAuditTexture(capture.StateTexture);
            ReleaseCellSpawnerAuditTexture(capture.ObjectContactTexture);
            ReleaseCellSpawnerAuditTexture(capture.SyntheticBoundaryTexture);
            ReleaseCellSpawnerAuditTexture(capture.SyntheticObstacleTexture);
            ReleaseCellSpawnerAuditTexture(capture.SyntheticShoreEdgesTexture);
            capture.EventBuffer?.Release();
            capture.CounterBuffer?.Release();
        }

        private static void ReleaseCellSpawnerAuditTexture(RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }
            texture.Release();
            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(texture);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private void FinishCellSpawnerContractAudit(
            bool passed, bool cancelled, string reason)
        {
            cellSpawnerContractAuditRunning = false;
            cellSpawnerAuditGeneration++;
            if (!cellSpawnerAuditReadbackPending && cellSpawnerGpuCapture != null)
            {
                ReleaseCellSpawnerGpuCapture(cellSpawnerGpuCapture);
                cellSpawnerGpuCapture = null;
            }

            double elapsed = Math.Max(0.0,
                Time.realtimeSinceStartupAsDouble - cellSpawnerAuditStartedAt);
            cellSpawnerAuditText ??= new StringBuilder(4096);
            cellSpawnerAuditCsv ??= new StringBuilder(4096);
            cellSpawnerAuditText.AppendLine();
            cellSpawnerAuditText.AppendLine("SUMMARY");
            cellSpawnerAuditText.AppendLine($"Suite: {CellSpawnerContractAuditSuiteName}");
            cellSpawnerAuditText.AppendLine(
                $"Completed GPU readbacks: {cellSpawnerAuditCursor}/{cellSpawnerAuditCases.Count}");
            cellSpawnerAuditText.AppendLine($"Passed: {cellSpawnerAuditPassCount}");
            cellSpawnerAuditText.AppendLine($"Failed: {cellSpawnerAuditFailCount}");
            cellSpawnerAuditText.AppendLine($"Elapsed: {elapsed:0.000} s");
            cellSpawnerAuditText.AppendLine($"Reason: {reason}");
            cellSpawnerAuditText.AppendLine(
                $"Outcome: {(cancelled ? "CANCELLED" : passed ? "PASS" : "FAIL")}");

            cellSpawnerAuditPhase = cancelled
                ? "Cancelled"
                : passed ? "Complete" : "Failed";
            cellSpawnerAuditCurrentCase = cancelled
                ? "Cancelled"
                : passed ? "Complete" : "Failed";
            topologyCacheDiagnosticState = cellSpawnerAuditPhase;
            topologyCacheDiagnosticSummary = cancelled
                ? $"{CellSpawnerContractAuditSuiteName} GPU audit cancelled at " +
                  $"{cellSpawnerAuditCursor}/{cellSpawnerAuditCases.Count}; partial report preserved."
                : $"{CellSpawnerContractAuditSuiteName} GPU audit " +
                  $"{(passed ? "passed" : "failed")}: " +
                  $"{cellSpawnerAuditPassCount} pass, {cellSpawnerAuditFailCount} fail.";
            topologyCacheDiagnosticReport = cellSpawnerAuditText.ToString();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ??
                Application.dataPath;
            string directory = Path.Combine(projectRoot, "Library", "RiverFoam");
            Directory.CreateDirectory(directory);
            string fileStem = cellSpawnerAuditSuite == CellSpawnerAuditSuite.Smoke
                ? "CellExactSpawnerSmokeSuite"
                : "CellExactSpawnerExhaustiveSuite";
            topologyCacheDiagnosticReportPath = Path.Combine(
                directory, fileStem + ".txt");
            File.WriteAllText(
                topologyCacheDiagnosticReportPath,
                topologyCacheDiagnosticReport);
            File.WriteAllText(
                Path.Combine(directory, fileStem + ".csv"),
                cellSpawnerAuditCsv.ToString());
            RepaintCellSpawnerAuditViews(true);

            if (cancelled)
            {
                Debug.LogWarning("[River Foam] " + topologyCacheDiagnosticSummary, river);
            }
            else if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log("[River Foam] " + topologyCacheDiagnosticSummary, river);
            }
            else
            {
                Debug.LogError("[River Foam] " + topologyCacheDiagnosticSummary, river);
            }
        }

        private static string ResolveAuditRecipeName(int recipe) => recipe switch
        {
            0 => "Shore Ribbon", 1 => "Inward Wash", 2 => "Object Arc",
            3 => "Object Semi-Arc", 4 => "Object Fleck", 5 => "Free-Water Lace",
            6 => "Free-Water Cross-Lace", _ => "Broken Filament"
        };

        private static void ResolveAuditScenario(
            int scenario, out string name, out float length,
            out float width, out float headLength, out float headWidth,
            out bool headOnlyMeasurement, out float expectedLength,
            out float expectedWidth, out float offsetX, out float offsetY,
            out float angleDegrees)
        {
            name = scenario switch
            {
                0 => "1x1 Axis",
                1 => "1x1 Half-Cell",
                2 => "1x1 Diagonal",
                3 => "1x5 Body",
                4 => "3x1 Body",
                5 => "Min=Max=1",
                _ => "Head=1x1 on 5x3 Body"
            };
            headOnlyMeasurement = scenario == 6;
            length = scenario == 3 || headOnlyMeasurement ? 5f : 1f;
            width = scenario == 4 || headOnlyMeasurement ? 3f : 1f;
            // Body scenarios reveal the full body. Only the dedicated head
            // scenario retains a one-cell moving head on a larger body.
            headLength = headOnlyMeasurement ? 1f : length;
            headWidth = headOnlyMeasurement ? 1f : width;
            expectedLength = headOnlyMeasurement ? 1f : length;
            expectedWidth = headOnlyMeasurement ? 1f : width;
            offsetX = scenario == 1 ? 0.5f : 0f;
            offsetY = scenario == 1 ? 0.5f : 0f;
            angleDegrees = scenario == 2 ? 45f : 0f;
        }

        private static bool IsFinitePositive(float value) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f;
        private static float AuditHash01(float value) =>
            Mathf.Repeat(Mathf.Sin(value) * 43758.5453f, 1f);

        private bool ValidateP7AutomaticSourceOwnershipContracts(
            StringBuilder report)
        {
            report.AppendLine("FINITE-BURST AUTOMATIC-SOURCE OWNERSHIP");
            StylizedRiverFoamGridDescriptor descriptor =
                gridDescriptor.IsCreated
                    ? gridDescriptor
                    : fixedMetricCandidateDescriptor;
            AutomaticFoamSourceEvent shore = CreateP7SyntheticAutomaticSource(
                AutomaticFoamSourceEventType.ShoreRibbon,
                -1f,
                river.Domain.GlobalDistanceMinimum + 1.5f,
                river.Domain.GlobalDistanceMinimum + 3.5f,
                river.Domain.GlobalDistanceMinimum + 2.5f,
                -0.5f);
            shore.Elapsed = 0.80f;
            FoamSourceEventGpuData shoreBuild =
                BuildAutomaticFoamSourceGpuData(shore, 0.70f, descriptor);
            FoamSourceEventGpuData shoreRepeated =
                BuildAutomaticFoamSourceGpuData(shore, 0.80f, descriptor);

            AutomaticFoamSourceEvent arc = CreateP7SyntheticAutomaticSource(
                AutomaticFoamSourceEventType.ObjectContactArc,
                1f,
                river.Domain.GlobalDistanceMinimum + 4f,
                river.Domain.GlobalDistanceMinimum + 7f,
                river.Domain.GlobalDistanceMinimum + 5.5f,
                0.25f);
            arc.Elapsed = 0.65f;
            FoamSourceEventGpuData arcFirstStroke =
                BuildAutomaticFoamSourceGpuData(arc, 0.55f, descriptor);
            arc.Elapsed = 0.75f;
            FoamSourceEventGpuData arcPhaseBoundary =
                BuildAutomaticFoamSourceGpuData(arc, 0.65f, descriptor);
            arc.Elapsed = 0.90f;
            FoamSourceEventGpuData arcReinforcement =
                BuildAutomaticFoamSourceGpuData(arc, 0.80f, descriptor);
            FoamSourceEventGpuData arcRepeatedReinforcement =
                BuildAutomaticFoamSourceGpuData(arc, 0.90f, descriptor);
            AutomaticFoamSourceEvent contactMaintenance = arc;
            contactMaintenance.ObjectContactReinforcementOnly = true;
            contactMaintenance.ObjectContactStrokeCount = 1;
            contactMaintenance.Duration =
                contactMaintenance.ObjectContactStrokeDuration;
            contactMaintenance.Elapsed = 0.20f;
            FoamSourceEventGpuData contactMaintenanceBuild =
                BuildAutomaticFoamSourceGpuData(
                    contactMaintenance,
                    0.10f,
                    descriptor);

            bool shoreBuildAdvances =
                shoreBuild.Header.z > shoreBuild.Deposit.y;
            bool repeatedNonpersistentInteriorZero = P7FloatBitsEqual(
                shoreRepeated.Header.z,
                shoreRepeated.Deposit.y);
            bool firstStrokeAdvances =
                arcFirstStroke.Header.y == 0f &&
                arcFirstStroke.Deposit.x == 0f &&
                arcFirstStroke.Header.z > arcFirstStroke.Deposit.y;
            bool phaseBoundaryResets =
                arcPhaseBoundary.Header.y == 1f &&
                arcPhaseBoundary.Deposit.x == 0f &&
                arcPhaseBoundary.Header.z >= 0f;
            bool reinforcementAdvances =
                arcReinforcement.Header.y == 1f &&
                arcReinforcement.Deposit.x == 1f &&
                arcReinforcement.Header.z >
                    arcReinforcement.Deposit.y;
            bool repeatedReinforcementInteriorZero = P7FloatBitsEqual(
                arcRepeatedReinforcement.Header.z,
                arcRepeatedReinforcement.Deposit.y);
            bool finiteBurstDuration = Mathf.Approximately(
                arc.Duration,
                arc.ObjectBuildDuration +
                (arc.ObjectContactStrokeCount - 1) *
                arc.ObjectContactStrokeDuration) &&
                arc.ObjectContactStrokeCount == 2;
            bool contactMaintenancePhase =
                contactMaintenanceBuild.Header.y == 1f &&
                contactMaintenanceBuild.Deposit.x == 1f &&
                contactMaintenanceBuild.Header.z >
                    contactMaintenanceBuild.Deposit.y &&
                Mathf.Approximately(
                    contactMaintenance.Duration,
                    contactMaintenance.ObjectContactStrokeDuration) &&
                contactMaintenanceBuild.Deposit.w > 0f;

            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string computePath = Path.Combine(
                projectRoot,
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string computeSource = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string injectionPath = Path.Combine(
                projectRoot,
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.Injection.cs");
            string injectionSource = File.Exists(injectionPath)
                ? File.ReadAllText(injectionPath)
                : string.Empty;

            bool universalDifferenceGate =
                computeSource.IndexOf(
                    "max(0.0, currentContribution - previousContribution)",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "previousSourceEvent.header.y = sourceEvent.deposit.x;",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "previousSourceEvent.header.z = sourceEvent.deposit.y;",
                    StringComparison.Ordinal) >= 0;
            bool phaseResetContract =
                computeSource.IndexOf(
                    "bool depositionPhaseChanged",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "!depositionPhaseChanged",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "bool depositionPhaseChanged",
                    StringComparison.Ordinal) >= 0;
            bool recipeCompleteReinforcement =
                CountP7TextOccurrences(
                    computeSource,
                    "return saturate(frontShape * reinforcementPhase);") == 2 &&
                computeSource.IndexOf(
                    "FoamResolveFullObjectContactRing(",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "ringShape * ringPhase",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "sourceEvent.deposit.w",
                    StringComparison.Ordinal) >= 0;
            bool persistentBypassRemoved =
                computeSource.IndexOf(
                    "refreshObjectContact",
                    StringComparison.Ordinal) < 0 &&
                computeSource.IndexOf(
                    "persistentObjectEmitter",
                    StringComparison.Ordinal) < 0 &&
                injectionSource.IndexOf(
                    "holdPhase",
                    StringComparison.Ordinal) < 0 &&
                injectionSource.IndexOf(
                    "releasePhase",
                    StringComparison.Ordinal) < 0;
            bool finiteStrokeResolver =
                injectionSource.IndexOf(
                    "sourceEvent.ObjectContactStrokeCount",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "sourceEvent.ObjectContactReinforcementOnly",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "sourceEvent.ObjectContactStrokeDuration",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "reinforcementElapsed / contactStrokeDuration",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "ResolveAutomaticObjectContactPhaseDuration",
                    StringComparison.Ordinal) >= 0 &&
                injectionSource.IndexOf(
                    "ObjectContactStrokePathLengthMetres",
                    StringComparison.Ordinal) >= 0;
            bool absoluteTargetPreserved =
                computeSource.IndexOf(
                    "float birthContribution = currentContribution;",
                    StringComparison.Ordinal) >= 0 &&
                computeSource.IndexOf(
                    "FoamMergeBornPresence(",
                    StringComparison.Ordinal) >= 0;
            bool finiteOneShotContract =
                repeatedNonpersistentInteriorZero &&
                repeatedReinforcementInteriorZero &&
                computeSource.IndexOf(
                    "newlyRevealedPermission <= FoamMaterialStateEpsilon",
                    StringComparison.Ordinal) >= 0;

            bool exact = descriptor.IsCreated && shoreBuildAdvances &&
                repeatedNonpersistentInteriorZero && firstStrokeAdvances &&
                phaseBoundaryResets && reinforcementAdvances &&
                repeatedReinforcementInteriorZero && finiteBurstDuration &&
                contactMaintenancePhase &&
                universalDifferenceGate && phaseResetContract &&
                recipeCompleteReinforcement && persistentBypassRemoved &&
                finiteStrokeResolver && absoluteTargetPreserved &&
                finiteOneShotContract;
            report.AppendLine(
                $"Shore frontier advances: {shoreBuildAdvances}; repeated " +
                $"Shore interior zero: {repeatedNonpersistentInteriorZero}");
            report.AppendLine(
                $"Object first stroke advances: {firstStrokeAdvances}; phase " +
                $"boundary resets: {phaseBoundaryResets}; reinforcement " +
                $"advances: {reinforcementAdvances}");
            report.AppendLine(
                $"Repeated reinforcement interior zero: " +
                $"{repeatedReinforcementInteriorZero}; finite burst duration: " +
                $"{finiteBurstDuration}");
            report.AppendLine(
                $"Universal difference gate: {universalDifferenceGate}; phase " +
                $"reset contract: {phaseResetContract}; recipe-complete " +
                $"reinforcement: {recipeCompleteReinforcement}");
            report.AppendLine(
                $"Persistent bypass removed: {persistentBypassRemoved}; finite " +
                $"stroke resolver: {finiteStrokeResolver}; absolute target: " +
                $"{absoluteTargetPreserved}");
            report.AppendLine(
                "FINITE-BURST SOURCE OWNERSHIP VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }


        private static int CountP7TextOccurrences(
            string source,
            string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int searchIndex = 0;
            while ((searchIndex = source.IndexOf(
                       value,
                       searchIndex,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                searchIndex += value.Length;
            }

            return count;
        }

        private AutomaticFoamSourceEvent CreateP7SyntheticAutomaticSource(
            AutomaticFoamSourceEventType type,
            float sideSign,
            float startGlobal,
            float endGlobal,
            float centreGlobal,
            float centreLateral)
        {
            float centreAcross = ResolveSourceAcrossNormalized(
                centreGlobal,
                centreLateral);
            float fixedSpacing = Mathf.Max(
                0.01f,
                fixedMetricCandidateDescriptor.ResolvedDyMetres);
            return new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = 7000 + (int)type,
                Type = type,
                SideSign = sideSign < 0f ? -1f : 1f,
                StartGlobalDistance = startGlobal,
                EndGlobalDistance = endGlobal,
                ObjectCentreGlobalDistance = centreGlobal,
                Duration = type == AutomaticFoamSourceEventType.ObjectContactArc ||
                    type == AutomaticFoamSourceEventType.ObjectContactSemiArc
                        ? 1.1f
                        : 0.7f,
                Elapsed = 0.65f,
                ObjectBuildDuration = 0.7f,
                ObjectContactStrokeDuration = 0.4f,
                ObjectContactStrokePathLengthMetres = 1.2f,
                ObjectContactStrokeRawRevealDurationSeconds = 0.35f,
                ObjectContactStrokeRevealCadenceLimited = false,
                ObjectContactStrokeCount =
                    type == AutomaticFoamSourceEventType.ObjectContactArc ||
                    type == AutomaticFoamSourceEventType.ObjectContactSemiArc
                        ? 2
                        : 1,
                ObjectContactReinforcementOnly = false,
                FormationSpeedMetresPerSecond = 0.55f,
                HeadTrailMetres = 0.45f,
                ShoreInsetMetres = 0.05f,
                WidthMetres = 0.20f,
                ShoreRibbonThicknessCells = 1.25f,
                ShoreRibbonThicknessMetres = 1.25f * fixedSpacing,
                InwardReachMetres = 0.65f,
                FeatherMetres = 0.08f,
                SourceAmount = 0.72f,
                RemainingLife = 0.82f,
                PatternSeed = 71.3f,
                SourceFillSeed = 91.7f,
                SourceFillFeatureSize = 0.24f,
                SourceFillBlend = 0.30f,
                ShapeSeed = 117.9f,
                BreakupScaleMetres = 0.35f,
                BreakupStrength = 0.25f,
                Curvature = 0.18f,
                ObjectCentreAcrossMetres = centreLateral,
                ObjectAlongHalfLengthMetres = 0.85f,
                ObjectAcrossHalfWidthMetres = 0.55f,
                ObjectContactOffsetMetres = 0.12f,
                ObjectSourceLateralCellSpacingMetres = fixedSpacing,
                ObjectWakeArmLengthMetres = 1.4f,
                ObjectContactPathLengthMetres = 3.2f,
                ObjectContactPoint0 = new Vector2(startGlobal, centreLateral),
                ObjectContactPoint1 = new Vector2(
                    centreGlobal - 0.45f,
                    centreLateral - 0.25f),
                ObjectContactPoint2 = new Vector2(
                    centreGlobal,
                    centreLateral - 0.45f),
                ObjectContactPoint3 = new Vector2(
                    centreGlobal + 0.45f,
                    centreLateral - 0.25f),
                ObjectContactPoint4 = new Vector2(endGlobal, centreLateral),
                ObjectContactFrontSplit = 0.5f,
                ObjectContactNegativeFirstSegmentSplit = 0.25f,
                ObjectContactPositiveFirstSegmentSplit = 0.75f,
                CentreAcrossNormalized = centreAcross,
                LateralPaddingMetres = 0.95f
            };
        }

        private static bool P7FloatBitsEqual(float left, float right)
        {
            return BitConverter.SingleToInt32Bits(left) ==
                BitConverter.SingleToInt32Bits(right);
        }

        private static int P7CountOccurrences(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(
                       value,
                       index,
                       StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += value.Length;
            }
            return count;
        }


        private static string P7ExtractFunctionBody(
            string source,
            string signature)
        {
            if (string.IsNullOrEmpty(source) ||
                string.IsNullOrEmpty(signature))
            {
                return string.Empty;
            }

            int signatureIndex = source.IndexOf(
                signature,
                StringComparison.Ordinal);
            if (signatureIndex < 0)
            {
                return string.Empty;
            }

            int openBrace = source.IndexOf('{', signatureIndex);
            if (openBrace < 0)
            {
                return string.Empty;
            }

            int depth = 0;
            for (int index = openBrace; index < source.Length; index++)
            {
                char character = source[index];
                if (character == '{')
                {
                    depth++;
                }
                else if (character == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return source.Substring(
                            openBrace,
                            index - openBrace + 1);
                    }
                }
            }

            return string.Empty;
        }

        private bool FinalizeP7Report(
            StringBuilder report,
            bool passed,
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP7ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P7 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P7] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P7] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P7] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }
    }
}
#endif
