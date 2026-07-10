using System;
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

            if (activeFoamCompositionEventCount == 0)
            {
                ResetProgressiveBirthDiagnosticSession();
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

            foamCompositionEvents[slotIndex] = new FoamCompositionEvent
            {
                Active = true,
                EventId = eventId,
                StartGlobalDistance = startGlobalDistance,
                StartAcrossNormalized = startAcross,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = resolvedDrift,
                PathWander = resolvedWander,
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
                0f,
                false,
                true,
                start.x,
                ResolveAcrossNormalizedApproximation(start.y),
                compositionEvent.PreviousRadius,
                compositionEvent.PreviousEmissionAmount,
                end.x,
                ResolveAcrossNormalizedApproximation(end.y),
                headRadius,
                headAmount);
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
            ClearAutomaticFoamSourceEvents();
            latestFoamCompositionEventId = 0;
            latestFoamCompositionProgress = 0f;
            latestFoamCompositionHeadDistanceNormalized = 0f;
            latestFoamCompositionHeadAcrossNormalized = 0f;
            latestFoamCompositionPreviousDistanceNormalized = 0f;
            latestFoamCompositionPreviousAcrossNormalized = 0f;
            lastFoamCompositionSegmentLength = 0f;
            ResetProgressiveBirthDiagnosticSession();
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

            public float SlotSpacingMetres => Mathf.Lerp(
                AutomaticShoreSourceMaximumSlotSpacingMetres,
                AutomaticShoreSourceMinimumSlotSpacingMetres,
                Mathf.Sqrt(Coverage));
            public float EventsPerSecond => Mathf.Lerp(
                AutomaticShoreSourceMinimumEventsPerSecond,
                AutomaticShoreSourceMaximumEventsPerSecond,
                Activity);
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

            public float EventsPerSecond => Mathf.Lerp(
                AutomaticObjectSourceMinimumEventsPerSecond,
                AutomaticObjectSourceMaximumEventsPerSecond,
                Activity);
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

            public float SlotSpacingMetres => Mathf.Lerp(
                AutomaticFreeWaterSourceMaximumSlotSpacingMetres,
                AutomaticFreeWaterSourceMinimumSlotSpacingMetres,
                Mathf.Sqrt(Coverage));

            public float EventsPerSecond => Mathf.Lerp(
                AutomaticFreeWaterSourceMinimumEventsPerSecond,
                AutomaticFreeWaterSourceMaximumEventsPerSecond,
                Activity);
        }
        private bool IsAutomaticSourcePopulationActive =>
            river != null && river.FoamEnabled &&
            river.FoamAutomaticBirthEnabled &&
            river.FreezeAmount < 0.999f && river.Domain.IsValid &&
            ((river.FoamAutomaticShoreBirthActive &&
              river.FoamShoreFoamCoverage > 0.0001f &&
              river.FoamShoreFoamActivity > 0.0001f) ||
             (river.FoamAutomaticObjectBirthActive &&
              river.FoamObjectFoamCoverage > 0.0001f &&
              river.FoamObjectFoamActivity > 0.0001f) ||
             (river.FoamAutomaticFreeWaterBirthActive &&
              river.FoamFreeWaterFoamCoverage > 0.0001f &&
              river.FoamFreeWaterFoamActivity > 0.0001f));

        private bool AdvanceAutomaticBirthSources(
            float deltaTime,
            float now)
        {
            bool startedAny = false;
            startedAny |= AdvanceAutomaticShoreBirthSources(deltaTime, now);
            startedAny |= AdvanceAutomaticObjectBirthSources(deltaTime, now);
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
            float deltaTime,
            float now)
        {
            automaticObjectBirthSubmittedLastUpdate = 0;
            automaticObjectBirthRejectedLastUpdate = 0;
            automaticObjectBirthAnchorCountLastUpdate = 0;

            if (!ResolveAutomaticObjectSourceProfile(
                    out AutomaticObjectSourceProfile objectProfile,
                    out string inactiveStatus))
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus = inactiveStatus;
                return false;
            }

            disturbanceRuntime ??= GetComponent<StylizedRiverDisturbanceRuntime>();
            if (disturbanceRuntime == null)
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus = "Waiting for disturbance runtime";
                return false;
            }

            disturbanceRuntime.CopyStaticObjectFoamSourcesTo(
                automaticObjectFoamSources);
            automaticObjectBirthAnchorCountLastUpdate =
                automaticObjectFoamSources.Count;
            if (automaticObjectFoamSources.Count <= 0)
            {
                automaticObjectBirthAccumulator = 0f;
                automaticObjectBirthStatus =
                    "No registered static object source anchors";
                return false;
            }

            automaticObjectBirthAccumulator += Mathf.Max(0f, deltaTime) *
                objectProfile.EventsPerSecond;
            if (automaticObjectBirthAccumulator < 1f)
            {
                float secondsUntilNext =
                    (1f - automaticObjectBirthAccumulator) /
                    Mathf.Max(0.01f, objectProfile.EventsPerSecond);
                automaticObjectBirthStatus =
                    $"Armed / {automaticObjectFoamSources.Count} static object source(s) / next object source event in {secondsUntilNext:0.00}s";
                return false;
            }

            int startsThisUpdate = 0;
            int skippedThisUpdate = 0;
            while (automaticObjectBirthAccumulator >= 1f &&
                   startsThisUpdate < AutomaticObjectSourceMaximumStartsPerUpdate)
            {
                if (TryStartAutomaticObjectSourceEvent(
                        objectProfile,
                        out int skippedObjects))
                {
                    automaticObjectBirthAccumulator -= 1f;
                    startsThisUpdate++;
                    skippedThisUpdate += skippedObjects;
                    continue;
                }

                automaticObjectBirthAccumulator = Mathf.Min(
                    automaticObjectBirthAccumulator,
                    0.999f);
                skippedThisUpdate += skippedObjects;
                break;
            }

            automaticObjectBirthSubmittedLastUpdate = startsThisUpdate;
            automaticObjectBirthRejectedLastUpdate = skippedThisUpdate;
            automaticObjectBirthSubmittedTotal += startsThisUpdate;
            automaticObjectBirthStatus = startsThisUpdate > 0
                ? $"Started {startsThisUpdate} deterministic object source event(s), skipped {skippedThisUpdate} source(s)"
                : $"Scanned deterministic object source anchors, started 0, skipped {skippedThisUpdate}";
            return startsThisUpdate > 0;
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
            if (coverage <= 0.0001f)
            {
                inactiveStatus = "Object foam coverage is zero";
                return false;
            }

            if (activity <= 0.0001f)
            {
                inactiveStatus = "Object foam activity is zero";
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

        private bool TryStartAutomaticObjectSourceEvent(
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
                float sourceSeed = river.VisualSeed * 0.191f +
                    source.SourceId.GetHashCode() * 0.017f +
                    cycleIndex * 37.613f +
                    source.Phase * 11.0f;

                if (Hash01(sourceSeed + 1.7f) > profile.Coverage)
                {
                    skippedObjects++;
                    continue;
                }

                AutomaticObjectSourceRecipe recipe =
                    ResolveAutomaticObjectRecipe(profile.Pattern, sourceSeed);
                if (TryBeginAutomaticObjectSourceEvent(
                        profile,
                        recipe,
                        source,
                        sourceSeed))
                {
                    idleSince = 0.0;
                    return true;
                }

                skippedObjects++;
            }

            return false;
        }

        private AutomaticObjectSourceRecipe ResolveAutomaticObjectRecipe(
            StylizedRiverFoamObjectPattern pattern,
            float seed)
        {
            switch (pattern)
            {
                case StylizedRiverFoamObjectPattern.ContactArcs:
                    return AutomaticObjectSourceRecipe.ContactArc;
                case StylizedRiverFoamObjectPattern.ContactSemiArcs:
                    return AutomaticObjectSourceRecipe.ContactSemiArc;
                case StylizedRiverFoamObjectPattern.ContactFlecks:
                    return AutomaticObjectSourceRecipe.ContactFleck;
            }

            float arcWeight = river != null
                ? river.FoamObjectContactArcPatternWeight
                : 0.45f;
            float semiArcWeight = river != null
                ? river.FoamObjectContactSemiArcPatternWeight
                : 0.35f;
            float fleckWeight = river != null
                ? river.FoamObjectContactFleckPatternWeight
                : 0.20f;
            arcWeight = Mathf.Max(0f, arcWeight);
            semiArcWeight = Mathf.Max(0f, semiArcWeight);
            fleckWeight = Mathf.Max(0f, fleckWeight);
            float totalWeight = arcWeight + semiArcWeight + fleckWeight;
            if (totalWeight <= 0.0001f)
            {
                return AutomaticObjectSourceRecipe.ContactArc;
            }

            float roll = Hash01(seed + 4.1f) * totalWeight;
            if (roll < arcWeight)
            {
                return AutomaticObjectSourceRecipe.ContactArc;
            }

            roll -= arcWeight;
            return roll < semiArcWeight
                ? AutomaticObjectSourceRecipe.ContactSemiArc
                : AutomaticObjectSourceRecipe.ContactFleck;
        }

        private bool TryBeginAutomaticObjectSourceEvent(
            AutomaticObjectSourceProfile profile,
            AutomaticObjectSourceRecipe recipe,
            RiverFoamStaticObjectSource source,
            float seed)
        {
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float eventScale = Mathf.Clamp01(
                Mathf.Lerp(0.78f, 1.18f, Hash01(seed + 6.5f)));
            float widthJitter = Mathf.Lerp(0.92f, 1.08f, Hash01(seed + 7.1f));
            float offsetJitter = Mathf.Lerp(0.85f, 1.15f, Hash01(seed + 8.3f));
            float sourceKey = river.VisualSeed * 0.417f +
                source.GlobalDistance * 9.731f +
                source.AcrossMetres * 19.137f +
                source.SourceId.GetHashCode() * 0.011f +
                (recipe == AutomaticObjectSourceRecipe.ContactFleck
                    ? 907f
                    : (recipe == AutomaticObjectSourceRecipe.ContactSemiArc ? 809f : 701f));

            float length;
            float width;
            float offset;
            float amount;
            float remainingLife;
            float breakupScale;
            float breakupStrength;
            float patternFormationSpeedMultiplier;
            float lopsidedness = 0f;
            if (recipe == AutomaticObjectSourceRecipe.ContactFleck)
            {
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
                breakupScale = Mathf.Lerp(0.08f, 0.22f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamObjectContactFleckBreakupStrengthMin,
                    river.FoamObjectContactFleckBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamObjectContactFleckFormationSpeedMultiplier;
                amount = Mathf.Lerp(0.82f, 0.97f, eventScale);
            }
            else if (recipe == AutomaticObjectSourceRecipe.ContactSemiArc)
            {
                length = Mathf.Lerp(
                    river.FoamObjectContactSemiArcLengthMinMetres,
                    river.FoamObjectContactSemiArcLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamObjectContactSemiArcWidthMinMetres,
                    river.FoamObjectContactSemiArcWidthMaxMetres,
                    eventScale) * widthJitter;
                offset = Mathf.Lerp(
                    river.FoamObjectContactSemiArcOffsetMinMetres,
                    river.FoamObjectContactSemiArcOffsetMaxMetres,
                    eventScale) * offsetJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamObjectContactSemiArcInitialLifeMin,
                    river.FoamObjectContactSemiArcInitialLifeMax,
                    eventScale);
                breakupScale = Mathf.Lerp(0.12f, 0.38f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamObjectContactSemiArcBreakupStrengthMin,
                    river.FoamObjectContactSemiArcBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamObjectContactSemiArcFormationSpeedMultiplier;
                amount = Mathf.Lerp(0.84f, 0.98f, eventScale);
                float side = Hash01(seed + 13.9f) < 0.5f ? -1f : 1f;
                lopsidedness = side * Mathf.Lerp(
                    river.FoamObjectContactSemiArcLopsidednessMin,
                    river.FoamObjectContactSemiArcLopsidednessMax,
                    Hash01(seed + 14.7f));
            }
            else
            {
                length = Mathf.Lerp(
                    river.FoamObjectContactArcLengthMinMetres,
                    river.FoamObjectContactArcLengthMaxMetres,
                    eventScale);
                width = Mathf.Lerp(
                    river.FoamObjectContactArcWidthMinMetres,
                    river.FoamObjectContactArcWidthMaxMetres,
                    eventScale) * widthJitter;
                offset = Mathf.Lerp(
                    river.FoamObjectContactArcOffsetMinMetres,
                    river.FoamObjectContactArcOffsetMaxMetres,
                    eventScale) * offsetJitter;
                remainingLife = Mathf.Lerp(
                    river.FoamObjectContactArcInitialLifeMin,
                    river.FoamObjectContactArcInitialLifeMax,
                    eventScale);
                breakupScale = Mathf.Lerp(0.18f, 0.55f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamObjectContactArcBreakupStrengthMin,
                    river.FoamObjectContactArcBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamObjectContactArcFormationSpeedMultiplier;
                amount = Mathf.Lerp(0.88f, 1.0f, eventScale);
            }

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
            float feather = Mathf.Clamp(
                Mathf.Max(width * 0.65f, source.SurfaceHalfWidth * 0.010f),
                0.020f,
                0.110f);
            float halfLength = length * 0.5f;
            float startGlobalDistance = Mathf.Clamp(
                source.GlobalDistance - flowDirection * halfLength,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float endGlobalDistance = Mathf.Clamp(
                source.GlobalDistance + flowDirection * halfLength,
                river.Domain.GlobalDistanceMinimum,
                river.Domain.GlobalDistanceMaximum);
            float longitudinalDistance = Mathf.Abs(endGlobalDistance - startGlobalDistance);
            if (longitudinalDistance <= 0.05f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float formationSpeed = Mathf.Max(
                0.05f,
                profile.FormationSpeedMetresPerSecond *
                Mathf.Clamp(patternFormationSpeedMultiplier, 0.10f, 3.00f) *
                Mathf.Lerp(0.90f, 1.10f, Hash01(seed + 12.5f)));
            float sourcePathDistance = longitudinalDistance;
            float duration = Mathf.Clamp(
                sourcePathDistance / formationSpeed,
                AutomaticObjectSourceMinimumDuration,
                AutomaticObjectSourceMaximumDuration);
            float materialStepDuration = 1f / Mathf.Max(1f, ResolveUpdateRate());
            float headTrailMetres = Mathf.Clamp(
                Mathf.Max(feather * 1.35f, formationSpeed * materialStepDuration * 1.50f),
                AutomaticObjectSourceMinimumHeadTrailMetres,
                Mathf.Min(
                    AutomaticObjectSourceMaximumHeadTrailMetres,
                    Mathf.Max(AutomaticObjectSourceMinimumHeadTrailMetres, sourcePathDistance * 0.30f)));

            return BeginAutomaticObjectFoamSourceEvent(
                recipe,
                source,
                startGlobalDistance,
                endGlobalDistance,
                duration,
                formationSpeed,
                headTrailMetres,
                offset,
                width,
                feather,
                amount,
                remainingLife,
                sourceKey,
                breakupScale,
                breakupStrength,
                lopsidedness);
        }

        private bool BeginAutomaticObjectFoamSourceEvent(
            AutomaticObjectSourceRecipe recipe,
            RiverFoamStaticObjectSource source,
            float startGlobalDistance,
            float endGlobalDistance,
            float duration,
            float formationSpeedMetresPerSecond,
            float headTrailMetres,
            float contactOffsetMetres,
            float widthMetres,
            float featherMetres,
            float amount,
            float remainingLife,
            float sourceKey,
            float breakupScaleMetres,
            float breakupStrength,
            float lopsidedness)
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

            automaticFoamSourceEvents[slotIndex] = new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = eventId,
                Type = sourceType,
                SideSign = 1f,
                StartGlobalDistance = startGlobalDistance,
                EndGlobalDistance = endGlobalDistance,
                Duration = Mathf.Max(AutomaticObjectSourceMinimumDuration, duration),
                Elapsed = 0f,
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond),
                HeadTrailMetres = Mathf.Clamp(
                    headTrailMetres,
                    AutomaticObjectSourceMinimumHeadTrailMetres,
                    AutomaticObjectSourceMaximumHeadTrailMetres),
                ShoreInsetMetres = Mathf.Max(0f, contactOffsetMetres),
                WidthMetres = Mathf.Max(0.01f, widthMetres),
                InwardReachMetres = Mathf.Max(
                    0.01f,
                    Mathf.Max(source.StaticPressureAlongHalfLength, source.StaticPressureAcrossHalfWidth)),
                FeatherMetres = Mathf.Max(0.01f, featherMetres),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticObjectBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticObjectBirthSourceFillSeedSalt,
                SourceFillFeatureSize = Mathf.Max(
                    SourceFillMinimumFeatureSizeMetres * 0.55f,
                    Mathf.Max(widthMetres * 1.5f, featherMetres * 1.25f)),
                ShapeSeed = sourceKey + AutomaticObjectBirthShapeSeedSalt,
                BreakupScaleMetres = Mathf.Max(0.05f, breakupScaleMetres),
                BreakupStrength = Mathf.Clamp01(breakupStrength),
                Curvature = Mathf.Clamp(lopsidedness, -1f, 1f),
                ObjectCentreAcrossMetres = source.AcrossMetres,
                ObjectAlongHalfLengthMetres = Mathf.Max(
                    0.05f,
                    source.StaticPressureAlongHalfLength),
                ObjectAcrossHalfWidthMetres = Mathf.Max(
                    0.05f,
                    source.StaticPressureAcrossHalfWidth),
                ObjectContactOffsetMetres = Mathf.Max(0f, contactOffsetMetres)
            };

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
                float slotSeed = river.VisualSeed * 0.257f +
                    wrappedSlot * 23.719f +
                    cycleIndex * 41.137f;

                if (Hash01(slotSeed + 1.7f) > profile.Coverage)
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
            float eventScale = Mathf.Clamp01(Mathf.Lerp(0.72f, 1.16f, Hash01(seed + 6.5f)));
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
                breakupScale = Mathf.Lerp(0.10f, 0.42f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamFreeWaterFragmentBreakupStrengthMin,
                    river.FoamFreeWaterFragmentBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterFragmentFormationSpeedMultiplier;
                curvature = Mathf.Lerp(-1.0f, 1.0f, Hash01(seed + 11.7f));
                amount = Mathf.Lerp(0.76f, 0.94f, eventScale);
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
                breakupScale = Mathf.Lerp(0.20f, 0.68f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamFreeWaterCrossLaceBreakupStrengthMin,
                    river.FoamFreeWaterCrossLaceBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterCrossLaceFormationSpeedMultiplier;
                curvature = Mathf.Lerp(-1.0f, 1.0f, Hash01(seed + 11.7f));
                amount = Mathf.Lerp(0.78f, 0.96f, eventScale);
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
                breakupScale = Mathf.Lerp(0.20f, 0.70f, Hash01(seed + 9.5f));
                breakupStrength = Mathf.Lerp(
                    river.FoamFreeWaterLaceBreakupStrengthMin,
                    river.FoamFreeWaterLaceBreakupStrengthMax,
                    Hash01(seed + 10.1f));
                patternFormationSpeedMultiplier =
                    river.FoamFreeWaterLaceFormationSpeedMultiplier;
                float side = Hash01(seed + 11.7f) < 0.5f ? -1f : 1f;
                curvature = side * Mathf.Lerp(
                    river.FoamFreeWaterLaceCurvatureMin,
                    river.FoamFreeWaterLaceCurvatureMax,
                    Hash01(seed + 12.9f));
                amount = Mathf.Lerp(0.78f, 0.96f, eventScale);
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

            float formationSpeed = Mathf.Max(
                0.05f,
                profile.FormationSpeedMetresPerSecond *
                Mathf.Clamp(patternFormationSpeedMultiplier, 0.10f, 3.00f) *
                Mathf.Lerp(0.90f, 1.10f, Hash01(seed + 13.5f)));
            float duration = recipe == AutomaticFreeWaterSourceRecipe.TornFragment
                ? Mathf.Clamp(
                    0.35f + formationDistance / formationSpeed * 0.35f,
                    AutomaticFreeWaterSourceMinimumDuration,
                    1.35f)
                : Mathf.Clamp(
                    formationDistance / formationSpeed,
                    recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector ? 0.55f : 0.75f,
                    recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector ? 3.50f : AutomaticFreeWaterSourceMaximumDuration);
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
                duration,
                formationSpeed,
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
            float duration,
            float formationSpeedMetresPerSecond,
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
                Duration = Mathf.Max(AutomaticFreeWaterSourceMinimumDuration, duration),
                Elapsed = 0f,
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond),
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
                BreakupScaleMetres = Mathf.Max(0.05f, breakupScaleMetres),
                BreakupStrength = Mathf.Clamp01(breakupStrength),
                Curvature = Mathf.Clamp(curvature, -1f, 1f),
                ObjectCentreAcrossMetres = centreAcrossMetres,
                ObjectAlongHalfLengthMetres = halfLength,
                ObjectAcrossHalfWidthMetres = halfWidth,
                ObjectContactOffsetMetres = objectContactOffsetMetres,
                CentreAcrossNormalized = Mathf.Clamp(centreAcrossNormalized, -1f, 1f),
                LateralPaddingMetres = Mathf.Max(widthMetres * 2f, lateralPaddingMetres)
            };

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
                float slotSeed = river.VisualSeed * 0.137f +
                    wrappedSlot * 17.317f +
                    cycleIndex * 31.619f;

                if (Hash01(slotSeed + 1.7f) > profile.Coverage)
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
                    amount = Mathf.Lerp(0.84f, 0.98f, eventScale);
                    remainingLife = Mathf.Lerp(
                        river.FoamInwardWashInitialLifeMin,
                        river.FoamInwardWashInitialLifeMax,
                        eventScale);
                    breakupScale = Mathf.Lerp(0.14f, 0.34f, Hash01(seed + 9.5f));
                    breakupStrength = Mathf.Lerp(
                        river.FoamInwardWashBreakupStrengthMin,
                        river.FoamInwardWashBreakupStrengthMax,
                        Hash01(seed + 10.1f));
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
                    width = Mathf.Lerp(
                        river.FoamShoreRibbonWidthMinMetres,
                        river.FoamShoreRibbonWidthMaxMetres,
                        eventScale) * widthJitter;
                    shoreInset = Mathf.Lerp(
                        river.FoamShoreRibbonOffsetMinMetres,
                        river.FoamShoreRibbonOffsetMaxMetres,
                        eventScale) * offsetJitter;
                    width = Mathf.Min(width, Mathf.Max(0.018f, length * 0.040f));
                    inwardReach = Mathf.Lerp(0.16f, 0.42f, eventScale);
                    amount = Mathf.Lerp(0.90f, 1.00f, eventScale);
                    remainingLife = Mathf.Lerp(
                        river.FoamShoreRibbonInitialLifeMin,
                        river.FoamShoreRibbonInitialLifeMax,
                        eventScale);
                    breakupScale = Mathf.Lerp(0.42f, 1.05f, Hash01(seed + 9.5f));
                    breakupStrength = Mathf.Lerp(
                        river.FoamShoreRibbonBreakupStrengthMin,
                        river.FoamShoreRibbonBreakupStrengthMax,
                        Hash01(seed + 10.1f));
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
            inwardReach = Mathf.Clamp(
                inwardReach,
                0.06f,
                Mathf.Max(0.06f, visibleHalfWidth * 0.45f));
            shoreInset = Mathf.Clamp(
                shoreInset,
                0.005f,
                Mathf.Max(0.010f, visibleHalfWidth * 0.30f));
            width = Mathf.Clamp(
                width,
                0.012f,
                Mathf.Max(0.030f, visibleHalfWidth * 0.20f));
            feather = Mathf.Clamp(
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
            float formationSpeed = Mathf.Max(
                0.05f,
                profile.FormationSpeedMetresPerSecond *
                patternFormationSpeedMultiplier *
                Mathf.Lerp(0.88f, 1.12f, Hash01(seed + 12.5f)));
            float duration = Mathf.Clamp(
                sourcePathDistance / formationSpeed,
                AutomaticShoreSourceMinimumDuration,
                AutomaticShoreSourceMaximumDuration);
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
                duration,
                formationSpeed,
                headTrailMetres,
                shoreInset,
                width,
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
            float duration,
            float formationSpeedMetresPerSecond,
            float headTrailMetres,
            float shoreInsetMetres,
            float widthMetres,
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
                Duration = Mathf.Max(AutomaticShoreSourceMinimumDuration, duration),
                Elapsed = 0f,
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, formationSpeedMetresPerSecond),
                HeadTrailMetres = Mathf.Clamp(
                    headTrailMetres,
                    slotMinimumHeadTrailMetres,
                    slotMaximumHeadTrailMetres),
                ShoreInsetMetres = Mathf.Max(0f, shoreInsetMetres),
                WidthMetres = Mathf.Max(0.01f, widthMetres),
                InwardReachMetres = Mathf.Max(0.01f, inwardReachMetres),
                FeatherMetres = Mathf.Max(0.01f, featherMetres),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticShoreBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticShoreBirthSourceFillSeedSalt,
                SourceFillFeatureSize = Mathf.Max(
                    SourceFillMinimumFeatureSizeMetres,
                    sourceType == AutomaticFoamSourceEventType.InwardWash
                        ? Mathf.Max(widthMetres * 1.35f, featherMetres * 1.25f)
                        : Mathf.Max(widthMetres, inwardReachMetres * 0.45f)),
                ShapeSeed = sourceKey + AutomaticShoreBirthShapeSeedSalt,
                BreakupScaleMetres = Mathf.Max(0.10f, breakupScaleMetres),
                BreakupStrength = Mathf.Clamp01(breakupStrength),
                Curvature = curvature
            };

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
            activeAutomaticFoamSourceEventCount = 0;
            automaticSourceEventsRasterizedLastUpdate = 0;
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

            if (activeFoamCompositionEventCount == 0)
            {
                ResetProgressiveBirthDiagnosticSession();
            }

            int eventId = ++foamCompositionSequence;
            float startAcross = Mathf.Clamp(startAcrossNormalized, -1f, 1f);
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
                EventId = eventId,
                StartGlobalDistance = clampedStartGlobalDistance,
                StartAcrossNormalized = startAcross,
                Duration = resolvedDuration,
                TravelDistance = resolvedTravelDistance,
                FlowDirection = flowDirection,
                AcrossDrift = resolvedDrift,
                PathWander = resolvedWander,
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
                PreviousRadius = startRadius,
                PreviousEmissionAmount = Mathf.Clamp01(
                    resolvedAmount * Mathf.Clamp01(amountEnvelopeFloor)),
                DebugTrajectoryPending = true
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
