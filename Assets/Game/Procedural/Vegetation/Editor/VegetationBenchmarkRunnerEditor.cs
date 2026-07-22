using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationBenchmarkRunner))]
    public sealed class VegetationBenchmarkRunnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var runner = (VegetationBenchmarkRunner)target;
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(runner.SuiteRunning))
            {
                DrawPropertiesExcluding(serializedObject, "m_Script");
            }
            serializedObject.ApplyModifiedProperties();
            if (VegetationBenchmarkRunner.ActiveRunnerCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple active VegetationBenchmarkRunner components exist. " +
                    "Exactly one scene-level runner is required.",
                    MessageType.Error);
            }

            if (runner.BenchmarkMode ==
                VegetationBenchmarkRunnerMode.EnabledStack)
            {
                EditorGUILayout.HelpBox(
                    "Enabled Stack measures exactly the active, enabled, rendering " +
                    "VegetationLayer stack in scope. It toggles renderer flags for " +
                    "baseline windows and performs no layer rebuilds.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Controlled Layer Matrix first measures the exact current authored " +
                    "stack as one case, then isolates the selected active CrossedCards " +
                    "layer at 20, 35, and 50 clusters/m². If the selected layer's exact " +
                    "density is outside those tiers, it is measured once as an additional " +
                    "controlled case. All sibling flags and the exact selected recipe " +
                    "density are restored.",
                    runner.ControlledLayer != null
                        ? MessageType.Info
                        : MessageType.Warning);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inventory", EditorStyles.boldLabel);
            if (GUILayout.Button("Log Layer Inventory Report"))
            {
                Debug.Log(runner.BuildLayerInventoryReport(), runner);
            }
            if (GUILayout.Button("Copy Layer Inventory Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    runner.BuildLayerInventoryReport();
                Debug.Log(
                    "[Vegetation FOUNDATION.1] Scene layer inventory copied to clipboard.",
                    runner);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Timed Benchmark", EditorStyles.boldLabel);
            bool canRun = runner.CanRunTimedSuite(out string reason);
            using (new EditorGUI.DisabledScope(!canRun))
            {
                if (GUILayout.Button("Run Complete Stack-Aware Benchmark"))
                {
                    if (runner.BeginTimedSuite())
                    {
                        Debug.Log(
                            "[Vegetation FOUNDATION.1] Stack-aware benchmark started. " +
                            "Do not change layer hierarchy or recipe settings until it completes.",
                            runner);
                    }
                }
            }

            if (!canRun && !runner.SuiteRunning)
            {
                EditorGUILayout.HelpBox(reason, MessageType.Info);
            }

            using (new EditorGUI.DisabledScope(
                       runner.SuiteRunning || !runner.HasTimedSuiteReport))
            {
                if (GUILayout.Button("Copy Last Timed Benchmark Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        runner.LastTimedSuiteReport;
                    Debug.Log(
                        "[Vegetation FOUNDATION.1] Last timed benchmark report copied to clipboard.",
                        runner);
                }
            }

            if (runner.SuiteRunning)
            {
                float progress = runner.SuiteTotalCases > 0
                    ? (float)(runner.SuiteCurrentCase - 1) /
                      runner.SuiteTotalCases
                    : 0f;
                Rect progressRect = EditorGUILayout.GetControlRect(false, 20f);
                EditorGUI.ProgressBar(
                    progressRect,
                    Mathf.Clamp01(progress),
                    runner.SuiteStatus);
                EditorGUILayout.HelpBox(
                    "Disabling or destroying this runner interrupts the suite and " +
                    "restores captured layer state.",
                    MessageType.Warning);
                Repaint();
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Timed suite status",
                    runner.SuiteStatus);
                if (!string.IsNullOrEmpty(runner.LastTimedSuiteReportPath))
                {
                    EditorGUILayout.SelectableLabel(
                        runner.LastTimedSuiteReportPath,
                        EditorStyles.textField,
                        GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }
            }
        }
    }
}
