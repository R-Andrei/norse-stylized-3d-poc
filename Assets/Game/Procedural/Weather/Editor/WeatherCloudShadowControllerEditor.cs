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
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawSerializedProperties(controller);
            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                controller.RefreshNow();
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Weather Cloud-Shadow Actions",
                EditorStyles.boldLabel);

            if (GUILayout.Button("Rebuild Cloud Cookie"))
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
                if (GUILayout.Button("Evolve Cloud Cookie Now"))
                {
                    controller.EvolveCookieNow();
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }
            }

            using (new EditorGUI.DisabledScope(
                !controller.EvolutionInProgress))
            {
                if (GUILayout.Button("Complete Evolution Immediately"))
                {
                    controller.CompleteEvolutionImmediately();
                    EditorUtility.SetDirty(controller);
                    SceneView.RepaintAll();
                }
            }

            if (GUILayout.Button("Refresh Debug Focus"))
            {
                controller.RefreshDebugFocusNow();
                SceneView.RepaintAll();
            }

            if (controller.RuntimeDebugFocusOverride != null &&
                GUILayout.Button("Clear Runtime Debug Focus Override"))
            {
                controller.ClearDebugFocusOverride();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Cloud Area Debug",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "The world overlay samples the exact active directional cookie. " +
                "It is diagnostic only and does not change cloud generation, " +
                "receiver shaders, or sunlight strength. The directional cookie " +
                "tiles globally; Debug Overlay Focus controls only where this finite " +
                "debug overlay is displayed. By default, magenta marks cloud and " +
                "cyan marks open sunlight.",
                MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Cloud Areas"))
            {
                controller.SetDebugVisualization(
                    WeatherCloudShadowController.CloudDebugVisualization.CloudAreas);
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Show Cloud / Opening Map"))
            {
                controller.SetDebugVisualization(
                    WeatherCloudShadowController.CloudDebugVisualization.CloudAndOpenings);
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Hide Cloud Debug Overlay"))
            {
                controller.SetDebugVisualization(
                    WeatherCloudShadowController.CloudDebugVisualization.Off);
                EditorUtility.SetDirty(controller);
                SceneView.RepaintAll();
            }

            DrawCookiePreview(controller);

            if (GUILayout.Button("Copy Cloud-Shadow Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    controller.BuildComprehensiveReport();
                Debug.Log(
                    "[Weather Cloud Shadow V0] Report copied to clipboard.",
                    controller);
            }

            DrawPerformanceBenchmark(controller);

            EditorGUILayout.Space();
            DrawStatus(controller);
        }

        private void DrawPerformanceBenchmark(
            WeatherCloudShadowController controller)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Cloud-Shadow Performance Benchmark",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs paired cloud-cookie-disabled/static-cookie and cloud-cookie-disabled/moving-cookie " +
                "windows, one complete cookie evolution, and one post-evolution " +
                "control in the current Play Mode view. The suite creates only a " +
                "temporary hidden runtime runner, saves one report automatically, " +
                "and restores the captured cloud state on every exit path.",
                MessageType.Info);

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
                        "Evolution Timeout Seconds",
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
                EditorGUILayout.HelpBox(reason, MessageType.Info);
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
                EditorGUILayout.HelpBox(
                    "The benchmark is controlling cloud enablement, movement, " +
                    "automatic evolution, and debug visibility. Cancellation or " +
                    "Play Mode exit restores the captured values.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Benchmark status",
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
                EditorGUILayout.LabelField(
                    "Saved report",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.SelectableLabel(
                    WeatherCloudShadowBenchmark.LastReportPath,
                    EditorStyles.textField,
                    GUILayout.Height(
                        EditorGUIUtility.singleLineHeight));
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

        private void DrawSerializedProperties(
            WeatherCloudShadowController controller)
        {
            SerializedProperty property = serializedObject.GetIterator();
            bool enterChildren = true;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyPath == "m_Script")
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property, true);
                if (property.propertyPath == "evolutionUpdateRateHz")
                {
                    DrawEvolutionStatus(controller);
                }
                else if (property.propertyPath == "debugFallbackCamera")
                {
                    DrawResolvedDebugFocus(controller);
                }
            }
        }

        private static void DrawEvolutionStatus(
            WeatherCloudShadowController controller)
        {
            double secondsUntilNext = controller.SecondsUntilNextEvolution;
            string nextEvolution =
                double.IsPositiveInfinity(secondsUntilNext)
                    ? "Inactive"
                    : $"{secondsUntilNext:0.0} s";
            string nextSeed = controller.EvolutionInProgress
                ? controller.NextEvolutionSeed.ToString()
                : "None";

            EditorGUILayout.Space(2f);
            EditorGUILayout.HelpBox(
                $"State: {controller.EvolutionState}\n" +
                $"Current / Next Seed: {controller.CurrentCookieSeed} / {nextSeed}\n" +
                $"Progress: {controller.EvolutionProgress:P1}\n" +
                $"Next Automatic Evolution: {nextEvolution}\n" +
                $"Uploads This Transition: {controller.EvolutionUploadCount} " +
                $"({controller.EvolutionUploadedTexelBytes:N0} raw texel bytes)\n" +
                $"Estimated Configured Transition Upload: " +
                $"{controller.EstimatedEvolutionUploadBytesPerTransition:N0} raw texel bytes",
                MessageType.Info);

            if (!string.IsNullOrEmpty(controller.LastEvolutionError))
            {
                EditorGUILayout.HelpBox(
                    controller.LastEvolutionError,
                    MessageType.Warning);
            }
        }

        private static void DrawResolvedDebugFocus(
            WeatherCloudShadowController controller)
        {
            Transform resolvedFocus = controller.ResolvedDebugFocus;
            string focusName = resolvedFocus != null
                ? resolvedFocus.name
                : "None";
            string source = FormatDebugFocusSource(
                controller.ResolvedDebugFocusSource);
            string position =
                controller.ResolvedDebugFocusPosition.ToString("F3");

            EditorGUILayout.Space(2f);
            EditorGUILayout.HelpBox(
                $"Resolved Debug Focus: {focusName}\n" +
                $"Source: {source}\n" +
                $"World Position: {position}",
                MessageType.Info);

            if (controller.RuntimeDebugFocusOverride != null)
            {
                EditorGUILayout.HelpBox(
                    "A runtime Debug Focus Override is active and takes " +
                    "priority over both Inspector fields.",
                    MessageType.Warning);
                return;
            }

            if (controller.InspectorDebugFocusOverride != null &&
                controller.DebugFallbackCamera != null)
            {
                EditorGUILayout.HelpBox(
                    "Debug Focus Override takes priority. Debug Fallback " +
                    "Camera is currently ignored.",
                    MessageType.Warning);
                return;
            }

            if (controller.InspectorDebugFocusOverride == null &&
                controller.DebugFallbackCamera == null)
            {
                EditorGUILayout.HelpBox(
                    "Both serialized debug-focus fields are unassigned. " +
                    "Camera.main is resolved automatically; the fields " +
                    "remain None by design.",
                    MessageType.None);
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

        private static void DrawStatus(
            WeatherCloudShadowController controller)
        {
            if (WeatherCloudShadowController.ActiveControllerCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple Weather Cloud Shadow Controllers are active. " +
                    "The most recently enabled controller owns the sun cookie.",
                    MessageType.Warning);
            }

            if (!controller.IsPublished)
            {
                EditorGUILayout.HelpBox(
                    "This controller is not the active Weather cloud-shadow publisher.",
                    MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(controller.LastError))
            {
                EditorGUILayout.HelpBox(
                    controller.LastError,
                    MessageType.Error);
                return;
            }

            Light sun = controller.ResolvedSun;
            if (sun == null)
            {
                EditorGUILayout.HelpBox(
                    "No authoritative directional sun is resolved.",
                    MessageType.Error);
                return;
            }

            string windDirection =
                controller.ResolvedWindDirection.ToString("F3");
            string cookieOffset =
                controller.CurrentCookieOffset.ToString("F3");
            string debugState = controller.DebugVisualization.ToString();
            Transform debugFocus = controller.ResolvedDebugFocus;
            string debugFocusName = debugFocus != null
                ? debugFocus.name
                : "None";
            string debugFocusPosition =
                controller.ResolvedDebugFocusPosition.ToString("F3");
            EditorGUILayout.HelpBox(
                $"Sun: {sun.name}\n" +
                $"Cookie: {(controller.CookieReady ? "Ready" : "Not ready")} " +
                $"({controller.CookieResolution} × {controller.CookieResolution}, " +
                $"approximately {controller.EstimatedCookieTexelBytes:N0} R8 texel bytes)\n" +
                $"Cookie repeat period: {controller.CookieWorldSizeMetres:0.###} m per axis (globally tiled)\n" +
                $"Cookie evolution: {controller.EvolutionState}; " +
                $"seed {controller.CurrentCookieSeed}; " +
                $"progress {controller.EvolutionProgress:P1}\n" +
                $"Debug focus: {debugFocusName} " +
                $"({FormatDebugFocusSource(controller.ResolvedDebugFocusSource)}) at {debugFocusPosition}\n" +
                $"Wind direction XZ: {windDirection}\n" +
                $"Cookie offset: {cookieOffset}\n" +
                $"Debug visualization: {debugState} " +
                $"({controller.EffectiveDebugOverlaySizeMetres:0.###} m at Y " +
                $"{controller.DebugSampleHeightMetres:0.###}; " +
                $"follows resolved focus: {(controller.DebugFollowsResolvedFocus ? "Yes" : "No")})\n" +
                $"Sun gate: {(controller.SunGateActive ? "Active" : "Inactive")}",
                MessageType.Info);

            if (!string.IsNullOrEmpty(controller.LastDebugError))
            {
                EditorGUILayout.HelpBox(
                    controller.LastDebugError,
                    MessageType.Warning);
            }

            if (!string.IsNullOrEmpty(controller.LastEvolutionError))
            {
                EditorGUILayout.HelpBox(
                    controller.LastEvolutionError,
                    MessageType.Warning);
            }
        }

        private static void DrawCookiePreview(
            WeatherCloudShadowController controller)
        {
            Texture2D cookie = controller.GeneratedCookie;
            if (cookie == null)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Generated Cookie Preview",
                EditorStyles.boldLabel);
            Rect previewRect = GUILayoutUtility.GetAspectRect(
                1f,
                GUILayout.MaxHeight(180f));
            EditorGUI.DrawPreviewTexture(
                previewRect,
                cookie,
                null,
                ScaleMode.ScaleToFit);
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
