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
        internal const int AlgorithmVersion = 1;
        internal const int FinalResolution = 1024;
        internal const int WorkResolution = 2048;
        internal const int CandidateCount = 3;
        internal const int ExpectedSourceCount = 18;
        internal const int MinimumSourceDiversity = 12;
        internal const float MaximumSourceShare = 0.12f;

        private const int WorkScale = WorkResolution / FinalResolution;
        private const int QuietBlockSizeFinal = 32;
        private const int QuietBlockSizeWork = QuietBlockSizeFinal * WorkScale;
        private const int QuietBlockAxis = FinalResolution / QuietBlockSizeFinal;
        private const int QuietBlockCount = QuietBlockAxis * QuietBlockAxis;
        private const int MaximumPlacementAttempts = 6000;
        private const int PlacementCandidateSamples = 24;
        private const float BaseRockDiameterWork = 94f;
        private const float MinimumOverlapFraction = 0.018f;
        private const float MinimumSpacingFactor = 0.74f;
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

        private static readonly CandidateDefinition[] CandidateDefinitions =
        {
            new CandidateDefinition(
                "Quiet_Sparse_Riverbed",
                "Quiet Sparse Riverbed",
                91073,
                0.070f,
                0.060f,
                0.080f,
                0.72f),
            new CandidateDefinition(
                "Natural_Sparse_Riverbed",
                "Natural Sparse Riverbed",
                314159,
                0.090f,
                0.080f,
                0.105f,
                0.66f),
            new CandidateDefinition(
                "Dense_Sparse_Riverbed",
                "Dense Sparse Riverbed",
                731927,
                0.110f,
                0.100f,
                0.125f,
                0.58f)
        };

        internal sealed class CandidateDefinition
        {
            internal CandidateDefinition(
                string stableId,
                string displayName,
                int seed,
                float targetCoverage,
                float minimumCoverage,
                float maximumCoverage,
                float minimumQuietFraction)
            {
                StableId = stableId;
                DisplayName = displayName;
                Seed = seed;
                TargetCoverage = targetCoverage;
                MinimumCoverage = minimumCoverage;
                MaximumCoverage = maximumCoverage;
                MinimumQuietFraction = minimumQuietFraction;
            }

            internal string StableId { get; }
            internal string DisplayName { get; }
            internal int Seed { get; }
            internal float TargetCoverage { get; }
            internal float MinimumCoverage { get; }
            internal float MaximumCoverage { get; }
            internal float MinimumQuietFraction { get; }
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

            internal bool Passed =>
                MaskMean <= MaskSeamMeanTolerance &&
                HeightMean <= HeightSeamMeanTolerance &&
                NormalMean <= NormalSeamMeanTolerance &&
                VariationMean <= ScalarSeamMeanTolerance &&
                RootMean <= ScalarSeamMeanTolerance &&
                WearMean <= ScalarSeamMeanTolerance &&
                PreviewMean <= PreviewSeamMeanTolerance;
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
            internal int RejectedForSpacing;
            internal int RejectedForOverlap;
            internal int RejectedForQuietBudget;
            internal int RejectedForCoverage;
            internal int RejectedForLocalRepeat;
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
            internal string Fingerprint;
            internal string Failure;

            internal bool Succeeded => string.IsNullOrEmpty(Failure);
        }

        internal sealed class SuiteResult
        {
            internal readonly List<CandidateResult> Candidates =
                new List<CandidateResult>();
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
            internal readonly HashSet<int> QuietBlocks =
                new HashSet<int>();
            internal float Radius;
        }

        private sealed class MacroField
        {
            internal readonly Vector2[] Centers;
            internal readonly float[] Radii;
            internal readonly float[] Weights;

            internal MacroField(DeterministicRandom random)
            {
                const int count = 7;
                Centers = new Vector2[count];
                Radii = new float[count];
                Weights = new float[count];
                for (int index = 0; index < count; index++)
                {
                    Centers[index] = new Vector2(
                        random.Range(0f, WorkResolution),
                        random.Range(0f, WorkResolution));
                    Radii[index] = random.Range(210f, 430f);
                    Weights[index] = random.Range(0.70f, 1.20f);
                }
            }

            internal float Evaluate(Vector2 point)
            {
                float sum = 0f;
                float normalizer = 0f;
                for (int index = 0; index < Centers.Length; index++)
                {
                    float distance = ToroidalDistance(point, Centers[index]);
                    float normalized = distance /
                        Mathf.Max(1f, Radii[index]);
                    float contribution = Mathf.Exp(
                        -0.5f * normalized * normalized) * Weights[index];
                    sum += contribution;
                    normalizer += Weights[index];
                }

                return Mathf.Clamp01(
                    sum / Mathf.Max(0.0001f, normalizer) * 2.45f);
            }
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

        internal static SuiteResult BuildSuite()
        {
            return BuildSuite(true);
        }

        internal static SuiteResult BuildSuite(bool retainEvidence)
        {
            SuiteResult suite = new SuiteResult();
            try
            {
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
                        sources);
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
            IReadOnlyList<SourceCache> sources)
        {
            CandidateResult result = new CandidateResult
            {
                Definition = definition
            };
            WorkBuffers raw = new WorkBuffers();
            bool[] occupiedQuietBlocks = new bool[QuietBlockCount];
            int occupiedQuietBlockCount = 0;
            int occupiedPixelCount = 0;
            int[] sourceCounts = new int[sources.Count];
            DeterministicRandom random =
                new DeterministicRandom(definition.Seed);
            MacroField macroField = new MacroField(random);

            for (int attempt = 0;
                 attempt < MaximumPlacementAttempts;
                 attempt++)
            {
                float currentCoverage = occupiedPixelCount /
                    (float)(WorkResolution * WorkResolution);
                if (currentCoverage >= definition.TargetCoverage)
                {
                    break;
                }

                int sourceIndex = SelectBalancedSource(
                    sourceCounts,
                    random);
                SourceCache source = sources[sourceIndex];
                float scale = random.Range(0.75f, 1.25f);
                float burial = random.Range(0.18f, 0.32f);
                float rotation = random.Range(0f, 360f);
                Vector2 center = SelectPlacementCenter(
                    result.Placements,
                    macroField,
                    random,
                    attempt);
                PlacementEvidence placement = new PlacementEvidence
                {
                    Index = result.Placements.Count,
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

                if (!PassesSpacing(
                        result.Placements,
                        placement,
                        MinimumSpacingFactor))
                {
                    result.RejectedForSpacing++;
                    continue;
                }

                if (HasLocalStableIdRepeat(
                        result.Placements,
                        placement))
                {
                    result.RejectedForLocalRepeat++;
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
                if (projectedCoverage > definition.MaximumCoverage + 0.002f)
                {
                    result.RejectedForCoverage++;
                    continue;
                }

                int addedQuietBlocks = CountNewQuietBlocks(
                    raster.QuietBlocks,
                    occupiedQuietBlocks);
                int maximumOccupiedBlocks = Mathf.FloorToInt(
                    (1f - definition.MinimumQuietFraction) *
                    QuietBlockCount);
                if (occupiedQuietBlockCount + addedQuietBlocks >
                    maximumOccupiedBlocks)
                {
                    result.RejectedForQuietBudget++;
                    continue;
                }

                CommitPlacement(raw, raster, placement.Index);
                foreach (int block in raster.QuietBlocks)
                {
                    if (!occupiedQuietBlocks[block])
                    {
                        occupiedQuietBlocks[block] = true;
                        occupiedQuietBlockCount++;
                    }
                }

                occupiedPixelCount += newPixels;
                sourceCounts[sourceIndex]++;
                result.Placements.Add(placement);
            }

            float workCoverage = occupiedPixelCount /
                (float)(WorkResolution * WorkResolution);
            if (workCoverage < definition.MinimumCoverage)
            {
                result.Failure = string.Format(
                    CultureInfo.InvariantCulture,
                    "Coverage stopped at {0:P2}; minimum is {1:P2}.",
                    workCoverage,
                    definition.MinimumCoverage);
                return result;
            }

            if (result.Placements.Count < MinimumSourceDiversity)
            {
                result.Failure = "Only " + result.Placements.Count +
                    " placements were committed; at least " +
                    MinimumSourceDiversity + " are required.";
                return result;
            }

            WorkBuffers processed = BuildProcessedBuffers(
                raw,
                result.Placements,
                sources);
            FinalBuffers final = Downsample(raw, processed);
            BuildFinalEvidence(result, final, result.Placements, sources);
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
            result.Seams = MeasureSeams(final, result.Moderate);
            result.Fingerprint = CalculateCandidateFingerprint(result);
            return result;
        }

        private static int SelectBalancedSource(
            IReadOnlyList<int> counts,
            DeterministicRandom random)
        {
            int minimum = int.MaxValue;
            for (int index = 0; index < counts.Count; index++)
            {
                minimum = Mathf.Min(minimum, counts[index]);
            }

            int[] candidates = new int[counts.Count];
            int candidateCount = 0;
            for (int index = 0; index < counts.Count; index++)
            {
                if (counts[index] == minimum)
                {
                    candidates[candidateCount++] = index;
                }
            }

            return candidates[random.Range(0, candidateCount)];
        }

        private static Vector2 SelectPlacementCenter(
            IReadOnlyList<PlacementEvidence> placements,
            MacroField macroField,
            DeterministicRandom random,
            int attempt)
        {
            Vector2 best = Vector2.zero;
            float bestScore = float.NegativeInfinity;
            bool isolated = attempt > 0 && attempt % 9 == 0;
            for (int sample = 0;
                 sample < PlacementCandidateSamples;
                 sample++)
            {
                Vector2 point = new Vector2(
                    random.Range(0f, WorkResolution),
                    random.Range(0f, WorkResolution));
                float macro = macroField.Evaluate(point);
                float nearest = WorkResolution;
                for (int index = 0; index < placements.Count; index++)
                {
                    nearest = Mathf.Min(
                        nearest,
                        ToroidalDistance(
                            point,
                            new Vector2(
                                placements[index].CenterX,
                                placements[index].CenterY)));
                }

                float spacingPreference = placements.Count == 0
                    ? 0.5f
                    : Mathf.Clamp01(nearest / 360f);
                float score = isolated
                    ? spacingPreference * 0.72f +
                        (1f - macro) * 0.18f +
                        random.NextFloat() * 0.10f
                    : macro * 0.78f +
                        spacingPreference * 0.12f +
                        random.NextFloat() * 0.10f;
                if (score > bestScore)
                {
                    bestScore = score;
                    best = point;
                }
            }

            return best;
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

            foreach (int pixelIndex in output.Pixels.Keys)
            {
                int x = pixelIndex % WorkResolution;
                int y = pixelIndex / WorkResolution;
                int blockX = x / QuietBlockSizeWork;
                int blockY = y / QuietBlockSizeWork;
                output.QuietBlocks.Add(blockY * QuietBlockAxis + blockX);
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
            PlacementEvidence candidate,
            float spacingFactor)
        {
            Vector2 center = new Vector2(
                candidate.CenterX,
                candidate.CenterY);
            for (int index = 0; index < placements.Count; index++)
            {
                PlacementEvidence other = placements[index];
                float minimumDistance =
                    (candidate.Radius + other.Radius) * spacingFactor;
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

        private static bool HasLocalStableIdRepeat(
            IReadOnlyList<PlacementEvidence> placements,
            PlacementEvidence candidate)
        {
            Vector2 center = new Vector2(
                candidate.CenterX,
                candidate.CenterY);
            for (int index = 0; index < placements.Count; index++)
            {
                PlacementEvidence other = placements[index];
                if (!string.Equals(
                        other.StableId,
                        candidate.StableId,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                float distance = ToroidalDistance(
                    center,
                    new Vector2(other.CenterX, other.CenterY));
                if (distance < 230f)
                {
                    return true;
                }
            }

            return false;
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

        private static int CountNewQuietBlocks(
            ICollection<int> blocks,
            IReadOnlyList<bool> occupied)
        {
            int count = 0;
            foreach (int block in blocks)
            {
                if (!occupied[block])
                {
                    count++;
                }
            }

            return count;
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
                    float mask = 0f;
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
                            mask = Mathf.Max(mask, raw.Mask[source]);
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

                    final.Mask[destination] = mask;
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

        private static void BuildFinalEvidence(
            CandidateResult result,
            FinalBuffers final,
            IReadOnlyList<PlacementEvidence> placements,
            IReadOnlyList<SourceCache> sources)
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
            Color substrateA = new Color(0.16f, 0.14f, 0.11f, 1f);
            Color substrateB = new Color(0.20f, 0.18f, 0.14f, 1f);
            for (int y = 0; y < FinalResolution; y++)
            {
                for (int x = 0; x < FinalResolution; x++)
                {
                    int index = y * FinalResolution + x;
                    Color substrate = Color.Lerp(
                        substrateA,
                        substrateB,
                        0.25f);
                    result.Mask[index] = Grayscale(ToByte(final.Mask[index]));
                    result.Height[index] = Grayscale(ToByte(final.Height[index]));
                    result.Normals[index] = EncodeWorldNormal(
                        final.Normals[index]);
                    result.Variation[index] = Grayscale(
                        ToByte(final.Variation[index]));
                    result.RootDarkening[index] = Grayscale(
                        ToByte(final.RootDarkening[index]));
                    result.EdgeWear[index] = Grayscale(
                        ToByte(final.EdgeWear[index]));
                    if (final.Mask[index] > 0.5f)
                    {
                        result.Moderate[index] = (Color32)
                            GeneratedMassRiverRockProjectionBaker
                                .EvaluateFrozenModerateMaterial(
                                    final.Height[index],
                                    final.Variation[index],
                                    final.Exposure[index],
                                    final.DirectionalLight[index],
                                    final.RootDarkening[index],
                                    final.EdgeWear[index]);
                        result.StableIdDebug[index] = ResolveStableIdColor(
                            placements[final.Owner[index]].SourceIndex);
                    }
                    else
                    {
                        result.Moderate[index] = (Color32)substrate;
                        result.StableIdDebug[index] = (Color32)substrate;
                    }
                }
            }

            result.PlacementDebug = BuildPlacementDebug(
                result.Moderate,
                placements);
            result.MipContactSheet = BuildMipContactSheet(result.Moderate);
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
            Color32[] preview)
        {
            SeamEvidence evidence = new SeamEvidence();
            double mask = 0.0;
            double height = 0.0;
            double normal = 0.0;
            double variation = 0.0;
            double root = 0.0;
            double wear = 0.0;
            double color = 0.0;
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
        }

        private static string CalculateCandidateFingerprint(
            CandidateResult result)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(AlgorithmVersion);
                writer.Write(result.Definition.StableId);
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
