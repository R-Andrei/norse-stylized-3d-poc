using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeResolvedControlsDrawer
    {
        internal static void Draw(
            SerializedProperty controlsProperty,
            UnityEngine.Object owner)
        {
            if (controlsProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Exact recipe-only controls are missing.",
                    MessageType.Error);
                return;
            }

            string ownerKey = owner != null
                ? owner.GetEntityId().ToString()
                : "none";
            foreach (TreeControlSectionDescriptor section in
                     TreeControlDescriptorRegistry.Sections)
            {
                string sessionKey =
                    "PS3D.TreeControls.Exact." + ownerKey + "." +
                    section.Key;
                bool expanded = SessionState.GetBool(sessionKey, false);
                bool nextExpanded = EditorGUILayout.Foldout(
                    expanded,
                    section.Label,
                    true,
                    EditorStyles.foldoutHeader);
                if (nextExpanded != expanded)
                {
                    SessionState.SetBool(sessionKey, nextExpanded);
                }

                if (!nextExpanded)
                {
                    continue;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextArea(
                        section.Note,
                        EditorStyles.textArea,
                        GUILayout.MinHeight(42f));
                }
                EditorGUILayout.Space(3f);

                foreach (TreeControlDescriptor control in
                         TreeControlDescriptorRegistry.Enumerate(
                             section.Section))
                {
                    DrawControl(controlsProperty, control);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2f);
            }
        }

        private static void DrawControl(
            SerializedProperty controlsProperty,
            TreeControlDescriptor descriptor)
        {
            SerializedProperty property = controlsProperty.FindPropertyRelative(
                descriptor.PropertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing exact control for " +
                    descriptor.StableId + ".",
                    MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(
                       !IsConditionEnabled(
                           controlsProperty,
                           descriptor.PropertyName)))
            {
                GUIContent label = new GUIContent(
                    descriptor.Label,
                    descriptor.Tooltip);
                switch (descriptor.Kind)
                {
                    case TreeControlValueKind.Integer:
                        property.intValue = EditorGUILayout.IntSlider(
                            label,
                            property.intValue,
                            Mathf.RoundToInt(descriptor.HardMinimum),
                            Mathf.RoundToInt(descriptor.HardMaximum));
                        break;
                    case TreeControlValueKind.Angle:
                        property.floatValue = EditorGUILayout.Slider(
                            label,
                            property.floatValue,
                            descriptor.HardMinimum,
                            descriptor.HardMaximum);
                        break;
                    case TreeControlValueKind.Color:
                        Color exactColour = EditorGUILayout.ColorField(
                            label,
                            property.colorValue,
                            true,
                            false,
                            false);
                        exactColour.a = 1f;
                        property.colorValue = exactColour;
                        break;
                    default:
                        property.floatValue = EditorGUILayout.Slider(
                            label,
                            property.floatValue,
                            descriptor.HardMinimum,
                            descriptor.HardMaximum);
                        break;
                }
            }
        }

        private static bool IsConditionEnabled(
            SerializedProperty controls,
            string propertyName)
        {
            if (propertyName == "leanDirection")
            {
                return GetFloat(controls, "leanAmount") > 0f;
            }

            if (propertyName == "signedPathSpiralTurns")
            {
                return GetFloat(controls, "pathSpiralRadius") > 0f;
            }

            if (propertyName == "directionalBiasAngle")
            {
                return GetFloat(controls, "directionalBias") > 0f;
            }

            if (propertyName == "secondaryDensity")
            {
                return GetInt(controls, "maximumBranchOrder") >= 2;
            }

            if (propertyName == "tertiaryDensity")
            {
                return GetInt(controls, "maximumBranchOrder") >= 3;
            }

            return true;
        }

        private static float GetFloat(
            SerializedProperty controls,
            string propertyName)
        {
            SerializedProperty property = controls.FindPropertyRelative(
                propertyName);
            return property != null ? property.floatValue : 0f;
        }

        private static int GetInt(
            SerializedProperty controls,
            string propertyName)
        {
            SerializedProperty property = controls.FindPropertyRelative(
                propertyName);
            return property != null ? property.intValue : 0;
        }
    }
}
