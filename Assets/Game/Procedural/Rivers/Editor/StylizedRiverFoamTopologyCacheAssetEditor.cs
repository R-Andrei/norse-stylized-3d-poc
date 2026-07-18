using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiverFoamTopologyCacheAsset))]
    [CanEditMultipleObjects]
    internal sealed class StylizedRiverFoamTopologyCacheAssetEditor :
        UnityEditor.Editor
    {
        private bool showPayloadSections;
        private bool payloadAnalyzed;
        private bool payloadDiagnosticAvailable;
        private string analyzedPayloadHash = string.Empty;
        private string payloadDiagnosticReport = string.Empty;
        private string payloadDiagnosticError = string.Empty;

        public override void OnInspectorGUI()
        {
            if (targets.Length != 1 ||
                target is not StylizedRiverFoamTopologyCacheAsset asset)
            {
                EditorGUILayout.HelpBox(
                    "Select one River Foam topology cache asset to inspect its " +
                    "read-only contract metadata.",
                    MessageType.Info);
                return;
            }

            InvalidatePayloadAnalysisWhenAssetChanges(asset);
            EditorGUILayout.HelpBox(
                "Read-only cache metadata. This Inspector never edits payload " +
                "bytes or cache fields. Rebuild the asset through the owning " +
                "River's Foam Cache & Validation actions.",
                MessageType.None);

            EditorGUILayout.LabelField(
                "Storage and Coordinate Contract",
                EditorStyles.boldLabel);
            DrawReadOnly("Storage Contract Version",
                asset.StorageContractVersionValue.ToString());
            DrawReadOnly("Payload Format Version",
                asset.PayloadFormatVersion.ToString());
            DrawReadOnly("Generator Contract Version",
                asset.GeneratorContractVersion.ToString());
            DrawReadOnly("Grid Descriptor Contract Version",
                asset.GridDescriptorContractVersion.ToString());
            DrawReadOnly("Grid Mapping",
                $"{asset.GridMappingValue} — {asset.GridMappingDisplayName}");
            DrawReadOnly("Grid Mapping Contract Version",
                asset.GridMappingContractVersion.ToString());
            DrawReadOnly("Grid Initialization Signature",
                EmptyAsDash(asset.GridInitializationSignature));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Build and Payload",
                EditorStyles.boldLabel);
            DrawReadOnly("Source River", EmptyAsDash(asset.SourceRiverName));
            DrawReadOnly("Built UTC", EmptyAsDash(asset.BuiltUtc));
            DrawReadOnly("Payload Bytes", asset.PayloadByteCount.ToString("N0"));
            DrawReadOnly("Payload Hash", EmptyAsDash(asset.PayloadHash));

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Stable Input Fingerprints",
                EditorStyles.boldLabel);
            DrawReadOnly("Domain", EmptyAsDash(asset.DomainFingerprint));
            DrawReadOnly("Obstacles", EmptyAsDash(asset.ObstacleFingerprint));
            DrawReadOnly("Generation", EmptyAsDash(asset.GenerationFingerprint));
            DrawReadOnly("Combined", EmptyAsDash(asset.CombinedFingerprint));

            string metadataReport = BuildMetadataReport(asset);
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Explicit Payload Diagnostics",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Payload section decoding is intentionally explicit because a " +
                "cache may contain several megabytes of topology data. It is " +
                "never performed automatically during Inspector repaint.",
                MessageType.None);
            using (new EditorGUI.DisabledScope(!asset.HasPayload))
            {
                if (GUILayout.Button("Analyze Payload Sections"))
                {
                    payloadDiagnosticAvailable =
                        asset.TryGetPayloadDiagnosticReport(
                            out payloadDiagnosticReport,
                            out payloadDiagnosticError);
                    analyzedPayloadHash = asset.PayloadHash ?? string.Empty;
                    payloadAnalyzed = true;
                    showPayloadSections = true;
                }
            }

            if (payloadAnalyzed)
            {
                showPayloadSections = EditorGUILayout.Foldout(
                    showPayloadSections,
                    "Payload Section Digests",
                    true);
                if (showPayloadSections)
                {
                    EditorGUILayout.HelpBox(
                        payloadDiagnosticAvailable
                            ? payloadDiagnosticReport
                            : payloadDiagnosticError,
                        payloadDiagnosticAvailable
                            ? MessageType.None
                            : asset.HasPayload
                                ? MessageType.Error
                                : MessageType.Warning);
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Cache Metadata"))
            {
                EditorGUIUtility.systemCopyBuffer = metadataReport;
            }
            using (new EditorGUI.DisabledScope(
                       !payloadAnalyzed || !payloadDiagnosticAvailable))
            {
                if (GUILayout.Button("Copy Metadata + Sections"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        metadataReport + "\n\n" + payloadDiagnosticReport;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void InvalidatePayloadAnalysisWhenAssetChanges(
            StylizedRiverFoamTopologyCacheAsset asset)
        {
            if (!payloadAnalyzed ||
                string.Equals(
                    analyzedPayloadHash,
                    asset.PayloadHash ?? string.Empty,
                    System.StringComparison.Ordinal))
            {
                return;
            }

            payloadAnalyzed = false;
            payloadDiagnosticAvailable = false;
            analyzedPayloadHash = string.Empty;
            payloadDiagnosticReport = string.Empty;
            payloadDiagnosticError = string.Empty;
        }

        private static void DrawReadOnly(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(label, value ?? "—");
            }
        }

        private static string EmptyAsDash(string value)
        {
            return string.IsNullOrEmpty(value) ? "—" : value;
        }

        private static string BuildMetadataReport(
            StylizedRiverFoamTopologyCacheAsset asset)
        {
            StringBuilder builder = new(2048);
            builder.AppendLine("RIVER FOAM TOPOLOGY CACHE METADATA");
            builder.AppendLine($"Asset: {AssetDatabase.GetAssetPath(asset)}");
            builder.AppendLine(
                $"Storage Contract Version: " +
                asset.StorageContractVersionValue);
            builder.AppendLine(
                $"Payload Format Version: {asset.PayloadFormatVersion}");
            builder.AppendLine(
                $"Generator Contract Version: " +
                asset.GeneratorContractVersion);
            builder.AppendLine(
                $"Grid Descriptor Contract Version: " +
                asset.GridDescriptorContractVersion);
            builder.AppendLine(
                $"Grid Mapping: {asset.GridMappingValue} — " +
                asset.GridMappingDisplayName);
            builder.AppendLine(
                $"Grid Mapping Contract Version: " +
                asset.GridMappingContractVersion);
            builder.AppendLine(
                $"Grid Initialization Signature: " +
                EmptyAsDash(asset.GridInitializationSignature));
            builder.AppendLine(
                $"Source River: {EmptyAsDash(asset.SourceRiverName)}");
            builder.AppendLine(
                $"Built UTC: {EmptyAsDash(asset.BuiltUtc)}");
            builder.AppendLine(
                $"Payload: {asset.PayloadByteCount:N0} bytes / " +
                EmptyAsDash(asset.PayloadHash));
            builder.AppendLine(
                $"Domain Fingerprint: " +
                EmptyAsDash(asset.DomainFingerprint));
            builder.AppendLine(
                $"Obstacle Fingerprint: " +
                EmptyAsDash(asset.ObstacleFingerprint));
            builder.AppendLine(
                $"Generation Fingerprint: " +
                EmptyAsDash(asset.GenerationFingerprint));
            builder.AppendLine(
                $"Combined Fingerprint: " +
                EmptyAsDash(asset.CombinedFingerprint));
            return builder.ToString();
        }
    }
}
