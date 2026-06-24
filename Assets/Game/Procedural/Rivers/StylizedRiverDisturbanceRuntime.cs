using System;
using System.Collections.Generic;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct GeneratedRiverDisturbanceDiagnostics
    {
        public GeneratedRiverDisturbanceDiagnostics(
            StylizedRiver river,
            bool active,
            float acrossWidth,
            float alongLength,
            float localRiverWidth,
            float blockageRatio,
            float effectivePadding,
            float effectiveAmplitude,
            float effectiveWakeStrength,
            float maximumAllowedAmplitude,
            bool heightClampReached,
            float representativeSupportHeight,
            float pressureMinimumHeight,
            float pressureMaximumHeight,
            float pressureStrength,
            float waveAllowance,
            string status)
        {
            River = river;
            Active = active;
            AcrossWidth = acrossWidth;
            AlongLength = alongLength;
            LocalRiverWidth = localRiverWidth;
            BlockageRatio = blockageRatio;
            EffectivePadding = effectivePadding;
            EffectiveAmplitude = effectiveAmplitude;
            EffectiveWakeStrength = effectiveWakeStrength;
            MaximumAllowedAmplitude = maximumAllowedAmplitude;
            HeightClampReached = heightClampReached;
            RepresentativeSupportHeight = representativeSupportHeight;
            PressureMinimumHeight = pressureMinimumHeight;
            PressureMaximumHeight = pressureMaximumHeight;
            PressureStrength = pressureStrength;
            WaveAllowance = waveAllowance;
            Status = status ?? string.Empty;
        }

        public StylizedRiver River { get; }
        public bool Active { get; }
        public float AcrossWidth { get; }
        public float AlongLength { get; }
        public float LocalRiverWidth { get; }
        public float BlockageRatio { get; }
        public float EffectivePadding { get; }
        public float EffectiveAmplitude { get; }
        public float EffectiveWakeStrength { get; }
        public float MaximumAllowedAmplitude { get; }
        public bool HeightClampReached { get; }
        public float RepresentativeSupportHeight { get; }
        public float PressureMinimumHeight { get; }
        public float PressureMaximumHeight { get; }
        public float PressureStrength { get; }
        public float WaveAllowance { get; }
        public string Status { get; }
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    [AddComponentMenu("")]
    public sealed class StylizedRiverDisturbanceRuntime : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_RiverDisturbance";
        private const float ChunkLengthMetres = 32f;
        private const int ThreadGroupSize = 8;
        private const float StationarySpeedStart = 0.08f;
        private const float MovingSpeedFull = 0.45f;
        private const double SourceStaleSeconds = 0.35;
        private const float StaticOnlySimulationRate = 12f;
        public const float MaximumStaticPressureHeightMetres = 1.25f;
        private const float MaximumStaticPressureModulation = 1.16f;
        private const float RippleStabilitySafety = 0.42f;
        private const int MaximumRippleSubsteps = 32;
        private const float GoldenPhaseStep = 0.61803398875f;
        private const float AutomaticBoundsHorizontalPadding = 0.5f;
        private const float AutomaticBoundsVerticalPadding = 1.25f;
        private const int GeneratedSourcesPerFrame = 1;
        private const int MaximumStaticContourPoints =
            RiverDisturbanceFootprintResolver.MaximumContourPoints;

        private static uint sourcePhaseSequence = 1;

        private static readonly List<StylizedRiverDisturbanceRuntime>
            ActiveRuntimes = new();
        private static readonly Dictionary<EntityId, GeneratedRiverDisturbanceDiagnostics>
            GeneratedSourceDiagnostics = new();

        private static readonly int DisturbanceEnabledId =
            Shader.PropertyToID("_DisturbanceEnabled");
        private static readonly int DisturbancePreviousId =
            Shader.PropertyToID("_DisturbanceFieldPrevious");
        private static readonly int DisturbanceCurrentId =
            Shader.PropertyToID("_DisturbanceFieldCurrent");
        private static readonly int DisturbanceStaticTargetId =
            Shader.PropertyToID("_DisturbanceStaticTarget");
        private static readonly int DisturbanceStaticWakeSourceId =
            Shader.PropertyToID("_DisturbanceStaticWakeSource");
        private static readonly int DisturbanceWakePreviousId =
            Shader.PropertyToID("_DisturbanceWakePrevious");
        private static readonly int DisturbanceWakeCurrentId =
            Shader.PropertyToID("_DisturbanceWakeCurrent");
        private static readonly int DisturbanceWakeInterpolationId =
            Shader.PropertyToID("_DisturbanceWakeInterpolation");
        private static readonly int DisturbanceInterpolationId =
            Shader.PropertyToID("_DisturbanceInterpolation");
        private static readonly int DisturbanceGlobalStartId =
            Shader.PropertyToID("_DisturbanceGlobalStart");
        private static readonly int DisturbanceFieldLengthId =
            Shader.PropertyToID("_DisturbanceFieldLength");
        private static readonly int DisturbanceGeometryStrengthId =
            Shader.PropertyToID("_DisturbanceGeometryStrength");
        private static readonly int DisturbanceNormalStrengthId =
            Shader.PropertyToID("_DisturbanceNormalStrength");
        private static readonly int DisturbanceShoreInteractionId =
            Shader.PropertyToID("_DisturbanceShoreInteraction");
        private static readonly int DisturbanceMaximumHeightId =
            Shader.PropertyToID("_DisturbanceMaximumHeight");
        private static readonly int DisturbanceStaticMaximumHeightId =
            Shader.PropertyToID("_DisturbanceStaticMaximumHeight");
        private static readonly int DisturbanceDebugViewId =
            Shader.PropertyToID("_DisturbanceDebugView");
        private static readonly int DisturbanceFragmentDetailId =
            Shader.PropertyToID("_DisturbanceFragmentDetail");

        private readonly Dictionary<EntityId, ContinuousSource> continuousSources =
            new();
        private readonly List<EntityId> staleSourceIds = new();
        private readonly List<ImpactCommand> pendingImpacts = new();
        private readonly List<IGeneratedGeometrySource>
            generatedGeometryScratch = new();
        private readonly HashSet<EntityId> automaticGeneratedSourceIds =
            new();
        private readonly HashSet<EntityId>
            refreshedAutomaticGeneratedSourceIds = new();
        private readonly Vector4[] staticContourUpload =
            new Vector4[MaximumStaticContourPoints];
        private readonly Vector4[] staticPressureProfileUpload =
            new Vector4[RiverDisturbanceFootprintResolver.PressureSupportLateralSamples];

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture staticTarget;
        private RenderTexture staticWakeSource;
        private RenderTexture wakeA;
        private RenderTexture wakeB;
        private RenderTexture currentWake;
        private RenderTexture previousWake;
        private RenderTexture writeWake;
        private RenderTexture currentState;
        private RenderTexture previousState;
        private RenderTexture writeState;
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool[] chunkActive = Array.Empty<bool>();
        private bool[] chunkHasStaticSource = Array.Empty<bool>();
        private double[] wakeChunkActiveUntil = Array.Empty<double>();
        private bool[] wakeChunkActive = Array.Empty<bool>();

        private int clearKernel = -1;
        private int injectRippleKernel = -1;
        private int injectWakeKernel = -1;
        private int bakeStaticPressureKernel = -1;
        private int finalizeStaticPressureKernel = -1;
        private int bakeStaticWakeSourceKernel = -1;
        private int simulateRippleKernel = -1;
        private int simulateWakeKernel = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int wakeResolutionPerChunk;
        private int wakeFieldWidth;
        private int wakeFieldHeight;
        private int domainVersion = -1;
        private float fieldLength;
        private float averageSurfaceHalfWidth = 1f;
        private float simulationAccumulator;
        private float simulationInterpolation = 1f;
        private float wakeInterpolation = 1f;
        private double lastRuntimeTime;
        private double lastActivityTime;
        private bool supportWarningReported;
        private bool resourcesDirty = true;
        private bool staticTargetDirty = true;
        private int validStaticSourceCount;
        private bool generatedGeometryRegistryDirty = true;
        private bool generatedGeometryRefreshInProgress;
        private int generatedGeometryRefreshIndex;
        private Bounds generatedGeometryRefreshBounds;
        private bool wasFrozen;

        public bool IsSupported =>
            SystemInfo.supportsComputeShaders &&
            SystemInfo.SupportsRenderTextureFormat(
                RenderTextureFormat.ARGBHalf);
        public bool IsAllocated => currentState != null;
        public bool IsSleeping =>
            !HasActiveChunks() &&
            continuousSources.Count == 0 &&
            pendingImpacts.Count == 0;
        public int FieldWidth => fieldWidth;
        public int FieldHeight => fieldHeight;
        public int ChunkCount => chunkCount;
        public int ActiveChunkCount => CountActiveChunks();
        public int ContinuousSourceCount => continuousSources.Count;
        public float SimulationRate => ResolveSimulationRate();
        public long EstimatedMemoryBytes =>
            (long)fieldWidth * fieldHeight * 8L * 3L +
            (long)wakeFieldWidth * wakeFieldHeight * 8L * 3L;

        private struct ContinuousSource
        {
            public Vector3 WorldPosition;
            public float StartDistance;
            public float EndDistance;
            public float StartAcrossNormalized;
            public float EndAcrossNormalized;
            public float AcrossHalfWidth;
            public float AlongHalfLength;
            public float Strength;
            public float GeometryContribution;
            public float NormalContribution;
            public float StaticTargetHeightMetres;
            public float StaticPressureAcrossHalfWidth;
            public float StaticPressureAlongHalfLength;
            public Vector2[] StaticPressureContour;
            public RiverDisturbancePressureBakeProfile StaticPressureProfile;
            public float StaticWakeAmplitude;
            public float StaticResponseStiffness;
            public float StaticWakeReachMultiplier;
            public float StaticUnsteadiness;
            public Vector2[] StaticContour;
            public float MovementSpeed;
            public float Phase;
            public bool IsStatic;
            public bool StationaryObstruction;
            public double LastSeen;
        }

        private struct ImpactCommand
        {
            public float Distance;
            public float AcrossNormalized;
            public float SurfaceHalfWidth;
            public float Radius;
            public float Strength;
            public float GeometryContribution;
            public float NormalContribution;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            ActiveRuntimes.Clear();
            GeneratedSourceDiagnostics.Clear();
            sourcePhaseSequence = 1;
        }

        public static bool TryGetGeneratedSourceDiagnostics(
            IGeneratedGeometrySource source,
            out GeneratedRiverDisturbanceDiagnostics diagnostics)
        {
            diagnostics = default;
            if (source == null)
            {
                return false;
            }

            MeshFilter meshFilter = source.GeometryMeshFilter;
            return meshFilter != null &&
                   GeneratedSourceDiagnostics.TryGetValue(
                       meshFilter.GetEntityId(),
                       out diagnostics);
        }

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            propertyBlock ??= new MaterialPropertyBlock();

            if (!ActiveRuntimes.Contains(this))
            {
                ActiveRuntimes.Add(this);
            }

            if (river != null)
            {
                river.DomainChanged += HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded +=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved +=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged +=
                HandleGeneratedGeometrySourceChanged;

            lastRuntimeTime = Time.realtimeSinceStartupAsDouble;
            resourcesDirty = true;
            generatedGeometryRegistryDirty = true;
            BindDisabled();
        }

        private void OnDisable()
        {
            ActiveRuntimes.Remove(this);

            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            GeneratedGeometryRegistry.SourceAdded -=
                HandleGeneratedGeometrySourceAdded;
            GeneratedGeometryRegistry.SourceRemoved -=
                HandleGeneratedGeometrySourceRemoved;
            GeneratedGeometryRegistry.SourceChanged -=
                HandleGeneratedGeometrySourceChanged;

            RemoveOwnedGeneratedDiagnostics();
            BindDisabled();
            ReleaseResources();
            continuousSources.Clear();
            automaticGeneratedSourceIds.Clear();
            refreshedAutomaticGeneratedSourceIds.Clear();
            generatedGeometryScratch.Clear();
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            pendingImpacts.Clear();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void LateUpdate()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.RuntimeDisturbancesEnabled)
            {
                BindDisabled();
                ReleaseResources();
                return;
            }

            surfaceRenderer = river.SurfaceRenderer;

            if (!Application.isPlaying)
            {
                BindDisabled();
                return;
            }

            if (!IsSupported)
            {
                if (!supportWarningReported)
                {
                    Debug.LogWarning(
                        $"StylizedRiver disturbance field on '{name}' is disabled because compute shaders or ARGBHalf random-write textures are unavailable.",
                        this);
                    supportWarningReported = true;
                }

                BindDisabled();
                return;
            }

            supportWarningReported = false;

            if (river.LiquidFactor <= 0.0001f)
            {
                if (!wasFrozen)
                {
                    ClearField();
                }

                wasFrozen = true;
                BindDisabled();
                return;
            }

            wasFrozen = false;

            if (generatedGeometryRegistryDirty ||
                generatedGeometryRefreshInProgress)
            {
                RefreshGeneratedGeometrySources();
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float deltaTime = Mathf.Clamp(
                (float)(now - lastRuntimeTime),
                0f,
                0.1f);
            lastRuntimeTime = now;

            CleanupStaleSources(now);

            bool requiresField =
                pendingImpacts.Count > 0 ||
                continuousSources.Count > 0 ||
                HasActiveChunks();

            if (!requiresField)
            {
                if (currentState != null &&
                    now - lastActivityTime > 10.0)
                {
                    ReleaseResources();
                }

                BindDisabled();
                return;
            }

            if (!EnsureResources())
            {
                BindDisabled();
                return;
            }

            float interval = 1f / Mathf.Max(1f, ResolveSimulationRate());
            simulationAccumulator = Mathf.Min(
                simulationAccumulator + deltaTime,
                interval * 2.5f);

            int stepCount = 0;
            while (simulationAccumulator >= interval && stepCount < 2)
            {
                SimulateStep(interval, now);
                simulationAccumulator -= interval;
                stepCount++;
            }

            if (simulationAccumulator >= interval)
            {
                simulationAccumulator = 0f;
            }

            simulationInterpolation = Mathf.Clamp01(
                simulationAccumulator / interval);
            wakeInterpolation = simulationInterpolation;

            BindField();
        }

        public void NotifyRiverChanged()
        {
            resourcesDirty = true;
            staticTargetDirty = true;
            generatedGeometryRegistryDirty = true;
        }

        public void ClearField()
        {
            if (computeShader != null)
            {
                DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
                DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
                DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            }

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
            Array.Clear(wakeChunkActive, 0, wakeChunkActive.Length);
            Array.Clear(wakeChunkActiveUntil, 0, wakeChunkActiveUntil.Length);
            pendingImpacts.Clear();
            simulationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
        }

        public bool EmitImpact(
            Vector3 worldPosition,
            float radius,
            float strength,
            float geometryContribution = 1f,
            float normalContribution = 1f)
        {
            if (river == null ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                return false;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));

            pendingImpacts.Add(
                new ImpactCommand
                {
                    Distance = projection.GlobalDistance,
                    AcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    SurfaceHalfWidth = surfaceHalfWidth,
                    Radius = Mathf.Max(0.05f, radius),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution)
                });

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool RegisterStaticSource(
            EntityId sourceId,
            Vector3 worldPosition,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            float targetHeightFraction = -1f,
            float staticWakeAmplitude = -1f,
            float responseStiffness = 1f,
            float wakeReachMultiplier = 1f,
            float unsteadiness = 1f,
            IReadOnlyList<Vector2> contour = null,
            float explicitTargetHeightMetres = -1f,
            float pressureAcrossHalfWidth = -1f,
            float pressureAlongHalfLength = -1f,
            IReadOnlyList<Vector2> pressureContour = null,
            RiverDisturbancePressureBakeProfile pressureProfile = default,
            bool deferStaticTargetRebuild = false)
        {
            if (river == null ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    worldPosition,
                    out StylizedRiverProjection projection) ||
                !projection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(projection.LocalDistance);
            float surfaceHalfWidth = Mathf.Max(
                0.05f,
                sample.GetSurfaceHalfWidth(projection.AcrossMetres));
            float phase = ResolveSourcePhase(sourceId);
            float resolvedHeightMetres = explicitTargetHeightMetres >= 0f
                ? Mathf.Clamp(
                    explicitTargetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres)
                : targetHeightFraction >= 0f
                    ? river.DisturbanceMaximumHeight *
                      Mathf.Clamp01(targetHeightFraction)
                    : Mathf.Clamp(
                        Mathf.Max(0f, strength) *
                        Mathf.Clamp01(geometryContribution) *
                        0.040f,
                        0f,
                        MaximumStaticPressureHeightMetres);
            float resolvedWakeAmplitude = staticWakeAmplitude >= 0f
                ? Mathf.Max(0f, staticWakeAmplitude)
                : Mathf.Max(0f, strength) *
                  Mathf.Clamp01(normalContribution) *
                  0.22f;

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = worldPosition,
                    StartDistance = projection.GlobalDistance,
                    EndDistance = projection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        projection.AcrossMetres / surfaceHalfWidth,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = resolvedHeightMetres,
                    StaticPressureAcrossHalfWidth = pressureAcrossHalfWidth > 0f
                        ? Mathf.Max(0.05f, pressureAcrossHalfWidth)
                        : Mathf.Max(0.05f, acrossHalfWidth),
                    StaticPressureAlongHalfLength = pressureAlongHalfLength > 0f
                        ? Mathf.Max(0.05f, pressureAlongHalfLength)
                        : Mathf.Max(0.05f, alongHalfLength),
                    StaticPressureContour = CopyStaticContour(
                        pressureContour ?? contour),
                    StaticPressureProfile = pressureProfile,
                    StaticWakeAmplitude = resolvedWakeAmplitude,
                    StaticResponseStiffness = Mathf.Clamp(
                        responseStiffness,
                        0f,
                        2f),
                    StaticWakeReachMultiplier = Mathf.Clamp(
                        wakeReachMultiplier,
                        0.25f,
                        3f),
                    StaticUnsteadiness = Mathf.Clamp(
                        unsteadiness,
                        0f,
                        2f),
                    StaticContour = CopyStaticContour(contour),
                    MovementSpeed = 0f,
                    Phase = phase,
                    IsStatic = true,
                    StationaryObstruction = true,
                    LastSeen = double.PositiveInfinity
                };

            if (!deferStaticTargetRebuild)
            {
                staticTargetDirty = true;
            }

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool UpdateContinuousSource(
            EntityId sourceId,
            Vector3 previousWorldPosition,
            Vector3 currentWorldPosition,
            float sampleDeltaTime,
            float acrossHalfWidth,
            float alongHalfLength,
            float strength,
            float geometryContribution,
            float normalContribution,
            bool stationaryObstruction)
        {
            if (river == null ||
                !river.RuntimeDisturbancesEnabled ||
                !river.TryProjectWorldPoint(
                    currentWorldPosition,
                    out StylizedRiverProjection currentProjection) ||
                !currentProjection.IsInside)
            {
                RemoveContinuousSource(sourceId);
                return false;
            }

            bool previousValid =
                river.TryProjectWorldPoint(
                    previousWorldPosition,
                    out StylizedRiverProjection previousProjection) &&
                previousProjection.IsInside;

            if (!previousValid)
            {
                previousProjection = currentProjection;
            }

            StylizedRiverSplineSample currentSample =
                river.SampleAtLocalDistance(
                    currentProjection.LocalDistance);
            StylizedRiverSplineSample previousSample =
                river.SampleAtLocalDistance(
                    previousProjection.LocalDistance);

            float currentSurfaceHalf = Mathf.Max(
                0.05f,
                currentSample.GetSurfaceHalfWidth(
                    currentProjection.AcrossMetres));
            float previousSurfaceHalf = Mathf.Max(
                0.05f,
                previousSample.GetSurfaceHalfWidth(
                    previousProjection.AcrossMetres));

            float riverSpaceTravel = new Vector2(
                currentProjection.GlobalDistance -
                previousProjection.GlobalDistance,
                currentProjection.AcrossMetres -
                previousProjection.AcrossMetres).magnitude;

            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource previousSource) &&
                previousSource.IsStatic)
            {
                staticTargetDirty = true;
            }

            continuousSources[sourceId] =
                new ContinuousSource
                {
                    WorldPosition = currentWorldPosition,
                    StartDistance = previousProjection.GlobalDistance,
                    EndDistance = currentProjection.GlobalDistance,
                    StartAcrossNormalized = Mathf.Clamp(
                        previousProjection.AcrossMetres /
                        previousSurfaceHalf,
                        -1f,
                        1f),
                    EndAcrossNormalized = Mathf.Clamp(
                        currentProjection.AcrossMetres /
                        currentSurfaceHalf,
                        -1f,
                        1f),
                    AcrossHalfWidth = Mathf.Max(
                        0.05f,
                        acrossHalfWidth),
                    AlongHalfLength = Mathf.Max(
                        0.05f,
                        alongHalfLength),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    StaticTargetHeightMetres = 0f,
                    StaticWakeAmplitude = 0f,
                    StaticResponseStiffness = 1f,
                    StaticWakeReachMultiplier = 1f,
                    StaticUnsteadiness = 1f,
                    StaticContour = Array.Empty<Vector2>(),
                    MovementSpeed =
                        riverSpaceTravel /
                        Mathf.Max(0.001f, sampleDeltaTime),
                    Phase = ResolveSourcePhase(sourceId),
                    IsStatic = false,
                    StationaryObstruction = stationaryObstruction,
                    LastSeen = Time.realtimeSinceStartupAsDouble
                };

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public bool ContainsContinuousSource(EntityId sourceId)
        {
            return continuousSources.ContainsKey(sourceId);
        }

        public void RemoveContinuousSource(EntityId sourceId)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source) &&
                source.IsStatic)
            {
                staticTargetDirty = true;
            }

            continuousSources.Remove(sourceId);
        }

        public void EmitDebugImpactAtCentre()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtLocalDistance(
                    river.Domain.LocalLength * 0.5f);

            EmitImpact(
                sample.SurfacePoint,
                0.55f,
                1f,
                1f,
                1f);
        }

        public static bool TryFindContainingRiver(
            Vector3 worldPosition,
            float maximumVerticalDistance,
            out StylizedRiverDisturbanceRuntime runtime,
            out StylizedRiverProjection projection)
        {
            runtime = null;
            projection = default;
            float bestVerticalDistance = float.PositiveInfinity;

            for (int index = ActiveRuntimes.Count - 1; index >= 0; index--)
            {
                StylizedRiverDisturbanceRuntime candidate =
                    ActiveRuntimes[index];

                if (candidate == null)
                {
                    ActiveRuntimes.RemoveAt(index);
                    continue;
                }

                StylizedRiver candidateRiver = candidate.river;
                if (candidateRiver == null ||
                    !candidateRiver.RuntimeDisturbancesEnabled ||
                    !candidateRiver.TryProjectWorldPoint(
                        worldPosition,
                        out StylizedRiverProjection candidateProjection) ||
                    !candidateProjection.IsInside)
                {
                    continue;
                }

                float verticalDistance = Mathf.Abs(
                    worldPosition.y -
                    candidateProjection.SurfacePoint.y);

                if (verticalDistance > maximumVerticalDistance ||
                    verticalDistance >= bestVerticalDistance)
                {
                    continue;
                }

                runtime = candidate;
                projection = candidateProjection;
                bestVerticalDistance = verticalDistance;
            }

            return runtime != null;
        }

        private void HandleDomainChanged(RiverDomainSnapshot snapshot)
        {
            resourcesDirty = true;
            staticTargetDirty = true;
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceAdded(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceRemoved(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void HandleGeneratedGeometrySourceChanged(
            IGeneratedGeometrySource source)
        {
            generatedGeometryRegistryDirty = true;
        }

        private void RefreshGeneratedGeometrySources()
        {
            if (river == null ||
                !river.RuntimeDisturbancesEnabled ||
                !river.Domain.IsValid ||
                !river.TryGetSurfaceBounds(out Bounds currentRiverBounds))
            {
                generatedGeometryRefreshInProgress = false;
                generatedGeometryRefreshIndex = 0;
                return;
            }

            if (!generatedGeometryRefreshInProgress)
            {
                refreshedAutomaticGeneratedSourceIds.Clear();
                GeneratedGeometryRegistry.CopySourcesTo(
                    generatedGeometryScratch);

                currentRiverBounds.Expand(
                    new Vector3(
                        AutomaticBoundsHorizontalPadding * 2f,
                        AutomaticBoundsVerticalPadding * 2f,
                        AutomaticBoundsHorizontalPadding * 2f));

                generatedGeometryRefreshBounds = currentRiverBounds;
                generatedGeometryRefreshIndex = 0;
                generatedGeometryRefreshInProgress = true;

                // New registry events may set this back to true while the
                // current refresh is in flight. In that case another refresh
                // begins after this budgeted pass completes.
                generatedGeometryRegistryDirty = false;
            }

            int processedThisFrame = 0;
            while (generatedGeometryRefreshIndex <
                       generatedGeometryScratch.Count &&
                   processedThisFrame < GeneratedSourcesPerFrame)
            {
                IGeneratedGeometrySource source =
                    generatedGeometryScratch[generatedGeometryRefreshIndex++];
                ProcessGeneratedGeometrySource(source);
                processedThisFrame++;
            }

            if (generatedGeometryRefreshIndex <
                generatedGeometryScratch.Count)
            {
                return;
            }

            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                if (!refreshedAutomaticGeneratedSourceIds.Contains(sourceId))
                {
                    continuousSources.Remove(sourceId);
                    RemoveGeneratedDiagnostic(sourceId);
                }
            }

            automaticGeneratedSourceIds.Clear();
            automaticGeneratedSourceIds.UnionWith(
                refreshedAutomaticGeneratedSourceIds);
            generatedGeometryRefreshInProgress = false;
            generatedGeometryRefreshIndex = 0;
            staticTargetDirty = true;
            lastActivityTime = Time.realtimeSinceStartupAsDouble;
        }

        private void ProcessGeneratedGeometrySource(
            IGeneratedGeometrySource source)
        {
            if (source == null ||
                (source is UnityEngine.Object unityObject &&
                 unityObject == null) ||
                !source.IsSolidGeometry ||
                !source.IsStaticGeometry)
            {
                return;
            }

            GeneratedRiverInteractionSettings authoredSettings =
                source is IGeneratedRiverInteractionSource interactionSource
                    ? interactionSource.RiverInteractionSettings
                    : null;

            if (authoredSettings != null &&
                authoredSettings.Participation ==
                GeneratedRiverInteractionParticipation.Disabled)
            {
                return;
            }

            ResolvedGeneratedRiverInteraction interaction =
                authoredSettings != null
                    ? authoredSettings.Resolve()
                    : new ResolvedGeneratedRiverInteraction(
                        0.50f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        1f,
                        0.12f);

            MeshFilter meshFilter = source.GeometryMeshFilter;
            if (meshFilter == null ||
                meshFilter.sharedMesh == null ||
                !meshFilter.gameObject.activeInHierarchy ||
                !RiverDisturbanceFootprintResolver.TryGetWorldBounds(
                    meshFilter,
                    out Bounds sourceBounds) ||
                !generatedGeometryRefreshBounds.Intersects(sourceBounds) ||
                !river.TryProjectWorldPoint(
                    sourceBounds.center,
                    out StylizedRiverProjection boundsProjection))
            {
                return;
            }

            StylizedRiverSplineSample boundsSample =
                river.SampleAtLocalDistance(
                    boundsProjection.LocalDistance);
            float preliminaryRiverWidth = Mathf.Max(
                0.10f,
                boundsSample.LeftSurfaceHalfWidth +
                boundsSample.RightSurfaceHalfWidth);
            float effectivePadding =
                ResolveAutomaticFootprintPadding(
                    preliminaryRiverWidth,
                    interaction.FootprintPadding);

            if (!RiverDisturbanceFootprintResolver.TryResolve(
                    river,
                    meshFilter,
                    effectivePadding,
                    out RiverDisturbanceFootprint footprint,
                    out string footprintStatus) ||
                !river.TryProjectWorldPoint(
                    footprint.WorldPosition,
                    out StylizedRiverProjection footprintProjection) ||
                !footprintProjection.IsInside)
            {
                return;
            }

            RiverDisturbanceFootprint pressureFootprint = footprint;
            if (RiverDisturbanceFootprintResolver.TryResolve(
                    river,
                    meshFilter,
                    0f,
                    out RiverDisturbanceFootprint rawFootprint,
                    out _))
            {
                pressureFootprint = rawFootprint;
            }

            StylizedRiverSplineSample sample =
                river.SampleAtLocalDistance(
                    footprintProjection.LocalDistance);
            float localRiverWidth = Mathf.Max(
                0.10f,
                sample.LeftSurfaceHalfWidth +
                sample.RightSurfaceHalfWidth);
            float unpaddedAcrossHalfWidth = Mathf.Max(
                0.05f,
                footprint.AcrossHalfWidth - effectivePadding);
            float blockageRatio = Mathf.Clamp01(
                unpaddedAcrossHalfWidth * 2f / localRiverWidth);
            float blockageInfluence = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.04f,
                    0.55f,
                    blockageRatio));
            GeneratedRiverResponseProfile resolvedProfile =
                authoredSettings != null
                    ? authoredSettings.ResponseProfile
                    : GeneratedRiverResponseProfile.InheritRiver;

            float waveAllowance = Mathf.Clamp(
                river.MotionWaveHeight * 1.15f + 0.04f,
                0.04f,
                0.45f);
            float supportInspectionHeight =
                MaximumStaticPressureHeightMetres +
                waveAllowance + 0.10f;

            if (!RiverDisturbanceFootprintResolver.TryResolvePressureSupport(
                    river,
                    meshFilter,
                    pressureFootprint,
                    supportInspectionHeight,
                    out RiverDisturbancePressureSupportProfile pressureSupport,
                    out string pressureSupportStatus))
            {
                return;
            }

            float supportBudget = Mathf.Max(
                0f,
                pressureSupport.RepresentativeHeight - waveAllowance);
            float normalizedFlow = Mathf.Clamp01(
                Mathf.InverseLerp(
                    0.10f,
                    2.25f,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)));
            float flowResponse = normalizedFlow * normalizedFlow;
            float drive = flowResponse * Mathf.Lerp(
                0.35f,
                1f,
                blockageInfluence);
            float riverPressureStyle = Mathf.Lerp(
                0.85f,
                1.15f,
                Mathf.Clamp01(river.DisturbanceStrength * 0.5f));
            float unboundedPressureMaximum =
                supportBudget *
                Mathf.Lerp(0.04f, 0.92f, Mathf.Sqrt(drive)) *
                riverPressureStyle;
            float maximumAllowedPressureHeight = Mathf.Min(
                supportBudget / MaximumStaticPressureModulation,
                Mathf.Min(
                    unboundedPressureMaximum,
                    MaximumStaticPressureHeightMetres /
                    MaximumStaticPressureModulation));
            float minimumAllowedPressureHeight = Mathf.Min(
                maximumAllowedPressureHeight,
                supportBudget * Mathf.Lerp(
                    0.01f,
                    0.12f,
                    drive));
            float targetPressureHeight = Mathf.Lerp(
                minimumAllowedPressureHeight,
                maximumAllowedPressureHeight,
                interaction.PressureStrength);

            if (!RiverDisturbanceFootprintResolver.TryBuildPressureBakeProfile(
                    pressureSupport,
                    targetPressureHeight,
                    MaximumStaticPressureModulation,
                    out RiverDisturbancePressureBakeProfile pressureProfile))
            {
                return;
            }

            float wakeFlowFactor = Mathf.Lerp(
                0.15f,
                1.15f,
                Mathf.InverseLerp(
                    0.05f,
                    2.5f,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)));
            float wakeAmplitude = Mathf.Max(
                0f,
                (0.35f + blockageInfluence * 0.85f) *
                wakeFlowFactor *
                interaction.StrengthMultiplier *
                interaction.SurfaceDetail);

            EntityId sourceId = meshFilter.GetEntityId();
            if (!RegisterStaticSource(
                    sourceId,
                    footprint.WorldPosition,
                    footprint.AcrossHalfWidth,
                    footprint.AlongHalfLength,
                    1f,
                    1f,
                    1f,
                    -1f,
                    wakeAmplitude,
                    interaction.ResponseStiffness,
                    interaction.WakeLength,
                    interaction.Unsteadiness,
                    footprint.Contour,
                    targetPressureHeight,
                    pressureFootprint.AcrossHalfWidth,
                    pressureFootprint.AlongHalfLength,
                    pressureFootprint.Contour,
                    pressureProfile,
                    true))
            {
                return;
            }

            refreshedAutomaticGeneratedSourceIds.Add(sourceId);
            GeneratedSourceDiagnostics[sourceId] =
                new GeneratedRiverDisturbanceDiagnostics(
                    river,
                    true,
                    footprint.AcrossHalfWidth * 2f,
                    footprint.AlongHalfLength * 2f,
                    localRiverWidth,
                    blockageRatio,
                    effectivePadding,
                    targetPressureHeight,
                    wakeAmplitude,
                    maximumAllowedPressureHeight,
                    unboundedPressureMaximum >
                    supportBudget /
                    MaximumStaticPressureModulation + 0.0001f ||
                    unboundedPressureMaximum >
                    MaximumStaticPressureHeightMetres /
                    MaximumStaticPressureModulation + 0.0001f,
                    pressureSupport.RepresentativeHeight,
                    minimumAllowedPressureHeight,
                    maximumAllowedPressureHeight,
                    interaction.PressureStrength,
                    waveAllowance,
                    footprintStatus +
                    $" {pressureSupportStatus} " +
                    $"Contour {footprint.Contour.Length} points; " +
                    $"blockage {blockageRatio:P0}; " +
                    $"pressure strength {interaction.PressureStrength:P0}; " +
                    $"response profile {resolvedProfile}.");
        }

        private float ResolveAutomaticFootprintPadding(
            float localRiverWidth,
            float authoredPadding)
        {
            int localResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            int localFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            float longitudinalFieldCell =
                ChunkLengthMetres / localResolutionPerChunk;
            float lateralFieldCell =
                localRiverWidth / Mathf.Max(1, localFieldHeight);
            float surfaceSpacing = Mathf.Max(
                0.05f,
                river.ResolvedSurfaceLongitudinalSpacing);
            float resolutionMinimum = Mathf.Max(
                0.12f,
                longitudinalFieldCell * 0.70f,
                lateralFieldCell * 0.65f,
                surfaceSpacing * 0.55f);
            return Mathf.Max(
                Mathf.Max(0f, authoredPadding),
                resolutionMinimum);
        }

        private void RemoveGeneratedDiagnostic(EntityId sourceId)
        {
            if (GeneratedSourceDiagnostics.TryGetValue(
                    sourceId,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics) &&
                diagnostics.River == river)
            {
                GeneratedSourceDiagnostics.Remove(sourceId);
            }
        }

        private void RemoveOwnedGeneratedDiagnostics()
        {
            foreach (EntityId sourceId in automaticGeneratedSourceIds)
            {
                RemoveGeneratedDiagnostic(sourceId);
            }
        }

        private bool EnsureResources()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            if (!resourcesDirty &&
                currentState != null &&
                currentWake != null &&
                domainVersion == river.Domain.Version)
            {
                return true;
            }

            ReleaseResources();

            computeShader = Resources.Load<ComputeShader>(
                ComputeResourcePath);

            if (computeShader == null)
            {
                Debug.LogError(
                    $"StylizedRiver on '{name}' could not load compute shader Resources/{ComputeResourcePath}.",
                    this);
                return false;
            }

            clearKernel = computeShader.FindKernel("ClearRange");
            injectRippleKernel = computeShader.FindKernel("InjectRipple");
            injectWakeKernel = computeShader.FindKernel("InjectWake");
            bakeStaticPressureKernel = computeShader.FindKernel("BakeStaticPressure");
            finalizeStaticPressureKernel = computeShader.FindKernel("FinalizeStaticPressure");
            bakeStaticWakeSourceKernel = computeShader.FindKernel("BakeStaticWakeSource");
            simulateRippleKernel = computeShader.FindKernel("SimulateRipple");
            simulateWakeKernel = computeShader.FindKernel("SimulateWake");

            chunkCount = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    river.Domain.LocalLength /
                    ChunkLengthMetres));

            resolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 64,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };
            wakeResolutionPerChunk = river.Quality switch
            {
                StylizedRiverQuality.Low => 48,
                StylizedRiverQuality.Medium => 96,
                StylizedRiverQuality.High => 128,
                _ => 96
            };

            int maximumWidth = Mathf.Max(256, SystemInfo.maxTextureSize);
            if (resolutionPerChunk * chunkCount > maximumWidth)
            {
                resolutionPerChunk = Mathf.Max(
                    16,
                    maximumWidth / chunkCount);
            }
            if (wakeResolutionPerChunk * chunkCount > maximumWidth)
            {
                wakeResolutionPerChunk = Mathf.Max(
                    16,
                    maximumWidth / chunkCount);
            }

            fieldWidth = Mathf.Max(16, resolutionPerChunk * chunkCount);
            fieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 32,
                StylizedRiverQuality.Medium => 48,
                StylizedRiverQuality.High => 64,
                _ => 48
            };
            wakeFieldWidth = Mathf.Max(
                16,
                wakeResolutionPerChunk * chunkCount);
            wakeFieldHeight = river.Quality switch
            {
                StylizedRiverQuality.Low => 20,
                StylizedRiverQuality.Medium => 32,
                StylizedRiverQuality.High => 48,
                _ => 32
            };

            fieldLength = chunkCount * ChunkLengthMetres;
            averageSurfaceHalfWidth = ResolveAverageSurfaceHalfWidth();
            domainVersion = river.Domain.Version;

            stateA = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleA",
                fieldWidth,
                fieldHeight);
            stateB = CreateFieldTexture(
                "PS3D_RiverDisturbance_RippleB",
                fieldWidth,
                fieldHeight);
            staticTarget = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticPressure",
                fieldWidth,
                fieldHeight);
            wakeA = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeA",
                wakeFieldWidth,
                wakeFieldHeight);
            wakeB = CreateFieldTexture(
                "PS3D_RiverDisturbance_WakeB",
                wakeFieldWidth,
                wakeFieldHeight);
            staticWakeSource = CreateFieldTexture(
                "PS3D_RiverDisturbance_StaticWakeSource",
                wakeFieldWidth,
                wakeFieldHeight);

            currentState = stateA;
            previousState = stateA;
            writeState = stateB;
            currentWake = wakeA;
            previousWake = wakeA;
            writeWake = wakeB;

            chunkActiveUntil = new double[chunkCount];
            chunkActive = new bool[chunkCount];
            chunkHasStaticSource = new bool[chunkCount];
            wakeChunkActiveUntil = new double[chunkCount];
            wakeChunkActive = new bool[chunkCount];

            DispatchClear(stateA, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(stateB, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(staticTarget, fieldWidth, fieldHeight, 0, fieldWidth);
            DispatchClear(wakeA, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(wakeB, wakeFieldWidth, wakeFieldHeight, 0, wakeFieldWidth);
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);

            simulationAccumulator = 0f;
            simulationInterpolation = 1f;
            wakeInterpolation = 1f;
            validStaticSourceCount = 0;
            staticTargetDirty = true;
            resourcesDirty = false;
            return true;
        }

        private RenderTexture CreateFieldTexture(
            string textureName,
            int width,
            int height)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            texture.Create();
            return texture;
        }

        private void ReleaseResources()
        {
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref staticTarget);
            ReleaseTexture(ref staticWakeSource);
            ReleaseTexture(ref wakeA);
            ReleaseTexture(ref wakeB);
            currentState = null;
            previousState = null;
            writeState = null;
            currentWake = null;
            previousWake = null;
            writeWake = null;
            computeShader = null;
            clearKernel = -1;
            injectRippleKernel = -1;
            injectWakeKernel = -1;
            bakeStaticPressureKernel = -1;
            finalizeStaticPressureKernel = -1;
            bakeStaticWakeSourceKernel = -1;
            simulateRippleKernel = -1;
            simulateWakeKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            wakeFieldWidth = 0;
            wakeFieldHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            wakeResolutionPerChunk = 0;
            fieldLength = 0f;
            domainVersion = -1;
            chunkActiveUntil = Array.Empty<double>();
            chunkActive = Array.Empty<bool>();
            chunkHasStaticSource = Array.Empty<bool>();
            wakeChunkActiveUntil = Array.Empty<double>();
            wakeChunkActive = Array.Empty<bool>();
            validStaticSourceCount = 0;
            staticTargetDirty = true;
            resourcesDirty = true;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            if (texture.IsCreated())
            {
                texture.Release();
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }

            texture = null;
        }

        private void SimulateStep(float deltaTime, double now)
        {
            if (staticTargetDirty)
            {
                RebuildStaticTarget(now);
            }

            ExpireChunks(now);
            ExpireWakeChunks(now);

            for (int index = 0; index < pendingImpacts.Count; index++)
            {
                ImpactCommand impact = pendingImpacts[index];
                MarkActive(
                    impact.Distance,
                    impact.Radius,
                    now);
                DispatchRippleInjection(impact);
            }

            pendingImpacts.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (source.IsStatic)
                {
                    continue;
                }

                float absoluteFlowSpeed =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond);
                float movementBlend = source.StationaryObstruction
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(
                            StationarySpeedStart,
                            MovingSpeedFull,
                            source.MovementSpeed))
                    : 1f;
                float flowInfluence = Mathf.Lerp(
                    0.35f,
                    1.25f,
                    Mathf.InverseLerp(0f, 2.5f, absoluteFlowSpeed));
                float movementInfluence = Mathf.Lerp(
                    0.45f,
                    1.55f,
                    Mathf.InverseLerp(0f, 3f, source.MovementSpeed));
                float wakeStrength =
                    source.Strength *
                    river.DisturbanceStrength *
                    Mathf.Clamp01(source.NormalContribution) *
                    flowInfluence *
                    Mathf.Lerp(0.65f, movementInfluence, movementBlend);

                float segmentCentre =
                    (source.StartDistance + source.EndDistance) * 0.5f;
                float segmentHalfLength = Mathf.Abs(
                    source.EndDistance - source.StartDistance) * 0.5f;
                float wakeReach = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    absoluteFlowSpeed);
                MarkWakeActive(
                    segmentCentre + wakeReach * 0.5f,
                    segmentHalfLength + wakeReach * 0.5f +
                    Mathf.Max(source.AcrossHalfWidth, source.AlongHalfLength),
                    now);

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(source.EndDistance);
                float surfaceHalf = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(source.EndAcrossNormalized));

                DispatchWakeInjection(
                    source,
                    surfaceHalf,
                    wakeStrength,
                    movementBlend,
                    deltaTime);
            }

            SimulateRippleField(deltaTime);
            SimulateWakeField(deltaTime, now);
            simulationInterpolation = 0f;
            wakeInterpolation = 0f;
        }

        private void SimulateRippleField(float deltaTime)
        {
            if (!HasRippleActiveChunks())
            {
                return;
            }

            float cellSizeX = fieldLength / Mathf.Max(1, fieldWidth);
            float cellSizeY =
                averageSurfaceHalfWidth * 2f /
                Mathf.Max(1, fieldHeight - 1);
            float propagationSpeed = Mathf.Max(
                0.01f,
                river.DisturbancePropagationSpeed);
            float inverseLength = Mathf.Sqrt(
                1f / Mathf.Max(0.0001f, cellSizeX * cellSizeX) +
                1f / Mathf.Max(0.0001f, cellSizeY * cellSizeY));
            float maximumStableStep =
                RippleStabilitySafety /
                Mathf.Max(0.0001f, propagationSpeed * inverseLength);
            int substepCount = Mathf.Clamp(
                Mathf.CeilToInt(deltaTime / maximumStableStep),
                1,
                MaximumRippleSubsteps);
            float substepDelta = deltaTime / substepCount;
            float dampingPerSecond = Mathf.Lerp(
                2.8f,
                0.28f,
                river.DisturbancePersistence);

            for (int substep = 0; substep < substepCount; substep++)
            {
                float advectionPixels =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                    river.DisturbanceAdvection *
                    substepDelta /
                    Mathf.Max(0.001f, cellSizeX);

                computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
                computeShader.SetFloat("_DeltaTime", substepDelta);
                computeShader.SetFloat("_PropagationSpeed", propagationSpeed);
                computeShader.SetFloat("_DampingPerSecond", dampingPerSecond);
                computeShader.SetFloat("_AdvectionPixels", advectionPixels);
                computeShader.SetFloat("_CellSizeX", cellSizeX);
                computeShader.SetFloat("_CellSizeY", cellSizeY);
                computeShader.SetFloat(
                    "_MaximumHeight",
                    river.DisturbanceMaximumHeight);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateRead",
                    currentState);
                computeShader.SetTexture(
                    simulateRippleKernel,
                    "_StateWrite",
                    writeState);

                DispatchRippleActiveRanges();

                RenderTexture oldCurrent = currentState;
                currentState = writeState;
                previousState = oldCurrent;
                writeState = oldCurrent;
            }
        }

        private void DispatchRippleActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && chunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * resolutionPerChunk;
                int width = groupCount * resolutionPerChunk;
                computeShader.SetInt("_DispatchXOffset", xOffset);
                computeShader.SetInt("_DispatchWidth", width);
                computeShader.Dispatch(
                    simulateRippleKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1);
                groupStart = -1;
            }
        }

        private void SimulateWakeField(float deltaTime, double now)
        {
            if (!HasWakeActiveChunks())
            {
                return;
            }

            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                averageSurfaceHalfWidth * 2f /
                Mathf.Max(1, wakeFieldHeight - 1);
            float advectionPixels =
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                Mathf.Max(0.15f, river.DisturbanceAdvection) *
                deltaTime /
                Mathf.Max(0.001f, cellSizeX);
            float persistence = river.DisturbancePersistence;
            float decayPerSecond = Mathf.Lerp(2.4f, 0.42f, persistence);
            float lateralSpread = Mathf.Lerp(0.35f, 0.95f, persistence);
            float flowFactor = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0.05f,
                    1.25f,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)));

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetFloat("_WakeDeltaTime", deltaTime);
            computeShader.SetFloat("_WakeAdvectionPixels", advectionPixels);
            computeShader.SetFloat("_WakeCellSizeX", cellSizeX);
            computeShader.SetFloat("_WakeCellSizeY", cellSizeY);
            computeShader.SetFloat("_WakeLateralSpread", lateralSpread);
            computeShader.SetFloat("_WakeDecayPerSecond", decayPerSecond);
            computeShader.SetFloat("_WakeSourceRate", 1.45f);
            computeShader.SetFloat("_WakeFlowFactor", flowFactor);
            computeShader.SetFloat("_WakeTime", river.MotionTime);
            computeShader.SetFloat("_WakeGradientStrength", 0.32f);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeRead",
                currentWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_WakeWrite",
                writeWake);
            computeShader.SetTexture(
                simulateWakeKernel,
                "_StaticWakeSourceRead",
                staticWakeSource);

            DispatchWakeActiveRanges();

            RenderTexture oldCurrent = currentWake;
            currentWake = writeWake;
            previousWake = oldCurrent;
            writeWake = oldCurrent;
        }

        private void DispatchWakeActiveRanges()
        {
            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active = chunk < chunkCount && wakeChunkActive[chunk];
                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                int xOffset = groupStart * wakeResolutionPerChunk;
                int width = groupCount * wakeResolutionPerChunk;
                computeShader.SetInt("_WakeDispatchXOffset", xOffset);
                computeShader.SetInt("_WakeDispatchWidth", width);
                computeShader.Dispatch(
                    simulateWakeKernel,
                    Mathf.CeilToInt(width / (float)ThreadGroupSize),
                    Mathf.CeilToInt(wakeFieldHeight / (float)ThreadGroupSize),
                    1);
                groupStart = -1;
            }
        }

        private void DispatchRippleInjection(ImpactCommand impact)
        {
            float centreX = GlobalDistanceToPixel(impact.Distance);
            float centreY = AcrossToPixel(impact.AcrossNormalized);
            float radiusX =
                impact.Radius /
                Mathf.Max(0.001f, fieldLength / fieldWidth);
            float radiusY =
                impact.Radius /
                Mathf.Max(
                    0.001f,
                    impact.SurfaceHalfWidth * 2f / fieldHeight);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(centreX - radiusX - 2f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(centreX + radiusX + 2f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(centreY - radiusY - 2f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(centreY + radiusY + 2f),
                0,
                fieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            float strength = impact.Strength * river.DisturbanceStrength;

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_RippleInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_RippleInjectCentre",
                new Vector4(centreX, centreY, 0f, 0f));
            computeShader.SetVector(
                "_RippleInjectRadiusPixels",
                new Vector4(
                    Mathf.Max(1f, radiusX),
                    Mathf.Max(1f, radiusY),
                    0f,
                    0f));
            computeShader.SetFloat(
                "_RippleInjectHeight",
                strength * Mathf.Clamp01(impact.GeometryContribution) * 0.028f);
            computeShader.SetFloat(
                "_RippleInjectVelocity",
                strength * Mathf.Clamp01(impact.GeometryContribution) * 0.68f);
            computeShader.SetFloat(
                "_RippleInjectNormalDetail",
                strength * Mathf.Clamp01(impact.NormalContribution) * 0.12f);
            computeShader.SetFloat(
                "_MaximumHeight",
                river.DisturbanceMaximumHeight);
            computeShader.SetTexture(
                injectRippleKernel,
                "_StateWrite",
                currentState);
            computeShader.Dispatch(
                injectRippleKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1);
        }

        private void DispatchWakeInjection(
            ContinuousSource source,
            float surfaceHalfWidth,
            float wakeStrength,
            float movementBlend,
            float simulationDeltaTime)
        {
            float startX = WakeGlobalDistanceToPixel(source.StartDistance);
            float endX = WakeGlobalDistanceToPixel(source.EndDistance);
            float startY = WakeAcrossToPixel(source.StartAcrossNormalized);
            float endY = WakeAcrossToPixel(source.EndAcrossNormalized);
            float cellSizeX = fieldLength / Mathf.Max(1, wakeFieldWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, wakeFieldHeight);
            float alongPixels =
                source.AlongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                source.AcrossHalfWidth / Mathf.Max(0.001f, cellSizeY);
            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startX, endX) - alongPixels * 1.25f - 2f),
                0,
                wakeFieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startX, endX) + alongPixels * 2.0f + 3f),
                0,
                wakeFieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startY, endY) - acrossPixels * 1.40f - 2f),
                0,
                wakeFieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startY, endY) + acrossPixels * 1.40f + 2f),
                0,
                wakeFieldHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);

            computeShader.SetInts(
                "_WakeFieldSize",
                wakeFieldWidth,
                wakeFieldHeight);
            computeShader.SetInts(
                "_WakeInjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_WakeInjectStart",
                new Vector4(startX, startY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectEnd",
                new Vector4(endX, endY, 0f, 0f));
            computeShader.SetVector(
                "_WakeInjectFootprintPixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetFloat(
                "_WakeInjectStrength",
                Mathf.Max(0f, wakeStrength));
            computeShader.SetFloat(
                "_WakeInjectMovementBlend",
                Mathf.Clamp01(movementBlend));
            computeShader.SetFloat("_WakeInjectPersistence", 1f);
            computeShader.SetFloat(
                "_WakeInjectDeltaTime",
                Mathf.Max(0.0001f, simulationDeltaTime));
            computeShader.SetTexture(
                injectWakeKernel,
                "_WakeWrite",
                currentWake);
            computeShader.Dispatch(
                injectWakeKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1);
        }

        private void RebuildStaticTarget(double now)
        {
            if (staticTarget == null ||
                staticWakeSource == null ||
                computeShader == null)
            {
                return;
            }

            DispatchClear(
                staticTarget,
                fieldWidth,
                fieldHeight,
                0,
                fieldWidth);
            DispatchClear(
                staticWakeSource,
                wakeFieldWidth,
                wakeFieldHeight,
                0,
                wakeFieldWidth);

            double releasedWakeUntil =
                now + Mathf.Lerp(
                    1.5f,
                    8.0f,
                    river.DisturbancePersistence);
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (chunkHasStaticSource[chunk])
                {
                    chunkHasStaticSource[chunk] = false;
                    wakeChunkActive[chunk] = true;
                    wakeChunkActiveUntil[chunk] = releasedWakeUntil;
                }
            }

            validStaticSourceCount = 0;

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                ContinuousSource source = pair.Value;
                if (!source.IsStatic ||
                    !river.TryProjectWorldPoint(
                        source.WorldPosition,
                        out StylizedRiverProjection projection) ||
                    !projection.IsInside)
                {
                    continue;
                }

                StylizedRiverSplineSample sample =
                    river.SampleAtLocalDistance(projection.LocalDistance);
                float surfaceHalfWidth = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(projection.AcrossMetres));
                float acrossNormalized = Mathf.Clamp(
                    projection.AcrossMetres / surfaceHalfWidth,
                    -1f,
                    1f);
                float wakeLength = ResolveObstructionWakeLength(
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    Mathf.Abs(river.FlowSpeedMetresPerSecond)) *
                    source.StaticWakeReachMultiplier;

                DispatchStaticPressureBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source.StaticPressureAcrossHalfWidth,
                    source.StaticPressureAlongHalfLength,
                    source.StaticTargetHeightMetres,
                    source.StaticResponseStiffness,
                    source.StaticUnsteadiness,
                    source.Phase,
                    source.StaticPressureContour,
                    source.StaticPressureProfile);
                DispatchStaticWakeSourceBake(
                    projection.GlobalDistance,
                    acrossNormalized,
                    surfaceHalfWidth,
                    source.AcrossHalfWidth,
                    source.AlongHalfLength,
                    source.StaticWakeAmplitude,
                    source.StaticWakeReachMultiplier,
                    source.StaticUnsteadiness,
                    source.Phase,
                    source.StaticContour);

                MarkStaticRange(
                    projection.GlobalDistance,
                    source.AlongHalfLength,
                    wakeLength);
                validStaticSourceCount++;
            }

            if (validStaticSourceCount > 0)
            {
                computeShader.SetInts(
                    "_FieldSize",
                    fieldWidth,
                    fieldHeight);
                computeShader.SetVector(
                    "_StaticCellSize",
                    new Vector4(
                        fieldLength / Mathf.Max(1, fieldWidth),
                        averageSurfaceHalfWidth * 2f /
                        Mathf.Max(1, fieldHeight),
                        0f,
                        0f));
                computeShader.SetTexture(
                    finalizeStaticPressureKernel,
                    "_StaticPressureWrite",
                    staticTarget);
                computeShader.Dispatch(
                    finalizeStaticPressureKernel,
                    Mathf.CeilToInt(fieldWidth / (float)ThreadGroupSize),
                    Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                    1);
            }

            staticTargetDirty = false;
            lastActivityTime = now;
        }

        private void DispatchStaticPressureBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            float targetHeightMetres,
            float responseStiffness,
            float unsteadiness,
            float phase,
            Vector2[] contour,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                fieldWidth,
                fieldHeight,
                targetHeightMetres,
                0f,
                1f,
                responseStiffness,
                unsteadiness,
                phase,
                bakeStaticPressureKernel,
                staticTarget,
                true,
                pressureProfile);
        }

        private void DispatchStaticWakeSourceBake(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            float wakeAmplitude,
            float wakePersistence,
            float unsteadiness,
            float phase,
            Vector2[] contour)
        {
            DispatchStaticBakeCommon(
                globalDistance,
                acrossNormalized,
                surfaceHalfWidth,
                acrossHalfWidth,
                alongHalfLength,
                contour,
                wakeFieldWidth,
                wakeFieldHeight,
                0f,
                wakeAmplitude,
                wakePersistence,
                1f,
                unsteadiness,
                phase,
                bakeStaticWakeSourceKernel,
                staticWakeSource,
                false,
                default);
        }

        private void DispatchStaticBakeCommon(
            float globalDistance,
            float acrossNormalized,
            float surfaceHalfWidth,
            float acrossHalfWidth,
            float alongHalfLength,
            Vector2[] contour,
            int targetWidth,
            int targetHeight,
            float targetHeightMetres,
            float wakeAmplitude,
            float wakePersistence,
            float responseStiffness,
            float unsteadiness,
            float phase,
            int kernel,
            RenderTexture targetTexture,
            bool pressurePass,
            RiverDisturbancePressureBakeProfile pressureProfile)
        {
            float centreX = FieldGlobalDistanceToPixel(
                globalDistance,
                targetWidth);
            float centreY = FieldAcrossToPixel(
                acrossNormalized,
                targetHeight);
            float cellSizeX = fieldLength / Mathf.Max(1, targetWidth);
            float cellSizeY =
                surfaceHalfWidth * 2f / Mathf.Max(1, targetHeight);
            float alongPixels =
                alongHalfLength / Mathf.Max(0.001f, cellSizeX);
            float acrossPixels =
                acrossHalfWidth / Mathf.Max(0.001f, cellSizeY);
            float pressureDepthMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(
                        0.22f,
                        alongHalfLength * 2f * 0.08f,
                        cellSizeX * 1.15f,
                        river.ResolvedSurfaceLongitudinalSpacing * 1.50f),
                    0.22f,
                    0.48f)
                : 0f;
            float pressureInsideOverlapMetres = pressurePass
                ? Mathf.Clamp(
                    Mathf.Max(0.08f, cellSizeX * 0.35f),
                    0.08f,
                    0.16f)
                : 0f;
            float pressureDepthPixels = pressurePass
                ? pressureDepthMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float pressureInsideOverlapPixels = pressurePass
                ? pressureInsideOverlapMetres / Mathf.Max(0.001f, cellSizeX)
                : 0f;
            float longitudinalExpansion = pressurePass ? 1f : 1.75f;
            float lateralExpansion = pressurePass ? 1.20f : 1.55f;

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    pressurePass
                        ? centreX - alongPixels - pressureDepthPixels - 3f
                        : centreX - alongPixels * longitudinalExpansion - 4f),
                0,
                targetWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    pressurePass
                        ? centreX + alongPixels + 3f
                        : centreX + alongPixels * longitudinalExpansion + 5f),
                0,
                targetWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    centreY - acrossPixels * lateralExpansion - 4f),
                0,
                targetHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    centreY + acrossPixels * lateralExpansion + 4f),
                0,
                targetHeight - 1);
            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            int contourCount = Mathf.Min(
                contour != null ? contour.Length : 0,
                MaximumStaticContourPoints);
            for (int index = 0; index < MaximumStaticContourPoints; index++)
            {
                if (index < contourCount)
                {
                    Vector2 point = contour[index];
                    staticContourUpload[index] = new Vector4(
                        point.x / Mathf.Max(0.001f, cellSizeX),
                        point.y / Mathf.Max(0.001f, cellSizeY),
                        0f,
                        0f);
                }
                else
                {
                    staticContourUpload[index] = Vector4.zero;
                }
            }

            for (int index = 0;
                 index < staticPressureProfileUpload.Length;
                 index++)
            {
                if (pressurePass &&
                    pressureProfile.IsValid &&
                    index < pressureProfile.Samples.Length)
                {
                    Vector4 sample = pressureProfile.Samples[index];
                    staticPressureProfileUpload[index] = new Vector4(
                        sample.x / Mathf.Max(0.001f, cellSizeX),
                        sample.y / Mathf.Max(0.001f, cellSizeX),
                        sample.z,
                        sample.w);
                }
                else
                {
                    staticPressureProfileUpload[index] = Vector4.zero;
                }
            }

            computeShader.SetInts("_FieldSize", targetWidth, targetHeight);
            computeShader.SetInts(
                "_StaticRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_StaticCentre",
                new Vector4(centreX, centreY, 0f, 0f));
            computeShader.SetVector(
                "_StaticHalfSizePixels",
                new Vector4(
                    Mathf.Max(1f, alongPixels),
                    Mathf.Max(1f, acrossPixels),
                    0f,
                    0f));
            computeShader.SetVector(
                "_StaticCellSize",
                new Vector4(cellSizeX, cellSizeY, 0f, 0f));
            computeShader.SetInt("_StaticContourCount", contourCount);
            computeShader.SetVectorArray(
                "_StaticContour",
                staticContourUpload);
            computeShader.SetVectorArray(
                "_StaticPressureProfile",
                staticPressureProfileUpload);
            computeShader.SetFloat(
                "_StaticPressureProfileHalfWidthPixels",
                pressurePass && pressureProfile.IsValid
                    ? pressureProfile.AcrossHalfWidth /
                      Mathf.Max(0.001f, cellSizeY)
                    : acrossPixels);
            computeShader.SetInt(
                "_StaticPressureProfileValid",
                pressurePass && pressureProfile.IsValid ? 1 : 0);
            computeShader.SetFloat(
                "_StaticTargetHeight",
                Mathf.Clamp(
                    targetHeightMetres,
                    0f,
                    MaximumStaticPressureHeightMetres));
            computeShader.SetFloat(
                "_StaticPressureDepthPixels",
                pressureDepthPixels);
            computeShader.SetFloat(
                "_StaticPressureInsideOverlapPixels",
                pressureInsideOverlapPixels);
            computeShader.SetFloat(
                "_StaticMaximumHeight",
                MaximumStaticPressureHeightMetres);
            computeShader.SetFloat(
                "_StaticWakeSourceStrength",
                Mathf.Clamp(wakeAmplitude, 0f, 4f));
            computeShader.SetFloat(
                "_StaticWakePersistence",
                Mathf.Clamp(wakePersistence, 0.25f, 3f));
            computeShader.SetFloat(
                "_StaticPhase",
                Mathf.Repeat(phase, 1f));
            computeShader.SetFloat(
                "_StaticResponseStiffness",
                Mathf.Clamp(responseStiffness, 0f, 2f));
            computeShader.SetFloat(
                "_StaticUnsteadiness",
                Mathf.Clamp(unsteadiness, 0f, 2f));
            computeShader.SetFloat(
                "_MaximumHeight",
                river.DisturbanceMaximumHeight);
            computeShader.SetTexture(
                kernel,
                pressurePass
                    ? "_StaticPressureWrite"
                    : "_StaticWakeSourceWrite",
                targetTexture);
            computeShader.Dispatch(
                kernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1);
        }

        private void MarkStaticRange(
            float globalDistance,
            float alongHalfLength,
            float wakeLength)
        {
            float sourceLocal =
                globalDistance - river.Domain.GlobalDistanceMinimum;
            float upstreamReach = alongHalfLength * 0.80f;
            float downstreamReach = Mathf.Max(
                wakeLength,
                alongHalfLength * 1.20f);
            float minimumLocal = Mathf.Clamp(
                sourceLocal - upstreamReach,
                0f,
                fieldLength);
            float maximumLocal = Mathf.Clamp(
                sourceLocal + downstreamReach,
                0f,
                fieldLength);
            int minimumChunk = Mathf.Clamp(
                Mathf.FloorToInt(minimumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int maximumChunk = Mathf.Clamp(
                Mathf.FloorToInt(maximumLocal / ChunkLengthMetres),
                0,
                chunkCount - 1);

            for (int chunk = minimumChunk; chunk <= maximumChunk; chunk++)
            {
                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                chunkHasStaticSource[chunk] = true;
                wakeChunkActiveUntil[chunk] = double.PositiveInfinity;
            }
        }

        private void MarkWakeActive(
            float globalDistance,
            float radius,
            double now)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                fieldLength);
            int centreChunk = Mathf.Clamp(
                Mathf.FloorToInt(localDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int radiusChunks = Mathf.CeilToInt(
                radius / ChunkLengthMetres) + 1;
            double activeDuration = Mathf.Lerp(
                2.0f,
                10.0f,
                river.DisturbancePersistence);

            for (int chunk = centreChunk - radiusChunks;
                 chunk <= centreChunk + radiusChunks;
                 chunk++)
            {
                if (chunk < 0 || chunk >= chunkCount)
                {
                    continue;
                }

                if (!wakeChunkActive[chunk])
                {
                    int xOffset = chunk * wakeResolutionPerChunk;
                    DispatchClear(
                        wakeA,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    DispatchClear(
                        wakeB,
                        wakeFieldWidth,
                        wakeFieldHeight,
                        xOffset,
                        wakeResolutionPerChunk);
                    wakeChunkActive[chunk] = true;
                }

                wakeChunkActiveUntil[chunk] = Math.Max(
                    wakeChunkActiveUntil[chunk],
                    now + activeDuration);
            }

            lastActivityTime = now;
        }

        private static float ResolveObstructionWakeLength(
            float acrossHalfWidth,
            float alongHalfLength,
            float absoluteFlowSpeed)
        {
            float footprintScale = Mathf.Max(
                acrossHalfWidth * 1.20f,
                alongHalfLength * 1.40f);
            return footprintScale *
                   (1f + Mathf.Min(3f, absoluteFlowSpeed) * 0.12f);
        }

        private static Vector2[] CopyStaticContour(
            IReadOnlyList<Vector2> contour)
        {
            if (contour == null || contour.Count < 3)
            {
                return Array.Empty<Vector2>();
            }

            int count = Mathf.Min(
                contour.Count,
                MaximumStaticContourPoints);
            Vector2[] result = new Vector2[count];
            for (int index = 0; index < count; index++)
            {
                result[index] = contour[index];
            }

            return result;
        }

        private float ResolveSourcePhase(EntityId sourceId)
        {
            if (continuousSources.TryGetValue(
                    sourceId,
                    out ContinuousSource source))
            {
                return source.Phase;
            }

            float phase = Mathf.Repeat(
                sourcePhaseSequence * GoldenPhaseStep,
                1f);
            sourcePhaseSequence++;
            return phase;
        }

        private void DispatchClear(
            RenderTexture texture,
            int textureWidth,
            int textureHeight,
            int xOffset,
            int width)
        {
            if (texture == null || computeShader == null || clearKernel < 0)
            {
                return;
            }

            int safeOffset = Mathf.Clamp(xOffset, 0, Mathf.Max(0, textureWidth - 1));
            int safeWidth = Mathf.Clamp(width, 0, textureWidth - safeOffset);
            if (safeWidth <= 0)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", textureWidth, textureHeight);
            computeShader.SetInt("_DispatchXOffset", safeOffset);
            computeShader.SetInt("_DispatchWidth", safeWidth);
            computeShader.SetTexture(clearKernel, "_StateWrite", texture);
            computeShader.Dispatch(
                clearKernel,
                Mathf.CeilToInt(safeWidth / (float)ThreadGroupSize),
                Mathf.CeilToInt(textureHeight / (float)ThreadGroupSize),
                1);
        }

        private void MarkActive(
            float globalDistance,
            float radius,
            double now)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                fieldLength);
            int centreChunk = Mathf.Clamp(
                Mathf.FloorToInt(localDistance / ChunkLengthMetres),
                0,
                chunkCount - 1);
            int radiusChunks = Mathf.CeilToInt(
                radius / ChunkLengthMetres) + 1;
            double activeDuration = Mathf.Lerp(
                1.5f,
                8.0f,
                river.DisturbancePersistence);

            for (int chunk = centreChunk - radiusChunks;
                 chunk <= centreChunk + radiusChunks;
                 chunk++)
            {
                if (chunk < 0 || chunk >= chunkCount)
                {
                    continue;
                }

                if (!chunkActive[chunk])
                {
                    int xOffset = chunk * resolutionPerChunk;
                    DispatchClear(
                        stateA,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    DispatchClear(
                        stateB,
                        fieldWidth,
                        fieldHeight,
                        xOffset,
                        resolutionPerChunk);
                    chunkActive[chunk] = true;
                }

                chunkActiveUntil[chunk] = Mathf.Max(
                    (float)chunkActiveUntil[chunk],
                    (float)(now + activeDuration));
            }

            lastActivityTime = now;
        }

        private void ExpireChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!chunkActive[chunk] ||
                    now < chunkActiveUntil[chunk])
                {
                    continue;
                }

                int xOffset = chunk * resolutionPerChunk;
                DispatchClear(
                    stateA,
                    fieldWidth,
                    fieldHeight,
                    xOffset,
                    resolutionPerChunk);
                DispatchClear(
                    stateB,
                    fieldWidth,
                    fieldHeight,
                    xOffset,
                    resolutionPerChunk);
                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
            }
        }

        private void ExpireWakeChunks(double now)
        {
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                if (!wakeChunkActive[chunk] ||
                    chunkHasStaticSource[chunk] ||
                    now < wakeChunkActiveUntil[chunk])
                {
                    continue;
                }

                int xOffset = chunk * wakeResolutionPerChunk;
                DispatchClear(
                    wakeA,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                DispatchClear(
                    wakeB,
                    wakeFieldWidth,
                    wakeFieldHeight,
                    xOffset,
                    wakeResolutionPerChunk);
                wakeChunkActive[chunk] = false;
                wakeChunkActiveUntil[chunk] = 0.0;
            }
        }

        private void CleanupStaleSources(double now)
        {
            staleSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                if (!pair.Value.IsStatic &&
                    now - pair.Value.LastSeen > SourceStaleSeconds)
                {
                    staleSourceIds.Add(pair.Key);
                }
            }

            for (int index = 0; index < staleSourceIds.Count; index++)
            {
                continuousSources.Remove(staleSourceIds[index]);
            }
        }

        private float ResolveSimulationRate()
        {
            float qualityRate = river != null
                ? river.Quality switch
                {
                    StylizedRiverQuality.Low => 12f,
                    StylizedRiverQuality.Medium => 20f,
                    StylizedRiverQuality.High => 30f,
                    _ => 20f
                }
                : 20f;

            return HasStaticSources() &&
                   !HasDynamicSources() &&
                   pendingImpacts.Count == 0
                ? Mathf.Min(qualityRate, StaticOnlySimulationRate)
                : qualityRate;
        }

        private bool HasStaticSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasDynamicSources()
        {
            foreach (ContinuousSource source in continuousSources.Values)
            {
                if (!source.IsStatic)
                {
                    return true;
                }
            }

            return false;
        }

        private float ResolveAverageSurfaceHalfWidth()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 1f;
            }

            double sum = 0.0;
            for (int index = 0; index < river.Domain.SampleCount; index++)
            {
                StylizedRiverSplineSample sample = river.Domain.Samples[index];
                sum +=
                    (sample.LeftSurfaceHalfWidth +
                     sample.RightSurfaceHalfWidth) * 0.5;
            }

            return Mathf.Max(
                0.25f,
                (float)(sum / river.Domain.SampleCount));
        }

        private float GlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, fieldWidth);
        }

        private float WakeGlobalDistanceToPixel(float globalDistance)
        {
            return FieldGlobalDistanceToPixel(globalDistance, wakeFieldWidth);
        }

        private float FieldGlobalDistanceToPixel(
            float globalDistance,
            int targetWidth)
        {
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                fieldLength);
            return localDistance / Mathf.Max(0.001f, fieldLength) *
                   Mathf.Max(0, targetWidth - 1);
        }

        private float AcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, fieldHeight);
        }

        private float WakeAcrossToPixel(float acrossNormalized)
        {
            return FieldAcrossToPixel(acrossNormalized, wakeFieldHeight);
        }

        private static float FieldAcrossToPixel(
            float acrossNormalized,
            int targetHeight)
        {
            return
                (Mathf.Clamp(acrossNormalized, -1f, 1f) * 0.5f + 0.5f) *
                Mathf.Max(0, targetHeight - 1);
        }

        private bool HasActiveChunks()
        {
            return HasRippleActiveChunks() || HasWakeActiveChunks();
        }

        private bool HasRippleActiveChunks()
        {
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasWakeActiveChunks()
        {
            for (int index = 0; index < wakeChunkActive.Length; index++)
            {
                if (wakeChunkActive[index])
                {
                    return true;
                }
            }

            return false;
        }

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkCount; index++)
            {
                bool rippleActive =
                    index < chunkActive.Length && chunkActive[index];
                bool wakeActive =
                    index < wakeChunkActive.Length && wakeChunkActive[index];
                if (rippleActive || wakeActive)
                {
                    count++;
                }
            }

            return count;
        }

        private void BindField()
        {
            if (surfaceRenderer == null ||
                currentState == null ||
                previousState == null ||
                currentWake == null ||
                previousWake == null ||
                staticTarget == null ||
                staticWakeSource == null)
            {
                BindDisabled();
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 1f);
            propertyBlock.SetTexture(
                DisturbancePreviousId,
                previousState);
            propertyBlock.SetTexture(
                DisturbanceCurrentId,
                currentState);
            propertyBlock.SetTexture(
                DisturbanceStaticTargetId,
                staticTarget);
            propertyBlock.SetTexture(
                DisturbanceStaticWakeSourceId,
                staticWakeSource);
            propertyBlock.SetTexture(
                DisturbanceWakePreviousId,
                previousWake);
            propertyBlock.SetTexture(
                DisturbanceWakeCurrentId,
                currentWake);
            propertyBlock.SetFloat(
                DisturbanceInterpolationId,
                simulationInterpolation);
            propertyBlock.SetFloat(
                DisturbanceWakeInterpolationId,
                wakeInterpolation);
            propertyBlock.SetFloat(
                DisturbanceGlobalStartId,
                river.Domain.GlobalDistanceMinimum);
            propertyBlock.SetFloat(
                DisturbanceFieldLengthId,
                Mathf.Max(0.001f, fieldLength));
            propertyBlock.SetFloat(
                DisturbanceGeometryStrengthId,
                river.DisturbanceGeometryStrength);
            propertyBlock.SetFloat(
                DisturbanceNormalStrengthId,
                river.DisturbanceNormalStrength);
            propertyBlock.SetFloat(
                DisturbanceShoreInteractionId,
                river.DisturbanceShoreInteraction);
            propertyBlock.SetFloat(
                DisturbanceMaximumHeightId,
                river.DisturbanceMaximumHeight);
            propertyBlock.SetFloat(
                DisturbanceStaticMaximumHeightId,
                MaximumStaticPressureHeightMetres);
            propertyBlock.SetFloat(
                DisturbanceDebugViewId,
                (float)river.DisturbanceDebugView);
            propertyBlock.SetFloat(
                DisturbanceFragmentDetailId,
                river.Quality == StylizedRiverQuality.Low ? 0f : 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }

        private void BindDisabled()
        {
            if (surfaceRenderer == null && river != null)
            {
                surfaceRenderer = river.SurfaceRenderer;
            }

            if (surfaceRenderer == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            surfaceRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(DisturbanceEnabledId, 0f);
            propertyBlock.SetFloat(DisturbanceFragmentDetailId, 0f);
            propertyBlock.SetFloat(DisturbanceWakeInterpolationId, 1f);
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
