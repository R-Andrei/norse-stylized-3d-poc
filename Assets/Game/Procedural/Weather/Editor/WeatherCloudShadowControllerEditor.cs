using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherCloudShadowController))]
    public sealed class WeatherCloudShadowControllerEditor : UnityEditor.Editor
    {
        private const double EditorTickIntervalSeconds = 1.0 / 30.0;
        private const string BenchmarkWarmupFramesKey =
            "PS3D.WeatherCloudShadowBenchmark.WarmupFrames";
        private const string BenchmarkMeasurementFramesKey =
            "PS3D.WeatherCloudShadowBenchmark.MeasurementFrames";
        private const string BenchmarkRepetitionsKey =
            "PS3D.WeatherCloudShadowBenchmark.Repetitions";
        private const string BenchmarkEvolutionWarmupFramesKey =
            "PS3D.WeatherCloudShadowBenchmark.EvolutionWarmupFrames";
        private const string BenchmarkEvolutionTimeoutKey =
            "PS3D.WeatherCloudShadowBenchmark.EvolutionTimeout";

        private double nextEditorTickTime;
        private int benchmarkWarmupFrames;
        private int benchmarkMeasurementFrames;
        private int benchmarkRepetitions;
        private int benchmarkEvolutionWarmupFrames;
        private float benchmarkEvolutionTimeoutSeconds;

        private bool showActivationSource;
        private bool showCloudPattern;
        private bool showCloudMotion;
        private bool showEvolution;
        private bool showSunGate;
        private bool showDebugVisualization;
        private bool showCookiePreview;
        private bool showActionsReports;
        private bool showBenchmark;
        private bool showRuntimeStatus;

        private void OnEnable()
        {
            benchmarkWarmupFrames = EditorPrefs.GetInt(
                BenchmarkWarmupFramesKey,
                120);
            benchmarkMeasurementFrames = EditorPrefs.GetInt(
                BenchmarkMeasurementFramesKey,
                600);
            benchmarkRepetitions = EditorPrefs.GetInt(
                BenchmarkRepetitionsKey,
                2);
            benchmarkEvolutionWarmupFrames = EditorPrefs.GetInt(
                BenchmarkEvolutionWarmupFramesKey,
                120);
            benchmarkEvolutionTimeoutSeconds = EditorPrefs.GetFloat(
                BenchmarkEvolutionTimeoutKey,
                30f);
            ClampBenchmarkSettings();
            EditorApplication.update -= OnEditorUpdate;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SaveBenchmarkSettings();
            EditorApplication.update -= OnEditorUpdate;
        }

        public override void OnInspectorGUI()
        {
            var controller = (WeatherCloudShadowController)target;
            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);
            DrawImmediateWarnings(controller);

            DrawActivationSource();
            DrawCloudPattern();
            DrawCloudMotion();
            DrawEvolution();
            DrawSunGate();
            DrawDebugVisualization(controller);

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                controller.RefreshNow();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            DrawCookiePreview(controller);
            DrawActionsReports(controller);
            DrawPerformanceBenchmark(controller);
            DrawRuntimeStatus(controller);
        }

        private static void DrawImmediateWarnings(
            WeatherCloudShadowController controller)
        {
            if (WeatherCloudShadowController.ActiveControllerCount > 1)
            {
                WeatherInspectorGui.Warning(
                    "Multiple Weather Cloud Shadow Controllers are active. The " +
                    "most recently enabled controller owns the Sun cookie.");
            }

            if (!controller.IsPublished)
            {
                WeatherInspectorGui.Warning(
                    "This controller is not the active Weather cloud-shadow publisher.");
            }

            if (!string.IsNullOrEmpty(controller.LastError))
            {
                WeatherInspectorGui.Error(controller.LastError);
            }

            if (controller.ResolvedSun == null)
            {
                WeatherInspectorGui.Error(
                    "No authoritative directional Sun is resolved. Assign Sun Override or configure RenderSettings.sun.");
            }

            if (!string.IsNullOrEmpty(controller.LastDebugError))
            {
                WeatherInspectorGui.Warning(controller.LastDebugError);
            }

            if (!string.IsNullOrEmpty(controller.LastEvolutionError))
            {
                WeatherInspectorGui.Warning(controller.LastEvolutionError);
            }
        }

        private void DrawActivationSource()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActivationSource,
                    "Activation & Sun Source",
                    "Controls cloud-shadow activation, edit preview, and authoritative Sun resolution."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudShadowsEnabled",
                    "Cloud Shadows Enabled",
                    "Installs and updates the generated directional cookie when an authoritative Sun passes the availability gate.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "previewInEditMode",
                    "Update in Edit Mode",
                    "Runs bounded cloud-cookie movement and diagnostics outside Play Mode while the component is active.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "sunOverride",
                    "Sun Override",
                    "Optional explicit directional Sun. When unassigned, RenderSettings.sun is authoritative.");

            }
        }

        private void DrawCloudPattern()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCloudPattern,
                    "Cloud Pattern",
                    "Controls the deterministic globally tiled sunlight-transmission texture."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "seed",
                    "Pattern Seed",
                    "Deterministic seed used to generate the current cloud pattern.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cookieResolution",
                    "Shadow Texture Resolution",
                    "Texel count per axis of the generated R8 directional cookie. Higher values increase dirty-time generation, upload cost, and memory quadratically.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cookieWorldSizeMetres",
                    "World Repeat Period (m)",
                    "Per-axis world-space repeat period of the directional cookie. The production field tiles globally and is not bounded to this distance.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cloudCoverage",
                    "Cloud Coverage",
                    "Target fraction of the procedural pattern classified as clouded before opening cleanup and edge softening.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "primaryFeatureScaleMetres",
                    "Large Cloud Scale (m)",
                    "Approximate world-space scale of the primary broad cloud structure.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "secondaryFeatureScaleMetres",
                    "Small Cloud Scale (m)",
                    "Approximate world-space scale of secondary cloud-shape detail.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "secondaryShapeWeight",
                    "Small-Scale Detail Influence",
                    "Blend weight of secondary detail in the generated cloud shape.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "transitionSoftnessMetres",
                    "Cloud Edge Softness (m)",
                    "Approximate world-space width of softened transmission at cloud boundaries.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumOpeningDiameterMetres",
                    "Minimum Sun Opening Diameter (m)",
                    "Dirty-time cleanup removes isolated sunlight openings whose midpoint-area is smaller than this approximate diameter.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "shadedTransmission",
                    "Clouded Sunlight Transmission",
                    "Fraction of direct Sun light retained beneath fully clouded regions. This is transmission, not cloud opacity: lower values create darker cloud shade.");

                WeatherInspectorGui.Help(
                    "The generated texture stores direct-sun transmission: 1 is open " +
                    "sunlight; Clouded Sunlight Transmission is the fully clouded floor.");
            }
        }

        private void DrawCloudMotion()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCloudMotion,
                    "Cloud Motion",
                    "Controls coherent world-phase translation driven by published Weather wind or a fallback direction."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "movementSpeedMetresPerSecond",
                    "Movement Speed (m/s)",
                    "World-space speed at which the entire directional-cookie phase translates.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "windAngleOffsetDegrees",
                    "Weather Wind Direction Offset (°)",
                    "Horizontal angular offset applied after resolving Weather wind direction.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "windSampleRateHz",
                    "Weather Wind Sampling Rate (Hz)",
                    "Bounded cadence for sampling authoritative Weather wind direction. This does not change cookie movement update frequency.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fallbackDirection",
                    "Fallback Direction (XZ)",
                    "Horizontal movement direction used when no authoritative Weather Wind Domain is published. The vector is normalized internally.");

            }
        }

        private void DrawEvolution()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showEvolution,
                    "Pattern Evolution",
                    "Controls low-cadence deterministic seed changes and bounded cookie crossfades."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cookieEvolutionEnabled",
                    "Automatic Evolution",
                    "Automatically prepares a new deterministic seed at randomized intervals in Play Mode, then crossfades the existing cookie at a bounded upload cadence.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumEvolutionIntervalSeconds",
                    "Minimum Evolution Interval (s)",
                    "Shortest randomized delay between completed automatic cloud evolutions.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumEvolutionIntervalSeconds",
                    "Maximum Evolution Interval (s)",
                    "Longest randomized delay between completed automatic cloud evolutions.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "evolutionDurationSeconds",
                    "Crossfade Duration (s)",
                    "Duration of the low-frequency blend from the current generated seed to the prepared next seed.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "evolutionUpdateRateHz",
                    "Crossfade Update Rate (Hz)",
                    "Bounded texture blend/upload cadence during an active evolution. Higher values produce more uploads per transition.");

            }
        }

        private void DrawSunGate()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showSunGate,
                    "Sun Availability Gate",
                    "Prevents cloud-cookie presentation when the directional Sun is too weak or too close to the horizon."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumSunIntensity",
                    "Minimum Sun Intensity",
                    "Minimum resolved directional-light intensity required before cloud shadows are installed.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumSunElevation",
                    "Minimum Sun Elevation (up dot)",
                    "Minimum dot product between the direction toward the Sun and world up. Values near zero correspond to the horizon.");
                WeatherInspectorGui.Help(
                    "The elevation control is a direction dot product, not degrees. " +
                    "The cookie is disabled when either intensity or elevation fails this gate.");
            }
        }

        private void DrawDebugVisualization(
            WeatherCloudShadowController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showDebugVisualization,
                    "Debug Visualization",
                    "Controls the finite diagnostic overlay that samples the exact active directional cookie."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawDebugVisualizationPopup();
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugFocusOverride",
                    "Focus Override",
                    "Optional persistent Transform used only to position the finite debug overlay. It does not limit or move the globally tiled production cloud field.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugFallbackCamera",
                    "Fallback Camera",
                    "Optional camera used when no runtime or Inspector focus override is active. Camera.main is resolved automatically when this is unassigned.");
                SerializedProperty followFocus = WeatherInspectorGui.Property(
                    serializedObject,
                    "debugFollowResolvedFocus",
                    "Follow Resolved Focus",
                    "Centers the finite diagnostic overlay on the resolved debug focus. This changes only overlay placement.");
                SerializedProperty matchPeriod = WeatherInspectorGui.Property(
                    serializedObject,
                    "debugMatchCookieWorldSize",
                    "Match Cookie Repeat Period",
                    "Makes the finite overlay span exactly one complete cookie repeat period.");

                using (new EditorGUI.DisabledScope(
                    followFocus != null && followFocus.boolValue))
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "debugOverlayAnchor",
                        "Manual Overlay Anchor",
                        "Optional overlay centre used only while Follow Resolved Focus is disabled.");
                }

                using (new EditorGUI.DisabledScope(
                    matchPeriod != null && matchPeriod.boolValue))
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "debugOverlaySizeMetres",
                        "Overlay Size (m)",
                        "Per-axis world-space span of the finite diagnostic overlay when Match Cookie Repeat Period is disabled.");
                }

                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugSampleHeightMetres",
                    "Sample Plane Y",
                    "World-space Y coordinate of the horizontal plane on which the overlay samples the cookie.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugOverlayOpacity",
                    "Overlay Opacity",
                    "Alpha multiplier of the diagnostic overlay only.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugCloudColor",
                    "Cloud Colour",
                    "Colour used to identify clouded regions in the diagnostic overlay.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugOpeningColor",
                    "Opening Colour",
                    "Colour used to identify sunlight openings in Cloud + Sun Openings mode.");

                DrawDebugFocusAuthoringWarnings(controller);
                WeatherInspectorGui.Help(
                    "The overlay is diagnostic only. It does not change generation, " +
                    "receiver shaders, or sunlight. Default colours: magenta = cloud; cyan = open sunlight.");

                if (controller.RuntimeDebugFocusOverride != null &&
                    GUILayout.Button("Clear Runtime Focus Override"))
                {
                    controller.ClearDebugFocusOverride();
                    SceneView.RepaintAll();
                }
            }
        }

        private void DrawDebugVisualizationPopup()
        {
            SerializedProperty property =
                serializedObject.FindProperty("debugVisualization");
            if (property == null)
            {
                WeatherInspectorGui.Error(
                    "Inspector property 'debugVisualization' was not found.");
                return;
            }

            string[] labels =
            {
                "Off",
                "Cloud Areas Only",
                "Cloud + Sun Openings"
            };
            int currentIndex = Mathf.Clamp(
                property.enumValueIndex,
                0,
                labels.Length - 1);
            int nextIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug View",
                    "Off hides the finite overlay. Cloud Areas Only highlights clouded regions. Cloud + Sun Openings shows both cloud and clear classifications."),
                currentIndex,
                labels);
            if (nextIndex != currentIndex)
            {
                property.enumValueIndex = nextIndex;
            }
        }

        private static void DrawDebugFocusAuthoringWarnings(
            WeatherCloudShadowController controller)
        {

            if (controller.RuntimeDebugFocusOverride != null)
            {
                WeatherInspectorGui.Warning(
                    "A runtime focus override is active and takes priority over both Inspector references.");
            }
            else if (controller.InspectorDebugFocusOverride != null &&
                     controller.DebugFallbackCamera != null)
            {
                WeatherInspectorGui.Warning(
                    "Focus Override takes priority. Fallback Camera is currently ignored.");
            }
            else if (controller.InspectorDebugFocusOverride == null &&
                     controller.DebugFallbackCamera == null)
            {
                WeatherInspectorGui.Help(
                    "Both serialized references are unassigned. Camera.main is resolved automatically; the fields remain None by design.");
            }
        }

        private static string FormatDebugVisualization(
            WeatherCloudShadowController.CloudDebugVisualization visualization)
        {
            switch (visualization)
            {
                case WeatherCloudShadowController.CloudDebugVisualization.CloudAreas:
                    return "Cloud Areas Only";
                case WeatherCloudShadowController.CloudDebugVisualization.CloudAndOpenings:
                    return "Cloud + Sun Openings";
                default:
                    return "Off";
            }
        }

        private static string FormatDebugFocusSource(
            WeatherCloudShadowController.DebugFocusSource source)
        {
            switch (source)
            {
                case WeatherCloudShadowController.DebugFocusSource.RuntimeOverride:
                    return "Runtime Override";
                case WeatherCloudShadowController.DebugFocusSource.InspectorOverride:
                    return "Inspector Override";
                case WeatherCloudShadowController.DebugFocusSource.AssignedFallbackCamera:
                    return "Assigned Fallback Camera";
                case WeatherCloudShadowController.DebugFocusSource.AutomaticMainCamera:
                    return "Automatic Camera.main";
                default:
                    return "Controller Transform Fallback";
            }
        }

        private void DrawCookiePreview(
            WeatherCloudShadowController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCookiePreview,
                    "Generated Cookie Preview",
                    "Displays the current generated R8 sunlight-transmission texture."))
            {
                return;
            }

            Texture2D cookie = controller.GeneratedCookie;
            using (new EditorGUI.IndentLevelScope())
            {
                if (cookie == null)
                {
                    WeatherInspectorGui.Help(
                        "No generated cookie is currently available.");
                    return;
                }

                Rect previewRect = GUILayoutUtility.GetAspectRect(
                    1f,
                    GUILayout.MaxHeight(180f));
                EditorGUI.DrawPreviewTexture(
                    previewRect,
                    cookie,
                    null,
                    ScaleMode.ScaleToFit);
            }
        }

        private void DrawActionsReports(
            WeatherCloudShadowController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActionsReports,
                    "Actions & Reports",
                    "Manual pattern, motion, evolution, and report actions."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                if (GUILayout.Button("Rebuild Cloud Pattern"))
                {
                    controller.RebuildCookieNow();
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Reset Cloud Motion"))
                {
                    controller.ResetCloudMotion();
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }

                using (new EditorGUI.DisabledScope(
                    controller.EvolutionInProgress))
                {
                    if (GUILayout.Button("Start Pattern Evolution Now"))
                    {
                        controller.EvolveCookieNow();
                        EditorUtility.SetDirty(controller);
                        SceneView.RepaintAll();
                    }
                }

                using (new EditorGUI.DisabledScope(
                    !controller.EvolutionInProgress))
                {
                    if (GUILayout.Button("Finish Evolution Now"))
                    {
                        controller.CompleteEvolutionImmediately();
                        EditorUtility.SetDirty(controller);
                        SceneView.RepaintAll();
                    }
                }

                if (GUILayout.Button("Copy Comprehensive Cloud Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        controller.BuildComprehensiveReport();
                    Debug.Log(
                        "[Weather Cloud Shadow V0] Report copied to clipboard.",
                        controller);
                }
            }
        }

        private void DrawPerformanceBenchmark(
            WeatherCloudShadowController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showBenchmark,
                    "Performance Benchmark",
                    "Runs the complete paired cloud-cookie and evolution benchmark suite in the current Play Mode view."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Info(
                    "Runs paired cloud-cookie-disabled/static-cookie and " +
                    "cloud-cookie-disabled/moving-cookie windows, one complete cookie evolution, and one post-evolution control. A temporary hidden runner restores captured cloud state on every exit path.");

                using (new EditorGUI.DisabledScope(
                    WeatherCloudShadowBenchmark.IsRunning))
                {
                    EditorGUI.BeginChangeCheck();
                    benchmarkWarmupFrames = EditorGUILayout.IntField(
                        new GUIContent(
                            "Warm-up Frames",
                            "Frames discarded before each persistent measurement window."),
                        benchmarkWarmupFrames);
                    benchmarkMeasurementFrames = EditorGUILayout.IntField(
                        new GUIContent(
                            "Measurement Frames",
                            "Frames retained for each persistent baseline or candidate window."),
                        benchmarkMeasurementFrames);
                    benchmarkRepetitions = EditorGUILayout.IntSlider(
                        new GUIContent(
                            "Paired Repetitions",
                            "Each repetition alternates baseline-first and candidate-first ordering."),
                        benchmarkRepetitions,
                        1,
                        5);
                    benchmarkEvolutionWarmupFrames = EditorGUILayout.IntField(
                        new GUIContent(
                            "Evolution Warm-up Frames",
                            "Steady moving-cookie frames before the forced seed transition starts."),
                        benchmarkEvolutionWarmupFrames);
                    benchmarkEvolutionTimeoutSeconds = EditorGUILayout.FloatField(
                        new GUIContent(
                            "Evolution Timeout (s)",
                            "Hard failure timeout for the complete forced evolution transition."),
                        benchmarkEvolutionTimeoutSeconds);
                    if (EditorGUI.EndChangeCheck())
                    {
                        ClampBenchmarkSettings();
                        SaveBenchmarkSettings();
                    }
                }

                bool canRun = WeatherCloudShadowBenchmark.CanBegin(
                    controller,
                    out string reason);
                using (new EditorGUI.DisabledScope(!canRun))
                {
                    if (GUILayout.Button(
                        "Run Complete Cloud-Shadow Benchmark"))
                    {
                        var settings =
                            new WeatherCloudShadowBenchmark.Settings(
                                benchmarkWarmupFrames,
                                benchmarkMeasurementFrames,
                                benchmarkRepetitions,
                                benchmarkEvolutionWarmupFrames,
                                benchmarkEvolutionTimeoutSeconds);
                        if (!WeatherCloudShadowBenchmark.Begin(
                                controller,
                                settings,
                                out string startFailure))
                        {
                            Debug.LogError(
                                "[Weather Cloud Shadow V0.3E] Benchmark did not start: " +
                                startFailure,
                                controller);
                        }
                    }
                }

                if (!canRun && !WeatherCloudShadowBenchmark.IsRunning)
                {
                    WeatherInspectorGui.Info(reason);
                }

                if (WeatherCloudShadowBenchmark.IsRunning)
                {
                    Rect progressRect = EditorGUILayout.GetControlRect(
                        false,
                        20f);
                    EditorGUI.ProgressBar(
                        progressRect,
                        WeatherCloudShadowBenchmark.Progress,
                        WeatherCloudShadowBenchmark.LastStatus);
                    EditorGUILayout.LabelField(
                        "Frame",
                        $"{WeatherCloudShadowBenchmark.CurrentFrame:N0} / " +
                        $"{WeatherCloudShadowBenchmark.CurrentFrameTarget:N0}");
                    if (GUILayout.Button(
                        "Cancel Benchmark and Restore Cloud State"))
                    {
                        WeatherCloudShadowBenchmark.CancelAndRestore();
                    }
                    WeatherInspectorGui.Warning(
                        "The benchmark is controlling cloud enablement, movement, " +
                        "automatic evolution, and debug visibility. Cancellation or Play Mode exit restores captured values.");
                }
                else
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Benchmark Status",
                        WeatherCloudShadowBenchmark.LastStatus);
                }

                using (new EditorGUI.DisabledScope(
                    WeatherCloudShadowBenchmark.IsRunning ||
                    !WeatherCloudShadowBenchmark.HasReport))
                {
                    if (GUILayout.Button(
                        "Copy Last Complete Benchmark Report"))
                    {
                        EditorGUIUtility.systemCopyBuffer =
                            WeatherCloudShadowBenchmark.LastReport;
                    }
                }

                if (!string.IsNullOrEmpty(
                        WeatherCloudShadowBenchmark.LastReportPath))
                {
                    WeatherInspectorGui.ReadOnlyRow(
                        "Saved Report",
                        WeatherCloudShadowBenchmark.LastReportPath);
                }
            }
        }

        private void ClampBenchmarkSettings()
        {
            benchmarkWarmupFrames = Mathf.Clamp(
                benchmarkWarmupFrames,
                30,
                3600);
            benchmarkMeasurementFrames = Mathf.Clamp(
                benchmarkMeasurementFrames,
                120,
                7200);
            benchmarkRepetitions = Mathf.Clamp(
                benchmarkRepetitions,
                1,
                5);
            benchmarkEvolutionWarmupFrames = Mathf.Clamp(
                benchmarkEvolutionWarmupFrames,
                30,
                1800);
            benchmarkEvolutionTimeoutSeconds = Mathf.Clamp(
                benchmarkEvolutionTimeoutSeconds,
                5f,
                120f);
        }

        private void SaveBenchmarkSettings()
        {
            ClampBenchmarkSettings();
            EditorPrefs.SetInt(
                BenchmarkWarmupFramesKey,
                benchmarkWarmupFrames);
            EditorPrefs.SetInt(
                BenchmarkMeasurementFramesKey,
                benchmarkMeasurementFrames);
            EditorPrefs.SetInt(
                BenchmarkRepetitionsKey,
                benchmarkRepetitions);
            EditorPrefs.SetInt(
                BenchmarkEvolutionWarmupFramesKey,
                benchmarkEvolutionWarmupFrames);
            EditorPrefs.SetFloat(
                BenchmarkEvolutionTimeoutKey,
                benchmarkEvolutionTimeoutSeconds);
        }

        private void DrawRuntimeStatus(
            WeatherCloudShadowController controller)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showRuntimeStatus,
                    "Runtime Status",
                    "Read-only publisher, cookie, evolution, focus, motion, and Sun-gate state."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyRow(
                    "Published",
                    controller.IsPublished ? "Yes" : "No");
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Sun",
                    controller.ResolvedSun);
                WeatherInspectorGui.ReadOnlyRow(
                    "Cookie",
                    controller.CookieReady ? "Ready" : "Not ready");
                WeatherInspectorGui.ReadOnlyRow(
                    "Resolution",
                    $"{controller.CookieResolution} × {controller.CookieResolution}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Estimated Texture Memory",
                    $"{controller.EstimatedCookieTexelBytes:N0} bytes");
                WeatherInspectorGui.ReadOnlyRow(
                    "Repeat Period",
                    $"{controller.CookieWorldSizeMetres:0.###} m per axis");
                WeatherInspectorGui.ReadOnlyRow(
                    "Evolution",
                    $"{controller.EvolutionState}; seed {controller.CurrentCookieSeed}; {controller.EvolutionProgress:P1}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Next Evolution Seed",
                    controller.EvolutionInProgress
                        ? controller.NextEvolutionSeed.ToString()
                        : "None");
                double secondsUntilNext = controller.SecondsUntilNextEvolution;
                WeatherInspectorGui.ReadOnlyRow(
                    "Next Automatic Evolution",
                    double.IsPositiveInfinity(secondsUntilNext)
                        ? "Inactive"
                        : $"{secondsUntilNext:0.0} s");
                WeatherInspectorGui.ReadOnlyRow(
                    "Evolution Uploads / Bytes",
                    $"{controller.EvolutionUploadCount} / {controller.EvolutionUploadedTexelBytes:N0}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Evolution Preparation",
                    $"{controller.LastEvolutionPreparationMilliseconds:0.###} ms");
                WeatherInspectorGui.ReadOnlyRow(
                    "Evolution Blend Total / Max",
                    $"{controller.EvolutionBlendUploadTotalMilliseconds:0.###} / {controller.EvolutionBlendUploadMaximumMilliseconds:0.###} ms");
                WeatherInspectorGui.ReadOnlyObject(
                    "Debug Focus",
                    controller.ResolvedDebugFocus);
                WeatherInspectorGui.ReadOnlyRow(
                    "Focus Source",
                    FormatDebugFocusSource(controller.ResolvedDebugFocusSource));
                WeatherInspectorGui.ReadOnlyRow(
                    "Focus Position",
                    controller.ResolvedDebugFocusPosition.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Effective Overlay Size",
                    $"{controller.EffectiveDebugOverlaySizeMetres:0.###} m");
                WeatherInspectorGui.ReadOnlyObject(
                    "Published Wind Domain",
                    WeatherWindDomain.PublishedDomain);
                WeatherInspectorGui.ReadOnlyRow(
                    "Wind Direction XZ",
                    controller.ResolvedWindDirection.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Cookie Offset",
                    controller.CurrentCookieOffset.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Debug View",
                    FormatDebugVisualization(controller.DebugVisualization));
                WeatherInspectorGui.ReadOnlyRow(
                    "Sun Gate",
                    controller.SunGateActive ? "Active" : "Inactive");
            }
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying)
            {
                if (WeatherCloudShadowBenchmark.IsRunning &&
                    EditorApplication.timeSinceStartup >=
                    nextEditorTickTime)
                {
                    nextEditorTickTime =
                        EditorApplication.timeSinceStartup + 0.25;
                    Repaint();
                }
                return;
            }

            var controller = target as WeatherCloudShadowController;
            if (controller == null ||
                !controller.isActiveAndEnabled ||
                !controller.PreviewInEditMode)
            {
                return;
            }

            double now = EditorApplication.timeSinceStartup;
            if (now < nextEditorTickTime)
            {
                return;
            }

            nextEditorTickTime = now + EditorTickIntervalSeconds;
            controller.EditorTick();
            Repaint();
            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }
    }
}
