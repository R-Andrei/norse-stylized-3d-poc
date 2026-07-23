using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace ProgrammaticStylized3D.Weather
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather Cloud Shadow Controller")]
    public sealed class WeatherCloudShadowController : MonoBehaviour
    {
        private const float DirectionEpsilonSquared = 0.000001f;
        private const string DebugOverlayShaderName =
            "Hidden/PS3D/Weather Cloud Shadow Debug Overlay";

        private static readonly int DebugModeId =
            Shader.PropertyToID("_CloudDebugMode");
        private static readonly int DebugOpacityId =
            Shader.PropertyToID("_CloudDebugOpacity");
        private static readonly int DebugCloudColorId =
            Shader.PropertyToID("_CloudDebugCloudColor");
        private static readonly int DebugOpeningColorId =
            Shader.PropertyToID("_CloudDebugOpeningColor");
        private static readonly int DebugShadedTransmissionId =
            Shader.PropertyToID("_CloudDebugShadedTransmission");

        public enum CloudDebugVisualization
        {
            Off = 0,
            CloudAreas = 1,
            CloudAndOpenings = 2
        }

        public enum CookieEvolutionState
        {
            Idle = 0,
            Blending = 1
        }

        public enum DebugFocusSource
        {
            RuntimeOverride = 0,
            InspectorOverride = 1,
            AssignedFallbackCamera = 2,
            AutomaticMainCamera = 3,
            ControllerFallback = 4
        }

        internal readonly struct BenchmarkState
        {
            internal readonly bool CloudShadowsEnabled;
            internal readonly bool CookieEvolutionEnabled;
            internal readonly float MovementSpeedMetresPerSecond;
            internal readonly CloudDebugVisualization DebugVisualization;
            internal readonly int Seed;
            internal readonly Vector2 WorldPhaseXZ;
            internal readonly int EvolutionSequence;
            internal readonly double SecondsUntilNextEvolution;

            internal BenchmarkState(
                bool cloudShadowsEnabled,
                bool cookieEvolutionEnabled,
                float movementSpeedMetresPerSecond,
                CloudDebugVisualization debugVisualization,
                int seed,
                Vector2 worldPhaseXZ,
                int evolutionSequence,
                double secondsUntilNextEvolution)
            {
                CloudShadowsEnabled = cloudShadowsEnabled;
                CookieEvolutionEnabled = cookieEvolutionEnabled;
                MovementSpeedMetresPerSecond = movementSpeedMetresPerSecond;
                DebugVisualization = debugVisualization;
                Seed = seed;
                WorldPhaseXZ = worldPhaseXZ;
                EvolutionSequence = evolutionSequence;
                SecondsUntilNextEvolution = secondsUntilNextEvolution;
            }
        }

        private static readonly List<WeatherCloudShadowController>
            ActiveControllersInternal =
                new List<WeatherCloudShadowController>();

        [Header("Activation")]
        [SerializeField]
        private bool cloudShadowsEnabled = true;

        [SerializeField]
        private bool previewInEditMode = true;

        [SerializeField]
        [Tooltip("Optional explicit sun. When unassigned, RenderSettings.sun is used.")]
        private Light sunOverride;

        [Header("Cookie Pattern")]
        [SerializeField]
        private int seed = 7331;

        [SerializeField, Range(64, 1024)]
        private int cookieResolution = 256;

        [SerializeField, Min(16f)]
        [Tooltip("Per-axis world-space repeat period of the directional cookie. The generated directional cookie tiles across the world; this is not a finite coverage boundary.")]
        private float cookieWorldSizeMetres = 128f;

        [SerializeField, Range(0.05f, 0.95f)]
        private float cloudCoverage = 0.68f;

        [SerializeField, Min(5f)]
        private float primaryFeatureScaleMetres = 18f;

        [SerializeField, Min(3f)]
        private float secondaryFeatureScaleMetres = 9f;

        [SerializeField, Range(0f, 1f)]
        private float secondaryShapeWeight = 0.35f;

        [SerializeField, Min(0.1f)]
        private float transitionSoftnessMetres = 1.5f;

        [SerializeField, Min(0f)]
        [Tooltip("Dirty-time cleanup removes isolated sun openings whose midpoint-area is smaller than this approximate diameter.")]
        private float minimumOpeningDiameterMetres = 5f;

        [SerializeField, Range(0.05f, 1f)]
        private float shadedTransmission = 0.62f;

        [Header("Cookie Evolution")]
        [SerializeField]
        [Tooltip("Automatically prepares a new deterministic seed at randomized intervals in Play Mode, then crossfades the existing cookie at a bounded upload cadence.")]
        private bool cookieEvolutionEnabled = true;

        [SerializeField, Min(5f)]
        private float minimumEvolutionIntervalSeconds = 90f;

        [SerializeField, Min(5f)]
        private float maximumEvolutionIntervalSeconds = 180f;

        [SerializeField, Min(0.25f)]
        private float evolutionDurationSeconds = 10f;

        [SerializeField, Range(1f, 30f)]
        private float evolutionUpdateRateHz = 6f;

        [Header("Movement")]
        [SerializeField, Min(0f)]
        private float movementSpeedMetresPerSecond = 1.25f;

        [SerializeField, Range(-60f, 60f)]
        private float windAngleOffsetDegrees;

        [SerializeField, Range(1f, 30f)]
        private float windSampleRateHz = 8f;

        [SerializeField]
        private Vector2 fallbackDirection = new Vector2(1f, 0.25f);

        [Header("Debug Overlay Focus")]
        [SerializeField]
        [FormerlySerializedAs("coverageFocusOverride")]
        [Tooltip("Optional persistent Transform used only to position the finite cloud debug overlay. It does not limit or move the globally tiled directional-cookie field.")]
        private Transform debugFocusOverride;

        [SerializeField]
        [FormerlySerializedAs("fallbackCoverageCamera")]
        [Tooltip("Optional camera used only when no runtime or Inspector debug-focus override is active. When unassigned, Camera.main is resolved automatically.")]
        private Camera debugFallbackCamera;

        [Header("Sun Gate")]
        [SerializeField, Min(0f)]
        private float minimumSunIntensity = 0.01f;

        [SerializeField, Range(-0.25f, 0.5f)]
        [Tooltip("Minimum dot product between the direction toward the sun and world up.")]
        private float minimumSunElevation = 0.01f;

        [Header("Debug Visualization")]
        [SerializeField]
        [Tooltip("Draws an unlit world overlay that samples the exact active directional cookie. This does not change cloud generation or receiver lighting.")]
        private CloudDebugVisualization debugVisualization =
            CloudDebugVisualization.CloudAndOpenings;

        [SerializeField]
        [FormerlySerializedAs("debugFollowCoverageFocus")]
        [Tooltip("When enabled, the diagnostic overlay is centred on the resolved debug focus. This affects only visualization; the directional cookie itself tiles globally.")]
        private bool debugFollowResolvedFocus = true;

        [SerializeField]
        [Tooltip("When enabled, the diagnostic overlay spans one complete cookie repeat period.")]
        private bool debugMatchCookieWorldSize = true;

        [SerializeField]
        [Tooltip("Optional manual overlay centre used only when Debug Follow Resolved Focus is disabled.")]
        private Transform debugOverlayAnchor;

        [SerializeField, Min(1f)]
        private float debugOverlaySizeMetres = 64f;

        [SerializeField]
        [Tooltip("World-space Y coordinate of the horizontal debug sample plane.")]
        private float debugSampleHeightMetres;

        [SerializeField, Range(0.05f, 1f)]
        private float debugOverlayOpacity = 0.55f;

        [SerializeField]
        private Color debugCloudColor = new Color(1f, 0f, 0.75f, 1f);

        [SerializeField]
        private Color debugOpeningColor = new Color(0f, 0.85f, 1f, 1f);

        private Texture2D generatedCookie;
        private byte[] currentCookiePixels;
        private byte[] nextCookiePixels;
        private byte[] blendedCookiePixels;
        private WeatherCloudShadowCookieGenerator.Workspace generationWorkspace;
        private CookieEvolutionState evolutionState;
        private int currentCookieSeed;
        private int nextEvolutionSeed;
        private int evolutionSequence;
        private float evolutionProgress;
        private double evolutionStartRealtime;
        private double nextEvolutionBlendUpdateRealtime;
        private double nextAutomaticEvolutionRealtime = double.PositiveInfinity;
        private bool evolutionScheduleDirty = true;
        private int evolutionUploadCount;
        private long evolutionUploadedTexelBytes;
        private double lastEvolutionPreparationMilliseconds;
        private double evolutionBlendUploadTotalMilliseconds;
        private double evolutionBlendUploadMaximumMilliseconds;
        private int evolutionBlendUploadTimingCount;
        private string lastEvolutionError = string.Empty;
        private Light capturedSun;
        private UniversalAdditionalLightData capturedAdditionalLightData;
        private Texture originalCookie;
        private Vector2 originalCookieSize;
        private Vector2 originalCookieOffset;
        private bool originalSunStateCaptured;
        private bool cookieDirty = true;
        private int lastGenerationHash;
        private bool generationHashInitialized;
        private string lastSunError = string.Empty;
        private string lastGenerationError = string.Empty;
        private Vector2 resolvedWindDirection = Vector2.right;
        private Vector2 worldPhaseXZ;
        private double lastRealtime;
        private double nextWindSampleRealtime;
        private bool sunGateActive;
        private Vector2 appliedCookieOffset;
        private Transform runtimeDebugFocusOverride;
        private Transform resolvedDebugFocus;
        private DebugFocusSource resolvedDebugFocusSource;
        private Vector3 resolvedDebugFocusPosition;
        private Camera cachedMainCamera;
        private Mesh debugOverlayMesh;
        private Material debugOverlayMaterial;
        private MaterialPropertyBlock debugOverlayProperties;
        private string lastDebugError = string.Empty;

        public static int ActiveControllerCount =>
            ActiveControllersInternal.Count;
        public static WeatherCloudShadowController PublishedController
        {
            get;
            private set;
        }

        public bool CloudShadowsEnabled => cloudShadowsEnabled;
        public bool PreviewInEditMode => previewInEditMode;
        public bool IsPublished => PublishedController == this;
        public bool CookieReady => generatedCookie != null;
        public Texture2D GeneratedCookie => generatedCookie;
        public Light ResolvedSun => ResolveSun();
        public Vector2 ResolvedWindDirection => resolvedWindDirection;
        public Vector2 CurrentCookieOffset => appliedCookieOffset;
        public float CookieWorldSizeMetres => cookieWorldSizeMetres;
        public int CookieResolution => cookieResolution;
        public long EstimatedCookieTexelBytes =>
            (long)cookieResolution * cookieResolution;
        public bool CookieEvolutionEnabled => cookieEvolutionEnabled;
        public CookieEvolutionState EvolutionState => evolutionState;
        public bool EvolutionInProgress =>
            evolutionState == CookieEvolutionState.Blending;
        public float EvolutionProgress => evolutionProgress;
        public int CurrentCookieSeed => currentCookieSeed;
        public int NextEvolutionSeed => nextEvolutionSeed;
        public double SecondsUntilNextEvolution
        {
            get
            {
                if (double.IsPositiveInfinity(nextAutomaticEvolutionRealtime))
                {
                    return double.PositiveInfinity;
                }

                return Math.Max(
                    0.0,
                    nextAutomaticEvolutionRealtime -
                    Time.realtimeSinceStartupAsDouble);
            }
        }
        public int EvolutionUploadCount => evolutionUploadCount;
        public long EvolutionUploadedTexelBytes =>
            evolutionUploadedTexelBytes;
        public double LastEvolutionPreparationMilliseconds =>
            lastEvolutionPreparationMilliseconds;
        public double EvolutionBlendUploadTotalMilliseconds =>
            evolutionBlendUploadTotalMilliseconds;
        public double EvolutionBlendUploadMaximumMilliseconds =>
            evolutionBlendUploadMaximumMilliseconds;
        public int EvolutionBlendUploadTimingCount =>
            evolutionBlendUploadTimingCount;
        public long EstimatedEvolutionUploadBytesPerTransition
        {
            get
            {
                long pixelCount = EstimatedCookieTexelBytes;
                int uploadCount = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        evolutionDurationSeconds *
                        evolutionUpdateRateHz));
                return pixelCount * uploadCount;
            }
        }
        public string LastEvolutionError => lastEvolutionError;
        public bool SunGateActive => sunGateActive;
        public CloudDebugVisualization DebugVisualization =>
            debugVisualization;
        public float DebugOverlaySizeMetres => debugOverlaySizeMetres;
        public float EffectiveDebugOverlaySizeMetres =>
            debugMatchCookieWorldSize
                ? cookieWorldSizeMetres
                : debugOverlaySizeMetres;
        public float DebugSampleHeightMetres => debugSampleHeightMetres;
        public bool DebugFollowsResolvedFocus => debugFollowResolvedFocus;
        public bool DebugMatchesCookieWorldSize => debugMatchCookieWorldSize;
        public Transform InspectorDebugFocusOverride => debugFocusOverride;
        public Camera DebugFallbackCamera => debugFallbackCamera;
        public Transform RuntimeDebugFocusOverride =>
            runtimeDebugFocusOverride;
        public Transform ResolvedDebugFocus => resolvedDebugFocus;
        public DebugFocusSource ResolvedDebugFocusSource =>
            resolvedDebugFocusSource;
        public Vector3 ResolvedDebugFocusPosition =>
            resolvedDebugFocusPosition;
        public string LastDebugError => lastDebugError;
        public string LastError =>
            !string.IsNullOrEmpty(lastSunError)
                ? lastSunError
                : lastGenerationError;

        private void OnEnable()
        {
            if (!ActiveControllersInternal.Contains(this))
            {
                ActiveControllersInternal.Add(this);
            }

            if (PublishedController != null &&
                PublishedController != this)
            {
                PublishedController.RestoreCapturedSunState();
            }

            PublishedController = this;
            lastGenerationHash = ComputeGenerationHash();
            generationHashInitialized = true;
            cookieDirty = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            nextWindSampleRealtime = 0.0;
            TickController(true);
        }

        private void OnDisable()
        {
            DeactivateController();
        }

        private void OnDestroy()
        {
            DeactivateController();
        }

        private void OnValidate()
        {
            cookieResolution = Mathf.Clamp(
                Mathf.ClosestPowerOfTwo(cookieResolution),
                64,
                1024);
            cookieWorldSizeMetres = Mathf.Max(16f, cookieWorldSizeMetres);
            cloudCoverage = Mathf.Clamp(cloudCoverage, 0.05f, 0.95f);
            primaryFeatureScaleMetres = Mathf.Clamp(
                primaryFeatureScaleMetres,
                5f,
                cookieWorldSizeMetres * 0.5f);
            secondaryFeatureScaleMetres = Mathf.Clamp(
                secondaryFeatureScaleMetres,
                3f,
                primaryFeatureScaleMetres);
            secondaryShapeWeight = Mathf.Clamp01(secondaryShapeWeight);
            transitionSoftnessMetres = Mathf.Clamp(
                transitionSoftnessMetres,
                0.1f,
                primaryFeatureScaleMetres * 0.5f);
            minimumOpeningDiameterMetres = Mathf.Clamp(
                minimumOpeningDiameterMetres,
                0f,
                primaryFeatureScaleMetres);
            shadedTransmission = Mathf.Clamp(
                shadedTransmission,
                0.05f,
                1f);
            minimumEvolutionIntervalSeconds = Mathf.Max(
                5f,
                minimumEvolutionIntervalSeconds);
            maximumEvolutionIntervalSeconds = Mathf.Max(
                minimumEvolutionIntervalSeconds,
                maximumEvolutionIntervalSeconds);
            evolutionDurationSeconds = Mathf.Max(
                0.25f,
                evolutionDurationSeconds);
            evolutionUpdateRateHz = Mathf.Clamp(
                evolutionUpdateRateHz,
                1f,
                30f);
            evolutionScheduleDirty = true;
            movementSpeedMetresPerSecond = Mathf.Max(
                0f,
                movementSpeedMetresPerSecond);
            windSampleRateHz = Mathf.Clamp(windSampleRateHz, 1f, 30f);
            minimumSunIntensity = Mathf.Max(0f, minimumSunIntensity);
            minimumSunElevation = Mathf.Clamp(
                minimumSunElevation,
                -0.25f,
                0.5f);
            debugOverlaySizeMetres = Mathf.Max(
                1f,
                debugOverlaySizeMetres);
            debugOverlayOpacity = Mathf.Clamp(
                debugOverlayOpacity,
                0.05f,
                1f);

            if (fallbackDirection.sqrMagnitude < DirectionEpsilonSquared)
            {
                fallbackDirection = Vector2.right;
            }

            int generationHash = ComputeGenerationHash();
            if (!generationHashInitialized)
            {
                lastGenerationHash = generationHash;
                generationHashInitialized = true;
                cookieDirty = true;
            }
            else if (generationHash != lastGenerationHash)
            {
                lastGenerationHash = generationHash;
                cookieDirty = true;
            }
        }

        private void Update()
        {
            TickController(false);
        }

        public void RequestCookieRebuild()
        {
            cookieDirty = true;
            TickController(true);
        }

        public void RebuildCookieNow()
        {
            cookieDirty = true;
            EnsureCookie();
            TickController(true);
        }

        public void ResetCloudMotion()
        {
            worldPhaseXZ = Vector2.zero;
            nextWindSampleRealtime = 0.0;
            TickController(true);
        }

        public void EvolveCookieNow()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            EnsureCookie();
            if (generatedCookie == null || EvolutionInProgress)
            {
                return;
            }

            BeginCookieEvolution(now);
            TickController(true);
        }

        public void CompleteEvolutionImmediately()
        {
            double now = Time.realtimeSinceStartupAsDouble;
            EnsureCookie();
            if (generatedCookie == null)
            {
                return;
            }

            if (!EvolutionInProgress && !BeginCookieEvolution(now))
            {
                return;
            }

            CompleteCookieEvolution(now, false);
            TickController(true);
        }

        public void RefreshNow()
        {
            TickController(true);
        }

        public void SetDebugFocusOverride(
            Transform debugFocus,
            bool refreshImmediately = true)
        {
            runtimeDebugFocusOverride = debugFocus;
            cachedMainCamera = null;
            UpdateResolvedDebugFocus();
            if (refreshImmediately)
            {
                TickController(true);
            }
        }

        public void ClearDebugFocusOverride(
            Transform expectedDebugFocus = null,
            bool refreshImmediately = true)
        {
            if (expectedDebugFocus != null &&
                runtimeDebugFocusOverride != expectedDebugFocus)
            {
                return;
            }

            runtimeDebugFocusOverride = null;
            cachedMainCamera = null;
            UpdateResolvedDebugFocus();
            if (refreshImmediately)
            {
                TickController(true);
            }
        }

        public void RefreshDebugFocusNow()
        {
            cachedMainCamera = null;
            UpdateResolvedDebugFocus();
            TickController(true);
        }

        public void SetDebugVisualization(
            CloudDebugVisualization visualization)
        {
            debugVisualization = visualization;
            TickController(true);
        }

        public void EditorTick()
        {
            if (!Application.isPlaying && previewInEditMode)
            {
                TickController(false);
            }
        }

        internal bool CanRunPerformanceBenchmark(out string reason)
        {
            reason = string.Empty;
            if (!Application.isPlaying)
            {
                reason = "Cloud-shadow performance benchmarking requires Play Mode.";
                return false;
            }

            if (!isActiveAndEnabled || !IsPublished)
            {
                reason = "The active published Weather Cloud Shadow Controller is required.";
                return false;
            }

            if (!CookieReady || ResolvedSun == null || !SunGateActive)
            {
                reason = "The controller, generated cookie, and active directional-sun gate must all be ready.";
                return false;
            }

            if (EvolutionInProgress)
            {
                reason = "Wait for the current cookie evolution to complete before starting the benchmark.";
                return false;
            }

            return true;
        }

        internal BenchmarkState CapturePerformanceBenchmarkState()
        {
            return new BenchmarkState(
                cloudShadowsEnabled,
                cookieEvolutionEnabled,
                movementSpeedMetresPerSecond,
                debugVisualization,
                seed,
                worldPhaseXZ,
                evolutionSequence,
                SecondsUntilNextEvolution);
        }

        internal void ApplyPerformanceBenchmarkCase(
            bool enableCloudShadows,
            float movementSpeed)
        {
            if (EvolutionInProgress)
            {
                ResetCookieEvolutionState(true);
            }

            cloudShadowsEnabled = enableCloudShadows;
            movementSpeedMetresPerSecond = Mathf.Max(0f, movementSpeed);
            cookieEvolutionEnabled = false;
            debugVisualization = CloudDebugVisualization.Off;
            nextAutomaticEvolutionRealtime = double.PositiveInfinity;
            evolutionScheduleDirty = true;
            lastRealtime = Time.realtimeSinceStartupAsDouble;
            TickController(true);
        }

        internal bool BeginPerformanceBenchmarkEvolution()
        {
            if (EvolutionInProgress)
            {
                return false;
            }

            cookieEvolutionEnabled = false;
            nextAutomaticEvolutionRealtime = double.PositiveInfinity;
            evolutionScheduleDirty = true;
            bool started = BeginCookieEvolution(
                Time.realtimeSinceStartupAsDouble);
            TickController(true);
            return started;
        }

        internal void RestorePerformanceBenchmarkState(
            BenchmarkState state)
        {
            if (EvolutionInProgress)
            {
                ResetCookieEvolutionState(true);
            }

            cloudShadowsEnabled = state.CloudShadowsEnabled;
            cookieEvolutionEnabled = state.CookieEvolutionEnabled;
            movementSpeedMetresPerSecond =
                state.MovementSpeedMetresPerSecond;
            debugVisualization = state.DebugVisualization;
            worldPhaseXZ = state.WorldPhaseXZ;
            seed = state.Seed;
            cookieDirty = true;
            EnsureCookie();
            evolutionSequence = state.EvolutionSequence;

            double now = Time.realtimeSinceStartupAsDouble;
            if (cookieEvolutionEnabled &&
                !double.IsPositiveInfinity(
                    state.SecondsUntilNextEvolution))
            {
                nextAutomaticEvolutionRealtime = now + Math.Max(
                    0.0,
                    state.SecondsUntilNextEvolution);
                evolutionScheduleDirty = false;
            }
            else
            {
                nextAutomaticEvolutionRealtime = double.PositiveInfinity;
                evolutionScheduleDirty = true;
            }

            lastRealtime = now;
            nextWindSampleRealtime = 0.0;
            TickController(true);
        }

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(2048);
            Light sun = ResolveSun();
            string error = LastError;
            builder.AppendLine("[Weather Cloud-Shadow Directional-Cookie Report]");
            builder.Append("Status: ")
                .AppendLine(string.IsNullOrEmpty(error) ? "READY" : "NOT READY");
            builder.Append("Published controller: ")
                .AppendLine(IsPublished ? "Yes" : "No");
            builder.Append("Active controllers: ")
                .AppendLine(ActiveControllerCount.ToString());
            builder.Append("Cloud shadows enabled: ")
                .AppendLine(cloudShadowsEnabled ? "Yes" : "No");
            builder.Append("Edit-mode preview: ")
                .AppendLine(previewInEditMode ? "Yes" : "No");
            builder.Append("Resolved sun: ")
                .AppendLine(sun != null ? sun.name : "None");
            builder.Append("Sun gate active: ")
                .AppendLine(sunGateActive ? "Yes" : "No");
            builder.Append("Cookie ready / assigned: ")
                .Append(CookieReady ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(
                    sun != null && sun.cookie == generatedCookie
                        ? "Yes"
                        : "No");
            builder.Append("Cookie resolution: ")
                .Append(cookieResolution).Append(" × ")
                .AppendLine(cookieResolution.ToString());
            builder.Append("Estimated R8 texel bytes: ")
                .AppendLine(EstimatedCookieTexelBytes.ToString("N0"));
            builder.Append("Cookie repeat period: ")
                .Append(cookieWorldSizeMetres.ToString("0.###"))
                .AppendLine(" m per axis (globally tiled)");
            builder.Append("Debug focus: ")
                .Append(resolvedDebugFocus != null
                    ? resolvedDebugFocus.name
                    : "None")
                .Append(" | source: ")
                .AppendLine(resolvedDebugFocusSource.ToString());
            builder.Append("Debug focus position: ")
                .AppendLine(resolvedDebugFocusPosition.ToString("F3"));
            builder.Append("Runtime debug focus override: ")
                .AppendLine(runtimeDebugFocusOverride != null
                    ? runtimeDebugFocusOverride.name
                    : "None");
            builder.Append("Inspector debug focus override: ")
                .AppendLine(debugFocusOverride != null
                    ? debugFocusOverride.name
                    : "None");
            builder.Append("Debug fallback camera: ")
                .AppendLine(debugFallbackCamera != null
                    ? debugFallbackCamera.name
                    : "None (Camera.main automatic fallback)");
            builder.Append("Coverage / shaded transmission: ")
                .Append(cloudCoverage.ToString("0.###"))
                .Append(" / ")
                .AppendLine(shadedTransmission.ToString("0.###"));
            builder.Append("Primary / secondary feature scale: ")
                .Append(primaryFeatureScaleMetres.ToString("0.###"))
                .Append(" / ")
                .Append(secondaryFeatureScaleMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Secondary shape weight: ")
                .AppendLine(secondaryShapeWeight.ToString("0.###"));
            builder.Append("Transition softness: ")
                .Append(transitionSoftnessMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Minimum opening diameter cleanup: ")
                .Append(minimumOpeningDiameterMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Cookie evolution enabled / state: ")
                .Append(cookieEvolutionEnabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(evolutionState.ToString());
            builder.Append("Current / next evolution seed: ")
                .Append(currentCookieSeed)
                .Append(" / ")
                .AppendLine(
                    EvolutionInProgress
                        ? nextEvolutionSeed.ToString()
                        : "None");
            builder.Append("Evolution interval range: ")
                .Append(minimumEvolutionIntervalSeconds.ToString("0.###"))
                .Append("–")
                .Append(maximumEvolutionIntervalSeconds.ToString("0.###"))
                .AppendLine(" s");
            builder.Append("Evolution duration / update rate: ")
                .Append(evolutionDurationSeconds.ToString("0.###"))
                .Append(" s / ")
                .Append(evolutionUpdateRateHz.ToString("0.###"))
                .AppendLine(" Hz");
            builder.Append("Evolution progress: ")
                .AppendLine(evolutionProgress.ToString("P1"));
            builder.Append("Seconds until next automatic evolution: ")
                .AppendLine(
                    double.IsPositiveInfinity(SecondsUntilNextEvolution)
                        ? "Inactive"
                        : SecondsUntilNextEvolution.ToString("0.###"));
            builder.Append("Evolution uploads / raw texel bytes this transition: ")
                .Append(evolutionUploadCount)
                .Append(" / ")
                .AppendLine(evolutionUploadedTexelBytes.ToString("N0"));
            builder.Append("Evolution preparation CPU time: ")
                .Append(lastEvolutionPreparationMilliseconds.ToString("0.###"))
                .AppendLine(" ms");
            builder.Append("Evolution blend/upload CPU total / maximum: ")
                .Append(evolutionBlendUploadTotalMilliseconds.ToString("0.###"))
                .Append(" / ")
                .Append(evolutionBlendUploadMaximumMilliseconds.ToString("0.###"))
                .Append(" ms across ")
                .Append(evolutionBlendUploadTimingCount)
                .AppendLine(" timed updates");
            builder.Append("Estimated raw texel upload bytes per configured transition: ")
                .AppendLine(
                    EstimatedEvolutionUploadBytesPerTransition.ToString("N0"));
            builder.Append("Movement speed: ")
                .Append(movementSpeedMetresPerSecond.ToString("0.###"))
                .AppendLine(" m/s");
            builder.Append("Wind sample rate: ")
                .Append(windSampleRateHz.ToString("0.###"))
                .AppendLine(" Hz");
            builder.Append("Resolved movement direction XZ: ")
                .AppendLine(resolvedWindDirection.ToString("F3"));
            builder.Append("World movement phase XZ: ")
                .AppendLine(worldPhaseXZ.ToString("F3"));
            builder.Append("Applied URP cookie offset: ")
                .AppendLine(appliedCookieOffset.ToString("F3"));
            builder.Append("Debug visualization: ")
                .AppendLine(debugVisualization.ToString());
            builder.Append("Debug follows resolved focus / matches cookie period: ")
                .Append(debugFollowResolvedFocus ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(debugMatchCookieWorldSize ? "Yes" : "No");
            builder.Append("Debug overlay size / sample height: ")
                .Append(EffectiveDebugOverlaySizeMetres.ToString("0.###"))
                .Append(" m / ")
                .Append(debugSampleHeightMetres.ToString("0.###"))
                .AppendLine(" m");
            builder.Append("Debug overlay state: ")
                .AppendLine(
                    string.IsNullOrEmpty(lastDebugError)
                        ? "Ready"
                        : "Unavailable");
            builder.Append("World coverage model: ")
                .AppendLine("Directional cookie repeats globally; no per-player, per-camera, per-chunk, or whole-map cloud simulation");
            builder.Append("Steady-state work: ")
                .AppendLine("O(1) movement integration, bounded wind sampling, and an evolution timer/state check; no texture rebuild or coverage recenter while idle");
            builder.Append("Dirty/evolution work: ")
                .AppendLine("O(R^2) next-seed generation once, then O(R^2) byte blend plus one R8 upload at the configured bounded cadence during an active transition");

            if (!string.IsNullOrEmpty(error))
            {
                builder.AppendLine("Error:");
                builder.AppendLine(error);
            }

            if (!string.IsNullOrEmpty(lastDebugError))
            {
                builder.AppendLine("Debug visualization error:");
                builder.AppendLine(lastDebugError);
            }

            if (!string.IsNullOrEmpty(lastEvolutionError))
            {
                builder.AppendLine("Cookie evolution error:");
                builder.AppendLine(lastEvolutionError);
            }

            return builder.ToString();
        }

        private void TickController(bool force)
        {
            UpdateResolvedDebugFocus();

            if (PublishedController != this || !isActiveAndEnabled)
            {
                return;
            }

            bool shouldPreview = Application.isPlaying || previewInEditMode;
            if (!shouldPreview)
            {
                RestoreCapturedSunState();
                sunGateActive = false;
                lastDebugError = string.Empty;
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float deltaTime = force
                ? 0f
                : (float)Math.Max(0.0, Math.Min(0.25, now - lastRealtime));
            lastRealtime = now;

            ResolveAndCaptureSun();
            if (capturedSun == null || capturedAdditionalLightData == null)
            {
                sunGateActive = false;
                return;
            }

            EnsureCookie();
            if (generatedCookie == null)
            {
                sunGateActive = false;
                RestoreCapturedSunState();
                return;
            }

            UpdateCookieEvolution(now);
            UpdateResolvedWind(now);
            worldPhaseXZ +=
                resolvedWindDirection *
                (movementSpeedMetresPerSecond * deltaTime);

            sunGateActive = EvaluateSunGate(capturedSun);
            if (!sunGateActive)
            {
                ApplyOriginalSunStateWithoutRelease();
                lastDebugError = debugVisualization ==
                    CloudDebugVisualization.Off
                    ? string.Empty
                    : "The debug overlay requires the directional-cookie sun gate to be active.";
                return;
            }

            ApplyCookieToCapturedSun();
            DrawDebugOverlay();
        }

        private void ResolveAndCaptureSun()
        {
            Light resolvedSun = ResolveSun();
            if (resolvedSun == capturedSun &&
                originalSunStateCaptured &&
                capturedSun != null &&
                capturedSun.type == LightType.Directional &&
                capturedAdditionalLightData != null &&
                capturedSun.TryGetComponent(
                    out UniversalAdditionalLightData currentAdditionalData) &&
                currentAdditionalData == capturedAdditionalLightData)
            {
                return;
            }

            RestoreCapturedSunState();
            capturedSun = resolvedSun;
            capturedAdditionalLightData = null;
            originalSunStateCaptured = false;
            lastSunError = string.Empty;

            if (capturedSun == null)
            {
                lastSunError = "No authoritative sun is assigned through the override or RenderSettings.sun.";
                return;
            }

            if (capturedSun.type != LightType.Directional)
            {
                lastSunError = "The resolved Weather cloud-shadow sun is not directional.";
                return;
            }

            if (!capturedSun.TryGetComponent(out capturedAdditionalLightData))
            {
                lastSunError = "The resolved directional sun has no UniversalAdditionalLightData component.";
                return;
            }

            originalCookie = capturedSun.cookie;
            originalCookieSize = capturedAdditionalLightData.lightCookieSize;
            originalCookieOffset = capturedAdditionalLightData.lightCookieOffset;
            appliedCookieOffset = originalCookieOffset;
            originalSunStateCaptured = true;
        }

        private Light ResolveSun()
        {
            return sunOverride != null ? sunOverride : RenderSettings.sun;
        }

        private void EnsureCookie()
        {
            if (!cookieDirty)
            {
                return;
            }

            Texture2D previousCookie = generatedCookie;
            if (EvolutionInProgress &&
                previousCookie != null &&
                currentCookiePixels != null)
            {
                WeatherCloudShadowCookieGenerator.UploadPixels(
                    previousCookie,
                    currentCookiePixels);
            }

            ResetCookieEvolutionState(false);

            try
            {
                WeatherCloudShadowCookieGenerator.Settings settings =
                    BuildGeneratorSettings(seed);
                int resolvedResolution =
                    WeatherCloudShadowCookieGenerator.ResolveResolution(
                        settings.Resolution);
                int pixelCount = resolvedResolution * resolvedResolution;
                EnsureCookieBuffers(pixelCount);
                if (generationWorkspace == null)
                {
                    generationWorkspace =
                        new WeatherCloudShadowCookieGenerator.Workspace();
                }

                WeatherCloudShadowCookieGenerator.GeneratePixels(
                    settings,
                    blendedCookiePixels,
                    generationWorkspace);

                bool requiresNewTexture =
                    previousCookie == null ||
                    previousCookie.width != resolvedResolution ||
                    previousCookie.height != resolvedResolution ||
                    previousCookie.format != TextureFormat.R8;
                if (requiresNewTexture)
                {
                    generatedCookie =
                        WeatherCloudShadowCookieGenerator.CreateTexture(
                            settings,
                            blendedCookiePixels);
                    DestroyGeneratedTexture(previousCookie);
                }
                else
                {
                    WeatherCloudShadowCookieGenerator.UploadPixels(
                        previousCookie,
                        blendedCookiePixels);
                    WeatherCloudShadowCookieGenerator.SetTextureSeedName(
                        previousCookie,
                        seed);
                    generatedCookie = previousCookie;
                }

                SwapPixelBuffers(
                    ref currentCookiePixels,
                    ref blendedCookiePixels);
                currentCookieSeed = seed;
                cookieDirty = false;
                lastGenerationError = string.Empty;
                lastEvolutionError = string.Empty;
                lastGenerationHash = ComputeGenerationHash();
                generationHashInitialized = true;
                evolutionSequence = 0;
                ScheduleNextAutomaticEvolution(
                    Time.realtimeSinceStartupAsDouble);
            }
            catch (Exception exception)
            {
                generatedCookie = previousCookie;
                cookieDirty = false;
                lastGenerationError = exception.ToString();
                nextAutomaticEvolutionRealtime = double.PositiveInfinity;
            }
        }

        private WeatherCloudShadowCookieGenerator.Settings
            BuildGeneratorSettings(int settingsSeed)
        {
            return new WeatherCloudShadowCookieGenerator.Settings(
                cookieResolution,
                settingsSeed,
                cookieWorldSizeMetres,
                cloudCoverage,
                primaryFeatureScaleMetres,
                secondaryFeatureScaleMetres,
                secondaryShapeWeight,
                transitionSoftnessMetres,
                minimumOpeningDiameterMetres,
                shadedTransmission);
        }

        private void EnsureCookieBuffers(int pixelCount)
        {
            if (currentCookiePixels == null ||
                currentCookiePixels.Length != pixelCount)
            {
                currentCookiePixels = new byte[pixelCount];
            }

            if (nextCookiePixels == null ||
                nextCookiePixels.Length != pixelCount)
            {
                nextCookiePixels = new byte[pixelCount];
            }

            if (blendedCookiePixels == null ||
                blendedCookiePixels.Length != pixelCount)
            {
                blendedCookiePixels = new byte[pixelCount];
            }
        }

        private void UpdateCookieEvolution(double now)
        {
            if (EvolutionInProgress)
            {
                UpdateActiveCookieEvolution(now);
                return;
            }

            if (!Application.isPlaying || !cookieEvolutionEnabled)
            {
                nextAutomaticEvolutionRealtime = double.PositiveInfinity;
                evolutionScheduleDirty = true;
                return;
            }

            if (evolutionScheduleDirty ||
                double.IsPositiveInfinity(nextAutomaticEvolutionRealtime))
            {
                ScheduleNextAutomaticEvolution(now);
            }

            if (now >= nextAutomaticEvolutionRealtime)
            {
                BeginCookieEvolution(now);
            }
        }

        private bool BeginCookieEvolution(double now)
        {
            if (EvolutionInProgress ||
                generatedCookie == null ||
                currentCookiePixels == null)
            {
                return false;
            }

            lastEvolutionPreparationMilliseconds = 0.0;
            evolutionBlendUploadTotalMilliseconds = 0.0;
            evolutionBlendUploadMaximumMilliseconds = 0.0;
            evolutionBlendUploadTimingCount = 0;
            long preparationStartTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();

            try
            {
                int sequence = evolutionSequence + 1;
                nextEvolutionSeed = ResolveNextEvolutionSeed(
                    currentCookieSeed,
                    sequence);
                WeatherCloudShadowCookieGenerator.Settings settings =
                    BuildGeneratorSettings(nextEvolutionSeed);
                int pixelCount =
                    WeatherCloudShadowCookieGenerator.ResolvePixelCount(
                        settings);
                EnsureCookieBuffers(pixelCount);
                if (generationWorkspace == null)
                {
                    generationWorkspace =
                        new WeatherCloudShadowCookieGenerator.Workspace();
                }

                WeatherCloudShadowCookieGenerator.GeneratePixels(
                    settings,
                    nextCookiePixels,
                    generationWorkspace);
                lastEvolutionPreparationMilliseconds =
                    ResolveElapsedMilliseconds(
                        preparationStartTimestamp);

                evolutionState = CookieEvolutionState.Blending;
                evolutionProgress = 0f;
                evolutionStartRealtime = now;
                nextEvolutionBlendUpdateRealtime = now;
                nextAutomaticEvolutionRealtime = double.PositiveInfinity;
                evolutionUploadCount = 0;
                evolutionUploadedTexelBytes = 0L;
                lastEvolutionError = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                lastEvolutionPreparationMilliseconds =
                    ResolveElapsedMilliseconds(
                        preparationStartTimestamp);
                evolutionState = CookieEvolutionState.Idle;
                evolutionProgress = 0f;
                nextEvolutionSeed = 0;
                lastEvolutionError = exception.ToString();
                ScheduleNextAutomaticEvolution(now);
                return false;
            }
        }

        private void UpdateActiveCookieEvolution(double now)
        {
            float progress = Mathf.Clamp01(
                (float)((now - evolutionStartRealtime) /
                Math.Max(0.25, evolutionDurationSeconds)));
            if (progress < 1f && now < nextEvolutionBlendUpdateRealtime)
            {
                return;
            }

            UploadEvolutionBlend(progress);
            if (progress >= 1f)
            {
                CompleteCookieEvolution(now, true);
                return;
            }

            nextEvolutionBlendUpdateRealtime = now +
                1.0 / Math.Max(1.0, evolutionUpdateRateHz);
        }

        private void UploadEvolutionBlend(float progress)
        {
            if (generatedCookie == null ||
                currentCookiePixels == null ||
                nextCookiePixels == null ||
                blendedCookiePixels == null)
            {
                return;
            }

            long updateStartTimestamp =
                System.Diagnostics.Stopwatch.GetTimestamp();
            float smoothProgress =
                progress * progress * (3f - 2f * progress);
            int pixelCount = currentCookiePixels.Length;
            for (int index = 0; index < pixelCount; index++)
            {
                blendedCookiePixels[index] = (byte)Mathf.RoundToInt(
                    Mathf.Lerp(
                        currentCookiePixels[index],
                        nextCookiePixels[index],
                        smoothProgress));
            }

            WeatherCloudShadowCookieGenerator.UploadPixels(
                generatedCookie,
                blendedCookiePixels);
            evolutionProgress = progress;
            evolutionUploadCount++;
            evolutionUploadedTexelBytes += pixelCount;
            RecordEvolutionBlendUploadTiming(
                ResolveElapsedMilliseconds(updateStartTimestamp));
        }

        private void CompleteCookieEvolution(
            double now,
            bool finalPixelsAlreadyUploaded)
        {
            if (!EvolutionInProgress)
            {
                return;
            }

            if (!finalPixelsAlreadyUploaded)
            {
                long uploadStartTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp();
                WeatherCloudShadowCookieGenerator.UploadPixels(
                    generatedCookie,
                    nextCookiePixels);
                evolutionUploadCount++;
                evolutionUploadedTexelBytes += nextCookiePixels.Length;
                RecordEvolutionBlendUploadTiming(
                    ResolveElapsedMilliseconds(uploadStartTimestamp));
            }

            SwapPixelBuffers(
                ref currentCookiePixels,
                ref nextCookiePixels);
            seed = nextEvolutionSeed;
            currentCookieSeed = seed;
            evolutionSequence++;
            WeatherCloudShadowCookieGenerator.SetTextureSeedName(
                generatedCookie,
                currentCookieSeed);
            lastGenerationHash = ComputeGenerationHash();
            generationHashInitialized = true;
            evolutionState = CookieEvolutionState.Idle;
            evolutionProgress = 0f;
            nextEvolutionSeed = 0;
            evolutionStartRealtime = 0.0;
            nextEvolutionBlendUpdateRealtime = 0.0;
            evolutionScheduleDirty = true;
            ScheduleNextAutomaticEvolution(now);
        }

        private void ResetCookieEvolutionState(bool restoreCurrentPixels)
        {
            if (restoreCurrentPixels &&
                generatedCookie != null &&
                currentCookiePixels != null)
            {
                WeatherCloudShadowCookieGenerator.UploadPixels(
                    generatedCookie,
                    currentCookiePixels);
            }

            evolutionState = CookieEvolutionState.Idle;
            evolutionProgress = 0f;
            nextEvolutionSeed = 0;
            evolutionStartRealtime = 0.0;
            nextEvolutionBlendUpdateRealtime = 0.0;
            nextAutomaticEvolutionRealtime = double.PositiveInfinity;
            evolutionScheduleDirty = true;
            evolutionUploadCount = 0;
            evolutionUploadedTexelBytes = 0L;
            lastEvolutionPreparationMilliseconds = 0.0;
            evolutionBlendUploadTotalMilliseconds = 0.0;
            evolutionBlendUploadMaximumMilliseconds = 0.0;
            evolutionBlendUploadTimingCount = 0;
        }

        private void ScheduleNextAutomaticEvolution(double now)
        {
            evolutionScheduleDirty = false;
            if (!Application.isPlaying ||
                !cookieEvolutionEnabled ||
                generatedCookie == null)
            {
                nextAutomaticEvolutionRealtime = double.PositiveInfinity;
                return;
            }

            float random01 = ResolveEvolutionRandom01(
                currentCookieSeed,
                evolutionSequence,
                0x4F1BBCDCu);
            float interval = Mathf.Lerp(
                minimumEvolutionIntervalSeconds,
                maximumEvolutionIntervalSeconds,
                random01);
            nextAutomaticEvolutionRealtime = now + interval;
        }

        private static int ResolveNextEvolutionSeed(
            int currentSeed,
            int sequence)
        {
            uint value = MixEvolutionHash(
                unchecked((uint)currentSeed) ^
                unchecked((uint)sequence * 0x9E3779B9u) ^
                0xA511E9B3u);
            int resolved = unchecked((int)(value & 0x7FFFFFFFu));
            if (resolved == currentSeed)
            {
                resolved = unchecked(resolved ^ 0x5BD1E995);
            }

            return resolved;
        }

        private static float ResolveEvolutionRandom01(
            int currentSeed,
            int sequence,
            uint salt)
        {
            uint value = MixEvolutionHash(
                unchecked((uint)currentSeed) ^
                unchecked((uint)sequence * 0x85EBCA6Bu) ^
                salt);
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private static uint MixEvolutionHash(uint value)
        {
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return value;
        }

        private static void SwapPixelBuffers(
            ref byte[] first,
            ref byte[] second)
        {
            byte[] temporary = first;
            first = second;
            second = temporary;
        }

        private void RecordEvolutionBlendUploadTiming(
            double elapsedMilliseconds)
        {
            evolutionBlendUploadTotalMilliseconds += elapsedMilliseconds;
            evolutionBlendUploadMaximumMilliseconds = Math.Max(
                evolutionBlendUploadMaximumMilliseconds,
                elapsedMilliseconds);
            evolutionBlendUploadTimingCount++;
        }

        private static double ResolveElapsedMilliseconds(
            long startTimestamp)
        {
            long elapsedTicks =
                System.Diagnostics.Stopwatch.GetTimestamp() -
                startTimestamp;
            return elapsedTicks * 1000.0 /
                System.Diagnostics.Stopwatch.Frequency;
        }

        private void UpdateResolvedWind(double now)
        {
            if (now < nextWindSampleRealtime)
            {
                return;
            }

            nextWindSampleRealtime = now +
                1.0 / Math.Max(1.0, windSampleRateHz);
            Vector3 samplePosition = transform.position;
            WeatherWindDomain windDomain = WeatherWindDomain.PublishedDomain;
            if (windDomain != null && windDomain.FieldAnchor != null)
            {
                samplePosition = windDomain.FieldAnchor.position;
            }

            Vector2 direction;
            if (!WeatherWindDomain.TrySampleWindXZ(
                    samplePosition,
                    out Vector2 sampledWind) ||
                sampledWind.sqrMagnitude < DirectionEpsilonSquared)
            {
                direction = fallbackDirection;
            }
            else
            {
                direction = sampledWind;
            }

            if (direction.sqrMagnitude < DirectionEpsilonSquared)
            {
                direction = Vector2.right;
            }

            direction.Normalize();
            float radians = windAngleOffsetDegrees * Mathf.Deg2Rad;
            float sine = Mathf.Sin(radians);
            float cosine = Mathf.Cos(radians);
            resolvedWindDirection = new Vector2(
                direction.x * cosine - direction.y * sine,
                direction.x * sine + direction.y * cosine);
        }

        private bool EvaluateSunGate(Light sun)
        {
            if (!cloudShadowsEnabled ||
                sun == null ||
                !sun.enabled ||
                !sun.gameObject.activeInHierarchy ||
                sun.intensity < minimumSunIntensity)
            {
                return false;
            }

            float elevation = Vector3.Dot(
                -sun.transform.forward,
                Vector3.up);
            return elevation >= minimumSunElevation;
        }

        private void ApplyCookieToCapturedSun()
        {
            capturedSun.cookie = generatedCookie;
            capturedAdditionalLightData.lightCookieSize =
                Vector2.one * cookieWorldSizeMetres;

            Vector3 displacementWS = new Vector3(
                worldPhaseXZ.x,
                0f,
                worldPhaseXZ.y);
            Vector3 displacementLS =
                capturedSun.transform.InverseTransformVector(displacementWS);
            Vector2 localOffset = new Vector2(
                displacementLS.x,
                displacementLS.y);
            localOffset = new Vector2(
                WrapSigned(localOffset.x, cookieWorldSizeMetres),
                WrapSigned(localOffset.y, cookieWorldSizeMetres));
            appliedCookieOffset = originalCookieOffset + localOffset;
            capturedAdditionalLightData.lightCookieOffset =
                appliedCookieOffset;
        }

        private void ApplyOriginalSunStateWithoutRelease()
        {
            if (!originalSunStateCaptured || capturedSun == null)
            {
                return;
            }

            capturedSun.cookie = originalCookie;
            capturedAdditionalLightData.lightCookieSize = originalCookieSize;
            capturedAdditionalLightData.lightCookieOffset = originalCookieOffset;
            appliedCookieOffset = originalCookieOffset;
        }

        private void RestoreCapturedSunState()
        {
            if (originalSunStateCaptured &&
                capturedSun != null &&
                capturedAdditionalLightData != null)
            {
                capturedSun.cookie = originalCookie;
                capturedAdditionalLightData.lightCookieSize = originalCookieSize;
                capturedAdditionalLightData.lightCookieOffset = originalCookieOffset;
            }

            capturedSun = null;
            capturedAdditionalLightData = null;
            originalCookie = null;
            originalSunStateCaptured = false;
            sunGateActive = false;
        }

        private void DeactivateController()
        {
            ActiveControllersInternal.Remove(this);
            bool wasPublished = PublishedController == this;
            if (wasPublished)
            {
                RestoreCapturedSunState();
                PublishedController = ActiveControllersInternal.Count > 0
                    ? ActiveControllersInternal[
                        ActiveControllersInternal.Count - 1]
                    : null;
            }

            DestroyGeneratedTexture(generatedCookie);
            generatedCookie = null;
            currentCookiePixels = null;
            nextCookiePixels = null;
            blendedCookiePixels = null;
            generationWorkspace = null;
            ResetCookieEvolutionState(false);
            currentCookieSeed = 0;
            evolutionSequence = 0;
            lastEvolutionError = string.Empty;
            runtimeDebugFocusOverride = null;
            resolvedDebugFocus = null;
            cachedMainCamera = null;
            DestroyDebugResources();

            if (wasPublished && PublishedController != null)
            {
                PublishedController.cookieDirty = true;
                PublishedController.lastRealtime =
                    Time.realtimeSinceStartupAsDouble;
                PublishedController.TickController(true);
            }
        }

        private int ComputeGenerationHash()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + seed;
                hash = hash * 31 + cookieResolution;
                hash = hash * 31 + cookieWorldSizeMetres.GetHashCode();
                hash = hash * 31 + cloudCoverage.GetHashCode();
                hash = hash * 31 + primaryFeatureScaleMetres.GetHashCode();
                hash = hash * 31 + secondaryFeatureScaleMetres.GetHashCode();
                hash = hash * 31 + secondaryShapeWeight.GetHashCode();
                hash = hash * 31 + transitionSoftnessMetres.GetHashCode();
                hash = hash * 31 + minimumOpeningDiameterMetres.GetHashCode();
                hash = hash * 31 + shadedTransmission.GetHashCode();
                return hash;
            }
        }

        private static float WrapSigned(float value, float period)
        {
            float safePeriod = Mathf.Max(0.001f, period);
            return Mathf.Repeat(
                value + safePeriod * 0.5f,
                safePeriod) - safePeriod * 0.5f;
        }

        private void DrawDebugOverlay()
        {
            if (debugVisualization == CloudDebugVisualization.Off)
            {
                lastDebugError = string.Empty;
                return;
            }

            if (!EnsureDebugResources())
            {
                return;
            }

            Vector3 centre = ResolveDebugOverlayCentre();
            centre.y = debugSampleHeightMetres;
            float size = Mathf.Max(1f, EffectiveDebugOverlaySizeMetres);
            Matrix4x4 objectToWorld = Matrix4x4.TRS(
                centre,
                Quaternion.identity,
                new Vector3(size, 1f, size));

            debugOverlayProperties.Clear();
            debugOverlayProperties.SetFloat(
                DebugModeId,
                (float)debugVisualization);
            debugOverlayProperties.SetFloat(
                DebugOpacityId,
                debugOverlayOpacity);
            debugOverlayProperties.SetColor(
                DebugCloudColorId,
                debugCloudColor);
            debugOverlayProperties.SetColor(
                DebugOpeningColorId,
                debugOpeningColor);
            debugOverlayProperties.SetFloat(
                DebugShadedTransmissionId,
                shadedTransmission);

            var renderParams = new RenderParams(debugOverlayMaterial)
            {
                camera = null,
                layer = gameObject.layer,
                lightProbeUsage = LightProbeUsage.Off,
                reflectionProbeUsage = ReflectionProbeUsage.Off,
                motionVectorMode = MotionVectorGenerationMode.ForceNoMotion,
                receiveShadows = false,
                shadowCastingMode = ShadowCastingMode.Off,
                matProps = debugOverlayProperties,
                worldBounds = new Bounds(
                    centre,
                    new Vector3(size, 2f, size))
            };
            Graphics.RenderMesh(
                renderParams,
                debugOverlayMesh,
                0,
                objectToWorld);
            lastDebugError = string.Empty;
        }

        private Vector3 ResolveDebugOverlayCentre()
        {
            if (debugFollowResolvedFocus)
            {
                return resolvedDebugFocusPosition;
            }

            if (debugOverlayAnchor != null)
            {
                return debugOverlayAnchor.position;
            }

            return transform.position;
        }

        private void UpdateResolvedDebugFocus()
        {
            Transform focus;
            DebugFocusSource source;

            if (runtimeDebugFocusOverride != null)
            {
                focus = runtimeDebugFocusOverride;
                source = DebugFocusSource.RuntimeOverride;
            }
            else if (debugFocusOverride != null)
            {
                focus = debugFocusOverride;
                source = DebugFocusSource.InspectorOverride;
            }
            else if (debugFallbackCamera != null)
            {
                focus = debugFallbackCamera.transform;
                source = DebugFocusSource.AssignedFallbackCamera;
            }
            else
            {
                if (cachedMainCamera == null ||
                    !cachedMainCamera.isActiveAndEnabled)
                {
                    cachedMainCamera = Camera.main;
                }

                if (cachedMainCamera != null)
                {
                    focus = cachedMainCamera.transform;
                    source = DebugFocusSource.AutomaticMainCamera;
                }
                else
                {
                    focus = transform;
                    source = DebugFocusSource.ControllerFallback;
                }
            }

            resolvedDebugFocus = focus;
            resolvedDebugFocusSource = source;
            resolvedDebugFocusPosition = focus != null
                ? focus.position
                : transform.position;
        }

        private bool EnsureDebugResources()
        {
            if (debugOverlayMesh == null)
            {
                debugOverlayMesh = new Mesh
                {
                    name = "PS3D Weather Cloud Shadow Debug Overlay",
                    hideFlags = HideFlags.HideAndDontSave
                };
                debugOverlayMesh.vertices = new[]
                {
                    new Vector3(-0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, -0.5f),
                    new Vector3(0.5f, 0f, 0.5f),
                    new Vector3(-0.5f, 0f, 0.5f)
                };
                debugOverlayMesh.triangles =
                    new[] { 0, 2, 1, 0, 3, 2 };
                debugOverlayMesh.RecalculateBounds();
                debugOverlayMesh.UploadMeshData(true);
            }

            if (debugOverlayMaterial == null)
            {
                Shader shader = Shader.Find(DebugOverlayShaderName);
                if (shader == null)
                {
                    lastDebugError =
                        $"Required debug shader was not found: {DebugOverlayShaderName}.";
                    return false;
                }

                debugOverlayMaterial = new Material(shader)
                {
                    name = "PS3D Weather Cloud Shadow Debug Overlay",
                    hideFlags = HideFlags.HideAndDontSave
                };
            }

            if (debugOverlayProperties == null)
            {
                debugOverlayProperties = new MaterialPropertyBlock();
            }

            return true;
        }

        private void DestroyDebugResources()
        {
            DestroyTransientObject(debugOverlayMaterial);
            debugOverlayMaterial = null;
            DestroyTransientObject(debugOverlayMesh);
            debugOverlayMesh = null;
            debugOverlayProperties = null;
            lastDebugError = string.Empty;
        }

        private static void DestroyGeneratedTexture(Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(texture);
            }
            else
            {
                DestroyImmediate(texture);
            }
        }

        private static void DestroyTransientObject(UnityEngine.Object value)
        {
            if (value == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(value);
            }
            else
            {
                DestroyImmediate(value);
            }
        }
    }
}
