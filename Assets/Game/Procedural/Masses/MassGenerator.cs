using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
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
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            Vector3 dimensions = ResolveDimensions(recipe);

            TriangleSoup soup = BuildMassSoup(recipe, surfaceFeatures);

            ApplyDimensions(soup.Positions, dimensions);
            ApplyLean(soup.Positions, recipe.Lean, recipe.ShapeSeed);
            ApplyGrounding(soup.Positions, recipe.Grounding);
            RecenterOnGround(soup.Positions);

            return BuildMeshData(soup, recipe);
        }

        private static bool UsesRadialBuilder(MassArchetype archetype)
        {
            return archetype == MassArchetype.PolishedStone;
        }

        private static TriangleSoup BuildMassSoup(
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures)
        {
            return recipe.Archetype switch
            {
                MassArchetype.LayeredStone => BuildLayeredStoneMass(recipe),
                MassArchetype.CarvedMarkerStone => BuildCarvedMarkerMass(recipe),
                _ => UsesRadialBuilder(recipe.Archetype)
                    ? BuildRadialMass(recipe)
                    : BuildPlaneCutMass(recipe, surfaceFeatures)
            };
        }
    }
}
