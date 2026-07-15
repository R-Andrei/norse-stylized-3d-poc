using System;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    internal static class GroundPaintedAccentProductionBaker
    {
        private const string GeneratedRoot =
            "Assets/Game/Generated/Ground/PaintedAccents";
        private const string AssetPrefix =
            "GG_PaintedAccentCoverage_";

        internal static string GeneratedRootPath => GeneratedRoot;
        internal static string AssetNamePrefix => AssetPrefix;

        public static bool Bake(
            GeneratedGround ground,
            out string resultMessage)
        {
            resultMessage = string.Empty;
            if (ground == null)
            {
                resultMessage = "No GeneratedGround was selected.";
                return false;
            }

            if (Application.isPlaying)
            {
                resultMessage =
                    "Painted Accent production output can only be baked in Edit Mode.";
                return false;
            }

            if (EditorUtility.IsPersistent(ground) ||
                PrefabUtility.IsPartOfPrefabAsset(ground))
            {
                resultMessage =
                    "Bake a GeneratedGround scene instance, not a prefab or persistent asset.";
                return false;
            }

            if (!TryResolveSceneGuid(
                    ground,
                    out string sceneGuid,
                    out string failureReason))
            {
                resultMessage = failureReason;
                return false;
            }

            if (!ground.TryPreparePaintedAccentProductionBake(
                    out GroundPaintedAccentProductionBakeSource source,
                    out failureReason))
            {
                resultMessage = failureReason;
                return false;
            }

            string identifier = ResolveUniqueIdentifier(
                ground,
                sceneGuid);
            if (!EnsureGeneratedFolder(
                    sceneGuid,
                    out string outputFolder,
                    out failureReason))
            {
                resultMessage = failureReason;
                return false;
            }

            string assetPath = BuildAssetPath(
                outputFolder,
                identifier);
            if (!TryCreateOrUpdateTexture(
                    source.CoverageTexture,
                    assetPath,
                    identifier,
                    out Texture2D persistentTexture,
                    out failureReason))
            {
                resultMessage = failureReason;
                return false;
            }

            Undo.RecordObject(ground, "Bake Painted Accents");
            ground.EditorApplyPaintedAccentProductionBake(
                identifier,
                persistentTexture,
                source.CoverageSignature,
                source.OriginSize,
                source.Diagnostics);
            EditorUtility.SetDirty(ground);
            EditorUtility.SetDirty(persistentTexture);
            AssetDatabase.SaveAssetIfDirty(persistentTexture);
            SceneView.RepaintAll();

            resultMessage =
                $"Painted Accent production coverage is current.\n" +
                $"Asset: {assetPath}\n" +
                $"Resolution: {persistentTexture.width} × " +
                $"{persistentTexture.height} R8\n" +
                $"Covered texels: " +
                $"{source.Diagnostics.CoveredTexelCount:N0}";
            return true;
        }

        public static bool HasDuplicateIdentifier(
            GeneratedGround ground)
        {
            if (ground == null ||
                string.IsNullOrWhiteSpace(
                    ground.PaintedAccentProductionBakeIdentifier) ||
                !TryResolveSceneGuid(
                    ground,
                    out string sceneGuid,
                    out _))
            {
                return false;
            }

            return IsIdentifierInUseByAnotherGround(
                ground.PaintedAccentProductionBakeIdentifier,
                sceneGuid,
                ground);
        }

        public static bool HasOwnershipMismatch(
            GeneratedGround ground)
        {
            if (ground == null ||
                ground.PaintedAccentProductionCoverageTexture == null ||
                string.IsNullOrWhiteSpace(
                    ground.PaintedAccentProductionBakeIdentifier))
            {
                return false;
            }

            if (!TryBuildAssetPath(
                    ground,
                    ground.PaintedAccentProductionBakeIdentifier,
                    out string expectedPath,
                    out _))
            {
                return true;
            }

            string currentPath = AssetDatabase.GetAssetPath(
                ground.PaintedAccentProductionCoverageTexture);
            return !string.Equals(
                currentPath,
                expectedPath,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string ResolveUniqueIdentifier(
            GeneratedGround ground,
            string sceneGuid)
        {
            string existing = ground.PaintedAccentProductionBakeIdentifier;
            if (IsValidIdentifier(existing) &&
                !IsIdentifierInUseByAnotherGround(
                    existing,
                    sceneGuid,
                    ground))
            {
                return existing;
            }

            string candidate;
            do
            {
                candidate = Guid.NewGuid().ToString("N");
            }
            while (IsIdentifierInUseByAnotherGround(
                candidate,
                sceneGuid,
                ground));

            return candidate;
        }

        internal static bool IsValidIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) ||
                identifier.Length != 32)
            {
                return false;
            }

            for (int index = 0; index < identifier.Length; index++)
            {
                char character = identifier[index];
                bool hexadecimal =
                    character >= '0' && character <= '9' ||
                    character >= 'a' && character <= 'f' ||
                    character >= 'A' && character <= 'F';
                if (!hexadecimal)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierInUseByAnotherGround(
            string identifier,
            string ownerSceneGuid,
            GeneratedGround owner)
        {
            if (string.IsNullOrWhiteSpace(identifier) ||
                string.IsNullOrWhiteSpace(ownerSceneGuid))
            {
                return false;
            }

            GeneratedGround[] grounds =
                UnityEngine.Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);
            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround candidate = grounds[index];
                if (candidate == null ||
                    candidate == owner ||
                    EditorUtility.IsPersistent(candidate) ||
                    !TryResolveSceneGuid(
                        candidate,
                        out string candidateSceneGuid,
                        out _) ||
                    !string.Equals(
                        candidateSceneGuid,
                        ownerSceneGuid,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (string.Equals(
                        candidate.PaintedAccentProductionBakeIdentifier,
                        identifier,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool TryGetExpectedAssetPath(
            GeneratedGround ground,
            string identifier,
            out string assetPath,
            out string failureReason)
        {
            return TryBuildAssetPath(
                ground,
                identifier,
                out assetPath,
                out failureReason);
        }

        internal static bool TryGetExpectedAssetPath(
            string scenePath,
            string identifier,
            out string assetPath,
            out string failureReason)
        {
            assetPath = string.Empty;
            failureReason = string.Empty;
            if (string.IsNullOrWhiteSpace(scenePath))
            {
                failureReason =
                    "A saved scene path is required for generated-output ownership validation.";
                return false;
            }

            string sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            if (string.IsNullOrWhiteSpace(sceneGuid))
            {
                failureReason =
                    "The scene does not have a stable AssetDatabase GUID.";
                return false;
            }

            assetPath = BuildAssetPath(
                GeneratedRoot + "/" + sceneGuid,
                identifier);
            return true;
        }

        private static bool TryBuildAssetPath(
            GeneratedGround ground,
            string identifier,
            out string assetPath,
            out string failureReason)
        {
            assetPath = string.Empty;
            if (!TryResolveSceneGuid(
                    ground,
                    out string sceneGuid,
                    out failureReason))
            {
                return false;
            }

            assetPath = BuildAssetPath(
                GeneratedRoot + "/" + sceneGuid,
                identifier);
            return true;
        }

        private static bool TryResolveSceneGuid(
            GeneratedGround ground,
            out string sceneGuid,
            out string failureReason)
        {
            sceneGuid = string.Empty;
            failureReason = string.Empty;
            if (ground == null ||
                !ground.gameObject.scene.IsValid() ||
                string.IsNullOrWhiteSpace(ground.gameObject.scene.path))
            {
                failureReason =
                    "Save the scene before baking Painted Accent production output. A saved scene GUID is required for stable generated-resource ownership.";
                return false;
            }

            sceneGuid = AssetDatabase.AssetPathToGUID(
                ground.gameObject.scene.path);
            if (string.IsNullOrWhiteSpace(sceneGuid))
            {
                failureReason =
                    "The current scene does not have a stable AssetDatabase GUID. Save or reimport the scene before baking.";
                return false;
            }

            return true;
        }

        private static bool EnsureGeneratedFolder(
            string sceneGuid,
            out string outputFolder,
            out string failureReason)
        {
            outputFolder = string.Empty;
            failureReason = string.Empty;
            if (!EnsureFolderPath(GeneratedRoot, out failureReason))
            {
                return false;
            }

            outputFolder = GeneratedRoot + "/" + sceneGuid;
            return EnsureFolderPath(outputFolder, out failureReason);
        }

        private static bool EnsureFolderPath(
            string folderPath,
            out string failureReason)
        {
            failureReason = string.Empty;
            string[] segments = folderPath.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(
                        current,
                        segments[index]);
                    if (string.IsNullOrWhiteSpace(guid))
                    {
                        failureReason =
                            $"Could not create generated-output folder: {next}";
                        return false;
                    }
                }

                current = next;
            }

            return true;
        }

        private static string BuildAssetName(string identifier)
        {
            return AssetPrefix + identifier;
        }

        private static string BuildAssetPath(
            string outputFolder,
            string identifier)
        {
            return
                outputFolder + "/" +
                BuildAssetName(identifier) + ".asset";
        }

        private static bool TryCreateOrUpdateTexture(
            Texture2D source,
            string assetPath,
            string identifier,
            out Texture2D destination,
            out string failureReason)
        {
            destination = null;
            failureReason = string.Empty;
            if (source == null ||
                !source.isReadable ||
                source.format != TextureFormat.R8)
            {
                failureReason =
                    "The live Painted Accent source is not a readable R8 texture.";
                return false;
            }

            UnityEngine.Object existingMainAsset =
                AssetDatabase.LoadMainAssetAtPath(assetPath);
            destination = existingMainAsset as Texture2D;
            if (existingMainAsset != null && destination == null)
            {
                failureReason =
                    $"The generated output path is occupied by a non-texture asset: {assetPath}";
                return false;
            }

            bool created = destination == null;
            try
            {
                if (created)
                {
                    destination = CreatePersistentTexture(
                        source.width,
                        source.height,
                        identifier);
                }
                else if (
                    destination.width != source.width ||
                    destination.height != source.height ||
                    destination.format != TextureFormat.R8 ||
                    !destination.isReadable)
                {
                    if (!destination.Reinitialize(
                            source.width,
                            source.height,
                            TextureFormat.R8,
                            false))
                    {
                        failureReason =
                            $"Could not reinitialize generated coverage texture: {assetPath}";
                        return false;
                    }
                }

                destination.name =
                    BuildAssetName(identifier);
                destination.hideFlags = HideFlags.None;
                destination.filterMode = FilterMode.Bilinear;
                destination.wrapMode = TextureWrapMode.Clamp;
                destination.anisoLevel = 0;

                var rawData = source.GetRawTextureData<byte>();
                if (rawData.Length != source.width * source.height)
                {
                    failureReason =
                        "The live R8 coverage data length does not match its texture dimensions.";
                    return false;
                }

                destination.LoadRawTextureData(rawData);
                destination.Apply(false, false);

                if (created)
                {
                    AssetDatabase.CreateAsset(destination, assetPath);
                }

                return true;
            }
            catch (Exception exception)
            {
                failureReason =
                    $"Could not create or update Painted Accent production coverage at {assetPath}.\n{exception.Message}";
                return false;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(failureReason) &&
                    created &&
                    destination != null &&
                    !EditorUtility.IsPersistent(destination))
                {
                    UnityEngine.Object.DestroyImmediate(destination);
                    destination = null;
                }
            }
        }

        private static Texture2D CreatePersistentTexture(
            int width,
            int height,
            string identifier)
        {
            return new Texture2D(
                Mathf.Max(1, width),
                Mathf.Max(1, height),
                TextureFormat.R8,
                false,
                true)
            {
                name = BuildAssetName(identifier),
                hideFlags = HideFlags.None,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                anisoLevel = 0
            };
        }
    }
}
