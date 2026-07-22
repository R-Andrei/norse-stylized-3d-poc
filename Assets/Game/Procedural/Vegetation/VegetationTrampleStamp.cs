using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    public enum VegetationTrampleStampShape
    {
        Circle = 0,
        Cone = 1,
        Line = 2
    }

    public enum VegetationTrampleStampDisplacementMode
    {
        RadialOutward = 0,
        FixedWorldDirection = 1,
        AwayFromCentreline = 2,
        FlattenOnly = 3
    }

    public enum VegetationTrampleStampRecoveryMode
    {
        Timed = 0,
        SessionPersistent = 1
    }

    [Serializable]
    public struct VegetationTrampleStampRequest
    {
        public const float MinimumRadius = 0.05f;
        public const float MaximumRadius = 50f;
        public const float MinimumLineWidth = 0.05f;
        public const float MaximumLineWidth = 50f;
        public const float MinimumArcDegrees = 1f;
        public const float MaximumArcDegrees = 360f;
        public const float MaximumBendStrength = 2f;
        public const float MaximumRecoveryDelaySeconds = 300f;
        public const float MinimumRecoveryDurationSeconds = 0.05f;
        public const float MaximumRecoveryDurationSeconds = 30f;
        public const float MaximumEdgeIrregularity = 0.5f;
        public const float MinimumIrregularityScale = 0.1f;
        public const float MaximumIrregularityScale = 50f;

        public VegetationTrampleStampShape Shape;
        public Vector3 Origin;
        public Vector3 End;
        public Vector2 FacingDirectionXZ;
        public float Radius;
        public float ArcDegrees;
        public float LineWidth;
        public VegetationTrampleStampDisplacementMode DisplacementMode;
        public Vector2 FixedDisplacementDirectionXZ;
        public float BendStrength;
        public float FlattenStrength;
        public VegetationTrampleStampRecoveryMode RecoveryMode;
        public float RecoveryDelaySeconds;
        public float RecoveryDurationSeconds;
        public float EdgeIrregularity;
        public float IrregularityScale;
        public uint Seed;
        public int Priority;

        public static VegetationTrampleStampRequest CreateCircle(
            Vector3 centre,
            float radius,
            float bendStrength,
            float flattenStrength,
            VegetationTrampleStampDisplacementMode displacementMode,
            Vector2 fixedDisplacementDirectionXZ,
            VegetationTrampleStampRecoveryMode recoveryMode,
            float recoveryDelaySeconds,
            float recoveryDurationSeconds,
            float edgeIrregularity = 0f,
            float irregularityScale = 1f,
            uint seed = 1u,
            int priority = 0)
        {
            return new VegetationTrampleStampRequest
            {
                Shape = VegetationTrampleStampShape.Circle,
                Origin = centre,
                End = centre,
                FacingDirectionXZ = Vector2.up,
                Radius = radius,
                ArcDegrees = MaximumArcDegrees,
                LineWidth = radius * 2f,
                DisplacementMode = displacementMode,
                FixedDisplacementDirectionXZ = fixedDisplacementDirectionXZ,
                BendStrength = bendStrength,
                FlattenStrength = flattenStrength,
                RecoveryMode = recoveryMode,
                RecoveryDelaySeconds = recoveryDelaySeconds,
                RecoveryDurationSeconds = recoveryDurationSeconds,
                EdgeIrregularity = edgeIrregularity,
                IrregularityScale = irregularityScale,
                Seed = seed,
                Priority = priority
            };
        }

        public static VegetationTrampleStampRequest CreateCone(
            Vector3 origin,
            Vector2 facingDirectionXZ,
            float radius,
            float arcDegrees,
            float bendStrength,
            float flattenStrength,
            VegetationTrampleStampDisplacementMode displacementMode,
            Vector2 fixedDisplacementDirectionXZ,
            VegetationTrampleStampRecoveryMode recoveryMode,
            float recoveryDelaySeconds,
            float recoveryDurationSeconds,
            float edgeIrregularity = 0f,
            float irregularityScale = 1f,
            uint seed = 1u,
            int priority = 0)
        {
            return new VegetationTrampleStampRequest
            {
                Shape = VegetationTrampleStampShape.Cone,
                Origin = origin,
                End = origin,
                FacingDirectionXZ = facingDirectionXZ,
                Radius = radius,
                ArcDegrees = arcDegrees,
                LineWidth = radius * 2f,
                DisplacementMode = displacementMode,
                FixedDisplacementDirectionXZ = fixedDisplacementDirectionXZ,
                BendStrength = bendStrength,
                FlattenStrength = flattenStrength,
                RecoveryMode = recoveryMode,
                RecoveryDelaySeconds = recoveryDelaySeconds,
                RecoveryDurationSeconds = recoveryDurationSeconds,
                EdgeIrregularity = edgeIrregularity,
                IrregularityScale = irregularityScale,
                Seed = seed,
                Priority = priority
            };
        }

        public static VegetationTrampleStampRequest CreateLine(
            Vector3 start,
            Vector3 end,
            float width,
            float bendStrength,
            float flattenStrength,
            VegetationTrampleStampDisplacementMode displacementMode,
            Vector2 fixedDisplacementDirectionXZ,
            VegetationTrampleStampRecoveryMode recoveryMode,
            float recoveryDelaySeconds,
            float recoveryDurationSeconds,
            float edgeIrregularity = 0f,
            float irregularityScale = 1f,
            uint seed = 1u,
            int priority = 0)
        {
            Vector2 segment = new Vector2(end.x - start.x, end.z - start.z);
            return new VegetationTrampleStampRequest
            {
                Shape = VegetationTrampleStampShape.Line,
                Origin = start,
                End = end,
                FacingDirectionXZ = NormalizeOrFallback(segment, Vector2.up),
                Radius = width * 0.5f,
                ArcDegrees = MaximumArcDegrees,
                LineWidth = width,
                DisplacementMode = displacementMode,
                FixedDisplacementDirectionXZ = fixedDisplacementDirectionXZ,
                BendStrength = bendStrength,
                FlattenStrength = flattenStrength,
                RecoveryMode = recoveryMode,
                RecoveryDelaySeconds = recoveryDelaySeconds,
                RecoveryDurationSeconds = recoveryDurationSeconds,
                EdgeIrregularity = edgeIrregularity,
                IrregularityScale = irregularityScale,
                Seed = seed,
                Priority = priority
            };
        }

        public bool TryGetValidated(
            out VegetationTrampleStampRequest validated,
            out string error)
        {
            validated = this;
            error = string.Empty;

            if (!Enum.IsDefined(typeof(VegetationTrampleStampShape), Shape))
            {
                error = "Vegetation trample stamp shape is invalid.";
                return false;
            }
            if (!Enum.IsDefined(
                    typeof(VegetationTrampleStampDisplacementMode),
                    DisplacementMode))
            {
                error = "Vegetation trample displacement mode is invalid.";
                return false;
            }
            if (!Enum.IsDefined(
                    typeof(VegetationTrampleStampRecoveryMode),
                    RecoveryMode))
            {
                error = "Vegetation trample recovery mode is invalid.";
                return false;
            }
            if (!IsFinite(Origin) || !IsFinite(End) ||
                !IsFinite(FacingDirectionXZ) ||
                !IsFinite(FixedDisplacementDirectionXZ) ||
                !IsFinite(Radius) || !IsFinite(ArcDegrees) ||
                !IsFinite(LineWidth) || !IsFinite(BendStrength) ||
                !IsFinite(FlattenStrength) ||
                !IsFinite(RecoveryDelaySeconds) ||
                !IsFinite(RecoveryDurationSeconds) ||
                !IsFinite(EdgeIrregularity) ||
                !IsFinite(IrregularityScale))
            {
                error = "Vegetation trample stamp contains a non-finite value.";
                return false;
            }

            validated.LineWidth = Mathf.Clamp(
                Shape == VegetationTrampleStampShape.Line
                    ? LineWidth
                    : Mathf.Max(MinimumLineWidth, Radius * 2f),
                MinimumLineWidth,
                MaximumLineWidth);
            validated.Radius = Shape == VegetationTrampleStampShape.Line
                ? validated.LineWidth * 0.5f
                : Mathf.Clamp(Radius, MinimumRadius, MaximumRadius);
            validated.ArcDegrees = Shape == VegetationTrampleStampShape.Circle
                ? MaximumArcDegrees
                : Mathf.Clamp(
                    ArcDegrees,
                    MinimumArcDegrees,
                    MaximumArcDegrees);
            validated.FacingDirectionXZ = NormalizeOrFallback(
                Shape == VegetationTrampleStampShape.Line
                    ? new Vector2(End.x - Origin.x, End.z - Origin.z)
                    : FacingDirectionXZ,
                Vector2.up);
            validated.FixedDisplacementDirectionXZ = NormalizeOrFallback(
                FixedDisplacementDirectionXZ,
                validated.FacingDirectionXZ);
            validated.BendStrength = Mathf.Clamp(
                BendStrength,
                0f,
                MaximumBendStrength);
            validated.FlattenStrength = Mathf.Clamp01(FlattenStrength);
            validated.RecoveryDelaySeconds = Mathf.Clamp(
                RecoveryDelaySeconds,
                0f,
                MaximumRecoveryDelaySeconds);
            validated.RecoveryDurationSeconds = Mathf.Clamp(
                RecoveryDurationSeconds,
                MinimumRecoveryDurationSeconds,
                MaximumRecoveryDurationSeconds);
            validated.EdgeIrregularity = Mathf.Clamp(
                EdgeIrregularity,
                0f,
                MaximumEdgeIrregularity);
            validated.IrregularityScale = Mathf.Clamp(
                IrregularityScale,
                MinimumIrregularityScale,
                MaximumIrregularityScale);
            if (validated.Seed == 0u)
            {
                validated.Seed = 1u;
            }

            return true;
        }

        private static Vector2 NormalizeOrFallback(
            Vector2 value,
            Vector2 fallback)
        {
            return value.sqrMagnitude > 0.0000001f
                ? value.normalized
                : fallback.normalized;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }
    }
}
