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
        private bool showActiveAuthoredRay;
        private bool showProjectionDiagnostic;
        private bool showActionsReports;
        private bool showLiveStatus;
        private bool showProjectionTransmissionLabels;

        private static GUIStyle projectionTransmissionLabelStyle;

        public override void OnInspectorGUI()
        {
            var controller = (WeatherLightRayController)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);

            WeatherInspectorGui.Info(
                "Weather LightRay V1.1A/B renders one authored ray through the " +
                "mandatory hybrid path. The anchor now exposes the shared per-ray " +
                "source, lifecycle, strand, surface, and evolution descriptor used " +
                "by future procedural and gameplay-created rays.");
            DrawImmediateWarnings(controller);

            DrawSourceBinding(controller);
            DrawHybridRenderer(controller);
            DrawFoundationStorage(controller);
            DrawActiveAuthoredRay(controller);
            DrawProjectionDiagnostic(controller);

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                controller.RefreshNow();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

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
                    "Source Binding",
                    "Controls edit-preview updates and authoritative Sun-source resolution."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Update LightRay State in Edit Mode",
                    "Keeps authored registration, source state, intensity fades, and cloud-projection diagnostics current outside Play Mode.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunOverride",
                    "Sun Override",
                    "Optional explicit directional Sun. When unassigned, RenderSettings.sun is authoritative.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunProfile",
                    "Sun Source Profile",
                    "Optional profile defining source availability, colour multiplier, elevation fade, and maximum presentation lean.");

                WeatherLightRaySourceState sunState =
                    controller.SunSourceState;
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Sun",
                    sunState.SourceLight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Availability",
                    sunState.Available ? "Available" : "Unavailable");
                WeatherInspectorGui.ReadOnlyRow(
                    "Source Gate Weight",
                    sunState.AvailabilityWeight);
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Ray Direction",
                    sunState.RayDirectionWorld.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Elevation",
                    sunState.Elevation);
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Intensity",
                    sunState.Intensity);
                if (!sunState.Available &&
                    !string.IsNullOrEmpty(sunState.UnavailableReason))
                {
                    WeatherInspectorGui.Help(
                        "Sun unavailable: " +
                        sunState.UnavailableReason);
                }

                WeatherLightRaySourceState moonState =
                    controller.MoonSourceState;
                WeatherInspectorGui.ReadOnlyRow(
                    "Moon Source",
                    moonState.Available
                        ? "Available"
                        : "Unavailable by V1.1 design");
                if (!moonState.Available &&
                    !string.IsNullOrEmpty(moonState.UnavailableReason))
                {
                    WeatherInspectorGui.Help(
                        "Moon unavailable: " +
                        moonState.UnavailableReason);
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
                    "Final Composite shows the intended effect. Other modes isolate strand atmosphere, faint envelope haze, surface influence, ignore-cloud compensation, or scattered strands.");
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
                    "Fixed number of LightRay data slots. V1.1 renders one authored ray only; this capacity remains source-neutral for later authored, gameplay, and procedural rays.");
                WeatherInspectorGui.ReadOnlyRow(
                    "Active Slots",
                    controller.ActiveRayCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Total Capacity",
                    controller.StorageCapacity);
                WeatherInspectorGui.Help(
                    "Cloud Evolution Resume Threshold remains serialized but hidden. " +
                    "Transition suspension begins with procedural cloud-safe population, not this one-ray visual vertical slice.");
            }
        }

        private void DrawActiveAuthoredRay(
            WeatherLightRayController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActiveAuthoredRay,
                    "Active Authored Ray",
                    "Shows the one registered authored anchor and its authoritative immutable shared descriptor snapshot."))
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
                        "Add Weather LightRay Anchor to a scene marker at the desired " +
                        "ground/base centre. V1.1 accepts one active anchor.");
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
                        "Ground / Top Radius",
                        $"{snapshot.BaseEllipseAxes.x:0.###} / {snapshot.TopEllipseAxes.x:0.###} m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Height",
                        snapshot.Height,
                        "0.### m");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Strand Count",
                        snapshot.Descriptor.StrandCount);
                    WeatherInspectorGui.ReadOnlyRow(
                        "Strand / Envelope Strength",
                        $"{snapshot.Descriptor.StrandIntensity:0.###} / {snapshot.Descriptor.EnvelopeHazeIntensity:0.###}");
                    WeatherInspectorGui.ReadOnlyRow(
                        "Ground / Object Light",
                        $"{snapshot.Descriptor.GroundLightMultiplier:0.###} / {snapshot.Descriptor.VisibleSurfaceLightMultiplier:0.###}");
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
                if (GUILayout.Button("Copy LightRay V1.1A/B Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        controller.BuildComprehensiveReport();
                    Debug.Log(
                        "[Weather LightRay V1.1A/B] Report copied to clipboard.",
                        controller);
                }
            }
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
                !controller.ShowProjectionProbe ||
                Selection.activeGameObject != controller.gameObject)
            {
                return;
            }

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
