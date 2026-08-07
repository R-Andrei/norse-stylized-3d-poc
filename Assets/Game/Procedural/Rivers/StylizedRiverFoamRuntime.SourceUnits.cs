using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const int P7SourceUnitContractVersion = 1;
        private const float P7FixedProbePatchLengthMetres = 1.20f;
        private const float P7FixedProbePatchWidthMetres = 0.60f;
        private const float P7FixedProbeGapMetres = 0.45f;

        private readonly struct P7SourceDispatchRange
        {
            public P7SourceDispatchRange(
                int startX,
                int countX,
                int startY,
                int countY)
            {
                StartX = startX;
                CountX = countX;
                StartY = startY;
                CountY = countY;
            }

            public int StartX { get; }
            public int CountX { get; }
            public int StartY { get; }
            public int CountY { get; }
            public int EndX => StartX + CountX - 1;
            public int EndY => StartY + CountY - 1;
            public bool IsValid => CountX > 0 && CountY > 0;
        }

        private float ResolveSourceLateralMetres(
            float globalDistance,
            float acrossNormalized)
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 0f;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(globalDistance);
            float clamped = Mathf.Clamp(acrossNormalized, -1f, 1f);
            return clamped < 0f
                ? clamped * sample.LeftHalfWidth
                : clamped * sample.RightHalfWidth;
        }

        private float ResolveSourceAcrossNormalized(
            float globalDistance,
            float lateralMetres)
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 0f;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(globalDistance);
            return lateralMetres < 0f
                ? Mathf.Clamp(
                    lateralMetres / Mathf.Max(0.0001f, sample.LeftHalfWidth),
                    -1f,
                    0f)
                : Mathf.Clamp(
                    lateralMetres / Mathf.Max(0.0001f, sample.RightHalfWidth),
                    0f,
                    1f);
        }

        private float ResolveSourceLongitudinalSpacingMetres()
        {
            if (gridDescriptor.IsCreated && gridDescriptor.UsesFixedMetricLattice)
            {
                return Mathf.Max(0.005f, gridDescriptor.ResolvedDxMetres);
            }

            return Mathf.Max(
                0.005f,
                fieldLength / Mathf.Max(1, fieldWidth));
        }

        private float ResolveSourceLateralSpacingMetres(
            float globalDistance,
            float sideSign)
        {
            return ResolveSourceLateralSpacingMetres(
                gridDescriptor,
                fieldHeight,
                globalDistance,
                sideSign);
        }

        private float ResolveSourceLateralSpacingMetres(
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldHeight,
            float globalDistance,
            float sideSign)
        {
            if (descriptor.IsCreated && descriptor.UsesFixedMetricLattice)
            {
                return Mathf.Max(0.005f, descriptor.ResolvedDyMetres);
            }

            if (river == null || !river.Domain.IsValid)
            {
                return 0.005f;
            }

            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(globalDistance);
            float visibleHalfWidth = sample.GetVisibleHalfWidth(sideSign);
            return Mathf.Max(
                0.005f,
                visibleHalfWidth * 2f /
                Mathf.Max(1, resolvedFieldHeight));
        }

        private bool TryResolveSourceLongitudinalRange(
            float minimumGlobalDistance,
            float maximumGlobalDistance,
            int safetyCells,
            out int startX,
            out int countX)
        {
            return TryResolveSourceLongitudinalRange(
                gridDescriptor,
                fieldWidth,
                fieldLength,
                minimumGlobalDistance,
                maximumGlobalDistance,
                safetyCells,
                out startX,
                out countX);
        }

        private bool TryResolveSourceLongitudinalRange(
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldWidth,
            float resolvedFieldLength,
            float minimumGlobalDistance,
            float maximumGlobalDistance,
            int safetyCells,
            out int startX,
            out int countX)
        {
            startX = 0;
            countX = 0;
            if (river == null || !river.Domain.IsValid ||
                resolvedFieldWidth <= 0 || resolvedFieldLength <= 0.0001f)
            {
                return false;
            }

            float minimum = Mathf.Min(
                minimumGlobalDistance,
                maximumGlobalDistance);
            float maximum = Mathf.Max(
                minimumGlobalDistance,
                maximumGlobalDistance);
            int resolvedStart;
            int resolvedEnd;
            if (descriptor.IsCreated && descriptor.UsesFixedMetricLattice)
            {
                float minimumLocal =
                    minimum - river.Domain.GlobalDistanceMinimum;
                float maximumLocal =
                    maximum - river.Domain.GlobalDistanceMinimum;
                resolvedStart = Mathf.FloorToInt(
                    (minimumLocal - descriptor.FieldOrStripStartMetres) /
                    Mathf.Max(0.0001f, descriptor.ResolvedDxMetres));
                resolvedEnd = Mathf.CeilToInt(
                    (maximumLocal - descriptor.FieldOrStripStartMetres) /
                    Mathf.Max(0.0001f, descriptor.ResolvedDxMetres)) - 1;
            }
            else
            {
                // Exact legacy branch: same nearest-texel conversion and caller
                // safety expansion as the accepted pre-P7 implementation.
                float minimumLocal =
                    minimum - river.Domain.GlobalDistanceMinimum;
                float maximumLocal =
                    maximum - river.Domain.GlobalDistanceMinimum;
                resolvedStart = StylizedRiverFoamTopologyFieldSpace
                    .LocalDistanceToNearestTexel(
                        minimumLocal,
                        resolvedFieldWidth,
                        resolvedFieldLength);
                resolvedEnd = StylizedRiverFoamTopologyFieldSpace
                    .LocalDistanceToNearestTexel(
                        maximumLocal,
                        resolvedFieldWidth,
                        resolvedFieldLength);
            }

            int safety = Mathf.Max(0, safetyCells);
            startX = Mathf.Clamp(
                resolvedStart - safety,
                0,
                resolvedFieldWidth - 1);
            int endX = Mathf.Clamp(
                resolvedEnd + safety,
                0,
                resolvedFieldWidth - 1);
            countX = Mathf.Max(0, endX - startX + 1);
            return countX > 0;
        }

        private bool TryResolveSourceLateralRange(
            float centreGlobalDistance,
            float centreAcrossNormalized,
            float centreLateralMetres,
            float lateralExtentMetres,
            int safetyCells,
            out int startY,
            out int countY)
        {
            return TryResolveSourceLateralRange(
                gridDescriptor,
                fieldHeight,
                centreGlobalDistance,
                centreAcrossNormalized,
                centreLateralMetres,
                lateralExtentMetres,
                safetyCells,
                out startY,
                out countY);
        }

        private bool TryResolveSourceLateralRange(
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldHeight,
            float centreGlobalDistance,
            float centreAcrossNormalized,
            float centreLateralMetres,
            float lateralExtentMetres,
            int safetyCells,
            out int startY,
            out int countY)
        {
            startY = 0;
            countY = 0;
            if (resolvedFieldHeight <= 0)
            {
                return false;
            }

            int resolvedStart;
            int resolvedEnd;
            if (descriptor.IsCreated && descriptor.UsesFixedMetricLattice)
            {
                float minimumLateral =
                    centreLateralMetres - Mathf.Max(0f, lateralExtentMetres);
                float maximumLateral =
                    centreLateralMetres + Mathf.Max(0f, lateralExtentMetres);
                int minimumGlobalY =
                    descriptor.ResolveContainingGlobalY(minimumLateral);
                int maximumGlobalY =
                    descriptor.ResolveContainingGlobalY(maximumLateral);
                resolvedStart = minimumGlobalY - descriptor.GlobalYBase;
                resolvedEnd = maximumGlobalY - descriptor.GlobalYBase;
            }
            else
            {
                // Exact legacy branch retained from the pre-P7 free-water
                // source culling path.
                float centreAcross01 = Mathf.Clamp01(
                    centreAcrossNormalized * 0.5f + 0.5f);
                int centreY = Mathf.Clamp(
                    Mathf.RoundToInt(
                        centreAcross01 *
                        Mathf.Max(0, resolvedFieldHeight - 1)),
                    0,
                    resolvedFieldHeight - 1);
                float visibleHalfWidth = 1f;
                if (river != null && river.Domain.IsValid)
                {
                    StylizedRiverSplineSample sample =
                        river.Domain.SampleAtGlobalDistance(
                            centreGlobalDistance);
                    visibleHalfWidth = sample.GetVisibleHalfWidth(
                        centreAcrossNormalized < 0f ? -1f : 1f);
                }

                float normalizedPad = Mathf.Clamp01(
                    lateralExtentMetres /
                    Mathf.Max(0.10f, visibleHalfWidth));
                int padY = Mathf.CeilToInt(
                    normalizedPad * 0.5f * resolvedFieldHeight);
                resolvedStart = centreY - padY;
                resolvedEnd = centreY + padY;
            }

            int safety = Mathf.Max(0, safetyCells);
            startY = Mathf.Clamp(
                resolvedStart - safety,
                0,
                resolvedFieldHeight - 1);
            int endY = Mathf.Clamp(
                resolvedEnd + safety,
                0,
                resolvedFieldHeight - 1);
            countY = Mathf.Max(1, endY - startY + 1);
            return true;
        }

        private bool TryResolveAutomaticSourceDispatchRange(
            AutomaticFoamSourceEvent sourceEvent,
            FoamSourceEventGpuData gpuData,
            bool includePersistentDebugHead,
            out P7SourceDispatchRange range)
        {
            if (sourceEvent.Type != AutomaticFoamSourceEventType.ShoreRibbon)
            {
                return TryResolveAutomaticSourceDispatchRange(
                    sourceEvent,
                    out range);
            }

            range = default;
            int totalCellCount = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    sourceEvent.BodyLengthCells > 0f
                        ? sourceEvent.BodyLengthCells
                        : sourceEvent.RevealPathDistanceMetres));
            int previousHeadCell = gpuData.Deposit.z > 0.5f
                ? ResolveShoreRibbonHeadCellIndex(
                    sourceEvent,
                    gpuData.Deposit.y)
                : -1;
            int currentHeadCell = ResolveShoreRibbonHeadCellIndex(
                sourceEvent,
                gpuData.Header.z);
            int firstRequiredCell = previousHeadCell < currentHeadCell
                ? previousHeadCell + 1
                : currentHeadCell;
            if (!includePersistentDebugHead &&
                firstRequiredCell > currentHeadCell)
            {
                return false;
            }

            firstRequiredCell = Mathf.Clamp(
                firstRequiredCell,
                0,
                totalCellCount - 1);
            float pathLengthMetres = Mathf.Abs(
                sourceEvent.EndGlobalDistance -
                sourceEvent.StartGlobalDistance);
            float longitudinalCellSpacing = Mathf.Max(
                0.005f,
                pathLengthMetres / totalCellCount);
            float flowDirection = sourceEvent.EndGlobalDistance >=
                    sourceEvent.StartGlobalDistance
                ? 1f
                : -1f;
            float firstWorldGlobal = sourceEvent.StartGlobalDistance +
                flowDirection *
                (firstRequiredCell + 0.5f) * longitudinalCellSpacing;
            float lastWorldGlobal = sourceEvent.StartGlobalDistance +
                flowDirection *
                (currentHeadCell + 0.5f) * longitudinalCellSpacing;
            float firstStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    firstWorldGlobal);
            float lastStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    lastWorldGlobal);
            float halfCellPadding = longitudinalCellSpacing * 0.55f;
            if (!TryResolveSourceLongitudinalRange(
                    gridDescriptor,
                    fieldWidth,
                    fieldLength,
                    Mathf.Min(firstStorageGlobal, lastStorageGlobal) -
                        halfCellPadding,
                    Mathf.Max(firstStorageGlobal, lastStorageGlobal) +
                        halfCellPadding,
                    1,
                    out int startX,
                    out int countX))
            {
                return false;
            }

            range = new P7SourceDispatchRange(
                startX,
                countX,
                0,
                fieldHeight);
            return range.IsValid;
        }

        private bool TryResolveAutomaticSourceDispatchRange(
            AutomaticFoamSourceEvent sourceEvent,
            out P7SourceDispatchRange range)
        {
            return TryResolveAutomaticSourceDispatchRange(
                sourceEvent,
                gridDescriptor,
                fieldWidth,
                fieldHeight,
                fieldLength,
                out range);
        }

        private bool TryResolveAutomaticSourceDispatchRange(
            AutomaticFoamSourceEvent sourceEvent,
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldWidth,
            int resolvedFieldHeight,
            float resolvedFieldLength,
            out P7SourceDispatchRange range)
        {
            range = default;
            float startStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.StartGlobalDistance);
            float endStorageGlobal =
                WorldGlobalDistanceToFoamStorageGlobalDistance(
                    sourceEvent.EndGlobalDistance);
            float padding = Mathf.Max(
                sourceEvent.FeatherMetres * 2f,
                Mathf.Max(
                    sourceEvent.WidthMetres,
                    sourceEvent.InwardReachMetres) * 1.25f);
            bool arc =
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactSemiArc;
            if (arc)
            {
                float longitudinalCellSpacing =
                    descriptor.IsCreated &&
                    descriptor.UsesFixedMetricLattice
                        ? Mathf.Max(0.01f, descriptor.ResolvedDxMetres)
                        : resolvedFieldWidth > 0
                            ? Mathf.Max(
                                0.01f,
                                resolvedFieldLength / resolvedFieldWidth)
                            : 0.01f;
                padding = longitudinalCellSpacing * 2f;
            }
            else if (sourceEvent.Type ==
                AutomaticFoamSourceEventType.ObjectContactFleck)
            {
                padding = Mathf.Max(
                    padding,
                    Mathf.Max(
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres) +
                    sourceEvent.ObjectContactOffsetMetres +
                    sourceEvent.WidthMetres * 4f +
                    sourceEvent.FeatherMetres * 2f);
            }

            if (!TryResolveSourceLongitudinalRange(
                    descriptor,
                    resolvedFieldWidth,
                    resolvedFieldLength,
                    Mathf.Min(startStorageGlobal, endStorageGlobal) - padding,
                    Mathf.Max(startStorageGlobal, endStorageGlobal) + padding,
                    2,
                    out int startX,
                    out int countX))
            {
                return false;
            }

            int startY = 0;
            int countY = resolvedFieldHeight;
            if (descriptor.IsCreated && descriptor.UsesFixedMetricLattice)
            {
                if (!TryResolveFixedAutomaticSourceLateralRange(
                        sourceEvent,
                        descriptor,
                        resolvedFieldHeight,
                        startX,
                        countX,
                        out startY,
                        out countY))
                {
                    return false;
                }
            }
            else if (arc)
            {
                // Exact legacy Arc/Semi-Arc branch.
                float sourceLateralCellSpacing = Mathf.Max(
                    0.01f,
                    sourceEvent.ObjectSourceLateralCellSpacingMetres);
                float lateralExtent = Mathf.Max(
                    sourceLateralCellSpacing,
                    sourceEvent.LateralPaddingMetres);
                int centreY = StylizedRiverFoamTopologyFieldSpace
                    .SignedAcrossNormalizedToNearestTexel(
                        sourceEvent.CentreAcrossNormalized,
                        resolvedFieldHeight);
                int padY = Mathf.CeilToInt(
                    lateralExtent / sourceLateralCellSpacing) + 2;
                startY = Mathf.Clamp(
                    centreY - padY,
                    0,
                    resolvedFieldHeight - 1);
                int endY = Mathf.Clamp(
                    centreY + padY,
                    0,
                    resolvedFieldHeight - 1);
                countY = Mathf.Max(1, endY - startY + 1);
            }
            else if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterLaceConnector ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterTornFragment ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector)
            {
                // Exact legacy Free-Water branch.
                float centreWorldDistance =
                    (sourceEvent.StartGlobalDistance +
                     sourceEvent.EndGlobalDistance) * 0.5f;
                TryResolveSourceLateralRange(
                    descriptor,
                    resolvedFieldHeight,
                    centreWorldDistance,
                    sourceEvent.CentreAcrossNormalized,
                    sourceEvent.ObjectCentreAcrossMetres,
                    sourceEvent.LateralPaddingMetres,
                    3,
                    out startY,
                    out countY);
            }

            range = new P7SourceDispatchRange(
                startX,
                countX,
                startY,
                countY);
            return range.IsValid;
        }


        private bool TryResolveFixedAutomaticSourceLateralRange(
            AutomaticFoamSourceEvent sourceEvent,
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldHeight,
            int startX,
            int countX,
            out int startY,
            out int countY)
        {
            startY = 0;
            countY = 0;
            if (!descriptor.IsCreated || !descriptor.UsesFixedMetricLattice ||
                resolvedFieldHeight <= 0 || river == null ||
                !river.Domain.IsValid)
            {
                return false;
            }

            float minimumLateral;
            float maximumLateral;
            switch (sourceEvent.Type)
            {
                case AutomaticFoamSourceEventType.ShoreRibbon:
                case AutomaticFoamSourceEventType.InwardWash:
                {
                    minimumLateral = float.PositiveInfinity;
                    maximumLateral = float.NegativeInfinity;
                    float start = Mathf.Min(
                        sourceEvent.StartGlobalDistance,
                        sourceEvent.EndGlobalDistance);
                    float end = Mathf.Max(
                        sourceEvent.StartGlobalDistance,
                        sourceEvent.EndGlobalDistance);
                    float lateralCellSpacing = Mathf.Max(
                        0.005f,
                        descriptor.ResolvedDyMetres);
                    float inwardExtent = Mathf.Max(
                        (sourceEvent.ShoreInsetMetres +
                            sourceEvent.InwardReachMetres) * lateralCellSpacing,
                        (sourceEvent.ShoreInsetMetres +
                            sourceEvent.WidthMetres * 0.5f) * lateralCellSpacing);
                    inwardExtent +=
                        sourceEvent.FeatherMetres * 0.5f * lateralCellSpacing +
                        descriptor.ResolvedDyMetres;
                    int sampleCount = Mathf.Max(0, countX) + 2;
                    for (int sampleIndex = 0;
                         sampleIndex < sampleCount;
                         sampleIndex++)
                    {
                        float globalDistance;
                        if (sampleIndex == 0)
                        {
                            globalDistance = start;
                        }
                        else if (sampleIndex == sampleCount - 1)
                        {
                            globalDistance = end;
                        }
                        else
                        {
                            int localX = Mathf.Clamp(
                                startX + sampleIndex - 1,
                                0,
                                descriptor.ColumnCount - 1);
                            globalDistance = Mathf.Clamp(
                                river.Domain.GlobalDistanceMinimum +
                                descriptor.ResolveLocalDistanceAtColumnCentre(
                                    localX),
                                river.Domain.GlobalDistanceMinimum,
                                river.Domain.GlobalDistanceMaximum);
                        }

                        StylizedRiverSplineSample sample =
                            river.Domain.SampleAtGlobalDistance(globalDistance);
                        float shoreLateral = sourceEvent.SideSign < 0f
                            ? -sample.LeftHalfWidth
                            : sample.RightHalfWidth;
                        float outside = sourceEvent.FeatherMetres +
                            descriptor.ResolvedDyMetres;
                        float localMinimum = sourceEvent.SideSign < 0f
                            ? shoreLateral - outside
                            : shoreLateral - inwardExtent;
                        float localMaximum = sourceEvent.SideSign < 0f
                            ? shoreLateral + inwardExtent
                            : shoreLateral + outside;
                        minimumLateral = Mathf.Min(
                            minimumLateral,
                            localMinimum);
                        maximumLateral = Mathf.Max(
                            maximumLateral,
                            localMaximum);
                    }
                    break;
                }

                case AutomaticFoamSourceEventType.ObjectContactArc:
                case AutomaticFoamSourceEventType.ObjectContactSemiArc:
                {
                    float sourceSpacing = Mathf.Max(
                        descriptor.ResolvedDyMetres,
                        sourceEvent.ObjectSourceLateralCellSpacingMetres);
                    float extent = Mathf.Max(
                        sourceSpacing,
                        sourceEvent.LateralPaddingMetres) +
                        descriptor.ResolvedDyMetres * 2f;
                    minimumLateral =
                        sourceEvent.ObjectCentreAcrossMetres - extent;
                    maximumLateral =
                        sourceEvent.ObjectCentreAcrossMetres + extent;
                    break;
                }

                case AutomaticFoamSourceEventType.ObjectContactFleck:
                {
                    float extent = Mathf.Max(
                        sourceEvent.LateralPaddingMetres,
                        sourceEvent.ObjectAcrossHalfWidthMetres +
                        sourceEvent.ObjectContactOffsetMetres +
                        sourceEvent.WidthMetres * 4f +
                        sourceEvent.FeatherMetres * 2f) +
                        descriptor.ResolvedDyMetres * 2f;
                    minimumLateral =
                        sourceEvent.ObjectCentreAcrossMetres - extent;
                    maximumLateral =
                        sourceEvent.ObjectCentreAcrossMetres + extent;
                    break;
                }

                case AutomaticFoamSourceEventType.FreeWaterLaceConnector:
                case AutomaticFoamSourceEventType.FreeWaterTornFragment:
                case AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector:
                {
                    float extent = Mathf.Max(
                        sourceEvent.LateralPaddingMetres,
                        sourceEvent.WidthMetres +
                        sourceEvent.FeatherMetres * 2f) +
                        descriptor.ResolvedDyMetres * 2f;
                    minimumLateral =
                        sourceEvent.ObjectCentreAcrossMetres - extent;
                    maximumLateral =
                        sourceEvent.ObjectCentreAcrossMetres + extent;
                    break;
                }

                default:
                    return false;
            }

            float centreLateral =
                (minimumLateral + maximumLateral) * 0.5f;
            float lateralExtent =
                Mathf.Max(0f, (maximumLateral - minimumLateral) * 0.5f);
            float centreGlobalDistance =
                (sourceEvent.StartGlobalDistance +
                 sourceEvent.EndGlobalDistance) * 0.5f;
            return TryResolveSourceLateralRange(
                descriptor,
                resolvedFieldHeight,
                centreGlobalDistance,
                sourceEvent.CentreAcrossNormalized,
                centreLateral,
                lateralExtent,
                1,
                out startY,
                out countY);
        }

        private void ResolveP7LifeProbeLayout(
            float distanceNormalized,
            float acrossNormalized,
            out int centreX,
            out int centreY,
            out int patchWidth,
            out int patchHeight,
            out int gap)
        {
            ResolveP7LifeProbeLayout(
                gridDescriptor,
                fieldWidth,
                fieldHeight,
                distanceNormalized,
                acrossNormalized,
                out centreX,
                out centreY,
                out patchWidth,
                out patchHeight,
                out gap);
        }

        private void ResolveP7LifeProbeLayout(
            StylizedRiverFoamGridDescriptor descriptor,
            int resolvedFieldWidth,
            int resolvedFieldHeight,
            float distanceNormalized,
            float acrossNormalized,
            out int centreX,
            out int centreY,
            out int patchWidth,
            out int patchHeight,
            out int gap)
        {
            if (descriptor.IsCreated && descriptor.UsesFixedMetricLattice)
            {
                patchWidth = Mathf.Max(
                    3,
                    Mathf.CeilToInt(
                        P7FixedProbePatchLengthMetres /
                        Mathf.Max(0.0001f, descriptor.ResolvedDxMetres)));
                patchHeight = Mathf.Max(
                    3,
                    Mathf.CeilToInt(
                        P7FixedProbePatchWidthMetres /
                        Mathf.Max(0.0001f, descriptor.ResolvedDyMetres)));
                gap = Mathf.Max(
                    2,
                    Mathf.CeilToInt(
                        P7FixedProbeGapMetres /
                        Mathf.Max(0.0001f, descriptor.ResolvedDxMetres)));

                float globalDistance = Mathf.Lerp(
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum,
                    Mathf.Clamp01(distanceNormalized));
                float localDistance =
                    globalDistance - river.Domain.GlobalDistanceMinimum;
                centreX = Mathf.RoundToInt(
                    (localDistance - descriptor.FieldOrStripStartMetres) /
                    Mathf.Max(0.0001f, descriptor.ResolvedDxMetres) -
                    0.5f);
                float lateralMetres = ResolveSourceLateralMetres(
                    globalDistance,
                    acrossNormalized);
                int globalY = descriptor.ResolveNearestGlobalY(lateralMetres);
                centreY = globalY - descriptor.GlobalYBase;
            }
            else
            {
                // Exact accepted legacy percentage/cell layout.
                patchWidth = Mathf.Clamp(
                    Mathf.RoundToInt(resolvedFieldWidth * 0.035f),
                    3,
                    10);
                patchHeight = Mathf.Clamp(
                    Mathf.RoundToInt(resolvedFieldHeight * 0.075f),
                    3,
                    10);
                gap = Mathf.Clamp(
                    Mathf.RoundToInt(resolvedFieldWidth * 0.018f),
                    2,
                    8);
                centreX = Mathf.RoundToInt(
                    Mathf.Clamp01(distanceNormalized) *
                    Mathf.Max(0, resolvedFieldWidth - 1));
                centreY = Mathf.RoundToInt(
                    Mathf.Clamp01(acrossNormalized * 0.5f + 0.5f) *
                    Mathf.Max(0, resolvedFieldHeight - 1));
            }

            int groupHalfWidth = patchWidth + gap + patchWidth / 2 + 2;
            centreX = Mathf.Clamp(
                centreX,
                groupHalfWidth,
                Mathf.Max(
                    groupHalfWidth,
                    resolvedFieldWidth - 1 - groupHalfWidth));
            int halfHeight = Mathf.Max(1, patchHeight / 2);
            centreY = Mathf.Clamp(
                centreY,
                halfHeight + 1,
                Mathf.Max(
                    halfHeight + 1,
                    resolvedFieldHeight - halfHeight - 2));
        }
    }
}
