using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        public readonly struct PlaneCutBevelPreviewStatus
        {
            public readonly bool PreviewApplied;
            public readonly int ActiveEdgeCount;
            public readonly int BuiltEdgeCount;
            public readonly int DeferredEdgeCount;
            public readonly int RejectedEdgeCount;
            public readonly string Diagnostic;

            public PlaneCutBevelPreviewStatus(
                bool previewApplied,
                int activeEdgeCount,
                int builtEdgeCount,
                int deferredEdgeCount,
                int rejectedEdgeCount,
                string diagnostic)
            {
                PreviewApplied = previewApplied;
                ActiveEdgeCount = activeEdgeCount;
                BuiltEdgeCount = builtEdgeCount;
                DeferredEdgeCount = deferredEdgeCount;
                RejectedEdgeCount = rejectedEdgeCount;
                Diagnostic = diagnostic ?? string.Empty;
            }
        }


        public readonly struct BoundedEdgePreviewStatus
        {
            public readonly bool PreviewApplied;
            public readonly int CandidateCount;
            public readonly int SelectedOrdinal;
            public readonly int SourceEdgeIndex;
            public readonly int BevelFaceCount;
            public readonly int EndpointCapCount;
            public readonly int ModifiedSourceFaceCount;
            public readonly int ForeignSourceFaceModifiedCount;
            public readonly float RailDeviation;
            public readonly float MaximumExtentBeyondRails;
            public readonly string Diagnostic;

            public BoundedEdgePreviewStatus(
                bool previewApplied,
                int candidateCount,
                int selectedOrdinal,
                int sourceEdgeIndex,
                int bevelFaceCount,
                int endpointCapCount,
                int modifiedSourceFaceCount,
                int foreignSourceFaceModifiedCount,
                float railDeviation,
                float maximumExtentBeyondRails,
                string diagnostic)
            {
                PreviewApplied = previewApplied;
                CandidateCount = candidateCount;
                SelectedOrdinal = selectedOrdinal;
                SourceEdgeIndex = sourceEdgeIndex;
                BevelFaceCount = bevelFaceCount;
                EndpointCapCount = endpointCapCount;
                ModifiedSourceFaceCount = modifiedSourceFaceCount;
                ForeignSourceFaceModifiedCount =
                    foreignSourceFaceModifiedCount;
                RailDeviation = railDeviation;
                MaximumExtentBeyondRails = maximumExtentBeyondRails;
                Diagnostic = diagnostic ?? string.Empty;
            }
        }



        public enum EdgeWearDebugEdgeState
        {
            Unassessed,
            Certified,
            Selected,
            EligibleUnselected,
            ArtisticFiltered,
            WidthFloorFailure,
            IsolatedRailFailure,
            GeometricExcluded,
            StructuralExcluded,
            CoexistenceExcluded
        }

        public struct EdgeWearDebugEdgeRecord
        {
            public int EdgeIndex;
            public Vector3 Start;
            public Vector3 End;
            public bool Selected;
            public bool Focus;
            public EdgeWearDebugEdgeState State;
            public string Reason;
            public float Length;
            public float DihedralDegrees;

            public EdgeWearDebugEdgeRecord(
                int edgeIndex,
                Vector3 start,
                Vector3 end,
                bool selected,
                bool focus)
                : this(
                    edgeIndex,
                    start,
                    end,
                    selected,
                    focus,
                    selected
                        ? EdgeWearDebugEdgeState.Selected
                        : EdgeWearDebugEdgeState.Unassessed,
                    string.Empty,
                    (end - start).magnitude,
                    0f)
            {
            }

            public EdgeWearDebugEdgeRecord(
                int edgeIndex,
                Vector3 start,
                Vector3 end,
                bool selected,
                bool focus,
                EdgeWearDebugEdgeState state,
                string reason,
                float length,
                float dihedralDegrees)
            {
                EdgeIndex = edgeIndex;
                Start = start;
                End = end;
                Selected = selected;
                Focus = focus;
                State = state;
                Reason = reason ?? string.Empty;
                Length = length;
                DihedralDegrees = dihedralDegrees;
            }
        }

        public readonly struct SourceEdgeIndexDebugStatus
        {
            public readonly bool Available;
            public readonly string Diagnostic;
            public readonly EdgeWearDebugEdgeRecord[] Edges;

            public SourceEdgeIndexDebugStatus(
                bool available,
                string diagnostic,
                EdgeWearDebugEdgeRecord[] edges)
            {
                Available = available;
                Diagnostic = diagnostic ?? string.Empty;
                Edges = edges ?? Array.Empty<EdgeWearDebugEdgeRecord>();
            }
        }

#if UNITY_EDITOR
        public sealed class EdgeWearArtisticEdgeAuditRecord
        {
            public int SourceEdgeIndex = -1;
            public int CandidateIndex = -1;
            public Vector3 Start;
            public Vector3 End;
            public Vector3 Midpoint;
            public Vector3 OwnerNormalA;
            public Vector3 OwnerNormalB;
            public Vector3 BevelNormal;
            public int FaceA = -1;
            public int FaceB = -1;
            public int FaceCount;
            public float Length;
            public float DihedralDegrees;
            public float Vertical01;
            public string Classification = string.Empty;
            public int CoincidentBoundarySeamReconciled;
            public int StructuralEligible;
            public int GeometricEligible;
            public int CoexistenceEligible;
            public int ArtisticEligible;
            public int ArtisticLengthEligible;
            public int ArtisticAngleEligible;
            public int ArtisticBaseEligible;
            public string ArtisticFilterReason = string.Empty;
            public string CandidateReason = string.Empty;
            public string FinalReason = string.Empty;
            public float Score;
            public float ArtisticMinimumLength;
            public float ArtisticLengthScore;
            public float ArtisticAngleScore;
            public float ArtisticRandomScore;
            public float ArtisticBaseSuppression;
            public float ArtisticUpwardEdgeBoost;
            public float ArtisticCharacterBoost;
            public float ArtisticEdgeAxisVertical01;
            public float ArtisticEdgeAxisAbsX;
            public float ArtisticEdgeAxisAbsY;
            public float ArtisticEdgeAxisAbsZ;
            public float ArtisticSilhouettePotential;
            public float ArtisticFeasibleWidthFraction;
            public float ArtisticSolvedWidthFraction;
            public float ArtisticLocalDensity01;
            public int ArtisticSharedVertexDegreeA;
            public int ArtisticSharedVertexDegreeB;
            public int ArtisticSelectionRank = -1;
            public float ArtisticSelectionThreshold;
            public float ArtisticSelectionDelta;
            public float ArtisticDeterministicVariation;
            public float ArtisticStrength;
            public float ArtisticDepthMultiplier;
            public float RequestedWidth;
            public float RequiredFootprintLength;
            public float LengthToWidthRatio;
            public float LocalityRetainPlaneFloor;
            public float LocalityRemovalPlaneCeiling;
            public float LocalityFeasibleMargin;
            public float LocalityGuardMargin;
            public float LocalityMinimumRemoval;
            public int LocalityLimitingVertex = -1;
            public Vector3 LocalityLimitingPosition;
            public float MaximumLocallyFeasibleWidth;
            public float FeasibleWidthFraction;
            public int IsolatedSucceeded;
            public int IsolatedWidthAttemptCount;
            public float IsolatedLastAttemptedWidth;
            public float IsolatedMaximumCertifiedWidth;
            public float IsolatedMaximumCertifiedWidthFraction;
            public float EndpointConsumptionA;
            public float EndpointConsumptionB;
            public float RemainingCentralSpan;
            public float MinimumCentralSpan;
            public int IsolatedOpenEdgeCount;
            public int IsolatedNonManifoldEdgeCount;
            public int IsolatedTJunctionCount;
            public int IsolatedInvalidFaceCount;
            public string IsolatedDiagnostic = string.Empty;
            public string ViabilityFailureReason = string.Empty;
            public float SolvedWidth;
            public float MaterializedWidth;
            public float MaterializedWidthScale;
            public int WidthReduced;
            public int Candidate;
            public int Selected;
            public int WidthInactive;
            public int Active;
            public int AttemptedBuilt;
            public int CertifiedBuilt;
            public int TrialRejected;
            public int Deferred;
            public int Rejected;
        }

        public sealed class EdgeWearBatchAuditCaseResult
        {
            public bool Completed;
            public bool AuditCaptured;
            public bool PlacementCaptured;
            public bool CornerSolutionValid;
            public bool PreviewApplied;
            public bool RequireAllGeometricCandidates;
            public int ShapeSeed;
            public float EdgeWearWidth;
            public double TotalMilliseconds;
            public double PreflightMilliseconds;
            public int RawSourceEdgeCount;
            public int SourceEdgeCount;
            public int CoincidentBoundarySeamPairCount;
            public int CoincidentGraphVertexReconciliationCount;
            public int CoincidentGraphBoundarySeamPairCount;
            public int BaselineGeometricEligibleCount;
            public int RecoveredGeometricEdgeCount;
            public int CollateralLostEdgeCount;
            public int CollateralChangedEdgeCount;
            public int CollateralPreservationValid;
            public string RecoveredGeometricEdgeIds = string.Empty;
            public string CollateralLostEdgeIds = string.Empty;
            public string CollateralChangedEdgeIds = string.Empty;
            public int StructuralEligibleCount;
            public int GeometricEligibleCount;
            public int CoexistenceEligibleCount;
            public int CoexistenceIneligibleCount;
            public int ArtisticEligibleCount;
            public int ArtisticFilteredCount;
            public int ArtisticShortFilteredCount;
            public int ArtisticShallowFilteredCount;
            public int ArtisticBaseFilteredCount;
            public int ArtisticOtherFilteredCount;
            public float ArtisticSelectionThreshold;
            public float ArtisticScoreMinimum;
            public float ArtisticScoreMedian;
            public float ArtisticScoreMaximum;
            public float ArtisticSelectedScoreMinimum;
            public float ArtisticSelectedScoreMedian;
            public float ArtisticSelectedScoreMaximum;
            public float ArtisticFilteredScoreMinimum;
            public float ArtisticFilteredScoreMedian;
            public float ArtisticFilteredScoreMaximum;
            public string ArtisticLengthBins = string.Empty;
            public string ArtisticDihedralBins = string.Empty;
            public string ArtisticOrientationBins = string.Empty;
            public string ArtisticSilhouetteBins = string.Empty;
            public string ArtisticLocalDensityBins = string.Empty;
            public string ArtisticCrowdingBins = string.Empty;
            public EdgeWearArtisticEdgeAuditRecord[] ArtisticEdges =
                Array.Empty<EdgeWearArtisticEdgeAuditRecord>();
            public int CandidateCount;
            public int SelectedCount;
            public int CertifiedCount;
            public int DeferredCount;
            public int RejectedCount;
            public int TrialRejectedCount;
            public int BoundaryExclusionCount;
            public int DihedralExclusionCount;
            public int FootprintExclusionCount;
            public int LocalityExclusionCount;
            public int IsolatedRailExclusionCount;
            public int SupportExclusionCount;
            public int WidthFractionExclusionCount;
            public int EndpointSpanExclusionCount;
            public int OtherExclusionCount;
            public int SourceVertexStarExclusionCount;
            public int PlanePairExclusionCount;
            public int PlaneBandExclusionCount;
            public int GlobalWidthFloorExclusionCount;
            public int CandidateConservationExclusionCount;
            public int CornerWidthMissingExclusionCount;
            public int CornerWidthInactiveExclusionCount;
            public int CoexistenceTrialCount;
            public int CoexistenceCacheUseCount;
            public int CoexistenceSearchStatesEvaluated;
            public int CoexistenceSearchStatesDeduplicated;
            public int CoexistenceSearchMaximumDepth;
            public int CoexistenceSearchFrontierRemaining;
            public int CoexistenceSearchWinningDepth;
            public int CandidateConservationFailureCount;
            public int SolverPassCount;
            public int WidthReductionCount;
            public float MinimumWidthScale = 1f;
            public int OpenEdgeCount;
            public int NonManifoldEdgeCount;
            public int TJunctionCount;
            public int InvalidFaceCount;
            public int NonPlanarFaceCount;
            public int SurfaceRenderValid;
            public int MeshValid;
            public int GeometryValid;
            public int CoverageValid;
            public int StableFingerprintPrepared;
            public int LocalityEvaluationCount;
            public int LocalityConstructionUseCount;
            public int LocalityCacheMissCount;
            public int LocalitySolverRecomputationCount;
            public int PlacementFrameUsesImmutableSource;
            public int PreviewDerivedPlacementParameters;
            public int ObjectTransformChanged;
            public int PreviewUsesCanonicalFrame;
            public string ExclusionReasonHash = string.Empty;
            public string SelectedEdgeHash = string.Empty;
            public string CertifiedEdgeHash = string.Empty;
            public string GeometryTopologyHash = string.Empty;
            public string PlacementFrameHash = string.Empty;
            public string EvaluationHash = string.Empty;
            public string PrimaryFailure = string.Empty;
            public string CoexistenceSearchTrace = string.Empty;

            public int ExpectedCertificationCount =>
                RequireAllGeometricCandidates
                    ? CoexistenceEligibleCount
                    : SelectedCount;

            public float CertifiedRatio => ExpectedCertificationCount > 0
                ? (float)CertifiedCount / ExpectedCertificationCount
                : CertifiedCount == 0 ? 1f : 0f;

            public bool Passed =>
                Completed &&
                AuditCaptured &&
                PlacementCaptured &&
                CornerSolutionValid &&
                PreviewApplied &&
                (!RequireAllGeometricCandidates ||
                 SelectedCount == CoexistenceEligibleCount) &&
                CertifiedCount == ExpectedCertificationCount &&
                MinimumWidthScale + 0.0001f >=
                    EdgeWearMinimumFeasibleWidthFraction &&
                DeferredCount == 0 &&
                RejectedCount == 0 &&
                TrialRejectedCount == 0 &&
                CoverageValid == 1 &&
                GeometryValid == 1 &&
                MeshValid == 1 &&
                SurfaceRenderValid == 1 &&
                StableFingerprintPrepared == 1 &&
                OpenEdgeCount == 0 &&
                NonManifoldEdgeCount == 0 &&
                TJunctionCount == 0 &&
                InvalidFaceCount == 0 &&
                NonPlanarFaceCount == 0 &&
                LocalityCacheMissCount == 0 &&
                LocalitySolverRecomputationCount == 0 &&
                ObjectTransformChanged == 0 &&
                PreviewDerivedPlacementParameters == 0 &&
                PreviewUsesCanonicalFrame == 1 &&
                CollateralPreservationValid == 1 &&
                CollateralLostEdgeCount == 0 &&
                CollateralChangedEdgeCount == 0;
        }
#endif

        public readonly struct UnifiedEdgeWearPreviewStatus
        {
            public readonly bool PreviewApplied;
            public readonly int CandidateCount;
            public readonly int RailSolvedEdgeCount;
            public readonly int ActiveEdgeCount;
            public readonly int DeferredEdgeCount;
            public readonly int RejectedEdgeCount;
            public readonly int BevelFaceCount;
            public readonly int VertexJunctionFaceCount;
            public readonly int TriangleCount;
            public readonly string Diagnostic;
            public readonly EdgeWearDebugEdgeRecord[] DebugEdges;

            public UnifiedEdgeWearPreviewStatus(
                bool previewApplied,
                int candidateCount,
                int railSolvedEdgeCount,
                int activeEdgeCount,
                int deferredEdgeCount,
                int rejectedEdgeCount,
                int bevelFaceCount,
                int vertexJunctionFaceCount,
                int triangleCount,
                string diagnostic,
                EdgeWearDebugEdgeRecord[] debugEdges)
            {
                PreviewApplied = previewApplied;
                CandidateCount = candidateCount;
                RailSolvedEdgeCount = railSolvedEdgeCount;
                ActiveEdgeCount = activeEdgeCount;
                DeferredEdgeCount = deferredEdgeCount;
                RejectedEdgeCount = rejectedEdgeCount;
                BevelFaceCount = bevelFaceCount;
                VertexJunctionFaceCount = vertexJunctionFaceCount;
                TriangleCount = triangleCount;
                Diagnostic = diagnostic ?? string.Empty;
                DebugEdges = debugEdges ??
                    Array.Empty<EdgeWearDebugEdgeRecord>();
            }
        }

        private enum EdgeWearEvaluationMode
        {
            None,
            PlaneCutPreview,
            LegacyDiagnosticAudit,
            BoundedSingleEdgePreview,
            UnifiedBoundedPreview,
            UnifiedBatchAudit,
            UnifiedPreviewBatchAudit,
            SourceEdgeIndexDebug
        }

        private const float PlaneEpsilon = 0.0001f;

        // Position welding tolerance in the normalized pre-scale mass.
        // Keep this small: larger values can collapse legitimate short cut edges.
        private const float PointMergeDistance = 0.00001f;
        private const float PointMergeDistanceSqr =
            PointMergeDistance * PointMergeDistance;

        // Dimensionless, scale-relative tests. These must not share PlaneEpsilon:
        // plane distance, edge length and triangle area use different units.
        private const float RelativeCollinearEpsilon = 0.0000000001f;
        private const float RelativeTriangleAreaEpsilon = 0.000000000001f;
        private const float MinimumEdgeLengthSqr = 0.000000000001f;
        private const float TinyFaceAreaEpsilon = 0.0000000001f;
        private static readonly Vector3[] BaseVertices =
        {
            new Vector3(-1f,  1.618034f,  0f),
            new Vector3( 1f,  1.618034f,  0f),
            new Vector3(-1f, -1.618034f,  0f),
            new Vector3( 1f, -1.618034f,  0f),
            new Vector3( 0f, -1f,  1.618034f),
            new Vector3( 0f,  1f,  1.618034f),
            new Vector3( 0f, -1f, -1.618034f),
            new Vector3( 0f,  1f, -1.618034f),
            new Vector3( 1.618034f,  0f, -1f),
            new Vector3( 1.618034f,  0f,  1f),
            new Vector3(-1.618034f,  0f, -1f),
            new Vector3(-1.618034f,  0f,  1f)
        };

        private static readonly int[] BaseTriangles =
        {
             0, 11,  5,
             0,  5,  1,
             0,  1,  7,
             0,  7, 10,
             0, 10, 11,
             1,  5,  9,
             5, 11,  4,
            11, 10,  2,
            10,  7,  6,
             7,  1,  8,
             3,  9,  4,
             3,  4,  2,
             3,  2,  6,
             3,  6,  8,
             3,  8,  9,
             4,  9,  5,
             2,  4, 11,
             6,  2, 10,
             8,  6,  7,
             9,  8,  1
        };

        public static MeshData Generate(MassRecipe recipe)
        {
            return Generate(recipe, null);
        }

        public static MeshData Generate(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            return GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.None,
                -1,
                out _,
                out _,
                out _);
        }

#if UNITY_EDITOR
        public static SourceEdgeIndexDebugStatus
            GenerateSourceEdgeIndexDebug(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures)
        {
            GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.SourceEdgeIndexDebug,
                -1,
                out _,
                out _,
                out UnifiedEdgeWearPreviewStatus debugStatus);
            EdgeWearDebugEdgeRecord[] edges =
                debugStatus.DebugEdges ??
                    Array.Empty<EdgeWearDebugEdgeRecord>();
            string diagnostic = edges.Length > 0
                ? "source topology and edge-wear eligibility graph built; " +
                    "seed=" + recipe.ShapeSeed +
                    ",edges=" + edges.Length
                : "source-edge indexing is unavailable for this mass archetype";
            return new SourceEdgeIndexDebugStatus(
                edges.Length > 0,
                diagnostic,
                edges);
        }

        public static MeshData GeneratePlaneCutBevelPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            out PlaneCutBevelPreviewStatus previewStatus)
        {
            return GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.PlaneCutPreview,
                -1,
                out previewStatus,
                out _,
                out _);
        }

        public static MeshData GenerateBoundedSingleEdgeBevelPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            int selectedOrdinal,
            out BoundedEdgePreviewStatus previewStatus)
        {
            return GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.BoundedSingleEdgePreview,
                selectedOrdinal,
                out _,
                out previewStatus,
                out _);
        }

        public static void RunLegacyEdgeWearDiagnosticAudit(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.LegacyDiagnosticAudit,
                -1,
                out _,
                out _,
                out _);
        }

        public static MeshData GenerateUnifiedEdgeWearPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            out UnifiedEdgeWearPreviewStatus previewStatus)
        {
            return GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedBoundedPreview,
                -1,
                out _,
                out _,
                out previewStatus);
        }

        public static EdgeWearBatchAuditCaseResult
            GenerateUnifiedEdgeWearBatchAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures)
        {
            return GenerateEdgeWearBatchAuditCase(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedBatchAudit,
                true);
        }

        public static EdgeWearBatchAuditCaseResult
            GenerateUnifiedEdgeWearPreviewParityAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures)
        {
            return GenerateEdgeWearBatchAuditCase(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedPreviewBatchAudit,
                false);
        }

        private static EdgeWearBatchAuditCaseResult
            GenerateEdgeWearBatchAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                EdgeWearEvaluationMode evaluationMode,
                bool requireAllGeometricCandidates)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            if (!TryBeginEdgeWearBatchAuditCapture(
                    recipe.ShapeSeed,
                    surfaceFeatures.EdgeWearWidth,
                    requireAllGeometricCandidates,
                    out EdgeWearBatchAuditCaseResult immediateFailure))
            {
                return immediateFailure;
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            Exception evaluationException = null;
            try
            {
                GenerateInternal(
                    recipe,
                    surfaceFeatures,
                    evaluationMode,
                    -1,
                    out _,
                    out _,
                    out _);
            }
            catch (Exception exception)
            {
                evaluationException = exception;
            }
            finally
            {
                stopwatch.Stop();
            }

            return CompleteEdgeWearBatchAuditCapture(
                stopwatch.Elapsed.TotalMilliseconds,
                evaluationException);
        }
#endif

        private static MeshData GenerateInternal(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            EdgeWearEvaluationMode edgeWearEvaluationMode,
            int boundedEdgeOrdinal,
            out PlaneCutBevelPreviewStatus previewStatus,
            out BoundedEdgePreviewStatus boundedPreviewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedPreviewStatus)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            Vector3 dimensions = ResolveDimensions(recipe);

            TriangleSoup soup = BuildMassSoup(
                recipe,
                surfaceFeatures,
                edgeWearEvaluationMode,
                boundedEdgeOrdinal,
                out TriangleSoup placementReferenceSoup,
                out previewStatus,
                out boundedPreviewStatus,
                out unifiedPreviewStatus);
            if (placementReferenceSoup == null)
            {
                placementReferenceSoup = soup;
            }
            bool usesImmutableSourcePlacementFrame =
                !ReferenceEquals(soup, placementReferenceSoup);

#if UNITY_EDITOR
            List<Vector3> edgeDebugPositions =
                ExtractEdgeWearDebugPositions(
                    unifiedPreviewStatus.DebugEdges);
#endif
            ApplyDimensions(soup.Positions, dimensions);
            if (usesImmutableSourcePlacementFrame)
            {
                ApplyDimensions(
                    placementReferenceSoup.Positions,
                    dimensions);
            }
#if UNITY_EDITOR
            ApplyDimensions(edgeDebugPositions, dimensions);
            bool previewApplied =
                previewStatus.PreviewApplied ||
                boundedPreviewStatus.PreviewApplied ||
                unifiedPreviewStatus.PreviewApplied;
            bool hasLegacyPreviewFrame =
                usesImmutableSourcePlacementFrame && previewApplied;
            MassPlacementFrame legacyPreviewFrame = default;
            if (hasLegacyPreviewFrame)
            {
                List<Vector3> legacyPreviewPositions =
                    new List<Vector3>(soup.Positions);
                legacyPreviewFrame =
                    ResolveAndApplyMassPlacementFrame(
                        legacyPreviewPositions,
                        recipe.Lean,
                        recipe.ShapeSeed,
                        recipe.Grounding);
            }
#endif

            MassPlacementFrame placementFrame;
            if (usesImmutableSourcePlacementFrame)
            {
                placementFrame = ResolveAndApplyMassPlacementFrame(
                    placementReferenceSoup.Positions,
                    recipe.Lean,
                    recipe.ShapeSeed,
                    recipe.Grounding);
                ApplyMassPlacementFrame(
                    soup.Positions,
                    placementFrame);
            }
            else
            {
                placementFrame = ResolveAndApplyMassPlacementFrame(
                    soup.Positions,
                    recipe.Lean,
                    recipe.ShapeSeed,
                    recipe.Grounding);
            }
#if UNITY_EDITOR
            ApplyMassPlacementFrame(
                edgeDebugPositions,
                placementFrame);
            ApplyEdgeWearDebugPositions(
                unifiedPreviewStatus.DebugEdges,
                edgeDebugPositions);
            if (edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedBoundedPreview ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedBatchAudit ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedPreviewBatchAudit)
            {
                AppendMassPlacementFrameTelemetry(
                    placementFrame,
                    legacyPreviewFrame,
                    hasLegacyPreviewFrame,
                    usesImmutableSourcePlacementFrame,
                    previewApplied,
                    soup.Positions.Count,
                    edgeDebugPositions.Count);
            }
#endif

            return BuildMeshData(soup, recipe);
        }

#if UNITY_EDITOR
        private static List<Vector3> ExtractEdgeWearDebugPositions(
            EdgeWearDebugEdgeRecord[] debugEdges)
        {
            List<Vector3> positions = new List<Vector3>(
                debugEdges == null ? 0 : debugEdges.Length * 2);
            if (debugEdges == null)
            {
                return positions;
            }
            for (int edgeIndex = 0;
                 edgeIndex < debugEdges.Length;
                 edgeIndex++)
            {
                positions.Add(debugEdges[edgeIndex].Start);
                positions.Add(debugEdges[edgeIndex].End);
            }
            return positions;
        }

        private static void ApplyEdgeWearDebugPositions(
            EdgeWearDebugEdgeRecord[] debugEdges,
            List<Vector3> debugPositions)
        {
            if (debugEdges == null || debugPositions == null ||
                debugPositions.Count != debugEdges.Length * 2)
            {
                return;
            }
            for (int edgeIndex = 0;
                 edgeIndex < debugEdges.Length;
                 edgeIndex++)
            {
                EdgeWearDebugEdgeRecord record = debugEdges[edgeIndex];
                record.Start = debugPositions[edgeIndex * 2];
                record.End = debugPositions[edgeIndex * 2 + 1];
                debugEdges[edgeIndex] = record;
            }
        }
#endif

        private static bool UsesRadialBuilder(MassArchetype archetype)
        {
            return archetype == MassArchetype.PolishedStone;
        }

        private static TriangleSoup BuildMassSoup(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            EdgeWearEvaluationMode edgeWearEvaluationMode,
            int boundedEdgeOrdinal,
            out TriangleSoup placementReferenceSoup,
            out PlaneCutBevelPreviewStatus previewStatus,
            out BoundedEdgePreviewStatus boundedPreviewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedPreviewStatus)
        {
            placementReferenceSoup = null;
            previewStatus = default;
            boundedPreviewStatus = default;
            unifiedPreviewStatus = default;
            if (recipe.Archetype == MassArchetype.LayeredStone)
            {
                TriangleSoup soup = BuildLayeredStoneMass(recipe);
                placementReferenceSoup = soup;
                return soup;
            }
            if (recipe.Archetype == MassArchetype.CarvedMarkerStone)
            {
                TriangleSoup soup = BuildCarvedMarkerMass(recipe);
                placementReferenceSoup = soup;
                return soup;
            }
            if (UsesRadialBuilder(recipe.Archetype))
            {
                TriangleSoup soup = BuildRadialMass(recipe);
                placementReferenceSoup = soup;
                return soup;
            }

            return BuildPlaneCutMass(
                recipe,
                surfaceFeatures,
                edgeWearEvaluationMode,
                boundedEdgeOrdinal,
                out placementReferenceSoup,
                out previewStatus,
                out boundedPreviewStatus,
                out unifiedPreviewStatus);
        }
    }
}
