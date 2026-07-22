using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(GroundVegetation))]
    public sealed class GroundVegetationEditor : UnityEditor.Editor
    {
        private VegetationLayer duplicateSource;

        public override void OnInspectorGUI()
        {
            var root = (GroundVegetation)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Resolved Ground",
                    root.SurfaceGround,
                    typeof(GeneratedGround),
                    true);
            }
            EditorGUILayout.LabelField(
                "Direct Recipe Layers",
                root.DirectLayerCount.ToString());

            if (root.SurfaceGround == null)
            {
                EditorGUILayout.HelpBox(
                    "Invalid hierarchy: GroundVegetation requires a " +
                    "GeneratedGround ancestor.",
                    MessageType.Error);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layer Authoring", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(root.SurfaceGround == null))
            {
                if (GUILayout.Button("Create Empty Layer"))
                {
                    VegetationLayer created =
                        VegetationLayerAuthoring.CreateEmptyLayer(
                            root,
                            "Vegetation Layer");
                    Selection.activeGameObject = created.gameObject;
                    SceneView.RepaintAll();
                }

                duplicateSource = (VegetationLayer)EditorGUILayout.ObjectField(
                    "Recipe to Duplicate",
                    duplicateSource,
                    typeof(VegetationLayer),
                    true);
                bool duplicateSourceValid =
                    duplicateSource != null &&
                    duplicateSource.transform.parent == root.transform;
                if (duplicateSource != null && !duplicateSourceValid)
                {
                    EditorGUILayout.HelpBox(
                        "Recipe to Duplicate must be a direct VegetationLayer child " +
                        "of this Vegetation root.",
                        MessageType.Warning);
                }
                using (new EditorGUI.DisabledScope(!duplicateSourceValid))
                {
                    if (GUILayout.Button("Duplicate Recipe as Empty Layer"))
                    {
                        VegetationLayer created =
                            VegetationLayerAuthoring.DuplicateLayerAsEmpty(
                                root,
                                duplicateSource);
                        duplicateSource = created;
                        Selection.activeGameObject = created.gameObject;
                        SceneView.RepaintAll();
                    }
                }

                if (GUILayout.Button("Rebuild All Layers"))
                {
                    var layers = new List<VegetationLayer>();
                    root.CollectDirectLayers(layers);
                    for (int index = 0; index < layers.Count; index++)
                    {
                        layers[index].RebuildVegetation();
                        EditorUtility.SetDirty(layers[index]);
                    }
                    EditorApplication.QueuePlayerLoopUpdate();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Historical Trample", EditorStyles.boldLabel);
            VegetationTrampleDomain trampleDomain = root.TrampleDomain;
            if (trampleDomain == null)
            {
                EditorGUILayout.HelpBox(
                    "No historical trample field is attached. Ordinary immediate interaction remains available and history-free.",
                    MessageType.Info);
                using (new EditorGUI.DisabledScope(root.SurfaceGround == null))
                {
                    if (GUILayout.Button("Add Historical Trample Domain"))
                    {
                        trampleDomain = Undo.AddComponent<VegetationTrampleDomain>(
                            root.gameObject);
                        EditorUtility.SetDirty(root.gameObject);
                        Selection.activeObject = trampleDomain;
                    }
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        "Trample Domain",
                        trampleDomain,
                        typeof(VegetationTrampleDomain),
                        true);
                }
                if (GUILayout.Button("Select Historical Trample Domain"))
                {
                    Selection.activeObject = trampleDomain;
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate Layer Stack"))
            {
                Debug.Log(root.BuildLayerStackReport(), root);
            }
            if (GUILayout.Button("Copy Layer Stack Report"))
            {
                EditorGUIUtility.systemCopyBuffer = root.BuildLayerStackReport();
                Debug.Log(
                    "[Vegetation INFRA.1B] Ground layer-stack report copied to clipboard.",
                    root);
            }
        }
    }
}
