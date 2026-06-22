using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiverPlanarReflection))]
    [CanEditMultipleObjects]
    internal sealed class StylizedRiverPlanarReflectionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Planar Reflection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reflectionsEnabled"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sourceCameraOverride"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("resolutionScale"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reflectionMask"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("renderSkybox"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("renderShadows"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("includeSceneView"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("clipPlaneOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxRenderDistance"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reflectionStrength"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("reflectionDistortion"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Update Cost", EditorStyles.boldLabel);
            SerializedProperty updateMode = serializedObject.FindProperty("updateMode");
            EditorGUILayout.PropertyField(updateMode);

            if ((StylizedRiverReflectionUpdateMode)updateMode.enumValueIndex ==
                StylizedRiverReflectionUpdateMode.EveryNthFrame)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("updateEveryNFrames"));
            }

            serializedObject.ApplyModifiedProperties();

            if (targets.Length == 1)
            {
                StylizedRiverPlanarReflection reflection =
                    target as StylizedRiverPlanarReflection;

                if (reflection != null)
                {
                    EditorGUILayout.Space(8f);
                    EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(
                        "Reflection Texture",
                        reflection.HasRenderedTexture
                            ? $"{reflection.ReflectionTexture.width} × {reflection.ReflectionTexture.height}"
                            : "Not allocated");
                }
            }

            EditorGUILayout.Space(8f);

            if (GUILayout.Button("Render Reflection Now"))
            {
                for (int index = 0; index < targets.Length; index++)
                {
                    if (targets[index] is StylizedRiverPlanarReflection reflection)
                    {
                        reflection.RequestRender();
                        EditorUtility.SetDirty(reflection);
                    }
                }

                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Release Reflection Texture"))
            {
                for (int index = 0; index < targets.Length; index++)
                {
                    if (targets[index] is StylizedRiverPlanarReflection reflection)
                    {
                        reflection.ReleaseReflectionTexture();
                    }
                }
            }
        }
    }
}
