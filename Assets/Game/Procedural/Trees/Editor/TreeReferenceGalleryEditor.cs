using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [CustomEditor(typeof(TreeReferenceGallery))]
    public sealed class TreeReferenceGalleryEditor : UnityEditor.Editor
    {
        private static bool showAdvancedValidation;
        private static bool showMaintenance;
        private static bool showDiagnostics;

        private SerializedProperty referenceGround;
        private SerializedProperty sourceScale;
        private SerializedProperty alignToGround;
        private SerializedProperty familyRowSpacing;
        private SerializedProperty pairColumnSpacing;
        private SerializedProperty comparisonPairOffset;
        private SerializedProperty completeGalleryLeftClearance;
        private SerializedProperty completeGalleryRowGap;
        private SerializedProperty completeGalleryFamilyGap;
        private SerializedProperty completeGalleryPadMargin;
        private SerializedProperty completeGalleryPadThickness;
        private SerializedProperty completeGalleryPadColor;
        private SerializedProperty windEnabled;
        private SerializedProperty importedWindMaskMode;
        private SerializedProperty debugMode;
        private SerializedProperty foliageAlphaCutoff;
        private SerializedProperty foliageAlphaShadowCasting;
        private SerializedProperty foliageDebugMode;
        private SerializedProperty foliageCanopyDepthStrength;
        private SerializedProperty foliageCanopyDepthPower;
        private SerializedProperty foliageOrientationContrast;
        private SerializedProperty foliageOrientationReadability;
        private SerializedProperty foliageUndersideDarkening;
        private SerializedProperty foliageClusterVariationStrength;
        private SerializedProperty foliageClusterVariationScale;
        private SerializedProperty foliageDiffuseWrap;
        private SerializedProperty foliageShadowReceiveStrength;
        private SerializedProperty foliageShadowFloor;
        private SerializedProperty generationLibrary;
        private SerializedProperty showGeneratedStructuralPreviews;
        private SerializedProperty generatedPreviewScope;
        private SerializedProperty showGeneratedTrunk;
        private SerializedProperty showGeneratedPrimaryBranches;
        private SerializedProperty showGeneratedHigherOrderBranches;
        private SerializedProperty showGeneratedAttachmentPoints;
        private SerializedProperty showGeneratedBounds;
        private SerializedProperty showGeneratedTransportedFrames;

        private void OnEnable()
        {
            referenceGround = serializedObject.FindProperty("referenceGround");
            sourceScale = serializedObject.FindProperty("sourceScale");
            alignToGround = serializedObject.FindProperty("alignToGround");
            familyRowSpacing = serializedObject.FindProperty("familyRowSpacing");
            pairColumnSpacing = serializedObject.FindProperty("pairColumnSpacing");
            comparisonPairOffset =
                serializedObject.FindProperty("comparisonPairOffset");
            completeGalleryLeftClearance = serializedObject.FindProperty(
                "completeGalleryLeftClearance");
            completeGalleryRowGap = serializedObject.FindProperty(
                "completeGalleryRowGap");
            completeGalleryFamilyGap = serializedObject.FindProperty(
                "completeGalleryFamilyGap");
            completeGalleryPadMargin = serializedObject.FindProperty(
                "completeGalleryPadMargin");
            completeGalleryPadThickness = serializedObject.FindProperty(
                "completeGalleryPadThickness");
            completeGalleryPadColor = serializedObject.FindProperty(
                "completeGalleryPadColor");
            windEnabled = serializedObject.FindProperty("windEnabled");
            importedWindMaskMode =
                serializedObject.FindProperty("importedWindMaskMode");
            debugMode = serializedObject.FindProperty("debugMode");
            foliageAlphaCutoff =
                serializedObject.FindProperty("foliageAlphaCutoff");
            foliageAlphaShadowCasting =
                serializedObject.FindProperty("foliageAlphaShadowCasting");
            foliageDebugMode =
                serializedObject.FindProperty("foliageDebugMode");
            foliageCanopyDepthStrength = serializedObject.FindProperty(
                "foliageCanopyDepthStrength");
            foliageCanopyDepthPower = serializedObject.FindProperty(
                "foliageCanopyDepthPower");
            foliageOrientationContrast = serializedObject.FindProperty(
                "foliageOrientationContrast");
            foliageOrientationReadability = serializedObject.FindProperty(
                "foliageOrientationReadability");
            foliageUndersideDarkening = serializedObject.FindProperty(
                "foliageUndersideDarkening");
            foliageClusterVariationStrength = serializedObject.FindProperty(
                "foliageClusterVariationStrength");
            foliageClusterVariationScale = serializedObject.FindProperty(
                "foliageClusterVariationScale");
            foliageDiffuseWrap = serializedObject.FindProperty(
                "foliageDiffuseWrap");
            foliageShadowReceiveStrength = serializedObject.FindProperty(
                "foliageShadowReceiveStrength");
            foliageShadowFloor = serializedObject.FindProperty(
                "foliageShadowFloor");
            generationLibrary = serializedObject.FindProperty(
                "generationLibrary");
            showGeneratedStructuralPreviews = serializedObject.FindProperty(
                "showGeneratedStructuralPreviews");
            generatedPreviewScope = serializedObject.FindProperty(
                "generatedPreviewScope");
            showGeneratedTrunk = serializedObject.FindProperty(
                "showGeneratedTrunk");
            showGeneratedPrimaryBranches = serializedObject.FindProperty(
                "showGeneratedPrimaryBranches");
            showGeneratedHigherOrderBranches = serializedObject.FindProperty(
                "showGeneratedHigherOrderBranches");
            showGeneratedAttachmentPoints = serializedObject.FindProperty(
                "showGeneratedAttachmentPoints");
            showGeneratedBounds = serializedObject.FindProperty(
                "showGeneratedBounds");
            showGeneratedTransportedFrames = serializedObject.FindProperty(
                "showGeneratedTransportedFrames");
        }

        public override void OnInspectorGUI()
        {
            var gallery = (TreeReferenceGallery)target;
            serializedObject.UpdateIfRequiredOrScript();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
            }

            DrawOwnership(gallery);
            DrawLayout();
            DrawCompleteGalleryLayout();
            DrawRendering();
            EditorGUI.BeginChangeCheck();
            DrawGeneratedTreeLibrary();
            bool previewSettingsChanged = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            if (previewSettingsChanged)
            {
                Undo.RecordObject(gallery, "Change Generated Tree Preview Settings");
                gallery.ApplyGeneratedPreviewSettings();
                EditorUtility.SetDirty(gallery);
                if (gallery.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
                }
                SceneView.RepaintAll();
            }

            DrawActions(gallery);
            DrawDiagnostics(gallery);
            DrawStatus(gallery);
        }

        private void OnSceneGUI()
        {
            var gallery = (TreeReferenceGallery)target;
            DrawSpecimenLabels(
                gallery,
                gallery.transform.Find(
                    TreeReferenceGalleryBuilder.VerticalSliceRootName));
            DrawSpecimenLabels(
                gallery,
                gallery.transform.Find(
                    TreeReferenceGalleryBuilder.CompleteGalleryRootName));
        }

        private static void DrawSpecimenLabels(
            TreeReferenceGallery gallery,
            Transform contentRoot)
        {
            if (contentRoot == null)
            {
                return;
            }

            TreeReferenceSpecimen[] specimens =
                contentRoot.GetComponentsInChildren<TreeReferenceSpecimen>(
                    true);
            for (int index = 0; index < specimens.Length; index++)
            {
                TreeReferenceSpecimen specimen = specimens[index];
                if (specimen == null || !specimen.gameObject.activeInHierarchy)
                {
                    continue;
                }

                bool imported =
                    specimen.Role == TreeReferenceRole.ImportedReference;
                float labelHeight = imported
                    ? specimen.VisibleHeight * gallery.SourceScale + 0.6f
                    : 0.6f;
                Vector3 position =
                    specimen.ResolveComparisonRootWorldPosition() +
                    Vector3.up * labelHeight;
                ProceduralTreeInstance generated = imported
                    ? null
                    : specimen.GetComponent<ProceduralTreeInstance>();
                string role = imported ? "REF" : "PROC";
                string metrics = imported
                    ? $"H {specimen.VisibleHeight:F2} m | {specimen.TriangleCount} tris"
                    : generated != null && generated.HasGeneratedDefinition
                        ? $"{generated.GeneratedDefinition.Metrics.BranchCount} branches | {generated.GeneratedDefinition.StructuralFingerprint}"
                        : "Generated structure not built";
                Handles.Label(
                    position,
                    $"{role} | {specimen.Family} {specimen.SourceVariantIndex}\n{metrics}");
            }
        }

        private void DrawOwnership(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Independent Gallery Ownership",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The gallery is a standalone diagnostic object. Its Ground " +
                "reference is used for vertical-slice surface sampling and to " +
                "position the complete gallery outside the chunk; the gallery " +
                "must not be parented under GeneratedGround.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                referenceGround,
                new GUIContent("Reference Ground"));

            if (gallery.ReferenceGround == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign the Ground that will provide the vertical-slice surface and playable-domain boundary.",
                    MessageType.Warning);
            }
            else if (gallery.transform.IsChildOf(
                         gallery.ReferenceGround.transform))
            {
                EditorGUILayout.HelpBox(
                    "Invalid gallery hierarchy: move this object out from under " +
                    "the assigned Ground. Use Place as Ground Sibling below.",
                    MessageType.Error);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Assign Closest Ground"))
                {
                    if (TreeReferenceGalleryBuilder.AssignClosestGround(
                            gallery,
                            out string result))
                    {
                        Debug.Log("[TREE-GALLERY.2B] " + result, gallery);
                    }
                    else
                    {
                        Debug.LogWarning(
                            "[TREE-GALLERY.2B] " + result,
                            gallery);
                    }
                }

                using (new EditorGUI.DisabledScope(
                           gallery.ReferenceGround == null))
                {
                    if (GUILayout.Button("Place as Ground Sibling"))
                    {
                        if (TreeReferenceGalleryBuilder.PlaceBesideAssignedGround(
                                gallery,
                                out string result))
                        {
                            Debug.Log("[TREE-GALLERY.2B] " + result, gallery);
                        }
                        else
                        {
                            Debug.LogWarning(
                                "[TREE-GALLERY.2B] " + result,
                                gallery);
                        }
                    }
                }
            }
        }

        private void DrawLayout()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Gallery Layout",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceScale);
            EditorGUILayout.PropertyField(alignToGround);
            EditorGUILayout.PropertyField(familyRowSpacing);
            EditorGUILayout.PropertyField(pairColumnSpacing);
            EditorGUILayout.PropertyField(comparisonPairOffset);
        }

        private void DrawCompleteGalleryLayout()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Complete Imported Gallery",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(completeGalleryLeftClearance);
            EditorGUILayout.PropertyField(completeGalleryRowGap);
            EditorGUILayout.PropertyField(completeGalleryFamilyGap);
            EditorGUILayout.PropertyField(completeGalleryPadMargin);
            EditorGUILayout.PropertyField(completeGalleryPadThickness);
            EditorGUILayout.PropertyField(completeGalleryPadColor);
            EditorGUILayout.HelpBox(
                "TREE-GALLERY.3A builds all twenty references and twenty " +
                "procedural comparison slots outside the playable chunk to " +
                "the Ground's left. All four family blocks remain active and " +
                "are arranged sequentially in one inspection strip. Each block " +
                "owns a lightweight shadow-receiver pad; it does not duplicate " +
                "or depend on the production Ground mesh.",
                MessageType.Info);
        }

        private void DrawRendering()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Reference Rendering",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(windEnabled);
            EditorGUILayout.PropertyField(importedWindMaskMode);
            EditorGUILayout.PropertyField(debugMode);
            EditorGUILayout.PropertyField(foliageAlphaCutoff);
            EditorGUILayout.PropertyField(
                foliageAlphaShadowCasting,
                new GUIContent("Foliage Shadow Casting"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Foliage Readability",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(foliageCanopyDepthStrength);
            EditorGUILayout.PropertyField(foliageCanopyDepthPower);
            EditorGUILayout.PropertyField(foliageOrientationContrast);
            EditorGUILayout.PropertyField(foliageOrientationReadability);
            EditorGUILayout.PropertyField(foliageUndersideDarkening);
            EditorGUILayout.PropertyField(foliageClusterVariationStrength);
            EditorGUILayout.PropertyField(foliageClusterVariationScale);
            EditorGUILayout.PropertyField(foliageDiffuseWrap);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Foliage Shadow Reception",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(foliageShadowReceiveStrength);
            EditorGUILayout.PropertyField(foliageShadowFloor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Foliage Diagnostics",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(foliageDebugMode);
            EditorGUILayout.HelpBox(
                "Rebuild the vertical slice after changing these values. " +
                "Foliage Shadow Casting controls the alpha-clipped shadow " +
                "caster pass. Shadow Receive Strength and Shadow Floor soften " +
                "harsh realtime trunk/card self-shadowing without weakening " +
                "the authoritative cloud cookie. Imported cluster variation " +
                "uses an object-space fallback; generated trees will provide " +
                "explicit cluster metadata later.",
                MessageType.None);
        }

        private void DrawGeneratedTreeLibrary()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Generated Tree Library",
                EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    generationLibrary,
                    new GUIContent("Managed Library"));
            }

            EditorGUILayout.PropertyField(
                showGeneratedStructuralPreviews,
                new GUIContent("Show Structural Previews"));
            using (new EditorGUI.DisabledScope(
                       !showGeneratedStructuralPreviews.boolValue))
            {
                EditorGUILayout.PropertyField(
                    generatedPreviewScope,
                    new GUIContent("Preview Scope"));
                EditorGUILayout.PropertyField(
                    showGeneratedTrunk,
                    new GUIContent("Trunk"));
                EditorGUILayout.PropertyField(
                    showGeneratedPrimaryBranches,
                    new GUIContent("Primary Branches"));
                EditorGUILayout.PropertyField(
                    showGeneratedHigherOrderBranches,
                    new GUIContent("Higher-Order Branches"));
                EditorGUILayout.PropertyField(
                    showGeneratedAttachmentPoints,
                    new GUIContent("Attachment Points"));
                EditorGUILayout.PropertyField(
                    showGeneratedBounds,
                    new GUIContent("Bounds"));
                EditorGUILayout.PropertyField(
                    showGeneratedTransportedFrames,
                    new GUIContent("Transported Frames"));
            }

            EditorGUILayout.HelpBox(
                "Default preview scope is Selected Tree. Select a PROC_*_SLOT to compare one generated skeleton against its imported reference without drawing all twenty graphs at once.",
                MessageType.Info);
        }

        private static void DrawActions(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            if (GUILayout.Button("Rebuild Complete Tree Comparison Gallery"))
            {
                RecordUnifiedResult(
                    gallery,
                    TreeGalleryGenerationCoordinator.Rebuild(gallery));
            }

            bool hasGeneratedOutputs =
                TreeGalleryGenerationCoordinator.CountGeneratedInstances(
                    gallery) > 0;
            using (new EditorGUI.DisabledScope(!hasGeneratedOutputs))
            {
                if (GUILayout.Button("Remove Generated Tree Outputs"))
                {
                    RecordUnifiedResult(
                        gallery,
                        TreeGalleryGenerationCoordinator.RemoveGeneratedOutputs(
                            gallery));
                }
            }

            EditorGUILayout.HelpBox(
                "Normal workflow: use Rebuild Complete Tree Comparison Gallery. It performs source audit/repair, reference rebuild, library setup, slot binding, structural generation, and deterministic validation in one action.",
                MessageType.Info);

            showAdvancedValidation = EditorGUILayout.Foldout(
                showAdvancedValidation,
                "Advanced Validation",
                true);
            if (showAdvancedValidation)
            {
                bool canBuild = gallery.LastSourceAuditPassed &&
                    gallery.ReferenceGround != null;
                bool hasSlice =
                    TreeReferenceGalleryBuilder.HasVerticalSlice(gallery);
                using (new EditorGUI.DisabledScope(!canBuild))
                {
                    if (GUILayout.Button(
                            "Rebuild On-Map Four-Family Validation Slice"))
                    {
                        RecordBuildResult(
                            gallery,
                            TreeReferenceGalleryBuilder.BuildVerticalSlice(
                                gallery,
                                hasSlice));
                    }
                }

                using (new EditorGUI.DisabledScope(!hasSlice))
                {
                    if (GUILayout.Button("Remove On-Map Validation Slice"))
                    {
                        RecordBuildResult(
                            gallery,
                            TreeReferenceGalleryBuilder.RemoveVerticalSlice(
                                gallery));
                    }
                }
            }

            showMaintenance = EditorGUILayout.Foldout(
                showMaintenance,
                "Maintenance",
                true);
            if (showMaintenance)
            {
                bool repairRequired =
                    TreeReferenceGalleryBuilder.BarkNormalCorrectionsRequired(
                        out string repairSummary);
                using (new EditorGUI.DisabledScope(!repairRequired))
                {
                    if (GUILayout.Button("Repair Required Tree Source Imports"))
                    {
                        TreeGalleryBuildResult result =
                            TreeReferenceGalleryBuilder.ApplyBarkNormalCorrections();
                        EditorGUIUtility.systemCopyBuffer = result.Report;
                        if (result.Passed)
                        {
                            Debug.Log(
                                "[Tree Reference Gallery] Tree source import repair passed and the report was copied to the clipboard.",
                                gallery);
                        }
                        else
                        {
                            Debug.LogError(
                                "[Tree Reference Gallery] Tree source import repair failed and the report was copied to the clipboard.",
                                gallery);
                        }
                    }
                }

                EditorGUILayout.HelpBox(repairSummary, MessageType.None);

                bool hasCompleteGallery =
                    TreeReferenceGalleryBuilder.HasCompleteGallery(gallery);
                using (new EditorGUI.DisabledScope(!hasCompleteGallery))
                {
                    if (GUILayout.Button("Remove Entire Off-Map Comparison Gallery"))
                    {
                        RecordCompleteGalleryResult(
                            gallery,
                            TreeReferenceGalleryBuilder.RemoveCompleteGallery(
                                gallery));
                    }
                }
            }
        }

        private static void DrawDiagnostics(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            showDiagnostics = EditorGUILayout.Foldout(
                showDiagnostics,
                "Diagnostics And Reports",
                true);
            if (!showDiagnostics)
            {
                return;
            }

            if (GUILayout.Button("Run Complete Tree Source Audit"))
            {
                TreeSourceAuditResult result = TreeSourceAssetAudit.Run(gallery);
                Undo.RecordObject(gallery, "Run Complete Tree Source Audit");
                gallery.RecordSourceAudit(
                    result.Passed,
                    result.SourceFolderAvailable,
                    result.FoundModelCount,
                    result.FoundTextureCount,
                    result.Timestamp,
                    result.Report);
                EditorUtility.SetDirty(gallery);
                if (gallery.gameObject.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(
                        gallery.gameObject.scene);
                }

                EditorGUIUtility.systemCopyBuffer = result.Report;
                Debug.Log(
                    result.Passed
                        ? "[Tree Reference Gallery] Complete tree source audit passed and was copied to the clipboard."
                        : "[Tree Reference Gallery] Complete tree source audit failed and was copied to the clipboard.",
                    gallery);
            }

            using (new EditorGUI.DisabledScope(
                       !gallery.HasUnifiedGenerationReport))
            {
                if (GUILayout.Button("Copy Last Complete Comparison Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        gallery.LastUnifiedGenerationReport;
                }
            }

            using (new EditorGUI.DisabledScope(!gallery.HasSourceAuditReport))
            {
                if (GUILayout.Button("Copy Last Source Audit Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        gallery.LastSourceAuditReport;
                }
            }

            using (new EditorGUI.DisabledScope(!gallery.HasVerticalSliceReport))
            {
                if (GUILayout.Button("Copy Last On-Map Validation Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        gallery.LastVerticalSliceReport;
                }
            }
        }

        private static void DrawStatus(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Source root",
                TreeReferenceGallery.SourceRootPath);
            EditorGUILayout.LabelField(
                "Source folder available now",
                TreeSourceAssetAudit.SourceFolderExists ? "Yes" : "No");
            EditorGUILayout.LabelField(
                "Last source audit",
                gallery.HasSourceAuditReport
                    ? gallery.LastSourceAuditPassed ? "PASS" : "FAIL"
                    : "Not run");
            EditorGUILayout.LabelField(
                "Audit revision",
                gallery.SourceAuditRevision.ToString());
            EditorGUILayout.LabelField(
                "Audited models",
                gallery.LastAuditedModelCount + " / " +
                TreeReferenceGallery.RequiredModelCount);
            EditorGUILayout.LabelField(
                "Audited textures",
                gallery.LastAuditedTextureCount + " / " +
                TreeReferenceGallery.RequiredTextureCount);
            EditorGUILayout.LabelField(
                "Vertical slice present",
                TreeReferenceGalleryBuilder.HasVerticalSlice(gallery)
                    ? "Yes"
                    : "No");
            EditorGUILayout.LabelField(
                "Last vertical-slice action",
                gallery.HasVerticalSliceReport
                    ? gallery.LastVerticalSliceBuildPassed ? "PASS" : "FAIL"
                    : "Not run");
            EditorGUILayout.LabelField(
                "Vertical-slice revision",
                gallery.VerticalSliceRevision.ToString());
            EditorGUILayout.LabelField(
                "Recorded specimens/slots",
                gallery.LastVerticalSliceSpecimenCount.ToString());
            EditorGUILayout.LabelField(
                "Vertical-slice timestamp",
                string.IsNullOrEmpty(gallery.LastVerticalSliceTimestamp)
                    ? "Not run"
                    : gallery.LastVerticalSliceTimestamp);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Complete gallery present",
                TreeReferenceGalleryBuilder.HasCompleteGallery(gallery)
                    ? "Yes"
                    : "No");
            EditorGUILayout.LabelField(
                "Last complete-gallery action",
                gallery.HasCompleteGalleryReport
                    ? gallery.LastCompleteGalleryBuildPassed ? "PASS" : "FAIL"
                    : "Not run");
            EditorGUILayout.LabelField(
                "Complete-gallery revision",
                gallery.CompleteGalleryRevision.ToString());
            EditorGUILayout.LabelField(
                "Complete gallery specimens/slots",
                gallery.LastCompleteGallerySpecimenCount.ToString());
            EditorGUILayout.LabelField(
                "Complete-gallery timestamp",
                string.IsNullOrEmpty(gallery.LastCompleteGalleryTimestamp)
                    ? "Not run"
                    : gallery.LastCompleteGalleryTimestamp);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Managed generation library",
                gallery.GenerationLibrary != null
                    ? gallery.GenerationLibrary.name
                    : "Not created");
            EditorGUILayout.LabelField(
                "Generated procedural instances",
                TreeGalleryGenerationCoordinator.CountGeneratedInstances(
                    gallery).ToString() + " / 20");
            EditorGUILayout.LabelField(
                "Last unified comparison build",
                gallery.HasUnifiedGenerationReport
                    ? gallery.LastUnifiedGenerationPassed ? "PASS" : "FAIL"
                    : "Not run");
            EditorGUILayout.LabelField(
                "Unified build revision",
                gallery.UnifiedGenerationRevision.ToString());
            EditorGUILayout.LabelField(
                "Generated structures recorded",
                gallery.LastGeneratedTreeCount.ToString());
            EditorGUILayout.LabelField(
                "Unified build timestamp",
                string.IsNullOrEmpty(gallery.LastUnifiedGenerationTimestamp)
                    ? "Not run"
                    : gallery.LastUnifiedGenerationTimestamp);
        }

        private static void RecordUnifiedResult(
            TreeReferenceGallery gallery,
            TreeUnifiedGalleryBuildResult result)
        {
            Undo.RecordObject(
                gallery,
                "Record Unified Tree Comparison Result");
            gallery.RecordUnifiedGenerationBuild(
                result.Passed,
                result.GeneratedTreeCount,
                result.Timestamp,
                result.Report);
            EditorUtility.SetDirty(gallery);
            if (gallery.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
            }

            EditorGUIUtility.systemCopyBuffer = result.Report;
            SceneView.RepaintAll();
            if (result.Passed)
            {
                Debug.Log(
                    "[Tree Reference Gallery] Unified comparison-gallery action passed and the complete report was copied to the clipboard.",
                    gallery);
            }
            else
            {
                Debug.LogError(
                    "[Tree Reference Gallery] Unified comparison-gallery action failed and the complete report was copied to the clipboard.",
                    gallery);
            }
        }

        private static void RecordCompleteGalleryResult(
            TreeReferenceGallery gallery,
            TreeGalleryBuildResult result)
        {
            Undo.RecordObject(
                gallery,
                "Record Complete Tree Gallery Result");
            gallery.RecordCompleteGalleryBuild(
                result.Passed,
                result.SpecimenCount,
                result.Timestamp,
                result.Report);
            EditorUtility.SetDirty(gallery);
            if (gallery.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
            }

            EditorGUIUtility.systemCopyBuffer = result.Report;
            SceneView.RepaintAll();
            if (result.Passed)
            {
                Debug.Log(
                    "[TREE-GALLERY.3A] Complete-gallery action passed and the " +
                    "complete report was copied to the clipboard.",
                    gallery);
            }
            else
            {
                Debug.LogError(
                    "[TREE-GALLERY.3A] Complete-gallery action failed and the " +
                    "complete report was copied to the clipboard.",
                    gallery);
            }
        }

        private static void RecordBuildResult(
            TreeReferenceGallery gallery,
            TreeGalleryBuildResult result)
        {
            Undo.RecordObject(gallery, "Record Tree Vertical Slice Result");
            gallery.RecordVerticalSliceBuild(
                result.Passed,
                result.SpecimenCount,
                result.Timestamp,
                result.Report);
            EditorUtility.SetDirty(gallery);
            if (gallery.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gallery.gameObject.scene);
            }

            EditorGUIUtility.systemCopyBuffer = result.Report;
            SceneView.RepaintAll();
            if (result.Passed)
            {
                Debug.Log(
                    "[TREE-GALLERY.2B] Vertical-slice action passed and the " +
                    "complete report was copied to the clipboard.",
                    gallery);
            }
            else
            {
                Debug.LogError(
                    "[TREE-GALLERY.2B] Vertical-slice action failed and the " +
                    "complete report was copied to the clipboard.",
                    gallery);
            }
        }
    }
}
