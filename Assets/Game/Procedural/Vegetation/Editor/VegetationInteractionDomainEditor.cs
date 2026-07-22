using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationInteractionDomain))]
    public sealed class VegetationInteractionDomainEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var domain = (VegetationInteractionDomain)target;
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(domain);
                SceneView.RepaintAll();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Published",
                VegetationInteractionDomain.PublishedDomain == domain
                    ? "Yes"
                    : "No");
            EditorGUILayout.LabelField(
                "Active Domains",
                VegetationInteractionDomain.ActiveDomainCount.ToString());
            EditorGUILayout.LabelField(
                "Simulation State",
                !Application.isPlaying
                    ? "Inactive — Play Mode simulation not running"
                    : domain.ResourcesReady
                        ? "Ready"
                        : "Not ready");
            EditorGUILayout.LabelField(
                "Field Coverage",
                $"{domain.FieldWorldSizeMetres:0.###} × " +
                $"{domain.FieldWorldSizeMetres:0.###} m");
            EditorGUILayout.LabelField(
                "Uploaded / Overflow",
                $"{domain.LastUploadedInteractorCount} / " +
                domain.LastOverflowInteractorCount);
            EditorGUILayout.LabelField(
                "Texture Memory",
                domain.EstimatedTextureBytes.ToString("N0") + " bytes");

            if (VegetationInteractionDomain.ActiveDomainCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "More than one active VegetationInteractionDomain exists. " +
                    "Only the most recently enabled domain publishes shader state.",
                    MessageType.Warning);
            }
            if (!string.IsNullOrEmpty(domain.LastError))
            {
                EditorGUILayout.HelpBox(domain.LastError, MessageType.Error);
            }
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Immediate interaction simulation runs in Play Mode. " +
                    "Edit Mode vegetation receives a cleared interaction field.",
                    MessageType.Info);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset Immediate Interaction Field"))
            {
                domain.ResetField();
                EditorUtility.SetDirty(domain);
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Copy Immediate Interaction Report"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    domain.BuildComprehensiveReport();
                Debug.Log(
                    "[Vegetation INTERACT.1B] Immediate interaction report " +
                    "copied to clipboard.",
                    domain);
            }
        }
    }
}
