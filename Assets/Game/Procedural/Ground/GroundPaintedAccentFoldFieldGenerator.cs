using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    internal readonly struct GroundPaintedAccentSurfaceStroke
    {
        public GroundPaintedAccentSurfaceStroke(
            Vector3[] localPoints,
            Vector3[] localNormals,
            float width,
            float bodyWidth,
            float strength,
            int seed)
        {
            LocalPoints = localPoints ?? Array.Empty<Vector3>();
            LocalNormals = localNormals ?? Array.Empty<Vector3>();
            Width = Mathf.Max(0.001f, width);
            BodyWidth = Mathf.Max(Width, bodyWidth);
            Strength = Mathf.Clamp01(strength);
            Seed = seed;
        }

        public Vector3[] LocalPoints { get; }
        public Vector3[] LocalNormals { get; }
        public float Width { get; }
        public float BodyWidth { get; }
        public float Strength { get; }
        public int Seed { get; }

        public bool IsValid =>
            LocalPoints != null &&
            LocalNormals != null &&
            LocalPoints.Length >= 2 &&
            LocalNormals.Length == LocalPoints.Length;
    }

    internal readonly struct GroundPaintedAccentPlacementDiagnostics
    {
        public GroundPaintedAccentPlacementDiagnostics(
            int targetProposals,
            int candidatePool,
            int proposed,
            int accepted,
            float distributionPatchScale,
            float distributionPatchiness,
            float distributionSparseFloor,
            float proposalPatchWeightMin,
            float proposalPatchWeightMean,
            float proposalPatchWeightMax,
            int rejectedSampling,
            int rejectedRiver,
            int rejectedModifierExclusion,
            int rejectedBroadSlope,
            int rejectedLocalGrade,
            float nearestStrokeDistanceMin,
            float nearestStrokeDistanceMean,
            float nearestStrokeDistanceMax)
        {
            TargetProposals = targetProposals;
            CandidatePool = candidatePool;
            Proposed = proposed;
            Accepted = accepted;
            DistributionPatchScale = distributionPatchScale;
            DistributionPatchiness = distributionPatchiness;
            DistributionSparseFloor = distributionSparseFloor;
            ProposalPatchWeightMin = proposalPatchWeightMin;
            ProposalPatchWeightMean = proposalPatchWeightMean;
            ProposalPatchWeightMax = proposalPatchWeightMax;
            RejectedSampling = rejectedSampling;
            RejectedRiver = rejectedRiver;
            RejectedModifierExclusion = rejectedModifierExclusion;
            RejectedBroadSlope = rejectedBroadSlope;
            RejectedLocalGrade = rejectedLocalGrade;
            NearestStrokeDistanceMin = nearestStrokeDistanceMin;
            NearestStrokeDistanceMean = nearestStrokeDistanceMean;
            NearestStrokeDistanceMax = nearestStrokeDistanceMax;
        }

        public int TargetProposals { get; }
        public int CandidatePool { get; }
        public int Proposed { get; }
        public int Accepted { get; }
        public float DistributionPatchScale { get; }
        public float DistributionPatchiness { get; }
        public float DistributionSparseFloor { get; }
        public float ProposalPatchWeightMin { get; }
        public float ProposalPatchWeightMean { get; }
        public float ProposalPatchWeightMax { get; }
        public int RejectedSampling { get; }
        public int RejectedRiver { get; }
        public int RejectedModifierExclusion { get; }
        public int RejectedBroadSlope { get; }
        public int RejectedLocalGrade { get; }
        public float NearestStrokeDistanceMin { get; }
        public float NearestStrokeDistanceMean { get; }
        public float NearestStrokeDistanceMax { get; }

        public static GroundPaintedAccentPlacementDiagnostics Empty =>
            new GroundPaintedAccentPlacementDiagnostics(
                0, 0, 0, 0,
                0f, 0f, 0f,
                0f, 0f, 0f,
                0, 0, 0, 0, 0,
                0f, 0f, 0f);
    }

    public readonly struct GroundPaintedAccentDistributionDebugSample
    {
        public GroundPaintedAccentDistributionDebugSample(
            Vector3 localPosition,
            float patchWeight,
            float effectiveProposalWeight,
            bool isValid)
        {
            LocalPosition = localPosition;
            PatchWeight = Mathf.Clamp01(patchWeight);
            EffectiveProposalWeight =
                Mathf.Clamp01(effectiveProposalWeight);
            IsValid = isValid;
        }

        public Vector3 LocalPosition { get; }
        public float PatchWeight { get; }
        public float EffectiveProposalWeight { get; }
        public bool IsValid { get; }
    }

    public readonly struct GroundPaintedAccentProposalDebugPoint
    {
        public GroundPaintedAccentProposalDebugPoint(
            Vector3 localPosition,
            float patchWeight,
            float effectiveProposalWeight)
        {
            LocalPosition = localPosition;
            PatchWeight = Mathf.Clamp01(patchWeight);
            EffectiveProposalWeight =
                Mathf.Clamp01(effectiveProposalWeight);
        }

        public Vector3 LocalPosition { get; }
        public float PatchWeight { get; }
        public float EffectiveProposalWeight { get; }
    }

    public readonly struct GroundPaintedAccentPlacementDebugSnapshot
    {
        public GroundPaintedAccentPlacementDebugSnapshot(
            GroundPaintedAccentDistributionDebugSample[] distributionSamples,
            int distributionSampleResolution,
            GroundPaintedAccentProposalDebugPoint[] proposedPoints,
            int targetProposals,
            int candidatePool)
        {
            DistributionSamples =
                distributionSamples ??
                Array.Empty<GroundPaintedAccentDistributionDebugSample>();
            DistributionSampleResolution =
                Mathf.Max(0, distributionSampleResolution);
            ProposedPoints =
                proposedPoints ??
                Array.Empty<GroundPaintedAccentProposalDebugPoint>();
            TargetProposals = Mathf.Max(0, targetProposals);
            CandidatePool = Mathf.Max(0, candidatePool);
        }

        public GroundPaintedAccentDistributionDebugSample[] DistributionSamples
        {
            get;
        }

        public int DistributionSampleResolution { get; }

        public GroundPaintedAccentProposalDebugPoint[] ProposedPoints
        {
            get;
        }

        public int TargetProposals { get; }
        public int CandidatePool { get; }

        public bool IsValid =>
            DistributionSamples != null &&
            DistributionSampleResolution >= 2 &&
            DistributionSamples.Length ==
                DistributionSampleResolution * DistributionSampleResolution &&
            ProposedPoints != null;

        public static GroundPaintedAccentPlacementDebugSnapshot Empty =>
            new GroundPaintedAccentPlacementDebugSnapshot(
                Array.Empty<GroundPaintedAccentDistributionDebugSample>(),
                0,
                Array.Empty<GroundPaintedAccentProposalDebugPoint>(),
                0,
                0);
    }

    internal static class GroundPaintedAccentFoldFieldGenerator
    {
        public const int Resolution = 256;

        private const float MinimumFieldSize = 0.0001f;
        private const int MinimumStrokePointCount = 5;
        private const int CandidatePoolMultiplier = 16;
        private const int MaximumTargetStrokeCount = 240;
        private const int DebugDistributionSampleResolution = 21;
        private const float DebugSurfaceOffset = 0.025f;
        private const int MinimumValidationSampleCount = 13;
        private const float MaximumValidationSpacing = 0.25f;
        private const float RiverSafetyClearance = 0.15f;
        private const float MaximumBroadSlopeDegrees = 45f;
        private const float MaximumLocalGradeDegrees = 40f;

        private enum StrokeRejectionReason
        {
            None,
            Sampling,
            River,
            ModifierExclusion,
            BroadSlope,
            LocalGrade
        }

        private readonly struct StrokeCandidate
        {
            public StrokeCandidate(
                int column,
                int row,
                int seed,
                Vector2 centerXZ,
                float priority,
                float patchWeight,
                float semanticSupport,
                float selectionWeight)
            {
                Column = column;
                Row = row;
                Seed = seed;
                CenterXZ = centerXZ;
                Priority = priority;
                PatchWeight = patchWeight;
                SemanticSupport = semanticSupport;
                SelectionWeight = selectionWeight;
            }

            public int Column { get; }
            public int Row { get; }
            public int Seed { get; }
            public Vector2 CenterXZ { get; }
            public float Priority { get; }
            public float PatchWeight { get; }
            public float SemanticSupport { get; }
            public float SelectionWeight { get; }
        }

        public static GroundPaintedAccentSurfaceStroke[] GenerateSurfaceStrokes(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            Vector2Int patchCoordinate,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out Vector4 originSize,
            out GroundPaintedAccentPlacementDiagnostics diagnostics)
        {
            originSize = ResolveOriginSize(localBounds);
            FieldSettings settings =
                FieldSettings.Create(feature, shapeSeed, originSize);

            return GenerateSurfaceStrokesInternal(
                originSize,
                baseSurface,
                feature,
                settings,
                patchCoordinate,
                rivers,
                modifiers,
                out diagnostics);
        }

        public static GroundPaintedAccentPlacementDebugSnapshot
            BuildPlacementDebugSnapshot(
                Bounds localBounds,
                GroundHeightFieldSnapshot baseSurface,
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed,
                Vector2Int patchCoordinate)
        {
            if (baseSurface == null || !baseSurface.IsValid)
            {
                return GroundPaintedAccentPlacementDebugSnapshot.Empty;
            }

            Vector4 originSize = ResolveOriginSize(localBounds);
            FieldSettings settings =
                FieldSettings.Create(feature, shapeSeed, originSize);
            List<StrokeCandidate> candidates =
                settings.TargetStrokeCount > 0
                    ? BuildStrokeCandidates(
                        originSize,
                        baseSurface,
                        feature,
                        settings,
                        patchCoordinate)
                    : new List<StrokeCandidate>();

            candidates.Sort(CompareStrokeCandidates);

            int proposedCount =
                Mathf.Min(settings.TargetStrokeCount, candidates.Count);
            List<GroundPaintedAccentProposalDebugPoint> proposedPoints =
                new List<GroundPaintedAccentProposalDebugPoint>(proposedCount);

            for (int index = 0; index < proposedCount; index++)
            {
                StrokeCandidate candidate = candidates[index];
                if (!TryResolveDebugLocalPosition(
                        baseSurface,
                        candidate.CenterXZ,
                        out Vector3 localPosition))
                {
                    continue;
                }

                proposedPoints.Add(
                    new GroundPaintedAccentProposalDebugPoint(
                        localPosition,
                        candidate.PatchWeight,
                        candidate.SelectionWeight));
            }

            int sampleResolution = DebugDistributionSampleResolution;
            List<GroundPaintedAccentDistributionDebugSample>
                distributionSamples =
                    new List<GroundPaintedAccentDistributionDebugSample>(
                        sampleResolution * sampleResolution);

            for (int z = 0; z < sampleResolution; z++)
            {
                float v =
                    sampleResolution > 1
                        ? z / (float)(sampleResolution - 1)
                        : 0.5f;

                for (int x = 0; x < sampleResolution; x++)
                {
                    float u =
                        sampleResolution > 1
                            ? x / (float)(sampleResolution - 1)
                            : 0.5f;
                    Vector2 localXZ =
                        new Vector2(
                            originSize.x + u * originSize.z,
                            originSize.y + v * originSize.w);

                    bool hasSurfacePosition =
                        TryResolveDebugLocalPosition(
                            baseSurface,
                            localXZ,
                            out Vector3 localPosition);

                    if (!hasSurfacePosition)
                    {
                        distributionSamples.Add(
                            new GroundPaintedAccentDistributionDebugSample(
                                localPosition,
                                0f,
                                0f,
                                false));
                        continue;
                    }

                    Vector2 distributionSamplePosition =
                        ResolveDistributionSamplePosition(
                            localXZ,
                            patchCoordinate,
                            originSize);
                    float patchWeight =
                        ResolveDistributionPatchWeight(
                            distributionSamplePosition,
                            settings.DistributionPatchScale,
                            settings.DistributionPatchiness,
                            settings.DistributionSparseFloor,
                            settings.Seed);
                    float semanticSupport =
                        ResolveSemanticSupport(
                            baseSurface,
                            localXZ,
                            feature != null ? feature.MaskInfluence : 0f);
                    float semanticWeight =
                        Mathf.Lerp(0.45f, 1f, semanticSupport);
                    float effectiveProposalWeight =
                        Mathf.Clamp01(patchWeight * semanticWeight);

                    distributionSamples.Add(
                        new GroundPaintedAccentDistributionDebugSample(
                            localPosition,
                            patchWeight,
                            effectiveProposalWeight,
                            true));
                }
            }

            return new GroundPaintedAccentPlacementDebugSnapshot(
                distributionSamples.ToArray(),
                sampleResolution,
                proposedPoints.ToArray(),
                settings.TargetStrokeCount,
                candidates.Count);
        }

        public static Texture2D RasterizeSurfaceStrokes(
            Bounds localBounds,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            int shapeSeed,
            GroundPaintedAccentSurfaceStroke[] surfaceStrokes,
            out Vector4 originSize,
            out Vector4 texelSize,
            out float[] bodyValues)
        {
            originSize = ResolveOriginSize(localBounds);
            texelSize =
                new Vector4(
                    1f / Resolution,
                    1f / Resolution,
                    Resolution,
                    Resolution);

            FieldSettings settings =
                FieldSettings.Create(feature, shapeSeed, originSize);
            GroundPaintedAccentSurfaceStroke[] strokes =
                surfaceStrokes ?? Array.Empty<GroundPaintedAccentSurfaceStroke>();

            float[] selectedLineField = new float[Resolution * Resolution];
            float[] bodyField = new float[Resolution * Resolution];
            float[] signedSideField = new float[Resolution * Resolution];
            float[] supportField = new float[Resolution * Resolution];

            RasterizeSurfaceStrokesInternal(
                originSize,
                baseSurface,
                feature,
                strokes,
                settings,
                selectedLineField,
                bodyField,
                signedSideField,
                supportField);

            bodyValues = bodyField;

            Texture2D texture =
                new Texture2D(
                    Resolution,
                    Resolution,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "GeneratedGround_PaintedAccentFoldField",
                    hideFlags = HideFlags.DontSave,
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = TextureFilterModeForFoldField()
                };

            Color32[] pixels =
                BuildPixelsFromSurfaceStrokeFields(
                    selectedLineField,
                    bodyField,
                    signedSideField,
                    supportField);

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Vector4 ResolveOriginSize(Bounds localBounds)
        {
            return new Vector4(
                localBounds.min.x,
                localBounds.min.z,
                Mathf.Max(MinimumFieldSize, localBounds.size.x),
                Mathf.Max(MinimumFieldSize, localBounds.size.z));
        }

        private static FilterMode TextureFilterModeForFoldField()
        {
            return FilterMode.Bilinear;
        }

        private static GroundPaintedAccentSurfaceStroke[] GenerateSurfaceStrokesInternal(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            FieldSettings settings,
            Vector2Int patchCoordinate,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out GroundPaintedAccentPlacementDiagnostics diagnostics)
        {
            diagnostics = GroundPaintedAccentPlacementDiagnostics.Empty;

            if (baseSurface == null ||
                !baseSurface.IsValid ||
                settings.TargetStrokeCount <= 0)
            {
                return Array.Empty<GroundPaintedAccentSurfaceStroke>();
            }

            List<StrokeCandidate> candidates =
                BuildStrokeCandidates(
                    originSize,
                    baseSurface,
                    feature,
                    settings,
                    patchCoordinate);

            candidates.Sort(CompareStrokeCandidates);

            int proposed =
                Mathf.Min(settings.TargetStrokeCount, candidates.Count);
            List<GroundPaintedAccentSurfaceStroke> strokes =
                new List<GroundPaintedAccentSurfaceStroke>(proposed);
            float minimumProposalPatchWeight = float.PositiveInfinity;
            float maximumProposalPatchWeight = 0f;
            double totalProposalPatchWeight = 0d;
            int rejectedSampling = 0;
            int rejectedRiver = 0;
            int rejectedModifierExclusion = 0;
            int rejectedBroadSlope = 0;
            int rejectedLocalGrade = 0;

            for (int proposalIndex = 0;
                 proposalIndex < proposed;
                 proposalIndex++)
            {
                StrokeCandidate candidate = candidates[proposalIndex];
                minimumProposalPatchWeight =
                    Mathf.Min(minimumProposalPatchWeight, candidate.PatchWeight);
                maximumProposalPatchWeight =
                    Mathf.Max(maximumProposalPatchWeight, candidate.PatchWeight);
                totalProposalPatchWeight += candidate.PatchWeight;

                uint strokeHash = (uint)candidate.Seed;
                Vector2 axis = ResolveStrokeAxis(settings, strokeHash);
                float length =
                    Mathf.Lerp(
                        settings.StrokeLengthMin,
                        settings.StrokeLengthMax,
                        Hash01(strokeHash, 53u));
                float width =
                    settings.StrokeWidthWorld *
                    Mathf.Lerp(0.84f, 1.18f, Hash01(strokeHash, 59u));
                float bodyWidth =
                    Mathf.Max(
                        width * 2.75f,
                        settings.BodyWidthWorld *
                        Mathf.Lerp(0.82f, 1.18f, Hash01(strokeHash, 61u)));
                float curvature =
                    bodyWidth *
                    Mathf.Lerp(0.10f, 0.30f, settings.Contrast) *
                    (Hash01(strokeHash, 67u) * 2f - 1f);
                float strength =
                    settings.Strength *
                    Mathf.Lerp(0.78f, 1.0f, candidate.SemanticSupport) *
                    Mathf.Lerp(0.80f, 1.0f, Hash01(strokeHash, 71u));

                if (TryCreateSurfaceStroke(
                        candidate.CenterXZ,
                        axis,
                        length,
                        width,
                        bodyWidth,
                        curvature,
                        strength,
                        candidate.Seed,
                        baseSurface,
                        rivers,
                        modifiers,
                        out GroundPaintedAccentSurfaceStroke stroke,
                        out StrokeRejectionReason rejectionReason))
                {
                    strokes.Add(stroke);
                    continue;
                }

                switch (rejectionReason)
                {
                    case StrokeRejectionReason.River:
                        rejectedRiver++;
                        break;
                    case StrokeRejectionReason.ModifierExclusion:
                        rejectedModifierExclusion++;
                        break;
                    case StrokeRejectionReason.BroadSlope:
                        rejectedBroadSlope++;
                        break;
                    case StrokeRejectionReason.LocalGrade:
                        rejectedLocalGrade++;
                        break;
                    case StrokeRejectionReason.Sampling:
                    default:
                        rejectedSampling++;
                        break;
                }
            }

            ResolveNearestStrokeDistanceStatistics(
                strokes,
                out float nearestDistanceMin,
                out float nearestDistanceMean,
                out float nearestDistanceMax);

            float proposalPatchWeightMin =
                proposed > 0 ? minimumProposalPatchWeight : 0f;
            float proposalPatchWeightMean =
                proposed > 0
                    ? (float)(totalProposalPatchWeight / proposed)
                    : 0f;

            diagnostics =
                new GroundPaintedAccentPlacementDiagnostics(
                    settings.TargetStrokeCount,
                    candidates.Count,
                    proposed,
                    strokes.Count,
                    settings.DistributionPatchScale,
                    settings.DistributionPatchiness,
                    settings.DistributionSparseFloor,
                    proposalPatchWeightMin,
                    proposalPatchWeightMean,
                    proposed > 0 ? maximumProposalPatchWeight : 0f,
                    rejectedSampling,
                    rejectedRiver,
                    rejectedModifierExclusion,
                    rejectedBroadSlope,
                    rejectedLocalGrade,
                    nearestDistanceMin,
                    nearestDistanceMean,
                    nearestDistanceMax);

            return strokes.ToArray();
        }

        private static List<StrokeCandidate> BuildStrokeCandidates(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            FieldSettings settings,
            Vector2Int patchCoordinate)
        {
            int desiredCandidateCount =
                Mathf.Max(
                    settings.TargetStrokeCount,
                    settings.TargetStrokeCount * CandidatePoolMultiplier);
            float aspect =
                Mathf.Max(0.25f, originSize.z / Mathf.Max(0.0001f, originSize.w));
            int columns =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(Mathf.Sqrt(desiredCandidateCount * aspect)));
            int rows =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(desiredCandidateCount / (float)columns));
            float cellWidth = originSize.z / columns;
            float cellHeight = originSize.w / rows;
            List<StrokeCandidate> candidates =
                new List<StrokeCandidate>(columns * rows);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int globalColumn = column + patchCoordinate.x * columns;
                    int globalRow = row + patchCoordinate.y * rows;
                    int cellHash = Hash(globalColumn, globalRow, settings.Seed);
                    float jitterX = Hash01((uint)cellHash, 19u);
                    float jitterZ = Hash01((uint)cellHash, 31u);
                    float localX =
                        originSize.x +
                        (column + Mathf.Lerp(0.08f, 0.92f, jitterX)) * cellWidth;
                    float localZ =
                        originSize.y +
                        (row + Mathf.Lerp(0.08f, 0.92f, jitterZ)) * cellHeight;
                    Vector2 centerXZ = new Vector2(localX, localZ);
                    float semanticSupport =
                        ResolveSemanticSupport(
                            baseSurface,
                            centerXZ,
                            feature != null ? feature.MaskInfluence : 0f);
                    Vector2 distributionSamplePosition =
                        ResolveDistributionSamplePosition(
                            centerXZ,
                            patchCoordinate,
                            originSize);
                    float patchWeight =
                        ResolveDistributionPatchWeight(
                            distributionSamplePosition,
                            settings.DistributionPatchScale,
                            settings.DistributionPatchiness,
                            settings.DistributionSparseFloor,
                            settings.Seed);
                    float semanticWeight =
                        Mathf.Lerp(0.45f, 1f, semanticSupport);
                    float selectionWeight =
                        Mathf.Max(0.001f, patchWeight * semanticWeight);
                    float selectionRoll =
                        Mathf.Max(0.000001f, Hash01((uint)cellHash, 43u));
                    float priority =
                        -Mathf.Log(selectionRoll) / selectionWeight;

                    candidates.Add(
                        new StrokeCandidate(
                            column,
                            row,
                            cellHash,
                            centerXZ,
                            priority,
                            patchWeight,
                            semanticSupport,
                            selectionWeight));
                }
            }

            return candidates;
        }

        private static Vector2 ResolveDistributionSamplePosition(
            Vector2 localXZ,
            Vector2Int patchCoordinate,
            Vector4 originSize)
        {
            return
                localXZ +
                new Vector2(
                    patchCoordinate.x * originSize.z,
                    patchCoordinate.y * originSize.w);
        }

        private static float ResolveDistributionPatchWeight(
            Vector2 distributionSamplePosition,
            float patchScale,
            float patchiness,
            float sparseFloor,
            int seed)
        {
            float patchNoise =
                ResolveDistributionPatchNoise(
                    distributionSamplePosition,
                    patchScale,
                    seed);

            return Mathf.Lerp(
                1f,
                Mathf.Lerp(
                    Mathf.Clamp(sparseFloor, 0.02f, 0.40f),
                    1f,
                    Smooth01(patchNoise)),
                Mathf.Clamp01(patchiness));
        }

        private static bool TryResolveDebugLocalPosition(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            out Vector3 localPosition)
        {
            localPosition = new Vector3(localXZ.x, 0f, localXZ.y);
            if (baseSurface == null ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return false;
            }

            localPosition.y = sample.Height + DebugSurfaceOffset;
            return true;
        }

        private static bool TryCreateSurfaceStroke(
            Vector2 centerXZ,
            Vector2 axis,
            float length,
            float width,
            float bodyWidth,
            float curvature,
            float strength,
            int strokeSeed,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out GroundPaintedAccentSurfaceStroke stroke,
            out StrokeRejectionReason rejectionReason)
        {
            stroke = default;
            rejectionReason = StrokeRejectionReason.None;

            axis = ResolveSafeAxis(axis);
            Vector2 fixedCrossAxis = new Vector2(-axis.y, axis.x);
            float phase = Hash01((uint)strokeSeed, 83u) * Mathf.PI * 2f;
            float secondaryPhase = Hash01((uint)strokeSeed, 89u) * Mathf.PI * 2f;
            float estimatedValidationLength =
                length + Mathf.Abs(curvature) * 9f;
            int validationSampleCount =
                Mathf.Max(
                    MinimumValidationSampleCount,
                    Mathf.CeilToInt(
                        estimatedValidationLength /
                        MaximumValidationSpacing) + 1);
            float validationStep =
                validationSampleCount > 1
                    ? 1f / (validationSampleCount - 1f)
                    : 1f;
            bool hasPreviousCenter = false;
            Vector2 previousCenterXZ = Vector2.zero;
            float previousCenterHeight = 0f;

            for (int sampleIndex = 0;
                 sampleIndex < validationSampleCount;
                 sampleIndex++)
            {
                float t =
                    validationSampleCount <= 1
                        ? 0f
                        : sampleIndex / (float)(validationSampleCount - 1);
                Vector2 sampleCenterXZ =
                    EvaluateStrokeCenterline(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t);
                Vector2 tangent =
                    ResolveStrokeTangent(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t,
                        validationStep);
                Vector2 sideAxis = new Vector2(-tangent.y, tangent.x);
                float halfWidth = Mathf.Max(0.0005f, width * 0.5f);
                Vector2 leftXZ = sampleCenterXZ - sideAxis * halfWidth;
                Vector2 rightXZ = sampleCenterXZ + sideAxis * halfWidth;

                if (!baseSurface.TrySample(
                        sampleCenterXZ,
                        out GroundSurfaceSample centerSample) ||
                    !baseSurface.TrySample(
                        leftXZ,
                        out GroundSurfaceSample leftSample) ||
                    !baseSurface.TrySample(
                        rightXZ,
                        out GroundSurfaceSample rightSample))
                {
                    rejectionReason = StrokeRejectionReason.Sampling;
                    return false;
                }

                if (ExceedsBroadSlope(centerSample) ||
                    ExceedsBroadSlope(leftSample) ||
                    ExceedsBroadSlope(rightSample))
                {
                    rejectionReason = StrokeRejectionReason.BroadSlope;
                    return false;
                }

                if (IsExcludedByRiver(
                        sampleCenterXZ,
                        rivers,
                        halfWidth + RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        leftXZ,
                        rivers,
                        RiverSafetyClearance) ||
                    IsExcludedByRiver(
                        rightXZ,
                        rivers,
                        RiverSafetyClearance))
                {
                    rejectionReason = StrokeRejectionReason.River;
                    return false;
                }

                if (IsExcludedByModifier(sampleCenterXZ, modifiers) ||
                    IsExcludedByModifier(leftXZ, modifiers) ||
                    IsExcludedByModifier(rightXZ, modifiers))
                {
                    rejectionReason = StrokeRejectionReason.ModifierExclusion;
                    return false;
                }

                if (ExceedsGrade(
                        leftSample.Height,
                        centerSample.Height,
                        Vector2.Distance(leftXZ, sampleCenterXZ)) ||
                    ExceedsGrade(
                        centerSample.Height,
                        rightSample.Height,
                        Vector2.Distance(sampleCenterXZ, rightXZ)))
                {
                    rejectionReason = StrokeRejectionReason.LocalGrade;
                    return false;
                }

                if (hasPreviousCenter &&
                    ExceedsGrade(
                        previousCenterHeight,
                        centerSample.Height,
                        Vector2.Distance(previousCenterXZ, sampleCenterXZ)))
                {
                    rejectionReason = StrokeRejectionReason.LocalGrade;
                    return false;
                }

                hasPreviousCenter = true;
                previousCenterXZ = sampleCenterXZ;
                previousCenterHeight = centerSample.Height;
            }

            int pointCount =
                Mathf.Clamp(
                    Mathf.CeilToInt(length * 1.85f),
                    MinimumStrokePointCount,
                    11);
            Vector3[] localPoints = new Vector3[pointCount];
            Vector3[] localNormals = new Vector3[pointCount];

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                float t =
                    pointCount <= 1
                        ? 0f
                        : pointIndex / (float)(pointCount - 1);
                Vector2 localXZ =
                    EvaluateStrokeCenterline(
                        centerXZ,
                        axis,
                        fixedCrossAxis,
                        length,
                        curvature,
                        phase,
                        secondaryPhase,
                        t);

                if (!baseSurface.TrySample(
                        localXZ,
                        out GroundSurfaceSample sample))
                {
                    rejectionReason = StrokeRejectionReason.Sampling;
                    return false;
                }

                localPoints[pointIndex] =
                    new Vector3(localXZ.x, sample.Height, localXZ.y);
                localNormals[pointIndex] = sample.RenderNormal;
            }

            stroke =
                new GroundPaintedAccentSurfaceStroke(
                    localPoints,
                    localNormals,
                    width,
                    bodyWidth,
                    strength,
                    strokeSeed);
            return stroke.IsValid;
        }

        private static Vector2 EvaluateStrokeCenterline(
            Vector2 centerXZ,
            Vector2 axis,
            Vector2 crossAxis,
            float length,
            float curvature,
            float phase,
            float secondaryPhase,
            float t)
        {
            float clampedT = Mathf.Clamp01(t);
            float signedT = clampedT * 2f - 1f;
            float endFade = Mathf.Sin(clampedT * Mathf.PI);
            float wobble =
                (Mathf.Sin(clampedT * Mathf.PI * 1.35f + phase) * 0.72f +
                 Mathf.Sin(clampedT * Mathf.PI * 2.10f + secondaryPhase) * 0.28f) *
                curvature *
                endFade;

            return
                centerXZ +
                axis * (signedT * length * 0.5f) +
                crossAxis * wobble;
        }

        private static Vector2 ResolveStrokeTangent(
            Vector2 centerXZ,
            Vector2 axis,
            Vector2 crossAxis,
            float length,
            float curvature,
            float phase,
            float secondaryPhase,
            float t,
            float sampleStep)
        {
            float halfStep = Mathf.Max(0.001f, sampleStep * 0.5f);
            float beforeT = Mathf.Clamp01(t - halfStep);
            float afterT = Mathf.Clamp01(t + halfStep);
            Vector2 before =
                EvaluateStrokeCenterline(
                    centerXZ,
                    axis,
                    crossAxis,
                    length,
                    curvature,
                    phase,
                    secondaryPhase,
                    beforeT);
            Vector2 after =
                EvaluateStrokeCenterline(
                    centerXZ,
                    axis,
                    crossAxis,
                    length,
                    curvature,
                    phase,
                    secondaryPhase,
                    afterT);
            Vector2 tangent = after - before;
            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : axis;
        }

        private static bool ExceedsBroadSlope(GroundSurfaceSample sample)
        {
            Vector3 normal =
                sample.RenderNormal.sqrMagnitude > 0.000001f
                    ? sample.RenderNormal.normalized
                    : Vector3.up;
            return Vector3.Angle(normal, Vector3.up) > MaximumBroadSlopeDegrees;
        }

        private static bool ExceedsGrade(
            float heightA,
            float heightB,
            float horizontalDistance)
        {
            if (horizontalDistance <= 0.00001f)
            {
                return Mathf.Abs(heightB - heightA) > 0.00001f;
            }

            float gradeDegrees =
                Mathf.Atan2(
                    Mathf.Abs(heightB - heightA),
                    horizontalDistance) *
                Mathf.Rad2Deg;
            return gradeDegrees > MaximumLocalGradeDegrees;
        }

        private static bool IsExcludedByRiver(
            Vector2 point,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            float clearance)
        {
            if (rivers == null)
            {
                return false;
            }

            for (int riverIndex = 0;
                 riverIndex < rivers.Count;
                 riverIndex++)
            {
                StylizedRiverGroundSnapshot river = rivers[riverIndex];
                if (!river.IsValid ||
                    !river.TryEvaluate(
                        point,
                        out float distance,
                        out _,
                        out _,
                        out float surfaceHalfWidth))
                {
                    continue;
                }

                if (distance <=
                    river.ResolveHandoffHalfWidth(surfaceHalfWidth) +
                    Mathf.Max(0f, clearance))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsExcludedByModifier(
            Vector2 point,
            IReadOnlyList<GroundModifierSnapshot> modifiers)
        {
            if (modifiers == null)
            {
                return false;
            }

            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                GroundModifierSnapshot modifier = modifiers[modifierIndex];
                if (!modifier.Excludes(
                        GroundSurfaceFeatureExclusionFlags.PaintedAccentLines))
                {
                    continue;
                }

                if (modifier.EvaluateWeight(point) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ResolveNearestStrokeDistanceStatistics(
            IReadOnlyList<GroundPaintedAccentSurfaceStroke> strokes,
            out float minimum,
            out float mean,
            out float maximum)
        {
            minimum = 0f;
            mean = 0f;
            maximum = 0f;

            if (strokes == null || strokes.Count < 2)
            {
                return;
            }

            float globalMinimum = float.PositiveInfinity;
            float globalMaximum = 0f;
            double totalNearest = 0d;
            int measuredCount = 0;

            for (int strokeIndex = 0;
                 strokeIndex < strokes.Count;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke = strokes[strokeIndex];
                if (!stroke.IsValid)
                {
                    continue;
                }

                float nearest = float.PositiveInfinity;
                for (int otherIndex = 0;
                     otherIndex < strokes.Count;
                     otherIndex++)
                {
                    if (strokeIndex == otherIndex)
                    {
                        continue;
                    }

                    GroundPaintedAccentSurfaceStroke other = strokes[otherIndex];
                    if (!other.IsValid)
                    {
                        continue;
                    }

                    float pointDistance =
                        ResolveStrokePointDistance(stroke, other);
                    float edgeDistance =
                        Mathf.Max(
                            0f,
                            pointDistance -
                            (stroke.Width + other.Width) * 0.5f);
                    nearest = Mathf.Min(nearest, edgeDistance);
                }

                if (float.IsPositiveInfinity(nearest))
                {
                    continue;
                }

                globalMinimum = Mathf.Min(globalMinimum, nearest);
                globalMaximum = Mathf.Max(globalMaximum, nearest);
                totalNearest += nearest;
                measuredCount++;
            }

            if (measuredCount <= 0)
            {
                return;
            }

            minimum = globalMinimum;
            mean = (float)(totalNearest / measuredCount);
            maximum = globalMaximum;
        }

        private static float ResolveStrokePointDistance(
            GroundPaintedAccentSurfaceStroke left,
            GroundPaintedAccentSurfaceStroke right)
        {
            float minimumSquared = float.PositiveInfinity;
            Vector3[] leftPoints = left.LocalPoints;
            Vector3[] rightPoints = right.LocalPoints;

            for (int leftIndex = 0;
                 leftIndex < leftPoints.Length - 1;
                 leftIndex++)
            {
                Vector2 leftStart =
                    new Vector2(leftPoints[leftIndex].x, leftPoints[leftIndex].z);
                Vector2 leftEnd =
                    new Vector2(
                        leftPoints[leftIndex + 1].x,
                        leftPoints[leftIndex + 1].z);

                for (int rightIndex = 0;
                     rightIndex < rightPoints.Length - 1;
                     rightIndex++)
                {
                    Vector2 rightStart =
                        new Vector2(
                            rightPoints[rightIndex].x,
                            rightPoints[rightIndex].z);
                    Vector2 rightEnd =
                        new Vector2(
                            rightPoints[rightIndex + 1].x,
                            rightPoints[rightIndex + 1].z);
                    minimumSquared =
                        Mathf.Min(
                            minimumSquared,
                            ResolveSegmentDistanceSquared(
                                leftStart,
                                leftEnd,
                                rightStart,
                                rightEnd));

                    if (minimumSquared <= 0.0000001f)
                    {
                        return 0f;
                    }
                }
            }

            return float.IsPositiveInfinity(minimumSquared)
                ? 0f
                : Mathf.Sqrt(minimumSquared);
        }

        private static float ResolveSegmentDistanceSquared(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d)
        {
            if (SegmentsIntersect(a, b, c, d))
            {
                return 0f;
            }

            return Mathf.Min(
                Mathf.Min(
                    ResolvePointSegmentDistanceSquared(a, c, d),
                    ResolvePointSegmentDistanceSquared(b, c, d)),
                Mathf.Min(
                    ResolvePointSegmentDistanceSquared(c, a, b),
                    ResolvePointSegmentDistanceSquared(d, a, b)));
        }

        private static float ResolvePointSegmentDistanceSquared(
            Vector2 point,
            Vector2 start,
            Vector2 end)
        {
            Vector2 segment = end - start;
            float lengthSquared = segment.sqrMagnitude;
            if (lengthSquared <= 0.0000001f)
            {
                return (point - start).sqrMagnitude;
            }

            float t =
                Mathf.Clamp01(
                    Vector2.Dot(point - start, segment) /
                    lengthSquared);
            Vector2 closest = start + segment * t;
            return (point - closest).sqrMagnitude;
        }

        private static bool SegmentsIntersect(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d)
        {
            float abC = Cross2D(b - a, c - a);
            float abD = Cross2D(b - a, d - a);
            float cdA = Cross2D(d - c, a - c);
            float cdB = Cross2D(d - c, b - c);
            const float Epsilon = 0.000001f;

            if (((abC > Epsilon && abD < -Epsilon) ||
                 (abC < -Epsilon && abD > Epsilon)) &&
                ((cdA > Epsilon && cdB < -Epsilon) ||
                 (cdA < -Epsilon && cdB > Epsilon)))
            {
                return true;
            }

            return
                Mathf.Abs(abC) <= Epsilon && IsPointOnSegment(c, a, b) ||
                Mathf.Abs(abD) <= Epsilon && IsPointOnSegment(d, a, b) ||
                Mathf.Abs(cdA) <= Epsilon && IsPointOnSegment(a, c, d) ||
                Mathf.Abs(cdB) <= Epsilon && IsPointOnSegment(b, c, d);
        }

        private static bool IsPointOnSegment(
            Vector2 point,
            Vector2 start,
            Vector2 end)
        {
            const float Epsilon = 0.000001f;
            return point.x >= Mathf.Min(start.x, end.x) - Epsilon &&
                   point.x <= Mathf.Max(start.x, end.x) + Epsilon &&
                   point.y >= Mathf.Min(start.y, end.y) - Epsilon &&
                   point.y <= Mathf.Max(start.y, end.y) + Epsilon;
        }

        private static float Cross2D(Vector2 left, Vector2 right)
        {
            return left.x * right.y - left.y * right.x;
        }

        private static void RasterizeSurfaceStrokesInternal(
            Vector4 originSize,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            GroundPaintedAccentSurfaceStroke[] strokes,
            FieldSettings settings,
            float[] selectedLineField,
            float[] bodyField,
            float[] signedSideField,
            float[] supportField)
        {
            float maskInfluence =
                feature != null
                    ? feature.MaskInfluence
                    : 0f;
            float texelWorldSize =
                Mathf.Max(
                    originSize.z,
                    originSize.w) /
                Resolution;
            float lineFeather =
                Mathf.Max(texelWorldSize * 0.85f, settings.StrokeWidthWorld * 0.20f);

            for (int z = 0; z < Resolution; z++)
            {
                for (int x = 0; x < Resolution; x++)
                {
                    int index = z * Resolution + x;
                    Vector2 localXZ =
                        TexelToLocalXZ(
                            x,
                            z,
                            originSize);
                    float support =
                        ResolveSemanticSupport(
                            baseSurface,
                            localXZ,
                            maskInfluence);
                    supportField[index] = support;

                    float bestBody = 0f;
                    float bestSignedSide = 0f;
                    float bestLine = 0f;

                    for (int strokeIndex = 0;
                         strokeIndex < strokes.Length;
                         strokeIndex++)
                    {
                        GroundPaintedAccentSurfaceStroke stroke =
                            strokes[strokeIndex];
                        if (!stroke.IsValid)
                        {
                            continue;
                        }

                        if (!TryResolveClosestStrokeSample(
                                stroke,
                                localXZ,
                                out float distance,
                                out float signedSide))
                        {
                            continue;
                        }

                        float lineHalfWidth =
                            Mathf.Max(
                                stroke.Width * 0.5f,
                                texelWorldSize * 0.42f);
                        float line =
                            1f - SmoothStep(
                                lineHalfWidth,
                                lineHalfWidth + lineFeather,
                                distance);
                        float body =
                            1f - SmoothStep(
                                stroke.BodyWidth * 0.18f,
                                stroke.BodyWidth,
                                distance);

                        line = Mathf.Clamp01(line) * stroke.Strength * support;
                        body = Mathf.Clamp01(body) * stroke.Strength * support;

                        if (line > bestLine)
                        {
                            bestLine = line;
                        }

                        if (body > bestBody)
                        {
                            bestBody = body;
                            bestSignedSide = signedSide;
                        }
                    }

                    selectedLineField[index] = Mathf.Clamp01(bestLine);
                    bodyField[index] = Mathf.Clamp01(bestBody);
                    signedSideField[index] = Mathf.Clamp(bestSignedSide, -1f, 1f);
                }
            }
        }

        private static bool TryResolveClosestStrokeSample(
            GroundPaintedAccentSurfaceStroke stroke,
            Vector2 localXZ,
            out float distance,
            out float signedSide)
        {
            distance = float.PositiveInfinity;
            signedSide = 0f;

            Vector3[] points = stroke.LocalPoints;
            if (points == null || points.Length < 2)
            {
                return false;
            }

            for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
            {
                Vector2 start = new Vector2(
                    points[pointIndex].x,
                    points[pointIndex].z);
                Vector2 end = new Vector2(
                    points[pointIndex + 1].x,
                    points[pointIndex + 1].z);
                Vector2 segment = end - start;
                float lengthSquared = segment.sqrMagnitude;
                if (lengthSquared <= 0.000001f)
                {
                    continue;
                }

                float t =
                    Mathf.Clamp01(
                        Vector2.Dot(localXZ - start, segment) /
                        lengthSquared);
                Vector2 closest = start + segment * t;
                Vector2 offset = localXZ - closest;
                float candidateDistance = offset.magnitude;
                if (candidateDistance >= distance)
                {
                    continue;
                }

                Vector2 segmentDirection = segment / Mathf.Sqrt(lengthSquared);
                Vector2 sideAxis = new Vector2(-segmentDirection.y, segmentDirection.x);
                float side =
                    Vector2.Dot(offset, sideAxis) /
                    Mathf.Max(0.0001f, stroke.BodyWidth);

                distance = candidateDistance;
                signedSide = Mathf.Clamp(side, -1f, 1f);
            }

            return !float.IsInfinity(distance);
        }

        private static Color32[] BuildPixelsFromSurfaceStrokeFields(
            float[] selectedLineField,
            float[] bodyField,
            float[] signedSideField,
            float[] supportField)
        {
            Color32[] pixels = new Color32[Resolution * Resolution];

            for (int index = 0; index < pixels.Length; index++)
            {
                float selectedLine =
                    selectedLineField != null && index < selectedLineField.Length
                        ? Mathf.Clamp01(selectedLineField[index])
                        : 0f;
                float body =
                    bodyField != null && index < bodyField.Length
                        ? Mathf.Clamp01(bodyField[index])
                        : 0f;
                float signedSide =
                    signedSideField != null && index < signedSideField.Length
                        ? Mathf.Clamp(signedSideField[index], -1f, 1f)
                        : 0f;
                float support =
                    supportField != null && index < supportField.Length
                        ? Mathf.Clamp01(supportField[index])
                        : 0f;

                pixels[index] =
                    new Color32(
                        ToByte(selectedLine),
                        ToByte(body),
                        ToByte(0.5f + signedSide * 0.5f),
                        ToByte(support));
            }

            return pixels;
        }

        private static float ResolveSemanticSupport(
            GroundHeightFieldSnapshot baseSurface,
            Vector2 localXZ,
            float maskInfluence)
        {
            if (baseSurface == null ||
                !baseSurface.IsValid ||
                !baseSurface.TrySample(localXZ, out GroundSurfaceSample sample))
            {
                return 1f;
            }

            float semanticSupport =
                0.22f +
                sample.DampDeposit * 0.22f +
                sample.VegetationSuitability * 0.24f +
                sample.Compaction * 0.24f +
                sample.RockyDry * 0.08f;

            return Mathf.Lerp(
                1f,
                Mathf.Clamp01(semanticSupport),
                Mathf.Clamp01(maskInfluence));
        }

        private static Vector2 TexelToLocalXZ(
            int x,
            int z,
            Vector4 originSize)
        {
            float u = (x + 0.5f) / Resolution;
            float v = (z + 0.5f) / Resolution;

            return new Vector2(
                originSize.x + u * originSize.z,
                originSize.y + v * originSize.w);
        }

        private static Vector2 ResolveStrokeAxis(
            FieldSettings settings,
            uint strokeHash)
        {
            float facingDirectionDegrees = settings.StrokeFacingDirectionDegrees;
            float perpendicularStrokeAngleDegrees = facingDirectionDegrees + 90f;
            float signedRoll = Hash01(strokeHash, 73u) * 2f - 1f;
            float jitterDegrees = signedRoll * settings.AngleJitterDegrees;
            float finalAngle =
                (perpendicularStrokeAngleDegrees + jitterDegrees) *
                Mathf.Deg2Rad;

            return new Vector2(
                Mathf.Cos(finalAngle),
                Mathf.Sin(finalAngle));
        }

        private static int CompareStrokeCandidates(
            StrokeCandidate left,
            StrokeCandidate right)
        {
            int order = left.Priority.CompareTo(right.Priority);
            if (order != 0)
            {
                return order;
            }

            order = left.Row.CompareTo(right.Row);
            if (order != 0)
            {
                return order;
            }

            return left.Column.CompareTo(right.Column);
        }

        private static float ResolveDistributionPatchNoise(
            Vector2 point,
            float patchScale,
            int seed)
        {
            float broadScale = Mathf.Max(0.01f, patchScale);
            float detailScale = Mathf.Max(0.01f, patchScale * 0.43f);
            float broad =
                ValueNoise2D(point / broadScale, seed ^ 0x2A71);
            float detail =
                ValueNoise2D(
                    point / detailScale + new Vector2(13.7f, -9.2f),
                    seed ^ 0x61C5);
            return Mathf.Clamp01(broad * 0.72f + detail * 0.28f);
        }

        private static float ValueNoise2D(Vector2 point, int seed)
        {
            int x0 = Mathf.FloorToInt(point.x);
            int y0 = Mathf.FloorToInt(point.y);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            float tx = Smooth01(point.x - x0);
            float ty = Smooth01(point.y - y0);
            float a = Hash01((uint)Hash(x0, y0, seed), 101u);
            float b = Hash01((uint)Hash(x1, y0, seed), 103u);
            float c = Hash01((uint)Hash(x0, y1, seed), 107u);
            float d = Hash01((uint)Hash(x1, y1, seed), 109u);
            float lower = Mathf.Lerp(a, b, tx);
            float upper = Mathf.Lerp(c, d, tx);
            return Mathf.Lerp(lower, upper, ty);
        }

        private static Vector2 ResolveSafeAxis(Vector2 axis)
        {
            if (axis.sqrMagnitude < 0.0001f)
            {
                return Vector2.right;
            }

            return axis.normalized;
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            if (Mathf.Abs(edge1 - edge0) <= 0.00001f)
            {
                return value >= edge1 ? 1f : 0f;
            }

            return Smooth01((value - edge0) / (edge1 - edge0));
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Clamp01(value) * 255f),
                0,
                255);
        }

        private static int Hash(
            int a,
            int b,
            int salt)
        {
            unchecked
            {
                uint h = 2166136261u;
                h = (h ^ (uint)a) * 16777619u;
                h = (h ^ (uint)b) * 16777619u;
                h = (h ^ (uint)salt) * 16777619u;
                h ^= h >> 13;
                h *= 1274126177u;
                h ^= h >> 16;
                return (int)h;
            }
        }

        private static float Hash01(
            uint hash,
            uint salt)
        {
            uint value = hash ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777215f;
        }

        private readonly struct FieldSettings
        {
            private FieldSettings(
                int seed,
                float strength,
                float contrast,
                int targetStrokeCount,
                float strokeLengthMin,
                float strokeLengthMax,
                float strokeWidthWorld,
                float bodyWidthWorld,
                float strokeFacingDirectionDegrees,
                float angleJitterDegrees,
                float distributionPatchScale,
                float distributionPatchiness,
                float distributionSparseFloor)
            {
                Seed = seed;
                Strength = strength;
                Contrast = contrast;
                TargetStrokeCount = targetStrokeCount;
                StrokeLengthMin = strokeLengthMin;
                StrokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                StrokeWidthWorld = strokeWidthWorld;
                BodyWidthWorld = bodyWidthWorld;
                StrokeFacingDirectionDegrees = strokeFacingDirectionDegrees;
                AngleJitterDegrees = angleJitterDegrees;
                DistributionPatchScale = distributionPatchScale;
                DistributionPatchiness = distributionPatchiness;
                DistributionSparseFloor = distributionSparseFloor;
            }

            public int Seed { get; }
            public float Strength { get; }
            public float Contrast { get; }
            public int TargetStrokeCount { get; }
            public float StrokeLengthMin { get; }
            public float StrokeLengthMax { get; }
            public float StrokeWidthWorld { get; }
            public float BodyWidthWorld { get; }
            public float StrokeFacingDirectionDegrees { get; }
            public float AngleJitterDegrees { get; }
            public float DistributionPatchScale { get; }
            public float DistributionPatchiness { get; }
            public float DistributionSparseFloor { get; }

            public static FieldSettings Create(
                GroundSurfaceFeatureRecipe feature,
                int shapeSeed,
                Vector4 originSize)
            {
                float strength =
                    feature != null
                        ? Mathf.Clamp01(feature.Strength)
                        : 0f;
                float contrast =
                    feature != null
                        ? Mathf.Clamp01(feature.Contrast)
                        : 0.5f;
                int seed =
                    Hash(
                        shapeSeed,
                        feature != null ? feature.SeedOffset : 0,
                        0x5A3D);
                float area = Mathf.Max(1f, originSize.z * originSize.w);
                float areaFactor = Mathf.Max(0.05f, area / 1600f);
                float strokeDensity =
                    feature != null
                        ? feature.PaintedAccentStrokeDensity
                        : 34f;
                int targetStrokeCount =
                    Mathf.Clamp(
                        Mathf.RoundToInt(strokeDensity * areaFactor),
                        0,
                        MaximumTargetStrokeCount);
                float strokeLengthMin =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMin
                        : 0.55f;
                float strokeLengthMax =
                    feature != null
                        ? feature.PaintedAccentStrokeLengthMax
                        : 1.55f;
                strokeLengthMax = Mathf.Max(strokeLengthMin + 0.05f, strokeLengthMax);
                float strokeWidthWorld =
                    feature != null
                        ? feature.PaintedAccentStrokeWidth
                        : 0.12f;
                float bodyWidthWorld =
                    Mathf.Max(
                        strokeWidthWorld * 3.25f,
                        strokeLengthMax * 0.12f);
                float strokeFacingDirectionDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeFacingDirectionDegrees
                        : 90f;
                float angleJitterDegrees =
                    feature != null
                        ? feature.PaintedAccentStrokeAngleJitterDegrees
                        : 18f;
                float distributionPatchScale =
                    feature != null
                        ? feature.PaintedAccentDistributionPatchScale
                        : 9f;
                float distributionPatchiness =
                    feature != null
                        ? feature.PaintedAccentDistributionPatchiness
                        : 0.70f;
                float distributionSparseFloor =
                    feature != null
                        ? feature.PaintedAccentDistributionSparseFloor
                        : 0.18f;

                return new FieldSettings(
                    seed,
                    Mathf.Max(0.05f, strength),
                    contrast,
                    targetStrokeCount,
                    strokeLengthMin,
                    strokeLengthMax,
                    strokeWidthWorld,
                    bodyWidthWorld,
                    strokeFacingDirectionDegrees,
                    angleJitterDegrees,
                    distributionPatchScale,
                    distributionPatchiness,
                    distributionSparseFloor);
            }
        }
    }
}
