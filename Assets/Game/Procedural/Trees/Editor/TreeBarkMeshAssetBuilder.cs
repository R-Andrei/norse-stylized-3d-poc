using System;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Trees.Editor
{
    internal static class TreeBarkMeshAssetBuilder
    {
        internal const string GeneratedBarkChildName = "Generated Bark Mesh";

        private static readonly int BaseColorId =
            Shader.PropertyToID("_BaseColor");
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

        internal static bool BuildOrUpdate(
            TreeReferenceGallery gallery,
            TreeGenerationLibrary library,
            ProceduralTreeInstance instance,
            out TreeBarkMeshBuildResult buildResult,
            out string report,
            out string failure)
        {
            buildResult = null;
            report = string.Empty;
            failure = string.Empty;
            if (gallery == null || library == null || instance == null)
            {
                failure = "Gallery, generation library, or procedural instance is null.";
                return false;
            }

            if (!instance.HasGeneratedDefinition)
            {
                failure = "Procedural instance has no valid structural definition.";
                return false;
            }

            Material material = TreeReferenceGalleryBuilder.LoadSharedBarkMaterial(
                instance.Family);
            if (material == null)
            {
                failure =
                    "Shared bark material is missing for " + instance.Family + ".";
                return false;
            }

            Mesh mesh = FindOrCreateManagedMesh(library, instance);
            if (mesh == null)
            {
                failure = "Managed bark mesh asset could not be created.";
                return false;
            }

            TreeBarkMeshSettings settings =
                TreeBarkMeshSettings.CreateVerticalSliceDefaults(
                    instance.Family);
            buildResult = TreeBarkMeshGenerator.Build(
                instance.GeneratedDefinition,
                settings,
                mesh);
            if (!buildResult.Passed)
            {
                return FailBuild(
                    instance,
                    mesh,
                    buildResult,
                    buildResult.Failure,
                    out failure);
            }

            if (!ValidateRepeatableBuild(
                    instance.GeneratedDefinition,
                    settings,
                    buildResult,
                    out string repeatabilityFailure))
            {
                return FailBuild(
                    instance,
                    mesh,
                    buildResult,
                    repeatabilityFailure,
                    out failure);
            }

            buildResult.MarkRepeatabilityPassed();
            GameObject barkObject = EnsureBarkChild(instance);
            MeshFilter filter = barkObject.GetComponent<MeshFilter>();
            if (filter == null)
            {
                filter = Undo.AddComponent<MeshFilter>(barkObject);
            }

            MeshRenderer renderer = barkObject.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                renderer = Undo.AddComponent<MeshRenderer>(barkObject);
            }

            filter.sharedMesh = mesh;
            renderer.sharedMaterial = material;
            renderer.receiveShadows = true;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.BlendProbes;
            ApplyPropertyBlock(gallery, instance, renderer);

            report = BuildReport(instance, settings, buildResult, material);
            instance.RecordGeneratedBarkMesh(
                mesh,
                barkObject,
                buildResult,
                report);
            EditorUtility.SetDirty(mesh);
            EditorUtility.SetDirty(filter);
            EditorUtility.SetDirty(renderer);
            EditorUtility.SetDirty(instance);
            MarkSceneDirty(instance);
            return true;
        }

        internal static bool RebuildRepresentativeIfPresent(
            TreeReferenceGallery gallery,
            ProceduralTreeInstance instance,
            out string report,
            out string failure)
        {
            report = string.Empty;
            failure = string.Empty;
            if (gallery == null || instance == null)
            {
                failure = "Gallery or procedural instance is null.";
                return false;
            }

            if (instance.SourceVariantIndex != 1)
            {
                RemoveSceneOutput(instance);
                return true;
            }

            return BuildOrUpdate(
                gallery,
                instance.Library,
                instance,
                out _,
                out report,
                out failure);
        }

        internal static void RemoveSceneOutput(
            ProceduralTreeInstance instance)
        {
            if (instance == null)
            {
                return;
            }

            Transform barkTransform = instance.transform.Find(
                GeneratedBarkChildName);
            if (barkTransform != null)
            {
                Undo.DestroyObjectImmediate(barkTransform.gameObject);
            }

            instance.ClearGeneratedBarkOutput();
            EditorUtility.SetDirty(instance);
        }

        private static bool ValidateRepeatableBuild(
            TreeDefinition definition,
            TreeBarkMeshSettings settings,
            TreeBarkMeshBuildResult baseline,
            out string failure)
        {
            failure = string.Empty;
            var verificationMesh = new Mesh
            {
                name = "TREE-GEN.2C Repeatability Verification",
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                TreeBarkMeshBuildResult repeated = TreeBarkMeshGenerator.Build(
                    definition,
                    settings,
                    verificationMesh);
                bool passed =
                    repeated.Passed &&
                    repeated.InputFingerprint == baseline.InputFingerprint &&
                    repeated.GeometryFingerprint == baseline.GeometryFingerprint &&
                    repeated.VertexCount == baseline.VertexCount &&
                    repeated.TriangleCount == baseline.TriangleCount &&
                    repeated.MeshedBranchCount == baseline.MeshedBranchCount;
                if (!passed)
                {
                    failure =
                        "Bark mesh repeatability validation failed. baseline=" +
                        baseline.GeometryFingerprint + " repeated=" +
                        (repeated.Passed
                            ? repeated.GeometryFingerprint
                            : "BUILD FAILED: " + repeated.Failure);
                }

                return passed;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(verificationMesh);
            }
        }

        private static bool FailBuild(
            ProceduralTreeInstance instance,
            Mesh mesh,
            TreeBarkMeshBuildResult buildResult,
            string buildFailure,
            out string failure)
        {
            failure = buildFailure ?? "Generated bark build failed.";
            if (buildResult != null)
            {
                buildResult.MarkFailed(failure);
            }
            mesh.Clear();
            Transform staleBark = instance.transform.Find(GeneratedBarkChildName);
            if (staleBark != null)
            {
                Undo.DestroyObjectImmediate(staleBark.gameObject);
            }

            instance.RecordGeneratedBarkMesh(
                null,
                null,
                buildResult,
                "FAIL | " + failure);
            EditorUtility.SetDirty(mesh);
            EditorUtility.SetDirty(instance);
            MarkSceneDirty(instance);
            return false;
        }

        private static Mesh FindOrCreateManagedMesh(
            TreeGenerationLibrary library,
            ProceduralTreeInstance instance)
        {
            string meshName = BuildManagedMeshName(instance);
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(
                TreeGenerationLibraryBuilder.LibraryAssetPath);
            for (int index = 0; index < assets.Length; index++)
            {
                if (assets[index] is Mesh existing &&
                    existing.name == meshName)
                {
                    return existing;
                }
            }

            var mesh = new Mesh
            {
                name = meshName,
                hideFlags = HideFlags.HideInHierarchy
            };
            AssetDatabase.AddObjectToAsset(mesh, library);
            return mesh;
        }

        private static GameObject EnsureBarkChild(
            ProceduralTreeInstance instance)
        {
            Transform existing = instance.transform.Find(
                GeneratedBarkChildName);
            GameObject child;
            if (existing != null)
            {
                child = existing.gameObject;
                Undo.RecordObject(child.transform, "Rebuild Generated Tree Bark Mesh");
            }
            else
            {
                child = new GameObject(GeneratedBarkChildName);
                Undo.RegisterCreatedObjectUndo(
                    child,
                    "Create Generated Tree Bark Mesh");
                child.transform.SetParent(instance.transform, false);
            }

            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static void ApplyPropertyBlock(
            TreeReferenceGallery gallery,
            ProceduralTreeInstance instance,
            MeshRenderer renderer)
        {
            TreeDefinition definition = instance.GeneratedDefinition;
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(
                BaseColorId,
                definition.ResolvedParameters.BarkTint);
            block.SetFloat(WindEnabledId, gallery.WindEnabled ? 1f : 0f);
            block.SetFloat(WindMaskModeId, 1f);
            block.SetFloat(BoundsMinYId, definition.LocalBounds.min.y);
            block.SetFloat(
                BoundsHeightId,
                Mathf.Max(0.0001f, definition.LocalBounds.size.y));
            block.SetVector(
                RootPositionOsId,
                new Vector4(0f, 0f, 0f, 1f));
            ResolveWindResponse(
                instance.Family,
                out float stiffness,
                out float macroStrength);
            block.SetFloat(StiffnessId, stiffness);
            block.SetFloat(MacroWindStrengthId, macroStrength);
            block.SetFloat(FoliageFlutterStrengthId, 0f);
            int phaseSeed = TreeDeterministicUtility.DeriveSeed(
                "tree-bark-phase",
                instance.MasterSeed,
                instance.StableSlotIdentity);
            block.SetFloat(
                PhaseId,
                (phaseSeed & 0x00FFFFFF) / 16777216f);
            block.SetFloat(DebugModeId, (float)gallery.DebugMode);
            renderer.SetPropertyBlock(block);
        }

        private static void ResolveWindResponse(
            TreeFamily family,
            out float stiffness,
            out float macroStrength)
        {
            switch (family)
            {
                case TreeFamily.Pine:
                    stiffness = 0.65f;
                    macroStrength = 0.45f;
                    break;
                case TreeFamily.Twisted:
                    stiffness = 0.45f;
                    macroStrength = 0.55f;
                    break;
                case TreeFamily.Dead:
                    stiffness = 0.85f;
                    macroStrength = 0.15f;
                    break;
                default:
                    stiffness = 0.35f;
                    macroStrength = 0.65f;
                    break;
            }
        }

        private static string BuildManagedMeshName(
            ProceduralTreeInstance instance)
        {
            return "MESH_ProceduralTreeBark_" +
                instance.Family + "_" +
                instance.SourceVariantIndex;
        }

        private static string BuildReport(
            ProceduralTreeInstance instance,
            TreeBarkMeshSettings settings,
            TreeBarkMeshBuildResult result,
            Material material)
        {
            var report = new StringBuilder(1024);
            report.AppendLine("[TREE-GEN.2C Generated Bark Mesh]");
            report.Append("Slot: ")
                .Append(instance.Family)
                .Append(" ")
                .AppendLine(instance.SourceVariantIndex.ToString());
            report.Append("Branches meshed: ")
                .AppendLine(result.MeshedBranchCount.ToString());
            report.Append("Vertices / triangles: ")
                .Append(result.VertexCount)
                .Append(" / ")
                .AppendLine(result.TriangleCount.ToString());
            report.Append("Tip/base caps: ")
                .AppendLine(result.TipCapCount.ToString());
            report.Append("Alternate tube-quad diagonals: ")
                .AppendLine(result.AlternateQuadDiagonalCount.ToString());
            report.Append("Phase-aligned render rings: ")
                .AppendLine(result.PhaseAlignedRingCount.ToString());
            report.Append("Curvature-radius safety clamps: ")
                .AppendLine(result.CurvatureRadiusClampCount.ToString());
            TreeResolvedParameters resolved =
                instance.GeneratedDefinition.ResolvedParameters;
            report.Append("Radial segments effective T / authored P/S/T: ")
                .Append(result.EffectiveTrunkRadialSegments).Append(" / ")
                .Append(settings.PrimaryRadialSegments).Append("/")
                .Append(settings.SecondaryRadialSegments).Append("/")
                .AppendLine(settings.TertiaryRadialSegments.ToString());
            report.Append("Trunk twist degrees / ridges / depth: ")
                .Append(resolved.TrunkSurfaceTorsionDegrees.ToString("F2"))
                .Append(" / ")
                .Append(resolved.TrunkTwistRidgeCount)
                .Append(" / ")
                .AppendLine(resolved.TrunkTwistRidgeDepth.ToString("F3"));
            report.Append("Root buttress strength / height / flare: ")
                .Append(resolved.RootButtressStrength.ToString("F3"))
                .Append(" / ")
                .Append(resolved.RootButtressHeight.ToString("F3"))
                .Append(" / ")
                .AppendLine(resolved.RootFlareScale.ToString("F3"));
            report.Append("Cross-section max multiplier / root width / depth: ")
                .Append(result.MaximumCrossSectionMultiplier.ToString("F3"))
                .Append(" / ")
                .Append(result.GeneratedRootWidth.ToString("F3"))
                .Append(" / ")
                .AppendLine(result.GeneratedRootDepth.ToString("F3"));
            report.Append("Root inset / blend radii / radius scale / collar / rings: ")
                .Append(settings.BranchRootInsetRatio.ToString("F3")).Append(" / ")
                .Append(settings.BranchRootBlendLengthInChildRadii.ToString("F3")).Append(" / ")
                .Append(settings.BranchRootRadiusScale.ToString("F3")).Append(" / ")
                .Append(settings.BranchRootCollarStrength.ToString("F3")).Append(" / ")
                .AppendLine(settings.BranchRootTransitionRingCount.ToString());
            report.Append("Material: ")
                .AppendLine(material.name);
            report.Append("Bark input fingerprint: ")
                .AppendLine(result.InputFingerprint);
            report.Append("Bark geometry fingerprint: ")
                .AppendLine(result.GeometryFingerprint);
            report.Append("Repeatability rebuild: ")
                .AppendLine(result.RepeatabilityPassed ? "PASS" : "FAIL");
            report.Append("Shared material path: ")
                .AppendLine(AssetDatabase.GetAssetPath(material));
            report.Append("Shared base texture path: ")
                .AppendLine(material.mainTexture != null
                    ? AssetDatabase.GetAssetPath(material.mainTexture)
                    : "None");
            report.Append("Material base colour: ")
                .AppendLine(material.HasProperty(BaseColorId)
                    ? material.GetColor(BaseColorId).ToString()
                    : "Not exposed");
            report.Append("Resolved palette/recipe bark tint: ")
                .AppendLine(instance.GeneratedDefinition.ResolvedParameters.BarkTint.ToString());
            report.Append("Final property-block bark tint: ")
                .AppendLine(instance.GeneratedDefinition.ResolvedParameters.BarkTint.ToString());
            if (result.TopologyAudit != null)
            {
                report.AppendLine();
                report.Append(result.TopologyAudit.Report);
            }
            report.AppendLine("Status: PASS");
            return report.ToString();
        }

        private static void MarkSceneDirty(
            ProceduralTreeInstance instance)
        {
            if (instance != null && instance.gameObject.scene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(instance.gameObject.scene);
            }
        }
    }
}
