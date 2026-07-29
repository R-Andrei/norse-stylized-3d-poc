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
        private const float ShoreRibbonAuditMaterialHz = 8f;
        private const float ShoreRibbonAuditCoverageThreshold = 0.01f;

        private readonly struct ShoreRibbonAuditCase
        {
            public ShoreRibbonAuditCase(string name, float speedCellsPerSecond,
                int lengthCells, float sideSign, bool delayedTick)
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
            public float ProgressCells;
            public float PreviousProgressCells;
            public bool Final;
            public int PendingReadbacks;
            public bool ReadbackError;
            public ushort[] BirthData;
            public ushort[] LifecycleData;
            public ushort[] TransportData;
            public ushort[] CombinedData;
        }

        private readonly struct ShoreRibbonStageStats
        {
            public ShoreRibbonStageStats(double coverage, double presence,
                double life, int minX, int maxX, int minY, int maxY,
                int internalGaps, float longitudinalSpanMetres,
                float lateralSpanMetres)
            {
                Coverage = coverage;
                Presence = presence;
                Life = life;
                MinX = minX;
                MaxX = maxX;
                MinY = minY;
                MaxY = maxY;
                InternalGaps = internalGaps;
                LongitudinalSpanMetres = longitudinalSpanMetres;
                LateralSpanMetres = lateralSpanMetres;
            }

            public double Coverage { get; }
            public double Presence { get; }
            public double Life { get; }
            public int MinX { get; }
            public int MaxX { get; }
            public int MinY { get; }
            public int MaxY { get; }
            public int InternalGaps { get; }
            public float LongitudinalSpanMetres { get; }
            public float LateralSpanMetres { get; }
            public float DirectionRatio => LateralSpanMetres > 0.0001f
                ? LongitudinalSpanMetres / LateralSpanMetres
                : 0f;
        }

        private readonly List<ShoreRibbonAuditCase> shoreRibbonAuditCases = new();
        private StringBuilder shoreRibbonAuditText;
        private StringBuilder shoreRibbonAuditCsv;
        private bool shoreRibbonBehaviorAuditRunning;
        private bool shoreRibbonAuditReadbackPending;
        private bool shoreRibbonAuditCancelRequested;
        private int shoreRibbonAuditGeneration;
        private int shoreRibbonAuditCaseIndex;
        private int shoreRibbonAuditPassCount;
        private int shoreRibbonAuditFailCount;
        private int shoreRibbonAuditCompletedCheckpoints;
        private int shoreRibbonAuditTotalCheckpoints;
        private int shoreRibbonAuditTickIndex;
        private int shoreRibbonAuditNextCheckpointCell;
        private float shoreRibbonAuditElapsed;
        private float shoreRibbonAuditPreviousElapsed;
        private double shoreRibbonAuditStartedAt;
        private double shoreRibbonAuditWaitStartedAt;
        private double shoreRibbonAuditLastRepaintAt;
        private string shoreRibbonAuditPhase = "Idle";
        private string shoreRibbonAuditCurrentCase = "Idle";
        private string shoreRibbonAuditLastResult = "None";
        private AutomaticFoamSourceEvent shoreRibbonAuditEvent;
        private P7SourceDispatchRange shoreRibbonAuditRange;
        private RenderTexture shoreRibbonAuditStateTexture;
        private RenderTexture shoreRibbonAuditBirthTexture;
        private RenderTexture shoreRibbonAuditBoundaryTexture;
        private RenderTexture shoreRibbonAuditObstacleTexture;
        private RenderTexture shoreRibbonAuditShoreTexture;
        private RenderTexture shoreRibbonAuditObjectTexture;
        private RenderTexture shoreRibbonAuditLifecycleInputTexture;
        private RenderTexture shoreRibbonAuditLifecycleOutputTexture;
        private RenderTexture shoreRibbonAuditTransportInputTexture;
        private RenderTexture shoreRibbonAuditTransportOutputTexture;
        private RenderTexture shoreRibbonAuditCombinedInputTexture;
        private RenderTexture shoreRibbonAuditCombinedOutputTexture;
        private ComputeBuffer shoreRibbonAuditEventBuffer;
        private ComputeBuffer shoreRibbonAuditCounterBuffer;
        private float shoreRibbonAuditLastSupportMinX;
        private float shoreRibbonAuditLastSupportMaxX;
        private int shoreRibbonAuditLastInternalGapCount;

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
            ? Mathf.Clamp01((float)shoreRibbonAuditCompletedCheckpoints / shoreRibbonAuditTotalCheckpoints)
            : 0f;
        public double ShoreRibbonBehaviorAuditElapsedSeconds => shoreRibbonBehaviorAuditRunning
            ? Math.Max(0.0, Time.realtimeSinceStartupAsDouble - shoreRibbonAuditStartedAt)
            : 0.0;
        public double ShoreRibbonBehaviorAuditEtaSeconds
        {
            get
            {
                double elapsed = ShoreRibbonBehaviorAuditElapsedSeconds;
                double rate = elapsed > 0.001 ? shoreRibbonAuditCompletedCheckpoints / elapsed : 0.0;
                return rate > 0.001
                    ? Math.Max(0.0, (shoreRibbonAuditTotalCheckpoints - shoreRibbonAuditCompletedCheckpoints) / rate)
                    : 0.0;
            }
        }

        public bool RunShoreRibbonBehaviorSuite()
        {
            if (shoreRibbonBehaviorAuditRunning || cellSpawnerContractAuditRunning)
            {
                return false;
            }
            if (!Application.isPlaying)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary = "PLAY MODE REQUIRED — enter Play Mode before running the Shore Ribbon Behavior Suite.";
                return false;
            }
            if (!IsSupported || !SystemInfo.supportsAsyncGPUReadback)
            {
                topologyCacheDiagnosticState = "Unsupported";
                topologyCacheDiagnosticSummary = "Compute support and asynchronous GPU readback are required; no synchronous fallback is allowed.";
                return false;
            }

            river = GetComponent<StylizedRiver>();
            if (river == null || !river.FoamEnabled || !river.Domain.IsValid)
            {
                topologyCacheDiagnosticState = "Unavailable";
                topologyCacheDiagnosticSummary = "A valid enabled StylizedRiver Foam runtime is required.";
                return false;
            }

            shoreRibbonAuditCases.Clear();
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Right bank · 1 cell/s · 8 cells", 1f, 8, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Left bank · 1 cell/s · 8 cells", 1f, 8, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Right bank · 1 cell/s · 20 cells", 1f, 20, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Left bank · 1 cell/s · 20 cells", 1f, 20, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Right bank · 2 cells/s · 8 cells", 2f, 8, 1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Left bank · 2 cells/s · 20 cells", 2f, 20, -1f, false));
            shoreRibbonAuditCases.Add(new ShoreRibbonAuditCase("Right bank · delayed 3.5-cell material tick", 1f, 8, 1f, true));

            shoreRibbonAuditTotalCheckpoints = 1;
            for (int i = 0; i < shoreRibbonAuditCases.Count; i++)
            {
                ShoreRibbonAuditCase auditCase = shoreRibbonAuditCases[i];
                shoreRibbonAuditTotalCheckpoints += auditCase.DelayedTick
                    ? 7
                    : auditCase.LengthCells + 1;
            }

            shoreRibbonAuditGeneration++;
            shoreRibbonAuditCaseIndex = -1;
            shoreRibbonAuditPassCount = 0;
            shoreRibbonAuditFailCount = 0;
            shoreRibbonAuditCompletedCheckpoints = 0;
            shoreRibbonAuditReadbackPending = false;
            shoreRibbonAuditCancelRequested = false;
            shoreRibbonAuditStartedAt = Time.realtimeSinceStartupAsDouble;
            shoreRibbonAuditWaitStartedAt = shoreRibbonAuditStartedAt;
            shoreRibbonAuditPhase = "Control-authority preflight";
            shoreRibbonAuditCurrentCase = "Scene source controls";
            shoreRibbonAuditLastResult = "Pending";
            shoreRibbonAuditText = new StringBuilder(131072);
            shoreRibbonAuditCsv = new StringBuilder(65536);
            shoreRibbonAuditText.AppendLine("RIVER FOAM SHORE RIBBON BEHAVIOR SUITE");
            shoreRibbonAuditText.AppendLine($"UTC: {DateTime.UtcNow:O}");
            shoreRibbonAuditText.AppendLine($"Unity: {Application.unityVersion}");
            shoreRibbonAuditText.AppendLine($"Platform: {Application.platform}");
            shoreRibbonAuditText.AppendLine($"River: {river.name}");
            shoreRibbonAuditText.AppendLine("Purpose: dedicated Shore-only, multi-tick birth and production-pipeline localization audit.");
            shoreRibbonAuditText.AppendLine("Normal speeds: 1 and 2 cells/s. Material cadence: 8 Hz. The delayed case is stall robustness only.");
            shoreRibbonAuditText.AppendLine("Every checkpoint captures four audit-owned stages: accumulated birth, lifecycle-only, transport-only, and combined production simulation.");
            shoreRibbonAuditText.AppendLine("The suite never mutates the visible persistent Foam field or serialized scene state.");
            shoreRibbonAuditText.AppendLine();
            shoreRibbonAuditCsv.AppendLine("phase,case,side,speed_cells_s,length_cells,checkpoint,progress_cells,stage,coverage_area,presence_area,life_area,min_x,max_x,min_y,max_y,internal_gap_columns,longitudinal_span_m,lateral_span_m,direction_ratio,coverage_retention,presence_retention,life_retention,pass,detail");
            shoreRibbonBehaviorAuditRunning = true;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary = "Shore Ribbon Behavior Suite waiting for runtime readiness.";
            RepaintShoreRibbonAuditViews(true);
            return true;
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
                FinishShoreRibbonBehaviorSuite(false, true, "Cancelled by user; partial report preserved.");
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
                FinishShoreRibbonBehaviorSuite(false, true, "Play Mode ended; partial report preserved.");
                return;
            }
            if (shoreRibbonAuditCancelRequested && !shoreRibbonAuditReadbackPending)
            {
                FinishShoreRibbonBehaviorSuite(false, true, "Cancelled by user; partial report preserved.");
                return;
            }

            bool resourcesReady = AreCellSpawnerAuditResourcesReady(out string readinessFailure);
            if (!resourcesReady)
            {
                shoreRibbonAuditPhase = "Runtime readiness";
                shoreRibbonAuditCurrentCase = readinessFailure;
                double wait = Time.realtimeSinceStartupAsDouble - shoreRibbonAuditWaitStartedAt;
                if (initializationPhase == InitializationPhase.CachePreparationRequired ||
                    initializationPhase == InitializationPhase.Failed ||
                    wait >= ShoreRibbonAuditReadyTimeoutSeconds)
                {
                    FinishShoreRibbonBehaviorSuite(false, false,
                        $"Runtime readiness failed after {wait:0.0}s: {readinessFailure}");
                }
                RepaintShoreRibbonAuditViews();
                return;
            }
            shoreRibbonAuditWaitStartedAt = 0.0;
            // LateUpdate invokes diagnostics once before and once after the
            // runtime-ready branch. Advance behavior cases only from the latter
            // call so one material tick cannot be dispatched twice in a frame.
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
                    FinishShoreRibbonBehaviorSuite(false, false,
                        "Control-authority preflight failed. No geometry cases were run.");
                    return;
                }
                shoreRibbonAuditCompletedCheckpoints++;
                shoreRibbonAuditCaseIndex = 0;
                if (!BeginShoreRibbonAuditCase())
                {
                    FinishShoreRibbonBehaviorSuite(false, false,
                        "Failed to prepare the first deterministic Shore Ribbon case.");
                }
                return;
            }

            if (shoreRibbonAuditCaseIndex >= shoreRibbonAuditCases.Count)
            {
                FinishShoreRibbonBehaviorSuite(shoreRibbonAuditFailCount == 0, false,
                    shoreRibbonAuditFailCount == 0
                        ? "All dedicated Shore Ribbon behavior cases completed."
                        : "One or more Shore Ribbon behavior contracts failed.");
                return;
            }

            AdvanceShoreRibbonAuditTick();
            RepaintShoreRibbonAuditViews();
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
                if (!e.Active) continue;
                activeTotal++;
                switch (e.Type)
                {
                    case AutomaticFoamSourceEventType.ShoreRibbon: activeShore++; break;
                    case AutomaticFoamSourceEventType.InwardWash: activeInward++; break;
                    case AutomaticFoamSourceEventType.ObjectContactArc:
                    case AutomaticFoamSourceEventType.ObjectContactSemiArc:
                    case AutomaticFoamSourceEventType.ObjectContactFleck: activeObject++; break;
                    default: activeFree++; break;
                }
            }

            bool shoreActive = river.FoamAutomaticShoreBirthActive;
            bool objectInactive = !river.FoamAutomaticObjectBirthActive;
            bool freeInactive = !river.FoamAutomaticFreeWaterBirthActive;
            bool pureRibbon = river.FoamShoreFoamPattern == StylizedRiverFoamShorePattern.ShoreRibbons ||
                (river.FoamShoreRibbonPatternWeight > 0.999f && river.FoamInwardWashPatternWeight < 0.001f);
            bool activeTypesAuthoritative = activeInward == 0 && activeObject == 0 && activeFree == 0;
            bool pass = shoreActive && objectInactive && freeInactive && pureRibbon && activeTypesAuthoritative;

            shoreRibbonAuditText.AppendLine("CONTROL-AUTHORITY PREFLIGHT");
            shoreRibbonAuditText.AppendLine($"Automatic birth: {river.FoamAutomaticBirthEnabled}");
            shoreRibbonAuditText.AppendLine($"Shore active: {shoreActive}; coverage={river.FoamShoreFoamCoverage:0.###}; activity={river.FoamShoreFoamActivity:0.###}");
            shoreRibbonAuditText.AppendLine($"Pattern={river.FoamShoreFoamPattern}; ribbonWeight={river.FoamShoreRibbonPatternWeight:0.###}; inwardWeight={river.FoamInwardWashPatternWeight:0.###}");
            shoreRibbonAuditText.AppendLine($"Object active: {river.FoamAutomaticObjectBirthActive}; Free-water active: {river.FoamAutomaticFreeWaterBirthActive}");
            shoreRibbonAuditText.AppendLine($"Live active events: total={activeTotal}, ribbon={activeShore}, inward={activeInward}, object={activeObject}, free={activeFree}");
            shoreRibbonAuditText.AppendLine("CONTROL-AUTHORITY VERDICT: " + (pass ? "PASS" : "FAIL"));
            shoreRibbonAuditText.AppendLine();
            shoreRibbonAuditLastResult = pass ? "Control authority PASS" : "Control authority FAIL";
            if (pass) shoreRibbonAuditPassCount++; else shoreRibbonAuditFailCount++;
        }

        private bool BeginShoreRibbonAuditCase()
        {
            ReleaseShoreRibbonAuditCaseResources();
            if (shoreRibbonAuditCaseIndex < 0 || shoreRibbonAuditCaseIndex >= shoreRibbonAuditCases.Count)
            {
                return false;
            }

            ShoreRibbonAuditCase test = shoreRibbonAuditCases[shoreRibbonAuditCaseIndex];
            CellSpawnerAuditCase geometryCase = new CellSpawnerAuditCase(0, 0, 0);
            shoreRibbonAuditEvent = BuildCellSpawnerAuditEvent(
                geometryCase, test.LengthCells, 1f, 1f, 1f,
                false, 0f, 0f, 0f);
            shoreRibbonAuditEvent.EventId = 910000 + shoreRibbonAuditCaseIndex;
            shoreRibbonAuditEvent.Type = AutomaticFoamSourceEventType.ShoreRibbon;
            shoreRibbonAuditEvent.SideSign = test.SideSign;
            shoreRibbonAuditEvent.BodyLengthCells = test.LengthCells;
            shoreRibbonAuditEvent.BodyWidthCells = 1f;
            shoreRibbonAuditEvent.HeadLengthCells = 1f;
            shoreRibbonAuditEvent.HeadWidthCells = 1f;
            shoreRibbonAuditEvent.WidthMetres = 1f;
            shoreRibbonAuditEvent.ShoreInsetMetres = 0f;
            shoreRibbonAuditEvent.SourceAmount = 1f;
            shoreRibbonAuditEvent.RemainingLife = 1f;
            shoreRibbonAuditEvent.FormationSpeedMetresPerSecond =
                test.SpeedCellsPerSecond * Mathf.Max(0.01f, gridDescriptor.ResolvedDxMetres);
            shoreRibbonAuditEvent.RevealPathDistanceMetres =
                test.LengthCells * Mathf.Max(0.01f, gridDescriptor.ResolvedDxMetres);
            shoreRibbonAuditEvent.Duration = test.LengthCells / Mathf.Max(0.01f, test.SpeedCellsPerSecond);
            shoreRibbonAuditEvent.Elapsed = 0f;
            shoreRibbonAuditEvent.HeadTrailMetres = Mathf.Max(0.01f, gridDescriptor.ResolvedDxMetres);

            if (!TryResolveAutomaticSourceDispatchRange(shoreRibbonAuditEvent, out shoreRibbonAuditRange))
            {
                RecordShoreRibbonAuditPreparationFailure(test, "dispatch range resolution failed");
                return false;
            }

            shoreRibbonAuditStateTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_State");
            shoreRibbonAuditBirthTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Birth");
            shoreRibbonAuditBoundaryTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Boundary");
            shoreRibbonAuditObstacleTexture = CreateObstacleExclusionTexture("PS3D_ShoreRibbonBehavior_Obstacle");
            shoreRibbonAuditShoreTexture = CreateShoreEdgesTexture("PS3D_ShoreRibbonBehavior_Shore");
            shoreRibbonAuditObjectTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Object");
            shoreRibbonAuditLifecycleInputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Lifecycle_Input");
            shoreRibbonAuditLifecycleOutputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Lifecycle_Output");
            shoreRibbonAuditTransportInputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Transport_Input");
            shoreRibbonAuditTransportOutputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Transport_Output");
            shoreRibbonAuditCombinedInputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Combined_Input");
            shoreRibbonAuditCombinedOutputTexture = CreateFieldTexture("PS3D_ShoreRibbonBehavior_Combined_Output");
            ClearRenderTexture(shoreRibbonAuditStateTexture);
            ClearRenderTexture(shoreRibbonAuditBirthTexture);
            ClearCellSpawnerAuditTexture(shoreRibbonAuditBoundaryTexture, Color.white);
            ClearCellSpawnerAuditTexture(shoreRibbonAuditObstacleTexture, Color.black);
            ClearCellSpawnerAuditTexture(shoreRibbonAuditObjectTexture, new Color(1f, -1f, 0f, 1f));
            if (!TryPopulateCellSpawnerSyntheticShoreEdges(
                    shoreRibbonAuditShoreTexture, shoreRibbonAuditEvent,
                    shoreRibbonAuditRange, out string fixtureFailure))
            {
                RecordShoreRibbonAuditPreparationFailure(test, fixtureFailure);
                return false;
            }

            shoreRibbonAuditEventBuffer = new ComputeBuffer(1,
                Marshal.SizeOf<FoamSourceEventGpuData>(), ComputeBufferType.Structured);
            shoreRibbonAuditCounterBuffer = new ComputeBuffer(
                AutomaticBirthDebugCounterCount, sizeof(uint), ComputeBufferType.Structured);
            shoreRibbonAuditCounterBuffer.SetData(new uint[AutomaticBirthDebugCounterCount]);
            shoreRibbonAuditTickIndex = 0;
            shoreRibbonAuditNextCheckpointCell = 0;
            shoreRibbonAuditElapsed = 0f;
            shoreRibbonAuditPreviousElapsed = 0f;
            shoreRibbonAuditLastSupportMinX = float.NaN;
            shoreRibbonAuditLastSupportMaxX = float.NaN;
            shoreRibbonAuditLastInternalGapCount = 0;
            shoreRibbonAuditCurrentCase = $"{shoreRibbonAuditCaseIndex + 1}/{shoreRibbonAuditCases.Count} · {test.Name}";
            shoreRibbonAuditPhase = "Production raster ticks";
            return true;
        }

        private void AdvanceShoreRibbonAuditTick()
        {
            ShoreRibbonAuditCase test = shoreRibbonAuditCases[shoreRibbonAuditCaseIndex];
            float normalDelta = 1f / ShoreRibbonAuditMaterialHz;
            float delta = normalDelta;
            if (test.DelayedTick && shoreRibbonAuditTickIndex == 8)
            {
                delta = 3.5f / Mathf.Max(0.01f, test.SpeedCellsPerSecond);
            }

            shoreRibbonAuditPreviousElapsed = shoreRibbonAuditElapsed;
            shoreRibbonAuditElapsed = Mathf.Min(
                shoreRibbonAuditEvent.Duration,
                shoreRibbonAuditElapsed + delta);
            shoreRibbonAuditEvent.Elapsed = shoreRibbonAuditElapsed;
            DispatchShoreRibbonAuditProductionTick(shoreRibbonAuditPreviousElapsed);
            shoreRibbonAuditTickIndex++;

            float progressCells = test.LengthCells * Mathf.Clamp01(
                shoreRibbonAuditElapsed / Mathf.Max(0.0001f, shoreRibbonAuditEvent.Duration));
            bool final = shoreRibbonAuditElapsed >= shoreRibbonAuditEvent.Duration - 0.00001f;
            bool checkpoint = final || progressCells + 0.0001f >= shoreRibbonAuditNextCheckpointCell;
            if (!checkpoint)
            {
                return;
            }

            int checkpointIndex = shoreRibbonAuditNextCheckpointCell;
            while (shoreRibbonAuditNextCheckpointCell <= Mathf.FloorToInt(progressCells + 0.0001f))
            {
                shoreRibbonAuditNextCheckpointCell++;
            }
            ShoreRibbonAuditCapture capture = new ShoreRibbonAuditCapture
            {
                Generation = shoreRibbonAuditGeneration,
                CaseIndex = shoreRibbonAuditCaseIndex,
                Checkpoint = checkpointIndex,
                ProgressCells = progressCells,
                PreviousProgressCells = test.LengthCells * Mathf.Clamp01(
                    shoreRibbonAuditPreviousElapsed / Mathf.Max(0.0001f, shoreRibbonAuditEvent.Duration)),
                Final = final,
                PendingReadbacks = 4
            };
            PrepareShoreRibbonAuditStageBranches(1f / ShoreRibbonAuditMaterialHz);
            shoreRibbonAuditReadbackPending = true;
            RequestShoreRibbonStageReadback(capture, shoreRibbonAuditStateTexture, 0);
            RequestShoreRibbonStageReadback(capture, shoreRibbonAuditLifecycleOutputTexture, 1);
            RequestShoreRibbonStageReadback(capture, shoreRibbonAuditTransportOutputTexture, 2);
            RequestShoreRibbonStageReadback(capture, shoreRibbonAuditCombinedOutputTexture, 3);
        }

        private void DispatchShoreRibbonAuditProductionTick(float previousElapsed)
        {
            FoamSourceEventGpuData gpuData = BuildAutomaticFoamSourceGpuData(
                shoreRibbonAuditEvent, previousElapsed);
            shoreRibbonAuditEventBuffer.SetData(new[] { gpuData });
            ClearRenderTexture(shoreRibbonAuditBirthTexture);
            shoreRibbonAuditCounterBuffer.SetData(new uint[AutomaticBirthDebugCounterCount]);

            ConfigureGridDescriptorComputeParameters();
            int kernel = rasterizeFoamSourceEventDebugKernel;
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamValidLength", validFieldLength);
            computeShader.SetFloat("_FoamSimulationLength", simulationFieldLength);
            computeShader.SetFloat("_FoamGlobalStart", river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetInt("_FoamRangeStart", shoreRibbonAuditRange.StartX);
            computeShader.SetInt("_FoamRangeCount", shoreRibbonAuditRange.CountX);
            computeShader.SetInt("_FoamRangeStartY", shoreRibbonAuditRange.StartY);
            computeShader.SetInt("_FoamRangeCountY", shoreRibbonAuditRange.CountY);
            computeShader.SetInt("_FoamSourceEventIndex", 0);
            computeShader.SetInt("_FoamSourceEventDebugComponentMode", 0);
            computeShader.SetBuffer(kernel, "_FoamMetricRows", metricBuffer);
            computeShader.SetBuffer(kernel, "_FoamSourceEvents", shoreRibbonAuditEventBuffer);
            computeShader.SetTexture(kernel, "_FoamBoundary", shoreRibbonAuditBoundaryTexture);
            computeShader.SetTexture(kernel, "_FoamObstacleExclusionRead", shoreRibbonAuditObstacleTexture);
            computeShader.SetTexture(kernel, "_FoamCurrentShoreEdgesRead", shoreRibbonAuditShoreTexture);
            Texture neutral = neutralDisturbanceTexture != null
                ? (Texture)neutralDisturbanceTexture
                : Texture2D.blackTexture;
            computeShader.SetInts("_FoamStaticPressureDimensions", 1, 1);
            computeShader.SetTexture(kernel, "_FoamStaticPressureField", neutral);
            computeShader.SetTexture(kernel, "_FoamObjectContactFieldRead", shoreRibbonAuditObjectTexture);
            computeShader.SetTexture(kernel, "_FoamStateWrite", shoreRibbonAuditStateTexture);
            computeShader.SetTexture(kernel, "_FoamBirthDebugWrite", shoreRibbonAuditBirthTexture);
            computeShader.SetBuffer(kernel, "_FoamBirthDebugCounters", shoreRibbonAuditCounterBuffer);
            Dispatch(kernel, shoreRibbonAuditRange.CountX, shoreRibbonAuditRange.CountY);
        }

        private void PrepareShoreRibbonAuditStageBranches(float deltaTime)
        {
            Graphics.CopyTexture(shoreRibbonAuditStateTexture, shoreRibbonAuditLifecycleInputTexture);
            Graphics.CopyTexture(shoreRibbonAuditStateTexture, shoreRibbonAuditTransportInputTexture);
            Graphics.CopyTexture(shoreRibbonAuditStateTexture, shoreRibbonAuditCombinedInputTexture);
            DispatchShoreRibbonAuditSimulationBranch(
                shoreRibbonAuditLifecycleInputTexture,
                shoreRibbonAuditLifecycleOutputTexture,
                0f,
                deltaTime);
            DispatchShoreRibbonAuditSimulationBranch(
                shoreRibbonAuditTransportInputTexture,
                shoreRibbonAuditTransportOutputTexture,
                deltaTime,
                0f);
            DispatchShoreRibbonAuditSimulationBranch(
                shoreRibbonAuditCombinedInputTexture,
                shoreRibbonAuditCombinedOutputTexture,
                deltaTime,
                deltaTime);
        }

        private void DispatchShoreRibbonAuditSimulationBranch(
            RenderTexture input,
            RenderTexture output,
            float transportDeltaTime,
            float lifecycleDeltaTime)
        {
            ConfigureSharedComputeParameters(transportDeltaTime, lifecycleDeltaTime);
            ConfigureTransportSubstepDiagnostics(false);
            computeShader.SetInt("_FoamBulkTransportIntegerShift", 0);
            computeShader.SetTexture(simulateKernel, "_FoamBoundary", shoreRibbonAuditBoundaryTexture);
            computeShader.SetTexture(simulateKernel, "_FoamObstacleExclusionRead", shoreRibbonAuditObstacleTexture);
            computeShader.SetTexture(simulateKernel, "_FoamStateRead", input);
            computeShader.SetTexture(simulateKernel, "_FoamStateWrite", output);
            computeShader.SetInt("_FoamRangeStart", 0);
            computeShader.SetInt("_FoamRangeCount", fieldWidth);
            Dispatch(simulateKernel, fieldWidth, fieldHeight);
        }

        private void RequestShoreRibbonStageReadback(
            ShoreRibbonAuditCapture capture,
            RenderTexture texture,
            int stageIndex)
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
                    switch (stageIndex)
                    {
                        case 0: capture.BirthData = managed; break;
                        case 1: capture.LifecycleData = managed; break;
                        case 2: capture.TransportData = managed; break;
                        default: capture.CombinedData = managed; break;
                    }
                }
                capture.PendingReadbacks--;
                if (capture.PendingReadbacks <= 0)
                {
                    CompleteShoreRibbonAuditReadbacks(capture);
                }
            });
        }

        private ShoreRibbonStageStats MeasureShoreRibbonStage(ushort[] data)
        {
            double coverageArea = 0.0;
            double presenceArea = 0.0;
            double lifeArea = 0.0;
            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;
            bool[] occupiedColumns = new bool[fieldWidth];
            if (data == null)
            {
                return new ShoreRibbonStageStats(0, 0, 0, -1, -1, -1, -1, 0, 0f, 0f);
            }

            for (int y = 0; y < fieldHeight; y++)
            {
                for (int x = 0; x < fieldWidth; x++)
                {
                    int index = (y * fieldWidth + x) * 4;
                    float amount = Mathf.Max(0f, Mathf.HalfToFloat(data[index]));
                    float lifeMoment = Mathf.Max(0f, Mathf.HalfToFloat(data[index + 1]));
                    float coverage = Mathf.Max(0f, Mathf.HalfToFloat(data[index + 3]));
                    if (coverage <= ShoreRibbonAuditCoverageThreshold) continue;
                    float presence = amount / Mathf.Max(coverage, 0.000001f);
                    float life = amount > 0.000001f ? lifeMoment / amount : 0f;
                    coverageArea += coverage;
                    presenceArea += coverage * Mathf.Clamp01(presence);
                    lifeArea += coverage * Mathf.Clamp01(life);
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                    occupiedColumns[x] = true;
                }
            }

            int internalGaps = 0;
            if (minX <= maxX)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (!occupiedColumns[x]) internalGaps++;
                }
            }
            float longitudinalSpan = minX <= maxX
                ? (maxX - minX + 1f) * Mathf.Max(0.0001f, gridDescriptor.ResolvedDxMetres)
                : 0f;
            float lateralSpan = minY <= maxY
                ? Mathf.Abs(gridDescriptor.ResolveLateralMetresAtRowCentre(maxY) -
                    gridDescriptor.ResolveLateralMetresAtRowCentre(minY)) +
                    Mathf.Max(0.0001f, gridDescriptor.ResolvedDyMetres)
                : 0f;
            return new ShoreRibbonStageStats(
                coverageArea, presenceArea, lifeArea,
                minX <= maxX ? minX : -1,
                minX <= maxX ? maxX : -1,
                minY <= maxY ? minY : -1,
                minY <= maxY ? maxY : -1,
                internalGaps, longitudinalSpan, lateralSpan);
        }

        private void AppendShoreRibbonStageRow(
            ShoreRibbonAuditCapture capture,
            ShoreRibbonAuditCase test,
            string stage,
            ShoreRibbonStageStats stats,
            ShoreRibbonStageStats birth,
            bool pass,
            string detail)
        {
            double coverageRetention = birth.Coverage > 0.000001
                ? stats.Coverage / birth.Coverage : 1.0;
            double presenceRetention = birth.Presence > 0.000001
                ? stats.Presence / birth.Presence : 1.0;
            double lifeRetention = birth.Life > 0.000001
                ? stats.Life / birth.Life : 1.0;
            shoreRibbonAuditText.AppendLine(
                $"    {stage}: {(pass ? "PASS" : "FAIL")} · coverage={stats.Coverage:0.###}; " +
                $"presence={stats.Presence:0.###}; life={stats.Life:0.###}; " +
                $"gaps={stats.InternalGaps}; metricAlong={stats.LongitudinalSpanMetres:0.###}m; " +
                $"metricAcross={stats.LateralSpanMetres:0.###}m; ratio={stats.DirectionRatio:0.###}; " +
                $"retention C/P/L={coverageRetention:0.###}/{presenceRetention:0.###}/{lifeRetention:0.###}; {detail}");
            shoreRibbonAuditCsv.AppendLine(string.Join(",",
                "checkpoint",
                Csv(test.Name),
                test.SideSign.ToString("0", CultureInfo.InvariantCulture),
                test.SpeedCellsPerSecond.ToString("0.###", CultureInfo.InvariantCulture),
                test.LengthCells.ToString(CultureInfo.InvariantCulture),
                capture.Checkpoint.ToString(CultureInfo.InvariantCulture),
                capture.ProgressCells.ToString("0.###", CultureInfo.InvariantCulture),
                Csv(stage),
                stats.Coverage.ToString("0.######", CultureInfo.InvariantCulture),
                stats.Presence.ToString("0.######", CultureInfo.InvariantCulture),
                stats.Life.ToString("0.######", CultureInfo.InvariantCulture),
                stats.MinX.ToString(CultureInfo.InvariantCulture),
                stats.MaxX.ToString(CultureInfo.InvariantCulture),
                stats.MinY.ToString(CultureInfo.InvariantCulture),
                stats.MaxY.ToString(CultureInfo.InvariantCulture),
                stats.InternalGaps.ToString(CultureInfo.InvariantCulture),
                stats.LongitudinalSpanMetres.ToString("0.######", CultureInfo.InvariantCulture),
                stats.LateralSpanMetres.ToString("0.######", CultureInfo.InvariantCulture),
                stats.DirectionRatio.ToString("0.######", CultureInfo.InvariantCulture),
                coverageRetention.ToString("0.######", CultureInfo.InvariantCulture),
                presenceRetention.ToString("0.######", CultureInfo.InvariantCulture),
                lifeRetention.ToString("0.######", CultureInfo.InvariantCulture),
                pass ? "1" : "0",
                Csv(detail)));
        }

        private void CompleteShoreRibbonAuditReadbacks(ShoreRibbonAuditCapture capture)
        {
            if (capture.Generation != shoreRibbonAuditGeneration ||
                !shoreRibbonBehaviorAuditRunning)
            {
                return;
            }
            shoreRibbonAuditReadbackPending = false;
            if (capture.ReadbackError || capture.BirthData == null ||
                capture.LifecycleData == null || capture.TransportData == null ||
                capture.CombinedData == null)
            {
                shoreRibbonAuditFailCount++;
                shoreRibbonAuditLastResult = "AsyncGPUReadback failed";
                shoreRibbonAuditText.AppendLine($"{shoreRibbonAuditCurrentCase}: FAIL · one or more stage readbacks failed.");
                FinishShoreRibbonBehaviorSuite(false, false, "GPU stage readback failed.");
                return;
            }

            ShoreRibbonAuditCase test = shoreRibbonAuditCases[capture.CaseIndex];
            ShoreRibbonStageStats birth = MeasureShoreRibbonStage(capture.BirthData);
            ShoreRibbonStageStats lifecycle = MeasureShoreRibbonStage(capture.LifecycleData);
            ShoreRibbonStageStats transport = MeasureShoreRibbonStage(capture.TransportData);
            ShoreRibbonStageStats combined = MeasureShoreRibbonStage(capture.CombinedData);

            bool nondecreasing = float.IsNaN(shoreRibbonAuditLastSupportMaxX) ||
                birth.MaxX + 0.01f >= shoreRibbonAuditLastSupportMaxX;
            bool birthContinuity = birth.InternalGaps == 0;
            bool birthDirection = capture.ProgressCells < 4f || birth.DirectionRatio >= 1.25f;
            bool birthMaterial = birth.Coverage <= 0.001 ||
                (birth.Presence > 0.001 && birth.Life > 0.001);
            bool birthPass = birthContinuity && birthDirection && birthMaterial && nondecreasing;
            bool lifecyclePass = lifecycle.InternalGaps == 0 &&
                lifecycle.Coverage >= birth.Coverage * 0.98 &&
                lifecycle.Presence >= birth.Presence * 0.98 &&
                lifecycle.Life > 0.001;
            bool transportPass = transport.InternalGaps == 0 &&
                transport.Presence >= birth.Presence * 0.80;
            bool combinedPass = combined.InternalGaps == 0 &&
                combined.Presence >= birth.Presence * 0.75 &&
                combined.Life > 0.001;
            bool pass = birthPass && lifecyclePass && transportPass && combinedPass;

            shoreRibbonAuditLastSupportMinX = birth.MinX;
            shoreRibbonAuditLastSupportMaxX = birth.MaxX;
            shoreRibbonAuditLastInternalGapCount = birth.InternalGaps;
            shoreRibbonAuditText.AppendLine(
                $"{capture.CaseIndex + 1:00}.{capture.Checkpoint:00} {test.Name} · " +
                $"progress={capture.ProgressCells:0.###} cells — {(pass ? "PASS" : "FAIL")}");
            AppendShoreRibbonStageRow(capture, test, "BIRTH", birth, birth, birthPass,
                "accumulated repeated production source raster");
            AppendShoreRibbonStageRow(capture, test, "LIFECYCLE_ONLY", lifecycle, birth, lifecyclePass,
                "transport dt=0; one 8 Hz lifecycle step");
            AppendShoreRibbonStageRow(capture, test, "TRANSPORT_ONLY", transport, birth, transportPass,
                "lifecycle dt=0; one 8 Hz transport step");
            AppendShoreRibbonStageRow(capture, test, "COMBINED", combined, birth, combinedPass,
                "one production-order simulation step before the next birth merge");

            shoreRibbonAuditCompletedCheckpoints++;
            if (pass) shoreRibbonAuditPassCount++; else shoreRibbonAuditFailCount++;
            shoreRibbonAuditLastResult = $"{(pass ? "PASS" : "FAIL")} · {test.Name} · {capture.ProgressCells:0.##} cells";

            if (capture.Final)
            {
                bool completionPass = Mathf.Abs(shoreRibbonAuditEvent.Elapsed - shoreRibbonAuditEvent.Duration) <= 0.0001f;
                shoreRibbonAuditText.AppendLine(
                    $"COMPLETION: elapsed={shoreRibbonAuditEvent.Elapsed:0.###}/{shoreRibbonAuditEvent.Duration:0.###}s; " +
                    $"resolvedLength={test.LengthCells} cells; event would despawn on this production completion boundary; " +
                    $"verdict={(completionPass ? "PASS" : "FAIL")}");
                shoreRibbonAuditText.AppendLine();
                if (!completionPass) shoreRibbonAuditFailCount++;
                shoreRibbonAuditCaseIndex++;
                if (shoreRibbonAuditCaseIndex < shoreRibbonAuditCases.Count)
                {
                    if (!BeginShoreRibbonAuditCase())
                    {
                        FinishShoreRibbonBehaviorSuite(false, false,
                            "Failed to prepare the next deterministic case.");
                    }
                }
            }
            RepaintShoreRibbonAuditViews(true);
        }

        private static string Csv(string value)
        {
            value ??= string.Empty;
            return '"' + value.Replace("\"", "\"\"") + '"';
        }

        private void RecordShoreRibbonAuditPreparationFailure(
            ShoreRibbonAuditCase test, string reason)
        {
            shoreRibbonAuditFailCount++;
            shoreRibbonAuditLastResult = "Preparation FAIL · " + reason;
            shoreRibbonAuditText.AppendLine($"{test.Name}: PREPARATION FAIL · {reason}");
        }

        private void FinishShoreRibbonBehaviorSuite(bool success, bool cancelled, string reason)
        {
            if (!shoreRibbonBehaviorAuditRunning) return;
            shoreRibbonBehaviorAuditRunning = false;
            shoreRibbonAuditReadbackPending = false;
            ReleaseShoreRibbonAuditCaseResources();
            double elapsed = Math.Max(0.0,
                Time.realtimeSinceStartupAsDouble - shoreRibbonAuditStartedAt);
            shoreRibbonAuditText.AppendLine("SUMMARY");
            shoreRibbonAuditText.AppendLine($"Completed checkpoints: {shoreRibbonAuditCompletedCheckpoints}/{shoreRibbonAuditTotalCheckpoints}");
            shoreRibbonAuditText.AppendLine($"Passed observations: {shoreRibbonAuditPassCount}");
            shoreRibbonAuditText.AppendLine($"Failed observations: {shoreRibbonAuditFailCount}");
            shoreRibbonAuditText.AppendLine($"Elapsed: {elapsed:0.000} s");
            shoreRibbonAuditText.AppendLine($"Reason: {reason}");
            shoreRibbonAuditText.AppendLine($"Outcome: {(cancelled ? "CANCELLED" : success ? "PASS" : "FAIL")}");
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string directory = Path.Combine(projectRoot, "Library", "RiverFoam");
            Directory.CreateDirectory(directory);
            string textPath = Path.Combine(directory, "ShoreRibbonBehaviorSuite.txt");
            string csvPath = Path.Combine(directory, "ShoreRibbonBehaviorSuite.csv");
            File.WriteAllText(textPath, shoreRibbonAuditText.ToString());
            File.WriteAllText(csvPath, shoreRibbonAuditCsv.ToString());
            topologyCacheDiagnosticReport = shoreRibbonAuditText.ToString();
            topologyCacheDiagnosticReportPath = textPath;
            topologyCacheDiagnosticState = cancelled ? "Cancelled" : success ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary =
                $"Shore Ribbon Behavior Suite {(cancelled ? "cancelled" : success ? "passed" : "failed")} · " +
                $"{shoreRibbonAuditCompletedCheckpoints}/{shoreRibbonAuditTotalCheckpoints} checkpoints · " +
                $"PASS {shoreRibbonAuditPassCount} · FAIL {shoreRibbonAuditFailCount} · {elapsed:0.0}s";
            shoreRibbonAuditPhase = cancelled ? "Cancelled" : success ? "Complete" : "Failed";
            shoreRibbonAuditCurrentCase = "None";
            RepaintShoreRibbonAuditViews(true);
        }

        private void ReleaseShoreRibbonAuditCaseResources()
        {
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditStateTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditBirthTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditBoundaryTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditObstacleTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditShoreTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditObjectTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditLifecycleInputTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditLifecycleOutputTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditTransportInputTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditTransportOutputTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditCombinedInputTexture);
            ReleaseCellSpawnerAuditTexture(shoreRibbonAuditCombinedOutputTexture);
            shoreRibbonAuditStateTexture = null;
            shoreRibbonAuditBirthTexture = null;
            shoreRibbonAuditBoundaryTexture = null;
            shoreRibbonAuditObstacleTexture = null;
            shoreRibbonAuditShoreTexture = null;
            shoreRibbonAuditObjectTexture = null;
            shoreRibbonAuditLifecycleInputTexture = null;
            shoreRibbonAuditLifecycleOutputTexture = null;
            shoreRibbonAuditTransportInputTexture = null;
            shoreRibbonAuditTransportOutputTexture = null;
            shoreRibbonAuditCombinedInputTexture = null;
            shoreRibbonAuditCombinedOutputTexture = null;
            shoreRibbonAuditEventBuffer?.Release();
            shoreRibbonAuditCounterBuffer?.Release();
            shoreRibbonAuditEventBuffer = null;
            shoreRibbonAuditCounterBuffer = null;
        }

        private void RepaintShoreRibbonAuditViews(bool force = false)
        {
            double now = EditorApplication.timeSinceStartup;
            if (!force && now - shoreRibbonAuditLastRepaintAt < 0.10) return;
            shoreRibbonAuditLastRepaintAt = now;
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            SceneView.RepaintAll();
        }
    }
}
#endif
