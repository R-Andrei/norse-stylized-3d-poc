using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        public bool StartProgressiveRibbonNormalized(
            float distanceNormalized,
            float acrossNormalized,
            float ribbonHalfWidth,
            float amount,
            float remainingLife,
            float duration,
            float travelDistance,
            float acrossDrift,
            float pathWander)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                progressiveRibbonRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeProgressiveRibbonSlot();
            if (slotIndex < 0)
            {
                progressiveRibbonRejectedCount++;
                return false;
            }

            float startGlobalDistance = Mathf.Lerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                Mathf.Clamp01(distanceNormalized));
            float availableDownstreamDistance = Mathf.Max(
                0f,
                river.Domain.GlobalDistanceMaximum - startGlobalDistance);
            float resolvedTravelDistance = Mathf.Min(
                Mathf.Clamp(
                    travelDistance,
                    ProgressiveRibbonMinimumTravelDistance,
                    ProgressiveRibbonMaximumTravelDistance),
                availableDownstreamDistance);
            float resolvedAmount = Mathf.Clamp01(amount);
            if (resolvedTravelDistance <= 0.01f ||
                resolvedAmount <= 0.0001f)
            {
                progressiveRibbonRejectedCount++;
                return false;
            }

            if (activeProgressiveRibbonEventCount == 0)
            {
                ResetProgressiveBirthDiagnosticSession();
            }

            int eventId = ++progressiveRibbonSequence;
            float shapeSeed = river.VisualSeed + eventId * 37.719f;
            float patternSeed =
                river.VisualSeed * 0.613f +
                eventId * 97.217f +
                ProgressivePatternSeedSalt;
            float sourceFillSeed =
                river.VisualSeed * 0.431f +
                eventId * 53.173f +
                ProgressiveSourceFillSeedSalt;
            float bendSign = Hash01(shapeSeed + 11.3f) < 0.5f ? -1f : 1f;
            float widthPhase = Hash01(shapeSeed + 23.7f) * Mathf.PI * 2f;
            float resolvedRemainingLife = Mathf.Clamp01(remainingLife);
            float startAcross = Mathf.Clamp(acrossNormalized, -1f, 1f);
            float resolvedHalfWidth = Mathf.Clamp(
                ribbonHalfWidth,
                ProgressiveRibbonMinimumHalfWidth,
                ProgressiveRibbonMaximumHalfWidth);
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedHalfWidth);
            float startRadius = ResolveProgressiveRibbonRadius(
                resolvedHalfWidth,
                0f,
                widthPhase,
                0f);

            progressiveRibbonEvents[slotIndex] = new ProgressiveRibbonEvent
            {
                Active = true,
                EventId = eventId,
                StartGlobalDistance = startGlobalDistance,
                StartAcrossNormalized = startAcross,
                Duration = Mathf.Clamp(
                    duration,
                    ProgressiveRibbonMinimumDuration,
                    ProgressiveRibbonMaximumDuration),
                TravelDistance = resolvedTravelDistance,
                AcrossDrift = Mathf.Clamp(acrossDrift, -1f, 1f),
                PathWander = Mathf.Clamp01(pathWander),
                BaseRadius = resolvedHalfWidth,
                SourceAmount = resolvedAmount,
                RemainingLife = resolvedRemainingLife,
                PatternSeed = patternSeed,
                ShapeSeed = shapeSeed,
                SourceFillSeed = sourceFillSeed,
                SourceFillFeatureSize = sourceFillFeatureSize,
                BendSign = bendSign,
                WidthPhase = widthPhase,
                Elapsed = 0f,
                PreviousGlobalDistance = startGlobalDistance,
                PreviousAcrossNormalized = startAcross,
                PreviousRadius = startRadius,
                PreviousEmissionAmount = 0f,
                DebugTrajectoryPending = true
            };

            activeProgressiveRibbonEventCount++;
            progressiveRibbonStartedCount++;
            latestProgressiveRibbonEventId = eventId;
            latestProgressiveRibbonProgress = 0f;
            latestProgressiveRibbonHeadDistanceNormalized =
                Mathf.Clamp01(distanceNormalized);
            latestProgressiveRibbonPreviousDistanceNormalized =
                latestProgressiveRibbonHeadDistanceNormalized;
            latestProgressiveRibbonHeadAcrossNormalized = startAcross;
            latestProgressiveRibbonPreviousAcrossNormalized = startAcross;
            lastProgressiveRibbonSegmentLength = 0f;
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
            return true;
        }

        private bool AdvanceProgressiveRibbonEvents(
            float deltaTime,
            float now)
        {
            if (activeProgressiveRibbonEventCount <= 0)
            {
                return false;
            }

            bool depositedAny = false;
            for (int slotIndex = 0;
                 slotIndex < progressiveRibbonEvents.Length;
                 slotIndex++)
            {
                ProgressiveRibbonEvent ribbonEvent =
                    progressiveRibbonEvents[slotIndex];
                if (!ribbonEvent.Active)
                {
                    continue;
                }

                progressiveRibbonEventUpdateCount++;
                PrepareProgressiveBirthDebugEvent(ref ribbonEvent);

                ribbonEvent.Elapsed = Mathf.Min(
                    ribbonEvent.Duration,
                    ribbonEvent.Elapsed + deltaTime);
                float progress = Mathf.Clamp01(
                    ribbonEvent.Elapsed /
                    Mathf.Max(0.0001f, ribbonEvent.Duration));
                ResolveProgressiveRibbonHead(
                    ribbonEvent,
                    progress,
                    out float headGlobalDistance,
                    out float headAcrossNormalized);
                float envelope = ResolveProgressiveRibbonEnvelope(progress);
                float headRadius = ResolveProgressiveRibbonRadius(
                    ribbonEvent.BaseRadius,
                    progress,
                    ribbonEvent.WidthPhase,
                    envelope);
                float headAmount = ribbonEvent.SourceAmount * envelope;

                float segmentLength = Vector2.Distance(
                    new Vector2(
                        ribbonEvent.PreviousGlobalDistance,
                        ResolveAcrossMetresApproximation(
                            ribbonEvent.PreviousAcrossNormalized)),
                    new Vector2(
                        headGlobalDistance,
                        ResolveAcrossMetresApproximation(
                            headAcrossNormalized)));

                if (segmentLength > 0.0001f &&
                    (ribbonEvent.PreviousEmissionAmount > 0.0001f ||
                     headAmount > 0.0001f))
                {
                    PendingInjection segment =
                        CreateProgressiveRibbonSegment(
                            ribbonEvent,
                            headGlobalDistance,
                            headAcrossNormalized,
                            headRadius,
                            headAmount);
                    progressiveRibbonSegmentDispatchAttemptCount++;
                    ActivateInjectionRange(segment, now);
                    if (PaintProgressiveBirthSourceSegment(segment))
                    {
                        progressiveRibbonSegmentDispatchSubmittedCount++;
                        progressiveRibbonCumulativeCentrelineDistance +=
                            segmentLength;
                        PaintProgressiveBirthDebugSegment(segment);
                        injectedLastUpdate++;
                        depositedAny = true;
                        lastProgressiveRibbonSegmentLength = segmentLength;
                    }
                }

                UpdateLatestProgressiveRibbonDiagnostics(
                    ribbonEvent,
                    progress,
                    headGlobalDistance,
                    headAcrossNormalized);

                ribbonEvent.PreviousGlobalDistance = headGlobalDistance;
                ribbonEvent.PreviousAcrossNormalized =
                    headAcrossNormalized;
                ribbonEvent.PreviousRadius = headRadius;
                ribbonEvent.PreviousEmissionAmount = headAmount;

                if (progress >= 0.999999f)
                {
                    CompleteProgressiveRibbonEvent(ribbonEvent, now);
                    progressiveRibbonEvents[slotIndex] = default;
                    activeProgressiveRibbonEventCount = Mathf.Max(
                        0,
                        activeProgressiveRibbonEventCount - 1);
                    progressiveRibbonCompletedCount++;
                    continue;
                }

                progressiveRibbonEvents[slotIndex] = ribbonEvent;
            }

            return depositedAny;
        }

        private PendingInjection CreateProgressiveRibbonSegment(
            ProgressiveRibbonEvent ribbonEvent,
            float headGlobalDistance,
            float headAcrossNormalized,
            float headRadius,
            float headAmount)
        {
            float centreGlobalDistance =
                (ribbonEvent.PreviousGlobalDistance +
                 headGlobalDistance) * 0.5f;
            float centreAcross =
                (ribbonEvent.PreviousAcrossNormalized +
                 headAcrossNormalized) * 0.5f;
            float maximumRadius = Mathf.Max(
                ribbonEvent.PreviousRadius,
                headRadius);
            float maximumAmount = Mathf.Max(
                ribbonEvent.PreviousEmissionAmount,
                headAmount);

            return new PendingInjection(
                centreGlobalDistance,
                centreAcross,
                maximumRadius,
                maximumAmount,
                ribbonEvent.RemainingLife,
                ribbonEvent.PatternSeed,
                1f,
                false,
                ribbonEvent.SourceFillSeed,
                ribbonEvent.SourceFillFeatureSize,
                ribbonEvent.ShapeSeed,
                0f,
                false,
                true,
                ribbonEvent.PreviousGlobalDistance,
                ribbonEvent.PreviousAcrossNormalized,
                ribbonEvent.PreviousRadius,
                ribbonEvent.PreviousEmissionAmount,
                headGlobalDistance,
                headAcrossNormalized,
                headRadius,
                headAmount);
        }

        private void CompleteProgressiveRibbonEvent(
            ProgressiveRibbonEvent ribbonEvent,
            float now)
        {
            float alongRadius =
                ribbonEvent.TravelDistance * 0.5f +
                ribbonEvent.BaseRadius * 1.2f;
            reservations.Add(
                new FoamReservation
                {
                    CentreGlobalDistance =
                        ribbonEvent.StartGlobalDistance +
                        ribbonEvent.TravelDistance * 0.5f,
                    AlongRadius = alongRadius,
                    Elapsed = 0f,
                    MaximumLifetime = Mathf.Clamp(
                        ribbonEvent.RemainingLife *
                        ResolveMaximumMaterialReservationSeconds() +
                        EndOfLifeDissipationSeconds * 2f,
                        EndOfLifeDissipationSeconds,
                        MaximumManualReservationSeconds)
                });
            ActivateGlobalRange(
                ribbonEvent.StartGlobalDistance - ribbonEvent.BaseRadius,
                ribbonEvent.StartGlobalDistance +
                    ribbonEvent.TravelDistance +
                    ribbonEvent.BaseRadius,
                now + Mathf.Min(
                    5f,
                    ResolveMaximumMaterialReservationSeconds()));
        }

        private void ResolveProgressiveRibbonHead(
            ProgressiveRibbonEvent ribbonEvent,
            float progress,
            out float globalDistance,
            out float acrossNormalized)
        {
            globalDistance = ribbonEvent.StartGlobalDistance +
                ribbonEvent.TravelDistance * progress;
            float bend =
                Mathf.Sin(progress * Mathf.PI) *
                ribbonEvent.BendSign *
                ProgressiveRibbonMaximumBendAcross *
                ribbonEvent.PathWander;
            acrossNormalized = Mathf.Clamp(
                ribbonEvent.StartAcrossNormalized +
                ribbonEvent.AcrossDrift * progress +
                bend,
                -1f,
                1f);
        }

        private static float ResolveSourceFillFeatureSize(float sourceRadius)
        {
            return Mathf.Max(
                SourceFillMinimumFeatureSizeMetres,
                Mathf.Max(0.05f, sourceRadius) *
                SourceFillFeatureSizeRadiusMultiplier);
        }

        private static float ResolveProgressiveRibbonEnvelope(float progress)
        {
            float rampIn = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    0f,
                    ProgressiveRibbonRampInEnd,
                    progress));
            float taperOut = 1f - Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(
                    ProgressiveRibbonTaperStart,
                    1f,
                    progress));
            return Mathf.Clamp01(rampIn * taperOut);
        }

        private static float ResolveProgressiveRibbonRadius(
            float baseRadius,
            float progress,
            float widthPhase,
            float envelope)
        {
            float coherentVariation = 1f +
                Mathf.Sin(progress * Mathf.PI * 2f + widthPhase) *
                ProgressiveRibbonWidthVariation;
            float taperScale = Mathf.Lerp(0.25f, 1f, envelope);
            return Mathf.Max(
                0.025f,
                baseRadius * coherentVariation * taperScale);
        }

        private void UpdateLatestProgressiveRibbonDiagnostics(
            ProgressiveRibbonEvent ribbonEvent,
            float progress,
            float headGlobalDistance,
            float headAcrossNormalized)
        {
            if (ribbonEvent.EventId < latestProgressiveRibbonEventId)
            {
                return;
            }

            latestProgressiveRibbonEventId = ribbonEvent.EventId;
            latestProgressiveRibbonProgress = progress;
            latestProgressiveRibbonPreviousDistanceNormalized =
                GlobalDistanceToNormalized(
                    ribbonEvent.PreviousGlobalDistance);
            latestProgressiveRibbonPreviousAcrossNormalized =
                ribbonEvent.PreviousAcrossNormalized;
            latestProgressiveRibbonHeadDistanceNormalized =
                GlobalDistanceToNormalized(headGlobalDistance);
            latestProgressiveRibbonHeadAcrossNormalized =
                headAcrossNormalized;
        }

        private float GlobalDistanceToNormalized(float globalDistance)
        {
            if (river == null || !river.Domain.IsValid)
            {
                return 0f;
            }

            return Mathf.InverseLerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                globalDistance);
        }

        private float ResolveAcrossMetresApproximation(
            float acrossNormalized)
        {
            float halfWidth = river != null
                ? Mathf.Max(0.25f, river.ResolvedMaximumVisibleWidth * 0.5f)
                : 1f;
            return Mathf.Clamp(acrossNormalized, -1f, 1f) * halfWidth;
        }

        private int FindFreeProgressiveRibbonSlot()
        {
            for (int index = 0;
                 index < progressiveRibbonEvents.Length;
                 index++)
            {
                if (!progressiveRibbonEvents[index].Active)
                {
                    return index;
                }
            }

            return -1;
        }

        private void ClearProgressiveRibbonEvents()
        {
            Array.Clear(
                progressiveRibbonEvents,
                0,
                progressiveRibbonEvents.Length);
            activeProgressiveRibbonEventCount = 0;
            latestProgressiveRibbonEventId = 0;
            latestProgressiveRibbonProgress = 0f;
            latestProgressiveRibbonHeadDistanceNormalized = 0f;
            latestProgressiveRibbonHeadAcrossNormalized = 0f;
            latestProgressiveRibbonPreviousDistanceNormalized = 0f;
            latestProgressiveRibbonPreviousAcrossNormalized = 0f;
            lastProgressiveRibbonSegmentLength = 0f;
            ResetProgressiveBirthDiagnosticSession();
        }

        private static float Hash01(float value)
        {
            return Mathf.Repeat(
                Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f,
                1f);
        }
    }
}
