using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Lighting
{
    [Serializable]
    public sealed class TimeOfDayCheckpoint
    {
        [Tooltip("Editor-only label to make the checkpoint list readable.")]
        public string label = "Checkpoint";

        [Tooltip("Hour on a 24-hour clock. Use 0 for midnight; do not use 24.")]
        [Range(0f, 23.999f)]
        public float hour;

        [Header("Sun")]
        [ColorUsage(false, false)]
        public Color sunColor = Color.white;

        [Min(0f)]
        public float sunIntensity = 1f;

        [Header("Ambient floor")]
        [ColorUsage(false, false)]
        public Color ambientColor = Color.black;

        [Min(0f)]
        public float ambientIntensity = 0.1f;

        [Header("Procedural skybox")]
        [ColorUsage(false, false)]
        public Color skyTint = Color.gray;

        [ColorUsage(false, false)]
        public Color skyGroundColor = Color.gray;

        [Range(0f, 5f)]
        public float skyAtmosphereThickness = 1f;

        [Min(0f)]
        public float skyExposure = 1f;

        [Header("Environment reflections")]
        [Range(0f, 1f)]
        public float reflectionIntensity = 0.2f;

        [Header("Fog")]
        [ColorUsage(false, false)]
        public Color fogColor = Color.gray;

        [Tooltip("Exponential-squared fog density. Zero disables fog.")]
        [Range(0f, 0.1f)]
        public float fogDensity;
    }

    public struct TimeOfDayLightingState
    {
        public Color sunColor;
        public float sunIntensity;

        public Color ambientColor;
        public float ambientIntensity;

        public Color skyTint;
        public Color skyGroundColor;
        public float skyAtmosphereThickness;
        public float skyExposure;

        public float reflectionIntensity;

        public Color fogColor;
        public float fogDensity;

        public static TimeOfDayLightingState FromCheckpoint(TimeOfDayCheckpoint checkpoint)
        {
            return new TimeOfDayLightingState
            {
                sunColor = checkpoint.sunColor,
                sunIntensity = checkpoint.sunIntensity,
                ambientColor = checkpoint.ambientColor,
                ambientIntensity = checkpoint.ambientIntensity,
                skyTint = checkpoint.skyTint,
                skyGroundColor = checkpoint.skyGroundColor,
                skyAtmosphereThickness = checkpoint.skyAtmosphereThickness,
                skyExposure = checkpoint.skyExposure,
                reflectionIntensity = checkpoint.reflectionIntensity,
                fogColor = checkpoint.fogColor,
                fogDensity = checkpoint.fogDensity
            };
        }

        public static TimeOfDayLightingState Lerp(
            TimeOfDayLightingState from,
            TimeOfDayLightingState to,
            float t)
        {
            t = Mathf.Clamp01(t);

            return new TimeOfDayLightingState
            {
                sunColor = Color.Lerp(from.sunColor, to.sunColor, t),
                sunIntensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, t),
                ambientColor = Color.Lerp(from.ambientColor, to.ambientColor, t),
                ambientIntensity = Mathf.Lerp(from.ambientIntensity, to.ambientIntensity, t),
                skyTint = Color.Lerp(from.skyTint, to.skyTint, t),
                skyGroundColor = Color.Lerp(from.skyGroundColor, to.skyGroundColor, t),
                skyAtmosphereThickness = Mathf.Lerp(
                    from.skyAtmosphereThickness,
                    to.skyAtmosphereThickness,
                    t),
                skyExposure = Mathf.Lerp(from.skyExposure, to.skyExposure, t),
                reflectionIntensity = Mathf.Lerp(
                    from.reflectionIntensity,
                    to.reflectionIntensity,
                    t),
                fogColor = Color.Lerp(from.fogColor, to.fogColor, t),
                fogDensity = Mathf.Lerp(from.fogDensity, to.fogDensity, t)
            };
        }
    }

    [CreateAssetMenu(
        fileName = "TOD_Default",
        menuName = "Game/Lighting/Time of Day Profile",
        order = 10)]
    public sealed class TimeOfDayProfile : ScriptableObject
    {
        [Tooltip(
            "Controls how every pair of checkpoints blends. " +
            "The horizontal axis is progress from one checkpoint to the next.")]
        [SerializeField]
        private AnimationCurve segmentBlend =
            AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [SerializeField]
        private List<TimeOfDayCheckpoint> checkpoints =
            CreateDefaultCheckpoints();

        public IReadOnlyList<TimeOfDayCheckpoint> Checkpoints => checkpoints;

        public TimeOfDayLightingState Evaluate(float hour)
        {
            if (checkpoints == null || checkpoints.Count == 0)
            {
                return default;
            }

            if (checkpoints.Count == 1)
            {
                return TimeOfDayLightingState.FromCheckpoint(checkpoints[0]);
            }

            hour = Mathf.Repeat(hour, 24f);

            int nextIndex = 0;
            bool foundNext = false;

            for (int i = 0; i < checkpoints.Count; i++)
            {
                if (hour < checkpoints[i].hour)
                {
                    nextIndex = i;
                    foundNext = true;
                    break;
                }
            }

            if (!foundNext)
            {
                nextIndex = 0;
            }

            int previousIndex =
                (nextIndex - 1 + checkpoints.Count) % checkpoints.Count;

            TimeOfDayCheckpoint previous = checkpoints[previousIndex];
            TimeOfDayCheckpoint next = checkpoints[nextIndex];

            float previousHour = previous.hour;
            float nextHour = next.hour;
            float adjustedHour = hour;

            // The last segment wraps from the final checkpoint through midnight
            // and into the first checkpoint.
            if (nextIndex == 0)
            {
                nextHour += 24f;

                if (adjustedHour < previousHour)
                {
                    adjustedHour += 24f;
                }
            }

            float segmentDuration = nextHour - previousHour;

            if (segmentDuration <= 0.0001f)
            {
                return TimeOfDayLightingState.FromCheckpoint(next);
            }

            float rawT =
                Mathf.Clamp01((adjustedHour - previousHour) / segmentDuration);

            float curvedT = segmentBlend == null
                ? rawT
                : Mathf.Clamp01(segmentBlend.Evaluate(rawT));

            TimeOfDayLightingState fromState =
                TimeOfDayLightingState.FromCheckpoint(previous);
            TimeOfDayLightingState toState =
                TimeOfDayLightingState.FromCheckpoint(next);

            return TimeOfDayLightingState.Lerp(fromState, toState, curvedT);
        }

        private void OnValidate()
        {
            if (checkpoints == null)
            {
                checkpoints = new List<TimeOfDayCheckpoint>();
            }

            checkpoints.RemoveAll(checkpoint => checkpoint == null);

            foreach (TimeOfDayCheckpoint checkpoint in checkpoints)
            {
                checkpoint.hour = Mathf.Clamp(checkpoint.hour, 0f, 23.999f);
                checkpoint.sunIntensity = Mathf.Max(0f, checkpoint.sunIntensity);
                checkpoint.ambientIntensity =
                    Mathf.Max(0f, checkpoint.ambientIntensity);
                checkpoint.skyAtmosphereThickness =
                    Mathf.Clamp(checkpoint.skyAtmosphereThickness, 0f, 5f);
                checkpoint.skyExposure = Mathf.Max(0f, checkpoint.skyExposure);
                checkpoint.reflectionIntensity =
                    Mathf.Clamp01(checkpoint.reflectionIntensity);
                checkpoint.fogDensity =
                    Mathf.Clamp(checkpoint.fogDensity, 0f, 0.1f);
            }

            checkpoints.Sort(
                (left, right) => left.hour.CompareTo(right.hour));
        }

        private static List<TimeOfDayCheckpoint> CreateDefaultCheckpoints()
        {
            return new List<TimeOfDayCheckpoint>
            {
                new TimeOfDayCheckpoint
                {
                    label = "Deep Night",
                    hour = 0f,
                    sunColor = Hex("#7D89A8"),
                    sunIntensity = 0f,
                    ambientColor = Hex("#0B0E13"),
                    ambientIntensity = 0.08f,
                    skyTint = Hex("#080A15"),
                    skyGroundColor = Hex("#020203"),
                    skyAtmosphereThickness = 0.15f,
                    skyExposure = 0.08f,
                    reflectionIntensity = 0.05f,
                    fogColor = Hex("#111421"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Late Night",
                    hour = 3f,
                    sunColor = Hex("#7D89A8"),
                    sunIntensity = 0f,
                    ambientColor = Hex("#0E1018"),
                    ambientIntensity = 0.09f,
                    skyTint = Hex("#100E20"),
                    skyGroundColor = Hex("#030204"),
                    skyAtmosphereThickness = 0.20f,
                    skyExposure = 0.10f,
                    reflectionIntensity = 0.06f,
                    fogColor = Hex("#171425"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Pre-Dawn",
                    hour = 5.25f,
                    sunColor = Hex("#C97879"),
                    sunIntensity = 0f,
                    ambientColor = Hex("#171326"),
                    ambientIntensity = 0.12f,
                    skyTint = Hex("#2C1B35"),
                    skyGroundColor = Hex("#080609"),
                    skyAtmosphereThickness = 0.45f,
                    skyExposure = 0.22f,
                    reflectionIntensity = 0.08f,
                    fogColor = Hex("#2C2433"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Sunrise",
                    hour = 6.5f,
                    sunColor = Hex("#FF875C"),
                    sunIntensity = 0.45f,
                    ambientColor = Hex("#35253A"),
                    ambientIntensity = 0.25f,
                    skyTint = Hex("#6A4E78"),
                    skyGroundColor = Hex("#20120E"),
                    skyAtmosphereThickness = 1.20f,
                    skyExposure = 0.55f,
                    reflectionIntensity = 0.20f,
                    fogColor = Hex("#80636E"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Morning",
                    hour = 9f,
                    sunColor = Hex("#FFD6A0"),
                    sunIntensity = 0.90f,
                    ambientColor = Hex("#9299A3"),
                    ambientIntensity = 0.45f,
                    skyTint = Hex("#78A4B8"),
                    skyGroundColor = Hex("#5B493C"),
                    skyAtmosphereThickness = 0.75f,
                    skyExposure = 0.90f,
                    reflectionIntensity = 0.35f,
                    fogColor = Hex("#A7B0B4"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Midday",
                    hour = 12.5f,
                    sunColor = Hex("#EAF6E8"),
                    sunIntensity = 1.15f,
                    ambientColor = Hex("#B8C2C0"),
                    ambientIntensity = 0.55f,
                    skyTint = Hex("#72A9BC"),
                    skyGroundColor = Hex("#68706A"),
                    skyAtmosphereThickness = 0.65f,
                    skyExposure = 1.00f,
                    reflectionIntensity = 0.45f,
                    fogColor = Hex("#B5C1C2"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Afternoon",
                    hour = 16f,
                    sunColor = Hex("#FFD3A0"),
                    sunIntensity = 1.00f,
                    ambientColor = Hex("#9B928A"),
                    ambientIntensity = 0.45f,
                    skyTint = Hex("#799AA8"),
                    skyGroundColor = Hex("#665344"),
                    skyAtmosphereThickness = 0.75f,
                    skyExposure = 0.85f,
                    reflectionIntensity = 0.35f,
                    fogColor = Hex("#ADA4A0"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Sunset",
                    hour = 19f,
                    sunColor = Hex("#FF6047"),
                    sunIntensity = 0.60f,
                    ambientColor = Hex("#4A2E46"),
                    ambientIntensity = 0.25f,
                    skyTint = Hex("#7A355C"),
                    skyGroundColor = Hex("#271216"),
                    skyAtmosphereThickness = 1.30f,
                    skyExposure = 0.55f,
                    reflectionIntensity = 0.20f,
                    fogColor = Hex("#7A5364"),
                    fogDensity = 0f
                },
                new TimeOfDayCheckpoint
                {
                    label = "Twilight",
                    hour = 21.5f,
                    sunColor = Hex("#B45C7E"),
                    sunIntensity = 0f,
                    ambientColor = Hex("#18152B"),
                    ambientIntensity = 0.12f,
                    skyTint = Hex("#281B46"),
                    skyGroundColor = Hex("#08060B"),
                    skyAtmosphereThickness = 0.55f,
                    skyExposure = 0.20f,
                    reflectionIntensity = 0.08f,
                    fogColor = Hex("#28233A"),
                    fogDensity = 0f
                }
            };
        }

        private static Color Hex(string htmlColor)
        {
            return ColorUtility.TryParseHtmlString(htmlColor, out Color color)
                ? color
                : Color.white;
        }
    }
}