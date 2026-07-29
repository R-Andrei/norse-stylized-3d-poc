using System;
using System.Collections.Generic;
using System.Text;
using Game.Lighting;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Weather/Weather LightRay Controller")]
    public sealed class WeatherLightRayController : MonoBehaviour
    {
        private const float FallbackMinimumSourceIntensity = 0.01f;
        private const float FallbackMinimumSourceElevation = 0.1f;
        private const int MinimumStorageCapacity = 4;
        private const int MaximumStorageCapacity = 64;
        private const float SurfaceSpotMinimumHeightMetres = 1.5f;
        private const float SurfaceSpotHeightRadiusMultiplier = 2f;
        private const float SurfaceSpotSoftnessHalfWidthRatio = 0.35f;
        private const float SurfaceSpotRangeMargin = 1.5f;
        private const float SurfaceSpotReferenceIntensityAtOneMetre = 3f;
        private const float SurfaceSpotEnableThreshold = 0.0001f;
        private const int DefaultRenderingLayerMask = 1;
        private const int SurfaceSpotRenderingLayerMask =
            DefaultRenderingLayerMask;
        private const float VegetationAccentDirectionMinimumLengthSquared =
            0.000001f;
        private const float SharedAccentLineMaximumRelativeScale = 1000f;
        private const float SharedAccentLineExponentialBase =
            SharedAccentLineMaximumRelativeScale + 1f;
        private const float SharedAccentLineOutputMultiplier = 0.2f;
        private const float SharedAccentLineBaselineDefault = 0.03f;
        private const string ImplementationPatchIdentifier =
            "WEATHER-LIGHT-RAY-V1.2E";

        private static readonly int VegetationAccentDirectionId =
            Shader.PropertyToID(
                "_WeatherLightRayVegetationAccentDirectionWS");
        private static readonly int VegetationAccentSpotPositionId =
            Shader.PropertyToID(
                "_WeatherLightRayVegetationAccentSpotPositionWS");
        private static readonly int VegetationAccentDiagnosticModeId =
            Shader.PropertyToID(
                "_WeatherLightRayVegetationDiagnosticMode");
        private static readonly int AccentLineIntensityId =
            Shader.PropertyToID(
                "_WeatherLightRayAccentLineIntensity");
        private static readonly int AccentLineResolvedScaleId =
            Shader.PropertyToID(
                "_WeatherLightRayAccentLineResolvedScale");
        private static readonly int VegetationAccentCoverageId =
            Shader.PropertyToID(
                "_WeatherLightRayVegetationAccentCoverage");

        public enum ProbeFocusSource
        {
            InspectorOverride = 0,
            AssignedFallbackCamera = 1,
            AutomaticMainCamera = 2,
            ControllerFallback = 3,
            CloudDebugOverlay = 4
        }

        private enum PopulationMetric
        {
            Active,
            Pending,
            Retiring,
            Cooldown,
            CandidateChecks,
            GroundRaycasts,
            CloudSamples,
            Cells
        }

        private struct RuntimeSlot
        {
            public bool Active;
            public uint Generation;
            public WeatherLightRayAnchor AuthoredOwner;
            public bool Procedural;
            public WeatherLightRaySpawnRequest ProceduralRequest;
            public bool ProceduralVisible;
            public uint ProceduralRevision;
            public float SmoothedGateWeight;
            public uint LifecycleRevision;
            public double LastUpdateTime;
            public double SpawnTime;
            public uint EvolutionCurrentSeed;
            public uint EvolutionNextSeed;
            public uint EvolutionAuthoredSeed;
            public double EvolutionElapsedSeconds;
            public float EvolutionDurationSeconds;
            public float EvolutionBlend;
            public int CompletedEvolutionTransitions;
            public bool EvolutionInitialized;
            public WeatherLightRaySnapshot Snapshot;
        }

        private sealed class RuntimeSurfaceLight
        {
            public GameObject GameObject;
            public Light Light;
            public float HeightMetres;
            public float InnerRadiusMetres;
            public float OuterRadiusMetres;
            public float AppliedIntensity;
        }

        // WEATHER VEGETATION ACCENT CONTRACT — DO NOT REINTERPRET.
        // Parameters and source direction are one inseparable per-Light record.
        // Parameters: x = preset-resolved radiance scale, y = stable whole-card
        // coverage, z = selected edge-profile softness, w = Weather override.
        // SourceDirectionWS: xyz = normalized horizontal direction from the
        // receiver toward the celestial/LightRay source, w = direction valid.
        // The renderer mirrors this exact two-float4 layout in HLSL. Never use
        // the punctual Spot's radial Light.direction as a substitute: the Spot
        // direction belongs only to ordinary body lighting and attenuation.
        private struct VegetationAccentOverrideData
        {
            public Vector4 Parameters;
            public Vector4 SourceDirectionWS;
        }

        private static readonly List<WeatherLightRayController>
            ActiveControllersInternal =
                new List<WeatherLightRayController>();

        [Header("Activation")]
        [SerializeField]
        private bool lightRaysEnabled = true;

        [SerializeField]
        private bool previewInEditMode = true;

        [Header("Preset Configuration")]
        [SerializeField]
        private WeatherLightRayPreset activePreset;

        [SerializeField]
        private WeatherLightRayPresetCatalog presetCatalog;

        [Header("Preset Selection & Activation")]
        [SerializeField]
        private WeatherLightRayPresetControlMode presetControlMode =
            WeatherLightRayPresetControlMode.Manual;

        [SerializeField]
        private WeatherLightRaySelectionProfile selectionProfile;

        [SerializeField]
        private WeatherLightRayCycleSourceMode cycleSourceMode =
            WeatherLightRayCycleSourceMode.TimeOfDay;

        [SerializeField]
        private TimeOfDayController timeOfDayController;

        [SerializeField, Range(0f, 1f)]
        private float manualNormalizedCycle = 0.5f;

        [System.NonSerialized] private WeatherLightRayPreset previousPresentationPreset;
        [System.NonSerialized] private double presetTransitionStartedAt;
        [System.NonSerialized] private float presetTransitionDurationSeconds;

        [Header("Sun Source")]
        [SerializeField]
        [Tooltip(
            "Optional explicit Sun. When unassigned, RenderSettings.sun is used.")]
        private Light sunOverride;

        [SerializeField]
        private WeatherLightRaySourceProfile sunProfile;

        [Header("Hybrid Renderer")]
        [SerializeField]
        [Tooltip(
            "Optional designated gameplay camera. When unassigned, Camera.main is used.")]
        private Camera renderCameraOverride;

        [SerializeField]
        private WeatherLightRayRenderDebugView renderDebugView =
            WeatherLightRayRenderDebugView.FinalComposite;

        [Header("Shared LightRay Accent Response")]
        [SerializeField, Range(0f, 1f)]
        [Tooltip(
            "Fallback-only master used when no active Weather LightRay preset is assigned. " +
            "An active preset is authoritative for LightRay-specific stylized accent-line responses. " +
            "0 disables those accents. The response is intentionally exponential: " +
            "approximately 0.03 is 0.046x the former AF5D maximum, 0.10 is 0.20x, " +
            "0.20 is about 0.60x, 0.50 is about 6.13x, and 1.0 is 200x. " +
            "This is exactly 40% of the AF5F output at every slider value and does " +
            "not change real surface-light intensity or ordinary lights. Newly " +
            "created controllers default to 0.03; existing serialized controllers " +
            "retain their saved value and are not migrated by source-default changes.")]
        private float accentLineIntensity = SharedAccentLineBaselineDefault;

        [SerializeField, Range(0f, 1f)]
        [Tooltip(
            "Fallback-only coverage used when no active Weather LightRay preset is assigned. " +
            "Controls how many vegetation blade/card candidates participate in " +
            "the registered Weather LightRay Spot accent response. " +
            "0 selects none; 1 preserves the current full participation. " +
            "This does not dim surviving accents and does not affect ordinary " +
            "lights, vegetation body lighting, or atmospheric beams.")]
        private float lightRayVegetationAccentCoverage = 1f;


        [Header("Beam Evolution Defaults")]
        [SerializeField]
        private WeatherLightRayEvolutionPreset evolutionPreset =
            WeatherLightRayEvolutionPreset.Subtle;

        [SerializeField, Range(0f, 1f)]
        private float evolutionStrength = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float evolutionSpeed = 0.25f;

        [Header("Central Storage")]
        [SerializeField, Range(MinimumStorageCapacity, MaximumStorageCapacity)]
        [Tooltip(
            "Fixed LightRay slot capacity. This is not the desired visible-ray count.")]
        private int maximumActiveRays = 16;

        [Header("Cloud Transition")]
        [SerializeField, Range(0f, 1f)]
        private float cloudEvolutionResumeThreshold = 0.8f;

        [Header("Automatic Population")]
        [SerializeField]
        private bool automaticPopulationEnabled;

        [SerializeField]
        private int automaticPopulationSeed = 7331;

        [SerializeField]
        private Transform automaticPopulationFocusOverride;

        [SerializeField]
        private LayerMask automaticPopulationGroundMask;

        [SerializeField, Range(0, MaximumStorageCapacity)]
        private int automaticPopulationDesiredRayCount = 3;

        [SerializeField, Range(0, MaximumStorageCapacity)]
        private int automaticPopulationMaximumRayCount = 6;

        [SerializeField, Min(0.5f)]
        private float automaticPopulationMinimumSpacingMetres = 12f;

        [SerializeField, Min(0f)]
        private float automaticPopulationOffscreenMarginMetres = 10f;

        [SerializeField, Min(1f)]
        private float automaticPopulationFallbackActiveRadiusMetres = 40f;

        [SerializeField, Range(1f, 30f)]
        private float automaticPopulationEvaluationRateHz = 4f;

        [SerializeField, Range(1, 64)]
        private int automaticPopulationCandidateChecksPerTick = 8;

        [SerializeField, Range(0f, 1f)]
        private float automaticPopulationMinimumClearance = 0.75f;

        [SerializeField, Min(0f)]
        private float automaticPopulationQualificationDurationSeconds = 0.5f;

        [SerializeField, Min(0f)]
        private float automaticPopulationInvalidGraceDurationSeconds = 0.75f;

        [SerializeField, Min(0.75f)]
        private float automaticPopulationMinimumViableOpeningDurationSeconds = 4f;

        [SerializeField, Range(0f, 89f)]
        private float automaticPopulationMaximumGroundSlopeDegrees = 50f;

        [SerializeField, Min(1f)]
        private float automaticPopulationGroundSearchDistanceMetres = 500f;

        [SerializeField]
        private bool showAutomaticPopulationCandidates;

        [Header("Projection Diagnostic")]
        [SerializeField]
        private bool showProjectionProbe = true;

        [SerializeField]
        private Transform projectionProbeFocusOverride;

        [SerializeField]
        private Camera projectionProbeFallbackCamera;

        [SerializeField, Range(3, 9)]
        private int projectionProbeGridResolution = 5;

        [SerializeField, Min(1f)]
        private float projectionProbeSpanMetres = 24f;

        [SerializeField]
        private float projectionProbeSampleHeightMetres;

        [SerializeField, Range(0.05f, 1f)]
        private float projectionProbeMarkerRadiusMetres = 0.2f;

        private RuntimeSlot[] runtimeSlots;
        private RuntimeSurfaceLight[] runtimeSurfaceLights;
        private WeatherLightRayPopulationRuntime automaticPopulationRuntime;
        private WeatherLightRayPopulationRuntime[] selectionPopulationRuntimes =
            Array.Empty<WeatherLightRayPopulationRuntime>();
        private readonly List<WeatherLightRayPopulationRuntime>
            retiringSelectionPopulationRuntimes =
                new List<WeatherLightRayPopulationRuntime>();
        private WeatherLightRayPopulationProfile activePopulationProfile;
        private WeatherLightRaySelectionRuntime selectionRuntime;
        private WeatherLightRayResolvedSelectionDependency
            resolvedSelectionDependency;
        private ulong activePopulationDependencySignature;
        private int[] selectionPopulationRuleOrder = Array.Empty<int>();
        private TimeOfDayController cachedTimeOfDayController;
        private bool timeOfDayDiscoveryAttempted;
        private int discoveredTimeOfDayControllerCount;
        private bool externalCycleOverrideValid;
        private float externalNormalizedCycle;
        private float resolvedNormalizedCycle;
        private string cycleResolutionError = string.Empty;
        private int activeRayCount;
        private int activeProceduralRayCount;
        private int activeSurfaceSpotLightCount;
        private WeatherLightRaySourceState sunSourceState;
        private WeatherLightRaySourceState moonSourceState;
        private WeatherLightRaySourceState independentSourceState;
        private Transform resolvedProbeFocus;
        private ProbeFocusSource resolvedProbeFocusSource;
        private Vector3 resolvedProbeCentre;
        private Camera cachedMainCamera;
        private Camera resolvedRenderCamera;
        private Vector3 publishedVegetationAccentDirection;
        private Vector3 publishedVegetationAccentSpotPosition;
        private float publishedVegetationAccentSpotRange;
        private float cachedAccentLineInput = float.NaN;
        private float cachedAccentLineNormalized;
        private float cachedAccentLineResolvedScale;
        private bool sharedAccentLineCacheDirty = true;
        private bool vegetationAccentOverrideActive;
        private bool vegetationAccentDiagnosticSuiteActive;
        private int vegetationAccentDiagnosticRunId;
        private double vegetationAccentDiagnosticStartedAt;
        private string vegetationAccentDiagnosticCpuVerdict = "Not run";
        private string lastVegetationAccentDiagnosticResults = string.Empty;
        private string lastError = string.Empty;
        // Keyed by the real proxy Light EntityId so the renderer can publish
        // one direct record in each camera's own URP additional-light order.
        // Do not collapse this back to one global owner or a shader-side search.
        private readonly Dictionary<EntityId, VegetationAccentOverrideData>
            vegetationAccentOverridesByLight =
                new Dictionary<EntityId, VegetationAccentOverrideData>();
        private int publishedVegetationAdditionalLightCount;
        private int publishedVegetationWeatherOverrideCount;
        private int publishedVegetationAccentBufferCapacity;
        private bool publishedVegetationAccentIndexOverflow;

        public static int ActiveControllerCount =>
            ActiveControllersInternal.Count;

        public static WeatherLightRayController PublishedController
        {
            get;
            private set;
        }

        public bool LightRaysEnabled => lightRaysEnabled;
        public WeatherLightRayPreset ActivePreset => activePreset;
        public WeatherLightRayPresetCatalog PresetCatalog => presetCatalog;
        public bool UsesPresetAuthority => activePreset != null;
        public bool PreviewInEditMode => previewInEditMode;
        public bool IsPublished => PublishedController == this;
        public int StorageCapacity =>
            runtimeSlots != null ? runtimeSlots.Length : maximumActiveRays;
        public int ActiveRayCount => activeRayCount;
        public int ActiveProceduralRayCount => activeProceduralRayCount;
        public int ActiveAuthoredRayCount => Mathf.Max(
            0,
            activeRayCount - activeProceduralRayCount);
        public int ActiveSurfaceSpotLightCount =>
            activeSurfaceSpotLightCount;
        public float CloudEvolutionResumeThreshold =>
            cloudEvolutionResumeThreshold;
        public WeatherLightRaySourceState SunSourceState => sunSourceState;
        public WeatherLightRaySourceState MoonSourceState => moonSourceState;
        public Camera ResolvedRenderCamera => resolvedRenderCamera;
        public WeatherLightRayRenderDebugView RenderDebugView =>
            renderDebugView;

        // PRESET CONTROL CONTRACT.
        // These three resolved properties are the only production authority for
        // Weather vegetation accent controls. Active presets override Controller
        // fallback serialization. Intensity controls radiance, Coverage controls
        // stable whole-card participation, and Softness controls only the selected
        // blade-edge profile. Do not merge or reinterpret those responsibilities.
        public float AccentLineIntensity => activePreset != null
            ? Mathf.Lerp(
                previousPresentationPreset != null ? previousPresentationPreset.AccentLineIntensity : activePreset.AccentLineIntensity,
                activePreset.AccentLineIntensity,
                PresetPresentationBlend)
            : accentLineIntensity;
        public float LightRayVegetationAccentCoverage => activePreset != null
            ? Mathf.Lerp(
                previousPresentationPreset != null ? previousPresentationPreset.VegetationAccentCoverage : activePreset.VegetationAccentCoverage,
                activePreset.VegetationAccentCoverage,
                PresetPresentationBlend)
            : lightRayVegetationAccentCoverage;
        public float LightRayVegetationAccentSoftness => activePreset != null
            ? Mathf.Lerp(
                previousPresentationPreset != null ? previousPresentationPreset.VegetationAccentSoftness : activePreset.VegetationAccentSoftness,
                activePreset.VegetationAccentSoftness,
                PresetPresentationBlend)
            : 0.5f;
        public WeatherLightRayEvolutionPreset EvolutionPreset => activePreset != null
            ? activePreset.EvolutionPreset
            : evolutionPreset;
        public float EvolutionStrength => activePreset != null
            ? activePreset.EvolutionStrength
            : ResolveEvolutionStrength(evolutionPreset, evolutionStrength);
        public float EvolutionSpeed => activePreset != null
            ? activePreset.EvolutionSpeed
            : ResolveEvolutionSpeed(evolutionPreset, evolutionSpeed);
        public float AccentLineResolvedScale
        {
            get
            {
                RefreshSharedAccentLineCacheIfRequired();
                return cachedAccentLineResolvedScale;
            }
        }
        public bool ProductionVegetationAccentMatchingEnabled =>
            AccentLineResolvedScale > 0f;
        public int SupportedVegetationAccentSpots => Mathf.Clamp(
            StorageCapacity,
            MinimumStorageCapacity,
            MaximumStorageCapacity);
        public int PublishedVegetationAdditionalLightCount =>
            publishedVegetationAdditionalLightCount;
        public int PublishedVegetationWeatherOverrideCount =>
            publishedVegetationWeatherOverrideCount;
        public int PublishedVegetationAccentBufferCapacity =>
            publishedVegetationAccentBufferCapacity;
        public bool PublishedVegetationAccentIndexOverflow =>
            publishedVegetationAccentIndexOverflow;
        public WeatherLightRayPresetControlMode PresetControlMode =>
            presetControlMode;
        public WeatherLightRaySelectionProfile SelectionProfile =>
            selectionProfile;
        public WeatherLightRayCycleSourceMode CycleSourceMode =>
            cycleSourceMode;
        public float ResolvedNormalizedCycle => resolvedNormalizedCycle;
        public string CycleResolutionError => cycleResolutionError;
        public string ActiveSelectionEntryName =>
            selectionRuntime != null && selectionRuntime.SelectedEntry != null
                ? selectionRuntime.SelectedEntry.DisplayName
                : "None";
        public float ActiveSelectionWeight => selectionRuntime != null
            ? selectionRuntime.EffectiveWeight
            : 0f;
        public string SelectionSuspensionReason => selectionRuntime != null
            ? selectionRuntime.SuspensionReason
            : "Selection Profile mode is inactive.";
        public bool AutomaticPopulationEnabled => automaticPopulationEnabled;
        public bool ShowAutomaticPopulationCandidates =>
            showAutomaticPopulationCandidates;
        public string AutomaticPopulationSuspensionReason =>
            ResolveAutomaticPopulationSuspensionReason();
        public Vector3 AutomaticPopulationFocusWorld =>
            ResolveAutomaticPopulationFocusWorld();
        public float AutomaticPopulationActiveRadiusMetres =>
            ResolveAutomaticPopulationActiveRadius();
        public int AutomaticPopulationActiveCount =>
            SumPopulationMetric(PopulationMetric.Active);
        public int AutomaticPopulationPendingCount =>
            SumPopulationMetric(PopulationMetric.Pending);
        public int AutomaticPopulationRetiringCount =>
            SumPopulationMetric(PopulationMetric.Retiring);
        public int AutomaticPopulationCooldownCount =>
            SumPopulationMetric(PopulationMetric.Cooldown);
        public int AutomaticPopulationCandidateChecksLastTick =>
            SumPopulationMetric(PopulationMetric.CandidateChecks);
        public int AutomaticPopulationGroundRaycastsLastTick =>
            SumPopulationMetric(PopulationMetric.GroundRaycasts);
        public int AutomaticPopulationCloudSamplesLastTick =>
            SumPopulationMetric(PopulationMetric.CloudSamples);
        public int AutomaticPopulationCellsInActiveRegion =>
            SumPopulationMetric(PopulationMetric.Cells);
        internal int AutomaticPopulationFreeSlotCount => Mathf.Max(
            0,
            StorageCapacity - activeRayCount);
        public bool ShowProjectionProbe => showProjectionProbe;
        public int ProjectionProbeGridResolution =>
            projectionProbeGridResolution;
        public float ProjectionProbeSpanMetres =>
            projectionProbeSpanMetres;
        public float ProjectionProbeSampleHeightMetres =>
            projectionProbeSampleHeightMetres;
        public float ProjectionProbeMarkerRadiusMetres =>
            projectionProbeMarkerRadiusMetres;
        public Transform ResolvedProbeFocus => resolvedProbeFocus;
        public ProbeFocusSource ResolvedProbeFocusSource =>
            resolvedProbeFocusSource;
        public Vector3 ResolvedProbeCentre => resolvedProbeCentre;
        public bool VegetationAccentDiagnosticSuiteActive =>
            vegetationAccentDiagnosticSuiteActive;
        public int VegetationAccentDiagnosticRunId =>
            vegetationAccentDiagnosticRunId;
        public string VegetationAccentDiagnosticCpuVerdict =>
            vegetationAccentDiagnosticCpuVerdict;
        public string LastVegetationAccentDiagnosticResults =>
            lastVegetationAccentDiagnosticResults;
        public string LastError => lastError;

        private int SumPopulationMetric(PopulationMetric metric)
        {
            int total = 0;
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                total += ReadPopulationMetric(automaticPopulationRuntime, metric);
            }
            else
            {
                for (int index = 0;
                    index < selectionPopulationRuntimes.Length;
                    index++)
                {
                    total += ReadPopulationMetric(
                        selectionPopulationRuntimes[index],
                        metric);
                }
            }

            for (int index = 0;
                index < retiringSelectionPopulationRuntimes.Count;
                index++)
            {
                total += ReadPopulationMetric(
                    retiringSelectionPopulationRuntimes[index],
                    metric);
            }

            return total;
        }

        private static int ReadPopulationMetric(
            WeatherLightRayPopulationRuntime runtime,
            PopulationMetric metric)
        {
            if (runtime == null)
            {
                return 0;
            }

            switch (metric)
            {
                case PopulationMetric.Active:
                    return runtime.ActiveCount;
                case PopulationMetric.Pending:
                    return runtime.PendingCount;
                case PopulationMetric.Retiring:
                    return runtime.RetiringCount;
                case PopulationMetric.Cooldown:
                    return runtime.CooldownCount;
                case PopulationMetric.CandidateChecks:
                    return runtime.CandidateChecksLastTick;
                case PopulationMetric.GroundRaycasts:
                    return runtime.GroundRaycastsLastTick;
                case PopulationMetric.CloudSamples:
                    return runtime.CloudSamplesLastTick;
                case PopulationMetric.Cells:
                    return runtime.CellsInActiveRegion;
                default:
                    return 0;
            }
        }

        private string ResolveAutomaticPopulationSuspensionReason()
        {
            if (presetControlMode ==
                WeatherLightRayPresetControlMode.SelectionProfile &&
                (selectionRuntime == null ||
                    selectionRuntime.SelectedEntry == null))
            {
                return selectionRuntime != null
                    ? selectionRuntime.SuspensionReason
                    : "Selection Profile mode has not initialized.";
            }

            bool foundRuntime = false;
            bool foundRunning = false;
            string firstReason = string.Empty;
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                AppendPopulationSuspensionState(
                    automaticPopulationRuntime,
                    ref foundRuntime,
                    ref foundRunning,
                    ref firstReason);
            }
            else
            {
                for (int index = 0;
                    index < selectionPopulationRuntimes.Length;
                    index++)
                {
                    AppendPopulationSuspensionState(
                        selectionPopulationRuntimes[index],
                        ref foundRuntime,
                        ref foundRunning,
                        ref firstReason);
                }
            }

            if (foundRunning)
            {
                return string.Empty;
            }

            return foundRuntime
                ? firstReason
                : "Automatic population has not initialized.";
        }

        private static void AppendPopulationSuspensionState(
            WeatherLightRayPopulationRuntime runtime,
            ref bool foundRuntime,
            ref bool foundRunning,
            ref string firstReason)
        {
            if (runtime == null)
            {
                return;
            }

            foundRuntime = true;
            if (runtime.IsEnabledAndRunning)
            {
                foundRunning = true;
                return;
            }

            if (string.IsNullOrEmpty(firstReason))
            {
                firstReason = runtime.SuspensionReason;
            }
        }

        private Vector3 ResolveAutomaticPopulationFocusWorld()
        {
            WeatherLightRayPopulationRuntime runtime =
                GetPrimaryPopulationRuntime();
            return runtime != null ? runtime.FocusWorld : Vector3.zero;
        }

        private float ResolveAutomaticPopulationActiveRadius()
        {
            WeatherLightRayPopulationRuntime runtime =
                GetPrimaryPopulationRuntime();
            return runtime != null ? runtime.ActiveRadiusMetres : 0f;
        }

        private WeatherLightRayPopulationRuntime GetPrimaryPopulationRuntime()
        {
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                return automaticPopulationRuntime;
            }

            for (int index = 0;
                index < selectionPopulationRuntimes.Length;
                index++)
            {
                if (selectionPopulationRuntimes[index] != null)
                {
                    return selectionPopulationRuntimes[index];
                }
            }

            return null;
        }

        private void OnEnable()
        {
            if (!ActiveControllersInternal.Contains(this))
            {
                ActiveControllersInternal.Add(this);
            }

            PublishedController = this;
            timeOfDayDiscoveryAttempted = false;
            discoveredTimeOfDayControllerCount = 0;
            MarkSharedAccentLineCacheDirty();
            PublishVegetationAccentDiagnosticMode(false);
            EnsureStorage();
            TickController();
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
            maximumActiveRays = Mathf.Clamp(
                maximumActiveRays,
                MinimumStorageCapacity,
                MaximumStorageCapacity);
            cloudEvolutionResumeThreshold = Mathf.Clamp01(
                cloudEvolutionResumeThreshold);
            manualNormalizedCycle = Mathf.Clamp01(
                manualNormalizedCycle);
            automaticPopulationDesiredRayCount = Mathf.Clamp(
                automaticPopulationDesiredRayCount,
                0,
                MaximumStorageCapacity);
            automaticPopulationMaximumRayCount = Mathf.Clamp(
                automaticPopulationMaximumRayCount,
                0,
                MaximumStorageCapacity);
            automaticPopulationDesiredRayCount = Mathf.Min(
                automaticPopulationDesiredRayCount,
                automaticPopulationMaximumRayCount);
            automaticPopulationMinimumSpacingMetres = Mathf.Max(
                0.5f,
                automaticPopulationMinimumSpacingMetres);
            automaticPopulationOffscreenMarginMetres = Mathf.Max(
                0f,
                automaticPopulationOffscreenMarginMetres);
            automaticPopulationFallbackActiveRadiusMetres = Mathf.Max(
                1f,
                automaticPopulationFallbackActiveRadiusMetres);
            automaticPopulationEvaluationRateHz = Mathf.Clamp(
                automaticPopulationEvaluationRateHz,
                1f,
                30f);
            automaticPopulationCandidateChecksPerTick = Mathf.Clamp(
                automaticPopulationCandidateChecksPerTick,
                1,
                64);
            automaticPopulationMinimumClearance = Mathf.Clamp01(
                automaticPopulationMinimumClearance);
            automaticPopulationQualificationDurationSeconds = Mathf.Max(
                0f,
                automaticPopulationQualificationDurationSeconds);
            automaticPopulationInvalidGraceDurationSeconds = Mathf.Max(
                0f,
                automaticPopulationInvalidGraceDurationSeconds);
            automaticPopulationMinimumViableOpeningDurationSeconds =
                Mathf.Max(
                    0.75f,
                    automaticPopulationMinimumViableOpeningDurationSeconds);
            automaticPopulationMaximumGroundSlopeDegrees = Mathf.Clamp(
                automaticPopulationMaximumGroundSlopeDegrees,
                0f,
                89f);
            automaticPopulationGroundSearchDistanceMetres = Mathf.Max(
                1f,
                automaticPopulationGroundSearchDistanceMetres);
            accentLineIntensity = Mathf.Clamp01(accentLineIntensity);
            lightRayVegetationAccentCoverage = Mathf.Clamp01(
                lightRayVegetationAccentCoverage);
            evolutionStrength = Mathf.Clamp01(evolutionStrength);
            evolutionSpeed = Mathf.Clamp01(evolutionSpeed);
            projectionProbeGridResolution = Mathf.Clamp(
                projectionProbeGridResolution,
                3,
                9);
            if ((projectionProbeGridResolution & 1) == 0)
            {
                projectionProbeGridResolution++;
            }

            projectionProbeSpanMetres = Mathf.Max(
                1f,
                projectionProbeSpanMetres);
            projectionProbeMarkerRadiusMetres = Mathf.Clamp(
                projectionProbeMarkerRadiusMetres,
                0.05f,
                1f);
            timeOfDayDiscoveryAttempted = false;
            discoveredTimeOfDayControllerCount = 0;
            cachedTimeOfDayController = null;
            MarkSharedAccentLineCacheDirty();

            if (isActiveAndEnabled)
            {
                EnsureStorage();
                TickController(false);
            }
        }

        private void Update()
        {
            if (Application.isPlaying || previewInEditMode)
            {
                TickController();
            }
        }

        public void RefreshNow()
        {
            cachedMainCamera = null;
            cachedTimeOfDayController = null;
            timeOfDayDiscoveryAttempted = false;
            discoveredTimeOfDayControllerCount = 0;
            TickController();
        }

        public void SetExternalNormalizedCycle(float normalizedCycle)
        {
            externalNormalizedCycle = Mathf.Clamp01(normalizedCycle);
            externalCycleOverrideValid = true;
        }

        public void ClearExternalNormalizedCycle()
        {
            externalCycleOverrideValid = false;
        }

        public bool TryGetSnapshot(
            WeatherLightRayHandle handle,
            out WeatherLightRaySnapshot snapshot)
        {
            snapshot = default;
            if (!handle.IsValid ||
                runtimeSlots == null ||
                handle.SlotIndex < 0 ||
                handle.SlotIndex >= runtimeSlots.Length)
            {
                return false;
            }

            RuntimeSlot slot = runtimeSlots[handle.SlotIndex];
            if (!slot.Active || slot.Generation != handle.Generation)
            {
                return false;
            }

            snapshot = slot.Snapshot;
            return true;
        }

        public int CopyActiveSnapshots(
            WeatherLightRaySnapshot[] destination)
        {
            if (destination == null)
            {
                return activeRayCount;
            }

            int copied = 0;
            if (runtimeSlots == null)
            {
                return copied;
            }

            for (int index = 0;
                index < runtimeSlots.Length && copied < destination.Length;
                index++)
            {
                if (!runtimeSlots[index].Active)
                {
                    continue;
                }

                destination[copied++] = runtimeSlots[index].Snapshot;
            }

            return copied;
        }


        public int CopyAutomaticPopulationDebugRecords(
            WeatherLightRayPopulationDebugRecord[] destination)
        {
            int total = 0;
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                if (automaticPopulationRuntime != null)
                {
                    total += automaticPopulationRuntime.CopyDebugRecords(
                        destination,
                        total);
                }
            }
            else
            {
                for (int index = 0;
                    index < selectionPopulationRuntimes.Length;
                    index++)
                {
                    WeatherLightRayPopulationRuntime runtime =
                        selectionPopulationRuntimes[index];
                    if (runtime != null)
                    {
                        total += runtime.CopyDebugRecords(destination, total);
                    }
                }
            }

            for (int index = 0;
                index < retiringSelectionPopulationRuntimes.Count;
                index++)
            {
                total += retiringSelectionPopulationRuntimes[index].
                    CopyDebugRecords(destination, total);
            }

            return total;
        }

        internal bool IsAutomaticPopulationPositionClear(
            Vector3 position,
            float minimumSpacingMetres,
            long ignoredIdentity)
        {
            if (runtimeSlots == null)
            {
                return true;
            }

            float spacingSquared = minimumSpacingMetres *
                minimumSpacingMetres;
            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                RuntimeSlot slot = runtimeSlots[index];
                if (!slot.Active)
                {
                    continue;
                }

                if (slot.Procedural &&
                    slot.ProceduralRequest.ExternalIdentity ==
                        ignoredIdentity)
                {
                    continue;
                }

                Vector3 existingPosition = slot.Snapshot.BaseCentreWorld;
                if (slot.Snapshot.Handle.IsValid == false)
                {
                    existingPosition = slot.Procedural
                        ? slot.ProceduralRequest.BaseCentreWorld
                        : slot.AuthoredOwner != null
                            ? slot.AuthoredOwner.transform.position
                            : existingPosition;
                }

                Vector2 delta = new Vector2(
                    existingPosition.x - position.x,
                    existingPosition.z - position.z);
                if (delta.sqrMagnitude < spacingSquared)
                {
                    return false;
                }
            }

            return true;
        }

        public float PresetPresentationBlend
        {
            get
            {
                if (previousPresentationPreset == null || presetTransitionDurationSeconds <= 0f)
                {
                    return 1f;
                }

                float blend = Mathf.Clamp01((float)((Time.realtimeSinceStartupAsDouble - presetTransitionStartedAt) / presetTransitionDurationSeconds));
                if (blend >= 1f)
                {
                    previousPresentationPreset = null;
                    presetTransitionDurationSeconds = 0f;
                }
                return blend;
            }
        }

        public bool TrySetActivePreset(
            WeatherLightRayPreset preset,
            float transitionDurationSeconds,
            out string error)
        {
            error = string.Empty;
            if (preset == null)
            {
                error = "The requested Weather LightRay preset is missing.";
                return false;
            }

            if (preset == activePreset)
            {
                return true;
            }

            previousPresentationPreset = transitionDurationSeconds > 0f ? activePreset : null;
            activePreset = preset;
            presetTransitionStartedAt = Time.realtimeSinceStartupAsDouble;
            presetTransitionDurationSeconds = Mathf.Max(0f, transitionDurationSeconds);
            RefreshNow();
            return true;
        }

        public bool TryRegisterOrUpdateAuthoredRay(
            WeatherLightRayAnchor anchor,
            ref WeatherLightRayHandle handle,
            out string error)
        {
            error = string.Empty;
            if (anchor == null)
            {
                handle = default;
                error = "The authored LightRay anchor is missing.";
                return false;
            }

            if (!isActiveAndEnabled || !IsPublished)
            {
                handle = default;
                error =
                    "The selected Weather LightRay Controller is not the active published controller.";
                return false;
            }

            EnsureStorage();
            if (TryResolveAuthoredSlot(anchor, handle, out int existingIndex))
            {
                handle = new WeatherLightRayHandle(
                    existingIndex,
                    runtimeSlots[existingIndex].Generation);
                return true;
            }

            int freeIndex = FindFreeSlotIndex();

            if (freeIndex < 0)
            {
                handle = default;
                error = "The Weather LightRay storage is full.";
                return false;
            }

            RuntimeSlot slot = runtimeSlots[freeIndex];
            slot.Active = true;
            slot.Generation = NextGeneration(slot.Generation);
            slot.AuthoredOwner = anchor;
            slot.Procedural = false;
            slot.ProceduralRequest = default;
            slot.ProceduralVisible = false;
            slot.ProceduralRevision = 0u;
            slot.SmoothedGateWeight = 0f;
            slot.LifecycleRevision = anchor.LifecycleRevision;
            slot.LastUpdateTime = 0.0;
            slot.SpawnTime = Time.realtimeSinceStartupAsDouble;
            ResetEvolutionState(ref slot);
            slot.Snapshot = default;
            runtimeSlots[freeIndex] = slot;
            activeRayCount++;
            handle = new WeatherLightRayHandle(
                freeIndex,
                slot.Generation);
            return true;
        }

        public void ReleaseAuthoredRay(
            WeatherLightRayAnchor anchor,
            WeatherLightRayHandle handle)
        {
            if (anchor == null || runtimeSlots == null)
            {
                return;
            }

            if (handle.IsValid &&
                handle.SlotIndex >= 0 &&
                handle.SlotIndex < runtimeSlots.Length)
            {
                RuntimeSlot slot = runtimeSlots[handle.SlotIndex];
                if (slot.Active &&
                    slot.Generation == handle.Generation &&
                    slot.AuthoredOwner == anchor)
                {
                    ReleaseSlot(handle.SlotIndex);
                    return;
                }
            }

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (runtimeSlots[index].Active &&
                    runtimeSlots[index].AuthoredOwner == anchor)
                {
                    ReleaseSlot(index);
                    return;
                }
            }
        }

        public bool IsValid(WeatherLightRayHandle handle)
        {
            return TryResolveActiveSlot(handle, out _);
        }

        public bool TrySpawnProceduralRay(
            in WeatherLightRaySpawnRequest request,
            out WeatherLightRayHandle handle,
            out string error)
        {
            handle = default;
            error = string.Empty;
            if (!isActiveAndEnabled || !IsPublished)
            {
                error =
                    "The selected Weather LightRay Controller is not the active published controller.";
                return false;
            }

            if (activePreset == null)
            {
                error =
                    "Procedural LightRay spawning requires an assigned Active Preset.";
                return false;
            }

            if (!TryValidateProceduralRequest(request, out error))
            {
                return false;
            }

            EnsureStorage();
            int freeIndex = FindFreeSlotIndex();
            if (freeIndex < 0)
            {
                error = $"The Weather LightRay storage is full ({activeRayCount}/{StorageCapacity}).";
                return false;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            RuntimeSlot slot = runtimeSlots[freeIndex];
            slot.Active = true;
            slot.Generation = NextGeneration(slot.Generation);
            slot.AuthoredOwner = null;
            slot.Procedural = true;
            slot.ProceduralRequest = request;
            slot.ProceduralVisible = request.InitiallyVisible;
            slot.ProceduralRevision = 1u;
            slot.SmoothedGateWeight = 0f;
            slot.LifecycleRevision = 1u;
            slot.LastUpdateTime = 0.0;
            slot.SpawnTime = now;
            ResetEvolutionState(ref slot);
            slot.Snapshot = default;
            runtimeSlots[freeIndex] = slot;
            activeRayCount++;
            activeProceduralRayCount++;
            handle = new WeatherLightRayHandle(freeIndex, slot.Generation);
            UpdateProceduralSlot(freeIndex, now);
            return true;
        }

        public bool TrySpawnCloudAwareRay(
            in WeatherLightRayCloudQuery query,
            IWeatherLightRayCloudClearanceProvider provider,
            in WeatherLightRayCloudSpawnSettings settings,
            out WeatherLightRayHandle handle,
            out string error)
        {
            handle = default;
            error = string.Empty;
            if (provider == null)
            {
                error =
                    "Cloud-aware LightRay spawning requires a clearance provider.";
                return false;
            }

            if (!TryValidateCloudQuery(query, out error))
            {
                return false;
            }

            if (!provider.TryResolveOpening(query, out var opening))
            {
                error =
                    "The cloud-clearance provider did not resolve a valid opening.";
                return false;
            }

            if (opening.SourceKind != query.SourceKind)
            {
                error =
                    "The resolved cloud opening source does not match the query source.";
                return false;
            }

            if (opening.AreaDiameterMetres < query.MinimumDiameterMetres ||
                opening.AreaDiameterMetres > query.MaximumDiameterMetres)
            {
                error =
                    "The resolved cloud opening diameter is outside the query bounds.";
                return false;
            }

            if (opening.Confidence < query.MinimumConfidence)
            {
                error =
                    "The resolved cloud opening confidence is below the query minimum.";
                return false;
            }

            return TrySpawnFromResolvedCloudOpening(
                opening,
                settings,
                out handle,
                out error);
        }

        public bool TrySpawnFromResolvedCloudOpening(
            in WeatherLightRayCloudOpening opening,
            in WeatherLightRayCloudSpawnSettings settings,
            out WeatherLightRayHandle handle,
            out string error)
        {
            handle = default;
            if (!TryBuildCloudOpeningRequest(
                    opening,
                    settings,
                    out WeatherLightRaySpawnRequest request,
                    out error))
            {
                return false;
            }

            return TrySpawnProceduralRay(
                request,
                out handle,
                out error);
        }

        public bool TryUpdateFromResolvedCloudOpening(
            WeatherLightRayHandle handle,
            in WeatherLightRayCloudOpening opening,
            in WeatherLightRayCloudSpawnSettings settings,
            out string error)
        {
            if (!TryBuildCloudOpeningRequest(
                    opening,
                    settings,
                    out WeatherLightRaySpawnRequest request,
                    out error))
            {
                return false;
            }

            var update = new WeatherLightRayUpdateRequest(
                request,
                settings.ResetLifecycleOnUpdate);
            return TryUpdateProceduralRay(handle, update, out error);
        }

        public bool TrySpawnOrUpdateResolvedCloudOpening(
            ref WeatherLightRayHandle handle,
            in WeatherLightRayCloudOpening opening,
            in WeatherLightRayCloudSpawnSettings settings,
            out bool spawned,
            out string error)
        {
            spawned = false;
            if (IsValid(handle))
            {
                return TryUpdateFromResolvedCloudOpening(
                    handle,
                    opening,
                    settings,
                    out error);
            }

            if (!TrySpawnFromResolvedCloudOpening(
                    opening,
                    settings,
                    out WeatherLightRayHandle spawnedHandle,
                    out error))
            {
                handle = default;
                return false;
            }

            handle = spawnedHandle;
            spawned = true;
            return true;
        }

        public bool TryUpdateProceduralRay(
            WeatherLightRayHandle handle,
            in WeatherLightRayUpdateRequest update,
            out string error)
        {
            error = string.Empty;
            if (!TryResolveActiveSlot(handle, out int slotIndex))
            {
                error = "The procedural LightRay handle is invalid or stale.";
                return false;
            }

            RuntimeSlot slot = runtimeSlots[slotIndex];
            if (!slot.Procedural)
            {
                error = "The supplied handle belongs to an authored LightRay.";
                return false;
            }

            if (!TryValidateProceduralRequest(
                    update.SpawnRequest,
                    out error))
            {
                return false;
            }

            slot.ProceduralRequest = update.SpawnRequest;
            slot.ProceduralVisible = update.SpawnRequest.InitiallyVisible;
            slot.ProceduralRevision = NextGeneration(slot.ProceduralRevision);
            if (update.ResetLifecycle)
            {
                slot.LifecycleRevision = NextGeneration(
                    slot.LifecycleRevision);
                slot.SpawnTime = Time.realtimeSinceStartupAsDouble;
                slot.LastUpdateTime = 0.0;
                slot.SmoothedGateWeight = 0f;
            }

            runtimeSlots[slotIndex] = slot;
            UpdateProceduralSlot(
                slotIndex,
                Time.realtimeSinceStartupAsDouble);
            return true;
        }

        public bool TrySetProceduralRayVisible(
            WeatherLightRayHandle handle,
            bool visible,
            out string error)
        {
            error = string.Empty;
            if (!TryResolveActiveSlot(handle, out int slotIndex))
            {
                error = "The procedural LightRay handle is invalid or stale.";
                return false;
            }

            RuntimeSlot slot = runtimeSlots[slotIndex];
            if (!slot.Procedural)
            {
                error = "The supplied handle belongs to an authored LightRay.";
                return false;
            }

            slot.ProceduralVisible = visible;
            runtimeSlots[slotIndex] = slot;
            return true;
        }

        public bool TryReleaseProceduralRay(
            WeatherLightRayHandle handle,
            out string error)
        {
            error = string.Empty;
            if (!TryResolveActiveSlot(handle, out int slotIndex))
            {
                error = "The procedural LightRay handle is invalid or stale.";
                return false;
            }

            if (!runtimeSlots[slotIndex].Procedural)
            {
                error = "The supplied handle belongs to an authored LightRay.";
                return false;
            }

            ReleaseSlot(slotIndex);
            return true;
        }

        public bool TryGetPrimaryRenderableRay(
            out WeatherLightRaySnapshot snapshot,
            out WeatherLightRaySourceState sourceState)
        {
            snapshot = default;
            sourceState = sunSourceState;
            if (runtimeSlots == null)
            {
                return false;
            }

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                RuntimeSlot slot = runtimeSlots[index];
                if (!slot.Active ||
                    slot.Snapshot.CurrentIntensity <= 0.0001f)
                {
                    continue;
                }

                snapshot = slot.Snapshot;
                sourceState = ResolveRenderableSourceState(snapshot);
                return true;
            }

            return false;
        }

        public WeatherLightRaySourceState ResolveRenderableSourceState(
            WeatherLightRaySnapshot snapshot)
        {
            WeatherLightRaySourceState state = GetSourceState(
                snapshot.SourceKind);
            if (snapshot.SourceGatePolicy !=
                    WeatherLightRaySourceGatePolicy.IgnoreSourceGate ||
                state.Available)
            {
                return state;
            }

            Vector3 rayDirection = snapshot.RayDirectionWorld.sqrMagnitude >
                0.000001f
                    ? snapshot.RayDirectionWorld.normalized
                    : Vector3.down;
            Color fallbackColour = state.SourceLight != null
                ? state.Colour
                : Color.white;
            return new WeatherLightRaySourceState(
                snapshot.SourceKind,
                state.SourceLight,
                state.Profile,
                rayDirection,
                -rayDirection,
                fallbackColour,
                Mathf.Max(1f, state.Intensity),
                Vector3.Dot(-rayDirection, Vector3.up),
                1f,
                true,
                string.Empty);
        }

        public WeatherLightRayAnchor GetPrimaryAuthoredAnchor()
        {
            if (runtimeSlots == null)
            {
                return null;
            }

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (runtimeSlots[index].Active &&
                    runtimeSlots[index].AuthoredOwner != null)
                {
                    return runtimeSlots[index].AuthoredOwner;
                }
            }

            return null;
        }

        public Light GetSurfaceSpotLight(
            WeatherLightRayHandle handle)
        {
            if (!IsCurrentActiveHandle(handle) ||
                runtimeSurfaceLights == null ||
                handle.SlotIndex >= runtimeSurfaceLights.Length)
            {
                return null;
            }

            RuntimeSurfaceLight proxy =
                runtimeSurfaceLights[handle.SlotIndex];
            return proxy != null ? proxy.Light : null;
        }

        public bool TryGetSurfaceSpotLightState(
            WeatherLightRayHandle handle,
            out float heightMetres,
            out float innerRadiusMetres,
            out float outerRadiusMetres,
            out float appliedIntensity)
        {
            heightMetres = 0f;
            innerRadiusMetres = 0f;
            outerRadiusMetres = 0f;
            appliedIntensity = 0f;
            if (!IsCurrentActiveHandle(handle) ||
                runtimeSurfaceLights == null ||
                handle.SlotIndex >= runtimeSurfaceLights.Length)
            {
                return false;
            }

            RuntimeSurfaceLight proxy =
                runtimeSurfaceLights[handle.SlotIndex];
            if (proxy == null || proxy.Light == null)
            {
                return false;
            }

            heightMetres = proxy.HeightMetres;
            innerRadiusMetres = proxy.InnerRadiusMetres;
            outerRadiusMetres = proxy.OuterRadiusMetres;
            appliedIntensity = proxy.AppliedIntensity;
            return true;
        }


        private bool IsCurrentActiveHandle(
            WeatherLightRayHandle handle)
        {
            if (!handle.IsValid ||
                runtimeSlots == null ||
                handle.SlotIndex < 0 ||
                handle.SlotIndex >= runtimeSlots.Length)
            {
                return false;
            }

            RuntimeSlot slot = runtimeSlots[handle.SlotIndex];
            return slot.Active && slot.Generation == handle.Generation;
        }

        public bool TrySampleCloudTransmission(
            Vector3 worldPosition,
            WeatherLightRaySourceKind sourceKind,
            out WeatherCloudTransmissionSample sample)
        {
            WeatherLightRaySourceState sourceState = GetSourceState(
                sourceKind);
            if (sourceState.SourceLight == null)
            {
                sample = WeatherCloudTransmissionSample.Unavailable(
                    sourceState.UnavailableReason);
                return false;
            }

            WeatherCloudShadowController cloudController =
                WeatherCloudShadowController.PublishedController;
            if (cloudController == null)
            {
                sample = WeatherCloudTransmissionSample.ClearSky();
                return true;
            }

            return cloudController.TrySampleCloudTransmission(
                worldPosition,
                sourceState.SourceLight,
                out sample);
        }

        public Vector3 GetProjectionProbeWorldPosition(
            int xIndex,
            int yIndex)
        {
            int resolution = Mathf.Max(1, projectionProbeGridResolution);
            float denominator = Mathf.Max(1, resolution - 1);
            float x = Mathf.Clamp(xIndex, 0, resolution - 1) /
                denominator - 0.5f;
            float z = Mathf.Clamp(yIndex, 0, resolution - 1) /
                denominator - 0.5f;
            return new Vector3(
                resolvedProbeCentre.x + x * projectionProbeSpanMetres,
                projectionProbeSampleHeightMetres,
                resolvedProbeCentre.z + z * projectionProbeSpanMetres);
        }

        public bool TryGetProjectionProbeSample(
            int xIndex,
            int yIndex,
            out Vector3 worldPosition,
            out WeatherCloudTransmissionSample sample)
        {
            worldPosition = GetProjectionProbeWorldPosition(
                xIndex,
                yIndex);
            return TrySampleCloudTransmission(
                worldPosition,
                WeatherLightRaySourceKind.Sun,
                out sample);
        }

        public void ToggleVegetationAccentDiagnosticSuite()
        {
            if (vegetationAccentDiagnosticSuiteActive)
            {
                StopVegetationAccentDiagnosticSuite();
                return;
            }

            RunVegetationAccentDiagnosticSuite();
        }

        public void RunVegetationAccentDiagnosticSuite()
        {
            RefreshNow();
            vegetationAccentDiagnosticRunId++;
            vegetationAccentDiagnosticStartedAt =
                Time.realtimeSinceStartupAsDouble;
            vegetationAccentDiagnosticSuiteActive = IsPublished;
            PublishVegetationAccentDiagnosticMode(
                vegetationAccentDiagnosticSuiteActive);
            lastVegetationAccentDiagnosticResults =
                BuildVegetationAccentDiagnosticReport();
        }

        public void StopVegetationAccentDiagnosticSuite()
        {
            vegetationAccentDiagnosticSuiteActive = false;
            PublishVegetationAccentDiagnosticMode(false);
            lastVegetationAccentDiagnosticResults =
                BuildVegetationAccentDiagnosticReport();
        }

        public string RefreshVegetationAccentDiagnosticResults()
        {
            lastVegetationAccentDiagnosticResults =
                BuildVegetationAccentDiagnosticReport();
            return lastVegetationAccentDiagnosticResults;
        }

        public string BuildVegetationAccentDiagnosticReport()
        {
            bool cpuPreflightPassed =
                EvaluateVegetationAccentDiagnosticCpuPreflight(
                    out string cpuVerdict);
            vegetationAccentDiagnosticCpuVerdict = cpuVerdict;

            var builder = new StringBuilder(8192);
            builder.AppendLine(
                "[Weather LightRay V1.1D-AH1 Vegetation Accent Diagnostic Suite]");
            builder.Append("Implementation patch: ")
                .AppendLine(ImplementationPatchIdentifier);
            builder.AppendLine(
                "Response mapping: 0.2 * (1001^c - 1); reference basis = former AF5D maximum");
            builder.Append("Generated UTC: ")
                .AppendLine(System.DateTime.UtcNow.ToString("O"));
            builder.Append("Run ID / active / started realtime: ")
                .Append(vegetationAccentDiagnosticRunId)
                .Append(" / ")
                .Append(vegetationAccentDiagnosticSuiteActive
                    ? "Yes"
                    : "No")
                .Append(" / ")
                .AppendLine(
                    vegetationAccentDiagnosticStartedAt.ToString("0.000"));
            builder.Append("CPU preflight: ")
                .AppendLine(cpuPreflightPassed
                    ? "PASS — the indexed additional-light sidecar is published and every enabled Weather Spot has an override record."
                    : "FAIL — " + cpuVerdict);
            builder.Append("Application playing / controller published: ")
                .Append(Application.isPlaying ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(IsPublished ? "Yes" : "No");
            builder.Append("LightRays enabled / active rays / enabled Spots: ")
                .Append(lightRaysEnabled ? "Yes" : "No")
                .Append(" / ")
                .Append(activeRayCount)
                .Append(" / ")
                .AppendLine(activeSurfaceSpotLightCount.ToString());
            builder.Append("Enabled LightRay Spots / supported identity capacity: ")
                .Append(activeSurfaceSpotLightCount)
                .Append(" / ")
                .AppendLine(SupportedVegetationAccentSpots.ToString());
            builder.Append("Production matching enabled / diagnostic-forced matching: ")
                .Append(ProductionVegetationAccentMatchingEnabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(vegetationAccentDiagnosticSuiteActive ? "Yes" : "No");
            builder.Append("Code default for new controllers / current serialized authored value: ")
                .Append(SharedAccentLineBaselineDefault.ToString("0.###"))
                .Append(" / ")
                .AppendLine(accentLineIntensity.ToString("0.###"));
            builder.Append("LightRay vegetation accent coverage / diagnostic bypass: ")
                .Append(lightRayVegetationAccentCoverage.ToString("0.###"))
                .Append(" / ")
                .AppendLine(vegetationAccentDiagnosticSuiteActive ? "Yes" : "No");
            builder.Append("Shared accent-line intensity / vegetation gain: ")
                .Append(accentLineIntensity.ToString("0.###"))
                .Append(" / ")
                .AppendLine(
                    AccentLineResolvedScale
                        .ToString("0.###") +
                    "x former AF5D maximum");
            builder.Append("Render camera: ")
                .AppendLine(resolvedRenderCamera != null
                    ? resolvedRenderCamera.name
                    : "None");

            builder.AppendLine();
            builder.AppendLine("[Indexed vegetation accent sidecar]");
            builder.AppendLine(
                "One float4 record is published in URP additional-light order. Weather Spot records contain preset strength, coverage, softness, and override weight; ordinary lights contain zero override weight.");
            builder.Append("Published additional lights / Weather overrides / buffer capacity: ")
                .Append(publishedVegetationAdditionalLightCount)
                .Append(" / ")
                .Append(publishedVegetationWeatherOverrideCount)
                .Append(" / ")
                .AppendLine(publishedVegetationAccentBufferCapacity.ToString());
            builder.Append("Index overflow: ")
                .AppendLine(publishedVegetationAccentIndexOverflow ? "Yes" : "No");
            AppendMaskReport(
                builder,
                "Runtime Spot receiver mask",
                SurfaceSpotRenderingLayerMask);

            builder.AppendLine();
            builder.AppendLine("[Runtime Spot proxies]");
            if (runtimeSurfaceLights == null)
            {
                builder.AppendLine("No runtime Spot storage allocated.");
            }
            else
            {
                for (int slotIndex = 0;
                    slotIndex < runtimeSurfaceLights.Length;
                    slotIndex++)
                {
                    RuntimeSurfaceLight proxy =
                        runtimeSurfaceLights[slotIndex];
                    builder.Append("Slot ")
                        .Append(slotIndex)
                        .AppendLine(":");
                    if (proxy == null || proxy.Light == null)
                    {
                        builder.AppendLine("  Spot: Not allocated");
                        continue;
                    }

                    Light light = proxy.Light;
                    int mask = light.renderingLayerMask;
                    builder.Append("  Object / enabled / active: ")
                        .Append(proxy.GameObject != null
                            ? proxy.GameObject.name
                            : "None")
                        .Append(" / ")
                        .Append(light.enabled ? "Yes" : "No")
                        .Append(" / ")
                        .AppendLine(light.gameObject.activeInHierarchy
                            ? "Yes"
                            : "No");
                    builder.Append("  Type / intensity / range: ")
                        .Append(light.type)
                        .Append(" / ")
                        .Append(light.intensity.ToString("0.######"))
                        .Append(" / ")
                        .AppendLine(light.range.ToString("0.###"));
                    builder.Append("  Inner / outer Spot angle: ")
                        .Append(light.innerSpotAngle.ToString("0.###"))
                        .Append(" / ")
                        .AppendLine(light.spotAngle.ToString("0.###"));
                    builder.Append("  Position / forward: ")
                        .Append(light.transform.position.ToString("F6"))
                        .Append(" / ")
                        .AppendLine(light.transform.forward.ToString("F6"));
                    builder.Append("  Culling mask: ")
                        .Append(light.cullingMask)
                        .Append(" / 0x")
                        .AppendLine(unchecked((uint)light.cullingMask)
                            .ToString("X8"));
                    AppendMaskReport(
                        builder,
                        "  Rendering Layer mask",
                        mask);
                    builder.Append("  Default receiver bit present: ")
                        .AppendLine((mask & DefaultRenderingLayerMask) != 0
                            ? "Yes"
                            : "No");
                    builder.Append("  Indexed accent sidecar override: ")
                        .AppendLine((mask & DefaultRenderingLayerMask) != 0
                            ? "Yes"
                            : "No");
                    builder.Append("  Cached height / inner radius / outer radius / applied intensity: ")
                        .Append(proxy.HeightMetres.ToString("0.###"))
                        .Append(" / ")
                        .Append(proxy.InnerRadiusMetres.ToString("0.###"))
                        .Append(" / ")
                        .Append(proxy.OuterRadiusMetres.ToString("0.###"))
                        .Append(" / ")
                        .AppendLine(proxy.AppliedIntensity.ToString("0.######"));
                }
            }

            builder.AppendLine();
            builder.AppendLine("[Legacy diagnostic reference Spot globals — inactive in indexed production]");
            Vector4 globalSpotPosition = Shader.GetGlobalVector(
                VegetationAccentSpotPositionId);
            Vector3 globalSpotPositionXyz = new Vector3(
                globalSpotPosition.x,
                globalSpotPosition.y,
                globalSpotPosition.z);
            builder.Append("Controller active / cached Spot position / range: ")
                .Append(vegetationAccentOverrideActive ? "Yes" : "No")
                .Append(" / ")
                .Append(publishedVegetationAccentSpotPosition.ToString("F6"))
                .Append(" / ")
                .AppendLine(
                    publishedVegetationAccentSpotRange.ToString("0.######"));
            builder.Append("Shader Spot global xyz / range: ")
                .Append(globalSpotPositionXyz.ToString("F6"))
                .Append(" / ")
                .AppendLine(globalSpotPosition.w.ToString("0.######"));
            builder.Append("Cached/global Spot-position delta: ")
                .AppendLine(Vector3.Distance(
                    publishedVegetationAccentSpotPosition,
                    globalSpotPositionXyz).ToString("0.######"));

            builder.AppendLine();
            builder.AppendLine("[Published shared accent response]");
            builder.Append("Controller value / shader global / mapping: ")
                .Append(AccentLineIntensity.ToString("0.######"))
                .Append(" / ")
                .Append(Shader.GetGlobalFloat(AccentLineIntensityId)
                    .ToString("0.######"))
                .Append(" / resolved scale ")
                .Append(Shader.GetGlobalFloat(AccentLineResolvedScaleId)
                    .ToString("0.######"))
                .Append(" / ")
                .AppendLine(
                    "0 = off, ~0.03 = 0.046x former AF5D maximum, " +
                    "0.10 = ~0.20x, 0.20 = ~0.60x, " +
                    "0.50 = ~6.13x, 1.00 = 200x former AF5D maximum");

            builder.AppendLine();
            builder.AppendLine("[Legacy shared direction global — indexed production uses per-Light directions]");
            Vector4 globalDirection = Shader.GetGlobalVector(
                VegetationAccentDirectionId);
            builder.Append("Controller active / cached direction: ")
                .Append(vegetationAccentOverrideActive ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(
                    publishedVegetationAccentDirection.ToString("F6"));
            builder.Append("Shader direction global xyz / active marker: ")
                .Append(new Vector3(
                    globalDirection.x,
                    globalDirection.y,
                    globalDirection.z).ToString("F6"))
                .Append(" / ")
                .AppendLine(globalDirection.w.ToString("0.######"));
            builder.Append("Global direction magnitude / absolute Y: ")
                .Append(new Vector3(
                    globalDirection.x,
                    globalDirection.y,
                    globalDirection.z).magnitude.ToString("0.######"))
                .Append(" / ")
                .AppendLine(Mathf.Abs(globalDirection.y)
                    .ToString("0.######"));
            builder.Append("Diagnostic global mode: ")
                .AppendLine(Shader.GetGlobalFloat(
                    VegetationAccentDiagnosticModeId).ToString("0.###"));

            if (TryGetPrimaryRenderableRay(
                    out WeatherLightRaySnapshot renderSnapshot,
                    out WeatherLightRaySourceState renderSource))
            {
                TryResolveVegetationAccentDirection(
                    renderSnapshot,
                    renderSource,
                    out Vector3 derivedDirection);
                builder.Append("Primary ray direction / source direction: ")
                    .Append(renderSnapshot.RayDirectionWorld.ToString("F6"))
                    .Append(" / ")
                    .AppendLine(renderSource.DirectionToSourceWorld
                        .ToString("F6"));
                builder.Append("Derived horizontal accent direction: ")
                    .AppendLine(derivedDirection.ToString("F6"));
            }
            else
            {
                builder.AppendLine(
                    "Primary renderable ray: None; no derived direction available.");
            }

            builder.AppendLine();
            builder.AppendLine("[Production vegetation shader]");
            Shader vegetationShader = Shader.Find(
                "PS3D/Vegetation/Stylized Vegetation Benchmark");
            builder.Append("Found / supported / maximum LOD: ")
                .Append(vegetationShader != null ? "Yes" : "No")
                .Append(" / ")
                .Append(vegetationShader != null &&
                    vegetationShader.isSupported
                        ? "Yes"
                        : "No")
                .Append(" / ")
                .AppendLine(vegetationShader != null
                    ? vegetationShader.maximumLOD.ToString()
                    : "N/A");
            builder.AppendLine(
                "The CPU proves every enabled Spot is registered for the indexed sidecar. The false-colour view proves whether an evaluated GPU additional-light index selected that override and emitted edge radiance.");

            builder.AppendLine();
            AppendVegetationAccentDiagnosticLegend(builder);
            builder.AppendLine();
            builder.AppendLine("[Evidence request]");
            builder.AppendLine(
                "While the suite is active, capture grass inside each LightRay Spot and include this copied report. Green blade-edge strips prove indexed override selection and actual edge radiance. Orange means no evaluated additional light resolved a Weather sidecar override.");

            return builder.ToString();
        }

        private bool EvaluateVegetationAccentDiagnosticCpuPreflight(
            out string verdict)
        {
            if (!IsPublished)
            {
                verdict = "This controller is not the published LightRay controller.";
                return false;
            }

            if (!lightRaysEnabled)
            {
                verdict = "LightRays are disabled.";
                return false;
            }

            if (runtimeSurfaceLights == null)
            {
                verdict = "No runtime Spot storage exists.";
                return false;
            }

            if (publishedVegetationAdditionalLightCount <= 0 ||
                publishedVegetationWeatherOverrideCount <= 0 ||
                publishedVegetationAccentIndexOverflow)
            {
                verdict =
                    "The indexed vegetation accent sidecar is unavailable, contains no Weather overrides, or reported an index-count mismatch.";
                return false;
            }

            int enabledSpotCount = 0;
            for (int slotIndex = 0;
                slotIndex < runtimeSurfaceLights.Length;
                slotIndex++)
            {
                RuntimeSurfaceLight proxy = runtimeSurfaceLights[slotIndex];
                if (proxy == null ||
                    proxy.Light == null ||
                    !proxy.Light.enabled)
                {
                    continue;
                }

                int mask = proxy.Light.renderingLayerMask;
                if ((mask & DefaultRenderingLayerMask) == 0)
                {
                    verdict = $"Enabled Spot slot {slotIndex} is missing the default receiver bit.";
                    return false;
                }

                if (!vegetationAccentOverridesByLight.TryGetValue(
                        proxy.Light.GetEntityId(),
                        out VegetationAccentOverrideData registeredAccent))
                {
                    verdict =
                        $"Enabled Spot slot {slotIndex} has no CPU sidecar registration.";
                    return false;
                }

                Vector4 direction = registeredAccent.SourceDirectionWS;
                Vector3 directionXyz = new Vector3(
                    direction.x,
                    direction.y,
                    direction.z);
                if (direction.w <= 0.5f ||
                    directionXyz.sqrMagnitude <
                        VegetationAccentDirectionMinimumLengthSquared ||
                    Mathf.Abs(direction.y) > 0.0001f)
                {
                    verdict =
                        $"Enabled Spot slot {slotIndex} has no valid normalized horizontal source direction in its indexed sidecar record.";
                    return false;
                }

                enabledSpotCount++;
            }

            if (enabledSpotCount != activeSurfaceSpotLightCount)
            {
                verdict =
                    $"Enabled Spot sidecar-registration count mismatch: audited {enabledSpotCount}, controller reports {activeSurfaceSpotLightCount}.";
                return false;
            }

            verdict = "All enabled LightRay Spots are registered with preset parameters and valid per-Light horizontal source directions; sidecar count checks passed.";
            return true;
        }

        private static void AppendMaskReport(
            StringBuilder builder,
            string label,
            int mask)
        {
            builder.Append(label)
                .Append(": ")
                .Append(mask)
                .Append(" / 0x")
                .AppendLine(unchecked((uint)mask).ToString("X8"));
        }

        private static void AppendVegetationAccentDiagnosticLegend(
            StringBuilder builder)
        {
            builder.AppendLine("[GPU false-colour legend]");
            builder.AppendLine(
                "Magenta: the indexed sidecar binding/count is inactive or invalid.");
            builder.AppendLine(
                "Red: no additional light reached this fragment.");
            builder.AppendLine(
                "Orange: additional light data exists, but no evaluated light selected a Weather sidecar override.");
            builder.AppendLine(
                "Purple: a Weather sidecar override was selected, but the Spot failed the vegetation receiver Rendering Layer filter.");
            builder.AppendLine(
                "Yellow: a Weather sidecar override was selected, but its per-Light horizontal source direction is inactive or invalid.");
            builder.AppendLine(
                "Cyan: Spot and direction matched, but the accent override was not selected.");
            builder.AppendLine(
                "Dark blue: override selected, but the matched Spot produced no body radiance here.");
            builder.AppendLine(
                "Blue: override selected and body radiance exists, but actual edge radiance is zero here.");
            builder.AppendLine(
                "Green: override selected and actual LightRay edge radiance is nonzero on this blade-edge fragment.");
        }

        private void AppendPresetSelectionReport(StringBuilder builder)
        {
            builder.AppendLine("[Preset Selection & Activation]");
            builder.Append("Control mode / cycle source: ")
                .Append(presetControlMode)
                .Append(" / ")
                .AppendLine(cycleSourceMode.ToString());
            builder.Append("Resolved normalized cycle: ")
                .AppendLine(resolvedNormalizedCycle.ToString("0.###"));
            if (!string.IsNullOrEmpty(cycleResolutionError))
            {
                builder.Append("Cycle suspension: ")
                    .AppendLine(cycleResolutionError);
            }

            builder.Append("Selection profile / active visual preset: ")
                .Append(selectionProfile != null
                    ? selectionProfile.name
                    : "None")
                .Append(" / ")
                .AppendLine(activePreset != null
                    ? activePreset.name
                    : "None");
            if (presetControlMode !=
                WeatherLightRayPresetControlMode.SelectionProfile)
            {
                builder.AppendLine(
                    "Selection runtime: inactive; Manual preset authority is preserved.");
                return;
            }

            WeatherLightRaySelectionProfile.Entry entry =
                selectionRuntime != null
                    ? selectionRuntime.SelectedEntry
                    : null;
            builder.Append("Selected entry / stable ID: ")
                .Append(entry != null ? entry.DisplayName : "None")
                .Append(" / ")
                .AppendLine(entry != null
                    ? entry.StableId
                    : "None");
            builder.Append("Effective weight / dependency signature: ")
                .Append(selectionRuntime != null
                    ? selectionRuntime.EffectiveWeight.ToString("0.###")
                    : "0")
                .Append(" / 0x")
                .AppendLine(
                    activePopulationDependencySignature.ToString("X16"));
            builder.Append("Resolved source / direction / cloud projection: ")
                .Append(resolvedSelectionDependency.SourceKind)
                .Append(" / ")
                .Append(resolvedSelectionDependency.RayDirectionWorld
                    .ToString("F3"))
                .Append(" / ")
                .AppendLine(
                    resolvedSelectionDependency.CloudProjectionLight != null
                        ? resolvedSelectionDependency.CloudProjectionLight.name
                        : "None");
            string suspension = selectionRuntime != null
                ? selectionRuntime.SuspensionReason
                : "Selection runtime has not initialized.";
            builder.Append("Selection suspension: ")
                .AppendLine(string.IsNullOrEmpty(suspension)
                    ? "None"
                    : suspension);
        }

        private void AppendAutomaticPopulationReports(StringBuilder builder)
        {
            int freeSlots = AutomaticPopulationFreeSlotCount;
            bool appended = false;
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                if (automaticPopulationRuntime != null)
                {
                    automaticPopulationRuntime.AppendReport(
                        builder,
                        BuildLegacyAutomaticPopulationSettings(
                            automaticPopulationEnabled &&
                            Application.isPlaying &&
                            retiringSelectionPopulationRuntimes.Count == 0),
                        freeSlots);
                    appended = true;
                }
            }
            else
            {
                WeatherLightRaySelectionProfile.Entry entry =
                    selectionRuntime != null
                        ? selectionRuntime.SelectedEntry
                        : null;
                WeatherLightRayPopulationProfile profile =
                    entry != null ? entry.PopulationProfile : null;
                if (profile != null &&
                    selectionPopulationRuntimes.Length == profile.RuleCount)
                {
                    WeatherCloudShadowController cloud =
                        WeatherCloudShadowController.PublishedController;
                    float cloudCover = cloud != null &&
                            cloud.IsPublished &&
                            cloud.CloudShadowsEnabled &&
                            cloud.CookieReady
                        ? cloud.MeasuredCloudCover
                        : 0f;
                    int enabledRuleCount = 0;
                    for (int index = 0;
                        index < profile.RuleCount;
                        index++)
                    {
                        WeatherLightRayPopulationProfile.Rule rule =
                            profile.GetRule(index);
                        if (rule != null && rule.Enabled)
                        {
                            enabledRuleCount++;
                        }
                    }

                    int checksPerRule = enabledRuleCount > 0
                        ? Mathf.Max(
                            1,
                            automaticPopulationCandidateChecksPerTick /
                                enabledRuleCount)
                        : 1;
                    int remainingBudget = Mathf.Min(
                        automaticPopulationMaximumRayCount,
                        MaximumStorageCapacity);
                    for (int orderIndex = 0;
                        orderIndex < selectionPopulationRuleOrder.Length;
                        orderIndex++)
                    {
                        int ruleIndex =
                            selectionPopulationRuleOrder[orderIndex];
                        WeatherLightRayPopulationProfile.Rule rule =
                            profile.GetRule(ruleIndex);
                        WeatherLightRayPopulationRuntime runtime =
                            selectionPopulationRuntimes[ruleIndex];
                        if (runtime == null)
                        {
                            continue;
                        }

                        if (rule == null || !rule.Enabled)
                        {
                            runtime.AppendReport(
                                builder,
                                BuildDisabledPopulationSettings(
                                    rule != null
                                        ? rule.DisplayName
                                        : "Disabled Population Rule"),
                                freeSlots);
                            appended = true;
                            continue;
                        }

                        float activation =
                            rule.EvaluateCloudCoverActivation(cloudCover);
                        int requestedMaximum = Mathf.Clamp(
                            Mathf.RoundToInt(
                                rule.MaximumCount * activation),
                            0,
                            MaximumStorageCapacity);
                        int allocatedMaximum = Mathf.Min(
                            requestedMaximum,
                            remainingBudget);
                        int desired = Mathf.Clamp(
                            Mathf.RoundToInt(
                                rule.DesiredCount * activation),
                            0,
                            allocatedMaximum);
                        remainingBudget -= allocatedMaximum;
                        runtime.AppendReport(
                            builder,
                            BuildSelectionPopulationSettings(
                                profile,
                                rule,
                                desired,
                                allocatedMaximum,
                                checksPerRule,
                                automaticPopulationEnabled &&
                                    Application.isPlaying &&
                                    allocatedMaximum > 0 &&
                                    retiringSelectionPopulationRuntimes.Count == 0),
                            freeSlots);
                        appended = true;
                    }
                }
            }

            for (int index = 0;
                index < retiringSelectionPopulationRuntimes.Count;
                index++)
            {
                WeatherLightRayPopulationRuntime runtime =
                    retiringSelectionPopulationRuntimes[index];
                if (runtime == null)
                {
                    continue;
                }

                runtime.AppendReport(
                    builder,
                    BuildDisabledPopulationSettings(
                        "Retiring Incompatible Population"),
                    freeSlots);
                appended = true;
            }

            if (!appended)
            {
                builder.AppendLine("[Automatic Population]");
                builder.AppendLine("Runtime state has not initialized.");
            }
        }

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine("[Weather LightRay V1.2E Selection and Population Report]");
            builder.Append("Implementation patch: ")
                .AppendLine(ImplementationPatchIdentifier);
            builder.AppendLine(
                "Response mapping: 0.2 * (1001^c - 1); reference basis = former AF5D maximum");
            builder.Append("Status: ")
                .AppendLine(string.IsNullOrEmpty(lastError)
                    ? "SOURCE IMPLEMENTED — UNITY VALIDATION PENDING"
                    : "NOT READY");
            builder.Append("Published controller: ")
                .AppendLine(IsPublished ? "Yes" : "No");
            builder.Append("Active controllers: ")
                .AppendLine(ActiveControllerCount.ToString());
            builder.Append("LightRays enabled / edit preview: ")
                .Append(lightRaysEnabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(previewInEditMode ? "Yes" : "No");
            builder.Append("Storage active / authored / procedural / capacity: ")
                .Append(activeRayCount)
                .Append(" / ")
                .Append(ActiveAuthoredRayCount)
                .Append(" / ")
                .Append(activeProceduralRayCount)
                .Append(" / ")
                .AppendLine(StorageCapacity.ToString());
            builder.Append("Enabled real surface Spot Lights: ")
                .AppendLine(activeSurfaceSpotLightCount.ToString());
            builder.Append("Enabled LightRay Spots / supported identity capacity: ")
                .Append(activeSurfaceSpotLightCount)
                .Append(" / ")
                .AppendLine(SupportedVegetationAccentSpots.ToString());
            builder.Append("Code default for new controllers / current serialized authored value: ")
                .Append(SharedAccentLineBaselineDefault.ToString("0.###"))
                .Append(" / ")
                .AppendLine(accentLineIntensity.ToString("0.###"));
            builder.Append("Production matching enabled / diagnostic-forced matching: ")
                .Append(ProductionVegetationAccentMatchingEnabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(vegetationAccentDiagnosticSuiteActive ? "Yes" : "No");
            builder.Append("LightRay vegetation accent coverage / diagnostic bypass: ")
                .Append(lightRayVegetationAccentCoverage.ToString("0.###"))
                .Append(" / ")
                .AppendLine(vegetationAccentDiagnosticSuiteActive ? "Yes" : "No");
            builder.Append("Shared accent-line intensity / vegetation gain: ")
                .Append(accentLineIntensity.ToString("0.###"))
                .Append(" / ")
                .AppendLine(
                    AccentLineResolvedScale
                        .ToString("0.###") +
                    "x former AF5D maximum");
            builder.Append("Vegetation accent override / direction / mask: ")
                .Append(vegetationAccentOverrideActive ? "Active" : "Inactive")
                .Append(" / ")
                .Append(publishedVegetationAccentDirection.ToString("F3"))
                .Append(" / ")
                .AppendLine(SurfaceSpotRenderingLayerMask.ToString());
            builder.Append("Authored registration: ")
                .AppendLine("Implemented; every active authored or procedural slot can own a runtime surface Spot. Vegetation tuning is published by URP additional-light index with no single-Spot owner.");
            builder.Append("Resolved render camera / debug view: ")
                .Append(resolvedRenderCamera != null
                    ? resolvedRenderCamera.name
                    : "None")
                .Append(" / ")
                .AppendLine(renderDebugView.ToString());

            WeatherLightRayAnchor primaryAnchor = GetPrimaryAuthoredAnchor();
            builder.Append("Primary authored anchor: ")
                .AppendLine(primaryAnchor != null
                    ? primaryAnchor.name
                    : "None");
            if (TryGetPrimaryRenderableRay(
                    out WeatherLightRaySnapshot renderSnapshot,
                    out WeatherLightRaySourceState renderSource))
            {
                builder.Append("Renderable handle / lifecycle: ")
                    .Append(renderSnapshot.Handle)
                    .Append(" / ")
                    .AppendLine(renderSnapshot.LifecycleState.ToString());
                builder.Append("Centre / direction / height: ")
                    .Append(renderSnapshot.BaseCentreWorld.ToString("F3"))
                    .Append(" / ")
                    .Append(renderSnapshot.RayDirectionWorld.ToString("F3"))
                    .Append(" / ")
                    .Append(renderSnapshot.Height.ToString("0.###"))
                    .AppendLine(" m");
                builder.Append("Area diameter / footprint radius: ")
                    .Append(renderSnapshot.Descriptor.AreaDiameterMetres.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.FootprintRadiusMetres.ToString("0.###"))
                    .AppendLine(" m");
                WeatherLightRayAreaLayout areaLayout =
                    WeatherLightRayAreaLayout.Calculate(
                        renderSnapshot.Descriptor.AreaDiameterMetres,
                        renderSnapshot.Descriptor.BeamSpacingMetres);
                builder.Append("Beam spacing / resolved beams / centre pitch / representative beam / overlap: ")
                    .Append(renderSnapshot.Descriptor.BeamSpacingMetres.ToString("0.###"))
                    .Append(" m / ")
                    .Append(renderSnapshot.Descriptor.BeamCount)
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.BeamPitchMetres.ToString("0.###"))
                    .Append(" m / ")
                    .Append(areaLayout.AverageAtmosphericBeamWidthMetres.ToString("0.###"))
                    .Append(" m / ")
                    .Append(areaLayout.AverageAtmosphericOverlapMetres.ToString("0.###"))
                    .AppendLine(" m");
                builder.Append("Contact axis / width weight range: ")
                    .Append("World X / ")
                    .AppendLine(renderSnapshot.Descriptor.BeamWidthRatioRange.ToString("F2"));
                builder.Append("Current intensity / cloud transmission: ")
                    .Append(renderSnapshot.CurrentIntensity.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.CurrentCloudTransmission.ToString("0.###"));
                builder.Append("Source / lifetime / source gate: ")
                    .Append(renderSnapshot.SourceKind)
                    .Append(" / ")
                    .Append(renderSnapshot.LifetimePolicy)
                    .Append(" / ")
                    .AppendLine(renderSnapshot.SourceGatePolicy.ToString());
                builder.Append("Atmosphere / softening / edge softness: ")
                    .Append(renderSnapshot.Descriptor.AtmosphericIntensity.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.SofteningStrength.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.BeamEdgeSoftness.ToString("0.###"));
                float representativeFadeLength =
                    renderSnapshot.Height * renderSnapshot.Descriptor.GroundFade;
                float aboveContactFadeLength = representativeFadeLength * 0.65f;
                float belowContactExtension = representativeFadeLength * 0.35f;
                builder.Append("Upper fade / ground contact fade length: ")
                    .Append(renderSnapshot.Descriptor.UpperFade.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.GroundFade.ToString("0.###"));
                builder.Append("Contact-plane opacity / above-contact fade / below-contact extension: ")
                    .Append(renderSnapshot.Descriptor.ContactPlaneOpacity.ToString("0.###"))
                    .Append(" / ")
                    .Append(aboveContactFadeLength.ToString("0.###"))
                    .Append(" m / ")
                    .Append(belowContactExtension.ToString("0.###"))
                    .AppendLine(" m");
                builder.Append("Real Spot / optional screen complement / edge softness: ")
                    .Append(renderSnapshot.Descriptor.SurfaceSpotLightIntensity.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.ScreenSpaceSurfaceIntensity.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.FootprintEdgeSoftness.ToString("0.###"));
                Light surfaceSpot = GetSurfaceSpotLight(renderSnapshot.Handle);
                builder.Append("Runtime surface Spot: ")
                    .AppendLine(surfaceSpot != null
                        ? surfaceSpot.name +
                            (surfaceSpot.enabled ? " (enabled)" : " (disabled)")
                        : "Not created");
                if (TryGetSurfaceSpotLightState(
                        renderSnapshot.Handle,
                        out float spotHeight,
                        out float spotInnerRadius,
                        out float spotOuterRadius,
                        out float spotAppliedIntensity))
                {
                    builder.Append("Spot height / inner radius / outer radius / applied intensity: ")
                        .Append(spotHeight.ToString("0.###"))
                        .Append(" m / ")
                        .Append(spotInnerRadius.ToString("0.###"))
                        .Append(" m / ")
                        .Append(spotOuterRadius.ToString("0.###"))
                        .Append(" m / ")
                        .AppendLine(spotAppliedIntensity.ToString("0.###"));
                }
                builder.Append("Colour multiplier / Sun warmth: ")
                    .Append(renderSnapshot.Descriptor.ColourMultiplier.ToString())
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.WarmthContribution.ToString("0.###"));
                builder.Append("Cloud policy / source colour: ")
                    .Append(renderSnapshot.CloudPolicy)
                    .Append(" / ")
                    .AppendLine((renderSource.Colour * renderSource.Intensity *
                        renderSnapshot.ColourMultiplier).ToString());
            }
            else
            {
                builder.AppendLine("Primary renderable ray: None");
            }
            AppendSourceReport(builder, sunSourceState);
            AppendSourceReport(builder, moonSourceState);
            AppendSourceReport(builder, independentSourceState);

            AppendPresetSelectionReport(builder);

            WeatherCloudShadowController cloudController =
                WeatherCloudShadowController.PublishedController;
            builder.Append("Published cloud controller: ")
                .AppendLine(cloudController != null
                    ? cloudController.name
                    : "None (clear-sky fallback)");
            if (cloudController != null)
            {
                builder.Append("Cloud cookie ready / evolution: ")
                    .Append(cloudController.CookieReady ? "Yes" : "No")
                    .Append(" / ")
                    .AppendLine(cloudController.EvolutionState.ToString());
                builder.Append("Evolution resume threshold: ")
                    .AppendLine(
                        cloudEvolutionResumeThreshold.ToString("P0"));
            }

            AppendAutomaticPopulationReports(builder);

            builder.Append("Projection focus: ")
                .Append(resolvedProbeFocus != null
                    ? resolvedProbeFocus.name
                    : "None")
                .Append(" | source: ")
                .AppendLine(resolvedProbeFocusSource.ToString());
            builder.Append("Projection centre / grid / span: ")
                .Append(resolvedProbeCentre.ToString("F3"))
                .Append(" / ")
                .Append(projectionProbeGridResolution)
                .Append(" × ")
                .Append(projectionProbeGridResolution)
                .Append(" / ")
                .Append(projectionProbeSpanMetres.ToString("0.###"))
                .AppendLine(" m");
            AppendProjectionDiagnostic(builder, cloudController);

            if (!string.IsNullOrEmpty(lastError))
            {
                builder.AppendLine("Error:");
                builder.AppendLine(lastError);
            }

            return builder.ToString();
        }

        /// <summary>
        /// WEATHER LIGHTRAY CONTROLLER EXECUTION ORDER CONTRACT.
        ///
        /// Resolve source/camera state first, then select visual appearance and
        /// explicit dependencies, then execute population policies, and only
        /// afterwards rebuild ray snapshots and surface lights. Do not move
        /// population policy into visual presets or let population selection
        /// bypass the shared slot/lifecycle paths.
        /// </summary>
        private void TickController(bool allowSurfaceLightCreation = true)
        {
            if (!isActiveAndEnabled || PublishedController != this)
            {
                DisableAllSurfaceSpotLights();
                return;
            }

            EnsureStorage();
            PublishSharedAccentLineIntensity();
            ResolveSourceStates();
            ResolveRenderCamera();
            lastError = string.Empty;
            TickPresetSelection();
            TickAutomaticPopulation();
            UpdateRegisteredRays();
            UpdateSurfaceSpotLights(allowSurfaceLightCreation);
            ResolveProjectionFocus();
        }

        /// <summary>
        /// WEATHER LIGHTRAY PRESET-SELECTION CONTRACT.
        ///
        /// Manual mode preserves the serialized Active Preset. Selection mode
        /// evaluates only normalized-cycle curves and explicit dependencies.
        /// Visual preset SourceKind metadata is never consulted here. A visual
        /// change with the same population profile and dependency signature must
        /// preserve automatic handles; dependency changes retire the old
        /// population before new rules qualify.
        /// </summary>
        private void TickPresetSelection()
        {
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                // Manual mode has no activation-cycle dependency. It must remain
                // fully functional even when no Time Of Day or external cycle
                // provider exists, and switching back to Manual must always retire
                // Selection Profile-owned populations.
                resolvedNormalizedCycle = 0f;
                cycleResolutionError = string.Empty;
                selectionRuntime?.Shutdown();
                resolvedSelectionDependency = default;
                EnsureSelectionPopulationContext(null, 0UL);
                return;
            }

            if (!TryResolveNormalizedCycle(out resolvedNormalizedCycle,
                    out cycleResolutionError))
            {
                resolvedNormalizedCycle = 0f;
                selectionRuntime?.Shutdown();
                resolvedSelectionDependency = default;
                EnsureSelectionPopulationContext(null, 0UL);
                return;
            }

            if (selectionRuntime == null)
            {
                selectionRuntime = new WeatherLightRaySelectionRuntime();
            }

            selectionRuntime.Tick(
                this,
                selectionProfile,
                resolvedNormalizedCycle,
                Time.realtimeSinceStartupAsDouble);
            WeatherLightRaySelectionProfile.Entry entry =
                selectionRuntime.SelectedEntry;
            if (entry == null)
            {
                resolvedSelectionDependency = default;
                EnsureSelectionPopulationContext(null, 0UL);
                return;
            }

            resolvedSelectionDependency = selectionRuntime.Dependency;
            EnsureSelectionPopulationContext(
                entry.PopulationProfile,
                selectionRuntime.DependencySignature);
        }

        private bool TryResolveNormalizedCycle(
            out float cycle,
            out string error)
        {
            cycle = 0f;
            error = string.Empty;
            switch (cycleSourceMode)
            {
                case WeatherLightRayCycleSourceMode.ManualNormalizedValue:
                    cycle = Mathf.Clamp01(manualNormalizedCycle);
                    return true;
                case WeatherLightRayCycleSourceMode.ExternalRuntimeOverride:
                    if (!externalCycleOverrideValid)
                    {
                        error =
                            "External normalized-cycle mode has no runtime override.";
                        return false;
                    }
                    cycle = Mathf.Clamp01(externalNormalizedCycle);
                    return true;
                default:
                    TimeOfDayController controller = ResolveTimeOfDayController();
                    if (controller == null)
                    {
                        if (timeOfDayController != null)
                        {
                            error =
                                "The explicitly assigned TimeOfDayController is not active and enabled.";
                        }
                        else if (discoveredTimeOfDayControllerCount > 1)
                        {
                            error =
                                "Time Of Day cycle mode found multiple active candidates. Assign the intended TimeOfDayController explicitly.";
                        }
                        else
                        {
                            error =
                                "Time Of Day cycle mode requires one explicit or unambiguous active TimeOfDayController.";
                        }
                        return false;
                    }
                    cycle = Mathf.Clamp01(controller.NormalizedTime);
                    return true;
            }
        }

        private TimeOfDayController ResolveTimeOfDayController()
        {
            if (timeOfDayController != null)
            {
                return timeOfDayController.isActiveAndEnabled
                    ? timeOfDayController
                    : null;
            }

            if (cachedTimeOfDayController != null)
            {
                return cachedTimeOfDayController.isActiveAndEnabled
                    ? cachedTimeOfDayController
                    : null;
            }

            // Automatic discovery is deliberately one-shot per enable/refresh.
            // FindObjectsByType allocates an array, so it must never become a
            // recurring selection-tick or frame cost.
            if (timeOfDayDiscoveryAttempted)
            {
                return null;
            }

            timeOfDayDiscoveryAttempted = true;
            TimeOfDayController[] controllers =
                FindObjectsByType<TimeOfDayController>();
            discoveredTimeOfDayControllerCount = controllers.Length;
            cachedTimeOfDayController = controllers.Length == 1 &&
                controllers[0] != null &&
                controllers[0].isActiveAndEnabled
                    ? controllers[0]
                    : null;
            return cachedTimeOfDayController;
        }

        private void EnsureSelectionPopulationContext(
            WeatherLightRayPopulationProfile populationProfile,
            ulong dependencySignature)
        {
            if (activePopulationProfile == populationProfile &&
                activePopulationDependencySignature == dependencySignature)
            {
                return;
            }

            RetireSelectionPopulationRuntimes();
            activePopulationProfile = populationProfile;
            activePopulationDependencySignature = dependencySignature;
            if (populationProfile == null)
            {
                selectionPopulationRuntimes =
                    Array.Empty<WeatherLightRayPopulationRuntime>();
                selectionPopulationRuleOrder = Array.Empty<int>();
                return;
            }

            int ruleCount = populationProfile.RuleCount;
            selectionPopulationRuntimes =
                new WeatherLightRayPopulationRuntime[ruleCount];
            selectionPopulationRuleOrder = new int[ruleCount];
            for (int index = 0; index < ruleCount; index++)
            {
                selectionPopulationRuntimes[index] =
                    new WeatherLightRayPopulationRuntime();
                selectionPopulationRuleOrder[index] = index;
            }

            // Dirty-time/profile-change ordering only. Profile order is the
            // deterministic tie-breaker for equal priority.
            for (int index = 1; index < ruleCount; index++)
            {
                int value = selectionPopulationRuleOrder[index];
                int valuePriority = populationProfile.GetRule(value) != null
                    ? populationProfile.GetRule(value).Priority
                    : int.MinValue;
                int cursor = index - 1;
                while (cursor >= 0)
                {
                    int other = selectionPopulationRuleOrder[cursor];
                    int otherPriority = populationProfile.GetRule(other) != null
                        ? populationProfile.GetRule(other).Priority
                        : int.MinValue;
                    if (otherPriority >= valuePriority)
                    {
                        break;
                    }
                    selectionPopulationRuleOrder[cursor + 1] = other;
                    cursor--;
                }
                selectionPopulationRuleOrder[cursor + 1] = value;
            }
        }

        private void RetireSelectionPopulationRuntimes()
        {
            for (int index = 0;
                index < selectionPopulationRuntimes.Length;
                index++)
            {
                WeatherLightRayPopulationRuntime runtime =
                    selectionPopulationRuntimes[index];
                if (runtime == null)
                {
                    continue;
                }

                runtime.Shutdown(this, false);
                retiringSelectionPopulationRuntimes.Add(runtime);
            }

            selectionPopulationRuntimes =
                Array.Empty<WeatherLightRayPopulationRuntime>();
            selectionPopulationRuleOrder = Array.Empty<int>();
        }

        private void TickAutomaticPopulation()
        {
            TickRetiringPopulationRuntimes();
            if (presetControlMode == WeatherLightRayPresetControlMode.Manual)
            {
                if (automaticPopulationRuntime == null)
                {
                    automaticPopulationRuntime =
                        new WeatherLightRayPopulationRuntime();
                }

                WeatherLightRayPopulationRuntime.Settings settings =
                    BuildLegacyAutomaticPopulationSettings(
                        automaticPopulationEnabled &&
                        Application.isPlaying &&
                        retiringSelectionPopulationRuntimes.Count == 0);
                automaticPopulationRuntime.Tick(
                    this,
                    settings,
                    Time.realtimeSinceStartupAsDouble);
                return;
            }

            if (automaticPopulationRuntime != null)
            {
                automaticPopulationRuntime.Shutdown(this, false);
                retiringSelectionPopulationRuntimes.Add(
                    automaticPopulationRuntime);
                automaticPopulationRuntime = null;
            }

            TickSelectionPopulationRuntimes();
        }

        private void TickRetiringPopulationRuntimes()
        {
            if (retiringSelectionPopulationRuntimes.Count == 0)
            {
                return;
            }

            WeatherLightRayPopulationRuntime.Settings settings =
                BuildDisabledPopulationSettings("Retiring Population");
            double now = Time.realtimeSinceStartupAsDouble;
            for (int index =
                    retiringSelectionPopulationRuntimes.Count - 1;
                index >= 0;
                index--)
            {
                WeatherLightRayPopulationRuntime runtime =
                    retiringSelectionPopulationRuntimes[index];
                runtime.Tick(this, settings, now);
                if (runtime.ActiveCount == 0 &&
                    runtime.PendingCount == 0 &&
                    runtime.RetiringCount == 0 &&
                    runtime.CooldownCount == 0)
                {
                    retiringSelectionPopulationRuntimes.RemoveAt(index);
                }
            }
        }

        private void TickSelectionPopulationRuntimes()
        {
            WeatherLightRaySelectionProfile.Entry entry =
                selectionRuntime != null
                    ? selectionRuntime.SelectedEntry
                    : null;
            WeatherLightRayPopulationProfile profile =
                entry != null ? entry.PopulationProfile : null;
            if (profile == null ||
                selectionPopulationRuntimes.Length != profile.RuleCount)
            {
                return;
            }

            WeatherCloudShadowController cloud =
                WeatherCloudShadowController.PublishedController;
            float cloudCover = cloud != null &&
                    cloud.IsPublished &&
                    cloud.CloudShadowsEnabled &&
                    cloud.CookieReady
                ? cloud.MeasuredCloudCover
                : 0f;
            int enabledRuleCount = 0;
            for (int index = 0; index < profile.RuleCount; index++)
            {
                WeatherLightRayPopulationProfile.Rule rule =
                    profile.GetRule(index);
                if (rule != null && rule.Enabled)
                {
                    enabledRuleCount++;
                }
            }

            int checksPerRule = enabledRuleCount > 0
                ? Mathf.Max(
                    1,
                    automaticPopulationCandidateChecksPerTick /
                        enabledRuleCount)
                : 1;
            int remainingGlobalBudget = Mathf.Min(
                automaticPopulationMaximumRayCount,
                MaximumStorageCapacity);
            double now = Time.realtimeSinceStartupAsDouble;
            for (int orderIndex = 0;
                orderIndex < selectionPopulationRuleOrder.Length;
                orderIndex++)
            {
                int ruleIndex = selectionPopulationRuleOrder[orderIndex];
                WeatherLightRayPopulationProfile.Rule rule =
                    profile.GetRule(ruleIndex);
                WeatherLightRayPopulationRuntime runtime =
                    selectionPopulationRuntimes[ruleIndex];
                if (rule == null || !rule.Enabled)
                {
                    runtime.Tick(
                        this,
                        BuildDisabledPopulationSettings(
                            rule != null
                                ? rule.DisplayName
                                : "Disabled Population Rule"),
                        now);
                    continue;
                }

                float activation = rule.EvaluateCloudCoverActivation(
                    cloudCover);
                int requestedMaximum = Mathf.Clamp(
                    Mathf.RoundToInt(rule.MaximumCount * activation),
                    0,
                    MaximumStorageCapacity);
                int allocatedMaximum = Mathf.Min(
                    requestedMaximum,
                    remainingGlobalBudget);
                int requestedDesired = Mathf.Clamp(
                    Mathf.RoundToInt(rule.DesiredCount * activation),
                    0,
                    allocatedMaximum);
                remainingGlobalBudget -= allocatedMaximum;

                WeatherLightRayPopulationRuntime.Settings settings =
                    BuildSelectionPopulationSettings(
                        profile,
                        rule,
                        requestedDesired,
                        allocatedMaximum,
                        checksPerRule,
                        automaticPopulationEnabled &&
                            Application.isPlaying &&
                            allocatedMaximum > 0 &&
                            retiringSelectionPopulationRuntimes.Count == 0);
                runtime.Tick(this, settings, now);
            }
        }

        private WeatherLightRayPopulationRuntime.Settings
            BuildLegacyAutomaticPopulationSettings(bool runtimeEnabled)
        {
            return new WeatherLightRayPopulationRuntime.Settings(
                "Automatic Population (Manual Legacy)",
                runtimeEnabled,
                lightRaysEnabled,
                automaticPopulationSeed,
                0x4C45474143595631UL,
                automaticPopulationFocusOverride,
                resolvedRenderCamera,
                automaticPopulationGroundMask,
                automaticPopulationDesiredRayCount,
                automaticPopulationMaximumRayCount,
                automaticPopulationMinimumSpacingMetres,
                automaticPopulationOffscreenMarginMetres,
                automaticPopulationFallbackActiveRadiusMetres,
                automaticPopulationEvaluationRateHz,
                automaticPopulationCandidateChecksPerTick,
                automaticPopulationMinimumClearance,
                0f,
                0f,
                automaticPopulationQualificationDurationSeconds,
                automaticPopulationInvalidGraceDurationSeconds,
                automaticPopulationMinimumViableOpeningDurationSeconds,
                automaticPopulationMaximumGroundSlopeDegrees,
                automaticPopulationGroundSearchDistanceMetres,
                cloudEvolutionResumeThreshold,
                activePreset,
                WeatherLightRaySourceKind.Sun,
                sunSourceState.RayDirectionWorld,
                WeatherLightRaySourceGatePolicy.RequireActiveSource,
                sunSourceState.Available &&
                    sunSourceState.SourceLight != null,
                sunSourceState.UnavailableReason,
                sunSourceState.SourceLight,
                WeatherLightRayCloudDataRequirement.Required,
                WeatherLightRaySpatialCloudPolicy.ClearFootprint,
                WeatherCloudShadowController.PublishedController);
        }

        private WeatherLightRayPopulationRuntime.Settings
            BuildSelectionPopulationSettings(
                WeatherLightRayPopulationProfile profile,
                WeatherLightRayPopulationProfile.Rule rule,
                int desiredCount,
                int maximumCount,
                int candidateChecksPerTick,
                bool runtimeEnabled)
        {
            // Stable candidate identity is population-policy identity, not
            // visual-preset or asset-name identity. Renaming a profile or
            // switching to another compatible visual preset must not reshuffle
            // existing world candidates.
            ulong identitySalt = HashStableText(profile.StableId) ^
                HashStableText(rule.StableId) ^
                resolvedSelectionDependency.Signature;
            return new WeatherLightRayPopulationRuntime.Settings(
                rule.DisplayName,
                runtimeEnabled,
                lightRaysEnabled,
                automaticPopulationSeed,
                identitySalt,
                automaticPopulationFocusOverride,
                resolvedRenderCamera,
                automaticPopulationGroundMask,
                desiredCount,
                maximumCount,
                rule.MinimumSpacingMetres,
                automaticPopulationOffscreenMarginMetres,
                automaticPopulationFallbackActiveRadiusMetres,
                automaticPopulationEvaluationRateHz,
                candidateChecksPerTick,
                rule.MinimumClearance,
                rule.MinimumDistinctOpeningContrast,
                rule.SurroundingSampleRadiusMetres,
                automaticPopulationQualificationDurationSeconds,
                automaticPopulationInvalidGraceDurationSeconds,
                automaticPopulationMinimumViableOpeningDurationSeconds,
                automaticPopulationMaximumGroundSlopeDegrees,
                automaticPopulationGroundSearchDistanceMetres,
                cloudEvolutionResumeThreshold,
                activePreset,
                resolvedSelectionDependency.SourceKind,
                resolvedSelectionDependency.RayDirectionWorld,
                resolvedSelectionDependency.SourceGatePolicy,
                resolvedSelectionDependency.Valid,
                resolvedSelectionDependency.FailureReason,
                resolvedSelectionDependency.CloudProjectionLight,
                rule.CloudDataRequirement,
                rule.SpatialCloudPolicy,
                WeatherCloudShadowController.PublishedController);
        }

        private WeatherLightRayPopulationRuntime.Settings
            BuildDisabledPopulationSettings(string label)
        {
            return new WeatherLightRayPopulationRuntime.Settings(
                label,
                false,
                lightRaysEnabled,
                automaticPopulationSeed,
                0UL,
                automaticPopulationFocusOverride,
                resolvedRenderCamera,
                automaticPopulationGroundMask,
                0,
                0,
                automaticPopulationMinimumSpacingMetres,
                automaticPopulationOffscreenMarginMetres,
                automaticPopulationFallbackActiveRadiusMetres,
                automaticPopulationEvaluationRateHz,
                1,
                automaticPopulationMinimumClearance,
                0f,
                0f,
                automaticPopulationQualificationDurationSeconds,
                automaticPopulationInvalidGraceDurationSeconds,
                automaticPopulationMinimumViableOpeningDurationSeconds,
                automaticPopulationMaximumGroundSlopeDegrees,
                automaticPopulationGroundSearchDistanceMetres,
                cloudEvolutionResumeThreshold,
                activePreset,
                WeatherLightRaySourceKind.Independent,
                Vector3.down,
                WeatherLightRaySourceGatePolicy.IgnoreSourceGate,
                true,
                string.Empty,
                null,
                WeatherLightRayCloudDataRequirement.Ignored,
                WeatherLightRaySpatialCloudPolicy.AnyPosition,
                null);
        }

        private void EnsureStorage()
        {
            int capacity = Mathf.Clamp(
                maximumActiveRays,
                MinimumStorageCapacity,
                MaximumStorageCapacity);
            if (runtimeSlots != null && runtimeSlots.Length == capacity)
            {
                return;
            }

            var replacement = new RuntimeSlot[capacity];
            int copiedActiveCount = 0;
            int copiedProceduralCount = 0;
            if (runtimeSlots != null)
            {
                int copyCount = Mathf.Min(
                    runtimeSlots.Length,
                    replacement.Length);
                for (int index = 0; index < copyCount; index++)
                {
                    replacement[index] = runtimeSlots[index];
                    if (replacement[index].Active)
                    {
                        copiedActiveCount++;
                        if (replacement[index].Procedural)
                        {
                            copiedProceduralCount++;
                        }
                    }
                }
            }

            runtimeSlots = replacement;
            activeRayCount = copiedActiveCount;
            activeProceduralRayCount = copiedProceduralCount;
            EnsureSurfaceLightStorage(capacity);
        }

        private static bool TryValidateCloudQuery(
            in WeatherLightRayCloudQuery query,
            out string error)
        {
            error = string.Empty;
            if (!IsFinite(query.SearchCentreWorld) ||
                !IsFinite(query.PreferredRayDirectionWorld))
            {
                error = "The cloud-opening query contains non-finite vectors.";
                return false;
            }

            if (!IsFinite(query.MinimumDiameterMetres) ||
                !IsFinite(query.MaximumDiameterMetres) ||
                query.MinimumDiameterMetres <
                    WeatherLightRayAreaLayout.MinimumDiameterMetres ||
                query.MaximumDiameterMetres < query.MinimumDiameterMetres)
            {
                error = "The cloud-opening query diameter range is invalid.";
                return false;
            }

            if (!IsFinite(query.MinimumConfidence) ||
                query.MinimumConfidence < 0f ||
                query.MinimumConfidence > 1f)
            {
                error = "The cloud-opening query confidence is invalid.";
                return false;
            }

            return true;
        }

        private static bool TryBuildCloudOpeningRequest(
            in WeatherLightRayCloudOpening opening,
            in WeatherLightRayCloudSpawnSettings settings,
            out WeatherLightRaySpawnRequest request,
            out string error)
        {
            request = default;
            error = string.Empty;
            if (opening.StableIdentity == 0L)
            {
                error =
                    "A resolved cloud opening requires a non-zero stable identity.";
                return false;
            }

            if (!IsFinite(opening.BaseCentreWorld) ||
                !IsFinite(opening.RayDirectionWorld) ||
                !IsFinite(opening.AreaDiameterMetres) ||
                opening.AreaDiameterMetres <
                    WeatherLightRayAreaLayout.MinimumDiameterMetres)
            {
                error = "The resolved cloud opening geometry is invalid.";
                return false;
            }

            if (!IsFinite(opening.ClearanceStrength) ||
                !IsFinite(opening.EdgeSoftnessSignal) ||
                !IsFinite(opening.Confidence) ||
                opening.ClearanceStrength < 0f ||
                opening.ClearanceStrength > 1f ||
                opening.EdgeSoftnessSignal < 0f ||
                opening.EdgeSoftnessSignal > 1f ||
                opening.Confidence < 0f ||
                opening.Confidence > 1f)
            {
                error = "The resolved cloud opening weights are invalid.";
                return false;
            }

            float resolvedIntensity =
                settings.LocalIntensityMultiplier *
                opening.ClearanceStrength;
            request = new WeatherLightRaySpawnRequest(
                opening.BaseCentreWorld,
                opening.AreaDiameterMetres,
                settings.VariationSeed,
                localIntensityMultiplier: resolvedIntensity,
                lifetimePolicy: settings.LifetimePolicy,
                fadeInDurationSeconds: settings.FadeInDurationSeconds,
                holdDurationSeconds: settings.HoldDurationSeconds,
                fadeOutDurationSeconds: settings.FadeOutDurationSeconds,
                initiallyVisible: settings.InitiallyVisible,
                rayDirectionWorld: opening.RayDirectionWorld,
                sourceKind: opening.SourceKind,
                overrideHeight: settings.OverrideHeight,
                heightMetres: settings.HeightMetres,
                overrideMaximumVisualLean:
                    settings.OverrideMaximumVisualLean,
                maximumVisualLeanDegrees:
                    settings.MaximumVisualLeanDegrees,
                overrideBeamSpacing: settings.OverrideBeamSpacing,
                beamSpacingMetres: settings.BeamSpacingMetres,
                cloudPolicy: settings.RuntimeCloudPolicy,
                sourceGatePolicy: settings.SourceGatePolicy,
                movementPolicy: settings.MovementPolicy,
                gameplayChannel: settings.GameplayChannel,
                externalIdentity: opening.StableIdentity,
                priority: settings.Priority);
            return TryValidateProceduralRequest(request, out error);
        }

        private static bool TryValidateProceduralRequest(
            in WeatherLightRaySpawnRequest request,
            out string error)
        {
            error = string.Empty;
            if (!IsFinite(request.BaseCentreWorld))
            {
                error = "The procedural LightRay base centre is not finite.";
                return false;
            }

            if (!IsFinite(request.RayDirectionWorld))
            {
                error = "The procedural LightRay direction is not finite.";
                return false;
            }

            if (!IsFinite(request.AreaDiameterMetres) ||
                request.AreaDiameterMetres <
                    WeatherLightRayAreaLayout.MinimumDiameterMetres)
            {
                error =
                    "The procedural LightRay area diameter is invalid.";
                return false;
            }

            if (!IsFinite(request.LocalIntensityMultiplier) ||
                request.LocalIntensityMultiplier < 0f)
            {
                error =
                    "The procedural LightRay local intensity multiplier is invalid.";
                return false;
            }

            if (request.OverrideHeight &&
                (!IsFinite(request.HeightMetres) ||
                    request.HeightMetres <= 0f))
            {
                error =
                    "The procedural LightRay height override is invalid.";
                return false;
            }

            if (request.OverrideMaximumVisualLean &&
                (!IsFinite(request.MaximumVisualLeanDegrees) ||
                    request.MaximumVisualLeanDegrees < 0f ||
                    request.MaximumVisualLeanDegrees > 75f))
            {
                error =
                    "The procedural LightRay lean override is invalid.";
                return false;
            }

            if (request.OverrideBeamSpacing &&
                (!IsFinite(request.BeamSpacingMetres) ||
                    request.BeamSpacingMetres <
                        WeatherLightRayAreaLayout.MinimumBeamSpacingMetres ||
                    request.BeamSpacingMetres >
                        WeatherLightRayAreaLayout.MaximumBeamSpacingMetres))
            {
                error =
                    "The procedural LightRay beam-spacing override is invalid.";
                return false;
            }

            if (!IsFinite(request.FadeInDurationSeconds) ||
                !IsFinite(request.HoldDurationSeconds) ||
                !IsFinite(request.FadeOutDurationSeconds) ||
                request.FadeInDurationSeconds < 0f ||
                request.HoldDurationSeconds < 0f ||
                request.FadeOutDurationSeconds < 0f)
            {
                error =
                    "The procedural LightRay lifecycle durations are invalid.";
                return false;
            }

            if (request.LifetimePolicy ==
                    WeatherLightRayLifetimePolicy.Timed &&
                request.FadeInDurationSeconds +
                    request.HoldDurationSeconds +
                    request.FadeOutDurationSeconds <= 0f)
            {
                error =
                    "A timed procedural LightRay requires a positive total lifetime.";
                return false;
            }

            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) &&
                IsFinite(value.y) &&
                IsFinite(value.z);
        }

        private int FindFreeSlotIndex()
        {
            if (runtimeSlots == null)
            {
                return -1;
            }

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (!runtimeSlots[index].Active)
                {
                    return index;
                }
            }

            return -1;
        }

        private bool TryResolveActiveSlot(
            WeatherLightRayHandle handle,
            out int slotIndex)
        {
            slotIndex = -1;
            if (!handle.IsValid ||
                runtimeSlots == null ||
                handle.SlotIndex < 0 ||
                handle.SlotIndex >= runtimeSlots.Length)
            {
                return false;
            }

            RuntimeSlot slot = runtimeSlots[handle.SlotIndex];
            if (!slot.Active || slot.Generation != handle.Generation)
            {
                return false;
            }

            slotIndex = handle.SlotIndex;
            return true;
        }

        private static void ResetEvolutionState(ref RuntimeSlot slot)
        {
            slot.EvolutionInitialized = false;
            slot.EvolutionCurrentSeed = 0u;
            slot.EvolutionNextSeed = 0u;
            slot.EvolutionAuthoredSeed = 0u;
            slot.EvolutionElapsedSeconds = 0.0;
            slot.EvolutionDurationSeconds = 0f;
            slot.EvolutionBlend = 0f;
            slot.CompletedEvolutionTransitions = 0;
        }

        private static uint NextGeneration(uint generation)
        {
            generation++;
            return generation == 0u ? 1u : generation;
        }

        private bool TryResolveAuthoredSlot(
            WeatherLightRayAnchor anchor,
            WeatherLightRayHandle handle,
            out int slotIndex)
        {
            slotIndex = -1;
            if (runtimeSlots == null)
            {
                return false;
            }

            if (handle.IsValid &&
                handle.SlotIndex >= 0 &&
                handle.SlotIndex < runtimeSlots.Length)
            {
                RuntimeSlot slot = runtimeSlots[handle.SlotIndex];
                if (slot.Active &&
                    slot.Generation == handle.Generation &&
                    slot.AuthoredOwner == anchor)
                {
                    slotIndex = handle.SlotIndex;
                    return true;
                }
            }

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (runtimeSlots[index].Active &&
                    runtimeSlots[index].AuthoredOwner == anchor)
                {
                    slotIndex = index;
                    return true;
                }
            }

            return false;
        }

        private void ReleaseSlot(int slotIndex)
        {
            if (runtimeSlots == null ||
                slotIndex < 0 ||
                slotIndex >= runtimeSlots.Length ||
                !runtimeSlots[slotIndex].Active)
            {
                return;
            }

            RuntimeSlot slot = runtimeSlots[slotIndex];
            bool wasProcedural = slot.Procedural;
            slot.Active = false;
            slot.AuthoredOwner = null;
            slot.Procedural = false;
            slot.ProceduralRequest = default;
            slot.ProceduralVisible = false;
            slot.ProceduralRevision = 0u;
            slot.SmoothedGateWeight = 0f;
            slot.LifecycleRevision = 0u;
            slot.LastUpdateTime = 0.0;
            slot.SpawnTime = 0.0;
            slot.EvolutionInitialized = false;
            slot.EvolutionCurrentSeed = 0u;
            slot.EvolutionNextSeed = 0u;
            slot.EvolutionAuthoredSeed = 0u;
            slot.EvolutionElapsedSeconds = 0.0;
            slot.EvolutionDurationSeconds = 0f;
            slot.EvolutionBlend = 0f;
            slot.CompletedEvolutionTransitions = 0;
            slot.Snapshot = default;
            runtimeSlots[slotIndex] = slot;
            activeRayCount = Mathf.Max(0, activeRayCount - 1);
            if (wasProcedural)
            {
                activeProceduralRayCount = Mathf.Max(
                    0,
                    activeProceduralRayCount - 1);
            }
            DisableSurfaceSpotLight(slotIndex);
        }

        private void EnsureSurfaceLightStorage(int requiredCapacity)
        {
            int capacity = Mathf.Max(0, requiredCapacity);
            if (runtimeSurfaceLights != null &&
                runtimeSurfaceLights.Length >= capacity)
            {
                return;
            }

            var replacement = new RuntimeSurfaceLight[capacity];
            if (runtimeSurfaceLights != null)
            {
                System.Array.Copy(
                    runtimeSurfaceLights,
                    replacement,
                    runtimeSurfaceLights.Length);
            }

            runtimeSurfaceLights = replacement;
        }

        private void UpdateSurfaceSpotLights(bool allowCreation)
        {
            activeSurfaceSpotLightCount = 0;
            vegetationAccentOverridesByLight.Clear();
            if (runtimeSlots == null)
            {
                DisableAllSurfaceSpotLights();
                PublishVegetationAccentOverride(
                    Vector3.zero,
                    0f,
                    Vector3.zero,
                    false);
                return;
            }

            EnsureSurfaceLightStorage(runtimeSlots.Length);
            for (int slotIndex = 0;
                slotIndex < runtimeSlots.Length;
                slotIndex++)
            {
                RuntimeSlot slot = runtimeSlots[slotIndex];
                WeatherLightRaySnapshot raySnapshot = slot.Snapshot;
                if (!slot.Active ||
                    raySnapshot.CurrentIntensity <=
                        SurfaceSpotEnableThreshold ||
                    raySnapshot.Descriptor.SurfaceSpotLightIntensity <=
                        SurfaceSpotEnableThreshold)
                {
                    DisableSurfaceSpotLight(slotIndex);
                    continue;
                }

                RuntimeSurfaceLight proxy = GetOrCreateSurfaceSpotLight(
                    slotIndex,
                    allowCreation);
                if (proxy == null)
                {
                    continue;
                }

                WeatherLightRaySourceState renderableSource =
                    ResolveRenderableSourceState(raySnapshot);
                if (UpdateSurfaceSpotLight(
                        proxy,
                        raySnapshot,
                        renderableSource))
                {
                    activeSurfaceSpotLightCount++;
                    if (proxy.Light != null)
                    {
                        // PROTECTED AUTHORITY BOUNDARY:
                        // - values come from the active preset-resolved public
                        //   properties, never the hidden serialized fallbacks;
                        // - body lighting continues to use the real Spot;
                        // - edge selection receives the horizontal source
                        //   direction stored alongside this Light's parameters.
                        bool directionValid =
                            TryResolveVegetationAccentDirection(
                                raySnapshot,
                                renderableSource,
                                out Vector3 accentDirection);
                        vegetationAccentOverridesByLight[
                            proxy.Light.GetEntityId()] =
                                new VegetationAccentOverrideData
                                {
                                    Parameters = new Vector4(
                                        AccentLineResolvedScale,
                                        LightRayVegetationAccentCoverage,
                                        LightRayVegetationAccentSoftness,
                                        1f),
                                    SourceDirectionWS = new Vector4(
                                        accentDirection.x,
                                        accentDirection.y,
                                        accentDirection.z,
                                        directionValid ? 1f : 0f)
                                };
                    }
                }
            }

            // V1.2C3 no longer publishes one production-match Spot. These
            // globals remain inactive for the legacy diagnostic colour path.
            PublishVegetationAccentOverride(
                Vector3.zero,
                0f,
                Vector3.zero,
                false);

            if (runtimeSurfaceLights == null)
            {
                return;
            }

            for (int slotIndex = runtimeSlots.Length;
                slotIndex < runtimeSurfaceLights.Length;
                slotIndex++)
            {
                DisableSurfaceSpotLight(slotIndex);
            }
        }

        // Renderer-facing contract. The two vectors must be copied together
        // into the mirrored GPU record for the SAME camera-visible Light index.
        // Do not publish parameters without source direction, and do not infer
        // direction from the Spot position in the shader.
        public bool TryGetVegetationAccentOverride(
            Light light,
            out Vector4 parameters,
            out Vector4 sourceDirectionWS)
        {
            if (light != null &&
                vegetationAccentOverridesByLight.TryGetValue(
                    light.GetEntityId(),
                    out VegetationAccentOverrideData accentData))
            {
                parameters = accentData.Parameters;
                sourceDirectionWS = accentData.SourceDirectionWS;
                return true;
            }

            parameters = Vector4.zero;
            sourceDirectionWS = Vector4.zero;
            return false;
        }

        public void RecordVegetationAccentSidecarPublication(
            int additionalLightCount,
            int weatherOverrideCount,
            int bufferCapacity,
            bool indexOverflow)
        {
            publishedVegetationAdditionalLightCount = Mathf.Max(
                0,
                additionalLightCount);
            publishedVegetationWeatherOverrideCount = Mathf.Max(
                0,
                weatherOverrideCount);
            publishedVegetationAccentBufferCapacity = Mathf.Max(
                0,
                bufferCapacity);
            publishedVegetationAccentIndexOverflow = indexOverflow;
        }

        private RuntimeSurfaceLight GetOrCreateSurfaceSpotLight(
            int slotIndex,
            bool allowCreation)
        {
            if (runtimeSurfaceLights == null ||
                slotIndex < 0 ||
                slotIndex >= runtimeSurfaceLights.Length)
            {
                return null;
            }

            RuntimeSurfaceLight existing = runtimeSurfaceLights[slotIndex];
            if (existing != null && existing.Light != null)
            {
                return existing;
            }

            if (!allowCreation)
            {
                return null;
            }

            var lightObject = new GameObject(
                $"Weather LightRay Surface Spot [{slotIndex}]");
            lightObject.hideFlags = HideFlags.HideAndDontSave;
            lightObject.transform.SetParent(transform, false);

            Light light = lightObject.AddComponent<Light>();
            light.enabled = false;
            light.type = LightType.Spot;
            light.shadows = LightShadows.None;
            light.shadowStrength = 0f;
            light.renderMode = LightRenderMode.Auto;
            light.lightmapBakeType = LightmapBakeType.Realtime;
            light.cullingMask = ~0;
            light.renderingLayerMask = SurfaceSpotRenderingLayerMask;
            light.bounceIntensity = 0f;
            light.cookie = null;
            light.useColorTemperature = false;

            var created = new RuntimeSurfaceLight
            {
                GameObject = lightObject,
                Light = light
            };
            runtimeSurfaceLights[slotIndex] = created;
            return created;
        }

        private static bool UpdateSurfaceSpotLight(
            RuntimeSurfaceLight proxy,
            WeatherLightRaySnapshot raySnapshot,
            WeatherLightRaySourceState sourceState)
        {
            if (proxy == null || proxy.Light == null)
            {
                return false;
            }

            WeatherLightRayDescriptor descriptor = raySnapshot.Descriptor;
            float radius = Mathf.Max(
                0.1f,
                descriptor.FootprintRadiusMetres);
            float height = Mathf.Max(
                SurfaceSpotMinimumHeightMetres,
                radius * SurfaceSpotHeightRadiusMultiplier);
            float transitionHalfWidth =
                radius *
                SurfaceSpotSoftnessHalfWidthRatio *
                descriptor.FootprintEdgeSoftness;
            float innerRadius = Mathf.Max(
                0f,
                radius - transitionHalfWidth);
            float outerRadius = radius + transitionHalfWidth;
            float innerAngle = Mathf.Clamp(
                2f * Mathf.Atan2(innerRadius, height) * Mathf.Rad2Deg,
                0f,
                179f);
            float outerAngle = Mathf.Clamp(
                2f * Mathf.Atan2(outerRadius, height) * Mathf.Rad2Deg,
                innerAngle,
                179f);
            float maximumReceiverDistance = Mathf.Sqrt(
                height * height + outerRadius * outerRadius);

            Color resolvedColour = ResolveSurfaceSpotLightColour(
                raySnapshot,
                sourceState,
                out float colourPeak);
            float sourceIntensity = sourceState.SourceLight != null
                ? Mathf.Max(0f, sourceState.Intensity)
                : 1f;
            float appliedIntensity =
                descriptor.SurfaceSpotLightIntensity *
                raySnapshot.CurrentIntensity *
                sourceIntensity *
                colourPeak *
                SurfaceSpotReferenceIntensityAtOneMetre *
                height * height;

            Light light = proxy.Light;
            light.renderingLayerMask = SurfaceSpotRenderingLayerMask;
            Transform lightTransform = light.transform;
            lightTransform.SetPositionAndRotation(
                raySnapshot.BaseCentreWorld + Vector3.up * height,
                Quaternion.LookRotation(Vector3.down, Vector3.forward));
            light.color = resolvedColour;
            light.range = Mathf.Max(
                0.01f,
                maximumReceiverDistance * SurfaceSpotRangeMargin);
            light.spotAngle = outerAngle;
            light.innerSpotAngle = innerAngle;
            light.intensity = Mathf.Max(0f, appliedIntensity);
            light.enabled = appliedIntensity > SurfaceSpotEnableThreshold;

            proxy.HeightMetres = height;
            proxy.InnerRadiusMetres = innerRadius;
            proxy.OuterRadiusMetres = outerRadius;
            proxy.AppliedIntensity = light.enabled
                ? appliedIntensity
                : 0f;
            return light.enabled;
        }

        // WEATHER EDGE-DIRECTION CONTRACT.
        // This returns the horizontal direction from vegetation toward the
        // celestial/LightRay source. It is used ONLY by the stylized blade-edge
        // selector. The real Spot's Light.direction remains authoritative for
        // body diffuse, cone attenuation, range, colour, and local-light energy.
        // Never replace this with a direction reconstructed from Spot position.
        private static bool TryResolveVegetationAccentDirection(
            WeatherLightRaySnapshot raySnapshot,
            WeatherLightRaySourceState sourceState,
            out Vector3 accentDirection)
        {
            accentDirection = ProjectHorizontalDirection(
                -raySnapshot.RayDirectionWorld);
            if (accentDirection.sqrMagnitude >=
                VegetationAccentDirectionMinimumLengthSquared)
            {
                accentDirection.Normalize();
                return true;
            }

            accentDirection = ProjectHorizontalDirection(
                sourceState.DirectionToSourceWorld);
            if (accentDirection.sqrMagnitude >=
                VegetationAccentDirectionMinimumLengthSquared)
            {
                accentDirection.Normalize();
                return true;
            }

            accentDirection = Vector3.zero;
            return false;
        }

        private static Vector3 ProjectHorizontalDirection(
            Vector3 direction)
        {
            if (direction.sqrMagnitude <
                VegetationAccentDirectionMinimumLengthSquared)
            {
                return Vector3.zero;
            }

            Vector3 normalized = direction.normalized;
            return normalized -
                Vector3.up * Vector3.Dot(normalized, Vector3.up);
        }


        private static float EvaluateSharedAccentLineRelativeScale(
            float normalizedIntensity)
        {
            float clamped = Mathf.Clamp01(normalizedIntensity);
            if (clamped <= 0f)
            {
                return 0f;
            }

            return SharedAccentLineOutputMultiplier *
                (Mathf.Pow(SharedAccentLineExponentialBase, clamped) - 1f);
        }

        private void MarkSharedAccentLineCacheDirty()
        {
            sharedAccentLineCacheDirty = true;
        }

        // PRESET AUTHORITY CONTRACT — DO NOT BYPASS THIS PROPERTY PATH.
        // While an active preset exists, AccentLineIntensity is the sole
        // authoring authority. The serialized controller field is fallback-only
        // for controllers with no preset. Reading that field directly makes the
        // preset control appear hard-coded and is a production regression.
        private void RefreshSharedAccentLineCacheIfRequired()
        {
            float normalized = lightRaysEnabled
                ? Mathf.Clamp01(AccentLineIntensity)
                : 0f;
            if (!sharedAccentLineCacheDirty &&
                Mathf.Approximately(cachedAccentLineInput, normalized))
            {
                return;
            }

            cachedAccentLineInput = normalized;
            cachedAccentLineNormalized = normalized;
            cachedAccentLineResolvedScale =
                EvaluateSharedAccentLineRelativeScale(normalized);
            sharedAccentLineCacheDirty = false;
        }

        private void PublishSharedAccentLineIntensity()
        {
            if (PublishedController != this)
            {
                return;
            }

            RefreshSharedAccentLineCacheIfRequired();
            Shader.SetGlobalFloat(
                AccentLineIntensityId,
                cachedAccentLineNormalized);
            Shader.SetGlobalFloat(
                AccentLineResolvedScaleId,
                cachedAccentLineResolvedScale);
            Shader.SetGlobalFloat(
                VegetationAccentCoverageId,
                lightRaysEnabled
                    ? Mathf.Clamp01(LightRayVegetationAccentCoverage)
                    : 0f);
        }

        private void PublishVegetationAccentOverride(
            Vector3 spotPosition,
            float spotRange,
            Vector3 direction,
            bool active)
        {
            bool valid = active &&
                spotRange > SurfaceSpotEnableThreshold &&
                direction.sqrMagnitude >=
                    VegetationAccentDirectionMinimumLengthSquared;
            publishedVegetationAccentSpotPosition = valid
                ? spotPosition
                : Vector3.zero;
            publishedVegetationAccentSpotRange = valid
                ? spotRange
                : 0f;
            publishedVegetationAccentDirection = valid
                ? direction.normalized
                : Vector3.zero;
            vegetationAccentOverrideActive = valid;
            Shader.SetGlobalVector(
                VegetationAccentSpotPositionId,
                new Vector4(
                    publishedVegetationAccentSpotPosition.x,
                    publishedVegetationAccentSpotPosition.y,
                    publishedVegetationAccentSpotPosition.z,
                    publishedVegetationAccentSpotRange));
            Shader.SetGlobalVector(
                VegetationAccentDirectionId,
                new Vector4(
                    publishedVegetationAccentDirection.x,
                    publishedVegetationAccentDirection.y,
                    publishedVegetationAccentDirection.z,
                    valid ? 1f : 0f));
        }

        private void PublishVegetationAccentDiagnosticMode(bool active)
        {
            if (PublishedController != this)
            {
                return;
            }

            Shader.SetGlobalFloat(
                VegetationAccentDiagnosticModeId,
                active ? 1f : 0f);
        }

        private static Color ResolveSurfaceSpotLightColour(
            WeatherLightRaySnapshot raySnapshot,
            WeatherLightRaySourceState sourceState,
            out float colourPeak)
        {
            Color sourceColour = sourceState.SourceLight != null
                ? sourceState.Colour
                : Color.white;
            if (raySnapshot.SourceKind == WeatherLightRaySourceKind.Sun)
            {
                Color warmSunColour = new Color(
                    1f,
                    0.76f,
                    0.46f,
                    1f);
                sourceColour = Color.Lerp(
                    sourceColour,
                    warmSunColour,
                    raySnapshot.Descriptor.WarmthContribution);
            }

            Color effectiveColour = sourceColour *
                raySnapshot.Descriptor.ColourMultiplier;
            colourPeak = Mathf.Max(
                0f,
                Mathf.Max(
                    effectiveColour.r,
                    Mathf.Max(effectiveColour.g, effectiveColour.b)));
            if (colourPeak <= 0.0001f)
            {
                colourPeak = 0f;
                return Color.black;
            }

            return new Color(
                Mathf.Max(0f, effectiveColour.r) / colourPeak,
                Mathf.Max(0f, effectiveColour.g) / colourPeak,
                Mathf.Max(0f, effectiveColour.b) / colourPeak,
                1f);
        }

        private void DisableSurfaceSpotLight(int slotIndex)
        {
            if (runtimeSurfaceLights == null ||
                slotIndex < 0 ||
                slotIndex >= runtimeSurfaceLights.Length)
            {
                return;
            }

            RuntimeSurfaceLight proxy = runtimeSurfaceLights[slotIndex];
            if (proxy == null || proxy.Light == null)
            {
                return;
            }

            proxy.Light.enabled = false;
            proxy.AppliedIntensity = 0f;
        }

        private void DisableAllSurfaceSpotLights()
        {
            activeSurfaceSpotLightCount = 0;
            if (runtimeSurfaceLights == null)
            {
                return;
            }

            for (int slotIndex = 0;
                slotIndex < runtimeSurfaceLights.Length;
                slotIndex++)
            {
                DisableSurfaceSpotLight(slotIndex);
            }
        }

        private void DestroyAllSurfaceSpotLights()
        {
            activeSurfaceSpotLightCount = 0;
            if (runtimeSurfaceLights == null)
            {
                return;
            }

            for (int slotIndex = 0;
                slotIndex < runtimeSurfaceLights.Length;
                slotIndex++)
            {
                RuntimeSurfaceLight proxy = runtimeSurfaceLights[slotIndex];
                if (proxy == null)
                {
                    continue;
                }

                if (proxy.GameObject != null)
                {
                    CoreUtils.Destroy(proxy.GameObject);
                }

                runtimeSurfaceLights[slotIndex] = null;
            }

            runtimeSurfaceLights = null;
        }

        private void ResolveRenderCamera()
        {
            if (renderCameraOverride != null &&
                renderCameraOverride.isActiveAndEnabled)
            {
                resolvedRenderCamera = renderCameraOverride;
                return;
            }

            if (cachedMainCamera == null ||
                !cachedMainCamera.isActiveAndEnabled)
            {
                cachedMainCamera = Camera.main;
            }

            resolvedRenderCamera = cachedMainCamera;
        }

        private void UpdateRegisteredRays()
        {
            if (runtimeSlots == null)
            {
                return;
            }

            double now = Time.realtimeSinceStartupAsDouble;
            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                RuntimeSlot slot = runtimeSlots[index];
                if (!slot.Active)
                {
                    continue;
                }

                if (slot.Procedural)
                {
                    UpdateProceduralSlot(index, now);
                    continue;
                }

                WeatherLightRayAnchor owner = slot.AuthoredOwner;
                if (owner == null || !owner.isActiveAndEnabled)
                {
                    ReleaseSlot(index);
                    continue;
                }

                UpdateAuthoredSlot(index, owner, now);
            }
        }

        private void UpdateAuthoredSlot(
            int slotIndex,
            WeatherLightRayAnchor anchor,
            double now)
        {
            RuntimeSlot slot = runtimeSlots[slotIndex];
            WeatherLightRayDescriptor descriptor = anchor.BuildDescriptor(
                EvolutionPreset,
                EvolutionStrength,
                EvolutionSpeed);
            if (activePreset != null)
            {
                descriptor = activePreset.ApplyTo(
                    descriptor,
                    anchor.OverridePresetBeamSpacing,
                    anchor.BeamSpacingMetres,
                    anchor.LocalIntensityMultiplier,
                    previousPresentationPreset,
                    PresetPresentationBlend);
            }

            if (slot.LifecycleRevision != anchor.LifecycleRevision)
            {
                slot.LifecycleRevision = anchor.LifecycleRevision;
                slot.SpawnTime = now;
            }

            UpdateRuntimeSlot(
                slotIndex,
                descriptor,
                anchor.transform.position,
                Vector3.zero,
                anchor.ExternallyControlledVisible,
                now,
                false);
        }

        private void UpdateProceduralSlot(int slotIndex, double now)
        {
            RuntimeSlot slot = runtimeSlots[slotIndex];
            if (!slot.Active || !slot.Procedural)
            {
                return;
            }

            if (activePreset == null)
            {
                lastError =
                    "An active procedural LightRay requires an assigned Active Preset.";
                ReleaseSlot(slotIndex);
                return;
            }

            WeatherLightRaySpawnRequest request = slot.ProceduralRequest;
            WeatherLightRayDescriptor descriptor =
                BuildProceduralDescriptor(
                    request,
                    PresetPresentationBlend);
            UpdateRuntimeSlot(
                slotIndex,
                descriptor,
                request.BaseCentreWorld,
                request.RayDirectionWorld,
                slot.ProceduralVisible,
                now,
                true);
        }

        private WeatherLightRayDescriptor BuildProceduralDescriptor(
            in WeatherLightRaySpawnRequest request,
            float presentationBlend)
        {
            float height = request.OverrideHeight
                ? request.HeightMetres
                : activePreset.DefaultHeightMetres;
            float lean = request.OverrideMaximumVisualLean
                ? request.MaximumVisualLeanDegrees
                : activePreset.DefaultMaximumVisualLeanDegrees;
            float spacing = request.OverrideBeamSpacing
                ? request.BeamSpacingMetres
                : activePreset.BeamSpacingMetres;
            var localDescriptor = new WeatherLightRayDescriptor(
                request.SourceKind,
                WeatherLightRayOriginKind.Procedural,
                request.CloudPolicy,
                request.LifetimePolicy,
                request.SourceGatePolicy,
                request.MovementPolicy,
                height,
                lean,
                request.AreaDiameterMetres,
                spacing,
                activePreset.BeamWidthRatioRange,
                activePreset.BeamIntensityVariation,
                activePreset.BeamEdgeSoftness,
                activePreset.BeamSoftnessVariation,
                activePreset.UpperFade,
                activePreset.GroundFade,
                activePreset.ContactPlaneOpacity,
                activePreset.ColourMultiplier,
                activePreset.WarmthContribution,
                activePreset.AtmosphericIntensity,
                activePreset.SofteningStrength,
                activePreset.CameraIntersectionFade,
                activePreset.SurfaceSpotLightIntensity,
                activePreset.ScreenSpaceSurfaceIntensity,
                activePreset.FootprintEdgeSoftness,
                activePreset.EvolutionPreset,
                activePreset.EvolutionStrength,
                activePreset.EvolutionSpeed,
                request.FadeInDurationSeconds,
                request.HoldDurationSeconds,
                request.FadeOutDurationSeconds,
                request.GameplayChannel,
                request.VariationSeed);
            return activePreset.ApplyTo(
                localDescriptor,
                request.OverrideBeamSpacing,
                request.BeamSpacingMetres,
                request.LocalIntensityMultiplier,
                previousPresentationPreset,
                presentationBlend);
        }

        private void UpdateRuntimeSlot(
            int slotIndex,
            WeatherLightRayDescriptor descriptor,
            Vector3 baseCentreWorld,
            Vector3 rayDirectionOverride,
            bool externallyControlledVisible,
            double now,
            bool releaseTimedExpiry)
        {
            RuntimeSlot slot = runtimeSlots[slotIndex];
            WeatherLightRaySourceState sourceState = GetSourceState(
                descriptor.SourceKind);
            float sourceWeight = 0f;
            if (lightRaysEnabled)
            {
                sourceWeight = descriptor.SourceGatePolicy ==
                    WeatherLightRaySourceGatePolicy.IgnoreSourceGate
                        ? 1f
                        : sourceState.Available
                            ? sourceState.AvailabilityWeight
                            : 0f;
            }

            float cloudTransmission = 1f;
            float cloudOpenWeight = 1f;
            WeatherCloudTransmissionSample sample = default;
            bool cloudSampleAvailable = sourceState.SourceLight != null &&
                TrySampleCloudTransmission(
                    baseCentreWorld,
                    descriptor.SourceKind,
                    out sample) &&
                sample.IsUsable;
            if (cloudSampleAvailable)
            {
                cloudTransmission = sample.Transmission;
            }

            if (descriptor.CloudPolicy ==
                WeatherLightRayCloudPolicy.RespectClouds)
            {
                WeatherCloudShadowController cloudController =
                    WeatherCloudShadowController.PublishedController;
                bool unstableBelowResumeThreshold =
                    cloudSampleAvailable &&
                    !sample.IsStable &&
                    cloudController != null &&
                    cloudController.EvolutionInProgress &&
                    cloudController.EvolutionProgress <
                        cloudEvolutionResumeThreshold;
                if (cloudSampleAvailable &&
                    !unstableBelowResumeThreshold)
                {
                    float shaded = cloudController != null
                        ? cloudController.ShadedTransmission
                        : 0f;
                    cloudOpenWeight = Mathf.Clamp01(
                        (cloudTransmission - shaded) /
                        Mathf.Max(0.0001f, 1f - shaded));
                }
                else if (unstableBelowResumeThreshold)
                {
                    cloudOpenWeight = 0f;
                }
                else
                {
                    cloudTransmission = 0f;
                    cloudOpenWeight = 0f;
                    if (sourceState.SourceLight == null)
                    {
                        lastError =
                            "A cloud-respecting LightRay requires an authoritative directional source light.";
                    }
                    else if (!string.IsNullOrEmpty(sample.Error))
                    {
                        lastError = sample.Error;
                    }
                }
            }

            float externalWeight = descriptor.LifetimePolicy ==
                WeatherLightRayLifetimePolicy.ExternallyControlled &&
                !externallyControlledVisible
                    ? 0f
                    : 1f;
            float gateTarget = sourceWeight * cloudOpenWeight *
                externalWeight;
            bool firstUpdate = slot.LastUpdateTime <= 0.0;
            double deltaTime = firstUpdate
                ? 0.0
                : System.Math.Max(0.0, now - slot.LastUpdateTime);
            float responseDuration = gateTarget >= slot.SmoothedGateWeight
                ? descriptor.FadeInDuration
                : descriptor.FadeOutDuration;
            if (firstUpdate && descriptor.LifetimePolicy ==
                WeatherLightRayLifetimePolicy.Timed)
            {
                slot.SmoothedGateWeight = gateTarget;
            }
            else if (responseDuration <= 0.0001f)
            {
                slot.SmoothedGateWeight = gateTarget;
            }
            else
            {
                slot.SmoothedGateWeight = Mathf.MoveTowards(
                    slot.SmoothedGateWeight,
                    gateTarget,
                    (float)(deltaTime / responseDuration));
            }

            slot.LastUpdateTime = now;
            float lifecycleWeight = EvaluateLifecycleWeight(
                descriptor,
                slot.SpawnTime,
                now,
                out WeatherLightRayLifecycleState lifecycleState,
                out double holdOrExpiryTime);
            float currentIntensity = Mathf.Clamp01(
                slot.SmoothedGateWeight * lifecycleWeight);
            lifecycleState = ResolveLifecycleState(
                descriptor,
                externallyControlledVisible,
                lifecycleState,
                lifecycleWeight,
                slot.SmoothedGateWeight,
                gateTarget,
                currentIntensity);

            Vector3 baseDirection = rayDirectionOverride.sqrMagnitude >
                0.000001f
                    ? rayDirectionOverride.normalized
                    : sourceState.RayDirectionWorld;
            Vector3 presentationDirection = ResolvePresentationDirection(
                baseDirection,
                sourceState.Profile,
                descriptor.MaximumVisualLeanDegrees);
            UpdateEvolutionState(ref slot, descriptor, deltaTime);
            WeatherLightRayHandle handle = new WeatherLightRayHandle(
                slotIndex,
                slot.Generation);
            slot.Snapshot = new WeatherLightRaySnapshot(
                handle,
                descriptor,
                lifecycleState,
                baseCentreWorld,
                presentationDirection,
                slot.SpawnTime,
                holdOrExpiryTime,
                currentIntensity,
                cloudTransmission,
                slot.EvolutionCurrentSeed,
                slot.EvolutionNextSeed,
                slot.EvolutionBlend,
                slot.EvolutionDurationSeconds,
                slot.CompletedEvolutionTransitions);
            runtimeSlots[slotIndex] = slot;

            if (releaseTimedExpiry &&
                descriptor.LifetimePolicy ==
                    WeatherLightRayLifetimePolicy.Timed &&
                lifecycleState == WeatherLightRayLifecycleState.Inactive)
            {
                ReleaseSlot(slotIndex);
            }
        }

        private static float ResolveEvolutionStrength(
            WeatherLightRayEvolutionPreset preset,
            float customStrength)
        {
            switch (preset)
            {
                case WeatherLightRayEvolutionPreset.Static:
                    return 0f;
                case WeatherLightRayEvolutionPreset.Subtle:
                    return 0.35f;
                case WeatherLightRayEvolutionPreset.Living:
                    return 0.65f;
                default:
                    return Mathf.Clamp01(customStrength);
            }
        }

        private static float ResolveEvolutionSpeed(
            WeatherLightRayEvolutionPreset preset,
            float customSpeed)
        {
            switch (preset)
            {
                case WeatherLightRayEvolutionPreset.Static:
                    return 0f;
                case WeatherLightRayEvolutionPreset.Subtle:
                    return 0.25f;
                case WeatherLightRayEvolutionPreset.Living:
                    return 0.50f;
                default:
                    return Mathf.Clamp01(customSpeed);
            }
        }

        private static void UpdateEvolutionState(
            ref RuntimeSlot slot,
            WeatherLightRayDescriptor descriptor,
            double deltaTime)
        {
            uint authoredSeed = descriptor.VariationSeed == 0u
                ? 1u
                : descriptor.VariationSeed;
            float strength = Mathf.Clamp01(descriptor.EvolutionStrength);
            float speed = Mathf.Clamp01(descriptor.EvolutionSpeed);
            bool staticEvolution = strength <= 0.0001f || speed <= 0.0001f;

            if (!slot.EvolutionInitialized ||
                slot.EvolutionAuthoredSeed != authoredSeed)
            {
                slot.EvolutionInitialized = true;
                slot.EvolutionAuthoredSeed = authoredSeed;
                slot.EvolutionCurrentSeed = authoredSeed;
                slot.EvolutionNextSeed = NextEvolutionSeed(authoredSeed);
                slot.EvolutionElapsedSeconds = 0.0;
                slot.EvolutionDurationSeconds = staticEvolution
                    ? 0f
                    : ResolveEvolutionDurationSeconds(speed);
                slot.EvolutionBlend = 0f;
                slot.CompletedEvolutionTransitions = 0;
            }

            if (staticEvolution)
            {
                slot.EvolutionCurrentSeed = authoredSeed;
                slot.EvolutionNextSeed = authoredSeed;
                slot.EvolutionElapsedSeconds = 0.0;
                slot.EvolutionDurationSeconds = 0f;
                slot.EvolutionBlend = 0f;
                slot.CompletedEvolutionTransitions = 0;
                return;
            }

            float duration = ResolveEvolutionDurationSeconds(speed);
            if (slot.EvolutionDurationSeconds <= 0.0001f ||
                slot.EvolutionNextSeed == slot.EvolutionCurrentSeed)
            {
                slot.EvolutionNextSeed = NextEvolutionSeed(
                    slot.EvolutionCurrentSeed);
                slot.EvolutionElapsedSeconds = 0.0;
                slot.EvolutionBlend = 0f;
            }
            slot.EvolutionDurationSeconds = duration;
            slot.EvolutionElapsedSeconds += System.Math.Max(0.0, deltaTime);
            while (slot.EvolutionElapsedSeconds >= duration)
            {
                slot.EvolutionElapsedSeconds -= duration;
                slot.EvolutionCurrentSeed = slot.EvolutionNextSeed;
                slot.EvolutionNextSeed = NextEvolutionSeed(
                    slot.EvolutionCurrentSeed);
                slot.CompletedEvolutionTransitions++;
            }

            float linearBlend = Mathf.Clamp01(
                (float)(slot.EvolutionElapsedSeconds / duration));
            slot.EvolutionBlend = linearBlend * linearBlend *
                (3f - 2f * linearBlend);
        }

        private static float ResolveEvolutionDurationSeconds(float speed)
        {
            float normalizedSpeed = Mathf.Clamp01(speed);
            return 3f * Mathf.Pow(2f, 4f * (1f - normalizedSpeed));
        }

        private static uint NextEvolutionSeed(uint seed)
        {
            uint value = seed == 0u ? 1u : seed;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            return value == 0u ? 1u : value;
        }

        private static float EvaluateLifecycleWeight(
            WeatherLightRayDescriptor descriptor,
            double spawnTime,
            double now,
            out WeatherLightRayLifecycleState state,
            out double holdOrExpiryTime)
        {
            if (descriptor.LifetimePolicy !=
                WeatherLightRayLifetimePolicy.Timed)
            {
                state = WeatherLightRayLifecycleState.Holding;
                holdOrExpiryTime = double.PositiveInfinity;
                return 1f;
            }

            double elapsed = System.Math.Max(0.0, now - spawnTime);
            double fadeInEnd = descriptor.FadeInDuration;
            double holdEnd = fadeInEnd + descriptor.HoldDuration;
            double expiry = holdEnd + descriptor.FadeOutDuration;
            holdOrExpiryTime = spawnTime + expiry;

            if (descriptor.FadeInDuration > 0.0001f &&
                elapsed < fadeInEnd)
            {
                state = WeatherLightRayLifecycleState.FadingIn;
                return Mathf.Clamp01(
                    (float)(elapsed / descriptor.FadeInDuration));
            }

            if (elapsed < holdEnd)
            {
                state = WeatherLightRayLifecycleState.Holding;
                return 1f;
            }

            if (descriptor.FadeOutDuration > 0.0001f &&
                elapsed < expiry)
            {
                state = WeatherLightRayLifecycleState.FadingOut;
                return Mathf.Clamp01(
                    1f - (float)((elapsed - holdEnd) /
                        descriptor.FadeOutDuration));
            }

            state = WeatherLightRayLifecycleState.Inactive;
            return 0f;
        }

        private static WeatherLightRayLifecycleState ResolveLifecycleState(
            WeatherLightRayDescriptor descriptor,
            bool externallyControlledVisible,
            WeatherLightRayLifecycleState lifecycleState,
            float lifecycleWeight,
            float smoothedGateWeight,
            float gateTarget,
            float currentIntensity)
        {
            if (descriptor.LifetimePolicy ==
                WeatherLightRayLifetimePolicy.Timed)
            {
                if (lifecycleState == WeatherLightRayLifecycleState.Inactive)
                {
                    return lifecycleState;
                }

                if (currentIntensity <= 0.0001f &&
                    gateTarget <= 0.0001f)
                {
                    return WeatherLightRayLifecycleState.Suspended;
                }

                return lifecycleState;
            }

            if (descriptor.LifetimePolicy ==
                WeatherLightRayLifetimePolicy.ExternallyControlled &&
                !externallyControlledVisible)
            {
                return currentIntensity > 0.0001f
                    ? WeatherLightRayLifecycleState.FadingOut
                    : WeatherLightRayLifecycleState.Inactive;
            }

            if (smoothedGateWeight + 0.0001f < gateTarget)
            {
                return WeatherLightRayLifecycleState.FadingIn;
            }

            if (smoothedGateWeight > gateTarget + 0.0001f)
            {
                return WeatherLightRayLifecycleState.FadingOut;
            }

            if (currentIntensity <= 0.0001f || lifecycleWeight <= 0.0001f)
            {
                return WeatherLightRayLifecycleState.Suspended;
            }

            return WeatherLightRayLifecycleState.Holding;
        }

        private static Vector3 ResolvePresentationDirection(
            Vector3 sourceRayDirection,
            WeatherLightRaySourceProfile profile,
            float descriptorMaximumLeanDegrees)
        {
            Vector3 safeDirection = sourceRayDirection.sqrMagnitude > 0.000001f
                ? sourceRayDirection.normalized
                : Vector3.down;
            float maximumLean = Mathf.Clamp(
                descriptorMaximumLeanDegrees,
                0f,
                75f);
            if (profile != null)
            {
                maximumLean = Mathf.Min(
                    maximumLean,
                    profile.MaximumPresentationLeanDegrees);
            }

            float angle = Vector3.Angle(Vector3.down, safeDirection);
            if (angle <= maximumLean || angle <= 0.0001f)
            {
                return safeDirection;
            }

            return Vector3.Slerp(
                Vector3.down,
                safeDirection,
                maximumLean / angle).normalized;
        }

        private WeatherLightRaySourceState GetSourceState(
            WeatherLightRaySourceKind sourceKind)
        {
            switch (sourceKind)
            {
                case WeatherLightRaySourceKind.Sun:
                    return sunSourceState;
                case WeatherLightRaySourceKind.Moon:
                    return moonSourceState;
                default:
                    return independentSourceState;
            }
        }

        /// <summary>
        /// Resolves one Selection Profile dependency without consulting visual
        /// preset metadata. Controller-direction entries use the authoritative
        /// bound source state. Vertical/fixed entries are Independent and must
        /// ignore source gating. Cloud projection remains a separate dependency.
        /// </summary>
        internal bool TryResolveSelectionDependency(
            WeatherLightRaySelectionProfile.Entry entry,
            out WeatherLightRayResolvedSelectionDependency resolved)
        {
            resolved = default;
            if (entry == null)
            {
                return false;
            }

            WeatherLightRaySourceKind sourceKind;
            Vector3 rayDirection;
            WeatherLightRaySourceGatePolicy sourceGatePolicy;
            float availabilityWeight;
            bool valid;
            string failureReason = string.Empty;
            Light sourceLight = null;

            switch (entry.DirectionMode)
            {
                case WeatherLightRayDirectionMode.Vertical:
                    sourceKind = WeatherLightRaySourceKind.Independent;
                    rayDirection = Vector3.down;
                    sourceGatePolicy =
                        WeatherLightRaySourceGatePolicy.IgnoreSourceGate;
                    availabilityWeight = 1f;
                    valid = true;
                    break;
                case WeatherLightRayDirectionMode.FixedWorldDirection:
                    sourceKind = WeatherLightRaySourceKind.Independent;
                    rayDirection = entry.FixedWorldDirection.sqrMagnitude >
                        0.000001f
                            ? entry.FixedWorldDirection.normalized
                            : Vector3.down;
                    sourceGatePolicy =
                        WeatherLightRaySourceGatePolicy.IgnoreSourceGate;
                    availabilityWeight = 1f;
                    valid = true;
                    break;
                default:
                    sourceKind = entry.SourceKind;
                    WeatherLightRaySourceState state = GetSourceState(
                        sourceKind);
                    sourceLight = state.SourceLight;
                    rayDirection = state.RayDirectionWorld.sqrMagnitude >
                        0.000001f
                            ? state.RayDirectionWorld.normalized
                            : Vector3.down;
                    sourceGatePolicy =
                        WeatherLightRaySourceGatePolicy.RequireActiveSource;
                    if (entry.SourceAvailabilityPolicy ==
                        WeatherLightRaySourceAvailabilityPolicy.Ignore)
                    {
                        availabilityWeight = 0f;
                        valid = false;
                        failureReason =
                            "Controller Directional Source cannot ignore source availability.";
                    }
                    else if (entry.SourceAvailabilityPolicy ==
                        WeatherLightRaySourceAvailabilityPolicy.
                            MultiplyActivation)
                    {
                        availabilityWeight = state.AvailabilityWeight;
                        valid = state.SourceLight != null &&
                            availabilityWeight > 0f;
                        failureReason = valid
                            ? string.Empty
                            : state.UnavailableReason;
                    }
                    else
                    {
                        availabilityWeight = state.Available ? 1f : 0f;
                        valid = state.Available && state.SourceLight != null;
                        failureReason = valid
                            ? string.Empty
                            : state.UnavailableReason;
                    }
                    break;
            }

            Light cloudProjectionLight = null;
            if (entry.CloudProjectionMode ==
                WeatherLightRayCloudProjectionMode.
                    CloudControllerDirectionalSource)
            {
                WeatherCloudShadowController cloud =
                    WeatherCloudShadowController.PublishedController;
                cloudProjectionLight = cloud != null
                    ? cloud.ResolvedSun
                    : null;
            }

            ulong signature = 0x9E3779B97F4A7C15UL;
            signature = MixDependencyHash(
                signature ^ (ulong)entry.DirectionMode + 1UL);
            signature = MixDependencyHash(
                signature ^ (ulong)sourceKind + 1UL);
            signature = MixDependencyHash(
                signature ^ (ulong)entry.SourceAvailabilityPolicy + 1UL);
            signature = MixDependencyHash(
                signature ^ (ulong)entry.CloudProjectionMode + 1UL);
            // Controller-source direction changes continuously as the Time Of
            // Day rig moves. It is live instance data, not a dependency-context
            // change, and must not retire otherwise compatible populations.
            // Only authored source-independent direction modes participate in
            // the continuity signature.
            if (entry.DirectionMode !=
                WeatherLightRayDirectionMode.ControllerDirectionalSource)
            {
                signature = MixDependencyHash(
                    signature ^ HashVectorDirection(rayDirection));
            }
            if (sourceLight != null)
            {
                signature = MixDependencyHash(
                    signature ^ unchecked((ulong)(uint)
                        sourceLight.GetEntityId().GetHashCode()));
            }
            if (cloudProjectionLight != null)
            {
                signature = MixDependencyHash(
                    signature ^ unchecked((ulong)(uint)
                        cloudProjectionLight.GetEntityId().GetHashCode()));
            }

            resolved = new WeatherLightRayResolvedSelectionDependency(
                sourceKind,
                rayDirection,
                sourceGatePolicy,
                cloudProjectionLight,
                availabilityWeight,
                signature,
                valid,
                failureReason);
            return valid;
        }

        private static ulong HashVectorDirection(Vector3 direction)
        {
            Vector3 normalized = direction.sqrMagnitude > 0.000001f
                ? direction.normalized
                : Vector3.down;
            ulong x = unchecked((ulong)(uint)Mathf.RoundToInt(
                normalized.x * 10000f));
            ulong y = unchecked((ulong)(uint)Mathf.RoundToInt(
                normalized.y * 10000f));
            ulong z = unchecked((ulong)(uint)Mathf.RoundToInt(
                normalized.z * 10000f));
            return MixDependencyHash(x ^ (y << 21) ^ (z << 42));
        }

        private static ulong HashStableText(string value)
        {
            ulong hash = 1469598103934665603UL;
            if (!string.IsNullOrEmpty(value))
            {
                for (int index = 0; index < value.Length; index++)
                {
                    hash ^= value[index];
                    hash *= 1099511628211UL;
                }
            }
            return MixDependencyHash(hash);
        }

        private static ulong MixDependencyHash(ulong value)
        {
            value ^= value >> 30;
            value *= 0xBF58476D1CE4E5B9UL;
            value ^= value >> 27;
            value *= 0x94D049BB133111EBUL;
            value ^= value >> 31;
            return value;
        }

        private void ResolveSourceStates()
        {
            Light sun = sunOverride != null
                ? sunOverride
                : RenderSettings.sun;
            sunSourceState = ResolveDirectionalSourceState(
                WeatherLightRaySourceKind.Sun,
                sun,
                sunProfile);
            moonSourceState = new WeatherLightRaySourceState(
                WeatherLightRaySourceKind.Moon,
                null,
                null,
                Vector3.down,
                Vector3.up,
                Color.black,
                0f,
                -1f,
                0f,
                false,
                "Moon source unavailable: Time of Day has no approved Moon light contract.");
            independentSourceState = new WeatherLightRaySourceState(
                WeatherLightRaySourceKind.Independent,
                null,
                null,
                Vector3.down,
                Vector3.up,
                Color.white,
                1f,
                1f,
                1f,
                true,
                string.Empty);
        }

        private static WeatherLightRaySourceState
            ResolveDirectionalSourceState(
                WeatherLightRaySourceKind kind,
                Light sourceLight,
                WeatherLightRaySourceProfile profile)
        {
            if (sourceLight == null)
            {
                return new WeatherLightRaySourceState(
                    kind,
                    null,
                    profile,
                    Vector3.down,
                    Vector3.up,
                    Color.black,
                    0f,
                    -1f,
                    0f,
                    false,
                    "No authoritative directional source light is available.");
            }

            Vector3 rayDirection = sourceLight.transform.forward.normalized;
            Vector3 directionToSource = -rayDirection;
            float elevation = Vector3.Dot(
                directionToSource,
                Vector3.up);
            bool available;
            string unavailableReason;

            if (profile != null)
            {
                if (profile.SourceKind != kind)
                {
                    available = false;
                    unavailableReason =
                        "The assigned LightRay source profile kind does not match the source binding.";
                }
                else
                {
                    available = profile.EvaluateAvailability(
                        sourceLight,
                        elevation,
                        out unavailableReason);
                }
            }
            else
            {
                available = sourceLight.type == LightType.Directional &&
                    sourceLight.enabled &&
                    sourceLight.gameObject.activeInHierarchy &&
                    sourceLight.intensity >=
                        FallbackMinimumSourceIntensity &&
                    elevation >= FallbackMinimumSourceElevation;
                unavailableReason = available
                    ? string.Empty
                    : "The source failed the fallback intensity, activity, direction, or horizon gate.";
            }

            float minimumElevation = profile != null
                ? profile.MinimumSourceElevation
                : FallbackMinimumSourceElevation;
            float elevationFadeRange = profile != null
                ? profile.ElevationFadeRange
                : 0.15f;
            float availabilityWeight = available
                ? Mathf.Clamp01(
                    (elevation - minimumElevation) /
                    Mathf.Max(0.001f, elevationFadeRange))
                : 0f;

            Color colour = sourceLight.color;
            if (profile != null)
            {
                colour *= profile.ColourMultiplier;
            }

            return new WeatherLightRaySourceState(
                kind,
                sourceLight,
                profile,
                rayDirection,
                directionToSource,
                colour,
                sourceLight.intensity,
                elevation,
                availabilityWeight,
                available,
                unavailableReason);
        }

        private void ResolveProjectionFocus()
        {
            Transform focus;
            ProbeFocusSource source;
            Vector3 centre;
            if (projectionProbeFocusOverride != null)
            {
                focus = projectionProbeFocusOverride;
                source = ProbeFocusSource.InspectorOverride;
                centre = focus.position;
            }
            else
            {
                WeatherCloudShadowController cloudController =
                    WeatherCloudShadowController.PublishedController;
                if (cloudController != null)
                {
                    focus = cloudController.EffectiveDebugOverlayFocus;
                    source = ProbeFocusSource.CloudDebugOverlay;
                    centre = cloudController.EffectiveDebugOverlayCentre;
                }
                else if (projectionProbeFallbackCamera != null)
                {
                    focus = projectionProbeFallbackCamera.transform;
                    source = ProbeFocusSource.AssignedFallbackCamera;
                    centre = focus.position;
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
                        source = ProbeFocusSource.AutomaticMainCamera;
                        centre = focus.position;
                    }
                    else
                    {
                        focus = transform;
                        source = ProbeFocusSource.ControllerFallback;
                        centre = transform.position;
                    }
                }
            }

            resolvedProbeFocus = focus;
            resolvedProbeFocusSource = source;
            resolvedProbeCentre = centre;
            resolvedProbeCentre.y = projectionProbeSampleHeightMetres;
        }

        private void AppendProjectionDiagnostic(
            StringBuilder builder,
            WeatherCloudShadowController cloudController)
        {
            builder.AppendLine("[CPU Cloud Projection Probe]");
            if (sunSourceState.SourceLight == null)
            {
                builder.AppendLine(
                    "Unavailable: no Sun light can define the projection plane.");
                return;
            }

            if (cloudController == null)
            {
                builder.AppendLine(
                    "Clear sky: no published cloud controller participates.");
                return;
            }

            float maximumInstalledOffsetDelta = 0f;
            int usableSamples = 0;
            int unstableSamples = 0;
            int failedSamples = 0;
            for (int y = 0; y < projectionProbeGridResolution; y++)
            {
                for (int x = 0; x < projectionProbeGridResolution; x++)
                {
                    Vector3 position = GetProjectionProbeWorldPosition(x, y);
                    bool success = cloudController.TrySampleCloudTransmission(
                        position,
                        sunSourceState.SourceLight,
                        out WeatherCloudTransmissionSample sample);
                    builder.Append(x)
                        .Append(',')
                        .Append(y)
                        .Append(" | WS ")
                        .Append(position.ToString("F2"))
                        .Append(" | ")
                        .Append(sample.Status)
                        .Append(" | T ")
                        .Append(sample.Transmission.ToString("0.000"))
                        .Append(" | UV ")
                        .AppendLine(sample.CookieUv.ToString("F4"));

                    if (!success)
                    {
                        failedSamples++;
                        continue;
                    }

                    usableSamples++;
                    if (!sample.IsStable)
                    {
                        unstableSamples++;
                    }

                    if (cloudController.SunGateActive &&
                        sunSourceState.SourceLight ==
                            cloudController.ResolvedSun)
                    {
                        maximumInstalledOffsetDelta = Mathf.Max(
                            maximumInstalledOffsetDelta,
                            (sample.CookieOffset -
                                cloudController.CurrentCookieOffset).magnitude);
                    }
                }
            }

            builder.Append("Usable / unstable / failed samples: ")
                .Append(usableSamples)
                .Append(" / ")
                .Append(unstableSamples)
                .Append(" / ")
                .AppendLine(failedSamples.ToString());
            builder.Append("Maximum query-offset delta versus installed Sun cookie: ")
                .AppendLine(
                    maximumInstalledOffsetDelta.ToString("0.######"));
            builder.AppendLine(
                "Visual comparison: use Cloud + Sun Openings and compare the high-contrast CPU markers in Scene view. V1.0C alignment was accepted from user screenshots.");
        }

        private static void AppendSourceReport(
            StringBuilder builder,
            WeatherLightRaySourceState state)
        {
            builder.Append('[')
                .Append(state.Kind)
                .AppendLine(" Source]");
            builder.Append("Light / profile: ")
                .Append(state.SourceLight != null
                    ? state.SourceLight.name
                    : "None")
                .Append(" / ")
                .AppendLine(state.Profile != null
                    ? state.Profile.name
                    : "None (fallback gate)");
            builder.Append("Available / gate weight / intensity / elevation: ")
                .Append(state.Available ? "Yes" : "No")
                .Append(" / ")
                .Append(state.AvailabilityWeight.ToString("0.###"))
                .Append(" / ")
                .Append(state.Intensity.ToString("0.###"))
                .Append(" / ")
                .AppendLine(state.Elevation.ToString("0.###"));
            builder.Append("Ray direction / direction to source: ")
                .Append(state.RayDirectionWorld.ToString("F3"))
                .Append(" / ")
                .AppendLine(state.DirectionToSourceWorld.ToString("F3"));
            if (!state.Available &&
                !string.IsNullOrEmpty(state.UnavailableReason))
            {
                builder.Append("Unavailable reason: ")
                    .AppendLine(state.UnavailableReason);
            }
        }

        private void DeactivateController()
        {
            if (automaticPopulationRuntime != null)
            {
                automaticPopulationRuntime.Shutdown(
                    this,
                    true);
                automaticPopulationRuntime = null;
            }

            for (int index = 0;
                index < selectionPopulationRuntimes.Length;
                index++)
            {
                selectionPopulationRuntimes[index]?.Shutdown(this, true);
            }
            selectionPopulationRuntimes =
                Array.Empty<WeatherLightRayPopulationRuntime>();
            selectionPopulationRuleOrder = Array.Empty<int>();
            activePopulationProfile = null;

            for (int index = 0;
                index < retiringSelectionPopulationRuntimes.Count;
                index++)
            {
                retiringSelectionPopulationRuntimes[index]?.Shutdown(
                    this,
                    true);
            }
            retiringSelectionPopulationRuntimes.Clear();
            selectionRuntime?.Shutdown();
            resolvedSelectionDependency = default;
            activePopulationDependencySignature = 0UL;

            if (PublishedController == this)
            {
                Shader.SetGlobalFloat(AccentLineIntensityId, 0f);
                Shader.SetGlobalFloat(AccentLineResolvedScaleId, 0f);
                Shader.SetGlobalFloat(VegetationAccentCoverageId, 0f);
                PublishVegetationAccentOverride(
                    Vector3.zero,
                    0f,
                    Vector3.zero,
                    false);
                vegetationAccentDiagnosticSuiteActive = false;
                PublishVegetationAccentDiagnosticMode(false);
            }

            DestroyAllSurfaceSpotLights();
            ActiveControllersInternal.Remove(this);
            if (PublishedController != this)
            {
                return;
            }

            PublishedController = ActiveControllersInternal.Count > 0
                ? ActiveControllersInternal[
                    ActiveControllersInternal.Count - 1]
                : null;
            if (PublishedController != null)
            {
                PublishedController.cachedMainCamera = null;
                PublishedController.EnsureStorage();
                PublishedController.TickController();
            }
        }
    }
}
