using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal sealed class TreeSourceAuditResult
    {
        public bool Passed;
        public bool SourceFolderAvailable;
        public int FoundModelCount;
        public int FoundTextureCount;
        public int FailureCount;
        public int WarningCount;
        public string Timestamp = string.Empty;
        public string Report = string.Empty;
    }

    internal static class TreeSourceAssetAudit
    {
        private sealed class ModelExpectation
        {
            public TreeFamily Family;
            public int VariantIndex;
            public string Path = string.Empty;
            public string ExpectedBarkMaterial = string.Empty;
            public string ExpectedFoliageMaterial = string.Empty;
            public bool ExpectsFoliage;
        }

        private sealed class TextureExpectation
        {
            public string Path = string.Empty;
            public string Usage = string.Empty;
            public bool ExpectsAlpha;
            public bool IsNormalMap;
            public bool IsTintOriented;
        }

        private sealed class AuditContext
        {
            public readonly StringBuilder Builder = new StringBuilder(65536);
            public int Failures;
            public int Warnings;

            public void Fail(string message)
            {
                Failures++;
                Builder.Append("FAIL: ").AppendLine(message);
            }

            public void Warn(string message)
            {
                Warnings++;
                Builder.Append("WARNING: ").AppendLine(message);
            }
        }

        private sealed class VertexStatistics
        {
            public long Count;
            public Vector4 Minimum = new Vector4(
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity,
                float.PositiveInfinity);
            public Vector4 Maximum = new Vector4(
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity,
                float.NegativeInfinity);
            public double SumR;
            public double SumG;
            public double SumB;
            public double SumA;
            public double SumY;
            public double SumRed;
            public double SumYSquared;
            public double SumRedSquared;
            public double SumYRed;

            public void Add(Color32 color, float localY)
            {
                float r = color.r / 255f;
                float g = color.g / 255f;
                float b = color.b / 255f;
                float a = color.a / 255f;
                var value = new Vector4(r, g, b, a);
                Minimum = new Vector4(
                    Mathf.Min(Minimum.x, value.x),
                    Mathf.Min(Minimum.y, value.y),
                    Mathf.Min(Minimum.z, value.z),
                    Mathf.Min(Minimum.w, value.w));
                Maximum = new Vector4(
                    Mathf.Max(Maximum.x, value.x),
                    Mathf.Max(Maximum.y, value.y),
                    Mathf.Max(Maximum.z, value.z),
                    Mathf.Max(Maximum.w, value.w));
                SumR += r;
                SumG += g;
                SumB += b;
                SumA += a;
                SumY += localY;
                SumRed += r;
                SumYSquared += localY * localY;
                SumRedSquared += r * r;
                SumYRed += localY * r;
                Count++;
            }

            public Vector4 Average => Count > 0
                ? new Vector4(
                    (float)(SumR / Count),
                    (float)(SumG / Count),
                    (float)(SumB / Count),
                    (float)(SumA / Count))
                : Vector4.zero;

            public bool TryGetRedHeightCorrelation(out double correlation)
            {
                correlation = 0.0;
                if (Count < 2)
                {
                    return false;
                }

                double numerator = Count * SumYRed - SumY * SumRed;
                double yTerm = Count * SumYSquared - SumY * SumY;
                double redTerm = Count * SumRedSquared - SumRed * SumRed;
                double denominator = Math.Sqrt(
                    Math.Max(0.0, yTerm) * Math.Max(0.0, redTerm));
                if (denominator <= 1e-12)
                {
                    return false;
                }

                correlation = numerator / denominator;
                return true;
            }
        }

        private sealed class ModelTotals
        {
            public int RendererCount;
            public int MeshCount;
            public int SkinnedRendererCount;
            public int SubmeshCount;
            public long VertexCount;
            public long TriangleCount;
            public int MeshesWithColors;
            public int MeshesWithUv0;
            public int MeshesWithUv1;
            public int MeshesWithUv2;
            public int MeshesWithUv3;
            public int MeshesWithNormals;
            public int MeshesWithTangents;
            public bool HasBounds;
            public Bounds CombinedBounds;
            public readonly HashSet<string> MaterialNames =
                new HashSet<string>(StringComparer.Ordinal);
            public readonly VertexStatistics AllVertexColors =
                new VertexStatistics();
            public readonly VertexStatistics BarkCandidateColors =
                new VertexStatistics();
            public readonly VertexStatistics FoliageCandidateColors =
                new VertexStatistics();
        }

        private static readonly ModelExpectation[] ModelExpectations =
            BuildModelExpectations();

        private static readonly TextureExpectation[] TextureExpectations =
        {
            Texture(
                "Bark_DeadTree.png",
                "Dead bark albedo",
                false,
                false,
                false),
            Texture(
                "Bark_DeadTree_Normal.png",
                "Dead bark normal",
                false,
                true,
                false),
            Texture(
                "Bark_NormalTree.png",
                "Common/Pine bark albedo",
                false,
                false,
                false),
            Texture(
                "Bark_NormalTree_Normal.png",
                "Common/Pine bark normal",
                false,
                true,
                false),
            Texture(
                "Bark_TwistedTree.png",
                "Twisted bark albedo",
                false,
                false,
                false),
            Texture(
                "Bark_TwistedTree_Normal.png",
                "Twisted bark normal",
                false,
                true,
                false),
            Texture(
                "Leaf_Pine.png",
                "Pine white/tintable foliage",
                true,
                false,
                true),
            Texture(
                "Leaf_Pine_C.png",
                "Pine coloured foliage",
                true,
                false,
                false),
            Texture(
                "Leaves_NormalTree.png",
                "Common white/tintable foliage",
                true,
                false,
                true),
            Texture(
                "Leaves_NormalTree_C.png",
                "Common coloured foliage",
                true,
                false,
                false),
            Texture(
                "Leaves_TwistedTree.png",
                "Twisted white/tintable foliage",
                true,
                false,
                true),
            Texture(
                "Leaves_TwistedTree_C.png",
                "Twisted coloured foliage",
                true,
                false,
                false)
        };

        internal static bool SourceFolderExists =>
            AssetDatabase.IsValidFolder(TreeReferenceGallery.SourceRootPath);

        internal static TreeSourceAuditResult Run(TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var context = new AuditContext();
            bool sourceFolderAvailable = SourceFolderExists;
            int foundModelCount = 0;
            int foundTextureCount = 0;

            AppendHeader(
                context,
                gallery,
                timestamp,
                sourceFolderAvailable);
            AppendGalleryContract(context, gallery);

            context.Builder.AppendLine("[Model Assets]");
            for (int index = 0; index < ModelExpectations.Length; index++)
            {
                if (AuditModel(context, ModelExpectations[index]))
                {
                    foundModelCount++;
                }
            }

            context.Builder.AppendLine("[Texture Assets]");
            for (int index = 0; index < TextureExpectations.Length; index++)
            {
                if (AuditTexture(context, TextureExpectations[index]))
                {
                    foundTextureCount++;
                }
            }

            bool passed =
                sourceFolderAvailable &&
                foundModelCount == ModelExpectations.Length &&
                foundTextureCount == TextureExpectations.Length &&
                context.Failures == 0;

            AppendSummary(
                context,
                passed,
                sourceFolderAvailable,
                foundModelCount,
                foundTextureCount);

            return new TreeSourceAuditResult
            {
                Passed = passed,
                SourceFolderAvailable = sourceFolderAvailable,
                FoundModelCount = foundModelCount,
                FoundTextureCount = foundTextureCount,
                FailureCount = context.Failures,
                WarningCount = context.Warnings,
                Timestamp = timestamp,
                Report = context.Builder.ToString()
            };
        }

        private static void AppendHeader(
            AuditContext context,
            TreeReferenceGallery gallery,
            string timestamp,
            bool sourceFolderAvailable)
        {
            context.Builder.AppendLine(
                "[TREE-GALLERY.1 Complete Tree Source Audit]");
            context.Builder.Append("Generated: ").AppendLine(timestamp);
            context.Builder.Append("Unity: ")
                .AppendLine(Application.unityVersion);
            Scene scene = gallery != null
                ? gallery.gameObject.scene
                : SceneManager.GetActiveScene();
            context.Builder.Append("Scene: ")
                .AppendLine(string.IsNullOrEmpty(scene.path)
                    ? scene.name
                    : scene.path);
            context.Builder.Append("Gallery: ")
                .AppendLine(gallery != null
                    ? TreeReferenceGalleryBuilder.GetHierarchyPath(
                        gallery.transform)
                    : "None");
            context.Builder.Append("Source root: ")
                .AppendLine(TreeReferenceGallery.SourceRootPath);
            context.Builder.Append("Source folder available: ")
                .AppendLine(sourceFolderAvailable ? "Yes" : "No");
            context.Builder.AppendLine(
                "Mutation: source FBXs, textures, importers, materials, meshes, and hierarchy are read only; only the gallery's serialized last-report state is updated by its Inspector action");
            context.Builder.AppendLine();

            if (!sourceFolderAvailable)
            {
                context.Fail(
                    "The ignored local source vault is absent. Transfer " +
                    TreeReferenceGallery.SourceRootPath +
                    " before running the authoritative Unity asset audit.");
                context.Builder.AppendLine();
            }
        }

        private static void AppendGalleryContract(
            AuditContext context,
            TreeReferenceGallery gallery)
        {
            context.Builder.AppendLine("[Standalone Gallery Contract]");
            if (gallery == null)
            {
                context.Fail("No TreeReferenceGallery was supplied.");
                context.Builder.AppendLine();
                return;
            }

            GeneratedGround ground = gallery.ReferenceGround;
            context.Builder.Append("Reference Ground: ")
                .AppendLine(ground != null
                    ? TreeReferenceGalleryBuilder.GetHierarchyPath(
                        ground.transform)
                    : "None");
            context.Builder.Append("Gallery parent: ")
                .AppendLine(gallery.transform.parent != null
                    ? TreeReferenceGalleryBuilder.GetHierarchyPath(
                        gallery.transform.parent)
                    : "Scene root");

            if (ground == null)
            {
                context.Fail(
                    "The standalone gallery requires an explicit Reference Ground.");
            }
            else
            {
                if (gallery.transform.IsChildOf(ground.transform))
                {
                    context.Fail(
                        "The gallery is parented under its Reference Ground. " +
                        "Use Place as Ground Sibling; Ground does not own the gallery.");
                }

                if (gallery.gameObject.scene != ground.gameObject.scene)
                {
                    context.Fail(
                        "The gallery and Reference Ground are not in the same scene.");
                }
            }

            GeneratedGround ancestor =
                gallery.GetComponentInParent<GeneratedGround>(true);
            if (ancestor != null)
            {
                context.Fail(
                    "The gallery has a GeneratedGround ancestor (" +
                    TreeReferenceGalleryBuilder.GetHierarchyPath(
                        ancestor.transform) +
                    "). The gallery must remain an independent sibling/root object.");
            }

            context.Builder.AppendLine();
        }

        private static bool AuditModel(
            AuditContext context,
            ModelExpectation expectation)
        {
            context.Builder.Append("--- ")
                .Append(expectation.Family)
                .Append(' ')
                .Append(expectation.VariantIndex)
                .Append(" | ")
                .AppendLine(expectation.Path);

            if (!File.Exists(expectation.Path))
            {
                context.Fail("Missing FBX file: " + expectation.Path);
                context.Builder.AppendLine();
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(expectation.Path);
            if (string.IsNullOrEmpty(guid))
            {
                context.Fail("FBX has no AssetDatabase GUID: " + expectation.Path);
            }
            context.Builder.Append("GUID: ").AppendLine(
                string.IsNullOrEmpty(guid) ? "None" : guid);

            var importer = AssetImporter.GetAtPath(expectation.Path)
                as ModelImporter;
            if (importer == null)
            {
                context.Fail("Asset is not imported by ModelImporter.");
                context.Builder.AppendLine();
                return true;
            }

            AppendModelImporter(context.Builder, importer);

            GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(
                expectation.Path);
            if (root == null)
            {
                context.Fail("FBX root GameObject failed to load.");
                context.Builder.AppendLine();
                return true;
            }

            context.Builder.Append("Root transform: position=")
                .Append(FormatVector(root.transform.localPosition))
                .Append(" rotation=")
                .Append(FormatVector(root.transform.localEulerAngles))
                .Append(" scale=")
                .AppendLine(FormatVector(root.transform.localScale));
            context.Builder.AppendLine("Hierarchy:");
            AppendHierarchy(context.Builder, root.transform, root.transform, 0);

            var totals = new ModelTotals();
            MeshRenderer[] meshRenderers =
                root.GetComponentsInChildren<MeshRenderer>(true);
            SkinnedMeshRenderer[] skinnedRenderers =
                root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            totals.RendererCount = meshRenderers.Length +
                skinnedRenderers.Length;
            totals.SkinnedRendererCount = skinnedRenderers.Length;

            context.Builder.AppendLine("Renderer/mesh details:");
            for (int index = 0; index < meshRenderers.Length; index++)
            {
                MeshRenderer renderer = meshRenderers[index];
                MeshFilter filter = renderer.GetComponent<MeshFilter>();
                AuditRendererMesh(
                    context,
                    expectation,
                    root.transform,
                    renderer,
                    filter != null ? filter.sharedMesh : null,
                    totals);
            }

            for (int index = 0; index < skinnedRenderers.Length; index++)
            {
                SkinnedMeshRenderer renderer = skinnedRenderers[index];
                AuditRendererMesh(
                    context,
                    expectation,
                    root.transform,
                    renderer,
                    renderer.sharedMesh,
                    totals);
            }

            if (totals.RendererCount == 0)
            {
                context.Fail("FBX contains no MeshRenderer or SkinnedMeshRenderer.");
            }
            if (totals.SkinnedRendererCount > 0)
            {
                context.Fail(
                    "FBX contains " + totals.SkinnedRendererCount +
                    " SkinnedMeshRenderer component(s); the retained tree contract " +
                    "expects static source meshes without skeleton ownership.");
            }
            if (totals.MeshCount == 0)
            {
                context.Fail("FBX contains no auditable mesh.");
            }

            ValidateExpectedMaterials(context, expectation, totals.MaterialNames);
            ValidateModelAttributes(context, totals);
            AppendModelTotals(context.Builder, totals);
            context.Builder.AppendLine();
            return true;
        }

        private static void AppendModelImporter(
            StringBuilder builder,
            ModelImporter importer)
        {
            builder.Append("Importer: globalScale=")
                .Append(importer.globalScale.ToString("0.#####"))
                .Append(" useFileScale=")
                .Append(importer.useFileScale ? "Yes" : "No")
                .Append(" useFileUnits=")
                .Append(importer.useFileUnits ? "Yes" : "No")
                .Append(" fileScale=")
                .Append(importer.fileScale.ToString("0.#####"))
                .Append(" bakeAxisConversion=")
                .Append(importer.bakeAxisConversion ? "Yes" : "No")
                .AppendLine();
            builder.Append("Importer geometry: normals=")
                .Append(importer.importNormals)
                .Append(" tangents=")
                .Append(importer.importTangents)
                .Append(" readable=")
                .Append(importer.isReadable ? "Yes" : "No")
                .Append(" animation=")
                .Append(importer.importAnimation ? "Yes" : "No")
                .Append(" blendShapes=")
                .Append(importer.importBlendShapes ? "Yes" : "No")
                .AppendLine();
        }

        private static void AuditRendererMesh(
            AuditContext context,
            ModelExpectation expectation,
            Transform root,
            Renderer renderer,
            Mesh mesh,
            ModelTotals totals)
        {
            string rendererPath = GetRelativePath(root, renderer.transform);
            string[] materialNames = GetMaterialNames(renderer.sharedMaterials);
            context.Builder.Append("  ")
                .Append(rendererPath)
                .Append(" | ")
                .Append(renderer.GetType().Name)
                .Append(" | materials=[")
                .Append(string.Join(", ", materialNames))
                .Append(']');

            for (int index = 0; index < materialNames.Length; index++)
            {
                if (!string.IsNullOrEmpty(materialNames[index]))
                {
                    totals.MaterialNames.Add(materialNames[index]);
                }
            }

            if (mesh == null)
            {
                context.Builder.AppendLine(" | mesh=None");
                context.Fail("Renderer has no mesh: " + rendererPath);
                return;
            }

            totals.MeshCount++;
            totals.SubmeshCount += mesh.subMeshCount;
            totals.VertexCount += mesh.vertexCount;
            long meshTriangles = 0L;
            for (int submeshIndex = 0;
                 submeshIndex < mesh.subMeshCount;
                 submeshIndex++)
            {
                MeshTopology topology = mesh.GetTopology(submeshIndex);
                if (topology != MeshTopology.Triangles)
                {
                    context.Fail(
                        "Non-triangle topology " + topology + " in " +
                        rendererPath + " submesh " + submeshIndex + ".");
                    continue;
                }

                meshTriangles +=
                    (long)(mesh.GetIndexCount(submeshIndex) / 3UL);
            }
            totals.TriangleCount += meshTriangles;

            bool hasColors =
                mesh.HasVertexAttribute(VertexAttribute.Color);
            bool hasUv0 =
                mesh.HasVertexAttribute(VertexAttribute.TexCoord0);
            bool hasUv1 =
                mesh.HasVertexAttribute(VertexAttribute.TexCoord1);
            bool hasUv2 =
                mesh.HasVertexAttribute(VertexAttribute.TexCoord2);
            bool hasUv3 =
                mesh.HasVertexAttribute(VertexAttribute.TexCoord3);
            bool hasNormals =
                mesh.HasVertexAttribute(VertexAttribute.Normal);
            bool hasTangents =
                mesh.HasVertexAttribute(VertexAttribute.Tangent);

            totals.MeshesWithColors += hasColors ? 1 : 0;
            totals.MeshesWithUv0 += hasUv0 ? 1 : 0;
            totals.MeshesWithUv1 += hasUv1 ? 1 : 0;
            totals.MeshesWithUv2 += hasUv2 ? 1 : 0;
            totals.MeshesWithUv3 += hasUv3 ? 1 : 0;
            totals.MeshesWithNormals += hasNormals ? 1 : 0;
            totals.MeshesWithTangents += hasTangents ? 1 : 0;

            Matrix4x4 meshToRoot =
                root.worldToLocalMatrix * renderer.transform.localToWorldMatrix;
            Bounds meshBounds = TransformBounds(mesh.bounds, meshToRoot);
            EncapsulateBounds(totals, meshBounds);

            bool barkCandidate = Contains(
                materialNames,
                expectation.ExpectedBarkMaterial);
            bool foliageCandidate = expectation.ExpectsFoliage && Contains(
                materialNames,
                expectation.ExpectedFoliageMaterial);
            if (hasColors)
            {
                AccumulateVertexStatistics(
                    mesh,
                    meshToRoot,
                    totals.AllVertexColors,
                    barkCandidate ? totals.BarkCandidateColors : null,
                    foliageCandidate ? totals.FoliageCandidateColors : null);
            }

            context.Builder.Append(" | mesh=")
                .Append(mesh.name)
                .Append(" vertices=")
                .Append(mesh.vertexCount)
                .Append(" triangles=")
                .Append(meshTriangles)
                .Append(" submeshes=")
                .Append(mesh.subMeshCount)
                .Append(" bounds=")
                .Append(FormatBounds(meshBounds))
                .Append(" attributes[C/UV0/UV1/UV2/UV3/N/T]=")
                .Append(hasColors ? 'Y' : 'N')
                .Append('/')
                .Append(hasUv0 ? 'Y' : 'N')
                .Append('/')
                .Append(hasUv1 ? 'Y' : 'N')
                .Append('/')
                .Append(hasUv2 ? 'Y' : 'N')
                .Append('/')
                .Append(hasUv3 ? 'Y' : 'N')
                .Append('/')
                .Append(hasNormals ? 'Y' : 'N')
                .Append('/')
                .Append(hasTangents ? 'Y' : 'N')
                .AppendLine();

            if (renderer.sharedMaterials.Length != mesh.subMeshCount)
            {
                context.Fail(
                    "Material-slot/submesh mismatch on " + rendererPath +
                    ": " + renderer.sharedMaterials.Length + " material slots for " +
                    mesh.subMeshCount + " submeshes.");
            }
        }

        private static void AccumulateVertexStatistics(
            Mesh mesh,
            Matrix4x4 meshToRoot,
            VertexStatistics all,
            VertexStatistics bark,
            VertexStatistics foliage)
        {
            using (Mesh.MeshDataArray dataArray =
                   Mesh.AcquireReadOnlyMeshData(mesh))
            {
                Mesh.MeshData data = dataArray[0];
                using (var colors = new NativeArray<Color32>(
                           data.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                using (var positions = new NativeArray<Vector3>(
                           data.vertexCount,
                           Allocator.Temp,
                           NativeArrayOptions.UninitializedMemory))
                {
                    data.GetColors(colors);
                    data.GetVertices(positions);
                    for (int index = 0; index < data.vertexCount; index++)
                    {
                        float localY = meshToRoot.MultiplyPoint3x4(
                            positions[index]).y;
                        Color32 color = colors[index];
                        all.Add(color, localY);
                        bark?.Add(color, localY);
                        foliage?.Add(color, localY);
                    }
                }
            }
        }

        private static void ValidateExpectedMaterials(
            AuditContext context,
            ModelExpectation expectation,
            HashSet<string> actual)
        {
            var expected = new HashSet<string>(StringComparer.Ordinal)
            {
                expectation.ExpectedBarkMaterial
            };
            if (expectation.ExpectsFoliage)
            {
                expected.Add(expectation.ExpectedFoliageMaterial);
            }

            foreach (string expectedName in expected)
            {
                if (!actual.Contains(expectedName))
                {
                    context.Fail(
                        "Missing expected material identity: " + expectedName +
                        ". Actual: [" + JoinSorted(actual) + "].");
                }
            }

            foreach (string actualName in actual)
            {
                if (!expected.Contains(actualName))
                {
                    context.Fail(
                        "Unrecognized material identity: " + actualName +
                        ". Expected: [" + JoinSorted(expected) + "].");
                }
            }
        }

        private static void ValidateModelAttributes(
            AuditContext context,
            ModelTotals totals)
        {
            if (totals.MeshesWithUv0 != totals.MeshCount)
            {
                context.Fail(
                    "UV0 is missing from " +
                    (totals.MeshCount - totals.MeshesWithUv0) +
                    " mesh(es).");
            }
            if (totals.MeshesWithNormals != totals.MeshCount)
            {
                context.Fail(
                    "Normals are missing from " +
                    (totals.MeshCount - totals.MeshesWithNormals) +
                    " mesh(es).");
            }
            if (totals.MeshesWithTangents != totals.MeshCount)
            {
                context.Fail(
                    "Tangents are missing from " +
                    (totals.MeshCount - totals.MeshesWithTangents) +
                    " mesh(es); bark normal-map validation requires tangents.");
            }
            if (totals.MeshesWithColors != totals.MeshCount)
            {
                context.Fail(
                    "Vertex colours are missing from " +
                    (totals.MeshCount - totals.MeshesWithColors) +
                    " mesh(es); imported wind-mask evidence would be incomplete.");
            }
        }

        private static void AppendModelTotals(
            StringBuilder builder,
            ModelTotals totals)
        {
            builder.Append("Totals: renderers=")
                .Append(totals.RendererCount)
                .Append(" meshes=")
                .Append(totals.MeshCount)
                .Append(" submeshes=")
                .Append(totals.SubmeshCount)
                .Append(" vertices=")
                .Append(totals.VertexCount)
                .Append(" triangles=")
                .Append(totals.TriangleCount)
                .AppendLine();
            builder.Append("Materials: [")
                .Append(JoinSorted(totals.MaterialNames))
                .AppendLine("]");

            if (totals.HasBounds)
            {
                builder.Append("Combined local bounds: ")
                    .AppendLine(FormatBounds(totals.CombinedBounds));
                builder.Append("Lowest visible local Y: ")
                    .AppendLine(totals.CombinedBounds.min.y.ToString("0.#####"));
                builder.Append("Visible height / canopy width: ")
                    .Append(totals.CombinedBounds.size.y.ToString("0.#####"))
                    .Append(" / ")
                    .AppendLine(Mathf.Max(
                        totals.CombinedBounds.size.x,
                        totals.CombinedBounds.size.z).ToString("0.#####"));
            }

            builder.Append("Attribute coverage (of ")
                .Append(totals.MeshCount)
                .Append(" meshes): colours=")
                .Append(totals.MeshesWithColors)
                .Append(" UV0=")
                .Append(totals.MeshesWithUv0)
                .Append(" UV1=")
                .Append(totals.MeshesWithUv1)
                .Append(" UV2=")
                .Append(totals.MeshesWithUv2)
                .Append(" UV3=")
                .Append(totals.MeshesWithUv3)
                .Append(" normals=")
                .Append(totals.MeshesWithNormals)
                .Append(" tangents=")
                .Append(totals.MeshesWithTangents)
                .AppendLine();

            AppendVertexStatistics(
                builder,
                "All vertex colours",
                totals.AllVertexColors);
            AppendVertexStatistics(
                builder,
                "Bark-candidate vertex colours",
                totals.BarkCandidateColors);
            AppendVertexStatistics(
                builder,
                "Foliage-candidate vertex colours",
                totals.FoliageCandidateColors);
        }

        private static void AppendVertexStatistics(
            StringBuilder builder,
            string label,
            VertexStatistics statistics)
        {
            builder.Append(label).Append(": count=")
                .Append(statistics.Count);
            if (statistics.Count == 0)
            {
                builder.AppendLine(" stats=Unavailable");
                return;
            }

            builder.Append(" min=")
                .Append(FormatVector(statistics.Minimum))
                .Append(" max=")
                .Append(FormatVector(statistics.Maximum))
                .Append(" avg=")
                .Append(FormatVector(statistics.Average));
            if (statistics.TryGetRedHeightCorrelation(out double correlation))
            {
                builder.Append(" red/height correlation=")
                    .Append(correlation.ToString("0.#####"));
            }
            else
            {
                builder.Append(" red/height correlation=Unavailable");
            }
            builder.AppendLine();
        }

        private static bool AuditTexture(
            AuditContext context,
            TextureExpectation expectation)
        {
            context.Builder.Append("--- ")
                .Append(expectation.Usage)
                .Append(" | ")
                .AppendLine(expectation.Path);

            if (!File.Exists(expectation.Path))
            {
                context.Fail("Missing texture file: " + expectation.Path);
                context.Builder.AppendLine();
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(expectation.Path);
            if (string.IsNullOrEmpty(guid))
            {
                context.Fail(
                    "Texture has no AssetDatabase GUID: " + expectation.Path);
            }
            context.Builder.Append("GUID: ").AppendLine(
                string.IsNullOrEmpty(guid) ? "None" : guid);

            var importer = AssetImporter.GetAtPath(expectation.Path)
                as TextureImporter;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                expectation.Path);
            if (importer == null)
            {
                context.Fail("Asset is not imported by TextureImporter.");
                context.Builder.AppendLine();
                return true;
            }
            if (texture == null)
            {
                context.Fail("Texture2D failed to load.");
                context.Builder.AppendLine();
                return true;
            }

            bool sourceHasAlpha = importer.DoesSourceTextureHaveAlpha();
            long runtimeBytes = Profiler.GetRuntimeMemorySizeLong(texture);
            long sourceBytes = new FileInfo(expectation.Path).Length;

            context.Builder.Append("Classification: ")
                .Append(expectation.IsNormalMap ? "Bark normal" :
                    expectation.ExpectsAlpha ? "Foliage" : "Bark albedo")
                .Append(" | tint-oriented=")
                .Append(expectation.IsTintOriented ? "Yes" : "No")
                .AppendLine();
            context.Builder.Append("Dimensions/format: ")
                .Append(texture.width)
                .Append('x')
                .Append(texture.height)
                .Append(" format=")
                .Append(texture.format)
                .Append(" mipCount=")
                .Append(texture.mipmapCount)
                .AppendLine();
            context.Builder.Append("Importer: type=")
                .Append(importer.textureType)
                .Append(" sRGB=")
                .Append(importer.sRGBTexture ? "Yes" : "No")
                .Append(" sourceAlpha=")
                .Append(sourceHasAlpha ? "Yes" : "No")
                .Append(" alphaSource=")
                .Append(importer.alphaSource)
                .Append(" alphaTransparency=")
                .Append(importer.alphaIsTransparency ? "Yes" : "No")
                .Append(" mipmaps=")
                .Append(importer.mipmapEnabled ? "Yes" : "No")
                .Append(" readable=")
                .Append(importer.isReadable ? "Yes" : "No")
                .AppendLine();
            context.Builder.Append("Compression: ")
                .Append(importer.textureCompression)
                .Append(" quality=")
                .Append(importer.compressionQuality)
                .Append(" maxSize=")
                .Append(importer.maxTextureSize)
                .AppendLine();
            context.Builder.Append("Source/runtime bytes: ")
                .Append(sourceBytes)
                .Append(" / ")
                .Append(runtimeBytes)
                .AppendLine();

            if (texture.width <= 0 || texture.height <= 0)
            {
                context.Fail("Texture has invalid dimensions.");
            }
            if (expectation.ExpectsAlpha && !sourceHasAlpha)
            {
                context.Fail(
                    "Foliage texture lacks the required source alpha channel.");
            }
            if (expectation.IsNormalMap &&
                (importer.textureType != TextureImporterType.NormalMap ||
                 importer.sRGBTexture))
            {
                context.Warn(
                    "NORMAL_IMPORT_CORRECTION_REQUIRED: expected NormalMap with " +
                    "sRGB disabled; correction belongs to TREE-GALLERY.2.");
            }
            if (!expectation.IsNormalMap && !importer.sRGBTexture)
            {
                context.Warn(
                    "Colour texture has sRGB disabled; verify intended colour-space " +
                    "handling before TREE-GALLERY.2 material creation.");
            }

            context.Builder.AppendLine();
            return true;
        }

        private static void AppendSummary(
            AuditContext context,
            bool passed,
            bool sourceFolderAvailable,
            int foundModelCount,
            int foundTextureCount)
        {
            context.Builder.AppendLine("[Summary]");
            context.Builder.Append("Status: ")
                .AppendLine(passed ? "PASS" : "FAIL");
            context.Builder.Append("Source folder: ")
                .AppendLine(sourceFolderAvailable ? "Available" : "Missing");
            context.Builder.Append("Models found: ")
                .Append(foundModelCount)
                .Append(" / ")
                .AppendLine(ModelExpectations.Length.ToString());
            context.Builder.Append("Textures found: ")
                .Append(foundTextureCount)
                .Append(" / ")
                .AppendLine(TextureExpectations.Length.ToString());
            context.Builder.Append("Failures / warnings: ")
                .Append(context.Failures)
                .Append(" / ")
                .AppendLine(context.Warnings.ToString());
            context.Builder.AppendLine(
                passed
                    ? "Readiness: TREE-GALLERY.2 is unblocked at the source-contract level. Apply only the explicitly reported bark-normal import corrections before material validation."
                    : "Readiness: TREE-GALLERY.2 remains blocked. Resolve every FAIL item and rerun this complete audit; do not infer material mapping or pivot corrections from partial results.");
        }

        private static ModelExpectation[] BuildModelExpectations()
        {
            var models = new List<ModelExpectation>(20);
            AddFamilyModels(
                models,
                TreeFamily.Common,
                "CommonTree",
                "Bark_NormalTree",
                "Leaves_NormalTree",
                true);
            AddFamilyModels(
                models,
                TreeFamily.Pine,
                "Pine",
                "Bark_NormalTree",
                "Leaves_Pine",
                true);
            AddFamilyModels(
                models,
                TreeFamily.Twisted,
                "TwistedTree",
                "Bark_TwistedTree",
                "Leaves_TwistedTree",
                true);
            AddFamilyModels(
                models,
                TreeFamily.Dead,
                "DeadTree",
                "Bark_DeadTree",
                string.Empty,
                false);
            return models.ToArray();
        }

        private static void AddFamilyModels(
            List<ModelExpectation> models,
            TreeFamily family,
            string filenamePrefix,
            string barkMaterial,
            string foliageMaterial,
            bool expectsFoliage)
        {
            for (int variantIndex = 1; variantIndex <= 5; variantIndex++)
            {
                models.Add(new ModelExpectation
                {
                    Family = family,
                    VariantIndex = variantIndex,
                    Path = TreeReferenceGallery.SourceRootPath + "/" +
                        filenamePrefix + "_" + variantIndex + ".fbx",
                    ExpectedBarkMaterial = barkMaterial,
                    ExpectedFoliageMaterial = foliageMaterial,
                    ExpectsFoliage = expectsFoliage
                });
            }
        }

        private static TextureExpectation Texture(
            string filename,
            string usage,
            bool expectsAlpha,
            bool isNormalMap,
            bool isTintOriented)
        {
            return new TextureExpectation
            {
                Path = TreeReferenceGallery.SourceRootPath + "/" + filename,
                Usage = usage,
                ExpectsAlpha = expectsAlpha,
                IsNormalMap = isNormalMap,
                IsTintOriented = isTintOriented
            };
        }


        private static string JoinSorted(IEnumerable<string> values)
        {
            var sorted = new List<string>(values);
            sorted.Sort(StringComparer.Ordinal);
            return string.Join(", ", sorted);
        }

        private static string[] GetMaterialNames(Material[] materials)
        {
            var names = new string[materials.Length];
            for (int index = 0; index < materials.Length; index++)
            {
                names[index] = materials[index] != null
                    ? NormalizeMaterialName(materials[index].name)
                    : "<null>";
            }
            return names;
        }

        private static string NormalizeMaterialName(string name)
        {
            const string instanceSuffix = " (Instance)";
            if (!string.IsNullOrEmpty(name) &&
                name.EndsWith(instanceSuffix, StringComparison.Ordinal))
            {
                return name.Substring(0, name.Length - instanceSuffix.Length);
            }
            return name ?? string.Empty;
        }

        private static bool Contains(string[] values, string expected)
        {
            for (int index = 0; index < values.Length; index++)
            {
                if (string.Equals(
                        values[index],
                        expected,
                        StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private static void AppendHierarchy(
            StringBuilder builder,
            Transform root,
            Transform current,
            int depth)
        {
            builder.Append(' ', (depth + 1) * 2)
                .Append(GetRelativePath(root, current))
                .Append(" | localPosition=")
                .Append(FormatVector(current.localPosition))
                .Append(" localRotation=")
                .Append(FormatVector(current.localEulerAngles))
                .Append(" localScale=")
                .AppendLine(FormatVector(current.localScale));

            for (int childIndex = 0;
                 childIndex < current.childCount;
                 childIndex++)
            {
                AppendHierarchy(
                    builder,
                    root,
                    current.GetChild(childIndex),
                    depth + 1);
            }
        }

        private static string GetRelativePath(Transform root, Transform target)
        {
            if (target == root)
            {
                return root.name;
            }

            var names = new List<string>();
            Transform current = target;
            while (current != null && current != root)
            {
                names.Add(current.name);
                current = current.parent;
            }
            names.Add(root.name);
            names.Reverse();
            return string.Join("/", names);
        }

        private static Bounds TransformBounds(
            Bounds bounds,
            Matrix4x4 matrix)
        {
            Vector3 center = matrix.MultiplyPoint3x4(bounds.center);
            Vector3 extents = bounds.extents;
            Vector3 axisX = matrix.MultiplyVector(
                new Vector3(extents.x, 0f, 0f));
            Vector3 axisY = matrix.MultiplyVector(
                new Vector3(0f, extents.y, 0f));
            Vector3 axisZ = matrix.MultiplyVector(
                new Vector3(0f, 0f, extents.z));
            Vector3 transformedExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, transformedExtents * 2f);
        }

        private static void EncapsulateBounds(
            ModelTotals totals,
            Bounds bounds)
        {
            if (!totals.HasBounds)
            {
                totals.CombinedBounds = bounds;
                totals.HasBounds = true;
                return;
            }

            totals.CombinedBounds.Encapsulate(bounds.min);
            totals.CombinedBounds.Encapsulate(bounds.max);
        }

        private static string FormatBounds(Bounds bounds)
        {
            return "center=" + FormatVector(bounds.center) +
                " size=" + FormatVector(bounds.size);
        }

        private static string FormatVector(Vector3 value)
        {
            return "(" + value.x.ToString("0.#####") + ", " +
                value.y.ToString("0.#####") + ", " +
                value.z.ToString("0.#####") + ")";
        }

        private static string FormatVector(Vector4 value)
        {
            return "(" + value.x.ToString("0.#####") + ", " +
                value.y.ToString("0.#####") + ", " +
                value.z.ToString("0.#####") + ", " +
                value.w.ToString("0.#####") + ")";
        }
    }
}
