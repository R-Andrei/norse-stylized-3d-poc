using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear contained-boundary diagnostics

        private enum ChamferContainedBoundaryClassification
        {
            ExactValid,
            SplitEquivalent,
            ResidualMissing,
            ExternalUnsplit,
            Underused,
            Overused,
            Ambiguous
        }

        private enum ChamferContainedBoundaryEdgeOwner
        {
            TargetPatch,
            ResidualOwner,
            OtherReplacement,
            Bevel,
            OtherPatch
        }

        private sealed class ChamferContainedBoundaryAudit
        {
            public readonly ChamferContainedBoundaryClassification
                Classification;
            public readonly bool ExactTwoUse;
            public readonly int SegmentCount;
            public readonly int ExactValidSegments;
            public readonly int SplitEquivalentSegments;
            public readonly int ResidualMissingSegments;
            public readonly int ExternalUnsplitSegments;
            public readonly int UnderusedSegments;
            public readonly int OverusedSegments;
            public readonly int AmbiguousSegments;
            public readonly string Diagnostic;

            public ChamferContainedBoundaryAudit(
                ChamferContainedBoundaryClassification classification,
                bool exactTwoUse,
                int segmentCount,
                int exactValidSegments,
                int splitEquivalentSegments,
                int residualMissingSegments,
                int externalUnsplitSegments,
                int underusedSegments,
                int overusedSegments,
                int ambiguousSegments,
                string diagnostic)
            {
                Classification = classification;
                ExactTwoUse = exactTwoUse;
                SegmentCount = segmentCount;
                ExactValidSegments = exactValidSegments;
                SplitEquivalentSegments = splitEquivalentSegments;
                ResidualMissingSegments = residualMissingSegments;
                ExternalUnsplitSegments = externalUnsplitSegments;
                UnderusedSegments = underusedSegments;
                OverusedSegments = overusedSegments;
                AmbiguousSegments = ambiguousSegments;
                Diagnostic = diagnostic;
            }
        }

        private sealed class ChamferContainedBoundarySegmentAudit
        {
            public readonly ChamferContainedBoundaryClassification
                Classification;
            public readonly bool ExactTwoUse;
            public readonly string Diagnostic;

            public ChamferContainedBoundarySegmentAudit(
                ChamferContainedBoundaryClassification classification,
                bool exactTwoUse,
                string diagnostic)
            {
                Classification = classification;
                ExactTwoUse = exactTwoUse;
                Diagnostic = diagnostic;
            }
        }

        private readonly struct ChamferContainedBoundaryEdgeRecord
        {
            public readonly Vector3 Start;
            public readonly Vector3 End;
            public readonly ChamferProvisionalFaceKind Kind;
            public readonly int SourceFaceIndex;
            public readonly int PatchLoopIndex;

            public ChamferContainedBoundaryEdgeRecord(
                Vector3 start,
                Vector3 end,
                ChamferProvisionalFaceKind kind,
                int sourceFaceIndex,
                int patchLoopIndex)
            {
                Start = start;
                End = end;
                Kind = kind;
                SourceFaceIndex = sourceFaceIndex;
                PatchLoopIndex = patchLoopIndex;
            }
        }

        private readonly struct ChamferContainedBoundaryInterval
        {
            public readonly float Start;
            public readonly float End;
            public readonly float RawStart;
            public readonly float RawEnd;
            public readonly ChamferContainedBoundaryEdgeOwner Owner;

            public ChamferContainedBoundaryInterval(
                float start,
                float end,
                float rawStart,
                float rawEnd,
                ChamferContainedBoundaryEdgeOwner owner)
            {
                Start = start;
                End = end;
                RawStart = rawStart;
                RawEnd = rawEnd;
                Owner = owner;
            }

            public bool Contains(float parameter, float epsilon)
            {
                return parameter >= Start - epsilon &&
                    parameter <= End + epsilon;
            }
        }

        private readonly struct ChamferContainedRepartitionShadowAudit
        {
            public readonly bool OverlapRemoved;
            public readonly bool TopologyClean;
            public readonly bool TJunctionIncrease;
            public readonly bool UnexpectedOpenEdgeIncrease;
            public readonly bool SourceBoundaryIncrease;
            public readonly bool NonManifoldIncrease;

            public ChamferContainedRepartitionShadowAudit(
                bool overlapRemoved,
                bool topologyClean,
                bool tJunctionIncrease,
                bool unexpectedOpenEdgeIncrease,
                bool sourceBoundaryIncrease,
                bool nonManifoldIncrease)
            {
                OverlapRemoved = overlapRemoved;
                TopologyClean = topologyClean;
                TJunctionIncrease = tJunctionIncrease;
                UnexpectedOpenEdgeIncrease = unexpectedOpenEdgeIncrease;
                SourceBoundaryIncrease = sourceBoundaryIncrease;
                NonManifoldIncrease = nonManifoldIncrease;
            }
        }

        private static readonly HashSet<
            ChamferContainedBoundaryClassification>
            LoggedChamferContainedBoundaryClassifications =
                new HashSet<ChamferContainedBoundaryClassification>();

        private static ChamferContainedBoundaryAudit
            AuditChamferContainedBoundaryIncidence(
                ChamferContainedPatchCandidate candidate,
                ChamferProvisionalFaceRecord ownerRecord,
                List<ChamferProvisionalFaceRecord> patchRecords,
                List<ChamferProvisionalFaceRecord> transformedRecords,
                float minimumStableEdgeLength)
        {
            List<ChamferBoundarySegment> patchBoundary =
                BuildChamferPatchBoundarySegments(patchRecords);
            SortChamferBoundarySegments(patchBoundary);
            PolygonFace ownerFace = BuildChamferRenderFaithfulFace(
                ownerRecord.Face);
            List<ChamferBoundarySegment> ownerBoundary =
                BuildChamferFaceBoundarySegments(ownerFace);
            List<ChamferContainedBoundaryEdgeRecord> edgeRecords =
                BuildChamferContainedBoundaryEdgeRecords(
                    transformedRecords);
            float tolerance = CalculateTopologyTJunctionTolerance(
                minimumStableEdgeLength);

            int exactValid = 0;
            int splitEquivalent = 0;
            int residualMissing = 0;
            int externalUnsplit = 0;
            int underused = 0;
            int overused = 0;
            int ambiguous = 0;
            bool exactTwoUse = patchBoundary.Count > 0;
            string diagnostic = string.Empty;
            for (int segmentIndex = 0;
                 segmentIndex < patchBoundary.Count;
                 segmentIndex++)
            {
                ChamferContainedBoundarySegmentAudit segmentAudit =
                    AuditChamferContainedBoundarySegment(
                        candidate,
                        ownerRecord,
                        patchBoundary[segmentIndex],
                        ownerBoundary,
                        edgeRecords,
                        tolerance);
                exactTwoUse &= segmentAudit.ExactTwoUse;
                switch (segmentAudit.Classification)
                {
                    case ChamferContainedBoundaryClassification.ExactValid:
                        exactValid++;
                        break;
                    case ChamferContainedBoundaryClassification.SplitEquivalent:
                        splitEquivalent++;
                        break;
                    case ChamferContainedBoundaryClassification.ResidualMissing:
                        residualMissing++;
                        break;
                    case ChamferContainedBoundaryClassification.ExternalUnsplit:
                        externalUnsplit++;
                        break;
                    case ChamferContainedBoundaryClassification.Underused:
                        underused++;
                        break;
                    case ChamferContainedBoundaryClassification.Overused:
                        overused++;
                        break;
                    default:
                        ambiguous++;
                        break;
                }
                if (string.IsNullOrEmpty(diagnostic) &&
                    segmentAudit.Classification !=
                        ChamferContainedBoundaryClassification.ExactValid)
                {
                    diagnostic = "segment=" + segmentIndex + "," +
                        segmentAudit.Diagnostic;
                }
            }

            ChamferContainedBoundaryClassification classification =
                ClassifyChamferContainedBoundaryCandidate(
                    patchBoundary.Count,
                    exactValid,
                    splitEquivalent,
                    residualMissing,
                    externalUnsplit,
                    underused,
                    overused,
                    ambiguous);
            return new ChamferContainedBoundaryAudit(
                classification,
                exactTwoUse,
                patchBoundary.Count,
                exactValid,
                splitEquivalent,
                residualMissing,
                externalUnsplit,
                underused,
                overused,
                ambiguous,
                diagnostic);
        }

        private static ChamferContainedBoundarySegmentAudit
            AuditChamferContainedBoundarySegment(
                ChamferContainedPatchCandidate candidate,
                ChamferProvisionalFaceRecord ownerRecord,
                ChamferBoundarySegment boundary,
                List<ChamferBoundarySegment> ownerBoundary,
                List<ChamferContainedBoundaryEdgeRecord> edgeRecords,
                float tolerance)
        {
            TopologyEdgeKey boundaryKey = new TopologyEdgeKey(
                new VertexKey(boundary.Start),
                new VertexKey(boundary.End));
            int exactTotal = 0;
            int exactPatch = 0;
            int exactResidual = 0;
            int exactOtherReplacement = 0;
            int exactBevel = 0;
            int exactOtherPatch = 0;
            List<ChamferContainedBoundaryInterval> intervals =
                new List<ChamferContainedBoundaryInterval>();
            List<float> breakpoints = new List<float>
            {
                0f,
                1f
            };
            Vector3 direction = boundary.End - boundary.Start;
            float length = direction.magnitude;
            float parameterEpsilon = length > 0f
                ? tolerance / length
                : 1f;

            for (int edgeIndex = 0;
                 edgeIndex < edgeRecords.Count;
                 edgeIndex++)
            {
                ChamferContainedBoundaryEdgeRecord edge =
                    edgeRecords[edgeIndex];
                ChamferContainedBoundaryEdgeOwner edgeOwner =
                    ClassifyChamferContainedBoundaryEdgeOwner(
                        candidate,
                        ownerRecord,
                        edge);
                TopologyEdgeKey edgeKey = new TopologyEdgeKey(
                    new VertexKey(edge.Start),
                    new VertexKey(edge.End));
                if (edgeKey.Equals(boundaryKey))
                {
                    exactTotal++;
                    IncrementChamferContainedBoundaryOwnerCount(
                        edgeOwner,
                        ref exactPatch,
                        ref exactResidual,
                        ref exactOtherReplacement,
                        ref exactBevel,
                        ref exactOtherPatch);
                }

                if (!TryBuildChamferContainedBoundaryInterval(
                        boundary,
                        edge,
                        edgeOwner,
                        tolerance,
                        out ChamferContainedBoundaryInterval interval))
                {
                    continue;
                }
                intervals.Add(interval);
                AddChamferContainedBoundaryBreakpoint(
                    breakpoints,
                    interval.Start,
                    parameterEpsilon);
                AddChamferContainedBoundaryBreakpoint(
                    breakpoints,
                    interval.End,
                    parameterEpsilon);
            }

            bool onOwnerBoundary =
                IsChamferContainedBoundarySegmentCovered(
                    boundary,
                    ownerBoundary,
                    tolerance);
            breakpoints.Sort();
            bool intervalFailure = false;
            bool residualMissing = false;
            bool externalUnsplit = false;
            bool underused = false;
            bool overused = false;
            bool ambiguous = false;
            for (int breakpointIndex = 0;
                 breakpointIndex < breakpoints.Count - 1;
                 breakpointIndex++)
            {
                float intervalStart = breakpoints[breakpointIndex];
                float intervalEnd = breakpoints[breakpointIndex + 1];
                if (intervalEnd - intervalStart <= parameterEpsilon)
                {
                    continue;
                }
                float midpoint = (intervalStart + intervalEnd) * 0.5f;
                int patchCount = 0;
                int residualCount = 0;
                int otherReplacementCount = 0;
                int bevelCount = 0;
                int otherPatchCount = 0;
                bool intervalExternalUnsplit = false;
                for (int intervalIndex = 0;
                     intervalIndex < intervals.Count;
                     intervalIndex++)
                {
                    ChamferContainedBoundaryInterval interval =
                        intervals[intervalIndex];
                    if (!interval.Contains(midpoint, parameterEpsilon))
                    {
                        continue;
                    }
                    IncrementChamferContainedBoundaryOwnerCount(
                        interval.Owner,
                        ref patchCount,
                        ref residualCount,
                        ref otherReplacementCount,
                        ref bevelCount,
                        ref otherPatchCount);
                    if (interval.Owner !=
                            ChamferContainedBoundaryEdgeOwner.TargetPatch &&
                        interval.Owner !=
                            ChamferContainedBoundaryEdgeOwner.ResidualOwner &&
                        (interval.RawStart < -parameterEpsilon ||
                            interval.RawEnd > 1f + parameterEpsilon))
                    {
                        intervalExternalUnsplit = true;
                    }
                }

                int externalCount = otherReplacementCount + bevelCount +
                    otherPatchCount;
                int totalCount = patchCount + residualCount + externalCount;
                if (patchCount > 1 || totalCount > 2 ||
                    residualCount > 1 || externalCount > 1)
                {
                    overused = true;
                    intervalFailure = true;
                    continue;
                }
                if (patchCount < 1)
                {
                    underused = true;
                    intervalFailure = true;
                    continue;
                }

                if (onOwnerBoundary)
                {
                    if (residualCount == 0 && externalCount == 1)
                    {
                        externalUnsplit |= intervalExternalUnsplit;
                        intervalFailure |= intervalExternalUnsplit;
                    }
                    else if (residualCount == 0 && externalCount == 0)
                    {
                        underused = true;
                        intervalFailure = true;
                    }
                    else
                    {
                        ambiguous = true;
                        intervalFailure = true;
                    }
                }
                else
                {
                    if (residualCount == 1 && externalCount == 0)
                    {
                        continue;
                    }
                    if (residualCount == 0 && externalCount == 0)
                    {
                        residualMissing = true;
                        intervalFailure = true;
                    }
                    else
                    {
                        ambiguous = true;
                        intervalFailure = true;
                    }
                }
            }

            bool exactTwoUse = exactTotal == 2;
            ChamferContainedBoundaryClassification classification;
            if (overused)
            {
                classification =
                    ChamferContainedBoundaryClassification.Overused;
            }
            else if (ambiguous ||
                CountChamferContainedBoundaryDefectKinds(
                    residualMissing,
                    externalUnsplit,
                    underused) > 1)
            {
                classification =
                    ChamferContainedBoundaryClassification.Ambiguous;
            }
            else if (residualMissing)
            {
                classification =
                    ChamferContainedBoundaryClassification.ResidualMissing;
            }
            else if (externalUnsplit)
            {
                classification =
                    ChamferContainedBoundaryClassification.ExternalUnsplit;
            }
            else if (underused || intervals.Count == 0)
            {
                classification =
                    ChamferContainedBoundaryClassification.Underused;
            }
            else if (!intervalFailure && exactTwoUse)
            {
                classification =
                    ChamferContainedBoundaryClassification.ExactValid;
            }
            else if (!intervalFailure)
            {
                classification =
                    ChamferContainedBoundaryClassification.SplitEquivalent;
            }
            else
            {
                classification =
                    ChamferContainedBoundaryClassification.Ambiguous;
            }

            string diagnostic =
                "class=" + classification +
                ",ownerBoundary=" + (onOwnerBoundary ? 1 : 0) +
                ",exact=" + exactTotal + "/" + exactPatch + "/" +
                    exactResidual + "/" + exactOtherReplacement + "/" +
                    exactBevel + "/" + exactOtherPatch +
                ",intervals=" + intervals.Count;
            return new ChamferContainedBoundarySegmentAudit(
                classification,
                exactTwoUse,
                diagnostic);
        }

        private static ChamferContainedBoundaryClassification
            ClassifyChamferContainedBoundaryCandidate(
                int segmentCount,
                int exactValid,
                int splitEquivalent,
                int residualMissing,
                int externalUnsplit,
                int underused,
                int overused,
                int ambiguous)
        {
            if (segmentCount > 0 && exactValid == segmentCount)
            {
                return ChamferContainedBoundaryClassification.ExactValid;
            }
            int activeKinds = 0;
            ChamferContainedBoundaryClassification selected =
                ChamferContainedBoundaryClassification.Ambiguous;
            RegisterChamferContainedBoundaryCandidateKind(
                splitEquivalent,
                ChamferContainedBoundaryClassification.SplitEquivalent,
                ref activeKinds,
                ref selected);
            RegisterChamferContainedBoundaryCandidateKind(
                residualMissing,
                ChamferContainedBoundaryClassification.ResidualMissing,
                ref activeKinds,
                ref selected);
            RegisterChamferContainedBoundaryCandidateKind(
                externalUnsplit,
                ChamferContainedBoundaryClassification.ExternalUnsplit,
                ref activeKinds,
                ref selected);
            RegisterChamferContainedBoundaryCandidateKind(
                underused,
                ChamferContainedBoundaryClassification.Underused,
                ref activeKinds,
                ref selected);
            RegisterChamferContainedBoundaryCandidateKind(
                overused,
                ChamferContainedBoundaryClassification.Overused,
                ref activeKinds,
                ref selected);
            RegisterChamferContainedBoundaryCandidateKind(
                ambiguous,
                ChamferContainedBoundaryClassification.Ambiguous,
                ref activeKinds,
                ref selected);
            return activeKinds == 1
                ? selected
                : ChamferContainedBoundaryClassification.Ambiguous;
        }

        private static void RegisterChamferContainedBoundaryCandidateKind(
            int count,
            ChamferContainedBoundaryClassification classification,
            ref int activeKinds,
            ref ChamferContainedBoundaryClassification selected)
        {
            if (count <= 0)
            {
                return;
            }
            activeKinds++;
            selected = classification;
        }

        private static void RegisterChamferContainedBoundaryAudit(
            ChamferContainedPatchCandidate candidate,
            ChamferContainedBoundaryAudit audit,
            ref ChamferEmissionStats stats)
        {
            stats.PatchContainedBoundaryCandidates++;
            switch (audit.Classification)
            {
                case ChamferContainedBoundaryClassification.ExactValid:
                    stats.PatchContainedBoundaryExactValid++;
                    break;
                case ChamferContainedBoundaryClassification.SplitEquivalent:
                    stats.PatchContainedBoundarySplitEquivalent++;
                    break;
                case ChamferContainedBoundaryClassification.ResidualMissing:
                    stats.PatchContainedBoundaryResidualMissing++;
                    break;
                case ChamferContainedBoundaryClassification.ExternalUnsplit:
                    stats.PatchContainedBoundaryExternalUnsplit++;
                    break;
                case ChamferContainedBoundaryClassification.Underused:
                    stats.PatchContainedBoundaryUnderused++;
                    break;
                case ChamferContainedBoundaryClassification.Overused:
                    stats.PatchContainedBoundaryOverused++;
                    break;
                default:
                    stats.PatchContainedBoundaryAmbiguous++;
                    break;
            }

            stats.PatchContainedBoundarySegments += audit.SegmentCount;
            stats.PatchContainedBoundarySegmentExactValid +=
                audit.ExactValidSegments;
            stats.PatchContainedBoundarySegmentSplitEquivalent +=
                audit.SplitEquivalentSegments;
            stats.PatchContainedBoundarySegmentResidualMissing +=
                audit.ResidualMissingSegments;
            stats.PatchContainedBoundarySegmentExternalUnsplit +=
                audit.ExternalUnsplitSegments;
            stats.PatchContainedBoundarySegmentUnderused +=
                audit.UnderusedSegments;
            stats.PatchContainedBoundarySegmentOverused +=
                audit.OverusedSegments;
            stats.PatchContainedBoundarySegmentAmbiguous +=
                audit.AmbiguousSegments;

#if UNITY_EDITOR
            if (EnableVerboseChamferDiagnostics &&
                audit.Classification !=
                    ChamferContainedBoundaryClassification.ExactValid &&
                LoggedChamferContainedBoundaryClassifications.Add(
                    audit.Classification))
            {
                LogChamferNoStackTrace(
                    "GeneratedMass contained-boundary diagnostic. " +
                    "loop=" + candidate.PatchLoopIndex +
                    ", ownerRecord=" +
                        candidate.OwnerPrePatchRecordIndex +
                    ", classification=" + audit.Classification +
                    ", segments=" + audit.SegmentCount +
                    (string.IsNullOrEmpty(audit.Diagnostic)
                        ? string.Empty
                        : ", " + audit.Diagnostic),
                    false);
            }
#endif
        }

        private static ChamferContainedRepartitionShadowAudit
            AuditChamferContainedRepartitionShadow(
                ChamferVertexPatchPlan plan,
                List<ChamferProvisionalFaceRecord> patchRecords,
                List<ChamferProvisionalFaceRecord> transformedRecords,
                HashSet<TopologyEdgeKey> baselineUnexpectedOpenEdges,
                HashSet<TopologyEdgeKey> baselineSourceBoundaryFailures,
                EdgeWearTopologyStats baselineTopology,
                float minimumStableEdgeLength,
                float minimumPatchTriangleArea)
        {
            List<PolygonFace> transformedFaces =
                ExtractChamferProvisionalFaces(transformedRecords);
            Dictionary<TopologyEdgeKey, int> useCounts =
                BuildTopologyEdgeUseCounts(transformedFaces);
            HashSet<TopologyEdgeKey> unexpectedOpenEdges =
                BuildChamferUnexpectedOpenEdgeSet(
                    useCounts,
                    plan.FinalSourceBoundaryEdges);
            unexpectedOpenEdges.ExceptWith(
                baselineUnexpectedOpenEdges);
            HashSet<TopologyEdgeKey> sourceBoundaryFailures =
                BuildChamferSourceBoundaryFailureSet(
                    useCounts,
                    plan.FinalSourceBoundaryEdges);
            sourceBoundaryFailures.ExceptWith(
                baselineSourceBoundaryFailures);
            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                transformedFaces,
                minimumStableEdgeLength);
            bool tJunctionIncrease = topology.TJunctionCount >
                baselineTopology.TJunctionCount;
            bool nonManifoldIncrease = topology.NonManifoldEdgeCount >
                baselineTopology.NonManifoldEdgeCount;
            bool unexpectedOpenEdgeIncrease =
                unexpectedOpenEdges.Count > 0;
            bool sourceBoundaryIncrease =
                sourceBoundaryFailures.Count > 0;
            bool topologyClean = !tJunctionIncrease &&
                !nonManifoldIncrease &&
                !unexpectedOpenEdgeIncrease &&
                !sourceBoundaryIncrease;
            bool overlapRemoved =
                !DoesChamferContainedPatchStillOverlapReplacement(
                    patchRecords,
                    transformedRecords,
                    minimumStableEdgeLength,
                    minimumPatchTriangleArea);
            return new ChamferContainedRepartitionShadowAudit(
                overlapRemoved,
                topologyClean,
                tJunctionIncrease,
                unexpectedOpenEdgeIncrease,
                sourceBoundaryIncrease,
                nonManifoldIncrease);
        }

        private static void RegisterChamferContainedRepartitionShadow(
            ChamferContainedRepartitionShadowAudit audit,
            ref ChamferEmissionStats stats)
        {
            stats.PatchContainedShadowTested++;
            if (audit.OverlapRemoved)
            {
                stats.PatchContainedShadowOverlapRemoved++;
            }
            if (audit.TopologyClean)
            {
                stats.PatchContainedShadowTopologyClean++;
            }
            if (audit.TJunctionIncrease)
            {
                stats.PatchContainedShadowTJunctionIncrease++;
            }
            if (audit.UnexpectedOpenEdgeIncrease)
            {
                stats.PatchContainedShadowUnexpectedOpenEdgeIncrease++;
            }
            if (audit.SourceBoundaryIncrease)
            {
                stats.PatchContainedShadowSourceBoundaryIncrease++;
            }
            if (audit.NonManifoldIncrease)
            {
                stats.PatchContainedShadowNonManifoldIncrease++;
            }
        }

        private static List<ChamferContainedBoundaryEdgeRecord>
            BuildChamferContainedBoundaryEdgeRecords(
                List<ChamferProvisionalFaceRecord> records)
        {
            List<ChamferContainedBoundaryEdgeRecord> result =
                new List<ChamferContainedBoundaryEdgeRecord>();
            for (int recordIndex = 0;
                 recordIndex < records.Count;
                 recordIndex++)
            {
                ChamferProvisionalFaceRecord record = records[recordIndex];
                PolygonFace face = record.Face;
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 2)
                {
                    continue;
                }
                for (int edgeIndex = 0;
                     edgeIndex < face.Vertices.Count;
                     edgeIndex++)
                {
                    Vector3 start = face.Vertices[edgeIndex];
                    Vector3 end = face.Vertices[
                        (edgeIndex + 1) % face.Vertices.Count];
                    if (new VertexKey(start).Equals(new VertexKey(end)))
                    {
                        continue;
                    }
                    result.Add(new ChamferContainedBoundaryEdgeRecord(
                        start,
                        end,
                        record.Kind,
                        record.SourceFaceIndex,
                        record.PatchLoopIndex));
                }
            }
            return result;
        }

        private static ChamferContainedBoundaryEdgeOwner
            ClassifyChamferContainedBoundaryEdgeOwner(
                ChamferContainedPatchCandidate candidate,
                ChamferProvisionalFaceRecord ownerRecord,
                ChamferContainedBoundaryEdgeRecord edge)
        {
            if (edge.Kind == ChamferProvisionalFaceKind.VertexPatch)
            {
                return edge.PatchLoopIndex == candidate.PatchLoopIndex
                    ? ChamferContainedBoundaryEdgeOwner.TargetPatch
                    : ChamferContainedBoundaryEdgeOwner.OtherPatch;
            }
            if (edge.Kind == ChamferProvisionalFaceKind.BevelStrip)
            {
                return ChamferContainedBoundaryEdgeOwner.Bevel;
            }
            return edge.SourceFaceIndex == ownerRecord.SourceFaceIndex
                ? ChamferContainedBoundaryEdgeOwner.ResidualOwner
                : ChamferContainedBoundaryEdgeOwner.OtherReplacement;
        }

        private static bool TryBuildChamferContainedBoundaryInterval(
            ChamferBoundarySegment boundary,
            ChamferContainedBoundaryEdgeRecord edge,
            ChamferContainedBoundaryEdgeOwner edgeOwner,
            float tolerance,
            out ChamferContainedBoundaryInterval interval)
        {
            interval = default;
            Vector3 direction = boundary.End - boundary.Start;
            float lengthSquared = direction.sqrMagnitude;
            if (lengthSquared <= tolerance * tolerance)
            {
                return false;
            }
            float edgeStartParameter = Vector3.Dot(
                edge.Start - boundary.Start,
                direction) / lengthSquared;
            float edgeEndParameter = Vector3.Dot(
                edge.End - boundary.Start,
                direction) / lengthSquared;
            Vector3 projectedStart = boundary.Start +
                direction * edgeStartParameter;
            Vector3 projectedEnd = boundary.Start +
                direction * edgeEndParameter;
            float toleranceSquared = tolerance * tolerance;
            if ((edge.Start - projectedStart).sqrMagnitude >
                    toleranceSquared ||
                (edge.End - projectedEnd).sqrMagnitude >
                    toleranceSquared)
            {
                return false;
            }
            float rawStart = Mathf.Min(
                edgeStartParameter,
                edgeEndParameter);
            float rawEnd = Mathf.Max(
                edgeStartParameter,
                edgeEndParameter);
            float start = Mathf.Max(0f, rawStart);
            float end = Mathf.Min(1f, rawEnd);
            float parameterEpsilon = tolerance /
                Mathf.Sqrt(lengthSquared);
            if (end - start <= parameterEpsilon)
            {
                return false;
            }
            interval = new ChamferContainedBoundaryInterval(
                start,
                end,
                rawStart,
                rawEnd,
                edgeOwner);
            return true;
        }

        private static bool IsChamferContainedBoundarySegmentCovered(
            ChamferBoundarySegment target,
            List<ChamferBoundarySegment> sourceSegments,
            float tolerance)
        {
            List<ChamferContainedBoundaryInterval> intervals =
                new List<ChamferContainedBoundaryInterval>();
            for (int i = 0; i < sourceSegments.Count; i++)
            {
                ChamferContainedBoundaryEdgeRecord source =
                    new ChamferContainedBoundaryEdgeRecord(
                        sourceSegments[i].Start,
                        sourceSegments[i].End,
                        ChamferProvisionalFaceKind.ReplacementBase,
                        -1,
                        -1);
                if (TryBuildChamferContainedBoundaryInterval(
                        target,
                        source,
                        ChamferContainedBoundaryEdgeOwner.ResidualOwner,
                        tolerance,
                        out ChamferContainedBoundaryInterval interval))
                {
                    intervals.Add(interval);
                }
            }
            if (intervals.Count == 0)
            {
                return false;
            }
            intervals.Sort((left, right) =>
            {
                int startComparison = left.Start.CompareTo(right.Start);
                return startComparison != 0
                    ? startComparison
                    : left.End.CompareTo(right.End);
            });
            Vector3 direction = target.End - target.Start;
            float length = direction.magnitude;
            float parameterEpsilon = length > 0f
                ? tolerance / length
                : 1f;
            float coveredEnd = 0f;
            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Start > coveredEnd + parameterEpsilon)
                {
                    return false;
                }
                coveredEnd = Mathf.Max(coveredEnd, intervals[i].End);
                if (coveredEnd >= 1f - parameterEpsilon)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AddChamferContainedBoundaryBreakpoint(
            List<float> breakpoints,
            float value,
            float epsilon)
        {
            for (int i = 0; i < breakpoints.Count; i++)
            {
                if (Mathf.Abs(breakpoints[i] - value) <= epsilon)
                {
                    return;
                }
            }
            breakpoints.Add(Mathf.Clamp01(value));
        }

        private static void IncrementChamferContainedBoundaryOwnerCount(
            ChamferContainedBoundaryEdgeOwner owner,
            ref int patch,
            ref int residual,
            ref int otherReplacement,
            ref int bevel,
            ref int otherPatch)
        {
            switch (owner)
            {
                case ChamferContainedBoundaryEdgeOwner.TargetPatch:
                    patch++;
                    break;
                case ChamferContainedBoundaryEdgeOwner.ResidualOwner:
                    residual++;
                    break;
                case ChamferContainedBoundaryEdgeOwner.OtherReplacement:
                    otherReplacement++;
                    break;
                case ChamferContainedBoundaryEdgeOwner.Bevel:
                    bevel++;
                    break;
                default:
                    otherPatch++;
                    break;
            }
        }

        private static int CountChamferContainedBoundaryDefectKinds(
            bool residualMissing,
            bool externalUnsplit,
            bool underused)
        {
            int count = 0;
            if (residualMissing)
            {
                count++;
            }
            if (externalUnsplit)
            {
                count++;
            }
            if (underused)
            {
                count++;
            }
            return count;
        }

        #endregion
    }
}
