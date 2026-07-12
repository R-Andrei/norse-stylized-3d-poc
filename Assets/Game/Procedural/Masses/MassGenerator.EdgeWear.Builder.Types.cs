using System.Collections.Generic;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear builder result boundary

        private sealed class ChamferBuildArtifacts
        {
            public readonly ChamferVertexPatchPlan Plan;
            public readonly HashSet<int> FailedSliverLoopIndices;
            public readonly List<ChamferProvisionalFaceRecord>
                PrePatchFaceRecords;
            public readonly List<ChamferProvisionalFaceRecord>
                SuccessfulPatchRecords;
            public readonly ChamferTopologyContext Context;
            public readonly List<ChamferExpectedVertexBoundary>
                NormalizedVertexBoundaries;
            public readonly List<ChamferSourceBoundaryRecord>
                SourceBoundaryRecords;
            public readonly float MinimumStableEdgeLength;
            public readonly float MinimumStableFaceArea;
            public readonly float MinimumPatchTriangleArea;
            public readonly Dictionary<int, ChamferSharedEdgeSpan> SharedSpans;
            public readonly List<ChamferContainedPatchCandidate>
                ContainedPatchCandidates;

            public ChamferBuildArtifacts(
                ChamferVertexPatchPlan plan,
                HashSet<int> failedSliverLoopIndices,
                List<ChamferProvisionalFaceRecord> prePatchFaceRecords,
                List<ChamferProvisionalFaceRecord> successfulPatchRecords,
                ChamferTopologyContext context,
                List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries,
                List<ChamferSourceBoundaryRecord> sourceBoundaryRecords,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                float minimumPatchTriangleArea,
                Dictionary<int, ChamferSharedEdgeSpan> sharedSpans,
                List<ChamferContainedPatchCandidate>
                    containedPatchCandidates)
            {
                Plan = plan;
                FailedSliverLoopIndices = failedSliverLoopIndices;
                PrePatchFaceRecords = prePatchFaceRecords;
                SuccessfulPatchRecords = successfulPatchRecords;
                Context = context;
                NormalizedVertexBoundaries = normalizedVertexBoundaries;
                SourceBoundaryRecords = sourceBoundaryRecords;
                MinimumStableEdgeLength = minimumStableEdgeLength;
                MinimumStableFaceArea = minimumStableFaceArea;
                MinimumPatchTriangleArea = minimumPatchTriangleArea;
                SharedSpans = sharedSpans;
                ContainedPatchCandidates = containedPatchCandidates;
            }
        }


        private sealed class ChamferFaceIntersectionCache
        {
            public readonly List<ChamferProvisionalFaceRecord> Records;
            public readonly List<List<ChamferDirectedTriangleGeometry>>
                FanTriangles;
            public readonly List<List<ChamferDirectedTriangleGeometry>>
                PolygonTriangles;
            public readonly List<List<ChamferBoundarySegment>>
                FaceBoundarySegments;
            public readonly List<bool> PolygonTriangulationFailures;

            public ChamferFaceIntersectionCache(
                List<ChamferProvisionalFaceRecord> records,
                List<List<ChamferDirectedTriangleGeometry>> fanTriangles,
                List<List<ChamferDirectedTriangleGeometry>>
                    polygonTriangles,
                List<List<ChamferBoundarySegment>> faceBoundarySegments,
                List<bool> polygonTriangulationFailures)
            {
                Records = records;
                FanTriangles = fanTriangles;
                PolygonTriangles = polygonTriangles;
                FaceBoundarySegments = faceBoundarySegments;
                PolygonTriangulationFailures =
                    polygonTriangulationFailures;
            }
        }

        private enum ChamferPatchOverlapKind
        {
            None,
            PatchContainedInReplacement,
            ReplacementContainedInPatch,
            PartialCoplanarArea,
            NonCoplanarPenetration,
            BevelStripPenetration,
            Unclassified
        }

        private sealed class ChamferPatchOverlapClassification
        {
            public ChamferPatchOverlapKind Kind;
            public bool HasBlockingOverlap;
            public bool HasBoundaryOwner;
            public int ContainingReplacementCount;
            public int ContainingBoundaryOwnerCount;
            public int DeterministicOwnerRecordIndex = -1;
            public long ProjectedAreaNanounits;
        }

        private sealed class ChamferContainedPatchCandidate
        {
            public readonly int PatchLoopIndex;
            public readonly int OwnerPrePatchRecordIndex;
            public readonly int ContainingReplacementCount;
            public readonly int BoundaryOwnerCount;

            public bool HasDeterministicOwner =>
                ContainingReplacementCount > 0 &&
                OwnerPrePatchRecordIndex >= 0 &&
                BoundaryOwnerCount == 1;

            public ChamferContainedPatchCandidate(
                int patchLoopIndex,
                int ownerPrePatchRecordIndex,
                int containingReplacementCount,
                int boundaryOwnerCount)
            {
                PatchLoopIndex = patchLoopIndex;
                OwnerPrePatchRecordIndex = ownerPrePatchRecordIndex;
                ContainingReplacementCount = containingReplacementCount;
                BoundaryOwnerCount = boundaryOwnerCount;
            }
        }

        #endregion
    }
}
