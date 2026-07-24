using System.Collections.Generic;
using System.Text;
using UnityEngine;

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

        public enum ProbeFocusSource
        {
            InspectorOverride = 0,
            AssignedFallbackCamera = 1,
            AutomaticMainCamera = 2,
            ControllerFallback = 3,
            CloudDebugOverlay = 4
        }

        private struct RuntimeSlot
        {
            public bool Active;
            public uint Generation;
            public WeatherLightRayAnchor AuthoredOwner;
            public float SmoothedGateWeight;
            public uint LifecycleRevision;
            public double LastUpdateTime;
            public double SpawnTime;
            public WeatherLightRaySnapshot Snapshot;
        }

        private static readonly List<WeatherLightRayController>
            ActiveControllersInternal =
                new List<WeatherLightRayController>();

        [Header("Activation")]
        [SerializeField]
        private bool lightRaysEnabled = true;

        [SerializeField]
        private bool previewInEditMode = true;

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

        [Header("Central Storage")]
        [SerializeField, Range(MinimumStorageCapacity, MaximumStorageCapacity)]
        [Tooltip(
            "Fixed LightRay slot capacity. This is not the desired visible-ray count.")]
        private int maximumActiveRays = 16;

        [Header("Cloud Transition")]
        [SerializeField, Range(0f, 1f)]
        private float cloudEvolutionResumeThreshold = 0.8f;

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
        private int activeRayCount;
        private WeatherLightRaySourceState sunSourceState;
        private WeatherLightRaySourceState moonSourceState;
        private Transform resolvedProbeFocus;
        private ProbeFocusSource resolvedProbeFocusSource;
        private Vector3 resolvedProbeCentre;
        private Camera cachedMainCamera;
        private Camera resolvedRenderCamera;
        private string lastError = string.Empty;

        public static int ActiveControllerCount =>
            ActiveControllersInternal.Count;

        public static WeatherLightRayController PublishedController
        {
            get;
            private set;
        }

        public bool LightRaysEnabled => lightRaysEnabled;
        public bool PreviewInEditMode => previewInEditMode;
        public bool IsPublished => PublishedController == this;
        public int StorageCapacity =>
            runtimeSlots != null ? runtimeSlots.Length : maximumActiveRays;
        public int ActiveRayCount => activeRayCount;
        public float CloudEvolutionResumeThreshold =>
            cloudEvolutionResumeThreshold;
        public WeatherLightRaySourceState SunSourceState => sunSourceState;
        public WeatherLightRaySourceState MoonSourceState => moonSourceState;
        public Camera ResolvedRenderCamera => resolvedRenderCamera;
        public WeatherLightRayRenderDebugView RenderDebugView =>
            renderDebugView;
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

            if (isActiveAndEnabled)
            {
                EnsureStorage();
                TickController();
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

            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (runtimeSlots[index].Active &&
                    runtimeSlots[index].AuthoredOwner != anchor)
                {
                    handle = default;
                    error =
                        "WEATHER-LIGHT-RAY-V1.1A/B supports one active authored LightRay. Disable the existing anchor before registering another.";
                    return false;
                }
            }

            int freeIndex = -1;
            for (int index = 0; index < runtimeSlots.Length; index++)
            {
                if (!runtimeSlots[index].Active)
                {
                    freeIndex = index;
                    break;
                }
            }

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
            slot.SmoothedGateWeight = 0f;
            slot.LifecycleRevision = anchor.LifecycleRevision;
            slot.LastUpdateTime = 0.0;
            slot.SpawnTime = Time.realtimeSinceStartupAsDouble;
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
            if (runtimeSlots == null)
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

        private WeatherLightRaySourceState ResolveRenderableSourceState(
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

        public string BuildComprehensiveReport()
        {
            var builder = new StringBuilder(4096);
            builder.AppendLine("[Weather LightRay V1.1A/B Structured Authored Report]");
            builder.Append("Status: ")
                .AppendLine(string.IsNullOrEmpty(lastError)
                    ? "SOURCE PREPARED FOR UNITY VALIDATION"
                    : "NOT READY");
            builder.Append("Published controller: ")
                .AppendLine(IsPublished ? "Yes" : "No");
            builder.Append("Active controllers: ")
                .AppendLine(ActiveControllerCount.ToString());
            builder.Append("LightRays enabled / edit preview: ")
                .Append(lightRaysEnabled ? "Yes" : "No")
                .Append(" / ")
                .AppendLine(previewInEditMode ? "Yes" : "No");
            builder.Append("Storage active / capacity: ")
                .Append(activeRayCount)
                .Append(" / ")
                .AppendLine(StorageCapacity.ToString());
            builder.Append("Authored registration: ")
                .AppendLine("Implemented; one active authored ray using the shared per-ray descriptor");
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
                builder.Append("Base / top radius: ")
                    .Append(renderSnapshot.BaseEllipseAxes.x.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.TopEllipseAxes.x.ToString("0.###"))
                    .AppendLine(" m");
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
                builder.Append("Strands / width range / spread: ")
                    .Append(renderSnapshot.Descriptor.StrandCount)
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.StrandWidthRange.ToString("F3"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.StrandSpread.ToString("0.###"));
                builder.Append("Strand / envelope / ground / object strengths: ")
                    .Append(renderSnapshot.Descriptor.StrandIntensity.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.EnvelopeHazeIntensity.ToString("0.###"))
                    .Append(" / ")
                    .Append(renderSnapshot.Descriptor.GroundLightMultiplier.ToString("0.###"))
                    .Append(" / ")
                    .AppendLine(renderSnapshot.Descriptor.VisibleSurfaceLightMultiplier.ToString("0.###"));
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

        private void TickController()
        {
            if (!isActiveAndEnabled || PublishedController != this)
            {
                return;
            }

            EnsureStorage();
            ResolveSourceStates();
            ResolveRenderCamera();
            lastError = string.Empty;
            UpdateRegisteredRays();
            ResolveProjectionFocus();
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
                    }
                }
            }

            runtimeSlots = replacement;
            activeRayCount = copiedActiveCount;
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
            slot.Active = false;
            slot.AuthoredOwner = null;
            slot.SmoothedGateWeight = 0f;
            slot.LifecycleRevision = 0u;
            slot.LastUpdateTime = 0.0;
            slot.SpawnTime = 0.0;
            slot.Snapshot = default;
            runtimeSlots[slotIndex] = slot;
            activeRayCount = Mathf.Max(0, activeRayCount - 1);
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
                WeatherLightRayAnchor owner = slot.AuthoredOwner;
                if (!slot.Active)
                {
                    continue;
                }

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
            WeatherLightRayDescriptor descriptor = anchor.BuildDescriptor();
            if (slot.LifecycleRevision != anchor.LifecycleRevision)
            {
                slot.LifecycleRevision = anchor.LifecycleRevision;
                slot.SpawnTime = now;
            }

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
                    anchor.transform.position,
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
                if (cloudSampleAvailable)
                {
                    WeatherCloudShadowController cloudController =
                        WeatherCloudShadowController.PublishedController;
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
                !anchor.ExternallyControlledVisible
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
                // Timed lifecycle already owns its initial fade-in. Avoid
                // multiplying it by a second identical gate fade.
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
                anchor.ExternallyControlledVisible,
                lifecycleState,
                lifecycleWeight,
                slot.SmoothedGateWeight,
                gateTarget,
                currentIntensity);

            Vector3 presentationDirection = ResolvePresentationDirection(
                sourceState.RayDirectionWorld,
                sourceState.Profile,
                descriptor.MaximumVisualLeanDegrees);
            WeatherLightRayHandle handle = new WeatherLightRayHandle(
                slotIndex,
                slot.Generation);
            slot.Snapshot = new WeatherLightRaySnapshot(
                handle,
                descriptor,
                lifecycleState,
                anchor.transform.position,
                presentationDirection,
                slot.SpawnTime,
                holdOrExpiryTime,
                currentIntensity,
                cloudTransmission);
            runtimeSlots[slotIndex] = slot;
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
            return sourceKind == WeatherLightRaySourceKind.Sun
                ? sunSourceState
                : moonSourceState;
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
