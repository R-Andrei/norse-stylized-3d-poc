using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationTrampleDomain))]
    public sealed class VegetationTrampleDomainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var domain = (VegetationTrampleDomain)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset Historical Trample Field"))
            {
                domain.ResetField();
                EditorUtility.SetDirty(domain);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Copy Historical Trample Report"))
            {
                EditorGUIUtility.systemCopyBuffer = domain.BuildReport();
                Debug.Log(
                    "[Vegetation INTERACT.2B] Historical trample report copied to clipboard.",
                    domain);
            }

            EditorGUILayout.Space();
            string status = !Application.isPlaying
                ? "Inactive in Edit Mode. Historical simulation starts in Play Mode."
                : domain.ResourcesReady
                    ? $"Ready: {domain.LastUploadedWriterCount} trail writers, {domain.PendingAbilityStampCount} pending ability stamps, {domain.LastUploadedAbilityStampCount} ability stamps uploaded on the last historical step."
                    : "Historical trample resources are not ready.";
            MessageType statusType = !Application.isPlaying || domain.ResourcesReady
                ? MessageType.Info
                : MessageType.Warning;
            EditorGUILayout.HelpBox(status, statusType);
            if (!string.IsNullOrEmpty(domain.LastError))
            {
                EditorGUILayout.HelpBox(domain.LastError, MessageType.Error);
            }
        }
    }
}
