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
            bool runUnifiedEvaluation =
                applyUnifiedBoundedPreview ||
                runUnifiedBatchAudit ||
                runUnifiedPreviewBatchAudit ||
                buildSourceEdgeIndexDebug;
            bool includeAllGeometricCandidates = runUnifiedBatchAudit;
            bool logUnifiedAudit = !buildSourceEdgeIndexDebug;
            if (!surfaceFeatures.HasValue || faces == null || faces.Count < 4)
            {
                return null;
            }

            MassSurfaceFeatureSettings settings = surfaceFeatures.Value;
            float amount01 = Mathf.Clamp01(settings.EdgeWearAmount * 0.5f);
            if (amount01 <= 0.0001f)
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
            List<EdgeWearBevelCandidate> candidates =
                BuildEdgeWearBevelCandidates(
                    faces,
                    bounds,
                    maximumDimension,
                    recipe,
                    settings,
                    amount01,
                    requestedWidth,
                    includeAllGeometricCandidates,
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
                                faces,
                                coverageAudit));
                }
                return null;
            }

            candidates.Sort((left, right) => right.Score.CompareTo(left.Score));

            float coverage01 = Mathf.Clamp01(settings.EdgeWearCoverage * 0.5f);
            int selectedCount = Mathf.Clamp(
                Mathf.CeilToInt(candidates.Count * coverage01),
                0,
                candidates.Count);
            CaptureEdgeWearArtisticSelectionAudit(
                coverageAudit,
                candidates,
                selectedCount);
            if (selectedCount <= 0)
            {
                return null;
            }

            float minimumStableEdgeLength = maximumDimension * 0.0012f;
            ChamferReadinessStats stats = new ChamferReadinessStats(
                candidates.Count,
                selectedCount);

            bool ready = TryBuildChamferTopologyContext(
                faces,
                candidates,
                selectedCount,
                minimumStableEdgeLength,
                ref stats,
                out ChamferTopologyContext context,
                out string blocker);

            if (ready)
            {
                MapEdgeWearCoverageAuditToGraph(
                    coverageAudit,
                    context);
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
                            faces,
                            coverageAudit));
            }

            if (ready)
            {
                float minimumStableFaceArea =
                    maximumDimension * maximumDimension * 0.000001f;

                if (runUnifiedEvaluation)
                {
                    ChamferCornerStats cornerStats =
                        new ChamferCornerStats();
                    bool cornersReady;
                    ChamferCornerSolution cornerSolution;
                    string cornerBlocker;
                    PlaneCutBevelAuditResult allEdgeAudit;
                    TriangleSoup allEdgePreviewSoup;
                    if (HasSelectedWidthRecoveryProvisional(
                            context,
                            coverageAudit))
                    {
                        cornersReady =
                            TryAuditSingleSearchChamferPlaneSolution(
                                faces,
                                context,
                                recipe,
                                requestedWidth,
                                minimumStableEdgeLength,
                                minimumStableFaceArea,
                                coverageAudit,
                                ref cornerStats,
                                out cornerSolution,
                                out allEdgeAudit,
                                out allEdgePreviewSoup,
                                out cornerBlocker);
                    }
                    else
                    {
                        cornersReady = AuditExplicitChamferCornerSolution(
                            faces,
                            context,
                            requestedWidth,
                            minimumStableEdgeLength,
                            minimumStableFaceArea,
                            coverageAudit,
                            null,
                            ref cornerStats,
                            out cornerSolution,
                            out cornerBlocker);
                        allEdgeAudit = default;
                        allEdgePreviewSoup = null;
                        if (cornersReady)
                        {
                            ApplyEdgeWearCoverageCornerSolution(
                                coverageAudit,
                                context,
                                cornerSolution);
                            allEdgeAudit = AuditPlaneCutBevelKernel(
                                faces,
                                context,
                                cornerSolution,
                                minimumStableEdgeLength,
                                minimumStableFaceArea,
                                coverageAudit,
                                true,
                                out allEdgePreviewSoup);
                        }
                        else
                        {
                            allEdgeAudit.CoverageAudit = coverageAudit;
                            allEdgeAudit.SelectedEdgeCount =
                                context.SelectedSourceEdges.Count;
                            allEdgeAudit.Diagnostic =
                                string.IsNullOrEmpty(cornerBlocker)
                                    ? "the shared all-edge corner solution failed"
                                    : cornerBlocker;
                        }
                    }

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
                                coverageAudit,
                                allEdgeAudit.DebugFocusEdgeIndices));
                    if (previewApplied)
                    {
                        return allEdgePreviewSoup;
                    }
                }
                else if (applyBoundedSingleEdgePreview)
                {
                    BoundedSingleEdgeAuditResult boundedAudit =
                        AuditBoundedSingleEdgeBevel(
                            faces,
                            context,
                            boundedEdgeOrdinal,
                            requestedWidth,
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
                        faces,
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
                                    faces,
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
                                    faces,
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
            EdgeWearDebugEdgeRecord[] records =
                new EdgeWearDebugEdgeRecord[graph.Edges.Count];
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
                records[edgeIndex] = new EdgeWearDebugEdgeRecord(
                    edgeIndex,
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
            }
            return records;
        }

        private sealed class ChamferPlaneRetentionTrialOutcome
        {
            public readonly SortedSet<int> ForcedDeferredEdges =
                new SortedSet<int>();
            public ChamferCornerSolution CornerSolution;
            public PlaneCutBevelAuditResult PlaneAudit;
            public TriangleSoup PreviewSoup;
            public EdgeWearCoverageAudit Coverage;
            public bool CornersReady;
            public bool FullyValid;
            public string Blocker = string.Empty;
        }

        private static bool HasSelectedWidthRecoveryProvisional(
            ChamferTopologyContext context,
            EdgeWearCoverageAudit coverageAudit)
        {
            if (context == null || coverageAudit == null)
            {
                return false;
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
                    return true;
                }
            }
            return false;
        }

        private static bool TryAuditSingleSearchChamferPlaneSolution(
            List<PolygonFace> sourceFaces,
            ChamferTopologyContext context,
            MassRecipe recipe,
            float requestedWidth,
            float minimumStableEdgeLength,
            float minimumStableFaceArea,
            EdgeWearCoverageAudit coverageAudit,
            ref ChamferCornerStats stats,
            out ChamferCornerSolution winningSolution,
            out PlaneCutBevelAuditResult winningAudit,
            out TriangleSoup winningPreviewSoup,
            out string blocker)
        {
            const int MaximumSearchStates = 128;
            const int MaximumForcedDeferrals = 10;
            const double MaximumSearchMilliseconds = 5000.0;

            winningSolution = null;
            winningAudit = default;
            winningPreviewSoup = null;
            blocker = string.Empty;

            List<SortedSet<int>> frontier = new List<SortedSet<int>>
            {
                new SortedSet<int>()
            };
            HashSet<string> visited = new HashSet<string>();
            ChamferPlaneRetentionTrialOutcome winner = null;
            int evaluatedStates = 0;
            bool timeBudgetExceeded = false;
            bool cancelled = false;
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
                        forced);
                if (outcome.FullyValid)
                {
                    winner = outcome;
                    break;
                }

                if (forced.Count >= MaximumForcedDeferrals)
                {
                    continue;
                }

                List<int> branchEdges =
                    CollectChamferPlaneRetentionBranchEdges(
                        context,
                        coverageAudit,
                        outcome);
                for (int edgeIndex = 0;
                     edgeIndex < branchEdges.Count;
                     edgeIndex++)
                {
                    int edgeToDefer = branchEdges[edgeIndex];
                    if (forced.Contains(edgeToDefer))
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
            stats.ConflictSearchStatesEvaluated = evaluatedStates;
            stats.ConflictSearchElapsedMilliseconds =
                stopwatch.Elapsed.TotalMilliseconds;
            stats.ConflictSearchTimeBudgetExceeded =
                timeBudgetExceeded ? 1 : 0;
            stats.ConflictSearchCancelled = cancelled ? 1 : 0;
            if (winner == null)
            {
                if (cancelled)
                {
                    blocker = "conflict-search-cancelled";
                }
                else if (timeBudgetExceeded)
                {
                    blocker = "conflict-search-time-budget-exceeded" +
                        " (statesEvaluated=" + evaluatedStates +
                        ",frontierRemaining=" + frontier.Count + ")";
                }
                else if (frontier.Count > 0)
                {
                    blocker = "conflict-search-state-budget-exceeded" +
                        " (statesEvaluated=" + evaluatedStates +
                        ",frontierRemaining=" + frontier.Count + ")";
                }
                else
                {
                    blocker = "single conflict-directed search found no " +
                        "fully certified shell";
                }
                return false;
            }

            HashSet<int> winningForced =
                new HashSet<int>(winner.ForcedDeferredEdges);
            stats.ConflictSearchCommittedExclusionCount =
                winningForced.Count;
            bool cornersReady = AuditExplicitChamferCornerSolution(
                sourceFaces,
                context,
                requestedWidth,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                coverageAudit,
                winningForced,
                ref stats,
                out winningSolution,
                out blocker);
            if (!cornersReady)
            {
                return false;
            }

            ApplyEdgeWearCoverageCornerSolution(
                coverageAudit,
                context,
                winningSolution);
            try
            {
                winningAudit = AuditPlaneCutBevelKernel(
                    sourceFaces,
                    context,
                    winningSolution,
                    minimumStableEdgeLength,
                    minimumStableFaceArea,
                    coverageAudit,
                    false,
                    out winningPreviewSoup);
            }
            catch (InvalidOperationException exception)
            {
                blocker = "the committed single-search state failed " +
                    "final render-channel validation: " +
                    exception.Message;
                winningAudit.CoverageAudit = coverageAudit;
                winningAudit.Diagnostic = blocker;
                winningPreviewSoup = null;
                return false;
            }

            if (winningAudit.GeometryValid == 1 &&
                winningPreviewSoup != null)
            {
                try
                {
                    BuildMeshData(winningPreviewSoup, recipe);
                }
                catch (InvalidOperationException exception)
                {
                    blocker = "the committed single-search state failed " +
                        "final mesh-data validation: " + exception.Message;
                    winningAudit.Diagnostic = blocker;
                    winningPreviewSoup = null;
                    return false;
                }
            }

            if (winningAudit.GeometryValid != 1 ||
                winningPreviewSoup == null)
            {
                blocker = string.IsNullOrEmpty(winningAudit.Diagnostic)
                    ? "the committed single-search state did not certify"
                    : winningAudit.Diagnostic;
                return false;
            }
            return true;
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
                SortedSet<int> forcedDeferredEdges)
        {
            ChamferPlaneRetentionTrialOutcome outcome =
                new ChamferPlaneRetentionTrialOutcome();
            outcome.ForcedDeferredEdges.UnionWith(
                forcedDeferredEdges);
            outcome.Coverage = sourceCoverage.CloneForTrial();
            ChamferCornerStats trialStats = new ChamferCornerStats();
            HashSet<int> forced = new HashSet<int>(
                forcedDeferredEdges);
            outcome.CornersReady = AuditExplicitChamferCornerSolution(
                sourceFaces,
                context,
                requestedWidth,
                minimumStableEdgeLength,
                minimumStableFaceArea,
                outcome.Coverage,
                forced,
                ref trialStats,
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
                    false,
                    out outcome.PreviewSoup);
            }
            catch (InvalidOperationException exception)
            {
                outcome.Blocker =
                    "render-channel-validation:" + exception.Message;
                return outcome;
            }

            if (outcome.PlaneAudit.GeometryValid == 1 &&
                outcome.PreviewSoup != null)
            {
                try
                {
                    BuildMeshData(outcome.PreviewSoup, recipe);
                }
                catch (InvalidOperationException exception)
                {
                    outcome.Blocker =
                        "render-channel-validation:" + exception.Message;
                    outcome.PreviewSoup = null;
                }
            }
            outcome.FullyValid =
                outcome.PlaneAudit.GeometryValid == 1 &&
                outcome.PreviewSoup != null;
            if (outcome.FullyValid)
            {
                outcome.Blocker = string.Empty;
            }
            else if (string.IsNullOrEmpty(outcome.Blocker))
            {
                outcome.Blocker = outcome.PlaneAudit.Diagnostic;
            }
            return outcome;
        }

        private static List<int>
            CollectChamferPlaneRetentionBranchEdges(
                ChamferTopologyContext context,
                EdgeWearCoverageAudit coverageAudit,
                ChamferPlaneRetentionTrialOutcome outcome)
        {
            SortedSet<int> branchEdges = new SortedSet<int>();
            if (outcome.CornerSolution != null)
            {
                List<ChamferCornerConflictRecord> conflicts =
                    outcome.CornerSolution.Conflicts;
                for (int conflictIndex = 0;
                     conflictIndex < conflicts.Count;
                     conflictIndex++)
                {
                    ChamferCornerConflictRecord conflict =
                        conflicts[conflictIndex];
                    for (int participantIndex = 0;
                         participantIndex <
                            conflict.ParticipatingSelectedEdges.Count;
                         participantIndex++)
                    {
                        branchEdges.Add(
                            conflict.ParticipatingSelectedEdges[
                                participantIndex]);
                    }
                }
            }

            if (outcome.CornersReady &&
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
            }

            if (branchEdges.Count == 0 &&
                coverageAudit != null)
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
                        viability.WidthRecoveryProvisional)
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
                coexistenceFailure == "global-width-floor-conflict" ||
                coexistenceFailure == "corner-width-missing" ||
                coexistenceFailure == "corner-width-inactive")
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
