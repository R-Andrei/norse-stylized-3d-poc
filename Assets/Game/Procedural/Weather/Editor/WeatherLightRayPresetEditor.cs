#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherLightRayPreset))]
    public sealed class WeatherLightRayPresetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.HelpBox(
                "One preset defines the shared appearance and beam behaviour for every active LightRay. Individual rays retain only placement, lifecycle, seed, local intensity, and explicit geometry overrides.",
                MessageType.Info);
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
