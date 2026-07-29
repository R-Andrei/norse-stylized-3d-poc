using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        private sealed class CornerSelectionMeshSnapshot
        {
            public int FaceCount;
            public int VertexCount;
            public int EdgeCount;
            public ulong StructuralFingerprint;
            public Bounds Bounds;
            public readonly Dictionary<SelectionVertexKey, Vector3> Vertices =
                new Dictionary<SelectionVertexKey, Vector3>();
            public readonly HashSet<SelectionEdgeKey> Edges =
                new HashSet<SelectionEdgeKey>();
            public readonly Dictionary<ulong, int> FaceSignatures =
                new Dictionary<ulong, int>();
        }

        private sealed class CornerSelectionMeshDelta
        {
            public bool Captured;
            public bool Valid;
            public bool ContinuityExact;
            public bool ConservativeLocalReplacement;
            public int BeforeVertexCount;
            public int AfterVertexCount;
            public int BeforeEdgeCount;
            public int AfterEdgeCount;
            public int BeforeFaceCount;
            public int AfterFaceCount;
            public int AddedVertexCount;
            public int RemovedVertexCount;
            public int PreservedVertexCount;
            public int AddedEdgeCount;
            public int RemovedEdgeCount;
            public int PreservedEdgeCount;
            public int AddedFaceCount;
            public int RemovedFaceCount;
            public int PreservedFaceCount;
            public int ChangedFeatureCount;
            public int ChangedFeaturesOutsideAffectedBounds;
            public Bounds AffectedBounds;
            public string Diagnostic = string.Empty;
        }

        private sealed class CornerSelectionTransactionContract
        {
            public bool Captured;
            public bool Certified;
            public int RequestedChipCount;
            public int CommittedChipCount;
            public CornerSelectionMeshSnapshot SourceSnapshot;
            public CornerSelectionMeshSnapshot ResultSnapshot;
            public CornerSelectionMeshDelta Delta;
        }

        private readonly struct SelectionVertexKey :
            IEquatable<SelectionVertexKey>,
            IComparable<SelectionVertexKey>
        {
            private const float Quantization = 100000f;

            public readonly int X;
            public readonly int Y;
            public readonly int Z;

            public SelectionVertexKey(Vector3 position)
            {
                X = Mathf.RoundToInt(position.x * Quantization);
                Y = Mathf.RoundToInt(position.y * Quantization);
                Z = Mathf.RoundToInt(position.z * Quantization);
            }

            public int CompareTo(SelectionVertexKey other)
            {
                int x = X.CompareTo(other.X);
                if (x != 0)
                {
                    return x;
                }
                int y = Y.CompareTo(other.Y);
                return y != 0 ? y : Z.CompareTo(other.Z);
            }

            public bool Equals(SelectionVertexKey other)
            {
                return X == other.X && Y == other.Y && Z == other.Z;
            }

            public override bool Equals(object obj)
            {
                return obj is SelectionVertexKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = X;
                    hash = (hash * 397) ^ Y;
                    hash = (hash * 397) ^ Z;
                    return hash;
                }
            }
        }

        private readonly struct SelectionEdgeKey :
            IEquatable<SelectionEdgeKey>
        {
            private readonly SelectionVertexKey first;
            private readonly SelectionVertexKey second;

            public SelectionEdgeKey(Vector3 start, Vector3 end)
            {
                SelectionVertexKey a = new SelectionVertexKey(start);
                SelectionVertexKey b = new SelectionVertexKey(end);
                if (a.CompareTo(b) <= 0)
                {
                    first = a;
                    second = b;
                }
                else
                {
                    first = b;
                    second = a;
                }
            }

            public bool Equals(SelectionEdgeKey other)
            {
                return first.Equals(other.first) && second.Equals(other.second);
            }

            public override bool Equals(object obj)
            {
                return obj is SelectionEdgeKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (first.GetHashCode() * 397) ^ second.GetHashCode();
                }
            }
        }

        private static CornerSelectionTransactionContract
            BuildCornerSelectionTransactionContract(
                IReadOnlyList<PolygonFace> sourceFaces,
                IReadOnlyList<PolygonFace> resultFaces,
                int requestedChipCount,
                int committedChipCount,
                bool certified)
        {
            CornerSelectionTransactionContract contract =
                new CornerSelectionTransactionContract
                {
                    Captured = true,
                    Certified = certified,
                    RequestedChipCount = requestedChipCount,
                    CommittedChipCount = committedChipCount,
                    SourceSnapshot = BuildCornerSelectionMeshSnapshot(
                        sourceFaces),
                    ResultSnapshot = BuildCornerSelectionMeshSnapshot(
                        resultFaces)
                };
            contract.Delta = BuildCornerSelectionMeshDelta(
                contract.SourceSnapshot,
                contract.ResultSnapshot,
                certified);
            return contract;
        }

        private static CornerSelectionMeshSnapshot
            BuildCornerSelectionMeshSnapshot(
                IReadOnlyList<PolygonFace> faces)
        {
            CornerSelectionMeshSnapshot snapshot =
                new CornerSelectionMeshSnapshot();
            if (faces == null || faces.Count == 0)
            {
                return snapshot;
            }

            bool hasBounds = false;
            Bounds bounds = default;
            for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
            {
                PolygonFace face = faces[faceIndex];
                if (face == null || face.Vertices == null ||
                    face.Vertices.Count < 3)
                {
                    continue;
                }
                snapshot.FaceCount++;
                ulong faceSignature = BuildCornerSelectionFaceSignature(face);
                snapshot.FaceSignatures.TryGetValue(
                    faceSignature,
                    out int signatureCount);
                snapshot.FaceSignatures[faceSignature] = signatureCount + 1;

                for (int vertexIndex = 0;
                     vertexIndex < face.Vertices.Count;
                     vertexIndex++)
                {
                    Vector3 vertex = face.Vertices[vertexIndex];
                    SelectionVertexKey vertexKey =
                        new SelectionVertexKey(vertex);
                    if (!snapshot.Vertices.ContainsKey(vertexKey))
                    {
                        snapshot.Vertices.Add(vertexKey, vertex);
                    }
                    Vector3 next = face.Vertices[
                        (vertexIndex + 1) % face.Vertices.Count];
                    snapshot.Edges.Add(new SelectionEdgeKey(vertex, next));

                    if (!hasBounds)
                    {
                        bounds = new Bounds(vertex, Vector3.zero);
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(vertex);
                    }
                }
            }

            snapshot.VertexCount = snapshot.Vertices.Count;
            snapshot.EdgeCount = snapshot.Edges.Count;
            snapshot.Bounds = bounds;
            snapshot.StructuralFingerprint =
                BuildCornerSelectionSnapshotFingerprint(snapshot);
            return snapshot;
        }

        private static CornerSelectionMeshDelta BuildCornerSelectionMeshDelta(
            CornerSelectionMeshSnapshot before,
            CornerSelectionMeshSnapshot after,
            bool certified)
        {
            CornerSelectionMeshDelta delta =
                new CornerSelectionMeshDelta
                {
                    Captured = before != null && after != null,
                    ContinuityExact = false,
                    ConservativeLocalReplacement = true
                };
            if (!delta.Captured)
            {
                delta.Diagnostic = "pre-chip or post-chip snapshot was unavailable";
                return delta;
            }

            delta.BeforeVertexCount = before.VertexCount;
            delta.AfterVertexCount = after.VertexCount;
            delta.BeforeEdgeCount = before.EdgeCount;
            delta.AfterEdgeCount = after.EdgeCount;
            delta.BeforeFaceCount = before.FaceCount;
            delta.AfterFaceCount = after.FaceCount;

            bool hasAffectedBounds = false;
            Bounds affectedBounds = default;
            foreach (KeyValuePair<SelectionVertexKey, Vector3> entry in
                     before.Vertices)
            {
                if (after.Vertices.ContainsKey(entry.Key))
                {
                    delta.PreservedVertexCount++;
                    continue;
                }
                delta.RemovedVertexCount++;
                EncapsulateSelectionDeltaPoint(
                    ref affectedBounds,
                    ref hasAffectedBounds,
                    entry.Value);
            }
            foreach (KeyValuePair<SelectionVertexKey, Vector3> entry in
                     after.Vertices)
            {
                if (before.Vertices.ContainsKey(entry.Key))
                {
                    continue;
                }
                delta.AddedVertexCount++;
                EncapsulateSelectionDeltaPoint(
                    ref affectedBounds,
                    ref hasAffectedBounds,
                    entry.Value);
            }

            foreach (SelectionEdgeKey edge in before.Edges)
            {
                if (after.Edges.Contains(edge))
                {
                    delta.PreservedEdgeCount++;
                }
                else
                {
                    delta.RemovedEdgeCount++;
                }
            }
            foreach (SelectionEdgeKey edge in after.Edges)
            {
                if (!before.Edges.Contains(edge))
                {
                    delta.AddedEdgeCount++;
                }
            }

            CountCornerSelectionFaceDelta(
                before.FaceSignatures,
                after.FaceSignatures,
                out delta.PreservedFaceCount,
                out delta.RemovedFaceCount,
                out delta.AddedFaceCount);

            delta.ChangedFeatureCount =
                delta.AddedVertexCount + delta.RemovedVertexCount +
                delta.AddedEdgeCount + delta.RemovedEdgeCount +
                delta.AddedFaceCount + delta.RemovedFaceCount;
            delta.AffectedBounds = affectedBounds;
            delta.ChangedFeaturesOutsideAffectedBounds = 0;
            delta.Valid = certified && delta.ChangedFeatureCount > 0 &&
                hasAffectedBounds &&
                delta.PreservedVertexCount + delta.RemovedVertexCount ==
                    delta.BeforeVertexCount &&
                delta.PreservedVertexCount + delta.AddedVertexCount ==
                    delta.AfterVertexCount &&
                delta.PreservedEdgeCount + delta.RemovedEdgeCount ==
                    delta.BeforeEdgeCount &&
                delta.PreservedEdgeCount + delta.AddedEdgeCount ==
                    delta.AfterEdgeCount &&
                delta.PreservedFaceCount + delta.RemovedFaceCount ==
                    delta.BeforeFaceCount &&
                delta.PreservedFaceCount + delta.AddedFaceCount ==
                    delta.AfterFaceCount;
            delta.Diagnostic = delta.Valid
                ? "certified conservative local replacement delta captured; " +
                  "cross-snapshot feature continuity remains intentionally uncertain"
                : "corner transaction delta failed count or affected-bounds parity";
            return delta;
        }

        private static void CountCornerSelectionFaceDelta(
            IReadOnlyDictionary<ulong, int> before,
            IReadOnlyDictionary<ulong, int> after,
            out int preserved,
            out int removed,
            out int added)
        {
            preserved = 0;
            removed = 0;
            added = 0;
            foreach (KeyValuePair<ulong, int> entry in before)
            {
                after.TryGetValue(entry.Key, out int afterCount);
                int shared = Mathf.Min(entry.Value, afterCount);
                preserved += shared;
                removed += entry.Value - shared;
            }
            foreach (KeyValuePair<ulong, int> entry in after)
            {
                before.TryGetValue(entry.Key, out int beforeCount);
                added += Mathf.Max(0, entry.Value - beforeCount);
            }
        }

        private static void EncapsulateSelectionDeltaPoint(
            ref Bounds bounds,
            ref bool initialized,
            Vector3 point)
        {
            if (!initialized)
            {
                bounds = new Bounds(point, Vector3.zero);
                initialized = true;
                return;
            }
            bounds.Encapsulate(point);
        }

        private static ulong BuildCornerSelectionFaceSignature(
            PolygonFace face)
        {
            List<SelectionVertexKey> keys =
                new List<SelectionVertexKey>(face.Vertices.Count);
            for (int index = 0; index < face.Vertices.Count; index++)
            {
                keys.Add(new SelectionVertexKey(face.Vertices[index]));
            }
            keys.Sort();
            ulong hash = 1469598103934665603UL;
            hash = MixCornerSelectionHash(hash, (ulong)keys.Count);
            hash = MixCornerSelectionHash(
                hash,
                (ulong)(int)face.ProvenanceKind);
            hash = MixCornerSelectionHash(
                hash,
                unchecked((ulong)(uint)face.ProvenanceIndex));
            for (int index = 0; index < keys.Count; index++)
            {
                hash = MixCornerSelectionHash(
                    hash,
                    unchecked((ulong)(uint)keys[index].X));
                hash = MixCornerSelectionHash(
                    hash,
                    unchecked((ulong)(uint)keys[index].Y));
                hash = MixCornerSelectionHash(
                    hash,
                    unchecked((ulong)(uint)keys[index].Z));
            }
            return hash;
        }

        private static ulong BuildCornerSelectionSnapshotFingerprint(
            CornerSelectionMeshSnapshot snapshot)
        {
            ulong hash = 1469598103934665603UL;
            hash = MixCornerSelectionHash(hash, (ulong)snapshot.VertexCount);
            hash = MixCornerSelectionHash(hash, (ulong)snapshot.EdgeCount);
            hash = MixCornerSelectionHash(hash, (ulong)snapshot.FaceCount);
            List<ulong> signatures =
                new List<ulong>(snapshot.FaceSignatures.Keys);
            signatures.Sort();
            for (int index = 0; index < signatures.Count; index++)
            {
                ulong signature = signatures[index];
                hash = MixCornerSelectionHash(hash, signature);
                hash = MixCornerSelectionHash(
                    hash,
                    (ulong)snapshot.FaceSignatures[signature]);
            }
            return hash;
        }

        private static ulong MixCornerSelectionHash(ulong hash, ulong value)
        {
            unchecked
            {
                hash ^= value;
                return hash * 1099511628211UL;
            }
        }


        private static void AppendCornerSelectionTransactionContract(
            StringBuilder builder,
            CornerSelectionTransactionContract contract)
        {
            CornerSelectionMeshDelta delta = contract == null
                ? null
                : contract.Delta;
            builder.Append("gmSelPhase2=");
            builder.Append(contract != null && contract.Captured ? "1" : "0");
            builder.Append('/');
            builder.Append(contract != null && contract.Certified ? "1" : "0");
            builder.Append('/');
            builder.Append(delta != null && delta.Captured ? "1" : "0");
            builder.Append('/');
            builder.AppendLine(delta != null && delta.Valid ? "1" : "0");
            if (delta == null)
            {
                builder.AppendLine("gmSelDeltaDiagnostic=unavailable");
                return;
            }

            builder.Append("gmSelDeltaTopology=");
            builder.Append(delta.BeforeVertexCount);
            builder.Append('/');
            builder.Append(delta.BeforeEdgeCount);
            builder.Append('/');
            builder.Append(delta.BeforeFaceCount);
            builder.Append("->");
            builder.Append(delta.AfterVertexCount);
            builder.Append('/');
            builder.Append(delta.AfterEdgeCount);
            builder.Append('/');
            builder.AppendLine(delta.AfterFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("gmSelDeltaVertices=preserved:");
            builder.Append(delta.PreservedVertexCount);
            builder.Append(",removed:");
            builder.Append(delta.RemovedVertexCount);
            builder.Append(",added:");
            builder.AppendLine(delta.AddedVertexCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("gmSelDeltaEdges=preserved:");
            builder.Append(delta.PreservedEdgeCount);
            builder.Append(",removed:");
            builder.Append(delta.RemovedEdgeCount);
            builder.Append(",added:");
            builder.AppendLine(delta.AddedEdgeCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("gmSelDeltaFaces=preserved:");
            builder.Append(delta.PreservedFaceCount);
            builder.Append(",removed:");
            builder.Append(delta.RemovedFaceCount);
            builder.Append(",added:");
            builder.AppendLine(delta.AddedFaceCount.ToString(
                CultureInfo.InvariantCulture));
            builder.Append("gmSelDeltaContract=continuityExact:");
            builder.Append(delta.ContinuityExact ? "1" : "0");
            builder.Append(",conservativeLocalReplacement:");
            builder.Append(delta.ConservativeLocalReplacement ? "1" : "0");
            builder.Append(",changedFeatures:");
            builder.Append(delta.ChangedFeatureCount);
            builder.Append(",outsideAffectedBounds:");
            builder.AppendLine(delta.ChangedFeaturesOutsideAffectedBounds
                .ToString(CultureInfo.InvariantCulture));
            builder.Append("gmSelDeltaAffectedBounds=");
            builder.AppendLine(FormatCornerSelectionDeltaBounds(
                delta.AffectedBounds));
            builder.Append("gmSelDeltaDiagnostic=");
            builder.AppendLine(delta.Diagnostic ?? string.Empty);
        }

        private static string FormatCornerSelectionDeltaBounds(Bounds bounds)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "center({0:G9}/{1:G9}/{2:G9}) size({3:G9}/{4:G9}/{5:G9})",
                bounds.center.x,
                bounds.center.y,
                bounds.center.z,
                bounds.size.x,
                bounds.size.y,
                bounds.size.z);
        }
    }
}
