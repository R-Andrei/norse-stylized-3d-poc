using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const int FoamChipAdmissionThreadsPerGroup = 64;
        private const int FoamChipAdmissionLongitudinalGuardCells = 3;
        private const int FoamChipAdmissionLateralGuardCells = 6;
        private const int FoamChipAdmissionMaximumRecordCount = 4_000_000;

        private static readonly int FoamChipApplicationModeId =
            Shader.PropertyToID("_FoamChipApplicationMode");
        private static readonly int FoamChipStraddleAdmissionAvailableId =
            Shader.PropertyToID("_FoamChipStraddleAdmissionAvailable");
        private static readonly int FoamChipStraddleAdmissionOriginId =
            Shader.PropertyToID("_FoamChipStraddleAdmissionOrigin");
        private static readonly int FoamChipStraddleAdmissionDimensionsId =
            Shader.PropertyToID("_FoamChipStraddleAdmissionDimensions");
        private static readonly int FoamChipStraddleAdmissionTextureId =
            Shader.PropertyToID("_FoamChipStraddleAdmission");

        private int buildFoamChipStraddleAdmissionKernel = -1;
        private RenderTexture foamChipStraddleAdmissionTexture;
        private Vector2Int foamChipStraddleAdmissionOrigin;
        private Vector2Int foamChipStraddleAdmissionDimensions = Vector2Int.one;
        private bool foamChipStraddleAdmissionAvailable;
        private bool foamChipStraddleAdmissionHasWrittenData;
        private bool foamChipStraddleCapacityWarningReported;
        private bool foamChipStraddleFormatWarningReported;
        private double nextFoamChipStraddleRefreshAt;

        private bool IsFoamChipStraddleRouteRequested =>
            river != null &&
            river.FoamPresenceFootprintMode ==
                StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude &&
            river.FoamChipApplicationMode ==
                StylizedRiverFoamChipApplicationMode.CandidateStraddle &&
            river.FoamChipActivation > 0.0001f;

        private void BindFoamChipStraddleAdmission(
            MaterialPropertyBlock targetPropertyBlock,
            bool enabled)
        {
            if (targetPropertyBlock == null)
            {
                return;
            }

            StylizedRiverFoamChipApplicationMode applicationMode =
                river != null
                    ? river.FoamChipApplicationMode
                    : StylizedRiverFoamChipApplicationMode.RenderedEdgeBand;
            targetPropertyBlock.SetFloat(
                FoamChipApplicationModeId,
                (float)applicationMode);
            targetPropertyBlock.SetFloat(
                FoamChipStraddleAdmissionAvailableId,
                enabled && foamChipStraddleAdmissionAvailable
                    ? 1f
                    : 0f);
            targetPropertyBlock.SetVector(
                FoamChipStraddleAdmissionOriginId,
                new Vector4(
                    foamChipStraddleAdmissionOrigin.x,
                    foamChipStraddleAdmissionOrigin.y,
                    0f,
                    0f));
            targetPropertyBlock.SetVector(
                FoamChipStraddleAdmissionDimensionsId,
                new Vector4(
                    Mathf.Max(1, foamChipStraddleAdmissionDimensions.x),
                    Mathf.Max(1, foamChipStraddleAdmissionDimensions.y),
                    0f,
                    0f));
            targetPropertyBlock.SetTexture(
                FoamChipStraddleAdmissionTextureId,
                enabled && foamChipStraddleAdmissionTexture != null
                    ? foamChipStraddleAdmissionTexture
                    : Texture2D.blackTexture);
        }

        private void UpdateFoamChipStraddleAdmission(bool force)
        {
            foamChipStraddleAdmissionAvailable = false;
            if (!IsFoamChipStraddleRouteRequested)
            {
                foamChipStraddleAdmissionHasWrittenData = false;
                nextFoamChipStraddleRefreshAt = 0.0;
                return;
            }

            if (computeShader == null ||
                buildFoamChipStraddleAdmissionKernel < 0 ||
                previousState == null ||
                currentState == null ||
                !gridDescriptor.IsCreated)
            {
                return;
            }

            double now = Time.timeAsDouble;
            if (!force && now < nextFoamChipStraddleRefreshAt)
            {
                foamChipStraddleAdmissionAvailable =
                    foamChipStraddleAdmissionHasWrittenData &&
                    foamChipStraddleAdmissionTexture != null &&
                    foamChipStraddleAdmissionTexture.IsCreated();
                return;
            }

            float spacing = Mathf.Max(
                0.10f,
                river.FoamChipCandidateSpacing);
            float evolutionTime = Mathf.Max(0f, Time.time);
            float downstreamShift = Mathf.Max(
                0f,
                river.FoamChipFieldSpeed) * evolutionTime;
            float globalMinimum = allocatedGlobalStart +
                gridDescriptor.AllocatedLocalDistanceMinimumMetres;
            float globalMaximum = allocatedGlobalStart +
                gridDescriptor.ValidLocalDistanceMaximumMetres;
            int minimumX = Mathf.FloorToInt(
                (globalMinimum - downstreamShift) / spacing) -
                FoamChipAdmissionLongitudinalGuardCells;
            int maximumX = Mathf.FloorToInt(
                (globalMaximum - downstreamShift) / spacing) +
                FoamChipAdmissionLongitudinalGuardCells;
            int minimumY = Mathf.FloorToInt(
                gridDescriptor.RepresentedLateralMinimumMetres / spacing) -
                FoamChipAdmissionLateralGuardCells;
            int maximumY = Mathf.FloorToInt(
                gridDescriptor.RepresentedLateralMaximumMetres / spacing) +
                FoamChipAdmissionLateralGuardCells;
            int width = Mathf.Max(1, maximumX - minimumX + 1);
            int height = Mathf.Max(1, maximumY - minimumY + 1);
            long requestedCountLong = (long)width * height;
            if (requestedCountLong <= 0L ||
                requestedCountLong > FoamChipAdmissionMaximumRecordCount)
            {
                if (!foamChipStraddleCapacityWarningReported)
                {
                    Debug.LogWarning(
                        $"Candidate Straddle on '{name}' requested " +
                        $"{requestedCountLong:N0} admission records. The " +
                        $"experimental route is falling back to Rendered " +
                        $"Edge Band because the safety ceiling is " +
                        $"{FoamChipAdmissionMaximumRecordCount:N0}.",
                        this);
                    foamChipStraddleCapacityWarningReported = true;
                }

                return;
            }

            foamChipStraddleCapacityWarningReported = false;
            int requestedCount = (int)requestedCountLong;
            Vector2Int requestedOrigin = new Vector2Int(
                minimumX,
                minimumY);
            Vector2Int requestedDimensions = new Vector2Int(
                width,
                height);
            bool historyValid =
                foamChipStraddleAdmissionHasWrittenData &&
                foamChipStraddleAdmissionTexture != null &&
                foamChipStraddleAdmissionTexture.IsCreated() &&
                foamChipStraddleAdmissionOrigin == requestedOrigin &&
                foamChipStraddleAdmissionDimensions == requestedDimensions;
            EnsureFoamChipStraddleAdmissionCapacity(width, height);
            if (foamChipStraddleAdmissionTexture == null)
            {
                return;
            }

            foamChipStraddleAdmissionOrigin = requestedOrigin;
            foamChipStraddleAdmissionDimensions = requestedDimensions;

            ConfigureGridDescriptorComputeParameters();
            computeShader.SetInts("_FoamDimensions", fieldWidth, fieldHeight);
            computeShader.SetFloat("_FoamGlobalStart", allocatedGlobalStart);
            computeShader.SetFloat(
                "_FoamFieldLength",
                Mathf.Max(0.001f, fieldLength));
            computeShader.SetInts(
                "_FoamChipAdmissionDimensions",
                width,
                height);
            computeShader.SetInts(
                "_FoamChipAdmissionOrigin",
                minimumX,
                minimumY);
            computeShader.SetInt(
                "_FoamChipAdmissionRecordCount",
                requestedCount);
            computeShader.SetInt(
                "_FoamChipAdmissionHistoryValid",
                historyValid ? 1 : 0);
            computeShader.SetFloat(
                "_FoamChipAdmissionInterpolation",
                simulationInterpolation);
            computeShader.SetFloat(
                "_FoamChipAdmissionEvolutionTime",
                evolutionTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionCandidateSpacing",
                river.FoamChipCandidateSpacing);
            computeShader.SetFloat(
                "_FoamChipAdmissionActivation",
                river.FoamChipActivation);
            computeShader.SetFloat(
                "_FoamChipAdmissionSize",
                river.FoamChipSize);
            computeShader.SetFloat(
                "_FoamChipAdmissionIrregularity",
                river.FoamChipIrregularity);
            computeShader.SetFloat(
                "_FoamChipAdmissionMaximumViewScale",
                river.FoamChipMaximumViewScale);
            computeShader.SetFloat(
                "_FoamChipAdmissionFieldSpeed",
                river.FoamChipFieldSpeed);
            computeShader.SetFloat(
                "_FoamChipAdmissionFormationTime",
                river.FoamChipFormationTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionStableTime",
                river.FoamChipStableTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionDissolveTime",
                river.FoamChipDissolveTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionDormantTime",
                river.FoamChipDormantTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionLateralMotionAmount",
                river.FoamChipLateralMotionAmount);
            computeShader.SetFloat(
                "_FoamChipAdmissionLateralMotionSpeed",
                river.FoamChipLateralMotionSpeed);
            computeShader.SetFloat(
                "_FoamChipAdmissionRotationAmountDegrees",
                river.FoamChipRotationAmountDegrees);
            computeShader.SetFloat(
                "_FoamChipAdmissionRotationSpeed",
                river.FoamChipRotationSpeed);
            computeShader.SetFloat(
                "_FoamChipAdmissionSizePulseAmount",
                river.FoamChipSizePulseAmount);
            computeShader.SetFloat(
                "_FoamChipAdmissionSizePulseSpeed",
                river.FoamChipSizePulseSpeed);
            computeShader.SetFloat(
                "_FoamChipAdmissionShapeChangeAmount",
                river.FoamChipShapeChangeAmount);
            computeShader.SetFloat(
                "_FoamChipAdmissionShapeChangeSpeed",
                river.FoamChipShapeChangeSpeed);
            computeShader.SetFloat(
                "_FoamChipAdmissionShapeTransitionTime",
                river.FoamChipShapeTransitionTime);
            computeShader.SetFloat(
                "_FoamChipAdmissionSharpness",
                MaterialContourSharpness);
            computeShader.SetFloat(
                "_FoamChipAdmissionFinalVisibilityMode",
                (float)river.FoamFinalVisibilityMode);
            computeShader.SetFloat(
                "_FoamChipAdmissionStrandStrength",
                river.FoamStrandStrength);
            computeShader.SetFloat(
                "_FoamChipAdmissionStrandScale",
                river.FoamStrandScale);
            computeShader.SetFloat(
                "_FoamChipAdmissionStrandDensity",
                river.FoamStrandDensity);
            computeShader.SetFloat(
                "_FoamChipAdmissionStrandReach",
                river.FoamStrandReach);
            float supportFootprint = Mathf.Clamp(
                Mathf.Min(
                    Mathf.Max(0.0001f, gridDescriptor.ResolvedDxMetres),
                    Mathf.Max(0.0001f, gridDescriptor.ResolvedDyMetres)) *
                    0.25f,
                0.015f,
                0.05f);
            computeShader.SetFloat(
                "_FoamChipAdmissionSupportFootprintMetres",
                supportFootprint);
            computeShader.SetTexture(
                buildFoamChipStraddleAdmissionKernel,
                "_FoamChipAdmissionPreviousStateRead",
                previousState);
            computeShader.SetTexture(
                buildFoamChipStraddleAdmissionKernel,
                "_FoamChipAdmissionCurrentStateRead",
                currentState);
            computeShader.SetTexture(
                buildFoamChipStraddleAdmissionKernel,
                "_FoamChipStraddleAdmissionWrite",
                foamChipStraddleAdmissionTexture);
            DispatchOneDimensional(
                buildFoamChipStraddleAdmissionKernel,
                requestedCount,
                FoamChipAdmissionThreadsPerGroup);

            foamChipStraddleAdmissionHasWrittenData = true;
            foamChipStraddleAdmissionAvailable = true;
            nextFoamChipStraddleRefreshAt = now +
                1.0 / Mathf.Max(1f, river.FoamChipStraddleRefreshRate);
        }

        private void EnsureFoamChipStraddleAdmissionCapacity(
            int width,
            int height)
        {
            if (!SystemInfo.SupportsRenderTextureFormat(
                    RenderTextureFormat.RFloat))
            {
                if (!foamChipStraddleFormatWarningReported)
                {
                    Debug.LogWarning(
                        $"Candidate Straddle on '{name}' requires an RFloat " +
                        "random-write admission texture. This platform does " +
                        "not expose that format, so Chipping is falling back " +
                        "to Rendered Edge Band.",
                        this);
                    foamChipStraddleFormatWarningReported = true;
                }

                ReleaseTexture(ref foamChipStraddleAdmissionTexture);
                return;
            }

            foamChipStraddleFormatWarningReported = false;
            int safeWidth = Mathf.Max(1, width);
            int safeHeight = Mathf.Max(1, height);
            if (foamChipStraddleAdmissionTexture != null &&
                foamChipStraddleAdmissionTexture.width == safeWidth &&
                foamChipStraddleAdmissionTexture.height == safeHeight &&
                foamChipStraddleAdmissionTexture.IsCreated())
            {
                return;
            }

            ReleaseTexture(ref foamChipStraddleAdmissionTexture);
            foamChipStraddleAdmissionTexture = new RenderTexture(
                safeWidth,
                safeHeight,
                0,
                RenderTextureFormat.RFloat,
                RenderTextureReadWrite.Linear)
            {
                name = $"{name}_FoamChipStraddleAdmission",
                enableRandomWrite = true,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            foamChipStraddleAdmissionTexture.Create();
            if (!foamChipStraddleAdmissionTexture.IsCreated())
            {
                if (!foamChipStraddleFormatWarningReported)
                {
                    Debug.LogWarning(
                        $"Candidate Straddle on '{name}' could not create " +
                        "its RFloat random-write admission texture. " +
                        "Chipping is falling back to Rendered Edge Band.",
                        this);
                    foamChipStraddleFormatWarningReported = true;
                }

                ReleaseTexture(ref foamChipStraddleAdmissionTexture);
                return;
            }

            foamChipStraddleAdmissionAvailable = false;
            foamChipStraddleAdmissionHasWrittenData = false;
        }

        private void ReleaseFoamChipStraddleAdmissionResources()
        {
            ReleaseTexture(ref foamChipStraddleAdmissionTexture);
            foamChipStraddleAdmissionOrigin = Vector2Int.zero;
            foamChipStraddleAdmissionDimensions = Vector2Int.one;
            foamChipStraddleAdmissionAvailable = false;
            foamChipStraddleAdmissionHasWrittenData = false;
            foamChipStraddleCapacityWarningReported = false;
            foamChipStraddleFormatWarningReported = false;
            nextFoamChipStraddleRefreshAt = 0.0;
        }
    }
}
