using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    /// <summary>
    /// WEATHER LIGHTRAY POPULATION CONTRACT — INSTANCE POLICY ONLY.
    ///
    /// This asset controls how automatic instances are budgeted and spatially
    /// qualified. It never selects visual appearance and never owns scene
    /// bindings. Rules that ignore clouds must not execute cloud queries.
    /// Optional cloud data treats a genuinely absent/disabled producer as clear
    /// sky, but an enabled producer with invalid or unready data is never silently
    /// treated as clear. Authored and gameplay-created rays remain outside these
    /// budgets and may never be evicted by this profile.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WeatherLightRayPopulationProfile",
        menuName = "PS3D/Weather/LightRay Population Profile")]
    public sealed class WeatherLightRayPopulationProfile : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string stableId;

        public string StableId => stableId ?? string.Empty;

        [Serializable]
        public sealed class Rule
        {
            [SerializeField] private bool enabled = true;
            [SerializeField, HideInInspector] private string stableId;
            [SerializeField] private string displayName = "Population Rule";
            [SerializeField] private int priority;
            [SerializeField, Range(0, 64)] private int desiredCount = 3;
            [SerializeField, Range(0, 64)] private int maximumCount = 6;
            [SerializeField, Min(0.5f)] private float minimumSpacingMetres = 12f;
            [SerializeField] private WeatherLightRayCloudDataRequirement
                cloudDataRequirement =
                    WeatherLightRayCloudDataRequirement.Optional;
            [SerializeField] private WeatherLightRaySpatialCloudPolicy
                spatialCloudPolicy =
                    WeatherLightRaySpatialCloudPolicy.ClearFootprint;
            [SerializeField] private AnimationCurve cloudCoverActivationCurve =
                AnimationCurve.Linear(0f, 1f, 1f, 1f);
            [SerializeField, Range(0f, 1f)]
            private float minimumClearance = 0.75f;
            [SerializeField, Range(0f, 1f)]
            private float minimumDistinctOpeningContrast = 0.2f;
            [SerializeField, Min(0f)]
            private float surroundingSampleRadiusMetres = 3f;

            public bool Enabled => enabled;
            public string StableId => stableId ?? string.Empty;
            public string DisplayName => string.IsNullOrWhiteSpace(displayName)
                ? "Population Rule"
                : displayName;
            public int Priority => priority;
            public int DesiredCount => Mathf.Clamp(desiredCount, 0, 64);
            public int MaximumCount => Mathf.Clamp(maximumCount, 0, 64);
            public float MinimumSpacingMetres => Mathf.Max(
                0.5f,
                minimumSpacingMetres);
            public WeatherLightRayCloudDataRequirement CloudDataRequirement =>
                cloudDataRequirement;
            public WeatherLightRaySpatialCloudPolicy SpatialCloudPolicy =>
                spatialCloudPolicy;
            public AnimationCurve CloudCoverActivationCurve =>
                cloudCoverActivationCurve;
            public float MinimumClearance => Mathf.Clamp01(minimumClearance);
            public float MinimumDistinctOpeningContrast =>
                Mathf.Clamp01(minimumDistinctOpeningContrast);
            public float SurroundingSampleRadiusMetres =>
                Mathf.Max(0f, surroundingSampleRadiusMetres);

            public float EvaluateCloudCoverActivation(float cloudCover)
            {
                if (!enabled)
                {
                    return 0f;
                }

                if (cloudDataRequirement ==
                    WeatherLightRayCloudDataRequirement.Ignored)
                {
                    return 1f;
                }

                if (cloudCoverActivationCurve == null)
                {
                    return 0f;
                }

                return Mathf.Clamp01(
                    cloudCoverActivationCurve.Evaluate(
                        Mathf.Clamp01(cloudCover)));
            }

            internal void Validate()
            {
                if (string.IsNullOrWhiteSpace(stableId))
                {
                    RegenerateStableId();
                }

                desiredCount = Mathf.Clamp(desiredCount, 0, 64);
                maximumCount = Mathf.Clamp(maximumCount, 0, 64);
                desiredCount = Mathf.Min(desiredCount, maximumCount);
                minimumSpacingMetres = Mathf.Max(0.5f, minimumSpacingMetres);
                minimumClearance = Mathf.Clamp01(minimumClearance);
                minimumDistinctOpeningContrast = Mathf.Clamp01(
                    minimumDistinctOpeningContrast);
                surroundingSampleRadiusMetres = Mathf.Max(
                    0f,
                    surroundingSampleRadiusMetres);
                if (cloudCoverActivationCurve == null ||
                    cloudCoverActivationCurve.length == 0)
                {
                    cloudCoverActivationCurve = AnimationCurve.Linear(
                        0f,
                        1f,
                        1f,
                        1f);
                }

                if (cloudDataRequirement ==
                    WeatherLightRayCloudDataRequirement.Ignored)
                {
                    spatialCloudPolicy =
                        WeatherLightRaySpatialCloudPolicy.AnyPosition;
                }
                else if (spatialCloudPolicy ==
                    WeatherLightRaySpatialCloudPolicy.DistinctCloudOpening)
                {
                    cloudDataRequirement =
                        WeatherLightRayCloudDataRequirement.Required;
                }
            }

            internal void RegenerateStableId()
            {
                stableId = Guid.NewGuid().ToString("N");
            }
        }

        [SerializeField]
        private List<Rule> rules = new List<Rule>();

        public IReadOnlyList<Rule> Rules => rules;
        public int RuleCount => rules != null ? rules.Count : 0;

        public Rule GetRule(int index)
        {
            return rules != null && index >= 0 && index < rules.Count
                ? rules[index]
                : null;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                stableId = Guid.NewGuid().ToString("N");
            }

            if (rules == null)
            {
                rules = new List<Rule>();
                return;
            }

            // Rule IDs participate directly in deterministic candidate
            // identity. Duplicate entries inside one profile would otherwise
            // collide and must be repaired at authoring time, never at runtime.
            var stableIds = new HashSet<string>();
            for (int index = 0; index < rules.Count; index++)
            {
                Rule rule = rules[index];
                if (rule == null)
                {
                    continue;
                }

                rule.Validate();
                while (!stableIds.Add(rule.StableId))
                {
                    rule.RegenerateStableId();
                }
            }
        }
    }
}
