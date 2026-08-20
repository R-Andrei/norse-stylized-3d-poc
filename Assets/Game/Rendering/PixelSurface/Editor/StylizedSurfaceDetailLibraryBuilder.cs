using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    public static class StylizedSurfaceDetailLibraryBuilder
    {
        internal const int AuthoredColorGenerationAlgorithmVersion = 4;
        internal const int PrepackedTextureFormAlgorithmVersion = 3;
        internal const int EmptyLibraryBackingAlgorithmVersion = 1;
        internal const float AuthoredColorSeamMeanRatioLimit = 1.15f;
        internal const float AuthoredColorSeamP95RatioLimit = 1.25f;
        internal const float MinimumFeatureTextureFormMean = 0.001f;
        internal const float MaximumFeatureTextureFormMean = 0.025f;
        internal const float MinimumFeatureTextureFormMaximum = 0.90f;
        internal const float MinimumFeatureSubstrateFormMean = 0.54f;
        internal const float MaximumFeatureSubstrateFormMean = 0.70f;
        internal const float MinimumFeatureSubstrateRoughnessMean = 0.55f;
        internal const float MaximumFeatureSubstrateRoughnessMean = 0.80f;
        internal const float MinimumFeatureMaximumSupportRadiusUv = 0.001f;
        internal const float MaximumFeatureMaximumSupportRadiusUv = 0.25f;
        internal const float MinimumFeatureAnchorDistanceMean = 0.10f;
        internal const float MaximumFeatureAnchorDistanceMean = 0.95f;

        private const int AuthoredColorSeamRepairBandAt256 = 8;
        private const float MinimumMeanBoundaryDifference = 1f / 255f;
        private const float MinimumP95BoundaryDifference = 2f / 255f;

        internal readonly struct PeriodicSeamMetrics
        {
            public PeriodicSeamMetrics(
                float leftRightMean,
                float leftRightAdjacentMean,
                float leftRightP95,
                float leftRightAdjacentP95,
                float topBottomMean,
                float topBottomAdjacentMean,
                float topBottomP95,
                float topBottomAdjacentP95)
            {
                LeftRightMean = leftRightMean;
                LeftRightAdjacentMean = leftRightAdjacentMean;
                LeftRightP95 = leftRightP95;
                LeftRightAdjacentP95 = leftRightAdjacentP95;
                TopBottomMean = topBottomMean;
                TopBottomAdjacentMean = topBottomAdjacentMean;
                TopBottomP95 = topBottomP95;
                TopBottomAdjacentP95 = topBottomAdjacentP95;
            }

            public float LeftRightMean { get; }
            public float LeftRightAdjacentMean { get; }
            public float LeftRightP95 { get; }
            public float LeftRightAdjacentP95 { get; }
            public float TopBottomMean { get; }
            public float TopBottomAdjacentMean { get; }
            public float TopBottomP95 { get; }
            public float TopBottomAdjacentP95 { get; }
            public float LeftRightMeanRatio => DivideOrInfinity(
                LeftRightMean,
                LeftRightAdjacentMean);
            public float LeftRightP95Ratio => DivideOrInfinity(
                LeftRightP95,
                LeftRightAdjacentP95);
            public float TopBottomMeanRatio => DivideOrInfinity(
                TopBottomMean,
                TopBottomAdjacentMean);
            public float TopBottomP95Ratio => DivideOrInfinity(
                TopBottomP95,
                TopBottomAdjacentP95);

            private static float DivideOrInfinity(
                float value,
                float divisor)
            {
                if (divisor > 0.000001f)
                {
                    return value / divisor;
                }

                return value > 0.000001f
                    ? float.PositiveInfinity
                    : 0f;
            }
        }

        internal sealed class AuthoredColorBuildResult
        {
            public Color[] SourceDerivedBasePixels { get; set; }
            public Color[] NormalizedFormBasePixels { get; set; }
            public float FormLowPercentile { get; set; }
            public float FormMedian { get; set; }
            public float FormHighPercentile { get; set; }
            public List<Color[]> MipPixels { get; } = new List<Color[]>();
            public List<int> MipWidths { get; } = new List<int>();
            public List<int> MipHeights { get; } = new List<int>();
            public List<PeriodicSeamMetrics> BeforeRepair { get; } =
                new List<PeriodicSeamMetrics>();
            public List<PeriodicSeamMetrics> AfterRepair { get; } =
                new List<PeriodicSeamMetrics>();
            public List<bool> LeftRightRepairApplied { get; } =
                new List<bool>();
            public List<bool> TopBottomRepairApplied { get; } =
                new List<bool>();
        }

        private static bool repairScheduled;
        private static bool buildInProgress;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            ScheduleRepair("InitializeOnLoad");
            EditorApplication.projectChanged -= OnProjectChanged;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        public static void ScheduleRepair()
        {
            ScheduleRepair("ExternalRequest");
        }

        private static void OnProjectChanged()
        {
            ScheduleRepair("ProjectChanged");
        }

        private static void ScheduleRepair(string reason)
        {
            EditorReloadDiagnostics.RecordPixelSurfaceSchedule(
                reason,
                repairScheduled,
                buildInProgress,
                EditorApplication.isCompiling);
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

            bool diagnosticCapture =
                EditorReloadDiagnostics.IsCaptureActive;
            long totalStart = diagnosticCapture
                ? EditorReloadDiagnostics.BeginTiming()
                : 0L;
            string signature = CalculateSignature(library);
            Texture2DArray detailArray = library.GeneratedTextureArray;
            bool detailInvalid =
                detailArray == null ||
                detailArray.width != library.SliceResolution ||
                detailArray.height != library.SliceResolution ||
                detailArray.depth != library.RequiredPackedBackingDepth;

            int textureFormEntryCount = CountTextureFormEntries(library);
            Texture2DArray colorArray =
                library.GeneratedAuthoredColorArray;
            bool colorInvalid = textureFormEntryCount == 0
                ? colorArray != null
                : colorArray == null ||
                  colorArray.width != library.SliceResolution ||
                  colorArray.height != library.SliceResolution ||
                  colorArray.depth != textureFormEntryCount;
            bool mappingInvalid =
                library.GeneratedAuthoredColorSliceIndices == null ||
                library.GeneratedAuthoredColorSliceIndices.Count !=
                library.Entries.Count;
            bool signatureInvalid = !string.Equals(
                signature,
                library.GeneratedSignature,
                StringComparison.Ordinal);
            bool needsRebuild =
                detailInvalid ||
                colorInvalid ||
                mappingInvalid ||
                signatureInvalid;
            if (diagnosticCapture)
            {
                EditorReloadDiagnostics.RecordTimedStage(
                    "NeedsRebuild",
                    totalStart,
                    $"library={library.name}, result={needsRebuild}, " +
                    $"detailInvalid={detailInvalid}, colorInvalid={colorInvalid}, " +
                    $"mappingInvalid={mappingInvalid}, signatureInvalid={signatureInvalid}");
            }

            return needsRebuild;
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
                    if (entry.UsesPrepackedTextureForm)
                    {
                        ValidatePrepackedTextureFormEntry(
                            library,
                            entry,
                            messages);
                    }
                }
            }

            return messages;
        }

        public static bool Rebuild(
            StylizedSurfaceDetailLibrary library,
            bool logResult = true)
        {
            return Rebuild(
                library,
                out _,
                logResult);
        }

        internal static bool Rebuild(
            StylizedSurfaceDetailLibrary library,
            out IReadOnlyList<string> failureMessages,
            bool logResult = true)
        {
            List<string> failures = new List<string>();
            failureMessages = failures;
            if (library == null)
            {
                failures.Add("The detail library is missing.");
                if (logResult)
                {
                    Debug.LogError(
                        "Could not rebuild a missing detail library.");
                }

                return false;
            }

            if (buildInProgress)
            {
                failures.Add(
                    $"A detail-library rebuild is already in progress; " +
                    $"'{library.name}' was not rebuilt.");
                if (logResult)
                {
                    Debug.LogError(
                        failures[0],
                        library);
                }

                return false;
            }

            long rebuildTotalStart = EditorReloadDiagnostics.BeginTiming();
            bool rebuildSucceeded = false;
            buildInProgress = true;
            try
            {
                long normalizeStart = EditorReloadDiagnostics.BeginTiming();
                NormalizeSourceImporters(library);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.NormalizeSourceImporters",
                    normalizeStart,
                    $"library={library.name}");

                long validationStart = EditorReloadDiagnostics.BeginTiming();
                IReadOnlyList<string> validation = Validate(library);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.Validate",
                    validationStart,
                    $"library={library.name}, failures={validation.Count}");
                if (validation.Count > 0)
                {
                    failures.AddRange(validation);
                    if (logResult)
                    {
                        Debug.LogError(
                            $"Could not rebuild '{library.name}':\n- " +
                            string.Join("\n- ", validation),
                            library);
                    }

                    return false;
                }

                long constructStart = EditorReloadDiagnostics.BeginTiming();
                int resolution = library.SliceResolution;
                int logicalDepth = library.LogicalEntryCount;
                int backingDepth = library.RequiredPackedBackingDepth;
                Texture2DArray detailArray = new Texture2DArray(
                    resolution,
                    resolution,
                    backingDepth,
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

                int textureFormCount = CountTextureFormEntries(library);
                Texture2DArray authoredColorArray = textureFormCount > 0
                    ? new Texture2DArray(
                        resolution,
                        resolution,
                        textureFormCount,
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
                    new List<int>(logicalDepth);
                int authoredColorSlice = 0;
                if (library.UsesInternalNeutralBackingSlice)
                {
                    CopyGeneratedMipChain(
                        detailArray,
                        0,
                        BuildNeutralPackedDetailPixels(resolution),
                        resolution,
                        true);
                }

                for (int slice = 0; slice < logicalDepth; slice++)
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
                        AuthoredColorBuildResult authoredColorBuild =
                            BuildAuthoredColorMipChain(
                                entry.AuthoredBaseColor,
                                resolution);
                        CopyGeneratedMipChain(
                            authoredColorArray,
                            authoredColorSlice,
                            authoredColorBuild.MipPixels);
                        authoredColorSliceIndices.Add(
                            authoredColorSlice);
                        authoredColorSlice++;
                    }
                    else
                    {
                        CopySourceMipChain(
                            entry.SourceTexture,
                            detailArray,
                            slice);
                        if (entry.UsesPrepackedTextureForm)
                        {
                            CopySourceMipChain(
                                entry.PrepackedTextureForm,
                                authoredColorArray,
                                authoredColorSlice);
                            authoredColorSliceIndices.Add(
                                authoredColorSlice);
                            authoredColorSlice++;
                        }
                        else
                        {
                            authoredColorSliceIndices.Add(-1);
                        }
                    }
                }
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.ConstructAndPopulateArrays",
                    constructStart,
                    $"library={library.name}, logicalDepth={logicalDepth}, " +
                    $"backingDepth={backingDepth}, textureFormCount={textureFormCount}, " +
                    $"resolution={resolution}");

                long applyStart = EditorReloadDiagnostics.BeginTiming();
                detailArray.Apply(false, true);
                authoredColorArray?.Apply(false, true);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.TextureArrayApply",
                    applyStart,
                    $"library={library.name}");

                long subAssetStart = EditorReloadDiagnostics.BeginTiming();
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
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.ReplaceGeneratedSubAssets",
                    subAssetStart,
                    $"library={library.name}, asset={libraryPath}");

                long finalizeStateStart = EditorReloadDiagnostics.BeginTiming();
                library.SetGeneratedArrays(
                    detailArray,
                    authoredColorArray,
                    authoredColorSliceIndices,
                    CalculateSignature(library));
                EditorUtility.SetDirty(library);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.FinalizeGeneratedState",
                    finalizeStateStart,
                    $"library={library.name}");

                long saveStart = EditorReloadDiagnostics.BeginTiming();
                AssetDatabase.SaveAssetIfDirty(library);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.SaveAssetIfDirty",
                    saveStart,
                    $"library={library.name}, asset={libraryPath}");

                long importStart = EditorReloadDiagnostics.BeginTiming();
                AssetDatabase.ImportAsset(
                    libraryPath,
                    ImportAssetOptions.ForceUpdate);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.ImportAssetForceUpdate",
                    importStart,
                    $"library={library.name}, asset={libraryPath}");

                long reloadStart = EditorReloadDiagnostics.BeginTiming();
                StylizedSurfaceDetailLibrary refreshedLibrary =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceDetailLibrary>(libraryPath);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.ReloadLibraryAsset",
                    reloadStart,
                    $"library={library.name}, asset={libraryPath}");

                long notifyStart = EditorReloadDiagnostics.BeginTiming();
                NotifyMaterialsUsingLibrary(
                    refreshedLibrary != null
                        ? refreshedLibrary
                        : library);
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.NotifyMaterialsUsingLibrary",
                    notifyStart,
                    $"library={library.name}");

                if (logResult)
                {
                    Debug.Log(
                        $"Rebuilt '{library.name}' with {logicalDepth} logical detail entry/entries, {backingDepth} packed backing slice(s), and {textureFormCount} texture-form slice(s) at {resolution}×{resolution}.",
                        library);
                }

                rebuildSucceeded = true;
                return true;
            }
            finally
            {
                buildInProgress = false;
                EditorReloadDiagnostics.RecordTimedStage(
                    "Rebuild.TOTAL",
                    rebuildTotalStart,
                    $"library={library.name}, success={rebuildSucceeded}, failures={failures.Count}");
            }
        }

        public static string CalculateSignature(
            StylizedSurfaceDetailLibrary library)
        {
            if (library == null)
            {
                return string.Empty;
            }

            bool diagnosticCapture =
                EditorReloadDiagnostics.IsCaptureActive;
            long totalStart = diagnosticCapture
                ? EditorReloadDiagnostics.BeginTiming()
                : 0L;
            StringBuilder builder = new StringBuilder();
            builder.Append(library.SliceResolution).Append('|');
            if (library.UsesInternalNeutralBackingSlice)
            {
                builder.Append("empty-backing-algorithm=")
                    .Append(EmptyLibraryBackingAlgorithmVersion)
                    .Append('|');
            }

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
                    builder.Append("texture-form-algorithm=")
                        .Append(AuthoredColorGenerationAlgorithmVersion)
                        .Append('|');
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
                    if (entry.UsesPrepackedTextureForm)
                    {
                        builder.Append("prepacked-texture-form-algorithm=")
                            .Append(PrepackedTextureFormAlgorithmVersion)
                            .Append('|');
                        AppendTextureSignature(
                            builder,
                            entry.PrepackedTextureForm);
                        if (entry.UsesFeatureTextureForm)
                        {
                            builder.Append("feature-substrate-roughness=")
                                .Append(entry.FeatureSubstrateRoughness)
                                .Append('|');
                            builder.Append("feature-max-radius-uv=")
                                .Append(entry.FeatureMaximumSupportRadiusUv)
                                .Append('|');
                        }
                    }
                }
            }

            string result = Hash128.Compute(builder.ToString()).ToString();
            if (diagnosticCapture)
            {
                EditorReloadDiagnostics.RecordTimedStage(
                    "CalculateSignature",
                    totalStart,
                    $"library={library.name}, entries={library.Entries.Count}");
            }

            return result;
        }

        private static void RepairAllLibraries()
        {
            bool diagnosticCapture =
                EditorReloadDiagnostics.IsCaptureActive;
            long totalStart = diagnosticCapture
                ? EditorReloadDiagnostics.BeginTiming()
                : 0L;
            int invocation = diagnosticCapture
                ? EditorReloadDiagnostics.RecordPixelSurfaceRepairStart()
                : 0;
            string outcome = "completed";
            try
            {
                repairScheduled = false;
                if (buildInProgress || EditorApplication.isCompiling)
                {
                    outcome = buildInProgress
                        ? "deferred:build-in-progress"
                        : "deferred:editor-compiling";
                    ScheduleRepair("RepairDeferred");
                    return;
                }

                long findStart = diagnosticCapture
                    ? EditorReloadDiagnostics.BeginTiming()
                    : 0L;
                string[] guids = AssetDatabase.FindAssets(
                    "t:StylizedSurfaceDetailLibrary");
                if (diagnosticCapture)
                {
                    EditorReloadDiagnostics.RecordTimedStage(
                        "RepairAllLibraries.FindAssets",
                        findStart,
                        $"count={guids.Length}");
                }

                int rebuilt = 0;
                int current = 0;
                int missing = 0;
                for (int index = 0; index < guids.Length; index++)
                {
                    long loadStart = diagnosticCapture
                        ? EditorReloadDiagnostics.BeginTiming()
                        : 0L;
                    string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                    StylizedSurfaceDetailLibrary library =
                        AssetDatabase.LoadAssetAtPath<
                            StylizedSurfaceDetailLibrary>(path);
                    if (diagnosticCapture)
                    {
                        EditorReloadDiagnostics.RecordTimedStage(
                            "RepairAllLibraries.LoadLibrary",
                            loadStart,
                            $"index={index}, asset={path}");
                    }

                    if (library == null)
                    {
                        missing++;
                        continue;
                    }

                    bool needsRebuild = NeedsRebuild(library);
                    if (diagnosticCapture)
                    {
                        EditorReloadDiagnostics.RecordEvent(
                            "PixelSurface",
                            $"Library decision: name={library.name}, asset={path}, " +
                            $"needsRebuild={needsRebuild}.");
                    }

                    if (!needsRebuild)
                    {
                        current++;
                        continue;
                    }

                    long rebuildStart = diagnosticCapture
                        ? EditorReloadDiagnostics.BeginTiming()
                        : 0L;
                    bool rebuiltSuccessfully = Rebuild(library, false);
                    if (diagnosticCapture)
                    {
                        EditorReloadDiagnostics.RecordTimedStage(
                            "RepairAllLibraries.Rebuild",
                            rebuildStart,
                            $"library={library.name}, success={rebuiltSuccessfully}");
                    }

                    if (rebuiltSuccessfully)
                    {
                        rebuilt++;
                    }
                }

                outcome =
                    $"completed:found={guids.Length}, current={current}, " +
                    $"rebuilt={rebuilt}, missing={missing}";
            }
            finally
            {
                if (diagnosticCapture)
                {
                    EditorReloadDiagnostics.RecordPixelSurfaceRepairEnd(
                        invocation,
                        totalStart,
                        outcome);
                }
            }
        }

        private static void NotifyMaterialsUsingLibrary(
            StylizedSurfaceDetailLibrary library)
        {
            bool diagnosticCapture =
                EditorReloadDiagnostics.IsCaptureActive;
            long findStart = diagnosticCapture
                ? EditorReloadDiagnostics.BeginTiming()
                : 0L;
            string[] guids = AssetDatabase.FindAssets(
                "t:StylizedSurfaceMaterialProfile");
            if (diagnosticCapture)
            {
                EditorReloadDiagnostics.RecordTimedStage(
                    "NotifyMaterials.FindAssets",
                    findStart,
                    $"library={library?.name ?? "<null>"}, count={guids.Length}");
            }

            int notified = 0;
            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                StylizedSurfaceMaterialProfile profile =
                    AssetDatabase.LoadAssetAtPath<
                        StylizedSurfaceMaterialProfile>(path);
                if (profile != null && profile.DetailLibrary == library)
                {
                    profile.NotifyEditorChanged();
                    notified++;
                }
            }

            if (diagnosticCapture)
            {
                EditorReloadDiagnostics.RecordEvent(
                    "PixelSurface",
                    $"NotifyMaterialsUsingLibrary: library={library?.name ?? "<null>"}, " +
                    $"profilesScanned={guids.Length}, notified={notified}.");
            }
        }

        private static int CountTextureFormEntries(
            StylizedSurfaceDetailLibrary library)
        {
            int count = 0;
            for (int index = 0; index < library.Entries.Count; index++)
            {
                StylizedSurfaceDetailLibrary.Entry entry =
                    library.Entries[index];
                if (entry == null || !entry.UsesTextureForm)
                {
                    continue;
                }

                bool hasSource = entry.UsesAuthoredMaterialSet
                    ? entry.AuthoredBaseColor != null
                    : entry.PrepackedTextureForm != null;
                if (hasSource)
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

        private static void ValidatePrepackedTextureFormEntry(
            StylizedSurfaceDetailLibrary library,
            StylizedSurfaceDetailLibrary.Entry entry,
            ICollection<string> messages)
        {
            Texture2D source = entry.PrepackedTextureForm;
            if (source == null)
            {
                messages.Add(
                    $"Entry '{entry.DisplayName}' has no paired Palette Form texture.");
                return;
            }

            if (source.width != library.SliceResolution ||
                source.height != library.SliceResolution)
            {
                messages.Add(
                    $"Entry '{entry.DisplayName}' Palette Form is {source.width}×{source.height}; expected {library.SliceResolution}×{library.SliceResolution}.");
            }

            ValidateImporter(
                source,
                entry.DisplayName + " Palette Form",
                true,
                true,
                messages);

            if (entry.UsesFeatureTextureForm && source.isReadable)
            {
                Color32[] pixels = source.GetPixels32(0);
                Color32[] packedPixels =
                    entry.SourceTexture != null &&
                    entry.SourceTexture.isReadable
                        ? entry.SourceTexture.GetPixels32(0)
                        : null;
                if (pixels == null ||
                    pixels.Length == 0 ||
                    packedPixels == null ||
                    packedPixels.Length != pixels.Length)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' feature-aware paired payload has missing or mismatched readable pixels.");
                    return;
                }

                double featureSum = 0.0;
                double substrateFormSum = 0.0;
                double anchorDistanceSum = 0.0;
                int anchorCount = 0;
                float featureMaximum = 0f;
                float anchorDistanceMinimum = 1f;
                float anchorDistanceMaximum = 0f;
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    float anchorX = DecodeSrgbByte(pixel.b) * 2f - 1f;
                    float anchorY = pixel.a / 255f * 2f - 1f;
                    float anchorDistance = Mathf.Sqrt(
                        anchorX * anchorX + anchorY * anchorY);
                    Color32 packedPixel = packedPixels[index];
                    float slopeX = packedPixel.r / 255f * 2f - 1f;
                    float slopeY = packedPixel.g / 255f * 2f - 1f;
                    float slopeEvidence = Mathf.Sqrt(
                        slopeX * slopeX + slopeY * slopeY);
                    float cavityEvidence = packedPixel.b / 255f;
                    float formEvidence = Mathf.Abs(
                        DecodeSrgbByte(pixel.r) -
                        DecodeSrgbByte(pixel.g));
                    float feature =
                        slopeEvidence >= 0.008f ||
                        cavityEvidence >= 0.001f ||
                        formEvidence >= 0.001f
                            ? 1f
                            : 0f;
                    featureSum += feature;
                    featureMaximum = Mathf.Max(featureMaximum, feature);
                    substrateFormSum += DecodeSrgbByte(pixel.g);
                    if (feature > 0.05f)
                    {
                        anchorDistanceSum += anchorDistance;
                        anchorCount++;
                        anchorDistanceMinimum = Mathf.Min(
                            anchorDistanceMinimum,
                            anchorDistance);
                        anchorDistanceMaximum = Mathf.Max(
                            anchorDistanceMaximum,
                            anchorDistance);
                    }
                }

                float inverseCount = 1f / pixels.Length;
                float featureMean = (float)(featureSum * inverseCount);
                float substrateFormMean =
                    (float)(substrateFormSum * inverseCount);
                float anchorDistanceMean = anchorCount > 0
                    ? (float)(anchorDistanceSum / anchorCount)
                    : 0f;
                if (featureMaximum < MinimumFeatureTextureFormMaximum ||
                    featureMean < MinimumFeatureTextureFormMean ||
                    featureMean > MaximumFeatureTextureFormMean)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' feature mask mean/max is {featureMean:F5}/{featureMaximum:F5}; expected mean {MinimumFeatureTextureFormMean:F5}–{MaximumFeatureTextureFormMean:F5} and maximum at least {MinimumFeatureTextureFormMaximum:F5}.");
                }

                if (substrateFormMean < MinimumFeatureSubstrateFormMean ||
                    substrateFormMean > MaximumFeatureSubstrateFormMean ||
                    entry.FeatureSubstrateRoughness <
                        MinimumFeatureSubstrateRoughnessMean ||
                    entry.FeatureSubstrateRoughness >
                        MaximumFeatureSubstrateRoughnessMean ||
                    entry.FeatureMaximumSupportRadiusUv <
                        MinimumFeatureMaximumSupportRadiusUv ||
                    entry.FeatureMaximumSupportRadiusUv >
                        MaximumFeatureMaximumSupportRadiusUv ||
                    anchorCount <= 0 ||
                    anchorDistanceMean < MinimumFeatureAnchorDistanceMean ||
                    anchorDistanceMean > MaximumFeatureAnchorDistanceMean ||
                    anchorDistanceMinimum < 0f ||
                    anchorDistanceMaximum > 1.01f)
                {
                    messages.Add(
                        $"Entry '{entry.DisplayName}' feature payload form/anchor-distance/roughness/radius is {substrateFormMean:F5} / {anchorDistanceMinimum:F5}–{anchorDistanceMaximum:F5} mean {anchorDistanceMean:F5} / {entry.FeatureSubstrateRoughness:F5} / {entry.FeatureMaximumSupportRadiusUv:F6}; expected valid algorithm-10 centre-offset B/A plus scalar roughness/radius metadata.");
                }
            }
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

        internal static Color[] BuildNeutralPackedDetailPixels(
            int resolution)
        {
            int safeResolution = Mathf.Max(1, resolution);
            Color neutral = new Color(0.5f, 0.5f, 0f, 0.5f);
            Color[] pixels = new Color[safeResolution * safeResolution];
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = neutral;
            }

            return pixels;
        }

        internal static Color[] BuildPackedMaterialPixels(
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

        private static void CopySourceMipChain(
            Texture2D source,
            Texture2DArray destination,
            int slice)
        {
            int mipCount = Mathf.Min(
                source.mipmapCount,
                destination.mipmapCount);
            for (int mip = 0; mip < mipCount; mip++)
            {
                destination.SetPixels(
                    source.GetPixels(mip),
                    slice,
                    mip);
            }
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

        private static void CopyGeneratedMipChain(
            Texture2DArray destination,
            int slice,
            IReadOnlyList<Color[]> mipPixels)
        {
            int mipCount = Mathf.Min(
                destination.mipmapCount,
                mipPixels != null ? mipPixels.Count : 0);
            for (int mip = 0; mip < mipCount; mip++)
            {
                Color[] sourcePixels = mipPixels[mip];
                Color32[] encodedPixels = new Color32[sourcePixels.Length];
                for (int index = 0; index < sourcePixels.Length; index++)
                {
                    encodedPixels[index] = sourcePixels[index];
                }

                // Normalized grayscale texture form is stored in the existing
                // sRGB RGBA32 compatibility array. Raw upload preserves the
                // encoded form values without Color-to-texture conversion.
                destination.SetPixelData(encodedPixels, mip, slice, 0);
            }
        }

        internal static AuthoredColorBuildResult BuildAuthoredColorMipChain(
            Texture2D source,
            int resolution)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            int width = Mathf.Max(1, resolution);
            int height = width;
            Color[] sourceDerived = AreaResamplePixels(source, width, height);
            Color[] current = NormalizeAuthoredFormPixels(
                sourceDerived,
                out float formLow,
                out float formMedian,
                out float formHigh);
            AuthoredColorBuildResult result = new AuthoredColorBuildResult
            {
                SourceDerivedBasePixels = (Color[])sourceDerived.Clone(),
                NormalizedFormBasePixels = (Color[])current.Clone(),
                FormLowPercentile = formLow,
                FormMedian = formMedian,
                FormHighPercentile = formHigh
            };

            while (true)
            {
                PeriodicSeamMetrics before = CalculatePeriodicSeamMetrics(
                    current,
                    width,
                    height);
                bool repairLeftRight = NeedsPeriodicRepair(
                    before.LeftRightMean,
                    before.LeftRightAdjacentMean,
                    before.LeftRightP95,
                    before.LeftRightAdjacentP95);
                bool repairTopBottom = NeedsPeriodicRepair(
                    before.TopBottomMean,
                    before.TopBottomAdjacentMean,
                    before.TopBottomP95,
                    before.TopBottomAdjacentP95);

                if (repairLeftRight || repairTopBottom)
                {
                    current = RepairPeriodicBoundaries(
                        current,
                        width,
                        height,
                        repairLeftRight,
                        repairTopBottom);
                }

                PeriodicSeamMetrics after = CalculatePeriodicSeamMetrics(
                    current,
                    width,
                    height);
                result.MipPixels.Add(current);
                result.MipWidths.Add(width);
                result.MipHeights.Add(height);
                result.BeforeRepair.Add(before);
                result.AfterRepair.Add(after);
                result.LeftRightRepairApplied.Add(repairLeftRight);
                result.TopBottomRepairApplied.Add(repairTopBottom);

                if (width == 1 && height == 1)
                {
                    break;
                }

                int nextWidth = Mathf.Max(1, width / 2);
                int nextHeight = Mathf.Max(1, height / 2);
                current = DownsampleMip(
                    current,
                    width,
                    height,
                    nextWidth,
                    nextHeight);
                width = nextWidth;
                height = nextHeight;
            }

            return result;
        }

        internal static bool PassesPeriodicSeamThresholds(
            PeriodicSeamMetrics metrics)
        {
            return PassesBoundaryThreshold(
                       metrics.LeftRightMean,
                       metrics.LeftRightMeanRatio,
                       MinimumMeanBoundaryDifference,
                       AuthoredColorSeamMeanRatioLimit) &&
                   PassesBoundaryThreshold(
                       metrics.LeftRightP95,
                       metrics.LeftRightP95Ratio,
                       MinimumP95BoundaryDifference,
                       AuthoredColorSeamP95RatioLimit) &&
                   PassesBoundaryThreshold(
                       metrics.TopBottomMean,
                       metrics.TopBottomMeanRatio,
                       MinimumMeanBoundaryDifference,
                       AuthoredColorSeamMeanRatioLimit) &&
                   PassesBoundaryThreshold(
                       metrics.TopBottomP95,
                       metrics.TopBottomP95Ratio,
                       MinimumP95BoundaryDifference,
                       AuthoredColorSeamP95RatioLimit);
        }

        private static Color[] AreaResamplePixels(
            Texture2D source,
            int destinationWidth,
            int destinationHeight)
        {
            Color[] destination =
                new Color[destinationWidth * destinationHeight];
            Color32[] sourcePixels = source.GetPixels32(0);
            if (source.width < destinationWidth ||
                source.height < destinationHeight)
            {
                for (int y = 0; y < destinationHeight; y++)
                {
                    float v = (y + 0.5f) / destinationHeight;
                    for (int x = 0; x < destinationWidth; x++)
                    {
                        float u = (x + 0.5f) / destinationWidth;
                        destination[y * destinationWidth + x] =
                            SampleEncodedBilinear(
                                sourcePixels,
                                source.width,
                                source.height,
                                u,
                                v);
                    }
                }

                return destination;
            }

            double scaleX = source.width / (double)destinationWidth;
            double scaleY = source.height / (double)destinationHeight;
            for (int y = 0; y < destinationHeight; y++)
            {
                double sourceYStart = y * scaleY;
                double sourceYEnd = (y + 1) * scaleY;
                int sourceYFirst = Mathf.Clamp(
                    (int)Math.Floor(sourceYStart),
                    0,
                    source.height - 1);
                int sourceYLast = Mathf.Clamp(
                    (int)Math.Ceiling(sourceYEnd) - 1,
                    0,
                    source.height - 1);

                for (int x = 0; x < destinationWidth; x++)
                {
                    double sourceXStart = x * scaleX;
                    double sourceXEnd = (x + 1) * scaleX;
                    int sourceXFirst = Mathf.Clamp(
                        (int)Math.Floor(sourceXStart),
                        0,
                        source.width - 1);
                    int sourceXLast = Mathf.Clamp(
                        (int)Math.Ceiling(sourceXEnd) - 1,
                        0,
                        source.width - 1);

                    double red = 0.0;
                    double green = 0.0;
                    double blue = 0.0;
                    double totalWeight = 0.0;
                    for (int sourceY = sourceYFirst;
                         sourceY <= sourceYLast;
                         sourceY++)
                    {
                        double yWeight = Math.Max(
                            0.0,
                            Math.Min(sourceYEnd, sourceY + 1.0) -
                            Math.Max(sourceYStart, sourceY));
                        for (int sourceX = sourceXFirst;
                             sourceX <= sourceXLast;
                             sourceX++)
                        {
                            double xWeight = Math.Max(
                                0.0,
                                Math.Min(sourceXEnd, sourceX + 1.0) -
                                Math.Max(sourceXStart, sourceX));
                            double weight = xWeight * yWeight;
                            Color32 sample = sourcePixels[
                                sourceY * source.width + sourceX];
                            red += (sample.r / 255.0) * weight;
                            green += (sample.g / 255.0) * weight;
                            blue += (sample.b / 255.0) * weight;
                            totalWeight += weight;
                        }
                    }

                    double inverseWeight = totalWeight > 0.0
                        ? 1.0 / totalWeight
                        : 0.0;
                    destination[y * destinationWidth + x] = new Color(
                        (float)(red * inverseWeight),
                        (float)(green * inverseWeight),
                        (float)(blue * inverseWeight),
                        1f);
                }
            }

            return destination;
        }


        private static Color SampleEncodedBilinear(
            Color32[] sourcePixels,
            int width,
            int height,
            float u,
            float v)
        {
            float x = Mathf.Repeat(u, 1f) * width - 0.5f;
            float y = Mathf.Repeat(v, 1f) * height - 0.5f;
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            float tx = x - x0;
            float ty = y - y0;
            int x1 = PositiveModulo(x0 + 1, width);
            int y1 = PositiveModulo(y0 + 1, height);
            x0 = PositiveModulo(x0, width);
            y0 = PositiveModulo(y0, height);

            Color c00 = sourcePixels[y0 * width + x0];
            Color c10 = sourcePixels[y0 * width + x1];
            Color c01 = sourcePixels[y1 * width + x0];
            Color c11 = sourcePixels[y1 * width + x1];
            Color bottom = Color.LerpUnclamped(c00, c10, tx);
            Color top = Color.LerpUnclamped(c01, c11, tx);
            Color result = Color.LerpUnclamped(bottom, top, ty);
            result.a = 1f;
            return result;
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
        }

        private static Color[] NormalizeAuthoredFormPixels(
            Color[] source,
            out float lowPercentile,
            out float median,
            out float highPercentile)
        {
            if (source == null || source.Length == 0)
            {
                lowPercentile = 0f;
                median = 0.5f;
                highPercentile = 1f;
                return Array.Empty<Color>();
            }

            float[] luminance = new float[source.Length];
            for (int index = 0; index < source.Length; index++)
            {
                Color sample = source[index];
                float linearRed = Mathf.GammaToLinearSpace(
                    Mathf.Clamp01(sample.r));
                float linearGreen = Mathf.GammaToLinearSpace(
                    Mathf.Clamp01(sample.g));
                float linearBlue = Mathf.GammaToLinearSpace(
                    Mathf.Clamp01(sample.b));
                luminance[index] = Mathf.Clamp01(
                    linearRed * 0.2126f +
                    linearGreen * 0.7152f +
                    linearBlue * 0.0722f);
            }

            lowPercentile = CalculatePercentile(luminance, 0.05f);
            median = CalculatePercentile(luminance, 0.50f);
            highPercentile = CalculatePercentile(luminance, 0.95f);
            float lowerRange = Mathf.Max(0.0001f, median - lowPercentile);
            float upperRange = Mathf.Max(0.0001f, highPercentile - median);

            Color[] normalized = new Color[source.Length];
            for (int index = 0; index < luminance.Length; index++)
            {
                float value = luminance[index] <= median
                    ? 0.5f * (luminance[index] - lowPercentile) / lowerRange
                    : 0.5f +
                      0.5f * (luminance[index] - median) / upperRange;
                float encoded = Mathf.LinearToGammaSpace(
                    Mathf.Clamp01(value));
                normalized[index] = new Color(
                    encoded,
                    encoded,
                    encoded,
                    1f);
            }

            return normalized;
        }

        internal static float DecodeFormValue(Color encodedForm)
        {
            return Mathf.Clamp01(
                Mathf.GammaToLinearSpace(
                    Mathf.Clamp01(encodedForm.r)));
        }

        internal static float DecodeSrgbByte(byte encodedValue)
        {
            return Mathf.Clamp01(
                Mathf.GammaToLinearSpace(encodedValue / 255f));
        }

        private static Color EncodeFormValue(float linearForm)
        {
            float encoded = Mathf.LinearToGammaSpace(
                Mathf.Clamp01(linearForm));
            return new Color(encoded, encoded, encoded, 1f);
        }

        private static float CalculatePercentile(
            float[] values,
            float percentile)
        {
            if (values == null || values.Length == 0)
            {
                return 0f;
            }

            float[] ordered = (float[])values.Clone();
            Array.Sort(ordered);
            float position = Mathf.Clamp01(percentile) *
                             (ordered.Length - 1);
            int lower = Mathf.FloorToInt(position);
            int upper = Mathf.Min(ordered.Length - 1, lower + 1);
            return Mathf.LerpUnclamped(
                ordered[lower],
                ordered[upper],
                position - lower);
        }

        private static Color[] DownsampleMip(
            Color[] source,
            int sourceWidth,
            int sourceHeight,
            int destinationWidth,
            int destinationHeight)
        {
            Color[] destination =
                new Color[destinationWidth * destinationHeight];
            for (int y = 0; y < destinationHeight; y++)
            {
                int sourceY0 = Mathf.Min(sourceHeight - 1, y * 2);
                int sourceY1 = Mathf.Min(sourceHeight - 1, sourceY0 + 1);
                for (int x = 0; x < destinationWidth; x++)
                {
                    int sourceX0 = Mathf.Min(sourceWidth - 1, x * 2);
                    int sourceX1 = Mathf.Min(sourceWidth - 1, sourceX0 + 1);
                    float linearAverage = (
                        DecodeFormValue(source[
                            sourceY0 * sourceWidth + sourceX0]) +
                        DecodeFormValue(source[
                            sourceY0 * sourceWidth + sourceX1]) +
                        DecodeFormValue(source[
                            sourceY1 * sourceWidth + sourceX0]) +
                        DecodeFormValue(source[
                            sourceY1 * sourceWidth + sourceX1])) * 0.25f;
                    float encoded = Mathf.LinearToGammaSpace(
                        Mathf.Clamp01(linearAverage));
                    destination[y * destinationWidth + x] = new Color(
                        encoded,
                        encoded,
                        encoded,
                        1f);
                }
            }

            return destination;
        }

        private static Color[] RepairPeriodicBoundaries(
            Color[] source,
            int width,
            int height,
            bool repairLeftRight,
            bool repairTopBottom)
        {
            Color[] repaired = (Color[])source.Clone();
            if (repairLeftRight && width > 1)
            {
                int band = CalculateSeamRepairBand(width);
                for (int y = 0; y < height; y++)
                {
                    int rowStart = y * width;
                    for (int distance = 0; distance < band; distance++)
                    {
                        float weight = CalculateSeamRepairWeight(
                            distance,
                            band);
                        int leftIndex = rowStart + distance;
                        int rightIndex = rowStart + width - 1 - distance;
                        float leftValue = DecodeFormValue(
                            repaired[leftIndex]);
                        float rightValue = DecodeFormValue(
                            repaired[rightIndex]);
                        float averageValue = (leftValue + rightValue) * 0.5f;
                        repaired[leftIndex] = EncodeFormValue(
                            Mathf.LerpUnclamped(
                                leftValue,
                                averageValue,
                                weight));
                        repaired[rightIndex] = EncodeFormValue(
                            Mathf.LerpUnclamped(
                                rightValue,
                                averageValue,
                                weight));
                    }
                }
            }

            if (repairTopBottom && height > 1)
            {
                int band = CalculateSeamRepairBand(height);
                for (int x = 0; x < width; x++)
                {
                    for (int distance = 0; distance < band; distance++)
                    {
                        float weight = CalculateSeamRepairWeight(
                            distance,
                            band);
                        int bottomIndex = distance * width + x;
                        int topIndex = (height - 1 - distance) * width + x;
                        float bottomValue = DecodeFormValue(
                            repaired[bottomIndex]);
                        float topValue = DecodeFormValue(
                            repaired[topIndex]);
                        float averageValue = (bottomValue + topValue) * 0.5f;
                        repaired[bottomIndex] = EncodeFormValue(
                            Mathf.LerpUnclamped(
                                bottomValue,
                                averageValue,
                                weight));
                        repaired[topIndex] = EncodeFormValue(
                            Mathf.LerpUnclamped(
                                topValue,
                                averageValue,
                                weight));
                    }
                }
            }

            return repaired;
        }

        private static int CalculateSeamRepairBand(int dimension)
        {
            int scaled = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    AuthoredColorSeamRepairBandAt256 *
                    dimension /
                    256f));
            return Mathf.Min(scaled, Mathf.Max(1, dimension / 2));
        }

        private static float CalculateSeamRepairWeight(
            int distance,
            int band)
        {
            if (band <= 1)
            {
                return 1f;
            }

            float t = distance / (band - 1f);
            float smooth = t * t * (3f - 2f * t);
            return 1f - smooth;
        }

        private static PeriodicSeamMetrics CalculatePeriodicSeamMetrics(
            Color[] pixels,
            int width,
            int height)
        {
            float[] leftRightBoundary = new float[Mathf.Max(1, height)];
            float[] topBottomBoundary = new float[Mathf.Max(1, width)];
            float[] horizontalAdjacent = width > 1
                ? new float[height * (width - 1)]
                : Array.Empty<float>();
            float[] verticalAdjacent = height > 1
                ? new float[(height - 1) * width]
                : Array.Empty<float>();

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * width;
                leftRightBoundary[y] = RgbDifference(
                    pixels[rowStart],
                    pixels[rowStart + width - 1]);
                for (int x = 0; x < width - 1; x++)
                {
                    horizontalAdjacent[y * (width - 1) + x] =
                        RgbDifference(
                            pixels[rowStart + x],
                            pixels[rowStart + x + 1]);
                }
            }

            for (int x = 0; x < width; x++)
            {
                topBottomBoundary[x] = RgbDifference(
                    pixels[x],
                    pixels[(height - 1) * width + x]);
                for (int y = 0; y < height - 1; y++)
                {
                    verticalAdjacent[y * width + x] = RgbDifference(
                        pixels[y * width + x],
                        pixels[(y + 1) * width + x]);
                }
            }

            return new PeriodicSeamMetrics(
                Mean(leftRightBoundary),
                Mean(horizontalAdjacent),
                Percentile95(leftRightBoundary),
                Percentile95(horizontalAdjacent),
                Mean(topBottomBoundary),
                Mean(verticalAdjacent),
                Percentile95(topBottomBoundary),
                Percentile95(verticalAdjacent));
        }

        private static bool NeedsPeriodicRepair(
            float boundaryMean,
            float adjacentMean,
            float boundaryP95,
            float adjacentP95)
        {
            return !PassesBoundaryThreshold(
                       boundaryMean,
                       CalculateBoundaryRatio(
                           boundaryMean,
                           adjacentMean),
                       MinimumMeanBoundaryDifference,
                       AuthoredColorSeamMeanRatioLimit) ||
                   !PassesBoundaryThreshold(
                       boundaryP95,
                       CalculateBoundaryRatio(
                           boundaryP95,
                           adjacentP95),
                       MinimumP95BoundaryDifference,
                       AuthoredColorSeamP95RatioLimit);
        }

        private static float CalculateBoundaryRatio(
            float boundaryDifference,
            float adjacentDifference)
        {
            if (adjacentDifference > 0.000001f)
            {
                return boundaryDifference / adjacentDifference;
            }

            return boundaryDifference > 0.000001f
                ? float.PositiveInfinity
                : 0f;
        }

        private static bool PassesBoundaryThreshold(
            float boundaryDifference,
            float ratio,
            float minimumDifference,
            float ratioLimit)
        {
            return boundaryDifference <= minimumDifference ||
                   ratio <= ratioLimit;
        }

        private static float RgbDifference(Color left, Color right)
        {
            return (
                Mathf.Abs(left.r - right.r) +
                Mathf.Abs(left.g - right.g) +
                Mathf.Abs(left.b - right.b)) / 3f;
        }

        private static float Mean(float[] values)
        {
            if (values == null || values.Length == 0)
            {
                return 0f;
            }

            double sum = 0.0;
            for (int index = 0; index < values.Length; index++)
            {
                sum += values[index];
            }

            return (float)(sum / values.Length);
        }

        private static float Percentile95(float[] values)
        {
            if (values == null || values.Length == 0)
            {
                return 0f;
            }

            float[] ordered = (float[])values.Clone();
            Array.Sort(ordered);
            int index = Mathf.Clamp(
                Mathf.CeilToInt(ordered.Length * 0.95f) - 1,
                0,
                ordered.Length - 1);
            return ordered[index];
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
                    if (entry.UsesPrepackedTextureForm)
                    {
                        NormalizeImporter(
                            entry.PrepackedTextureForm,
                            true,
                            true,
                            library.SliceResolution);
                    }
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
                long reimportStart = EditorReloadDiagnostics.BeginTiming();
                importer.SaveAndReimport();
                EditorReloadDiagnostics.RecordTimedStage(
                    "NormalizeImporter.SaveAndReimport",
                    reimportStart,
                    $"asset={path}");
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
                bool diagnosticCapture =
                    EditorReloadDiagnostics.IsCaptureActive;
                long dependencyStart = diagnosticCapture
                    ? EditorReloadDiagnostics.BeginTiming()
                    : 0L;
                Hash128 dependencyHash =
                    AssetDatabase.GetAssetDependencyHash(path);
                if (diagnosticCapture)
                {
                    EditorReloadDiagnostics.RecordTimedStage(
                        "GetAssetDependencyHash",
                        dependencyStart,
                        $"asset={path}");
                }

                builder.Append(dependencyHash);
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
