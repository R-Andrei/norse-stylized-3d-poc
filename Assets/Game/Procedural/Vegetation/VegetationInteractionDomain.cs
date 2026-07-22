using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Vegetation Interaction Domain")]
    public sealed class VegetationInteractionDomain : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DVegetation/Compute/CS_VegetationInteractionField";
        private const int ThreadGroupSize = 8;
        private const int InteractorRecordStride = 48;

        private static readonly int PreviousFieldId =
            Shader.PropertyToID("_VegetationInteractionPreviousField");
        private static readonly int CurrentFieldId =
            Shader.PropertyToID("_VegetationInteractionCurrentField");
        private static readonly int FieldOriginCellSizeId =
            Shader.PropertyToID("_VegetationInteractionFieldOriginCellSize");
        private static readonly int FieldResolutionOffsetId =
            Shader.PropertyToID("_VegetationInteractionFieldResolutionOffset");
        private static readonly int FieldTimingId =
            Shader.PropertyToID("_VegetationInteractionFieldTiming");

        private static readonly List<VegetationInteractionDomain> ActiveDomainsInternal =
            new List<VegetationInteractionDomain>();
        private static readonly Comparison<InteractorCandidate> CandidateComparison =
            CompareCandidates;

        [Header("Domain Anchor")]
        [SerializeField]
        [Tooltip("Preferred XZ centre for the moving interaction field, normally the gameplay camera follow target or player root.")]
        private Transform fieldAnchor;

        [SerializeField]
        [Tooltip("Fallback camera used when no field anchor is assigned. Its forward ray is projected onto the configured XZ field plane. Camera.main is resolved once when this is empty.")]
        private Camera targetCamera;

        [SerializeField]
        [Tooltip("World-space Y height of the horizontal plane used for fallback camera projection and domain gizmos.")]
        private float fieldPlaneY;

        [Header("Immediate Field")]
        [SerializeField, Range(64, 512)]
        private int fieldResolution = 256;

        [SerializeField, Range(0.1f, 2f)]
        private float cellSizeMetres = 0.25f;

        [SerializeField, Range(0.25f, 8f)]
        [Tooltip("Distance the anchor may move away from the current field centre before one accumulated toroidal recenter occurs.")]
        private float recenterMarginMetres = 1.5f;

        [SerializeField, Range(5f, 60f)]
        [Tooltip("Fixed immediate-interaction update rate. The shader interpolates between field steps; 10 Hz is intentionally supported for testing.")]
        private float updateRateHz = 20f;

        [SerializeField, Range(1, 8)]
        private int maximumStepsPerFrame = 4;

        [SerializeField, Range(1, 96)]
        private int maximumInteractors = 48;

        [Header("Immediate Response")]
        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Time constant used while grass moves toward an occupied actor target.")]
        private float responseTimeSeconds = 0.06f;

        [SerializeField, Range(0.01f, 2f)]
        [Tooltip("Time constant used when immediate displacement returns to zero. This is transient recovery, not trail lifetime.")]
        private float recoveryTimeSeconds = 0.18f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Target strength retained at the previous endpoint of a moving swept capsule. Zero releases the tail completely; one preserves the former uniform sweep.")]
        private float sweepTailRetention = 0.10f;

        [Header("Debug")]
        [SerializeField]
        private bool showFieldBounds = true;

        private ComputeShader computeShader;
        private RenderTexture responseA;
        private RenderTexture responseB;
        private RenderTexture currentResponse;
        private RenderTexture previousResponse;
        private GraphicsBuffer interactorBuffer;
        private GpuInteractorRecord[] uploadRecords;
        private readonly List<InteractorCandidate> candidates =
            new List<InteractorCandidate>(96);
        private Camera resolvedCamera;
        private int initializeKernel = -1;
        private int recenterKernel = -1;
        private int simulateKernel = -1;
        private Vector2Int originCell;
        private Vector2Int ringOffset;
        private bool originInitialized;
        private bool resourcesDirty = true;
        private bool resourcesReady;
        private string lastError = string.Empty;
        private double lastRealtime;
        private float simulationAccumulator;
        private float simulationTime;
        private int lastFrameStepCount;
        private int lastFrameDispatchCount;
        private int totalSimulationDispatchCount;
        private int totalRecenterDispatchCount;
        private int lastRegisteredInteractorCount;
        private int lastCandidateInteractorCount;
        private int lastUploadedInteractorCount;
        private int lastOverflowInteractorCount;
        private int lastValidatedConfigurationHash;
        private bool configurationHashInitialized;

        [StructLayout(LayoutKind.Sequential)]
        private struct GpuInteractorRecord
        {
            public Vector4 StartEnd;
            public Vector4 Parameters;
            public Vector4 DirectionParameters;
        }

        private readonly struct InteractorCandidate
        {
            public InteractorCandidate(
                VegetationInteractorSample sample,
                float distanceSquared,
                int entityHash)
            {
                Sample = sample;
                DistanceSquared = distanceSquared;
                EntityHash = entityHash;
            }

            public VegetationInteractorSample Sample { get; }
            public float DistanceSquared { get; }
            public int EntityHash { get; }
        }

        public static int ActiveDomainCount => ActiveDomainsInternal.Count;
        public static VegetationInteractionDomain PublishedDomain { get; private set; }
        public Transform FieldAnchor => fieldAnchor;
        public Camera TargetCamera => targetCamera != null
            ? targetCamera
            : resolvedCamera;
        public int FieldResolution => fieldResolution;
        public float CellSizeMetres => cellSizeMetres;
        public float FieldWorldSizeMetres => fieldResolution * cellSizeMetres;
        public float RecenterMarginMetres => recenterMarginMetres;
        public float UpdateRateHz => updateRateHz;
        public int MaximumInteractors => maximumInteractors;
        public float ResponseTimeSeconds => responseTimeSeconds;
        public float RecoveryTimeSeconds => recoveryTimeSeconds;
        public float SweepTailRetention => sweepTailRetention;
        public bool ResourcesReady => resourcesReady;
        public string LastError => lastError;
        public Vector2 FieldOriginXZ => new Vector2(
            originCell.x * cellSizeMetres,
            originCell.y * cellSizeMetres);
        public Vector2Int RingOffset => ringOffset;
        public RenderTexture CurrentResponseTexture => currentResponse;
        public RenderTexture PreviousResponseTexture => previousResponse;
        public long EstimatedTextureBytes =>
            (long)fieldResolution * fieldResolution * 8L * 2L;
        public long EstimatedInteractorBufferBytes =>
            (long)maximumInteractors * InteractorRecordStride;
        public int LastFrameStepCount => lastFrameStepCount;
        public int LastFrameDispatchCount => lastFrameDispatchCount;
        public int TotalSimulationDispatchCount => totalSimulationDispatchCount;
        public int TotalRecenterDispatchCount => totalRecenterDispatchCount;
        public int LastRegisteredInteractorCount => lastRegisteredInteractorCount;
        public int LastCandidateInteractorCount => lastCandidateInteractorCount;
        public int LastUploadedInteractorCount => lastUploadedInteractorCount;
        public int LastOverflowInteractorCount => lastOverflowInteractorCount;

        private void OnEnable()
        {
            if (!ActiveDomainsInternal.Contains(this))
            {
                ActiveDomainsInternal.Add(this);
            }

            PublishedDomain = this;
            ResolveCameraOnce();
            lastValidatedConfigurationHash = ComputeResourceConfigurationHash();
            configurationHashInitialized = true;
            resourcesDirty = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            ResetAllInteractorHistories();
            if (Application.isPlaying)
            {
                EnsureResources();
                simulationAccumulator = 1f / Mathf.Max(5f, updateRateHz);
                PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        private void OnDisable()
        {
            ActiveDomainsInternal.Remove(this);
            bool wasPublished = PublishedDomain == this;
            ReleaseResources();
            if (!wasPublished)
            {
                return;
            }

            PublishedDomain = ActiveDomainsInternal.Count > 0
                ? ActiveDomainsInternal[ActiveDomainsInternal.Count - 1]
                : null;
            if (PublishedDomain != null && Application.isPlaying)
            {
                PublishedDomain.resourcesDirty = true;
                PublishedDomain.EnsureResources();
                PublishedDomain.PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        private void OnDestroy()
        {
            ActiveDomainsInternal.Remove(this);
            bool wasPublished = PublishedDomain == this;
            ReleaseResources();
            if (!wasPublished)
            {
                return;
            }

            PublishedDomain = ActiveDomainsInternal.Count > 0
                ? ActiveDomainsInternal[ActiveDomainsInternal.Count - 1]
                : null;
            if (PublishedDomain != null && Application.isPlaying)
            {
                PublishedDomain.resourcesDirty = true;
                PublishedDomain.EnsureResources();
                PublishedDomain.PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        private void OnValidate()
        {
            fieldResolution = Mathf.Clamp(
                Mathf.ClosestPowerOfTwo(fieldResolution),
                64,
                512);
            cellSizeMetres = Mathf.Clamp(cellSizeMetres, 0.1f, 2f);
            recenterMarginMetres = Mathf.Clamp(
                recenterMarginMetres,
                0.25f,
                ComputeMaximumRecenterMarginMetres());
            updateRateHz = Mathf.Clamp(updateRateHz, 5f, 60f);
            maximumStepsPerFrame = Mathf.Clamp(maximumStepsPerFrame, 1, 8);
            maximumInteractors = Mathf.Clamp(maximumInteractors, 1, 96);
            responseTimeSeconds = Mathf.Clamp(responseTimeSeconds, 0.01f, 1f);
            recoveryTimeSeconds = Mathf.Clamp(recoveryTimeSeconds, 0.01f, 2f);
            sweepTailRetention = Mathf.Clamp01(sweepTailRetention);
            resolvedCamera = targetCamera;

            int configurationHash = ComputeResourceConfigurationHash();
            if (!configurationHashInitialized)
            {
                lastValidatedConfigurationHash = configurationHash;
                configurationHashInitialized = true;
            }
            else if (configurationHash != lastValidatedConfigurationHash)
            {
                lastValidatedConfigurationHash = configurationHash;
                resourcesDirty = true;
            }
        }

        private void Update()
        {
            if (PublishedDomain != this)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                ClearShaderGlobals();
                return;
            }

            ResolveCameraOnce();
            lastFrameStepCount = 0;
            lastFrameDispatchCount = 0;
            if (!EnsureResources())
            {
                PublishShaderGlobals();
                return;
            }

            RecenterIfNeeded();
            double now = Time.realtimeSinceStartupAsDouble;
            float elapsed = (float)Math.Max(
                0.0,
                Math.Min(0.25, now - lastRealtime));
            lastRealtime = now;
            simulationAccumulator += elapsed;

            float fixedStep = 1f / Mathf.Max(5f, updateRateHz);
            int stepCount = 0;
            while (simulationAccumulator >= fixedStep &&
                   stepCount < maximumStepsPerFrame)
            {
                simulationTime += fixedStep;
                UploadInteractors(fixedStep);
                DispatchSimulation(fixedStep);
                simulationAccumulator -= fixedStep;
                stepCount++;
            }

            if (stepCount == maximumStepsPerFrame &&
                simulationAccumulator > fixedStep)
            {
                simulationAccumulator = fixedStep;
            }

            lastFrameStepCount = stepCount;
            PublishShaderGlobals();
        }

        public void RequestRebuild()
        {
            resourcesDirty = true;
        }

        public void ResetField()
        {
            if (PublishedDomain != this)
            {
                PublishedDomain = this;
            }

            ResetAllInteractorHistories();
            resourcesDirty = true;
            if (Application.isPlaying)
            {
                EnsureResources();
                simulationAccumulator = 1f / Mathf.Max(5f, updateRateHz);
                PublishShaderGlobals();
            }
            else
            {
                ClearShaderGlobals();
            }
        }

        public Rect GetFieldWorldRectXZ()
        {
            float size = FieldWorldSizeMetres;
            Vector2 origin = FieldOriginXZ;
            return new Rect(origin.x, origin.y, size, size);
        }

        public Vector3 GetResolvedAnchorPosition()
        {
            return ResolveAnchorPosition();
        }

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(2048);
            builder.AppendLine("[Vegetation INTERACT.1B Immediate Domain Report]");
            string status = !Application.isPlaying
                ? "INACTIVE — PLAY MODE SIMULATION NOT RUNNING"
                : resourcesReady
                    ? "READY"
                    : "NOT READY";
            builder.Append("Status: ").AppendLine(status);
            builder.Append("Published domain: ")
                .AppendLine(PublishedDomain == this ? "Yes" : "No");
            builder.Append("Active interaction domains: ")
                .AppendLine(ActiveDomainCount.ToString());
            builder.Append("Field anchor: ")
                .AppendLine(fieldAnchor != null
                    ? fieldAnchor.name
                    : "Camera ground projection");
            builder.Append("Fallback camera: ")
                .AppendLine(TargetCamera != null
                    ? TargetCamera.name
                    : "Component transform");
            builder.Append("Resolved anchor position: ")
                .AppendLine(ResolveAnchorPosition().ToString("F3"));
            builder.Append("XZ field resolution: ")
                .Append(fieldResolution).Append(" × ")
                .AppendLine(fieldResolution.ToString());
            builder.Append("Cell size: ")
                .Append(cellSizeMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("World coverage: ")
                .Append(FieldWorldSizeMetres.ToString("0.###"))
                .Append(" × ")
                .Append(FieldWorldSizeMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Recenter margin: ")
                .Append(recenterMarginMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Update rate: ")
                .Append(updateRateHz.ToString("0.###"))
                .AppendLine(" Hz (allowed 5–60 Hz)");
            builder.Append("Response / recovery: ")
                .Append(responseTimeSeconds.ToString("0.###"))
                .Append(" / ")
                .Append(recoveryTimeSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Sweep tail retention: ")
                .AppendLine(sweepTailRetention.ToString("0.###"));
            builder.AppendLine("Render-time release compensation: Enabled");
            builder.Append("Field origin XZ: ")
                .AppendLine(FieldOriginXZ.ToString("F3"));
            builder.Append("Toroidal offset: ")
                .AppendLine(ringOffset.ToString());
            builder.Append("Estimated texture memory: ")
                .Append(EstimatedTextureBytes.ToString("N0"))
                .AppendLine(" bytes");
            builder.Append("Estimated actor-buffer memory: ")
                .Append(EstimatedInteractorBufferBytes.ToString("N0"))
                .AppendLine(" bytes (48 bytes/interactor)");
            builder.AppendLine(
                "Direction shaping: Per interactor — Radial / World X Biased / Hybrid");
            builder.Append("Registered / candidate / uploaded / overflow: ")
                .Append(lastRegisteredInteractorCount).Append(" / ")
                .Append(lastCandidateInteractorCount).Append(" / ")
                .Append(lastUploadedInteractorCount).Append(" / ")
                .AppendLine(lastOverflowInteractorCount.ToString());
            builder.Append("Last frame interaction steps: ")
                .AppendLine(lastFrameStepCount.ToString());
            builder.Append("Last frame compute dispatches: ")
                .AppendLine(lastFrameDispatchCount.ToString());
            builder.Append("Total simulation dispatches: ")
                .AppendLine(totalSimulationDispatchCount.ToString("N0"));
            builder.Append("Total recenter dispatches: ")
                .AppendLine(totalRecenterDispatchCount.ToString("N0"));
            float recenterPercentage = totalSimulationDispatchCount > 0
                ? totalRecenterDispatchCount * 100f /
                    totalSimulationDispatchCount
                : 0f;
            builder.Append("Recenter dispatches / simulation steps: ")
                .Append(recenterPercentage.ToString("0.0"))
                .AppendLine("%");
            builder.AppendLine("Persistent trail state: Not present in INTERACT.1");
            if (!string.IsNullOrEmpty(lastError))
            {
                builder.Append("Last error: ").AppendLine(lastError);
            }
            return builder.ToString();
        }

        private bool EnsureResources()
        {
            if (!resourcesDirty && ResourcesAreValid())
            {
                return true;
            }

            ReleaseResources();
            lastError = string.Empty;
            if (!SystemInfo.supportsComputeShaders)
            {
                lastError = "Immediate vegetation interaction requires compute-shader support.";
                return false;
            }
            if (!SystemInfo.SupportsRenderTextureFormat(
                    RenderTextureFormat.ARGBHalf))
            {
                lastError = "ARGBHalf random-write textures are not supported on the current platform.";
                return false;
            }
            if (Marshal.SizeOf<GpuInteractorRecord>() != InteractorRecordStride)
            {
                lastError =
                    $"Vegetation interactor GPU record stride mismatch: " +
                    $"expected {InteractorRecordStride}, runtime " +
                    $"{Marshal.SizeOf<GpuInteractorRecord>()}.";
                return false;
            }

            computeShader = Resources.Load<ComputeShader>(ComputeResourcePath);
            if (computeShader == null)
            {
                lastError =
                    "Required vegetation interaction compute shader was not found at Resources/" +
                    ComputeResourcePath + ".";
                return false;
            }

            try
            {
                initializeKernel = computeShader.FindKernel("InitializeField");
                recenterKernel = computeShader.FindKernel("RecenterField");
                simulateKernel = computeShader.FindKernel("SimulateField");
                responseA = CreateTexture("PS3D_VegetationInteraction_A");
                responseB = CreateTexture("PS3D_VegetationInteraction_B");
                currentResponse = responseA;
                previousResponse = responseB;
                interactorBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    maximumInteractors,
                    InteractorRecordStride);
                uploadRecords = new GpuInteractorRecord[maximumInteractors];
                originCell = ComputeDesiredOriginCell();
                ringOffset = Vector2Int.zero;
                originInitialized = true;
                simulationAccumulator = 0f;
                simulationTime = 0f;
                DispatchInitialize();
                resourcesDirty = false;
                resourcesReady = true;
                return true;
            }
            catch (Exception exception)
            {
                lastError = exception.ToString();
                ReleaseResources();
                return false;
            }
        }

        private bool ResourcesAreValid()
        {
            return resourcesReady &&
                   computeShader != null &&
                   responseA != null && responseA.IsCreated() &&
                   responseB != null && responseB.IsCreated() &&
                   interactorBuffer != null &&
                   interactorBuffer.IsValid() &&
                   uploadRecords != null &&
                   uploadRecords.Length == maximumInteractors;
        }

        private RenderTexture CreateTexture(string textureName)
        {
            var texture = new RenderTexture(
                fieldResolution,
                fieldResolution,
                0,
                RenderTextureFormat.ARGBHalf,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Repeat,
                hideFlags = HideFlags.DontSave
            };
            if (!texture.Create())
            {
                DestroyRuntimeObject(texture);
                throw new InvalidOperationException(
                    "Could not create vegetation interaction texture " +
                    textureName + ".");
            }
            return texture;
        }

        private void DispatchInitialize()
        {
            SetCommonComputeParameters(0f);
            computeShader.SetTexture(initializeKernel, "_StateAWrite", responseA);
            computeShader.SetTexture(initializeKernel, "_StateBWrite", responseB);
            Dispatch(initializeKernel);
        }

        private void RecenterIfNeeded()
        {
            Vector2Int desiredOrigin = ComputeDesiredOriginCell();
            if (!originInitialized)
            {
                originCell = desiredOrigin;
                ringOffset = Vector2Int.zero;
                originInitialized = true;
                DispatchInitialize();
                return;
            }

            Vector2Int delta = desiredOrigin - originCell;
            if (delta == Vector2Int.zero)
            {
                return;
            }

            bool resetAll = Mathf.Abs(delta.x) >= fieldResolution ||
                            Mathf.Abs(delta.y) >= fieldResolution;
            originCell = desiredOrigin;
            if (resetAll)
            {
                ringOffset = Vector2Int.zero;
            }
            else
            {
                ringOffset = new Vector2Int(
                    PositiveMod(ringOffset.x + delta.x, fieldResolution),
                    PositiveMod(ringOffset.y + delta.y, fieldResolution));
            }

            SetCommonComputeParameters(0f);
            computeShader.SetInts("_RecenterDelta", delta.x, delta.y);
            computeShader.SetInt("_ResetAll", resetAll ? 1 : 0);
            computeShader.SetTexture(recenterKernel, "_StateAWrite", responseA);
            computeShader.SetTexture(recenterKernel, "_StateBWrite", responseB);
            Dispatch(recenterKernel);
            totalRecenterDispatchCount++;
        }

        private void UploadInteractors(float deltaTime)
        {
            candidates.Clear();
            IReadOnlyList<VegetationInteractor> active =
                VegetationInteractor.ActiveInteractors;
            lastRegisteredInteractorCount = active.Count;
            Rect fieldRect = GetFieldWorldRectXZ();
            Vector3 anchorPosition = ResolveAnchorPosition();
            Vector2 anchorXZ = new Vector2(
                anchorPosition.x,
                anchorPosition.z);

            for (int index = 0; index < active.Count; index++)
            {
                VegetationInteractor interactor = active[index];
                if (interactor == null || !interactor.isActiveAndEnabled)
                {
                    continue;
                }

                VegetationInteractorSample sample =
                    interactor.CaptureSample(deltaTime);
                if (!SweptCircleIntersectsRect(sample, fieldRect))
                {
                    continue;
                }

                float distanceSquared =
                    (sample.EndXZ - anchorXZ).sqrMagnitude;
                candidates.Add(new InteractorCandidate(
                    sample,
                    distanceSquared,
                    interactor.GetEntityId().GetHashCode()));
            }

            candidates.Sort(CandidateComparison);
            lastCandidateInteractorCount = candidates.Count;
            lastUploadedInteractorCount = Mathf.Min(
                maximumInteractors,
                candidates.Count);
            lastOverflowInteractorCount = Mathf.Max(
                0,
                candidates.Count - lastUploadedInteractorCount);

            for (int index = 0; index < lastUploadedInteractorCount; index++)
            {
                VegetationInteractorSample sample = candidates[index].Sample;
                uploadRecords[index] = new GpuInteractorRecord
                {
                    StartEnd = new Vector4(
                        sample.StartXZ.x,
                        sample.StartXZ.y,
                        sample.EndXZ.x,
                        sample.EndXZ.y),
                    Parameters = new Vector4(
                        sample.Radius,
                        sample.BendStrength,
                        sample.FlattenStrength,
                        sample.MovementBlend),
                    DirectionParameters = new Vector4(
                        (float)sample.DirectionMode,
                        sample.WorldXBias,
                        sample.WorldZStrength,
                        0f)
                };
            }

            if (lastUploadedInteractorCount > 0)
            {
                interactorBuffer.SetData(
                    uploadRecords,
                    0,
                    0,
                    lastUploadedInteractorCount);
            }
        }

        private static int CompareCandidates(
            InteractorCandidate left,
            InteractorCandidate right)
        {
            int priorityComparison =
                right.Sample.Priority.CompareTo(left.Sample.Priority);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }

            int distanceComparison =
                left.DistanceSquared.CompareTo(right.DistanceSquared);
            if (distanceComparison != 0)
            {
                return distanceComparison;
            }

            return left.EntityHash.CompareTo(right.EntityHash);
        }

        private static bool SweptCircleIntersectsRect(
            VegetationInteractorSample sample,
            Rect rect)
        {
            float minimumX = Mathf.Min(sample.StartXZ.x, sample.EndXZ.x) -
                sample.Radius;
            float maximumX = Mathf.Max(sample.StartXZ.x, sample.EndXZ.x) +
                sample.Radius;
            float minimumZ = Mathf.Min(sample.StartXZ.y, sample.EndXZ.y) -
                sample.Radius;
            float maximumZ = Mathf.Max(sample.StartXZ.y, sample.EndXZ.y) +
                sample.Radius;
            return maximumX >= rect.xMin &&
                   minimumX <= rect.xMax &&
                   maximumZ >= rect.yMin &&
                   minimumZ <= rect.yMax;
        }

        private void DispatchSimulation(float deltaTime)
        {
            SetCommonComputeParameters(deltaTime);
            computeShader.SetInt(
                "_InteractorCount",
                lastUploadedInteractorCount);
            computeShader.SetBuffer(
                simulateKernel,
                "_Interactors",
                interactorBuffer);
            computeShader.SetTexture(
                simulateKernel,
                "_StateRead",
                currentResponse);
            computeShader.SetTexture(
                simulateKernel,
                "_StateWrite",
                previousResponse);
            Dispatch(simulateKernel);

            RenderTexture oldCurrent = currentResponse;
            currentResponse = previousResponse;
            previousResponse = oldCurrent;
            totalSimulationDispatchCount++;
        }

        private void SetCommonComputeParameters(float deltaTime)
        {
            computeShader.SetInts(
                "_FieldResolution",
                fieldResolution,
                fieldResolution);
            computeShader.SetInts(
                "_FieldOffset",
                ringOffset.x,
                ringOffset.y);
            computeShader.SetVector(
                "_FieldOriginCellSize",
                new Vector4(
                    originCell.x * cellSizeMetres,
                    originCell.y * cellSizeMetres,
                    cellSizeMetres,
                    0f));
            computeShader.SetFloat("_DeltaTime", deltaTime);
            computeShader.SetVector(
                "_ResponseParameters",
                new Vector4(
                    responseTimeSeconds,
                    recoveryTimeSeconds,
                    sweepTailRetention,
                    0f));
        }

        private void Dispatch(int kernel)
        {
            int groupCount = Mathf.CeilToInt(
                fieldResolution / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupCount, groupCount, 1);
            lastFrameDispatchCount++;
        }

        private void PublishShaderGlobals()
        {
            if (PublishedDomain != this)
            {
                return;
            }

            if (!resourcesReady ||
                currentResponse == null ||
                previousResponse == null)
            {
                ClearShaderGlobals();
                return;
            }

            float fixedStep = 1f / Mathf.Max(5f, updateRateHz);
            float interpolation = Mathf.Clamp01(
                simulationAccumulator / fixedStep);
            Shader.SetGlobalTexture(PreviousFieldId, previousResponse);
            Shader.SetGlobalTexture(CurrentFieldId, currentResponse);
            Shader.SetGlobalVector(
                FieldOriginCellSizeId,
                new Vector4(
                    originCell.x * cellSizeMetres,
                    originCell.y * cellSizeMetres,
                    cellSizeMetres,
                    1f));
            Shader.SetGlobalVector(
                FieldResolutionOffsetId,
                new Vector4(
                    fieldResolution,
                    fieldResolution,
                    ringOffset.x,
                    ringOffset.y));
            Shader.SetGlobalVector(
                FieldTimingId,
                new Vector4(
                    interpolation,
                    fixedStep,
                    recoveryTimeSeconds,
                    1f));
        }

        private static void ClearShaderGlobals()
        {
            Shader.SetGlobalTexture(PreviousFieldId, Texture2D.blackTexture);
            Shader.SetGlobalTexture(CurrentFieldId, Texture2D.blackTexture);
            Shader.SetGlobalVector(FieldOriginCellSizeId, Vector4.zero);
            Shader.SetGlobalVector(FieldResolutionOffsetId, Vector4.zero);
            Shader.SetGlobalVector(FieldTimingId, Vector4.zero);
        }

        private Vector2Int ComputeDesiredOriginCell()
        {
            Vector3 anchor = ResolveAnchorPosition();
            Vector2Int anchorCell = new Vector2Int(
                Mathf.FloorToInt(anchor.x / cellSizeMetres),
                Mathf.FloorToInt(anchor.z / cellSizeMetres));
            int halfResolution = fieldResolution / 2;
            Vector2Int centredOrigin = new Vector2Int(
                anchorCell.x - halfResolution,
                anchorCell.y - halfResolution);
            if (!originInitialized)
            {
                return centredOrigin;
            }

            Vector2Int currentCentre = originCell + new Vector2Int(
                halfResolution,
                halfResolution);
            Vector2Int centreDelta = anchorCell - currentCentre;
            int marginCells = Mathf.Max(
                1,
                Mathf.CeilToInt(recenterMarginMetres / cellSizeMetres));
            if (Mathf.Abs(centreDelta.x) <= marginCells &&
                Mathf.Abs(centreDelta.y) <= marginCells)
            {
                return originCell;
            }

            return centredOrigin;
        }

        private float ComputeMaximumRecenterMarginMetres()
        {
            float halfExtent = fieldResolution * cellSizeMetres * 0.5f;
            return Mathf.Max(
                0.25f,
                Mathf.Min(8f, halfExtent - cellSizeMetres * 2f));
        }

        private Vector3 ResolveAnchorPosition()
        {
            if (fieldAnchor != null)
            {
                return fieldAnchor.position;
            }

            Camera camera = TargetCamera;
            if (camera == null)
            {
                return new Vector3(
                    transform.position.x,
                    fieldPlaneY,
                    transform.position.z);
            }

            Vector3 cameraPosition = camera.transform.position;
            Vector3 cameraForward = camera.transform.forward;
            if (Mathf.Abs(cameraForward.y) > 0.0001f)
            {
                float projectionDistance =
                    (fieldPlaneY - cameraPosition.y) / cameraForward.y;
                if (projectionDistance >= 0f)
                {
                    return cameraPosition +
                        cameraForward * projectionDistance;
                }
            }

            return new Vector3(
                cameraPosition.x,
                fieldPlaneY,
                cameraPosition.z);
        }

        private void ResolveCameraOnce()
        {
            if (targetCamera != null)
            {
                resolvedCamera = targetCamera;
                return;
            }

            if (resolvedCamera == null)
            {
                resolvedCamera = Camera.main;
            }
        }

        private int ComputeResourceConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + fieldResolution;
                hash = hash * 31 + cellSizeMetres.GetHashCode();
                hash = hash * 31 + maximumInteractors;
                return hash;
            }
        }

        private void ResetAllInteractorHistories()
        {
            IReadOnlyList<VegetationInteractor> active =
                VegetationInteractor.ActiveInteractors;
            for (int index = 0; index < active.Count; index++)
            {
                VegetationInteractor interactor = active[index];
                if (interactor != null)
                {
                    interactor.ResetSampleHistory();
                }
            }
        }

        private void ReleaseResources()
        {
            resourcesReady = false;
            ReleaseTexture(ref responseA);
            ReleaseTexture(ref responseB);
            currentResponse = null;
            previousResponse = null;
            interactorBuffer?.Release();
            interactorBuffer = null;
            uploadRecords = null;
            computeShader = null;
            initializeKernel = -1;
            recenterKernel = -1;
            simulateKernel = -1;
            originInitialized = false;
        }

        private static void ReleaseTexture(ref RenderTexture texture)
        {
            if (texture == null)
            {
                return;
            }

            texture.Release();
            DestroyRuntimeObject(texture);
            texture = null;
        }

        private static void DestroyRuntimeObject(UnityEngine.Object target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                UnityEngine.Object.Destroy(target);
            }
            else
            {
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static int PositiveMod(int value, int modulus)
        {
            int remainder = value % modulus;
            return remainder < 0 ? remainder + modulus : remainder;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showFieldBounds)
            {
                return;
            }

            Vector3 anchor = ResolveAnchorPosition();
            float size = FieldWorldSizeMetres;
            Vector2 fieldOrigin = originInitialized
                ? FieldOriginXZ
                : new Vector2(
                    anchor.x - size * 0.5f,
                    anchor.z - size * 0.5f);
            Vector3 fieldCentre = new Vector3(
                fieldOrigin.x + size * 0.5f,
                fieldPlaneY,
                fieldOrigin.y + size * 0.5f);
            Color previousColor = Gizmos.color;
            Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.85f);
            Gizmos.DrawWireCube(
                fieldCentre,
                new Vector3(size, 0.05f, size));
            Gizmos.DrawWireCube(
                fieldCentre,
                new Vector3(
                    recenterMarginMetres * 2f,
                    0.08f,
                    recenterMarginMetres * 2f));
            Gizmos.DrawSphere(anchor, 0.12f);
            Gizmos.color = previousColor;
        }
    }
}
