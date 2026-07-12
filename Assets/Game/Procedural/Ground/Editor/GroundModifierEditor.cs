using UnityEditor;
using UnityEngine;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    [CustomEditor(typeof(GroundModifier))]
    [CanEditMultipleObjects]
    public sealed class GroundModifierEditor : UnityEditor.Editor
    {
        private SerializedProperty mode;
        private SerializedProperty shape;
        private SerializedProperty priority;
        private SerializedProperty surfaceEffectMode;
        private SerializedProperty featureExclusions;
        private SerializedProperty surfaceCompactionStrength;
        private SerializedProperty surfaceDampDepositStrength;
        private SerializedProperty surfaceStandingWaterStrength;
        private SerializedProperty strength;
        private SerializedProperty blendDistance;
        private SerializedProperty circleRadius;
        private SerializedProperty boxSize;
        private SerializedProperty heightAmount;
        private SerializedProperty preserveDetail;
        private SerializedProperty autoRegenerateParent;

        private void OnEnable()
        {
            mode = serializedObject.FindProperty("mode");
            shape = serializedObject.FindProperty("shape");
            priority = serializedObject.FindProperty("priority");
            surfaceEffectMode = serializedObject.FindProperty("surfaceEffectMode");
            featureExclusions = serializedObject.FindProperty("featureExclusions");
            surfaceCompactionStrength = serializedObject.FindProperty("surfaceCompactionStrength");
            surfaceDampDepositStrength = serializedObject.FindProperty("surfaceDampDepositStrength");
            surfaceStandingWaterStrength = serializedObject.FindProperty("surfaceStandingWaterStrength");
            strength = serializedObject.FindProperty("strength");
            blendDistance = serializedObject.FindProperty("blendDistance");
            circleRadius = serializedObject.FindProperty("circleRadius");
            boxSize = serializedObject.FindProperty("boxSize");
            heightAmount = serializedObject.FindProperty("heightAmount");
            preserveDetail = serializedObject.FindProperty("preserveDetail");
            autoRegenerateParent = serializedObject.FindProperty("autoRegenerateParent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Height Influence", EditorStyles.boldLabel);
            DrawProperty(mode, "Mode");
            DrawProperty(shape, "Shape");
            DrawProperty(priority, "Priority");
            DrawProperty(strength, "Height Strength");
            DrawProperty(blendDistance, "Blend Distance");

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Feature Exclusions", EditorStyles.boldLabel);
            DrawProperty(featureExclusions, "Excluded Surface Features");
            EditorGUILayout.HelpBox(
                "Feature exclusions use the full modifier shape plus Blend Distance. Mode None and Surface Effect Mode None create a pure deterministic placement exclusion zone without changing height or surface masks.",
                MessageType.None);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Authored Surface Masks", EditorStyles.boldLabel);
            DrawProperty(surfaceEffectMode, "Surface Effect Mode");

            if (surfaceEffectMode == null)
            {
                EditorGUILayout.HelpBox(
                    "Surface mask fields were not found on this GroundModifier script. Reimport the Patch T GroundModifier.cs file before using surface masks.",
                    MessageType.Error);
            }
            else
            {
                if (surfaceEffectMode.enumValueIndex == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Auto From Height preserves the legacy behavior: Flatten writes compaction/path influence; Raise/Lower/None do not write authored surface masks.",
                        MessageType.Info);
                }
                else if (surfaceEffectMode.enumValueIndex == 1)
                {
                    EditorGUILayout.HelpBox(
                        "Surface masks are disabled for this modifier. Height behavior still applies if Mode is Flatten, Raise, or Lower.",
                        MessageType.Info);
                }
                else
                {
                    DrawProperty(surfaceCompactionStrength, "Compaction / Path Strength");
                    DrawProperty(surfaceDampDepositStrength, "Damp / Deposit Strength");
                    DrawProperty(surfaceStandingWaterStrength, "Standing Water Potential");
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            DrawShapeProperties();

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Mode Settings", EditorStyles.boldLabel);
            DrawModeSettings();

            EditorGUILayout.Space(6f);
            DrawProperty(autoRegenerateParent, "Live Parent Regeneration");

            using (new EditorGUI.DisabledScope(targets.Length != 1))
            {
                if (GUILayout.Button("Regenerate Parent Ground"))
                {
                    ((GroundModifier)target).RegenerateParentGround();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawShapeProperties()
        {
            if (shape == null)
            {
                return;
            }

            switch (shape.enumValueIndex)
            {
                case 0:
                    DrawProperty(circleRadius, "Radius");
                    break;
                case 1:
                    DrawProperty(boxSize, "Box Size");
                    break;
            }
        }

        private void DrawModeSettings()
        {
            if (mode == null)
            {
                return;
            }

            switch (mode.enumValueIndex)
            {
                case 0:
                    DrawProperty(preserveDetail, "Preserve Detail");
                    break;
                case 1:
                    DrawProperty(heightAmount, "Raise Amount");
                    break;
                case 2:
                    DrawProperty(heightAmount, "Lower Amount");
                    break;
                case 3:
                    EditorGUILayout.HelpBox(
                        "Mode None does not change terrain height. Use feature exclusions and/or Custom surface masks above for placement and authored metadata.",
                        MessageType.None);
                    break;
            }
        }

        private static void DrawProperty(
            SerializedProperty property,
            string label)
        {
            if (property == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(label));
        }
    }
}
