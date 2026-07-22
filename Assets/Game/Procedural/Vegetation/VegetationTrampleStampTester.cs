using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    public enum VegetationTrampleStampTestFacingMode
    {
        TransformForward = 0,
        WorldPositiveZ = 1
    }

    [Serializable]
    public sealed class VegetationTrampleStampTestConfiguration
    {
        public string Name = "Circle Slam";
        public VegetationTrampleStampShape Shape =
            VegetationTrampleStampShape.Circle;
        public VegetationTrampleStampTestFacingMode FacingMode =
            VegetationTrampleStampTestFacingMode.TransformForward;
        public Vector3 LocalOriginOffset;
        [Range(-180f, 180f)] public float FacingYawOffsetDegrees;
        [Range(0.05f, 50f)] public float Radius = 2.5f;
        [Range(1f, 360f)] public float ArcDegrees = 90f;
        [Range(0.05f, 50f)] public float LineLength = 5f;
        [Range(0.05f, 50f)] public float LineWidth = 1.2f;
        public VegetationTrampleStampDisplacementMode DisplacementMode =
            VegetationTrampleStampDisplacementMode.RadialOutward;
        [Range(-180f, 180f)] public float FixedDirectionYawOffsetDegrees;
        [Range(0f, 2f)] public float BendStrength = 0.9f;
        [Range(0f, 1f)] public float FlattenStrength = 0.8f;
        public VegetationTrampleStampRecoveryMode RecoveryMode =
            VegetationTrampleStampRecoveryMode.Timed;
        [Range(0f, 300f)] public float RecoveryDelaySeconds = 6f;
        [Range(0.05f, 30f)] public float RecoveryDurationSeconds = 2f;
        [Range(0f, 0.5f)] public float EdgeIrregularity = 0.15f;
        [Range(0.1f, 50f)] public float IrregularityScale = 1.25f;
        public int Seed = 7319;
        public int Priority;

        public void ClampValues()
        {
            Name = string.IsNullOrWhiteSpace(Name) ? "Unnamed Stamp" : Name.Trim();
            if (!Enum.IsDefined(typeof(VegetationTrampleStampShape), Shape))
            {
                Shape = VegetationTrampleStampShape.Circle;
            }
            if (!Enum.IsDefined(
                    typeof(VegetationTrampleStampTestFacingMode),
                    FacingMode))
            {
                FacingMode = VegetationTrampleStampTestFacingMode.TransformForward;
            }
            if (!Enum.IsDefined(
                    typeof(VegetationTrampleStampDisplacementMode),
                    DisplacementMode))
            {
                DisplacementMode =
                    VegetationTrampleStampDisplacementMode.RadialOutward;
            }
            if (!Enum.IsDefined(
                    typeof(VegetationTrampleStampRecoveryMode),
                    RecoveryMode))
            {
                RecoveryMode = VegetationTrampleStampRecoveryMode.Timed;
            }
            FacingYawOffsetDegrees = Mathf.Clamp(
                FacingYawOffsetDegrees,
                -180f,
                180f);
            Radius = Mathf.Clamp(Radius, 0.05f, 50f);
            ArcDegrees = Shape == VegetationTrampleStampShape.Circle
                ? 360f
                : Mathf.Clamp(ArcDegrees, 1f, 360f);
            LineLength = Mathf.Clamp(LineLength, 0.05f, 50f);
            LineWidth = Mathf.Clamp(LineWidth, 0.05f, 50f);
            FixedDirectionYawOffsetDegrees = Mathf.Clamp(
                FixedDirectionYawOffsetDegrees,
                -180f,
                180f);
            BendStrength = Mathf.Clamp(BendStrength, 0f, 2f);
            FlattenStrength = Mathf.Clamp01(FlattenStrength);
            RecoveryDelaySeconds = Mathf.Clamp(
                RecoveryDelaySeconds,
                0f,
                300f);
            RecoveryDurationSeconds = Mathf.Clamp(
                RecoveryDurationSeconds,
                0.05f,
                30f);
            EdgeIrregularity = Mathf.Clamp(EdgeIrregularity, 0f, 0.5f);
            IrregularityScale = Mathf.Clamp(IrregularityScale, 0.1f, 50f);
            if (Seed == 0)
            {
                Seed = 1;
            }
        }
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Testing/Vegetation Trample Stamp Tester")]
    public sealed class VegetationTrampleStampTester : MonoBehaviour
    {
        [SerializeField]
        private List<VegetationTrampleStampTestConfiguration> configurations =
            new List<VegetationTrampleStampTestConfiguration>();

        [SerializeField, HideInInspector]
        private int selectedConfigurationIndex;

        [Header("Randomized Variant")]
        [SerializeField, Range(0f, 0.5f)]
        private float sizeVariation = 0.15f;

        [SerializeField, Range(0f, 45f)]
        private float facingJitterDegrees = 12f;

        [SerializeField, Range(0f, 0.5f)]
        private float strengthVariation = 0.10f;

        [SerializeField, Range(0f, 0.5f)]
        private float recoveryVariation = 0.15f;

        [SerializeField, Range(0f, 0.25f)]
        private float irregularityVariation = 0.08f;

        [SerializeField]
        private int randomizationSeed = 8889;

        [Header("Preview")]
        [SerializeField]
        private bool showSelectedShape = true;

        [SerializeField, Range(8, 96)]
        private int previewSegments = 40;

        [NonSerialized]
        private uint randomSequence;

        [NonSerialized]
        private string lastStampResult = "Not stamped.";

        public IReadOnlyList<VegetationTrampleStampTestConfiguration> Configurations =>
            configurations;
        public int SelectedConfigurationIndex => selectedConfigurationIndex;
        public int ConfigurationCount => configurations != null
            ? configurations.Count
            : 0;
        public string LastStampResult => lastStampResult;
        public string SelectedConfigurationName =>
            TryGetSelectedConfiguration(out VegetationTrampleStampTestConfiguration selected)
                ? selected.Name
                : "None";

        private void Reset()
        {
            configurations = CreateDefaultConfigurations();
            selectedConfigurationIndex = 0;
            randomSequence = 0u;
        }

        private void OnValidate()
        {
            if (configurations == null)
            {
                configurations = new List<VegetationTrampleStampTestConfiguration>();
            }
            for (int index = configurations.Count - 1; index >= 0; index--)
            {
                VegetationTrampleStampTestConfiguration configuration =
                    configurations[index];
                if (configuration == null)
                {
                    configurations.RemoveAt(index);
                    continue;
                }
                configuration.ClampValues();
            }
            selectedConfigurationIndex = configurations.Count > 0
                ? Mathf.Clamp(
                    selectedConfigurationIndex,
                    0,
                    configurations.Count - 1)
                : 0;
            sizeVariation = Mathf.Clamp(sizeVariation, 0f, 0.5f);
            facingJitterDegrees = Mathf.Clamp(facingJitterDegrees, 0f, 45f);
            strengthVariation = Mathf.Clamp(strengthVariation, 0f, 0.5f);
            recoveryVariation = Mathf.Clamp(recoveryVariation, 0f, 0.5f);
            irregularityVariation = Mathf.Clamp(
                irregularityVariation,
                0f,
                0.25f);
            previewSegments = Mathf.Clamp(previewSegments, 8, 96);
        }

        public void RestoreDefaultConfigurations()
        {
            configurations = CreateDefaultConfigurations();
            selectedConfigurationIndex = 0;
            randomSequence = 0u;
        }

        public void SelectConfiguration(int index)
        {
            if (ConfigurationCount == 0)
            {
                selectedConfigurationIndex = 0;
                return;
            }
            selectedConfigurationIndex = Mathf.Clamp(
                index,
                0,
                ConfigurationCount - 1);
        }

        public void SelectPreviousConfiguration()
        {
            if (ConfigurationCount == 0)
            {
                return;
            }
            selectedConfigurationIndex =
                (selectedConfigurationIndex - 1 + ConfigurationCount) %
                ConfigurationCount;
        }

        public void SelectNextConfiguration()
        {
            if (ConfigurationCount == 0)
            {
                return;
            }
            selectedConfigurationIndex =
                (selectedConfigurationIndex + 1) % ConfigurationCount;
        }

        public void SelectRandomConfiguration()
        {
            if (ConfigurationCount == 0)
            {
                return;
            }
            uint state = NextRandomState();
            selectedConfigurationIndex = (int)(state % (uint)ConfigurationCount);
        }

        public int StampSelectedConfiguration()
        {
            return StampSelectedConfiguration(false);
        }

        public int StampRandomizedVariant()
        {
            return StampSelectedConfiguration(true);
        }

        public int StampRandomConfiguration()
        {
            SelectRandomConfiguration();
            return StampSelectedConfiguration(true);
        }

        private int StampSelectedConfiguration(bool randomized)
        {
            if (!Application.isPlaying)
            {
                lastStampResult = "Stamp rejected: Play Mode is required.";
                return 0;
            }
            if (!TryGetSelectedConfiguration(
                    out VegetationTrampleStampTestConfiguration configuration))
            {
                lastStampResult = "Stamp rejected: no test configuration exists.";
                return 0;
            }

            VegetationTrampleStampRequest request = BuildRequest(
                configuration,
                randomized);
            int acceptedDomains = VegetationTrampleDomain.SubmitStamp(request);
            lastStampResult = acceptedDomains > 0
                ? $"Queued {configuration.Name} on {acceptedDomains} Ground domain(s)."
                : $"{configuration.Name} did not reach an active intersecting Ground domain.";
            return acceptedDomains;
        }

        public VegetationTrampleStampRequest BuildSelectedPreviewRequest()
        {
            return TryGetSelectedConfiguration(
                    out VegetationTrampleStampTestConfiguration configuration)
                ? BuildRequest(configuration, false)
                : default;
        }

        private VegetationTrampleStampRequest BuildRequest(
            VegetationTrampleStampTestConfiguration configuration,
            bool randomized)
        {
            configuration.ClampValues();
            uint randomState = randomized
                ? NextRandomState()
                : unchecked((uint)configuration.Seed);
            float randomizedSize = randomized
                ? 1f + SignedRandom(ref randomState) * sizeVariation
                : 1f;
            float randomizedStrength = randomized
                ? 1f + SignedRandom(ref randomState) * strengthVariation
                : 1f;
            float randomizedRecovery = randomized
                ? 1f + SignedRandom(ref randomState) * recoveryVariation
                : 1f;
            float yawJitter = randomized
                ? SignedRandom(ref randomState) * facingJitterDegrees
                : 0f;
            float irregularityOffset = randomized
                ? SignedRandom(ref randomState) * irregularityVariation
                : 0f;

            Vector3 origin = transform.TransformPoint(
                configuration.LocalOriginOffset);
            Vector2 facing = ResolveFacing(configuration.FacingMode);
            facing = RotateXZ(
                facing,
                configuration.FacingYawOffsetDegrees + yawJitter);
            Vector2 fixedDirection = RotateXZ(
                facing,
                configuration.FixedDirectionYawOffsetDegrees);
            float bendStrength = Mathf.Clamp(
                configuration.BendStrength * randomizedStrength,
                0f,
                2f);
            float flattenStrength = Mathf.Clamp01(
                configuration.FlattenStrength * randomizedStrength);
            float recoveryDelay = Mathf.Clamp(
                configuration.RecoveryDelaySeconds * randomizedRecovery,
                0f,
                300f);
            float recoveryDuration = Mathf.Clamp(
                configuration.RecoveryDurationSeconds * randomizedRecovery,
                0.05f,
                30f);
            float edgeIrregularity = Mathf.Clamp(
                configuration.EdgeIrregularity + irregularityOffset,
                0f,
                0.5f);
            uint seed = randomized
                ? NextNonZero(ref randomState)
                : unchecked((uint)configuration.Seed);
            if (seed == 0u)
            {
                seed = 1u;
            }

            switch (configuration.Shape)
            {
                case VegetationTrampleStampShape.Cone:
                    return VegetationTrampleStampRequest.CreateCone(
                        origin,
                        facing,
                        configuration.Radius * randomizedSize,
                        configuration.ArcDegrees,
                        bendStrength,
                        flattenStrength,
                        configuration.DisplacementMode,
                        fixedDirection,
                        configuration.RecoveryMode,
                        recoveryDelay,
                        recoveryDuration,
                        edgeIrregularity,
                        configuration.IrregularityScale,
                        seed,
                        configuration.Priority);

                case VegetationTrampleStampShape.Line:
                    Vector3 end = origin + new Vector3(
                        facing.x,
                        0f,
                        facing.y) * (configuration.LineLength * randomizedSize);
                    return VegetationTrampleStampRequest.CreateLine(
                        origin,
                        end,
                        configuration.LineWidth * randomizedSize,
                        bendStrength,
                        flattenStrength,
                        configuration.DisplacementMode,
                        fixedDirection,
                        configuration.RecoveryMode,
                        recoveryDelay,
                        recoveryDuration,
                        edgeIrregularity,
                        configuration.IrregularityScale,
                        seed,
                        configuration.Priority);

                default:
                    return VegetationTrampleStampRequest.CreateCircle(
                        origin,
                        configuration.Radius * randomizedSize,
                        bendStrength,
                        flattenStrength,
                        configuration.DisplacementMode,
                        fixedDirection,
                        configuration.RecoveryMode,
                        recoveryDelay,
                        recoveryDuration,
                        edgeIrregularity,
                        configuration.IrregularityScale,
                        seed,
                        configuration.Priority);
            }
        }

        private bool TryGetSelectedConfiguration(
            out VegetationTrampleStampTestConfiguration configuration)
        {
            if (configurations == null || configurations.Count == 0)
            {
                configuration = null;
                return false;
            }
            selectedConfigurationIndex = Mathf.Clamp(
                selectedConfigurationIndex,
                0,
                configurations.Count - 1);
            configuration = configurations[selectedConfigurationIndex];
            return configuration != null;
        }

        private Vector2 ResolveFacing(
            VegetationTrampleStampTestFacingMode facingMode)
        {
            if (facingMode == VegetationTrampleStampTestFacingMode.WorldPositiveZ)
            {
                return Vector2.up;
            }
            Vector3 forward = transform.forward;
            Vector2 projected = new Vector2(forward.x, forward.z);
            return projected.sqrMagnitude > 0.0000001f
                ? projected.normalized
                : Vector2.up;
        }

        private static Vector2 RotateXZ(Vector2 direction, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sine = Mathf.Sin(radians);
            float cosine = Mathf.Cos(radians);
            return new Vector2(
                direction.x * cosine + direction.y * sine,
                -direction.x * sine + direction.y * cosine).normalized;
        }

        private uint NextRandomState()
        {
            uint state = unchecked((uint)randomizationSeed) ^
                0x9e3779b9u ^ (++randomSequence * 0x85ebca6bu);
            return XorShift(ref state);
        }

        private static float SignedRandom(ref uint state)
        {
            uint value = XorShift(ref state);
            return (value & 0x00ffffffu) / 8388607.5f - 1f;
        }

        private static uint NextNonZero(ref uint state)
        {
            uint value = XorShift(ref state);
            return value == 0u ? 1u : value;
        }

        private static uint XorShift(ref uint state)
        {
            if (state == 0u)
            {
                state = 0x6d2b79f5u;
            }
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        private static List<VegetationTrampleStampTestConfiguration>
            CreateDefaultConfigurations()
        {
            return new List<VegetationTrampleStampTestConfiguration>
            {
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Circle Slam",
                    Shape = VegetationTrampleStampShape.Circle,
                    Radius = 2.5f,
                    BendStrength = 0.9f,
                    FlattenStrength = 0.8f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.RadialOutward,
                    RecoveryDelaySeconds = 6f,
                    RecoveryDurationSeconds = 2f,
                    EdgeIrregularity = 0.14f,
                    IrregularityScale = 1.2f,
                    Seed = 7319
                },
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Wide Circle Slam",
                    Shape = VegetationTrampleStampShape.Circle,
                    Radius = 4.5f,
                    BendStrength = 0.7f,
                    FlattenStrength = 0.9f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.RadialOutward,
                    RecoveryDelaySeconds = 12f,
                    RecoveryDurationSeconds = 3f,
                    EdgeIrregularity = 0.20f,
                    IrregularityScale = 1.8f,
                    Seed = 2223
                },
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Narrow Cone",
                    Shape = VegetationTrampleStampShape.Cone,
                    Radius = 5f,
                    ArcDegrees = 55f,
                    BendStrength = 1f,
                    FlattenStrength = 0.65f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.FixedWorldDirection,
                    RecoveryDelaySeconds = 8f,
                    RecoveryDurationSeconds = 2.5f,
                    EdgeIrregularity = 0.12f,
                    IrregularityScale = 1.1f,
                    Seed = 5727
                },
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Wide Cone",
                    Shape = VegetationTrampleStampShape.Cone,
                    Radius = 4f,
                    ArcDegrees = 120f,
                    BendStrength = 0.85f,
                    FlattenStrength = 0.75f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.RadialOutward,
                    RecoveryDelaySeconds = 8f,
                    RecoveryDurationSeconds = 2.5f,
                    EdgeIrregularity = 0.16f,
                    IrregularityScale = 1.4f,
                    Seed = 8889
                },
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Short Line",
                    Shape = VegetationTrampleStampShape.Line,
                    LineLength = 4f,
                    LineWidth = 1.4f,
                    BendStrength = 0.9f,
                    FlattenStrength = 0.75f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.AwayFromCentreline,
                    RecoveryDelaySeconds = 7f,
                    RecoveryDurationSeconds = 2f,
                    EdgeIrregularity = 0.10f,
                    IrregularityScale = 0.9f,
                    Seed = 1357
                },
                new VegetationTrampleStampTestConfiguration
                {
                    Name = "Long Line",
                    Shape = VegetationTrampleStampShape.Line,
                    LineLength = 8f,
                    LineWidth = 1f,
                    BendStrength = 1f,
                    FlattenStrength = 0.7f,
                    DisplacementMode =
                        VegetationTrampleStampDisplacementMode.FixedWorldDirection,
                    RecoveryDelaySeconds = 10f,
                    RecoveryDurationSeconds = 3f,
                    EdgeIrregularity = 0.12f,
                    IrregularityScale = 1.25f,
                    Seed = 2468
                }
            };
        }

        private void OnDrawGizmosSelected()
        {
            if (!showSelectedShape ||
                !TryGetSelectedConfiguration(
                    out VegetationTrampleStampTestConfiguration configuration))
            {
                return;
            }

            VegetationTrampleStampRequest request = BuildRequest(
                configuration,
                false);
            Color previousColor = Gizmos.color;
            Gizmos.color = new Color(1f, 0.25f, 0.75f, 0.9f);
            if (request.Shape == VegetationTrampleStampShape.Line)
            {
                DrawLineCapsulePreview(
                    request.Origin,
                    request.End,
                    request.LineWidth * 0.5f);
            }
            else
            {
                DrawSectorPreview(
                    request.Origin,
                    request.FacingDirectionXZ,
                    request.Radius,
                    request.Shape == VegetationTrampleStampShape.Circle
                        ? 360f
                        : request.ArcDegrees);
            }
            Gizmos.color = previousColor;
        }

        private void DrawSectorPreview(
            Vector3 origin,
            Vector2 facing,
            float radius,
            float arcDegrees)
        {
            int segmentCount = Mathf.Max(
                8,
                Mathf.CeilToInt(previewSegments * arcDegrees / 360f));
            float halfArc = arcDegrees * 0.5f;
            Vector3 previous = origin + ToWorldXZ(
                RotateXZ(facing, -halfArc)) * radius;
            if (arcDegrees < 359.9f)
            {
                Gizmos.DrawLine(origin, previous);
            }
            for (int index = 1; index <= segmentCount; index++)
            {
                float fraction = index / (float)segmentCount;
                Vector2 direction = RotateXZ(
                    facing,
                    Mathf.Lerp(-halfArc, halfArc, fraction));
                Vector3 current = origin + ToWorldXZ(direction) * radius;
                Gizmos.DrawLine(previous, current);
                previous = current;
            }
            if (arcDegrees < 359.9f)
            {
                Gizmos.DrawLine(origin, previous);
            }
        }

        private void DrawLineCapsulePreview(
            Vector3 start,
            Vector3 end,
            float radius)
        {
            Vector2 segment = new Vector2(end.x - start.x, end.z - start.z);
            Vector2 direction = segment.sqrMagnitude > 0.0000001f
                ? segment.normalized
                : Vector2.up;
            Vector2 side = new Vector2(-direction.y, direction.x);
            Vector3 sideWorld = ToWorldXZ(side) * radius;
            Gizmos.DrawLine(start + sideWorld, end + sideWorld);
            Gizmos.DrawLine(start - sideWorld, end - sideWorld);
            DrawSectorPreview(start, -direction, radius, 180f);
            DrawSectorPreview(end, direction, radius, 180f);
        }

        private static Vector3 ToWorldXZ(Vector2 value)
        {
            return new Vector3(value.x, 0f, value.y);
        }
    }
}
