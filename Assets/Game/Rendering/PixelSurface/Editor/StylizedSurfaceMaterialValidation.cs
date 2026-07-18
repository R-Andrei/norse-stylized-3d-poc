using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    public static class StylizedSurfaceMaterialValidation
    {
        private const string OutputDirectory =
            "Library/SurfaceMaterialDiagnostics";

        public static string RunAndCopy(GeneratedGround ground)
        {
            string report = BuildReport(ground);
            Directory.CreateDirectory(OutputDirectory);
            string safeName = ground != null
                ? SanitizeFileName(ground.name)
                : "MissingGround";
            string path = Path.Combine(
                OutputDirectory,
                safeName + "_SurfaceMaterialValidation.txt");
            File.WriteAllText(path, report, Encoding.UTF8);
            EditorGUIUtility.systemCopyBuffer = report;
            Debug.Log(
                $"[Surface Material Validation] Report written to {path} and copied to the clipboard.",
                ground);
            return path;
        }

        public static string BuildReport(GeneratedGround ground)
        {
            StringBuilder builder = new StringBuilder(4096);
            List<string> failures = new List<string>();
            builder.AppendLine("SURFACE MATERIAL COMPREHENSIVE VALIDATION");
            builder.AppendLine($"Generated UTC: {DateTime.UtcNow:O}");
            builder.AppendLine($"Unity: {Application.unityVersion}");
            builder.AppendLine($"Ground: {(ground != null ? ground.name : "<missing>")}");
            builder.AppendLine();

            if (ground == null)
            {
                failures.Add("No GeneratedGround was supplied.");
            }
            else
            {
                AppendLayerReport(
                    builder,
                    failures,
                    "BANK",
                    ground.BankSurfaceLayer);
                AppendLayerReport(
                    builder,
                    failures,
                    "RIVERBED",
                    ground.ResolvedRiverbedSurfaceLayer);
            }

            builder.AppendLine("SUMMARY");
            builder.AppendLine(
                failures.Count == 0
                    ? "VERDICT: PASS — static editor/material contract checks passed."
                    : $"VERDICT: FAIL — {failures.Count} issue(s) detected.");
            for (int index = 0; index < failures.Count; index++)
            {
                builder.AppendLine($"- {failures[index]}");
            }

            builder.AppendLine();
            builder.AppendLine(
                "PENDING UNITY GATES: shader compilation, production-camera visual acceptance, normal convention, wetness response, Memory Profiler evidence, and Ground-pass GPU timing.");
            return builder.ToString();
        }

        private static void AppendLayerReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            GroundSurfaceLayerProfile layer)
        {
            builder.AppendLine(label);
            if (layer == null)
            {
                builder.AppendLine("Layer: <Primary Ground / none>");
                builder.AppendLine();
                return;
            }

            StylizedSurfaceMaterialProfile profile = layer.SurfaceMaterial;
            builder.AppendLine($"Layer: {AssetDatabase.GetAssetPath(layer)}");
            builder.AppendLine(
                $"Material: {(profile != null ? AssetDatabase.GetAssetPath(profile) : "<legacy fallback>")}");
            if (profile == null)
            {
                builder.AppendLine("Payload: legacy layer fallback");
                builder.AppendLine();
                return;
            }

            builder.AppendLine($"Payload: {profile.PayloadMode}");
            builder.AppendLine($"Stable entry: {profile.DetailEntryId}");
            builder.AppendLine(
                $"Application: authoredStrength={layer.AuthoredColorStrength:F3}, authoredLighting={layer.AuthoredColorLightingStrength:F3}, normal={layer.DetailNormalStrength:F3}, cavity={layer.DetailCavityStrength:F3}, scale={layer.DetailWorldScale:F3}m");

            StylizedSurfaceDetailLibrary library = profile.DetailLibrary;
            if (library == null)
            {
                failures.Add($"{label}: material has no detail library.");
                builder.AppendLine("Library: <missing>");
                builder.AppendLine();
                return;
            }

            if (StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library))
            {
                StylizedSurfaceDetailLibraryBuilder.Rebuild(
                    library,
                    false);
            }

            builder.AppendLine($"Library: {AssetDatabase.GetAssetPath(library)}");
            AppendArrayReport(
                builder,
                failures,
                label + " packed detail",
                library.GeneratedTextureArray,
                library.SliceResolution);
            AppendArrayReport(
                builder,
                failures,
                label + " authored colour",
                library.GeneratedAuthoredColorArray,
                library.SliceResolution,
                profile.UsesAuthoredColor);

            bool detailResolved = profile.TryResolveDetail(
                out Texture2DArray detailArray,
                out int detailSlice);
            bool colorResolved = profile.TryResolveAuthoredColor(
                out Texture2DArray colorArray,
                out int colorSlice);
            builder.AppendLine(
                $"Resolved: detail={detailResolved} slice={detailSlice}; authoredColor={colorResolved} slice={colorSlice}");
            if (!detailResolved)
            {
                failures.Add($"{label}: packed detail did not resolve.");
            }

            if (profile.UsesAuthoredColor && !colorResolved)
            {
                failures.Add($"{label}: authored colour did not resolve.");
            }

            if (!profile.UsesAuthoredColor && colorResolved)
            {
                failures.Add(
                    $"{label}: palette-detail material unexpectedly resolved authored colour.");
            }

            AppendEntrySourceReport(
                builder,
                failures,
                label,
                library,
                profile.DetailEntryId);
            AppendRuntimeReferenceAudit(
                builder,
                failures,
                label,
                profile,
                layer);
            builder.AppendLine();
        }

        private static void AppendArrayReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            Texture2DArray array,
            int expectedResolution,
            bool required = true)
        {
            if (array == null)
            {
                builder.AppendLine($"{label}: <missing>");
                if (required)
                {
                    failures.Add($"{label}: generated array is missing.");
                }
                return;
            }

            long estimatedBytes = EstimateRgba32MipBytes(
                array.width,
                array.height,
                array.depth);
            builder.AppendLine(
                $"{label}: {array.width}x{array.height}x{array.depth}, format={array.format}, mips={array.mipmapCount}, estimatedRGBA32={estimatedBytes / 1048576f:F3} MiB");
            if (array.width != expectedResolution ||
                array.height != expectedResolution)
            {
                failures.Add(
                    $"{label}: expected {expectedResolution}x{expectedResolution}, found {array.width}x{array.height}.");
            }
        }

        private static void AppendEntrySourceReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            StylizedSurfaceDetailLibrary library,
            string stableId)
        {
            StylizedSurfaceDetailLibrary.Entry resolved = null;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry = library.Entries[index];
                if (entry != null &&
                    string.Equals(
                        entry.StableId,
                        stableId,
                        StringComparison.Ordinal))
                {
                    resolved = entry;
                    break;
                }
            }

            if (resolved == null)
            {
                failures.Add($"{label}: stable entry '{stableId}' is missing.");
                return;
            }

            builder.AppendLine($"Source mode: {resolved.SourceMode}");
            if (!resolved.UsesAuthoredMaterialSet)
            {
                AppendSourceTexture(
                    builder,
                    failures,
                    label + " prepacked",
                    resolved.SourceTexture,
                    false,
                    false);
                return;
            }

            AppendSourceTexture(
                builder,
                failures,
                label + " base colour",
                resolved.AuthoredBaseColor,
                true,
                true);
            AppendSourceTexture(
                builder,
                failures,
                label + " normal",
                resolved.AuthoredNormal,
                false,
                true);
            AppendSourceTexture(
                builder,
                failures,
                label + " height",
                resolved.AuthoredHeight,
                false,
                true);
            AppendSourceTexture(
                builder,
                failures,
                label + " ambient occlusion",
                resolved.AuthoredAmbientOcclusion,
                false,
                true);
            AppendSourceTexture(
                builder,
                failures,
                label + " roughness",
                resolved.AuthoredRoughness,
                false,
                true);
            builder.AppendLine(
                $"Normal green flip: {resolved.FlipAuthoredNormalGreen}");
        }

        private static void AppendSourceTexture(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            Texture2D texture,
            bool expectedSrgb,
            bool requireEditorOnly)
        {
            if (texture == null)
            {
                failures.Add($"{label}: source texture is missing.");
                return;
            }

            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            bool editorOnly = path.Replace('\\', '/').Contains("/Editor/");
            builder.AppendLine(
                $"{label}: {path}, {texture.width}x{texture.height}, editorOnly={editorOnly}, sRGB={(importer != null && importer.sRGBTexture)}, readable={(importer != null && importer.isReadable)}");
            if (requireEditorOnly && !editorOnly)
            {
                failures.Add($"{label}: source is outside an Editor folder.");
            }
            if (importer == null || importer.sRGBTexture != expectedSrgb)
            {
                failures.Add(
                    $"{label}: expected {(expectedSrgb ? "sRGB" : "linear")} importer interpretation.");
            }
        }

        private static void AppendRuntimeReferenceAudit(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            params UnityEngine.Object[] assets)
        {
            int textureReferences = 0;
            for (int assetIndex = 0; assetIndex < assets.Length; assetIndex++)
            {
                UnityEngine.Object asset = assets[assetIndex];
                if (asset == null)
                {
                    continue;
                }

                SerializedObject serialized = new SerializedObject(asset);
                SerializedProperty iterator = serialized.GetIterator();
                bool enterChildren = true;
                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (iterator.propertyType !=
                        SerializedPropertyType.ObjectReference)
                    {
                        continue;
                    }

                    if (iterator.objectReferenceValue is Texture2D)
                    {
                        textureReferences++;
                    }
                }
            }

            builder.AppendLine(
                $"Runtime profile/layer direct Texture2D references: {textureReferences}");
            if (textureReferences != 0)
            {
                failures.Add(
                    $"{label}: runtime profile/layer directly references source Texture2D assets.");
            }
        }

        private static long EstimateRgba32MipBytes(
            int width,
            int height,
            int depth)
        {
            long totalPixels = 0;
            int mipWidth = Mathf.Max(1, width);
            int mipHeight = Mathf.Max(1, height);
            while (true)
            {
                totalPixels += (long)mipWidth * mipHeight;
                if (mipWidth == 1 && mipHeight == 1)
                {
                    break;
                }

                mipWidth = Mathf.Max(1, mipWidth / 2);
                mipHeight = Mathf.Max(1, mipHeight / 2);
            }

            return totalPixels * Mathf.Max(1, depth) * 4L;
        }

        private static string SanitizeFileName(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(value)
                ? "GeneratedGround"
                : value;
        }
    }
}
