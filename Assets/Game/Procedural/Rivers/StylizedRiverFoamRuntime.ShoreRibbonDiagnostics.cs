#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const double ShoreRibbonAuditReadyTimeoutSeconds = 15.0;
        private const float ShoreRibbonAuditCoverageThreshold = 0.01f;

        private readonly struct ShoreRibbonAuditCase
        {
            public ShoreRibbonAuditCase(
                string name,
                float speedCellsPerSecond,
                int lengthCells,
                float sideSign,
                bool delayedTick)
            {
                Name = name;
                SpeedCellsPerSecond = speedCellsPerSecond;
                LengthCells = lengthCells;
                SideSign = sideSign;
                DelayedTick = delayedTick;
            }

            public string Name { get; }
            public float SpeedCellsPerSecond { get; }
            public int LengthCells { get; }
            public float SideSign { get; }
            public bool DelayedTick { get; }
        }

        private sealed class ShoreRibbonAuditCapture
        {
            public int Generation;
            public int CaseIndex;
            public int Checkpoint;
            public int PreviousRevealedCells;
            public int CurrentRevealedCells;
            public int ExpectedNewCells;
            public int LogicalDispatchCount;
            public bool PostCompletion;
            public int PendingReadbacks;
            public bool ReadbackError;
            public ushort[] CurrentTickData;
            public ushort[] AccumulatedData;
        }

        private readonly struct ShoreRibbonBirthStats
        {
            public ShoreRibbonBirthStats(
                double coverage,
                int occupiedCells,
                int occupiedColumns,
                int maximumRowsPerColumn,
                int internalGapColumns,
                int minX,
                int maxX,
                int minY,
                int maxY)
            {
                Coverage = coverage;
                OccupiedCells = occupiedCells;
                OccupiedColumns = occupiedColumns;
                MaximumRowsPerColumn = maximumRowsPerColumn;
                InternalGapColumns = internalGapColumns;
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
            }

            public double Coverage { get; }
            public int OccupiedCells { get; }
            public int OccupiedColumns { get; }
            public int MaximumRowsPerColumn { get; }
            public int InternalGapColumns { get; }
            public int MinX { get; }
            public int MaxX { get; }
            public int MinY { get; }
            public int MaxY { get; }
        }

        private readonly List<ShoreRibbonAuditCase> shoreRibbonAuditCases = new();
        private StringBuilder shoreRibbonAuditText;
        private StringBuilder shoreRibbonAuditCsv;
        private bool shoreRibbonBehaviorAuditRunning;
        private bool shoreRibbonAuditReadbackPending;
        private bool shoreRibbonAuditCancelRequested;
        private bool shoreRibbonAuditPostCompletionPending;
        private int shoreRibbonAuditGeneration;
        private int shoreRibbonAuditCaseIndex;
        private int shoreRibbonAuditPassCount;
        private int shoreRibbonAuditFailCount;
        private int shoreRibbonAuditCompletedCheckpoints;
        private int shoreRibbonAuditTotalCheckpoints;
        private int shoreRibbonAuditTickIndex;
        private int shoreRibbonAuditCheckpointIndex;
        private int shoreRibbonAuditLogicalDispatchCount;
        private float shoreRibbonAuditElapsed;
        private float shoreRibbonAuditMaterialHz;
        private float shoreRibbonAuditMaterialStepDuration;
        private double shoreRibbonAuditStartedAt;
        private double shoreRibbonAuditWaitStartedAt;
        private double shoreRibbonAuditLastRepaintAt;
        private string shoreRibbonAuditPhase = "Idle";
        private string shoreRibbonAuditCurrentCase = "Idle";
        private string shoreRibbonAuditLastResult = "None";
        private AutomaticFoamSourceEvent shoreRibbonAuditEvent;
        private P7SourceDispatchRange shoreRibbonAuditRange;
        private RenderTexture shoreRibbonAuditCurrentTickTexture;
        private RenderTexture shoreRibbonAuditAccumulatedTexture;
        private RenderTexture shoreRibbonAuditBoundaryTexture;
        private RenderTexture shoreRibbonAuditObstacleTexture;
        private RenderTexture shoreRibbonAuditShoreTexture;
        private RenderTexture shoreRibbonAuditObjectTexture;
        private ComputeBuffer shoreRibbonAuditEventBuffer;

        public bool ShoreRibbonBehaviorAuditRunning => shoreRibbonBehaviorAuditRunning;
        public bool ShoreRibbonBehaviorAuditForcesRuntimeWork => shoreRibbonBehaviorAuditRunning;
        public bool ShoreRibbonBehaviorAuditReadbackPending => shoreRibbonAuditReadbackPending;
        public string ShoreRibbonBehaviorAuditPhase => shoreRibbonAuditPhase;
        public string ShoreRibbonBehaviorAuditCurrentCase => shoreRibbonAuditCurrentCase;
        public string ShoreRibbonBehaviorAuditLastResult => shoreRibbonAuditLastResult;
        public int ShoreRibbonBehaviorAuditPassCount => shoreRibbonAuditPassCount;
        public int ShoreRibbonBehaviorAuditFailCount => shoreRibbonAuditFailCount;
        public int ShoreRibbonBehaviorAuditCompleted => shoreRibbonAuditCompletedCheckpoints;
        public int ShoreRibbonBehaviorAuditTotal => shoreRibbonAuditTotalCheckpoints;
        public float ShoreRibbonBehaviorAuditProgress => shoreRibbonAuditTotalCheckpoints > 0
            ? Mathf.Clamp01(
                (float)shoreRibbonAuditCompletedCheckpoints /
                shoreRibbonAuditTotalCheckpoints)
            : 0f;
        public double ShoreRibbonBehaviorAuditElapsedSeconds =>
            shoreRibbonBehaviorAuditRunning
                ? Math.Max(
                    0.0,
                    Time.realtimeSinceStartupAsDouble -
                    shoreRibbonAuditStartedAt)
                : 0.0;
        public double ShoreRibbonBehaviorAuditEtaSeconds
        {
            get
            {
                double elapsed = ShoreRibbonBehaviorAuditElapsedSeconds;
                double rate = elapsed > 0.001
                    ? shoreRibbonAuditCompletedCheckpoints / elapsed
                    : 0.0;
                return rate > 0.001
                    ? Math.Max(
                        0.0,
                        (shoreRibbonAuditTotalCheckpoints -
                            shoreRibbonAuditCompletedCheckpoints) / rate)
                    : 0.0;
            }
        }

        public bool RunShoreRibbonBehaviorSuite()
        {
            if (shoreRibbonBehaviorAuditRunning ||
                cellSpawnerContractAuditRunning)
            {
                return false;
            }
            if (!Application.isPlaying)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary =
                    "PLAY MODE REQUIRED — enter Play Mode before running " +
                    "the Shore Ribbon Behavior Suite.";
                return false;
            }
            if (!IsSupported || !SystemInfo.supportsAsyncGPUReadback)
            {
                topologyCacheDiagnosticState = "Unsupported";
                topologyCacheDiagnosticSummary =
                    "Compute support and asynchronous GPU readback are " +
                    "required; no synchronous fallback is allowed.";
                return false;
            }

            river = GetComponent<StylizedRiver>();
            if (river == null || !river.FoamEnabled || !river.Domain.IsValid)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary =
                    "A valid enabled StylizedRiver Foam runtime is required.";
                return false;
            }

            shoreRibbonAuditCases.Clear();
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Right bank · 1 cell/s · 8 cells", 1f, 8, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Left bank · 1 cell/s · 8 cells", 1f, 8, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Right bank · 1 cell/s · 20 cells", 1f, 20, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Left bank · 1 cell/s · 20 cells", 1f, 20, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Right bank · 2 cells/s · 20 cells", 2f, 20, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Left bank · 2 cells/s · 20 cells", 2f, 20, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase(
                "Right bank · delayed 3.5-cell material tick",
                1f,
                8,
                1f,
                true));

            shoreRibbonAuditMaterialHz = Mathf.Max(1f, ResolveUpdateRate());
            shoreRibbonAuditMaterialStepDuration =
                1f / shoreRibbonAuditMaterialHz;
            shoreRibbonAuditTotalCheckpoints = 1;
            for (int i = 0; i < shoreRibbonAuditCases.Count; i++)
            {
                shoreRibbonAuditTotalCheckpoints +=
                    CountShoreRibbonAuditCaptures(shoreRibbonAuditCases[i]);
            }

            shoreRibbonAuditGeneration++;
            shoreRibbonAuditCaseIndex = -1;
            shoreRibbonAuditPassCount = 0;
            shoreRibbonAuditFailCount = 0;
            shoreRibbonAuditCompletedCheckpoints = 0;
            shoreRibbonAuditReadbackPending = false;
            shoreRibbonAuditCancelRequested = false;
            shoreRibbonAuditPostCompletionPending = false;
            shoreRibbonAuditStartedAt = Time.realtimeSinceStartupAsDouble;
            shoreRibbonAuditWaitStartedAt = shoreRibbonAuditStartedAt;
            shoreRibbonAuditLastRepaintAt = 0.0;
            shoreRibbonAuditPhase = "Runtime readiness";
            shoreRibbonAuditCurrentCase = "Waiting for River Foam runtime";
            shoreRibbonAuditLastResult = "None";
            shoreRibbonAuditText = new StringBuilder(65536);
            shoreRibbonAuditCsv = new StringBuilder(65536);
            shoreRibbonAuditText.AppendLine(
                "RIVER FOAM SHORE RIBBON DISCRETE BIRTH AUDIT");
            shoreRibbonAuditText.AppendLine(BuildCommonEnvironmentHeader());
            shoreRibbonAuditText.AppendLine(
                $"Active material cadence: {shoreRibbonAuditMaterialHz:0.###} Hz.");
            shoreRibbonAuditText.AppendLine(
                "Birth scope only: no transport, lifecycle, or final-render " +
                "kernel is dispatched by this suite.");
            shoreRibbonAuditText.AppendLine(
                "CURRENT_TICK_SOURCE must contain exactly one occupied row " +
                "and one occupied column per newly entered path cell.");
            shoreRibbonAuditText.AppendLine(
                "ACCUMULATED_BIRTH must contain every revealed path cell once, " +
                "with zero internal columns and maximum lateral width one cell.");
            shoreRibbonAuditText.AppendLine(
                "The visible persistent Foam field and serialized scene state " +
                "are never used as audit targets.");
            shoreRibbonAuditText.AppendLine();
            shoreRibbonAuditCsv.AppendLine(
                "phase,case,side,speed_cells_s,length_cells,checkpoint," +
                "previous_revealed,current_revealed,expected_new_cells," +
                "logical_dispatch_count,post_completion,target,coverage," +
                "occupied_cells,occupied_columns,max_rows_per_column," +
                "internal_gap_columns,min_x,max_x,min_y,max_y,pass,detail");
            shoreRibbonBehaviorAuditRunning = true;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Shore Ribbon discrete birth audit waiting for runtime readiness.";
            RepaintShoreRibbonAuditViews(true);
            return true;
        }

        private int CountShoreRibbonAuditCaptures(ShoreRibbonAuditCase test)
        {
            float duration = test.LengthCells /
                Mathf.Max(0.01f, test.SpeedCellsPerSecond);
            float elapsed = 0f;
            int tickIndex = 0;
            int previousCount = 0;
            int captures = 1; // One post-completion zero-birth capture.
            while (elapsed < duration - 0.000001f)
            {
                float delta = shoreRibbonAuditMaterialStepDuration;
                if (test.DelayedTick && tickIndex == 8)
                {
                    delta = 3.5f /
                        Mathf.Max(0.01f, test.SpeedCellsPerSecond);
                }
                elapsed = Mathf.Min(duration, elapsed + delta);
                int currentCount = ResolveShoreRibbonAuditRevealedCellCount(
                    elapsed,
                    test.SpeedCellsPerSecond,
                    test.LengthCells);
                if (currentCount > previousCount)
                {
                    captures++;
                }
                previousCount = currentCount;
                tickIndex++;
            }
            return captures;
        }

        private static int ResolveShoreRibbonAuditRevealedCellCount(
            float elapsed,
            float speedCellsPerSecond,
            int totalCellCount)
        {
            int resolvedTotalCellCount = Mathf.Max(1, totalCellCount);
            float headDistanceCells = ResolveAutomaticRevealHeadDistanceCells(
                resolvedTotalCellCount,
                speedCellsPerSecond,
                elapsed);
            return Mathf.Clamp(
                Mathf.FloorToInt(headDistanceCells + 0.00001f),
                0,
                resolvedTotalCellCount);
        }

        public void CancelShoreRibbonBehaviorSuite()
        {
            if (!shoreRibbonBehaviorAuditRunning)
            {
                return;
            }
            shoreRibbonAuditCancelRequested = true;
            if (!shoreRibbonAuditReadbackPending)
            {
                FinishShoreRibbonBehaviorSuite(
                    false,
                    true,
                    "Cancelled by user; partial report preserved.");
            }
        }

        private void CancelShoreRibbonBehaviorAuditForLifecycle(string reason)
        {
            if (shoreRibbonBehaviorAuditRunning)
            {
                FinishShoreRibbonBehaviorSuite(false, true, reason);
            }
        }

        private void AdvanceShoreRibbonBehaviorAuditPlayMode(bool runtimeReady)
        {
            if (!shoreRibbonBehaviorAuditRunning)
            {
                return;
            }
            if (!Application.isPlaying)
            {
                FinishShoreRibbonBehaviorSuite(
                    false,
                    true,
                    "Play Mode ended; partial report preserved.");
                return;
            }
            if (shoreRibbonAuditCancelRequested &&
                !shoreRibbonAuditReadbackPending)
            {
                FinishShoreRibbonBehaviorSuite(
                    false,
                    true,
                    "Cancelled by user; partial report preserved.");
                return;
            }

            if (!AreShoreRibbonAuditResourcesReady(out string failure))
            {
                shoreRibbonAuditPhase = "Runtime readiness";
                shoreRibbonAuditCurrentCase = failure;
                double wait = Time.realtimeSinceStartupAsDouble -
                    shoreRibbonAuditWaitStartedAt;
                if (initializationPhase ==
                        InitializationPhase.CachePreparationRequired ||
                    initializationPhase == InitializationPhase.Failed ||
                    wait >= ShoreRibbonAuditReadyTimeoutSeconds)
                {
                    FinishShoreRibbonBehaviorSuite(
                        false,
                        false,
                        $"Runtime readiness failed after {wait:0.0}s: {failure}");
                }
                RepaintShoreRibbonAuditViews();
                return;
            }
            shoreRibbonAuditWaitStartedAt = 0.0;

            // LateUpdate invokes diagnostics once before and once after the
            // runtime-ready branch. Advance only from the latter call.
            if (!runtimeReady)
            {
                RepaintShoreRibbonAuditViews();
                return;
            }
            if (shoreRibbonAuditReadbackPending)
            {
                shoreRibbonAuditPhase = "GPU readback";
                RepaintShoreRibbonAuditViews();
                return;
            }

            if (shoreRibbonAuditCaseIndex < 0)
            {
                RunShoreRibbonControlAuthorityPreflight();
                if (shoreRibbonAuditFailCount > 0)
                {
                    FinishShoreRibbonBehaviorSuite(
                        false,
                        false,
                        "Control-authority preflight failed. " +
                        "No geometry cases were run.");
                    return;
                }
                shoreRibbonAuditCompletedCheckpoints++;
                shoreRibbonAuditCaseIndex = 0;
                if (!BeginShoreRibbonAuditCase())
                {
                    FinishShoreRibbonBehaviorSuite(
                        false,
                        false,
                        "Failed to prepare the first deterministic Shore " +
                        "Ribbon case.");
                }
                return;
            }

            if (shoreRibbonAuditCaseIndex >= shoreRibbonAuditCases.Count)
            {
                FinishShoreRibbonBehaviorSuite(
                    shoreRibbonAuditFailCount == 0,
                    false,
                    shoreRibbonAuditFailCount == 0
                        ? "All dedicated Shore Ribbon birth cases completed."
                        : "One or more Shore Ribbon birth contracts failed.");
                return;
            }

            AdvanceShoreRibbonAuditTick();
            RepaintShoreRibbonAuditViews();
        }

        private bool AreShoreRibbonAuditResourcesReady(out string failure)
        {
            if (!AreCellSpawnerAuditResourcesReady(out failure))
            {
                return false;
            }
            if (rasterizeFoamSourceEventKernel < 0)
            {
                failure = "production Shore Ribbon raster kernel unavailable";
                return false;
            }
            return true;
        }

        private void RunShoreRibbonControlAuthorityPreflight()
        {
            shoreRibbonAuditPhase = "Control-authority preflight";
            int activeTotal = 0;
            int activeShore = 0;
            int activeInward = 0;
            int activeObject = 0;
            int activeFree = 0;
            for (int i = 0; i < automaticFoamSourceEvents.Length; i++)
            {
                AutomaticFoamSourceEvent e = automaticFoamSourceEvents[i];
                if (!e.Active)
                {
                    continue;
                }
                activeTotal++;
                switch (e.Type)
                {
                    case AutomaticFoamSourceEventType.ShoreRibbon:
                        activeShore++;
                        break;
                    case AutomaticFoamSourceEventType.InwardWash:
                        activeInward++;
                        break;
                    case AutomaticFoamSourceEventType.ObjectContactArc:
                    case AutomaticFoamSourceEventType.ObjectContactSemiArc:
                    case AutomaticFoamSourceEventType.ObjectContactFleck:
                        activeObject++;
                        break;
                    default:
                        activeFree++;
                        break;
                }
            }

            bool shoreActive = river.FoamAutomaticShoreBirthActive;
            bool objectInactive = !river.FoamAutomaticObjectBirthActive;
            bool freeInactive = !river.FoamAutomaticFreeWaterBirthActive;
            bool pureRibbon = river.FoamShoreFoamPattern ==
                    StylizedRiverFoamShorePattern.ShoreRibbons ||
                (river.FoamShoreRibbonPatternWeight > 0.999f &&
                    river.FoamInwardWashPatternWeight < 0.001f);
            bool activeTypesAuthoritative = activeInward == 0 &&
                activeObject == 0 && activeFree == 0;
            bool pass = shoreActive && objectInactive && freeInactive &&
                pureRibbon && activeTypesAuthoritative;

            shoreRibbonAuditText.AppendLine("CONTROL-AUTHORITY PREFLIGHT");
            shoreRibbonAuditText.AppendLine(
                $"Automatic birth: {river.FoamAutomaticBirthEnabled}");
            shoreRibbonAuditText.AppendLine(
                $"Shore active: {shoreActive}; " +
                $"activity={river.FoamShoreFoamActivity:0.###}; " +
                $"scheduling=all-shore length-scaled buckets");
            shoreRibbonAuditText.AppendLine(
                $"Pattern={river.FoamShoreFoamPattern}; " +
                $"ribbonWeight={river.FoamShoreRibbonPatternWeight:0.###}; " +
                $"inwardWeight={river.FoamInwardWashPatternWeight:0.###}");
            shoreRibbonAuditText.AppendLine(
                $"Object active: {river.FoamAutomaticObjectBirthActive}; " +
                $"Free-water active: {river.FoamAutomaticFreeWaterBirthActive}");
            shoreRibbonAuditText.AppendLine(
                $"Live active events: total={activeTotal}, " +
                $"ribbon={activeShore}, inward={activeInward}, " +
                $"object={activeObject}, free={activeFree}");
            shoreRibbonAuditText.AppendLine(
                "CONTROL-AUTHORITY VERDICT: " + (pass ? "PASS" : "FAIL"));
            shoreRibbonAuditText.AppendLine();
            shoreRibbonAuditLastResult = pass
                ? "Control authority PASS"
                : "Control authority FAIL";
            if (pass)
            {
                shoreRibbonAuditPassCount++;
            }
            else
            {
                shoreRibbonAuditFailCount++;
            }
        }

        private bool BeginShoreRibbonAuditCase()
        {
            ReleaseShoreRibbonAuditCaseResources();
            if (shoreRibbonAuditCaseIndex < 0 ||
                shoreRibbonAuditCaseIndex >= shoreRibbonAuditCases.Count)
            {
                return false;
            }

            ShoreRibbonAuditCase test =
                shoreRibbonAuditCases[shoreRibbonAuditCaseIndex];
            CellSpawnerAuditCase geometryCase =
                new CellSpawnerAuditCase(0, 0, 0);
            shoreRibbonAuditEvent = BuildCellSpawnerAuditEvent(
                geometryCase,
                test.LengthCells,
                1f,
                1f,
                1f,
                false,
                0f,
                0f,
                0f);
            shoreRibbonAuditEvent.EventId =
                910000 + shoreRibbonAuditCaseIndex;
            shoreRibbonAuditEvent.Type =
                AutomaticFoamSourceEventType.ShoreRibbon;
            shoreRibbonAuditEvent.SideSign = test.SideSign;
            shoreRibbonAuditEvent.BodyLengthCells = test.LengthCells;
            shoreRibbonAuditEvent.BodyWidthCells = 1f;
            shoreRibbonAuditEvent.HeadLengthCells = 1f;
            shoreRibbonAuditEvent.HeadWidthCells = 1f;
            shoreRibbonAuditEvent.WidthMetres = 1f;
            shoreRibbonAuditEvent.FeatherMetres = 1f;
            shoreRibbonAuditEvent.ShoreInsetMetres = 0f;
            shoreRibbonAuditEvent.SourceAmount = 1f;
            shoreRibbonAuditEvent.RemainingLife = 1f;
            shoreRibbonAuditEvent.RevealSpeedCellsPerSecond =
                test.SpeedCellsPerSecond;
            shoreRibbonAuditEvent.RevealPathLengthCells =
                test.LengthCells;
            shoreRibbonAuditEvent.Duration =
                test.LengthCells /
                Mathf.Max(0.01f, test.SpeedCellsPerSecond);
            shoreRibbonAuditEvent.Elapsed = 0f;
            shoreRibbonAuditEvent.HeadTrailMetres = 1f;

            if (!TryResolveAutomaticSourceDispatchRange(
                    shoreRibbonAuditEvent,
                    out shoreRibbonAuditRange))
            {
                RecordShoreRibbonAuditPreparationFailure(
                    test,
                    "dispatch range resolution failed");
                return false;
            }

            shoreRibbonAuditCurrentTickTexture = CreateFieldTexture(
                "PS3D_ShoreRibbonBirth_CurrentTick");
            shoreRibbonAuditAccumulatedTexture = CreateFieldTexture(
                "PS3D_ShoreRibbonBirth_Accumulated");
            shoreRibbonAuditBoundaryTexture = CreateFieldTexture(
                "PS3D_ShoreRibbonBirth_Boundary");
            shoreRibbonAuditObstacleTexture = CreateObstacleExclusionTexture(
                "PS3D_ShoreRibbonBirth_Obstacle");
            shoreRibbonAuditShoreTexture = CreateShoreEdgesTexture(
                "PS3D_ShoreRibbonBirth_Shore");
            shoreRibbonAuditObjectTexture = CreateFieldTexture(
                "PS3D_ShoreRibbonBirth_Object");
            ClearRenderTexture(shoreRibbonAuditCurrentTickTexture);
            ClearRenderTexture(shoreRibbonAuditAccumulatedTexture);
            ClearCellSpawnerAuditTexture(
                shoreRibbonAuditBoundaryTexture,
                Color.white);
            ClearCellSpawnerAuditTexture(
                shoreRibbonAuditObstacleTexture,
                Color.black);
            ClearCellSpawnerAuditTexture(
                shoreRibbonAuditObjectTexture,
                new Color(1f, -1f, 0f, 1f));
            if (!TryPopulateCellSpawnerSyntheticShoreEdges(
                    shoreRibbonAuditShoreTexture,
                    shoreRibbonAuditEvent,
                    shoreRibbonAuditRange,
                    out string fixtureFailure))
            {
                RecordShoreRibbonAuditPreparationFailure(
                    test,
                    fixtureFailure);
                return false;
            }

            shoreRibbonAuditEventBuffer = new ComputeBuffer(
                1,
                Marshal.SizeOf<FoamSourceEventGpuData>(),
                ComputeBufferType.Structured);
            shoreRibbonAuditTickIndex = 0;
            shoreRibbonAuditCheckpointIndex = 0;
            shoreRibbonAuditLogicalDispatchCount = 0;
            shoreRibbonAuditElapsed = 0f;
            shoreRibbonAuditPostCompletionPending = false;
            shoreRibbonAuditCurrentCase =
                $"{shoreRibbonAuditCaseIndex + 1}/" +
                $"{shoreRibbonAuditCases.Count} · {test.Name}";
            shoreRibbonAuditPhase = "Discrete birth ticks";
            return true;
        }

        private void AdvanceShoreRibbonAuditTick()
        {
            ShoreRibbonAuditCase test =
                shoreRibbonAuditCases[shoreRibbonAuditCaseIndex];
            if (shoreRibbonAuditPostCompletionPending)
            {
                RequestShoreRibbonAuditCapture(
                    test.LengthCells,
                    test.LengthCells,
                    0,
                    true);
                shoreRibbonAuditPostCompletionPending = false;
                return;
            }

            float previousElapsed = shoreRibbonAuditElapsed;
            int previousCount = ResolveShoreRibbonAuditRevealedCellCount(
                previousElapsed,
                test.SpeedCellsPerSecond,
                test.LengthCells);
            float sourceDelta = shoreRibbonAuditMaterialStepDuration;
            if (test.DelayedTick && shoreRibbonAuditTickIndex == 8)
            {
                sourceDelta = 3.5f /
                    Mathf.Max(0.01f, test.SpeedCellsPerSecond);
            }
            shoreRibbonAuditElapsed = Mathf.Min(
                shoreRibbonAuditEvent.Duration,
                shoreRibbonAuditElapsed + sourceDelta);
            shoreRibbonAuditEvent.Elapsed = shoreRibbonAuditElapsed;
            int currentCount = ResolveShoreRibbonAuditRevealedCellCount(
                shoreRibbonAuditElapsed,
                test.SpeedCellsPerSecond,
                test.LengthCells);
            shoreRibbonAuditTickIndex++;

            if (currentCount > previousCount)
            {
                ClearRenderTexture(shoreRibbonAuditCurrentTickTexture);
                PrepareShoreRibbonAuditEventBuffer(previousElapsed);
                DispatchShoreRibbonAuditBirth(
                    shoreRibbonAuditCurrentTickTexture);
                DispatchShoreRibbonAuditBirth(
                    shoreRibbonAuditAccumulatedTexture);
                shoreRibbonAuditLogicalDispatchCount++;
                RequestShoreRibbonAuditCapture(
                    previousCount,
                    currentCount,
                    currentCount - previousCount,
                    false);
                return;
            }

            if (shoreRibbonAuditElapsed >=
                shoreRibbonAuditEvent.Duration - 0.000001f)
            {
                shoreRibbonAuditPostCompletionPending = true;
            }
        }

        private void PrepareShoreRibbonAuditEventBuffer(float previousElapsed)
        {
            FoamSourceEventGpuData gpuData =
                BuildAutomaticFoamSourceGpuData(
                    shoreRibbonAuditEvent,
                    previousElapsed);
            shoreRibbonAuditEventBuffer.SetData(new[] { gpuData });
        }

        private void DispatchShoreRibbonAuditBirth(RenderTexture target)
        {
            ConfigureGridDescriptorComputeParameters();
            int kernel = rasterizeFoamSourceEventKernel;
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat("_FoamBulkTransportPhaseCells", 0f);
            computeShader.SetInt("_FoamBulkTransportIntegerShift", 0);
            computeShader.SetInt(
                "_FoamRangeStart",
                shoreRibbonAuditRange.StartX);
            computeShader.SetInt(
                "_FoamRangeCount",
                shoreRibbonAuditRange.CountX);
            computeShader.SetInt(
                "_FoamRangeStartY",
                shoreRibbonAuditRange.StartY);
            computeShader.SetInt(
                "_FoamRangeCountY",
                shoreRibbonAuditRange.CountY);
            computeShader.SetInt("_FoamSourceEventIndex", 0);
            computeShader.SetInt("_FoamSourceEventDebugComponentMode", 0);
            computeShader.SetBuffer(kernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetBuffer(
                kernel,
                "_FoamSourceEvents",
                shoreRibbonAuditEventBuffer);
            computeShader.SetTexture(
                kernel,
                "_FoamBoundary",
                shoreRibbonAuditBoundaryTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamObstacleExclusionRead",
                shoreRibbonAuditObstacleTexture);
            computeShader.SetTexture(
                kernel,
                "_FoamCurrentShoreEdgesRead",
                shoreRibbonAuditShoreTexture);
            Texture neutral = neutralDisturbanceTexture != null
                ? (Texture)neutralDisturbanceTexture
                : Texture2D.blackTexture;
            computeShader.SetInts("_FoamStaticPressureDimensions", 1, 1);
            computeShader.SetTexture(
                kernel,
                "_FoamStaticPressureField",
                neutral);
            computeShader.SetTexture(
                kernel,
                "_FoamObjectContactFieldRead",
                shoreRibbonAuditObjectTexture);
            computeShader.SetTexture(kernel, "_FoamStateWrite", target);
            Dispatch(
                kernel,
                shoreRibbonAuditRange.CountX,
                shoreRibbonAuditRange.CountY);
        }

        private void RequestShoreRibbonAuditCapture(
            int previousCount,
            int currentCount,
            int expectedNewCells,
            bool postCompletion)
        {
            if (postCompletion)
            {
                ClearRenderTexture(shoreRibbonAuditCurrentTickTexture);
            }

            ShoreRibbonAuditCapture capture = new ShoreRibbonAuditCapture
            {
                Generation = shoreRibbonAuditGeneration,
                CaseIndex = shoreRibbonAuditCaseIndex,
                Checkpoint = ++shoreRibbonAuditCheckpointIndex,
                PreviousRevealedCells = previousCount,
                CurrentRevealedCells = currentCount,
                ExpectedNewCells = expectedNewCells,
                LogicalDispatchCount = shoreRibbonAuditLogicalDispatchCount,
                PostCompletion = postCompletion,
                PendingReadbacks = 2
            };
            shoreRibbonAuditReadbackPending = true;
            shoreRibbonAuditPhase = "GPU readback";
            RequestShoreRibbonAuditReadback(
                capture,
                shoreRibbonAuditCurrentTickTexture,
                true);
            RequestShoreRibbonAuditReadback(
                capture,
                shoreRibbonAuditAccumulatedTexture,
                false);
        }

        private void RequestShoreRibbonAuditReadback(
            ShoreRibbonAuditCapture capture,
            RenderTexture texture,
            bool currentTick)
        {
            AsyncGPUReadback.Request(texture, 0, request =>
            {
                if (capture.Generation != shoreRibbonAuditGeneration ||
                    !shoreRibbonBehaviorAuditRunning)
                {
                    return;
                }
                if (request.hasError)
                {
                    capture.ReadbackError = true;
                }
                else
                {
                    var native = request.GetData<ushort>();
                    ushort[] managed = new ushort[native.Length];
                    native.CopyTo(managed);
                    if (currentTick)
                    {
                        capture.CurrentTickData = managed;
                    }
                    else
                    {
                        capture.AccumulatedData = managed;
                    }
                }
                capture.PendingReadbacks--;
                if (capture.PendingReadbacks <= 0)
                {
                    CompleteShoreRibbonAuditReadbacks(capture);
                }
            });
        }

        private ShoreRibbonBirthStats MeasureShoreRibbonBirth(ushort[] data)
        {
            if (data == null)
            {
                return new ShoreRibbonBirthStats(
                    0.0,
                    0,
                    0,
                    0,
                    0,
                    -1,
                    -1,
                    -1,
                    -1);
            }

            double coverageArea = 0.0;
            int occupiedCells = 0;
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            int[] occupiedRowsPerColumn = new int[fieldWidth];
            for (int y = 0; y < fieldHeight; y++)
            {
                for (int x = 0; x < fieldWidth; x++)
                {
                    int index = (y * fieldWidth + x) * 4;
                    float coverage = Mathf.Max(
                        0f,
                        Mathf.HalfToFloat(data[index + 3]));
                    if (coverage <= ShoreRibbonAuditCoverageThreshold)
                    {
                        continue;
                    }
                    coverageArea += coverage;
                    occupiedCells++;
                    occupiedRowsPerColumn[x]++;
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            int occupiedColumns = 0;
            int maximumRowsPerColumn = 0;
            int internalGapColumns = 0;
            if (minX <= maxX)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int rows = occupiedRowsPerColumn[x];
                    if (rows > 0)
                    {
                        occupiedColumns++;
                        maximumRowsPerColumn = Mathf.Max(
                            maximumRowsPerColumn,
                            rows);
                    }
                    else
                    {
                        internalGapColumns++;
                    }
                }
            }

            bool any = minX <= maxX;
            return new ShoreRibbonBirthStats(
                coverageArea,
                occupiedCells,
                occupiedColumns,
                maximumRowsPerColumn,
                internalGapColumns,
                any ? minX : -1,
                any ? maxX : -1,
                any ? minY : -1,
                any ? maxY : -1);
        }

        private void CompleteShoreRibbonAuditReadbacks(
            ShoreRibbonAuditCapture capture)
        {
            if (capture.Generation != shoreRibbonAuditGeneration ||
                !shoreRibbonBehaviorAuditRunning)
            {
                return;
            }
            shoreRibbonAuditReadbackPending = false;
            if (capture.ReadbackError ||
                capture.CurrentTickData == null ||
                capture.AccumulatedData == null)
            {
                shoreRibbonAuditFailCount++;
                shoreRibbonAuditLastResult = "AsyncGPUReadback failed";
                shoreRibbonAuditText.AppendLine(
                    $"{shoreRibbonAuditCurrentCase}: FAIL · " +
                    "one or more birth readbacks failed.");
                FinishShoreRibbonBehaviorSuite(
                    false,
                    false,
                    "GPU birth readback failed.");
                return;
            }

            ShoreRibbonAuditCase test =
                shoreRibbonAuditCases[capture.CaseIndex];
            ShoreRibbonBirthStats current =
                MeasureShoreRibbonBirth(capture.CurrentTickData);
            ShoreRibbonBirthStats accumulated =
                MeasureShoreRibbonBirth(capture.AccumulatedData);

            bool currentPass = current.OccupiedCells ==
                    capture.ExpectedNewCells &&
                current.OccupiedColumns == capture.ExpectedNewCells &&
                current.MaximumRowsPerColumn <= 1 &&
                current.InternalGapColumns == 0;
            bool accumulatedPass = accumulated.OccupiedCells ==
                    capture.CurrentRevealedCells &&
                accumulated.OccupiedColumns ==
                    capture.CurrentRevealedCells &&
                accumulated.MaximumRowsPerColumn <= 1 &&
                accumulated.InternalGapColumns == 0;
            bool postCompletionPass = !capture.PostCompletion ||
                (capture.ExpectedNewCells == 0 &&
                    current.OccupiedCells == 0 &&
                    capture.CurrentRevealedCells == test.LengthCells &&
                    accumulated.OccupiedCells == test.LengthCells);
            bool pass = currentPass && accumulatedPass &&
                postCompletionPass;

            string phase = capture.PostCompletion
                ? "POST_COMPLETION"
                : "BIRTH_TICK";
            shoreRibbonAuditText.AppendLine(
                $"{capture.CaseIndex + 1:00}." +
                $"{capture.Checkpoint:00} {test.Name} · {phase} · " +
                $"revealed={capture.PreviousRevealedCells}->" +
                $"{capture.CurrentRevealedCells}; " +
                $"new={capture.ExpectedNewCells}; " +
                $"dispatches={capture.LogicalDispatchCount} — " +
                $"{(pass ? "PASS" : "FAIL")}");
            AppendShoreRibbonBirthRow(
                capture,
                test,
                phase,
                "CURRENT_TICK_SOURCE",
                current,
                currentPass,
                $"expectedCells={capture.ExpectedNewCells}; " +
                "exact one row and one column per new path cell");
            AppendShoreRibbonBirthRow(
                capture,
                test,
                phase,
                "ACCUMULATED_BIRTH",
                accumulated,
                accumulatedPass && postCompletionPass,
                $"expectedCells={capture.CurrentRevealedCells}; " +
                "zero internal columns; maximum width one cell");

            shoreRibbonAuditCompletedCheckpoints++;
            if (pass)
            {
                shoreRibbonAuditPassCount++;
            }
            else
            {
                shoreRibbonAuditFailCount++;
            }
            shoreRibbonAuditLastResult =
                $"{(pass ? "PASS" : "FAIL")} · {test.Name} · " +
                $"{capture.CurrentRevealedCells}/{test.LengthCells} cells";

            if (capture.PostCompletion)
            {
                bool completionPass = Mathf.Abs(
                        shoreRibbonAuditEvent.Elapsed -
                        shoreRibbonAuditEvent.Duration) <= 0.0001f &&
                    accumulated.OccupiedCells == test.LengthCells &&
                    current.OccupiedCells == 0;
                shoreRibbonAuditText.AppendLine(
                    "COMPLETION: " +
                    $"elapsed={shoreRibbonAuditEvent.Elapsed:0.###}/" +
                    $"{shoreRibbonAuditEvent.Duration:0.###}s; " +
                    $"resolvedLength={test.LengthCells}; " +
                    $"postCompletionBirths={current.OccupiedCells}; " +
                    $"verdict={(completionPass ? "PASS" : "FAIL")}");
                shoreRibbonAuditText.AppendLine();
                if (!completionPass)
                {
                    shoreRibbonAuditFailCount++;
                }
                shoreRibbonAuditCaseIndex++;
                if (shoreRibbonAuditCaseIndex <
                    shoreRibbonAuditCases.Count)
                {
                    if (!BeginShoreRibbonAuditCase())
                    {
                        FinishShoreRibbonBehaviorSuite(
                            false,
                            false,
                            "Failed to prepare the next deterministic case.");
                    }
                }
            }
            else if (shoreRibbonAuditElapsed >=
                shoreRibbonAuditEvent.Duration - 0.000001f)
            {
                shoreRibbonAuditPostCompletionPending = true;
            }

            RepaintShoreRibbonAuditViews(true);
        }

        private void AppendShoreRibbonBirthRow(
            ShoreRibbonAuditCapture capture,
            ShoreRibbonAuditCase test,
            string phase,
            string target,
            ShoreRibbonBirthStats stats,
            bool pass,
            string detail)
        {
            shoreRibbonAuditText.AppendLine(
                $"    {target}: {(pass ? "PASS" : "FAIL")} · " +
                $"coverage={stats.Coverage:0.###}; " +
                $"cells={stats.OccupiedCells}; " +
                $"columns={stats.OccupiedColumns}; " +
                $"maxRows/column={stats.MaximumRowsPerColumn}; " +
                $"gaps={stats.InternalGapColumns}; " +
                $"bounds=x[{stats.MinX},{stats.MaxX}] " +
                $"y[{stats.MinY},{stats.MaxY}]; {detail}");
            shoreRibbonAuditCsv.AppendLine(string.Join(",",
                Csv(phase),
                Csv(test.Name),
                test.SideSign.ToString("0", CultureInfo.InvariantCulture),
                test.SpeedCellsPerSecond.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture),
                test.LengthCells.ToString(CultureInfo.InvariantCulture),
                capture.Checkpoint.ToString(CultureInfo.InvariantCulture),
                capture.PreviousRevealedCells.ToString(
                    CultureInfo.InvariantCulture),
                capture.CurrentRevealedCells.ToString(
                    CultureInfo.InvariantCulture),
                capture.ExpectedNewCells.ToString(
                    CultureInfo.InvariantCulture),
                capture.LogicalDispatchCount.ToString(
                    CultureInfo.InvariantCulture),
                capture.PostCompletion ? "1" : "0",
                Csv(target),
                stats.Coverage.ToString(
                    "0.######",
                    CultureInfo.InvariantCulture),
                stats.OccupiedCells.ToString(CultureInfo.InvariantCulture),
                stats.OccupiedColumns.ToString(CultureInfo.InvariantCulture),
                stats.MaximumRowsPerColumn.ToString(
                    CultureInfo.InvariantCulture),
                stats.InternalGapColumns.ToString(
                    CultureInfo.InvariantCulture),
                stats.MinX.ToString(CultureInfo.InvariantCulture),
                stats.MaxX.ToString(CultureInfo.InvariantCulture),
                stats.MinY.ToString(CultureInfo.InvariantCulture),
                stats.MaxY.ToString(CultureInfo.InvariantCulture),
                pass ? "1" : "0",
                Csv(detail)));
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            return '"' + value.Replace("\"", "\"\"") + '"';
        }

        private void RecordShoreRibbonAuditPreparationFailure(
            ShoreRibbonAuditCase test,
            string reason)
        {
            shoreRibbonAuditFailCount++;
            shoreRibbonAuditLastResult = "Preparation FAIL · " + reason;
            shoreRibbonAuditText.AppendLine(
                $"{test.Name}: PREPARATION FAIL · {reason}");
        }

        private void FinishShoreRibbonBehaviorSuite(
            bool success,
            bool cancelled,
            string reason)
        {
            if (!shoreRibbonBehaviorAuditRunning)
            {
                return;
            }
            shoreRibbonBehaviorAuditRunning = false;
            shoreRibbonAuditReadbackPending = false;
            ReleaseShoreRibbonAuditCaseResources();
            double elapsed = Math.Max(
                0.0,
                Time.realtimeSinceStartupAsDouble - shoreRibbonAuditStartedAt);
            shoreRibbonAuditText.AppendLine("SUMMARY");
            shoreRibbonAuditText.AppendLine(
                $"Completed checkpoints: " +
                $"{shoreRibbonAuditCompletedCheckpoints}/" +
                $"{shoreRibbonAuditTotalCheckpoints}");
            shoreRibbonAuditText.AppendLine(
                $"Passed observations: {shoreRibbonAuditPassCount}");
            shoreRibbonAuditText.AppendLine(
                $"Failed observations: {shoreRibbonAuditFailCount}");
            shoreRibbonAuditText.AppendLine($"Elapsed: {elapsed:0.000} s");
            shoreRibbonAuditText.AppendLine($"Reason: {reason}");
            shoreRibbonAuditText.AppendLine(
                $"Outcome: {(cancelled ? "CANCELLED" : success ? "PASS" : "FAIL")}");
            string projectRoot =
                Directory.GetParent(Application.dataPath)?.FullName ??
                Application.dataPath;
            string directory = Path.Combine(
                projectRoot,
                "Library",
                "RiverFoam");
            Directory.CreateDirectory(directory);
            string textPath = Path.Combine(
                directory,
                "ShoreRibbonBehaviorSuite.txt");
            string csvPath = Path.Combine(
                directory,
                "ShoreRibbonBehaviorSuite.csv");
            File.WriteAllText(textPath, shoreRibbonAuditText.ToString());
            File.WriteAllText(csvPath, shoreRibbonAuditCsv.ToString());
            topologyCacheDiagnosticReport = shoreRibbonAuditText.ToString();
            topologyCacheDiagnosticReportPath = textPath;
            topologyCacheDiagnosticState = cancelled
                ? "Cancelled"
                : success
                    ? "Passed"
                    : "Failed";
            topologyCacheDiagnosticSummary =
                $"Shore Ribbon discrete birth audit " +
                $"{(cancelled ? "cancelled" : success ? "passed" : "failed")} · " +
                $"{shoreRibbonAuditCompletedCheckpoints}/" +
                $"{shoreRibbonAuditTotalCheckpoints} checkpoints · " +
                $"PASS {shoreRibbonAuditPassCount} · " +
                $"FAIL {shoreRibbonAuditFailCount} · {elapsed:0.0}s";
            shoreRibbonAuditPhase = cancelled
                ? "Cancelled"
                : success
                    ? "Complete"
                    : "Failed";
            shoreRibbonAuditCurrentCase = "None";
            RepaintShoreRibbonAuditViews(true);
        }

        private void ReleaseShoreRibbonAuditCaseResources()
        {
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditCurrentTickTexture);
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditAccumulatedTexture);
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditBoundaryTexture);
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditObstacleTexture);
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditShoreTexture);
            ReleaseCellSpawnerAuditTexture(
                shoreRibbonAuditObjectTexture);
            shoreRibbonAuditCurrentTickTexture = null;
            shoreRibbonAuditAccumulatedTexture = null;
            shoreRibbonAuditBoundaryTexture = null;
            shoreRibbonAuditObstacleTexture = null;
            shoreRibbonAuditShoreTexture = null;
            shoreRibbonAuditObjectTexture = null;
            shoreRibbonAuditEventBuffer?.Release();
            shoreRibbonAuditEventBuffer = null;
        }

        private void RepaintShoreRibbonAuditViews(bool force = false)
        {
            double now = EditorApplication.timeSinceStartup;
            if (!force && now - shoreRibbonAuditLastRepaintAt < 0.10)
            {
                return;
            }
            shoreRibbonAuditLastRepaintAt = now;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
        }
    }
}
#endif
