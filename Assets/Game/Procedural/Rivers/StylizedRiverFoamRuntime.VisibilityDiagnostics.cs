using System;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private const int VisibilityDiagnosticChannelCount = 4;
        private const float VisibilityDiagnosticAuthorityCoverage = 0.02f;

        private sealed class FoamVisibilityDiagnosticCapture
        {
            public int Generation;
            public int Width;
            public int Height;
            public float Interpolation;
            public StylizedRiverFoamTransportScheme TransportScheme;
            public StylizedRiverFinalFoamVisibilityMode VisibilityMode;
            public StylizedRiverFoamPresenceFootprintMode PresenceMode;
            public bool StateHeld;
            public StylizedRiverFoamGridDescriptor GridDescriptor;
            public ushort[] PreviousData;
            public ushort[] CurrentData;
            public bool PreviousComplete;
            public bool CurrentComplete;
        }

        private bool foamVisibilityDiagnosticReadbackPending;
        private int foamVisibilityDiagnosticGeneration;
        private string foamVisibilityDiagnosticReport = string.Empty;
        private FoamVisibilityDiagnosticCapture foamVisibilityDiagnosticCapture;

        public bool FoamVisibilityDiagnosticReadbackPending =>
            foamVisibilityDiagnosticReadbackPending;

        public string FoamVisibilityDiagnosticReport =>
            foamVisibilityDiagnosticReport;

        public void CaptureFoamVisibilityDiagnosticReport()
        {
            if (foamVisibilityDiagnosticReadbackPending)
            {
                return;
            }

            if (!Application.isPlaying)
            {
                foamVisibilityDiagnosticReport =
                    "Coverage diagnostic requires Play Mode.";
                return;
            }

            if (river == null ||
                previousState == null ||
                currentState == null ||
                !previousState.IsCreated() ||
                !currentState.IsCreated() ||
                fieldWidth <= 0 ||
                fieldHeight <= 0)
            {
                foamVisibilityDiagnosticReport =
                    "Coverage diagnostic unavailable: committed Foam state " +
                    "resources are not ready.";
                return;
            }

            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                foamVisibilityDiagnosticReport =
                    "Coverage diagnostic unavailable: Async GPU Readback is " +
                    "not supported on this platform.";
                return;
            }

            int generation = ++foamVisibilityDiagnosticGeneration;
            FoamVisibilityDiagnosticCapture capture =
                new FoamVisibilityDiagnosticCapture
                {
                    Generation = generation,
                    Width = fieldWidth,
                    Height = fieldHeight,
                    Interpolation = Mathf.Clamp01(simulationInterpolation),
                    TransportScheme = river.FoamTransportScheme,
                    VisibilityMode = river.FoamFinalVisibilityMode,
                    PresenceMode = river.FoamPresenceFootprintMode,
                    StateHeld = river.FoamStateHeld,
                    GridDescriptor = gridDescriptor
                };

            foamVisibilityDiagnosticCapture = capture;
            foamVisibilityDiagnosticReadbackPending = true;
            foamVisibilityDiagnosticReport = string.Empty;

            RenderTexture requestedPrevious = previousState;
            RenderTexture requestedCurrent = currentState;
            AsyncGPUReadback.Request(
                requestedPrevious,
                0,
                request => CompleteFoamVisibilityDiagnosticReadback(
                    capture,
                    true,
                    request));
            AsyncGPUReadback.Request(
                requestedCurrent,
                0,
                request => CompleteFoamVisibilityDiagnosticReadback(
                    capture,
                    false,
                    request));
        }

        private void CompleteFoamVisibilityDiagnosticReadback(
            FoamVisibilityDiagnosticCapture capture,
            bool previous,
            AsyncGPUReadbackRequest request)
        {
            if (this == null ||
                capture == null ||
                capture.Generation != foamVisibilityDiagnosticGeneration ||
                foamVisibilityDiagnosticCapture != capture)
            {
                return;
            }

            if (request.hasError)
            {
                FailFoamVisibilityDiagnosticCapture(
                    capture,
                    previous
                        ? "previous committed state readback failed"
                        : "current committed state readback failed");
                return;
            }

            var data = request.GetData<ushort>();
            int expectedLength = capture.Width * capture.Height *
                VisibilityDiagnosticChannelCount;
            if (data.Length != expectedLength)
            {
                FailFoamVisibilityDiagnosticCapture(
                    capture,
                    $"unexpected ARGBHalf payload length {data.Length:N0}; " +
                    $"expected {expectedLength:N0}");
                return;
            }

            ushort[] copy = new ushort[data.Length];
            for (int index = 0; index < data.Length; index++)
            {
                copy[index] = data[index];
            }

            if (previous)
            {
                capture.PreviousData = copy;
                capture.PreviousComplete = true;
            }
            else
            {
                capture.CurrentData = copy;
                capture.CurrentComplete = true;
            }

            if (!capture.PreviousComplete || !capture.CurrentComplete)
            {
                return;
            }

            foamVisibilityDiagnosticReport =
                BuildFoamVisibilityDiagnosticReport(capture);
            foamVisibilityDiagnosticReadbackPending = false;
            foamVisibilityDiagnosticCapture = null;
        }

        private void FailFoamVisibilityDiagnosticCapture(
            FoamVisibilityDiagnosticCapture capture,
            string reason)
        {
            if (capture == null ||
                capture.Generation != foamVisibilityDiagnosticGeneration)
            {
                return;
            }

            foamVisibilityDiagnosticReport =
                "Coverage diagnostic readback failed: " + reason + ".";
            foamVisibilityDiagnosticReadbackPending = false;
            foamVisibilityDiagnosticCapture = null;
            foamVisibilityDiagnosticGeneration++;
        }

        private static string BuildFoamVisibilityDiagnosticReport(
            FoamVisibilityDiagnosticCapture capture)
        {
            string[] bucketLabels =
            {
                "0 < C < 0.02",
                "0.02 <= C < 0.10",
                "0.10 <= C < 0.20",
                "0.20 <= C < 0.30",
                "0.30 <= C < 0.40",
                "0.40 <= C <= 1.00"
            };
            int bucketCount = bucketLabels.Length;
            long[] cells = new long[bucketCount];
            long[] baseSupportedCells = new long[bucketCount];
            double[] coverageSums = new double[bucketCount];
            double[] amountSums = new double[bucketCount];
            double[] lifeAmountSums = new double[bucketCount];
            double[] baseSums = new double[bucketCount];

            long totalCells = (long)capture.Width * capture.Height;
            long emptyCells = 0;
            long materialCells = 0;
            long authorityCells = 0;
            double totalCoverage = 0.0;
            double totalAmount = 0.0;
            double totalLifeAmount = 0.0;
            double totalBase = 0.0;
            float interpolation = Mathf.Clamp01(capture.Interpolation);

            for (long cellIndex = 0; cellIndex < totalCells; cellIndex++)
            {
                int channelIndex = checked((int)(
                    cellIndex * VisibilityDiagnosticChannelCount));
                float previousR = Mathf.HalfToFloat(
                    capture.PreviousData[channelIndex]);
                float previousG = Mathf.HalfToFloat(
                    capture.PreviousData[channelIndex + 1]);
                float previousA = Mathf.HalfToFloat(
                    capture.PreviousData[channelIndex + 3]);
                float currentR = Mathf.HalfToFloat(
                    capture.CurrentData[channelIndex]);
                float currentG = Mathf.HalfToFloat(
                    capture.CurrentData[channelIndex + 1]);
                float currentA = Mathf.HalfToFloat(
                    capture.CurrentData[channelIndex + 3]);

                float materialAmount = Mathf.Clamp01(Mathf.Lerp(
                    previousR,
                    currentR,
                    interpolation));
                float storedCoverage = Mathf.Clamp01(Mathf.Lerp(
                    previousA,
                    currentA,
                    interpolation));
                float lifeMoment = Mathf.Clamp01(Mathf.Lerp(
                    previousG,
                    currentG,
                    interpolation));
                bool legacyPackedState =
                    storedCoverage <= 0.00000001f &&
                    materialAmount > 0f;
                float coverage = legacyPackedState
                    ? materialAmount
                    : storedCoverage;

                if (coverage <= 0f && materialAmount <= 0f)
                {
                    emptyCells++;
                    continue;
                }

                materialCells++;
                if (coverage >= VisibilityDiagnosticAuthorityCoverage)
                {
                    authorityCells++;
                }

                float baseVisibility = ResolveDiagnosticBaseVisibility(
                    coverage,
                    capture.VisibilityMode);
                int bucket = ResolveCoverageDiagnosticBucket(coverage);
                cells[bucket]++;
                if (baseVisibility > 0.0001f)
                {
                    baseSupportedCells[bucket]++;
                }

                coverageSums[bucket] += coverage;
                amountSums[bucket] += materialAmount;
                lifeAmountSums[bucket] += lifeMoment;
                baseSums[bucket] += baseVisibility;
                totalCoverage += coverage;
                totalAmount += materialAmount;
                totalLifeAmount += lifeMoment;
                totalBase += baseVisibility;
            }

            CultureInfo culture = CultureInfo.InvariantCulture;
            StringBuilder report = new StringBuilder(4096);
            report.AppendLine("[River Foam Coverage / Visibility Diagnostic]");
            report.Append("Generated: ")
                .AppendLine(DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss",
                    culture));
            report.Append("Field: ")
                .Append(capture.Width.ToString("N0", culture))
                .Append(" x ")
                .Append(capture.Height.ToString("N0", culture))
                .Append(" = ")
                .Append(totalCells.ToString("N0", culture))
                .AppendLine(" cells");
            report.Append("Committed interpolation alpha: ")
                .AppendLine(interpolation.ToString("0.000000", culture));
            report.Append("Material Transport: ")
                .AppendLine(ResolveTransportDiagnosticLabel(
                    capture.TransportScheme));
            report.Append("Final Visibility: ")
                .AppendLine(ResolveVisibilityDiagnosticLabel(
                    capture.VisibilityMode));
            report.Append("Presence Footprint: ")
                .AppendLine(ResolvePresenceDiagnosticLabel(
                    capture.PresenceMode));
            report.Append("Foam state held: ")
                .AppendLine(capture.StateHeld ? "Yes" : "No");
            AppendGridDescriptorDiagnostic(
                report,
                capture.GridDescriptor,
                culture);
            report.AppendLine();
            report.AppendLine("[Global State]");
            report.Append("Empty cells: ")
                .AppendLine(emptyCells.ToString("N0", culture));
            report.Append("Nonzero material cells: ")
                .Append(materialCells.ToString("N0", culture))
                .Append(" (")
                .Append(ResolvePercentage(materialCells, totalCells, culture))
                .AppendLine(")");
            report.Append("Cells at C >= 0.02: ")
                .Append(authorityCells.ToString("N0", culture))
                .Append(" (")
                .Append(ResolvePercentage(authorityCells, materialCells, culture))
                .AppendLine(" of material cells)");
            report.Append("Integrated Coverage ΣC: ")
                .AppendLine(totalCoverage.ToString("0.000000", culture));
            report.Append("Integrated Material Amount Σ(C×P): ")
                .AppendLine(totalAmount.ToString("0.000000", culture));
            report.Append("Integrated Life Amount Σ(C×P×L): ")
                .AppendLine(totalLifeAmount.ToString("0.000000", culture));
            report.Append("Coverage-weighted Presence Σ(C×P)/ΣC: ")
                .AppendLine(ResolveRatio(
                    totalAmount,
                    totalCoverage,
                    culture));
            report.Append("Material-weighted Remaining Life Σ(C×P×L)/Σ(C×P): ")
                .AppendLine(ResolveRatio(
                    totalLifeAmount,
                    totalAmount,
                    culture));
            report.Append("Integrated selected base visibility ΣB: ")
                .AppendLine(totalBase.ToString("0.000000", culture));
            report.Append("Base visibility / Coverage ratio ΣB/ΣC: ")
                .AppendLine(ResolveRatio(totalBase, totalCoverage, culture));
            report.AppendLine();
            report.AppendLine("[Coverage Buckets]");

            for (int bucket = 0; bucket < bucketCount; bucket++)
            {
                report.Append(bucketLabels[bucket]).AppendLine(":");
                report.Append("  cells: ")
                    .Append(cells[bucket].ToString("N0", culture))
                    .Append(" (")
                    .Append(ResolvePercentage(
                        cells[bucket],
                        materialCells,
                        culture))
                    .AppendLine(" of material cells)");
                report.Append("  base-supported cells (B > 0.0001): ")
                    .Append(baseSupportedCells[bucket].ToString("N0", culture))
                    .Append(" (")
                    .Append(ResolvePercentage(
                        baseSupportedCells[bucket],
                        cells[bucket],
                        culture))
                    .AppendLine(")");
                report.Append("  ΣC / Σ(C×P) / Σ(C×P×L): ")
                    .Append(coverageSums[bucket].ToString("0.000000", culture))
                    .Append(" / ")
                    .Append(amountSums[bucket].ToString("0.000000", culture))
                    .Append(" / ")
                    .AppendLine(lifeAmountSums[bucket].ToString(
                        "0.000000",
                        culture));
                report.Append("  average C / weighted P / weighted L: ")
                    .Append(ResolveRatio(
                        coverageSums[bucket],
                        cells[bucket],
                        culture))
                    .Append(" / ")
                    .Append(ResolveRatio(
                        amountSums[bucket],
                        coverageSums[bucket],
                        culture))
                    .Append(" / ")
                    .AppendLine(ResolveRatio(
                        lifeAmountSums[bucket],
                        amountSums[bucket],
                        culture));
                report.Append("  Σ selected base visibility B: ")
                    .AppendLine(baseSums[bucket].ToString(
                        "0.000000",
                        culture));
            }

            report.AppendLine();
            report.AppendLine("[Interpretation Contract]");
            report.AppendLine(
                "Coverage C is occupied cell fraction. Presence P is intrinsic " +
                "strength inside that fraction. Remaining Life L is normalized " +
                "lifecycle state of the existing material.");
            report.AppendLine(
                "Material Amount is C×P. Life Amount is C×P×L. Bright decoded " +
                "Presence or Remaining Life does not imply high Coverage.");
            report.AppendLine(
                "B is only the selected scalar visibility-policy base: " +
                "Concentration + Lifetime uses RiverWaterFoamSharpenCoverage; " +
                "Lifecycle-Faithful uses smoothstep(0.02, 0.10, C).");
            report.AppendLine(
                "Exact Pattern/life erosion, screen-derivative resolution, " +
                "surface warp/wake coupling, Presence-footprint selection, and " +
                "the final pre-Chip production mask are pixel-space operations. " +
                "Inspect them through Visibility Pipeline Composite: red = " +
                "meaningful Coverage, green = B, blue = exact pre-Chip mask.");
            report.AppendLine(
                "For a strict same-state comparison, enable Hold Foam State " +
                "before capturing this report and the debug screenshots.");
            return report.ToString();
        }

        private static float ResolveDiagnosticBaseVisibility(
            float coverage,
            StylizedRiverFinalFoamVisibilityMode visibilityMode)
        {
            float c = Mathf.Clamp01(coverage);
            if (visibilityMode ==
                StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful)
            {
                return SmoothStepDiagnostic(0.02f, 0.10f, c);
            }

            float sharpness = Mathf.Clamp01(MaterialContourSharpness);
            float low = Mathf.Lerp(0.105f, 0.185f, sharpness);
            float high = Mathf.Lerp(0.365f, 0.575f, sharpness);
            float shaped = SmoothStepDiagnostic(low, high, c);
            float hard = SmoothStepDiagnostic(0.18f, 0.82f, shaped);
            hard = Mathf.Pow(
                Mathf.Max(0f, hard),
                Mathf.Lerp(1.65f, 2.15f, sharpness));
            return Mathf.Clamp01(hard);
        }

        private static float SmoothStepDiagnostic(
            float edge0,
            float edge1,
            float value)
        {
            float range = Mathf.Max(0.0000001f, edge1 - edge0);
            float t = Mathf.Clamp01((value - edge0) / range);
            return t * t * (3f - 2f * t);
        }

        private static int ResolveCoverageDiagnosticBucket(float coverage)
        {
            if (coverage < 0.02f)
            {
                return 0;
            }
            if (coverage < 0.10f)
            {
                return 1;
            }
            if (coverage < 0.20f)
            {
                return 2;
            }
            if (coverage < 0.30f)
            {
                return 3;
            }
            if (coverage < 0.40f)
            {
                return 4;
            }
            return 5;
        }

        private static string ResolveTransportDiagnosticLabel(
            StylizedRiverFoamTransportScheme scheme)
        {
            return scheme switch
            {
                StylizedRiverFoamTransportScheme.TvdSuperbee =>
                    "TVD Superbee",
                StylizedRiverFoamTransportScheme.BulkPhaseResidualTvd =>
                    "Bulk-Phase Residual TVD",
                _ => "Donor Cell"
            };
        }

        private static string ResolveVisibilityDiagnosticLabel(
            StylizedRiverFinalFoamVisibilityMode mode)
        {
            return mode ==
                StylizedRiverFinalFoamVisibilityMode.LifecycleFaithful
                ? "Lifecycle-Faithful"
                : "Concentration + Lifetime";
        }

        private static string ResolvePresenceDiagnosticLabel(
            StylizedRiverFoamPresenceFootprintMode mode)
        {
            return mode ==
                StylizedRiverFoamPresenceFootprintMode.PresenceAmplitude
                ? "Presence-Amplitude"
                : "Coverage-Only";
        }

        private static string ResolvePercentage(
            long numerator,
            long denominator,
            CultureInfo culture)
        {
            if (denominator <= 0)
            {
                return "n/a";
            }

            return ((double)numerator / denominator)
                .ToString("0.000 %", culture);
        }

        private static string ResolveRatio(
            double numerator,
            double denominator,
            CultureInfo culture)
        {
            return denominator > 0.000000000001
                ? (numerator / denominator).ToString("0.000000", culture)
                : "n/a";
        }

        private static void AppendGridDescriptorDiagnostic(
            StringBuilder report,
            StylizedRiverFoamGridDescriptor descriptor,
            CultureInfo culture)
        {
            if (!descriptor.IsCreated)
            {
                report.AppendLine("Grid descriptor: unavailable");
                return;
            }

            report.Append("Grid mapping: ")
                .AppendLine(descriptor.Mapping.ToString());
            report.Append("Resolved spacing: ")
                .Append(descriptor.ResolvedDxMetres.ToString(
                    "0.000000",
                    culture))
                .Append(" m longitudinal x ")
                .Append(descriptor.ResolvedDyMetres.ToString(
                    "0.000000",
                    culture))
                .AppendLine(" m lateral");
            if (descriptor.ResolvedDxMetres > 0f &&
                descriptor.ResolvedDyMetres > 0f)
            {
                report.Append("Metric cell area: ")
                    .Append((descriptor.ResolvedDxMetres *
                        descriptor.ResolvedDyMetres).ToString(
                            "0.000000",
                            culture))
                    .AppendLine(" m^2");
            }
        }
    }
}
