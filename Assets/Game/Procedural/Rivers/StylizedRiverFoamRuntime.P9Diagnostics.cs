#if UNITY_EDITOR
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        /// <summary>
        /// One-button RG-METRIC-P9 validation pipeline. It installs the assigned
        /// cache into temporary live resources, validates film, shape, and render
        /// coordinate contracts while those resources are alive, then releases
        /// the transaction and proves cache immutability.
        /// </summary>
        public bool RunP9ComprehensiveValidationReport()
        {
            topologyCacheDiagnosticRunCount++;
            topologyCacheDiagnosticState = "Running";
            topologyCacheDiagnosticSummary =
                "Running the P9 film, shape, and rendering validation pipeline.";
            topologyCacheDiagnosticReport = string.Empty;
            topologyCacheDiagnosticReportPath = string.Empty;

            if (Application.isPlaying)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "The P9 comprehensive report is Edit Mode only.");
            }

            river = GetComponent<StylizedRiver>();
            surfaceRenderer = river != null ? river.SurfaceRenderer : null;
            disturbanceRuntime ??=
                GetComponent<StylizedRiverDisturbanceRuntime>();
            if (river == null || !river.Domain.IsValid ||
                disturbanceRuntime == null)
            {
                return FailCacheDiagnostic(
                    "Unavailable",
                    "A valid river and Disturbance runtime are required.");
            }

            System.Diagnostics.Stopwatch stopwatch =
                System.Diagnostics.Stopwatch.StartNew();
            StringBuilder report = new(65536);
            report.AppendLine(
                "RIVER FOAM FIXED-METRIC P9 COMPREHENSIVE VALIDATION");
            report.AppendLine(BuildCommonEnvironmentHeader());
            report.AppendLine(
                "Operation: one explicit user-triggered report; installs the " +
                "assigned cache into temporary live resources, validates film, " +
                "shape, and production/debug coordinate contracts before " +
                "cleanup, then releases resources. No cache, scene, prefab, " +
                "material, or serialized river state is stored.");
            report.AppendLine(
                "Contracts: exact structural-to-film grouping + represented " +
                "physical area + descriptor-owned fixed render mapping; the " +
                "authored active grid selection must remain authoritative.");
            report.AppendLine();

            StylizedRiverFoamTopologyCacheAsset assignedAsset =
                river.FoamTopologyCacheAsset;
            string assignedMetadataBefore = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadBefore = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? assignedAsset.GetPayloadCopy()
                    : Array.Empty<byte>();

            bool livePreparationReady = false;
            bool transactionStarted = false;
            bool resourcesCompleteWhileMeasured = false;
            bool activeSelectionExact = false;
            bool fixedCandidateReady = false;
            bool groupingExact = false;
            bool areaExact = false;
            bool filmSourceExact = false;
            bool occupancyExact = false;
            bool shapeExact = false;
            bool renderMappingExact = false;
            bool kernelResourceExact = false;
            bool liveStateUntouched = false;
            bool cleanupCompleted = false;
            bool cleanupStateExact = false;
            bool bindingsDisabled = false;
            string cleanupFailure = string.Empty;

            RenderTexture stateBefore = currentState;
            RenderTexture filmSourceBefore = filmSourceTexture;
            RenderTexture filmSupportBefore = filmSupportTexture;
            RenderTexture occupancyBefore = currentVisualOccupancy;
            RenderTexture shapeBefore = shapeMaskTexture;

            try
            {
                livePreparationReady = TryPrepareP6LiveValidationResources(
                    report,
                    out transactionStarted,
                    out string preparationFailure);
                resourcesCompleteWhileMeasured = livePreparationReady &&
                    initializationPhase == InitializationPhase.Ready &&
                    AreResourcesCompleteAndCurrent();
                activeSelectionExact = resourcesCompleteWhileMeasured &&
                    FoamGridSelectionMatchesActive;
                fixedCandidateReady = resourcesCompleteWhileMeasured &&
                    fixedMetricCandidateDescriptor.IsCreated &&
                    fixedMetricCandidateDescriptor.UsesFixedMetricLattice;

                report.AppendLine("ACTIVE FILM / RENDER OWNERSHIP");
                report.AppendLine(
                    $"Live preparation ready: {livePreparationReady}");
                report.AppendLine(
                    $"Resources complete while measured: " +
                    $"{resourcesCompleteWhileMeasured}");
                report.AppendLine(
                    $"Authored selection: {river.FoamGridMode} / " +
                    $"{river.FoamFixedMetricCellSize} / " +
                    $"requested={river.FoamFixedMetricRequestedCellSizeMetres:0.000}m");
                report.AppendLine(
                    $"Active descriptor: " +
                    $"{(gridDescriptor.IsCreated ? gridDescriptor.Mapping.ToString() : "Unallocated")} / " +
                    $"{gridDescriptor.InitializationSignature:X16}");
                report.AppendLine(
                    $"Fixed candidate: {fixedCandidateReady}; status=" +
                    $"{fixedMetricCandidateFailureReason}");
                report.AppendLine(
                    "P9 preserves the authored active mapping while validating " +
                    "the migrated film, shape, and render consumers. It does not " +
                    "retune sources or alter unrelated water rendering.");
                if (!livePreparationReady &&
                    !string.IsNullOrEmpty(preparationFailure))
                {
                    report.AppendLine(
                        "Preparation failure: " + preparationFailure);
                }
                report.AppendLine(
                    "ACTIVE GRID SELECTION VERDICT: " +
                    (activeSelectionExact && fixedCandidateReady
                        ? "PASS"
                        : "FAIL"));
                report.AppendLine();

                if (resourcesCompleteWhileMeasured &&
                    activeSelectionExact && fixedCandidateReady)
                {
                    stateBefore = currentState;
                    filmSourceBefore = filmSourceTexture;
                    filmSupportBefore = filmSupportTexture;
                    occupancyBefore = currentVisualOccupancy;
                    shapeBefore = shapeMaskTexture;

                    groupingExact = ValidateP9FilmGrouping(report);
                    areaExact = ValidateP9RepresentedArea(report);
                    filmSourceExact = ValidateP9GpuFilmSource(report);
                    occupancyExact = ValidateP9VisualOccupancyGeometry(report);
                    shapeExact = ValidateP9GpuShapeMapping(report);
                    renderMappingExact = ValidateP9ProductionDebugMapping(report);
                    kernelResourceExact = ValidateP9KernelResourceContract(report);

                    liveStateUntouched =
                        currentState == stateBefore &&
                        filmSourceTexture == filmSourceBefore &&
                        filmSupportTexture == filmSupportBefore &&
                        currentVisualOccupancy == occupancyBefore &&
                        shapeMaskTexture == shapeBefore;
                    report.AppendLine("LIVE RUNTIME STATE MUTATION PROOF");
                    report.AppendLine(
                        $"Persistent state reference unchanged: " +
                        $"{currentState == stateBefore}");
                    report.AppendLine(
                        $"Film source/support references unchanged: " +
                        $"{filmSourceTexture == filmSourceBefore}/" +
                        $"{filmSupportTexture == filmSupportBefore}");
                    report.AppendLine(
                        $"Visual occupancy/shape references unchanged: " +
                        $"{currentVisualOccupancy == occupancyBefore}/" +
                        $"{shapeMaskTexture == shapeBefore}");
                    report.AppendLine(
                        "LIVE STATE VERDICT: " +
                        (liveStateUntouched ? "PASS" : "FAIL"));
                    report.AppendLine();
                }
                else
                {
                    AppendP9SkippedContract(
                        report,
                        "FILM GROUPING AND ODD-EDGE CONTRACT",
                        "FILM GROUPING VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "REPRESENTED PHYSICAL AREA CONTRACT",
                        "REPRESENTED AREA VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "GPU FILM SOURCE AGGREGATION",
                        "GPU FILM SOURCE VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "VISUAL OCCUPANCY PHYSICAL GEOMETRY",
                        "VISUAL OCCUPANCY VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "GPU SHAPE / FILM MAPPING",
                        "GPU SHAPE MAPPING VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "PRODUCTION / DEBUG PHYSICAL-POINT MAPPING",
                        "PRODUCTION / DEBUG VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "KERNEL AND RESOURCE CONTRACT",
                        "KERNEL / RESOURCE VERDICT",
                        preparationFailure);
                    AppendP9SkippedContract(
                        report,
                        "LIVE RUNTIME STATE MUTATION PROOF",
                        "LIVE STATE VERDICT",
                        preparationFailure);
                }
            }
            catch (Exception exception)
            {
                report.AppendLine("P9 LIVE EVIDENCE EXCEPTION");
                report.AppendLine(exception.ToString());
                report.AppendLine("LIVE EVIDENCE VERDICT: FAIL");
                report.AppendLine();
            }
            finally
            {
                if (transactionStarted)
                {
                    try
                    {
                        ReleaseResources();
                        editorTopologyPreparationInProgress = false;
                        explicitTopologyGenerationInProgress = false;
                        initializationPhase = InitializationPhase.NotStarted;
                        resourcesDirty = true;
                        boundaryDirty = true;
                        BindDisabled();
                        cleanupCompleted = true;
                        cleanupStateExact =
                            IsP6DiagnosticCleanupStateExact();
                        bindingsDisabled = TryVerifyP6BindingsDisabled(
                            out string bindingDetail);
                        report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                        report.AppendLine(
                            $"Cleanup completed: {cleanupCompleted}");
                        report.AppendLine(
                            $"Runtime state reset: {cleanupStateExact}");
                        report.AppendLine(
                            $"Renderer bindings disabled: {bindingsDisabled}");
                        report.AppendLine(
                            $"Cleanup detail: {bindingDetail}");
                        report.AppendLine(
                            "DIAGNOSTIC CLEANUP VERDICT: " +
                            (cleanupCompleted && cleanupStateExact &&
                             bindingsDisabled ? "PASS" : "FAIL"));
                        report.AppendLine();
                    }
                    catch (Exception cleanupException)
                    {
                        cleanupFailure = cleanupException.ToString();
                        report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                        report.AppendLine("Cleanup exception:");
                        report.AppendLine(cleanupFailure);
                        report.AppendLine(
                            "DIAGNOSTIC CLEANUP VERDICT: FAIL");
                        report.AppendLine();
                    }
                }
                else
                {
                    report.AppendLine("DIAGNOSTIC CLEANUP PROOF");
                    report.AppendLine(
                        "No P9-owned live transaction was started; no foreign " +
                        "runtime state was released.");
                    report.AppendLine(
                        "DIAGNOSTIC CLEANUP VERDICT: " +
                        (!livePreparationReady ? "PASS" : "FAIL"));
                    report.AppendLine();
                    cleanupCompleted = !livePreparationReady;
                    cleanupStateExact = !livePreparationReady;
                    bindingsDisabled = !livePreparationReady;
                }
            }

            string assignedMetadataAfter = assignedAsset != null
                ? BuildAssignedCacheMetadataSignature(assignedAsset)
                : "<none>";
            byte[] assignedPayloadAfter = assignedAsset != null &&
                assignedAsset.HasPayload
                    ? assignedAsset.GetPayloadReadOnlyReference()
                    : Array.Empty<byte>();
            bool metadataUnchanged = string.Equals(
                assignedMetadataBefore,
                assignedMetadataAfter,
                StringComparison.Ordinal);
            bool payloadUnchanged = ByteArraysEqual(
                assignedPayloadBefore,
                assignedPayloadAfter,
                out int firstDifference);

            report.AppendLine("ASSIGNED CACHE MUTATION PROOF");
            report.AppendLine($"Metadata unchanged: {metadataUnchanged}");
            report.AppendLine($"Payload unchanged: {payloadUnchanged}");
            if (!payloadUnchanged)
            {
                report.AppendLine(
                    $"First differing payload byte: {firstDifference}");
            }
            report.AppendLine(
                "ASSIGNED CACHE VERDICT: " +
                (metadataUnchanged && payloadUnchanged ? "PASS" : "FAIL"));
            report.AppendLine();

            bool cleanupExact = cleanupCompleted && cleanupStateExact &&
                bindingsDisabled && string.IsNullOrEmpty(cleanupFailure);
            bool cacheExact = metadataUnchanged && payloadUnchanged;
            bool overall = livePreparationReady &&
                resourcesCompleteWhileMeasured && activeSelectionExact &&
                fixedCandidateReady && groupingExact && areaExact &&
                filmSourceExact && occupancyExact && shapeExact &&
                renderMappingExact && kernelResourceExact &&
                liveStateUntouched && cleanupExact && cacheExact;

            stopwatch.Stop();
            report.AppendLine("FINAL LEDGER");
            report.AppendLine(
                "Live diagnostic preparation transaction: " +
                (livePreparationReady ? "PASS" : "FAIL"));
            report.AppendLine(
                "Authored active grid selection preserved: " +
                (activeSelectionExact && fixedCandidateReady
                    ? "PASS"
                    : "FAIL"));
            report.AppendLine(
                "Film grouping and odd-edge mapping: " +
                (groupingExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Represented physical area and bank/padded coverage: " +
                (areaExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Film source actual GPU aggregation: " +
                (filmSourceExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Visual occupancy physical geometry: " +
                (occupancyExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Shape/film structural mapping actual GPU: " +
                (shapeExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Production/debug physical-point mapping: " +
                (renderMappingExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Kernel/resource and unrelated-render ownership: " +
                (kernelResourceExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Live runtime state remained untouched: " +
                (liveStateUntouched ? "PASS" : "FAIL"));
            report.AppendLine(
                "Diagnostic cleanup and disabled bindings: " +
                (cleanupExact ? "PASS" : "FAIL"));
            report.AppendLine(
                "Assigned cache remained unchanged: " +
                (cacheExact ? "PASS" : "FAIL"));
            report.AppendLine($"Elapsed: {stopwatch.Elapsed.TotalMilliseconds:F3} ms");
            report.AppendLine("Overall: " + (overall ? "PASS" : "FAIL"));

            return FinalizeP9Report(
                report,
                overall,
                overall
                    ? "P9 film, shape, and rendering contracts passed."
                    : "P9 film, shape, or rendering validation failed.");
        }

        private static bool ValidateP9FilmGrouping(StringBuilder report)
        {
            report.AppendLine("FILM GROUPING AND ODD-EDGE CONTRACT");
            bool exact = true;
            int dimensionsChecked = 0;
            int structuralCellsChecked = 0;
            for (int structuralCount = 1; structuralCount <= 513;
                 structuralCount++)
            {
                int filmCount = Mathf.Max(1, (structuralCount + 1) / 2);
                bool[] covered = new bool[structuralCount];
                for (int filmIndex = 0; filmIndex < filmCount; filmIndex++)
                {
                    P9ResolveFilmRange(
                        filmIndex,
                        structuralCount,
                        out int start,
                        out int count);
                    exact &= count >= 1 && count <= 2;
                    exact &= start == filmIndex * 2;
                    exact &= start + count <= structuralCount;
                    float fieldUv = (start + count * 0.5f) / structuralCount;
                    float filmUv = P9FieldUvToFilmUv(
                        fieldUv,
                        structuralCount,
                        filmCount);
                    exact &= P9Approximately(
                        filmUv,
                        (filmIndex + 0.5f) / filmCount,
                        0.000002f);
                    for (int local = 0; local < count; local++)
                    {
                        int structuralIndex = start + local;
                        exact &= !covered[structuralIndex];
                        covered[structuralIndex] = true;
                        structuralCellsChecked++;
                    }
                }
                for (int index = 0; index < covered.Length; index++)
                {
                    exact &= covered[index];
                }
                if ((structuralCount & 1) != 0)
                {
                    P9ResolveFilmRange(
                        filmCount - 1,
                        structuralCount,
                        out int terminalStart,
                        out int terminalCount);
                    exact &= terminalStart == structuralCount - 1 &&
                        terminalCount == 1;
                }
                dimensionsChecked++;
            }

            report.AppendLine(
                $"Structural dimensions checked: {dimensionsChecked}");
            report.AppendLine(
                $"Structural cells covered exactly once: " +
                $"{structuralCellsChecked:N0}");
            report.AppendLine(
                "Odd terminal groups represent one structural cell; all other " +
                "film groups represent two structural cells.");
            report.AppendLine(
                "FILM GROUPING VERDICT: " + (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private static bool ValidateP9RepresentedArea(StringBuilder report)
        {
            report.AppendLine("REPRESENTED PHYSICAL AREA CONTRACT");
            const int width = 17;
            const int height = 9;
            const float dx = 0.15f;
            const float dy = 0.15f;
            int filmWidth = (width + 1) / 2;
            int filmHeight = (height + 1) / 2;
            float[] curvature = new float[width];
            for (int x = 0; x < width; x++)
            {
                curvature[x] = Mathf.Lerp(-0.035f, 0.042f,
                    x / (float)(width - 1));
            }

            float structuralArea = 0f;
            float filmArea = 0f;
            float validStructuralArea = 0f;
            float validFilmArea = 0f;
            bool exact = true;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float n = (y - 4) * dy;
                    float area = dx * dy *
                        Mathf.Max(0.25f, 1f - curvature[x] * n);
                    structuralArea += area;
                    bool valid = x < width - 1 && y > 0 &&
                        !(x == 3 && y == 4);
                    if (valid)
                    {
                        validStructuralArea += area;
                    }
                }
            }

            int partialGroups = 0;
            for (int fx = 0; fx < filmWidth; fx++)
            {
                P9ResolveFilmRange(fx, width, out int startX, out int countX);
                for (int fy = 0; fy < filmHeight; fy++)
                {
                    P9ResolveFilmRange(
                        fy,
                        height,
                        out int startY,
                        out int countY);
                    if (countX != 2 || countY != 2)
                    {
                        partialGroups++;
                    }
                    float represented = 0f;
                    float representedValid = 0f;
                    for (int lx = 0; lx < countX; lx++)
                    {
                        for (int ly = 0; ly < countY; ly++)
                        {
                            int x = startX + lx;
                            int y = startY + ly;
                            float n = (y - 4) * dy;
                            float area = dx * dy *
                                Mathf.Max(0.25f, 1f - curvature[x] * n);
                            represented += area;
                            bool valid = x < width - 1 && y > 0 &&
                                !(x == 3 && y == 4);
                            if (valid)
                            {
                                representedValid += area;
                            }
                        }
                    }
                    exact &= represented > 0f;
                    exact &= representedValid >= 0f &&
                        representedValid <= represented + 0.000001f;
                    filmArea += represented;
                    validFilmArea += representedValid;
                }
            }
            exact &= P9Approximately(structuralArea, filmArea, 0.00001f);
            exact &= P9Approximately(
                validStructuralArea,
                validFilmArea,
                0.00001f);

            report.AppendLine(
                $"Structural/film represented area: " +
                $"{structuralArea:R} / {filmArea:R} m²");
            report.AppendLine(
                $"Valid structural/film area: " +
                $"{validStructuralArea:R} / {validFilmArea:R} m²");
            report.AppendLine(
                $"Odd-edge partial film groups: {partialGroups}");
            report.AppendLine(
                "Banks, obstacles, and padded columns contribute zero valid " +
                "coverage while retaining their represented physical area in " +
                "the aggregation denominator.");
            report.AppendLine(
                "REPRESENTED AREA VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP9GpuFilmSource(StringBuilder report)
        {
            report.AppendLine("GPU FILM SOURCE AGGREGATION");
            const int width = 5;
            const int height = 5;
            const int filmWidth = 3;
            const int filmHeight = 3;
            const float dx = 0.15f;
            const float dy = 0.15f;
            const float validLength = 0.60f;

            if (computeShader == null || buildFilmSourceKernel < 0)
            {
                report.AppendLine(
                    "BuildFoamFilmSource kernel is unavailable.");
                report.AppendLine("GPU FILM SOURCE VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            RenderTexture output = null;
            Texture2D state = null;
            Texture2D boundary = null;
            Texture2D obstacle = null;
            Texture2D topology = null;
            Texture2D topologySources = null;
            Texture2D readback = null;
            ComputeBuffer metricRows = null;
            try
            {
                output = P9CreateRFloatRenderTexture(
                    filmWidth,
                    filmHeight,
                    "PS3D_P9_FilmSourceOutput");
                state = P9CreateColorTexture(
                    width,
                    height,
                    new Color(1f, 1f, 0.35f, 0f),
                    "PS3D_P9_FilmSourceState");
                float[] boundaryValues = new float[width * height];
                Array.Fill(boundaryValues, 1f);
                for (int x = 0; x < width; x++)
                {
                    boundaryValues[x] = 0f;
                    boundaryValues[(height - 1) * width + x] = 0f;
                }
                boundary = P9CreateScalarTexture(
                    width,
                    height,
                    boundaryValues,
                    "PS3D_P9_FilmSourceBoundary");
                float[] obstacleValues = new float[width * height];
                obstacleValues[1 + width] = 1f;
                obstacleValues[1 + (height - 2) * width] = 1f;
                obstacle = P9CreateScalarTexture(
                    width,
                    height,
                    obstacleValues,
                    "PS3D_P9_FilmSourceObstacle");
                topology = P9CreateColorTexture(
                    width,
                    height,
                    Color.clear,
                    "PS3D_P9_FilmSourceTopology");
                topologySources = P9CreateColorTexture(
                    width,
                    height,
                    Color.clear,
                    "PS3D_P9_FilmSourceTopologySources");

                FoamMetricRow[] rows = P9CreateSyntheticMetricRows(
                    width,
                    dx,
                    dy,
                    0f);
                metricRows = new ComputeBuffer(
                    width,
                    Marshal.SizeOf<FoamMetricRow>(),
                    ComputeBufferType.Structured);
                metricRows.SetData(rows);

                P9ConfigureSyntheticFixedDescriptor(
                    width,
                    height,
                    filmWidth,
                    filmHeight,
                    dx,
                    dy,
                    validLength);
                computeShader.SetInts("_FoamDimensions", width, height);
                computeShader.SetInts(
                    "_FoamFilmDimensions",
                    filmWidth,
                    filmHeight);
                computeShader.SetInts(
                    "_FoamTopologyDimensions",
                    width,
                    height);
                computeShader.SetFloat("_FoamFieldLength", width * dx);
                computeShader.SetFloat("_FoamValidLength", validLength);
                computeShader.SetFloat(
                    "_FoamSimulationLength",
                    width * dx);
                computeShader.SetBuffer(
                    buildFilmSourceKernel,
                    "_FoamMetricRows",
                    metricRows);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamBoundary",
                    boundary);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamObstacleExclusionRead",
                    obstacle);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamStateRead",
                    state);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamTopologyRead",
                    topology);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamTopologySourcesRead",
                    topologySources);
                computeShader.SetTexture(
                    buildFilmSourceKernel,
                    "_FoamFilmSourceWrite",
                    output);
                computeShader.Dispatch(
                    buildFilmSourceKernel,
                    Mathf.CeilToInt(filmWidth / 8f),
                    Mathf.CeilToInt(filmHeight / 8f),
                    1);

                readback = P9ReadbackRFloat(output);
                int mismatches = 0;
                Vector2Int firstMismatch = new(-1, -1);
                float firstExpected = 0f;
                float firstObserved = 0f;
                for (int fy = 0; fy < filmHeight; fy++)
                {
                    P9ResolveFilmRange(
                        fy,
                        height,
                        out int startY,
                        out int countY);
                    for (int fx = 0; fx < filmWidth; fx++)
                    {
                        P9ResolveFilmRange(
                            fx,
                            width,
                            out int startX,
                            out int countX);
                        int represented = countX * countY;
                        int valid = 0;
                        for (int lx = 0; lx < countX; lx++)
                        {
                            for (int ly = 0; ly < countY; ly++)
                            {
                                int x = startX + lx;
                                int y = startY + ly;
                                bool insideValidLength =
                                    (x + 0.5f) * dx <=
                                    validLength + 0.000001f;
                                bool fluid = y > 0 && y < height - 1 &&
                                    !(x == 1 &&
                                      (y == 1 || y == height - 2)) &&
                                    insideValidLength;
                                if (fluid)
                                {
                                    valid++;
                                }
                            }
                        }
                        float expected = represented > 0
                            ? 0.94f * valid / represented
                            : 0f;
                        float observed = readback.GetPixel(fx, fy).r;
                        if (!P9Approximately(expected, observed, 0.002f))
                        {
                            if (mismatches == 0)
                            {
                                firstMismatch = new Vector2Int(fx, fy);
                                firstExpected = expected;
                                firstObserved = observed;
                            }
                            mismatches++;
                        }
                    }
                }

                bool exact = mismatches == 0;
                report.AppendLine(
                    $"Synthetic dimensions: structural={width}x{height}; " +
                    $"film={filmWidth}x{filmHeight}; validLength=" +
                    $"{validLength:R}m");
                report.AppendLine(
                    $"GPU mismatches: {mismatches}; first=" +
                    $"({firstMismatch.x},{firstMismatch.y}); expected=" +
                    $"{firstExpected:R}; observed={firstObserved:R}");
                report.AppendLine(
                    "GPU FILM SOURCE VERDICT: " +
                    (exact ? "PASS" : "FAIL"));
                report.AppendLine();
                return exact;
            }
            catch (Exception exception)
            {
                report.AppendLine("GPU film-source exception:");
                report.AppendLine(exception.ToString());
                report.AppendLine("GPU FILM SOURCE VERDICT: FAIL");
                report.AppendLine();
                return false;
            }
            finally
            {
                metricRows?.Release();
                ReleaseTexture(ref output);
                DestroyUnityObject(state);
                DestroyUnityObject(boundary);
                DestroyUnityObject(obstacle);
                DestroyUnityObject(topology);
                DestroyUnityObject(topologySources);
                DestroyUnityObject(readback);
            }
        }

        private bool ValidateP9VisualOccupancyGeometry(
            StringBuilder report)
        {
            report.AppendLine("VISUAL OCCUPANCY PHYSICAL GEOMETRY");
            const int width = 15;
            const int height = 7;
            const float dx = 0.15f;
            const float dy = 0.15f;
            int filmWidth = (width + 1) / 2;
            int filmHeight = (height + 1) / 2;
            int cases = 0;
            bool exact = true;
            for (int fx = 0; fx < filmWidth; fx++)
            {
                P9ResolveFilmRange(fx, width, out int startX, out int countX);
                for (int fy = 0; fy < filmHeight; fy++)
                {
                    P9ResolveFilmRange(
                        fy,
                        height,
                        out int startY,
                        out int countY);
                    float area = 0f;
                    float lateralFace = 0f;
                    for (int lx = 0; lx < countX; lx++)
                    {
                        int x = startX + lx;
                        float curvature = -0.04f + x * 0.006f;
                        for (int ly = 0; ly < countY; ly++)
                        {
                            int y = startY + ly;
                            float n = (y - 3) * dy;
                            area += dx * dy *
                                Mathf.Max(0.25f, 1f - curvature * n);
                        }
                        float faceN = (startY + countY - 3) * dy;
                        lateralFace += dx *
                            Mathf.Max(0.25f, 1f - curvature * faceN);
                    }
                    float longitudinalFace = countY * dy;
                    exact &= area > 0f;
                    exact &= lateralFace > 0f;
                    exact &= P9Approximately(
                        longitudinalFace,
                        countY * dy,
                        0.000001f);
                    cases++;
                }
            }

            string computePath = P9ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string compute = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string advanceBody = P7ExtractFunctionBody(
                compute,
                "void AdvanceFoamVisualOccupancy(");
            string insideSimulationBody = P7ExtractFunctionBody(
                compute,
                "bool FoamVisualOccupancyInsideSimulation(");
            string cellAreaBody = P7ExtractFunctionBody(
                compute,
                "float FoamVisualOccupancyCellArea(");
            string lateralFaceBody = P7ExtractFunctionBody(
                compute,
                "void FoamResolveVisualLateralFaceFlux(");
            string lateralSpacingBody = P7ExtractFunctionBody(
                compute,
                "float FoamVisualOccupancyLateralSpacing(");
            string longitudinalBody = P7ExtractFunctionBody(
                compute,
                "float FoamVisualOccupancyLongitudinalSpacing(");
            bool bodiesFound = !string.IsNullOrEmpty(advanceBody) &&
                !string.IsNullOrEmpty(insideSimulationBody) &&
                !string.IsNullOrEmpty(cellAreaBody) &&
                !string.IsNullOrEmpty(lateralFaceBody) &&
                !string.IsNullOrEmpty(lateralSpacingBody) &&
                !string.IsNullOrEmpty(longitudinalBody);
            bool physicalOwners = bodiesFound &&
                insideSimulationBody.Contains(
                    "IsFoamGridColumnInsideSimulation",
                    StringComparison.Ordinal) &&
                advanceBody.Contains(
                    "FoamVisualOccupancyCellArea(coordinate)",
                    StringComparison.Ordinal) &&
                cellAreaBody.Contains(
                    "FoamTransportCellArea",
                    StringComparison.Ordinal) &&
                lateralFaceBody.Contains(
                    "FoamTransportLateralFaceLength",
                    StringComparison.Ordinal) &&
                lateralSpacingBody.Contains(
                    "FoamTransportLateralSpacing",
                    StringComparison.Ordinal) &&
                longitudinalBody.Contains(
                    "FoamTransportLongitudinalSpacing",
                    StringComparison.Ordinal);
            exact &= physicalOwners;
            bool gpuAdvectionExact = ValidateP9GpuVisualOccupancyAdvection(
                report);
            exact &= gpuAdvectionExact;

            report.AppendLine(
                $"Synthetic film geometry cases: {cases}");
            report.AppendLine(
                $"Inspected function bodies found: advance=" +
                $"{!string.IsNullOrEmpty(advanceBody)}; insideSimulation=" +
                $"{!string.IsNullOrEmpty(insideSimulationBody)}; area=" +
                $"{!string.IsNullOrEmpty(cellAreaBody)}; lateralFace=" +
                $"{!string.IsNullOrEmpty(lateralFaceBody)}; lateralSpacing=" +
                $"{!string.IsNullOrEmpty(lateralSpacingBody)}; longitudinal=" +
                $"{!string.IsNullOrEmpty(longitudinalBody)}");
            report.AppendLine(
                $"Curvature-aware area/face ownership: {physicalOwners}");
            report.AppendLine(
                $"Actual GPU finite-volume advection: {gpuAdvectionExact}");
            report.AppendLine(
                "VISUAL OCCUPANCY VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP9GpuVisualOccupancyAdvection(
            StringBuilder report)
        {
            const int width = 5;
            const int height = 5;
            const int filmWidth = 3;
            const int filmHeight = 3;
            const float dx = 0.15f;
            const float dy = 0.15f;
            const float deltaTime = 0.10f;
            const float downstreamSpeed = 0.10f;

            if (computeShader == null || advanceVisualOccupancyKernel < 0)
            {
                report.AppendLine(
                    "AdvanceFoamVisualOccupancy kernel is unavailable.");
                return false;
            }

            RenderTexture output = null;
            Texture2D boundary = null;
            Texture2D obstacle = null;
            Texture2D motionLane = null;
            Texture2D routing = null;
            Texture2D filmSource = null;
            Texture2D filmSupport = null;
            Texture2D occupancy = null;
            Texture2D readback = null;
            ComputeBuffer metricRows = null;
            try
            {
                output = P9CreateRFloatRenderTexture(
                    filmWidth,
                    filmHeight,
                    "PS3D_P9_VisualOccupancyOutput");
                boundary = P8CreateScalarTexture(
                    width,
                    height,
                    1f,
                    "PS3D_P9_VisualOccupancyBoundary");
                obstacle = P8CreateScalarTexture(
                    width,
                    height,
                    0f,
                    "PS3D_P9_VisualOccupancyObstacle");
                motionLane = P8CreateScalarTexture(
                    width,
                    height,
                    0f,
                    "PS3D_P9_VisualOccupancyMotionLane");
                routing = P9CreateRoutingTexture(
                    width,
                    height,
                    "PS3D_P9_VisualOccupancyRouting");
                filmSource = P8CreateScalarTexture(
                    filmWidth,
                    filmHeight,
                    0f,
                    "PS3D_P9_VisualOccupancyFilmSource");
                filmSupport = P8CreateScalarTexture(
                    filmWidth,
                    filmHeight,
                    0f,
                    "PS3D_P9_VisualOccupancyFilmSupport");

                float[] occupancyValues = new float[filmWidth * filmHeight];
                float[] rowValues = { 0.20f, 0.50f, 0.80f };
                for (int y = 0; y < filmHeight; y++)
                {
                    for (int x = 0; x < filmWidth; x++)
                    {
                        occupancyValues[y * filmWidth + x] = rowValues[x];
                    }
                }
                occupancy = P9CreateScalarTexture(
                    filmWidth,
                    filmHeight,
                    occupancyValues,
                    "PS3D_P9_VisualOccupancyRead");

                FoamMetricRow[] rows = P9CreateSyntheticMetricRows(
                    width,
                    dx,
                    dy,
                    0f);
                metricRows = new ComputeBuffer(
                    width,
                    Marshal.SizeOf<FoamMetricRow>(),
                    ComputeBufferType.Structured);
                metricRows.SetData(rows);

                P9ConfigureSyntheticFixedDescriptor(
                    width,
                    height,
                    filmWidth,
                    filmHeight,
                    dx,
                    dy,
                    width * dx);
                computeShader.SetInts("_FoamDimensions", width, height);
                computeShader.SetInts(
                    "_FoamFilmDimensions",
                    filmWidth,
                    filmHeight);
                computeShader.SetFloat("_FoamFieldLength", width * dx);
                computeShader.SetFloat("_FoamValidLength", width * dx);
                computeShader.SetFloat(
                    "_FoamSimulationLength",
                    width * dx);
                computeShader.SetFloat("_FoamDeltaTime", deltaTime);
                computeShader.SetFloat("_FoamFlowDirection", 1f);
                computeShader.SetFloat(
                    "_FoamBaseDownstreamSpeed",
                    downstreamSpeed);
                computeShader.SetFloat(
                    "_FoamMaximumLateralSpeedRatio",
                    0f);
                computeShader.SetFloat(
                    "_FoamObstacleSlowdownStrength",
                    0f);
                computeShader.SetFloat(
                    "_FoamObstacleMinimumDownstreamFactor",
                    1f);
                computeShader.SetFloat("_FoamMotionLaneScrollCells", 0f);
                computeShader.SetFloat(
                    "_FoamVisualOccupancyBuildTime",
                    1e20f);
                computeShader.SetFloat(
                    "_FoamVisualOccupancyReleaseTime",
                    1e20f);
                computeShader.SetBuffer(
                    advanceVisualOccupancyKernel,
                    "_FoamMetricRows",
                    metricRows);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamBoundary",
                    boundary);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamObstacleExclusionRead",
                    obstacle);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamMotionLaneRead",
                    motionLane);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamObstacleRoutingRead",
                    routing);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamFilmSourceRead",
                    filmSource);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamFilmSupportRead",
                    filmSupport);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamVisualOccupancyRead",
                    occupancy);
                computeShader.SetTexture(
                    advanceVisualOccupancyKernel,
                    "_FoamVisualOccupancyWrite",
                    output);
                computeShader.Dispatch(
                    advanceVisualOccupancyKernel,
                    Mathf.CeilToInt(filmWidth / 8f),
                    Mathf.CeilToInt(filmHeight / 8f),
                    1);

                readback = P9ReadbackRFloat(output);
                int mismatches = 0;
                Vector2Int firstMismatch = new(-1, -1);
                float firstExpected = 0f;
                float firstObserved = 0f;
                for (int y = 0; y < filmHeight; y++)
                {
                    P9ResolveFilmRange(
                        y,
                        height,
                        out _,
                        out int representedY);
                    float faceLength = representedY * dy;
                    for (int x = 0; x < filmWidth; x++)
                    {
                        P9ResolveFilmRange(
                            x,
                            width,
                            out _,
                            out int representedX);
                        float area = representedX * dx * faceLength;
                        float westFlux = x > 0
                            ? downstreamSpeed * faceLength * rowValues[x - 1]
                            : 0f;
                        float eastFlux = downstreamSpeed * faceLength *
                            rowValues[x];
                        float expected = rowValues[x] -
                            deltaTime / area * (eastFlux - westFlux);
                        float observed = readback.GetPixel(x, y).r;
                        if (!P9Approximately(expected, observed, 0.002f))
                        {
                            if (mismatches == 0)
                            {
                                firstMismatch = new Vector2Int(x, y);
                                firstExpected = expected;
                                firstObserved = observed;
                            }
                            mismatches++;
                        }
                    }
                }

                report.AppendLine(
                    $"Actual GPU advection: film={filmWidth}x{filmHeight}; " +
                    $"u={downstreamSpeed:R}m/s; dt={deltaTime:R}s; " +
                    $"mismatches={mismatches}; first=" +
                    $"({firstMismatch.x},{firstMismatch.y}); expected=" +
                    $"{firstExpected:R}; observed={firstObserved:R}");
                return mismatches == 0;
            }
            catch (Exception exception)
            {
                report.AppendLine("GPU visual-occupancy exception:");
                report.AppendLine(exception.ToString());
                return false;
            }
            finally
            {
                metricRows?.Release();
                ReleaseTexture(ref output);
                DestroyUnityObject(boundary);
                DestroyUnityObject(obstacle);
                DestroyUnityObject(motionLane);
                DestroyUnityObject(routing);
                DestroyUnityObject(filmSource);
                DestroyUnityObject(filmSupport);
                DestroyUnityObject(occupancy);
                DestroyUnityObject(readback);
            }
        }

        private bool ValidateP9GpuShapeMapping(StringBuilder report)
        {
            report.AppendLine("GPU SHAPE / FILM MAPPING");
            const int width = 5;
            const int height = 5;
            const int filmWidth = 3;
            const int filmHeight = 3;
            const float spacing = 0.15f;
            if (computeShader == null || evaluateShapeKernel < 0)
            {
                report.AppendLine("EvaluateFoamShape kernel is unavailable.");
                report.AppendLine("GPU SHAPE MAPPING VERDICT: FAIL");
                report.AppendLine();
                return false;
            }

            RenderTexture output = null;
            Texture2D boundary = null;
            Texture2D obstacle = null;
            Texture2D state = null;
            Texture2D occupancy = null;
            Texture2D readback = null;
            try
            {
                output = P9CreateRFloatRenderTexture(
                    width,
                    height,
                    "PS3D_P9_ShapeOutput");
                boundary = P8CreateScalarTexture(
                    width,
                    height,
                    1f,
                    "PS3D_P9_ShapeBoundary");
                obstacle = P8CreateScalarTexture(
                    width,
                    height,
                    0f,
                    "PS3D_P9_ShapeObstacle");
                state = P9CreateColorTexture(
                    width,
                    height,
                    Color.clear,
                    "PS3D_P9_ShapeState");
                float[] occupancyValues =
                {
                    0.05f, 0.15f, 0.25f,
                    0.35f, 0.45f, 0.55f,
                    0.65f, 0.75f, 0.85f
                };
                occupancy = P9CreateScalarTexture(
                    filmWidth,
                    filmHeight,
                    occupancyValues,
                    "PS3D_P9_ShapeOccupancy");

                P9ConfigureSyntheticFixedDescriptor(
                    width,
                    height,
                    filmWidth,
                    filmHeight,
                    spacing,
                    spacing,
                    width * spacing);
                computeShader.SetInts("_FoamDimensions", width, height);
                computeShader.SetInts(
                    "_FoamFilmDimensions",
                    filmWidth,
                    filmHeight);
                computeShader.SetFloat("_FoamFieldLength", width * spacing);
                computeShader.SetFloat("_FoamValidLength", width * spacing);
                computeShader.SetFloat(
                    "_FoamSimulationLength",
                    width * spacing);
                computeShader.SetInt("_FoamRangeStart", 0);
                computeShader.SetInt("_FoamRangeCount", width);
                computeShader.SetTexture(
                    evaluateShapeKernel,
                    "_FoamBoundary",
                    boundary);
                computeShader.SetTexture(
                    evaluateShapeKernel,
                    "_FoamObstacleExclusionRead",
                    obstacle);
                computeShader.SetTexture(
                    evaluateShapeKernel,
                    "_FoamStateRead",
                    state);
                computeShader.SetTexture(
                    evaluateShapeKernel,
                    "_FoamVisualOccupancyRead",
                    occupancy);
                computeShader.SetTexture(
                    evaluateShapeKernel,
                    "_FoamShapeMaskWrite",
                    output);
                computeShader.Dispatch(
                    evaluateShapeKernel,
                    Mathf.CeilToInt(width / 8f),
                    Mathf.CeilToInt(height / 8f),
                    1);

                readback = P9ReadbackRFloat(output);
                int mismatches = 0;
                Vector2Int firstMismatch = new(-1, -1);
                float firstExpected = 0f;
                float firstObserved = 0f;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float fieldU = (x + 0.5f) / width;
                        float fieldV = (y + 0.5f) / height;
                        float filmU = P9FieldUvToFilmUv(
                            fieldU,
                            width,
                            filmWidth);
                        float filmV = P9FieldUvToFilmUv(
                            fieldV,
                            height,
                            filmHeight);
                        float expected = P9SampleBilinear(
                            occupancyValues,
                            filmWidth,
                            filmHeight,
                            filmU,
                            filmV);
                        float observed = readback.GetPixel(x, y).r;
                        if (!P9Approximately(expected, observed, 0.002f))
                        {
                            if (mismatches == 0)
                            {
                                firstMismatch = new Vector2Int(x, y);
                                firstExpected = expected;
                                firstObserved = observed;
                            }
                            mismatches++;
                        }
                    }
                }

                bool exact = mismatches == 0;
                report.AppendLine(
                    $"Structural/film dimensions: {width}x{height} / " +
                    $"{filmWidth}x{filmHeight}");
                report.AppendLine(
                    $"GPU mismatches: {mismatches}; first=" +
                    $"({firstMismatch.x},{firstMismatch.y}); expected=" +
                    $"{firstExpected:R}; observed={firstObserved:R}");
                report.AppendLine(
                    "GPU SHAPE MAPPING VERDICT: " +
                    (exact ? "PASS" : "FAIL"));
                report.AppendLine();
                return exact;
            }
            catch (Exception exception)
            {
                report.AppendLine("GPU shape-mapping exception:");
                report.AppendLine(exception.ToString());
                report.AppendLine("GPU SHAPE MAPPING VERDICT: FAIL");
                report.AppendLine();
                return false;
            }
            finally
            {
                ReleaseTexture(ref output);
                DestroyUnityObject(boundary);
                DestroyUnityObject(obstacle);
                DestroyUnityObject(state);
                DestroyUnityObject(occupancy);
                DestroyUnityObject(readback);
            }
        }

        private static bool ValidateP9ProductionDebugMapping(
            StringBuilder report)
        {
            report.AppendLine("PRODUCTION / DEBUG PHYSICAL-POINT MAPPING");
            string includePath = P9ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/" +
                "Includes/RiverWaterFoam.hlsl");
            string shaderPath = P9ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Shaders/" +
                "SH_CleanStylizedRiver.shader");
            string include = File.Exists(includePath)
                ? File.ReadAllText(includePath)
                : string.Empty;
            string shader = File.Exists(shaderPath)
                ? File.ReadAllText(shaderPath)
                : string.Empty;

            string resolveBody = P7ExtractFunctionBody(
                include,
                "float2 RiverWaterFoamResolveFieldUV(");
            string validBody = P7ExtractFunctionBody(
                include,
                "bool RiverWaterFoamPointInsideValidField(");
            string filmBody = P7ExtractFunctionBody(
                include,
                "float2 RiverWaterFoamFieldUVToFilmUV(");
            string sampleValidityBody = P7ExtractFunctionBody(
                include,
                "bool RiverWaterFoamFieldUVInsideValidSample(");
            string metresBody = P7ExtractFunctionBody(
                include,
                "float2 RiverWaterFoamMetresToFieldUV(");
            string evaluateBody = P7ExtractFunctionBody(
                include,
                "RiverWaterFoamResult RiverWaterEvaluateFoam(");
            bool bodiesFound = !string.IsNullOrEmpty(resolveBody) &&
                !string.IsNullOrEmpty(validBody) &&
                !string.IsNullOrEmpty(filmBody) &&
                !string.IsNullOrEmpty(sampleValidityBody) &&
                !string.IsNullOrEmpty(metresBody) &&
                !string.IsNullOrEmpty(evaluateBody);

            bool fixedMapping = bodiesFound &&
                resolveBody.Contains(
                    "gridLongitudinal.x",
                    StringComparison.Ordinal) &&
                resolveBody.Contains(
                    "gridLateral.x",
                    StringComparison.Ordinal) &&
                resolveBody.Contains(
                    "gridLateral.y",
                    StringComparison.Ordinal) &&
                validBody.Contains(
                    "gridLongitudinal.z",
                    StringComparison.Ordinal) &&
                validBody.Contains(
                    "gridExtent.x",
                    StringComparison.Ordinal) &&
                validBody.Contains(
                    "gridExtent.y",
                    StringComparison.Ordinal) &&
                sampleValidityBody.Contains(
                    "gridLongitudinal.z",
                    StringComparison.Ordinal) &&
                evaluateBody.Contains(
                    "visualFoamSampleValid",
                    StringComparison.Ordinal) &&
                evaluateBody.Contains(
                    "leadSampleValid",
                    StringComparison.Ordinal) &&
                evaluateBody.Contains(
                    "trailSampleValid",
                    StringComparison.Ordinal) &&
                metresBody.Contains(
                    "gridSpacing.w",
                    StringComparison.Ordinal) &&
                metresBody.Contains(
                    "gridLateral.z",
                    StringComparison.Ordinal) &&
                metresBody.Contains(
                    "gridLongitudinal.y",
                    StringComparison.Ordinal);
            bool sharedProductionPoint = shader.Contains(
                    "RiverWaterFoamResult foam = RiverWaterEvaluateFoam(",
                    StringComparison.Ordinal) &&
                shader.Contains(
                    "float2 foamMotionFieldUV = foam.fieldUV;",
                    StringComparison.Ordinal) &&
                shader.Contains(
                    "float2 foamFilmUV = RiverWaterFoamFieldUVToFilmUV(",
                    StringComparison.Ordinal) &&
                P8ContainsInOrder(
                    shader,
                    "_FoamFilmSource,",
                    "foamFilmUV).r") &&
                P8ContainsInOrder(
                    shader,
                    "_FoamVisualOccupancy,",
                    "foamFilmUV).r");
            bool debugValidClip = shader.Contains(
                "if (foamDebug != 0 && foam.validField < 0.5)",
                StringComparison.Ordinal) &&
                evaluateBody.Contains(
                    "result.validField = 1.0;",
                    StringComparison.Ordinal);
            bool descriptorCall = P8ContainsInOrder(
                shader,
                "RiverWaterFoamResult foam = RiverWaterEvaluateFoam(",
                "_FoamGridDescriptorContract,",
                "_FoamGridDescriptorSpacing,",
                "_FoamGridDescriptorLateral,",
                "_FoamGridDescriptorLongitudinal,",
                "_FoamGridDescriptorExtent,",
                "foamSurface);");
            bool exact = fixedMapping && sharedProductionPoint &&
                debugValidClip && descriptorCall;

            report.AppendLine(
                $"Inspected function bodies found: resolve=" +
                $"{!string.IsNullOrEmpty(resolveBody)}; valid=" +
                $"{!string.IsNullOrEmpty(validBody)}; film=" +
                $"{!string.IsNullOrEmpty(filmBody)}; sampleValidity=" +
                $"{!string.IsNullOrEmpty(sampleValidityBody)}; metres=" +
                $"{!string.IsNullOrEmpty(metresBody)}; evaluate=" +
                $"{!string.IsNullOrEmpty(evaluateBody)}");
            report.AppendLine(
                $"Descriptor-owned fixed field/valid/offset mapping: " +
                $"{fixedMapping}");
            report.AppendLine(
                $"Production and debug paths reuse foam.fieldUV / foamFilmUV: " +
                $"{sharedProductionPoint}");
            report.AppendLine(
                $"Padded/outside fixed debug samples are rejected: " +
                $"{debugValidClip}");
            report.AppendLine(
                $"Production evaluator receives all descriptor lanes: " +
                $"{descriptorCall}");
            report.AppendLine(
                "PRODUCTION / DEBUG VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private bool ValidateP9KernelResourceContract(StringBuilder report)
        {
            report.AppendLine("KERNEL AND RESOURCE CONTRACT");
            string computePath = P9ResolveProjectAbsolutePath(
                "Assets/Game/Rendering/Water/Resources/PS3DRiver/Compute/" +
                "CS_RiverFoam.compute");
            string runtimePath = P9ResolveProjectAbsolutePath(
                "Assets/Game/Procedural/Rivers/" +
                "StylizedRiverFoamRuntime.Compute.cs");
            string compute = File.Exists(computePath)
                ? File.ReadAllText(computePath)
                : string.Empty;
            string runtime = File.Exists(runtimePath)
                ? File.ReadAllText(runtimePath)
                : string.Empty;

            string sourceBody = P7ExtractFunctionBody(
                compute,
                "void BuildFoamFilmSource(");
            string supportBody = P7ExtractFunctionBody(
                compute,
                "void BuildFoamFilmSupport(");
            string occupancyBody = P7ExtractFunctionBody(
                compute,
                "void AdvanceFoamVisualOccupancy(");
            string shapeBody = P7ExtractFunctionBody(
                compute,
                "void EvaluateFoamShape(");
            string boundaryBody = P7ExtractFunctionBody(
                compute,
                "void ApplyBoundary(");
            bool bodiesFound = !string.IsNullOrEmpty(sourceBody) &&
                !string.IsNullOrEmpty(supportBody) &&
                !string.IsNullOrEmpty(occupancyBody) &&
                !string.IsNullOrEmpty(shapeBody) &&
                !string.IsNullOrEmpty(boundaryBody);
            bool fixedFilmOwners = bodiesFound &&
                sourceBody.Contains(
                    "FoamResolveFixedFilmSource",
                    StringComparison.Ordinal) &&
                supportBody.Contains(
                    "FoamFilmTexelCentreFieldUV",
                    StringComparison.Ordinal) &&
                occupancyBody.Contains(
                    "FoamVisualOccupancyCellArea",
                    StringComparison.Ordinal) &&
                shapeBody.Contains(
                    "FoamSampleVisualOccupancyBilinear",
                    StringComparison.Ordinal) &&
                boundaryBody.Contains(
                    "IsFoamGridColumnInsideSimulation",
                    StringComparison.Ordinal);
            string configureVisualBody = P7ExtractFunctionBody(
                runtime,
                "private void ConfigureVisualShapeParameters(float deltaTime)");
            string applyBoundaryBody = P7ExtractFunctionBody(
                runtime,
                "private void ApplyBoundaryToState(RenderTexture target)");
            bool immediateDescriptorBinding =
                !string.IsNullOrEmpty(configureVisualBody) &&
                !string.IsNullOrEmpty(applyBoundaryBody) &&
                configureVisualBody.Contains(
                    "ConfigureGridDescriptorComputeParameters();",
                    StringComparison.Ordinal) &&
                applyBoundaryBody.Contains(
                    "ConfigureGridDescriptorComputeParameters();",
                    StringComparison.Ordinal);
            bool kernelsResolved = buildFilmSourceKernel >= 0 &&
                buildFilmSupportKernel >= 0 &&
                advanceVisualOccupancyKernel >= 0 &&
                evaluateShapeKernel >= 0 &&
                applyBoundaryKernel >= 0;
            bool noDiagnosticProductionResource =
                !compute.Contains(
                    "#pragma kernel P9",
                    StringComparison.Ordinal) &&
                !runtime.Contains(
                    "P9CreateRFloatRenderTexture",
                    StringComparison.Ordinal);
            bool exact = bodiesFound && fixedFilmOwners &&
                immediateDescriptorBinding && kernelsResolved &&
                noDiagnosticProductionResource;

            report.AppendLine(
                $"Inspected kernel bodies found: source=" +
                $"{!string.IsNullOrEmpty(sourceBody)}; support=" +
                $"{!string.IsNullOrEmpty(supportBody)}; occupancy=" +
                $"{!string.IsNullOrEmpty(occupancyBody)}; shape=" +
                $"{!string.IsNullOrEmpty(shapeBody)}; boundary=" +
                $"{!string.IsNullOrEmpty(boundaryBody)}");
            report.AppendLine(
                $"Fixed film/shape ownership present: {fixedFilmOwners}");
            report.AppendLine(
                $"Immediate descriptor binding present: " +
                $"{immediateDescriptorBinding}");
            report.AppendLine(
                $"Production kernels resolved: {kernelsResolved}");
            report.AppendLine(
                $"No P9-only production kernel/resource: " +
                $"{noDiagnosticProductionResource}");
            report.AppendLine(
                "Unrelated water-render byte-equivalence is verified by the " +
                "mechanical patch audit; this live report validates only the " +
                "owned Foam coordinate paths.");
            report.AppendLine(
                "KERNEL / RESOURCE VERDICT: " +
                (exact ? "PASS" : "FAIL"));
            report.AppendLine();
            return exact;
        }

        private void P9ConfigureSyntheticFixedDescriptor(
            int width,
            int height,
            int filmWidth,
            int filmHeight,
            float dx,
            float dy,
            float validLength)
        {
            float lateralMinimum = -0.5f * height * dy;
            float lateralMaximum = 0.5f * height * dy;
            computeShader.SetVector(
                "_FoamGridDescriptorContract",
                new Vector4(1f, 1f, 1f, (float)river.Quality));
            computeShader.SetVector(
                "_FoamGridDescriptorSpacing",
                new Vector4(dx, dy, dx, dy));
            computeShader.SetVector(
                "_FoamGridDescriptorLateral",
                new Vector4(0f, -(height / 2), height, 0f));
            computeShader.SetVector(
                "_FoamGridDescriptorLongitudinal",
                new Vector4(0f, width * dx, validLength, width));
            computeShader.SetVector(
                "_FoamGridDescriptorExtent",
                new Vector4(
                    lateralMinimum,
                    lateralMaximum,
                    filmWidth,
                    filmHeight));
        }

        private static FoamMetricRow[] P9CreateSyntheticMetricRows(
            int width,
            float dx,
            float dy,
            float curvature)
        {
            FoamMetricRow[] rows = new FoamMetricRow[width];
            for (int index = 0; index < rows.Length; index++)
            {
                rows[index] = new FoamMetricRow
                {
                    WidthsAndSpacing = new Vector4(
                        100f,
                        100f,
                        dx,
                        dy),
                    TopologyData = new Vector4(curvature, 0f, 0f, 0f),
                    ShoreData = Vector4.zero
                };
            }
            return rows;
        }

        private static RenderTexture P9CreateRFloatRenderTexture(
            int width,
            int height,
            string textureName)
        {
            RenderTexture texture = new RenderTexture(
                width,
                height,
                0,
                RenderTextureFormat.RFloat,
                RenderTextureReadWrite.Linear)
            {
                name = textureName,
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.Create();
            return texture;
        }

        private static Texture2D P9CreateColorTexture(
            int width,
            int height,
            Color value,
            string textureName)
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RGBAFloat,
                false,
                true)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            Color[] values = new Color[width * height];
            if (value != Color.clear)
            {
                Array.Fill(values, value);
            }
            texture.SetPixels(values);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D P9CreateRoutingTexture(
            int width,
            int height,
            string textureName)
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RGHalf,
                false,
                true)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixelData(new ushort[width * height * 2], 0);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D P9CreateScalarTexture(
            int width,
            int height,
            float[] values,
            string textureName)
        {
            Texture2D texture = new Texture2D(
                width,
                height,
                TextureFormat.RFloat,
                false,
                true)
            {
                name = textureName,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixelData(values, 0);
            texture.Apply(false, false);
            return texture;
        }

        private static Texture2D P9ReadbackRFloat(RenderTexture source)
        {
            RenderTexture previous = RenderTexture.active;
            Texture2D readback = new Texture2D(
                source.width,
                source.height,
                TextureFormat.RFloat,
                false,
                true)
            {
                name = source.name + "_Readback",
                hideFlags = HideFlags.DontSave
            };
            try
            {
                RenderTexture.active = source;
                readback.ReadPixels(
                    new Rect(0f, 0f, source.width, source.height),
                    0,
                    0,
                    false);
                readback.Apply(false, false);
            }
            finally
            {
                RenderTexture.active = previous;
            }
            return readback;
        }

        private static void P9ResolveFilmRange(
            int filmIndex,
            int structuralCount,
            out int start,
            out int count)
        {
            start = Mathf.Clamp(filmIndex * 2, 0, structuralCount - 1);
            count = Mathf.Clamp(structuralCount - start, 1, 2);
        }

        private static float P9FieldUvToFilmUv(
            float fieldUv,
            int structuralCount,
            int filmCount)
        {
            int safeStructural = Mathf.Max(1, structuralCount);
            int safeFilm = Mathf.Max(1, filmCount);
            float structuralPosition = Mathf.Clamp01(fieldUv) *
                safeStructural;
            int filmIndex = Mathf.Min(
                Mathf.FloorToInt(structuralPosition * 0.5f),
                safeFilm - 1);
            P9ResolveFilmRange(
                filmIndex,
                safeStructural,
                out int structuralStart,
                out int representedCount);
            float localPosition = Mathf.Clamp01(
                (structuralPosition - structuralStart) /
                Mathf.Max(1f, representedCount));
            return (filmIndex + localPosition) / safeFilm;
        }

        private static float P9SampleBilinear(
            float[] values,
            int width,
            int height,
            float u,
            float v)
        {
            float x = Mathf.Clamp(u, 0f, 1f) * width - 0.5f;
            float y = Mathf.Clamp(v, 0f, 1f) * height - 0.5f;
            x = Mathf.Clamp(x, 0f, width - 1f);
            y = Mathf.Clamp(y, 0f, height - 1f);
            int x0 = Mathf.FloorToInt(x);
            int y0 = Mathf.FloorToInt(y);
            int x1 = Mathf.Min(x0 + 1, width - 1);
            int y1 = Mathf.Min(y0 + 1, height - 1);
            float tx = x - x0;
            float ty = y - y0;
            float a = values[y0 * width + x0];
            float b = values[y0 * width + x1];
            float c = values[y1 * width + x0];
            float d = values[y1 * width + x1];
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        private static bool P9Approximately(
            float left,
            float right,
            float tolerance = 0.00001f)
        {
            return Mathf.Abs(left - right) <= tolerance;
        }

        private static string P9ResolveProjectAbsolutePath(
            string projectRelativePath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?
                .FullName ?? string.Empty;
            return Path.GetFullPath(Path.Combine(
                projectRoot,
                projectRelativePath.Replace('/', Path.DirectorySeparatorChar)));
        }

        private static void AppendP9SkippedContract(
            StringBuilder report,
            string heading,
            string verdictLabel,
            string reason)
        {
            report.AppendLine(heading);
            report.AppendLine(
                "Skipped because the live diagnostic preparation transaction " +
                "did not reach a complete legacy-active/fixed-candidate state.");
            if (!string.IsNullOrEmpty(reason))
            {
                report.AppendLine("Preparation detail: " + reason);
            }
            report.AppendLine(verdictLabel + ": FAIL");
            report.AppendLine();
        }

        private bool FinalizeP9Report(
            StringBuilder report,
            bool passed,
            string summary)
        {
            topologyCacheDiagnosticState = passed ? "Passed" : "Failed";
            topologyCacheDiagnosticSummary = summary ?? string.Empty;
            topologyCacheDiagnosticReport = report?.ToString() ?? string.Empty;
            if (!TryWriteLatestDiagnosticReport(
                    "LatestP9ComprehensiveValidation",
                    topologyCacheDiagnosticReport,
                    out topologyCacheDiagnosticReportPath,
                    out string writeError))
            {
                topologyCacheDiagnosticState = "Failed";
                topologyCacheDiagnosticSummary =
                    "The P9 report could not be written: " + writeError;
                Debug.LogError(
                    "[River Foam P9] " + topologyCacheDiagnosticSummary,
                    river);
                return false;
            }

            if (passed)
            {
                topologyCacheDiagnosticPassCount++;
                Debug.Log(
                    "[River Foam P9] PASS — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            else
            {
                Debug.LogError(
                    "[River Foam P9] FAIL — " +
                    topologyCacheDiagnosticReportPath,
                    river);
            }
            return passed;
        }
    }
}
#endif
