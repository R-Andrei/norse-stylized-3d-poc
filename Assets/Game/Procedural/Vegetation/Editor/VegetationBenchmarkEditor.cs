using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using ProgrammaticStylized3D.Weather;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationBenchmark))]
    public sealed class VegetationBenchmarkEditor : UnityEditor.Editor
    {
        private static readonly string[] RenderingOptions =
        {
            "Enabled",
            "Disabled"
        };

        public override void OnInspectorGUI()
        {
            var benchmark = (VegetationBenchmark)target;
            int rebuildHashBefore = benchmark.ComputeRebuildConfigurationHash();
            int lightingHashBefore = benchmark.ComputeLightingConfigurationHash();
            bool inspectorChanged = DrawDefaultInspector();

            if (inspectorChanged && !benchmark.SuiteRunning)
            {
                int rebuildHashAfter = benchmark.ComputeRebuildConfigurationHash();
                int lightingHashAfter = benchmark.ComputeLightingConfigurationHash();

                if (rebuildHashAfter != rebuildHashBefore)
                {
                    benchmark.RebuildBenchmark();
                }
                else if (lightingHashAfter != lightingHashBefore)
                {
                    benchmark.RefreshLightingMaterialProperties();
                }

                EditorUtility.SetDirty(benchmark);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            int currentRenderingOption = benchmark.RenderBenchmarkEnabled ? 0 : 1;
            EditorGUI.BeginChangeCheck();
            int selectedRenderingOption = EditorGUILayout.Popup(
                "Rendering",
                currentRenderingOption,
                RenderingOptions);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(benchmark, "Change Vegetation Benchmark Rendering");
                benchmark.SetRenderBenchmark(selectedRenderingOption == 0);
                EditorUtility.SetDirty(benchmark);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Benchmark Actions", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Geometry, placement, coverage, silhouette, and Grass Macro Patch Scale/Seed/Transition/Separation changes rebuild the benchmark. " +
                "Grass Dark/Light Patch Strength and Stylized Lighting controls update only the runtime material and do not rebuild instances.",
                MessageType.Info);

            if (GUILayout.Button("Rebuild Vegetation Benchmark"))
            {
                Undo.RecordObject(benchmark, "Rebuild Vegetation Benchmark");
                benchmark.RebuildBenchmark();
                EditorUtility.SetDirty(benchmark);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Copy Comprehensive Vegetation Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    benchmark.BuildComprehensiveReport();
                Debug.Log(
                    "[Vegetation V1F] Comprehensive benchmark report copied to clipboard.",
                    benchmark);
            }

            if (GUILayout.Button("Run and Copy Structural Configuration Matrix"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    benchmark.BuildAllConfigurationComparisonsReport();
                Debug.Log(
                    "[Vegetation V1F] Structural geometry × density matrix copied " +
                    "to the clipboard.",
                    benchmark);
                SceneView.RepaintAll();
            }

            using (new EditorGUI.DisabledScope(
                       benchmark.SuiteRunning || !Application.isPlaying))
            {
                if (GUILayout.Button("Run Complete Timed Comparison Suite"))
                {
                    if (benchmark.BeginTimedComparisonSuite())
                    {
                        Debug.Log(
                            "[Vegetation V1F] Automated timed suite started. " +
                            "All geometry × silhouette profile × density cases will run without manual input.",
                            benchmark);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(
                       benchmark.SuiteRunning || !benchmark.HasTimedSuiteReport))
            {
                if (GUILayout.Button("Copy Last Timed Suite Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        benchmark.LastTimedSuiteReport;
                    Debug.Log(
                        "[Vegetation V1F] Last automated timed suite report copied " +
                        "to the clipboard.",
                        benchmark);
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode, then press Run Complete Timed Comparison Suite once. " +
                    "It automatically runs every geometry, three canonical silhouette profiles, and 35/50 density stress cases under forced-full coverage, interleaves " +
                    "render-disabled baselines when enabled, performs every configured " +
                    "pass, requests screenshots when enabled, restores the original " +
                    "configuration, and retains one consolidated report.",
                    MessageType.Info);
            }

            if (benchmark.SuiteRunning)
            {
                float progress = benchmark.SuiteTotalCases > 0
                    ? (float)(benchmark.SuiteCurrentCase - 1) /
                      benchmark.SuiteTotalCases
                    : 0f;
                Rect progressRect = EditorGUILayout.GetControlRect(false, 20f);
                EditorGUI.ProgressBar(
                    progressRect,
                    Mathf.Clamp01(progress),
                    benchmark.SuiteStatus);
                Repaint();
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Timed suite status",
                    benchmark.SuiteStatus);
            }

            EditorGUILayout.Space();
            DrawStatus(benchmark);
        }


        private static void DrawStatus(VegetationBenchmark benchmark)
        {
            MessageType statusType = benchmark.ResourcesReady
                ? MessageType.Info
                : MessageType.Warning;
            string status = benchmark.ResourcesReady
                ? $"Ready: {benchmark.InstanceCount:N0} instances, " +
                  $"{benchmark.ClusterTriangleCount:N0} triangles per cluster."
                : "Benchmark resources are not ready.";
            EditorGUILayout.HelpBox(status, statusType);

            if (!string.IsNullOrEmpty(benchmark.LastBuildError))
            {
                EditorGUILayout.HelpBox(
                    benchmark.LastBuildError,
                    MessageType.Error);
            }

            if (benchmark.UseGroundCoverage)
            {
                if (benchmark.CoverageGround == null)
                {
                    EditorGUILayout.HelpBox(
                        "Ground coverage is enabled but no GeneratedGround is assigned. " +
                        "The benchmark falls back to the full flat field.",
                        MessageType.Warning);
                }
                else if (!benchmark.CoverageGround.VegetationCoverageInitialized)
                {
                    EditorGUILayout.HelpBox(
                        $"Ground coverage source: {benchmark.CoverageGround.name}. " +
                        "Its mask is not initialized, so placement currently uses full " +
                        "coverage instead of rejecting all grass. Initialize Empty, " +
                        "Initialize Full, or paint the Ground to author coverage.",
                        MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"Ground coverage source: {benchmark.CoverageGround.name}. " +
                        $"Average authored coverage: " +
                        $"{benchmark.CoverageGround.CalculateVegetationCoverageFraction() * 100f:0.0}%.",
                        MessageType.Info);
                }
            }

            if (benchmark.SceneViewPreviewEnabled)
            {
                EditorGUILayout.HelpBox(
                    benchmark.SuiteRunning
                        ? "Scene View Preview is temporarily suppressed while the timed suite owns the benchmark."
                        : "Scene View Preview is enabled and renders only through SceneView cameras; Preview and Reflection cameras are excluded.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Scene View Preview is disabled. Game-camera rendering is unchanged.",
                    MessageType.Info);
            }

            int domainCount = WeatherWindDomain.ActiveDomainCount;
            WeatherWindDomain publishedDomain = WeatherWindDomain.PublishedDomain;
            if (domainCount == 0 || publishedDomain == null)
            {
                EditorGUILayout.HelpBox(
                    "No shared Weather XZ Wind Domain is active. Grass remains static by design.",
                    MessageType.Warning);
            }
            else if (domainCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple Weather XZ Wind Domains are active. The most recently enabled domain publishes the global field.",
                    MessageType.Warning);
            }
            else if (!publishedDomain.ResourcesReady)
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(publishedDomain.LastError)
                        ? "The Weather XZ Wind Domain is active but its resources are not ready."
                        : publishedDomain.LastError,
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"Shared Weather XZ field active: {publishedDomain.FieldResolution} × " +
                    $"{publishedDomain.FieldResolution}, {publishedDomain.CellSizeMetres:0.###} m/cell, " +
                    $"{publishedDomain.FieldWorldSizeMetres:0.###} m coverage.",
                    MessageType.Info);
            }
        }
    }
    [InitializeOnLoad]
    internal static class VegetationBenchmarkEditorPreview
    {
        static VegetationBenchmarkEditorPreview()
        {
            RenderPipelineManager.beginCameraRendering -=
                HandleBeginCameraRendering;
            RenderPipelineManager.beginCameraRendering +=
                HandleBeginCameraRendering;
        }

        private static void HandleBeginCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (Application.isPlaying || camera == null ||
                (camera.cameraType != CameraType.SceneView &&
                 camera.cameraType != CameraType.Game))
            {
                return;
            }

            var benchmarks = VegetationBenchmark.ActiveBenchmarks;
            for (int index = 0; index < benchmarks.Count; index++)
            {
                VegetationBenchmark benchmark = benchmarks[index];
                if (benchmark == null || !benchmark.isActiveAndEnabled ||
                    benchmark.SuiteRunning)
                {
                    continue;
                }

                benchmark.RenderEditorPreview(camera);
            }
        }
    }

}
