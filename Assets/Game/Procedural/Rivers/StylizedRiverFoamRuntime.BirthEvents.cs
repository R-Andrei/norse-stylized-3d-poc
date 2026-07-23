using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        public bool StartFoamCompositionNormalized(
            float distanceNormalized,
            float acrossNormalized,
            float scale,
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
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeFoamCompositionSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float startGlobalDistance = Mathf.Lerp(
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum,
                Mathf.Clamp01(distanceNormalized));
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float availableDownstreamDistance = Mathf.Max(
                0f,
                flowDirection > 0f
                    ? river.Domain.GlobalDistanceMaximum - startGlobalDistance
                    : startGlobalDistance - river.Domain.GlobalDistanceMinimum);
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
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            float startAcross = Mathf.Clamp(acrossNormalized, -1f, 1f);
            float resolvedHalfWidth = Mathf.Clamp(
                scale,
                ProgressiveRibbonMinimumHalfWidth,
                ProgressiveRibbonMaximumHalfWidth);
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedHalfWidth);
            float resolvedDuration = Mathf.Clamp(
                duration,
                ProgressiveRibbonMinimumDuration,
                ProgressiveRibbonMaximumDuration);
            float resolvedDrift = Mathf.Clamp(acrossDrift, -1f, 1f);
            float resolvedWander = Mathf.Clamp01(pathWander);
            float sourceKey =
                river.VisualSeed * 0.613f +
                Mathf.Clamp01(distanceNormalized) * 1009.17f +
                startAcross * 503.31f +
                resolvedHalfWidth * 311.73f +
                resolvedTravelDistance * 67.19f +
                resolvedDrift * 59.7f +
                resolvedWander * 37.1f;
            float shapeSeed = sourceKey + 37.719f;
            float patternSeed = sourceKey + ProgressivePatternSeedSalt;
            float sourceFillSeed = sourceKey + ProgressiveSourceFillSeedSalt;
            float bendSign = Hash01(shapeSeed + 11.3f) < 0.5f ? -1f : 1f;
            float startRadius = ResolveProgressiveRibbonRadius(
                resolvedHalfWidth,
                0f,
                0f,
                0f,
                0f);
            float startLateralApproximation =
                ResolveAcrossMetresApproximation(startAcross);

            foamCompositionEvents[slotIndex] = new FoamCompositionEvent
            {
                Active = true,
                UsesMetricLateral = false,
                EventId = eventId,
                StartGlobalDistance = startGlobalDistance,
                StartAcrossNormalized = startAcross,
                StartLateralMetres = startLateralApproximation,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = resolvedDrift,
                AcrossDriftMetres = 0f,
                PathWander = resolvedWander,
                PathWanderMetres = 0f,
                BaseRadius = resolvedHalfWidth,
                SourceAmount = resolvedAmount,
                RemainingLife = Mathf.Clamp01(remainingLife),
                AmountEnvelopeFloor = 0f,
                RadiusEnvelopeFloor = 0f,
                PatternSeed = patternSeed,
                ShapeSeed = shapeSeed,
                SourceFillSeed = sourceFillSeed,
                SourceFillFeatureSize = sourceFillFeatureSize,
                BendSign = bendSign,
                WidthPhase = 0f,
                StrokeAspect = ManualSourceStrokeAspect,
                WidthVariation = 0f,
                Elapsed = 0f,
                PreviousGlobalDistance = startGlobalDistance,
                PreviousAcrossNormalized = startAcross,
                PreviousLateralMetres = startLateralApproximation,
                PreviousRadius = startRadius,
                PreviousEmissionAmount = 0f
            };

            ActivateFoamCompositionEvent(
                eventId,
                Mathf.Clamp01(distanceNormalized),
                startAcross,
                "Remaining Life / full-field direct simulation");
            return true;
        }

        public bool StartFoamCompositionMetric(
            float startGlobalDistance,
            float startLateralMetres,
            float scale,
            float amount,
            float remainingLife,
            float duration,
            float travelDistance,
            float acrossDriftMetres,
            float pathWanderMetres)
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeFoamCompositionSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float clampedStartGlobalDistance = Mathf.Clamp(
                startGlobalDistance,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float startAcross = ResolveSourceAcrossNormalized(
                clampedStartGlobalDistance,
                startLateralMetres);
            float clampedStartLateralMetres = ResolveSourceLateralMetres(
                clampedStartGlobalDistance,
                startAcross);
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float availableDownstreamDistance = Mathf.Max(
                0f,
                flowDirection > 0f
                    ? river.Domain.GlobalDistanceMaximum -
                        clampedStartGlobalDistance
                    : clampedStartGlobalDistance -
                        river.Domain.GlobalDistanceMinimum);
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
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            float resolvedHalfWidth = Mathf.Clamp(
                scale,
                ProgressiveRibbonMinimumHalfWidth,
                ProgressiveRibbonMaximumHalfWidth);
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedHalfWidth);
            float resolvedDuration = Mathf.Clamp(
                duration,
                ProgressiveRibbonMinimumDuration,
                ProgressiveRibbonMaximumDuration);
            float resolvedDriftMetres = Mathf.Clamp(
                acrossDriftMetres,
                -16f,
                16f);
            float resolvedWanderMetres = Mathf.Clamp(
                Mathf.Abs(pathWanderMetres),
                0f,
                16f);
            float sourceKey =
                river.VisualSeed * 0.613f +
                clampedStartGlobalDistance * 1009.17f +
                clampedStartLateralMetres * 503.31f +
                resolvedHalfWidth * 311.73f +
                resolvedTravelDistance * 67.19f +
                resolvedDriftMetres * 59.7f +
                resolvedWanderMetres * 37.1f;
            float shapeSeed = sourceKey + 37.719f;
            float patternSeed = sourceKey + ProgressivePatternSeedSalt;
            float sourceFillSeed = sourceKey + ProgressiveSourceFillSeedSalt;
            float bendSign = Hash01(shapeSeed + 11.3f) < 0.5f ? -1f : 1f;
            float startRadius = ResolveProgressiveRibbonRadius(
                resolvedHalfWidth,
                0f,
                0f,
                0f,
                0f);

            foamCompositionEvents[slotIndex] = new FoamCompositionEvent
            {
                Active = true,
                UsesMetricLateral = true,
                EventId = eventId,
                StartGlobalDistance = clampedStartGlobalDistance,
                StartAcrossNormalized = startAcross,
                StartLateralMetres = clampedStartLateralMetres,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = 0f,
                AcrossDriftMetres = resolvedDriftMetres,
                PathWander = 0f,
                PathWanderMetres = resolvedWanderMetres,
                BaseRadius = resolvedHalfWidth,
                SourceAmount = resolvedAmount,
                RemainingLife = Mathf.Clamp01(remainingLife),
                AmountEnvelopeFloor = 0f,
                RadiusEnvelopeFloor = 0f,
                PatternSeed = patternSeed,
                ShapeSeed = shapeSeed,
                SourceFillSeed = sourceFillSeed,
                SourceFillFeatureSize = sourceFillFeatureSize,
                BendSign = bendSign,
                WidthPhase = 0f,
                StrokeAspect = ManualSourceStrokeAspect,
                WidthVariation = 0f,
                Elapsed = 0f,
                PreviousGlobalDistance = clampedStartGlobalDistance,
                PreviousAcrossNormalized = startAcross,
                PreviousLateralMetres = clampedStartLateralMetres,
                PreviousRadius = startRadius,
                PreviousEmissionAmount = 0f
            };

            ActivateFoamCompositionEvent(
                eventId,
                GlobalDistanceToNormalized(clampedStartGlobalDistance),
                startAcross,
                "Remaining Life / full-field direct simulation");
            return true;
        }

        private void ActivateFoamCompositionEvent(
            int eventId,
            float startDistanceNormalized,
            float startAcrossNormalized,
            string authorityStatus)
        {
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus = authorityStatus;
            activeFoamCompositionEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                Mathf.Clamp01(startDistanceNormalized);
            latestFoamCompositionPreviousDistanceNormalized =
                latestFoamCompositionHeadDistanceNormalized;
            latestFoamCompositionHeadAcrossNormalized =
                Mathf.Clamp(startAcrossNormalized, -1f, 1f);
            latestFoamCompositionPreviousAcrossNormalized =
                latestFoamCompositionHeadAcrossNormalized;
            lastFoamCompositionSegmentLength = 0f;
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
        }

        private bool AdvanceFoamCompositionEvents(
            float deltaTime,
            float now)
        {
            if (activeFoamCompositionEventCount <= 0)
            {
                return false;
            }

            int budget = ResolveFoamCompositionBirthBudgetPerStep();
            int slotCount = foamCompositionEvents.Length;
            int startIndex = Mathf.Clamp(
                foamCompositionScanCursor,
                0,
                Mathf.Max(0, slotCount - 1));
            bool depositedAny = false;

            for (int visited = 0; visited < slotCount; visited++)
            {
                int slotIndex = (startIndex + visited) % slotCount;
                FoamCompositionEvent compositionEvent =
                    foamCompositionEvents[slotIndex];
                if (!compositionEvent.Active)
                {
                    continue;
                }

                foamCompositionEventUpdateCount++;
                compositionEvent.Elapsed = Mathf.Min(
                    compositionEvent.Duration,
                    compositionEvent.Elapsed + deltaTime);
                float progress = Mathf.Clamp01(
                    compositionEvent.Elapsed /
                    Mathf.Max(0.0001f, compositionEvent.Duration));
                ResolveFoamCompositionHead(
                    compositionEvent,
                    progress,
                    out float headGlobalDistance,
                    out float headLateralMetres,
                    out float headAcrossNormalized);
                float envelope = ResolveProgressiveRibbonEnvelope(progress);
                float amountEnvelope = Mathf.Lerp(
                    Mathf.Clamp01(compositionEvent.AmountEnvelopeFloor),
                    1f,
                    envelope);
                float radiusEnvelope = Mathf.Lerp(
                    Mathf.Clamp01(compositionEvent.RadiusEnvelopeFloor),
                    1f,
                    envelope);
                float headRadius = ResolveProgressiveRibbonRadius(
                    compositionEvent.BaseRadius,
                    progress,
                    compositionEvent.WidthPhase,
                    radiusEnvelope,
                    compositionEvent.WidthVariation);
                float headAmount = Mathf.Clamp01(
                    compositionEvent.SourceAmount) * amountEnvelope;

                float segmentLength = Vector2.Distance(
                    new Vector2(
                        compositionEvent.PreviousGlobalDistance,
                        compositionEvent.PreviousLateralMetres),
                    new Vector2(
                        headGlobalDistance,
                        headLateralMetres));

                bool shouldEmit = segmentLength > 0.0001f &&
                    (compositionEvent.PreviousEmissionAmount > 0.0001f ||
                     headAmount > 0.0001f);
                bool emitted = false;
                if (shouldEmit)
                {
                    foamCompositionSegmentDispatchAttemptCount++;
                }

                if (shouldEmit && budget > 0)
                {
                    PendingInjection segment =
                        CreateFoamCompositionSegment(
                            compositionEvent,
                            headGlobalDistance,
                            headLateralMetres,
                            headAcrossNormalized,
                            headRadius,
                            headAmount);
                    QueueMaterialBirth(segment);
                    foamCompositionSegmentDispatchSubmittedCount++;
                    foamCompositionCumulativeCentrelineDistance +=
                        segmentLength;
                    injectedLastUpdate++;
                    depositedAny = true;
                    emitted = true;
                    budget--;
                    lastFoamCompositionSegmentLength = segmentLength;
                }

                UpdateLatestFoamCompositionDiagnostics(
                    compositionEvent,
                    progress,
                    headGlobalDistance,
                    headAcrossNormalized);

                if (emitted)
                {
                    compositionEvent.PreviousGlobalDistance = headGlobalDistance;
                    compositionEvent.PreviousAcrossNormalized =
                        headAcrossNormalized;
                    compositionEvent.PreviousLateralMetres =
                        headLateralMetres;
                    compositionEvent.PreviousRadius = headRadius;
                    compositionEvent.PreviousEmissionAmount = headAmount;
                }

                if (progress >= 0.999999f && (!shouldEmit || emitted))
                {
                    CompleteFoamCompositionEvent(compositionEvent, now);
                    foamCompositionEvents[slotIndex] = default;
                    activeFoamCompositionEventCount = Mathf.Max(
                        0,
                        activeFoamCompositionEventCount - 1);
                    foamCompositionCompletedCount++;
                    continue;
                }

                foamCompositionEvents[slotIndex] = compositionEvent;
            }

            foamCompositionScanCursor = slotCount > 0
                ? (startIndex + 1) % slotCount
                : 0;
            return depositedAny;
        }

        private PendingInjection CreateFoamCompositionSegment(
            FoamCompositionEvent compositionEvent,
            float headGlobalDistance,
            float headLateralMetres,
            float headAcrossNormalized,
            float headRadius,
            float headAmount)
        {
            float previousLateralMetres = compositionEvent.UsesMetricLateral
                ? compositionEvent.PreviousLateralMetres
                : ResolveAcrossMetresApproximation(
                    compositionEvent.PreviousAcrossNormalized);
            float resolvedHeadLateralMetres = compositionEvent.UsesMetricLateral
                ? headLateralMetres
                : ResolveAcrossMetresApproximation(headAcrossNormalized);
            Vector2 start = new Vector2(
                compositionEvent.PreviousGlobalDistance,
                previousLateralMetres);
            Vector2 end = new Vector2(
                headGlobalDistance,
                resolvedHeadLateralMetres);
            Vector2 axis = end - start;
            float maximumRadius = Mathf.Max(
                compositionEvent.PreviousRadius,
                headRadius);
            float minimumStrokeLength = Mathf.Max(
                maximumRadius * 2f,
                maximumRadius * compositionEvent.StrokeAspect);
            if (axis.sqrMagnitude < minimumStrokeLength * minimumStrokeLength)
            {
                Vector2 direction = axis.sqrMagnitude > 0.000001f
                    ? axis.normalized
                    : new Vector2(compositionEvent.FlowDirection, 0f);
                Vector2 centre = (start + end) * 0.5f;
                start = centre - direction * (minimumStrokeLength * 0.5f);
                end = centre + direction * (minimumStrokeLength * 0.5f);
            }

            float centreGlobalDistance = (start.x + end.x) * 0.5f;
            float centreLateralMetres = (start.y + end.y) * 0.5f;
            float centreAcross = compositionEvent.UsesMetricLateral
                ? ResolveSourceAcrossNormalized(
                    centreGlobalDistance,
                    centreLateralMetres)
                : ResolveAcrossNormalizedApproximation(centreLateralMetres);
            float startAcross = compositionEvent.UsesMetricLateral
                ? ResolveSourceAcrossNormalized(start.x, start.y)
                : ResolveAcrossNormalizedApproximation(start.y);
            float endAcross = compositionEvent.UsesMetricLateral
                ? ResolveSourceAcrossNormalized(end.x, end.y)
                : ResolveAcrossNormalizedApproximation(end.y);
            float maximumAmount = Mathf.Max(
                compositionEvent.PreviousEmissionAmount,
                headAmount);

            return new PendingInjection(
                centreGlobalDistance,
                centreAcross,
                maximumRadius,
                maximumAmount,
                compositionEvent.RemainingLife,
                compositionEvent.PatternSeed,
                compositionEvent.StrokeAspect,
                false,
                compositionEvent.SourceFillSeed,
                compositionEvent.SourceFillFeatureSize,
                compositionEvent.ShapeSeed,
                0f,
                false,
                true,
                start.x,
                startAcross,
                compositionEvent.PreviousRadius,
                compositionEvent.PreviousEmissionAmount,
                end.x,
                endAcross,
                headRadius,
                headAmount,
                compositionEvent.UsesMetricLateral,
                centreLateralMetres,
                start.y,
                end.y);
        }

        private void CompleteFoamCompositionEvent(
            FoamCompositionEvent compositionEvent,
            float now)
        {
            // Composition events only own bounded source birth. Persistent
            // material survival remains owned by the full-field lifecycle pass.
        }

        private void ResolveFoamCompositionHead(
            FoamCompositionEvent compositionEvent,
            float progress,
            out float globalDistance,
            out float lateralMetres,
            out float acrossNormalized)
        {
            globalDistance = compositionEvent.StartGlobalDistance +
                compositionEvent.FlowDirection *
                compositionEvent.TravelDistance * progress;
            if (!compositionEvent.UsesMetricLateral)
            {
                float bend =
                    Mathf.Sin(progress * Mathf.PI) *
                    compositionEvent.BendSign *
                    ProgressiveRibbonMaximumBendAcross *
                    compositionEvent.PathWander;
                acrossNormalized = Mathf.Clamp(
                    compositionEvent.StartAcrossNormalized +
                    compositionEvent.AcrossDrift * progress +
                    bend,
                    -1f,
                    1f);
                lateralMetres =
                    ResolveAcrossMetresApproximation(acrossNormalized);
                return;
            }

            float bendMetres =
                Mathf.Sin(progress * Mathf.PI) *
                compositionEvent.BendSign *
                compositionEvent.PathWanderMetres;
            lateralMetres = compositionEvent.StartLateralMetres +
                compositionEvent.AcrossDriftMetres * progress +
                bendMetres;
            acrossNormalized = ResolveSourceAcrossNormalized(
                globalDistance,
                lateralMetres);
            lateralMetres = ResolveSourceLateralMetres(
                globalDistance,
                acrossNormalized);
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
            float envelope,
            float widthVariation)
        {
            float coherentVariation = 1f +
                Mathf.Sin(progress * Mathf.PI * 2f + widthPhase) *
                Mathf.Clamp(widthVariation, 0f, 0.65f);
            float taperScale = Mathf.Lerp(0.25f, 1f, envelope);
            return Mathf.Max(
                0.025f,
                baseRadius * coherentVariation * taperScale);
        }

        private void UpdateLatestFoamCompositionDiagnostics(
            FoamCompositionEvent compositionEvent,
            float progress,
            float headGlobalDistance,
            float headAcrossNormalized)
        {
            if (compositionEvent.EventId < latestFoamCompositionEventId)
            {
                return;
            }

            latestFoamCompositionEventId = compositionEvent.EventId;
            latestFoamCompositionProgress = progress;
            latestFoamCompositionPreviousDistanceNormalized =
                GlobalDistanceToNormalized(
                    compositionEvent.PreviousGlobalDistance);
            latestFoamCompositionPreviousAcrossNormalized =
                compositionEvent.PreviousAcrossNormalized;
            latestFoamCompositionHeadDistanceNormalized =
                GlobalDistanceToNormalized(headGlobalDistance);
            latestFoamCompositionHeadAcrossNormalized =
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

        private float ResolveAcrossNormalizedApproximation(float acrossMetres)
        {
            float halfWidth = river != null
                ? Mathf.Max(0.25f, river.ResolvedMaximumVisibleWidth * 0.5f)
                : 1f;
            return Mathf.Clamp(acrossMetres / halfWidth, -1f, 1f);
        }

        private int FindFreeFoamCompositionSlot()
        {
            for (int index = 0;
                 index < foamCompositionEvents.Length;
                 index++)
            {
                if (!foamCompositionEvents[index].Active)
                {
                    return index;
                }
            }

            return -1;
        }

        private void ClearFoamCompositionEvents()
        {
            Array.Clear(
                foamCompositionEvents,
                0,
                foamCompositionEvents.Length);
            activeFoamCompositionEventCount = 0;
            foamCompositionScanCursor = 0;
            ClearAutomaticFoamSourceEvents();
            latestFoamCompositionEventId = 0;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized = 0f;
            latestFoamCompositionHeadAcrossNormalized = 0f;
            latestFoamCompositionPreviousDistanceNormalized = 0f;
            latestFoamCompositionPreviousAcrossNormalized = 0f;
            lastFoamCompositionSegmentLength = 0f;
            ResetAutomaticBirthDiagnosticSession();
        }

        private readonly struct ResolvedAutomaticRevealTiming
        {
            public ResolvedAutomaticRevealTiming(
                float pathDistanceMetres,
                float requestedSpeedMetresPerSecond,
                float rawDurationSeconds,
                float resolvedDurationSeconds,
                bool cadenceLimited)
            {
                PathDistanceMetres = pathDistanceMetres;
                RequestedSpeedMetresPerSecond =
                    requestedSpeedMetresPerSecond;
                RawDurationSeconds = rawDurationSeconds;
                ResolvedDurationSeconds = resolvedDurationSeconds;
                ActualSpeedMetresPerSecond = pathDistanceMetres /
                    Mathf.Max(0.0001f, resolvedDurationSeconds);
                CadenceLimited = cadenceLimited;
            }

            public float PathDistanceMetres { get; }
            public float RequestedSpeedMetresPerSecond { get; }
            public float RawDurationSeconds { get; }
            public float ResolvedDurationSeconds { get; }
            public float ActualSpeedMetresPerSecond { get; }
            public bool CadenceLimited { get; }
        }

        private ResolvedAutomaticRevealTiming ResolveAutomaticRevealTiming(
            float pathDistanceMetres,
            float baseSpeedMetresPerSecond,
            float patternSpeedMultiplier,
            float deterministicSpeedJitter)
        {
            float resolvedPathDistance = Mathf.Max(0.0001f, pathDistanceMetres);
            float requestedSpeed = Mathf.Max(
                0.0001f,
                baseSpeedMetresPerSecond *
                Mathf.Clamp(patternSpeedMultiplier, 0.10f, 3.00f) *
                Mathf.Max(0.0001f, deterministicSpeedJitter));
            float rawDuration = resolvedPathDistance / requestedSpeed;
            float materialStepDuration =
                1f / Mathf.Max(1f, ResolveUpdateRate());
            float resolvedDuration = Mathf.Max(
                materialStepDuration,
                rawDuration);
            return new ResolvedAutomaticRevealTiming(
                resolvedPathDistance,
                requestedSpeed,
                rawDuration,
                resolvedDuration,
                rawDuration < materialStepDuration);
        }

        private void RecordAutomaticRevealTiming(
            int eventId,
            AutomaticFoamSourceEventType sourceType,
            ResolvedAutomaticRevealTiming timing)
        {
            int telemetryIndex = (int)sourceType;
            if (telemetryIndex <= 0 ||
                telemetryIndex >= automaticRevealTimingByType.Length)
            {
                return;
            }

            automaticRevealTimingByType[telemetryIndex] =
                new AutomaticRevealTimingTelemetry
                {
                    HasValue = true,
                    EventId = eventId,
                    Type = sourceType,
                    PathDistanceMetres = timing.PathDistanceMetres,
                    RequestedSpeedMetresPerSecond =
                        timing.RequestedSpeedMetresPerSecond,
                    RawDurationSeconds = timing.RawDurationSeconds,
                    ResolvedDurationSeconds = timing.ResolvedDurationSeconds,
                    ActualSpeedMetresPerSecond =
                        timing.ActualSpeedMetresPerSecond,
                    CadenceLimited = timing.CadenceLimited
                };
        }

        private enum AutomaticShoreSourceRecipe
        {
            ShoreRibbon,
            InwardWash
        }

        private readonly struct AutomaticShoreSourceProfile
        {
            public AutomaticShoreSourceProfile(
                bool enabled,
                float coverage,
                float activity,
                float patchSize,
                float formationSpeedMetresPerSecond,
                StylizedRiverFoamShorePattern pattern)
            {
                Enabled = enabled;
                Coverage = Mathf.Clamp01(coverage);
                Activity = Mathf.Clamp01(activity);
                PatchSize = Mathf.Clamp01(patchSize);
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond);
                Pattern = pattern;
            }

            public bool Enabled { get; }
            public float Coverage { get; }
            public float Activity { get; }
            public float PatchSize { get; }
            public float FormationSpeedMetresPerSecond { get; }
            public StylizedRiverFoamShorePattern Pattern { get; }

            public float SlotSpacingMetres =>
                AutomaticShoreSourceSlotSpacingMetres;
            public float EventsPerSecond =>
                AutomaticShoreSourceMaximumEventsPerSecond * Activity;
        }



        private enum AutomaticObjectSourceRecipe
        {
            ContactArc,
            ContactSemiArc,
            ContactFleck
        }

        private readonly struct AutomaticObjectSourceProfile
        {
            public AutomaticObjectSourceProfile(
                bool enabled,
                float coverage,
                float activity,
                float formationSpeedMetresPerSecond,
                StylizedRiverFoamObjectPattern pattern)
            {
                Enabled = enabled;
                Coverage = Mathf.Clamp01(coverage);
                Activity = Mathf.Clamp01(activity);
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond);
                Pattern = pattern;
            }

            public bool Enabled { get; }
            public float Coverage { get; }
            public float Activity { get; }
            public float FormationSpeedMetresPerSecond { get; }
            public StylizedRiverFoamObjectPattern Pattern { get; }

            public float EventsPerSecond =>
                AutomaticObjectSourceMaximumEventsPerSecond * Activity;
        }

        private readonly struct ResolvedAutomaticObjectContactProfile
        {
            public ResolvedAutomaticObjectContactProfile(
                Vector2 point0,
                Vector2 point1,
                Vector2 point2,
                Vector2 point3,
                Vector2 point4)
            {
                Point0 = point0;
                Point1 = point1;
                Point2 = point2;
                Point3 = point3;
                Point4 = point4;
                float negativeFirstLength = Vector2.Distance(point0, point1);
                float negativeSecondLength = Vector2.Distance(point1, point2);
                float positiveFirstLength = Vector2.Distance(point2, point3);
                float positiveSecondLength = Vector2.Distance(point3, point4);
                NegativeHalfLength =
                    negativeFirstLength + negativeSecondLength;
                PositiveHalfLength =
                    positiveFirstLength + positiveSecondLength;
                FrontPathLength = NegativeHalfLength + PositiveHalfLength;
                FrontSplit = NegativeHalfLength /
                    Mathf.Max(0.001f, FrontPathLength);
                NegativeFirstSegmentSplit = negativeFirstLength /
                    Mathf.Max(0.001f, NegativeHalfLength);
                PositiveFirstSegmentSplit = positiveFirstLength /
                    Mathf.Max(0.001f, PositiveHalfLength);
                MinimumX = Mathf.Min(
                    point0.x,
                    Mathf.Min(
                        point1.x,
                        Mathf.Min(point2.x, Mathf.Min(point3.x, point4.x))));
                MaximumX = Mathf.Max(
                    point0.x,
                    Mathf.Max(
                        point1.x,
                        Mathf.Max(point2.x, Mathf.Max(point3.x, point4.x))));
                MaximumAbsoluteY = Mathf.Max(
                    Mathf.Abs(point0.y),
                    Mathf.Max(
                        Mathf.Abs(point1.y),
                        Mathf.Max(
                            Mathf.Abs(point2.y),
                            Mathf.Max(Mathf.Abs(point3.y), Mathf.Abs(point4.y)))));
            }

            public Vector2 Point0 { get; }
            public Vector2 Point1 { get; }
            public Vector2 Point2 { get; }
            public Vector2 Point3 { get; }
            public Vector2 Point4 { get; }
            public float NegativeHalfLength { get; }
            public float PositiveHalfLength { get; }
            public float FrontPathLength { get; }
            public float FrontSplit { get; }
            public float NegativeFirstSegmentSplit { get; }
            public float PositiveFirstSegmentSplit { get; }
            public float MinimumX { get; }
            public float MaximumX { get; }
            public float MaximumAbsoluteY { get; }
            public bool IsValid =>
                NegativeHalfLength > 0.001f &&
                PositiveHalfLength > 0.001f &&
                FrontPathLength > 0.002f;
        }

        private enum AutomaticFreeWaterSourceRecipe
        {
            LaceConnector,
            CrossLaceConnector,
            TornFragment
        }

        private readonly struct AutomaticFreeWaterSourceProfile
        {
            public AutomaticFreeWaterSourceProfile(
                bool enabled,
                float coverage,
                float activity,
                float formationSpeedMetresPerSecond,
                StylizedRiverFoamFreeWaterPattern pattern)
            {
                Enabled = enabled;
                Coverage = Mathf.Clamp01(coverage);
                Activity = Mathf.Clamp01(activity);
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond);
                Pattern = pattern;
            }

            public bool Enabled { get; }
            public float Coverage { get; }
            public float Activity { get; }
            public float FormationSpeedMetresPerSecond { get; }
            public StylizedRiverFoamFreeWaterPattern Pattern { get; }

            public float SlotSpacingMetres =>
                AutomaticFreeWaterSourceSlotSpacingMetres;

            public float EventsPerSecond =>
                AutomaticFreeWaterSourceMaximumEventsPerSecond * Activity;
        }
        private bool IsAutomaticSourcePopulationActive =>
            river != null && river.FoamEnabled &&
            river.FoamAutomaticBirthEnabled &&
            river.FreezeAmount < 0.999f && river.Domain.IsValid &&
            ((river.FoamAutomaticShoreBirthActive &&
              river.FoamShoreFoamCoverage > 0.0001f &&
              river.FoamShoreFoamActivity > 0.0001f) ||
             (river.FoamAutomaticObjectBirthActive &&
              ((river.FoamObjectContactCyclesEnabled &&
                river.FoamObjectContactCycleCoverage > 0.0001f) ||
               (river.FoamObjectFoamCoverage > 0.0001f &&
                river.FoamObjectFoamActivity > 0.0001f))) ||
             (river.FoamAutomaticFreeWaterBirthActive &&
              river.FoamFreeWaterFoamCoverage > 0.0001f &&
              river.FoamFreeWaterFoamActivity > 0.0001f));

        private bool AdvanceAutomaticBirthSources(
            float deltaTime,
            float now)
        {
            bool startedAny = false;
            startedAny |= AdvanceAutomaticShoreBirthSources(deltaTime, now);
            startedAny |= AdvanceAutomaticObjectBirthSources(deltaTime);
            startedAny |= AdvanceAutomaticFreeWaterBirthSources(deltaTime, now);
            return startedAny;
        }

        private bool AdvanceAutomaticShoreBirthSources(
            float deltaTime,
            float now)
        {
            automaticShoreBirthSubmittedLastUpdate = 0;
            automaticShoreBirthRejectedLastUpdate = 0;

            if (!ResolveAutomaticShoreSourceProfile(
                    out AutomaticShoreSourceProfile shoreProfile,
                    out string inactiveStatus))
            {
                automaticShoreBirthAccumulator = 0f;
                automaticShoreBirthStatus = inactiveStatus;
                return false;
            }

            automaticShoreBirthAccumulator += Mathf.Max(0f, deltaTime) *
                shoreProfile.EventsPerSecond;
            if (automaticShoreBirthAccumulator < 1f)
            {
                float secondsUntilNext =
                    (1f - automaticShoreBirthAccumulator) /
                    Mathf.Max(0.01f, shoreProfile.EventsPerSecond);
                automaticShoreBirthStatus =
                    $"Armed / {river.FoamSourcePopulationPreset} / next shore source event in {secondsUntilNext:0.00}s";
                return false;
            }

            int startsThisUpdate = 0;
            int skippedThisUpdate = 0;
            while (automaticShoreBirthAccumulator >= 1f &&
                   startsThisUpdate < AutomaticShoreSourceMaximumStartsPerUpdate)
            {
                if (TryStartAutomaticShoreSourceEvent(
                        shoreProfile,
                        now,
                        out int skippedSlots))
                {
                    automaticShoreBirthAccumulator -= 1f;
                    startsThisUpdate++;
                    skippedThisUpdate += skippedSlots;
                    continue;
                }

                automaticShoreBirthAccumulator = Mathf.Min(
                    automaticShoreBirthAccumulator,
                    0.999f);
                skippedThisUpdate += skippedSlots;
                break;
            }

            automaticShoreBirthSubmittedLastUpdate = startsThisUpdate;
            automaticShoreBirthRejectedLastUpdate = skippedThisUpdate;
            automaticShoreBirthSubmittedTotal += startsThisUpdate;
            automaticShoreBirthStatus = startsThisUpdate > 0
                ? $"Started {startsThisUpdate} deterministic shore source event(s), skipped {skippedThisUpdate} slot(s)"
                : $"Scanned deterministic shore source slots, started 0, skipped {skippedThisUpdate}";
            return startsThisUpdate > 0;
        }

        private bool ResolveAutomaticShoreSourceProfile(
            out AutomaticShoreSourceProfile profile,
            out string inactiveStatus)
        {
            profile = default;
            if (river == null || !river.FoamEnabled)
            {
                inactiveStatus = "Foam disabled";
                return false;
            }

            if (!river.FoamAutomaticBirthEnabled)
            {
                inactiveStatus = "Automatic source population disabled";
                return false;
            }

            if (river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                inactiveStatus = "Waiting for active river domain";
                return false;
            }

            if (fieldWidth <= 0 || fieldHeight <= 0 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                inactiveStatus = "Waiting for Foam field resources";
                return false;
            }

            if (river.FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.Off)
            {
                inactiveStatus = "Source population preset Off";
                return false;
            }

            if (!river.FoamSourcePopulationPresetImplemented)
            {
                inactiveStatus =
                    $"Preset {river.FoamSourcePopulationPreset} is documented but not implemented yet";
                return false;
            }

            if (!river.FoamAutomaticShoreBirthActive)
            {
                inactiveStatus = "Shore/contact source class disabled";
                return false;
            }

            float coverage = river.FoamShoreFoamCoverage;
            float activity = river.FoamShoreFoamActivity;
            if (coverage <= 0.0001f)
            {
                inactiveStatus = "Shore foam coverage is zero";
                return false;
            }

            if (activity <= 0.0001f)
            {
                inactiveStatus = "Shore foam activity is zero";
                return false;
            }

            profile = new AutomaticShoreSourceProfile(
                true,
                coverage,
                activity,
                river.FoamShoreFoamPatchSize,
                river.FoamShoreFoamFormationSpeedMetresPerSecond,
                river.FoamShoreFoamPattern);
            inactiveStatus = string.Empty;
            return profile.Enabled;
        }


        private bool AdvanceAutomaticObjectBirthSources(
            float deltaTime)
        {
            automaticObjectBirthSubmittedLastUpdate = 0;
            automaticObjectBirthRejectedLastUpdate = 0;
            automaticObjectBirthAnchorCountLastUpdate = 0;
            automaticObjectContactCycleTime += Mathf.Max(0f, deltaTime);
            RefreshAutomaticObjectPatternAuthority();
            RefreshAutomaticObjectClearanceAuthority();

            if (!ResolveAutomaticObjectSourceProfile(
                    out AutomaticObjectSourceProfile objectProfile,
                    out string inactiveStatus))
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus = inactiveStatus;
                RefreshAutomaticObjectSourcePacketDiagnostics();
                return false;
            }

            disturbanceRuntime ??= GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime == null)
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus = "Waiting for disturbance runtime";
                RefreshAutomaticObjectSourcePacketDiagnostics();
                return false;
            }

            disturbanceRuntime.CopyStaticObjectFoamSourcesTo(
                automaticObjectFoamSources);
            automaticObjectBirthAnchorCountLastUpdate =
                automaticObjectFoamSources.Count;
            SynchronizeAutomaticObjectSourceStates();
            if (automaticObjectFoamSources.Count <= 0)
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus =
                    "No registered static object source anchors";
                RefreshAutomaticObjectSourcePacketDiagnostics();
                return false;
            }

            float fleckRateScale = ResolveAutomaticObjectFleckRateScale(
                objectProfile.Pattern);
            float fleckEventsPerSecond = objectProfile.EventsPerSecond *
                fleckRateScale;
            if (fleckEventsPerSecond > 0.0001f)
            {
                automaticObjectBirthAccumulator += Mathf.Max(0f, deltaTime) *
                    fleckEventsPerSecond;
            }
            else
            {
                automaticObjectBirthAccumulator = 0f;
            }

            int cycleStarts = 0;
            int fleckStarts = 0;
            int skippedThisUpdate = 0;
            if (river.FoamObjectContactCyclesEnabled)
            {
                while (cycleStarts < AutomaticObjectSourceMaximumStartsPerUpdate &&
                       TryStartAutomaticObjectContactCycle(
                           objectProfile,
                           out int skippedObjects))
                {
                    cycleStarts++;
                    skippedThisUpdate += skippedObjects;
                }
            }

            if (fleckEventsPerSecond > 0.0001f)
            {
                while (automaticObjectBirthAccumulator >= 1f &&
                       cycleStarts + fleckStarts <
                           AutomaticObjectSourceMaximumStartsPerUpdate)
                {
                    if (TryStartAutomaticObjectFleckEvent(
                            objectProfile,
                            out int skippedObjects))
                    {
                        automaticObjectBirthAccumulator -= 1f;
                        fleckStarts++;
                        skippedThisUpdate += skippedObjects;
                        continue;
                    }

                    automaticObjectBirthAccumulator = Mathf.Min(
                        automaticObjectBirthAccumulator,
                        0.999f);
                    skippedThisUpdate += skippedObjects;
                    break;
                }
            }

            int startsThisUpdate = cycleStarts + fleckStarts;
            automaticObjectBirthSubmittedLastUpdate = startsThisUpdate;
            automaticObjectBirthRejectedLastUpdate = skippedThisUpdate;
            automaticObjectBirthSubmittedTotal += startsThisUpdate;
            RefreshAutomaticObjectSourcePacketDiagnostics();
            automaticObjectBirthStatus =
                $"Object packets {automaticObjectContactBuildCount} building / " +
                $"{automaticObjectWaitingClearanceCount} waiting for clearance; " +
                $"started {cycleStarts} Arc/Semi-Arc + {fleckStarts} Fleck, " +
                $"skipped {skippedThisUpdate}";
            return startsThisUpdate > 0;
        }

        private void RefreshAutomaticObjectPatternAuthority()
        {
            if (river == null)
            {
                return;
            }

            int signature;
            unchecked
            {
                signature = 17;
                signature = signature * 31 +
                    (int)river.FoamObjectFoamPattern;
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObjectContactArcPatternWeight * 10000f);
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObjectContactSemiArcPatternWeight * 10000f);
            }

            if (automaticObjectPatternAuthoritySignature == int.MinValue)
            {
                automaticObjectPatternAuthoritySignature = signature;
                return;
            }

            if (automaticObjectPatternAuthoritySignature == signature)
            {
                return;
            }

            automaticObjectPatternAuthoritySignature = signature;
            automaticObjectBirthAccumulator = 0f;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active || !IsAutomaticObjectSourceType(sourceEvent.Type))
                {
                    continue;
                }

                automaticFoamSourceEvents[index] = default;
                automaticFoamSourceEventGpuData[index] = default;
                activeAutomaticFoamSourceEventCount = Mathf.Max(
                    0,
                    activeAutomaticFoamSourceEventCount - 1);
            }

            automaticObjectContactStaleSourceIds.Clear();
            foreach (EntityId sourceId in automaticObjectSourceStates.Keys)
            {
                automaticObjectContactStaleSourceIds.Add(sourceId);
            }

            for (int index = 0;
                 index < automaticObjectContactStaleSourceIds.Count;
                 index++)
            {
                EntityId sourceId = automaticObjectContactStaleSourceIds[index];
                AutomaticObjectSourceState state =
                    automaticObjectSourceStates[sourceId];
                state.NextStartTime = automaticObjectContactCycleTime;
                state.LastEventType = AutomaticFoamSourceEventType.None;
                automaticObjectSourceStates[sourceId] = state;
            }
            automaticObjectContactStaleSourceIds.Clear();
        }

        private void RefreshAutomaticObjectClearanceAuthority()
        {
            if (river == null)
            {
                return;
            }

            int signature;
            unchecked
            {
                signature = 29;
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObjectContactMinimumPacketGapMetres * 1000f);
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObstacleSlowdownStrength * 10000f);
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObstacleMinimumDownstreamFactor * 10000f);
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObjectContactSlowdownOuterReachMetres * 1000f);
                signature = signature * 31 + Mathf.RoundToInt(
                    ResolveBaseFoamDownstreamSpeedMetresPerSecond() * 1000f);
            }

            if (automaticObjectClearanceAuthoritySignature == int.MinValue)
            {
                automaticObjectClearanceAuthoritySignature = signature;
                return;
            }

            if (automaticObjectClearanceAuthoritySignature == signature)
            {
                return;
            }

            automaticObjectClearanceAuthoritySignature = signature;
            automaticObjectContactStaleSourceIds.Clear();
            foreach (EntityId sourceId in automaticObjectSourceStates.Keys)
            {
                if (!HasActiveAutomaticObjectSource(sourceId))
                {
                    automaticObjectContactStaleSourceIds.Add(sourceId);
                }
            }

            for (int index = 0;
                 index < automaticObjectContactStaleSourceIds.Count;
                 index++)
            {
                EntityId sourceId = automaticObjectContactStaleSourceIds[index];
                AutomaticObjectSourceState state =
                    automaticObjectSourceStates[sourceId];
                state.NextStartTime = automaticObjectContactCycleTime;
                automaticObjectSourceStates[sourceId] = state;
            }
            automaticObjectContactStaleSourceIds.Clear();
        }

        private bool ResolveAutomaticObjectSourceProfile(
            out AutomaticObjectSourceProfile profile,
            out string inactiveStatus)
        {
            profile = default;
            if (river == null || !river.FoamEnabled)
            {
                inactiveStatus = "Foam disabled";
                return false;
            }

            if (!river.FoamAutomaticBirthEnabled)
            {
                inactiveStatus = "Automatic source population disabled";
                return false;
            }

            if (river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                inactiveStatus = "Waiting for active river domain";
                return false;
            }

            if (fieldWidth <= 0 || fieldHeight <= 0 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                inactiveStatus = "Waiting for Foam field resources";
                return false;
            }

            if (river.FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.Off)
            {
                inactiveStatus = "Source population preset Off";
                return false;
            }

            if (!river.FoamAutomaticObjectBirthActive)
            {
                inactiveStatus = "Object source class disabled";
                return false;
            }

            float coverage = river.FoamObjectFoamCoverage;
            float activity = river.FoamObjectFoamActivity;
            bool contactCyclesEnabled = river.FoamObjectContactCyclesEnabled &&
                river.FoamObjectContactCycleCoverage > 0.0001f;
            bool flecksEnabled = coverage > 0.0001f && activity > 0.0001f;
            if (!contactCyclesEnabled && !flecksEnabled)
            {
                inactiveStatus = "Contact-cycle Anchor Coverage and Fleck population are both zero";
                return false;
            }

            profile = new AutomaticObjectSourceProfile(
                true,
                coverage,
                activity,
                river.FoamObjectFoamFormationSpeedMetresPerSecond,
                river.FoamObjectFoamPattern);
            inactiveStatus = string.Empty;
            return profile.Enabled;
        }

        private bool TryStartAutomaticObjectContactCycle(
            AutomaticObjectSourceProfile profile,
            out int skippedObjects)
        {
            skippedObjects = 0;
            int sourceCount = automaticObjectFoamSources.Count;
            if (river == null || !river.Domain.IsValid || sourceCount <= 0)
            {
                return false;
            }

            int scanBudget = Mathf.Min(
                Mathf.Max(1, sourceCount),
                AutomaticObjectSourceMaximumScansPerUpdate);
            for (int scan = 0; scan < scanBudget; scan++)
            {
                int cursor = automaticObjectBirthCursor++;
                int cyclePermutation = cursor / Mathf.Max(1, sourceCount);
                int scanIndex = PositiveModulo(cursor, sourceCount);
                int sourceIndex = ResolvePermutedAutomaticObjectSourceIndex(
                    scanIndex,
                    sourceCount,
                    cyclePermutation);
                RiverFoamStaticObjectSource source =
                    automaticObjectFoamSources[sourceIndex];
                float identitySeed = ResolveAutomaticObjectIdentitySeed(source);
                if (Hash01(identitySeed + 1.7f) >
                    river.FoamObjectContactCycleCoverage)
                {
                    skippedObjects++;
                    continue;
                }

                if (!automaticObjectSourceStates.TryGetValue(
                        source.SourceId,
                        out AutomaticObjectSourceState state))
                {
                    state = CreateInitialAutomaticObjectSourceState();
                    automaticObjectSourceStates[source.SourceId] = state;
                }

                if (HasActiveAutomaticObjectSource(source.SourceId) ||
                    automaticObjectContactCycleTime + 0.0001f <
                        state.NextStartTime)
                {
                    skippedObjects++;
                    continue;
                }

                bool fleckDue = automaticObjectBirthAccumulator >= 1f &&
                    ResolveAutomaticObjectFleckRateScale(profile.Pattern) > 0.0001f &&
                    Hash01(identitySeed + 1.7f) <= profile.Coverage &&
                    state.LastEventType !=
                        AutomaticFoamSourceEventType.ObjectContactFleck;
                if (fleckDue)
                {
                    // A pending supplemental Fleck may take this eligible slot,
                    // but a completed Fleck always yields the next opportunity
                    // back to Arc/Semi-Arc so high Activity cannot starve cycles.
                    skippedObjects++;
                    continue;
                }

                float cycleSeed = identitySeed + state.CycleIndex * 37.613f;
                AutomaticObjectSourceRecipe recipe =
                    ResolveAutomaticObjectContactCycleRecipe(
                        profile.Pattern,
                        cycleSeed);
                if (TryBeginAutomaticObjectSourceEvent(
                        profile,
                        recipe,
                        source,
                        cycleSeed))
                {
                    state.CycleIndex++;
                    state.NextStartTime = float.PositiveInfinity;
                    automaticObjectSourceStates[source.SourceId] = state;
                    idleSince = 0.0;
                    return true;
                }

                skippedObjects++;
            }

            return false;
        }

        private bool TryStartAutomaticObjectFleckEvent(
            AutomaticObjectSourceProfile profile,
            out int skippedObjects)
        {
            skippedObjects = 0;
            int sourceCount = automaticObjectFoamSources.Count;
            if (river == null || !river.Domain.IsValid || sourceCount <= 0)
            {
                return false;
            }

            int scanBudget = Mathf.Min(
                Mathf.Max(1, sourceCount),
                AutomaticObjectSourceMaximumScansPerUpdate);
            for (int scan = 0; scan < scanBudget; scan++)
            {
                int cursor = automaticObjectBirthCursor++;
                int cycleIndex = cursor / Mathf.Max(1, sourceCount);
                int scanIndex = PositiveModulo(cursor, sourceCount);
                int sourceIndex = ResolvePermutedAutomaticObjectSourceIndex(
                    scanIndex,
                    sourceCount,
                    cycleIndex);
                RiverFoamStaticObjectSource source =
                    automaticObjectFoamSources[sourceIndex];
                float identitySeed = ResolveAutomaticObjectIdentitySeed(source);
                if (Hash01(identitySeed + 1.7f) > profile.Coverage)
                {
                    skippedObjects++;
                    continue;
                }

                if (!automaticObjectSourceStates.TryGetValue(
                        source.SourceId,
                        out AutomaticObjectSourceState state))
                {
                    state = CreateInitialAutomaticObjectSourceState();
                    automaticObjectSourceStates[source.SourceId] = state;
                }

                bool contactCycleEligible =
                    river.FoamObjectContactCyclesEnabled &&
                    Hash01(identitySeed + 1.7f) <=
                        river.FoamObjectContactCycleCoverage;
                if (HasActiveAutomaticObjectSource(source.SourceId) ||
                    automaticObjectContactCycleTime + 0.0001f <
                        state.NextStartTime ||
                    (contactCycleEligible &&
                     state.LastEventType ==
                        AutomaticFoamSourceEventType.ObjectContactFleck))
                {
                    skippedObjects++;
                    continue;
                }

                float sourceSeed = identitySeed + state.CycleIndex * 53.137f;
                if (TryBeginAutomaticObjectSourceEvent(
                        profile,
                        AutomaticObjectSourceRecipe.ContactFleck,
                        source,
                        sourceSeed))
                {
                    state.CycleIndex++;
                    state.NextStartTime = float.PositiveInfinity;
                    automaticObjectSourceStates[source.SourceId] = state;
                    idleSince = 0.0;
                    return true;
                }

                skippedObjects++;
            }

            return false;
        }

        private AutomaticObjectSourceRecipe ResolveAutomaticObjectContactCycleRecipe(
            StylizedRiverFoamObjectPattern pattern,
            float seed)
        {
            if (pattern == StylizedRiverFoamObjectPattern.ContactArcs)
            {
                return AutomaticObjectSourceRecipe.ContactArc;
            }

            if (pattern == StylizedRiverFoamObjectPattern.ContactSemiArcs)
            {
                return AutomaticObjectSourceRecipe.ContactSemiArc;
            }

            float arcWeight = river != null
                ? Mathf.Max(0f, river.FoamObjectContactArcPatternWeight)
                : 0.45f;
            float semiArcWeight = river != null
                ? Mathf.Max(0f, river.FoamObjectContactSemiArcPatternWeight)
                : 0.35f;
            float totalWeight = arcWeight + semiArcWeight;
            if (totalWeight <= 0.0001f)
            {
                return AutomaticObjectSourceRecipe.ContactArc;
            }

            return Hash01(seed + 4.1f) * totalWeight < arcWeight
                ? AutomaticObjectSourceRecipe.ContactArc
                : AutomaticObjectSourceRecipe.ContactSemiArc;
        }

        private float ResolveAutomaticObjectFleckRateScale(
            StylizedRiverFoamObjectPattern pattern)
        {
            return pattern == StylizedRiverFoamObjectPattern.Mixed ||
                pattern == StylizedRiverFoamObjectPattern.ContactFlecks
                    ? 1f
                    : 0f;
        }

        private float ResolveAutomaticObjectIdentitySeed(
            RiverFoamStaticObjectSource source)
        {
            return river.VisualSeed * 0.191f +
                source.SourceId.GetHashCode() * 0.017f +
                source.Phase * 11.0f;
        }

        private AutomaticObjectSourceState CreateInitialAutomaticObjectSourceState()
        {
            return new AutomaticObjectSourceState
            {
                CycleIndex = 0,
                NextStartTime = automaticObjectContactCycleTime,
                LastEventType = AutomaticFoamSourceEventType.None
            };
        }

        private static bool IsAutomaticObjectSourceType(
            AutomaticFoamSourceEventType sourceType)
        {
            return sourceType == AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceType == AutomaticFoamSourceEventType.ObjectContactSemiArc ||
                sourceType == AutomaticFoamSourceEventType.ObjectContactFleck;
        }

        private bool HasActiveAutomaticObjectSource(EntityId sourceId)
        {
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (sourceEvent.Active &&
                    IsAutomaticObjectSourceType(sourceEvent.Type) &&
                    sourceEvent.ObjectSourceId.Equals(sourceId))
                {
                    return true;
                }
            }

            return false;
        }

        private void CompleteAutomaticObjectSourceEvent(
            AutomaticFoamSourceEvent sourceEvent)
        {
            if (!IsAutomaticObjectSourceType(sourceEvent.Type))
            {
                return;
            }

            automaticObjectSourceStates.TryGetValue(
                sourceEvent.ObjectSourceId,
                out AutomaticObjectSourceState state);
            state.LastEventType = sourceEvent.Type;
            float clearanceSeconds = ResolveAutomaticObjectPacketClearanceSeconds();
            state.NextStartTime = float.IsPositiveInfinity(clearanceSeconds)
                ? float.PositiveInfinity
                : automaticObjectContactCycleTime + clearanceSeconds;
            automaticObjectSourceStates[sourceEvent.ObjectSourceId] = state;
        }

        private void SynchronizeAutomaticObjectSourceStates()
        {
            automaticObjectContactLiveSourceIds.Clear();
            for (int index = 0; index < automaticObjectFoamSources.Count; index++)
            {
                EntityId sourceId = automaticObjectFoamSources[index].SourceId;
                automaticObjectContactLiveSourceIds.Add(sourceId);
                if (!automaticObjectSourceStates.ContainsKey(sourceId))
                {
                    automaticObjectSourceStates.Add(
                        sourceId,
                        CreateInitialAutomaticObjectSourceState());
                }
            }

            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (sourceEvent.Active &&
                    IsAutomaticObjectSourceType(sourceEvent.Type))
                {
                    automaticObjectContactLiveSourceIds.Add(
                        sourceEvent.ObjectSourceId);
                }
            }

            automaticObjectContactStaleSourceIds.Clear();
            foreach (KeyValuePair<EntityId, AutomaticObjectSourceState> pair
                     in automaticObjectSourceStates)
            {
                if (!automaticObjectContactLiveSourceIds.Contains(pair.Key))
                {
                    automaticObjectContactStaleSourceIds.Add(pair.Key);
                }
            }

            for (int index = 0;
                 index < automaticObjectContactStaleSourceIds.Count;
                 index++)
            {
                automaticObjectSourceStates.Remove(
                    automaticObjectContactStaleSourceIds[index]);
            }
            automaticObjectContactStaleSourceIds.Clear();
        }

        private void RefreshAutomaticObjectSourcePacketDiagnostics()
        {
            automaticObjectContactBuildCount = 0;
            automaticObjectContactFleckCount = 0;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (!sourceEvent.Active)
                {
                    continue;
                }

                if (sourceEvent.Type ==
                        AutomaticFoamSourceEventType.ObjectContactArc ||
                    sourceEvent.Type ==
                        AutomaticFoamSourceEventType.ObjectContactSemiArc)
                {
                    automaticObjectContactBuildCount++;
                }
                else if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ObjectContactFleck)
                {
                    automaticObjectContactFleckCount++;
                }
            }

            automaticObjectWaitingClearanceCount = 0;
            foreach (KeyValuePair<EntityId, AutomaticObjectSourceState> pair
                     in automaticObjectSourceStates)
            {
                if (!HasActiveAutomaticObjectSource(pair.Key) &&
                    pair.Value.NextStartTime > automaticObjectContactCycleTime)
                {
                    automaticObjectWaitingClearanceCount++;
                }
            }
        }

        private static ResolvedAutomaticObjectContactProfile
            ResolveAutomaticObjectContactProfile(
                RiverFoamStaticObjectSource source,
                float alongFlowOffsetMetres,
                float acrossRiverOffsetMetres)
        {
            RiverFoamStaticContactProfile sourceProfile =
                source.ContactProfile.IsValid
                    ? source.ContactProfile
                    : RiverDisturbanceFootprintResolver
                        .BuildFallbackFoamContactProfile(
                            source.StaticPressureAlongHalfLength,
                            source.StaticPressureAcrossHalfWidth);

            Vector2 point0 = sourceProfile.Point0;
            Vector2 point1 = sourceProfile.Point1;
            Vector2 point2 = sourceProfile.Point2;
            Vector2 point3 = sourceProfile.Point3;
            Vector2 point4 = sourceProfile.Point4;

            const float minimumScale = 0.01f;
            float frontAcross = point2.y;
            float negativeSpan = Mathf.Max(
                0.005f,
                frontAcross - point0.y);
            float positiveSpan = Mathf.Max(
                0.005f,
                point4.y - frontAcross);
            float negativeScale = Mathf.Max(
                minimumScale,
                (negativeSpan + acrossRiverOffsetMetres) / negativeSpan);
            float positiveScale = Mathf.Max(
                minimumScale,
                (positiveSpan + acrossRiverOffsetMetres) / positiveSpan);
            point0.y = frontAcross +
                (point0.y - frontAcross) * negativeScale;
            point1.y = frontAcross +
                (point1.y - frontAcross) * negativeScale;
            point3.y = frontAcross +
                (point3.y - frontAcross) * positiveScale;
            point4.y = frontAcross +
                (point4.y - frontAcross) * positiveScale;

            float shoulderAcrossSpan = Mathf.Max(
                0.005f,
                point4.y - point0.y);
            Vector2[] points =
            {
                point0, point1, point2, point3, point4
            };
            float maximumFrontDepth = 0.005f;
            for (int index = 0; index < points.Length; index++)
            {
                float shoulderInterpolation = Mathf.Clamp01(
                    (points[index].y - point0.y) / shoulderAcrossSpan);
                float shoulderBaseline = Mathf.Lerp(
                    point0.x,
                    point4.x,
                    shoulderInterpolation);
                maximumFrontDepth = Mathf.Max(
                    maximumFrontDepth,
                    shoulderBaseline - points[index].x);
            }

            float targetFrontDepth = Mathf.Max(
                0.005f,
                maximumFrontDepth + alongFlowOffsetMetres);
            float alongScale = targetFrontDepth / maximumFrontDepth;
            for (int index = 0; index < points.Length; index++)
            {
                float shoulderInterpolation = Mathf.Clamp01(
                    (points[index].y - point0.y) / shoulderAcrossSpan);
                float shoulderBaseline = Mathf.Lerp(
                    point0.x,
                    point4.x,
                    shoulderInterpolation);
                float frontDepth = Mathf.Max(
                    0f,
                    shoulderBaseline - points[index].x);
                points[index].x = shoulderBaseline - frontDepth * alongScale;
            }


            return new ResolvedAutomaticObjectContactProfile(
                points[0],
                points[1],
                points[2],
                points[3],
                points[4]);
        }

        private bool TryBeginAutomaticObjectSourceEvent(
            AutomaticObjectSourceProfile profile,
            AutomaticObjectSourceRecipe recipe,
            RiverFoamStaticObjectSource source,
            float seed)
        {
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float sourceKey = river.VisualSeed * 0.417f +
                source.GlobalDistance * 9.731f +
                source.AcrossMetres * 19.137f +
                source.SourceId.GetHashCode() * 0.011f +
                (recipe == AutomaticObjectSourceRecipe.ContactFleck
                    ? 907f
                    : (recipe == AutomaticObjectSourceRecipe.ContactSemiArc ? 809f : 701f));

            float length = 0f;
            float width = 0f;
            float offset = 0f;
            float amount;
            float remainingLife;
            float breakupScale = 0f;
            float breakupStrength = 0f;
            float patternFormationSpeedMultiplier;
            float lopsidedness = 0f;
            float objectWakeArmLengthMetres = 0f;
            float objectSourceLateralCellSpacingMetres = 0f;
            float objectAlongHalfLengthMetres = 0f;
            float objectAcrossHalfWidthMetres = 0f;
            float sourcePathDistance;
            ResolvedAutomaticObjectContactProfile resolvedContactProfile = default;
            float startGlobalDistance;
            float endGlobalDistance;

            if (recipe == AutomaticObjectSourceRecipe.ContactFleck)
            {
                float eventScale = Hash01(seed + 6.5f);
                float widthJitter = Mathf.Lerp(0.92f, 1.08f, Hash01(seed + 7.1f));
                float offsetJitter = Mathf.Lerp(0.85f, 1.15f, Hash01(seed + 8.3f));
                length = Mathf.Lerp(
                    river.FoamObjectContactFleckLengthMinMetres,
                    river.FoamObjectContactFleckLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamObjectContactFleckWidthMinMetres,
                    river.FoamObjectContactFleckWidthMaxMetres,
                    eventScale) * widthJitter;
                offset = Mathf.Lerp(
                    river.FoamObjectContactFleckOffsetMinMetres,
                    river.FoamObjectContactFleckOffsetMaxMetres,
                    eventScale) * offsetJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamObjectContactFleckInitialLifeMin,
                    river.FoamObjectContactFleckInitialLifeMax,
                    eventScale);
                breakupScale = 0f;
                breakupStrength = 0f;
                patternFormationSpeedMultiplier =
                    river.FoamObjectContactFleckFormationSpeedMultiplier;
                amount = Mathf.Lerp(
                    river.FoamObjectContactFleckInitialPresenceMin,
                    river.FoamObjectContactFleckInitialPresenceMax,
                    eventScale);

                length = Mathf.Clamp(
                    length,
                    0.05f,
                    Mathf.Max(0.05f, source.StaticPressureAcrossHalfWidth * 2.6f));
                width = Mathf.Clamp(
                    width,
                    0.012f,
                    Mathf.Max(0.020f, length * 0.18f));
                offset = Mathf.Clamp(
                    offset,
                    0.0f,
                    Mathf.Max(0.01f, source.SurfaceHalfWidth * 0.10f));
                float halfLength = length * 0.5f;
                startGlobalDistance = Mathf.Clamp(
                    source.GlobalDistance - flowDirection * halfLength,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                endGlobalDistance = Mathf.Clamp(
                    source.GlobalDistance + flowDirection * halfLength,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                sourcePathDistance = Mathf.Abs(
                    endGlobalDistance - startGlobalDistance);
            }
            else
            {
                bool semiArc = recipe == AutomaticObjectSourceRecipe.ContactSemiArc;
                objectWakeArmLengthMetres = Mathf.Lerp(
                    semiArc
                        ? river.FoamObjectContactSemiArcWakeArmLengthMinMetres
                        : river.FoamObjectContactArcWakeArmLengthMinMetres,
                    semiArc
                        ? river.FoamObjectContactSemiArcWakeArmLengthMaxMetres
                        : river.FoamObjectContactArcWakeArmLengthMaxMetres,
                    Hash01(seed + 6.5f));
                amount = Mathf.Lerp(
                    semiArc
                        ? river.FoamObjectContactSemiArcInitialPresenceMin
                        : river.FoamObjectContactArcInitialPresenceMin,
                    semiArc
                        ? river.FoamObjectContactSemiArcInitialPresenceMax
                        : river.FoamObjectContactArcInitialPresenceMax,
                    Hash01(seed + 9.1f));
                remainingLife = Mathf.Lerp(
                    semiArc
                        ? river.FoamObjectContactSemiArcInitialLifeMin
                        : river.FoamObjectContactArcInitialLifeMin,
                    semiArc
                        ? river.FoamObjectContactSemiArcInitialLifeMax
                        : river.FoamObjectContactArcInitialLifeMax,
                    Hash01(seed + 10.3f));
                patternFormationSpeedMultiplier = semiArc
                    ? river.FoamObjectContactSemiArcFormationSpeedMultiplier
                    : river.FoamObjectContactArcFormationSpeedMultiplier;

                if (semiArc)
                {
                    // Semi-Arc selects exactly one physical front half and
                    // one straight downstream arm. Curvature carries only the
                    // deterministic selected-side sign; legacy Lopsidedness
                    // magnitude is no longer an active runtime authority.
                    lopsidedness = Hash01(seed + 13.9f) < 0.5f ? -1f : 1f;
                }

                float domainLength = Mathf.Max(
                    0.01f,
                    river.Domain.GlobalDistanceMaximum -
                    river.Domain.GlobalDistanceMinimum);
                float longitudinalCellSpacing = domainLength /
                    Mathf.Max(1, fieldWidth);
                float crossRiverCellSpacing = Mathf.Max(
                    0.01f,
                    source.SurfaceHalfWidth * 2f / Mathf.Max(1, fieldHeight));
                objectSourceLateralCellSpacingMetres = crossRiverCellSpacing;
                float alongContactOffsetMetres = semiArc
                    ? river.FoamObjectContactSemiArcAlongFlowContactOffsetMetres
                    : river.FoamObjectContactArcAlongFlowContactOffsetMetres;
                float acrossContactOffsetMetres = semiArc
                    ? river.FoamObjectContactSemiArcAcrossRiverContactOffsetMetres
                    : river.FoamObjectContactArcAcrossRiverContactOffsetMetres;
                resolvedContactProfile = ResolveAutomaticObjectContactProfile(
                    source,
                    alongContactOffsetMetres,
                    acrossContactOffsetMetres);
                if (!resolvedContactProfile.IsValid)
                {
                    foamCompositionRejectedCount++;
                    return false;
                }

                objectAlongHalfLengthMetres = Mathf.Max(
                    0.005f,
                    Mathf.Max(
                        Mathf.Abs(resolvedContactProfile.MinimumX),
                        Mathf.Abs(resolvedContactProfile.MaximumX)));
                objectAcrossHalfWidthMetres = Mathf.Max(
                    0.005f,
                    resolvedContactProfile.MaximumAbsoluteY);
                float dominantArmLength = Mathf.Max(
                    0.05f,
                    objectWakeArmLengthMetres);
                float negativeArmLength = semiArc && lopsidedness > 0f
                    ? 0f
                    : dominantArmLength;
                float positiveArmLength = semiArc && lopsidedness < 0f
                    ? 0f
                    : dominantArmLength;
                float selectedFrontLength = semiArc
                    ? (lopsidedness < 0f
                        ? resolvedContactProfile.NegativeHalfLength
                        : resolvedContactProfile.PositiveHalfLength)
                    : resolvedContactProfile.FrontPathLength;
                sourcePathDistance = Mathf.Max(
                    0.001f,
                    negativeArmLength +
                    selectedFrontLength +
                    positiveArmLength);

                float minimumLocalX;
                float maximumLocalX;
                float maximumAbsoluteY;
                if (semiArc && lopsidedness < 0f)
                {
                    Vector2 armTip = resolvedContactProfile.Point0 +
                        new Vector2(negativeArmLength, 0f);
                    minimumLocalX = Mathf.Min(
                        resolvedContactProfile.Point0.x,
                        Mathf.Min(
                            resolvedContactProfile.Point1.x,
                            resolvedContactProfile.Point2.x));
                    maximumLocalX = Mathf.Max(
                        armTip.x,
                        Mathf.Max(
                            resolvedContactProfile.Point0.x,
                            Mathf.Max(
                                resolvedContactProfile.Point1.x,
                                resolvedContactProfile.Point2.x)));
                    maximumAbsoluteY = Mathf.Max(
                        Mathf.Abs(resolvedContactProfile.Point0.y),
                        Mathf.Max(
                            Mathf.Abs(resolvedContactProfile.Point1.y),
                            Mathf.Abs(resolvedContactProfile.Point2.y)));
                }
                else if (semiArc)
                {
                    Vector2 armTip = resolvedContactProfile.Point4 +
                        new Vector2(positiveArmLength, 0f);
                    minimumLocalX = Mathf.Min(
                        resolvedContactProfile.Point2.x,
                        Mathf.Min(
                            resolvedContactProfile.Point3.x,
                            resolvedContactProfile.Point4.x));
                    maximumLocalX = Mathf.Max(
                        armTip.x,
                        Mathf.Max(
                            resolvedContactProfile.Point2.x,
                            Mathf.Max(
                                resolvedContactProfile.Point3.x,
                                resolvedContactProfile.Point4.x)));
                    maximumAbsoluteY = Mathf.Max(
                        Mathf.Abs(resolvedContactProfile.Point2.y),
                        Mathf.Max(
                            Mathf.Abs(resolvedContactProfile.Point3.y),
                            Mathf.Abs(resolvedContactProfile.Point4.y)));
                }
                else
                {
                    Vector2 negativeArmTip = resolvedContactProfile.Point0 +
                        new Vector2(negativeArmLength, 0f);
                    Vector2 positiveArmTip = resolvedContactProfile.Point4 +
                        new Vector2(positiveArmLength, 0f);
                    minimumLocalX = resolvedContactProfile.MinimumX;
                    maximumLocalX = Mathf.Max(
                        resolvedContactProfile.MaximumX,
                        Mathf.Max(negativeArmTip.x, positiveArmTip.x));
                    maximumAbsoluteY =
                        resolvedContactProfile.MaximumAbsoluteY;
                }

                startGlobalDistance = Mathf.Clamp(
                    source.GlobalDistance +
                    minimumLocalX - longitudinalCellSpacing,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                endGlobalDistance = Mathf.Clamp(
                    source.GlobalDistance +
                    maximumLocalX + longitudinalCellSpacing,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                objectAcrossHalfWidthMetres = Mathf.Max(
                    objectAcrossHalfWidthMetres,
                    maximumAbsoluteY);
            }

            if (sourcePathDistance <= 0.05f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            ResolvedAutomaticRevealTiming revealTiming =
                ResolveAutomaticRevealTiming(
                    sourcePathDistance,
                    profile.FormationSpeedMetresPerSecond,
                    patternFormationSpeedMultiplier,
                    Mathf.Lerp(0.90f, 1.10f, Hash01(seed + 12.5f)));
            float formationSpeed =
                revealTiming.RequestedSpeedMetresPerSecond;
            bool contactCycle = recipe != AutomaticObjectSourceRecipe.ContactFleck;
            float materialStepDuration = 1f / Mathf.Max(1f, ResolveUpdateRate());
            float feather = contactCycle
                ? 0f
                : Mathf.Clamp(
                    Mathf.Max(width * 0.65f, source.SurfaceHalfWidth * 0.010f),
                    0.020f,
                    0.110f);
            float headTrailMetres = Mathf.Clamp(
                Mathf.Max(feather * 1.35f, formationSpeed * materialStepDuration * 1.50f),
                AutomaticObjectSourceMinimumHeadTrailMetres,
                Mathf.Min(
                    AutomaticObjectSourceMaximumHeadTrailMetres,
                    Mathf.Max(
                        AutomaticObjectSourceMinimumHeadTrailMetres,
                        sourcePathDistance * 0.30f)));

            return BeginAutomaticObjectFoamSourceEvent(
                recipe,
                source,
                startGlobalDistance,
                endGlobalDistance,
                source.GlobalDistance,
                revealTiming,
                headTrailMetres,
                offset,
                width,
                feather,
                amount,
                remainingLife,
                sourceKey,
                breakupScale,
                breakupStrength,
                lopsidedness,
                objectAlongHalfLengthMetres,
                objectAcrossHalfWidthMetres,
                objectSourceLateralCellSpacingMetres,
                objectWakeArmLengthMetres,
                sourcePathDistance,
                resolvedContactProfile);
        }

        private bool BeginAutomaticObjectFoamSourceEvent(
            AutomaticObjectSourceRecipe recipe,
            RiverFoamStaticObjectSource source,
            float startGlobalDistance,
            float endGlobalDistance,
            float objectCentreGlobalDistance,
            ResolvedAutomaticRevealTiming revealTiming,
            float headTrailMetres,
            float contactOffsetMetres,
            float widthMetres,
            float featherMetres,
            float amount,
            float remainingLife,
            float sourceKey,
            float breakupScaleMetres,
            float breakupStrength,
            float lopsidedness,
            float objectAlongHalfLengthMetres,
            float objectAcrossHalfWidthMetres,
            float objectSourceLateralCellSpacingMetres,
            float objectWakeArmLengthMetres,
            float objectContactPathLengthMetres,
            ResolvedAutomaticObjectContactProfile contactProfile)
        {
            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeAutomaticFoamSourceSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            AutomaticFoamSourceEventType sourceType;
            switch (recipe)
            {
                case AutomaticObjectSourceRecipe.ContactFleck:
                    sourceType = AutomaticFoamSourceEventType.ObjectContactFleck;
                    break;
                case AutomaticObjectSourceRecipe.ContactSemiArc:
                    sourceType = AutomaticFoamSourceEventType.ObjectContactSemiArc;
                    break;
                default:
                    sourceType = AutomaticFoamSourceEventType.ObjectContactArc;
                    break;
            }

            bool contactCycle =
                sourceType == AutomaticFoamSourceEventType.ObjectContactArc ||
                sourceType == AutomaticFoamSourceEventType.ObjectContactSemiArc;
            float resolvedBuildDuration =
                revealTiming.ResolvedDurationSeconds;
            int objectContactStrokeCount = contactCycle && river != null
                ? river.FoamObjectContactStrokeCount
                : 1;
            float resolvedEventDuration = resolvedBuildDuration *
                Mathf.Max(1, objectContactStrokeCount);

            automaticFoamSourceEvents[slotIndex] = new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = eventId,
                Type = sourceType,
                ObjectSourceId = source.SourceId,
                SideSign = 1f,
                StartGlobalDistance = startGlobalDistance,
                EndGlobalDistance = endGlobalDistance,
                ObjectCentreGlobalDistance = objectCentreGlobalDistance,
                Duration = resolvedEventDuration,
                Elapsed = 0f,
                ObjectBuildDuration = resolvedBuildDuration,
                ObjectContactStrokeCount = objectContactStrokeCount,
                FormationSpeedMetresPerSecond =
                    revealTiming.RequestedSpeedMetresPerSecond,
                RevealPathDistanceMetres = revealTiming.PathDistanceMetres,
                RawRevealDurationSeconds = revealTiming.RawDurationSeconds,
                RevealCadenceLimited = revealTiming.CadenceLimited,
                HeadTrailMetres = Mathf.Clamp(
                    headTrailMetres,
                    AutomaticObjectSourceMinimumHeadTrailMetres,
                    AutomaticObjectSourceMaximumHeadTrailMetres),
                ShoreInsetMetres = Mathf.Max(0f, contactOffsetMetres),
                WidthMetres = contactCycle
                    ? 0f
                    : Mathf.Max(0.01f, widthMetres),
                InwardReachMetres = Mathf.Max(
                    0.01f,
                    Mathf.Max(source.StaticPressureAlongHalfLength, source.StaticPressureAcrossHalfWidth)),
                FeatherMetres = contactCycle
                    ? 0f
                    : Mathf.Max(0.01f, featherMetres),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticObjectBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticObjectBirthSourceFillSeedSalt,
                SourceFillFeatureSize = contactCycle
                    ? SourceFillMinimumFeatureSizeMetres
                    : Mathf.Max(
                        SourceFillMinimumFeatureSizeMetres * 0.55f,
                        Mathf.Max(widthMetres * 1.5f, featherMetres * 1.25f)),
                ShapeSeed = sourceKey + AutomaticObjectBirthShapeSeedSalt,
                BreakupScaleMetres = 0f,
                BreakupStrength = 0f,
                Curvature = Mathf.Clamp(lopsidedness, -1f, 1f),
                // Arc/Semi-Arc contact cycles must never receive breakup or patterned
                // source-fill holes inside their upstream bridge or straight wake arms. Flecks
                // retain their accepted stochastic fill variation.
                SourceFillBlend = sourceType == AutomaticFoamSourceEventType.ObjectContactFleck
                    ? 0.20f
                    : 0f,
                ObjectCentreAcrossMetres = source.AcrossMetres,
                ObjectAlongHalfLengthMetres = contactCycle
                    ? Mathf.Max(0.005f, objectAlongHalfLengthMetres)
                    : Mathf.Max(0.05f, source.StaticPressureAlongHalfLength),
                ObjectAcrossHalfWidthMetres = contactCycle
                    ? Mathf.Max(0.005f, objectAcrossHalfWidthMetres)
                    : Mathf.Max(0.05f, source.StaticPressureAcrossHalfWidth),
                ObjectContactOffsetMetres = contactCycle
                    ? 0f
                    : Mathf.Max(0f, contactOffsetMetres),
                ObjectSourceLateralCellSpacingMetres = contactCycle
                    ? Mathf.Max(0.01f, objectSourceLateralCellSpacingMetres)
                    : 0f,
                ObjectWakeArmLengthMetres = contactCycle
                    ? Mathf.Max(0.05f, objectWakeArmLengthMetres)
                    : 0f,
                ObjectContactPathLengthMetres = contactCycle
                    ? Mathf.Max(0.001f, objectContactPathLengthMetres)
                    : 0f,
                ObjectContactPoint0 = contactCycle
                    ? contactProfile.Point0
                    : Vector2.zero,
                ObjectContactPoint1 = contactCycle
                    ? contactProfile.Point1
                    : Vector2.zero,
                ObjectContactPoint2 = contactCycle
                    ? contactProfile.Point2
                    : Vector2.zero,
                ObjectContactPoint3 = contactCycle
                    ? contactProfile.Point3
                    : Vector2.zero,
                ObjectContactPoint4 = contactCycle
                    ? contactProfile.Point4
                    : Vector2.zero,
                ObjectContactFrontSplit = contactCycle
                    ? Mathf.Clamp(contactProfile.FrontSplit, 0.001f, 0.999f)
                    : 0.5f,
                ObjectContactNegativeFirstSegmentSplit = contactCycle
                    ? Mathf.Clamp(
                        contactProfile.NegativeFirstSegmentSplit,
                        0.001f,
                        0.999f)
                    : 0.5f,
                ObjectContactPositiveFirstSegmentSplit = contactCycle
                    ? Mathf.Clamp(
                        contactProfile.PositiveFirstSegmentSplit,
                        0.001f,
                        0.999f)
                    : 0.5f,
                CentreAcrossNormalized = contactCycle
                    ? Mathf.Clamp(source.AcrossNormalized, -1f, 1f)
                    : 0f,
                LateralPaddingMetres = contactCycle
                    ? Mathf.Max(
                        0.05f,
                        objectAcrossHalfWidthMetres +
                        objectSourceLateralCellSpacingMetres * 2f)
                    : 0f
            };

            RecordAutomaticRevealTiming(
                eventId,
                sourceType,
                revealTiming);
            activeAutomaticFoamSourceEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                GlobalDistanceToNormalized(startGlobalDistance);
            latestFoamCompositionPreviousDistanceNormalized =
                latestFoamCompositionHeadDistanceNormalized;
            latestFoamCompositionHeadAcrossNormalized = source.AcrossNormalized;
            latestFoamCompositionPreviousAcrossNormalized = source.AcrossNormalized;
            lastFoamCompositionSegmentLength = 0f;
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / automatic object source-event rasterizer";
            RecordMaterialBirthCommand();
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
            return true;
        }

        private float ResolveLatestAutomaticSourceEventDurationSeconds()
        {
            int eventId = latestFoamCompositionEventId;
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                AutomaticFoamSourceEvent sourceEvent =
                    automaticFoamSourceEvents[index];
                if (sourceEvent.Active && sourceEvent.EventId == eventId)
                {
                    return Mathf.Max(0f, sourceEvent.Duration);
                }
            }

            return 0f;
        }

        private float ResolveAutomaticObjectPacketClearanceSeconds()
        {
            if (river == null)
            {
                return float.PositiveInfinity;
            }

            float baseSpeed = ResolveBaseFoamDownstreamSpeedMetresPerSecond();
            if (baseSpeed <= 0.0001f)
            {
                return float.PositiveInfinity;
            }

            float contactSpeedFactor = ResolveObjectContactPacketSpeedFactor();
            if (contactSpeedFactor <= 0.0001f)
            {
                return float.PositiveInfinity;
            }

            float haloClearanceSeconds =
                river.FoamObjectContactSlowdownOuterReachMetres /
                Mathf.Max(0.0001f, baseSpeed * contactSpeedFactor);
            float gapClearanceSeconds =
                river.FoamObjectContactMinimumPacketGapMetres /
                Mathf.Max(0.0001f, baseSpeed);
            return Mathf.Max(0f, haloClearanceSeconds) +
                Mathf.Max(0f, gapClearanceSeconds);
        }

        private float ResolveAutomaticPacketClearanceSeconds(
            float packetGapMetres,
            float localSpeedFactor = 1f)
        {
            float downstreamSpeed = river != null
                ? Mathf.Abs(river.FlowSpeedMetresPerSecond) *
                    Mathf.Max(0f, river.FoamDownstreamSpeedRatio) *
                    Mathf.Max(0f, localSpeedFactor)
                : 0f;
            return Mathf.Max(0f, packetGapMetres) /
                Mathf.Max(
                    AutomaticPacketClearanceMinimumSpeedMetresPerSecond,
                    downstreamSpeed);
        }

        private float ResolveObjectContactPacketSpeedFactor()
        {
            if (river == null || river.FoamObstacleSlowdownStrength <= 0.0001f)
            {
                return 1f;
            }

            return Mathf.Clamp01(river.FoamObstacleMinimumDownstreamFactor);
        }



        private int ResolvePermutedAutomaticObjectSourceIndex(
            int scanIndex,
            int sourceCount,
            int cycleIndex)
        {
            if (sourceCount <= 1)
            {
                return 0;
            }

            int stride = ResolveCoprimeAutomaticSourceStride(
                sourceCount,
                cycleIndex + 73);
            int offset = PositiveModulo(
                Mathf.RoundToInt(Hash01(
                    river.VisualSeed * 0.193f + cycleIndex * 23.731f) *
                    sourceCount),
                sourceCount);
            return PositiveModulo(offset + scanIndex * stride, sourceCount);
        }

        private bool AdvanceAutomaticFreeWaterBirthSources(
            float deltaTime,
            float now)
        {
            automaticFreeWaterBirthSubmittedLastUpdate = 0;
            automaticFreeWaterBirthRejectedLastUpdate = 0;

            if (!ResolveAutomaticFreeWaterSourceProfile(
                    out AutomaticFreeWaterSourceProfile freeWaterProfile,
                    out string inactiveStatus))
            {
                automaticFreeWaterBirthAccumulator = 0f;
                automaticFreeWaterBirthStatus = inactiveStatus;
                return false;
            }

            automaticFreeWaterBirthAccumulator += Mathf.Max(0f, deltaTime) *
                freeWaterProfile.EventsPerSecond;
            if (automaticFreeWaterBirthAccumulator < 1f)
            {
                float secondsUntilNext =
                    (1f - automaticFreeWaterBirthAccumulator) /
                    Mathf.Max(0.01f, freeWaterProfile.EventsPerSecond);
                automaticFreeWaterBirthStatus =
                    $"Armed / next free-water source event in {secondsUntilNext:0.00}s";
                return false;
            }

            int startsThisUpdate = 0;
            int skippedThisUpdate = 0;
            while (automaticFreeWaterBirthAccumulator >= 1f &&
                   startsThisUpdate < AutomaticFreeWaterSourceMaximumStartsPerUpdate)
            {
                if (TryStartAutomaticFreeWaterSourceEvent(
                        freeWaterProfile,
                        now,
                        out int skippedSlots))
                {
                    automaticFreeWaterBirthAccumulator -= 1f;
                    startsThisUpdate++;
                    skippedThisUpdate += skippedSlots;
                    continue;
                }

                automaticFreeWaterBirthAccumulator = Mathf.Min(
                    automaticFreeWaterBirthAccumulator,
                    0.999f);
                skippedThisUpdate += skippedSlots;
                break;
            }

            automaticFreeWaterBirthSubmittedLastUpdate = startsThisUpdate;
            automaticFreeWaterBirthRejectedLastUpdate = skippedThisUpdate;
            automaticFreeWaterBirthSubmittedTotal += startsThisUpdate;
            automaticFreeWaterBirthStatus = startsThisUpdate > 0
                ? $"Started {startsThisUpdate} deterministic free-water source event(s), skipped {skippedThisUpdate} slot(s)"
                : $"Scanned deterministic free-water source slots, started 0, skipped {skippedThisUpdate}";
            return startsThisUpdate > 0;
        }

        private bool ResolveAutomaticFreeWaterSourceProfile(
            out AutomaticFreeWaterSourceProfile profile,
            out string inactiveStatus)
        {
            profile = default;
            if (river == null || !river.FoamEnabled)
            {
                inactiveStatus = "Foam disabled";
                return false;
            }

            if (!river.FoamAutomaticBirthEnabled)
            {
                inactiveStatus = "Automatic source population disabled";
                return false;
            }

            if (river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                inactiveStatus = "Waiting for active river domain";
                return false;
            }

            if (fieldWidth <= 0 || fieldHeight <= 0 ||
                fieldLength <= 0.0001f || validFieldLength <= 0.0001f)
            {
                inactiveStatus = "Waiting for Foam field resources";
                return false;
            }

            if (river.FoamSourcePopulationPreset ==
                StylizedRiverFoamSourcePopulationPreset.Off)
            {
                inactiveStatus = "Source population preset Off";
                return false;
            }

            if (!river.FoamAutomaticFreeWaterBirthActive)
            {
                inactiveStatus = "Free Water source class disabled";
                return false;
            }

            float coverage = river.FoamFreeWaterFoamCoverage;
            float activity = river.FoamFreeWaterFoamActivity;
            if (coverage <= 0.0001f)
            {
                inactiveStatus = "Free Water foam coverage is zero";
                return false;
            }

            if (activity <= 0.0001f)
            {
                inactiveStatus = "Free Water foam activity is zero";
                return false;
            }

            profile = new AutomaticFreeWaterSourceProfile(
                true,
                coverage,
                activity,
                river.FoamFreeWaterFoamFormationSpeedMetresPerSecond,
                river.FoamFreeWaterFoamPattern);
            inactiveStatus = string.Empty;
            return profile.Enabled;
        }

        private bool TryStartAutomaticFreeWaterSourceEvent(
            AutomaticFreeWaterSourceProfile profile,
            float now,
            out int skippedSlots)
        {
            skippedSlots = 0;
            if (river == null || !river.Domain.IsValid || validFieldLength <= 0.0001f)
            {
                return false;
            }

            float spacing = Mathf.Max(0.25f, profile.SlotSpacingMetres);
            int longitudinalSlotCount = Mathf.Max(
                1,
                Mathf.FloorToInt(validFieldLength / spacing));
            int totalSlotCount = Mathf.Max(
                1,
                longitudinalSlotCount * AutomaticFreeWaterSourceLateralLaneCount);
            int scanBudget = Mathf.Min(
                totalSlotCount,
                AutomaticFreeWaterSourceMaximumScansPerUpdate);

            disturbanceRuntime ??= GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime != null)
            {
                automaticObjectFoamSources.Clear();
                disturbanceRuntime.CopyStaticObjectFoamSourcesTo(
                    automaticObjectFoamSources);
            }

            for (int scan = 0; scan < scanBudget; scan++)
            {
                int cursor = automaticFreeWaterBirthCursor++;
                int cycleIndex = cursor / totalSlotCount;
                int scanIndex = PositiveModulo(cursor, totalSlotCount);
                int wrappedSlot = ResolvePermutedAutomaticFreeWaterSlot(
                    scanIndex,
                    totalSlotCount,
                    cycleIndex);
                int longitudinalIndex =
                    wrappedSlot / AutomaticFreeWaterSourceLateralLaneCount;
                int lateralIndex =
                    wrappedSlot % AutomaticFreeWaterSourceLateralLaneCount;
                float identitySeed = river.VisualSeed * 0.257f +
                    wrappedSlot * 23.719f;
                float slotSeed = identitySeed + cycleIndex * 41.137f;

                if (Hash01(identitySeed + 1.7f) > profile.Coverage ||
                    (automaticFreeWaterSlotNextStartTimes.TryGetValue(
                         wrappedSlot,
                         out float nextStartTime) &&
                     now + 0.0001f < nextStartTime))
                {
                    skippedSlots++;
                    continue;
                }

                float alongJitter = (Hash01(slotSeed + 2.9f) - 0.5f) * 0.55f;
                float candidateT = (longitudinalIndex + 0.5f + alongJitter) /
                    Mathf.Max(1, longitudinalSlotCount);
                float globalDistance = Mathf.Lerp(
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMinimum + validFieldLength,
                    Mathf.Clamp01(candidateT));

                float laneT = AutomaticFreeWaterSourceLateralLaneCount <= 1
                    ? 0.5f
                    : lateralIndex /
                        (float)(AutomaticFreeWaterSourceLateralLaneCount - 1);
                float acrossNormalized = Mathf.Lerp(-0.70f, 0.70f, laneT) +
                    (Hash01(slotSeed + 3.7f) - 0.5f) * 0.20f;
                acrossNormalized = Mathf.Clamp(acrossNormalized, -0.76f, 0.76f);

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float visibleHalfWidth = sample.GetVisibleHalfWidth(
                    acrossNormalized < 0f ? -1f : 1f);
                if (visibleHalfWidth <= 0.20f)
                {
                    skippedSlots++;
                    continue;
                }

                float centreAcrossMetres = acrossNormalized * visibleHalfWidth;
                AutomaticFreeWaterSourceRecipe recipe =
                    ResolveAutomaticFreeWaterRecipe(profile.Pattern, slotSeed);
                if (TryBeginAutomaticFreeWaterSourceEvent(
                        profile,
                        recipe,
                        slotSeed,
                        globalDistance,
                        acrossNormalized,
                        centreAcrossMetres,
                        visibleHalfWidth))
                {
                    automaticFreeWaterSlotNextStartTimes[wrappedSlot] =
                        now + ResolveLatestAutomaticSourceEventDurationSeconds() +
                        ResolveAutomaticPacketClearanceSeconds(
                            river.FoamFreeWaterMinimumPacketGapMetres);
                    idleSince = 0.0;
                    return true;
                }

                skippedSlots++;
            }

            return false;
        }

        private AutomaticFreeWaterSourceRecipe ResolveAutomaticFreeWaterRecipe(
            StylizedRiverFoamFreeWaterPattern pattern,
            float seed)
        {
            switch (pattern)
            {
                case StylizedRiverFoamFreeWaterPattern.LaceConnectors:
                    return AutomaticFreeWaterSourceRecipe.LaceConnector;
                case StylizedRiverFoamFreeWaterPattern.CrossLaceConnectors:
                    return AutomaticFreeWaterSourceRecipe.CrossLaceConnector;
                case StylizedRiverFoamFreeWaterPattern.TornFragments:
                    return AutomaticFreeWaterSourceRecipe.TornFragment;
            }

            float laceWeight = river != null
                ? river.FoamFreeWaterLaceConnectorPatternWeight
                : 0.30f;
            float crossLaceWeight = river != null
                ? river.FoamFreeWaterCrossLaceConnectorPatternWeight
                : 0.45f;
            float fragmentWeight = river != null
                ? river.FoamFreeWaterTornFragmentPatternWeight
                : 0.25f;
            float totalWeight = Mathf.Max(0f, laceWeight) +
                Mathf.Max(0f, crossLaceWeight) +
                Mathf.Max(0f, fragmentWeight);
            if (totalWeight <= 0.0001f)
            {
                return AutomaticFreeWaterSourceRecipe.CrossLaceConnector;
            }

            float roll = Hash01(seed + 4.1f) * totalWeight;
            float positiveLaceWeight = Mathf.Max(0f, laceWeight);
            if (roll < positiveLaceWeight)
            {
                return AutomaticFreeWaterSourceRecipe.LaceConnector;
            }

            roll -= positiveLaceWeight;
            return roll < Mathf.Max(0f, crossLaceWeight)
                ? AutomaticFreeWaterSourceRecipe.CrossLaceConnector
                : AutomaticFreeWaterSourceRecipe.TornFragment;
        }

        private bool TryBeginAutomaticFreeWaterSourceEvent(
            AutomaticFreeWaterSourceProfile profile,
            AutomaticFreeWaterSourceRecipe recipe,
            float seed,
            float globalDistance,
            float acrossNormalized,
            float centreAcrossMetres,
            float visibleHalfWidth)
        {
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float eventScale = Hash01(seed + 6.5f);
            float widthJitter = Mathf.Lerp(0.88f, 1.12f, Hash01(seed + 7.1f));
            float sourceKey = river.VisualSeed * 0.457f +
                globalDistance * 7.731f +
                centreAcrossMetres * 17.137f +
                seed * 0.053f +
                (recipe == AutomaticFreeWaterSourceRecipe.TornFragment
                    ? 1207f
                    : (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector ? 1321f : 1009f));

            float length;
            float width;
            float amount;
            float remainingLife;
            float breakupScale;
            float breakupStrength;
            float curvature;
            float patternFormationSpeedMultiplier;

            if (recipe == AutomaticFreeWaterSourceRecipe.TornFragment)
            {
                length = Mathf.Lerp(
                    river.FoamFreeWaterFragmentLengthMinMetres,
                    river.FoamFreeWaterFragmentLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamFreeWaterFragmentWidthMinMetres,
                    river.FoamFreeWaterFragmentWidthMaxMetres,
                    eventScale) * widthJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamFreeWaterFragmentInitialLifeMin,
                    river.FoamFreeWaterFragmentInitialLifeMax,
                    eventScale);
                breakupScale = 0f;
                breakupStrength = 0f;
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterFragmentFormationSpeedMultiplier;
                curvature = Mathf.Lerp(-1.0f, 1.0f, Hash01(seed + 11.7f));
                amount = Mathf.Lerp(
                    river.FoamFreeWaterFragmentInitialPresenceMin,
                    river.FoamFreeWaterFragmentInitialPresenceMax,
                    eventScale);
            }
            else if (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector)
            {
                length = Mathf.Lerp(
                    river.FoamFreeWaterCrossLaceLengthMinMetres,
                    river.FoamFreeWaterCrossLaceLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamFreeWaterCrossLaceWidthMinMetres,
                    river.FoamFreeWaterCrossLaceWidthMaxMetres,
                    eventScale) * widthJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamFreeWaterCrossLaceInitialLifeMin,
                    river.FoamFreeWaterCrossLaceInitialLifeMax,
                    eventScale);
                breakupScale = 0f;
                breakupStrength = 0f;
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterCrossLaceFormationSpeedMultiplier;
                curvature = Mathf.Lerp(-1.0f, 1.0f, Hash01(seed + 11.7f));
                amount = Mathf.Lerp(
                    river.FoamFreeWaterCrossLaceInitialPresenceMin,
                    river.FoamFreeWaterCrossLaceInitialPresenceMax,
                    eventScale);
            }
            else
            {
                length = Mathf.Lerp(
                    river.FoamFreeWaterLaceLengthMinMetres,
                    river.FoamFreeWaterLaceLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamFreeWaterLaceWidthMinMetres,
                    river.FoamFreeWaterLaceWidthMaxMetres,
                    eventScale) * widthJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamFreeWaterLaceInitialLifeMin,
                    river.FoamFreeWaterLaceInitialLifeMax,
                    eventScale);
                breakupScale = 0f;
                breakupStrength = 0f;
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterLaceFormationSpeedMultiplier;
                float side = Hash01(seed + 11.7f) < 0.5f ? -1f : 1f;
                curvature = side * Mathf.Lerp(
                    river.FoamFreeWaterLaceCurvatureMin,
                    river.FoamFreeWaterLaceCurvatureMax,
                    Hash01(seed + 12.9f));
                amount = Mathf.Lerp(
                    river.FoamFreeWaterLaceInitialPresenceMin,
                    river.FoamFreeWaterLaceInitialPresenceMax,
                    eventScale);
            }

            length = Mathf.Clamp(length, 0.05f, Mathf.Max(0.05f, validFieldLength * 0.38f));
            width = Mathf.Clamp(width, 0.006f, Mathf.Max(0.015f, visibleHalfWidth * 0.22f));
            if (Mathf.Abs(centreAcrossMetres) + width * 2.5f > visibleHalfWidth * 0.92f)
            {
                return false;
            }

            float feather = Mathf.Clamp(
                Mathf.Max(width * 0.60f, visibleHalfWidth * 0.010f),
                0.012f,
                recipe == AutomaticFreeWaterSourceRecipe.TornFragment ? 0.090f : 0.070f);
            float shapeHalfLength = length * 0.5f;
            float objectContactOffset = 0f;
            float startGlobalDistance;
            float endGlobalDistance;
            float formationDistance;

            if (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector)
            {
                float allowedHalfLength = Mathf.Max(
                    0.08f,
                    visibleHalfWidth * 0.92f - Mathf.Abs(centreAcrossMetres) - width * 2.0f);
                shapeHalfLength = Mathf.Min(shapeHalfLength, allowedHalfLength);
                if (shapeHalfLength <= 0.08f)
                {
                    return false;
                }

                objectContactOffset = Hash01(seed + 14.7f) < 0.5f ? -1f : 1f;
                formationDistance = shapeHalfLength * 2.0f;
                float xPad = width * 3.0f + feather * 2.0f + 0.06f;
                startGlobalDistance = Mathf.Clamp(
                    globalDistance - xPad,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                endGlobalDistance = Mathf.Clamp(
                    globalDistance + xPad,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
            }
            else
            {
                float halfLength = length * 0.5f;
                startGlobalDistance = Mathf.Clamp(
                    globalDistance - flowDirection * halfLength,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                endGlobalDistance = Mathf.Clamp(
                    globalDistance + flowDirection * halfLength,
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMaximum);
                formationDistance = Mathf.Abs(endGlobalDistance - startGlobalDistance);
                shapeHalfLength = Mathf.Max(0.025f, formationDistance * 0.5f);
            }

            if (formationDistance <= 0.05f ||
                Mathf.Abs(endGlobalDistance - startGlobalDistance) <= 0.01f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float objectProximityLength = recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector
                ? width * 6.0f + feather * 2.0f
                : length;
            if (IsFreeWaterSourceTooCloseToObjectSource(
                    globalDistance,
                    centreAcrossMetres,
                    objectProximityLength,
                    width))
            {
                return false;
            }

            ResolvedAutomaticRevealTiming revealTiming =
                ResolveAutomaticRevealTiming(
                    formationDistance,
                    profile.FormationSpeedMetresPerSecond,
                    patternFormationSpeedMultiplier,
                    Mathf.Lerp(0.90f, 1.10f, Hash01(seed + 13.5f)));
            float formationSpeed =
                revealTiming.RequestedSpeedMetresPerSecond;
            float materialStepDuration = 1f / Mathf.Max(1f, ResolveUpdateRate());
            float headTrailMetres = recipe == AutomaticFreeWaterSourceRecipe.TornFragment
                ? 0f
                : Mathf.Clamp(
                    Mathf.Max(width * 4.0f, formationSpeed * materialStepDuration * 1.50f),
                    AutomaticFreeWaterSourceMinimumHeadTrailMetres,
                    Mathf.Min(
                        AutomaticFreeWaterSourceMaximumHeadTrailMetres,
                        Mathf.Max(AutomaticFreeWaterSourceMinimumHeadTrailMetres, formationDistance * 0.22f)));
            float lateralPadding = recipe == AutomaticFreeWaterSourceRecipe.TornFragment
                ? width * 2.8f + feather * 2f
                : (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector
                    ? shapeHalfLength + width * 2.8f + feather * 2f
                    : Mathf.Abs(curvature) * width * 5.2f + width * 2.6f + feather * 2f);

            return BeginAutomaticFreeWaterFoamSourceEvent(
                recipe,
                startGlobalDistance,
                endGlobalDistance,
                acrossNormalized,
                centreAcrossMetres,
                revealTiming,
                headTrailMetres,
                width,
                feather,
                amount,
                remainingLife,
                sourceKey,
                breakupScale,
                breakupStrength,
                curvature,
                lateralPadding,
                shapeHalfLength,
                objectContactOffset);
        }

        private bool IsFreeWaterSourceTooCloseToObjectSource(
            float globalDistance,
            float centreAcrossMetres,
            float lengthMetres,
            float widthMetres)
        {
            if (automaticObjectFoamSources == null || automaticObjectFoamSources.Count <= 0)
            {
                return false;
            }

            float halfLength = Mathf.Max(0.05f, lengthMetres * 0.5f);
            float halfWidth = Mathf.Max(0.02f, widthMetres);
            for (int index = 0; index < automaticObjectFoamSources.Count; index++)
            {
                RiverFoamStaticObjectSource source = automaticObjectFoamSources[index];
                float alongDelta = Mathf.Abs(globalDistance - source.GlobalDistance);
                float acrossDelta = Mathf.Abs(centreAcrossMetres - source.AcrossMetres);
                if (alongDelta < source.StaticPressureAlongHalfLength + halfLength * 0.65f &&
                    acrossDelta < source.StaticPressureAcrossHalfWidth + halfWidth * 3.0f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool BeginAutomaticFreeWaterFoamSourceEvent(
            AutomaticFreeWaterSourceRecipe recipe,
            float startGlobalDistance,
            float endGlobalDistance,
            float centreAcrossNormalized,
            float centreAcrossMetres,
            ResolvedAutomaticRevealTiming revealTiming,
            float headTrailMetres,
            float widthMetres,
            float featherMetres,
            float amount,
            float remainingLife,
            float sourceKey,
            float breakupScaleMetres,
            float breakupStrength,
            float curvature,
            float lateralPaddingMetres,
            float shapeHalfLengthMetres,
            float objectContactOffsetMetres)
        {
            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeAutomaticFoamSourceSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            AutomaticFoamSourceEventType sourceType = recipe ==
                AutomaticFreeWaterSourceRecipe.TornFragment
                    ? AutomaticFoamSourceEventType.FreeWaterTornFragment
                    : (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector
                        ? AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector
                        : AutomaticFoamSourceEventType.FreeWaterLaceConnector);
            float halfLength = Mathf.Max(0.025f, shapeHalfLengthMetres);
            float halfWidth = Mathf.Max(0.005f, widthMetres);

            automaticFoamSourceEvents[slotIndex] = new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = eventId,
                Type = sourceType,
                SideSign = 0f,
                StartGlobalDistance = startGlobalDistance,
                EndGlobalDistance = endGlobalDistance,
                Duration = revealTiming.ResolvedDurationSeconds,
                Elapsed = 0f,
                FormationSpeedMetresPerSecond =
                    revealTiming.RequestedSpeedMetresPerSecond,
                RevealPathDistanceMetres = revealTiming.PathDistanceMetres,
                RawRevealDurationSeconds = revealTiming.RawDurationSeconds,
                RevealCadenceLimited = revealTiming.CadenceLimited,
                HeadTrailMetres = Mathf.Clamp(
                    headTrailMetres,
                    0f,
                    AutomaticFreeWaterSourceMaximumHeadTrailMetres),
                ShoreInsetMetres = 0f,
                WidthMetres = Mathf.Max(0.006f, widthMetres),
                InwardReachMetres = 0f,
                FeatherMetres = Mathf.Max(0.006f, featherMetres),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticFreeWaterBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticFreeWaterBirthSourceFillSeedSalt,
                SourceFillFeatureSize = Mathf.Max(
                    SourceFillMinimumFeatureSizeMetres * 0.50f,
                    Mathf.Max(widthMetres * 2.0f, featherMetres * 1.25f)),
                ShapeSeed = sourceKey + AutomaticFreeWaterBirthShapeSeedSalt,
                BreakupScaleMetres = 0f,
                BreakupStrength = 0f,
                Curvature = Mathf.Clamp(curvature, -1f, 1f),
                SourceFillBlend = sourceType == AutomaticFoamSourceEventType.FreeWaterLaceConnector
                    ? 0.18f
                    : (sourceType == AutomaticFoamSourceEventType.FreeWaterTornFragment ? 0.32f : 0.06f),
                ObjectCentreAcrossMetres = centreAcrossMetres,
                ObjectAlongHalfLengthMetres = halfLength,
                ObjectAcrossHalfWidthMetres = halfWidth,
                ObjectContactOffsetMetres = objectContactOffsetMetres,
                CentreAcrossNormalized = Mathf.Clamp(centreAcrossNormalized, -1f, 1f),
                LateralPaddingMetres = Mathf.Max(widthMetres * 2f, lateralPaddingMetres)
            };

            RecordAutomaticRevealTiming(
                eventId,
                sourceType,
                revealTiming);
            activeAutomaticFoamSourceEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                GlobalDistanceToNormalized(startGlobalDistance);
            latestFoamCompositionPreviousDistanceNormalized =
                latestFoamCompositionHeadDistanceNormalized;
            latestFoamCompositionHeadAcrossNormalized = centreAcrossNormalized;
            latestFoamCompositionPreviousAcrossNormalized = centreAcrossNormalized;
            lastFoamCompositionSegmentLength = 0f;
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / automatic free-water source-event rasterizer";
            RecordMaterialBirthCommand();
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
            return true;
        }

        private int ResolvePermutedAutomaticFreeWaterSlot(
            int scanIndex,
            int slotCount,
            int cycleIndex)
        {
            if (slotCount <= 1)
            {
                return 0;
            }

            int stride = ResolveCoprimeAutomaticSourceStride(
                slotCount,
                cycleIndex + 149);
            int offset = PositiveModulo(
                Mathf.RoundToInt(Hash01(
                    river.VisualSeed * 0.293f + cycleIndex * 29.731f) *
                    slotCount),
                slotCount);
            return PositiveModulo(offset + scanIndex * stride, slotCount);
        }

        private bool TryStartAutomaticShoreSourceEvent(
            AutomaticShoreSourceProfile profile,
            float now,
            out int skippedSlots)
        {
            skippedSlots = 0;
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            float spacing = Mathf.Max(0.25f, profile.SlotSpacingMetres);
            int longitudinalSlotCount = Mathf.Max(
                1,
                Mathf.FloorToInt(validFieldLength / spacing));
            int totalSlotCount = longitudinalSlotCount * 2;
            int scanBudget = Mathf.Min(
                Mathf.Max(2, totalSlotCount),
                AutomaticShoreSourceMaximumScansPerUpdate);

            for (int scan = 0; scan < scanBudget; scan++)
            {
                int slotCursor = automaticShoreBirthCursor++;
                int cycleIndex = slotCursor / Mathf.Max(1, totalSlotCount);
                int scanIndex = PositiveModulo(slotCursor, totalSlotCount);
                int wrappedSlot = ResolvePermutedAutomaticShoreSlot(
                    scanIndex,
                    totalSlotCount,
                    cycleIndex);
                int longitudinalIndex = wrappedSlot / 2;
                int sideIndex = wrappedSlot & 1;
                float sideSign = sideIndex == 0 ? -1f : 1f;
                float identitySeed = river.VisualSeed * 0.137f +
                    wrappedSlot * 17.317f;
                float slotSeed = identitySeed + cycleIndex * 31.619f;

                if (Hash01(identitySeed + 1.7f) > profile.Coverage ||
                    (automaticShoreSlotNextStartTimes.TryGetValue(
                         wrappedSlot,
                         out float nextStartTime) &&
                     now + 0.0001f < nextStartTime))
                {
                    skippedSlots++;
                    continue;
                }

                float slotJitter = (Hash01(slotSeed + 2.9f) - 0.5f) * 0.45f;
                float candidateT = (longitudinalIndex + 0.5f + slotJitter) /
                    Mathf.Max(1, longitudinalSlotCount);
                float globalDistance = Mathf.Lerp(
                    river.Domain.GlobalDistanceMinimum,
                    river.Domain.GlobalDistanceMinimum + validFieldLength,
                    Mathf.Clamp01(candidateT));

                StylizedRiverSplineSample sample =
                    river.Domain.SampleAtGlobalDistance(globalDistance);
                float visibleHalfWidth = sample.GetVisibleHalfWidth(sideSign);
                if (visibleHalfWidth <= 0.05f)
                {
                    skippedSlots++;
                    continue;
                }

                AutomaticShoreSourceRecipe recipe =
                    ResolveAutomaticShoreRecipe(profile.Pattern, slotSeed);
                if (TryBeginAutomaticShoreSourceEvent(
                        profile,
                        recipe,
                        slotSeed,
                        globalDistance,
                        sideSign,
                        visibleHalfWidth))
                {
                    automaticShoreSlotNextStartTimes[wrappedSlot] =
                        now + ResolveLatestAutomaticSourceEventDurationSeconds() +
                        ResolveAutomaticPacketClearanceSeconds(
                            river.FoamShoreMinimumPacketGapMetres);
                    idleSince = 0.0;
                    return true;
                }

                skippedSlots++;
            }

            return false;
        }

        private AutomaticShoreSourceRecipe ResolveAutomaticShoreRecipe(
            StylizedRiverFoamShorePattern pattern,
            float seed)
        {
            switch (pattern)
            {
                case StylizedRiverFoamShorePattern.ShoreRibbons:
                    return AutomaticShoreSourceRecipe.ShoreRibbon;
                case StylizedRiverFoamShorePattern.InwardWash:
                    return AutomaticShoreSourceRecipe.InwardWash;
            }

            float ribbonWeight = river != null
                ? river.FoamShoreRibbonPatternWeight
                : 0.88f;
            float washWeight = river != null
                ? river.FoamInwardWashPatternWeight
                : 0.12f;
            float totalWeight = Mathf.Max(0f, ribbonWeight) +
                Mathf.Max(0f, washWeight);
            if (totalWeight <= 0.0001f)
            {
                return AutomaticShoreSourceRecipe.ShoreRibbon;
            }

            float ribbonChance = Mathf.Clamp01(
                Mathf.Max(0f, ribbonWeight) / totalWeight);
            return Hash01(seed + 4.1f) < ribbonChance
                ? AutomaticShoreSourceRecipe.ShoreRibbon
                : AutomaticShoreSourceRecipe.InwardWash;
        }

        private bool TryBeginAutomaticShoreSourceEvent(
            AutomaticShoreSourceProfile profile,
            AutomaticShoreSourceRecipe recipe,
            float seed,
            float globalDistance,
            float sideSign,
            float visibleHalfWidth)
        {
            float size = profile.PatchSize;
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float eventScale = Mathf.Clamp01(
                size * Mathf.Lerp(0.82f, 1.18f, Hash01(seed + 6.5f)));
            float widthJitter = Mathf.Lerp(0.94f, 1.06f, Hash01(seed + 7.1f));
            float reachJitter = Mathf.Lerp(0.92f, 1.08f, Hash01(seed + 7.7f));
            float offsetJitter = Mathf.Lerp(0.85f, 1.15f, Hash01(seed + 8.3f));
            float approximateCrossCellSpacing =
                ResolveSourceLateralSpacingMetres(
                    globalDistance,
                    sideSign);
            float shoreRibbonThicknessCells = 0f;
            float shoreRibbonThicknessMetres = 0f;
            float sourceKey = river.VisualSeed * 0.317f +
                globalDistance * 13.731f +
                sideSign * 29.137f +
                seed * 0.071f +
                (recipe == AutomaticShoreSourceRecipe.InwardWash ? 503f : 211f);

            float length;
            float width;
            float inwardReach;
            float shoreInset;
            float feather;
            float amount;
            float remainingLife;
            float breakupScale;
            float breakupStrength;
            float curvature;
            float patternFormationSpeedMultiplier;

            switch (recipe)
            {
                case AutomaticShoreSourceRecipe.InwardWash:
                    length = Mathf.Lerp(
                        river.FoamInwardWashLengthMinMetres,
                        river.FoamInwardWashLengthMaxMetres,
                        eventScale);
                    width = Mathf.Lerp(
                        river.FoamInwardWashWidthMinMetres,
                        river.FoamInwardWashWidthMaxMetres,
                        eventScale) * widthJitter;
                    inwardReach = Mathf.Lerp(
                        river.FoamInwardWashReachMinMetres,
                        river.FoamInwardWashReachMaxMetres,
                        eventScale) * reachJitter;
                    shoreInset = Mathf.Lerp(
                        river.FoamInwardWashOffsetMinMetres,
                        river.FoamInwardWashOffsetMaxMetres,
                        eventScale) * offsetJitter;
                    width = Mathf.Min(width, Mathf.Max(0.012f, length * 0.080f));
                    inwardReach = Mathf.Clamp(
                        inwardReach,
                        Mathf.Max(0.030f, width * 2.0f),
                        Mathf.Max(0.050f, length * 0.45f));
                    amount = Mathf.Lerp(
                        river.FoamInwardWashInitialPresenceMin,
                        river.FoamInwardWashInitialPresenceMax,
                        eventScale);
                    remainingLife = Mathf.Lerp(
                        river.FoamInwardWashInitialLifeMin,
                        river.FoamInwardWashInitialLifeMax,
                        eventScale);
                    breakupScale = 0f;
                    breakupStrength = 0f;
                    curvature = flowDirection * Mathf.Lerp(
                        0.18f,
                        0.56f,
                        Hash01(seed + 10.7f)) *
                        (Hash01(seed + 11.3f) < 0.5f ? -1f : 1f);
                    patternFormationSpeedMultiplier =
                        river.FoamInwardWashFormationSpeedMultiplier;
                    break;
                default:
                    length = Mathf.Lerp(
                        river.FoamShoreRibbonLengthMinMetres,
                        river.FoamShoreRibbonLengthMaxMetres,
                        eventScale);
                    shoreRibbonThicknessCells =
                        river.FoamShoreRibbonThicknessCells;
                    shoreRibbonThicknessMetres =
                        shoreRibbonThicknessCells *
                        approximateCrossCellSpacing;
                    width = shoreRibbonThicknessMetres;
                    float offsetVariationMetres =
                        river.FoamShoreRibbonOffsetVariationCells *
                        approximateCrossCellSpacing;
                    shoreInset = Mathf.Max(
                        0f,
                        river.FoamShoreRibbonOffsetMetres +
                        (Hash01(seed + 8.3f) * 2f - 1f) *
                        offsetVariationMetres);
                    inwardReach = 0f;
                    amount = Mathf.Lerp(
                        river.FoamShoreRibbonInitialPresenceMin,
                        river.FoamShoreRibbonInitialPresenceMax,
                        eventScale);
                    remainingLife = Mathf.Lerp(
                        river.FoamShoreRibbonInitialLifeMin,
                        river.FoamShoreRibbonInitialLifeMax,
                        eventScale);
                    breakupScale = 0f;
                    breakupStrength = 0f;
                    curvature = Mathf.Lerp(
                        -0.10f,
                        0.10f,
                        Hash01(seed + 10.7f));
                    patternFormationSpeedMultiplier =
                        river.FoamShoreRibbonFormationSpeedMultiplier;
                    break;
            }

            patternFormationSpeedMultiplier = Mathf.Clamp(
                patternFormationSpeedMultiplier,
                0.10f,
                3.00f);
            remainingLife = Mathf.Clamp01(remainingLife);
            breakupStrength = Mathf.Clamp01(breakupStrength);
            length = Mathf.Max(0.05f, length);
            bool isShoreRibbon =
                recipe == AutomaticShoreSourceRecipe.ShoreRibbon;
            inwardReach = isShoreRibbon
                ? 0f
                : Mathf.Clamp(
                    inwardReach,
                    0.06f,
                    Mathf.Max(0.06f, visibleHalfWidth * 0.45f));
            shoreInset = Mathf.Clamp(
                shoreInset,
                0f,
                Mathf.Max(0.010f, visibleHalfWidth * 0.30f));
            width = isShoreRibbon
                ? Mathf.Max(0.005f, width)
                : Mathf.Clamp(
                    width,
                    0.012f,
                    Mathf.Max(0.030f, visibleHalfWidth * 0.20f));
            feather = isShoreRibbon
                ? approximateCrossCellSpacing * 0.50f
                : Mathf.Clamp(
                    Mathf.Max(width * 0.45f, visibleHalfWidth * 0.012f),
                    0.025f,
                    0.120f);

            float halfLength = length * 0.5f;
            float startGlobalDistance = Mathf.Clamp(
                globalDistance - flowDirection * halfLength,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float endGlobalDistance = Mathf.Clamp(
                globalDistance + flowDirection * halfLength,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);

            float longitudinalDistance = Mathf.Abs(endGlobalDistance - startGlobalDistance);
            if (longitudinalDistance <= 0.05f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float sourcePathDistance = recipe == AutomaticShoreSourceRecipe.InwardWash
                ? Mathf.Sqrt(longitudinalDistance * longitudinalDistance +
                    inwardReach * inwardReach) * Mathf.Lerp(
                        1.04f,
                        1.18f,
                        Mathf.Clamp01(Mathf.Abs(curvature)))
                : longitudinalDistance;
            ResolvedAutomaticRevealTiming revealTiming =
                ResolveAutomaticRevealTiming(
                    sourcePathDistance,
                    profile.FormationSpeedMetresPerSecond,
                    patternFormationSpeedMultiplier,
                    Mathf.Lerp(0.88f, 1.12f, Hash01(seed + 12.5f)));
            float formationSpeed =
                revealTiming.RequestedSpeedMetresPerSecond;
            float materialStepDuration = 1f / Mathf.Max(1f, ResolveUpdateRate());
            bool isInwardWash = recipe == AutomaticShoreSourceRecipe.InwardWash;
            float minimumHeadTrailMetres = isInwardWash
                ? AutomaticShoreWashMinimumHeadTrailMetres
                : AutomaticShoreSourceMinimumHeadTrailMetres;
            float maximumHeadTrailMetres = isInwardWash
                ? Mathf.Min(
                    AutomaticShoreWashMaximumHeadTrailMetres,
                    sourcePathDistance * AutomaticShoreWashMaximumHeadTrailFraction)
                : Mathf.Min(
                    AutomaticShoreSourceMaximumHeadTrailMetres,
                    sourcePathDistance * 0.28f);
            maximumHeadTrailMetres = Mathf.Max(
                minimumHeadTrailMetres,
                maximumHeadTrailMetres);
            float headTrailMetres = Mathf.Clamp(
                Mathf.Max(feather * 1.35f, formationSpeed * materialStepDuration * 1.50f),
                minimumHeadTrailMetres,
                maximumHeadTrailMetres);

            return BeginAutomaticFoamSourceEvent(
                recipe,
                sideSign,
                startGlobalDistance,
                endGlobalDistance,
                revealTiming,
                headTrailMetres,
                shoreInset,
                width,
                shoreRibbonThicknessCells,
                shoreRibbonThicknessMetres,
                inwardReach,
                feather,
                amount,
                remainingLife,
                sourceKey,
                breakupScale,
                breakupStrength,
                curvature);
        }


        private bool BeginAutomaticFoamSourceEvent(
            AutomaticShoreSourceRecipe recipe,
            float sideSign,
            float startGlobalDistance,
            float endGlobalDistance,
            ResolvedAutomaticRevealTiming revealTiming,
            float headTrailMetres,
            float shoreInsetMetres,
            float widthMetres,
            float shoreRibbonThicknessCells,
            float shoreRibbonThicknessMetres,
            float inwardReachMetres,
            float featherMetres,
            float amount,
            float remainingLife,
            float sourceKey,
            float breakupScaleMetres,
            float breakupStrength,
            float curvature)
        {
            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f || !river.Domain.IsValid)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeAutomaticFoamSourceSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            AutomaticFoamSourceEventType sourceType = recipe ==
                AutomaticShoreSourceRecipe.InwardWash
                    ? AutomaticFoamSourceEventType.InwardWash
                    : AutomaticFoamSourceEventType.ShoreRibbon;

            float slotMinimumHeadTrailMetres = sourceType ==
                AutomaticFoamSourceEventType.InwardWash
                    ? AutomaticShoreWashMinimumHeadTrailMetres
                    : AutomaticShoreSourceMinimumHeadTrailMetres;
            float slotMaximumHeadTrailMetres = sourceType ==
                AutomaticFoamSourceEventType.InwardWash
                    ? AutomaticShoreWashMaximumHeadTrailMetres
                    : AutomaticShoreSourceMaximumHeadTrailMetres;

            automaticFoamSourceEvents[slotIndex] = new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = eventId,
                Type = sourceType,
                SideSign = sideSign < 0f ? -1f : 1f,
                StartGlobalDistance = startGlobalDistance,
                EndGlobalDistance = endGlobalDistance,
                Duration = revealTiming.ResolvedDurationSeconds,
                Elapsed = 0f,
                FormationSpeedMetresPerSecond =
                    revealTiming.RequestedSpeedMetresPerSecond,
                RevealPathDistanceMetres = revealTiming.PathDistanceMetres,
                RawRevealDurationSeconds = revealTiming.RawDurationSeconds,
                RevealCadenceLimited = revealTiming.CadenceLimited,
                HeadTrailMetres = Mathf.Clamp(
                    headTrailMetres,
                    slotMinimumHeadTrailMetres,
                    slotMaximumHeadTrailMetres),
                ShoreInsetMetres = Mathf.Max(0f, shoreInsetMetres),
                WidthMetres = Mathf.Max(0.005f, widthMetres),
                ShoreRibbonThicknessCells = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? Mathf.Clamp(shoreRibbonThicknessCells, 0.5f, 4f)
                        : 0f,
                ShoreRibbonThicknessMetres = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? Mathf.Max(0.005f, shoreRibbonThicknessMetres)
                        : 0f,
                InwardReachMetres = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? 0f
                        : Mathf.Max(0.01f, inwardReachMetres),
                FeatherMetres = Mathf.Max(0.005f, featherMetres),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticShoreBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticShoreBirthSourceFillSeedSalt,
                SourceFillFeatureSize = Mathf.Max(
                    SourceFillMinimumFeatureSizeMetres,
                    sourceType == AutomaticFoamSourceEventType.InwardWash
                        ? Mathf.Max(widthMetres * 1.35f, featherMetres * 1.25f)
                        : Mathf.Max(widthMetres, featherMetres * 1.25f)),
                ShapeSeed = sourceKey + AutomaticShoreBirthShapeSeedSalt,
                BreakupScaleMetres = 0f,
                BreakupStrength = 0f,
                Curvature = curvature,
                SourceFillBlend = sourceType == AutomaticFoamSourceEventType.InwardWash ? 0.08f : 0.35f
            };

            RecordAutomaticRevealTiming(
                eventId,
                sourceType,
                revealTiming);
            activeAutomaticFoamSourceEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                GlobalDistanceToNormalized(startGlobalDistance);
            latestFoamCompositionPreviousDistanceNormalized =
                latestFoamCompositionHeadDistanceNormalized;
            latestFoamCompositionHeadAcrossNormalized = 0f;
            latestFoamCompositionPreviousAcrossNormalized = 0f;
            lastFoamCompositionSegmentLength = 0f;
            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / automatic source-event rasterizer";
            RecordMaterialBirthCommand();
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
            return true;
        }

        private int FindFreeAutomaticFoamSourceSlot()
        {
            for (int index = 0; index < automaticFoamSourceEvents.Length; index++)
            {
                if (!automaticFoamSourceEvents[index].Active)
                {
                    return index;
                }
            }

            return -1;
        }

        private void ClearAutomaticFoamSourceEvents()
        {
            Array.Clear(
                automaticFoamSourceEvents,
                0,
                automaticFoamSourceEvents.Length);
            Array.Clear(
                automaticFoamSourceEventGpuData,
                0,
                automaticFoamSourceEventGpuData.Length);
            Array.Clear(
                automaticRevealTimingByType,
                0,
                automaticRevealTimingByType.Length);
            activeAutomaticFoamSourceEventCount = 0;
            automaticSourceEventsRasterizedLastUpdate = 0;
            automaticObjectSourceStates.Clear();
            automaticShoreSlotNextStartTimes.Clear();
            automaticFreeWaterSlotNextStartTimes.Clear();
            automaticObjectContactLiveSourceIds.Clear();
            automaticObjectContactStaleSourceIds.Clear();
            automaticObjectContactCycleTime = 0f;
            automaticObjectPatternAuthoritySignature = int.MinValue;
            automaticObjectClearanceAuthoritySignature = int.MinValue;
            automaticObjectContactBuildCount = 0;
            automaticObjectContactFleckCount = 0;
            automaticObjectWaitingClearanceCount = 0;
        }

        private int ResolvePermutedAutomaticShoreSlot(
            int scanIndex,
            int slotCount,
            int cycleIndex)
        {
            if (slotCount <= 1)
            {
                return 0;
            }

            int stride = ResolveCoprimeAutomaticSourceStride(slotCount, cycleIndex);
            int offset = PositiveModulo(
                Mathf.RoundToInt(
                    Hash01(river.VisualSeed * 0.173f + cycleIndex * 19.31f) *
                    slotCount),
                slotCount);
            return PositiveModulo(offset + scanIndex * stride, slotCount);
        }

        private int ResolveCoprimeAutomaticSourceStride(
            int slotCount,
            int cycleIndex)
        {
            int stride = Mathf.Max(1, Mathf.RoundToInt(
                Mathf.Lerp(
                    1f,
                    Mathf.Max(1, slotCount - 1),
                    Hash01(river.VisualSeed * 0.271f + cycleIndex * 7.13f))));
            if ((stride & 1) == 0)
            {
                stride++;
            }

            stride = PositiveModulo(stride, slotCount);
            if (stride == 0)
            {
                stride = 1;
            }

            int guard = 0;
            while (GreatestCommonDivisor(stride, slotCount) != 1 &&
                   guard < slotCount)
            {
                stride = PositiveModulo(stride + 2, slotCount);
                if (stride == 0)
                {
                    stride = 1;
                }

                guard++;
            }

            return Mathf.Max(1, stride);
        }

        private static int GreatestCommonDivisor(int a, int b)
        {
            a = Mathf.Abs(a);
            b = Mathf.Abs(b);
            while (b != 0)
            {
                int remainder = a % b;
                a = b;
                b = remainder;
            }

            return Mathf.Max(1, a);
        }


        private bool BeginFoamCompositionEvent(
            float startGlobalDistance,
            float startAcrossNormalized,
            float baseRadius,
            float amount,
            float remainingLife,
            float duration,
            float travelDistance,
            float acrossDrift,
            float pathWander,
            float strokeAspect,
            float widthVariation,
            float amountEnvelopeFloor,
            float radiusEnvelopeFloor,
            float sourceKey)
        {
            if (river == null || !river.FoamEnabled ||
                river.FreezeAmount >= 0.999f ||
                !river.Domain.IsValid)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int slotIndex = FindFreeFoamCompositionSlot();
            if (slotIndex < 0)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float clampedStartGlobalDistance = Mathf.Clamp(
                startGlobalDistance,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float availableDownstreamDistance = Mathf.Max(
                0f,
                flowDirection > 0f
                    ? river.Domain.GlobalDistanceMaximum - clampedStartGlobalDistance
                    : clampedStartGlobalDistance - river.Domain.GlobalDistanceMinimum);
            float resolvedTravelDistance = Mathf.Min(
                Mathf.Max(0.01f, Mathf.Abs(travelDistance)),
                availableDownstreamDistance);
            float resolvedAmount = Mathf.Clamp01(amount);
            if (resolvedTravelDistance <= 0.01f ||
                resolvedAmount <= 0.0001f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            int eventId = ++foamCompositionSequence;
            float startAcross = Mathf.Clamp(startAcrossNormalized, -1f, 1f);
            float startLateralMetres =
                ResolveAcrossMetresApproximation(startAcross);
            float resolvedRadius = Mathf.Max(0.020f, baseRadius);
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedRadius);
            float resolvedDuration = Mathf.Max(0.05f, duration);
            float resolvedDrift = Mathf.Clamp(acrossDrift, -1f, 1f);
            float resolvedWander = Mathf.Clamp01(pathWander);
            float shapeSeed = sourceKey + 37.719f;
            float patternSeed = sourceKey + ProgressivePatternSeedSalt;
            float sourceFillSeed = sourceKey + ProgressiveSourceFillSeedSalt;
            float bendSign = Hash01(shapeSeed + 11.3f) < 0.5f ? -1f : 1f;
            float startRadius = ResolveProgressiveRibbonRadius(
                resolvedRadius,
                0f,
                0f,
                Mathf.Clamp01(radiusEnvelopeFloor),
                Mathf.Clamp(widthVariation, 0f, 0.65f));

            foamCompositionEvents[slotIndex] = new FoamCompositionEvent
            {
                Active = true,
                UsesMetricLateral = false,
                EventId = eventId,
                StartGlobalDistance = clampedStartGlobalDistance,
                StartAcrossNormalized = startAcross,
                StartLateralMetres = startLateralMetres,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = resolvedDrift,
                AcrossDriftMetres = 0f,
                PathWander = resolvedWander,
                PathWanderMetres = 0f,
                BaseRadius = resolvedRadius,
                SourceAmount = resolvedAmount,
                RemainingLife = Mathf.Clamp01(remainingLife),
                AmountEnvelopeFloor = Mathf.Clamp01(amountEnvelopeFloor),
                RadiusEnvelopeFloor = Mathf.Clamp01(radiusEnvelopeFloor),
                PatternSeed = patternSeed,
                ShapeSeed = shapeSeed,
                SourceFillSeed = sourceFillSeed,
                SourceFillFeatureSize = sourceFillFeatureSize,
                BendSign = bendSign,
                WidthPhase = Hash01(shapeSeed + 19.7f) * Mathf.PI * 2f,
                StrokeAspect = Mathf.Clamp(strokeAspect, 1f, 6f),
                WidthVariation = Mathf.Clamp(widthVariation, 0f, 0.65f),
                Elapsed = 0f,
                PreviousGlobalDistance = clampedStartGlobalDistance,
                PreviousAcrossNormalized = startAcross,
                PreviousLateralMetres = startLateralMetres,
                PreviousRadius = startRadius,
                PreviousEmissionAmount = Mathf.Clamp01(
                    resolvedAmount * Mathf.Clamp01(amountEnvelopeFloor))
            };

            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / automatic shore source event";
            activeFoamCompositionEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                GlobalDistanceToNormalized(clampedStartGlobalDistance);
            latestFoamCompositionPreviousDistanceNormalized =
                latestFoamCompositionHeadDistanceNormalized;
            latestFoamCompositionHeadAcrossNormalized = startAcross;
            latestFoamCompositionPreviousAcrossNormalized = startAcross;
            lastFoamCompositionSegmentLength = 0f;
            simulationAccumulator = Mathf.Max(
                simulationAccumulator,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
            idleSince = 0.0;
            return true;
        }

        private int ResolveAutomaticShoreBirthBudgetPerTick()
        {
            return AutomaticShoreSourceMaximumStartsPerUpdate;
        }

        private int ResolveFoamCompositionBirthBudgetPerStep()
        {
            StylizedRiverQuality quality = river != null
                ? river.Quality
                : StylizedRiverQuality.Medium;
            return quality switch
            {
                StylizedRiverQuality.Low => LowFoamCompositionBirthBudgetPerStep,
                StylizedRiverQuality.High => HighFoamCompositionBirthBudgetPerStep,
                _ => MediumFoamCompositionBirthBudgetPerStep
            };
        }

        private static float Hash01(float value)
        {
            return Mathf.Repeat(
                Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f,
                1f);
        }
    }
}
