using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiverFoamSimulation))]
    [CanEditMultipleObjects]
    internal sealed class StylizedRiverFoamSimulationEditor : UnityEditor.Editor
    {
        private bool showAdvanced;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty style = serializedObject.FindProperty("style");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(style, new GUIContent("Foam Style"));
            bool styleChanged = EditorGUI.EndChangeCheck();

            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("foamAmount"),
                new GUIContent("Foam Amount"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("patternScale"),
                new GUIContent("Patch Scale", "Approximate size of connected foam structures in metres."));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("flowSpeed"),
                new GUIContent("Flow Speed"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("breakup"),
                new GUIContent("Breakup", "Controls lateral bending and how readily persistent patches split and reconnect."));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("foamColor"),
                new GUIContent("Foam Colour"));

            showAdvanced = EditorGUILayout.Foldout(
                showAdvanced,
                "Advanced Simulation",
                true);


            if (showAdvanced)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("quality"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("updatesPerSecond"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("warmupSteps"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("simulateInSceneView"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("resetWhenEnabled"));

                if ((StylizedRiverFoamStyle)style.enumValueIndex == StylizedRiverFoamStyle.Custom)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField("Custom Reaction Field", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customFeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customKill"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customCohesion"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customBankSource"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customAmbientSource"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customLifetime"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customContactStrength"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customContactDepth"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customThreshold"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("customSoftness"));
                }

                EditorGUI.indentLevel--;
            }

            bool changed = serializedObject.ApplyModifiedProperties();

            if (styleChanged)
            {
                ApplyToTargets("Apply River Foam Style", simulation => simulation.ApplyStyleDefaults());
            }
            else if (changed)
            {
                ApplyToTargets("Update Stateful River Foam", simulation => simulation.RefreshBinding());
            }

            DrawStatus();

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Apply Style Defaults"))
            {
                ApplyToTargets("Apply River Foam Style", simulation => simulation.ApplyStyleDefaults());
            }

            if (GUILayout.Button("Reset Stateful Foam"))
            {
                ApplyToTargets("Reset Stateful River Foam", simulation => simulation.ResetSimulation());
            }
        }

        private void DrawStatus()
        {
            if (targets.Length != 1)
            {
                return;
            }

            StylizedRiverFoamSimulation simulation =
                target as StylizedRiverFoamSimulation;

            if (simulation == null)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("State", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Texture",
                simulation.HasStateTexture
                    ? $"{simulation.StateTextureSize.x} × {simulation.StateTextureSize.y}"
                    : "Waiting for simulation");

            if (!SystemInfo.supportsComputeShaders)
            {
                EditorGUILayout.HelpBox(
                    "Compute shaders are not supported on the current graphics device.",
                    MessageType.Error);
            }
            else if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "A frozen preview is prepared in Edit Mode. The persistent field evolves continuously in Play Mode. Enable Simulate In Scene View only when continuous editor GPU work is intentional.",
                    MessageType.Info);
            }
        }

        private void ApplyToTargets(string undoName, SimulationAction action)
        {
            foreach (Object selectedTarget in targets)
            {
                StylizedRiverFoamSimulation simulation =
                    selectedTarget as StylizedRiverFoamSimulation;

                if (simulation == null)
                {
                    continue;
                }

                Undo.RecordObject(simulation, undoName);
                action(simulation);
                EditorUtility.SetDirty(simulation);
            }

            serializedObject.Update();
            Repaint();
            SceneView.RepaintAll();
        }

        private delegate void SimulationAction(
            StylizedRiverFoamSimulation simulation);
    }
}
