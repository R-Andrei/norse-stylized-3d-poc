using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface.Editor
{
    /// <summary>
    /// Editor-only synthesis of sparse facet-owned rounded-stone riverbed
    /// material-data candidates. No donor image or extracted stamp contributes
    /// to this generator and no output becomes a runtime asset.
    /// </summary>
    internal static class SparseRiverbedCandidateSynthesizer
    {
        internal const int AlgorithmVersion = 5;
        internal const int Resolution = 512;
        internal const int ProceduralMotifCount = 48;
        internal const int MaximumPlacementAttempts = 36000;

        private const int MotifFamilyCount = 5;
        private const int CrownProfileCount = 6;
        private const int EdgeProfileCount = 6;
        private const int BurialProfileCount = 5;
        private const int FeatureTypeCount = 7;
        private const int MacroBlockSize = 32;
        private const int MacroBlocksPerAxis = Resolution / MacroBlockSize;
        private const int MacroBlockCount =
            MacroBlocksPerAxis * MacroBlocksPerAxis;
        private const float MaximumOverlapFraction = 0.065f;
        private const float MinimumCenterSpacingScale = 0.78f;
        private const float MinimumAllowedRadialScale = 0.78f;
        private const float MaximumAllowedBoundaryPerturbation = 0.22f;
        private const float MaximumAllowedAspect = 1.85f;
        private const float MinimumAllowedExponent = 2.0f;
        private const float MinimumFeatureResidualRms = 0.020f;
        private const float MinimumHighCurvatureFraction = 0.012f;
        private const float MinimumFinalPlacedFeatureResidualRms = 0.018f;
        private const float MinimumFinalPlacedHighCurvatureFraction = 0.120f;
        private const float SeamMeanThreshold = 0.018f;
        private const float SeamP95Threshold = 0.090f;
        private const float SeamLocalExcessMeanThreshold = 0.012f;

        internal enum MotifFamily
        {
            RoundedPebble = 0,
            BroadOval = 1,
            LowSlab = 2,
            SoftAngular = 3,
            RoundedChip = 4
        }

        internal enum CrownProfile
        {
            RoundedDome = 0,
            FlattenedDome = 1,
            OffsetShoulder = 2,
            TwinShoulder = 3,
            OneSidedRise = 4,
            LowSlabTop = 5
        }

        internal enum EdgeProfile
        {
            SoftEven = 0,
            MixedHardness = 1,
            OneSideBuried = 2,
            ShoulderDrop = 3,
            BroadLocalChip = 4,
            FlattenedSide = 5
        }

        internal enum BurialProfile
        {
            LightEmbed = 0,
            HalfBuried = 1,
            OneSideBuried = 2,
            SlabSet = 3,
            ShallowSink = 4
        }

        internal enum FeatureType
        {
            PlanarFacet = 0,
            DiagonalRidge = 1,
            ShallowCrease = 2,
            LocalDepression = 3,
            SecondaryLobe = 4,
            RoundedNotch = 5,
            BuriedSideCut = 6
        }

        internal sealed class Definition
        {
            internal string StableId;
            internal string DisplayName;
            internal uint Seed;
            internal float TargetCoverage;
            internal float MinimumCoverage;
            internal float MaximumCoverage;
            internal float MinimumQuietBlockFraction;
            internal int MaximumOccupiedMacroBlocks;
            internal int MacroRegionCount;
            internal float[] FamilyWeights;
            internal float[] SizeWeights;
            internal int SubstrateVariant;
            internal float ReliefScale;
            internal float EmbeddingScale;
            internal float ContactScale;
            internal float StructurePreference;
            internal Color SubstrateDark;
            internal Color SubstrateLight;
            internal Color StoneDark;
            internal Color StoneLight;
            internal Color CavityColor;
        }

        internal sealed class Feature
        {
            internal FeatureType Type;
            internal float Angle;
            internal float OffsetX;
            internal float OffsetY;
            internal float Width;
            internal float Strength;
            internal float Secondary;
        }

        internal sealed class FacetPlane
        {
            internal float Angle;
            internal float Offset;
            internal float PrimarySlope;
            internal float SecondarySlope;
        }

        internal sealed class Motif
        {
            internal int Id;
            internal MotifFamily Family;
            internal CrownProfile Crown;
            internal EdgeProfile Edge;
            internal BurialProfile Burial;
            internal uint Seed;
            internal float Aspect;
            internal float Exponent;
            internal float Harmonic2;
            internal float Harmonic3;
            internal float Harmonic4;
            internal float Phase2;
            internal float Phase3;
            internal float Phase4;
            internal float FlattenAngle;
            internal float FlattenDepth;
            internal float FlattenWidth;
            internal float CrownShiftX;
            internal float CrownShiftY;
            internal float CrownExponent;
            internal float CrownAngle;
            internal float EdgeAngle;
            internal float EdgeStrength;
            internal float FacetBlend;
            internal FacetPlane[] FacetPlanes;
            internal float Relief;
            internal float Embedding;
            internal float TiltX;
            internal float TiltY;
            internal float Roughness;
            internal Feature[] Features;
            internal float MinimumRadialScale;
            internal float MaximumBoundaryPerturbation;
            internal float FeatureResidualRms;
            internal float HighCurvatureFraction;
        }

        internal sealed class Placement
        {
            internal int MotifId;
            internal MotifFamily Family;
            internal CrownProfile Crown;
            internal EdgeProfile Edge;
            internal BurialProfile Burial;
            internal int FeatureCount;
            internal int SizeBucket;
            internal int CenterX;
            internal int CenterY;
            internal float RotationRadians;
            internal float RadiusX;
            internal float RadiusY;
            internal float BoundingRadius;
            internal float Variation;
            internal float Embedding;
            internal float Relief;
            internal int AddedPixels;
        }

        internal sealed class SeamMetrics
        {
            internal float HorizontalMean;
            internal float HorizontalP95;
            internal float HorizontalLocalExcessMean;
            internal float VerticalMean;
            internal float VerticalP95;
            internal float VerticalLocalExcessMean;
        }

        internal sealed class CandidateResult
        {
            internal Definition Definition;
            internal string MotifCatalogFingerprint;
            internal readonly List<Placement> Placements =
                new List<Placement>();
            internal float[] SubstrateHeight;
            internal float[] SubstrateVariation;
            internal float[] Height;
            internal float[] StoneMask;
            internal float[] StoneVariation;
            internal float[] Cavity;
            internal float[] Roughness;
            internal Color32[] Normals;
            internal Color32[] ColorPreview;
            internal Color32[] PlacementDebug;
            internal Color32[] MotifCatalogPreview;
            internal Color32[] MotifNormalCatalogPreview;
            internal Color32[] FinalStructureDebug;
            internal float ActualCoverage;
            internal float QuietBlockFraction;
            internal int OccupiedMacroBlocks;
            internal int LargestConnectedStonePixels;
            internal int[] SizeBucketPlacements = new int[3];
            internal int[] FamilyPlacements = new int[MotifFamilyCount];
            internal int[] CrownPlacements = new int[CrownProfileCount];
            internal int[] EdgePlacements = new int[EdgeProfileCount];
            internal int[] BurialPlacements = new int[BurialProfileCount];
            internal int[] FeaturePlacements = new int[FeatureTypeCount];
            internal float AverageFeatureResidualRms;
            internal float AverageHighCurvatureFraction;
            internal float AverageModifierCount;
            internal float FinalPlacedFeatureResidualRms;
            internal float FinalPlacedHighCurvatureFraction;
            internal int ProposalCount;
            internal int DensityRejected;
            internal int SpacingRejected;
            internal int EmptyStampRejected;
            internal int OverlapRejected;
            internal int CoverageRejected;
            internal int QuietBlockRejected;
            internal SeamMetrics Seams;
            internal float[] MipOccupiedFractions = new float[5];
            internal string Fingerprint;
            internal string Failure;

            internal bool Succeeded => string.IsNullOrEmpty(Failure);
        }

        internal sealed class SynthesisResult
        {
            internal readonly List<CandidateResult> Candidates =
                new List<CandidateResult>();
            internal readonly List<Motif> Motifs = new List<Motif>();
            internal string MotifCatalogFingerprint;
            internal int[] MotifFamilyCounts = new int[MotifFamilyCount];
            internal int[] CrownProfileCounts = new int[CrownProfileCount];
            internal int[] EdgeProfileCounts = new int[EdgeProfileCount];
            internal int[] BurialProfileCounts = new int[BurialProfileCount];
            internal int[] FeatureTypeCounts = new int[FeatureTypeCount];
            internal int[] ModifierCountCounts = new int[4];
            internal float MinimumExponent = float.PositiveInfinity;
            internal float MaximumAspect;
            internal float MinimumRadialScale = float.PositiveInfinity;
            internal float MaximumBoundaryPerturbation;
            internal float MinimumFeatureResidualRms =
                float.PositiveInfinity;
            internal float MaximumFeatureResidualRms;
            internal float MinimumHighCurvatureFraction =
                float.PositiveInfinity;
            internal float MaximumHighCurvatureFraction;
            internal int ExtractedDonorPlacementCount;
            internal string CombinedFingerprint;
            internal bool Succeeded;
            internal string Failure;
        }

        private sealed class SubstrateData
        {
            internal float[] Height;
            internal float[] Variation;
            internal float[] Cavity;
            internal float[] Roughness;
        }

        private sealed class MacroRegion
        {
            internal float CenterX;
            internal float CenterY;
            internal float RadiusX;
            internal float RadiusY;
            internal float Rotation;
            internal float Strength;
        }

        private sealed class StampPixel
        {
            internal int DestinationIndex;
            internal float Mask;
            internal float RaisedHeight;
            internal float Depression;
            internal float Cavity;
            internal float Roughness;
            internal float Variation;
        }

        private static readonly Definition[] Definitions =
        {
            new Definition
            {
                StableId = "quiet-embedded-stones",
                DisplayName = "Quiet Embedded Stones",
                Seed = 2721u,
                TargetCoverage = 0.070f,
                MinimumCoverage = 0.060f,
                MaximumCoverage = 0.080f,
                MinimumQuietBlockFraction = 0.72f,
                MaximumOccupiedMacroBlocks = 71,
                MacroRegionCount = 5,
                FamilyWeights = new[] { 0.38f, 0.32f, 0.14f, 0.12f, 0.04f },
                SizeWeights = new[] { 0.06f, 0.50f, 0.44f },
                SubstrateVariant = 0,
                ReliefScale = 0.92f,
                EmbeddingScale = 1.18f,
                ContactScale = 0.52f,
                StructurePreference = 0.45f,
                SubstrateDark = new Color(0.42f, 0.37f, 0.29f, 1f),
                SubstrateLight = new Color(0.63f, 0.56f, 0.44f, 1f),
                StoneDark = new Color(0.31f, 0.32f, 0.30f, 1f),
                StoneLight = new Color(0.57f, 0.56f, 0.50f, 1f),
                CavityColor = new Color(0.24f, 0.21f, 0.17f, 1f)
            },
            new Definition
            {
                StableId = "natural-sparse-riverbed",
                DisplayName = "Natural Sparse Riverbed",
                Seed = 2722u,
                TargetCoverage = 0.088f,
                MinimumCoverage = 0.078f,
                MaximumCoverage = 0.098f,
                MinimumQuietBlockFraction = 0.66f,
                MaximumOccupiedMacroBlocks = 87,
                MacroRegionCount = 7,
                FamilyWeights = new[] { 0.28f, 0.27f, 0.22f, 0.19f, 0.04f },
                SizeWeights = new[] { 0.12f, 0.46f, 0.42f },
                SubstrateVariant = 1,
                ReliefScale = 1.02f,
                EmbeddingScale = 1.06f,
                ContactScale = 0.62f,
                StructurePreference = 0.60f,
                SubstrateDark = new Color(0.39f, 0.35f, 0.28f, 1f),
                SubstrateLight = new Color(0.60f, 0.53f, 0.41f, 1f),
                StoneDark = new Color(0.29f, 0.31f, 0.30f, 1f),
                StoneLight = new Color(0.58f, 0.57f, 0.51f, 1f),
                CavityColor = new Color(0.21f, 0.19f, 0.15f, 1f)
            },
            new Definition
            {
                StableId = "dense-sparse-riverbed",
                DisplayName = "Dense Sparse Riverbed",
                Seed = 2723u,
                TargetCoverage = 0.102f,
                MinimumCoverage = 0.092f,
                MaximumCoverage = 0.112f,
                MinimumQuietBlockFraction = 0.60f,
                MaximumOccupiedMacroBlocks = 107,
                MacroRegionCount = 8,
                FamilyWeights = new[] { 0.16f, 0.22f, 0.34f, 0.24f, 0.04f },
                SizeWeights = new[] { 0.08f, 0.42f, 0.50f },
                SubstrateVariant = 2,
                ReliefScale = 1.10f,
                EmbeddingScale = 1.08f,
                ContactScale = 0.70f,
                StructurePreference = 0.74f,
                SubstrateDark = new Color(0.36f, 0.33f, 0.27f, 1f),
                SubstrateLight = new Color(0.56f, 0.50f, 0.39f, 1f),
                StoneDark = new Color(0.28f, 0.30f, 0.29f, 1f),
                StoneLight = new Color(0.56f, 0.55f, 0.50f, 1f),
                CavityColor = new Color(0.20f, 0.18f, 0.15f, 1f)
            }
        };

        internal static float SeamMeanLimit => SeamMeanThreshold;
        internal static float SeamP95Limit => SeamP95Threshold;
        internal static float SeamLocalExcessMeanLimit =>
            SeamLocalExcessMeanThreshold;
        internal static float FeatureResidualRmsLimit =>
            MinimumFeatureResidualRms;
        internal static float HighCurvatureFractionLimit =>
            MinimumHighCurvatureFraction;
        internal static float FinalPlacedFeatureResidualRmsLimit =>
            MinimumFinalPlacedFeatureResidualRms;
        internal static float FinalPlacedHighCurvatureFractionLimit =>
            MinimumFinalPlacedHighCurvatureFraction;
        internal static int QuietMacroBlockSize => MacroBlockSize;
        internal static int QuietMacroBlockCount => MacroBlockCount;

        internal static SynthesisResult SynthesizeAll()
        {
            SynthesisResult result = new SynthesisResult
            {
                ExtractedDonorPlacementCount = 0
            };

            List<Motif> motifs = BuildMotifCatalog();
            result.Motifs.AddRange(motifs);
            if (!ValidateMotifCatalog(result))
            {
                return result;
            }

            result.MotifCatalogFingerprint = CalculateMotifCatalogFingerprint(
                motifs);
            Color32[] catalogPreview = BuildMotifCatalogPreview(motifs);
            Color32[] normalCatalogPreview =
                BuildMotifNormalCatalogPreview(motifs);
            StringBuilder combined = new StringBuilder(2048);
            combined.Append("v=");
            combined.Append(AlgorithmVersion);
            combined.Append(";catalog=");
            combined.Append(result.MotifCatalogFingerprint);
            combined.Append(';');

            for (int index = 0; index < Definitions.Length; index++)
            {
                CandidateResult candidate = SynthesizeCandidate(
                    Definitions[index],
                    motifs,
                    catalogPreview,
                    normalCatalogPreview);
                result.Candidates.Add(candidate);
                if (!candidate.Succeeded)
                {
                    result.Failure = candidate.Definition.DisplayName + ": " +
                        candidate.Failure;
                    return result;
                }

                combined.Append(candidate.Definition.StableId);
                combined.Append(':');
                combined.Append(candidate.Fingerprint);
                combined.Append('|');
            }

            result.CombinedFingerprint = CalculateSha256(
                Encoding.UTF8.GetBytes(combined.ToString()));
            result.Succeeded = true;
            return result;
        }

        private static List<Motif> BuildMotifCatalog()
        {
            List<Motif> motifs = new List<Motif>(ProceduralMotifCount);
            int[] counts = { 11, 10, 10, 9, 8 };
            int id = 0;
            for (int familyIndex = 0;
                 familyIndex < counts.Length;
                 familyIndex++)
            {
                for (int local = 0; local < counts[familyIndex]; local++)
                {
                    uint seed = (uint)(9200 + familyIndex * 149 + local * 23);
                    motifs.Add(BuildMotif(
                        id,
                        (MotifFamily)familyIndex,
                        seed));
                    id++;
                }
            }

            return motifs;
        }

        private static Motif BuildMotif(
            int id,
            MotifFamily family,
            uint seed)
        {
            DeterministicRandom random = new DeterministicRandom(seed);
            Motif motif = new Motif
            {
                Id = id,
                Family = family,
                Crown = (CrownProfile)(id % CrownProfileCount),
                Edge = (EdgeProfile)((id * 5 + (int)family) %
                    EdgeProfileCount),
                Burial = (BurialProfile)((id * 3 + (int)family) %
                    BurialProfileCount),
                Seed = seed,
                Phase2 = random.NextFloat() * Mathf.PI * 2f,
                Phase3 = random.NextFloat() * Mathf.PI * 2f,
                Phase4 = random.NextFloat() * Mathf.PI * 2f,
                FlattenAngle = random.NextFloat() * Mathf.PI * 2f,
                CrownShiftX = random.Range(-0.15f, 0.15f),
                CrownShiftY = random.Range(-0.15f, 0.15f),
                CrownAngle = random.NextFloat() * Mathf.PI * 2f,
                EdgeAngle = random.NextFloat() * Mathf.PI * 2f,
                TiltX = random.Range(-0.12f, 0.12f),
                TiltY = random.Range(-0.12f, 0.12f)
            };

            switch (family)
            {
                case MotifFamily.RoundedPebble:
                    motif.Aspect = random.Range(1.00f, 1.30f);
                    motif.Exponent = random.Range(2.00f, 2.70f);
                    motif.Harmonic2 = random.Range(0.018f, 0.050f);
                    motif.Harmonic3 = random.Range(0.014f, 0.034f);
                    motif.Harmonic4 = random.Range(0.009f, 0.024f);
                    motif.FlattenDepth = random.Range(0.018f, 0.060f);
                    motif.FlattenWidth = random.Range(0.75f, 1.10f);
                    motif.CrownExponent = random.Range(0.62f, 0.90f);
                    motif.EdgeStrength = random.Range(0.18f, 0.36f);
                    motif.Relief = random.Range(0.135f, 0.205f);
                    motif.Embedding = random.Range(0.13f, 0.27f);
                    motif.Roughness = random.Range(0.54f, 0.63f);
                    break;
                case MotifFamily.BroadOval:
                    motif.Aspect = random.Range(1.24f, 1.70f);
                    motif.Exponent = random.Range(2.05f, 2.90f);
                    motif.Harmonic2 = random.Range(0.014f, 0.040f);
                    motif.Harmonic3 = random.Range(0.014f, 0.034f);
                    motif.Harmonic4 = random.Range(0.009f, 0.024f);
                    motif.FlattenDepth = random.Range(0.020f, 0.065f);
                    motif.FlattenWidth = random.Range(0.74f, 1.10f);
                    motif.CrownExponent = random.Range(0.64f, 0.96f);
                    motif.EdgeStrength = random.Range(0.20f, 0.40f);
                    motif.Relief = random.Range(0.125f, 0.190f);
                    motif.Embedding = random.Range(0.16f, 0.31f);
                    motif.Roughness = random.Range(0.55f, 0.65f);
                    break;
                case MotifFamily.LowSlab:
                    motif.Aspect = random.Range(1.15f, 1.72f);
                    motif.Exponent = random.Range(2.60f, 4.10f);
                    motif.Harmonic2 = random.Range(0.018f, 0.046f);
                    motif.Harmonic3 = random.Range(0.010f, 0.030f);
                    motif.Harmonic4 = random.Range(0.005f, 0.021f);
                    motif.FlattenDepth = random.Range(0.028f, 0.080f);
                    motif.FlattenWidth = random.Range(0.68f, 1.02f);
                    motif.CrownExponent = random.Range(0.44f, 0.68f);
                    motif.EdgeStrength = random.Range(0.28f, 0.52f);
                    motif.Relief = random.Range(0.100f, 0.160f);
                    motif.Embedding = random.Range(0.24f, 0.42f);
                    motif.Roughness = random.Range(0.58f, 0.68f);
                    break;
                case MotifFamily.SoftAngular:
                    motif.Aspect = random.Range(1.02f, 1.52f);
                    motif.Exponent = random.Range(2.75f, 4.40f);
                    motif.Harmonic2 = random.Range(0.020f, 0.052f);
                    motif.Harmonic3 = random.Range(0.014f, 0.038f);
                    motif.Harmonic4 = random.Range(0.008f, 0.026f);
                    motif.FlattenDepth = random.Range(0.030f, 0.085f);
                    motif.FlattenWidth = random.Range(0.66f, 0.98f);
                    motif.CrownExponent = random.Range(0.52f, 0.82f);
                    motif.EdgeStrength = random.Range(0.30f, 0.56f);
                    motif.Relief = random.Range(0.130f, 0.210f);
                    motif.Embedding = random.Range(0.18f, 0.34f);
                    motif.Roughness = random.Range(0.56f, 0.67f);
                    break;
                default:
                    motif.Aspect = random.Range(1.00f, 1.48f);
                    motif.Exponent = random.Range(2.10f, 3.10f);
                    motif.Harmonic2 = random.Range(0.018f, 0.046f);
                    motif.Harmonic3 = random.Range(0.012f, 0.034f);
                    motif.Harmonic4 = random.Range(0.005f, 0.020f);
                    motif.FlattenDepth = random.Range(0.060f, 0.115f);
                    motif.FlattenWidth = random.Range(0.62f, 0.88f);
                    motif.CrownExponent = random.Range(0.56f, 0.88f);
                    motif.EdgeStrength = random.Range(0.24f, 0.48f);
                    motif.Relief = random.Range(0.110f, 0.175f);
                    motif.Embedding = random.Range(0.20f, 0.36f);
                    motif.Roughness = random.Range(0.57f, 0.69f);
                    break;
            }

            int facetCount = ResolveFacetPlaneCount(motif.Crown);
            motif.FacetBlend = ResolveFacetBlend(
                motif,
                random);
            motif.FacetPlanes = BuildFacetPlanes(
                facetCount,
                motif,
                random);

            int featureCount = 1 + id % 3;
            motif.Features = new Feature[featureCount];
            for (int featureIndex = 0;
                 featureIndex < featureCount;
                 featureIndex++)
            {
                FeatureType type = (FeatureType)(
                    (id * 2 + featureIndex * 3 + (int)family) %
                    FeatureTypeCount);
                motif.Features[featureIndex] = BuildFeature(
                    type,
                    random);
            }

            MeasureMotifBoundary(motif);
            MeasureMotifHeightComplexity(motif);
            return motif;
        }

        private static int ResolveFacetPlaneCount(
            CrownProfile crown)
        {
            switch (crown)
            {
                case CrownProfile.RoundedDome:
                    return 3;
                case CrownProfile.FlattenedDome:
                case CrownProfile.OffsetShoulder:
                case CrownProfile.OneSidedRise:
                    return 4;
                case CrownProfile.TwinShoulder:
                    return 5;
                default:
                    return 6;
            }
        }

        private static float ResolveFacetBlend(
            Motif motif,
            DeterministicRandom random)
        {
            float minimum;
            float maximum;
            switch (motif.Crown)
            {
                case CrownProfile.RoundedDome:
                    minimum = 0.72f;
                    maximum = 0.84f;
                    break;
                case CrownProfile.FlattenedDome:
                case CrownProfile.OffsetShoulder:
                case CrownProfile.OneSidedRise:
                    minimum = 0.80f;
                    maximum = 0.91f;
                    break;
                case CrownProfile.TwinShoulder:
                    minimum = 0.84f;
                    maximum = 0.94f;
                    break;
                default:
                    minimum = 0.88f;
                    maximum = 0.97f;
                    break;
            }

            if (motif.Family == MotifFamily.LowSlab ||
                motif.Family == MotifFamily.SoftAngular)
            {
                minimum += 0.03f;
                maximum += 0.03f;
            }

            return Mathf.Clamp01(random.Range(minimum, maximum));
        }

        private static FacetPlane[] BuildFacetPlanes(
            int count,
            Motif motif,
            DeterministicRandom random)
        {
            FacetPlane[] planes = new FacetPlane[count];
            float phase = random.NextFloat() * Mathf.PI * 2f;
            float apexHeight = ResolveFacetApexHeight(motif.Crown);
            float minimumSlope = motif.Crown == CrownProfile.LowSlabTop
                ? 0.12f
                : 0.18f;
            float maximumSlope = motif.Crown == CrownProfile.LowSlabTop
                ? 0.24f
                : 0.34f;
            for (int index = 0; index < count; index++)
            {
                float angle = phase +
                    index / (float)count * Mathf.PI * 2f +
                    random.Range(-0.30f, 0.30f);
                planes[index] = new FacetPlane
                {
                    Angle = angle,
                    Offset = apexHeight + random.Range(-0.06f, 0.04f),
                    PrimarySlope = -random.Range(
                        minimumSlope,
                        maximumSlope),
                    SecondarySlope = random.Range(-0.18f, 0.18f)
                };
            }

            return planes;
        }

        private static float ResolveFacetApexHeight(
            CrownProfile crown)
        {
            switch (crown)
            {
                case CrownProfile.FlattenedDome:
                    return 0.78f;
                case CrownProfile.OffsetShoulder:
                    return 0.84f;
                case CrownProfile.TwinShoulder:
                    return 0.86f;
                case CrownProfile.OneSidedRise:
                    return 0.82f;
                case CrownProfile.LowSlabTop:
                    return 0.70f;
                default:
                    return 0.88f;
            }
        }

        private static Feature BuildFeature(
            FeatureType type,
            DeterministicRandom random)
        {
            Feature feature = new Feature
            {
                Type = type,
                Angle = random.NextFloat() * Mathf.PI * 2f,
                OffsetX = random.Range(-0.34f, 0.34f),
                OffsetY = random.Range(-0.34f, 0.34f),
                Width = random.Range(0.16f, 0.40f),
                Secondary = random.Range(-0.28f, 0.28f)
            };

            switch (type)
            {
                case FeatureType.PlanarFacet:
                    feature.Strength = random.Range(0.42f, 0.76f);
                    feature.Width = random.Range(0.28f, 0.52f);
                    break;
                case FeatureType.DiagonalRidge:
                    feature.Strength = random.Range(0.038f, 0.082f);
                    feature.Width = random.Range(0.14f, 0.28f);
                    break;
                case FeatureType.ShallowCrease:
                    feature.Strength = random.Range(0.030f, 0.070f);
                    feature.Width = random.Range(0.12f, 0.24f);
                    break;
                case FeatureType.LocalDepression:
                    feature.Strength = random.Range(0.030f, 0.080f);
                    feature.Width = random.Range(0.15f, 0.30f);
                    break;
                case FeatureType.SecondaryLobe:
                    feature.Strength = random.Range(0.026f, 0.072f);
                    feature.Width = random.Range(0.20f, 0.42f);
                    break;
                case FeatureType.RoundedNotch:
                    feature.Strength = random.Range(0.040f, 0.085f);
                    feature.Width = random.Range(0.12f, 0.26f);
                    break;
                default:
                    feature.Strength = random.Range(0.28f, 0.58f);
                    feature.Width = random.Range(0.24f, 0.48f);
                    break;
            }

            return feature;
        }

        private static void MeasureMotifBoundary(Motif motif)
        {
            float minimum = float.PositiveInfinity;
            float maximumPerturbation = 0f;
            const int sampleCount = 720;
            for (int index = 0; index < sampleCount; index++)
            {
                float angle = index / (float)sampleCount * Mathf.PI * 2f;
                float radial = EvaluateRadialScale(motif, angle);
                minimum = Mathf.Min(minimum, radial);
                maximumPerturbation = Mathf.Max(
                    maximumPerturbation,
                    Mathf.Abs(radial - 1f));
            }

            motif.MinimumRadialScale = minimum;
            motif.MaximumBoundaryPerturbation = maximumPerturbation;
        }

        private static void MeasureMotifHeightComplexity(Motif motif)
        {
            const int sampleSize = 48;
            float[] finalHeight = new float[sampleSize * sampleSize];
            float[] referenceHeight = new float[sampleSize * sampleSize];
            bool[] occupied = new bool[sampleSize * sampleSize];
            double residualSum = 0.0;
            int occupiedCount = 0;
            for (int y = 0; y < sampleSize; y++)
            {
                float localY = Mathf.Lerp(
                    -1.08f,
                    1.08f,
                    (y + 0.5f) / sampleSize);
                for (int x = 0; x < sampleSize; x++)
                {
                    float localX = Mathf.Lerp(
                        -1.08f,
                        1.08f,
                        (x + 0.5f) / sampleSize);
                    int index = y * sampleSize + x;
                    float reference;
                    float height = EvaluateStoneHeight(
                        motif,
                        localX,
                        localY,
                        out reference);
                    finalHeight[index] = height;
                    referenceHeight[index] = reference;
                    if (reference <= 0.001f)
                    {
                        continue;
                    }

                    occupied[index] = true;
                    float residual = height - reference;
                    residualSum += residual * residual;
                    occupiedCount++;
                }
            }

            motif.FeatureResidualRms = occupiedCount > 0
                ? Mathf.Sqrt((float)(residualSum / occupiedCount))
                : 0f;

            int curvatureCount = 0;
            int curvatureSamples = 0;
            for (int y = 1; y < sampleSize - 1; y++)
            {
                for (int x = 1; x < sampleSize - 1; x++)
                {
                    int index = y * sampleSize + x;
                    if (!occupied[index])
                    {
                        continue;
                    }

                    float finalLaplacian =
                        finalHeight[index - 1] +
                        finalHeight[index + 1] +
                        finalHeight[index - sampleSize] +
                        finalHeight[index + sampleSize] -
                        finalHeight[index] * 4f;
                    float referenceLaplacian =
                        referenceHeight[index - 1] +
                        referenceHeight[index + 1] +
                        referenceHeight[index - sampleSize] +
                        referenceHeight[index + sampleSize] -
                        referenceHeight[index] * 4f;
                    if (Mathf.Abs(finalLaplacian - referenceLaplacian) >
                        0.010f)
                    {
                        curvatureCount++;
                    }

                    curvatureSamples++;
                }
            }

            motif.HighCurvatureFraction = curvatureSamples > 0
                ? curvatureCount / (float)curvatureSamples
                : 0f;
        }

        private static bool ValidateMotifCatalog(SynthesisResult result)
        {
            if (result.Motifs.Count != ProceduralMotifCount)
            {
                result.Failure = $"Expected {ProceduralMotifCount} procedural motifs; received {result.Motifs.Count}.";
                return false;
            }

            for (int index = 0; index < result.Motifs.Count; index++)
            {
                Motif motif = result.Motifs[index];
                int family = Mathf.Clamp(
                    (int)motif.Family,
                    0,
                    MotifFamilyCount - 1);
                int crown = Mathf.Clamp(
                    (int)motif.Crown,
                    0,
                    CrownProfileCount - 1);
                int edge = Mathf.Clamp(
                    (int)motif.Edge,
                    0,
                    EdgeProfileCount - 1);
                int burial = Mathf.Clamp(
                    (int)motif.Burial,
                    0,
                    BurialProfileCount - 1);
                result.MotifFamilyCounts[family]++;
                result.CrownProfileCounts[crown]++;
                result.EdgeProfileCounts[edge]++;
                result.BurialProfileCounts[burial]++;
                int modifierCount = motif.Features != null
                    ? motif.Features.Length
                    : 0;
                if (modifierCount >= 0 &&
                    modifierCount < result.ModifierCountCounts.Length)
                {
                    result.ModifierCountCounts[modifierCount]++;
                }

                if (motif.Features != null)
                {
                    for (int featureIndex = 0;
                         featureIndex < motif.Features.Length;
                         featureIndex++)
                    {
                        int feature = Mathf.Clamp(
                            (int)motif.Features[featureIndex].Type,
                            0,
                            FeatureTypeCount - 1);
                        result.FeatureTypeCounts[feature]++;
                    }
                }

                result.MinimumExponent = Mathf.Min(
                    result.MinimumExponent,
                    motif.Exponent);
                result.MaximumAspect = Mathf.Max(
                    result.MaximumAspect,
                    motif.Aspect);
                result.MinimumRadialScale = Mathf.Min(
                    result.MinimumRadialScale,
                    motif.MinimumRadialScale);
                result.MaximumBoundaryPerturbation = Mathf.Max(
                    result.MaximumBoundaryPerturbation,
                    motif.MaximumBoundaryPerturbation);
                result.MinimumFeatureResidualRms = Mathf.Min(
                    result.MinimumFeatureResidualRms,
                    motif.FeatureResidualRms);
                result.MaximumFeatureResidualRms = Mathf.Max(
                    result.MaximumFeatureResidualRms,
                    motif.FeatureResidualRms);
                result.MinimumHighCurvatureFraction = Mathf.Min(
                    result.MinimumHighCurvatureFraction,
                    motif.HighCurvatureFraction);
                result.MaximumHighCurvatureFraction = Mathf.Max(
                    result.MaximumHighCurvatureFraction,
                    motif.HighCurvatureFraction);

                if (motif.Exponent < MinimumAllowedExponent ||
                    motif.Aspect > MaximumAllowedAspect ||
                    motif.MinimumRadialScale < MinimumAllowedRadialScale ||
                    motif.MaximumBoundaryPerturbation >
                        MaximumAllowedBoundaryPerturbation ||
                    motif.FlattenWidth < 0.55f)
                {
                    result.Failure = string.Format(
                        CultureInfo.InvariantCulture,
                        "Procedural motif {0} violates rounded-shape bounds (family {1}, exponent {2:0.000}, aspect {3:0.000}, minimum radial scale {4:0.000}, maximum perturbation {5:0.000}, flatten width {6:0.000}).",
                        motif.Id,
                        motif.Family,
                        motif.Exponent,
                        motif.Aspect,
                        motif.MinimumRadialScale,
                        motif.MaximumBoundaryPerturbation,
                        motif.FlattenWidth);
                    return false;
                }

                if (modifierCount < 1 || modifierCount > 3)
                {
                    result.Failure =
                        $"Procedural motif {motif.Id} has {modifierCount} modifiers; expected one to three.";
                    return false;
                }

                if (motif.FeatureResidualRms < MinimumFeatureResidualRms ||
                    motif.HighCurvatureFraction <
                        MinimumHighCurvatureFraction)
                {
                    result.Failure = string.Format(
                        CultureInfo.InvariantCulture,
                        "Procedural motif {0} is insufficiently structured (feature residual RMS {1:0.0000}, high-curvature fraction {2:0.0000}).",
                        motif.Id,
                        motif.FeatureResidualRms,
                        motif.HighCurvatureFraction);
                    return false;
                }
            }

            if (!AllPositive(result.MotifFamilyCounts) ||
                !AllPositive(result.CrownProfileCounts) ||
                !AllPositive(result.EdgeProfileCounts) ||
                !AllPositive(result.BurialProfileCounts) ||
                !AllPositive(result.FeatureTypeCounts))
            {
                result.Failure =
                    "Procedural motif catalog does not include every required family, profile, and feature type.";
                return false;
            }

            return true;
        }

        private static bool AllPositive(int[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (values[index] <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static CandidateResult SynthesizeCandidate(
            Definition definition,
            IReadOnlyList<Motif> motifs,
            Color32[] catalogPreview,
            Color32[] normalCatalogPreview)
        {
            SubstrateData substrate = BuildPeriodicSubstrate(definition);
            int pixelCount = Resolution * Resolution;
            CandidateResult result = new CandidateResult
            {
                Definition = definition,
                MotifCatalogFingerprint = CalculateMotifCatalogFingerprint(
                    motifs),
                SubstrateHeight = substrate.Height,
                SubstrateVariation = substrate.Variation,
                Height = (float[])substrate.Height.Clone(),
                StoneMask = new float[pixelCount],
                StoneVariation = new float[pixelCount],
                Cavity = (float[])substrate.Cavity.Clone(),
                Roughness = (float[])substrate.Roughness.Clone(),
                PlacementDebug = BuildSubstrateDebug(substrate.Variation),
                MotifCatalogPreview = (Color32[])catalogPreview.Clone(),
                MotifNormalCatalogPreview =
                    (Color32[])normalCatalogPreview.Clone()
            };

            DeterministicRandom random = new DeterministicRandom(
                definition.Seed);
            MacroRegion[] macroRegions = BuildMacroRegions(definition);
            bool[] occupiedMacroBlocks = new bool[MacroBlockCount];
            int[] macroBlockVisitGeneration = new int[MacroBlockCount];
            int visitGeneration = 0;
            int occupiedPixels = 0;
            float residualSum = 0f;
            float curvatureSum = 0f;
            float modifierCountSum = 0f;
            for (int attempt = 0;
                 attempt < MaximumPlacementAttempts;
                 attempt++)
            {
                result.ProposalCount++;
                int centerX = random.NextInt(Resolution);
                int centerY = random.NextInt(Resolution);
                float density = PeriodicDensity(
                    definition,
                    macroRegions,
                    centerX,
                    centerY);
                float densityAcceptance = Mathf.Lerp(
                    0.012f,
                    0.82f,
                    Mathf.Pow(density, 1.80f));
                if (random.NextFloat() > densityAcceptance)
                {
                    result.DensityRejected++;
                    continue;
                }

                int familyIndex = SelectWeightedIndex(
                    definition.FamilyWeights,
                    random.NextFloat());
                int sizeBucket = SelectWeightedIndex(
                    definition.SizeWeights,
                    random.NextFloat());
                Motif motif = SelectMotif(
                    motifs,
                    (MotifFamily)familyIndex,
                    definition,
                    result,
                    random);
                if (motif == null)
                {
                    result.Failure =
                        "No procedural motif exists for family " +
                        ((MotifFamily)familyIndex).ToString() + ".";
                    return result;
                }

                ResolvePlacementSize(
                    motif,
                    sizeBucket,
                    definition,
                    random,
                    out float radiusX,
                    out float radiusY);
                float rotation = random.NextFloat() * Mathf.PI * 2f;
                float boundingRadius = Mathf.Max(radiusX, radiusY) * 1.06f;
                if (!HasPlacementSpacing(
                        result.Placements,
                        centerX,
                        centerY,
                        boundingRadius))
                {
                    result.SpacingRejected++;
                    continue;
                }

                float variation = random.NextFloat();
                float embedding = Mathf.Clamp01(
                    motif.Embedding * definition.EmbeddingScale *
                    random.Range(0.88f, 1.12f));
                float relief = motif.Relief * definition.ReliefScale *
                    random.Range(0.90f, 1.10f);
                List<StampPixel> stamp = BuildProceduralStamp(
                    motif,
                    centerX,
                    centerY,
                    rotation,
                    radiusX,
                    radiusY,
                    variation,
                    embedding,
                    relief,
                    definition.ContactScale);
                if (stamp.Count == 0)
                {
                    result.EmptyStampRejected++;
                    continue;
                }

                int overlap = 0;
                int newPixels = 0;
                int occupiedStampPixels = 0;
                for (int stampIndex = 0;
                     stampIndex < stamp.Count;
                     stampIndex++)
                {
                    StampPixel pixel = stamp[stampIndex];
                    if (pixel.Mask <= 0.5f)
                    {
                        continue;
                    }

                    occupiedStampPixels++;
                    if (result.StoneMask[pixel.DestinationIndex] > 0.5f)
                    {
                        overlap++;
                    }
                    else
                    {
                        newPixels++;
                    }
                }

                if (occupiedStampPixels == 0)
                {
                    result.EmptyStampRejected++;
                    continue;
                }

                if (overlap / (float)occupiedStampPixels >
                    MaximumOverlapFraction)
                {
                    result.OverlapRejected++;
                    continue;
                }

                float proposedCoverage =
                    (occupiedPixels + newPixels) / (float)pixelCount;
                if (proposedCoverage > definition.MaximumCoverage)
                {
                    result.CoverageRejected++;
                    continue;
                }

                visitGeneration++;
                int newMacroBlocks = CountNewMacroBlocks(
                    stamp,
                    occupiedMacroBlocks,
                    macroBlockVisitGeneration,
                    visitGeneration);
                if (result.OccupiedMacroBlocks + newMacroBlocks >
                    definition.MaximumOccupiedMacroBlocks)
                {
                    result.QuietBlockRejected++;
                    continue;
                }

                Placement placement = new Placement
                {
                    MotifId = motif.Id,
                    Family = motif.Family,
                    Crown = motif.Crown,
                    Edge = motif.Edge,
                    Burial = motif.Burial,
                    FeatureCount = motif.Features.Length,
                    SizeBucket = sizeBucket,
                    CenterX = centerX,
                    CenterY = centerY,
                    RotationRadians = rotation,
                    RadiusX = radiusX,
                    RadiusY = radiusY,
                    BoundingRadius = boundingRadius,
                    Variation = variation,
                    Embedding = embedding,
                    Relief = relief,
                    AddedPixels = newPixels
                };
                CommitStamp(result, stamp, placement);
                MarkOccupiedMacroBlocks(
                    stamp,
                    occupiedMacroBlocks);
                result.OccupiedMacroBlocks += newMacroBlocks;
                result.Placements.Add(placement);
                result.SizeBucketPlacements[Mathf.Clamp(
                    sizeBucket,
                    0,
                    2)]++;
                result.FamilyPlacements[Mathf.Clamp(
                    (int)motif.Family,
                    0,
                    MotifFamilyCount - 1)]++;
                result.CrownPlacements[Mathf.Clamp(
                    (int)motif.Crown,
                    0,
                    CrownProfileCount - 1)]++;
                result.EdgePlacements[Mathf.Clamp(
                    (int)motif.Edge,
                    0,
                    EdgeProfileCount - 1)]++;
                result.BurialPlacements[Mathf.Clamp(
                    (int)motif.Burial,
                    0,
                    BurialProfileCount - 1)]++;
                for (int featureIndex = 0;
                     featureIndex < motif.Features.Length;
                     featureIndex++)
                {
                    result.FeaturePlacements[Mathf.Clamp(
                        (int)motif.Features[featureIndex].Type,
                        0,
                        FeatureTypeCount - 1)]++;
                }

                residualSum += motif.FeatureResidualRms;
                curvatureSum += motif.HighCurvatureFraction;
                modifierCountSum += motif.Features.Length;
                occupiedPixels += newPixels;
                result.ActualCoverage = occupiedPixels / (float)pixelCount;

                if (result.ActualCoverage >= definition.TargetCoverage &&
                    result.ActualCoverage >= definition.MinimumCoverage &&
                    result.ActualCoverage <= definition.MaximumCoverage)
                {
                    break;
                }
            }

            if (result.ActualCoverage < definition.MinimumCoverage ||
                result.ActualCoverage > definition.MaximumCoverage)
            {
                result.Failure = string.Format(
                    CultureInfo.InvariantCulture,
                    "Coverage {0:0.0000} is outside accepted range {1:0.0000}–{2:0.0000} after {3} proposals.",
                    result.ActualCoverage,
                    definition.MinimumCoverage,
                    definition.MaximumCoverage,
                    result.ProposalCount);
                return result;
            }

            if (result.Placements.Count > 0)
            {
                float reciprocal = 1f / result.Placements.Count;
                result.AverageFeatureResidualRms = residualSum * reciprocal;
                result.AverageHighCurvatureFraction =
                    curvatureSum * reciprocal;
                result.AverageModifierCount =
                    modifierCountSum * reciprocal;
            }

            MeasureFinalPlacedStructure(result);
            result.Normals = BuildNormals(
                result.Height,
                Resolution,
                Resolution,
                30.0f);
            result.ColorPreview = BuildColorPreview(result);
            result.QuietBlockFraction = CalculateQuietBlockFraction(
                result.StoneMask,
                MacroBlockSize);
            result.LargestConnectedStonePixels =
                CalculateLargestWrappedComponent(result.StoneMask);
            result.Seams = MeasureSeams(result.Height);
            CalculateMipOccupancy(
                result.StoneMask,
                result.MipOccupiedFractions);
            result.Fingerprint = CalculateCandidateFingerprint(result);
            return result;
        }

        private static Motif SelectMotif(
            IReadOnlyList<Motif> motifs,
            MotifFamily family,
            Definition definition,
            CandidateResult candidate,
            DeterministicRandom random)
        {
            float total = 0f;
            for (int index = 0; index < motifs.Count; index++)
            {
                Motif motif = motifs[index];
                if (motif.Family != family)
                {
                    continue;
                }

                total += ResolveMotifSelectionWeight(
                    motif,
                    definition,
                    candidate);
            }

            if (total <= 0.000001f)
            {
                return null;
            }

            float target = random.NextFloat() * total;
            float accumulated = 0f;
            for (int index = 0; index < motifs.Count; index++)
            {
                Motif motif = motifs[index];
                if (motif.Family != family)
                {
                    continue;
                }

                accumulated += ResolveMotifSelectionWeight(
                    motif,
                    definition,
                    candidate);
                if (target <= accumulated)
                {
                    return motif;
                }
            }

            return null;
        }

        private static float ResolveMotifSelectionWeight(
            Motif motif,
            Definition definition,
            CandidateResult candidate)
        {
            float structured = Mathf.Clamp01(
                (motif.FeatureResidualRms -
                 MinimumFeatureResidualRms) / 0.12f);
            float preference = Mathf.Clamp01(
                definition.StructurePreference);
            float weight = Mathf.Lerp(
                1.35f - structured * 0.55f,
                0.55f + structured * 1.45f,
                preference);

            if (preference < 0.40f)
            {
                if (motif.Crown == CrownProfile.RoundedDome ||
                    motif.Crown == CrownProfile.FlattenedDome)
                {
                    weight *= 1.30f;
                }

                if (motif.Edge == EdgeProfile.SoftEven ||
                    motif.Edge == EdgeProfile.OneSideBuried)
                {
                    weight *= 1.20f;
                }

                if (motif.Burial == BurialProfile.HalfBuried ||
                    motif.Burial == BurialProfile.ShallowSink)
                {
                    weight *= 1.18f;
                }
            }
            else if (preference > 0.70f)
            {
                if (motif.Crown == CrownProfile.OffsetShoulder ||
                    motif.Crown == CrownProfile.TwinShoulder ||
                    motif.Crown == CrownProfile.LowSlabTop)
                {
                    weight *= 1.22f;
                }

                if (motif.Edge == EdgeProfile.MixedHardness ||
                    motif.Edge == EdgeProfile.ShoulderDrop)
                {
                    weight *= 1.22f;
                }
            }

            if (candidate != null)
            {
                if (candidate.CrownPlacements[(int)motif.Crown] == 0)
                {
                    weight *= 1.75f;
                }

                if (candidate.EdgePlacements[(int)motif.Edge] == 0)
                {
                    weight *= 1.60f;
                }

                if (candidate.BurialPlacements[(int)motif.Burial] == 0)
                {
                    weight *= 1.35f;
                }

                int unseenFeatures = 0;
                for (int index = 0; index < motif.Features.Length; index++)
                {
                    if (candidate.FeaturePlacements[
                            (int)motif.Features[index].Type] == 0)
                    {
                        unseenFeatures++;
                    }
                }

                weight *= 1f + unseenFeatures * 0.22f;
            }

            return Mathf.Max(0.05f, weight);
        }

        private static int SelectWeightedIndex(float[] weights, float value)
        {
            float total = 0f;
            for (int index = 0; index < weights.Length; index++)
            {
                total += Mathf.Max(0f, weights[index]);
            }

            if (total <= 0.000001f)
            {
                return 0;
            }

            float target = value * total;
            float accumulated = 0f;
            for (int index = 0; index < weights.Length; index++)
            {
                accumulated += Mathf.Max(0f, weights[index]);
                if (target <= accumulated)
                {
                    return index;
                }
            }

            return weights.Length - 1;
        }

        private static void ResolvePlacementSize(
            Motif motif,
            int sizeBucket,
            Definition definition,
            DeterministicRandom random,
            out float radiusX,
            out float radiusY)
        {
            float baseRadius;
            switch (sizeBucket)
            {
                case 0:
                    baseRadius = random.Range(10.0f, 14.5f);
                    break;
                case 1:
                    baseRadius = random.Range(16.5f, 23.5f);
                    break;
                default:
                    baseRadius = random.Range(24.5f, 32.5f);
                    break;
            }

            if (definition.SubstrateVariant == 2)
            {
                baseRadius *= 1.04f;
            }

            float aspectRoot = Mathf.Sqrt(motif.Aspect);
            radiusX = baseRadius * aspectRoot;
            radiusY = baseRadius / aspectRoot;
        }

        private static bool HasPlacementSpacing(
            IReadOnlyList<Placement> placements,
            int centerX,
            int centerY,
            float radius)
        {
            for (int index = 0; index < placements.Count; index++)
            {
                Placement existing = placements[index];
                float deltaX = WrappedDelta(centerX - existing.CenterX);
                float deltaY = WrappedDelta(centerY - existing.CenterY);
                float minimum = (radius + existing.BoundingRadius) *
                    MinimumCenterSpacingScale;
                if (deltaX * deltaX + deltaY * deltaY <
                    minimum * minimum)
                {
                    return false;
                }
            }

            return true;
        }

        private static float WrappedDelta(float value)
        {
            value = Mathf.Abs(value);
            return Mathf.Min(value, Resolution - value);
        }

        private static List<StampPixel> BuildProceduralStamp(
            Motif motif,
            int centerX,
            int centerY,
            float rotation,
            float radiusX,
            float radiusY,
            float variation,
            float embedding,
            float relief,
            float contactScale)
        {
            float minimumRadius = Mathf.Min(radiusX, radiusY);
            float outerContactPixels = Mathf.Lerp(2.8f, 4.8f, contactScale);
            int extent = Mathf.CeilToInt(
                Mathf.Max(radiusX, radiusY) + outerContactPixels + 2f);
            List<StampPixel> pixels = new List<StampPixel>(
                (extent * 2 + 1) * (extent * 2 + 1));
            float cosine = Mathf.Cos(rotation);
            float sine = Mathf.Sin(rotation);
            float antialias = 1.20f / Mathf.Max(4f, minimumRadius);
            float narrowContactPixels = Mathf.Lerp(0.75f, 1.45f, contactScale);
            float broadDepressionDepth = Mathf.Lerp(0.0026f, 0.0062f, contactScale);
            float narrowCavityStrength = Mathf.Lerp(0.045f, 0.11f, contactScale);

            for (int offsetY = -extent; offsetY <= extent; offsetY++)
            {
                for (int offsetX = -extent; offsetX <= extent; offsetX++)
                {
                    float localX =
                        (cosine * offsetX + sine * offsetY) / radiusX;
                    float localY =
                        (-sine * offsetX + cosine * offsetY) / radiusY;
                    float angle = Mathf.Atan2(localY, localX);
                    float signed = EvaluateRadialScale(motif, angle) -
                        EvaluateSuperellipseDistance(
                            localX,
                            localY,
                            motif.Exponent);
                    float mask = SmoothStep(
                        -antialias,
                        antialias,
                        signed);
                    float approximateDistancePixels = signed * minimumRadius;
                    float outsideDistance = Mathf.Max(0f, -approximateDistancePixels);
                    float broadDepression = outsideDistance < outerContactPixels
                        ? SmoothStep(outerContactPixels, 0f, outsideDistance)
                        : 0f;
                    float narrowContact = outsideDistance < narrowContactPixels
                        ? SmoothStep(narrowContactPixels, 0f, outsideDistance)
                        : 0f;
                    if (mask <= 0.001f && broadDepression <= 0.001f)
                    {
                        continue;
                    }

                    float referenceHeight;
                    float normalizedHeight = EvaluateStoneHeight(
                        motif,
                        localX,
                        localY,
                        out referenceHeight);
                    float burialFactor = ResolveBurialFactor(motif, localX, localY);
                    float contactWeight = ResolveDirectionalContactWeight(motif, localX, localY);
                    float contactBreakup = ResolveContactBreakup(motif, angle, localX, localY);
                    float finalContactWeight = contactWeight * contactBreakup;
                    float burialReduction = Mathf.Clamp01(embedding * burialFactor * 0.44f);
                    float embeddedHeight = normalizedHeight * Mathf.Lerp(1f, 0.50f, burialReduction);
                    float raised = relief * embeddedHeight * mask;
                    float insideContact = mask *
                        SmoothStep(0.16f, 0.035f, Mathf.Max(0f, signed)) *
                        0.016f * contactScale *
                        finalContactWeight;
                    float depression = broadDepressionDepth *
                        broadDepression *
                        (1f - mask * 0.88f) *
                        Mathf.Lerp(0.70f, 1.12f, burialFactor) *
                        finalContactWeight;
                    float cavity = Mathf.Max(
                        narrowContact * narrowCavityStrength *
                            (1f - mask * 0.88f) *
                            finalContactWeight,
                        insideContact);
                    if (finalContactWeight < 0.10f)
                    {
                        depression *= 0.20f;
                        cavity *= 0.10f;
                    }

                    int outputX = Wrap(centerX + offsetX, Resolution);
                    int outputY = Wrap(centerY + offsetY, Resolution);
                    pixels.Add(new StampPixel
                    {
                        DestinationIndex = outputY * Resolution + outputX,
                        Mask = mask,
                        RaisedHeight = raised,
                        Depression = depression,
                        Cavity = cavity,
                        Roughness = motif.Roughness,
                        Variation = variation
                    });
                }
            }

            return pixels;
        }

        private static float EvaluateStoneHeight(
            Motif motif,
            float localX,
            float localY,
            out float referenceHeight)
        {
            float angle = Mathf.Atan2(localY, localX);
            float radialScale = EvaluateRadialScale(motif, angle);
            float distance = EvaluateSuperellipseDistance(
                localX,
                localY,
                motif.Exponent);
            float inside = Mathf.Clamp01(
                1f - distance / Mathf.Max(0.001f, radialScale));
            if (inside <= 0f)
            {
                referenceHeight = 0f;
                return 0f;
            }

            float shiftedX = localX - motif.CrownShiftX;
            float shiftedY = localY - motif.CrownShiftY;
            float shiftedDistance = EvaluateSuperellipseDistance(
                shiftedX,
                shiftedY,
                Mathf.Lerp(2f, motif.Exponent, 0.36f));
            float shiftedInside = Mathf.Clamp01(
                1f - shiftedDistance / Mathf.Max(0.001f, radialScale));
            referenceHeight = Mathf.Pow(shiftedInside, motif.CrownExponent);

            float height = ApplyCrownProfile(
                motif,
                localX,
                localY,
                inside,
                shiftedInside,
                referenceHeight);
            height = ApplyFacetEnvelope(
                motif,
                localX,
                localY,
                inside,
                height);
            for (int index = 0; index < motif.Features.Length; index++)
            {
                height = ApplyFeature(
                    motif.Features[index],
                    localX,
                    localY,
                    inside,
                    height);
            }

            height = ApplyEdgeProfile(
                motif,
                localX,
                localY,
                inside,
                height);
            float tilt = Mathf.Clamp(
                1f + motif.TiltX * localX +
                motif.TiltY * localY,
                0.82f,
                1.20f);
            height *= tilt;

            float micro = (LocalStoneNoise(motif, localX, localY) - 0.5f) *
                0.045f * SmoothStep(0.06f, 0.60f, inside);
            height += micro;

            float edgeWidth = ResolveFinalEdgeShoulderWidth(motif);
            float edgeSupport = SmoothStep(0f, edgeWidth, inside);
            return Mathf.Clamp01(height) * edgeSupport;
        }

        private static float ApplyCrownProfile(
            Motif motif,
            float x,
            float y,
            float inside,
            float shiftedInside,
            float reference)
        {
            float direction = DirectionalCoordinate(x, y, motif.CrownAngle);
            float shoulder = Mathf.Pow(Mathf.Clamp01(inside), 0.38f) * 0.78f;
            float plateau = Mathf.Pow(Mathf.Clamp01(shiftedInside), 0.55f) * 0.88f;
            switch (motif.Crown)
            {
                case CrownProfile.FlattenedDome:
                {
                    float cap = Mathf.Min(plateau, 0.82f + direction * 0.05f);
                    return Mathf.Lerp(reference * 0.74f + shoulder * 0.22f, cap, 0.52f);
                }
                case CrownProfile.OffsetShoulder:
                {
                    float rise = SmoothStep(-0.45f, 0.48f, direction);
                    float cap = Mathf.Lerp(0.58f, 0.86f, rise);
                    return Mathf.Max(reference * Mathf.Lerp(0.72f, 0.98f, rise), shoulder * 0.76f + cap * 0.24f);
                }
                case CrownProfile.TwinShoulder:
                {
                    float a = DirectionalCoordinate(x, y, motif.CrownAngle + Mathf.PI * 0.5f);
                    float ribs = Mathf.Exp(-0.5f * Mathf.Pow((a - 0.24f) / 0.20f, 2f)) +
                        Mathf.Exp(-0.5f * Mathf.Pow((a + 0.24f) / 0.20f, 2f));
                    return Mathf.Max(reference * 0.68f + shoulder * 0.18f, 0.52f + ribs * 0.16f);
                }
                case CrownProfile.OneSidedRise:
                {
                    float rise = SmoothStep(-0.70f, 0.58f, direction);
                    return Mathf.Max(reference * Mathf.Lerp(0.62f, 1.02f, rise), shoulder * 0.70f);
                }
                case CrownProfile.LowSlabTop:
                {
                    float slab = Mathf.Min(0.76f + direction * 0.03f, shoulder * 0.92f + 0.10f);
                    return Mathf.Max(reference * 0.56f, slab);
                }
                default:
                    return Mathf.Max(reference * 0.78f, shoulder * 0.68f);
            }
        }

        private static float ApplyFacetEnvelope(
            Motif motif,
            float x,
            float y,
            float inside,
            float height)
        {
            if (motif.FacetPlanes == null ||
                motif.FacetPlanes.Length == 0 ||
                motif.FacetBlend <= 0.0001f)
            {
                return height;
            }

            float faceted = Mathf.Max(height * 0.58f, inside * 0.16f);
            float interiorWeight = SmoothStep(0.10f, 0.34f, inside);
            for (int index = 0; index < motif.FacetPlanes.Length; index++)
            {
                FacetPlane plane = motif.FacetPlanes[index];
                float centerX = Mathf.Cos(plane.Angle) * (0.10f + index * 0.02f) + motif.CrownShiftX * 0.22f;
                float centerY = Mathf.Sin(plane.Angle) * (0.10f + index * 0.02f) + motif.CrownShiftY * 0.22f;
                float localX = x - centerX;
                float localY = y - centerY;
                float primary = DirectionalCoordinate(localX, localY, plane.Angle);
                float secondary = DirectionalCoordinate(localX, localY, plane.Angle + Mathf.PI * 0.5f);
                float region = Mathf.Exp(-0.5f * (
                    Mathf.Pow(localX / 0.42f, 2f) +
                    Mathf.Pow(localY / 0.30f, 2f)));
                float planeHeight = plane.Offset +
                    primary * plane.PrimarySlope * 0.55f +
                    secondary * plane.SecondarySlope * 0.55f;
                float patch = Mathf.Clamp01(planeHeight) * region;
                faceted = Mathf.Max(faceted, patch);
            }

            float blend = Mathf.Clamp01(motif.FacetBlend * 0.70f) * interiorWeight;
            return Mathf.Lerp(height, faceted, blend);
        }

        private static float ApplyEdgeProfile(
            Motif motif,
            float x,
            float y,
            float inside,
            float height)
        {
            float direction = DirectionalCoordinate(x, y, motif.EdgeAngle);
            float strength = motif.EdgeStrength;
            float edgeBand = 1f - SmoothStep(0.20f, 0.72f, inside);
            float edgeNoise = 0.65f + 0.35f * LocalStoneNoise(motif, x * 1.2f, y * 1.2f);
            switch (motif.Edge)
            {
                case EdgeProfile.MixedHardness:
                {
                    float sharpness = Mathf.Lerp(0.62f, 1.24f, SmoothStep(-0.68f, 0.68f, direction));
                    float edge = Mathf.Pow(Mathf.Clamp01(inside), sharpness);
                    return Mathf.Lerp(height, height * edge / Mathf.Max(0.001f, inside), strength * 0.70f);
                }
                case EdgeProfile.OneSideBuried:
                {
                    float buried = 1f - SmoothStep(-0.52f, 0.44f, direction);
                    return height * Mathf.Lerp(0.56f, 1f, Mathf.Lerp(1f, buried, strength));
                }
                case EdgeProfile.ShoulderDrop:
                {
                    float drop = Mathf.Lerp(0.42f, 1f, SmoothStep(0.04f, 0.36f, inside));
                    return Mathf.Lerp(height, height * drop, strength * 0.78f);
                }
                case EdgeProfile.BroadLocalChip:
                {
                    float chipAngle = WrappedAngle(Mathf.Atan2(y, x) - motif.EdgeAngle);
                    float chip = Mathf.Exp(-0.5f * Mathf.Pow(chipAngle / 0.48f, 2f));
                    return height * (1f - chip * edgeBand * edgeNoise * strength * 0.50f);
                }
                case EdgeProfile.FlattenedSide:
                {
                    float flat = SmoothStep(-0.16f, 0.62f, direction);
                    return Mathf.Lerp(height * 0.88f, height, flat);
                }
                default:
                    return height * Mathf.Lerp(0.96f, 1f, edgeBand * 0.35f * edgeNoise);
            }
        }

        private static float ApplyFeature(
            Feature feature,
            float x,
            float y,
            float inside,
            float height)
        {
            float cosine = Mathf.Cos(feature.Angle);
            float sine = Mathf.Sin(feature.Angle);
            float localX = x - feature.OffsetX;
            float localY = y - feature.OffsetY;
            float along = localX * cosine + localY * sine;
            float across = -localX * sine + localY * cosine;
            float lineDistance = Mathf.Abs(across - feature.Secondary);
            float radial = Mathf.Sqrt(localX * localX + localY * localY);
            float interior = SmoothStep(0f, 0.24f, inside);
            float localRegion = Mathf.Exp(-0.5f * (
                Mathf.Pow(localX / Mathf.Max(0.12f, feature.Width * 1.25f), 2f) +
                Mathf.Pow(localY / Mathf.Max(0.12f, feature.Width), 2f)));

            switch (feature.Type)
            {
                case FeatureType.PlanarFacet:
                {
                    float cap = Mathf.Clamp01(0.56f + along * 0.14f + across * 0.06f);
                    float clipped = Mathf.Min(height, cap);
                    return Mathf.Lerp(height, clipped, feature.Strength * localRegion * interior);
                }
                case FeatureType.DiagonalRidge:
                {
                    float ridge = Mathf.Exp(-0.5f * Mathf.Pow(lineDistance / Mathf.Max(0.08f, feature.Width * 0.70f), 2f)) *
                        Mathf.Exp(-0.5f * Mathf.Pow(along / Mathf.Max(0.16f, feature.Width * 1.45f), 2f));
                    return height + ridge * feature.Strength * interior * 0.90f;
                }
                case FeatureType.ShallowCrease:
                {
                    float crease = Mathf.Exp(-0.5f * Mathf.Pow(lineDistance / Mathf.Max(0.08f, feature.Width * 0.65f), 2f)) *
                        Mathf.Exp(-0.5f * Mathf.Pow(along / Mathf.Max(0.18f, feature.Width * 1.55f), 2f));
                    return height - crease * feature.Strength * interior * 0.92f;
                }
                case FeatureType.LocalDepression:
                {
                    float depression = Mathf.Exp(-0.5f * Mathf.Pow(radial / Mathf.Max(0.08f, feature.Width), 2f));
                    return height - depression * feature.Strength * interior;
                }
                case FeatureType.SecondaryLobe:
                {
                    float lobe = Mathf.Exp(-0.5f * Mathf.Pow(radial / Mathf.Max(0.10f, feature.Width), 2f));
                    return height + lobe * feature.Strength * interior * Mathf.Lerp(0.35f, 0.85f, inside);
                }
                case FeatureType.RoundedNotch:
                {
                    float notch = Mathf.Exp(-0.5f * Mathf.Pow(radial / Mathf.Max(0.08f, feature.Width), 2f));
                    float edgeBias = 1f - SmoothStep(0.28f, 0.68f, inside);
                    return height - notch * feature.Strength * edgeBias * Mathf.Lerp(0.30f, 1f, localRegion);
                }
                default:
                {
                    float cut = SmoothStep(-feature.Width, feature.Width, along - feature.Secondary);
                    float edgeBias = 1f - SmoothStep(0.18f, 0.58f, inside);
                    return height * (1f - feature.Strength * 0.40f * cut * edgeBias * Mathf.Lerp(0.35f, 1f, localRegion));
                }
            }
        }

        private static float LocalStoneNoise(
            Motif motif,
            float x,
            float y)
        {
            float value =
                Mathf.Sin(x * 4.3f + y * 2.1f + motif.Phase2) * 0.50f +
                Mathf.Sin(x * 7.1f - y * 5.4f + motif.Phase3) * 0.30f +
                Mathf.Sin((x + y) * 9.0f + motif.Phase4) * 0.20f;
            return Mathf.Clamp01(value * 0.5f + 0.5f);
        }

        private static float ResolveContactBreakup(
            Motif motif,
            float angle,
            float x,
            float y)
        {
            float angular =
                Mathf.Sin(angle * 3f + motif.Phase2) * 0.42f +
                Mathf.Sin(angle * 5f + motif.Phase3) * 0.33f +
                Mathf.Sin(angle * 7f + motif.Phase4) * 0.25f;
            float directional = 0.5f + 0.5f * angular;
            float local = 0.35f + 0.65f * LocalStoneNoise(motif, x * 1.3f, y * 1.3f);
            return Mathf.Pow(Mathf.Clamp01(directional * local), 1.45f);
        }

        private static float ResolveBurialFactor(
            Motif motif,
            float x,
            float y)
        {
            float direction = DirectionalCoordinate(
                x,
                y,
                motif.EdgeAngle + 0.47f);
            switch (motif.Burial)
            {
                case BurialProfile.HalfBuried:
                    return 1.15f;
                case BurialProfile.OneSideBuried:
                    return Mathf.Lerp(
                        1.38f,
                        0.72f,
                        SmoothStep(-0.55f, 0.55f, direction));
                case BurialProfile.SlabSet:
                    return 1.30f;
                case BurialProfile.ShallowSink:
                    return 1.06f;
                default:
                    return 0.76f;
            }
        }

        private static float ResolveFinalEdgeShoulderWidth(Motif motif)
        {
            switch (motif.Edge)
            {
                case EdgeProfile.SoftEven:
                    return 0.24f;
                case EdgeProfile.OneSideBuried:
                    return 0.20f;
                case EdgeProfile.ShoulderDrop:
                    return 0.10f;
                case EdgeProfile.FlattenedSide:
                    return 0.12f;
                default:
                    return motif.Family == MotifFamily.LowSlab
                        ? 0.10f
                        : 0.15f;
            }
        }

        private static float ResolveDirectionalContactWeight(
            Motif motif,
            float x,
            float y)
        {
            float direction = DirectionalCoordinate(
                x,
                y,
                motif.EdgeAngle + 0.47f);
            float buriedSide = 1f - SmoothStep(
                -0.30f,
                0.42f,
                direction);
            switch (motif.Burial)
            {
                case BurialProfile.HalfBuried:
                    return Mathf.Lerp(0.10f, 1f, buriedSide);
                case BurialProfile.OneSideBuried:
                    return Mathf.Lerp(0.00f, 1f, buriedSide);
                case BurialProfile.SlabSet:
                    return Mathf.Lerp(0.08f, 0.82f, buriedSide);
                case BurialProfile.ShallowSink:
                    return Mathf.Lerp(0.04f, 0.52f, buriedSide);
                default:
                    return Mathf.Lerp(0.02f, 0.42f, buriedSide);
            }
        }

        private static float DirectionalCoordinate(
            float x,
            float y,
            float angle)
        {
            return x * Mathf.Cos(angle) +
                y * Mathf.Sin(angle);
        }

        private static float EvaluateSuperellipseDistance(
            float x,
            float y,
            float exponent)
        {
            float power = Mathf.Max(
                MinimumAllowedExponent,
                exponent);
            float sum = Mathf.Pow(Mathf.Abs(x), power) +
                Mathf.Pow(Mathf.Abs(y), power);
            return Mathf.Pow(
                Mathf.Max(0f, sum),
                1f / power);
        }

        private static float EvaluateRadialScale(
            Motif motif,
            float angle)
        {
            float radial = 1f +
                motif.Harmonic2 * Mathf.Cos(angle * 2f + motif.Phase2) +
                motif.Harmonic3 * Mathf.Cos(angle * 3f + motif.Phase3) +
                motif.Harmonic4 * Mathf.Cos(angle * 4f + motif.Phase4) +
                0.018f * Mathf.Cos(angle * 5f + motif.Phase2 * 1.37f) +
                0.012f * Mathf.Sin(angle * 7f + motif.Phase3 * 0.83f);
            if (motif.FlattenDepth > 0.0001f)
            {
                float delta = WrappedAngle(angle - motif.FlattenAngle);
                float normalized = delta / Mathf.Max(0.1f, motif.FlattenWidth);
                radial -= motif.FlattenDepth * Mathf.Exp(-0.5f * normalized * normalized);
            }

            if (motif.Features != null)
            {
                for (int index = 0; index < motif.Features.Length; index++)
                {
                    Feature feature = motif.Features[index];
                    float delta = WrappedAngle(angle - feature.Angle);
                    float spread = Mathf.Max(0.16f, feature.Width * 1.6f);
                    float influence = Mathf.Exp(-0.5f * Mathf.Pow(delta / spread, 2f));
                    if (feature.Type == FeatureType.RoundedNotch ||
                        feature.Type == FeatureType.BuriedSideCut)
                    {
                        radial -= influence * feature.Strength * 0.18f;
                    }
                    else if (feature.Type == FeatureType.SecondaryLobe)
                    {
                        radial += influence * feature.Strength * 0.10f;
                    }
                    else if (feature.Type == FeatureType.PlanarFacet)
                    {
                        radial -= influence * feature.Strength * 0.06f;
                    }
                }
            }

            return Mathf.Clamp(radial, 0.79f, 1.18f);
        }

        private static float WrappedAngle(float value)
        {
            while (value > Mathf.PI)
            {
                value -= Mathf.PI * 2f;
            }

            while (value < -Mathf.PI)
            {
                value += Mathf.PI * 2f;
            }

            return value;
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
                (value - edge0) /
                (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }

        private static void CommitStamp(
            CandidateResult result,
            IReadOnlyList<StampPixel> stamp,
            Placement placement)
        {
            Color32 familyColor = ResolveFamilyColor(
                placement.Family);
            for (int index = 0; index < stamp.Count; index++)
            {
                StampPixel pixel = stamp[index];
                int destination = pixel.DestinationIndex;
                result.Height[destination] -=
                    pixel.Depression;
                result.Cavity[destination] = Mathf.Max(
                    result.Cavity[destination],
                    pixel.Cavity);
                if (pixel.Mask >
                    result.StoneMask[destination])
                {
                    result.StoneMask[destination] =
                        pixel.Mask;
                    result.StoneVariation[destination] =
                        pixel.Variation;
                }

                float desiredHeight =
                    result.SubstrateHeight[destination] +
                    pixel.RaisedHeight -
                    pixel.Depression;
                result.Height[destination] = Mathf.Max(
                    result.Height[destination],
                    desiredHeight);
                result.Roughness[destination] = Mathf.Lerp(
                    result.Roughness[destination],
                    pixel.Roughness,
                    pixel.Mask);
                if (pixel.Mask > 0.25f)
                {
                    result.PlacementDebug[destination] =
                        familyColor;
                }
            }
        }

        private static SubstrateData BuildPeriodicSubstrate(
            Definition definition)
        {
            int pixelCount = Resolution * Resolution;
            SubstrateData substrate = new SubstrateData
            {
                Height = new float[pixelCount],
                Variation = new float[pixelCount],
                Cavity = new float[pixelCount],
                Roughness = new float[pixelCount]
            };
            float twoPi = Mathf.PI * 2f;
            for (int y = 0; y < Resolution; y++)
            {
                float v = y / (float)Resolution;
                for (int x = 0; x < Resolution; x++)
                {
                    float u = x / (float)Resolution;
                    float macro =
                        Mathf.Sin(twoPi *
                            (u + v * 2f + 0.17f)) * 0.50f +
                        Mathf.Sin(twoPi *
                            (u * 2f - v + 0.43f)) * 0.31f +
                        Mathf.Sin(twoPi *
                            (u * 3f + v * 2f + 0.71f)) *
                            0.19f;
                    float sediment =
                        Mathf.Sin(twoPi *
                            (u * 5f + v * 3f + 0.29f)) *
                            0.52f +
                        Mathf.Sin(twoPi *
                            (u * 7f - v * 4f + 0.58f)) *
                            0.31f +
                        Mathf.Sin(twoPi *
                            (u * 11f + v * 9f + 0.83f)) *
                            0.17f;
                    float ripple = Mathf.Sin(
                        twoPi *
                        (u *
                            (definition.SubstrateVariant + 2f) +
                         v * 0.75f + 0.21f));
                    float macroNormalized = Mathf.Clamp01(
                        macro * 0.42f + 0.50f);
                    float sedimentNormalized = Mathf.Clamp01(
                        sediment * 0.45f + 0.50f);
                    float variantStrength =
                        definition.SubstrateVariant == 0
                            ? 0.0055f
                            : definition.SubstrateVariant == 1
                                ? 0.0075f
                                : 0.0095f;
                    float height = 0.40f +
                        (macroNormalized - 0.5f) * 0.025f +
                        (sedimentNormalized - 0.5f) *
                            variantStrength +
                        ripple *
                            (definition.SubstrateVariant == 2
                                ? 0.0032f
                                : 0.0015f);
                    int index = y * Resolution + x;
                    substrate.Height[index] = height;
                    substrate.Variation[index] =
                        Mathf.Clamp01(
                            macroNormalized * 0.70f +
                            sedimentNormalized * 0.30f);
                    substrate.Roughness[index] = Mathf.Lerp(
                        0.79f,
                        0.86f,
                        sedimentNormalized);
                }
            }

            DeterministicRandom random =
                new DeterministicRandom(
                    (uint)(8300 +
                    definition.SubstrateVariant * 107));
            int pitCount = 9 +
                definition.SubstrateVariant * 3;
            for (int pit = 0; pit < pitCount; pit++)
            {
                float centerX =
                    random.NextFloat() * Resolution;
                float centerY =
                    random.NextFloat() * Resolution;
                float radius =
                    random.Range(5f, 12f);
                float depth =
                    random.Range(0.0020f, 0.0055f) *
                    (1f +
                    definition.SubstrateVariant * 0.14f);
                ApplyPeriodicPit(
                    substrate,
                    centerX,
                    centerY,
                    radius,
                    depth);
            }

            return substrate;
        }

        private static void ApplyPeriodicPit(
            SubstrateData substrate,
            float centerX,
            float centerY,
            float radius,
            float depth)
        {
            int extent = Mathf.CeilToInt(radius * 2f);
            for (int offsetY = -extent;
                 offsetY <= extent;
                 offsetY++)
            {
                for (int offsetX = -extent;
                     offsetX <= extent;
                     offsetX++)
                {
                    float distance = Mathf.Sqrt(
                        offsetX * offsetX +
                        offsetY * offsetY);
                    if (distance > radius)
                    {
                        continue;
                    }

                    float t = 1f - distance / radius;
                    float influence =
                        t * t * (3f - 2f * t);
                    int x = Wrap(
                        Mathf.RoundToInt(centerX) +
                        offsetX,
                        Resolution);
                    int y = Wrap(
                        Mathf.RoundToInt(centerY) +
                        offsetY,
                        Resolution);
                    int index = y * Resolution + x;
                    substrate.Height[index] -=
                        influence * depth;
                    substrate.Cavity[index] =
                        Mathf.Max(
                            substrate.Cavity[index],
                            influence * 0.065f);
                    substrate.Roughness[index] =
                        Mathf.Lerp(
                            substrate.Roughness[index],
                            0.87f,
                            influence * 0.42f);
                }
            }
        }

        private static MacroRegion[] BuildMacroRegions(
            Definition definition)
        {
            int count = Mathf.Max(1, definition.MacroRegionCount);
            MacroRegion[] regions = new MacroRegion[count];
            DeterministicRandom random = new DeterministicRandom(
                definition.Seed ^ 0x9E3779B9u);
            for (int index = 0; index < count; index++)
            {
                float centerX = 0f;
                float centerY = 0f;
                float bestSeparation = -1f;
                int candidateCount = index == 0 ? 1 : 18;
                for (int candidate = 0;
                     candidate < candidateCount;
                     candidate++)
                {
                    float candidateX = random.NextFloat() * Resolution;
                    float candidateY = random.NextFloat() * Resolution;
                    float minimumSeparation = float.PositiveInfinity;
                    for (int previous = 0;
                         previous < index;
                         previous++)
                    {
                        float deltaX = WrappedSignedDelta(
                            candidateX - regions[previous].CenterX);
                        float deltaY = WrappedSignedDelta(
                            candidateY - regions[previous].CenterY);
                        minimumSeparation = Mathf.Min(
                            minimumSeparation,
                            deltaX * deltaX + deltaY * deltaY);
                    }

                    if (minimumSeparation > bestSeparation)
                    {
                        bestSeparation = minimumSeparation;
                        centerX = candidateX;
                        centerY = candidateY;
                    }
                }

                float baseRadius = random.Range(50f, 82f) +
                    definition.SubstrateVariant * 4f;
                float aspect = random.Range(0.72f, 1.34f);
                float aspectRoot = Mathf.Sqrt(aspect);
                regions[index] = new MacroRegion
                {
                    CenterX = centerX,
                    CenterY = centerY,
                    RadiusX = baseRadius * aspectRoot,
                    RadiusY = baseRadius / aspectRoot,
                    Rotation = random.NextFloat() * Mathf.PI * 2f,
                    Strength = random.Range(0.80f, 1f)
                };
            }

            return regions;
        }

        private static float PeriodicDensity(
            Definition definition,
            IReadOnlyList<MacroRegion> regions,
            int x,
            int y)
        {
            float density = 0f;
            for (int index = 0; index < regions.Count; index++)
            {
                MacroRegion region = regions[index];
                float deltaX = WrappedSignedDelta(x - region.CenterX);
                float deltaY = WrappedSignedDelta(y - region.CenterY);
                float cosine = Mathf.Cos(region.Rotation);
                float sine = Mathf.Sin(region.Rotation);
                float localX =
                    (cosine * deltaX + sine * deltaY) /
                    Mathf.Max(1f, region.RadiusX);
                float localY =
                    (-sine * deltaX + cosine * deltaY) /
                    Mathf.Max(1f, region.RadiusY);
                float distance = Mathf.Sqrt(
                    localX * localX + localY * localY);
                float core = 1f - SmoothStep(0.18f, 0.88f, distance);
                float shoulder = 1f - SmoothStep(0.70f, 1.22f, distance);
                float influence = Mathf.Clamp01(
                    core * 0.78f + shoulder * 0.22f) *
                    region.Strength;
                density = Mathf.Max(density, influence);
            }

            float floor = definition.SubstrateVariant == 0
                ? 0.010f
                : 0.016f;
            return Mathf.Clamp01(Mathf.Max(floor, density));
        }

        private static float WrappedSignedDelta(float value)
        {
            while (value > Resolution * 0.5f)
            {
                value -= Resolution;
            }

            while (value < -Resolution * 0.5f)
            {
                value += Resolution;
            }

            return value;
        }

        private static int CountNewMacroBlocks(
            IReadOnlyList<StampPixel> stamp,
            bool[] occupiedBlocks,
            int[] visitGeneration,
            int generation)
        {
            int count = 0;
            for (int index = 0; index < stamp.Count; index++)
            {
                StampPixel pixel = stamp[index];
                if (pixel.Mask <= 0.25f)
                {
                    continue;
                }

                int block = ResolveMacroBlock(pixel.DestinationIndex);
                if (occupiedBlocks[block] ||
                    visitGeneration[block] == generation)
                {
                    continue;
                }

                visitGeneration[block] = generation;
                count++;
            }

            return count;
        }

        private static void MarkOccupiedMacroBlocks(
            IReadOnlyList<StampPixel> stamp,
            bool[] occupiedBlocks)
        {
            for (int index = 0; index < stamp.Count; index++)
            {
                StampPixel pixel = stamp[index];
                if (pixel.Mask <= 0.25f)
                {
                    continue;
                }

                occupiedBlocks[ResolveMacroBlock(
                    pixel.DestinationIndex)] = true;
            }
        }

        private static int ResolveMacroBlock(int destinationIndex)
        {
            int x = destinationIndex % Resolution;
            int y = destinationIndex / Resolution;
            return y / MacroBlockSize * MacroBlocksPerAxis +
                x / MacroBlockSize;
        }

        private static Color32[] BuildSubstrateDebug(
            float[] variation)
        {
            Color32[] pixels =
                new Color32[variation.Length];
            for (int index = 0;
                 index < pixels.Length;
                 index++)
            {
                byte value = (byte)Mathf.RoundToInt(
                    Mathf.Lerp(
                        38f,
                        92f,
                        variation[index]));
                pixels[index] =
                    new Color32(
                        value,
                        value,
                        value,
                        255);
            }

            return pixels;
        }

        private static Color32 ResolveFamilyColor(
            MotifFamily family)
        {
            switch (family)
            {
                case MotifFamily.RoundedPebble:
                    return new Color32(54, 184, 255, 255);
                case MotifFamily.BroadOval:
                    return new Color32(72, 232, 116, 255);
                case MotifFamily.LowSlab:
                    return new Color32(255, 190, 46, 255);
                case MotifFamily.SoftAngular:
                    return new Color32(206, 91, 255, 255);
                default:
                    return new Color32(255, 104, 94, 255);
            }
        }

        private static Color32 ResolveMotifPreviewColor(
            MotifFamily family)
        {
            switch (family)
            {
                case MotifFamily.RoundedPebble:
                    return new Color32(150, 149, 139, 255);
                case MotifFamily.BroadOval:
                    return new Color32(153, 150, 142, 255);
                case MotifFamily.LowSlab:
                    return new Color32(162, 157, 146, 255);
                case MotifFamily.SoftAngular:
                    return new Color32(145, 144, 137, 255);
                default:
                    return new Color32(157, 151, 140, 255);
            }
        }

        private static Color32[] BuildMotifCatalogPreview(
            IReadOnlyList<Motif> motifs)
        {
            return BuildMotifCatalog(
                motifs,
                false);
        }

        private static Color32[] BuildMotifNormalCatalogPreview(
            IReadOnlyList<Motif> motifs)
        {
            return BuildMotifCatalog(
                motifs,
                true);
        }

        private static Color32[] BuildMotifCatalog(
            IReadOnlyList<Motif> motifs,
            bool normals)
        {
            Color32[] pixels =
                new Color32[Resolution * Resolution];
            Fill(
                pixels,
                normals
                    ? new Color32(128, 128, 255, 255)
                    : new Color32(13, 13, 13, 255));
            const int columns = 8;
            const int rows = 6;
            int cellWidth = Resolution / columns;
            int cellHeight = Resolution / rows;
            for (int index = 0;
                 index < motifs.Count;
                 index++)
            {
                Motif motif = motifs[index];
                int column = index % columns;
                int row = index / columns;
                int centerX =
                    column * cellWidth +
                    cellWidth / 2;
                int centerY =
                    row * cellHeight +
                    cellHeight / 2;
                float radius =
                    Mathf.Min(cellWidth, cellHeight) *
                    0.34f;
                float aspectRoot =
                    Mathf.Sqrt(motif.Aspect);
                float radiusX =
                    radius * aspectRoot;
                float radiusY =
                    radius / aspectRoot;
                DrawCatalogMotif(
                    pixels,
                    motif,
                    centerX,
                    centerY,
                    radiusX,
                    radiusY,
                    normals);
            }

            return pixels;
        }

        private static void DrawCatalogMotif(
            Color32[] pixels,
            Motif motif,
            int centerX,
            int centerY,
            float radiusX,
            float radiusY,
            bool normalOutput)
        {
            int extent = Mathf.CeilToInt(
                Mathf.Max(radiusX, radiusY) + 2f);
            float minimumRadius =
                Mathf.Min(radiusX, radiusY);
            float antialias =
                1.20f / Mathf.Max(
                    4f,
                    minimumRadius);
            Color32 familyColor =
                ResolveMotifPreviewColor(motif.Family);
            Vector3 lightDirection =
                new Vector3(
                    -0.58f,
                    0.66f,
                    0.46f).normalized;
            for (int offsetY = -extent;
                 offsetY <= extent;
                 offsetY++)
            {
                for (int offsetX = -extent;
                     offsetX <= extent;
                     offsetX++)
                {
                    float localX =
                        offsetX / radiusX;
                    float localY =
                        offsetY / radiusY;
                    float angle =
                        Mathf.Atan2(
                            localY,
                            localX);
                    float signed =
                        EvaluateRadialScale(
                            motif,
                            angle) -
                        EvaluateSuperellipseDistance(
                            localX,
                            localY,
                            motif.Exponent);
                    float mask = SmoothStep(
                        -antialias,
                        antialias,
                        signed);
                    if (mask <= 0.001f)
                    {
                        continue;
                    }

                    const float epsilon = 0.022f;
                    float reference;
                    float height = EvaluateStoneHeight(
                        motif,
                        localX,
                        localY,
                        out reference);
                    float unused;
                    float heightX0 =
                        EvaluateStoneHeight(
                            motif,
                            localX - epsilon,
                            localY,
                            out unused);
                    float heightX1 =
                        EvaluateStoneHeight(
                            motif,
                            localX + epsilon,
                            localY,
                            out unused);
                    float heightY0 =
                        EvaluateStoneHeight(
                            motif,
                            localX,
                            localY - epsilon,
                            out unused);
                    float heightY1 =
                        EvaluateStoneHeight(
                            motif,
                            localX,
                            localY + epsilon,
                            out unused);
                    Vector3 normal = new Vector3(
                        -(heightX1 - heightX0) /
                            (epsilon * 2f) * 1.05f,
                        -(heightY1 - heightY0) /
                            (epsilon * 2f) * 1.05f,
                        1f).normalized;
                    Color color;
                    if (normalOutput)
                    {
                        color = new Color(
                            normal.x * 0.5f + 0.5f,
                            normal.y * 0.5f + 0.5f,
                            normal.z * 0.5f + 0.5f,
                            1f);
                    }
                    else
                    {
                        float lighting = Mathf.Lerp(
                            0.42f,
                            1.05f,
                            Mathf.Clamp01(
                                Vector3.Dot(
                                    normal,
                                    lightDirection) *
                                0.5f + 0.5f));
                        float grain = 0.80f + LocalStoneNoise(motif, localX, localY) * 0.20f;
                        float value = Mathf.Clamp01(height * 0.66f + lighting * 0.34f);
                        color = Color.Lerp(
                            new Color(
                                familyColor.r / 255f * 0.46f,
                                familyColor.g / 255f * 0.46f,
                                familyColor.b / 255f * 0.46f,
                                1f),
                            new Color(
                                familyColor.r / 255f,
                                familyColor.g / 255f,
                                familyColor.b / 255f,
                                1f),
                            value * grain);
                    }

                    int x = centerX + offsetX;
                    int y = centerY + offsetY;
                    if (x < 0 ||
                        x >= Resolution ||
                        y < 0 ||
                        y >= Resolution)
                    {
                        continue;
                    }

                    int destination =
                        y * Resolution + x;
                    Color existing =
                        pixels[destination];
                    pixels[destination] =
                        (Color32)Color.Lerp(
                            existing,
                            color,
                            mask);
                }
            }
        }

        private static void MeasureFinalPlacedStructure(
            CandidateResult result)
        {
            int pixelCount = Resolution * Resolution;
            float[] normalizedStoneHeight = new float[pixelCount];
            float maximum = 0f;
            for (int index = 0; index < pixelCount; index++)
            {
                float stoneHeight = Mathf.Max(
                    0f,
                    result.Height[index] -
                    result.SubstrateHeight[index]);
                normalizedStoneHeight[index] = stoneHeight;
                if (result.StoneMask[index] > 0.50f)
                {
                    maximum = Mathf.Max(maximum, stoneHeight);
                }
            }

            float inverseMaximum = 1f / Mathf.Max(0.000001f, maximum);
            for (int index = 0; index < pixelCount; index++)
            {
                normalizedStoneHeight[index] *= inverseMaximum;
            }

            Color32[] debug = new Color32[pixelCount];
            Fill(debug, new Color32(12, 12, 12, 255));
            double residualSquaredSum = 0.0;
            int highCurvatureCount = 0;
            int sampleCount = 0;
            for (int y = 0; y < Resolution; y++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = y * Resolution + x;
                    if (result.StoneMask[index] < 0.72f)
                    {
                        continue;
                    }

                    int left = y * Resolution + Wrap(x - 1, Resolution);
                    int right = y * Resolution + Wrap(x + 1, Resolution);
                    int down = Wrap(y - 1, Resolution) * Resolution + x;
                    int up = Wrap(y + 1, Resolution) * Resolution + x;
                    if (result.StoneMask[left] < 0.55f ||
                        result.StoneMask[right] < 0.55f ||
                        result.StoneMask[down] < 0.55f ||
                        result.StoneMask[up] < 0.55f)
                    {
                        continue;
                    }

                    float smooth = 0f;
                    int smoothSamples = 0;
                    for (int offsetY = -2; offsetY <= 2; offsetY++)
                    {
                        int sampleY = Wrap(y + offsetY, Resolution);
                        for (int offsetX = -2; offsetX <= 2; offsetX++)
                        {
                            int sampleX = Wrap(x + offsetX, Resolution);
                            int sampleIndex =
                                sampleY * Resolution + sampleX;
                            if (result.StoneMask[sampleIndex] < 0.40f)
                            {
                                continue;
                            }

                            smooth += normalizedStoneHeight[sampleIndex];
                            smoothSamples++;
                        }
                    }

                    if (smoothSamples <= 0)
                    {
                        continue;
                    }

                    smooth /= smoothSamples;
                    float height = normalizedStoneHeight[index];
                    float residual = Mathf.Abs(height - smooth);
                    float laplacian = Mathf.Abs(
                        normalizedStoneHeight[left] +
                        normalizedStoneHeight[right] +
                        normalizedStoneHeight[down] +
                        normalizedStoneHeight[up] -
                        height * 4f);
                    residualSquaredSum += residual * residual;
                    bool highCurvature = residual > 0.018f ||
                        laplacian > 0.028f;
                    if (highCurvature)
                    {
                        highCurvatureCount++;
                    }

                    byte residualByte = (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(residual * 12f) * 255f);
                    byte curvatureByte = (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(laplacian * 9f) * 255f);
                    byte heightByte = (byte)Mathf.RoundToInt(
                        Mathf.Clamp01(height) * 180f + 32f);
                    debug[index] = new Color32(
                        curvatureByte,
                        residualByte,
                        heightByte,
                        255);
                    sampleCount++;
                }
            }

            result.FinalPlacedFeatureResidualRms = sampleCount > 0
                ? Mathf.Sqrt((float)(residualSquaredSum / sampleCount))
                : 0f;
            result.FinalPlacedHighCurvatureFraction = sampleCount > 0
                ? highCurvatureCount / (float)sampleCount
                : 0f;
            result.FinalStructureDebug = debug;
        }

        private static Color32[] BuildNormals(
            float[] height,
            int width,
            int heightPixels,
            float strength)
        {
            Color32[] normals =
                new Color32[height.Length];
            for (int y = 0;
                 y < heightPixels;
                 y++)
            {
                int previousY =
                    Wrap(y - 1, heightPixels);
                int nextY =
                    Wrap(y + 1, heightPixels);
                for (int x = 0;
                     x < width;
                     x++)
                {
                    int previousX =
                        Wrap(x - 1, width);
                    int nextX =
                        Wrap(x + 1, width);
                    float deltaX =
                        height[y * width + nextX] -
                        height[y * width + previousX];
                    float deltaY =
                        height[nextY * width + x] -
                        height[previousY * width + x];
                    Vector3 normal = new Vector3(
                        -deltaX * strength,
                        -deltaY * strength,
                        1f).normalized;
                    normals[y * width + x] =
                        new Color32(
                            EncodeSigned(normal.x),
                            EncodeSigned(normal.y),
                            EncodeSigned(normal.z),
                            255);
                }
            }

            return normals;
        }

        private static byte EncodeSigned(float value)
        {
            return (byte)Mathf.RoundToInt(
                Mathf.Clamp01(
                    value * 0.5f + 0.5f) *
                255f);
        }

        private static Color32[] BuildColorPreview(
            CandidateResult result)
        {
            Color32[] pixels =
                new Color32[result.Height.Length];
            Vector3 lightDirection =
                new Vector3(
                    -0.62f,
                    0.66f,
                    0.42f).normalized;
            for (int index = 0;
                 index < pixels.Length;
                 index++)
            {
                int x = index % Resolution;
                int y = index / Resolution;
                float mask = Mathf.Clamp01(result.StoneMask[index]);
                float rise = Mathf.Clamp01(
                    (result.Height[index] -
                     result.SubstrateHeight[index] +
                     0.004f) / 0.16f);
                float stoneVariation = result.StoneVariation[index];
                float substrateNoise = Mathf.Clamp01(
                    result.SubstrateVariation[index] * 0.72f +
                    (Mathf.Sin(x * 0.031f + y * 0.017f + 0.6f) * 0.5f + 0.5f) * 0.28f);
                Color substrate = Color.Lerp(
                    result.Definition.SubstrateDark,
                    result.Definition.SubstrateLight,
                    substrateNoise);
                float grain = Mathf.Clamp01(
                    0.5f +
                    Mathf.Sin(x * 0.073f + y * 0.041f + stoneVariation * 5.3f) * 0.20f +
                    Mathf.Sin(x * 0.129f - y * 0.091f + stoneVariation * 9.7f) * 0.14f +
                    Mathf.Sin((x + y) * 0.053f + 1.3f) * 0.10f);
                float stoneMix = Mathf.Clamp01(0.22f + rise * 0.42f + grain * 0.26f + stoneVariation * 0.10f);
                Color stone = Color.Lerp(
                    result.Definition.StoneDark,
                    result.Definition.StoneLight,
                    stoneMix);
                Vector3 normal = DecodeNormal(result.Normals[index]);
                float lightResponse = SmoothStep(
                    0.18f,
                    0.82f,
                    Mathf.Clamp01(Vector3.Dot(normal, lightDirection) * 0.5f + 0.5f));
                float lighting = Mathf.Lerp(0.60f, 1.20f, lightResponse);
                float cavityDarken = Mathf.Clamp01(result.Cavity[index] * 0.10f);
                float edgeWear = mask * (1f - rise) * (0.85f - grain * 0.25f);
                stone = Color.Lerp(stone, result.Definition.StoneLight * 1.02f, Mathf.Clamp01(edgeWear * 0.18f));
                Color color = Color.Lerp(substrate, stone, mask);
                color *= lighting;
                color = Color.Lerp(color, result.Definition.CavityColor, cavityDarken);
                pixels[index] = (Color32)color;
            }

            return pixels;
        }

        private static Vector3 DecodeNormal(
            Color32 encoded)
        {
            Vector3 normal = new Vector3(
                encoded.r / 255f * 2f - 1f,
                encoded.g / 255f * 2f - 1f,
                encoded.b / 255f * 2f - 1f);
            return normal.sqrMagnitude >
                0.000001f
                ? normal.normalized
                : Vector3.forward;
        }

        private static float CalculateQuietBlockFraction(
            float[] mask,
            int blockSize)
        {
            int blocksX = Resolution / blockSize;
            int blocksY = Resolution / blockSize;
            int quiet = 0;
            int total = blocksX * blocksY;
            for (int blockY = 0;
                 blockY < blocksY;
                 blockY++)
            {
                for (int blockX = 0;
                     blockX < blocksX;
                     blockX++)
                {
                    bool occupied = false;
                    for (int y = 0;
                         y < blockSize && !occupied;
                         y++)
                    {
                        int sampleY =
                            blockY * blockSize + y;
                        for (int x = 0;
                             x < blockSize;
                             x++)
                        {
                            int sampleX =
                                blockX * blockSize + x;
                            if (mask[
                                sampleY * Resolution +
                                sampleX] > 0.25f)
                            {
                                occupied = true;
                                break;
                            }
                        }
                    }

                    if (!occupied)
                    {
                        quiet++;
                    }
                }
            }

            return total > 0
                ? quiet / (float)total
                : 0f;
        }

        private static int CalculateLargestWrappedComponent(
            float[] mask)
        {
            bool[] visited =
                new bool[mask.Length];
            Queue<int> queue =
                new Queue<int>();
            int largest = 0;
            for (int start = 0;
                 start < mask.Length;
                 start++)
            {
                if (visited[start] ||
                    mask[start] <= 0.5f)
                {
                    continue;
                }

                visited[start] = true;
                queue.Enqueue(start);
                int count = 0;
                while (queue.Count > 0)
                {
                    int index = queue.Dequeue();
                    count++;
                    int x = index % Resolution;
                    int y = index / Resolution;
                    VisitNeighbour(
                        Wrap(x - 1, Resolution),
                        y,
                        mask,
                        visited,
                        queue);
                    VisitNeighbour(
                        Wrap(x + 1, Resolution),
                        y,
                        mask,
                        visited,
                        queue);
                    VisitNeighbour(
                        x,
                        Wrap(y - 1, Resolution),
                        mask,
                        visited,
                        queue);
                    VisitNeighbour(
                        x,
                        Wrap(y + 1, Resolution),
                        mask,
                        visited,
                        queue);
                }

                largest = Mathf.Max(
                    largest,
                    count);
            }

            return largest;
        }

        private static void VisitNeighbour(
            int x,
            int y,
            float[] mask,
            bool[] visited,
            Queue<int> queue)
        {
            int index = y * Resolution + x;
            if (visited[index] ||
                mask[index] <= 0.5f)
            {
                return;
            }

            visited[index] = true;
            queue.Enqueue(index);
        }

        private static SeamMetrics MeasureSeams(
            float[] values)
        {
            List<float> horizontal =
                new List<float>(Resolution);
            List<float> vertical =
                new List<float>(Resolution);
            List<float> horizontalLocal =
                new List<float>(Resolution);
            List<float> verticalLocal =
                new List<float>(Resolution);
            for (int y = 0; y < Resolution; y++)
            {
                float boundary = Mathf.Abs(
                    values[y * Resolution] -
                    values[y * Resolution +
                    Resolution - 1]);
                float local = (
                    Mathf.Abs(
                        values[y * Resolution + 1] -
                        values[y * Resolution]) +
                    Mathf.Abs(
                        values[y * Resolution +
                        Resolution - 1] -
                        values[y * Resolution +
                        Resolution - 2])) *
                    0.5f;
                horizontal.Add(boundary);
                horizontalLocal.Add(
                    Mathf.Max(0f, boundary - local));
            }

            for (int x = 0; x < Resolution; x++)
            {
                float boundary = Mathf.Abs(
                    values[x] -
                    values[
                        (Resolution - 1) *
                        Resolution + x]);
                float local = (
                    Mathf.Abs(
                        values[Resolution + x] -
                        values[x]) +
                    Mathf.Abs(
                        values[
                            (Resolution - 1) *
                            Resolution + x] -
                        values[
                            (Resolution - 2) *
                            Resolution + x])) *
                    0.5f;
                vertical.Add(boundary);
                verticalLocal.Add(
                    Mathf.Max(0f, boundary - local));
            }

            float[] horizontalArray =
                horizontal.ToArray();
            float[] verticalArray =
                vertical.ToArray();
            Array.Sort(horizontalArray);
            Array.Sort(verticalArray);
            return new SeamMetrics
            {
                HorizontalMean =
                    Mean(horizontalArray),
                HorizontalP95 =
                    PercentileSorted(
                        horizontalArray,
                        0.95f),
                HorizontalLocalExcessMean =
                    Mean(horizontalLocal.ToArray()),
                VerticalMean =
                    Mean(verticalArray),
                VerticalP95 =
                    PercentileSorted(
                        verticalArray,
                        0.95f),
                VerticalLocalExcessMean =
                    Mean(verticalLocal.ToArray())
            };
        }

        internal static bool SeamMetricsPass(
            SeamMetrics metrics)
        {
            return metrics != null &&
                metrics.HorizontalMean <=
                    SeamMeanThreshold &&
                metrics.VerticalMean <=
                    SeamMeanThreshold &&
                metrics.HorizontalP95 <=
                    SeamP95Threshold &&
                metrics.VerticalP95 <=
                    SeamP95Threshold &&
                metrics.HorizontalLocalExcessMean <=
                    SeamLocalExcessMeanThreshold &&
                metrics.VerticalLocalExcessMean <=
                    SeamLocalExcessMeanThreshold;
        }

        private static float Mean(float[] values)
        {
            if (values.Length == 0)
            {
                return 0f;
            }

            double total = 0.0;
            for (int index = 0;
                 index < values.Length;
                 index++)
            {
                total += values[index];
            }

            return (float)(total / values.Length);
        }

        private static float PercentileSorted(
            float[] values,
            float percentile)
        {
            if (values.Length == 0)
            {
                return 0f;
            }

            int index = Mathf.Clamp(
                Mathf.RoundToInt(
                    percentile *
                    (values.Length - 1)),
                0,
                values.Length - 1);
            return values[index];
        }

        private static void CalculateMipOccupancy(
            float[] mask,
            float[] output)
        {
            float[] current =
                (float[])mask.Clone();
            int width = Resolution;
            int height = Resolution;
            for (int mip = 0;
                 mip < output.Length;
                 mip++)
            {
                int occupied = 0;
                for (int index = 0;
                     index < current.Length;
                     index++)
                {
                    if (current[index] > 0.25f)
                    {
                        occupied++;
                    }
                }

                output[mip] =
                    current.Length > 0
                        ? occupied /
                            (float)current.Length
                        : 0f;
                if (mip < output.Length - 1)
                {
                    current = BuildWrappedMip(
                        current,
                        width,
                        height,
                        out width,
                        out height);
                }
            }
        }

        private static string CalculateMotifCatalogFingerprint(
            IReadOnlyList<Motif> motifs)
        {
            StringBuilder builder =
                new StringBuilder(16384);
            builder.Append("v=");
            builder.Append(AlgorithmVersion);
            builder.Append(';');
            for (int index = 0;
                 index < motifs.Count;
                 index++)
            {
                Motif motif = motifs[index];
                builder.Append(motif.Id);
                builder.Append(':');
                builder.Append((int)motif.Family);
                builder.Append(',');
                builder.Append((int)motif.Crown);
                builder.Append(',');
                builder.Append((int)motif.Edge);
                builder.Append(',');
                builder.Append((int)motif.Burial);
                builder.Append(',');
                AppendFloat(builder, motif.Aspect);
                AppendFloat(builder, motif.Exponent);
                AppendFloat(builder, motif.Harmonic2);
                AppendFloat(builder, motif.Harmonic3);
                AppendFloat(builder, motif.Harmonic4);
                AppendFloat(builder, motif.FlattenDepth);
                AppendFloat(builder, motif.FlattenWidth);
                AppendFloat(builder, motif.CrownShiftX);
                AppendFloat(builder, motif.CrownShiftY);
                AppendFloat(builder, motif.CrownExponent);
                AppendFloat(builder, motif.EdgeStrength);
                AppendFloat(builder, motif.FacetBlend);
                if (motif.FacetPlanes != null)
                {
                    builder.Append("facets=");
                    for (int facetIndex = 0;
                         facetIndex < motif.FacetPlanes.Length;
                         facetIndex++)
                    {
                        FacetPlane facet =
                            motif.FacetPlanes[facetIndex];
                        AppendFloat(builder, facet.Angle);
                        AppendFloat(builder, facet.Offset);
                        AppendFloat(builder, facet.PrimarySlope);
                        AppendFloat(builder, facet.SecondarySlope);
                    }
                }

                AppendFloat(builder, motif.Relief);
                AppendFloat(builder, motif.Embedding);
                AppendFloat(
                    builder,
                    motif.FeatureResidualRms);
                AppendFloat(
                    builder,
                    motif.HighCurvatureFraction);
                builder.Append("features=");
                for (int featureIndex = 0;
                     featureIndex < motif.Features.Length;
                     featureIndex++)
                {
                    Feature feature =
                        motif.Features[featureIndex];
                    builder.Append((int)feature.Type);
                    builder.Append(',');
                    AppendFloat(builder, feature.Angle);
                    AppendFloat(builder, feature.OffsetX);
                    AppendFloat(builder, feature.OffsetY);
                    AppendFloat(builder, feature.Width);
                    AppendFloat(builder, feature.Strength);
                    AppendFloat(builder, feature.Secondary);
                }

                builder.Append('|');
            }

            return CalculateSha256(
                Encoding.UTF8.GetBytes(
                    builder.ToString()));
        }

        private static string CalculateCandidateFingerprint(
            CandidateResult result)
        {
            StringBuilder builder =
                new StringBuilder(12288);
            builder.Append("v=");
            builder.Append(AlgorithmVersion);
            builder.Append(";id=");
            builder.Append(
                result.Definition.StableId);
            builder.Append(";coverage=");
            builder.Append(
                result.ActualCoverage.ToString(
                    "R",
                    CultureInfo.InvariantCulture));
            builder.Append(";macro=");
            builder.Append(result.OccupiedMacroBlocks);
            builder.Append(";quietReject=");
            builder.Append(result.QuietBlockRejected);
            builder.Append(";finalResidual=");
            AppendFloat(builder, result.FinalPlacedFeatureResidualRms);
            builder.Append("finalCurvature=");
            AppendFloat(builder, result.FinalPlacedHighCurvatureFraction);
            builder.Append(';');
            for (int index = 0;
                 index < result.Placements.Count;
                 index++)
            {
                Placement placement =
                    result.Placements[index];
                builder.Append(placement.MotifId);
                builder.Append('@');
                builder.Append(placement.CenterX);
                builder.Append(',');
                builder.Append(placement.CenterY);
                builder.Append(',');
                AppendFloat(
                    builder,
                    placement.RotationRadians);
                AppendFloat(
                    builder,
                    placement.RadiusX);
                AppendFloat(
                    builder,
                    placement.RadiusY);
                AppendFloat(
                    builder,
                    placement.Embedding);
                AppendFloat(
                    builder,
                    placement.Relief);
                builder.Append('|');
            }

            byte[] fields =
                new byte[
                    result.StoneMask.Length * 5];
            for (int index = 0;
                 index < result.StoneMask.Length;
                 index++)
            {
                fields[index * 5] =
                    Quantize(
                        result.StoneMask[index]);
                fields[index * 5 + 1] =
                    Quantize(
                        result.Height[index]);
                fields[index * 5 + 2] =
                    Quantize(
                        result.Cavity[index]);
                fields[index * 5 + 3] =
                    Quantize(
                        result.Roughness[index]);
                fields[index * 5 + 4] =
                    Quantize(
                        result.StoneVariation[index]);
            }

            string metadataHash = CalculateSha256(
                Encoding.UTF8.GetBytes(
                    builder.ToString()));
            string fieldHash =
                CalculateSha256(fields);
            return CalculateSha256(
                Encoding.UTF8.GetBytes(
                    metadataHash + fieldHash));
        }

        private static void AppendFloat(
            StringBuilder builder,
            float value)
        {
            builder.Append(
                value.ToString(
                    "R",
                    CultureInfo.InvariantCulture));
            builder.Append(',');
        }

        private static byte Quantize(float value)
        {
            return (byte)Mathf.RoundToInt(
                Mathf.Clamp01(value) * 255f);
        }

        internal static string CalculateSha256(
            byte[] bytes)
        {
            using (SHA256 sha256 =
                SHA256.Create())
            {
                byte[] hash =
                    sha256.ComputeHash(bytes);
                StringBuilder builder =
                    new StringBuilder(
                        hash.Length * 2);
                for (int index = 0;
                     index < hash.Length;
                     index++)
                {
                    builder.Append(
                        hash[index].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        internal static float[] BuildWrappedMip(
            float[] source,
            int sourceWidth,
            int sourceHeight,
            out int destinationWidth,
            out int destinationHeight)
        {
            destinationWidth =
                Mathf.Max(1, sourceWidth / 2);
            destinationHeight =
                Mathf.Max(1, sourceHeight / 2);
            float[] destination =
                new float[
                    destinationWidth *
                    destinationHeight];
            for (int y = 0;
                 y < destinationHeight;
                 y++)
            {
                for (int x = 0;
                     x < destinationWidth;
                     x++)
                {
                    int sourceX = x * 2;
                    int sourceY = y * 2;
                    float sum = 0f;
                    for (int offsetY = 0;
                         offsetY < 2;
                         offsetY++)
                    {
                        for (int offsetX = 0;
                             offsetX < 2;
                             offsetX++)
                        {
                            int sampleX = Wrap(
                                sourceX + offsetX,
                                sourceWidth);
                            int sampleY = Wrap(
                                sourceY + offsetY,
                                sourceHeight);
                            sum += source[
                                sampleY * sourceWidth +
                                sampleX];
                        }
                    }

                    destination[
                        y * destinationWidth + x] =
                        sum * 0.25f;
                }
            }

            return destination;
        }

        internal static Color32[] BuildWrappedMip(
            Color32[] source,
            int sourceWidth,
            int sourceHeight,
            out int destinationWidth,
            out int destinationHeight)
        {
            destinationWidth =
                Mathf.Max(1, sourceWidth / 2);
            destinationHeight =
                Mathf.Max(1, sourceHeight / 2);
            Color32[] destination =
                new Color32[
                    destinationWidth *
                    destinationHeight];
            for (int y = 0;
                 y < destinationHeight;
                 y++)
            {
                for (int x = 0;
                     x < destinationWidth;
                     x++)
                {
                    int sourceX = x * 2;
                    int sourceY = y * 2;
                    int red = 0;
                    int green = 0;
                    int blue = 0;
                    for (int offsetY = 0;
                         offsetY < 2;
                         offsetY++)
                    {
                        for (int offsetX = 0;
                             offsetX < 2;
                             offsetX++)
                        {
                            int sampleX = Wrap(
                                sourceX + offsetX,
                                sourceWidth);
                            int sampleY = Wrap(
                                sourceY + offsetY,
                                sourceHeight);
                            Color32 sample =
                                source[
                                    sampleY *
                                    sourceWidth +
                                    sampleX];
                            red += sample.r;
                            green += sample.g;
                            blue += sample.b;
                        }
                    }

                    destination[
                        y * destinationWidth + x] =
                        new Color32(
                            (byte)(red / 4),
                            (byte)(green / 4),
                            (byte)(blue / 4),
                            255);
                }
            }

            return destination;
        }

        private static int Wrap(
            int value,
            int size)
        {
            int wrapped = value % size;
            return wrapped < 0
                ? wrapped + size
                : wrapped;
        }

        private static void Fill(
            Color32[] pixels,
            Color32 color)
        {
            for (int index = 0;
                 index < pixels.Length;
                 index++)
            {
                pixels[index] = color;
            }
        }

        private struct DeterministicRandom
        {
            private uint state;

            internal DeterministicRandom(uint seed)
            {
                state = seed != 0u
                    ? seed
                    : 0x6D2B79F5u;
            }

            internal uint NextUInt()
            {
                uint value = state;
                value ^= value << 13;
                value ^= value >> 17;
                value ^= value << 5;
                state = value;
                return value;
            }

            internal float NextFloat()
            {
                return (
                    NextUInt() &
                    0x00FFFFFFu) /
                    16777216f;
            }

            internal float Range(
                float minimum,
                float maximum)
            {
                return Mathf.Lerp(
                    minimum,
                    maximum,
                    NextFloat());
            }

            internal int NextInt(
                int maximumExclusive)
            {
                if (maximumExclusive <= 1)
                {
                    return 0;
                }

                return (int)(
                    NextUInt() %
                    (uint)maximumExclusive);
            }
        }
    }
}
