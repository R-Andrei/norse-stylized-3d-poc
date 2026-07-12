using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear overlap diagnostics
        private static void AuditChamferSuccessfulPatchIntersectionBaseline(
            List<ChamferProvisionalFaceRecord> successfulPatchRecords,
            List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
            float minimumStableEdgeLength,
            float minimumPatchTriangleArea,
            ref ChamferEmissionStats stats)
        {
            Dictionary<int, List<ChamferProvisionalFaceRecord>> byLoop =
                new Dictionary<int,
                    List<ChamferProvisionalFaceRecord>>();
            for (int i = 0; i < successfulPatchRecords.Count; i++)
            {
                ChamferProvisionalFaceRecord record =
                    successfulPatchRecords[i];
                if (!byLoop.TryGetValue(
                        record.PatchLoopIndex,
                        out List<ChamferProvisionalFaceRecord> records))
                {
                    records = new List<ChamferProvisionalFaceRecord>();
                    byLoop.Add(record.PatchLoopIndex, records);
                }
                records.Add(record);
            }
            List<int> loopIndices = new List<int>(byLoop.Keys);
            loopIndices.Sort();
            ChamferFaceIntersectionCache baselineIntersectionCache =
                BuildChamferFaceIntersectionCache(
                    prePatchFaceRecords,
                    minimumPatchTriangleArea,
                    ref stats);
            List<ChamferDirectedTriangleGeometry> accepted =
                new List<ChamferDirectedTriangleGeometry>();
            List<ChamferBoundarySegment> acceptedBoundarySegments =
                new List<ChamferBoundarySegment>();
            for (int i = 0; i < loopIndices.Count; i++)
            {
                List<ChamferProvisionalFaceRecord> records =
                    byLoop[loopIndices[i]];
                if (!TryBuildChamferCorrectedPatchTriangleGeometry(
                        records,
                        minimumPatchTriangleArea,
                        out List<ChamferDirectedTriangleGeometry>
                            triangles))
                {
                    stats.PatchCorrectedBaselineLoopsRejected++;
                    stats.PatchOverlapLoopsClassified++;
                    stats.PatchOverlapUnclassified++;
                    stats.PatchOverlapNonBoundaryOwner++;
                    continue;
                }
                List<ChamferBoundarySegment> loopBoundarySegments =
                    BuildChamferPatchBoundarySegments(records);
                int failures = AuditChamferCorrectedPatchIntersections(
                    triangles,
                    loopBoundarySegments,
                    accepted,
                    acceptedBoundarySegments,
                    baselineIntersectionCache,
                    minimumStableEdgeLength,
                    minimumPatchTriangleArea,
                    true,
                    loopIndices[i],
                    ref stats);
                if (failures > 0)
                {
                    stats.PatchCorrectedBaselineLoopsRejected++;
                    if (!AuditChamferPatchReplacementOverlapOwnership(
                            triangles,
                            loopBoundarySegments,
                            baselineIntersectionCache,
                            minimumStableEdgeLength,
                            ref stats))
                    {
                        stats.PatchOverlapLoopsClassified++;
                        stats.PatchOverlapUnclassified++;
                        stats.PatchOverlapNonBoundaryOwner++;
                    }
                }
                accepted.AddRange(triangles);
                acceptedBoundarySegments.AddRange(loopBoundarySegments);
            }
        }

        private static bool
            AuditChamferPatchReplacementOverlapOwnership(
                List<ChamferDirectedTriangleGeometry> patchTriangles,
                List<ChamferBoundarySegment> patchBoundarySegments,
                ChamferFaceIntersectionCache faceCache,
                float minimumStableEdgeLength,
                ref ChamferEmissionStats stats)
        {
            ChamferPatchOverlapClassification classification =
                ClassifyChamferPatchOverlap(
                    patchTriangles,
                    patchBoundarySegments,
                    faceCache,
                    minimumStableEdgeLength);
            if (!classification.HasBlockingOverlap)
            {
                return false;
            }

            stats.PatchOverlapLoopsClassified++;
            switch (classification.Kind)
            {
                case ChamferPatchOverlapKind.BevelStripPenetration:
                    stats.PatchOverlapBevelStripPenetration++;
                    break;
                case ChamferPatchOverlapKind.NonCoplanarPenetration:
                    stats.PatchOverlapNonCoplanarPenetration++;
                    break;
                case ChamferPatchOverlapKind.PartialCoplanarArea:
                    stats.PatchOverlapPartialCoplanarArea++;
                    break;
                case ChamferPatchOverlapKind.
                    ReplacementContainedInPatch:
                    stats.PatchOverlapReplacementContainedInPatch++;
                    break;
                case ChamferPatchOverlapKind.
                    PatchContainedInReplacement:
                    stats.PatchOverlapPatchContainedInReplacement++;
                    break;
                default:
                    stats.PatchOverlapUnclassified++;
                    break;
            }
            if (classification.HasBoundaryOwner)
            {
                stats.PatchOverlapBoundaryOwner++;
            }
            else
            {
                stats.PatchOverlapNonBoundaryOwner++;
            }
            stats.PatchOverlapProjectedAreaNanounits +=
                classification.ProjectedAreaNanounits;
            return true;
        }

        private static int AuditChamferCorrectedPatchIntersections(
            List<ChamferDirectedTriangleGeometry> candidates,
            List<ChamferBoundarySegment> candidateBoundarySegments,
            List<ChamferDirectedTriangleGeometry> accepted,
            List<ChamferBoundarySegment> acceptedBoundarySegments,
            ChamferFaceIntersectionCache faceCache,
            float minimumStableEdgeLength,
            float minimumPatchTriangleArea,
            bool baseline,
            int loopOrComponentIndex,
            ref ChamferEmissionStats stats)
        {
            int improperCount = 0;
            float epsilon = Mathf.Max(
                PointMergeDistance * 2f,
                minimumStableEdgeLength * 0.001f);
            for (int first = 0; first < candidates.Count; first++)
            {
                for (int second = first + 1;
                     second < candidates.Count;
                     second++)
                {
                    bool hit = ChamferDirectedTrianglesIntersectImproperly(
                        candidates[first],
                        candidates[second],
                        epsilon);
                    if (!hit)
                    {
                        continue;
                    }
                    improperCount++;
                }

                if (TryFindChamferImproperTriangleListIntersection(
                        candidates[first],
                        accepted,
                        candidateBoundarySegments,
                        acceptedBoundarySegments,
                        epsilon,
                        out ChamferDirectedTriangleGeometry acceptedHit,
                        out int acceptedAllowedContacts))
                {
                    improperCount++;
                }
                AddChamferAllowedBoundaryContactCount(
                    acceptedAllowedContacts,
                    baseline,
                    ref stats);

                for (int faceIndex = 0;
                     faceIndex < faceCache.Records.Count;
                     faceIndex++)
                {
                    List<ChamferDirectedTriangleGeometry> renderTriangles =
                        faceCache.FanTriangles[faceIndex];
                    List<ChamferDirectedTriangleGeometry> diagnosticTriangles =
                        faceCache.PolygonTriangles[faceIndex];
                    List<ChamferBoundarySegment> faceBoundarySegments =
                        faceCache.FaceBoundarySegments[faceIndex];

                    bool rawRenderHit = TryFindChamferIntersectingTriangle(
                        candidates[first],
                        renderTriangles,
                        epsilon,
                        out _);
                    bool rawDiagnosticHit =
                        TryFindChamferIntersectingTriangle(
                            candidates[first],
                            diagnosticTriangles,
                            epsilon,
                            out _);
                    List<ChamferDirectedTriangleGeometry> gateTriangles =
                        renderTriangles;
                    bool gateImproper =
                        TryFindChamferImproperTriangleListIntersection(
                            candidates[first],
                            gateTriangles,
                            candidateBoundarySegments,
                            faceBoundarySegments,
                            epsilon,
                            out ChamferDirectedTriangleGeometry gateHit,
                            out int gateAllowedContacts);
                    bool diagnosticImproper =
                        diagnosticTriangles.Count > 0 &&
                        TryFindChamferImproperTriangleListIntersection(
                            candidates[first],
                            diagnosticTriangles,
                            candidateBoundarySegments,
                            faceBoundarySegments,
                            epsilon,
                            out _,
                            out _);

                    AddChamferAllowedBoundaryContactCount(
                        gateAllowedContacts,
                        baseline,
                        ref stats);
                    if (rawRenderHit && !rawDiagnosticHit)
                    {
                    }
                    if (diagnosticImproper)
                    {
                    }
                    if (!gateImproper)
                    {
                        continue;
                    }

                    improperCount++;
                }
            }
            return improperCount;
        }

        private static void AddChamferAllowedBoundaryContactCount(
            int count,
            bool baseline,
            ref ChamferEmissionStats stats)
        {
            if (count <= 0)
            {
                return;
            }
        }

        private static bool
            TryFindChamferImproperTriangleListIntersection(
                ChamferDirectedTriangleGeometry candidate,
                List<ChamferDirectedTriangleGeometry> others,
                List<ChamferBoundarySegment> candidateBoundarySegments,
                List<ChamferBoundarySegment> otherBoundarySegments,
                float epsilon,
                out ChamferDirectedTriangleGeometry hit,
                out int allowedContacts)
        {
            hit = null;
            allowedContacts = 0;
            for (int i = 0; i < others.Count; i++)
            {
                if (!ChamferDirectedTrianglesIntersectImproperly(
                        candidate,
                        others[i],
                        epsilon))
                {
                    continue;
                }
                if (IsChamferTriangleIntersectionConfinedToBoundary(
                        candidate,
                        others[i],
                        candidateBoundarySegments,
                        otherBoundarySegments,
                        epsilon))
                {
                    allowedContacts++;
                    continue;
                }
                hit = others[i];
                return true;
            }
            return false;
        }

        #endregion
    }
}
