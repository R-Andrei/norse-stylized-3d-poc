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



        public struct EdgeWearDebugEdgeRecord
        {
            public int EdgeIndex;
            public Vector3 Start;
            public Vector3 End;
            public bool Selected;
            public bool Focus;

            public EdgeWearDebugEdgeRecord(
                int edgeIndex,
                Vector3 start,
                Vector3 end,
                bool selected,
                bool focus)
            {
                EdgeIndex = edgeIndex;
                Start = start;
                End = end;
                Selected = selected;
                Focus = focus;
            }
        }

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
            UnifiedBoundedPreview
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
                out previewStatus,
                out boundedPreviewStatus,
                out unifiedPreviewStatus);

#if UNITY_EDITOR
            List<Vector3> edgeDebugPositions =
                ExtractEdgeWearDebugPositions(
                    unifiedPreviewStatus.DebugEdges);
#endif
            ApplyDimensions(soup.Positions, dimensions);
#if UNITY_EDITOR
            ApplyDimensions(edgeDebugPositions, dimensions);
            ApplyLeanToDebugPositions(
                edgeDebugPositions,
                soup.Positions,
                recipe.Lean,
                recipe.ShapeSeed);
#endif
            ApplyLean(soup.Positions, recipe.Lean, recipe.ShapeSeed);
#if UNITY_EDITOR
            ApplyGroundingToDebugPositions(
                edgeDebugPositions,
                soup.Positions,
                recipe.Grounding);
#endif
            ApplyGrounding(soup.Positions, recipe.Grounding);
#if UNITY_EDITOR
            RecenterDebugPositionsOnGround(
                edgeDebugPositions,
                soup.Positions);
#endif
            RecenterOnGround(soup.Positions);
#if UNITY_EDITOR
            ApplyEdgeWearDebugPositions(
                unifiedPreviewStatus.DebugEdges,
                edgeDebugPositions);
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

        private static void ApplyLeanToDebugPositions(
            List<Vector3> debugPositions,
            List<Vector3> referencePositions,
            LeanStyle lean,
            int shapeSeed)
        {
            if (debugPositions == null ||
                debugPositions.Count == 0 ||
                referencePositions == null ||
                referencePositions.Count == 0)
            {
                return;
            }
            float leanAmount = lean switch
            {
                LeanStyle.None => 0f,
                LeanStyle.Subtle => 0.055f,
                LeanStyle.Pronounced => 0.14f,
                _ => 0f
            };
            if (leanAmount <= 0f)
            {
                return;
            }
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            float height = Mathf.Max(0.001f, maximumY - minimumY);
            System.Random random = CreateRandom(shapeSeed, 0x5F3759DF);
            Vector3 direction = RandomHorizontalDirection(random);
            Bounds bounds = CalculateBounds(referencePositions);
            float distance = leanAmount *
                Mathf.Max(bounds.size.x, bounds.size.z);
            for (int positionIndex = 0;
                 positionIndex < debugPositions.Count;
                 positionIndex++)
            {
                Vector3 position = debugPositions[positionIndex];
                float influence = (position.y - minimumY) / height;
                position += direction * distance * influence;
                debugPositions[positionIndex] = position;
            }
        }

        private static void ApplyGroundingToDebugPositions(
            List<Vector3> debugPositions,
            List<Vector3> referencePositions,
            GroundingStyle grounding)
        {
            if (debugPositions == null ||
                debugPositions.Count == 0 ||
                referencePositions == null ||
                referencePositions.Count == 0)
            {
                return;
            }
            GetGroundingSettings(
                grounding,
                out float bandFraction,
                out float flatteningStrength,
                out float broadeningStrength);
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            float height = Mathf.Max(0.001f, maximumY - minimumY);
            float groundingTop = minimumY + height * bandFraction;
            for (int positionIndex = 0;
                 positionIndex < debugPositions.Count;
                 positionIndex++)
            {
                Vector3 position = debugPositions[positionIndex];
                if (position.y >= groundingTop)
                {
                    continue;
                }
                float influence = 1f - Mathf.InverseLerp(
                    minimumY,
                    groundingTop,
                    position.y);
                influence = Mathf.SmoothStep(0f, 1f, influence);
                position.y = Mathf.Lerp(
                    position.y,
                    minimumY,
                    flatteningStrength * influence);
                float broadening = 1f +
                    broadeningStrength * influence;
                position.x *= broadening;
                position.z *= broadening;
                debugPositions[positionIndex] = position;
            }
        }

        private static void RecenterDebugPositionsOnGround(
            List<Vector3> debugPositions,
            List<Vector3> referencePositions)
        {
            if (debugPositions == null ||
                debugPositions.Count == 0 ||
                referencePositions == null ||
                referencePositions.Count == 0)
            {
                return;
            }
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            float height = Mathf.Max(0.001f, maximumY - minimumY);
            float contactBand = minimumY + height * 0.08f;
            Vector2 contactCentre = Vector2.zero;
            int contactCount = 0;
            for (int positionIndex = 0;
                 positionIndex < referencePositions.Count;
                 positionIndex++)
            {
                Vector3 position = referencePositions[positionIndex];
                if (position.y > contactBand)
                {
                    continue;
                }
                contactCentre += new Vector2(position.x, position.z);
                contactCount++;
            }
            if (contactCount > 0)
            {
                contactCentre /= contactCount;
            }
            for (int positionIndex = 0;
                 positionIndex < debugPositions.Count;
                 positionIndex++)
            {
                Vector3 position = debugPositions[positionIndex];
                position.x -= contactCentre.x;
                position.z -= contactCentre.y;
                position.y -= minimumY;
                debugPositions[positionIndex] = position;
            }
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
            out PlaneCutBevelPreviewStatus previewStatus,
            out BoundedEdgePreviewStatus boundedPreviewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedPreviewStatus)
        {
            previewStatus = default;
            boundedPreviewStatus = default;
            unifiedPreviewStatus = default;
            if (recipe.Archetype == MassArchetype.LayeredStone)
            {
                return BuildLayeredStoneMass(recipe);
            }
            if (recipe.Archetype == MassArchetype.CarvedMarkerStone)
            {
                return BuildCarvedMarkerMass(recipe);
            }
            if (UsesRadialBuilder(recipe.Archetype))
            {
                return BuildRadialMass(recipe);
            }

            return BuildPlaneCutMass(
                recipe,
                surfaceFeatures,
                edgeWearEvaluationMode,
                boundedEdgeOrdinal,
                out previewStatus,
                out boundedPreviewStatus,
                out unifiedPreviewStatus);
        }
    }
}
