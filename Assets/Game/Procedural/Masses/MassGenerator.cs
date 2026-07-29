using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        public enum CornerDamageRecoveryTournamentStrategy
        {
            RemoteComponentConforming,
            RemoteComponentFixedPoint,
            RemoteComponentSimpleCycleFixedPoint,
            RemoteComponentSourceStripsFixedPoint,
            RemoteComponentHalfEdgeFixedPoint,
            RemoteComponentCellFanFixedPoint,
            RemoteComponentAxialTransitionFixedPoint,
            RemoteComponentTaperTransitionFixedPoint,
            RemoteComponentRawEdgeFanFixedPoint,
            OwnLimitFixedPoint,
            OwnLimitSimpleCycleFixedPoint,
            OwnLimitSourceStripsFixedPoint,
            OwnLimitHalfEdgeFixedPoint,
            OwnLimitCellFanFixedPoint,
            OwnLimitRawEdgeFanFixedPoint,
            OwnLimitAxialTransitionFixedPoint,
            OwnLimitTaperTransitionFixedPoint,
            WidthPreconditionedRemoteComponentFixedPoint,
            SingleBandSuppressionFixedPoint,
            AllButOneBandSuppressionFixedPoint,
            AllBandSuppressionFixedPoint,
            GeometricCellRemoteComponentFixedPoint,
            GeometricCellRemoteComponentSimpleCycleFixedPoint,
            GeometricCellRemoteComponentSourceStripsFixedPoint,
            GeometricCellRemoteComponentHalfEdgeFixedPoint,
            GeometricCellRemoteComponentCellFanFixedPoint,
            GeometricCellRemoteComponentRawEdgeFanFixedPoint,
            GeometricCellRemoteComponentAxialTransitionFixedPoint,
            GeometricCellRemoteComponentTaperTransitionFixedPoint,
            LegacyBoundedEndpointCell
        }

        public readonly struct CornerDamageRecoveryTournamentConfiguration
        {
            public readonly CornerDamageRecoveryTournamentStrategy Strategy;
            public readonly int VariantIndex;
            public readonly float PrimaryParameter;
            public readonly float SecondaryParameter;
            public readonly double CaseBudgetMilliseconds;
            public readonly string Name;

            public CornerDamageRecoveryTournamentConfiguration(
                CornerDamageRecoveryTournamentStrategy strategy,
                int variantIndex,
                float primaryParameter,
                float secondaryParameter,
                double caseBudgetMilliseconds,
                string name)
            {
                Strategy = strategy;
                VariantIndex = Mathf.Max(0, variantIndex);
                PrimaryParameter = primaryParameter;
                SecondaryParameter = secondaryParameter;
                CaseBudgetMilliseconds = Math.Max(100d, caseBudgetMilliseconds);
                Name = name ?? strategy.ToString();
            }
        }

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
        private static CornerDamagePreflightReplayCache
            cornerDamagePreflightReplayCache;

        private readonly struct CornerDamagePreflightReplayScope : IDisposable
        {
            private readonly CornerDamagePreflightReplayCache previous;

            public CornerDamagePreflightReplayScope(
                CornerDamagePreflightReplayCache cache)
            {
                previous = cornerDamagePreflightReplayCache;
                cornerDamagePreflightReplayCache = cache;
            }

            public void Dispose()
            {
                cornerDamagePreflightReplayCache = previous;
            }
        }

        [ThreadStatic]
        private static CornerDamageIntegrationPlan
            cornerDamageIntegrationPlanOverride;

        [ThreadStatic]
        private static bool cornerDamageRecoveryTournamentActive;

        [ThreadStatic]
        private static CornerDamageRecoveryTournamentConfiguration
            cornerDamageRecoveryTournamentConfiguration;

        private readonly struct CornerDamageRecoveryTournamentScope : IDisposable
        {
            private readonly bool previousActive;
            private readonly CornerDamageRecoveryTournamentConfiguration
                previousConfiguration;

            public CornerDamageRecoveryTournamentScope(
                CornerDamageRecoveryTournamentConfiguration configuration)
            {
                previousActive = cornerDamageRecoveryTournamentActive;
                previousConfiguration =
                    cornerDamageRecoveryTournamentConfiguration;
                cornerDamageRecoveryTournamentActive = true;
                cornerDamageRecoveryTournamentConfiguration = configuration;
            }

            public void Dispose()
            {
                cornerDamageRecoveryTournamentActive = previousActive;
                cornerDamageRecoveryTournamentConfiguration =
                    previousConfiguration;
            }
        }

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

        private static bool IsCornerDamageRecoveryTournamentActive()
        {
#if UNITY_EDITOR
            return cornerDamageRecoveryTournamentActive;
#else
            return false;
#endif
        }

        private static CornerDamageRecoveryTournamentConfiguration
            ResolveCornerDamageRecoveryTournamentConfiguration()
        {
#if UNITY_EDITOR
            return cornerDamageRecoveryTournamentConfiguration;
#else
            return default;
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
            public int SelectionArchitectureParityCaptured;
            public int SelectionArchitectureParityValid;
            public int SelectionArchitectureLifecycleCandidateCount;
            public int SelectionArchitectureReturnedCandidateCount;
            public string SelectionArchitectureDiagnostic = string.Empty;
            public int FullRebuildOracleCaptured;
            public int FullRebuildOracleValid;
            public int FullRebuildOraclePrimaryCandidateCount;
            public int FullRebuildOracleRebuildCandidateCount;
            public int FullRebuildOraclePrimaryLifecycleCount;
            public int FullRebuildOracleRebuildLifecycleCount;
            public int FullRebuildOracleCandidateMismatchCount;
            public int FullRebuildOracleLifecycleMismatchCount;
            public string FullRebuildOracleDiagnostic = string.Empty;
            public int IsolatedEligibilityCaptured;
            public int IsolatedEligibilityValid;
            public int IsolatedEligibilityLifecycleCount;
            public int IsolatedEligibilityStructuralCount;
            public int IsolatedEligibilityGeometricCount;
            public int IsolatedEligibilityWidthEvidenceCount;
            public int IsolatedEligibilityWidthFeasibleCount;
            public int IsolatedEligibilityCertifiedCount;
            public int IsolatedEligibilityMissingEvidenceCount;
            public int IsolatedEligibilityInvalidIntervalCount;
            public int IsolatedEligibilityInconsistentCount;
            public float IsolatedEligibilityMinimumCertifiedWidth;
            public float IsolatedEligibilityMaximumCertifiedWidth;
            public string IsolatedEligibilityProblematicEdges = string.Empty;
            public string IsolatedEligibilityDiagnostic = string.Empty;
            public int PotentialInteractionCaptured;
            public int PotentialInteractionValid;
            public int PotentialInteractionCandidateCount;
            public int PotentialInteractionTotalPairCount;
            public int PotentialInteractionPotentialPairCount;
            public int PotentialInteractionDisjointPairCount;
            public int PotentialInteractionSharedEndpointCount;
            public int PotentialInteractionSharedFaceCount;
            public int PotentialInteractionExpandedBoundsCount;
            public int PotentialInteractionMissingEvidenceCount;
            public int PotentialInteractionDuplicatePairCount;
            public int PotentialInteractionMaximumDegree;
            public string PotentialInteractionSamplePairs = string.Empty;
            public string PotentialInteractionProblematicCandidates = string.Empty;
            public string PotentialInteractionDiagnostic = string.Empty;
            public int PairwiseCompatibilityCaptured;
            public int PairwiseCompatibilityValid;
            public int PairwiseCompatibilityPotentialPairs;
            public int PairwiseCompatibilityEvaluatedPairs;
            public int PairwiseCompatibilityCompatiblePairs;
            public int PairwiseCompatibilityIncompatiblePairs;
            public int PairwiseCompatibilityUnresolvedPairs;
            public int PairwiseCompatibilityMissingRelations;
            public int PairwiseCompatibilityDuplicateRelations;
            public float PairwiseCompatibilityMinimumClearance;
            public string PairwiseCompatibilityIncompatibleEvidence = string.Empty;
            public string PairwiseCompatibilityUnresolvedEvidence = string.Empty;
            public string PairwiseCompatibilityDiagnostic = string.Empty;
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
            public int EndpointConflictGuardAttemptCount;
            public int EndpointConflictGuardPassCount;
            public int EndpointConflictGuardRejectCount;
            public int EndpointConflictGuardFalseNegativeCount;
            public int EndpointConflictGuardTestedRailCount;
            public double EndpointConflictGuardMilliseconds;
            public int EndpointPatchRecoveryAttemptCount;
            public int EndpointPatchRecoveryPreparedCount;
            public int EndpointPatchRecoveryRejectCount;
            public int EndpointPatchRecoveryAppliedCount;
            public int EndpointPatchRecoveryFalsePositiveCount;
            public int EndpointPatchRecoveryUnsupportedStarCount;
            public int EndpointPatchRecoveryPatchExtractionCount;
            public int EndpointPatchRecoveryDisconnectedPatchCount;
            public int EndpointPatchRecoveryBoundaryLoopCount;
            public int EndpointPatchRecoveryBoundaryCrossingCount;
            public int EndpointPatchRecoveryNoLocalRemovalCount;
            public int EndpointPatchRecoveryCapCreationCount;
            public int EndpointPatchRecoveryIncidentBandJoinCount;
            public int EndpointPatchRecoveryStitchTopologyCount;
            public int EndpointPatchRecoveryLocalityCount;
            public int EndpointPatchRecoveryBandIntegrityCount;
            public int EndpointPatchRecoveryPreparedMinimumParityCount;
            public int EndpointPatchRecoveryMaterializationSignatureCount;
            public float EndpointPatchRecoveryMaximumRemovedVertexRadius;
            public float EndpointPatchRecoveryMaximumIntersectionRadius;
            public float EndpointPatchRecoveryMaximumReplacementVertexRadius;
            public int EndpointPatchRecoveryRetainedOutsideRadiusCount;
            public int EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter;
            public int EndpointPatchRecoverySelectedFaceCountAfterLocalFilter;
            public int EndpointPatchRecoveryLocalSupportSampleCount;
            public int EndpointPatchRecoveryMinimumSamplesPerIncident;
            public float EndpointPatchRecoveryMaximumGlobalMinusLocalSupportDelta;
            public float EndpointPatchRecoveryMaximumControllingSupportRadius;
            public float EndpointPatchRecoveryMaximumAxialInfluence;
            public float EndpointPatchRecoveryMinimumAllowedAxialInfluence;
            public int EndpointPatchRecoveryFacesSubdivided;
            public int EndpointPatchRecoveryLocalFragmentCount;
            public int EndpointPatchRecoveryRemoteRemainderCount;
            public int EndpointPatchRecoverySyntheticIncidentFragmentCount;
            public int EndpointPatchRecoveryMaximumCellVertexCount;
            public int EndpointPatchRecoveryMaximumCellFaceCount;
            public double EndpointPatchRecoveryMilliseconds;
            public int PreflightFoundationBuildCount;
            public int PreflightFoundationReuseCount;
            public int IsolatedReplayAttemptCount;
            public int IsolatedReplayHitCount;
            public int IsolatedReplayMissCount;
            public int IsolatedFullEvaluationCount;
            public bool EndpointConflictGuardAttempted;
            public bool EndpointConflictGuardPassed;
            public int EndpointConflictGuardConflictCount;
            public int EndpointConflictGuardVictimEdgeIndex = -1;
            public int EndpointConflictGuardForeignEdgeIndex = -1;
            public float EndpointConflictGuardAxialParameter;
            public float EndpointConflictGuardEndpointAllowance;
            public float EndpointConflictGuardVictimMinimumScale;
            public float EndpointConflictGuardForeignMinimumScale;
            public float EndpointConflictGuardVictimRetreatCapacity;
            public float EndpointConflictGuardForeignRetreatCapacity;
            public int[] EndpointConflictGuardClusterEdges = Array.Empty<int>();
            public bool EndpointConflictGuardFalseNegative;
            public string EndpointConflictGuardDiagnostic = string.Empty;
            public bool EndpointPatchRecoveryAttempted;
            public bool EndpointPatchRecoveryPrepared;
            public bool EndpointPatchRecoveryApplied;
            public bool EndpointPatchRecoveryFalsePositive;
            public int EndpointPatchRecoveryLocalAttemptCount;
            public int EndpointPatchRecoveryVertexIndex = -1;
            public int EndpointPatchRecoveryVictimEdgeIndex = -1;
            public int EndpointPatchRecoveryForeignEdgeIndex = -1;
            public int EndpointPatchRecoveryIncidentBandCount;
            public int EndpointPatchRecoveryNormalRank = -1;
            public int EndpointPatchRecoveryCapVertexCount;
            public float EndpointPatchRecoveryCutDepth;
            public float EndpointPatchRecoveryCompactness;
            public float EndpointPatchRecoveryAspectRatio;
            public double EndpointPatchRecoveryLocalMilliseconds;
            public string EndpointPatchRecoveryRejection = string.Empty;
            public int EndpointPatchRecoverySelectedFaceCount;
            public int EndpointPatchRecoveryBoundaryVertexCount;
            public string EndpointPatchRecoveryBoundarySignature = string.Empty;
            public float EndpointPatchRecoveryLocalMaximumRemovedVertexRadius;
            public float EndpointPatchRecoveryLocalMaximumIntersectionRadius;
            public float EndpointPatchRecoveryLocalMaximumReplacementVertexRadius;
            public int EndpointPatchRecoveryLocalRetainedOutsideRadiusCount;
            public int EndpointPatchRecoveryLocalSelectedFaceCountBeforeLocalFilter;
            public int EndpointPatchRecoveryLocalSelectedFaceCountAfterLocalFilter;
            public string EndpointPatchRecoveryLocalityFailureSource = string.Empty;
            public int EndpointPatchRecoveryAttemptSupportSampleCount;
            public int EndpointPatchRecoveryAttemptMinimumSamplesPerIncident;
            public string EndpointPatchRecoverySamplesPerIncident = string.Empty;
            public float EndpointPatchRecoveryLocalSupportRadius;
            public float EndpointPatchRecoveryLocalSupportProjection;
            public float EndpointPatchRecoveryGlobalSupportProjection;
            public float EndpointPatchRecoveryGlobalMinusLocalSupportDelta;
            public int EndpointPatchRecoveryControllingSupportEdgeIndex = -1;
            public float EndpointPatchRecoveryControllingSupportRadius;
            public string EndpointPatchRecoverySupportFailureSource = string.Empty;
            public float EndpointPatchRecoveryAttemptMaximumAxialInfluence;
            public float EndpointPatchRecoveryAttemptMinimumAllowedAxialInfluence;
            public int EndpointPatchRecoveryAxialRejectedEdgeIndex = -1;
            public int EndpointPatchRecoveryAxialRejectedEndpointVertexIndex = -1;
            public string EndpointPatchRecoveryAxialInfluenceSignature = string.Empty;
            public string EndpointPatchRecoveryCellLimitSignature = string.Empty;
            public int EndpointPatchRecoveryAttemptFacesSubdivided;
            public int EndpointPatchRecoveryAttemptLocalFragmentCount;
            public int EndpointPatchRecoveryAttemptRemoteRemainderCount;
            public int EndpointPatchRecoveryAttemptSyntheticIncidentFragmentCount;
            public string EndpointPatchRecoverySyntheticIncidentIdentities = string.Empty;
            public int EndpointPatchRecoveryAttemptCellVertexCount;
            public int EndpointPatchRecoveryAttemptCellFaceCount;
            public string EndpointPatchRecoveryCellSplitSignature = string.Empty;
            public string EndpointPatchRecoveryLocalFragmentSignature = string.Empty;
            public string EndpointPatchRecoveryRemoteRemainderSignature = string.Empty;
            public string EndpointPatchRecoveryCellFailureSource = string.Empty;
            public int EndpointPatchRecoveryBoundaryComponentCount;
            public int EndpointPatchRecoveryClosedCycleCount;
            public int EndpointPatchRecoveryOpenChainCount;
            public int EndpointPatchRecoveryBranchVertexCount;
            public int EndpointPatchRecoveryTransitionFaceCount;
            public int EndpointPatchRecoveryResidualOpenEdgeCount;
            public string EndpointPatchRecoveryMechanismSignature = string.Empty;
            public string EndpointPatchRecoveryModifiedIdentitySignature = string.Empty;
            public string EndpointPatchRecoveryDiagnostic = string.Empty;
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
            public string PreparedPlanHash = string.Empty;
            public string IntegrationPlanHash = string.Empty;
            public string EmittedPlanHash = string.Empty;
            public int[] PreparedOrdinaryIdentities = Array.Empty<int>();
            public int[] PreparedMandatoryIdentities = Array.Empty<int>();
            public int[] PlannedOrdinaryIdentities = Array.Empty<int>();
            public int[] PlannedMandatoryIdentities = Array.Empty<int>();
            public int[] MissingPlannedOrdinary = Array.Empty<int>();
            public int[] UnexpectedFinalOrdinary = Array.Empty<int>();
            public int[] MissingPlannedMandatory = Array.Empty<int>();
            public int[] UnexpectedFinalMandatory = Array.Empty<int>();
            public int RequestedChipCount;
            public int CommittedChipCount;
            public int CandidateAttemptCount;
            public int DepthTrialCount;
            public int InitialSafeCapacity;
            public string EarlyStopReason = string.Empty;
            public Vector3[] CommittedChipPositions = Array.Empty<Vector3>();
            public float[] CommittedChipDepths = Array.Empty<float>();
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
            BaseGeometryOnly,
            ProductionSurfaceFeatures,
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

        private static EdgeWearEvaluationMode
            ResolveProductionSurfaceFeatureBuildMode(
                MassSurfaceFeatureSettings? surfaceFeatures)
        {
            if (!surfaceFeatures.HasValue)
            {
                return EdgeWearEvaluationMode.BaseGeometryOnly;
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            bool edgeWearEnabled = settings.EdgeWearAmount > 0.0001f;
            return edgeWearEnabled || settings.CornerChippingEnabled
                ? EdgeWearEvaluationMode.ProductionSurfaceFeatures
                : EdgeWearEvaluationMode.BaseGeometryOnly;
        }

        public static MeshData Generate(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            EdgeWearEvaluationMode mode =
                ResolveProductionSurfaceFeatureBuildMode(surfaceFeatures);
            if (mode == EdgeWearEvaluationMode.BaseGeometryOnly)
            {
                return GenerateInternal(
                    recipe,
                    surfaceFeatures,
                    mode,
                    -1,
                    out _,
                    out _,
                    out _);
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            if (settings.CornerChippingEnabled)
            {
                return GenerateCornerDamageFullCertificationSearch(
                    recipe,
                    settings,
                    false,
                    null,
                    default,
                    0d,
                    0d,
                    CornerDamageSearchHardBudgetMilliseconds,
                    out _,
                    out _);
            }

            return GenerateInternal(
                recipe,
                settings,
                EdgeWearEvaluationMode.ProductionSurfaceFeatures,
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

        private const double CornerDamageSearchHardBudgetMilliseconds = 8000d;

        private sealed class CornerDamageBaselineBundle
        {
            public readonly MeshData Mesh;
            public readonly UnifiedEdgeWearPreviewStatus Status;
            public readonly double BuildMilliseconds;

            public CornerDamageBaselineBundle(
                MeshData mesh,
                UnifiedEdgeWearPreviewStatus status,
                double buildMilliseconds)
            {
                Mesh = mesh;
                Status = status;
                BuildMilliseconds = Math.Max(0d, buildMilliseconds);
            }
        }

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
                null,
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
                null,
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
                MeshData baselineMesh,
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
                if (baselineMesh != null && baselineStatus.PreviewApplied)
                {
                    unifiedStatus = baselineStatus;
                    return baselineMesh;
                }

                return GenerateUnifiedEdgeWearPreviewBaseline(
                    recipe,
                    surfaceFeatures,
                    out unifiedStatus);
            }

            return GenerateCornerDamageFullCertificationSearch(
                recipe,
                surfaceFeatures,
                baselineMesh != null && baselineStatus.PreviewApplied,
                baselineMesh,
                baselineStatus,
                baselineBuildMilliseconds,
                estimatedIntegrationMilliseconds,
                CornerDamageSearchHardBudgetMilliseconds,
                out previewStatus,
                out unifiedStatus);
        }

#if UNITY_EDITOR
        public static MeshData GenerateCornerDamageRecoveryTournamentPreview(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            MeshData baselineMesh,
            UnifiedEdgeWearPreviewStatus baselineStatus,
            double baselineBuildMilliseconds,
            double estimatedIntegrationMilliseconds,
            CornerDamageRecoveryTournamentConfiguration configuration,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            MassSurfaceFeatureSettings effectiveSettings =
                ApplyCornerDamageRecoveryTournamentSettings(
                    surfaceFeatures,
                    configuration);
            using (new CornerDamageRecoveryTournamentScope(configuration))
            {
                return GenerateCornerDamageFullCertificationSearch(
                    recipe,
                    effectiveSettings,
                    baselineMesh != null && baselineStatus.PreviewApplied,
                    baselineMesh,
                    baselineStatus,
                    baselineBuildMilliseconds,
                    estimatedIntegrationMilliseconds,
                    configuration.CaseBudgetMilliseconds,
                    out previewStatus,
                    out unifiedStatus);
            }
        }

        private static MassSurfaceFeatureSettings
            ApplyCornerDamageRecoveryTournamentSettings(
                MassSurfaceFeatureSettings source,
                CornerDamageRecoveryTournamentConfiguration configuration)
        {
            return source;
        }
#endif

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
                null,
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
            MeshData providedBaselineMesh,
            UnifiedEdgeWearPreviewStatus providedBaselineStatus,
            double providedBaselineMilliseconds,
            double estimatedIntegrationMilliseconds,
            double hardBudgetMilliseconds,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            // EW-C1C.1: corner selection and ordinary bevel construction are
            // deliberately separate transactions. The chip audit commits the
            // requested sequential chip count against exact certified face
            // topology, reranking after each chip. The accepted first rank is
            // then replayed once through CornerDamageIntegrationPreview, whose
            // runUnifiedEvaluation branch performs the same complete sequence
            // and enters the ordinary augmentation path only after the final
            // chip. No explicit corner solution, prepared plane-cut plan,
            // mandatory ring, or legacy endpoint recovery is allowed.
            CornerDamageSearchTelemetry telemetry =
                new CornerDamageSearchTelemetry();
            double countScaledHardBudgetMilliseconds =
                hardBudgetMilliseconds *
                (1d + 0.5d * Math.Max(
                    0,
                    surfaceFeatures.CornerChipCount - 1));
            System.Diagnostics.Stopwatch searchStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            CornerDamageBaselineBundle baseline =
                BuildCornerDamageBaselineBundle(
                    recipe,
                    surfaceFeatures,
                    useProvidedBaseline,
                    providedBaselineMesh,
                    providedBaselineStatus,
                    providedBaselineMilliseconds,
                    telemetry);
            // The complete case stopwatch still includes baseline generation,
            // but the ordinary integration deadline begins only after the
            // baseline exists. A cold baseline must not consume the entire
            // plane-solve window before the accepted chip is evaluated.
            using CornerDamageSearchDeadlineScope deadlineScope =
                new CornerDamageSearchDeadlineScope(
                    countScaledHardBudgetMilliseconds);

            int candidateCornerCount = -1;
            int attemptedCornerCount = 0;
            int acceptedCandidateRank = -1;
            CornerDamageTransactionAuditResult acceptedTransaction = null;
            string selectionFailureReason =
                "no certified corner-damage transaction was available";
            StringBuilder searchAttempts = new StringBuilder(256);

            for (int candidateRank = 0;
                 candidateCornerCount < 0 ||
                 candidateRank < candidateCornerCount;
                 candidateRank++)
            {
                if (IsEdgeWearAuditCancellationRequested())
                {
                    selectionFailureReason = "cancelled by user";
                    break;
                }
                if (searchStopwatch.Elapsed.TotalMilliseconds >=
                    countScaledHardBudgetMilliseconds)
                {
                    telemetry.CaseBudgetExceeded = true;
                    selectionFailureReason =
                        "corner transaction search exceeded the case hard budget";
                    break;
                }

                telemetry.TransactionAttemptCount++;
                ResetCornerDamageTransactionAuditCapture();
                System.Diagnostics.Stopwatch transactionStopwatch =
                    System.Diagnostics.Stopwatch.StartNew();
                InvalidOperationException transactionException = null;
                try
                {
                    using (new CornerDamageSearchAttemptScope(
                               candidateRank,
                               1f))
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
                }
                catch (InvalidOperationException exception)
                {
                    transactionException = exception;
                }
                finally
                {
                    transactionStopwatch.Stop();
                }

                CornerDamageTransactionAuditResult transaction =
                    CompleteCornerDamageTransactionAuditResultCapture();
                if (transaction != null)
                {
                    telemetry.CandidateRankingMilliseconds +=
                        transaction.CandidateRankingMilliseconds;
                    telemetry.TransactionMilliseconds += Mathf.Max(
                        0f,
                        (float)(transaction.TransactionMilliseconds -
                            transaction.CandidateRankingMilliseconds));
                }
                else
                {
                    telemetry.TransactionMilliseconds +=
                        transactionStopwatch.Elapsed.TotalMilliseconds;
                }

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

                if (searchAttempts.Length > 0)
                {
                    searchAttempts.Append(';');
                }
                searchAttempts.Append("r");
                searchAttempts.Append(candidateRank);
                searchAttempts.Append("@1:");
                if (transactionException != null)
                {
                    selectionFailureReason =
                        "corner transaction exception: " +
                        transactionException.Message;
                    searchAttempts.Append("transaction-exception");
                    continue;
                }
                if (transaction == null)
                {
                    selectionFailureReason =
                        "corner transaction audit result was unavailable";
                    searchAttempts.Append("transaction-unavailable");
                    continue;
                }
                if (!transaction.Succeeded)
                {
                    selectionFailureReason = string.IsNullOrEmpty(
                            transaction.Diagnostic)
                        ? "corner transaction did not certify"
                        : transaction.Diagnostic;
                    searchAttempts.Append("transaction-rejected");
                    continue;
                }

                acceptedCandidateRank = candidateRank;
                acceptedTransaction = transaction;
                searchAttempts.Append("chip-certified");
                break;
            }

            if (acceptedTransaction == null || acceptedCandidateRank < 0)
            {
                searchStopwatch.Stop();
                CornerDamagePreviewStatus selectionFailure =
                    new CornerDamagePreviewStatus
                    {
                        PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                        ShapeSeed = recipe.ShapeSeed,
                        AuthoringEnabled = true,
                        TransactionCertified = false,
                        Diagnostic = selectionFailureReason
                    };
                ApplyCornerDamageSearchSummary(
                    selectionFailure,
                    Mathf.Max(0, candidateCornerCount),
                    attemptedCornerCount,
                    0,
                    -1,
                    0f,
                    telemetry.CaseBudgetExceeded
                        ? "performance-budget"
                        : "corner-transaction",
                    selectionFailureReason,
                    searchAttempts.ToString(),
                    telemetry);
                return ReturnCornerDamageBaselineFallback(
                    baseline,
                    selectionFailure,
                    out previewStatus,
                    out unifiedStatus);
            }

            telemetry.FullIntegrationBuildCount = 1;
            System.Diagnostics.Stopwatch directStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            ResetCornerDamagePreviewCapture();
            MeshData directMesh = null;
            UnifiedEdgeWearPreviewStatus directUnified = default;
            InvalidOperationException directException = null;
            try
            {
                using (new CornerDamageSearchAttemptScope(
                           acceptedCandidateRank,
                           1f))
                {
                    directMesh = GenerateInternal(
                        recipe,
                        surfaceFeatures,
                        EdgeWearEvaluationMode.CornerDamageIntegrationPreview,
                        -1,
                        out _,
                        out _,
                        out directUnified);
                }
            }
            catch (InvalidOperationException exception)
            {
                directException = exception;
            }
            finally
            {
                directStopwatch.Stop();
            }

            telemetry.IntegrationMilliseconds =
                directStopwatch.Elapsed.TotalMilliseconds;
            CornerDamagePreviewStatus directStatus =
                CompleteCornerDamagePreviewCapture(
                    recipe,
                    baseline.Status,
                    directUnified,
                    baseline.BuildMilliseconds,
                    directStopwatch.Elapsed.TotalMilliseconds);
            if (directStatus == null)
            {
                directStatus = new CornerDamagePreviewStatus
                {
                    PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                    ShapeSeed = recipe.ShapeSeed,
                    AuthoringEnabled = true,
                    TransactionCertified = true,
                    AcceptedCornerRank = acceptedCandidateRank
                };
            }

            // These fields remain named "Preflight" for report-schema
            // compatibility, but in C1B.1d they are direct ordinary-production
            // evidence captured from the actual unified build.
            directStatus.PreflightCandidateCount =
                directUnified.CandidateCount;
            directStatus.PreflightSelectedCount =
                directUnified.CandidateCount;
            directStatus.PreflightSelectedGraphEdgeCount =
                directUnified.CandidateCount;
            directStatus.PreflightCandidateConservationValid =
                directUnified.CandidateCount > 0;
            directStatus.PreflightTopologyReady =
                directUnified.CandidateCount > 0;
            directStatus.PreflightWidthSolutionReady =
                directUnified.PreviewApplied;
            directStatus.PreflightMandatorySolvedCount = 0;
            directStatus.IntegrationPreflightDiagnostic =
                directUnified.Diagnostic ?? string.Empty;

            bool directApplied =
                directException == null &&
                directMesh != null &&
                directStatus.PreviewApplied &&
                directUnified.PreviewApplied &&
                directUnified.CandidateCount > 0 &&
                directUnified.ActiveEdgeCount > 0 &&
                directUnified.BevelFaceCount > 0;
            string directFailureReason;
            if (directApplied)
            {
                directFailureReason = "none";
            }
            else if (directException != null)
            {
                directFailureReason =
                    "direct ordinary bevel build exception: " +
                    directException.Message;
            }
            else if (!string.IsNullOrEmpty(directStatus.Diagnostic))
            {
                directFailureReason = directStatus.Diagnostic;
            }
            else if (!string.IsNullOrEmpty(directUnified.Diagnostic))
            {
                directFailureReason = directUnified.Diagnostic;
            }
            else
            {
                directFailureReason =
                    "direct ordinary bevel build did not certify";
            }

            if (searchAttempts.Length > 0)
            {
                searchAttempts.Append(';');
            }
            searchAttempts.Append("r");
            searchAttempts.Append(acceptedCandidateRank);
            searchAttempts.Append("@1:");
            searchAttempts.Append(
                directApplied
                    ? "direct-ordinary-certified"
                    : "direct-ordinary-rejected");

            searchStopwatch.Stop();
            ApplyCornerDamageSearchSummary(
                directStatus,
                Mathf.Max(0, candidateCornerCount),
                attemptedCornerCount,
                1,
                directApplied ? acceptedCandidateRank : -1,
                0f,
                directApplied ? "none" : "direct-ordinary-build",
                directFailureReason,
                searchAttempts.ToString(),
                telemetry);
            if (directApplied)
            {
                previewStatus = directStatus;
                unifiedStatus = directUnified;
                return directMesh;
            }

            directStatus.PreviewApplied = false;
            directStatus.Diagnostic = directFailureReason;
            return ReturnCornerDamageChipFirstFallback(
                recipe,
                surfaceFeatures,
                baseline,
                acceptedCandidateRank,
                directStatus,
                out previewStatus,
                out unifiedStatus);
        }

        private static MeshData GenerateCornerDamageLegacyFullCertificationSearch(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            bool useProvidedBaseline,
            MeshData providedBaselineMesh,
            UnifiedEdgeWearPreviewStatus providedBaselineStatus,
            double providedBaselineMilliseconds,
            double estimatedIntegrationMilliseconds,
            double hardBudgetMilliseconds,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            CornerDamageSearchTelemetry telemetry =
                new CornerDamageSearchTelemetry();
            CornerDamagePreflightReplayCache replayCache =
                new CornerDamagePreflightReplayCache();
            using CornerDamagePreflightReplayScope replayScope =
                new CornerDamagePreflightReplayScope(replayCache);
            using CornerDamageSearchDeadlineScope deadlineScope =
                new CornerDamageSearchDeadlineScope(hardBudgetMilliseconds);
            System.Diagnostics.Stopwatch searchStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            CornerDamageBaselineBundle baseline =
                BuildCornerDamageBaselineBundle(
                    recipe,
                    surfaceFeatures,
                    useProvidedBaseline,
                    providedBaselineMesh,
                    providedBaselineStatus,
                    providedBaselineMilliseconds,
                    telemetry);
            UnifiedEdgeWearPreviewStatus baselineStatus = baseline.Status;
            double baselineMilliseconds = baseline.BuildMilliseconds;

            int candidateCornerCount = -1;
            int attemptedCornerCount = 0;
            int attemptedConfigurationCount = 0;
            int bestFailurePriority = int.MinValue;
            CornerDamagePreviewStatus bestFailure = null;
            string bestFailureStage = "candidate-availability";
            string bestFailureReason =
                "no eligible corner-damage candidate was available";
            StringBuilder searchAttempts = new StringBuilder(512);
            CornerDamageIntegrationPreflightRecord acceptedPreflight = null;
            CornerDamageIntegrationPlan acceptedPlan = null;
            int acceptedCandidateRank = -1;
            int chipFallbackCandidateRank = -1;

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
                    if (transaction.Succeeded && chipFallbackCandidateRank < 0)
                    {
                        chipFallbackCandidateRank = candidateRank;
                    }
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
                    // C1B.1: a certified chip locks the selected corner.
                    // The bevel pass may succeed, reduce its selected set, or
                    // fail back to this exact closed chip-only mesh; it may not
                    // choose a different corner to preserve pre-chip bevels.
                    preflight.PredictedUnrelatedBaselineCount = 0;
                    preflight.PredictedUnrelatedRetainedCount = 0;
                    preflight.PredictedCollateralLostCount = 0;
                    preflight.PredictedCollateralLostIdentities =
                        Array.Empty<int>();
                    telemetry.IntegrationPlanAttemptCount++;
                    telemetry.AuthoritativeSolveAttemptCount++;
                    bool preparationBuilt =
                        TryPrepareCornerDamageIntegrationPlan(
                            recipe,
                            preflight,
                            out CornerDamageIntegrationPlan candidatePlan,
                            out double preparationMilliseconds);
                    telemetry.IntegrationPlanMilliseconds +=
                        preparationMilliseconds;
                    telemetry.AuthoritativeSolveMilliseconds +=
                        preparationMilliseconds;
                    if (candidatePlan != null &&
                        candidatePlan.EndpointConflictGuardAttempted)
                    {
                        telemetry.EndpointConflictGuardAttemptCount++;
                        telemetry.EndpointConflictGuardTestedRailCount +=
                            candidatePlan.EndpointConflictGuardTestedRailCount;
                        telemetry.EndpointConflictGuardMilliseconds +=
                            candidatePlan.EndpointConflictGuardMilliseconds;
                        if (candidatePlan.EndpointConflictGuardPassed)
                        {
                            telemetry.EndpointConflictGuardPassCount++;
                        }
                        else if (candidatePlan.EndpointConflictGuardConflictCount > 0)
                        {
                            telemetry.EndpointConflictGuardRejectCount++;
                        }
                    }
                    if (candidatePlan != null &&
                        candidatePlan.EndpointPatchRecoveryAttempted)
                    {
                        telemetry.EndpointPatchRecoveryAttemptCount +=
                            candidatePlan.EndpointPatchRecoveryAttemptCount;
                        telemetry.EndpointPatchRecoveryMilliseconds +=
                            candidatePlan.EndpointPatchRecoveryMilliseconds;
                        telemetry.EndpointPatchRecoveryUnsupportedStarCount +=
                            candidatePlan.EndpointPatchRecoveryUnsupportedStarCount;
                        telemetry.EndpointPatchRecoveryPatchExtractionCount +=
                            candidatePlan.EndpointPatchRecoveryPatchExtractionCount;
                        telemetry.EndpointPatchRecoveryDisconnectedPatchCount +=
                            candidatePlan.EndpointPatchRecoveryDisconnectedPatchCount;
                        telemetry.EndpointPatchRecoveryBoundaryLoopCount +=
                            candidatePlan.EndpointPatchRecoveryBoundaryLoopCount;
                        telemetry.EndpointPatchRecoveryBoundaryCrossingCount +=
                            candidatePlan.EndpointPatchRecoveryBoundaryCrossingCount;
                        telemetry.EndpointPatchRecoveryNoLocalRemovalCount +=
                            candidatePlan.EndpointPatchRecoveryNoLocalRemovalCount;
                        telemetry.EndpointPatchRecoveryCapCreationCount +=
                            candidatePlan.EndpointPatchRecoveryCapCreationCount;
                        telemetry.EndpointPatchRecoveryIncidentBandJoinCount +=
                            candidatePlan.EndpointPatchRecoveryIncidentBandJoinCount;
                        telemetry.EndpointPatchRecoveryStitchTopologyCount +=
                            candidatePlan.EndpointPatchRecoveryStitchTopologyCount;
                        telemetry.EndpointPatchRecoveryLocalityCount +=
                            candidatePlan.EndpointPatchRecoveryLocalityCount;
                        telemetry.EndpointPatchRecoveryBandIntegrityCount +=
                            candidatePlan.EndpointPatchRecoveryBandIntegrityCount;
                        telemetry.EndpointPatchRecoveryPreparedMinimumParityCount +=
                            candidatePlan.EndpointPatchRecoveryPreparedMinimumParityCount;
                        telemetry.EndpointPatchRecoveryMaterializationSignatureCount +=
                            candidatePlan.EndpointPatchRecoveryMaterializationSignatureCount;
                        telemetry.EndpointPatchRecoveryMaximumRemovedVertexRadius =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumRemovedVertexRadius,
                                candidatePlan.EndpointPatchRecoveryMaximumRemovedVertexRadius);
                        telemetry.EndpointPatchRecoveryMaximumIntersectionRadius =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumIntersectionRadius,
                                candidatePlan.EndpointPatchRecoveryMaximumIntersectionRadius);
                        telemetry.EndpointPatchRecoveryMaximumReplacementVertexRadius =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumReplacementVertexRadius,
                                candidatePlan.EndpointPatchRecoveryMaximumReplacementVertexRadius);
                        telemetry.EndpointPatchRecoveryRetainedOutsideRadiusCount +=
                            candidatePlan.EndpointPatchRecoveryRetainedOutsideRadiusCount;
                        telemetry.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter +=
                            candidatePlan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter;
                        telemetry.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter +=
                            candidatePlan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter;
                        telemetry.EndpointPatchRecoveryLocalSupportSampleCount +=
                            candidatePlan.EndpointPatchRecoveryLocalSupportSampleCount;
                        if (candidatePlan.EndpointPatchRecoveryMinimumSamplesPerIncident > 0)
                        {
                            telemetry.EndpointPatchRecoveryMinimumSamplesPerIncident =
                                telemetry.EndpointPatchRecoveryMinimumSamplesPerIncident == 0
                                    ? candidatePlan.EndpointPatchRecoveryMinimumSamplesPerIncident
                                    : Mathf.Min(
                                        telemetry.EndpointPatchRecoveryMinimumSamplesPerIncident,
                                        candidatePlan.EndpointPatchRecoveryMinimumSamplesPerIncident);
                        }
                        telemetry.EndpointPatchRecoveryMaximumGlobalMinusLocalSupportDelta =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumGlobalMinusLocalSupportDelta,
                                candidatePlan.EndpointPatchRecoveryGlobalMinusLocalSupportDelta);
                        telemetry.EndpointPatchRecoveryMaximumControllingSupportRadius =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumControllingSupportRadius,
                                candidatePlan.EndpointPatchRecoveryControllingSupportRadius);
                        telemetry.EndpointPatchRecoveryMaximumAxialInfluence =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumAxialInfluence,
                                candidatePlan.EndpointPatchRecoveryMaximumAxialInfluence);
                        if (candidatePlan.EndpointPatchRecoveryMinimumAllowedAxialInfluence > 0f)
                        {
                            telemetry.EndpointPatchRecoveryMinimumAllowedAxialInfluence =
                                float.IsInfinity(
                                    telemetry.EndpointPatchRecoveryMinimumAllowedAxialInfluence)
                                    ? candidatePlan.EndpointPatchRecoveryMinimumAllowedAxialInfluence
                                    : Mathf.Min(
                                        telemetry.EndpointPatchRecoveryMinimumAllowedAxialInfluence,
                                        candidatePlan.EndpointPatchRecoveryMinimumAllowedAxialInfluence);
                        }
                        telemetry.EndpointPatchRecoveryFacesSubdivided +=
                            candidatePlan.EndpointPatchRecoveryFacesSubdivided;
                        telemetry.EndpointPatchRecoveryLocalFragmentCount +=
                            candidatePlan.EndpointPatchRecoveryLocalFragmentCount;
                        telemetry.EndpointPatchRecoveryRemoteRemainderCount +=
                            candidatePlan.EndpointPatchRecoveryRemoteRemainderCount;
                        telemetry.EndpointPatchRecoverySyntheticIncidentFragmentCount +=
                            candidatePlan.EndpointPatchRecoverySyntheticIncidentFragmentCount;
                        telemetry.EndpointPatchRecoveryMaximumCellVertexCount =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumCellVertexCount,
                                candidatePlan.EndpointPatchRecoveryCellVertexCount);
                        telemetry.EndpointPatchRecoveryMaximumCellFaceCount =
                            Mathf.Max(
                                telemetry.EndpointPatchRecoveryMaximumCellFaceCount,
                                candidatePlan.EndpointPatchRecoveryCellFaceCount);
                        if (candidatePlan.EndpointPatchRecoveryPrepared)
                        {
                            telemetry.EndpointPatchRecoveryPreparedCount++;
                        }
                        else
                        {
                            telemetry.EndpointPatchRecoveryRejectCount++;
                        }
                    }
                    if (preparationBuilt && candidatePlan != null &&
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
                            "candidate-preparation-certified",
                            preflightStatus);
                        break;
                    }

                    telemetry.AuthoritativeSolveRejectCount++;
                    if (IsCornerDamageSearchDeadlineExceeded())
                    {
                        telemetry.DeadlineAbortCount++;
                    }
                    string preparationDiagnostic = candidatePlan == null
                        ? "candidate preparation result was unavailable"
                        : candidatePlan.Diagnostic;
                    preflightStatus.PreviewApplied = false;
                    preflightStatus.Diagnostic = preparationDiagnostic;
                    ApplyCornerDamageIntegrationPlanEvidence(
                        preflightStatus,
                        candidatePlan);
                    string preparationFailureStage =
                        candidatePlan != null &&
                        candidatePlan.EndpointConflictGuardAttempted &&
                        !candidatePlan.EndpointConflictGuardPassed &&
                        candidatePlan.EndpointConflictGuardConflictCount > 0
                            ? "endpoint-conflict-guard"
                            : "candidate-preparation";
                    AppendCornerDamageSearchAttempt(
                        searchAttempts,
                        candidateRank,
                        preflight.ResolvedUniformScale,
                        preparationFailureStage,
                        preflightStatus);
                    RetainCornerDamageSearchFailure(
                        preflightStatus,
                        preparationFailureStage,
                        ref bestFailure,
                        ref bestFailurePriority,
                        ref bestFailureStage,
                        ref bestFailureReason);
                    // The chip transaction already certified. Do not let
                    // bevel success influence corner selection.
                    break;
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
                    preflightStage,
                    ref bestFailure,
                    ref bestFailurePriority,
                    ref bestFailureStage,
                    ref bestFailureReason);
                if (transaction != null && transaction.Succeeded)
                {
                    // A valid chip is authoritative even when the ordinary
                    // bevel pass has no certifiable result.
                    break;
                }
            }

            if (acceptedPreflight == null || acceptedPlan == null)
            {
                searchStopwatch.Stop();
                CornerDamagePreviewStatus failureStatus = bestFailure ??
                    new CornerDamagePreviewStatus
                    {
                        PreviewKind = CornerDamagePreviewKind.WithEdgeWear,
                        ShapeSeed = recipe.ShapeSeed,
                        AuthoringEnabled = true,
                        Diagnostic = bestFailureReason
                    };
                ApplyCornerDamageSearchSummary(
                    failureStatus,
                    Mathf.Max(0, candidateCornerCount),
                    attemptedCornerCount,
                    attemptedConfigurationCount,
                    -1,
                    0f,
                    bestFailureStage,
                    bestFailureReason,
                    searchAttempts.ToString(),
                    telemetry);
                return ReturnCornerDamageChipFirstFallback(
                    recipe,
                    surfaceFeatures,
                    baseline,
                    chipFallbackCandidateRank,
                    failureStatus,
                    out previewStatus,
                    out unifiedStatus);
            }

            double remainingBudget = hardBudgetMilliseconds -
                searchStopwatch.Elapsed.TotalMilliseconds;
            const double MinimumCompleteBuildBudgetMilliseconds = 250d;
            if (remainingBudget + 0.001d <
                MinimumCompleteBuildBudgetMilliseconds)
            {
                telemetry.CaseBudgetExceeded = true;
                telemetry.DeadlineAbortCount++;
                CornerDamagePreviewStatus budgetStatus =
                    BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        acceptedPreflight,
                        "insufficient case budget remains for the one complete authoritative build");
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
                return ReturnCornerDamageChipFirstFallback(
                    recipe,
                    surfaceFeatures,
                    baseline,
                    acceptedCandidateRank,
                    budgetStatus,
                    out previewStatus,
                    out unifiedStatus);
            }

            telemetry.PlanMaterializationBuildCount = 1;
            telemetry.FullIntegrationBuildCount = 1;
            attemptedConfigurationCount = 1;
            bool completed = TryCompleteCornerDamageIntegrationPlan(
                acceptedPlan,
                acceptedPreflight.ExpectedMandatoryCount,
                out double completeBuildMilliseconds,
                out string completeBuildFailureStage);
            telemetry.PlanMaterializationMilliseconds +=
                completeBuildMilliseconds;
            telemetry.IntegrationMilliseconds =
                completeBuildMilliseconds;
            if (acceptedPlan.EndpointConflictGuardFalseNegative)
            {
                telemetry.EndpointConflictGuardFalseNegativeCount++;
            }
            if (acceptedPlan.EndpointPatchRecoveryPrepared)
            {
                if (completed &&
                    acceptedPlan.EndpointPatchRecoveryApplied)
                {
                    telemetry.EndpointPatchRecoveryAppliedCount++;
                }
                else if (!completed)
                {
                    acceptedPlan.EndpointPatchRecoveryFalsePositive = true;
                    telemetry.EndpointPatchRecoveryFalsePositiveCount++;
                    telemetry.EndpointPatchRecoveryMaterializationSignatureCount +=
                        acceptedPlan.EndpointPatchRecoveryMaterializationSignatureCount;
                }
            }
            if (!completed)
            {
                bool deadlineExceeded =
                    IsCornerDamageSearchDeadlineExceeded() ||
                    searchStopwatch.Elapsed.TotalMilliseconds >=
                        hardBudgetMilliseconds;
                if (deadlineExceeded)
                {
                    telemetry.CaseBudgetExceeded = true;
                    telemetry.DeadlineAbortCount++;
                }
                CornerDamagePreviewStatus completionStatus =
                    BuildCornerDamagePreflightStatus(
                        recipe,
                        surfaceFeatures,
                        acceptedPreflight,
                        acceptedPlan.Diagnostic);
                ApplyCornerDamageIntegrationPlanEvidence(
                    completionStatus,
                    acceptedPlan);
                string completionStage = deadlineExceeded
                    ? "performance-budget"
                    : string.IsNullOrEmpty(completeBuildFailureStage)
                        ? "complete-authoritative-build"
                        : completeBuildFailureStage;
                AppendCornerDamageSearchAttempt(
                    searchAttempts,
                    acceptedCandidateRank,
                    acceptedPreflight.ResolvedUniformScale,
                    completionStage,
                    completionStatus);
                ApplyCornerDamageSearchSummary(
                    completionStatus,
                    candidateCornerCount,
                    attemptedCornerCount,
                    attemptedConfigurationCount,
                    -1,
                    0f,
                    completionStage,
                    completionStatus.Diagnostic,
                    searchAttempts.ToString(),
                    telemetry);
                return ReturnCornerDamageChipFirstFallback(
                    recipe,
                    surfaceFeatures,
                    baseline,
                    acceptedCandidateRank,
                    completionStatus,
                    out previewStatus,
                    out unifiedStatus);
            }

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
                    ? "emitted shell disagreed with the completed authoritative build"
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
                        "candidate preparation, complete authoritative build, and emission exceeded the case hard budget";
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
            return ReturnCornerDamageChipFirstFallback(
                recipe,
                surfaceFeatures,
                baseline,
                acceptedCandidateRank,
                finalStatus,
                out previewStatus,
                out unifiedStatus);
        }

        private static CornerDamagePreflightReplayCache
            ResolveCornerDamagePreflightReplayCache()
        {
            return cornerDamagePreflightReplayCache;
        }

        private static void CaptureCornerDamagePreflightReplayTelemetry(
            CornerDamageSearchTelemetry telemetry)
        {
            if (telemetry == null)
            {
                return;
            }

            CornerDamagePreflightReplayCache cache =
                ResolveCornerDamagePreflightReplayCache();
            if (cache == null)
            {
                return;
            }

            telemetry.PreflightFoundationBuildCount =
                cache.NormalizedFoundationBuildCount;
            telemetry.PreflightFoundationReuseCount =
                cache.NormalizedFoundationReuseCount;
            telemetry.IsolatedReplayAttemptCount =
                cache.IsolatedReplayAttemptCount;
            telemetry.IsolatedReplayHitCount =
                cache.IsolatedReplayHitCount;
            telemetry.IsolatedReplayMissCount =
                cache.IsolatedReplayMissCount;
            telemetry.IsolatedFullEvaluationCount =
                cache.IsolatedFullEvaluationCount;
        }

        private static CornerDamageBaselineBundle
            BuildCornerDamageBaselineBundle(
                MassRecipe recipe,
                MassSurfaceFeatureSettings surfaceFeatures,
                bool useProvidedBaseline,
                MeshData providedBaselineMesh,
                UnifiedEdgeWearPreviewStatus providedBaselineStatus,
                double providedBaselineMilliseconds,
                CornerDamageSearchTelemetry telemetry)
        {
            if (useProvidedBaseline && providedBaselineMesh != null &&
                providedBaselineStatus.PreviewApplied)
            {
                telemetry.BaselineCacheUseCount = 1;
                return new CornerDamageBaselineBundle(
                    providedBaselineMesh,
                    providedBaselineStatus,
                    providedBaselineMilliseconds);
            }

            System.Diagnostics.Stopwatch baselineStopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            MeshData baselineMesh = GenerateInternal(
                recipe,
                surfaceFeatures,
                EdgeWearEvaluationMode.UnifiedBoundedPreview,
                -1,
                out _,
                out _,
                out UnifiedEdgeWearPreviewStatus baselineStatus);
            baselineStopwatch.Stop();
            telemetry.BaselineBuildCount = 1;
            return new CornerDamageBaselineBundle(
                baselineMesh,
                baselineStatus,
                baselineStopwatch.Elapsed.TotalMilliseconds);
        }

        private static MeshData ReturnCornerDamageChipFirstFallback(
            MassRecipe recipe,
            MassSurfaceFeatureSettings surfaceFeatures,
            CornerDamageBaselineBundle baseline,
            int candidateRank,
            CornerDamagePreviewStatus failureStatus,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            if (candidateRank >= 0)
            {
                ResetCornerDamagePreviewCapture();
                System.Diagnostics.Stopwatch stopwatch =
                    System.Diagnostics.Stopwatch.StartNew();
                MeshData chipMesh = null;
                UnifiedEdgeWearPreviewStatus chipUnified = default;
                InvalidOperationException chipException = null;
                try
                {
                    using (new CornerDamageSearchAttemptScope(
                               candidateRank,
                               1f))
                    {
                        chipMesh = GenerateInternal(
                            recipe,
                            surfaceFeatures,
                            EdgeWearEvaluationMode.CornerDamageGeometryPreview,
                            -1,
                            out _,
                            out _,
                            out chipUnified);
                    }
                }
                catch (InvalidOperationException exception)
                {
                    chipException = exception;
                }
                finally
                {
                    stopwatch.Stop();
                }

                CornerDamagePreviewStatus chipStatus =
                    CompleteCornerDamagePreviewCapture(
                        recipe,
                        default,
                        chipUnified,
                        0d,
                        stopwatch.Elapsed.TotalMilliseconds);
                if (chipException == null && chipMesh != null &&
                    chipStatus != null && chipStatus.PreviewApplied)
                {
                    chipStatus.PreviewKind =
                        CornerDamagePreviewKind.WithEdgeWear;
                    chipStatus.ExpectedCapRingEdgeCount = 0;
                    chipStatus.MandatoryCandidateCount = 0;
                    chipStatus.MandatorySelectedCount = 0;
                    chipStatus.MandatoryBuiltCount = 0;
                    chipStatus.MandatoryCapRingIdentities =
                        Array.Empty<int>();
                    chipStatus.CapRingCommittedScale = 0f;
                    chipStatus.CapRingRequestedWidth = 0f;
                    string preservedFailureReason =
                        failureStatus == null
                            ? "fresh ordinary bevel pass did not certify"
                            : !string.IsNullOrEmpty(
                                failureStatus.SearchFailureReason) &&
                              !string.Equals(
                                  failureStatus.SearchFailureReason,
                                  "none",
                                  StringComparison.Ordinal)
                                ? failureStatus.SearchFailureReason
                                : !string.IsNullOrEmpty(
                                    failureStatus.Diagnostic)
                                    ? failureStatus.Diagnostic
                                    : "fresh ordinary bevel pass did not certify";
                    string preservedFailureStage =
                        failureStatus == null ||
                        string.IsNullOrEmpty(
                            failureStatus.SearchFailureStage) ||
                        string.Equals(
                            failureStatus.SearchFailureStage,
                            "none",
                            StringComparison.Ordinal)
                            ? "direct-ordinary-build"
                            : failureStatus.SearchFailureStage;
                    chipStatus.Diagnostic =
                        "closed chip-only fallback retained: " +
                        preservedFailureReason;
                    chipStatus.Report =
                        (chipStatus.Report ?? string.Empty) +
                        Environment.NewLine +
                        "C1B.1 fallback=closed-chip-only; mandatoryRing=disabled";
                    if (failureStatus != null)
                    {
                        chipStatus.PreflightCandidateCount =
                            failureStatus.PreflightCandidateCount;
                        chipStatus.PreflightSelectedCount =
                            failureStatus.PreflightSelectedCount;
                        chipStatus.PreflightSelectedGraphEdgeCount =
                            failureStatus.PreflightSelectedGraphEdgeCount;
                        chipStatus.PreflightCandidateConservationValid =
                            failureStatus.PreflightCandidateConservationValid;
                        chipStatus.PreflightTopologyReady =
                            failureStatus.PreflightTopologyReady;
                        chipStatus.PreflightWidthSolutionReady =
                            failureStatus.PreflightWidthSolutionReady;
                        chipStatus.IntegrationPreflightDiagnostic =
                            failureStatus.IntegrationPreflightDiagnostic;
                    }

                    CornerDamageSearchTelemetry fallbackTelemetry =
                        CopyCornerDamageSearchTelemetry(failureStatus);
                    fallbackTelemetry.FullFallbackBuildCount = 1;
                    fallbackTelemetry.GeometrySearchReuseCount++;
                    ApplyCornerDamageSearchSummary(
                        chipStatus,
                        failureStatus == null
                            ? chipStatus.CandidateCornerCount
                            : failureStatus.CandidateCornerCount,
                        failureStatus == null
                            ? 1
                            : failureStatus.AttemptedCornerCount,
                        failureStatus == null
                            ? 1
                            : failureStatus.AttemptedConfigurationCount,
                        candidateRank,
                        0f,
                        preservedFailureStage,
                        preservedFailureReason,
                        failureStatus == null
                            ? string.Empty
                            : failureStatus.SearchAttemptSummary,
                        fallbackTelemetry);
                    previewStatus = chipStatus;
                    unifiedStatus = chipUnified;
                    return chipMesh;
                }
            }

            return ReturnCornerDamageBaselineFallback(
                baseline,
                failureStatus,
                out previewStatus,
                out unifiedStatus);
        }

        private static MeshData ReturnCornerDamageBaselineFallback(
            CornerDamageBaselineBundle baseline,
            CornerDamagePreviewStatus failureStatus,
            out CornerDamagePreviewStatus previewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedStatus)
        {
            if (baseline == null || baseline.Mesh == null)
            {
                throw new InvalidOperationException(
                    "ordinary corner-fallback baseline mesh was unavailable");
            }

            if (failureStatus != null)
            {
                failureStatus.PreviewApplied = false;
            }
            previewStatus = failureStatus;
            unifiedStatus = baseline.Status;
            return baseline.Mesh;
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

        private static bool TryPrepareCornerDamageIntegrationPlan(
            MassRecipe recipe,
            CornerDamageIntegrationPreflightRecord preflight,
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
                        "corner search deadline exceeded before candidate preparation";
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
                    out PlaneCutBevelSolvedPlan solvedPlan);
                EdgeWearCoverageAudit effectiveCoverage =
                    solvedPlan == null
                        ? audit.CoverageAudit ?? coverage
                        : solvedPlan.CoverageAudit ?? coverage;
                int[] ordinary = CollectCornerDamagePreparedPlanIdentities(
                    solvedPlan,
                    effectiveCoverage,
                    false);
                int[] mandatory = CollectCornerDamagePreparedPlanIdentities(
                    solvedPlan,
                    effectiveCoverage,
                    true);
                // C1B.1: the post-chip graph owns a fresh identity space.
                // There is deliberately no comparison with the unchipped
                // ordinary-bevel baseline.
                List<int> lost = new List<int>();
                int unrelatedBaselineCount = 0;
                int unrelatedRetainedCount = 0;

                plan.SolvedPlan = solvedPlan;
                plan.PlaneAudit = audit;
                plan.PreparedOrdinaryIdentities = ordinary;
                plan.PreparedMandatoryIdentities = mandatory;
                plan.UnrelatedBaselineCount = unrelatedBaselineCount;
                plan.UnrelatedRetainedCount = unrelatedRetainedCount;
                plan.CollateralLostCount = lost.Count;
                plan.CollateralLostIdentities = lost.ToArray();
                plan.PreparedPlanHash =
                    BuildCornerDamageIntegrationPlanHash(
                        plan.Transaction,
                        plan.ResolvedUniformScale,
                        ordinary,
                        mandatory);
                plan.IntegrationPlanHash = string.Empty;
                plan.EmittedPlanHash = string.Empty;

                if (solvedPlan == null ||
                    !solvedPlan.SolveValid)
                {
                    plan.Diagnostic = string.IsNullOrEmpty(audit.Diagnostic)
                        ? "candidate plane-and-rail preparation did not certify"
                        : audit.Diagnostic;
                    return false;
                }
                if (mandatory.Length != preflight.ExpectedMandatoryCount)
                {
                    plan.Diagnostic =
                        "candidate preparation has an incomplete mandatory cap ring";
                    return false;
                }
                if (lost.Count > 0)
                {
                    plan.Diagnostic =
                        "candidate preparation predicts unrelated baseline bevel loss";
                    return false;
                }
                ResetCornerDamageEndpointPatchSupportAndAxialEvidence(
                    plan);
                // C1B.1: do not run the identity-preserving endpoint splice
                // guard. The ordinary bevel kernel owns viability, width
                // reduction, deferral, and rejection on the rebuilt chip mesh.
                plan.EndpointConflictGuardAttempted = false;
                plan.EndpointConflictGuardPassed = false;
                plan.EndpointConflictGuardDiagnostic =
                    "not applicable to fresh post-chip ordinary beveling";

                plan.Valid = true;
                plan.Diagnostic =
                    "corner-integration candidate preparation certified";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                plan.Diagnostic =
                    "candidate preparation exception: " +
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

        private static int[] CollectCornerDamagePreparedPlanIdentities(
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

        private static void
            ResetCornerDamageEndpointPatchSupportAndAxialEvidence(
                CornerDamageIntegrationPlan plan)
        {
            if (plan == null)
            {
                return;
            }
            plan.EndpointPatchRecoveryLocalSupportSampleCount = 0;
            plan.EndpointPatchRecoveryMinimumSamplesPerIncident = 0;
            plan.EndpointPatchRecoverySamplesPerIncident = string.Empty;
            plan.EndpointPatchRecoveryLocalSupportRadius = 0f;
            plan.EndpointPatchRecoveryLocalSupportProjection = 0f;
            plan.EndpointPatchRecoveryGlobalSupportProjection = 0f;
            plan.EndpointPatchRecoveryGlobalMinusLocalSupportDelta = 0f;
            plan.EndpointPatchRecoveryControllingSupportEdgeIndex = -1;
            plan.EndpointPatchRecoveryControllingSupportRadius = 0f;
            plan.EndpointPatchRecoverySupportFailureSource = string.Empty;
            plan.EndpointPatchRecoveryMaximumAxialInfluence = 0f;
            plan.EndpointPatchRecoveryMinimumAllowedAxialInfluence = 0f;
            plan.EndpointPatchRecoveryAxialRejectedEdgeIndex = -1;
            plan.EndpointPatchRecoveryAxialRejectedEndpointVertexIndex = -1;
            plan.EndpointPatchRecoveryAxialInfluenceSignature = string.Empty;
            plan.EndpointPatchRecoveryCellLimitSignature = string.Empty;
            plan.EndpointPatchRecoveryFacesSubdivided = 0;
            plan.EndpointPatchRecoveryLocalFragmentCount = 0;
            plan.EndpointPatchRecoveryRemoteRemainderCount = 0;
            plan.EndpointPatchRecoverySyntheticIncidentFragmentCount = 0;
            plan.EndpointPatchRecoverySyntheticIncidentIdentities = string.Empty;
            plan.EndpointPatchRecoveryCellVertexCount = 0;
            plan.EndpointPatchRecoveryCellFaceCount = 0;
            plan.EndpointPatchRecoveryCellSplitSignature = string.Empty;
            plan.EndpointPatchRecoveryLocalFragmentSignature = string.Empty;
            plan.EndpointPatchRecoveryRemoteRemainderSignature = string.Empty;
            plan.EndpointPatchRecoveryCellFailureSource = string.Empty;
            plan.EndpointPatchRecoveryBoundaryComponentCount = 0;
            plan.EndpointPatchRecoveryClosedCycleCount = 0;
            plan.EndpointPatchRecoveryOpenChainCount = 0;
            plan.EndpointPatchRecoveryBranchVertexCount = 0;
            plan.EndpointPatchRecoveryTransitionFaceCount = 0;
            plan.EndpointPatchRecoveryResidualOpenEdgeCount = 0;
            plan.EndpointPatchRecoveryMechanismSignature = string.Empty;
            plan.EndpointPatchRecoveryModifiedIdentitySignature = string.Empty;
        }

        private static bool TryPassCornerDamageEndpointConflictGuard(
            CornerDamageIntegrationPlan plan)
        {
            if (plan == null)
            {
                return true;
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            plan.EndpointConflictGuardAttempted = true;
            plan.EndpointConflictGuardPassed = true;
            plan.EndpointConflictGuardTestedRailCount = 0;
            plan.EndpointConflictGuardConflictCount = 0;
            plan.EndpointConflictGuardVictimEdgeIndex = -1;
            plan.EndpointConflictGuardForeignEdgeIndex = -1;
            plan.EndpointConflictGuardAxialParameter = 0f;
            plan.EndpointConflictGuardEndpointAllowance = 0f;
            plan.EndpointConflictGuardVictimMinimumScale = 0f;
            plan.EndpointConflictGuardForeignMinimumScale = 0f;
            plan.EndpointConflictGuardVictimRetreatCapacity = 0f;
            plan.EndpointConflictGuardForeignRetreatCapacity = 0f;
            plan.EndpointConflictGuardClusterEdges = Array.Empty<int>();
            plan.EndpointConflictGuardFalseNegative = false;
            plan.EndpointConflictGuardDiagnostic = string.Empty;
            plan.EndpointPatchRecoveryAttempted = false;
            plan.EndpointPatchRecoveryPrepared = false;
            plan.EndpointPatchRecoveryApplied = false;
            plan.EndpointPatchRecoveryFalsePositive = false;
            plan.EndpointPatchRecoveryAttemptCount = 0;
            plan.EndpointPatchRecoveryTrialCount = 0;
            plan.EndpointPatchRecoveryVertexIndex = -1;
            plan.EndpointPatchRecoveryVictimEdgeIndex = -1;
            plan.EndpointPatchRecoveryForeignEdgeIndex = -1;
            plan.EndpointPatchRecoveryIncidentBandCount = 0;
            plan.EndpointPatchRecoveryNormalRank = -1;
            plan.EndpointPatchRecoveryCapVertexCount = 0;
            plan.EndpointPatchRecoveryCutDepth = 0f;
            plan.EndpointPatchRecoveryCompactness = 0f;
            plan.EndpointPatchRecoveryAspectRatio = 0f;
            plan.EndpointPatchRecoveryMilliseconds = 0d;
            plan.EndpointPatchRecoveryRejection =
                PlaneCutEndpointPatchRejectionKind.None;
            plan.EndpointPatchRecoverySelectedFaceCount = 0;
            plan.EndpointPatchRecoveryBoundaryVertexCount = 0;
            plan.EndpointPatchRecoveryBoundarySignature = string.Empty;
            plan.EndpointPatchRecoveryMaximumRemovedVertexRadius = 0f;
            plan.EndpointPatchRecoveryMaximumIntersectionRadius = 0f;
            plan.EndpointPatchRecoveryMaximumReplacementVertexRadius = 0f;
            plan.EndpointPatchRecoveryRetainedOutsideRadiusCount = 0;
            plan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter = 0;
            plan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter = 0;
            plan.EndpointPatchRecoveryLocalityFailureSource = string.Empty;
            plan.EndpointPatchRecoveryDiagnostic = string.Empty;
            if (plan.SolvedPlan != null)
            {
                plan.SolvedPlan.PreparedEndpointPatch = null;
                if (plan.SolvedPlan.PreparedJunctions != null)
                {
                    plan.SolvedPlan.PreparedJunctions.Clear();
                }
            }

            try
            {
                PlaneCutBevelSolvedPlan solvedPlan = plan.SolvedPlan;
                if (solvedPlan == null || !solvedPlan.SolveValid ||
                    solvedPlan.Context == null ||
                    solvedPlan.Context.Graph == null ||
                    solvedPlan.SourceFaces == null ||
                    solvedPlan.RetainedCandidates == null ||
                    solvedPlan.RetainedCandidates.Count < 2)
                {
                    plan.EndpointConflictGuardDiagnostic =
                        "endpoint-conflict guard was inconclusive because prepared plane state was unavailable; authoritative build required";
                    return true;
                }

                List<PlaneCutBevelCandidate> orderedCandidates =
                    new List<PlaneCutBevelCandidate>(
                        solvedPlan.RetainedCandidates);
                orderedCandidates.Sort((left, right) =>
                    left.SourceEdgeIndex.CompareTo(right.SourceEdgeIndex));
                ChamferTopologyContext context = solvedPlan.Context;
                EdgeWearTopologyGraph graph = context.Graph;
                float pointTolerance = Mathf.Max(
                    PointMergeDistance * 4f,
                    solvedPlan.MinimumStableEdgeLength * 0.01f);
                float strictInteriorClearance = Mathf.Max(
                    pointTolerance * 2f,
                    solvedPlan.MinimumStableEdgeLength * 0.02f);

                for (int victimIndex = 0;
                     victimIndex < orderedCandidates.Count;
                     victimIndex++)
                {
                    PlaneCutBevelCandidate victim =
                        orderedCandidates[victimIndex];
                    if (victim.SourceEdgeIndex < 0 ||
                        victim.SourceEdgeIndex >= graph.Edges.Count)
                    {
                        continue;
                    }
                    EdgeWearGraphEdge victimGraphEdge =
                        graph.Edges[victim.SourceEdgeIndex];
                    if (victimGraphEdge.FaceA < 0 ||
                        victimGraphEdge.FaceB < 0 ||
                        victimGraphEdge.FaceA >= graph.Faces.Count ||
                        victimGraphEdge.FaceB >= graph.Faces.Count ||
                        victimGraphEdge.FaceA == victimGraphEdge.FaceB)
                    {
                        continue;
                    }
                    if (victimGraphEdge.VertexA < 0 ||
                        victimGraphEdge.VertexB < 0 ||
                        victimGraphEdge.VertexA >= graph.Vertices.Count ||
                        victimGraphEdge.VertexB >= graph.Vertices.Count)
                    {
                        continue;
                    }
                    Vector3 sourceA =
                        graph.Vertices[victimGraphEdge.VertexA].Position;
                    Vector3 sourceB =
                        graph.Vertices[victimGraphEdge.VertexB].Position;
                    Vector3 sourceAxis = sourceB - sourceA;
                    float sourceLength = sourceAxis.magnitude;
                    if (sourceLength <= pointTolerance)
                    {
                        continue;
                    }
                    sourceAxis /= sourceLength;

                    for (int foreignIndex = 0;
                         foreignIndex < orderedCandidates.Count;
                         foreignIndex++)
                    {
                        PlaneCutBevelCandidate foreign =
                            orderedCandidates[foreignIndex];
                        if (foreign.SourceEdgeIndex ==
                            victim.SourceEdgeIndex)
                        {
                            continue;
                        }

                        int[] clusterEdges =
                            BuildCornerDamageEndpointConflictCluster(
                                orderedCandidates,
                                victim,
                                foreign);
                        if (clusterEdges.Length == 0)
                        {
                            continue;
                        }
                        HashSet<int> clusterSet = new HashSet<int>(
                            clusterEdges);
                        Dictionary<int, float> scaleByEdge =
                            new Dictionary<int, float>(
                                orderedCandidates.Count);
                        for (int candidateIndex = 0;
                             candidateIndex < orderedCandidates.Count;
                             candidateIndex++)
                        {
                            PlaneCutBevelCandidate candidate =
                                orderedCandidates[candidateIndex];
                            scaleByEdge[candidate.SourceEdgeIndex] =
                                clusterSet.Contains(
                                    candidate.SourceEdgeIndex)
                                    ? ResolvePlaneCutCandidateMinimumScale(
                                        candidate,
                                        solvedPlan.MinimumStableEdgeLength)
                                    : 1f;
                        }
                        List<PlaneCutBevelCandidate> scaledCandidates =
                            BuildScaledPlaneCutCandidates(
                                orderedCandidates,
                                context,
                                scaleByEdge,
                                solvedPlan.MinimumStableEdgeLength);
                        if (!TryFindPlaneCutCandidateBySourceEdge(
                                scaledCandidates,
                                victim.SourceEdgeIndex,
                                out PlaneCutBevelCandidate scaledVictim) ||
                            !TryFindPlaneCutCandidateBySourceEdge(
                                scaledCandidates,
                                foreign.SourceEdgeIndex,
                                out PlaneCutBevelCandidate scaledForeign))
                        {
                            continue;
                        }

                        PolygonFace ownerFaceA = graph.Faces[
                            victimGraphEdge.FaceA].SourceFace;
                        PolygonFace ownerFaceB = graph.Faces[
                            victimGraphEdge.FaceB].SourceFace;
                        if (!TryResolveCornerDamageFacePlane(
                                ownerFaceA,
                                out CutPlane ownerPlaneA) ||
                            !TryResolveCornerDamageFacePlane(
                                ownerFaceB,
                                out CutPlane ownerPlaneB) ||
                            !TryIntersectCornerDamagePlanes(
                                scaledVictim.Plane,
                                scaledForeign.Plane,
                                ownerPlaneA,
                                out Vector3 segmentStart) ||
                            !TryIntersectCornerDamagePlanes(
                                scaledVictim.Plane,
                                scaledForeign.Plane,
                                ownerPlaneB,
                                out Vector3 segmentEnd))
                        {
                            continue;
                        }
                        if (!TryClipCornerDamageEndpointConflictSegmentToShell(
                                ref segmentStart,
                                ref segmentEnd,
                                solvedPlan.SourceFaces,
                                scaledCandidates,
                                scaledVictim.SourceEdgeIndex,
                                scaledForeign.SourceEdgeIndex,
                                pointTolerance,
                                strictInteriorClearance))
                        {
                            continue;
                        }

                        Vector3 segmentMidpoint =
                            (segmentStart + segmentEnd) * 0.5f;
                        plan.EndpointConflictGuardTestedRailCount++;
                        float startParameter = Vector3.Dot(
                            segmentStart - sourceA,
                            sourceAxis) / sourceLength;
                        float endParameter = Vector3.Dot(
                            segmentEnd - sourceA,
                            sourceAxis) / sourceLength;
                        float midpointParameter =
                            (startParameter + endParameter) * 0.5f;
                        float endpointAllowance = Mathf.Clamp(
                            Mathf.Max(
                                scaledVictim.Width * 4f,
                                solvedPlan.MinimumStableEdgeLength * 0.5f) /
                                sourceLength,
                            0.03f,
                            0.25f);
                        float axialTolerance = Mathf.Max(
                            0.0001f,
                            pointTolerance / sourceLength);
                        if (midpointParameter <=
                                endpointAllowance + axialTolerance ||
                            midpointParameter >=
                                1f - endpointAllowance - axialTolerance)
                        {
                            continue;
                        }

                        float victimMinimumScale = scaleByEdge[
                            victim.SourceEdgeIndex];
                        float foreignMinimumScale = scaleByEdge[
                            foreign.SourceEdgeIndex];
                        plan.EndpointConflictGuardConflictCount = 1;
                        plan.EndpointConflictGuardVictimEdgeIndex =
                            victim.SourceEdgeIndex;
                        plan.EndpointConflictGuardForeignEdgeIndex =
                            foreign.SourceEdgeIndex;
                        plan.EndpointConflictGuardAxialParameter =
                            midpointParameter;
                        plan.EndpointConflictGuardEndpointAllowance =
                            endpointAllowance;
                        plan.EndpointConflictGuardVictimMinimumScale =
                            victimMinimumScale;
                        plan.EndpointConflictGuardForeignMinimumScale =
                            foreignMinimumScale;
                        plan.EndpointConflictGuardVictimRetreatCapacity =
                            Mathf.Clamp01(1f - victimMinimumScale);
                        plan.EndpointConflictGuardForeignRetreatCapacity =
                            Mathf.Clamp01(1f - foreignMinimumScale);
                        plan.EndpointConflictGuardClusterEdges = clusterEdges;
                        int conflictVertexIndex = midpointParameter < 0.5f
                            ? victimGraphEdge.VertexA
                            : victimGraphEdge.VertexB;
                        bool recoveryPrepared =
                            TryPrepareCornerDamageBevelTerminationRecovery(
                                plan,
                                orderedCandidates,
                                scaledCandidates,
                                victim,
                                foreign,
                                conflictVertexIndex);
                        if (recoveryPrepared)
                        {
                            plan.EndpointConflictGuardPassed = true;
                            plan.EndpointConflictGuardDiagnostic =
                                "minimum-width endpoint conflict recovered by conflict-local bevel termination";
                            return true;
                        }

                        plan.EndpointConflictGuardPassed = false;
                        plan.EndpointConflictGuardDiagnostic =
                            "minimum-width endpoint-conflict guard proved that foreign generated plane EdgeBevelPlane:" +
                            foreign.SourceEdgeIndex +
                            " still splits bevel-band edge " +
                            victim.SourceEdgeIndex +
                            " at axial parameter " +
                            midpointParameter.ToString("G6") +
                            " beyond endpoint allowance " +
                            endpointAllowance.ToString("G6") +
                            "; local conflict cluster exhausted legal retreat; conflict-local bevel termination: " +
                            plan.EndpointPatchRecoveryDiagnostic;
                        return false;
                    }
                }

                plan.EndpointConflictGuardDiagnostic =
                    "minimum-width foreign-plane endpoint-conflict guard passed";
                return true;
            }
            finally
            {
                stopwatch.Stop();
                plan.EndpointConflictGuardMilliseconds =
                    stopwatch.Elapsed.TotalMilliseconds;
            }
        }

        private static int[] BuildCornerDamageEndpointConflictCluster(
            List<PlaneCutBevelCandidate> candidates,
            PlaneCutBevelCandidate victim,
            PlaneCutBevelCandidate foreign)
        {
            HashSet<int> clusterVertices = new HashSet<int>
            {
                victim.VertexA,
                victim.VertexB,
                foreign.VertexA,
                foreign.VertexB
            };
            SortedSet<int> clusterEdges = new SortedSet<int>
            {
                victim.SourceEdgeIndex,
                foreign.SourceEdgeIndex
            };
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate =
                    candidates[candidateIndex];
                if (clusterVertices.Contains(candidate.VertexA) ||
                    clusterVertices.Contains(candidate.VertexB))
                {
                    clusterEdges.Add(candidate.SourceEdgeIndex);
                }
            }
            int[] result = new int[clusterEdges.Count];
            clusterEdges.CopyTo(result);
            return result;
        }

        private static bool TryResolveCornerDamageFacePlane(
            PolygonFace face,
            out CutPlane plane)
        {
            plane = default;
            if (face == null || face.Vertices == null ||
                face.Vertices.Count == 0 ||
                !IsFiniteCornerDamageVector(face.Normal) ||
                face.Normal.sqrMagnitude <= 0.000001f)
            {
                return false;
            }
            Vector3 normal = face.Normal.normalized;
            plane = new CutPlane(
                normal,
                Vector3.Dot(normal, face.Vertices[0]));
            return true;
        }

        private static bool TryIntersectCornerDamagePlanes(
            CutPlane first,
            CutPlane second,
            CutPlane third,
            out Vector3 intersection)
        {
            intersection = Vector3.zero;
            Vector3 secondCrossThird = Vector3.Cross(
                second.Normal,
                third.Normal);
            float denominator = Vector3.Dot(
                first.Normal,
                secondCrossThird);
            if (Mathf.Abs(denominator) <= 0.000001f)
            {
                return false;
            }
            intersection =
                (secondCrossThird * first.Distance +
                 Vector3.Cross(third.Normal, first.Normal) *
                    second.Distance +
                 Vector3.Cross(first.Normal, second.Normal) *
                    third.Distance) /
                denominator;
            return IsFiniteCornerDamageVector(intersection);
        }

        private static bool TryClipCornerDamageEndpointConflictSegmentToShell(
            ref Vector3 segmentStart,
            ref Vector3 segmentEnd,
            List<PolygonFace> sourceFaces,
            List<PlaneCutBevelCandidate> candidates,
            int victimEdgeIndex,
            int foreignEdgeIndex,
            float tolerance,
            float strictInteriorClearance)
        {
            if (!IsFiniteCornerDamageVector(segmentStart) ||
                !IsFiniteCornerDamageVector(segmentEnd) ||
                sourceFaces == null || candidates == null)
            {
                return false;
            }

            Vector3 originalStart = segmentStart;
            Vector3 direction = segmentEnd - segmentStart;
            if (direction.sqrMagnitude <= tolerance * tolerance)
            {
                return false;
            }
            float minimumParameter = 0f;
            float maximumParameter = 1f;
            for (int faceIndex = 0;
                 faceIndex < sourceFaces.Count;
                 faceIndex++)
            {
                if (!TryResolveCornerDamageFacePlane(
                        sourceFaces[faceIndex],
                        out CutPlane facePlane) ||
                    !TryClipCornerDamageEndpointConflictParameterRange(
                        facePlane,
                        originalStart,
                        direction,
                        tolerance,
                        ref minimumParameter,
                        ref maximumParameter))
                {
                    return false;
                }
            }
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                if (candidate.SourceEdgeIndex == victimEdgeIndex ||
                    candidate.SourceEdgeIndex == foreignEdgeIndex)
                {
                    continue;
                }
                if (!TryClipCornerDamageEndpointConflictParameterRange(
                        candidate.Plane,
                        originalStart,
                        direction,
                        tolerance,
                        ref minimumParameter,
                        ref maximumParameter))
                {
                    return false;
                }
            }
            if (maximumParameter - minimumParameter <= 0.000001f)
            {
                return false;
            }

            segmentStart = originalStart + direction * minimumParameter;
            segmentEnd = originalStart + direction * maximumParameter;
            if ((segmentEnd - segmentStart).sqrMagnitude <=
                tolerance * tolerance)
            {
                return false;
            }

            Vector3 midpoint = (segmentStart + segmentEnd) * 0.5f;
            for (int candidateIndex = 0;
                 candidateIndex < candidates.Count;
                 candidateIndex++)
            {
                PlaneCutBevelCandidate candidate = candidates[candidateIndex];
                if (candidate.SourceEdgeIndex == victimEdgeIndex ||
                    candidate.SourceEdgeIndex == foreignEdgeIndex)
                {
                    continue;
                }
                if (candidate.Plane.SignedDistance(midpoint) >
                    -strictInteriorClearance)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool TryClipCornerDamageEndpointConflictParameterRange(
            CutPlane plane,
            Vector3 origin,
            Vector3 direction,
            float tolerance,
            ref float minimumParameter,
            ref float maximumParameter)
        {
            float originDistance = plane.SignedDistance(origin);
            float directionDistance = Vector3.Dot(
                plane.Normal,
                direction);
            if (Mathf.Abs(directionDistance) <= 0.000001f)
            {
                return originDistance <= tolerance;
            }

            float intersectionParameter =
                (tolerance - originDistance) / directionDistance;
            if (directionDistance > 0f)
            {
                maximumParameter = Mathf.Min(
                    maximumParameter,
                    intersectionParameter);
            }
            else
            {
                minimumParameter = Mathf.Max(
                    minimumParameter,
                    intersectionParameter);
            }
            return minimumParameter <= maximumParameter + 0.000001f;
        }

        private static bool IsFiniteCornerDamageVector(Vector3 value)
        {
            return !float.IsNaN(value.x) &&
                !float.IsInfinity(value.x) &&
                !float.IsNaN(value.y) &&
                !float.IsInfinity(value.y) &&
                !float.IsNaN(value.z) &&
                !float.IsInfinity(value.z);
        }

        private static bool TryCompleteCornerDamageIntegrationPlan(
            CornerDamageIntegrationPlan plan,
            int expectedMandatoryCount,
            out double elapsedMilliseconds,
            out string failureStage)
        {
            elapsedMilliseconds = 0d;
            failureStage = string.Empty;
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (plan == null || !plan.Valid || plan.SolvedPlan == null)
                {
                    if (plan != null)
                    {
                        plan.Diagnostic =
                            "candidate preparation result was unavailable";
                        plan.Valid = false;
                    }
                    failureStage = "candidate-preparation";
                    return false;
                }
                if (IsCornerDamageSearchDeadlineExceeded())
                {
                    plan.Diagnostic =
                        "corner search deadline exceeded before the complete authoritative build";
                    plan.Valid = false;
                    failureStage = "performance-budget";
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
                // C1B.1: materialization is certified against the
                // rebuilt post-chip topology only. Historical edge retention
                // is neither required nor meaningful.
                List<int> lost = new List<int>();
                int unrelatedBaselineCount = 0;
                int unrelatedRetainedCount = 0;

                plan.PreviewSoup = previewSoup;
                plan.UnifiedStatus = unifiedStatus;
                plan.PlaneAudit = audit;
                plan.PlannedOrdinaryIdentities = finalOrdinary;
                plan.PlannedMandatoryIdentities = finalMandatory;
                plan.IntegrationPlanHash =
                    BuildCornerDamageIntegrationPlanHash(
                        plan.Transaction,
                        plan.ResolvedUniformScale,
                        finalOrdinary,
                        finalMandatory);
                plan.EmittedPlanHash = string.Empty;
                plan.MissingPlannedOrdinary = Array.Empty<int>();
                plan.UnexpectedFinalOrdinary = Array.Empty<int>();
                plan.MissingPlannedMandatory = Array.Empty<int>();
                plan.UnexpectedFinalMandatory = Array.Empty<int>();
                plan.UnrelatedBaselineCount = unrelatedBaselineCount;
                plan.UnrelatedRetainedCount = unrelatedRetainedCount;
                plan.CollateralLostCount = lost.Count;
                plan.CollateralLostIdentities = lost.ToArray();
                plan.EndpointPatchRecoveryApplied =
                    previewApplied &&
                    plan.EndpointPatchRecoveryPrepared &&
                    plan.SolvedPlan.PreparedEndpointPatch != null &&
                    plan.SolvedPlan.PreparedJunctions != null &&
                    plan.SolvedPlan.PreparedJunctions.Count == 1;

                bool endpointConflictFloorFailure =
                    audit.EdgeConflictWidthReductions != null &&
                    audit.EdgeConflictWidthReductions.Count > 0 &&
                    string.Equals(
                        audit.EdgeConflictWidthReductions[
                            audit.EdgeConflictWidthReductions.Count - 1].Result,
                        "unresolved-geometric-floor",
                        StringComparison.Ordinal);
                if (!previewApplied &&
                    plan.EndpointConflictGuardAttempted &&
                    plan.EndpointConflictGuardPassed &&
                    endpointConflictFloorFailure &&
                    audit.EdgeConflictUnresolvedCount > 0 &&
                    audit.EdgeConflictVictimEdgeIndex >= 0 &&
                    audit.EdgeConflictForeignEdgeIndex >= 0)
                {
                    plan.EndpointConflictGuardFalseNegative = true;
                    plan.EndpointConflictGuardVictimEdgeIndex =
                        audit.EdgeConflictVictimEdgeIndex;
                    plan.EndpointConflictGuardForeignEdgeIndex =
                        audit.EdgeConflictForeignEdgeIndex;
                    plan.EndpointConflictGuardAxialParameter =
                        audit.EdgeConflictForeignAxialParameter;
                    plan.EndpointConflictGuardDiagnostic =
                        "endpoint-conflict guard false negative: " +
                        audit.Diagnostic;
                }

                if (!previewApplied &&
                    plan.EndpointPatchRecoveryPrepared)
                {
                    plan.EndpointPatchRecoveryFalsePositive = true;
                    plan.EndpointPatchRecoveryRejection =
                        PlaneCutEndpointPatchRejectionKind.MaterializationSignature;
                    plan.EndpointPatchRecoveryMaterializationSignatureCount++;
                    plan.EndpointPatchRecoveryDiagnostic =
                        "prepared conflict-local bevel termination failed authoritative materialization: " +
                        (audit.Diagnostic ?? string.Empty);
                }

                if (!previewApplied)
                {
                    plan.Diagnostic = string.IsNullOrEmpty(audit.Diagnostic)
                        ? "complete authoritative shell build failed"
                        : audit.Diagnostic;
                    plan.Valid = false;
                    failureStage = "complete-authoritative-build";
                    return false;
                }
                if (finalMandatory.Length != expectedMandatoryCount)
                {
                    plan.Diagnostic =
                        "completed authoritative shell has an incomplete mandatory cap ring";
                    plan.Valid = false;
                    failureStage = "complete-build-mandatory";
                    return false;
                }
                if (lost.Count > 0)
                {
                    plan.Diagnostic =
                        "completed authoritative shell loses unrelated baseline bevel identities";
                    plan.Valid = false;
                    failureStage = "complete-build-retention";
                    return false;
                }

                plan.Valid = true;
                plan.Diagnostic =
                    "complete authoritative corner-integration shell certified";
                return true;
            }
            catch (InvalidOperationException exception)
            {
                if (plan != null)
                {
                    plan.Valid = false;
                    plan.Diagnostic =
                        "complete authoritative build exception: " +
                        exception.Message;
                }
                failureStage = "complete-authoritative-build";
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
                clone.AddTriangleWithSurfaceContributions(
                    source.Positions[vertexIndex],
                    source.Positions[vertexIndex + 1],
                    source.Positions[vertexIndex + 2],
                    source.ResolveFeature(vertexIndex),
                    source.ResolveFeatureStrength(vertexIndex),
                    normal,
                    surfaceGroup,
                    source.ResolvePrimarySurfaceContribution(vertexIndex),
                    source.ResolveSecondarySurfaceContribution(vertexIndex),
                    source.TryResolveProvenance(
                        vertexIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out int provenanceIndex)
                            ? provenanceKind
                            : PolygonFaceProvenanceKind.None,
                    provenanceIndex);
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
            status.PreparedPlanHash = plan.PreparedPlanHash;
            status.IntegrationPlanHash = plan.IntegrationPlanHash;
            status.EmittedPlanHash = plan.EmittedPlanHash;
            status.PreparedOrdinaryIdentities =
                plan.PreparedOrdinaryIdentities ?? Array.Empty<int>();
            status.PreparedMandatoryIdentities =
                plan.PreparedMandatoryIdentities ?? Array.Empty<int>();
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
            status.EndpointConflictGuardAttempted =
                plan.EndpointConflictGuardAttempted;
            status.EndpointConflictGuardPassed =
                plan.EndpointConflictGuardPassed;
            status.EndpointConflictGuardTestedRailCount =
                plan.EndpointConflictGuardTestedRailCount;
            status.EndpointConflictGuardConflictCount =
                plan.EndpointConflictGuardConflictCount;
            status.EndpointConflictGuardVictimEdgeIndex =
                plan.EndpointConflictGuardVictimEdgeIndex;
            status.EndpointConflictGuardForeignEdgeIndex =
                plan.EndpointConflictGuardForeignEdgeIndex;
            status.EndpointConflictGuardAxialParameter =
                plan.EndpointConflictGuardAxialParameter;
            status.EndpointConflictGuardEndpointAllowance =
                plan.EndpointConflictGuardEndpointAllowance;
            status.EndpointConflictGuardVictimMinimumScale =
                plan.EndpointConflictGuardVictimMinimumScale;
            status.EndpointConflictGuardForeignMinimumScale =
                plan.EndpointConflictGuardForeignMinimumScale;
            status.EndpointConflictGuardVictimRetreatCapacity =
                plan.EndpointConflictGuardVictimRetreatCapacity;
            status.EndpointConflictGuardForeignRetreatCapacity =
                plan.EndpointConflictGuardForeignRetreatCapacity;
            status.EndpointConflictGuardClusterEdges =
                plan.EndpointConflictGuardClusterEdges ?? Array.Empty<int>();
            status.EndpointConflictGuardMilliseconds =
                plan.EndpointConflictGuardMilliseconds;
            status.EndpointConflictGuardFalseNegative =
                plan.EndpointConflictGuardFalseNegative;
            status.EndpointConflictGuardDiagnostic =
                plan.EndpointConflictGuardDiagnostic ?? string.Empty;
            status.EndpointPatchRecoveryAttempted =
                plan.EndpointPatchRecoveryAttempted;
            status.EndpointPatchRecoveryPrepared =
                plan.EndpointPatchRecoveryPrepared;
            status.EndpointPatchRecoveryApplied =
                plan.EndpointPatchRecoveryApplied;
            status.EndpointPatchRecoveryFalsePositive =
                plan.EndpointPatchRecoveryFalsePositive;
            status.EndpointPatchRecoveryLocalAttemptCount =
                plan.EndpointPatchRecoveryTrialCount;
            status.EndpointPatchRecoveryVertexIndex =
                plan.EndpointPatchRecoveryVertexIndex;
            status.EndpointPatchRecoveryVictimEdgeIndex =
                plan.EndpointPatchRecoveryVictimEdgeIndex;
            status.EndpointPatchRecoveryForeignEdgeIndex =
                plan.EndpointPatchRecoveryForeignEdgeIndex;
            status.EndpointPatchRecoveryIncidentBandCount =
                plan.EndpointPatchRecoveryIncidentBandCount;
            status.EndpointPatchRecoveryNormalRank =
                plan.EndpointPatchRecoveryNormalRank;
            status.EndpointPatchRecoveryCapVertexCount =
                plan.EndpointPatchRecoveryCapVertexCount;
            status.EndpointPatchRecoveryCutDepth =
                plan.EndpointPatchRecoveryCutDepth;
            status.EndpointPatchRecoveryCompactness =
                plan.EndpointPatchRecoveryCompactness;
            status.EndpointPatchRecoveryAspectRatio =
                plan.EndpointPatchRecoveryAspectRatio;
            status.EndpointPatchRecoveryLocalMilliseconds =
                plan.EndpointPatchRecoveryMilliseconds;
            status.EndpointPatchRecoveryRejection =
                plan.EndpointPatchRecoveryRejection.ToString();
            status.EndpointPatchRecoverySelectedFaceCount =
                plan.EndpointPatchRecoverySelectedFaceCount;
            status.EndpointPatchRecoveryBoundaryVertexCount =
                plan.EndpointPatchRecoveryBoundaryVertexCount;
            status.EndpointPatchRecoveryBoundarySignature =
                plan.EndpointPatchRecoveryBoundarySignature ?? string.Empty;
            status.EndpointPatchRecoveryLocalMaximumRemovedVertexRadius =
                plan.EndpointPatchRecoveryMaximumRemovedVertexRadius;
            status.EndpointPatchRecoveryLocalMaximumIntersectionRadius =
                plan.EndpointPatchRecoveryMaximumIntersectionRadius;
            status.EndpointPatchRecoveryLocalMaximumReplacementVertexRadius =
                plan.EndpointPatchRecoveryMaximumReplacementVertexRadius;
            status.EndpointPatchRecoveryLocalRetainedOutsideRadiusCount =
                plan.EndpointPatchRecoveryRetainedOutsideRadiusCount;
            status.EndpointPatchRecoveryLocalSelectedFaceCountBeforeLocalFilter =
                plan.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter;
            status.EndpointPatchRecoveryLocalSelectedFaceCountAfterLocalFilter =
                plan.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter;
            status.EndpointPatchRecoveryLocalityFailureSource =
                plan.EndpointPatchRecoveryLocalityFailureSource ?? string.Empty;
            status.EndpointPatchRecoveryAttemptSupportSampleCount =
                plan.EndpointPatchRecoveryLocalSupportSampleCount;
            status.EndpointPatchRecoveryAttemptMinimumSamplesPerIncident =
                plan.EndpointPatchRecoveryMinimumSamplesPerIncident;
            status.EndpointPatchRecoverySamplesPerIncident =
                plan.EndpointPatchRecoverySamplesPerIncident ?? string.Empty;
            status.EndpointPatchRecoveryLocalSupportRadius =
                plan.EndpointPatchRecoveryLocalSupportRadius;
            status.EndpointPatchRecoveryLocalSupportProjection =
                plan.EndpointPatchRecoveryLocalSupportProjection;
            status.EndpointPatchRecoveryGlobalSupportProjection =
                plan.EndpointPatchRecoveryGlobalSupportProjection;
            status.EndpointPatchRecoveryGlobalMinusLocalSupportDelta =
                plan.EndpointPatchRecoveryGlobalMinusLocalSupportDelta;
            status.EndpointPatchRecoveryControllingSupportEdgeIndex =
                plan.EndpointPatchRecoveryControllingSupportEdgeIndex;
            status.EndpointPatchRecoveryControllingSupportRadius =
                plan.EndpointPatchRecoveryControllingSupportRadius;
            status.EndpointPatchRecoverySupportFailureSource =
                plan.EndpointPatchRecoverySupportFailureSource ?? string.Empty;
            status.EndpointPatchRecoveryAttemptMaximumAxialInfluence =
                plan.EndpointPatchRecoveryMaximumAxialInfluence;
            status.EndpointPatchRecoveryAttemptMinimumAllowedAxialInfluence =
                plan.EndpointPatchRecoveryMinimumAllowedAxialInfluence;
            status.EndpointPatchRecoveryAxialRejectedEdgeIndex =
                plan.EndpointPatchRecoveryAxialRejectedEdgeIndex;
            status.EndpointPatchRecoveryAxialRejectedEndpointVertexIndex =
                plan.EndpointPatchRecoveryAxialRejectedEndpointVertexIndex;
            status.EndpointPatchRecoveryAxialInfluenceSignature =
                plan.EndpointPatchRecoveryAxialInfluenceSignature ?? string.Empty;
            status.EndpointPatchRecoveryCellLimitSignature =
                plan.EndpointPatchRecoveryCellLimitSignature ?? string.Empty;
            status.EndpointPatchRecoveryAttemptFacesSubdivided =
                plan.EndpointPatchRecoveryFacesSubdivided;
            status.EndpointPatchRecoveryAttemptLocalFragmentCount =
                plan.EndpointPatchRecoveryLocalFragmentCount;
            status.EndpointPatchRecoveryAttemptRemoteRemainderCount =
                plan.EndpointPatchRecoveryRemoteRemainderCount;
            status.EndpointPatchRecoveryAttemptSyntheticIncidentFragmentCount =
                plan.EndpointPatchRecoverySyntheticIncidentFragmentCount;
            status.EndpointPatchRecoverySyntheticIncidentIdentities =
                plan.EndpointPatchRecoverySyntheticIncidentIdentities ?? string.Empty;
            status.EndpointPatchRecoveryAttemptCellVertexCount =
                plan.EndpointPatchRecoveryCellVertexCount;
            status.EndpointPatchRecoveryAttemptCellFaceCount =
                plan.EndpointPatchRecoveryCellFaceCount;
            status.EndpointPatchRecoveryCellSplitSignature =
                plan.EndpointPatchRecoveryCellSplitSignature ?? string.Empty;
            status.EndpointPatchRecoveryLocalFragmentSignature =
                plan.EndpointPatchRecoveryLocalFragmentSignature ?? string.Empty;
            status.EndpointPatchRecoveryRemoteRemainderSignature =
                plan.EndpointPatchRecoveryRemoteRemainderSignature ?? string.Empty;
            status.EndpointPatchRecoveryCellFailureSource =
                plan.EndpointPatchRecoveryCellFailureSource ?? string.Empty;
            status.EndpointPatchRecoveryBoundaryComponentCount =
                plan.EndpointPatchRecoveryBoundaryComponentCount;
            status.EndpointPatchRecoveryClosedCycleCount =
                plan.EndpointPatchRecoveryClosedCycleCount;
            status.EndpointPatchRecoveryOpenChainCount =
                plan.EndpointPatchRecoveryOpenChainCount;
            status.EndpointPatchRecoveryBranchVertexCount =
                plan.EndpointPatchRecoveryBranchVertexCount;
            status.EndpointPatchRecoveryTransitionFaceCount =
                plan.EndpointPatchRecoveryTransitionFaceCount;
            status.EndpointPatchRecoveryResidualOpenEdgeCount =
                plan.EndpointPatchRecoveryResidualOpenEdgeCount;
            status.EndpointPatchRecoveryMechanismSignature =
                plan.EndpointPatchRecoveryMechanismSignature ?? string.Empty;
            status.EndpointPatchRecoveryModifiedIdentitySignature =
                plan.EndpointPatchRecoveryModifiedIdentitySignature ?? string.Empty;
            status.EndpointPatchRecoveryDiagnostic =
                plan.EndpointPatchRecoveryDiagnostic ?? string.Empty;
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
            string failureStage,
            ref CornerDamagePreviewStatus bestFailure,
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
                EndpointConflictGuardAttemptCount = status == null
                    ? 0
                    : status.EndpointConflictGuardAttemptCount,
                EndpointConflictGuardPassCount = status == null
                    ? 0
                    : status.EndpointConflictGuardPassCount,
                EndpointConflictGuardRejectCount = status == null
                    ? 0
                    : status.EndpointConflictGuardRejectCount,
                EndpointConflictGuardFalseNegativeCount = status == null
                    ? 0
                    : status.EndpointConflictGuardFalseNegativeCount,
                EndpointConflictGuardTestedRailCount = status == null
                    ? 0
                    : status.EndpointConflictGuardTestedRailCount,
                EndpointConflictGuardMilliseconds = status == null
                    ? 0d
                    : status.EndpointConflictGuardMilliseconds,
                EndpointPatchRecoveryAttemptCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryAttemptCount,
                EndpointPatchRecoveryPreparedCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryPreparedCount,
                EndpointPatchRecoveryRejectCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryRejectCount,
                EndpointPatchRecoveryAppliedCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryAppliedCount,
                EndpointPatchRecoveryFalsePositiveCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryFalsePositiveCount,
                EndpointPatchRecoveryUnsupportedStarCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryUnsupportedStarCount,
                EndpointPatchRecoveryPatchExtractionCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryPatchExtractionCount,
                EndpointPatchRecoveryDisconnectedPatchCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryDisconnectedPatchCount,
                EndpointPatchRecoveryBoundaryLoopCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryBoundaryLoopCount,
                EndpointPatchRecoveryBoundaryCrossingCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryBoundaryCrossingCount,
                EndpointPatchRecoveryNoLocalRemovalCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryNoLocalRemovalCount,
                EndpointPatchRecoveryCapCreationCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryCapCreationCount,
                EndpointPatchRecoveryIncidentBandJoinCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryIncidentBandJoinCount,
                EndpointPatchRecoveryStitchTopologyCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryStitchTopologyCount,
                EndpointPatchRecoveryLocalityCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryLocalityCount,
                EndpointPatchRecoveryBandIntegrityCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryBandIntegrityCount,
                EndpointPatchRecoveryPreparedMinimumParityCount =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoveryPreparedMinimumParityCount,
                EndpointPatchRecoveryMaterializationSignatureCount =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoveryMaterializationSignatureCount,
                EndpointPatchRecoveryMaximumRemovedVertexRadius =
                    status == null
                        ? 0f
                        : status.EndpointPatchRecoveryMaximumRemovedVertexRadius,
                EndpointPatchRecoveryMaximumIntersectionRadius =
                    status == null
                        ? 0f
                        : status.EndpointPatchRecoveryMaximumIntersectionRadius,
                EndpointPatchRecoveryMaximumReplacementVertexRadius =
                    status == null
                        ? 0f
                        : status.EndpointPatchRecoveryMaximumReplacementVertexRadius,
                EndpointPatchRecoveryRetainedOutsideRadiusCount =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoveryRetainedOutsideRadiusCount,
                EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoverySelectedFaceCountBeforeLocalFilter,
                EndpointPatchRecoverySelectedFaceCountAfterLocalFilter =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoverySelectedFaceCountAfterLocalFilter,
                EndpointPatchRecoveryLocalSupportSampleCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryLocalSupportSampleCount,
                EndpointPatchRecoveryMinimumSamplesPerIncident = status == null
                    ? 0
                    : status.EndpointPatchRecoveryMinimumSamplesPerIncident,
                EndpointPatchRecoveryMaximumGlobalMinusLocalSupportDelta =
                    status == null
                        ? 0f
                        : status.EndpointPatchRecoveryMaximumGlobalMinusLocalSupportDelta,
                EndpointPatchRecoveryMaximumControllingSupportRadius =
                    status == null
                        ? 0f
                        : status.EndpointPatchRecoveryMaximumControllingSupportRadius,
                EndpointPatchRecoveryMaximumAxialInfluence = status == null
                    ? 0f
                    : status.EndpointPatchRecoveryMaximumAxialInfluence,
                EndpointPatchRecoveryMinimumAllowedAxialInfluence =
                    status == null
                        ? float.PositiveInfinity
                        : status.EndpointPatchRecoveryMinimumAllowedAxialInfluence,
                EndpointPatchRecoveryFacesSubdivided = status == null
                    ? 0
                    : status.EndpointPatchRecoveryFacesSubdivided,
                EndpointPatchRecoveryLocalFragmentCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryLocalFragmentCount,
                EndpointPatchRecoveryRemoteRemainderCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryRemoteRemainderCount,
                EndpointPatchRecoverySyntheticIncidentFragmentCount =
                    status == null
                        ? 0
                        : status.EndpointPatchRecoverySyntheticIncidentFragmentCount,
                EndpointPatchRecoveryMaximumCellVertexCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryMaximumCellVertexCount,
                EndpointPatchRecoveryMaximumCellFaceCount = status == null
                    ? 0
                    : status.EndpointPatchRecoveryMaximumCellFaceCount,
                EndpointPatchRecoveryMilliseconds = status == null
                    ? 0d
                    : status.EndpointPatchRecoveryMilliseconds,
                PreflightFoundationBuildCount = status == null
                    ? 0
                    : status.PreflightFoundationBuildCount,
                PreflightFoundationReuseCount = status == null
                    ? 0
                    : status.PreflightFoundationReuseCount,
                IsolatedReplayAttemptCount = status == null
                    ? 0
                    : status.IsolatedReplayAttemptCount,
                IsolatedReplayHitCount = status == null
                    ? 0
                    : status.IsolatedReplayHitCount,
                IsolatedReplayMissCount = status == null
                    ? 0
                    : status.IsolatedReplayMissCount,
                IsolatedFullEvaluationCount = status == null
                    ? 0
                    : status.IsolatedFullEvaluationCount,
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
            if (status.PreviewApplied)
            {
                return "none";
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
                "performance-budget" => 10,
                "integration-plan-mismatch" => 10,
                "plan-emission" => 9,
                "complete-build-retention" => 9,
                "complete-build-mandatory" => 9,
                "complete-authoritative-build" => 8,
                "endpoint-conflict-guard" => 8,
                "candidate-preparation" => 7,
                "integration-plan" => 7,
                "integration-preflight-mismatch" => 7,
                "unrelated-retention" => 6,
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
                    baselineMesh,
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
