using ProgrammaticStylized3D.Weather.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherLightRayController))]
    public sealed class WeatherLightRayControllerEditor : UnityEditor.Editor
    {
        private bool showCoreSetup;
        private bool showSourceRendering;
        private bool showAutomaticPopulation;
        private bool showPopulationMain;
        private bool showPopulationAdvanced;
        private bool showAdvancedSystem;
        private bool showDiagnostics;
        private bool showRuntimeStatus;
        private bool showReport;

        private WeatherLightRayPopulationDebugRecord[] populationDebugRecords =
            new WeatherLightRayPopulationDebugRecord[64];
        private readonly Vector3[] populationFootprintPoints =
            new Vector3[8];

        public override void OnInspectorGUI()
        {
            var controller = (WeatherLightRayController)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);

            WeatherInspectorGui.Info(
                "LightRays are a generic rendering and gameplay-capable system. " +
                "The current automatic producer creates cloud-opening atmospheric rays, " +
                "but source eligibility and future automatic preset selection belong to Weather orchestration rather than presets or the core LightRay system.");
            DrawImmediateWarnings(controller);

            DrawCoreSetup();
            DrawSourceRendering();
            DrawAutomaticPopulation();
            DrawAdvancedSystem();
            DrawDiagnostics();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                controller.RefreshNow();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            DrawRuntimeStatus(controller);
            DrawReport(controller);
        }

        private static void DrawImmediateWarnings(
            WeatherLightRayController controller)
        {
            if (WeatherLightRayController.ActiveControllerCount > 1)
            {
                WeatherInspectorGui.Warning(
                    "Multiple Weather LightRay Controllers are active. The most recently enabled controller is published.");
            }

            if (!controller.IsPublished)
            {
                WeatherInspectorGui.Warning(
                    "This controller is not the active Weather LightRay publisher.");
            }

            if (controller.DefaultPreset == null)
            {
                WeatherInspectorGui.Warning(
                    "No Default Preset is assigned. Automatic atmospheric population and rays that do not provide a Preset Override cannot become active. Explicit per-ray overrides remain valid.");
            }

            if (WeatherLightRayRendererFeature.ActiveFeatureCount == 0)
            {
                WeatherInspectorGui.Warning(
                    "No active Weather LightRay Renderer Feature is loaded in the current URP renderer.");
            }

            if (!string.IsNullOrEmpty(controller.LastError))
            {
                WeatherInspectorGui.Error(controller.LastError);
            }
        }

        private void DrawCoreSetup()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCoreSetup,
                    "Core Setup",
                    "Primary LightRay activation, appearance, and edit-preview authoring."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "lightRaysEnabled",
                    "LightRays Enabled",
                    "Global LightRay rendering and lifecycle gate. This does not define which runtime systems are allowed to request rays.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "activePreset",
                    "Default Preset",
                    "Inherited visual preset for rays that do not provide a Preset Override. Future Weather orchestration may change this default without affecting explicit per-ray overrides.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Edit Mode Preview",
                    "Keeps authored LightRay registration, source state, lifecycle, rendering data, and diagnostics current outside Play Mode.");
            }
        }

        private void DrawSourceRendering()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showSourceRendering,
                    "Source & Rendering",
                    "Current directional-source binding and render-camera authoring. These bindings do not make the generic LightRay system source-owned."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunOverride",
                    "Atmospheric Directional Source Override",
                    "Optional explicit directional light used by the current daylight atmospheric population and Sun-bound ray requests. When unassigned, RenderSettings.sun supplies the current source.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunProfile",
                    "Atmospheric Source Profile",
                    "Optional availability, colour, elevation-fade, and presentation profile for the currently resolved daylight directional source.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "renderCameraOverride",
                    "Render Camera Override",
                    "Optional gameplay camera used by the renderer and automatic-population footprint. When unassigned, Camera.main is resolved.");
            }
        }

        private void DrawAutomaticPopulation()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showAutomaticPopulation,
                    "Automatic Population",
                    "Authoring for the current cloud-opening atmospheric population. Runtime state is shown only under Runtime Status."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                if (WeatherInspectorGui.Foldout(
                        ref showPopulationMain,
                        "Main Controls"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationEnabled",
                            "Enabled",
                            "Runs the current atmospheric automatic producer in Play Mode. The generic authored and procedural LightRay APIs remain available independently.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationGroundMask",
                            "Ground Mask",
                            "Physics layers eligible for camera-footprint projection and candidate ground acquisition. A non-empty mask is required.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationDesiredRayCount",
                            "Target Ray Count",
                            "Preferred number of active automatically populated atmospheric rays.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationMaximumRayCount",
                            "Ray Budget",
                            "Hard automatic-ray budget. Candidate checks per update are derived as clamp(Ray Budget × 2, 4, 64).");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationMinimumSpacingMetres",
                            "Minimum Ray Spacing (m)",
                            "Minimum world-space spacing between automatic candidates and all active LightRays.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationOffscreenMarginMetres",
                            "Camera Margin (m)",
                            "Outward expansion applied to the projected camera-ground footprint.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationMinimumClearance",
                            "Minimum Openness",
                            "Minimum normalized cloud openness required by the six-sample present-and-future placement test.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationSpawnFadeDurationSeconds",
                            "Spawn Fade Duration (s)",
                            "Fade-in duration assigned to newly spawned automatic rays.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationDespawnFadeDurationSeconds",
                            "Despawn Fade Duration (s)",
                            "Fade-out duration used when an automatic ray retires.");
                        WeatherInspectorGui.MinMaxProperties(
                            serializedObject,
                            "Assigned Ray Lifetime",
                            "automaticPopulationMinimumRayLifetimeSeconds",
                            "Minimum Ray Lifetime (s)",
                            "Minimum deterministic lifetime assigned to a spawned automatic ray.",
                            "automaticPopulationMaximumRayLifetimeSeconds",
                            "Maximum Ray Lifetime (s)",
                            "Maximum deterministic lifetime assigned to a spawned automatic ray.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationReplacementDelaySeconds",
                            "Candidate Reuse Delay (s)",
                            "Cooldown before a retired world-cell identity may be considered again.");
                    }
                }

                if (WeatherInspectorGui.Foldout(
                        ref showPopulationAdvanced,
                        "Advanced Controls"))
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationSeed",
                            "Population Seed",
                            "Deterministic world-cell placement seed.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationFocusOverride",
                            "Population Focus Override",
                            "Optional XZ target that translates the complete camera-ground footprint without changing its shape. A valid render camera remains required.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationEvaluationRateHz",
                            "Population Update Rate (Hz)",
                            "Fixed cadence for automatic lifecycle and candidate evaluation.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationInvalidGraceDurationSeconds",
                            "Offscreen Exit Grace (s)",
                            "How long an active automatic ray may remain outside the translated camera footprint before retirement begins.");
                        WeatherInspectorGui.Property(
                            serializedObject,
                            "automaticPopulationMaximumGroundSlopeDegrees",
                            "Maximum Ground Slope (°)",
                            "Maximum accepted candidate-ground slope relative to world up.");
                    }
                }
            }
        }

        private void DrawAdvancedSystem()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showAdvancedSystem,
                    "Advanced System",
                    "Fixed storage and cross-system transition policy."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumActiveRays",
                    "Storage Capacity",
                    "Fixed central LightRay slot capacity shared by authored, gameplay-requested, and automatic rays.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudEvolutionResumeThreshold",
                    "Cloud Transition Spawn Resume",
                    "New automatic atmospheric spawning remains paused during a cloud-pattern transition until progress reaches this value. Existing rays continue normally.");
            }
        }

        private void DrawDiagnostics()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showDiagnostics,
                    "Diagnostics",
                    "Editable renderer and population visualization controls."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "renderDebugView",
                    "Render Debug View",
                    "Selects the LightRay Renderer Feature output used for visual diagnosis.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "showAutomaticPopulationCandidates",
                    "Show Population Debug",
                    "Draws the active camera footprint and automatic candidate states in the Scene view.");
            }
        }

        private void DrawRuntimeStatus(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showRuntimeStatus,
                    "Runtime Status",
                    "The sole read-only LightRay telemetry surface."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField("Overview", EditorStyles.boldLabel);
                WeatherInspectorGui.ReadOnlyRow(
                    "Published",
                    controller.IsPublished ? "Yes" : "No");
                WeatherInspectorGui.ReadOnlyObject(
                    "Default Preset",
                    controller.DefaultPreset);
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Camera",
                    controller.ResolvedRenderCamera);
                WeatherInspectorGui.ReadOnlyRow(
                    "Rays Total / Authored / Procedural",
                    $"{controller.ActiveRayCount} / {controller.ActiveAuthoredRayCount} / {controller.ActiveProceduralRayCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Surface Spot Lights",
                    controller.ActiveSurfaceSpotLightCount);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Sun Source Binding", EditorStyles.boldLabel);
                WeatherLightRaySourceState source = controller.SunSourceState;
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Light",
                    source.SourceLight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Availability",
                    source.Available ? "Available" : "Unavailable");
                WeatherInspectorGui.ReadOnlyRow(
                    "Gate Weight",
                    source.AvailabilityWeight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Ray Direction",
                    source.RayDirectionWorld.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Elevation / Intensity",
                    $"{source.Elevation:0.###} / {source.Intensity:0.###}");
                if (!source.Available &&
                    !string.IsNullOrEmpty(source.UnavailableReason))
                {
                    WeatherInspectorGui.Help(source.UnavailableReason);
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
                WeatherInspectorGui.ReadOnlyRow(
                    "Renderer Feature Count",
                    WeatherLightRayRendererFeature.ActiveFeatureCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Debug View",
                    controller.RenderDebugView.ToString());
                WeatherInspectorGui.ReadOnlyRow(
                    "Presentation Groups",
                    WeatherLightRayRenderPass.LastPresentationGroupCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Vegetation Additional Lights / Overrides",
                    $"{controller.PublishedVegetationAdditionalLightCount} / {controller.PublishedVegetationWeatherOverrideCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Vegetation Buffer Capacity / Overflow",
                    $"{controller.PublishedVegetationAccentBufferCapacity} / {(controller.PublishedVegetationAccentIndexOverflow ? "Yes" : "No")}");

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Storage", EditorStyles.boldLabel);
                WeatherInspectorGui.ReadOnlyRow(
                    "Used / Free / Capacity",
                    $"{controller.ActiveRayCount} / {Mathf.Max(0, controller.StorageCapacity - controller.ActiveRayCount)} / {controller.StorageCapacity}");

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Automatic Population", EditorStyles.boldLabel);
                WeatherInspectorGui.ReadOnlyRow(
                    "State",
                    controller.AutomaticPopulationState.ToString());
                WeatherInspectorGui.ReadOnlyRow(
                    "Status Reason",
                    string.IsNullOrEmpty(controller.AutomaticPopulationStatusReason)
                        ? "None"
                        : controller.AutomaticPopulationStatusReason);
                WeatherInspectorGui.ReadOnlyRow(
                    "Focus / Enclosing Radius",
                    controller.AutomaticPopulationFocusWorld.ToString("F3") +
                        $" / {controller.AutomaticPopulationActiveRadiusMetres:0.###} m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Active / Retiring / Cooldown",
                    $"{controller.AutomaticPopulationActiveCount} / {controller.AutomaticPopulationRetiringCount} / {controller.AutomaticPopulationCooldownCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Cells / Derived Candidate Checks",
                    $"{controller.AutomaticPopulationCellsInActiveRegion} / {controller.AutomaticPopulationDerivedCandidateChecksPerUpdate}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Derived Ground Raycast Distance",
                    $"{controller.AutomaticPopulationDerivedGroundRaycastDistanceMetres:0.###} m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Last Checks / Ground Raycasts / Cloud Samples",
                    $"{controller.AutomaticPopulationCandidateChecksLastTick} / {controller.AutomaticPopulationGroundRaycastsLastTick} / {controller.AutomaticPopulationCloudSamplesLastTick}");

                WeatherCloudShadowController cloudController =
                    WeatherCloudShadowController.PublishedController;
                if (cloudController != null)
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Cloud Transition / Spawn Resume",
                        $"{cloudController.EvolutionProgress:P1} / {controller.CloudEvolutionResumeThreshold:P0}");
                }

                if (!string.IsNullOrEmpty(controller.LastError))
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField("Errors", EditorStyles.boldLabel);
                    WeatherInspectorGui.Error(controller.LastError);
                }
            }
        }

        private void DrawReport(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showReport,
                    "Report",
                    "Copies the complete current LightRay, source, rendering, storage, and automatic-population report."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                if (GUILayout.Button("Copy LightRay Report"))
                {
                    controller.RefreshNow();
                    EditorGUIUtility.systemCopyBuffer =
                        controller.BuildComprehensiveReport();
                }
            }
        }

        private void OnSceneGUI()
        {
            var controller = (WeatherLightRayController)target;
            if (controller == null ||
                !controller.ShowAutomaticPopulationCandidates)
            {
                return;
            }

            DrawAutomaticPopulationCandidates(controller);
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
                int footprintCount = controller.CopyAutomaticPopulationFootprint(
                    populationFootprintPoints);
                if (footprintCount >= 3)
                {
                    for (int index = 0; index < footprintCount; index++)
                    {
                        int next = (index + 1) % footprintCount;
                        Handles.DrawLine(
                            populationFootprintPoints[index],
                            populationFootprintPoints[next]);
                    }
                }

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
    }
}
