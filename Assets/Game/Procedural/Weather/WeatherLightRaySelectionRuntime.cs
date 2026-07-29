using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    internal readonly struct WeatherLightRayResolvedSelectionDependency
    {
        internal readonly WeatherLightRaySourceKind SourceKind;
        internal readonly Vector3 RayDirectionWorld;
        internal readonly WeatherLightRaySourceGatePolicy SourceGatePolicy;
        internal readonly Light CloudProjectionLight;
        internal readonly float AvailabilityWeight;
        internal readonly ulong Signature;
        internal readonly bool Valid;
        internal readonly string FailureReason;

        internal WeatherLightRayResolvedSelectionDependency(
            WeatherLightRaySourceKind sourceKind,
            Vector3 rayDirectionWorld,
            WeatherLightRaySourceGatePolicy sourceGatePolicy,
            Light cloudProjectionLight,
            float availabilityWeight,
            ulong signature,
            bool valid,
            string failureReason)
        {
            SourceKind = sourceKind;
            RayDirectionWorld = rayDirectionWorld;
            SourceGatePolicy = sourceGatePolicy;
            CloudProjectionLight = cloudProjectionLight;
            AvailabilityWeight = Mathf.Clamp01(availabilityWeight);
            Signature = signature;
            Valid = valid;
            FailureReason = failureReason ?? string.Empty;
        }
    }

    /// <summary>
    /// WEATHER LIGHTRAY SELECTION RUNTIME CONTRACT.
    ///
    /// This runtime evaluates normalized-cycle selection entries only at the
    /// profile cadence. It may switch visual presets through the Controller,
    /// but it never creates rays or reads visual-preset source metadata.
    /// Dependency changes are exposed through a stable signature so the
    /// Controller can preserve compatible populations and retire incompatible
    /// ones. Do not add periodic random rerolls or per-frame allocations here.
    /// </summary>
    internal sealed class WeatherLightRaySelectionRuntime
    {
        private WeatherLightRaySelectionProfile currentProfile;
        private double[] cooldownUntilByEntry;
        private int selectedEntryIndex = -1;
        private double selectedAt;
        private double holdUntil;
        private double nextEvaluationTime;
        private float normalizedCycle;
        private float effectiveWeight;
        private ulong dependencySignature;
        private WeatherLightRayResolvedSelectionDependency dependency;
        private string suspensionReason = "Selection Profile mode is inactive.";

        internal int SelectedEntryIndex => selectedEntryIndex;
        internal float NormalizedCycle => normalizedCycle;
        internal float EffectiveWeight => effectiveWeight;
        internal ulong DependencySignature => dependencySignature;
        internal WeatherLightRayResolvedSelectionDependency Dependency =>
            dependency;
        internal string SuspensionReason => suspensionReason;
        internal WeatherLightRaySelectionProfile.Entry SelectedEntry =>
            currentProfile != null
                ? currentProfile.GetEntry(selectedEntryIndex)
                : null;

        internal bool Tick(
            WeatherLightRayController controller,
            WeatherLightRaySelectionProfile profile,
            float cycle01,
            double now)
        {
            normalizedCycle = Mathf.Clamp01(cycle01);
            if (profile == null)
            {
                ClearSelection("No LightRay Selection Profile is assigned.");
                return false;
            }

            EnsureProfile(profile);
            double interval = 1.0 / Math.Max(1.0, profile.EvaluationRateHz);
            if (now < nextEvaluationTime)
            {
                return false;
            }

            nextEvaluationTime = now + interval;
            int bestIndex = -1;
            int bestPriority = int.MinValue;
            float bestWeight = 0f;
            WeatherLightRayResolvedSelectionDependency bestDependency =
                default;

            for (int index = 0; index < profile.EntryCount; index++)
            {
                WeatherLightRaySelectionProfile.Entry entry =
                    profile.GetEntry(index);
                if (entry == null || !entry.Enabled || entry.Preset == null ||
                    now < cooldownUntilByEntry[index])
                {
                    continue;
                }

                float temporalWeight = entry.EvaluateActivation(
                    normalizedCycle);
                if (temporalWeight <= 0f ||
                    !controller.TryResolveSelectionDependency(
                        entry,
                        out WeatherLightRayResolvedSelectionDependency
                            resolvedDependency))
                {
                    continue;
                }

                float candidateWeight = temporalWeight *
                    entry.SelectionWeight *
                    resolvedDependency.AvailabilityWeight;
                if (candidateWeight <= 0f)
                {
                    continue;
                }

                if (entry.Priority > bestPriority ||
                    (entry.Priority == bestPriority &&
                        candidateWeight > bestWeight))
                {
                    bestIndex = index;
                    bestPriority = entry.Priority;
                    bestWeight = candidateWeight;
                    bestDependency = resolvedDependency;
                }
            }

            WeatherLightRaySelectionProfile.Entry currentEntry =
                profile.GetEntry(selectedEntryIndex);
            float currentWeight = 0f;
            WeatherLightRayResolvedSelectionDependency currentDependency =
                default;
            bool currentEligible = false;
            if (currentEntry != null && currentEntry.Enabled &&
                currentEntry.Preset != null)
            {
                float temporalWeight = currentEntry.EvaluateActivation(
                    normalizedCycle);
                currentEligible = temporalWeight > 0f &&
                    controller.TryResolveSelectionDependency(
                        currentEntry,
                        out currentDependency);
                if (currentEligible)
                {
                    currentWeight = temporalWeight *
                        currentEntry.SelectionWeight *
                        currentDependency.AvailabilityWeight;
                    currentEligible = currentWeight > 0f;
                }
            }

            if (currentEligible)
            {
                bool holdActive = now < holdUntil;
                bool challengerWins = bestIndex >= 0 &&
                    bestIndex != selectedEntryIndex &&
                    (profile.GetEntry(bestIndex).Priority >
                        currentEntry.Priority ||
                     (profile.GetEntry(bestIndex).Priority ==
                        currentEntry.Priority &&
                      bestWeight > currentWeight +
                        profile.ChallengerMargin));
                if (holdActive || !challengerWins)
                {
                    effectiveWeight = currentWeight;
                    dependency = currentDependency;
                    dependencySignature = currentDependency.Signature;
                    suspensionReason = string.Empty;
                    return false;
                }
            }

            if (bestIndex < 0)
            {
                if (selectedEntryIndex >= 0 && currentEntry != null)
                {
                    cooldownUntilByEntry[selectedEntryIndex] = now +
                        currentEntry.CooldownDurationSeconds;
                }

                ClearSelection(
                    "No LightRay selection entry is currently eligible.");
                return true;
            }

            if (selectedEntryIndex >= 0 && currentEntry != null &&
                selectedEntryIndex != bestIndex)
            {
                cooldownUntilByEntry[selectedEntryIndex] = now +
                    currentEntry.CooldownDurationSeconds;
            }

            WeatherLightRaySelectionProfile.Entry selected =
                profile.GetEntry(bestIndex);
            if (!controller.TrySetActivePreset(
                    selected.Preset,
                    selected.TransitionDurationSeconds,
                    out string error))
            {
                suspensionReason = error;
                return false;
            }

            selectedEntryIndex = bestIndex;
            selectedAt = now;
            holdUntil = selectedAt + selected.MinimumHoldDurationSeconds;
            effectiveWeight = bestWeight;
            dependency = bestDependency;
            dependencySignature = bestDependency.Signature;
            suspensionReason = string.Empty;
            return true;
        }

        internal void Shutdown()
        {
            currentProfile = null;
            cooldownUntilByEntry = null;
            selectedEntryIndex = -1;
            selectedAt = 0.0;
            holdUntil = 0.0;
            nextEvaluationTime = 0.0;
            normalizedCycle = 0f;
            effectiveWeight = 0f;
            dependencySignature = 0UL;
            dependency = default;
            suspensionReason = "Selection Profile mode is inactive.";
        }

        private void EnsureProfile(WeatherLightRaySelectionProfile profile)
        {
            if (currentProfile == profile && cooldownUntilByEntry != null &&
                cooldownUntilByEntry.Length == profile.EntryCount)
            {
                return;
            }

            currentProfile = profile;
            cooldownUntilByEntry = new double[profile.EntryCount];
            selectedEntryIndex = -1;
            selectedAt = 0.0;
            holdUntil = 0.0;
            nextEvaluationTime = 0.0;
            effectiveWeight = 0f;
            dependencySignature = 0UL;
            dependency = default;
            suspensionReason = string.Empty;
        }

        private void ClearSelection(string reason)
        {
            selectedEntryIndex = -1;
            selectedAt = 0.0;
            holdUntil = 0.0;
            effectiveWeight = 0f;
            dependencySignature = 0UL;
            dependency = default;
            suspensionReason = reason ?? string.Empty;
        }
    }
}
