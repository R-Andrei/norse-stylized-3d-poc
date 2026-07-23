using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Geometry.Masses;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    /// <summary>
    /// Editor-only deterministic assembly proof for seamless sparse riverbed
    /// material candidates. It creates no runtime or imported project asset.
    /// </summary>
    internal static class GeneratedMassSparseRiverbedTileAssembler
    {
        internal const int AlgorithmVersion = 10;
        internal const int FinalResolution = 1024;
        internal const int WorkResolution = 2048;
        internal const int CandidateCount = 3;
        internal const int ExpectedSourceCount = 18;
        internal const int FeatureBoundarySweepWidth = 1024;
        internal const int FeatureBoundarySweepHeight = 256;
        internal const float MinimumPlacementScale = 0.55f;
        internal const float MaximumPlacementScale = 1.20f;
        internal const float MinimumSmallPlacementFraction = 0.65f;
        internal const float MaximumRootPerimeterFraction = 0.62f;
        internal const float MinimumSpacingFactor = 1.05f;
        internal const float NearHotspotRadiusWork = 320f;
        internal const float BroadHotspotRadiusWork = 640f;
        internal const int MaximumNearNeighbourCount = 1;
        internal const int SharedPlacementSeed = 91073;
        internal const int SharedSubstrateSeed = 0x4D554433;

        private const int WorkScale = WorkResolution / FinalResolution;
        private const int QuietBlockSizeFinal = 32;
        private const int QuietBlockAxis = FinalResolution / QuietBlockSizeFinal;
        private const int QuietBlockCount = QuietBlockAxis * QuietBlockAxis;
        private const int MaximumPlacementAttempts = 6000;
        private const float BaseRockDiameterWork = 94f;
        private const float MinimumOverlapFraction = 0.018f;
        private const float StrongHeightFilterRangeSigma = 0.045f;
        private const float MildHeightFilterRangeSigma = 0.026f;
        private const int StrongHeightFilterPasses = 3;
        private const int MildHeightFilterPasses = 1;
        private const float StrongNormalStrength = 6.2f;
        private const float MildNormalStrength = 7.2f;
        private const float MildNormalBlend = 0.22f;
        private const int WearSilhouetteExclusionRadius = 3;
        private const int WearNormalizationBinCount = 64;
        private const float WearNormalizationPercentile = 0.90f;
        private const float MinimumWearNormalizationSignal = 0.055f;
        private const float RootAffectedThreshold = 0.08f;
        private const float MaskSeamMeanTolerance = 0.035f;
        private const float HeightSeamMeanTolerance = 0.060f;
        private const float NormalSeamMeanTolerance = 0.150f;
        private const float ScalarSeamMeanTolerance = 0.090f;
        private const float PreviewSeamMeanTolerance = 0.100f;
        private const float PalettePayloadSeamMeanTolerance = 0.020f;
        private const float SubstratePaletteFormCenter = 0.62f;
        private const float SubstratePaletteFormGain = 1.50f;
        private const float RockFormLowLuminance = 0.14f;
        private const float RockFormMedianLuminance = 0.405f;
        private const float RockFormHighLuminance = 0.50f;
        private const float RockFormDarkMinimum = 0.04f;
        private const float RockFormBaseMedian = 0.34f;
        private const float RockFormLightMaximum = 0.56f;
        private const float PalettePreviewCavityBias = 0.15f;
        private const float SilhouetteCoverageLower = 0.30f;
        private const float SilhouetteCoverageUpper = 0.98f;
        private const int SilhouetteFilterRadius = 3;
        private const float AnchorProofCoverageThreshold = 0.05f;
        private const float AnchorProofMaximumDistance = 0.92f;
        private const float AnchorSupportPaddingTexels = 1f;
        private const int AnchorProofMaximumMip = 5;
        private const float FeatureSlopeEvidenceThreshold = 0.008f;
        private const float FeatureCavityEvidenceThreshold = 0.001f;
        private const float FeatureFormEvidenceThreshold = 0.001f;
        private const float FeatureRoughnessEvidenceThreshold = 0.008f;
        private const int FeatureBoundaryProofOrientationCount = 8;
        private const float FeatureBoundaryTransitionRadiusFactor = 0.65f;
        private const float FeatureBoundarySafetyRadiusFactor = 0.20f;
        private const float FeatureBoundaryFadeRadiusFactor = 1.00f;
        private const float FeatureBoundaryWeightTolerance = 0.025f;

        private static readonly CandidateDefinition[] CandidateDefinitions =
        {
            new CandidateDefinition(
                "Ultra_Sparse_Riverbed",
                "Ultra Sparse Riverbed",
                6,
                0.0025f,
                0.0100f,
                3),
            new CandidateDefinition(
                "Very_Sparse_Riverbed",
                "Very Sparse Riverbed",
                9,
                0.0045f,
                0.0140f,
                4),
            new CandidateDefinition(
                "Sparse_Riverbed",
                "Sparse Riverbed",
                12,
                0.0065f,
                0.0180f,
                5)
        };

        private static readonly PaletteDefinition NeutralPalette =
            new PaletteDefinition(
                "Neutral",
                "Neutral",
                new Color(0.517f, 0.503f, 0.458f, 1f),
                new Color(0.140f, 0.150f, 0.140f, 1f),
                new Color(0.580f, 0.560f, 0.500f, 1f),
                new Color(0.055f, 0.050f, 0.041f, 1f));

        private static readonly PaletteDefinition HigherContrastPalette =
            new PaletteDefinition(
                "Higher_Contrast",
                "Higher Contrast",
                new Color(0.517f, 0.503f, 0.458f, 1f),
                new Color(0.090f, 0.100f, 0.095f, 1f),
                new Color(0.640f, 0.620f, 0.550f, 1f),
                new Color(0.045f, 0.041f, 0.034f, 1f));

        private static readonly PaletteDefinition AlternatePalette =
            new PaletteDefinition(
                "Alternate",
                "Alternate",
                new Color(0.430f, 0.450f, 0.390f, 1f),
                new Color(0.110f, 0.150f, 0.120f, 1f),
                new Color(0.620f, 0.640f, 0.530f, 1f),
                new Color(0.045f, 0.055f, 0.043f, 1f));

        private static readonly PaletteDefinition[] PaletteDefinitions =
        {
            NeutralPalette,
            HigherContrastPalette,
            AlternatePalette
        };

        internal sealed class PaletteDefinition
        {
            internal PaletteDefinition(
                string stableId,
                string displayName,
                Color baseColor,
                Color darkColor,
                Color lightColor,
                Color cavityColor)
            {
                StableId = stableId;
                DisplayName = displayName;
                BaseColor = baseColor;
                DarkColor = darkColor;
                LightColor = lightColor;
                CavityColor = cavityColor;
            }

            internal string StableId { get; }
            internal string DisplayName { get; }
            internal Color BaseColor { get; }
            internal Color DarkColor { get; }
            internal Color LightColor { get; }
            internal Color CavityColor { get; }
        }

        internal sealed class CandidateDefinition
        {
            internal CandidateDefinition(
                string stableId,
                string displayName,
                int exactPlacementCount,
                float minimumCoverage,
                float maximumCoverage,
                int maximumBroadCenterCount)
            {
                StableId = stableId;
                DisplayName = displayName;
                ExactPlacementCount = exactPlacementCount;
                MinimumCoverage = minimumCoverage;
                MaximumCoverage = maximumCoverage;
                MaximumBroadCenterCount = maximumBroadCenterCount;
            }

            internal string StableId { get; }
            internal string DisplayName { get; }
            internal int ExactPlacementCount { get; }
            internal float MinimumCoverage { get; }
            internal float MaximumCoverage { get; }
            internal int MaximumBroadCenterCount { get; }
        }

        internal sealed class SourceUsage
        {
            internal string StableId;
            internal int Count;
            internal bool UsedFallbackMesh;
        }

        internal sealed class PlacementEvidence
        {
            internal int Index;
            internal string StableId;
            internal int SourceIndex;
            internal float CenterX;
            internal float CenterY;
            internal float Radius;
            internal float RotationDegrees;
            internal float UniformScale;
            internal float BurialFraction;
            internal bool UsedFallbackMesh;
            internal int RootContactPixels;
            internal float RootPerimeterAffectedFraction;
        }

        internal sealed class SeamEvidence
        {
            internal float MaskMean;
            internal float HeightMean;
            internal float NormalMean;
            internal float VariationMean;
            internal float RootMean;
            internal float WearMean;
            internal float PreviewMean;
            internal float PaletteFormMean;
            internal float PackedDetailMean;
            internal float PalettePreviewMean;

            internal bool Passed =>
                MaskMean <= MaskSeamMeanTolerance &&
                HeightMean <= HeightSeamMeanTolerance &&
                NormalMean <= NormalSeamMeanTolerance &&
                VariationMean <= ScalarSeamMeanTolerance &&
                RootMean <= ScalarSeamMeanTolerance &&
                WearMean <= ScalarSeamMeanTolerance &&
                PreviewMean <= PreviewSeamMeanTolerance &&
                PaletteFormMean <= PalettePayloadSeamMeanTolerance &&
                PackedDetailMean <= PalettePayloadSeamMeanTolerance &&
                PalettePreviewMean <= PreviewSeamMeanTolerance;
        }

        internal sealed class CandidateResult
        {
            internal CandidateDefinition Definition;
            internal readonly List<PlacementEvidence> Placements =
                new List<PlacementEvidence>();
            internal readonly List<SourceUsage> SourceUsage =
                new List<SourceUsage>();
            internal float Coverage;
            internal float QuietBlockFraction;
            internal int OccupiedQuietBlocks;
            internal int UniqueSourceCount;
            internal float MaximumObservedSourceShare;
            internal float MaximumRootPerimeterAffectedFraction;
            internal float MinimumObservedScale;
            internal float MaximumObservedScale;
            internal float MeanObservedScale;
            internal int SmallScaleCount;
            internal int MediumScaleCount;
            internal int LargeScaleCount;
            internal int AccentScaleCount;
            internal float MinimumNormalizedNeighbourSeparation;
            internal int MaximumNearNeighbourCount;
            internal int MaximumBroadCenterCount;
            internal int RejectedForSpacing;
            internal int RejectedForHotspot;
            internal int RejectedForOverlap;
            internal int RejectedForCoverage;
            internal SeamEvidence Seams;
            internal Color32[] Moderate;
            internal Color32[] PlacementDebug;
            internal Color32[] StableIdDebug;
            internal Color32[] Mask;
            internal Color32[] Height;
            internal Color32[] Normals;
            internal Color32[] Variation;
            internal Color32[] RootDarkening;
            internal Color32[] EdgeWear;
            internal Color32[] MipContactSheet;
            internal Color32[] PaletteForm;
            internal Color32[] RuntimePackedDetail;
            internal Color32[] PalettePreviewNeutral;
            internal Color32[] PalettePreviewHigherContrast;
            internal Color32[] PalettePreviewAlternate;
            internal Color32[] PaletteComparison;
            internal float PaletteFormMinimum;
            internal float PaletteFormMaximum;
            internal float PaletteFormSubstrateMean;
            internal float PaletteFormRockMean;
            internal float PackedSubstrateSlopeDeviationMean;
            internal float PackedSubstrateCavityMean;
            internal float PackedRockSlopeMagnitudeMean;
            internal float PackedRockCavityMean;
            internal float PackedRockCavityMaximum;
            internal float NeutralToHigherContrastMeanDifference;
            internal float NeutralToAlternateMeanDifference;
            internal float FractionalSilhouetteCoverageFraction;
            internal float MaximumAdjacentPaletteFormDifference;
            internal float FeatureMaskMean;
            internal float FeatureMaskMaximum;
            internal float SubstrateOnlyFormMean;
            internal float SubstrateOnlyRoughnessMean;
            internal float FeatureSubstrateRoughness;
            internal float FeatureSubstrateRoughnessMaximumDeviation;
            internal float FeatureMaximumSupportRadiusUv;
            internal float FeatureAnchorDistanceMinimum;
            internal float FeatureAnchorDistanceMaximum;
            internal float FeatureAnchorDistanceMean;
            internal int FeatureAnchorOwnerMismatchCount;
            internal int FeatureAnchorInvalidSampleCount;
            internal float FeatureAnchorMaximumCenterErrorUv;
            internal float FeatureAnchorMaximumRetentionSpread;
            internal int FeatureAnchorInconsistentRockCount;
            internal int FeatureResponseUngatedPixelCount;
            internal int FeatureNeutralGeometricMaskPixelCount;
            internal int FeatureAnchorLastAcceptedMip;
            internal Color32[] FeatureBoundarySweepContactSheet;
            internal float FeatureBoundaryHardMaximumWeightSpread;
            internal float FeatureBoundaryFadeMaximumWeightSpread;
            internal int FeatureBoundaryHardPartialRockCount;
            internal float FeatureBoundaryRemovedMaximumResidual;
            internal int FeatureBoundaryFadeInconsistentRockCount;
            internal string PalettePayloadFingerprint;
            internal string PalettePreviewNeutralFingerprint;
            internal string PalettePreviewHigherContrastFingerprint;
            internal string PalettePreviewAlternateFingerprint;
            internal string SubstrateFingerprint;
            internal string Fingerprint;
            internal string Failure;

            internal bool Succeeded => string.IsNullOrEmpty(Failure);
        }

        internal sealed class SubstrateResult
        {
            internal Color32[] Color;
            internal float[] Variation;
            internal float MeanLuminance;
            internal float FifthPercentileLuminance;
            internal float NinetyFifthPercentileLuminance;
            internal float RmsContrast;
            internal float OppositeEdgeMeanDifference;
            internal float MaximumBlockMeanDeviation64;
            internal float MaximumBlockMeanDeviation128;
            internal float MaximumBlockMeanDeviation256;
            internal float BlockMeanRmsDeviation64;
            internal float BlockMeanRmsDeviation128;
            internal float BlockMeanRmsDeviation256;
            internal string Fingerprint;
        }

        internal sealed class SuiteResult
        {
            internal readonly List<CandidateResult> Candidates =
                new List<CandidateResult>();
            internal SubstrateResult Substrate;
            internal string Fingerprint;
            internal string Failure;

            internal bool Succeeded => string.IsNullOrEmpty(Failure);
        }

        private sealed class SourceCache
        {
            internal GeneratedMassRiverRockProjectionBaker
                .FrozenSourceDefinition Definition;
            internal MeshData Mesh;
            internal bool UsedFallbackMesh;
            internal string FallbackReason;
            internal float CenterX;
            internal float CenterZ;
            internal float MinimumY;
            internal float MaximumY;
            internal float MaximumHorizontalDimension;
        }

        private sealed class WorkBuffers
        {
            internal readonly float[] Depth;
            internal readonly float[] Height;
            internal readonly float[] DetailHeight;
            internal readonly float[] Mask;
            internal readonly float[] Variation;
            internal readonly float[] Exposure;
            internal readonly float[] DirectionalLight;
            internal readonly float[] Crevice;
            internal readonly float[] EdgeWear;
            internal readonly float[] LocalX;
            internal readonly float[] LocalY;
            internal readonly Vector3[] Normals;
            internal readonly int[] Owner;

            internal WorkBuffers()
            {
                int count = WorkResolution * WorkResolution;
                Depth = new float[count];
                Height = new float[count];
                DetailHeight = new float[count];
                Mask = new float[count];
                Variation = new float[count];
                Exposure = new float[count];
                DirectionalLight = new float[count];
                Crevice = new float[count];
                EdgeWear = new float[count];
                LocalX = new float[count];
                LocalY = new float[count];
                Normals = new Vector3[count];
                Owner = new int[count];
                for (int index = 0; index < count; index++)
                {
                    Depth[index] = float.NegativeInfinity;
                    Normals[index] = Vector3.up;
                    Owner[index] = -1;
                }
            }
        }

        private sealed class FinalBuffers
        {
            internal readonly float[] Mask =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] Height =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] Variation =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] Exposure =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] DirectionalLight =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] RootDarkening =
                new float[FinalResolution * FinalResolution];
            internal readonly float[] EdgeWear =
                new float[FinalResolution * FinalResolution];
            internal readonly Vector3[] Normals =
                new Vector3[FinalResolution * FinalResolution];
            internal readonly int[] Owner =
                new int[FinalResolution * FinalResolution];
        }

        private struct RasterPixel
        {
            internal float Depth;
            internal float Height;
            internal Vector3 Normal;
            internal float Variation;
            internal float Exposure;
            internal float Crevice;
            internal float EdgeWear;
            internal float LocalX;
            internal float LocalY;
        }

        private sealed class PlacementRaster
        {
            internal readonly Dictionary<int, RasterPixel> Pixels =
                new Dictionary<int, RasterPixel>();
            internal float Radius;
        }

        private sealed class SubstrateField
        {
            private readonly int seed;

            internal SubstrateField(int seed)
            {
                this.seed = seed;
            }

            internal SubstrateSample Evaluate(int x, int y)
            {
                float coarseMicro =
                    PeriodicValueNoise(x, y, 18, seed + 101) - 0.5f;
                float mediumMicro =
                    PeriodicValueNoise(x, y, 37, seed + 307) - 0.5f;
                float fineMicro =
                    PeriodicValueNoise(x, y, 79, seed + 701) - 0.5f;
                float speckle =
                    PeriodicValueNoise(x, y, 151, seed + 1103) - 0.5f;
                float grain =
                    PeriodicValueNoise(x, y, 281, seed + 1709) - 0.5f;
                float tone = coarseMicro * 0.0140f +
                    mediumMicro * 0.0126f +
                    fineMicro * 0.0098f +
                    speckle * 0.0056f +
                    grain * 0.0035f;
                float warmDrift =
                    (mediumMicro - speckle) * 0.0049f +
                    grain * 0.0021f;
                Color baseMud = new Color(0.517f, 0.503f, 0.458f, 1f);
                Color color = new Color(
                    Mathf.Clamp01(baseMud.r + tone + warmDrift),
                    Mathf.Clamp01(baseMud.g + tone),
                    Mathf.Clamp01(baseMud.b + tone - warmDrift * 0.60f),
                    1f);
                return new SubstrateSample
                {
                    Color = color,
                    Variation = Mathf.Clamp01(
                        0.50f +
                        coarseMicro * 0.050f +
                        fineMicro * 0.040f +
                        speckle * 0.025f +
                        grain * 0.015f)
                };
            }
        }

        private struct SubstrateSample
        {
            internal Color Color;
            internal float Variation;
        }

        private struct BlockMeanDeviationMetrics
        {
            internal float MaximumDeviation;
            internal float RmsDeviation;
        }

        private struct DeterministicRandom
        {
            private uint state;

            internal DeterministicRandom(int seed)
            {
                state = (uint)Mathf.Max(1, seed);
            }

            internal uint NextUInt()
            {
                uint value = state;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                state = value != 0u ? value : 0xA341316Cu;
                return state;
            }

            internal float NextFloat()
            {
                return (NextUInt() & 0x00FFFFFFu) / 16777215f;
            }

            internal float Range(float minimum, float maximum)
            {
                return Mathf.Lerp(minimum, maximum, NextFloat());
            }

            internal int Range(int minimumInclusive, int maximumExclusive)
            {
                if (maximumExclusive <= minimumInclusive)
                {
                    return minimumInclusive;
                }

                return minimumInclusive +
                    (int)(NextUInt() %
                        (uint)(maximumExclusive - minimumInclusive));
            }
        }

        internal static IReadOnlyList<CandidateDefinition>
            GetCandidateDefinitions()
        {
            return CandidateDefinitions;
        }

        internal static IReadOnlyList<PaletteDefinition>
            GetPaletteDefinitions()
        {
            return PaletteDefinitions;
        }

        internal static SuiteResult BuildSuite()
        {
            return BuildSuite(true);
        }

        internal static SuiteResult BuildSuite(bool retainEvidence)
        {
            SuiteResult suite = new SuiteResult();
            try
            {
                suite.Substrate = BuildSubstrate();
                List<SourceCache> sources = BuildSourceCache();
                if (sources.Count != ExpectedSourceCount)
                {
                    suite.Failure = "Frozen source cache count is " +
                        sources.Count + "; expected " +
                        ExpectedSourceCount + ".";
                    return suite;
                }

                for (int index = 0;
                     index < CandidateDefinitions.Length;
                     index++)
                {
                    CandidateResult candidate = BuildCandidate(
                        CandidateDefinitions[index],
                        sources,
                        suite.Substrate);
                    suite.Candidates.Add(candidate);
                    if (!candidate.Succeeded)
                    {
                        suite.Failure = candidate.Definition.StableId +
                            " failed: " + candidate.Failure;
                        return suite;
                    }

                    if (!retainEvidence)
                    {
                        ReleaseCandidateEvidence(candidate);
                    }
                }

                suite.Fingerprint = CalculateSuiteFingerprint(suite);
                if (!retainEvidence)
                {
                    ReleaseSubstrateEvidence(suite.Substrate);
                }

                return suite;
            }
            catch (Exception exception)
            {
                suite.Failure = exception.ToString();
                return suite;
            }
        }

        private static List<SourceCache> BuildSourceCache()
        {
            IReadOnlyList<GeneratedMassRiverRockProjectionBaker
                .FrozenSourceDefinition> definitions =
                GeneratedMassRiverRockProjectionBaker
                    .GetFrozenSourceDefinitions();
            List<SourceCache> sources =
                new List<SourceCache>(definitions.Count);
            for (int index = 0; index < definitions.Count; index++)
            {
                GeneratedMassRiverRockProjectionBaker.GeneratedFrozenSource
                    generated = GeneratedMassRiverRockProjectionBaker
                        .GenerateFrozenSource(definitions[index].StableId);
                ValidateMesh(generated.Mesh);
                SourceCache source = new SourceCache
                {
                    Definition = generated.Definition,
                    Mesh = generated.Mesh,
                    UsedFallbackMesh = generated.UsedFallbackMesh,
                    FallbackReason = generated.FallbackReason
                };
                MeasureSourceBounds(source);
                sources.Add(source);
            }

            return sources;
        }

        private static void ValidateMesh(MeshData mesh)
        {
            if (mesh == null)
            {
                throw new InvalidOperationException(
                    "Frozen Generated Mass source returned a null mesh.");
            }

            mesh.Validate();
            if (!mesh.HasNormals)
            {
                throw new InvalidOperationException(
                    "Frozen Generated Mass source has no complete normals.");
            }
        }

        private static void MeasureSourceBounds(SourceCache source)
        {
            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumY = float.PositiveInfinity;
            float maximumY = float.NegativeInfinity;
            float minimumZ = float.PositiveInfinity;
            float maximumZ = float.NegativeInfinity;
            for (int index = 0; index < source.Mesh.Vertices.Count; index++)
            {
                Vector3 point = source.Mesh.Vertices[index];
                minimumX = Mathf.Min(minimumX, point.x);
                maximumX = Mathf.Max(maximumX, point.x);
                minimumY = Mathf.Min(minimumY, point.y);
                maximumY = Mathf.Max(maximumY, point.y);
                minimumZ = Mathf.Min(minimumZ, point.z);
                maximumZ = Mathf.Max(maximumZ, point.z);
            }

            source.CenterX = (minimumX + maximumX) * 0.5f;
            source.CenterZ = (minimumZ + maximumZ) * 0.5f;
            source.MinimumY = minimumY;
            source.MaximumY = maximumY;
            source.MaximumHorizontalDimension = Mathf.Max(
                0.0001f,
                Mathf.Max(maximumX - minimumX, maximumZ - minimumZ));
        }

        private static CandidateResult BuildCandidate(
            CandidateDefinition definition,
            IReadOnlyList<SourceCache> sources,
            SubstrateResult substrate)
        {
            CandidateResult result = new CandidateResult
            {
                Definition = definition,
                SubstrateFingerprint = substrate.Fingerprint
            };
            WorkBuffers raw = new WorkBuffers();
            int occupiedPixelCount = 0;
            int[] sourceCounts = new int[sources.Count];
            DeterministicRandom random =
                new DeterministicRandom(SharedPlacementSeed);
            int[] sourceOrder = BuildShuffledSourceOrder(
                sources.Count,
                ref random);
            float[] placementScales = BuildPlacementScales();

            for (int attempt = 0;
                 attempt < MaximumPlacementAttempts &&
                 result.Placements.Count < definition.ExactPlacementCount;
                 attempt++)
            {
                int placementIndex = result.Placements.Count;
                int sourceIndex = sourceOrder[placementIndex];
                SourceCache source = sources[sourceIndex];
                float scale = placementScales[placementIndex];
                float burial = random.Range(0.18f, 0.32f);
                float rotation = random.Range(0f, 360f);
                Vector2 center = SelectPlacementCenter(ref random);
                PlacementEvidence placement = new PlacementEvidence
                {
                    Index = placementIndex,
                    StableId = source.Definition.StableId,
                    SourceIndex = sourceIndex,
                    CenterX = center.x,
                    CenterY = center.y,
                    RotationDegrees = rotation,
                    UniformScale = scale,
                    BurialFraction = burial,
                    UsedFallbackMesh = source.UsedFallbackMesh
                };
                PlacementRaster raster = RasterizePlacement(
                    source,
                    placement);
                placement.Radius = raster.Radius;
                if (raster.Pixels.Count <= 0)
                {
                    continue;
                }

                if (!PassesSpacing(result.Placements, placement))
                {
                    result.RejectedForSpacing++;
                    continue;
                }

                CandidateDefinition milestone = ResolveMilestoneDefinition(
                    placementIndex + 1);
                if (!PassesHotspotLimits(
                        result.Placements,
                        placement,
                        milestone.MaximumBroadCenterCount))
                {
                    result.RejectedForHotspot++;
                    continue;
                }

                int overlap = CountOverlap(raw, raster);
                if (overlap > raster.Pixels.Count * MinimumOverlapFraction)
                {
                    result.RejectedForOverlap++;
                    continue;
                }

                int newPixels = raster.Pixels.Count - overlap;
                float projectedCoverage =
                    (occupiedPixelCount + newPixels) /
                    (float)(WorkResolution * WorkResolution);
                if (projectedCoverage > milestone.MaximumCoverage + 0.001f)
                {
                    result.RejectedForCoverage++;
                    continue;
                }

                CommitPlacement(raw, raster, placement.Index);
                occupiedPixelCount += newPixels;
                sourceCounts[sourceIndex]++;
                result.Placements.Add(placement);
            }

            if (result.Placements.Count != definition.ExactPlacementCount)
            {
                result.Failure = "Committed " + result.Placements.Count +
                    " placements; expected exactly " +
                    definition.ExactPlacementCount + ".";
                return result;
            }

            MeasurePlacementComposition(result);
            WorkBuffers processed = BuildProcessedBuffers(
                raw,
                result.Placements,
                sources);
            FinalBuffers final = Downsample(raw, processed);
            BuildFinalEvidence(
                result,
                final,
                result.Placements,
                substrate);
            result.OccupiedQuietBlocks = CountOccupiedQuietBlocks(final.Mask);
            result.QuietBlockFraction = 1f -
                result.OccupiedQuietBlocks / (float)QuietBlockCount;
            result.Coverage = CountCoveredPixels(final.Mask) /
                (float)(FinalResolution * FinalResolution);
            BuildSourceUsage(result, sources, sourceCounts);
            for (int index = 0; index < result.Placements.Count; index++)
            {
                result.MaximumRootPerimeterAffectedFraction = Mathf.Max(
                    result.MaximumRootPerimeterAffectedFraction,
                    result.Placements[index]
                        .RootPerimeterAffectedFraction);
            }
            result.Seams = MeasureSeams(
                final,
                result.Moderate,
                result.PaletteForm,
                result.RuntimePackedDetail,
                result.PalettePreviewHigherContrast);
            result.Fingerprint = CalculateCandidateFingerprint(result);
            return result;
        }

        private static int[] BuildShuffledSourceOrder(
            int sourceCount,
            ref DeterministicRandom random)
        {
            int[] order = new int[sourceCount];
            for (int index = 0; index < sourceCount; index++)
            {
                order[index] = index;
            }

            for (int index = sourceCount - 1; index > 0; index--)
            {
                int swapIndex = random.Range(0, index + 1);
                int value = order[index];
                order[index] = order[swapIndex];
                order[swapIndex] = value;
            }

            return order;
        }

        private static CandidateDefinition ResolveMilestoneDefinition(
            int placementCount)
        {
            for (int index = 0; index < CandidateDefinitions.Length; index++)
            {
                if (placementCount <=
                    CandidateDefinitions[index].ExactPlacementCount)
                {
                    return CandidateDefinitions[index];
                }
            }

            return CandidateDefinitions[CandidateDefinitions.Length - 1];
        }

        private static float[] BuildPlacementScales()
        {
            int maximumCount = CandidateDefinitions[
                CandidateDefinitions.Length - 1].ExactPlacementCount;
            int[] scaleClasses = new int[maximumCount];
            int smallCount = Mathf.RoundToInt(maximumCount * 0.75f);
            for (int index = smallCount; index < maximumCount; index++)
            {
                scaleClasses[index] = 1;
            }

            DeterministicRandom random = new DeterministicRandom(
                SharedPlacementSeed ^ 0x5343414C);
            for (int index = scaleClasses.Length - 1; index > 0; index--)
            {
                int swapIndex = random.Range(0, index + 1);
                int value = scaleClasses[index];
                scaleClasses[index] = scaleClasses[swapIndex];
                scaleClasses[swapIndex] = value;
            }

            float[] scales = new float[maximumCount];
            for (int index = 0; index < scales.Length; index++)
            {
                scales[index] = scaleClasses[index] == 0
                    ? random.Range(0.55f, 0.80f)
                    : random.Range(0.80f, 1.05f);
            }

            return scales;
        }

        private static Vector2 SelectPlacementCenter(
            ref DeterministicRandom random)
        {
            return new Vector2(
                random.Range(0f, WorkResolution),
                random.Range(0f, WorkResolution));
        }

        private static void MeasurePlacementComposition(
            CandidateResult result)
        {
            result.MinimumObservedScale = float.PositiveInfinity;
            result.MinimumNormalizedNeighbourSeparation =
                float.PositiveInfinity;
            float scaleSum = 0f;
            for (int index = 0; index < result.Placements.Count; index++)
            {
                PlacementEvidence placement = result.Placements[index];
                float scale = placement.UniformScale;
                result.MinimumObservedScale = Mathf.Min(
                    result.MinimumObservedScale,
                    scale);
                result.MaximumObservedScale = Mathf.Max(
                    result.MaximumObservedScale,
                    scale);
                scaleSum += scale;
                if (scale < 0.80f)
                {
                    result.SmallScaleCount++;
                }
                else if (scale < 1.05f)
                {
                    result.MediumScaleCount++;
                }
                else if (scale <= MaximumPlacementScale + 0.0001f)
                {
                    result.LargeScaleCount++;
                }
                else
                {
                    result.AccentScaleCount++;
                }

                int nearNeighbours = 0;
                int broadCenterCount = 1;
                for (int otherIndex = 0;
                     otherIndex < result.Placements.Count;
                     otherIndex++)
                {
                    if (otherIndex == index)
                    {
                        continue;
                    }

                    PlacementEvidence other =
                        result.Placements[otherIndex];
                    float distance = ToroidalDistance(
                        new Vector2(placement.CenterX, placement.CenterY),
                        new Vector2(other.CenterX, other.CenterY));
                    float normalized = distance /
                        Mathf.Max(1f, placement.Radius + other.Radius);
                    result.MinimumNormalizedNeighbourSeparation = Mathf.Min(
                        result.MinimumNormalizedNeighbourSeparation,
                        normalized);
                    if (distance < NearHotspotRadiusWork)
                    {
                        nearNeighbours++;
                    }

                    if (distance < BroadHotspotRadiusWork)
                    {
                        broadCenterCount++;
                    }
                }

                result.MaximumNearNeighbourCount = Mathf.Max(
                    result.MaximumNearNeighbourCount,
                    nearNeighbours);
                result.MaximumBroadCenterCount = Mathf.Max(
                    result.MaximumBroadCenterCount,
                    broadCenterCount);
            }

            result.MeanObservedScale = result.Placements.Count > 0
                ? scaleSum / result.Placements.Count
                : 0f;
            if (float.IsPositiveInfinity(result.MinimumObservedScale))
            {
                result.MinimumObservedScale = 0f;
            }

            if (float.IsPositiveInfinity(
                    result.MinimumNormalizedNeighbourSeparation))
            {
                result.MinimumNormalizedNeighbourSeparation = 0f;
            }
        }

        private static PlacementRaster RasterizePlacement(
            SourceCache source,
            PlacementEvidence placement)
        {
            MeshData mesh = source.Mesh;
            PlacementRaster output = new PlacementRaster();
            int vertexCount = mesh.Vertices.Count;
            Vector3[] positions = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Vector2[] projected = new Vector2[vertexCount];
            float radians = placement.RotationDegrees * Mathf.Deg2Rad;
            float cosine = Mathf.Cos(radians);
            float sine = Mathf.Sin(radians);
            float pixelsPerUnit = BaseRockDiameterWork *
                placement.UniformScale /
                source.MaximumHorizontalDimension;
            float burialY = source.MinimumY +
                (source.MaximumY - source.MinimumY) *
                placement.BurialFraction;
            float visibleHeight = Mathf.Max(
                0.0001f,
                source.MaximumY - burialY);
            float maximumRadius = 0f;

            for (int index = 0; index < vertexCount; index++)
            {
                Vector3 sourcePoint = mesh.Vertices[index];
                float centeredX = sourcePoint.x - source.CenterX;
                float centeredZ = sourcePoint.z - source.CenterZ;
                Vector3 transformed = new Vector3(
                    centeredX * cosine + centeredZ * sine,
                    sourcePoint.y,
                    -centeredX * sine + centeredZ * cosine);
                positions[index] = transformed;
                Vector3 sourceNormal = mesh.Normals[index];
                normals[index] = new Vector3(
                    sourceNormal.x * cosine + sourceNormal.z * sine,
                    sourceNormal.y,
                    -sourceNormal.x * sine + sourceNormal.z * cosine)
                    .normalized;
                projected[index] = new Vector2(
                    placement.CenterX + transformed.x * pixelsPerUnit,
                    placement.CenterY + transformed.z * pixelsPerUnit);
                maximumRadius = Mathf.Max(
                    maximumRadius,
                    new Vector2(
                        transformed.x * pixelsPerUnit,
                        transformed.z * pixelsPerUnit).magnitude);
            }

            output.Radius = Mathf.Max(4f, maximumRadius);
            for (int offset = 0;
                 offset < mesh.Triangles.Count;
                 offset += 3)
            {
                RasterizePlacementTriangle(
                    mesh,
                    positions,
                    normals,
                    projected,
                    mesh.Triangles[offset],
                    mesh.Triangles[offset + 1],
                    mesh.Triangles[offset + 2],
                    burialY,
                    visibleHeight,
                    placement,
                    output);
            }

            return output;
        }

        private static void RasterizePlacementTriangle(
            MeshData mesh,
            Vector3[] positions,
            Vector3[] normals,
            Vector2[] projected,
            int indexA,
            int indexB,
            int indexC,
            float burialY,
            float visibleHeight,
            PlacementEvidence placement,
            PlacementRaster output)
        {
            Vector2 pointA = projected[indexA];
            Vector2 pointB = projected[indexB];
            Vector2 pointC = projected[indexC];
            float area = Edge(pointA, pointB, pointC);
            if (Mathf.Abs(area) <= 0.00001f)
            {
                return;
            }

            int minimumX = Mathf.FloorToInt(Mathf.Min(
                pointA.x,
                Mathf.Min(pointB.x, pointC.x)));
            int maximumX = Mathf.CeilToInt(Mathf.Max(
                pointA.x,
                Mathf.Max(pointB.x, pointC.x)));
            int minimumY = Mathf.FloorToInt(Mathf.Min(
                pointA.y,
                Mathf.Min(pointB.y, pointC.y)));
            int maximumY = Mathf.CeilToInt(Mathf.Max(
                pointA.y,
                Mathf.Max(pointB.y, pointC.y)));
            Color colorA = ResolveColor(mesh, indexA);
            Color colorB = ResolveColor(mesh, indexB);
            Color colorC = ResolveColor(mesh, indexC);
            float localRadians = placement.RotationDegrees * Mathf.Deg2Rad;
            float localCosine = Mathf.Cos(localRadians);
            float localSine = Mathf.Sin(localRadians);
            Vector4 uv2A = ResolveUv2(mesh, indexA);
            Vector4 uv2B = ResolveUv2(mesh, indexB);
            Vector4 uv2C = ResolveUv2(mesh, indexC);

            for (int y = minimumY; y <= maximumY; y++)
            {
                for (int x = minimumX; x <= maximumX; x++)
                {
                    Vector2 sample = new Vector2(x + 0.5f, y + 0.5f);
                    float weightA = Edge(pointB, pointC, sample) / area;
                    float weightB = Edge(pointC, pointA, sample) / area;
                    float weightC = 1f - weightA - weightB;
                    if (weightA < -0.0001f ||
                        weightB < -0.0001f ||
                        weightC < -0.0001f)
                    {
                        continue;
                    }

                    float worldY =
                        positions[indexA].y * weightA +
                        positions[indexB].y * weightB +
                        positions[indexC].y * weightC;
                    if (worldY <= burialY)
                    {
                        continue;
                    }

                    int wrappedX = Wrap(x, WorkResolution);
                    int wrappedY = Wrap(y, WorkResolution);
                    int destination = wrappedY * WorkResolution + wrappedX;
                    RasterPixel existing;
                    if (output.Pixels.TryGetValue(destination, out existing) &&
                        worldY <= existing.Depth + 0.000001f)
                    {
                        continue;
                    }

                    Vector3 normal = (
                        normals[indexA] * weightA +
                        normals[indexB] * weightB +
                        normals[indexC] * weightC).normalized;
                    Color color =
                        colorA * weightA +
                        colorB * weightB +
                        colorC * weightC;
                    Vector4 uv2 =
                        uv2A * weightA +
                        uv2B * weightB +
                        uv2C * weightC;
                    float screenDeltaX = ToroidalDelta(
                        x + 0.5f,
                        placement.CenterX,
                        WorkResolution);
                    float screenDeltaY = ToroidalDelta(
                        y + 0.5f,
                        placement.CenterY,
                        WorkResolution);
                    float inverseRadius = 1f /
                        Mathf.Max(1f, output.Radius);
                    output.Pixels[destination] = new RasterPixel
                    {
                        Depth = worldY,
                        Height = Mathf.Clamp01(
                            (worldY - burialY) / visibleHeight),
                        Normal = normal,
                        Variation = Mathf.Clamp01(color.r),
                        Exposure = Mathf.Clamp01(color.g),
                        Crevice = Mathf.Clamp01(color.b),
                        EdgeWear = Mathf.Clamp01(
                            Mathf.Max(color.a, uv2.z)),
                        LocalX = (screenDeltaX * localCosine -
                            screenDeltaY * localSine) * inverseRadius,
                        LocalY = (screenDeltaX * localSine +
                            screenDeltaY * localCosine) * inverseRadius
                    };
                }
            }
        }

        private static bool PassesSpacing(
            IReadOnlyList<PlacementEvidence> placements,
            PlacementEvidence candidate)
        {
            Vector2 center = new Vector2(
                candidate.CenterX,
                candidate.CenterY);
            for (int index = 0; index < placements.Count; index++)
            {
                PlacementEvidence other = placements[index];
                float minimumDistance =
                    (candidate.Radius + other.Radius) * MinimumSpacingFactor;
                float distance = ToroidalDistance(
                    center,
                    new Vector2(other.CenterX, other.CenterY));
                if (distance < minimumDistance)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool PassesHotspotLimits(
            IReadOnlyList<PlacementEvidence> placements,
            PlacementEvidence candidate,
            int maximumBroadCenterCount)
        {
            Vector2 center = new Vector2(
                candidate.CenterX,
                candidate.CenterY);
            int candidateNearNeighbours = 0;
            int candidateBroadCenterCount = 1;
            for (int index = 0; index < placements.Count; index++)
            {
                PlacementEvidence other = placements[index];
                float distance = ToroidalDistance(
                    center,
                    new Vector2(other.CenterX, other.CenterY));
                if (distance < NearHotspotRadiusWork)
                {
                    candidateNearNeighbours++;
                    if (CountNeighboursWithin(
                            placements,
                            index,
                            NearHotspotRadiusWork) + 1 >
                        MaximumNearNeighbourCount)
                    {
                        return false;
                    }
                }

                if (distance < BroadHotspotRadiusWork)
                {
                    candidateBroadCenterCount++;
                    int existingCenterCountAfter = CountNeighboursWithin(
                        placements,
                        index,
                        BroadHotspotRadiusWork) + 2;
                    if (existingCenterCountAfter > maximumBroadCenterCount)
                    {
                        return false;
                    }
                }
            }

            return candidateNearNeighbours <= MaximumNearNeighbourCount &&
                candidateBroadCenterCount <= maximumBroadCenterCount;
        }

        private static int CountNeighboursWithin(
            IReadOnlyList<PlacementEvidence> placements,
            int placementIndex,
            float radius)
        {
            PlacementEvidence placement = placements[placementIndex];
            Vector2 center = new Vector2(
                placement.CenterX,
                placement.CenterY);
            int count = 0;
            for (int index = 0; index < placements.Count; index++)
            {
                if (index == placementIndex)
                {
                    continue;
                }

                PlacementEvidence other = placements[index];
                if (ToroidalDistance(
                        center,
                        new Vector2(other.CenterX, other.CenterY)) < radius)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountOverlap(
            WorkBuffers raw,
            PlacementRaster raster)
        {
            int overlap = 0;
            foreach (int index in raster.Pixels.Keys)
            {
                if (raw.Mask[index] > 0.5f)
                {
                    overlap++;
                }
            }

            return overlap;
        }

        private static void CommitPlacement(
            WorkBuffers raw,
            PlacementRaster raster,
            int ownerIndex)
        {
            foreach (KeyValuePair<int, RasterPixel> pair in raster.Pixels)
            {
                int index = pair.Key;
                RasterPixel pixel = pair.Value;
                if (pixel.Depth <= raw.Depth[index] + 0.000001f)
                {
                    continue;
                }

                raw.Depth[index] = pixel.Depth;
                raw.Height[index] = pixel.Height;
                raw.Mask[index] = 1f;
                raw.Normals[index] = pixel.Normal;
                raw.Variation[index] = pixel.Variation;
                raw.Exposure[index] = pixel.Exposure;
                raw.Crevice[index] = pixel.Crevice;
                raw.EdgeWear[index] = pixel.EdgeWear;
                raw.LocalX[index] = pixel.LocalX;
                raw.LocalY[index] = pixel.LocalY;
                raw.Owner[index] = ownerIndex;
            }
        }

        private static WorkBuffers BuildProcessedBuffers(
            WorkBuffers raw,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
        {
            WorkBuffers processed = new WorkBuffers();
            Array.Copy(raw.Mask, processed.Mask, raw.Mask.Length);
            Array.Copy(raw.Depth, processed.Depth, raw.Depth.Length);
            Array.Copy(raw.Owner, processed.Owner, raw.Owner.Length);
            Array.Copy(raw.LocalX, processed.LocalX, raw.LocalX.Length);
            Array.Copy(raw.LocalY, processed.LocalY, raw.LocalY.Length);

            float[] strongHeight = (float[])raw.Height.Clone();
            for (int pass = 0; pass < StrongHeightFilterPasses; pass++)
            {
                strongHeight = ApplyMaskedEdgeAwareHeightPass(
                    strongHeight,
                    raw,
                    StrongHeightFilterRangeSigma);
            }

            float[] mildHeight = (float[])raw.Height.Clone();
            for (int pass = 0; pass < MildHeightFilterPasses; pass++)
            {
                mildHeight = ApplyMaskedEdgeAwareHeightPass(
                    mildHeight,
                    raw,
                    MildHeightFilterRangeSigma);
            }

            Array.Copy(strongHeight, processed.Height, strongHeight.Length);
            Array.Copy(mildHeight, processed.DetailHeight, mildHeight.Length);
            Vector3[] volumeNormals = BuildHeightDerivedNormals(
                strongHeight,
                raw,
                StrongNormalStrength);
            Vector3[] planeNormals = BuildHeightDerivedNormals(
                mildHeight,
                raw,
                MildNormalStrength);
            float[] rootSeeds = new float[raw.Mask.Length];
            Vector3 lightDirection = GeneratedMassRiverRockProjectionBaker
                .FrozenDiagnosticLightDirection;

            for (int index = 0; index < raw.Mask.Length; index++)
            {
                if (raw.Mask[index] <= 0.5f)
                {
                    processed.Normals[index] = Vector3.up;
                    processed.DirectionalLight[index] = 0.5f;
                    continue;
                }

                int owner = raw.Owner[index];
                PlacementEvidence placement = placements[owner];
                SourceCache source = sources[placement.SourceIndex];
                processed.Normals[index] = Vector3.Lerp(
                    volumeNormals[index],
                    planeNormals[index],
                    MildNormalBlend).normalized;
                processed.Variation[index] = BuildMaterialVariation(
                    source.Definition.SurfaceSeed,
                    raw.LocalX[index],
                    raw.LocalY[index]);
                float upward = SmoothStep(
                    0.18f,
                    0.98f,
                    processed.Normals[index].y);
                processed.Exposure[index] = Mathf.Clamp01(
                    0.12f + upward * 0.72f);
                processed.DirectionalLight[index] = Mathf.Clamp01(
                    Vector3.Dot(
                        processed.Normals[index],
                        lightDirection) * 0.5f + 0.5f);
                rootSeeds[index] = BuildRootSeed(
                    raw,
                    processed,
                    placement,
                    source,
                    index);
            }

            float[] root = ExpandRootSectors(
                rootSeeds,
                raw,
                placements,
                sources);
            Array.Copy(root, processed.Crevice, root.Length);
            LimitRootPerimeterParticipation(
                raw,
                processed,
                placements,
                MaximumRootPerimeterFraction);
            BuildProcessedEdgeWear(raw, processed, placements, sources);
            MeasureRootEvidence(raw, processed, placements);
            return processed;
        }

        private static void MeasureRootEvidence(
            WorkBuffers raw,
            WorkBuffers processed,
            IReadOnlyList<PlacementEvidence> placements)
        {
            int[] contactPixels = new int[placements.Count];
            int[] perimeterPixels = new int[placements.Count];
            int[] affectedPerimeter = new int[placements.Count];
            for (int y = 0; y < WorkResolution; y++)
            {
                for (int x = 0; x < WorkResolution; x++)
                {
                    int index = y * WorkResolution + x;
                    int owner = raw.Owner[index];
                    if (owner < 0 || raw.Mask[index] <= 0.5f)
                    {
                        continue;
                    }

                    if (processed.Crevice[index] > RootAffectedThreshold)
                    {
                        contactPixels[owner]++;
                    }

                    if (!IsOwnerPerimeterPixel(raw, x, y, owner))
                    {
                        continue;
                    }

                    perimeterPixels[owner]++;
                    if (processed.Crevice[index] > RootAffectedThreshold)
                    {
                        affectedPerimeter[owner]++;
                    }
                }
            }

            for (int index = 0; index < placements.Count; index++)
            {
                placements[index].RootContactPixels = contactPixels[index];
                placements[index].RootPerimeterAffectedFraction =
                    perimeterPixels[index] > 0
                        ? affectedPerimeter[index] /
                            (float)perimeterPixels[index]
                        : 0f;
            }
        }

        private static bool IsOwnerPerimeterPixel(
            WorkBuffers raw,
            int x,
            int y,
            int owner)
        {
            int left = Wrap(x - 1, WorkResolution);
            int right = Wrap(x + 1, WorkResolution);
            int down = Wrap(y - 1, WorkResolution);
            int up = Wrap(y + 1, WorkResolution);
            return raw.Owner[y * WorkResolution + left] != owner ||
                raw.Owner[y * WorkResolution + right] != owner ||
                raw.Owner[down * WorkResolution + x] != owner ||
                raw.Owner[up * WorkResolution + x] != owner;
        }

        private static float[] ApplyMaskedEdgeAwareHeightPass(
            float[] source,
            WorkBuffers raw,
            float rangeSigma)
        {
            float[] output = (float[])source.Clone();
            float inverseRange = 1f /
                Mathf.Max(0.000001f, 2f * rangeSigma * rangeSigma);
            for (int y = 0; y < WorkResolution; y++)
            {
                for (int x = 0; x < WorkResolution; x++)
                {
                    int index = y * WorkResolution + x;
                    if (raw.Mask[index] <= 0.5f)
                    {
                        continue;
                    }

                    int owner = raw.Owner[index];
                    float center = source[index];
                    float weighted = center * 1.5f;
                    float totalWeight = 1.5f;
                    for (int offsetY = -1; offsetY <= 1; offsetY++)
                    {
                        for (int offsetX = -1; offsetX <= 1; offsetX++)
                        {
                            if (offsetX == 0 && offsetY == 0)
                            {
                                continue;
                            }

                            int sampleX = Wrap(x + offsetX, WorkResolution);
                            int sampleY = Wrap(y + offsetY, WorkResolution);
                            int sample = sampleY * WorkResolution + sampleX;
                            if (raw.Mask[sample] <= 0.5f ||
                                raw.Owner[sample] != owner)
                            {
                                continue;
                            }

                            float delta = source[sample] - center;
                            float rangeWeight = Mathf.Exp(
                                -(delta * delta) * inverseRange);
                            float spatialWeight = offsetX != 0 &&
                                offsetY != 0
                                    ? 0.70f
                                    : 1f;
                            float weight = rangeWeight * spatialWeight;
                            weighted += source[sample] * weight;
                            totalWeight += weight;
                        }
                    }

                    output[index] = weighted /
                        Mathf.Max(0.000001f, totalWeight);
                }
            }

            return output;
        }

        private static Vector3[] BuildHeightDerivedNormals(
            float[] height,
            WorkBuffers raw,
            float strength)
        {
            Vector3[] normals = new Vector3[height.Length];
            for (int y = 0; y < WorkResolution; y++)
            {
                for (int x = 0; x < WorkResolution; x++)
                {
                    int index = y * WorkResolution + x;
                    if (raw.Mask[index] <= 0.5f)
                    {
                        normals[index] = Vector3.up;
                        continue;
                    }

                    int owner = raw.Owner[index];
                    float center = height[index];
                    float left = SampleOwnerHeight(
                        height, raw, x - 1, y, owner, center);
                    float right = SampleOwnerHeight(
                        height, raw, x + 1, y, owner, center);
                    float down = SampleOwnerHeight(
                        height, raw, x, y - 1, owner, center);
                    float up = SampleOwnerHeight(
                        height, raw, x, y + 1, owner, center);
                    normals[index] = new Vector3(
                        -(right - left) * strength,
                        1f,
                        -(up - down) * strength).normalized;
                }
            }

            return normals;
        }

        private static float SampleOwnerHeight(
            float[] height,
            WorkBuffers raw,
            int x,
            int y,
            int owner,
            float fallback)
        {
            int wrappedX = Wrap(x, WorkResolution);
            int wrappedY = Wrap(y, WorkResolution);
            int index = wrappedY * WorkResolution + wrappedX;
            return raw.Mask[index] > 0.5f && raw.Owner[index] == owner
                ? height[index]
                : fallback;
        }

        private static float BuildMaterialVariation(
            int seed,
            float localX,
            float localY)
        {
            float phaseA = Hash01(seed * 37 + 101) * Mathf.PI * 2f;
            float phaseB = Hash01(seed * 53 + 211) * Mathf.PI * 2f;
            float phaseC = Hash01(seed * 71 + 307) * Mathf.PI * 2f;
            float broadA = Mathf.Sin(
                (localX * 0.72f + localY * 0.31f) * Mathf.PI + phaseA);
            float broadB = Mathf.Sin(
                (-localX * 0.38f + localY * 0.81f) *
                Mathf.PI * 1.25f + phaseB);
            float grain = Mathf.Sin(
                (localX * 3.1f + localY * 2.6f) *
                Mathf.PI + phaseC) * 0.65f +
                Mathf.Sin(
                    (localX * 5.3f - localY * 4.1f) *
                    Mathf.PI + phaseA * 0.63f) * 0.35f;
            float rockOffset = (Hash01(seed * 97 + 401) - 0.5f) * 0.10f;
            return Mathf.Clamp01(
                0.50f +
                rockOffset +
                broadA * 0.070f +
                broadB * 0.045f +
                grain * 0.018f);
        }

        private static float BuildRootSeed(
            WorkBuffers raw,
            WorkBuffers processed,
            PlacementEvidence placement,
            SourceCache source,
            int index)
        {
            float localX = raw.LocalX[index];
            float localY = raw.LocalY[index];
            float radius = Mathf.Sqrt(localX * localX + localY * localY);
            if (radius <= 0.0001f)
            {
                return 0f;
            }

            int seed = source.Definition.SurfaceSeed;
            float angle = Mathf.Atan2(localY, localX);
            float contactAngle = Hash01(seed * 43 + 503) * Mathf.PI * 2f;
            Vector2 radial = new Vector2(localX, localY).normalized;
            Vector2 primary = new Vector2(
                Mathf.Cos(contactAngle),
                Mathf.Sin(contactAngle));
            float primarySector = SmoothStep(
                0.16f,
                0.78f,
                Vector2.Dot(radial, primary));
            float secondaryAngle = contactAngle + Mathf.Lerp(
                1.65f,
                2.45f,
                Hash01(seed * 47 + 557));
            Vector2 secondary = new Vector2(
                Mathf.Cos(secondaryAngle),
                Mathf.Sin(secondaryAngle));
            float secondarySector = SmoothStep(
                0.38f,
                0.86f,
                Vector2.Dot(radial, secondary)) *
                Mathf.Lerp(0.18f, 0.52f, Hash01(seed * 61 + 617));
            float sector = Mathf.Max(primarySector, secondarySector);
            float breakupPhase = Hash01(seed * 59 + 601) * Mathf.PI * 2f;
            float broadBreakup = 0.5f + 0.5f * Mathf.Sin(
                angle * 2f + breakupPhase +
                Mathf.Sin(angle * 3f + contactAngle) * 0.42f);
            float broken = SmoothStep(0.42f, 0.70f, broadBreakup);
            float lowHeight = SmoothStep(
                0.48f,
                0.025f,
                processed.Height[index]);
            float sideResponse = SmoothStep(
                0.03f,
                0.45f,
                1f - processed.Normals[index].y);
            float sourceSupport = Mathf.Lerp(
                0.82f,
                1f,
                Mathf.Clamp01(raw.Crevice[index]));
            float burialStrength = Mathf.Lerp(
                0.88f,
                1.08f,
                Mathf.InverseLerp(
                    0.18f,
                    0.28f,
                    placement.BurialFraction));
            return Mathf.Clamp01(
                lowHeight *
                Mathf.Lerp(0.50f, 1f, sideResponse) *
                sector *
                Mathf.Lerp(0.52f, 1f, broken) *
                sourceSupport *
                burialStrength);
        }

        private static float[] ExpandRootSectors(
            float[] seeds,
            WorkBuffers raw,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
        {
            float[] current = (float[])seeds.Clone();
            float[] next = new float[current.Length];
            const int maximumRadius = 7;
            for (int pass = 1; pass <= maximumRadius; pass++)
            {
                Array.Copy(current, next, current.Length);
                for (int y = 0; y < WorkResolution; y++)
                {
                    for (int x = 0; x < WorkResolution; x++)
                    {
                        int index = y * WorkResolution + x;
                        int owner = raw.Owner[index];
                        if (owner < 0 || raw.Mask[index] <= 0.5f)
                        {
                            continue;
                        }

                        PlacementEvidence placement = placements[owner];
                        SourceCache source = sources[placement.SourceIndex];
                        int radius = 3 + Mathf.FloorToInt(
                            Hash01(
                                source.Definition.SurfaceSeed * 67 + 661) *
                            4.999f);
                        if (pass > radius)
                        {
                            continue;
                        }

                        float neighbour = 0f;
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            for (int offsetX = -1; offsetX <= 1; offsetX++)
                            {
                                if (offsetX == 0 && offsetY == 0)
                                {
                                    continue;
                                }

                                int sampleX = Wrap(
                                    x + offsetX,
                                    WorkResolution);
                                int sampleY = Wrap(
                                    y + offsetY,
                                    WorkResolution);
                                int sample = sampleY * WorkResolution + sampleX;
                                if (raw.Owner[sample] != owner ||
                                    raw.Mask[sample] <= 0.5f)
                                {
                                    continue;
                                }

                                float diagonal = offsetX != 0 && offsetY != 0
                                    ? 0.90f
                                    : 1f;
                                neighbour = Mathf.Max(
                                    neighbour,
                                    current[sample] * diagonal);
                            }
                        }

                        next[index] = Mathf.Max(
                            next[index],
                            neighbour * 0.86f);
                    }
                }

                float[] swap = current;
                current = next;
                next = swap;
            }

            for (int index = 0; index < current.Length; index++)
            {
                current[index] = raw.Mask[index] > 0.5f
                    ? SmoothStep(0.025f, 0.72f, current[index])
                    : 0f;
            }

            return current;
        }

        private static void LimitRootPerimeterParticipation(
            WorkBuffers raw,
            WorkBuffers processed,
            IReadOnlyList<PlacementEvidence> placements,
            float maximumFraction)
        {
            List<int>[] perimeter = new List<int>[placements.Count];
            List<int>[] affected = new List<int>[placements.Count];
            for (int index = 0; index < placements.Count; index++)
            {
                perimeter[index] = new List<int>();
                affected[index] = new List<int>();
            }

            for (int y = 0; y < WorkResolution; y++)
            {
                for (int x = 0; x < WorkResolution; x++)
                {
                    int index = y * WorkResolution + x;
                    int owner = raw.Owner[index];
                    if (owner < 0 ||
                        !IsOwnerPerimeterPixel(raw, x, y, owner))
                    {
                        continue;
                    }

                    perimeter[owner].Add(index);
                    if (processed.Crevice[index] > RootAffectedThreshold)
                    {
                        affected[owner].Add(index);
                    }
                }
            }

            for (int owner = 0; owner < placements.Count; owner++)
            {
                int allowed = Mathf.FloorToInt(
                    perimeter[owner].Count * maximumFraction);
                if (affected[owner].Count <= allowed)
                {
                    continue;
                }

                affected[owner].Sort((a, b) =>
                {
                    int comparison = processed.Crevice[b].CompareTo(
                        processed.Crevice[a]);
                    return comparison != 0 ? comparison : a.CompareTo(b);
                });
                for (int index = allowed;
                     index < affected[owner].Count;
                     index++)
                {
                    processed.Crevice[affected[owner][index]] =
                        RootAffectedThreshold * 0.92f;
                }
            }
        }

        private static void BuildProcessedEdgeWear(
            WorkBuffers raw,
            WorkBuffers processed,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
        {
            for (int y = 0; y < WorkResolution; y++)
            {
                for (int x = 0; x < WorkResolution; x++)
                {
                    int index = y * WorkResolution + x;
                    int owner = raw.Owner[index];
                    if (owner < 0 ||
                        raw.Mask[index] <= 0.5f ||
                        !IsInteriorAtRadius(
                            raw,
                            x,
                            y,
                            owner,
                            WearSilhouetteExclusionRadius))
                    {
                        continue;
                    }

                    int left = OwnerIndex(raw, x - 1, y, owner, index);
                    int right = OwnerIndex(raw, x + 1, y, owner, index);
                    int down = OwnerIndex(raw, x, y - 1, owner, index);
                    int up = OwnerIndex(raw, x, y + 1, owner, index);
                    float height = processed.DetailHeight[index];
                    float laplacian =
                        processed.DetailHeight[left] +
                        processed.DetailHeight[right] +
                        processed.DetailHeight[down] +
                        processed.DetailHeight[up] -
                        height * 4f;
                    float convex = Mathf.Clamp01(
                        Mathf.Max(0f, -laplacian) * 14f);
                    Vector3 normal = processed.Normals[index];
                    float normalBreak = 0f;
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[left]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[right]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[down]));
                    normalBreak += 1f - Mathf.Clamp01(
                        Vector3.Dot(normal, processed.Normals[up]));
                    normalBreak = Mathf.Clamp01(normalBreak * 1.85f);
                    PlacementEvidence placement = placements[owner];
                    SourceCache source = sources[placement.SourceIndex];
                    float localX = raw.LocalX[index];
                    float localY = raw.LocalY[index];
                    float phase = Hash01(
                        source.Definition.SurfaceSeed * 83 + 709) *
                        Mathf.PI * 2f;
                    float breakupA = 0.5f + 0.5f * Mathf.Sin(
                        (localX * 4.7f + localY * 3.9f) * Mathf.PI + phase);
                    float breakupB = 0.5f + 0.5f * Mathf.Sin(
                        (localX * 8.3f - localY * 5.1f) * Mathf.PI +
                        phase * 0.73f + 1.17f);
                    float intermittent = Mathf.Lerp(
                        0.12f,
                        1f,
                        SmoothStep(
                            0.40f,
                            0.76f,
                            Mathf.Lerp(breakupA, breakupB, 0.38f)));
                    float heightSupport = SmoothStep(
                        0.12f,
                        0.30f,
                        processed.Height[index]);
                    float nativeWear = Mathf.Pow(
                        Mathf.Clamp01(raw.EdgeWear[index]),
                        0.84f) * 0.60f;
                    float fallbackWear = Mathf.Max(
                        convex,
                        normalBreak) * 0.62f;
                    processed.EdgeWear[index] = Mathf.Clamp01(
                        Mathf.Max(nativeWear, fallbackWear) *
                        intermittent *
                        heightSupport);
                }
            }

            DilateInteriorWear(raw, processed, placements, sources);
            NormalizeProcessedWear(raw, processed, placements, sources);
        }

        private static void DilateInteriorWear(
            WorkBuffers raw,
            WorkBuffers processed,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
        {
            float[] current = (float[])processed.EdgeWear.Clone();
            float[] next = new float[current.Length];
            for (int pass = 1; pass <= 2; pass++)
            {
                Array.Copy(current, next, current.Length);
                for (int y = 0; y < WorkResolution; y++)
                {
                    for (int x = 0; x < WorkResolution; x++)
                    {
                        int index = y * WorkResolution + x;
                        int owner = raw.Owner[index];
                        if (owner < 0 ||
                            !IsInteriorAtRadius(
                                raw,
                                x,
                                y,
                                owner,
                                WearSilhouetteExclusionRadius))
                        {
                            continue;
                        }

                        PlacementEvidence placement = placements[owner];
                        SourceCache source = sources[placement.SourceIndex];
                        int radius = 1 + Mathf.FloorToInt(
                            Hash01(
                                source.Definition.SurfaceSeed * 89 + 733) *
                            1.999f);
                        if (pass > radius)
                        {
                            continue;
                        }

                        float strongest = 0f;
                        for (int offsetY = -1; offsetY <= 1; offsetY++)
                        {
                            for (int offsetX = -1; offsetX <= 1; offsetX++)
                            {
                                if (offsetX == 0 && offsetY == 0)
                                {
                                    continue;
                                }

                                int sampleX = Wrap(
                                    x + offsetX,
                                    WorkResolution);
                                int sampleY = Wrap(
                                    y + offsetY,
                                    WorkResolution);
                                int sample = sampleY * WorkResolution + sampleX;
                                if (raw.Owner[sample] != owner)
                                {
                                    continue;
                                }

                                strongest = Mathf.Max(
                                    strongest,
                                    current[sample]);
                            }
                        }

                        if (strongest > 0.16f)
                        {
                            next[index] = Mathf.Max(
                                next[index],
                                strongest * 0.82f);
                        }
                    }
                }

                float[] swap = current;
                current = next;
                next = swap;
            }

            Array.Copy(current, processed.EdgeWear, current.Length);
        }

        private static void NormalizeProcessedWear(
            WorkBuffers raw,
            WorkBuffers processed,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
        {
            int ownerCount = placements.Count;
            int[,] histograms = new int[ownerCount, WearNormalizationBinCount];
            int[] sampleCounts = new int[ownerCount];
            for (int index = 0; index < processed.EdgeWear.Length; index++)
            {
                int owner = raw.Owner[index];
                float value = processed.EdgeWear[index];
                if (owner < 0 || value <= 0.0001f)
                {
                    continue;
                }

                int bin = Mathf.Clamp(
                    Mathf.FloorToInt(value * WearNormalizationBinCount),
                    0,
                    WearNormalizationBinCount - 1);
                histograms[owner, bin]++;
                sampleCounts[owner]++;
            }

            float[] gains = new float[ownerCount];
            for (int owner = 0; owner < ownerCount; owner++)
            {
                gains[owner] = 1f;
                int count = sampleCounts[owner];
                if (count <= 0)
                {
                    continue;
                }

                int targetSample = Mathf.Max(
                    1,
                    Mathf.CeilToInt(count * WearNormalizationPercentile));
                int accumulated = 0;
                int percentileBin = 0;
                for (int bin = 0; bin < WearNormalizationBinCount; bin++)
                {
                    accumulated += histograms[owner, bin];
                    if (accumulated >= targetSample)
                    {
                        percentileBin = bin;
                        break;
                    }
                }

                float percentile = (percentileBin + 0.5f) /
                    WearNormalizationBinCount;
                if (percentile < MinimumWearNormalizationSignal)
                {
                    continue;
                }

                PlacementEvidence placement = placements[owner];
                SourceCache source = sources[placement.SourceIndex];
                float target = source.UsedFallbackMesh
                    ? GeneratedMassRiverRockProjectionBaker
                        .FrozenFallbackWearTargetPercentile
                    : GeneratedMassRiverRockProjectionBaker
                        .FrozenUnifiedWearTargetPercentile;
                gains[owner] = Mathf.Clamp(
                    target / percentile,
                    0.55f,
                    2.60f);
            }

            for (int index = 0; index < processed.EdgeWear.Length; index++)
            {
                int owner = raw.Owner[index];
                if (owner >= 0)
                {
                    processed.EdgeWear[index] = Mathf.Clamp01(
                        processed.EdgeWear[index] * gains[owner]);
                }
            }
        }

        private static bool IsInteriorAtRadius(
            WorkBuffers raw,
            int x,
            int y,
            int owner,
            int radius)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                for (int offsetX = -radius; offsetX <= radius; offsetX++)
                {
                    if (Mathf.Abs(offsetX) + Mathf.Abs(offsetY) > radius)
                    {
                        continue;
                    }

                    int sampleX = Wrap(x + offsetX, WorkResolution);
                    int sampleY = Wrap(y + offsetY, WorkResolution);
                    int sample = sampleY * WorkResolution + sampleX;
                    if (raw.Mask[sample] <= 0.92f ||
                        raw.Owner[sample] != owner)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static int OwnerIndex(
            WorkBuffers raw,
            int x,
            int y,
            int owner,
            int fallback)
        {
            int wrappedX = Wrap(x, WorkResolution);
            int wrappedY = Wrap(y, WorkResolution);
            int index = wrappedY * WorkResolution + wrappedX;
            return raw.Owner[index] == owner ? index : fallback;
        }

        private static FinalBuffers Downsample(
            WorkBuffers raw,
            WorkBuffers processed)
        {
            FinalBuffers final = new FinalBuffers();
            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    int destination = y * FinalResolution + x;
                    float maskSum = 0f;
                    float height = 0f;
                    float variation = 0f;
                    float exposure = 0f;
                    float directional = 0f;
                    float root = 0f;
                    float wear = 0f;
                    Vector3 normal = Vector3.zero;
                    int count = 0;
                    int owner = -1;
                    float ownerHeight = float.NegativeInfinity;
                    for (int offsetY = 0; offsetY < WorkScale; offsetY++)
                    {
                        for (int offsetX = 0; offsetX < WorkScale; offsetX++)
                        {
                            int sourceX = x * WorkScale + offsetX;
                            int sourceY = y * WorkScale + offsetY;
                            int source = sourceY * WorkResolution + sourceX;
                            maskSum += Mathf.Clamp01(raw.Mask[source]);
                            wear = Mathf.Max(wear, processed.EdgeWear[source]);
                            if (raw.Mask[source] <= 0.5f)
                            {
                                continue;
                            }

                            height += processed.Height[source];
                            variation += processed.Variation[source];
                            exposure += processed.Exposure[source];
                            directional += processed.DirectionalLight[source];
                            root += processed.Crevice[source];
                            normal += processed.Normals[source];
                            count++;
                            if (processed.Height[source] > ownerHeight)
                            {
                                ownerHeight = processed.Height[source];
                                owner = raw.Owner[source];
                            }
                        }
                    }

                    final.Mask[destination] =
                        maskSum / (WorkScale * WorkScale);
                    final.EdgeWear[destination] = wear;
                    final.Owner[destination] = owner;
                    if (count > 0)
                    {
                        float inverse = 1f / count;
                        final.Height[destination] = height * inverse;
                        final.Variation[destination] = variation * inverse;
                        final.Exposure[destination] = exposure * inverse;
                        final.DirectionalLight[destination] = directional * inverse;
                        final.RootDarkening[destination] = root * inverse;
                        final.Normals[destination] = normal.sqrMagnitude > 0f
                            ? normal.normalized
                            : Vector3.up;
                    }
                    else
                    {
                        final.Normals[destination] = Vector3.up;
                        final.DirectionalLight[destination] = 0.5f;
                    }
                }
            }

            return final;
        }

        private static SubstrateResult BuildSubstrate()
        {
            int count = FinalResolution * FinalResolution;
            SubstrateResult result = new SubstrateResult
            {
                Color = new Color32[count],
                Variation = new float[count]
            };
            SubstrateField field = new SubstrateField(SharedSubstrateSeed);
            int[] histogram = new int[256];
            double sum = 0.0;
            double sumSquares = 0.0;
            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    int index = y * FinalResolution + x;
                    SubstrateSample sample = field.Evaluate(x, y);
                    result.Color[index] = (Color32)sample.Color;
                    result.Variation[index] = sample.Variation;
                    float luminance =
                        sample.Color.r * 0.2126f +
                        sample.Color.g * 0.7152f +
                        sample.Color.b * 0.0722f;
                    sum += luminance;
                    sumSquares += luminance * luminance;
                    histogram[Mathf.Clamp(
                        Mathf.RoundToInt(luminance * 255f),
                        0,
                        255)]++;
                }
            }

            result.MeanLuminance = (float)(sum / count);
            result.RmsContrast = Mathf.Sqrt(Mathf.Max(
                0f,
                (float)(sumSquares / count) -
                    result.MeanLuminance * result.MeanLuminance));
            result.FifthPercentileLuminance = ResolveHistogramPercentile(
                histogram,
                count,
                0.05f);
            result.NinetyFifthPercentileLuminance =
                ResolveHistogramPercentile(histogram, count, 0.95f);
            result.OppositeEdgeMeanDifference =
                MeasureSubstrateEdgeDifference(result.Color);
            BlockMeanDeviationMetrics block64 =
                MeasureBlockMeanDeviation(result.Color, 64, result.MeanLuminance);
            BlockMeanDeviationMetrics block128 =
                MeasureBlockMeanDeviation(result.Color, 128, result.MeanLuminance);
            BlockMeanDeviationMetrics block256 =
                MeasureBlockMeanDeviation(result.Color, 256, result.MeanLuminance);
            result.MaximumBlockMeanDeviation64 = block64.MaximumDeviation;
            result.MaximumBlockMeanDeviation128 = block128.MaximumDeviation;
            result.MaximumBlockMeanDeviation256 = block256.MaximumDeviation;
            result.BlockMeanRmsDeviation64 = block64.RmsDeviation;
            result.BlockMeanRmsDeviation128 = block128.RmsDeviation;
            result.BlockMeanRmsDeviation256 = block256.RmsDeviation;
            result.Fingerprint = CalculateSubstrateFingerprint(result);
            return result;
        }

        private static float ResolveHistogramPercentile(
            IReadOnlyList<int> histogram,
            int sampleCount,
            float percentile)
        {
            int target = Mathf.Max(
                1,
                Mathf.CeilToInt(sampleCount * percentile));
            int accumulated = 0;
            for (int index = 0; index < histogram.Count; index++)
            {
                accumulated += histogram[index];
                if (accumulated >= target)
                {
                    return index / 255f;
                }
            }

            return 1f;
        }

        private static float MeasureSubstrateEdgeDifference(
            IReadOnlyList<Color32> color)
        {
            double difference = 0.0;
            int samples = 0;
            for (int index = 0; index < FinalResolution; index++)
            {
                difference += ColorDifference(
                    color[index * FinalResolution],
                    color[index * FinalResolution + FinalResolution - 1]);
                difference += ColorDifference(
                    color[index],
                    color[(FinalResolution - 1) * FinalResolution + index]);
                samples += 2;
            }

            return (float)(difference / Mathf.Max(1, samples));
        }

        private static float ColorDifference(Color32 a, Color32 b)
        {
            return (
                Mathf.Abs(a.r - b.r) +
                Mathf.Abs(a.g - b.g) +
                Mathf.Abs(a.b - b.b)) /
                (255f * 3f);
        }

        private static BlockMeanDeviationMetrics MeasureBlockMeanDeviation(
            IReadOnlyList<Color32> color,
            int blockSize,
            float globalMeanLuminance)
        {
            int blocksPerAxis = FinalResolution / blockSize;
            int samplesPerBlock = blockSize * blockSize;
            float maximumDeviation = 0f;
            double sumSquaredDeviation = 0.0;
            int blockCount = 0;
            for (int blockY = 0; blockY < blocksPerAxis; blockY++)
            {
                int originY = blockY * blockSize;
                for (int blockX = 0; blockX < blocksPerAxis; blockX++)
                {
                    int originX = blockX * blockSize;
                    double blockSum = 0.0;
                    for (int localY = 0; localY < blockSize; localY++)
                    {
                        int row = (originY + localY) * FinalResolution + originX;
                        for (int localX = 0; localX < blockSize; localX++)
                        {
                            blockSum += ResolveLuminance(color[row + localX]);
                        }
                    }

                    float blockMean = (float)(blockSum / samplesPerBlock);
                    float deviation = Mathf.Abs(blockMean - globalMeanLuminance);
                    maximumDeviation = Mathf.Max(maximumDeviation, deviation);
                    sumSquaredDeviation += deviation * deviation;
                    blockCount++;
                }
            }

            return new BlockMeanDeviationMetrics
            {
                MaximumDeviation = maximumDeviation,
                RmsDeviation = blockCount > 0
                    ? Mathf.Sqrt((float)(sumSquaredDeviation / blockCount))
                    : 0f
            };
        }

        private static float ResolveLuminance(Color32 color)
        {
            return (
                color.r / 255f * 0.2126f +
                color.g / 255f * 0.7152f +
                color.b / 255f * 0.0722f);
        }

        private static string CalculateSubstrateFingerprint(
            SubstrateResult substrate)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                WritePixels(writer, substrate.Color);
                writer.Write(substrate.Variation.Length);
                for (int index = 0;
                     index < substrate.Variation.Length;
                     index++)
                {
                    writer.Write(ToByte(substrate.Variation[index]));
                }

                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static void BuildFinalEvidence(
            CandidateResult result,
            FinalBuffers final,
            IReadOnlyList<PlacementEvidence> placements,
            SubstrateResult substrate)
        {
            int count = FinalResolution * FinalResolution;
            result.Moderate = new Color32[count];
            result.Mask = new Color32[count];
            result.Height = new Color32[count];
            result.Normals = new Color32[count];
            result.Variation = new Color32[count];
            result.RootDarkening = new Color32[count];
            result.EdgeWear = new Color32[count];
            result.StableIdDebug = new Color32[count];
            result.PaletteForm = new Color32[count];
            result.RuntimePackedDetail = new Color32[count];
            float[] silhouetteCoverage =
                BuildPayloadSilhouetteCoverage(final.Mask);
            float[] featureCenterOffsetX = new float[count];
            float[] featureCenterOffsetY = new float[count];
            int[] featureAnchorOwner = new int[count];
            MeasureFeaturePayloadMetadata(result, placements, substrate);
            BuildFeatureAnchorPayload(
                placements,
                result.FeatureMaximumSupportRadiusUv,
                featureCenterOffsetX,
                featureCenterOffsetY,
                featureAnchorOwner);
            for (int index = 0; index < count; index++)
            {
                float rockCoverage = silhouetteCoverage[index];
                bool hasRockData =
                    final.Mask[index] > 0.0001f &&
                    final.Owner[index] >= 0;
                bool rockCore = final.Mask[index] > 0.5f;
                result.Mask[index] = Grayscale(ToByte(final.Mask[index]));
                result.Height[index] = Grayscale(ToByte(final.Height[index]));
                result.Normals[index] = EncodeWorldNormal(
                    final.Normals[index]);
                result.Variation[index] = Grayscale(
                    ToByte(hasRockData
                        ? Mathf.Lerp(
                            substrate.Variation[index],
                            final.Variation[index],
                            rockCoverage)
                        : substrate.Variation[index]));
                result.RootDarkening[index] = Grayscale(
                    ToByte(final.RootDarkening[index] * rockCoverage));
                result.EdgeWear[index] = Grayscale(
                    ToByte(final.EdgeWear[index] * rockCoverage));

                Color substrateColor = substrate.Color[index];
                float substrateForm = ResolveSubstratePaletteForm(
                    substrate.Variation[index]);
                Color rockColor = substrateColor;
                float rockForm = substrateForm;
                if (hasRockData)
                {
                    rockColor = GeneratedMassRiverRockProjectionBaker
                        .EvaluateFrozenModerateMaterial(
                            final.Height[index],
                            final.Variation[index],
                            final.Exposure[index],
                            final.DirectionalLight[index],
                            final.RootDarkening[index],
                            final.EdgeWear[index]);
                    rockForm = ResolveRockPaletteForm(rockColor);
                }

                Color moderateColor = Color.Lerp(
                    substrateColor,
                    rockColor,
                    rockCoverage);
                float paletteForm = Mathf.Lerp(
                    substrateForm,
                    rockForm,
                    rockCoverage);
                result.StableIdDebug[index] = rockCore && hasRockData
                    ? ResolveStableIdColor(
                        placements[final.Owner[index]].SourceIndex)
                    : substrate.Color[index];
                result.Moderate[index] = (Color32)moderateColor;
                if (rockCoverage > 0.001f &&
                    final.Owner[index] >= 0 &&
                    featureAnchorOwner[index] != final.Owner[index])
                {
                    result.FeatureAnchorOwnerMismatchCount++;
                }
                result.PaletteForm[index] = EncodePalettePayload(
                    paletteForm,
                    substrateForm,
                    featureCenterOffsetX[index],
                    featureCenterOffsetY[index]);
                result.RuntimePackedDetail[index] =
                    BuildRuntimePackedDetailPixel(
                        final,
                        substrate,
                        index,
                        rockCoverage,
                        hasRockData);
            }

            result.PalettePreviewNeutral = BuildPalettePreview(
                result.PaletteForm,
                result.RuntimePackedDetail,
                NeutralPalette);
            result.PalettePreviewHigherContrast = BuildPalettePreview(
                result.PaletteForm,
                result.RuntimePackedDetail,
                HigherContrastPalette);
            result.PalettePreviewAlternate = BuildPalettePreview(
                result.PaletteForm,
                result.RuntimePackedDetail,
                AlternatePalette);
            result.PaletteComparison = BuildPaletteComparison(
                result.PalettePreviewNeutral,
                result.PalettePreviewHigherContrast,
                result.PalettePreviewAlternate,
                result.PaletteForm);
            MeasurePalettePayload(result, final);
            float[] decodedFeatureAnchorX =
                DecodeFeatureAnchorXPayload(result.PaletteForm);
            float[] decodedFeatureAnchorY =
                DecodeFeatureAnchorYPayload(result.PaletteForm);
            MeasureFeatureAnchorReconstruction(
                result,
                placements,
                decodedFeatureAnchorX,
                decodedFeatureAnchorY);
            MeasureWholeFeatureBoundaryComposition(
                result,
                final,
                placements,
                decodedFeatureAnchorX,
                decodedFeatureAnchorY);
            result.PalettePayloadFingerprint =
                CalculatePairedPayloadFingerprint(result);
            result.PalettePreviewNeutralFingerprint =
                CalculatePixelFingerprint(
                    NeutralPalette.StableId,
                    result.PalettePreviewNeutral);
            result.PalettePreviewHigherContrastFingerprint =
                CalculatePixelFingerprint(
                    HigherContrastPalette.StableId,
                    result.PalettePreviewHigherContrast);
            result.PalettePreviewAlternateFingerprint =
                CalculatePixelFingerprint(
                    AlternatePalette.StableId,
                    result.PalettePreviewAlternate);
            result.PlacementDebug = BuildPlacementDebug(
                result.Moderate,
                placements);
            result.MipContactSheet = BuildMipContactSheet(result.Moderate);
        }

        private static float[] BuildPayloadSilhouetteCoverage(
            IReadOnlyList<float> mask)
        {
            int count = FinalResolution * FinalResolution;
            float[] horizontal = new float[count];
            float[] coverage = new float[count];
            int[] weights = { 1, 6, 15, 20, 15, 6, 1 };
            const float inverseWeightSum = 1f / 64f;

            for (int y = 0; y < FinalResolution; y++)
            {
                int row = y * FinalResolution;
                for (int x = 0; x < FinalResolution; x++)
                {
                    float sum = 0f;
                    for (int offset = -SilhouetteFilterRadius;
                         offset <= SilhouetteFilterRadius;
                         offset++)
                    {
                        int sampleX = Wrap(x + offset, FinalResolution);
                        sum += Mathf.Clamp01(mask[row + sampleX]) *
                            weights[offset + SilhouetteFilterRadius];
                    }

                    horizontal[row + x] = sum * inverseWeightSum;
                }
            }

            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    float sum = 0f;
                    for (int offset = -SilhouetteFilterRadius;
                         offset <= SilhouetteFilterRadius;
                         offset++)
                    {
                        int sampleY = Wrap(y + offset, FinalResolution);
                        sum += horizontal[
                            sampleY * FinalResolution + x] *
                            weights[offset + SilhouetteFilterRadius];
                    }

                    int index = y * FinalResolution + x;
                    float filteredCoverage = sum * inverseWeightSum;
                    coverage[index] = mask[index] > 0.0001f
                        ? SmoothStep(
                            SilhouetteCoverageLower,
                            SilhouetteCoverageUpper,
                            filteredCoverage)
                        : 0f;
                }
            }

            return coverage;
        }

        private static float ResolveSubstratePaletteForm(
            float substrateVariation)
        {
            return Mathf.Clamp(
                SubstratePaletteFormCenter +
                (substrateVariation - 0.5f) * SubstratePaletteFormGain,
                0.54f,
                0.70f);
        }

        private static float ResolveRockPaletteForm(Color moderateColor)
        {
            float luminance =
                moderateColor.r * 0.2126f +
                moderateColor.g * 0.7152f +
                moderateColor.b * 0.0722f;
            if (luminance <= RockFormMedianLuminance)
            {
                float darkT = Mathf.InverseLerp(
                    RockFormLowLuminance,
                    RockFormMedianLuminance,
                    luminance);
                return Mathf.Lerp(
                    RockFormDarkMinimum,
                    RockFormBaseMedian,
                    darkT);
            }

            float lightT = Mathf.InverseLerp(
                RockFormMedianLuminance,
                RockFormHighLuminance,
                luminance);
            return Mathf.Lerp(
                RockFormBaseMedian,
                RockFormLightMaximum,
                lightT);
        }

        private static Color32 EncodePalettePayload(
            float combinedForm,
            float substrateForm,
            float featureCenterOffsetX,
            float featureCenterOffsetY)
        {
            return new Color32(
                EncodeLinearSrgbByte(combinedForm),
                EncodeLinearSrgbByte(substrateForm),
                EncodeLinearSrgbByte(
                    featureCenterOffsetX * 0.5f + 0.5f),
                ToByte(featureCenterOffsetY * 0.5f + 0.5f));
        }

        private static byte EncodeLinearSrgbByte(float value)
        {
            return ToByte(
                Mathf.LinearToGammaSpace(Mathf.Clamp01(value)));
        }

        private static float DecodePaletteForm(Color32 encodedForm)
        {
            return Mathf.Clamp01(
                Mathf.GammaToLinearSpace(encodedForm.r / 255f));
        }

        private static float ResolveSubstrateRoughness(
            float substrateVariation)
        {
            return Mathf.Clamp(
                0.68f +
                (0.5f - substrateVariation) * 0.10f,
                0.55f,
                0.80f);
        }

        private static Color32 BuildRuntimePackedDetailPixel(
            FinalBuffers final,
            SubstrateResult substrate,
            int index,
            float rockCoverage,
            bool hasRockData)
        {
            float coverage = Mathf.Clamp01(rockCoverage);
            Vector3 rockNormal = hasRockData
                ? final.Normals[index]
                : Vector3.up;
            Vector3 normal = Vector3.Lerp(
                Vector3.up,
                rockNormal,
                coverage).normalized;
            float safeY = Mathf.Max(0.25f, Mathf.Abs(normal.y));
            Vector2 slope = Vector2.ClampMagnitude(
                new Vector2(normal.x, normal.z) / safeY,
                1f);
            float cavity = hasRockData
                ? Mathf.Clamp01(
                    final.RootDarkening[index] * 1.05f * coverage)
                : 0f;
            float substrateRoughness = ResolveSubstrateRoughness(
                substrate.Variation[index]);
            float rockRoughness = hasRockData
                ? Mathf.Clamp(
                    0.64f +
                    (0.5f - final.Variation[index]) * 0.18f +
                    final.RootDarkening[index] * 0.12f -
                    final.EdgeWear[index] * 0.08f,
                    0.35f,
                    0.90f)
                : substrateRoughness;
            float roughness = Mathf.Lerp(
                substrateRoughness,
                rockRoughness,
                coverage);
            return new Color32(
                ToByte(slope.x * 0.5f + 0.5f),
                ToByte(slope.y * 0.5f + 0.5f),
                ToByte(cavity),
                ToByte(roughness));
        }

        private static Color32[] BuildPalettePreview(
            IReadOnlyList<Color32> paletteForm,
            IReadOnlyList<Color32> packedDetail,
            PaletteDefinition palette)
        {
            int count = FinalResolution * FinalResolution;
            Color32[] output = new Color32[count];
            for (int index = 0; index < count; index++)
            {
                float form = DecodePaletteForm(paletteForm[index]);
                float signedForm = form * 2f - 1f;
                Color color = signedForm < 0f
                    ? Color.Lerp(
                        palette.BaseColor,
                        palette.DarkColor,
                        -signedForm)
                    : Color.Lerp(
                        palette.BaseColor,
                        palette.LightColor,
                        signedForm);
                float cavityRaw = Mathf.Clamp01(
                    (packedDetail[index].b / 255f -
                        PalettePreviewCavityBias) /
                    Mathf.Max(0.001f, 1f - PalettePreviewCavityBias));
                float cavity = SmoothStep(0f, 0.82f, cavityRaw);
                float cavityCore = SmoothStep(0.66f, 0.98f, cavityRaw);
                color = Color.Lerp(
                    color,
                    palette.DarkColor,
                    Mathf.Clamp01(cavity * 0.42f));
                color = Color.Lerp(
                    color,
                    palette.CavityColor,
                    cavityCore);
                output[index] = (Color32)color;
            }

            return output;
        }

        private static Color32[] BuildSubstratePalettePreview(
            IReadOnlyList<Color32> paletteForm,
            PaletteDefinition palette)
        {
            int count = FinalResolution * FinalResolution;
            Color32[] output = new Color32[count];
            for (int index = 0; index < count; index++)
            {
                float form = Mathf.Clamp01(
                    Mathf.GammaToLinearSpace(
                        paletteForm[index].g / 255f));
                float signedForm = form * 2f - 1f;
                Color color = signedForm < 0f
                    ? Color.Lerp(
                        palette.BaseColor,
                        palette.DarkColor,
                        -signedForm)
                    : Color.Lerp(
                        palette.BaseColor,
                        palette.LightColor,
                        signedForm);
                output[index] = (Color32)color;
            }

            return output;
        }

        private static Color32[] BuildPaletteComparison(
            Color32[] neutral,
            Color32[] higherContrast,
            Color32[] alternate,
            Color32[] paletteForm)
        {
            Color32[] output = new Color32[
                FinalResolution * FinalResolution];
            int panelSize = FinalResolution / 2;
            BlitScaled(
                neutral,
                FinalResolution,
                output,
                FinalResolution,
                0,
                panelSize,
                panelSize);
            BlitScaled(
                higherContrast,
                FinalResolution,
                output,
                FinalResolution,
                panelSize,
                panelSize,
                panelSize);
            BlitScaled(
                alternate,
                FinalResolution,
                output,
                FinalResolution,
                0,
                0,
                panelSize);
            Color32[] paletteFormPreview = new Color32[paletteForm.Length];
            for (int index = 0; index < paletteForm.Length; index++)
            {
                paletteFormPreview[index] = Grayscale(paletteForm[index].r);
            }
            BlitScaled(
                paletteFormPreview,
                FinalResolution,
                output,
                FinalResolution,
                panelSize,
                0,
                panelSize);
            return output;
        }

        private static void MeasurePalettePayload(
            CandidateResult result,
            FinalBuffers final)
        {
            double substrateForm = 0.0;
            double rockForm = 0.0;
            double substrateSlope = 0.0;
            double substrateCavity = 0.0;
            double rockSlope = 0.0;
            double rockCavity = 0.0;
            int substrateCount = 0;
            int rockCount = 0;
            int fractionalCoverageCount = 0;
            double featureMask = 0.0;
            double substrateOnlyForm = 0.0;
            double featureAnchorDistance = 0.0;
            int featureAnchorCount = 0;
            int featureResponseUngatedPixelCount = 0;
            int featureNeutralGeometricMaskPixelCount = 0;
            float minimumFeatureAnchorDistance = 1f;
            float maximumFeatureAnchorDistance = 0f;
            float maximumFeatureMask = 0f;
            float minimumForm = 1f;
            float maximumForm = 0f;
            float maximumRockCavity = 0f;
            for (int index = 0; index < result.PaletteForm.Length; index++)
            {
                Color32 palettePayload = result.PaletteForm[index];
                float form = DecodePaletteForm(palettePayload);
                float substrateFormValue = Mathf.Clamp01(
                    Mathf.GammaToLinearSpace(palettePayload.g / 255f));
                float featureOffsetX =
                    Mathf.Clamp01(
                        Mathf.GammaToLinearSpace(
                            palettePayload.b / 255f)) * 2f - 1f;
                float featureOffsetY =
                    palettePayload.a / 255f * 2f - 1f;
                float featureAnchorDistanceValue = Mathf.Sqrt(
                    featureOffsetX * featureOffsetX +
                    featureOffsetY * featureOffsetY);
                Color32 packed = result.RuntimePackedDetail[index];
                float slopeX = packed.r / 255f * 2f - 1f;
                float slopeY = packed.g / 255f * 2f - 1f;
                float slopeMagnitude = Mathf.Sqrt(
                    slopeX * slopeX + slopeY * slopeY);
                float cavity = packed.b / 255f;
                float roughness = packed.a / 255f;
                float formEvidence = Mathf.Abs(
                    form - substrateFormValue);
                float roughnessEvidence = Mathf.Abs(
                    roughness - result.FeatureSubstrateRoughness);
                bool hasSlopeResponse =
                    slopeMagnitude >= FeatureSlopeEvidenceThreshold;
                bool hasCavityResponse =
                    cavity >= FeatureCavityEvidenceThreshold;
                bool hasFormResponse =
                    formEvidence >= FeatureFormEvidenceThreshold;
                bool hasRoughnessResponse =
                    roughnessEvidence >= FeatureRoughnessEvidenceThreshold;
                bool hasEmittedFeatureResponse =
                    hasSlopeResponse ||
                    hasCavityResponse ||
                    hasFormResponse ||
                    hasRoughnessResponse;
                float decodedFeatureMaskValue =
                    hasSlopeResponse ||
                    hasCavityResponse ||
                    hasRoughnessResponse
                        ? 1f
                        : 0f;
                float formFeatureMaskValue = hasFormResponse ? 1f : 0f;
                float featureMaskValue = Mathf.Max(
                    decodedFeatureMaskValue,
                    formFeatureMaskValue);
                substrateOnlyForm += substrateFormValue;
                if (featureMaskValue > AnchorProofCoverageThreshold)
                {
                    featureAnchorDistance += featureAnchorDistanceValue;
                    featureAnchorCount++;
                    minimumFeatureAnchorDistance = Mathf.Min(
                        minimumFeatureAnchorDistance,
                        featureAnchorDistanceValue);
                    maximumFeatureAnchorDistance = Mathf.Max(
                        maximumFeatureAnchorDistance,
                        featureAnchorDistanceValue);
                }
                featureMask += featureMaskValue;
                maximumFeatureMask = Mathf.Max(
                    maximumFeatureMask,
                    featureMaskValue);
                float maskCoverage = Mathf.Clamp01(final.Mask[index]);
                if (hasEmittedFeatureResponse && featureMaskValue < 0.5f)
                {
                    featureResponseUngatedPixelCount++;
                }

                if (maskCoverage > AnchorProofCoverageThreshold &&
                    !hasEmittedFeatureResponse)
                {
                    featureNeutralGeometricMaskPixelCount++;
                }

                if (maskCoverage > 0.0001f && maskCoverage < 0.9999f)
                {
                    fractionalCoverageCount++;
                }
                minimumForm = Mathf.Min(minimumForm, form);
                maximumForm = Mathf.Max(maximumForm, form);
                if (final.Mask[index] > 0.5f)
                {
                    rockForm += form;
                    rockSlope += slopeMagnitude;
                    rockCavity += cavity;
                    maximumRockCavity = Mathf.Max(
                        maximumRockCavity,
                        cavity);
                    rockCount++;
                }
                else
                {
                    substrateForm += form;
                    substrateSlope += slopeMagnitude;
                    substrateCavity += cavity;
                    substrateCount++;
                }
            }

            result.PaletteFormMinimum = minimumForm;
            result.PaletteFormMaximum = maximumForm;
            result.PaletteFormSubstrateMean = substrateCount > 0
                ? (float)(substrateForm / substrateCount)
                : 0f;
            result.PaletteFormRockMean = rockCount > 0
                ? (float)(rockForm / rockCount)
                : 0f;
            result.PackedSubstrateSlopeDeviationMean = substrateCount > 0
                ? (float)(substrateSlope / substrateCount)
                : 0f;
            result.PackedSubstrateCavityMean = substrateCount > 0
                ? (float)(substrateCavity / substrateCount)
                : 0f;
            result.PackedRockSlopeMagnitudeMean = rockCount > 0
                ? (float)(rockSlope / rockCount)
                : 0f;
            result.PackedRockCavityMean = rockCount > 0
                ? (float)(rockCavity / rockCount)
                : 0f;
            result.PackedRockCavityMaximum = maximumRockCavity;
            result.NeutralToHigherContrastMeanDifference =
                MeasureMeanColorDifference(
                    result.PalettePreviewNeutral,
                    result.PalettePreviewHigherContrast);
            result.NeutralToAlternateMeanDifference =
                MeasureMeanColorDifference(
                    result.PalettePreviewNeutral,
                    result.PalettePreviewAlternate);
            result.FractionalSilhouetteCoverageFraction =
                fractionalCoverageCount /
                (float)Mathf.Max(1, final.Mask.Length);
            result.MaximumAdjacentPaletteFormDifference =
                MeasureMaximumAdjacentPaletteFormDifference(
                    result.PaletteForm);
            float payloadCount = Mathf.Max(1, result.PaletteForm.Length);
            result.FeatureMaskMean = (float)(featureMask / payloadCount);
            result.FeatureMaskMaximum = maximumFeatureMask;
            result.FeatureResponseUngatedPixelCount =
                featureResponseUngatedPixelCount;
            result.FeatureNeutralGeometricMaskPixelCount =
                featureNeutralGeometricMaskPixelCount;
            result.SubstrateOnlyFormMean =
                (float)(substrateOnlyForm / payloadCount);
            result.SubstrateOnlyRoughnessMean =
                result.FeatureSubstrateRoughness;
            result.FeatureAnchorDistanceMinimum = featureAnchorCount > 0
                ? minimumFeatureAnchorDistance
                : 0f;
            result.FeatureAnchorDistanceMaximum = featureAnchorCount > 0
                ? maximumFeatureAnchorDistance
                : 0f;
            result.FeatureAnchorDistanceMean = featureAnchorCount > 0
                ? (float)(featureAnchorDistance / featureAnchorCount)
                : 0f;
        }

        private static void BuildFeatureAnchorPayload(
            IReadOnlyList<PlacementEvidence> placements,
            float maximumSupportRadiusUv,
            float[] centerOffsetX,
            float[] centerOffsetY,
            int[] anchorOwner)
        {
            float maximumSupportRadiusWork = Mathf.Max(
                0.0001f,
                maximumSupportRadiusUv * WorkResolution);
            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    int index = y * FinalResolution + x;
                    Vector2 point = new Vector2(
                        (x + 0.5f) * WorkScale,
                        (y + 0.5f) * WorkScale);
                    float bestDistance = float.PositiveInfinity;
                    int bestOwner = -1;
                    for (int placementIndex = 0;
                         placementIndex < placements.Count;
                         placementIndex++)
                    {
                        PlacementEvidence placement = placements[placementIndex];
                        float distance = ToroidalDistance(
                            point,
                            new Vector2(
                                placement.CenterX,
                                placement.CenterY));
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestOwner = placementIndex;
                        }
                    }

                    if (bestOwner < 0)
                    {
                        centerOffsetX[index] = 1f;
                        centerOffsetY[index] = 1f;
                        anchorOwner[index] = -1;
                        continue;
                    }

                    PlacementEvidence owner = placements[bestOwner];
                    centerOffsetX[index] = Mathf.Clamp(
                        ToroidalDelta(
                            point.x,
                            owner.CenterX,
                            WorkResolution) /
                        maximumSupportRadiusWork,
                        -1f,
                        1f);
                    centerOffsetY[index] = Mathf.Clamp(
                        ToroidalDelta(
                            point.y,
                            owner.CenterY,
                            WorkResolution) /
                        maximumSupportRadiusWork,
                        -1f,
                        1f);
                    anchorOwner[index] = bestOwner;
                }
            }
        }

        private static void MeasureFeaturePayloadMetadata(
            CandidateResult result,
            IReadOnlyList<PlacementEvidence> placements,
            SubstrateResult substrate)
        {
            double roughnessSum = 0.0;
            for (int index = 0; index < substrate.Variation.Length; index++)
            {
                roughnessSum += ResolveSubstrateRoughness(
                    substrate.Variation[index]);
            }

            result.FeatureSubstrateRoughness =
                (float)(roughnessSum /
                    Mathf.Max(1, substrate.Variation.Length));
            float maximumDeviation = 0f;
            for (int index = 0; index < substrate.Variation.Length; index++)
            {
                maximumDeviation = Mathf.Max(
                    maximumDeviation,
                    Mathf.Abs(
                        ResolveSubstrateRoughness(
                            substrate.Variation[index]) -
                        result.FeatureSubstrateRoughness));
            }

            result.FeatureSubstrateRoughnessMaximumDeviation =
                maximumDeviation;
            float maximumRadius = 0f;
            for (int index = 0; index < placements.Count; index++)
            {
                maximumRadius = Mathf.Max(
                    maximumRadius,
                    placements[index].Radius);
            }

            result.FeatureMaximumSupportRadiusUv =
                maximumRadius / WorkResolution +
                AnchorSupportPaddingTexels / FinalResolution;
        }

        private static float[] DecodeFeatureAnchorXPayload(
            IReadOnlyList<Color32> paletteForm)
        {
            float[] offset = new float[paletteForm.Count];
            for (int index = 0; index < paletteForm.Count; index++)
            {
                offset[index] =
                    Mathf.Clamp01(
                        Mathf.GammaToLinearSpace(
                            paletteForm[index].b / 255f)) *
                    2f - 1f;
            }

            return offset;
        }

        private static float[] DecodeFeatureAnchorYPayload(
            IReadOnlyList<Color32> paletteForm)
        {
            float[] offset = new float[paletteForm.Count];
            for (int index = 0; index < paletteForm.Count; index++)
            {
                offset[index] = paletteForm[index].a / 255f * 2f - 1f;
            }

            return offset;
        }

        private static void MeasureFeatureAnchorReconstruction(
            CandidateResult result,
            IReadOnlyList<PlacementEvidence> placements,
            float[] baseCenterOffsetX,
            float[] baseCenterOffsetY)
        {
            float[] centerOffsetX = baseCenterOffsetX;
            float[] centerOffsetY = baseCenterOffsetY;
            int resolution = FinalResolution;
            int lastAcceptedMip = -1;
            float maximumCenterErrorAcrossMips = 0f;
            float maximumSpreadAcrossMips = 0f;
            int invalidSamplesAcrossMips = 0;
            int inconsistentRocksAcrossMips = 0;
            float supportRadiusUv = result.FeatureMaximumSupportRadiusUv;

            for (int mip = 0;
                 mip <= AnchorProofMaximumMip && resolution >= 8;
                 mip++)
            {
                float minimumRadiusPixels = float.PositiveInfinity;
                for (int index = 0; index < placements.Count; index++)
                {
                    minimumRadiusPixels = Mathf.Min(
                        minimumRadiusPixels,
                        placements[index].Radius /
                            WorkResolution * resolution);
                }

                // Once the smallest visible rock falls below this footprint,
                // centre-anchor reconstruction is no longer a meaningful
                // whole-rock contract. Do not count the first unsupported mip
                // as a proof failure; the validator requires the last accepted
                // mip to reach the production-relevant threshold.
                if (minimumRadiusPixels < 1.25f)
                {
                    break;
                }

                float maximumCenterError = 0f;
                int invalidSamples = 0;
                int acceptedSamples = 0;
                float[] rockMinimum = new float[placements.Count];
                float[] rockMaximum = new float[placements.Count];
                for (int index = 0; index < placements.Count; index++)
                {
                    rockMinimum[index] = float.PositiveInfinity;
                    rockMaximum[index] = float.NegativeInfinity;
                }

                float cellGuard = 0.75f * 1.41421356237f / resolution;
                for (int y = 0; y < resolution; y++)
                {
                    for (int x = 0; x < resolution; x++)
                    {
                        float cellCenterX = (x + 0.5f) / resolution;
                        float cellCenterY = (y + 0.5f) / resolution;
                        int cellOwner = FindNearestPlacementUv(
                            placements,
                            cellCenterX,
                            cellCenterY);
                        if (cellOwner < 0)
                        {
                            continue;
                        }

                        PlacementEvidence cellPlacement =
                            placements[cellOwner];
                        float cellRadiusUv =
                            cellPlacement.Radius / WorkResolution;
                        float cellDistanceUv = ToroidalDistanceUv(
                            cellCenterX,
                            cellCenterY,
                            cellPlacement.CenterX / WorkResolution,
                            cellPlacement.CenterY / WorkResolution);
                        if (cellDistanceUv >
                            cellRadiusUv * AnchorProofMaximumDistance +
                            cellGuard)
                        {
                            continue;
                        }

                        for (int subY = 0; subY < 2; subY++)
                        {
                            for (int subX = 0; subX < 2; subX++)
                            {
                                float pointX = (
                                    x + (subX == 0 ? 0.25f : 0.75f)) /
                                    resolution;
                                float pointY = (
                                    y + (subY == 0 ? 0.25f : 0.75f)) /
                                    resolution;
                                int expectedOwner = FindNearestPlacementUv(
                                    placements,
                                    pointX,
                                    pointY);
                                if (expectedOwner < 0)
                                {
                                    invalidSamples++;
                                    continue;
                                }

                                PlacementEvidence placement =
                                    placements[expectedOwner];
                                float expectedCenterX =
                                    placement.CenterX / WorkResolution;
                                float expectedCenterY =
                                    placement.CenterY / WorkResolution;
                                float expectedRadiusUv =
                                    placement.Radius / WorkResolution;
                                float sampleDistanceUv = ToroidalDistanceUv(
                                    pointX,
                                    pointY,
                                    expectedCenterX,
                                    expectedCenterY);
                                if (sampleDistanceUv >
                                    expectedRadiusUv *
                                    AnchorProofMaximumDistance)
                                {
                                    continue;
                                }

                                float offsetX = SampleToroidalBilinearField(
                                    centerOffsetX,
                                    resolution,
                                    pointX,
                                    pointY);
                                float offsetY = SampleToroidalBilinearField(
                                    centerOffsetY,
                                    resolution,
                                    pointX,
                                    pointY);
                                if (float.IsNaN(offsetX) ||
                                    float.IsInfinity(offsetX) ||
                                    float.IsNaN(offsetY) ||
                                    float.IsInfinity(offsetY))
                                {
                                    invalidSamples++;
                                    continue;
                                }

                                float centerX = Repeat01(
                                    pointX - offsetX * supportRadiusUv);
                                float centerY = Repeat01(
                                    pointY - offsetY * supportRadiusUv);
                                int reconstructedOwner = FindNearestPlacementUv(
                                    placements,
                                    centerX,
                                    centerY);
                                if (reconstructedOwner != expectedOwner)
                                {
                                    invalidSamples++;
                                    continue;
                                }

                                int owner = expectedOwner;
                                float centerError = ToroidalDistanceUv(
                                    centerX,
                                    centerY,
                                    expectedCenterX,
                                    expectedCenterY);
                                maximumCenterError = Mathf.Max(
                                    maximumCenterError,
                                    centerError);

                                float errorX = ToroidalDeltaUv(
                                    centerX,
                                    expectedCenterX);
                                float errorY = ToroidalDeltaUv(
                                    centerY,
                                    expectedCenterY);
                                float angle =
                                    (owner * 0.61803398875f + 0.173f) *
                                    Mathf.PI * 2f;
                                float edgeProxy =
                                    errorX * Mathf.Cos(angle) +
                                    errorY * Mathf.Sin(angle) -
                                    supportRadiusUv;
                                rockMinimum[owner] = Mathf.Min(
                                    rockMinimum[owner],
                                    edgeProxy);
                                rockMaximum[owner] = Mathf.Max(
                                    rockMaximum[owner],
                                    edgeProxy);
                                acceptedSamples++;
                            }
                        }
                    }
                }

                float maximumSpread = 0f;
                int inconsistentRocks = 0;
                for (int index = 0; index < placements.Count; index++)
                {
                    if (float.IsPositiveInfinity(rockMinimum[index]))
                    {
                        continue;
                    }

                    float spread = rockMaximum[index] - rockMinimum[index];
                    maximumSpread = Mathf.Max(maximumSpread, spread);
                    if (spread > 0.01f)
                    {
                        inconsistentRocks++;
                    }
                }

                maximumCenterErrorAcrossMips = Mathf.Max(
                    maximumCenterErrorAcrossMips,
                    maximumCenterError);
                maximumSpreadAcrossMips = Mathf.Max(
                    maximumSpreadAcrossMips,
                    maximumSpread);
                invalidSamplesAcrossMips += invalidSamples;
                inconsistentRocksAcrossMips = Mathf.Max(
                    inconsistentRocksAcrossMips,
                    inconsistentRocks);

                if (acceptedSamples > 0 &&
                    invalidSamples == 0 &&
                    maximumCenterError <= 0.01f &&
                    maximumSpread <= 0.01f &&
                    inconsistentRocks == 0)
                {
                    lastAcceptedMip = mip;
                }
                else
                {
                    break;
                }

                if (mip < AnchorProofMaximumMip)
                {
                    centerOffsetX = DownsampleLinearField(
                        centerOffsetX,
                        resolution);
                    centerOffsetY = DownsampleLinearField(
                        centerOffsetY,
                        resolution);
                    resolution /= 2;
                }
            }

            result.FeatureAnchorInvalidSampleCount =
                invalidSamplesAcrossMips;
            result.FeatureAnchorMaximumCenterErrorUv =
                maximumCenterErrorAcrossMips;
            result.FeatureAnchorMaximumRetentionSpread =
                maximumSpreadAcrossMips;
            result.FeatureAnchorInconsistentRockCount =
                inconsistentRocksAcrossMips;
            result.FeatureAnchorLastAcceptedMip = lastAcceptedMip;
        }

        private static void MeasureWholeFeatureBoundaryComposition(
            CandidateResult result,
            FinalBuffers final,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<float> centerOffsetX,
            IReadOnlyList<float> centerOffsetY)
        {
            List<int>[] featurePixels = new List<int>[placements.Count];
            for (int index = 0; index < featurePixels.Length; index++)
            {
                featurePixels[index] = new List<int>();
            }

            for (int index = 0; index < result.PaletteForm.Length; index++)
            {
                int owner = final.Owner[index];
                if (owner < 0 || owner >= placements.Count ||
                    !HasEncodedFeatureResponse(result, index))
                {
                    continue;
                }

                featurePixels[owner].Add(index);
            }

            float supportRadius = Mathf.Max(
                0.0001f,
                result.FeatureMaximumSupportRadiusUv);
            float transitionDistance =
                supportRadius * FeatureBoundaryTransitionRadiusFactor;
            float safetyMargin =
                supportRadius * FeatureBoundarySafetyRadiusFactor;
            float fadeDistance =
                supportRadius * FeatureBoundaryFadeRadiusFactor;
            float requiredClearance =
                transitionDistance + safetyMargin;
            float guardDistance = Mathf.Max(
                2f / FinalResolution,
                result.FeatureAnchorMaximumCenterErrorUv * 3f);

            float hardMaximumSpread = 0f;
            float fadeMaximumSpread = 0f;
            float removedMaximumResidual = 0f;
            int hardPartialRockCount = 0;
            int fadeInconsistentRockCount = 0;

            for (int orientation = 0;
                 orientation < FeatureBoundaryProofOrientationCount;
                 orientation++)
            {
                float angle =
                    orientation /
                    (float)FeatureBoundaryProofOrientationCount *
                    Mathf.PI * 2f;
                Vector2 normal = new Vector2(
                    Mathf.Cos(angle),
                    Mathf.Sin(angle));

                for (int owner = 0; owner < placements.Count; owner++)
                {
                    IReadOnlyList<int> pixels = featurePixels[owner];
                    if (pixels.Count == 0)
                    {
                        continue;
                    }

                    WeightRange removed = MeasureWholeFeatureWeightRange(
                        pixels,
                        placements[owner],
                        centerOffsetX,
                        centerOffsetY,
                        normal,
                        requiredClearance - guardDistance,
                        supportRadius,
                        requiredClearance,
                        0f);
                    WeightRange retained = MeasureWholeFeatureWeightRange(
                        pixels,
                        placements[owner],
                        centerOffsetX,
                        centerOffsetY,
                        normal,
                        requiredClearance + guardDistance,
                        supportRadius,
                        requiredClearance,
                        0f);
                    WeightRange faded = MeasureWholeFeatureWeightRange(
                        pixels,
                        placements[owner],
                        centerOffsetX,
                        centerOffsetY,
                        normal,
                        requiredClearance + fadeDistance * 0.5f,
                        supportRadius,
                        requiredClearance,
                        fadeDistance);

                    hardMaximumSpread = Mathf.Max(
                        hardMaximumSpread,
                        Mathf.Max(removed.Spread, retained.Spread));
                    fadeMaximumSpread = Mathf.Max(
                        fadeMaximumSpread,
                        faded.Spread);
                    removedMaximumResidual = Mathf.Max(
                        removedMaximumResidual,
                        removed.Maximum);
                    if (removed.Maximum > FeatureBoundaryWeightTolerance ||
                        retained.Minimum <
                            1f - FeatureBoundaryWeightTolerance ||
                        removed.Spread > FeatureBoundaryWeightTolerance ||
                        retained.Spread > FeatureBoundaryWeightTolerance)
                    {
                        hardPartialRockCount++;
                    }

                    if (faded.Spread > FeatureBoundaryWeightTolerance)
                    {
                        fadeInconsistentRockCount++;
                    }
                }
            }

            result.FeatureBoundaryHardMaximumWeightSpread =
                hardMaximumSpread;
            result.FeatureBoundaryFadeMaximumWeightSpread =
                fadeMaximumSpread;
            result.FeatureBoundaryHardPartialRockCount =
                hardPartialRockCount;
            result.FeatureBoundaryRemovedMaximumResidual =
                removedMaximumResidual;
            result.FeatureBoundaryFadeInconsistentRockCount =
                fadeInconsistentRockCount;
            result.FeatureBoundarySweepContactSheet =
                BuildFeatureBoundarySweepContactSheet(
                    result,
                    final,
                    placements,
                    centerOffsetX,
                    centerOffsetY,
                    requiredClearance,
                    fadeDistance,
                    guardDistance,
                    supportRadius);
        }

        private struct WeightRange
        {
            internal float Minimum;
            internal float Maximum;
            internal float Spread => Maximum - Minimum;
        }

        private static WeightRange MeasureWholeFeatureWeightRange(
            IReadOnlyList<int> pixels,
            PlacementEvidence placement,
            IReadOnlyList<float> centerOffsetX,
            IReadOnlyList<float> centerOffsetY,
            Vector2 boundaryNormal,
            float intendedFeatureEdgeDistance,
            float supportRadius,
            float requiredClearance,
            float fadeDistance)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int index = 0; index < pixels.Count; index++)
            {
                float weight = ResolveProofWholeFeatureWeight(
                    pixels[index],
                    placement,
                    centerOffsetX,
                    centerOffsetY,
                    boundaryNormal,
                    intendedFeatureEdgeDistance,
                    supportRadius,
                    requiredClearance,
                    fadeDistance);
                minimum = Mathf.Min(minimum, weight);
                maximum = Mathf.Max(maximum, weight);
            }

            if (float.IsPositiveInfinity(minimum))
            {
                minimum = 0f;
                maximum = 0f;
            }

            return new WeightRange
            {
                Minimum = minimum,
                Maximum = maximum
            };
        }

        private static float ResolveProofWholeFeatureWeight(
            int pixelIndex,
            PlacementEvidence placement,
            IReadOnlyList<float> centerOffsetX,
            IReadOnlyList<float> centerOffsetY,
            Vector2 boundaryNormal,
            float intendedFeatureEdgeDistance,
            float supportRadius,
            float requiredClearance,
            float fadeDistance)
        {
            int x = pixelIndex % FinalResolution;
            int y = pixelIndex / FinalResolution;
            float pointX = (x + 0.5f) / FinalResolution;
            float pointY = (y + 0.5f) / FinalResolution;
            float centerX = placement.CenterX / WorkResolution;
            float centerY = placement.CenterY / WorkResolution;
            Vector2 actualOffset = new Vector2(
                ToroidalDeltaUv(pointX, centerX),
                ToroidalDeltaUv(pointY, centerY));
            Vector2 decodedOffset = new Vector2(
                centerOffsetX[pixelIndex],
                centerOffsetY[pixelIndex]) * supportRadius;
            float reconstructedFeatureEdgeDistance =
                intendedFeatureEdgeDistance +
                Vector2.Dot(
                    actualOffset - decodedOffset,
                    boundaryNormal);
            if (fadeDistance <= 0.0001f)
            {
                return reconstructedFeatureEdgeDistance >= requiredClearance
                    ? 1f
                    : 0f;
            }

            return SmoothStep(
                requiredClearance,
                requiredClearance + fadeDistance,
                reconstructedFeatureEdgeDistance);
        }

        private static bool HasEncodedFeatureResponse(
            CandidateResult result,
            int index)
        {
            Color32 palettePayload = result.PaletteForm[index];
            float form = DecodePaletteForm(palettePayload);
            float substrateForm = Mathf.Clamp01(
                Mathf.GammaToLinearSpace(
                    palettePayload.g / 255f));
            Color32 packed = result.RuntimePackedDetail[index];
            float slopeX = packed.r / 255f * 2f - 1f;
            float slopeY = packed.g / 255f * 2f - 1f;
            float slopeMagnitude = Mathf.Sqrt(
                slopeX * slopeX + slopeY * slopeY);
            float cavity = packed.b / 255f;
            float roughness = packed.a / 255f;
            return
                slopeMagnitude >= FeatureSlopeEvidenceThreshold ||
                cavity >= FeatureCavityEvidenceThreshold ||
                Mathf.Abs(form - substrateForm) >=
                    FeatureFormEvidenceThreshold ||
                Mathf.Abs(
                    roughness - result.FeatureSubstrateRoughness) >=
                    FeatureRoughnessEvidenceThreshold;
        }

        private static Color32[] BuildFeatureBoundarySweepContactSheet(
            CandidateResult result,
            FinalBuffers final,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<float> centerOffsetX,
            IReadOnlyList<float> centerOffsetY,
            float requiredClearance,
            float fadeDistance,
            float guardDistance,
            float supportRadius)
        {
            Color32[] output = new Color32[
                FeatureBoundarySweepWidth * FeatureBoundarySweepHeight];
            Color32[] substratePreview = BuildSubstratePalettePreview(
                result.PaletteForm,
                NeutralPalette);
            float[] edgeDistances =
            {
                requiredClearance - guardDistance,
                requiredClearance + guardDistance,
                requiredClearance + fadeDistance * 0.25f,
                requiredClearance + fadeDistance * 0.75f
            };
            float[] fades =
            {
                0f,
                0f,
                fadeDistance,
                fadeDistance
            };
            Vector2 normal = new Vector2(0.8320503f, 0.5547002f);
            int panelSize = FeatureBoundarySweepHeight;

            for (int panel = 0; panel < edgeDistances.Length; panel++)
            {
                Color32[] panelPixels = new Color32[
                    FinalResolution * FinalResolution];
                for (int index = 0; index < panelPixels.Length; index++)
                {
                    int owner = final.Owner[index];
                    float weight = 0f;
                    if (owner >= 0 && owner < placements.Count &&
                        HasEncodedFeatureResponse(result, index))
                    {
                        weight = ResolveProofWholeFeatureWeight(
                            index,
                            placements[owner],
                            centerOffsetX,
                            centerOffsetY,
                            normal,
                            edgeDistances[panel],
                            supportRadius,
                            requiredClearance,
                            fades[panel]);
                    }

                    panelPixels[index] = (Color32)Color.Lerp(
                        substratePreview[index],
                        result.PalettePreviewNeutral[index],
                        weight);
                }

                BlitScaled(
                    panelPixels,
                    FinalResolution,
                    output,
                    FeatureBoundarySweepWidth,
                    panel * panelSize,
                    0,
                    panelSize);
            }

            return output;
        }

        private static float SampleToroidalBilinearField(
            IReadOnlyList<float> field,
            int resolution,
            float u,
            float v)
        {
            float sampleX = Repeat01(u) * resolution - 0.5f;
            float sampleY = Repeat01(v) * resolution - 0.5f;
            int x0 = Mathf.FloorToInt(sampleX);
            int y0 = Mathf.FloorToInt(sampleY);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tX = sampleX - Mathf.Floor(sampleX);
            float tY = sampleY - Mathf.Floor(sampleY);
            x0 = Wrap(x0, resolution);
            x1 = Wrap(x1, resolution);
            y0 = Wrap(y0, resolution);
            y1 = Wrap(y1, resolution);
            float row0 = Mathf.LerpUnclamped(
                field[y0 * resolution + x0],
                field[y0 * resolution + x1],
                tX);
            float row1 = Mathf.LerpUnclamped(
                field[y1 * resolution + x0],
                field[y1 * resolution + x1],
                tX);
            return Mathf.LerpUnclamped(row0, row1, tY);
        }

        private static float[] DownsampleLinearField(
            IReadOnlyList<float> source,
            int sourceResolution)
        {
            int destinationResolution = sourceResolution / 2;
            float[] destination = new float[
                destinationResolution * destinationResolution];
            for (int y = 0; y < destinationResolution; y++)
            {
                for (int x = 0; x < destinationResolution; x++)
                {
                    float sum = 0f;
                    for (int offsetY = 0; offsetY < 2; offsetY++)
                    {
                        for (int offsetX = 0; offsetX < 2; offsetX++)
                        {
                            sum += source[
                                (y * 2 + offsetY) * sourceResolution +
                                x * 2 + offsetX];
                        }
                    }

                    destination[y * destinationResolution + x] =
                        sum * 0.25f;
                }
            }

            return destination;
        }

        private static int FindNearestPlacementUv(
            IReadOnlyList<PlacementEvidence> placements,
            float x,
            float y)
        {
            if (placements == null || placements.Count == 0)
            {
                return -1;
            }

            float bestDistance = float.PositiveInfinity;
            int bestIndex = -1;
            for (int index = 0; index < placements.Count; index++)
            {
                float distance = ToroidalDistanceUv(
                    x,
                    y,
                    placements[index].CenterX / WorkResolution,
                    placements[index].CenterY / WorkResolution);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        private static float ToroidalDistanceUv(
            float xA,
            float yA,
            float xB,
            float yB)
        {
            float deltaX = Mathf.Abs(ToroidalDeltaUv(xA, xB));
            float deltaY = Mathf.Abs(ToroidalDeltaUv(yA, yB));
            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private static float ToroidalDeltaUv(float value, float center)
        {
            float delta = value - center;
            if (delta > 0.5f)
            {
                delta -= 1f;
            }
            else if (delta < -0.5f)
            {
                delta += 1f;
            }

            return delta;
        }

        private static float Repeat01(float value)
        {
            return value - Mathf.Floor(value);
        }

        private static float MeasureMaximumAdjacentPaletteFormDifference(
            IReadOnlyList<Color32> paletteForm)
        {
            float maximum = 0f;
            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    int index = y * FinalResolution + x;
                    int right = y * FinalResolution +
                        Wrap(x + 1, FinalResolution);
                    int up = Wrap(y + 1, FinalResolution) *
                        FinalResolution + x;
                    float value = DecodePaletteForm(paletteForm[index]);
                    maximum = Mathf.Max(
                        maximum,
                        Mathf.Abs(
                            value - DecodePaletteForm(paletteForm[right])));
                    maximum = Mathf.Max(
                        maximum,
                        Mathf.Abs(
                            value - DecodePaletteForm(paletteForm[up])));
                }
            }

            return maximum;
        }

        private static float MeasureMeanColorDifference(
            IReadOnlyList<Color32> a,
            IReadOnlyList<Color32> b)
        {
            double difference = 0.0;
            int count = Mathf.Min(a.Count, b.Count);
            for (int index = 0; index < count; index++)
            {
                difference += ColorDifference(a[index], b[index]);
            }

            return count > 0 ? (float)(difference / count) : 0f;
        }

        private static string CalculatePairedPayloadFingerprint(
            CandidateResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(result.Definition.StableId);
                writer.Write(result.FeatureSubstrateRoughness);
                writer.Write(result.FeatureMaximumSupportRadiusUv);
                WritePixels(writer, result.PaletteForm);
                WritePixels(writer, result.RuntimePackedDetail);
                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static string CalculatePixelFingerprint(
            string label,
            Color32[] pixels)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(label ?? string.Empty);
                WritePixels(writer, pixels);
                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static Color32[] BuildPlacementDebug(
            Color32[] moderate,
            IReadOnlyList<PlacementEvidence> placements)
        {
            Color32[] output = (Color32[])moderate.Clone();
            Color32 centerColor = new Color32(255, 226, 96, 255);
            for (int index = 0; index < placements.Count; index++)
            {
                int centerX = Mathf.RoundToInt(
                    placements[index].CenterX / WorkScale);
                int centerY = Mathf.RoundToInt(
                    placements[index].CenterY / WorkScale);
                for (int offset = -3; offset <= 3; offset++)
                {
                    SetFinalPixel(output, centerX + offset, centerY, centerColor);
                    SetFinalPixel(output, centerX, centerY + offset, centerColor);
                }
            }

            return output;
        }

        private static void SetFinalPixel(
            Color32[] pixels,
            int x,
            int y,
            Color32 color)
        {
            int wrappedX = Wrap(x, FinalResolution);
            int wrappedY = Wrap(y, FinalResolution);
            pixels[wrappedY * FinalResolution + wrappedX] = color;
        }

        internal static Color32[] BuildThreeByThreeEvidence(Color32[] tile)
        {
            int resolution = FinalResolution * 3;
            Color32[] output = new Color32[resolution * resolution];
            for (int tileY = 0; tileY < 3; tileY++)
            {
                for (int tileX = 0; tileX < 3; tileX++)
                {
                    for (int y = 0; y < FinalResolution; y++)
                    {
                        int source = y * FinalResolution;
                        int destination =
                            (tileY * FinalResolution + y) * resolution +
                            tileX * FinalResolution;
                        Array.Copy(
                            tile,
                            source,
                            output,
                            destination,
                            FinalResolution);
                    }
                }
            }

            return output;
        }

        private static Color32[] BuildMipContactSheet(Color32[] source)
        {
            Color32[] output = new Color32[
                FinalResolution * FinalResolution];
            Color32 background = new Color32(22, 22, 22, 255);
            for (int index = 0; index < output.Length; index++)
            {
                output[index] = background;
            }

            int[] sizes = { 512, 512, 512, 512 };
            int[] sourceLevels = { 1024, 512, 256, 128 };
            int[] originsX = { 0, 512, 0, 512 };
            int[] originsY = { 512, 512, 0, 0 };
            Color32[] current = DownsampleColor(source, FinalResolution);
            Color32[][] levels = new Color32[4][];
            levels[0] = source;
            levels[1] = current;
            levels[2] = DownsampleColor(current, 512);
            levels[3] = DownsampleColor(levels[2], 256);
            for (int panel = 0; panel < 4; panel++)
            {
                BlitScaled(
                    levels[panel],
                    sourceLevels[panel],
                    output,
                    FinalResolution,
                    originsX[panel],
                    originsY[panel],
                    sizes[panel]);
            }

            return output;
        }

        private static Color32[] DownsampleColor(
            Color32[] source,
            int sourceResolution)
        {
            int targetResolution = sourceResolution / 2;
            Color32[] output = new Color32[
                targetResolution * targetResolution];
            for (int y = 0; y < targetResolution; y++)
            {
                for (int x = 0; x < targetResolution; x++)
                {
                    int sourceX = x * 2;
                    int sourceY = y * 2;
                    Color32 a = source[sourceY * sourceResolution + sourceX];
                    Color32 b = source[sourceY * sourceResolution + sourceX + 1];
                    Color32 c = source[(sourceY + 1) * sourceResolution + sourceX];
                    Color32 d = source[(sourceY + 1) * sourceResolution + sourceX + 1];
                    output[y * targetResolution + x] = new Color32(
                        (byte)((a.r + b.r + c.r + d.r) / 4),
                        (byte)((a.g + b.g + c.g + d.g) / 4),
                        (byte)((a.b + b.b + c.b + d.b) / 4),
                        255);
                }
            }

            return output;
        }

        private static void BlitScaled(
            Color32[] source,
            int sourceResolution,
            Color32[] destination,
            int destinationResolution,
            int originX,
            int originY,
            int size)
        {
            for (int y = 0; y < size; y++)
            {
                int sourceY = Mathf.Clamp(
                    Mathf.FloorToInt(y / (float)size * sourceResolution),
                    0,
                    sourceResolution - 1);
                for (int x = 0; x < size; x++)
                {
                    int sourceX = Mathf.Clamp(
                        Mathf.FloorToInt(x / (float)size * sourceResolution),
                        0,
                        sourceResolution - 1);
                    destination[(originY + y) * destinationResolution +
                        originX + x] =
                        source[sourceY * sourceResolution + sourceX];
                }
            }
        }

        private static void BuildSourceUsage(
            CandidateResult result,
            IReadOnlyList<SourceCache> sources,
            IReadOnlyList<int> counts)
        {
            int maximum = 0;
            int unique = 0;
            for (int index = 0; index < sources.Count; index++)
            {
                if (counts[index] > 0)
                {
                    unique++;
                }

                maximum = Mathf.Max(maximum, counts[index]);
                result.SourceUsage.Add(new SourceUsage
                {
                    StableId = sources[index].Definition.StableId,
                    Count = counts[index],
                    UsedFallbackMesh = sources[index].UsedFallbackMesh
                });
            }

            result.UniqueSourceCount = unique;
            result.MaximumObservedSourceShare = result.Placements.Count > 0
                ? maximum / (float)result.Placements.Count
                : 0f;
        }

        private static int CountCoveredPixels(IReadOnlyList<float> mask)
        {
            int count = 0;
            for (int index = 0; index < mask.Count; index++)
            {
                if (mask[index] > 0.5f)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountOccupiedQuietBlocks(float[] mask)
        {
            int occupied = 0;
            for (int blockY = 0; blockY < QuietBlockAxis; blockY++)
            {
                for (int blockX = 0; blockX < QuietBlockAxis; blockX++)
                {
                    bool blockOccupied = false;
                    int startX = blockX * QuietBlockSizeFinal;
                    int startY = blockY * QuietBlockSizeFinal;
                    for (int y = startY;
                         y < startY + QuietBlockSizeFinal && !blockOccupied;
                         y++)
                    {
                        for (int x = startX;
                             x < startX + QuietBlockSizeFinal;
                             x++)
                        {
                            if (mask[y * FinalResolution + x] > 0.25f)
                            {
                                blockOccupied = true;
                                break;
                            }
                        }
                    }

                    occupied += blockOccupied ? 1 : 0;
                }
            }

            return occupied;
        }

        private static SeamEvidence MeasureSeams(
            FinalBuffers final,
            Color32[] preview,
            Color32[] paletteForm,
            Color32[] packedDetail,
            Color32[] palettePreview)
        {
            SeamEvidence evidence = new SeamEvidence();
            double mask = 0.0;
            double height = 0.0;
            double normal = 0.0;
            double variation = 0.0;
            double root = 0.0;
            double wear = 0.0;
            double color = 0.0;
            double paletteFormDifference = 0.0;
            double packedDetailDifference = 0.0;
            double palettePreviewDifference = 0.0;
            int sampleCount = 0;
            for (int index = 0; index < FinalResolution; index++)
            {
                AccumulateSeam(
                    final,
                    preview,
                    index * FinalResolution + FinalResolution - 1,
                    index * FinalResolution,
                    ref mask,
                    ref height,
                    ref normal,
                    ref variation,
                    ref root,
                    ref wear,
                    ref color);
                AccumulateColorSeam(
                    paletteForm,
                    packedDetail,
                    palettePreview,
                    index * FinalResolution + FinalResolution - 1,
                    index * FinalResolution,
                    ref paletteFormDifference,
                    ref packedDetailDifference,
                    ref palettePreviewDifference);
                AccumulateSeam(
                    final,
                    preview,
                    (FinalResolution - 1) * FinalResolution + index,
                    index,
                    ref mask,
                    ref height,
                    ref normal,
                    ref variation,
                    ref root,
                    ref wear,
                    ref color);
                AccumulateColorSeam(
                    paletteForm,
                    packedDetail,
                    palettePreview,
                    (FinalResolution - 1) * FinalResolution + index,
                    index,
                    ref paletteFormDifference,
                    ref packedDetailDifference,
                    ref palettePreviewDifference);
                sampleCount += 2;
            }

            float inverse = 1f / Mathf.Max(1, sampleCount);
            evidence.MaskMean = (float)mask * inverse;
            evidence.HeightMean = (float)height * inverse;
            evidence.NormalMean = (float)normal * inverse;
            evidence.VariationMean = (float)variation * inverse;
            evidence.RootMean = (float)root * inverse;
            evidence.WearMean = (float)wear * inverse;
            evidence.PreviewMean = (float)color * inverse;
            evidence.PaletteFormMean =
                (float)paletteFormDifference * inverse;
            evidence.PackedDetailMean =
                (float)packedDetailDifference * inverse;
            evidence.PalettePreviewMean =
                (float)palettePreviewDifference * inverse;
            return evidence;
        }

        private static void AccumulateSeam(
            FinalBuffers final,
            Color32[] preview,
            int a,
            int b,
            ref double mask,
            ref double height,
            ref double normal,
            ref double variation,
            ref double root,
            ref double wear,
            ref double color)
        {
            mask += Mathf.Abs(final.Mask[a] - final.Mask[b]);
            height += Mathf.Abs(final.Height[a] - final.Height[b]);
            normal += 1f - Mathf.Clamp01(
                Vector3.Dot(final.Normals[a], final.Normals[b]));
            variation += Mathf.Abs(
                final.Variation[a] - final.Variation[b]);
            root += Mathf.Abs(
                final.RootDarkening[a] - final.RootDarkening[b]);
            wear += Mathf.Abs(final.EdgeWear[a] - final.EdgeWear[b]);
            Color32 colorA = preview[a];
            Color32 colorB = preview[b];
            color += (
                Mathf.Abs(colorA.r - colorB.r) +
                Mathf.Abs(colorA.g - colorB.g) +
                Mathf.Abs(colorA.b - colorB.b)) /
                (255f * 3f);
        }

        private static void AccumulateColorSeam(
            IReadOnlyList<Color32> paletteForm,
            IReadOnlyList<Color32> packedDetail,
            IReadOnlyList<Color32> palettePreview,
            int a,
            int b,
            ref double form,
            ref double packed,
            ref double preview)
        {
            form += PackedColorDifference(paletteForm[a], paletteForm[b]);
            packed += PackedColorDifference(
                packedDetail[a],
                packedDetail[b]);
            preview += ColorDifference(palettePreview[a], palettePreview[b]);
        }

        private static float PackedColorDifference(
            Color32 a,
            Color32 b)
        {
            return (
                Mathf.Abs(a.r - b.r) +
                Mathf.Abs(a.g - b.g) +
                Mathf.Abs(a.b - b.b) +
                Mathf.Abs(a.a - b.a)) /
                (255f * 4f);
        }

        private static void ReleaseCandidateEvidence(
            CandidateResult candidate)
        {
            candidate.Moderate = null;
            candidate.PlacementDebug = null;
            candidate.StableIdDebug = null;
            candidate.Mask = null;
            candidate.Height = null;
            candidate.Normals = null;
            candidate.Variation = null;
            candidate.RootDarkening = null;
            candidate.EdgeWear = null;
            candidate.MipContactSheet = null;
            candidate.FeatureBoundarySweepContactSheet = null;
            candidate.PaletteForm = null;
            candidate.RuntimePackedDetail = null;
            candidate.PalettePreviewNeutral = null;
            candidate.PalettePreviewHigherContrast = null;
            candidate.PalettePreviewAlternate = null;
            candidate.PaletteComparison = null;
        }

        private static void ReleaseSubstrateEvidence(
            SubstrateResult substrate)
        {
            if (substrate == null)
            {
                return;
            }

            substrate.Color = null;
            substrate.Variation = null;
        }

        private static string CalculateCandidateFingerprint(
            CandidateResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(result.Definition.StableId);
                writer.Write(result.SubstrateFingerprint ?? string.Empty);
                writer.Write(result.Placements.Count);
                for (int index = 0; index < result.Placements.Count; index++)
                {
                    PlacementEvidence placement = result.Placements[index];
                    writer.Write(placement.StableId);
                    writer.Write(placement.CenterX);
                    writer.Write(placement.CenterY);
                    writer.Write(placement.RotationDegrees);
                    writer.Write(placement.UniformScale);
                    writer.Write(placement.BurialFraction);
                }

                writer.Write(result.MinimumNormalizedNeighbourSeparation);
                writer.Write(result.MaximumNearNeighbourCount);
                writer.Write(result.MaximumBroadCenterCount);
                writer.Write(result.FeatureSubstrateRoughness);
                writer.Write(result.FeatureMaximumSupportRadiusUv);
                writer.Write(result.FeatureAnchorMaximumCenterErrorUv);
                writer.Write(result.FeatureAnchorMaximumRetentionSpread);
                writer.Write(result.FeatureBoundaryHardMaximumWeightSpread);
                writer.Write(result.FeatureBoundaryFadeMaximumWeightSpread);
                writer.Write(result.FeatureBoundaryHardPartialRockCount);
                writer.Write(result.FeatureBoundaryRemovedMaximumResidual);
                writer.Write(result.FeatureBoundaryFadeInconsistentRockCount);
                WritePixels(writer, result.Moderate);
                WritePixels(writer, result.PlacementDebug);
                WritePixels(writer, result.StableIdDebug);
                WritePixels(writer, result.Mask);
                WritePixels(writer, result.Height);
                WritePixels(writer, result.Normals);
                WritePixels(writer, result.Variation);
                WritePixels(writer, result.RootDarkening);
                WritePixels(writer, result.EdgeWear);
                WritePixels(writer, result.MipContactSheet);
                WritePixels(writer, result.FeatureBoundarySweepContactSheet);
                WritePixels(writer, result.PaletteForm);
                WritePixels(writer, result.RuntimePackedDetail);
                WritePixels(writer, result.PalettePreviewNeutral);
                WritePixels(writer, result.PalettePreviewHigherContrast);
                WritePixels(writer, result.PalettePreviewAlternate);
                WritePixels(writer, result.PaletteComparison);
                writer.Write(result.PalettePayloadFingerprint ?? string.Empty);
                writer.Write(
                    result.PalettePreviewNeutralFingerprint ?? string.Empty);
                writer.Write(
                    result.PalettePreviewHigherContrastFingerprint ??
                    string.Empty);
                writer.Write(
                    result.PalettePreviewAlternateFingerprint ??
                    string.Empty);
                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static string CalculateSuiteFingerprint(SuiteResult suite)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(
                    suite.Substrate != null
                        ? suite.Substrate.Fingerprint ?? string.Empty
                        : string.Empty);
                writer.Write(suite.Candidates.Count);
                for (int index = 0; index < suite.Candidates.Count; index++)
                {
                    writer.Write(
                        suite.Candidates[index].Fingerprint ?? string.Empty);
                }

                writer.Flush();
                return CalculateSha256(stream.ToArray());
            }
        }

        private static void WritePixels(
            BinaryWriter writer,
            Color32[] pixels)
        {
            writer.Write(pixels != null ? pixels.Length : 0);
            if (pixels == null)
            {
                return;
            }

            for (int index = 0; index < pixels.Length; index++)
            {
                writer.Write(pixels[index].r);
                writer.Write(pixels[index].g);
                writer.Write(pixels[index].b);
                writer.Write(pixels[index].a);
            }
        }

        private static string CalculateSha256(byte[] data)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(data);
                StringBuilder builder = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    builder.Append(hash[index].ToString(
                        "x2",
                        CultureInfo.InvariantCulture));
                }

                return builder.ToString();
            }
        }

        private static Color32 ResolveStableIdColor(int sourceIndex)
        {
            float hue = Mathf.Repeat(sourceIndex * 0.61803398875f, 1f);
            Color color = Color.HSVToRGB(hue, 0.68f, 0.92f);
            return (Color32)color;
        }

        private static Color ResolveColor(MeshData mesh, int index)
        {
            return mesh.Colors.Count == mesh.Vertices.Count
                ? mesh.Colors[index]
                : new Color(0.5f, 0.5f, 0f, 0f);
        }

        private static Vector4 ResolveUv2(MeshData mesh, int index)
        {
            return mesh.UV2.Count == mesh.Vertices.Count
                ? mesh.UV2[index]
                : Vector4.zero;
        }

        private static float Edge(
            Vector2 pointA,
            Vector2 pointB,
            Vector2 point)
        {
            return (point.x - pointA.x) *
                    (pointB.y - pointA.y) -
                (point.y - pointA.y) *
                    (pointB.x - pointA.x);
        }

        private static float ToroidalDistance(
            Vector2 a,
            Vector2 b)
        {
            float deltaX = Mathf.Abs(a.x - b.x);
            float deltaY = Mathf.Abs(a.y - b.y);
            deltaX = Mathf.Min(deltaX, WorkResolution - deltaX);
            deltaY = Mathf.Min(deltaY, WorkResolution - deltaY);
            return Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
        }

        private static float ToroidalDelta(
            float value,
            float center,
            int period)
        {
            float delta = value - center;
            if (delta > period * 0.5f)
            {
                delta -= period;
            }
            else if (delta < -period * 0.5f)
            {
                delta += period;
            }

            return delta;
        }

        private static int Wrap(int value, int period)
        {
            int wrapped = value % period;
            return wrapped < 0 ? wrapped + period : wrapped;
        }

        private static float PeriodicValueNoise(
            int x,
            int y,
            int period,
            int seed)
        {
            float coordinateX = x * period / (float)FinalResolution;
            float coordinateY = y * period / (float)FinalResolution;
            int x0 = Mathf.FloorToInt(coordinateX);
            int y0 = Mathf.FloorToInt(coordinateY);
            float fractionX = coordinateX - x0;
            float fractionY = coordinateY - y0;
            int wrappedX0 = Wrap(x0, period);
            int wrappedY0 = Wrap(y0, period);
            int wrappedX1 = Wrap(x0 + 1, period);
            int wrappedY1 = Wrap(y0 + 1, period);
            float smoothX = fractionX * fractionX *
                (3f - 2f * fractionX);
            float smoothY = fractionY * fractionY *
                (3f - 2f * fractionY);
            float value00 = HashLattice(
                wrappedX0,
                wrappedY0,
                seed);
            float value10 = HashLattice(
                wrappedX1,
                wrappedY0,
                seed);
            float value01 = HashLattice(
                wrappedX0,
                wrappedY1,
                seed);
            float value11 = HashLattice(
                wrappedX1,
                wrappedY1,
                seed);
            return Mathf.Lerp(
                Mathf.Lerp(value00, value10, smoothX),
                Mathf.Lerp(value01, value11, smoothX),
                smoothY);
        }

        private static float HashLattice(int x, int y, int seed)
        {
            unchecked
            {
                return Hash01(
                    seed ^
                    x * 73856093 ^
                    y * 19349663);
            }
        }

        private static float Hash01(int value)
        {
            unchecked
            {
                uint x = (uint)value;
                x ^= x >> 16;
                x *= 0x7FEB352Du;
                x ^= x >> 15;
                x *= 0x846CA68Bu;
                x ^= x >> 16;
                return (x & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.000001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            float t = Mathf.Clamp01(
                (value - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        }

        private static Color32 Grayscale(byte value)
        {
            return new Color32(value, value, value, 255);
        }

        private static Color32 EncodeWorldNormal(Vector3 normal)
        {
            return new Color32(
                ToByte(normal.x * 0.5f + 0.5f),
                ToByte(normal.y * 0.5f + 0.5f),
                ToByte(normal.z * 0.5f + 0.5f),
                255);
        }
    }
}
