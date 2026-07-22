using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Masses
{
    public static partial class MassGenerator
    {
        #region Edge wear orchestration

        private static TriangleSoup ApplyGeneratedEdgeWearBevels(
            List<PolygonFace> faces,
            MassRecipe recipe,
            MassSurfaceFeatureSettings? surfaceFeatures,
            EdgeWearEvaluationMode evaluationMode,
            int boundedEdgeOrdinal,
            out PlaneCutBevelPreviewStatus previewStatus,
            out BoundedEdgePreviewStatus boundedPreviewStatus,
            out UnifiedEdgeWearPreviewStatus unifiedPreviewStatus)
        {
            previewStatus = default;
            boundedPreviewStatus = default;
            unifiedPreviewStatus = default;
            if (evaluationMode == EdgeWearEvaluationMode.None)
            {
                return null;
            }

            bool applyPlaneCutBevelPreview =
                evaluationMode == EdgeWearEvaluationMode.PlaneCutPreview;
            bool runLegacyDiagnosticAudit =
                evaluationMode == EdgeWearEvaluationMode.LegacyDiagnosticAudit;
            bool applyBoundedSingleEdgePreview =
                evaluationMode ==
                    EdgeWearEvaluationMode.BoundedSingleEdgePreview;
            bool runCornerDamageGeometryPreview =
                evaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageGeometryPreview;
            bool runCornerDamageIntegrationPreview =
                evaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageIntegrationPreview;
            bool runCornerDamagePreview =
                runCornerDamageGeometryPreview ||
                runCornerDamageIntegrationPreview;
            bool applyUnifiedBoundedPreview =
                evaluationMode ==
                    EdgeWearEvaluationMode.UnifiedBoundedPreview;
            bool runUnifiedBatchAudit =
                evaluationMode == EdgeWearEvaluationMode.UnifiedBatchAudit;
            bool runUnifiedPreviewBatchAudit =
                evaluationMode ==
                    EdgeWearEvaluationMode.UnifiedPreviewBatchAudit;
            bool buildSourceEdgeIndexDebug =
                evaluationMode == EdgeWearEvaluationMode.SourceEdgeIndexDebug;
            bool runCornerDamageTransactionAudit =
                evaluationMode ==
                    EdgeWearEvaluationMode.CornerDamageTransactionAudit;
            bool runUnifiedEvaluation =
                applyUnifiedBoundedPreview ||
                runCornerDamageIntegrationPreview ||
                runUnifiedBatchAudit ||
                runUnifiedPreviewBatchAudit ||
                buildSourceEdgeIndexDebug;
            bool includeAllGeometricCandidates = runUnifiedBatchAudit;
            bool logUnifiedAudit =
                !buildSourceEdgeIndexDebug &&
                !runCornerDamagePreview;
            if (!surfaceFeatures.HasValue || faces == null || faces.Count < 4)
            {
                return null;
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            float amount01 = Mathf.Clamp01(settings.EdgeWearAmount * 0.5f);
            if (amount01 <= 0.0001f &&
                !runCornerDamageTransactionAudit &&
                !runCornerDamagePreview)
            {
                return null;
            }

            Bounds bounds = CalculateFaceBounds(faces);
            float maximumDimension = Mathf.Max(
                0.0001f,
                Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z)));

            float requestedWidth = ResolveGeneratedEdgeWearWidth(
                maximumDimension,
                settings.EdgeWearWidth);
            float minimumStyleWidth = ResolveGeneratedEdgeWearWidth(
                maximumDimension,
                EdgeWearMinimumStyleWidthSetting);
            EdgeWearMicroTopologyNormalizationResult
                microTopologyNormalization =
                    NormalizeEdgeWearMicroTopology(
                        faces,
                        maximumDimension,
                        minimumStyleWidth);
            List<PolygonFace> edgeWearFaces =
                microTopologyNormalization.Faces ?? faces;
            Bounds edgeWearBounds = CalculateFaceBounds(edgeWearFaces);
            CornerDamageTransactionAuditResult cornerDamageTransaction = null;
            float capRingRequestedWidth = 0f;
            float capRingOrdinaryLimit = 0f;
            float capRingDepthLimit = 0f;
            float capRingEdgeLimit = 0f;
            string capRingWinningLimit = "none";
            if (runCornerDamageTransactionAudit || runCornerDamagePreview)
            {
                cornerDamageTransaction = EvaluateCornerDamageTransaction(
                    edgeWearFaces,
                    microTopologyNormalization,
                    edgeWearBounds,
                    maximumDimension,
                    recipe,
                    settings);
                if (runCornerDamageTransactionAudit)
                {
                    CaptureCornerDamageTransactionAudit(
                        cornerDamageTransaction);
                    return null;
                }

                if (runCornerDamageIntegrationPreview)
                {
                    capRingRequestedWidth =
                        ResolveCornerDamageCapRingRequestedWidth(
                            cornerDamageTransaction,
                            requestedWidth,
                            settings.CornerChipCapRingWidthScale,
                            out capRingOrdinaryLimit,
                            out capRingDepthLimit,
                            out capRingEdgeLimit,
                            out capRingWinningLimit);
                }
                BeginCornerDamagePreviewCapture(
                    runCornerDamageGeometryPreview
                        ? CornerDamagePreviewKind.GeometryOnly
                        : CornerDamagePreviewKind.WithEdgeWear,
                    cornerDamageTransaction,
                    settings,
                    requestedWidth,
                    capRingOrdinaryLimit,
                    capRingDepthLimit,
                    capRingEdgeLimit,
                    capRingWinningLimit,
                    capRingRequestedWidth);
                if (cornerDamageTransaction == null ||
                    !cornerDamageTransaction.Succeeded ||
                    cornerDamageTransaction.AcceptedFaces == null ||
                    cornerDamageTransaction.AcceptedCapFace == null)
                {
                    CaptureCornerDamagePreviewBlocker(
                        cornerDamageTransaction == null
                            ? "corner-damage transaction capture was unavailable"
                            : cornerDamageTransaction.Diagnostic);
                    return null;
                }
                if (runCornerDamageGeometryPreview)
                {
                    BoundedSingleEdgeAuditResult geometryAudit = default;
                    bool triangulated = TryTriangulateBoundedPreviewFaces(
                        cornerDamageTransaction.AcceptedFaces,
                        cornerDamageTransaction.MinimumStableFaceArea,
                        ref geometryAudit,
                        out TriangleSoup geometryPreviewSoup,
                        out string geometryBlocker);
                    string geometryDiagnostic = triangulated
                        ? "certified corner chip geometry applied without edge-wear bevel construction"
                        : geometryBlocker;
                    unifiedPreviewStatus =
                        new UnifiedEdgeWearPreviewStatus(
                            triangulated && geometryPreviewSoup != null,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            geometryPreviewSoup == null
                                ? 0
                                : geometryPreviewSoup.Positions.Count / 3,
                            geometryDiagnostic,
                            Array.Empty<EdgeWearDebugEdgeRecord>());
                    CaptureCornerDamagePreviewOutcome(
                        unifiedPreviewStatus,
                        geometryBlocker);
                    if (unifiedPreviewStatus.PreviewApplied)
                    {
                        return geometryPreviewSoup;
                    }
                    return null;
                }
                if (cornerDamageTransaction.AcceptedConstructionFaces ==
                        null ||
                    cornerDamageTransaction.
                        ConstructionSourceFaceCountExpected <= 0 ||
                    cornerDamageTransaction.
                        ConstructionSourceFaceCountAttributed !=
                    cornerDamageTransaction.
                        ConstructionSourceFaceCountExpected)
                {
                    CaptureCornerDamagePreviewBlocker(
                        "corner-damage construction source-face attribution is incomplete");
                    return null;
                }
                if (capRingRequestedWidth + PointMergeDistance <
                    minimumStyleWidth)
                {
                    CaptureCornerDamagePreviewBlocker(
                        "cap-ring requested width is below the minimum stable style width");
                    return null;
                }

                edgeWearFaces =
                    cornerDamageTransaction.AcceptedConstructionFaces;
                edgeWearBounds = CalculateFaceBounds(edgeWearFaces);
            }

            List<EdgeWearBevelCandidate> candidates =
                BuildEdgeWearBevelCandidates(
                    edgeWearFaces,
                    edgeWearBounds,
                    maximumDimension,
                    recipe,
                    settings,
                    amount01,
                    requestedWidth,
                    includeAllGeometricCandidates,
                    microTopologyNormalization,
                    cornerDamageTransaction,
                    capRingRequestedWidth,
                    out EdgeWearCoverageAudit coverageAudit);
            if (candidates.Count == 0)
            {
                const string noViableCandidateReason =
                    "no geometrically viable edge-wear candidates";
                LogChamferReadiness(
                    new ChamferReadinessStats(0, 0),
                    false,
                    noViableCandidateReason);
                if (runUnifiedEvaluation)
                {
                    PlaneCutBevelAuditResult emptyAudit = default;
                    emptyAudit.CoverageAudit = coverageAudit;
                    emptyAudit.Diagnostic = noViableCandidateReason;
                    if (logUnifiedAudit)
                    {
                        LogUnifiedAllEdgeBevelAudit(
                            emptyAudit,
                            false,
                            noViableCandidateReason);
                    }
                    unifiedPreviewStatus =
                        new UnifiedEdgeWearPreviewStatus(
                            false,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            0,
                            noViableCandidateReason,
                            BuildSourceEdgeIndexDebugEdges(
                                edgeWearFaces,
                                coverageAudit));
                }
                if (runCornerDamagePreview)
                {
                    CaptureCornerDamagePreviewBlocker(
                        noViableCandidateReason);
                }
                return null;
            }

            if (runCornerDamagePreview)
            {
                candidates.Sort(CompareCornerDamagePreviewCandidates);
            }
            else
            {
                candidates.Sort((left, right) =>
                    right.Score.CompareTo(left.Score));
            }

            int mandatoryCandidateCount = 0;
            if (runCornerDamagePreview)
            {
                for (int candidateIndex = 0;
                     candidateIndex < candidates.Count;
                     candidateIndex++)
                {
                    if (candidates[candidateIndex].Mandatory)
                    {
                        mandatoryCandidateCount++;
                    }
                }
            }
            float coverage01 = Mathf.Clamp01(settings.EdgeWearCoverage * 0.5f);
            int ordinaryCandidateCount =
                candidates.Count - mandatoryCandidateCount;
            int selectedCount = runCornerDamagePreview
                ? mandatoryCandidateCount + Mathf.Clamp(
                    Mathf.CeilToInt(ordinaryCandidateCount * coverage01),
                    0,
                    ordinaryCandidateCount)
                : Mathf.Clamp(
                    Mathf.CeilToInt(candidates.Count * coverage01),
                    0,
                    candidates.Count);
            if (runCornerDamagePreview)
            {
                CaptureCornerDamagePreviewCandidateSelection(
                    cornerDamageTransaction == null
                        ? 0
                        : cornerDamageTransaction.CapRingKeys.Count,
                    mandatoryCandidateCount,
                    Mathf.Min(mandatoryCandidateCount, selectedCount));
            }
            CaptureEdgeWearArtisticSelectionAudit(
                coverageAudit,
                candidates,
                selectedCount);
            if (selectedCount <= 0)
            {
                if (runCornerDamagePreview)
                {
                    CaptureCornerDamagePreviewBlocker(
                        "corner preview selected no bevel candidates");
                }
                return null;
            }

            float minimumStableEdgeLength = maximumDimension * 0.0012f;
            ChamferReadinessStats stats = new ChamferReadinessStats(
                candidates.Count,
                selectedCount);

            bool ready = TryBuildChamferTopologyContext(
                edgeWearFaces,
                candidates,
                selectedCount,
                minimumStableEdgeLength,
                ref stats,
                out ChamferTopologyContext context,
                out string blocker);

            SortedSet<int> materialWidthRecoveryTargets =
                new SortedSet<int>();
            if (ready)
            {
                MapEdgeWearCoverageAuditToGraph(
                    coverageAudit,
                    context);
                materialWidthRecoveryTargets =
                    CaptureImmutableMaterialWidthRecoveryTargets(
                        context,
                        coverageAudit);
            }

            LogChamferReadiness(stats, ready, blocker);
            if (!ready && runUnifiedEvaluation)
            {
                PlaneCutBevelAuditResult readinessAudit = default;
                readinessAudit.CoverageAudit = coverageAudit;
                readinessAudit.SelectedEdgeCount = selectedCount;
                readinessAudit.Diagnostic = string.IsNullOrEmpty(blocker)
                    ? "viable edge topology context failed"
                    : blocker;
                if (logUnifiedAudit)
                {
                    LogUnifiedAllEdgeBevelAudit(
                        readinessAudit,
                        false,
                        readinessAudit.Diagnostic);
                }
                unifiedPreviewStatus =
                    new UnifiedEdgeWearPreviewStatus(
                        false,
                        selectedCount,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        readinessAudit.Diagnostic,
                        BuildSourceEdgeIndexDebugEdges(
                            edgeWearFaces,
                            coverageAudit));
                if (runCornerDamagePreview)
                {
                    CaptureCornerDamagePreviewOutcome(
                        unifiedPreviewStatus,
                        readinessAudit.Diagnostic);
                }
            }

            if (ready)
            {
                float minimumStableFaceArea =
                    maximumDimension * maximumDimension * 0.000001f;

                if (runUnifiedEvaluation)
                {
                    ChamferCornerStats cornerStats =
                        new ChamferCornerStats();
                    bool cornersReady =
                        TryAuditCertifiedBaselineAugmentation(
                            edgeWearFaces,
                            context,
                            recipe,
                            requestedWidth,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            coverageAudit,
                            materialWidthRecoveryTargets,
                            ref cornerStats,
                            out ChamferCornerSolution cornerSolution,
                            out PlaneCutBevelAuditResult allEdgeAudit,
                            out TriangleSoup allEdgePreviewSoup,
                            out string cornerBlocker);
                    EdgeWearCoverageAudit effectiveCoverageAudit =
                        allEdgeAudit.CoverageAudit ?? coverageAudit;

                    if (logUnifiedAudit)
                    {
                        LogUnifiedAllEdgeBevelAudit(
                            allEdgeAudit,
                            cornersReady,
                            cornerBlocker);
                    }

                    bool previewApplied =
                        cornersReady &&
                        allEdgeAudit.GeometryValid == 1 &&
                        allEdgePreviewSoup != null;
                    unifiedPreviewStatus =
                        new UnifiedEdgeWearPreviewStatus(
                            previewApplied,
                            allEdgeAudit.SelectedEdgeCount,
                            allEdgeAudit.ActiveEdgeCount,
                            allEdgeAudit.PlanesBuilt,
                            allEdgeAudit.PlanesDeferred,
                            allEdgeAudit.PlanesRejected,
                            allEdgeAudit.BevelRegionFaceCount,
                            0,
                            allEdgeAudit.PreviewTriangleCount,
                            allEdgeAudit.Diagnostic,
                            BuildUnifiedEdgeWearDebugEdges(
                                context,
                                effectiveCoverageAudit,
                                allEdgeAudit.DebugFocusEdgeIndices));
                    if (runCornerDamagePreview)
                    {
                        CaptureCornerDamagePreviewOutcome(
                            unifiedPreviewStatus,
                            cornerBlocker);
                    }
                    if (previewApplied)
                    {
                        return allEdgePreviewSoup;
                    }
                }
                else if (applyBoundedSingleEdgePreview)
                {
                    float boundedRequestedWidth = requestedWidth;
                    List<EdgeWearSelectedGraphEdge> boundedEligible =
                        BuildBoundedSingleEdgeEligibleList(context);
                    if (boundedEdgeOrdinal >= 0 &&
                        boundedEdgeOrdinal < boundedEligible.Count)
                    {
                        boundedRequestedWidth =
                            ResolveEdgeWearRequestedWidth(
                                coverageAudit,
                                boundedEligible[boundedEdgeOrdinal]
                                    .GraphEdgeIndex,
                                requestedWidth);
                    }
                    BoundedSingleEdgeAuditResult boundedAudit =
                        AuditBoundedSingleEdgeBevel(
                            edgeWearFaces,
                            context,
                            boundedEdgeOrdinal,
                            boundedRequestedWidth,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            true,
                            out TriangleSoup boundedPreviewSoup);
                    LogBoundedSingleEdgeAudit(boundedAudit);

                    bool previewApplied =
                        boundedAudit.GeometryValid == 1 &&
                        boundedPreviewSoup != null;
                    boundedPreviewStatus =
                        new BoundedEdgePreviewStatus(
                            previewApplied,
                            boundedAudit.CandidateCount,
                            boundedAudit.SelectedOrdinal,
                            boundedAudit.SourceEdgeIndex,
                            boundedAudit.BevelFaceCount,
                            boundedAudit.EndpointCapCount,
                            boundedAudit.ModifiedSourceFaceCount,
                            boundedAudit.ForeignSourceFaceModifiedCount,
                            boundedAudit.RailDeviation,
                            boundedAudit.MaximumExtentBeyondRails,
                            boundedAudit.Diagnostic);
                    if (previewApplied)
                    {
                        return boundedPreviewSoup;
                    }
                }
                else
                {
                    ChamferCornerStats cornerStats =
                        new ChamferCornerStats();
                    bool cornersReady = AuditExplicitChamferCornerSolution(
                        edgeWearFaces,
                        context,
                        requestedWidth,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        coverageAudit,
                        null,
                        ref cornerStats,
                        out ChamferCornerSolution cornerSolution,
                        out string cornerBlocker);

                    LogChamferCornerAudit(
                        cornerStats,
                        cornersReady,
                        cornerBlocker);
                    if (cornersReady)
                    {
                        if (applyPlaneCutBevelPreview)
                        {
                            PlaneCutBevelAuditResult planeCutAudit =
                                AuditPlaneCutBevelKernel(
                                    edgeWearFaces,
                                    context,
                                    cornerSolution,
                                    minimumStableEdgeLength,
                                    minimumStableFaceArea,
                                    coverageAudit,
                                    true,
                                    out TriangleSoup planeCutPreviewSoup);
                            LogPlaneCutBevelAudit(planeCutAudit);

                            bool previewApplied =
                                planeCutAudit.GeometryValid == 1 &&
                                planeCutPreviewSoup != null;
                            previewStatus = new PlaneCutBevelPreviewStatus(
                                previewApplied,
                                planeCutAudit.ActiveEdgeCount,
                                planeCutAudit.PlanesBuilt,
                                planeCutAudit.PlanesDeferred,
                                planeCutAudit.PlanesRejected,
                                planeCutAudit.Diagnostic);
                            if (previewApplied)
                            {
                                return planeCutPreviewSoup;
                            }
                        }
                        else if (runLegacyDiagnosticAudit)
                        {
                            ChamferEmissionStats emissionStats =
                                new ChamferEmissionStats();
                            bool emissionReady =
                                AuditProvisionalChamferEmission(
                                    edgeWearFaces,
                                    context,
                                    cornerSolution,
                                    minimumStableEdgeLength,
                                    minimumStableFaceArea,
                                    ref emissionStats,
                                    out string emissionBlocker);
                            LogChamferEmissionAudit(
                                emissionStats,
                                emissionReady,
                                emissionBlocker,
                                default);
                        }
                    }
                }
            }

            // EW-C2S4 preserves raw face/segment provenance, segments every
            // source-compatible provisional T-junction (including guarded preserved
            // source-boundary subdivision) before boundary normalization, updates
            // split boundary ownership, audits compact unique failures, then discards
            // the provisional result. The original PolygonFace list remains rendered
            // until explicit vertex patches pass.
            return null;
        }

        private static float ResolveGeneratedEdgeWearWidth(
            float maximumDimension,
            float widthSetting)
        {
            float clampedSetting = Mathf.Clamp(widthSetting, 0.05f, 2f);
            float relativeWidth;
            if (clampedSetting < 0.25f)
            {
                relativeWidth = Mathf.Lerp(
                    0.0015f,
                    0.006f,
                    Mathf.InverseLerp(0.05f, 0.25f, clampedSetting));
            }
            else
            {
                relativeWidth = Mathf.Lerp(
                    0.006f,
                    0.028f,
                    Mathf.InverseLerp(0.25f, 2f, clampedSetting));
            }

            return Mathf.Max(0.0001f, maximumDimension) * relativeWidth;
        }

        private static bool AuditProvisionalChamferEmission(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            ChamferCornerSolution solution,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            ref ChamferEmissionStats stats,
            out string blocker)
        {
            blocker = string.Empty;
            stats.SourceFaceCount = context.Graph.Faces.Count;
            stats.CandidateSelectedEdgeCount = context.SelectedSourceEdges.Count;
            List<ChamferProvisionalFaceRecord> provisionalFaceRecords =
                new List<ChamferProvisionalFaceRecord>(
                    context.Graph.Faces.Count +
                    context.SelectedSourceEdges.Count);
            if (!TryBuildChamferSourceBoundaryRecords(
                    context,
                    solution,
                    out List<ChamferSourceBoundaryRecord>
                        sourceBoundaryRecords,
                    out blocker))
            {
                return false;
            }
            List<ChamferExpectedVertexBoundary> vertexBoundaries =
                new List<ChamferExpectedVertexBoundary>(
                    context.SelectedSourceEdges.Count * 4);
            HashSet<TopologyEdgeKey> expectedVertexBoundaryEdges =
                new HashSet<TopologyEdgeKey>();

            for (int faceIndex = 0;
                 faceIndex < context.Graph.Faces.Count;
                 faceIndex++)
            {
                stats.ReplacementFacesAttempted++;
                EdgeWearGraphFace graphFace = context.Graph.Faces[faceIndex];
                PolygonFace sourceFace = sourceFaces[graphFace.SourceFaceIndex];
                List<Vector3> vertices = new List<Vector3>(
                    graphFace.VertexIndices.Count * 3);
                List<ChamferExpectedVertexBoundary> localBoundaries =
                    new List<ChamferExpectedVertexBoundary>(
                        graphFace.VertexIndices.Count * 2);
                for (int i = 0; i < graphFace.VertexIndices.Count; i++)
                {
                    int startVertex = graphFace.VertexIndices[i];
                    int endVertex = graphFace.VertexIndices[
                        (i + 1) % graphFace.VertexIndices.Count];
                    int sourceEdgeIndex = graphFace.EdgeIndices[i];
                    AppendChamferReplacementEdgeChain(
                        faceIndex,
                        startVertex,
                        endVertex,
                        sourceEdgeIndex,
                        solution.Corners,
                        solution.SharedSpans,
                        vertices,
                        localBoundaries);
                }
                HashSet<TopologyEdgeKey> initialRetraceRemovedKeys =
                    new HashSet<TopologyEdgeKey>();
                ReduceChamferFaceRetraces(
                        vertices,
                        initialRetraceRemovedKeys);

                if (vertices.Count < 3)
                {
                    stats.FaceLocalNormalizationFailureCount++;
                    blocker = "a shared-span replacement face collapses during exact retrace normalization";
                    return false;
                }
                if (!TryFindDuplicateChamferFaceEdge(
                        vertices,
                        out _,
                        out _,
                        out _))
                {
                    stats.FaceLocalDuplicateEdgeFailureCount++;
                    blocker = "a shared-span replacement face contains a repeated topology edge";
                    return false;
                }

                HashSet<TopologyEdgeKey> emittedFaceEdgeKeys =
                    BuildChamferFaceEdgeKeySet(vertices);
                for (int boundaryIndex = 0;
                     boundaryIndex < localBoundaries.Count;
                     boundaryIndex++)
                {
                    ChamferExpectedVertexBoundary boundary =
                        localBoundaries[boundaryIndex];
                    if (emittedFaceEdgeKeys.Contains(boundary.Key))
                    {
                        vertexBoundaries.Add(boundary);
                    }
                    else if (initialRetraceRemovedKeys.Contains(boundary.Key))
                    {
                    }
                    else
                    {
                        stats.StaleBoundaryRegistrationFailureCount++;
                        blocker = "a replacement-face boundary registration has no emitted topology edge";
                        return false;
                    }
                }

                if (CalculatePolygonArea(vertices) <= minimumStableFaceArea ||
                    !IsFinite(CalculatePolygonNormal(vertices)))
                {
                    blocker = "a shared-span replacement face is geometrically invalid";
                    return false;
                }

                Vector3 replacementNormal = CalculatePolygonNormal(vertices);
                if (Vector3.Dot(replacementNormal, sourceFace.Normal) <= 0.25f)
                {
                    blocker = "a shared-span replacement face has invalid winding";
                    return false;
                }

                PolygonFace replacementFace = new PolygonFace(
                    vertices,
                    sourceFace.Normal,
                    sourceFace.Feature,
                    sourceFace.FeatureStrength);
                provisionalFaceRecords.Add(new ChamferProvisionalFaceRecord(
                    replacementFace,
                    ChamferProvisionalFaceKind.ReplacementBase,
                    faceIndex,
                    -1));
                stats.ReplacementFacesBuilt++;
            }

            Dictionary<int, EdgeWearSelectedGraphEdge> selectedByGraphEdge =
                new Dictionary<int, EdgeWearSelectedGraphEdge>();
            for (int i = 0; i < context.SelectedEdges.Count; i++)
            {
                selectedByGraphEdge[context.SelectedEdges[i].GraphEdgeIndex] =
                    context.SelectedEdges[i];
            }

            foreach (int edgeIndex in context.SelectedSourceEdges)
            {
                if (!solution.WidthByEdge.TryGetValue(edgeIndex, out float width) ||
                    width <= PointMergeDistance)
                {
                    stats.DeferredSelectedEdgeCount++;
                    continue;
                }

                stats.ActiveSelectedEdgeCount++;
                stats.BevelStripsAttempted++;
                EdgeWearGraphEdge edge = context.Graph.Edges[edgeIndex];
                if (edge.FaceA < 0 || edge.FaceB < 0 ||
                    !selectedByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearSelectedGraphEdge selected))
                {
                    blocker = "an active selected edge lacks two incident faces or candidate provenance";
                    return false;
                }

                Vector3 a0 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexA)].Position;
                Vector3 b0 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceA, edge.VertexB)].Position;
                Vector3 a1 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexA)].Position;
                Vector3 b1 = solution.Corners[
                    new ChamferFaceCornerKey(edge.FaceB, edge.VertexB)].Position;

                List<Vector3> strip = new List<Vector3> { a0, b0, b1, a1 };
                ReduceChamferFaceRetraces(strip, null);
                if (strip.Count < 3)
                {
                    stats.FaceLocalNormalizationFailureCount++;
                    blocker = "an active selected edge collapses during exact bevel retrace normalization";
                    return false;
                }
                if (!TryFindDuplicateChamferFaceEdge(
                        strip,
                        out _,
                        out _,
                        out _))
                {
                    stats.FaceLocalDuplicateEdgeFailureCount++;
                    blocker = "an active selected edge produces a repeated bevel topology edge";
                    return false;
                }
                Vector3 expectedNormal = selected.Candidate.BevelNormal;
                Vector3 stripNormal = CalculatePolygonNormal(strip);
                if (!IsFinite(stripNormal) ||
                    stripNormal.sqrMagnitude <= 0.00000001f)
                {
                    blocker = "an active selected edge produces an invalid bevel strip normal";
                    return false;
                }
                if (Vector3.Dot(stripNormal, expectedNormal) < 0f)
                {
                    strip.Reverse();
                    stripNormal = -stripNormal;
                }
                if (CalculatePolygonArea(strip) <= minimumStableFaceArea)
                {
                    blocker = "an active selected edge produces an insufficient bevel-strip area";
                    return false;
                }

                PolygonFace bevelFace = new PolygonFace(
                    strip,
                    stripNormal,
                    PolygonFaceFeature.ConvexEdgeWear,
                    selected.Candidate.Strength);
                provisionalFaceRecords.Add(new ChamferProvisionalFaceRecord(
                    bevelFace,
                    ChamferProvisionalFaceKind.BevelStrip,
                    -1,
                    edgeIndex));
                stats.BevelStripsBuilt++;

                AddExpectedVertexBoundary(
                    vertexBoundaries,
                    edge.VertexA,
                    edgeIndex,
                    edge.FaceA,
                    ChamferVertexBoundaryKind.BevelStripEndpoint,
                    a0,
                    a1);
                AddExpectedVertexBoundary(
                    vertexBoundaries,
                    edge.VertexB,
                    edgeIndex,
                    edge.FaceA,
                    ChamferVertexBoundaryKind.BevelStripEndpoint,
                    b0,
                    b1);
            }

            for (int i = 0; i < vertexBoundaries.Count; i++)
            {
            }

            SegmentRawChamferTJunctions(
                provisionalFaceRecords,
                context,
                solution.SharedSpans,
                vertexBoundaries,
                sourceBoundaryRecords,
                minimumStableEdgeLength,
                ref stats);

            HashSet<TopologyEdgeKey> postSegmentationRetraceRemovedKeys =
                new HashSet<TopologyEdgeKey>();
            if (!NormalizeChamferProvisionalFaceWalks(
                    provisionalFaceRecords,
                    minimumStableFaceArea,
                    postSegmentationRetraceRemovedKeys,
                    ref stats,
                    out blocker))
            {
                return false;
            }

            List<PolygonFace> provisionalFaces =
                ExtractChamferProvisionalFaces(provisionalFaceRecords);
            Dictionary<TopologyEdgeKey, int> useCounts =
                BuildTopologyEdgeUseCounts(provisionalFaces);
            RemoveRetraceDeletedChamferBoundaries(
                vertexBoundaries,
                useCounts,
                postSegmentationRetraceRemovedKeys,
                ref stats);


            HashSet<TopologyEdgeKey> sourceBoundarySegmentKeys =
                BuildChamferSourceBoundarySegmentKeys(
                    sourceBoundaryRecords);
            List<ChamferProvisionalSegmentRecord> finalSegments =
                BuildChamferProvisionalSegmentRecords(
                    provisionalFaceRecords,
                    vertexBoundaries,
                    sourceBoundarySegmentKeys,
                    solution.SharedSpans);
            List<ChamferExpectedVertexBoundary> normalizedVertexBoundaries =
                NormalizeChamferVertexBoundaries(
                    vertexBoundaries,
                    useCounts,
                    finalSegments,
                    ref stats);
            for (int i = 0; i < normalizedVertexBoundaries.Count; i++)
            {
                expectedVertexBoundaryEdges.Add(
                    normalizedVertexBoundaries[i].Key);
            }

            Dictionary<
                TopologyEdgeKey,
                List<ChamferSourceBoundaryChildOccurrence>>
                rawSourceBoundaryOccurrences =
                    BuildChamferSourceBoundaryChildOccurrences(
                        sourceBoundaryRecords);
            NormalizeChamferSourceBoundaryLoops(
                sourceBoundaryRecords,
                useCounts,
                expectedVertexBoundaryEdges,
                finalSegments,
                ref stats);
            CollapseChamferSourceBoundaryTerminalTransferAliases(
                sourceBoundaryRecords,
                rawSourceBoundaryOccurrences,
                useCounts,
                expectedVertexBoundaryEdges,
                finalSegments,
                ref stats);

            HashSet<TopologyEdgeKey> expectedSourceBoundaryEdges =
                AuditChamferSourceBoundaryOwnership(
                    sourceBoundaryRecords,
                    useCounts,
                    expectedVertexBoundaryEdges,
                    finalSegments,
                    rawSourceBoundaryOccurrences,
                    ref stats);

            AuditExpectedVertexBoundaryComponents(
                normalizedVertexBoundaries,
                ref stats);

            HashSet<TopologyEdgeKey> actualOpenEdges =
                new HashSet<TopologyEdgeKey>();
            foreach (KeyValuePair<TopologyEdgeKey, int> pair in useCounts)
            {
                if (pair.Value == 1)
                {
                    actualOpenEdges.Add(pair.Key);
                }
                else if (pair.Value > 2)
                {
                    stats.ProvisionalNonManifoldEdgeCount++;
                }
            }
            stats.ProvisionalOpenEdgeCount = actualOpenEdges.Count;

            foreach (TopologyEdgeKey key in expectedVertexBoundaryEdges)
            {
                if (actualOpenEdges.Contains(key))
                {
                }
                else
                {
                    stats.MissingExpectedVertexBoundaryEdgeCount++;
                }
            }
            foreach (TopologyEdgeKey key in actualOpenEdges)
            {
                if (!expectedSourceBoundaryEdges.Contains(key) &&
                    !expectedVertexBoundaryEdges.Contains(key))
                {
                    stats.UnexpectedProvisionalOpenEdgeCount++;
                }
            }

            EdgeWearTopologyStats topology = AuditEdgeWearTopology(
                provisionalFaces,
                minimumStableEdgeLength);
            stats.ProvisionalTJunctionCount = topology.TJunctionCount;
            stats.ProvisionalNonManifoldEdgeCount = Mathf.Max(
                stats.ProvisionalNonManifoldEdgeCount,
                topology.NonManifoldEdgeCount);

            if (stats.MatchedSourceBoundaryEdgeCount !=
                    stats.ExpectedSourceBoundaryEdgeCount ||
                stats.MissingExpectedVertexBoundaryEdgeCount > 0 ||
                stats.UnexpectedProvisionalOpenEdgeCount > 0 ||
                stats.ProvisionalNonManifoldEdgeCount > 0 ||
                stats.ProvisionalTJunctionCount > 0 ||
                stats.VertexBoundarySameOwnerDuplicateFailureCount > 0 ||
                stats.VertexBoundaryMultiOwnerFailureCount > 0 ||
                stats.StaleBoundaryRegistrationFailureCount > 0 ||
                stats.FaceLocalNormalizationFailureCount > 0 ||
                stats.FaceLocalDuplicateEdgeFailureCount > 0 ||
                stats.SourceBoundaryTerminalTransferFailureCount > 0 ||
                stats.SourceBoundaryChildIncidenceFailureCount > 0 ||
                stats.SourceBoundaryDuplicateChildKeyFailureCount > 0 ||
                stats.SourceBoundaryLoopNormalizationFailureCount > 0 ||
                stats.SourceBoundaryTerminalAliasNormalizationFailureCount > 0 ||
                stats.VertexBoundaryBranchFailureCount > 0 ||
                stats.VertexBoundaryDuplicateFailureCount > 0)
            {
                blocker = "raw-provenance segmented provisional topology does not match the explicit source-boundary and vertex-patch boundary contract";
                return false;
            }

            ChamferVertexPatchPlan patchPlan =
                AuditChamferVertexPatchComponents(
                    normalizedVertexBoundaries,
                    sourceFaces,
                    context,
                    solution.WidthByEdge,
                    sourceBoundaryRecords,
                    useCounts,
                    expectedVertexBoundaryEdges,
                    expectedSourceBoundaryEdges,
                    finalSegments,
                    ref stats);
            if (patchPlan == null || !patchPlan.Ready)
            {
                blocker = patchPlan != null &&
                    !string.IsNullOrEmpty(patchPlan.Failure)
                    ? patchPlan.Failure
                    : "vertex-patch plan is not ready";
                return false;
            }
            bool patchReady = TryEmitAndAuditChamferVertexPatches(
                provisionalFaceRecords,
                patchPlan,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                TinyFaceAreaEpsilon,
                solution.SharedSpans,
                context,
                normalizedVertexBoundaries,
                sourceBoundaryRecords,
                ref stats,
                out ChamferBuildArtifacts buildArtifacts,
                out blocker);
            if (buildArtifacts != null)
            {
                stats.DiagnosticGeometrySignature =
                    BuildChamferDiagnosticGeometrySignature(
                        buildArtifacts.PrePatchFaceRecords);
                RunChamferDiagnosticHarness(
                    buildArtifacts,
                    ref stats);
            }
            if (!patchReady)
            {
                return false;
            }
            return true;
        }

        private static EdgeWearDebugEdgeRecord[]
            BuildSourceEdgeIndexDebugEdges(
                List<PolygonFace> faces,
                EdgeWearCoverageAudit coverageAudit = null)
        {
            if (faces == null || faces.Count == 0 ||
                !TryBuildEdgeWearTopologyGraph(
                    faces,
                    out EdgeWearTopologyGraph graph,
                    out _))
            {
                return Array.Empty<EdgeWearDebugEdgeRecord>();
            }

            return BuildEdgeWearDebugEdges(
                graph,
                coverageAudit,
                null);
        }

        private static EdgeWearDebugEdgeRecord[]
            BuildUnifiedEdgeWearDebugEdges(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit coverageAudit,
                List<int> focusEdgeIndices)
        {
            if (context == null || context.Graph == null)
            {
                return Array.Empty<EdgeWearDebugEdgeRecord>();
            }

            return BuildEdgeWearDebugEdges(
                context.Graph,
                coverageAudit,
                focusEdgeIndices);
        }

        private static EdgeWearDebugEdgeRecord[]
            BuildEdgeWearDebugEdges(
                EdgeWearTopologyGraph graph,
                EdgeWearCoverageAudit coverageAudit,
                List<int> focusEdgeIndices)
        {
            if (graph == null)
            {
                return Array.Empty<EdgeWearDebugEdgeRecord>();
            }

            HashSet<int> focusEdges = focusEdgeIndices == null
                ? new HashSet<int>()
                : new HashSet<int>(focusEdgeIndices);
            EdgeWearMicroTopologyNormalizationResult normalization =
                coverageAudit == null
                    ? null
                    : coverageAudit.MicroTopologyNormalization;
            int suppressedCount = normalization == null
                ? 0
                : normalization.SuppressedEdges.Count;
            EdgeWearDebugEdgeRecord[] records =
                new EdgeWearDebugEdgeRecord[
                    graph.Edges.Count + suppressedCount];
            for (int edgeIndex = 0;
                 edgeIndex < graph.Edges.Count;
                 edgeIndex++)
            {
                EdgeWearGraphEdge edge = graph.Edges[edgeIndex];
                Vector3 start =
                    graph.Vertices[edge.VertexA].Position;
                Vector3 end =
                    graph.Vertices[edge.VertexB].Position;
                EdgeWearEdgeLifecycleRecord lifecycle = null;
                if (coverageAudit != null)
                {
                    if (!coverageAudit.RecordByGraphEdge.TryGetValue(
                            edgeIndex,
                            out lifecycle))
                    {
                        coverageAudit.RecordByKey.TryGetValue(
                            new EdgeKey(start, end),
                            out lifecycle);
                    }
                }

                bool manifold = edge.FaceA >= 0 &&
                    edge.FaceB >= 0 &&
                    edge.ExtraFaceCount == 0;
                bool selected = lifecycle != null
                    ? lifecycle.Selected
                    : edge.Selected;
                EdgeWearDebugEdgeState state = lifecycle != null
                    ? ResolveEdgeWearDebugEdgeState(lifecycle)
                    : manifold
                        ? EdgeWearDebugEdgeState.Unassessed
                        : EdgeWearDebugEdgeState.StructuralExcluded;
                string reason = lifecycle != null
                    ? ResolveEdgeWearDebugEdgeReason(lifecycle)
                    : manifold
                        ? "unassessed"
                        : "non-manifold-or-boundary";
                int displayEdgeIndex = lifecycle != null &&
                        lifecycle.OriginalSourceEdgeIndex >= 0
                    ? lifecycle.OriginalSourceEdgeIndex
                    : normalization != null &&
                        lifecycle != null &&
                        lifecycle.MicroTopologyGeneratedTransition
                        ? normalization.OriginalEdgeCount + edgeIndex
                        : edgeIndex;
                records[edgeIndex] = new EdgeWearDebugEdgeRecord(
                    displayEdgeIndex,
                    start,
                    end,
                    selected,
                    focusEdges.Contains(edgeIndex),
                    state,
                    reason,
                    lifecycle != null
                        ? lifecycle.Length
                        : (end - start).magnitude,
                    lifecycle != null
                        ? lifecycle.DihedralDegrees
                        : 0f);
                records[edgeIndex].GraphEdgeIndex = edgeIndex;
                if (lifecycle != null)
                {
                    records[edgeIndex].Mandatory = lifecycle.Mandatory;
                    records[edgeIndex].CornerDamageCapRing =
                        lifecycle.CandidateClass ==
                            EdgeWearCandidateClass.CornerDamageCapRing;
                }
                if (lifecycle != null && lifecycle.Viability != null)
                {
                    EdgeWearEdgeViabilityRecord viability =
                        lifecycle.Viability;
                    records[edgeIndex].MacroBaseRequestedWidth =
                        viability.BaseRequestedWidth;
                    records[edgeIndex].MacroIdentity01 =
                        viability.MacroIdentity01;
                    records[edgeIndex].MacroSampledMultiplier =
                        viability.MacroSampledMultiplier;
                    records[edgeIndex].MacroEffectiveMultiplier =
                        viability.MacroEffectiveMultiplier;
                    records[edgeIndex].MacroRequestedWidth =
                        viability.RequestedWidth;
                    records[edgeIndex].MacroMinimumStyleClamped =
                        viability.MacroMinimumStyleClamped;
                }
            }

            for (int suppressedIndex = 0;
                 suppressedIndex < suppressedCount;
                 suppressedIndex++)
            {
                EdgeWearMicroTopologySuppressedEdge suppressed =
                    normalization.SuppressedEdges[suppressedIndex];
                int recordIndex = graph.Edges.Count + suppressedIndex;
                records[recordIndex] = new EdgeWearDebugEdgeRecord(
                    suppressed.OriginalSourceEdgeIndex,
                    suppressed.Start,
                    suppressed.End,
                    false,
                    false,
                    EdgeWearDebugEdgeState.MicroTopologySuppressed,
                    "micro-topology-suppressed",
                    suppressed.Length,
                    0f);
                records[recordIndex].GraphEdgeIndex = -1;
            }
            return records;
        }

        private sealed class ChamferPlaneRetentionTrialOutcome
        {
            public readonly SortedSet<int> ForcedDeferredEdges =
                new SortedSet<int>();
            public ChamferCornerSolution CornerSolution;
            public ChamferCornerStats CornerStats;
            public PlaneCutBevelAuditResult PlaneAudit;
            public TriangleSoup PreviewSoup;
            public EdgeWearCoverageAudit Coverage;
            public bool CornersReady;
            public bool FullyValid;
            public int CertifiedCount;
            public double CertifiedScore;
            public double CertifiedWidth;
            public int RecoveredProvisionalCount;
            public string Blocker = string.Empty;
        }

        private static bool TryAuditCertifiedBaselineAugmentation(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            MassRecipe recipe,
            float requestedWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            EdgeWearCoverageAudit coverageAudit,
            ICollection<int> immutableMaterialWidthRecoveryTargets,
            ref ChamferCornerStats stats,
            out ChamferCornerSolution winningSolution,
            out PlaneCutBevelAuditResult winningAudit,
            out TriangleSoup winningPreviewSoup,
            out string blocker)
        {
            const int MaximumSearchStates = 128;
            const int MaximumAdditionalDeferrals = 8;
            const double MaximumSearchMilliseconds = 5000.0;

            winningSolution = null;
            winningAudit = default;
            winningPreviewSoup = null;
            blocker = string.Empty;

            SortedSet<int> materialWidthRecoveryTargets =
                immutableMaterialWidthRecoveryTargets == null
                    ? new SortedSet<int>()
                    : new SortedSet<int>(
                        immutableMaterialWidthRecoveryTargets);
            SortedSet<int> baselineProvisionalEdges =
                CollectSelectedWidthRecoveryProvisionalEdges(
                    context,
                    coverageAudit);
            SortedSet<int> nonMaterialRecoveryEdges =
                CollectSelectedMultiSupportHullRecoveryEdges(
                    context,
                    coverageAudit);
            nonMaterialRecoveryEdges.ExceptWith(
                materialWidthRecoveryTargets);
            baselineProvisionalEdges.UnionWith(nonMaterialRecoveryEdges);

            ChamferPlaneRetentionTrialOutcome certifiedBaseline =
                EvaluateChamferPlaneRetentionTrial(
                    sourceFaces,
                    context,
                    recipe,
                    requestedWidth,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    coverageAudit,
                    baselineProvisionalEdges,
                    true,
                    false);
            if (!certifiedBaseline.FullyValid)
            {
                blocker = string.IsNullOrEmpty(certifiedBaseline.Blocker)
                    ? "the certified edge-wear baseline did not produce a fully certified shell"
                    : "certified-baseline-failed:" +
                        certifiedBaseline.Blocker;
                winningAudit = certifiedBaseline.PlaneAudit;
                winningAudit.CoverageAudit = certifiedBaseline.Coverage;
                winningAudit.BaselineCertified = 0;
                winningAudit.BaselineApplied = 0;
                winningAudit.AugmentationAttempted = 0;
                winningAudit.AugmentationApplied = 0;
                winningAudit.AugmentationFailure = blocker;
                return false;
            }

            bool recoveryRequested =
                materialWidthRecoveryTargets.Count > 0 ||
                nonMaterialRecoveryEdges.Count > 0;
            if (recoveryRequested &&
                !TryValidateChamferPlaneRetentionRenderChannels(
                    certifiedBaseline,
                    recipe))
            {
                blocker = string.IsNullOrEmpty(certifiedBaseline.Blocker)
                    ? "certified-baseline-render-validation-failed"
                    : "certified-baseline-failed:" +
                        certifiedBaseline.Blocker;
                winningAudit = certifiedBaseline.PlaneAudit;
                winningAudit.CoverageAudit = certifiedBaseline.Coverage;
                winningAudit.BaselineCertified = 0;
                winningAudit.BaselineApplied = 0;
                winningAudit.AugmentationAttempted = 0;
                winningAudit.AugmentationApplied = 0;
                winningAudit.AugmentationFailure = blocker;
                return false;
            }

            CaptureMaterialWidthRecoveryBaselineEvidence(
                coverageAudit,
                certifiedBaseline.Coverage,
                materialWidthRecoveryTargets);
            stats = certifiedBaseline.CornerStats;
            ApplyCertifiedBaselineAugmentationMetadata(
                ref certifiedBaseline.PlaneAudit,
                certifiedBaseline,
                false,
                false,
                0,
                0,
                0.0,
                false,
                false,
                string.Empty,
                string.Empty,
                string.Empty);

            MaterialWidthRecoveryTrialSummary materialSummary =
                EvaluateMaterialWidthRecoveryTargets(
                    sourceFaces,
                    context,
                    recipe,
                    requestedWidth,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    coverageAudit,
                    certifiedBaseline,
                    materialWidthRecoveryTargets);
            ChamferPlaneRetentionTrialOutcome recoveryBaseline =
                materialSummary.Winner ?? certifiedBaseline;
            bool materialPhaseTerminal = materialSummary.Cancelled ||
                materialSummary.TimeBudgetExceeded;

            Dictionary<int, SortedSet<int>> cornerRecoveryParticipants =
                new Dictionary<int, SortedSet<int>>();
            if (!materialPhaseTerminal)
            {
                CollectCornerInactiveRecoveryEdges(
                    context,
                    coverageAudit,
                    recoveryBaseline.Coverage,
                    recoveryBaseline.CornerSolution,
                    nonMaterialRecoveryEdges,
                    cornerRecoveryParticipants);
            }
            else
            {
                nonMaterialRecoveryEdges.Clear();
            }
            if (!recoveryRequested &&
                nonMaterialRecoveryEdges.Count > 0 &&
                !TryValidateChamferPlaneRetentionRenderChannels(
                    recoveryBaseline,
                    recipe))
            {
                blocker = string.IsNullOrEmpty(recoveryBaseline.Blocker)
                    ? "certified-baseline-render-validation-failed"
                    : "certified-baseline-failed:" +
                        recoveryBaseline.Blocker;
                winningAudit = recoveryBaseline.PlaneAudit;
                winningAudit.CoverageAudit = recoveryBaseline.Coverage;
                winningAudit.BaselineCertified = 0;
                winningAudit.BaselineApplied = 0;
                winningAudit.AugmentationAttempted = 0;
                winningAudit.AugmentationApplied = 0;
                winningAudit.AugmentationFailure = blocker;
                return false;
            }

            bool materialAttempted = materialSummary.AttemptedCount > 0;
            bool materialApplied = materialSummary.Winner != null;
            if (nonMaterialRecoveryEdges.Count == 0)
            {
                FinalizeMaterialWidthRecoveryResolutions(
                    recoveryBaseline.Coverage,
                    materialWidthRecoveryTargets);
                string materialFailure = ResolveMaterialWidthRecoveryFailure(
                    materialSummary,
                    materialWidthRecoveryTargets);
                ApplyCertifiedBaselineAugmentationMetadata(
                    ref recoveryBaseline.PlaneAudit,
                    certifiedBaseline,
                    materialAttempted,
                    materialApplied,
                    materialSummary.AttemptedCount,
                    0,
                    materialSummary.ElapsedMilliseconds,
                    materialSummary.TimeBudgetExceeded,
                    materialSummary.Cancelled,
                    materialFailure,
                    materialSummary.LastFailure,
                    materialSummary.ImplicatedEdgeEvidence);
                if (!string.IsNullOrEmpty(materialFailure))
                {
                    recoveryBaseline.PlaneAudit.Diagnostic =
                        AppendEdgeWearDiagnostic(
                            recoveryBaseline.PlaneAudit.Diagnostic,
                            materialFailure +
                            (materialApplied
                                ? "; certified material recovery retained"
                                : "; certified baseline retained"));
                }
                stats = recoveryBaseline.CornerStats;
                winningSolution = recoveryBaseline.CornerSolution;
                winningAudit = recoveryBaseline.PlaneAudit;
                winningAudit.CoverageAudit = recoveryBaseline.Coverage;
                winningPreviewSoup = recoveryBaseline.PreviewSoup;
                return true;
            }

            SortedSet<int> certifiedBaselineExclusions =
                CollectBaselineSelectedExclusions(
                    context,
                    recoveryBaseline.Coverage);
            SortedSet<int> allRecoveryEnabled =
                new SortedSet<int>(certifiedBaselineExclusions);
            foreach (int recoveryEdge in nonMaterialRecoveryEdges)
            {
                allRecoveryEnabled.Remove(recoveryEdge);
            }

            List<SortedSet<int>> frontier = new List<SortedSet<int>>
            {
                allRecoveryEnabled
            };
            foreach (int recoveryEdge in nonMaterialRecoveryEdges)
            {
                SortedSet<int> targetState =
                    new SortedSet<int>(certifiedBaselineExclusions);
                targetState.Remove(recoveryEdge);
                frontier.Add(targetState);
                if (cornerRecoveryParticipants.TryGetValue(
                        recoveryEdge,
                        out SortedSet<int> participants))
                {
                    AddCornerRecoveryProtectedStates(
                        frontier,
                        targetState,
                        participants,
                        new[] { recoveryEdge });
                }
            }

            SortedSet<int> protectedMaterialEdges =
                CollectCertifiedMaterialWidthRecoveryEdges(
                    recoveryBaseline.Coverage,
                    materialWidthRecoveryTargets);
            HashSet<string> visited = new HashSet<string>();
            ChamferPlaneRetentionTrialOutcome winner = null;
            ChamferPlaneRetentionTrialOutcome lastOutcome = null;
            int evaluatedStates = 0;
            bool timeBudgetExceeded = false;
            bool cancelled = false;
            string lastImplicatedEvidence = string.Empty;
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            while (frontier.Count > 0 &&
                evaluatedStates < MaximumSearchStates)
            {
                if (IsEdgeWearAuditCancellationRequested())
                {
                    cancelled = true;
                    break;
                }
                if (stopwatch.Elapsed.TotalMilliseconds >=
                    MaximumSearchMilliseconds)
                {
                    timeBudgetExceeded = true;
                    break;
                }

                frontier.Sort((left, right) =>
                    CompareChamferForcedDeferralSetsByPriority(
                        coverageAudit,
                        left,
                        right));
                SortedSet<int> forced = frontier[0];
                frontier.RemoveAt(0);
                string key = FormatChamferForcedDeferralKey(forced);
                if (!visited.Add(key))
                {
                    continue;
                }

                evaluatedStates++;
                ChamferPlaneRetentionTrialOutcome outcome =
                    EvaluateChamferPlaneRetentionTrial(
                        sourceFaces,
                        context,
                        recipe,
                        requestedWidth,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        coverageAudit,
                        forced,
                        false,
                        true);
                CopyMaterialWidthRecoveryExecutionEvidence(
                    coverageAudit,
                    outcome.Coverage,
                    materialWidthRecoveryTargets);
                lastOutcome = outcome;
                outcome.RecoveredProvisionalCount =
                    CountRecoveredProvisionalEdges(
                        nonMaterialRecoveryEdges,
                        recoveryBaseline.Coverage,
                        outcome.Coverage);
                if (outcome.FullyValid &&
                    outcome.RecoveredProvisionalCount > 0 &&
                    IsChamferPlaneRetentionTrialAcceptableForRecovery(
                        outcome,
                        recoveryBaseline,
                        nonMaterialRecoveryEdges,
                        cornerRecoveryParticipants))
                {
                    if (winner == null ||
                        IsChamferPlaneRecoveryTrialBetter(
                            outcome,
                            winner))
                    {
                        winner = outcome;
                    }
                }

                if (forced.Count >=
                    certifiedBaselineExclusions.Count +
                    MaximumAdditionalDeferrals)
                {
                    continue;
                }

                List<int> branchEdges =
                    CollectChamferPlaneRetentionBranchEdges(
                        context,
                        coverageAudit,
                        outcome);
                lastImplicatedEvidence =
                    FormatChamferForcedDeferralKey(branchEdges);
                for (int edgeIndex = 0;
                     edgeIndex < branchEdges.Count;
                     edgeIndex++)
                {
                    int edgeToDefer = branchEdges[edgeIndex];
                    if (forced.Contains(edgeToDefer) ||
                        nonMaterialRecoveryEdges.Contains(edgeToDefer) ||
                        protectedMaterialEdges.Contains(edgeToDefer))
                    {
                        continue;
                    }
                    SortedSet<int> child =
                        new SortedSet<int>(forced)
                        {
                            edgeToDefer
                        };
                    if (!visited.Contains(
                            FormatChamferForcedDeferralKey(child)))
                    {
                        frontier.Add(child);
                    }
                }
            }

            stopwatch.Stop();
            int totalStates = materialSummary.AttemptedCount +
                evaluatedStates;
            double totalElapsed = materialSummary.ElapsedMilliseconds +
                stopwatch.Elapsed.TotalMilliseconds;
            bool anyAttempted = materialAttempted || evaluatedStates > 0;
            if (winner != null)
            {
                CopyMaterialWidthRecoveryExecutionEvidence(
                    coverageAudit,
                    winner.Coverage,
                    materialWidthRecoveryTargets);
                FinalizeMaterialWidthRecoveryResolutions(
                    winner.Coverage,
                    materialWidthRecoveryTargets);
                ApplyCornerRecoveryResolution(
                    winner.Coverage,
                    cornerRecoveryParticipants.Keys,
                    "certified-recovery",
                    string.Empty);
                stats = winner.CornerStats;
                ApplyCertifiedBaselineAugmentationMetadata(
                    ref winner.PlaneAudit,
                    certifiedBaseline,
                    anyAttempted,
                    true,
                    totalStates,
                    frontier.Count,
                    totalElapsed,
                    false,
                    false,
                    string.Empty,
                    string.Empty,
                    lastImplicatedEvidence);
                winningSolution = winner.CornerSolution;
                winningAudit = winner.PlaneAudit;
                winningAudit.CoverageAudit = winner.Coverage;
                winningPreviewSoup = winner.PreviewSoup;
                return true;
            }

            string augmentationFailure;
            if (cancelled)
            {
                augmentationFailure = "augmentation-cancelled";
            }
            else if (timeBudgetExceeded)
            {
                augmentationFailure =
                    "augmentation-time-budget-exceeded";
            }
            else if (frontier.Count > 0)
            {
                augmentationFailure =
                    "augmentation-state-budget-exceeded";
            }
            else
            {
                augmentationFailure =
                    "augmentation-found-no-superior-certified-shell";
            }
            string lastFailure = lastOutcome == null
                ? string.Empty
                : lastOutcome.Blocker;
            bool recoverySearchExhausted = !cancelled &&
                !timeBudgetExceeded &&
                frontier.Count == 0;
            ApplyCornerRecoveryResolution(
                recoveryBaseline.Coverage,
                cornerRecoveryParticipants.Keys,
                recoverySearchExhausted
                    ? "proven-infeasible"
                    : "unresolved",
                string.IsNullOrEmpty(lastFailure)
                    ? augmentationFailure
                    : augmentationFailure + ":" + lastFailure);
            FinalizeMaterialWidthRecoveryResolutions(
                recoveryBaseline.Coverage,
                materialWidthRecoveryTargets);
            string materialPhaseFailure = ResolveMaterialWidthRecoveryFailure(
                materialSummary,
                materialWidthRecoveryTargets);
            string combinedFailure = string.IsNullOrEmpty(materialPhaseFailure)
                ? augmentationFailure
                : materialPhaseFailure + ";" + augmentationFailure;
            string combinedLastFailure = string.IsNullOrEmpty(lastFailure)
                ? materialSummary.LastFailure
                : lastFailure;
            ApplyCertifiedBaselineAugmentationMetadata(
                ref recoveryBaseline.PlaneAudit,
                certifiedBaseline,
                anyAttempted,
                materialApplied,
                totalStates,
                frontier.Count,
                totalElapsed,
                timeBudgetExceeded ||
                    materialSummary.TimeBudgetExceeded,
                cancelled || materialSummary.Cancelled,
                combinedFailure,
                combinedLastFailure,
                string.IsNullOrEmpty(lastImplicatedEvidence)
                    ? materialSummary.ImplicatedEdgeEvidence
                    : lastImplicatedEvidence);
            recoveryBaseline.PlaneAudit.Diagnostic =
                AppendEdgeWearDiagnostic(
                    recoveryBaseline.PlaneAudit.Diagnostic,
                    combinedFailure +
                    (materialApplied
                        ? "; certified material recovery retained"
                        : "; certified baseline retained"));
            stats = recoveryBaseline.CornerStats;
            winningSolution = recoveryBaseline.CornerSolution;
            winningAudit = recoveryBaseline.PlaneAudit;
            winningAudit.CoverageAudit = recoveryBaseline.Coverage;
            winningPreviewSoup = recoveryBaseline.PreviewSoup;
            blocker = string.Empty;
            return true;
        }

        private sealed class MaterialWidthRecoveryTrialSummary
        {
            public ChamferPlaneRetentionTrialOutcome Winner;
            public int AttemptedCount;
            public int CompletedCount;
            public double ElapsedMilliseconds;
            public bool Cancelled;
            public bool TimeBudgetExceeded;
            public string LastFailure = string.Empty;
            public string ImplicatedEdgeEvidence = string.Empty;
        }

        private static MaterialWidthRecoveryTrialSummary
            EvaluateMaterialWidthRecoveryTargets(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                MassRecipe recipe,
                float requestedWidth,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                EdgeWearCoverageAudit sourceCoverage,
                ChamferPlaneRetentionTrialOutcome certifiedBaseline,
                ICollection<int> materialWidthRecoveryTargets)
        {
            MaterialWidthRecoveryTrialSummary summary =
                new MaterialWidthRecoveryTrialSummary();
            if (sourceCoverage == null || certifiedBaseline == null ||
                materialWidthRecoveryTargets == null ||
                materialWidthRecoveryTargets.Count == 0)
            {
                return summary;
            }

            ChamferPlaneRetentionTrialOutcome workingBaseline =
                certifiedBaseline;
            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            foreach (int targetEdge in materialWidthRecoveryTargets)
            {
                if (IsEdgeWearAuditCancellationRequested())
                {
                    summary.Cancelled = true;
                    break;
                }

                summary.AttemptedCount++;
                summary.ImplicatedEdgeEvidence =
                    FormatChamferForcedDeferralKey(
                        materialWidthRecoveryTargets);
                SetMaterialWidthRecoveryTrialEvidence(
                    sourceCoverage,
                    targetEdge,
                    true,
                    false,
                    false,
                    false,
                    string.Empty);
                SetMaterialWidthRecoveryTrialEvidence(
                    certifiedBaseline.Coverage,
                    targetEdge,
                    true,
                    false,
                    false,
                    false,
                    string.Empty);

                SortedSet<int> forced =
                    CollectBaselineSelectedExclusions(
                        context,
                        workingBaseline.Coverage);
                forced.Remove(targetEdge);
                ChamferPlaneRetentionTrialOutcome outcome =
                    EvaluateChamferPlaneRetentionTrial(
                        sourceFaces,
                        context,
                        recipe,
                        requestedWidth,
                        minimumStableEdgeLength,
                        minimumStableFaceArea,
                        sourceCoverage,
                        forced,
                        false,
                        true);
                bool trialCompleted =
                    IsMaterialWidthRecoveryTrialCompleted(outcome);
                if (trialCompleted)
                {
                    summary.CompletedCount++;
                }
                bool trialSucceeded = trialCompleted &&
                    IsMaterialWidthRecoveryTrialAcceptable(
                        outcome,
                        workingBaseline,
                        targetEdge);
                string failure = trialSucceeded
                    ? string.Empty
                    : ResolveMaterialWidthRecoveryTrialFailure(
                        outcome,
                        targetEdge,
                        trialCompleted);
                SetMaterialWidthRecoveryTrialEvidence(
                    sourceCoverage,
                    targetEdge,
                    true,
                    trialCompleted,
                    trialSucceeded,
                    false,
                    failure);
                SetMaterialWidthRecoveryTrialEvidence(
                    certifiedBaseline.Coverage,
                    targetEdge,
                    true,
                    trialCompleted,
                    trialSucceeded,
                    false,
                    failure);
                SetMaterialWidthRecoveryTrialEvidence(
                    outcome.Coverage,
                    targetEdge,
                    true,
                    trialCompleted,
                    trialSucceeded,
                    false,
                    failure);
                if (!trialSucceeded)
                {
                    summary.LastFailure = failure;
                    bool trialCancelled =
                        IsMaterialWidthRecoveryTrialCancelled(outcome);
                    bool trialTimeBudgetExceeded =
                        IsMaterialWidthRecoveryTrialTimeBudgetExceeded(
                            outcome);
                    summary.Cancelled |= trialCancelled;
                    summary.TimeBudgetExceeded |=
                        trialTimeBudgetExceeded;
                    if (trialCancelled || trialTimeBudgetExceeded)
                    {
                        break;
                    }
                    continue;
                }

                workingBaseline = outcome;
                summary.Winner = outcome;
                SetMaterialWidthRecoveryTrialEvidence(
                    sourceCoverage,
                    targetEdge,
                    true,
                    true,
                    true,
                    true,
                    string.Empty);
                SetMaterialWidthRecoveryTrialEvidence(
                    workingBaseline.Coverage,
                    targetEdge,
                    true,
                    true,
                    true,
                    true,
                    string.Empty);
            }
            stopwatch.Stop();
            summary.ElapsedMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;

            EdgeWearCoverageAudit finalCoverage = summary.Winner == null
                ? certifiedBaseline.Coverage
                : summary.Winner.Coverage;
            CopyMaterialWidthRecoveryExecutionEvidence(
                sourceCoverage,
                finalCoverage,
                materialWidthRecoveryTargets);
            return summary;
        }

        private static bool IsMaterialWidthRecoveryTrialAcceptable(
            ChamferPlaneRetentionTrialOutcome candidate,
            ChamferPlaneRetentionTrialOutcome baseline,
            int targetEdge)
        {
            if (candidate == null || !candidate.FullyValid ||
                candidate.Coverage == null || baseline == null ||
                !baseline.FullyValid || baseline.Coverage == null ||
                !candidate.Coverage.RecordByGraphEdge.TryGetValue(
                    targetEdge,
                    out EdgeWearEdgeLifecycleRecord targetRecord) ||
                targetRecord == null || !targetRecord.Built)
            {
                return false;
            }

            foreach (KeyValuePair<int, EdgeWearEdgeLifecycleRecord> pair
                in baseline.Coverage.RecordByGraphEdge)
            {
                EdgeWearEdgeLifecycleRecord baselineRecord = pair.Value;
                if (baselineRecord == null || !baselineRecord.Built)
                {
                    continue;
                }
                if (!candidate.Coverage.RecordByGraphEdge.TryGetValue(
                        pair.Key,
                        out EdgeWearEdgeLifecycleRecord candidateRecord) ||
                    candidateRecord == null || !candidateRecord.Built)
                {
                    return false;
                }
            }
            return candidate.CertifiedCount > baseline.CertifiedCount;
        }

        private static void CaptureMaterialWidthRecoveryBaselineEvidence(
            EdgeWearCoverageAudit sourceCoverage,
            EdgeWearCoverageAudit baselineCoverage,
            ICollection<int> targets)
        {
            if (sourceCoverage == null || baselineCoverage == null ||
                targets == null)
            {
                return;
            }

            foreach (int targetEdge in targets)
            {
                bool baselineDeferred =
                    baselineCoverage.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord baselineRecord) &&
                    baselineRecord != null &&
                    baselineRecord.RecoveryBaselineDeferred;
                if (sourceCoverage.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord sourceRecord) &&
                    sourceRecord != null)
                {
                    sourceRecord.MaterialWidthRecoveryTarget = true;
                    sourceRecord.MaterialWidthRecoveryBaselineDeferred =
                        baselineDeferred;
                }
                if (baselineRecord != null)
                {
                    baselineRecord.MaterialWidthRecoveryTarget = true;
                    baselineRecord.MaterialWidthRecoveryBaselineDeferred =
                        baselineDeferred;
                }
            }
        }

        private static bool IsMaterialWidthRecoveryTrialCancelled(
            ChamferPlaneRetentionTrialOutcome outcome)
        {
            return outcome != null &&
                (outcome.CornerStats.ConflictSearchCancelled != 0 ||
                 outcome.PlaneAudit.CoexistenceSearchCancelled != 0);
        }

        private static bool
            IsMaterialWidthRecoveryTrialTimeBudgetExceeded(
                ChamferPlaneRetentionTrialOutcome outcome)
        {
            return outcome != null &&
                (outcome.CornerStats.ConflictSearchTimeBudgetExceeded != 0 ||
                 outcome.PlaneAudit.SolveTimedOut != 0 ||
                 outcome.PlaneAudit
                     .CoexistenceSearchTimeBudgetExceeded != 0);
        }

        private static bool IsMaterialWidthRecoveryTrialCompleted(
            ChamferPlaneRetentionTrialOutcome outcome)
        {
            if (outcome == null)
            {
                return false;
            }

            return outcome.CornerStats.ConflictSearchCancelled == 0 &&
                outcome.CornerStats.ConflictSearchTimeBudgetExceeded == 0 &&
                outcome.PlaneAudit.SolveTimedOut == 0 &&
                outcome.PlaneAudit.CoexistenceSearchCancelled == 0 &&
                outcome.PlaneAudit.CoexistenceSearchTimeBudgetExceeded == 0;
        }

        private static string ResolveIncompleteMaterialWidthRecoveryTrialReason(
            ChamferPlaneRetentionTrialOutcome outcome)
        {
            if (outcome == null)
            {
                return "material-width-target-trial-produced-no-outcome";
            }
            if (outcome.CornerStats.ConflictSearchCancelled != 0 ||
                outcome.PlaneAudit.CoexistenceSearchCancelled != 0)
            {
                return "material-width-target-trial-cancelled";
            }
            if (outcome.CornerStats.ConflictSearchTimeBudgetExceeded != 0)
            {
                return "material-width-target-corner-time-budget-exceeded";
            }
            if (outcome.PlaneAudit.CoexistenceSearchTimeBudgetExceeded != 0)
            {
                return "material-width-target-coexistence-time-budget-exceeded";
            }
            if (outcome.PlaneAudit.SolveTimedOut != 0)
            {
                return "material-width-target-plane-solve-timed-out";
            }
            return "material-width-target-trial-incomplete";
        }

        private static string ResolveMaterialWidthRecoveryTrialFailure(
            ChamferPlaneRetentionTrialOutcome outcome,
            int targetEdge,
            bool trialCompleted)
        {
            if (!trialCompleted)
            {
                return ResolveIncompleteMaterialWidthRecoveryTrialReason(
                    outcome);
            }
            if (outcome == null)
            {
                return "material-width-target-trial-produced-no-outcome";
            }
            if (!outcome.FullyValid)
            {
                return string.IsNullOrEmpty(outcome.Blocker)
                    ? "material-width-target-trial-failed-full-shell-certification"
                    : outcome.Blocker;
            }
            bool targetBuilt = outcome.Coverage != null &&
                outcome.Coverage.RecordByGraphEdge.TryGetValue(
                    targetEdge,
                    out EdgeWearEdgeLifecycleRecord targetRecord) &&
                targetRecord != null && targetRecord.Built;
            if (!targetBuilt)
            {
                return "material-width-target-trial-did-not-certify-target";
            }
            return "material-width-target-trial-would-remove-certified-baseline-edge";
        }

        private static void SetMaterialWidthRecoveryTrialEvidence(
            EdgeWearCoverageAudit coverage,
            int targetEdge,
            bool attempted,
            bool trialCompleted,
            bool trialSucceeded,
            bool certified,
            string failure)
        {
            if (coverage == null ||
                !coverage.RecordByGraphEdge.TryGetValue(
                    targetEdge,
                    out EdgeWearEdgeLifecycleRecord record) ||
                record == null)
            {
                return;
            }

            record.MaterialWidthRecoveryTarget = true;
            record.MaterialWidthRecoveryAttempted |= attempted;
            record.MaterialWidthRecoveryTrialCompleted |= trialCompleted;
            record.MaterialWidthRecoveryTrialSucceeded |= trialSucceeded;
            record.MaterialWidthRecoveryCertified |= certified;
            if (!string.IsNullOrEmpty(failure))
            {
                record.MaterialWidthRecoveryFailure = failure;
                record.WidthRecoveryEvidence = failure;
            }
        }

        private static void CopyMaterialWidthRecoveryExecutionEvidence(
            EdgeWearCoverageAudit source,
            EdgeWearCoverageAudit destination,
            ICollection<int> targets)
        {
            if (source == null || destination == null || targets == null)
            {
                return;
            }

            foreach (int targetEdge in targets)
            {
                if (!source.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord sourceRecord) ||
                    sourceRecord == null ||
                    !destination.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord destinationRecord) ||
                    destinationRecord == null)
                {
                    continue;
                }

                destinationRecord.MaterialWidthRecoveryTarget =
                    sourceRecord.MaterialWidthRecoveryTarget;
                destinationRecord.MaterialWidthRecoveryBaselineDeferred =
                    sourceRecord.MaterialWidthRecoveryBaselineDeferred;
                destinationRecord.MaterialWidthRecoveryAttempted =
                    sourceRecord.MaterialWidthRecoveryAttempted;
                destinationRecord.MaterialWidthRecoveryTrialCompleted =
                    sourceRecord.MaterialWidthRecoveryTrialCompleted;
                destinationRecord.MaterialWidthRecoveryTrialSucceeded =
                    sourceRecord.MaterialWidthRecoveryTrialSucceeded;
                destinationRecord.MaterialWidthRecoveryCertified =
                    sourceRecord.MaterialWidthRecoveryCertified;
                destinationRecord.MaterialWidthRecoveryFailure =
                    sourceRecord.MaterialWidthRecoveryFailure;
                destinationRecord.WidthRecoveryEvidence =
                    sourceRecord.WidthRecoveryEvidence;
            }
        }

        private static void FinalizeMaterialWidthRecoveryResolutions(
            EdgeWearCoverageAudit coverage,
            ICollection<int> targets)
        {
            if (coverage == null || targets == null)
            {
                return;
            }

            foreach (int targetEdge in targets)
            {
                if (!coverage.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord record) ||
                    record == null)
                {
                    continue;
                }

                record.MaterialWidthRecoveryCertified = record.Built;
                if (record.Built)
                {
                    record.MaterialWidthRecoveryFailure = string.Empty;
                    record.WidthRecoveryEvidence = string.Empty;
                    record.WidthRecoveryResolution = "certified-recovery";
                    continue;
                }
                if (!record.MaterialWidthRecoveryAttempted)
                {
                    record.WidthRecoveryResolution =
                        "unresolved:material-width-target-not-attempted";
                    continue;
                }
                if (!record.MaterialWidthRecoveryTrialCompleted)
                {
                    record.WidthRecoveryResolution =
                        "unresolved:material-width-target-trial-incomplete";
                    if (string.IsNullOrEmpty(
                            record.MaterialWidthRecoveryFailure))
                    {
                        record.MaterialWidthRecoveryFailure =
                            "material-width-target-trial-incomplete";
                    }
                    record.WidthRecoveryEvidence =
                        record.MaterialWidthRecoveryFailure;
                    continue;
                }
                if (record.MaterialWidthRecoveryTrialSucceeded)
                {
                    record.WidthRecoveryResolution =
                        "unresolved:certified-target-trial-not-committed";
                    if (string.IsNullOrEmpty(
                            record.MaterialWidthRecoveryFailure))
                    {
                        record.MaterialWidthRecoveryFailure =
                            "certified-target-trial-not-committed";
                    }
                    record.WidthRecoveryEvidence =
                        record.MaterialWidthRecoveryFailure;
                    continue;
                }

                record.FinalReason = "width-recovery-proven-infeasible";
                record.CoexistenceFailureReason = record.FinalReason;
                record.WidthRecoveryResolution =
                    string.IsNullOrEmpty(record.MaterialWidthRecoveryFailure)
                        ? "proven-infeasible"
                        : "proven-infeasible:" +
                            record.MaterialWidthRecoveryFailure;
                record.WidthRecoveryEvidence =
                    record.MaterialWidthRecoveryFailure;
            }
        }

        private static SortedSet<int>
            CollectCertifiedMaterialWidthRecoveryEdges(
                EdgeWearCoverageAudit coverage,
                ICollection<int> targets)
        {
            SortedSet<int> result = new SortedSet<int>();
            if (coverage == null || targets == null)
            {
                return result;
            }
            foreach (int targetEdge in targets)
            {
                if (coverage.RecordByGraphEdge.TryGetValue(
                        targetEdge,
                        out EdgeWearEdgeLifecycleRecord record) &&
                    record != null && record.Built)
                {
                    result.Add(targetEdge);
                }
            }
            return result;
        }

        private static string ResolveMaterialWidthRecoveryFailure(
            MaterialWidthRecoveryTrialSummary summary,
            ICollection<int> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return string.Empty;
            }
            if (summary == null)
            {
                return "material-width-target-trials-unavailable";
            }
            if (summary.Cancelled)
            {
                return "material-width-target-trials-cancelled";
            }
            if (summary.TimeBudgetExceeded)
            {
                return
                    "material-width-target-trials-time-budget-exceeded";
            }
            if (summary.AttemptedCount < targets.Count ||
                summary.CompletedCount < targets.Count)
            {
                return "material-width-target-trials-incomplete";
            }
            if (summary.Winner == null)
            {
                return "material-width-target-trials-failed";
            }
            return string.Empty;
        }


        private static ChamferPlaneRetentionTrialOutcome
            EvaluateChamferPlaneRetentionTrial(
                List<PolygonFace> sourceFaces,
                ChamferTopologyContext context,
                MassRecipe recipe,
                float requestedWidth,
                float minimumStableEdgeLength,
                float minimumStableFaceArea,
                EdgeWearCoverageAudit sourceCoverage,
                ICollection<int> forcedDeferredEdges,
                bool allowKernelConflictSearch,
                bool validateRenderChannels)
        {
            ChamferPlaneRetentionTrialOutcome outcome =
                new ChamferPlaneRetentionTrialOutcome();
            if (forcedDeferredEdges != null)
            {
                outcome.ForcedDeferredEdges.UnionWith(
                    forcedDeferredEdges);
            }
            outcome.Coverage = sourceCoverage.CloneForTrial();
            HashSet<int> forced = forcedDeferredEdges == null
                ? null
                : new HashSet<int>(forcedDeferredEdges);
            outcome.CornersReady = AuditExplicitChamferCornerSolution(
                sourceFaces,
                context,
                requestedWidth,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                outcome.Coverage,
                forced,
                ref outcome.CornerStats,
                out outcome.CornerSolution,
                out outcome.Blocker);
            if (!outcome.CornersReady)
            {
                return outcome;
            }

            ApplyEdgeWearCoverageCornerSolution(
                outcome.Coverage,
                context,
                outcome.CornerSolution);
            try
            {
                outcome.PlaneAudit = AuditPlaneCutBevelKernel(
                    sourceFaces,
                    context,
                    outcome.CornerSolution,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    outcome.Coverage,
                    allowKernelConflictSearch,
                    out outcome.PreviewSoup);
            }
            catch (InvalidOperationException exception)
            {
                outcome.Blocker =
                    "render-channel-validation:" + exception.Message;
                return outcome;
            }

            outcome.FullyValid =
                outcome.PlaneAudit.GeometryValid == 1 &&
                outcome.PreviewSoup != null;
            if (validateRenderChannels && outcome.FullyValid)
            {
                TryValidateChamferPlaneRetentionRenderChannels(
                    outcome,
                    recipe);
            }
            if (outcome.FullyValid)
            {
                outcome.Blocker = string.Empty;
            }
            else if (string.IsNullOrEmpty(outcome.Blocker))
            {
                outcome.Blocker = outcome.PlaneAudit.Diagnostic;
            }
            CalculateChamferPlaneRetentionMetrics(outcome);
            return outcome;
        }

        private static bool TryValidateChamferPlaneRetentionRenderChannels(
            ChamferPlaneRetentionTrialOutcome outcome,
            MassRecipe recipe)
        {
            if (outcome == null || !outcome.FullyValid ||
                outcome.PreviewSoup == null)
            {
                return outcome != null && outcome.FullyValid;
            }
            try
            {
                BuildMeshData(outcome.PreviewSoup, recipe);
                return true;
            }
            catch (InvalidOperationException exception)
            {
                outcome.Blocker =
                    "render-channel-validation:" + exception.Message;
                outcome.PreviewSoup = null;
                outcome.FullyValid = false;
                return false;
            }
        }

        private static SortedSet<int>
            CollectSelectedWidthRecoveryProvisionalEdges(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit coverageAudit)
        {
            SortedSet<int> result = new SortedSet<int>();
            if (context == null || coverageAudit == null)
            {
                return result;
            }
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                int graphEdgeIndex =
                    context.SelectedEdges[selectedIndex].GraphEdgeIndex;
                if (coverageAudit.ViabilityByGraphEdge.TryGetValue(
                        graphEdgeIndex,
                        out EdgeWearEdgeViabilityRecord viability) &&
                    viability != null &&
                    viability.WidthRecoveryProvisional)
                {
                    result.Add(graphEdgeIndex);
                }
            }
            return result;
        }

        private static SortedSet<int>
            CollectSelectedMultiSupportHullRecoveryEdges(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit coverageAudit)
        {
            SortedSet<int> result = new SortedSet<int>();
            if (context == null || coverageAudit == null)
            {
                return result;
            }
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                int graphEdgeIndex =
                    context.SelectedEdges[selectedIndex].GraphEdgeIndex;
                if (coverageAudit.ViabilityByGraphEdge.TryGetValue(
                        graphEdgeIndex,
                        out EdgeWearEdgeViabilityRecord viability) &&
                    viability != null &&
                    viability.MultiSupportHullRecovery)
                {
                    result.Add(graphEdgeIndex);
                }
            }
            return result;
        }

        private static void CollectCornerInactiveRecoveryEdges(
            ChamferTopologyContext context,
            EdgeWearCoverageAudit sourceCoverage,
            EdgeWearCoverageAudit baselineCoverage,
            ChamferCornerSolution baselineSolution,
            SortedSet<int> recoveryEdges,
            Dictionary<int, SortedSet<int>> recoveryParticipants)
        {
            if (context == null || baselineCoverage == null ||
                baselineSolution == null || recoveryEdges == null ||
                recoveryParticipants == null)
            {
                return;
            }
            for (int conflictIndex = 0;
                 conflictIndex < baselineSolution.Conflicts.Count;
                 conflictIndex++)
            {
                ChamferCornerConflictRecord conflict =
                    baselineSolution.Conflicts[conflictIndex];
                for (int zeroedIndex = 0;
                     zeroedIndex < conflict.ZeroedSelectedEdges.Count;
                     zeroedIndex++)
                {
                    int edgeIndex =
                        conflict.ZeroedSelectedEdges[zeroedIndex];
                    if (!baselineSolution.WidthByEdge.TryGetValue(
                            edgeIndex,
                            out float width) ||
                        width > PointMergeDistance ||
                        !baselineCoverage.RecordByGraphEdge.TryGetValue(
                            edgeIndex,
                            out EdgeWearEdgeLifecycleRecord record) ||
                        record == null ||
                        !record.GeometricEligible ||
                        record.Viability == null ||
                        !record.Viability.FeasibleWidthFractionValid ||
                        record.Viability.WidthRecoveryProvisional ||
                        !record.CornerRecoveryProvisional ||
                        !string.Equals(
                            record.FinalReason,
                            "corner-width-inactive",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    recoveryEdges.Add(edgeIndex);
                    if (!recoveryParticipants.TryGetValue(
                            edgeIndex,
                            out SortedSet<int> participants))
                    {
                        participants = new SortedSet<int>();
                        recoveryParticipants.Add(edgeIndex, participants);
                    }
                    participants.UnionWith(
                        conflict.ParticipatingSelectedEdges);
                    participants.Remove(edgeIndex);

                    float lastPositiveWidth =
                        record.CornerRecoveryLastPositiveWidth;
                    if (lastPositiveWidth <= PointMergeDistance)
                    {
                        conflict.ParticipantWidthBeforeScale.TryGetValue(
                            edgeIndex,
                            out lastPositiveWidth);
                    }
                    if (lastPositiveWidth <= PointMergeDistance &&
                        record.Viability != null)
                    {
                        lastPositiveWidth = Mathf.Max(
                            record.Viability.IsolatedMaximumCertifiedWidth,
                            record.Viability.MaximumLocallyFeasibleWidth);
                    }
                    ApplyCornerRecoveryProvisionalEvidence(
                        record,
                        conflict,
                        lastPositiveWidth);
                    if (sourceCoverage != null &&
                        sourceCoverage.RecordByGraphEdge.TryGetValue(
                            edgeIndex,
                            out EdgeWearEdgeLifecycleRecord sourceRecord) &&
                        sourceRecord != null &&
                        !ReferenceEquals(sourceRecord, record))
                    {
                        ApplyCornerRecoveryProvisionalEvidence(
                            sourceRecord,
                            conflict,
                            lastPositiveWidth);
                    }
                }
            }
        }

        private static void AddCornerRecoveryProtectedStates(
            List<SortedSet<int>> frontier,
            ICollection<int> baselineExclusions,
            ICollection<int> conflictParticipants,
            ICollection<int> recoveryEdges)
        {
            const int MaximumEnumeratedParticipants = 6;
            if (frontier == null || conflictParticipants == null ||
                conflictParticipants.Count == 0)
            {
                return;
            }

            List<int> neighbours = new List<int>();
            foreach (int participant in conflictParticipants)
            {
                if (recoveryEdges == null ||
                    !recoveryEdges.Contains(participant))
                {
                    neighbours.Add(participant);
                }
            }
            neighbours.Sort();
            if (neighbours.Count == 0)
            {
                return;
            }

            if (neighbours.Count <= MaximumEnumeratedParticipants)
            {
                int stateCount = 1 << neighbours.Count;
                for (int mask = 1; mask < stateCount; mask++)
                {
                    SortedSet<int> state = baselineExclusions == null
                        ? new SortedSet<int>()
                        : new SortedSet<int>(baselineExclusions);
                    for (int participantIndex = 0;
                         participantIndex < neighbours.Count;
                         participantIndex++)
                    {
                        if ((mask & (1 << participantIndex)) != 0)
                        {
                            state.Add(neighbours[participantIndex]);
                        }
                    }
                    frontier.Add(state);
                }
                return;
            }

            for (int participantIndex = 0;
                 participantIndex < neighbours.Count;
                 participantIndex++)
            {
                SortedSet<int> state = baselineExclusions == null
                    ? new SortedSet<int>()
                    : new SortedSet<int>(baselineExclusions);
                state.Add(neighbours[participantIndex]);
                frontier.Add(state);
            }
            SortedSet<int> allNeighbours = baselineExclusions == null
                ? new SortedSet<int>()
                : new SortedSet<int>(baselineExclusions);
            allNeighbours.UnionWith(neighbours);
            frontier.Add(allNeighbours);
        }

        private static void ApplyCornerRecoveryProvisionalEvidence(
            EdgeWearEdgeLifecycleRecord record,
            ChamferCornerConflictRecord conflict,
            float lastPositiveWidth)
        {
            if (record == null || conflict == null)
            {
                return;
            }
            record.CornerRecoveryProvisional = true;
            record.CornerRecoveryCollapsedSourceEdgeIndex =
                conflict.UnselectedSourceEdgeIndex;
            record.CornerRecoveryLastPositiveWidth =
                Mathf.Max(0f, lastPositiveWidth);
            record.CornerRecoveryUniformScale =
                conflict.UniformScale;
            record.CornerRecoveryZeroingStage =
                conflict.ZeroingStage;
            record.CornerRecoveryParticipants =
                FormatChamferForcedDeferralKey(
                    conflict.ParticipatingSelectedEdges);
            record.CornerRecoveryZeroedParticipants =
                FormatChamferForcedDeferralKey(
                    conflict.ZeroedSelectedEdges);
        }

        private static void ApplyCornerRecoveryResolution(
            EdgeWearCoverageAudit coverage,
            ICollection<int> recoveryEdges,
            string resolution,
            string evidence)
        {
            if (coverage == null || recoveryEdges == null)
            {
                return;
            }

            foreach (int recoveryEdge in recoveryEdges)
            {
                if (!coverage.RecordByGraphEdge.TryGetValue(
                        recoveryEdge,
                        out EdgeWearEdgeLifecycleRecord record) ||
                    record == null ||
                    !record.CornerRecoveryProvisional)
                {
                    continue;
                }

                record.CornerRecoveryResolution =
                    string.IsNullOrEmpty(evidence)
                        ? resolution ?? string.Empty
                        : (resolution ?? string.Empty) + ":" + evidence;
                if (string.Equals(
                        resolution,
                        "certified-recovery",
                        StringComparison.Ordinal))
                {
                    record.CornerRecoveryResolution = record.Built
                        ? "certified-recovery"
                        : "unresolved:not-recovered-by-winning-augmentation";
                }
                else if (string.Equals(
                        resolution,
                        "proven-infeasible",
                        StringComparison.Ordinal))
                {
                    record.FinalReason =
                        "corner-recovery-proven-infeasible";
                    record.CoexistenceFailureReason =
                        record.FinalReason;
                }
            }
        }

        private static SortedSet<int> CollectBaselineSelectedExclusions(
            ChamferTopologyContext context,
            EdgeWearCoverageAudit baselineCoverage)
        {
            SortedSet<int> exclusions = new SortedSet<int>();
            if (context == null || baselineCoverage == null)
            {
                return exclusions;
            }
            for (int selectedIndex = 0;
                 selectedIndex < context.SelectedEdges.Count;
                 selectedIndex++)
            {
                int graphEdgeIndex =
                    context.SelectedEdges[selectedIndex].GraphEdgeIndex;
                if (!baselineCoverage.RecordByGraphEdge.TryGetValue(
                        graphEdgeIndex,
                        out EdgeWearEdgeLifecycleRecord record) ||
                    record == null ||
                    !record.Built)
                {
                    exclusions.Add(graphEdgeIndex);
                }
            }
            return exclusions;
        }

        private static int CountRecoveredProvisionalEdges(
            ICollection<int> recoveryEdges,
            EdgeWearCoverageAudit baselineCoverage,
            EdgeWearCoverageAudit candidateCoverage)
        {
            if (recoveryEdges == null || candidateCoverage == null)
            {
                return 0;
            }
            int count = 0;
            foreach (int edgeIndex in recoveryEdges)
            {
                bool baselineBuilt = baselineCoverage != null &&
                    baselineCoverage.RecordByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeLifecycleRecord baselineRecord) &&
                    baselineRecord != null &&
                    baselineRecord.Built;
                bool candidateBuilt =
                    candidateCoverage.RecordByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeLifecycleRecord candidateRecord) &&
                    candidateRecord != null &&
                    candidateRecord.Built;
                if (!baselineBuilt && candidateBuilt)
                {
                    count++;
                }
            }
            return count;
        }

        private static void CalculateChamferPlaneRetentionMetrics(
            ChamferPlaneRetentionTrialOutcome outcome)
        {
            if (outcome == null || outcome.Coverage == null)
            {
                return;
            }
            for (int recordIndex = 0;
                 recordIndex < outcome.Coverage.Records.Count;
                 recordIndex++)
            {
                EdgeWearEdgeLifecycleRecord record =
                    outcome.Coverage.Records[recordIndex];
                if (!record.Built)
                {
                    continue;
                }
                outcome.CertifiedCount++;
                outcome.CertifiedScore += record.Score;
                outcome.CertifiedWidth += record.MaterializedWidth;
            }
        }

        private static bool
            IsChamferPlaneRetentionTrialAcceptableForRecovery(
                ChamferPlaneRetentionTrialOutcome candidate,
                ChamferPlaneRetentionTrialOutcome baseline,
                ICollection<int> recoveryEdges,
                Dictionary<int, SortedSet<int>>
                    cornerRecoveryParticipants)
        {
            if (candidate == null || !candidate.FullyValid ||
                candidate.RecoveredProvisionalCount <= 0 ||
                (baseline != null && baseline.FullyValid &&
                 candidate.CertifiedCount < baseline.CertifiedCount))
            {
                return false;
            }
            if (baseline == null || !baseline.FullyValid ||
                baseline.Coverage == null || candidate.Coverage == null)
            {
                return true;
            }

            HashSet<int> allowedBaselineLosses = new HashSet<int>();
            if (recoveryEdges != null &&
                cornerRecoveryParticipants != null)
            {
                foreach (int recoveryEdge in recoveryEdges)
                {
                    bool baselineBuilt =
                        baseline.Coverage.RecordByGraphEdge.TryGetValue(
                            recoveryEdge,
                            out EdgeWearEdgeLifecycleRecord baselineRecord) &&
                        baselineRecord != null && baselineRecord.Built;
                    bool candidateBuilt =
                        candidate.Coverage.RecordByGraphEdge.TryGetValue(
                            recoveryEdge,
                            out EdgeWearEdgeLifecycleRecord candidateRecord) &&
                        candidateRecord != null && candidateRecord.Built;
                    if (!baselineBuilt && candidateBuilt &&
                        cornerRecoveryParticipants.TryGetValue(
                            recoveryEdge,
                            out SortedSet<int> participants))
                    {
                        allowedBaselineLosses.UnionWith(participants);
                    }
                }
            }

            foreach (KeyValuePair<int, EdgeWearEdgeLifecycleRecord> pair
                in baseline.Coverage.RecordByGraphEdge)
            {
                int edgeIndex = pair.Key;
                EdgeWearEdgeLifecycleRecord baselineRecord = pair.Value;
                if (baselineRecord == null || !baselineRecord.Built)
                {
                    continue;
                }
                bool candidateBuilt =
                    candidate.Coverage.RecordByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeLifecycleRecord candidateRecord) &&
                    candidateRecord != null && candidateRecord.Built;
                if (!candidateBuilt &&
                    !allowedBaselineLosses.Contains(edgeIndex))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsChamferPlaneRecoveryTrialBetter(
            ChamferPlaneRetentionTrialOutcome candidate,
            ChamferPlaneRetentionTrialOutcome current)
        {
            if (candidate == null || !candidate.FullyValid)
            {
                return false;
            }
            if (current == null || !current.FullyValid)
            {
                return true;
            }
            if (candidate.RecoveredProvisionalCount !=
                current.RecoveredProvisionalCount)
            {
                return candidate.RecoveredProvisionalCount >
                    current.RecoveredProvisionalCount;
            }
            return IsChamferPlaneRetentionTrialBetter(
                candidate,
                current);
        }

        private static bool IsChamferPlaneRetentionTrialBetter(
            ChamferPlaneRetentionTrialOutcome candidate,
            ChamferPlaneRetentionTrialOutcome baseline)
        {
            const double Tolerance = 0.000000001;
            if (candidate == null || !candidate.FullyValid)
            {
                return false;
            }
            if (baseline == null || !baseline.FullyValid)
            {
                return true;
            }
            if (candidate.CertifiedCount != baseline.CertifiedCount)
            {
                return candidate.CertifiedCount > baseline.CertifiedCount;
            }
            if (candidate.CertifiedScore >
                baseline.CertifiedScore + Tolerance)
            {
                return true;
            }
            if (baseline.CertifiedScore >
                candidate.CertifiedScore + Tolerance)
            {
                return false;
            }
            return candidate.CertifiedWidth >
                baseline.CertifiedWidth + Tolerance;
        }

        private static void ApplyCertifiedBaselineAugmentationMetadata(
            ref PlaneCutBevelAuditResult audit,
            ChamferPlaneRetentionTrialOutcome baseline,
            bool attempted,
            bool applied,
            int statesEvaluated,
            int frontierRemaining,
            double elapsedMilliseconds,
            bool timeBudgetExceeded,
            bool cancelled,
            string failure,
            string lastFailure,
            string implicatedEdges)
        {
            audit.BaselineCertified = baseline != null &&
                baseline.FullyValid ? 1 : 0;
            audit.BaselineApplied = applied ? 0 : audit.BaselineCertified;
            audit.AugmentationAttempted = attempted ? 1 : 0;
            audit.AugmentationApplied = applied ? 1 : 0;
            audit.AugmentationStatesEvaluated = statesEvaluated;
            audit.AugmentationFrontierRemaining = frontierRemaining;
            audit.AugmentationElapsedMilliseconds = elapsedMilliseconds;
            audit.AugmentationTimeBudgetExceeded =
                timeBudgetExceeded ? 1 : 0;
            audit.AugmentationCancelled = cancelled ? 1 : 0;
            audit.AugmentationFailure = failure ?? string.Empty;
            audit.AugmentationLastFailure = lastFailure ?? string.Empty;
            audit.AugmentationImplicatedEdgeEvidence =
                implicatedEdges ?? string.Empty;
        }

        private static string AppendEdgeWearDiagnostic(
            string current,
            string addition)
        {
            if (string.IsNullOrEmpty(addition))
            {
                return current ?? string.Empty;
            }
            return string.IsNullOrEmpty(current)
                ? addition
                : current + "; " + addition;
        }

        private static List<int>
            CollectChamferPlaneRetentionBranchEdges(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit coverageAudit,
                ChamferPlaneRetentionTrialOutcome outcome)
        {
            SortedSet<int> branchEdges = new SortedSet<int>();
            if (outcome != null && outcome.CornerSolution != null)
            {
                List<ChamferCornerConflictRecord> conflicts =
                    outcome.CornerSolution.Conflicts;
                for (int conflictIndex = 0;
                     conflictIndex < conflicts.Count;
                     conflictIndex++)
                {
                    branchEdges.UnionWith(
                        conflicts[conflictIndex]
                            .ParticipatingSelectedEdges);
                }
            }

            if (outcome != null && outcome.CornersReady &&
                outcome.PlaneAudit.SelectedEdgeCount > 0)
            {
                if (outcome.PlaneAudit.EdgeConflictVictimEdgeIndex >= 0)
                {
                    branchEdges.Add(
                        outcome.PlaneAudit.EdgeConflictVictimEdgeIndex);
                }
                if (outcome.PlaneAudit.EdgeConflictForeignEdgeIndex >= 0)
                {
                    branchEdges.Add(
                        outcome.PlaneAudit.EdgeConflictForeignEdgeIndex);
                }
                if (outcome.PlaneAudit.DebugFocusEdgeIndices != null)
                {
                    branchEdges.UnionWith(
                        outcome.PlaneAudit.DebugFocusEdgeIndices);
                }
            }

            if (branchEdges.Count == 0 && coverageAudit != null)
            {
                for (int selectedIndex = 0;
                     selectedIndex < context.SelectedEdges.Count;
                     selectedIndex++)
                {
                    int graphEdgeIndex =
                        context.SelectedEdges[selectedIndex]
                            .GraphEdgeIndex;
                    if (coverageAudit.ViabilityByGraphEdge.TryGetValue(
                            graphEdgeIndex,
                            out EdgeWearEdgeViabilityRecord viability) &&
                        viability != null &&
                        viability.MultiSupportHullRecovery)
                    {
                        branchEdges.Add(graphEdgeIndex);
                    }
                }
            }
            return new List<int>(branchEdges);
        }

        private static int
            CompareChamferForcedDeferralSetsByPriority(
                EdgeWearCoverageAudit coverageAudit,
                ICollection<int> left,
                ICollection<int> right)
        {
            if (left.Count != right.Count)
            {
                return left.Count.CompareTo(right.Count);
            }

            double leftScore = CalculateChamferDeferredScore(
                coverageAudit,
                left);
            double rightScore = CalculateChamferDeferredScore(
                coverageAudit,
                right);
            const double ScoreTolerance = 0.000000001;
            if (leftScore + ScoreTolerance < rightScore)
            {
                return -1;
            }
            if (rightScore + ScoreTolerance < leftScore)
            {
                return 1;
            }

            double leftWidth = CalculateChamferDeferredCertifiedWidth(
                coverageAudit,
                left);
            double rightWidth = CalculateChamferDeferredCertifiedWidth(
                coverageAudit,
                right);
            if (leftWidth + ScoreTolerance < rightWidth)
            {
                return -1;
            }
            if (rightWidth + ScoreTolerance < leftWidth)
            {
                return 1;
            }
            return CompareChamferForcedDeferralSets(left, right);
        }

        private static double CalculateChamferDeferredScore(
            EdgeWearCoverageAudit coverageAudit,
            ICollection<int> edges)
        {
            if (coverageAudit == null || edges == null)
            {
                return 0.0;
            }

            double total = 0.0;
            foreach (int edgeIndex in edges)
            {
                if (coverageAudit.RecordByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeLifecycleRecord record) &&
                    record != null)
                {
                    total += record.Score;
                }
            }
            return total;
        }

        private static double CalculateChamferDeferredCertifiedWidth(
            EdgeWearCoverageAudit coverageAudit,
            ICollection<int> edges)
        {
            if (coverageAudit == null || edges == null)
            {
                return 0.0;
            }

            double total = 0.0;
            foreach (int edgeIndex in edges)
            {
                if (coverageAudit.ViabilityByGraphEdge.TryGetValue(
                        edgeIndex,
                        out EdgeWearEdgeViabilityRecord viability) &&
                    viability != null)
                {
                    total += viability.IsolatedMaximumCertifiedWidth;
                }
            }
            return total;
        }

        private static int CompareChamferForcedDeferralSets(
            ICollection<int> left,
            ICollection<int> right)
        {
            if (left.Count != right.Count)
            {
                return left.Count.CompareTo(right.Count);
            }
            List<int> leftOrdered = new List<int>(left);
            List<int> rightOrdered = new List<int>(right);
            leftOrdered.Sort();
            rightOrdered.Sort();
            for (int index = 0; index < leftOrdered.Count; index++)
            {
                int order = leftOrdered[index].CompareTo(
                    rightOrdered[index]);
                if (order != 0)
                {
                    return order;
                }
            }
            return 0;
        }

        private static string FormatChamferForcedDeferralKey(
            ICollection<int> edges)
        {
            if (edges == null || edges.Count == 0)
            {
                return "none";
            }
            List<int> ordered = new List<int>(edges);
            ordered.Sort();
            return string.Join("/", ordered);
        }

        private static EdgeWearDebugEdgeState
            ResolveEdgeWearDebugEdgeState(
                EdgeWearEdgeLifecycleRecord record)
        {
            if (record == null)
            {
                return EdgeWearDebugEdgeState.Unassessed;
            }
            if (record.MicroTopologySuppressed)
            {
                return EdgeWearDebugEdgeState.MicroTopologySuppressed;
            }
            if (record.Built)
            {
                return EdgeWearDebugEdgeState.Certified;
            }

            string viabilityFailure = record.Viability != null
                ? record.Viability.FailureReason
                : string.Empty;
            string coexistenceFailure =
                record.CoexistenceFailureReason ?? string.Empty;
            if (viabilityFailure ==
                    "maximum-feasible-width-below-minimum-scale" ||
                viabilityFailure ==
                    "maximum-certified-width-at-stable-width-floor" ||
                coexistenceFailure == "global-width-floor-conflict" ||
                coexistenceFailure == "corner-width-missing" ||
                coexistenceFailure == "corner-width-inactive" ||
                coexistenceFailure == "recovery-baseline-deferred" ||
                coexistenceFailure ==
                    "width-recovery-proven-infeasible")
            {
                return EdgeWearDebugEdgeState.WidthFloorFailure;
            }
            if (viabilityFailure.StartsWith(
                    "isolated-rail",
                    StringComparison.Ordinal))
            {
                return EdgeWearDebugEdgeState.IsolatedRailFailure;
            }
            if (record.GeometricEligible && !record.ArtisticEligible)
            {
                return EdgeWearDebugEdgeState.ArtisticFiltered;
            }
            if (record.GeometricEligible &&
                !record.CoexistenceEligible)
            {
                return EdgeWearDebugEdgeState.CoexistenceExcluded;
            }
            if (record.Selected)
            {
                return EdgeWearDebugEdgeState.Selected;
            }
            if (record.GeometricEligible &&
                record.CoexistenceEligible &&
                record.ArtisticEligible)
            {
                return EdgeWearDebugEdgeState.EligibleUnselected;
            }
            if (!record.StructuralEligible)
            {
                return EdgeWearDebugEdgeState.StructuralExcluded;
            }
            if (!record.GeometricEligible)
            {
                return EdgeWearDebugEdgeState.GeometricExcluded;
            }
            return EdgeWearDebugEdgeState.Unassessed;
        }

        private static string ResolveEdgeWearDebugEdgeReason(
            EdgeWearEdgeLifecycleRecord record)
        {
            if (record == null)
            {
                return "unassessed";
            }
            if (record.Built)
            {
                return "certified";
            }
            if (!string.IsNullOrEmpty(record.CoexistenceFailureReason))
            {
                return record.CoexistenceFailureReason;
            }
            if (record.Viability != null &&
                !string.IsNullOrEmpty(record.Viability.FailureReason))
            {
                return record.Viability.FailureReason;
            }
            if (!string.IsNullOrEmpty(record.CandidateReason))
            {
                return record.CandidateReason;
            }
            return string.IsNullOrEmpty(record.FinalReason)
                ? "unassessed"
                : record.FinalReason;
        }

        #endregion
    }
}
