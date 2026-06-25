using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiverDisturbanceEmitter))]
    [CanEditMultipleObjects]
    public sealed class StylizedRiverDisturbanceEmitterEditor :
        UnityEditor.Editor
    {
        private SerializedProperty explicitRiver;
        private SerializedProperty autoDetectRiver;
        private SerializedProperty verticalContactTolerance;
        private SerializedProperty sourceMobility;
        private SerializedProperty useSeparateFootprintDimensions;
        private SerializedProperty linkedFootprintRadius;
        private SerializedProperty acrossFlowHalfWidth;
        private SerializedProperty alongFlowHalfLength;
        private SerializedProperty strength;
        private SerializedProperty geometryContribution;
        private SerializedProperty normalContribution;
        private SerializedProperty stationaryObstruction;
        private SerializedProperty emitEntryImpact;
        private SerializedProperty entryImpact;
        private SerializedProperty emitExitImpact;
        private SerializedProperty exitImpact;
        private SerializedProperty sourceUpdateInterval;

        private void OnEnable()
        {
            explicitRiver = serializedObject.FindProperty("explicitRiver");
            autoDetectRiver = serializedObject.FindProperty("autoDetectRiver");
            verticalContactTolerance = serializedObject.FindProperty(
                "verticalContactTolerance");
            sourceMobility = serializedObject.FindProperty("sourceMobility");
            useSeparateFootprintDimensions = serializedObject.FindProperty(
                "useSeparateFootprintDimensions");
            linkedFootprintRadius = serializedObject.FindProperty(
                "linkedFootprintRadius");
            acrossFlowHalfWidth = serializedObject.FindProperty(
                "acrossFlowHalfWidth");
            alongFlowHalfLength = serializedObject.FindProperty(
                "alongFlowHalfLength");
            strength = serializedObject.FindProperty("strength");
            geometryContribution = serializedObject.FindProperty(
                "geometryContribution");
            normalContribution = serializedObject.FindProperty(
                "normalContribution");
            stationaryObstruction = serializedObject.FindProperty(
                "stationaryObstruction");
            emitEntryImpact = serializedObject.FindProperty(
                "emitEntryImpact");
            entryImpact = serializedObject.FindProperty(
                "entryImpact");
            emitExitImpact = serializedObject.FindProperty(
                "emitExitImpact");
            exitImpact = serializedObject.FindProperty(
                "exitImpact");
            sourceUpdateInterval = serializedObject.FindProperty(
                "sourceUpdateInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            bool legacyStaticEmitter =
                sourceMobility != null &&
                !sourceMobility.hasMultipleDifferentValues &&
                sourceMobility.enumValueIndex == 0;

            if (legacyStaticEmitter)
            {
                EditorGUILayout.HelpBox(
                    "This is a legacy Static River Disturbance Emitter. It is " +
                    "inert because stationary geometry is now handled through " +
                    "the generated-geometry registry. Remove this component.",
                    MessageType.Warning);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.LabelField("River Contact", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(explicitRiver);
            EditorGUILayout.PropertyField(autoDetectRiver);
            EditorGUILayout.PropertyField(verticalContactTolerance);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Source Footprint", EditorStyles.boldLabel);
            DrawManualFootprint();

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Continuous Wake",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(strength);
            EditorGUILayout.PropertyField(geometryContribution);
            EditorGUILayout.PropertyField(normalContribution);
            EditorGUILayout.PropertyField(stationaryObstruction);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Impact Ripples",
                EditorStyles.boldLabel);
            DrawImpactEvent(
                emitEntryImpact,
                entryImpact,
                "Entry Impact");
            DrawImpactEvent(
                emitExitImpact,
                exitImpact,
                "Exit / Suction Impact");

            using (new EditorGUI.DisabledScope(
                !Application.isPlaying ||
                targets.Length != 1))
            {
                if (GUILayout.Button("Emit Entry Impact Now") &&
                    target is StylizedRiverDisturbanceEmitter emitter)
                {
                    serializedObject.ApplyModifiedProperties();
                    emitter.EmitImpactNow();
                    serializedObject.Update();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Runtime Sampling",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceUpdateInterval);

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawImpactEvent(
            SerializedProperty enabledProperty,
            SerializedProperty settingsProperty,
            string label)
        {
            EditorGUILayout.PropertyField(
                enabledProperty,
                new GUIContent(label));

            bool disabled =
                enabledProperty != null &&
                !enabledProperty.hasMultipleDifferentValues &&
                !enabledProperty.boolValue;

            using (new EditorGUI.DisabledScope(disabled))
            using (new EditorGUI.IndentLevelScope())
            {
                if (settingsProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        settingsProperty,
                        new GUIContent("Settings"),
                        true);
                }
            }
        }

        private void DrawManualFootprint()
        {
            EditorGUILayout.PropertyField(useSeparateFootprintDimensions);

            if (useSeparateFootprintDimensions.boolValue)
            {
                EditorGUILayout.PropertyField(acrossFlowHalfWidth);
                EditorGUILayout.PropertyField(alongFlowHalfLength);
            }
            else
            {
                EditorGUILayout.PropertyField(linkedFootprintRadius);
            }
        }
    }
}
