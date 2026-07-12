using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear diagnostic types

        private sealed class ChamferBoundaryOccurrenceAudit
        {
            public int MissingOpposite;
            public int DuplicateOpposite;
            public int DirectionMismatch;
            public int ExtraPatchBoundary;

            public int Total =>
                MissingOpposite +
                DuplicateOpposite +
                DirectionMismatch +
                ExtraPatchBoundary;
        }

        private sealed class ChamferSliverComponentDelta
        {
            public int PreCollapseComponents;
            public int PostCollapseComponents;
            public int ReservedPreCollapseComponents;
            public int ExactComponentMatches;
            public int DisappearedPreComponents;
            public int MergedPostComponents;
            public int SplitPreComponents;
            public int MissingLoopCount;
            public string Diagnostic = "none";
        }


        private sealed class ChamferReservedSliverPatch
        {
            public readonly int LoopIndex;
            public readonly ChamferProvisionalFaceRecord Record;
            public readonly ChamferDirectedTriangleGeometry Geometry;
            public readonly List<int> BoundaryHalfEdgeIndices;
            public readonly List<ChamferBoundarySegment> BoundarySegments;
            public readonly VertexKey RemovedKey;
            public readonly VertexKey RepresentativeKey;

            public ChamferReservedSliverPatch(
                int loopIndex,
                ChamferProvisionalFaceRecord record,
                ChamferDirectedTriangleGeometry geometry,
                List<int> boundaryHalfEdgeIndices,
                List<ChamferBoundarySegment> boundarySegments,
                VertexKey removedKey,
                VertexKey representativeKey)
            {
                LoopIndex = loopIndex;
                Record = record;
                Geometry = geometry;
                BoundaryHalfEdgeIndices = boundaryHalfEdgeIndices;
                BoundarySegments = boundarySegments;
                RemovedKey = removedKey;
                RepresentativeKey = representativeKey;
            }
        }

        #endregion
    }
}
