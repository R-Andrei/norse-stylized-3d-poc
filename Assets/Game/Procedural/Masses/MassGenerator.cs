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

        [ThreadStatic]
        private static bool cornerDamageSearchDeadlineActive;

        [ThreadStatic]
        private static long cornerDamageSearchDeadlineTimestamp;

        private readonly struct CornerDamageSearchDeadlineScope : IDisposable
        {
            private readonly bool previousActive;
            private readonly long previousTimestamp;

            public CornerDamageSearchDeadlineScope(double budgetMilliseconds)
            {
                previousActive = cornerDamageSearchDeadlineActive;
                previousTimestamp = cornerDamageSearchDeadlineTimestamp;
                double clampedMilliseconds = Math.Max(0d, budgetMilliseconds);
                long budgetTicks = (long)Math.Ceiling(
                    clampedMilliseconds *
                    System.Diagnostics.Stopwatch.Frequency /
                    1000d);
                cornerDamageSearchDeadlineActive = true;
                cornerDamageSearchDeadlineTimestamp =
                    System.Diagnostics.Stopwatch.GetTimestamp() + budgetTicks;
            }

            public void Dispose()
            {
                cornerDamageSearchDeadlineActive = previousActive;
                cornerDamageSearchDeadlineTimestamp = previousTimestamp;
            }
        }

        [ThreadStatic]
        private static CornerDamageIntegrationPlan
            cornerDamageIntegrationPlanOverride;

        private readonly struct CornerDamageIntegrationPlanScope : IDisposable
        {
            private readonly CornerDamageIntegrationPlan previous;

            public CornerDamageIntegrationPlanScope(
                CornerDamageIntegrationPlan plan)
            {
                previous = cornerDamageIntegrationPlanOverride;
                cornerDamageIntegrationPlanOverride = plan;
            }

            public void Dispose()
            {
                cornerDamageIntegrationPlanOverride = previous;
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

        private static CornerDamageIntegrationPlan
            ResolveCornerDamageIntegrationPlanOverride()
        {
#if UNITY_EDITOR
            return cornerDamageIntegrationPlanOverride;
#else
            return null;
#endif
        }

        private static bool IsCornerDamageSearchDeadlineExceeded()
        {
#if UNITY_EDITOR
            return cornerDamageSearchDeadlineActive &&
                System.Diagnostics.Stopwatch.GetTimestamp() >=
                    cornerDamageSearchDeadlineTimestamp;
#else
            return false;
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
            public MeshData GeneratedOrdinaryBaselineMesh;
            public UnifiedEdgeWearPreviewStatus
                GeneratedOrdinaryBaselineStatus;
            public double GeneratedOrdinaryBaselineMilliseconds;
            public string GeneratedOrdinaryBaselineFingerprint = string.Empty;
            public string GeneratedOrdinaryBaselineDiagnostic = string.Empty;
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
            public int BaselineBuildCount;
            public int BaselineCacheUseCount;
            public int TransactionAttemptCount;
            public int IntegrationPreflightAttemptCount;
            public int FullIntegrationBuildCount;
            public int FullFallbackBuildCount;
            public int GeometrySearchReuseCount;
            public int IntegrationPreflightMismatchCount;
            public int IntegrationPlanAttemptCount;
            public int IntegrationPlanMismatchCount;
            public int AuthoritativeSolveAttemptCount;
            public int AuthoritativeSolveRejectCount;
            public int PlanMaterializationBuildCount;
            public int PlanMaterializationMismatchCount;
            public int DeadlineAbortCount;
            public int PreflightCandidateCount;
            public int PreflightSelectedCount;
            public int PreflightSelectedGraphEdgeCount;
            public bool PreflightCandidateConservationValid;
            public bool PreflightTopologyReady;
            public bool PreflightWidthSolutionReady;
            public int PreflightMandatorySolvedCount;
            public int PreflightUnrelatedBaselineCount;
            public int PreflightUnrelatedRetainedCount;
            public int PreflightCollateralLostCount;
            public string IntegrationPreflightDiagnostic = string.Empty;
            public string IntegrationPlanHash = string.Empty;
            public string EmittedPlanHash = string.Empty;
            public int[] PlannedOrdinaryIdentities = Array.Empty<int>();
            public int[] PlannedMandatoryIdentities = Array.Empty<int>();
            public int[] MissingPlannedOrdinary = Array.Empty<int>();
            public int[] UnexpectedFinalOrdinary = Array.Empty<int>();
            public int[] MissingPlannedMandatory = Array.Empty<int>();
            public int[] UnexpectedFinalMandatory = Array.Empty<int>();
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
            public double CandidateRankingMilliseconds;
            public double TransactionMilliseconds;
            public double IntegrationPreflightMilliseconds;
            public double IntegrationPlanMilliseconds;
            public double AuthoritativeSolveMilliseconds;
            public double PlanMaterializationMilliseconds;
            public double IntegrationMilliseconds;
            public bool CaseBudgetExceeded;
            public bool MatrixBudgetExceeded;
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

        public readonly struct CornerDamageSelectionFingerprint
        {
            public readonly bool Valid;
            public readonly int CandidateCornerCount;
            public readonly int SelectedCornerRank;
            public readonly int SelectedGraphVertexIndex;
            public readonly int AcceptedTrialIndex;
            public readonly float AcceptedDepth;
            public readonly int[] MandatoryCapRingIdentities;

            public CornerDamageSelectionFingerprint(
                bool valid,
                int candidateCornerCount,
                int selectedCornerRank,
                int selectedGraphVertexIndex,
                int acceptedTrialIndex,
                float acceptedDepth,
                int[] mandatoryCapRingIdentities)
            {
                Valid = valid;
                CandidateCornerCount = candidateCornerCount;
                SelectedCornerRank = selectedCornerRank;
                SelectedGraphVertexIndex = selectedGraphVertexIndex;
                AcceptedTrialIndex = acceptedTrialIndex;
                AcceptedDepth = acceptedDepth;
                MandatoryCapRingIdentities =
                    mandatoryCapRingIdentities ?? Array.Empty<int>();
            }
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
            CornerDamageIntegrationPreflight,
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

        private const double CornerDamageSearchHardBudgetMilliseconds = 5000d;

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
                false,
                default,
                0d,
                0d,
                CornerDamageSearchHardBudgetMilliseconds,
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
            CornerDamageSearchTelemetry telemetry =
                CopyCornerDamageSearchTelemetry(certifiedStatus);
            telemetry.GeometrySearchReuseCount++;
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
                certifiedStatus.SearchAttemptSummary,
                telemetry);
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

        public static MeshData
            GenerateCornerDamageIntegrationPreviewWithBaseline(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                UnifiedEdgeWearPreviewStatus baselineStatus,
                out CornerDamagePreviewStatus previewStatus,
                out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            return GenerateCornerDamageIntegrationPreviewWithBaseline(
                recipe,
                surfaceFeatures,
                baselineStatus,
                0d,
                0d,
                out previewStatus,
                out unifiedStatus);
        }

        public static MeshData
            GenerateCornerDamageIntegrationPreviewWithBaseline(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                UnifiedEdgeWearPreviewStatus baselineStatus,
                double baselineBuildMilliseconds,
                double estimatedIntegrationMilliseconds,
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
                unifiedStatus = baselineStatus;
                return Generate(recipe, surfaceFeatures);
            }

            return GenerateCornerDamageFullCertificationSearch(
                recipe,
                surfaceFeatures,
                true,
                baselineStatus,
                baselineBuildMilliseconds,
                estimatedIntegrationMilliseconds,
                CornerDamageSearchHardBudgetMilliseconds,
                out previewStatus,
                out unifiedStatus);
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
                false,
                default,
                0d,
                0d,
                CornerDamageSearchHardBudgetMilliseconds,
                out previewStatus,
                out unifiedStatus);
        }

        public static CornerDamageSelectionFingerprint
            GenerateCornerDamageSelectionFingerprint(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                int candidateRank)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            ResetCornerDamageTransactionAuditCapture();
            using (new CornerDamageSearchAttemptScope(candidateRank, 1f))
            {
                GenerateInternal(
                    recipe,
                    surfaceFeatures,
                    EdgeWearEvaluationMode.CornerDamageTransactionAudit,
                    -1,
                    out _,
                    out _,
                    out _);
            }
            CornerDamageTransactionAuditResult transaction =
                CompleteCornerDamageTransactionAuditResultCapture();
            if (transaction == null)
            {
                return default;
            }

            List<int> mandatory = new List<int>(
                transaction.CapRingGeneratedIdentities);
            mandatory.Sort();
            return new CornerDamageSelectionFingerprint(
                transaction.Succeeded,
                transaction.EligibleCandidateCount,
                transaction.SelectedCandidateRank,
                transaction.SelectedGraphVertexIndex,
                transaction.AcceptedTrialIndex,
                transaction.AcceptedDepth,
                mandatory.ToArray());
        }

        private static MeshData GenerateCornerDamageFullCertificationSearch(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            bool useProvidedBaseline,
            UnifiedEdgeWearPreviewStatus providedBaselineStatus,
            double providedBaselineMilliseconds,
            double estimatedIntegrationMilliseconds,
            double hardBudgetMilliseconds,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            CornerDamageSearchTelemetry telemetry =
                new CornerDamageSearchTelemetry();
            using CornerDamageSearchDeadlineScope deadlineScope =
                new CornerDamageSearchDeadlineScope(hardBudgetMilliseconds);
            System.Diagnostics.Stopwatch searchStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            UnifiedEdgeWearPreviewStatus baselineStatus;
            double baselineMilliseconds = 0d;
            if (useProvidedBaseline)
            {
                baselineStatus = providedBaselineStatus;
                baselineMilliseconds = Mathf.Max(
                    0f,
                    (float)providedBaselineMilliseconds);
                telemetry.BaselineCacheUseCount = 1;
            }
            else
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
                    out baselineStatus);
                baselineStopwatch.Stop();
                baselineMilliseconds =
                    baselineStopwatch.Elapsed.TotalMilliseconds;
                telemetry.BaselineBuildCount = 1;
            }

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
            CornerDamageIntegrationPreflightRecord acceptedPreflight = null;
            CornerDamageIntegrationPlan acceptedPlan = null;
            int acceptedCandidateRank = -1;

            for (int candidateRank = 0;
                 candidateCornerCount < 0 ||
                 candidateRank < candidateCornerCount;
                 candidateRank++)
            {
                if (IsEdgeWearAuditCancellationRequested())
                {
                    bestFailureStage = "cancelled";
                    bestFailureReason = "cancelled by user";
                    break;
                }
                if (searchStopwatch.Elapsed.TotalMilliseconds >=
                    hardBudgetMilliseconds)
                {
                    telemetry.CaseBudgetExceeded = true;
                    bestFailureStage = "performance-budget";
                    bestFailureReason =
                        "corner preflight exceeded the case hard budget";
                    break;
                }

                telemetry.TransactionAttemptCount++;
                telemetry.IntegrationPreflightAttemptCount++;
                System.Diagnostics.Stopwatch preflightStopwatch =
                    System.Diagnostics.Stopwatch.StartNew();
                CornerDamageIntegrationPreflightRecord preflight = null;
                InvalidOperationException preflightException = null;
                try
                {
                    ResetCornerDamageIntegrationPreflightCapture();
                    using (new CornerDamageSearchAttemptScope(
                               candidateRank,
                               1f))
                    {
                        GenerateInternal(
                            recipe,
                            surfaceFeatures,
                            EdgeWearEvaluationMode.
                                CornerDamageIntegrationPreflight,
                            -1,
                            out _,
                            out _,
                            out _);
                    }
                    preflight =
                        CompleteCornerDamageIntegrationPreflightCapture();
                }
                catch (InvalidOperationException exception)
                {
                    preflightException = exception;
                }
                finally
                {
                    preflightStopwatch.Stop();
                }

                CornerDamageTransactionAuditResult transaction =
                    preflight == null ? null : preflight.Transaction;
                if (transaction != null)
                {
                    telemetry.CandidateRankingMilliseconds +=
                        transaction.CandidateRankingMilliseconds;
                    telemetry.TransactionMilliseconds += Mathf.Max(
                        0f,
                        (float)(transaction.TransactionMilliseconds -
                            transaction.CandidateRankingMilliseconds));
                }
                telemetry.IntegrationPreflightMilliseconds += Mathf.Max(
                    0f,
                    (float)(preflightStopwatch.Elapsed.TotalMilliseconds -
                        (transaction == null
                            ? 0d
                            : transaction.TransactionMilliseconds)));

                if (candidateCornerCount < 0)
                {
                    candidateCornerCount = transaction == null
                        ? 0
                        : transaction.EligibleCandidateCount;
                }
                if (candidateRank < candidateCornerCount)
                {
                    attemptedCornerCount++;
                }

                CornerDamagePreviewStatus preflightStatus =
                    BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        preflight,
                        preflightException == null
                            ? string.Empty
                            : "integration preflight exception: " +
                                preflightException.Message);
                string preflightStage = preflightException != null
                    ? "integration-preflight"
                    : preflight == null
                        ? "integration-preflight"
                        : preflight.FailureStage;
                if (string.IsNullOrEmpty(preflightStage))
                {
                    preflightStage = "integration-preflight";
                }

                if (preflightException == null && preflight != null &&
                    string.Equals(
                        preflight.FailureStage,
                        "none",
                        StringComparison.Ordinal))
                {
                    ApplyCornerDamageIntegrationPreflightRetention(
                        preflight,
                        baselineStatus);
                    if (preflight.PredictedCollateralLostCount > 0)
                    {
                        preflight.FailureStage = "unrelated-retention";
                        preflight.Diagnostic =
                            "integration preflight predicts unrelated bevel loss";
                        preflightStatus = BuildCornerDamagePreflightStatus(
                            recipe,
                            surfaceFeatures,
                            preflight,
                            preflight.Diagnostic);
                        preflightStage = preflight.FailureStage;
                    }
                    else
                    {
                        telemetry.IntegrationPlanAttemptCount++;
                        telemetry.AuthoritativeSolveAttemptCount++;
                        bool solveBuilt =
                            TrySolveCornerDamageIntegrationPlan(
                                recipe,
                                preflight,
                                baselineStatus,
                                out CornerDamageIntegrationPlan candidatePlan,
                                out double solveMilliseconds);
                        telemetry.IntegrationPlanMilliseconds +=
                            solveMilliseconds;
                        telemetry.AuthoritativeSolveMilliseconds +=
                            solveMilliseconds;
                        if (solveBuilt && candidatePlan != null &&
                            candidatePlan.Valid)
                        {
                            acceptedPreflight = preflight;
                            acceptedPlan = candidatePlan;
                            acceptedCandidateRank = candidateRank;
                            ApplyCornerDamageIntegrationPlanEvidence(
                                preflightStatus,
                                candidatePlan);
                            AppendCornerDamageSearchAttempt(
                                searchAttempts,
                                candidateRank,
                                preflight.ResolvedUniformScale,
                                "authoritative-solve-certified",
                                preflightStatus);
                            break;
                        }

                        telemetry.AuthoritativeSolveRejectCount++;
                        if (IsCornerDamageSearchDeadlineExceeded())
                        {
                            telemetry.DeadlineAbortCount++;
                        }
                        string solveDiagnostic = candidatePlan == null
                            ? "authoritative solved plan was unavailable"
                            : candidatePlan.Diagnostic;
                        preflightStatus.PreviewApplied = false;
                        preflightStatus.Diagnostic = solveDiagnostic;
                        ApplyCornerDamageIntegrationPlanEvidence(
                            preflightStatus,
                            candidatePlan);
                        AppendCornerDamageSearchAttempt(
                            searchAttempts,
                            candidateRank,
                            preflight.ResolvedUniformScale,
                            "authoritative-solve",
                            preflightStatus);
                        RetainCornerDamageSearchFailure(
                            preflightStatus,
                            default,
                            "authoritative-solve",
                            ref bestFailure,
                            ref bestFailureUnified,
                            ref bestFailurePriority,
                            ref bestFailureStage,
                            ref bestFailureReason);
                        continue;
                    }
                }

                AppendCornerDamageSearchAttempt(
                    searchAttempts,
                    candidateRank,
                    preflight == null
                        ? 0f
                        : preflight.ResolvedUniformScale,
                    preflightStage,
                    preflightStatus);
                RetainCornerDamageSearchFailure(
                    preflightStatus,
                    default,
                    preflightStage,
                    ref bestFailure,
                    ref bestFailureUnified,
                    ref bestFailurePriority,
                    ref bestFailureStage,
                    ref bestFailureReason);
            }

            if (acceptedPreflight == null || acceptedPlan == null)
            {
                searchStopwatch.Stop();
                previewStatus = bestFailure ??
                    new CornerDamagePreviewStatus
                    {
                        PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                        ShapeSeed = recipe.ShapeSeed,
                        AuthoringEnabled = true,
                        Diagnostic = bestFailureReason
                    };
                unifiedStatus = bestFailureUnified;
                ApplyCornerDamageSearchSummary(
                    previewStatus,
                    Mathf.Max(0, candidateCornerCount),
                    attemptedCornerCount,
                    attemptedConfigurationCount,
                    -1,
                    0f,
                    bestFailureStage,
                    bestFailureReason,
                    searchAttempts.ToString(),
                    telemetry);
                return Generate(recipe, surfaceFeatures);
            }

            double remainingBudget = hardBudgetMilliseconds -
                searchStopwatch.Elapsed.TotalMilliseconds;
            const double MinimumPlanMaterializationBudgetMilliseconds = 250d;
            if (remainingBudget + 0.001d <
                MinimumPlanMaterializationBudgetMilliseconds)
            {
                telemetry.CaseBudgetExceeded = true;
                telemetry.DeadlineAbortCount++;
                CornerDamagePreviewStatus budgetStatus =
                    BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        acceptedPreflight,
                        "insufficient case budget remains to materialize the authoritative solved plan");
                ApplyCornerDamageIntegrationPlanEvidence(
                    budgetStatus,
                    acceptedPlan);
                ApplyCornerDamageSearchSummary(
                    budgetStatus,
                    Mathf.Max(0, candidateCornerCount),
                    attemptedCornerCount,
                    0,
                    -1,
                    0f,
                    "performance-budget",
                    budgetStatus.Diagnostic,
                    searchAttempts.ToString(),
                    telemetry);
                previewStatus = budgetStatus;
                unifiedStatus = default;
                return Generate(recipe, surfaceFeatures);
            }

            telemetry.PlanMaterializationBuildCount = 1;
            attemptedConfigurationCount = 1;
            bool materialized = TryMaterializeCornerDamageIntegrationPlan(
                acceptedPlan,
                acceptedPreflight.ExpectedMandatoryCount,
                out double materializationMilliseconds,
                out bool materializationIdentityMismatch);
            telemetry.PlanMaterializationMilliseconds +=
                materializationMilliseconds;
            telemetry.IntegrationMilliseconds =
                materializationMilliseconds;
            if (!materialized)
            {
                if (materializationIdentityMismatch)
                {
                    telemetry.PlanMaterializationMismatchCount++;
                    telemetry.IntegrationPlanMismatchCount++;
                }
                bool deadlineExceeded =
                    IsCornerDamageSearchDeadlineExceeded() ||
                    searchStopwatch.Elapsed.TotalMilliseconds >=
                        hardBudgetMilliseconds;
                if (deadlineExceeded)
                {
                    telemetry.CaseBudgetExceeded = true;
                    telemetry.DeadlineAbortCount++;
                }
                CornerDamagePreviewStatus materializationStatus =
                    BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        acceptedPreflight,
                        acceptedPlan.Diagnostic);
                ApplyCornerDamageIntegrationPlanEvidence(
                    materializationStatus,
                    acceptedPlan);
                string materializationStage = deadlineExceeded
                    ? "performance-budget"
                    : materializationIdentityMismatch
                        ? "plan-materialization-mismatch"
                        : "plan-materialization";
                AppendCornerDamageSearchAttempt(
                    searchAttempts,
                    acceptedCandidateRank,
                    acceptedPreflight.ResolvedUniformScale,
                    materializationStage,
                    materializationStatus);
                ApplyCornerDamageSearchSummary(
                    materializationStatus,
                    candidateCornerCount,
                    attemptedCornerCount,
                    attemptedConfigurationCount,
                    -1,
                    0f,
                    materializationStage,
                    materializationStatus.Diagnostic,
                    searchAttempts.ToString(),
                    telemetry);
                previewStatus = materializationStatus;
                unifiedStatus = default;
                return Generate(recipe, surfaceFeatures);
            }

            telemetry.FullIntegrationBuildCount = 1;
            telemetry.GeometrySearchReuseCount++;
            MeshData finalMesh = RunCornerDamageIntegrationPlanEmission(
                recipe,
                surfaceFeatures,
                acceptedPlan,
                baselineStatus,
                baselineMilliseconds,
                out CornerDamagePreviewStatus finalStatus,
                out UnifiedEdgeWearPreviewStatus finalUnified,
                out double finalMilliseconds,
                out InvalidOperationException finalException);
            telemetry.IntegrationMilliseconds += finalMilliseconds;
            ApplyCornerDamageIntegrationPreflightEvidence(
                finalStatus,
                acceptedPreflight);
            ApplyCornerDamageIntegrationPlanEvidence(
                finalStatus,
                acceptedPlan);

            bool exceededBudget =
                searchStopwatch.Elapsed.TotalMilliseconds >=
                    hardBudgetMilliseconds;
            string finalFailureStage = finalException == null
                ? ResolveCornerDamageSearchFailureStage(finalStatus)
                : "plan-emission";
            string mismatchReason = string.Empty;
            bool planMatchesFinal = finalException == null &&
                finalStatus != null && finalStatus.PreviewApplied &&
                TryValidateCornerDamageIntegrationPlanEmission(
                    acceptedPlan,
                    finalStatus,
                    finalUnified,
                    out mismatchReason);
            ApplyCornerDamageIntegrationPlanEvidence(
                finalStatus,
                acceptedPlan);
            if (!planMatchesFinal && !exceededBudget)
            {
                telemetry.IntegrationPlanMismatchCount = 1;
                finalFailureStage = "integration-plan-mismatch";
                if (finalStatus == null)
                {
                    finalStatus = BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        acceptedPreflight,
                        mismatchReason);
                }
                finalStatus.PreviewApplied = false;
                finalStatus.Diagnostic = string.IsNullOrEmpty(mismatchReason)
                    ? "emitted shell disagreed with the committed integration plan"
                    : mismatchReason;
                ApplyCornerDamageIntegrationPlanEvidence(
                    finalStatus,
                    acceptedPlan);
            }
            if (exceededBudget)
            {
                telemetry.CaseBudgetExceeded = true;
                finalFailureStage = "performance-budget";
                if (finalStatus != null)
                {
                    finalStatus.PreviewApplied = false;
                    finalStatus.Diagnostic =
                        "authoritative solve, plan materialization, and emission exceeded the case hard budget";
                }
            }

            AppendCornerDamageSearchAttempt(
                searchAttempts,
                acceptedCandidateRank,
                acceptedPreflight.ResolvedUniformScale,
                finalFailureStage,
                finalStatus);
            searchStopwatch.Stop();
            if (!exceededBudget && planMatchesFinal)
            {
                ApplyCornerDamageSearchSummary(
                    finalStatus,
                    candidateCornerCount,
                    attemptedCornerCount,
                    attemptedConfigurationCount,
                    acceptedCandidateRank,
                    acceptedPreflight.ResolvedUniformScale,
                    "none",
                    "none",
                    searchAttempts.ToString(),
                    telemetry);
                previewStatus = finalStatus;
                unifiedStatus = finalUnified;
                return finalMesh;
            }

            if (finalStatus == null)
            {
                finalStatus = BuildCornerDamagePreflightStatus(
                    recipe,
                    surfaceFeatures,
                    acceptedPreflight,
                    finalException == null
                        ? mismatchReason
                        : "planned shell emission exception: " +
                            finalException.Message);
                ApplyCornerDamageIntegrationPlanEvidence(
                    finalStatus,
                    acceptedPlan);
            }
            ApplyCornerDamageSearchSummary(
                finalStatus,
                candidateCornerCount,
                attemptedCornerCount,
                attemptedConfigurationCount,
                -1,
                0f,
                finalFailureStage,
                finalStatus.Diagnostic,
                searchAttempts.ToString(),
                telemetry);
            previewStatus = finalStatus;
            unifiedStatus = finalUnified;
            return Generate(recipe, surfaceFeatures);
        }

        private static void ApplyCornerDamageIntegrationPreflightEvidence(
            CornerDamagePreviewStatus status,
            CornerDamageIntegrationPreflightRecord preflight)
        {
            if (status == null || preflight == null)
            {
                return;
            }

            status.PreflightCandidateCount = preflight.CandidateCount;
            status.PreflightSelectedCount = preflight.SelectedCount;
            status.PreflightSelectedGraphEdgeCount =
                preflight.SelectedGraphEdgeCount;
            status.PreflightCandidateConservationValid =
                preflight.CandidateConservationValid;
            status.PreflightTopologyReady = preflight.TopologyReady;
            status.PreflightWidthSolutionReady =
                preflight.WidthSolutionReady;
            status.PreflightMandatorySolvedCount =
                preflight.MandatorySolvedCount;
            status.PreflightUnrelatedBaselineCount =
                preflight.PredictedUnrelatedBaselineCount;
            status.PreflightUnrelatedRetainedCount =
                preflight.PredictedUnrelatedRetainedCount;
            status.PreflightCollateralLostCount =
                preflight.PredictedCollateralLostCount;
            status.IntegrationPreflightDiagnostic =
                preflight.Diagnostic ?? string.Empty;
        }

        private static void ApplyCornerDamageIntegrationPreflightRetention(
            CornerDamageIntegrationPreflightRecord preflight,
            UnifiedEdgeWearPreviewStatus baselineStatus)
        {
            if (preflight == null)
            {
                return;
            }

            HashSet<int> baseline = CollectCertifiedOrdinaryEdgeIdentities(
                baselineStatus.DebugEdges);
            HashSet<int> predicted = new HashSet<int>(
                preflight.PredictedOrdinaryIdentities ??
                    Array.Empty<int>());
            HashSet<int> affected = preflight.Transaction == null
                ? new HashSet<int>()
                : new HashSet<int>(
                    preflight.Transaction.AffectedOriginalEdgeIndices);
            List<int> lost = new List<int>();
            foreach (int identity in baseline)
            {
                if (affected.Contains(identity))
                {
                    continue;
                }
                preflight.PredictedUnrelatedBaselineCount++;
                if (predicted.Contains(identity))
                {
                    preflight.PredictedUnrelatedRetainedCount++;
                }
                else
                {
                    lost.Add(identity);
                }
            }
            lost.Sort();
            preflight.PredictedCollateralLostIdentities = lost.ToArray();
            preflight.PredictedCollateralLostCount = lost.Count;
        }

        private static bool TrySolveCornerDamageIntegrationPlan(
            MassRecipe recipe,
            CornerDamageIntegrationPreflightRecord preflight,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            out CornerDamageIntegrationPlan plan,
            out double elapsedMilliseconds)
        {
            elapsedMilliseconds = 0d;
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            plan = new CornerDamageIntegrationPlan
            {
                Transaction = preflight == null
                    ? null
                    : preflight.Transaction,
                ResolvedUniformScale = preflight == null
                    ? 0f
                    : preflight.ResolvedUniformScale,
                OrdinaryRequestedWidth = preflight == null
                    ? 0f
                    : preflight.RequestedOrdinaryWidth,
                CapRingOrdinaryLimit = preflight == null
                    ? 0f
                    : preflight.CapRingOrdinaryLimit,
                CapRingDepthLimit = preflight == null
                    ? 0f
                    : preflight.CapRingDepthLimit,
                CapRingEdgeLimit = preflight == null
                    ? 0f
                    : preflight.CapRingEdgeLimit,
                CapRingWinningLimit = preflight == null
                    ? string.Empty
                    : preflight.CapRingWinningLimit,
                CapRingWearStrength = preflight == null
                    ? 0f
                    : preflight.CapRingWearStrength,
                CapRingRequestedWidth = preflight == null
                    ? 0f
                    : preflight.RequestedRingWidth
            };

            try
            {
                if (recipe == null || preflight == null ||
                    preflight.Transaction == null ||
                    preflight.PreparedFaces == null ||
                    preflight.PreparedContext == null ||
                    preflight.PreparedSolution == null ||
                    preflight.PreparedCoverage == null)
                {
                    plan.Diagnostic =
                        "prepared corner-integration state was unavailable";
                    return false;
                }
                if (IsCornerDamageSearchDeadlineExceeded())
                {
                    plan.Diagnostic =
                        "corner search deadline exceeded before authoritative solve";
                    return false;
                }

                EdgeWearCoverageAudit coverage =
                    preflight.PreparedCoverage.CloneForTrial();
                PlaneCutBevelAuditResult audit = SolvePlaneCutBevelKernel(
                    preflight.PreparedFaces,
                    preflight.PreparedContext,
                    preflight.PreparedSolution,
                    preflight.MinimumStableEdgeLength,
                    preflight.MinimumStableFaceArea,
                    coverage,
                    false,
                    out PlaneCutBevelSolvedPlan solvedPlan);
                EdgeWearCoverageAudit effectiveCoverage =
                    solvedPlan == null
                        ? audit.CoverageAudit ?? coverage
                        : solvedPlan.CoverageAudit ?? coverage;
                int[] ordinary = CollectCornerDamageSolvedPlanIdentities(
                    solvedPlan,
                    effectiveCoverage,
                    false);
                int[] mandatory = CollectCornerDamageSolvedPlanIdentities(
                    solvedPlan,
                    effectiveCoverage,
                    true);
                HashSet<int> baseline =
                    CollectCertifiedOrdinaryEdgeIdentities(
                        baselineStatus.DebugEdges);
                HashSet<int> affected = new HashSet<int>(
                    preflight.Transaction.AffectedOriginalEdgeIndices);
                HashSet<int> ordinarySet = new HashSet<int>(ordinary);
                List<int> lost = new List<int>();
                int unrelatedBaselineCount = 0;
                int unrelatedRetainedCount = 0;
                foreach (int identity in baseline)
                {
                    if (affected.Contains(identity))
                    {
                        continue;
                    }
                    unrelatedBaselineCount++;
                    if (ordinarySet.Contains(identity))
                    {
                        unrelatedRetainedCount++;
                    }
                    else
                    {
                        lost.Add(identity);
                    }
                }
                lost.Sort();

                plan.SolvedPlan = solvedPlan;
                plan.PlaneAudit = audit;
                plan.PlannedOrdinaryIdentities = ordinary;
                plan.PlannedMandatoryIdentities = mandatory;
                plan.UnrelatedBaselineCount = unrelatedBaselineCount;
                plan.UnrelatedRetainedCount = unrelatedRetainedCount;
                plan.CollateralLostCount = lost.Count;
                plan.CollateralLostIdentities = lost.ToArray();
                plan.IntegrationPlanHash =
                    BuildCornerDamageIntegrationPlanHash(plan);
                plan.EmittedPlanHash = string.Empty;

                if (solvedPlan == null ||
                    !solvedPlan.SolveValid)
                {
                    plan.Diagnostic = string.IsNullOrEmpty(audit.Diagnostic)
                        ? "authoritative bevel solve did not certify"
                        : audit.Diagnostic;
                    return false;
                }
                if (mandatory.Length != preflight.ExpectedMandatoryCount)
                {
                    plan.Diagnostic =
                        "authoritative solve has an incomplete mandatory cap ring";
                    return false;
                }
                if (lost.Count > 0)
                {
                    plan.Diagnostic =
                        "authoritative solve loses unrelated baseline bevel identities";
                    return false;
                }

                plan.Valid = true;
                plan.Diagnostic =
                    "authoritative corner-integration solve certified";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                plan.Diagnostic =
                    "authoritative solve exception: " +
                    exception.Message;
                return false;
            }
            finally
            {
                stopwatch.Stop();
                elapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private static int[] CollectCornerDamageSolvedPlanIdentities(
            PlaneCutBevelSolvedPlan solvedPlan,
            EdgeWearCoverageAudit coverage,
            bool mandatory)
        {
            if (solvedPlan == null ||
                solvedPlan.RetainedCandidates == null ||
                coverage == null)
            {
                return Array.Empty<int>();
            }
            HashSet<int> retainedGraphEdges = new HashSet<int>();
            for (int index = 0;
                 index < solvedPlan.RetainedCandidates.Count;
                 index++)
            {
                retainedGraphEdges.Add(
                    solvedPlan.RetainedCandidates[index].SourceEdgeIndex);
            }
            SortedSet<int> identities = new SortedSet<int>();
            for (int index = 0; index < coverage.Records.Count; index++)
            {
                EdgeWearEdgeLifecycleRecord record = coverage.Records[index];
                if (record == null || record.Mandatory != mandatory ||
                    record.OriginalSourceEdgeIndex < 0 ||
                    !retainedGraphEdges.Contains(record.SourceEdgeIndex))
                {
                    continue;
                }
                identities.Add(record.OriginalSourceEdgeIndex);
            }
            int[] result = new int[identities.Count];
            identities.CopyTo(result);
            return result;
        }

        private static bool TryMaterializeCornerDamageIntegrationPlan(
            CornerDamageIntegrationPlan plan,
            int expectedMandatoryCount,
            out double elapsedMilliseconds,
            out bool identityMismatch)
        {
            elapsedMilliseconds = 0d;
            identityMismatch = false;
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (plan == null || !plan.Valid || plan.SolvedPlan == null)
                {
                    if (plan != null)
                    {
                        plan.Diagnostic =
                            "authoritative solved plan was unavailable";
                        plan.Valid = false;
                    }
                    return false;
                }
                if (IsCornerDamageSearchDeadlineExceeded())
                {
                    plan.Diagnostic =
                        "corner search deadline exceeded before plan materialization";
                    plan.Valid = false;
                    return false;
                }

                PlaneCutBevelAuditResult audit =
                    MaterializePlaneCutBevelSolvedPlan(
                        plan.SolvedPlan,
                        out TriangleSoup previewSoup);
                EdgeWearCoverageAudit effectiveCoverage =
                    audit.CoverageAudit ??
                    plan.SolvedPlan.CoverageAudit;
                bool previewApplied = audit.GeometryValid == 1 &&
                    previewSoup != null &&
                    plan.SolvedPlan.Materialized;
                UnifiedEdgeWearPreviewStatus unifiedStatus =
                    new UnifiedEdgeWearPreviewStatus(
                        previewApplied,
                        audit.SelectedEdgeCount,
                        audit.ActiveEdgeCount,
                        audit.PlanesBuilt,
                        audit.PlanesDeferred,
                        audit.PlanesRejected,
                        audit.BevelRegionFaceCount,
                        0,
                        audit.PreviewTriangleCount,
                        audit.Diagnostic,
                        BuildUnifiedEdgeWearDebugEdges(
                            plan.SolvedPlan.Context,
                            effectiveCoverage,
                            audit.DebugFocusEdgeIndices));

                int[] finalOrdinary = CollectCornerDamagePlanIdentities(
                    effectiveCoverage,
                    false);
                int[] finalMandatory = CollectCornerDamagePlanIdentities(
                    effectiveCoverage,
                    true);
                plan.MissingPlannedOrdinary =
                    ResolveCornerDamageIdentityDifference(
                        plan.PlannedOrdinaryIdentities,
                        finalOrdinary);
                plan.UnexpectedFinalOrdinary =
                    ResolveCornerDamageIdentityDifference(
                        finalOrdinary,
                        plan.PlannedOrdinaryIdentities);
                plan.MissingPlannedMandatory =
                    ResolveCornerDamageIdentityDifference(
                        plan.PlannedMandatoryIdentities,
                        finalMandatory);
                plan.UnexpectedFinalMandatory =
                    ResolveCornerDamageIdentityDifference(
                        finalMandatory,
                        plan.PlannedMandatoryIdentities);
                plan.EmittedPlanHash = BuildCornerDamageIntegrationPlanHash(
                    plan.Transaction,
                    plan.ResolvedUniformScale,
                    finalOrdinary,
                    finalMandatory);
                identityMismatch =
                    plan.MissingPlannedOrdinary.Length != 0 ||
                    plan.UnexpectedFinalOrdinary.Length != 0 ||
                    plan.MissingPlannedMandatory.Length != 0 ||
                    plan.UnexpectedFinalMandatory.Length != 0 ||
                    !string.Equals(
                        plan.IntegrationPlanHash,
                        plan.EmittedPlanHash,
                        StringComparison.Ordinal);

                plan.PreviewSoup = previewSoup;
                plan.UnifiedStatus = unifiedStatus;
                plan.PlaneAudit = audit;
                if (!previewApplied)
                {
                    plan.Diagnostic = string.IsNullOrEmpty(audit.Diagnostic)
                        ? "authoritative plan materialization failed"
                        : audit.Diagnostic;
                    plan.Valid = false;
                    return false;
                }
                if (finalMandatory.Length != expectedMandatoryCount)
                {
                    plan.Diagnostic =
                        "materialized mandatory cap ring is incomplete";
                    plan.Valid = false;
                    return false;
                }
                if (identityMismatch)
                {
                    plan.Diagnostic =
                        "materialized shell differs from the authoritative solved plan";
                    plan.Valid = false;
                    return false;
                }

                plan.Valid = true;
                plan.Diagnostic =
                    "authoritative corner-integration plan materialized";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                if (plan != null)
                {
                    plan.Valid = false;
                    plan.Diagnostic =
                        "plan materialization exception: " +
                        exception.Message;
                }
                return false;
            }
            finally
            {
                stopwatch.Stop();
                elapsedMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private static int[] CollectCornerDamagePlanIdentities(
            EdgeWearCoverageAudit coverage,
            bool mandatory)
        {
            if (coverage == null)
            {
                return Array.Empty<int>();
            }
            SortedSet<int> identities = new SortedSet<int>();
            for (int index = 0; index < coverage.Records.Count; index++)
            {
                EdgeWearEdgeLifecycleRecord record = coverage.Records[index];
                if (record == null || record.Mandatory != mandatory ||
                    !record.Built || record.OriginalSourceEdgeIndex < 0)
                {
                    continue;
                }
                identities.Add(record.OriginalSourceEdgeIndex);
            }
            int[] result = new int[identities.Count];
            identities.CopyTo(result);
            return result;
        }

        private static TriangleSoup CloneCornerDamageIntegrationPlanSoup(
            TriangleSoup source)
        {
            if (source == null)
            {
                return null;
            }
            TriangleSoup clone = new TriangleSoup();
            for (int vertexIndex = 0;
                 vertexIndex + 2 < source.Positions.Count;
                 vertexIndex += 3)
            {
                source.TryResolveAuthoredSurfaceNormal(
                    vertexIndex,
                    out Vector3 normal);
                source.TryResolveAuthoredSurfaceGroup(
                    vertexIndex,
                    out int surfaceGroup);
                clone.AddTriangle(
                    source.Positions[vertexIndex],
                    source.Positions[vertexIndex + 1],
                    source.Positions[vertexIndex + 2],
                    source.ResolveFeature(vertexIndex),
                    source.ResolveFeatureStrength(vertexIndex),
                    normal,
                    surfaceGroup);
            }
            return clone;
        }

        private static string BuildCornerDamageIntegrationPlanHash(
            CornerDamageIntegrationPlan plan)
        {
            return plan == null
                ? string.Empty
                : BuildCornerDamageIntegrationPlanHash(
                    plan.Transaction,
                    plan.ResolvedUniformScale,
                    plan.PlannedOrdinaryIdentities,
                    plan.PlannedMandatoryIdentities);
        }

        private static string BuildCornerDamageIntegrationPlanHash(
            CornerDamageTransactionAuditResult transaction,
            float resolvedUniformScale,
            int[] ordinaryIdentities,
            int[] mandatoryIdentities)
        {
            if (transaction == null)
            {
                return string.Empty;
            }
            ulong hash = 1469598103934665603UL;
            AppendCornerDamagePlanHashValue(
                ref hash,
                transaction.SelectedCandidateRank);
            AppendCornerDamagePlanHashValue(
                ref hash,
                transaction.SelectedGraphVertexIndex);
            AppendCornerDamagePlanHashValue(
                ref hash,
                transaction.AcceptedTrialIndex);
            AppendCornerDamagePlanHashValue(
                ref hash,
                BitConverter.SingleToInt32Bits(
                    transaction.AcceptedDepth));
            AppendCornerDamagePlanHashValue(
                ref hash,
                BitConverter.SingleToInt32Bits(
                    resolvedUniformScale));
            AppendCornerDamagePlanHashValues(
                ref hash,
                ordinaryIdentities);
            AppendCornerDamagePlanHashValues(
                ref hash,
                mandatoryIdentities);
            return hash.ToString("X16");
        }

        private static void AppendCornerDamagePlanHashValues(
            ref ulong hash,
            int[] values)
        {
            values ??= Array.Empty<int>();
            AppendCornerDamagePlanHashValue(ref hash, values.Length);
            for (int index = 0; index < values.Length; index++)
            {
                AppendCornerDamagePlanHashValue(ref hash, values[index]);
            }
        }

        private static void AppendCornerDamagePlanHashValue(
            ref ulong hash,
            int value)
        {
            unchecked
            {
                uint encoded = (uint)value;
                for (int byteIndex = 0; byteIndex < 4; byteIndex++)
                {
                    hash ^= (byte)(encoded >> (byteIndex * 8));
                    hash *= 1099511628211UL;
                }
            }
        }

        private static void ApplyCornerDamageIntegrationPlanEvidence(
            CornerDamagePreviewStatus status,
            CornerDamageIntegrationPlan plan)
        {
            if (status == null || plan == null)
            {
                return;
            }
            status.IntegrationPlanHash = plan.IntegrationPlanHash;
            status.EmittedPlanHash = plan.EmittedPlanHash;
            status.PlannedOrdinaryIdentities =
                plan.PlannedOrdinaryIdentities ?? Array.Empty<int>();
            status.PlannedMandatoryIdentities =
                plan.PlannedMandatoryIdentities ?? Array.Empty<int>();
            status.MissingPlannedOrdinary =
                plan.MissingPlannedOrdinary ?? Array.Empty<int>();
            status.UnexpectedFinalOrdinary =
                plan.UnexpectedFinalOrdinary ?? Array.Empty<int>();
            status.MissingPlannedMandatory =
                plan.MissingPlannedMandatory ?? Array.Empty<int>();
            status.UnexpectedFinalMandatory =
                plan.UnexpectedFinalMandatory ?? Array.Empty<int>();
        }

        private static bool TryValidateCornerDamageIntegrationPlanEmission(
            CornerDamageIntegrationPlan plan,
            CornerDamagePreviewStatus finalStatus,
            UnifiedEdgeWearPreviewStatus finalUnified,
            out string blocker)
        {
            blocker = string.Empty;
            if (plan == null || !plan.Valid || finalStatus == null)
            {
                blocker = "integration plan or emitted status was unavailable";
                return false;
            }
            HashSet<int> finalOrdinarySet =
                CollectCertifiedOrdinaryEdgeIdentities(
                    finalUnified.DebugEdges);
            int[] finalOrdinary = new int[finalOrdinarySet.Count];
            finalOrdinarySet.CopyTo(finalOrdinary);
            Array.Sort(finalOrdinary);
            int[] sourceMandatory = finalStatus.MandatoryCapRingIdentities ??
                Array.Empty<int>();
            int[] finalMandatory = new int[sourceMandatory.Length];
            Array.Copy(
                sourceMandatory,
                finalMandatory,
                sourceMandatory.Length);
            Array.Sort(finalMandatory);
            plan.MissingPlannedOrdinary = ResolveCornerDamageIdentityDifference(
                plan.PlannedOrdinaryIdentities,
                finalOrdinary);
            plan.UnexpectedFinalOrdinary = ResolveCornerDamageIdentityDifference(
                finalOrdinary,
                plan.PlannedOrdinaryIdentities);
            plan.MissingPlannedMandatory = ResolveCornerDamageIdentityDifference(
                plan.PlannedMandatoryIdentities,
                finalMandatory);
            plan.UnexpectedFinalMandatory = ResolveCornerDamageIdentityDifference(
                finalMandatory,
                plan.PlannedMandatoryIdentities);
            plan.EmittedPlanHash = BuildCornerDamageIntegrationPlanHash(
                plan.Transaction,
                plan.ResolvedUniformScale,
                finalOrdinary,
                finalMandatory);
            bool matches = plan.MissingPlannedOrdinary.Length == 0 &&
                plan.UnexpectedFinalOrdinary.Length == 0 &&
                plan.MissingPlannedMandatory.Length == 0 &&
                plan.UnexpectedFinalMandatory.Length == 0 &&
                string.Equals(
                    plan.IntegrationPlanHash,
                    plan.EmittedPlanHash,
                    StringComparison.Ordinal);
            if (!matches)
            {
                blocker = "emitted shell differs from the committed integration plan";
            }
            return matches;
        }

        private static int[] ResolveCornerDamageIdentityDifference(
            int[] left,
            int[] right)
        {
            SortedSet<int> result = new SortedSet<int>(
                left ?? Array.Empty<int>());
            result.ExceptWith(right ?? Array.Empty<int>());
            int[] values = new int[result.Count];
            result.CopyTo(values);
            return values;
        }

        private static MeshData RunCornerDamageIntegrationPlanEmission(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            CornerDamageIntegrationPlan plan,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            double baselineMilliseconds,
            out CornerDamagePreviewStatus attemptStatus,
            out UnifiedEdgeWearPreviewStatus attemptUnified,
            out double elapsedMilliseconds,
            out InvalidOperationException attemptException)
        {
            ResetCornerDamagePreviewCapture();
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            MeshData attemptMesh = null;
            attemptUnified = default;
            attemptException = null;
            try
            {
                using (new CornerDamageSearchAttemptScope(
                           plan.Transaction.SelectedCandidateRank,
                           plan.ResolvedUniformScale))
                using (new CornerDamageIntegrationPlanScope(plan))
                {
                    attemptMesh = GenerateInternal(
                        recipe,
                        surfaceFeatures,
                        EdgeWearEvaluationMode.CornerDamageIntegrationPreview,
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
                stopwatch.Stop();
            }

            elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;
            attemptStatus = CompleteCornerDamagePreviewCapture(
                recipe,
                baselineStatus,
                attemptUnified,
                baselineMilliseconds,
                elapsedMilliseconds);
            ApplyCornerDamageIntegrationPlanEvidence(attemptStatus, plan);
            if (attemptException != null && attemptStatus != null)
            {
                attemptStatus.PreviewApplied = false;
                attemptStatus.Diagnostic =
                    "planned shell emission exception: " +
                    attemptException.Message;
            }
            return attemptMesh;
        }

        private static CornerDamagePreviewStatus
            BuildCornerDamagePreflightStatus(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                CornerDamageIntegrationPreflightRecord preflight,
                string overrideDiagnostic)
        {
            CornerDamageTransactionAuditResult transaction =
                preflight == null ? null : preflight.Transaction;
            CornerDamagePreviewStatus status =
                new CornerDamagePreviewStatus
                {
                    PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                    ShapeSeed = recipe == null ? 0 : recipe.ShapeSeed,
                    AuthoringEnabled = surfaceFeatures.CornerChippingEnabled,
                    TransactionCertified = transaction != null &&
                        transaction.Succeeded,
                    CandidateCornerCount = transaction == null
                        ? 0
                        : transaction.EligibleCandidateCount,
                    AcceptedCornerRank = transaction == null
                        ? -1
                        : transaction.SelectedCandidateRank,
                    SelectedGraphVertexIndex = transaction == null
                        ? -1
                        : transaction.SelectedGraphVertexIndex,
                    AcceptedTrialIndex = transaction == null
                        ? -1
                        : transaction.AcceptedTrialIndex,
                    AcceptedDepth = transaction == null
                        ? 0f
                        : transaction.AcceptedDepth,
                    ExpectedCapRingEdgeCount = preflight == null
                        ? 0
                        : preflight.ExpectedMandatoryCount,
                    MandatoryCandidateCount = preflight == null
                        ? 0
                        : preflight.MandatoryRecordCount,
                    MandatorySelectedCount = preflight == null
                        ? 0
                        : preflight.MandatorySelectedCount,
                    MandatoryBuiltCount = preflight == null
                        ? 0
                        : preflight.MandatorySolvedCount,
                    UnrelatedBaselineBuiltCount = preflight == null
                        ? 0
                        : preflight.PredictedUnrelatedBaselineCount,
                    UnrelatedRetainedCount = preflight == null
                        ? 0
                        : preflight.PredictedUnrelatedRetainedCount,
                    CollateralLostCount = preflight == null
                        ? 0
                        : preflight.PredictedCollateralLostCount,
                    PreflightCandidateCount = preflight == null
                        ? 0
                        : preflight.CandidateCount,
                    PreflightSelectedCount = preflight == null
                        ? 0
                        : preflight.SelectedCount,
                    PreflightSelectedGraphEdgeCount = preflight == null
                        ? 0
                        : preflight.SelectedGraphEdgeCount,
                    PreflightCandidateConservationValid = preflight != null &&
                        preflight.CandidateConservationValid,
                    PreflightTopologyReady = preflight != null &&
                        preflight.TopologyReady,
                    PreflightWidthSolutionReady = preflight != null &&
                        preflight.WidthSolutionReady,
                    PreflightMandatorySolvedCount = preflight == null
                        ? 0
                        : preflight.MandatorySolvedCount,
                    PreflightUnrelatedBaselineCount = preflight == null
                        ? 0
                        : preflight.PredictedUnrelatedBaselineCount,
                    PreflightUnrelatedRetainedCount = preflight == null
                        ? 0
                        : preflight.PredictedUnrelatedRetainedCount,
                    PreflightCollateralLostCount = preflight == null
                        ? 0
                        : preflight.PredictedCollateralLostCount,
                    IntegrationPreflightDiagnostic = preflight == null
                        ? string.Empty
                        : preflight.Diagnostic,
                    Diagnostic = string.IsNullOrEmpty(overrideDiagnostic)
                        ? preflight == null
                            ? "integration preflight status was unavailable"
                            : preflight.Diagnostic
                        : overrideDiagnostic
                };
            if (transaction != null)
            {
                status.RequestedDepthFraction =
                    transaction.RequestedDepthFraction;
                status.DepthVariation = transaction.DepthVariation;
                status.DepthVariationIdentity =
                    transaction.DepthVariationIdentity;
                status.ResolvedDepthFraction =
                    transaction.ResolvedDepthFraction;
                status.TopFacingPreference = transaction.TopFacingPreference;
                status.ShortestIncidentEdgeLength =
                    transaction.ShortestIncidentEdgeLength;
                status.RequestedDepthAbsolute = transaction.BaseDepth;
                status.AcceptedDepthFraction =
                    transaction.ShortestIncidentEdgeLength >
                        PointMergeDistance
                        ? transaction.AcceptedDepth /
                            transaction.ShortestIncidentEdgeLength
                        : 0f;
                status.AcceptedRetryFactor =
                    transaction.AcceptedRetryFactor;
                status.AcceptedVsRequestedRatio =
                    transaction.BaseDepth > PointMergeDistance
                        ? transaction.AcceptedDepth /
                            transaction.BaseDepth
                        : 0f;
                status.CapFaceCount = transaction.AcceptedCapFace == null
                    ? 0
                    : 1;
                List<int> mandatory = new List<int>(
                    transaction.CapRingGeneratedIdentities);
                mandatory.Sort();
                status.MandatoryCapRingIdentities = mandatory.ToArray();
            }
            return status;
        }

        private static void RetainCornerDamageSearchFailure(
            CornerDamagePreviewStatus attemptStatus,
            UnifiedEdgeWearPreviewStatus attemptUnified,
            string failureStage,
            ref CornerDamagePreviewStatus bestFailure,
            ref UnifiedEdgeWearPreviewStatus bestFailureUnified,
            ref int bestFailurePriority,
            ref string bestFailureStage,
            ref string bestFailureReason)
        {
            int priority = ResolveCornerDamageSearchFailurePriority(
                failureStage);
            if (bestFailure != null && priority <= bestFailurePriority)
            {
                return;
            }
            bestFailure = attemptStatus;
            bestFailureUnified = attemptUnified;
            bestFailurePriority = priority;
            bestFailureStage = failureStage;
            bestFailureReason = attemptStatus == null
                ? "corner search attempt status was unavailable"
                : !string.IsNullOrEmpty(
                      attemptStatus.SearchFailureReason) &&
                  !string.Equals(
                      attemptStatus.SearchFailureReason,
                      "none",
                      StringComparison.Ordinal)
                    ? attemptStatus.SearchFailureReason
                    : attemptStatus.Diagnostic;
        }

        private static CornerDamageSearchTelemetry
            CopyCornerDamageSearchTelemetry(
                CornerDamagePreviewStatus status)
        {
            return new CornerDamageSearchTelemetry
            {
                BaselineBuildCount = status == null
                    ? 0
                    : status.BaselineBuildCount,
                BaselineCacheUseCount = status == null
                    ? 0
                    : status.BaselineCacheUseCount,
                TransactionAttemptCount = status == null
                    ? 0
                    : status.TransactionAttemptCount,
                IntegrationPreflightAttemptCount = status == null
                    ? 0
                    : status.IntegrationPreflightAttemptCount,
                FullIntegrationBuildCount = status == null
                    ? 0
                    : status.FullIntegrationBuildCount,
                FullFallbackBuildCount = status == null
                    ? 0
                    : status.FullFallbackBuildCount,
                GeometrySearchReuseCount = status == null
                    ? 0
                    : status.GeometrySearchReuseCount,
                IntegrationPreflightMismatchCount = status == null
                    ? 0
                    : status.IntegrationPreflightMismatchCount,
                IntegrationPlanAttemptCount = status == null
                    ? 0
                    : status.IntegrationPlanAttemptCount,
                IntegrationPlanMismatchCount = status == null
                    ? 0
                    : status.IntegrationPlanMismatchCount,
                AuthoritativeSolveAttemptCount = status == null
                    ? 0
                    : status.AuthoritativeSolveAttemptCount,
                AuthoritativeSolveRejectCount = status == null
                    ? 0
                    : status.AuthoritativeSolveRejectCount,
                PlanMaterializationBuildCount = status == null
                    ? 0
                    : status.PlanMaterializationBuildCount,
                PlanMaterializationMismatchCount = status == null
                    ? 0
                    : status.PlanMaterializationMismatchCount,
                DeadlineAbortCount = status == null
                    ? 0
                    : status.DeadlineAbortCount,
                CandidateRankingMilliseconds = status == null
                    ? 0d
                    : status.CandidateRankingMilliseconds,
                TransactionMilliseconds = status == null
                    ? 0d
                    : status.TransactionMilliseconds,
                IntegrationPreflightMilliseconds = status == null
                    ? 0d
                    : status.IntegrationPreflightMilliseconds,
                IntegrationPlanMilliseconds = status == null
                    ? 0d
                    : status.IntegrationPlanMilliseconds,
                AuthoritativeSolveMilliseconds = status == null
                    ? 0d
                    : status.AuthoritativeSolveMilliseconds,
                PlanMaterializationMilliseconds = status == null
                    ? 0d
                    : status.PlanMaterializationMilliseconds,
                IntegrationMilliseconds = status == null
                    ? 0d
                    : status.IntegrationMilliseconds,
                CaseBudgetExceeded = status != null &&
                    status.CaseBudgetExceeded,
                MatrixBudgetExceeded = status != null &&
                    status.MatrixBudgetExceeded
            };
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
                "performance-budget" => 9,
                "integration-plan-mismatch" => 9,
                "integration-plan" => 8,
                "integration-preflight-mismatch" => 8,
                "unrelated-retention" => 7,
                "post-chip-construction" => 6,
                "width-solution" => 5,
                "candidate-conservation" => 4,
                "topology-context" => 4,
                "cap-ring-completion" => 3,
                "cap-ring-preflight" => 2,
                "integration-preflight" => 2,
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

        public static MeshData GenerateUnifiedEdgeWearPreviewWithBaseline(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            MeshData baselineMesh,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            out UnifiedEdgeWearPreviewStatus previewStatus,
            out CornerDamagePreviewStatus cornerStatus)
        {
            return GenerateUnifiedEdgeWearPreviewWithBaseline(
                recipe,
                surfaceFeatures,
                baselineMesh,
                baselineStatus,
                0d,
                0d,
                out previewStatus,
                out cornerStatus);
        }

        public static MeshData GenerateUnifiedEdgeWearPreviewWithBaseline(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            MeshData baselineMesh,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            double baselineBuildMilliseconds,
            double estimatedIntegrationMilliseconds,
            out UnifiedEdgeWearPreviewStatus previewStatus,
            out CornerDamagePreviewStatus cornerStatus)
        {
            if (surfaceFeatures.CornerChippingEnabled)
            {
                return GenerateCornerDamageIntegrationPreviewWithBaseline(
                    recipe,
                    surfaceFeatures,
                    baselineStatus,
                    baselineBuildMilliseconds,
                    estimatedIntegrationMilliseconds,
                    out cornerStatus,
                    out previewStatus);
            }

            cornerStatus = null;
            previewStatus = baselineStatus;
            return baselineMesh;
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
            return GenerateUnifiedEdgeWearBatchAuditCase(
                recipe,
                surfaceFeatures,
                false);
        }

        public static EdgeWearBatchAuditCaseResult
            GenerateUnifiedEdgeWearBatchAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                bool captureOrdinaryBaseline)
        {
            return GenerateUnifiedEdgeWearBatchAuditCase(
                recipe,
                surfaceFeatures,
                captureOrdinaryBaseline
                    ? surfaceFeatures
                    : (MassSurfaceFeatureSettings?)null);
        }

        public static EdgeWearBatchAuditCaseResult
            GenerateUnifiedEdgeWearBatchAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                MassSurfaceFeatureSettings? ordinaryBaselineSettings)
        {
            return GenerateEdgeWearBatchAuditCase(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedBatchAudit,
                true,
                ordinaryBaselineSettings);
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
                false,
                null);
        }

        private static EdgeWearBatchAuditCaseResult
            GenerateEdgeWearBatchAuditCase(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                EdgeWearEvaluationMode evaluationMode,
                bool requireAllGeometricCandidates,
                MassSurfaceFeatureSettings? ordinaryBaselineSettings)
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

            EdgeWearBatchAuditCaseResult result =
                CompleteEdgeWearBatchAuditCapture(
                    stopwatch.Elapsed.TotalMilliseconds,
                    evaluationException);
            if (result != null && result.Passed &&
                ordinaryBaselineSettings.HasValue)
            {
                System.Diagnostics.Stopwatch baselineStopwatch =
                    System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    result.GeneratedOrdinaryBaselineMesh = GenerateInternal(
                        recipe,
                        ordinaryBaselineSettings.Value,
                        EdgeWearEvaluationMode.UnifiedBoundedPreview,
                        -1,
                        out _,
                        out _,
                        out UnifiedEdgeWearPreviewStatus baselineStatus);
                    result.GeneratedOrdinaryBaselineStatus = baselineStatus;
                }
                catch (Exception exception)
                {
                    result.GeneratedOrdinaryBaselineDiagnostic =
                        exception.GetType().Name + ":" + exception.Message;
                }
                finally
                {
                    baselineStopwatch.Stop();
                    result.GeneratedOrdinaryBaselineMilliseconds =
                        baselineStopwatch.Elapsed.TotalMilliseconds;
                }
            }
            return result;
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
#if UNITY_EDITOR
            if (edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageIntegrationPreflight ||
                edgeWearEvaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageTransactionAudit)
            {
                return null;
            }
#endif
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
