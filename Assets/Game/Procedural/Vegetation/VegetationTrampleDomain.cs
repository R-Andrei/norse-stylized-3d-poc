using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(GroundVegetation))]
    [AddComponentMenu("PS3D/Vegetation/Vegetation Trample Domain")]
    public sealed class VegetationTrampleDomain : MonoBehaviour
    {
        private const string ComputeResourcePath =
            "PS3DVegetation/Compute/CS_VegetationTrampleField";
        private const int ThreadGroupSize = 8;
        private const int WriterRecordStride = 64;
        private const int StampRecordStride = 96;

        private static readonly int PreviousFieldId =
            Shader.PropertyToID("_VegetationTramplePreviousField");
        private static readonly int CurrentFieldId =
            Shader.PropertyToID("_VegetationTrampleCurrentField");
        private static readonly int WorldToLocalId =
            Shader.PropertyToID("_VegetationTrampleWorldToLocal");
        private static readonly int DomainParametersId =
            Shader.PropertyToID("_VegetationTrampleDomainParameters");
        private static readonly List<VegetationTrampleDomain> ActiveDomainsInternal =
            new List<VegetationTrampleDomain>();
        private static readonly Comparison<WriterCandidate> CandidateComparison =
            CompareCandidates;
        private static readonly Comparison<QueuedStamp> StampComparison =
            CompareQueuedStamps;

        [Header("Historical Field")]
        [SerializeField, Range(64, 512)]
        private int fieldResolution = 256;

        [SerializeField, Range(5f, 60f)]
        [Tooltip("Fixed historical-trample update rate. This cadence is independent from immediate interaction.")]
        private float updateRateHz = 12f;

        [SerializeField, Range(1, 8)]
        private int maximumStepsPerFrame = 4;

        [SerializeField, Range(1, 96)]
        private int maximumTrailWriters = 48;

        [Header("Ability Stamps")]
        [SerializeField, Range(1, 64)]
        [Tooltip("Maximum queued one-shot ability stamps uploaded to one historical fixed step.")]
        private int maximumAbilityStampsPerStep = 32;

        [SerializeField, Range(1, 512)]
        [Tooltip("Maximum validated one-shot ability requests retained until a historical fixed step consumes them.")]
        private int maximumQueuedAbilityStamps = 128;

        [Header("Debug")]
        [SerializeField]
        private bool showFieldBounds = true;

        private GroundVegetation vegetationRoot;
        private GeneratedGround surfaceGround;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture currentState;
        private RenderTexture previousState;
        private RenderTexture timingState;
        private GraphicsBuffer writerBuffer;
        private GpuWriterRecord[] uploadRecords;
        private GraphicsBuffer stampBuffer;
        private GpuStampRecord[] stampUploadRecords;
        private readonly List<WriterCandidate> candidates =
            new List<WriterCandidate>(96);
        private readonly Dictionary<VegetationInteractor, TrailHistory> histories =
            new Dictionary<VegetationInteractor, TrailHistory>(96);
        private readonly List<VegetationInteractor> staleHistoryKeys =
            new List<VegetationInteractor>(32);
        private readonly List<QueuedStamp> pendingAbilityStamps =
            new List<QueuedStamp>(128);
        private int initializeKernel = -1;
        private int simulateKernel = -1;
        private bool resourcesDirty = true;
        private bool resourcesReady;
        private string lastError = string.Empty;
        private float simulationAccumulator;
        private double lastRealtime;
        private int lastValidatedConfigurationHash;
        private bool configurationHashInitialized;
        private int lastSurfaceRevision = int.MinValue;
        private float resolvedHalfSize;
        private float resolvedDomainSize;
        private int lastFrameStepCount;
        private int lastFrameDispatchCount;
        private int totalSimulationDispatchCount;
        private int lastRegisteredInteractorCount;
        private int lastCandidateWriterCount;
        private int lastUploadedWriterCount;
        private int lastOverflowWriterCount;
        private int lastUploadedAbilityStampCount;
        private int totalAcceptedAbilityStampCount;
        private int totalRejectedAbilityStampCount;
        private int totalReplacedAbilityStampCount;
        private ulong nextAbilityStampSequence;

        [StructLayout(LayoutKind.Sequential)]
        private struct GpuWriterRecord
        {
            public Vector4 StartEnd;
            public Vector4 Parameters;
            public Vector4 DirectionParameters;
            public Vector4 PersistenceParameters;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct FloatUIntBits
        {
            [FieldOffset(0)]
            public uint UIntValue;

            [FieldOffset(0)]
            public float FloatValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GpuStampRecord
        {
            public Vector4 OriginEnd;
            public Vector4 ShapeParameters;
            public Vector4 DirectionParameters;
            public Vector4 EffectParameters;
            public Vector4 RecoveryParameters;
            public Vector4 FixedDirectionParameters;
        }

        private struct TrailHistory
        {
            public Vector3 PreviousProbePosition;
            public Vector3 LastStampPosition;
            public bool Initialized;
        }

        private readonly struct QueuedStamp
        {
            public QueuedStamp(
                VegetationTrampleStampRequest request,
                ulong sequence)
            {
                Request = request;
                Sequence = sequence;
            }

            public VegetationTrampleStampRequest Request { get; }
            public ulong Sequence { get; }
        }

        private readonly struct WriterCandidate
        {
            public WriterCandidate(
                VegetationInteractor source,
                Vector2 startXZ,
                Vector2 endXZ,
                float movementBlend,
                float distanceSquared)
            {
                Source = source;
                StartXZ = startXZ;
                EndXZ = endXZ;
                MovementBlend = movementBlend;
                DistanceSquared = distanceSquared;
            }

            public VegetationInteractor Source { get; }
            public Vector2 StartXZ { get; }
            public Vector2 EndXZ { get; }
            public float MovementBlend { get; }
            public float DistanceSquared { get; }
        }

        public static IReadOnlyList<VegetationTrampleDomain> ActiveDomains =>
            ActiveDomainsInternal;
        public GroundVegetation VegetationRoot => vegetationRoot;
        public GeneratedGround SurfaceGround => surfaceGround;
        public int FieldResolution => fieldResolution;
        public float UpdateRateHz => updateRateHz;
        public int MaximumTrailWriters => maximumTrailWriters;
        public int MaximumAbilityStampsPerStep => maximumAbilityStampsPerStep;
        public int MaximumQueuedAbilityStamps => maximumQueuedAbilityStamps;
        public bool ResourcesReady => resourcesReady;
        public string LastError => lastError;
        public long EstimatedTextureBytes =>
            (long)fieldResolution * fieldResolution * (8L * 2L + 8L);
        public long EstimatedWriterBufferBytes =>
            (long)maximumTrailWriters * WriterRecordStride;
        public long EstimatedAbilityStampBufferBytes =>
            (long)maximumAbilityStampsPerStep * StampRecordStride;
        public int LastFrameStepCount => lastFrameStepCount;
        public int LastFrameDispatchCount => lastFrameDispatchCount;
        public int TotalSimulationDispatchCount => totalSimulationDispatchCount;
        public int LastRegisteredInteractorCount => lastRegisteredInteractorCount;
        public int LastCandidateWriterCount => lastCandidateWriterCount;
        public int LastUploadedWriterCount => lastUploadedWriterCount;
        public int LastOverflowWriterCount => lastOverflowWriterCount;
        public int PendingAbilityStampCount => pendingAbilityStamps.Count;
        public int LastUploadedAbilityStampCount => lastUploadedAbilityStampCount;
        public int TotalAcceptedAbilityStampCount => totalAcceptedAbilityStampCount;
        public int TotalRejectedAbilityStampCount => totalRejectedAbilityStampCount;
        public int TotalReplacedAbilityStampCount => totalReplacedAbilityStampCount;

        private void OnEnable()
        {
            if (!ActiveDomainsInternal.Contains(this))
            {
                ActiveDomainsInternal.Add(this);
            }

            ResolveOwnership();
            lastValidatedConfigurationHash = ComputeResourceConfigurationHash();
            configurationHashInitialized = true;
            resourcesDirty = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            if (Application.isPlaying)
            {
                EnsureResources();
                simulationAccumulator = 1f / Mathf.Max(5f, updateRateHz);
            }
        }

        private void OnDisable()
        {
            ActiveDomainsInternal.Remove(this);
            ReleaseResources();
            histories.Clear();
            pendingAbilityStamps.Clear();
        }

        private void OnDestroy()
        {
            ActiveDomainsInternal.Remove(this);
            ReleaseResources();
            histories.Clear();
            pendingAbilityStamps.Clear();
        }

        private void OnValidate()
        {
            fieldResolution = Mathf.Clamp(
                Mathf.ClosestPowerOfTwo(fieldResolution),
                64,
                512);
            updateRateHz = Mathf.Clamp(updateRateHz, 5f, 60f);
            maximumStepsPerFrame = Mathf.Clamp(maximumStepsPerFrame, 1, 8);
            maximumTrailWriters = Mathf.Clamp(maximumTrailWriters, 1, 96);
            maximumAbilityStampsPerStep = Mathf.Clamp(
                maximumAbilityStampsPerStep,
                1,
                64);
            maximumQueuedAbilityStamps = Mathf.Clamp(
                maximumQueuedAbilityStamps,
                maximumAbilityStampsPerStep,
                512);
            ResolveOwnership();

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

        private void OnTransformParentChanged()
        {
            ResolveOwnership();
            resourcesDirty = true;
        }

        private void Update()
        {
            lastFrameStepCount = 0;
            lastFrameDispatchCount = 0;
            if (!Application.isPlaying)
            {
                return;
            }

            if (!EnsureResources())
            {
                return;
            }

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
                UploadTrailWriters(fixedStep);
                UploadAbilityStamps();
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
        }

        public static int SubmitStamp(VegetationTrampleStampRequest request)
        {
            if (!Application.isPlaying ||
                !request.TryGetValidated(
                    out VegetationTrampleStampRequest validated,
                    out _))
            {
                return 0;
            }

            int acceptedDomains = 0;
            for (int index = 0; index < ActiveDomainsInternal.Count; index++)
            {
                VegetationTrampleDomain domain = ActiveDomainsInternal[index];
                if (domain != null && domain.isActiveAndEnabled &&
                    domain.TryQueueValidatedStamp(validated))
                {
                    acceptedDomains++;
                }
            }
            return acceptedDomains;
        }

        public bool TryQueueStamp(
            VegetationTrampleStampRequest request,
            out string error)
        {
            if (!Application.isPlaying)
            {
                error = "Ability trample stamps require Play Mode.";
                totalRejectedAbilityStampCount++;
                return false;
            }
            if (!request.TryGetValidated(
                    out VegetationTrampleStampRequest validated,
                    out error))
            {
                totalRejectedAbilityStampCount++;
                return false;
            }
            if (!TryQueueValidatedStamp(validated))
            {
                error = "The stamp did not intersect this Ground or the bounded request queue rejected it.";
                return false;
            }
            error = string.Empty;
            return true;
        }

        private bool TryQueueValidatedStamp(
            VegetationTrampleStampRequest request)
        {
            ResolveOwnership();
            if (surfaceGround == null ||
                CountDomainsForGround(surfaceGround) != 1 ||
                !StampIntersectsGround(request))
            {
                return false;
            }

            var queued = new QueuedStamp(request, nextAbilityStampSequence++);
            if (pendingAbilityStamps.Count < maximumQueuedAbilityStamps)
            {
                pendingAbilityStamps.Add(queued);
                totalAcceptedAbilityStampCount++;
                return true;
            }

            int worstIndex = 0;
            for (int index = 1; index < pendingAbilityStamps.Count; index++)
            {
                QueuedStamp candidate = pendingAbilityStamps[index];
                QueuedStamp worst = pendingAbilityStamps[worstIndex];
                if (candidate.Request.Priority < worst.Request.Priority ||
                    (candidate.Request.Priority == worst.Request.Priority &&
                     candidate.Sequence > worst.Sequence))
                {
                    worstIndex = index;
                }
            }

            QueuedStamp currentWorst = pendingAbilityStamps[worstIndex];
            if (request.Priority <= currentWorst.Request.Priority)
            {
                totalRejectedAbilityStampCount++;
                return false;
            }

            pendingAbilityStamps[worstIndex] = queued;
            totalAcceptedAbilityStampCount++;
            totalReplacedAbilityStampCount++;
            return true;
        }

        private bool StampIntersectsGround(
            VegetationTrampleStampRequest request)
        {
            if (surfaceGround == null ||
                !surfaceGround.TryGetSurfaceDomain(
                    out resolvedHalfSize,
                    out resolvedDomainSize))
            {
                return false;
            }

            Vector2 startXZ = new Vector2(request.Origin.x, request.Origin.z);
            Vector2 endXZ = request.Shape == VegetationTrampleStampShape.Line
                ? new Vector2(request.End.x, request.End.z)
                : startXZ;
            float radius = request.Shape == VegetationTrampleStampShape.Line
                ? request.LineWidth * 0.5f
                : request.Radius;
            return SweptCircleIntersectsGround(startXZ, endXZ, radius);
        }

        public void RequestRebuild()
        {
            resourcesDirty = true;
        }

        public void ResetField()
        {
            histories.Clear();
            pendingAbilityStamps.Clear();
            lastUploadedAbilityStampCount = 0;
            resourcesDirty = true;
            if (Application.isPlaying)
            {
                EnsureResources();
                simulationAccumulator = 1f / Mathf.Max(5f, updateRateHz);
            }
        }

        public string BuildReport()
        {
            ResolveOwnership();
            int matchingDomains = CountDomainsForGround(surfaceGround);
            var builder = new StringBuilder(2048);
            builder.AppendLine("[Vegetation INTERACT.2B Historical Trample Domain Report]");
            builder.Append("Status: ")
                .AppendLine(!Application.isPlaying
                    ? "INACTIVE — PLAY MODE SIMULATION NOT RUNNING"
                    : resourcesReady ? "READY" : "NOT READY");
            if (surfaceGround != null &&
                surfaceGround.TryGetSurfaceDomain(
                    out float reportHalfSize,
                    out float reportDomainSize))
            {
                resolvedHalfSize = reportHalfSize;
                resolvedDomainSize = reportDomainSize;
            }
            builder.Append("Vegetation root: ")
                .AppendLine(vegetationRoot != null ? vegetationRoot.name : "None");
            builder.Append("Resolved Ground: ")
                .AppendLine(surfaceGround != null ? surfaceGround.name : "None");
            builder.Append("Domains targeting this Ground: ")
                .AppendLine(matchingDomains.ToString());
            builder.Append("XZ field resolution: ")
                .Append(fieldResolution).Append(" × ").AppendLine(fieldResolution.ToString());
            builder.Append("Ground-local coverage: ")
                .Append(resolvedDomainSize.ToString("0.###"))
                .Append(" × ")
                .Append(resolvedDomainSize.ToString("0.###"))
                .AppendLine(" local m");
            builder.Append("Update rate: ")
                .Append(updateRateHz.ToString("0.###"))
                .AppendLine(" Hz (allowed 5–60 Hz)");
            builder.Append("Estimated texture memory: ")
                .Append(EstimatedTextureBytes.ToString("N0"))
                .AppendLine(" bytes");
            builder.Append("Estimated writer-buffer memory: ")
                .Append(EstimatedWriterBufferBytes.ToString("N0"))
                .Append(" bytes (")
                .Append(WriterRecordStride)
                .AppendLine(" bytes/writer)");
            builder.Append("Estimated ability-stamp buffer memory: ")
                .Append(EstimatedAbilityStampBufferBytes.ToString("N0"))
                .Append(" bytes (")
                .Append(StampRecordStride)
                .AppendLine(" bytes/stamp)");
            builder.Append("Registered / candidate / uploaded / overflow: ")
                .Append(lastRegisteredInteractorCount).Append(" / ")
                .Append(lastCandidateWriterCount).Append(" / ")
                .Append(lastUploadedWriterCount).Append(" / ")
                .AppendLine(lastOverflowWriterCount.ToString());
            builder.Append("Ability pending / uploaded this step / queue capacity: ")
                .Append(pendingAbilityStamps.Count).Append(" / ")
                .Append(lastUploadedAbilityStampCount).Append(" / ")
                .AppendLine(maximumQueuedAbilityStamps.ToString());
            builder.Append("Ability accepted / rejected / replaced: ")
                .Append(totalAcceptedAbilityStampCount).Append(" / ")
                .Append(totalRejectedAbilityStampCount).Append(" / ")
                .AppendLine(totalReplacedAbilityStampCount.ToString());
            builder.Append("Last frame trample steps: ")
                .AppendLine(lastFrameStepCount.ToString());
            builder.Append("Last frame compute dispatches: ")
                .AppendLine(lastFrameDispatchCount.ToString());
            builder.Append("Total simulation dispatches: ")
                .AppendLine(totalSimulationDispatchCount.ToString("N0"));
            builder.AppendLine("Timed recovery: full hold delay, then asymmetric slow-fast-slow return (50% time ≈ 15% restored; 90% time ≈ 90% restored)");
            builder.AppendLine("Recovery modes: Timed / Session Persistent");
            builder.AppendLine("Ability stamps: Circle / Cone radial sectors and Line capsules");
            builder.AppendLine("Ability displacement: Radial Outward / Fixed World Direction / Away From Centreline / Flatten Only");
            if (!string.IsNullOrEmpty(lastError))
            {
                builder.Append("Last error: ").AppendLine(lastError);
            }
            return builder.ToString();
        }

        internal static void BindToMaterial(
            Material material,
            GeneratedGround ground)
        {
            if (material == null)
            {
                return;
            }

            VegetationTrampleDomain domain = FindDomainForGround(ground);
            if (domain == null || !domain.resourcesReady ||
                domain.currentState == null || domain.previousState == null ||
                domain.surfaceGround == null)
            {
                material.SetTexture(PreviousFieldId, Texture2D.blackTexture);
                material.SetTexture(CurrentFieldId, Texture2D.blackTexture);
                material.SetMatrix(WorldToLocalId, Matrix4x4.identity);
                material.SetVector(DomainParametersId, Vector4.zero);
                return;
            }

            float fixedStep = 1f / Mathf.Max(5f, domain.updateRateHz);
            float interpolation = Mathf.Clamp01(
                domain.simulationAccumulator / fixedStep);
            material.SetTexture(PreviousFieldId, domain.previousState);
            material.SetTexture(CurrentFieldId, domain.currentState);
            material.SetMatrix(
                WorldToLocalId,
                domain.surfaceGround.transform.worldToLocalMatrix);
            material.SetVector(
                DomainParametersId,
                new Vector4(
                    domain.resolvedHalfSize,
                    domain.resolvedDomainSize,
                    interpolation,
                    1f));
        }

        private static VegetationTrampleDomain FindDomainForGround(
            GeneratedGround ground)
        {
            if (ground == null)
            {
                return null;
            }

            VegetationTrampleDomain result = null;
            for (int index = 0; index < ActiveDomainsInternal.Count; index++)
            {
                VegetationTrampleDomain candidate = ActiveDomainsInternal[index];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }
                if (candidate.surfaceGround == ground)
                {
                    result = candidate;
                }
            }
            return result;
        }

        private static int CountDomainsForGround(GeneratedGround ground)
        {
            if (ground == null)
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < ActiveDomainsInternal.Count; index++)
            {
                VegetationTrampleDomain candidate = ActiveDomainsInternal[index];
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }
                if (candidate.surfaceGround == ground)
                {
                    count++;
                }
            }
            return count;
        }

        private void ResolveOwnership()
        {
            vegetationRoot = GetComponent<GroundVegetation>();
            surfaceGround = vegetationRoot != null
                ? vegetationRoot.SurfaceGround
                : GetComponentInParent<GeneratedGround>(true);
        }

        private bool EnsureResources()
        {
            if (!Application.isPlaying)
            {
                return false;
            }

            if (surfaceGround == null ||
                !surfaceGround.TryGetSurfaceDomain(
                    out float halfSize,
                    out float domainSize))
            {
                lastError = "VegetationTrampleDomain requires a valid GeneratedGround surface domain.";
                ReleaseResources();
                return false;
            }
            if (CountDomainsForGround(surfaceGround) > 1)
            {
                lastError = "Multiple active VegetationTrampleDomain components target the same GeneratedGround. Keep exactly one historical field per Ground.";
                ReleaseResources();
                return false;
            }

            int surfaceRevision = surfaceGround.SurfaceGeometryRevision;
            if (surfaceRevision != lastSurfaceRevision ||
                !Mathf.Approximately(domainSize, resolvedDomainSize))
            {
                lastSurfaceRevision = surfaceRevision;
                resolvedHalfSize = halfSize;
                resolvedDomainSize = domainSize;
                resourcesDirty = true;
            }

            if (!resourcesDirty && ResourcesAreValid())
            {
                return true;
            }

            ReleaseResources();
            try
            {
                computeShader = Resources.Load<ComputeShader>(ComputeResourcePath);
                if (computeShader == null)
                {
                    throw new InvalidOperationException(
                        "Missing compute shader at Resources/" +
                        ComputeResourcePath + ".");
                }

                initializeKernel = computeShader.FindKernel("InitializeField");
                simulateKernel = computeShader.FindKernel("SimulateField");
                stateA = CreateDeformationTexture("PS3D_VegetationTrample_A");
                stateB = CreateDeformationTexture("PS3D_VegetationTrample_B");
                timingState = CreateTimingTexture("PS3D_VegetationTrample_Timing");
                currentState = stateA;
                previousState = stateB;
                writerBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    maximumTrailWriters,
                    WriterRecordStride);
                uploadRecords = new GpuWriterRecord[maximumTrailWriters];
                stampBuffer = new GraphicsBuffer(
                    GraphicsBuffer.Target.Structured,
                    maximumAbilityStampsPerStep,
                    StampRecordStride);
                stampUploadRecords = new GpuStampRecord[
                    maximumAbilityStampsPerStep];
                PrimeStampBufferWithZeroRecord();
                simulationAccumulator = 0f;
                DispatchInitialize();
                resourcesDirty = false;
                resourcesReady = true;
                lastError = string.Empty;
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
                   stateA != null && stateA.IsCreated() &&
                   stateB != null && stateB.IsCreated() &&
                   timingState != null && timingState.IsCreated() &&
                   writerBuffer != null && writerBuffer.IsValid() &&
                   uploadRecords != null &&
                   uploadRecords.Length == maximumTrailWriters &&
                   stampBuffer != null && stampBuffer.IsValid() &&
                   stampUploadRecords != null &&
                   stampUploadRecords.Length == maximumAbilityStampsPerStep;
        }

        private RenderTexture CreateDeformationTexture(string textureName)
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
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            if (!texture.Create())
            {
                DestroyRuntimeObject(texture);
                throw new InvalidOperationException(
                    "Could not create vegetation trample texture " +
                    textureName + ".");
            }
            return texture;
        }

        private RenderTexture CreateTimingTexture(string textureName)
        {
            var texture = new RenderTexture(
                fieldResolution,
                fieldResolution,
                0,
                RenderTextureFormat.RGFloat,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            if (!texture.Create())
            {
                DestroyRuntimeObject(texture);
                throw new InvalidOperationException(
                    "Could not create vegetation trample timing texture " +
                    textureName + ".");
            }
            return texture;
        }

        private void DispatchInitialize()
        {
            SetCommonComputeParameters(0f);
            computeShader.SetTexture(initializeKernel, "_StateAWrite", stateA);
            computeShader.SetTexture(initializeKernel, "_StateBWrite", stateB);
            computeShader.SetTexture(initializeKernel, "_TimingWrite", timingState);
            Dispatch(initializeKernel);
        }

        private void UploadTrailWriters(float deltaTime)
        {
            candidates.Clear();
            IReadOnlyList<VegetationInteractor> active =
                VegetationInteractor.ActiveInteractors;
            lastRegisteredInteractorCount = active.Count;
            Vector3 groundCentre = surfaceGround.transform.position;

            for (int index = 0; index < active.Count; index++)
            {
                VegetationInteractor interactor = active[index];
                if (interactor == null || !interactor.isActiveAndEnabled ||
                    interactor.TrailMode == VegetationTrailMode.Off)
                {
                    continue;
                }

                if (!TryCaptureTrailSegment(
                        interactor,
                        deltaTime,
                        out Vector2 startXZ,
                        out Vector2 endXZ,
                        out float movementBlend))
                {
                    continue;
                }

                if (!SweptCircleIntersectsGround(
                        startXZ,
                        endXZ,
                        interactor.TrailRadius))
                {
                    continue;
                }

                Vector2 groundXZ = new Vector2(groundCentre.x, groundCentre.z);
                candidates.Add(new WriterCandidate(
                    interactor,
                    startXZ,
                    endXZ,
                    movementBlend,
                    (endXZ - groundXZ).sqrMagnitude));
            }

            PruneStaleHistories();
            candidates.Sort(CandidateComparison);
            lastCandidateWriterCount = candidates.Count;
            lastUploadedWriterCount = Mathf.Min(
                maximumTrailWriters,
                candidates.Count);
            lastOverflowWriterCount = Mathf.Max(
                0,
                candidates.Count - lastUploadedWriterCount);

            for (int index = 0; index < lastUploadedWriterCount; index++)
            {
                WriterCandidate candidate = candidates[index];
                VegetationInteractor interactor = candidate.Source;
                uploadRecords[index] = new GpuWriterRecord
                {
                    StartEnd = new Vector4(
                        candidate.StartXZ.x,
                        candidate.StartXZ.y,
                        candidate.EndXZ.x,
                        candidate.EndXZ.y),
                    Parameters = new Vector4(
                        interactor.TrailRadius,
                        interactor.TrailBendStrength,
                        interactor.TrailFlattenStrength,
                        candidate.MovementBlend),
                    DirectionParameters = new Vector4(
                        (float)interactor.DirectionMode,
                        interactor.WorldXBias,
                        interactor.WorldZStrength,
                        0f),
                    PersistenceParameters = new Vector4(
                        interactor.TrailMode ==
                            VegetationTrailMode.SessionPersistent ? 1f : 0f,
                        interactor.TrailRecoveryDelaySeconds,
                        interactor.TrailRecoveryDurationSeconds,
                        0f)
                };
            }

            if (lastUploadedWriterCount > 0)
            {
                writerBuffer.SetData(
                    uploadRecords,
                    0,
                    0,
                    lastUploadedWriterCount);
            }
        }

        private void UploadAbilityStamps()
        {
            if (pendingAbilityStamps.Count == 0)
            {
                lastUploadedAbilityStampCount = 0;
                PrimeStampBufferWithZeroRecord();
                return;
            }

            pendingAbilityStamps.Sort(StampComparison);
            lastUploadedAbilityStampCount = Mathf.Min(
                maximumAbilityStampsPerStep,
                pendingAbilityStamps.Count);
            for (int index = 0; index < lastUploadedAbilityStampCount; index++)
            {
                VegetationTrampleStampRequest request =
                    pendingAbilityStamps[index].Request;
                stampUploadRecords[index] = new GpuStampRecord
                {
                    OriginEnd = new Vector4(
                        request.Origin.x,
                        request.Origin.z,
                        request.End.x,
                        request.End.z),
                    ShapeParameters = new Vector4(
                        (float)request.Shape,
                        request.Radius,
                        request.ArcDegrees * Mathf.Deg2Rad,
                        request.LineWidth),
                    DirectionParameters = new Vector4(
                        (float)request.DisplacementMode,
                        request.FacingDirectionXZ.x,
                        request.FacingDirectionXZ.y,
                        request.EdgeIrregularity),
                    EffectParameters = new Vector4(
                        request.BendStrength,
                        request.FlattenStrength,
                        request.IrregularityScale,
                        request.RecoveryMode ==
                            VegetationTrampleStampRecoveryMode.SessionPersistent
                                ? 1f
                                : 0f),
                    RecoveryParameters = new Vector4(
                        request.RecoveryDelaySeconds,
                        request.RecoveryDurationSeconds,
                        UIntBitsToFloat(request.Seed),
                        0f),
                    FixedDirectionParameters = new Vector4(
                        request.FixedDisplacementDirectionXZ.x,
                        request.FixedDisplacementDirectionXZ.y,
                        0f,
                        0f)
                };
            }

            stampBuffer.SetData(
                stampUploadRecords,
                0,
                0,
                lastUploadedAbilityStampCount);
            pendingAbilityStamps.RemoveRange(
                0,
                lastUploadedAbilityStampCount);
        }

        private void PrimeStampBufferWithZeroRecord()
        {
            if (stampBuffer == null || !stampBuffer.IsValid() ||
                stampUploadRecords == null || stampUploadRecords.Length == 0)
            {
                return;
            }

            stampUploadRecords[0] = default;
            stampBuffer.SetData(stampUploadRecords, 0, 0, 1);
        }

        private bool TryCaptureTrailSegment(
            VegetationInteractor interactor,
            float deltaTime,
            out Vector2 startXZ,
            out Vector2 endXZ,
            out float movementBlend)
        {
            Vector3 current = interactor.transform.position;
            if (!histories.TryGetValue(interactor, out TrailHistory history) ||
                !history.Initialized)
            {
                history = new TrailHistory
                {
                    PreviousProbePosition = current,
                    LastStampPosition = current,
                    Initialized = true
                };
                histories[interactor] = history;
                startXZ = endXZ = new Vector2(current.x, current.z);
                movementBlend = 0f;
                return false;
            }

            Vector2 previousProbeXZ = new Vector2(
                history.PreviousProbePosition.x,
                history.PreviousProbePosition.z);
            Vector2 currentXZ = new Vector2(current.x, current.z);
            float probeDistance = Vector2.Distance(previousProbeXZ, currentXZ);
            float speed = deltaTime > 0.000001f
                ? probeDistance / deltaTime
                : 0f;
            history.PreviousProbePosition = current;

            if (probeDistance > interactor.MaximumSweepDistance ||
                speed < interactor.MinimumTrailSpeed)
            {
                history.LastStampPosition = current;
                histories[interactor] = history;
                startXZ = endXZ = currentXZ;
                movementBlend = 0f;
                return false;
            }

            startXZ = new Vector2(
                history.LastStampPosition.x,
                history.LastStampPosition.z);
            endXZ = currentXZ;
            if (Vector2.Distance(startXZ, endXZ) < interactor.TrailStampSpacing)
            {
                histories[interactor] = history;
                movementBlend = 0f;
                return false;
            }

            history.LastStampPosition = current;
            histories[interactor] = history;
            movementBlend = interactor.MovementDirectionInfluence *
                Mathf.Clamp01(
                    speed / Mathf.Max(0.05f, interactor.FullMovementResponseSpeed));
            return true;
        }

        private bool SweptCircleIntersectsGround(
            Vector2 startXZ,
            Vector2 endXZ,
            float worldRadius)
        {
            Matrix4x4 worldToLocal = surfaceGround.transform.worldToLocalMatrix;
            Vector3 startLocal = worldToLocal.MultiplyPoint3x4(
                new Vector3(startXZ.x, surfaceGround.transform.position.y, startXZ.y));
            Vector3 endLocal = worldToLocal.MultiplyPoint3x4(
                new Vector3(endXZ.x, surfaceGround.transform.position.y, endXZ.y));
            float xScale = surfaceGround.transform.localToWorldMatrix
                .MultiplyVector(Vector3.right).magnitude;
            float zScale = surfaceGround.transform.localToWorldMatrix
                .MultiplyVector(Vector3.forward).magnitude;
            float localRadius = worldRadius / Mathf.Max(
                0.0001f,
                Mathf.Min(xScale, zScale));
            float minimumX = Mathf.Min(startLocal.x, endLocal.x) - localRadius;
            float maximumX = Mathf.Max(startLocal.x, endLocal.x) + localRadius;
            float minimumZ = Mathf.Min(startLocal.z, endLocal.z) - localRadius;
            float maximumZ = Mathf.Max(startLocal.z, endLocal.z) + localRadius;
            return maximumX >= -resolvedHalfSize &&
                   minimumX <= resolvedHalfSize &&
                   maximumZ >= -resolvedHalfSize &&
                   minimumZ <= resolvedHalfSize;
        }

        private void PruneStaleHistories()
        {
            staleHistoryKeys.Clear();
            foreach (KeyValuePair<VegetationInteractor, TrailHistory> pair in histories)
            {
                VegetationInteractor interactor = pair.Key;
                if (interactor == null || !interactor.isActiveAndEnabled ||
                    interactor.TrailMode == VegetationTrailMode.Off)
                {
                    staleHistoryKeys.Add(interactor);
                }
            }
            for (int index = 0; index < staleHistoryKeys.Count; index++)
            {
                histories.Remove(staleHistoryKeys[index]);
            }
        }

        private void DispatchSimulation(float deltaTime)
        {
            SetCommonComputeParameters(deltaTime);
            computeShader.SetInt("_WriterCount", lastUploadedWriterCount);
            computeShader.SetBuffer(simulateKernel, "_Writers", writerBuffer);
            computeShader.SetInt(
                "_StampCount",
                lastUploadedAbilityStampCount);
            computeShader.SetBuffer(simulateKernel, "_Stamps", stampBuffer);
            computeShader.SetTexture(simulateKernel, "_StateRead", currentState);
            computeShader.SetTexture(simulateKernel, "_StateWrite", previousState);
            computeShader.SetTexture(simulateKernel, "_TimingState", timingState);
            Dispatch(simulateKernel);

            RenderTexture oldCurrent = currentState;
            currentState = previousState;
            previousState = oldCurrent;
            totalSimulationDispatchCount++;
        }

        private void SetCommonComputeParameters(float deltaTime)
        {
            computeShader.SetInts(
                "_FieldResolution",
                fieldResolution,
                fieldResolution);
            computeShader.SetVector(
                "_GroundDomain",
                new Vector4(
                    resolvedHalfSize,
                    resolvedDomainSize,
                    0f,
                    0f));
            computeShader.SetMatrix(
                "_GroundLocalToWorld",
                surfaceGround != null
                    ? surfaceGround.transform.localToWorldMatrix
                    : Matrix4x4.identity);
            computeShader.SetFloat("_DeltaTime", deltaTime);
        }

        private void Dispatch(int kernel)
        {
            int groupCount = Mathf.CeilToInt(
                fieldResolution / (float)ThreadGroupSize);
            computeShader.Dispatch(kernel, groupCount, groupCount, 1);
            lastFrameDispatchCount++;
        }

        private int ComputeResourceConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + fieldResolution;
                hash = hash * 31 + maximumTrailWriters;
                hash = hash * 31 + maximumAbilityStampsPerStep;
                return hash;
            }
        }

        private static float UIntBitsToFloat(uint value)
        {
            return new FloatUIntBits { UIntValue = value }.FloatValue;
        }

        private static int CompareQueuedStamps(QueuedStamp left, QueuedStamp right)
        {
            int priorityComparison =
                right.Request.Priority.CompareTo(left.Request.Priority);
            if (priorityComparison != 0)
            {
                return priorityComparison;
            }
            return left.Sequence.CompareTo(right.Sequence);
        }

        private static int CompareCandidates(
            WriterCandidate left,
            WriterCandidate right)
        {
            int priorityComparison =
                right.Source.TrailPriority.CompareTo(left.Source.TrailPriority);
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

            return left.Source.GetEntityId().GetHashCode().CompareTo(
                right.Source.GetEntityId().GetHashCode());
        }

        private void ReleaseResources()
        {
            resourcesReady = false;
            ReleaseTexture(ref stateA);
            ReleaseTexture(ref stateB);
            ReleaseTexture(ref timingState);
            currentState = null;
            previousState = null;
            writerBuffer?.Release();
            writerBuffer = null;
            uploadRecords = null;
            stampBuffer?.Release();
            stampBuffer = null;
            stampUploadRecords = null;
            lastUploadedAbilityStampCount = 0;
            computeShader = null;
            initializeKernel = -1;
            simulateKernel = -1;
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

        private void OnDrawGizmosSelected()
        {
            if (!showFieldBounds)
            {
                return;
            }

            ResolveOwnership();
            if (surfaceGround == null ||
                !surfaceGround.TryGetSurfaceDomain(
                    out float halfSize,
                    out float domainSize))
            {
                return;
            }

            Matrix4x4 matrix = surfaceGround.transform.localToWorldMatrix;
            Color previousColor = Gizmos.color;
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = matrix;
            Gizmos.color = new Color(1f, 0.55f, 0.1f, 0.9f);
            Gizmos.DrawWireCube(
                Vector3.zero,
                new Vector3(domainSize, 0.08f, domainSize));
            Gizmos.DrawWireSphere(Vector3.zero, Mathf.Min(halfSize, 0.2f));
            Gizmos.matrix = previousMatrix;
            Gizmos.color = previousColor;
        }
    }
}
