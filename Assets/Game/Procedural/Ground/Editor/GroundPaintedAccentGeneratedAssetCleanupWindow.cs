using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal sealed class GroundPaintedAccentGeneratedAssetCleanupWindow :
        EditorWindow
    {
        private Vector2 reportScroll;
        private Vector2 confirmationScroll;
        private GroundPaintedAccentGeneratedAssetAuditReport report;
        private bool confirmingDeletion;

        [MenuItem(
            "Tools/Generated Ground/Audit and Clean Painted Accent Assets...")]
        private static void OpenFromMenu()
        {
            OpenAndAudit();
        }

        public static void OpenAndAudit()
        {
            GroundPaintedAccentGeneratedAssetCleanupWindow window =
                GetWindow<GroundPaintedAccentGeneratedAssetCleanupWindow>();
            window.titleContent =
                new GUIContent("Painted Accent Cleanup");
            window.minSize = new Vector2(720f, 480f);
            window.Show();
            window.Focus();
            window.RunAudit();
        }

        public static void OpenAndPrepareDeletion()
        {
            GroundPaintedAccentGeneratedAssetCleanupWindow window =
                GetWindow<GroundPaintedAccentGeneratedAssetCleanupWindow>();
            window.titleContent =
                new GUIContent("Painted Accent Cleanup");
            window.minSize = new Vector2(720f, 480f);
            window.Show();
            window.Focus();
            window.RunAudit();
            window.confirmingDeletion =
                window.report != null &&
                window.report.CanDeleteConfirmedOrphans;
            window.Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(
                "Generated Painted Accent Asset Audit and Cleanup",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "The audit scans all project scenes and project-asset dependencies. Only managed outputs with no Ground owner and no project reference are classified as confirmed orphans. Scenes and prefabs are never saved or modified by this tool.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Audit", GUILayout.Width(120f)))
                {
                    RunAudit();
                }

                using (new EditorGUI.DisabledScope(report == null))
                {
                    if (GUILayout.Button(
                            "Copy Report",
                            GUILayout.Width(120f)))
                    {
                        GroundPaintedAccentGeneratedAssetCleanup
                            .CopyLastReport();
                    }
                }

                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledScope(
                           report == null ||
                           !report.CanDeleteConfirmedOrphans))
                {
                    if (GUILayout.Button(
                            "Delete Confirmed Orphans",
                            GUILayout.Width(190f)))
                    {
                        confirmingDeletion = true;
                    }
                }
            }

            EditorGUILayout.Space(4f);
            if (report == null)
            {
                EditorGUILayout.HelpBox(
                    "Run the audit to inspect generated outputs.",
                    MessageType.None);
                return;
            }

            if (confirmingDeletion)
            {
                DrawDeletionConfirmation();
                return;
            }

            reportScroll = EditorGUILayout.BeginScrollView(reportScroll);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    report.BuildReport(),
                    GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawDeletionConfirmation()
        {
            List<string> orphanPaths =
                report.GetConfirmedOrphanPaths();
            EditorGUILayout.HelpBox(
                "Review every exact path below. Deletion performs a fresh audit and proceeds only when the confirmed-orphan set is unchanged and no loaded scene or asset has unsaved changes.",
                MessageType.Warning);

            confirmationScroll =
                EditorGUILayout.BeginScrollView(confirmationScroll);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(
                    string.Join("\n", orphanPaths),
                    GUILayout.ExpandHeight(true));
            }
            EditorGUILayout.EndScrollView();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Cancel"))
                {
                    confirmingDeletion = false;
                }

                using (new EditorGUI.DisabledScope(
                           !report.CanDeleteConfirmedOrphans))
                {
                    if (GUILayout.Button(
                            $"Confirm Delete {orphanPaths.Count} Asset(s)"))
                    {
                        bool deleted =
                            GroundPaintedAccentGeneratedAssetCleanup
                                .TryDeleteConfirmedOrphans(
                                    report,
                                    out GroundPaintedAccentGeneratedAssetAuditReport
                                        refreshedReport,
                                    out string resultMessage);
                        report = refreshedReport;
                        confirmingDeletion = false;
                        EditorUtility.DisplayDialog(
                            deleted
                                ? "Painted Accent Cleanup Complete"
                                : "Painted Accent Cleanup Stopped",
                            resultMessage,
                            "OK");
                    }
                }
            }
        }

        private void RunAudit()
        {
            confirmingDeletion = false;
            report =
                GroundPaintedAccentGeneratedAssetCleanup.RunAudit();
            Repaint();
        }
    }
}
