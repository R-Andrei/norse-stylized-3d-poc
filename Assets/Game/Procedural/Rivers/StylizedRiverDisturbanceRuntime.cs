using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
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

        private static readonly List<StylizedRiverDisturbanceRuntime>
            ActiveRuntimes = new();

        private static readonly int DisturbanceEnabledId =
            Shader.PropertyToID("_DisturbanceEnabled");
        private static readonly int DisturbancePreviousId =
            Shader.PropertyToID("_DisturbanceFieldPrevious");
        private static readonly int DisturbanceCurrentId =
            Shader.PropertyToID("_DisturbanceFieldCurrent");
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
        private static readonly int DisturbanceDebugViewId =
            Shader.PropertyToID("_DisturbanceDebugView");

        private readonly Dictionary<EntityId, ContinuousSource> continuousSources =
            new();
        private readonly List<EntityId> staleSourceIds = new();
        private readonly List<ImpactCommand> pendingImpacts = new();

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture currentState;
        private RenderTexture previousState;
        private RenderTexture writeState;
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool[] chunkActive = Array.Empty<bool>();

        private int clearKernel = -1;
        private int injectKernel = -1;
        private int simulateKernel = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int domainVersion = -1;
        private float fieldLength;
        private float averageSurfaceHalfWidth = 1f;
        private float simulationAccumulator;
        private float simulationInterpolation = 1f;
        private double lastRuntimeTime;
        private double lastActivityTime;
        private bool supportWarningReported;
        private bool resourcesDirty = true;
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
            (long)fieldWidth * fieldHeight * 8L * 2L;

        private struct ContinuousSource
        {
            public float StartDistance;
            public float EndDistance;
            public float StartAcrossNormalized;
            public float EndAcrossNormalized;
            public float Radius;
            public float Strength;
            public float GeometryContribution;
            public float NormalContribution;
            public float MovementSpeed;
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

            lastRuntimeTime = Time.realtimeSinceStartupAsDouble;
            resourcesDirty = true;
            BindDisabled();
        }

        private void OnDisable()
        {
            ActiveRuntimes.Remove(this);

            if (river != null)
            {
                river.DomainChanged -= HandleDomainChanged;
            }

            BindDisabled();
            ReleaseResources();
            continuousSources.Clear();
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

            BindField();
        }

        public void NotifyRiverChanged()
        {
            resourcesDirty = true;
        }

        public void ClearField()
        {
            if (stateA != null && computeShader != null)
            {
                DispatchClear(stateA, 0, fieldWidth);
            }

            if (stateB != null && computeShader != null)
            {
                DispatchClear(stateB, 0, fieldWidth);
            }

            Array.Clear(chunkActive, 0, chunkActive.Length);
            Array.Clear(chunkActiveUntil, 0, chunkActiveUntil.Length);
            pendingImpacts.Clear();
            simulationAccumulator = 0f;
            simulationInterpolation = 1f;
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

        public bool UpdateContinuousSource(
            EntityId sourceId,
            Vector3 previousWorldPosition,
            Vector3 currentWorldPosition,
            float sampleDeltaTime,
            float radius,
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

            continuousSources[sourceId] =
                new ContinuousSource
                {
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
                    Radius = Mathf.Max(0.05f, radius),
                    Strength = Mathf.Max(0f, strength),
                    GeometryContribution = Mathf.Clamp01(
                        geometryContribution),
                    NormalContribution = Mathf.Clamp01(
                        normalContribution),
                    MovementSpeed =
                        riverSpaceTravel /
                        Mathf.Max(0.001f, sampleDeltaTime),
                    StationaryObstruction = stationaryObstruction,
                    LastSeen = Time.realtimeSinceStartupAsDouble
                };

            lastActivityTime = Time.realtimeSinceStartupAsDouble;
            return true;
        }

        public void RemoveContinuousSource(EntityId sourceId)
        {
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
        }

        private bool EnsureResources()
        {
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            if (!resourcesDirty &&
                currentState != null &&
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
            injectKernel = computeShader.FindKernel("Inject");
            simulateKernel = computeShader.FindKernel("Simulate");

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

            int maximumWidth = Mathf.Max(256, SystemInfo.maxTextureSize);
            if (resolutionPerChunk * chunkCount > maximumWidth)
            {
                resolutionPerChunk = Mathf.Max(
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

            fieldLength = chunkCount * ChunkLengthMetres;
            averageSurfaceHalfWidth = ResolveAverageSurfaceHalfWidth();
            domainVersion = river.Domain.Version;

            stateA = CreateFieldTexture("PS3D_RiverDisturbance_A");
            stateB = CreateFieldTexture("PS3D_RiverDisturbance_B");
            currentState = stateA;
            previousState = stateA;
            writeState = stateB;

            chunkActiveUntil = new double[chunkCount];
            chunkActive = new bool[chunkCount];

            DispatchClear(stateA, 0, fieldWidth);
            DispatchClear(stateB, 0, fieldWidth);

            simulationAccumulator = 0f;
            simulationInterpolation = 1f;
            resourcesDirty = false;
            return true;
        }

        private RenderTexture CreateFieldTexture(string textureName)
        {
            RenderTexture texture = new RenderTexture(
                fieldWidth,
                fieldHeight,
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
            currentState = null;
            previousState = null;
            writeState = null;
            computeShader = null;
            clearKernel = -1;
            injectKernel = -1;
            simulateKernel = -1;
            fieldWidth = 0;
            fieldHeight = 0;
            chunkCount = 0;
            resolutionPerChunk = 0;
            fieldLength = 0f;
            domainVersion = -1;
            chunkActiveUntil = Array.Empty<double>();
            chunkActive = Array.Empty<bool>();
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
            ExpireChunks(now);

            for (int index = 0; index < pendingImpacts.Count; index++)
            {
                ImpactCommand impact = pendingImpacts[index];
                MarkActive(
                    impact.Distance,
                    impact.Radius,
                    now);
                DispatchInjection(
                    impact.Distance,
                    impact.AcrossNormalized,
                    impact.Distance,
                    impact.AcrossNormalized,
                    impact.SurfaceHalfWidth,
                    impact.Radius,
                    impact.Strength * river.DisturbanceStrength,
                    impact.GeometryContribution,
                    impact.NormalContribution,
                    0,
                    0f,
                    0f);
            }

            pendingImpacts.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                ContinuousSource source = pair.Value;
                float absoluteFlowSpeed =
                    Mathf.Abs(river.FlowSpeedMetresPerSecond);
                float flowInfluence = absoluteFlowSpeed * 0.18f;
                float movementInfluence = source.MovementSpeed * 0.12f;
                float sourceStrength =
                    source.Strength *
                    river.DisturbanceStrength *
                    deltaTime *
                    (0.35f + flowInfluence + movementInfluence);

                float movementBlend = source.StationaryObstruction
                    ? Mathf.SmoothStep(
                        0f,
                        1f,
                        Mathf.InverseLerp(
                            StationarySpeedStart,
                            MovingSpeedFull,
                            source.MovementSpeed))
                    : 1f;

                float obstructionWakeLength =
                    source.Radius *
                    (1.8f + absoluteFlowSpeed * 0.55f);

                float segmentCentre =
                    (source.StartDistance + source.EndDistance) * 0.5f;
                float segmentHalfLength =
                    Mathf.Abs(
                        source.EndDistance -
                        source.StartDistance) * 0.5f;
                float downstreamReach =
                    obstructionWakeLength * (1f - movementBlend);

                MarkActive(
                    segmentCentre + downstreamReach * 0.5f,
                    segmentHalfLength +
                    downstreamReach * 0.5f +
                    source.Radius,
                    now);

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(source.EndDistance);
                float surfaceHalf = Mathf.Max(
                    0.05f,
                    sample.GetSurfaceHalfWidth(
                        source.EndAcrossNormalized));

                DispatchInjection(
                    source.StartDistance,
                    source.StartAcrossNormalized,
                    source.EndDistance,
                    source.EndAcrossNormalized,
                    surfaceHalf,
                    source.Radius,
                    sourceStrength,
                    source.GeometryContribution,
                    source.NormalContribution,
                    1,
                    movementBlend,
                    obstructionWakeLength);
            }

            if (!HasActiveChunks())
            {
                return;
            }

            float cellSizeX = fieldLength / Mathf.Max(1, fieldWidth);
            float cellSizeY =
                averageSurfaceHalfWidth * 2f /
                Mathf.Max(1, fieldHeight - 1);
            float dampingPerSecond = Mathf.Lerp(
                2.8f,
                0.28f,
                river.DisturbancePersistence);
            float advectionPixels =
                Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                river.DisturbanceAdvection *
                deltaTime /
                Mathf.Max(0.001f, cellSizeX);

            computeShader.SetInts(
                "_FieldSize",
                fieldWidth,
                fieldHeight);
            computeShader.SetFloat("_DeltaTime", deltaTime);
            computeShader.SetFloat(
                "_PropagationSpeed",
                river.DisturbancePropagationSpeed);
            computeShader.SetFloat("_DampingPerSecond", dampingPerSecond);
            computeShader.SetFloat("_AdvectionPixels", advectionPixels);
            computeShader.SetFloat("_CellSizeX", cellSizeX);
            computeShader.SetFloat("_CellSizeY", cellSizeY);
            computeShader.SetFloat(
                "_MaximumHeight",
                river.DisturbanceMaximumHeight);
            computeShader.SetTexture(
                simulateKernel,
                "_StateRead",
                currentState);
            computeShader.SetTexture(
                simulateKernel,
                "_StateWrite",
                writeState);

            int groupStart = -1;
            for (int chunk = 0; chunk <= chunkCount; chunk++)
            {
                bool active =
                    chunk < chunkCount &&
                    chunkActive[chunk];

                if (active && groupStart < 0)
                {
                    groupStart = chunk;
                }

                if (active || groupStart < 0)
                {
                    continue;
                }

                int groupCount = chunk - groupStart;
                DispatchSimulationRange(
                    groupStart * resolutionPerChunk,
                    groupCount * resolutionPerChunk);
                groupStart = -1;
            }

            RenderTexture oldCurrent = currentState;
            currentState = writeState;
            previousState = oldCurrent;
            writeState = oldCurrent;
            simulationInterpolation = 0f;
        }

        private void DispatchSimulationRange(int xOffset, int width)
        {
            computeShader.SetInt("_DispatchXOffset", xOffset);
            computeShader.SetInt("_DispatchWidth", width);
            computeShader.Dispatch(
                simulateKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
                1);
        }

        private void DispatchInjection(
            float startGlobalDistance,
            float startAcrossNormalized,
            float endGlobalDistance,
            float endAcrossNormalized,
            float surfaceHalfWidth,
            float radius,
            float strength,
            float geometryContribution,
            float normalContribution,
            int mode,
            float movementBlend,
            float obstructionWakeLength)
        {
            float startX = GlobalDistanceToPixel(startGlobalDistance);
            float endX = GlobalDistanceToPixel(endGlobalDistance);
            float startY = AcrossToPixel(startAcrossNormalized);
            float endY = AcrossToPixel(endAcrossNormalized);
            float cellSizeX = fieldLength / Mathf.Max(1, fieldWidth);
            float radiusX =
                radius /
                Mathf.Max(0.001f, cellSizeX);
            float radiusY =
                radius /
                Mathf.Max(
                    0.001f,
                    surfaceHalfWidth * 2f / fieldHeight);
            float wakeLengthPixels =
                mode == 1
                    ? obstructionWakeLength *
                      (1f - Mathf.Clamp01(movementBlend)) /
                      Mathf.Max(0.001f, cellSizeX)
                    : 0f;
            float wakeRectContribution = wakeLengthPixels;

            int minX = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startX, endX) - radiusX * 1.25f - 1f),
                0,
                fieldWidth - 1);
            int maxX = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startX, endX + wakeRectContribution) +
                    radiusX * 1.25f +
                    1f),
                0,
                fieldWidth - 1);
            int minY = Mathf.Clamp(
                Mathf.FloorToInt(
                    Mathf.Min(startY, endY) - radiusY * 1.25f - 1f),
                0,
                fieldHeight - 1);
            int maxY = Mathf.Clamp(
                Mathf.CeilToInt(
                    Mathf.Max(startY, endY) + radiusY * 1.25f + 1f),
                0,
                fieldHeight - 1);

            int width = Mathf.Max(1, maxX - minX + 1);
            int height = Mathf.Max(1, maxY - minY + 1);
            float geometryStrength =
                strength * Mathf.Clamp01(geometryContribution);
            float normalStrength =
                strength * Mathf.Clamp01(normalContribution);
            float injectedHeight = geometryStrength * 0.028f;

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInts(
                "_InjectRect",
                minX,
                minY,
                width,
                height);
            computeShader.SetVector(
                "_InjectStart",
                new Vector4(startX, startY, 0f, 0f));
            computeShader.SetVector(
                "_InjectEnd",
                new Vector4(endX, endY, 0f, 0f));
            computeShader.SetVector(
                "_InjectRadiusPixels",
                new Vector4(
                    Mathf.Max(1f, radiusX),
                    Mathf.Max(1f, radiusY),
                    0f,
                    0f));
            computeShader.SetFloat(
                "_InjectHeight",
                injectedHeight);
            computeShader.SetFloat(
                "_InjectVelocity",
                geometryStrength * 0.68f);
            computeShader.SetFloat(
                "_InjectGeometrySlope",
                injectedHeight / Mathf.Max(0.05f, radius));
            computeShader.SetFloat(
                "_InjectNormalDetail",
                normalStrength * 0.12f);
            computeShader.SetFloat(
                "_InjectMovementBlend",
                Mathf.Clamp01(movementBlend));
            computeShader.SetFloat(
                "_InjectWakeLengthPixels",
                Mathf.Max(0f, wakeLengthPixels));
            computeShader.SetFloat(
                "_MaximumHeight",
                river.DisturbanceMaximumHeight);
            computeShader.SetInt("_InjectMode", mode);
            computeShader.SetTexture(
                injectKernel,
                "_StateWrite",
                currentState);
            computeShader.Dispatch(
                injectKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(height / (float)ThreadGroupSize),
                1);
        }

        private void DispatchClear(
            RenderTexture texture,
            int xOffset,
            int width)
        {
            if (texture == null || computeShader == null || clearKernel < 0)
            {
                return;
            }

            computeShader.SetInts("_FieldSize", fieldWidth, fieldHeight);
            computeShader.SetInt("_DispatchXOffset", xOffset);
            computeShader.SetInt("_DispatchWidth", width);
            computeShader.SetTexture(clearKernel, "_StateWrite", texture);
            computeShader.Dispatch(
                clearKernel,
                Mathf.CeilToInt(width / (float)ThreadGroupSize),
                Mathf.CeilToInt(fieldHeight / (float)ThreadGroupSize),
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
                    DispatchClear(stateA, xOffset, resolutionPerChunk);
                    DispatchClear(stateB, xOffset, resolutionPerChunk);
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
                DispatchClear(stateA, xOffset, resolutionPerChunk);
                DispatchClear(stateB, xOffset, resolutionPerChunk);
                chunkActive[chunk] = false;
                chunkActiveUntil[chunk] = 0.0;
            }
        }

        private void CleanupStaleSources(double now)
        {
            staleSourceIds.Clear();

            foreach (KeyValuePair<EntityId, ContinuousSource> pair in continuousSources)
            {
                if (now - pair.Value.LastSeen > SourceStaleSeconds)
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
            return river != null
                ? river.Quality switch
                {
                    StylizedRiverQuality.Low => 12f,
                    StylizedRiverQuality.Medium => 20f,
                    StylizedRiverQuality.High => 30f,
                    _ => 20f
                }
                : 20f;
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
            float localDistance = Mathf.Clamp(
                globalDistance - river.Domain.GlobalDistanceMinimum,
                0f,
                fieldLength);
            return localDistance / Mathf.Max(0.001f, fieldLength) *
                   (fieldWidth - 1);
        }

        private float AcrossToPixel(float acrossNormalized)
        {
            return
                (Mathf.Clamp(acrossNormalized, -1f, 1f) * 0.5f + 0.5f) *
                (fieldHeight - 1);
        }

        private bool HasActiveChunks()
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

        private int CountActiveChunks()
        {
            int count = 0;
            for (int index = 0; index < chunkActive.Length; index++)
            {
                if (chunkActive[index])
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
                previousState == null)
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
            propertyBlock.SetFloat(
                DisturbanceInterpolationId,
                simulationInterpolation);
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
                DisturbanceDebugViewId,
                (float)river.DisturbanceDebugView);
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
            surfaceRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
