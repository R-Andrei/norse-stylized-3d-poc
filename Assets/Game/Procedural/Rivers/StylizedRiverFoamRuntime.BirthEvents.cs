using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        public bool StartFoamCompositionNormalized(
            StylizedRiverFoamSpawnPreset pattern,
            float distanceNormalized,
            float acrossNormalized,
            float scale,
            float amount,
            float remainingLife,
            float complexity,
            float density,
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
                    ResolveFoamCompositionTravelDistance(
                        pattern,
                        travelDistance),
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

            if (activeFoamCompositionEventCount == 0)
            {
                ResetProgressiveBirthDiagnosticSession();
            }

            int eventId = ++foamCompositionSequence;
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
            float resolvedComplexity = Mathf.Clamp01(complexity);
            float resolvedDensity = Mathf.Clamp01(density);
            float startAcross = Mathf.Clamp(acrossNormalized, -1f, 1f);
            float resolvedHalfWidth = Mathf.Clamp(
                ResolveFoamCompositionHalfWidth(pattern, scale),
                ProgressiveRibbonMinimumHalfWidth,
                ProgressiveRibbonMaximumHalfWidth);
            float resolvedStrokeAspect = ResolveFoamCompositionStrokeAspect(
                pattern,
                resolvedComplexity,
                resolvedDensity);
            float resolvedFragmentStrength = ResolveFoamCompositionFragmentStrength(
                pattern,
                resolvedComplexity);
            float resolvedWidthVariation = ResolveFoamCompositionWidthVariation(
                pattern,
                resolvedComplexity);
            float resolvedSourceFillFeatureScale = ResolveFoamCompositionFeatureScale(
                pattern,
                resolvedComplexity,
                resolvedDensity);
            float sourceFillFeatureSize =
                ResolveSourceFillFeatureSize(resolvedHalfWidth) *
                resolvedSourceFillFeatureScale;
            float resolvedDuration = Mathf.Clamp(
                ResolveFoamCompositionDuration(pattern, duration),
                ProgressiveRibbonMinimumDuration,
                ProgressiveRibbonMaximumDuration);
            float resolvedDrift = Mathf.Clamp(
                ResolveFoamCompositionAcrossDrift(
                    pattern,
                    acrossDrift,
                    startAcross),
                -1f,
                1f);
            float resolvedWander = Mathf.Clamp01(
                pathWander * Mathf.Lerp(0.55f, 1.20f, resolvedComplexity));
            bool sheetStyle = pattern == StylizedRiverFoamSpawnPreset.TornSheetRibbon ||
                pattern == StylizedRiverFoamSpawnPreset.ShoreSkirt;
            float startRadius = ResolveProgressiveRibbonRadius(
                resolvedHalfWidth,
                0f,
                widthPhase,
                0f,
                resolvedWidthVariation);

            foamCompositionEvents[slotIndex] = new FoamCompositionEvent
            {
                Active = true,
                EventId = eventId,
                Pattern = pattern,
                StartGlobalDistance = startGlobalDistance,
                StartAcrossNormalized = startAcross,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = resolvedDrift,
                PathWander = resolvedWander,
                BaseRadius = resolvedHalfWidth,
                SourceAmount = resolvedAmount * Mathf.Lerp(0.45f, 1.15f, resolvedDensity),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = patternSeed,
                ShapeSeed = shapeSeed,
                SourceFillSeed = sourceFillSeed,
                SourceFillFeatureSize = sourceFillFeatureSize,
                BendSign = bendSign,
                WidthPhase = widthPhase,
                StrokeAspect = resolvedStrokeAspect,
                FragmentStrength = resolvedFragmentStrength,
                WidthVariation = resolvedWidthVariation,
                Complexity = resolvedComplexity,
                Density = resolvedDensity,
                SheetStyle = sheetStyle,
                Elapsed = 0f,
                PreviousGlobalDistance = startGlobalDistance,
                PreviousAcrossNormalized = startAcross,
                PreviousRadius = startRadius,
                PreviousEmissionAmount = 0f,
                DebugTrajectoryPending = true
            };

            materialLifetimeAuthorityActive = true;
            materialLifetimeEmptyMetricReadbacks = 0;
            lifetimeAuthorityStatus =
                "Remaining Life / full-field direct simulation";
            activeFoamCompositionEventCount++;
            foamCompositionStartedCount++;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized =
                Mathf.Clamp01(distanceNormalized);
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
                PrepareProgressiveBirthDebugEvent(ref compositionEvent);

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
                    out float headAcrossNormalized);
                float envelope = ResolveProgressiveRibbonEnvelope(progress);
                float headRadius = ResolveProgressiveRibbonRadius(
                    compositionEvent.BaseRadius,
                    progress,
                    compositionEvent.WidthPhase,
                    envelope,
                    compositionEvent.WidthVariation);
                float headAmount = Mathf.Clamp01(
                    compositionEvent.SourceAmount) * envelope;

                float segmentLength = Vector2.Distance(
                    new Vector2(
                        compositionEvent.PreviousGlobalDistance,
                        ResolveAcrossMetresApproximation(
                            compositionEvent.PreviousAcrossNormalized)),
                    new Vector2(
                        headGlobalDistance,
                        ResolveAcrossMetresApproximation(
                            headAcrossNormalized)));

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
                            headAcrossNormalized,
                            headRadius,
                            headAmount);
                    QueueMaterialBirth(segment);
                    foamCompositionSegmentDispatchSubmittedCount++;
                    foamCompositionCumulativeCentrelineDistance +=
                        segmentLength;
                    PaintProgressiveBirthDebugSegment(segment);
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
            float headAcrossNormalized,
            float headRadius,
            float headAmount)
        {
            float previousAcrossMetres = ResolveAcrossMetresApproximation(
                compositionEvent.PreviousAcrossNormalized);
            float headAcrossMetres = ResolveAcrossMetresApproximation(
                headAcrossNormalized);
            Vector2 start = new Vector2(
                compositionEvent.PreviousGlobalDistance,
                previousAcrossMetres);
            Vector2 end = new Vector2(
                headGlobalDistance,
                headAcrossMetres);
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
            float centreAcross = ResolveAcrossNormalizedApproximation(
                (start.y + end.y) * 0.5f);
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
                compositionEvent.FragmentStrength,
                false,
                true,
                start.x,
                ResolveAcrossNormalizedApproximation(start.y),
                compositionEvent.PreviousRadius,
                compositionEvent.PreviousEmissionAmount,
                end.x,
                ResolveAcrossNormalizedApproximation(end.y),
                headRadius,
                headAmount,
                compositionEvent.SheetStyle,
                compositionEvent.Pattern,
                compositionEvent.Complexity,
                compositionEvent.Density);
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
            out float acrossNormalized)
        {
            globalDistance = compositionEvent.StartGlobalDistance +
                compositionEvent.FlowDirection *
                compositionEvent.TravelDistance * progress;
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
            latestFoamCompositionEventId = 0;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized = 0f;
            latestFoamCompositionHeadAcrossNormalized = 0f;
            latestFoamCompositionPreviousDistanceNormalized = 0f;
            latestFoamCompositionPreviousAcrossNormalized = 0f;
            lastFoamCompositionSegmentLength = 0f;
            ResetProgressiveBirthDiagnosticSession();
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

        private static float ResolveFoamCompositionHalfWidth(
            StylizedRiverFoamSpawnPreset pattern,
            float scale)
        {
            float multiplier = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.82f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 1.00f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 1.10f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 1.85f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 1.35f,
                _ => 1.00f
            };
            return scale * multiplier;
        }

        private static float ResolveFoamCompositionDuration(
            StylizedRiverFoamSpawnPreset pattern,
            float duration)
        {
            float multiplier = pattern switch
            {
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 1.00f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 1.00f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 1.00f,
                _ => 1.00f
            };
            return duration * multiplier;
        }

        private static float ResolveFoamCompositionTravelDistance(
            StylizedRiverFoamSpawnPreset pattern,
            float travelDistance)
        {
            float multiplier = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.58f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 0.75f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 0.72f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 0.90f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 0.95f,
                _ => 0.35f
            };
            return travelDistance * multiplier;
        }

        private static float ResolveFoamCompositionAcrossDrift(
            StylizedRiverFoamSpawnPreset pattern,
            float acrossDrift,
            float startAcross)
        {
            if (pattern == StylizedRiverFoamSpawnPreset.ShoreSkirt)
            {
                float inwardSign = startAcross >= 0f ? -1f : 1f;
                return inwardSign * Mathf.Abs(acrossDrift) * 0.35f;
            }

            float multiplier = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.30f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 0.12f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 0.10f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 0.04f,
                _ => 1.00f
            };
            return acrossDrift * multiplier;
        }

        private static float ResolveFoamCompositionStrokeAspect(
            StylizedRiverFoamSpawnPreset pattern,
            float complexity,
            float density)
        {
            float baseAspect = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 8.75f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 7.50f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 8.50f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 5.40f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 7.20f,
                _ => 1.45f
            };
            return Mathf.Clamp(
                baseAspect * Mathf.Lerp(0.88f, 1.15f, density) *
                Mathf.Lerp(0.95f, 1.08f, complexity),
                1f,
                12f);
        }

        private static float ResolveFoamCompositionFragmentStrength(
            StylizedRiverFoamSpawnPreset pattern,
            float complexity)
        {
            float baseStrength = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.84f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 0.22f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 0.78f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 0.86f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 0.58f,
                _ => 0f
            };
            return Mathf.Clamp01(baseStrength * Mathf.Lerp(0.45f, 1.20f, complexity));
        }

        private static float ResolveFoamCompositionWidthVariation(
            StylizedRiverFoamSpawnPreset pattern,
            float complexity)
        {
            float baseVariation = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.42f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 0.18f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 0.44f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 0.50f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 0.30f,
                _ => ProgressiveRibbonWidthVariation
            };
            return Mathf.Clamp(
                baseVariation * Mathf.Lerp(0.60f, 1.20f, complexity),
                0f,
                0.65f);
        }

        private static float ResolveFoamCompositionFeatureScale(
            StylizedRiverFoamSpawnPreset pattern,
            float complexity,
            float density)
        {
            float baseScale = pattern switch
            {
                StylizedRiverFoamSpawnPreset.ThinScratchStreaks => 0.72f,
                StylizedRiverFoamSpawnPreset.SmoothSurfaceLane => 1.25f,
                StylizedRiverFoamSpawnPreset.FracturedRibbonBundle => 0.72f,
                StylizedRiverFoamSpawnPreset.TornSheetRibbon => 0.50f,
                StylizedRiverFoamSpawnPreset.ShoreSkirt => 0.64f,
                _ => 1.00f
            };
            return Mathf.Clamp(
                baseScale * Mathf.Lerp(1.20f, 0.85f, density) *
                Mathf.Lerp(1.10f, 0.82f, complexity),
                0.35f,
                2.5f);
        }

        private static float Hash01(float value)
        {
            return Mathf.Repeat(
                Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f,
                1f);
        }
    }
}
