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
            if (selectedInstance == null && selected != null)
            {
                TreeRecipeSpawner spawner =
                    selected.GetComponentInParent<TreeRecipeSpawner>();
                selectedInstance = spawner != null
                    ? spawner.GeneratedInstance
                    : null;
            }
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
        private SerializedProperty recipe;
        private SerializedProperty masterSeed;
        private SerializedProperty instanceOverrides;
        private SerializedProperty exactControls;
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
            recipe = serializedObject.FindProperty("recipe");
            masterSeed = serializedObject.FindProperty("masterSeed");
            instanceOverrides = serializedObject.FindProperty(
                "instanceOverrides");
            exactControls = serializedObject.FindProperty("exactControls");
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
            }

            DrawRecipeOnlyFoundation(instance);
            serializedObject.UpdateIfRequiredOrScript();
            TreeResolvedControlsDrawer.Draw(exactControls, target);
            DrawLegacyCompatibility(instance);

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
                "Preview scope and visibility are managed from Blockout > Tree Reference Gallery > Curated Recipe Generation.",
                MessageType.None);

            serializedObject.ApplyModifiedProperties();
            DrawActions(instance);
            DrawStatus(instance);
        }

        private void DrawRecipeOnlyFoundation(
            ProceduralTreeInstance instance)
        {
            EditorGUILayout.LabelField(
                "Recipe-Only Exact Generation",
                EditorStyles.boldLabel);
            TreeGenerationRecipe nextRecipe =
                (TreeGenerationRecipe)EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Source Recipe",
                        "Standalone source recipe. The exact controls below are a snapshot sampled from its intervals."),
                    instance.Recipe,
                    typeof(TreeGenerationRecipe),
                    false);
            if (nextRecipe != instance.Recipe)
            {
                Undo.RecordObject(instance, "Assign Standalone Tree Recipe");
                instance.ConfigureStandaloneRecipe(nextRecipe, true);
                EditorUtility.SetDirty(instance);
                MarkSceneDirty(instance);
                serializedObject.UpdateIfRequiredOrScript();
            }

            EditorGUILayout.PropertyField(
                masterSeed,
                new GUIContent(
                    "Master Seed",
                    "Samples every recipe-only control independently through its permanent stable control ID."));
            serializedObject.ApplyModifiedProperties();

            using (new EditorGUI.DisabledScope(instance.Recipe == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Reapply Recipe To Exact Controls"))
                {
                    Undo.RecordObject(instance, "Reapply Tree Recipe Controls");
                    instance.SampleExactControlsFromRecipe();
                    EditorUtility.SetDirty(instance);
                    MarkSceneDirty(instance);
                    serializedObject.UpdateIfRequiredOrScript();
                }

                if (GUILayout.Button("Randomize Seed And Reapply"))
                {
                    int seed = TreeDeterministicUtility.DeriveSeed(
                        DateTime.UtcNow.Ticks,
                        instance.GetEntityId(),
                        instance.StableSlotIdentity,
                        "recipe-only-controls");
                    Undo.RecordObject(instance, "Randomize Tree Recipe Controls");
                    instance.SetMasterSeed(seed);
                    instance.SampleExactControlsFromRecipe();
                    EditorUtility.SetDirty(instance);
                    MarkSceneDirty(instance);
                    serializedObject.UpdateIfRequiredOrScript();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.HelpBox(
                "Standalone recipes sample into this exact snapshot. Regenerate From Exact Controls changes this tree only; recipe edits never silently mutate existing spawned trees.",
                MessageType.Info);
        }

        private void DrawLegacyCompatibility(
            ProceduralTreeInstance instance)
        {
            string sessionKey =
                "PS3D.TreeControls.LegacyInstance." +
                target.GetEntityId();
            bool expanded = SessionState.GetBool(sessionKey, false);
            bool nextExpanded = EditorGUILayout.Foldout(
                expanded,
                "Legacy Compatibility — Temporary",
                true,
                EditorStyles.foldoutHeader);
            if (nextExpanded != expanded)
            {
                SessionState.SetBool(sessionKey, nextExpanded);
            }

            if (!nextExpanded)
            {
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    "These bindings and overrides are retained only for explicit legacy compatibility evidence. Curated recipes and spawned gallery children use the exact controls above and perform zero behavioral family/calibration reads.",
                    EditorStyles.textArea,
                    GUILayout.MinHeight(42f));
                EditorGUILayout.PropertyField(library);
                EditorGUILayout.PropertyField(family);
                EditorGUILayout.PropertyField(sourceVariantIndex);
                EditorGUILayout.PropertyField(recipe);
            }

            DrawRecipeSelector(instance);
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(instanceOverrides, true);
            EditorGUILayout.EndVertical();
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
            using (new EditorGUI.DisabledScope(!instance.HasExactControls))
            {
                if (GUILayout.Button("Regenerate From Exact Controls"))
                {
                    Generate(instance, useExactSnapshot: true);
                }
            }

            using (new EditorGUI.DisabledScope(instance.Recipe == null))
            {
                if (GUILayout.Button("Apply Recipe With Current Seed"))
                {
                    Undo.RecordObject(instance, "Apply Tree Recipe");
                    instance.SampleExactControlsFromRecipe();
                    Generate(instance, useExactSnapshot: true);
                }

                if (GUILayout.Button("Randomize Seed And Apply Recipe"))
                {
                    int seed = TreeDeterministicUtility.DeriveSeed(
                        DateTime.UtcNow.Ticks,
                        instance.GetEntityId(),
                        instance.StableSlotIdentity,
                        "recipe-only-generation");
                    Undo.RecordObject(instance, "Randomize Tree Recipe Seed");
                    instance.SetMasterSeed(seed);
                    instance.SampleExactControlsFromRecipe();
                    Generate(instance, useExactSnapshot: true);
                }
            }

            bool legacyGenerationAvailable =
                instance.Recipe != null &&
                instance.Recipe.FamilyProfile != null;
            if (legacyGenerationAvailable)
            {
                string sessionKey =
                    "PS3D.TreeControls.LegacyGenerationActions." +
                    instance.GetEntityId();
                bool expanded = SessionState.GetBool(sessionKey, false);
                bool next = EditorGUILayout.Foldout(
                    expanded,
                    "Legacy Compatibility Actions",
                    true,
                    EditorStyles.foldoutHeader);
                if (next != expanded)
                {
                    SessionState.SetBool(sessionKey, next);
                }

                if (next)
                {
                    if (GUILayout.Button("Regenerate Through Legacy Path"))
                    {
                        Generate(instance, useExactSnapshot: false);
                    }

                    if (GUILayout.Button("Reset Legacy Overrides"))
                    {
                        Undo.RecordObject(
                            instance,
                            "Reset Procedural Tree Overrides");
                        instance.ResetInstanceOverrides();
                        EditorUtility.SetDirty(instance);
                        MarkSceneDirty(instance);
                    }
                }
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Exhaustive Control Validation",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs all 41 controls at baseline, low, neutral and high across available Alder, Norway Spruce, Wych Elm and Dead gallery representatives. The suite advances one bounded case per Editor update, checkpoints TXT/CSV output, reports ETA and remains cancellable.",
                MessageType.None);
            if (TreeControlResponseSuite.IsRunning)
            {
                Rect progressRect = GUILayoutUtility.GetRect(
                    10f,
                    18f,
                    GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(
                    progressRect,
                    TreeControlResponseSuite.CurrentProgress,
                    TreeControlResponseSuite.ProgressLabel);
                EditorGUILayout.LabelField(
                    "Current",
                    TreeControlResponseSuite.CurrentDetail);
                EditorGUILayout.LabelField(
                    "Timing",
                    TreeControlResponseSuite.CurrentEta);
                if (GUILayout.Button("Cancel 41-Control Response Suite"))
                {
                    TreeControlResponseSuite.RequestCancel();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(
                    TreeGeometryEfficiencyAudit.IsRunning ||
                    TreeRootCollapseTournament.IsRunning))
                {
                    if (GUILayout.Button("Run 41-Control Response Suite"))
                    {
                        TreeControlResponseSuite.Start(instance);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(
                    TreeControlResponseSuite.LastReportPath)))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Control Response Report"))
                {
                    TreeControlResponseSuite.CopyLastReport();
                }
                if (GUILayout.Button("Open Control Response Folder"))
                {
                    TreeControlResponseSuite.OpenOutputFolder();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Procedural Tree Geometry Efficiency Audit",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs all twenty procedural comparison-gallery trees through Production Current, Legacy Pre-Patch-1, and Radial Aggressive. Production Current is the accepted Patch 1 axial plus contour-owned radial representation. The audit records topology, exact production mesh parity, geometry reductions, mixed-resolution stitching, radius-aware branch counts, and fixed-camera silhouettes without committing candidates.",
                MessageType.None);
            if (TreeGeometryEfficiencyAudit.IsRunning)
            {
                Rect geometryProgressRect = GUILayoutUtility.GetRect(
                    10f,
                    18f,
                    GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(
                    geometryProgressRect,
                    TreeGeometryEfficiencyAudit.CurrentProgress,
                    TreeGeometryEfficiencyAudit.ProgressLabel);
                EditorGUILayout.LabelField(
                    "Current",
                    TreeGeometryEfficiencyAudit.CurrentDetail);
                EditorGUILayout.LabelField(
                    "Timing",
                    TreeGeometryEfficiencyAudit.CurrentEta);
                if (GUILayout.Button("Cancel Geometry Efficiency Audit"))
                {
                    TreeGeometryEfficiencyAudit.RequestCancel();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(
                    TreeControlResponseSuite.IsRunning ||
                    TreeRootCollapseTournament.IsRunning))
                {
                    if (GUILayout.Button("Run Geometry Efficiency Audit"))
                    {
                        TreeGeometryEfficiencyAudit.Start(instance);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(
                    TreeGeometryEfficiencyAudit.LastReportPath)))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Geometry Audit Report"))
                {
                    TreeGeometryEfficiencyAudit.CopyLastReport();
                }
                if (GUILayout.Button("Open Geometry Audit Folder"))
                {
                    TreeGeometryEfficiencyAudit.OpenOutputFolder();
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Wych Elm Root-Frame Strategy Tournament",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Runs 60 Wych Elm bark builds across Root Height 0.030, 0.050, and 0.100; four Reach/Thickness profiles; and five root-frame strategies. The suite advances one case per Editor update, remains cancellable, checkpoints TXT/CSV output, and ranks only complete topology winners. The unsafe preview builds the rejected production Root Height 0.050 mesh as a temporary unsaved Scene object for visual inspection.",
                MessageType.None);
            if (TreeRootCollapseTournament.IsRunning)
            {
                Rect tournamentProgressRect = GUILayoutUtility.GetRect(
                    10f,
                    18f,
                    GUILayout.ExpandWidth(true));
                EditorGUI.ProgressBar(
                    tournamentProgressRect,
                    TreeRootCollapseTournament.CurrentProgress,
                    TreeRootCollapseTournament.ProgressLabel);
                EditorGUILayout.LabelField(
                    "Current",
                    TreeRootCollapseTournament.CurrentDetail);
                EditorGUILayout.LabelField(
                    "Timing",
                    TreeRootCollapseTournament.CurrentEta);
                if (GUILayout.Button("Cancel Root-Frame Tournament"))
                {
                    TreeRootCollapseTournament.RequestCancel();
                }
            }
            else
            {
                using (new EditorGUI.DisabledScope(
                    TreeControlResponseSuite.IsRunning ||
                    TreeGeometryEfficiencyAudit.IsRunning))
                {
                    if (GUILayout.Button("Run Root-Frame Tournament"))
                    {
                        TreeRootCollapseTournament.Start(instance);
                    }
                    if (GUILayout.Button("Build Unsafe Wych Failure Preview"))
                    {
                        TreeRootCollapseTournament.BuildUnsafeVisualPreview(instance);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(
                string.IsNullOrEmpty(
                    TreeRootCollapseTournament.LastReportPath)))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Tournament Report"))
                {
                    TreeRootCollapseTournament.CopyLastReport();
                }
                if (GUILayout.Button("Open Tournament Folder"))
                {
                    TreeRootCollapseTournament.OpenOutputFolder();
                }
                EditorGUILayout.EndHorizontal();
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
                    ? instance.Recipe.RecipeDisplayName
                    : "Unassigned");
            EditorGUILayout.LabelField(
                "Generation path",
                instance.UsesRecipeOnlyGeneration
                    ? "Recipe-only exact controls"
                    : "Legacy compatibility");
            EditorGUILayout.LabelField(
                "Exact control snapshot",
                instance.HasExactControls ? "Available" : "None");
            EditorGUILayout.LabelField(
                "Snapshot source",
                string.IsNullOrEmpty(instance.ExactControlsSourceRecipeIdentity)
                    ? "Unassigned"
                    : instance.ExactControlsSourceRecipeIdentity);
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
                if (!instance.UsesRecipeOnlyGeneration)
                {
                    EditorGUILayout.LabelField(
                        "Reference ratio H/W/D",
                        instance.GeneratedDefinition.Metrics.CalibrationHeightRatio.ToString("F3") +
                        " / " +
                        instance.GeneratedDefinition.Metrics.CalibrationWidthRatio.ToString("F3") +
                        " / " +
                        instance.GeneratedDefinition.Metrics.CalibrationDepthRatio.ToString("F3"));
                }
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
                    : "Not built or stale");
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

        private static void Generate(
            ProceduralTreeInstance instance,
            bool useExactSnapshot)
        {
            Undo.RecordObject(instance, "Generate Procedural Tree Structure");
            TreeGenerationResult result = useExactSnapshot
                ? instance.RegenerateFromExactControls()
                : instance.GenerateStructure();
            string clipboardReport = result.Report;
            if (result.Passed)
            {
                TreeReferenceGallery gallery =
                    instance.GetComponentInParent<TreeReferenceGallery>();
                if (gallery == null)
                {
                    const string barkFailure =
                        "The procedural tree instance is not parented beneath a Tree Reference Gallery.";
                    clipboardReport +=
                        "\n\n[TREE-CONTROLS.4 Bark Mesh]\nStatus: FAIL\n" +
                        barkFailure;
                    Debug.LogError(
                        "[TREE-CONTROLS.4] Bark mesh regeneration failed: " +
                        barkFailure,
                        instance);
                }
                else if (instance.UsesRecipeOnlyGeneration &&
                         gallery.GenerationLibrary == null)
                {
                    const string barkFailure =
                        "The gallery bark-mesh storage library is not assigned.";
                    clipboardReport +=
                        "\n\n[TREE-CONTROLS.4 Bark Mesh]\nStatus: FAIL\n" +
                        barkFailure;
                    Debug.LogError(
                        "[TREE-CONTROLS.4] Bark mesh regeneration failed: " +
                        barkFailure,
                        instance);
                }
                else if (TreeBarkMeshAssetBuilder.BuildOrUpdate(
                             gallery,
                             instance.UsesRecipeOnlyGeneration
                                 ? gallery.GenerationLibrary
                                 : instance.Library,
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
                        "\n\n[TREE-CONTROLS.4 Bark Mesh]\nStatus: FAIL\n" +
                        barkFailure;
                    Debug.LogError(
                        "[TREE-CONTROLS.4] Bark mesh regeneration failed: " +
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
                    "[TREE-CONTROLS.4] Procedural tree structure regenerated; representative bark mesh was refreshed when applicable, and the report was copied to the clipboard.",
                    instance);
            }
            else
            {
                Debug.LogError(
                    "[TREE-CONTROLS.4] Procedural tree generation failed and its report was copied to the clipboard.",
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
