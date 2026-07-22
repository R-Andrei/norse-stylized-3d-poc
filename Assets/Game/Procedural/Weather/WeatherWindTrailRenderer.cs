using System;
using System.Runtime.InteropServices;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Weather
{
    public enum WeatherWindTrailCandidateStatus : byte
    {
        NotEvaluated = 0,
        OutsideViewport = 1,
        BelowWindFloor = 2,
        TooClose = 3,
        Eligible = 4,
        Selected = 5
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather Wind Trail Renderer")]
    public sealed class WeatherWindTrailRenderer : MonoBehaviour
    {
        private const int MaximumTopCandidateCapacity = 8;
        private const int MaximumSpawnSweepsPerFrame = 2;
        private const int CurrentSerializedBaselineVersion = 2;
        private const float MinimumDirectionMagnitudeSquared = 0.0000001f;
        private const float MinimumElapsedSeconds = 0.000001f;

        private const string RuntimeResourcesError =
            "Wind-trail runtime resources are not ready.";
        private const string MissingDomainError =
            "A co-located WeatherWindDomain is required.";
        private const string DomainNotPublishedError =
            "The co-located WeatherWindDomain is not the published Weather domain.";
        private const string DomainResourcesError =
            "The published WeatherWindDomain resources are not ready.";
        private const string MissingCameraError =
            "The published WeatherWindDomain has no resolved target camera.";
        private const string MissingShaderError =
            "A serialized wind-trail shader is required.";
        private const string UnsupportedShaderError =
            "The serialized wind-trail shader is not supported on this platform.";

        private static readonly ProfilerMarker UpdateProfilerMarker =
            new ProfilerMarker("WeatherWindTrails.Update");
        private static readonly ProfilerMarker CandidateSelectionProfilerMarker =
            new ProfilerMarker("WeatherWindTrails.CandidateSelection");
        private static readonly ProfilerMarker PathIntegrationProfilerMarker =
            new ProfilerMarker("WeatherWindTrails.PathIntegration");
        private static readonly ProfilerMarker MeshUploadProfilerMarker =
            new ProfilerMarker("WeatherWindTrails.MeshUpload");
        private static readonly ProfilerMarker RenderSubmissionProfilerMarker =
            new ProfilerMarker("WeatherWindTrails.RenderSubmission");

        private static readonly int TrailColorId =
            Shader.PropertyToID("_TrailColor");
        private static readonly int TrailPresentationTimeId =
            Shader.PropertyToID("_TrailPresentationTime");
        private static readonly int UniformBodyOpacityId =
            Shader.PropertyToID("_UniformBodyOpacity");
        private static readonly int EdgeSoftnessId =
            Shader.PropertyToID("_EdgeSoftness");
        private static readonly int StrengthOpacityInfluenceId =
            Shader.PropertyToID("_StrengthOpacityInfluence");
        private static readonly int VariationOpacityInfluenceId =
            Shader.PropertyToID("_VariationOpacityInfluence");

        [Header("Rendering")]
        [SerializeField]
        [Tooltip("Serialized shader reference used for build retention and the hidden runtime material.")]
        private Shader trailShader;

        [SerializeField, ColorUsage(true, false)]
        private Color trailColor = Color.white;

        [SerializeField]
        [Tooltip("Keep alpha spatially uniform across the visible trail body. Head and tail shaping use physical width taper instead of broad alpha gradients.")]
        private bool uniformBodyOpacity = true;

        [SerializeField, Range(0.01f, 1f)]
        [Tooltip("Cross-width alpha softness used only when Uniform Body Opacity is disabled.")]
        private float edgeSoftness = 0.15f;

        [SerializeField, Range(0f, 1f)]
        private float strengthOpacityInfluence = 0f;

        [SerializeField, Range(0f, 1f)]
        private float variationOpacityInfluence = 0f;

        [Header("Capacity and Spawn Cadence")]
        [SerializeField, Range(1, 16)]
        private int maximumActiveTrails = 3;

        [SerializeField, Range(0.25f, 8f)]
        private float spawnAttemptsPerSecond = 1f;

        [SerializeField, Range(4, 16)]
        private int candidateGridResolution = 8;

        [SerializeField, Range(1, MaximumTopCandidateCapacity)]
        private int strongestCandidateSubset = 6;

        [SerializeField, Range(0f, 0.9f)]
        private float candidateCellJitter = 0.7f;

        [SerializeField]
        private int trailSeed = 6247;

        [Header("Strong-Wind Placement")]
        [SerializeField, Min(0f)]
        private float minimumWindStrength = 0.18f;

        [SerializeField, Range(0.25f, 6f)]
        private float strengthScoreExponent = 2f;

        [SerializeField, Range(0.25f, 4f)]
        private float spacingScoreExponent = 1f;

        [SerializeField, Min(0.25f)]
        private float minimumTrailSeparationMetres = 8f;

        [SerializeField, Min(0f)]
        private float separationCooldownSeconds = 3f;

        [Header("Streamline Construction")]
        [SerializeField, Range(8, 96)]
        private int maximumCentrelinePoints = 80;

        [SerializeField, Range(0.1f, 2f)]
        private float integrationStepMetres = 0.5f;

        [SerializeField, Min(0f)]
        private float minimumPathWindStrength = 0.12f;

        [SerializeField, Min(0.25f)]
        private float minimumCompletedPathLengthMetres = 4f;

        [SerializeField, Range(5f, 120f)]
        private float maximumTurnDegreesPerSegment = 55f;

        [SerializeField, Range(0.05f, 2f)]
        private float selfApproachDistanceMetres = 0.3f;

        [SerializeField, Range(-1f, 1f)]
        private float minimumSegmentWindAlignment = 0.35f;

        [Header("Presentation Data")]
        [FormerlySerializedAs("minimumLifetimeSeconds")]
        [SerializeField, Min(0.1f)]
        private float minimumAliveDurationSeconds = 7f;

        [FormerlySerializedAs("maximumLifetimeSeconds")]
        [SerializeField, Min(0.1f)]
        private float maximumAliveDurationSeconds = 11f;

        [SerializeField, Min(0.005f)]
        private float minimumWidthMetres = 0.04f;

        [SerializeField, Min(0.005f)]
        private float maximumWidthMetres = 0.1f;

        [SerializeField, Min(0.05f)]
        private float minimumPresentationSpeed = 1f;

        [SerializeField, Min(0.05f)]
        private float maximumPresentationSpeed = 1.5f;

        [FormerlySerializedAs("minimumVisibleTailLengthMetres")]
        [SerializeField, Min(0.1f)]
        private float minimumVisibleBodyLengthMetres = 5.5f;

        [FormerlySerializedAs("maximumVisibleTailLengthMetres")]
        [SerializeField, Min(0.1f)]
        private float maximumVisibleBodyLengthMetres = 8.5f;

        [SerializeField, Min(0.05f)]
        [Tooltip("Maximum extra endpoint speed used only while growing or shrinking a trail. Per-trail allowance is clamped below its normal travel speed.")]
        private float lifecycleTipSpeedAllowance = 0.75f;

        [SerializeField, Min(0.05f)]
        [Tooltip("Physical distance over which each visible endpoint tapers to a point.")]
        private float pointedEndLengthMetres = 0.75f;

        [SerializeField, Min(0f)]
        private float minimumAltitudeMetres = 1f;

        [SerializeField, Min(0f)]
        private float maximumAltitudeMetres = 2.5f;

        [SerializeField, Min(0f)]
        private float maximumVerticalDeviationMetres = 0.15f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("Probability that an accepted streamline receives one bounded broad lateral wave.")]
        private float occasionalBroadWaveChance = 0.22f;

        [SerializeField, Range(0f, 1.5f)]
        [Tooltip("Maximum lateral XZ displacement for trails selected to receive an occasional broad wave.")]
        private float occasionalBroadWaveStrengthMetres = 0.45f;

        [SerializeField, Range(0f, 1f)]
        private float trailOpacity = 0.95f;

        [SerializeField, HideInInspector]
        private int serializedBaselineVersion;

        [Header("Camera Relevance")]
        [SerializeField, Range(0f, 0.5f)]
        private float candidateViewportMargin = 0.12f;


        private WeatherWindDomain weatherDomain;
        private Camera resolvedCamera;
        private Mesh trailMesh;
        private Material runtimeMaterial;
        private bool resourcesDirty = true;
        private bool resourcesReady;
        private string lastError = string.Empty;
        private int lastConfigurationHash;
        private bool configurationHashInitialized;
        private bool dependencyResolutionAttempted;
        private double lastRealtime;
        private float presentationTime;
        private float spawnAccumulator;
        private int spawnEpoch;
        private int nextSlotSearchIndex;
        private int lastDomainConfigurationHash;
        private float lastDomainSimulationTime;
        private Vector2 lastFieldOriginXZ;
        private bool domainRuntimeStateInitialized;

        private bool[] trailActive;
        private Vector2[] trailSeedsXZ;
        private float[] trailBirthTimes;
        private float[] trailTotalLifetimes;
        private int[] trailPointCounts;
        private float[] trailLengths;
        private float[] trailStrengths;
        private float[] trailMinimumAlignments;
        private bool[] trailUsesBroadWave;
        private Vector3[] trailPoints;
        private float[] trailPointDistances;

        private bool[] cooldownActive;
        private Vector2[] cooldownSeedsXZ;
        private float[] cooldownExpiryTimes;

        private Vector3[] candidateWorldPositions;
        private float[] candidateStrengths;
        private float[] candidateScores;
        private float[] candidateNearestDistances;
        private WeatherWindTrailCandidateStatus[] candidateStatuses;
        private int[] topCandidateIndices;
        private float[] topCandidateScores;

        private Vector2[] forwardScratch;
        private Vector2[] combinedPathScratch;
        private Vector2[] undeformedPathScratch;
        private Vector3[] worldPathScratch;
        private float[] pathDistanceScratch;

        private TrailVertex[] meshVertices;
        private ushort[] meshIndices;

        private int activeTrailCount;
        private int cooldownCount;
        private long totalSpawnAttemptCount;
        private long totalSuccessfulSpawnCount;
        private long totalCandidateEvaluationCount;
        private long totalViewportRejectionCount;
        private long totalCalmRejectionCount;
        private long totalSeparationRejectionCount;
        private long totalNoEligibleCandidateCount;
        private long totalPathRejectionCount;
        private long totalTargetWindSampleCount;
        private int totalDomainConfigurationResetCount;
        private int totalSimulationRewindResetCount;
        private int totalLargeTeleportResetCount;
        private int lastCandidateCount;
        private int lastVisibleCandidateCount;
        private int lastEligibleCandidateCount;
        private float lastSampledCandidateMinimumStrength = -1f;
        private float lastSampledCandidateMaximumStrength = -1f;
        private float lastAcceptedCandidateStrength = -1f;
        private float lastAcceptedNearestSeparation = -1f;
        private int lastGeneratedPathPointCount;
        private float lastGeneratedPathLengthMetres;
        private float lastGeneratedPathMinimumAlignment = -1f;
        private int lastAttemptTargetWindSampleCount;
        private int currentAttemptTargetWindSampleCount;
        private int lastMeshUploadVertexCount;
        private long totalRenderSubmissionCount;
        private int lastRenderedTrailCount;
        private long totalBroadWaveTrailCount;
        private bool lastGeneratedPathUsedBroadWave;
        private float lastResolvedBodyLengthMetres = -1f;
        private float lastResolvedTravelSpeed = -1f;
        private float lastResolvedTipSpeedAllowance = -1f;
        private float lastResolvedSpawnDuration = -1f;
        private float lastResolvedAliveDuration = -1f;
        private float lastResolvedDespawnDuration = -1f;
        private float lastResolvedTotalLifetime = -1f;
        private float lastRequiredPathLengthMetres = -1f;

        private struct ResolvedTrailLifecycle
        {
            public float bodyLength;
            public float travelSpeed;
            public float tipSpeedAllowance;
            public float aliveDuration;
            public float spawnDuration;
            public float despawnDuration;
            public float totalLifetime;
            public float requiredPathLength;
            public float pointedEndLength;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TrailVertex
        {
            public Vector3 position;
            public Vector3 tangent;
            public Color32 presentation;
            public Vector2 signedHalfWidthAndDistance;
            public Vector4 lifecycleMotion;
            public Vector4 lifecycleTiming;
        }

        public WeatherWindDomain WeatherDomain => weatherDomain;
        public Camera TargetCamera => resolvedCamera;
        public Shader TrailShader => trailShader;
        public Mesh TrailMesh => trailMesh;
        public Material RuntimeMaterial => runtimeMaterial;
        public bool ResourcesReady => resourcesReady;
        public bool RenderingReady => runtimeMaterial != null &&
            trailShader != null && trailShader.isSupported;
        public bool RuntimeReady => Application.isPlaying && CanRun(false);
        public string LastError => lastError;
        public int MaximumActiveTrails => maximumActiveTrails;
        public int MaximumCentrelinePoints => maximumCentrelinePoints;
        public int CandidateCapacity => candidateGridResolution * candidateGridResolution;
        public int ActiveTrailCount => activeTrailCount;
        public int CooldownCount => cooldownCount;
        public int LastCandidateCount => lastCandidateCount;
        public int LastVisibleCandidateCount => lastVisibleCandidateCount;
        public int LastEligibleCandidateCount => lastEligibleCandidateCount;
        public int MeshVertexCapacity => meshVertices != null ? meshVertices.Length : 0;
        public int MeshIndexCapacity => meshIndices != null ? meshIndices.Length : 0;
        public int ConfigurationHash => ComputeConfigurationHash();
        public float PresentationTime => presentationTime;
        public int SerializedBaselineVersion => serializedBaselineVersion;
        public static int CurrentBaselineVersion => CurrentSerializedBaselineVersion;

        private void OnEnable()
        {
            UpgradeSerializedBaselineIfNeeded();
            dependencyResolutionAttempted = false;
            ResolveDependencies();
            lastConfigurationHash = ComputeConfigurationHash();
            configurationHashInitialized = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            presentationTime = 0f;
            spawnAccumulator = 0f;
            spawnEpoch = 0;
            nextSlotSearchIndex = 0;
            domainRuntimeStateInitialized = false;
            ResetDiagnosticCounters();
            resourcesDirty = true;

            if (Application.isPlaying)
            {
                EnsureResources();
            }
            else
            {
                resourcesReady = false;
                lastError = string.Empty;
            }
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }

        private void OnValidate()
        {
            UpgradeSerializedBaselineIfNeeded();
            maximumActiveTrails = Mathf.Clamp(maximumActiveTrails, 1, 16);
            spawnAttemptsPerSecond = Mathf.Clamp(spawnAttemptsPerSecond, 0.25f, 8f);
            candidateGridResolution = Mathf.Clamp(candidateGridResolution, 4, 16);
            strongestCandidateSubset = Mathf.Clamp(
                strongestCandidateSubset,
                1,
                MaximumTopCandidateCapacity);
            candidateCellJitter = Mathf.Clamp(candidateCellJitter, 0f, 0.9f);
            minimumWindStrength = Mathf.Max(0f, minimumWindStrength);
            strengthScoreExponent = Mathf.Clamp(strengthScoreExponent, 0.25f, 6f);
            spacingScoreExponent = Mathf.Clamp(spacingScoreExponent, 0.25f, 4f);
            minimumTrailSeparationMetres = Mathf.Max(
                0.25f,
                minimumTrailSeparationMetres);
            separationCooldownSeconds = Mathf.Max(0f, separationCooldownSeconds);
            maximumCentrelinePoints = Mathf.Clamp(maximumCentrelinePoints, 8, 96);
            integrationStepMetres = Mathf.Clamp(integrationStepMetres, 0.1f, 2f);
            minimumPathWindStrength = Mathf.Max(0f, minimumPathWindStrength);
            minimumCompletedPathLengthMetres = Mathf.Max(
                0.25f,
                minimumCompletedPathLengthMetres);
            maximumTurnDegreesPerSegment = Mathf.Clamp(
                maximumTurnDegreesPerSegment,
                5f,
                120f);
            selfApproachDistanceMetres = Mathf.Clamp(
                selfApproachDistanceMetres,
                0.05f,
                2f);
            minimumSegmentWindAlignment = Mathf.Clamp(
                minimumSegmentWindAlignment,
                -1f,
                1f);
            minimumAliveDurationSeconds = Mathf.Max(
                0.1f,
                minimumAliveDurationSeconds);
            maximumAliveDurationSeconds = Mathf.Max(
                minimumAliveDurationSeconds,
                maximumAliveDurationSeconds);
            minimumWidthMetres = Mathf.Max(0.005f, minimumWidthMetres);
            maximumWidthMetres = Mathf.Max(minimumWidthMetres, maximumWidthMetres);
            minimumPresentationSpeed = Mathf.Max(0.05f, minimumPresentationSpeed);
            maximumPresentationSpeed = Mathf.Max(
                minimumPresentationSpeed,
                maximumPresentationSpeed);
            minimumVisibleBodyLengthMetres = Mathf.Max(
                0.1f,
                minimumVisibleBodyLengthMetres);
            maximumVisibleBodyLengthMetres = Mathf.Max(
                minimumVisibleBodyLengthMetres,
                maximumVisibleBodyLengthMetres);
            lifecycleTipSpeedAllowance = Mathf.Max(
                0.05f,
                lifecycleTipSpeedAllowance);
            pointedEndLengthMetres = Mathf.Max(0.05f, pointedEndLengthMetres);
            minimumAltitudeMetres = Mathf.Max(0f, minimumAltitudeMetres);
            maximumAltitudeMetres = Mathf.Max(
                minimumAltitudeMetres,
                maximumAltitudeMetres);
            maximumVerticalDeviationMetres = Mathf.Max(
                0f,
                maximumVerticalDeviationMetres);
            occasionalBroadWaveChance = Mathf.Clamp01(
                occasionalBroadWaveChance);
            occasionalBroadWaveStrengthMetres = Mathf.Clamp(
                occasionalBroadWaveStrengthMetres,
                0f,
                1.5f);
            trailOpacity = Mathf.Clamp01(trailOpacity);
            trailColor.r = Mathf.Clamp01(trailColor.r);
            trailColor.g = Mathf.Clamp01(trailColor.g);
            trailColor.b = Mathf.Clamp01(trailColor.b);
            trailColor.a = Mathf.Clamp01(trailColor.a);
            edgeSoftness = Mathf.Clamp(edgeSoftness, 0.01f, 1f);
            strengthOpacityInfluence = Mathf.Clamp01(strengthOpacityInfluence);
            variationOpacityInfluence = Mathf.Clamp01(variationOpacityInfluence);
            candidateViewportMargin = Mathf.Clamp(candidateViewportMargin, 0f, 0.5f);

            int configurationHash = ComputeConfigurationHash();
            if (!configurationHashInitialized)
            {
                lastConfigurationHash = configurationHash;
                configurationHashInitialized = true;
            }
            else if (lastConfigurationHash != configurationHash)
            {
                lastConfigurationHash = configurationHash;
                resourcesDirty = true;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            using var profilerScope = UpdateProfilerMarker.Auto();

            RefreshConfigurationState();
            ResolveDependencies();

            double now = Time.realtimeSinceStartupAsDouble;
            float elapsed = (float)Math.Max(0.0, Math.Min(0.25, now - lastRealtime));
            lastRealtime = now;
            presentationTime += elapsed;

            if (!EnsureResources())
            {
                return;
            }

            ExpireTrails();
            PruneCooldowns();

            if (!CanRun(true))
            {
                if (activeTrailCount > 0)
                {
                    ClearAllTrails(false);
                }

                if (cooldownCount > 0)
                {
                    ClearCooldowns();
                }

                domainRuntimeStateInitialized = false;
                spawnAccumulator = 0f;
                return;
            }

            RefreshDomainRuntimeState();
            UpdateMeshBounds();
            spawnAccumulator += elapsed;
            float spawnInterval = 1f / Mathf.Max(0.25f, spawnAttemptsPerSecond);
            int sweepCount = 0;

            while (spawnAccumulator >= spawnInterval &&
                   sweepCount < MaximumSpawnSweepsPerFrame &&
                   FindFreeTrailSlot() >= 0)
            {
                spawnAccumulator -= spawnInterval;
                TrySpawnTrail();
                sweepCount++;
            }

            if (sweepCount == MaximumSpawnSweepsPerFrame &&
                spawnAccumulator > spawnInterval)
            {
                spawnAccumulator = spawnInterval;
            }
            else if (FindFreeTrailSlot() < 0)
            {
                spawnAccumulator = Mathf.Min(spawnAccumulator, spawnInterval);
            }

            SubmitTrailRender();
        }

        public void ResetTrailSimulation()
        {
            if (!Application.isPlaying || !EnsureResources())
            {
                return;
            }

            ClearAllTrails(false);
            ClearCooldowns();
            spawnAccumulator = 0f;
            spawnEpoch = 0;
            presentationTime = 0f;
            nextSlotSearchIndex = 0;
            domainRuntimeStateInitialized = false;
            ResetDiagnosticCounters();
            UpdateMeshBounds();
        }

        public bool TryGetLastCandidate(
            int index,
            out Vector3 worldPosition,
            out float strength,
            out float score,
            out float nearestDistance,
            out WeatherWindTrailCandidateStatus status)
        {
            if (candidateWorldPositions == null ||
                index < 0 ||
                index >= lastCandidateCount)
            {
                worldPosition = Vector3.zero;
                strength = 0f;
                score = 0f;
                nearestDistance = -1f;
                status = WeatherWindTrailCandidateStatus.NotEvaluated;
                return false;
            }

            worldPosition = candidateWorldPositions[index];
            strength = candidateStrengths[index];
            score = candidateScores[index];
            nearestDistance = candidateNearestDistances[index];
            status = candidateStatuses[index];
            return true;
        }

        public bool TryGetTrailPoint(
            int trailIndex,
            int pointIndex,
            out Vector3 worldPosition)
        {
            if (trailActive == null ||
                trailIndex < 0 || trailIndex >= trailActive.Length ||
                !trailActive[trailIndex] ||
                pointIndex < 0 || pointIndex >= trailPointCounts[trailIndex])
            {
                worldPosition = Vector3.zero;
                return false;
            }

            worldPosition = trailPoints[TrailPointIndex(trailIndex, pointIndex)];
            return true;
        }

        public int GetTrailPointCount(int trailIndex)
        {
            if (trailActive == null ||
                trailIndex < 0 || trailIndex >= trailActive.Length ||
                !trailActive[trailIndex])
            {
                return 0;
            }

            return trailPointCounts[trailIndex];
        }

        public void GetResolvedLifecycleDurationRanges(
            out Vector2 spawnDurationRange,
            out Vector2 despawnDurationRange,
            out Vector2 totalLifetimeRange)
        {
            float minimumSpawn = float.PositiveInfinity;
            float maximumSpawn = 0f;
            float minimumDespawn = float.PositiveInfinity;
            float maximumDespawn = 0f;
            float minimumTotal = float.PositiveInfinity;
            float maximumTotal = 0f;

            for (int lengthIndex = 0; lengthIndex < 2; lengthIndex++)
            {
                float bodyLength = lengthIndex == 0
                    ? minimumVisibleBodyLengthMetres
                    : maximumVisibleBodyLengthMetres;
                for (int speedIndex = 0; speedIndex < 2; speedIndex++)
                {
                    float speed = speedIndex == 0
                        ? minimumPresentationSpeed
                        : maximumPresentationSpeed;
                    float allowance = ResolveTipSpeedAllowance(speed);
                    float spawnDuration = bodyLength /
                        Mathf.Max(MinimumElapsedSeconds, speed + allowance);
                    float despawnDuration = bodyLength /
                        Mathf.Max(MinimumElapsedSeconds, allowance * 2f);

                    minimumSpawn = Mathf.Min(minimumSpawn, spawnDuration);
                    maximumSpawn = Mathf.Max(maximumSpawn, spawnDuration);
                    minimumDespawn = Mathf.Min(minimumDespawn, despawnDuration);
                    maximumDespawn = Mathf.Max(maximumDespawn, despawnDuration);

                    for (int aliveIndex = 0; aliveIndex < 2; aliveIndex++)
                    {
                        float aliveDuration = aliveIndex == 0
                            ? minimumAliveDurationSeconds
                            : maximumAliveDurationSeconds;
                        float total = spawnDuration + aliveDuration +
                            despawnDuration;
                        minimumTotal = Mathf.Min(minimumTotal, total);
                        maximumTotal = Mathf.Max(maximumTotal, total);
                    }
                }
            }

            spawnDurationRange = new Vector2(minimumSpawn, maximumSpawn);
            despawnDurationRange = new Vector2(
                minimumDespawn,
                maximumDespawn);
            totalLifetimeRange = new Vector2(minimumTotal, maximumTotal);
        }

        public string BuildComprehensiveReport()
        {
            bool playMode = Application.isPlaying;
            WeatherWindDomain reportDomain = weatherDomain != null
                ? weatherDomain
                : GetComponent<WeatherWindDomain>();
            Camera reportCamera = resolvedCamera != null
                ? resolvedCamera
                : reportDomain != null ? reportDomain.TargetCamera : null;
            GetResolvedLifecycleDurationRanges(
                out Vector2 spawnRange,
                out Vector2 despawnRange,
                out Vector2 totalRange);

            var builder = new StringBuilder(4608);
            builder.AppendLine("[Weather Wind Trails V0.6 Lifecycle Report]");
            builder.Append("Status: ")
                .AppendLine(!playMode
                    ? "EDITOR IDLE"
                    : RuntimeReady ? "READY" : "NOT READY");
            builder.Append("Component resources ready: ")
                .AppendLine(!playMode
                    ? "No (Play Mode only)"
                    : resourcesReady ? "Yes" : "No");
            builder.Append("Co-located Weather domain: ")
                .AppendLine(reportDomain != null ? reportDomain.name : "None");
            builder.Append("Published co-located domain: ")
                .AppendLine(reportDomain != null &&
                    WeatherWindDomain.PublishedDomain == reportDomain ? "Yes" : "No");
            builder.Append("Weather resources ready: ")
                .AppendLine(reportDomain != null && reportDomain.ResourcesReady ? "Yes" : "No");
            builder.Append("Resolved camera: ")
                .AppendLine(reportCamera != null ? reportCamera.name : "None");
            builder.Append("Serialized trail shader: ")
                .AppendLine(trailShader != null ? trailShader.name : "None");
            builder.Append("Shader supported: ")
                .AppendLine(trailShader != null && trailShader.isSupported ? "Yes" : "No");
            builder.Append("Runtime material ready: ")
                .AppendLine(runtimeMaterial != null ? "Yes" : "No");
            builder.Append("Rendering mode: ")
                .AppendLine("Play Mode target camera only");
            builder.Append("Active / maximum trails: ")
                .Append(activeTrailCount).Append(" / ")
                .AppendLine(maximumActiveTrails.ToString());
            builder.Append("Active broad-wave trails: ")
                .AppendLine(CountActiveBroadWaveTrails().ToString());
            builder.Append("Cooldown positions: ")
                .AppendLine(cooldownCount.ToString());
            builder.Append("Candidate lattice: ")
                .Append(candidateGridResolution).Append(" × ")
                .Append(candidateGridResolution).Append(" = ")
                .AppendLine(CandidateCapacity.ToString());
            builder.Append("Spawn attempts per second: ")
                .AppendLine(spawnAttemptsPerSecond.ToString("0.###"));
            builder.Append("Strongest weighted subset: ")
                .AppendLine(strongestCandidateSubset.ToString());
            builder.Append("Minimum wind / path wind: ")
                .Append(minimumWindStrength.ToString("0.###")).Append(" / ")
                .AppendLine(minimumPathWindStrength.ToString("0.###"));
            builder.Append("Minimum separation / cooldown: ")
                .Append(minimumTrailSeparationMetres.ToString("0.###")).Append(" m / ")
                .Append(separationCooldownSeconds.ToString("0.###")).AppendLine(" s");
            builder.Append("Centreline point capacity: ")
                .AppendLine(maximumCentrelinePoints.ToString());
            builder.Append("Integration step / minimum path length: ")
                .Append(integrationStepMetres.ToString("0.###")).Append(" m / ")
                .Append(minimumCompletedPathLengthMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Alive duration: ")
                .Append(minimumAliveDurationSeconds.ToString("0.###")).Append("–")
                .Append(maximumAliveDurationSeconds.ToString("0.###")).AppendLine(" s");
            builder.Append("Travel speed: ")
                .Append(minimumPresentationSpeed.ToString("0.###")).Append("–")
                .Append(maximumPresentationSpeed.ToString("0.###")).AppendLine(" m/s");
            builder.Append("Visible body length: ")
                .Append(minimumVisibleBodyLengthMetres.ToString("0.###")).Append("–")
                .Append(maximumVisibleBodyLengthMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Lifecycle tip speed allowance / pointed end: ")
                .Append(lifecycleTipSpeedAllowance.ToString("0.###")).Append(" m/s / ")
                .Append(pointedEndLengthMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Resolved spawn duration interval: ")
                .Append(spawnRange.x.ToString("0.###")).Append("–")
                .Append(spawnRange.y.ToString("0.###")).AppendLine(" s");
            builder.Append("Resolved despawn duration interval: ")
                .Append(despawnRange.x.ToString("0.###")).Append("–")
                .Append(despawnRange.y.ToString("0.###")).AppendLine(" s");
            builder.Append("Resolved total lifetime interval: ")
                .Append(totalRange.x.ToString("0.###")).Append("–")
                .Append(totalRange.y.ToString("0.###")).AppendLine(" s");
            builder.Append("Uniform body opacity: ")
                .AppendLine(uniformBodyOpacity ? "Yes" : "No");
            builder.Append("Broad-wave chance / strength: ")
                .Append((occasionalBroadWaveChance * 100f).ToString("0.#")).Append("% / ")
                .Append(occasionalBroadWaveStrengthMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Mesh vertex / index capacity: ")
                .Append(MeshVertexCapacity).Append(" / ")
                .AppendLine(MeshIndexCapacity.ToString());
            builder.Append("Trail vertex stride: ")
                .Append(Marshal.SizeOf<TrailVertex>()).AppendLine(" bytes");
            builder.Append("Total spawn attempts / successes: ")
                .Append(totalSpawnAttemptCount.ToString("N0")).Append(" / ")
                .AppendLine(totalSuccessfulSpawnCount.ToString("N0"));
            builder.Append("Total broad-wave trails: ")
                .AppendLine(totalBroadWaveTrailCount.ToString("N0"));
            builder.Append("Total candidates evaluated: ")
                .AppendLine(totalCandidateEvaluationCount.ToString("N0"));
            builder.Append("Viewport / calm / separation rejections: ")
                .Append(totalViewportRejectionCount.ToString("N0")).Append(" / ")
                .Append(totalCalmRejectionCount.ToString("N0")).Append(" / ")
                .AppendLine(totalSeparationRejectionCount.ToString("N0"));
            builder.Append("No-eligible / path rejections: ")
                .Append(totalNoEligibleCandidateCount.ToString("N0")).Append(" / ")
                .AppendLine(totalPathRejectionCount.ToString("N0"));
            builder.Append("Total target-wind samples: ")
                .AppendLine(totalTargetWindSampleCount.ToString("N0"));
            builder.Append("Weather configuration / time-rewind / large-teleport resets: ")
                .Append(totalDomainConfigurationResetCount.ToString("N0")).Append(" / ")
                .Append(totalSimulationRewindResetCount.ToString("N0")).Append(" / ")
                .AppendLine(totalLargeTeleportResetCount.ToString("N0"));
            builder.Append("Tracked Weather configuration hash: ")
                .AppendLine(domainRuntimeStateInitialized
                    ? lastDomainConfigurationHash.ToString()
                    : "Not initialized");
            builder.Append("Tracked Weather simulation time: ")
                .AppendLine(domainRuntimeStateInitialized
                    ? lastDomainSimulationTime.ToString("0.###")
                    : "Not initialized");
            builder.Append("Tracked field origin XZ: ")
                .AppendLine(domainRuntimeStateInitialized
                    ? lastFieldOriginXZ.ToString("F3")
                    : "Not initialized");
            builder.Append("Last candidates visible / eligible: ")
                .Append(lastVisibleCandidateCount).Append(" / ")
                .AppendLine(lastEligibleCandidateCount.ToString());
            builder.Append("Last sampled candidate strength range: ")
                .Append(lastSampledCandidateMinimumStrength.ToString("0.###")).Append(" – ")
                .AppendLine(lastSampledCandidateMaximumStrength.ToString("0.###"));
            builder.Append("Last accepted strength / nearest separation: ")
                .Append(lastAcceptedCandidateStrength.ToString("0.###")).Append(" / ")
                .Append(lastAcceptedNearestSeparation.ToString("0.###")).AppendLine(" m");
            builder.Append("Last path points / length / minimum alignment: ")
                .Append(lastGeneratedPathPointCount).Append(" / ")
                .Append(lastGeneratedPathLengthMetres.ToString("0.###")).Append(" m / ")
                .AppendLine(lastGeneratedPathMinimumAlignment.ToString("0.###"));
            builder.Append("Last path used broad wave: ")
                .AppendLine(lastGeneratedPathUsedBroadWave ? "Yes" : "No");
            builder.Append("Last resolved body / travel / allowance: ")
                .Append(lastResolvedBodyLengthMetres.ToString("0.###")).Append(" m / ")
                .Append(lastResolvedTravelSpeed.ToString("0.###")).Append(" m/s / ")
                .Append(lastResolvedTipSpeedAllowance.ToString("0.###")).AppendLine(" m/s");
            builder.Append("Last resolved spawn / alive / despawn: ")
                .Append(lastResolvedSpawnDuration.ToString("0.###")).Append(" / ")
                .Append(lastResolvedAliveDuration.ToString("0.###")).Append(" / ")
                .Append(lastResolvedDespawnDuration.ToString("0.###")).AppendLine(" s");
            builder.Append("Last resolved total life / required path: ")
                .Append(lastResolvedTotalLifetime.ToString("0.###")).Append(" s / ")
                .Append(lastRequiredPathLengthMetres.ToString("0.###")).AppendLine(" m");
            builder.Append("Serialized baseline version: ")
                .Append(serializedBaselineVersion).Append(" / ")
                .AppendLine(CurrentSerializedBaselineVersion.ToString());
            builder.Append("Last attempt target-wind samples: ")
                .AppendLine(lastAttemptTargetWindSampleCount.ToString());
            builder.Append("Last mesh upload: ")
                .Append(lastMeshUploadVertexCount).AppendLine(" vertices");
            builder.Append("Total render submissions: ")
                .AppendLine(totalRenderSubmissionCount.ToString("N0"));
            builder.Append("Last submitted active trails: ")
                .AppendLine(lastRenderedTrailCount.ToString());
            builder.Append("Configuration hash: ")
                .AppendLine(ConfigurationHash.ToString());

            if (playMode && !string.IsNullOrEmpty(lastError))
            {
                builder.AppendLine("Error:");
                builder.AppendLine(lastError);
            }

            return builder.ToString();
        }

        public bool UpgradeSerializedBaselineIfNeeded()
        {
            if (serializedBaselineVersion >= CurrentSerializedBaselineVersion)
            {
                return false;
            }

            bool changed = false;
            if (serializedBaselineVersion < 1)
            {
                changed |= ReplaceIfEqual(ref maximumActiveTrails, 8, 3);
                changed |= ReplaceIfApproximately(ref spawnAttemptsPerSecond, 4f, 1f);
                changed |= ReplaceIfApproximately(
                    ref minimumTrailSeparationMetres,
                    6f,
                    8f);
                changed |= ReplaceIfApproximately(
                    ref separationCooldownSeconds,
                    1.5f,
                    3f);
                changed |= ReplaceIfEqual(ref maximumCentrelinePoints, 24, 32);
                changed |= ReplaceIfApproximately(
                    ref minimumCompletedPathLengthMetres,
                    3.5f,
                    4f);
                changed |= ReplaceIfApproximately(
                    ref minimumAliveDurationSeconds,
                    1.5f,
                    4f);
                changed |= ReplaceIfApproximately(
                    ref maximumAliveDurationSeconds,
                    3f,
                    7f);
                changed |= ReplaceIfApproximately(
                    ref minimumPresentationSpeed,
                    2f,
                    1.2f);
                changed |= ReplaceIfApproximately(
                    ref maximumPresentationSpeed,
                    5f,
                    2f);
                changed |= ReplaceIfApproximately(
                    ref minimumVisibleBodyLengthMetres,
                    2.5f,
                    5f);
                changed |= ReplaceIfApproximately(
                    ref maximumVisibleBodyLengthMetres,
                    5f,
                    8f);
                changed |= ReplaceIfApproximately(ref trailOpacity, 0.85f, 0.95f);
                changed |= ReplaceIfColorApproximately(
                    ref trailColor,
                    new Color(0.92f, 0.97f, 1f, 1f),
                    Color.white);
                changed |= ReplaceIfApproximately(ref edgeSoftness, 0.35f, 0.15f);
                changed |= ReplaceIfApproximately(
                    ref strengthOpacityInfluence,
                    0.25f,
                    0f);
                changed |= ReplaceIfApproximately(
                    ref variationOpacityInfluence,
                    0.12f,
                    0f);

                if (!uniformBodyOpacity)
                {
                    uniformBodyOpacity = true;
                    changed = true;
                }

                serializedBaselineVersion = 1;
                changed = true;
            }

            if (serializedBaselineVersion < 2)
            {
                changed |= ReplaceIfEqual(ref maximumCentrelinePoints, 32, 80);
                changed |= ReplaceIfApproximately(
                    ref minimumAliveDurationSeconds,
                    4f,
                    7f);
                changed |= ReplaceIfApproximately(
                    ref maximumAliveDurationSeconds,
                    7f,
                    11f);
                changed |= ReplaceIfApproximately(
                    ref minimumPresentationSpeed,
                    1.2f,
                    1f);
                changed |= ReplaceIfApproximately(
                    ref maximumPresentationSpeed,
                    2f,
                    1.5f);
                changed |= ReplaceIfApproximately(
                    ref minimumVisibleBodyLengthMetres,
                    5f,
                    5.5f);
                changed |= ReplaceIfApproximately(
                    ref maximumVisibleBodyLengthMetres,
                    8f,
                    8.5f);
                if (lifecycleTipSpeedAllowance <= 0.0001f)
                {
                    lifecycleTipSpeedAllowance = 0.75f;
                    changed = true;
                }

                if (pointedEndLengthMetres <= 0.0001f)
                {
                    pointedEndLengthMetres = 0.75f;
                    changed = true;
                }

                serializedBaselineVersion = 2;
                changed = true;
            }

            if (changed)
            {
                resourcesDirty = true;
            }

            return changed;
        }

        private int CountActiveBroadWaveTrails()
        {
            if (trailActive == null || trailUsesBroadWave == null)
            {
                return 0;
            }

            int count = 0;
            for (int index = 0; index < trailActive.Length; index++)
            {
                if (trailActive[index] && trailUsesBroadWave[index])
                {
                    count++;
                }
            }

            return count;
        }

        private static bool ReplaceIfEqual(
            ref int value,
            int oldValue,
            int newValue)
        {
            if (value != oldValue)
            {
                return false;
            }

            value = newValue;
            return true;
        }

        private static bool ReplaceIfApproximately(
            ref float value,
            float oldValue,
            float newValue)
        {
            if (!Mathf.Approximately(value, oldValue))
            {
                return false;
            }

            value = newValue;
            return true;
        }

        private static bool ReplaceIfColorApproximately(
            ref Color value,
            Color oldValue,
            Color newValue)
        {
            if (!Mathf.Approximately(value.r, oldValue.r) ||
                !Mathf.Approximately(value.g, oldValue.g) ||
                !Mathf.Approximately(value.b, oldValue.b) ||
                !Mathf.Approximately(value.a, oldValue.a))
            {
                return false;
            }

            value = newValue;
            return true;
        }

        private void RefreshConfigurationState()
        {
            int configurationHash = ComputeConfigurationHash();
            if (!configurationHashInitialized)
            {
                lastConfigurationHash = configurationHash;
                configurationHashInitialized = true;
                return;
            }

            if (lastConfigurationHash == configurationHash)
            {
                return;
            }

            lastConfigurationHash = configurationHash;
            resourcesDirty = true;
        }

        private void ResolveDependencies()
        {
            if (!dependencyResolutionAttempted ||
                (weatherDomain != null && weatherDomain.gameObject != gameObject))
            {
                weatherDomain = GetComponent<WeatherWindDomain>();
                dependencyResolutionAttempted = true;
            }

            resolvedCamera = weatherDomain != null
                ? weatherDomain.TargetCamera
                : null;
        }


        private void RefreshDomainRuntimeState()
        {
            int configurationHash = weatherDomain.SimulationConfigurationHash;
            float simulationTime = weatherDomain.SimulationTime;
            Vector2 fieldOrigin = weatherDomain.FieldOriginXZ;
            if (!domainRuntimeStateInitialized)
            {
                lastDomainConfigurationHash = configurationHash;
                lastDomainSimulationTime = simulationTime;
                lastFieldOriginXZ = fieldOrigin;
                domainRuntimeStateInitialized = true;
                return;
            }

            bool configurationChanged =
                configurationHash != lastDomainConfigurationHash;
            bool simulationRewound =
                simulationTime + 0.0001f < lastDomainSimulationTime;
            Vector2 originDelta = fieldOrigin - lastFieldOriginXZ;
            float teleportThreshold = weatherDomain.FieldWorldSizeMetres * 0.5f;
            bool largeTeleport =
                Mathf.Abs(originDelta.x) >= teleportThreshold ||
                Mathf.Abs(originDelta.y) >= teleportThreshold;

            lastDomainConfigurationHash = configurationHash;
            lastDomainSimulationTime = simulationTime;
            lastFieldOriginXZ = fieldOrigin;
            if (!configurationChanged && !simulationRewound && !largeTeleport)
            {
                return;
            }

            if (activeTrailCount > 0)
            {
                ClearAllTrails(false);
            }

            ClearCooldowns();
            spawnAccumulator = 0f;
            if (configurationChanged)
            {
                totalDomainConfigurationResetCount++;
            }

            if (simulationRewound)
            {
                totalSimulationRewindResetCount++;
            }

            if (largeTeleport)
            {
                totalLargeTeleportResetCount++;
            }
        }

        private bool CanRun(bool updateError)
        {
            if (!Application.isPlaying)
            {
                if (updateError)
                {
                    lastError = string.Empty;
                }

                return false;
            }

            string error = string.Empty;
            bool ready = true;

            if (trailShader == null)
            {
                error = MissingShaderError;
                ready = false;
            }
            else if (!trailShader.isSupported)
            {
                error = UnsupportedShaderError;
                ready = false;
            }
            else if (!resourcesReady || runtimeMaterial == null)
            {
                error = RuntimeResourcesError;
                ready = false;
            }
            else if (weatherDomain == null)
            {
                error = MissingDomainError;
                ready = false;
            }
            else if (!weatherDomain.isActiveAndEnabled ||
                     WeatherWindDomain.PublishedDomain != weatherDomain)
            {
                error = DomainNotPublishedError;
                ready = false;
            }
            else if (!weatherDomain.ResourcesReady)
            {
                error = DomainResourcesError;
                ready = false;
            }
            else if (resolvedCamera == null)
            {
                error = MissingCameraError;
                ready = false;
            }

            if (updateError)
            {
                lastError = error;
            }

            return ready;
        }

        private bool EnsureResources()
        {
            if (!Application.isPlaying)
            {
                if (trailMesh != null || runtimeMaterial != null || resourcesReady)
                {
                    ReleaseResources();
                }

                lastError = string.Empty;
                return false;
            }

            if (trailShader == null)
            {
                if (trailMesh != null || runtimeMaterial != null || resourcesReady)
                {
                    ReleaseResources();
                }

                lastError = MissingShaderError;
                return false;
            }

            if (!trailShader.isSupported)
            {
                if (trailMesh != null || runtimeMaterial != null || resourcesReady)
                {
                    ReleaseResources();
                }

                lastError = UnsupportedShaderError;
                return false;
            }

            int expectedCandidates = candidateGridResolution * candidateGridResolution;
            int expectedTrailPoints = maximumActiveTrails * maximumCentrelinePoints;
            int expectedVertices = expectedTrailPoints * 2;
            int expectedIndices = maximumActiveTrails *
                                  (maximumCentrelinePoints - 1) * 6;

            if (!resourcesDirty &&
                resourcesReady &&
                trailMesh != null &&
                runtimeMaterial != null &&
                runtimeMaterial.shader == trailShader &&
                trailActive != null && trailActive.Length == maximumActiveTrails &&
                trailTotalLifetimes != null &&
                trailTotalLifetimes.Length == maximumActiveTrails &&
                trailUsesBroadWave != null &&
                trailUsesBroadWave.Length == maximumActiveTrails &&
                candidateWorldPositions != null &&
                candidateWorldPositions.Length == expectedCandidates &&
                trailPoints != null && trailPoints.Length == expectedTrailPoints &&
                undeformedPathScratch != null &&
                undeformedPathScratch.Length == maximumCentrelinePoints &&
                meshVertices != null && meshVertices.Length == expectedVertices &&
                meshIndices != null && meshIndices.Length == expectedIndices)
            {
                return true;
            }

            ReleaseResources();
            lastError = string.Empty;

            try
            {
                trailActive = new bool[maximumActiveTrails];
                trailSeedsXZ = new Vector2[maximumActiveTrails];
                trailBirthTimes = new float[maximumActiveTrails];
                trailTotalLifetimes = new float[maximumActiveTrails];
                trailPointCounts = new int[maximumActiveTrails];
                trailLengths = new float[maximumActiveTrails];
                trailStrengths = new float[maximumActiveTrails];
                trailMinimumAlignments = new float[maximumActiveTrails];
                trailUsesBroadWave = new bool[maximumActiveTrails];
                trailPoints = new Vector3[expectedTrailPoints];
                trailPointDistances = new float[expectedTrailPoints];

                cooldownActive = new bool[maximumActiveTrails];
                cooldownSeedsXZ = new Vector2[maximumActiveTrails];
                cooldownExpiryTimes = new float[maximumActiveTrails];

                candidateWorldPositions = new Vector3[expectedCandidates];
                candidateStrengths = new float[expectedCandidates];
                candidateScores = new float[expectedCandidates];
                candidateNearestDistances = new float[expectedCandidates];
                candidateStatuses = new WeatherWindTrailCandidateStatus[expectedCandidates];
                topCandidateIndices = new int[MaximumTopCandidateCapacity];
                topCandidateScores = new float[MaximumTopCandidateCapacity];

                forwardScratch = new Vector2[maximumCentrelinePoints];
                combinedPathScratch = new Vector2[maximumCentrelinePoints];
                undeformedPathScratch = new Vector2[maximumCentrelinePoints];
                worldPathScratch = new Vector3[maximumCentrelinePoints];
                pathDistanceScratch = new float[maximumCentrelinePoints];

                meshVertices = new TrailVertex[expectedVertices];
                meshIndices = new ushort[expectedIndices];
                BuildFixedIndexBuffer();
                WriteAllInactiveVertices();

                trailMesh = new Mesh
                {
                    name = "PS3D_WeatherWindTrails_Runtime",
                    hideFlags = HideFlags.HideAndDontSave
                };
                trailMesh.MarkDynamic();
                trailMesh.SetVertexBufferParams(
                    expectedVertices,
                    new VertexAttributeDescriptor(
                        VertexAttribute.Position,
                        VertexAttributeFormat.Float32,
                        3),
                    new VertexAttributeDescriptor(
                        VertexAttribute.Normal,
                        VertexAttributeFormat.Float32,
                        3),
                    new VertexAttributeDescriptor(
                        VertexAttribute.Color,
                        VertexAttributeFormat.UNorm8,
                        4),
                    new VertexAttributeDescriptor(
                        VertexAttribute.TexCoord0,
                        VertexAttributeFormat.Float32,
                        2),
                    new VertexAttributeDescriptor(
                        VertexAttribute.TexCoord1,
                        VertexAttributeFormat.Float32,
                        4),
                    new VertexAttributeDescriptor(
                        VertexAttribute.TexCoord2,
                        VertexAttributeFormat.Float32,
                        4));
                trailMesh.SetIndexBufferParams(expectedIndices, IndexFormat.UInt16);

                const MeshUpdateFlags uploadFlags =
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontValidateIndices;
                trailMesh.SetVertexBufferData(
                    meshVertices,
                    0,
                    0,
                    expectedVertices,
                    0,
                    uploadFlags);
                trailMesh.SetIndexBufferData(
                    meshIndices,
                    0,
                    0,
                    expectedIndices,
                    uploadFlags);
                trailMesh.subMeshCount = 1;
                trailMesh.SetSubMesh(
                    0,
                    new SubMeshDescriptor(
                        0,
                        expectedIndices,
                        MeshTopology.Triangles),
                    MeshUpdateFlags.DontRecalculateBounds);

                runtimeMaterial = new Material(trailShader)
                {
                    name = "PS3D Weather Wind Trails Runtime Material",
                    hideFlags = HideFlags.HideAndDontSave
                };
                ApplyRuntimeMaterialProperties();

                activeTrailCount = 0;
                cooldownCount = 0;
                nextSlotSearchIndex = 0;
                lastMeshUploadVertexCount = expectedVertices;
                UpdateMeshBounds();
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

        private void BuildFixedIndexBuffer()
        {
            int indexCursor = 0;
            for (int trailIndex = 0; trailIndex < maximumActiveTrails; trailIndex++)
            {
                int trailVertexStart = trailIndex * maximumCentrelinePoints * 2;
                for (int pointIndex = 0;
                     pointIndex < maximumCentrelinePoints - 1;
                     pointIndex++)
                {
                    int currentLeft = trailVertexStart + pointIndex * 2;
                    int currentRight = currentLeft + 1;
                    int nextLeft = currentLeft + 2;
                    int nextRight = currentLeft + 3;

                    meshIndices[indexCursor++] = (ushort)currentLeft;
                    meshIndices[indexCursor++] = (ushort)nextLeft;
                    meshIndices[indexCursor++] = (ushort)currentRight;
                    meshIndices[indexCursor++] = (ushort)currentRight;
                    meshIndices[indexCursor++] = (ushort)nextLeft;
                    meshIndices[indexCursor++] = (ushort)nextRight;
                }
            }
        }

        private void WriteAllInactiveVertices()
        {
            for (int trailIndex = 0; trailIndex < maximumActiveTrails; trailIndex++)
            {
                WriteInactiveSlotVertices(trailIndex);
            }
        }

        private void WriteInactiveSlotVertices(int trailIndex)
        {
            int firstVertex = TrailVertexIndex(trailIndex, 0, 0);
            int vertexCount = maximumCentrelinePoints * 2;
            var inactiveVertex = new TrailVertex
            {
                position = Vector3.zero,
                tangent = Vector3.forward,
                signedHalfWidthAndDistance = Vector2.zero,
                lifecycleMotion = Vector4.zero,
                lifecycleTiming = Vector4.zero,
                presentation = new Color32(0, 0, 0, 0)
            };

            for (int index = 0; index < vertexCount; index++)
            {
                meshVertices[firstVertex + index] = inactiveVertex;
            }
        }

        private void TrySpawnTrail()
        {
            totalSpawnAttemptCount++;
            int attemptEpoch = spawnEpoch++;
            currentAttemptTargetWindSampleCount = 0;
            int freeSlot = FindFreeTrailSlot();
            if (freeSlot < 0)
            {
                FinishAttemptSampling();
                return;
            }

            float sampleTime = weatherDomain.SimulationTime;
            int selectedCandidate;
            using (CandidateSelectionProfilerMarker.Auto())
            {
                selectedCandidate = EvaluateAndSelectCandidate(sampleTime, attemptEpoch);
            }

            if (selectedCandidate < 0)
            {
                totalNoEligibleCandidateCount++;
                FinishAttemptSampling();
                return;
            }

            Vector3 selectedPosition = candidateWorldPositions[selectedCandidate];
            bool pathBuilt;
            int pathPointCount;
            float pathLength;
            float minimumAlignment;
            bool usedBroadWave;
            using (PathIntegrationProfilerMarker.Auto())
            {
                pathBuilt = TryBuildPath(
                    new Vector2(selectedPosition.x, selectedPosition.z),
                    selectedPosition.y,
                    sampleTime,
                    attemptEpoch,
                    out pathPointCount,
                    out pathLength,
                    out minimumAlignment,
                    out usedBroadWave);
            }

            if (!pathBuilt)
            {
                totalPathRejectionCount++;
                FinishAttemptSampling();
                return;
            }

            uint propertyHash = MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                unchecked((uint)selectedCandidate),
                0x27d4eb2fu);
            if (!TryResolveTrailLifecycle(
                    pathLength,
                    propertyHash,
                    out ResolvedTrailLifecycle lifecycle))
            {
                totalPathRejectionCount++;
                FinishAttemptSampling();
                return;
            }

            candidateStatuses[selectedCandidate] =
                WeatherWindTrailCandidateStatus.Selected;
            ActivateTrail(
                freeSlot,
                selectedCandidate,
                attemptEpoch,
                pathPointCount,
                pathLength,
                minimumAlignment,
                usedBroadWave,
                lifecycle);
            totalSuccessfulSpawnCount++;
            FinishAttemptSampling();
        }

        private int EvaluateAndSelectCandidate(float sampleTime, int attemptEpoch)
        {
            Rect fieldRect = weatherDomain.GetFieldWorldRectXZ();
            float anchorY = weatherDomain.GetDebugAnchorPosition().y;
            int grid = candidateGridResolution;
            int candidateCount = grid * grid;
            float cellWidth = fieldRect.width / grid;
            float cellHeight = fieldRect.height / grid;
            int topCount = 0;

            lastCandidateCount = candidateCount;
            lastVisibleCandidateCount = 0;
            lastEligibleCandidateCount = 0;
            lastSampledCandidateMinimumStrength = float.PositiveInfinity;
            lastSampledCandidateMaximumStrength = float.NegativeInfinity;
            lastAcceptedCandidateStrength = -1f;
            lastAcceptedNearestSeparation = -1f;
            lastGeneratedPathPointCount = 0;
            lastGeneratedPathLengthMetres = 0f;
            lastGeneratedPathMinimumAlignment = -1f;
            lastGeneratedPathUsedBroadWave = false;

            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                int cellX = candidateIndex % grid;
                int cellY = candidateIndex / grid;
                uint baseHash = CandidateHash(cellX, cellY, attemptEpoch, 0u);
                float jitterX = (Hash01(baseHash ^ 0xa511e9b3u) - 0.5f) *
                                candidateCellJitter;
                float jitterY = (Hash01(baseHash ^ 0x63d83595u) - 0.5f) *
                                candidateCellJitter;
                float worldX = fieldRect.xMin +
                    (cellX + 0.5f + jitterX) * cellWidth;
                float worldZ = fieldRect.yMin +
                    (cellY + 0.5f + jitterY) * cellHeight;
                float altitude = RandomRange(
                    minimumAltitudeMetres,
                    maximumAltitudeMetres,
                    baseHash ^ 0xb5297a4du);
                Vector3 worldPosition = new Vector3(
                    worldX,
                    anchorY + altitude,
                    worldZ);

                candidateWorldPositions[candidateIndex] = worldPosition;
                candidateStrengths[candidateIndex] = 0f;
                candidateScores[candidateIndex] = 0f;
                candidateNearestDistances[candidateIndex] = -1f;
                candidateStatuses[candidateIndex] =
                    WeatherWindTrailCandidateStatus.NotEvaluated;
                totalCandidateEvaluationCount++;

                if (!IsInsideExpandedViewport(worldPosition, candidateViewportMargin))
                {
                    candidateStatuses[candidateIndex] =
                        WeatherWindTrailCandidateStatus.OutsideViewport;
                    totalViewportRejectionCount++;
                    continue;
                }

                lastVisibleCandidateCount++;
                Vector2 targetWind = SampleTargetWind(
                    new Vector2(worldX, worldZ),
                    sampleTime);
                float strength = targetWind.magnitude;
                candidateStrengths[candidateIndex] = strength;
                lastSampledCandidateMinimumStrength = Mathf.Min(
                    lastSampledCandidateMinimumStrength,
                    strength);
                lastSampledCandidateMaximumStrength = Mathf.Max(
                    lastSampledCandidateMaximumStrength,
                    strength);

                if (strength < minimumWindStrength)
                {
                    candidateStatuses[candidateIndex] =
                        WeatherWindTrailCandidateStatus.BelowWindFloor;
                    totalCalmRejectionCount++;
                    continue;
                }

                float nearestDistance = ComputeNearestOccupiedDistance(
                    new Vector2(worldX, worldZ));
                candidateNearestDistances[candidateIndex] =
                    float.IsPositiveInfinity(nearestDistance)
                        ? -1f
                        : nearestDistance;
                if (!float.IsPositiveInfinity(nearestDistance) &&
                    nearestDistance < minimumTrailSeparationMetres)
                {
                    candidateStatuses[candidateIndex] =
                        WeatherWindTrailCandidateStatus.TooClose;
                    totalSeparationRejectionCount++;
                    continue;
                }

                float maximumWind = Mathf.Max(
                    minimumWindStrength + 0.0001f,
                    weatherDomain.MaximumWindStrength);
                float strength01 = Mathf.Clamp01(
                    (strength - minimumWindStrength) /
                    (maximumWind - minimumWindStrength));
                float spacing01 = float.IsPositiveInfinity(nearestDistance)
                    ? 1f
                    : Mathf.Lerp(
                        0.35f,
                        1f,
                        Mathf.Clamp01(
                            (nearestDistance - minimumTrailSeparationMetres) /
                            minimumTrailSeparationMetres));
                float score = Mathf.Pow(
                                  Mathf.Max(0.0001f, strength01),
                                  strengthScoreExponent) *
                              Mathf.Pow(
                                  Mathf.Max(0.0001f, spacing01),
                                  spacingScoreExponent);

                candidateScores[candidateIndex] = score;
                candidateStatuses[candidateIndex] =
                    WeatherWindTrailCandidateStatus.Eligible;
                lastEligibleCandidateCount++;
                InsertTopCandidate(candidateIndex, score, ref topCount);
            }

            if (lastSampledCandidateMinimumStrength == float.PositiveInfinity)
            {
                lastSampledCandidateMinimumStrength = -1f;
                lastSampledCandidateMaximumStrength = -1f;
            }

            if (topCount <= 0)
            {
                return -1;
            }

            return SelectWeightedTopCandidate(topCount, attemptEpoch);
        }

        private void InsertTopCandidate(
            int candidateIndex,
            float score,
            ref int topCount)
        {
            int capacity = Mathf.Min(
                strongestCandidateSubset,
                MaximumTopCandidateCapacity);
            if (topCount == capacity &&
                score <= topCandidateScores[capacity - 1])
            {
                return;
            }

            int insertIndex = topCount;
            if (insertIndex > capacity - 1)
            {
                insertIndex = capacity - 1;
            }

            while (insertIndex > 0 &&
                   topCandidateScores[insertIndex - 1] < score)
            {
                if (insertIndex < capacity)
                {
                    topCandidateScores[insertIndex] =
                        topCandidateScores[insertIndex - 1];
                    topCandidateIndices[insertIndex] =
                        topCandidateIndices[insertIndex - 1];
                }

                insertIndex--;
            }

            if (insertIndex >= capacity)
            {
                return;
            }

            topCandidateScores[insertIndex] = score;
            topCandidateIndices[insertIndex] = candidateIndex;
            if (topCount < capacity)
            {
                topCount++;
            }
        }

        private int SelectWeightedTopCandidate(int topCount, int attemptEpoch)
        {
            float totalWeight = 0f;
            for (int index = 0; index < topCount; index++)
            {
                totalWeight += Mathf.Max(0.0001f, topCandidateScores[index]);
            }

            if (totalWeight <= 0f)
            {
                return topCandidateIndices[0];
            }

            uint selectionHash = MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                0xd1b54a35u,
                0x94d049bbu);
            float targetWeight = Hash01(selectionHash) * totalWeight;
            float accumulated = 0f;
            for (int index = 0; index < topCount; index++)
            {
                accumulated += Mathf.Max(0.0001f, topCandidateScores[index]);
                if (targetWeight <= accumulated)
                {
                    return topCandidateIndices[index];
                }
            }

            return topCandidateIndices[topCount - 1];
        }

        private bool TryBuildPath(
            Vector2 seedXZ,
            float seedWorldY,
            float sampleTime,
            int attemptEpoch,
            out int pointCount,
            out float pathLength,
            out float minimumAlignment,
            out bool usedBroadWave)
        {
            pointCount = 0;
            pathLength = 0f;
            minimumAlignment = -1f;
            usedBroadWave = false;

            Rect fieldRect = weatherDomain.GetFieldWorldRectXZ();
            float safetyMargin = Mathf.Max(
                weatherDomain.CellSizeMetres,
                integrationStepMetres * 0.5f);
            Rect safeRect = Rect.MinMaxRect(
                fieldRect.xMin + safetyMargin,
                fieldRect.yMin + safetyMargin,
                fieldRect.xMax - safetyMargin,
                fieldRect.yMax - safetyMargin);
            if (safeRect.width <= 0f || safeRect.height <= 0f)
            {
                safeRect = fieldRect;
            }

            int combinedCount = BuildIntegrationSide(
                seedXZ,
                sampleTime,
                1f,
                maximumCentrelinePoints - 1,
                safeRect,
                forwardScratch);
            if (combinedCount < 2)
            {
                return false;
            }

            Array.Copy(
                forwardScratch,
                combinedPathScratch,
                combinedCount);

            if (!TryEvaluateCompletedPath(
                    combinedCount,
                    sampleTime,
                    safeRect,
                    out pathLength,
                    out minimumAlignment))
            {
                return false;
            }

            usedBroadWave = TryApplyOccasionalBroadWave(
                combinedCount,
                sampleTime,
                attemptEpoch,
                safeRect,
                ref pathLength,
                ref minimumAlignment);

            uint verticalHash = MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                0x9e3779b9u,
                0x85ebca6bu);
            float verticalPhase = Hash01(verticalHash) * Mathf.PI * 2f;
            for (int index = 0; index < combinedCount; index++)
            {
                float normalizedDistance = pathLength > 0f
                    ? pathDistanceScratch[index] / pathLength
                    : 0f;
                float verticalOffset = Mathf.Sin(
                    normalizedDistance * Mathf.PI * 2f + verticalPhase) *
                    maximumVerticalDeviationMetres;
                Vector2 pointXZ = combinedPathScratch[index];
                worldPathScratch[index] = new Vector3(
                    pointXZ.x,
                    seedWorldY + verticalOffset,
                    pointXZ.y);
            }

            pointCount = combinedCount;
            return true;
        }

        private bool TryEvaluateCompletedPath(
            int pointCount,
            float sampleTime,
            Rect safeRect,
            out float pathLength,
            out float minimumAlignment)
        {
            pathLength = 0f;
            minimumAlignment = 1f;
            pathDistanceScratch[0] = 0f;

            for (int index = 0; index < pointCount; index++)
            {
                Vector2 point = combinedPathScratch[index];
                if (!safeRect.Contains(point))
                {
                    return false;
                }

                if (index > 0)
                {
                    pathLength += Vector2.Distance(
                        combinedPathScratch[index - 1],
                        point);
                    pathDistanceScratch[index] = pathLength;
                }
            }

            if (pathLength < minimumCompletedPathLengthMetres)
            {
                return false;
            }

            for (int index = 0; index < pointCount - 1; index++)
            {
                Vector2 segment =
                    combinedPathScratch[index + 1] - combinedPathScratch[index];
                float segmentMagnitude = segment.magnitude;
                if (segmentMagnitude <= 0.0001f)
                {
                    return false;
                }

                Vector2 midpoint =
                    (combinedPathScratch[index] + combinedPathScratch[index + 1]) *
                    0.5f;
                Vector2 targetWind = SampleTargetWind(midpoint, sampleTime);
                float windMagnitude = targetWind.magnitude;
                if (windMagnitude < minimumPathWindStrength)
                {
                    return false;
                }

                float alignment = Vector2.Dot(
                    segment / segmentMagnitude,
                    targetWind / windMagnitude);
                minimumAlignment = Mathf.Min(minimumAlignment, alignment);
                if (alignment < minimumSegmentWindAlignment)
                {
                    return false;
                }
            }

            return true;
        }

        private bool TryApplyOccasionalBroadWave(
            int pointCount,
            float sampleTime,
            int attemptEpoch,
            Rect safeRect,
            ref float pathLength,
            ref float minimumAlignment)
        {
            if (occasionalBroadWaveChance <= 0f ||
                occasionalBroadWaveStrengthMetres <= 0.0001f ||
                pointCount < 4)
            {
                return false;
            }

            uint waveHash = MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                0x6a09e667u,
                0xbb67ae85u);
            if (Hash01(waveHash ^ 0x3c6ef372u) > occasionalBroadWaveChance)
            {
                return false;
            }

            Array.Copy(
                combinedPathScratch,
                undeformedPathScratch,
                pointCount);
            float originalPathLength = pathLength;
            float originalMinimumAlignment = minimumAlignment;
            float amplitude = occasionalBroadWaveStrengthMetres * Mathf.Lerp(
                0.65f,
                1f,
                Hash01(waveHash ^ 0xa54ff53au));
            float sideSign = Hash01(waveHash ^ 0x510e527fu) < 0.5f
                ? -1f
                : 1f;
            float phase = Hash01(waveHash ^ 0x9b05688cu) * Mathf.PI;

            for (int index = 0; index < pointCount; index++)
            {
                float normalizedDistance = originalPathLength > 0f
                    ? pathDistanceScratch[index] / originalPathLength
                    : 0f;
                Vector2 tangent = ComputeScratchTangent(
                    undeformedPathScratch,
                    index,
                    pointCount);
                Vector2 lateral = new Vector2(-tangent.y, tangent.x);
                float endpointEnvelope = Mathf.Sin(
                    normalizedDistance * Mathf.PI);
                float wave = Mathf.Sin(
                    normalizedDistance * Mathf.PI * 2f + phase);
                combinedPathScratch[index] = undeformedPathScratch[index] +
                    lateral * (wave * endpointEnvelope * amplitude * sideSign);
            }

            if (!PathSelfApproaches(combinedPathScratch, pointCount) &&
                TryEvaluateCompletedPath(
                    pointCount,
                    sampleTime,
                    safeRect,
                    out pathLength,
                    out minimumAlignment))
            {
                return true;
            }

            Array.Copy(
                undeformedPathScratch,
                combinedPathScratch,
                pointCount);
            pathLength = originalPathLength;
            minimumAlignment = originalMinimumAlignment;
            RebuildPathDistances(pointCount, out pathLength);
            return false;
        }

        private void RebuildPathDistances(int pointCount, out float pathLength)
        {
            pathLength = 0f;
            pathDistanceScratch[0] = 0f;
            for (int index = 1; index < pointCount; index++)
            {
                pathLength += Vector2.Distance(
                    combinedPathScratch[index - 1],
                    combinedPathScratch[index]);
                pathDistanceScratch[index] = pathLength;
            }
        }

        private bool PathSelfApproaches(Vector2[] points, int count)
        {
            float minimumSquared =
                selfApproachDistanceMetres * selfApproachDistanceMetres;
            for (int index = 2; index < count; index++)
            {
                for (int earlier = 0; earlier < index - 1; earlier++)
                {
                    if ((points[index] - points[earlier]).sqrMagnitude <
                        minimumSquared)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Vector2 ComputeScratchTangent(
            Vector2[] points,
            int index,
            int count)
        {
            Vector2 tangent;
            if (index <= 0)
            {
                tangent = points[1] - points[0];
            }
            else if (index >= count - 1)
            {
                tangent = points[count - 1] - points[count - 2];
            }
            else
            {
                tangent = points[index + 1] - points[index - 1];
            }

            return tangent.sqrMagnitude > MinimumDirectionMagnitudeSquared
                ? tangent.normalized
                : Vector2.right;
        }

        private int BuildIntegrationSide(
            Vector2 seedXZ,
            float sampleTime,
            float directionSign,
            int maximumNewPoints,
            Rect safeRect,
            Vector2[] output)
        {
            output[0] = seedXZ;
            int count = 1;
            Vector2 current = seedXZ;
            Vector2 previousDirection = Vector2.zero;
            float minimumTurnDot = Mathf.Cos(
                maximumTurnDegreesPerSegment * Mathf.Deg2Rad);

            for (int stepIndex = 0; stepIndex < maximumNewPoints; stepIndex++)
            {
                Vector2 initialWind = SampleTargetWind(current, sampleTime);
                float initialMagnitude = initialWind.magnitude;
                if (initialMagnitude < minimumPathWindStrength)
                {
                    break;
                }

                Vector2 initialDirection =
                    initialWind / initialMagnitude * directionSign;
                Vector2 midpoint = current +
                    initialDirection * (integrationStepMetres * 0.5f);
                Vector2 midpointWind = SampleTargetWind(midpoint, sampleTime);
                float midpointMagnitude = midpointWind.magnitude;
                if (midpointMagnitude < minimumPathWindStrength)
                {
                    break;
                }

                Vector2 movementDirection =
                    midpointWind / midpointMagnitude * directionSign;
                if (previousDirection.sqrMagnitude >
                        MinimumDirectionMagnitudeSquared &&
                    Vector2.Dot(previousDirection, movementDirection) < minimumTurnDot)
                {
                    break;
                }

                Vector2 next = current + movementDirection * integrationStepMetres;
                if (!safeRect.Contains(next) ||
                    SelfApproaches(output, count, next))
                {
                    break;
                }

                output[count++] = next;
                current = next;
                previousDirection = movementDirection;
            }

            return count;
        }

        private bool SelfApproaches(Vector2[] points, int count, Vector2 candidate)
        {
            if (count < 3)
            {
                return false;
            }

            float minimumSquared =
                selfApproachDistanceMetres * selfApproachDistanceMetres;
            for (int index = 0; index < count - 1; index++)
            {
                if ((points[index] - candidate).sqrMagnitude < minimumSquared)
                {
                    return true;
                }
            }

            return false;
        }

        private bool TryResolveTrailLifecycle(
            float availablePathLength,
            uint propertyHash,
            out ResolvedTrailLifecycle lifecycle)
        {
            lifecycle = default;
            float desiredBodyLength = RandomRange(
                minimumVisibleBodyLengthMetres,
                maximumVisibleBodyLengthMetres,
                propertyHash ^ 0xb55a4f09u);
            float desiredAliveDuration = RandomRange(
                minimumAliveDurationSeconds,
                maximumAliveDurationSeconds,
                propertyHash ^ 0x165667b1u);
            float desiredSpeed = RandomRange(
                minimumPresentationSpeed,
                maximumPresentationSpeed,
                propertyHash ^ 0xfd7046c5u);
            float minimumSpeed = minimumPresentationSpeed;

            float bodyLength = desiredBodyLength;
            float maximumAliveAtMinimumSpeed = MaximumAliveDurationForPath(
                availablePathLength,
                bodyLength,
                minimumSpeed);
            if (maximumAliveAtMinimumSpeed < minimumAliveDurationSeconds)
            {
                bodyLength = MaximumBodyLengthForPath(
                    availablePathLength,
                    minimumSpeed,
                    minimumAliveDurationSeconds);
                if (bodyLength < minimumVisibleBodyLengthMetres)
                {
                    return false;
                }

                bodyLength = Mathf.Min(desiredBodyLength, bodyLength);
                maximumAliveAtMinimumSpeed = MaximumAliveDurationForPath(
                    availablePathLength,
                    bodyLength,
                    minimumSpeed);
            }

            float aliveDuration = Mathf.Clamp(
                desiredAliveDuration,
                minimumAliveDurationSeconds,
                Mathf.Max(
                    minimumAliveDurationSeconds,
                    maximumAliveAtMinimumSpeed));
            float speed = FindMaximumFittingSpeed(
                availablePathLength,
                bodyLength,
                aliveDuration,
                minimumSpeed,
                desiredSpeed);
            if (speed < minimumSpeed - 0.0001f)
            {
                return false;
            }

            float allowance = ResolveTipSpeedAllowance(speed);
            float spawnDuration = bodyLength /
                Mathf.Max(MinimumElapsedSeconds, speed + allowance);
            float despawnDuration = bodyLength /
                Mathf.Max(MinimumElapsedSeconds, allowance * 2f);
            float requiredPathLength = RequiredLifecyclePathLength(
                bodyLength,
                aliveDuration,
                speed);
            if (requiredPathLength > availablePathLength + 0.001f)
            {
                return false;
            }

            lifecycle.bodyLength = bodyLength;
            lifecycle.travelSpeed = speed;
            lifecycle.tipSpeedAllowance = allowance;
            lifecycle.aliveDuration = aliveDuration;
            lifecycle.spawnDuration = spawnDuration;
            lifecycle.despawnDuration = despawnDuration;
            lifecycle.totalLifetime = spawnDuration + aliveDuration +
                despawnDuration;
            lifecycle.requiredPathLength = requiredPathLength;
            lifecycle.pointedEndLength = Mathf.Min(
                pointedEndLengthMetres,
                bodyLength * 0.49f);
            return true;
        }

        private float ResolveTipSpeedAllowance(float travelSpeed)
        {
            return Mathf.Clamp(
                lifecycleTipSpeedAllowance,
                0.05f,
                Mathf.Max(0.05f, travelSpeed * 0.9f));
        }

        private float RequiredLifecyclePathLength(
            float bodyLength,
            float aliveDuration,
            float travelSpeed)
        {
            float allowance = ResolveTipSpeedAllowance(travelSpeed);
            return travelSpeed * aliveDuration +
                bodyLength * 0.5f +
                travelSpeed * bodyLength /
                    Mathf.Max(MinimumElapsedSeconds, allowance * 2f);
        }

        private float MaximumAliveDurationForPath(
            float availablePathLength,
            float bodyLength,
            float travelSpeed)
        {
            float allowance = ResolveTipSpeedAllowance(travelSpeed);
            float nonAliveDistance = bodyLength * 0.5f +
                travelSpeed * bodyLength /
                    Mathf.Max(MinimumElapsedSeconds, allowance * 2f);
            return (availablePathLength - nonAliveDistance) /
                Mathf.Max(MinimumElapsedSeconds, travelSpeed);
        }

        private float MaximumBodyLengthForPath(
            float availablePathLength,
            float travelSpeed,
            float aliveDuration)
        {
            float allowance = ResolveTipSpeedAllowance(travelSpeed);
            float availableForBody = availablePathLength -
                travelSpeed * aliveDuration;
            float bodyDistanceFactor = 0.5f +
                travelSpeed /
                    Mathf.Max(MinimumElapsedSeconds, allowance * 2f);
            return availableForBody /
                Mathf.Max(MinimumElapsedSeconds, bodyDistanceFactor);
        }

        private float FindMaximumFittingSpeed(
            float availablePathLength,
            float bodyLength,
            float aliveDuration,
            float minimumSpeed,
            float desiredSpeed)
        {
            if (RequiredLifecyclePathLength(
                    bodyLength,
                    aliveDuration,
                    minimumSpeed) > availablePathLength + 0.001f)
            {
                return -1f;
            }

            if (RequiredLifecyclePathLength(
                    bodyLength,
                    aliveDuration,
                    desiredSpeed) <= availablePathLength + 0.001f)
            {
                return desiredSpeed;
            }

            float lower = minimumSpeed;
            float upper = desiredSpeed;
            for (int iteration = 0; iteration < 20; iteration++)
            {
                float midpoint = (lower + upper) * 0.5f;
                if (RequiredLifecyclePathLength(
                        bodyLength,
                        aliveDuration,
                        midpoint) <= availablePathLength)
                {
                    lower = midpoint;
                }
                else
                {
                    upper = midpoint;
                }
            }

            return lower;
        }

        private void ActivateTrail(
            int trailIndex,
            int selectedCandidate,
            int attemptEpoch,
            int pointCount,
            float pathLength,
            float minimumAlignment,
            bool usedBroadWave,
            ResolvedTrailLifecycle lifecycle)
        {
            uint propertyHash = MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                unchecked((uint)selectedCandidate),
                0x27d4eb2fu);
            float width = RandomRange(
                minimumWidthMetres,
                maximumWidthMetres,
                propertyHash ^ 0xd3a2646cu);
            float variation = Hash01(propertyHash ^ 0x9e3779b9u);
            float strength = candidateStrengths[selectedCandidate];
            float strength01 = Mathf.Clamp01(
                strength / Mathf.Max(0.0001f, weatherDomain.MaximumWindStrength));

            trailActive[trailIndex] = true;
            trailSeedsXZ[trailIndex] = new Vector2(
                candidateWorldPositions[selectedCandidate].x,
                candidateWorldPositions[selectedCandidate].z);
            trailBirthTimes[trailIndex] = presentationTime;
            trailTotalLifetimes[trailIndex] = lifecycle.totalLifetime;
            trailPointCounts[trailIndex] = pointCount;
            trailLengths[trailIndex] = pathLength;
            trailStrengths[trailIndex] = strength;
            trailMinimumAlignments[trailIndex] = minimumAlignment;
            trailUsesBroadWave[trailIndex] = usedBroadWave;
            activeTrailCount++;
            lastAcceptedCandidateStrength = candidateStrengths[selectedCandidate];
            lastAcceptedNearestSeparation =
                candidateNearestDistances[selectedCandidate];

            int firstPoint = TrailPointIndex(trailIndex, 0);
            for (int pointIndex = 0;
                 pointIndex < maximumCentrelinePoints;
                 pointIndex++)
            {
                int sourceIndex = Mathf.Min(pointIndex, pointCount - 1);
                trailPoints[firstPoint + pointIndex] = worldPathScratch[sourceIndex];
                trailPointDistances[firstPoint + pointIndex] =
                    pathDistanceScratch[sourceIndex];
            }

            WriteActiveSlotVertices(
                trailIndex,
                pointCount,
                width,
                variation,
                strength01,
                lifecycle);
            UploadTrailSlot(trailIndex);

            lastGeneratedPathPointCount = pointCount;
            lastGeneratedPathLengthMetres = pathLength;
            lastGeneratedPathMinimumAlignment = minimumAlignment;
            lastGeneratedPathUsedBroadWave = usedBroadWave;
            lastResolvedBodyLengthMetres = lifecycle.bodyLength;
            lastResolvedTravelSpeed = lifecycle.travelSpeed;
            lastResolvedTipSpeedAllowance = lifecycle.tipSpeedAllowance;
            lastResolvedSpawnDuration = lifecycle.spawnDuration;
            lastResolvedAliveDuration = lifecycle.aliveDuration;
            lastResolvedDespawnDuration = lifecycle.despawnDuration;
            lastResolvedTotalLifetime = lifecycle.totalLifetime;
            lastRequiredPathLengthMetres = lifecycle.requiredPathLength;
            if (usedBroadWave)
            {
                totalBroadWaveTrailCount++;
            }

            nextSlotSearchIndex = (trailIndex + 1) % maximumActiveTrails;
        }

        private void WriteActiveSlotVertices(
            int trailIndex,
            int pointCount,
            float width,
            float variation,
            float strength01,
            ResolvedTrailLifecycle lifecycle)
        {
            int firstPoint = TrailPointIndex(trailIndex, 0);
            float halfWidth = width * 0.5f;
            byte opacityByte = ToByte(trailOpacity);
            byte strengthByte = ToByte(strength01);
            byte variationByte = ToByte(variation);
            var lifecycleMotion = new Vector4(
                trailBirthTimes[trailIndex],
                lifecycle.travelSpeed,
                lifecycle.bodyLength,
                lifecycle.aliveDuration);
            var lifecycleTiming = new Vector4(
                lifecycle.spawnDuration,
                lifecycle.despawnDuration,
                lifecycle.pointedEndLength,
                lifecycle.totalLifetime);

            for (int pointIndex = 0;
                 pointIndex < maximumCentrelinePoints;
                 pointIndex++)
            {
                bool pointActive = pointIndex < pointCount;
                int clampedPointIndex = Mathf.Min(pointIndex, pointCount - 1);
                Vector3 position = trailPoints[firstPoint + clampedPointIndex];
                float distance = trailPointDistances[firstPoint + clampedPointIndex];
                Vector3 tangent = ComputePointTangent(
                    trailIndex,
                    clampedPointIndex,
                    pointCount);
                byte activeByte = pointActive ? (byte)255 : (byte)0;
                var presentation = new Color32(
                    opacityByte,
                    strengthByte,
                    variationByte,
                    activeByte);

                int leftVertex = TrailVertexIndex(trailIndex, pointIndex, 0);
                int rightVertex = leftVertex + 1;
                meshVertices[leftVertex] = new TrailVertex
                {
                    position = position,
                    tangent = tangent,
                    signedHalfWidthAndDistance = new Vector2(-halfWidth, distance),
                    lifecycleMotion = lifecycleMotion,
                    lifecycleTiming = lifecycleTiming,
                    presentation = presentation
                };
                meshVertices[rightVertex] = new TrailVertex
                {
                    position = position,
                    tangent = tangent,
                    signedHalfWidthAndDistance = new Vector2(halfWidth, distance),
                    lifecycleMotion = lifecycleMotion,
                    lifecycleTiming = lifecycleTiming,
                    presentation = presentation
                };
            }
        }

        private Vector3 ComputePointTangent(
            int trailIndex,
            int pointIndex,
            int pointCount)
        {
            int firstPoint = TrailPointIndex(trailIndex, 0);
            Vector3 tangent;
            if (pointCount <= 1)
            {
                tangent = Vector3.forward;
            }
            else if (pointIndex <= 0)
            {
                tangent = trailPoints[firstPoint + 1] - trailPoints[firstPoint];
            }
            else if (pointIndex >= pointCount - 1)
            {
                tangent = trailPoints[firstPoint + pointCount - 1] -
                          trailPoints[firstPoint + pointCount - 2];
            }
            else
            {
                tangent = trailPoints[firstPoint + pointIndex + 1] -
                          trailPoints[firstPoint + pointIndex - 1];
            }

            return tangent.sqrMagnitude > MinimumDirectionMagnitudeSquared
                ? tangent.normalized
                : Vector3.forward;
        }

        private void ExpireTrails()
        {
            if (trailActive == null)
            {
                return;
            }

            for (int trailIndex = 0;
                 trailIndex < trailActive.Length;
                 trailIndex++)
            {
                if (!trailActive[trailIndex])
                {
                    continue;
                }

                float age = presentationTime - trailBirthTimes[trailIndex];
                if (age < trailTotalLifetimes[trailIndex])
                {
                    continue;
                }

                ExpireTrail(trailIndex, true);
            }
        }

        private void ExpireTrail(int trailIndex, bool preserveSeparation)
        {
            if (!trailActive[trailIndex])
            {
                return;
            }

            if (preserveSeparation && separationCooldownSeconds > 0f)
            {
                AddCooldown(
                    trailSeedsXZ[trailIndex],
                    presentationTime + separationCooldownSeconds);
            }

            trailActive[trailIndex] = false;
            trailTotalLifetimes[trailIndex] = 0f;
            trailPointCounts[trailIndex] = 0;
            trailLengths[trailIndex] = 0f;
            trailStrengths[trailIndex] = 0f;
            trailMinimumAlignments[trailIndex] = 0f;
            trailUsesBroadWave[trailIndex] = false;
            activeTrailCount = Mathf.Max(0, activeTrailCount - 1);
            WriteInactiveSlotVertices(trailIndex);
            UploadTrailSlot(trailIndex);
        }

        private void AddCooldown(Vector2 seedXZ, float expiryTime)
        {
            int targetIndex = -1;
            float earliestExpiry = float.PositiveInfinity;
            int earliestIndex = 0;

            for (int index = 0; index < cooldownActive.Length; index++)
            {
                if (!cooldownActive[index])
                {
                    targetIndex = index;
                    break;
                }

                if (cooldownExpiryTimes[index] < earliestExpiry)
                {
                    earliestExpiry = cooldownExpiryTimes[index];
                    earliestIndex = index;
                }
            }

            if (targetIndex < 0)
            {
                targetIndex = earliestIndex;
            }
            else
            {
                cooldownCount++;
            }

            cooldownActive[targetIndex] = true;
            cooldownSeedsXZ[targetIndex] = seedXZ;
            cooldownExpiryTimes[targetIndex] = expiryTime;
        }


        private void ClearCooldowns()
        {
            if (cooldownActive == null)
            {
                cooldownCount = 0;
                return;
            }

            Array.Clear(cooldownActive, 0, cooldownActive.Length);
            cooldownCount = 0;
        }

        private void PruneCooldowns()
        {
            if (cooldownActive == null)
            {
                return;
            }

            for (int index = 0; index < cooldownActive.Length; index++)
            {
                if (!cooldownActive[index] ||
                    cooldownExpiryTimes[index] > presentationTime)
                {
                    continue;
                }

                cooldownActive[index] = false;
                cooldownCount = Mathf.Max(0, cooldownCount - 1);
            }
        }

        private float ComputeNearestOccupiedDistance(Vector2 candidateXZ)
        {
            float nearestSquared = float.PositiveInfinity;

            for (int trailIndex = 0;
                 trailIndex < maximumActiveTrails;
                 trailIndex++)
            {
                if (!trailActive[trailIndex])
                {
                    continue;
                }

                nearestSquared = Mathf.Min(
                    nearestSquared,
                    (trailSeedsXZ[trailIndex] - candidateXZ).sqrMagnitude);
            }

            for (int cooldownIndex = 0;
                 cooldownIndex < maximumActiveTrails;
                 cooldownIndex++)
            {
                if (!cooldownActive[cooldownIndex])
                {
                    continue;
                }

                nearestSquared = Mathf.Min(
                    nearestSquared,
                    (cooldownSeedsXZ[cooldownIndex] - candidateXZ).sqrMagnitude);
            }

            return float.IsPositiveInfinity(nearestSquared)
                ? float.PositiveInfinity
                : Mathf.Sqrt(nearestSquared);
        }

        private int FindFreeTrailSlot()
        {
            if (trailActive == null)
            {
                return -1;
            }

            for (int offset = 0; offset < maximumActiveTrails; offset++)
            {
                int trailIndex =
                    (nextSlotSearchIndex + offset) % maximumActiveTrails;
                if (!trailActive[trailIndex])
                {
                    return trailIndex;
                }
            }

            return -1;
        }

        private void ClearAllTrails(bool preserveSeparation)
        {
            if (trailActive == null)
            {
                return;
            }

            for (int trailIndex = 0;
                 trailIndex < maximumActiveTrails;
                 trailIndex++)
            {
                if (trailActive[trailIndex])
                {
                    ExpireTrail(trailIndex, preserveSeparation);
                }
            }
        }

        private bool IsInsideExpandedViewport(Vector3 worldPosition, float margin)
        {
            if (resolvedCamera == null)
            {
                return false;
            }

            Vector3 viewport = resolvedCamera.WorldToViewportPoint(worldPosition);
            return viewport.z > 0f &&
                   viewport.z <= resolvedCamera.farClipPlane &&
                   viewport.x >= -margin && viewport.x <= 1f + margin &&
                   viewport.y >= -margin && viewport.y <= 1f + margin;
        }

        private Vector2 SampleTargetWind(Vector2 worldXZ, float sampleTime)
        {
            currentAttemptTargetWindSampleCount++;
            return weatherDomain.SampleTargetWindXZ(worldXZ, sampleTime);
        }

        private void FinishAttemptSampling()
        {
            lastAttemptTargetWindSampleCount = currentAttemptTargetWindSampleCount;
            totalTargetWindSampleCount += currentAttemptTargetWindSampleCount;
            currentAttemptTargetWindSampleCount = 0;
        }

        private void UploadTrailSlot(int trailIndex)
        {
            if (trailMesh == null || meshVertices == null)
            {
                return;
            }

            using var profilerScope = MeshUploadProfilerMarker.Auto();
            int firstVertex = TrailVertexIndex(trailIndex, 0, 0);
            int vertexCount = maximumCentrelinePoints * 2;
            trailMesh.SetVertexBufferData(
                meshVertices,
                firstVertex,
                firstVertex,
                vertexCount,
                0,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices);
            lastMeshUploadVertexCount = vertexCount;
        }

        private void ApplyRuntimeMaterialProperties()
        {
            if (runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetColor(TrailColorId, trailColor);
            runtimeMaterial.SetFloat(
                UniformBodyOpacityId,
                uniformBodyOpacity ? 1f : 0f);
            runtimeMaterial.SetFloat(EdgeSoftnessId, edgeSoftness);
            runtimeMaterial.SetFloat(
                StrengthOpacityInfluenceId,
                strengthOpacityInfluence);
            runtimeMaterial.SetFloat(
                VariationOpacityInfluenceId,
                variationOpacityInfluence);
            runtimeMaterial.SetFloat(TrailPresentationTimeId, presentationTime);
        }

        private void SubmitTrailRender()
        {
            if (!Application.isPlaying ||
                activeTrailCount <= 0 ||
                trailMesh == null ||
                runtimeMaterial == null ||
                resolvedCamera == null)
            {
                return;
            }

            using var profilerScope = RenderSubmissionProfilerMarker.Auto();
            runtimeMaterial.SetFloat(TrailPresentationTimeId, presentationTime);
            RenderParams renderParams = new RenderParams(runtimeMaterial)
            {
                worldBounds = trailMesh.bounds,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                layer = gameObject.layer,
                entityId = gameObject.GetEntityId(),
                camera = resolvedCamera
            };

            Graphics.RenderMesh(
                renderParams,
                trailMesh,
                0,
                Matrix4x4.identity);
            totalRenderSubmissionCount++;
            lastRenderedTrailCount = activeTrailCount;
        }

        private void UpdateMeshBounds()
        {
            if (trailMesh == null)
            {
                return;
            }

            Rect fieldRect = weatherDomain != null
                ? weatherDomain.GetFieldWorldRectXZ()
                : new Rect(-1f, -1f, 2f, 2f);
            float anchorY = weatherDomain != null
                ? weatherDomain.GetDebugAnchorPosition().y
                : transform.position.y;
            float lowerY = anchorY + minimumAltitudeMetres -
                           maximumVerticalDeviationMetres;
            float upperY = anchorY + maximumAltitudeMetres +
                           maximumVerticalDeviationMetres;
            trailMesh.bounds = new Bounds(
                new Vector3(
                    fieldRect.center.x,
                    (lowerY + upperY) * 0.5f,
                    fieldRect.center.y),
                new Vector3(
                    Mathf.Max(0.1f, fieldRect.width),
                    Mathf.Max(0.1f, upperY - lowerY + 0.5f),
                    Mathf.Max(0.1f, fieldRect.height)));
        }

        private void ResetDiagnosticCounters()
        {
            totalSpawnAttemptCount = 0;
            totalSuccessfulSpawnCount = 0;
            totalCandidateEvaluationCount = 0;
            totalViewportRejectionCount = 0;
            totalCalmRejectionCount = 0;
            totalSeparationRejectionCount = 0;
            totalNoEligibleCandidateCount = 0;
            totalPathRejectionCount = 0;
            totalTargetWindSampleCount = 0;
            totalDomainConfigurationResetCount = 0;
            totalSimulationRewindResetCount = 0;
            totalLargeTeleportResetCount = 0;
            lastCandidateCount = 0;
            lastVisibleCandidateCount = 0;
            lastEligibleCandidateCount = 0;
            lastSampledCandidateMinimumStrength = -1f;
            lastSampledCandidateMaximumStrength = -1f;
            lastAcceptedCandidateStrength = -1f;
            lastAcceptedNearestSeparation = -1f;
            lastGeneratedPathPointCount = 0;
            lastGeneratedPathLengthMetres = 0f;
            lastGeneratedPathMinimumAlignment = -1f;
            lastAttemptTargetWindSampleCount = 0;
            lastMeshUploadVertexCount = 0;
            totalRenderSubmissionCount = 0;
            lastRenderedTrailCount = 0;
            totalBroadWaveTrailCount = 0;
            lastGeneratedPathUsedBroadWave = false;
            lastResolvedBodyLengthMetres = -1f;
            lastResolvedTravelSpeed = -1f;
            lastResolvedTipSpeedAllowance = -1f;
            lastResolvedSpawnDuration = -1f;
            lastResolvedAliveDuration = -1f;
            lastResolvedDespawnDuration = -1f;
            lastResolvedTotalLifetime = -1f;
            lastRequiredPathLengthMetres = -1f;
        }

        private int ComputeConfigurationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + maximumActiveTrails;
                hash = hash * 31 + spawnAttemptsPerSecond.GetHashCode();
                hash = hash * 31 + candidateGridResolution;
                hash = hash * 31 + strongestCandidateSubset;
                hash = hash * 31 + candidateCellJitter.GetHashCode();
                hash = hash * 31 + trailSeed;
                hash = hash * 31 + minimumWindStrength.GetHashCode();
                hash = hash * 31 + strengthScoreExponent.GetHashCode();
                hash = hash * 31 + spacingScoreExponent.GetHashCode();
                hash = hash * 31 + minimumTrailSeparationMetres.GetHashCode();
                hash = hash * 31 + separationCooldownSeconds.GetHashCode();
                hash = hash * 31 + maximumCentrelinePoints;
                hash = hash * 31 + integrationStepMetres.GetHashCode();
                hash = hash * 31 + minimumPathWindStrength.GetHashCode();
                hash = hash * 31 + minimumCompletedPathLengthMetres.GetHashCode();
                hash = hash * 31 + maximumTurnDegreesPerSegment.GetHashCode();
                hash = hash * 31 + selfApproachDistanceMetres.GetHashCode();
                hash = hash * 31 + minimumSegmentWindAlignment.GetHashCode();
                hash = hash * 31 + minimumAliveDurationSeconds.GetHashCode();
                hash = hash * 31 + maximumAliveDurationSeconds.GetHashCode();
                hash = hash * 31 + minimumWidthMetres.GetHashCode();
                hash = hash * 31 + maximumWidthMetres.GetHashCode();
                hash = hash * 31 + minimumPresentationSpeed.GetHashCode();
                hash = hash * 31 + maximumPresentationSpeed.GetHashCode();
                hash = hash * 31 + minimumVisibleBodyLengthMetres.GetHashCode();
                hash = hash * 31 + maximumVisibleBodyLengthMetres.GetHashCode();
                hash = hash * 31 + lifecycleTipSpeedAllowance.GetHashCode();
                hash = hash * 31 + pointedEndLengthMetres.GetHashCode();
                hash = hash * 31 + minimumAltitudeMetres.GetHashCode();
                hash = hash * 31 + maximumAltitudeMetres.GetHashCode();
                hash = hash * 31 + maximumVerticalDeviationMetres.GetHashCode();
                hash = hash * 31 + occasionalBroadWaveChance.GetHashCode();
                hash = hash * 31 + occasionalBroadWaveStrengthMetres.GetHashCode();
                hash = hash * 31 + trailOpacity.GetHashCode();
                hash = hash * 31 + trailColor.GetHashCode();
                hash = hash * 31 + uniformBodyOpacity.GetHashCode();
                hash = hash * 31 + edgeSoftness.GetHashCode();
                hash = hash * 31 + strengthOpacityInfluence.GetHashCode();
                hash = hash * 31 + variationOpacityInfluence.GetHashCode();
                hash = hash * 31 + candidateViewportMargin.GetHashCode();
                hash = hash * 31 +
                    (trailShader != null
                        ? trailShader.GetEntityId().GetHashCode()
                        : 0);
                return hash;
            }
        }

        private void ReleaseResources()
        {
            resourcesReady = false;
            if (runtimeMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(runtimeMaterial);
                }
                else
                {
                    DestroyImmediate(runtimeMaterial);
                }
            }

            runtimeMaterial = null;
            if (trailMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(trailMesh);
                }
                else
                {
                    DestroyImmediate(trailMesh);
                }
            }

            trailMesh = null;
            trailActive = null;
            trailSeedsXZ = null;
            trailBirthTimes = null;
            trailTotalLifetimes = null;
            trailPointCounts = null;
            trailLengths = null;
            trailStrengths = null;
            trailMinimumAlignments = null;
            trailUsesBroadWave = null;
            trailPoints = null;
            trailPointDistances = null;
            cooldownActive = null;
            cooldownSeedsXZ = null;
            cooldownExpiryTimes = null;
            candidateWorldPositions = null;
            candidateStrengths = null;
            candidateScores = null;
            candidateNearestDistances = null;
            candidateStatuses = null;
            topCandidateIndices = null;
            topCandidateScores = null;
            forwardScratch = null;
            combinedPathScratch = null;
            undeformedPathScratch = null;
            worldPathScratch = null;
            pathDistanceScratch = null;
            meshVertices = null;
            meshIndices = null;
            activeTrailCount = 0;
            cooldownCount = 0;
            domainRuntimeStateInitialized = false;
            resourcesDirty = true;
        }

        private int TrailPointIndex(int trailIndex, int pointIndex)
        {
            return trailIndex * maximumCentrelinePoints + pointIndex;
        }

        private int TrailVertexIndex(
            int trailIndex,
            int pointIndex,
            int sideIndex)
        {
            return (trailIndex * maximumCentrelinePoints + pointIndex) * 2 +
                   sideIndex;
        }

        private uint CandidateHash(
            int cellX,
            int cellY,
            int attemptEpoch,
            uint channel)
        {
            return MixHash(
                unchecked((uint)trailSeed),
                unchecked((uint)attemptEpoch),
                unchecked((uint)(cellX + cellY * candidateGridResolution)),
                channel);
        }

        private static uint MixHash(uint a, uint b, uint c, uint d)
        {
            unchecked
            {
                uint state = a * 0x9e3779b9u;
                state ^= b * 0x85ebca6bu;
                state ^= c * 0xc2b2ae35u;
                state ^= d * 0x27d4eb2fu;
                state ^= state >> 16;
                state *= 0x7feb352du;
                state ^= state >> 15;
                state *= 0x846ca68bu;
                state ^= state >> 16;
                return state;
            }
        }

        private static float Hash01(uint state)
        {
            state ^= state >> 16;
            state *= 0x7feb352du;
            state ^= state >> 15;
            state *= 0x846ca68bu;
            state ^= state >> 16;
            return (state & 0x00ffffffu) / 16777215f;
        }

        private static float RandomRange(float minimum, float maximum, uint hash)
        {
            return Mathf.Lerp(minimum, maximum, Hash01(hash));
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        }
    }
}
