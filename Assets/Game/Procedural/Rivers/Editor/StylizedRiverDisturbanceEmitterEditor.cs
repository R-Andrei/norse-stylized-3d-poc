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
        private SerializedProperty footprintMode;
        private SerializedProperty automaticMeshFilter;
        private SerializedProperty automaticFootprintPadding;
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
            footprintMode = serializedObject.FindProperty("footprintMode");
            automaticMeshFilter = serializedObject.FindProperty(
                "automaticMeshFilter");
            automaticFootprintPadding = serializedObject.FindProperty(
                "automaticFootprintPadding");
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

            EditorGUILayout.LabelField("River Contact", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(explicitRiver);
            EditorGUILayout.PropertyField(autoDetectRiver);
            EditorGUILayout.PropertyField(verticalContactTolerance);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Source Footprint", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceMobility);
            EditorGUILayout.PropertyField(footprintMode);

            bool registryManaged = false;
            if (!serializedObject.isEditingMultipleObjects)
            {
                StylizedRiverDisturbanceEmitter emitter =
                    (StylizedRiverDisturbanceEmitter)target;
                registryManaged =
                    emitter.IsAutomaticallyManagedGeneratedSource;

                if (registryManaged)
                {
                    EditorGUILayout.HelpBox(
                        "This static generated object is discovered directly by the river through the generated-geometry registry. This emitter does not register a duplicate static obstruction. Remove it unless the object also needs dynamic movement or explicit impact behaviour.",
                        MessageType.Info);
                }
            }

            if (registryManaged)
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            bool automatic =
                footprintMode.enumValueIndex ==
                (int)StylizedRiverDisturbanceFootprintMode.Automatic;
            bool isStatic =
                sourceMobility.enumValueIndex ==
                (int)StylizedRiverDisturbanceSourceMobility.Static;

            if (automatic)
            {
                EditorGUILayout.PropertyField(automaticMeshFilter);
                EditorGUILayout.PropertyField(automaticFootprintPadding);

                if (!isStatic)
                {
                    EditorGUILayout.HelpBox(
                        "Automatic waterline footprints are currently used by Static sources. Dynamic sources retain their manual footprint dimensions.",
                        MessageType.Info);
                    DrawManualFootprint();
                }
                else if (!serializedObject.isEditingMultipleObjects)
                {
                    StylizedRiverDisturbanceEmitter emitter =
                        (StylizedRiverDisturbanceEmitter)target;
                    EditorGUILayout.HelpBox(
                        emitter.AutomaticFootprintStatus,
                        emitter.HasResolvedAutomaticFootprint
                            ? MessageType.Info
                            : MessageType.None);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.FloatField(
                            "Resolved Across Half Width",
                            emitter.ResolvedAutomaticAcrossHalfWidth);
                        EditorGUILayout.FloatField(
                            "Resolved Along Half Length",
                            emitter.ResolvedAutomaticAlongHalfLength);
                    }
                }
            }
            else
            {
                DrawManualFootprint();
            }
            

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
