using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public enum StylizedRiverFoamMajorCandidatePreviewStage
    {
        RawField,
        Thresholded,
        Cleaned,
        FinalSupport
    }

    public enum StylizedRiverFoamMajorCandidateRejectionReason
    {
        None,
        Empty,
        AreaTooSmall,
        AreaTooLarge,
        TouchesStorageBoundary,
        Disconnected,
        ContainsHole,
        NeckTooNarrow,
        TooCompact,
        TooFragmented
    }

    /// <summary>
    /// Topology-only settings for one local field-first Major candidate.
    /// These are generator inputs rather than river authoring controls.
    /// </summary>
    public readonly struct StylizedRiverFoamMajorCandidateSettings
    {
        public StylizedRiverFoamMajorCandidateSettings(
            int resolution,
            int emptyMargin,
            int maximumAttempts,
            float targetOccupancy,
            float warpStrength,
            float minimumAreaFraction,
            float maximumAreaFraction,
            int minimumNeckWidthCells,
            float minimumCompactness,
            float maximumCompactness,
            float supportFeatherCells)
        {
            Resolution = Mathf.Max(32, resolution);
            EmptyMargin = Mathf.Clamp(
                emptyMargin,
                3,
                Mathf.Max(3, Resolution / 4));
            MaximumAttempts = Mathf.Clamp(maximumAttempts, 1, 16);
            TargetOccupancy = Mathf.Clamp(targetOccupancy, 0.12f, 0.70f);
            WarpStrength = Mathf.Clamp(warpStrength, 0f, 0.45f);
            MinimumAreaFraction = Mathf.Clamp01(minimumAreaFraction);
            MaximumAreaFraction = Mathf.Clamp(
                maximumAreaFraction,
                MinimumAreaFraction,
                1f);
            MinimumNeckWidthCells = Mathf.Max(1, minimumNeckWidthCells);
            MinimumCompactness = Mathf.Clamp01(minimumCompactness);
            MaximumCompactness = Mathf.Clamp(
                maximumCompactness,
                MinimumCompactness,
                1f);
            SupportFeatherCells = Mathf.Max(0.5f, supportFeatherCells);
        }

        public int Resolution { get; }
        public int EmptyMargin { get; }
        public int MaximumAttempts { get; }
        public float TargetOccupancy { get; }
        public float WarpStrength { get; }
        public float MinimumAreaFraction { get; }
        public float MaximumAreaFraction { get; }
        public int MinimumNeckWidthCells { get; }
        public float MinimumCompactness { get; }
        public float MaximumCompactness { get; }
        public float SupportFeatherCells { get; }
    }

    /// <summary>
    /// Immutable output from one bounded deterministic Major-candidate build.
    /// The four retained stages keep the candidate generator immediately
    /// inspectable. Whole-river placement consumes only the accepted support
    /// mask and does not alter these candidate-local proof stages.
    /// </summary>
    public sealed class StylizedRiverFoamMajorCandidate
    {
        private readonly float[] rawField;
        private readonly byte[] thresholdedMask;
        private readonly byte[] cleanedMask;
        private readonly float[] finalSupport;

        internal StylizedRiverFoamMajorCandidate(
            int seed,
            int attemptIndex,
            int resolution,
            int emptyMargin,
            bool accepted,
            StylizedRiverFoamMajorCandidateRejectionReason rejectionReason,
            int occupiedCellCount,
            float occupiedAreaFraction,
            int minimumNeckWidthCells,
            float compactness,
            float[] rawField,
            byte[] thresholdedMask,
            byte[] cleanedMask,
            float[] finalSupport)
        {
            Seed = seed;
            AttemptIndex = attemptIndex;
            Resolution = resolution;
            EmptyMargin = emptyMargin;
            Accepted = accepted;
            RejectionReason = rejectionReason;
            OccupiedCellCount = occupiedCellCount;
            OccupiedAreaFraction = occupiedAreaFraction;
            MinimumNeckWidthCells = minimumNeckWidthCells;
            Compactness = compactness;
            this.rawField = rawField ?? throw new ArgumentNullException(nameof(rawField));
            this.thresholdedMask = thresholdedMask ??
                throw new ArgumentNullException(nameof(thresholdedMask));
            this.cleanedMask = cleanedMask ??
                throw new ArgumentNullException(nameof(cleanedMask));
            this.finalSupport = finalSupport ??
                throw new ArgumentNullException(nameof(finalSupport));
        }

        public int Seed { get; }
        public int AttemptIndex { get; }
        public int Resolution { get; }
        public int EmptyMargin { get; }
        public bool Accepted { get; }
        public StylizedRiverFoamMajorCandidateRejectionReason RejectionReason { get; }
        public int OccupiedCellCount { get; }
        public float OccupiedAreaFraction { get; }
        public int MinimumNeckWidthCells { get; }
        public float Compactness { get; }

        internal void CopyCleanedMaskTo(byte[] destination)
        {
            if (destination == null || destination.Length < cleanedMask.Length)
            {
                throw new ArgumentException(
                    $"Major candidate mask copy requires at least {cleanedMask.Length} cells.",
                    nameof(destination));
            }

            Buffer.BlockCopy(
                cleanedMask,
                0,
                destination,
                0,
                cleanedMask.Length);
        }

        internal void CopyFinalSupportTo(float[] destination)
        {
            if (destination == null || destination.Length < finalSupport.Length)
            {
                throw new ArgumentException(
                    $"Major candidate support copy requires at least {finalSupport.Length} cells.",
                    nameof(destination));
            }

            Buffer.BlockCopy(
                finalSupport,
                0,
                destination,
                0,
                finalSupport.Length * sizeof(float));
        }

        internal float SampleFinalSupportBilinear(float x, float y)
        {
            if (x < -0.5f || x > Resolution - 0.5f ||
                y < -0.5f || y > Resolution - 0.5f)
            {
                return 0f;
            }

            float clampedX = Mathf.Clamp(x - 0.5f, 0f, Resolution - 1f);
            float clampedY = Mathf.Clamp(y - 0.5f, 0f, Resolution - 1f);
            int x0 = Mathf.FloorToInt(clampedX);
            int y0 = Mathf.FloorToInt(clampedY);
            int x1 = Mathf.Min(Resolution - 1, x0 + 1);
            int y1 = Mathf.Min(Resolution - 1, y0 + 1);
            float tx = clampedX - x0;
            float ty = clampedY - y0;
            float a = finalSupport[x0 + y0 * Resolution];
            float b = finalSupport[x1 + y0 * Resolution];
            float c = finalSupport[x0 + y1 * Resolution];
            float d = finalSupport[x1 + y1 * Resolution];
            return Mathf.Lerp(
                Mathf.Lerp(a, b, tx),
                Mathf.Lerp(c, d, tx),
                ty);
        }

        public void FillPreview(
            StylizedRiverFoamMajorCandidatePreviewStage stage,
            Color32[] destination)
        {
            int cellCount = Resolution * Resolution;
            if (destination == null || destination.Length < cellCount)
            {
                throw new ArgumentException(
                    $"Major candidate preview requires at least {cellCount} pixels.",
                    nameof(destination));
            }

            for (int index = 0; index < cellCount; index++)
            {
                float value = stage switch
                {
                    StylizedRiverFoamMajorCandidatePreviewStage.RawField =>
                        rawField[index],
                    StylizedRiverFoamMajorCandidatePreviewStage.Thresholded =>
                        thresholdedMask[index] != 0 ? 1f : 0f,
                    StylizedRiverFoamMajorCandidatePreviewStage.Cleaned =>
                        cleanedMask[index] != 0 ? 1f : 0f,
                    StylizedRiverFoamMajorCandidatePreviewStage.FinalSupport =>
                        finalSupport[index],
                    _ => 0f
                };

                byte channel = (byte)Mathf.RoundToInt(
                    Mathf.Clamp01(value) * 255f);
                destination[index] = new Color32(
                    channel,
                    channel,
                    channel,
                    255);
            }
        }
    }
}
