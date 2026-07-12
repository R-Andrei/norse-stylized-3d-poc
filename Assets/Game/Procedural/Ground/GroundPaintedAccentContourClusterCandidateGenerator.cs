using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundPaintedAccentContourClusterChainRole
    {
        PrimaryLeftArm = 0,
        PrimaryRightArm = 1,
        Branch = 2,
        Echo = 3
    }

    public enum GroundPaintedAccentContourClusterRejectionReason
    {
        None = 0,
        Sampling = 1,
        River = 2,
        ModifierExclusion = 3,
        BroadSlope = 4,
        LocalGrade = 5,
        UpwardExcursion = 6
    }

    public readonly struct GroundPaintedAccentContourClusterChain
    {
        public GroundPaintedAccentContourClusterChain(
            Vector3[] localSurfacePoints,
            float[] halfWidths,
            GroundPaintedAccentContourClusterChainRole role,
            int parentChainIndex)
        {
            LocalSurfacePoints =
                localSurfacePoints ?? Array.Empty<Vector3>();
            HalfWidths = halfWidths ?? Array.Empty<float>();
            Role = role;
            ParentChainIndex = parentChainIndex;
        }

        public Vector3[] LocalSurfacePoints { get; }
        public float[] HalfWidths { get; }
        public GroundPaintedAccentContourClusterChainRole Role { get; }
        public int ParentChainIndex { get; }

        public bool IsValid =>
            LocalSurfacePoints != null &&
            HalfWidths != null &&
            LocalSurfacePoints.Length >= 2 &&
            HalfWidths.Length == LocalSurfacePoints.Length;
    }

    public readonly struct GroundPaintedAccentContourClusterCandidate
    {
        public GroundPaintedAccentContourClusterCandidate(
            GroundPaintedAccentContourClusterChain[] chains,
            Vector3 sourceAnchorLocalPosition,
            Vector3 highPointLocalPosition,
            int seed,
            float maximumUpwardExcursion,
            float planarSpan,
            float visualDrop)
        {
            Chains =
                chains ?? Array.Empty<GroundPaintedAccentContourClusterChain>();
            SourceAnchorLocalPosition = sourceAnchorLocalPosition;
            HighPointLocalPosition = highPointLocalPosition;
            Seed = seed;
            MaximumUpwardExcursion = Mathf.Max(0f, maximumUpwardExcursion);
            PlanarSpan = Mathf.Max(0f, planarSpan);
            VisualDrop = Mathf.Max(0f, visualDrop);
        }

        public GroundPaintedAccentContourClusterChain[] Chains { get; }
        public Vector3 SourceAnchorLocalPosition { get; }
        public Vector3 HighPointLocalPosition { get; }
        public int Seed { get; }
        public float MaximumUpwardExcursion { get; }
        public float PlanarSpan { get; }
        public float VisualDrop { get; }

        public bool IsValid
        {
            get
            {
                if (Chains == null || Chains.Length < 2)
                {
                    return false;
                }

                for (int chainIndex = 0;
                     chainIndex < Chains.Length;
                     chainIndex++)
                {
                    if (!Chains[chainIndex].IsValid)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }

    public readonly struct GroundPaintedAccentContourClusterRejectionDebugPoint
    {
        public GroundPaintedAccentContourClusterRejectionDebugPoint(
            Vector3 localPosition,
            GroundPaintedAccentContourClusterRejectionReason reason)
        {
            LocalPosition = localPosition;
            Reason = reason;
        }

        public Vector3 LocalPosition { get; }
        public GroundPaintedAccentContourClusterRejectionReason Reason { get; }
    }

    public readonly struct GroundPaintedAccentContourClusterDiagnostics
    {
        public GroundPaintedAccentContourClusterDiagnostics(
            int baseDescriptors,
            int densitySelected,
            int densitySkipped,
            int candidatesAccepted,
            int rejectedSampling,
            int rejectedRiver,
            int rejectedModifier,
            int rejectedBroadSlope,
            int rejectedLocalGrade,
            int rejectedUpwardExcursion,
            int chainsTotal,
            int primaryArmsTotal,
            int branchesTotal,
            int echoesTotal,
            int chainCountMin,
            float chainCountMean,
            int chainCountMax,
            int pointCountMin,
            float pointCountMean,
            int pointCountMax,
            float planarSpanMin,
            float planarSpanMean,
            float planarSpanMax,
            float visualDropMin,
            float visualDropMean,
            float visualDropMax,
            float maximumUpwardExcursion,
            int acceptedUpwardViolationCount)
        {
            BaseDescriptors = Mathf.Max(0, baseDescriptors);
            DensitySelected = Mathf.Max(0, densitySelected);
            DensitySkipped = Mathf.Max(0, densitySkipped);
            CandidatesAccepted = Mathf.Max(0, candidatesAccepted);
            RejectedSampling = Mathf.Max(0, rejectedSampling);
            RejectedRiver = Mathf.Max(0, rejectedRiver);
            RejectedModifier = Mathf.Max(0, rejectedModifier);
            RejectedBroadSlope = Mathf.Max(0, rejectedBroadSlope);
            RejectedLocalGrade = Mathf.Max(0, rejectedLocalGrade);
            RejectedUpwardExcursion = Mathf.Max(0, rejectedUpwardExcursion);
            ChainsTotal = Mathf.Max(0, chainsTotal);
            PrimaryArmsTotal = Mathf.Max(0, primaryArmsTotal);
            BranchesTotal = Mathf.Max(0, branchesTotal);
            EchoesTotal = Mathf.Max(0, echoesTotal);
            ChainCountMin = Mathf.Max(0, chainCountMin);
            ChainCountMean = Mathf.Max(0f, chainCountMean);
            ChainCountMax = Mathf.Max(0, chainCountMax);
            PointCountMin = Mathf.Max(0, pointCountMin);
            PointCountMean = Mathf.Max(0f, pointCountMean);
            PointCountMax = Mathf.Max(0, pointCountMax);
            PlanarSpanMin = Mathf.Max(0f, planarSpanMin);
            PlanarSpanMean = Mathf.Max(0f, planarSpanMean);
            PlanarSpanMax = Mathf.Max(0f, planarSpanMax);
            VisualDropMin = Mathf.Max(0f, visualDropMin);
            VisualDropMean = Mathf.Max(0f, visualDropMean);
            VisualDropMax = Mathf.Max(0f, visualDropMax);
            MaximumUpwardExcursion = Mathf.Max(0f, maximumUpwardExcursion);
            AcceptedUpwardViolationCount =
                Mathf.Max(0, acceptedUpwardViolationCount);
        }

        public int BaseDescriptors { get; }
        public int DensitySelected { get; }
        public int DensitySkipped { get; }
        public int CandidatesAccepted { get; }
        public int RejectedSampling { get; }
        public int RejectedRiver { get; }
        public int RejectedModifier { get; }
        public int RejectedBroadSlope { get; }
        public int RejectedLocalGrade { get; }
        public int RejectedUpwardExcursion { get; }
        public int ChainsTotal { get; }
        public int PrimaryArmsTotal { get; }
        public int BranchesTotal { get; }
        public int EchoesTotal { get; }
        public int ChainCountMin { get; }
        public float ChainCountMean { get; }
        public int ChainCountMax { get; }
        public int PointCountMin { get; }
        public float PointCountMean { get; }
        public int PointCountMax { get; }
        public float PlanarSpanMin { get; }
        public float PlanarSpanMean { get; }
        public float PlanarSpanMax { get; }
        public float VisualDropMin { get; }
        public float VisualDropMean { get; }
        public float VisualDropMax { get; }
        public float MaximumUpwardExcursion { get; }
        public int AcceptedUpwardViolationCount { get; }

        public int RejectedTotal =>
            RejectedSampling +
            RejectedRiver +
            RejectedModifier +
            RejectedBroadSlope +
            RejectedLocalGrade +
            RejectedUpwardExcursion;

        public static GroundPaintedAccentContourClusterDiagnostics Empty =>
            default;
    }

    public readonly struct GroundPaintedAccentContourClusterDebugSnapshot
    {
        public GroundPaintedAccentContourClusterDebugSnapshot(
            GroundPaintedAccentContourClusterCandidate[] candidates,
            GroundPaintedAccentContourClusterRejectionDebugPoint[] rejections,
            GroundPaintedAccentContourClusterDiagnostics diagnostics)
        {
            Candidates =
                candidates ??
                Array.Empty<GroundPaintedAccentContourClusterCandidate>();
            Rejections =
                rejections ??
                Array.Empty<GroundPaintedAccentContourClusterRejectionDebugPoint>();
            Diagnostics = diagnostics;
            isValid = true;
        }

        public GroundPaintedAccentContourClusterCandidate[] Candidates { get; }
        public GroundPaintedAccentContourClusterRejectionDebugPoint[] Rejections
        {
            get;
        }
        public GroundPaintedAccentContourClusterDiagnostics Diagnostics { get; }

        private readonly bool isValid;

        public bool IsValid =>
            isValid && Candidates != null && Rejections != null;

        public static GroundPaintedAccentContourClusterDebugSnapshot Empty =>
            default;
    }

    internal static class GroundPaintedAccentContourClusterCandidateGenerator
    {
        private const float CandidateSelectionFraction = 0.64f;
        private const float RiverSafetyClearance = 0.15f;
        private const float MaximumBroadSlopeDegrees = 45f;
        private const float MaximumLocalGradeDegrees = 40f;
        private const float RejectionMarkerSurfaceOffset = 0.04f;
        private const float MaximumAcceptedUpwardExcursion = 0.0005f;
        private const float TargetSampleSpacing = 0.055f;
        private const int MinimumSampleCount = 17;
        private const int MaximumSampleCount = 49;
        private const float MinimumFreeEndWidthScale = 0.14f;

        public static GroundPaintedAccentContourClusterDebugSnapshot Build(
            IReadOnlyList<GroundPaintedAccentSurfaceStroke> baseStrokes,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 projectionLocalDirection)
        {
            if (baseStrokes == null ||
                baseSurface == null ||
                !baseSurface.IsValid ||
                feature == null)
            {
                return GroundPaintedAccentContourClusterDebugSnapshot.Empty;
            }

            Vector2 localNorth =
                projectionLocalDirection.sqrMagnitude > 0.000001f
                    ? projectionLocalDirection.normalized
                    : Vector2.up;
            Vector2 localEast =
                new Vector2(localNorth.y, -localNorth.x);

            List<GroundPaintedAccentContourClusterCandidate> candidates =
                new List<GroundPaintedAccentContourClusterCandidate>();
            List<GroundPaintedAccentContourClusterRejectionDebugPoint>
                rejections =
                    new List<GroundPaintedAccentContourClusterRejectionDebugPoint>();

            int densitySelected = 0;
            int densitySkipped = 0;
            int rejectedSampling = 0;
            int rejectedRiver = 0;
            int rejectedModifier = 0;
            int rejectedBroadSlope = 0;
            int rejectedLocalGrade = 0;
            int rejectedUpward = 0;
            int chainsTotal = 0;
            int primaryArmsTotal = 0;
            int branchesTotal = 0;
            int echoesTotal = 0;
            int chainCountMinimum = int.MaxValue;
            int chainCountMaximum = 0;
            double chainCountTotal = 0.0;
            int pointCountMinimum = int.MaxValue;
            int pointCountMaximum = 0;
            double pointCountTotal = 0.0;
            int acceptedChainCount = 0;
            float planarSpanMinimum = float.PositiveInfinity;
            float planarSpanMaximum = 0f;
            double planarSpanTotal = 0.0;
            float visualDropMinimum = float.PositiveInfinity;
            float visualDropMaximum = 0f;
            double visualDropTotal = 0.0;
            float maximumUpwardExcursion = 0f;
            int acceptedUpwardViolationCount = 0;

            for (int strokeIndex = 0;
                 strokeIndex < baseStrokes.Count;
                 strokeIndex++)
            {
                GroundPaintedAccentSurfaceStroke stroke = baseStrokes[strokeIndex];
                if (!stroke.IsValid)
                {
                    rejectedSampling++;
                    continue;
                }

                if (Hash01((uint)stroke.Seed, 0xA901u) >
                    CandidateSelectionFraction)
                {
                    densitySkipped++;
                    continue;
                }

                densitySelected++;
                if (TryCreateCandidate(
                        stroke,
                        baseSurface,
                        feature,
                        rivers,
                        modifiers,
                        localNorth,
                        localEast,
                        out GroundPaintedAccentContourClusterCandidate candidate,
                        out GroundPaintedAccentContourClusterRejectionReason reason))
                {
                    candidates.Add(candidate);
                    int chainCount = candidate.Chains.Length;
                    chainCountMinimum = Mathf.Min(chainCountMinimum, chainCount);
                    chainCountMaximum = Mathf.Max(chainCountMaximum, chainCount);
                    chainCountTotal += chainCount;
                    chainsTotal += chainCount;
                    planarSpanMinimum =
                        Mathf.Min(planarSpanMinimum, candidate.PlanarSpan);
                    planarSpanMaximum =
                        Mathf.Max(planarSpanMaximum, candidate.PlanarSpan);
                    planarSpanTotal += candidate.PlanarSpan;
                    visualDropMinimum =
                        Mathf.Min(visualDropMinimum, candidate.VisualDrop);
                    visualDropMaximum =
                        Mathf.Max(visualDropMaximum, candidate.VisualDrop);
                    visualDropTotal += candidate.VisualDrop;
                    maximumUpwardExcursion =
                        Mathf.Max(
                            maximumUpwardExcursion,
                            candidate.MaximumUpwardExcursion);
                    if (candidate.MaximumUpwardExcursion >
                        MaximumAcceptedUpwardExcursion)
                    {
                        acceptedUpwardViolationCount++;
                    }

                    for (int chainIndex = 0;
                         chainIndex < candidate.Chains.Length;
                         chainIndex++)
                    {
                        GroundPaintedAccentContourClusterChain chain =
                            candidate.Chains[chainIndex];
                        int pointCount = chain.LocalSurfacePoints.Length;
                        pointCountMinimum =
                            Mathf.Min(pointCountMinimum, pointCount);
                        pointCountMaximum =
                            Mathf.Max(pointCountMaximum, pointCount);
                        pointCountTotal += pointCount;
                        acceptedChainCount++;

                        switch (chain.Role)
                        {
                            case GroundPaintedAccentContourClusterChainRole.Branch:
                                branchesTotal++;
                                break;
                            case GroundPaintedAccentContourClusterChainRole.Echo:
                                echoesTotal++;
                                break;
                            case GroundPaintedAccentContourClusterChainRole.PrimaryLeftArm:
                            case GroundPaintedAccentContourClusterChainRole.PrimaryRightArm:
                            default:
                                primaryArmsTotal++;
                                break;
                        }
                    }

                    continue;
                }

                switch (reason)
                {
                    case GroundPaintedAccentContourClusterRejectionReason.River:
                        rejectedRiver++;
                        break;
                    case GroundPaintedAccentContourClusterRejectionReason.ModifierExclusion:
                        rejectedModifier++;
                        break;
                    case GroundPaintedAccentContourClusterRejectionReason.BroadSlope:
                        rejectedBroadSlope++;
                        break;
                    case GroundPaintedAccentContourClusterRejectionReason.LocalGrade:
                        rejectedLocalGrade++;
                        break;
                    case GroundPaintedAccentContourClusterRejectionReason.UpwardExcursion:
                        rejectedUpward++;
                        break;
                    case GroundPaintedAccentContourClusterRejectionReason.Sampling:
                    default:
                        rejectedSampling++;
                        break;
                }

                Vector3 markerPosition =
                    stroke.LocalPoints[stroke.LocalPoints.Length / 2];
                markerPosition.y += RejectionMarkerSurfaceOffset;
                rejections.Add(
                    new GroundPaintedAccentContourClusterRejectionDebugPoint(
                        markerPosition,
                        reason));
            }

            int acceptedCount = candidates.Count;
            float candidateDivisor = Mathf.Max(1, acceptedCount);
            float chainDivisor = Mathf.Max(1, acceptedChainCount);
            GroundPaintedAccentContourClusterDiagnostics diagnostics =
                new GroundPaintedAccentContourClusterDiagnostics(
                    baseStrokes.Count,
                    densitySelected,
                    densitySkipped,
                    acceptedCount,
                    rejectedSampling,
                    rejectedRiver,
                    rejectedModifier,
                    rejectedBroadSlope,
                    rejectedLocalGrade,
                    rejectedUpward,
                    chainsTotal,
                    primaryArmsTotal,
                    branchesTotal,
                    echoesTotal,
                    acceptedCount > 0 ? chainCountMinimum : 0,
                    (float)(chainCountTotal / candidateDivisor),
                    chainCountMaximum,
                    acceptedChainCount > 0 ? pointCountMinimum : 0,
                    (float)(pointCountTotal / chainDivisor),
                    pointCountMaximum,
                    acceptedCount > 0 ? planarSpanMinimum : 0f,
                    (float)(planarSpanTotal / candidateDivisor),
                    planarSpanMaximum,
                    acceptedCount > 0 ? visualDropMinimum : 0f,
                    (float)(visualDropTotal / candidateDivisor),
                    visualDropMaximum,
                    maximumUpwardExcursion,
                    acceptedUpwardViolationCount);

            return new GroundPaintedAccentContourClusterDebugSnapshot(
                candidates.ToArray(),
                rejections.ToArray(),
                diagnostics);
        }

        private static bool TryCreateCandidate(
            GroundPaintedAccentSurfaceStroke stroke,
            GroundHeightFieldSnapshot baseSurface,
            GroundSurfaceFeatureRecipe feature,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            Vector2 localNorth,
            Vector2 localEast,
            out GroundPaintedAccentContourClusterCandidate candidate,
            out GroundPaintedAccentContourClusterRejectionReason rejectionReason)
        {
            candidate = default;
            rejectionReason =
                GroundPaintedAccentContourClusterRejectionReason.None;

            Vector3[] strokePoints = stroke.LocalPoints;
            if (strokePoints == null || strokePoints.Length < 2)
            {
                rejectionReason =
                    GroundPaintedAccentContourClusterRejectionReason.Sampling;
                return false;
            }

            Vector3 anchor3 = strokePoints[strokePoints.Length / 2];
            Vector2 anchor = new Vector2(anchor3.x, anchor3.z);
            Vector2 descriptorDirection =
                new Vector2(
                    strokePoints[strokePoints.Length - 1].x -
                        strokePoints[0].x,
                    strokePoints[strokePoints.Length - 1].z -
                        strokePoints[0].z);
            if (Vector2.Dot(descriptorDirection, localEast) < 0f)
            {
                localEast = -localEast;
            }

            float sourceLength = ResolvePlanarLength(strokePoints);
            if (sourceLength <= 0.0001f)
            {
                rejectionReason =
                    GroundPaintedAccentContourClusterRejectionReason.Sampling;
                return false;
            }

            float irregularity = feature.PaintedAccentFoldIrregularity;
            float widthHalf = Mathf.Max(0.0025f, stroke.Width * 0.5f);
            float visualScale = Mathf.Clamp(
                Mathf.Max(
                    sourceLength * Mathf.Lerp(0.15f, 0.24f, irregularity),
                    feature.PaintedAccentFoldHeight * 0.85f + 0.045f),
                0.08f,
                0.58f);
            float rootLift =
                visualScale *
                Mathf.Lerp(
                    0.16f,
                    0.42f,
                    Hash01((uint)stroke.Seed, 0xA911u));
            Vector2 highRoot = anchor + localNorth * rootLift;

            float asymmetry =
                HashSigned((uint)stroke.Seed, 0xA913u) *
                Mathf.Lerp(0.18f, 0.42f, irregularity);
            float leftLength =
                sourceLength *
                Mathf.Clamp(
                    Mathf.Lerp(
                        0.78f,
                        1.34f,
                        Hash01((uint)stroke.Seed, 0xA917u)) *
                    (1f - asymmetry),
                    0.52f,
                    1.55f);
            float rightLength =
                sourceLength *
                Mathf.Clamp(
                    Mathf.Lerp(
                        0.78f,
                        1.34f,
                        Hash01((uint)stroke.Seed, 0xA919u)) *
                    (1f + asymmetry),
                    0.52f,
                    1.55f);
            float leftDrop =
                visualScale *
                Mathf.Lerp(
                    0.58f,
                    1.18f,
                    Hash01((uint)stroke.Seed, 0xA91Du));
            float rightDrop =
                visualScale *
                Mathf.Lerp(
                    0.58f,
                    1.18f,
                    Hash01((uint)stroke.Seed, 0xA91Fu));

            List<PlanarChain> planarChains = new List<PlanarChain>(6);
            PlanarChain leftArm = BuildPrimaryArm(
                highRoot,
                -localEast,
                localNorth,
                leftLength,
                leftDrop,
                widthHalf,
                stroke.Seed,
                0xA929u,
                GroundPaintedAccentContourClusterChainRole.PrimaryLeftArm);
            PlanarChain rightArm = BuildPrimaryArm(
                highRoot,
                localEast,
                localNorth,
                rightLength,
                rightDrop,
                widthHalf,
                stroke.Seed,
                0xA92Bu,
                GroundPaintedAccentContourClusterChainRole.PrimaryRightArm);
            if (!leftArm.IsValid || !rightArm.IsValid)
            {
                rejectionReason =
                    GroundPaintedAccentContourClusterRejectionReason.Sampling;
                return false;
            }

            planarChains.Add(leftArm);
            planarChains.Add(rightArm);

            float branchRoll = Hash01((uint)stroke.Seed, 0xA931u);
            int branchCount =
                branchRoll < 0.16f
                    ? 0
                    : branchRoll < 0.70f
                        ? 1
                        : 2;
            for (int branchIndex = 0;
                 branchIndex < branchCount;
                 branchIndex++)
            {
                bool useLeft =
                    branchCount == 2
                        ? branchIndex == 0
                        : Hash01((uint)stroke.Seed, 0xA937u) < 0.5f;
                int parentIndex = useLeft ? 0 : 1;
                PlanarChain parent = planarChains[parentIndex];
                PlanarChain branch = BuildBranch(
                    parent,
                    localNorth,
                    sourceLength,
                    visualScale,
                    widthHalf,
                    stroke.Seed,
                    branchIndex,
                    parentIndex);
                if (branch.IsValid)
                {
                    planarChains.Add(branch);
                }
            }

            if (Hash01((uint)stroke.Seed, 0xA941u) < 0.42f)
            {
                int parentIndex =
                    Hash01((uint)stroke.Seed, 0xA943u) < 0.5f ? 0 : 1;
                PlanarChain echo = BuildEcho(
                    planarChains[parentIndex],
                    localNorth,
                    visualScale,
                    widthHalf,
                    stroke.Seed,
                    parentIndex);
                if (echo.IsValid)
                {
                    planarChains.Add(echo);
                }
            }

            float maximumUpwardExcursion = 0f;
            for (int chainIndex = 0;
                 chainIndex < planarChains.Count;
                 chainIndex++)
            {
                float chainExcursion = ResolveMaximumUpwardExcursion(
                    planarChains[chainIndex].Points,
                    localNorth);
                maximumUpwardExcursion =
                    Mathf.Max(maximumUpwardExcursion, chainExcursion);
            }

            if (maximumUpwardExcursion > MaximumAcceptedUpwardExcursion)
            {
                rejectionReason =
                    GroundPaintedAccentContourClusterRejectionReason.UpwardExcursion;
                return false;
            }

            GroundPaintedAccentContourClusterChain[] sampledChains =
                new GroundPaintedAccentContourClusterChain[planarChains.Count];
            for (int chainIndex = 0;
                 chainIndex < planarChains.Count;
                 chainIndex++)
            {
                PlanarChain planarChain = planarChains[chainIndex];
                if (!TryValidateAndSampleChain(
                        planarChain.Points,
                        planarChain.HalfWidths,
                        baseSurface,
                        rivers,
                        modifiers,
                        out Vector3[] localSurfacePoints,
                        out rejectionReason))
                {
                    return false;
                }

                sampledChains[chainIndex] =
                    new GroundPaintedAccentContourClusterChain(
                        localSurfacePoints,
                        planarChain.HalfWidths,
                        planarChain.Role,
                        planarChain.ParentChainIndex);
            }

            Vector3 highPoint = sampledChains[0].LocalSurfacePoints[0];
            float planarSpan = ResolvePlanarSpan(planarChains, localEast);
            float visualDrop = ResolveVisualDrop(planarChains, localNorth);
            candidate =
                new GroundPaintedAccentContourClusterCandidate(
                    sampledChains,
                    anchor3,
                    highPoint,
                    stroke.Seed,
                    maximumUpwardExcursion,
                    planarSpan,
                    visualDrop);
            return candidate.IsValid;
        }

        private static PlanarChain BuildPrimaryArm(
            Vector2 root,
            Vector2 outwardAxis,
            Vector2 localNorth,
            float totalLength,
            float totalDrop,
            float baseHalfWidth,
            int seed,
            uint salt,
            GroundPaintedAccentContourClusterChainRole role)
        {
            int controlCount =
                Mathf.Clamp(
                    5 + Mathf.FloorToInt(Hash01((uint)seed, salt) * 4f),
                    5,
                    8);
            float[] outward = new float[controlCount];
            float[] drop = new float[controlCount];
            float[] outwardWeights = new float[controlCount - 1];
            float[] dropWeights = new float[controlCount - 1];
            float outwardWeightTotal = 0f;
            float dropWeightTotal = 0f;

            for (int index = 0; index < controlCount - 1; index++)
            {
                float position = (index + 0.5f) / (controlCount - 1f);
                float outwardWeight =
                    Mathf.Lerp(
                        0.72f,
                        1.28f,
                        Hash01((uint)seed, salt + (uint)(index * 17 + 3)));
                float dropWeight =
                    Mathf.Pow(
                        Hash01(
                            (uint)seed,
                            salt + (uint)(index * 19 + 7)),
                        1.45f);

                if (index == 0)
                {
                    dropWeight *= Mathf.Lerp(0.04f, 0.18f, position);
                }
                else if (index == 1)
                {
                    dropWeight *= 0.45f;
                }

                outwardWeights[index] = outwardWeight;
                dropWeights[index] = Mathf.Max(0.002f, dropWeight);
                outwardWeightTotal += outwardWeights[index];
                dropWeightTotal += dropWeights[index];
            }

            float accumulatedOutward = 0f;
            float accumulatedDrop = 0f;
            for (int index = 1; index < controlCount; index++)
            {
                accumulatedOutward +=
                    totalLength *
                    outwardWeights[index - 1] /
                    Mathf.Max(0.0001f, outwardWeightTotal);
                accumulatedDrop +=
                    totalDrop *
                    dropWeights[index - 1] /
                    Mathf.Max(0.0001f, dropWeightTotal);
                outward[index] = accumulatedOutward;
                drop[index] = accumulatedDrop;
            }

            Vector2[] points = SampleDirectedMonotoneChain(
                root,
                outwardAxis,
                localNorth,
                outward,
                drop);
            float[] halfWidths = BuildHalfWidths(
                points.Length,
                baseHalfWidth,
                taperStart: false,
                taperEnd: true,
                roleScale: 1f);
            return new PlanarChain(points, halfWidths, role, -1);
        }

        private static PlanarChain BuildBranch(
            PlanarChain parent,
            Vector2 localNorth,
            float sourceLength,
            float visualScale,
            float baseHalfWidth,
            int seed,
            int branchIndex,
            int parentChainIndex)
        {
            if (!parent.IsValid || parent.Points.Length < 8)
            {
                return default;
            }

            uint baseSalt = (uint)(0xA951 + branchIndex * 0x40);
            float attachT =
                Mathf.Lerp(
                    0.28f,
                    0.66f,
                    Hash01((uint)seed, baseSalt + 1u));
            int attachIndex = Mathf.Clamp(
                Mathf.RoundToInt(attachT * (parent.Points.Length - 1)),
                2,
                parent.Points.Length - 4);
            Vector2 root = parent.Points[attachIndex];
            Vector2 tangent =
                parent.Points[attachIndex + 1] -
                parent.Points[attachIndex - 1];
            if (tangent.sqrMagnitude <= 0.000001f)
            {
                return default;
            }

            tangent.Normalize();
            float tangentNorth = Vector2.Dot(tangent, localNorth);
            if (tangentNorth > 0f)
            {
                tangent -= localNorth * tangentNorth;
                tangent.Normalize();
            }

            Vector2 outwardAxis =
                tangent - localNorth * Vector2.Dot(tangent, localNorth);
            if (outwardAxis.sqrMagnitude <= 0.000001f)
            {
                return default;
            }

            outwardAxis.Normalize();
            float totalLength =
                sourceLength *
                Mathf.Lerp(
                    0.34f,
                    0.78f,
                    Hash01((uint)seed, baseSalt + 3u));
            float totalDrop =
                visualScale *
                Mathf.Lerp(
                    0.24f,
                    0.72f,
                    Hash01((uint)seed, baseSalt + 5u));
            int controlCount =
                Mathf.Clamp(
                    4 + Mathf.FloorToInt(
                        Hash01((uint)seed, baseSalt + 7u) * 3f),
                    4,
                    6);
            float[] outward = new float[controlCount];
            float[] drop = new float[controlCount];
            float initialStep = totalLength * 0.16f;
            float parentDropSlope =
                Mathf.Max(0f, -Vector2.Dot(tangent, localNorth)) /
                Mathf.Max(
                    0.08f,
                    Mathf.Abs(Vector2.Dot(tangent, outwardAxis)));
            outward[1] = initialStep;
            drop[1] =
                Mathf.Min(
                    totalDrop * 0.28f,
                    initialStep * parentDropSlope);

            for (int index = 2; index < controlCount; index++)
            {
                float t = (index - 1f) / (controlCount - 2f);
                float shaped = ResolveSmootherStep01(t);
                float outwardJitter =
                    HashSigned((uint)seed, baseSalt + (uint)(index * 11)) *
                    totalLength * 0.035f;
                outward[index] =
                    Mathf.Max(
                        outward[index - 1] + totalLength * 0.08f,
                        Mathf.Lerp(initialStep, totalLength, t) +
                        outwardJitter);
                drop[index] =
                    Mathf.Max(
                        drop[index - 1],
                        Mathf.Lerp(drop[1], totalDrop, shaped));
            }

            outward[controlCount - 1] = totalLength;
            drop[controlCount - 1] = totalDrop;
            Vector2[] points = SampleDirectedMonotoneChain(
                root,
                outwardAxis,
                localNorth,
                outward,
                drop);
            float[] halfWidths = BuildHalfWidths(
                points.Length,
                baseHalfWidth,
                taperStart: false,
                taperEnd: true,
                roleScale: 0.76f);
            return new PlanarChain(
                points,
                halfWidths,
                GroundPaintedAccentContourClusterChainRole.Branch,
                parentChainIndex);
        }

        private static PlanarChain BuildEcho(
            PlanarChain parent,
            Vector2 localNorth,
            float visualScale,
            float baseHalfWidth,
            int seed,
            int parentChainIndex)
        {
            if (!parent.IsValid || parent.Points.Length < 12)
            {
                return default;
            }

            float startT =
                Mathf.Lerp(
                    0.20f,
                    0.46f,
                    Hash01((uint)seed, 0xA971u));
            float lengthT =
                Mathf.Lerp(
                    0.34f,
                    0.56f,
                    Hash01((uint)seed, 0xA973u));
            float endT = Mathf.Min(0.94f, startT + lengthT);
            int startIndex = Mathf.Clamp(
                Mathf.FloorToInt(startT * (parent.Points.Length - 1)),
                1,
                parent.Points.Length - 5);
            int endIndex = Mathf.Clamp(
                Mathf.CeilToInt(endT * (parent.Points.Length - 1)),
                startIndex + 3,
                parent.Points.Length - 2);
            int pointCount = endIndex - startIndex + 1;
            Vector2[] points = new Vector2[pointCount];
            float downOffset =
                visualScale *
                Mathf.Lerp(
                    0.18f,
                    0.36f,
                    Hash01((uint)seed, 0xA977u));
            for (int index = 0; index < pointCount; index++)
            {
                points[index] =
                    parent.Points[startIndex + index] -
                    localNorth * downOffset;
            }

            float[] halfWidths = BuildHalfWidths(
                pointCount,
                baseHalfWidth,
                taperStart: true,
                taperEnd: true,
                roleScale: 0.64f);
            return new PlanarChain(
                points,
                halfWidths,
                GroundPaintedAccentContourClusterChainRole.Echo,
                parentChainIndex);
        }

        private static Vector2[] SampleDirectedMonotoneChain(
            Vector2 root,
            Vector2 outwardAxis,
            Vector2 localNorth,
            float[] outwardControls,
            float[] dropControls)
        {
            if (outwardControls == null ||
                dropControls == null ||
                outwardControls.Length < 2 ||
                outwardControls.Length != dropControls.Length)
            {
                return Array.Empty<Vector2>();
            }

            float[] outwardTangents =
                ResolveMonotoneTangents(outwardControls);
            float[] dropTangents = ResolveMonotoneTangents(dropControls);
            float estimatedLength = 0f;
            for (int index = 1; index < outwardControls.Length; index++)
            {
                estimatedLength += Vector2.Distance(
                    new Vector2(
                        outwardControls[index - 1],
                        dropControls[index - 1]),
                    new Vector2(
                        outwardControls[index],
                        dropControls[index]));
            }

            int sampleCount = Mathf.Clamp(
                Mathf.CeilToInt(
                    estimatedLength / Mathf.Max(0.01f, TargetSampleSpacing)) +
                1,
                MinimumSampleCount,
                MaximumSampleCount);
            Vector2[] points = new Vector2[sampleCount];
            int segmentCount = outwardControls.Length - 1;

            for (int sampleIndex = 0;
                 sampleIndex < sampleCount;
                 sampleIndex++)
            {
                float global =
                    sampleIndex / (float)(sampleCount - 1) * segmentCount;
                int segmentIndex = Mathf.Min(
                    segmentCount - 1,
                    Mathf.FloorToInt(global));
                float t = Mathf.Clamp01(global - segmentIndex);
                float outward = EvaluateHermite(
                    outwardControls[segmentIndex],
                    outwardControls[segmentIndex + 1],
                    outwardTangents[segmentIndex],
                    outwardTangents[segmentIndex + 1],
                    t);
                float drop = EvaluateHermite(
                    dropControls[segmentIndex],
                    dropControls[segmentIndex + 1],
                    dropTangents[segmentIndex],
                    dropTangents[segmentIndex + 1],
                    t);
                outward = Mathf.Clamp(
                    outward,
                    outwardControls[segmentIndex],
                    outwardControls[segmentIndex + 1]);
                drop = Mathf.Clamp(
                    drop,
                    dropControls[segmentIndex],
                    dropControls[segmentIndex + 1]);
                points[sampleIndex] =
                    root + outwardAxis * outward - localNorth * drop;
            }

            points[0] = root;
            points[sampleCount - 1] =
                root +
                outwardAxis * outwardControls[outwardControls.Length - 1] -
                localNorth * dropControls[dropControls.Length - 1];
            return points;
        }

        private static float[] ResolveMonotoneTangents(float[] values)
        {
            int count = values != null ? values.Length : 0;
            if (count < 2)
            {
                return Array.Empty<float>();
            }

            float[] secants = new float[count - 1];
            for (int index = 0; index < count - 1; index++)
            {
                secants[index] = values[index + 1] - values[index];
            }

            float[] tangents = new float[count];
            tangents[0] = secants[0];
            tangents[count - 1] = secants[count - 2];
            for (int index = 1; index < count - 1; index++)
            {
                float before = secants[index - 1];
                float after = secants[index];
                if (before <= 0f || after <= 0f)
                {
                    tangents[index] = 0f;
                }
                else
                {
                    tangents[index] =
                        2f * before * after /
                        Mathf.Max(0.000001f, before + after);
                }
            }

            for (int index = 0; index < count - 1; index++)
            {
                float secant = secants[index];
                if (secant <= 0.000001f)
                {
                    tangents[index] = 0f;
                    tangents[index + 1] = 0f;
                    continue;
                }

                float alpha = tangents[index] / secant;
                float beta = tangents[index + 1] / secant;
                float magnitude = alpha * alpha + beta * beta;
                if (magnitude <= 9f)
                {
                    continue;
                }

                float scale = 3f / Mathf.Sqrt(magnitude);
                tangents[index] = scale * alpha * secant;
                tangents[index + 1] = scale * beta * secant;
            }

            return tangents;
        }

        private static float EvaluateHermite(
            float value0,
            float value1,
            float tangent0,
            float tangent1,
            float t)
        {
            float tt = t * t;
            float ttt = tt * t;
            float h00 = 2f * ttt - 3f * tt + 1f;
            float h10 = ttt - 2f * tt + t;
            float h01 = -2f * ttt + 3f * tt;
            float h11 = ttt - tt;
            return
                h00 * value0 +
                h10 * tangent0 +
                h01 * value1 +
                h11 * tangent1;
        }

        private static float[] BuildHalfWidths(
            int pointCount,
            float baseHalfWidth,
            bool taperStart,
            bool taperEnd,
            float roleScale)
        {
            if (pointCount < 2)
            {
                return Array.Empty<float>();
            }

            float[] halfWidths = new float[pointCount];
            float scaledWidth =
                Mathf.Max(0.001f, baseHalfWidth * roleScale);
            for (int pointIndex = 0;
                 pointIndex < pointCount;
                 pointIndex++)
            {
                float t = pointIndex / (float)(pointCount - 1);
                float startEnvelope =
                    taperStart
                        ? ResolveSmootherStep01(t / 0.16f)
                        : 1f;
                float endEnvelope =
                    taperEnd
                        ? ResolveSmootherStep01((1f - t) / 0.16f)
                        : 1f;
                float envelope = Mathf.Clamp01(startEnvelope * endEnvelope);
                halfWidths[pointIndex] =
                    scaledWidth *
                    Mathf.Lerp(
                        MinimumFreeEndWidthScale,
                        1f,
                        envelope);
            }

            return halfWidths;
        }

        private static bool TryValidateAndSampleChain(
            IReadOnlyList<Vector2> planarPoints,
            IReadOnlyList<float> halfWidths,
            GroundHeightFieldSnapshot baseSurface,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            out Vector3[] localSurfacePoints,
            out GroundPaintedAccentContourClusterRejectionReason rejectionReason)
        {
            localSurfacePoints = Array.Empty<Vector3>();
            rejectionReason =
                GroundPaintedAccentContourClusterRejectionReason.None;

            if (planarPoints == null ||
                halfWidths == null ||
                planarPoints.Count < 2 ||
                planarPoints.Count != halfWidths.Count)
            {
                rejectionReason =
                    GroundPaintedAccentContourClusterRejectionReason.Sampling;
                return false;
            }

            Vector3[] sampledPoints = new Vector3[planarPoints.Count];
            bool hasPreviousCenter = false;
            Vector2 previousCenterXZ = Vector2.zero;
            float previousCenterHeight = 0f;

            for (int pointIndex = 0;
                 pointIndex < planarPoints.Count;
                 pointIndex++)
            {
                Vector2 centerXZ = planarPoints[pointIndex];
                Vector2 tangent = ResolveTangent(planarPoints, pointIndex);
                Vector2 sideAxis = new Vector2(-tangent.y, tangent.x);
                float halfWidth = Mathf.Max(0.0005f, halfWidths[pointIndex]);
                Vector2 leftXZ = centerXZ - sideAxis * halfWidth;
                Vector2 rightXZ = centerXZ + sideAxis * halfWidth;

                if (!baseSurface.TrySample(
                        centerXZ,
                        out GroundSurfaceSample centerSample) ||
                    !baseSurface.TrySample(
                        leftXZ,
                        out GroundSurfaceSample leftSample) ||
                    !baseSurface.TrySample(
                        rightXZ,
                        out GroundSurfaceSample rightSample))
                {
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.Sampling;
                    return false;
                }

                if (ExceedsBroadSlope(centerSample) ||
                    ExceedsBroadSlope(leftSample) ||
                    ExceedsBroadSlope(rightSample))
                {
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.BroadSlope;
                    return false;
                }

                if (IsExcludedByRiver(
                        centerXZ,
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
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.River;
                    return false;
                }

                if (IsExcludedByModifier(centerXZ, modifiers) ||
                    IsExcludedByModifier(leftXZ, modifiers) ||
                    IsExcludedByModifier(rightXZ, modifiers))
                {
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.ModifierExclusion;
                    return false;
                }

                if (ExceedsGrade(
                        leftSample.Height,
                        centerSample.Height,
                        Vector2.Distance(leftXZ, centerXZ)) ||
                    ExceedsGrade(
                        centerSample.Height,
                        rightSample.Height,
                        Vector2.Distance(centerXZ, rightXZ)))
                {
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.LocalGrade;
                    return false;
                }

                if (hasPreviousCenter &&
                    ExceedsGrade(
                        previousCenterHeight,
                        centerSample.Height,
                        Vector2.Distance(previousCenterXZ, centerXZ)))
                {
                    rejectionReason =
                        GroundPaintedAccentContourClusterRejectionReason.LocalGrade;
                    return false;
                }

                sampledPoints[pointIndex] =
                    new Vector3(centerXZ.x, centerSample.Height, centerXZ.y);
                hasPreviousCenter = true;
                previousCenterXZ = centerXZ;
                previousCenterHeight = centerSample.Height;
            }

            localSurfacePoints = sampledPoints;
            return true;
        }

        private static Vector2 ResolveTangent(
            IReadOnlyList<Vector2> points,
            int pointIndex)
        {
            int beforeIndex = Mathf.Max(0, pointIndex - 1);
            int afterIndex = Mathf.Min(points.Count - 1, pointIndex + 1);
            Vector2 tangent = points[afterIndex] - points[beforeIndex];
            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : Vector2.right;
        }

        private static float ResolveMaximumUpwardExcursion(
            IReadOnlyList<Vector2> points,
            Vector2 localNorth)
        {
            if (points == null || points.Count < 2)
            {
                return 0f;
            }

            float maximum = 0f;
            float previousHeight = Vector2.Dot(points[0], localNorth);
            for (int pointIndex = 1;
                 pointIndex < points.Count;
                 pointIndex++)
            {
                float height = Vector2.Dot(points[pointIndex], localNorth);
                maximum = Mathf.Max(maximum, height - previousHeight);
                previousHeight = height;
            }

            return maximum;
        }

        private static float ResolvePlanarLength(Vector3[] points)
        {
            float length = 0f;
            for (int pointIndex = 1;
                 points != null && pointIndex < points.Length;
                 pointIndex++)
            {
                length += Vector2.Distance(
                    new Vector2(
                        points[pointIndex - 1].x,
                        points[pointIndex - 1].z),
                    new Vector2(
                        points[pointIndex].x,
                        points[pointIndex].z));
            }

            return length;
        }

        private static float ResolvePlanarSpan(
            IReadOnlyList<PlanarChain> chains,
            Vector2 localEast)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int chainIndex = 0;
                 chains != null && chainIndex < chains.Count;
                 chainIndex++)
            {
                Vector2[] points = chains[chainIndex].Points;
                for (int pointIndex = 0;
                     points != null && pointIndex < points.Length;
                     pointIndex++)
                {
                    float coordinate = Vector2.Dot(points[pointIndex], localEast);
                    minimum = Mathf.Min(minimum, coordinate);
                    maximum = Mathf.Max(maximum, coordinate);
                }
            }

            return
                float.IsPositiveInfinity(minimum) ||
                float.IsNegativeInfinity(maximum)
                    ? 0f
                    : Mathf.Max(0f, maximum - minimum);
        }

        private static float ResolveVisualDrop(
            IReadOnlyList<PlanarChain> chains,
            Vector2 localNorth)
        {
            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int chainIndex = 0;
                 chains != null && chainIndex < chains.Count;
                 chainIndex++)
            {
                Vector2[] points = chains[chainIndex].Points;
                for (int pointIndex = 0;
                     points != null && pointIndex < points.Length;
                     pointIndex++)
                {
                    float coordinate = Vector2.Dot(points[pointIndex], localNorth);
                    minimum = Mathf.Min(minimum, coordinate);
                    maximum = Mathf.Max(maximum, coordinate);
                }
            }

            return
                float.IsPositiveInfinity(minimum) ||
                float.IsNegativeInfinity(maximum)
                    ? 0f
                    : Mathf.Max(0f, maximum - minimum);
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

        private static float ResolveSmootherStep01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        private static float HashSigned(uint seed, uint salt)
        {
            return Hash01(seed, salt) * 2f - 1f;
        }

        private static float Hash01(uint seed, uint salt)
        {
            uint value = seed ^ (salt * 0x9E3779B9u);
            value ^= value >> 16;
            value *= 0x7FEB352Du;
            value ^= value >> 15;
            value *= 0x846CA68Bu;
            value ^= value >> 16;
            return (value & 0x00FFFFFFu) / 16777216f;
        }

        private readonly struct PlanarChain
        {
            public PlanarChain(
                Vector2[] points,
                float[] halfWidths,
                GroundPaintedAccentContourClusterChainRole role,
                int parentChainIndex)
            {
                Points = points ?? Array.Empty<Vector2>();
                HalfWidths = halfWidths ?? Array.Empty<float>();
                Role = role;
                ParentChainIndex = parentChainIndex;
            }

            public Vector2[] Points { get; }
            public float[] HalfWidths { get; }
            public GroundPaintedAccentContourClusterChainRole Role { get; }
            public int ParentChainIndex { get; }

            public bool IsValid =>
                Points != null &&
                HalfWidths != null &&
                Points.Length >= 2 &&
                HalfWidths.Length == Points.Length;
        }
    }
}
