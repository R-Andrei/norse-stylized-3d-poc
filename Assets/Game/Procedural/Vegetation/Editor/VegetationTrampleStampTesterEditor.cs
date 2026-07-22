using System;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationTrampleStampTester))]
    public sealed class VegetationTrampleStampTesterEditor : UnityEditor.Editor
    {
        private SerializedProperty configurationsProperty;
        private SerializedProperty sizeVariationProperty;
        private SerializedProperty facingJitterProperty;
        private SerializedProperty strengthVariationProperty;
        private SerializedProperty recoveryVariationProperty;
        private SerializedProperty irregularityVariationProperty;
        private SerializedProperty randomizationSeedProperty;
        private SerializedProperty showSelectedShapeProperty;
        private SerializedProperty previewSegmentsProperty;

        private void OnEnable()
        {
            configurationsProperty = serializedObject.FindProperty("configurations");
            sizeVariationProperty = serializedObject.FindProperty("sizeVariation");
            facingJitterProperty = serializedObject.FindProperty("facingJitterDegrees");
            strengthVariationProperty = serializedObject.FindProperty("strengthVariation");
            recoveryVariationProperty = serializedObject.FindProperty("recoveryVariation");
            irregularityVariationProperty = serializedObject.FindProperty("irregularityVariation");
            randomizationSeedProperty = serializedObject.FindProperty("randomizationSeed");
            showSelectedShapeProperty = serializedObject.FindProperty("showSelectedShape");
            previewSegmentsProperty = serializedObject.FindProperty("previewSegments");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            var tester = (VegetationTrampleStampTester)target;

            EditorGUILayout.LabelField("Test Configurations", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(configurationsProperty, true);
            serializedObject.ApplyModifiedProperties();

            string[] configurationNames = BuildConfigurationNames(tester);
            int selectedIndex = configurationNames.Length > 0
                ? Mathf.Clamp(
                    tester.SelectedConfigurationIndex,
                    0,
                    configurationNames.Length - 1)
                : 0;
            if (configurationNames.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No test configurations exist. Restore defaults or add one to the list.",
                    MessageType.Warning);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                int requestedIndex = EditorGUILayout.Popup(
                    "Selected Configuration",
                    selectedIndex,
                    configurationNames);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(tester, "Select Trample Test Configuration");
                    tester.SelectConfiguration(requestedIndex);
                    EditorUtility.SetDirty(tester);
                    SceneView.RepaintAll();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Previous"))
                    {
                        ChangeSelection(tester, tester.SelectPreviousConfiguration);
                    }
                    if (GUILayout.Button("Next"))
                    {
                        ChangeSelection(tester, tester.SelectNextConfiguration);
                    }
                    if (GUILayout.Button("Random Selection"))
                    {
                        ChangeSelection(tester, tester.SelectRandomConfiguration);
                    }
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Randomized Variant", EditorStyles.boldLabel);
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(sizeVariationProperty);
            EditorGUILayout.PropertyField(facingJitterProperty);
            EditorGUILayout.PropertyField(strengthVariationProperty);
            EditorGUILayout.PropertyField(recoveryVariationProperty);
            EditorGUILayout.PropertyField(irregularityVariationProperty);
            EditorGUILayout.PropertyField(randomizationSeedProperty);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(showSelectedShapeProperty);
            EditorGUILayout.PropertyField(previewSegmentsProperty);
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying || tester.ConfigurationCount == 0))
            {
                if (GUILayout.Button("Stamp Selected Configuration"))
                {
                    Stamp(tester, tester.StampSelectedConfiguration);
                }
                if (GUILayout.Button("Stamp Randomized Variant"))
                {
                    Stamp(tester, tester.StampRandomizedVariant);
                }
                if (GUILayout.Button("Stamp Random Configuration"))
                {
                    Stamp(tester, tester.StampRandomConfiguration);
                }
            }

            if (GUILayout.Button("Restore Default Test Configurations"))
            {
                Undo.RecordObject(tester, "Restore Trample Test Configurations");
                tester.RestoreDefaultConfigurations();
                EditorUtility.SetDirty(tester);
                serializedObject.UpdateIfRequiredOrScript();
                SceneView.RepaintAll();
            }

            EditorGUILayout.HelpBox(
                Application.isPlaying
                    ? tester.LastStampResult
                    : "Enter Play Mode to submit stamps. The selected shape is still previewed with Gizmos in Edit Mode.",
                MessageType.Info);
        }

        private static string[] BuildConfigurationNames(
            VegetationTrampleStampTester tester)
        {
            int count = tester.ConfigurationCount;
            var names = new string[count];
            for (int index = 0; index < count; index++)
            {
                VegetationTrampleStampTestConfiguration configuration =
                    tester.Configurations[index];
                names[index] = configuration != null &&
                    !string.IsNullOrWhiteSpace(configuration.Name)
                        ? configuration.Name
                        : $"Configuration {index + 1}";
            }
            return names;
        }

        private static void ChangeSelection(
            VegetationTrampleStampTester tester,
            Action change)
        {
            Undo.RecordObject(tester, "Change Trample Test Configuration");
            change();
            EditorUtility.SetDirty(tester);
            SceneView.RepaintAll();
        }

        private static void Stamp(
            VegetationTrampleStampTester tester,
            Func<int> action)
        {
            int acceptedDomains = action();
            if (acceptedDomains > 0)
            {
                Debug.Log(
                    $"[Vegetation INTERACT.2B] {tester.LastStampResult}",
                    tester);
            }
            else
            {
                Debug.LogWarning(
                    $"[Vegetation INTERACT.2B] {tester.LastStampResult}",
                    tester);
            }
            EditorUtility.SetDirty(tester);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
    }
}
