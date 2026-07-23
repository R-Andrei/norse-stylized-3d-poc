using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees.Editor
{
    [CustomEditor(typeof(TreeReferenceGallery))]
    public sealed class TreeReferenceGalleryEditor : UnityEditor.Editor
    {
        private SerializedProperty referenceGround;
        private SerializedProperty sourceScale;
        private SerializedProperty alignToGround;
        private SerializedProperty familyRowSpacing;
        private SerializedProperty pairColumnSpacing;
        private SerializedProperty comparisonPairOffset;
        private SerializedProperty windEnabled;
        private SerializedProperty foliageShadowCasting;

        private void OnEnable()
        {
            referenceGround = serializedObject.FindProperty("referenceGround");
            sourceScale = serializedObject.FindProperty("sourceScale");
            alignToGround = serializedObject.FindProperty("alignToGround");
            familyRowSpacing = serializedObject.FindProperty("familyRowSpacing");
            pairColumnSpacing = serializedObject.FindProperty("pairColumnSpacing");
            comparisonPairOffset =
                serializedObject.FindProperty("comparisonPairOffset");
            windEnabled = serializedObject.FindProperty("windEnabled");
            foliageShadowCasting =
                serializedObject.FindProperty("foliageShadowCasting");
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
            DrawLayoutFoundation();
            DrawRenderingFoundation();
            serializedObject.ApplyModifiedProperties();
            DrawActions(gallery);
            DrawDiagnostics(gallery);
            DrawStatus(gallery);
        }

        private void DrawOwnership(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Independent Gallery Ownership",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The gallery is a standalone diagnostic object. Its Ground " +
                "reference is used only for later surface sampling; the gallery " +
                "must not be parented under GeneratedGround.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                referenceGround,
                new GUIContent("Reference Ground"));

            if (gallery.ReferenceGround == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign the Ground that will provide the comparison surface.",
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
                        Debug.Log("[TREE-GALLERY.1] " + result, gallery);
                    }
                    else
                    {
                        Debug.LogWarning("[TREE-GALLERY.1] " + result, gallery);
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
                            Debug.Log("[TREE-GALLERY.1] " + result, gallery);
                        }
                        else
                        {
                            Debug.LogWarning(
                                "[TREE-GALLERY.1] " + result,
                                gallery);
                        }
                    }
                }
            }
        }

        private void DrawLayoutFoundation()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Gallery Layout Foundation",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sourceScale);
            EditorGUILayout.PropertyField(alignToGround);
            EditorGUILayout.PropertyField(familyRowSpacing);
            EditorGUILayout.PropertyField(pairColumnSpacing);
            EditorGUILayout.PropertyField(comparisonPairOffset);
        }

        private void DrawRenderingFoundation()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Reference Rendering Foundation",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(windEnabled);
            EditorGUILayout.PropertyField(foliageShadowCasting);
            EditorGUILayout.HelpBox(
                "These values reserve the accepted gallery contract. Tree " +
                "materials and rendered specimens are implemented in later patches.",
                MessageType.None);
        }

        private static void DrawActions(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                GUILayout.Button("Build Complete Imported Gallery");
                GUILayout.Button("Rebuild Complete Imported Gallery");
                GUILayout.Button("Remove Imported Gallery Children");
            }

            EditorGUILayout.HelpBox(
                gallery.LastSourceAuditPassed
                    ? "The source audit passed. Gallery construction remains " +
                      "disabled until TREE-GALLERY.3 implements the deterministic builder."
                    : "Gallery construction is unavailable until the complete " +
                      "source audit passes and TREE-GALLERY.3 is implemented.",
                MessageType.Info);
        }

        private static void DrawDiagnostics(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Diagnostics", EditorStyles.boldLabel);

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

                if (result.Passed)
                {
                    Debug.Log(
                        "[TREE-GALLERY.1] Complete tree source audit passed and " +
                        "was copied to the clipboard.",
                        gallery);
                }
                else
                {
                    Debug.LogError(
                        "[TREE-GALLERY.1] Complete tree source audit failed and " +
                        "was copied to the clipboard. See the complete report.",
                        gallery);
                }
            }

            using (new EditorGUI.DisabledScope(!gallery.HasSourceAuditReport))
            {
                if (GUILayout.Button("Copy Last Tree Source Audit"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        gallery.LastSourceAuditReport;
                    Debug.Log(
                        "[TREE-GALLERY.1] Last tree source audit copied to the clipboard.",
                        gallery);
                }
            }
        }

        private static void DrawStatus(TreeReferenceGallery gallery)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audit Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Source root",
                TreeReferenceGallery.SourceRootPath);
            EditorGUILayout.LabelField(
                "Source folder available now",
                TreeSourceAssetAudit.SourceFolderExists ? "Yes" : "No");
            EditorGUILayout.LabelField(
                "Available at last audit",
                gallery.SourceFolderAvailable ? "Yes" : "No");
            EditorGUILayout.LabelField(
                "Last audit",
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
                "Timestamp",
                string.IsNullOrEmpty(gallery.LastSourceAuditTimestamp)
                    ? "Not run"
                    : gallery.LastSourceAuditTimestamp);
        }
    }
}
