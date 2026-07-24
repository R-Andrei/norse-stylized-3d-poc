using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Trees
{
    public sealed class TreeBarkMeshBuildResult
    {
        public bool Passed { get; internal set; }
        public int MeshedBranchCount { get; internal set; }
        public int VertexCount { get; internal set; }
        public int TriangleCount { get; internal set; }
        public int TipCapCount { get; internal set; }
        public int AlternateQuadDiagonalCount { get; internal set; }
        public int PhaseAlignedRingCount { get; internal set; }
        public int CurvatureRadiusClampCount { get; internal set; }
        public int EffectiveTrunkRadialSegments { get; internal set; }
        public float MaximumCrossSectionMultiplier { get; internal set; }
        public float GeneratedRootWidth { get; internal set; }
        public float GeneratedRootDepth { get; internal set; }
        public Bounds LocalBounds { get; internal set; }
        public string InputFingerprint { get; internal set; }
        public string GeometryFingerprint { get; internal set; }
        public bool RepeatabilityPassed { get; internal set; }
        public TreeBarkMeshTopologyAuditResult TopologyAudit { get; internal set; }
        public string Failure { get; internal set; }

        public void MarkRepeatabilityPassed()
        {
            RepeatabilityPassed = true;
        }

        public void MarkFailed(string failure)
        {
            Passed = false;
            RepeatabilityPassed = false;
            Failure = failure ?? string.Empty;
        }
    }

    public static class TreeBarkMeshGenerator
    {
        private const float TwoPi = Mathf.PI * 2f;
        private const float Epsilon = 0.000001f;

        private struct RenderSample
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal Vector3 Normal;
            internal Vector3 Binormal;
            internal float Radius;
            internal float NormalizedDistance;
        }

        private struct ParentFrame
        {
            internal Vector3 Position;
            internal Vector3 Tangent;
            internal Vector3 Normal;
            internal Vector3 Binormal;
            internal float Radius;
        }

        public static TreeBarkMeshBuildResult Build(
            TreeDefinition definition,
            TreeBarkMeshSettings settings,
            Mesh targetMesh)
        {
            var result = new TreeBarkMeshBuildResult();
            if (definition == null || !definition.IsValid)
            {
                result.Failure = "Tree definition is null or invalid.";
                return result;
            }

            if (settings == null)
            {
                result.Failure = "Bark mesh settings are null.";
                return result;
            }

            if (targetMesh == null)
            {
                result.Failure = "Target bark mesh is null.";
                return result;
            }

            var vertices = new List<Vector3>(4096);
            var normals = new List<Vector3>(4096);
            var tangents = new List<Vector4>(4096);
            var colours = new List<Color32>(4096);
            var uv0 = new List<Vector2>(4096);
            var triangles = new List<int>(8192);
            var branchAuditRecords = new List<TreeBarkMeshBranchAuditRecord>();
            var capAuditRecords = new List<TreeBarkMeshCapAuditRecord>();

            int meshedBranches = 0;
            int capCount = 0;
            int alternateQuadDiagonalCount = 0;
            int phaseAlignedRingCount = 0;
            int curvatureRadiusClampCount = 0;
            int effectiveTrunkRadialSegments = 0;
            float maximumCrossSectionMultiplier = 1f;
            float generatedRootWidth = 0f;
            float generatedRootDepth = 0f;
            TreeResolvedParameters resolved = definition.ResolvedParameters;
            IReadOnlyList<TreeBranchDefinition> branches = definition.Branches;
            for (int branchIndex = 0;
                 branchIndex < branches.Count;
                 branchIndex++)
            {
                TreeBranchDefinition branch = branches[branchIndex];
                IReadOnlyList<TreeCurveSample> samples = branch.Samples;
                if (samples == null || samples.Count < 2)
                {
                    result.Failure =
                        "Branch " + branch.StableBranchId +
                        " does not contain enough samples for bark meshing.";
                    targetMesh.Clear();
                    return result;
                }

                int radialSegments = settings.ResolveRadialSegments(
                    branch.BranchOrder,
                    resolved.TrunkTwistRidgeCount);
                if (branch.BranchOrder == 0)
                {
                    effectiveTrunkRadialSegments = radialSegments;
                }
                if (!AppendBranchTube(
                        definition,
                        branch,
                        samples,
                        radialSegments,
                        settings,
                        vertices,
                        normals,
                        tangents,
                        colours,
                        uv0,
                        triangles,
                        branchAuditRecords,
                        capAuditRecords,
                        ref capCount,
                        ref alternateQuadDiagonalCount,
                        ref phaseAlignedRingCount,
                        ref curvatureRadiusClampCount,
                        ref maximumCrossSectionMultiplier,
                        ref generatedRootWidth,
                        ref generatedRootDepth,
                        out string failure))
                {
                    result.Failure = failure;
                    targetMesh.Clear();
                    return result;
                }

                meshedBranches++;
            }

            if (vertices.Count == 0 || triangles.Count < 3)
            {
                result.Failure = "Bark mesh generation produced no renderable geometry.";
                targetMesh.Clear();
                return result;
            }

            result.TopologyAudit = TreeBarkMeshTopologyAudit.Run(
                definition,
                vertices,
                normals,
                tangents,
                uv0,
                triangles,
                branchAuditRecords,
                capAuditRecords);
            if (!result.TopologyAudit.Passed)
            {
                result.Failure =
                    "Bark topology audit failed.\n" +
                    result.TopologyAudit.Report;
                targetMesh.Clear();
                return result;
            }

            targetMesh.Clear();
            targetMesh.indexFormat = vertices.Count > ushort.MaxValue
                ? IndexFormat.UInt32
                : IndexFormat.UInt16;
            targetMesh.SetVertices(vertices);
            targetMesh.SetNormals(normals);
            targetMesh.SetTangents(tangents);
            targetMesh.SetColors(colours);
            targetMesh.SetUVs(0, uv0);
            targetMesh.SetTriangles(triangles, 0, true);
            targetMesh.RecalculateBounds();
            targetMesh.UploadMeshData(false);

            result.Passed = true;
            result.MeshedBranchCount = meshedBranches;
            result.VertexCount = vertices.Count;
            result.TriangleCount = triangles.Count / 3;
            result.TipCapCount = capCount;
            result.AlternateQuadDiagonalCount = alternateQuadDiagonalCount;
            result.PhaseAlignedRingCount = phaseAlignedRingCount;
            result.CurvatureRadiusClampCount = curvatureRadiusClampCount;
            result.EffectiveTrunkRadialSegments =
                effectiveTrunkRadialSegments;
            result.MaximumCrossSectionMultiplier =
                maximumCrossSectionMultiplier;
            result.GeneratedRootWidth = generatedRootWidth;
            result.GeneratedRootDepth = generatedRootDepth;
            result.LocalBounds = targetMesh.bounds;
            result.InputFingerprint = CalculateInputFingerprint(
                definition,
                settings);
            result.GeometryFingerprint = BuildGeometryFingerprint(
                vertices,
                normals,
                tangents,
                colours,
                uv0,
                triangles);
            result.Failure = string.Empty;
            return result;
        }

        private static bool AppendBranchTube(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<TreeCurveSample> sourceSamples,
            int radialSegments,
            TreeBarkMeshSettings settings,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color32> colours,
            List<Vector2> uv0,
            List<int> triangles,
            List<TreeBarkMeshBranchAuditRecord> branchAuditRecords,
            List<TreeBarkMeshCapAuditRecord> capAuditRecords,
            ref int capCount,
            ref int alternateQuadDiagonalCount,
            ref int phaseAlignedRingCount,
            ref int curvatureRadiusClampCount,
            ref float maximumCrossSectionMultiplier,
            ref float generatedRootWidth,
            ref float generatedRootDepth,
            out string failure)
        {
            failure = string.Empty;
            radialSegments = Mathf.Max(3, radialSegments);
            List<RenderSample> samples = BuildRenderSamples(
                definition,
                branch,
                sourceSamples,
                settings,
                ref curvatureRadiusClampCount);
            if (branch.BranchOrder == 0)
            {
                samples = RefineTrunkRenderSamples(
                    samples,
                    definition.ResolvedParameters);
            }

            if (samples.Count < 2)
            {
                failure =
                    "Branch " + branch.StableBranchId +
                    " produced fewer than two render samples.";
                return false;
            }

            int ringStride = radialSegments + 1;
            int firstRingVertex = vertices.Count;
            var ringPhaseOffsets = new int[samples.Count];
            float accumulatedDistance = 0f;
            Vector3 previousPosition = samples[0].Position;

            for (int sampleIndex = 0;
                 sampleIndex < samples.Count;
                 sampleIndex++)
            {
                RenderSample sample = samples[sampleIndex];
                if (!IsUsableSample(sample))
                {
                    failure =
                        "Branch " + branch.StableBranchId +
                        " contains non-finite or degenerate transported-frame data.";
                    return false;
                }

                if (sampleIndex > 0)
                {
                    accumulatedDistance += Vector3.Distance(
                        previousPosition,
                        sample.Position);
                    previousPosition = sample.Position;

                    // Physical phase repair is valid for circular branches. It
                    // must not rotate a non-circular trunk profile because that
                    // would cancel or alter authored axial twist.
                    if (branch.BranchOrder != 0)
                    {
                        int previousRing = firstRingVertex +
                            (sampleIndex - 1) * ringStride;
                        ringPhaseOffsets[sampleIndex] = ResolveBestRingPhase(
                            sample,
                            radialSegments,
                            previousRing,
                            vertices,
                            normals,
                            ringPhaseOffsets[sampleIndex - 1]);
                        if (ringPhaseOffsets[sampleIndex] !=
                            ringPhaseOffsets[sampleIndex - 1])
                        {
                            phaseAlignedRingCount++;
                        }
                    }
                }

                int phaseOffset = ringPhaseOffsets[sampleIndex];
                Color32 metadata = BuildVertexMetadata(
                    definition,
                    branch,
                    sample.Position);
                for (int side = 0; side <= radialSegments; side++)
                {
                    float authoredSide = side / (float)radialSegments;
                    float circularBranchSide =
                        (side + phaseOffset) / (float)radialSegments;
                    float geometrySide = branch.BranchOrder == 0
                        ? authoredSide
                        : circularBranchSide;

                    BuildSurfaceVertex(
                        definition,
                        branch,
                        samples,
                        sampleIndex,
                        geometrySide,
                        radialSegments,
                        out Vector3 position,
                        out Vector3 normal,
                        out Vector3 circumferenceTangent,
                        out float crossSectionMultiplier);

                    vertices.Add(position);
                    normals.Add(normal);
                    tangents.Add(new Vector4(
                        circumferenceTangent.x,
                        circumferenceTangent.y,
                        circumferenceTangent.z,
                        1f));
                    colours.Add(metadata);

                    float uvSide = branch.BranchOrder == 0
                        ? authoredSide
                        : circularBranchSide;
                    uv0.Add(new Vector2(
                        uvSide,
                        accumulatedDistance / settings.BarkMetersPerTile));

                    if (branch.BranchOrder == 0)
                    {
                        maximumCrossSectionMultiplier = Mathf.Max(
                            maximumCrossSectionMultiplier,
                            crossSectionMultiplier);
                    }
                }
            }

            if (branch.BranchOrder == 0)
            {
                CalculateRootDimensions(
                    vertices,
                    firstRingVertex,
                    radialSegments,
                    out generatedRootWidth,
                    out generatedRootDepth);
            }

            int sideTriangleStart = triangles.Count;
            for (int ring = 0; ring < samples.Count - 1; ring++)
            {
                int currentRing = firstRingVertex + ring * ringStride;
                int nextRing = currentRing + ringStride;
                for (int side = 0; side < radialSegments; side++)
                {
                    int a = currentRing + side;
                    int b = nextRing + side;
                    int c = nextRing + side + 1;
                    int d = currentRing + side + 1;

                    AppendBestOutwardQuad(
                        a,
                        b,
                        c,
                        d,
                        vertices,
                        normals,
                        triangles,
                        ref alternateQuadDiagonalCount);
                }
            }

            int zeroLengthRingSegments = 0;
            for (int sampleIndex = 1; sampleIndex < samples.Count; sampleIndex++)
            {
                if ((samples[sampleIndex].Position -
                     samples[sampleIndex - 1].Position).sqrMagnitude <= Epsilon)
                {
                    zeroLengthRingSegments++;
                }
            }

            branchAuditRecords.Add(new TreeBarkMeshBranchAuditRecord
            {
                Branch = branch,
                SideTriangleStart = sideTriangleStart,
                SideTriangleCount = triangles.Count - sideTriangleStart,
                RadialSegments = radialSegments,
                RingCount = samples.Count,
                RootCenter = samples[0].Position,
                RootRadius = samples[0].Radius,
                ZeroLengthRingSegmentCount = zeroLengthRingSegments
            });

            if (branch.BranchOrder == 0 && settings.CapTrunkBase)
            {
                AppendCap(
                    samples,
                    0,
                    radialSegments,
                    definition,
                    branch,
                    false,
                    vertices,
                    normals,
                    tangents,
                    colours,
                    uv0,
                    triangles,
                    capAuditRecords);
                capCount++;
            }

            if (settings.CapBranchTips)
            {
                AppendCap(
                    samples,
                    samples.Count - 1,
                    radialSegments,
                    definition,
                    branch,
                    true,
                    vertices,
                    normals,
                    tangents,
                    colours,
                    uv0,
                    triangles,
                    capAuditRecords);
                capCount++;
            }

            return true;
        }

        private static List<RenderSample> RefineTrunkRenderSamples(
            IReadOnlyList<RenderSample> source,
            TreeResolvedParameters parameters)
        {
            if (source == null || source.Count < 2)
            {
                return source == null
                    ? new List<RenderSample>()
                    : new List<RenderSample>(source);
            }

            const float MaximumTwistStepDegrees = 10f;
            float rootHeight = Mathf.Clamp(
                parameters.RootButtressHeight,
                0.01f,
                0.6f);
            float maximumRootStep = Mathf.Max(0.015f, rootHeight / 5f);
            var refined = new List<RenderSample>(source.Count + 24);
            refined.Add(source[0]);

            for (int index = 0; index < source.Count - 1; index++)
            {
                RenderSample a = source[index];
                RenderSample b = source[index + 1];
                float normalizedSpan = Mathf.Max(
                    0f,
                    b.NormalizedDistance - a.NormalizedDistance);
                float twistSpan = Mathf.Abs(
                    parameters.TrunkSurfaceTorsionDegrees) *
                    normalizedSpan;
                int subdivisions = Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        twistSpan / MaximumTwistStepDegrees));
                if (a.NormalizedDistance < rootHeight)
                {
                    subdivisions = Mathf.Max(
                        subdivisions,
                        Mathf.CeilToInt(
                            normalizedSpan / maximumRootStep));
                }

                subdivisions = Mathf.Clamp(subdivisions, 1, 12);
                for (int step = 1; step <= subdivisions; step++)
                {
                    float t = step / (float)subdivisions;
                    refined.Add(InterpolateRenderSample(a, b, t));
                }
            }

            RebuildTransportedFrames(refined);
            return refined;
        }

        private static RenderSample InterpolateRenderSample(
            RenderSample a,
            RenderSample b,
            float t)
        {
            Vector3 tangent = SafeNormalize(
                Vector3.Slerp(a.Tangent, b.Tangent, t),
                Vector3.Lerp(a.Tangent, b.Tangent, t));
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent);
            normal = SafeNormalize(normal, ChooseInitialNormal(tangent));
            Vector3 binormal = SafeNormalize(
                Vector3.Cross(tangent, normal),
                Vector3.Cross(tangent, ChooseInitialNormal(tangent)));
            normal = SafeNormalize(
                Vector3.Cross(binormal, tangent),
                normal);
            return new RenderSample
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Normal = normal,
                Binormal = binormal,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t),
                NormalizedDistance = Mathf.Lerp(
                    a.NormalizedDistance,
                    b.NormalizedDistance,
                    t)
            };
        }

        private static void BuildSurfaceVertex(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            float normalizedSide,
            int radialSegments,
            out Vector3 position,
            out Vector3 normal,
            out Vector3 circumferenceTangent,
            out float crossSectionMultiplier)
        {
            RenderSample sample = samples[sampleIndex];
            float angle = normalizedSide * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            Vector3 radialReference = SafeNormalize(
                sample.Normal * cosine +
                sample.Binormal * sine,
                sample.Normal);

            if (branch.BranchOrder != 0)
            {
                position = sample.Position +
                    radialReference * sample.Radius;
                normal = radialReference;
                circumferenceTangent = SafeNormalize(
                    sample.Normal * -sine +
                    sample.Binormal * cosine,
                    sample.Binormal);
                crossSectionMultiplier = 1f;
                return;
            }

            position = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide,
                out crossSectionMultiplier);
            radialReference = SafeNormalize(
                position - sample.Position,
                radialReference);

            float sideDelta = 1f /
                Mathf.Max(48f, radialSegments * 8f);
            Vector3 previousSide = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide - sideDelta,
                out _);
            Vector3 nextSide = EvaluateTrunkSurfacePosition(
                definition,
                branch,
                sample,
                normalizedSide + sideDelta,
                out _);
            circumferenceTangent = SafeNormalize(
                nextSide - previousSide,
                sample.Binormal);

            Vector3 previousLongitudinal;
            Vector3 nextLongitudinal;
            if (sampleIndex == 0)
            {
                previousLongitudinal = position;
                nextLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[1],
                    normalizedSide,
                    out _);
            }
            else if (sampleIndex == samples.Count - 1)
            {
                previousLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex - 1],
                    normalizedSide,
                    out _);
                nextLongitudinal = position;
            }
            else
            {
                previousLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex - 1],
                    normalizedSide,
                    out _);
                nextLongitudinal = EvaluateTrunkSurfacePosition(
                    definition,
                    branch,
                    samples[sampleIndex + 1],
                    normalizedSide,
                    out _);
            }

            Vector3 longitudinalTangent = SafeNormalize(
                nextLongitudinal - previousLongitudinal,
                sample.Tangent);
            normal = SafeNormalize(
                Vector3.Cross(
                    circumferenceTangent,
                    longitudinalTangent),
                radialReference);
            if (Vector3.Dot(normal, radialReference) < 0f)
            {
                normal = -normal;
            }
        }

        private static Vector3 EvaluateTrunkSurfacePosition(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            RenderSample sample,
            float normalizedSide,
            out float crossSectionMultiplier)
        {
            TreeResolvedParameters parameters =
                definition.ResolvedParameters;
            float angle = normalizedSide * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            ResolveTrunkSurfaceFrame(
                parameters,
                sample,
                out _,
                out Vector3 surfaceNormalAxis,
                out Vector3 surfaceBinormalAxis);
            Vector3 radial = SafeNormalize(
                surfaceNormalAxis * cosine +
                surfaceBinormalAxis * sine,
                surfaceNormalAxis);

            crossSectionMultiplier =
                EvaluateTrunkCrossSectionMultiplier(
                    parameters,
                    branch.Phase,
                    sample.NormalizedDistance,
                    angle);
            return sample.Position +
                radial *
                sample.Radius *
                crossSectionMultiplier;
        }

        private static void ResolveTrunkSurfaceFrame(
            TreeResolvedParameters parameters,
            RenderSample sample,
            out Vector3 tangent,
            out Vector3 normal,
            out Vector3 binormal)
        {
            float rootEnvelope = EvaluateRootEnvelope(
                parameters,
                sample.NormalizedDistance);
            Vector3 groundTangent = Vector3.up;
            Vector3 groundNormal = Vector3.right;
            Vector3 groundBinormal = Vector3.Cross(
                groundTangent,
                groundNormal).normalized;
            if (Vector3.Dot(groundNormal, sample.Normal) < 0f)
            {
                groundNormal = -groundNormal;
                groundBinormal = -groundBinormal;
            }

            tangent = SafeNormalize(
                Vector3.Slerp(
                    sample.Tangent,
                    groundTangent,
                    rootEnvelope),
                sample.Tangent);
            normal = Vector3.Slerp(
                sample.Normal,
                groundNormal,
                rootEnvelope);
            normal = Vector3.ProjectOnPlane(normal, tangent);
            normal = SafeNormalize(
                normal,
                ChooseInitialNormal(tangent));
            binormal = SafeNormalize(
                Vector3.Cross(tangent, normal),
                groundBinormal);
            normal = SafeNormalize(
                Vector3.Cross(binormal, tangent),
                normal);
        }

        private static float EvaluateRootEnvelope(
            TreeResolvedParameters parameters,
            float normalizedDistance)
        {
            float rootHeight = Mathf.Max(
                0.01f,
                parameters.RootButtressHeight);
            float rootLinear = Mathf.Clamp01(
                1f - normalizedDistance / rootHeight);
            return rootLinear * rootLinear *
                (3f - 2f * rootLinear);
        }

        private static float EvaluateTrunkCrossSectionMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance,
            float angle)
        {
            int ridgeCount = Mathf.Clamp(
                parameters.TrunkTwistRidgeCount,
                3,
                10);
            // The transported structural frame already contains the
            // existing authored trunk torsion. Keep the non-circular profile
            // fixed in that frame so the same control becomes visibly twisted
            // without being applied twice.
            float twistPhase = branchPhase * TwoPi;
            float ridgeWave = Mathf.Cos(
                ridgeCount * (angle - twistPhase));
            float ridgeDepth = Mathf.Clamp(
                parameters.TrunkTwistRidgeDepth,
                0f,
                0.45f);
            float ridgeMultiplier = 1f + ridgeDepth * ridgeWave;

            float rootEnvelope = EvaluateRootEnvelope(
                parameters,
                normalizedDistance);
            float positiveLobe = Mathf.Clamp01(
                ridgeWave * 0.5f + 0.5f);
            positiveLobe *= positiveLobe;

            float rootFlare = Mathf.Lerp(
                1f,
                Mathf.Max(1f, parameters.RootFlareScale),
                rootEnvelope);
            float buttress = 1f +
                Mathf.Max(0f, parameters.RootButtressStrength) *
                0.6f *
                rootEnvelope *
                positiveLobe;
            float asymmetry =
                1f +
                Mathf.Clamp01(parameters.TrunkIrregularity) *
                0.18f *
                rootEnvelope *
                Mathf.Sin(
                    angle * 2f +
                    branchPhase * TwoPi * 1.618034f);

            return Mathf.Max(
                0.2f,
                ridgeMultiplier *
                rootFlare *
                buttress *
                asymmetry);
        }

        private static float CalculateMaximumTrunkCrossSectionMultiplier(
            TreeResolvedParameters parameters,
            float branchPhase,
            float normalizedDistance)
        {
            int sampleCount = Mathf.Max(
                24,
                Mathf.Clamp(parameters.TrunkTwistRidgeCount, 3, 10) * 4);
            float maximum = 1f;
            for (int index = 0; index < sampleCount; index++)
            {
                float angle = index / (float)sampleCount * TwoPi;
                maximum = Mathf.Max(
                    maximum,
                    EvaluateTrunkCrossSectionMultiplier(
                        parameters,
                        branchPhase,
                        normalizedDistance,
                        angle));
            }

            return maximum;
        }

        private static void CalculateRootDimensions(
            IReadOnlyList<Vector3> vertices,
            int firstRingVertex,
            int radialSegments,
            out float width,
            out float depth)
        {
            float minimumX = float.PositiveInfinity;
            float maximumX = float.NegativeInfinity;
            float minimumZ = float.PositiveInfinity;
            float maximumZ = float.NegativeInfinity;
            for (int side = 0; side < radialSegments; side++)
            {
                Vector3 vertex = vertices[firstRingVertex + side];
                minimumX = Mathf.Min(minimumX, vertex.x);
                maximumX = Mathf.Max(maximumX, vertex.x);
                minimumZ = Mathf.Min(minimumZ, vertex.z);
                maximumZ = Mathf.Max(maximumZ, vertex.z);
            }

            width = Mathf.Max(0f, maximumX - minimumX);
            depth = Mathf.Max(0f, maximumZ - minimumZ);
        }


        private static int ResolveBestRingPhase(
            RenderSample sample,
            int radialSegments,
            int previousRing,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            int previousPhase)
        {
            int bestPhase = previousPhase;
            float bestMinimumOrientation = float.NegativeInfinity;
            float bestEdgeCost = float.PositiveInfinity;
            int bestAbsoluteDelta = int.MaxValue;
            int firstDelta = -radialSegments / 2;

            for (int candidateIndex = 0;
                 candidateIndex < radialSegments;
                 candidateIndex++)
            {
                int delta = firstDelta + candidateIndex;
                int candidatePhase = previousPhase + delta;
                float minimumOrientation = float.PositiveInfinity;
                float edgeCost = 0f;

                for (int side = 0; side < radialSegments; side++)
                {
                    int aIndex = previousRing + side;
                    int dIndex = previousRing + side + 1;
                    BuildRingVertex(
                        sample,
                        radialSegments,
                        side + candidatePhase,
                        out Vector3 bPosition,
                        out Vector3 bNormal);
                    BuildRingVertex(
                        sample,
                        radialSegments,
                        side + 1 + candidatePhase,
                        out Vector3 cPosition,
                        out Vector3 cNormal);

                    Vector3 aPosition = vertices[aIndex];
                    Vector3 dPosition = vertices[dIndex];
                    Vector3 aNormal = normals[aIndex];
                    Vector3 dNormal = normals[dIndex];

                    float currentMinimum = Mathf.Min(
                        EvaluateTriangleOrientation(
                            aPosition,
                            dPosition,
                            cPosition,
                            aNormal,
                            dNormal,
                            cNormal),
                        EvaluateTriangleOrientation(
                            aPosition,
                            cPosition,
                            bPosition,
                            aNormal,
                            cNormal,
                            bNormal));
                    float alternateMinimum = Mathf.Min(
                        EvaluateTriangleOrientation(
                            aPosition,
                            dPosition,
                            bPosition,
                            aNormal,
                            dNormal,
                            bNormal),
                        EvaluateTriangleOrientation(
                            dPosition,
                            cPosition,
                            bPosition,
                            dNormal,
                            cNormal,
                            bNormal));
                    minimumOrientation = Mathf.Min(
                        minimumOrientation,
                        Mathf.Max(currentMinimum, alternateMinimum));
                    edgeCost +=
                        (aPosition - bPosition).sqrMagnitude +
                        (dPosition - cPosition).sqrMagnitude;
                }

                int absoluteDelta = Mathf.Abs(delta);
                bool better =
                    minimumOrientation >
                        bestMinimumOrientation + 0.000001f ||
                    (Mathf.Abs(
                         minimumOrientation - bestMinimumOrientation) <=
                         0.000001f &&
                     edgeCost < bestEdgeCost - 0.000001f) ||
                    (Mathf.Abs(
                         minimumOrientation - bestMinimumOrientation) <=
                         0.000001f &&
                     Mathf.Abs(edgeCost - bestEdgeCost) <= 0.000001f &&
                     absoluteDelta < bestAbsoluteDelta);
                if (better)
                {
                    bestPhase = candidatePhase;
                    bestMinimumOrientation = minimumOrientation;
                    bestEdgeCost = edgeCost;
                    bestAbsoluteDelta = absoluteDelta;
                }
            }

            return bestPhase;
        }

        private static void BuildRingVertex(
            RenderSample sample,
            int radialSegments,
            int unwrappedSide,
            out Vector3 position,
            out Vector3 normal)
        {
            float angle =
                unwrappedSide / (float)radialSegments * TwoPi;
            float cosine = Mathf.Cos(angle);
            float sine = Mathf.Sin(angle);
            normal = SafeNormalize(
                sample.Normal * cosine +
                sample.Binormal * sine,
                sample.Normal);
            position = sample.Position + normal * sample.Radius;
        }

        private static void ApplyCurvatureRadiusSafety(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            List<RenderSample> samples,
            float minimumRenderedRadius,
            ref int curvatureRadiusClampCount)
        {
            if (samples == null || samples.Count < 3)
            {
                return;
            }

            int sampleCount = samples.Count;
            var targetRadii = new float[sampleCount];
            var maximumPhysicalRadii = new float[sampleCount];
            var crossSectionMultipliers = new float[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                targetRadii[index] = samples[index].Radius;
                maximumPhysicalRadii[index] = float.PositiveInfinity;
                crossSectionMultipliers[index] = branch.BranchOrder == 0
                    ? CalculateMaximumTrunkCrossSectionMultiplier(
                        definition.ResolvedParameters,
                        branch.Phase,
                        samples[index].NormalizedDistance)
                    : 1f;
            }

            for (int index = 1; index < sampleCount - 1; index++)
            {
                Vector3 incoming =
                    samples[index].Position - samples[index - 1].Position;
                Vector3 outgoing =
                    samples[index + 1].Position - samples[index].Position;
                float incomingLength = incoming.magnitude;
                float outgoingLength = outgoing.magnitude;
                if (incomingLength <= Epsilon || outgoingLength <= Epsilon)
                {
                    continue;
                }

                float turnAngle = Vector3.Angle(incoming, outgoing) *
                    Mathf.Deg2Rad;
                float sineHalf = Mathf.Sin(turnAngle * 0.5f);
                if (sineHalf <= 0.0001f)
                {
                    continue;
                }

                float curvatureRadius =
                    Mathf.Min(incomingLength, outgoingLength) /
                    (2f * sineHalf);
                float safePhysicalRadius = curvatureRadius * 0.65f;
                maximumPhysicalRadii[index] = Mathf.Min(
                    maximumPhysicalRadii[index],
                    safePhysicalRadius);
                maximumPhysicalRadii[index - 1] = Mathf.Min(
                    maximumPhysicalRadii[index - 1],
                    safePhysicalRadius * 1.45f);
                maximumPhysicalRadii[index + 1] = Mathf.Min(
                    maximumPhysicalRadii[index + 1],
                    safePhysicalRadius * 1.45f);
            }

            for (int index = 1; index < sampleCount; index++)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index - 1]))
                {
                    maximumPhysicalRadii[index] = Mathf.Min(
                        maximumPhysicalRadii[index],
                        maximumPhysicalRadii[index - 1] * 1.45f);
                }
            }

            for (int index = sampleCount - 2; index >= 0; index--)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index + 1]))
                {
                    maximumPhysicalRadii[index] = Mathf.Min(
                        maximumPhysicalRadii[index],
                        maximumPhysicalRadii[index + 1] * 1.45f);
                }
            }

            for (int index = 0; index < sampleCount; index++)
            {
                if (!float.IsPositiveInfinity(
                        maximumPhysicalRadii[index]))
                {
                    float allowedScalarRadius =
                        maximumPhysicalRadii[index] /
                        Mathf.Max(
                            1f,
                            crossSectionMultipliers[index]);
                    targetRadii[index] = Mathf.Min(
                        targetRadii[index],
                        allowedScalarRadius);
                }
            }

            for (int index = 0; index < sampleCount; index++)
            {
                RenderSample sample = samples[index];
                float safeRadius = Mathf.Max(
                    minimumRenderedRadius,
                    targetRadii[index]);
                if (safeRadius < sample.Radius - 0.000001f)
                {
                    sample.Radius = safeRadius;
                    samples[index] = sample;
                    curvatureRadiusClampCount++;
                }
            }
        }


        private static void AppendBestOutwardQuad(
            int a,
            int b,
            int c,
            int d,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            List<int> triangles,
            ref int alternateQuadDiagonalCount)
        {
            // A transported tube quad can become strongly skewed on highly
            // curved/twisted branches. Both diagonals are topologically valid,
            // but only one may keep both triangles aligned with the authored
            // radial normals. Select deterministically by the weaker triangle.
            float currentFirst = EvaluateTriangleOrientation(
                a,
                d,
                c,
                vertices,
                normals);
            float currentSecond = EvaluateTriangleOrientation(
                a,
                c,
                b,
                vertices,
                normals);
            float alternateFirst = EvaluateTriangleOrientation(
                a,
                d,
                b,
                vertices,
                normals);
            float alternateSecond = EvaluateTriangleOrientation(
                d,
                c,
                b,
                vertices,
                normals);

            float currentMinimum = Mathf.Min(currentFirst, currentSecond);
            float alternateMinimum = Mathf.Min(alternateFirst, alternateSecond);
            if (alternateMinimum > currentMinimum + 0.000001f)
            {
                triangles.Add(a);
                triangles.Add(d);
                triangles.Add(b);
                triangles.Add(d);
                triangles.Add(c);
                triangles.Add(b);
                alternateQuadDiagonalCount++;
                return;
            }

            // Default/tie path preserves the established outward winding.
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
        }

        private static float EvaluateTriangleOrientation(
            int a,
            int b,
            int c,
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals)
        {
            return EvaluateTriangleOrientation(
                vertices[a],
                vertices[b],
                vertices[c],
                normals[a],
                normals[b],
                normals[c]);
        }

        private static float EvaluateTriangleOrientation(
            Vector3 aPosition,
            Vector3 bPosition,
            Vector3 cPosition,
            Vector3 aNormal,
            Vector3 bNormal,
            Vector3 cNormal)
        {
            Vector3 faceNormal = Vector3.Cross(
                bPosition - aPosition,
                cPosition - aPosition);
            Vector3 expected = aNormal + bNormal + cNormal;
            if (faceNormal.sqrMagnitude <= Epsilon ||
                expected.sqrMagnitude <= Epsilon)
            {
                return float.NegativeInfinity;
            }

            return Vector3.Dot(
                faceNormal.normalized,
                expected.normalized);
        }

        private static List<RenderSample> BuildRenderSamples(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            IReadOnlyList<TreeCurveSample> sourceSamples,
            TreeBarkMeshSettings settings,
            ref int curvatureRadiusClampCount)
        {
            var samples = new List<RenderSample>(sourceSamples.Count);
            ParentFrame parentFrame = default;
            bool hasParent = branch.ParentBranchIndex >= 0 &&
                branch.ParentBranchIndex < definition.Branches.Count;
            if (hasParent)
            {
                parentFrame = EvaluateFrame(
                    definition.Branches[branch.ParentBranchIndex],
                    branch.ParentAttachmentDistance);
            }

            int transitionCount = 0;
            if (hasParent)
            {
                int maximumTransitionCount = Mathf.Min(
                    sourceSamples.Count,
                    settings.BranchRootTransitionRingCount);
                float targetBlendLength =
                    Mathf.Max(settings.MinimumRenderedRadius, sourceSamples[0].Radius) *
                    settings.BranchRootBlendLengthInChildRadii;
                float travelled = 0f;
                transitionCount = 1;
                for (int index = 1;
                     index < maximumTransitionCount;
                     index++)
                {
                    travelled += Vector3.Distance(
                        sourceSamples[index - 1].Position,
                        sourceSamples[index].Position);
                    transitionCount = index + 1;
                    if (travelled >= targetBlendLength && transitionCount >= 2)
                    {
                        break;
                    }
                }

                transitionCount = Mathf.Max(2, transitionCount);
            }
            for (int index = 0; index < sourceSamples.Count; index++)
            {
                TreeCurveSample source = sourceSamples[index];
                Vector3 position = source.Position;
                float radius = Mathf.Max(
                    settings.MinimumRenderedRadius,
                    source.Radius);
                if (hasParent && index < transitionCount)
                {
                    float transition = transitionCount <= 1
                        ? 1f
                        : index / (float)(transitionCount - 1);
                    float rootScale = Mathf.Lerp(
                        settings.BranchRootRadiusScale,
                        1f,
                        transition);
                    float collar = 1f +
                        settings.BranchRootCollarStrength *
                        Mathf.Sin(Mathf.PI * transition) * 0.22f;
                    radius *= rootScale * collar;
                    if (index == 0)
                    {
                        radius = Mathf.Min(
                            radius,
                            Mathf.Max(
                                settings.MinimumRenderedRadius,
                                parentFrame.Radius * 0.9f));
                    }

                    float inward =
                        (1f - transition) *
                        settings.BranchRootInsetRatio *
                        parentFrame.Radius;
                    position -= branch.LocalReferenceAxis * inward;

                    if (index == 0)
                    {
                        Vector3 delta = position - parentFrame.Position;
                        float axial = Vector3.Dot(delta, parentFrame.Tangent);
                        Vector3 radial = delta - parentFrame.Tangent * axial;
                        float maximumRadial = Mathf.Max(
                            0f,
                            parentFrame.Radius - radius * 1.04f);
                        if (radial.magnitude > maximumRadial &&
                            radial.sqrMagnitude > Epsilon)
                        {
                            position -= radial.normalized *
                                (radial.magnitude - maximumRadial);
                        }
                    }
                }

                samples.Add(new RenderSample
                {
                    Position = position,
                    Tangent = source.Tangent,
                    Normal = source.Normal,
                    Binormal = source.Binormal,
                    Radius = radius,
                    NormalizedDistance = source.NormalizedDistance
                });
            }

            RebuildTransportedFrames(samples);
            ApplyCurvatureRadiusSafety(
                definition,
                branch,
                samples,
                settings.MinimumRenderedRadius,
                ref curvatureRadiusClampCount);
            return samples;
        }

        private static void RebuildTransportedFrames(
            List<RenderSample> samples)
        {
            if (samples == null || samples.Count < 2)
            {
                return;
            }

            Vector3 previousNormal = Vector3.zero;
            Vector3 previousTangent = Vector3.zero;
            for (int index = 0; index < samples.Count; index++)
            {
                RenderSample source = samples[index];
                Vector3 tangent;
                if (index == 0)
                {
                    tangent = samples[1].Position - source.Position;
                }
                else if (index == samples.Count - 1)
                {
                    tangent = source.Position - samples[index - 1].Position;
                }
                else
                {
                    tangent = samples[index + 1].Position -
                        samples[index - 1].Position;
                }

                tangent = SafeNormalize(
                    tangent,
                    source.Tangent.sqrMagnitude > Epsilon
                        ? source.Tangent
                        : Vector3.up);

                // Preserve the source frame's authored/generated roll (surface
                // torsion) while adapting it to the root-adjusted tangent. A
                // pure previous-frame transport would erase torsion from every
                // branch after root transition construction.
                Vector3 normal = Vector3.ProjectOnPlane(
                    source.Normal,
                    tangent);
                if (normal.sqrMagnitude <= Epsilon &&
                    previousNormal.sqrMagnitude > Epsilon)
                {
                    Quaternion transport = Quaternion.FromToRotation(
                        previousTangent,
                        tangent);
                    normal = Vector3.ProjectOnPlane(
                        transport * previousNormal,
                        tangent);
                }

                if (normal.sqrMagnitude <= Epsilon)
                {
                    normal = ChooseInitialNormal(tangent);
                }
                else
                {
                    normal.Normalize();
                }

                if (previousNormal.sqrMagnitude > Epsilon &&
                    Vector3.Dot(previousNormal, normal) < 0f)
                {
                    normal = -normal;
                }

                Vector3 binormal = Vector3.Cross(tangent, normal);
                if (binormal.sqrMagnitude <= Epsilon)
                {
                    normal = ChooseInitialNormal(tangent);
                    binormal = Vector3.Cross(tangent, normal);
                }

                binormal.Normalize();
                normal = Vector3.Cross(binormal, tangent).normalized;
                source.Tangent = tangent;
                source.Normal = normal;
                source.Binormal = binormal;
                samples[index] = source;
                previousTangent = tangent;
                previousNormal = normal;
            }
        }

        private static Vector3 ChooseInitialNormal(Vector3 tangent)
        {
            Vector3 candidate = Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) < 0.92f
                ? Vector3.up
                : Vector3.right;
            candidate = Vector3.ProjectOnPlane(candidate, tangent);
            return SafeNormalize(candidate, Vector3.forward);
        }

        private static void AppendCap(
            IReadOnlyList<RenderSample> samples,
            int sampleIndex,
            int radialSegments,
            TreeDefinition definition,
            TreeBranchDefinition branch,
            bool isTip,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color32> colours,
            List<Vector2> uv0,
            List<int> triangles,
            List<TreeBarkMeshCapAuditRecord> capAuditRecords)
        {
            RenderSample sample = samples[sampleIndex];
            Vector3 capAxis = sample.Tangent;
            Vector3 capTangentAxis = sample.Normal;
            if (branch.BranchOrder == 0)
            {
                ResolveTrunkSurfaceFrame(
                    definition.ResolvedParameters,
                    sample,
                    out capAxis,
                    out capTangentAxis,
                    out _);
            }

            Vector3 capNormal = isTip
                ? SafeNormalize(capAxis, Vector3.up)
                : -SafeNormalize(capAxis, Vector3.up);
            Vector3 capTangent = SafeNormalize(
                capTangentAxis,
                Vector3.right);
            float tangentHandedness = isTip ? 1f : -1f;
            Color32 metadata = BuildVertexMetadata(
                definition,
                branch,
                sample.Position);
            int centreIndex = vertices.Count;
            vertices.Add(sample.Position);
            normals.Add(capNormal);
            tangents.Add(new Vector4(
                capTangent.x,
                capTangent.y,
                capTangent.z,
                tangentHandedness));
            colours.Add(metadata);
            uv0.Add(new Vector2(0.5f, 0.5f));

            int capRingStart = vertices.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                float normalizedSide = side / (float)radialSegments;
                float angle = normalizedSide * TwoPi;
                float cosine = Mathf.Cos(angle);
                float sine = Mathf.Sin(angle);
                Vector3 position;
                if (branch.BranchOrder == 0)
                {
                    position = EvaluateTrunkSurfacePosition(
                        definition,
                        branch,
                        sample,
                        normalizedSide,
                        out _);
                }
                else
                {
                    Vector3 radial = SafeNormalize(
                        sample.Normal * cosine +
                        sample.Binormal * sine,
                        sample.Normal);
                    position = sample.Position +
                        radial * sample.Radius;
                }

                vertices.Add(position);
                normals.Add(capNormal);
                tangents.Add(new Vector4(
                    capTangent.x,
                    capTangent.y,
                    capTangent.z,
                    tangentHandedness));
                colours.Add(metadata);
                uv0.Add(new Vector2(
                    0.5f + cosine * 0.5f,
                    0.5f + sine * 0.5f));
            }

            int triangleStart = triangles.Count;
            for (int side = 0; side < radialSegments; side++)
            {
                int current = capRingStart + side;
                int next = capRingStart + (side + 1) % radialSegments;
                triangles.Add(centreIndex);
                if (isTip)
                {
                    triangles.Add(current);
                    triangles.Add(next);
                }
                else
                {
                    triangles.Add(next);
                    triangles.Add(current);
                }
            }

            capAuditRecords.Add(new TreeBarkMeshCapAuditRecord
            {
                TriangleStart = triangleStart,
                TriangleCount = triangles.Count - triangleStart,
                ExpectedNormal = capNormal
            });
        }


        private static Color32 BuildVertexMetadata(
            TreeDefinition definition,
            TreeBranchDefinition branch,
            Vector3 position)
        {
            Bounds bounds = definition.LocalBounds;
            float height = Mathf.Max(0.0001f, bounds.size.y);
            float normalizedHeight = Mathf.Clamp01(
                (position.y - bounds.min.y) / height);
            float windMask = normalizedHeight * normalizedHeight *
                (3f - 2f * normalizedHeight);
            byte red = ToByte(windMask);
            byte green = ToByte(Mathf.Clamp01(branch.BranchOrder / 3f));
            byte blue = ToByte(Mathf.Clamp01(branch.Stiffness));
            byte alpha = ToByte(Mathf.Repeat(branch.Phase, 1f));
            return new Color32(red, green, blue, alpha);
        }

        private static ParentFrame EvaluateFrame(
            TreeBranchDefinition branch,
            float normalizedDistance)
        {
            IReadOnlyList<TreeCurveSample> samples = branch.Samples;
            float scaled = Mathf.Clamp01(normalizedDistance) * (samples.Count - 1);
            int lower = Mathf.Clamp(Mathf.FloorToInt(scaled), 0, samples.Count - 1);
            int upper = Mathf.Min(samples.Count - 1, lower + 1);
            float t = scaled - lower;
            TreeCurveSample a = samples[lower];
            TreeCurveSample b = samples[upper];
            Vector3 tangent = Vector3.Slerp(a.Tangent, b.Tangent, t).normalized;
            Vector3 normal = Vector3.Slerp(a.Normal, b.Normal, t);
            normal = Vector3.ProjectOnPlane(normal, tangent).normalized;
            Vector3 binormal = Vector3.Cross(tangent, normal).normalized;
            return new ParentFrame
            {
                Position = Vector3.Lerp(a.Position, b.Position, t),
                Tangent = tangent,
                Normal = normal,
                Binormal = binormal,
                Radius = Mathf.Lerp(a.Radius, b.Radius, t)
            };
        }

        private static byte ToByte(float value)
        {
            return (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);
        }

        private static bool IsUsableSample(RenderSample sample)
        {
            return IsFinite(sample.Position) &&
                IsFinite(sample.Tangent) &&
                IsFinite(sample.Normal) &&
                IsFinite(sample.Binormal) &&
                sample.Tangent.sqrMagnitude > Epsilon &&
                sample.Normal.sqrMagnitude > Epsilon &&
                sample.Binormal.sqrMagnitude > Epsilon &&
                TreeDeterministicUtility.IsFinite(sample.Radius) &&
                sample.Radius > 0f;
        }

        private static bool IsFinite(Vector3 value)
        {
            return TreeDeterministicUtility.IsFinite(value.x) &&
                TreeDeterministicUtility.IsFinite(value.y) &&
                TreeDeterministicUtility.IsFinite(value.z);
        }

        private static Vector3 SafeNormalize(
            Vector3 value,
            Vector3 fallback)
        {
            return value.sqrMagnitude > Epsilon
                ? value.normalized
                : fallback.normalized;
        }

        public static string CalculateInputFingerprint(
            TreeDefinition definition,
            TreeBarkMeshSettings settings)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(
                ref hash,
                definition.StructuralFingerprint);
            TreeDeterministicUtility.Append(ref hash, settings.SettingsVersion);
            TreeDeterministicUtility.Append(ref hash, settings.TrunkRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.PrimaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.SecondaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.TertiaryRadialSegments);
            TreeDeterministicUtility.Append(ref hash, settings.BarkMetersPerTile);
            TreeDeterministicUtility.Append(ref hash, settings.MinimumRenderedRadius);
            TreeDeterministicUtility.Append(ref hash, settings.CapTrunkBase);
            TreeDeterministicUtility.Append(ref hash, settings.CapBranchTips);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootInsetRatio);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootBlendLengthInChildRadii);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootRadiusScale);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootCollarStrength);
            TreeDeterministicUtility.Append(ref hash, settings.BranchRootTransitionRingCount);
            TreeResolvedParameters parameters = definition.ResolvedParameters;
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.TrunkSurfaceTorsionDegrees);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.TrunkTwistRidgeCount);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.TrunkTwistRidgeDepth);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootButtressStrength);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootButtressHeight);
            TreeDeterministicUtility.Append(
                ref hash,
                parameters.RootFlareScale);
            return TreeDeterministicUtility.FormatHash(hash);
        }

        private static string BuildGeometryFingerprint(
            IReadOnlyList<Vector3> vertices,
            IReadOnlyList<Vector3> normals,
            IReadOnlyList<Vector4> tangents,
            IReadOnlyList<Color32> colours,
            IReadOnlyList<Vector2> uv0,
            IReadOnlyList<int> triangles)
        {
            ulong hash = TreeDeterministicUtility.BeginHash();
            TreeDeterministicUtility.Append(ref hash, vertices.Count);
            for (int index = 0; index < vertices.Count; index++)
            {
                TreeDeterministicUtility.Append(ref hash, vertices[index]);
                TreeDeterministicUtility.Append(ref hash, normals[index]);
                TreeDeterministicUtility.Append(ref hash, tangents[index].x);
                TreeDeterministicUtility.Append(ref hash, tangents[index].y);
                TreeDeterministicUtility.Append(ref hash, tangents[index].z);
                TreeDeterministicUtility.Append(ref hash, tangents[index].w);
                Color32 colour = colours[index];
                TreeDeterministicUtility.Append(ref hash, colour.r);
                TreeDeterministicUtility.Append(ref hash, colour.g);
                TreeDeterministicUtility.Append(ref hash, colour.b);
                TreeDeterministicUtility.Append(ref hash, colour.a);
                TreeDeterministicUtility.Append(ref hash, uv0[index].x);
                TreeDeterministicUtility.Append(ref hash, uv0[index].y);
            }

            TreeDeterministicUtility.Append(ref hash, triangles.Count);
            for (int index = 0; index < triangles.Count; index++)
            {
                TreeDeterministicUtility.Append(ref hash, triangles[index]);
            }

            return TreeDeterministicUtility.FormatHash(hash);
        }
    }
}
