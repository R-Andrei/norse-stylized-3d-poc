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
            Dictionary<string, StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult>
                authoredColorBuilds =
                    new Dictionary<string, StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult>(
                        StringComparer.Ordinal);
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
                    ground.BankSurfaceLayer,
                    authoredColorBuilds);
                AppendLayerReport(
                    builder,
                    failures,
                    "RIVERBED",
                    ground.ResolvedRiverbedSurfaceLayer,
                    authoredColorBuilds);
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
                "PENDING UNITY GATES: source-driven array rebuild, shader compilation, tiled-diagnostic visual inspection, production-camera seam acceptance across scale, normal convention, wetness response, Memory Profiler evidence, and Ground-pass GPU timing.");
            return builder.ToString();
        }

        internal static void AppendLibraryBackingContractReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            StylizedSurfaceDetailLibrary library,
            IReadOnlyCollection<string> forbiddenStableIds = null)
        {
            builder.AppendLine(label);
            if (library == null)
            {
                builder.AppendLine("Library: <missing>");
                failures.Add($"{label}: detail library is missing.");
                builder.AppendLine();
                return;
            }

            string libraryPath = AssetDatabase.GetAssetPath(library);
            int logicalEntryCount = library.LogicalEntryCount;
            int requiredBackingDepth = library.RequiredPackedBackingDepth;
            builder.AppendLine($"Library: {libraryPath}");
            builder.AppendLine($"Logical entries: {logicalEntryCount}");
            builder.AppendLine(
                $"Required packed backing depth: {requiredBackingDepth}");
            builder.AppendLine(
                $"Internal neutral backing: {(library.UsesInternalNeutralBackingSlice ? "Yes" : "No")}");
            string expectedSignature =
                StylizedSurfaceDetailLibraryBuilder.CalculateSignature(library);
            bool signatureCurrent = string.Equals(
                expectedSignature,
                library.GeneratedSignature,
                StringComparison.Ordinal);
            bool rebuildRequired =
                StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library);
            builder.AppendLine($"Generated signature current: {signatureCurrent}");
            builder.AppendLine($"Rebuild required: {rebuildRequired}");
            if (!signatureCurrent || rebuildRequired)
            {
                failures.Add(
                    $"{label}: generated backing is stale after rebuild verification.");
            }

            Texture2DArray packedArray = library.GeneratedTextureArray;
            AppendArrayReport(
                builder,
                failures,
                label + " packed backing",
                packedArray,
                library.SliceResolution);
            if (packedArray != null &&
                packedArray.depth != requiredBackingDepth)
            {
                failures.Add(
                    $"{label}: packed backing depth is {packedArray.depth}; expected {requiredBackingDepth} for {logicalEntryCount} logical entries.");
            }

            int textureFormEntryCount = 0;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry != null && entry.UsesTextureForm)
                {
                    textureFormEntryCount++;
                }
            }

            Texture2DArray textureFormArray =
                library.GeneratedAuthoredColorArray;
            if (textureFormEntryCount == 0)
            {
                builder.AppendLine(
                    "Texture-form backing: <none> (expected; no texture-form entries)");
                if (textureFormArray != null)
                {
                    failures.Add(
                        $"{label}: texture-form array exists with zero texture-form entries.");
                }
            }
            else
            {
                AppendArrayReport(
                    builder,
                    failures,
                    label + " texture-form backing",
                    textureFormArray,
                    library.SliceResolution);
                if (textureFormArray != null &&
                    textureFormArray.depth != textureFormEntryCount)
                {
                    failures.Add(
                        $"{label}: texture-form backing depth is {textureFormArray.depth}; expected {textureFormEntryCount}.");
                }
            }

            int mappingCount =
                library.GeneratedAuthoredColorSliceIndices != null
                    ? library.GeneratedAuthoredColorSliceIndices.Count
                    : 0;
            builder.AppendLine(
                $"Texture-form slice mapping count: {mappingCount}");
            if (mappingCount != logicalEntryCount)
            {
                failures.Add(
                    $"{label}: texture-form slice mapping count is {mappingCount}; expected {logicalEntryCount}.");
            }

            int resolvedLogicalIds = 0;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null ||
                    string.IsNullOrWhiteSpace(entry.StableId))
                {
                    continue;
                }

                if (!library.TryResolve(
                        entry.StableId,
                        out _,
                        out _))
                {
                    failures.Add(
                        $"{label}: logical entry '{entry.StableId}' does not resolve to packed detail.");
                    continue;
                }

                resolvedLogicalIds++;
                bool textureFormResolved = library.TryResolveAuthoredColor(
                    entry.StableId,
                    out _,
                    out _);
                if (entry.UsesTextureForm != textureFormResolved)
                {
                    failures.Add(
                        $"{label}: logical entry '{entry.StableId}' texture-form resolution does not match source mode.");
                }
            }

            builder.AppendLine(
                $"Resolvable logical stable IDs: {resolvedLogicalIds}/{logicalEntryCount}");

            int resolvedForbiddenIds = 0;
            if (forbiddenStableIds != null)
            {
                foreach (string stableId in forbiddenStableIds)
                {
                    bool packedResolved = library.TryResolve(
                        stableId,
                        out _,
                        out _);
                    bool textureFormResolved =
                        library.TryResolveAuthoredColor(
                            stableId,
                            out _,
                            out _);
                    if (!packedResolved && !textureFormResolved)
                    {
                        continue;
                    }

                    resolvedForbiddenIds++;
                    failures.Add(
                        $"{label}: forbidden stable ID '{stableId}' still resolves (packed={packedResolved}, textureForm={textureFormResolved}).");
                }
            }

            builder.AppendLine(
                $"Resolvable forbidden stable IDs: {resolvedForbiddenIds}");
            builder.AppendLine();
        }

        private static void AppendLayerReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            GroundSurfaceLayerProfile layer,
            IDictionary<string, StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult>
                authoredColorBuilds)
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
                builder.AppendLine("Material source: legacy layer fallback");
                builder.AppendLine();
                return;
            }

            builder.AppendLine(
                $"Material source: {(profile.UsesTextureForm ? "Imported grayscale texture form" : "Prepacked palette detail")}");
            builder.AppendLine($"Stable entry: {profile.DetailEntryId}");
            builder.AppendLine(
                $"Application: textureForm={layer.TextureFormStrength:F3}, sceneLighting={layer.SceneLightingResponse:F3}, roughnessVariation={layer.RoughnessVariationStrength:F3}, normal={layer.DetailNormalStrength:F3}, cavity={layer.DetailCavityStrength:F3}, scale={layer.DetailWorldScale:F3}m");

            StylizedSurfaceDetailLibrary library = profile.DetailLibrary;
            if (library == null)
            {
                failures.Add($"{label}: material has no detail library.");
                builder.AppendLine("Library: <missing>");
                builder.AppendLine();
                return;
            }

            if (StylizedSurfaceDetailLibraryBuilder.NeedsRebuild(library) &&
                !StylizedSurfaceDetailLibraryBuilder.Rebuild(
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
                            $"{label}: detail-library rebuild: " +
                            rebuildFailures[index]);
                    }
                }
                else
                {
                    failures.Add(
                        $"{label}: detail-library rebuild failed without " +
                        "a detailed builder message.");
                }
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
                label + " texture form",
                library.GeneratedAuthoredColorArray,
                library.SliceResolution,
                profile.UsesTextureForm);

            bool detailResolved = profile.TryResolveDetail(
                out Texture2DArray detailArray,
                out int detailSlice);
            bool colorResolved = profile.TryResolveTextureForm(
                out Texture2DArray colorArray,
                out int colorSlice);
            builder.AppendLine(
                $"Resolved: detail={detailResolved} slice={detailSlice}; textureForm={colorResolved} slice={colorSlice}");
            if (!detailResolved)
            {
                failures.Add($"{label}: packed detail did not resolve.");
            }

            if (profile.UsesTextureForm && !colorResolved)
            {
                failures.Add($"{label}: texture form did not resolve.");
            }

            if (!profile.UsesTextureForm && colorResolved)
            {
                failures.Add(
                    $"{label}: prepacked material unexpectedly resolved texture form.");
            }

            StylizedSurfaceDetailLibrary.Entry resolvedEntry =
                AppendEntrySourceReport(
                    builder,
                    failures,
                    label,
                    library,
                    profile.DetailEntryId);
            if (profile.UsesTextureForm && resolvedEntry != null)
            {
                AppendTextureFormReport(
                    builder,
                    failures,
                    label,
                    library,
                    resolvedEntry,
                    authoredColorBuilds);
            }

            AppendControlIntegrityReport(
                builder,
                failures,
                label,
                profile,
                resolvedEntry);
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

        private static StylizedSurfaceDetailLibrary.Entry
            AppendEntrySourceReport(
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
                return null;
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
                    resolved.UsesPrepackedTextureForm);
                if (resolved.UsesFeatureTextureForm &&
                    !resolved.UsesPrepackedTextureForm)
                {
                    failures.Add(
                        $"{library.name}/{resolved.StableId}: feature-aware " +
                        "texture form is not recognized as a prepacked form.");
                }

                if (resolved.UsesPrepackedTextureForm)
                {
                    AppendSourceTexture(
                        builder,
                        failures,
                        label + " Palette Form",
                        resolved.PrepackedTextureForm,
                        true,
                        true);
                }

                return resolved;
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
            return resolved;
        }

        private static void AppendTextureFormReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            StylizedSurfaceDetailLibrary library,
            StylizedSurfaceDetailLibrary.Entry entry,
            IDictionary<string, StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult>
                authoredColorBuilds)
        {
            if (entry.UsesPrepackedTextureForm)
            {
                Texture2D pairedSource = entry.PrepackedTextureForm;
                if (pairedSource == null || !pairedSource.isReadable)
                {
                    failures.Add(
                        $"{label}: paired Palette Form source is missing or not readable.");
                    return;
                }

                builder.AppendLine(
                    $"Texture-form source: pre-normalized paired payload, algorithm {StylizedSurfaceDetailLibraryBuilder.PrepackedTextureFormAlgorithmVersion}");
                if (entry.UsesFeatureTextureForm)
                {
                    AppendFeatureTextureFormPayloadReport(
                        builder,
                        failures,
                        label,
                        pairedSource.GetPixels32(0));
                }
                else
                {
                    AppendTextureFormBandCoverage(
                        builder,
                        failures,
                        label,
                        pairedSource.GetPixels(0),
                        false);
                }

                return;
            }

            Texture2D source = entry.AuthoredBaseColor;
            if (source == null)
            {
                failures.Add(
                    $"{label}: texture-form diagnostics have no base-colour source.");
                return;
            }

            builder.AppendLine(
                $"Texture-form generation algorithm: {StylizedSurfaceDetailLibraryBuilder.AuthoredColorGenerationAlgorithmVersion}");
            string diagnosticKey =
                AssetDatabase.GetAssetPath(library) + "|" + entry.StableId;
            bool generatedNow = !authoredColorBuilds.TryGetValue(
                diagnosticKey,
                out StylizedSurfaceDetailLibraryBuilder.AuthoredColorBuildResult
                    result);
            if (generatedNow)
            {
                result = StylizedSurfaceDetailLibraryBuilder
                    .BuildAuthoredColorMipChain(
                        source,
                        library.SliceResolution);
                authoredColorBuilds.Add(diagnosticKey, result);
            }
            builder.AppendLine(
                $"Texture-form normalization (linear luminance): p05={result.FormLowPercentile:F5}, median={result.FormMedian:F5}, p95={result.FormHighPercentile:F5}");
            AppendTextureFormBandCoverage(
                builder,
                failures,
                label,
                result.MipPixels[0],
                true);

            for (int mip = 0; mip < result.MipPixels.Count; mip++)
            {
                StylizedSurfaceDetailLibraryBuilder.PeriodicSeamMetrics
                    before = result.BeforeRepair[mip];
                StylizedSurfaceDetailLibraryBuilder.PeriodicSeamMetrics
                    after = result.AfterRepair[mip];
                bool passes = StylizedSurfaceDetailLibraryBuilder
                    .PassesPeriodicSeamThresholds(after);
                builder.AppendLine(
                    $"Texture-form mip {mip} ({result.MipWidths[mip]}x{result.MipHeights[mip]}): repairLR={result.LeftRightRepairApplied[mip]}, repairTB={result.TopBottomRepairApplied[mip]}, " +
                    $"before[LR mean={before.LeftRightMean:F5} ratio={before.LeftRightMeanRatio:F3}, p95={before.LeftRightP95:F5} ratio={before.LeftRightP95Ratio:F3}; " +
                    $"TB mean={before.TopBottomMean:F5} ratio={before.TopBottomMeanRatio:F3}, p95={before.TopBottomP95:F5} ratio={before.TopBottomP95Ratio:F3}], " +
                    $"after[LR mean={after.LeftRightMean:F5} ratio={after.LeftRightMeanRatio:F3}, p95={after.LeftRightP95:F5} ratio={after.LeftRightP95Ratio:F3}; " +
                    $"TB mean={after.TopBottomMean:F5} ratio={after.TopBottomMeanRatio:F3}, p95={after.TopBottomP95:F5} ratio={after.TopBottomP95Ratio:F3}], pass={passes}");
                if (!passes)
                {
                    failures.Add(
                        $"{label}: texture-form mip {mip} exceeds periodic seam limits (mean ratio <= {StylizedSurfaceDetailLibraryBuilder.AuthoredColorSeamMeanRatioLimit:F2}, p95 ratio <= {StylizedSurfaceDetailLibraryBuilder.AuthoredColorSeamP95RatioLimit:F2}).");
                }
            }

            if (!generatedNow)
            {
                builder.AppendLine(
                    "Texture-form tiled diagnostics: already emitted for the shared material entry.");
                return;
            }

            string prefix =
                SanitizeFileName(library.name) + "_" +
                SanitizeFileName(entry.StableId);
            try
            {
                string sourcePath = WriteTiledDiagnosticPng(
                    prefix + "_SourceBaseColor_3x3.png",
                    result.SourceDerivedBasePixels,
                    result.MipWidths[0],
                    result.MipHeights[0]);
                builder.AppendLine(
                    $"Source base-colour 3x3 diagnostic: {sourcePath}");
                string normalizedPath = WriteTiledDiagnosticPng(
                    prefix + "_NormalizedFormBase_3x3.png",
                    result.NormalizedFormBasePixels,
                    result.MipWidths[0],
                    result.MipHeights[0]);
                builder.AppendLine(
                    $"Normalized grayscale form 3x3 diagnostic: {normalizedPath}");

                int diagnosticMipCount = Mathf.Min(4, result.MipPixels.Count);
                for (int mip = 0; mip < diagnosticMipCount; mip++)
                {
                    string generatedPath = WriteTiledDiagnosticPng(
                        prefix + $"_GeneratedForm_Mip{mip}_3x3.png",
                        result.MipPixels[mip],
                        result.MipWidths[mip],
                        result.MipHeights[mip]);
                    builder.AppendLine(
                        $"Texture-form generated mip {mip} 3x3 diagnostic: {generatedPath}");
                }
            }
            catch (Exception exception)
            {
                failures.Add(
                    $"{label}: could not write texture-form tiled diagnostics: {exception.Message}");
            }
        }

        private static void AppendFeatureTextureFormPayloadReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            IReadOnlyList<Color32> pixels)
        {
            if (pixels == null || pixels.Count == 0)
            {
                failures.Add(
                    $"{label}: feature-aware Palette Form has no pixels.");
                return;
            }

            int dark = 0;
            int baseline = 0;
            int light = 0;
            double featureSum = 0.0;
            double substrateFormSum = 0.0;
            double substrateRoughnessSum = 0.0;
            float featureMaximum = 0f;
            float combinedMinimum = 1f;
            float combinedMaximum = 0f;
            for (int index = 0; index < pixels.Count; index++)
            {
                Color32 pixel = pixels[index];
                float combinedForm =
                    StylizedSurfaceDetailLibraryBuilder.DecodeSrgbByte(
                        pixel.r);
                float substrateForm =
                    StylizedSurfaceDetailLibraryBuilder.DecodeSrgbByte(
                        pixel.g);
                float substrateRoughness =
                    StylizedSurfaceDetailLibraryBuilder.DecodeSrgbByte(
                        pixel.b);
                float feature = pixel.a / 255f;
                combinedMinimum = Mathf.Min(
                    combinedMinimum,
                    combinedForm);
                combinedMaximum = Mathf.Max(
                    combinedMaximum,
                    combinedForm);
                substrateFormSum += substrateForm;
                substrateRoughnessSum += substrateRoughness;
                featureSum += feature;
                featureMaximum = Mathf.Max(featureMaximum, feature);
                if (combinedForm < 0.45f)
                {
                    dark++;
                }
                else if (combinedForm <= 0.55f)
                {
                    baseline++;
                }
                else
                {
                    light++;
                }
            }

            float inverseCount = 1f / pixels.Count;
            float featureMean = (float)(featureSum * inverseCount);
            float substrateFormMean =
                (float)(substrateFormSum * inverseCount);
            float substrateRoughnessMean =
                (float)(substrateRoughnessSum * inverseCount);
            float count = pixels.Count;
            builder.AppendLine(
                $"Feature-aware combined form min/max: " +
                $"{combinedMinimum:F5}/{combinedMaximum:F5}");
            builder.AppendLine(
                $"Feature-aware palette-band coverage: Dark=" +
                $"{dark * 100f / count:F2}%, Base=" +
                $"{baseline * 100f / count:F2}%, Light=" +
                $"{light * 100f / count:F2}%");
            builder.AppendLine(
                $"Feature mask mean/max: " +
                $"{featureMean:F5}/{featureMaximum:F5}");
            builder.AppendLine(
                $"Substrate-only form/roughness means (linear): " +
                $"{substrateFormMean:F5}/{substrateRoughnessMean:F5}");

            if (featureMaximum <
                    StylizedSurfaceDetailLibraryBuilder
                        .MinimumFeatureTextureFormMaximum ||
                featureMean <
                    StylizedSurfaceDetailLibraryBuilder
                        .MinimumFeatureTextureFormMean ||
                featureMean >
                    StylizedSurfaceDetailLibraryBuilder
                        .MaximumFeatureTextureFormMean)
            {
                failures.Add(
                    $"{label}: feature mask mean/max is " +
                    $"{featureMean:F5}/{featureMaximum:F5}; expected mean " +
                    $"{StylizedSurfaceDetailLibraryBuilder.MinimumFeatureTextureFormMean:F5}–" +
                    $"{StylizedSurfaceDetailLibraryBuilder.MaximumFeatureTextureFormMean:F5} and maximum at least " +
                    $"{StylizedSurfaceDetailLibraryBuilder.MinimumFeatureTextureFormMaximum:F5}.");
            }

            if (substrateFormMean <
                    StylizedSurfaceDetailLibraryBuilder
                        .MinimumFeatureSubstrateFormMean ||
                substrateFormMean >
                    StylizedSurfaceDetailLibraryBuilder
                        .MaximumFeatureSubstrateFormMean ||
                substrateRoughnessMean <
                    StylizedSurfaceDetailLibraryBuilder
                        .MinimumFeatureSubstrateRoughnessMean ||
                substrateRoughnessMean >
                    StylizedSurfaceDetailLibraryBuilder
                        .MaximumFeatureSubstrateRoughnessMean)
            {
                failures.Add(
                    $"{label}: substrate-only form/roughness means are " +
                    $"{substrateFormMean:F5}/{substrateRoughnessMean:F5}; " +
                    $"expected form " +
                    $"{StylizedSurfaceDetailLibraryBuilder.MinimumFeatureSubstrateFormMean:F2}–" +
                    $"{StylizedSurfaceDetailLibraryBuilder.MaximumFeatureSubstrateFormMean:F2} and roughness " +
                    $"{StylizedSurfaceDetailLibraryBuilder.MinimumFeatureSubstrateRoughnessMean:F2}–" +
                    $"{StylizedSurfaceDetailLibraryBuilder.MaximumFeatureSubstrateRoughnessMean:F2}.");
            }
        }

        private static void AppendTextureFormBandCoverage(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            IReadOnlyList<Color> formPixels,
            bool pixelsAreGammaEncoded)
        {
            if (formPixels == null || formPixels.Count == 0)
            {
                failures.Add($"{label}: normalized texture form has no pixels.");
                return;
            }

            int dark = 0;
            int baseline = 0;
            int light = 0;
            float maximumChannelDelta = 0f;
            for (int index = 0; index < formPixels.Count; index++)
            {
                Color encoded = formPixels[index];
                maximumChannelDelta = Mathf.Max(
                    maximumChannelDelta,
                    Mathf.Abs(encoded.r - encoded.g),
                    Mathf.Abs(encoded.r - encoded.b),
                    Mathf.Abs(encoded.g - encoded.b));
                float value = pixelsAreGammaEncoded
                    ? StylizedSurfaceDetailLibraryBuilder
                        .DecodeFormValue(encoded)
                    : Mathf.Clamp01(encoded.r);
                if (value < 0.45f)
                {
                    dark++;
                }
                else if (value <= 0.55f)
                {
                    baseline++;
                }
                else
                {
                    light++;
                }
            }

            float count = formPixels.Count;
            float darkPercent = dark * 100f / count;
            float baselinePercent = baseline * 100f / count;
            float lightPercent = light * 100f / count;
            builder.AppendLine(
                $"Texture-form chroma maximum channel delta: {maximumChannelDelta:F6}");
            builder.AppendLine(
                $"Palette-band coverage: Dark={darkPercent:F2}%, Base={baselinePercent:F2}%, Light={lightPercent:F2}%");
            if (maximumChannelDelta > 1f / 255f)
            {
                failures.Add(
                    $"{label}: generated texture form retains RGB chroma (max channel delta {maximumChannelDelta:F6}).");
            }
            if (darkPercent < 5f)
            {
                failures.Add(
                    $"{label}: Dark palette band receives only {darkPercent:F2}% of normalized form pixels.");
            }
            if (baselinePercent < 1f)
            {
                failures.Add(
                    $"{label}: Base palette band receives only {baselinePercent:F2}% of normalized form pixels.");
            }
            if (lightPercent < 5f)
            {
                failures.Add(
                    $"{label}: Light palette band receives only {lightPercent:F2}% of normalized form pixels.");
            }
        }

        private static void AppendControlIntegrityReport(
            StringBuilder builder,
            ICollection<string> failures,
            string label,
            StylizedSurfaceMaterialProfile profile,
            StylizedSurfaceDetailLibrary.Entry entry)
        {
            builder.AppendLine("CONTROL INTEGRITY");
            builder.AppendLine(
                "Colour authority: Base / Dark / Light / Cavity palette only");
            builder.AppendLine("Imported source hue contribution: 0.000");
            builder.AppendLine(
                "Texture-form capability selection: automatic from detail entry source mode");
            builder.AppendLine(
                "Dry Smoothness baseline coefficient: 1.000 at every Roughness Variation value");

            bool entryUsesTextureForm =
                entry != null && entry.UsesTextureForm;
            if (profile.UsesTextureForm != entryUsesTextureForm)
            {
                failures.Add(
                    $"{label}: resolved texture-form capability does not match the selected detail entry.");
            }

            List<string> activeControls = new List<string>();
            if (profile.DetailEnabled)
            {
                if (profile.UsesTextureForm &&
                    profile.TextureFormStrength > 0.0001f)
                {
                    activeControls.Add("Texture Form Strength");
                    if (profile.SceneLightingResponse > 0.0001f)
                    {
                        activeControls.Add("Scene Lighting Response");
                    }
                }
                if (profile.DetailNormalStrength > 0.0001f)
                {
                    activeControls.Add("Normal Strength");
                }
                if (profile.DetailCavityStrength > 0.0001f)
                {
                    activeControls.Add("Cavity Strength");
                }
                if (!profile.UsesTextureForm &&
                    (profile.DetailValueStrength > 0.0001f ||
                     profile.DetailFormHighlightStrength > 0.0001f))
                {
                    activeControls.Add("Packed Value / Form");
                }
                if (profile.UsesTextureForm &&
                    profile.RoughnessVariationStrength > 0.0001f)
                {
                    activeControls.Add("Roughness Variation");
                }
                else if (!profile.UsesTextureForm &&
                         profile.FinishVariationStrength > 0.0001f)
                {
                    activeControls.Add("Finish Variation");
                }
            }
            if (profile.LegacyPixelCellInfluence > 0.0001f)
            {
                activeControls.Add("Legacy Cell Influence");
            }

            builder.AppendLine(
                activeControls.Count > 0
                    ? "Active application controls: " +
                      string.Join(", ", activeControls)
                    : "Active application controls: none");
        }

        private static string WriteTiledDiagnosticPng(
            string fileName,
            Color[] pixels,
            int width,
            int height)
        {
            const int displayedTileSize = 256;
            const int tileCount = 3;
            int outputSize = displayedTileSize * tileCount;
            Color[] output = new Color[outputSize * outputSize];
            for (int y = 0; y < outputSize; y++)
            {
                int localY = y % displayedTileSize;
                int sourceY = Mathf.Clamp(
                    Mathf.FloorToInt(
                        localY * height / (float)displayedTileSize),
                    0,
                    height - 1);
                for (int x = 0; x < outputSize; x++)
                {
                    int localX = x % displayedTileSize;
                    int sourceX = Mathf.Clamp(
                        Mathf.FloorToInt(
                            localX * width / (float)displayedTileSize),
                        0,
                        width - 1);
                    output[y * outputSize + x] =
                        pixels[sourceY * width + sourceX];
                }
            }

            Texture2D diagnostic = new Texture2D(
                outputSize,
                outputSize,
                TextureFormat.RGBA32,
                false,
                false)
            {
                name = "Surface Material Periodic Diagnostic",
                hideFlags = HideFlags.HideAndDontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Point
            };
            try
            {
                diagnostic.SetPixels(output);
                diagnostic.Apply(false, false);
                Directory.CreateDirectory(OutputDirectory);
                string path = Path.Combine(OutputDirectory, fileName);
                File.WriteAllBytes(path, diagnostic.EncodeToPNG());
                return path;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(diagnostic);
            }
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
