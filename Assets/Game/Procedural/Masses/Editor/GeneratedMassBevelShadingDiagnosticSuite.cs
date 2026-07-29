using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses.Editor
{
    internal static class GeneratedMassBevelShadingDiagnosticSuite
    {
        private const string ReportPath = "Library/GeneratedMassBevelShadingDiagnostic.txt";
        private const float PositionQuantization = 100000f;
        private const float Epsilon = 0.000001f;
        private static Job activeJob;
        private static string lastReport = string.Empty;
        private static string lastSummary = string.Empty;

        internal static bool IsRunning => activeJob != null;
        internal static bool HasReport => !string.IsNullOrEmpty(lastReport);
        internal static string LastSummary => lastSummary;
        internal static string ProgressText => activeJob == null ? string.Empty : activeJob.ProgressText;

        internal static void Start(GeneratedMass target)
        {
            if (activeJob != null || target == null || target.GeometryMeshFilter == null) return;
            activeJob = new Job(target);
            EditorApplication.update -= Advance;
            EditorApplication.update += Advance;
        }

        internal static void Cancel() { if (activeJob != null) activeJob.CancelRequested = true; }
        internal static void CopyLastReport() { if (HasReport) EditorGUIUtility.systemCopyBuffer = lastReport; }

        private static void Advance()
        {
            Job job = activeJob;
            if (job == null) return;
            if (job.CancelRequested || job.Target == null) { Finish(job, true, "cancelled"); return; }
            if (EditorUtility.DisplayCancelableProgressBar("Generated Mass Bevel-Shading Evidence Suite", job.ProgressText, job.Progress01))
            { Finish(job, true, "cancelled"); return; }
            try
            {
                switch (job.Stage)
                {
                    case Stage.CaptureGeneration:
                        MassGenerator.BeginBevelShadingDiagnosticCapture();
                        try { job.Target.Regenerate(); }
                        finally { job.Snapshot = MassGenerator.EndBevelShadingDiagnosticCapture(); }
                        job.Mesh = job.Target.GeometryMeshFilter.sharedMesh;
                        job.LoadFinalMesh();
                        job.Stage = Stage.BuildIndices;
                        break;
                    case Stage.BuildIndices:
                        job.BuildIndices();
                        job.Stage = job.CaptureContractValid
                            ? Stage.AnalyzeBevels
                            : Stage.Finalize;
                        break;
                    case Stage.AnalyzeBevels:
                        if (!job.AnalyzeNextBevel()) job.Stage = Stage.Finalize;
                        break;
                    case Stage.Finalize:
                        Finish(job, false, string.Empty);
                        break;
                }
            }
            catch (Exception exception)
            {
                if (MassGenerator.EndBevelShadingDiagnosticCapture() != null) { }
                Finish(job, true, exception.GetType().Name + ":" + exception.Message);
            }
        }

        private static void Finish(Job job, bool cancelled, string reason)
        {
            EditorApplication.update -= Advance;
            EditorUtility.ClearProgressBar();
            string report = BuildReport(job, cancelled, reason);
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath) ?? "Library");
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            lastReport = report;
            EditorGUIUtility.systemCopyBuffer = report;
            lastSummary = cancelled ? "Bevel-shading suite stopped: " + reason : "Bevel-shading suite complete; report copied.";
            activeJob = null;
            Debug.Log(lastSummary, job.Target);
        }

        private static string BuildReport(Job job, bool cancelled, string reason)
        {
            StringBuilder b = new StringBuilder(65536);
            b.AppendLine("GENERATED MASS BEVEL-SHADING EVIDENCE SUITE");
            b.AppendLine("contract=GM-SURFACE-BEVEL-SHADING-AUDIT-2-COMPREHENSIVE");
            b.AppendLine("generatedUtc=" + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            b.AppendLine("terminal=1"); b.AppendLine("cancelled=" + (cancelled ? 1 : 0)); b.AppendLine("terminalReason=" + reason);
            b.AppendLine("object=" + (job.Target == null ? "<destroyed>" : job.Target.name));
            b.AppendLine("mesh=" + (job.Mesh == null ? "<none>" : job.Mesh.name));
            b.AppendLine("shader=" + job.ShaderName); b.AppendLine("material=" + job.MaterialName);
            b.AppendLine("vertices=" + job.VertexCount); b.AppendLine("triangles=" + job.TriangleCount);
            MassGenerator.BevelShadingDiagnosticBuildRecord acceptedBuild = job.AcceptedBuild;
            b.AppendLine("runId=" + (job.Snapshot == null ? -1 : job.Snapshot.RunId));
            b.AppendLine("internalBuildCount=" + (job.Snapshot == null ? 0 : job.Snapshot.Builds.Count));
            b.AppendLine("acceptedBuildId=" + (acceptedBuild == null ? -1 : acceptedBuild.BuildId));
            b.AppendLine("captureContractValid=" + (job.CaptureContractValid ? 1 : 0));
            b.AppendLine("causalAnalysisPerformed=" + (job.CaptureContractValid ? 1 : 0));
            b.AppendLine("logicalBevelsCaptured=" + (acceptedBuild == null ? 0 : acceptedBuild.LogicalBevels.Count));
            b.AppendLine("acceptedCapturedTriangles=" + (acceptedBuild == null ? 0 : acceptedBuild.FinalTriangles.Count));
            b.AppendLine("uploadedMeshTriangles=" + job.TriangleCount);
            b.AppendLine("unmappedBevelTriangles=" + job.UnmappedBevelTriangles);
            b.AppendLine("sourceFaceTriangles=" + CountProvenance(acceptedBuild, "SourceFace"));
            b.AppendLine("edgeBevelPlaneTriangles=" + CountProvenance(acceptedBuild, "EdgeBevelPlane"));
            b.AppendLine("boundedEdgeBevelTriangles=" + CountProvenance(acceptedBuild, "BoundedEdgeBevel"));
            b.AppendLine("vertexJunctionPlaneTriangles=" + CountProvenance(acceptedBuild, "VertexJunctionPlane"));
            b.AppendLine("boundedEndpointCapTriangles=" + CountProvenance(acceptedBuild, "BoundedEndpointCap"));
            b.AppendLine("cornerDamageCapTriangles=" + CountProvenance(acceptedBuild, "CornerDamageCap"));
            b.AppendLine("preMaskImmutableFingerprint=" +
                (acceptedBuild == null ? "0" :
                    acceptedBuild.PreMaskImmutableFingerprint.ToString("X16")));
            b.AppendLine("postMaskImmutableFingerprint=" +
                (acceptedBuild == null ? "0" :
                    acceptedBuild.PostMaskImmutableFingerprint.ToString("X16")));
            b.AppendLine("preMaskValueFingerprint=" +
                (acceptedBuild == null ? "0" :
                    acceptedBuild.PreMaskValueFingerprint.ToString("X16")));
            b.AppendLine("postMaskValueFingerprint=" +
                (acceptedBuild == null ? "0" :
                    acceptedBuild.PostMaskValueFingerprint.ToString("X16")));
            b.AppendLine("geometryFingerprintMatch=" +
                (acceptedBuild != null &&
                 acceptedBuild.PreMaskImmutableFingerprint ==
                    acceptedBuild.PostMaskImmutableFingerprint ? 1 : 0));
            b.AppendLine("sourceFaceMaskChanges=" +
                (acceptedBuild == null ? 0 :
                    acceptedBuild.SourceFaceMaskChangeCount));
            b.AppendLine("reconciledLogicalBevelPositionGroups=" +
                (acceptedBuild == null ? 0 :
                    acceptedBuild.ReconciledLogicalBevelPositionGroups));
            b.AppendLine("reconciledLogicalBevelVertices=" +
                (acceptedBuild == null ? 0 :
                    acceptedBuild.ReconciledLogicalBevelVertices));
            b.AppendLine("preMaskDegenerateTriangles=" +
                (acceptedBuild == null ? 0 :
                    acceptedBuild.PreMaskDegenerateTriangleCount));
            b.AppendLine("postMaskDegenerateTriangles=" +
                (acceptedBuild == null ? 0 :
                    acceptedBuild.PostMaskDegenerateTriangleCount));
            b.AppendLine("degenerateParity=" +
                (acceptedBuild != null &&
                 acceptedBuild.PreMaskDegenerateTriangleCount ==
                    acceptedBuild.PostMaskDegenerateTriangleCount &&
                 acceptedBuild.PreMaskDegenerateTriangleFingerprint ==
                    acceptedBuild.PostMaskDegenerateTriangleFingerprint ? 1 : 0));
            int uploadedMeshDegenerateTriangles =
                CountUploadedMeshDegenerateTriangles(job);
            if (job.Snapshot != null)
            {
                foreach (var build in job.Snapshot.Builds)
                    b.AppendLine("build=" + build.BuildId + ",purpose=" + build.Purpose + ",completed=" + (build.Completed ? 1 : 0) + ",succeeded=" + (build.Succeeded ? 1 : 0) + ",logicalBevels=" + build.LogicalBevels.Count + ",triangles=" + build.FinalTriangles.Count + ",accepted=" + (build.AcceptedForUpload ? 1 : 0));
            }
            foreach (string failure in job.CaptureFailures) b.AppendLine("captureFailure=" + failure);
            b.AppendLine("mainLightDirectionLocal=" + V(job.MainLightLocal));
            b.AppendLine(); b.AppendLine("[Material properties]");
            foreach (var p in job.MaterialProperties.OrderBy(x => x.Key)) b.AppendLine(p.Key + "=" + F(p.Value));

            int groupFrag = job.Results.Count(x => x.SurfaceGroupCount > 1);
            int normalFrag = job.Results.Count(x => x.InternalRenderNormalJumpCount > 0);
            int maskJump = job.Results.Count(x => x.InternalMaskJumpCount > 0);
            int valueGradient = job.Results.Count(x => x.ValueGradientJumpCount > 0);
            int structuralGradient = job.Results.Count(x => x.StructuralGradientJumpCount > 0);
            int geometricFacet = job.Results.Count(x => x.GeometricFacetRiskCount > 0);
            int sliverRisk = job.Results.Count(x => x.SliverTriangleCount > 0);
            int lowLightNormalSensitivity = job.Results.Count(x => x.LowLightNormalSensitivityCount > 0);
            int cone = job.Results.Count(x => x.ParentConeViolationCount > 0);
            int darker = job.Results.Count(x => x.ActiveLightDarkerThanBothCount > 0);
            int meshMismatch = job.Results.Count(x => x.FinalMeshMismatchCount > 0);
            int logicalBevelDegenerateTriangles =
                job.Results.Sum(x => x.DegenerateTriangleCount);
            int acceptedOrdinaryBevelDegenerateTriangles = acceptedBuild == null
                ? 0
                : CountCapturedOrdinaryBevelDegenerateTriangles(acceptedBuild);
            bool degenerateAccountingMismatch =
                logicalBevelDegenerateTriangles !=
                    acceptedOrdinaryBevelDegenerateTriangles ||
                uploadedMeshDegenerateTriangles <
                    logicalBevelDegenerateTriangles;
            bool geometryRegression = acceptedBuild != null &&
                (acceptedBuild.PreMaskImmutableFingerprint !=
                    acceptedBuild.PostMaskImmutableFingerprint ||
                 acceptedBuild.SourceFaceMaskChangeCount != 0 ||
                 acceptedBuild.PreMaskDegenerateTriangleCount !=
                    acceptedBuild.PostMaskDegenerateTriangleCount ||
                 acceptedBuild.PreMaskDegenerateTriangleFingerprint !=
                    acceptedBuild.PostMaskDegenerateTriangleFingerprint);
            int supportedCauseFamilies =
                (groupFrag > 0 ? 1 : 0) +
                (valueGradient > 0 ? 1 : 0) +
                (structuralGradient > 0 ? 1 : 0) +
                (geometricFacet > 0 || sliverRisk > 0 ? 1 : 0) +
                (normalFrag > 0 || cone > 0 || lowLightNormalSensitivity > 0 ? 1 : 0) +
                (maskJump > 0 ? 1 : 0) +
                (darker > 0 ? 1 : 0);
            string decision = !job.CaptureContractValid ? "CAPTURE_CONTRACT_FAILURE" :
                uploadedMeshDegenerateTriangles > 0 ?
                    "UPLOADED_MESH_DEGENERATE_TRIANGLES" :
                degenerateAccountingMismatch ?
                    "DEGENERATE_ACCOUNTING_MISMATCH" :
                geometryRegression ? "GEOMETRY_REGRESSION" :
                meshMismatch > 0 ? "CAPTURE_TO_FINAL_MESH_MISMATCH" :
                supportedCauseFamilies > 1 ? "MULTIPLE_RESIDUAL_SHADING_RISK_FAMILIES" :
                groupFrag > 0 ? "LOGICAL_BEVEL_SURFACE_GROUP_FRAGMENTATION" :
                valueGradient > 0 ? "PRELIGHT_VALUE_FIELD_GRADIENT_DISCONTINUITY" :
                structuralGradient > 0 ? "STRUCTURAL_CHANNEL_GRADIENT_DISCONTINUITY" :
                geometricFacet > 0 || sliverRisk > 0 ? "GEOMETRIC_TRIANGULATION_FACET_RISK" :
                normalFrag > 0 || cone > 0 || lowLightNormalSensitivity > 0 ? "NORMAL_OR_LOW_LIGHT_RESPONSE_RISK" :
                maskJump > 0 ? "PRE_LIGHT_MASK_EDGE_JUMP" :
                darker > 0 ? "ACTIVE_LIGHT_RESPONSE_DARKER_THAN_BOTH_PARENTS" :
                "NO_CAPTURED_CAUSE_FAMILY_REPRODUCED_RESIDUAL";
            bool includeDetailedEvidence = true;
            b.AppendLine(); b.AppendLine("[Decision summary]");
            b.AppendLine("logicalBevelsAnalyzed=" + job.Results.Count);
            b.AppendLine("surfaceGroupFragmentation=" + groupFrag);
            b.AppendLine("internalRenderNormalFragmentation=" + normalFrag);
            b.AppendLine("parentNormalConeViolations=" + cone);
            b.AppendLine("internalMaskEdgeJumps=" + maskJump);
            b.AppendLine("valueGradientDiscontinuityBevels=" + valueGradient);
            b.AppendLine("structuralGradientDiscontinuityBevels=" + structuralGradient);
            b.AppendLine("geometricFacetRiskBevels=" + geometricFacet);
            b.AppendLine("sliverTriangleRiskBevels=" + sliverRisk);
            b.AppendLine("lowLightNormalSensitivityBevels=" + lowLightNormalSensitivity);
            b.AppendLine("maximumSurfaceVariationGradientJump=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaxSurfaceVariationGradientJump)));
            b.AppendLine("maximumExposureGradientJump=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaxExposureGradientJump)));
            b.AppendLine("maximumCreviceGradientJump=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaxCreviceGradientJump)));
            b.AppendLine("maximumDirtGradientJump=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaxDirtGradientJump)));
            b.AppendLine("maximumStructuralGradientJump=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaxStructuralGradientJump)));
            b.AppendLine("maximumGeometricNormalJumpDeg=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaximumInternalGeometricNormalJump)));
            b.AppendLine("maximumTriangleAspectRatio=" + F(job.Results.Count == 0 ? 0f : job.Results.Max(x => x.MaximumAspectRatio)));
            b.AppendLine("activeLightDarkerThanBothParents=" + darker);
            b.AppendLine("captureToFinalMeshMismatches=" + meshMismatch);
            b.AppendLine("uploadedMeshDegenerateTriangles=" +
                uploadedMeshDegenerateTriangles);
            b.AppendLine("sumOfLogicalBevelDegenerateTriangles=" +
                logicalBevelDegenerateTriangles);
            b.AppendLine("acceptedOrdinaryBevelDegenerateTriangles=" +
                acceptedOrdinaryBevelDegenerateTriangles);
            b.AppendLine("degenerateAccountingMismatch=" +
                (degenerateAccountingMismatch ? 1 : 0));
            b.AppendLine("geometryRegression=" + (geometryRegression ? 1 : 0));
            b.AppendLine("reportDetail=comprehensive-evidence");
            b.AppendLine("decision=" + decision);
            b.AppendLine(); b.AppendLine("[Shader-side paths not reconstructed by CPU suite]");
            b.AppendLine("worldPositionPixelCells=1");
            b.AppendLine("worldPositionBroadNoise=1");
            b.AppendLine("generatedFeatureAtlasSampling=1");
            b.AppendLine("wholeSurfaceNormalFunction=1");
            b.AppendLine("sphericalHarmonicsAmbient=1");
            b.AppendLine("mainAndAdditionalLightShadows=1");
            b.AppendLine("screenSpaceAmbientOcclusion=1");
            b.AppendLine("specularAndSmoothnessPBR=1");
            b.AppendLine("fogAndPostProcessing=1");
            b.AppendLine("existingVisualIsolationModes=SurfaceVariation/Exposure/CreviceBase/DirtDeposit/ConvexBoundaryProximity/ConcaveBoundaryProximity/BoundaryFieldDiagnostic/BoundaryModulationDiagnostic");

            if (includeDetailedEvidence)
            {
                foreach (BevelResult r in job.Results.OrderByDescending(x => x.Severity).ThenBy(x => x.EdgeId))
                {
                    b.AppendLine(); b.AppendLine("[Logical bevel " + r.EdgeId + "]");
                    b.AppendLine("candidateId=" + r.CandidateId);
                    MassGenerator.LogicalBevelRecord logicalRecord =
                    acceptedBuild.LogicalBevels[r.EdgeId];
                    b.AppendLine("graphEdgeId=" + logicalRecord.GraphEdgeIndex +
                    ",sourceEdgeId=" + logicalRecord.SourceEdgeIndex +
                    ",emittedProvenance=" +
                    logicalRecord.EmittedProvenanceKindName + ":" +
                    logicalRecord.EmittedProvenanceIndex);
                    b.AppendLine("parentFaceA=" + r.ParentFaceA + ",parentFaceB=" + r.ParentFaceB);
                    b.AppendLine("parentNormalA=" + V(r.ParentNormalA)); b.AppendLine("parentNormalB=" + V(r.ParentNormalB));
                    b.AppendLine("parentNormalAngleDeg=" + F(r.ParentNormalAngle));
                    b.AppendLine("sourceEdgeA=" + V(r.SourceA) + ",sourceEdgeB=" + V(r.SourceB));
                    b.AppendLine("triangleCount=" + r.TriangleCount + ",surfaceGroupCount=" + r.SurfaceGroupCount + ",surfaceGroups=" + string.Join("/", r.SurfaceGroups));
                    b.AppendLine("renderNormalClusters=" + r.RenderNormalClusters + ",geometricNormalClusters=" + r.GeometricNormalClusters);
                    b.AppendLine("maximumInternalRenderNormalJumpDeg=" + F(r.MaximumInternalRenderNormalJump));
                    b.AppendLine("maximumInternalGeometricNormalJumpDeg=" + F(r.MaximumInternalGeometricNormalJump));
                    b.AppendLine("internalRenderNormalJumpCount=" + r.InternalRenderNormalJumpCount);
                    b.AppendLine("internalMaskJumpCount=" + r.InternalMaskJumpCount);
                    b.AppendLine("maxExposureEdgeJump=" + F(r.MaxExposureJump) + ",maxCreviceEdgeJump=" + F(r.MaxCreviceJump) + ",maxDirtEdgeJump=" + F(r.MaxDirtJump));
                    b.AppendLine("valueGradientJumpCount=" + r.ValueGradientJumpCount + ",structuralGradientJumpCount=" + r.StructuralGradientJumpCount);
                    b.AppendLine("maxSurfaceVariationGradientJump=" + F(r.MaxSurfaceVariationGradientJump) + ",maxExposureGradientJump=" + F(r.MaxExposureGradientJump) + ",maxCreviceGradientJump=" + F(r.MaxCreviceGradientJump) + ",maxDirtGradientJump=" + F(r.MaxDirtGradientJump));
                    b.AppendLine("maxStructuralGradientJump=" + F(r.MaxStructuralGradientJump));
                    b.AppendLine("minimumTriangleArea=" + F(r.MinimumTriangleArea) + ",maximumAspectRatio=" + F(r.MaximumAspectRatio) + ",sliverTriangleCount=" + r.SliverTriangleCount);
                    b.AppendLine("geometricFacetRiskCount=" + r.GeometricFacetRiskCount + ",lowLightNormalSensitivityCount=" + r.LowLightNormalSensitivityCount + ",maxLowHighVisibilityRatio=" + F(r.MaxLowHighVisibilityRatio));
                    b.AppendLine("parentConeViolationCount=" + r.ParentConeViolationCount + ",worstParentConeDot=" + F(r.WorstParentConeDot));
                    b.AppendLine("activeLightParentA=" + F(r.ActiveParentA) + ",activeLightBevelMin=" + F(r.ActiveBevelMin) + ",activeLightBevelMax=" + F(r.ActiveBevelMax) + ",activeLightParentB=" + F(r.ActiveParentB));
                    b.AppendLine("activeLightDarkerThanBothCount=" + r.ActiveLightDarkerThanBothCount + ",activeLightBrighterThanBothCount=" + r.ActiveLightBrighterThanBothCount);
                    b.AppendLine("exposureRange=" + F(r.ExposureMin) + ".." + F(r.ExposureMax));
                    b.AppendLine("creviceRange=" + F(r.CreviceMin) + ".." + F(r.CreviceMax));
                    b.AppendLine("dirtRange=" + F(r.DirtMin) + ".." + F(r.DirtMax));
                    b.AppendLine("storedVsGeometricMinDot=" + F(r.MinStoredVsGeometricDot));
                    b.AppendLine("tangentOrthogonalityMaxAbsDot=" + F(r.MaxTangentNormalAbsDot));
                    b.AppendLine("captureToFinalMeshMismatchCount=" + r.FinalMeshMismatchCount);
                    b.AppendLine("degenerateTriangleCount=" + r.DegenerateTriangleCount);
                    b.AppendLine("triangleIds=" + string.Join("/", r.TriangleIds));
                    foreach (string e in r.EdgeEvidence.Take(32)) b.AppendLine("edgeEvidence=" + e);
                    foreach (string t in r.TriangleEvidence.Take(64)) b.AppendLine("triangleEvidence=" + t);
                }
            }
            b.AppendLine(); b.AppendLine("[Interpretation contract]");
            b.AppendLine("- every section is keyed by generation-time graph edge identity, not final-mesh connectivity.");
            b.AppendLine("- parent normals and parent face IDs come from the generation topology that created the bevel.");
            b.AppendLine("- discontinuities are measured only across shared internal edges of the same logical bevel.");
            b.AppendLine("- active-light comparisons use the current directional light transformed into mass-local space.");
            b.AppendLine("- capture-to-final checks compare the generation snapshot against the actual uploaded Unity mesh.");
            b.AppendLine("- gradient jumps compare piecewise-linear scalar fields across internal triangulation edges and are normalized by shared-edge scale and local value range.");
            b.AppendLine("- low/high response evidence compares normal-driven contrast at nominal and amplified direct intensity; it does not simulate the complete URP frame.");
            b.AppendLine("- world-position procedural noise, feature-atlas filtering, SH ambient, screen-space AO, shadows and post-processing are listed as shader-side paths not fully reconstructable by this CPU audit.");
            b.AppendLine("- visual failure remains authoritative even when no captured cause family reproduces it.");
            return b.ToString();
        }

        private static int CountProvenance(
            MassGenerator.BevelShadingDiagnosticBuildRecord build,
            string provenanceKindName)
        {
            if (build == null) return 0;
            return build.FinalTriangles.Count(x =>
                string.Equals(
                    x.ProvenanceKindName,
                    provenanceKindName,
                    StringComparison.Ordinal));
        }

        private static int CountUploadedMeshDegenerateTriangles(Job job)
        {
            if (job == null || job.Vertices == null || job.Indices == null)
            {
                return 0;
            }

            int count = 0;
            for (int triangle = 0; triangle + 2 < job.Indices.Length;
                 triangle += 3)
            {
                int ia = job.Indices[triangle];
                int ib = job.Indices[triangle + 1];
                int ic = job.Indices[triangle + 2];
                if (ia < 0 || ib < 0 || ic < 0 ||
                    ia >= job.Vertices.Length ||
                    ib >= job.Vertices.Length ||
                    ic >= job.Vertices.Length ||
                    ia == ib || ib == ic || ic == ia)
                {
                    count++;
                    continue;
                }

                Vector3 cross = Vector3.Cross(
                    job.Vertices[ib] - job.Vertices[ia],
                    job.Vertices[ic] - job.Vertices[ia]);
                if (float.IsNaN(cross.x) || float.IsInfinity(cross.x) ||
                    float.IsNaN(cross.y) || float.IsInfinity(cross.y) ||
                    float.IsNaN(cross.z) || float.IsInfinity(cross.z) ||
                    cross.sqrMagnitude <= Epsilon * Epsilon)
                {
                    count++;
                }
            }
            return count;
        }

        private static int CountCapturedOrdinaryBevelDegenerateTriangles(
            MassGenerator.BevelShadingDiagnosticBuildRecord build)
        {
            if (build == null)
            {
                return 0;
            }

            int count = 0;
            foreach (var triangle in build.FinalTriangles)
            {
                if (!triangle.IsOrdinaryBevel)
                {
                    continue;
                }
                if (N(triangle.GeometricNormal) == Vector3.zero)
                {
                    count++;
                }
            }
            return count;
        }

        private sealed class Job
        {
            internal readonly GeneratedMass Target;
            internal Stage Stage = Stage.CaptureGeneration;
            internal bool CancelRequested;
            internal MassGenerator.BevelShadingDiagnosticSnapshot Snapshot;
            internal Mesh Mesh;
            internal Vector3[] Vertices = Array.Empty<Vector3>();
            internal Vector3[] Normals = Array.Empty<Vector3>();
            internal Vector4[] Tangents = Array.Empty<Vector4>();
            internal Color[] Colors = Array.Empty<Color>();
            internal Vector4[] UV2 = Array.Empty<Vector4>();
            internal Vector4[] Structural = Array.Empty<Vector4>();
            internal int[] Indices = Array.Empty<int>();
            internal readonly Dictionary<int, List<MassGenerator.FinalTriangleRecord>> Bevels = new();
            internal readonly List<string> CaptureFailures = new();
            internal MassGenerator.BevelShadingDiagnosticBuildRecord AcceptedBuild;
            internal bool CaptureContractValid;
            internal int UnmappedBevelTriangles;
            internal readonly List<BevelResult> Results = new();
            internal readonly Dictionary<string, float> MaterialProperties = new();
            internal int NextBevel;
            internal Vector3 MainLightLocal;
            internal string ShaderName = "<none>";
            internal string MaterialName = "<none>";
            internal int VertexCount => Vertices.Length;
            internal int TriangleCount => Indices.Length / 3;
            internal float Progress01 => Stage == Stage.CaptureGeneration ? 0.05f : Stage == Stage.BuildIndices ? 0.2f : Stage == Stage.AnalyzeBevels ? 0.2f + 0.78f * NextBevel / Mathf.Max(1, Bevels.Count) : 0.99f;
            internal string ProgressText => Stage == Stage.CaptureGeneration ? "Capturing generation provenance and rebuilding the selected mass" : Stage == Stage.BuildIndices ? "Indexing logical bevels and final mesh channels" : Stage == Stage.AnalyzeBevels ? "Analyzing logical bevel " + (NextBevel + 1) + "/" + Bevels.Count : "Writing copyable report";

            internal Job(GeneratedMass target)
            {
                Target = target;
                Renderer renderer = target.GeometryMeshFilter == null ? null : target.GeometryMeshFilter.GetComponent<Renderer>();
                Material material = renderer == null ? null : renderer.sharedMaterial;
                MaterialName = material == null ? "<none>" : material.name;
                ShaderName = material == null || material.shader == null ? "<none>" : material.shader.name;
                ReadMaterialProperties(material, MaterialProperties);
                MainLightLocal = ResolveMainLightDirectionLocal(target.transform);
            }

            internal void LoadFinalMesh()
            {
                if (Mesh == null) return;
                Vertices = Mesh.vertices; Normals = Mesh.normals; Tangents = Mesh.tangents; Colors = Mesh.colors; Indices = Mesh.triangles;
                UV2 = ReadUv(Mesh, 2, Vertices.Length); Structural = ReadUv(Mesh, 4, Vertices.Length);
            }

            internal void BuildIndices()
            {
                CaptureContractValid = false;
                if (Snapshot == null)
                {
                    CaptureFailures.Add("snapshot is null");
                    return;
                }
                CaptureFailures.AddRange(Snapshot.ContractFailures);
                AcceptedBuild = Snapshot.AcceptedBuild;
                int acceptedCount = Snapshot.Builds.Count(x => x.AcceptedForUpload);
                if (acceptedCount != 1) CaptureFailures.Add("accepted build count=" + acceptedCount + ",expected=1");
                if (AcceptedBuild == null)
                {
                    CaptureFailures.Add("accepted build is missing");
                    return;
                }
                if (!AcceptedBuild.Completed || !AcceptedBuild.Succeeded) CaptureFailures.Add("accepted build did not complete successfully");
                if (AcceptedBuild.FinalTriangles.Count != TriangleCount) CaptureFailures.Add("accepted triangle count=" + AcceptedBuild.FinalTriangles.Count + ",uploaded=" + TriangleCount);
                if (AcceptedBuild.LogicalBevels.Count == 0) CaptureFailures.Add("accepted build captured zero logical bevels");

                foreach (var t in AcceptedBuild.FinalTriangles)
                {
                    if (!t.IsOrdinaryBevel) continue;
                    if (t.LogicalBevelId < 0 || !AcceptedBuild.LogicalBevels.ContainsKey(t.LogicalBevelId))
                    {
                        UnmappedBevelTriangles++;
                        continue;
                    }
                    if (!Bevels.TryGetValue(t.LogicalBevelId, out var list))
                    {
                        list = new List<MassGenerator.FinalTriangleRecord>();
                        Bevels.Add(t.LogicalBevelId, list);
                    }
                    list.Add(t);
                }
                if (UnmappedBevelTriangles > 0) CaptureFailures.Add("unmapped ordinary bevel triangles=" + UnmappedBevelTriangles);
                if (Bevels.Count != AcceptedBuild.LogicalBevels.Count) CaptureFailures.Add("mapped logical bevels=" + Bevels.Count + ",captured=" + AcceptedBuild.LogicalBevels.Count);
                foreach (int logicalId in AcceptedBuild.LogicalBevels.Keys)
                    if (!Bevels.ContainsKey(logicalId)) CaptureFailures.Add("logical bevel has no final triangles=" + logicalId);
                CaptureContractValid = CaptureFailures.Count == 0 && Bevels.Count > 0;
            }

            internal bool AnalyzeNextBevel()
            {
                if (NextBevel >= Bevels.Count) return false;
                var pair = Bevels.OrderBy(x => x.Key).ElementAt(NextBevel++);
                AcceptedBuild.LogicalBevels.TryGetValue(pair.Key, out var logical);
                Results.Add(Analyze(pair.Key, logical, pair.Value, this));
                return NextBevel < Bevels.Count;
            }
        }

        private static BevelResult Analyze(int edgeId, MassGenerator.LogicalBevelRecord logical, List<MassGenerator.FinalTriangleRecord> triangles, Job job)
        {
            BevelResult r = new BevelResult { EdgeId = edgeId, TriangleCount = triangles.Count };
            if (logical != null)
            {
                r.CandidateId = logical.CandidateIndex; r.ParentFaceA = logical.ParentFaceA; r.ParentFaceB = logical.ParentFaceB;
                r.ParentNormalA = N(logical.ParentNormalA); r.ParentNormalB = N(logical.ParentNormalB); r.SourceA = logical.SourceA; r.SourceB = logical.SourceB;
                r.ParentNormalAngle = Angle(r.ParentNormalA, r.ParentNormalB);
            }
            r.SurfaceGroups = triangles.Select(x => x.SurfaceGroup).Distinct().OrderBy(x => x).ToArray(); r.SurfaceGroupCount = r.SurfaceGroups.Length;
            r.RenderNormalClusters = Cluster(triangles.Select(x => x.RenderNormal)); r.GeometricNormalClusters = Cluster(triangles.Select(x => x.GeometricNormal));
            r.ExposureMin = triangles.Min(x => Min3(x.MaskA.y, x.MaskB.y, x.MaskC.y)); r.ExposureMax = triangles.Max(x => Max3(x.MaskA.y, x.MaskB.y, x.MaskC.y));
            r.CreviceMin = triangles.Min(x => Min3(x.MaskA.z, x.MaskB.z, x.MaskC.z)); r.CreviceMax = triangles.Max(x => Max3(x.MaskA.z, x.MaskB.z, x.MaskC.z));
            r.DirtMin = triangles.Min(x => Min3(x.MaskA.w, x.MaskB.w, x.MaskC.w)); r.DirtMax = triangles.Max(x => Max3(x.MaskA.w, x.MaskB.w, x.MaskC.w));
            r.MinStoredVsGeometricDot = triangles.Min(x => Vector3.Dot(N(x.RenderNormal), N(x.GeometricNormal)));
            r.ActiveLightDirection = job.MainLightLocal;
            r.AmbientStrength = GetMaterial(job, "_AmbientStrength", 1f);
            r.DirectStrength = GetMaterial(job, "_DirectStrength", 1f);
            r.FlatNormalStrength = GetMaterial(job, "_FlatNormalStrength", 0f);
            r.ActiveParentA = Mathf.Max(0f, Vector3.Dot(r.ParentNormalA, job.MainLightLocal)); r.ActiveParentB = Mathf.Max(0f, Vector3.Dot(r.ParentNormalB, job.MainLightLocal));
            r.ActiveBevelMin = 1f; r.ActiveBevelMax = 0f;
            foreach (var t in triangles)
            {
                float area = TriangleArea(t.A, t.B, t.C);
                float aspect = TriangleAspectRatio(t.A, t.B, t.C, area);
                r.MinimumTriangleArea = Mathf.Min(r.MinimumTriangleArea, area);
                r.MaximumAspectRatio = Mathf.Max(r.MaximumAspectRatio, aspect);
                if (area <= 0.00000001f || aspect > 20f) r.SliverTriangleCount++;
                float response = Mathf.Max(0f, Vector3.Dot(N(t.RenderNormal), job.MainLightLocal));
                r.ActiveBevelMin = Mathf.Min(r.ActiveBevelMin, response); r.ActiveBevelMax = Mathf.Max(r.ActiveBevelMax, response);
                float parentMin = Mathf.Min(r.ActiveParentA, r.ActiveParentB), parentMax = Mathf.Max(r.ActiveParentA, r.ActiveParentB);
                if (response + 0.005f < parentMin) r.ActiveLightDarkerThanBothCount++;
                if (response - 0.005f > parentMax) r.ActiveLightBrighterThanBothCount++;
                float coneDot = Mathf.Min(Vector3.Dot(N(t.RenderNormal), r.ParentNormalA), Vector3.Dot(N(t.RenderNormal), r.ParentNormalB));
                if (coneDot < -0.0001f) { r.ParentConeViolationCount++; r.WorstParentConeDot = Mathf.Min(r.WorstParentConeDot, coneDot); }
                r.TriangleIds.Add(t.TriangleIndex);
                r.TriangleEvidence.Add("tri=" + t.TriangleIndex + ",group=" + t.SurfaceGroup + ",geomN=" + V(t.GeometricNormal) + ",renderN=" + V(t.RenderNormal) + ",authoredN=" + V(t.AuthoredNormal) + ",maskA=" + V4(t.MaskA) + ",maskB=" + V4(t.MaskB) + ",maskC=" + V4(t.MaskC));
                CompareFinalMesh(t, job, r);
            }
            AnalyzeInternalEdges(triangles, r);
            r.Severity = r.FinalMeshMismatchCount * 100000 + (r.SurfaceGroupCount - 1) * 10000 + r.InternalRenderNormalJumpCount * 1000 + r.ParentConeViolationCount * 100 + r.InternalMaskJumpCount * 50 + r.ValueGradientJumpCount * 40 + r.StructuralGradientJumpCount * 30 + r.GeometricFacetRiskCount * 20 + r.SliverTriangleCount * 10 + r.ActiveLightDarkerThanBothCount;
            return r;
        }

        private static void AnalyzeInternalEdges(List<MassGenerator.FinalTriangleRecord> triangles, BevelResult r)
        {
            var edges = new Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>>();
            foreach (var t in triangles) { Add(edges, t, t.A, t.B); Add(edges, t, t.B, t.C); Add(edges, t, t.C, t.A); }
            foreach (var pair in edges.Where(x => x.Value.Count == 2))
            {
                var a = pair.Value[0]; var b = pair.Value[1];
                float rn = Angle(a.RenderNormal, b.RenderNormal), gn = Angle(a.GeometricNormal, b.GeometricNormal);
                r.MaximumInternalRenderNormalJump = Mathf.Max(r.MaximumInternalRenderNormalJump, rn); r.MaximumInternalGeometricNormalJump = Mathf.Max(r.MaximumInternalGeometricNormalJump, gn);
                if (rn > 0.5f) r.InternalRenderNormalJumpCount++;
                if (gn > 5f) r.GeometricFacetRiskCount++;
                Vector4 am = SharedEdgeMask(a, pair.Key), bm = SharedEdgeMask(b, pair.Key);
                float ex = Mathf.Abs(am.y - bm.y), cr = Mathf.Abs(am.z - bm.z), di = Mathf.Abs(am.w - bm.w);
                r.MaxExposureJump = Mathf.Max(r.MaxExposureJump, ex); r.MaxCreviceJump = Mathf.Max(r.MaxCreviceJump, cr); r.MaxDirtJump = Mathf.Max(r.MaxDirtJump, di);
                if (ex > 0.00001f || cr > 0.00001f || di > 0.00001f) r.InternalMaskJumpCount++;

                float svGrad = GradientJump(a, b, 0, false, pair.Key);
                float exGrad = GradientJump(a, b, 1, false, pair.Key);
                float crGrad = GradientJump(a, b, 2, false, pair.Key);
                float diGrad = GradientJump(a, b, 3, false, pair.Key);
                float st0 = GradientJump(a, b, 0, true, pair.Key);
                float st1 = GradientJump(a, b, 1, true, pair.Key);
                float st2 = GradientJump(a, b, 2, true, pair.Key);
                float st3 = GradientJump(a, b, 3, true, pair.Key);
                float structuralGrad = Mathf.Max(Mathf.Max(st0, st1), Mathf.Max(st2, st3));
                r.MaxSurfaceVariationGradientJump = Mathf.Max(r.MaxSurfaceVariationGradientJump, svGrad);
                r.MaxExposureGradientJump = Mathf.Max(r.MaxExposureGradientJump, exGrad);
                r.MaxCreviceGradientJump = Mathf.Max(r.MaxCreviceGradientJump, crGrad);
                r.MaxDirtGradientJump = Mathf.Max(r.MaxDirtGradientJump, diGrad);
                r.MaxStructuralGradientJump = Mathf.Max(r.MaxStructuralGradientJump, structuralGrad);
                float valueGrad = Mathf.Max(Mathf.Max(svGrad, exGrad), Mathf.Max(crGrad, diGrad));
                if (valueGrad > 0.35f) r.ValueGradientJumpCount++;
                if (structuralGrad > 0.35f) r.StructuralGradientJumpCount++;

                float lowContrast = Mathf.Abs(Mathf.Max(0f, Vector3.Dot(N(a.GeometricNormal), r.ActiveLightDirection)) - Mathf.Max(0f, Vector3.Dot(N(b.GeometricNormal), r.ActiveLightDirection)));
                float ambientFloor = Mathf.Max(0.001f, r.AmbientStrength);
                float lowDirect = Mathf.Max(0f, r.DirectStrength) * 0.25f;
                float highDirect = Mathf.Max(0f, r.DirectStrength) * 4f;
                float lowVisible = lowContrast * lowDirect / (ambientFloor + lowDirect);
                float highVisible = lowContrast * highDirect / (ambientFloor + highDirect);
                float visibilityRatio = highVisible <= Epsilon ? 0f : lowVisible / highVisible;
                r.MaxLowHighVisibilityRatio = Mathf.Max(r.MaxLowHighVisibilityRatio, visibilityRatio);
                if (r.FlatNormalStrength > 0.001f && gn > 1f && lowContrast > 0.01f) r.LowLightNormalSensitivityCount++;

                if (rn > 0.5f || gn > 5f || ex > 0.00001f || cr > 0.00001f || di > 0.00001f || valueGrad > 0.35f || structuralGrad > 0.35f)
                    r.EdgeEvidence.Add("edge=" + pair.Key + ",renderJumpDeg=" + F(rn) + ",geomJumpDeg=" + F(gn) + ",exposureJump=" + F(ex) + ",creviceJump=" + F(cr) + ",dirtJump=" + F(di) + ",surfaceGradientJump=" + F(svGrad) + ",exposureGradientJump=" + F(exGrad) + ",creviceGradientJump=" + F(crGrad) + ",dirtGradientJump=" + F(diGrad) + ",structuralGradientJump=" + F(structuralGrad));
            }
        }

        private static float GradientJump(MassGenerator.FinalTriangleRecord a, MassGenerator.FinalTriangleRecord b, int channel, bool structural, EdgeKey edge)
        {
            Vector3 ga = TriangleScalarGradient(a, channel, structural);
            Vector3 gb = TriangleScalarGradient(b, channel, structural);
            float edgeLength = edge.Length / PositionQuantization;
            float range = Mathf.Max(0.05f, Mathf.Max(TriangleChannelRange(a, channel, structural), TriangleChannelRange(b, channel, structural)));
            return (ga - gb).magnitude * Mathf.Max(edgeLength, 0.00001f) / range;
        }

        private static Vector3 TriangleScalarGradient(MassGenerator.FinalTriangleRecord t, int channel, bool structural)
        {
            Vector4 va = structural ? t.StructuralA : t.MaskA;
            Vector4 vb = structural ? t.StructuralB : t.MaskB;
            Vector4 vc = structural ? t.StructuralC : t.MaskC;
            float sa = va[channel], sb = vb[channel], sc = vc[channel];
            Vector3 e1 = t.B - t.A, e2 = t.C - t.A;
            float d11 = Vector3.Dot(e1, e1), d22 = Vector3.Dot(e2, e2), d12 = Vector3.Dot(e1, e2);
            float det = d11 * d22 - d12 * d12;
            if (Mathf.Abs(det) <= Epsilon * Epsilon) return Vector3.zero;
            float c1 = ((sb - sa) * d22 - (sc - sa) * d12) / det;
            float c2 = ((sc - sa) * d11 - (sb - sa) * d12) / det;
            return e1 * c1 + e2 * c2;
        }

        private static float TriangleChannelRange(MassGenerator.FinalTriangleRecord t, int channel, bool structural)
        {
            Vector4 a = structural ? t.StructuralA : t.MaskA;
            Vector4 b = structural ? t.StructuralB : t.MaskB;
            Vector4 c = structural ? t.StructuralC : t.MaskC;
            float min = Mathf.Min(a[channel], Mathf.Min(b[channel], c[channel]));
            float max = Mathf.Max(a[channel], Mathf.Max(b[channel], c[channel]));
            return max - min;
        }

        private static float TriangleArea(Vector3 a, Vector3 b, Vector3 c) => Vector3.Cross(b - a, c - a).magnitude * 0.5f;
        private static float TriangleAspectRatio(Vector3 a, Vector3 b, Vector3 c, float area)
        {
            float ab = (b-a).magnitude, bc = (c-b).magnitude, ca = (a-c).magnitude;
            float longest = Mathf.Max(ab, Mathf.Max(bc, ca));
            float altitude = longest <= Epsilon ? 0f : 2f * area / longest;
            return altitude <= Epsilon ? float.PositiveInfinity : longest / altitude;
        }

        private static void CompareFinalMesh(MassGenerator.FinalTriangleRecord t, Job j, BevelResult r)
        {
            int o = t.TriangleIndex * 3;
            if (o + 2 >= j.Indices.Length)
            {
                r.FinalMeshMismatchCount++;
                return;
            }
            int ia = j.Indices[o];
            int ib = j.Indices[o + 1];
            int ic = j.Indices[o + 2];
            Vector3 gn = N(Vector3.Cross(
                j.Vertices[ib] - j.Vertices[ia],
                j.Vertices[ic] - j.Vertices[ia]));
            Vector3 capturedGn = N(t.GeometricNormal);
            Vector3 rn = N(j.Normals[ia] + j.Normals[ib] + j.Normals[ic]);
            bool uploadedDegenerate = gn == Vector3.zero;
            bool capturedDegenerate = capturedGn == Vector3.zero;
            if (uploadedDegenerate || capturedDegenerate)
            {
                r.DegenerateTriangleCount++;
                if (uploadedDegenerate != capturedDegenerate)
                {
                    r.FinalMeshMismatchCount++;
                }
            }
            else if (Angle(gn, capturedGn) > 0.05f)
            {
                r.FinalMeshMismatchCount++;
            }
            if (Angle(rn, t.RenderNormal) > 0.05f)
            {
                r.FinalMeshMismatchCount++;
            }
            if (j.Tangents.Length == j.Vertices.Length)
            {
                r.MaxTangentNormalAbsDot = Mathf.Max(
                    r.MaxTangentNormalAbsDot,
                    Mathf.Abs(Vector3.Dot(
                        j.Normals[ia],
                        new Vector3(
                            j.Tangents[ia].x,
                            j.Tangents[ia].y,
                            j.Tangents[ia].z))));
            }
        }

        private static void Add(Dictionary<EdgeKey,List<MassGenerator.FinalTriangleRecord>> d, MassGenerator.FinalTriangleRecord t, Vector3 a, Vector3 b) { var k=new EdgeKey(a,b); if(!d.TryGetValue(k,out var l)){l=new List<MassGenerator.FinalTriangleRecord>(2);d.Add(k,l);} l.Add(t); }
        private static Vector4 SharedEdgeMask(MassGenerator.FinalTriangleRecord t, EdgeKey e) { Vector4 s=Vector4.zero; int n=0; AddMask(t.A,t.MaskA,e,ref s,ref n); AddMask(t.B,t.MaskB,e,ref s,ref n); AddMask(t.C,t.MaskC,e,ref s,ref n); return n>0?s/n:s; }
        private static void AddMask(Vector3 p,Vector4 m,EdgeKey e,ref Vector4 s,ref int n){var q=new Q(p);if(e.Contains(q)){s+=m;n++;}}

        private sealed class BevelResult
        {
            internal int EdgeId,CandidateId=-1,ParentFaceA=-1,ParentFaceB=-1,TriangleCount,SurfaceGroupCount,RenderNormalClusters,GeometricNormalClusters,InternalRenderNormalJumpCount,InternalMaskJumpCount,ValueGradientJumpCount,StructuralGradientJumpCount,GeometricFacetRiskCount,SliverTriangleCount,LowLightNormalSensitivityCount,ParentConeViolationCount,ActiveLightDarkerThanBothCount,ActiveLightBrighterThanBothCount,FinalMeshMismatchCount,DegenerateTriangleCount,Severity;
            internal Vector3 ParentNormalA,ParentNormalB,SourceA,SourceB,ActiveLightDirection; internal float ParentNormalAngle,MaximumInternalRenderNormalJump,MaximumInternalGeometricNormalJump,MaxExposureJump,MaxCreviceJump,MaxDirtJump,MaxSurfaceVariationGradientJump,MaxExposureGradientJump,MaxCreviceGradientJump,MaxDirtGradientJump,MaxStructuralGradientJump,MinimumTriangleArea=float.PositiveInfinity,MaximumAspectRatio,MaxLowHighVisibilityRatio,AmbientStrength,DirectStrength,FlatNormalStrength,WorstParentConeDot=1f,ActiveParentA,ActiveParentB,ActiveBevelMin,ActiveBevelMax,ExposureMin,ExposureMax,CreviceMin,CreviceMax,DirtMin,DirtMax,MinStoredVsGeometricDot=1f,MaxTangentNormalAbsDot;
            internal int[] SurfaceGroups=Array.Empty<int>(); internal readonly List<int> TriangleIds=new(); internal readonly List<string> EdgeEvidence=new(),TriangleEvidence=new();
        }

        private enum Stage { CaptureGeneration, BuildIndices, AnalyzeBevels, Finalize }
        private readonly struct Q:IEquatable<Q>,IComparable<Q>{readonly int x,y,z;internal int X=>x;internal int Y=>y;internal int Z=>z;internal Q(Vector3 p){x=Mathf.RoundToInt(p.x*PositionQuantization);y=Mathf.RoundToInt(p.y*PositionQuantization);z=Mathf.RoundToInt(p.z*PositionQuantization);}public int CompareTo(Q o){int r=x.CompareTo(o.x);if(r!=0)return r;r=y.CompareTo(o.y);return r!=0?r:z.CompareTo(o.z);}public bool Equals(Q o)=>x==o.x&&y==o.y&&z==o.z;public override bool Equals(object o)=>o is Q q&&Equals(q);public override int GetHashCode()=>((x*397)^y)*397^z;public override string ToString()=>x+","+y+","+z;}
        private readonly struct EdgeKey:IEquatable<EdgeKey>{readonly Q a,b;internal EdgeKey(Vector3 x,Vector3 y){var qx=new Q(x);var qy=new Q(y);if(qx.CompareTo(qy)<=0){a=qx;b=qy;}else{a=qy;b=qx;}}internal bool Contains(Q q)=>a.Equals(q)||b.Equals(q);internal float Length{get{double dx=a.X-b.X,dy=a.Y-b.Y,dz=a.Z-b.Z;return (float)Math.Sqrt(dx*dx+dy*dy+dz*dz);}}public bool Equals(EdgeKey o)=>a.Equals(o.a)&&b.Equals(o.b);public override bool Equals(object o)=>o is EdgeKey e&&Equals(e);public override int GetHashCode()=>a.GetHashCode()*397^b.GetHashCode();public override string ToString()=>a+"|"+b;}
        private static int Cluster(IEnumerable<Vector3> values){var c=new List<Vector3>();foreach(var n0 in values){var n=N(n0);if(n==Vector3.zero)continue;if(!c.Any(x=>Mathf.Abs(Vector3.Dot(x,n))>0.99985f))c.Add(n);}return c.Count;}
        private static float Angle(Vector3 a,Vector3 b){a=N(a);b=N(b);return a==Vector3.zero||b==Vector3.zero?180f:Mathf.Acos(Mathf.Clamp(Vector3.Dot(a,b),-1f,1f))*Mathf.Rad2Deg;}
        private static Vector3 N(Vector3 v)=>v.sqrMagnitude>Epsilon*Epsilon?v.normalized:Vector3.zero;
        private static float Min3(float a,float b,float c)=>Mathf.Min(a,Mathf.Min(b,c)); private static float Max3(float a,float b,float c)=>Mathf.Max(a,Mathf.Max(b,c));
        private static string F(float v)=>v.ToString("R",CultureInfo.InvariantCulture); private static string V(Vector3 v)=>F(v.x)+","+F(v.y)+","+F(v.z); private static string V4(Vector4 v)=>F(v.x)+","+F(v.y)+","+F(v.z)+","+F(v.w);
        private static Vector4[] ReadUv(Mesh mesh,int channel,int count){var l=new List<Vector4>(count);mesh.GetUVs(channel,l);return l.Count==count?l.ToArray():Enumerable.Repeat(Vector4.zero,count).ToArray();}
        private static Vector3 ResolveMainLightDirectionLocal(Transform t){Light l=RenderSettings.sun;if(l==null)l=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude).FirstOrDefault(x=>x.type==LightType.Directional&&x.enabled);return l==null?Vector3.zero:t.InverseTransformDirection(-l.transform.forward).normalized;}
        private static float GetMaterial(Job job, string name, float fallback) => job.MaterialProperties.TryGetValue(name, out float value) ? value : fallback;
        private static void ReadMaterialProperties(Material m,Dictionary<string,float>d){if(m==null)return;string[] n={"_GeneratedMassGeometryEdgeWearEnabled","_GeneratedMassEdgeWearResponseStrength","_GeneratedMassEdgeWearBrightnessLift","_GeneratedMassEdgeWearTintStrength","_GeneratedMassWholeRockNormalStrength","_GeneratedMassWholeRockNormalScale","_GeneratedMassLightingTintInfluence","_ExposureTintStrength","_CreviceStrength","_BaseDarkeningStrength","_DirtDepositStrength","_Smoothness","_Metallic","_SpecularStrength","_AmbientStrength","_DirectStrength","_DiffuseWrap","_ShadowAmbientStrength","_FlatNormalStrength","_ReceiveShadows","_PixelVariation","_PixelVertexVariation","_PixelBroadVariation","_PixelEffectStrength","_PixelWarpStrength","_StoneMottleStrength","_StoneMottleScale","_StoneMottleSoftness","_BottomDarkenStrength","_EdgeDarkenStrength","_HighlightCompressStrength","_GeneratedMassSurfaceNormalStrength","_GeneratedMassSurfaceNormalScale"};foreach(string x in n)if(m.HasProperty(x))d[x]=m.GetFloat(x);}
    }
}
