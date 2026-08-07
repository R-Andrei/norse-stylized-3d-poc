using System.Collections.Generic;
using System.Text;
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
        private const float SharedAccentLineMaximumRelativeScale = 1000f;
        private const float SharedAccentLineExponentialBase =
            SharedAccentLineMaximumRelativeScale + 1f;
        private const float SharedAccentLineOutputMultiplier = 0.2f;
        private const float SharedAccentLineBaselineDefault = 0.03f;
        private const string ImplementationPatchIdentifier =
            "WEATHER-LIGHT-RAY-CLEANUP-V1.3A4-PER-RAY-PRESET-AUTHORITY";

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
            public WeatherLightRayPreset ResolvedPreset;
            public WeatherLightRayPreset PreviousResolvedPreset;
            public bool InheritsDefaultPreset;
            public double PresetTransitionStartedAt;
            public float PresetTransitionDurationSeconds;
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
            "Global master for LightRay-specific stylized accent-line responses. " +
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

        [SerializeField, Range(1f, 30f)]
        private float automaticPopulationEvaluationRateHz = 4f;

        [SerializeField, Range(0f, 1f)]
        private float automaticPopulationMinimumClearance = 0.50f;

        [SerializeField, Min(0f)]
        private float automaticPopulationInvalidGraceDurationSeconds = 0.75f;

        [SerializeField, Min(0.01f)]
        private float automaticPopulationSpawnFadeDurationSeconds = 2f;

        [SerializeField, Min(0.01f)]
        private float automaticPopulationDespawnFadeDurationSeconds = 2.5f;

        [SerializeField, Min(0.1f)]
        private float automaticPopulationMinimumRayLifetimeSeconds = 5f;

        [SerializeField, Min(0.1f)]
        private float automaticPopulationMaximumRayLifetimeSeconds = 12f;

        [SerializeField, Min(0f)]
        private float automaticPopulationReplacementDelaySeconds = 1.5f;

        [SerializeField, Range(0f, 89f)]
        private float automaticPopulationMaximumGroundSlopeDegrees = 50f;

        [SerializeField]
        private bool showAutomaticPopulationCandidates;

        private RuntimeSlot[] runtimeSlots;
        private RuntimeSurfaceLight[] runtimeSurfaceLights;
        private WeatherLightRayPopulationRuntime automaticPopulationRuntime;
        private int activeRayCount;
        private int activeProceduralRayCount;
        private int activeSurfaceSpotLightCount;
        private WeatherLightRaySourceState sunSourceState;
        private WeatherLightRaySourceState moonSourceState;
        private Camera cachedMainCamera;
        private Camera resolvedRenderCamera;
        private string lastError = string.Empty;
        private readonly Dictionary<EntityId, Vector4> vegetationAccentOverridesByLight =
            new Dictionary<EntityId, Vector4>();
        private readonly Dictionary<EntityId, Vector4> vegetationAccentDirectionsByLight =
            new Dictionary<EntityId, Vector4>();
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
        /// <summary>
        /// Controller-level inherited preset. Kept under the historical
        /// ActivePreset API name for source compatibility; authoring presents
        /// this value as the Default Preset.
        /// </summary>
        public WeatherLightRayPreset ActivePreset => activePreset;
        public WeatherLightRayPreset DefaultPreset => activePreset;
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
        public int PublishedVegetationAdditionalLightCount =>
            publishedVegetationAdditionalLightCount;
        public int PublishedVegetationWeatherOverrideCount =>
            publishedVegetationWeatherOverrideCount;
        public int PublishedVegetationAccentBufferCapacity =>
            publishedVegetationAccentBufferCapacity;
        public bool PublishedVegetationAccentIndexOverflow =>
            publishedVegetationAccentIndexOverflow;
        public bool AutomaticPopulationEnabled => automaticPopulationEnabled;
        public bool ShowAutomaticPopulationCandidates =>
            showAutomaticPopulationCandidates;
        public WeatherLightRayPopulationRuntimeState AutomaticPopulationState =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.RuntimeState
                : WeatherLightRayPopulationRuntimeState.Disabled;
        public string AutomaticPopulationStatusReason =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.StatusReason
                : "Automatic population has not initialized.";
        public int AutomaticPopulationDerivedCandidateChecksPerUpdate =>
            Mathf.Clamp(automaticPopulationMaximumRayCount * 2, 4, 64);
        public float AutomaticPopulationDerivedGroundRaycastDistanceMetres
        {
            get
            {
                float farClip = resolvedRenderCamera != null &&
                    !float.IsNaN(resolvedRenderCamera.farClipPlane) &&
                    !float.IsInfinity(resolvedRenderCamera.farClipPlane)
                        ? resolvedRenderCamera.farClipPlane
                        : 100f;
                return Mathf.Max(100f, farClip);
            }
        }
        public Vector3 AutomaticPopulationFocusWorld =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.FocusWorld
                : Vector3.zero;
        public float AutomaticPopulationActiveRadiusMetres =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.ActiveRadiusMetres
                : 0f;
        public int AutomaticPopulationActiveCount =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.ActiveCount
                : 0;
        public int AutomaticPopulationPendingCount =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.PendingCount
                : 0;
        public int AutomaticPopulationRetiringCount =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.RetiringCount
                : 0;
        public int AutomaticPopulationCooldownCount =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CooldownCount
                : 0;
        public int AutomaticPopulationCandidateChecksLastTick =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CandidateChecksLastTick
                : 0;
        public int AutomaticPopulationGroundRaycastsLastTick =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.GroundRaycastsLastTick
                : 0;
        public int AutomaticPopulationCloudSamplesLastTick =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CloudSamplesLastTick
                : 0;
        public int AutomaticPopulationCellsInActiveRegion =>
            automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CellsInActiveRegion
                : 0;
        internal int AutomaticPopulationFreeSlotCount => Mathf.Max(
            0,
            StorageCapacity - activeRayCount);
        public string LastError => lastError;

        private void OnEnable()
        {
            if (!ActiveControllersInternal.Contains(this))
            {
                ActiveControllersInternal.Add(this);
            }

            PublishedController = this;
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
            automaticPopulationEvaluationRateHz = Mathf.Clamp(
                automaticPopulationEvaluationRateHz,
                1f,
                30f);
            automaticPopulationMinimumClearance = Mathf.Clamp01(
                automaticPopulationMinimumClearance);
            automaticPopulationInvalidGraceDurationSeconds = Mathf.Max(
                0f,
                automaticPopulationInvalidGraceDurationSeconds);
            automaticPopulationSpawnFadeDurationSeconds = Mathf.Max(
                0.01f,
                automaticPopulationSpawnFadeDurationSeconds);
            automaticPopulationDespawnFadeDurationSeconds = Mathf.Max(
                0.01f,
                automaticPopulationDespawnFadeDurationSeconds);
            automaticPopulationMinimumRayLifetimeSeconds = Mathf.Max(
                0.1f,
                automaticPopulationMinimumRayLifetimeSeconds);
            automaticPopulationMaximumRayLifetimeSeconds = Mathf.Max(
                automaticPopulationMinimumRayLifetimeSeconds,
                automaticPopulationMaximumRayLifetimeSeconds);
            automaticPopulationReplacementDelaySeconds = Mathf.Max(
                0f,
                automaticPopulationReplacementDelaySeconds);
            automaticPopulationMaximumGroundSlopeDegrees = Mathf.Clamp(
                automaticPopulationMaximumGroundSlopeDegrees,
                0f,
                89f);
            accentLineIntensity = Mathf.Clamp01(accentLineIntensity);
            lightRayVegetationAccentCoverage = Mathf.Clamp01(
                lightRayVegetationAccentCoverage);
            evolutionStrength = Mathf.Clamp01(evolutionStrength);
            evolutionSpeed = Mathf.Clamp01(evolutionSpeed);

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
            TickController();
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
            return automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CopyDebugRecords(destination)
                : 0;
        }

        public int CopyAutomaticPopulationFootprint(Vector3[] destination)
        {
            return automaticPopulationRuntime != null
                ? automaticPopulationRuntime.CopyActiveFootprint(destination)
                : 0;
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

        private WeatherLightRayPreset ResolvePresetOverride(
            WeatherLightRayPreset presetOverride)
        {
            return presetOverride != null
                ? presetOverride
                : activePreset;
        }

        private bool TryResolveSlotPreset(
            ref RuntimeSlot slot,
            WeatherLightRayPreset desiredPreset,
            bool inheritsDefaultPreset,
            double now,
            out WeatherLightRayPreset resolvedPreset,
            out WeatherLightRayPreset previousPreset,
            out float presentationBlend)
        {
            resolvedPreset = desiredPreset;
            previousPreset = null;
            presentationBlend = 1f;
            if (desiredPreset == null)
            {
                return false;
            }

            bool firstResolution = slot.ResolvedPreset == null;
            bool targetChanged = slot.ResolvedPreset != desiredPreset ||
                slot.InheritsDefaultPreset != inheritsDefaultPreset;
            if (firstResolution || targetChanged)
            {
                bool canJoinDefaultTransition = inheritsDefaultPreset &&
                    activePreset == desiredPreset &&
                    previousPresentationPreset != null &&
                    presetTransitionDurationSeconds > 0f &&
                    (!firstResolution
                        ? slot.InheritsDefaultPreset &&
                            slot.ResolvedPreset == previousPresentationPreset
                        : true);

                slot.ResolvedPreset = desiredPreset;
                slot.InheritsDefaultPreset = inheritsDefaultPreset;
                if (canJoinDefaultTransition)
                {
                    slot.PreviousResolvedPreset = previousPresentationPreset;
                    slot.PresetTransitionStartedAt = presetTransitionStartedAt;
                    slot.PresetTransitionDurationSeconds =
                        presetTransitionDurationSeconds;
                }
                else
                {
                    slot.PreviousResolvedPreset = null;
                    slot.PresetTransitionStartedAt = now;
                    slot.PresetTransitionDurationSeconds = 0f;
                }
            }

            if (slot.PreviousResolvedPreset != null &&
                slot.PresetTransitionDurationSeconds > 0f)
            {
                presentationBlend = Mathf.Clamp01((float)(
                    (now - slot.PresetTransitionStartedAt) /
                    slot.PresetTransitionDurationSeconds));
                if (presentationBlend >= 1f)
                {
                    slot.PreviousResolvedPreset = null;
                    slot.PresetTransitionDurationSeconds = 0f;
                    presentationBlend = 1f;
                }
            }

            resolvedPreset = slot.ResolvedPreset;
            previousPreset = slot.PreviousResolvedPreset;
            return true;
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

            WeatherLightRayPreset resolvedPreset = ResolvePresetOverride(
                anchor.PresetOverride);
            if (resolvedPreset == null)
            {
                handle = default;
                error =
                    "The authored LightRay requires either a Preset Override or a Controller Default Preset.";
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
            slot.ResolvedPreset = null;
            slot.PreviousResolvedPreset = null;
            slot.InheritsDefaultPreset = false;
            slot.PresetTransitionStartedAt = slot.SpawnTime;
            slot.PresetTransitionDurationSeconds = 0f;
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

            WeatherLightRayPreset resolvedPreset = ResolvePresetOverride(
                request.PresetOverride);
            if (resolvedPreset == null)
            {
                error =
                    "Procedural LightRay spawning requires either a Preset Override or a Controller Default Preset.";
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
            slot.ResolvedPreset = null;
            slot.PreviousResolvedPreset = null;
            slot.InheritsDefaultPreset = false;
            slot.PresetTransitionStartedAt = now;
            slot.PresetTransitionDurationSeconds = 0f;
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

            if (ResolvePresetOverride(update.SpawnRequest.PresetOverride) == null)
            {
                error =
                    "The procedural LightRay update requires either a Preset Override or a Controller Default Preset.";
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
            WeatherLightRaySourceState sourceState = sourceKind ==
                WeatherLightRaySourceKind.Sun
                    ? sunSourceState
                    : moonSourceState;
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

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine(
                "[Weather LightRay Cleanup V1.3A4 Comprehensive Report]");
            builder.Append("Implementation patch: ")
                .AppendLine(ImplementationPatchIdentifier);
            builder.Append("Published / active controllers: ")
                .Append(IsPublished ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(ActiveControllerCount.ToString());
            builder.Append("Enabled / edit preview / default preset: ")
                .Append(lightRaysEnabled ? "Yes" : "No")
                .Append(" / ")
                .Append(previewInEditMode ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(activePreset != null ? activePreset.name : "None");
            builder.Append("Storage active / authored / procedural / capacity / free: ")
                .Append(activeRayCount)
                .Append(" / ")
                .Append(ActiveAuthoredRayCount)
                .Append(" / ")
                .Append(activeProceduralRayCount)
                .Append(" / ")
                .Append(StorageCapacity)
                .Append(" / ")
                .AppendLine(AutomaticPopulationFreeSlotCount.ToString());
            builder.Append("Surface Spot Lights: ")
                .AppendLine(activeSurfaceSpotLightCount.ToString());
            builder.Append("Render camera / debug view: ")
                .Append(resolvedRenderCamera != null
                    ? resolvedRenderCamera.name
                    : "None")
                .Append(" / ")
                .AppendLine(renderDebugView.ToString());
            builder.Append("Default-preset vegetation accent intensity / coverage / softness: ");
            if (activePreset != null)
            {
                builder.Append(AccentLineIntensity.ToString("0.###"))
                    .Append(" / ")
                    .Append(LightRayVegetationAccentCoverage.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(LightRayVegetationAccentSoftness.ToString("0.###"));
            }
            else
            {
                builder.AppendLine("N/A (no Controller Default Preset)");
            }
            builder.Append("Published vegetation additional lights / Weather overrides / buffer capacity / overflow: ")
                .Append(publishedVegetationAdditionalLightCount)
                .Append(" / ")
                .Append(publishedVegetationWeatherOverrideCount)
                .Append(" / ")
                .Append(publishedVegetationAccentBufferCapacity)
                .Append(" / ")
                .AppendLine(publishedVegetationAccentIndexOverflow
                    ? "Yes"
                    : "No");
            builder.AppendLine();
            AppendSourceReport(builder, sunSourceState);
            AppendSourceReport(builder, moonSourceState);

            builder.AppendLine();
            WeatherLightRayPopulationRuntime.Settings populationSettings =
                BuildAutomaticPopulationSettings(
                    automaticPopulationEnabled && Application.isPlaying);
            if (automaticPopulationRuntime != null)
            {
                automaticPopulationRuntime.AppendReport(
                    builder,
                    populationSettings,
                    AutomaticPopulationFreeSlotCount);
            }
            else
            {
                builder.AppendLine("[Automatic Atmospheric Population]");
                builder.AppendLine("Runtime has not initialized.");
            }

            builder.AppendLine();
            builder.AppendLine("[Active Rays]");
            if (runtimeSlots == null || activeRayCount == 0)
            {
                builder.AppendLine("None");
            }
            else
            {
                for (int index = 0; index < runtimeSlots.Length; index++)
                {
                    RuntimeSlot slot = runtimeSlots[index];
                    if (!slot.Active)
                    {
                        continue;
                    }

                    WeatherLightRaySnapshot snapshot = slot.Snapshot;
                    builder.Append("Slot ")
                        .Append(index)
                        .Append(" | ")
                        .Append(slot.Procedural ? "Procedural" : "Authored")
                        .Append(" | handle ")
                        .Append(snapshot.Handle)
                        .Append(" | preset ")
                        .Append(snapshot.ResolvedPreset != null
                            ? snapshot.ResolvedPreset.DisplayName
                            : "None")
                        .Append(snapshot.InheritsDefaultPreset
                            ? " (Default)"
                            : " (Override)")
                        .Append(" | preset blend ")
                        .Append(snapshot.PresetPresentationBlend.ToString("0.###"))
                        .Append(" | veg I/C/S ")
                        .Append(snapshot.Descriptor.VegetationAccentIntensity.ToString("0.###"))
                        .Append('/')
                        .Append(snapshot.Descriptor.VegetationAccentCoverage.ToString("0.###"))
                        .Append('/')
                        .Append(snapshot.Descriptor.VegetationAccentSoftness.ToString("0.###"))
                        .Append(" | source ")
                        .Append(snapshot.SourceKind)
                        .Append(" | lifecycle ")
                        .Append(snapshot.LifecycleState)
                        .Append(" | intensity ")
                        .Append(snapshot.CurrentIntensity.ToString("0.###"))
                        .Append(" | cloud ")
                        .Append(snapshot.CurrentCloudTransmission.ToString("0.###"))
                        .Append(" | centre ")
                        .Append(snapshot.BaseCentreWorld.ToString("F3"))
                        .Append(" | beams ")
                        .Append(snapshot.BeamCount)
                        .Append(" | evolution ")
                        .Append(snapshot.EvolutionCurrentSeed)
                        .Append(" -> ")
                        .Append(snapshot.EvolutionNextSeed)
                        .Append(" @ ")
                        .AppendLine(snapshot.EvolutionBlend.ToString("0.###"));
                }
            }

            if (!string.IsNullOrEmpty(lastError))
            {
                builder.AppendLine();
                builder.AppendLine("[Last Error]");
                builder.AppendLine(lastError);
            }

            return builder.ToString();
        }

        private void TickController(bool allowSurfaceLightCreation = true)
        {
            if (!isActiveAndEnabled || PublishedController != this)
            {
                DisableAllSurfaceSpotLights();
                return;
            }

            EnsureStorage();
            ResolveSourceStates();
            ResolveRenderCamera();
            lastError = string.Empty;
            TickAutomaticPopulation();
            UpdateRegisteredRays();
            UpdateSurfaceSpotLights(allowSurfaceLightCreation);
        }

        private void TickAutomaticPopulation()
        {
            if (automaticPopulationRuntime == null)
            {
                automaticPopulationRuntime =
                    new WeatherLightRayPopulationRuntime();
            }

            WeatherLightRayPopulationRuntime.Settings settings =
                BuildAutomaticPopulationSettings(
                    automaticPopulationEnabled && Application.isPlaying);
            automaticPopulationRuntime.Tick(
                this,
                settings,
                Time.realtimeSinceStartupAsDouble);
        }

        private WeatherLightRayPopulationRuntime.Settings
            BuildAutomaticPopulationSettings(bool runtimeEnabled)
        {
            return new WeatherLightRayPopulationRuntime.Settings(
                runtimeEnabled,
                lightRaysEnabled,
                automaticPopulationSeed,
                automaticPopulationFocusOverride,
                resolvedRenderCamera,
                automaticPopulationGroundMask,
                automaticPopulationDesiredRayCount,
                automaticPopulationMaximumRayCount,
                automaticPopulationMinimumSpacingMetres,
                automaticPopulationOffscreenMarginMetres,
                automaticPopulationEvaluationRateHz,
                automaticPopulationMinimumClearance,
                automaticPopulationInvalidGraceDurationSeconds,
                automaticPopulationSpawnFadeDurationSeconds,
                automaticPopulationDespawnFadeDurationSeconds,
                automaticPopulationMinimumRayLifetimeSeconds,
                automaticPopulationMaximumRayLifetimeSeconds,
                automaticPopulationReplacementDelaySeconds,
                automaticPopulationMaximumGroundSlopeDegrees,
                cloudEvolutionResumeThreshold,
                activePreset,
                sunSourceState,
                WeatherCloudShadowController.PublishedController);
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
                priority: settings.Priority,
                presetOverride: settings.PresetOverride);
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
            slot.ResolvedPreset = null;
            slot.PreviousResolvedPreset = null;
            slot.InheritsDefaultPreset = false;
            slot.PresetTransitionStartedAt = 0.0;
            slot.PresetTransitionDurationSeconds = 0f;
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
            vegetationAccentDirectionsByLight.Clear();
            if (runtimeSlots == null)
            {
                DisableAllSurfaceSpotLights();
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

                if (UpdateSurfaceSpotLight(
                        proxy,
                        raySnapshot))
                {
                    activeSurfaceSpotLightCount++;
                    if (proxy.Light != null)
                    {
                        EntityId lightEntityId = proxy.Light.GetEntityId();
                        WeatherLightRayDescriptor descriptor =
                            raySnapshot.Descriptor;
                        float accentScale = lightRaysEnabled
                            ? EvaluateSharedAccentLineRelativeScale(
                                descriptor.VegetationAccentIntensity)
                            : 0f;
                        Vector4 accentData = new Vector4(
                            accentScale,
                            descriptor.VegetationAccentCoverage,
                            descriptor.VegetationAccentSoftness,
                            1f);
                        vegetationAccentOverridesByLight[lightEntityId] = accentData;
                        Vector3 directionToSource = -raySnapshot.RayDirectionWorld;
                        directionToSource.y = 0f;
                        bool directionValid =
                            directionToSource.sqrMagnitude > 0.000001f;
                        if (directionValid)
                        {
                            directionToSource.Normalize();
                        }
                        vegetationAccentDirectionsByLight[lightEntityId] =
                            new Vector4(
                                directionToSource.x,
                                directionToSource.y,
                                directionToSource.z,
                                directionValid ? 1f : 0f);
                    }
                }
            }

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

        public bool TryGetVegetationAccentOverride(
            Light light,
            out Vector4 accentData)
        {
            return TryGetVegetationAccentOverride(
                light,
                out accentData,
                out _);
        }

        public bool TryGetVegetationAccentOverride(
            Light light,
            out Vector4 accentData,
            out Vector4 sourceDirectionWS)
        {
            if (light != null)
            {
                EntityId entityId = light.GetEntityId();
                if (vegetationAccentOverridesByLight.TryGetValue(
                        entityId,
                        out accentData))
                {
                    if (!vegetationAccentDirectionsByLight.TryGetValue(
                            entityId,
                            out sourceDirectionWS))
                    {
                        sourceDirectionWS = Vector4.zero;
                    }
                    return true;
                }
            }

            accentData = Vector4.zero;
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
            WeatherLightRaySnapshot raySnapshot)
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
                out float colourPeak);
            float sourceIntensity = raySnapshot.ResolvedSourceIntensity;
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

        private static Color ResolveSourcePresentationColour(
            WeatherLightRayDescriptor descriptor,
            WeatherLightRaySourceState sourceState)
        {
            Color sourceColour = sourceState.SourceLight != null
                ? sourceState.Colour
                : Color.white;
            if (descriptor.SourceKind == WeatherLightRaySourceKind.Sun)
            {
                Color warmSunColour = new Color(
                    1f,
                    0.76f,
                    0.46f,
                    1f);
                sourceColour = Color.Lerp(
                    sourceColour,
                    warmSunColour,
                    descriptor.WarmthContribution);
            }

            return sourceColour;
        }

        private static Color ResolveSurfaceSpotLightColour(
            WeatherLightRaySnapshot raySnapshot,
            out float colourPeak)
        {
            Color effectiveColour = raySnapshot.ResolvedSourceColour *
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
            WeatherLightRayPreset desiredPreset = ResolvePresetOverride(
                anchor.PresetOverride);
            bool inheritsDefaultPreset = anchor.PresetOverride == null;
            if (!TryResolveSlotPreset(
                    ref slot,
                    desiredPreset,
                    inheritsDefaultPreset,
                    now,
                    out WeatherLightRayPreset resolvedPreset,
                    out WeatherLightRayPreset previousPreset,
                    out float presentationBlend))
            {
                lastError =
                    "An authored LightRay requires either a Preset Override or a Controller Default Preset.";
                ReleaseSlot(slotIndex);
                return;
            }

            WeatherLightRayDescriptor descriptor = resolvedPreset.ApplyTo(
                anchor.BuildLocalDescriptor(),
                anchor.OverridePresetBeamSpacing,
                anchor.BeamSpacingMetres,
                anchor.LocalIntensityMultiplier,
                previousPreset,
                presentationBlend);

            if (slot.LifecycleRevision != anchor.LifecycleRevision)
            {
                slot.LifecycleRevision = anchor.LifecycleRevision;
                slot.SpawnTime = now;
            }

            runtimeSlots[slotIndex] = slot;
            UpdateRuntimeSlot(
                slotIndex,
                descriptor,
                anchor.transform.position,
                Vector3.zero,
                anchor.ExternallyControlledVisible,
                now,
                false,
                resolvedPreset,
                previousPreset,
                inheritsDefaultPreset,
                presentationBlend);
        }

        private void UpdateProceduralSlot(int slotIndex, double now)
        {
            RuntimeSlot slot = runtimeSlots[slotIndex];
            if (!slot.Active || !slot.Procedural)
            {
                return;
            }

            WeatherLightRaySpawnRequest request = slot.ProceduralRequest;
            WeatherLightRayPreset desiredPreset = ResolvePresetOverride(
                request.PresetOverride);
            bool inheritsDefaultPreset = request.PresetOverride == null;
            if (!TryResolveSlotPreset(
                    ref slot,
                    desiredPreset,
                    inheritsDefaultPreset,
                    now,
                    out WeatherLightRayPreset resolvedPreset,
                    out WeatherLightRayPreset previousPreset,
                    out float presentationBlend))
            {
                lastError =
                    "An active procedural LightRay requires either a Preset Override or a Controller Default Preset.";
                ReleaseSlot(slotIndex);
                return;
            }

            WeatherLightRayDescriptor descriptor =
                BuildProceduralDescriptor(
                    request,
                    resolvedPreset,
                    previousPreset,
                    presentationBlend);
            runtimeSlots[slotIndex] = slot;
            UpdateRuntimeSlot(
                slotIndex,
                descriptor,
                request.BaseCentreWorld,
                request.RayDirectionWorld,
                slot.ProceduralVisible,
                now,
                true,
                resolvedPreset,
                previousPreset,
                inheritsDefaultPreset,
                presentationBlend);
        }

        private static WeatherLightRayDescriptor BuildProceduralDescriptor(
            in WeatherLightRaySpawnRequest request,
            WeatherLightRayPreset resolvedPreset,
            WeatherLightRayPreset previousPreset,
            float presentationBlend)
        {
            float height = request.OverrideHeight
                ? request.HeightMetres
                : resolvedPreset.DefaultHeightMetres;
            float lean = request.OverrideMaximumVisualLean
                ? request.MaximumVisualLeanDegrees
                : resolvedPreset.DefaultMaximumVisualLeanDegrees;
            float spacing = request.OverrideBeamSpacing
                ? request.BeamSpacingMetres
                : resolvedPreset.BeamSpacingMetres;
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
                Vector2.one,
                0f,
                0.5f,
                0f,
                0.1f,
                0.1f,
                0f,
                Color.white,
                0f,
                1f,
                0f,
                0f,
                0f,
                0f,
                1f,
                0f,
                0f,
                0.5f,
                WeatherLightRayEvolutionPreset.Static,
                0f,
                0f,
                request.FadeInDurationSeconds,
                request.HoldDurationSeconds,
                request.FadeOutDurationSeconds,
                request.GameplayChannel,
                request.VariationSeed);
            return resolvedPreset.ApplyTo(
                localDescriptor,
                request.OverrideBeamSpacing,
                request.BeamSpacingMetres,
                request.LocalIntensityMultiplier,
                previousPreset,
                presentationBlend);
        }

        private void UpdateRuntimeSlot(
            int slotIndex,
            WeatherLightRayDescriptor descriptor,
            Vector3 baseCentreWorld,
            Vector3 rayDirectionOverride,
            bool externallyControlledVisible,
            double now,
            bool releaseTimedExpiry,
            WeatherLightRayPreset resolvedPreset,
            WeatherLightRayPreset previousResolvedPreset,
            bool inheritsDefaultPreset,
            float presetPresentationBlend)
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
                if (cloudSampleAvailable)
                {
                    float shaded = cloudController != null
                        ? cloudController.ShadedTransmission
                        : 0f;
                    cloudOpenWeight = Mathf.Clamp01(
                        (cloudTransmission - shaded) /
                        Mathf.Max(0.0001f, 1f - shaded));
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
            Color resolvedSourceColour = ResolveSourcePresentationColour(
                descriptor,
                sourceState);
            float resolvedSourceIntensity = descriptor.SourceGatePolicy ==
                    WeatherLightRaySourceGatePolicy.IgnoreSourceGate &&
                !sourceState.Available
                    ? Mathf.Max(1f, sourceState.Intensity)
                    : sourceState.SourceLight != null
                        ? Mathf.Max(0f, sourceState.Intensity)
                        : 1f;
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
                slot.CompletedEvolutionTransitions,
                resolvedPreset,
                previousResolvedPreset,
                inheritsDefaultPreset,
                presetPresentationBlend,
                resolvedSourceColour,
                resolvedSourceIntensity);
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
                    return new WeatherLightRaySourceState(
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
