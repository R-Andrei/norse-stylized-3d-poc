using System;
using System.Collections.Generic;
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
        private static readonly int DeadBranchMetadataEnabledId =
            Shader.PropertyToID("_TreeDeadBranchMetadataEnabled");

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
            if (gallery == null || instance == null)
            {
                failure = "Gallery or procedural instance is null.";
                return false;
            }

            if (!instance.HasGeneratedDefinition)
            {
                failure = "Procedural instance has no valid structural definition.";
                return false;
            }

            Material material;
            if (instance.UsesRecipeOnlyGeneration)
            {
                material = instance.Recipe != null
                    ? instance.Recipe.BarkMaterial
                    : null;
                if (material == null)
                {
                    failure =
                        "Standalone recipe bark material is missing. Recipe-only " +
                        "generation does not inherit a material from the imported " +
                        "reference grouping.";
                    return false;
                }
            }
            else
            {
                material = instance.Recipe != null &&
                    instance.Recipe.BarkMaterial != null
                        ? instance.Recipe.BarkMaterial
                        : TreeReferenceGalleryBuilder.LoadSharedBarkMaterial(
                            instance.Family);
                if (material == null)
                {
                    failure =
                        "Legacy recipe/shared bark material is missing for " +
                        instance.Family + ".";
                    return false;
                }
            }

            TreeBarkMeshSettings settings = instance.UsesRecipeOnlyGeneration
                ? TreeBarkMeshSettings.CreateRecipeOnlyDefaults()
                : TreeBarkMeshSettings.CreateVerticalSliceDefaults(
                    instance.Family);
            var candidateMesh = new Mesh
            {
                name = BuildManagedMeshName(instance) + " Candidate",
                hideFlags = HideFlags.HideAndDontSave
            };
            try
            {
                buildResult = TreeBarkMeshGenerator.Build(
                    instance.GeneratedDefinition,
                    settings,
                    candidateMesh);
                if (!buildResult.Passed)
                {
                    return FailBuild(
                        instance,
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
                        buildResult,
                        repeatabilityFailure,
                        out failure);
                }

                buildResult.MarkRepeatabilityPassed();
                Mesh mesh = FindOrCreateManagedMesh(library, instance);
                if (mesh == null)
                {
                    return FailBuild(
                        instance,
                        buildResult,
                        "Managed bark mesh asset could not be created.",
                        out failure);
                }

                CommitCandidateMesh(candidateMesh, mesh);
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

                filter.sharedMesh = null;
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
            finally
            {
                UnityEngine.Object.DestroyImmediate(candidateMesh);
            }
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
                name = "TREE-CONTROLS.4 Bark Repeatability Verification",
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
                    repeated.MeshedBranchCount == baseline.MeshedBranchCount &&
                    repeated.TrunkTipClosureApplied ==
                        baseline.TrunkTipClosureApplied &&
                    repeated.TrunkTipRemovedRingCount ==
                        baseline.TrunkTipRemovedRingCount &&
                    Mathf.Abs(
                        repeated.TrunkTipClosureLength -
                        baseline.TrunkTipClosureLength) <= 0.0001f &&
                    repeated.EffectiveTrunkRingCount ==
                        baseline.EffectiveTrunkRingCount &&
                    repeated.RootZoneLongitudinalIntervals ==
                        baseline.RootZoneLongitudinalIntervals &&
                    Mathf.Abs(
                        repeated.AuthoredRootHeightNormalized -
                        baseline.AuthoredRootHeightNormalized) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.EffectiveRootTransitionHeightNormalized -
                        baseline.EffectiveRootTransitionHeightNormalized) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.RootTransitionSafetyTailNormalized -
                        baseline.RootTransitionSafetyTailNormalized) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.ButtressSamplesPerLobe -
                        baseline.ButtressSamplesPerLobe) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.GroundButtressCrestMultiplier -
                        baseline.GroundButtressCrestMultiplier) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.HalfHeightButtressCrestMultiplier -
                        baseline.HalfHeightButtressCrestMultiplier) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.HalfHeightRootExtensionRatio -
                        baseline.HalfHeightRootExtensionRatio) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.HalfHeightButtressAngularWidthScale -
                        baseline.HalfHeightButtressAngularWidthScale) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.RootTopRootOnlyMultiplier -
                        baseline.RootTopRootOnlyMultiplier) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.MaximumGroundButtressCrestTurnDegrees -
                        baseline.MaximumGroundButtressCrestTurnDegrees) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.MaximumPathSpiralRadius -
                        baseline.MaximumPathSpiralRadius) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.MinimumGroundCrossSectionMultiplier -
                        baseline.MinimumGroundCrossSectionMultiplier) <= 0.0001f &&
                    Mathf.Abs(
                        repeated.MeasuredAxialTwistDegrees -
                        baseline.MeasuredAxialTwistDegrees) <= 0.0001f;
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
            TreeBarkMeshBuildResult buildResult,
            string buildFailure,
            out string failure)
        {
            failure = buildFailure ?? "Generated bark build failed.";
            if (buildResult != null)
            {
                buildResult.MarkFailed(failure);
            }

            Mesh preservedMesh = instance != null
                ? instance.GeneratedBarkMesh
                : null;
            GameObject preservedObject = instance != null
                ? instance.GeneratedBarkObject
                : null;
            bool preserved = preservedMesh != null && preservedObject != null;
            if (instance != null)
            {
                instance.RecordGeneratedBarkMesh(
                    preservedMesh,
                    preservedObject,
                    buildResult,
                    "FAIL | " + failure +
                    " | Previous valid bark output preserved: " +
                    (preserved ? "YES" : "NO"));
                EditorUtility.SetDirty(instance);
                MarkSceneDirty(instance);
            }

            return false;
        }

        private static void CommitCandidateMesh(
            Mesh candidate,
            Mesh destination)
        {
            string destinationName = destination.name;
            HideFlags destinationFlags = destination.hideFlags;

            destination.Clear(false);
            destination.indexFormat = candidate.indexFormat;
            destination.SetVertices(candidate.vertices);
            destination.SetNormals(candidate.normals);
            destination.SetTangents(candidate.tangents);
            destination.SetColors(candidate.colors);
            destination.SetUVs(0, candidate.uv);
            destination.subMeshCount = candidate.subMeshCount;
            for (int subMesh = 0; subMesh < candidate.subMeshCount; subMesh++)
            {
                destination.SetIndices(
                    candidate.GetIndices(subMesh),
                    candidate.GetTopology(subMesh),
                    subMesh,
                    false);
            }

            destination.bounds = candidate.bounds;
            destination.name = destinationName;
            destination.hideFlags = destinationFlags;
            destination.UploadMeshData(false);

            if (destination.vertexCount != candidate.vertexCount ||
                destination.GetIndexCount(0) != candidate.GetIndexCount(0))
            {
                throw new InvalidOperationException(
                    "Committed bark mesh buffers do not match the validated candidate. " +
                    "candidateVertices=" + candidate.vertexCount +
                    " destinationVertices=" + destination.vertexCount +
                    " candidateIndices=" + candidate.GetIndexCount(0) +
                    " destinationIndices=" + destination.GetIndexCount(0) + ".");
            }
        }

        private static Mesh FindOrCreateManagedMesh(
            TreeGenerationLibrary library,
            ProceduralTreeInstance instance)
        {
            string meshName = BuildManagedMeshName(instance);
            if (library != null)
            {
                string libraryPath = AssetDatabase.GetAssetPath(library);
                UnityEngine.Object[] assets =
                    string.IsNullOrEmpty(libraryPath)
                        ? Array.Empty<UnityEngine.Object>()
                        : AssetDatabase.LoadAllAssetsAtPath(libraryPath);
                for (int index = 0; index < assets.Length; index++)
                {
                    if (assets[index] is Mesh existing &&
                        existing.name == meshName)
                    {
                        return existing;
                    }
                }

                var managedMesh = new Mesh
                {
                    name = meshName,
                    hideFlags = HideFlags.HideInHierarchy
                };
                AssetDatabase.AddObjectToAsset(managedMesh, library);
                return managedMesh;
            }

            if (instance.GeneratedBarkMesh != null)
            {
                instance.GeneratedBarkMesh.name = meshName;
                return instance.GeneratedBarkMesh;
            }

            return new Mesh
            {
                name = meshName,
                hideFlags = HideFlags.DontSaveInBuild
            };
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
                instance,
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
            block.SetFloat(
                DeadBranchMetadataEnabledId,
                definition.ResolvedParameters.RecipeOnlyControlSource
                    ? 1f
                    : 0f);
            renderer.SetPropertyBlock(block);
        }

        private static void ResolveWindResponse(
            ProceduralTreeInstance instance,
            out float stiffness,
            out float macroStrength)
        {
            if (instance != null && instance.UsesRecipeOnlyGeneration)
            {
                // Wind-response authoring is not part of the accepted 40-control
                // structural schema. Keep one neutral recipe-only response rather
                // than deriving hidden behavior from a reference-family label or
                // overloading an unrelated damage control.
                stiffness = 0.55f;
                macroStrength = 0.45f;
                return;
            }

            TreeFamily family = instance != null
                ? instance.Family
                : TreeFamily.Common;
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

        private static float CalculateMaximumNonTrunkSegmentTurn(
            TreeDefinition definition)
        {
            float maximumTurn = 0f;
            if (definition == null || definition.Branches == null)
            {
                return maximumTurn;
            }

            IReadOnlyList<TreeBranchDefinition> branches = definition.Branches;
            for (int branchIndex = 0;
                 branchIndex < branches.Count;
                 branchIndex++)
            {
                TreeBranchDefinition branch = branches[branchIndex];
                if (branch == null || branch.BranchOrder == 0 ||
                    branch.Samples == null || branch.Samples.Count < 3)
                {
                    continue;
                }

                Vector3 previousDirection =
                    branch.Samples[1].Position -
                    branch.Samples[0].Position;
                if (previousDirection.sqrMagnitude <= 0.000001f)
                {
                    continue;
                }
                previousDirection.Normalize();

                for (int sampleIndex = 2;
                     sampleIndex < branch.Samples.Count;
                     sampleIndex++)
                {
                    Vector3 segment =
                        branch.Samples[sampleIndex].Position -
                        branch.Samples[sampleIndex - 1].Position;
                    if (segment.sqrMagnitude <= 0.000001f)
                    {
                        continue;
                    }

                    Vector3 direction = segment.normalized;
                    maximumTurn = Mathf.Max(
                        maximumTurn,
                        Vector3.Angle(previousDirection, direction));
                    previousDirection = direction;
                }
            }

            return maximumTurn;
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
            report.AppendLine("[TREE-CONTROLS.4 Control-Contract Generated Bark Mesh]");
            report.Append("Bark algorithm version: ")
                .AppendLine(TreeBarkMeshGenerator.BarkAlgorithmVersion.ToString());
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
            report.Append("Collapsed circular render rings removed: ")
                .AppendLine(result.CircularBranchRingRemovalCount.ToString());
            report.Append("Tapered trunk-tip closure / removed rings / length: ")
                .Append(result.TrunkTipClosureApplied ? "YES" : "NO")
                .Append(" / ")
                .Append(result.TrunkTipRemovedRingCount)
                .Append(" / ")
                .AppendLine(result.TrunkTipClosureLength.ToString("F4"));
            TreeResolvedParameters resolved =
                instance.GeneratedDefinition.ResolvedParameters;
            report.Append("Radial segments effective T / authored P/S/T: ")
                .Append(result.EffectiveTrunkRadialSegments).Append(" / ")
                .Append(settings.PrimaryRadialSegments).Append("/")
                .Append(settings.SecondaryRadialSegments).Append("/")
                .AppendLine(settings.TertiaryRadialSegments.ToString());
            report.Append("Effective trunk rings / root-zone intervals / buttress samples per lobe: ")
                .Append(result.EffectiveTrunkRingCount)
                .Append(" / ")
                .Append(result.RootZoneLongitudinalIntervals)
                .Append(" / ")
                .AppendLine(result.ButtressSamplesPerLobe.ToString("F2"));
            report.Append("Trunk axial twist degrees: ")
                .AppendLine(resolved.TrunkSurfaceTorsionDegrees.ToString("F2"));
            report.Append("Requested / measured twist / error: ")
                .Append(result.RequestedAxialTwistDegrees.ToString("F2"))
                .Append(" / ")
                .Append(result.MeasuredAxialTwistDegrees.ToString("F2"))
                .Append(" / ")
                .AppendLine(result.AxialTwistErrorDegrees.ToString("F3"));
            report.Append("Twist turns: ")
                .AppendLine(result.AxialTwistTurns.ToString("F3"));
            report.Append(
                    resolved.RecipeOnlyControlSource
                        ? "Path spiral radius fraction / signed turns / maximum radius: "
                        : "Legacy path spiral strength / turns / direction / maximum radius: ")
                .Append(result.PathSpiralStrength.ToString("F3"))
                .Append(" / ");
            if (resolved.RecipeOnlyControlSource)
            {
                report.Append((
                    result.PathSpiralTurns *
                    (result.PathSpiralDirection < 0f ? -1f : 1f))
                    .ToString("F2"));
            }
            else
            {
                report.Append(result.PathSpiralTurns.ToString("F2"))
                    .Append(" / ")
                    .Append(result.PathSpiralDirection < 0f ? "CW" : "CCW");
            }
            report.Append(" / ")
                .AppendLine(result.MaximumPathSpiralRadius.ToString("F3"));
            if (resolved.RecipeOnlyControlSource)
            {
                report.Append("Recipe roots count / reach / thickness / height: ")
                    .Append(resolved.RootButtressCount)
                    .Append(" / ")
                    .Append(resolved.RootReach.ToString("F3"))
                    .Append(" / ")
                    .Append(resolved.RootThickness.ToString("F3"))
                    .Append(" / ")
                    .AppendLine(resolved.RootButtressHeight.ToString("F3"));
            }
            else
            {
                report.Append("Legacy roots count / strength / height / flare: ")
                    .Append(resolved.RootButtressCount)
                    .Append(" / ")
                    .Append(resolved.RootButtressStrength.ToString("F3"))
                    .Append(" / ")
                    .Append(resolved.RootButtressHeight.ToString("F3"))
                    .Append(" / ")
                    .AppendLine(resolved.RootFlareScale.ToString("F3"));
            }
            if (resolved.RecipeOnlyControlSource)
            {
                report.Append("Root transition authored / effective / safety tail: ")
                    .Append(result.AuthoredRootHeightNormalized.ToString("F4"))
                    .Append(" / ")
                    .Append(result.EffectiveRootTransitionHeightNormalized.ToString("F4"))
                    .Append(" / ")
                    .AppendLine(result.RootTransitionSafetyTailNormalized.ToString("F4"));
                report.Append("Root transition plateau end / lobe collapse end: ")
                    .Append(result.RootGroundPlateauEndNormalized.ToString("F4"))
                    .Append(" / ")
                    .AppendLine(result.RootLobeCollapseEndNormalized.ToString("F4"));
            }
            if (resolved.RecipeOnlyControlSource)
            {
                report.Append("Recipe branch count / secondary density / tertiary density / elevation: ")
                    .Append(resolved.PrimaryBranchCount)
                    .Append(" / ")
                    .Append(resolved.SecondaryDensity.ToString("F3"))
                    .Append(" / ")
                    .Append(resolved.TertiaryDensity.ToString("F3"))
                    .Append(" / ")
                    .Append(resolved.InitialBranchElevationDegrees.ToString("F1"))
                    .AppendLine(" degrees");
            }
            else
            {
                report.Append("Legacy branch count / secondary-per-primary / elevation: ")
                    .Append(resolved.PrimaryBranchCount)
                    .Append(" / ")
                    .Append(resolved.SecondaryBranchesPerPrimary)
                    .Append(" / ")
                    .Append(resolved.InitialBranchElevationDegrees.ToString("F1"))
                    .AppendLine(" degrees");
            }
            report.Append("Primary length / radius / signed arch / late sag: ")
                .Append(resolved.PrimaryBranchLengthRatio.ToString("F3"))
                .Append(" / ")
                .Append(resolved.PrimaryBranchRadiusRatio.ToString("F3"))
                .Append(" / ")
                .Append((resolved.BranchArchDirection *
                    resolved.BranchArchStrength).ToString("F3"))
                .Append(" / ")
                .AppendLine(resolved.LateBranchSag.ToString("F3"));
            report.Append("Primary start/end / symmetry / directional bias: ")
                .Append(resolved.PrimaryBranchStartHeight.ToString("F3"))
                .Append(" / ")
                .Append(resolved.PrimaryBranchEndHeight.ToString("F3"))
                .Append(" / ")
                .Append(resolved.AzimuthSymmetry.ToString("F3"))
                .Append(" / ")
                .AppendLine(resolved.DirectionalBiasStrength.ToString("F3"));
            float sampledTurnLimit;
            if (instance.UsesRecipeOnlyGeneration)
            {
                sampledTurnLimit = Mathf.Max(
                    4f,
                    TreeGenerationRuntimePolicy.RecipeOnly()
                        .MaximumBranchSegmentTurnDegrees * 0.45f);
            }
            else
            {
                sampledTurnLimit =
                    (instance.Family == TreeFamily.Twisted ||
                     instance.Family == TreeFamily.Dead) &&
                    instance.Recipe != null &&
                    instance.Recipe.FamilyProfile != null
                        ? Mathf.Max(
                            4f,
                            instance.Recipe.FamilyProfile.StructuralConstraints
                                .MaximumBranchSegmentTurnDegrees * 0.45f)
                        : 0f;
            }
            report.Append("Measured maximum branch-segment turn / sampled limit: ")
                .Append(CalculateMaximumNonTrunkSegmentTurn(
                    instance.GeneratedDefinition).ToString("F2"))
                .Append(" / ")
                .AppendLine(sampledTurnLimit.ToString("F2"));
            report.Append("Root profile ground crest / valley / half crest / half extension ratio: ")
                .Append(result.GroundButtressCrestMultiplier.ToString("F3"))
                .Append(" / ")
                .Append(result.MinimumGroundCrossSectionMultiplier.ToString("F3"))
                .Append(" / ")
                .Append(result.HalfHeightButtressCrestMultiplier.ToString("F3"))
                .Append(" / ")
                .AppendLine(result.HalfHeightRootExtensionRatio.ToString("F3"));
            report.Append("Root profile half-height angular width scale: ")
                .AppendLine(result.HalfHeightButtressAngularWidthScale.ToString("F3"));
            report.Append("Ground root half-extension full angular width / chord width: ")
                .Append(result.GroundRootHalfExtensionAngularWidthDegrees.ToString("F2"))
                .Append(" degrees / ")
                .Append(result.GroundRootHalfExtensionChordWidth.ToString("F3"))
                .AppendLine(" m");
            report.Append("Root support requested / emitted / count clamp: ")
                .Append(result.RequestedRootSupportAngularWidthDegrees.ToString("F2"))
                .Append(" / ")
                .Append(result.EmittedRootSupportAngularWidthDegrees.ToString("F2"))
                .Append(" degrees / ")
                .AppendLine(result.RootSupportWidthClampedByCount ? "YES" : "NO");
            report.Append("Root profile top root-only / maximum crest turn: ")
                .Append(result.RootTopRootOnlyMultiplier.ToString("F4"))
                .Append(" / ")
                .Append(result.MaximumGroundButtressCrestTurnDegrees.ToString("F2"))
                .AppendLine(" degrees");
            report.Append("Cross-section max / root width / depth: ")
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
            report.Append("Resolved bark tint: ")
                .AppendLine(instance.GeneratedDefinition.ResolvedParameters.BarkTint.ToString());
            report.Append("Final property-block bark tint: ")
                .AppendLine(instance.GeneratedDefinition.ResolvedParameters.BarkTint.ToString());
            report.Append("Dead-branch vertex metadata enabled: ")
                .AppendLine(
                    instance.GeneratedDefinition.ResolvedParameters
                        .RecipeOnlyControlSource
                            ? "YES"
                            : "NO (legacy compatibility metadata preserved)");
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
