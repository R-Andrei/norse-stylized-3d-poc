using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    internal static class GeneratedMassSparseRiverbedSurfaceInstaller
    {
        private const string ProofOutputDirectory =
            "Library/SurfaceMaterialDiagnostics/" +
            "GeneratedMassSparseRiverbedAssembly";
        private const string ProofReportPath = ProofOutputDirectory +
            "/GeneratedMassSparseRiverbedAssemblyReport.txt";
        private const string InstallOutputDirectory =
            "Library/SurfaceMaterialDiagnostics/" +
            "GeneratedMassSparseRiverbedSurfaceInstall";
        private const string InstallReportPath = InstallOutputDirectory +
            "/GeneratedMassSparseRiverbedSurfaceInstallReport.txt";
        private const string SourceRoot =
            "Assets/Game/ArtSources/Editor/SurfaceMaterials/" +
            "SparseRiverbedCandidates";
        private const string ProfileRoot =
            "Assets/Game/Demo/Profiles/SurfaceMaterials/" +
            "SparseRiverbedCandidates";
        private const string LayerRoot =
            "Assets/Game/Demo/Profiles/Ground/Layers";
        private const string LibraryPath = ProfileRoot +
            "/SSDL_SparseRiverbedCandidates.asset";
        private const int PayloadResolution = 1024;

        private static readonly Color InitialBaseColor =
            new Color(0.517f, 0.503f, 0.458f, 1f);
        private static readonly Color InitialDarkColor =
            new Color(0.090f, 0.100f, 0.095f, 1f);
        private static readonly Color InitialLightColor =
            new Color(0.640f, 0.620f, 0.550f, 1f);
        private static readonly Color InitialCavityColor =
            new Color(0.045f, 0.041f, 0.034f, 1f);

        private static readonly CandidateDefinition[] Candidates =
        {
            new CandidateDefinition(
                "Ultra_Sparse_Riverbed",
                "riverbed-ultra-sparse",
                "Riverbed — Ultra Sparse",
                "SSMP_RiverbedUltraSparse.asset",
                "GSLP_RiverbedUltraSparse.asset"),
            new CandidateDefinition(
                "Very_Sparse_Riverbed",
                "riverbed-very-sparse",
                "Riverbed — Very Sparse",
                "SSMP_RiverbedVerySparse.asset",
                "GSLP_RiverbedVerySparse.asset"),
            new CandidateDefinition(
                "Sparse_Riverbed",
                "riverbed-sparse",
                "Riverbed — Sparse",
                "SSMP_RiverbedSparse.asset",
                "GSLP_RiverbedSparse.asset")
        };

        private sealed class CandidateDefinition
        {
            internal CandidateDefinition(
                string proofPrefix,
                string stableId,
                string displayName,
                string materialFileName,
                string layerFileName)
            {
                ProofPrefix = proofPrefix;
                StableId = stableId;
                DisplayName = displayName;
                MaterialPath = ProfileRoot + "/" + materialFileName;
                LayerPath = LayerRoot + "/" + layerFileName;
                PackedSourcePath = SourceRoot + "/" + proofPrefix +
                    "_RuntimePackedDetail.png";
                FormSourcePath = SourceRoot + "/" + proofPrefix +
                    "_PaletteForm.png";
            }

            internal string ProofPrefix { get; }
            internal string StableId { get; }
            internal string DisplayName { get; }
            internal string MaterialPath { get; }
            internal string LayerPath { get; }
            internal string PackedSourcePath { get; }
            internal string FormSourcePath { get; }
            internal float FeatureSubstrateRoughness { get; set; } = 0.5f;
            internal float FeatureMaximumSupportRadiusUv { get; set; }
        }

        private sealed class LibraryAssetSnapshot
        {
            internal bool ExistedBeforeRun;
            internal byte[] AssetBytes;
            internal string Guid;
        }

        [MenuItem(
            "Tools/PS3D/Install All Sparse Riverbed Surface Candidates")]
        private static void InstallAllCandidates()
        {
            Directory.CreateDirectory(InstallOutputDirectory);
            List<string> actions = new List<string>();
            List<string> warnings = new List<string>();
            List<string> failures = new List<string>();
            Dictionary<string, string> existingGuids =
                CaptureCanonicalAssetGuids();
            try
            {
                ValidateProofOutputs(failures);
                ValidateAssetConflicts(failures);
                DetectExternalDuplicateNames(warnings);
                if (failures.Count > 0)
                {
                    FinishReport(actions, warnings, failures);
                    return;
                }

                EnsureAssetFolder(SourceRoot);
                EnsureAssetFolder(ProfileRoot);
                EnsureAssetFolder(LayerRoot);
                CopyAndImportPayloads(actions, failures);
                if (failures.Count > 0)
                {
                    FinishReport(actions, warnings, failures);
                    return;
                }

                StylizedSurfaceDetailLibrary library =
                    CreateOrLoadAsset<StylizedSurfaceDetailLibrary>(
                        LibraryPath,
                        out bool libraryCreated,
                        failures);
                if (library == null || failures.Count > 0)
                {
                    FinishReport(actions, warnings, failures);
                    return;
                }

                if (!TryConfigureAndRebuildLibrary(
                        library,
                        libraryCreated,
                        actions,
                        failures))
                {
                    FinishReport(actions, warnings, failures);
                    return;
                }

                List<GroundSurfaceLayerProfile> layers =
                    new List<GroundSurfaceLayerProfile>(Candidates.Length);
                for (int index = 0; index < Candidates.Length; index++)
                {
                    CandidateDefinition definition = Candidates[index];
                    StylizedSurfaceMaterialProfile material =
                        CreateOrLoadAsset<StylizedSurfaceMaterialProfile>(
                            definition.MaterialPath,
                            out bool materialCreated,
                            failures);
                    if (material == null)
                    {
                        continue;
                    }

                    ConfigureMaterial(
                        material,
                        materialCreated,
                        library,
                        definition,
                        actions);

                    GroundSurfaceLayerProfile layer =
                        CreateOrLoadAsset<GroundSurfaceLayerProfile>(
                            definition.LayerPath,
                            out bool layerCreated,
                            failures);
                    if (layer == null)
                    {
                        continue;
                    }

                    ConfigureLayer(
                        layer,
                        layerCreated,
                        material,
                        definition,
                        actions);
                    layers.Add(layer);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                VerifyInstallation(library, layers, failures);
                VerifyCanonicalAssetGuids(existingGuids, actions, failures);
                FinishReport(actions, warnings, failures);
                if (failures.Count == 0 && layers.Count > 0)
                {
                    Selection.activeObject = layers[0];
                    EditorGUIUtility.PingObject(layers[0]);
                }
            }
            catch (Exception exception)
            {
                failures.Add(exception.ToString());
                FinishReport(actions, warnings, failures);
            }
        }

        private static bool TryConfigureAndRebuildLibrary(
            StylizedSurfaceDetailLibrary library,
            bool libraryCreated,
            List<string> actions,
            ICollection<string> failures)
        {
            LibraryAssetSnapshot snapshot = CaptureLibraryAssetSnapshot(
                library,
                libraryCreated,
                failures);
            if (snapshot == null)
            {
                return false;
            }

            int actionMarker = actions.Count;
            try
            {
                ConfigureLibrary(
                    library,
                    libraryCreated,
                    actions,
                    failures);
                if (failures.Count > 0)
                {
                    RollBackLibraryAsset(
                        snapshot,
                        actionMarker,
                        actions,
                        failures);
                    return false;
                }

                if (!StylizedSurfaceDetailLibraryBuilder.Rebuild(
                        library,
                        out IReadOnlyList<string> rebuildFailures,
                        false))
                {
                    if (rebuildFailures != null &&
                        rebuildFailures.Count > 0)
                    {
                        for (int index = 0;
                             index < rebuildFailures.Count;
                             index++)
                        {
                            failures.Add(
                                "Library rebuild: " +
                                rebuildFailures[index]);
                        }
                    }
                    else
                    {
                        failures.Add(
                            "The sparse-riverbed detail library rebuild " +
                            "failed without a detailed builder message.");
                    }

                    RollBackLibraryAsset(
                        snapshot,
                        actionMarker,
                        actions,
                        failures);
                    return false;
                }

                actions.Add(
                    "Rebuilt the dedicated packed-detail and texture-form " +
                    "arrays.");
                return true;
            }
            catch (Exception exception)
            {
                failures.Add(
                    "The sparse-riverbed detail library refresh threw an " +
                    "exception: " + exception);
                RollBackLibraryAsset(
                    snapshot,
                    actionMarker,
                    actions,
                    failures);
                return false;
            }
        }

        private static LibraryAssetSnapshot CaptureLibraryAssetSnapshot(
            StylizedSurfaceDetailLibrary library,
            bool libraryCreated,
            ICollection<string> failures)
        {
            LibraryAssetSnapshot snapshot = new LibraryAssetSnapshot
            {
                ExistedBeforeRun = !libraryCreated,
                Guid = AssetDatabase.AssetPathToGUID(LibraryPath)
            };
            if (libraryCreated)
            {
                return snapshot;
            }

            try
            {
                AssetDatabase.SaveAssetIfDirty(library);
                string absolutePath = AbsoluteProjectPath(LibraryPath);
                if (!File.Exists(absolutePath))
                {
                    failures.Add(
                        "The existing canonical detail-library asset file " +
                        "could not be captured before refresh: " +
                        LibraryPath + ".");
                    return null;
                }

                snapshot.AssetBytes = File.ReadAllBytes(absolutePath);
                return snapshot;
            }
            catch (Exception exception)
            {
                failures.Add(
                    "Could not capture the canonical detail-library state " +
                    "before refresh: " + exception.Message);
                return null;
            }
        }

        private static void RollBackLibraryAsset(
            LibraryAssetSnapshot snapshot,
            int actionMarker,
            List<string> actions,
            ICollection<string> failures)
        {
            while (actions.Count > actionMarker)
            {
                actions.RemoveAt(actions.Count - 1);
            }

            if (snapshot == null)
            {
                failures.Add(
                    "The canonical detail library could not be rolled back " +
                    "because no pre-refresh snapshot exists.");
                return;
            }

            try
            {
                if (!snapshot.ExistedBeforeRun)
                {
                    if (!AssetDatabase.DeleteAsset(LibraryPath))
                    {
                        failures.Add(
                            "Rollback could not delete the newly created " +
                            "canonical detail library: " + LibraryPath + ".");
                        return;
                    }

                    actions.Add(
                        "Rolled back the failed refresh by deleting the " +
                        "newly created canonical detail library.");
                    return;
                }

                if (snapshot.AssetBytes == null ||
                    snapshot.AssetBytes.Length == 0)
                {
                    failures.Add(
                        "Rollback snapshot bytes are missing for the " +
                        "existing canonical detail library.");
                    return;
                }

                File.WriteAllBytes(
                    AbsoluteProjectPath(LibraryPath),
                    snapshot.AssetBytes);
                AssetDatabase.ImportAsset(
                    LibraryPath,
                    ImportAssetOptions.ForceSynchronousImport |
                    ImportAssetOptions.ForceUpdate);
                StylizedSurfaceDetailLibrary restored =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceDetailLibrary>(LibraryPath);
                if (restored == null)
                {
                    failures.Add(
                        "Rollback restored the canonical asset bytes but " +
                        "Unity did not reload a detail-library asset.");
                    return;
                }

                string restoredGuid =
                    AssetDatabase.AssetPathToGUID(LibraryPath);
                if (!string.Equals(
                        restoredGuid,
                        snapshot.Guid,
                        StringComparison.Ordinal))
                {
                    failures.Add(
                        "Rollback changed the canonical detail-library GUID: " +
                        snapshot.Guid + " -> " + restoredGuid + ".");
                    return;
                }

                actions.Add(
                    "Rolled back the failed refresh and restored the " +
                    "pre-run canonical detail-library asset exactly.");
            }
            catch (Exception exception)
            {
                failures.Add(
                    "Could not roll back the canonical detail-library " +
                    "asset after refresh failure: " + exception.Message);
            }
        }

        private static Dictionary<string, string>
            CaptureCanonicalAssetGuids()
        {
            Dictionary<string, string> result =
                new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (string path in EnumerateCanonicalAssetPaths())
            {
                string guid = AssetDatabase.AssetPathToGUID(path);
                if (!string.IsNullOrEmpty(guid))
                {
                    result[path] = guid;
                }
            }

            return result;
        }

        private static void VerifyCanonicalAssetGuids(
            IReadOnlyDictionary<string, string> previousGuids,
            ICollection<string> actions,
            ICollection<string> failures)
        {
            foreach (string path in EnumerateCanonicalAssetPaths())
            {
                string currentGuid = AssetDatabase.AssetPathToGUID(path);
                if (string.IsNullOrEmpty(currentGuid))
                {
                    failures.Add(
                        $"Canonical asset has no GUID after refresh: '{path}'.");
                    continue;
                }

                if (previousGuids.TryGetValue(
                        path,
                        out string previousGuid))
                {
                    if (!string.Equals(
                            currentGuid,
                            previousGuid,
                            StringComparison.Ordinal))
                    {
                        failures.Add(
                            $"Canonical asset GUID changed for '{path}': " +
                            $"{previousGuid} -> {currentGuid}.");
                    }
                    else
                    {
                        actions.Add(
                            $"Preserved GUID {currentGuid} for {path}.");
                    }
                }
                else
                {
                    actions.Add(
                        $"Assigned new canonical GUID {currentGuid} to {path}.");
                }
            }
        }

        private static IEnumerable<string> EnumerateCanonicalAssetPaths()
        {
            yield return LibraryPath;
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                yield return candidate.PackedSourcePath;
                yield return candidate.FormSourcePath;
                yield return candidate.MaterialPath;
                yield return candidate.LayerPath;
            }
        }

        private static void DetectExternalDuplicateNames(
            ICollection<string> warnings)
        {
            DetectExternalDuplicateName<StylizedSurfaceDetailLibrary>(
                LibraryPath,
                warnings);
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                DetectExternalDuplicateName<Texture2D>(
                    candidate.PackedSourcePath,
                    warnings);
                DetectExternalDuplicateName<Texture2D>(
                    candidate.FormSourcePath,
                    warnings);
                DetectExternalDuplicateName<StylizedSurfaceMaterialProfile>(
                    candidate.MaterialPath,
                    warnings);
                DetectExternalDuplicateName<GroundSurfaceLayerProfile>(
                    candidate.LayerPath,
                    warnings);
            }
        }

        private static void DetectExternalDuplicateName<T>(
            string canonicalPath,
            ICollection<string> warnings)
            where T : UnityEngine.Object
        {
            string assetName = Path.GetFileNameWithoutExtension(canonicalPath);
            string[] guids = AssetDatabase.FindAssets(
                assetName + " t:" + typeof(T).Name);
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                if (!string.Equals(
                        Path.GetFileNameWithoutExtension(path),
                        assetName,
                        StringComparison.Ordinal) ||
                    string.Equals(
                        path,
                        canonicalPath,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                warnings.Add(
                    $"Duplicate canonical asset name '{assetName}' exists " +
                    $"outside the installer path: {path}. It was not changed.");
            }
        }

        private static void ValidateProofOutputs(ICollection<string> failures)
        {
            string reportPath = AbsoluteProjectPath(ProofReportPath);
            if (!File.Exists(reportPath))
            {
                failures.Add(
                    "The M2.7C.5E.2.4B proof report is missing. Run Tools > " +
                    "PS3D > Run Generated Mass Sparse Riverbed Assembly " +
                    "Proof before installing the surfaces.");
                return;
            }

            string report = File.ReadAllText(reportPath);
            if (report.IndexOf(
                    "GSU-M2.7C.5E.2.4B",
                    StringComparison.Ordinal) < 0 ||
                report.IndexOf(
                    "Assembler algorithm version: 10",
                    StringComparison.Ordinal) < 0 ||
                report.IndexOf(
                    "VERDICT: PASS",
                    StringComparison.Ordinal) < 0)
            {
                failures.Add(
                    "The local sparse-riverbed proof is not a passing " +
                    "M2.7C.5E.2.4B algorithm-10 run.");
                return;
            }

            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                ParseProofFeatureMetadata(report, candidate, failures);
                ValidateProofFile(
                    candidate.ProofPrefix + "_PaletteForm.png",
                    failures);
                ValidateProofFile(
                    candidate.ProofPrefix + "_RuntimePackedDetail.png",
                    failures);
            }
        }

        private static void ParseProofFeatureMetadata(
            string report,
            CandidateDefinition candidate,
            ICollection<string> failures)
        {
            string sectionToken = "[" + candidate.ProofPrefix + "]";
            int sectionStart = report.IndexOf(
                sectionToken,
                StringComparison.Ordinal);
            if (sectionStart < 0)
            {
                failures.Add(
                    "Proof report has no candidate section " +
                    sectionToken + ".");
                return;
            }

            int sectionEnd = report.IndexOf(
                "\n[",
                sectionStart + sectionToken.Length,
                StringComparison.Ordinal);
            if (sectionEnd < 0)
            {
                sectionEnd = report.Length;
            }

            string section = report.Substring(
                sectionStart,
                sectionEnd - sectionStart);
            if (!TryParseProofFloat(
                    section,
                    "Feature substrate roughness scalar:",
                    out float substrateRoughness) ||
                !TryParseProofFloat(
                    section,
                    "Feature maximum support radius UV:",
                    out float maximumSupportRadiusUv))
            {
                failures.Add(
                    candidate.ProofPrefix +
                    ": algorithm-10 proof metadata is missing or invalid.");
                return;
            }

            if (substrateRoughness < 0.55f ||
                substrateRoughness > 0.80f ||
                maximumSupportRadiusUv <= 0f ||
                maximumSupportRadiusUv > 0.25f)
            {
                failures.Add(
                    candidate.ProofPrefix +
                    ": proof metadata roughness/radius is out of range: " +
                    substrateRoughness.ToString(
                        "F5",
                        CultureInfo.InvariantCulture) + " / " +
                    maximumSupportRadiusUv.ToString(
                        "F6",
                        CultureInfo.InvariantCulture) + ".");
                return;
            }

            candidate.FeatureSubstrateRoughness = substrateRoughness;
            candidate.FeatureMaximumSupportRadiusUv =
                maximumSupportRadiusUv;
        }

        private static bool TryParseProofFloat(
            string section,
            string label,
            out float value)
        {
            value = 0f;
            int labelIndex = section.IndexOf(
                label,
                StringComparison.Ordinal);
            if (labelIndex < 0)
            {
                return false;
            }

            int valueStart = labelIndex + label.Length;
            int lineEnd = section.IndexOf('\n', valueStart);
            if (lineEnd < 0)
            {
                lineEnd = section.Length;
            }

            string text = section.Substring(
                valueStart,
                lineEnd - valueStart).Trim();
            return float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) &&
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }

        private static void ValidateProofFile(
            string fileName,
            ICollection<string> failures)
        {
            string path = AbsoluteProjectPath(
                ProofOutputDirectory + "/" + fileName);
            if (!File.Exists(path))
            {
                failures.Add("Missing proof payload: " + path);
            }
        }

        private static void ValidateAssetConflicts(
            ICollection<string> failures)
        {
            ValidateAssetTypeConflict<StylizedSurfaceDetailLibrary>(
                LibraryPath,
                failures);
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition definition = Candidates[index];
                ValidateAssetTypeConflict<StylizedSurfaceMaterialProfile>(
                    definition.MaterialPath,
                    failures);
                ValidateAssetTypeConflict<GroundSurfaceLayerProfile>(
                    definition.LayerPath,
                    failures);
                ValidateAssetTypeConflict<Texture2D>(
                    definition.PackedSourcePath,
                    failures);
                ValidateAssetTypeConflict<Texture2D>(
                    definition.FormSourcePath,
                    failures);
            }
        }

        private static void ValidateAssetTypeConflict<T>(
            string path,
            ICollection<string> failures)
            where T : UnityEngine.Object
        {
            UnityEngine.Object existing =
                AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null && !(existing is T))
            {
                failures.Add(
                    $"Asset path '{path}' is occupied by " +
                    $"{existing.GetType().Name}, expected {typeof(T).Name}.");
            }
        }

        private static void CopyAndImportPayloads(
            ICollection<string> actions,
            ICollection<string> failures)
        {
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                CopyPayload(
                    candidate.ProofPrefix + "_RuntimePackedDetail.png",
                    candidate.PackedSourcePath,
                    false,
                    actions,
                    failures);
                CopyPayload(
                    candidate.ProofPrefix + "_PaletteForm.png",
                    candidate.FormSourcePath,
                    true,
                    actions,
                    failures);
            }
        }

        private static void CopyPayload(
            string proofFileName,
            string destinationAssetPath,
            bool sRgb,
            ICollection<string> actions,
            ICollection<string> failures)
        {
            string sourcePath = AbsoluteProjectPath(
                ProofOutputDirectory + "/" + proofFileName);
            string destinationPath =
                AbsoluteProjectPath(destinationAssetPath);
            try
            {
                Directory.CreateDirectory(
                    Path.GetDirectoryName(destinationPath) ?? string.Empty);
                bool existed = File.Exists(destinationPath);
                string sourceHash = CalculateFileSha256(sourcePath);
                string previousHash = existed
                    ? CalculateFileSha256(destinationPath)
                    : string.Empty;
                bool contentChanged = !existed ||
                    !string.Equals(
                        sourceHash,
                        previousHash,
                        StringComparison.Ordinal);
                if (contentChanged)
                {
                    File.Copy(sourcePath, destinationPath, true);
                    AssetDatabase.ImportAsset(
                        destinationAssetPath,
                        ImportAssetOptions.ForceSynchronousImport |
                        ImportAssetOptions.ForceUpdate);
                }

                bool importerChanged =
                    NormalizePayloadImporter(destinationAssetPath, sRgb);
                bool changed = contentChanged || importerChanged;
                Texture2D imported =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        destinationAssetPath);
                if (imported == null ||
                    imported.width != PayloadResolution ||
                    imported.height != PayloadResolution)
                {
                    failures.Add(
                        $"Imported payload '{destinationAssetPath}' is not " +
                        $"{PayloadResolution}×{PayloadResolution}.");
                    return;
                }

                actions.Add(
                    (existed
                        ? changed ? "Updated " : "Unchanged "
                        : "Created ") +
                    destinationAssetPath + " (SHA-256 " +
                    CalculateFileSha256(destinationPath) + ").");
            }
            catch (Exception exception)
            {
                failures.Add(
                    $"Could not import '{destinationAssetPath}': " +
                    exception.Message);
            }
        }

        private static bool NormalizePayloadImporter(
            string assetPath,
            bool sRgb)
        {
            TextureImporter importer =
                AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                throw new InvalidOperationException(
                    "No TextureImporter exists for " + assetPath + ".");
            }

            bool changed =
                importer.textureType != TextureImporterType.Default ||
                importer.sRGBTexture != sRgb ||
                importer.alphaSource !=
                    TextureImporterAlphaSource.FromInput ||
                importer.alphaIsTransparency ||
                !importer.isReadable ||
                !importer.mipmapEnabled ||
                importer.streamingMipmaps ||
                importer.wrapMode != TextureWrapMode.Repeat ||
                importer.filterMode != FilterMode.Bilinear ||
                importer.anisoLevel != 1 ||
                importer.textureCompression !=
                    TextureImporterCompression.Uncompressed ||
                importer.maxTextureSize != PayloadResolution ||
                importer.npotScale != TextureImporterNPOTScale.None;

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = sRgb;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = false;
            importer.isReadable = true;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = false;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 1;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;
            importer.maxTextureSize = PayloadResolution;
            importer.npotScale = TextureImporterNPOTScale.None;
            if (changed)
            {
                importer.SaveAndReimport();
            }

            return changed;
        }

        private static void ConfigureLibrary(
            StylizedSurfaceDetailLibrary library,
            bool created,
            ICollection<string> actions,
            ICollection<string> failures)
        {

            bool matchedBefore =
                !created && LibraryMatchesCanonicalSources(library);

            SerializedObject serialized = new SerializedObject(library);
            serialized.FindProperty("sliceResolution").intValue =
                PayloadResolution;
            SerializedProperty entries =
                serialized.FindProperty("entries");
            entries.arraySize = Candidates.Length;
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                SerializedProperty entry =
                    entries.GetArrayElementAtIndex(index);
                entry.FindPropertyRelative("stableId").stringValue =
                    candidate.StableId;
                entry.FindPropertyRelative("displayName").stringValue =
                    candidate.DisplayName;
                entry.FindPropertyRelative("sourceMode").intValue =
                    (int)StylizedSurfaceDetailSourceMode
                        .PrepackedDetailWithFeatureTextureForm;
                entry.FindPropertyRelative("sourceTexture")
                    .objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        candidate.PackedSourcePath);
                entry.FindPropertyRelative("prepackedTextureForm")
                    .objectReferenceValue =
                    AssetDatabase.LoadAssetAtPath<Texture2D>(
                        candidate.FormSourcePath);
                entry.FindPropertyRelative("featureSubstrateRoughness")
                    .floatValue = candidate.FeatureSubstrateRoughness;
                entry.FindPropertyRelative("featureMaximumSupportRadiusUv")
                    .floatValue = candidate.FeatureMaximumSupportRadiusUv;
                ClearAuthoredMaterialReferences(entry);
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssetIfDirty(library);
            actions.Add(
                (created
                    ? "Created "
                    : matchedBefore ? "Unchanged " : "Updated ") +
                LibraryPath + ".");
        }

        private static bool LibraryMatchesCanonicalSources(
            StylizedSurfaceDetailLibrary library)
        {
            if (library == null ||
                library.SliceResolution != PayloadResolution ||
                library.Entries.Count != Candidates.Length)
            {
                return false;
            }

            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null ||
                    !string.Equals(
                        entry.StableId,
                        candidate.StableId,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        entry.DisplayName,
                        candidate.DisplayName,
                        StringComparison.Ordinal) ||
                    entry.SourceMode !=
                        StylizedSurfaceDetailSourceMode
                            .PrepackedDetailWithFeatureTextureForm ||
                    entry.SourceTexture !=
                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                            candidate.PackedSourcePath) ||
                    entry.PrepackedTextureForm !=
                        AssetDatabase.LoadAssetAtPath<Texture2D>(
                            candidate.FormSourcePath) ||
                    Mathf.Abs(
                        entry.FeatureSubstrateRoughness -
                        candidate.FeatureSubstrateRoughness) > 0.00001f ||
                    Mathf.Abs(
                        entry.FeatureMaximumSupportRadiusUv -
                        candidate.FeatureMaximumSupportRadiusUv) > 0.000001f)
                {
                    return false;
                }
            }

            return true;
        }

        private static void ClearAuthoredMaterialReferences(
            SerializedProperty entry)
        {
            string[] names =
            {
                "authoredBaseColor",
                "authoredNormal",
                "authoredHeight",
                "authoredAmbientOcclusion",
                "authoredRoughness"
            };
            for (int index = 0; index < names.Length; index++)
            {
                SerializedProperty property =
                    entry.FindPropertyRelative(names[index]);
                if (property != null)
                {
                    property.objectReferenceValue = null;
                }
            }
        }

        private static void ConfigureMaterial(
            StylizedSurfaceMaterialProfile material,
            bool created,
            StylizedSurfaceDetailLibrary library,
            CandidateDefinition candidate,
            ICollection<string> actions)
        {
            bool requiredReferencesChanged =
                !created &&
                (!material.DetailEnabled ||
                 material.DetailLibrary != library ||
                 !string.Equals(
                     material.DetailEntryId,
                     candidate.StableId,
                     StringComparison.Ordinal));
            SerializedObject serialized = new SerializedObject(material);
            if (created)
            {
                SetString(serialized, "displayName", candidate.DisplayName);
                SetColor(serialized, "baseColor", InitialBaseColor);
                SetColor(serialized, "darkColor", InitialDarkColor);
                SetColor(serialized, "lightColor", InitialLightColor);
                SetColor(serialized, "cavityColor", InitialCavityColor);
                SetFloat(serialized, "authoredColorStrength", 1f);
                SetFloat(
                    serialized,
                    "authoredColorLightingStrength",
                    0.60f);
                SetFloat(
                    serialized,
                    "authoredRoughnessStrength",
                    1f);
                SetFloat(serialized, "macroContrast", 0f);
                SetFloat(serialized, "legacyPixelCellInfluence", 0f);
                SetFloat(serialized, "detailValueStrength", 0f);
                SetFloat(serialized, "detailWorldScale", 8f);
                SetFloat(serialized, "detailNormalStrength", 0.85f);
                SetFloat(serialized, "detailCavityStrength", 1f);
                SetFloat(serialized, "detailCavityBias", 0.15f);
                SetFloat(
                    serialized,
                    "detailFormHighlightStrength",
                    0f);
                SetFloat(serialized, "drySmoothness", 0.16f);
                SetFloat(serialized, "drySpecularStrength", 0.05f);
                SetFloat(serialized, "finishVariationStrength", 0f);
            }

            SetBool(serialized, "detailEnabled", true);
            SetObject(serialized, "detailLibrary", library);
            SetString(serialized, "detailEntryId", candidate.StableId);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssetIfDirty(material);
            material.NotifyEditorChanged();
            actions.Add(
                (created
                    ? "Created "
                    : requiredReferencesChanged
                        ? "Updated "
                        : "Unchanged ") +
                candidate.MaterialPath +
                (created
                    ? " with Higher Contrast palette defaults."
                    : " while preserving existing palette/tuning."));
        }

        private static void ConfigureLayer(
            GroundSurfaceLayerProfile layer,
            bool created,
            StylizedSurfaceMaterialProfile material,
            CandidateDefinition candidate,
            ICollection<string> actions)
        {
            bool requiredReferenceChanged =
                !created && layer.SurfaceMaterial != material;
            SerializedObject serialized = new SerializedObject(layer);
            if (created)
            {
                SetString(serialized, "displayName", candidate.DisplayName);
                SetColor(serialized, "baseColor", InitialBaseColor);
                SetColor(serialized, "darkColor", InitialDarkColor);
                SetColor(serialized, "lightColor", InitialLightColor);
                SetFloat(serialized, "macroContrast", 0f);
                SetFloat(serialized, "pixelContrast", 0f);
                SetFloat(serialized, "drySmoothness", 0.16f);
                SetFloat(serialized, "drySpecularStrength", 0.05f);
                SetFloat(serialized, "vegetationRetention", 0.05f);
                SetFloat(serialized, "snowRetention", 0.50f);
                SetFloat(serialized, "frostRetention", 0.30f);
                SetFloat(serialized, "paintedAccentRetention", 0.10f);
            }

            SetObject(serialized, "surfaceMaterial", material);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(layer);
            AssetDatabase.SaveAssetIfDirty(layer);
            actions.Add(
                (created
                    ? "Created "
                    : requiredReferenceChanged
                        ? "Updated "
                        : "Unchanged ") +
                candidate.LayerPath +
                (created
                    ? "."
                    : " while preserving existing layer tuning."));
        }

        private static void VerifyInstallation(
            StylizedSurfaceDetailLibrary library,
            IReadOnlyList<GroundSurfaceLayerProfile> layers,
            ICollection<string> failures)
        {
            IReadOnlyList<string> libraryValidation =
                StylizedSurfaceDetailLibraryBuilder.Validate(library);
            for (int index = 0; index < libraryValidation.Count; index++)
            {
                failures.Add(
                    "Library validation: " + libraryValidation[index]);
            }

            if (StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library))
            {
                failures.Add(
                    "The dedicated detail library is still stale after rebuild.");
            }

            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                if (!library.TryResolve(
                        candidate.StableId,
                        out Texture2DArray packedArray,
                        out int packedSlice) ||
                    packedArray == null ||
                    packedSlice < 0)
                {
                    failures.Add(
                        candidate.StableId +
                        " does not resolve a packed-detail slice.");
                }

                if (!library.TryResolveAuthoredColor(
                        candidate.StableId,
                        out Texture2DArray formArray,
                        out int formSlice) ||
                    formArray == null ||
                    formSlice < 0)
                {
                    failures.Add(
                        candidate.StableId +
                        " does not resolve a Palette Form slice.");
                }

                StylizedSurfaceMaterialProfile material =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceMaterialProfile>(
                        candidate.MaterialPath);
                if (material == null ||
                    material.DetailLibrary != library ||
                    !string.Equals(
                        material.DetailEntryId,
                        candidate.StableId,
                        StringComparison.Ordinal) ||
                    !material.UsesTextureForm ||
                    !material.UsesFeatureTextureForm ||
                    !material.TryResolveDetail(out _, out _) ||
                    !material.TryResolveTextureForm(out _, out _))
                {
                    failures.Add(
                        candidate.MaterialPath +
                        " does not resolve the paired runtime payload.");
                }

                GroundSurfaceLayerProfile layer =
                    AssetDatabase.LoadAssetAtPath<
                        GroundSurfaceLayerProfile>(candidate.LayerPath);
                if (layer == null || layer.SurfaceMaterial != material)
                {
                    failures.Add(
                        candidate.LayerPath +
                        " does not reference its material profile.");
                }
            }

            if (layers.Count != Candidates.Length)
            {
                failures.Add(
                    $"Installed layer count is {layers.Count}; expected " +
                    $"{Candidates.Length}.");
            }
        }

        private static T CreateOrLoadAsset<T>(
            string path,
            out bool created,
            ICollection<string> failures)
            where T : ScriptableObject
        {
            created = false;
            UnityEngine.Object existing =
                AssetDatabase.LoadMainAssetAtPath(path);
            if (existing != null)
            {
                if (existing is T typed)
                {
                    return typed;
                }

                failures.Add(
                    $"Asset path '{path}' contains {existing.GetType().Name}, " +
                    $"expected {typeof(T).Name}.");
                return null;
            }

            T asset = ScriptableObject.CreateInstance<T>();
            asset.name = Path.GetFileNameWithoutExtension(path);
            AssetDatabase.CreateAsset(asset, path);
            created = true;
            return asset;
        }

        private static void EnsureAssetFolder(string path)
        {
            string normalized = path.Replace('\\', '/');
            string[] segments = normalized.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(
                        current,
                        segments[index]);
                }

                current = next;
            }
        }

        private static void SetString(
            SerializedObject serialized,
            string name,
            string value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.stringValue = value;
            }
        }

        private static void SetFloat(
            SerializedObject serialized,
            string name,
            float value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.floatValue = value;
            }
        }

        private static void SetBool(
            SerializedObject serialized,
            string name,
            bool value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.boolValue = value;
            }
        }

        private static void SetColor(
            SerializedObject serialized,
            string name,
            Color value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.colorValue = value;
            }
        }

        private static void SetObject(
            SerializedObject serialized,
            string name,
            UnityEngine.Object value)
        {
            SerializedProperty property = serialized.FindProperty(name);
            if (property != null)
            {
                property.objectReferenceValue = value;
            }
        }

        private static void FinishReport(
            IReadOnlyCollection<string> actions,
            IReadOnlyCollection<string> warnings,
            IReadOnlyCollection<string> failures)
        {
            StringBuilder builder = new StringBuilder(16384);
            builder.AppendLine(
                "GENERATED MASS SPARSE RIVERBED SURFACE REFRESH — " +
                "GSU-M2.7C.5E.2.4B.1");
            builder.AppendLine(
                "Generated UTC: " +
                DateTime.UtcNow.ToString(
                    "O",
                    CultureInfo.InvariantCulture));
            builder.AppendLine("Unity: " + Application.unityVersion);
            builder.AppendLine(
                "Source proof: " + ProofReportPath);
            builder.AppendLine(
                "Dedicated library: " + LibraryPath);
            builder.AppendLine();
            builder.AppendLine("ACTIONS");
            if (actions.Count == 0)
            {
                builder.AppendLine("- None.");
            }
            else
            {
                foreach (string action in actions)
                {
                    builder.AppendLine("- " + action);
                }
            }

            builder.AppendLine();
            builder.AppendLine("WARNINGS");
            if (warnings.Count == 0)
            {
                builder.AppendLine("- None.");
            }
            else
            {
                foreach (string warning in warnings)
                {
                    builder.AppendLine("- " + warning);
                }
            }

            builder.AppendLine();
            builder.AppendLine("SURFACES");
            for (int index = 0; index < Candidates.Length; index++)
            {
                CandidateDefinition candidate = Candidates[index];
                builder.AppendLine(
                    $"- {candidate.DisplayName}: {candidate.LayerPath}");
                builder.AppendLine(
                    $"  Material: {candidate.MaterialPath}");
                builder.AppendLine(
                    $"  Stable detail ID: {candidate.StableId}");
            }

            builder.AppendLine();
            if (failures.Count > 0)
            {
                builder.AppendLine(
                    $"VERDICT: FAIL — {failures.Count} issue(s).");
                foreach (string failure in failures)
                {
                    builder.AppendLine("- " + failure);
                }
            }
            else
            {
                builder.AppendLine(
                    "VERDICT: PASS — three paired-payload sparse-riverbed " +
                    "surface layers resolve and are ready for in-scene " +
                    "comparison. No scene assignment was performed.");
            }

            string report = builder.ToString();
            Directory.CreateDirectory(InstallOutputDirectory);
            File.WriteAllText(
                AbsoluteProjectPath(InstallReportPath),
                report,
                Encoding.UTF8);
            EditorGUIUtility.systemCopyBuffer = report;
            if (failures.Count > 0)
            {
                Debug.LogError(
                    "[GSU-M2.7C.5E.2.4B.1] Sparse riverbed surface refresh " +
                    "failed. Report written to " + InstallReportPath +
                    " and copied to the clipboard.");
            }
            else
            {
                Debug.Log(
                    "[GSU-M2.7C.5E.2.4B.1] Refreshed all three sparse " +
                    "riverbed surface candidates. Report written to " +
                    InstallReportPath +
                    " and copied to the clipboard.");
            }
        }

        private static string AbsoluteProjectPath(string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    relativePath));
        }

        private static string CalculateFileSha256(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    builder.Append(hash[index].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
