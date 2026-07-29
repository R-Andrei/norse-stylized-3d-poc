using ProgrammaticStylized3D.Weather.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherLightRayController))]
    public sealed class WeatherLightRayControllerEditor : UnityEditor.Editor
    {
        private bool showSourceBinding;
        private bool showHybridRenderer;
        private bool showFoundationStorage;
        private bool showAutomaticPopulation;
        private bool showActiveAuthoredRay;
        private bool showProjectionDiagnostic;
        private bool showVegetationAccentDiagnostic;
        private bool showActionsReports;
        private bool showLiveStatus;
        private bool showProjectionTransmissionLabels;
        private string beamEvolutionRuntimeAudit = string.Empty;
        private WeatherLightRayHandle proceduralTestHandleA;
        private WeatherLightRayHandle proceduralTestHandleB;
        private WeatherLightRayHandle cloudAwareTestHandle;
        private int cloudAwareTestPhase;
        private WeatherLightRayPopulationDebugRecord[] populationDebugRecords =
            new WeatherLightRayPopulationDebugRecord[64];

        private sealed class DeterministicCloudOpeningProvider :
            IWeatherLightRayCloudClearanceProvider
        {
            private readonly WeatherLightRayCloudOpening opening;

            public DeterministicCloudOpeningProvider(
                in WeatherLightRayCloudOpening opening)
            {
                this.opening = opening;
            }

            public bool TryResolveOpening(
                in WeatherLightRayCloudQuery query,
                out WeatherLightRayCloudOpening resolvedOpening)
            {
                resolvedOpening = opening;
                return opening.SourceKind == query.SourceKind &&
                    opening.AreaDiameterMetres >=
                        query.MinimumDiameterMetres &&
                    opening.AreaDiameterMetres <=
                        query.MaximumDiameterMetres &&
                    opening.Confidence >= query.MinimumConfidence;
            }
        }

        private static GUIStyle projectionTransmissionLabelStyle;

        public override void OnInspectorGUI()
        {
            var controller = (WeatherLightRayController)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);

            WeatherInspectorGui.Info(
                "Weather LightRay V1.2E separates visual presets, normalized 0–1 activation selection, and reusable population policies. Manual mode preserves the validated V1.2D behaviour; Selection Profile mode must be explicitly assigned and enabled.");
            DrawImmediateWarnings(controller);

            DrawSourceBinding(controller);
            DrawHybridRenderer(controller);
            DrawFoundationStorage(controller);
            DrawAutomaticPopulation(controller);
            DrawActiveAuthoredRay(controller);
            DrawProjectionDiagnostic(controller);

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                controller.RefreshNow();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            DrawVegetationAccentDiagnosticSuite(controller);
            DrawActionsReports(controller);
            DrawLiveStatus(controller);
        }

        private static void DrawImmediateWarnings(
            WeatherLightRayController controller)
        {
            if (WeatherLightRayController.ActiveControllerCount > 1)
            {
                WeatherInspectorGui.Warning(
                    "Multiple Weather LightRay Controllers are active. The most " +
                    "recently enabled controller is published.");
            }

            if (!controller.IsPublished)
            {
                WeatherInspectorGui.Warning(
                    "This controller is not the active Weather LightRay publisher.");
            }

            if (controller.PresetControlMode ==
                    WeatherLightRayPresetControlMode.Manual &&
                controller.ActivePreset == null)
            {
                WeatherInspectorGui.Warning(
                    "Manual preset control is active but no Active Preset is assigned. Legacy serialized values remain only as compatibility fallbacks.");
            }
            else if (controller.PresetControlMode ==
                        WeatherLightRayPresetControlMode.SelectionProfile &&
                controller.SelectionProfile == null)
            {
                WeatherInspectorGui.Warning(
                    "Selection Profile control is active but no LightRay Selection Profile is assigned.");
            }

            if (WeatherLightRayRendererFeature.ActiveFeatureCount == 0)
            {
                WeatherInspectorGui.Warning(
                    "No active WeatherLightRayRendererFeature is loaded. After " +
                    "compilation, add the feature to Assets/Settings/PC_Renderer.asset " +
                    "through the Unity Renderer Inspector. Do not raw-edit the asset.");
            }

            if (!string.IsNullOrEmpty(controller.LastError))
            {
                WeatherInspectorGui.Error(controller.LastError);
            }
        }

        private void DrawSourceBinding(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showSourceBinding,
                    "Preset Selection & Source Binding",
                    "Separates visual-preset authority, normalized 0–1 activation selection, and explicit directional dependencies."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "presetControlMode",
                    "Preset Control Mode",
                    "Manual preserves one explicitly assigned visual preset. Selection Profile evaluates normalized 0–1 activation curves and explicit dependencies at a bounded cadence.");

                SerializedProperty controlMode = serializedObject.FindProperty(
                    "presetControlMode");
                bool selectionMode = controlMode != null &&
                    controlMode.enumValueIndex == (int)
                        WeatherLightRayPresetControlMode.SelectionProfile;
                if (selectionMode)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "selectionProfile",
                        "Selection Profile",
                        "Defines visual-preset eligibility, priority, transition stability, and explicit direction/cloud dependencies. Visual presets do not own these rules.");
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "cycleSourceMode",
                        "Normalized Cycle Source",
                        "Selects the authoritative normalized 0–1 activation input. No hour or named-daypart interpretation is used.");
                    SerializedProperty cycleMode = serializedObject.FindProperty(
                        "cycleSourceMode");
                    int cycleModeValue = cycleMode != null
                        ? cycleMode.enumValueIndex
                        : 0;
                    if (cycleModeValue == (int)
                        WeatherLightRayCycleSourceMode.TimeOfDay)
                    {
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "timeOfDayController",
                            "Time Of Day Controller",
                            "Optional explicit normalized-cycle provider. When unassigned, exactly one active TimeOfDayController may be resolved; zero or multiple providers suspend selection.");
                    }
                    else if (cycleModeValue == (int)
                        WeatherLightRayCycleSourceMode.ManualNormalizedValue)
                    {
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "manualNormalizedCycle",
                            "Manual Normalized Cycle",
                            "Direct normalized 0–1 activation input for static scenes and validation.");
                    }

                    WeatherInspectorGui.ReadOnlyObject(
                        "Resolved Active Preset",
                        controller.ActivePreset);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Resolved Cycle",
                        controller.ResolvedNormalizedCycle);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Selected Entry",
                        controller.ActiveSelectionEntryName);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Selection Weight",
                        controller.ActiveSelectionWeight);
                    if (!string.IsNullOrEmpty(
                            controller.SelectionSuspensionReason))
                    {
                        WeatherInspectorGui.Help(
                            "Selection: " +
                            controller.SelectionSuspensionReason);
                    }
                    if (!string.IsNullOrEmpty(controller.CycleResolutionError))
                    {
                        WeatherInspectorGui.Warning(
                            controller.CycleResolutionError);
                    }
                }
                else
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "activePreset",
                        "Active Preset",
                        "Authoritative shared visual appearance in Manual mode. Source, activation, cloud, and population policy do not belong to this asset.");
                }

                WeatherInspectorGui.Property(
                    serializedObject,
                    "presetCatalog",
                    "Preset Catalog",
                    "Optional catalog of approved visual presets.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Update LightRay State in Edit Mode",
                    "Keeps authored registration, source state, intensity fades, and cloud-projection diagnostics current outside Play Mode.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunOverride",
                    "Directional Source Override",
                    "Optional explicit directional source. When unassigned, RenderSettings.sun remains the current Controller directional source contract.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunProfile",
                    "Directional Source Profile",
                    "Optional source availability, colour multiplier, elevation fade, and presentation-lean profile.");

                WeatherLightRaySourceState sunState =
                    controller.SunSourceState;
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Directional Source",
                    sunState.SourceLight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Availability",
                    sunState.Available ? "Available" : "Unavailable");
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Gate Weight",
                    sunState.AvailabilityWeight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Ray Direction",
                    sunState.RayDirectionWorld.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Elevation",
                    sunState.Elevation);
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Intensity",
                    sunState.Intensity);
                if (!sunState.Available &&
                    !string.IsNullOrEmpty(sunState.UnavailableReason))
                {
                    WeatherInspectorGui.Help(
                        "Directional source unavailable: " +
                        sunState.UnavailableReason);
                }
            }
        }

        private void DrawHybridRenderer(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showHybridRenderer,
                    "Hybrid Renderer",
                    "Controls the global renderer gate, designated Base Game camera, and structured-ray diagnostic output."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "lightRaysEnabled",
                    "LightRays Enabled",
                    "Global authoritative gate for authored LightRay intensity and hybrid rendering. Disabling it fades the active ray using the authored fade-out duration.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "renderCameraOverride",
                    "Render Camera Override",
                    "Optional exact Base Game camera allowed to execute the Renderer Feature. When unassigned, Camera.main is resolved.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "renderDebugView",
                    "Render Debug View",
                    "Final Composite shows atmosphere, the real material-lighting Spot response, and any optional screen-space complement. Raw Continuous Beams shows the direct unfiltered full-resolution atmospheric mask. Surface Illumination diagnoses the shared footprint geometry and optional complement only; the real Spot Light is visible through normal receiver lighting in Final Composite. Softened Continuous Beams shows the bounded atmospheric halo result.");
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Render Camera",
                    controller.ResolvedRenderCamera);
                WeatherInspectorGui.ReadOnlyRow(
                    "Loaded Renderer Features",
                    WeatherLightRayRendererFeature.ActiveFeatureCount);
                WeatherInspectorGui.Help(
                    "V1.1 executes only for the resolved Base Game camera. Scene, Preview, " +
                    "Reflection, overlay-stack, and unrelated secondary cameras are skipped. " +
                    "The Renderer Feature must be added to PC_Renderer in Unity after compilation.");
            }
        }


        private void DrawFoundationStorage(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showFoundationStorage,
                    "Central Storage",
                    "Controls the fixed nonallocating slot array owned by the central controller."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumActiveRays",
                    "Storage Capacity",
                    "Fixed number of CPU LightRay runtime slots. Every active compatible zone can now publish into the shared atmospheric mask; this is storage capacity, not a visible-zone target.");
                WeatherInspectorGui.ReadOnlyRow(
                    "Active Slots",
                    controller.ActiveRayCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Authored Slots",
                    controller.ActiveAuthoredRayCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Procedural Slots",
                    controller.ActiveProceduralRayCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Total Capacity",
                    controller.StorageCapacity);
                WeatherInspectorGui.Help(
                    "Cloud Evolution Resume Threshold remains centralized and hidden. Automatic population and authored or gameplay Respect Clouds rays use it to suspend through unstable seed evolution.");
            }
        }

        private void DrawAutomaticPopulation(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showAutomaticPopulation,
                    "Automatic Population",
                    "Maintains bounded deterministic automatic populations. In Selection Profile mode, reusable Population Profile rules own desired counts, spacing, and cloud qualification."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationEnabled",
                    "Enabled",
                    "Enables automatic population in Play Mode. Disabling it fades and releases automatic rays only.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationSeed",
                    "Population Seed",
                    "Stable world-cell seed. Camera motion does not reshuffle candidates.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationFocusOverride",
                    "Population Focus Override",
                    "Optional explicit world focus. When unassigned, the resolved Base Game camera centre is projected onto the Ground Mask.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationGroundMask",
                    "Ground Mask",
                    "Required scene physics mask for camera-footprint projection and candidate ground acquisition. No layer is inferred or created.");

                bool selectionMode = controller.PresetControlMode ==
                    WeatherLightRayPresetControlMode.SelectionProfile;
                if (!selectionMode)
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "automaticPopulationDesiredRayCount",
                        "Desired Ray Count",
                        "Manual-mode target automatic count. Selection Profile mode takes this value from each Population Profile rule.");
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "automaticPopulationMinimumSpacingMetres",
                        "Minimum Spacing",
                        "Manual-mode spacing. Selection Profile mode takes spacing from each Population Profile rule.");
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "automaticPopulationMinimumClearance",
                        "Minimum Clearance",
                        "Manual-mode complete-footprint cloud threshold. Selection Profile mode takes cloud thresholds from each Population Profile rule.");
                }
                else
                {
                    WeatherInspectorGui.ReadOnlyObject(
                        "Resolved Selection Profile",
                        controller.SelectionProfile);
                    WeatherInspectorGui.Help(
                        "Desired count, per-rule maximum, spacing, cloud-data requirement, spatial cloud policy, cloud-cover activation, and opening thresholds are authored in the selected entry's Population Profile.");
                }

                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationMaximumRayCount",
                    "Global Maximum Automatic Rays",
                    "Shared rendering budget across every active population rule. Authored and caller-created procedural rays are never evicted.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationOffscreenMarginMetres",
                    "Offscreen Margin",
                    "Extra evaluated radius beyond the projected camera ground footprint.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationFallbackActiveRadiusMetres",
                    "Fallback Active Radius",
                    "Used around a valid focus when camera-corner projection cannot resolve a complete ground footprint.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationEvaluationRateHz",
                    "Population Evaluation Rate",
                    "Bounded candidate and active-ray revalidation cadence. Selection-entry evaluation uses its Selection Profile cadence.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationCandidateChecksPerTick",
                    "Candidate Checks Per Tick",
                    "Shared maximum candidate budget divided among enabled population rules.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationQualificationDurationSeconds",
                    "Qualification Duration",
                    "How long a pending candidate must remain valid before spawning. At least two valid evaluations are required.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationInvalidGraceDurationSeconds",
                    "Invalid Grace Duration",
                    "How long an active ray may fail revalidation before graceful retirement starts.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationMinimumViableOpeningDurationSeconds",
                    "Minimum Viable Opening Duration",
                    "Forecast horizon used by cloud-qualified population rules.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationMaximumGroundSlopeDegrees",
                    "Maximum Ground Slope",
                    "Rejects candidate ground hits steeper than this angle from horizontal.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "automaticPopulationGroundSearchDistanceMetres",
                    "Ground Search Distance",
                    "Maximum distance for camera and candidate ground raycasts.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "showAutomaticPopulationCandidates",
                    "Show Population Candidates",
                    "Draws compact Scene-view markers for pending, active, retiring, and cooldown candidates while this Controller is selected.");

                SerializedProperty groundMask = serializedObject.FindProperty(
                    "automaticPopulationGroundMask");
                if (groundMask != null && groundMask.intValue == 0)
                {
                    WeatherInspectorGui.Warning(
                        "Ground Mask is Nothing. Automatic population cannot acquire a focus or candidate ground.");
                }

                WeatherInspectorGui.ReadOnlyRow(
                    "Active / Pending",
                    $"{controller.AutomaticPopulationActiveCount} / {controller.AutomaticPopulationPendingCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Retiring / Cooldown",
                    $"{controller.AutomaticPopulationRetiringCount} / {controller.AutomaticPopulationCooldownCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Candidate Checks / Ground Rays / Cloud Samples",
                    $"{controller.AutomaticPopulationCandidateChecksLastTick} / {controller.AutomaticPopulationGroundRaycastsLastTick} / {controller.AutomaticPopulationCloudSamplesLastTick}");
                if (!string.IsNullOrEmpty(
                        controller.AutomaticPopulationSuspensionReason))
                {
                    WeatherInspectorGui.Help(
                        "Population: " +
                        controller.AutomaticPopulationSuspensionReason);
                }
            }
        }

        private void DrawActiveAuthoredRay(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActiveAuthoredRay,
                    "Active Authored Ray",
                    "Shows the first registered authored anchor for concise inspection; all active slots remain renderable."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherLightRayAnchor anchor = controller.GetPrimaryAuthoredAnchor();
                WeatherInspectorGui.ReadOnlyObject(
                    "Anchor",
                    anchor);
                if (anchor == null)
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "State",
                        "No authored anchor registered");
                    WeatherInspectorGui.Help(
                        "Add Weather LightRay Anchor to a scene marker at the desired ground/base centre.");
                    return;
                }

                WeatherInspectorGui.ReadOnlyRow(
                    "Handle",
                    anchor.Handle.ToString());
                WeatherInspectorGui.ReadOnlyRow(
                    "Cloud Policy",
                    anchor.CloudPolicy.ToString());
                if (controller.TryGetSnapshot(
                        anchor.Handle,
                        out WeatherLightRaySnapshot snapshot))
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Source / Lifetime",
                        $"{snapshot.SourceKind} / {snapshot.LifetimePolicy}");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Lifecycle",
                        snapshot.LifecycleState.ToString());
                    WeatherInspectorGui.ReadOnlyRow(
                        "Current Intensity",
                        snapshot.CurrentIntensity);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Cloud Transmission",
                        snapshot.CurrentCloudTransmission);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Base Centre",
                        snapshot.BaseCentreWorld.ToString("F3"));
                    WeatherInspectorGui.ReadOnlyRow(
                        "Presentation Direction",
                        snapshot.RayDirectionWorld.ToString("F3"));
                    WeatherInspectorGui.ReadOnlyRow(
                        "Height",
                        snapshot.Height,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Area Diameter / Radius",
                        $"{snapshot.Descriptor.AreaDiameterMetres:0.###} m / {snapshot.Descriptor.FootprintRadiusMetres:0.###} m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Resolved Beams / Centre Pitch",
                        $"{snapshot.Descriptor.BeamCount} / {snapshot.Descriptor.BeamPitchMetres:0.###} m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Average Atmospheric Beam Width",
                        WeatherLightRayAreaLayout.Calculate(
                            snapshot.Descriptor.AreaDiameterMetres,
                            snapshot.Descriptor.BeamSpacingMetres)
                            .AverageAtmosphericBeamWidthMetres,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Representative Adjacent Overlap",
                        WeatherLightRayAreaLayout.Calculate(
                            snapshot.Descriptor.AreaDiameterMetres,
                            snapshot.Descriptor.BeamSpacingMetres)
                            .AverageAtmosphericOverlapMetres,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Contact Layout Axis",
                        "World X");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Width Weight Range",
                        snapshot.Descriptor.BeamWidthRatioRange.ToString("F2"));
                    WeatherInspectorGui.ReadOnlyRow(
                        "Atmosphere / Softening",
                        $"{snapshot.Descriptor.AtmosphericIntensity:0.###} / {snapshot.Descriptor.SofteningStrength:0.###}");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Real Spot / Screen Complement / Softness",
                        $"{snapshot.Descriptor.SurfaceSpotLightIntensity:0.###} / " +
                        $"{snapshot.Descriptor.ScreenSpaceSurfaceIntensity:0.###} / " +
                        $"{snapshot.Descriptor.FootprintEdgeSoftness:0.###}");
                    Light surfaceSpot = controller.GetSurfaceSpotLight(
                        anchor.Handle);
                    WeatherInspectorGui.ReadOnlyObject(
                        "Runtime Surface Spot",
                        surfaceSpot);
                    if (controller.TryGetSurfaceSpotLightState(
                            anchor.Handle,
                            out float spotHeight,
                            out float spotInnerRadius,
                            out float spotOuterRadius,
                            out float spotAppliedIntensity))
                    {
                        WeatherInspectorGui.ReadOnlyRow(
                            "Spot Height / Inner / Outer",
                            $"{spotHeight:0.###} m / {spotInnerRadius:0.###} m / " +
                            $"{spotOuterRadius:0.###} m");
                        WeatherInspectorGui.ReadOnlyRow(
                            "Applied Spot Intensity",
                            spotAppliedIntensity);
                    }
                    WeatherInspectorGui.ReadOnlyRow(
                        "Sun Warmth Contribution",
                        snapshot.Descriptor.WarmthContribution);
                }
                else
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Snapshot",
                        "Registration pending or stale");
                }
            }
        }

        private void DrawProjectionDiagnostic(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showProjectionDiagnostic,
                    "Cloud Projection Diagnostic",
                    "Controls the accepted V1.0C Scene-view comparison between CPU cloud queries and the shader-sampled cloud overlay."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "showProjectionProbe",
                    "Show CPU Cloud-Sampling Probe",
                    "Draws the accepted high-contrast CPU cloud-transmission grid in Scene view while this Weather object is selected.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeFocusOverride",
                    "Probe Focus Override",
                    "Optional Transform defining the grid centre. This explicit override has priority over the published Cloud Shadow debug overlay.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeFallbackCamera",
                    "Probe Fallback Camera",
                    "Used only when no explicit override and no published Cloud Shadow Controller exist. Camera.main is resolved when this is also unassigned.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeGridResolution",
                    "Grid Resolution",
                    "Number of CPU sample markers per axis. Higher values increase editor-only sampling and drawing work.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeSpanMetres",
                    "World Span (m)",
                    "Editor-only per-axis span covered by the diagnostic square. This does not define runtime LightRay coverage.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeSampleHeightMetres",
                    "Sample Plane Y",
                    "World-space Y coordinate at which the CPU cloud-transmission query is sampled.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "projectionProbeMarkerRadiusMetres",
                    "Marker Screen Scale",
                    "Screen-relative marker scale. The serialized field name remains unchanged to preserve existing data.");

                EditorGUI.BeginChangeCheck();
                showProjectionTransmissionLabels = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Show Transmission Labels",
                        "Displays the numeric CPU transmission beside each marker. This Editor-only option is not serialized."),
                    showProjectionTransmissionLabels);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Frame Projection Probe in Scene View"))
                {
                    FrameProjectionProbe(controller);
                }

                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Focus",
                    controller.ResolvedProbeFocus);
                WeatherInspectorGui.ReadOnlyRow(
                    "Focus Source",
                    FormatProbeFocusSource(controller.ResolvedProbeFocusSource));
                WeatherInspectorGui.ReadOnlyRow(
                    "Probe Centre",
                    controller.ResolvedProbeCentre.ToString("F3"));
                WeatherInspectorGui.Help(
                    "Comparison path: Weather Cloud Shadow Controller -> Debug " +
                    "Visualization -> Debug View -> Cloud + Sun Openings. Green " +
                    "markers align with cyan/open regions; orange aligns with " +
                    "magenta/cloud. V1.0C alignment was accepted from user screenshots.");
            }
        }

        private static string FormatProbeFocusSource(
            WeatherLightRayController.ProbeFocusSource source)
        {
            switch (source)
            {
                case WeatherLightRayController.ProbeFocusSource.InspectorOverride:
                    return "Inspector Override";
                case WeatherLightRayController.ProbeFocusSource.CloudDebugOverlay:
                    return "Published Cloud Debug Overlay";
                case WeatherLightRayController.ProbeFocusSource.AssignedFallbackCamera:
                    return "Assigned Fallback Camera";
                case WeatherLightRayController.ProbeFocusSource.AutomaticMainCamera:
                    return "Automatic Camera.main";
                default:
                    return "Controller Transform Fallback";
            }
        }

        private void DrawVegetationAccentDiagnosticSuite(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showVegetationAccentDiagnostic,
                    "Vegetation Accent Diagnostic Suite",
                    "One consolidated CPU preflight and GPU false-colour proof for the AH geometric LightRay Spot match, shared accent-line control, and vegetation edge-accent override path."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyRow(
                    "Suite State",
                    controller.VegetationAccentDiagnosticSuiteActive
                        ? "Running — vegetation false-colour view active"
                        : "Stopped — normal vegetation shading active");
                WeatherInspectorGui.ReadOnlyRow(
                    "Latest Run ID",
                    controller.VegetationAccentDiagnosticRunId.ToString());
                WeatherInspectorGui.ReadOnlyRow(
                    "CPU Preflight",
                    controller.VegetationAccentDiagnosticCpuVerdict);

                WeatherInspectorGui.Help(
                    "GPU legend: Magenta = published Spot geometry inactive; Red = no additional light; " +
                    "Orange = no geometric Spot match; Purple = matched Spot failed mesh-layer filtering; " +
                    "Yellow = accent direction inactive; Cyan = override not selected; " +
                    "Dark blue = no matched-Spot body radiance; Blue = body radiance but no edge radiance; " +
                    "Green blade-edge strips = full geometric override path reached real edge output. " +
                    "At Accent Line Intensity 0, AH still forces geometric diagnostics but zero artistic edge radiance is expected, so a successful match may remain blue rather than green.");

                string runLabel =
                    controller.VegetationAccentDiagnosticSuiteActive
                        ? "Stop Vegetation Accent Diagnostic Suite"
                        : "Run Vegetation Accent Diagnostic Suite";
                if (GUILayout.Button(runLabel))
                {
                    controller.ToggleVegetationAccentDiagnosticSuite();
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                    Debug.Log(
                        controller.VegetationAccentDiagnosticSuiteActive
                            ? "[Weather LightRay V1.1D-AI4A] Vegetation accent diagnostic suite started. Capture the false-colour vegetation view, then copy the diagnostic results."
                            : "[Weather LightRay V1.1D-AI4A] Vegetation accent diagnostic suite stopped; normal vegetation shading restored.",
                        controller);
                }

                if (GUILayout.Button(
                        "Copy Vegetation Accent Diagnostic Results"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        controller.RefreshVegetationAccentDiagnosticResults();
                    Debug.Log(
                        "[Weather LightRay V1.1D-AI4A] Vegetation accent diagnostic results copied to clipboard.",
                        controller);
                }
            }
        }

        private void DrawActionsReports(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActionsReports,
                    "Actions & Reports",
                    "Copies the complete registration, shared descriptor, lifecycle, renderer, and cloud-projection state."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                bool hasProceduralTestPair =
                    controller.IsValid(proceduralTestHandleA) ||
                    controller.IsValid(proceduralTestHandleB);
                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    string proceduralTestLabel = hasProceduralTestPair
                        ? "Release Procedural Test Pair"
                        : "Spawn Procedural Test Pair";
                    if (GUILayout.Button(proceduralTestLabel))
                    {
                        if (hasProceduralTestPair)
                        {
                            ReleaseProceduralTestPair(controller);
                        }
                        else
                        {
                            SpawnProceduralTestPair(controller);
                        }
                    }
                }

                if (!Application.isPlaying)
                {
                    WeatherInspectorGui.Help(
                        "Enter Play Mode to use Spawn Procedural Test Pair. The action calls the production TrySpawnProceduralRay API directly and creates no GameObjects or scene changes.");
                }

                bool cloudTestValid =
                    controller.IsValid(cloudAwareTestHandle);
                if (!cloudTestValid)
                {
                    cloudAwareTestPhase = 0;
                }

                string cloudTestLabel = cloudAwareTestPhase == 0
                    ? "Spawn Cloud-Aware Test Ray"
                    : cloudAwareTestPhase == 1
                        ? "Update Cloud-Aware Test Ray"
                        : "Release Cloud-Aware Test Ray";
                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button(cloudTestLabel))
                    {
                        RunCloudAwareTestStep(controller);
                    }
                }

                if (!Application.isPlaying)
                {
                    WeatherInspectorGui.Help(
                        "Enter Play Mode to run the three-step cloud-aware smoke test: spawn through a deterministic provider, update the same handle from a moved resolved opening, then release it.");
                }

                if (GUILayout.Button("Run Beam Evolution Runtime Audit"))
                {
                    beamEvolutionRuntimeAudit = BuildBeamEvolutionRuntimeAudit(
                        controller);
                    Debug.Log(beamEvolutionRuntimeAudit, controller);
                }

                using (new EditorGUI.DisabledScope(
                    string.IsNullOrEmpty(beamEvolutionRuntimeAudit)))
                {
                    if (GUILayout.Button("Copy Beam Evolution Runtime Audit"))
                    {
                        EditorGUIUtility.systemCopyBuffer =
                            beamEvolutionRuntimeAudit;
                    }
                }

                if (GUILayout.Button("Copy LightRay V1.2E Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        controller.BuildComprehensiveReport();
                    Debug.Log(
                        "[Weather LightRay V1.2E] Report copied to clipboard.",
                        controller);
                }
            }
        }


        private void RunCloudAwareTestStep(
            WeatherLightRayController controller)
        {
            WeatherLightRayAnchor anchor = controller.GetPrimaryAuthoredAnchor();
            Vector3 centre = anchor != null
                ? anchor.transform.position
                : controller.transform.position;
            WeatherLightRaySourceKind sourceKind = anchor != null
                ? anchor.SourceKind
                : WeatherLightRaySourceKind.Sun;
            float diameter = anchor != null
                ? anchor.AreaDiameterMetres
                : controller.ActivePreset != null
                    ? controller.ActivePreset.DefaultAreaDiameterMetres
                    : 4f;
            // Ordinary cloud-aware rays inherit the active celestial source
            // direction. A non-zero value is reserved for an intentional
            // per-instance override.
            Vector3 direction = Vector3.zero;
            var settings = new WeatherLightRayCloudSpawnSettings(
                variationSeed: 22001u,
                localIntensityMultiplier: 1f,
                lifetimePolicy:
                    WeatherLightRayLifetimePolicy.ExternallyControlled,
                initiallyVisible: true,
                runtimeCloudPolicy: WeatherLightRayCloudPolicy.IgnoreClouds,
                sourceGatePolicy:
                    WeatherLightRaySourceGatePolicy.RequireActiveSource);

            if (cloudAwareTestPhase == 0)
            {
                var opening = new WeatherLightRayCloudOpening(
                    stableIdentity: 22001L,
                    sourceKind: sourceKind,
                    baseCentreWorld: centre + Vector3.forward * diameter,
                    rayDirectionWorld: direction,
                    areaDiameterMetres: diameter,
                    clearanceStrength: 0.9f,
                    edgeSoftnessSignal: 0.5f,
                    confidence: 1f,
                    dataVersion: 1u);
                var query = new WeatherLightRayCloudQuery(
                    sourceKind,
                    opening.BaseCentreWorld,
                    diameter * 0.5f,
                    diameter * 1.5f,
                    minimumConfidence: 0.75f,
                    preferredRayDirectionWorld: direction,
                    identityHint: opening.StableIdentity);
                var provider = new DeterministicCloudOpeningProvider(opening);
                if (!controller.TrySpawnCloudAwareRay(
                        query,
                        provider,
                        settings,
                        out cloudAwareTestHandle,
                        out string error))
                {
                    Debug.LogError(
                        "[Weather LightRay V1.2C1] Cloud-aware spawn failed: " +
                        error,
                        controller);
                    cloudAwareTestHandle = default;
                    cloudAwareTestPhase = 0;
                    return;
                }

                cloudAwareTestPhase = 1;
                Debug.Log(
                    "[Weather LightRay V1.2C1] Cloud-aware test ray spawned through the provider as " +
                    cloudAwareTestHandle +
                    ". Click Update Cloud-Aware Test Ray next.",
                    controller);
            }
            else if (cloudAwareTestPhase == 1)
            {
                var movedOpening = new WeatherLightRayCloudOpening(
                    stableIdentity: 22001L,
                    sourceKind: sourceKind,
                    baseCentreWorld:
                        centre + Vector3.forward * diameter +
                        Vector3.right * Mathf.Max(1f, diameter * 0.5f),
                    rayDirectionWorld: direction,
                    areaDiameterMetres: diameter * 1.15f,
                    clearanceStrength: 0.65f,
                    edgeSoftnessSignal: 0.7f,
                    confidence: 1f,
                    dataVersion: 2u);
                WeatherLightRayHandle previousHandle = cloudAwareTestHandle;
                if (!controller.TrySpawnOrUpdateResolvedCloudOpening(
                        ref cloudAwareTestHandle,
                        movedOpening,
                        settings,
                        out bool spawned,
                        out string error))
                {
                    Debug.LogError(
                        "[Weather LightRay V1.2C1] Cloud-aware update failed: " +
                        error,
                        controller);
                    return;
                }

                if (spawned || cloudAwareTestHandle != previousHandle)
                {
                    Debug.LogError(
                        "[Weather LightRay V1.2C1] The cloud-aware update replaced the stable handle unexpectedly.",
                        controller);
                    return;
                }

                cloudAwareTestPhase = 2;
                Debug.Log(
                    "[Weather LightRay V1.2C1] Cloud-aware opening updated in place with stable handle " +
                    cloudAwareTestHandle +
                    ". Click Release Cloud-Aware Test Ray next.",
                    controller);
            }
            else
            {
                if (!controller.TryReleaseProceduralRay(
                        cloudAwareTestHandle,
                        out string error))
                {
                    Debug.LogError(
                        "[Weather LightRay V1.2C1] Cloud-aware test release failed: " +
                        error,
                        controller);
                    return;
                }

                Debug.Log(
                    "[Weather LightRay V1.2C1] Cloud-aware test ray released.",
                    controller);
                cloudAwareTestHandle = default;
                cloudAwareTestPhase = 0;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
            Repaint();
        }

        private void SpawnProceduralTestPair(
            WeatherLightRayController controller)
        {
            WeatherLightRayAnchor anchor = controller.GetPrimaryAuthoredAnchor();
            Vector3 centre = anchor != null
                ? anchor.transform.position
                : controller.transform.position;
            WeatherLightRaySourceKind sourceKind = anchor != null
                ? anchor.SourceKind
                : WeatherLightRaySourceKind.Sun;
            float diameter = anchor != null
                ? anchor.AreaDiameterMetres
                : controller.ActivePreset != null
                    ? controller.ActivePreset.DefaultAreaDiameterMetres
                    : 4f;
            float separation = Mathf.Max(2f, diameter * 0.75f);

            var requestA = new WeatherLightRaySpawnRequest(
                centre + Vector3.left * separation,
                diameter,
                12001u,
                localIntensityMultiplier: 1f,
                lifetimePolicy: WeatherLightRayLifetimePolicy.ExternallyControlled,
                initiallyVisible: true,
                sourceKind: sourceKind,
                cloudPolicy: WeatherLightRayCloudPolicy.IgnoreClouds,
                sourceGatePolicy: WeatherLightRaySourceGatePolicy.RequireActiveSource,
                externalIdentity: 12001L);
            var requestB = new WeatherLightRaySpawnRequest(
                centre + Vector3.right * separation,
                diameter,
                12002u,
                localIntensityMultiplier: 0.8f,
                lifetimePolicy: WeatherLightRayLifetimePolicy.ExternallyControlled,
                initiallyVisible: true,
                sourceKind: sourceKind,
                cloudPolicy: WeatherLightRayCloudPolicy.IgnoreClouds,
                sourceGatePolicy: WeatherLightRaySourceGatePolicy.RequireActiveSource,
                externalIdentity: 12002L);

            if (!controller.TrySpawnProceduralRay(
                    requestA,
                    out proceduralTestHandleA,
                    out string errorA))
            {
                Debug.LogError(
                    "[Weather LightRay V1.2C1] Failed to spawn procedural test ray A: " +
                    errorA,
                    controller);
                proceduralTestHandleA = default;
                proceduralTestHandleB = default;
                return;
            }

            if (!controller.TrySpawnProceduralRay(
                    requestB,
                    out proceduralTestHandleB,
                    out string errorB))
            {
                controller.TryReleaseProceduralRay(
                    proceduralTestHandleA,
                    out _);
                Debug.LogError(
                    "[Weather LightRay V1.2C1] Failed to spawn procedural test ray B: " +
                    errorB,
                    controller);
                proceduralTestHandleA = default;
                proceduralTestHandleB = default;
                return;
            }

            Debug.Log(
                "[Weather LightRay V1.2C1] Spawned procedural test pair: " +
                proceduralTestHandleA + " and " + proceduralTestHandleB +
                ". Central Storage > Procedural Slots should now report 2.",
                controller);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private void ReleaseProceduralTestPair(
            WeatherLightRayController controller)
        {
            if (controller.IsValid(proceduralTestHandleA))
            {
                controller.TryReleaseProceduralRay(
                    proceduralTestHandleA,
                    out _);
            }

            if (controller.IsValid(proceduralTestHandleB))
            {
                controller.TryReleaseProceduralRay(
                    proceduralTestHandleB,
                    out _);
            }

            proceduralTestHandleA = default;
            proceduralTestHandleB = default;
            Debug.Log(
                "[Weather LightRay V1.2C1] Released the procedural test pair. Central Storage > Procedural Slots should now report 0.",
                controller);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private static string BuildBeamEvolutionRuntimeAudit(
            WeatherLightRayController controller)
        {
            int activeCount = controller.CopyActiveSnapshots(null);
            var snapshots = new WeatherLightRaySnapshot[
                Mathf.Max(0, activeCount)];
            int copied = controller.CopyActiveSnapshots(snapshots);
            var builder = new System.Text.StringBuilder(2048);
            builder.AppendLine("[Weather LightRay V1.2C1 Runtime Audit]");
            builder.Append("Active snapshots / authored / procedural: ")
                .Append(copied)
                .Append(" / ")
                .Append(controller.ActiveAuthoredRayCount)
                .Append(" / ")
                .AppendLine(controller.ActiveProceduralRayCount.ToString());
            builder.Append("Last rendered compatible zones: ")
                .AppendLine(WeatherLightRayRenderPass.LastVisibleZoneCount.ToString());
            builder.Append("Buffered compatible zones: ")
                .AppendLine(WeatherLightRayRenderPass.LastBufferedZoneCount.ToString());
            builder.Append("Last rendered beams: ")
                .AppendLine(WeatherLightRayRenderPass.LastTotalBeamCount.ToString());
            builder.Append("Beam buffer used/capacity: ")
                .Append(WeatherLightRayRenderPass.LastTotalBeamCount)
                .Append(" / ")
                .AppendLine(WeatherLightRayRenderPass.LastBeamBufferCapacity.ToString());
            builder.Append("Zone buffer used/capacity: ")
                .Append(WeatherLightRayRenderPass.LastBufferedZoneCount)
                .Append(" / ")
                .AppendLine(WeatherLightRayRenderPass.LastZoneBufferCapacity.ToString());
            builder.Append("Endpoint uploads: ")
                .AppendLine(WeatherLightRayRenderPass.LastEndpointUploadCount.ToString());
            builder.Append("Zone-state uploads: ")
                .AppendLine(WeatherLightRayRenderPass.LastZoneUploadCount.ToString());

            for (int index = 0; index < copied; index++)
            {
                WeatherLightRaySnapshot snapshot = snapshots[index];
                builder.Append("Zone ").Append(index)
                    .Append(" · ").Append(snapshot.Handle)
                    .Append(" · origin ").Append(snapshot.OriginKind)
                    .Append(" · source ").Append(snapshot.SourceKind)
                    .Append(" · beams ").Append(snapshot.Descriptor.BeamCount)
                    .Append(" · pitch ").Append(snapshot.Descriptor.BeamPitchMetres.ToString("0.###"))
                    .Append(" · seeds ").Append(snapshot.EvolutionCurrentSeed)
                    .Append(" -> ").Append(snapshot.EvolutionNextSeed)
                    .Append(" · blend ").Append(snapshot.EvolutionBlend.ToString("0.000"))
                    .Append(" · duration ").Append(snapshot.EvolutionDurationSeconds.ToString("0.###"))
                    .Append("s · transitions ").Append(snapshot.CompletedEvolutionTransitions)
                    .Append(" · intensity ").Append(snapshot.CurrentIntensity.ToString("0.###"))
                    .AppendLine();
            }

            return builder.ToString();
        }

        private void DrawLiveStatus(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLiveStatus,
                    "Live Status",
                    "Read-only publisher, source, renderer, storage, cloud-controller, and probe state."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyRow(
                    "Published",
                    controller.IsPublished ? "Yes" : "No");
                WeatherInspectorGui.ReadOnlyRow(
                    "Storage",
                    $"{controller.ActiveRayCount} / {controller.StorageCapacity}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Authored / Procedural",
                    $"{controller.ActiveAuthoredRayCount} / {controller.ActiveProceduralRayCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Enabled Surface Spot Lights",
                    controller.ActiveSurfaceSpotLightCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Source",
                    controller.SunSourceState.Available
                        ? "Available"
                        : "Unavailable");
                WeatherInspectorGui.ReadOnlyRow(
                    "Moon Source",
                    controller.MoonSourceState.Available
                        ? "Available"
                        : "Unavailable by V1.1 design");
                WeatherInspectorGui.ReadOnlyObject(
                    "Render Camera",
                    controller.ResolvedRenderCamera);
                WeatherInspectorGui.ReadOnlyRow(
                    "Renderer Feature Count",
                    WeatherLightRayRendererFeature.ActiveFeatureCount);
                WeatherInspectorGui.ReadOnlyObject(
                    "Cloud Controller",
                    WeatherCloudShadowController.PublishedController);
                WeatherInspectorGui.ReadOnlyRow(
                    "Automatic Population",
                    $"{controller.AutomaticPopulationActiveCount} active / {controller.AutomaticPopulationPendingCount} pending");
                WeatherInspectorGui.ReadOnlyRow(
                    "Population Suspension",
                    string.IsNullOrEmpty(
                        controller.AutomaticPopulationSuspensionReason)
                        ? "None"
                        : controller.AutomaticPopulationSuspensionReason);
                WeatherInspectorGui.ReadOnlyObject(
                    "Probe Focus",
                    controller.ResolvedProbeFocus);
                WeatherInspectorGui.ReadOnlyRow(
                    "Probe Focus Source",
                    FormatProbeFocusSource(controller.ResolvedProbeFocusSource));
            }
        }

        private void OnSceneGUI()
        {
            var controller = target as WeatherLightRayController;
            if (controller == null ||
                Selection.activeGameObject != controller.gameObject)
            {
                return;
            }

            if (controller.ShowProjectionProbe)
            {
                DrawProjectionProbe(controller);
            }

            if (controller.ShowAutomaticPopulationCandidates)
            {
                DrawAutomaticPopulationCandidates(controller);
            }
        }

        private void DrawProjectionProbe(
            WeatherLightRayController controller)
        {
            int resolution = controller.ProjectionProbeGridResolution;
            float markerScale = Mathf.Max(
                0.01f,
                controller.ProjectionProbeMarkerRadiusMetres);
            WeatherCloudShadowController cloudController =
                WeatherCloudShadowController.PublishedController;
            float shadedTransmission = cloudController != null
                ? cloudController.ShadedTransmission
                : 1f;

            SceneView sceneView = SceneView.currentDrawingSceneView != null
                ? SceneView.currentDrawingSceneView
                : SceneView.lastActiveSceneView;
            Camera sceneCamera = sceneView != null
                ? sceneView.camera
                : Camera.current;
            Vector3 markerNormal = sceneCamera != null
                ? -sceneCamera.transform.forward
                : Vector3.up;
            Vector3 labelRight = sceneCamera != null
                ? sceneCamera.transform.right
                : Vector3.right;
            Vector3 labelUp = sceneCamera != null
                ? sceneCamera.transform.up
                : Vector3.up;

            Color previousColour = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            try
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        bool success = controller.TryGetProjectionProbeSample(
                            x,
                            y,
                            out Vector3 position,
                            out WeatherCloudTransmissionSample sample);
                        float radius = HandleUtility.GetHandleSize(position) *
                            markerScale;
                        DrawProjectionProbeMarker(
                            position,
                            markerNormal,
                            radius,
                            ResolveSampleColour(
                                success,
                                sample,
                                shadedTransmission));

                        if (showProjectionTransmissionLabels)
                        {
                            Vector3 labelPosition = position +
                                labelRight * (radius * 1.65f) +
                                labelUp * (radius * 0.75f);
                            string label = success
                                ? $"T {sample.Transmission:0.000}"
                                : "Query failed";
                            Handles.Label(
                                labelPosition,
                                label,
                                GetProjectionTransmissionLabelStyle());
                        }
                    }
                }

                DrawProjectionProbeBoundary(controller);
            }
            finally
            {
                Handles.color = previousColour;
                Handles.zTest = previousZTest;
            }
        }

        private void DrawAutomaticPopulationCandidates(
            WeatherLightRayController controller)
        {
            int required = controller.CopyAutomaticPopulationDebugRecords(null);
            if (populationDebugRecords == null ||
                populationDebugRecords.Length < required)
            {
                populationDebugRecords =
                    new WeatherLightRayPopulationDebugRecord[
                        Mathf.NextPowerOfTwo(Mathf.Max(1, required))];
            }

            int count = controller.CopyAutomaticPopulationDebugRecords(
                populationDebugRecords);
            Color previousColour = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            try
            {
                Handles.color = new Color(0.2f, 0.75f, 1f, 0.35f);
                Handles.DrawWireDisc(
                    controller.AutomaticPopulationFocusWorld,
                    Vector3.up,
                    controller.AutomaticPopulationActiveRadiusMetres);
                for (int index = 0; index < count; index++)
                {
                    WeatherLightRayPopulationDebugRecord record =
                        populationDebugRecords[index];
                    Handles.color = ResolvePopulationCandidateColour(
                        record.State);
                    float radius = HandleUtility.GetHandleSize(
                        record.PositionWorld) * 0.08f;
                    Handles.DrawSolidDisc(
                        record.PositionWorld + Vector3.up * 0.03f,
                        Vector3.up,
                        radius);
                    Handles.DrawWireDisc(
                        record.PositionWorld + Vector3.up * 0.03f,
                        Vector3.up,
                        radius * 1.6f);
                }
            }
            finally
            {
                Handles.color = previousColour;
                Handles.zTest = previousZTest;
            }
        }

        private static Color ResolvePopulationCandidateColour(
            WeatherLightRayPopulationCandidateState state)
        {
            switch (state)
            {
                case WeatherLightRayPopulationCandidateState.Active:
                    return new Color(0.1f, 1f, 0.2f, 0.9f);
                case WeatherLightRayPopulationCandidateState.Pending:
                    return new Color(0.1f, 0.9f, 1f, 0.9f);
                case WeatherLightRayPopulationCandidateState.Retiring:
                    return new Color(1f, 0.55f, 0.05f, 0.9f);
                default:
                    return new Color(1f, 0.85f, 0.1f, 0.75f);
            }
        }

        private static void DrawProjectionProbeMarker(
            Vector3 position,
            Vector3 normal,
            float radius,
            Color classificationColour)
        {
            Handles.color = new Color(0.015f, 0.015f, 0.015f, 1f);
            Handles.DrawSolidDisc(
                position,
                normal,
                radius * 1.55f);
            Handles.color = Color.white;
            Handles.DrawSolidDisc(
                position,
                normal,
                radius * 1.28f);
            Handles.color = classificationColour;
            Handles.DrawSolidDisc(
                position,
                normal,
                radius);
        }

        private static void DrawProjectionProbeBoundary(
            WeatherLightRayController controller)
        {
            float halfSpan = controller.ProjectionProbeSpanMetres * 0.5f;
            Vector3 centre = controller.ResolvedProbeCentre;
            centre.y = controller.ProjectionProbeSampleHeightMetres;
            Vector3 a = centre + new Vector3(-halfSpan, 0f, -halfSpan);
            Vector3 b = centre + new Vector3(halfSpan, 0f, -halfSpan);
            Vector3 c = centre + new Vector3(halfSpan, 0f, halfSpan);
            Vector3 d = centre + new Vector3(-halfSpan, 0f, halfSpan);
            Vector3[] boundary = { a, b, c, d, a };

            Handles.color = new Color(0.02f, 0.02f, 0.02f, 1f);
            Handles.DrawAAPolyLine(8f, boundary);
            Handles.color = new Color(1f, 0.82f, 0.05f, 1f);
            Handles.DrawAAPolyLine(4f, boundary);
        }

        private static Color ResolveSampleColour(
            bool success,
            WeatherCloudTransmissionSample sample,
            float shadedTransmission)
        {
            if (!success || !sample.IsUsable)
            {
                return new Color(1f, 0.08f, 0.04f, 1f);
            }

            if (!sample.IsStable)
            {
                return new Color(1f, 0.82f, 0.05f, 1f);
            }

            if (!sample.UsesCloudField)
            {
                return new Color(0.1f, 1f, 0.2f, 1f);
            }

            float openThreshold = Mathf.Lerp(
                Mathf.Clamp01(shadedTransmission),
                1f,
                0.5f);
            return sample.Transmission >= openThreshold
                ? new Color(0.1f, 1f, 0.2f, 1f)
                : new Color(1f, 0.42f, 0.02f, 1f);
        }

        private static GUIStyle GetProjectionTransmissionLabelStyle()
        {
            if (projectionTransmissionLabelStyle != null)
            {
                return projectionTransmissionLabelStyle;
            }

            projectionTransmissionLabelStyle = new GUIStyle(
                EditorStyles.miniBoldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(3, 3, 1, 1)
            };
            projectionTransmissionLabelStyle.normal.textColor = Color.white;
            projectionTransmissionLabelStyle.normal.background =
                Texture2D.blackTexture;
            return projectionTransmissionLabelStyle;
        }

        private static void FrameProjectionProbe(
            WeatherLightRayController controller)
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                Debug.LogWarning(
                    "[Weather LightRay V1.0B] No active Scene view is available " +
                    "to frame the projection probe.",
                    controller);
                return;
            }

            float span = Mathf.Max(1f, controller.ProjectionProbeSpanMetres);
            Vector3 centre = controller.ResolvedProbeCentre;
            centre.y = controller.ProjectionProbeSampleHeightMetres;
            var bounds = new Bounds(
                centre,
                new Vector3(span, Mathf.Max(2f, span * 0.1f), span));
            sceneView.Frame(bounds, false);
            sceneView.Repaint();
        }

    }
}
