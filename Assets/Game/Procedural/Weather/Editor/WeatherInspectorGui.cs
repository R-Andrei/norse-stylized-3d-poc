using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather.Editor
{
    internal static class WeatherInspectorGui
    {
        public static void DrawScriptReference(SerializedObject serializedObject)
        {
            SerializedProperty scriptProperty =
                serializedObject.FindProperty("m_Script");
            if (scriptProperty == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }
        }

        public static bool Foldout(
            ref bool expanded,
            string label,
            string tooltip = null)
        {
            EditorGUILayout.Space(2f);
            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);
            return expanded;
        }

        public static SerializedProperty Property(
            SerializedObject serializedObject,
            string propertyName,
            string label,
            string tooltip,
            bool includeChildren = true)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                EditorGUILayout.HelpBox(
                    $"Inspector property '{propertyName}' was not found.",
                    MessageType.Error);
                return null;
            }

            EditorGUILayout.PropertyField(
                property,
                new GUIContent(label, tooltip),
                includeChildren);
            return property;
        }

        public static void MinMaxProperties(
            SerializedObject serializedObject,
            string label,
            string minimumPropertyName,
            string minimumLabel,
            string minimumTooltip,
            string maximumPropertyName,
            string maximumLabel,
            string maximumTooltip)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                Property(
                    serializedObject,
                    minimumPropertyName,
                    minimumLabel,
                    minimumTooltip);
                Property(
                    serializedObject,
                    maximumPropertyName,
                    maximumLabel,
                    maximumTooltip);
            }
        }

        public static void ReadOnlyRow(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(label, value ?? string.Empty);
            }
        }

        public static void ReadOnlyRow(string label, int value)
        {
            ReadOnlyRow(label, value.ToString());
        }

        public static void ReadOnlyRow(string label, long value)
        {
            ReadOnlyRow(label, value.ToString("N0"));
        }

        public static void ReadOnlyRow(
            string label,
            float value,
            string format = "0.###")
        {
            ReadOnlyRow(label, value.ToString(format));
        }

        public static void ReadOnlyObject(
            string label,
            Object value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    label,
                    value,
                    value != null ? value.GetType() : typeof(Object),
                    true);
            }
        }

        public static void Help(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.None);
        }

        public static void Info(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Info);
        }

        public static void Warning(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        public static void Error(string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Error);
        }
    }
}
