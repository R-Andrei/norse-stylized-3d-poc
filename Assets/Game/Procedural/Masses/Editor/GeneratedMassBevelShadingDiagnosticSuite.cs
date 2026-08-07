using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ProgrammaticStylized3D.Geometry;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Geometry.Masses.Editor
{
    /// <summary>
    /// Integrated Generated Mass surface-causality audit.
    ///
    /// The production triangulation is deliberately not modified here. The
    /// suite captures the committed production mesh, classifies final triangles
    /// through one canonical double-precision contract, audits alternative
    /// triangulations without uploading them, runs a frozen-mesh material parity
    /// matrix, and performs shader-contribution isolation through audit-owned
    /// temporary materials and asynchronous GPU readback.
    /// </summary>
    internal static class GeneratedMassBevelShadingDiagnosticSuite
    {
        private const string ReportPath = "Library/GeneratedMassSurfaceCausalityAudit.txt";
        private const string CsvPath = "Library/GeneratedMassSurfaceCausalityAudit.csv";
        private const float PositionQuantization = 100000f;
        private const float ScalarEpsilon = 0.000001f;
        private static Job activeJob;
        private static string lastReport = string.Empty;
        private static string lastSummary = string.Empty;

        internal static bool IsRunning => activeJob != null;
        internal static bool HasReport => !string.IsNullOrEmpty(lastReport);
        internal static string LastSummary => lastSummary;
        internal static string ProgressText => activeJob == null
            ? string.Empty
            : activeJob.ProgressText;

        internal static void Start(
            GeneratedMass suspect,
            GeneratedMass reference = null)
        {
            if (activeJob != null ||
                suspect == null ||
                suspect.GeometryMeshFilter == null)
            {
                return;
            }
            if (reference == suspect) reference = null;
            if (reference != null && reference.GeometryMeshFilter == null)
            {
                UnityEngine.Debug.LogError(
                    "Surface-causality reference has no Generated Mass geometry filter.",
                    reference);
                return;
            }
            activeJob = new Job(suspect, reference);
            EditorApplication.update -= Advance;
            EditorApplication.update += Advance;
            WriteCheckpoint(activeJob, false, string.Empty);
        }

        internal static void Cancel()
        {
            if (activeJob != null) activeJob.CancelRequested = true;
        }

        internal static void CopyLastReport()
        {
            if (HasReport) EditorGUIUtility.systemCopyBuffer = lastReport;
        }

        private static void Advance()
        {
            Job job = activeJob;
            if (job == null) return;
            if (job.CancelRequested || job.Suspect.Target == null)
            {
                Finish(job, true, "cancelled");
                return;
            }
            if (EditorUtility.DisplayCancelableProgressBar(
                "Generated Mass Surface-Causality Suite",
                job.ProgressText,
                job.Progress01))
            {
                Finish(job, true, "cancelled");
                return;
            }

            try
            {
                int previousCompletedRenderPasses =
                    job.RenderAudit == null
                        ? 0
                        : job.RenderAudit.Results.Count;
                switch (job.Stage)
                {
                    case Stage.CaptureSuspect:
                        Capture(job.Suspect);
                        job.Stage = job.Reference == null
                            ? Stage.BuildSubjects
                            : Stage.CaptureReference;
                        job.CompletedUnits++;
                        WriteCheckpoint(job, false, string.Empty);
                        break;
                    case Stage.CaptureReference:
                        Capture(job.Reference);
                        job.Stage = Stage.BuildSubjects;
                        job.CompletedUnits++;
                        WriteCheckpoint(job, false, string.Empty);
                        break;
                    case Stage.BuildSubjects:
                        BuildSubject(job.Suspect);
                        if (job.Reference != null) BuildSubject(job.Reference);
                        job.TotalBevels =
                            job.Suspect.Bevels.Count +
                            (job.Reference == null ? 0 : job.Reference.Bevels.Count);
                        job.Stage = Stage.AnalyzeSubjects;
                        job.CompletedUnits++;
                        WriteCheckpoint(job, false, string.Empty);
                        break;
                    case Stage.AnalyzeSubjects:
                        if (!AnalyzeNextSubjectBevel(job))
                        {
                            job.Stage = Stage.PrepareRenderAudit;
                        }
                        else
                        {
                            job.CompletedUnits++;
                            WriteCheckpoint(job, false, string.Empty);
                        }
                        break;
                    case Stage.PrepareRenderAudit:
                        job.PrepareRenderAudit();
                        job.Stage = Stage.RenderAudit;
                        job.CompletedUnits++;
                        WriteCheckpoint(job, false, string.Empty);
                        break;
                    case Stage.RenderAudit:
                        if (job.RenderAudit.Advance())
                        {
                            job.Stage = Stage.Finalize;
                        }
                        if (job.RenderAudit.Results.Count !=
                            previousCompletedRenderPasses)
                        {
                            job.CompletedUnits++;
                            WriteCheckpoint(job, false, string.Empty);
                        }
                        break;
                    case Stage.Finalize:
                        Finish(job, false, string.Empty);
                        break;
                }
            }
            catch (Exception exception)
            {
                MassGenerator.EndBevelShadingDiagnosticCapture();
                Finish(
                    job,
                    true,
                    exception.GetType().Name + ":" + exception.Message +
                    "\n" + exception.StackTrace);
            }
        }

        private static void Capture(SubjectData subject)
        {
            subject.EnsureInitialRendererState();
            MassGenerator.BeginBevelShadingDiagnosticCapture();
            try
            {
                subject.Target.Regenerate();
            }
            finally
            {
                bool restored = false;
                try
                {
                    subject.Snapshot =
                        MassGenerator.EndBevelShadingDiagnosticCapture();
                    subject.RecordRendererStateAfterRegeneration();
                }
                finally
                {
                    restored = subject.RestoreInitialRendererState(
                        afterCapture: true);
                }
                if (!restored)
                {
                    throw new InvalidOperationException(
                        subject.Role +
                        " renderer material/property-block state could not be restored after diagnostic capture: " +
                        subject.MaterialRestoreError);
                }
            }
            subject.Mesh = subject.Target.GeometryMeshFilter.sharedMesh;
            subject.LoadFinalMesh();
        }

        private static void BuildSubject(SubjectData subject)
        {
            subject.BuildIndices();
            subject.BuildMaterialEvidence();
        }

        private static bool AnalyzeNextSubjectBevel(Job job)
        {
            if (job.Suspect.NextBevel < job.Suspect.Bevels.Count)
            {
                job.Suspect.AnalyzeNextBevel();
                return true;
            }
            if (job.Reference != null &&
                job.Reference.NextBevel < job.Reference.Bevels.Count)
            {
                job.Reference.AnalyzeNextBevel();
                return true;
            }
            return false;
        }

        private static void Finish(Job job, bool cancelled, string reason)
        {
            EditorApplication.update -= Advance;
            EditorUtility.ClearProgressBar();
            job.RestoreInitialRendererStates();
            string terminalReason = reason;
            try
            {
                job.RenderAudit?.Dispose();
            }
            catch (Exception exception)
            {
                terminalReason = string.IsNullOrEmpty(terminalReason)
                    ? "RenderAuditDispose:" + exception.GetType().Name + ":" +
                        exception.Message
                    : terminalReason + "\nRenderAuditDispose:" +
                        exception.GetType().Name + ":" + exception.Message;
            }
            string report = BuildReport(
                job,
                true,
                cancelled,
                terminalReason);
            Directory.CreateDirectory(
                Path.GetDirectoryName(ReportPath) ?? "Library");
            File.WriteAllText(ReportPath, report, Encoding.UTF8);
            if (job.RenderAudit != null)
            {
                File.WriteAllText(
                    CsvPath,
                    BuildPerTriangleCsv(job.RenderAudit),
                    Encoding.UTF8);
            }
            lastReport = report;
            EditorGUIUtility.systemCopyBuffer = report;
            lastSummary = cancelled
                ? "Surface-causality suite stopped; partial report copied."
                : "Surface-causality suite complete; report copied.";
            activeJob = null;
            UnityEngine.Debug.Log(lastSummary, job.Suspect.Target);
        }

        private static void WriteCheckpoint(
            Job job,
            bool terminal,
            string reason)
        {
            Directory.CreateDirectory(
                Path.GetDirectoryName(ReportPath) ?? "Library");
            File.WriteAllText(
                ReportPath,
                BuildReport(job, terminal, false, reason),
                Encoding.UTF8);
        }

        private static string BuildReport(
            Job job,
            bool terminal,
            bool cancelled,
            string reason)
        {
            StringBuilder builder = new StringBuilder(262144);
            builder.AppendLine("GENERATED MASS SURFACE-CAUSALITY SUITE");
            builder.AppendLine("contract=GM-SURFACE-5N-H2-KEYWORD-FREE-BRDF-DECOMPOSITION-READBACK-ALIGNMENT");
            builder.AppendLine("generatedUtc=" +
                DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            builder.AppendLine("terminal=" + (terminal ? 1 : 0));
            builder.AppendLine("cancelled=" + (cancelled ? 1 : 0));
            builder.AppendLine("terminalReason=" + reason);
            builder.AppendLine("stage=" + job.Stage);
            builder.AppendLine("productionTriangulationBaseline=PRE_5I_RESTORED");
            builder.AppendLine("productionTriangulationModifiedBySuite=0");
            builder.AppendLine("serializedSceneOrMaterialWrites=0");
            builder.AppendLine("sourceRendererMaterialStateRestoreAttempted=" +
                (job.FinalRendererRestoreAttempted ? 1 : 0));
            builder.AppendLine("sourceRendererMaterialStateRestored=" +
                (job.RendererMaterialStateRestored ? 1 : 0));
            builder.AppendLine("gpuReadbackMode=ASYNC");
            builder.AppendLine("checkpointing=1");
            builder.AppendLine("elapsedSeconds=" + F((float)job.Elapsed.TotalSeconds));
            builder.AppendLine("progress=" + F(job.Progress01));
            builder.AppendLine("estimatedRemainingSeconds=" +
                F((float)job.EstimatedRemaining.TotalSeconds));
            builder.AppendLine();

            AppendSubject(builder, job.Suspect);
            if (job.Reference != null) AppendSubject(builder, job.Reference);
            else
            {
                builder.AppendLine("[Reference subject]");
                builder.AppendLine("present=0");
                builder.AppendLine("parityMatrixAvailable=0");
                builder.AppendLine();
            }

            builder.AppendLine("[Material and shader parity]");
            if (job.RenderAudit != null)
            {
                builder.AppendLine(job.RenderAudit.BuildMaterialDiffReport());
            }
            else
            {
                builder.AppendLine("status=pending");
            }
            builder.AppendLine();

            builder.AppendLine("[Lighting and camera environment]");
            if (job.RenderAudit != null)
                builder.AppendLine(job.RenderAudit.BuildEnvironmentReport());
            else
                builder.AppendLine("status=pending");
            builder.AppendLine();

            builder.AppendLine("[Rendered causality tournament]");
            if (job.RenderAudit == null)
            {
                builder.AppendLine("status=pending");
            }
            else
            {
                builder.AppendLine("captureSize=" +
                    GeneratedMassSurfaceCausalityRenderAudit.CaptureSize);
                builder.AppendLine("completedCases=" +
                    job.RenderAudit.CompletedCases);
                builder.AppendLine("totalCases=" +
                    job.RenderAudit.TotalCases);
                builder.AppendLine("auxiliaryIdentityCases=" +
                    job.RenderAudit.AuxiliaryIdentityCases);
                builder.AppendLine("totalRenderPasses=" +
                    job.RenderAudit.TotalRenderPasses);
                builder.AppendLine("perTriangleCsv=" + CsvPath);
                foreach (
                    GeneratedMassSurfaceCausalityRenderAudit.CaseResult result
                    in job.RenderAudit.Results)
                {
                    builder.AppendLine(
                        "renderCase=" + result.Name +
                        ",mesh=" + result.MeshRole +
                        ",material=" + result.MaterialRole +
                        ",family=" + result.Family +
                        ",propertyBlock=" + result.PropertyBlockMode +
                        ",maskClass=" + result.MaskClass +
                        ",highIntensity=" + (result.HighIntensity ? 1 : 0) +
                        ",mode=" + result.CausalityMode +
                        ",maskDebug=" + result.MaskDebugMode +
                        ",ablation=" + (result.IsAblation ? 1 : 0) +
                        ",triangleIdentity=" +
                            (result.IsTriangleIdentity ? 1 : 0) +
                        ",brdfSweep=" + (result.IsBrdfSweep ? 1 : 0) +
                        ",adaptiveBrdf=" +
                            (result.IsAdaptiveBrdf ? 1 : 0) +
                        ",auxiliaryIdentity=" +
                            (result.IsAuxiliaryIdentity ? 1 : 0) +
                        ",countsTowardDecision=" +
                            (result.CountsTowardDecisionTotal ? 1 : 0) +
                        ",view=" + result.ViewName +
                        ",cameraAzimuthDegrees=" +
                            F(result.CameraAzimuthDegrees) +
                        ",direction=" + result.DirectionName +
                        ",lightDirectionLocal=" +
                            F(result.LightDirectionLocal.x) + "," +
                            F(result.LightDirectionLocal.y) + "," +
                            F(result.LightDirectionLocal.z) +
                        ",brdfVariant=" + result.BrdfVariant +
                        ",lambertPreflight=" +
                            (result.IsLambertPreflight ? 1 : 0) +
                        ",identityFlipRelativeToLighting=" +
                            (result.IdentityFlipRelativeToLighting ? 1 : 0) +
                        ",foregroundAlignmentIoU=" +
                            F(result.ForegroundAlignmentIoU) +
                        ",foregroundPixelCountDifferenceRatio=" +
                            F(result.ForegroundPixelCountDifferenceRatio) +
                        ",lightingForegroundPixels=" +
                            result.LightingForegroundPixelCount +
                        ",identityForegroundPixels=" +
                            result.IdentityForegroundPixelCount +
                        ",lambertContractValid=" +
                            (result.LambertContractValid ? 1 : 0) +
                        ",lambertEligibleTriangles=" +
                            result.LambertEligibleTriangleCount +
                        ",lambertPositiveResponseTriangles=" +
                            result.LambertPositiveResponseTriangleCount +
                        ",lambertScale=" + F(result.LambertScale) +
                        ",lambertNormalizedRmse=" +
                            F(result.LambertNormalizedRmse) +
                        ",lambertMeanForegroundLuma=" +
                            F(result.LambertMeanForegroundLuma) +
                        ",visibleTriangles=" +
                            result.TriangleStatistics.Count +
                        ",triangleIdentityContractValid=" +
                            (result.TriangleIdentityContractValid ? 1 : 0) +
                        ",triangleIdentityPixels=" +
                            result.TriangleIdentityPixelCount +
                        ",triangleIdentityInvalidPixels=" +
                            result.TriangleIdentityInvalidPixelCount +
                        ",triangleIdentityDistinctTriangles=" +
                            result.TriangleIdentityDistinctTriangleCount +
                        ",triangleIdentityForegroundWidth=" +
                            result.TriangleIdentityForegroundWidth +
                        ",triangleIdentityForegroundHeight=" +
                            result.TriangleIdentityForegroundHeight +
                        ",triangleIdentityCpuRoundTripFailures=" +
                            result.TriangleIdentityCpuRoundTripFailures +
                        ",triangleCoverageRatio=" +
                            F(result.TriangleCoverageRatio) +
                        ",nonFinitePixels=" +
                            result.NonFinitePixelCount +
                        ",internalEdges=" + result.TotalInternalEdges +
                        ",frontFacingEdges=" + result.FrontFacingEdges +
                        ",projectedEdges=" + result.ProjectedEdges +
                        ",samples=" + result.ValidFacetSamples +
                        ",flipY=" + (result.UsedFlippedReadback ? 1 : 0) +
                        ",meanGradientJump=" + F(result.MeanGradientJump) +
                        ",p90GradientJump=" + F(result.P90GradientJump) +
                        ",maximumGradientJump=" + F(result.MaximumGradientJump) +
                        ",meanValueStep=" + F(result.MeanValueStep) +
                        ",meanRawGradientJump=" +
                            F(result.MeanRawGradientJump) +
                        ",p90RawGradientJump=" +
                            F(result.P90RawGradientJump) +
                        ",meanColorGradientJump=" +
                            F(result.MeanColorGradientJump) +
                        ",p90ColorGradientJump=" +
                            F(result.P90ColorGradientJump) +
                        ",facetScore=" + F(result.FacetScore) +
                        ",meanMaskedLuma=" + F(result.MeanMaskedLuma) +
                        ",saturatedMaskedPixelFraction=" +
                            F(result.SaturatedMaskedPixelFraction) +
                        ",ablationComparable=" +
                            (result.ComparableForAblationRanking ? 1 : 0) +
                        ",ablationExclusion=" +
                            result.AblationExclusionReason +
                        ",reductionFromBaseline=" +
                            F(result.ReductionFromBaseline) +
                        ",bevelParentSamples=" +
                            result.ValidBevelParentSamples +
                        ",bevelOutsideParentEnvelope=" +
                            result.BevelOutsideParentEnvelopeCount +
                        ",meanBevelOutsideEnvelope=" +
                            F(result.MeanBevelOutsideParentEnvelopeMagnitude) +
                        ",maximumBevelOutsideEnvelope=" +
                            F(result.MaximumBevelOutsideParentEnvelopeMagnitude) +
                        ",readbackError=" + (result.ReadbackError ? 1 : 0) +
                        ",error=" + result.Error);
                    foreach (
                        GeneratedMassSurfaceCausalityRenderAudit.SurfaceClassStatistics
                        statistics in result.ClassStatistics.Values
                            .OrderBy(item => item.Class))
                    {
                        builder.AppendLine(
                            "renderClass=" + result.Name +
                            ",class=" + statistics.Class +
                            ",pixels=" + statistics.PixelCount +
                            ",meanLuma=" + F(statistics.MeanLuma) +
                            ",p10Luma=" + F(statistics.P10Luma) +
                            ",medianLuma=" + F(statistics.MedianLuma) +
                            ",p90Luma=" + F(statistics.P90Luma) +
                            ",relativeToWhole=" +
                                F(statistics.RelativeToWhole));
                    }
                    if (result.Name ==
                            "SUSPECT_MESH__SUSPECT_RENDERER_STATE__LOW" ||
                        result.Name == "SAME_MESH__LEGACY_MATERIAL__LOW" ||
                        result.Family == "PreLightStage" ||
                        result.IsBrdfSweep)
                    {
                        int sampleLimit = result.IsBrdfSweep
                            ? int.MaxValue
                            : 16;
                        foreach (var sample in result.BevelParentSamples
                            .OrderByDescending(item =>
                                item.OutsideEnvelopeMagnitude)
                            .Take(sampleLimit))
                        {
                            builder.AppendLine(
                                "bevelParentSample=" + result.Name +
                                ",logicalBevel=" + sample.LogicalBevelId +
                                ",sample=" + sample.SampleIndex +
                                ",parentA=" + F(sample.ParentALuma) +
                                ",bevel=" + F(sample.BevelLuma) +
                                ",parentB=" + F(sample.ParentBLuma) +
                                ",transition=" +
                                    F(sample.NormalizedTransition) +
                                ",outsideEnvelope=" +
                                    F(sample.OutsideEnvelopeMagnitude) +
                                ",ordering=" + sample.Ordering);
                        }
                    }
                }
                GeneratedMassSurfaceCausalityRenderAudit.Summary summary =
                    job.RenderAudit.FinalSummary;
                if (summary != null)
                {
                    builder.AppendLine("ownership=" + summary.Ownership);
                    builder.AppendLine("ownershipConfidence=" +
                        F(summary.OwnershipConfidence));
                    builder.AppendLine("materialEffect=" +
                        F(summary.MaterialEffect));
                    builder.AppendLine("meshEffect=" +
                        F(summary.MeshEffect));
                    builder.AppendLine("interactionEffect=" +
                        F(summary.InteractionEffect));
                    builder.AppendLine("highIntensitySuppression=" +
                        F(summary.HighIntensitySuppression));
                    builder.AppendLine("highIntensityNoPostSuppression=" +
                        F(summary.HighIntensityNoPostSuppression));
                    builder.AppendLine("suspectBaselineFacetScore=" +
                        F(summary.SuspectBaselineScore));
                    builder.AppendLine("referenceBaselineFacetScore=" +
                        F(summary.ReferenceBaselineScore));
                    builder.AppendLine("suspectMeshMaterialAssetEffect=" +
                        F(summary.SuspectMeshMaterialAssetEffect));
                    builder.AppendLine("suspectMeshPropertyBlockEffect=" +
                        F(summary.SuspectMeshPropertyBlockEffect));
                    builder.AppendLine("referenceMeshMaterialAssetEffect=" +
                        F(summary.ReferenceMeshMaterialAssetEffect));
                    builder.AppendLine("referenceMeshPropertyBlockEffect=" +
                        F(summary.ReferenceMeshPropertyBlockEffect));
                    builder.AppendLine("legacyControlAvailable=" +
                        (summary.LegacyControlAvailable ? 1 : 0));
                    builder.AppendLine("legacyMaterial=" +
                        summary.LegacyMaterialName);
                    builder.AppendLine("legacyShader=" +
                        summary.LegacyShaderName);
                    builder.AppendLine("currentSourceRelativeResponse=" +
                        F(summary.CurrentSourceRelativeResponse));
                    builder.AppendLine("currentBevelRelativeResponse=" +
                        F(summary.CurrentBevelRelativeResponse));
                    builder.AppendLine("legacySourceRelativeResponse=" +
                        F(summary.LegacySourceRelativeResponse));
                    builder.AppendLine("legacyBevelRelativeResponse=" +
                        F(summary.LegacyBevelRelativeResponse));
                    builder.AppendLine("sourceRelativeDeltaFromLegacy=" +
                        F(summary.SourceRelativeDeltaFromLegacy));
                    builder.AppendLine("bevelRelativeDeltaFromLegacy=" +
                        F(summary.BevelRelativeDeltaFromLegacy));
                    builder.AppendLine("comparedBevelParentSamples=" +
                        summary.ComparedBevelParentSamples);
                    builder.AppendLine("orderingMismatchAgainstLegacyCount=" +
                        summary.OrderingMismatchAgainstLegacyCount);
                    builder.AppendLine("meanTransitionDeviationAgainstLegacy=" +
                        F(summary.MeanTransitionDeviationAgainstLegacy));
                    builder.AppendLine("firstDivergentStage=" +
                        summary.FirstDivergentStage);
                    builder.AppendLine("surfaceLightingOwnership=" +
                        summary.SurfaceLightingOwnership);
                    builder.AppendLine("propertyBlockMismatchReduction=" +
                        F(summary.PropertyBlockMismatchReduction));
                    builder.AppendLine("matchedMainDirectClassMismatch=" +
                        F(summary.MatchedMainDirectClassMismatch));
                    builder.AppendLine("matchedIndirectClassMismatch=" +
                        F(summary.MatchedIndirectClassMismatch));
                    builder.AppendLine("currentSourceFinalToPrelightResponse=" +
                        F(summary.CurrentSourceFinalToPrelightResponse));
                    builder.AppendLine("currentBevelFinalToPrelightResponse=" +
                        F(summary.CurrentBevelFinalToPrelightResponse));
                    builder.AppendLine("currentBevelMinusSourceLightingResponse=" +
                        F(summary.CurrentBevelMinusSourceLightingResponse));
                    builder.AppendLine("currentSourceMainDirectResponse=" +
                        F(summary.CurrentSourceMainDirectResponse));
                    builder.AppendLine("currentBevelMainDirectResponse=" +
                        F(summary.CurrentBevelMainDirectResponse));
                    builder.AppendLine("predictedSourceMainDiffuse=" +
                        F(summary.PredictedSourceMainDiffuse));
                    builder.AppendLine("predictedBevelMainDiffuse=" +
                        F(summary.PredictedBevelMainDiffuse));
                    builder.AppendLine("expectedBevelToSourceMainDiffuseRatio=" +
                        F(summary.ExpectedBevelToSourceMainDiffuseRatio));
                    builder.AppendLine("observedBevelToSourceMainDirectRatio=" +
                        F(summary.ObservedBevelToSourceMainDirectRatio));
                    builder.AppendLine("mainDirectNormalPredictionResidual=" +
                        F(summary.MainDirectNormalPredictionResidual));
                    builder.AppendLine("generatedNormalClassMismatchReduction=" +
                        F(summary.GeneratedNormalClassMismatchReduction));
                    builder.AppendLine("additionalLightsClassMismatchReduction=" +
                        F(summary.AdditionalLightsClassMismatchReduction));
                    builder.AppendLine("specularClassMismatchReduction=" +
                        F(summary.SpecularClassMismatchReduction));
                    builder.AppendLine("brdfSweepAvailable=" +
                        (summary.BrdfSweepAvailable ? 1 : 0));
                    builder.AppendLine("brdfComparedDirections=" +
                        summary.BrdfComparedDirections);
                    builder.AppendLine("brdfWorstDirection=" +
                        summary.BrdfWorstDirection);
                    builder.AppendLine("brdfCurrentMeanAbsoluteResidual=" +
                        F(summary.BrdfCurrentMeanAbsoluteResidual));
                    builder.AppendLine("brdfDielectricMeanAbsoluteResidual=" +
                        F(summary.BrdfDielectricMeanAbsoluteResidual));
                    builder.AppendLine("brdfDielectricResidualReduction=" +
                        F(summary.BrdfDielectricResidualReduction));
                    builder.AppendLine("brdfCurrentOverResponseCount=" +
                        summary.BrdfCurrentOverResponseCount);
                    builder.AppendLine("brdfCurrentUnderResponseCount=" +
                        summary.BrdfCurrentUnderResponseCount);
                    builder.AppendLine("brdfDielectricOverResponseCount=" +
                        summary.BrdfDielectricOverResponseCount);
                    builder.AppendLine("brdfDielectricUnderResponseCount=" +
                        summary.BrdfDielectricUnderResponseCount);
                    builder.AppendLine("brdfCurrentOrderingInversionCount=" +
                        summary.BrdfCurrentOrderingInversionCount);
                    builder.AppendLine("brdfDielectricOrderingInversionCount=" +
                        summary.BrdfDielectricOrderingInversionCount);
                    builder.AppendLine("brdfDielectricImprovedDirectionCount=" +
                        summary.BrdfDielectricImprovedDirectionCount);
                    builder.AppendLine("brdfAdaptiveDirectionCount=" +
                        summary.BrdfAdaptiveDirectionCount);
                    builder.AppendLine(
                        "brdfActualCurrentMeanAbsoluteResidual=" +
                        F(summary.BrdfActualCurrentMeanAbsoluteResidual));
                    builder.AppendLine(
                        "brdfActualDielectricMeanAbsoluteResidual=" +
                        F(summary.BrdfActualDielectricMeanAbsoluteResidual));
                    builder.AppendLine(
                        "brdfActualDielectricResidualReduction=" +
                        F(summary.BrdfActualDielectricResidualReduction));
                    builder.AppendLine(
                        "brdfDiffuseEnergyMatchedMeanAbsoluteResidual=" +
                        F(summary.BrdfDiffuseEnergyMatchedMeanAbsoluteResidual));
                    builder.AppendLine("brdfWorkflowVerdict=" +
                        summary.BrdfWorkflowVerdict);
                    foreach (var direction in summary.BrdfDirections
                        .OrderBy(item => item.DirectionName,
                            StringComparer.Ordinal))
                    {
                        builder.AppendLine(
                            "brdfDirection=" + direction.DirectionName +
                            ",lightDirectionLocal=" +
                                F(direction.LightDirectionLocal.x) + "," +
                                F(direction.LightDirectionLocal.y) + "," +
                                F(direction.LightDirectionLocal.z) +
                            ",triangles=" + direction.ComparedTriangles +
                            ",sourceTriangles=" +
                                direction.ComparedSourceTriangles +
                            ",bevelTriangles=" +
                                direction.ComparedBevelTriangles +
                            ",evaluable=" +
                                (direction.IsEvaluable ? 1 : 0) +
                            ",currentOver=" +
                                direction.CurrentOverResponseCount +
                            ",currentUnder=" +
                                direction.CurrentUnderResponseCount +
                            ",dielectricOver=" +
                                direction.DielectricOverResponseCount +
                            ",dielectricUnder=" +
                                direction.DielectricUnderResponseCount +
                            ",currentOrderingInversions=" +
                                direction.CurrentOrderingInversionCount +
                            ",dielectricOrderingInversions=" +
                                direction.DielectricOrderingInversionCount +
                            ",currentMeanAbsResidual=" +
                                F(direction.CurrentMeanAbsoluteResidual) +
                            ",currentP90AbsResidual=" +
                                F(direction.CurrentP90AbsoluteResidual) +
                            ",dielectricMeanAbsResidual=" +
                                F(direction.DielectricMeanAbsoluteResidual) +
                            ",dielectricP90AbsResidual=" +
                                F(direction.DielectricP90AbsoluteResidual) +
                            ",dielectricResidualReduction=" +
                                F(direction.DielectricResidualReduction) +
                            ",adaptiveAvailable=" +
                                (direction.AdaptiveStageAvailable ? 1 : 0) +
                            ",actualCurrentMeanAbsResidual=" +
                                F(direction.ActualCurrentMeanAbsoluteResidual) +
                            ",actualDielectricMeanAbsResidual=" +
                                F(direction.ActualDielectricMeanAbsoluteResidual) +
                            ",actualDielectricResidualReduction=" +
                                F(direction.ActualDielectricResidualReduction) +
                            ",diffuseEnergyMatchedMeanAbsResidual=" +
                                F(direction.DiffuseEnergyMatchedMeanAbsoluteResidual));
                        foreach (var triangle in direction.TriangleComparisons
                            .OrderBy(item => item.TriangleIndex))
                        {
                            builder.AppendLine(
                                "brdfTriangle=" + direction.DirectionName +
                                ",triangle=" + triangle.TriangleIndex +
                                ",class=" + triangle.SurfaceClass +
                                ",logicalBevel=" + triangle.LogicalBevelId +
                                ",pixels=" + triangle.PixelCount +
                                ",legacyRgb=" +
                                    F(triangle.LegacyLinearRgb.x) + "," +
                                    F(triangle.LegacyLinearRgb.y) + "," +
                                    F(triangle.LegacyLinearRgb.z) +
                                ",hlslF0_016Rgb=" +
                                    F(triangle.CurrentHlslLinearRgb.x) + "," +
                                    F(triangle.CurrentHlslLinearRgb.y) + "," +
                                    F(triangle.CurrentHlslLinearRgb.z) +
                                ",hlslF0_004Rgb=" +
                                    F(triangle.DielectricHlslLinearRgb.x) + "," +
                                    F(triangle.DielectricHlslLinearRgb.y) + "," +
                                    F(triangle.DielectricHlslLinearRgb.z) +
                                ",legacyLuma=" + F(triangle.LegacyLuma) +
                                ",hlslF0_016Luma=" +
                                    F(triangle.CurrentHlslLuma) +
                                ",hlslF0_004Luma=" +
                                    F(triangle.DielectricHlslLuma) +
                                ",signedResidual016=" +
                                    F(triangle.SignedResidualCurrent) +
                                ",signedResidual004=" +
                                    F(triangle.SignedResidualDielectric) +
                                ",rgbResidual016=" +
                                    F(triangle.RgbResidualCurrent) +
                                ",rgbResidual004=" +
                                    F(triangle.RgbResidualDielectric));
                        }
                    }
                    builder.AppendLine("expectedDecisionCases=" +
                        summary.ExpectedDecisionCases);
                    builder.AppendLine("completedDecisionCases=" +
                        summary.CompletedDecisionCases);
                    builder.AppendLine("auxiliaryIdentityCases=" +
                        summary.AuxiliaryIdentityCases);
                    builder.AppendLine("readbackErrorCount=" +
                        summary.ReadbackErrorCount);
                    builder.AppendLine("minimumCaseCoverageRatio=" +
                        F(summary.MinimumCaseCoverageRatio));
                    builder.AppendLine("neutralDiffuseMeanAbsResidual=" +
                        F(summary.NeutralDiffuseMeanAbsoluteResidual));
                    builder.AppendLine("stageAEvaluableDirectionCount=" +
                        summary.StageAEvaluableDirectionCount);
                    builder.AppendLine("stageAF0ResidualReduction=" +
                        F(summary.StageAF0ResidualReduction));
                    builder.AppendLine("stageAF0ImprovedDirectionCount=" +
                        summary.StageAF0ImprovedDirectionCount);
                    builder.AppendLine("stageBStoredF0MinimumReduction=" +
                        F(summary.StageBStoredF0MinimumReduction));
                    builder.AppendLine("stageBGeneratedNormalMeanReduction=" +
                        F(summary.StageBGeneratedNormalMeanReduction));
                    builder.AppendLine(
                        "stageBActualStoredDielectricMeanAbsResidual=" +
                        F(summary.StageBActualStoredDielectricMeanAbsoluteResidual));
                    builder.AppendLine("lambertContractValid=" +
                        (summary.LambertContractValid ? 1 : 0));
                    builder.AppendLine("lambertEligibleTriangles=" +
                        summary.LambertEligibleTriangleCount);
                    builder.AppendLine("lambertPositiveResponseTriangles=" +
                        summary.LambertPositiveResponseTriangleCount);
                    builder.AppendLine("lambertScale=" +
                        F(summary.LambertScale));
                    builder.AppendLine("lambertNormalizedRmse=" +
                        F(summary.LambertNormalizedRmse));
                    builder.AppendLine("lambertMeanForegroundLuma=" +
                        F(summary.LambertMeanForegroundLuma));
                    builder.AppendLine("minimumForegroundAlignmentIoU=" +
                        F(summary.MinimumForegroundAlignmentIoU));
                    builder.AppendLine(
                        "maximumForegroundPixelCountDifferenceRatio=" +
                        F(summary.MaximumForegroundPixelCountDifferenceRatio));
                    builder.AppendLine("stageCMinimumF0Reduction=" +
                        F(summary.StageCMinimumF0Reduction));
                    builder.AppendLine("stageCGeneratedNormalMeanReduction=" +
                        F(summary.StageCGeneratedNormalMeanReduction));
                    builder.AppendLine("indirectCurrentMeanAbsResidual=" +
                        F(summary.IndirectCurrentMeanAbsoluteResidual));
                    builder.AppendLine("indirectDielectricMeanAbsResidual=" +
                        F(summary.IndirectDielectricMeanAbsoluteResidual));
                    builder.AppendLine("actualSceneCurrentMeanAbsResidual=" +
                        F(summary.ActualSceneCurrentMeanAbsoluteResidual));
                    builder.AppendLine("actualSceneDielectricMeanAbsResidual=" +
                        F(summary.ActualSceneDielectricMeanAbsoluteResidual));
                    builder.AppendLine("completenessFailure=" +
                        summary.CompletenessFailure);
                    builder.AppendLine("dominantContributor=" +
                        summary.DominantContributor);
                    builder.AppendLine("dominantContributorReduction=" +
                        F(summary.DominantContributorReduction));
                    foreach (var ranked in summary.RankedContributors.Take(12))
                    {
                        builder.AppendLine(
                            "contributorRank=" + ranked.Name +
                            ",family=" + ranked.Family +
                            ",reduction=" + F(ranked.ReductionFromBaseline) +
                            ",score=" + F(ranked.FacetScore));
                    }
                }
            }
            builder.AppendLine();

            builder.AppendLine("[Decision]");
            string suspectGeometry = job.Suspect.GeometryStatus;
            string referenceGeometry = job.Reference == null
                ? "NOT_EVALUATED"
                : job.Reference.GeometryStatus;
            builder.AppendLine("suspectGeometryStatus=" + suspectGeometry);
            builder.AppendLine("referenceGeometryStatus=" + referenceGeometry);
            GeneratedMassSurfaceCausalityRenderAudit.Summary finalSummary =
                job.RenderAudit?.FinalSummary;
            builder.AppendLine("causalOwnership=" +
                (finalSummary == null ? "PENDING" : finalSummary.Ownership));
            builder.AppendLine("productionFixSelected=0");
            builder.AppendLine("decision=" + ResolveDecision(job));
            builder.AppendLine();

            builder.AppendLine("[Interpretation contract]");
            builder.AppendLine("- literal degeneracy, numerical under-resolution, and extreme sliver conditioning are separate classifications.");
            builder.AppendLine("- every uploaded and captured triangle is evaluated by MassGenerator.EvaluateFinalTriangleQuality; Vector3.normalized is not a validity oracle.");
            builder.AppendLine("- alternative triangulations are audit-only and never replace the production mesh in this suite.");
            builder.AppendLine("- the primary behavioural control applies Assets/Game/Demo/Materials/Stone/M_PixelStone.mat and the current HLSL material to the exact same frozen mesh; an optional selected reference mesh remains supplementary.");
            builder.AppendLine("- subject capture may trigger the named Stone Surface Profile to reassert its HLSL material during regeneration; the suite snapshots every source material slot plus global/per-slot property blocks before capture, restores them immediately and at finalization, and treats any restoration mismatch as terminal failure.");
            builder.AppendLine("- all rendered cases use hidden temporary meshes, renderers, cameras, material clones, and property blocks; the suite never assigns a diagnostic material or shader to the source renderer.");
            builder.AppendLine("- a dedicated editor-only depth-tested identity shader owns visible provenance attribution; alternate camera views receive separate validated identity maps and isolated class-only meshes are not used.");
            builder.AppendLine("- a controlled Lambert preflight validates light publication, float readback, identity-to-lighting alignment, stored-normal attribution, and per-triangle reduction before Stage A is queued.");
            builder.AppendLine("- Stage A captures six keyword-free neutral stored-normal variants under 27 deterministic light directions: legacy/HLSL full response and black-albedo specular-only response at F0 0.16 and F0 0.04; diffuse is derived as full minus specular-only.");
            builder.AppendLine("- Stage B reruns the four worst directions with actual albedo and generated/stored normal pairs at both F0 values; Stage C repeats the two worst directions from two additional camera azimuths; Stage D captures indirect and actual-scene closure.");
            builder.AppendLine("- every lighting case uses linear floating-point GPU capture and preserves per-triangle RGB, luminance, signed residuals, parent IDs, stored-normal NdotL/NdotV/NdotH predictions, and ordering; any non-finite sample is terminal.");
            builder.AppendLine("- controlled stages disable fog, post-processing, shadows, additional lights, probes, ambient, reflections, and cookies as declared per case; actual-scene closure deliberately restores the scene environment.");
            builder.AppendLine("- the active selected mass is the suspect; when two masses are selected, the other selected mass is the reference.");
            builder.AppendLine("- a visual result can still override an inconclusive numerical classification; the suite reports evidence rather than silently authoring a fix.");
            return builder.ToString();
        }

        private static string BuildPerTriangleCsv(
            GeneratedMassSurfaceCausalityRenderAudit audit)
        {
            StringBuilder csv = new StringBuilder(1048576);
            csv.AppendLine(
                "view,lightDirection,variant,triangleIndex,surfaceClass," +
                "logicalBevelId,parentFaceA,parentFaceB,pixelCount," +
                "meanLinearR,meanLinearG,meanLinearB,meanLuma," +
                "storedNormalX,storedNormalY,storedNormalZ," +
                "predictedNdotL,predictedNdotV,predictedNdotH," +
                "decompositionAvailable,legacyDiffuse,legacySpecular,hlsl016Diffuse," +
                "hlsl016Specular,hlsl004Diffuse,hlsl004Specular," +
                "signedResidual016,signedResidual004," +
                "orderingLegacy,ordering016,ordering004");

            IReadOnlyList<GeneratedMassSurfaceCausalityRenderAudit.CaseResult>
                results = audit.Results;
            foreach (var result in results
                .Where(item => item.IsBrdfSweep && !item.IsLambertPreflight)
                .OrderBy(item => item.Family, StringComparer.Ordinal)
                .ThenBy(item => item.DirectionName, StringComparer.Ordinal)
                .ThenBy(item => item.ViewName, StringComparer.Ordinal)
                .ThenBy(item => item.BrdfVariant, StringComparer.Ordinal))
            {
                ResolveCsvComparisonCases(
                    results,
                    result,
                    out var legacyFull,
                    out var currentFull,
                    out var dielectricFull,
                    out var legacySpecularCase,
                    out var currentSpecularCase,
                    out var dielectricSpecularCase);
                foreach (var triangle in result.TriangleStatistics.Values
                    .OrderBy(item => item.TriangleIndex))
                {
                    float legacyFullLuma = GetTriangleLuma(
                        legacyFull,
                        triangle.TriangleIndex);
                    float currentFullLuma = GetTriangleLuma(
                        currentFull,
                        triangle.TriangleIndex);
                    float dielectricFullLuma = GetTriangleLuma(
                        dielectricFull,
                        triangle.TriangleIndex);
                    bool decompositionAvailable =
                        legacySpecularCase != null &&
                        currentSpecularCase != null &&
                        dielectricSpecularCase != null &&
                        legacySpecularCase.TriangleStatistics.ContainsKey(
                            triangle.TriangleIndex) &&
                        currentSpecularCase.TriangleStatistics.ContainsKey(
                            triangle.TriangleIndex) &&
                        dielectricSpecularCase.TriangleStatistics.ContainsKey(
                            triangle.TriangleIndex);
                    float legacySpecular = decompositionAvailable
                        ? GetTriangleLuma(
                            legacySpecularCase,
                            triangle.TriangleIndex)
                        : float.NaN;
                    float hlsl016Specular = decompositionAvailable
                        ? GetTriangleLuma(
                            currentSpecularCase,
                            triangle.TriangleIndex)
                        : float.NaN;
                    float hlsl004Specular = decompositionAvailable
                        ? GetTriangleLuma(
                            dielectricSpecularCase,
                            triangle.TriangleIndex)
                        : float.NaN;
                    float legacyDiffuse = decompositionAvailable
                        ? Mathf.Max(0f, legacyFullLuma - legacySpecular)
                        : float.NaN;
                    float hlsl016Diffuse = decompositionAvailable
                        ? Mathf.Max(0f, currentFullLuma - hlsl016Specular)
                        : float.NaN;
                    float hlsl004Diffuse = decompositionAvailable
                        ? Mathf.Max(0f, dielectricFullLuma - hlsl004Specular)
                        : float.NaN;
                    float denominator = Mathf.Max(0.02f, legacyFullLuma);
                    float signed016 =
                        (currentFullLuma - legacyFullLuma) / denominator;
                    float signed004 =
                        (dielectricFullLuma - legacyFullLuma) / denominator;

                    csv.Append(Csv(result.ViewName)).Append(',')
                        .Append(Csv(result.DirectionName)).Append(',')
                        .Append(Csv(result.BrdfVariant)).Append(',')
                        .Append(triangle.TriangleIndex).Append(',')
                        .Append(triangle.SurfaceClass).Append(',')
                        .Append(triangle.LogicalBevelId).Append(',')
                        .Append(triangle.ParentFaceA).Append(',')
                        .Append(triangle.ParentFaceB).Append(',')
                        .Append(triangle.PixelCount).Append(',')
                        .Append(F(triangle.MeanLinearRgb.x)).Append(',')
                        .Append(F(triangle.MeanLinearRgb.y)).Append(',')
                        .Append(F(triangle.MeanLinearRgb.z)).Append(',')
                        .Append(F(triangle.MeanLuma)).Append(',')
                        .Append(F(triangle.StoredNormalLocal.x)).Append(',')
                        .Append(F(triangle.StoredNormalLocal.y)).Append(',')
                        .Append(F(triangle.StoredNormalLocal.z)).Append(',')
                        .Append(F(triangle.PredictedNdotL)).Append(',')
                        .Append(F(triangle.PredictedNdotV)).Append(',')
                        .Append(F(triangle.PredictedNdotH)).Append(',')
                        .Append(decompositionAvailable ? 1 : 0).Append(',')
                        .Append(CsvFloat(legacyDiffuse)).Append(',')
                        .Append(CsvFloat(legacySpecular)).Append(',')
                        .Append(CsvFloat(hlsl016Diffuse)).Append(',')
                        .Append(CsvFloat(hlsl016Specular)).Append(',')
                        .Append(CsvFloat(hlsl004Diffuse)).Append(',')
                        .Append(CsvFloat(hlsl004Specular)).Append(',')
                        .Append(F(signed016)).Append(',')
                        .Append(F(signed004)).Append(',')
                        .Append(Csv(GetOrdering(
                            legacyFull,
                            triangle.LogicalBevelId))).Append(',')
                        .Append(Csv(GetOrdering(
                            currentFull,
                            triangle.LogicalBevelId))).Append(',')
                        .Append(Csv(GetOrdering(
                            dielectricFull,
                            triangle.LogicalBevelId)))
                        .AppendLine();
                }
            }
            return csv.ToString();
        }

        private static void ResolveCsvComparisonCases(
            IReadOnlyList<GeneratedMassSurfaceCausalityRenderAudit.CaseResult>
                results,
            GeneratedMassSurfaceCausalityRenderAudit.CaseResult source,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult legacyFull,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult currentFull,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult
                dielectricFull,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult
                legacySpecular,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult
                currentSpecular,
            out GeneratedMassSurfaceCausalityRenderAudit.CaseResult
                dielectricSpecular)
        {
            string legacyVariant;
            string currentVariant;
            string dielectricVariant;
            string legacySpecularVariant = string.Empty;
            string currentSpecularVariant = string.Empty;
            string dielectricSpecularVariant = string.Empty;
            switch (source.Family)
            {
                case "StageA":
                    legacyVariant = "A_LEGACY_NEUTRAL_FULL";
                    currentVariant = "A_HLSL016_NEUTRAL_STORED";
                    dielectricVariant = "A_HLSL004_NEUTRAL_STORED";
                    legacySpecularVariant = "A_LEGACY_BLACK_SPECULAR";
                    currentSpecularVariant =
                        "A_HLSL016_BLACK_SPECULAR_STORED";
                    dielectricSpecularVariant =
                        "A_HLSL004_BLACK_SPECULAR_STORED";
                    break;
                case "StageB":
                    legacyVariant = "B_LEGACY_ACTUAL_FULL";
                    currentVariant = "B_HLSL016_ACTUAL_STORED";
                    dielectricVariant = "B_HLSL004_ACTUAL_STORED";
                    break;
                case "StageC":
                    legacyVariant = "C_LEGACY_ACTUAL_FULL";
                    currentVariant = "C_HLSL016_ACTUAL_GENERATED";
                    dielectricVariant = "C_HLSL004_ACTUAL_GENERATED";
                    break;
                default:
                    bool indirect = source.BrdfVariant.IndexOf(
                        "INDIRECT",
                        StringComparison.Ordinal) >= 0;
                    legacyVariant = indirect
                        ? "D_LEGACY_INDIRECT_ONLY"
                        : "D_LEGACY_ACTUAL_SCENE";
                    currentVariant = indirect
                        ? "D_HLSL016_INDIRECT_ONLY"
                        : "D_HLSL016_ACTUAL_SCENE";
                    dielectricVariant = indirect
                        ? "D_HLSL004_INDIRECT_ONLY"
                        : "D_HLSL004_ACTUAL_SCENE";
                    break;
            }

            legacyFull = FindCsvCase(results, source, legacyVariant);
            currentFull = FindCsvCase(results, source, currentVariant);
            dielectricFull = FindCsvCase(results, source, dielectricVariant);
            legacySpecular = string.IsNullOrEmpty(legacySpecularVariant)
                ? null
                : FindCsvCase(results, source, legacySpecularVariant);
            currentSpecular = string.IsNullOrEmpty(currentSpecularVariant)
                ? null
                : FindCsvCase(results, source, currentSpecularVariant);
            dielectricSpecular = string.IsNullOrEmpty(
                dielectricSpecularVariant)
                ? null
                : FindCsvCase(results, source, dielectricSpecularVariant);
        }

        private static GeneratedMassSurfaceCausalityRenderAudit.CaseResult
            FindCsvCase(
                IReadOnlyList<GeneratedMassSurfaceCausalityRenderAudit.CaseResult>
                    results,
                GeneratedMassSurfaceCausalityRenderAudit.CaseResult source,
                string variant)
        {
            return results.FirstOrDefault(item =>
                item.IsBrdfSweep &&
                string.Equals(
                    item.DirectionName,
                    source.DirectionName,
                    StringComparison.Ordinal) &&
                string.Equals(
                    item.ViewName,
                    source.ViewName,
                    StringComparison.Ordinal) &&
                string.Equals(
                    item.BrdfVariant,
                    variant,
                    StringComparison.Ordinal));
        }

        private static float GetTriangleLuma(
            GeneratedMassSurfaceCausalityRenderAudit.CaseResult result,
            int triangleIndex)
        {
            return result != null &&
                result.TriangleStatistics.TryGetValue(
                    triangleIndex,
                    out var triangle)
                        ? triangle.MeanLuma
                        : 0f;
        }

        private static string GetOrdering(
            GeneratedMassSurfaceCausalityRenderAudit.CaseResult result,
            int logicalBevelId)
        {
            if (result == null || logicalBevelId < 0)
            {
                return string.Empty;
            }
            return result.BevelParentSamples
                .FirstOrDefault(item =>
                    item.LogicalBevelId == logicalBevelId)
                ?.Ordering ?? string.Empty;
        }


        private static string CsvFloat(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value)
                ? string.Empty
                : F(value);
        }

        private static string Csv(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static string ResolveDecision(Job job)
        {
            if (job.FinalRendererRestoreAttempted &&
                !job.RendererMaterialStateRestored)
            {
                return "SOURCE_RENDERER_MATERIAL_STATE_RESTORE_FAILURE";
            }
            if (!job.Suspect.CaptureContractValid)
                return "SUSPECT_CAPTURE_CONTRACT_FAILURE";
            if (job.Reference != null && !job.Reference.CaptureContractValid)
                return "REFERENCE_CAPTURE_CONTRACT_FAILURE";
            if (job.Stage != Stage.Finalize &&
                job.RenderAudit?.FinalSummary == null)
                return "IN_PROGRESS";
            if (job.Suspect.StructurallyInvalidTriangles > 0 ||
                (job.Reference != null &&
                 job.Reference.StructurallyInvalidTriangles > 0))
                return "STRUCTURAL_GEOMETRY_DEFECT_CONFIRMED";
            if (job.Suspect.VertexFrameStatus != "VERTEX_FRAME_VALID" ||
                (job.Reference != null &&
                 job.Reference.VertexFrameStatus != "VERTEX_FRAME_VALID"))
                return "VERTEX_FRAME_DEFECT_CONFIRMED";
            string ownership = job.RenderAudit?.FinalSummary?.Ownership;
            return string.IsNullOrEmpty(ownership)
                ? "INCONCLUSIVE"
                : ownership;
        }

        private static void AppendSubject(
            StringBuilder builder,
            SubjectData subject)
        {
            builder.AppendLine("[" + subject.Role + " subject]");
            builder.AppendLine("present=1");
            builder.AppendLine("object=" +
                (subject.Target == null ? "<destroyed>" : subject.Target.name));
            builder.AppendLine("mesh=" +
                (subject.Mesh == null ? "<none>" : subject.Mesh.name));
            builder.AppendLine("material=" + subject.MaterialName);
            builder.AppendLine("shader=" + subject.ShaderName);
            builder.AppendLine("initialRendererMaterial=" +
                (subject.InitialRendererState == null
                    ? "<unavailable>"
                    : subject.InitialRendererState.PrimaryMaterialName));
            builder.AppendLine("initialRendererShader=" +
                (subject.InitialRendererState == null
                    ? "<unavailable>"
                    : subject.InitialRendererState.PrimaryShaderName));
            builder.AppendLine("observedMaterialAfterRegeneration=" +
                subject.ObservedMaterialAfterRegeneration);
            builder.AppendLine("observedShaderAfterRegeneration=" +
                subject.ObservedShaderAfterRegeneration);
            builder.AppendLine("materialMutationObservedDuringCapture=" +
                (subject.MaterialMutationObservedDuringCapture ? 1 : 0));
            builder.AppendLine("materialStateRestoredAfterCapture=" +
                (subject.MaterialStateRestoredAfterCapture ? 1 : 0));
            builder.AppendLine("materialStateRestoredAtFinalize=" +
                (subject.MaterialStateRestoredAtFinalize ? 1 : 0));
            builder.AppendLine("materialRestoreError=" +
                subject.MaterialRestoreError);
            builder.AppendLine("rendererMaterialStateFingerprint=" +
                (subject.InitialRendererState == null
                    ? "<unavailable>"
                    : subject.InitialRendererState.Fingerprint.ToString("X16")));
            builder.AppendLine("vertices=" + subject.Vertices.Length);
            builder.AppendLine("triangles=" + subject.Indices.Length / 3);
            builder.AppendLine("captureContractValid=" +
                (subject.CaptureContractValid ? 1 : 0));
            builder.AppendLine("acceptedBuildId=" +
                (subject.AcceptedBuild == null
                    ? -1
                    : subject.AcceptedBuild.BuildId));
            builder.AppendLine("logicalBevelsCaptured=" +
                (subject.AcceptedBuild == null
                    ? 0
                    : subject.AcceptedBuild.LogicalBevels.Count));
            builder.AppendLine("unmappedBevelTriangles=" +
                subject.UnmappedBevelTriangles);
            builder.AppendLine("captureToFinalMeshMismatches=" +
                subject.CaptureToFinalMeshMismatches);
            builder.AppendLine("acceptedMeshDataUploadMismatches=" +
                subject.AcceptedMeshDataUploadMismatches);
            builder.AppendLine("canonicalAccountingMismatch=" +
                subject.CanonicalAccountingMismatch);
            builder.AppendLine("exactDegenerateTriangles=" +
                subject.ExactDegenerateTriangles);
            builder.AppendLine("duplicateIndexTriangles=" +
                subject.DuplicateIndexTriangles);
            builder.AppendLine("coincidentPositionTriangles=" +
                subject.CoincidentPositionTriangles);
            builder.AppendLine("nonFiniteTriangles=" +
                subject.NonFiniteTriangles);
            builder.AppendLine("indexOutOfRangeTriangles=" +
                subject.IndexOutOfRangeTriangles);
            builder.AppendLine("windingInvalidTriangles=" +
                subject.WindingInvalidTriangles);
            builder.AppendLine("numericallyUnderResolvedTriangles=" +
                subject.NumericallyUnderResolvedTriangles);
            builder.AppendLine("extremeSliverTriangles=" +
                subject.ExtremeSliverTriangles);
            builder.AppendLine("conditionedTriangles=" +
                subject.ConditionedTriangles);
            builder.AppendLine("maximumAspectRatio=" +
                F((float)subject.MaximumAspectRatio));
            builder.AppendLine("minimumAngleDegrees=" +
                F((float)subject.MinimumAngleDegrees));
            builder.AppendLine("minimumNormalizedDoubleArea=" +
                F((float)subject.MinimumNormalizedDoubleArea));
            builder.AppendLine("maximumConditionedGeometricNormalJumpDeg=" +
                F(subject.MaximumConditionedGeometricNormalJump));
            builder.AppendLine("invalidNormalVertices=" +
                subject.InvalidNormalVertices);
            builder.AppendLine("invalidTangentVertices=" +
                subject.InvalidTangentVertices);
            builder.AppendLine("invalidTangentHandednessVertices=" +
                subject.InvalidTangentHandednessVertices);
            builder.AppendLine("maximumTangentNormalAbsDot=" +
                F(subject.MaximumTangentNormalAbsDot));
            builder.AppendLine("vertexFrameStatus=" +
                subject.VertexFrameStatus);
            builder.AppendLine("underResolvedInternalAdjacencies=" +
                subject.UnderResolvedInternalAdjacencies);
            builder.AppendLine("internalRenderNormalFragmentation=" +
                subject.InternalRenderNormalFragmentation);
            builder.AppendLine("internalMaskEdgeJumps=" +
                subject.InternalMaskEdgeJumps);
            builder.AppendLine("valueGradientDiscontinuityBevels=" +
                subject.ValueGradientDiscontinuityBevels);
            builder.AppendLine("structuralGradientDiscontinuityBevels=" +
                subject.StructuralGradientDiscontinuityBevels);
            builder.AppendLine("alternativeTriangulationAuditedBevels=" +
                subject.AlternativeTriangulationAuditedBevels);
            builder.AppendLine("alternativeTriangulationImprovementBevels=" +
                subject.AlternativeTriangulationImprovementBevels);
            builder.AppendLine("geometryStatus=" + subject.GeometryStatus);
            if (subject.AcceptedBuild != null)
            {
                builder.AppendLine("preMaskImmutableFingerprint=" +
                    subject.AcceptedBuild.PreMaskImmutableFingerprint.ToString("X16"));
                builder.AppendLine("postMaskImmutableFingerprint=" +
                    subject.AcceptedBuild.PostMaskImmutableFingerprint.ToString("X16"));
                builder.AppendLine("preMaskValueFingerprint=" +
                    subject.AcceptedBuild.PreMaskValueFingerprint.ToString("X16"));
                builder.AppendLine("postMaskValueFingerprint=" +
                    subject.AcceptedBuild.PostMaskValueFingerprint.ToString("X16"));
                builder.AppendLine("sourceFaceMaskChanges=" +
                    subject.AcceptedBuild.SourceFaceMaskChangeCount);
            }
            foreach (string failure in subject.CaptureFailures)
                builder.AppendLine("captureFailure=" + failure);
            builder.AppendLine();
            builder.AppendLine("[" + subject.Role + " worst logical bevels]");
            foreach (BevelResult result in
                subject.Results
                    .OrderByDescending(x => x.Severity)
                    .ThenBy(x => x.EdgeId)
                    .Take(16))
            {
                builder.AppendLine(
                    "logicalBevel=" + result.EdgeId +
                    ",triangles=" + result.TriangleCount +
                    ",structurallyInvalid=" + result.StructurallyInvalidTriangles +
                    ",underResolved=" + result.NumericallyUnderResolvedTriangles +
                    ",extremeSliver=" + result.ExtremeSliverTriangles +
                    ",maxAspect=" + F((float)result.MaximumAspectRatio) +
                    ",minAngle=" + F((float)result.MinimumAngleDegrees) +
                    ",maxGeomJump=" + F(result.MaximumConditionedGeometricNormalJump) +
                    ",underResolvedAdjacencies=" + result.UnderResolvedInternalAdjacencies +
                    ",renderNormalJumps=" + result.InternalRenderNormalJumpCount +
                    ",maskEdgeJumps=" + result.InternalMaskJumpCount +
                    ",valueGradientJumps=" + result.ValueGradientJumpCount +
                    ",structuralGradientJumps=" + result.StructuralGradientJumpCount +
                    ",alternative=" + result.Alternative.Kind +
                    ",alternativeImproves=" + (result.Alternative.Improves ? 1 : 0) +
                    ",alternativeCandidates=" + result.Alternative.CandidatesAudited +
                    ",alternativeRejected=" + result.Alternative.CandidatesRejected +
                    ",alternativeMaxAspect=" + F((float)result.Alternative.MaximumAspectRatio) +
                    ",alternativeMinAngle=" + F((float)result.Alternative.MinimumAngleDegrees));
                foreach (string evidence in result.Evidence.Take(12))
                    builder.AppendLine("evidence=" + evidence);
            }
            builder.AppendLine();
        }

        private sealed class Job
        {
            internal readonly SubjectData Suspect;
            internal readonly SubjectData Reference;
            internal Stage Stage = Stage.CaptureSuspect;
            internal bool CancelRequested;
            internal int CompletedUnits;
            internal int TotalBevels;
            internal GeneratedMassSurfaceCausalityRenderAudit RenderAudit;
            private readonly Stopwatch stopwatch = Stopwatch.StartNew();

            internal Job(GeneratedMass suspect, GeneratedMass reference)
            {
                Suspect = new SubjectData("Suspect", suspect);
                Reference = reference == null
                    ? null
                    : new SubjectData("Reference", reference);
            }

            internal TimeSpan Elapsed => stopwatch.Elapsed;
            internal int EstimatedTotalUnits
            {
                get
                {
                    int captures = Reference == null ? 1 : 2;
                    int renderCases = RenderAudit == null
                        ? 192
                        : RenderAudit.TotalRenderPasses;
                    return captures + 2 + Mathf.Max(1, TotalBevels) + renderCases;
                }
            }
            internal float Progress01 => Mathf.Clamp01(
                CompletedUnits / (float)Mathf.Max(1, EstimatedTotalUnits));
            internal TimeSpan EstimatedRemaining
            {
                get
                {
                    if (CompletedUnits <= 0 || Progress01 <= 0f)
                        return TimeSpan.Zero;
                    double total = stopwatch.Elapsed.TotalSeconds / Progress01;
                    return TimeSpan.FromSeconds(
                        Math.Max(0.0, total - stopwatch.Elapsed.TotalSeconds));
                }
            }
            internal string ProgressText
            {
                get
                {
                    string detail = Stage switch
                    {
                        Stage.CaptureSuspect => "Capturing and freezing suspect production mesh",
                        Stage.CaptureReference => "Capturing and freezing reference production mesh",
                        Stage.BuildSubjects => "Building canonical final-triangle indices",
                        Stage.AnalyzeSubjects => "Analyzing logical bevel " +
                            (Suspect.NextBevel +
                             (Reference == null ? 0 : Reference.NextBevel) + 1) +
                            "/" + Mathf.Max(1, TotalBevels),
                        Stage.PrepareRenderAudit => "Preparing frozen-mesh material parity tournament",
                        Stage.RenderAudit => RenderAudit == null
                            ? "Preparing render tournament"
                            : RenderAudit.ProgressText,
                        _ => "Writing terminal causality report"
                    };
                    if (EstimatedRemaining > TimeSpan.Zero)
                    {
                        detail += " · ETA " +
                            Math.Ceiling(EstimatedRemaining.TotalSeconds) + " s";
                    }
                    return detail;
                }
            }

            internal bool FinalRendererRestoreAttempted;
            internal bool FinalRendererRestoreSucceeded;

            internal bool RendererMaterialStateRestored =>
                FinalRendererRestoreAttempted &&
                FinalRendererRestoreSucceeded;

            internal void RestoreInitialRendererStates()
            {
                FinalRendererRestoreAttempted = true;
                bool suspectRestored =
                    Suspect.RestoreInitialRendererState(afterCapture: false);
                bool referenceRestored =
                    Reference == null ||
                    Reference.RestoreInitialRendererState(afterCapture: false);
                FinalRendererRestoreSucceeded =
                    suspectRestored && referenceRestored;
            }

            internal void PrepareRenderAudit()
            {
                GeneratedMassSurfaceCausalityRenderAudit.Subject suspectSubject =
                    Suspect.CreateRenderSubject();
                GeneratedMassSurfaceCausalityRenderAudit.Subject referenceSubject =
                    Reference?.CreateRenderSubject();
                GeneratedMassSurfaceCausalityRenderAudit.BuildInternalEdges(
                    suspectSubject);
                if (referenceSubject != null)
                {
                    GeneratedMassSurfaceCausalityRenderAudit.BuildInternalEdges(
                        referenceSubject);
                }
                RenderAudit = new GeneratedMassSurfaceCausalityRenderAudit(
                    suspectSubject,
                    referenceSubject);
            }
        }

        private sealed class RendererMaterialStateSnapshot
        {
            private static readonly string[] GeneratedMassPropertyNames =
            {
                "_BaseColor",
                "_Color",
                "_MaskDebugMode",
                "_SurfaceContract",
                "_GeneratedMassFeatureAtlas0Enabled",
                "_GeneratedMassFeatureAtlas1Enabled",
                "_GeneratedMassFeatureAtlasQuality",
                "_GeneratedMassGeometryEdgeWearEnabled",
                "_GeneratedMassEdgeWearCoverage",
                "_GeneratedMassEdgeWearSoftness",
                "_GeneratedMassEdgeWearResponseStrength",
                "_GeneratedMassEdgeWearBrightnessLift",
                "_GeneratedMassEdgeWearTint",
                "_GeneratedMassEdgeWearTintStrength",
                "_GeneratedMassEdgeWearMacroVariation",
                "_GeneratedMassExposureResponse",
                "_GeneratedMassCreviceResponse",
                "_GeneratedMassBaseResponse",
                "_GeneratedMassDirtDepositResponse",
                "_GeneratedMassExposureTint",
                "_GeneratedMassExposureTintStrength",
                "_GeneratedMassCreviceTint",
                "_GeneratedMassCreviceTintStrength",
                "_GeneratedMassBaseTint",
                "_GeneratedMassBaseTintStrength",
                "_GeneratedMassDirtDepositTint",
                "_GeneratedMassDirtDepositTintStrength",
                "_GeneratedMassOverallRockTint",
                "_GeneratedMassOverallRockTintStrength",
                "_GeneratedMassLightingTintInfluence",
                "_GeneratedMassSurfaceNormalStrength",
                "_GeneratedMassSurfaceNormalScale"
            };

            internal readonly Renderer Renderer;
            internal readonly Material[] SharedMaterials;
            internal readonly MaterialPropertyBlock GlobalBlock;
            internal readonly bool GlobalBlockWasEmpty;
            internal readonly MaterialPropertyBlock[] IndexedBlocks;
            internal readonly bool[] IndexedBlocksWereEmpty;
            internal readonly ulong Fingerprint;

            internal Material PrimaryMaterial =>
                SharedMaterials != null && SharedMaterials.Length > 0
                    ? SharedMaterials[0]
                    : null;
            internal string PrimaryMaterialName =>
                PrimaryMaterial == null ? "<none>" : PrimaryMaterial.name;
            internal string PrimaryShaderName =>
                PrimaryMaterial == null || PrimaryMaterial.shader == null
                    ? "<none>"
                    : PrimaryMaterial.shader.name;

            private RendererMaterialStateSnapshot(
                Renderer renderer,
                Material[] sharedMaterials,
                MaterialPropertyBlock globalBlock,
                bool globalBlockWasEmpty,
                MaterialPropertyBlock[] indexedBlocks,
                bool[] indexedBlocksWereEmpty)
            {
                Renderer = renderer;
                SharedMaterials = sharedMaterials ?? Array.Empty<Material>();
                GlobalBlock = globalBlock;
                GlobalBlockWasEmpty = globalBlockWasEmpty;
                IndexedBlocks = indexedBlocks ?? Array.Empty<MaterialPropertyBlock>();
                IndexedBlocksWereEmpty =
                    indexedBlocksWereEmpty ?? Array.Empty<bool>();
                Fingerprint = CalculateCurrentFingerprint();
            }

            internal static RendererMaterialStateSnapshot Capture(
                Renderer renderer)
            {
                if (renderer == null)
                {
                    return null;
                }

                Material[] materials = renderer.sharedMaterials == null
                    ? Array.Empty<Material>()
                    : (Material[])renderer.sharedMaterials.Clone();
                MaterialPropertyBlock global = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(global);
                int count = materials.Length;
                MaterialPropertyBlock[] indexed =
                    new MaterialPropertyBlock[count];
                bool[] indexedEmpty = new bool[count];
                for (int index = 0; index < count; index++)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    renderer.GetPropertyBlock(block, index);
                    indexed[index] = block;
                    indexedEmpty[index] = block.isEmpty;
                }

                return new RendererMaterialStateSnapshot(
                    renderer,
                    materials,
                    global,
                    global.isEmpty,
                    indexed,
                    indexedEmpty);
            }

            internal bool RestoreAndVerify()
            {
                if (Renderer == null)
                {
                    return false;
                }

                int currentSlots = Renderer.sharedMaterials == null
                    ? 0
                    : Renderer.sharedMaterials.Length;
                Renderer.SetPropertyBlock(null);
                for (int index = 0; index < currentSlots; index++)
                {
                    Renderer.SetPropertyBlock(null, index);
                }

                Renderer.sharedMaterials =
                    (Material[])SharedMaterials.Clone();
                Renderer.SetPropertyBlock(
                    GlobalBlockWasEmpty ? null : GlobalBlock);
                for (int index = 0; index < IndexedBlocks.Length; index++)
                {
                    Renderer.SetPropertyBlock(
                        IndexedBlocksWereEmpty[index]
                            ? null
                            : IndexedBlocks[index],
                        index);
                }

                return MatchesCurrent();
            }

            internal bool MatchesCurrent()
            {
                if (Renderer == null)
                {
                    return false;
                }

                Material[] current = Renderer.sharedMaterials ??
                    Array.Empty<Material>();
                if (current.Length != SharedMaterials.Length)
                {
                    return false;
                }
                for (int index = 0; index < current.Length; index++)
                {
                    if (current[index] != SharedMaterials[index])
                    {
                        return false;
                    }
                }

                return CalculateCurrentFingerprint() == Fingerprint;
            }

            private ulong CalculateCurrentFingerprint()
            {
                if (Renderer == null)
                {
                    return 0UL;
                }

                Material[] materials = Renderer.sharedMaterials ??
                    Array.Empty<Material>();
                MaterialPropertyBlock global = new MaterialPropertyBlock();
                Renderer.GetPropertyBlock(global);
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, materials.Length);
                for (int index = 0; index < materials.Length; index++)
                {
                    Material material = materials[index];
                    Hash(ref hash, material == null ? 0 : material.GetEntityId().GetHashCode());
                }
                HashBlock(ref hash, global, materials);
                for (int index = 0; index < materials.Length; index++)
                {
                    MaterialPropertyBlock block = new MaterialPropertyBlock();
                    Renderer.GetPropertyBlock(block, index);
                    HashBlock(ref hash, block, materials);
                }
                return hash;
            }

            private static void HashBlock(
                ref ulong hash,
                MaterialPropertyBlock block,
                Material[] materials)
            {
                Hash(ref hash, block == null || block.isEmpty ? 0 : 1);
                SortedSet<string> names = new SortedSet<string>(
                    GeneratedMassPropertyNames,
                    StringComparer.Ordinal);
                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        Shader shader = material == null
                            ? null
                            : material.shader;
                        if (shader == null)
                        {
                            continue;
                        }
                        int propertyCount = shader.GetPropertyCount();
                        for (int index = 0; index < propertyCount; index++)
                        {
                            names.Add(shader.GetPropertyName(index));
                        }
                    }
                }

                foreach (string name in names)
                {
                    int id = Shader.PropertyToID(name);
                    Hash(ref hash, id);
                    ShaderPropertyType type = ResolvePropertyType(
                        materials,
                        name);
                    switch (type)
                    {
                        case ShaderPropertyType.Texture:
                            Texture texture = block?.GetTexture(id);
                            Hash(
                                ref hash,
                                texture == null ? 0 : texture.GetEntityId().GetHashCode());
                            break;
                        case ShaderPropertyType.Color:
                        case ShaderPropertyType.Vector:
                            Vector4 vector = block == null
                                ? Vector4.zero
                                : block.GetVector(id);
                            Hash(ref hash, vector.x);
                            Hash(ref hash, vector.y);
                            Hash(ref hash, vector.z);
                            Hash(ref hash, vector.w);
                            break;
                        case ShaderPropertyType.Int:
                            Hash(
                                ref hash,
                                block == null ? 0 : block.GetInt(id));
                            break;
                        default:
                            Hash(
                                ref hash,
                                block == null ? 0f : block.GetFloat(id));
                            break;
                    }
                }
            }

            private static ShaderPropertyType ResolvePropertyType(
                Material[] materials,
                string name)
            {
                if (materials != null)
                {
                    foreach (Material material in materials)
                    {
                        Shader shader = material == null
                            ? null
                            : material.shader;
                        if (shader == null)
                        {
                            continue;
                        }
                        int count = shader.GetPropertyCount();
                        for (int index = 0; index < count; index++)
                        {
                            if (string.Equals(
                                shader.GetPropertyName(index),
                                name,
                                StringComparison.Ordinal))
                            {
                                return shader.GetPropertyType(index);
                            }
                        }
                    }
                }
                return ShaderPropertyType.Float;
            }

            private static void Hash(ref ulong hash, int value)
            {
                unchecked
                {
                    hash ^= (uint)value;
                    hash *= 1099511628211UL;
                }
            }

            private static void Hash(ref ulong hash, float value)
            {
                Hash(ref hash, BitConverter.SingleToInt32Bits(value));
            }
        }

        private sealed class SubjectData
        {
            internal readonly string Role;
            internal readonly GeneratedMass Target;
            internal MassGenerator.BevelShadingDiagnosticSnapshot Snapshot;
            internal MassGenerator.BevelShadingDiagnosticBuildRecord AcceptedBuild;
            internal Mesh Mesh;
            internal Vector3[] Vertices = Array.Empty<Vector3>();
            internal Vector3[] Normals = Array.Empty<Vector3>();
            internal Vector4[] Tangents = Array.Empty<Vector4>();
            internal Vector2[] UV0 = Array.Empty<Vector2>();
            internal Color[] Colors = Array.Empty<Color>();
            internal Vector4[] UV2 = Array.Empty<Vector4>();
            internal Vector4[] Structural = Array.Empty<Vector4>();
            internal int[] Indices = Array.Empty<int>();
            internal readonly Dictionary<int, List<MassGenerator.FinalTriangleRecord>> Bevels = new();
            internal readonly List<string> CaptureFailures = new();
            internal readonly List<BevelResult> Results = new();
            internal bool CaptureContractValid;
            internal int UnmappedBevelTriangles;
            internal int NextBevel;
            internal string MaterialName = "<none>";
            internal string ShaderName = "<none>";
            internal Material Material;
            internal RendererMaterialStateSnapshot InitialRendererState;
            internal bool MaterialMutationObservedDuringCapture;
            internal bool MaterialStateRestoredAfterCapture;
            internal bool MaterialStateRestoredAtFinalize;
            internal string ObservedMaterialAfterRegeneration = "<not-captured>";
            internal string ObservedShaderAfterRegeneration = "<not-captured>";
            internal string MaterialRestoreError = string.Empty;
            internal int ExactDegenerateTriangles;
            internal int DuplicateIndexTriangles;
            internal int CoincidentPositionTriangles;
            internal int NonFiniteTriangles;
            internal int IndexOutOfRangeTriangles;
            internal int WindingInvalidTriangles;
            internal int NumericallyUnderResolvedTriangles;
            internal int ExtremeSliverTriangles;
            internal int ConditionedTriangles;
            internal int CaptureToFinalMeshMismatches;
            internal int AcceptedMeshDataUploadMismatches;
            internal int CanonicalAccountingMismatch;
            internal int UnderResolvedInternalAdjacencies;
            internal int InternalRenderNormalFragmentation;
            internal int InternalMaskEdgeJumps;
            internal int ValueGradientDiscontinuityBevels;
            internal int StructuralGradientDiscontinuityBevels;
            internal int AlternativeTriangulationAuditedBevels;
            internal int AlternativeTriangulationImprovementBevels;
            internal double MaximumAspectRatio;
            internal double MinimumAngleDegrees = double.PositiveInfinity;
            internal double MinimumNormalizedDoubleArea = double.PositiveInfinity;
            internal float MaximumConditionedGeometricNormalJump;
            internal int InvalidNormalVertices;
            internal int InvalidTangentVertices;
            internal int InvalidTangentHandednessVertices;
            internal float MaximumTangentNormalAbsDot;

            internal SubjectData(string role, GeneratedMass target)
            {
                Role = role;
                Target = target;
                EnsureInitialRendererState();
            }

            internal int StructurallyInvalidTriangles =>
                ExactDegenerateTriangles +
                DuplicateIndexTriangles +
                CoincidentPositionTriangles +
                NonFiniteTriangles +
                IndexOutOfRangeTriangles +
                WindingInvalidTriangles;

            internal string GeometryStatus
            {
                get
                {
                    if (StructurallyInvalidTriangles > 0)
                        return "GEOMETRY_STRUCTURALLY_INVALID";
                    if (NumericallyUnderResolvedTriangles > 0)
                        return "GEOMETRY_NUMERICALLY_UNDER_RESOLVED";
                    if (ExtremeSliverTriangles > 0)
                        return "GEOMETRY_VALID_BUT_POORLY_CONDITIONED";
                    return "GEOMETRY_VALID";
                }
            }

            internal string VertexFrameStatus =>
                InvalidNormalVertices > 0 ||
                InvalidTangentVertices > 0 ||
                InvalidTangentHandednessVertices > 0 ||
                MaximumTangentNormalAbsDot > 0.01f
                    ? "VERTEX_FRAME_INVALID"
                    : "VERTEX_FRAME_VALID";

            internal void LoadFinalMesh()
            {
                if (Mesh == null) return;
                Vertices = Mesh.vertices;
                Normals = Mesh.normals;
                Tangents = Mesh.tangents;
                UV0 = Mesh.uv;
                Colors = Mesh.colors;
                Indices = Mesh.triangles;
                UV2 = ReadUv(Mesh, 2, Vertices.Length);
                Structural = ReadUv(Mesh, 4, Vertices.Length);
            }

            internal void EnsureInitialRendererState()
            {
                if (InitialRendererState != null || Target == null)
                {
                    return;
                }

                Renderer renderer =
                    Target.GeometryMeshFilter == null
                        ? null
                        : Target.GeometryMeshFilter.GetComponent<Renderer>();
                InitialRendererState =
                    RendererMaterialStateSnapshot.Capture(renderer);
            }

            internal void RecordRendererStateAfterRegeneration()
            {
                EnsureInitialRendererState();
                Renderer renderer = InitialRendererState?.Renderer;
                Material observed = renderer == null
                    ? null
                    : renderer.sharedMaterial;
                ObservedMaterialAfterRegeneration =
                    observed == null ? "<none>" : observed.name;
                ObservedShaderAfterRegeneration =
                    observed == null || observed.shader == null
                        ? "<none>"
                        : observed.shader.name;
                MaterialMutationObservedDuringCapture =
                    InitialRendererState != null &&
                    !InitialRendererState.MatchesCurrent();
            }

            internal bool RestoreInitialRendererState(bool afterCapture)
            {
                EnsureInitialRendererState();
                bool restored = false;
                try
                {
                    restored =
                        InitialRendererState != null &&
                        InitialRendererState.RestoreAndVerify();
                    MaterialRestoreError = restored
                        ? string.Empty
                        : "snapshot unavailable or verification mismatch";
                }
                catch (Exception exception)
                {
                    MaterialRestoreError =
                        exception.GetType().Name + ":" + exception.Message;
                    restored = false;
                }
                if (afterCapture)
                {
                    MaterialStateRestoredAfterCapture = restored;
                }
                else
                {
                    MaterialStateRestoredAtFinalize = restored;
                }
                return restored;
            }

            internal void BuildMaterialEvidence()
            {
                EnsureInitialRendererState();
                Material = InitialRendererState?.PrimaryMaterial;
                MaterialName = Material == null ? "<none>" : Material.name;
                ShaderName = Material == null || Material.shader == null
                    ? "<none>"
                    : Material.shader.name;
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
                if (acceptedCount != 1)
                    CaptureFailures.Add(
                        "accepted build count=" + acceptedCount + ",expected=1");
                if (AcceptedBuild == null)
                {
                    CaptureFailures.Add("accepted build is missing");
                    return;
                }
                if (!AcceptedBuild.Completed || !AcceptedBuild.Succeeded)
                    CaptureFailures.Add("accepted build did not complete successfully");
                if (AcceptedBuild.FinalTriangles.Count != Indices.Length / 3)
                    CaptureFailures.Add(
                        "accepted triangle count=" +
                        AcceptedBuild.FinalTriangles.Count +
                        ",uploaded=" + Indices.Length / 3);
                if (AcceptedBuild.LogicalBevels.Count == 0)
                    CaptureFailures.Add("accepted build captured zero logical bevels");

                AuditAcceptedMeshDataUpload();
                AnalyzeVertexFrames();

                foreach (MassGenerator.FinalTriangleRecord triangle in
                    AcceptedBuild.FinalTriangles)
                {
                    AnalyzeCanonicalTriangle(triangle);
                    if (!triangle.IsOrdinaryBevel) continue;
                    if (triangle.LogicalBevelId < 0 ||
                        !AcceptedBuild.LogicalBevels.ContainsKey(
                            triangle.LogicalBevelId))
                    {
                        UnmappedBevelTriangles++;
                        continue;
                    }
                    if (!Bevels.TryGetValue(
                        triangle.LogicalBevelId,
                        out List<MassGenerator.FinalTriangleRecord> list))
                    {
                        list = new List<MassGenerator.FinalTriangleRecord>();
                        Bevels.Add(triangle.LogicalBevelId, list);
                    }
                    list.Add(triangle);
                }
                if (CaptureToFinalMeshMismatches > 0)
                {
                    CaptureFailures.Add(
                        "capture-to-upload mismatches=" +
                        CaptureToFinalMeshMismatches);
                }
                if (CanonicalAccountingMismatch > 0)
                {
                    CaptureFailures.Add(
                        "canonical accounting mismatches=" +
                        CanonicalAccountingMismatch);
                }
                if (UnmappedBevelTriangles > 0)
                    CaptureFailures.Add(
                        "unmapped ordinary bevel triangles=" +
                        UnmappedBevelTriangles);
                if (Bevels.Count != AcceptedBuild.LogicalBevels.Count)
                    CaptureFailures.Add(
                        "mapped logical bevels=" + Bevels.Count +
                        ",captured=" + AcceptedBuild.LogicalBevels.Count);
                CaptureContractValid =
                    CaptureFailures.Count == 0 &&
                    Bevels.Count > 0;
            }

            private void AuditAcceptedMeshDataUpload()
            {
                MeshData meshData = AcceptedBuild?.MeshData;
                if (meshData == null)
                {
                    AcceptedMeshDataUploadMismatches++;
                    return;
                }
                if (meshData.Vertices.Count != Vertices.Length ||
                    meshData.Triangles.Count != Indices.Length)
                {
                    AcceptedMeshDataUploadMismatches++;
                    return;
                }
                for (int index = 0; index < Indices.Length; index++)
                {
                    if (meshData.Triangles[index] != Indices[index])
                        AcceptedMeshDataUploadMismatches++;
                }
                for (int index = 0; index < Vertices.Length; index++)
                {
                    if (!SamePosition(meshData.Vertices[index], Vertices[index]))
                        AcceptedMeshDataUploadMismatches++;
                    if (meshData.Normals.Count == Vertices.Length &&
                        Normals.Length == Vertices.Length &&
                        Angle(meshData.Normals[index], Normals[index]) > 0.02f)
                    {
                        AcceptedMeshDataUploadMismatches++;
                    }
                    if (meshData.Colors.Count == Vertices.Length &&
                        Colors.Length == Vertices.Length &&
                        !SameColor(meshData.Colors[index], Colors[index]))
                    {
                        AcceptedMeshDataUploadMismatches++;
                    }
                    if (meshData.UV0.Count == Vertices.Length &&
                        UV0.Length == Vertices.Length &&
                        (meshData.UV0[index] - UV0[index]).sqrMagnitude > 1e-12f)
                    {
                        AcceptedMeshDataUploadMismatches++;
                    }
                    if (meshData.UV2.Count == Vertices.Length &&
                        UV2.Length == Vertices.Length &&
                        !SameVector4(meshData.UV2[index], UV2[index]))
                    {
                        AcceptedMeshDataUploadMismatches++;
                    }
                    if (meshData.SurfaceFeatures.Count == Vertices.Length &&
                        Structural.Length == Vertices.Length &&
                        !SameVector4(
                            meshData.SurfaceFeatures[index],
                            Structural[index]))
                    {
                        AcceptedMeshDataUploadMismatches++;
                    }
                }
                if (AcceptedMeshDataUploadMismatches > 0)
                {
                    CaptureFailures.Add(
                        "accepted MeshData/upload mismatches=" +
                        AcceptedMeshDataUploadMismatches);
                }
            }

            private void AnalyzeVertexFrames()
            {
                if (Normals.Length != Vertices.Length)
                {
                    InvalidNormalVertices = Vertices.Length;
                }
                if (Tangents.Length != Vertices.Length)
                {
                    InvalidTangentVertices = Vertices.Length;
                }
                int count = Vertices.Length;
                for (int index = 0; index < count; index++)
                {
                    Vector3 normal = index < Normals.Length
                        ? Normals[index]
                        : Vector3.zero;
                    Vector4 tangent4 = index < Tangents.Length
                        ? Tangents[index]
                        : Vector4.zero;
                    Vector3 tangent = new Vector3(
                        tangent4.x,
                        tangent4.y,
                        tangent4.z);
                    if (!IsFinite(normal) ||
                        normal.sqrMagnitude < 0.5f ||
                        normal.sqrMagnitude > 1.5f)
                    {
                        if (Normals.Length == Vertices.Length)
                            InvalidNormalVertices++;
                    }
                    if (!IsFinite(tangent) ||
                        tangent.sqrMagnitude < 0.5f ||
                        tangent.sqrMagnitude > 1.5f)
                    {
                        if (Tangents.Length == Vertices.Length)
                            InvalidTangentVertices++;
                    }
                    if (Tangents.Length == Vertices.Length &&
                        ((float.IsNaN(tangent4.w) ||
                          float.IsInfinity(tangent4.w)) ||
                         Mathf.Abs(Mathf.Abs(tangent4.w) - 1f) > 0.01f))
                    {
                        InvalidTangentHandednessVertices++;
                    }
                    if (normal != Vector3.zero && tangent != Vector3.zero)
                    {
                        MaximumTangentNormalAbsDot = Mathf.Max(
                            MaximumTangentNormalAbsDot,
                            Mathf.Abs(Vector3.Dot(
                                normal.normalized,
                                tangent.normalized)));
                    }
                }
            }

            private void AnalyzeCanonicalTriangle(
                MassGenerator.FinalTriangleRecord captured)
            {
                int offset = captured.TriangleIndex * 3;
                if (offset + 2 >= Indices.Length)
                {
                    CaptureToFinalMeshMismatches++;
                    return;
                }
                int ia = Indices[offset];
                int ib = Indices[offset + 1];
                int ic = Indices[offset + 2];
                MassGenerator.FinalTriangleQuality uploaded =
                    MassGenerator.EvaluateFinalTriangleQuality(
                        Vertices,
                        ia,
                        ib,
                        ic,
                        captured.AuthoredNormal);
                MassGenerator.FinalTriangleQuality capturedQuality =
                    MassGenerator.EvaluateFinalTriangleQuality(
                        captured.A,
                        captured.B,
                        captured.C,
                        captured.IndexA,
                        captured.IndexB,
                        captured.IndexC,
                        captured.AuthoredNormal);
                bool uploadedIndicesInRange =
                    uploaded.HasValidIndexRange;
                bool uploadedChannelsMatch = true;
                if (uploadedIndicesInRange)
                {
                    uploadedChannelsMatch =
                        SameVector4(ReadUploadedMask(ia), captured.MaskA) &&
                        SameVector4(ReadUploadedMask(ib), captured.MaskB) &&
                        SameVector4(ReadUploadedMask(ic), captured.MaskC) &&
                        SameVector4(ReadUploadedStructural(ia), captured.StructuralA) &&
                        SameVector4(ReadUploadedStructural(ib), captured.StructuralB) &&
                        SameVector4(ReadUploadedStructural(ic), captured.StructuralC);
                    if (Normals.Length == Vertices.Length)
                    {
                        Vector3 uploadedRenderNormal = N(
                            Normals[ia] + Normals[ib] + Normals[ic]);
                        uploadedChannelsMatch &=
                            Angle(uploadedRenderNormal, captured.RenderNormal) <= 0.02f;
                    }
                }
                if (ia != captured.IndexA ||
                    ib != captured.IndexB ||
                    ic != captured.IndexC ||
                    !uploadedIndicesInRange ||
                    (uploadedIndicesInRange &&
                     (!SamePosition(Vertices[ia], captured.A) ||
                      !SamePosition(Vertices[ib], captured.B) ||
                      !SamePosition(Vertices[ic], captured.C))) ||
                    !uploadedChannelsMatch ||
                    uploaded.PrimaryCondition != capturedQuality.PrimaryCondition ||
                    Math.Abs(uploaded.NormalizedDoubleArea -
                        capturedQuality.NormalizedDoubleArea) > 1e-7)
                {
                    CaptureToFinalMeshMismatches++;
                }
                if (captured.TriangleCondition != capturedQuality.PrimaryCondition)
                {
                    CanonicalAccountingMismatch++;
                }
                CountQuality(uploaded);
            }

            private void CountQuality(MassGenerator.FinalTriangleQuality quality)
            {
                switch (quality.PrimaryCondition)
                {
                    case MassGenerator.FinalTriangleCondition.NonFinite:
                        NonFiniteTriangles++;
                        break;
                    case MassGenerator.FinalTriangleCondition.IndexOutOfRange:
                        IndexOutOfRangeTriangles++;
                        break;
                    case MassGenerator.FinalTriangleCondition.DuplicateIndices:
                        DuplicateIndexTriangles++;
                        break;
                    case MassGenerator.FinalTriangleCondition.CoincidentPositions:
                        CoincidentPositionTriangles++;
                        break;
                    case MassGenerator.FinalTriangleCondition.ExactDegenerate:
                        ExactDegenerateTriangles++;
                        break;
                    case MassGenerator.FinalTriangleCondition.WindingInvalid:
                        WindingInvalidTriangles++;
                        break;
                }
                if (!quality.IsStructurallyInvalid)
                {
                    if (quality.IsNumericallyUnderResolved)
                        NumericallyUnderResolvedTriangles++;
                    if (quality.IsExtremeSliver)
                        ExtremeSliverTriangles++;
                    if (quality.IsConditionedForDifferentialAnalysis)
                        ConditionedTriangles++;
                }
                MaximumAspectRatio = Math.Max(
                    MaximumAspectRatio,
                    quality.AspectRatio);
                MinimumAngleDegrees = Math.Min(
                    MinimumAngleDegrees,
                    quality.MinimumAngleDegrees);
                MinimumNormalizedDoubleArea = Math.Min(
                    MinimumNormalizedDoubleArea,
                    quality.NormalizedDoubleArea);
            }

            private static bool SamePosition(Vector3 a, Vector3 b)
            {
                double dx = (double)a.x - b.x;
                double dy = (double)a.y - b.y;
                double dz = (double)a.z - b.z;
                double scale = Math.Max(
                    1.0,
                    Math.Max(
                        Math.Max(a.magnitude, b.magnitude),
                        Math.Max((double)Mathf.Abs(a.x), Mathf.Abs(b.x))));
                double tolerance = scale * 1e-7;
                return dx * dx + dy * dy + dz * dz <= tolerance * tolerance;
            }

            private Vector4 ReadUploadedMask(int index)
            {
                if (index < 0 ||
                    index >= Colors.Length ||
                    index >= UV2.Length)
                {
                    return Vector4.zero;
                }
                Color color = Colors[index];
                return new Vector4(
                    color.r,
                    color.g,
                    color.b,
                    UV2[index].y);
            }

            private Vector4 ReadUploadedStructural(int index)
            {
                return index >= 0 && index < Structural.Length
                    ? Structural[index]
                    : Vector4.zero;
            }

            private static bool SameVector4(Vector4 a, Vector4 b)
            {
                return
                    Mathf.Abs(a.x - b.x) <= ScalarEpsilon &&
                    Mathf.Abs(a.y - b.y) <= ScalarEpsilon &&
                    Mathf.Abs(a.z - b.z) <= ScalarEpsilon &&
                    Mathf.Abs(a.w - b.w) <= ScalarEpsilon;
            }

            private static bool SameColor(Color a, Color b)
            {
                return
                    Mathf.Abs(a.r - b.r) <= ScalarEpsilon &&
                    Mathf.Abs(a.g - b.g) <= ScalarEpsilon &&
                    Mathf.Abs(a.b - b.b) <= ScalarEpsilon &&
                    Mathf.Abs(a.a - b.a) <= ScalarEpsilon;
            }

            private static bool IsFinite(Vector3 value)
            {
                return
                    !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
                    !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
                    !float.IsNaN(value.z) && !float.IsInfinity(value.z);
            }

            internal void AnalyzeNextBevel()
            {
                KeyValuePair<int, List<MassGenerator.FinalTriangleRecord>> pair =
                    Bevels.OrderBy(x => x.Key).ElementAt(NextBevel++);
                AcceptedBuild.LogicalBevels.TryGetValue(
                    pair.Key,
                    out MassGenerator.LogicalBevelRecord logical);
                BevelResult result = AnalyzeBevel(
                    pair.Key,
                    logical,
                    pair.Value,
                    this);
                Results.Add(result);
                UnderResolvedInternalAdjacencies +=
                    result.UnderResolvedInternalAdjacencies;
                InternalRenderNormalFragmentation +=
                    result.InternalRenderNormalJumpCount;
                InternalMaskEdgeJumps += result.InternalMaskJumpCount;
                if (result.ValueGradientJumpCount > 0)
                    ValueGradientDiscontinuityBevels++;
                if (result.StructuralGradientJumpCount > 0)
                    StructuralGradientDiscontinuityBevels++;
                MaximumConditionedGeometricNormalJump = Mathf.Max(
                    MaximumConditionedGeometricNormalJump,
                    result.MaximumConditionedGeometricNormalJump);
                if (result.Alternative.Audited)
                    AlternativeTriangulationAuditedBevels++;
                if (result.Alternative.Improves)
                    AlternativeTriangulationImprovementBevels++;
            }

            internal GeneratedMassSurfaceCausalityRenderAudit.Subject
                CreateRenderSubject()
            {
                return new GeneratedMassSurfaceCausalityRenderAudit.Subject
                {
                    Role = Role,
                    Target = Target,
                    Mesh = Mesh,
                    Material = Material,
                    Build = AcceptedBuild
                };
            }
        }

        private static BevelResult AnalyzeBevel(
            int edgeId,
            MassGenerator.LogicalBevelRecord logical,
            List<MassGenerator.FinalTriangleRecord> triangles,
            SubjectData subject)
        {
            BevelResult result = new BevelResult
            {
                EdgeId = edgeId,
                TriangleCount = triangles.Count
            };
            Dictionary<int, MassGenerator.FinalTriangleQuality> qualityByTriangle =
                new Dictionary<int, MassGenerator.FinalTriangleQuality>();
            foreach (MassGenerator.FinalTriangleRecord triangle in triangles)
            {
                int offset = triangle.TriangleIndex * 3;
                if (offset + 2 >= subject.Indices.Length)
                {
                    result.StructurallyInvalidTriangles++;
                    continue;
                }
                int ia = subject.Indices[offset];
                int ib = subject.Indices[offset + 1];
                int ic = subject.Indices[offset + 2];
                MassGenerator.FinalTriangleQuality quality =
                    MassGenerator.EvaluateFinalTriangleQuality(
                        subject.Vertices,
                        ia,
                        ib,
                        ic,
                        triangle.AuthoredNormal);
                qualityByTriangle[triangle.TriangleIndex] = quality;
                if (quality.IsStructurallyInvalid)
                    result.StructurallyInvalidTriangles++;
                if (quality.IsNumericallyUnderResolved)
                    result.NumericallyUnderResolvedTriangles++;
                if (quality.IsExtremeSliver)
                    result.ExtremeSliverTriangles++;
                result.MaximumAspectRatio = Math.Max(
                    result.MaximumAspectRatio,
                    quality.AspectRatio);
                result.MinimumAngleDegrees = Math.Min(
                    result.MinimumAngleDegrees,
                    quality.MinimumAngleDegrees);
                result.Evidence.Add(
                    "triangle=" + triangle.TriangleIndex +
                    ",condition=" + quality.PrimaryCondition +
                    ",normalizedDoubleArea=" + D(quality.NormalizedDoubleArea) +
                    ",aspect=" + D(quality.AspectRatio) +
                    ",minimumAngle=" + D(quality.MinimumAngleDegrees) +
                    ",geometricNormal=" + V(quality.GeometricNormal));
            }

            AnalyzeInternalEdges(
                triangles,
                qualityByTriangle,
                result);
            result.Alternative = AuditAlternativeTriangulations(
                triangles,
                logical == null ? Vector3.zero :
                    N(logical.ParentNormalA + logical.ParentNormalB));
            result.Severity =
                result.StructurallyInvalidTriangles * 100000 +
                result.NumericallyUnderResolvedTriangles * 10000 +
                result.InternalRenderNormalJumpCount * 1000 +
                result.InternalMaskJumpCount * 500 +
                result.UnderResolvedInternalAdjacencies * 100 +
                result.ValueGradientJumpCount * 20 +
                result.ExtremeSliverTriangles * 10;
            return result;
        }

        private static void AnalyzeInternalEdges(
            List<MassGenerator.FinalTriangleRecord> triangles,
            Dictionary<int, MassGenerator.FinalTriangleQuality> qualityByTriangle,
            BevelResult result)
        {
            Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>> edges =
                new Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>>();
            foreach (MassGenerator.FinalTriangleRecord triangle in triangles)
            {
                AddEdge(edges, triangle, triangle.A, triangle.B);
                AddEdge(edges, triangle, triangle.B, triangle.C);
                AddEdge(edges, triangle, triangle.C, triangle.A);
            }
            foreach (KeyValuePair<EdgeKey, List<MassGenerator.FinalTriangleRecord>> pair
                in edges.Where(x => x.Value.Count == 2))
            {
                MassGenerator.FinalTriangleRecord a = pair.Value[0];
                MassGenerator.FinalTriangleRecord b = pair.Value[1];
                float renderJump = Angle(a.RenderNormal, b.RenderNormal);
                if (renderJump > 0.5f)
                    result.InternalRenderNormalJumpCount++;

                MassGenerator.FinalTriangleQuality qa =
                    qualityByTriangle[a.TriangleIndex];
                MassGenerator.FinalTriangleQuality qb =
                    qualityByTriangle[b.TriangleIndex];
                if (!qa.IsConditionedForDifferentialAnalysis ||
                    !qb.IsConditionedForDifferentialAnalysis)
                {
                    result.UnderResolvedInternalAdjacencies++;
                }
                else
                {
                    float geometricJump = Angle(
                        qa.GeometricNormal,
                        qb.GeometricNormal);
                    result.MaximumConditionedGeometricNormalJump = Mathf.Max(
                        result.MaximumConditionedGeometricNormalJump,
                        geometricJump);
                }

                Vector4 maskA = SharedEdgeMask(a, pair.Key);
                Vector4 maskB = SharedEdgeMask(b, pair.Key);
                float exposureJump = Mathf.Abs(maskA.y - maskB.y);
                float creviceJump = Mathf.Abs(maskA.z - maskB.z);
                float dirtJump = Mathf.Abs(maskA.w - maskB.w);
                if (exposureJump > 0.00001f ||
                    creviceJump > 0.00001f ||
                    dirtJump > 0.00001f)
                {
                    result.InternalMaskJumpCount++;
                }

                if (!qa.IsConditionedForDifferentialAnalysis ||
                    !qb.IsConditionedForDifferentialAnalysis)
                {
                    continue;
                }
                float valueGradient = 0f;
                for (int channel = 0; channel < 4; channel++)
                {
                    valueGradient = Mathf.Max(
                        valueGradient,
                        GradientJump(a, b, channel, false, pair.Key));
                }
                float structuralGradient = 0f;
                for (int channel = 0; channel < 4; channel++)
                {
                    structuralGradient = Mathf.Max(
                        structuralGradient,
                        GradientJump(a, b, channel, true, pair.Key));
                }
                if (valueGradient > 0.35f)
                    result.ValueGradientJumpCount++;
                if (structuralGradient > 0.35f)
                    result.StructuralGradientJumpCount++;
                result.Evidence.Add(
                    "internalEdge=" + pair.Key +
                    ",renderJump=" + F(renderJump) +
                    ",valueGradientJump=" + F(valueGradient) +
                    ",structuralGradientJump=" + F(structuralGradient) +
                    ",conditionA=" + qa.PrimaryCondition +
                    ",conditionB=" + qb.PrimaryCondition);
            }
        }

        private static AlternativeTriangulationResult
            AuditAlternativeTriangulations(
                List<MassGenerator.FinalTriangleRecord> triangles,
                Vector3 expectedNormal)
        {
            AlternativeTriangulationResult result =
                new AlternativeTriangulationResult();
            List<Vector3> boundary;
            if (!TryBuildBoundaryCycle(triangles, out boundary) ||
                boundary.Count < 3)
            {
                result.Kind = "boundary-unavailable";
                return result;
            }
            Vector3 resolvedExpectedNormal = expectedNormal == Vector3.zero
                ? CalculatePolygonNormal(boundary)
                : N(expectedNormal);
            Vector3 boundaryNormal = CalculatePolygonNormal(boundary);
            if (resolvedExpectedNormal != Vector3.zero &&
                boundaryNormal != Vector3.zero &&
                Vector3.Dot(boundaryNormal, resolvedExpectedNormal) < 0f)
            {
                boundary.Reverse();
                boundaryNormal = -boundaryNormal;
            }
            if (resolvedExpectedNormal == Vector3.zero)
                resolvedExpectedNormal = boundaryNormal;
            result.Audited = true;
            TriangulationScore current = ScoreTriangles(
                triangles.Select(x => (x.A, x.B, x.C)),
                resolvedExpectedNormal);
            result.CurrentMaximumAspectRatio = current.MaximumAspectRatio;
            result.CurrentMinimumAngleDegrees = current.MinimumAngleDegrees;

            List<TriangulationCandidate> candidates = new();
            for (int anchor = 0; anchor < boundary.Count; anchor++)
            {
                List<(Vector3, Vector3, Vector3)> fan = BuildFan(
                    boundary,
                    anchor);
                result.CandidatesAudited++;
                if (fan.Count == boundary.Count - 2 &&
                    ValidateTriangulationCoverage(
                        boundary,
                        fan,
                        resolvedExpectedNormal))
                {
                    candidates.Add(new TriangulationCandidate(
                        "fan-" + anchor,
                        ScoreTriangles(fan, resolvedExpectedNormal)));
                }
                else
                {
                    result.CandidatesRejected++;
                }
            }
            List<(Vector3, Vector3, Vector3)> ear;
            result.CandidatesAudited++;
            if (TryEarClip(boundary, resolvedExpectedNormal, out ear))
            {
                if (ValidateTriangulationCoverage(
                    boundary,
                    ear,
                    resolvedExpectedNormal))
                {
                    candidates.Add(new TriangulationCandidate(
                        "ear-clipping",
                        ScoreTriangles(ear, resolvedExpectedNormal)));
                }
                else
                {
                    result.CandidatesRejected++;
                }
            }
            else
            {
                result.CandidatesRejected++;
            }
            TriangulationCandidate best = candidates
                .Where(x => x.Score.StructurallyInvalid == 0)
                .OrderBy(x => x.Score.NumericallyUnderResolved)
                .ThenBy(x => x.Score.ExtremeSlivers)
                .ThenBy(x => x.Score.MaximumAspectRatio)
                .ThenByDescending(x => x.Score.MinimumAngleDegrees)
                .ThenBy(x => x.Kind, StringComparer.Ordinal)
                .FirstOrDefault();
            if (best == null)
            {
                result.Kind = "no-safe-audit-candidate";
                return result;
            }
            result.Kind = best.Kind;
            result.MaximumAspectRatio = best.Score.MaximumAspectRatio;
            result.MinimumAngleDegrees = best.Score.MinimumAngleDegrees;
            result.StructurallyInvalid = best.Score.StructurallyInvalid;
            result.NumericallyUnderResolved =
                best.Score.NumericallyUnderResolved;
            result.ExtremeSlivers = best.Score.ExtremeSlivers;
            result.Improves =
                best.Score.StructurallyInvalid < current.StructurallyInvalid ||
                (best.Score.StructurallyInvalid == current.StructurallyInvalid &&
                 best.Score.NumericallyUnderResolved <
                    current.NumericallyUnderResolved) ||
                (best.Score.StructurallyInvalid == current.StructurallyInvalid &&
                 best.Score.NumericallyUnderResolved ==
                    current.NumericallyUnderResolved &&
                 best.Score.MaximumAspectRatio <
                    current.MaximumAspectRatio * 0.90);
            return result;
        }

        private static bool ValidateTriangulationCoverage(
            List<Vector3> boundary,
            List<(Vector3 A, Vector3 B, Vector3 C)> triangles,
            Vector3 expectedNormal)
        {
            if (boundary == null ||
                boundary.Count < 3 ||
                triangles == null ||
                triangles.Count != boundary.Count - 2)
            {
                return false;
            }

            Vector3 polygonNormal = expectedNormal == Vector3.zero
                ? CalculatePolygonNormal(boundary)
                : N(expectedNormal);
            if (polygonNormal == Vector3.zero)
                polygonNormal = CalculatePolygonNormal(boundary);
            if (polygonNormal == Vector3.zero) return false;

            int axis = DominantAxis(polygonNormal);
            List<Vector2> polygon = boundary
                .Select(point => Project2D(point, axis))
                .ToList();
            double polygonSignedDoubleArea = SignedDoubleArea(polygon);
            if (Math.Abs(polygonSignedDoubleArea) <= 1e-12) return false;
            double orientation = Math.Sign(polygonSignedDoubleArea);
            double accumulatedSignedDoubleArea = 0.0;

            HashSet<QuantizedPoint> boundaryPoints = new HashSet<QuantizedPoint>(
                boundary.Select(point => new QuantizedPoint(point)));
            Dictionary<EdgeKey, int> edgeUse = new Dictionary<EdgeKey, int>();
            List<(Vector2 A, Vector2 B, EdgeKey Key)> internalSegments = new();

            foreach ((Vector3 A, Vector3 B, Vector3 C) triangle in triangles)
            {
                QuantizedPoint qa = new QuantizedPoint(triangle.A);
                QuantizedPoint qb = new QuantizedPoint(triangle.B);
                QuantizedPoint qc = new QuantizedPoint(triangle.C);
                if (!boundaryPoints.Contains(qa) ||
                    !boundaryPoints.Contains(qb) ||
                    !boundaryPoints.Contains(qc) ||
                    qa.Equals(qb) || qb.Equals(qc) || qc.Equals(qa))
                {
                    return false;
                }

                Vector2 a = Project2D(triangle.A, axis);
                Vector2 b = Project2D(triangle.B, axis);
                Vector2 c = Project2D(triangle.C, axis);
                double signedDoubleArea = Cross2DDouble(b - a, c - a);
                if (Math.Abs(signedDoubleArea) <= 1e-12 ||
                    Math.Sign(signedDoubleArea) != orientation)
                {
                    return false;
                }
                Vector2 centroid = (a + b + c) / 3f;
                if (!PointInPolygonInclusive(centroid, polygon)) return false;
                accumulatedSignedDoubleArea += signedDoubleArea;

                AccumulateCandidateEdge(
                    edgeUse,
                    internalSegments,
                    triangle.A,
                    triangle.B,
                    a,
                    b,
                    boundary);
                AccumulateCandidateEdge(
                    edgeUse,
                    internalSegments,
                    triangle.B,
                    triangle.C,
                    b,
                    c,
                    boundary);
                AccumulateCandidateEdge(
                    edgeUse,
                    internalSegments,
                    triangle.C,
                    triangle.A,
                    c,
                    a,
                    boundary);
            }

            double areaTolerance = Math.Max(
                1e-10,
                Math.Abs(polygonSignedDoubleArea) * 1e-5);
            if (Math.Abs(
                accumulatedSignedDoubleArea - polygonSignedDoubleArea) >
                areaTolerance)
            {
                return false;
            }

            HashSet<EdgeKey> boundaryEdges = new HashSet<EdgeKey>();
            for (int i = 0; i < boundary.Count; i++)
            {
                boundaryEdges.Add(new EdgeKey(
                    boundary[i],
                    boundary[(i + 1) % boundary.Count]));
            }
            foreach (KeyValuePair<EdgeKey, int> pair in edgeUse)
            {
                int expectedUse = boundaryEdges.Contains(pair.Key) ? 1 : 2;
                if (pair.Value != expectedUse) return false;
            }
            if (boundaryEdges.Any(edge =>
                !edgeUse.TryGetValue(edge, out int count) || count != 1))
            {
                return false;
            }

            for (int i = 0; i < internalSegments.Count; i++)
            {
                for (int j = i + 1; j < internalSegments.Count; j++)
                {
                    var first = internalSegments[i];
                    var second = internalSegments[j];
                    if (first.Key.Equals(second.Key) ||
                        first.Key.Contains(second.Key.A) ||
                        first.Key.Contains(second.Key.B))
                    {
                        continue;
                    }
                    if (SegmentsProperlyIntersect(
                        first.A,
                        first.B,
                        second.A,
                        second.B))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static void AccumulateCandidateEdge(
            Dictionary<EdgeKey, int> edgeUse,
            List<(Vector2 A, Vector2 B, EdgeKey Key)> internalSegments,
            Vector3 a3,
            Vector3 b3,
            Vector2 a2,
            Vector2 b2,
            List<Vector3> boundary)
        {
            EdgeKey key = new EdgeKey(a3, b3);
            edgeUse.TryGetValue(key, out int count);
            edgeUse[key] = count + 1;
            if (!IsBoundaryEdge(key, boundary))
                internalSegments.Add((a2, b2, key));
        }

        private static bool IsBoundaryEdge(
            EdgeKey edge,
            List<Vector3> boundary)
        {
            for (int i = 0; i < boundary.Count; i++)
            {
                if (edge.Equals(new EdgeKey(
                    boundary[i],
                    boundary[(i + 1) % boundary.Count])))
                {
                    return true;
                }
            }
            return false;
        }

        private static double SignedDoubleArea(List<Vector2> polygon)
        {
            double area = 0.0;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[(i + 1) % polygon.Count];
                area += (double)a.x * b.y - (double)b.x * a.y;
            }
            return area;
        }

        private static double Cross2DDouble(Vector2 a, Vector2 b)
        {
            return (double)a.x * b.y - (double)a.y * b.x;
        }

        private static bool PointInPolygonInclusive(
            Vector2 point,
            List<Vector2> polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Count - 1;
                 i < polygon.Count;
                 j = i++)
            {
                Vector2 a = polygon[j];
                Vector2 b = polygon[i];
                if (Math.Abs(Cross2DDouble(b - a, point - a)) <= 1e-7 &&
                    point.x >= Math.Min(a.x, b.x) - 1e-6 &&
                    point.x <= Math.Max(a.x, b.x) + 1e-6 &&
                    point.y >= Math.Min(a.y, b.y) - 1e-6 &&
                    point.y <= Math.Max(a.y, b.y) + 1e-6)
                {
                    return true;
                }
                bool crosses =
                    ((a.y > point.y) != (b.y > point.y)) &&
                    point.x <
                    (b.x - a.x) * (point.y - a.y) /
                    (b.y - a.y) + a.x;
                if (crosses) inside = !inside;
            }
            return inside;
        }

        private static bool SegmentsProperlyIntersect(
            Vector2 a,
            Vector2 b,
            Vector2 c,
            Vector2 d)
        {
            double abC = Cross2DDouble(b - a, c - a);
            double abD = Cross2DDouble(b - a, d - a);
            double cdA = Cross2DDouble(d - c, a - c);
            double cdB = Cross2DDouble(d - c, b - c);
            const double epsilon = 1e-9;
            return
                ((abC > epsilon && abD < -epsilon) ||
                 (abC < -epsilon && abD > epsilon)) &&
                ((cdA > epsilon && cdB < -epsilon) ||
                 (cdA < -epsilon && cdB > epsilon));
        }

        private static TriangulationScore ScoreTriangles(
            IEnumerable<(Vector3 A, Vector3 B, Vector3 C)> triangles,
            Vector3 expectedNormal)
        {
            TriangulationScore score = new TriangulationScore
            {
                MinimumAngleDegrees = double.PositiveInfinity
            };
            foreach ((Vector3 A, Vector3 B, Vector3 C) triangle in triangles)
            {
                MassGenerator.FinalTriangleQuality quality =
                    MassGenerator.EvaluateFinalTriangleQuality(
                        triangle.A,
                        triangle.B,
                        triangle.C,
                        -1,
                        -1,
                        -1,
                        expectedNormal);
                if (quality.IsStructurallyInvalid) score.StructurallyInvalid++;
                if (quality.IsNumericallyUnderResolved)
                    score.NumericallyUnderResolved++;
                if (quality.IsExtremeSliver) score.ExtremeSlivers++;
                score.MaximumAspectRatio = Math.Max(
                    score.MaximumAspectRatio,
                    quality.AspectRatio);
                score.MinimumAngleDegrees = Math.Min(
                    score.MinimumAngleDegrees,
                    quality.MinimumAngleDegrees);
                score.TriangleCount++;
            }
            return score;
        }

        private static List<(Vector3, Vector3, Vector3)> BuildFan(
            List<Vector3> boundary,
            int anchor)
        {
            List<(Vector3, Vector3, Vector3)> triangles = new();
            for (int offset = 1; offset < boundary.Count - 1; offset++)
            {
                int b = (anchor + offset) % boundary.Count;
                int c = (anchor + offset + 1) % boundary.Count;
                triangles.Add((boundary[anchor], boundary[b], boundary[c]));
            }
            return triangles;
        }

        private static bool TryBuildBoundaryCycle(
            List<MassGenerator.FinalTriangleRecord> triangles,
            out List<Vector3> boundary)
        {
            Dictionary<EdgeKey, int> counts = new();
            Dictionary<QuantizedPoint, Vector3> positions = new();
            foreach (MassGenerator.FinalTriangleRecord triangle in triangles)
            {
                CountBoundaryEdge(counts, positions, triangle.A, triangle.B);
                CountBoundaryEdge(counts, positions, triangle.B, triangle.C);
                CountBoundaryEdge(counts, positions, triangle.C, triangle.A);
            }
            Dictionary<QuantizedPoint, List<QuantizedPoint>> adjacency = new();
            foreach (KeyValuePair<EdgeKey, int> pair in counts)
            {
                if (pair.Value != 1) continue;
                AddAdjacency(adjacency, pair.Key.A, pair.Key.B);
                AddAdjacency(adjacency, pair.Key.B, pair.Key.A);
            }
            if (adjacency.Count < 3 || adjacency.Any(x => x.Value.Count != 2))
            {
                boundary = null;
                return false;
            }
            QuantizedPoint start = adjacency.Keys.OrderBy(x => x).First();
            QuantizedPoint previous = default;
            bool hasPrevious = false;
            QuantizedPoint current = start;
            boundary = new List<Vector3>();
            for (int step = 0; step <= adjacency.Count; step++)
            {
                boundary.Add(positions[current]);
                List<QuantizedPoint> neighbours = adjacency[current]
                    .OrderBy(x => x)
                    .ToList();
                QuantizedPoint next = !hasPrevious || !neighbours[0].Equals(previous)
                    ? neighbours[0]
                    : neighbours[1];
                previous = current;
                hasPrevious = true;
                current = next;
                if (current.Equals(start)) break;
            }
            return boundary.Count == adjacency.Count && current.Equals(start);
        }

        private static bool TryEarClip(
            List<Vector3> boundary,
            Vector3 expectedNormal,
            out List<(Vector3, Vector3, Vector3)> triangles)
        {
            triangles = new List<(Vector3, Vector3, Vector3)>();
            if (boundary.Count < 3) return false;
            Vector3 normal = expectedNormal == Vector3.zero
                ? CalculatePolygonNormal(boundary)
                : expectedNormal;
            int axis = DominantAxis(normal);
            List<Vector2> projected = boundary.Select(x => Project2D(x, axis)).ToList();
            bool ccw = SignedArea(projected) > 0f;
            List<int> indices = Enumerable.Range(0, boundary.Count).ToList();
            int guard = boundary.Count * boundary.Count;
            while (indices.Count > 3 && guard-- > 0)
            {
                bool clipped = false;
                for (int local = 0; local < indices.Count; local++)
                {
                    int i0 = indices[(local - 1 + indices.Count) % indices.Count];
                    int i1 = indices[local];
                    int i2 = indices[(local + 1) % indices.Count];
                    float cross = Cross2D(
                        projected[i1] - projected[i0],
                        projected[i2] - projected[i1]);
                    if (ccw ? cross <= 0f : cross >= 0f) continue;
                    bool contains = false;
                    for (int test = 0; test < indices.Count; test++)
                    {
                        int point = indices[test];
                        if (point == i0 || point == i1 || point == i2) continue;
                        if (PointInTriangle(
                            projected[point],
                            projected[i0],
                            projected[i1],
                            projected[i2]))
                        {
                            contains = true;
                            break;
                        }
                    }
                    if (contains) continue;
                    triangles.Add((boundary[i0], boundary[i1], boundary[i2]));
                    indices.RemoveAt(local);
                    clipped = true;
                    break;
                }
                if (!clipped) return false;
            }
            if (indices.Count != 3) return false;
            triangles.Add((
                boundary[indices[0]],
                boundary[indices[1]],
                boundary[indices[2]]));
            return triangles.Count == boundary.Count - 2;
        }

        private static float GradientJump(
            MassGenerator.FinalTriangleRecord a,
            MassGenerator.FinalTriangleRecord b,
            int channel,
            bool structural,
            EdgeKey sharedEdge)
        {
            Vector3 gradientA = TriangleScalarGradient(a, channel, structural);
            Vector3 gradientB = TriangleScalarGradient(b, channel, structural);
            float edgeLength = sharedEdge.Length;
            float range = Mathf.Max(
                0.05f,
                Mathf.Max(
                    TriangleChannelRange(a, channel, structural),
                    TriangleChannelRange(b, channel, structural)));
            return
                (gradientA - gradientB).magnitude *
                Mathf.Max(edgeLength, 0.00001f) /
                range;
        }

        private static Vector3 TriangleScalarGradient(
            MassGenerator.FinalTriangleRecord triangle,
            int channel,
            bool structural)
        {
            Vector4 va = structural ? triangle.StructuralA : triangle.MaskA;
            Vector4 vb = structural ? triangle.StructuralB : triangle.MaskB;
            Vector4 vc = structural ? triangle.StructuralC : triangle.MaskC;
            float sa = va[channel];
            float sb = vb[channel];
            float sc = vc[channel];
            Vector3 e1 = triangle.B - triangle.A;
            Vector3 e2 = triangle.C - triangle.A;
            double d11 = Vector3.Dot(e1, e1);
            double d22 = Vector3.Dot(e2, e2);
            double d12 = Vector3.Dot(e1, e2);
            double determinant = d11 * d22 - d12 * d12;
            if (Math.Abs(determinant) <= 1e-20) return Vector3.zero;
            double c1 = ((sb - sa) * d22 - (sc - sa) * d12) / determinant;
            double c2 = ((sc - sa) * d11 - (sb - sa) * d12) / determinant;
            return e1 * (float)c1 + e2 * (float)c2;
        }

        private static float TriangleChannelRange(
            MassGenerator.FinalTriangleRecord triangle,
            int channel,
            bool structural)
        {
            Vector4 a = structural ? triangle.StructuralA : triangle.MaskA;
            Vector4 b = structural ? triangle.StructuralB : triangle.MaskB;
            Vector4 c = structural ? triangle.StructuralC : triangle.MaskC;
            return Mathf.Max(a[channel], Mathf.Max(b[channel], c[channel])) -
                Mathf.Min(a[channel], Mathf.Min(b[channel], c[channel]));
        }

        private static void AddEdge(
            Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>> edges,
            MassGenerator.FinalTriangleRecord triangle,
            Vector3 a,
            Vector3 b)
        {
            EdgeKey key = new EdgeKey(a, b);
            if (!edges.TryGetValue(
                key,
                out List<MassGenerator.FinalTriangleRecord> list))
            {
                list = new List<MassGenerator.FinalTriangleRecord>(2);
                edges.Add(key, list);
            }
            list.Add(triangle);
        }

        private static Vector4 SharedEdgeMask(
            MassGenerator.FinalTriangleRecord triangle,
            EdgeKey edge)
        {
            Vector4 sum = Vector4.zero;
            int count = 0;
            AddMask(triangle.A, triangle.MaskA, edge, ref sum, ref count);
            AddMask(triangle.B, triangle.MaskB, edge, ref sum, ref count);
            AddMask(triangle.C, triangle.MaskC, edge, ref sum, ref count);
            return count > 0 ? sum / count : sum;
        }

        private static void AddMask(
            Vector3 position,
            Vector4 mask,
            EdgeKey edge,
            ref Vector4 sum,
            ref int count)
        {
            QuantizedPoint point = new QuantizedPoint(position);
            if (!edge.Contains(point)) return;
            sum += mask;
            count++;
        }

        private static void CountBoundaryEdge(
            Dictionary<EdgeKey, int> counts,
            Dictionary<QuantizedPoint, Vector3> positions,
            Vector3 a,
            Vector3 b)
        {
            QuantizedPoint qa = new QuantizedPoint(a);
            QuantizedPoint qb = new QuantizedPoint(b);
            EdgeKey key = new EdgeKey(a, b);
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
            positions[qa] = a;
            positions[qb] = b;
        }

        private static void AddAdjacency(
            Dictionary<QuantizedPoint, List<QuantizedPoint>> adjacency,
            QuantizedPoint a,
            QuantizedPoint b)
        {
            if (!adjacency.TryGetValue(a, out List<QuantizedPoint> list))
            {
                list = new List<QuantizedPoint>(2);
                adjacency.Add(a, list);
            }
            if (!list.Contains(b)) list.Add(b);
        }

        private static Vector3 CalculatePolygonNormal(List<Vector3> vertices)
        {
            Vector3 normal = Vector3.zero;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 current = vertices[i];
                Vector3 next = vertices[(i + 1) % vertices.Count];
                normal.x += (current.y - next.y) * (current.z + next.z);
                normal.y += (current.z - next.z) * (current.x + next.x);
                normal.z += (current.x - next.x) * (current.y + next.y);
            }
            return N(normal);
        }

        private static int DominantAxis(Vector3 normal)
        {
            Vector3 absolute = new Vector3(
                Mathf.Abs(normal.x),
                Mathf.Abs(normal.y),
                Mathf.Abs(normal.z));
            if (absolute.x >= absolute.y && absolute.x >= absolute.z) return 0;
            return absolute.y >= absolute.z ? 1 : 2;
        }

        private static Vector2 Project2D(Vector3 point, int axis)
        {
            return axis switch
            {
                0 => new Vector2(point.y, point.z),
                1 => new Vector2(point.x, point.z),
                _ => new Vector2(point.x, point.y)
            };
        }

        private static float SignedArea(List<Vector2> polygon)
        {
            float area = 0f;
            for (int i = 0; i < polygon.Count; i++)
            {
                Vector2 a = polygon[i];
                Vector2 b = polygon[(i + 1) % polygon.Count];
                area += a.x * b.y - b.x * a.y;
            }
            return area * 0.5f;
        }

        private static float Cross2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private static bool PointInTriangle(
            Vector2 point,
            Vector2 a,
            Vector2 b,
            Vector2 c)
        {
            float c0 = Cross2D(b - a, point - a);
            float c1 = Cross2D(c - b, point - b);
            float c2 = Cross2D(a - c, point - c);
            bool negative = c0 < 0f || c1 < 0f || c2 < 0f;
            bool positive = c0 > 0f || c1 > 0f || c2 > 0f;
            return !(negative && positive);
        }

        private sealed class BevelResult
        {
            internal int EdgeId;
            internal int TriangleCount;
            internal int StructurallyInvalidTriangles;
            internal int NumericallyUnderResolvedTriangles;
            internal int ExtremeSliverTriangles;
            internal int UnderResolvedInternalAdjacencies;
            internal int InternalRenderNormalJumpCount;
            internal int InternalMaskJumpCount;
            internal int ValueGradientJumpCount;
            internal int StructuralGradientJumpCount;
            internal int Severity;
            internal double MaximumAspectRatio;
            internal double MinimumAngleDegrees = double.PositiveInfinity;
            internal float MaximumConditionedGeometricNormalJump;
            internal AlternativeTriangulationResult Alternative =
                new AlternativeTriangulationResult();
            internal readonly List<string> Evidence = new();
        }

        private sealed class AlternativeTriangulationResult
        {
            internal bool Audited;
            internal bool Improves;
            internal string Kind = "not-audited";
            internal int CandidatesAudited;
            internal int CandidatesRejected;
            internal int StructurallyInvalid;
            internal int NumericallyUnderResolved;
            internal int ExtremeSlivers;
            internal double MaximumAspectRatio;
            internal double MinimumAngleDegrees;
            internal double CurrentMaximumAspectRatio;
            internal double CurrentMinimumAngleDegrees;
        }

        private sealed class TriangulationCandidate
        {
            internal readonly string Kind;
            internal readonly TriangulationScore Score;
            internal TriangulationCandidate(string kind, TriangulationScore score)
            {
                Kind = kind;
                Score = score;
            }
        }

        private sealed class TriangulationScore
        {
            internal int TriangleCount;
            internal int StructurallyInvalid;
            internal int NumericallyUnderResolved;
            internal int ExtremeSlivers;
            internal double MaximumAspectRatio;
            internal double MinimumAngleDegrees;
        }

        private enum Stage
        {
            CaptureSuspect,
            CaptureReference,
            BuildSubjects,
            AnalyzeSubjects,
            PrepareRenderAudit,
            RenderAudit,
            Finalize
        }

        private readonly struct QuantizedPoint :
            IEquatable<QuantizedPoint>,
            IComparable<QuantizedPoint>
        {
            internal readonly int X;
            internal readonly int Y;
            internal readonly int Z;
            internal QuantizedPoint(Vector3 position)
            {
                X = Mathf.RoundToInt(position.x * PositionQuantization);
                Y = Mathf.RoundToInt(position.y * PositionQuantization);
                Z = Mathf.RoundToInt(position.z * PositionQuantization);
            }
            public int CompareTo(QuantizedPoint other)
            {
                int result = X.CompareTo(other.X);
                if (result != 0) return result;
                result = Y.CompareTo(other.Y);
                return result != 0 ? result : Z.CompareTo(other.Z);
            }
            public bool Equals(QuantizedPoint other) =>
                X == other.X && Y == other.Y && Z == other.Z;
            public override bool Equals(object obj) =>
                obj is QuantizedPoint other && Equals(other);
            public override int GetHashCode() =>
                ((X * 397) ^ Y) * 397 ^ Z;
            public override string ToString() => X + "," + Y + "," + Z;
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            internal readonly QuantizedPoint A;
            internal readonly QuantizedPoint B;
            internal EdgeKey(Vector3 a, Vector3 b)
            {
                QuantizedPoint qa = new QuantizedPoint(a);
                QuantizedPoint qb = new QuantizedPoint(b);
                if (qa.CompareTo(qb) <= 0)
                {
                    A = qa;
                    B = qb;
                }
                else
                {
                    A = qb;
                    B = qa;
                }
            }
            internal bool Contains(QuantizedPoint point) =>
                A.Equals(point) || B.Equals(point);
            internal float Length
            {
                get
                {
                    double dx = A.X - B.X;
                    double dy = A.Y - B.Y;
                    double dz = A.Z - B.Z;
                    return (float)(Math.Sqrt(dx * dx + dy * dy + dz * dz) /
                        PositionQuantization);
                }
            }
            public bool Equals(EdgeKey other) =>
                A.Equals(other.A) && B.Equals(other.B);
            public override bool Equals(object obj) =>
                obj is EdgeKey other && Equals(other);
            public override int GetHashCode() =>
                A.GetHashCode() * 397 ^ B.GetHashCode();
            public override string ToString() => A + "|" + B;
        }

        private static Vector4[] ReadUv(Mesh mesh, int channel, int count)
        {
            List<Vector4> values = new List<Vector4>(count);
            mesh.GetUVs(channel, values);
            return values.Count == count
                ? values.ToArray()
                : Enumerable.Repeat(Vector4.zero, count).ToArray();
        }

        private static float Angle(Vector3 a, Vector3 b)
        {
            a = N(a);
            b = N(b);
            if (a == Vector3.zero || b == Vector3.zero) return 0f;
            return Mathf.Acos(
                Mathf.Clamp(Vector3.Dot(a, b), -1f, 1f)) *
                Mathf.Rad2Deg;
        }

        private static Vector3 N(Vector3 value)
        {
            double magnitudeSquared =
                (double)value.x * value.x +
                (double)value.y * value.y +
                (double)value.z * value.z;
            if (magnitudeSquared <= 1e-30) return Vector3.zero;
            double inverse = 1.0 / Math.Sqrt(magnitudeSquared);
            return new Vector3(
                (float)(value.x * inverse),
                (float)(value.y * inverse),
                (float)(value.z * inverse));
        }

        private static string F(float value) =>
            value.ToString("R", CultureInfo.InvariantCulture);
        private static string D(double value) =>
            value.ToString("R", CultureInfo.InvariantCulture);
        private static string V(Vector3 value) =>
            F(value.x) + "," + F(value.y) + "," + F(value.z);
    }
}
