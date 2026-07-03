using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private bool IsProgressiveBirthTransferDebugActive =>
            river != null &&
            river.FoamDebugView ==
                StylizedRiverFoamDebugView.ProgressiveBirthTransfer;

        private void BeginProgressiveBirthSourceStep()
        {
            if (computeShader == null ||
                progressiveBirthSourceTexture == null ||
                progressiveBirthTransferDebugTexture == null ||
                fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                return;
            }

            if (activeProgressiveRibbonEventCount > 0 ||
                progressiveBirthSourceContainsData)
            {
                DispatchClear(
                    progressiveBirthSourceTexture,
                    0,
                    fieldWidth);
                progressiveBirthSourceContainsData = false;
            }

            if (IsProgressiveBirthTransferDebugActive ||
                activeProgressiveRibbonEventCount > 0)
            {
                DispatchClear(
                    progressiveBirthTransferDebugTexture,
                    0,
                    fieldWidth);
            }
        }

        private bool PaintProgressiveBirthSourceSegment(
            PendingInjection segment)
        {
            if (computeShader == null ||
                progressiveBirthSourceTexture == null ||
                paintProgressiveBirthSourceSegmentKernel < 0 ||
                metricBuffer == null ||
                boundaryTexture == null ||
                obstacleExclusionTexture == null ||
                !segment.SegmentShape)
            {
                return false;
            }

            float segmentPadding = Mathf.Max(
                segment.SegmentStartRadius,
                segment.SegmentEndRadius);
            float minimumGlobal = Mathf.Min(
                segment.SegmentStartGlobalDistance,
                segment.SegmentEndGlobalDistance) - segmentPadding;
            float maximumGlobal = Mathf.Max(
                segment.SegmentStartGlobalDistance,
                segment.SegmentEndGlobalDistance) + segmentPadding;
            int startX = Mathf.Clamp(
                GlobalDistanceToX(minimumGlobal) - 2,
                0,
                fieldWidth - 1);
            int endX = Mathf.Clamp(
                GlobalDistanceToX(maximumGlobal) + 2,
                0,
                fieldWidth - 1);
            int countX = endX - startX + 1;
            if (countX <= 0)
            {
                return false;
            }

            computeShader.SetInts(
                "_FoamDimensions",
                fieldWidth,
                fieldHeight);
            computeShader.SetFloat(
                "_FoamValidLength",
                validFieldLength);
            computeShader.SetFloat(
                "_FoamSimulationLength",
                simulationFieldLength);
            computeShader.SetInt("_FoamRangeStart", startX);
            computeShader.SetInt("_FoamRangeCount", countX);
            computeShader.SetFloat(
                "_FoamGlobalStart",
                river.Domain.GlobalDistanceMinimum);
            computeShader.SetFloat("_FoamFieldLength", fieldLength);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillSeed",
                segment.SourceFillSeed);
            computeShader.SetFloat(
                "_FoamInjectionSourceFillFeatureSize",
                segment.SourceFillFeatureSize);
            computeShader.SetFloat(
                "_FoamInjectionRemainingLife",
                segment.RemainingLife);
            computeShader.SetFloat(
                "_FoamInjectionIntegrity",
                segment.Integrity);
            computeShader.SetFloat(
                "_FoamInjectionPhase",
                segment.Phase);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartGlobalDistance",
                segment.SegmentStartGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAcrossNormalized",
                segment.SegmentStartAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartRadius",
                segment.SegmentStartRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentStartAmount",
                segment.SegmentStartSourceAmount);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndGlobalDistance",
                segment.SegmentEndGlobalDistance);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAcrossNormalized",
                segment.SegmentEndAcrossNormalized);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndRadius",
                segment.SegmentEndRadius);
            computeShader.SetFloat(
                "_FoamInjectionSegmentEndAmount",
                segment.SegmentEndSourceAmount);
            computeShader.SetBuffer(
                paintProgressiveBirthSourceSegmentKernel,
                "_FoamMetricRows",
                metricBuffer);
            computeShader.SetTexture(
                paintProgressiveBirthSourceSegmentKernel,
                "_FoamBoundary",
                boundaryTexture);
            computeShader.SetTexture(
                paintProgressiveBirthSourceSegmentKernel,
                "_FoamObstacleExclusionRead",
                obstacleExclusionTexture);
            computeShader.SetTexture(
                paintProgressiveBirthSourceSegmentKernel,
                "_FoamProgressiveBirthSourceWrite",
                progressiveBirthSourceTexture);
            Dispatch(
                paintProgressiveBirthSourceSegmentKernel,
                countX,
                fieldHeight);
            progressiveBirthSourceContainsData = true;
            return true;
        }
    }
}
