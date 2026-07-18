using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    public static class StylizedSurfaceDetailLibraryBuilder
    {
        private static bool repairScheduled;
        private static bool buildInProgress;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            ScheduleRepair();
            EditorApplication.projectChanged += ScheduleRepair;
        }

        public static void ScheduleRepair()
        {
            if (repairScheduled)
            {
                return;
            }

            repairScheduled = true;
            EditorApplication.delayCall += RepairAllLibraries;
        }

        public static bool NeedsRebuild(
            StylizedSurfaceDetailLibrary library)
        {
            if (library == null)
            {
                return false;
            }

            string signature = CalculateSignature(library);
            Texture2DArray array = library.GeneratedTextureArray;
            return array == null ||
                   array.width != library.SliceResolution ||
                   array.height != library.SliceResolution ||
                   array.depth != library.Entries.Count ||
                   !string.Equals(
                       signature,
                       library.GeneratedSignature,
                       StringComparison.Ordinal);
        }

        public static IReadOnlyList<string> Validate(
            StylizedSurfaceDetailLibrary library)
        {
            List<string> messages = new List<string>();
            if (library == null)
            {
                messages.Add("The detail library is missing.");
                return messages;
            }

            if (library.Entries.Count == 0)
            {
                messages.Add("The detail library has no entries.");
                return messages;
            }

            HashSet<string> ids =
                new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null)
                {
                    messages.Add($"Entry {index} is null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.StableId))
                {
                    messages.Add($"Entry {index} has no stable ID.");
                }
                else if (!ids.Add(entry.StableId))
                {
                    messages.Add(
                        $"Stable ID '{entry.StableId}' is duplicated.");
                }

                Texture2D source = entry.SourceTexture;
                if (source == null)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' has no source texture.");
                    continue;
                }

                if (source.width != library.SliceResolution ||
                    source.height != library.SliceResolution)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' is {source.width}×{source.height}; expected {library.SliceResolution}×{library.SliceResolution}.");
                }

                string path = AssetDatabase.GetAssetPath(source);
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' is not backed by a TextureImporter asset.");
                    continue;
                }

                if (importer.sRGBTexture)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' must use linear sampling (sRGB disabled).");
                }

                if (!importer.isReadable)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' must be Read/Write enabled for editor-time array packing.");
                }

                if (!importer.mipmapEnabled)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' must generate mipmaps.");
                }

                if (importer.wrapMode != TextureWrapMode.Repeat)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' must use Repeat wrapping.");
                }
            }

            return messages;
        }

        public static bool Rebuild(
            StylizedSurfaceDetailLibrary library,
            bool logResult = true)
        {
            if (library == null || buildInProgress)
            {
                return false;
            }

            buildInProgress = true;
            try
            {
                NormalizeSourceImporters(library);
                IReadOnlyList<string> validation = Validate(library);
                if (validation.Count > 0)
                {
                    if (logResult)
                    {
                        Debug.LogError(
                            $"Could not rebuild '{library.name}':\n- " +
                            string.Join("\n- ", validation),
                            library);
                    }

                    return false;
                }

                int resolution = library.SliceResolution;
                int depth = library.Entries.Count;
                Texture2DArray array = new Texture2DArray(
                    resolution,
                    resolution,
                    depth,
                    TextureFormat.RGBA32,
                    true,
                    true)
                {
                    name = library.name + "_PackedArray",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 1,
                    hideFlags = HideFlags.HideInHierarchy
                };

                for (int slice = 0; slice < depth; slice++)
                {
                    Texture2D source =
                        library.Entries[slice].SourceTexture;
                    int mipCount = Mathf.Min(
                        source.mipmapCount,
                        array.mipmapCount);
                    for (int mip = 0; mip < mipCount; mip++)
                    {
                        array.SetPixels(
                            source.GetPixels(mip),
                            slice,
                            mip);
                    }
                }

                array.Apply(false, true);

                string libraryPath = AssetDatabase.GetAssetPath(library);
                Texture2DArray previous = library.GeneratedTextureArray;
                library.SetGeneratedTextureArray(null, string.Empty);
                if (previous != null && AssetDatabase.IsSubAsset(previous))
                {
                    UnityEngine.Object.DestroyImmediate(previous, true);
                }

                AssetDatabase.AddObjectToAsset(array, library);
                library.SetGeneratedTextureArray(
                    array,
                    CalculateSignature(library));
                EditorUtility.SetDirty(library);
                AssetDatabase.SaveAssetIfDirty(library);
                AssetDatabase.ImportAsset(
                    libraryPath,
                    ImportAssetOptions.ForceUpdate);
                StylizedSurfaceDetailLibrary refreshedLibrary =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceDetailLibrary>(libraryPath);
                NotifyMaterialsUsingLibrary(
                    refreshedLibrary != null
                        ? refreshedLibrary
                        : library);

                if (logResult)
                {
                    Debug.Log(
                        $"Rebuilt '{library.name}' with {depth} packed detail slice(s) at {resolution}×{resolution}.",
                        library);
                }

                return true;
            }
            finally
            {
                buildInProgress = false;
            }
        }

        public static string CalculateSignature(
            StylizedSurfaceDetailLibrary library)
        {
            if (library == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            builder.Append(library.SliceResolution).Append('|');
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null)
                {
                    builder.Append("<null>|");
                    continue;
                }

                builder.Append(entry.StableId).Append('|');
                Texture2D source = entry.SourceTexture;
                string path = AssetDatabase.GetAssetPath(source);
                builder.Append(path).Append('|');
                if (!string.IsNullOrWhiteSpace(path))
                {
                    builder.Append(
                        AssetDatabase.GetAssetDependencyHash(path));
                }

                builder.Append('|');
            }

            return Hash128.Compute(builder.ToString()).ToString();
        }

        private static void RepairAllLibraries()
        {
            repairScheduled = false;
            if (buildInProgress || EditorApplication.isCompiling)
            {
                ScheduleRepair();
                return;
            }

            string[] guids = AssetDatabase.FindAssets(
                "t:StylizedSurfaceDetailLibrary");
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                StylizedSurfaceDetailLibrary library =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceDetailLibrary>(path);
                if (library != null && NeedsRebuild(library))
                {
                    Rebuild(library, false);
                }
            }
        }

        private static void NotifyMaterialsUsingLibrary(
            StylizedSurfaceDetailLibrary library)
        {
            string[] guids = AssetDatabase.FindAssets(
                "t:StylizedSurfaceMaterialProfile");
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                StylizedSurfaceMaterialProfile profile =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceMaterialProfile>(path);
                if (profile != null && profile.DetailLibrary == library)
                {
                    profile.NotifyEditorChanged();
                }
            }
        }

        private static void NormalizeSourceImporters(
            StylizedSurfaceDetailLibrary library)
        {
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                Texture2D source = entry != null
                    ? entry.SourceTexture
                    : null;
                if (source == null)
                {
                    continue;
                }

                string path = AssetDatabase.GetAssetPath(source);
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                bool changed = false;
                if (importer.sRGBTexture)
                {
                    importer.sRGBTexture = false;
                    changed = true;
                }

                if (!importer.isReadable)
                {
                    importer.isReadable = true;
                    changed = true;
                }

                if (!importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = true;
                    changed = true;
                }

                if (importer.wrapMode != TextureWrapMode.Repeat)
                {
                    importer.wrapMode = TextureWrapMode.Repeat;
                    changed = true;
                }

                if (importer.filterMode != FilterMode.Bilinear)
                {
                    importer.filterMode = FilterMode.Bilinear;
                    changed = true;
                }

                if (importer.textureCompression !=
                    TextureImporterCompression.Uncompressed)
                {
                    importer.textureCompression =
                        TextureImporterCompression.Uncompressed;
                    changed = true;
                }

                if (importer.maxTextureSize != library.SliceResolution)
                {
                    importer.maxTextureSize = library.SliceResolution;
                    changed = true;
                }

                if (importer.npotScale != TextureImporterNPOTScale.None)
                {
                    importer.npotScale = TextureImporterNPOTScale.None;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }
            }
        }
    }
}
