using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
#if UNITY_EDITOR
        private static Func<bool> edgeWearAuditCancellationProbe;

        public static void SetEditorEdgeWearAuditCancellationProbe(
            Func<bool> cancellationProbe)
        {
            edgeWearAuditCancellationProbe = cancellationProbe;
        }
#endif

        private static bool IsEdgeWearAuditCancellationRequested()
        {
#if UNITY_EDITOR
            Func<bool> probe = edgeWearAuditCancellationProbe;
            if (probe == null)
            {
                return false;
            }

            try
            {
                return probe();
            }
            catch (Exception)
            {
                edgeWearAuditCancellationProbe = null;
                return false;
            }
#else
            return false;
#endif
        }

#if UNITY_EDITOR
        private readonly struct CornerDamageSearchAttemptContext
        {
            public readonly bool Active;
            public readonly int CandidateRank;
            public readonly float CapRingUniformScale;

            public CornerDamageSearchAttemptContext(
                int candidateRank,
                float capRingUniformScale)
            {
                Active = true;
                CandidateRank = Mathf.Max(0, candidateRank);
                CapRingUniformScale = Mathf.Clamp(
                    capRingUniformScale,
                    0.0001f,
                    1f);
            }
        }

        [ThreadStatic]
        private static CornerDamageSearchAttemptContext
            cornerDamageSearchAttemptContext;

        private readonly struct CornerDamageSearchAttemptScope : IDisposable
        {
            private readonly CornerDamageSearchAttemptContext previous;

            public CornerDamageSearchAttemptScope(
                int candidateRank,
                float capRingUniformScale)
            {
                previous = cornerDamageSearchAttemptContext;
                cornerDamageSearchAttemptContext =
                    new CornerDamageSearchAttemptContext(
                        candidateRank,
                        capRingUniformScale);
            }

            public void Dispose()
            {
                cornerDamageSearchAttemptContext = previous;
            }
        }

#endif

        private static int ResolveCornerDamageCandidateRankOverride()
        {
#if UNITY_EDITOR
            return cornerDamageSearchAttemptContext.Active
                ? cornerDamageSearchAttemptContext.CandidateRank
                : 0;
#else
            return 0;
#endif
        }

        private static float ResolveCornerDamageCapRingScaleOverride()
        {
#if UNITY_EDITOR
            return cornerDamageSearchAttemptContext.Active
                ? cornerDamageSearchAttemptContext.CapRingUniformScale
                : 1f;
#else
            return 1f;
#endif
        }

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
            CoexistenceExcluded,
            MicroTopologySuppressed
        }

        public struct EdgeWearDebugEdgeRecord
        {
            public int EdgeIndex;
            public int GraphEdgeIndex;
            public Vector3 Start;
            public Vector3 End;
            public bool Selected;
            public bool Focus;
            public bool Mandatory;
            public bool CornerDamageCapRing;
            public EdgeWearDebugEdgeState State;
            public string Reason;
            public float Length;
            public float DihedralDegrees;
            public float MacroBaseRequestedWidth;
            public float MacroIdentity01;
            public float MacroSampledMultiplier;
            public float MacroEffectiveMultiplier;
            public float MacroRequestedWidth;
            public bool MacroMinimumStyleClamped;

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
                GraphEdgeIndex = edgeIndex;
                Start = start;
                End = end;
                Selected = selected;
                Focus = focus;
                Mandatory = false;
                CornerDamageCapRing = false;
                State = state;
                Reason = reason ?? string.Empty;
                Length = length;
                DihedralDegrees = dihedralDegrees;
                MacroBaseRequestedWidth = 0f;
                MacroIdentity01 = 0f;
                MacroSampledMultiplier = 1f;
                MacroEffectiveMultiplier = 1f;
                MacroRequestedWidth = 0f;
                MacroMinimumStyleClamped = false;
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

        public enum CornerDamagePreviewKind
        {
            GeometryOnly,
            WithEdgeWear
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
            public int MicroTopologySuppressed;
            public int MicroTopologyGeneratedTransition;
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
            public float MacroBaseRequestedWidth;
            public float MacroIdentity01;
            public float MacroSampledMultiplier = 1f;
            public float MacroEffectiveMultiplier = 1f;
            public int MacroMinimumStyleClamped;
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
            public float EdgeWearMacroVariationCoverage;
            public float EdgeWearMacroVariation;
            public float MacroBaseRequestedWidth;
            public float MacroMultiplierMinimum = 1f;
            public float MacroMultiplierMedian = 1f;
            public float MacroMultiplierMaximum = 1f;
            public float MacroRequestedWidthMinimum;
            public float MacroRequestedWidthMedian;
            public float MacroRequestedWidthMaximum;
            public int MacroEvaluatedEdgeCount;
            public int MacroParticipantEdgeCount;
            public int MacroVariedEdgeCount;
            public int MacroMinimumStyleClampedEdgeCount;
            public int MacroFeasibilityReducedEdgeCount;
            public string MacroSignature = string.Empty;
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

        public sealed class CornerDamagePreviewStatus
        {
            public CornerDamagePreviewKind PreviewKind;
            public bool PreviewApplied;
            public bool TransactionCertified;
            public bool AuthoringEnabled;
            public int ShapeSeed;
            public int CandidateCornerCount;
            public int AttemptedCornerCount;
            public int AttemptedConfigurationCount;
            public int AcceptedCornerRank = -1;
            public float CapRingCommittedScale;
            public string SearchFailureStage = string.Empty;
            public string SearchFailureReason = string.Empty;
            public string SearchAttemptSummary = string.Empty;
            public int SelectedGraphVertexIndex = -1;
            public int AcceptedTrialIndex = -1;
            public float RequestedDepthFraction;
            public float DepthVariation;
            public float DepthVariationIdentity;
            public float ResolvedDepthFraction;
            public float TopFacingPreference;
            public float ShortestIncidentEdgeLength;
            public float RequestedDepthAbsolute;
            public float AcceptedDepth;
            public float AcceptedDepthFraction;
            public float AcceptedRetryFactor;
            public float AcceptedVsRequestedRatio;
            public float OrdinaryRequestedWidth;
            public float CapRingWidthScale;
            public float CapRingOrdinaryLimit;
            public float CapRingDepthLimit;
            public float CapRingEdgeLimit;
            public string CapRingWinningLimit = string.Empty;
            public float CapRingWearStrength;
            public float CapRingRequestedWidth;
            public Vector3 SelectedCornerLocalPosition;
            public Vector3[] CapVerticesLocal = Array.Empty<Vector3>();
            public float[] CapEdgeLengths = Array.Empty<float>();
            public int CapFaceCount;
            public int SemanticCapFaceCount;
            public int GeometryFaceCount;
            public int ConstructionSourceFaceCountExpected;
            public int ConstructionSourceFaceCountAttributed;
            public int ExpectedCapRingEdgeCount;
            public int MandatoryCandidateCount;
            public int MandatorySelectedCount;
            public int MandatoryBuiltCount;
            public int BaselineBuiltOrdinaryCount;
            public int UnrelatedBaselineBuiltCount;
            public int UnrelatedRetainedCount;
            public int CollateralLostCount;
            public int CandidateCount;
            public int ActiveEdgeCount;
            public int DeferredEdgeCount;
            public int RejectedEdgeCount;
            public int BevelFaceCount;
            public int TriangleCount;
            public double BaselineMilliseconds;
            public double CornerMilliseconds;
            public string Diagnostic = string.Empty;
            public string Report = string.Empty;
            public int[] AffectedOriginalEdgeIndices =
                Array.Empty<int>();
            public int[] MandatoryCapRingIdentities =
                Array.Empty<int>();
            public int[] CollateralLostIdentities =
                Array.Empty<int>();
            public EdgeWearDebugEdgeRecord[] DebugEdges =
                Array.Empty<EdgeWearDebugEdgeRecord>();
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
            SourceEdgeIndexDebug,
            CornerDamageTransactionAudit,
            CornerDamageGeometryPreview,
            CornerDamageIntegrationPreview
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

        public static string GenerateCornerDamageTransactionAudit(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            ResetCornerDamageTransactionAuditCapture();
            GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.CornerDamageTransactionAudit,
                -1,
                out _,
                out _,
                out _);
            return CompleteCornerDamageTransactionAuditCapture(recipe);
        }

        private static readonly float[] CornerDamageCapRingSearchScales =
        {
            1f,
            0.75f,
            0.5625f,
            0.421875f,
            0.31640625f,
            0.25f
        };

        public static MeshData GenerateCornerDamageGeometryPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            out CornerDamagePreviewStatus previewStatus)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }
            if (!surfaceFeatures.CornerChippingEnabled)
            {
                ResetCornerDamagePreviewCapture();
                previewStatus = BuildDisabledCornerDamagePreviewStatus(
                    recipe,
                    surfaceFeatures,
                    CornerDamagePreviewKind.GeometryOnly);
                return Generate(recipe, surfaceFeatures);
            }

            GenerateCornerDamageFullCertificationSearch(
                recipe,
                surfaceFeatures,
                out CornerDamagePreviewStatus certifiedStatus,
                out _);
            if (certifiedStatus == null ||
                !certifiedStatus.PreviewApplied ||
                certifiedStatus.AcceptedCornerRank < 0)
            {
                previewStatus = certifiedStatus;
                return Generate(recipe, surfaceFeatures);
            }

            ResetCornerDamagePreviewCapture();
            System.Diagnostics.Stopwatch geometryStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            MeshData geometryMeshData;
            UnifiedEdgeWearPreviewStatus geometryStatus;
            using (new CornerDamageSearchAttemptScope(
                       certifiedStatus.AcceptedCornerRank,
                       Mathf.Max(
                           0.0001f,
                           certifiedStatus.CapRingCommittedScale)))
            {
                geometryMeshData = GenerateInternal(
                    recipe,
                    surfaceFeatures,
                    EdgeWearEvaluationMode.CornerDamageGeometryPreview,
                    -1,
                    out _,
                    out _,
                    out geometryStatus);
            }
            geometryStopwatch.Stop();

            previewStatus = CompleteCornerDamagePreviewCapture(
                recipe,
                default,
                geometryStatus,
                0d,
                geometryStopwatch.Elapsed.TotalMilliseconds);
            ApplyCornerDamageSearchSummary(
                previewStatus,
                certifiedStatus.CandidateCornerCount,
                certifiedStatus.AttemptedCornerCount,
                certifiedStatus.AttemptedConfigurationCount,
                certifiedStatus.AcceptedCornerRank,
                certifiedStatus.CapRingCommittedScale,
                previewStatus != null && previewStatus.PreviewApplied
                    ? "none"
                    : "geometry-emission",
                previewStatus != null && previewStatus.PreviewApplied
                    ? "none"
                    : previewStatus == null
                        ? "corner geometry preview status was unavailable"
                        : previewStatus.Diagnostic,
                certifiedStatus.SearchAttemptSummary);
            if (previewStatus != null && previewStatus.PreviewApplied)
            {
                return geometryMeshData;
            }

            return Generate(recipe, surfaceFeatures);
        }

        public static MeshData GenerateCornerDamageIntegrationPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            out CornerDamagePreviewStatus previewStatus)
        {
            return GenerateCornerDamageIntegrationPreview(
                recipe,
                surfaceFeatures,
                out previewStatus,
                out _);
        }

        private static MeshData GenerateCornerDamageIntegrationPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }
            if (!surfaceFeatures.CornerChippingEnabled)
            {
                ResetCornerDamagePreviewCapture();
                previewStatus = BuildDisabledCornerDamagePreviewStatus(
                    recipe,
                    surfaceFeatures,
                    CornerDamagePreviewKind.WithEdgeWear);
                return GenerateUnifiedEdgeWearPreviewBaseline(
                    recipe,
                    surfaceFeatures,
                    out unifiedStatus);
            }

            return GenerateCornerDamageFullCertificationSearch(
                recipe,
                surfaceFeatures,
                out previewStatus,
                out unifiedStatus);
        }

        private static MeshData GenerateCornerDamageFullCertificationSearch(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            System.Diagnostics.Stopwatch baselineStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedBoundedPreview,
                -1,
                out _,
                out _,
                out UnifiedEdgeWearPreviewStatus baselineStatus);
            baselineStopwatch.Stop();

            int candidateCornerCount = -1;
            int attemptedCornerCount = 0;
            int attemptedConfigurationCount = 0;
            int bestFailurePriority = int.MinValue;
            CornerDamagePreviewStatus bestFailure = null;
            UnifiedEdgeWearPreviewStatus bestFailureUnified = default;
            string bestFailureStage = "candidate-availability";
            string bestFailureReason =
                "no eligible corner-damage candidate was available";
            StringBuilder searchAttempts = new StringBuilder(512);

            for (int candidateRank = 0;
                 candidateCornerCount < 0 ||
                 candidateRank < candidateCornerCount;
                 candidateRank++)
            {
                bool countedCorner = false;
                for (int scaleIndex = 0;
                     scaleIndex < CornerDamageCapRingSearchScales.Length;
                     scaleIndex++)
                {
                    float capRingScale =
                        CornerDamageCapRingSearchScales[scaleIndex];
                    ResetCornerDamagePreviewCapture();
                    System.Diagnostics.Stopwatch cornerStopwatch =
                        System.Diagnostics.Stopwatch.StartNew();
                    MeshData attemptMesh = null;
                    UnifiedEdgeWearPreviewStatus attemptUnified = default;
                    InvalidOperationException attemptException = null;
                    try
                    {
                        using (new CornerDamageSearchAttemptScope(
                                   candidateRank,
                                   capRingScale))
                        {
                            attemptMesh = GenerateInternal(
                                recipe,
                                surfaceFeatures,
                                EdgeWearEvaluationMode
                                    .CornerDamageIntegrationPreview,
                                -1,
                                out _,
                                out _,
                                out attemptUnified);
                        }
                    }
                    catch (InvalidOperationException exception)
                    {
                        attemptException = exception;
                    }
                    finally
                    {
                        cornerStopwatch.Stop();
                    }
                    CornerDamagePreviewStatus attemptStatus =
                        CompleteCornerDamagePreviewCapture(
                            recipe,
                            baselineStatus,
                            attemptUnified,
                            baselineStopwatch.Elapsed.TotalMilliseconds,
                            cornerStopwatch.Elapsed.TotalMilliseconds);
                    if (attemptException != null && attemptStatus != null)
                    {
                        attemptStatus.PreviewApplied = false;
                        attemptStatus.Diagnostic =
                            "post-chip construction exception: " +
                            attemptException.Message;
                    }
                    attemptedConfigurationCount++;
                    if (candidateCornerCount < 0)
                    {
                        candidateCornerCount = attemptStatus == null
                            ? 0
                            : attemptStatus.CandidateCornerCount;
                    }
                    if (!countedCorner &&
                        candidateRank < candidateCornerCount)
                    {
                        attemptedCornerCount++;
                        countedCorner = true;
                    }

                    string failureStage = attemptException == null
                        ? ResolveCornerDamageSearchFailureStage(
                            attemptStatus)
                        : "post-chip-construction";
                    AppendCornerDamageSearchAttempt(
                        searchAttempts,
                        candidateRank,
                        capRingScale,
                        failureStage,
                        attemptStatus);
                    if (attemptStatus != null &&
                        attemptStatus.PreviewApplied)
                    {
                        ApplyCornerDamageSearchSummary(
                            attemptStatus,
                            candidateCornerCount,
                            attemptedCornerCount,
                            attemptedConfigurationCount,
                            candidateRank,
                            capRingScale,
                            "none",
                            "none",
                            searchAttempts.ToString());
                        previewStatus = attemptStatus;
                        unifiedStatus = attemptUnified;
                        return attemptMesh;
                    }

                    int failurePriority =
                        ResolveCornerDamageSearchFailurePriority(
                            failureStage);
                    if (bestFailure == null ||
                        failurePriority > bestFailurePriority)
                    {
                        bestFailure = attemptStatus;
                        bestFailureUnified = attemptUnified;
                        bestFailurePriority = failurePriority;
                        bestFailureStage = failureStage;
                        bestFailureReason = attemptStatus == null
                            ? "corner search attempt status was unavailable"
                            : attemptStatus.Diagnostic;
                    }

                    if (attemptStatus == null ||
                        !attemptStatus.TransactionCertified ||
                        string.Equals(
                            attemptStatus.Diagnostic,
                            "cap-ring requested width is below the minimum stable style width",
                            StringComparison.Ordinal))
                    {
                        break;
                    }
                }

                if (candidateCornerCount <= 0)
                {
                    break;
                }
            }

            previewStatus = bestFailure ??
                new CornerDamagePreviewStatus
                {
                    PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                    ShapeSeed = recipe.ShapeSeed,
                    AuthoringEnabled = true,
                    Diagnostic =
                        "no eligible corner-damage candidate was available"
                };
            unifiedStatus = bestFailureUnified;
            string finalFailureStage = bestFailure == null
                ? ResolveCornerDamageSearchFailureStage(previewStatus)
                : bestFailureStage;
            string finalFailureReason = bestFailure == null
                ? previewStatus.Diagnostic
                : bestFailureReason;
            ApplyCornerDamageSearchSummary(
                previewStatus,
                Mathf.Max(0, candidateCornerCount),
                attemptedCornerCount,
                attemptedConfigurationCount,
                -1,
                0f,
                finalFailureStage,
                finalFailureReason,
                searchAttempts.ToString());
            return Generate(recipe, surfaceFeatures);
        }

        private static string ResolveCornerDamageSearchFailureStage(
            CornerDamagePreviewStatus status)
        {
            if (status == null || status.CandidateCornerCount <= 0)
            {
                return "candidate-availability";
            }
            if (!status.TransactionCertified)
            {
                return "transaction-certification";
            }
            if (status.ExpectedCapRingEdgeCount <= 0 ||
                status.MandatoryCandidateCount !=
                    status.ExpectedCapRingEdgeCount ||
                status.MandatorySelectedCount !=
                    status.ExpectedCapRingEdgeCount ||
                status.MandatoryBuiltCount !=
                    status.ExpectedCapRingEdgeCount)
            {
                return "cap-ring-completion";
            }
            if (status.CollateralLostCount > 0)
            {
                return "unrelated-retention";
            }
            if (!status.PreviewApplied)
            {
                return "post-chip-construction";
            }
            return "none";
        }

        private static int ResolveCornerDamageSearchFailurePriority(
            string failureStage)
        {
            return failureStage switch
            {
                "unrelated-retention" => 4,
                "post-chip-construction" => 3,
                "cap-ring-completion" => 2,
                "transaction-certification" => 1,
                _ => 0
            };
        }

        private static void AppendCornerDamageSearchAttempt(
            StringBuilder builder,
            int candidateRank,
            float capRingScale,
            string failureStage,
            CornerDamagePreviewStatus status)
        {
            if (builder.Length > 0)
            {
                builder.Append(';');
            }
            builder.Append('r');
            builder.Append(candidateRank);
            builder.Append('@');
            builder.Append(capRingScale.ToString(
                "G6",
                System.Globalization.CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(failureStage);
            builder.Append("[v=");
            builder.Append(status == null
                ? -1
                : status.SelectedGraphVertexIndex);
            builder.Append(",t=");
            builder.Append(status == null
                ? -1
                : status.AcceptedTrialIndex);
            builder.Append(",ring=");
            builder.Append(status == null
                ? 0
                : status.MandatoryBuiltCount);
            builder.Append('/');
            builder.Append(status == null
                ? 0
                : status.ExpectedCapRingEdgeCount);
            builder.Append(",lost=");
            builder.Append(status == null
                ? 0
                : status.CollateralLostCount);
            builder.Append(']');
        }

        public static MeshData GenerateCornerDamagePreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            out CornerDamagePreviewStatus previewStatus)
        {
            return GenerateCornerDamageIntegrationPreview(
                recipe,
                surfaceFeatures,
                out previewStatus);
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
            return GenerateUnifiedEdgeWearPreview(
                recipe,
                surfaceFeatures,
                out previewStatus,
                out _);
        }

        public static MeshData GenerateUnifiedEdgeWearPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            out UnifiedEdgeWearPreviewStatus previewStatus,
            out CornerDamagePreviewStatus cornerStatus)
        {
            if (surfaceFeatures.HasValue &&
                surfaceFeatures.Value.CornerChippingEnabled)
            {
                return GenerateCornerDamageIntegrationPreview(
                    recipe,
                    surfaceFeatures.Value,
                    out cornerStatus,
                    out previewStatus);
            }

            cornerStatus = null;
            return GenerateUnifiedEdgeWearPreviewBaseline(
                recipe,
                surfaceFeatures,
                out previewStatus);
        }

        public static MeshData GenerateUnifiedEdgeWearPreviewBaseline(
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
                    surfaceFeatures.EdgeWearMacroVariationCoverage,
                    surfaceFeatures.EdgeWearMacroVariation,
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
            List<Vector3> cornerDamageMarkerPositions =
                ExtractCornerDamagePreviewMarkerPositions();
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
            ApplyDimensions(cornerDamageMarkerPositions, dimensions);
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
            ApplyMassPlacementFrame(
                cornerDamageMarkerPositions,
                placementFrame);
            ApplyEdgeWearDebugPositions(
                unifiedPreviewStatus.DebugEdges,
                edgeDebugPositions);
            ApplyCornerDamagePreviewMarkerPositions(
                cornerDamageMarkerPositions);
            if (edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedBoundedPreview ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedBatchAudit ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.UnifiedPreviewBatchAudit ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageIntegrationPreview ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageGeometryPreview)
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
