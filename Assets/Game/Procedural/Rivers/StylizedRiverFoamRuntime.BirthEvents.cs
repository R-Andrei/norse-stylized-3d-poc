using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
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
                float activity,
                float patchSize,
                float formationSpeedMetresPerSecond,
                StylizedRiverFoamShorePattern pattern)
            {
                Enabled = enabled;
                Activity = Mathf.Clamp01(activity);
                PatchSize = Mathf.Clamp01(patchSize);
                FormationSpeedMetresPerSecond = Mathf.Max(
                    0.01f,
                    formationSpeedMetresPerSecond);
                Pattern = pattern;
            }

            public bool Enabled { get; }
            public float Activity { get; }
            public float PatchSize { get; }
            public float FormationSpeedMetresPerSecond { get; }
            public StylizedRiverFoamShorePattern Pattern { get; }

            public float SlotSpacingMetres =>
                AutomaticShoreSourceSlotSpacingMetres;
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
            automaticPacketEnvelopeRejectedLastUpdate = 0;
            RefreshAutomaticFoamPacketReservations(now);
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
            automaticShorePopulationActiveBankLengthMetres =
                Mathf.Max(0f, validFieldLength) * 2f;

            if (!ResolveAutomaticShoreSourceProfile(
                    out AutomaticShoreSourceProfile shoreProfile,
                    out string inactiveStatus))
            {
                automaticShorePopulationMeanHeadCount = 0f;
                automaticShorePopulationMinimumHeadCount = 0;
                automaticShorePopulationMaximumHeadCount = 0;
                automaticShorePopulationTargetHeadCount = 0;
                automaticShorePopulationAuthoritySignature = int.MinValue;
                automaticShorePopulationTargetRefreshPending = true;
                automaticShorePopulationNextBoundaryTime = -1f;
                automaticShoreBirthStatus =
                    $"{inactiveStatus}; active {activeAutomaticShoreSourceEventCount}, " +
                    "target 0, predicted 0";
                return false;
            }

            int totalSlotCount = ResolveAutomaticShoreTotalSlotCount(
                validFieldLength);
            UpdateAutomaticShorePopulationTarget(
                shoreProfile,
                now,
                totalSlotCount);

            string predictedRange =
                automaticShorePopulationMinimumHeadCount ==
                    automaticShorePopulationMaximumHeadCount
                    ? automaticShorePopulationMinimumHeadCount.ToString()
                    : $"{automaticShorePopulationMinimumHeadCount}-" +
                      $"{automaticShorePopulationMaximumHeadCount}";

            if (activeAutomaticShoreSourceEventCount >=
                automaticShorePopulationTargetHeadCount)
            {
                automaticShoreBirthStatus =
                    $"Active {activeAutomaticShoreSourceEventCount}; " +
                    $"target {automaticShorePopulationTargetHeadCount}; " +
                    $"predicted {predictedRange} " +
                    $"(mean {automaticShorePopulationMeanHeadCount:0.##}); " +
                    $"shoreline {automaticShorePopulationActiveBankLengthMetres:0.#} m";
                return false;
            }

            int scanBudget = Mathf.Min(
                Mathf.Max(2, totalSlotCount),
                AutomaticShoreSourceMaximumScansPerUpdate);
            int startsThisUpdate = 0;
            int skippedThisUpdate = 0;
            int initializedThisUpdate = 0;

            for (int scan = 0;
                 scan < scanBudget &&
                 startsThisUpdate < AutomaticShoreSourceMaximumStartsPerUpdate &&
                 activeAutomaticShoreSourceEventCount <
                    automaticShorePopulationTargetHeadCount;
                 scan++)
            {
                int slotCursor = automaticShoreBirthCursor++;
                int scanCycle = slotCursor / Mathf.Max(1, totalSlotCount);
                int scanIndex = PositiveModulo(slotCursor, totalSlotCount);
                int wrappedSlot = ResolvePermutedAutomaticShoreSlot(
                    scanIndex,
                    totalSlotCount,
                    scanCycle);

                if (!automaticShoreSlotSchedules.TryGetValue(
                        wrappedSlot,
                        out AutomaticShoreSlotScheduleState schedule) ||
                    !schedule.Initialized)
                {
                    schedule = CreateAutomaticShoreSlotSchedule(
                        shoreProfile,
                        wrappedSlot,
                        now,
                        0);
                    automaticShoreSlotSchedules[wrappedSlot] = schedule;
                    initializedThisUpdate++;
                }

                if (schedule.ActiveEventId > 0 ||
                    now + 0.0001f < schedule.NextStartTime)
                {
                    skippedThisUpdate++;
                    continue;
                }

                if (TryStartAutomaticShoreSourceEvent(
                        shoreProfile,
                        wrappedSlot,
                        schedule.CycleIndex,
                        out _,
                        out int eventId))
                {
                    schedule.ActiveEventId = eventId;
                    schedule.NextStartTime = float.PositiveInfinity;
                    schedule.CycleIndex++;
                    automaticShoreSlotSchedules[wrappedSlot] = schedule;
                    startsThisUpdate++;
                    idleSince = 0.0;
                    continue;
                }

                schedule.CycleIndex++;
                schedule.NextStartTime = now +
                    ResolveAutomaticShoreRetryDelaySeconds();
                automaticShoreSlotSchedules[wrappedSlot] = schedule;
                skippedThisUpdate++;
            }

            automaticShoreBirthSubmittedLastUpdate = startsThisUpdate;
            automaticShoreBirthRejectedLastUpdate = skippedThisUpdate;
            automaticShoreBirthSubmittedTotal += startsThisUpdate;
            string fillStatus = activeAutomaticShoreSourceEventCount <
                    automaticShorePopulationTargetHeadCount
                ? "waiting for packet clearance or a valid shoreline start"
                : "target satisfied";
            automaticShoreBirthStatus =
                $"Active {activeAutomaticShoreSourceEventCount}; " +
                $"target {automaticShorePopulationTargetHeadCount}; " +
                $"predicted {predictedRange} " +
                $"(mean {automaticShorePopulationMeanHeadCount:0.##}); " +
                $"shoreline {automaticShorePopulationActiveBankLengthMetres:0.#} m; " +
                $"{fillStatus}; started {startsThisUpdate}, " +
                $"scanned {scanBudget}/{totalSlotCount}, skipped {skippedThisUpdate}, " +
                $"initialized {initializedThisUpdate}";
            return startsThisUpdate > 0;
        }

        private static int ResolveAutomaticShoreLongitudinalSlotCount(
            float lengthMetres)
        {
            return Mathf.Max(
                1,
                Mathf.CeilToInt(
                    Mathf.Max(0f, lengthMetres) /
                    Mathf.Max(0.25f, AutomaticShoreSourceSlotSpacingMetres)));
        }

        private static int ResolveAutomaticShoreTotalSlotCount(
            float lengthMetres)
        {
            return ResolveAutomaticShoreLongitudinalSlotCount(lengthMetres) * 2;
        }

        private AutomaticShoreSlotScheduleState CreateAutomaticShoreSlotSchedule(
            AutomaticShoreSourceProfile profile,
            int slotId,
            float now,
            int cycleIndex)
        {
            float phaseWindow = Mathf.Min(
                1f,
                ResolveAutomaticShorePopulationBoundarySeconds(profile));
            int resolvedCycleIndex = Mathf.Max(0, cycleIndex);
            float phase = Hash01(
                river.VisualSeed * 0.419f +
                slotId * 23.117f +
                resolvedCycleIndex * 31.619f);
            return new AutomaticShoreSlotScheduleState
            {
                Initialized = true,
                CycleIndex = resolvedCycleIndex,
                ActiveEventId = 0,
                NextStartTime = now + phase * phaseWindow
            };
        }

        private void UpdateAutomaticShorePopulationTarget(
            AutomaticShoreSourceProfile profile,
            float now,
            int totalSlotCount)
        {
            automaticShorePopulationActiveBankLengthMetres =
                Mathf.Max(0f, validFieldLength) * 2f;
            automaticShorePopulationMeanHeadCount = Mathf.Clamp(
                profile.Activity *
                automaticShorePopulationActiveBankLengthMetres /
                Mathf.Max(
                    0.01f,
                    StylizedRiver.AutomaticShoreFullActivityHeadSpacingMetres),
                0f,
                Mathf.Max(0, totalSlotCount));
            automaticShorePopulationMinimumHeadCount = Mathf.Clamp(
                Mathf.FloorToInt(automaticShorePopulationMeanHeadCount),
                0,
                Mathf.Max(0, totalSlotCount));
            automaticShorePopulationMaximumHeadCount = Mathf.Clamp(
                Mathf.CeilToInt(automaticShorePopulationMeanHeadCount),
                automaticShorePopulationMinimumHeadCount,
                Mathf.Max(0, totalSlotCount));

            int authoritySignature =
                ResolveAutomaticShorePopulationAuthoritySignature(
                    profile,
                    totalSlotCount);
            float boundarySeconds =
                ResolveAutomaticShorePopulationBoundarySeconds(profile);
            if (authoritySignature != automaticShorePopulationAuthoritySignature)
            {
                automaticShorePopulationAuthoritySignature = authoritySignature;
                automaticShorePopulationEpochIndex = 0;
                automaticShorePopulationNextBoundaryTime =
                    now + boundarySeconds;
                automaticShorePopulationTargetRefreshPending = true;
            }
            else if (automaticShorePopulationNextBoundaryTime < 0f)
            {
                automaticShorePopulationNextBoundaryTime =
                    now + boundarySeconds;
                automaticShorePopulationTargetRefreshPending = true;
            }
            else if (now + 0.0001f >=
                automaticShorePopulationNextBoundaryTime)
            {
                int elapsedBoundaries = Mathf.Max(
                    1,
                    Mathf.FloorToInt(
                        (now - automaticShorePopulationNextBoundaryTime) /
                        boundarySeconds) + 1);
                automaticShorePopulationEpochIndex += elapsedBoundaries;
                automaticShorePopulationNextBoundaryTime +=
                    elapsedBoundaries * boundarySeconds;
                automaticShorePopulationTargetRefreshPending = true;
            }

            if (!automaticShorePopulationTargetRefreshPending)
            {
                return;
            }

            float fractionalDuty =
                automaticShorePopulationMeanHeadCount -
                automaticShorePopulationMinimumHeadCount;
            float phase = Hash01(
                river.VisualSeed * 0.613f +
                automaticShorePopulationEpochIndex * 37.271f +
                authoritySignature * 0.000173f);
            automaticShorePopulationTargetHeadCount = Mathf.Clamp(
                automaticShorePopulationMinimumHeadCount +
                (phase < fractionalDuty ? 1 : 0),
                0,
                Mathf.Max(0, totalSlotCount));
            automaticShorePopulationTargetRefreshPending = false;
        }

        private int ResolveAutomaticShorePopulationAuthoritySignature(
            AutomaticShoreSourceProfile profile,
            int totalSlotCount)
        {
            unchecked
            {
                int signature = 17;
                signature = signature * 31 +
                    Mathf.RoundToInt(profile.Activity * 10000f);
                signature = signature * 31 +
                    Mathf.RoundToInt(validFieldLength * 100f);
                signature = signature * 31 + totalSlotCount;
                signature = signature * 31 + (int)profile.Pattern;
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonLengthMinCells * 10f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonLengthMaxCells * 10f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonRevealSpeedCellsPerSecond * 100f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamInwardWashAlongLengthMinCells * 10f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamInwardWashAlongLengthMaxCells * 10f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamInwardWashRevealSpeedCellsPerSecond * 100f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonPatternWeight * 1000f);
                signature = signature * 31 +
                    Mathf.RoundToInt(
                        river.FoamInwardWashPatternWeight * 1000f);
                return signature;
            }
        }

        private float ResolveAutomaticShorePopulationBoundarySeconds(
            AutomaticShoreSourceProfile profile)
        {
            float materialStepDuration =
                1f / Mathf.Max(1f, ResolveUpdateRate());
            float ribbonLength = 0.5f * (
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonLengthMinCells)) +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        river.FoamShoreRibbonLengthMaxCells)));
            float ribbonDuration = ribbonLength /
                Mathf.Max(0.01f, river.FoamShoreRibbonRevealSpeedCellsPerSecond);
            float inwardAlong = 0.5f * (
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        river.FoamInwardWashAlongLengthMinCells)) +
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        river.FoamInwardWashAlongLengthMaxCells)));
            float inwardReach = 0.5f * (
                Mathf.Max(0f, river.FoamInwardWashReachMinCells) +
                Mathf.Max(0f, river.FoamInwardWashReachMaxCells));
            float inwardDuration = Mathf.Sqrt(
                    inwardAlong * inwardAlong +
                    inwardReach * inwardReach) /
                Mathf.Max(0.01f, river.FoamInwardWashRevealSpeedCellsPerSecond);

            float referenceDuration;
            switch (profile.Pattern)
            {
                case StylizedRiverFoamShorePattern.ShoreRibbons:
                    referenceDuration = ribbonDuration;
                    break;
                case StylizedRiverFoamShorePattern.InwardWash:
                    referenceDuration = inwardDuration;
                    break;
                default:
                    float ribbonWeight = Mathf.Max(
                        0f,
                        river.FoamShoreRibbonPatternWeight);
                    float inwardWeight = Mathf.Max(
                        0f,
                        river.FoamInwardWashPatternWeight);
                    float totalWeight = ribbonWeight + inwardWeight;
                    referenceDuration = totalWeight > 0.0001f
                        ? (ribbonDuration * ribbonWeight +
                           inwardDuration * inwardWeight) / totalWeight
                        : ribbonDuration;
                    break;
            }

            return Mathf.Clamp(
                Mathf.Max(materialStepDuration, referenceDuration),
                0.5f,
                10f);
        }

        private float ResolveAutomaticShoreRetryDelaySeconds()
        {
            return Mathf.Max(
                0.10f,
                1f / Mathf.Max(1f, ResolveUpdateRate()));
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

            float activity = river.FoamShoreFoamActivity;
            if (activity <= 0.0001f)
            {
                inactiveStatus = "Shore foam activity is zero";
                return false;
            }

            profile = new AutomaticShoreSourceProfile(
                true,
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
            RefreshAutomaticObjectReinforcementAuthority();

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
            int reinforcementStarts = 0;
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

            if (river.FoamObjectContactReinforcementEnabled &&
                river.FoamObjectContactCyclesEnabled)
            {
                while (cycleStarts + reinforcementStarts <
                           AutomaticObjectSourceMaximumStartsPerUpdate &&
                       TryStartAutomaticObjectContactReinforcement(
                           objectProfile,
                           out int skippedObjects))
                {
                    reinforcementStarts++;
                    skippedThisUpdate += skippedObjects;
                }
            }

            if (fleckEventsPerSecond > 0.0001f)
            {
                while (automaticObjectBirthAccumulator >= 1f &&
                       cycleStarts + reinforcementStarts + fleckStarts <
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

            int startsThisUpdate = cycleStarts + reinforcementStarts + fleckStarts;
            automaticObjectBirthSubmittedLastUpdate = startsThisUpdate;
            automaticObjectBirthRejectedLastUpdate = skippedThisUpdate;
            automaticObjectBirthSubmittedTotal += startsThisUpdate;
            RefreshAutomaticObjectSourcePacketDiagnostics();
            automaticObjectBirthStatus =
                $"Object packets {automaticObjectContactBuildCount} full / " +
                $"{automaticObjectContactReinforcementCount} reinforcement / " +
                $"{automaticObjectWaitingClearanceCount} waiting for packet clearance; " +
                $"started {cycleStarts} full + {reinforcementStarts} reinforcement + " +
                $"{fleckStarts} Fleck, skipped {skippedThisUpdate}";
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
                state.NextPacketStartTime = automaticObjectContactCycleTime;
                state.NextReinforcementTime = float.PositiveInfinity;
                state.LastEventType = AutomaticFoamSourceEventType.None;
                state.LastContactEventType = AutomaticFoamSourceEventType.None;
                state.LastContactSeed = 0f;
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
                state.NextPacketStartTime = automaticObjectContactCycleTime;
                automaticObjectSourceStates[sourceId] = state;
            }
            automaticObjectContactStaleSourceIds.Clear();
        }

        private void RefreshAutomaticObjectReinforcementAuthority()
        {
            if (river == null)
            {
                return;
            }

            int signature;
            unchecked
            {
                signature = 41;
                signature = signature * 31 +
                    (river.FoamObjectContactReinforcementEnabled ? 1 : 0);
                signature = signature * 31 + Mathf.RoundToInt(
                    river.FoamObjectContactReinforcementIntervalSeconds * 1000f);
            }

            if (automaticObjectReinforcementAuthoritySignature == int.MinValue)
            {
                automaticObjectReinforcementAuthoritySignature = signature;
                return;
            }

            if (automaticObjectReinforcementAuthoritySignature == signature)
            {
                return;
            }

            automaticObjectReinforcementAuthoritySignature = signature;
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
                state.NextReinforcementTime =
                    river.FoamObjectContactReinforcementEnabled &&
                    IsAutomaticObjectContactCycle(state.LastContactEventType)
                        ? automaticObjectContactCycleTime +
                            river.FoamObjectContactReinforcementIntervalSeconds
                        : float.PositiveInfinity;
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
                        state.NextPacketStartTime)
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
                        cycleSeed,
                        false))
                {
                    state.CycleIndex++;
                    state.NextPacketStartTime = float.PositiveInfinity;
                    state.NextReinforcementTime = float.PositiveInfinity;
                    state.LastContactEventType =
                        recipe == AutomaticObjectSourceRecipe.ContactSemiArc
                            ? AutomaticFoamSourceEventType.ObjectContactSemiArc
                            : AutomaticFoamSourceEventType.ObjectContactArc;
                    state.LastContactSeed = cycleSeed;
                    automaticObjectSourceStates[source.SourceId] = state;
                    idleSince = 0.0;
                    return true;
                }

                skippedObjects++;
            }

            return false;
        }

        private bool TryStartAutomaticObjectContactReinforcement(
            AutomaticObjectSourceProfile profile,
            out int skippedObjects)
        {
            skippedObjects = 0;
            int sourceCount = automaticObjectFoamSources.Count;
            if (river == null || !river.Domain.IsValid || sourceCount <= 0 ||
                !river.FoamObjectContactReinforcementEnabled)
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

                bool hasContactRecipe =
                    IsAutomaticObjectContactCycle(state.LastContactEventType);
                bool fullPacketStillWaiting =
                    automaticObjectContactCycleTime + 0.0001f <
                        state.NextPacketStartTime;
                bool reinforcementDue =
                    automaticObjectContactCycleTime + 0.0001f >=
                        state.NextReinforcementTime;
                if (!hasContactRecipe || !fullPacketStillWaiting ||
                    !reinforcementDue ||
                    HasActiveAutomaticObjectSource(source.SourceId))
                {
                    skippedObjects++;
                    continue;
                }

                AutomaticObjectSourceRecipe recipe =
                    state.LastContactEventType ==
                        AutomaticFoamSourceEventType.ObjectContactSemiArc
                            ? AutomaticObjectSourceRecipe.ContactSemiArc
                            : AutomaticObjectSourceRecipe.ContactArc;
                if (TryBeginAutomaticObjectSourceEvent(
                        profile,
                        recipe,
                        source,
                        state.LastContactSeed,
                        true))
                {
                    state.NextReinforcementTime = float.PositiveInfinity;
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
                        state.NextPacketStartTime ||
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
                        sourceSeed,
                        false))
                {
                    state.CycleIndex++;
                    state.NextPacketStartTime = float.PositiveInfinity;
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
                NextPacketStartTime = automaticObjectContactCycleTime,
                NextReinforcementTime = float.PositiveInfinity,
                LastEventType = AutomaticFoamSourceEventType.None,
                LastContactEventType = AutomaticFoamSourceEventType.None,
                LastContactSeed = 0f
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

        private void CompleteAutomaticShoreSourceEvent(
            AutomaticFoamSourceEvent sourceEvent)
        {
            if ((sourceEvent.Type != AutomaticFoamSourceEventType.ShoreRibbon &&
                 sourceEvent.Type != AutomaticFoamSourceEventType.InwardWash) ||
                sourceEvent.ShoreScheduleSlotId < 0)
            {
                return;
            }

            if (!automaticShoreSlotSchedules.TryGetValue(
                    sourceEvent.ShoreScheduleSlotId,
                    out AutomaticShoreSlotScheduleState schedule) ||
                schedule.ActiveEventId != sourceEvent.EventId)
            {
                return;
            }

            schedule.ActiveEventId = 0;
            float clearance = river != null
                ? ResolveAutomaticPacketClearanceSeconds(
                    river.FoamShoreMinimumPacketGapMetres)
                : 0f;
            schedule.NextStartTime = Time.realtimeSinceStartup + clearance;
            automaticShoreSlotSchedules[sourceEvent.ShoreScheduleSlotId] =
                schedule;
            automaticShorePopulationEpochIndex++;
            automaticShorePopulationNextBoundaryTime = -1f;
            automaticShorePopulationTargetRefreshPending = true;
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
            if (sourceEvent.ObjectContactReinforcementOnly)
            {
                state.NextReinforcementTime =
                    river != null && river.FoamObjectContactReinforcementEnabled
                        ? automaticObjectContactCycleTime +
                            river.FoamObjectContactReinforcementIntervalSeconds
                        : float.PositiveInfinity;
                automaticObjectSourceStates[sourceEvent.ObjectSourceId] = state;
                return;
            }

            state.LastEventType = sourceEvent.Type;
            float clearanceSeconds =
                ResolveAutomaticObjectPacketClearanceSeconds(sourceEvent);
            state.NextPacketStartTime = float.IsPositiveInfinity(clearanceSeconds)
                ? float.PositiveInfinity
                : automaticObjectContactCycleTime + clearanceSeconds;
            if (IsAutomaticObjectContactCycle(sourceEvent.Type))
            {
                state.NextReinforcementTime =
                    river != null && river.FoamObjectContactReinforcementEnabled
                        ? automaticObjectContactCycleTime +
                            river.FoamObjectContactReinforcementIntervalSeconds
                        : float.PositiveInfinity;
            }
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
            automaticObjectContactReinforcementCount = 0;
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
                    if (sourceEvent.ObjectContactReinforcementOnly)
                    {
                        automaticObjectContactReinforcementCount++;
                    }
                    else
                    {
                        automaticObjectContactBuildCount++;
                    }
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
                    pair.Value.NextPacketStartTime > automaticObjectContactCycleTime)
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
            float seed,
            bool contactOnlyReinforcement)
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
            float objectContactPathLengthMetres = 0f;
            float objectContactStrokePathLengthMetres = 0f;
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
                objectContactStrokePathLengthMetres = Mathf.Max(
                    0.001f,
                    selectedFrontLength);
                float fullContactRingPathLengthMetres = Mathf.Max(
                    0.001f,
                    resolvedContactProfile.FrontPathLength * 2f);
                objectContactPathLengthMetres = Mathf.Max(
                    0.001f,
                    fullContactRingPathLengthMetres +
                    negativeArmLength +
                    positiveArmLength);
                sourcePathDistance = contactOnlyReinforcement
                    ? objectContactStrokePathLengthMetres
                    : objectContactPathLengthMetres;
                float dispatchNegativeArmLength = contactOnlyReinforcement
                    ? 0f
                    : negativeArmLength;
                float dispatchPositiveArmLength = contactOnlyReinforcement
                    ? 0f
                    : positiveArmLength;

                float minimumLocalX;
                float maximumLocalX;
                float maximumAbsoluteY;
                if (semiArc && lopsidedness < 0f)
                {
                    Vector2 armTip = resolvedContactProfile.Point0 +
                        new Vector2(dispatchNegativeArmLength, 0f);
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
                        new Vector2(dispatchPositiveArmLength, 0f);
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
                        new Vector2(dispatchNegativeArmLength, 0f);
                    Vector2 positiveArmTip = resolvedContactProfile.Point4 +
                        new Vector2(dispatchPositiveArmLength, 0f);
                    minimumLocalX = resolvedContactProfile.MinimumX;
                    maximumLocalX = Mathf.Max(
                        resolvedContactProfile.MaximumX,
                        Mathf.Max(negativeArmTip.x, positiveArmTip.x));
                    maximumAbsoluteY =
                        resolvedContactProfile.MaximumAbsoluteY;
                }

                if (!contactOnlyReinforcement)
                {
                    minimumLocalX = Mathf.Min(
                        minimumLocalX,
                        -objectAlongHalfLengthMetres);
                    maximumLocalX = Mathf.Max(
                        maximumLocalX,
                        objectAlongHalfLengthMetres);
                    maximumAbsoluteY = Mathf.Max(
                        maximumAbsoluteY,
                        objectAcrossHalfWidthMetres);
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

            float minimumAcceptedPathDistance = contactOnlyReinforcement
                ? 0.001f
                : 0.05f;
            if (sourcePathDistance <= minimumAcceptedPathDistance)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float revealSpeedJitter = Mathf.Lerp(
                0.90f,
                1.10f,
                Hash01(seed + 12.5f));
            ResolvedAutomaticRevealTiming revealTiming =
                ResolveAutomaticRevealTiming(
                    sourcePathDistance,
                    profile.FormationSpeedMetresPerSecond,
                    patternFormationSpeedMultiplier,
                    revealSpeedJitter);
            ResolvedAutomaticRevealTiming contactStrokeRevealTiming =
                recipe == AutomaticObjectSourceRecipe.ContactFleck
                    ? revealTiming
                    : ResolveAutomaticRevealTiming(
                        objectContactStrokePathLengthMetres,
                        profile.FormationSpeedMetresPerSecond,
                        patternFormationSpeedMultiplier,
                        revealSpeedJitter);
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
                contactStrokeRevealTiming,
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
                objectContactPathLengthMetres,
                objectContactStrokePathLengthMetres,
                resolvedContactProfile,
                contactOnlyReinforcement);
        }

        private bool BeginAutomaticObjectFoamSourceEvent(
            AutomaticObjectSourceRecipe recipe,
            RiverFoamStaticObjectSource source,
            float startGlobalDistance,
            float endGlobalDistance,
            float objectCentreGlobalDistance,
            ResolvedAutomaticRevealTiming revealTiming,
            ResolvedAutomaticRevealTiming contactStrokeRevealTiming,
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
            float objectContactStrokePathLengthMetres,
            ResolvedAutomaticObjectContactProfile contactProfile,
            bool contactOnlyReinforcement)
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

            int eventId = foamCompositionSequence + 1;
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
            float resolvedContactStrokeDuration = contactCycle
                ? contactStrokeRevealTiming.ResolvedDurationSeconds
                : resolvedBuildDuration;
            int objectContactStrokeCount = contactCycle && river != null &&
                !contactOnlyReinforcement
                    ? river.FoamObjectContactStrokeCount
                    : 1;
            float resolvedEventDuration = contactCycle &&
                !contactOnlyReinforcement
                    ? resolvedBuildDuration +
                        Mathf.Max(0, objectContactStrokeCount - 1) *
                        resolvedContactStrokeDuration
                    : resolvedBuildDuration;

            AutomaticFoamSourceEvent candidateEvent = new AutomaticFoamSourceEvent
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
                ObjectContactStrokeDuration = resolvedContactStrokeDuration,
                ObjectContactStrokePathLengthMetres = contactCycle
                    ? Mathf.Max(0.001f, objectContactStrokePathLengthMetres)
                    : 0f,
                ObjectContactStrokeRawRevealDurationSeconds = contactCycle
                    ? contactStrokeRevealTiming.RawDurationSeconds
                    : 0f,
                ObjectContactStrokeRevealCadenceLimited = contactCycle &&
                    contactStrokeRevealTiming.CadenceLimited,
                ObjectContactStrokeCount = objectContactStrokeCount,
                ObjectContactReinforcementOnly = contactOnlyReinforcement,
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

            float objectLengthHash = Hash01(sourceKey + 61.17f);
            float objectWidthHash = Hash01(sourceKey + 67.31f);
            float objectWakeLengthHash = Hash01(sourceKey + 71.53f);
            float objectWakeWidthHash = Hash01(sourceKey + 79.07f);
            if (sourceType == AutomaticFoamSourceEventType.ObjectContactFleck)
            {
                candidateEvent.BodyLengthCells = Mathf.Lerp(
                    river.FoamObjectFleckLengthMinCells,
                    river.FoamObjectFleckLengthMaxCells,
                    objectLengthHash);
                candidateEvent.BodyWidthCells = Mathf.Lerp(
                    river.FoamObjectFleckWidthMinCells,
                    river.FoamObjectFleckWidthMaxCells,
                    objectWidthHash);
                candidateEvent.HeadLengthCells = river.FoamObjectFleckHeadLengthCells;
                candidateEvent.HeadWidthCells = river.FoamObjectFleckHeadWidthCells;
                candidateEvent.ShoreInsetMetres = Mathf.Lerp(
                    river.FoamObjectFleckOffsetMinCells,
                    river.FoamObjectFleckOffsetMaxCells,
                    Hash01(sourceKey + 83.29f));
            }
            else
            {
                bool semiArcCells = sourceType == AutomaticFoamSourceEventType.ObjectContactSemiArc;
                candidateEvent.ContactSpanCells = Mathf.Lerp(
                    semiArcCells ? river.FoamObjectSemiArcContactSpanMinCells : river.FoamObjectArcContactSpanMinCells,
                    semiArcCells ? river.FoamObjectSemiArcContactSpanMaxCells : river.FoamObjectArcContactSpanMaxCells,
                    objectLengthHash);
                candidateEvent.ContactWidthCells = Mathf.Lerp(
                    semiArcCells ? river.FoamObjectSemiArcContactWidthMinCells : river.FoamObjectArcContactWidthMinCells,
                    semiArcCells ? river.FoamObjectSemiArcContactWidthMaxCells : river.FoamObjectArcContactWidthMaxCells,
                    objectWidthHash);
                candidateEvent.WakeLengthCells = Mathf.Lerp(
                    semiArcCells ? river.FoamObjectSemiArcWakeLengthMinCells : river.FoamObjectArcWakeLengthMinCells,
                    semiArcCells ? river.FoamObjectSemiArcWakeLengthMaxCells : river.FoamObjectArcWakeLengthMaxCells,
                    objectWakeLengthHash);
                candidateEvent.WakeWidthCells = Mathf.Lerp(
                    semiArcCells ? river.FoamObjectSemiArcWakeWidthMinCells : river.FoamObjectArcWakeWidthMinCells,
                    semiArcCells ? river.FoamObjectSemiArcWakeWidthMaxCells : river.FoamObjectArcWakeWidthMaxCells,
                    objectWakeWidthHash);
                candidateEvent.HeadLengthCells = semiArcCells
                    ? river.FoamObjectSemiArcHeadLengthCells
                    : river.FoamObjectArcHeadLengthCells;
                candidateEvent.HeadWidthCells = semiArcCells
                    ? river.FoamObjectSemiArcHeadWidthCells
                    : river.FoamObjectArcHeadWidthCells;
            }

            if (!TryReserveAutomaticFoamPacket(candidateEvent))
            {
                foamCompositionRejectedCount++;
                return false;
            }

            foamCompositionSequence = eventId;
            automaticFoamSourceEvents[slotIndex] = candidateEvent;

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

        private float ResolveAutomaticObjectPacketClearanceSeconds(
            AutomaticFoamSourceEvent sourceEvent)
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

            float releasedPacketLength = IsAutomaticObjectContactCycle(
                    sourceEvent.Type)
                ? Mathf.Max(0f, sourceEvent.ObjectWakeArmLengthMetres)
                : 0f;
            float clearanceDistance = releasedPacketLength +
                river.FoamObjectContactMinimumPacketGapMetres;
            return Mathf.Max(0f, clearanceDistance) /
                Mathf.Max(0.0001f, baseSpeed);
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

            int eventId = foamCompositionSequence + 1;
            AutomaticFoamSourceEventType sourceType = recipe ==
                AutomaticFreeWaterSourceRecipe.TornFragment
                    ? AutomaticFoamSourceEventType.FreeWaterTornFragment
                    : (recipe == AutomaticFreeWaterSourceRecipe.CrossLaceConnector
                        ? AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector
                        : AutomaticFoamSourceEventType.FreeWaterLaceConnector);
            float halfLength = Mathf.Max(0.025f, shapeHalfLengthMetres);
            float halfWidth = Mathf.Max(0.005f, widthMetres);

            AutomaticFoamSourceEvent candidateEvent = new AutomaticFoamSourceEvent
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

            float freeLengthHash = Hash01(sourceKey + 101.13f);
            float freeWidthHash = Hash01(sourceKey + 103.37f);
            float freeBendHash = Hash01(sourceKey + 107.71f);
            if (sourceType == AutomaticFoamSourceEventType.FreeWaterCrossLaceConnector)
            {
                candidateEvent.BodyLengthCells = Mathf.Lerp(river.FoamFreeWaterCrossLaceLengthMinCells, river.FoamFreeWaterCrossLaceLengthMaxCells, freeLengthHash);
                candidateEvent.BodyWidthCells = Mathf.Lerp(river.FoamFreeWaterCrossLaceWidthMinCells, river.FoamFreeWaterCrossLaceWidthMaxCells, freeWidthHash);
                candidateEvent.HeadLengthCells = river.FoamFreeWaterCrossLaceHeadLengthCells;
                candidateEvent.HeadWidthCells = river.FoamFreeWaterCrossLaceHeadWidthCells;
                candidateEvent.BendAmplitudeCells = Mathf.Lerp(river.FoamFreeWaterCrossLaceBendMinCells, river.FoamFreeWaterCrossLaceBendMaxCells, freeBendHash);
            }
            else if (sourceType == AutomaticFoamSourceEventType.FreeWaterTornFragment)
            {
                candidateEvent.BodyLengthCells = Mathf.Lerp(river.FoamFreeWaterBrokenFilamentLengthMinCells, river.FoamFreeWaterBrokenFilamentLengthMaxCells, freeLengthHash);
                candidateEvent.BodyWidthCells = Mathf.Lerp(river.FoamFreeWaterBrokenFilamentWidthMinCells, river.FoamFreeWaterBrokenFilamentWidthMaxCells, freeWidthHash);
                candidateEvent.HeadLengthCells = river.FoamFreeWaterBrokenFilamentHeadLengthCells;
                candidateEvent.HeadWidthCells = river.FoamFreeWaterBrokenFilamentHeadWidthCells;
                candidateEvent.BendAmplitudeCells = Mathf.Lerp(river.FoamFreeWaterBrokenFilamentBendMinCells, river.FoamFreeWaterBrokenFilamentBendMaxCells, freeBendHash);
            }
            else
            {
                candidateEvent.BodyLengthCells = Mathf.Lerp(river.FoamFreeWaterLaceLengthMinCells, river.FoamFreeWaterLaceLengthMaxCells, freeLengthHash);
                candidateEvent.BodyWidthCells = Mathf.Lerp(river.FoamFreeWaterLaceWidthMinCells, river.FoamFreeWaterLaceWidthMaxCells, freeWidthHash);
                candidateEvent.HeadLengthCells = river.FoamFreeWaterLaceHeadLengthCells;
                candidateEvent.HeadWidthCells = river.FoamFreeWaterLaceHeadWidthCells;
                candidateEvent.BendAmplitudeCells = Mathf.Lerp(river.FoamFreeWaterLaceBendMinCells, river.FoamFreeWaterLaceBendMaxCells, freeBendHash);
            }

            if (!TryReserveAutomaticFoamPacket(candidateEvent))
            {
                foamCompositionRejectedCount++;
                return false;
            }

            foamCompositionSequence = eventId;
            automaticFoamSourceEvents[slotIndex] = candidateEvent;

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
            int slotId,
            int cycleIndex,
            out float eventDuration,
            out int eventId)
        {
            eventDuration = 0f;
            eventId = 0;
            if (river == null || !river.Domain.IsValid)
            {
                return false;
            }

            int longitudinalSlotCount =
                ResolveAutomaticShoreLongitudinalSlotCount(validFieldLength);
            int wrappedSlot = PositiveModulo(
                slotId,
                Mathf.Max(1, longitudinalSlotCount * 2));
            int longitudinalIndex = wrappedSlot / 2;
            int sideIndex = wrappedSlot & 1;
            float sideSign = sideIndex == 0 ? -1f : 1f;
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float longitudinalCellSpacing =
                ResolveSourceLongitudinalSpacingMetres();
            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float domainMaximum = Mathf.Min(
                river.Domain.GlobalDistanceMaximum,
                domainMinimum + validFieldLength);
            float spacing = Mathf.Max(0.25f, profile.SlotSpacingMetres);
            float bucketMinimum = domainMinimum + longitudinalIndex * spacing;
            float bucketMaximum = Mathf.Min(
                domainMaximum,
                bucketMinimum + spacing);
            if (bucketMaximum <= bucketMinimum + 0.0001f)
            {
                return false;
            }

            float identitySeed = river.VisualSeed * 0.137f +
                wrappedSlot * 17.317f;
            float slotSeed = identitySeed + cycleIndex * 31.619f;
            AutomaticShoreSourceRecipe recipe =
                ResolveAutomaticShoreRecipe(profile.Pattern, slotSeed);
            bool isInwardWash =
                recipe == AutomaticShoreSourceRecipe.InwardWash;
            int authoredMinimumLengthCells = Mathf.Max(
                1,
                Mathf.RoundToInt(
                    isInwardWash
                        ? river.FoamInwardWashAlongLengthMinCells
                        : river.FoamShoreRibbonLengthMinCells));
            int authoredMaximumLengthCells = Mathf.Max(
                authoredMinimumLengthCells,
                Mathf.RoundToInt(
                    isInwardWash
                        ? river.FoamInwardWashAlongLengthMaxCells
                        : river.FoamShoreRibbonLengthMaxCells));

            // Candidate placement is constrained only by the authored minimum.
            // After the candidate is fixed, the event chooses uniformly from
            // every authored whole-cell length that physically fits there.
            float geometryMinimum;
            float geometryMaximum;
            if (isInwardWash)
            {
                float halfMinimumLengthMetres =
                    authoredMinimumLengthCells *
                    longitudinalCellSpacing * 0.5f;
                geometryMinimum = domainMinimum + halfMinimumLengthMetres;
                geometryMaximum = domainMaximum - halfMinimumLengthMetres;
            }
            else if (flowDirection >= 0f)
            {
                geometryMinimum = domainMinimum +
                    longitudinalCellSpacing * 0.5f;
                geometryMaximum = domainMaximum -
                    (authoredMinimumLengthCells - 0.5f) *
                    longitudinalCellSpacing;
            }
            else
            {
                geometryMinimum = domainMinimum +
                    (authoredMinimumLengthCells - 0.5f) *
                    longitudinalCellSpacing;
                geometryMaximum = domainMaximum -
                    longitudinalCellSpacing * 0.5f;
            }

            float candidateMinimum = Mathf.Max(
                bucketMinimum,
                geometryMinimum);
            float candidateMaximum = Mathf.Min(
                bucketMaximum,
                geometryMaximum);
            if (candidateMaximum < candidateMinimum - 0.0001f)
            {
                return false;
            }

            float candidate01 = Hash01(slotSeed + 2.9f);
            float globalDistance = candidateMaximum > candidateMinimum
                ? Mathf.Lerp(candidateMinimum, candidateMaximum, candidate01)
                : candidateMinimum;
            int maximumFittingLengthCells;
            if (isInwardWash)
            {
                float availableHalfLengthMetres = Mathf.Min(
                    globalDistance - domainMinimum,
                    domainMaximum - globalDistance);
                maximumFittingLengthCells = Mathf.FloorToInt(
                    2f * Mathf.Max(0f, availableHalfLengthMetres) /
                    longitudinalCellSpacing + 0.0001f);
            }
            else
            {
                float startBoundary = globalDistance -
                    flowDirection * longitudinalCellSpacing * 0.5f;
                float availableLengthMetres = flowDirection >= 0f
                    ? domainMaximum - startBoundary
                    : startBoundary - domainMinimum;
                maximumFittingLengthCells = Mathf.FloorToInt(
                    Mathf.Max(0f, availableLengthMetres) /
                    longitudinalCellSpacing + 0.0001f);
            }

            int fittingMaximumLengthCells = Mathf.Min(
                authoredMaximumLengthCells,
                maximumFittingLengthCells);
            if (fittingMaximumLengthCells < authoredMinimumLengthCells)
            {
                return false;
            }

            int effectiveAlongLengthCells = ResolveInclusiveCellCount(
                authoredMinimumLengthCells,
                fittingMaximumLengthCells,
                Hash01(slotSeed + 6.5f));
            StylizedRiverSplineSample sample =
                river.Domain.SampleAtGlobalDistance(globalDistance);
            float visibleHalfWidth = sample.GetVisibleHalfWidth(sideSign);
            if (visibleHalfWidth <= 0.05f)
            {
                return false;
            }

            return TryBeginAutomaticShoreSourceEvent(
                recipe,
                wrappedSlot,
                slotSeed,
                globalDistance,
                sideSign,
                effectiveAlongLengthCells,
                out eventDuration,
                out eventId);
        }

        private static int ResolveInclusiveCellCount(
            float authoredMinimum,
            float authoredMaximum,
            float deterministicSample)
        {
            int minimum = Mathf.Max(1, Mathf.RoundToInt(authoredMinimum));
            int maximum = Mathf.Max(
                minimum,
                Mathf.RoundToInt(authoredMaximum));
            int count = maximum - minimum + 1;
            int offset = Mathf.Min(
                count - 1,
                Mathf.FloorToInt(Mathf.Clamp01(deterministicSample) * count));
            return minimum + offset;
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
            AutomaticShoreSourceRecipe recipe,
            int shoreScheduleSlotId,
            float seed,
            float globalDistance,
            float sideSign,
            int effectiveAlongLengthCells,
            out float eventDuration,
            out int eventId)
        {
            eventDuration = 0f;
            eventId = 0;
            float flowDirection = river.FlowDirection >= 0f ? 1f : -1f;
            float longitudinalCellSpacing =
                ResolveSourceLongitudinalSpacingMetres();
            float lateralCellSpacing = ResolveSourceLateralSpacingMetres(
                globalDistance,
                sideSign);
            float widthHash = Hash01(seed + 7.1f);
            float reachHash = Hash01(seed + 7.7f);
            float bendHash = Hash01(seed + 10.7f);
            float sourceKey = river.VisualSeed * 0.317f +
                globalDistance * 13.731f +
                sideSign * 29.137f +
                seed * 0.071f +
                (recipe == AutomaticShoreSourceRecipe.InwardWash
                    ? 503f
                    : 211f);

            bool isInwardWash =
                recipe == AutomaticShoreSourceRecipe.InwardWash;
            float lengthCells = Mathf.Max(1, effectiveAlongLengthCells);
            float widthCells = isInwardWash
                ? Mathf.Lerp(
                    river.FoamInwardWashWidthMinCells,
                    river.FoamInwardWashWidthMaxCells,
                    widthHash)
                : 1f;
            float reachCells = isInwardWash
                ? Mathf.Lerp(
                    river.FoamInwardWashReachMinCells,
                    river.FoamInwardWashReachMaxCells,
                    reachHash)
                : 0f;
            float bendCells = isInwardWash
                ? Mathf.Lerp(
                    river.FoamInwardWashBendAmplitudeMinCells,
                    river.FoamInwardWashBendAmplitudeMaxCells,
                    bendHash) * (Hash01(seed + 11.3f) < 0.5f ? -1f : 1f)
                : 0f;
            float headLengthCells = isInwardWash
                ? river.FoamInwardWashHeadLengthCells
                : 1f;
            float headWidthCells = isInwardWash
                ? river.FoamInwardWashHeadWidthCells
                : 1f;
            float revealSpeedCells = isInwardWash
                ? river.FoamInwardWashRevealSpeedCellsPerSecond
                : river.FoamShoreRibbonRevealSpeedCellsPerSecond;

            widthCells = Mathf.Max(1f, widthCells);
            headLengthCells = Mathf.Max(1f, headLengthCells);
            headWidthCells = Mathf.Max(1f, headWidthCells);
            reachCells = Mathf.Max(0f, reachCells);

            float domainMinimum = river.Domain.GlobalDistanceMinimum;
            float domainMaximum = Mathf.Min(
                river.Domain.GlobalDistanceMaximum,
                domainMinimum + validFieldLength);
            float startGlobalDistance;
            float endGlobalDistance;
            float resolvedAlongCells = effectiveAlongLengthCells;
            if (isInwardWash)
            {
                float halfLengthMetres =
                    effectiveAlongLengthCells * longitudinalCellSpacing * 0.5f;
                startGlobalDistance = globalDistance -
                    flowDirection * halfLengthMetres;
                endGlobalDistance = globalDistance +
                    flowDirection * halfLengthMetres;
            }
            else
            {
                startGlobalDistance = globalDistance -
                    flowDirection * longitudinalCellSpacing * 0.5f;
                endGlobalDistance = startGlobalDistance +
                    flowDirection * effectiveAlongLengthCells *
                    longitudinalCellSpacing;
                widthCells = 1f;
                headLengthCells = 1f;
                headWidthCells = 1f;
            }

            float minimumEventDistance = Mathf.Min(
                startGlobalDistance,
                endGlobalDistance);
            float maximumEventDistance = Mathf.Max(
                startGlobalDistance,
                endGlobalDistance);
            if (minimumEventDistance < domainMinimum - 0.0001f ||
                maximumEventDistance > domainMaximum + 0.0001f)
            {
                foamCompositionRejectedCount++;
                return false;
            }

            float pathCells = isInwardWash
                ? Mathf.Sqrt(
                    resolvedAlongCells * resolvedAlongCells +
                    reachCells * reachCells)
                : resolvedAlongCells;
            float materialStepDuration =
                1f / Mathf.Max(1f, ResolveUpdateRate());
            float rawDuration =
                pathCells / Mathf.Max(0.01f, revealSpeedCells);
            float resolvedDuration = Mathf.Max(
                materialStepDuration,
                rawDuration);
            float representativeMetricSpeed = revealSpeedCells *
                Mathf.Sqrt(longitudinalCellSpacing * lateralCellSpacing);
            ResolvedAutomaticRevealTiming revealTiming =
                new ResolvedAutomaticRevealTiming(
                    pathCells * Mathf.Sqrt(
                        longitudinalCellSpacing * lateralCellSpacing),
                    representativeMetricSpeed,
                    rawDuration,
                    resolvedDuration,
                    rawDuration < materialStepDuration);

            float materialHash = Hash01(seed + 12.1f);
            float amount = isInwardWash
                ? Mathf.Lerp(
                    river.FoamInwardWashInitialPresenceMin,
                    river.FoamInwardWashInitialPresenceMax,
                    materialHash)
                : Mathf.Lerp(
                    river.FoamShoreRibbonInitialPresenceMin,
                    river.FoamShoreRibbonInitialPresenceMax,
                    materialHash);
            float remainingLife = isInwardWash
                ? Mathf.Lerp(
                    river.FoamInwardWashInitialLifeMin,
                    river.FoamInwardWashInitialLifeMax,
                    materialHash)
                : Mathf.Lerp(
                    river.FoamShoreRibbonInitialLifeMin,
                    river.FoamShoreRibbonInitialLifeMax,
                    materialHash);

            if (!BeginAutomaticFoamSourceEvent(
                    recipe,
                    shoreScheduleSlotId,
                    sideSign,
                    startGlobalDistance,
                    endGlobalDistance,
                    revealTiming,
                    headLengthCells,
                    headWidthCells,
                    0f,
                    widthCells,
                    reachCells,
                    revealSpeedCells,
                    pathCells,
                    amount,
                    remainingLife,
                    sourceKey,
                    bendCells,
                    out eventId))
            {
                return false;
            }

            eventDuration = resolvedDuration;
            return true;
        }

        private bool BeginAutomaticFoamSourceEvent(
            AutomaticShoreSourceRecipe recipe,
            int shoreScheduleSlotId,
            float sideSign,
            float startGlobalDistance,
            float endGlobalDistance,
            ResolvedAutomaticRevealTiming revealTiming,
            float headLengthCells,
            float headWidthCells,
            float shoreOffsetCells,
            float widthCells,
            float inwardReachCells,
            float revealSpeedCellsPerSecond,
            float pathLengthCells,
            float amount,
            float remainingLife,
            float sourceKey,
            float bendAmplitudeCells,
            out int createdEventId)
        {
            createdEventId = 0;
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

            int eventId = foamCompositionSequence + 1;
            AutomaticFoamSourceEventType sourceType = recipe ==
                AutomaticShoreSourceRecipe.InwardWash
                    ? AutomaticFoamSourceEventType.InwardWash
                    : AutomaticFoamSourceEventType.ShoreRibbon;

            AutomaticFoamSourceEvent candidateEvent = new AutomaticFoamSourceEvent
            {
                Active = true,
                EventId = eventId,
                Type = sourceType,
                SideSign = sideSign < 0f ? -1f : 1f,
                ShoreScheduleSlotId = shoreScheduleSlotId,
                StartGlobalDistance = startGlobalDistance,
                EndGlobalDistance = endGlobalDistance,
                Duration = revealTiming.ResolvedDurationSeconds,
                Elapsed = 0f,
                FormationSpeedMetresPerSecond = Mathf.Max(0.01f, revealSpeedCellsPerSecond),
                RevealPathDistanceMetres = Mathf.Max(1f, pathLengthCells),
                RawRevealDurationSeconds = revealTiming.RawDurationSeconds,
                RevealCadenceLimited = revealTiming.CadenceLimited,
                HeadTrailMetres = Mathf.Max(1f, headLengthCells),
                ShoreInsetMetres = Mathf.Max(0f, shoreOffsetCells),
                WidthMetres = Mathf.Max(1f, widthCells),
                ShoreRibbonThicknessCells = sourceType == AutomaticFoamSourceEventType.ShoreRibbon
                    ? Mathf.Max(1f, widthCells)
                    : 0f,
                ShoreRibbonThicknessMetres = Mathf.Max(1f, headWidthCells),
                InwardReachMetres = sourceType == AutomaticFoamSourceEventType.ShoreRibbon
                    ? 0f
                    : Mathf.Max(0f, inwardReachCells),
                FeatherMetres = Mathf.Max(1f, headWidthCells),
                SourceAmount = Mathf.Clamp01(amount),
                RemainingLife = Mathf.Clamp01(remainingLife),
                PatternSeed = sourceKey + AutomaticShoreBirthPatternSeedSalt,
                SourceFillSeed = sourceKey + AutomaticShoreBirthSourceFillSeedSalt,
                SourceFillFeatureSize = 1f,
                ShapeSeed = sourceKey + AutomaticShoreBirthShapeSeedSalt,
                BreakupScaleMetres = 0f,
                BreakupStrength = 0f,
                Curvature = bendAmplitudeCells,
                SourceFillBlend = 0f,
                BodyLengthCells = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? Mathf.Max(1f, pathLengthCells)
                        : 0f,
                BodyWidthCells = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? 1f
                        : 0f,
                HeadLengthCells = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? 1f
                        : 0f,
                HeadWidthCells = sourceType ==
                    AutomaticFoamSourceEventType.ShoreRibbon
                        ? 1f
                        : 0f
            };

            if (!TryReserveAutomaticFoamPacket(candidateEvent))
            {
                foamCompositionRejectedCount++;
                return false;
            }

            automaticFoamSourceEvents[slotIndex] = candidateEvent;
            foamCompositionSequence = eventId;
            latestFoamCompositionEventId = eventId;
            latestFoamCompositionProgress = 0f;
            activeAutomaticFoamSourceEventCount++;
            activeAutomaticShoreSourceEventCount++;
            RecordAutomaticRevealTiming(eventId, sourceType, revealTiming);
            createdEventId = eventId;
            return true;
        }

        private bool TryReserveAutomaticFoamPacket(
            AutomaticFoamSourceEvent candidate)
        {
            float now = Time.realtimeSinceStartup;
            if (!TryResolveAutomaticFoamPacketEnvelope(
                    candidate,
                    out float candidateMinimumGlobal,
                    out float candidateMaximumGlobal,
                    out float candidateMinimumLateral,
                    out float candidateMaximumLateral))
            {
                automaticPacketEnvelopeRejectedLastUpdate++;
                automaticPacketEnvelopeRejectedTotal++;
                return false;
            }

            int freeIndex = -1;
            for (int index = 0;
                 index < automaticFoamPacketReservations.Length;
                 index++)
            {
                AutomaticFoamPacketReservation reservation =
                    automaticFoamPacketReservations[index];
                if (!reservation.Active)
                {
                    if (freeIndex < 0)
                    {
                        freeIndex = index;
                    }
                    continue;
                }

                if (CanAutomaticPacketBypassReservation(
                        candidate,
                        reservation))
                {
                    continue;
                }

                bool separatedLongitudinally =
                    candidateMaximumGlobal <=
                        reservation.MinimumGlobalDistance ||
                    candidateMinimumGlobal >=
                        reservation.MaximumGlobalDistance;
                bool separatedLaterally =
                    candidateMaximumLateral <=
                        reservation.MinimumLateralMetres ||
                    candidateMinimumLateral >=
                        reservation.MaximumLateralMetres;
                if (!separatedLongitudinally && !separatedLaterally)
                {
                    automaticPacketEnvelopeRejectedLastUpdate++;
                    automaticPacketEnvelopeRejectedTotal++;
                    return false;
                }
            }

            if (freeIndex < 0)
            {
                automaticPacketEnvelopeRejectedLastUpdate++;
                automaticPacketEnvelopeRejectedTotal++;
                return false;
            }

            float clearanceSeconds =
                ResolveAutomaticPacketReservationClearanceSeconds(candidate);
            automaticFoamPacketReservations[freeIndex] =
                new AutomaticFoamPacketReservation
                {
                    Active = true,
                    EventId = candidate.EventId,
                    Type = candidate.Type,
                    ObjectSourceId = candidate.ObjectSourceId,
                    MinimumGlobalDistance = candidateMinimumGlobal,
                    MaximumGlobalDistance = candidateMaximumGlobal,
                    MinimumLateralMetres = candidateMinimumLateral,
                    MaximumLateralMetres = candidateMaximumLateral,
                    ExpiresAtRealtime = float.IsPositiveInfinity(
                            clearanceSeconds)
                        ? float.PositiveInfinity
                        : now + Mathf.Max(0f, candidate.Duration) +
                            Mathf.Max(0f, clearanceSeconds)
                };
            automaticPacketReservationActiveCount++;
            return true;
        }

        private void RefreshAutomaticFoamPacketReservations(float now)
        {
            int activeCount = 0;
            for (int index = 0;
                 index < automaticFoamPacketReservations.Length;
                 index++)
            {
                AutomaticFoamPacketReservation reservation =
                    automaticFoamPacketReservations[index];
                if (!reservation.Active)
                {
                    continue;
                }

                if (!float.IsPositiveInfinity(
                        reservation.ExpiresAtRealtime) &&
                    now + 0.0001f >= reservation.ExpiresAtRealtime)
                {
                    automaticFoamPacketReservations[index] = default;
                    continue;
                }

                activeCount++;
            }

            automaticPacketReservationActiveCount = activeCount;
        }

        private static bool CanAutomaticPacketBypassReservation(
            AutomaticFoamSourceEvent candidate,
            AutomaticFoamPacketReservation reservation)
        {
            // Contact-only reinforcement is the one intentional geometric
            // overlap. D7's added-Coverage-only merge still prevents it from
            // rejuvenating already occupied material.
            return candidate.ObjectContactReinforcementOnly &&
                IsAutomaticObjectContactCycle(candidate.Type) &&
                IsAutomaticObjectContactCycle(reservation.Type) &&
                candidate.ObjectSourceId.Equals(reservation.ObjectSourceId);
        }

        private float ResolveAutomaticPacketReservationClearanceSeconds(
            AutomaticFoamSourceEvent sourceEvent)
        {
            if (river == null)
            {
                return 0f;
            }

            float gapMetres;
            if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ShoreRibbon ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.InwardWash)
            {
                gapMetres = river.FoamShoreMinimumPacketGapMetres;
            }
            else if (IsAutomaticObjectSourceType(sourceEvent.Type))
            {
                gapMetres = river.FoamObjectContactMinimumPacketGapMetres;
            }
            else
            {
                gapMetres = river.FoamFreeWaterMinimumPacketGapMetres;
            }

            return ResolveAutomaticPacketClearanceSeconds(gapMetres);
        }

        private bool TryResolveAutomaticFoamPacketEnvelope(
            AutomaticFoamSourceEvent sourceEvent,
            out float minimumGlobalDistance,
            out float maximumGlobalDistance,
            out float minimumLateralMetres,
            out float maximumLateralMetres)
        {
            minimumGlobalDistance = Mathf.Min(
                sourceEvent.StartGlobalDistance,
                sourceEvent.EndGlobalDistance);
            maximumGlobalDistance = Mathf.Max(
                sourceEvent.StartGlobalDistance,
                sourceEvent.EndGlobalDistance);
            minimumLateralMetres = float.PositiveInfinity;
            maximumLateralMetres = float.NegativeInfinity;

            float longitudinalCellSpacing = gridDescriptor.IsCreated
                ? Mathf.Max(0.005f, gridDescriptor.ResolvedDxMetres)
                : 0.15f;
            float lateralCellSpacing = gridDescriptor.IsCreated
                ? Mathf.Max(0.005f, gridDescriptor.ResolvedDyMetres)
                : 0.15f;
            float padding = Mathf.Max(
                AutomaticFoamPacketEnvelopeMinimumPaddingMetres,
                Mathf.Max(
                    longitudinalCellSpacing,
                    lateralCellSpacing));
            bool cellExactShoreSource =
                sourceEvent.Type == AutomaticFoamSourceEventType.ShoreRibbon ||
                sourceEvent.Type == AutomaticFoamSourceEventType.InwardWash;
            float alongPadding = cellExactShoreSource
                ? padding + Mathf.Max(0f, sourceEvent.HeadTrailMetres) *
                    longitudinalCellSpacing
                : padding + Mathf.Max(0f, sourceEvent.HeadTrailMetres) +
                    Mathf.Max(0f, sourceEvent.FeatherMetres);
            minimumGlobalDistance -= alongPadding;
            maximumGlobalDistance += alongPadding;

            if (sourceEvent.Type ==
                    AutomaticFoamSourceEventType.ShoreRibbon ||
                sourceEvent.Type ==
                    AutomaticFoamSourceEventType.InwardWash)
            {
                AccumulateAutomaticShorePacketEnvelope(
                    sourceEvent,
                    sourceEvent.StartGlobalDistance,
                    padding,
                    ref minimumLateralMetres,
                    ref maximumLateralMetres);
                AccumulateAutomaticShorePacketEnvelope(
                    sourceEvent,
                    (sourceEvent.StartGlobalDistance +
                     sourceEvent.EndGlobalDistance) * 0.5f,
                    padding,
                    ref minimumLateralMetres,
                    ref maximumLateralMetres);
                AccumulateAutomaticShorePacketEnvelope(
                    sourceEvent,
                    sourceEvent.EndGlobalDistance,
                    padding,
                    ref minimumLateralMetres,
                    ref maximumLateralMetres);
            }
            else if (IsAutomaticObjectContactCycle(sourceEvent.Type))
            {
                float alongHalfExtent = Mathf.Max(
                    Mathf.Max(
                        sourceEvent.ObjectAlongHalfLengthMetres,
                        Mathf.Abs(sourceEvent.EndGlobalDistance -
                            sourceEvent.StartGlobalDistance) * 0.5f),
                    sourceEvent.ObjectWakeArmLengthMetres) +
                    sourceEvent.ObjectSourceLateralCellSpacingMetres * 2f +
                    padding;
                minimumGlobalDistance = Mathf.Min(
                    minimumGlobalDistance,
                    sourceEvent.ObjectCentreGlobalDistance -
                        alongHalfExtent);
                maximumGlobalDistance = Mathf.Max(
                    maximumGlobalDistance,
                    sourceEvent.ObjectCentreGlobalDistance +
                        alongHalfExtent);
                float lateralHalfExtent = Mathf.Max(
                    sourceEvent.LateralPaddingMetres,
                    sourceEvent.ObjectAcrossHalfWidthMetres) + padding;
                minimumLateralMetres =
                    sourceEvent.ObjectCentreAcrossMetres -
                    lateralHalfExtent;
                maximumLateralMetres =
                    sourceEvent.ObjectCentreAcrossMetres +
                    lateralHalfExtent;
            }
            else
            {
                float centreLateral =
                    sourceEvent.ObjectCentreAcrossMetres;
                float lateralHalfExtent = Mathf.Max(
                    sourceEvent.LateralPaddingMetres,
                    Mathf.Max(
                        sourceEvent.ObjectAcrossHalfWidthMetres,
                        sourceEvent.WidthMetres * 0.5f +
                            sourceEvent.FeatherMetres)) + padding;
                minimumLateralMetres = centreLateral - lateralHalfExtent;
                maximumLateralMetres = centreLateral + lateralHalfExtent;

                float centreGlobal =
                    (sourceEvent.StartGlobalDistance +
                     sourceEvent.EndGlobalDistance) * 0.5f;
                float shapeHalfLength = Mathf.Max(
                    sourceEvent.ObjectAlongHalfLengthMetres,
                    Mathf.Abs(sourceEvent.EndGlobalDistance -
                        sourceEvent.StartGlobalDistance) * 0.5f);
                minimumGlobalDistance = Mathf.Min(
                    minimumGlobalDistance,
                    centreGlobal - shapeHalfLength - padding);
                maximumGlobalDistance = Mathf.Max(
                    maximumGlobalDistance,
                    centreGlobal + shapeHalfLength + padding);
            }

            return !float.IsInfinity(minimumLateralMetres) &&
                !float.IsInfinity(maximumLateralMetres) &&
                maximumGlobalDistance > minimumGlobalDistance &&
                maximumLateralMetres > minimumLateralMetres;
        }

        private void AccumulateAutomaticShorePacketEnvelope(
            AutomaticFoamSourceEvent sourceEvent,
            float globalDistance,
            float padding,
            ref float minimumLateralMetres,
            ref float maximumLateralMetres)
        {
            float sideSign = sourceEvent.SideSign < 0f ? -1f : 1f;
            float lateralCellSpacing = ResolveSourceLateralSpacingMetres(
                globalDistance,
                sideSign);
            float shoreLateral = ResolveSourceLateralMetres(
                globalDistance,
                sideSign);
            if (sourceEvent.Type ==
                AutomaticFoamSourceEventType.ShoreRibbon)
            {
                float centreLateral = shoreLateral -
                    sideSign * sourceEvent.ShoreInsetMetres *
                    lateralCellSpacing;
                float halfWidth = Mathf.Max(
                    sourceEvent.WidthMetres,
                    sourceEvent.FeatherMetres) * 0.5f *
                    lateralCellSpacing + padding;
                minimumLateralMetres = Mathf.Min(
                    minimumLateralMetres,
                    centreLateral - halfWidth);
                maximumLateralMetres = Mathf.Max(
                    maximumLateralMetres,
                    centreLateral + halfWidth);
                return;
            }

            float nearLateral = shoreLateral -
                sideSign * sourceEvent.ShoreInsetMetres *
                lateralCellSpacing;
            float farLateral = shoreLateral -
                sideSign * (sourceEvent.ShoreInsetMetres +
                    sourceEvent.InwardReachMetres) * lateralCellSpacing;
            float halfStrokeWidth = Mathf.Max(
                sourceEvent.WidthMetres,
                sourceEvent.FeatherMetres) * 0.5f *
                lateralCellSpacing + padding;
            minimumLateralMetres = Mathf.Min(
                minimumLateralMetres,
                Mathf.Min(nearLateral, farLateral) - halfStrokeWidth);
            maximumLateralMetres = Mathf.Max(
                maximumLateralMetres,
                Mathf.Max(nearLateral, farLateral) + halfStrokeWidth);
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
                automaticFoamPacketReservations,
                0,
                automaticFoamPacketReservations.Length);
            Array.Clear(
                automaticRevealTimingByType,
                0,
                automaticRevealTimingByType.Length);
            activeAutomaticFoamSourceEventCount = 0;
            activeAutomaticShoreSourceEventCount = 0;
            automaticShorePopulationMeanHeadCount = 0f;
            automaticShorePopulationMinimumHeadCount = 0;
            automaticShorePopulationMaximumHeadCount = 0;
            automaticShorePopulationTargetHeadCount = 0;
            automaticShorePopulationActiveBankLengthMetres = 0f;
            automaticShorePopulationEpochIndex = 0;
            automaticShorePopulationNextBoundaryTime = -1f;
            automaticShorePopulationAuthoritySignature = int.MinValue;
            automaticShorePopulationTargetRefreshPending = true;
            automaticPacketReservationActiveCount = 0;
            automaticPacketEnvelopeRejectedLastUpdate = 0;
            automaticSourceEventsRasterizedLastUpdate = 0;
            automaticObjectSourceStates.Clear();
            automaticShoreSlotSchedules.Clear();
            automaticFreeWaterSlotNextStartTimes.Clear();
            automaticObjectContactLiveSourceIds.Clear();
            automaticObjectContactStaleSourceIds.Clear();
            automaticObjectContactCycleTime = 0f;
            automaticObjectPatternAuthoritySignature = int.MinValue;
            automaticObjectClearanceAuthoritySignature = int.MinValue;
            automaticObjectReinforcementAuthoritySignature = int.MinValue;
            automaticObjectContactBuildCount = 0;
            automaticObjectContactReinforcementCount = 0;
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


        private int ResolveAutomaticShoreBirthBudgetPerTick()
        {
            return AutomaticShoreSourceMaximumStartsPerUpdate;
        }

        private static float Hash01(float value)
        {
            return Mathf.Repeat(
                Mathf.Sin(value * 12.9898f + 78.233f) * 43758.5453f,
                1f);
        }
    }
}
