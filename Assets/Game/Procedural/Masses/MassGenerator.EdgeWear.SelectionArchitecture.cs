using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Generated Mass incremental selection architecture

        private enum GeneratedMassCornerSelectionStage
        {
            Undiscovered,
            Discovered,
            TopologicallyEligible,
            GeometricallyEligible,
            DepthFeasible,
            IsolatedPreflightCertified,
            CompatibilityCurrent,
            ConflictFree,
            PlanCertified,
            Selected,
            Committed,
            Rejected,
            Removed
        }

        private enum GeneratedMassBevelSelectionStage
        {
            Undiscovered,
            Discovered,
            StructurallyEligible,
            GeometricallyEligible,
            WidthFeasible,
            IsolatedBuildCertified,
            CompatibilityCurrent,
            ConflictFree,
            FullSetDryBuildCertified,
            Selected,
            Committed,
            Rejected,
            Removed
        }

        private enum GeneratedMassLegacyBevelObservedStage
        {
            Discovered,
            StructurallyEligible,
            GeometricallyEligible,
            CoexistenceEligible,
            ArtisticallyEligible,
            Candidate,
            Selected,
            AttemptedBuilt,
            Built,
            Rejected
        }

        private sealed class GeneratedMassSelectionSnapshotSummary
        {
            public int FaceCount;
            public int RawSourceEdgeCount;
            public int SourceEdgeCount;
            public int LifecycleRecordCount;
            public int ReturnedCandidateCount;
            public bool FreshPostChipOrdinaryPass;
        }



        private sealed class GeneratedMassFullRebuildOracleAudit
        {
            public bool Captured;
            public bool Valid;
            public int PrimaryCandidateCount;
            public int RebuildCandidateCount;
            public int PrimaryLifecycleRecordCount;
            public int RebuildLifecycleRecordCount;
            public int CandidateMismatchCount;
            public int LifecycleMismatchCount;
            public ulong PrimaryCandidateHash;
            public ulong RebuildCandidateHash;
            public ulong PrimaryLifecycleHash;
            public ulong RebuildLifecycleHash;
            public string CandidateMismatchIndices = string.Empty;
            public string LifecycleMismatchIndices = string.Empty;
            public string Diagnostic = string.Empty;

            public GeneratedMassFullRebuildOracleAudit Clone()
            {
                return (GeneratedMassFullRebuildOracleAudit)MemberwiseClone();
            }
        }



        private sealed class GeneratedMassIsolatedEligibilityAudit
        {
            public bool Captured;
            public bool Valid;
            public int LifecycleRecordCount;
            public int StructurallyEligibleCount;
            public int GeometricallyEligibleCount;
            public int WidthEvidenceCount;
            public int WidthFeasibleCount;
            public int IsolatedCertifiedCount;
            public int RejectedBeforeWidthCount;
            public int MissingViabilityEvidenceCount;
            public int InvalidWidthIntervalCount;
            public int InconsistentIsolatedEvidenceCount;
            public float MinimumCertifiedWidth;
            public float MaximumCertifiedWidth;
            public string ProblematicSourceEdges = string.Empty;
            public string Diagnostic = string.Empty;

            public GeneratedMassIsolatedEligibilityAudit Clone()
            {
                return (GeneratedMassIsolatedEligibilityAudit)MemberwiseClone();
            }
        }



        private sealed class GeneratedMassPotentialInteractionAudit
        {
            public bool Captured;
            public bool Valid;
            public int CandidateCount;
            public int TotalPairCount;
            public int PotentialPairCount;
            public int DisjointPairCount;
            public int SharedEndpointPairCount;
            public int SharedFacePairCount;
            public int ExpandedBoundsPairCount;
            public int MissingWidthEvidenceCount;
            public int DuplicatePairCount;
            public int MaximumCandidateDegree;
            public ulong RelationHash;
            public string SamplePotentialPairs = string.Empty;
            public string ProblematicCandidates = string.Empty;
            public string Diagnostic = string.Empty;

            public GeneratedMassPotentialInteractionAudit Clone()
            {
                return (GeneratedMassPotentialInteractionAudit)MemberwiseClone();
            }
        }

        private sealed class GeneratedMassPairwiseCompatibilityAudit
        {
            public bool Captured;
            public bool Valid;
            public int CandidateCount;
            public int PotentialPairCount;
            public int EvaluatedPairCount;
            public int CompatiblePairCount;
            public int IncompatiblePairCount;
            public int UnresolvedPairCount;
            public int MissingRelationCount;
            public int DuplicateRelationCount;
            public float MinimumClearance;
            public string IncompatiblePairs = string.Empty;
            public string UnresolvedPairs = string.Empty;
            public string Diagnostic = string.Empty;

            public GeneratedMassPairwiseCompatibilityAudit Clone()
            {
                return (GeneratedMassPairwiseCompatibilityAudit)MemberwiseClone();
            }
        }

        private sealed class GeneratedMassSelectionArchitectureAudit
        {
            public GeneratedMassSelectionSnapshotSummary Snapshot;
            public bool Captured;
            public bool ParityValid;
            public int LifecycleCandidateCount;
            public int ReturnedCandidateCount;
            public int UniqueCandidateIndexCount;
            public int UniqueStableIdentityCount;
            public int MappedReturnedCandidateCount;
            public int UnexpectedReturnedCandidateCount;
            public int MissingReturnedCandidateCount;
            public readonly int[] StageCounts =
                new int[System.Enum.GetValues(
                    typeof(GeneratedMassLegacyBevelObservedStage)).Length];
            public string UnexpectedCandidateIndices = string.Empty;
            public string MissingCandidateIndices = string.Empty;
            public string DuplicateCandidateIndices = string.Empty;
            public string DuplicateStableIdentities = string.Empty;
            public string Diagnostic = string.Empty;
            public GeneratedMassFullRebuildOracleAudit FullRebuildOracle;
            public GeneratedMassIsolatedEligibilityAudit IsolatedEligibility;
            public GeneratedMassPotentialInteractionAudit PotentialInteraction;
            public GeneratedMassPairwiseCompatibilityAudit PairwiseCompatibility;

            public GeneratedMassSelectionArchitectureAudit Clone()
            {
                GeneratedMassSelectionArchitectureAudit clone =
                    new GeneratedMassSelectionArchitectureAudit
                    {
                        Snapshot = Snapshot == null
                            ? null
                            : new GeneratedMassSelectionSnapshotSummary
                            {
                                FaceCount = Snapshot.FaceCount,
                                RawSourceEdgeCount =
                                    Snapshot.RawSourceEdgeCount,
                                SourceEdgeCount = Snapshot.SourceEdgeCount,
                                LifecycleRecordCount =
                                    Snapshot.LifecycleRecordCount,
                                ReturnedCandidateCount =
                                    Snapshot.ReturnedCandidateCount,
                                FreshPostChipOrdinaryPass =
                                    Snapshot.FreshPostChipOrdinaryPass
                            },
                        Captured = Captured,
                        ParityValid = ParityValid,
                        LifecycleCandidateCount = LifecycleCandidateCount,
                        ReturnedCandidateCount = ReturnedCandidateCount,
                        UniqueCandidateIndexCount = UniqueCandidateIndexCount,
                        UniqueStableIdentityCount = UniqueStableIdentityCount,
                        MappedReturnedCandidateCount =
                            MappedReturnedCandidateCount,
                        UnexpectedReturnedCandidateCount =
                            UnexpectedReturnedCandidateCount,
                        MissingReturnedCandidateCount =
                            MissingReturnedCandidateCount,
                        UnexpectedCandidateIndices =
                            UnexpectedCandidateIndices,
                        MissingCandidateIndices = MissingCandidateIndices,
                        DuplicateCandidateIndices =
                            DuplicateCandidateIndices,
                        DuplicateStableIdentities =
                            DuplicateStableIdentities,
                        Diagnostic = Diagnostic,
                        FullRebuildOracle = FullRebuildOracle == null
                            ? null
                            : FullRebuildOracle.Clone(),
                        IsolatedEligibility = IsolatedEligibility == null
                            ? null
                            : IsolatedEligibility.Clone(),
                        PotentialInteraction = PotentialInteraction == null
                            ? null
                            : PotentialInteraction.Clone(),
                        PairwiseCompatibility = PairwiseCompatibility == null
                            ? null
                            : PairwiseCompatibility.Clone()
                    };
                System.Array.Copy(
                    StageCounts,
                    clone.StageCounts,
                    StageCounts.Length);
                return clone;
            }
        }

        private static void CaptureGeneratedMassSelectionArchitectureAudit(
            List<PolygonFace> faces,
            List<EdgeWearBevelCandidate> candidates,
            EdgeWearCoverageAudit coverageAudit,
            bool freshPostChipOrdinaryPass)
        {
            if (coverageAudit == null)
            {
                return;
            }

            GeneratedMassSelectionArchitectureAudit audit =
                new GeneratedMassSelectionArchitectureAudit
                {
                    Captured = true,
                    Snapshot = new GeneratedMassSelectionSnapshotSummary
                    {
                        FaceCount = faces == null ? 0 : faces.Count,
                        RawSourceEdgeCount = coverageAudit.RawSourceEdgeCount,
                        SourceEdgeCount = coverageAudit.SourceEdgeCount,
                        LifecycleRecordCount = coverageAudit.Records.Count,
                        ReturnedCandidateCount =
                            candidates == null ? 0 : candidates.Count,
                        FreshPostChipOrdinaryPass =
                            freshPostChipOrdinaryPass
                    },
                    ReturnedCandidateCount =
                        candidates == null ? 0 : candidates.Count
                };

            Dictionary<int, EdgeWearEdgeLifecycleRecord> lifecycleByCandidate =
                new Dictionary<int, EdgeWearEdgeLifecycleRecord>();
            List<int> duplicateLifecycleCandidateIndices = new List<int>();
            for (int recordIndex = 0;
                 recordIndex < coverageAudit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    coverageAudit.Records[recordIndex];
                GeneratedMassLegacyBevelObservedStage stage =
                    ResolveGeneratedMassLegacyBevelObservedStage(record);
                audit.StageCounts[(int)stage]++;
                if (!record.Candidate)
                {
                    continue;
                }

                audit.LifecycleCandidateCount++;
                if (lifecycleByCandidate.ContainsKey(record.CandidateIndex))
                {
                    duplicateLifecycleCandidateIndices.Add(
                        record.CandidateIndex);
                    continue;
                }
                lifecycleByCandidate.Add(record.CandidateIndex, record);
            }

            HashSet<int> candidateIndices = new HashSet<int>();
            HashSet<int> stableIdentities = new HashSet<int>();
            List<int> duplicateCandidateIndices = new List<int>();
            List<int> duplicateStableIdentities = new List<int>();
            List<int> unexpectedCandidateIndices = new List<int>();
            HashSet<int> mappedLifecycleCandidateIndices = new HashSet<int>();

            if (candidates != null)
            {
                for (int candidateListIndex = 0;
                     candidateListIndex < candidates.Count;
                     candidateListIndex++)
                {
                    EdgeWearBevelCandidate candidate =
                        candidates[candidateListIndex];
                    if (!candidateIndices.Add(candidate.CandidateIndex))
                    {
                        duplicateCandidateIndices.Add(
                            candidate.CandidateIndex);
                    }
                    if (!stableIdentities.Add(candidate.StableIdentity))
                    {
                        duplicateStableIdentities.Add(
                            candidate.StableIdentity);
                    }
                    if (!lifecycleByCandidate.TryGetValue(
                            candidate.CandidateIndex,
                            out EdgeWearEdgeLifecycleRecord lifecycle) ||
                        !lifecycle.Candidate)
                    {
                        unexpectedCandidateIndices.Add(
                            candidate.CandidateIndex);
                        continue;
                    }
                    mappedLifecycleCandidateIndices.Add(
                        candidate.CandidateIndex);
                    audit.MappedReturnedCandidateCount++;
                }
            }

            List<int> missingCandidateIndices = new List<int>();
            foreach (KeyValuePair<int, EdgeWearEdgeLifecycleRecord> pair in
                lifecycleByCandidate)
            {
                if (!mappedLifecycleCandidateIndices.Contains(pair.Key))
                {
                    missingCandidateIndices.Add(pair.Key);
                }
            }

            duplicateLifecycleCandidateIndices.Sort();
            duplicateCandidateIndices.Sort();
            duplicateStableIdentities.Sort();
            unexpectedCandidateIndices.Sort();
            missingCandidateIndices.Sort();

            audit.UniqueCandidateIndexCount = candidateIndices.Count;
            audit.UniqueStableIdentityCount = stableIdentities.Count;
            audit.UnexpectedReturnedCandidateCount =
                unexpectedCandidateIndices.Count;
            audit.MissingReturnedCandidateCount =
                missingCandidateIndices.Count;
            audit.UnexpectedCandidateIndices =
                JoinGeneratedMassSelectionIndices(unexpectedCandidateIndices);
            audit.MissingCandidateIndices =
                JoinGeneratedMassSelectionIndices(missingCandidateIndices);
            audit.DuplicateCandidateIndices =
                JoinGeneratedMassSelectionIndices(
                    duplicateLifecycleCandidateIndices,
                    duplicateCandidateIndices);
            audit.DuplicateStableIdentities =
                JoinGeneratedMassSelectionIndices(
                    duplicateStableIdentities);
            audit.ParityValid =
                audit.LifecycleCandidateCount == audit.ReturnedCandidateCount &&
                audit.UniqueCandidateIndexCount == audit.ReturnedCandidateCount &&
                audit.UniqueStableIdentityCount == audit.ReturnedCandidateCount &&
                audit.MappedReturnedCandidateCount ==
                    audit.ReturnedCandidateCount &&
                audit.UnexpectedReturnedCandidateCount == 0 &&
                audit.MissingReturnedCandidateCount == 0 &&
                duplicateLifecycleCandidateIndices.Count == 0;
            audit.Diagnostic =
                BuildGeneratedMassSelectionArchitectureDiagnostic(audit);
            coverageAudit.SelectionArchitectureAudit = audit;
        }


        private static void CaptureGeneratedMassFullRebuildOracle(
            List<PolygonFace> faces,
            Bounds bounds,
            float maximumDimension,
            MassRecipe recipe,
            MassSurfaceFeatureSettings settings,
            float amount01,
            float requestedWidth,
            bool includeAllGeometricCandidates,
            EdgeWearMicroTopologyNormalizationResult microTopologyNormalization,
            List<EdgeWearBevelCandidate> primaryCandidates,
            EdgeWearCoverageAudit primaryCoverage)
        {
            if (primaryCoverage == null ||
                primaryCoverage.SelectionArchitectureAudit == null)
            {
                return;
            }

            List<EdgeWearBevelCandidate> rebuiltCandidates =
                BuildEdgeWearBevelCandidates(
                    faces,
                    bounds,
                    maximumDimension,
                    recipe,
                    settings,
                    amount01,
                    requestedWidth,
                    includeAllGeometricCandidates,
                    microTopologyNormalization,
                    out EdgeWearCoverageAudit rebuiltCoverage);

            GeneratedMassFullRebuildOracleAudit audit =
                new GeneratedMassFullRebuildOracleAudit
                {
                    Captured = true,
                    PrimaryCandidateCount = primaryCandidates == null
                        ? 0
                        : primaryCandidates.Count,
                    RebuildCandidateCount = rebuiltCandidates == null
                        ? 0
                        : rebuiltCandidates.Count,
                    PrimaryLifecycleRecordCount = primaryCoverage.Records.Count,
                    RebuildLifecycleRecordCount = rebuiltCoverage == null
                        ? 0
                        : rebuiltCoverage.Records.Count
                };

            List<int> candidateMismatches = new List<int>();
            int candidateCount = System.Math.Max(
                audit.PrimaryCandidateCount,
                audit.RebuildCandidateCount);
            ulong primaryCandidateHash = 1469598103934665603UL;
            ulong rebuildCandidateHash = 1469598103934665603UL;
            for (int index = 0; index < candidateCount; index++)
            {
                bool hasPrimary = primaryCandidates != null &&
                    index < primaryCandidates.Count;
                bool hasRebuild = rebuiltCandidates != null &&
                    index < rebuiltCandidates.Count;
                if (hasPrimary)
                {
                    primaryCandidateHash = HashGeneratedMassBevelCandidate(
                        primaryCandidateHash,
                        primaryCandidates[index]);
                }
                if (hasRebuild)
                {
                    rebuildCandidateHash = HashGeneratedMassBevelCandidate(
                        rebuildCandidateHash,
                        rebuiltCandidates[index]);
                }
                if (!hasPrimary || !hasRebuild ||
                    !AreGeneratedMassBevelCandidatesEqual(
                        primaryCandidates[index],
                        rebuiltCandidates[index]))
                {
                    candidateMismatches.Add(index);
                }
            }

            List<int> lifecycleMismatches = new List<int>();
            int lifecycleCount = System.Math.Max(
                audit.PrimaryLifecycleRecordCount,
                audit.RebuildLifecycleRecordCount);
            ulong primaryLifecycleHash = 1469598103934665603UL;
            ulong rebuildLifecycleHash = 1469598103934665603UL;
            for (int index = 0; index < lifecycleCount; index++)
            {
                bool hasPrimary = index < primaryCoverage.Records.Count;
                bool hasRebuild = rebuiltCoverage != null &&
                    index < rebuiltCoverage.Records.Count;
                if (hasPrimary)
                {
                    primaryLifecycleHash = HashGeneratedMassLifecycleRecord(
                        primaryLifecycleHash,
                        primaryCoverage.Records[index]);
                }
                if (hasRebuild)
                {
                    rebuildLifecycleHash = HashGeneratedMassLifecycleRecord(
                        rebuildLifecycleHash,
                        rebuiltCoverage.Records[index]);
                }
                if (!hasPrimary || !hasRebuild ||
                    !AreGeneratedMassLifecycleRecordsEqual(
                        primaryCoverage.Records[index],
                        rebuiltCoverage.Records[index]))
                {
                    lifecycleMismatches.Add(index);
                }
            }

            audit.PrimaryCandidateHash = primaryCandidateHash;
            audit.RebuildCandidateHash = rebuildCandidateHash;
            audit.PrimaryLifecycleHash = primaryLifecycleHash;
            audit.RebuildLifecycleHash = rebuildLifecycleHash;
            audit.CandidateMismatchCount = candidateMismatches.Count;
            audit.LifecycleMismatchCount = lifecycleMismatches.Count;
            audit.CandidateMismatchIndices =
                JoinGeneratedMassSelectionIndices(candidateMismatches);
            audit.LifecycleMismatchIndices =
                JoinGeneratedMassSelectionIndices(lifecycleMismatches);
            audit.Valid =
                audit.PrimaryCandidateCount == audit.RebuildCandidateCount &&
                audit.PrimaryLifecycleRecordCount ==
                    audit.RebuildLifecycleRecordCount &&
                audit.CandidateMismatchCount == 0 &&
                audit.LifecycleMismatchCount == 0 &&
                audit.PrimaryCandidateHash == audit.RebuildCandidateHash &&
                audit.PrimaryLifecycleHash == audit.RebuildLifecycleHash;
            audit.Diagnostic = BuildGeneratedMassFullRebuildDiagnostic(audit);
            primaryCoverage.SelectionArchitectureAudit.FullRebuildOracle = audit;
        }

        private static bool AreGeneratedMassBevelCandidatesEqual(
            EdgeWearBevelCandidate a,
            EdgeWearBevelCandidate b)
        {
            return a.CandidateIndex == b.CandidateIndex &&
                a.StableIdentity == b.StableIdentity &&
                a.CandidateClass == b.CandidateClass &&
                a.Mandatory == b.Mandatory &&
                a.Start.Equals(b.Start) &&
                a.End.Equals(b.End) &&
                a.FaceA == b.FaceA &&
                a.FaceB == b.FaceB &&
                a.NormalA.Equals(b.NormalA) &&
                a.NormalB.Equals(b.NormalB) &&
                a.Midpoint.Equals(b.Midpoint) &&
                a.BevelNormal.Equals(b.BevelNormal) &&
                a.Score.Equals(b.Score) &&
                a.Strength.Equals(b.Strength) &&
                a.DepthMultiplier.Equals(b.DepthMultiplier);
        }

        private static bool AreGeneratedMassLifecycleRecordsEqual(
            EdgeWearEdgeLifecycleRecord a,
            EdgeWearEdgeLifecycleRecord b)
        {
            if (a == null || b == null)
            {
                return a == b;
            }
            return a.SourceEdgeIndex == b.SourceEdgeIndex &&
                a.OriginalSourceEdgeIndex == b.OriginalSourceEdgeIndex &&
                a.CandidateClass == b.CandidateClass &&
                a.Mandatory == b.Mandatory &&
                a.MicroTopologySuppressed == b.MicroTopologySuppressed &&
                a.MicroTopologyGeneratedTransition ==
                    b.MicroTopologyGeneratedTransition &&
                a.CandidateIndex == b.CandidateIndex &&
                a.Start.Equals(b.Start) &&
                a.End.Equals(b.End) &&
                a.Midpoint.Equals(b.Midpoint) &&
                a.OwnerNormalA.Equals(b.OwnerNormalA) &&
                a.OwnerNormalB.Equals(b.OwnerNormalB) &&
                a.BevelNormal.Equals(b.BevelNormal) &&
                a.FaceA == b.FaceA && a.FaceB == b.FaceB &&
                a.FaceCount == b.FaceCount &&
                a.Length.Equals(b.Length) &&
                a.DihedralDegrees.Equals(b.DihedralDegrees) &&
                a.Vertical01.Equals(b.Vertical01) &&
                a.Score.Equals(b.Score) &&
                a.StructuralEligible == b.StructuralEligible &&
                a.GeometricEligible == b.GeometricEligible &&
                a.CoexistenceEligible == b.CoexistenceEligible &&
                a.ArtisticEligible == b.ArtisticEligible &&
                a.ViabilityState == b.ViabilityState &&
                a.Candidate == b.Candidate &&
                a.CandidateReason == b.CandidateReason &&
                a.CoexistenceFailureReason == b.CoexistenceFailureReason &&
                a.FinalReason == b.FinalReason;
        }

        private static ulong HashGeneratedMassBevelCandidate(
            ulong hash,
            EdgeWearBevelCandidate candidate)
        {
            hash = HashGeneratedMassOracleValue(hash, candidate.CandidateIndex);
            hash = HashGeneratedMassOracleValue(hash, candidate.StableIdentity);
            hash = HashGeneratedMassOracleValue(hash, (int)candidate.CandidateClass);
            hash = HashGeneratedMassOracleValue(hash, candidate.Mandatory ? 1 : 0);
            hash = HashGeneratedMassOracleVector(hash, candidate.Start);
            hash = HashGeneratedMassOracleVector(hash, candidate.End);
            hash = HashGeneratedMassOracleValue(hash, candidate.FaceA);
            hash = HashGeneratedMassOracleValue(hash, candidate.FaceB);
            hash = HashGeneratedMassOracleVector(hash, candidate.NormalA);
            hash = HashGeneratedMassOracleVector(hash, candidate.NormalB);
            hash = HashGeneratedMassOracleVector(hash, candidate.Midpoint);
            hash = HashGeneratedMassOracleVector(hash, candidate.BevelNormal);
            hash = HashGeneratedMassOracleValue(hash, candidate.Score.GetHashCode());
            hash = HashGeneratedMassOracleValue(hash, candidate.Strength.GetHashCode());
            return HashGeneratedMassOracleValue(
                hash,
                candidate.DepthMultiplier.GetHashCode());
        }

        private static ulong HashGeneratedMassLifecycleRecord(
            ulong hash,
            EdgeWearEdgeLifecycleRecord record)
        {
            if (record == null)
            {
                return HashGeneratedMassOracleValue(hash, -1);
            }
            hash = HashGeneratedMassOracleValue(hash, record.SourceEdgeIndex);
            hash = HashGeneratedMassOracleValue(
                hash,
                record.OriginalSourceEdgeIndex);
            hash = HashGeneratedMassOracleValue(hash, (int)record.CandidateClass);
            hash = HashGeneratedMassOracleValue(hash, record.Mandatory ? 1 : 0);
            hash = HashGeneratedMassOracleValue(
                hash,
                record.MicroTopologySuppressed ? 1 : 0);
            hash = HashGeneratedMassOracleValue(
                hash,
                record.MicroTopologyGeneratedTransition ? 1 : 0);
            hash = HashGeneratedMassOracleValue(hash, record.CandidateIndex);
            hash = HashGeneratedMassOracleVector(hash, record.Start);
            hash = HashGeneratedMassOracleVector(hash, record.End);
            hash = HashGeneratedMassOracleVector(hash, record.Midpoint);
            hash = HashGeneratedMassOracleVector(hash, record.OwnerNormalA);
            hash = HashGeneratedMassOracleVector(hash, record.OwnerNormalB);
            hash = HashGeneratedMassOracleVector(hash, record.BevelNormal);
            hash = HashGeneratedMassOracleValue(hash, record.FaceA);
            hash = HashGeneratedMassOracleValue(hash, record.FaceB);
            hash = HashGeneratedMassOracleValue(hash, record.FaceCount);
            hash = HashGeneratedMassOracleValue(hash, record.Length.GetHashCode());
            hash = HashGeneratedMassOracleValue(
                hash,
                record.DihedralDegrees.GetHashCode());
            hash = HashGeneratedMassOracleValue(hash, record.Vertical01.GetHashCode());
            hash = HashGeneratedMassOracleValue(hash, record.Score.GetHashCode());
            hash = HashGeneratedMassOracleValue(
                hash,
                ResolveGeneratedMassLegacyBevelObservedStage(record).GetHashCode());
            hash = HashGeneratedMassOracleString(hash, record.CandidateReason);
            hash = HashGeneratedMassOracleString(
                hash,
                record.CoexistenceFailureReason);
            return HashGeneratedMassOracleString(hash, record.FinalReason);
        }

        private static ulong HashGeneratedMassOracleVector(
            ulong hash,
            UnityEngine.Vector3 value)
        {
            hash = HashGeneratedMassOracleValue(hash, value.x.GetHashCode());
            hash = HashGeneratedMassOracleValue(hash, value.y.GetHashCode());
            return HashGeneratedMassOracleValue(hash, value.z.GetHashCode());
        }

        private static ulong HashGeneratedMassOracleString(
            ulong hash,
            string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return HashGeneratedMassOracleValue(hash, 0);
            }
            for (int index = 0; index < value.Length; index++)
            {
                hash ^= value[index];
                hash *= 1099511628211UL;
            }
            return hash;
        }

        private static ulong HashGeneratedMassOracleValue(
            ulong hash,
            int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
                return hash;
            }
        }

        private static string BuildGeneratedMassFullRebuildDiagnostic(
            GeneratedMassFullRebuildOracleAudit audit)
        {
            StringBuilder builder = new StringBuilder(384);
            builder.Append("GM-SEL Phase 3 full-rebuild oracle/");
            builder.Append("primaryCandidates:").Append(audit.PrimaryCandidateCount);
            builder.Append("/rebuildCandidates:").Append(audit.RebuildCandidateCount);
            builder.Append("/primaryRecords:")
                .Append(audit.PrimaryLifecycleRecordCount);
            builder.Append("/rebuildRecords:")
                .Append(audit.RebuildLifecycleRecordCount);
            builder.Append("/candidateMismatches:")
                .Append(audit.CandidateMismatchCount);
            builder.Append("/lifecycleMismatches:")
                .Append(audit.LifecycleMismatchCount);
            builder.Append("/valid:").Append(audit.Valid ? 1 : 0);
            if (!audit.Valid)
            {
                builder.Append("/candidateIndices:")
                    .Append(audit.CandidateMismatchIndices);
                builder.Append("/lifecycleIndices:")
                    .Append(audit.LifecycleMismatchIndices);
            }
            return builder.ToString();
        }

        private static GeneratedMassLegacyBevelObservedStage
            ResolveGeneratedMassLegacyBevelObservedStage(
                EdgeWearEdgeLifecycleRecord record)
        {
            if (record == null)
            {
                return GeneratedMassLegacyBevelObservedStage.Discovered;
            }
            if (record.Rejected)
            {
                return GeneratedMassLegacyBevelObservedStage.Rejected;
            }
            if (record.Built)
            {
                return GeneratedMassLegacyBevelObservedStage.Built;
            }
            if (record.AttemptedBuilt)
            {
                return GeneratedMassLegacyBevelObservedStage.AttemptedBuilt;
            }
            if (record.Selected)
            {
                return GeneratedMassLegacyBevelObservedStage.Selected;
            }
            if (record.Candidate)
            {
                return GeneratedMassLegacyBevelObservedStage.Candidate;
            }
            if (record.ArtisticEligible)
            {
                return GeneratedMassLegacyBevelObservedStage.ArtisticallyEligible;
            }
            if (record.CoexistenceEligible)
            {
                return GeneratedMassLegacyBevelObservedStage.CoexistenceEligible;
            }
            if (record.GeometricEligible)
            {
                return GeneratedMassLegacyBevelObservedStage.GeometricallyEligible;
            }
            if (record.StructuralEligible)
            {
                return GeneratedMassLegacyBevelObservedStage.StructurallyEligible;
            }
            return GeneratedMassLegacyBevelObservedStage.Discovered;
        }

        private static string BuildGeneratedMassSelectionArchitectureDiagnostic(
            GeneratedMassSelectionArchitectureAudit audit)
        {
            StringBuilder builder = new StringBuilder(512);
            builder.Append("GM-SEL Phase 1 parity/");
            builder.Append("faces:").Append(audit.Snapshot.FaceCount);
            builder.Append("/rawEdges:")
                .Append(audit.Snapshot.RawSourceEdgeCount);
            builder.Append("/sourceEdges:")
                .Append(audit.Snapshot.SourceEdgeCount);
            builder.Append("/records:")
                .Append(audit.Snapshot.LifecycleRecordCount);
            builder.Append("/lifecycleCandidates:")
                .Append(audit.LifecycleCandidateCount);
            builder.Append("/returnedCandidates:")
                .Append(audit.ReturnedCandidateCount);
            builder.Append("/mapped:")
                .Append(audit.MappedReturnedCandidateCount);
            builder.Append("/uniqueCandidateIndices:")
                .Append(audit.UniqueCandidateIndexCount);
            builder.Append("/uniqueStableIdentities:")
                .Append(audit.UniqueStableIdentityCount);
            builder.Append("/freshPostChip:")
                .Append(audit.Snapshot.FreshPostChipOrdinaryPass ? 1 : 0);
            builder.Append("/parityValid:")
                .Append(audit.ParityValid ? 1 : 0);
            if (!audit.ParityValid)
            {
                builder.Append("/unexpected:")
                    .Append(audit.UnexpectedCandidateIndices);
                builder.Append("/missing:")
                    .Append(audit.MissingCandidateIndices);
                builder.Append("/duplicateCandidateIndices:")
                    .Append(audit.DuplicateCandidateIndices);
                builder.Append("/duplicateStableIdentities:")
                    .Append(audit.DuplicateStableIdentities);
            }
            return builder.ToString();
        }

        private static string JoinGeneratedMassSelectionIndices(
            params List<int>[] lists)
        {
            StringBuilder builder = new StringBuilder();
            bool first = true;
            for (int listIndex = 0; listIndex < lists.Length; listIndex++)
            {
                List<int> list = lists[listIndex];
                if (list == null)
                {
                    continue;
                }
                for (int valueIndex = 0;
                     valueIndex < list.Count;
                     valueIndex++)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }
                    builder.Append(list[valueIndex]);
                    first = false;
                }
            }
            return first ? "none" : builder.ToString();
        }


        private static void CaptureGeneratedMassIsolatedEligibilityAudit(
            EdgeWearCoverageAudit coverageAudit)
        {
            if (coverageAudit == null ||
                coverageAudit.SelectionArchitectureAudit == null)
            {
                return;
            }

            GeneratedMassIsolatedEligibilityAudit audit =
                new GeneratedMassIsolatedEligibilityAudit
                {
                    Captured = true,
                    LifecycleRecordCount = coverageAudit.Records.Count,
                    MinimumCertifiedWidth = float.PositiveInfinity
                };
            List<int> problematic = new List<int>();

            for (int recordIndex = 0;
                 recordIndex < coverageAudit.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    coverageAudit.Records[recordIndex];
                if (record.StructuralEligible)
                {
                    audit.StructurallyEligibleCount++;
                }
                if (!record.GeometricEligible)
                {
                    audit.RejectedBeforeWidthCount++;
                    continue;
                }

                audit.GeometricallyEligibleCount++;
                EdgeWearEdgeViabilityRecord viability = record.Viability;
                if (viability == null || !viability.Evaluated)
                {
                    audit.MissingViabilityEvidenceCount++;
                    problematic.Add(record.SourceEdgeIndex);
                    continue;
                }

                audit.WidthEvidenceCount++;
                bool validInterval =
                    viability.MinimumRequiredCertifiedWidth >= 0f &&
                    viability.IsolatedMaximumCertifiedWidth >= 0f &&
                    viability.IsolatedMaximumCertifiedWidth + 0.0000001f >=
                        viability.MinimumRequiredCertifiedWidth;
                if (!validInterval)
                {
                    audit.InvalidWidthIntervalCount++;
                    problematic.Add(record.SourceEdgeIndex);
                }
                else if (viability.IsolatedMaximumCertifiedWidth > 0f)
                {
                    audit.WidthFeasibleCount++;
                }

                bool isolatedConsistent =
                    viability.IsolatedConstructionValid ==
                        viability.IsolatedSucceeded &&
                    (!viability.IsolatedSucceeded ||
                     (viability.IsolatedMaximumCertifiedWidth > 0f &&
                      viability.IsolatedWidthAttemptCount > 0 &&
                      viability.IsolatedOpenEdgeCount == 0 &&
                      viability.IsolatedNonManifoldEdgeCount == 0 &&
                      viability.IsolatedTJunctionCount == 0 &&
                      viability.IsolatedInvalidFaceCount == 0));
                if (!isolatedConsistent)
                {
                    audit.InconsistentIsolatedEvidenceCount++;
                    problematic.Add(record.SourceEdgeIndex);
                }
                if (viability.IsolatedSucceeded && validInterval)
                {
                    audit.IsolatedCertifiedCount++;
                    audit.MinimumCertifiedWidth = Mathf.Min(
                        audit.MinimumCertifiedWidth,
                        viability.IsolatedMaximumCertifiedWidth);
                    audit.MaximumCertifiedWidth = Mathf.Max(
                        audit.MaximumCertifiedWidth,
                        viability.IsolatedMaximumCertifiedWidth);
                }
            }

            if (float.IsPositiveInfinity(audit.MinimumCertifiedWidth))
            {
                audit.MinimumCertifiedWidth = 0f;
            }
            problematic.Sort();
            audit.ProblematicSourceEdges = JoinGeneratedMassSelectionIndices(
                problematic);
            audit.Valid =
                audit.MissingViabilityEvidenceCount == 0 &&
                audit.InvalidWidthIntervalCount == 0 &&
                audit.InconsistentIsolatedEvidenceCount == 0;
            audit.Diagnostic =
                "GM-SEL Phase 4 isolated eligibility" +
                "/records:" + audit.LifecycleRecordCount +
                "/structural:" + audit.StructurallyEligibleCount +
                "/geometric:" + audit.GeometricallyEligibleCount +
                "/widthEvidence:" + audit.WidthEvidenceCount +
                "/widthFeasible:" + audit.WidthFeasibleCount +
                "/isolatedCertified:" + audit.IsolatedCertifiedCount +
                "/rejectedBeforeWidth:" + audit.RejectedBeforeWidthCount +
                "/missingEvidence:" + audit.MissingViabilityEvidenceCount +
                "/invalidIntervals:" + audit.InvalidWidthIntervalCount +
                "/inconsistentIsolated:" +
                    audit.InconsistentIsolatedEvidenceCount +
                "/certifiedWidthRange:" +
                    audit.MinimumCertifiedWidth.ToString("G9") + "-" +
                    audit.MaximumCertifiedWidth.ToString("G9") +
                "/problematic:{" + audit.ProblematicSourceEdges + "}" +
                "/valid:" + (audit.Valid ? "1" : "0");
            coverageAudit.SelectionArchitectureAudit.IsolatedEligibility =
                audit;
        }


        private static void CaptureGeneratedMassPotentialInteractionAudit(
            List<EdgeWearBevelCandidate> candidates,
            EdgeWearCoverageAudit coverageAudit)
        {
            if (coverageAudit == null ||
                coverageAudit.SelectionArchitectureAudit == null)
            {
                return;
            }

            GeneratedMassPotentialInteractionAudit audit =
                new GeneratedMassPotentialInteractionAudit
                {
                    Captured = true,
                    CandidateCount = candidates == null ? 0 : candidates.Count
                };
            Dictionary<int, EdgeWearEdgeLifecycleRecord> lifecycleByCandidate =
                new Dictionary<int, EdgeWearEdgeLifecycleRecord>();
            for (int i = 0; i < coverageAudit.Records.Count; i++)
            {
                EdgeWearEdgeLifecycleRecord record = coverageAudit.Records[i];
                if (record.Candidate && record.CandidateIndex >= 0)
                {
                    lifecycleByCandidate[record.CandidateIndex] = record;
                }
            }

            List<int> problematic = new List<int>();
            List<string> samples = new List<string>();
            int[] degree = new int[audit.CandidateCount];
            HashSet<ulong> pairKeys = new HashSet<ulong>();
            const float endpointTolerance = 0.0001f;
            float endpointToleranceSq = endpointTolerance * endpointTolerance;
            ulong hash = 1469598103934665603UL;
            if (candidates != null)
            {
                for (int a = 0; a < candidates.Count; a++)
                {
                    EdgeWearBevelCandidate candidateA = candidates[a];
                    if (!lifecycleByCandidate.TryGetValue(
                            candidateA.CandidateIndex, out var recordA) ||
                        recordA.Viability == null ||
                        !recordA.Viability.Evaluated)
                    {
                        audit.MissingWidthEvidenceCount++;
                        problematic.Add(candidateA.CandidateIndex);
                    }
                    for (int b = a + 1; b < candidates.Count; b++)
                    {
                        audit.TotalPairCount++;
                        EdgeWearBevelCandidate candidateB = candidates[b];
                        ulong key = ((ulong)(uint)candidateA.CandidateIndex << 32) |
                            (uint)candidateB.CandidateIndex;
                        if (!pairKeys.Add(key))
                        {
                            audit.DuplicatePairCount++;
                        }
                        bool sharedEndpoint =
                            (candidateA.Start - candidateB.Start).sqrMagnitude <= endpointToleranceSq ||
                            (candidateA.Start - candidateB.End).sqrMagnitude <= endpointToleranceSq ||
                            (candidateA.End - candidateB.Start).sqrMagnitude <= endpointToleranceSq ||
                            (candidateA.End - candidateB.End).sqrMagnitude <= endpointToleranceSq;
                        bool sharedFace =
                            candidateA.FaceA == candidateB.FaceA ||
                            candidateA.FaceA == candidateB.FaceB ||
                            candidateA.FaceB == candidateB.FaceA ||
                            candidateA.FaceB == candidateB.FaceB;
                        float widthA = 0f;
                        float widthB = 0f;
                        if (lifecycleByCandidate.TryGetValue(candidateA.CandidateIndex, out recordA) &&
                            recordA.Viability != null)
                        {
                            widthA = recordA.Viability.IsolatedMaximumCertifiedWidth;
                        }
                        if (lifecycleByCandidate.TryGetValue(candidateB.CandidateIndex, out var recordB) &&
                            recordB.Viability != null)
                        {
                            widthB = recordB.Viability.IsolatedMaximumCertifiedWidth;
                        }
                        Bounds boundsA = new Bounds(candidateA.Midpoint, Vector3.zero);
                        boundsA.Encapsulate(candidateA.Start);
                        boundsA.Encapsulate(candidateA.End);
                        boundsA.Expand(Mathf.Max(0.0001f, widthA * 2f));
                        Bounds boundsB = new Bounds(candidateB.Midpoint, Vector3.zero);
                        boundsB.Encapsulate(candidateB.Start);
                        boundsB.Encapsulate(candidateB.End);
                        boundsB.Expand(Mathf.Max(0.0001f, widthB * 2f));
                        bool expandedBounds = boundsA.Intersects(boundsB);
                        if (sharedEndpoint) audit.SharedEndpointPairCount++;
                        if (sharedFace) audit.SharedFacePairCount++;
                        if (expandedBounds) audit.ExpandedBoundsPairCount++;
                        bool potential = sharedEndpoint || sharedFace || expandedBounds;
                        if (potential)
                        {
                            audit.PotentialPairCount++;
                            degree[a]++;
                            degree[b]++;
                            if (samples.Count < 16)
                            {
                                samples.Add(candidateA.CandidateIndex + "-" +
                                    candidateB.CandidateIndex + "@" +
                                    widthA.ToString("G6") + "/" +
                                    widthB.ToString("G6"));
                            }
                        }
                        else
                        {
                            audit.DisjointPairCount++;
                        }
                        hash ^= key;
                        hash *= 1099511628211UL;
                        hash ^= potential ? 1UL : 0UL;
                        hash *= 1099511628211UL;
                    }
                }
            }
            for (int i = 0; i < degree.Length; i++)
            {
                audit.MaximumCandidateDegree = Mathf.Max(
                    audit.MaximumCandidateDegree, degree[i]);
            }
            problematic.Sort();
            audit.ProblematicCandidates = JoinGeneratedMassSelectionIndices(problematic);
            audit.SamplePotentialPairs = samples.Count == 0
                ? "none" : string.Join(",", samples);
            audit.RelationHash = hash;
            audit.Valid =
                audit.TotalPairCount ==
                    audit.PotentialPairCount + audit.DisjointPairCount &&
                audit.DuplicatePairCount == 0 &&
                audit.MissingWidthEvidenceCount == 0;
            audit.Diagnostic =
                "GM-SEL Phase 5 conservative interaction discovery" +
                "/candidates:" + audit.CandidateCount +
                "/pairs:" + audit.TotalPairCount +
                "/potential:" + audit.PotentialPairCount +
                "/disjoint:" + audit.DisjointPairCount +
                "/sharedEndpoint:" + audit.SharedEndpointPairCount +
                "/sharedFace:" + audit.SharedFacePairCount +
                "/expandedBounds:" + audit.ExpandedBoundsPairCount +
                "/maxDegree:" + audit.MaximumCandidateDegree +
                "/missingWidthEvidence:" + audit.MissingWidthEvidenceCount +
                "/duplicatePairs:" + audit.DuplicatePairCount +
                "/relationHash:" + audit.RelationHash.ToString("X16") +
                "/valid:" + (audit.Valid ? "1" : "0");
            coverageAudit.SelectionArchitectureAudit.PotentialInteraction = audit;
        }


        private static void CaptureGeneratedMassPairwiseCompatibilityAudit(
            List<EdgeWearBevelCandidate> candidates,
            EdgeWearCoverageAudit coverageAudit)
        {
            if (coverageAudit == null ||
                coverageAudit.SelectionArchitectureAudit == null ||
                coverageAudit.SelectionArchitectureAudit.PotentialInteraction == null)
            {
                return;
            }

            GeneratedMassPairwiseCompatibilityAudit audit =
                new GeneratedMassPairwiseCompatibilityAudit
                {
                    Captured = true,
                    CandidateCount = candidates == null ? 0 : candidates.Count,
                    PotentialPairCount = coverageAudit.SelectionArchitectureAudit
                        .PotentialInteraction.PotentialPairCount,
                    MinimumClearance = float.PositiveInfinity
                };
            Dictionary<int, EdgeWearEdgeLifecycleRecord> records =
                new Dictionary<int, EdgeWearEdgeLifecycleRecord>();
            for (int i = 0; i < coverageAudit.Records.Count; i++)
            {
                EdgeWearEdgeLifecycleRecord record = coverageAudit.Records[i];
                if (record.Candidate && record.CandidateIndex >= 0)
                    records[record.CandidateIndex] = record;
            }
            List<string> incompatible = new List<string>();
            List<string> unresolved = new List<string>();
            HashSet<ulong> relations = new HashSet<ulong>();
            const float endpointTolerance = 0.0001f;
            float endpointToleranceSq = endpointTolerance * endpointTolerance;

            if (candidates != null)
            {
                for (int a = 0; a < candidates.Count; a++)
                {
                    EdgeWearBevelCandidate ca = candidates[a];
                    for (int b = a + 1; b < candidates.Count; b++)
                    {
                        EdgeWearBevelCandidate cb = candidates[b];
                        bool sharedEndpoint =
                            (ca.Start - cb.Start).sqrMagnitude <= endpointToleranceSq ||
                            (ca.Start - cb.End).sqrMagnitude <= endpointToleranceSq ||
                            (ca.End - cb.Start).sqrMagnitude <= endpointToleranceSq ||
                            (ca.End - cb.End).sqrMagnitude <= endpointToleranceSq;
                        bool sharedFace = ca.FaceA == cb.FaceA || ca.FaceA == cb.FaceB ||
                            ca.FaceB == cb.FaceA || ca.FaceB == cb.FaceB;
                        if (!records.TryGetValue(ca.CandidateIndex, out var ra) ||
                            !records.TryGetValue(cb.CandidateIndex, out var rb) ||
                            ra.Viability == null || rb.Viability == null)
                        {
                            continue;
                        }
                        float wa = ra.Viability.IsolatedMaximumCertifiedWidth;
                        float wb = rb.Viability.IsolatedMaximumCertifiedWidth;
                        Bounds ba = new Bounds(ca.Midpoint, Vector3.zero);
                        ba.Encapsulate(ca.Start); ba.Encapsulate(ca.End);
                        ba.Expand(Mathf.Max(0.0001f, wa * 2f));
                        Bounds bb = new Bounds(cb.Midpoint, Vector3.zero);
                        bb.Encapsulate(cb.Start); bb.Encapsulate(cb.End);
                        bb.Expand(Mathf.Max(0.0001f, wb * 2f));
                        if (!(sharedEndpoint || sharedFace || ba.Intersects(bb)))
                            continue;

                        ulong key = ((ulong)(uint)ca.CandidateIndex << 32) |
                            (uint)cb.CandidateIndex;
                        if (!relations.Add(key)) audit.DuplicateRelationCount++;
                        audit.EvaluatedPairCount++;

                        if (wa <= 0f || wb <= 0f)
                        {
                            audit.UnresolvedPairCount++;
                            unresolved.Add(ca.CandidateIndex + "-" + cb.CandidateIndex + ":missing-width");
                            continue;
                        }

                        float segmentDistance = Mathf.Sqrt(
                            SegmentSegmentDistanceSquared(ca.Start, ca.End, cb.Start, cb.End));
                        float clearance = segmentDistance - (wa + wb);
                        audit.MinimumClearance = Mathf.Min(audit.MinimumClearance, clearance);
                        float minLength = Mathf.Min(
                            Vector3.Distance(ca.Start, ca.End),
                            Vector3.Distance(cb.Start, cb.End));
                        bool endpointOverconsumption = sharedEndpoint &&
                            (wa + wb) > minLength * 0.45f;
                        bool bandPenetration = !sharedEndpoint && clearance < -0.00001f;
                        bool faceOverconsumption = sharedFace && clearance < 0f &&
                            (wa + wb) > minLength * 0.25f;
                        if (endpointOverconsumption || bandPenetration || faceOverconsumption)
                        {
                            audit.IncompatiblePairCount++;
                            if (incompatible.Count < 32)
                                incompatible.Add(ca.CandidateIndex + "-" + cb.CandidateIndex +
                                    "@" + wa.ToString("G6") + "/" + wb.ToString("G6") +
                                    ":clearance=" + clearance.ToString("G6"));
                        }
                        else
                        {
                            audit.CompatiblePairCount++;
                        }
                    }
                }
            }
            if (float.IsPositiveInfinity(audit.MinimumClearance)) audit.MinimumClearance = 0f;
            audit.MissingRelationCount = Mathf.Max(0,
                audit.PotentialPairCount - audit.EvaluatedPairCount);
            audit.IncompatiblePairs = incompatible.Count == 0 ? "none" : string.Join(",", incompatible);
            audit.UnresolvedPairs = unresolved.Count == 0 ? "none" : string.Join(",", unresolved);
            audit.Valid = audit.DuplicateRelationCount == 0 &&
                audit.MissingRelationCount == 0 &&
                audit.EvaluatedPairCount == audit.CompatiblePairCount +
                    audit.IncompatiblePairCount + audit.UnresolvedPairCount;
            audit.Diagnostic = "GM-SEL Phase 6 pairwise compatibility" +
                "/potential:" + audit.PotentialPairCount +
                "/evaluated:" + audit.EvaluatedPairCount +
                "/compatible:" + audit.CompatiblePairCount +
                "/incompatible:" + audit.IncompatiblePairCount +
                "/unresolved:" + audit.UnresolvedPairCount +
                "/missing:" + audit.MissingRelationCount +
                "/duplicates:" + audit.DuplicateRelationCount +
                "/minClearance:" + audit.MinimumClearance.ToString("G9") +
                "/valid:" + (audit.Valid ? "1" : "0");
            coverageAudit.SelectionArchitectureAudit.PairwiseCompatibility = audit;
        }

        private static float SegmentSegmentDistanceSquared(
            Vector3 p1, Vector3 q1, Vector3 p2, Vector3 q2)
        {
            Vector3 d1 = q1 - p1;
            Vector3 d2 = q2 - p2;
            Vector3 r = p1 - p2;
            float a = Vector3.Dot(d1, d1);
            float e = Vector3.Dot(d2, d2);
            float f = Vector3.Dot(d2, r);
            float s, t;
            if (a <= 0.00000001f && e <= 0.00000001f) return r.sqrMagnitude;
            if (a <= 0.00000001f) { s = 0f; t = Mathf.Clamp01(f / e); }
            else
            {
                float c = Vector3.Dot(d1, r);
                if (e <= 0.00000001f) { t = 0f; s = Mathf.Clamp01(-c / a); }
                else
                {
                    float b = Vector3.Dot(d1, d2);
                    float denom = a * e - b * b;
                    s = denom != 0f ? Mathf.Clamp01((b * f - c * e) / denom) : 0f;
                    t = (b * s + f) / e;
                    if (t < 0f) { t = 0f; s = Mathf.Clamp01(-c / a); }
                    else if (t > 1f) { t = 1f; s = Mathf.Clamp01((b - c) / a); }
                }
            }
            Vector3 c1 = p1 + d1 * s;
            Vector3 c2 = p2 + d2 * t;
            return (c1 - c2).sqrMagnitude;
        }

        #endregion
    }
}
