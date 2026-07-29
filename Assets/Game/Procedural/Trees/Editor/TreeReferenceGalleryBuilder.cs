using System;
using System.Collections.Generic;
using System.Text;
using ProgrammaticStylized3D.Geometry.Ground;
using ProgrammaticStylized3D.Weather;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal sealed class TreeGalleryBuildResult
    {
        public bool Passed;
        public int SpecimenCount;
        public string Timestamp = string.Empty;
        public string Report = string.Empty;
    }

    internal static class TreeReferenceGalleryBuilder
    {
        internal const string VerticalSliceRootName =
            "Imported Reference Vertical Slice";
        internal const string CompleteGalleryRootName =
            "Complete Imported Gallery";
        internal const string CompleteGalleryShadowPadName =
            "Shadow Receiver Pad";

        private const string CreateMenuPath =
            "GameObject/PS3D/Trees/Tree Reference Gallery (Standalone)";
        private const string GalleryObjectName = "Tree Reference Gallery";
        private const string MaterialRootPath =
            "Assets/Game/Demo/Materials/Trees";
        private const string BarkShaderName =
            "PS3D/Trees/Stylized Tree Bark";
        private const string FoliageShaderName =
            "PS3D/Trees/Stylized Tree Foliage";

        internal const string CommonBarkMaterialPath =
            MaterialRootPath + "/MAT_TreeBark_CommonPine.mat";
        internal const string TwistedBarkMaterialPath =
            MaterialRootPath + "/MAT_TreeBark_Twisted.mat";
        internal const string DeadBarkMaterialPath =
            MaterialRootPath + "/MAT_TreeBark_Dead.mat";
        private const string CommonFoliageMaterialPath =
            MaterialRootPath + "/MAT_TreeFoliage_Common.mat";
        private const string PineFoliageMaterialPath =
            MaterialRootPath + "/MAT_TreeFoliage_Pine.mat";
        private const string TwistedFoliageMaterialPath =
            MaterialRootPath + "/MAT_TreeFoliage_Twisted.mat";
        private const string GalleryShadowPadMaterialPath =
            MaterialRootPath + "/MAT_TreeGallery_ShadowPad.mat";
        private const string GalleryShadowPadShaderName =
            "Universal Render Pipeline/Lit";

        private static readonly int WindEnabledId =
            Shader.PropertyToID("_TreeWindEnabled");
        private static readonly int WindMaskModeId =
            Shader.PropertyToID("_TreeWindMaskMode");
        private static readonly int BoundsMinYId =
            Shader.PropertyToID("_TreeBoundsMinY");
        private static readonly int BoundsHeightId =
            Shader.PropertyToID("_TreeBoundsHeight");
        private static readonly int RootPositionOsId =
            Shader.PropertyToID("_TreeRootPositionOS");
        private static readonly int StiffnessId =
            Shader.PropertyToID("_TreeStiffness");
        private static readonly int MacroWindStrengthId =
            Shader.PropertyToID("_TreeMacroWindStrength");
        private static readonly int FoliageFlutterStrengthId =
            Shader.PropertyToID("_TreeFoliageFlutterStrength");
        private static readonly int PhaseId =
            Shader.PropertyToID("_TreePhase");
        private static readonly int DebugModeId =
            Shader.PropertyToID("_TreeDebugMode");

        private static readonly SourceSpec[] VerticalSliceSpecs =
        {
            new SourceSpec(
                TreeFamily.Common,
                1,
                "CommonTree_1.fbx",
                new FamilyResponse(0.35f, 0.65f, 0.040f, 0.17f)),
            new SourceSpec(
                TreeFamily.Pine,
                5,
                "Pine_5.fbx",
                new FamilyResponse(0.65f, 0.45f, 0.025f, 0.39f)),
            new SourceSpec(
                TreeFamily.Twisted,
                1,
                "TwistedTree_1.fbx",
                new FamilyResponse(0.45f, 0.55f, 0.030f, 0.61f)),
            new SourceSpec(
                TreeFamily.Dead,
                1,
                "DeadTree_1.fbx",
                new FamilyResponse(0.85f, 0.15f, 0.000f, 0.83f))
        };


        private static readonly SourceSpec[] CompleteGallerySpecs =
        {
            new SourceSpec(TreeFamily.Common, 1, "CommonTree_1.fbx", new FamilyResponse(0.35f, 0.65f, 0.040f, 0.11f)),
            new SourceSpec(TreeFamily.Common, 2, "CommonTree_2.fbx", new FamilyResponse(0.35f, 0.65f, 0.040f, 0.23f)),
            new SourceSpec(TreeFamily.Common, 3, "CommonTree_3.fbx", new FamilyResponse(0.35f, 0.65f, 0.040f, 0.37f)),
            new SourceSpec(TreeFamily.Common, 4, "CommonTree_4.fbx", new FamilyResponse(0.35f, 0.65f, 0.040f, 0.53f)),
            new SourceSpec(TreeFamily.Common, 5, "CommonTree_5.fbx", new FamilyResponse(0.35f, 0.65f, 0.040f, 0.71f)),
            new SourceSpec(TreeFamily.Pine, 1, "Pine_1.fbx", new FamilyResponse(0.65f, 0.45f, 0.025f, 0.17f)),
            new SourceSpec(TreeFamily.Pine, 2, "Pine_2.fbx", new FamilyResponse(0.65f, 0.45f, 0.025f, 0.31f)),
            new SourceSpec(TreeFamily.Pine, 3, "Pine_3.fbx", new FamilyResponse(0.65f, 0.45f, 0.025f, 0.47f)),
            new SourceSpec(TreeFamily.Pine, 4, "Pine_4.fbx", new FamilyResponse(0.65f, 0.45f, 0.025f, 0.63f)),
            new SourceSpec(TreeFamily.Pine, 5, "Pine_5.fbx", new FamilyResponse(0.65f, 0.45f, 0.025f, 0.79f)),
            new SourceSpec(TreeFamily.Twisted, 1, "TwistedTree_1.fbx", new FamilyResponse(0.45f, 0.55f, 0.030f, 0.13f)),
            new SourceSpec(TreeFamily.Twisted, 2, "TwistedTree_2.fbx", new FamilyResponse(0.45f, 0.55f, 0.030f, 0.29f)),
            new SourceSpec(TreeFamily.Twisted, 3, "TwistedTree_3.fbx", new FamilyResponse(0.45f, 0.55f, 0.030f, 0.43f)),
            new SourceSpec(TreeFamily.Twisted, 4, "TwistedTree_4.fbx", new FamilyResponse(0.45f, 0.55f, 0.030f, 0.59f)),
            new SourceSpec(TreeFamily.Twisted, 5, "TwistedTree_5.fbx", new FamilyResponse(0.45f, 0.55f, 0.030f, 0.73f)),
            new SourceSpec(TreeFamily.Dead, 1, "DeadTree_1.fbx", new FamilyResponse(0.85f, 0.15f, 0.000f, 0.19f)),
            new SourceSpec(TreeFamily.Dead, 2, "DeadTree_2.fbx", new FamilyResponse(0.85f, 0.15f, 0.000f, 0.33f)),
            new SourceSpec(TreeFamily.Dead, 3, "DeadTree_3.fbx", new FamilyResponse(0.85f, 0.15f, 0.000f, 0.49f)),
            new SourceSpec(TreeFamily.Dead, 4, "DeadTree_4.fbx", new FamilyResponse(0.85f, 0.15f, 0.000f, 0.67f)),
            new SourceSpec(TreeFamily.Dead, 5, "DeadTree_5.fbx", new FamilyResponse(0.85f, 0.15f, 0.000f, 0.83f))
        };

        private static readonly NormalImportSpec[] BarkNormalImports =
        {
            new NormalImportSpec(
                "Dead bark normal",
                TreeReferenceGallery.SourceRootPath +
                "/Bark_DeadTree_Normal.png"),
            new NormalImportSpec(
                "Common/Pine bark normal",
                TreeReferenceGallery.SourceRootPath +
                "/Bark_NormalTree_Normal.png"),
            new NormalImportSpec(
                "Twisted bark normal",
                TreeReferenceGallery.SourceRootPath +
                "/Bark_TwistedTree_Normal.png")
        };

        [MenuItem(CreateMenuPath, false, 20)]
        private static void CreateStandaloneGallery()
        {
            GeneratedGround selectedGround = ResolveGroundFromSelection();
            var galleryObject = new GameObject(GalleryObjectName);
            Undo.RegisterCreatedObjectUndo(
                galleryObject,
                "Create Standalone Tree Reference Gallery");

            var gallery = Undo.AddComponent<TreeReferenceGallery>(galleryObject);
            if (selectedGround != null)
            {
                PlaceAsGroundSibling(gallery, selectedGround, true);
            }

            Selection.activeGameObject = galleryObject;
            EditorGUIUtility.PingObject(galleryObject);
            MarkSceneDirty(galleryObject);
        }

        internal static bool AssignClosestGround(
            TreeReferenceGallery gallery,
            out string result)
        {
            result = string.Empty;
            if (gallery == null)
            {
                result = "Tree Reference Gallery is missing.";
                return false;
            }

            GeneratedGround[] grounds =
                UnityEngine.Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);
            if (grounds.Length == 0)
            {
                result = "No GeneratedGround exists in the loaded scene.";
                return false;
            }

            GeneratedGround closest = null;
            float closestDistanceSquared = float.PositiveInfinity;
            Vector3 galleryPosition = gallery.transform.position;
            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround candidate = grounds[index];
                if (candidate == null ||
                    candidate.gameObject.scene != gallery.gameObject.scene)
                {
                    continue;
                }

                float distanceSquared =
                    (candidate.transform.position - galleryPosition)
                    .sqrMagnitude;
                if (distanceSquared >= closestDistanceSquared)
                {
                    continue;
                }

                closest = candidate;
                closestDistanceSquared = distanceSquared;
            }

            if (closest == null)
            {
                result =
                    "No GeneratedGround exists in the Tree Reference Gallery scene.";
                return false;
            }

            Undo.RecordObject(gallery, "Assign Closest Tree Reference Ground");
            gallery.SetReferenceGround(closest);
            EditorUtility.SetDirty(gallery);
            MarkSceneDirty(gallery.gameObject);
            result =
                $"Assigned closest Ground: {GetHierarchyPath(closest.transform)}";
            return true;
        }

        internal static bool PlaceBesideAssignedGround(
            TreeReferenceGallery gallery,
            out string result)
        {
            result = string.Empty;
            if (gallery == null)
            {
                result = "Tree Reference Gallery is missing.";
                return false;
            }

            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null)
            {
                result = "Assign a Reference Ground first.";
                return false;
            }

            if (ground.gameObject.scene != gallery.gameObject.scene)
            {
                result =
                    "The gallery and assigned Ground must be in the same scene.";
                return false;
            }

            PlaceAsGroundSibling(gallery, ground, false);
            result =
                "Placed the gallery as an independent sibling/root object beside " +
                GetHierarchyPath(ground.transform) + ".";
            return true;
        }

        internal static bool HasVerticalSlice(TreeReferenceGallery gallery)
        {
            return ResolveVerticalSliceRoot(gallery) != null;
        }

        internal static bool HasCompleteGallery(TreeReferenceGallery gallery)
        {
            return ResolveCompleteGalleryRoot(gallery) != null;
        }

        internal static bool BarkNormalCorrectionsRequired(out string summary)
        {
            var required = new List<string>();
            for (int index = 0; index < BarkNormalImports.Length; index++)
            {
                NormalImportSpec spec = BarkNormalImports[index];
                var importer = AssetImporter.GetAtPath(spec.Path) as TextureImporter;
                if (importer == null)
                {
                    required.Add(spec.Label + " importer missing");
                    continue;
                }

                if (importer.textureType != TextureImporterType.NormalMap ||
                    importer.sRGBTexture)
                {
                    required.Add(spec.Label);
                }
            }

            summary = required.Count == 0
                ? "All audited bark-normal importers are correct."
                : string.Join(", ", required);
            return required.Count > 0;
        }

        internal static TreeGalleryBuildResult ApplyBarkNormalCorrections()
        {
            var builder = new StringBuilder(2048);
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            builder.AppendLine(
                "[TREE-GALLERY.2B Bark Normal Import Normalization]");
            builder.Append("Generated: ").AppendLine(timestamp);
            builder.AppendLine(
                "Mutation: only textureType and sRGBTexture on the three audited bark-normal importers");
            builder.AppendLine();

            bool passed = ApplyBarkNormalCorrections(builder);
            builder.AppendLine();
            builder.Append("Status: ").AppendLine(passed ? "PASS" : "FAIL");
            return new TreeGalleryBuildResult
            {
                Passed = passed,
                SpecimenCount = 0,
                Timestamp = timestamp,
                Report = builder.ToString()
            };
        }

        internal static TreeGalleryBuildResult BuildVerticalSlice(
            TreeReferenceGallery gallery,
            bool rebuild)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(16384);
            report.AppendLine(
                "[TREE-GALLERY.2B Four-Family Vertical Slice Build]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.Append("Mode: ").AppendLine(rebuild ? "Rebuild" : "Build");
            report.AppendLine(
                "Mutation: three audited bark-normal importers, six deterministic shared material assets with reusable foliage readability/shadow settings, and the builder-owned gallery child root only");
            report.AppendLine();

            if (!ValidateBuildPrerequisites(gallery, rebuild, report))
            {
                AppendBuildSummary(report, false, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!ApplyBarkNormalCorrections(report))
            {
                AppendBuildSummary(report, false, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!TryEnsureSharedMaterials(
                    gallery,
                    report,
                    out MaterialSet materials))
            {
                AppendBuildSummary(report, false, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!TryResolveVerticalSliceLayout(
                    gallery,
                    report,
                    out Vector2[] familyPairCentres))
            {
                AppendBuildSummary(report, false, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            Transform existingRoot = ResolveVerticalSliceRoot(gallery);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot.gameObject);
            }

            GameObject sliceRoot = null;
            int specimenCount = 0;
            try
            {
                sliceRoot = CreateChild(
                    gallery.transform,
                    VerticalSliceRootName);
                sliceRoot.transform.localPosition = Vector3.zero;
                sliceRoot.transform.localRotation = Quaternion.identity;
                sliceRoot.transform.localScale = Vector3.one;

                report.AppendLine();
                report.AppendLine("[Environment]");
                AppendEnvironmentStatus(report);
                report.AppendLine();
                report.AppendLine("[Specimens]");

                for (int index = 0;
                     index < VerticalSliceSpecs.Length;
                     index++)
                {
                    SourceSpec spec = VerticalSliceSpecs[index];
                    if (!TryBuildSpecimenPair(
                            gallery,
                            sliceRoot.transform,
                            spec,
                            familyPairCentres[index],
                            materials,
                            report,
                            out int createdSpecimens,
                            out string failure))
                    {
                        report.Append("FAIL | ")
                            .Append(spec.Family)
                            .Append(" | ")
                            .AppendLine(failure);
                        Undo.DestroyObjectImmediate(sliceRoot);
                        AppendBuildSummary(report, false, 0);
                        return CreateResult(false, 0, timestamp, report);
                    }

                    specimenCount += createdSpecimens;
                }

                AssetDatabase.SaveAssets();
                MarkSceneDirty(gallery.gameObject);
                Selection.activeGameObject = sliceRoot;
                EditorGUIUtility.PingObject(sliceRoot);
                AppendBuildSummary(report, true, specimenCount);
                return CreateResult(
                    true,
                    specimenCount,
                    timestamp,
                    report);
            }
            catch (Exception exception)
            {
                if (sliceRoot != null)
                {
                    Undo.DestroyObjectImmediate(sliceRoot);
                }

                report.AppendLine();
                report.Append("FAIL | Exception: ")
                    .Append(exception.GetType().Name)
                    .Append(" | ")
                    .AppendLine(exception.Message);
                AppendBuildSummary(report, false, 0);
                return CreateResult(false, 0, timestamp, report);
            }
        }

        internal static TreeGalleryBuildResult RemoveVerticalSlice(
            TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(1024);
            report.AppendLine(
                "[TREE-GALLERY.2B Remove Four-Family Vertical Slice]");
            report.Append("Generated: ").AppendLine(timestamp);

            Transform root = ResolveVerticalSliceRoot(gallery);
            if (root == null)
            {
                report.AppendLine("Status: PASS");
                report.AppendLine("Result: no builder-owned vertical slice existed.");
                return CreateResult(true, 0, timestamp, report);
            }

            Undo.DestroyObjectImmediate(root.gameObject);
            MarkSceneDirty(gallery.gameObject);
            report.AppendLine("Status: PASS");
            report.AppendLine(
                "Result: removed the builder-owned vertical-slice root only.");
            return CreateResult(true, 0, timestamp, report);
        }


        internal static TreeGalleryBuildResult BuildCompleteGallery(
            TreeReferenceGallery gallery,
            bool rebuild)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(32768);
            report.AppendLine(
                "[TREE-GALLERY.3A Simultaneous Complete Imported Reference Gallery Build]");
            report.Append("Generated: ").AppendLine(timestamp);
            report.Append("Mode: ").AppendLine(rebuild ? "Rebuild" : "Build");
            report.AppendLine(
                "Mutation: three audited bark-normal importers, seven deterministic shared material assets, and the builder-owned complete-gallery root only");
            report.AppendLine(
                "Placement: all complete-gallery family blocks are outside the playable Ground domain to its left; all four blocks remain active simultaneously; blocks are arranged progressively farther left and each owns a lightweight shadow receiver pad");
            report.AppendLine();

            if (!ValidateCompleteGalleryPrerequisites(
                    gallery,
                    rebuild,
                    report))
            {
                AppendCompleteGallerySummary(report, false, 0, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!ApplyBarkNormalCorrections(report))
            {
                AppendCompleteGallerySummary(report, false, 0, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!TryEnsureSharedMaterials(
                    gallery,
                    report,
                    out MaterialSet materials) ||
                !TryEnsureGalleryShadowPadMaterial(
                    gallery,
                    report,
                    out Material shadowPadMaterial))
            {
                AppendCompleteGallerySummary(report, false, 0, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            if (!TryResolveCompleteGalleryLayouts(
                    gallery,
                    report,
                    out CompleteFamilyLayout[] layouts))
            {
                AppendCompleteGallerySummary(report, false, 0, 0);
                return CreateResult(false, 0, timestamp, report);
            }

            Transform existingRoot = ResolveCompleteGalleryRoot(gallery);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot.gameObject);
            }

            GameObject completeRoot = null;
            int specimenCount = 0;
            int importedTriangleCount = 0;
            try
            {
                completeRoot = CreateChild(
                    gallery.transform,
                    CompleteGalleryRootName);
                completeRoot.transform.localPosition = Vector3.zero;
                completeRoot.transform.localRotation = Quaternion.identity;
                completeRoot.transform.localScale = Vector3.one;

                report.AppendLine();
                report.AppendLine("[Environment]");
                AppendEnvironmentStatus(report);
                report.AppendLine();
                report.AppendLine("[Simultaneous Family Blocks]");

                for (int layoutIndex = 0;
                     layoutIndex < layouts.Length;
                     layoutIndex++)
                {
                    CompleteFamilyLayout layout = layouts[layoutIndex];
                    if (!TryBuildCompleteFamilyPage(
                            gallery,
                            completeRoot.transform,
                            layout,
                            materials,
                            shadowPadMaterial,
                            report,
                            out int createdSpecimens,
                            out int createdTriangles,
                            out string failure))
                    {
                        report.Append("FAIL | ")
                            .Append(layout.Family)
                            .Append(" | ")
                            .AppendLine(failure);
                        Undo.DestroyObjectImmediate(completeRoot);
                        AppendCompleteGallerySummary(report, false, 0, 0);
                        return CreateResult(false, 0, timestamp, report);
                    }

                    specimenCount += createdSpecimens;
                    importedTriangleCount += createdTriangles;
                }

                report.AppendLine();
                report.AppendLine(
                    "PASS | All four family blocks are active simultaneously; no family cycling state is required.");
                AssetDatabase.SaveAssets();
                MarkSceneDirty(gallery.gameObject);
                Selection.activeGameObject = completeRoot;
                EditorGUIUtility.PingObject(completeRoot);
                AppendCompleteGallerySummary(
                    report,
                    true,
                    specimenCount,
                    importedTriangleCount);
                return CreateResult(
                    true,
                    specimenCount,
                    timestamp,
                    report);
            }
            catch (Exception exception)
            {
                if (completeRoot != null)
                {
                    Undo.DestroyObjectImmediate(completeRoot);
                }

                report.AppendLine();
                report.Append("FAIL | Exception: ")
                    .Append(exception.GetType().Name)
                    .Append(" | ")
                    .AppendLine(exception.Message);
                AppendCompleteGallerySummary(report, false, 0, 0);
                return CreateResult(false, 0, timestamp, report);
            }
        }

        internal static TreeGalleryBuildResult RemoveCompleteGallery(
            TreeReferenceGallery gallery)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var report = new StringBuilder(1024);
            report.AppendLine(
                "[TREE-GALLERY.3A Remove Complete Imported Gallery]");
            report.Append("Generated: ").AppendLine(timestamp);

            Transform root = ResolveCompleteGalleryRoot(gallery);
            if (root == null)
            {
                report.AppendLine("Status: PASS");
                report.AppendLine(
                    "Result: no builder-owned complete gallery existed.");
                return CreateResult(true, 0, timestamp, report);
            }

            Undo.DestroyObjectImmediate(root.gameObject);
            MarkSceneDirty(gallery.gameObject);
            report.AppendLine("Status: PASS");
            report.AppendLine(
                "Result: removed the builder-owned complete-gallery root only.");
            return CreateResult(true, 0, timestamp, report);
        }

        private static bool ValidateBuildPrerequisites(
            TreeReferenceGallery gallery,
            bool rebuild,
            StringBuilder report)
        {
            report.AppendLine("[Prerequisites]");
            if (gallery == null)
            {
                report.AppendLine("FAIL | Tree Reference Gallery is missing.");
                return false;
            }

            if (!gallery.LastSourceAuditPassed)
            {
                report.AppendLine(
                    "FAIL | The complete source audit has not passed on this gallery.");
                return false;
            }

            if (!AssetDatabase.IsValidFolder(
                    TreeReferenceGallery.SourceRootPath))
            {
                report.Append("FAIL | Source folder is unavailable: ")
                    .AppendLine(TreeReferenceGallery.SourceRootPath);
                return false;
            }

            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null)
            {
                report.AppendLine("FAIL | Reference Ground is not assigned.");
                return false;
            }

            if (ground.gameObject.scene != gallery.gameObject.scene)
            {
                report.AppendLine(
                    "FAIL | Gallery and Reference Ground are not in the same scene.");
                return false;
            }

            if (gallery.transform.IsChildOf(ground.transform))
            {
                report.AppendLine(
                    "FAIL | Gallery is incorrectly parented under its Reference Ground.");
                return false;
            }

            bool exists = HasVerticalSlice(gallery);
            if (exists && !rebuild)
            {
                report.AppendLine(
                    "FAIL | A vertical slice already exists. Use Rebuild or Remove.");
                return false;
            }

            report.Append("PASS | Gallery: ")
                .AppendLine(GetHierarchyPath(gallery.transform));
            report.Append("PASS | Reference Ground: ")
                .AppendLine(GetHierarchyPath(ground.transform));
            report.Append("PASS | Existing slice: ")
                .AppendLine(exists ? "Yes" : "No");
            return true;
        }

        private static bool ApplyBarkNormalCorrections(
            StringBuilder report)
        {
            report.AppendLine();
            report.AppendLine("[Bark Normal Import Corrections]");
            bool passed = true;
            for (int index = 0; index < BarkNormalImports.Length; index++)
            {
                NormalImportSpec spec = BarkNormalImports[index];
                var importer = AssetImporter.GetAtPath(spec.Path)
                    as TextureImporter;
                if (importer == null)
                {
                    passed = false;
                    report.Append("FAIL | ")
                        .Append(spec.Label)
                        .Append(" | importer missing | ")
                        .AppendLine(spec.Path);
                    continue;
                }

                bool changed = false;
                if (importer.textureType != TextureImporterType.NormalMap)
                {
                    importer.textureType = TextureImporterType.NormalMap;
                    changed = true;
                }

                if (importer.sRGBTexture)
                {
                    importer.sRGBTexture = false;
                    changed = true;
                }

                if (changed)
                {
                    importer.SaveAndReimport();
                }

                TextureImporter verified = AssetImporter.GetAtPath(spec.Path)
                    as TextureImporter;
                bool correct =
                    verified != null &&
                    verified.textureType == TextureImporterType.NormalMap &&
                    !verified.sRGBTexture;
                passed &= correct;
                report.Append(correct ? "PASS | " : "FAIL | ")
                    .Append(spec.Label)
                    .Append(" | ")
                    .Append(changed ? "Corrected" : "Already correct")
                    .Append(" | type=")
                    .Append(verified != null
                        ? verified.textureType.ToString()
                        : "Missing")
                    .Append(" sRGB=")
                    .AppendLine(
                        verified != null && verified.sRGBTexture
                            ? "Yes"
                            : "No");
            }

            return passed;
        }

        private static bool TryEnsureSharedMaterials(
            TreeReferenceGallery gallery,
            StringBuilder report,
            out MaterialSet materials)
        {
            materials = default;
            report.AppendLine();
            report.AppendLine("[Shared Reference Materials]");

            Shader barkShader = Shader.Find(BarkShaderName);
            Shader foliageShader = Shader.Find(FoliageShaderName);
            if (barkShader == null || foliageShader == null)
            {
                report.Append("FAIL | Bark shader: ")
                    .Append(barkShader != null ? "Found" : "Missing")
                    .Append(" | Foliage shader: ")
                    .AppendLine(foliageShader != null ? "Found" : "Missing");
                return false;
            }

            if (!EnsureAssetFolder(MaterialRootPath, out string folderError))
            {
                report.Append("FAIL | ").AppendLine(folderError);
                return false;
            }

            Texture2D commonBark = LoadTexture(
                "Bark_NormalTree.png",
                report);
            Texture2D commonBarkNormal = LoadTexture(
                "Bark_NormalTree_Normal.png",
                report);
            Texture2D twistedBark = LoadTexture(
                "Bark_TwistedTree.png",
                report);
            Texture2D twistedBarkNormal = LoadTexture(
                "Bark_TwistedTree_Normal.png",
                report);
            Texture2D deadBark = LoadTexture(
                "Bark_DeadTree.png",
                report);
            Texture2D deadBarkNormal = LoadTexture(
                "Bark_DeadTree_Normal.png",
                report);
            Texture2D commonFoliage = LoadTexture(
                "Leaves_NormalTree_C.png",
                report);
            Texture2D pineFoliage = LoadTexture(
                "Leaf_Pine_C.png",
                report);
            Texture2D twistedFoliage = LoadTexture(
                "Leaves_TwistedTree_C.png",
                report);

            if (commonBark == null || commonBarkNormal == null ||
                twistedBark == null || twistedBarkNormal == null ||
                deadBark == null || deadBarkNormal == null ||
                commonFoliage == null || pineFoliage == null ||
                twistedFoliage == null)
            {
                report.AppendLine(
                    "FAIL | One or more required source textures failed to load.");
                return false;
            }

            materials = new MaterialSet
            {
                CommonBark = CreateOrUpdateBarkMaterial(
                    CommonBarkMaterialPath,
                    "MAT_TreeBark_CommonPine",
                    barkShader,
                    commonBark,
                    commonBarkNormal),
                TwistedBark = CreateOrUpdateBarkMaterial(
                    TwistedBarkMaterialPath,
                    "MAT_TreeBark_Twisted",
                    barkShader,
                    twistedBark,
                    twistedBarkNormal),
                DeadBark = CreateOrUpdateBarkMaterial(
                    DeadBarkMaterialPath,
                    "MAT_TreeBark_Dead",
                    barkShader,
                    deadBark,
                    deadBarkNormal),
                CommonFoliage = CreateOrUpdateFoliageMaterial(
                    CommonFoliageMaterialPath,
                    "MAT_TreeFoliage_Common",
                    foliageShader,
                    commonFoliage,
                    gallery),
                PineFoliage = CreateOrUpdateFoliageMaterial(
                    PineFoliageMaterialPath,
                    "MAT_TreeFoliage_Pine",
                    foliageShader,
                    pineFoliage,
                    gallery),
                TwistedFoliage = CreateOrUpdateFoliageMaterial(
                    TwistedFoliageMaterialPath,
                    "MAT_TreeFoliage_Twisted",
                    foliageShader,
                    twistedFoliage,
                    gallery)
            };

            bool complete = materials.IsComplete;
            report.Append(complete ? "PASS | " : "FAIL | ")
                .Append("Bark shader=")
                .Append(BarkShaderName)
                .Append(" | Foliage shader=")
                .AppendLine(FoliageShaderName);
            report.Append("Foliage ShadowCaster pass: ")
                .AppendLine(
                    gallery.FoliageShadowCasting ? "Enabled" : "Disabled");
            report.Append("Foliage alpha cutoff: ")
                .AppendLine(gallery.FoliageAlphaCutoff.ToString("F3"));
            report.Append("Foliage readability: canopyDepth=")
                .Append(gallery.FoliageCanopyDepthStrength.ToString("F3"))
                .Append(" power=")
                .Append(gallery.FoliageCanopyDepthPower.ToString("F3"))
                .Append(" orientationContrast=")
                .Append(gallery.FoliageOrientationContrast.ToString("F3"))
                .Append(" orientationReadability=")
                .Append(gallery.FoliageOrientationReadability.ToString("F3"))
                .Append(" underside=")
                .Append(gallery.FoliageUndersideDarkening.ToString("F3"))
                .Append(" clusterVariation=")
                .Append(gallery.FoliageClusterVariationStrength.ToString("F3"))
                .Append(" clusterScale=")
                .Append(gallery.FoliageClusterVariationScale.ToString("F3"))
                .Append(" diffuseWrap=")
                .AppendLine(gallery.FoliageDiffuseWrap.ToString("F3"));
            report.Append("Foliage realtime shadow reception: strength=")
                .Append(gallery.FoliageShadowReceiveStrength.ToString("F3"))
                .Append(" floor=")
                .AppendLine(gallery.FoliageShadowFloor.ToString("F3"));
            report.Append("Foliage diagnostic view: ")
                .AppendLine(gallery.FoliageDebugMode.ToString());
            return complete;
        }

        private static Texture2D LoadTexture(
            string filename,
            StringBuilder report)
        {
            string path = TreeReferenceGallery.SourceRootPath + "/" + filename;
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            report.Append(texture != null ? "PASS | " : "FAIL | ")
                .Append("Texture | ")
                .AppendLine(path);
            return texture;
        }


        internal static Material LoadSharedBarkMaterial(TreeFamily family)
        {
            string path;
            switch (family)
            {
                case TreeFamily.Twisted:
                    path = TwistedBarkMaterialPath;
                    break;
                case TreeFamily.Dead:
                    path = DeadBarkMaterialPath;
                    break;
                default:
                    path = CommonBarkMaterialPath;
                    break;
            }

            return AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        private static Material CreateOrUpdateBarkMaterial(
            string path,
            string name,
            Shader shader,
            Texture2D albedo,
            Texture2D normal)
        {
            Material material = LoadOrCreateMaterial(path, name, shader);
            material.SetTexture("_BaseMap", albedo);
            material.SetTexture("_BumpMap", normal);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_BumpScale", 1f);
            material.SetShaderPassEnabled("ShadowCaster", true);
            material.enableInstancing = true;
            material.doubleSidedGI = false;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material CreateOrUpdateFoliageMaterial(
            string path,
            string name,
            Shader shader,
            Texture2D texture,
            TreeReferenceGallery gallery)
        {
            Material material = LoadOrCreateMaterial(path, name, shader);
            material.SetTexture("_BaseMap", texture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Cutoff", gallery.FoliageAlphaCutoff);
            material.SetFloat(
                "_CanopyDepthStrength",
                gallery.FoliageCanopyDepthStrength);
            material.SetFloat(
                "_CanopyDepthPower",
                gallery.FoliageCanopyDepthPower);
            material.SetFloat(
                "_OrientationContrast",
                gallery.FoliageOrientationContrast);
            material.SetFloat(
                "_OrientationReadability",
                gallery.FoliageOrientationReadability);
            material.SetFloat(
                "_UndersideDarkening",
                gallery.FoliageUndersideDarkening);
            material.SetFloat(
                "_ClusterVariationStrength",
                gallery.FoliageClusterVariationStrength);
            material.SetFloat(
                "_ClusterVariationScale",
                gallery.FoliageClusterVariationScale);
            material.SetFloat(
                "_DiffuseWrap",
                gallery.FoliageDiffuseWrap);
            material.SetFloat(
                "_ShadowReceiveStrength",
                gallery.FoliageShadowReceiveStrength);
            material.SetFloat(
                "_ShadowFloor",
                gallery.FoliageShadowFloor);
            material.SetFloat(
                "_FoliageDebugMode",
                (float)gallery.FoliageDebugMode);
            material.SetShaderPassEnabled(
                "ShadowCaster",
                gallery.FoliageShadowCasting);
            material.enableInstancing = true;
            material.doubleSidedGI = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material LoadOrCreateMaterial(
            string path,
            string name,
            Shader shader)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = name
                };
                AssetDatabase.CreateAsset(material, path);
                return material;
            }

            Undo.RecordObject(material, "Update Shared Tree Reference Material");
            material.shader = shader;
            material.name = name;
            return material;
        }


        private static bool ValidateCompleteGalleryPrerequisites(
            TreeReferenceGallery gallery,
            bool rebuild,
            StringBuilder report)
        {
            report.AppendLine("[Prerequisites]");
            if (gallery == null)
            {
                report.AppendLine("FAIL | Tree Reference Gallery is missing.");
                return false;
            }

            if (!gallery.LastSourceAuditPassed)
            {
                report.AppendLine(
                    "FAIL | The complete source audit has not passed on this gallery.");
                return false;
            }

            if (!AssetDatabase.IsValidFolder(
                    TreeReferenceGallery.SourceRootPath))
            {
                report.Append("FAIL | Source folder is unavailable: ")
                    .AppendLine(TreeReferenceGallery.SourceRootPath);
                return false;
            }

            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null)
            {
                report.AppendLine(
                    "FAIL | Reference Ground is required only to locate the playable chunk boundary and orient the off-map gallery zone.");
                return false;
            }

            if (ground.gameObject.scene != gallery.gameObject.scene)
            {
                report.AppendLine(
                    "FAIL | Gallery and Reference Ground are not in the same scene.");
                return false;
            }

            if (gallery.transform.IsChildOf(ground.transform))
            {
                report.AppendLine(
                    "FAIL | Gallery is incorrectly parented under its Reference Ground.");
                return false;
            }

            if (!ground.TryGetSurfaceDomain(out _, out float domainSize))
            {
                report.AppendLine(
                    "FAIL | Reference Ground surface domain is unavailable, so the builder cannot prove that the complete gallery is outside the playable chunk.");
                return false;
            }

            bool exists = HasCompleteGallery(gallery);
            if (exists && !rebuild)
            {
                report.AppendLine(
                    "FAIL | A complete imported gallery already exists. Use Rebuild or Remove.");
                return false;
            }

            report.Append("PASS | Gallery: ")
                .AppendLine(GetHierarchyPath(gallery.transform));
            report.Append("PASS | Reference Ground: ")
                .AppendLine(GetHierarchyPath(ground.transform));
            report.Append("PASS | Playable Ground domain: ")
                .Append(domainSize.ToString("F3"))
                .AppendLine(" m");
            report.Append("PASS | Existing complete gallery: ")
                .AppendLine(exists ? "Yes" : "No");
            return true;
        }

        private static bool TryEnsureGalleryShadowPadMaterial(
            TreeReferenceGallery gallery,
            StringBuilder report,
            out Material material)
        {
            material = null;
            report.AppendLine();
            report.AppendLine("[Off-Map Shadow Receiver Material]");
            Shader shader = Shader.Find(GalleryShadowPadShaderName);
            if (shader == null)
            {
                report.Append("FAIL | Shader missing: ")
                    .AppendLine(GalleryShadowPadShaderName);
                return false;
            }

            if (!EnsureAssetFolder(MaterialRootPath, out string folderError))
            {
                report.Append("FAIL | ").AppendLine(folderError);
                return false;
            }

            material = LoadOrCreateMaterial(
                GalleryShadowPadMaterialPath,
                "MAT_TreeGallery_ShadowPad",
                shader);
            material.SetColor("_BaseColor", gallery.CompleteGalleryPadColor);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0f);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            report.Append("PASS | ")
                .Append(GalleryShadowPadMaterialPath)
                .Append(" | shader=")
                .Append(GalleryShadowPadShaderName)
                .Append(" | colour=")
                .AppendLine(gallery.CompleteGalleryPadColor.ToString());
            return true;
        }

        private static bool TryResolveCompleteGalleryLayouts(
            TreeReferenceGallery gallery,
            StringBuilder report,
            out CompleteFamilyLayout[] layouts)
        {
            layouts = Array.Empty<CompleteFamilyLayout>();
            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null ||
                !ground.TryGetSurfaceDomain(
                    out float halfSize,
                    out float domainSize))
            {
                report.AppendLine();
                report.AppendLine("[Off-Map Gallery Layout]");
                report.AppendLine(
                    "FAIL | Reference Ground surface domain is unavailable.");
                return false;
            }

            const int FamilyCount = 4;
            const int VariantsPerFamily = 5;
            layouts = new CompleteFamilyLayout[FamilyCount];
            Vector3 groundLeftEdgeWorld = ground.transform.TransformPoint(
                new Vector3(-halfSize, 0f, 0f));
            Vector3 leftDirection = -ground.transform.right.normalized;
            float nextPageRightEdgeClearance =
                gallery.CompleteGalleryLeftClearance;

            report.AppendLine();
            report.AppendLine("[Off-Map Gallery Layout]");
            report.Append("PASS | Playable domain=")
                .Append(domainSize.ToString("F3"))
                .Append(" m | left clearance=")
                .Append(gallery.CompleteGalleryLeftClearance.ToString("F3"))
                .Append(" m | family gap=")
                .Append(gallery.CompleteGalleryFamilyGap.ToString("F3"))
                .Append(" m | all four family blocks are active and arranged progressively farther left")
                .AppendLine();

            for (int familyIndex = 0;
                 familyIndex < FamilyCount;
                 familyIndex++)
            {
                TreeFamily family = (TreeFamily)familyIndex;
                var specs = new SourceSpec[VariantsPerFamily];
                var metrics = new SourceMetrics[VariantsPerFamily];
                var pairCentres = new Vector2[VariantsPerFamily];
                var pairSeparations = new float[VariantsPerFamily];
                var rowRadii = new float[VariantsPerFamily];
                var pairMinimumX = new float[VariantsPerFamily];
                var pairMaximumX = new float[VariantsPerFamily];
                int triangleTotal = 0;
                float minimumHeight = float.PositiveInfinity;
                float maximumHeight = 0f;
                float minimumWidth = float.PositiveInfinity;
                float maximumWidth = 0f;

                for (int variantIndex = 0;
                     variantIndex < VariantsPerFamily;
                     variantIndex++)
                {
                    SourceSpec spec = CompleteGallerySpecs[
                        familyIndex * VariantsPerFamily + variantIndex];
                    specs[variantIndex] = spec;
                    string sourcePath = TreeReferenceGallery.SourceRootPath +
                        "/" + spec.Filename;
                    GameObject sourceAsset =
                        AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                    if (sourceAsset == null)
                    {
                        report.Append("FAIL | ")
                            .Append(family)
                            .Append(" ")
                            .Append(spec.VariantIndex)
                            .AppendLine(
                                " layout metrics unavailable: source model failed to load.");
                        layouts = Array.Empty<CompleteFamilyLayout>();
                        return false;
                    }

                    if (!TryInspectSourceAsset(
                            sourceAsset,
                            family,
                            out SourceMetrics sourceMetrics,
                            out string failure))
                    {
                        report.Append("FAIL | ")
                            .Append(family)
                            .Append(" ")
                            .Append(spec.VariantIndex)
                            .Append(" layout metrics unavailable: ")
                            .AppendLine(failure);
                        layouts = Array.Empty<CompleteFamilyLayout>();
                        return false;
                    }

                    metrics[variantIndex] = sourceMetrics;
                    float scale = gallery.SourceScale;
                    float requiredSeparation = Mathf.Max(
                        gallery.PairColumnSpacing,
                        sourceMetrics.CanopyWidth * scale +
                        gallery.ComparisonPairOffset);
                    pairSeparations[variantIndex] = requiredSeparation;
                    float halfSeparation = requiredSeparation * 0.5f;
                    pairMinimumX[variantIndex] = Mathf.Min(
                        -halfSeparation + sourceMetrics.Bounds.min.x * scale,
                        halfSeparation + sourceMetrics.Bounds.min.x * scale);
                    pairMaximumX[variantIndex] = Mathf.Max(
                        -halfSeparation + sourceMetrics.Bounds.max.x * scale,
                        halfSeparation + sourceMetrics.Bounds.max.x * scale);
                    rowRadii[variantIndex] = Mathf.Max(
                        Mathf.Abs(sourceMetrics.Bounds.min.z),
                        Mathf.Abs(sourceMetrics.Bounds.max.z)) * scale;
                    triangleTotal += sourceMetrics.TriangleCount;
                    minimumHeight = Mathf.Min(
                        minimumHeight,
                        sourceMetrics.Bounds.size.y);
                    maximumHeight = Mathf.Max(
                        maximumHeight,
                        sourceMetrics.Bounds.size.y);
                    minimumWidth = Mathf.Min(
                        minimumWidth,
                        sourceMetrics.CanopyWidth);
                    maximumWidth = Mathf.Max(
                        maximumWidth,
                        sourceMetrics.CanopyWidth);
                }

                pairCentres[0] = Vector2.zero;
                for (int rowIndex = 1;
                     rowIndex < VariantsPerFamily;
                     rowIndex++)
                {
                    float distance = rowRadii[rowIndex - 1] +
                        rowRadii[rowIndex] +
                        gallery.CompleteGalleryRowGap;
                    pairCentres[rowIndex] = new Vector2(
                        0f,
                        pairCentres[rowIndex - 1].y + distance);
                }

                float minimumZ = pairCentres[0].y - rowRadii[0];
                float maximumZ = pairCentres[VariantsPerFamily - 1].y +
                    rowRadii[VariantsPerFamily - 1];
                float centreShiftZ = (minimumZ + maximumZ) * 0.5f;
                for (int rowIndex = 0;
                     rowIndex < VariantsPerFamily;
                     rowIndex++)
                {
                    pairCentres[rowIndex].y -= centreShiftZ;
                }

                float pageMinimumX = pairMinimumX[0];
                float pageMaximumX = pairMaximumX[0];
                minimumZ = pairCentres[0].y - rowRadii[0];
                maximumZ = pairCentres[0].y + rowRadii[0];
                for (int rowIndex = 1;
                     rowIndex < VariantsPerFamily;
                     rowIndex++)
                {
                    pageMinimumX = Mathf.Min(
                        pageMinimumX,
                        pairMinimumX[rowIndex]);
                    pageMaximumX = Mathf.Max(
                        pageMaximumX,
                        pairMaximumX[rowIndex]);
                    minimumZ = Mathf.Min(
                        minimumZ,
                        pairCentres[rowIndex].y - rowRadii[rowIndex]);
                    maximumZ = Mathf.Max(
                        maximumZ,
                        pairCentres[rowIndex].y + rowRadii[rowIndex]);
                }

                Bounds pageBounds = new Bounds();
                pageBounds.SetMinMax(
                    new Vector3(pageMinimumX, 0f, minimumZ),
                    new Vector3(pageMaximumX, maximumHeight, maximumZ));
                Bounds padBounds = pageBounds;
                padBounds.Expand(
                    new Vector3(
                        gallery.CompleteGalleryPadMargin * 2f,
                        0f,
                        gallery.CompleteGalleryPadMargin * 2f));

                float pageRightEdge = padBounds.max.x;
                float pageRightEdgeClearance = nextPageRightEdgeClearance;
                Vector3 pageWorldPosition = groundLeftEdgeWorld +
                    leftDirection *
                    (pageRightEdgeClearance + pageRightEdge);
                layouts[familyIndex] = new CompleteFamilyLayout(
                    family,
                    specs,
                    metrics,
                    pairCentres,
                    pairSeparations,
                    pageBounds,
                    padBounds,
                    pageWorldPosition,
                    ground.transform.rotation,
                    triangleTotal,
                    minimumHeight,
                    maximumHeight,
                    minimumWidth,
                    maximumWidth);

                Vector3 padRightEdgeWorld = pageWorldPosition +
                    ground.transform.right.normalized * padBounds.max.x;
                float provedClearance = Vector3.Dot(
                    groundLeftEdgeWorld - padRightEdgeWorld,
                    ground.transform.right.normalized);
                report.Append("PASS | ")
                    .Append(family)
                    .Append(" | pageSize=")
                    .Append(pageBounds.size.x.ToString("F3"))
                    .Append("x")
                    .Append(pageBounds.size.z.ToString("F3"))
                    .Append(" m | padSize=")
                    .Append(padBounds.size.x.ToString("F3"))
                    .Append("x")
                    .Append(padBounds.size.z.ToString("F3"))
                    .Append(" m | rightEdgeClearance=")
                    .Append(pageRightEdgeClearance.ToString("F3"))
                    .Append(" m | provedChunkClearance=")
                    .Append(provedClearance.ToString("F3"))
                    .Append(" m | heightRange=")
                    .Append(minimumHeight.ToString("F3"))
                    .Append("..")
                    .Append(maximumHeight.ToString("F3"))
                    .Append(" | widthRange=")
                    .Append(minimumWidth.ToString("F3"))
                    .Append("..")
                    .Append(maximumWidth.ToString("F3"))
                    .Append(" | triangles=")
                    .AppendLine(triangleTotal.ToString());

                nextPageRightEdgeClearance +=
                    padBounds.size.x + gallery.CompleteGalleryFamilyGap;
            }

            report.Append("PASS | Total simultaneous gallery strip reaches ")
                .Append((nextPageRightEdgeClearance - gallery.CompleteGalleryFamilyGap).ToString("F3"))
                .AppendLine(" m left of the playable Ground edge.");
            return true;
        }

        private static bool TryBuildCompleteFamilyPage(
            TreeReferenceGallery gallery,
            Transform completeRoot,
            CompleteFamilyLayout layout,
            MaterialSet materials,
            Material shadowPadMaterial,
            StringBuilder report,
            out int createdSpecimens,
            out int createdTriangles,
            out string failure)
        {
            createdSpecimens = 0;
            createdTriangles = 0;
            failure = string.Empty;

            GameObject pageRoot = CreateChild(
                completeRoot,
                GetFamilyPageName(layout.Family));
            pageRoot.transform.SetPositionAndRotation(
                layout.PageWorldPosition,
                layout.PageWorldRotation);
            pageRoot.transform.localScale = Vector3.one;
            CreateCompleteGalleryShadowPad(
                pageRoot.transform,
                layout.PadBounds,
                gallery.CompleteGalleryPadThickness,
                shadowPadMaterial);

            report.Append("PAGE | ")
                .Append(layout.Family)
                .Append(" | worldOrigin=")
                .Append(layout.PageWorldPosition.ToString("F3"))
                .AppendLine(" | activeAfterBuild=Yes");

            for (int index = 0; index < layout.Specs.Length; index++)
            {
                if (!TryBuildOffMapSpecimenPair(
                        gallery,
                        pageRoot.transform,
                        layout.Specs[index],
                        layout.Metrics[index],
                        layout.PairCentres[index],
                        layout.PairSeparations[index],
                        materials,
                        report,
                        out int pairSpecimenCount,
                        out string pairFailure))
                {
                    failure = pairFailure;
                    return false;
                }

                createdSpecimens += pairSpecimenCount;
                createdTriangles += layout.Metrics[index].TriangleCount;
            }

            report.Append("PASS | ")
                .Append(layout.Family)
                .Append(" block | specimens/slots=")
                .Append(createdSpecimens)
                .Append(" | importedTriangles=")
                .Append(createdTriangles)
                .Append(" | shadowPad=")
                .Append(layout.PadBounds.size.x.ToString("F3"))
                .Append("x")
                .Append(layout.PadBounds.size.z.ToString("F3"))
                .AppendLine(" m");
            return true;
        }

        private static void CreateCompleteGalleryShadowPad(
            Transform pageRoot,
            Bounds padBounds,
            float thickness,
            Material material)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(
                pad,
                "Create Tree Gallery Shadow Receiver Pad");
            pad.name = CompleteGalleryShadowPadName;
            pad.transform.SetParent(pageRoot, false);
            pad.transform.localPosition = new Vector3(
                padBounds.center.x,
                -thickness * 0.5f,
                padBounds.center.z);
            pad.transform.localRotation = Quaternion.identity;
            pad.transform.localScale = new Vector3(
                Mathf.Max(0.01f, padBounds.size.x),
                thickness,
                Mathf.Max(0.01f, padBounds.size.z));

            Collider collider = pad.GetComponent<Collider>();
            if (collider != null)
            {
                Undo.DestroyObjectImmediate(collider);
            }

            Renderer renderer = pad.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }

        private static bool TryBuildOffMapSpecimenPair(
            TreeReferenceGallery gallery,
            Transform pageRoot,
            SourceSpec spec,
            SourceMetrics metrics,
            Vector2 pairCentre,
            float pairSeparation,
            MaterialSet materials,
            StringBuilder report,
            out int createdSpecimens,
            out string failure)
        {
            createdSpecimens = 0;
            failure = string.Empty;
            string sourcePath = TreeReferenceGallery.SourceRootPath +
                "/" + spec.Filename;
            GameObject sourceAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (sourceAsset == null)
            {
                failure = "Source model failed to load: " + sourcePath;
                return false;
            }

            Material[] assignedMaterials =
                materials.ResolveForFamily(spec.Family);
            if (assignedMaterials == null)
            {
                failure = "No shared material mapping exists for " +
                    spec.Family + ".";
                return false;
            }

            float halfSeparation = pairSeparation * 0.5f;
            float baseCorrection =
                -metrics.Bounds.min.y * gallery.SourceScale;
            GameObject pairRoot = CreateChild(
                pageRoot,
                spec.Family + "_" + spec.VariantIndex + "_Pair");
            pairRoot.transform.localPosition = new Vector3(
                pairCentre.x,
                0f,
                pairCentre.y);
            pairRoot.transform.localRotation = Quaternion.identity;
            pairRoot.transform.localScale = Vector3.one;

            GameObject imported = PrefabUtility.InstantiatePrefab(
                    sourceAsset,
                    pairRoot.transform) as GameObject;
            if (imported == null)
            {
                failure = "PrefabUtility failed to instantiate " + sourcePath;
                return false;
            }

            Undo.RegisterCreatedObjectUndo(
                imported,
                "Create Complete Imported Tree Reference");
            imported.name = "REF_" + sourceAsset.name;
            imported.transform.localPosition = new Vector3(
                -halfSeparation,
                baseCorrection,
                0f);
            imported.transform.localRotation = Quaternion.identity;
            imported.transform.localScale =
                Vector3.one * gallery.SourceScale;

            Renderer[] renderers = imported.GetComponentsInChildren<Renderer>(
                true);
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                renderer.sharedMaterials = assignedMaterials;
                renderer.receiveShadows = true;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                ApplyTreePropertyBlock(
                    renderer,
                    gallery,
                    metrics,
                    spec.Response);
            }

            var importedSpecimen =
                Undo.AddComponent<TreeReferenceSpecimen>(imported);
            importedSpecimen.Configure(
                spec.Family,
                spec.VariantIndex,
                TreeReferenceRole.ImportedReference,
                sourcePath,
                AssetDatabase.AssetPathToGUID(sourcePath),
                metrics.Bounds,
                metrics.Bounds.min.y,
                baseCorrection,
                new Vector3(0f, metrics.Bounds.min.y, 0f),
                metrics.RendererCount,
                metrics.SubmeshCount,
                metrics.VertexCount,
                metrics.TriangleCount,
                metrics.MaterialLayout,
                BuildRenderingSummary(assignedMaterials));

            GameObject proceduralSlot = CreateChild(
                pairRoot.transform,
                "PROC_" + sourceAsset.name + "_SLOT");
            proceduralSlot.transform.localPosition = new Vector3(
                halfSeparation,
                0f,
                0f);
            var slotSpecimen = Undo.AddComponent<TreeReferenceSpecimen>(
                proceduralSlot);
            slotSpecimen.Configure(
                spec.Family,
                spec.VariantIndex,
                TreeReferenceRole.ProceduralComparison,
                sourcePath,
                AssetDatabase.AssetPathToGUID(sourcePath),
                metrics.Bounds,
                metrics.Bounds.min.y,
                0f,
                Vector3.zero,
                metrics.RendererCount,
                metrics.SubmeshCount,
                metrics.VertexCount,
                metrics.TriangleCount,
                metrics.MaterialLayout,
                "Curated recipe-spawned procedural comparison slot");

            if (!TreeCuratedGalleryUtility.TryConfigureSpawner(
                    gallery,
                    slotSpecimen,
                    out _,
                    out string spawnerFailure))
            {
                report.Append("  WARNING | Curated spawner not configured: ")
                    .AppendLine(spawnerFailure);
            }

            createdSpecimens = 2;
            report.Append("  PASS | ")
                .Append(spec.Family)
                .Append(" ")
                .Append(spec.VariantIndex)
                .Append(" | model=")
                .Append(sourceAsset.name)
                .Append(" | pairCentre=")
                .Append(pairCentre.ToString("F3"))
                .Append(" | pairSeparation=")
                .Append(pairSeparation.ToString("F3"))
                .Append(" | baseCorrection=")
                .Append(baseCorrection.ToString("F5"))
                .Append(" | height=")
                .Append(metrics.Bounds.size.y.ToString("F4"))
                .Append(" | width=")
                .Append(metrics.CanopyWidth.ToString("F4"))
                .Append(" | triangles=")
                .AppendLine(metrics.TriangleCount.ToString());
            return true;
        }

        private static bool TryResolveVerticalSliceLayout(
            TreeReferenceGallery gallery,
            StringBuilder report,
            out Vector2[] familyPairCentres)
        {
            const float DomainEdgeInset = 0.5f;
            const float MinimumCanopyGap = 0.75f;

            familyPairCentres = Array.Empty<Vector2>();
            GeneratedGround ground = gallery.ReferenceGround;
            if (ground == null ||
                !ground.TryGetSurfaceDomain(
                    out float halfSize,
                    out float domainSize))
            {
                report.AppendLine();
                report.AppendLine("[Gallery Layout]");
                report.AppendLine(
                    "FAIL | Reference Ground surface domain is unavailable.");
                return false;
            }

            int familyCount = VerticalSliceSpecs.Length;
            var rowRadii = new float[familyCount];
            var pairCentreOffsetsX = new float[familyCount];
            var importedMinimumX = new float[familyCount];
            var importedMaximumX = new float[familyCount];
            for (int index = 0; index < familyCount; index++)
            {
                SourceSpec spec = VerticalSliceSpecs[index];
                string sourcePath = TreeReferenceGallery.SourceRootPath +
                    "/" + spec.Filename;
                GameObject sourceAsset =
                    AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (sourceAsset == null)
                {
                    report.AppendLine();
                    report.AppendLine("[Gallery Layout]");
                    report.Append("FAIL | ")
                        .Append(spec.Family)
                        .AppendLine(
                            " layout metrics unavailable: source model failed to load.");
                    return false;
                }

                if (!TryInspectSourceAsset(
                        sourceAsset,
                        spec.Family,
                        out SourceMetrics metrics,
                        out string failure))
                {
                    report.AppendLine();
                    report.AppendLine("[Gallery Layout]");
                    report.Append("FAIL | ")
                        .Append(spec.Family)
                        .Append(" layout metrics unavailable: ")
                        .AppendLine(failure);
                    return false;
                }

                float sourceScale = gallery.SourceScale;
                float requiredSeparation = Mathf.Max(
                    gallery.PairColumnSpacing,
                    metrics.CanopyWidth * sourceScale +
                    gallery.ComparisonPairOffset);
                float maximumAbsoluteZ = Mathf.Max(
                    Mathf.Abs(metrics.Bounds.min.z),
                    Mathf.Abs(metrics.Bounds.max.z)) * sourceScale;

                rowRadii[index] = maximumAbsoluteZ;

                float halfSeparation = requiredSeparation * 0.5f;
                float domainMinimum = -halfSize + DomainEdgeInset;
                float domainMaximum = halfSize - DomainEdgeInset;
                float lowerCentreLimit = Mathf.Max(
                    domainMinimum + halfSeparation,
                    domainMinimum + halfSeparation -
                    metrics.Bounds.min.x * sourceScale);
                float upperCentreLimit = Mathf.Min(
                    domainMaximum - halfSeparation,
                    domainMaximum + halfSeparation -
                    metrics.Bounds.max.x * sourceScale);
                if (lowerCentreLimit > upperCentreLimit)
                {
                    report.AppendLine();
                    report.AppendLine("[Gallery Layout]");
                    report.Append("FAIL | ")
                        .Append(spec.Family)
                        .Append(" imported reference and comparison root cannot fit across the Ground domain. Required centre interval=")
                        .Append(lowerCentreLimit.ToString("F3"))
                        .Append("..")
                        .Append(upperCentreLimit.ToString("F3"))
                        .AppendLine(".");
                    return false;
                }

                float pairCentreX = Mathf.Clamp(
                    0f,
                    lowerCentreLimit,
                    upperCentreLimit);
                pairCentreOffsetsX[index] = pairCentreX;
                importedMinimumX[index] = pairCentreX - halfSeparation +
                    metrics.Bounds.min.x * sourceScale;
                importedMaximumX[index] = pairCentreX - halfSeparation +
                    metrics.Bounds.max.x * sourceScale;
            }

            var minimumDistances = new float[familyCount - 1];
            var preferredDistances = new float[familyCount - 1];
            float minimumDistanceSum = 0f;
            float preferredExtraSum = 0f;
            for (int index = 0; index < familyCount - 1; index++)
            {
                float minimumDistance = rowRadii[index] +
                    rowRadii[index + 1] + MinimumCanopyGap;
                float preferredDistance = Mathf.Max(
                    minimumDistance,
                    gallery.FamilyRowSpacing);
                minimumDistances[index] = minimumDistance;
                preferredDistances[index] = preferredDistance;
                minimumDistanceSum += minimumDistance;
                preferredExtraSum += preferredDistance - minimumDistance;
            }

            float availableCentreSpan = domainSize -
                rowRadii[0] - rowRadii[familyCount - 1] -
                DomainEdgeInset * 2f;
            if (availableCentreSpan < minimumDistanceSum)
            {
                report.AppendLine();
                report.AppendLine("[Gallery Layout]");
                report.Append("FAIL | Four-family rows require at least ")
                    .Append(minimumDistanceSum.ToString("F3"))
                    .Append(" m of centre span, but the Ground provides ")
                    .Append(availableCentreSpan.ToString("F3"))
                    .AppendLine(" m after canopy and edge insets.");
                return false;
            }

            float availableExtra = availableCentreSpan - minimumDistanceSum;
            float extraScale = preferredExtraSum > 0.0001f
                ? Mathf.Clamp01(availableExtra / preferredExtraSum)
                : 0f;
            var actualDistances = new float[familyCount - 1];
            float actualDistanceSum = 0f;
            for (int index = 0; index < actualDistances.Length; index++)
            {
                float preferredExtra =
                    preferredDistances[index] - minimumDistances[index];
                actualDistances[index] = minimumDistances[index] +
                    preferredExtra * extraScale;
                actualDistanceSum += actualDistances[index];
            }

            float usedSpan = rowRadii[0] + actualDistanceSum +
                rowRadii[familyCount - 1];
            float start = -halfSize + DomainEdgeInset +
                (domainSize - DomainEdgeInset * 2f - usedSpan) * 0.5f +
                rowRadii[0];
            var familyRowOffsets = new float[familyCount];
            familyRowOffsets[0] = start;
            for (int index = 1; index < familyCount; index++)
            {
                familyRowOffsets[index] =
                    familyRowOffsets[index - 1] + actualDistances[index - 1];
            }

            familyPairCentres = new Vector2[familyCount];
            for (int index = 0; index < familyCount; index++)
            {
                familyPairCentres[index] = new Vector2(
                    pairCentreOffsetsX[index],
                    familyRowOffsets[index]);
            }

            report.AppendLine();
            report.AppendLine("[Gallery Layout]");
            report.Append("PASS | Ground domain=")
                .Append(domainSize.ToString("F3"))
                .Append(" m | halfSize=")
                .Append(halfSize.ToString("F3"))
                .Append(" m | preferredRowSpacing=")
                .Append(gallery.FamilyRowSpacing.ToString("F3"))
                .AppendLine(" m");
            for (int index = 0; index < familyCount; index++)
            {
                report.Append("PASS | ")
                    .Append(VerticalSliceSpecs[index].Family)
                    .Append(" | rowLocalZ=")
                    .Append(familyRowOffsets[index].ToString("F3"))
                    .Append(" | rowRadius=")
                    .Append(rowRadii[index].ToString("F3"))
                    .Append(" | pairCentreLocalX=")
                    .Append(pairCentreOffsetsX[index].ToString("F3"))
                    .Append(" | importedLocalX=")
                    .Append(importedMinimumX[index].ToString("F3"))
                    .Append("..")
                    .AppendLine(importedMaximumX[index].ToString("F3"));
            }

            return true;
        }

        private static bool TryBuildSpecimenPair(
            TreeReferenceGallery gallery,
            Transform sliceRoot,
            SourceSpec spec,
            Vector2 pairCentreLocalXZ,
            MaterialSet materials,
            StringBuilder report,
            out int createdSpecimens,
            out string failure)
        {
            createdSpecimens = 0;
            failure = string.Empty;
            string sourcePath = TreeReferenceGallery.SourceRootPath +
                "/" + spec.Filename;
            GameObject sourceAsset =
                AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (sourceAsset == null)
            {
                failure = "Source model failed to load: " + sourcePath;
                return false;
            }

            if (!TryInspectSourceAsset(
                    sourceAsset,
                    spec.Family,
                    out SourceMetrics metrics,
                    out failure))
            {
                return false;
            }

            Material[] assignedMaterials =
                materials.ResolveForFamily(spec.Family);
            if (assignedMaterials == null)
            {
                failure = "No shared material mapping exists for " +
                    spec.Family + ".";
                return false;
            }

            float requiredSeparation = Mathf.Max(
                gallery.PairColumnSpacing,
                metrics.CanopyWidth * gallery.SourceScale +
                gallery.ComparisonPairOffset);
            float halfSeparation = requiredSeparation * 0.5f;
            float groundCorrection =
                -metrics.Bounds.min.y * gallery.SourceScale;

            GeneratedGround referenceGround = gallery.ReferenceGround;
            Vector3 importedRootWorld = referenceGround.transform.TransformPoint(
                new Vector3(
                    pairCentreLocalXZ.x - halfSeparation,
                    0f,
                    pairCentreLocalXZ.y));
            Vector3 comparisonRootWorld =
                referenceGround.transform.TransformPoint(
                    new Vector3(
                        pairCentreLocalXZ.x + halfSeparation,
                        0f,
                        pairCentreLocalXZ.y));
            Vector3 importedGroundNormal = Vector3.up;
            Vector3 comparisonGroundNormal = Vector3.up;
            if (gallery.AlignToGround)
            {
                if (!referenceGround.TrySampleBaseSurface(
                        importedRootWorld,
                        out float importedGroundHeight,
                        out importedGroundNormal))
                {
                    failure =
                        "Reference Ground could not sample the imported-tree root at " +
                        importedRootWorld.ToString("F3") + ".";
                    return false;
                }

                if (!referenceGround.TrySampleBaseSurface(
                        comparisonRootWorld,
                        out float comparisonGroundHeight,
                        out comparisonGroundNormal))
                {
                    failure =
                        "Reference Ground could not sample the procedural-slot root at " +
                        comparisonRootWorld.ToString("F3") + ".";
                    return false;
                }

                importedRootWorld.y = importedGroundHeight;
                comparisonRootWorld.y = comparisonGroundHeight;
            }

            Vector3 pairCentreWorld =
                (importedRootWorld + comparisonRootWorld) * 0.5f;
            Quaternion pairRotation = Quaternion.Euler(
                0f,
                referenceGround.transform.eulerAngles.y,
                0f);

            GameObject familyRoot = CreateChild(
                sliceRoot,
                spec.Family.ToString());
            GameObject pairRoot = CreateChild(
                familyRoot.transform,
                spec.Family + "_VerticalSlice_Pair");
            pairRoot.transform.SetPositionAndRotation(
                pairCentreWorld,
                pairRotation);
            pairRoot.transform.localScale = Vector3.one;

            Vector3 importedRootLocal =
                pairRoot.transform.InverseTransformPoint(importedRootWorld);
            Vector3 comparisonRootLocal =
                pairRoot.transform.InverseTransformPoint(comparisonRootWorld);

            report.Append("Ground roots | ")
                .Append(spec.Family)
                .Append(" | rowLocalZ=")
                .Append(pairCentreLocalXZ.y.ToString("F3"))
                .Append(" pairCentreLocalX=")
                .Append(pairCentreLocalXZ.x.ToString("F3"))
                .Append(" importedHeight=")
                .Append(importedRootWorld.y.ToString("F4"))
                .Append(" importedNormal=")
                .Append(importedGroundNormal.ToString("F3"))
                .Append(" comparisonHeight=")
                .Append(comparisonRootWorld.y.ToString("F4"))
                .Append(" comparisonNormal=")
                .AppendLine(comparisonGroundNormal.ToString("F3"));

            GameObject imported = PrefabUtility.InstantiatePrefab(
                    sourceAsset,
                    pairRoot.transform) as GameObject;
            if (imported == null)
            {
                failure = "PrefabUtility failed to instantiate " + sourcePath;
                return false;
            }

            Undo.RegisterCreatedObjectUndo(
                imported,
                "Create Imported Tree Reference Specimen");
            imported.name = "REF_" + sourceAsset.name;
            imported.transform.localPosition =
                importedRootLocal + Vector3.up * groundCorrection;
            imported.transform.localRotation = Quaternion.identity;
            imported.transform.localScale =
                Vector3.one * gallery.SourceScale;

            Renderer[] renderers = imported.GetComponentsInChildren<Renderer>(
                true);
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                renderer.sharedMaterials = assignedMaterials;
                renderer.receiveShadows = true;
                renderer.shadowCastingMode = ShadowCastingMode.On;
                ApplyTreePropertyBlock(
                    renderer,
                    gallery,
                    metrics,
                    spec.Response);
            }

            var importedSpecimen =
                Undo.AddComponent<TreeReferenceSpecimen>(imported);
            importedSpecimen.Configure(
                spec.Family,
                spec.VariantIndex,
                TreeReferenceRole.ImportedReference,
                sourcePath,
                AssetDatabase.AssetPathToGUID(sourcePath),
                metrics.Bounds,
                metrics.Bounds.min.y,
                groundCorrection,
                new Vector3(0f, metrics.Bounds.min.y, 0f),
                metrics.RendererCount,
                metrics.SubmeshCount,
                metrics.VertexCount,
                metrics.TriangleCount,
                metrics.MaterialLayout,
                BuildRenderingSummary(assignedMaterials));

            GameObject proceduralSlot = CreateChild(
                pairRoot.transform,
                "PROC_" + sourceAsset.name + "_SLOT");
            proceduralSlot.transform.localPosition =
                comparisonRootLocal;
            var slotSpecimen = Undo.AddComponent<TreeReferenceSpecimen>(
                proceduralSlot);
            slotSpecimen.Configure(
                spec.Family,
                spec.VariantIndex,
                TreeReferenceRole.ProceduralComparison,
                sourcePath,
                AssetDatabase.AssetPathToGUID(sourcePath),
                metrics.Bounds,
                metrics.Bounds.min.y,
                0f,
                Vector3.zero,
                metrics.RendererCount,
                metrics.SubmeshCount,
                metrics.VertexCount,
                metrics.TriangleCount,
                metrics.MaterialLayout,
                "Curated recipe-spawned procedural comparison slot");

            if (!TreeCuratedGalleryUtility.TryConfigureSpawner(
                    gallery,
                    slotSpecimen,
                    out _,
                    out string spawnerFailure))
            {
                report.Append("  WARNING | Curated spawner not configured: ")
                    .AppendLine(spawnerFailure);
            }

            createdSpecimens = 2;
            report.Append("PASS | ")
                .Append(spec.Family)
                .Append(" ")
                .Append(spec.VariantIndex)
                .Append(" | model=")
                .Append(sourceAsset.name)
                .Append(" | height=")
                .Append(metrics.Bounds.size.y.ToString("F4"))
                .Append(" | width=")
                .Append(metrics.CanopyWidth.ToString("F4"))
                .Append(" | triangles=")
                .Append(metrics.TriangleCount)
                .Append(" | lowestY=")
                .Append(metrics.Bounds.min.y.ToString("F5"))
                .Append(" | correction=")
                .Append(groundCorrection.ToString("F5"))
                .Append(" | pairSeparation=")
                .Append(requiredSeparation.ToString("F3"))
                .Append(" | materials=")
                .AppendLine(BuildRenderingSummary(assignedMaterials));
            return true;
        }

        private static void ApplyTreePropertyBlock(
            Renderer renderer,
            TreeReferenceGallery gallery,
            SourceMetrics metrics,
            FamilyResponse response)
        {
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat(WindEnabledId, gallery.WindEnabled ? 1f : 0f);
            block.SetFloat(
                WindMaskModeId,
                (float)gallery.ImportedWindMaskMode);
            block.SetFloat(BoundsMinYId, metrics.Bounds.min.y);
            block.SetFloat(
                BoundsHeightId,
                Mathf.Max(0.0001f, metrics.Bounds.size.y));
            block.SetVector(
                RootPositionOsId,
                new Vector4(0f, metrics.Bounds.min.y, 0f, 1f));
            block.SetFloat(StiffnessId, response.Stiffness);
            block.SetFloat(MacroWindStrengthId, response.MacroStrength);
            block.SetFloat(
                FoliageFlutterStrengthId,
                response.FoliageFlutter);
            block.SetFloat(PhaseId, response.Phase);
            block.SetFloat(DebugModeId, (float)gallery.DebugMode);
            renderer.SetPropertyBlock(block);
        }

        private static bool TryInspectSourceAsset(
            GameObject sourceAsset,
            TreeFamily family,
            out SourceMetrics metrics,
            out string failure)
        {
            metrics = default;
            failure = string.Empty;
            Renderer[] renderers = sourceAsset.GetComponentsInChildren<Renderer>(
                true);
            if (renderers.Length != 1 ||
                !(renderers[0] is MeshRenderer))
            {
                failure =
                    "Expected exactly one MeshRenderer but found " +
                    renderers.Length + ".";
                return false;
            }

            MeshFilter filter = renderers[0].GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
            {
                failure = "Expected MeshFilter/sharedMesh is missing.";
                return false;
            }

            string[] expectedNames = ResolveExpectedSourceMaterials(family);
            Material[] sourceMaterials = renderers[0].sharedMaterials;
            if (sourceMaterials.Length != expectedNames.Length ||
                filter.sharedMesh.subMeshCount != expectedNames.Length)
            {
                failure =
                    "Submesh/material count differs from the accepted audit.";
                return false;
            }

            var materialNames = new StringBuilder();
            for (int index = 0; index < expectedNames.Length; index++)
            {
                string actual = sourceMaterials[index] != null
                    ? sourceMaterials[index].name
                    : "<Missing>";
                if (!string.Equals(
                        actual,
                        expectedNames[index],
                        StringComparison.Ordinal))
                {
                    failure =
                        "Material slot " + index + " expected " +
                        expectedNames[index] + " but found " + actual + ".";
                    return false;
                }

                if (index > 0)
                {
                    materialNames.Append(", ");
                }

                materialNames.Append(actual);
            }

            Bounds bounds = CalculateRootLocalBounds(
                sourceAsset.transform,
                renderers);
            Mesh mesh = filter.sharedMesh;
            int triangleCount = 0;
            for (int submesh = 0;
                 submesh < mesh.subMeshCount;
                 submesh++)
            {
                triangleCount += checked((int)mesh.GetIndexCount(submesh) / 3);
            }

            metrics = new SourceMetrics
            {
                Bounds = bounds,
                CanopyWidth = Mathf.Max(bounds.size.x, bounds.size.z),
                RendererCount = renderers.Length,
                SubmeshCount = mesh.subMeshCount,
                VertexCount = mesh.vertexCount,
                TriangleCount = triangleCount,
                MaterialLayout = "[" + materialNames + "]"
            };
            return true;
        }

        private static Bounds CalculateRootLocalBounds(
            Transform root,
            Renderer[] renderers)
        {
            bool initialized = false;
            Bounds combined = default;
            for (int rendererIndex = 0;
                 rendererIndex < renderers.Length;
                 rendererIndex++)
            {
                Renderer renderer = renderers[rendererIndex];
                Bounds localBounds = renderer.localBounds;
                Matrix4x4 toRoot =
                    root.worldToLocalMatrix * renderer.localToWorldMatrix;
                Vector3 minimum = localBounds.min;
                Vector3 maximum = localBounds.max;
                for (int corner = 0; corner < 8; corner++)
                {
                    Vector3 point = new Vector3(
                        (corner & 1) == 0 ? minimum.x : maximum.x,
                        (corner & 2) == 0 ? minimum.y : maximum.y,
                        (corner & 4) == 0 ? minimum.z : maximum.z);
                    Vector3 rootPoint = toRoot.MultiplyPoint3x4(point);
                    if (!initialized)
                    {
                        combined = new Bounds(rootPoint, Vector3.zero);
                        initialized = true;
                    }
                    else
                    {
                        combined.Encapsulate(rootPoint);
                    }
                }
            }

            return initialized
                ? combined
                : new Bounds(Vector3.zero, Vector3.zero);
        }

        private static string[] ResolveExpectedSourceMaterials(
            TreeFamily family)
        {
            switch (family)
            {
                case TreeFamily.Common:
                    return new[]
                    {
                        "Bark_NormalTree",
                        "Leaves_NormalTree"
                    };
                case TreeFamily.Pine:
                    return new[]
                    {
                        "Bark_NormalTree",
                        "Leaves_Pine"
                    };
                case TreeFamily.Twisted:
                    return new[]
                    {
                        "Bark_TwistedTree",
                        "Leaves_TwistedTree"
                    };
                case TreeFamily.Dead:
                    return new[]
                    {
                        "Bark_DeadTree"
                    };
                default:
                    return Array.Empty<string>();
            }
        }

        private static string BuildRenderingSummary(Material[] materials)
        {
            var builder = new StringBuilder();
            for (int index = 0; index < materials.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(", ");
                }

                Material material = materials[index];
                builder.Append(material != null ? material.name : "<Missing>")
                    .Append("/")
                    .Append(material != null && material.shader != null
                        ? material.shader.name
                        : "<Missing shader>");
            }

            return builder.ToString();
        }

        private static void AppendEnvironmentStatus(StringBuilder report)
        {
            WeatherWindDomain wind = WeatherWindDomain.PublishedDomain;
            report.Append("Weather wind domain: ")
                .AppendLine(wind != null
                    ? GetHierarchyPath(wind.transform)
                    : "None");
            report.Append("Weather wind resources ready: ")
                .AppendLine(
                    wind != null && wind.ResourcesReady ? "Yes" : "No");
            if (wind != null)
            {
                report.Append("Weather field size: ")
                    .Append(wind.FieldWorldSizeMetres.ToString("F2"))
                    .Append(" m | maximum visual bend: ")
                    .Append(wind.MaximumVisualBendMetres.ToString("F3"))
                    .AppendLine(" m");
            }

            WeatherCloudShadowController cloud =
                WeatherCloudShadowController.PublishedController;
            report.Append("Weather cloud controller: ")
                .AppendLine(cloud != null
                    ? GetHierarchyPath(cloud.transform)
                    : "None");
            report.Append("Cloud cookie ready: ")
                .AppendLine(
                    cloud != null && cloud.CookieReady ? "Yes" : "No");
            report.AppendLine(
                "Cloud shader contract: PS3D tree bark and foliage are mandatory _LIGHT_COOKIES receivers; no custom cloud texture is sampled.");
        }

        private static void AppendBuildSummary(
            StringBuilder report,
            bool passed,
            int specimenCount)
        {
            report.AppendLine();
            report.AppendLine("[Summary]");
            report.Append("Status: ").AppendLine(passed ? "PASS" : "FAIL");
            report.Append("Gallery specimens/slots: ")
                .AppendLine(specimenCount.ToString());
            report.AppendLine(
                passed
                    ? "Readiness: foliage shadow/readability, Weather-wind, cloud-cookie, root-placement, rebuild, and Play Mode validation are required before TREE-GALLERY.3."
                    : "Readiness: blocked until the reported TREE-GALLERY.2B failure is corrected.");
        }

        private static void AppendCompleteGallerySummary(
            StringBuilder report,
            bool passed,
            int specimenCount,
            int importedTriangleCount)
        {
            report.AppendLine();
            report.AppendLine("[Summary]");
            report.Append("Status: ").AppendLine(passed ? "PASS" : "FAIL");
            report.Append("Gallery specimens/slots: ")
                .AppendLine(specimenCount.ToString());
            report.Append("Imported source triangles across all pages: ")
                .AppendLine(importedTriangleCount.ToString());
            report.AppendLine(
                passed
                    ? "Readiness: validate the simultaneous off-map twenty-tree gallery, all four shadow pads, rebuild/removal, Weather wind, cloud shading, and Play Mode stability before TREE-GALLERY.FREEZE."
                    : "Readiness: blocked until the reported TREE-GALLERY.3A failure is corrected.");
        }

        private static TreeGalleryBuildResult CreateResult(
            bool passed,
            int specimenCount,
            string timestamp,
            StringBuilder report)
        {
            return new TreeGalleryBuildResult
            {
                Passed = passed,
                SpecimenCount = specimenCount,
                Timestamp = timestamp,
                Report = report.ToString()
            };
        }

        private static Transform ResolveCompleteGalleryRoot(
            TreeReferenceGallery gallery)
        {
            return gallery != null
                ? gallery.transform.Find(CompleteGalleryRootName)
                : null;
        }

        private static string GetFamilyPageName(TreeFamily family)
        {
            return family + " Page";
        }

        private static Transform ResolveVerticalSliceRoot(
            TreeReferenceGallery gallery)
        {
            return gallery != null
                ? gallery.transform.Find(VerticalSliceRootName)
                : null;
        }

        private static GameObject CreateChild(
            Transform parent,
            string name)
        {
            var child = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(
                child,
                "Create Tree Reference Gallery Content");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static bool EnsureAssetFolder(
            string path,
            out string error)
        {
            error = string.Empty;
            if (AssetDatabase.IsValidFolder(path))
            {
                return true;
            }

            string[] segments = path.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                error = "Material root is not under Assets: " + path;
                return false;
            }

            string current = "Assets";
            for (int index = 1; index < segments.Length; index++)
            {
                string next = current + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    string guid = AssetDatabase.CreateFolder(
                        current,
                        segments[index]);
                    if (string.IsNullOrEmpty(guid))
                    {
                        error = "Failed to create asset folder: " + next;
                        return false;
                    }
                }

                current = next;
            }

            return true;
        }

        private static GeneratedGround ResolveGroundFromSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                GeneratedGround direct = selected.GetComponent<GeneratedGround>();
                if (direct != null)
                {
                    return direct;
                }

                GeneratedGround ancestor =
                    selected.GetComponentInParent<GeneratedGround>(true);
                if (ancestor != null)
                {
                    return ancestor;
                }
            }

            GeneratedGround[] grounds =
                UnityEngine.Object.FindObjectsByType<GeneratedGround>(
                    FindObjectsInactive.Include);
            GeneratedGround onlyGround = null;
            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround candidate = grounds[index];
                if (candidate == null)
                {
                    continue;
                }

                if (onlyGround != null)
                {
                    return null;
                }

                onlyGround = candidate;
            }

            return onlyGround;
        }

        private static void PlaceAsGroundSibling(
            TreeReferenceGallery gallery,
            GeneratedGround ground,
            bool assignGround)
        {
            Transform galleryTransform = gallery.transform;
            Transform groundTransform = ground.transform;
            Transform intendedParent = groundTransform.parent;

            Undo.SetTransformParent(
                galleryTransform,
                intendedParent,
                "Place Tree Reference Gallery Beside Ground");
            Undo.RecordObject(
                galleryTransform,
                "Align Tree Reference Gallery With Ground");
            galleryTransform.SetPositionAndRotation(
                groundTransform.position,
                groundTransform.rotation);
            galleryTransform.localScale = Vector3.one;

            int targetSiblingIndex = groundTransform.GetSiblingIndex() + 1;
            galleryTransform.SetSiblingIndex(targetSiblingIndex);

            if (assignGround)
            {
                Undo.RecordObject(gallery, "Assign Tree Reference Ground");
                gallery.SetReferenceGround(ground);
                EditorUtility.SetDirty(gallery);
            }

            MarkSceneDirty(gallery.gameObject);
        }

        private static void MarkSceneDirty(GameObject gameObject)
        {
            if (gameObject != null && gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        internal static string GetHierarchyPath(Transform transform)
        {
            if (transform == null)
            {
                return "None";
            }

            string path = transform.name;
            Transform current = transform.parent;
            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        private readonly struct CompleteFamilyLayout
        {
            public CompleteFamilyLayout(
                TreeFamily family,
                SourceSpec[] specs,
                SourceMetrics[] metrics,
                Vector2[] pairCentres,
                float[] pairSeparations,
                Bounds pageBounds,
                Bounds padBounds,
                Vector3 pageWorldPosition,
                Quaternion pageWorldRotation,
                int importedTriangleCount,
                float minimumHeight,
                float maximumHeight,
                float minimumWidth,
                float maximumWidth)
            {
                Family = family;
                Specs = specs;
                Metrics = metrics;
                PairCentres = pairCentres;
                PairSeparations = pairSeparations;
                PageBounds = pageBounds;
                PadBounds = padBounds;
                PageWorldPosition = pageWorldPosition;
                PageWorldRotation = pageWorldRotation;
                ImportedTriangleCount = importedTriangleCount;
                MinimumHeight = minimumHeight;
                MaximumHeight = maximumHeight;
                MinimumWidth = minimumWidth;
                MaximumWidth = maximumWidth;
            }

            public TreeFamily Family { get; }
            public SourceSpec[] Specs { get; }
            public SourceMetrics[] Metrics { get; }
            public Vector2[] PairCentres { get; }
            public float[] PairSeparations { get; }
            public Bounds PageBounds { get; }
            public Bounds PadBounds { get; }
            public Vector3 PageWorldPosition { get; }
            public Quaternion PageWorldRotation { get; }
            public int ImportedTriangleCount { get; }
            public float MinimumHeight { get; }
            public float MaximumHeight { get; }
            public float MinimumWidth { get; }
            public float MaximumWidth { get; }
        }

        private readonly struct SourceSpec
        {
            public SourceSpec(
                TreeFamily family,
                int variantIndex,
                string filename,
                FamilyResponse response)
            {
                Family = family;
                VariantIndex = variantIndex;
                Filename = filename;
                Response = response;
            }

            public TreeFamily Family { get; }
            public int VariantIndex { get; }
            public string Filename { get; }
            public FamilyResponse Response { get; }
        }

        private readonly struct FamilyResponse
        {
            public FamilyResponse(
                float stiffness,
                float macroStrength,
                float foliageFlutter,
                float phase)
            {
                Stiffness = stiffness;
                MacroStrength = macroStrength;
                FoliageFlutter = foliageFlutter;
                Phase = phase;
            }

            public float Stiffness { get; }
            public float MacroStrength { get; }
            public float FoliageFlutter { get; }
            public float Phase { get; }
        }

        private readonly struct NormalImportSpec
        {
            public NormalImportSpec(string label, string path)
            {
                Label = label;
                Path = path;
            }

            public string Label { get; }
            public string Path { get; }
        }

        private struct SourceMetrics
        {
            public Bounds Bounds;
            public float CanopyWidth;
            public int RendererCount;
            public int SubmeshCount;
            public int VertexCount;
            public int TriangleCount;
            public string MaterialLayout;
        }

        private struct MaterialSet
        {
            public Material CommonBark;
            public Material TwistedBark;
            public Material DeadBark;
            public Material CommonFoliage;
            public Material PineFoliage;
            public Material TwistedFoliage;

            public bool IsComplete =>
                CommonBark != null &&
                TwistedBark != null &&
                DeadBark != null &&
                CommonFoliage != null &&
                PineFoliage != null &&
                TwistedFoliage != null;

            public Material[] ResolveForFamily(TreeFamily family)
            {
                switch (family)
                {
                    case TreeFamily.Common:
                        return new[] { CommonBark, CommonFoliage };
                    case TreeFamily.Pine:
                        return new[] { CommonBark, PineFoliage };
                    case TreeFamily.Twisted:
                        return new[] { TwistedBark, TwistedFoliage };
                    case TreeFamily.Dead:
                        return new[] { DeadBark };
                    default:
                        return null;
                }
            }
        }
    }
}
