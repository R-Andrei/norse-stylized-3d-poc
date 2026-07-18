using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    [CustomEditor(typeof(StylizedSurfaceDetailLibrary))]
    public sealed class StylizedSurfaceDetailLibraryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            bool inspectorChanged = DrawDefaultInspector();
            bool applied = serializedObject.ApplyModifiedProperties();

            StylizedSurfaceDetailLibrary library =
                (StylizedSurfaceDetailLibrary)target;
            if (inspectorChanged || applied)
            {
                EditorUtility.SetDirty(library);
                StylizedSurfaceDetailLibraryBuilder.ScheduleRepair();
            }

            IReadOnlyList<string> validation =
                StylizedSurfaceDetailLibraryBuilder.Validate(library);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Generated Array",
                EditorStyles.boldLabel);

            Texture2DArray array = library.GeneratedTextureArray;
            EditorGUILayout.ObjectField(
                "Texture Array",
                array,
                typeof(Texture2DArray),
                false);
            EditorGUILayout.LabelField(
                "Status",
                validation.Count > 0
                    ? "Invalid source configuration"
                    : StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library)
                        ? "Missing or stale"
                        : "Current");

            for (int index = 0; index < validation.Count; index++)
            {
                EditorGUILayout.HelpBox(
                    validation[index],
                    MessageType.Error);
            }

            using (new EditorGUI.DisabledScope(validation.Count > 0))
            {
                if (GUILayout.Button("Rebuild Packed Detail Array"))
                {
                    StylizedSurfaceDetailLibraryBuilder.Rebuild(library);
                }
            }
        }
    }
}
