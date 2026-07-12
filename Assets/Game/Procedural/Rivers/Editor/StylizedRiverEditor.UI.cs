using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private bool IsSectionOpen(InspectorSection section)
        {
            return openInspectorSections.Contains(section);
        }

        private void DrawTopLevelSection(
            InspectorSection section,
            string label,
            InspectorSectionDrawer drawer)
        {
            EditorGUILayout.Space(4f);

            bool wasOpen = IsSectionOpen(section);
            bool isOpen = EditorGUILayout.Foldout(
                wasOpen,
                label,
                true);

            if (isOpen != wasOpen)
            {
                if (isOpen)
                {
                    openInspectorSections.Add(section);
                }
                else
                {
                    openInspectorSections.Remove(section);
                }
            }

            if (!isOpen)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                drawer();
            }
        }

        private void DrawNestedSection(
            InspectorSection section,
            string label,
            InspectorSectionDrawer drawer)
        {
            bool wasOpen = IsSectionOpen(section);
            bool isOpen = EditorGUILayout.Foldout(
                wasOpen,
                label,
                true);

            if (isOpen != wasOpen)
            {
                if (isOpen)
                {
                    openInspectorSections.Add(section);
                }
                else
                {
                    openInspectorSections.Remove(section);
                }
            }

            if (!isOpen)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                drawer();
            }
        }

        private bool DrawInlineFoldout(
            InspectorSection section,
            string label)
        {
            bool wasOpen = IsSectionOpen(section);
            bool isOpen = EditorGUILayout.Foldout(
                wasOpen,
                label,
                true);

            if (isOpen != wasOpen)
            {
                if (isOpen)
                {
                    openInspectorSections.Add(section);
                }
                else
                {
                    openInspectorSections.Remove(section);
                }
            }

            return isOpen;
        }

        private static void DrawReadOnlyRow(
            GUIContent label,
            string value)
        {
            Rect row = EditorGUILayout.GetControlRect();
            Rect valueRect = EditorGUI.PrefixLabel(row, label);
            EditorGUI.SelectableLabel(
                valueRect,
                string.IsNullOrEmpty(value) ? "—" : value,
                EditorStyles.label);
        }

        private static string FormatMemoryBytes(long bytes)
        {
            if (bytes < 0L)
            {
                return "—";
            }

            const float kilobyte = 1024f;
            const float megabyte = kilobyte * 1024f;

            return bytes >= megabyte
                ? $"{bytes / megabyte:0.00} MB"
                : bytes >= kilobyte
                    ? $"{bytes / kilobyte:0.0} KB"
                    : $"{bytes:N0} B";
        }

        private SerializedProperty Find(string propertyName)
        {
            return serializedObject.FindProperty(propertyName);
        }

        private static string FormatPercent(float value)
        {
            return $"{Mathf.Clamp01(value) * 100f:0.0}%";
        }

        private void ApplyToTargets(string undoName, RiverAction action)
        {
            foreach (Object selectedTarget in targets)
            {
                StylizedRiver river = selectedTarget as StylizedRiver;

                if (river == null)
                {
                    continue;
                }

                Undo.RecordObject(river, undoName);
                action(river);
                EditorUtility.SetDirty(river);
            }

            serializedObject.Update();
            Repaint();
            RepaintScene();
        }

        private static void RepaintScene()
        {
            SceneView.RepaintAll();
        }

        private delegate void InspectorSectionDrawer();

        private delegate void RiverAction(StylizedRiver river);
    }
}
