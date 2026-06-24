using UnityEditor;

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
        private SerializedProperty emitExitImpact;
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
            emitExitImpact = serializedObject.FindProperty(
                "emitExitImpact");
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
            EditorGUILayout.LabelField("Influence", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(strength);
            EditorGUILayout.PropertyField(geometryContribution);
            EditorGUILayout.PropertyField(normalContribution);
            EditorGUILayout.PropertyField(stationaryObstruction);
            EditorGUILayout.PropertyField(emitEntryImpact);
            EditorGUILayout.PropertyField(emitExitImpact);
            EditorGUILayout.PropertyField(sourceUpdateInterval);

            serializedObject.ApplyModifiedProperties();
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
