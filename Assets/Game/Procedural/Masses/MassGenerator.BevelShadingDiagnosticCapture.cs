using System;
using System.Collections.Generic;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        public sealed class BevelShadingDiagnosticSnapshot
        {
            public int RunId;
            public readonly List<BevelShadingDiagnosticBuildRecord> Builds = new();
            public readonly List<string> ContractFailures = new();
            public string GenerationException = string.Empty;
            public int AcceptedBuildId = -1;

            public BevelShadingDiagnosticBuildRecord AcceptedBuild
            {
                get
                {
                    if (AcceptedBuildId < 0) return null;
                    return Builds.Find(x => x.BuildId == AcceptedBuildId);
                }
            }
        }

        public sealed class BevelShadingDiagnosticBuildRecord
        {
            public int RunId;
            public int BuildId;
            public string Purpose = "MeshDataBuild";
            public bool Completed;
            public bool Succeeded;
            public bool AcceptedForUpload;
            public string UploadedMeshName = string.Empty;
            public MeshData MeshData;
            public readonly Dictionary<int, LogicalBevelRecord> LogicalBevels = new();
            public readonly List<FinalTriangleRecord> FinalTriangles = new();
            public ulong PreMaskImmutableFingerprint;
            public ulong PostMaskImmutableFingerprint;
            public ulong PreMaskValueFingerprint;
            public ulong PostMaskValueFingerprint;
            public int SourceFaceMaskChangeCount;
            public int ReconciledLogicalBevelPositionGroups;
            public int ReconciledLogicalBevelVertices;
            public int PreMaskDegenerateTriangleCount;
            public int PostMaskDegenerateTriangleCount;
            public ulong PreMaskDegenerateTriangleFingerprint;
            public ulong PostMaskDegenerateTriangleFingerprint;
            internal readonly Dictionary<int, Vector3> PreMaskSourceFaceMasks = new();
        }

        public sealed class LogicalBevelRecord
        {
            public int LogicalBevelId;
            public int GraphEdgeIndex;
            public int CandidateIndex;
            public int SourceEdgeIndex = -1;
            public int EmittedProvenanceKind;
            public string EmittedProvenanceKindName = string.Empty;
            public int EmittedProvenanceIndex = -1;
            public int ParentFaceA;
            public int ParentFaceB;
            public Vector3 ParentNormalA;
            public Vector3 ParentNormalB;
            public Vector3 SourceA;
            public Vector3 SourceB;
            public float Strength;
        }

        public sealed class FinalTriangleRecord
        {
            public int TriangleIndex;
            public int LogicalBevelId = -1;
            public int ProvenanceKind;
            public string ProvenanceKindName = string.Empty;
            public int ProvenanceIndex;
            public bool IsOrdinaryBevel;
            public int SurfaceGroup;
            public Vector3 A;
            public Vector3 B;
            public Vector3 C;
            public Vector3 GeometricNormal;
            public Vector3 RenderNormal;
            public Vector3 AuthoredNormal;
            public Vector4 MaskA;
            public Vector4 MaskB;
            public Vector4 MaskC;
            public Vector4 StructuralA;
            public Vector4 StructuralB;
            public Vector4 StructuralC;
        }

        private static BevelShadingDiagnosticSnapshot activeBevelShadingCapture;
        private static readonly Dictionary<int, LogicalBevelRecord> pendingBevelShadingLogicalRecords = new();
        private static BevelShadingDiagnosticBuildRecord activeBevelShadingBuild;
        private static int nextBevelShadingRunId;
        private static int nextBevelShadingBuildId;

        public static void BeginBevelShadingDiagnosticCapture()
        {
            activeBevelShadingCapture = new BevelShadingDiagnosticSnapshot
            {
                RunId = ++nextBevelShadingRunId
            };
            pendingBevelShadingLogicalRecords.Clear();
            activeBevelShadingBuild = null;
        }

        public static BevelShadingDiagnosticSnapshot EndBevelShadingDiagnosticCapture()
        {
            BevelShadingDiagnosticSnapshot result = activeBevelShadingCapture;
            if (result != null && activeBevelShadingBuild != null)
            {
                result.ContractFailures.Add(
                    "capture ended while build " + activeBevelShadingBuild.BuildId + " remained active");
            }
            activeBevelShadingCapture = null;
            activeBevelShadingBuild = null;
            pendingBevelShadingLogicalRecords.Clear();
            return result;
        }

        public static void MarkBevelShadingDiagnosticAcceptedMesh(
            MeshData meshData,
            string uploadedMeshName)
        {
            if (activeBevelShadingCapture == null || meshData == null) return;

            BevelShadingDiagnosticBuildRecord match = null;
            for (int i = 0; i < activeBevelShadingCapture.Builds.Count; i++)
            {
                BevelShadingDiagnosticBuildRecord build = activeBevelShadingCapture.Builds[i];
                if (!ReferenceEquals(build.MeshData, meshData)) continue;
                if (match != null)
                {
                    activeBevelShadingCapture.ContractFailures.Add(
                        "multiple completed builds reference the accepted MeshData");
                    return;
                }
                match = build;
            }

            if (match == null)
            {
                activeBevelShadingCapture.ContractFailures.Add(
                    "uploaded MeshData did not match a completed diagnostic build");
                return;
            }

            if (activeBevelShadingCapture.AcceptedBuildId >= 0 &&
                activeBevelShadingCapture.AcceptedBuildId != match.BuildId)
            {
                activeBevelShadingCapture.ContractFailures.Add(
                    "more than one diagnostic build was accepted for upload");
                return;
            }

            match.AcceptedForUpload = true;
            match.UploadedMeshName = uploadedMeshName ?? string.Empty;
            activeBevelShadingCapture.AcceptedBuildId = match.BuildId;
        }

        private static void BeginBevelShadingDiagnosticMeshBuild()
        {
            if (activeBevelShadingCapture == null) return;
            if (activeBevelShadingBuild != null)
            {
                activeBevelShadingCapture.ContractFailures.Add(
                    "nested MeshData diagnostic builds are unsupported");
                return;
            }

            activeBevelShadingBuild = new BevelShadingDiagnosticBuildRecord
            {
                RunId = activeBevelShadingCapture.RunId,
                BuildId = ++nextBevelShadingBuildId
            };
            foreach (KeyValuePair<int, LogicalBevelRecord> pair in pendingBevelShadingLogicalRecords)
            {
                activeBevelShadingBuild.LogicalBevels.Add(pair.Key, pair.Value);
            }
            activeBevelShadingCapture.Builds.Add(activeBevelShadingBuild);
        }

        private static void CompleteBevelShadingDiagnosticMeshBuild(
            MeshData meshData,
            bool succeeded)
        {
            if (activeBevelShadingCapture == null) return;
            if (activeBevelShadingBuild == null)
            {
                activeBevelShadingCapture.ContractFailures.Add(
                    "MeshData build completed without a diagnostic build scope");
                return;
            }

            activeBevelShadingBuild.MeshData = meshData;
            activeBevelShadingBuild.Succeeded = succeeded;
            activeBevelShadingBuild.Completed = true;
            HashSet<int> emittedLogicalBevelIds = new HashSet<int>();
            for (int triangleIndex = 0;
                 triangleIndex < activeBevelShadingBuild.FinalTriangles.Count;
                 triangleIndex++)
            {
                FinalTriangleRecord triangle =
                    activeBevelShadingBuild.FinalTriangles[triangleIndex];
                if (triangle.IsOrdinaryBevel && triangle.ProvenanceIndex >= 0)
                {
                    emittedLogicalBevelIds.Add(triangle.ProvenanceIndex);
                }
            }
            List<int> nonEmittedLogicalBevelIds = new List<int>();
            foreach (int logicalBevelId in activeBevelShadingBuild.LogicalBevels.Keys)
            {
                if (!emittedLogicalBevelIds.Contains(logicalBevelId))
                {
                    nonEmittedLogicalBevelIds.Add(logicalBevelId);
                }
            }
            for (int logicalIndex = 0;
                 logicalIndex < nonEmittedLogicalBevelIds.Count;
                 logicalIndex++)
            {
                activeBevelShadingBuild.LogicalBevels.Remove(
                    nonEmittedLogicalBevelIds[logicalIndex]);
            }
            activeBevelShadingBuild = null;
        }


        private static void BeginBevelShadingMaskCompilationDiagnostics(
            TriangleSoup soup,
            MeshData meshData)
        {
            if (activeBevelShadingBuild == null || meshData == null) return;
            BevelShadingDiagnosticBuildRecord build = activeBevelShadingBuild;
            build.PreMaskImmutableFingerprint =
                BuildBevelShadingImmutableFingerprint(soup, meshData);
            build.PreMaskValueFingerprint =
                BuildBevelShadingMaskValueFingerprint(meshData);
            build.PreMaskSourceFaceMasks.Clear();
            int triangleCount = meshData.Triangles.Count / 3;
            for (int triangleIndex = 0;
                 triangleIndex < triangleCount;
                 triangleIndex++)
            {
                int soupIndex = triangleIndex * 3;
                if (soup == null ||
                    !soup.TryResolveProvenance(
                        soupIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out _) ||
                    provenanceKind != PolygonFaceProvenanceKind.SourceFace)
                {
                    continue;
                }
                for (int corner = 0; corner < 3; corner++)
                {
                    int vertexIndex = meshData.Triangles[soupIndex + corner];
                    build.PreMaskSourceFaceMasks[vertexIndex] = new Vector3(
                        meshData.Colors[vertexIndex].g,
                        meshData.Colors[vertexIndex].b,
                        meshData.UV2[vertexIndex].y);
                }
            }
            BuildBevelShadingDegenerateTriangleEvidence(
                meshData,
                out build.PreMaskDegenerateTriangleCount,
                out build.PreMaskDegenerateTriangleFingerprint);
        }

        private static void CompleteBevelShadingMaskCompilationDiagnostics(
            TriangleSoup soup,
            MeshData meshData,
            int reconciledPositionGroups,
            int reconciledVertices)
        {
            if (activeBevelShadingBuild == null || meshData == null) return;
            BevelShadingDiagnosticBuildRecord build = activeBevelShadingBuild;
            build.PostMaskImmutableFingerprint =
                BuildBevelShadingImmutableFingerprint(soup, meshData);
            build.PostMaskValueFingerprint =
                BuildBevelShadingMaskValueFingerprint(meshData);
            build.ReconciledLogicalBevelPositionGroups =
                reconciledPositionGroups;
            build.ReconciledLogicalBevelVertices = reconciledVertices;
            build.SourceFaceMaskChangeCount = 0;
            foreach (KeyValuePair<int, Vector3> pair in
                     build.PreMaskSourceFaceMasks)
            {
                int vertexIndex = pair.Key;
                if (vertexIndex < 0 ||
                    vertexIndex >= meshData.Vertices.Count)
                {
                    build.SourceFaceMaskChangeCount++;
                    continue;
                }
                Vector3 after = new Vector3(
                    meshData.Colors[vertexIndex].g,
                    meshData.Colors[vertexIndex].b,
                    meshData.UV2[vertexIndex].y);
                if ((after - pair.Value).sqrMagnitude > 0.0000000001f)
                {
                    build.SourceFaceMaskChangeCount++;
                }
            }
            BuildBevelShadingDegenerateTriangleEvidence(
                meshData,
                out build.PostMaskDegenerateTriangleCount,
                out build.PostMaskDegenerateTriangleFingerprint);
        }

        private static ulong BuildBevelShadingImmutableFingerprint(
            TriangleSoup soup,
            MeshData meshData)
        {
            ulong hash = 1469598103934665603UL;
            void AddInt(int value)
            {
                unchecked
                {
                    hash ^= (uint)value;
                    hash *= 1099511628211UL;
                }
            }
            void AddFloat(float value)
            {
                AddInt(BitConverter.SingleToInt32Bits(value));
            }
            AddInt(meshData.Vertices.Count);
            AddInt(meshData.Triangles.Count);
            for (int i = 0; i < meshData.Vertices.Count; i++)
            {
                Vector3 p = meshData.Vertices[i];
                AddFloat(p.x); AddFloat(p.y); AddFloat(p.z);
                if (i < meshData.Normals.Count)
                {
                    Vector3 n = meshData.Normals[i];
                    AddFloat(n.x); AddFloat(n.y); AddFloat(n.z);
                }
                if (i < meshData.UV0.Count)
                {
                    Vector2 uv = meshData.UV0[i];
                    AddFloat(uv.x); AddFloat(uv.y);
                }
                if (i < meshData.SurfaceFeatures.Count)
                {
                    Vector4 f = meshData.SurfaceFeatures[i];
                    AddFloat(f.x); AddFloat(f.y);
                    AddFloat(f.z); AddFloat(f.w);
                }
                if (i < meshData.Colors.Count)
                {
                    Color c = meshData.Colors[i];
                    AddFloat(c.r); AddFloat(c.a);
                }
                if (i < meshData.UV2.Count)
                {
                    Vector4 uv2 = meshData.UV2[i];
                    AddFloat(uv2.x); AddFloat(uv2.z); AddFloat(uv2.w);
                }
            }
            for (int i = 0; i < meshData.Triangles.Count; i++)
            {
                AddInt(meshData.Triangles[i]);
            }
            int triangleCount = meshData.Triangles.Count / 3;
            for (int triangleIndex = 0;
                 triangleIndex < triangleCount;
                 triangleIndex++)
            {
                int soupIndex = triangleIndex * 3;
                if (soup != null && soup.TryResolveProvenance(
                        soupIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out int provenanceIndex))
                {
                    AddInt((int)provenanceKind);
                    AddInt(provenanceIndex);
                }
                else
                {
                    AddInt(-1);
                    AddInt(-1);
                }
                if (soup != null && soup.TryResolveAuthoredSurfaceGroup(
                        soupIndex,
                        out int surfaceGroup))
                {
                    AddInt(surfaceGroup);
                }
                else
                {
                    AddInt(-1);
                }
            }
            return hash;
        }

        private static ulong BuildBevelShadingMaskValueFingerprint(
            MeshData meshData)
        {
            ulong hash = 1469598103934665603UL;
            for (int i = 0; i < meshData.Vertices.Count; i++)
            {
                unchecked
                {
                    hash ^= (uint)BitConverter.SingleToInt32Bits(
                        meshData.Colors[i].g);
                    hash *= 1099511628211UL;
                    hash ^= (uint)BitConverter.SingleToInt32Bits(
                        meshData.Colors[i].b);
                    hash *= 1099511628211UL;
                    hash ^= (uint)BitConverter.SingleToInt32Bits(
                        meshData.UV2[i].y);
                    hash *= 1099511628211UL;
                }
            }
            return hash;
        }

        private static void BuildBevelShadingDegenerateTriangleEvidence(
            MeshData meshData,
            out int count,
            out ulong fingerprint)
        {
            count = 0;
            fingerprint = 1469598103934665603UL;
            for (int offset = 0;
                 offset + 2 < meshData.Triangles.Count;
                 offset += 3)
            {
                int ia = meshData.Triangles[offset];
                int ib = meshData.Triangles[offset + 1];
                int ic = meshData.Triangles[offset + 2];
                Vector3 cross = Vector3.Cross(
                    meshData.Vertices[ib] - meshData.Vertices[ia],
                    meshData.Vertices[ic] - meshData.Vertices[ia]);
                if (cross.sqrMagnitude > 0.000000000001f) continue;
                count++;
                unchecked
                {
                    fingerprint ^= (uint)(offset / 3);
                    fingerprint *= 1099511628211UL;
                    fingerprint ^= (uint)ia;
                    fingerprint *= 1099511628211UL;
                    fingerprint ^= (uint)ib;
                    fingerprint *= 1099511628211UL;
                    fingerprint ^= (uint)ic;
                    fingerprint *= 1099511628211UL;
                }
            }
        }

        private static void CaptureLogicalBevel(
            int graphEdgeIndex,
            int candidateIndex,
            int parentFaceA,
            int parentFaceB,
            Vector3 parentNormalA,
            Vector3 parentNormalB,
            Vector3 sourceA,
            Vector3 sourceB,
            float strength)
        {
            CaptureLogicalBevel(
                graphEdgeIndex,
                candidateIndex,
                graphEdgeIndex,
                PolygonFaceProvenanceKind.BoundedEdgeBevel,
                graphEdgeIndex,
                parentFaceA,
                parentFaceB,
                parentNormalA,
                parentNormalB,
                sourceA,
                sourceB,
                strength);
        }


        private static void CaptureLogicalBevel(
            int graphEdgeIndex,
            int candidateIndex,
            int sourceEdgeIndex,
            PolygonFaceProvenanceKind emittedProvenanceKind,
            int emittedProvenanceIndex,
            int parentFaceA,
            int parentFaceB,
            Vector3 parentNormalA,
            Vector3 parentNormalB,
            Vector3 sourceA,
            Vector3 sourceB,
            float strength)
        {
            if (activeBevelShadingCapture == null ||
                emittedProvenanceIndex < 0)
            {
                return;
            }
            pendingBevelShadingLogicalRecords[emittedProvenanceIndex] =
                new LogicalBevelRecord
                {
                    LogicalBevelId = emittedProvenanceIndex,
                    GraphEdgeIndex = graphEdgeIndex,
                    CandidateIndex = candidateIndex,
                    SourceEdgeIndex = sourceEdgeIndex,
                    EmittedProvenanceKind = (int)emittedProvenanceKind,
                    EmittedProvenanceKindName =
                        emittedProvenanceKind.ToString(),
                    EmittedProvenanceIndex = emittedProvenanceIndex,
                    ParentFaceA = parentFaceA,
                    ParentFaceB = parentFaceB,
                    ParentNormalA = parentNormalA,
                    ParentNormalB = parentNormalB,
                    SourceA = sourceA,
                    SourceB = sourceB,
                    Strength = strength
                };
        }

        private static void CaptureCommittedPlaneCutLogicalBevels(
            TriangleSoup soup,
            ChamferTopologyContext context)
        {
            if (activeBevelShadingCapture == null ||
                soup == null || context == null || context.Graph == null)
            {
                return;
            }

            pendingBevelShadingLogicalRecords.Clear();
            HashSet<int> emittedSourceEdges = new HashSet<int>();
            for (int vertexIndex = 0;
                 vertexIndex + 2 < soup.Positions.Count;
                 vertexIndex += 3)
            {
                if (!soup.TryResolveProvenance(
                        vertexIndex,
                        out PolygonFaceProvenanceKind provenanceKind,
                        out int provenanceIndex) ||
                    provenanceKind !=
                        PolygonFaceProvenanceKind.EdgeBevelPlane ||
                    provenanceIndex < 0)
                {
                    continue;
                }
                emittedSourceEdges.Add(provenanceIndex);
            }

            Dictionary<int, EdgeWearSelectedGraphEdge> selectedByGraphEdge =
                new Dictionary<int, EdgeWearSelectedGraphEdge>();
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                EdgeWearSelectedGraphEdge selected =
                    context.SelectedEdges[selectedIndex];
                selectedByGraphEdge[selected.GraphEdgeIndex] = selected;
            }

            foreach (int sourceEdgeIndex in emittedSourceEdges)
            {
                if (sourceEdgeIndex < 0 ||
                    sourceEdgeIndex >= context.Graph.Edges.Count ||
                    !selectedByGraphEdge.TryGetValue(
                        sourceEdgeIndex,
                        out EdgeWearSelectedGraphEdge selected))
                {
                    activeBevelShadingCapture.ContractFailures.Add(
                        "committed EdgeBevelPlane provenance " +
                        sourceEdgeIndex +
                        " did not resolve to a selected graph edge");
                    continue;
                }

                EdgeWearGraphEdge graphEdge =
                    context.Graph.Edges[sourceEdgeIndex];
                if (graphEdge.FaceA < 0 || graphEdge.FaceB < 0 ||
                    graphEdge.FaceA >= context.Graph.Faces.Count ||
                    graphEdge.FaceB >= context.Graph.Faces.Count ||
                    graphEdge.VertexA < 0 || graphEdge.VertexB < 0 ||
                    graphEdge.VertexA >= context.Graph.Vertices.Count ||
                    graphEdge.VertexB >= context.Graph.Vertices.Count)
                {
                    activeBevelShadingCapture.ContractFailures.Add(
                        "committed EdgeBevelPlane provenance " +
                        sourceEdgeIndex +
                        " has invalid parent topology");
                    continue;
                }

                CaptureLogicalBevel(
                    selected.GraphEdgeIndex,
                    selected.CandidateIndex,
                    sourceEdgeIndex,
                    PolygonFaceProvenanceKind.EdgeBevelPlane,
                    sourceEdgeIndex,
                    graphEdge.FaceA,
                    graphEdge.FaceB,
                    context.Graph.Faces[graphEdge.FaceA]
                        .SourceFace.Normal,
                    context.Graph.Faces[graphEdge.FaceB]
                        .SourceFace.Normal,
                    context.Graph.Vertices[graphEdge.VertexA].Position,
                    context.Graph.Vertices[graphEdge.VertexB].Position,
                    selected.Candidate.Strength);
            }
        }

        private static bool IsOrdinaryBevelProvenance(
            PolygonFaceProvenanceKind provenanceKind)
        {
            return provenanceKind == PolygonFaceProvenanceKind.EdgeBevelPlane ||
                provenanceKind == PolygonFaceProvenanceKind.BoundedEdgeBevel;
        }

        private static void CaptureFinalTriangle(
            int triangleIndex,
            PolygonFaceProvenanceKind provenanceKind,
            int provenanceIndex,
            int surfaceGroup,
            Vector3 a,
            Vector3 b,
            Vector3 c,
            Vector3 geometricNormal,
            Vector3 renderNormal,
            Vector3 authoredNormal,
            Vector4 maskA,
            Vector4 maskB,
            Vector4 maskC,
            Vector4 structuralA,
            Vector4 structuralB,
            Vector4 structuralC)
        {
            if (activeBevelShadingBuild == null) return;
            bool ordinaryBevel = IsOrdinaryBevelProvenance(provenanceKind);
            int logicalBevelId = ordinaryBevel &&
                activeBevelShadingBuild.LogicalBevels.ContainsKey(provenanceIndex)
                    ? provenanceIndex
                    : -1;
            activeBevelShadingBuild.FinalTriangles.Add(new FinalTriangleRecord
            {
                TriangleIndex = triangleIndex,
                LogicalBevelId = logicalBevelId,
                ProvenanceKind = (int)provenanceKind,
                ProvenanceKindName = provenanceKind.ToString(),
                ProvenanceIndex = provenanceIndex,
                IsOrdinaryBevel = ordinaryBevel,
                SurfaceGroup = surfaceGroup,
                A = a, B = b, C = c,
                GeometricNormal = geometricNormal,
                RenderNormal = renderNormal,
                AuthoredNormal = authoredNormal,
                MaskA = maskA, MaskB = maskB, MaskC = maskC,
                StructuralA = structuralA, StructuralB = structuralB, StructuralC = structuralC
            });
        }
    }
}
