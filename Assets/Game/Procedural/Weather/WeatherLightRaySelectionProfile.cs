using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Weather
{
    /// <summary>
    /// WEATHER LIGHTRAY SELECTION CONTRACT — APPEARANCE ELIGIBILITY ONLY.
    ///
    /// This asset decides when a visual preset may be selected and which
    /// runtime dependencies that selection requires. It does not own scene
    /// bindings, ground acquisition, cloud sampling, candidate placement, or
    /// ray storage. Activation curves are normalized 0..1 cycle curves; never
    /// reinterpret their X axis as hours or named dayparts in production code.
    /// Visual preset metadata must never be used as a substitute for the
    /// explicit dependency fields stored on each entry.
    /// </summary>
    [CreateAssetMenu(
        fileName = "WeatherLightRaySelectionProfile",
        menuName = "PS3D/Weather/LightRay Selection Profile")]
    public sealed class WeatherLightRaySelectionProfile : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField] private bool enabled = true;
            [SerializeField, HideInInspector] private string stableId;
            [SerializeField] private string displayName = "LightRay Selection";
            [SerializeField] private WeatherLightRayPreset preset;
            [SerializeField] private AnimationCurve activationCurve =
                AnimationCurve.Linear(0f, 1f, 1f, 1f);
            [SerializeField] private int priority;
            [SerializeField, Min(0f)] private float selectionWeight = 1f;
            [SerializeField, Min(0f)] private float transitionDurationSeconds = 1f;
            [SerializeField, Min(0f)] private float minimumHoldDurationSeconds = 2f;
            [SerializeField, Min(0f)] private float cooldownDurationSeconds = 1f;

            [Header("Dependencies")]
            [SerializeField] private WeatherLightRayDirectionMode directionMode =
                WeatherLightRayDirectionMode.ControllerDirectionalSource;
            [SerializeField] private WeatherLightRaySourceKind sourceKind =
                WeatherLightRaySourceKind.Sun;
            [SerializeField] private WeatherLightRaySourceAvailabilityPolicy
                sourceAvailabilityPolicy =
                    WeatherLightRaySourceAvailabilityPolicy.Require;
            [SerializeField] private Vector3 fixedWorldDirection = Vector3.down;
            [SerializeField] private WeatherLightRayCloudProjectionMode
                cloudProjectionMode =
                    WeatherLightRayCloudProjectionMode.
                        CloudControllerDirectionalSource;
            [SerializeField] private WeatherLightRayPopulationProfile
                populationProfile;

            public bool Enabled => enabled;
            public string StableId => stableId ?? string.Empty;
            public string DisplayName => string.IsNullOrWhiteSpace(displayName)
                ? (preset != null ? preset.DisplayName : "LightRay Selection")
                : displayName;
            public WeatherLightRayPreset Preset => preset;
            public AnimationCurve ActivationCurve => activationCurve;
            public int Priority => priority;
            public float SelectionWeight => Mathf.Max(0f, selectionWeight);
            public float TransitionDurationSeconds =>
                Mathf.Max(0f, transitionDurationSeconds);
            public float MinimumHoldDurationSeconds =>
                Mathf.Max(0f, minimumHoldDurationSeconds);
            public float CooldownDurationSeconds =>
                Mathf.Max(0f, cooldownDurationSeconds);
            public WeatherLightRayDirectionMode DirectionMode => directionMode;
            public WeatherLightRaySourceKind SourceKind =>
                directionMode == WeatherLightRayDirectionMode.
                    ControllerDirectionalSource
                    ? sourceKind
                    : WeatherLightRaySourceKind.Independent;
            public WeatherLightRaySourceAvailabilityPolicy
                SourceAvailabilityPolicy => sourceAvailabilityPolicy;
            public Vector3 FixedWorldDirection => fixedWorldDirection;
            public WeatherLightRayCloudProjectionMode CloudProjectionMode =>
                cloudProjectionMode;
            public WeatherLightRayPopulationProfile PopulationProfile =>
                populationProfile;

            public float EvaluateActivation(float normalizedCycle)
            {
                if (!enabled || activationCurve == null)
                {
                    return 0f;
                }

                return Mathf.Clamp01(
                    activationCurve.Evaluate(
                        Mathf.Clamp01(normalizedCycle)));
            }

            internal void Validate()
            {
                if (string.IsNullOrWhiteSpace(stableId))
                {
                    RegenerateStableId();
                }

                if (activationCurve == null || activationCurve.length == 0)
                {
                    activationCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
                }

                selectionWeight = Mathf.Max(0f, selectionWeight);
                transitionDurationSeconds = Mathf.Max(
                    0f,
                    transitionDurationSeconds);
                minimumHoldDurationSeconds = Mathf.Max(
                    0f,
                    minimumHoldDurationSeconds);
                cooldownDurationSeconds = Mathf.Max(
                    0f,
                    cooldownDurationSeconds);

                if (directionMode != WeatherLightRayDirectionMode.
                        ControllerDirectionalSource)
                {
                    sourceKind = WeatherLightRaySourceKind.Independent;
                    sourceAvailabilityPolicy =
                        WeatherLightRaySourceAvailabilityPolicy.Ignore;
                }
                else if (sourceKind == WeatherLightRaySourceKind.Independent)
                {
                    sourceKind = WeatherLightRaySourceKind.Sun;
                }

                if (directionMode ==
                        WeatherLightRayDirectionMode.FixedWorldDirection &&
                    (!IsFinite(fixedWorldDirection) ||
                        fixedWorldDirection.sqrMagnitude < 0.000001f))
                {
                    fixedWorldDirection = Vector3.down;
                }
            }

            internal void RegenerateStableId()
            {
                stableId = Guid.NewGuid().ToString("N");
            }

            private static bool IsFinite(Vector3 value)
            {
                return !float.IsNaN(value.x) &&
                    !float.IsInfinity(value.x) &&
                    !float.IsNaN(value.y) &&
                    !float.IsInfinity(value.y) &&
                    !float.IsNaN(value.z) &&
                    !float.IsInfinity(value.z);
            }
        }

        [SerializeField, Range(1f, 30f)]
        private float evaluationRateHz = 4f;
        [SerializeField, Range(0f, 1f)]
        private float challengerMargin = 0.1f;
        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        public float EvaluationRateHz => Mathf.Clamp(
            evaluationRateHz,
            1f,
            30f);
        public float ChallengerMargin => Mathf.Clamp01(challengerMargin);
        public IReadOnlyList<Entry> Entries => entries;
        public int EntryCount => entries != null ? entries.Count : 0;

        public Entry GetEntry(int index)
        {
            return entries != null && index >= 0 && index < entries.Count
                ? entries[index]
                : null;
        }

        private void OnValidate()
        {
            evaluationRateHz = Mathf.Clamp(evaluationRateHz, 1f, 30f);
            challengerMargin = Mathf.Clamp01(challengerMargin);
            if (entries == null)
            {
                entries = new List<Entry>();
                return;
            }

            // Entry IDs are authoring identities. Duplicate list elements
            // must never share one ID because future selection state/cooldown
            // persistence may key by this contract even though candidate world
            // identity deliberately excludes selection-entry identity.
            var stableIds = new HashSet<string>();
            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                entry.Validate();
                while (!stableIds.Add(entry.StableId))
                {
                    entry.RegenerateStableId();
                }
            }
        }
    }
}
