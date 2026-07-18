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
            Texture2DArray detailArray = library.GeneratedTextureArray;
            bool detailInvalid =
                detailArray == null ||
                detailArray.width != library.SliceResolution ||
                detailArray.height != library.SliceResolution ||
                detailArray.depth != library.Entries.Count;

            int authoredEntryCount = CountAuthoredColorEntries(library);
            Texture2DArray colorArray =
                library.GeneratedAuthoredColorArray;
            bool colorInvalid = authoredEntryCount == 0
                ? colorArray != null
                : colorArray == null ||
                  colorArray.width != library.SliceResolution ||
                  colorArray.height != library.SliceResolution ||
                  colorArray.depth != authoredEntryCount;
            bool mappingInvalid =
                library.GeneratedAuthoredColorSliceIndices == null ||
                library.GeneratedAuthoredColorSliceIndices.Count !=
                library.Entries.Count;

            return detailInvalid ||
                   colorInvalid ||
                   mappingInvalid ||
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

                if (entry.UsesAuthoredMaterialSet)
                {
                    ValidateAuthoredMaterialEntry(
                        library,
                        entry,
                        messages);
                }
                else
                {
                    ValidatePrepackedEntry(
                        library,
                        entry,
                        messages);
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
                Texture2DArray detailArray = new Texture2DArray(
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

                int authoredColorCount = CountAuthoredColorEntries(library);
                Texture2DArray authoredColorArray = authoredColorCount > 0
                    ? new Texture2DArray(
                        resolution,
                        resolution,
                        authoredColorCount,
                        TextureFormat.RGBA32,
                        true,
                        false)
                    {
                        name = library.name + "_AuthoredColorArray",
                        wrapMode = TextureWrapMode.Repeat,
                        filterMode = FilterMode.Bilinear,
                        anisoLevel = 1,
                        hideFlags = HideFlags.HideInHierarchy
                    }
                    : null;

                List<int> authoredColorSliceIndices =
                    new List<int>(depth);
                int authoredColorSlice = 0;
                for (int slice = 0; slice < depth; slice++)
                {
                    StylizedSurfaceDetailLibrary.Entry entry =
                        library.Entries[slice];
                    if (entry.UsesAuthoredMaterialSet)
                    {
                        CopyGeneratedMipChain(
                            detailArray,
                            slice,
                            BuildPackedMaterialPixels(
                                entry,
                                resolution),
                            resolution,
                            true);
                        CopyGeneratedMipChain(
                            authoredColorArray,
                            authoredColorSlice,
                            ResamplePixels(
                                entry.AuthoredBaseColor,
                                resolution),
                            resolution,
                            false);
                        authoredColorSliceIndices.Add(
                            authoredColorSlice);
                        authoredColorSlice++;
                    }
                    else
                    {
                        Texture2D source = entry.SourceTexture;
                        int mipCount = Mathf.Min(
                            source.mipmapCount,
                            detailArray.mipmapCount);
                        for (int mip = 0; mip < mipCount; mip++)
                        {
                            detailArray.SetPixels(
                                source.GetPixels(mip),
                                slice,
                                mip);
                        }

                        authoredColorSliceIndices.Add(-1);
                    }
                }

                detailArray.Apply(false, true);
                authoredColorArray?.Apply(false, true);

                string libraryPath = AssetDatabase.GetAssetPath(library);
                Texture2DArray previousDetail =
                    library.GeneratedTextureArray;
                Texture2DArray previousColor =
                    library.GeneratedAuthoredColorArray;
                library.SetGeneratedArrays(
                    null,
                    null,
                    Array.Empty<int>(),
                    string.Empty);
                DestroyGeneratedSubAsset(previousDetail);
                DestroyGeneratedSubAsset(previousColor);

                AssetDatabase.AddObjectToAsset(detailArray, library);
                if (authoredColorArray != null)
                {
                    AssetDatabase.AddObjectToAsset(
                        authoredColorArray,
                        library);
                }

                library.SetGeneratedArrays(
                    detailArray,
                    authoredColorArray,
                    authoredColorSliceIndices,
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
                        $"Rebuilt '{library.name}' with {depth} packed detail slice(s) and {authoredColorCount} authored colour slice(s) at {resolution}×{resolution}.",
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
                builder.Append((int)entry.SourceMode).Append('|');
                if (entry.UsesAuthoredMaterialSet)
                {
                    AppendTextureSignature(
                        builder,
                        entry.AuthoredBaseColor);
                    AppendTextureSignature(
                        builder,
                        entry.AuthoredNormal);
                    AppendTextureSignature(
                        builder,
                        entry.AuthoredHeight);
                    AppendTextureSignature(
                        builder,
                        entry.AuthoredAmbientOcclusion);
                    AppendTextureSignature(
                        builder,
                        entry.AuthoredRoughness);
                    builder.Append(entry.FlipAuthoredNormalGreen).Append('|');
                    builder.Append(entry.AuthoredHeightCavityWeight).Append('|');
                    builder.Append(
                        entry.AuthoredAmbientOcclusionCavityWeight).Append('|');
                    builder.Append(entry.AuthoredCavityFloor).Append('|');
                }
                else
                {
                    AppendTextureSignature(
                        builder,
                        entry.SourceTexture);
                }
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

        private static int CountAuthoredColorEntries(
            StylizedSurfaceDetailLibrary library)
        {
            int count = 0;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry != null &&
                    entry.UsesAuthoredMaterialSet &&
                    entry.AuthoredBaseColor != null)
                {
                    count++;
                }
            }

            return count;
        }

        private static void ValidatePrepackedEntry(
            StylizedSurfaceDetailLibrary library,
            StylizedSurfaceDetailLibrary.Entry entry,
            ICollection<string> messages)
        {
            Texture2D source = entry.SourceTexture;
            if (source == null)
            {
                messages.Add(
                    $"Entry '{entry.DisplayName}' has no prepacked source texture.");
                return;
            }

            if (source.width != library.SliceResolution ||
                source.height != library.SliceResolution)
            {
                messages.Add(
                    $"Entry '{entry.DisplayName}' is {source.width}×{source.height}; expected {library.SliceResolution}×{library.SliceResolution}.");
            }

            ValidateImporter(
                source,
                entry.DisplayName,
                false,
                true,
                messages);
        }

        private static void ValidateAuthoredMaterialEntry(
            StylizedSurfaceDetailLibrary library,
            StylizedSurfaceDetailLibrary.Entry entry,
            ICollection<string> messages)
        {
            Texture2D[] required =
            {
                entry.AuthoredBaseColor,
                entry.AuthoredNormal,
                entry.AuthoredHeight,
                entry.AuthoredAmbientOcclusion,
                entry.AuthoredRoughness
            };
            string[] labels =
            {
                "base colour",
                "normal",
                "height",
                "ambient occlusion",
                "roughness"
            };

            int width = -1;
            int height = -1;
            for (int index = 0; index < required.Length; index++)
            {
                Texture2D texture = required[index];
                if (texture == null)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' has no authored {labels[index]} source.");
                    continue;
                }

                if (width < 0)
                {
                    width = texture.width;
                    height = texture.height;
                }
                else if (texture.width != width ||
                         texture.height != height)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' authored maps must share dimensions; '{labels[index]}' is {texture.width}×{texture.height}, expected {width}×{height}.");
                }

                ValidateImporter(
                    texture,
                    entry.DisplayName + " " + labels[index],
                    index == 0,
                    false,
                    messages);
            }
        }

        private static void ValidateImporter(
            Texture2D texture,
            string label,
            bool expectedSrgb,
            bool requireMipmaps,
            ICollection<string> messages)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                messages.Add(
                    $"'{label}' is not backed by a TextureImporter asset.");
                return;
            }

            if (importer.sRGBTexture != expectedSrgb)
            {
                messages.Add(
                    $"'{label}' must use {(expectedSrgb ? "sRGB" : "linear")} sampling.");
            }

            if (!importer.isReadable)
            {
                messages.Add(
                    $"'{label}' must be Read/Write enabled for editor-time array generation.");
            }

            if (requireMipmaps && !importer.mipmapEnabled)
            {
                messages.Add($"'{label}' must generate mipmaps.");
            }

            if (importer.wrapMode != TextureWrapMode.Repeat)
            {
                messages.Add($"'{label}' must use Repeat wrapping.");
            }
        }

        private static Color[] BuildPackedMaterialPixels(
            StylizedSurfaceDetailLibrary.Entry entry,
            int resolution)
        {
            Color[] pixels = new Color[resolution * resolution];
            float heightWeight = entry.AuthoredHeightCavityWeight;
            float aoWeight =
                entry.AuthoredAmbientOcclusionCavityWeight;
            float cavityFloor = entry.AuthoredCavityFloor;

            for (int y = 0; y < resolution; y++)
            {
                float v = (y + 0.5f) / resolution;
                for (int x = 0; x < resolution; x++)
                {
                    float u = (x + 0.5f) / resolution;
                    Color normalSample =
                        entry.AuthoredNormal.GetPixelBilinear(u, v);
                    Vector3 normal = new Vector3(
                        normalSample.r * 2f - 1f,
                        normalSample.g * 2f - 1f,
                        normalSample.b * 2f - 1f);
                    if (entry.FlipAuthoredNormalGreen)
                    {
                        normal.y = -normal.y;
                    }

                    if (normal.sqrMagnitude < 0.000001f)
                    {
                        normal = Vector3.forward;
                    }
                    else
                    {
                        normal.Normalize();
                    }

                    float safeZ = Mathf.Max(0.25f, Mathf.Abs(normal.z));
                    Vector2 slope = Vector2.ClampMagnitude(
                        new Vector2(normal.x, normal.y) / safeZ,
                        1f);

                    float heightValue = SampleLuminance(
                        entry.AuthoredHeight,
                        u,
                        v);
                    float aoValue = SampleLuminance(
                        entry.AuthoredAmbientOcclusion,
                        u,
                        v);
                    float aoCavity = 1f - aoValue;
                    float heightCavity = 1f - heightValue;
                    float cavity = Mathf.Clamp01(
                        aoCavity * aoWeight +
                        heightCavity * aoCavity * heightWeight);
                    cavity = Mathf.Clamp01(
                        (cavity - cavityFloor) /
                        Mathf.Max(0.001f, 1f - cavityFloor));
                    float roughness = SampleLuminance(
                        entry.AuthoredRoughness,
                        u,
                        v);

                    pixels[y * resolution + x] = new Color(
                        slope.x * 0.5f + 0.5f,
                        slope.y * 0.5f + 0.5f,
                        cavity,
                        roughness);
                }
            }

            return pixels;
        }

        private static void CopyGeneratedMipChain(
            Texture2DArray destination,
            int slice,
            Color[] basePixels,
            int resolution,
            bool linear)
        {
            Texture2D temporary = new Texture2D(
                resolution,
                resolution,
                TextureFormat.RGBA32,
                true,
                linear)
            {
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            temporary.SetPixels(basePixels, 0);
            temporary.Apply(true, false);
            int mipCount = Mathf.Min(
                temporary.mipmapCount,
                destination.mipmapCount);
            for (int mip = 0; mip < mipCount; mip++)
            {
                destination.SetPixels(
                    temporary.GetPixels(mip),
                    slice,
                    mip);
            }

            UnityEngine.Object.DestroyImmediate(temporary);
        }

        private static Color[] ResamplePixels(
            Texture2D source,
            int resolution)
        {
            Color[] pixels = new Color[resolution * resolution];
            for (int y = 0; y < resolution; y++)
            {
                float v = (y + 0.5f) / resolution;
                for (int x = 0; x < resolution; x++)
                {
                    float u = (x + 0.5f) / resolution;
                    Color sample = source.GetPixelBilinear(u, v);
                    sample.a = 1f;
                    pixels[y * resolution + x] = sample;
                }
            }

            return pixels;
        }

        private static float SampleLuminance(
            Texture2D source,
            float u,
            float v)
        {
            Color sample = source.GetPixelBilinear(u, v);
            return Mathf.Clamp01(
                sample.r * 0.2126f +
                sample.g * 0.7152f +
                sample.b * 0.0722f);
        }

        private static void NormalizeSourceImporters(
            StylizedSurfaceDetailLibrary library)
        {
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null)
                {
                    continue;
                }

                if (entry.UsesAuthoredMaterialSet)
                {
                    NormalizeImporter(entry.AuthoredBaseColor, true, false);
                    NormalizeImporter(entry.AuthoredNormal, false, false);
                    NormalizeImporter(entry.AuthoredHeight, false, false);
                    NormalizeImporter(
                        entry.AuthoredAmbientOcclusion,
                        false,
                        false);
                    NormalizeImporter(entry.AuthoredRoughness, false, false);
                }
                else
                {
                    NormalizeImporter(
                        entry.SourceTexture,
                        false,
                        true,
                        library.SliceResolution);
                }
            }
        }

        private static void NormalizeImporter(
            Texture2D source,
            bool sRgb,
            bool mipmaps,
            int? maxTextureSize = null)
        {
            if (source == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(source);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Default)
            {
                importer.textureType = TextureImporterType.Default;
                changed = true;
            }

            if (importer.sRGBTexture != sRgb)
            {
                importer.sRGBTexture = sRgb;
                changed = true;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

            if (importer.mipmapEnabled != mipmaps)
            {
                importer.mipmapEnabled = mipmaps;
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

            if (maxTextureSize.HasValue &&
                importer.maxTextureSize != maxTextureSize.Value)
            {
                importer.maxTextureSize = maxTextureSize.Value;
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

        private static void AppendTextureSignature(
            StringBuilder builder,
            Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            builder.Append(path).Append('|');
            if (!string.IsNullOrWhiteSpace(path))
            {
                builder.Append(
                    AssetDatabase.GetAssetDependencyHash(path));
            }

            builder.Append('|');
        }

        private static void DestroyGeneratedSubAsset(
            Texture2DArray array)
        {
            if (array != null && AssetDatabase.IsSubAsset(array))
            {
                UnityEngine.Object.DestroyImmediate(array, true);
            }
        }
    }
}
