using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [InitializeOnLoad]
    internal static class ProceduralTreePreviewSelectionBridge
    {
        static ProceduralTreePreviewSelectionBridge()
        {
            Selection.selectionChanged += RefreshSelection;
            RefreshSelection();
        }

        private static void RefreshSelection()
        {
            GameObject selected = Selection.activeGameObject;
            ProceduralTreeInstance selectedInstance = selected != null
                ? selected.GetComponentInParent<ProceduralTreeInstance>()
                : null;
            ProceduralTreeInstance.SetEditorPreviewSelection(
                selectedInstance);
            SceneView.RepaintAll();
        }
    }

    [CustomEditor(typeof(ProceduralTreeInstance))]
    public sealed class ProceduralTreeInstanceEditor : UnityEditor.Editor
    {
        private SerializedProperty library;
        private SerializedProperty family;
        private SerializedProperty sourceVariantIndex;
        private SerializedProperty masterSeed;
        private SerializedProperty instanceOverrides;
        private SerializedProperty showStructuralPreview;
        private SerializedProperty previewScope;
        private SerializedProperty showTrunk;
        private SerializedProperty showPrimaryBranches;
        private SerializedProperty showHigherOrderBranches;
        private SerializedProperty showAttachmentPoints;
        private SerializedProperty showBounds;
        private SerializedProperty showFramesWhenSelected;
        private SerializedProperty frameDisplayScale;

        private void OnEnable()
        {
            library = serializedObject.FindProperty("library");
            family = serializedObject.FindProperty("family");
            sourceVariantIndex = serializedObject.FindProperty(
                "sourceVariantIndex");
            masterSeed = serializedObject.FindProperty("masterSeed");
            instanceOverrides = serializedObject.FindProperty(
                "instanceOverrides");
            showStructuralPreview = serializedObject.FindProperty(
                "showStructuralPreview");
            previewScope = serializedObject.FindProperty("previewScope");
            showTrunk = serializedObject.FindProperty("showTrunk");
            showPrimaryBranches = serializedObject.FindProperty(
                "showPrimaryBranches");
            showHigherOrderBranches = serializedObject.FindProperty(
                "showHigherOrderBranches");
            showAttachmentPoints = serializedObject.FindProperty(
                "showAttachmentPoints");
            showBounds = serializedObject.FindProperty("showBounds");
            showFramesWhenSelected = serializedObject.FindProperty(
                "showFramesWhenSelected");
            frameDisplayScale = serializedObject.FindProperty(
                "frameDisplayScale");
        }

        public override void OnInspectorGUI()
        {
            var instance = (ProceduralTreeInstance)target;
            serializedObject.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
                EditorGUILayout.PropertyField(library);
                EditorGUILayout.PropertyField(family);
                EditorGUILayout.PropertyField(sourceVariantIndex);
            }

            serializedObject.ApplyModifiedProperties();
            DrawRecipeSelector(instance);
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(masterSeed);
            EditorGUILayout.PropertyField(instanceOverrides, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Structural Preview",
                EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(showStructuralPreview);
                EditorGUILayout.PropertyField(previewScope);
                EditorGUILayout.PropertyField(showTrunk);
                EditorGUILayout.PropertyField(showPrimaryBranches);
                EditorGUILayout.PropertyField(showHigherOrderBranches);
                EditorGUILayout.PropertyField(showAttachmentPoints);
                EditorGUILayout.PropertyField(showBounds);
                EditorGUILayout.PropertyField(showFramesWhenSelected);
            }
            EditorGUILayout.PropertyField(frameDisplayScale);
            EditorGUILayout.HelpBox(
                "Preview scope and visibility are managed from Blockout > Tree Reference Gallery > Generated Tree Library.",
                MessageType.None);

            serializedObject.ApplyModifiedProperties();
            DrawActions(instance);
            DrawStatus(instance);
        }

        private static void DrawRecipeSelector(ProceduralTreeInstance instance)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generation Recipe", EditorStyles.boldLabel);
            TreeGenerationLibrary library = instance.Library;
            if (library == null || library.VariantCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "This slot is not bound to the managed tree-generation library. Rebuild the complete comparison gallery from the Tree Reference Gallery Inspector.",
                    MessageType.Warning);
                return;
            }

            string[] names = new string[library.VariantCount];
            int selectedIndex = 0;
            for (int index = 0; index < library.VariantCount; index++)
            {
                TreeGenerationLibraryVariant variant = library.Variants[index];
                names[index] = variant.DisplayName + " — " +
                    (variant.Recipe != null
                        ? variant.Recipe.StableIdentity
                        : "Missing recipe");
                if (variant.Recipe == instance.Recipe)
                {
                    selectedIndex = index;
                }
            }

            int nextIndex = EditorGUILayout.Popup(
                "Recipe",
                selectedIndex,
                names);
            if (nextIndex != selectedIndex)
            {
                TreeGenerationLibraryVariant variant =
                    library.Variants[nextIndex];
                Undo.RecordObject(instance, "Switch Procedural Tree Recipe");
                instance.ConfigureRecipe(library, variant, true);
                EditorUtility.SetDirty(instance);
                MarkSceneDirty(instance);
            }
        }

        private static void DrawActions(ProceduralTreeInstance instance)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(instance.Recipe == null))
            {
                if (GUILayout.Button("Regenerate This Tree"))
                {
                    Generate(instance);
                }

                if (GUILayout.Button("Randomize Seed And Regenerate"))
                {
                    int seed = TreeDeterministicUtility.DeriveSeed(
                        DateTime.UtcNow.Ticks,
                        instance.GetEntityId(),
                        instance.StableSlotIdentity);
                    Undo.RecordObject(instance, "Randomize Procedural Tree Seed");
                    instance.SetMasterSeed(seed);
                    Generate(instance);
                }
            }

            if (GUILayout.Button("Reset This Tree's Overrides"))
            {
                Undo.RecordObject(instance, "Reset Procedural Tree Overrides");
                instance.ResetInstanceOverrides();
                EditorUtility.SetDirty(instance);
                MarkSceneDirty(instance);
            }

            using (new EditorGUI.DisabledScope(!instance.HasGenerationReport))
            {
                if (GUILayout.Button("Copy This Tree's Generation Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        instance.LastGenerationReport;
                }
            }

            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(instance.LastBarkMeshReport)))
            {
                if (GUILayout.Button("Copy This Tree's Bark Mesh Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        instance.LastBarkMeshReport;
                }
            }
        }

        private static void DrawStatus(ProceduralTreeInstance instance)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Slot",
                instance.Family + " " + instance.SourceVariantIndex);
            EditorGUILayout.LabelField(
                "Recipe",
                instance.Recipe != null
                    ? instance.Recipe.StableIdentity
                    : "Unassigned");
            EditorGUILayout.LabelField(
                "Generated structure",
                instance.HasGeneratedDefinition ? "Available" : "None");
            EditorGUILayout.LabelField(
                "Last generation",
                instance.HasGenerationReport
                    ? instance.LastGenerationPassed ? "PASS" : "FAIL"
                    : "Not run");
            EditorGUILayout.LabelField(
                "Generation revision",
                instance.GenerationRevision.ToString());
            if (instance.HasGeneratedDefinition)
            {
                EditorGUILayout.LabelField(
                    "Branches",
                    instance.GeneratedDefinition.Metrics.BranchCount.ToString());
                EditorGUILayout.LabelField(
                    "Reference ratio H/W/D",
                    instance.GeneratedDefinition.Metrics.CalibrationHeightRatio.ToString("F3") +
                    " / " +
                    instance.GeneratedDefinition.Metrics.CalibrationWidthRatio.ToString("F3") +
                    " / " +
                    instance.GeneratedDefinition.Metrics.CalibrationDepthRatio.ToString("F3"));
                EditorGUILayout.LabelField(
                    "Max arc/chord",
                    instance.GeneratedDefinition.Metrics.MaximumArcChordRatio.ToString("F3"));
                EditorGUILayout.LabelField(
                    "Backward violations",
                    instance.GeneratedDefinition.Metrics.BackwardProgressViolationCount.ToString());
                EditorGUILayout.LabelField(
                    "Structural fingerprint",
                    instance.GeneratedDefinition.StructuralFingerprint);
            }

            EditorGUILayout.LabelField(
                "Generated bark mesh",
                instance.HasGeneratedBarkMesh
                    ? "Available"
                    : instance.SourceVariantIndex == 1
                        ? "Not built or stale"
                        : "Not part of TREE-GEN.2A slice");
            if (instance.HasGeneratedBarkMesh)
            {
                EditorGUILayout.LabelField(
                    "Bark vertices / triangles",
                    instance.GeneratedBarkVertexCount + " / " +
                    instance.GeneratedBarkTriangleCount);
                EditorGUILayout.LabelField(
                    "Bark mesh fingerprint",
                    instance.GeneratedBarkFingerprint);
            }
        }

        private static void Generate(ProceduralTreeInstance instance)
        {
            Undo.RecordObject(instance, "Generate Procedural Tree Structure");
            TreeGenerationResult result = instance.GenerateStructure();
            string clipboardReport = result.Report;
            if (result.Passed && instance.SourceVariantIndex == 1)
            {
                TreeReferenceGallery gallery =
                    instance.GetComponentInParent<TreeReferenceGallery>();
                if (gallery == null)
                {
                    const string barkFailure =
                        "The procedural tree instance is not parented beneath a Tree Reference Gallery.";
                    clipboardReport +=
                        "\n\n[TREE-GEN.2A Bark Mesh]\nStatus: FAIL\n" +
                        barkFailure;
                    Debug.LogError(
                        "[TREE-GEN.2A] Bark mesh regeneration failed: " +
                        barkFailure,
                        instance);
                }
                else if (TreeBarkMeshAssetBuilder.BuildOrUpdate(
                             gallery,
                             instance.Library,
                             instance,
                             out _,
                             out string barkReport,
                             out string barkFailure))
                {
                    clipboardReport += "\n\n" + barkReport;
                }
                else
                {
                    clipboardReport +=
                        "\n\n[TREE-GEN.2A Bark Mesh]\nStatus: FAIL\n" +
                        barkFailure;
                    Debug.LogError(
                        "[TREE-GEN.2A] Bark mesh regeneration failed: " +
                        barkFailure,
                        instance);
                }
            }

            EditorUtility.SetDirty(instance);
            MarkSceneDirty(instance);
            EditorGUIUtility.systemCopyBuffer = clipboardReport;
            SceneView.RepaintAll();
            if (result.Passed)
            {
                Debug.Log(
                    "[TREE-GEN.2A] Procedural tree structure regenerated; representative bark mesh was refreshed when applicable, and the report was copied to the clipboard.",
                    instance);
            }
            else
            {
                Debug.LogError(
                    "[TREE-GEN.2A] Procedural tree generation failed and its report was copied to the clipboard.",
                    instance);
            }
        }

        private static void MarkSceneDirty(ProceduralTreeInstance instance)
        {
            if (instance != null && instance.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(instance.gameObject.scene);
            }
        }
    }
}
