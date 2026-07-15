using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Shared transformation and mesh output

        private static Vector3 ResolveDimensions(MassRecipe recipe)
        {
            System.Random random =
                CreateRandom(recipe.ShapeSeed, 0x165667B1);

            Vector3 dimensions = GetBaseDimensions(recipe.Archetype);

            float variation = recipe.ShapeDiversity switch
            {
                ShapeDiversity.Restrained => 0.08f,
                ShapeDiversity.Broad => 0.25f,
                ShapeDiversity.Wild => 0.45f,
                _ => 0.25f
            };

            Vector3 proportionFactors = new Vector3(
                RandomRange(random, 1f - variation, 1f + variation),
                RandomRange(random, 1f - variation, 1f + variation),
                RandomRange(random, 1f - variation, 1f + variation));

            float averageFactor =
                (proportionFactors.x +
                 proportionFactors.y +
                 proportionFactors.z) / 3f;

            proportionFactors /= Mathf.Max(0.001f, averageFactor);

            dimensions = Vector3.Scale(dimensions, proportionFactors);
            dimensions = Vector3.Scale(
                dimensions,
                new Vector3(
                    recipe.WidthBias,
                    recipe.HeightBias,
                    recipe.DepthBias));

            dimensions *=
                GetSizeMultiplier(recipe.Size) *
                recipe.FineScale;

            ConstrainArchetypeDimensions(
                recipe.Archetype,
                ref dimensions);

            dimensions.x = Mathf.Max(0.1f, dimensions.x);
            dimensions.y = Mathf.Max(0.1f, dimensions.y);
            dimensions.z = Mathf.Max(0.1f, dimensions.z);

            return dimensions;
        }

        private static void ConstrainArchetypeDimensions(
            MassArchetype archetype,
            ref Vector3 dimensions)
        {
            float maximumHorizontal = Mathf.Max(dimensions.x, dimensions.z);
            float minimumHorizontal = Mathf.Min(dimensions.x, dimensions.z);

            switch (archetype)
            {
                case MassArchetype.StandingStone:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        maximumHorizontal * 1.55f);
                    break;

                case MassArchetype.FlatSlab:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        minimumHorizontal * 0.42f);
                    break;

                case MassArchetype.LayeredStone:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        minimumHorizontal * 0.56f);
                    break;

                case MassArchetype.CarvedMarkerStone:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        dimensions.x * 1.95f);
                    dimensions.z = Mathf.Min(
                        dimensions.z,
                        dimensions.x * 0.58f);
                    dimensions.z = Mathf.Max(
                        dimensions.z,
                        dimensions.x * 0.28f);
                    break;

                case MassArchetype.FracturedPillar:
                    dimensions.y = Mathf.Max(
                        dimensions.y,
                        maximumHorizontal * 2.18f);
                    dimensions.x = Mathf.Min(
                        dimensions.x,
                        dimensions.y * 0.46f);
                    dimensions.z = Mathf.Min(
                        dimensions.z,
                        dimensions.y * 0.38f);
                    break;

                case MassArchetype.SquatBoulder:
                    dimensions.y = Mathf.Min(
                        dimensions.y,
                        maximumHorizontal * 0.68f);
                    break;
            }
        }

        private static void ApplyDimensions(
            List<Vector3> positions,
            Vector3 dimensions)
        {
            Vector3 halfDimensions = dimensions * 0.5f;

            for (int i = 0; i < positions.Count; i++)
            {
                positions[i] = Vector3.Scale(
                    positions[i],
                    halfDimensions);
            }
        }

        private struct MassPlacementFrame
        {
            public int ReferenceVertexCount;
            public float LeanMinimumY;
            public float LeanHeight;
            public Vector3 LeanDirection;
            public float LeanDistance;
            public float GroundingMinimumY;
            public float GroundingHeight;
            public float GroundingTop;
            public float GroundingFlatteningStrength;
            public float GroundingBroadeningStrength;
            public float RecenterMinimumY;
            public float ContactBand;
            public Vector2 ContactCentre;
            public Vector3 RecenterOffset;
        }

        private static MassPlacementFrame ResolveAndApplyMassPlacementFrame(
            List<Vector3> referencePositions,
            LeanStyle lean,
            int shapeSeed,
            GroundingStyle grounding)
        {
            MassPlacementFrame frame = default;
            frame.ReferenceVertexCount = referencePositions == null
                ? 0
                : referencePositions.Count;
            if (referencePositions == null ||
                referencePositions.Count == 0)
            {
                return frame;
            }

            ResolveLeanPlacementFrame(
                referencePositions,
                lean,
                shapeSeed,
                ref frame);
            ApplyLean(referencePositions, frame);

            ResolveGroundingPlacementFrame(
                referencePositions,
                grounding,
                ref frame);
            ApplyGrounding(referencePositions, frame);

            ResolveGroundRecenterPlacementFrame(
                referencePositions,
                ref frame);
            ApplyGroundRecenter(referencePositions, frame);

            return frame;
        }

        private static void ResolveLeanPlacementFrame(
            List<Vector3> referencePositions,
            LeanStyle lean,
            int shapeSeed,
            ref MassPlacementFrame frame)
        {
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);
            frame.LeanMinimumY = minimumY;
            frame.LeanHeight = Mathf.Max(0.001f, maximumY - minimumY);

            float leanAmount = lean switch
            {
                LeanStyle.None => 0f,
                LeanStyle.Subtle => 0.055f,
                LeanStyle.Pronounced => 0.14f,
                _ => 0f
            };
            if (leanAmount <= 0f)
            {
                frame.LeanDirection = Vector3.zero;
                frame.LeanDistance = 0f;
                return;
            }

            System.Random random =
                CreateRandom(shapeSeed, 0x5F3759DF);
            frame.LeanDirection = RandomHorizontalDirection(random);
            Bounds bounds = CalculateBounds(referencePositions);
            frame.LeanDistance = leanAmount *
                Mathf.Max(bounds.size.x, bounds.size.z);
        }

        private static void ResolveGroundingPlacementFrame(
            List<Vector3> referencePositions,
            GroundingStyle grounding,
            ref MassPlacementFrame frame)
        {
            GetGroundingSettings(
                grounding,
                out float bandFraction,
                out float flatteningStrength,
                out float broadeningStrength);
            GetVerticalRange(
                referencePositions,
                out float minimumY,
                out float maximumY);

            frame.GroundingMinimumY = minimumY;
            frame.GroundingHeight = Mathf.Max(
                0.001f,
                maximumY - minimumY);
            frame.GroundingTop = minimumY +
                frame.GroundingHeight * bandFraction;
            frame.GroundingFlatteningStrength = flatteningStrength;
            frame.GroundingBroadeningStrength = broadeningStrength;
        }

        private static void ResolveGroundRecenterPlacementFrame(
            List<Vector3> referencePositions,
            ref MassPlacementFrame frame)
        {
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

            frame.RecenterMinimumY = minimumY;
            frame.ContactBand = contactBand;
            frame.ContactCentre = contactCentre;
            frame.RecenterOffset = new Vector3(
                -contactCentre.x,
                -minimumY,
                -contactCentre.y);
        }

        private static void ApplyMassPlacementFrame(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            if (positions == null || positions.Count == 0)
            {
                return;
            }
            ApplyLean(positions, frame);
            ApplyGrounding(positions, frame);
            ApplyGroundRecenter(positions, frame);
        }

        private static void ApplyLean(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            if (frame.LeanDistance <= 0f)
            {
                return;
            }
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                Vector3 position = positions[positionIndex];
                float influence =
                    (position.y - frame.LeanMinimumY) /
                    frame.LeanHeight;
                position += frame.LeanDirection *
                    frame.LeanDistance * influence;
                positions[positionIndex] = position;
            }
        }

        private static void ApplyGrounding(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                Vector3 position = positions[positionIndex];
                if (position.y >= frame.GroundingTop)
                {
                    continue;
                }
                float influence = 1f - Mathf.InverseLerp(
                    frame.GroundingMinimumY,
                    frame.GroundingTop,
                    position.y);
                influence = Mathf.SmoothStep(0f, 1f, influence);
                position.y = Mathf.Lerp(
                    position.y,
                    frame.GroundingMinimumY,
                    frame.GroundingFlatteningStrength * influence);
                float broadening = 1f +
                    frame.GroundingBroadeningStrength * influence;
                position.x *= broadening;
                position.z *= broadening;
                positions[positionIndex] = position;
            }
        }

        private static void ApplyGroundRecenter(
            List<Vector3> positions,
            MassPlacementFrame frame)
        {
            for (int positionIndex = 0;
                 positionIndex < positions.Count;
                 positionIndex++)
            {
                positions[positionIndex] += frame.RecenterOffset;
            }
        }

        private static MeshData BuildMeshData(
            TriangleSoup soup,
            MassRecipe recipe)
        {
            if (soup.Positions.Count < 3 ||
                soup.Positions.Count % 3 != 0)
            {
                throw new InvalidOperationException(
                    "Generated mass triangle soup is invalid.");
            }

            MeshData meshData = new MeshData();
            Vector3 centre = CalculateAverage(soup.Positions);
            Bounds bounds = CalculateBounds(soup.Positions);
            float safeWidth = Mathf.Max(0.001f, bounds.size.x);
            float safeHeight = Mathf.Max(0.001f, bounds.size.y);
            float safeDepth = Mathf.Max(0.001f, bounds.size.z);

            for (int i = 0; i < soup.Positions.Count; i += 3)
            {
                int faceIndex = i / 3;
                Vector3 a = soup.Positions[i];
                Vector3 b = soup.Positions[i + 1];
                Vector3 c = soup.Positions[i + 2];

                Vector3 normal = Vector3.Cross(b - a, c - a);
                bool hasAuthoredSurfaceNormal =
                    soup.TryResolveAuthoredSurfaceNormal(
                        i,
                        out Vector3 authoredSurfaceNormal);
                Vector3 faceNormal;
                if (hasAuthoredSurfaceNormal)
                {
                    if (Vector3.Dot(normal, authoredSurfaceNormal) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }
                    faceNormal = authoredSurfaceNormal;
                }
                else
                {
                    Vector3 faceCentre = (a + b + c) / 3f;
                    if (Vector3.Dot(normal, faceCentre - centre) < 0f)
                    {
                        Vector3 temporary = b;
                        b = c;
                        c = temporary;
                        normal = -normal;
                    }

                    faceNormal = normal.sqrMagnitude > MinimumEdgeLengthSqr
                        ? normal.normalized
                        : Vector3.up;
                }
                PolygonFaceFeature faceFeature = soup.ResolveFeature(i);
                float faceFeatureStrength = soup.ResolveFeatureStrength(i);
                bool hasAuthoredSurfaceGroup =
                    soup.TryResolveAuthoredSurfaceGroup(
                        i,
                        out int authoredSurfaceGroup);

                int indexA = AddRenderedVertex(
                    meshData,
                    a,
                    i,
                    0,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                int indexB = AddRenderedVertex(
                    meshData,
                    b,
                    i + 1,
                    1,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                int indexC = AddRenderedVertex(
                    meshData,
                    c,
                    i + 2,
                    2,
                    faceIndex,
                    bounds,
                    safeWidth,
                    safeHeight,
                    safeDepth,
                    faceNormal,
                    hasAuthoredSurfaceGroup,
                    authoredSurfaceGroup,
                    recipe,
                    faceFeature,
                    faceFeatureStrength);

                meshData.AddTriangle(indexA, indexB, indexC);
            }

            return meshData;
        }

        private static int AddRenderedVertex(
            MeshData meshData,
            Vector3 position,
            int vertexIndex,
            int cornerIndex,
            int faceIndex,
            Bounds bounds,
            float width,
            float height,
            float depth,
            Vector3 faceNormal,
            bool hasAuthoredSurfaceGroup,
            int authoredSurfaceGroup,
            MassRecipe recipe,
            PolygonFaceFeature faceFeature,
            float faceFeatureStrength)
        {
            Vector2 uv = new Vector2(
                (position.x - bounds.min.x) / width,
                (position.z - bounds.min.z) / depth);

            int surfaceVariationIndex = hasAuthoredSurfaceGroup
                ? authoredSurfaceGroup
                : vertexIndex;
            float randomValue = Hash01(
                recipe.SurfaceSeed,
                surfaceVariationIndex);

            float red = Mathf.Clamp01(
                0.5f +
                (randomValue - 0.5f) *
                recipe.SurfaceVariation);

            float vertical01 = Mathf.Clamp01(
                (position.y - bounds.min.y) / height);

            float green = ResolveExposureMask(
                faceNormal,
                vertical01,
                randomValue);

            float blue = ResolveCreviceMask(
                faceNormal,
                vertical01,
                green,
                randomValue);

            // Geometry-first edge wear writes only actual generated bevel faces.
            // Broad interpolated masks remain safe here, but line-like edge wear
            // must not be reconstructed from vertex gradients or packed atlases.
            float edgeWear = faceFeature == PolygonFaceFeature.ConvexEdgeWear
                ? Mathf.Clamp01(faceFeatureStrength)
                : 0f;
            float concaveCrease = 0f;

            float dirtDeposit = ResolveDirtDepositMask(
                position,
                vertical01,
                green,
                blue,
                recipe,
                0f);

            Vector4 materialMasks = new Vector4(
                concaveCrease,
                dirtDeposit,
                edgeWear,
                0f);

            int renderedVertex = meshData.AddVertex(
                position,
                uv,
                new Color(red, green, blue, edgeWear),
                materialMasks);
            meshData.Normals.Add(faceNormal);
            return renderedVertex;
        }

        // Vertex colour material contract:
        // R = existing deterministic surface variation.
        // G = upward/flat exposure mask for lighter worn or frosted planes.
        // B = base/side/occlusion mask for darker crevice-like broad grounding.
        // A = generated convex edge-wear strength on actual bevel/chamfer faces only.
        //     It mirrors UV2.z for inspection/backward compatibility.
        //
        // UV2 material contract:
        // X = reserved for future concave crease or selected crack-darkening strength.
        // Y = dirty deposit / mineral stain area mask.
        // Z = generated convex edge-wear strength on actual bevel/chamfer faces.
        // W = reserved for future concave crease localization data.
        //
        // Line-like features need a later line/overlay or per-edge representation.
        // Broad area features remain safe as vertex/interpolated masks.
        private static float ResolveExposureMask(
            Vector3 faceNormal,
            float vertical01,
            float randomValue)
        {
            float upward = Mathf.Clamp01(faceNormal.y);
            float flatness = Mathf.Pow(upward, 1.65f);
            float upperSurface = Mathf.SmoothStep(0.08f, 0.82f, vertical01);
            float surfaceBreakup = (randomValue - 0.5f) * 0.08f;

            return Mathf.Clamp01(
                flatness *
                Mathf.Lerp(0.45f, 1f, upperSurface) +
                surfaceBreakup);
        }

        private static float ResolveCreviceMask(
            Vector3 faceNormal,
            float vertical01,
            float exposure,
            float randomValue)
        {
            // Broad area mask only. Patch 12C.2 deliberately hard-clamps this
            // to the base/lower sheltered portion of the mass; the previous
            // versions left too much baseline value and debugged as a pale wash
            // across the whole rock. This channel is broad grounding/contact
            // darkness, not cracks.
            float upperKill = Mathf.Pow(
                1f - Mathf.SmoothStep(0.22f, 0.50f, vertical01),
                1.85f);

            float baseContact = Mathf.Pow(
                1f - Mathf.SmoothStep(0.000f, 0.125f, vertical01),
                2.40f);

            float sideAmount = Mathf.SmoothStep(
                0.30f,
                0.88f,
                1f - Mathf.Abs(faceNormal.y));

            float lowerSide = sideAmount * Mathf.Pow(
                1f - Mathf.SmoothStep(0.045f, 0.285f, vertical01),
                2.20f);

            float shelteredSurface =
                Mathf.Clamp01(1f - exposure) *
                sideAmount *
                Mathf.Pow(
                    1f - Mathf.SmoothStep(0.030f, 0.265f, vertical01),
                    2.55f);

            float underside = Mathf.Clamp01(-faceNormal.y) * Mathf.Pow(
                1f - Mathf.SmoothStep(0.015f, 0.220f, vertical01),
                2.10f);

            float surfaceBreakup = (randomValue - 0.5f) * 0.006f;

            float mask =
                baseContact * 0.92f +
                lowerSide * 0.24f +
                shelteredSurface * 0.16f +
                underside * 0.30f +
                surfaceBreakup;

            return Mathf.Clamp01(mask * upperKill);
        }

        private static float ResolveDirtDepositMask(
            Vector3 position,
            float vertical01,
            float exposure,
            float crevice,
            MassRecipe recipe,
            float authoredDepositBoost)
        {
            // Environmental area mask. Patch 12C.2 makes this primarily a
            // lower-rim / lower-side buildup mask. It may crawl upward in
            // broad irregular patches, but exposed upper and mid-body faces
            // should remain mostly neutral in debug.
            float upperKill = Mathf.Pow(
                1f - Mathf.SmoothStep(0.245f, 0.470f, vertical01),
                1.65f);

            float baseBand = Mathf.Pow(
                1f - Mathf.SmoothStep(0.000f, 0.185f, vertical01),
                1.95f);

            float crawlWindow =
                Mathf.SmoothStep(0.030f, 0.125f, vertical01) *
                (1f - Mathf.SmoothStep(0.170f, 0.355f, vertical01));

            float sideShelter = Mathf.Clamp01(1f - exposure);

            float broadNoiseA = HashPosition01(
                unchecked(recipe.SurfaceSeed ^ 0x51A7E),
                new Vector3(
                    position.x * 0.58f,
                    position.y * 0.24f,
                    position.z * 0.58f));

            float broadNoiseB = HashPosition01(
                unchecked(recipe.SurfaceSeed ^ 0x6D3B1),
                new Vector3(
                    position.x * 1.05f + 2.17f,
                    position.y * 0.42f - 1.31f,
                    position.z * 1.05f + 0.43f));

            float broadPatch = Mathf.SmoothStep(
                0.46f,
                0.86f,
                broadNoiseA * 0.64f + broadNoiseB * 0.36f);

            float fineBreakup = Mathf.Lerp(
                0.55f,
                1.08f,
                HashPosition01(
                    unchecked(recipe.SurfaceSeed ^ 0xD147),
                    position * 2.05f));

            float baseDeposit = baseBand * Mathf.Lerp(0.48f, 1.08f, broadPatch);
            float upwardPatch = crawlWindow * Mathf.Pow(sideShelter, 1.25f) * broadPatch;
            float creviceCatch = crevice * sideShelter * Mathf.Lerp(0.18f, 0.48f, broadPatch);

            float depositCore =
                baseDeposit * 0.86f +
                upwardPatch * 0.64f +
                creviceCatch +
                authoredDepositBoost * 0.0f;

            return Mathf.Clamp01(depositCore * fineBreakup * upperKill);
        }

        #endregion
    }
}
