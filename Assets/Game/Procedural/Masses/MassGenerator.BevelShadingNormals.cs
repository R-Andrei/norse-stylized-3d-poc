using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        private const float ConvexBevelNormalPositionQuantization = 100000f;
        private const float ConvexBevelMinimumRenderNormalDot = 0.5f;
        private const float ConvexBevelNormalAgreementDot = 0.9999f;
        private const int ConvexBevelSafetyBlendIterations = 16;

        private readonly struct ConvexBevelNormalPositionKey :
            IEquatable<ConvexBevelNormalPositionKey>
        {
            private readonly int provenanceKind;
            private readonly int provenanceIndex;
            private readonly Vector3Int position;

            public ConvexBevelNormalPositionKey(
                PolygonFaceProvenanceKind provenanceKind,
                int provenanceIndex,
                Vector3Int position)
            {
                this.provenanceKind = (int)provenanceKind;
                this.provenanceIndex = provenanceIndex;
                this.position = position;
            }

            public bool Equals(ConvexBevelNormalPositionKey other)
            {
                return provenanceKind == other.provenanceKind &&
                    provenanceIndex == other.provenanceIndex &&
                    position == other.position;
            }

            public override bool Equals(object obj)
            {
                return obj is ConvexBevelNormalPositionKey other &&
                    Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = provenanceKind;
                    hash = (hash * 397) ^ provenanceIndex;
                    return (hash * 397) ^ position.GetHashCode();
                }
            }
        }

        private readonly struct ConvexBevelSourceNormalSample
        {
            public readonly int SurfaceGroup;
            public readonly Vector3 Normal;

            public ConvexBevelSourceNormalSample(
                int surfaceGroup,
                Vector3 normal)
            {
                SurfaceGroup = surfaceGroup;
                Normal = normal;
            }
        }

        private sealed class ConvexBevelNormalPositionGroup
        {
            public readonly Vector3Int Position;
            public readonly List<int> VertexIndices = new List<int>(2);
            public readonly List<Vector3> IncidentGeometricNormals =
                new List<Vector3>(2);

            public ConvexBevelNormalPositionGroup(Vector3Int position)
            {
                Position = position;
            }
        }

        // GM-SURFACE.6B: preserve the authored/flat normal through all existing
        // material-mask and face-tone compilation, then replace only ordinary
        // convex bevel rail normals. Production uses EdgeBevelPlane provenance;
        // bounded preview paths use BoundedEdgeBevel. Source-face boundary
        // positions are the authoritative ownership seam already used by
        // material-mask inheritance; no topology state or runtime channel is
        // carried beyond generation.
        private static void CompileGeneratedMassConvexBevelShadingNormals(
            TriangleSoup soup,
            MeshData meshData)
        {
            if (soup == null || meshData == null ||
                meshData.Vertices.Count == 0 ||
                meshData.Normals.Count != meshData.Vertices.Count ||
                meshData.Triangles.Count == 0)
            {
                return;
            }

            // Source-face vertices already contain the final transformed authored
            // normals resolved by BuildMeshData. Reuse that final render stream as
            // the parent truth instead of running the authored-normal solver twice.
            Dictionary<Vector3Int, List<ConvexBevelSourceNormalSample>>
                sourceNormalsByPosition =
                    BuildConvexBevelSourceNormalsByPosition(soup, meshData);
            if (sourceNormalsByPosition.Count == 0)
            {
                return;
            }

            Dictionary<ConvexBevelNormalPositionKey,
                ConvexBevelNormalPositionGroup> bevelGroups =
                    BuildConvexBevelNormalPositionGroups(soup, meshData);
            if (bevelGroups.Count == 0)
            {
                return;
            }

            foreach (KeyValuePair<ConvexBevelNormalPositionKey,
                     ConvexBevelNormalPositionGroup> pair in bevelGroups)
            {
                ConvexBevelNormalPositionGroup group = pair.Value;
                if (group.VertexIndices.Count == 0 ||
                    group.IncidentGeometricNormals.Count == 0 ||
                    !sourceNormalsByPosition.TryGetValue(
                        group.Position,
                        out List<ConvexBevelSourceNormalSample> sourceSamples) ||
                    !TryResolveConvexBevelSourceNormal(
                        sourceSamples,
                        out Vector3 targetNormal) ||
                    !TryResolveConvexBevelBaseNormal(
                        meshData,
                        group.VertexIndices,
                        out Vector3 baseNormal))
                {
                    continue;
                }

                if (!IsConvexBevelRenderNormalSafe(
                        baseNormal,
                        group.IncidentGeometricNormals))
                {
                    // Existing authored normals are validated later by the normal
                    // stream contract. Do not invent a new recovery path here.
                    continue;
                }

                Vector3 resolvedNormal = ResolveSafeConvexBevelRenderNormal(
                    baseNormal,
                    targetNormal,
                    group.IncidentGeometricNormals);
                if (!TryNormalizeMassVector(
                        resolvedNormal,
                        out resolvedNormal))
                {
                    continue;
                }

                for (int vertex = 0;
                     vertex < group.VertexIndices.Count;
                     vertex++)
                {
                    meshData.Normals[group.VertexIndices[vertex]] =
                        resolvedNormal;
                }
            }
        }

        private static Dictionary<Vector3Int,
            List<ConvexBevelSourceNormalSample>>
            BuildConvexBevelSourceNormalsByPosition(
                TriangleSoup soup,
                MeshData meshData)
        {
            var result = new Dictionary<Vector3Int,
                List<ConvexBevelSourceNormalSample>>();
            int triangleCount = meshData.Triangles.Count / 3;
            for (int triangleIndex = 0;
                 triangleIndex < triangleCount;
                 triangleIndex++)
            {
                int soupIndex = triangleIndex * 3;
                if (!soup.TryResolveProvenance(
                        soupIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out _) ||
                    provenanceKind != PolygonFaceProvenanceKind.SourceFace ||
                    !soup.TryResolveAuthoredSurfaceGroup(
                        soupIndex,
                        out int surfaceGroup))
                {
                    continue;
                }

                for (int corner = 0; corner < 3; corner++)
                {
                    int vertexIndex =
                        meshData.Triangles[soupIndex + corner];
                    if (vertexIndex < 0 ||
                        vertexIndex >= meshData.Vertices.Count ||
                        vertexIndex >= meshData.Normals.Count ||
                        !TryNormalizeMassVector(
                            meshData.Normals[vertexIndex],
                            out Vector3 transformedNormal))
                    {
                        continue;
                    }

                    Vector3Int position = BuildGeneratedMassMaskPositionKey(
                        meshData.Vertices[vertexIndex],
                        ConvexBevelNormalPositionQuantization);
                    if (!result.TryGetValue(
                            position,
                            out List<ConvexBevelSourceNormalSample> samples))
                    {
                        samples = new List<ConvexBevelSourceNormalSample>(2);
                        result.Add(position, samples);
                    }

                    bool surfaceAlreadyPresent = false;
                    for (int sampleIndex = 0;
                         sampleIndex < samples.Count;
                         sampleIndex++)
                    {
                        if (samples[sampleIndex].SurfaceGroup == surfaceGroup)
                        {
                            surfaceAlreadyPresent = true;
                            break;
                        }
                    }
                    if (!surfaceAlreadyPresent)
                    {
                        samples.Add(new ConvexBevelSourceNormalSample(
                            surfaceGroup,
                            transformedNormal));
                    }
                }
            }
            return result;
        }

        private static Dictionary<ConvexBevelNormalPositionKey,
            ConvexBevelNormalPositionGroup>
            BuildConvexBevelNormalPositionGroups(
                TriangleSoup soup,
                MeshData meshData)
        {
            var result = new Dictionary<ConvexBevelNormalPositionKey,
                ConvexBevelNormalPositionGroup>();
            int triangleCount = meshData.Triangles.Count / 3;
            for (int triangleIndex = 0;
                 triangleIndex < triangleCount;
                 triangleIndex++)
            {
                int soupIndex = triangleIndex * 3;
                if (!soup.TryResolveProvenance(
                        soupIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out int provenanceIndex) ||
                    !IsOrdinaryBevelProvenance(provenanceKind) ||
                    provenanceIndex < 0)
                {
                    continue;
                }

                int ia = meshData.Triangles[soupIndex];
                int ib = meshData.Triangles[soupIndex + 1];
                int ic = meshData.Triangles[soupIndex + 2];
                if (ia < 0 || ia >= meshData.Vertices.Count ||
                    ib < 0 || ib >= meshData.Vertices.Count ||
                    ic < 0 || ic >= meshData.Vertices.Count)
                {
                    continue;
                }

                Vector3 geometricNormal = Vector3.Cross(
                    meshData.Vertices[ib] - meshData.Vertices[ia],
                    meshData.Vertices[ic] - meshData.Vertices[ia]);
                if (!TryNormalizeMassVector(
                        geometricNormal,
                        out geometricNormal))
                {
                    continue;
                }

                for (int corner = 0; corner < 3; corner++)
                {
                    int vertexIndex =
                        meshData.Triangles[soupIndex + corner];
                    Vector3Int position = BuildGeneratedMassMaskPositionKey(
                        meshData.Vertices[vertexIndex],
                        ConvexBevelNormalPositionQuantization);
                    ConvexBevelNormalPositionKey key =
                        new ConvexBevelNormalPositionKey(
                            provenanceKind,
                            provenanceIndex,
                            position);
                    if (!result.TryGetValue(
                            key,
                            out ConvexBevelNormalPositionGroup group))
                    {
                        group = new ConvexBevelNormalPositionGroup(position);
                        result.Add(key, group);
                    }

                    if (!group.VertexIndices.Contains(vertexIndex))
                    {
                        group.VertexIndices.Add(vertexIndex);
                    }
                    AddUniqueConvexBevelIncidentNormal(
                        group.IncidentGeometricNormals,
                        geometricNormal);
                }
            }
            return result;
        }

        private static void AddUniqueConvexBevelIncidentNormal(
            List<Vector3> normals,
            Vector3 normal)
        {
            for (int i = 0; i < normals.Count; i++)
            {
                if (Vector3.Dot(normals[i], normal) >=
                    ConvexBevelNormalAgreementDot)
                {
                    return;
                }
            }
            normals.Add(normal);
        }

        private static bool TryResolveConvexBevelSourceNormal(
            List<ConvexBevelSourceNormalSample> samples,
            out Vector3 normal)
        {
            normal = Vector3.zero;
            if (samples == null || samples.Count == 0)
            {
                return false;
            }

            Vector3 sum = Vector3.zero;
            Vector3 reference = Vector3.zero;
            bool hasReference = false;
            for (int i = 0; i < samples.Count; i++)
            {
                if (!TryNormalizeMassVector(
                        samples[i].Normal,
                        out Vector3 candidate))
                {
                    return false;
                }
                if (!hasReference)
                {
                    reference = candidate;
                    hasReference = true;
                }
                else if (Vector3.Dot(reference, candidate) <
                         ConvexBevelNormalAgreementDot)
                {
                    // A final position belonging to more than one materially
                    // different source surface is a junction, not an ordinary
                    // two-face rail ownership case. Preserve authored shading.
                    return false;
                }
                sum += candidate;
            }
            return TryNormalizeMassVector(sum, out normal);
        }

        private static bool TryResolveConvexBevelBaseNormal(
            MeshData meshData,
            List<int> vertexIndices,
            out Vector3 normal)
        {
            normal = Vector3.zero;
            if (meshData == null || vertexIndices == null ||
                vertexIndices.Count == 0)
            {
                return false;
            }

            Vector3 sum = Vector3.zero;
            Vector3 reference = Vector3.zero;
            bool hasReference = false;
            for (int i = 0; i < vertexIndices.Count; i++)
            {
                int vertexIndex = vertexIndices[i];
                if (vertexIndex < 0 ||
                    vertexIndex >= meshData.Normals.Count ||
                    !TryNormalizeMassVector(
                        meshData.Normals[vertexIndex],
                        out Vector3 candidate))
                {
                    return false;
                }
                if (!hasReference)
                {
                    reference = candidate;
                    hasReference = true;
                }
                else if (Vector3.Dot(reference, candidate) <
                         ConvexBevelNormalAgreementDot)
                {
                    // Never replace disagreeing triangle-local authored normals
                    // with an arbitrary common value; that would hide topology.
                    return false;
                }
                sum += candidate;
            }
            return TryNormalizeMassVector(sum, out normal);
        }

        private static Vector3 ResolveSafeConvexBevelRenderNormal(
            Vector3 baseNormal,
            Vector3 targetNormal,
            List<Vector3> incidentGeometricNormals)
        {
            if (IsConvexBevelRenderNormalSafe(
                    targetNormal,
                    incidentGeometricNormals))
            {
                return targetNormal;
            }

            float safe = 0f;
            float unsafeValue = 1f;
            Vector3 safeNormal = baseNormal;
            for (int iteration = 0;
                 iteration < ConvexBevelSafetyBlendIterations;
                 iteration++)
            {
                float t = (safe + unsafeValue) * 0.5f;
                Vector3 candidate = Vector3.Lerp(
                    baseNormal,
                    targetNormal,
                    t);
                if (!TryNormalizeMassVector(candidate, out candidate))
                {
                    unsafeValue = t;
                    continue;
                }

                if (IsConvexBevelRenderNormalSafe(
                        candidate,
                        incidentGeometricNormals))
                {
                    safe = t;
                    safeNormal = candidate;
                }
                else
                {
                    unsafeValue = t;
                }
            }
            return safeNormal;
        }

        private static bool IsConvexBevelRenderNormalSafe(
            Vector3 normal,
            List<Vector3> incidentGeometricNormals)
        {
            if (!TryNormalizeMassVector(normal, out normal) ||
                incidentGeometricNormals == null ||
                incidentGeometricNormals.Count == 0)
            {
                return false;
            }

            for (int i = 0;
                 i < incidentGeometricNormals.Count;
                 i++)
            {
                Vector3 geometricNormal = incidentGeometricNormals[i];
                if (!TryNormalizeMassVector(
                        geometricNormal,
                        out geometricNormal))
                {
                    return false;
                }
                float dot = Vector3.Dot(geometricNormal, normal);
                if (!IsFiniteMassValue(dot) ||
                    dot < ConvexBevelMinimumRenderNormalDot)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
