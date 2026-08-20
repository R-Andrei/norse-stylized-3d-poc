using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeControlRangeDrawer
    {
        internal static void Draw(
            SerializedProperty controlsProperty,
            UnityEngine.Object owner)
        {
            if (controlsProperty == null)
            {
                EditorGUILayout.HelpBox(
                    "Recipe-only control ranges are missing.",
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
                    "PS3D.TreeControls.Range." + ownerKey + "." +
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
                DrawSectionNote(section.Note);
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

        private static void DrawSectionNote(string note)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    note,
                    EditorStyles.textArea,
                    GUILayout.MinHeight(42f));
            }
            EditorGUILayout.Space(3f);
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
                    "Missing serialized range for " +
                    descriptor.StableId + ".",
                    MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(
                       !IsConditionEnabled(
                           controlsProperty,
                           descriptor.PropertyName)))
            {
                switch (descriptor.Kind)
                {
                    case TreeControlValueKind.Integer:
                        DrawIntegerRange(property, descriptor);
                        break;
                    case TreeControlValueKind.Angle:
                        DrawAngleRange(property, descriptor);
                        break;
                    case TreeControlValueKind.Color:
                        DrawColorRange(property, descriptor);
                        break;
                    default:
                        DrawFloatRange(property, descriptor);
                        break;
                }
            }
        }

        private static void DrawFloatRange(
            SerializedProperty property,
            TreeControlDescriptor descriptor)
        {
            SerializedProperty minimum = property.FindPropertyRelative(
                "minimum");
            SerializedProperty maximum = property.FindPropertyRelative(
                "maximum");
            GUIContent label = new GUIContent(
                descriptor.Label,
                descriptor.Tooltip);

            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            minimum.floatValue = EditorGUILayout.FloatField(
                "Min",
                minimum.floatValue);
            maximum.floatValue = EditorGUILayout.FloatField(
                "Max",
                maximum.floatValue);
            EditorGUILayout.EndHorizontal();

            float min = Mathf.Clamp(
                minimum.floatValue,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            float max = Mathf.Clamp(
                maximum.floatValue,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            if (max < min)
            {
                (min, max) = (max, min);
            }

            EditorGUILayout.MinMaxSlider(
                ref min,
                ref max,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            minimum.floatValue = min;
            maximum.floatValue = max;
            EditorGUI.indentLevel--;
        }

        private static void DrawIntegerRange(
            SerializedProperty property,
            TreeControlDescriptor descriptor)
        {
            SerializedProperty minimum = property.FindPropertyRelative(
                "minimum");
            SerializedProperty maximum = property.FindPropertyRelative(
                "maximum");
            GUIContent label = new GUIContent(
                descriptor.Label,
                descriptor.Tooltip);

            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            minimum.intValue = EditorGUILayout.IntField(
                "Min",
                minimum.intValue);
            maximum.intValue = EditorGUILayout.IntField(
                "Max",
                maximum.intValue);
            EditorGUILayout.EndHorizontal();

            float min = Mathf.Clamp(
                minimum.intValue,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            float max = Mathf.Clamp(
                maximum.intValue,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            if (max < min)
            {
                (min, max) = (max, min);
            }

            EditorGUILayout.MinMaxSlider(
                ref min,
                ref max,
                descriptor.HardMinimum,
                descriptor.HardMaximum);
            minimum.intValue = Mathf.RoundToInt(min);
            maximum.intValue = Mathf.RoundToInt(max);
            EditorGUI.indentLevel--;
        }

        private static void DrawAngleRange(
            SerializedProperty property,
            TreeControlDescriptor descriptor)
        {
            SerializedProperty minimum = property.FindPropertyRelative(
                "minimum");
            SerializedProperty maximum = property.FindPropertyRelative(
                "maximum");
            GUIContent label = new GUIContent(
                descriptor.Label,
                descriptor.Tooltip);

            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            minimum.floatValue = NormalizeAngle(
                EditorGUILayout.FloatField("Min", minimum.floatValue));
            maximum.floatValue = NormalizeAngle(
                EditorGUILayout.FloatField("Max", maximum.floatValue));
            EditorGUILayout.EndHorizontal();
            if (maximum.floatValue < minimum.floatValue)
            {
                EditorGUILayout.LabelField(
                    "Interval wraps through 0°.",
                    EditorStyles.miniLabel);
            }
            EditorGUI.indentLevel--;
        }

        private static void DrawColorRange(
            SerializedProperty property,
            TreeControlDescriptor descriptor)
        {
            SerializedProperty minimum = property.FindPropertyRelative(
                "minimum");
            SerializedProperty maximum = property.FindPropertyRelative(
                "maximum");
            GUIContent label = new GUIContent(
                descriptor.Label,
                descriptor.Tooltip);

            EditorGUILayout.LabelField(label);
            EditorGUI.indentLevel++;
            Color minColour = EditorGUILayout.ColorField(
                new GUIContent("Min"),
                minimum.colorValue,
                true,
                false,
                false);
            Color maxColour = EditorGUILayout.ColorField(
                new GUIContent("Max"),
                maximum.colorValue,
                true,
                false,
                false);
            minColour.a = 1f;
            maxColour.a = 1f;
            minimum.colorValue = minColour;
            maximum.colorValue = maxColour;
            EditorGUI.indentLevel--;
        }

        private static bool IsConditionEnabled(
            SerializedProperty controls,
            string propertyName)
        {
            if (propertyName == "signedPathSpiralTurns")
            {
                return GetFloatRangeMaximum(
                    controls,
                    "pathSpiralRadius") > 0f;
            }

            if (propertyName == "directionalBiasAngle")
            {
                return GetFloatRangeMaximum(
                    controls,
                    "directionalBias") > 0f;
            }

            if (propertyName == "secondaryDensity")
            {
                return GetIntRangeMaximum(
                    controls,
                    "maximumBranchOrder") >= 2;
            }

            if (propertyName == "tertiaryDensity")
            {
                return GetIntRangeMaximum(
                    controls,
                    "maximumBranchOrder") >= 3;
            }

            return true;
        }

        private static float GetFloatRangeMaximum(
            SerializedProperty controls,
            string propertyName)
        {
            SerializedProperty property = controls.FindPropertyRelative(
                propertyName);
            SerializedProperty maximum = property?.FindPropertyRelative(
                "maximum");
            return maximum != null ? maximum.floatValue : 0f;
        }

        private static int GetIntRangeMaximum(
            SerializedProperty controls,
            string propertyName)
        {
            SerializedProperty property = controls.FindPropertyRelative(
                propertyName);
            SerializedProperty maximum = property?.FindPropertyRelative(
                "maximum");
            return maximum != null ? maximum.intValue : 0;
        }

        private static float NormalizeAngle(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                return 0f;
            }

            if (Mathf.Approximately(value, 360f))
            {
                return 360f;
            }

            value %= 360f;
            return value < 0f ? value + 360f : value;
        }
    }
}
