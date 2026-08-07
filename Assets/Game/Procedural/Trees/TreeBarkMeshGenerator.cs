using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace ProgrammaticStylized3D.Trees
{
    public sealed class TreeBarkMeshBranchGeometryAccounting
    {
        public int StableBranchId { get; internal set; }
        public int BranchOrder { get; internal set; }
        public int SourceSampleCount { get; internal set; }
        public int RenderRingCount { get; internal set; }
        public int RadialSegments { get; internal set; }
        public int MinimumRadialSegments { get; internal set; }
        public int MaximumRadialSegments { get; internal set; }
        public float AverageRadialSegments { get; internal set; }
        public int RadialTransitionCount { get; internal set; }
        public int MixedResolutionStripCount { get; internal set; }
        public int StitchTriangleCount { get; internal set; }
        public float RootLobeAverageRadialSegments { get; internal set; }
        public int GroundContactRadialSegments { get; internal set; }
        public int GroundContactBoostedRingCount { get; internal set; }
        public float GroundContactBoostReleaseNormalizedDistance { get; internal set; }
        public float ButtressPersistenceAverageRadialSegments { get; internal set; }
        public float OrdinaryTrunkAverageRadialSegments { get; internal set; }
        public int SideVertexCount { get; internal set; }
        public int SideTriangleCount { get; internal set; }
        public int CapVertexCount { get; internal set; }
        public int CapTriangleCount { get; internal set; }
        public int SeamDuplicateVertexCount { get; internal set; }
        public int RootZoneRingCount { get; internal set; }
        public int RootZoneIntervalCount { get; internal set; }
        public int RootZoneVertexCount { get; internal set; }
        public int RootZoneTriangleCount { get; internal set; }
        public int RootLobeRingCount { get; internal set; }
        public int RootLobeIntervalCount { get; internal set; }
        public int RootLobeVertexCount { get; internal set; }
        public int RootLobeTriangleCount { get; internal set; }
        public int ButtressPersistenceRingCount { get; internal set; }
        public int ButtressPersistenceIntervalCount { get; internal set; }
        public int ButtressPersistenceVertexCount { get; internal set; }
        public int ButtressPersistenceTriangleCount { get; internal set; }
        public int OrdinaryTrunkVertexCount { get; internal set; }
        public int OrdinaryTrunkTriangleCount { get; internal set; }
        public int InsertedRenderRingCount { get; internal set; }
        public int RootRefinementInsertedRingCount { get; internal set; }
        public int TwistRefinementInsertedRingCount { get; internal set; }
        public int AdaptiveShapeRefinementInsertedRingCount { get; internal set; }
        public int RemovedRenderRingCount { get; internal set; }
        public int EfficiencyPolicyRemovedRingCount { get; internal set; }
        public int TopologyRepairRemovedRingCount { get; internal set; }
        public float AverageSegmentLength { get; internal set; }
        public float MaximumSegmentLength { get; internal set; }
        public float AverageTurnDegrees { get; internal set; }
        public float MaximumTurnDegrees { get; internal set; }
    }

    public sealed class TreeBarkMeshBuildResult
    {
        public bool Passed { get; internal set; }
        public int MeshedBranchCount { get; internal set; }
        public int VertexCount { get; internal set; }
        public int TriangleCount { get; internal set; }
        public int TipCapCount { get; internal set; }
        public int AlternateQuadDiagonalCount { get; internal set; }
        public int PhaseAlignedRingCount { get; internal set; }
        public int CurvatureRadiusClampCount { get; internal set; }
        public int CircularBranchRingRemovalCount { get; internal set; }
        public bool TrunkTipClosureApplied { get; internal set; }
        public int TrunkTipRemovedRingCount { get; internal set; }
        public float TrunkTipClosureLength { get; internal set; }
        public int EffectiveTrunkRadialSegments { get; internal set; }
        public int MinimumEffectiveTrunkRadialSegments { get; internal set; }
        public int MaximumEffectiveTrunkRadialSegments { get; internal set; }
        public float AverageEffectiveTrunkRadialSegments { get; internal set; }
        public int TrunkRadialTransitionCount { get; internal set; }
        public int TrunkMixedResolutionStripCount { get; internal set; }
        public int TrunkStitchTriangleCount { get; internal set; }
        public int EffectiveTrunkRingCount { get; internal set; }
        public int RootZoneLongitudinalIntervals { get; internal set; }
        public float ButtressSamplesPerLobe { get; internal set; }
        public float GroundButtressCrestMultiplier { get; internal set; }
        public float HalfHeightButtressCrestMultiplier { get; internal set; }
        public float HalfHeightRootExtensionRatio { get; internal set; }
        public float HalfHeightButtressAngularWidthScale { get; internal set; }
        public float GroundRootHalfExtensionAngularWidthDegrees { get; internal set; }
        public float GroundRootHalfExtensionChordWidth { get; internal set; }
        public float RequestedRootSupportAngularWidthDegrees { get; internal set; }
        public float EmittedRootSupportAngularWidthDegrees { get; internal set; }
        public bool RootSupportWidthClampedByCount { get; internal set; }
        public float EvaluatedRootThickness { get; internal set; }
        public float GroundRootBaseMergeFactor { get; internal set; }
        public float RootFootShapePlateauEndNormalized { get; internal set; }
        public float RootTopRootOnlyMultiplier { get; internal set; }
        public float AuthoredRootHeightNormalized { get; internal set; }
        public float EffectiveRootTransitionHeightNormalized { get; internal set; }
        public float RootTransitionSafetyTailNormalized { get; internal set; }
        public float RootGroundPlateauEndNormalized { get; internal set; }
        public float RootLobeCollapseEndNormalized { get; internal set; }
        public float MaximumGroundButtressCrestTurnDegrees { get; internal set; }
        public float PathSpiralStrength { get; internal set; }
        public float PathSpiralTurns { get; internal set; }
        public float PathSpiralDirection { get; internal set; }
        public float MaximumPathSpiralRadius { get; internal set; }
        public float MaximumCrossSectionMultiplier { get; internal set; }
        public float MinimumGroundCrossSectionMultiplier { get; internal set; }
        public float GeneratedRootWidth { get; internal set; }
        public float GeneratedRootDepth { get; internal set; }
        public float RequestedAxialTwistDegrees { get; internal set; }
        public float MeasuredAxialTwistDegrees { get; internal set; }
        public float AxialTwistErrorDegrees { get; internal set; }
        public float AxialTwistTurns { get; internal set; }
        public float FirstAuthoredAxialTwistNormalizedDistance
            { get; internal set; }
        public float AxialTwistAtGroundPlateauEndDegrees
            { get; internal set; }
        public float AxialTwistAtRootCollapseEndDegrees
            { get; internal set; }
        public float AxialTwistAtEarliestRootTransitionDegrees
            { get; internal set; }
        public float AxialTwistAtEffectiveRootTransitionDegrees
            { get; internal set; }
        public float MaximumAuthoredAxialTwistStepDegrees
            { get; internal set; }
        public float MaximumAllowedAxialTwistStepDegrees
            { get; internal set; }
        public float MaximumAuthoredAxialTwistStepStartNormalizedDistance
            { get; internal set; }
        public float MaximumAuthoredAxialTwistStepEndNormalizedDistance
            { get; internal set; }
        public float RootTrunkBoundaryMaximumMismatch { get; internal set; }
        public bool RootTrunkBoundaryMismatchEvaluated { get; internal set; }
        public bool RootTrunkBoundaryCandidateActivated { get; internal set; }
        public int RootTrunkBoundaryMorphRingsRequested { get; internal set; }
        public int RootTrunkBoundaryMorphRingsUsed { get; internal set; }
        public string FailureStage { get; internal set; }
        public Bounds LocalBounds { get; internal set; }
        public string InputFingerprint { get; internal set; }
        public string GeometryFingerprint { get; internal set; }
        public TreeBarkMeshEfficiencyPolicy EfficiencyPolicy { get; internal set; }
        public IReadOnlyList<TreeBarkMeshBranchGeometryAccounting>
            BranchGeometryAccounting { get; internal set; }
        public double GeometryBuildMilliseconds { get; internal set; }
        public double TopologyAuditMilliseconds { get; internal set; }
        public double MeshUploadMilliseconds { get; internal set; }
        public double TotalBuildMilliseconds { get; internal set; }
        public long EstimatedMeshBytes { get; internal set; }
        public bool RepeatabilityPassed { get; internal set; }
        public TreeBarkMeshTopologyAuditResult TopologyAudit { get; internal set; }
        public string Failure { get; internal set; }

        public void MarkRepeatabilityPassed()
        {
            RepeatabilityPassed = true;
        }

        public void MarkFailed(string failure)
        {
            Passed = false;
            RepeatabilityPassed = false;
            Failure = failure ?? string.Empty;
        }
    }

    public enum TreeRootCollapseTournamentStrategy
    {
        Production = 0,
        ImmediateFrameRelease = 1,
        BoundedFrameRelease = 2,
        DenseFrameAdoptionResampling = 3,
        TransportedContourBlend = 4
    }


    internal readonly struct TreeRootCollapseTournamentProfile
    {
        internal TreeRootCollapseTournamentProfile(
            float minimumPhysicalMetres,
            float radiusFactor,
            int collapseIntervals,
            bool useSmoothstep,
            float footExponent,
            bool exactZeroBeforeAdoption,
            int boundaryMorphRings)
        {
            MinimumPhysicalMetres = minimumPhysicalMetres;
            RadiusFactor = radiusFactor;
            CollapseIntervals = collapseIntervals;
            UseSmoothstep = useSmoothstep;
            FootExponent = footExponent;
            ExactZeroBeforeAdoption = exactZeroBeforeAdoption;
            BoundaryMorphRings = boundaryMorphRings;
        }

        internal float MinimumPhysicalMetres { get; }
        internal float RadiusFactor { get; }
        internal int CollapseIntervals { get; }
        internal bool UseSmoothstep { get; }
        internal float FootExponent { get; }
        internal bool ExactZeroBeforeAdoption { get; }
        internal int BoundaryMorphRings { get; }
    }

    public static class TreeBarkMeshGenerator
    {
        public const int BarkAlgorithmVersion = 28;
        private const float TwoPi = Mathf.PI * 2f;
        private const float Epsilon = 0.000001f;
        private const float TriangleAreaSquaredEpsilon = 0.0000000001f;
        [ThreadStatic]
        private static TreeRootCollapseTournamentStrategy?
            activeTournamentStrategy;
        [ThreadStatic] private static float activeBoundaryMaximumMismatch;
        [ThreadStatic] private static bool activeBoundaryMismatchEvaluated;
        [ThreadStatic] private static bool activeBoundaryCandidateActivated;
        [ThreadStatic] private static int activeBoundaryMorphRingsRequested;
        [ThreadStatic] private static int activeBoundaryMorphRingsUsed;
        [ThreadStatic] private static string activeFailureStage;
        [ThreadStatic] private static bool activeUnsafeVisualPreview;

        public static TreeBarkMeshBuildResult BuildForRootCollapseTournament(
            TreeDefinition definition,
            TreeBarkMeshSettings settings,
            Mesh targetMesh,
            TreeRootCollapseTournamentStrategy strategy)
        {
            TreeRootCollapseTournamentStrategy? previous =
                activeTournamentStrategy;
            activeTournamentStrategy = strategy;
            try
            {
                return Build(definition, settings, targetMesh);
            }
            finally
            {
                activeTournamentStrategy = previous;
            }
        }

        public static TreeBarkMeshBuildResult BuildUnsafeVisualPreview(
            TreeDefinition definition,
            TreeBarkMeshSettings settings,
            Mesh targetMesh)
        {
            bool previous = activeUnsafeVisualPreview;
            TreeRootCollapseTournamentStrategy? previousStrategy =
                activeTournamentStrategy;
            activeUnsafeVisualPreview = true;
            activeTournamentStrategy = TreeRootCollapseTournamentStrategy.Production;
            try
            {
                return Build(definition, settings, targetMesh);
            }
            finally
            {
                activeUnsafeVisualPreview = previous;
                activeTournamentStrategy = previousStrategy;
            }
        }

        internal static TreeRootCollapseTournamentProfile
            GetRootCollapseTournamentProfile(
                TreeRootCollapseTournamentStrategy strategy)
        {
            int morphRings = strategy ==
                TreeRootCollapseTournamentStrategy.TransportedContourBlend
                ? 16
                : 0;
            return new TreeRootCollapseTournamentProfile(
                0.04f, 0.12f, 24, false, 2f, false, morphRings);
        }

        private static TreeRootCollapseTournamentProfile
            GetActiveRootCollapseProfile()
        {
            return activeTournamentStrategy.HasValue
                ? GetRootCollapseTournamentProfile(activeTournamentStrategy.Value)
                : new TreeRootCollapseTournamentProfile(
                    0.04f, 0.12f, 24, false, 2f, false, 0);
        }




        private static void CopyBoundaryTelemetry(
            TreeBarkMeshBuildResult result)
        {
            if (result == null)
            {
                return;
            }
            result.RootTrunkBoundaryMaximumMismatch =
                activeBoundaryMaximumMismatch;
            result.RootTrunkBoundaryMismatchEvaluated =
                activeBoundaryMismatchEvaluated;
            result.RootTrunkBoundaryCandidateActivated =
                activeBoundaryCandidateActivated;
            result.RootTrunkBoundaryMorphRingsRequested =
                activeBoundaryMorphRingsRequested;
            result.RootTrunkBoundaryMorphRingsUsed =
                activeBoundaryMorphRingsUsed;
            result.FailureStage = activeFailureStage ?? string.Empty;
        }

        public static void EvaluateRootCollapseTournamentMetrics(
            TreeResolvedParameters parameters,
            TreeRootCollapseTournamentStrategy strategy,
            out float collapseEnd,
            out float maximumBodyDelta,
            out float maximumFootDelta)
        {
            TreeRootCollapseTournamentStrategy? previous =
                activeTournamentStrategy;
            activeTournamentStrategy = strategy;
            try
            {
                collapseEnd = CalculateEffectiveRootCollapseHeight(parameters);
                float plateau = CalculateRootGroundPlateauEnd(parameters);
                TreeRootCollapseTournamentProfile profile =
                    GetRootCollapseTournamentProfile(strategy);
                int intervals = Mathf.Max(1, profile.CollapseIntervals);
                maximumBodyDelta = 0f;
                maximumFootDelta = 0f;
                EvaluateRootEnvelopes(
                    parameters,
                    plateau,
                    out float previousBody,
                    out float previousFoot);
                for (int index = 1; index <= intervals; index++)
                {
                    float distance = Mathf.Lerp(
                        plateau,
                        collapseEnd,
                        index / (float)intervals);
                    EvaluateRootEnvelopes(
                        parameters,
                        distance,
                        out float body,
                        out float foot);
                    maximumBodyDelta = Mathf.Max(
                        maximumBodyDelta,
                        Mathf.Abs(body - previousBody));
                    maximumFootDelta = Mathf.Max(
                        maximumFootDelta,
                        Mathf.Abs(foot - previousFoot));
                    previousBody = body;
                    previousFoot = foot;
                }
            }
            finally
            {
                activeTournamentStrategy = previous;
            }
        }

        private struct RenderSample
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal Vector3 Normal;
            internal Vector3 Binormal;
            internal float Radius;
            internal float NormalizedDistance;
            internal float CumulativeDistance;
        }

        private struct AxialTwistTelemetry
        {
            internal float FirstNonZeroNormalizedDistance;
            internal float GroundPlateauEndDegrees;
            internal float RootCollapseEndDegrees;
            internal float EarliestRootTransitionDegrees;
            internal float EffectiveRootTransitionDegrees;
            internal float MaximumStepDegrees;
            internal float MaximumAllowedStepDegrees;
            internal float MaximumStepStartNormalizedDistance;
            internal float MaximumStepEndNormalizedDistance;
        }

        private struct TrunkTipClosure
        {
            internal bool Applied;
            internal Vector3 ApexPosition;
            internal int RemovedRingCount;
            internal float Length;
            internal float CollapsedNormalizedSpan;
        }

        private struct ParentFrame
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal Vector3 Normal;
            internal Vector3 Binormal;
            internal float Radius;
        }

        public static TreeBarkMeshBuildResult Build(
            TreeDefinition definition,
            TreeBarkMeshSettings settings,
            Mesh targetMesh)
        {
            var result = new TreeBarkMeshBuildResult();
            activeBoundaryMaximumMismatch = 0f;
            activeBoundaryMismatchEvaluated = false;
            activeBoundaryCandidateActivated = false;
            activeBoundaryMorphRingsRequested = activeTournamentStrategy.HasValue
                ? GetActiveRootCollapseProfile().BoundaryMorphRings
                : 0;
            activeBoundaryMorphRingsUsed = 0;
            activeFailureStage = string.Empty;
            if (definition == null || !definition.IsValid)
            {
                result.Failure = "Tree definition is null or invalid.";
                return result;
            }

            if (settings == null)
            {
                result.Failure = "Bark mesh settings are null.";
                return result;
            }

            if (targetMesh == null)
            {
                result.Failure = "Target bark mesh is null.";
                return result;
            }

            bool captureAuditTelemetry =
                settings.GeometryAuditTelemetryEnabled;
            Stopwatch totalBuildStopwatch = captureAuditTelemetry
                ? Stopwatch.StartNew()
                : null;
            result.EfficiencyPolicy = settings.EfficiencyPolicy;
            List<TreeBarkMeshBranchGeometryAccounting> geometryAccounting =
                captureAuditTelemetry
                    ? new List<TreeBarkMeshBranchGeometryAccounting>()
                    : null;
            result.BranchGeometryAccounting = geometryAccounting;
            var vertices = new List<Vector3>(4096);
            var normals = new List<Vector3>(4096);
            var tangents = new List<Vector4>(4096);
            var colours = new List<Color32>(4096);
            var uv0 = new List<Vector2>(4096);
            var triangles = new List<int>(8192);
            var branchAuditRecords = new List<TreeBarkMeshBranchAuditRecord>();
            var capAuditRecords = new List<TreeBarkMeshCapAuditRecord>();
            Stopwatch geometryBuildStopwatch = captureAuditTelemetry
                ? Stopwatch.StartNew()
                : null;

            int meshedBranches = 0;
            int capCount = 0;
            int alternateQuadDiagonalCount = 0;
            int phaseAlignedRingCount = 0;
            int curvatureRadiusClampCount = 0;
            int circularBranchRingRemovalCount = 0;
            bool trunkTipClosureApplied = false;
            int trunkTipRemovedRingCount = 0;
            float trunkTipClosureLength = 0f;
            int effectiveTrunkRadialSegments = 0;
            int minimumEffectiveTrunkRadialSegments = 0;
            int maximumEffectiveTrunkRadialSegments = 0;
            float averageEffectiveTrunkRadialSegments = 0f;
            int trunkRadialTransitionCount = 0;
            int trunkMixedResolutionStripCount = 0;
            int trunkStitchTriangleCount = 0;
            int effectiveTrunkRingCount = 0;
            int rootZoneLongitudinalIntervals = 0;
            float maximumCrossSectionMultiplier = 1f;
            float minimumGroundCrossSectionMultiplier = 1f;
            float groundButtressCrestMultiplier = 1f;
            float halfHeightButtressCrestMultiplier = 1f;
            float halfHeightRootExtensionRatio = 0f;
            float halfHeightButtressAngularWidthScale = 0f;
            float groundRootHalfExtensionAngularWidthDegrees = 0f;
            float groundRootHalfExtensionChordWidth = 0f;
            float requestedRootSupportAngularWidthDegrees = 0f;
            float emittedRootSupportAngularWidthDegrees = 0f;
            float evaluatedRootThickness = 0f;
            float groundRootBaseMergeFactor = 0f;
            float rootFootShapePlateauEndNormalized = 0f;
            float rootTopRootOnlyMultiplier = 0f;
            float maximumGroundButtressCrestTurnDegrees = 0f;
            float authoredRootHeightNormalized = 0f;
            float effectiveRootTransitionHeightNormalized = 0f;
            float rootTransitionSafetyTailNormalized = 0f;
            float rootGroundPlateauEndNormalized = 0f;
            float rootLobeCollapseEndNormalized = 0f;
            float generatedRootWidth = 0f;
            float generatedRootDepth = 0f;
            float requestedAxialTwistDegrees = 0f;
            float measuredAxialTwistDegrees = 0f;
            float axialTwistErrorDegrees = 0f;
            float axialTwistTurns = 0f;
            TreeResolvedParameters resolved = definition.ResolvedParameters;
            AxialTwistTelemetry axialTwistTelemetry =
                CreateAxialTwistTelemetry(resolved, settings);
            IReadOnlyList<TreeBranchDefinition> branches = definition.Branches;
            for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
            {
                TreeBranchDefinition candidate = branches[branchIndex];
                if (candidate != null && candidate.BranchOrder == 0)
                {
                    int diagnosticSegments = settings.ResolveRadialSegments(
                        0,
                        resolved.RootButtressCount);
                    float rootHeight = Mathf.Max(
                        0.01f,
                        resolved.RootButtressHeight);
                    authoredRootHeightNormalized = rootHeight;
                    effectiveRootTransitionHeightNormalized =
                        CalculateEffectiveRootTransitionHeight(resolved);
                    rootTransitionSafetyTailNormalized = Mathf.Max(
                        0f,
                        effectiveRootTransitionHeightNormalized - rootHeight);
                    rootGroundPlateauEndNormalized = rootHeight * 0.10f;
                    rootLobeCollapseEndNormalized =
                        CalculateEffectiveRootCollapseHeight(resolved);
                    evaluatedRootThickness = resolved.RootThickness;
                    groundRootBaseMergeFactor =
                        EvaluateGroundRootBaseMergeFactor(resolved);
                    rootFootShapePlateauEndNormalized =
                        CalculateRootFootShapePlateauEnd(resolved);
                    minimumGroundCrossSectionMultiplier =
                        CalculateMinimumTrunkCrossSectionMultiplier(
                            resolved,
                            candidate.Phase,
                            0f);
                    groundButtressCrestMultiplier =
                        CalculateButtressCrestMultiplier(
                            resolved,
                            candidate.Phase,
                            0f);
                    halfHeightButtressCrestMultiplier =
                        CalculateButtressCrestMultiplier(
                            resolved,
                            candidate.Phase,
                            rootHeight * 0.5f);
                    float groundRootExtension =
                        CalculateButtressCrestRootOnlyContribution(
                            resolved,
                            candidate.Phase,
                            0f);
                    float halfHeightRootExtension =
                        CalculateButtressCrestRootOnlyContribution(
                            resolved,
                            candidate.Phase,
                            rootHeight * 0.5f);
                    halfHeightRootExtensionRatio = groundRootExtension > Epsilon
                        ? halfHeightRootExtension / groundRootExtension
                        : 0f;
                    halfHeightButtressAngularWidthScale =
                        CalculateButtressAngularWidthScale(
                            resolved,
                            rootHeight * 0.5f);
                    CalculateGroundRootHalfExtensionWidth(
                        resolved,
                        candidate.Phase,
                        candidate.Samples != null &&
                        candidate.Samples.Count > 0
                            ? candidate.Samples[0].Radius
                            : resolved.TrunkBaseRadius,
                        out groundRootHalfExtensionAngularWidthDegrees,
                        out groundRootHalfExtensionChordWidth);
                    if (resolved.RecipeOnlyControlSource)
                    {
                        requestedRootSupportAngularWidthDegrees =
                            EvaluateRequestedRootFullWidthDegrees(
                                resolved.RootThickness);
                        emittedRootSupportAngularWidthDegrees = Mathf.Min(
                            requestedRootSupportAngularWidthDegrees,
                            360f / Mathf.Clamp(
                                resolved.RootButtressCount,
                                3,
                                8));
                    }
                    rootTopRootOnlyMultiplier =
                        CalculateMaximumRootOnlyContribution(
                            resolved,
                            candidate.Phase,
                            rootHeight);
                    maximumGroundButtressCrestTurnDegrees =
                        CalculateMaximumGroundButtressCrestTurnDegrees(
                            resolved,
                            candidate.Phase,
                            diagnosticSegments);
                    break;
                }
            }

            // Preserve root-transition telemetry even when trunk meshing
            // fails before a successful result can be committed.
            result.AuthoredRootHeightNormalized =
                authoredRootHeightNormalized;
            result.EffectiveRootTransitionHeightNormalized =
                effectiveRootTransitionHeightNormalized;
            result.RootTransitionSafetyTailNormalized =
                rootTransitionSafetyTailNormalized;
            result.RootGroundPlateauEndNormalized =
                rootGroundPlateauEndNormalized;
            result.RootLobeCollapseEndNormalized =
                rootLobeCollapseEndNormalized;
            result.RequestedRootSupportAngularWidthDegrees =
                requestedRootSupportAngularWidthDegrees;
            result.EmittedRootSupportAngularWidthDegrees =
                emittedRootSupportAngularWidthDegrees;
            result.RootSupportWidthClampedByCount =
                emittedRootSupportAngularWidthDegrees + 0.0001f <
                requestedRootSupportAngularWidthDegrees;
            result.EvaluatedRootThickness = evaluatedRootThickness;
            result.GroundRootBaseMergeFactor = groundRootBaseMergeFactor;
            result.RootFootShapePlateauEndNormalized =
                rootFootShapePlateauEndNormalized;
            CopyAxialTwistTelemetry(result, axialTwistTelemetry);

            for (int branchIndex = 0;
                 branchIndex < branches.Count;
                 branchIndex++)
            {
                TreeBranchDefinition branch = branches[branchIndex];
                IReadOnlyList<TreeCurveSample> samples = branch.Samples;
                if (samples == null || samples.Count < 2)
                {
                    result.Failure =
                        "Branch " + branch.StableBranchId +
                        " does not contain enough samples for bark meshing.";
                    targetMesh.Clear();
                    return result;
                }

                int radialSegments = settings.ResolveRadialSegments(
                    branch.BranchOrder,
                    resolved.RootButtressCount);
                if (branch.BranchOrder != 0 &&
                    settings.UsesRadiusAwareBranchRadialResolution)
                {
                    radialSegments = ResolveRadiusAwareBranchRadialSegments(
                        branch,
                        samples,
                        resolved,
                        settings,
                        radialSegments);
                }
                if (branch.BranchOrder == 0)
                {
                    effectiveTrunkRadialSegments = radialSegments;
                }
                if (!AppendBranchTube(
                        definition,
                        branch,
                        samples,
                        radialSegments,
                        settings,
                        vertices,
                        normals,
                        tangents,
                        colours,
                        uv0,
                        triangles,
                        branchAuditRecords,
                        capAuditRecords,
                        geometryAccounting,
                        ref capCount,
                        ref alternateQuadDiagonalCount,
                        ref phaseAlignedRingCount,
                        ref curvatureRadiusClampCount,
                        ref circularBranchRingRemovalCount,
                        ref trunkTipClosureApplied,
                        ref trunkTipRemovedRingCount,
                        ref trunkTipClosureLength,
                        ref effectiveTrunkRingCount,
                        ref minimumEffectiveTrunkRadialSegments,
                        ref maximumEffectiveTrunkRadialSegments,
                        ref averageEffectiveTrunkRadialSegments,
                        ref trunkRadialTransitionCount,
                        ref trunkMixedResolutionStripCount,
                        ref trunkStitchTriangleCount,
                        ref rootZoneLongitudinalIntervals,
                        ref maximumCrossSectionMultiplier,
                        ref generatedRootWidth,
                        ref generatedRootDepth,
                        ref requestedAxialTwistDegrees,
                        ref measuredAxialTwistDegrees,
                        ref axialTwistErrorDegrees,
                        ref axialTwistTurns,
                        ref axialTwistTelemetry,
                        out string failure))
                {
                    if (captureAuditTelemetry)
                    {
                        geometryBuildStopwatch.Stop();
                        result.GeometryBuildMilliseconds =
                            geometryBuildStopwatch.Elapsed.TotalMilliseconds;
                        result.TotalBuildMilliseconds =
                            totalBuildStopwatch.Elapsed.TotalMilliseconds;
                    }
                    CopyBoundaryTelemetry(result);
                    CopyAxialTwistTelemetry(result, axialTwistTelemetry);
                    result.Failure = failure;
                    targetMesh.Clear();
                    return result;
                }

                meshedBranches++;
            }

            if (captureAuditTelemetry)
            {
                geometryBuildStopwatch.Stop();
                result.GeometryBuildMilliseconds =
                    geometryBuildStopwatch.Elapsed.TotalMilliseconds;
            }

            if (vertices.Count == 0 || triangles.Count < 3)
            {
                result.Failure = "Bark mesh generation produced no renderable geometry.";
                targetMesh.Clear();
                return result;
            }

            Stopwatch topologyAuditStopwatch = captureAuditTelemetry
                ? Stopwatch.StartNew()
                : null;
            result.TopologyAudit = TreeBarkMeshTopologyAudit.Run(
                definition,
                vertices,
                normals,
                tangents,
                uv0,
                triangles,
                branchAuditRecords,
                capAuditRecords);
            if (captureAuditTelemetry)
            {
                topologyAuditStopwatch.Stop();
                result.TopologyAuditMilliseconds =
                    topologyAuditStopwatch.Elapsed.TotalMilliseconds;
            }
            if (!result.TopologyAudit.Passed)
            {
                activeFailureStage = "FINAL_TOPOLOGY_AUDIT";
                result.VertexCount = vertices.Count;
                result.TriangleCount = triangles.Count / 3;
                CopyBoundaryTelemetry(result);
                CopyAxialTwistTelemetry(result, axialTwistTelemetry);
                result.Failure =
                    "Bark topology audit failed.\n" +
                    result.TopologyAudit.Report;
                if (captureAuditTelemetry)
                {
                    result.TotalBuildMilliseconds =
                        totalBuildStopwatch.Elapsed.TotalMilliseconds;
                }
                targetMesh.Clear();
                if (activeUnsafeVisualPreview)
                {
                    targetMesh.indexFormat = vertices.Count > ushort.MaxValue
                        ? IndexFormat.UInt32
                        : IndexFormat.UInt16;
                    targetMesh.SetVertices(vertices);
                    targetMesh.SetNormals(normals);
                    targetMesh.SetTangents(tangents);
                    targetMesh.SetColors(colours);
                    targetMesh.SetUVs(0, uv0);
                    targetMesh.SetTriangles(triangles, 0, true);
                    targetMesh.RecalculateBounds();
                }
                return result;
            }

            Stopwatch meshUploadStopwatch = captureAuditTelemetry
                ? Stopwatch.StartNew()
                : null;
            targetMesh.Clear();
            targetMesh.indexFormat = vertices.Count > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;
            targetMesh.SetVertices(vertices);
            targetMesh.SetNormals(normals);
            targetMesh.SetTangents(tangents);
            targetMesh.SetColors(colours);
            targetMesh.SetUVs(0, uv0);
            targetMesh.SetTriangles(triangles, 0, true);
            targetMesh.RecalculateBounds();
            targetMesh.UploadMeshData(false);
            if (captureAuditTelemetry)
            {
                meshUploadStopwatch.Stop();
                result.MeshUploadMilliseconds =
                    meshUploadStopwatch.Elapsed.TotalMilliseconds;
            }

            result.Passed = true;
            result.MeshedBranchCount = meshedBranches;
            result.VertexCount = vertices.Count;
            result.TriangleCount = triangles.Count / 3;
            result.TipCapCount = capCount;
            result.AlternateQuadDiagonalCount = alternateQuadDiagonalCount;
            result.PhaseAlignedRingCount = phaseAlignedRingCount;
            result.CurvatureRadiusClampCount = curvatureRadiusClampCount;
            result.CircularBranchRingRemovalCount =
                circularBranchRingRemovalCount;
            result.TrunkTipClosureApplied = trunkTipClosureApplied;
            result.TrunkTipRemovedRingCount = trunkTipRemovedRingCount;
            result.TrunkTipClosureLength = trunkTipClosureLength;
            if (maximumEffectiveTrunkRadialSegments > 0)
            {
                effectiveTrunkRadialSegments =
                    maximumEffectiveTrunkRadialSegments;
            }
            result.EffectiveTrunkRadialSegments =
                effectiveTrunkRadialSegments;
            result.MinimumEffectiveTrunkRadialSegments =
                minimumEffectiveTrunkRadialSegments;
            result.MaximumEffectiveTrunkRadialSegments =
                maximumEffectiveTrunkRadialSegments;
            result.AverageEffectiveTrunkRadialSegments =
                averageEffectiveTrunkRadialSegments;
            result.TrunkRadialTransitionCount =
                trunkRadialTransitionCount;
            result.TrunkMixedResolutionStripCount =
                trunkMixedResolutionStripCount;
            result.TrunkStitchTriangleCount =
                trunkStitchTriangleCount;
            result.EffectiveTrunkRingCount = effectiveTrunkRingCount;
            result.RootZoneLongitudinalIntervals =
                rootZoneLongitudinalIntervals;
            result.ButtressSamplesPerLobe =
                effectiveTrunkRadialSegments /
                (float)Mathf.Max(3, resolved.RootButtressCount);
            result.GroundButtressCrestMultiplier =
                groundButtressCrestMultiplier;
            result.HalfHeightButtressCrestMultiplier =
                halfHeightButtressCrestMultiplier;
            result.HalfHeightRootExtensionRatio =
                halfHeightRootExtensionRatio;
            result.HalfHeightButtressAngularWidthScale =
                halfHeightButtressAngularWidthScale;
            result.GroundRootHalfExtensionAngularWidthDegrees =
                groundRootHalfExtensionAngularWidthDegrees;
            result.GroundRootHalfExtensionChordWidth =
                groundRootHalfExtensionChordWidth;
            result.RequestedRootSupportAngularWidthDegrees =
                requestedRootSupportAngularWidthDegrees;
            result.EmittedRootSupportAngularWidthDegrees =
                emittedRootSupportAngularWidthDegrees;
            result.RootSupportWidthClampedByCount =
                emittedRootSupportAngularWidthDegrees + 0.0001f <
                requestedRootSupportAngularWidthDegrees;
            result.EvaluatedRootThickness = evaluatedRootThickness;
            result.GroundRootBaseMergeFactor = groundRootBaseMergeFactor;
            result.RootFootShapePlateauEndNormalized =
                rootFootShapePlateauEndNormalized;
            result.RootTopRootOnlyMultiplier =
                rootTopRootOnlyMultiplier;
            result.AuthoredRootHeightNormalized =
                authoredRootHeightNormalized;
            result.EffectiveRootTransitionHeightNormalized =
                effectiveRootTransitionHeightNormalized;
            result.RootTransitionSafetyTailNormalized =
                rootTransitionSafetyTailNormalized;
            result.RootGroundPlateauEndNormalized =
                rootGroundPlateauEndNormalized;
            result.RootLobeCollapseEndNormalized =
                rootLobeCollapseEndNormalized;
            result.MaximumGroundButtressCrestTurnDegrees =
                maximumGroundButtressCrestTurnDegrees;
            result.PathSpiralStrength = resolved.TrunkSpiralStrength;
            result.PathSpiralTurns = resolved.TrunkSpiralTurns;
            result.PathSpiralDirection = resolved.TrunkSpiralDirection;
            result.MaximumPathSpiralRadius =
                resolved.TrunkSpiralStrength * resolved.Height * 0.35f;
            result.MaximumCrossSectionMultiplier =
                maximumCrossSectionMultiplier;
            result.MinimumGroundCrossSectionMultiplier =
                minimumGroundCrossSectionMultiplier;
            result.GeneratedRootWidth = generatedRootWidth;
            result.GeneratedRootDepth = generatedRootDepth;
            result.RequestedAxialTwistDegrees = requestedAxialTwistDegrees;
            result.MeasuredAxialTwistDegrees = measuredAxialTwistDegrees;
            result.AxialTwistErrorDegrees = axialTwistErrorDegrees;
            result.AxialTwistTurns = axialTwistTurns;
            CopyAxialTwistTelemetry(result, axialTwistTelemetry);
            CopyBoundaryTelemetry(result);
            result.LocalBounds = targetMesh.bounds;
            result.InputFingerprint = CalculateInputFingerprint(
                definition,
                settings);
            result.GeometryFingerprint = BuildGeometryFingerprint(
                vertices,
                normals,
                tangents,
                colours,
                uv0,
                triangles);
            if (captureAuditTelemetry)
            {
                int indexBytes = vertices.Count > ushort.MaxValue ? 4 : 2;
                result.EstimatedMeshBytes =
                    (long)vertices.Count * 52L +
                    (long)triangles.Count * indexBytes;
                totalBuildStopwatch.Stop();
                result.TotalBuildMilliseconds =
                    totalBuildStopwatch.Elapsed.TotalMilliseconds;
            }
            result.Failure = string.Empty;
            return result;
        }

        private static int ResolveRadiusAwareBranchRadialSegments(
            TreeBranchDefinition branch,
            IReadOnlyList<TreeCurveSample> samples,
            TreeResolvedParameters resolved,
            TreeBarkMeshSettings settings,
            int authoredSegments)
        {
            if (branch == null || branch.BranchOrder == 0 ||
                samples == null || samples.Count == 0 || settings == null)
            {
                return Mathf.Max(3, authoredSegments);
            }

            float maximumRadius = 0f;
            for (int index = 0; index < samples.Count; index++)
            {
                TreeCurveSample sample = samples[index];
                maximumRadius = Mathf.Max(
                    maximumRadius,
                    Mathf.Max(0f, sample.Radius));
            }

            return settings.ResolveRadiusAwareBranchRadialSegments(
                branch.BranchOrder,
                maximumRadius,
                Mathf.Max(0.0001f, resolved.TrunkBaseRadius),
                authoredSegments);
        }

        private static int[] BuildRingRadialSegments(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int authoredSegments,
            TreeBarkMeshSettings settings,
            out bool[] lobeOwnedRings,
            out bool[] groundContactBoostedRings,
            out int minimumSegments,
            out int maximumSegments,
            out float averageSegments,
            out int transitionCount)
        {
            int ringCount = samples != null ? samples.Count : 0;
            var resolved = new int[ringCount];
            lobeOwnedRings = new bool[ringCount];
            groundContactBoostedRings = new bool[ringCount];
            minimumSegments = 0;
            maximumSegments = 0;
            averageSegments = 0f;
            transitionCount = 0;
            if (ringCount == 0)
            {
                return resolved;
            }

            int safeAuthored = Mathf.Max(3, authoredSegments);
            bool contourOwned = branch != null &&
                branch.BranchOrder == 0 &&
                settings != null &&
                settings.UsesContourOwnedTrunkRadialResolution &&
                definition != null;
            if (!contourOwned)
            {
                for (int index = 0; index < ringCount; index++)
                {
                    resolved[index] = safeAuthored;
                }
                minimumSegments = safeAuthored;
                maximumSegments = safeAuthored;
                averageSegments = safeAuthored;
                return resolved;
            }

            TreeResolvedParameters parameters = definition.ResolvedParameters;
            int rootCount = Mathf.Clamp(parameters.RootButtressCount, 3, 8);
            float releaseThreshold =
                settings.ResolveCircularTrunkLobeReleaseThreshold();
            float trunkBaseRadius = Mathf.Max(
                0.0001f,
                parameters.TrunkBaseRadius);
            long segmentSum = 0L;
            int previous = 0;
            for (int index = 0; index < ringCount; index++)
            {
                RenderSample sample = samples[index];
                float lobeAmplitude = CalculateMaximumRootOnlyContribution(
                    parameters,
                    branch.Phase,
                    sample.NormalizedDistance);
                bool lobeOwned = lobeAmplitude > releaseThreshold;
                int candidate;
                if (lobeOwned)
                {
                    int samplesPerLobe =
                        settings.ResolveLobedTrunkSamplesPerLobe(
                            lobeAmplitude);
                    int boostedSamplesPerLobe = samplesPerLobe;
                    if (settings.EfficiencyPolicy ==
                            TreeBarkMeshEfficiencyPolicy.Current &&
                        parameters.RecipeOnlyControlSource)
                    {
                        boostedSamplesPerLobe =
                            ResolveGroundContactSamplesPerLobe(
                                parameters,
                                sample.NormalizedDistance,
                                lobeAmplitude,
                                samplesPerLobe,
                                rootCount,
                                safeAuthored);
                    }
                    groundContactBoostedRings[index] =
                        boostedSamplesPerLobe > samplesPerLobe;
                    samplesPerLobe = boostedSamplesPerLobe;
                    int circularFloor =
                        settings.ResolveCircularTrunkRadialSegments(
                            sample.Radius,
                            trunkBaseRadius);
                    int rootCompatibleCircularFloor =
                        Mathf.CeilToInt(
                            circularFloor / (float)rootCount) *
                        rootCount;
                    candidate = Mathf.Clamp(
                        Mathf.Max(
                            rootCount * samplesPerLobe,
                            rootCompatibleCircularFloor),
                        rootCount * 3,
                        safeAuthored);
                }
                else
                {
                    candidate = Mathf.Min(
                        safeAuthored,
                        settings.ResolveCircularTrunkRadialSegments(
                            sample.Radius,
                            trunkBaseRadius));
                }

                if (previous > 0)
                {
                    candidate = ResolveNextLowerRadialTier(
                        previous,
                        candidate,
                        rootCount,
                        safeAuthored,
                        settings.UsesAggressiveRadialResolution,
                        lobeOwned);
                    if (candidate != previous)
                    {
                        transitionCount++;
                    }
                }

                candidate = Mathf.Max(3, candidate);
                resolved[index] = candidate;
                lobeOwnedRings[index] = lobeOwned;
                minimumSegments = index == 0
                    ? candidate
                    : Mathf.Min(minimumSegments, candidate);
                maximumSegments = Mathf.Max(maximumSegments, candidate);
                segmentSum += candidate;
                previous = candidate;
            }

            averageSegments = segmentSum / (float)ringCount;
            return resolved;
        }

        private static int ResolveGroundContactSamplesPerLobe(
            TreeResolvedParameters parameters,
            float normalizedDistance,
            float lobeAmplitude,
            int baselineSamplesPerLobe,
            int rootCount,
            int authoredMaximumSegments)
        {
            int safeBaseline = Mathf.Max(3, baselineSamplesPerLobe);
            if (parameters == null ||
                !parameters.RecipeOnlyControlSource ||
                rootCount < 3 ||
                authoredMaximumSegments < rootCount * safeBaseline)
            {
                return safeBaseline;
            }

            int maximumCompatibleSamplesPerLobe = Mathf.Clamp(
                authoredMaximumSegments / rootCount,
                safeBaseline,
                10);
            if (maximumCompatibleSamplesPerLobe <= safeBaseline)
            {
                return safeBaseline;
            }

            float amplitudeDemand = Mathf.InverseLerp(
                0.35f,
                0.85f,
                Mathf.Max(0f, lobeAmplitude));
            amplitudeDemand = amplitudeDemand * amplitudeDemand *
                (3f - 2f * amplitudeDemand);
            if (amplitudeDemand <= Epsilon)
            {
                return safeBaseline;
            }

            EvaluateRootEnvelopes(
                parameters,
                normalizedDistance,
                out _,
                out float footShapeEnvelope);
            float contactWeight = Mathf.InverseLerp(
                0.25f,
                1f,
                footShapeEnvelope);
            contactWeight = contactWeight * contactWeight *
                (3f - 2f * contactWeight);
            float combinedWeight = Mathf.Clamp01(
                amplitudeDemand * contactWeight);
            if (combinedWeight <= Epsilon)
            {
                return safeBaseline;
            }

            return Mathf.Clamp(
                Mathf.RoundToInt(Mathf.Lerp(
                    safeBaseline,
                    maximumCompatibleSamplesPerLobe,
                    combinedWeight)),
                safeBaseline,
                maximumCompatibleSamplesPerLobe);
        }

        private static int ResolveNextLowerRadialTier(
            int previous,
            int requested,
            int rootCount,
            int authoredMaximum,
            bool aggressive,
            bool rootCompatibleOnly)
        {
            int safePrevious = Mathf.Max(3, previous);
            int safeRequested = Mathf.Clamp(
                requested,
                3,
                safePrevious);
            if (safeRequested >= safePrevious)
            {
                return safePrevious;
            }

            var tiers = new List<int>(12);
            AddRadialTier(tiers, authoredMaximum, safePrevious);
            AddRadialTier(tiers, rootCount * 6, safePrevious);
            AddRadialTier(tiers, rootCount * 5, safePrevious);
            AddRadialTier(tiers, rootCount * 4, safePrevious);
            AddRadialTier(tiers, rootCount * 3, safePrevious);
            AddRadialTier(tiers, rootCount * 2, safePrevious);
            if (!rootCompatibleOnly)
            {
                if (aggressive)
                {
                    AddRadialTier(tiers, 10, safePrevious);
                    AddRadialTier(tiers, 8, safePrevious);
                    AddRadialTier(tiers, 6, safePrevious);
                }
                else
                {
                    AddRadialTier(tiers, 12, safePrevious);
                    AddRadialTier(tiers, 10, safePrevious);
                    AddRadialTier(tiers, 8, safePrevious);
                    AddRadialTier(tiers, 6, safePrevious);
                }
            }
            AddRadialTier(tiers, safeRequested, safePrevious);
            tiers.Sort((a, b) => b.CompareTo(a));

            for (int index = 0; index < tiers.Count; index++)
            {
                int tier = tiers[index];
                if (tier < safePrevious && tier >= safeRequested)
                {
                    return tier;
                }
            }

            return safeRequested;
        }

        private static void AddRadialTier(
            List<int> tiers,
            int value,
            int maximum)
        {
            int tier = Mathf.Clamp(value, 3, Mathf.Max(3, maximum));
            if (!tiers.Contains(tier))
            {
                tiers.Add(tier);
            }
        }

        private static bool AppendBranchTube(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<TreeCurveSample> sourceSamples,
            int radialSegments,
            TreeBarkMeshSettings settings,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color32> colours,
            List<Vector2> uv0,
            List<int> triangles,
            List<TreeBarkMeshBranchAuditRecord> branchAuditRecords,
            List<TreeBarkMeshCapAuditRecord> capAuditRecords,
            List<TreeBarkMeshBranchGeometryAccounting> geometryAccounting,
            ref int capCount,
            ref int alternateQuadDiagonalCount,
            ref int phaseAlignedRingCount,
            ref int curvatureRadiusClampCount,
            ref int circularBranchRingRemovalCount,
            ref bool trunkTipClosureApplied,
            ref int trunkTipRemovedRingCount,
            ref float trunkTipClosureLength,
            ref int effectiveTrunkRingCount,
            ref int minimumEffectiveTrunkRadialSegments,
            ref int maximumEffectiveTrunkRadialSegments,
            ref float averageEffectiveTrunkRadialSegments,
            ref int trunkRadialTransitionCount,
            ref int trunkMixedResolutionStripCount,
            ref int trunkStitchTriangleCount,
            ref int rootZoneLongitudinalIntervals,
            ref float maximumCrossSectionMultiplier,
            ref float generatedRootWidth,
            ref float generatedRootDepth,
            ref float requestedAxialTwistDegrees,
            ref float measuredAxialTwistDegrees,
            ref float axialTwistErrorDegrees,
            ref float axialTwistTurns,
            ref AxialTwistTelemetry axialTwistTelemetry,
            out string failure)
        {
            failure = string.Empty;
            int branchVertexStart = vertices.Count;
            int branchTriangleIndexStart = triangles.Count;
            int sourceSampleCount = sourceSamples != null
                ? sourceSamples.Count
                : 0;
            radialSegments = Mathf.Max(3, radialSegments);
            List<RenderSample> samples = BuildRenderSamples(
                definition,
                branch,
                sourceSamples,
                settings,
                ref curvatureRadiusClampCount);
            TrunkTipClosure trunkTipClosure = default;
            int rootRefinementInsertedRings = 0;
            int twistRefinementInsertedRings = 0;
            int adaptiveShapeRefinementInsertedRings = 0;
            int efficiencyPolicyRemovedRings = 0;
            int topologyRepairRemovedRings = 0;
            if (branch.BranchOrder == 0)
            {
                samples = RefineTrunkRenderSamples(
                    samples,
                    definition.ResolvedParameters,
                    settings,
                    out rootRefinementInsertedRings,
                    out twistRefinementInsertedRings,
                    out adaptiveShapeRefinementInsertedRings);
                if (activeTournamentStrategy ==
                    TreeRootCollapseTournamentStrategy.DenseFrameAdoptionResampling)
                {
                    PrepareFrameAdoptionSamples(
                        definition.ResolvedParameters,
                        samples,
                        16);
                }
                if (activeTournamentStrategy.HasValue)
                {
                    PrepareBoundaryTransitionSamples(
                        definition,
                        samples,
                        GetActiveRootCollapseProfile().BoundaryMorphRings);
                }
                activeFailureStage = "TRUNK_PREFLIGHT";
                if (!PrepareTopologySafeTrunkTip(
                        definition,
                        branch,
                        samples,
                        radialSegments,
                        out trunkTipClosure,
                        out failure))
                {
                    if (!activeUnsafeVisualPreview)
                    {
                        return false;
                    }
                    failure = string.Empty;
                    trunkTipClosure = default;
                }

                activeFailureStage = string.Empty;

                if (trunkTipClosure.Applied &&
                    !settings.CapBranchTips)
                {
                    failure =
                        "A topology-safe terminal trunk closure requires " +
                        "branch-tip caps to be enabled.";
                    return false;
                }

                trunkTipClosureApplied = trunkTipClosure.Applied;
                trunkTipRemovedRingCount =
                    trunkTipClosure.RemovedRingCount;
                topologyRepairRemovedRings =
                    trunkTipClosure.RemovedRingCount;
                trunkTipClosureLength = trunkTipClosure.Length;
            }
            else
            {
                if (settings.UsesAdaptiveCircularBranchSampling)
                {
                    int ringsBeforeEfficiencyReduction = samples.Count;
                    ReduceCircularBranchRenderSamples(
                        samples,
                        branch.BranchOrder,
                        settings,
                        settings.BranchRootTransitionRingCount);
                    efficiencyPolicyRemovedRings = Mathf.Max(
                        0,
                        ringsBeforeEfficiencyReduction - samples.Count);
                }
                int topologyRemovalCountBefore =
                    circularBranchRingRemovalCount;
                RemoveTopologyCollapsedCircularSamples(
                    samples,
                    radialSegments,
                    ref circularBranchRingRemovalCount);
                topologyRepairRemovedRings = Mathf.Max(
                    0,
                    circularBranchRingRemovalCount -
                        topologyRemovalCountBefore);
            }

            if (branch.BranchOrder == 0)
            {
                effectiveTrunkRingCount = samples.Count;
                rootZoneLongitudinalIntervals =
                    CountRootZoneLongitudinalIntervals(
                        samples,
                        CalculateEffectiveRootTransitionHeight(
                            definition.ResolvedParameters));
            }

            if (samples.Count < 2)
            {
                failure =
                    "Branch " + branch.StableBranchId +
                    " produced fewer than two render samples.";
                return false;
            }

            if (branch.BranchOrder == 0)
            {
                CalculateAuthoredAxialTwistDistribution(
                    definition.ResolvedParameters,
                    samples,
                    ref axialTwistTelemetry);
                if (axialTwistTelemetry.MaximumStepDegrees >
                    axialTwistTelemetry.MaximumAllowedStepDegrees + 0.001f)
                {
                    failure =
                        "Generated trunk axial-roll sampling exceeded the " +
                        "active-policy step limit. measured=" +
                        axialTwistTelemetry.MaximumStepDegrees.ToString("F3") +
                        " allowed=" +
                        axialTwistTelemetry.MaximumAllowedStepDegrees
                            .ToString("F3") +
                        " interval=" +
                        axialTwistTelemetry
                            .MaximumStepStartNormalizedDistance
                            .ToString("F6") +
                        "->" +
                        axialTwistTelemetry
                            .MaximumStepEndNormalizedDistance
                            .ToString("F6") + ".";
                    return false;
                }
            }

            int minimumRadialSegments;
            int maximumRadialSegments;
            float averageRadialSegments;
            int radialTransitionCount;
            bool[] lobeOwnedRings;
            bool[] groundContactBoostedRings;
            int[] ringRadialSegments = BuildRingRadialSegments(
                definition,
                branch,
                samples,
                radialSegments,
                settings,
                out lobeOwnedRings,
                out groundContactBoostedRings,
                out minimumRadialSegments,
                out maximumRadialSegments,
                out averageRadialSegments,
                out radialTransitionCount);
            var ringVertexStarts = new int[samples.Count];
            var ringPhaseOffsets = new int[samples.Count];
            if (branch.BranchOrder == 0)
            {
                minimumEffectiveTrunkRadialSegments = minimumRadialSegments;
                maximumEffectiveTrunkRadialSegments = maximumRadialSegments;
                averageEffectiveTrunkRadialSegments = averageRadialSegments;
                trunkRadialTransitionCount = radialTransitionCount;
            }

            for (int sampleIndex = 0;
                 sampleIndex < samples.Count;
                 sampleIndex++)
            {
                RenderSample sample = samples[sampleIndex];
                if (!IsUsableSample(sample))
                {
                    failure =
                        "Branch " + branch.StableBranchId +
                        " contains non-finite or degenerate transported-frame data.";
                    return false;
                }

                int ringSegments = ringRadialSegments[sampleIndex];
                ringVertexStarts[sampleIndex] = vertices.Count;
                if (sampleIndex > 0 && branch.BranchOrder != 0)
                {
                    int previousRing = ringVertexStarts[sampleIndex - 1];
                    ringPhaseOffsets[sampleIndex] = ResolveBestRingPhase(
                        sample,
                        ringSegments,
                        previousRing,
                        vertices,
                        normals,
                        ringPhaseOffsets[sampleIndex - 1]);
                    if (ringPhaseOffsets[sampleIndex] !=
                        ringPhaseOffsets[sampleIndex - 1])
                    {
                        phaseAlignedRingCount++;
                    }
                }

                int phaseOffset = ringPhaseOffsets[sampleIndex];
                Color32 metadata = BuildVertexMetadata(
                    definition,
                    branch,
                    sample.Position);
                for (int side = 0; side <= ringSegments; side++)
                {
                    float authoredSide = side / (float)ringSegments;
                    float circularBranchSide =
                        (side + phaseOffset) / (float)ringSegments;
                    float geometrySide = branch.BranchOrder == 0
                        ? authoredSide
                        : circularBranchSide;

                    BuildCandidateSurfaceVertex(
                        definition,
                        branch,
                        samples,
                        sampleIndex,
                        geometrySide,
                        ringSegments,
                        out Vector3 position,
                        out Vector3 normal,
                        out Vector3 circumferenceTangent,
                        out float crossSectionMultiplier);

                    vertices.Add(position);
                    normals.Add(normal);
                    tangents.Add(new Vector4(
                        circumferenceTangent.x,
                        circumferenceTangent.y,
                        circumferenceTangent.z,
                        1f));
                    colours.Add(metadata);

                    float uvSide = branch.BranchOrder == 0
                        ? authoredSide
                        : circularBranchSide;
                    uv0.Add(new Vector2(
                        uvSide,
                        sample.CumulativeDistance /
                            settings.BarkMetersPerTile));

                    if (branch.BranchOrder == 0)
                    {
                        maximumCrossSectionMultiplier = Mathf.Max(
                            maximumCrossSectionMultiplier,
                            crossSectionMultiplier);
                    }
                }
            }

            if (branch.BranchOrder == 0)
            {
                CalculateRootDimensions(
                    vertices,
                    ringVertexStarts[0],
                    ringRadialSegments[0],
                    out generatedRootWidth,
                    out generatedRootDepth);

                requestedAxialTwistDegrees =
                    definition.ResolvedParameters.TrunkSurfaceTorsionDegrees;
                measuredAxialTwistDegrees =
                    MeasureGeneratedTrunkAxialTwist(
                        definition.ResolvedParameters,
                        samples,
                        vertices,
                        ringVertexStarts);
                if (trunkTipClosure.Applied)
                {
                    // Roll becomes geometrically undefined as the terminal
                    // cone collapses to one apex. Count the authored remaining
                    // roll as completed at that zero-radius point rather than
                    // falsely reporting a twist deficit caused by removing
                    // invalid finite-radius rings.
                    float collapsedStart = Mathf.Clamp01(
                        1f - trunkTipClosure.CollapsedNormalizedSpan);
                    measuredAxialTwistDegrees +=
                        ResolveAuthoredTrunkSurfaceRollDegrees(
                            definition.ResolvedParameters,
                            1f) -
                        ResolveAuthoredTrunkSurfaceRollDegrees(
                            definition.ResolvedParameters,
                            collapsedStart);
                }
                axialTwistErrorDegrees = Mathf.Abs(
                    measuredAxialTwistDegrees -
                    requestedAxialTwistDegrees);
                axialTwistTurns = measuredAxialTwistDegrees / 360f;
                if (axialTwistErrorDegrees > 2f)
                {
                    failure =
                        "Generated axial twist differs from the requested value by " +
                        axialTwistErrorDegrees.ToString("F3") +
                        " degrees. requested=" +
                        requestedAxialTwistDegrees.ToString("F3") +
                        " measured=" +
                        measuredAxialTwistDegrees.ToString("F3") + ".";
                    return false;
                }
            }

            int sideTriangleStart = triangles.Count;
            var sideTriangleUsesContour = new List<bool>();
            var sideTriangleExpectedDirections = new List<Vector3>();
            for (int ring = 0; ring < samples.Count - 1; ring++)
            {
                int currentRing = ringVertexStarts[ring];
                int nextRing = ringVertexStarts[ring + 1];
                int currentSegments = ringRadialSegments[ring];
                int nextSegments = ringRadialSegments[ring + 1];
                bool boundaryTransition = IsBoundaryTransitionStrip(
                    definition, samples, ring);
                bool useContour = boundaryTransition ||
                    ShouldUseRootContourValidation(
                        definition.ResolvedParameters,
                        branch,
                        samples[ring],
                        samples[ring + 1]);

                if (currentSegments != nextSegments)
                {
                    int trianglesBeforeStitch = triangles.Count;
                    int rootCount = Mathf.Clamp(
                        definition.ResolvedParameters.RootButtressCount,
                        3,
                        8);
                    bool sectorAligned = branch.BranchOrder == 0 &&
                        lobeOwnedRings[ring] &&
                        lobeOwnedRings[ring + 1] &&
                        currentSegments % rootCount == 0 &&
                        nextSegments % rootCount == 0;
                    AppendMixedResolutionStrip(
                        currentRing,
                        currentSegments,
                        nextRing,
                        nextSegments,
                        sectorAligned ? rootCount : 1,
                        vertices,
                        normals,
                        triangles,
                        sideTriangleUsesContour,
                        sideTriangleExpectedDirections,
                        ref alternateQuadDiagonalCount);
                    int stitchTriangles =
                        (triangles.Count - trianglesBeforeStitch) / 3;
                    if (branch.BranchOrder == 0)
                    {
                        trunkMixedResolutionStripCount++;
                        trunkStitchTriangleCount += stitchTriangles;
                    }
                    continue;
                }

                bool generatedContourSelfIntersects = false;
                Vector3[] contourExpectedBySide = boundaryTransition
                    ? BuildGeneratedContourExpectedDirections(
                        definition,
                        samples,
                        ring,
                        currentSegments,
                        vertices,
                        currentRing,
                        nextRing,
                        out generatedContourSelfIntersects)
                    : useContour
                        ? BuildRootContourExpectedDirections(
                            definition,
                            branch,
                            samples,
                            ring,
                            currentSegments)
                        : null;
                if (generatedContourSelfIntersects)
                {
                    failure = "Boundary transition contour self-intersects at strip " +
                        ring + ".";
                    activeFailureStage = "FINAL_TRIANGULATION";
                    return false;
                }

                for (int side = 0; side < currentSegments; side++)
                {
                    int a = currentRing + side;
                    int b = nextRing + side;
                    int c = nextRing + side + 1;
                    int d = currentRing + side + 1;
                    Vector3 contourExpected = useContour &&
                        contourExpectedBySide != null &&
                        side < contourExpectedBySide.Length
                            ? contourExpectedBySide[side]
                            : Vector3.zero;

                    AppendBestOutwardQuad(
                        a,
                        b,
                        c,
                        d,
                        vertices,
                        normals,
                        triangles,
                        useContour,
                        contourExpected,
                        sideTriangleUsesContour,
                        sideTriangleExpectedDirections,
                        ref alternateQuadDiagonalCount);
                }
            }
            int sideTriangleEnd = triangles.Count;

            int zeroLengthRingSegments = 0;
            for (int sampleIndex = 1; sampleIndex < samples.Count; sampleIndex++)
            {
                if ((samples[sampleIndex].Position -
                     samples[sampleIndex - 1].Position).sqrMagnitude <= Epsilon)
                {
                    zeroLengthRingSegments++;
                }
            }

            branchAuditRecords.Add(new TreeBarkMeshBranchAuditRecord
            {
                Branch = branch,
                SideTriangleStart = sideTriangleStart,
                SideTriangleCount = sideTriangleEnd - sideTriangleStart,
                RadialSegments = maximumRadialSegments,
                RingCount = samples.Count,
                RootCenter = samples[0].Position,
                RootRadius = samples[0].Radius,
                ZeroLengthRingSegmentCount = zeroLengthRingSegments,
                SideTriangleUsesContour = sideTriangleUsesContour.ToArray(),
                SideTriangleExpectedDirections =
                    sideTriangleExpectedDirections.ToArray()
            });

            if (branch.BranchOrder == 0 && settings.CapTrunkBase)
            {
                AppendCap(
                    samples,
                    0,
                    ringRadialSegments[0],
                    definition,
                    branch,
                    false,
                    vertices,
                    normals,
                    tangents,
                    colours,
                    uv0,
                    triangles,
                    capAuditRecords);
                capCount++;
            }

            if (settings.CapBranchTips)
            {
                int tipSegments = ringRadialSegments[samples.Count - 1];
                if (branch.BranchOrder == 0 && trunkTipClosure.Applied)
                {
                    AppendTaperedTrunkTipClosure(
                        samples,
                        samples.Count - 1,
                        tipSegments,
                        definition,
                        branch,
                        trunkTipClosure.ApexPosition,
                        vertices,
                        normals,
                        tangents,
                        colours,
                        uv0,
                        triangles,
                        capAuditRecords);
                }
                else
                {
                    AppendCap(
                        samples,
                        samples.Count - 1,
                        tipSegments,
                        definition,
                        branch,
                        true,
                        vertices,
                        normals,
                        tangents,
                        colours,
                        uv0,
                        triangles,
                        capAuditRecords);
                }

                capCount++;
            }

            if (geometryAccounting != null)
            {
                AppendGeometryAccounting(
                    geometryAccounting,
                    definition,
                    branch,
                    samples,
                    ringRadialSegments,
                    groundContactBoostedRings,
                    radialTransitionCount,
                    branch.BranchOrder == 0
                        ? trunkMixedResolutionStripCount
                        : 0,
                    branch.BranchOrder == 0
                        ? trunkStitchTriangleCount
                        : 0,
                    (sideTriangleEnd - sideTriangleStart) / 3,
                    sourceSampleCount,
                    rootRefinementInsertedRings,
                    twistRefinementInsertedRings,
                    adaptiveShapeRefinementInsertedRings,
                    efficiencyPolicyRemovedRings,
                    topologyRepairRemovedRings,
                    branchVertexStart,
                    branchTriangleIndexStart,
                    vertices.Count,
                    triangles.Count);
            }
            return true;
        }

        private static void RemoveTopologyCollapsedCircularSamples(
            List<RenderSample> samples,
            int radialSegments,
            ref int removalCount)
        {
            if (samples == null || samples.Count < 3)
            {
                return;
            }

            radialSegments = Mathf.Max(3, radialSegments);
            int safety = Mathf.Min(12, samples.Count - 2);
            for (int pass = 0; pass < safety; pass++)
            {
                int invalidStrip = -1;
                for (int ring = 0; ring < samples.Count - 1; ring++)
                {
                    if (!HasViableCircularStrip(
                            samples[ring],
                            samples[ring + 1],
                            radialSegments))
                    {
                        invalidStrip = ring;
                        break;
                    }
                }

                if (invalidStrip < 0 || samples.Count <= 3)
                {
                    return;
                }

                int removeIndex;
                if (invalidStrip <= 0)
                {
                    removeIndex = 1;
                }
                else if (invalidStrip >= samples.Count - 2)
                {
                    removeIndex = samples.Count - 2;
                }
                else
                {
                    float removeCurrentCost = Vector3.Distance(
                        samples[invalidStrip - 1].Position,
                        samples[invalidStrip + 1].Position);
                    float removeNextCost = Vector3.Distance(
                        samples[invalidStrip].Position,
                        samples[invalidStrip + 2].Position);
                    removeIndex = removeCurrentCost <= removeNextCost
                        ? invalidStrip
                        : invalidStrip + 1;
                    removeIndex = Mathf.Clamp(
                        removeIndex,
                        1,
                        samples.Count - 2);
                }

                samples.RemoveAt(removeIndex);
                removalCount++;
                RebuildTransportedFrames(samples);
            }
        }

        private static bool HasViableCircularStrip(
            RenderSample current,
            RenderSample next,
            int radialSegments)
        {
            const float minimumOrientation = 0.05f;
            int firstDelta = -radialSegments / 2;

            for (int candidateIndex = 0;
                 candidateIndex < radialSegments;
                 candidateIndex++)
            {
                int phaseDelta = firstDelta + candidateIndex;
                bool candidatePassed = true;
                for (int side = 0; side < radialSegments; side++)
                {
                    BuildRingVertex(
                        current,
                        radialSegments,
                        side,
                        out Vector3 aPosition,
                        out Vector3 aNormal);
                    BuildRingVertex(
                        current,
                        radialSegments,
                        side + 1,
                        out Vector3 dPosition,
                        out Vector3 dNormal);
                    BuildRingVertex(
                        next,
                        radialSegments,
                        side + phaseDelta,
                        out Vector3 bPosition,
                        out Vector3 bNormal);
                    BuildRingVertex(
                        next,
                        radialSegments,
                        side + 1 + phaseDelta,
                        out Vector3 cPosition,
                        out Vector3 cNormal);

                    bool currentDiagonal = IsViableTriangle(
                            aPosition,
                            dPosition,
                            cPosition,
                            aNormal,
                            dNormal,
                            cNormal,
                            TriangleAreaSquaredEpsilon,
                            minimumOrientation) &&
                        IsViableTriangle(
                            aPosition,
                            cPosition,
                            bPosition,
                            aNormal,
                            cNormal,
                            bNormal,
                            TriangleAreaSquaredEpsilon,
                            minimumOrientation);
                    bool alternateDiagonal = IsViableTriangle(
                            aPosition,
                            dPosition,
                            bPosition,
                            aNormal,
                            dNormal,
                            bNormal,
                            TriangleAreaSquaredEpsilon,
                            minimumOrientation) &&
                        IsViableTriangle(
                            dPosition,
                            cPosition,
                            bPosition,
                            dNormal,
                            cNormal,
                            bNormal,
                            TriangleAreaSquaredEpsilon,
                            minimumOrientation);
                    if (!currentDiagonal && !alternateDiagonal)
                    {
                        candidatePassed = false;
                        break;
                    }
                }

                if (candidatePassed)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsViableTriangle(
            Vector3 aPosition,
            Vector3 bPosition,
            Vector3 cPosition,
            Vector3 aNormal,
            Vector3 bNormal,
            Vector3 cNormal,
            float minimumAreaSquared,
            float minimumOrientation)
        {
            Vector3 faceNormal = Vector3.Cross(
                bPosition - aPosition,
                cPosition - aPosition);
            if (faceNormal.sqrMagnitude <= minimumAreaSquared)
            {
                return false;
            }

            Vector3 expected = aNormal + bNormal + cNormal;
            if (expected.sqrMagnitude <= Epsilon)
            {
                return false;
            }

            return Vector3.Dot(
                faceNormal.normalized,
                expected.normalized) >= minimumOrientation;
        }

        private static void ReduceCircularBranchRenderSamples(
            List<RenderSample> samples,
            int branchOrder,
            TreeBarkMeshSettings settings,
            int protectedRootRingCount)
        {
            if (samples == null || samples.Count < 4 ||
                settings == null)
            {
                return;
            }

            int count = samples.Count;
            var keep = new bool[count];
            int protectedCount = Mathf.Clamp(
                protectedRootRingCount,
                2,
                count);
            int keptCount = 0;
            for (int index = 0; index < protectedCount; index++)
            {
                keep[index] = true;
                keptCount++;
            }

            // Keep the last two rings so tip taper and cap orientation remain
            // stable even when the intervening branch span is nearly linear.
            for (int index = Mathf.Max(protectedCount, count - 2);
                 index < count;
                 index++)
            {
                if (!keep[index])
                {
                    keep[index] = true;
                    keptCount++;
                }
            }

            int minimumRingCount = Mathf.Clamp(
                settings.ResolveMinimumBranchRenderRings(branchOrder),
                keptCount,
                count);
            float maximumPositionErrorInRadii = Mathf.Max(
                0.0001f,
                settings.ResolveBranchPositionErrorInRadii(branchOrder));
            float maximumRadiusErrorRatio = Mathf.Max(
                0.0001f,
                settings.ResolveBranchRadiusErrorRatio(branchOrder));
            float maximumTangentErrorDegrees = Mathf.Max(
                0.01f,
                settings.ResolveBranchTangentErrorDegrees(branchOrder));

            while (true)
            {
                int bestIndex = -1;
                float bestScore = float.NegativeInfinity;
                int left = 0;
                while (left < count - 1)
                {
                    while (left < count && !keep[left])
                    {
                        left++;
                    }
                    if (left >= count - 1)
                    {
                        break;
                    }

                    int right = left + 1;
                    while (right < count && !keep[right])
                    {
                        right++;
                    }
                    if (right >= count)
                    {
                        break;
                    }

                    Vector3 chord =
                        samples[right].Position - samples[left].Position;
                    float normalizedSpan =
                        samples[right].NormalizedDistance -
                        samples[left].NormalizedDistance;
                    for (int candidate = left + 1;
                         candidate < right;
                         candidate++)
                    {
                        float t = normalizedSpan > Epsilon
                            ? Mathf.Clamp01(
                                (samples[candidate].NormalizedDistance -
                                 samples[left].NormalizedDistance) /
                                normalizedSpan)
                            : (candidate - left) /
                                (float)(right - left);
                        Vector3 expectedPosition = Vector3.Lerp(
                            samples[left].Position,
                            samples[right].Position,
                            t);
                        float expectedRadius = Mathf.Lerp(
                            samples[left].Radius,
                            samples[right].Radius,
                            t);
                        float localRadius = Mathf.Max(
                            settings.MinimumRenderedRadius,
                            Mathf.Max(
                                samples[candidate].Radius,
                                expectedRadius));
                        float positionErrorInRadii = Vector3.Distance(
                            samples[candidate].Position,
                            expectedPosition) / localRadius;
                        float radiusErrorRatio = Mathf.Abs(
                            samples[candidate].Radius - expectedRadius) /
                            localRadius;

                        Vector3 expectedTangent = Vector3.Slerp(
                            samples[left].Tangent,
                            samples[right].Tangent,
                            t);
                        expectedTangent = SafeNormalize(
                            expectedTangent,
                            chord.sqrMagnitude > Epsilon
                                ? chord.normalized
                                : samples[candidate].Tangent);
                        float tangentErrorDegrees = Vector3.Angle(
                            samples[candidate].Tangent,
                            expectedTangent);

                        float score = Mathf.Max(
                            positionErrorInRadii /
                                maximumPositionErrorInRadii,
                            Mathf.Max(
                                radiusErrorRatio /
                                    maximumRadiusErrorRatio,
                                tangentErrorDegrees /
                                    maximumTangentErrorDegrees));
                        if (score > bestScore + Epsilon ||
                            (Mathf.Abs(score - bestScore) <= Epsilon &&
                             (bestIndex < 0 || candidate < bestIndex)))
                        {
                            bestScore = score;
                            bestIndex = candidate;
                        }
                    }

                    left = right;
                }

                bool minimumRequiresAnotherRing =
                    keptCount < minimumRingCount;
                bool errorRequiresAnotherRing =
                    bestScore > 1f + Epsilon;
                if (bestIndex < 0 ||
                    (!minimumRequiresAnotherRing &&
                     !errorRequiresAnotherRing))
                {
                    break;
                }

                keep[bestIndex] = true;
                keptCount++;
            }

            var reduced = new List<RenderSample>(keptCount);
            for (int index = 0; index < count; index++)
            {
                if (keep[index])
                {
                    reduced.Add(samples[index]);
                }
            }

            samples.Clear();
            samples.AddRange(reduced);
            RebuildTransportedFrames(samples);
        }

        private static void AppendGeometryAccounting(
            List<TreeBarkMeshBranchGeometryAccounting> accounting,
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            IReadOnlyList<int> ringRadialSegments,
            IReadOnlyList<bool> groundContactBoostedRings,
            int radialTransitionCount,
            int mixedResolutionStripCount,
            int stitchTriangleCount,
            int sideTriangleCount,
            int sourceSampleCount,
            int rootRefinementInsertedRings,
            int twistRefinementInsertedRings,
            int adaptiveShapeRefinementInsertedRings,
            int efficiencyPolicyRemovedRings,
            int topologyRepairRemovedRings,
            int branchVertexStart,
            int branchTriangleIndexStart,
            int branchVertexEnd,
            int branchTriangleIndexEnd)
        {
            if (accounting == null || branch == null || samples == null ||
                ringRadialSegments == null ||
                ringRadialSegments.Count != samples.Count ||
                groundContactBoostedRings == null ||
                groundContactBoostedRings.Count != samples.Count)
            {
                return;
            }

            int ringCount = samples.Count;
            int sideVertexCount = 0;
            int minimumRadialSegments = 0;
            int maximumRadialSegments = 0;
            long radialSegmentSum = 0L;
            for (int index = 0; index < ringCount; index++)
            {
                int segments = Mathf.Max(3, ringRadialSegments[index]);
                sideVertexCount += segments + 1;
                minimumRadialSegments = index == 0
                    ? segments
                    : Mathf.Min(minimumRadialSegments, segments);
                maximumRadialSegments = Mathf.Max(
                    maximumRadialSegments,
                    segments);
                radialSegmentSum += segments;
            }
            float averageRadialSegments = ringCount > 0
                ? radialSegmentSum / (float)ringCount
                : 0f;
            int totalVertices = Mathf.Max(
                0,
                branchVertexEnd - branchVertexStart);
            int totalTriangles = Mathf.Max(
                0,
                branchTriangleIndexEnd - branchTriangleIndexStart) / 3;
            int rootIntervals = 0;
            int rootRings = 0;
            int rootLobeIntervals = 0;
            int rootLobeRings = 0;
            int persistenceIntervals = 0;
            int persistenceRings = 0;
            int rootVertices = 0;
            int rootTriangles = 0;
            int rootLobeVertices = 0;
            int rootLobeTriangles = 0;
            int persistenceVertices = 0;
            int persistenceTriangles = 0;
            int ordinaryVertices = 0;
            int ordinaryTriangles = 0;
            long rootLobeRadialSum = 0L;
            int groundContactRadialSegments = 0;
            int groundContactBoostedRingCount = 0;
            float groundContactBoostReleaseNormalizedDistance = 0f;
            long persistenceRadialSum = 0L;
            long ordinaryRadialSum = 0L;
            int ordinaryRings = 0;
            if (branch.BranchOrder == 0 && definition != null)
            {
                float rootLobeLimit = CalculateEffectiveRootCollapseHeight(
                    definition.ResolvedParameters);
                float rootOwnedLimit = CalculateEffectiveRootTransitionHeight(
                    definition.ResolvedParameters);
                for (int index = 0; index < ringCount; index++)
                {
                    float distance = samples[index].NormalizedDistance;
                    int segments = Mathf.Max(3, ringRadialSegments[index]);
                    if (index == 0)
                    {
                        groundContactRadialSegments = segments;
                    }
                    if (groundContactBoostedRings[index])
                    {
                        groundContactBoostedRingCount++;
                        groundContactBoostReleaseNormalizedDistance = Mathf.Max(
                            groundContactBoostReleaseNormalizedDistance,
                            distance);
                    }
                    int verticesAtRing = segments + 1;
                    if (distance <= rootLobeLimit + Epsilon)
                    {
                        rootLobeRings++;
                        rootRings++;
                        rootLobeVertices += verticesAtRing;
                        rootVertices += verticesAtRing;
                        rootLobeRadialSum += segments;
                    }
                    else if (distance <= rootOwnedLimit + Epsilon)
                    {
                        persistenceRings++;
                        rootRings++;
                        persistenceVertices += verticesAtRing;
                        rootVertices += verticesAtRing;
                        persistenceRadialSum += segments;
                    }
                    else
                    {
                        ordinaryRings++;
                        ordinaryVertices += verticesAtRing;
                        ordinaryRadialSum += segments;
                    }
                }

                for (int index = 0; index < ringCount - 1; index++)
                {
                    int stripTriangles =
                        Mathf.Max(3, ringRadialSegments[index]) +
                        Mathf.Max(3, ringRadialSegments[index + 1]);
                    float distance = samples[index].NormalizedDistance;
                    if (distance < rootLobeLimit - Epsilon)
                    {
                        rootLobeIntervals++;
                        rootIntervals++;
                        rootLobeTriangles += stripTriangles;
                        rootTriangles += stripTriangles;
                    }
                    else if (distance < rootOwnedLimit - Epsilon)
                    {
                        persistenceIntervals++;
                        rootIntervals++;
                        persistenceTriangles += stripTriangles;
                        rootTriangles += stripTriangles;
                    }
                    else
                    {
                        ordinaryTriangles += stripTriangles;
                    }
                }
            }

            CalculateSegmentMetrics(
                samples,
                out float averageLength,
                out float maximumLength,
                out float averageTurn,
                out float maximumTurn);
            accounting.Add(new TreeBarkMeshBranchGeometryAccounting
            {
                StableBranchId = branch.StableBranchId,
                BranchOrder = branch.BranchOrder,
                SourceSampleCount = sourceSampleCount,
                RenderRingCount = ringCount,
                RadialSegments = maximumRadialSegments,
                MinimumRadialSegments = minimumRadialSegments,
                MaximumRadialSegments = maximumRadialSegments,
                AverageRadialSegments = averageRadialSegments,
                RadialTransitionCount = radialTransitionCount,
                MixedResolutionStripCount = mixedResolutionStripCount,
                StitchTriangleCount = stitchTriangleCount,
                RootLobeAverageRadialSegments = rootLobeRings > 0
                    ? rootLobeRadialSum / (float)rootLobeRings
                    : 0f,
                GroundContactRadialSegments = groundContactRadialSegments,
                GroundContactBoostedRingCount = groundContactBoostedRingCount,
                GroundContactBoostReleaseNormalizedDistance =
                    groundContactBoostReleaseNormalizedDistance,
                ButtressPersistenceAverageRadialSegments =
                    persistenceRings > 0
                        ? persistenceRadialSum / (float)persistenceRings
                        : 0f,
                OrdinaryTrunkAverageRadialSegments = ordinaryRings > 0
                    ? ordinaryRadialSum / (float)ordinaryRings
                    : 0f,
                SideVertexCount = sideVertexCount,
                SideTriangleCount = sideTriangleCount,
                CapVertexCount = Mathf.Max(0, totalVertices - sideVertexCount),
                CapTriangleCount = Mathf.Max(
                    0,
                    totalTriangles - sideTriangleCount),
                SeamDuplicateVertexCount = ringCount,
                RootZoneRingCount = rootRings,
                RootZoneIntervalCount = rootIntervals,
                RootZoneVertexCount = rootVertices,
                RootZoneTriangleCount = rootTriangles,
                RootLobeRingCount = rootLobeRings,
                RootLobeIntervalCount = rootLobeIntervals,
                RootLobeVertexCount = rootLobeVertices,
                RootLobeTriangleCount = rootLobeTriangles,
                ButtressPersistenceRingCount = persistenceRings,
                ButtressPersistenceIntervalCount = persistenceIntervals,
                ButtressPersistenceVertexCount = persistenceVertices,
                ButtressPersistenceTriangleCount = persistenceTriangles,
                OrdinaryTrunkVertexCount = branch.BranchOrder == 0
                    ? ordinaryVertices
                    : 0,
                OrdinaryTrunkTriangleCount = branch.BranchOrder == 0
                    ? ordinaryTriangles
                    : 0,
                InsertedRenderRingCount = Mathf.Max(
                    0,
                    rootRefinementInsertedRings +
                    twistRefinementInsertedRings +
                    adaptiveShapeRefinementInsertedRings),
                RootRefinementInsertedRingCount =
                    rootRefinementInsertedRings,
                TwistRefinementInsertedRingCount =
                    twistRefinementInsertedRings,
                AdaptiveShapeRefinementInsertedRingCount =
                    adaptiveShapeRefinementInsertedRings,
                RemovedRenderRingCount = Mathf.Max(
                    0,
                    efficiencyPolicyRemovedRings +
                    topologyRepairRemovedRings),
                EfficiencyPolicyRemovedRingCount =
                    efficiencyPolicyRemovedRings,
                TopologyRepairRemovedRingCount =
                    topologyRepairRemovedRings,
                AverageSegmentLength = averageLength,
                MaximumSegmentLength = maximumLength,
                AverageTurnDegrees = averageTurn,
                MaximumTurnDegrees = maximumTurn
            });
        }

        private static void CalculateSegmentMetrics(
            IReadOnlyList<RenderSample> samples,
            out float averageLength,
            out float maximumLength,
            out float averageTurn,
            out float maximumTurn)
        {
            averageLength = 0f;
            maximumLength = 0f;
            averageTurn = 0f;
            maximumTurn = 0f;
            if (samples == null || samples.Count < 2)
            {
                return;
            }

            int lengthCount = 0;
            for (int index = 1; index < samples.Count; index++)
            {
                float length = Vector3.Distance(
                    samples[index - 1].Position,
                    samples[index].Position);
                averageLength += length;
                maximumLength = Mathf.Max(maximumLength, length);
                lengthCount++;
            }
            averageLength /= Mathf.Max(1, lengthCount);

            int turnCount = 0;
            for (int index = 1; index < samples.Count - 1; index++)
            {
                Vector3 incoming =
                    samples[index].Position - samples[index - 1].Position;
                Vector3 outgoing =
                    samples[index + 1].Position - samples[index].Position;
                if (incoming.sqrMagnitude <= Epsilon ||
                    outgoing.sqrMagnitude <= Epsilon)
                {
                    continue;
                }

                float turn = Vector3.Angle(incoming, outgoing);
                averageTurn += turn;
                maximumTurn = Mathf.Max(maximumTurn, turn);
                turnCount++;
            }
            averageTurn /= Mathf.Max(1, turnCount);
        }

        private static List<RenderSample> RefineTrunkRenderSamples(
            IReadOnlyList<RenderSample> source,
            TreeResolvedParameters parameters,
            TreeBarkMeshSettings settings,
            out int rootRefinementInsertedRings,
            out int twistRefinementInsertedRings,
            out int adaptiveShapeRefinementInsertedRings)
        {
            rootRefinementInsertedRings = 0;
            twistRefinementInsertedRings = 0;
            adaptiveShapeRefinementInsertedRings = 0;
            if (source == null || source.Count < 2)
            {
                return source == null
                    ? new List<RenderSample>()
                    : new List<RenderSample>(source);
            }

            bool legacyAxialSampling = settings.UsesLegacyAxialSampling;
            float maximumTwistStepDegrees =
                settings.ResolveMaximumTrunkTwistStepDegrees();
            float maximumTangentStepDegrees =
                settings.ResolveMaximumTrunkTangentStepDegrees();
            float maximumRadiusChangeRatio =
                settings.ResolveMaximumTrunkRadiusChangeRatio();
            float maximumRootEnvelopeStep =
                settings.ResolveMaximumRootEnvelopeStep();
            int minimumRootCollapseIntervals = Mathf.Max(
                1,
                legacyAxialSampling
                    ? GetActiveRootCollapseProfile().CollapseIntervals
                    : settings.ResolveRootCollapseIntervals());
            float rootTransitionEnd =
                CalculateEffectiveRootTransitionHeight(parameters);
            float rootPlateauEnd = CalculateRootGroundPlateauEnd(parameters);
            float rootCollapseEnd =
                CalculateEffectiveRootCollapseHeight(parameters);
            float rootCollapseSpan = Mathf.Max(
                0.0001f,
                rootCollapseEnd - rootPlateauEnd);
            float denseRootSamplingEnd = legacyAxialSampling
                ? rootTransitionEnd
                : rootCollapseEnd;
            float rootStepDomain = legacyAxialSampling
                ? rootTransitionEnd
                : rootCollapseEnd;
            float maximumRootStep = Mathf.Max(
                0.00005f,
                Mathf.Min(
                    rootStepDomain / 20f,
                    rootCollapseSpan / minimumRootCollapseIntervals));
            int maximumSubdivisions =
                settings.EfficiencyPolicy ==
                    TreeBarkMeshEfficiencyPolicy.Aggressive ||
                settings.EfficiencyPolicy ==
                    TreeBarkMeshEfficiencyPolicy.AxialAggressive
                    ? 16
                    : 24;
            var refined = new List<RenderSample>(source.Count + 24);
            refined.Add(source[0]);

            for (int index = 0; index < source.Count - 1; index++)
            {
                RenderSample a = source[index];
                RenderSample b = source[index + 1];
                bool crossesDenseRootBoundary =
                    !legacyAxialSampling &&
                    a.NormalizedDistance < denseRootSamplingEnd - Epsilon &&
                    b.NormalizedDistance > denseRootSamplingEnd + Epsilon;
                if (crossesDenseRootBoundary)
                {
                    float boundaryT = Mathf.InverseLerp(
                        a.NormalizedDistance,
                        b.NormalizedDistance,
                        denseRootSamplingEnd);
                    RenderSample boundary = InterpolateRenderSample(
                        a,
                        b,
                        boundaryT);
                    if (settings.GeometryAuditTelemetryEnabled)
                    {
                        rootRefinementInsertedRings++;
                    }
                    AppendRefinedTrunkSpan(
                        refined,
                        a,
                        boundary,
                        parameters,
                        settings,
                        true,
                        legacyAxialSampling,
                        maximumRootStep,
                        maximumTwistStepDegrees,
                        maximumTangentStepDegrees,
                        maximumRadiusChangeRatio,
                        maximumRootEnvelopeStep,
                        maximumSubdivisions,
                        ref rootRefinementInsertedRings,
                        ref twistRefinementInsertedRings,
                        ref adaptiveShapeRefinementInsertedRings);
                    AppendRefinedTrunkSpan(
                        refined,
                        boundary,
                        b,
                        parameters,
                        settings,
                        false,
                        legacyAxialSampling,
                        maximumRootStep,
                        maximumTwistStepDegrees,
                        maximumTangentStepDegrees,
                        maximumRadiusChangeRatio,
                        maximumRootEnvelopeStep,
                        maximumSubdivisions,
                        ref rootRefinementInsertedRings,
                        ref twistRefinementInsertedRings,
                        ref adaptiveShapeRefinementInsertedRings);
                    continue;
                }

                AppendRefinedTrunkSpan(
                    refined,
                    a,
                    b,
                    parameters,
                    settings,
                    a.NormalizedDistance < denseRootSamplingEnd,
                    legacyAxialSampling,
                    maximumRootStep,
                    maximumTwistStepDegrees,
                    maximumTangentStepDegrees,
                    maximumRadiusChangeRatio,
                    maximumRootEnvelopeStep,
                    maximumSubdivisions,
                    ref rootRefinementInsertedRings,
                    ref twistRefinementInsertedRings,
                    ref adaptiveShapeRefinementInsertedRings);
            }

            RebuildTransportedFrames(refined);
            return refined;
        }

        private static void AppendRefinedTrunkSpan(
            List<RenderSample> refined,
            RenderSample a,
            RenderSample b,
            TreeResolvedParameters parameters,
            TreeBarkMeshSettings settings,
            bool applyDenseRootSampling,
            bool legacyAxialSampling,
            float maximumRootStep,
            float maximumTwistStepDegrees,
            float maximumTangentStepDegrees,
            float maximumRadiusChangeRatio,
            float maximumRootEnvelopeStep,
            int maximumSubdivisions,
            ref int rootRefinementInsertedRings,
            ref int twistRefinementInsertedRings,
            ref int adaptiveShapeRefinementInsertedRings)
        {
            float normalizedSpan = Mathf.Max(
                0f,
                b.NormalizedDistance - a.NormalizedDistance);
            float twistSpan = Mathf.Abs(
                ResolveAuthoredTrunkSurfaceRollDegrees(
                    parameters,
                    b.NormalizedDistance) -
                ResolveAuthoredTrunkSurfaceRollDegrees(
                    parameters,
                    a.NormalizedDistance));
            int twistSubdivisions = Mathf.Max(
                1,
                Mathf.CeilToInt(
                    twistSpan / maximumTwistStepDegrees));
            int rootSubdivisions = applyDenseRootSampling
                ? Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        normalizedSpan / maximumRootStep))
                : 1;

            int adaptiveShapeSubdivisions = 1;
            if (!legacyAxialSampling)
            {
                float tangentChange = Vector3.Angle(
                    a.Tangent,
                    b.Tangent);
                int tangentSubdivisions = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        tangentChange / maximumTangentStepDegrees));
                float localRadius = Mathf.Max(
                    settings.MinimumRenderedRadius,
                    Mathf.Max(a.Radius, b.Radius));
                float radiusChangeRatio = Mathf.Abs(
                    b.Radius - a.Radius) / localRadius;
                int radiusSubdivisions = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        radiusChangeRatio /
                        maximumRadiusChangeRatio));

                EvaluateRootEnvelopes(
                    parameters,
                    a.NormalizedDistance,
                    out float aBodyEnvelope,
                    out float aFootEnvelope);
                EvaluateRootEnvelopes(
                    parameters,
                    b.NormalizedDistance,
                    out float bBodyEnvelope,
                    out float bFootEnvelope);
                float envelopeChange = Mathf.Max(
                    Mathf.Abs(bBodyEnvelope - aBodyEnvelope),
                    Mathf.Abs(bFootEnvelope - aFootEnvelope));
                int envelopeSubdivisions = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        envelopeChange / maximumRootEnvelopeStep));
                adaptiveShapeSubdivisions = Mathf.Max(
                    tangentSubdivisions,
                    Mathf.Max(
                        radiusSubdivisions,
                        envelopeSubdivisions));
            }

            int subdivisions = Mathf.Clamp(
                Mathf.Max(
                    twistSubdivisions,
                    Mathf.Max(
                        rootSubdivisions,
                        adaptiveShapeSubdivisions)),
                1,
                maximumSubdivisions);
            if (settings.GeometryAuditTelemetryEnabled)
            {
                int remainingInsertedRings = Mathf.Max(
                    0,
                    subdivisions - 1);
                int rootContribution = Mathf.Min(
                    remainingInsertedRings,
                    Mathf.Max(0, rootSubdivisions - 1));
                rootRefinementInsertedRings += rootContribution;
                remainingInsertedRings -= rootContribution;

                int twistContribution = Mathf.Min(
                    remainingInsertedRings,
                    Mathf.Max(0, twistSubdivisions - 1));
                twistRefinementInsertedRings += twistContribution;
                remainingInsertedRings -= twistContribution;

                adaptiveShapeRefinementInsertedRings +=
                    remainingInsertedRings;
            }

            for (int step = 1; step <= subdivisions; step++)
            {
                float t = step / (float)subdivisions;
                refined.Add(InterpolateRenderSample(a, b, t));
            }
        }

        private static bool PrepareTopologySafeTrunkTip(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            List<RenderSample> samples,
            int radialSegments,
            out TrunkTipClosure closure,
            out string failure)
        {
            closure = default;
            failure = string.Empty;
            if (samples == null || samples.Count < 2)
            {
                failure =
                    "Trunk produced fewer than two samples before tip closure.";
                return false;
            }

            const float MinimumOrientation = 0.05f;
            const int MaximumRemovedRings = 16;
            radialSegments = Mathf.Max(3, radialSegments);
            var original = new List<RenderSample>(samples);
            int originalCount = original.Count;

            for (int pass = 0; pass <= MaximumRemovedRings; pass++)
            {
                RebuildTransportedFrames(samples);
                List<int> unsafeStrips = FindTopologyUnsafeTrunkStrips(
                    definition,
                    branch,
                    samples,
                    radialSegments,
                    MinimumOrientation);
                if (unsafeStrips.Count == 0)
                {
                    break;
                }

                int firstUnsafe = unsafeStrips[0];
                bool terminalSuffix =
                    unsafeStrips[unsafeStrips.Count - 1] ==
                        samples.Count - 2;
                for (int index = 1;
                     terminalSuffix && index < unsafeStrips.Count;
                     index++)
                {
                    terminalSuffix =
                        unsafeStrips[index] == firstUnsafe + index;
                }

                if (!terminalSuffix)
                {
                    failure =
                        "Trunk topology contains non-terminal unsafe strips. " +
                        "A tip closure may not hide an interior surface defect. " +
                        "firstUnsafe=" + firstUnsafe +
                        " unsafeCount=" + unsafeStrips.Count + ".\n" +
                        BuildRootRingCorrespondenceDiagnostic(
                            definition,
                            branch,
                            samples,
                            radialSegments,
                            firstUnsafe,
                            MinimumOrientation);
                    return false;
                }

                int keepCount = firstUnsafe + 1;
                if (keepCount < 2)
                {
                    failure =
                        "Trunk topology became unsafe before a valid terminal " +
                        "closure ring was available.";
                    return false;
                }

                int removedAfterTrim = originalCount - keepCount;
                if (removedAfterTrim > MaximumRemovedRings)
                {
                    failure =
                        "Terminal trunk topology requires removing more than " +
                        MaximumRemovedRings + " rings. firstUnsafe=" +
                        firstUnsafe + " originalRings=" + originalCount + ".";
                    return false;
                }

                samples.RemoveRange(
                    keepCount,
                    samples.Count - keepCount);
            }

            RebuildTransportedFrames(samples);
            List<int> remainingUnsafe = FindTopologyUnsafeTrunkStrips(
                definition,
                branch,
                samples,
                radialSegments,
                MinimumOrientation);
            if (remainingUnsafe.Count > 0)
            {
                failure =
                    "Terminal trunk closure could not expose a fully safe " +
                    "side-surface prefix. remainingUnsafe=" +
                    remainingUnsafe.Count + ".";
                return false;
            }

            int removedRings = originalCount - samples.Count;
            if (removedRings == 0)
            {
                return true;
            }

            int closureIndex = samples.Count - 1;
            RenderSample closureSample = samples[closureIndex];
            RenderSample originalTip = original[original.Count - 1];
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                closureSample,
                out Vector3 closureAxis,
                out _,
                out _);
            closureAxis = SafeNormalize(closureAxis, closureSample.Tangent);

            float remainingArcLength = 0f;
            for (int index = closureIndex; index < original.Count - 1; index++)
            {
                remainingArcLength += Vector3.Distance(
                    original[index].Position,
                    original[index + 1].Position);
            }

            float forwardProjection = Vector3.Dot(
                originalTip.Position - closureSample.Position,
                closureAxis);
            if (forwardProjection <= Epsilon ||
                remainingArcLength <= Epsilon)
            {
                failure =
                    "Terminal trunk samples do not provide a positive forward " +
                    "distance for deterministic tip closure.";
                return false;
            }

            float minimumClosureLength = Mathf.Max(
                0.001f,
                closureSample.Radius * 0.5f);
            float closureLength = Mathf.Clamp(
                forwardProjection,
                minimumClosureLength,
                Mathf.Max(minimumClosureLength, remainingArcLength));
            Vector3 apexPosition =
                closureSample.Position + closureAxis * closureLength;
            if (!IsTaperedTrunkTipClosureViable(
                    definition,
                    branch,
                    samples,
                    closureIndex,
                    radialSegments,
                    apexPosition,
                    closureAxis))
            {
                failure =
                    "Deterministic tapered trunk-tip closure failed its own " +
                    "orientation or area preflight.";
                return false;
            }

            closure = new TrunkTipClosure
            {
                Applied = true,
                ApexPosition = apexPosition,
                RemovedRingCount = removedRings,
                Length = closureLength,
                CollapsedNormalizedSpan = Mathf.Clamp01(
                    originalTip.NormalizedDistance -
                    closureSample.NormalizedDistance)
            };
            return true;
        }

        private static bool IsTaperedTrunkTipClosureViable(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            int radialSegments,
            Vector3 apexPosition,
            Vector3 expectedNormal)
        {
            for (int side = 0; side < radialSegments; side++)
            {
                BuildSurfaceVertex(
                    definition,
                    branch,
                    samples,
                    sampleIndex,
                    side / (float)radialSegments,
                    radialSegments,
                    out Vector3 current,
                    out _,
                    out _,
                    out _);
                BuildSurfaceVertex(
                    definition,
                    branch,
                    samples,
                    sampleIndex,
                    (side + 1) / (float)radialSegments,
                    radialSegments,
                    out Vector3 next,
                    out _,
                    out _,
                    out _);
                Vector3 faceNormal = Vector3.Cross(
                    current - apexPosition,
                    next - apexPosition);
                if (faceNormal.sqrMagnitude <=
                        TriangleAreaSquaredEpsilon ||
                    Vector3.Dot(
                        faceNormal.normalized,
                        expectedNormal.normalized) <= 0f)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ShouldUseRootContourValidation(
            TreeResolvedParameters parameters,
            TreeBranchDefinition branch,
            RenderSample current,
            RenderSample next)
        {
            if (!parameters.RecipeOnlyControlSource ||
                branch == null || branch.BranchOrder != 0)
            {
                return false;
            }

            float effectiveTransition =
                CalculateEffectiveRootTransitionHeight(parameters);
            if (current.NormalizedDistance > effectiveTransition + 0.000001f ||
                next.NormalizedDistance > effectiveTransition + 0.000001f)
            {
                return false;
            }

            EvaluateRootEnvelopes(
                parameters, current.NormalizedDistance,
                out float currentBody, out float currentFoot);
            EvaluateRootEnvelopes(
                parameters, next.NormalizedDistance,
                out float nextBody, out float nextFoot);
            return Mathf.Max(
                Mathf.Max(currentBody, currentFoot),
                Mathf.Max(nextBody, nextFoot)) > 0.000001f;
        }

        private static Vector3[] BuildRootContourExpectedDirections(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int stripIndex,
            int radialSegments)
        {
            RenderSample current = samples[stripIndex];
            RenderSample next = samples[stripIndex + 1];
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters, current,
                out _, out Vector3 currentNormal,
                out Vector3 currentBinormal);
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters, next,
                out _, out Vector3 nextNormal,
                out Vector3 nextBinormal);
            Vector2[] currentContour = BuildDiagnosticRingContour(
                definition, branch, samples, stripIndex, radialSegments,
                current.Position, currentNormal, currentBinormal);
            Vector2[] nextContour = BuildDiagnosticRingContour(
                definition, branch, samples, stripIndex + 1, radialSegments,
                next.Position, nextNormal, nextBinormal);
            float currentArea = CalculateSignedContourArea(currentContour);
            float nextArea = CalculateSignedContourArea(nextContour);
            var expected = new Vector3[radialSegments];
            for (int side = 0; side < radialSegments; side++)
            {
                Vector2 currentOutward = CalculateContourOutwardNormal(
                    currentContour, side, currentArea);
                Vector2 nextOutward = CalculateContourOutwardNormal(
                    nextContour, side, nextArea);
                expected[side] =
                    currentNormal * currentOutward.x +
                    currentBinormal * currentOutward.y +
                    nextNormal * nextOutward.x +
                    nextBinormal * nextOutward.y;
            }

            return expected;
        }


        private static void PrepareBoundaryTransitionSamples(
            TreeDefinition definition,
            List<RenderSample> samples,
            int requestedMorphRings)
        {
            activeBoundaryMorphRingsRequested = requestedMorphRings;
            if (requestedMorphRings <= 0 || samples == null || samples.Count < 3)
            {
                return;
            }

            float boundary = CalculateTournamentBoundaryStart(
                definition.ResolvedParameters);
            int rootIndex = -1;
            for (int index = 0; index < samples.Count - 1; index++)
            {
                if (samples[index].NormalizedDistance <= boundary &&
                    samples[index + 1].NormalizedDistance > boundary)
                {
                    rootIndex = index;
                    break;
                }
            }
            if (rootIndex < 0 || rootIndex >= samples.Count - 1)
            {
                return;
            }

            float minimumLength = Mathf.Max(
                0.08f,
                samples[rootIndex].Radius * 0.24f);
            float traversed = 0f;
            int endpointIndex = rootIndex + 1;
            for (; endpointIndex < samples.Count; endpointIndex++)
            {
                traversed += Vector3.Distance(
                    samples[endpointIndex - 1].Position,
                    samples[endpointIndex].Position);
                if (traversed >= minimumLength)
                {
                    break;
                }
            }
            endpointIndex = Mathf.Clamp(
                endpointIndex,
                rootIndex + 1,
                samples.Count - 1);

            RenderSample root = samples[rootIndex];
            RenderSample endpoint = samples[endpointIndex];
            var inserted = new List<RenderSample>(requestedMorphRings);
            for (int ring = 1; ring <= requestedMorphRings; ring++)
            {
                float t = ring / (float)(requestedMorphRings + 1);
                inserted.Add(InterpolateRenderSample(root, endpoint, t));
            }
            int removeCount = endpointIndex - rootIndex - 1;
            if (removeCount > 0)
            {
                samples.RemoveRange(rootIndex + 1, removeCount);
            }
            samples.InsertRange(rootIndex + 1, inserted);
            RebuildTransportedFrames(samples);
            activeBoundaryCandidateActivated = true;
            activeBoundaryMorphRingsUsed = inserted.Count;
        }

        private static float CalculateTournamentBoundaryStart(
            TreeResolvedParameters parameters)
        {
            if (activeTournamentStrategy ==
                TreeRootCollapseTournamentStrategy.TransportedContourBlend)
            {
                return CalculateEffectiveRootCollapseHeight(parameters);
            }
            return CalculateRootBoundaryEnd(parameters);
        }

        private static void PrepareFrameAdoptionSamples(
            TreeResolvedParameters parameters,
            List<RenderSample> samples,
            int intervals)
        {
            if (samples == null || samples.Count < 3 || intervals < 2)
            {
                return;
            }
            float start = CalculateEffectiveRootCollapseHeight(parameters);
            float end = CalculateEffectiveRootTransitionHeight(parameters);
            int startIndex = -1;
            int endIndex = -1;
            for (int index = 0; index < samples.Count - 1; index++)
            {
                if (startIndex < 0 &&
                    samples[index].NormalizedDistance <= start &&
                    samples[index + 1].NormalizedDistance > start)
                {
                    startIndex = index;
                }
                if (samples[index].NormalizedDistance < end &&
                    samples[index + 1].NormalizedDistance >= end)
                {
                    endIndex = index + 1;
                    break;
                }
            }
            if (startIndex < 0 || endIndex <= startIndex)
            {
                return;
            }
            RenderSample a = samples[startIndex];
            RenderSample b = samples[endIndex];
            var inserted = new List<RenderSample>(intervals - 1);
            for (int i = 1; i < intervals; i++)
            {
                inserted.Add(InterpolateRenderSample(a, b, i / (float)intervals));
            }
            samples.RemoveRange(startIndex + 1, endIndex - startIndex - 1);
            samples.InsertRange(startIndex + 1, inserted);
            RebuildTransportedFrames(samples);
        }

        private static void ResolveBoundaryTransitionRange(
            TreeDefinition definition,
            IReadOnlyList<RenderSample> samples,
            out int rootIndex,
            out int firstMorphIndex,
            out int morphCount)
        {
            rootIndex = -1;
            firstMorphIndex = -1;
            morphCount = 0;
            if (!activeTournamentStrategy.HasValue ||
                samples == null || samples.Count < 2)
            {
                return;
            }
            int requested = GetActiveRootCollapseProfile().BoundaryMorphRings;
            if (requested <= 0)
            {
                return;
            }
            float boundary = CalculateTournamentBoundaryStart(
                definition.ResolvedParameters);
            for (int index = 0; index < samples.Count - 1; index++)
            {
                if (samples[index].NormalizedDistance <= boundary &&
                    samples[index + 1].NormalizedDistance > boundary)
                {
                    rootIndex = index;
                    firstMorphIndex = index + 1;
                    morphCount = Mathf.Min(
                        requested,
                        samples.Count - firstMorphIndex - 1);
                    return;
                }
            }
        }

        private static Vector3 EvaluateCandidateSurfacePosition(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            float normalizedSide,
            out float crossSectionMultiplier)
        {
            RenderSample sample = samples[sampleIndex];
            Vector3 ordinary = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide,
                out crossSectionMultiplier);
            if (branch.BranchOrder != 0 || !activeTournamentStrategy.HasValue)
            {
                return ordinary;
            }

            ResolveBoundaryTransitionRange(
                definition,
                samples,
                out int rootIndex,
                out int firstMorphIndex,
                out int morphCount);
            if (rootIndex < 0 || morphCount <= 0 ||
                sampleIndex < firstMorphIndex ||
                sampleIndex >= firstMorphIndex + morphCount)
            {
                return ordinary;
            }

            Vector3 rootPosition = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                samples[rootIndex],
                normalizedSide,
                out _);
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                samples[rootIndex],
                out _,
                out Vector3 rootNormalAxis,
                out Vector3 rootBinormalAxis);
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                sample,
                out _,
                out Vector3 sampleNormalAxis,
                out Vector3 sampleBinormalAxis);
            Vector3 rootOffset = rootPosition - samples[rootIndex].Position;
            Vector3 carriedRoot = sample.Position +
                sampleNormalAxis * Vector3.Dot(rootOffset, rootNormalAxis) +
                sampleBinormalAxis * Vector3.Dot(rootOffset, rootBinormalAxis);
            int localIndex = sampleIndex - firstMorphIndex;
            float morph = morphCount > 1
                ? localIndex / (float)(morphCount - 1)
                : 0f;
            morph = SmootherStep01(morph);
            activeBoundaryMismatchEvaluated = true;
            activeBoundaryMaximumMismatch = Mathf.Max(
                activeBoundaryMaximumMismatch,
                Vector3.Distance(carriedRoot, ordinary));
            return Vector3.Lerp(carriedRoot, ordinary, morph);
        }

        private static void BuildCandidateSurfaceVertex(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            float normalizedSide,
            int radialSegments,
            out Vector3 position,
            out Vector3 normal,
            out Vector3 circumferenceTangent,
            out float crossSectionMultiplier)
        {
            if (branch.BranchOrder != 0 || !activeTournamentStrategy.HasValue ||
                GetActiveRootCollapseProfile().BoundaryMorphRings <= 0)
            {
                BuildSurfaceVertex(
                    definition, branch, samples, sampleIndex, normalizedSide,
                    radialSegments, out position, out normal,
                    out circumferenceTangent, out crossSectionMultiplier);
                return;
            }

            position = EvaluateCandidateSurfacePosition(
                definition, branch, samples, sampleIndex, normalizedSide,
                out crossSectionMultiplier);
            float sideDelta = 1f / Mathf.Max(48f, radialSegments * 8f);
            Vector3 previousSide = EvaluateCandidateSurfacePosition(
                definition, branch, samples, sampleIndex,
                normalizedSide - sideDelta, out _);
            Vector3 nextSide = EvaluateCandidateSurfacePosition(
                definition, branch, samples, sampleIndex,
                normalizedSide + sideDelta, out _);
            circumferenceTangent = SafeNormalize(
                nextSide - previousSide,
                samples[sampleIndex].Binormal);

            int previousIndex = Mathf.Max(0, sampleIndex - 1);
            int nextIndex = Mathf.Min(samples.Count - 1, sampleIndex + 1);
            Vector3 previousLongitudinal = EvaluateCandidateSurfacePosition(
                definition, branch, samples, previousIndex,
                normalizedSide, out _);
            Vector3 nextLongitudinal = EvaluateCandidateSurfacePosition(
                definition, branch, samples, nextIndex,
                normalizedSide, out _);
            Vector3 longitudinalTangent = SafeNormalize(
                nextLongitudinal - previousLongitudinal,
                samples[sampleIndex].Tangent);
            Vector3 radial = SafeNormalize(
                position - samples[sampleIndex].Position,
                samples[sampleIndex].Normal);
            normal = SafeNormalize(
                Vector3.Cross(circumferenceTangent, longitudinalTangent),
                radial);
            if (Vector3.Dot(normal, radial) < 0f)
            {
                normal = -normal;
            }
            circumferenceTangent = SafeNormalize(
                Vector3.ProjectOnPlane(circumferenceTangent, normal),
                circumferenceTangent);
        }


        private static bool IsBoundaryTransitionStrip(
            TreeDefinition definition,
            IReadOnlyList<RenderSample> samples,
            int ring)
        {
            ResolveBoundaryTransitionRange(
                definition,
                samples,
                out int rootIndex,
                out int firstMorphIndex,
                out int morphCount);
            return rootIndex >= 0 && morphCount > 0 &&
                ring >= rootIndex && ring < firstMorphIndex + morphCount;
        }

        private static Vector3[] BuildGeneratedContourExpectedDirections(
            TreeDefinition definition,
            IReadOnlyList<RenderSample> samples,
            int ring,
            int radialSegments,
            IReadOnlyList<Vector3> positions,
            int currentRingStart,
            int nextRingStart,
            out bool selfIntersects)
        {
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                samples[ring],
                out _, out Vector3 currentNormal, out Vector3 currentBinormal);
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                samples[ring + 1],
                out _, out Vector3 nextNormal, out Vector3 nextBinormal);
            var currentContour = new Vector2[radialSegments];
            var nextContour = new Vector2[radialSegments];
            for (int side = 0; side < radialSegments; side++)
            {
                Vector3 currentOffset = positions[currentRingStart + side] -
                    samples[ring].Position;
                Vector3 nextOffset = positions[nextRingStart + side] -
                    samples[ring + 1].Position;
                currentContour[side] = new Vector2(
                    Vector3.Dot(currentOffset, currentNormal),
                    Vector3.Dot(currentOffset, currentBinormal));
                nextContour[side] = new Vector2(
                    Vector3.Dot(nextOffset, nextNormal),
                    Vector3.Dot(nextOffset, nextBinormal));
            }
            selfIntersects = ContourSelfIntersects(currentContour) ||
                ContourSelfIntersects(nextContour);
            float currentArea = CalculateSignedContourArea(currentContour);
            float nextArea = CalculateSignedContourArea(nextContour);
            var expected = new Vector3[radialSegments];
            for (int side = 0; side < radialSegments; side++)
            {
                Vector2 currentOutward = CalculateContourOutwardNormal(
                    currentContour, side, currentArea);
                Vector2 nextOutward = CalculateContourOutwardNormal(
                    nextContour, side, nextArea);
                expected[side] =
                    currentNormal * currentOutward.x +
                    currentBinormal * currentOutward.y +
                    nextNormal * nextOutward.x +
                    nextBinormal * nextOutward.y;
            }
            return expected;
        }

        private static List<int> FindTopologyUnsafeTrunkStrips(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int radialSegments,
            float minimumOrientation)
        {
            int ringStride = radialSegments + 1;
            var positions = new Vector3[samples.Count * ringStride];
            var surfaceNormals = new Vector3[positions.Length];
            for (int ring = 0; ring < samples.Count; ring++)
            {
                for (int side = 0; side <= radialSegments; side++)
                {
                    float normalizedSide = side / (float)radialSegments;
                    BuildCandidateSurfaceVertex(
                        definition,
                        branch,
                        samples,
                        ring,
                        normalizedSide,
                        radialSegments,
                        out Vector3 position,
                        out Vector3 normal,
                        out _,
                        out _);
                    int vertex = ring * ringStride + side;
                    positions[vertex] = position;
                    surfaceNormals[vertex] = normal;
                }
            }

            var unsafeStrips = new List<int>();
            for (int ring = 0; ring < samples.Count - 1; ring++)
            {
                int currentRing = ring * ringStride;
                int nextRing = currentRing + ringStride;
                bool boundaryTransition = IsBoundaryTransitionStrip(
                    definition, samples, ring);
                bool useContour = boundaryTransition ||
                    ShouldUseRootContourValidation(
                        definition.ResolvedParameters,
                        branch,
                        samples[ring],
                        samples[ring + 1]);
                bool generatedContourSelfIntersects = false;
                Vector3[] contourExpectedBySide = boundaryTransition
                    ? BuildGeneratedContourExpectedDirections(
                        definition, samples, ring, radialSegments, positions,
                        currentRing, nextRing,
                        out generatedContourSelfIntersects)
                    : useContour
                        ? BuildRootContourExpectedDirections(
                            definition, branch, samples, ring, radialSegments)
                        : null;
                if (generatedContourSelfIntersects)
                {
                    unsafeStrips.Add(ring);
                    continue;
                }
                if (useContour && !boundaryTransition)
                {
                    ResolveTrunkSurfaceFrame(
                        definition.ResolvedParameters, samples[ring],
                        out _, out Vector3 ringNormal,
                        out Vector3 ringBinormal);
                    Vector2[] ringContour = BuildDiagnosticRingContour(
                        definition, branch, samples, ring, radialSegments,
                        samples[ring].Position, ringNormal, ringBinormal);
                    ResolveTrunkSurfaceFrame(
                        definition.ResolvedParameters, samples[ring + 1],
                        out _, out Vector3 nextRingNormal,
                        out Vector3 nextRingBinormal);
                    Vector2[] followingContour = BuildDiagnosticRingContour(
                        definition, branch, samples, ring + 1,
                        radialSegments, samples[ring + 1].Position,
                        nextRingNormal, nextRingBinormal);
                    if (ContourSelfIntersects(ringContour) ||
                        ContourSelfIntersects(followingContour))
                    {
                        unsafeStrips.Add(ring);
                        continue;
                    }
                }
                bool stripUnsafe = false;
                for (int side = 0; side < radialSegments; side++)
                {
                    int a = currentRing + side;
                    int b = nextRing + side;
                    int c = nextRing + side + 1;
                    int d = currentRing + side + 1;
                    Vector3 contourExpected = useContour &&
                        contourExpectedBySide != null
                            ? contourExpectedBySide[side]
                            : Vector3.zero;
                    float currentMinimum = useContour
                        ? Mathf.Min(
                            EvaluateTriangleAgainstExpected(
                                positions[a], positions[d], positions[c],
                                contourExpected),
                            EvaluateTriangleAgainstExpected(
                                positions[a], positions[c], positions[b],
                                contourExpected))
                        : Mathf.Min(
                            EvaluateTriangleOrientation(
                                positions[a], positions[d], positions[c],
                                surfaceNormals[a], surfaceNormals[d],
                                surfaceNormals[c]),
                            EvaluateTriangleOrientation(
                                positions[a], positions[c], positions[b],
                                surfaceNormals[a], surfaceNormals[c],
                                surfaceNormals[b]));
                    float alternateMinimum = useContour
                        ? Mathf.Min(
                            EvaluateTriangleAgainstExpected(
                                positions[a], positions[d], positions[b],
                                contourExpected),
                            EvaluateTriangleAgainstExpected(
                                positions[d], positions[c], positions[b],
                                contourExpected))
                        : Mathf.Min(
                            EvaluateTriangleOrientation(
                                positions[a], positions[d], positions[b],
                                surfaceNormals[a], surfaceNormals[d],
                                surfaceNormals[b]),
                            EvaluateTriangleOrientation(
                                positions[d], positions[c], positions[b],
                                surfaceNormals[d], surfaceNormals[c],
                                surfaceNormals[b]));
                    if (Mathf.Max(currentMinimum, alternateMinimum) <
                        minimumOrientation)
                    {
                        stripUnsafe = true;
                        break;
                    }
                }

                if (stripUnsafe)
                {
                    unsafeStrips.Add(ring);
                }
            }

            return unsafeStrips;
        }

        private static string BuildRootRingCorrespondenceDiagnostic(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int radialSegments,
            int stripIndex,
            float minimumOrientation)
        {
            if (definition == null || branch == null || samples == null ||
                stripIndex < 0 || stripIndex + 1 >= samples.Count)
            {
                return "[Root Ring Correspondence Diagnostic]\n" +
                    "Diagnostic unavailable: invalid strip inputs.";
            }

            TreeResolvedParameters parameters = definition.ResolvedParameters;
            RenderSample current = samples[stripIndex];
            RenderSample next = samples[stripIndex + 1];
            radialSegments = Mathf.Max(3, radialSegments);

            float worstScore = float.PositiveInfinity;
            int worstSide = -1;
            float worstCurrentDiagonal = 0f;
            float worstAlternateDiagonal = 0f;
            Vector3 worstA = Vector3.zero;
            Vector3 worstB = Vector3.zero;
            Vector3 worstC = Vector3.zero;
            Vector3 worstD = Vector3.zero;
            float worstCurrentMultiplier = 0f;
            float worstNextMultiplier = 0f;
            float currentRingSignedArea = 0f;
            float nextRingSignedArea = 0f;
            bool currentRingSelfIntersects = false;
            bool nextRingSelfIntersects = false;
            float contourCurrentDiagonal = 0f;
            float contourAlternateDiagonal = 0f;
            float localSweepCurrentDiagonal = 0f;
            float localSweepAlternateDiagonal = 0f;
            Vector3 contourExpected = Vector3.zero;
            Vector3 localSweepExpected = Vector3.zero;

            for (int side = 0; side < radialSegments; side++)
            {
                float side0 = side / (float)radialSegments;
                float side1 = (side + 1) / (float)radialSegments;
                BuildSurfaceVertex(
                    definition, branch, samples, stripIndex, side0,
                    radialSegments, out Vector3 a, out Vector3 an,
                    out _, out float aMultiplier);
                BuildSurfaceVertex(
                    definition, branch, samples, stripIndex + 1, side0,
                    radialSegments, out Vector3 b, out Vector3 bn,
                    out _, out float bMultiplier);
                BuildSurfaceVertex(
                    definition, branch, samples, stripIndex + 1, side1,
                    radialSegments, out Vector3 c, out Vector3 cn,
                    out _, out _);
                BuildSurfaceVertex(
                    definition, branch, samples, stripIndex, side1,
                    radialSegments, out Vector3 d, out Vector3 dn,
                    out _, out _);

                float currentMinimum = Mathf.Min(
                    EvaluateTriangleOrientation(a, d, c, an, dn, cn),
                    EvaluateTriangleOrientation(a, c, b, an, cn, bn));
                float alternateMinimum = Mathf.Min(
                    EvaluateTriangleOrientation(a, d, b, an, dn, bn),
                    EvaluateTriangleOrientation(d, c, b, dn, cn, bn));
                float bestForQuad = Mathf.Max(
                    currentMinimum,
                    alternateMinimum);
                if (bestForQuad < worstScore)
                {
                    worstScore = bestForQuad;
                    worstSide = side;
                    worstCurrentDiagonal = currentMinimum;
                    worstAlternateDiagonal = alternateMinimum;
                    worstA = a;
                    worstB = b;
                    worstC = c;
                    worstD = d;
                    worstCurrentMultiplier = aMultiplier;
                    worstNextMultiplier = bMultiplier;
                }
            }

            ResolveTrunkSurfaceFrame(
                parameters, current, out Vector3 currentAxis,
                out Vector3 currentSurfaceNormal,
                out Vector3 currentSurfaceBinormal);
            ResolveTrunkSurfaceFrame(
                parameters, next, out Vector3 nextAxis,
                out Vector3 nextSurfaceNormal,
                out Vector3 nextSurfaceBinormal);

            if (worstSide >= 0)
            {
                Vector2[] currentContour = BuildDiagnosticRingContour(
                    definition, branch, samples, stripIndex, radialSegments,
                    current.Position, currentSurfaceNormal,
                    currentSurfaceBinormal);
                Vector2[] nextContour = BuildDiagnosticRingContour(
                    definition, branch, samples, stripIndex + 1,
                    radialSegments, next.Position, nextSurfaceNormal,
                    nextSurfaceBinormal);
                currentRingSignedArea = CalculateSignedContourArea(
                    currentContour);
                nextRingSignedArea = CalculateSignedContourArea(nextContour);
                currentRingSelfIntersects = ContourSelfIntersects(
                    currentContour);
                nextRingSelfIntersects = ContourSelfIntersects(nextContour);

                Vector2 currentOutward2 = CalculateContourOutwardNormal(
                    currentContour, worstSide, currentRingSignedArea);
                Vector2 nextOutward2 = CalculateContourOutwardNormal(
                    nextContour, worstSide, nextRingSignedArea);
                Vector3 currentOutward3 =
                    currentSurfaceNormal * currentOutward2.x +
                    currentSurfaceBinormal * currentOutward2.y;
                Vector3 nextOutward3 =
                    nextSurfaceNormal * nextOutward2.x +
                    nextSurfaceBinormal * nextOutward2.y;
                contourExpected = currentOutward3 + nextOutward3;
                EvaluateDiagnosticQuadAgainstExpected(
                    worstA, worstB, worstC, worstD, contourExpected,
                    out contourCurrentDiagonal,
                    out contourAlternateDiagonal);

                Vector3 circumference =
                    ((worstD - worstA) + (worstC - worstB)) * 0.5f;
                Vector3 longitudinal =
                    ((worstB - worstA) + (worstC - worstD)) * 0.5f;
                localSweepExpected = Vector3.Cross(
                    circumference, longitudinal);
                EvaluateDiagnosticQuadAgainstExpected(
                    worstA, worstB, worstC, worstD, localSweepExpected,
                    out localSweepCurrentDiagonal,
                    out localSweepAlternateDiagonal);
            }

            EvaluateRootEnvelopes(
                parameters, current.NormalizedDistance,
                out float currentBody, out float currentFoot);
            EvaluateRootEnvelopes(
                parameters, next.NormalizedDistance,
                out float nextBody, out float nextFoot);
            float currentFrameEnvelope = EvaluateRootFrameEnvelope(
                parameters, current.NormalizedDistance);
            float nextFrameEnvelope = EvaluateRootFrameEnvelope(
                parameters, next.NormalizedDistance);
            float currentRollDegrees =
                ResolveAuthoredTrunkSurfaceRollDegrees(
                    parameters,
                    current.NormalizedDistance);
            float nextRollDegrees =
                ResolveAuthoredTrunkSurfaceRollDegrees(
                    parameters,
                    next.NormalizedDistance);
            float requestedRollDegrees =
                parameters.TrunkSurfaceTorsionDegrees;
            float currentRollProgress =
                Mathf.Abs(requestedRollDegrees) > Epsilon
                    ? currentRollDegrees / requestedRollDegrees
                    : 0f;
            float nextRollProgress =
                Mathf.Abs(requestedRollDegrees) > Epsilon
                    ? nextRollDegrees / requestedRollDegrees
                    : 0f;
            float worstAngleDegrees = worstSide >= 0
                ? worstSide * 360f / radialSegments
                : 0f;

            var report = new StringBuilder(2048);
            report.AppendLine("[Root Ring Correspondence Diagnostic]");
            report.Append("strip / worstSide / radialSegments / threshold: ")
                .Append(stripIndex).Append(" / ")
                .Append(worstSide).Append(" / ")
                .Append(radialSegments).Append(" / ")
                .AppendLine(minimumOrientation.ToString("F4"));
            report.Append("normalized current -> next / delta: ")
                .Append(current.NormalizedDistance.ToString("F6"))
                .Append(" -> ")
                .Append(next.NormalizedDistance.ToString("F6"))
                .Append(" / ")
                .AppendLine((next.NormalizedDistance -
                    current.NormalizedDistance).ToString("F6"));
            report.Append("centres current -> next / displacement: ")
                .Append(FormatDiagnosticVector(current.Position))
                .Append(" -> ")
                .Append(FormatDiagnosticVector(next.Position))
                .Append(" / ")
                .AppendLine(Vector3.Distance(
                    current.Position, next.Position).ToString("F6"));
            report.Append("sample radii current -> next: ")
                .Append(current.Radius.ToString("F6"))
                .Append(" -> ")
                .AppendLine(next.Radius.ToString("F6"));
            report.Append("transported tangent angle / normal angle / binormal angle: ")
                .Append(Vector3.Angle(current.Tangent, next.Tangent)
                    .ToString("F4")).Append(" / ")
                .Append(Vector3.Angle(current.Normal, next.Normal)
                    .ToString("F4")).Append(" / ")
                .AppendLine(Vector3.Angle(current.Binormal, next.Binormal)
                    .ToString("F4"));
            report.Append("resolved surface-axis angle / normal angle / binormal angle: ")
                .Append(Vector3.Angle(currentAxis, nextAxis)
                    .ToString("F4")).Append(" / ")
                .Append(Vector3.Angle(currentSurfaceNormal, nextSurfaceNormal)
                    .ToString("F4")).Append(" / ")
                .AppendLine(Vector3.Angle(
                    currentSurfaceBinormal, nextSurfaceBinormal)
                    .ToString("F4"));
            report.Append("root body envelope current -> next: ")
                .Append(currentBody.ToString("F6")).Append(" -> ")
                .AppendLine(nextBody.ToString("F6"));
            report.Append("root foot envelope current -> next: ")
                .Append(currentFoot.ToString("F6")).Append(" -> ")
                .AppendLine(nextFoot.ToString("F6"));
            report.Append("root-frame envelope current -> next: ")
                .Append(currentFrameEnvelope.ToString("F6"))
                .Append(" -> ")
                .AppendLine(nextFrameEnvelope.ToString("F6"));
            report.Append("bark-roll progress current -> next / degrees: ")
                .Append(currentRollProgress.ToString("F6"))
                .Append(" -> ")
                .Append(nextRollProgress.ToString("F6"))
                .Append(" / ")
                .Append(currentRollDegrees.ToString("F3"))
                .Append(" -> ")
                .AppendLine(nextRollDegrees.ToString("F3"));
            report.Append("root phase / worst angle degrees / root count: ")
                .Append((ResolveRootPhase(branch.Phase) * Mathf.Rad2Deg)
                    .ToString("F3")).Append(" / ")
                .Append(worstAngleDegrees.ToString("F3")).Append(" / ")
                .AppendLine(parameters.RootButtressCount.ToString());
            report.Append("worst cross-section multiplier current -> next: ")
                .Append(worstCurrentMultiplier.ToString("F6"))
                .Append(" -> ")
                .AppendLine(worstNextMultiplier.ToString("F6"));
            report.Append("orientation currentDiagonal / alternateDiagonal / best: ")
                .Append(worstCurrentDiagonal.ToString("F6"))
                .Append(" / ")
                .Append(worstAlternateDiagonal.ToString("F6"))
                .Append(" / ")
                .AppendLine(worstScore.ToString("F6"));
            report.Append("shadow contour signed area current -> next / self-intersection: ")
                .Append(currentRingSignedArea.ToString("F6"))
                .Append(" -> ")
                .Append(nextRingSignedArea.ToString("F6"))
                .Append(" / ")
                .Append(currentRingSelfIntersects ? "YES" : "NO")
                .Append(" -> ")
                .AppendLine(nextRingSelfIntersects ? "YES" : "NO");
            report.Append("shadow contour-outward currentDiagonal / alternateDiagonal / best: ")
                .Append(contourCurrentDiagonal.ToString("F6"))
                .Append(" / ")
                .Append(contourAlternateDiagonal.ToString("F6"))
                .Append(" / ")
                .AppendLine(Mathf.Max(
                    contourCurrentDiagonal,
                    contourAlternateDiagonal).ToString("F6"));
            report.Append("shadow contour expected: ")
                .AppendLine(FormatDiagnosticVector(contourExpected));
            report.Append("shadow local-sweep currentDiagonal / alternateDiagonal / best: ")
                .Append(FormatDiagnosticScalar(localSweepCurrentDiagonal))
                .Append(" / ")
                .Append(FormatDiagnosticScalar(localSweepAlternateDiagonal))
                .Append(" / ")
                .AppendLine(FormatDiagnosticScalar(Mathf.Max(
                    localSweepCurrentDiagonal,
                    localSweepAlternateDiagonal)));
            report.Append("shadow local-sweep expected: ")
                .AppendLine(FormatDiagnosticVector(localSweepExpected));
            report.Append("quad A(current side) / B(next side) / C(next side+1) / D(current side+1): ")
                .Append(FormatDiagnosticVector(worstA)).Append(" / ")
                .Append(FormatDiagnosticVector(worstB)).Append(" / ")
                .Append(FormatDiagnosticVector(worstC)).Append(" / ")
                .AppendLine(FormatDiagnosticVector(worstD));
            return report.ToString().TrimEnd();
        }

        private static Vector2[] BuildDiagnosticRingContour(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            int radialSegments,
            Vector3 center,
            Vector3 surfaceNormal,
            Vector3 surfaceBinormal)
        {
            var contour = new Vector2[radialSegments];
            for (int side = 0; side < radialSegments; side++)
            {
                BuildSurfaceVertex(
                    definition, branch, samples, sampleIndex,
                    side / (float)radialSegments, radialSegments,
                    out Vector3 position, out _, out _, out _);
                Vector3 offset = position - center;
                contour[side] = new Vector2(
                    Vector3.Dot(offset, surfaceNormal),
                    Vector3.Dot(offset, surfaceBinormal));
            }

            return contour;
        }

        private static float CalculateSignedContourArea(
            IReadOnlyList<Vector2> contour)
        {
            if (contour == null || contour.Count < 3)
            {
                return 0f;
            }

            double twiceArea = 0.0;
            for (int index = 0; index < contour.Count; index++)
            {
                Vector2 a = contour[index];
                Vector2 b = contour[(index + 1) % contour.Count];
                twiceArea += (double)a.x * b.y - (double)b.x * a.y;
            }

            return (float)(twiceArea * 0.5);
        }

        private static Vector2 CalculateContourOutwardNormal(
            IReadOnlyList<Vector2> contour,
            int side,
            float signedArea)
        {
            if (contour == null || contour.Count < 2 || side < 0)
            {
                return Vector2.zero;
            }

            Vector2 a = contour[side % contour.Count];
            Vector2 b = contour[(side + 1) % contour.Count];
            Vector2 edge = b - a;
            if (edge.sqrMagnitude <= Epsilon)
            {
                return Vector2.zero;
            }

            Vector2 outward = signedArea >= 0f
                ? new Vector2(edge.y, -edge.x)
                : new Vector2(-edge.y, edge.x);
            return outward.normalized;
        }

        private static void EvaluateDiagnosticQuadAgainstExpected(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 d,
            Vector3 expected,
            out float currentMinimum,
            out float alternateMinimum)
        {
            currentMinimum = Mathf.Min(
                EvaluateDiagnosticTriangleAgainstExpected(a, d, c, expected),
                EvaluateDiagnosticTriangleAgainstExpected(a, c, b, expected));
            alternateMinimum = Mathf.Min(
                EvaluateDiagnosticTriangleAgainstExpected(a, d, b, expected),
                EvaluateDiagnosticTriangleAgainstExpected(d, c, b, expected));
        }

        private static float EvaluateDiagnosticTriangleAgainstExpected(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 expected)
        {
            Vector3 faceNormal = Vector3.Cross(b - a, c - a);
            if (faceNormal.sqrMagnitude <= TriangleAreaSquaredEpsilon ||
                expected.sqrMagnitude <= Epsilon)
            {
                return float.NegativeInfinity;
            }

            return Vector3.Dot(
                faceNormal.normalized,
                expected.normalized);
        }

        private static bool ContourSelfIntersects(
            IReadOnlyList<Vector2> contour)
        {
            if (contour == null || contour.Count < 4)
            {
                return false;
            }

            for (int first = 0; first < contour.Count; first++)
            {
                int firstNext = (first + 1) % contour.Count;
                for (int second = first + 1; second < contour.Count; second++)
                {
                    int secondNext = (second + 1) % contour.Count;
                    if (first == second || firstNext == second ||
                        secondNext == first)
                    {
                        continue;
                    }

                    if (SegmentsIntersect(
                            contour[first], contour[firstNext],
                            contour[second], contour[secondNext]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool SegmentsIntersect(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d)
        {
            float o1 = Cross2D(b - a, c - a);
            float o2 = Cross2D(b - a, d - a);
            float o3 = Cross2D(d - c, a - c);
            float o4 = Cross2D(d - c, b - c);
            const float tolerance = 0.0000001f;
            return ((o1 > tolerance && o2 < -tolerance) ||
                    (o1 < -tolerance && o2 > tolerance)) &&
                   ((o3 > tolerance && o4 < -tolerance) ||
                    (o3 < -tolerance && o4 > tolerance));
        }

        private static float Cross2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private static string FormatDiagnosticScalar(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? "undefined"
                : value.ToString("F6");
        }

        private static string FormatDiagnosticVector(Vector3 value)
        {
            return "(" + value.x.ToString("F6") + "," +
                value.y.ToString("F6") + "," +
                value.z.ToString("F6") + ")";
        }

        private static RenderSample InterpolateRenderSample(
            RenderSample a,
            RenderSample b,
            float t)
        {
            Vector3 tangent = SafeNormalize(
                Vector3.Slerp(a.Tangent, b.Tangent, t),
                Vector3.Lerp(a.Tangent, b.Tangent, t));
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent);
            normal = SafeNormalize(normal, ChooseInitialNormal(tangent));
            Vector3 binormal = SafeNormalize(
                Vector3.Cross(tangent, normal),
                Vector3.Cross(tangent, ChooseInitialNormal(tangent)));
            normal = SafeNormalize(
                Vector3.Cross(binormal, tangent),
                normal);
            return new RenderSample
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Normal = normal,
                Binormal = binormal,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t),
                NormalizedDistance = Mathf.Lerp(
                    a.NormalizedDistance,
                    b.NormalizedDistance,
                    t),
                CumulativeDistance = Mathf.Lerp(
                    a.CumulativeDistance,
                    b.CumulativeDistance,
                    t)
            };
        }

        private static void BuildSurfaceVertex(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            float normalizedSide,
            int radialSegments,
            out Vector3 position,
            out Vector3 normal,
            out Vector3 circumferenceTangent,
            out float crossSectionMultiplier)
        {
            RenderSample sample = samples[sampleIndex];
            float angle = normalizedSide * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            Vector3 radialReference = SafeNormalize(
                sample.Normal * cosine +
                sample.Binormal * sine,
                sample.Normal);

            if (branch.BranchOrder != 0)
            {
                position = sample.Position +
                    radialReference * sample.Radius;
                normal = radialReference;
                circumferenceTangent = SafeNormalize(
                    sample.Normal * -sine +
                    sample.Binormal * cosine,
                    sample.Binormal);
                crossSectionMultiplier = 1f;
                return;
            }

            position = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide,
                out crossSectionMultiplier);
            radialReference = SafeNormalize(
                position - sample.Position,
                radialReference);

            float sideDelta = 1f /
                Mathf.Max(48f, radialSegments * 8f);
            Vector3 previousSide = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide - sideDelta,
                out _);
            Vector3 nextSide = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide + sideDelta,
                out _);
            circumferenceTangent = SafeNormalize(
                nextSide - previousSide,
                sample.Binormal);

            Vector3 previousLongitudinal;
            Vector3 nextLongitudinal;
            if (sampleIndex == 0)
            {
                previousLongitudinal = position;
                nextLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[1],
                    normalizedSide,
                    out _);
            }
            else if (sampleIndex == samples.Count - 1)
            {
                previousLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex - 1],
                    normalizedSide,
                    out _);
                nextLongitudinal = position;
            }
            else
            {
                previousLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex - 1],
                    normalizedSide,
                    out _);
                nextLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex + 1],
                    normalizedSide,
                    out _);
            }

            Vector3 longitudinalTangent = SafeNormalize(
                nextLongitudinal - previousLongitudinal,
                sample.Tangent);
            normal = SafeNormalize(
                Vector3.Cross(
                    circumferenceTangent,
                    longitudinalTangent),
                radialReference);
            if (Vector3.Dot(normal, radialReference) < 0f)
            {
                normal = -normal;
            }
        }

        private static Vector3 EvaluateTrunkSurfacePosition(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            RenderSample sample,
            float normalizedSide,
            out float crossSectionMultiplier)
        {
            TreeResolvedParameters parameters =
                definition.ResolvedParameters;
            float angle = normalizedSide * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            ResolveTrunkSurfaceFrame(
                parameters,
                sample,
                out _,
                out Vector3 surfaceNormalAxis,
                out Vector3 surfaceBinormalAxis);
            Vector3 radial = SafeNormalize(
                surfaceNormalAxis * cosine +
                surfaceBinormalAxis * sine,
                surfaceNormalAxis);

            EvaluateTrunkRootContributions(
                parameters,
                branch.Phase,
                sample.NormalizedDistance,
                angle,
                out float bodyContribution,
                out float footContribution,
                out float footAnchorEnvelope);
            if (!parameters.RecipeOnlyControlSource)
            {
                crossSectionMultiplier = Mathf.Max(
                    0.35f,
                    1f + bodyContribution + footContribution);
                return sample.Position +
                    radial *
                    sample.Radius *
                    crossSectionMultiplier;
            }

            Vector3 bodyOffset = radial *
                sample.Radius *
                (1f + bodyContribution);
            Vector3 footRadial = ResolveGroundAnchoredRootFootRadial(
                parameters,
                sample.NormalizedDistance,
                angle,
                footAnchorEnvelope);
            Vector3 offset = bodyOffset +
                footRadial * sample.Radius * footContribution;
            crossSectionMultiplier = offset.magnitude /
                Mathf.Max(Epsilon, sample.Radius);
            return sample.Position + offset;
        }

        private static void ResolveTrunkSurfaceFrame(
            TreeResolvedParameters parameters,
            RenderSample sample,
            out Vector3 tangent,
            out Vector3 normal,
            out Vector3 binormal)
        {
            ResolveTrunkBaseSurfaceFrame(
                parameters,
                sample,
                out tangent,
                out normal,
                out binormal);

            if (parameters.RecipeOnlyControlSource &&
                Mathf.Abs(parameters.TrunkSurfaceTorsionDegrees) > Epsilon)
            {
                float rollDegrees =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        sample.NormalizedDistance);
                normal = Quaternion.AngleAxis(
                    rollDegrees,
                    tangent) * normal;
                normal = SafeNormalize(
                    Vector3.ProjectOnPlane(normal, tangent),
                    ChooseInitialNormal(tangent));
                binormal = SafeNormalize(
                    Vector3.Cross(tangent, normal),
                    binormal);
                normal = SafeNormalize(
                    Vector3.Cross(binormal, tangent),
                    normal);
            }
        }

        private static void ResolveTrunkBaseSurfaceFrame(
            TreeResolvedParameters parameters,
            RenderSample sample,
            out Vector3 tangent,
            out Vector3 normal,
            out Vector3 binormal)
        {
            float rootEnvelope = EvaluateRootFrameEnvelope(
                parameters,
                sample.NormalizedDistance);
            tangent = SafeNormalize(
                Vector3.Slerp(
                    sample.Tangent,
                    Vector3.up,
                    rootEnvelope),
                sample.Tangent);

            Vector3 transportedNormal = Vector3.ProjectOnPlane(
                sample.Normal,
                tangent);
            if (transportedNormal.sqrMagnitude <= Epsilon)
            {
                Vector3 projectedBinormal = Vector3.ProjectOnPlane(
                    sample.Binormal,
                    tangent);
                if (projectedBinormal.sqrMagnitude > Epsilon)
                {
                    transportedNormal = Vector3.Cross(
                        projectedBinormal.normalized,
                        tangent);
                }
            }
            transportedNormal = SafeNormalize(
                transportedNormal,
                ChooseInitialNormal(tangent));

            if (parameters.RecipeOnlyControlSource)
            {
                float adoption = 1f - rootEnvelope;
                Vector3 fixedRootNormal = SafeNormalize(
                    Vector3.ProjectOnPlane(Vector3.right, tangent),
                    ChooseInitialNormal(tangent));
                normal = SafeNormalize(
                    Vector3.Slerp(
                        fixedRootNormal,
                        transportedNormal,
                        adoption),
                    fixedRootNormal);
            }
            else
            {
                normal = transportedNormal;
            }

            binormal = SafeNormalize(
                Vector3.Cross(tangent, normal),
                sample.Binormal);
            normal = SafeNormalize(
                Vector3.Cross(binormal, tangent),
                normal);
        }

        private static float ResolveAuthoredTrunkSurfaceRollDegrees(
            TreeResolvedParameters parameters,
            float normalizedDistance)
        {
            return parameters.TrunkSurfaceTorsionDegrees *
                Mathf.Clamp01(normalizedDistance);
        }

        private static float CalculateRootGroundPlateauEnd(
            TreeResolvedParameters parameters)
        {
            float authored = Mathf.Max(
                0.01f,
                parameters.RootButtressHeight);
            return authored * 0.10f;
        }

        private static float CalculateEffectiveRootCollapseHeight(
            TreeResolvedParameters parameters)
        {
            float authored = Mathf.Clamp(
                parameters.RootButtressHeight,
                0.01f,
                0.6f);
            if (!parameters.RecipeOnlyControlSource)
            {
                return authored;
            }

            float treeHeight = Mathf.Max(0.01f, parameters.Height);
            float baseRadius = Mathf.Max(
                0.01f,
                parameters.TrunkBaseRadius);
            TreeRootCollapseTournamentProfile profile =
                GetActiveRootCollapseProfile();
            float minimumPhysicalCollapse = Mathf.Max(
                profile.MinimumPhysicalMetres,
                baseRadius * profile.RadiusFactor);
            float minimumNormalizedCollapse =
                minimumPhysicalCollapse / treeHeight;
            return Mathf.Clamp(
                Mathf.Max(authored * 0.72f, minimumNormalizedCollapse),
                authored * 0.72f,
                0.70f);
        }

        private static float CalculateEarliestRootTransitionHeight(
            TreeResolvedParameters parameters)
        {
            float authored = Mathf.Clamp(
                parameters.RootButtressHeight,
                0.01f,
                0.6f);
            if (!parameters.RecipeOnlyControlSource)
            {
                return authored;
            }

            float treeHeight = Mathf.Max(0.01f, parameters.Height);
            float minimumPhysicalTail = Mathf.Max(
                0.08f,
                Mathf.Max(0.01f, parameters.TrunkBaseRadius) * 0.18f);
            float minimumTailNormalized = minimumPhysicalTail / treeHeight;
            float collapseEnd =
                CalculateEffectiveRootCollapseHeight(parameters);
            return Mathf.Clamp(
                Mathf.Max(authored, collapseEnd + minimumTailNormalized),
                authored,
                0.75f);
        }

        private static float CalculateEffectiveRootTransitionHeight(
            TreeResolvedParameters parameters)
        {
            float earliest =
                CalculateEarliestRootTransitionHeight(parameters);
            if (!parameters.RecipeOnlyControlSource)
            {
                return earliest;
            }

            float persistence = Mathf.Clamp01(
                parameters.ButtressTransition);
            return Mathf.Lerp(earliest, 1f, persistence);
        }

        private static float CalculateEffectiveButtressBodyEnd(
            TreeResolvedParameters parameters)
        {
            float earliest =
                CalculateEffectiveRootCollapseHeight(parameters);
            if (!parameters.RecipeOnlyControlSource)
            {
                return earliest;
            }

            float persistence = Mathf.Clamp01(
                parameters.ButtressTransition);
            return Mathf.Lerp(earliest, 1f, persistence);
        }

        private static float CalculateRootBoundaryEnd(
            TreeResolvedParameters parameters)
        {
            float collapseEnd = CalculateEffectiveRootCollapseHeight(parameters);
            float effectiveTransition =
                CalculateEffectiveRootTransitionHeight(parameters);
            TreeRootCollapseTournamentProfile profile =
                GetActiveRootCollapseProfile();
            if (!profile.ExactZeroBeforeAdoption)
            {
                return effectiveTransition;
            }
            float tail = Mathf.Max(
                0.0001f,
                effectiveTransition - collapseEnd);
            return Mathf.Min(0.90f, effectiveTransition + tail);
        }

        private static float SmootherStep01(float value)
        {
            float t = Mathf.Clamp01(value);
            return t * t * t * (t * (t * 6f - 15f) + 10f);
        }

        private static float EvaluateRootFrameEnvelope(
            TreeResolvedParameters parameters,
            float normalizedDistance)
        {
            float rootHeight = Mathf.Max(
                0.01f,
                parameters.RootButtressHeight);
            if (!parameters.RecipeOnlyControlSource)
            {
                float rootLinear = Mathf.Clamp01(
                    1f - normalizedDistance / rootHeight);
                return rootLinear * rootLinear *
                    (3f - 2f * rootLinear);
            }

            float collapseEnd =
                CalculateEffectiveRootCollapseHeight(parameters);
            if (normalizedDistance <= collapseEnd)
            {
                return 1f;
            }

            float effectiveTransitionHeight =
                CalculateEffectiveRootTransitionHeight(parameters);
            TreeRootCollapseTournamentProfile profile =
                GetActiveRootCollapseProfile();
            float adoptionStart = collapseEnd;
            float adoptionEnd = effectiveTransitionHeight;
            if (profile.ExactZeroBeforeAdoption)
            {
                float tail = Mathf.Max(
                    0.0001f,
                    effectiveTransitionHeight - collapseEnd);
                adoptionStart = effectiveTransitionHeight;
                adoptionEnd = Mathf.Min(0.90f, effectiveTransitionHeight + tail);
            }
            if (activeTournamentStrategy ==
                TreeRootCollapseTournamentStrategy.ImmediateFrameRelease)
            {
                return normalizedDistance <= collapseEnd ? 1f : 0f;
            }
            if (activeTournamentStrategy ==
                TreeRootCollapseTournamentStrategy.BoundedFrameRelease)
            {
                float delayedStart = effectiveTransitionHeight;
                float delayedEnd = Mathf.Min(
                    0.90f,
                    effectiveTransitionHeight + Mathf.Max(
                        0.01f,
                        effectiveTransitionHeight - collapseEnd));
                float delayed = Mathf.InverseLerp(
                    delayedStart,
                    delayedEnd,
                    normalizedDistance);
                return 1f - SmootherStep01(delayed);
            }

            float adoption = Mathf.InverseLerp(
                adoptionStart,
                adoptionEnd,
                normalizedDistance);
            adoption = SmootherStep01(adoption);
            return 1f - adoption;
        }

        private static float EvaluateTrunkCrossSectionMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance,
            float angle)
        {
            EvaluateTrunkRootContributions(
                parameters,
                branchPhase,
                normalizedDistance,
                angle,
                out float bodyContribution,
                out float footContribution,
                out _);
            return Mathf.Max(
                0.35f,
                1f + bodyContribution + footContribution);
        }

        private static void EvaluateTrunkRootContributions(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance,
            float angle,
            out float bodyContribution,
            out float footContribution,
            out float footAnchorEnvelope)
        {
            int buttressCount = Mathf.Clamp(
                parameters.RootButtressCount,
                3,
                8);
            EvaluateRootEnvelopes(
                parameters,
                normalizedDistance,
                out float bodyEnvelope,
                out float footShapeEnvelope);
            footAnchorEnvelope = parameters.RecipeOnlyControlSource
                ? EvaluateRootFootAnchorEnvelope(
                    parameters,
                    normalizedDistance)
                : footShapeEnvelope;
            float shoulderWidth =
                EvaluateButtressAngularWidthScale(bodyEnvelope);
            EvaluateButtressMasks(
                angle,
                ResolveRootPhase(branchPhase),
                buttressCount,
                shoulderWidth,
                parameters.RootThickness,
                parameters.RecipeOnlyControlSource,
                out float bodyMask,
                out float footMask);

            if (parameters.RecipeOnlyControlSource)
            {
                float reach = Mathf.Max(0f, parameters.RootReach);
                bodyContribution =
                    reach * 0.28f * bodyEnvelope * bodyMask;
                float mergeFactor = EvaluateGroundRootBaseMergeFactor(
                    parameters);
                float mergeMask = Mathf.Clamp01(1f - bodyMask);
                float authoredFootContribution =
                    reach * 0.72f * footShapeEnvelope * footMask;
                float authoredRootContribution =
                    bodyContribution + authoredFootContribution;
                float mergeTarget = reach * footShapeEnvelope;
                float mergeDeficit = Mathf.Max(
                    0f,
                    mergeTarget - authoredRootContribution);
                float mergedBaseContribution =
                    mergeDeficit * mergeFactor * mergeMask;
                footContribution =
                    authoredFootContribution + mergedBaseContribution;
                return;
            }

            bodyContribution =
                0.25f *
                Mathf.Max(0f, parameters.RootButtressStrength) *
                bodyEnvelope *
                bodyMask;
            float footAmplitude =
                0.40f *
                Mathf.Max(0f, parameters.RootButtressStrength) +
                0.75f *
                (Mathf.Max(1f, parameters.RootFlareScale) - 1f);
            footContribution =
                footAmplitude * footShapeEnvelope * footMask;
        }

        private static Vector3 ResolveGroundAnchoredRootFootRadial(
            TreeResolvedParameters parameters,
            float normalizedDistance,
            float angle,
            float footAnchorEnvelope)
        {
            Vector3 anchoredNormal = Vector3.right;
            Vector3 anchoredBinormal = Vector3.Cross(
                Vector3.up,
                anchoredNormal).normalized;
            Vector3 anchoredRadial = SafeNormalize(
                anchoredNormal * Mathf.Cos(angle) +
                anchoredBinormal * Mathf.Sin(angle),
                anchoredNormal);
            float anchorWeight = Mathf.Clamp01(footAnchorEnvelope);
            float releasedRollDegrees =
                ResolveAuthoredTrunkSurfaceRollDegrees(
                    parameters,
                    normalizedDistance) *
                (1f - anchorWeight);
            return SafeNormalize(
                Quaternion.AngleAxis(
                    releasedRollDegrees,
                    Vector3.up) * anchoredRadial,
                anchoredRadial);
        }

        private static float ResolveRootPhase(float branchPhase)
        {
            float twistPhase = branchPhase * TwoPi;
            return twistPhase +
                branchPhase * TwoPi * 0.381966f;
        }

        private static void EvaluateButtressMasks(
            float angle,
            float phase,
            int count,
            float verticalWidthScale,
            float rootThickness,
            bool useRecipeThicknessProfile,
            out float bodyMask,
            out float footMask)
        {
            int safeCount = Mathf.Max(3, count);
            float sector = TwoPi / safeCount;
            if (!useRecipeThicknessProfile)
            {
                float delta = Mathf.Repeat(
                    angle - phase + sector * 0.5f,
                    sector) - sector * 0.5f;
                float legacyQ = Mathf.Clamp01(
                    Mathf.Abs(delta) /
                    (sector * 0.5f * Mathf.Clamp(
                        verticalWidthScale,
                        0.10f,
                        1f)));
                float legacyQ2 = legacyQ * legacyQ;
                float legacyQ4 = legacyQ2 * legacyQ2;
                float legacyBasis = Mathf.Max(0f, 1f - legacyQ4);
                bodyMask = legacyBasis * legacyBasis;
                footMask = bodyMask * legacyBasis;
                return;
            }

            float requestedFullWidthDegrees =
                EvaluateRequestedRootFullWidthDegrees(rootThickness);
            float sectorDegrees = 360f / safeCount;
            float emittedFullWidthDegrees = Mathf.Min(
                requestedFullWidthDegrees,
                sectorDegrees);
            float halfSupport = emittedFullWidthDegrees *
                Mathf.Deg2Rad * 0.5f *
                Mathf.Clamp(verticalWidthScale, 0.10f, 1f);
            float deltaToNearest = Mathf.Abs(Mathf.Repeat(
                angle - phase + sector * 0.5f,
                sector) - sector * 0.5f);
            if (halfSupport <= Epsilon ||
                deltaToNearest >= halfSupport)
            {
                bodyMask = 0f;
                footMask = 0f;
                return;
            }

            float q = Mathf.Clamp01(deltaToNearest / halfSupport);
            const float profilePower = 4f;
            float basis = Mathf.Max(
                0f,
                1f - Mathf.Pow(q, profilePower));
            bodyMask = basis * basis;
            footMask = bodyMask * basis;
        }

        private static float EvaluateRequestedRootFullWidthDegrees(
            float rootThickness)
        {
            float thickness = Mathf.Clamp(rootThickness, 0.10f, 2f);
            if (thickness <= 0.50f)
            {
                return Mathf.Lerp(
                    18f,
                    60f,
                    Mathf.InverseLerp(0.10f, 0.50f, thickness));
            }

            return 60f + (thickness - 0.50f) * 104f;
        }

        private static float EvaluateGroundRootBaseMergeFactor(
            TreeResolvedParameters parameters)
        {
            if (parameters == null ||
                !parameters.RecipeOnlyControlSource)
            {
                return 0f;
            }

            int count = Mathf.Clamp(parameters.RootButtressCount, 3, 8);
            float sectorDegrees = 360f / count;
            float requestedDegrees = EvaluateRequestedRootFullWidthDegrees(
                parameters.RootThickness);
            float excessRatio = Mathf.Max(
                0f,
                requestedDegrees / Mathf.Max(Epsilon, sectorDegrees) - 1f);
            return excessRatio / (1f + excessRatio);
        }

        private static float EvaluateButtressAngularWidthScale(
            float bodyEnvelope)
        {
            return Mathf.Lerp(
                0.60f,
                1f,
                Mathf.Clamp01(bodyEnvelope));
        }

        private static float CalculateButtressAngularWidthScale(
            TreeResolvedParameters parameters,
            float normalizedDistance)
        {
            EvaluateRootEnvelopes(
                parameters,
                normalizedDistance,
                out float bodyEnvelope,
                out _);
            float widthScale =
                EvaluateButtressAngularWidthScale(bodyEnvelope);
            if (!parameters.RecipeOnlyControlSource)
            {
                return widthScale;
            }

            int count = Mathf.Clamp(parameters.RootButtressCount, 3, 8);
            float requested = EvaluateRequestedRootFullWidthDegrees(
                parameters.RootThickness);
            float emitted = Mathf.Min(requested, 360f / count);
            return emitted / 60f * widthScale;
        }

        private static void EvaluateRootEnvelopes(
            TreeResolvedParameters parameters,
            float normalizedDistance,
            out float bodyEnvelope,
            out float footShapeEnvelope)
        {
            EvaluateProductionRootEnvelopes(
                parameters,
                normalizedDistance,
                out bodyEnvelope,
                out float productionFootEnvelope);
            if (!parameters.RecipeOnlyControlSource)
            {
                footShapeEnvelope = productionFootEnvelope;
                return;
            }

            float collapseEnd = CalculateEffectiveRootCollapseHeight(parameters);
            float u = Mathf.Clamp01(
                normalizedDistance / Mathf.Max(Epsilon, collapseEnd));
            float linear = 1f - u;
            footShapeEnvelope = linear * linear;
        }

        private static void EvaluateProductionRootEnvelopes(
            TreeResolvedParameters parameters,
            float normalizedDistance,
            out float bodyEnvelope,
            out float footEnvelope)
        {
            float rootHeight = Mathf.Max(
                0.01f,
                parameters.RootButtressHeight);
            if (!parameters.RecipeOnlyControlSource)
            {
                float u = Mathf.Clamp01(
                    normalizedDistance / Mathf.Max(0.0001f, rootHeight));
                bodyEnvelope = 1f -
                    u * u * (3f - 2f * u);
                float legacyRootLinear = 1f - u;
                footEnvelope = legacyRootLinear * legacyRootLinear;
                return;
            }

            float plateauEnd = CalculateRootGroundPlateauEnd(parameters);
            float footCollapseEnd =
                CalculateEffectiveRootCollapseHeight(parameters);
            float bodyCollapseEnd =
                CalculateEffectiveButtressBodyEnd(parameters);
            float bodyCollapse = Mathf.InverseLerp(
                plateauEnd,
                bodyCollapseEnd,
                normalizedDistance);
            float footCollapse = Mathf.InverseLerp(
                plateauEnd,
                footCollapseEnd,
                normalizedDistance);
            TreeRootCollapseTournamentProfile profile =
                GetActiveRootCollapseProfile();
            bodyCollapse = profile.UseSmoothstep
                ? bodyCollapse * bodyCollapse * (3f - 2f * bodyCollapse)
                : SmootherStep01(bodyCollapse);
            footCollapse = profile.UseSmoothstep
                ? footCollapse * footCollapse * (3f - 2f * footCollapse)
                : SmootherStep01(footCollapse);
            bodyEnvelope = 1f - bodyCollapse;
            float recipeRootLinear = 1f - footCollapse;
            footEnvelope = Mathf.Pow(
                recipeRootLinear,
                profile.FootExponent);
        }

        private static float EvaluateRootFootAnchorEnvelope(
            TreeResolvedParameters parameters,
            float normalizedDistance)
        {
            EvaluateProductionRootEnvelopes(
                parameters,
                normalizedDistance,
                out _,
                out float footEnvelope);
            return footEnvelope;
        }

        private static float CalculateRootFootShapePlateauEnd(
            TreeResolvedParameters parameters)
        {
            if (parameters == null || parameters.RecipeOnlyControlSource)
            {
                return 0f;
            }

            return CalculateRootGroundPlateauEnd(parameters);
        }

        private static float EvaluateRootOnlyContribution(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance,
            float angle)
        {
            int buttressCount = Mathf.Clamp(
                parameters.RootButtressCount,
                3,
                8);
            EvaluateRootEnvelopes(
                parameters,
                normalizedDistance,
                out float bodyEnvelope,
                out float footShapeEnvelope);
            float widthScale =
                EvaluateButtressAngularWidthScale(bodyEnvelope);

            EvaluateButtressMasks(
                angle,
                ResolveRootPhase(branchPhase),
                buttressCount,
                widthScale,
                parameters.RootThickness,
                parameters.RecipeOnlyControlSource,
                out float bodyMask,
                out float footMask);
            if (parameters.RecipeOnlyControlSource)
            {
                float reach = Mathf.Max(0f, parameters.RootReach);
                float mergeFactor = EvaluateGroundRootBaseMergeFactor(
                    parameters);
                float mergeMask = Mathf.Clamp01(1f - bodyMask);
                float authoredContribution =
                    reach * 0.28f * bodyEnvelope * bodyMask +
                    reach * 0.72f * footShapeEnvelope * footMask;
                float mergeTarget = reach * footShapeEnvelope;
                float mergeDeficit = Mathf.Max(
                    0f,
                    mergeTarget - authoredContribution);
                return authoredContribution +
                    mergeDeficit * mergeFactor * mergeMask;
            }

            float strength = Mathf.Max(
                0f,
                parameters.RootButtressStrength);
            float bodyContribution =
                0.25f * strength * bodyEnvelope * bodyMask;
            float footAmplitude =
                0.40f * strength +
                0.75f *
                (Mathf.Max(1f, parameters.RootFlareScale) - 1f);
            return bodyContribution +
                footAmplitude * footShapeEnvelope * footMask;
        }

        private static float CalculateButtressCrestMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            return EvaluateTrunkCrossSectionMultiplier(
                parameters,
                branchPhase,
                normalizedDistance,
                ResolveRootPhase(branchPhase));
        }

        private static float CalculateButtressCrestRootOnlyContribution(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            return EvaluateRootOnlyContribution(
                parameters,
                branchPhase,
                normalizedDistance,
                ResolveRootPhase(branchPhase));
        }

        private static void CalculateGroundRootHalfExtensionWidth(
            TreeResolvedParameters parameters,
            float branchPhase,
            float baseRadius,
            out float fullAngularWidthDegrees,
            out float chordWidth)
        {
            fullAngularWidthDegrees = 0f;
            chordWidth = 0f;
            if (!parameters.RecipeOnlyControlSource)
            {
                return;
            }

            int buttressCount = Mathf.Clamp(
                parameters.RootButtressCount,
                3,
                8);
            float sector = TwoPi / buttressCount;
            float crestAngle = ResolveRootPhase(branchPhase);
            float crestContribution = EvaluateRootOnlyContribution(
                parameters,
                branchPhase,
                0f,
                crestAngle);
            if (crestContribution <= Epsilon)
            {
                return;
            }

            const int samples = 2048;
            float threshold = crestContribution * 0.5f;
            float previousDelta = 0f;
            float previousContribution = crestContribution;
            float resolvedDelta = sector * 0.5f;
            for (int index = 1; index <= samples; index++)
            {
                float delta = sector * 0.5f * index / samples;
                float contribution = EvaluateRootOnlyContribution(
                    parameters,
                    branchPhase,
                    0f,
                    crestAngle + delta);
                if (contribution <= threshold)
                {
                    float denominator =
                        previousContribution - contribution;
                    float interpolation = denominator > Epsilon
                        ? Mathf.Clamp01(
                            (previousContribution - threshold) /
                            denominator)
                        : 0f;
                    resolvedDelta = Mathf.Lerp(
                        previousDelta,
                        delta,
                        interpolation);
                    break;
                }

                previousDelta = delta;
                previousContribution = contribution;
            }

            fullAngularWidthDegrees =
                resolvedDelta * 2f * Mathf.Rad2Deg;
            float radiusAtHalfExtension =
                Mathf.Max(Epsilon, baseRadius) *
                (1f + threshold);
            chordWidth = 2f * radiusAtHalfExtension *
                Mathf.Sin(resolvedDelta);
        }

        private static float CalculateMaximumRootOnlyContribution(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            int sampleCount = Mathf.Max(
                64,
                Mathf.Clamp(parameters.RootButtressCount, 3, 8) * 16);
            float maximum = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                float angle = index / (float)sampleCount * TwoPi;
                maximum = Mathf.Max(
                    maximum,
                    EvaluateRootOnlyContribution(
                        parameters,
                        branchPhase,
                        normalizedDistance,
                        angle));
            }

            return maximum;
        }

        private static int CountRootZoneLongitudinalIntervals(
            IReadOnlyList<RenderSample> samples,
            float rootHeight)
        {
            if (samples == null || samples.Count < 2)
            {
                return 0;
            }

            float limit = Mathf.Max(0.01f, rootHeight);
            int count = 0;
            for (int index = 0; index < samples.Count - 1; index++)
            {
                if (samples[index].NormalizedDistance < limit - Epsilon)
                {
                    count++;
                }
            }

            return count;
        }

        private static float CalculateMaximumGroundButtressCrestTurnDegrees(
            TreeResolvedParameters parameters,
            float branchPhase,
            int radialSegments)
        {
            radialSegments = Mathf.Max(3, radialSegments);
            int buttressCount = Mathf.Clamp(
                parameters.RootButtressCount,
                3,
                8);
            float rootPhase = ResolveRootPhase(branchPhase);
            var positions = new Vector2[radialSegments];
            for (int side = 0; side < radialSegments; side++)
            {
                float angle = side / (float)radialSegments * TwoPi;
                float multiplier = EvaluateTrunkCrossSectionMultiplier(
                    parameters,
                    branchPhase,
                    0f,
                    angle);
                positions[side] = new Vector2(
                    Mathf.Cos(angle) * multiplier,
                    Mathf.Sin(angle) * multiplier);
            }

            float maximumTurn = 0f;
            float sector = TwoPi / buttressCount;
            for (int buttress = 0; buttress < buttressCount; buttress++)
            {
                float crestAngle = rootPhase + buttress * sector;
                int crestIndex = Mathf.RoundToInt(
                    Mathf.Repeat(crestAngle, TwoPi) / TwoPi *
                    radialSegments) % radialSegments;
                int previous =
                    (crestIndex - 1 + radialSegments) % radialSegments;
                int next = (crestIndex + 1) % radialSegments;
                Vector2 incoming = positions[crestIndex] - positions[previous];
                Vector2 outgoing = positions[next] - positions[crestIndex];
                if (incoming.sqrMagnitude <= Epsilon ||
                    outgoing.sqrMagnitude <= Epsilon)
                {
                    continue;
                }

                maximumTurn = Mathf.Max(
                    maximumTurn,
                    Vector2.Angle(incoming, outgoing));
            }

            return maximumTurn;
        }

        private static float CalculateMinimumTrunkCrossSectionMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            int buttressCount = Mathf.Clamp(
                parameters.RootButtressCount,
                3,
                8);
            float rootPhase = ResolveRootPhase(branchPhase);
            float sector = TwoPi / buttressCount;
            float minimum = float.PositiveInfinity;
            for (int valley = 0; valley < buttressCount; valley++)
            {
                float angle = rootPhase +
                    (valley + 0.5f) * sector;
                minimum = Mathf.Min(
                    minimum,
                    EvaluateTrunkCrossSectionMultiplier(
                        parameters,
                        branchPhase,
                        normalizedDistance,
                        angle));
            }

            return TreeDeterministicUtility.IsFinite(minimum)
                ? minimum
                : 1f;
        }

        private static float CalculateMaximumTrunkCrossSectionMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            int sampleCount = Mathf.Max(
                40,
                Mathf.Clamp(parameters.RootButtressCount, 3, 8) * 8);
            float maximum = 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                float angle = index / (float)sampleCount * TwoPi;
                maximum = Mathf.Max(
                    maximum,
                    EvaluateTrunkCrossSectionMultiplier(
                        parameters,
                        branchPhase,
                        normalizedDistance,
                        angle));
            }

            return maximum;
        }

        private static float MeasureGeneratedTrunkAxialTwist(
            TreeResolvedParameters parameters,
            IReadOnlyList<RenderSample> samples,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> ringVertexStarts)
        {
            if (samples == null || samples.Count < 2 ||
                ringVertexStarts == null ||
                ringVertexStarts.Count != samples.Count)
            {
                return 0f;
            }

            if (!parameters.RecipeOnlyControlSource)
            {
                return MeasureLegacyGeneratedTrunkAxialTwist(
                    parameters,
                    samples,
                    vertices,
                    ringVertexStarts);
            }

            float previousWrappedAngle = 0f;
            float accumulatedAngle = 0f;
            for (int ring = 0; ring < samples.Count; ring++)
            {
                RenderSample sample = samples[ring];
                ResolveTrunkBaseSurfaceFrame(
                    parameters,
                    sample,
                    out Vector3 surfaceTangent,
                    out Vector3 zeroRollNormal,
                    out _);

                int vertexIndex = ringVertexStarts[ring];
                Vector3 emittedRadial = Vector3.ProjectOnPlane(
                    vertices[vertexIndex] - sample.Position,
                    surfaceTangent);
                emittedRadial = SafeNormalize(
                    emittedRadial,
                    zeroRollNormal);
                float wrappedAngle = Vector3.SignedAngle(
                    zeroRollNormal,
                    emittedRadial,
                    surfaceTangent);
                if (ring > 0)
                {
                    accumulatedAngle += Mathf.DeltaAngle(
                        previousWrappedAngle,
                        wrappedAngle);
                }

                previousWrappedAngle = wrappedAngle;
            }

            return accumulatedAngle;
        }

        private static float MeasureLegacyGeneratedTrunkAxialTwist(
            TreeResolvedParameters parameters,
            IReadOnlyList<RenderSample> samples,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> ringVertexStarts)
        {
            Vector3 previousStructuralTangent = SafeNormalize(
                samples[0].Tangent,
                Vector3.up);
            Vector3 zeroRollNormal = Vector3.ProjectOnPlane(
                samples[0].Normal,
                previousStructuralTangent);
            zeroRollNormal = SafeNormalize(
                zeroRollNormal,
                ChooseInitialNormal(previousStructuralTangent));
            float previousWrappedAngle = 0f;
            float accumulatedAngle = 0f;

            for (int ring = 0; ring < samples.Count; ring++)
            {
                RenderSample sample = samples[ring];
                Vector3 structuralTangent = SafeNormalize(
                    sample.Tangent,
                    previousStructuralTangent);
                if (ring > 0)
                {
                    Quaternion transport = Quaternion.FromToRotation(
                        previousStructuralTangent,
                        structuralTangent);
                    zeroRollNormal = Vector3.ProjectOnPlane(
                        transport * zeroRollNormal,
                        structuralTangent);
                    zeroRollNormal = SafeNormalize(
                        zeroRollNormal,
                        ChooseInitialNormal(structuralTangent));
                }

                ResolveTrunkSurfaceFrame(
                    parameters,
                    sample,
                    out Vector3 surfaceTangent,
                    out _,
                    out _);
                Vector3 referenceNormal = Vector3.ProjectOnPlane(
                    zeroRollNormal,
                    surfaceTangent);
                referenceNormal = SafeNormalize(
                    referenceNormal,
                    ChooseInitialNormal(surfaceTangent));

                int vertexIndex = ringVertexStarts[ring];
                Vector3 emittedRadial = Vector3.ProjectOnPlane(
                    vertices[vertexIndex] - sample.Position,
                    surfaceTangent);
                emittedRadial = SafeNormalize(
                    emittedRadial,
                    referenceNormal);
                float wrappedAngle = Vector3.SignedAngle(
                    referenceNormal,
                    emittedRadial,
                    surfaceTangent);
                if (ring > 0)
                {
                    accumulatedAngle += Mathf.DeltaAngle(
                        previousWrappedAngle,
                        wrappedAngle);
                }

                previousWrappedAngle = wrappedAngle;
                previousStructuralTangent = structuralTangent;
            }

            return accumulatedAngle;
        }

        private static AxialTwistTelemetry CreateAxialTwistTelemetry(
            TreeResolvedParameters parameters,
            TreeBarkMeshSettings settings)
        {
            float groundPlateauEnd = CalculateRootGroundPlateauEnd(
                parameters);
            float rootCollapseEnd = CalculateEffectiveRootCollapseHeight(
                parameters);
            float earliestTransition =
                CalculateEarliestRootTransitionHeight(parameters);
            float effectiveTransition =
                CalculateEffectiveRootTransitionHeight(parameters);
            return new AxialTwistTelemetry
            {
                FirstNonZeroNormalizedDistance = -1f,
                GroundPlateauEndDegrees =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        groundPlateauEnd),
                RootCollapseEndDegrees =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        rootCollapseEnd),
                EarliestRootTransitionDegrees =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        earliestTransition),
                EffectiveRootTransitionDegrees =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        effectiveTransition),
                MaximumAllowedStepDegrees =
                    settings.ResolveMaximumTrunkTwistStepDegrees()
            };
        }

        private static void CalculateAuthoredAxialTwistDistribution(
            TreeResolvedParameters parameters,
            IReadOnlyList<RenderSample> samples,
            ref AxialTwistTelemetry telemetry)
        {
            telemetry.FirstNonZeroNormalizedDistance = -1f;
            telemetry.MaximumStepDegrees = 0f;
            telemetry.MaximumStepStartNormalizedDistance = 0f;
            telemetry.MaximumStepEndNormalizedDistance = 0f;
            if (samples == null || samples.Count == 0 ||
                Mathf.Abs(parameters.TrunkSurfaceTorsionDegrees) <= Epsilon)
            {
                return;
            }

            float previousRoll = ResolveAuthoredTrunkSurfaceRollDegrees(
                parameters,
                samples[0].NormalizedDistance);
            bool foundFirst = Mathf.Abs(previousRoll) > Epsilon;
            if (foundFirst)
            {
                telemetry.FirstNonZeroNormalizedDistance =
                    samples[0].NormalizedDistance;
            }

            for (int index = 1; index < samples.Count; index++)
            {
                float currentRoll =
                    ResolveAuthoredTrunkSurfaceRollDegrees(
                        parameters,
                        samples[index].NormalizedDistance);
                if (!foundFirst && Mathf.Abs(currentRoll) > Epsilon)
                {
                    telemetry.FirstNonZeroNormalizedDistance =
                        samples[index].NormalizedDistance;
                    foundFirst = true;
                }

                float step = Mathf.Abs(currentRoll - previousRoll);
                if (step > telemetry.MaximumStepDegrees)
                {
                    telemetry.MaximumStepDegrees = step;
                    telemetry.MaximumStepStartNormalizedDistance =
                        samples[index - 1].NormalizedDistance;
                    telemetry.MaximumStepEndNormalizedDistance =
                        samples[index].NormalizedDistance;
                }
                previousRoll = currentRoll;
            }
        }

        private static void CopyAxialTwistTelemetry(
            TreeBarkMeshBuildResult result,
            AxialTwistTelemetry telemetry)
        {
            if (result == null)
            {
                return;
            }

            result.FirstAuthoredAxialTwistNormalizedDistance =
                telemetry.FirstNonZeroNormalizedDistance;
            result.AxialTwistAtGroundPlateauEndDegrees =
                telemetry.GroundPlateauEndDegrees;
            result.AxialTwistAtRootCollapseEndDegrees =
                telemetry.RootCollapseEndDegrees;
            result.AxialTwistAtEarliestRootTransitionDegrees =
                telemetry.EarliestRootTransitionDegrees;
            result.AxialTwistAtEffectiveRootTransitionDegrees =
                telemetry.EffectiveRootTransitionDegrees;
            result.MaximumAuthoredAxialTwistStepDegrees =
                telemetry.MaximumStepDegrees;
            result.MaximumAllowedAxialTwistStepDegrees =
                telemetry.MaximumAllowedStepDegrees;
            result.MaximumAuthoredAxialTwistStepStartNormalizedDistance =
                telemetry.MaximumStepStartNormalizedDistance;
            result.MaximumAuthoredAxialTwistStepEndNormalizedDistance =
                telemetry.MaximumStepEndNormalizedDistance;
        }

        private static void CalculateRootDimensions(
            IReadOnlyList<Vector3> vertices,
            int firstRingVertex,
            int radialSegments,
            out float width,
            out float depth)
        {
            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumZ = float.PositiveInfinity;
            float maximumZ = float.NegativeInfinity;
            for (int side = 0; side < radialSegments; side++)
            {
                Vector3 vertex = vertices[firstRingVertex + side];
                minimumX = Mathf.Min(minimumX, vertex.x);
                maximumX = Mathf.Max(maximumX, vertex.x);
                minimumZ = Mathf.Min(minimumZ, vertex.z);
                maximumZ = Mathf.Max(maximumZ, vertex.z);
            }

            width = Mathf.Max(0f, maximumX - minimumX);
            depth = Mathf.Max(0f, maximumZ - minimumZ);
        }


        private static int ResolveBestRingPhase(
            RenderSample sample,
            int radialSegments,
            int previousRing,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            int previousPhase)
        {
            int bestPhase = previousPhase;
            float bestMinimumOrientation = float.NegativeInfinity;
            float bestEdgeCost = float.PositiveInfinity;
            int bestAbsoluteDelta = int.MaxValue;
            int firstDelta = -radialSegments / 2;

            for (int candidateIndex = 0;
                 candidateIndex < radialSegments;
                 candidateIndex++)
            {
                int delta = firstDelta + candidateIndex;
                int candidatePhase = previousPhase + delta;
                float minimumOrientation = float.PositiveInfinity;
                float edgeCost = 0f;

                for (int side = 0; side < radialSegments; side++)
                {
                    int aIndex = previousRing + side;
                    int dIndex = previousRing + side + 1;
                    BuildRingVertex(
                        sample,
                        radialSegments,
                        side + candidatePhase,
                        out Vector3 bPosition,
                        out Vector3 bNormal);
                    BuildRingVertex(
                        sample,
                        radialSegments,
                        side + 1 + candidatePhase,
                        out Vector3 cPosition,
                        out Vector3 cNormal);

                    Vector3 aPosition = vertices[aIndex];
                    Vector3 dPosition = vertices[dIndex];
                    Vector3 aNormal = normals[aIndex];
                    Vector3 dNormal = normals[dIndex];

                    float currentMinimum = Mathf.Min(
                        EvaluateTriangleOrientation(
                            aPosition,
                            dPosition,
                            cPosition,
                            aNormal,
                            dNormal,
                            cNormal),
                        EvaluateTriangleOrientation(
                            aPosition,
                            cPosition,
                            bPosition,
                            aNormal,
                            cNormal,
                            bNormal));
                    float alternateMinimum = Mathf.Min(
                        EvaluateTriangleOrientation(
                            aPosition,
                            dPosition,
                            bPosition,
                            aNormal,
                            dNormal,
                            bNormal),
                        EvaluateTriangleOrientation(
                            dPosition,
                            cPosition,
                            bPosition,
                            dNormal,
                            cNormal,
                            bNormal));
                    minimumOrientation = Mathf.Min(
                        minimumOrientation,
                        Mathf.Max(currentMinimum, alternateMinimum));
                    edgeCost +=
                        (aPosition - bPosition).sqrMagnitude +
                        (dPosition - cPosition).sqrMagnitude;
                }

                int absoluteDelta = Mathf.Abs(delta);
                bool better =
                    minimumOrientation >
                        bestMinimumOrientation + 0.000001f ||
                    (Mathf.Abs(
                         minimumOrientation - bestMinimumOrientation) <=
                         0.000001f &&
                     edgeCost < bestEdgeCost - 0.000001f) ||
                    (Mathf.Abs(
                         minimumOrientation - bestMinimumOrientation) <=
                         0.000001f &&
                     Mathf.Abs(edgeCost - bestEdgeCost) <= 0.000001f &&
                     absoluteDelta < bestAbsoluteDelta);
                if (better)
                {
                    bestPhase = candidatePhase;
                    bestMinimumOrientation = minimumOrientation;
                    bestEdgeCost = edgeCost;
                    bestAbsoluteDelta = absoluteDelta;
                }
            }

            return bestPhase;
        }

        private static void BuildRingVertex(
            RenderSample sample,
            int radialSegments,
            int unwrappedSide,
            out Vector3 position,
            out Vector3 normal)
        {
            float angle =
                unwrappedSide / (float)radialSegments * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            normal = SafeNormalize(
                sample.Normal * cosine +
                sample.Binormal * sine,
                sample.Normal);
            position = sample.Position + normal * sample.Radius;
        }

        private static void ApplyCurvatureRadiusSafety(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            List<RenderSample> samples,
            float minimumRenderedRadius,
            ref int curvatureRadiusClampCount)
        {
            if (samples == null || samples.Count < 3)
            {
                return;
            }

            int sampleCount = samples.Count;
            var targetRadii = new float[sampleCount];
            var maximumPhysicalRadii = new float[sampleCount];
            var crossSectionMultipliers = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                targetRadii[index] = samples[index].Radius;
                maximumPhysicalRadii[index] = float.PositiveInfinity;
                crossSectionMultipliers[index] = branch.BranchOrder == 0
                    ? CalculateMaximumTrunkCrossSectionMultiplier(
                        definition.ResolvedParameters,
                        branch.Phase,
                        samples[index].NormalizedDistance)
                    : 1f;
            }

            for (int index = 1; index < sampleCount - 1; index++)
            {
                Vector3 incoming =
                    samples[index].Position - samples[index - 1].Position;
                Vector3 outgoing =
                    samples[index + 1].Position - samples[index].Position;
                float incomingLength = incoming.magnitude;
                float outgoingLength = outgoing.magnitude;
                if (incomingLength <= Epsilon || outgoingLength <= Epsilon)
                {
                    continue;
                }

                float turnAngle = Vector3.Angle(incoming, outgoing) *
                    Mathf.Deg2Rad;
                float sineHalf = Mathf.Sin(turnAngle * 0.5f);
                if (sineHalf <= 0.0001f)
                {
                    continue;
                }

                float curvatureRadius =
                    Mathf.Min(incomingLength, outgoingLength) /
                    (2f * sineHalf);
                float safePhysicalRadius = curvatureRadius * 0.65f;
                maximumPhysicalRadii[index] = Mathf.Min(
                    maximumPhysicalRadii[index],
                    safePhysicalRadius);
                maximumPhysicalRadii[index - 1] = Mathf.Min(
                    maximumPhysicalRadii[index - 1],
                    safePhysicalRadius * 1.45f);
                maximumPhysicalRadii[index + 1] = Mathf.Min(
                    maximumPhysicalRadii[index + 1],
                    safePhysicalRadius * 1.45f);
            }

            for (int index = 1; index < sampleCount; index++)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index - 1]))
                {
                    maximumPhysicalRadii[index] = Mathf.Min(
                        maximumPhysicalRadii[index],
                        maximumPhysicalRadii[index - 1] * 1.45f);
                }
            }

            for (int index = sampleCount - 2; index >= 0; index--)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index + 1]))
                {
                    maximumPhysicalRadii[index] = Mathf.Min(
                        maximumPhysicalRadii[index],
                        maximumPhysicalRadii[index + 1] * 1.45f);
                }
            }

            for (int index = 0; index < sampleCount; index++)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index]))
                {
                    float allowedScalarRadius =
                        maximumPhysicalRadii[index] /
                        Mathf.Max(
                            1f,
                            crossSectionMultipliers[index]);
                    targetRadii[index] = Mathf.Min(
                        targetRadii[index],
                        allowedScalarRadius);
                }
            }

            for (int index = 0; index < sampleCount; index++)
            {
                RenderSample sample = samples[index];
                float safeRadius = Mathf.Max(
                    minimumRenderedRadius,
                    targetRadii[index]);
                if (safeRadius < sample.Radius - 0.000001f)
                {
                    sample.Radius = safeRadius;
                    samples[index] = sample;
                    curvatureRadiusClampCount++;
                }
            }
        }


        private static void AppendMixedResolutionStrip(
            int currentRingStart,
            int currentSegments,
            int nextRingStart,
            int nextSegments,
            int sectorCount,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            List<int> triangles,
            List<bool> sideTriangleUsesContour,
            List<Vector3> sideTriangleExpectedDirections,
            ref int alternateQuadDiagonalCount)
        {
            int safeCurrent = Mathf.Max(3, currentSegments);
            int safeNext = Mathf.Max(3, nextSegments);
            int safeSectors = Mathf.Max(1, sectorCount);
            if (safeCurrent % safeSectors != 0 ||
                safeNext % safeSectors != 0)
            {
                safeSectors = 1;
            }

            int currentPerSector = safeCurrent / safeSectors;
            int nextPerSector = safeNext / safeSectors;
            for (int sector = 0; sector < safeSectors; sector++)
            {
                int currentOffset = sector * currentPerSector;
                int nextOffset = sector * nextPerSector;
                int currentStep = 0;
                int nextStep = 0;
                while (currentStep < currentPerSector ||
                       nextStep < nextPerSector)
                {
                    float currentAdvance = currentStep < currentPerSector
                        ? (currentStep + 1) / (float)currentPerSector
                        : float.PositiveInfinity;
                    float nextAdvance = nextStep < nextPerSector
                        ? (nextStep + 1) / (float)nextPerSector
                        : float.PositiveInfinity;
                    int a = currentRingStart + currentOffset + currentStep;
                    int b = nextRingStart + nextOffset + nextStep;

                    if (Mathf.Abs(currentAdvance - nextAdvance) <= 0.000001f)
                    {
                        int d = a + 1;
                        int c = b + 1;
                        Vector3 expected =
                            normals[a] + normals[b] + normals[c] + normals[d];
                        AppendBestOutwardQuad(
                            a,
                            b,
                            c,
                            d,
                            vertices,
                            normals,
                            triangles,
                            true,
                            expected,
                            sideTriangleUsesContour,
                            sideTriangleExpectedDirections,
                            ref alternateQuadDiagonalCount);
                        currentStep++;
                        nextStep++;
                    }
                    else if (currentAdvance < nextAdvance)
                    {
                        int d = a + 1;
                        AppendBestOutwardTriangle(
                            a,
                            b,
                            d,
                            vertices,
                            normals,
                            triangles,
                            sideTriangleUsesContour,
                            sideTriangleExpectedDirections);
                        currentStep++;
                    }
                    else
                    {
                        int c = b + 1;
                        AppendBestOutwardTriangle(
                            a,
                            b,
                            c,
                            vertices,
                            normals,
                            triangles,
                            sideTriangleUsesContour,
                            sideTriangleExpectedDirections);
                        nextStep++;
                    }
                }
            }
        }

        private static void AppendBestOutwardTriangle(
            int a,
            int b,
            int c,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            List<int> triangles,
            List<bool> sideTriangleUsesContour,
            List<Vector3> sideTriangleExpectedDirections)
        {
            Vector3 expected = normals[a] + normals[b] + normals[c];
            if (expected.sqrMagnitude <= Epsilon)
            {
                expected = Vector3.Cross(
                    vertices[b] - vertices[a],
                    vertices[c] - vertices[a]);
            }

            float forward = EvaluateTriangleAgainstExpected(
                vertices[a],
                vertices[b],
                vertices[c],
                expected);
            float reverse = EvaluateTriangleAgainstExpected(
                vertices[a],
                vertices[c],
                vertices[b],
                expected);
            triangles.Add(a);
            if (reverse > forward)
            {
                triangles.Add(c);
                triangles.Add(b);
            }
            else
            {
                triangles.Add(b);
                triangles.Add(c);
            }
            sideTriangleUsesContour.Add(true);
            sideTriangleExpectedDirections.Add(expected);
        }

        private static void AppendBestOutwardQuad(
            int a,
            int b,
            int c,
            int d,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            List<int> triangles,
            bool useContour,
            Vector3 contourExpected,
            List<bool> sideTriangleUsesContour,
            List<Vector3> sideTriangleExpectedDirections,
            ref int alternateQuadDiagonalCount)
        {
            // A transported tube quad can become strongly skewed on highly
            // curved/twisted branches. Both diagonals are topologically valid,
            // but only one may keep both triangles aligned with the authored
            // radial normals. Select deterministically by the weaker triangle.
            float currentFirst = useContour
                ? EvaluateTriangleAgainstExpected(
                    vertices[a], vertices[d], vertices[c], contourExpected)
                : EvaluateTriangleOrientation(a, d, c, vertices, normals);
            float currentSecond = useContour
                ? EvaluateTriangleAgainstExpected(
                    vertices[a], vertices[c], vertices[b], contourExpected)
                : EvaluateTriangleOrientation(a, c, b, vertices, normals);
            float alternateFirst = useContour
                ? EvaluateTriangleAgainstExpected(
                    vertices[a], vertices[d], vertices[b], contourExpected)
                : EvaluateTriangleOrientation(a, d, b, vertices, normals);
            float alternateSecond = useContour
                ? EvaluateTriangleAgainstExpected(
                    vertices[d], vertices[c], vertices[b], contourExpected)
                : EvaluateTriangleOrientation(d, c, b, vertices, normals);

            float currentMinimum = Mathf.Min(currentFirst, currentSecond);
            float alternateMinimum = Mathf.Min(alternateFirst, alternateSecond);
            if (alternateMinimum > currentMinimum + 0.000001f)
            {
                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
                triangles.Add(b);
                alternateQuadDiagonalCount++;
                sideTriangleUsesContour.Add(useContour);
                sideTriangleUsesContour.Add(useContour);
                sideTriangleExpectedDirections.Add(contourExpected);
                sideTriangleExpectedDirections.Add(contourExpected);
                return;
            }

            // Default/tie path preserves the established outward winding.
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            sideTriangleUsesContour.Add(useContour);
            sideTriangleUsesContour.Add(useContour);
            sideTriangleExpectedDirections.Add(contourExpected);
            sideTriangleExpectedDirections.Add(contourExpected);
        }

        private static float EvaluateTriangleAgainstExpected(
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 expected)
        {
            Vector3 faceNormal = Vector3.Cross(b - a, c - a);
            if (faceNormal.sqrMagnitude <= TriangleAreaSquaredEpsilon ||
                expected.sqrMagnitude <= Epsilon)
            {
                return float.NegativeInfinity;
            }

            return Vector3.Dot(faceNormal.normalized, expected.normalized);
        }

        private static float EvaluateTriangleOrientation(
            int a,
            int b,
            int c,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals)
        {
            return EvaluateTriangleOrientation(
                vertices[a],
                vertices[b],
                vertices[c],
                normals[a],
                normals[b],
                normals[c]);
        }

        private static float EvaluateTriangleOrientation(
            Vector3 aPosition,
            Vector3 bPosition,
            Vector3 cPosition,
            Vector3 aNormal,
            Vector3 bNormal,
            Vector3 cNormal)
        {
            Vector3 faceNormal = Vector3.Cross(
                bPosition - aPosition,
                cPosition - aPosition);
            Vector3 expected = aNormal + bNormal + cNormal;
            if (faceNormal.sqrMagnitude <= TriangleAreaSquaredEpsilon ||
                expected.sqrMagnitude <= Epsilon)
            {
                return float.NegativeInfinity;
            }

            return Vector3.Dot(
                faceNormal.normalized,
                expected.normalized);
        }

        private static List<RenderSample> BuildRenderSamples(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<TreeCurveSample> sourceSamples,
            TreeBarkMeshSettings settings,
            ref int curvatureRadiusClampCount)
        {
            var samples = new List<RenderSample>(sourceSamples.Count);
            ParentFrame parentFrame = default;
            bool hasParent = branch.ParentBranchIndex >= 0 &&
                branch.ParentBranchIndex < definition.Branches.Count;
            if (hasParent)
            {
                parentFrame = EvaluateFrame(
                    definition.Branches[branch.ParentBranchIndex],
                    branch.ParentAttachmentDistance);
            }

            int transitionCount = 0;
            if (hasParent)
            {
                int maximumTransitionCount = Mathf.Min(
                    sourceSamples.Count,
                    settings.BranchRootTransitionRingCount);
                float targetBlendLength =
                    Mathf.Max(settings.MinimumRenderedRadius, sourceSamples[0].Radius) *
                    settings.BranchRootBlendLengthInChildRadii;
                float travelled = 0f;
                transitionCount = 1;
                for (int index = 1;
                     index < maximumTransitionCount;
                     index++)
                {
                    travelled += Vector3.Distance(
                        sourceSamples[index - 1].Position,
                        sourceSamples[index].Position);
                    transitionCount = index + 1;
                    if (travelled >= targetBlendLength && transitionCount >= 2)
                    {
                        break;
                    }
                }

                transitionCount = Mathf.Max(2, transitionCount);
            }
            for (int index = 0; index < sourceSamples.Count; index++)
            {
                TreeCurveSample source = sourceSamples[index];
                Vector3 position = source.Position;
                float radius = Mathf.Max(
                    settings.MinimumRenderedRadius,
                    source.Radius);
                if (hasParent && index < transitionCount)
                {
                    float transition = transitionCount <= 1
                        ? 1f
                        : index / (float)(transitionCount - 1);
                    float rootScale = Mathf.Lerp(
                        settings.BranchRootRadiusScale,
                        1f,
                        transition);
                    float collar = 1f +
                        settings.BranchRootCollarStrength *
                        Mathf.Sin(Mathf.PI * transition) * 0.22f;
                    radius *= rootScale * collar;
                    if (index == 0)
                    {
                        radius = Mathf.Min(
                            radius,
                            Mathf.Max(
                                settings.MinimumRenderedRadius,
                                parentFrame.Radius * 0.9f));
                    }

                    float inward =
                        (1f - transition) *
                        settings.BranchRootInsetRatio *
                        parentFrame.Radius;
                    Vector3 rootAxis = Vector3.ProjectOnPlane(
                        branch.LocalReferenceAxis,
                        parentFrame.Tangent);
                    rootAxis = SafeNormalize(
                        rootAxis,
                        parentFrame.Normal);
                    position -= rootAxis * inward;

                    if (index == 0)
                    {
                        Vector3 delta = position - parentFrame.Position;
                        float axial = Vector3.Dot(delta, parentFrame.Tangent);
                        float maximumRadial = Mathf.Max(
                            0f,
                            parentFrame.Radius - radius * 1.04f);
                        float minimumSameSideRadial = Mathf.Min(
                            maximumRadial,
                            parentFrame.Radius * 0.01f);
                        Vector3 radial = delta - parentFrame.Tangent * axial;
                        float signedRadial = Vector3.Dot(
                            radial,
                            rootAxis);
                        float safeSignedRadial = Mathf.Clamp(
                            signedRadial,
                            minimumSameSideRadial,
                            maximumRadial);

                        // Root inset may embed the child into its parent, but
                        // it must never carry the root centre through the
                        // parent axis and make the branch emerge on the
                        // opposite side. Keep the root on its authored radial
                        // axis and inside the parent tube.
                        position =
                            parentFrame.Position +
                            parentFrame.Tangent * axial +
                            rootAxis * safeSignedRadial;
                    }
                }

                samples.Add(new RenderSample
                {
                    Position = position,
                    Tangent = source.Tangent,
                    Normal = source.Normal,
                    Binormal = source.Binormal,
                    Radius = radius,
                    NormalizedDistance = source.NormalizedDistance,
                    CumulativeDistance = 0f
                });
            }

            float cumulativeDistance = 0f;
            for (int index = 0; index < samples.Count; index++)
            {
                RenderSample sample = samples[index];
                if (index > 0)
                {
                    cumulativeDistance += Vector3.Distance(
                        samples[index - 1].Position,
                        sample.Position);
                }
                sample.CumulativeDistance = cumulativeDistance;
                samples[index] = sample;
            }

            RebuildTransportedFrames(samples);
            ApplyCurvatureRadiusSafety(
                definition,
                branch,
                samples,
                settings.MinimumRenderedRadius,
                ref curvatureRadiusClampCount);
            return samples;
        }

        private static void RebuildTransportedFrames(
            List<RenderSample> samples)
        {
            if (samples == null || samples.Count < 2)
            {
                return;
            }

            Vector3 previousNormal = Vector3.zero;
            Vector3 previousTangent = Vector3.zero;
            for (int index = 0; index < samples.Count; index++)
            {
                RenderSample source = samples[index];
                Vector3 tangent;
                if (index == 0)
                {
                    tangent = samples[1].Position - source.Position;
                }
                else if (index == samples.Count - 1)
                {
                    tangent = source.Position - samples[index - 1].Position;
                }
                else
                {
                    tangent = samples[index + 1].Position -
                        samples[index - 1].Position;
                }

                tangent = SafeNormalize(
                    tangent,
                    source.Tangent.sqrMagnitude > Epsilon
                        ? source.Tangent
                        : Vector3.up);

                // Preserve the source frame's authored/generated roll (surface
                // torsion) while adapting it to the root-adjusted tangent. A
                // pure previous-frame transport would erase torsion from every
                // branch after root transition construction.
                Vector3 normal = Vector3.ProjectOnPlane(
                    source.Normal,
                    tangent);
                if (normal.sqrMagnitude <= Epsilon &&
                    previousNormal.sqrMagnitude > Epsilon)
                {
                    Quaternion transport = Quaternion.FromToRotation(
                        previousTangent,
                        tangent);
                    normal = Vector3.ProjectOnPlane(
                        transport * previousNormal,
                        tangent);
                }

                if (normal.sqrMagnitude <= Epsilon)
                {
                    normal = ChooseInitialNormal(tangent);
                }
                else
                {
                    normal.Normalize();
                }

                if (previousNormal.sqrMagnitude > Epsilon &&
                    Vector3.Dot(previousNormal, normal) < 0f)
                {
                    normal = -normal;
                }

                Vector3 binormal = Vector3.Cross(tangent, normal);
                if (binormal.sqrMagnitude <= Epsilon)
                {
                    normal = ChooseInitialNormal(tangent);
                    binormal = Vector3.Cross(tangent, normal);
                }

                binormal.Normalize();
                normal = Vector3.Cross(binormal, tangent).normalized;
                source.Tangent = tangent;
                source.Normal = normal;
                source.Binormal = binormal;
                samples[index] = source;
                previousTangent = tangent;
                previousNormal = normal;
            }
        }

        private static Vector3 ChooseInitialNormal(Vector3 tangent)
        {
            Vector3 candidate = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) < 0.92f
                ? Vector3.up
                : Vector3.right;
            candidate = Vector3.ProjectOnPlane(candidate, tangent);
            return SafeNormalize(candidate, Vector3.forward);
        }

        private static void AppendTaperedTrunkTipClosure(
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            int radialSegments,
            TreeDefinition definition,
            TreeBranchDefinition branch,
            Vector3 apexPosition,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color32> colours,
            List<Vector2> uv0,
            List<int> triangles,
            List<TreeBarkMeshCapAuditRecord> capAuditRecords)
        {
            RenderSample sample = samples[sampleIndex];
            ResolveTrunkSurfaceFrame(
                definition.ResolvedParameters,
                sample,
                out Vector3 capAxis,
                out Vector3 capTangentAxis,
                out _);
            Vector3 capNormal = SafeNormalize(capAxis, Vector3.up);
            Vector3 capTangent = SafeNormalize(
                capTangentAxis,
                Vector3.right);
            Color32 apexMetadata = BuildVertexMetadata(
                definition,
                branch,
                apexPosition);
            int apexIndex = vertices.Count;
            vertices.Add(apexPosition);
            normals.Add(capNormal);
            tangents.Add(new Vector4(
                capTangent.x,
                capTangent.y,
                capTangent.z,
                1f));
            colours.Add(apexMetadata);
            uv0.Add(new Vector2(0.5f, 0.5f));

            Color32 ringMetadata = BuildVertexMetadata(
                definition,
                branch,
                sample.Position);
            int capRingStart = vertices.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                float normalizedSide = side / (float)radialSegments;
                float angle = normalizedSide * TwoPi;
                BuildSurfaceVertex(
                    definition,
                    branch,
                    samples,
                    sampleIndex,
                    normalizedSide,
                    radialSegments,
                    out Vector3 position,
                    out _,
                    out Vector3 circumferenceTangent,
                    out _);
                Vector3 radialReference = SafeNormalize(
                    position - sample.Position,
                    sample.Normal);
                Vector3 coneNormal = SafeNormalize(
                    Vector3.Cross(
                        circumferenceTangent,
                        apexPosition - position),
                    radialReference);
                if (Vector3.Dot(coneNormal, radialReference) < 0f)
                {
                    coneNormal = -coneNormal;
                }

                vertices.Add(position);
                normals.Add(coneNormal);
                tangents.Add(new Vector4(
                    circumferenceTangent.x,
                    circumferenceTangent.y,
                    circumferenceTangent.z,
                    1f));
                colours.Add(ringMetadata);
                uv0.Add(new Vector2(
                    0.5f + Mathf.Cos(angle) * 0.5f,
                    0.5f + Mathf.Sin(angle) * 0.5f));
            }

            int triangleStart = triangles.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                int current = capRingStart + side;
                int next = capRingStart +
                    (side + 1) % radialSegments;
                triangles.Add(apexIndex);
                triangles.Add(current);
                triangles.Add(next);
            }

            capAuditRecords.Add(new TreeBarkMeshCapAuditRecord
            {
                TriangleStart = triangleStart,
                TriangleCount = triangles.Count - triangleStart,
                ExpectedNormal = capNormal
            });
        }

        private static void AppendCap(
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            int radialSegments,
            TreeDefinition definition,
            TreeBranchDefinition branch,
            bool isTip,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color32> colours,
            List<Vector2> uv0,
            List<int> triangles,
            List<TreeBarkMeshCapAuditRecord> capAuditRecords)
        {
            RenderSample sample = samples[sampleIndex];
            Vector3 capAxis = sample.Tangent;
            Vector3 capTangentAxis = sample.Normal;
            if (branch.BranchOrder == 0)
            {
                ResolveTrunkSurfaceFrame(
                    definition.ResolvedParameters,
                    sample,
                    out capAxis,
                    out capTangentAxis,
                    out _);
            }

            Vector3 capNormal = isTip
                ? SafeNormalize(capAxis, Vector3.up)
                : -SafeNormalize(capAxis, Vector3.up);
            Vector3 capTangent = SafeNormalize(
                capTangentAxis,
                Vector3.right);
            float tangentHandedness = isTip ? 1f : -1f;
            Color32 metadata = BuildVertexMetadata(
                definition,
                branch,
                sample.Position);
            int centreIndex = vertices.Count;
            vertices.Add(sample.Position);
            normals.Add(capNormal);
            tangents.Add(new Vector4(
                capTangent.x,
                capTangent.y,
                capTangent.z,
                tangentHandedness));
            colours.Add(metadata);
            uv0.Add(new Vector2(0.5f, 0.5f));

            int capRingStart = vertices.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                float normalizedSide = side / (float)radialSegments;
                float angle = normalizedSide * TwoPi;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                Vector3 position;
                if (branch.BranchOrder == 0)
                {
                    position = EvaluateTrunkSurfacePosition(
                        definition,
                        branch,
                        sample,
                        normalizedSide,
                        out _);
                }
                else
                {
                    Vector3 radial = SafeNormalize(
                        sample.Normal * cosine +
                        sample.Binormal * sine,
                        sample.Normal);
                    position = sample.Position +
                        radial * sample.Radius;
                }

                vertices.Add(position);
                normals.Add(capNormal);
                tangents.Add(new Vector4(
                    capTangent.x,
                    capTangent.y,
                    capTangent.z,
                    tangentHandedness));
                colours.Add(metadata);
                uv0.Add(new Vector2(
                    0.5f + cosine * 0.5f,
                    0.5f + sine * 0.5f));
            }

            int triangleStart = triangles.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                int current = capRingStart + side;
                int next = capRingStart + (side + 1) % radialSegments;
                triangles.Add(centreIndex);
                if (isTip)
                {
                    triangles.Add(current);
                    triangles.Add(next);
                }
                else
                {
                    triangles.Add(next);
                    triangles.Add(current);
                }
            }

            capAuditRecords.Add(new TreeBarkMeshCapAuditRecord
            {
                TriangleStart = triangleStart,
                TriangleCount = triangles.Count - triangleStart,
                ExpectedNormal = capNormal
            });
        }


        private static Color32 BuildVertexMetadata(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            Vector3 position)
        {
            Bounds bounds = definition.LocalBounds;
            float height = Mathf.Max(0.0001f, bounds.size.y);
            float normalizedHeight = Mathf.Clamp01(
                (position.y - bounds.min.y) / height);
            float windMask = normalizedHeight * normalizedHeight *
                (3f - 2f * normalizedHeight);
            byte red = ToByte(windMask);
            byte green = ToByte(Mathf.Clamp01(branch.BranchOrder / 3f));
            byte blue = ToByte(Mathf.Clamp01(branch.Stiffness));
            byte alpha = definition.ResolvedParameters != null &&
                definition.ResolvedParameters.RecipeOnlyControlSource
                    ? branch.IsDead ? (byte)255 : (byte)0
                    : ToByte(Mathf.Repeat(branch.Phase, 1f));
            return new Color32(red, green, blue, alpha);
        }

        private static ParentFrame EvaluateFrame(
            TreeBranchDefinition branch,
            float normalizedDistance)
        {
            IReadOnlyList<TreeCurveSample> samples = branch.Samples;
            float scaled = Mathf.Clamp01(normalizedDistance) * (samples.Count - 1);
            int lower = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, samples.Count - 1);
            int upper = Mathf.Min(samples.Count - 1, lower + 1);
            float t = scaled - lower;
            TreeCurveSample a = samples[lower];
            TreeCurveSample b = samples[upper];
            Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            return new ParentFrame
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Normal = normal,
                Binormal = binormal,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t)
            };
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        }

        private static bool IsUsableSample(RenderSample sample)
        {
            return IsFinite(sample.Position) &&
                IsFinite(sample.Tangent) &&
                IsFinite(sample.Normal) &&
                IsFinite(sample.Binormal) &&
                sample.Tangent.sqrMagnitude > Epsilon &&
                sample.Normal.sqrMagnitude > Epsilon &&
                sample.Binormal.sqrMagnitude > Epsilon &&
                TreeDeterministicUtility.IsFinite(sample.Radius) &&
                sample.Radius > 0f &&
                TreeDeterministicUtility.IsFinite(
                    sample.CumulativeDistance);
        }

        private static bool IsFinite(Vector3 value)
        {
            return TreeDeterministicUtility.IsFinite(value.x) &&
                TreeDeterministicUtility.IsFinite(value.y) &&
                TreeDeterministicUtility.IsFinite(value.z);
        }

        private static Vector3 SafeNormalize(
            Vector3 value,
            Vector3 fallback)
        {
            return value.sqrMagnitude > Epsilon
                ? value.normalized
                : fallback.normalized;
        }

        public static string CalculateInputFingerprint(
            TreeDefinition definition,
            TreeBarkMeshSettings settings)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(
                ref hash,
                definition.StructuralFingerprint);
            TreeDeterministicUtility.Append(ref hash, BarkAlgorithmVersion);
            TreeDeterministicUtility.Append(ref hash, settings.SettingsVersion);
            TreeDeterministicUtility.Append(ref hash, settings.TrunkRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.PrimaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.SecondaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.TertiaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.BarkMetersPerTile);
            TreeDeterministicUtility.Append(ref hash, settings.MinimumRenderedRadius);
            TreeDeterministicUtility.Append(ref hash, settings.CapTrunkBase);
            TreeDeterministicUtility.Append(ref hash, settings.CapBranchTips);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootInsetRatio);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootBlendLengthInChildRadii);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootRadiusScale);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootCollarStrength);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootTransitionRingCount);
            if (settings.EfficiencyPolicy !=
                TreeBarkMeshEfficiencyPolicy.Current)
            {
                TreeDeterministicUtility.Append(
                    ref hash,
                    (int)settings.EfficiencyPolicy);
            }
            TreeResolvedParameters parameters = definition.ResolvedParameters;
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.TrunkSurfaceTorsionDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootButtressCount);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootButtressStrength);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootButtressHeight);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootFlareScale);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RecipeOnlyControlSource);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootReach);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootThickness);
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string BuildGeometryFingerprint(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector4> tangents,
            IReadOnlyList<Color32> colours,
            IReadOnlyList<Vector2> uv0,
            IReadOnlyList<int> triangles)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, vertices.Count);
            for (int index = 0; index < vertices.Count; index++)
            {
                TreeDeterministicUtility.Append(ref hash, vertices[index]);
                TreeDeterministicUtility.Append(ref hash, normals[index]);
                TreeDeterministicUtility.Append(ref hash, tangents[index].x);
                TreeDeterministicUtility.Append(ref hash, tangents[index].y);
                TreeDeterministicUtility.Append(ref hash, tangents[index].z);
                TreeDeterministicUtility.Append(ref hash, tangents[index].w);
                Color32 colour = colours[index];
                TreeDeterministicUtility.Append(ref hash, colour.r);
                TreeDeterministicUtility.Append(ref hash, colour.g);
                TreeDeterministicUtility.Append(ref hash, colour.b);
                TreeDeterministicUtility.Append(ref hash, colour.a);
                TreeDeterministicUtility.Append(ref hash, uv0[index].x);
                TreeDeterministicUtility.Append(ref hash, uv0[index].y);
            }

            TreeDeterministicUtility.Append(ref hash, triangles.Count);
            for (int index = 0; index < triangles.Count; index++)
            {
                TreeDeterministicUtility.Append(ref hash, triangles[index]);
            }

            return TreeDeterministicUtility.FormatHash(hash);
        }
    }
}
