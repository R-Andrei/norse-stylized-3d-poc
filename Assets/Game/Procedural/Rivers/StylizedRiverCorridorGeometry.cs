using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Geometry.Ground;

namespace ProgrammaticStylized3D.Rivers
{
    public readonly struct StylizedRiverCorridorBuildResult
    {
        public StylizedRiverCorridorBuildResult(
            int ringCount,
            int acrossVertexCount,
            int triangleCount,
            int colliderTriangleCount,
            float maximumOuterWidth,
            float maximumHandoffWidth,
            float integrationApronWidth,
            bool usedGroundHeightField,
            bool tightBendWarning)
        {
            RingCount = ringCount;
            AcrossVertexCount = acrossVertexCount;
            TriangleCount = triangleCount;
            ColliderTriangleCount = colliderTriangleCount;
            MaximumOuterWidth = maximumOuterWidth;
            MaximumHandoffWidth = maximumHandoffWidth;
            IntegrationApronWidth = integrationApronWidth;
            UsedGroundHeightField = usedGroundHeightField;
            TightBendWarning = tightBendWarning;
        }

        public int RingCount { get; }
        public int AcrossVertexCount { get; }
        public int TriangleCount { get; }
        public int ColliderTriangleCount { get; }
        public float MaximumOuterWidth { get; }
        public float MaximumHandoffWidth { get; }
        public float IntegrationApronWidth { get; }
        public bool UsedGroundHeightField { get; }
        public bool TightBendWarning { get; }
        public bool IsValid => RingCount >= 2 && AcrossVertexCount >= 3;
    }

    /// <summary>
    /// Builds the visible riverbed and banks independently from the broad ground
    /// grid. The authoritative Stage 1 domain owns distance and flow; this class
    /// only refines that domain for smooth visual geometry.
    /// </summary>
    public static class StylizedRiverCorridorGeometry
    {
        private enum CrossRegion
        {
            Centre,
            FlatBedEdge,
            BedSlope,
            HiddenCover,
            OuterBlend,
            BuriedApron
        }

        private readonly struct CrossPoint
        {
            public CrossPoint(CrossRegion region, float t)
            {
                Region = region;
                T = Mathf.Clamp01(t);
            }

            public CrossRegion Region { get; }
            public float T { get; }
        }

        private readonly struct CorridorRing
        {
            public CorridorRing(
                Vector3 centre,
                Vector3 tangent,
                Vector3 side,
                Vector3 up,
                float waterHeight,
                float leftVisibleHalfWidth,
                float rightVisibleHalfWidth,
                float leftSurfaceHalfWidth,
                float rightSurfaceHalfWidth,
                float localDistance,
                float shapeDistance)
            {
                Centre = centre;
                Tangent = tangent;
                Side = side;
                Up = up;
                WaterHeight = waterHeight;
                LeftVisibleHalfWidth = leftVisibleHalfWidth;
                RightVisibleHalfWidth = rightVisibleHalfWidth;
                LeftSurfaceHalfWidth = leftSurfaceHalfWidth;
                RightSurfaceHalfWidth = rightSurfaceHalfWidth;
                LocalDistance = localDistance;
                ShapeDistance = shapeDistance;
            }

            public Vector3 Centre { get; }
            public Vector3 Tangent { get; }
            public Vector3 Side { get; }
            public Vector3 Up { get; }
            public float WaterHeight { get; }
            public float LeftVisibleHalfWidth { get; }
            public float RightVisibleHalfWidth { get; }
            public float LeftSurfaceHalfWidth { get; }
            public float RightSurfaceHalfWidth { get; }
            public float LocalDistance { get; }
            public float ShapeDistance { get; }

            public float GetVisibleHalfWidth(float sign)
            {
                return sign < 0f
                    ? LeftVisibleHalfWidth
                    : sign > 0f
                        ? RightVisibleHalfWidth
                        : (LeftVisibleHalfWidth + RightVisibleHalfWidth) * 0.5f;
            }

            public float GetSurfaceHalfWidth(float sign)
            {
                return sign < 0f
                    ? LeftSurfaceHalfWidth
                    : sign > 0f
                        ? RightSurfaceHalfWidth
                        : (LeftSurfaceHalfWidth + RightSurfaceHalfWidth) * 0.5f;
            }
        }

        private readonly struct OuterBlendContext
        {
            public OuterBlendContext(
                float startGroundHeight,
                float endGroundHeight,
                float endGroundSlope,
                float width)
            {
                StartGroundHeight = startGroundHeight;
                EndGroundHeight = endGroundHeight;
                EndGroundSlope = endGroundSlope;
                Width = Mathf.Max(0.0001f, width);
            }

            public float StartGroundHeight { get; }
            public float EndGroundHeight { get; }
            public float EndGroundSlope { get; }
            public float Width { get; }
        }

        public static float ResolveIntegrationApronWidth(float groundGridSpacing)
        {
            // The apron must extend farther than a coarse ground triangle can
            // bridge so the heightfield's concealed-to-untouched transition is
            // always hidden beneath the corridor render mesh.
            float spacing = Mathf.Max(0.01f, groundGridSpacing);
            float cellDiagonal = spacing * 1.41421356237f;
            return Mathf.Clamp(
                Mathf.Max(0.35f, cellDiagonal * 1.10f),
                0.35f,
                3.0f);
        }

        public static float ResolveBurialOffset(float groundGridSpacing)
        {
            return Mathf.Clamp(
                Mathf.Max(0.01f, groundGridSpacing * 0.02f),
                0.01f,
                0.04f);
        }

        public static StylizedRiverCorridorBuildResult BuildMeshes(
            Transform owner,
            RiverDomainSnapshot domain,
            GeneratedGround ground,
            StylizedRiverQuality quality,
            float depth,
            float bedFlatness,
            float bankBlend,
            StylizedRiverBankProfile bankProfile,
            float terrainConformity,
            float wetClearance,
            float bankCover,
            float reservedDownwardDisplacement,
            StylizedRiverNaturalVariationSettings naturalVariation,
            Mesh renderMesh,
            Mesh colliderMesh)
        {
            if (owner == null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            if (renderMesh == null)
            {
                throw new ArgumentNullException(nameof(renderMesh));
            }

            if (colliderMesh == null)
            {
                throw new ArgumentNullException(nameof(colliderMesh));
            }

            renderMesh.Clear();
            colliderMesh.Clear();

            if (domain == null || !domain.IsValid)
            {
                return default;
            }

            float maximumRingSpacing = quality switch
            {
                StylizedRiverQuality.Low => 0.65f,
                StylizedRiverQuality.Medium => 0.35f,
                StylizedRiverQuality.High => 0.20f,
                _ => 0.35f
            };

            float maximumTurnDegrees = quality switch
            {
                StylizedRiverQuality.Low => 7f,
                StylizedRiverQuality.Medium => 4f,
                StylizedRiverQuality.High => 2.5f,
                _ => 4f
            };

            int bedSubdivisions = quality switch
            {
                StylizedRiverQuality.Low => 2,
                StylizedRiverQuality.Medium => 4,
                StylizedRiverQuality.High => 6,
                _ => 4
            };

            int slopeSubdivisions = quality switch
            {
                StylizedRiverQuality.Low => 2,
                StylizedRiverQuality.Medium => 4,
                StylizedRiverQuality.High => 6,
                _ => 4
            };

            int coverSubdivisions = quality switch
            {
                StylizedRiverQuality.Low => 2,
                StylizedRiverQuality.Medium => 3,
                StylizedRiverQuality.High => 4,
                _ => 3
            };

            int blendSubdivisions = quality switch
            {
                StylizedRiverQuality.Low => 3,
                StylizedRiverQuality.Medium => 5,
                StylizedRiverQuality.High => 8,
                _ => 5
            };

            int apronSubdivisions = quality switch
            {
                StylizedRiverQuality.Low => 2,
                StylizedRiverQuality.Medium => 3,
                StylizedRiverQuality.High => 5,
                _ => 3
            };

            List<CorridorRing> rings =
                BuildRefinedRings(
                    domain,
                    maximumRingSpacing,
                    maximumTurnDegrees,
                    out bool tightBendWarning);

            if (rings.Count < 2)
            {
                return default;
            }

            List<CrossPoint> positiveCrossPoints =
                BuildPositiveCrossPoints(
                    bedFlatness,
                    naturalVariation.BedRoughness > 0.0001f
                        ? bedSubdivisions
                        : 1,
                    slopeSubdivisions,
                    coverSubdivisions,
                    blendSubdivisions,
                    apronSubdivisions);

            int positiveCount = positiveCrossPoints.Count;
            int acrossVertexCount = positiveCount * 2 - 1;
            int colliderPositiveCount = positiveCount - apronSubdivisions;
            int colliderAcrossVertexCount = colliderPositiveCount * 2 - 1;

            MeshData renderData = new MeshData();
            MeshData colliderData = new MeshData();
            List<float> terrainIntegrationWeights =
                new List<float>();
            List<Vector3> sampledGroundNormals =
                new List<Vector3>();

            float resolvedDepth = Mathf.Max(0.05f, depth);
            float resolvedBankBlend = Mathf.Max(0.1f, bankBlend);
            float resolvedConformity = Mathf.Clamp01(terrainConformity);
            float requiredWetClearance =
                Mathf.Max(0.005f, wetClearance) +
                Mathf.Max(0f, reservedDownwardDisplacement);
            float resolvedBankCover = Mathf.Max(0.005f, bankCover);
            float resolvedBedRoughness =
                naturalVariation.ResolveSafeBedRoughness(
                    resolvedDepth,
                    requiredWetClearance);
            float safeBedScale =
                Mathf.Max(
                    naturalVariation.BedRoughnessScale,
                    maximumRingSpacing * 3f,
                    resolvedBedRoughness * 5f);
            StylizedRiverNaturalVariationSettings resolvedNaturalVariation =
                new StylizedRiverNaturalVariationSettings(
                    naturalVariation.Seed,
                    naturalVariation.BedRoughness,
                    safeBedScale,
                    naturalVariation.ShorelineIrregularity,
                    naturalVariation.ShorelineIrregularityScale,
                    naturalVariation.BankAsymmetry);
            float groundGridSpacing =
                ground != null
                    ? Mathf.Max(0.01f, ground.GridSpacing)
                    : Mathf.Max(0.05f, domain.RequestedSampleSpacing);
            float integrationApronWidth =
                ResolveIntegrationApronWidth(groundGridSpacing);
            float burialOffset =
                ResolveBurialOffset(groundGridSpacing);
            float maximumOuterWidth = 0f;
            float maximumHandoffWidth = 0f;
            bool usedGroundHeightField = false;

            for (int ringIndex = 0;
                 ringIndex < rings.Count;
                 ringIndex++)
            {
                CorridorRing ring = rings[ringIndex];
                float leftHandoffHalfWidth =
                    ring.LeftSurfaceHalfWidth + resolvedBankBlend;
                float rightHandoffHalfWidth =
                    ring.RightSurfaceHalfWidth + resolvedBankBlend;
                float leftRenderOuterHalfWidth =
                    leftHandoffHalfWidth + integrationApronWidth;
                float rightRenderOuterHalfWidth =
                    rightHandoffHalfWidth + integrationApronWidth;

                maximumHandoffWidth =
                    Mathf.Max(
                        maximumHandoffWidth,
                        leftHandoffHalfWidth + rightHandoffHalfWidth);
                maximumOuterWidth =
                    Mathf.Max(
                        maximumOuterWidth,
                        leftRenderOuterHalfWidth +
                        rightRenderOuterHalfWidth);

                OuterBlendContext leftBlendContext =
                    BuildOuterBlendContext(
                        ground,
                        ring,
                        -1f,
                        leftHandoffHalfWidth,
                        ref usedGroundHeightField);

                OuterBlendContext rightBlendContext =
                    BuildOuterBlendContext(
                        ground,
                        ring,
                        1f,
                        rightHandoffHalfWidth,
                        ref usedGroundHeightField);

                for (int acrossIndex = 0;
                     acrossIndex < acrossVertexCount;
                     acrossIndex++)
                {
                    int signedPositiveIndex =
                        acrossIndex < positiveCount - 1
                            ? positiveCount - 1 - acrossIndex
                            : acrossIndex - (positiveCount - 1);

                    float sign =
                        acrossIndex < positiveCount - 1
                            ? -1f
                            : acrossIndex == positiveCount - 1
                                ? 0f
                                : 1f;

                    CrossPoint crossPoint =
                        positiveCrossPoints[signedPositiveIndex];

                    float visibleHalfWidth =
                        ring.GetVisibleHalfWidth(sign);
                    float surfaceHalfWidth =
                        ring.GetSurfaceHalfWidth(sign);
                    float flatHalfWidth =
                        visibleHalfWidth *
                        Mathf.Clamp01(bedFlatness) *
                        0.90f;
                    float handoffHalfWidth =
                        surfaceHalfWidth + resolvedBankBlend;
                    float renderOuterHalfWidth =
                        handoffHalfWidth + integrationApronWidth;

                    float acrossDistance =
                        ResolveAcrossDistance(
                            crossPoint,
                            flatHalfWidth,
                            visibleHalfWidth,
                            surfaceHalfWidth,
                            handoffHalfWidth,
                            renderOuterHalfWidth);

                    Vector3 horizontalPosition =
                        ring.Centre +
                        ring.Side * (acrossDistance * sign);

                    GroundSurfaceSample groundSample =
                        SampleBaseSurface(
                            ground,
                            horizontalPosition,
                            ring.WaterHeight,
                            ref usedGroundHeightField);

                    OuterBlendContext blendContext =
                        sign < 0f
                            ? leftBlendContext
                            : rightBlendContext;

                    float height =
                        EvaluateHeight(
                            crossPoint,
                            acrossDistance,
                            flatHalfWidth,
                            visibleHalfWidth,
                            ring.WaterHeight,
                            groundSample.Height,
                            resolvedDepth,
                            bankProfile,
                            resolvedConformity,
                            requiredWetClearance,
                            resolvedBankCover,
                            burialOffset,
                            blendContext,
                            resolvedBedRoughness,
                            ring.ShapeDistance,
                            acrossDistance * sign,
                            resolvedNaturalVariation);

                    Vector3 worldPosition =
                        new Vector3(
                            horizontalPosition.x,
                            height,
                            horizontalPosition.z);

                    Vector2 uv = ResolveGroundUv(ground, worldPosition);
                    Vector3 localPosition =
                        owner.InverseTransformPoint(worldPosition);
                    float terrainIntegrationWeight =
                        ResolveTerrainIntegrationWeight(crossPoint);
                    // Match the immutable pre-river terrain normal. The
                    // post-concealment ground geometry is hidden beneath the
                    // corridor and must not leak its trench slope into lighting.
                    Vector3 localGroundNormal =
                        owner
                            .InverseTransformDirection(
                                groundSample.Normal)
                            .normalized;
                    Color colour =
                        new Color(
                            groundSample.SurfaceVariation,
                            0.5f,
                            0.5f,
                            1f);

                    renderData.AddVertex(localPosition, uv, colour);
                    renderData.UV2.Add(
                        new Vector4(
                            terrainIntegrationWeight,
                            groundSample.MaterialClassification,
                            0f,
                            0f));
                    terrainIntegrationWeights.Add(
                        terrainIntegrationWeight);
                    sampledGroundNormals.Add(localGroundNormal);

                    if (crossPoint.Region != CrossRegion.BuriedApron)
                    {
                        colliderData.AddVertex(localPosition, uv, colour);
                    }
                }
            }

            AddStripTriangles(
                renderData,
                rings.Count,
                acrossVertexCount);
            AddStripTriangles(
                colliderData,
                rings.Count,
                colliderAcrossVertexCount);

            ApplyTerrainMatchedNormals(
                renderData,
                terrainIntegrationWeights,
                sampledGroundNormals);

            MeshBuilder.ApplyToMesh(
                renderData,
                renderMesh,
                "PS3D_StylizedRiverCorridor");
            MeshBuilder.ApplyToMesh(
                colliderData,
                colliderMesh,
                "PS3D_StylizedRiverCorridorCollider");

            return new StylizedRiverCorridorBuildResult(
                rings.Count,
                acrossVertexCount,
                renderData.TriangleCount,
                colliderData.TriangleCount,
                maximumOuterWidth,
                maximumHandoffWidth,
                integrationApronWidth,
                usedGroundHeightField,
                tightBendWarning);
        }

        private static void AddStripTriangles(
            MeshData meshData,
            int ringCount,
            int acrossVertexCount)
        {
            for (int ringIndex = 0;
                 ringIndex < ringCount - 1;
                 ringIndex++)
            {
                int rowStart = ringIndex * acrossVertexCount;
                int nextRowStart = rowStart + acrossVertexCount;

                for (int acrossIndex = 0;
                     acrossIndex < acrossVertexCount - 1;
                     acrossIndex++)
                {
                    int a = rowStart + acrossIndex;
                    int b = a + 1;
                    int c = nextRowStart + acrossIndex;
                    int d = c + 1;

                    meshData.AddTriangle(a, c, b);
                    meshData.AddTriangle(b, c, d);
                }
            }
        }

        private static List<CorridorRing> BuildRefinedRings(
            RiverDomainSnapshot domain,
            float maximumSpacing,
            float maximumTurnDegrees,
            out bool tightBendWarning)
        {
            List<CorridorRing> rings = new List<CorridorRing>();
            IReadOnlyList<StylizedRiverSplineSample> samples = domain.Samples;
            tightBendWarning = false;

            for (int index = 0; index < samples.Count - 1; index++)
            {
                StylizedRiverSplineSample a = samples[index];
                StylizedRiverSplineSample b = samples[index + 1];
                float segmentLength =
                    Mathf.Max(0.0001f, b.Distance - a.Distance);
                float turnDegrees = Vector3.Angle(a.Tangent, b.Tangent);

                int subdivisions = Mathf.Max(
                    1,
                    Mathf.CeilToInt(segmentLength / maximumSpacing),
                    Mathf.CeilToInt(
                        turnDegrees /
                        Mathf.Max(0.1f, maximumTurnDegrees)));

                float turnRadians = turnDegrees * Mathf.Deg2Rad;
                if (turnRadians > 0.001f)
                {
                    float estimatedRadius = segmentLength / turnRadians;
                    float maximumHalfWidth =
                        Mathf.Max(
                            a.LeftSurfaceHalfWidth,
                            a.RightSurfaceHalfWidth,
                            b.LeftSurfaceHalfWidth,
                            b.RightSurfaceHalfWidth);

                    if (maximumHalfWidth > estimatedRadius * 0.80f)
                    {
                        tightBendWarning = true;
                    }
                }

                for (int step = 0; step < subdivisions; step++)
                {
                    float t = step / (float)subdivisions;
                    rings.Add(InterpolateRing(a, b, t, segmentLength));
                }
            }

            StylizedRiverSplineSample last = samples[samples.Count - 1];
            rings.Add(
                new CorridorRing(
                    last.Centre,
                    last.Tangent,
                    last.Side,
                    last.Up,
                    last.SurfaceHeight,
                    last.LeftHalfWidth,
                    last.RightHalfWidth,
                    last.LeftSurfaceHalfWidth,
                    last.RightSurfaceHalfWidth,
                    last.Distance,
                    last.Distance + domain.ConnectedDistanceOffset));

            return rings;
        }

        private static CorridorRing InterpolateRing(
            StylizedRiverSplineSample a,
            StylizedRiverSplineSample b,
            float t,
            float segmentLength)
        {
            Vector3 tangentA = a.Tangent * segmentLength;
            Vector3 tangentB = b.Tangent * segmentLength;
            Vector3 centre = Hermite(a.Centre, b.Centre, tangentA, tangentB, t);
            Vector3 tangent =
                HermiteDerivative(
                    a.Centre,
                    b.Centre,
                    tangentA,
                    tangentB,
                    t);

            tangent.y = 0f;
            if (tangent.sqrMagnitude <= 0.000001f)
            {
                tangent = Vector3.Slerp(a.Tangent, b.Tangent, t);
            }
            tangent.Normalize();

            Vector3 up = Vector3.Slerp(a.Up, b.Up, t).normalized;
            Vector3 side = Vector3.Cross(up, tangent).normalized;

            return new CorridorRing(
                centre,
                tangent,
                side,
                up,
                Mathf.Lerp(a.SurfaceHeight, b.SurfaceHeight, t),
                Mathf.Lerp(a.LeftHalfWidth, b.LeftHalfWidth, t),
                Mathf.Lerp(a.RightHalfWidth, b.RightHalfWidth, t),
                Mathf.Lerp(
                    a.LeftSurfaceHalfWidth,
                    b.LeftSurfaceHalfWidth,
                    t),
                Mathf.Lerp(
                    a.RightSurfaceHalfWidth,
                    b.RightSurfaceHalfWidth,
                    t),
                Mathf.Lerp(a.Distance, b.Distance, t),
                Mathf.Lerp(a.Distance, b.Distance, t) +
                Mathf.Lerp(
                    a.GlobalDistance - a.OrientedDistance,
                    b.GlobalDistance - b.OrientedDistance,
                    t));
        }

        private static List<CrossPoint> BuildPositiveCrossPoints(
            float bedFlatness,
            int bedSubdivisions,
            int slopeSubdivisions,
            int coverSubdivisions,
            int blendSubdivisions,
            int apronSubdivisions)
        {
            List<CrossPoint> points = new List<CrossPoint>
            {
                new CrossPoint(CrossRegion.Centre, 0f)
            };

            if (bedFlatness > 0.001f)
            {
                int resolvedBedSubdivisions = Mathf.Max(1, bedSubdivisions);

                for (int index = 1;
                     index <= resolvedBedSubdivisions;
                     index++)
                {
                    points.Add(
                        new CrossPoint(
                            CrossRegion.FlatBedEdge,
                            index / (float)resolvedBedSubdivisions));
                }
            }

            for (int index = 1; index <= slopeSubdivisions; index++)
            {
                points.Add(
                    new CrossPoint(
                        CrossRegion.BedSlope,
                        index / (float)slopeSubdivisions));
            }

            for (int index = 1; index <= coverSubdivisions; index++)
            {
                points.Add(
                    new CrossPoint(
                        CrossRegion.HiddenCover,
                        index / (float)coverSubdivisions));
            }

            for (int index = 1; index <= blendSubdivisions; index++)
            {
                points.Add(
                    new CrossPoint(
                        CrossRegion.OuterBlend,
                        index / (float)blendSubdivisions));
            }

            for (int index = 1; index <= apronSubdivisions; index++)
            {
                points.Add(
                    new CrossPoint(
                        CrossRegion.BuriedApron,
                        index / (float)apronSubdivisions));
            }

            return points;
        }

        private static float ResolveAcrossDistance(
            CrossPoint point,
            float flatHalfWidth,
            float visibleHalfWidth,
            float surfaceHalfWidth,
            float handoffHalfWidth,
            float renderOuterHalfWidth)
        {
            return point.Region switch
            {
                CrossRegion.Centre => 0f,
                CrossRegion.FlatBedEdge =>
                    Mathf.Lerp(0f, flatHalfWidth, point.T),
                CrossRegion.BedSlope =>
                    Mathf.Lerp(
                        flatHalfWidth,
                        visibleHalfWidth,
                        point.T),
                CrossRegion.HiddenCover =>
                    Mathf.Lerp(
                        visibleHalfWidth,
                        surfaceHalfWidth,
                        point.T),
                CrossRegion.OuterBlend =>
                    Mathf.Lerp(
                        surfaceHalfWidth,
                        handoffHalfWidth,
                        point.T),
                CrossRegion.BuriedApron =>
                    Mathf.Lerp(
                        handoffHalfWidth,
                        renderOuterHalfWidth,
                        point.T),
                _ => 0f
            };
        }

        private static float EvaluateHeight(
            CrossPoint point,
            float acrossDistance,
            float flatHalfWidth,
            float visibleHalfWidth,
            float waterHeight,
            float baseGroundHeight,
            float depth,
            StylizedRiverBankProfile bankProfile,
            float terrainConformity,
            float requiredWetClearance,
            float bankCover,
            float burialOffset,
            OuterBlendContext outerBlendContext,
            float resolvedBedRoughness,
            float shapeDistance,
            float signedLateralDistance,
            StylizedRiverNaturalVariationSettings naturalVariation)
        {
            float bedHeight = waterHeight - depth;

            switch (point.Region)
            {
                case CrossRegion.Centre:
                case CrossRegion.FlatBedEdge:
                {
                    float bedMask =
                        point.Region == CrossRegion.Centre
                            ? 1f
                            : 1f - SmoothStep(0.65f, 1f, point.T);

                    float bedOffset =
                        StylizedRiverNaturalVariation.EvaluateBedNoise(
                            shapeDistance,
                            signedLateralDistance,
                            naturalVariation) *
                        resolvedBedRoughness *
                        bedMask;

                    float shaped =
                        Mathf.Lerp(
                            baseGroundHeight,
                            bedHeight,
                            terrainConformity) +
                        bedOffset;

                    return Mathf.Min(
                        shaped,
                        waterHeight - requiredWetClearance);
                }

                case CrossRegion.BedSlope:
                {
                    float denominator =
                        Mathf.Max(
                            0.0001f,
                            visibleHalfWidth - flatHalfWidth);
                    float t =
                        Mathf.Clamp01(
                            (acrossDistance - flatHalfWidth) /
                            denominator);
                    float profileT = EvaluateBankProfile(bankProfile, t);
                    float authoredHeight =
                        Mathf.Lerp(
                            bedHeight,
                            waterHeight,
                            profileT);
                    float shaped =
                        Mathf.Lerp(
                            baseGroundHeight,
                            authoredHeight,
                            terrainConformity);
                    float clearanceFade =
                        1f - SmoothStep(0.72f, 1f, t);
                    float maximumHeight =
                        waterHeight -
                        requiredWetClearance * clearanceFade;

                    return Mathf.Min(shaped, maximumHeight);
                }

                case CrossRegion.HiddenCover:
                {
                    float coverT = SmoothStep(0f, 1f, point.T);
                    float mandatoryHeight =
                        Mathf.Lerp(
                            waterHeight,
                            waterHeight + bankCover,
                            coverT);
                    float preservedHeight =
                        Mathf.Max(baseGroundHeight, mandatoryHeight);

                    return Mathf.Lerp(
                        preservedHeight,
                        mandatoryHeight,
                        terrainConformity);
                }

                case CrossRegion.OuterBlend:
                {
                    float t = Mathf.Clamp01(point.T);
                    float innerHeight = waterHeight + bankCover;

                    // Preserve the sampled ground shape while applying only the
                    // offset needed to meet the authored bank at the inner edge.
                    // The offset decays with zero derivative at the handoff, so
                    // this path naturally recovers the original ground slope.
                    float preservedHeight =
                        baseGroundHeight +
                        (innerHeight -
                         outerBlendContext.StartGroundHeight) *
                        (1f - SmoothStep(0f, 1f, t));

                    // The authored path is a cubic Hermite transition that meets
                    // both the ground height and its cross-river derivative.
                    // Terrain Conformity therefore changes the bank character
                    // without reintroducing a lighting or collision crease.
                    float authoredHeight =
                        HermiteScalar(
                            innerHeight,
                            outerBlendContext.EndGroundHeight,
                            0f,
                            outerBlendContext.EndGroundSlope *
                            outerBlendContext.Width,
                            t);

                    float result =
                        Mathf.Lerp(
                            preservedHeight,
                            authoredHeight,
                            terrainConformity);

                    if (t >= 0.9999f)
                    {
                        result = outerBlendContext.EndGroundHeight;
                    }

                    return result;
                }

                case CrossRegion.BuriedApron:
                {
                    // The collider has already ended at the exact terrain
                    // handoff. The render-only apron follows the untouched base
                    // ground and sinks gently below it, hiding both its own raw
                    // edge and the coarse ground's concealment transition.
                    float buryT = SmoothStep(0f, 1f, point.T);
                    return baseGroundHeight - burialOffset * buryT;
                }

                default:
                    return baseGroundHeight;
            }
        }

        private static float EvaluateBankProfile(
            StylizedRiverBankProfile profile,
            float t)
        {
            t = Mathf.Clamp01(t);

            return profile switch
            {
                StylizedRiverBankProfile.Gentle =>
                    1f - (1f - t) * (1f - t),
                StylizedRiverBankProfile.Natural =>
                    SmoothStep(0f, 1f, t),
                StylizedRiverBankProfile.Steep =>
                    t * t * t,
                StylizedRiverBankProfile.Square =>
                    Mathf.Pow(t, 8f),
                _ => SmoothStep(0f, 1f, t)
            };
        }

        private static OuterBlendContext BuildOuterBlendContext(
            GeneratedGround ground,
            CorridorRing ring,
            float sign,
            float handoffHalfWidth,
            ref bool usedGroundHeightField)
        {
            Vector3 lateralDirection = ring.Side * Mathf.Sign(sign);

            float surfaceHalfWidth =
                ring.GetSurfaceHalfWidth(sign);

            Vector3 startPosition =
                ring.Centre +
                lateralDirection * surfaceHalfWidth;

            Vector3 handoffPosition =
                ring.Centre +
                lateralDirection * handoffHalfWidth;

            GroundSurfaceSample startSample =
                SampleBaseSurface(
                    ground,
                    startPosition,
                    ring.WaterHeight,
                    ref usedGroundHeightField);

            GroundSurfaceSample handoffSample =
                SampleBaseSurface(
                    ground,
                    handoffPosition,
                    ring.WaterHeight,
                    ref usedGroundHeightField);

            float groundSlope =
                ResolveGroundSlope(
                    handoffSample.Normal,
                    lateralDirection);

            return new OuterBlendContext(
                startSample.Height,
                handoffSample.Height,
                groundSlope,
                handoffHalfWidth - surfaceHalfWidth);
        }

        private static GroundSurfaceSample SampleBaseSurface(
            GeneratedGround ground,
            Vector3 worldPosition,
            float fallbackHeight,
            ref bool usedGroundHeightField)
        {
            if (ground != null &&
                ground.TrySampleBaseSurface(
                    worldPosition,
                    out GroundSurfaceSample sample))
            {
                usedGroundHeightField = true;
                return sample;
            }

            return new GroundSurfaceSample(
                fallbackHeight,
                Vector3.up,
                0.5f,
                0f);
        }

        private static float ResolveGroundSlope(
            Vector3 groundNormal,
            Vector3 horizontalDirection)
        {
            Vector3 direction =
                new Vector3(
                    horizontalDirection.x,
                    0f,
                    horizontalDirection.z);

            if (direction.sqrMagnitude <= 0.000001f)
            {
                return 0f;
            }

            direction.Normalize();

            float vertical = Mathf.Max(0.05f, Mathf.Abs(groundNormal.y));
            float horizontalDot =
                groundNormal.x * direction.x +
                groundNormal.z * direction.z;

            return -horizontalDot / vertical;
        }

        private static float ResolveTerrainIntegrationWeight(
            CrossPoint point)
        {
            return point.Region switch
            {
                CrossRegion.OuterBlend =>
                    SmoothStep(0.15f, 1f, point.T),
                CrossRegion.BuriedApron => 1f,
                _ => 0f
            };
        }

        private static void ApplyTerrainMatchedNormals(
            MeshData meshData,
            IReadOnlyList<float> terrainIntegrationWeights,
            IReadOnlyList<Vector3> sampledGroundNormals)
        {
            if (meshData == null ||
                terrainIntegrationWeights == null ||
                sampledGroundNormals == null ||
                terrainIntegrationWeights.Count != meshData.VertexCount ||
                sampledGroundNormals.Count != meshData.VertexCount)
            {
                return;
            }

            Vector3[] geometricNormals =
                CalculateGeometricNormals(
                    meshData.Vertices,
                    meshData.Triangles);

            meshData.Normals.Clear();

            for (int index = 0;
                 index < meshData.VertexCount;
                 index++)
            {
                Vector3 geometricNormal = geometricNormals[index];
                Vector3 groundNormal = sampledGroundNormals[index];
                float weight =
                    Mathf.Clamp01(terrainIntegrationWeights[index]);

                Vector3 blended =
                    Vector3.Slerp(
                        geometricNormal,
                        groundNormal,
                        weight);

                meshData.Normals.Add(
                    blended.sqrMagnitude > 0.000001f
                        ? blended.normalized
                        : Vector3.up);
            }
        }

        private static Vector3[] CalculateGeometricNormals(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<int> triangles)
        {
            Vector3[] normals = new Vector3[vertices.Count];

            for (int index = 0;
                 index + 2 < triangles.Count;
                 index += 3)
            {
                int a = triangles[index];
                int b = triangles[index + 1];
                int c = triangles[index + 2];

                Vector3 edgeAB = vertices[b] - vertices[a];
                Vector3 edgeAC = vertices[c] - vertices[a];
                Vector3 faceNormal = Vector3.Cross(edgeAB, edgeAC);

                if (faceNormal.sqrMagnitude <= 0.0000001f)
                {
                    continue;
                }

                normals[a] += faceNormal;
                normals[b] += faceNormal;
                normals[c] += faceNormal;
            }

            for (int index = 0;
                 index < normals.Length;
                 index++)
            {
                normals[index] =
                    normals[index].sqrMagnitude > 0.000001f
                        ? normals[index].normalized
                        : Vector3.up;
            }

            return normals;
        }

        private static Vector2 ResolveGroundUv(
            GeneratedGround ground,
            Vector3 worldPosition)
        {
            if (ground != null)
            {
                Vector3 local =
                    ground.transform.InverseTransformPoint(worldPosition);
                float patchSize = Mathf.Max(0.01f, ground.PatchSize);

                return new Vector2(
                    local.x / patchSize + 0.5f,
                    local.z / patchSize + 0.5f);
            }

            return new Vector2(
                worldPosition.x * 0.05f,
                worldPosition.z * 0.05f);
        }

        private static float HermiteScalar(
            float p0,
            float p1,
            float m0,
            float m1,
            float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        private static Vector3 Hermite(
            Vector3 p0,
            Vector3 p1,
            Vector3 m0,
            Vector3 m1,
            float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        private static Vector3 HermiteDerivative(
            Vector3 p0,
            Vector3 p1,
            Vector3 m0,
            Vector3 m1,
            float t)
        {
            float t2 = t * t;
            float h00 = 6f * t2 - 6f * t;
            float h10 = 3f * t2 - 4f * t + 1f;
            float h01 = -6f * t2 + 6f * t;
            float h11 = 3f * t2 - 2f * t;
            return h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;
        }

        private static float SmoothStep(float edge0, float edge1, float value)
        {
            float t = Mathf.InverseLerp(edge0, edge1, value);
            return t * t * (3f - 2f * t);
        }
    }
}
