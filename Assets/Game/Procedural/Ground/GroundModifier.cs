using System;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundModifierMode
    {
        Flatten = 0,
        Raise = 1,
        Lower = 2,
        None = 3
    }

    public enum GroundModifierSurfaceEffectMode
    {
        AutoFromHeight = 0,
        None = 1,
        Custom = 2
    }

    public enum GroundModifierShape
    {
        Circle,
        Box
    }

    public enum GroundModifierPriority
    {
        Low,
        Normal,
        High,
        Critical
    }


    [Flags]
    public enum GroundSurfaceFeatureExclusionFlags
    {
        None = 0,
        PaintedAccentLines = 1 << 0
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class GroundModifier : MonoBehaviour
    {
        [SerializeField]
        private GroundModifierMode mode =
            GroundModifierMode.Flatten;

        [SerializeField]
        private GroundModifierShape shape =
            GroundModifierShape.Circle;

        [SerializeField]
        private GroundModifierPriority priority =
            GroundModifierPriority.Normal;

        [Tooltip("Controls whether this modifier writes authored surface metadata independently from height. Auto From Height preserves the legacy Flatten-to-compaction behavior.")]
        [SerializeField]
        private GroundModifierSurfaceEffectMode surfaceEffectMode =
            GroundModifierSurfaceEffectMode.AutoFromHeight;


        [Tooltip("Generated surface features blocked inside this modifier shape and its Blend Distance. This can be used with Mode None and Surface Effect Mode None to create a pure placement exclusion zone.")]
        [SerializeField]
        private GroundSurfaceFeatureExclusionFlags featureExclusions =
            GroundSurfaceFeatureExclusionFlags.None;

        [Tooltip("Compaction/path mask strength written to UV2.x when Surface Effect Mode is Custom.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceCompactionStrength = 1f;

        [Tooltip("Additional damp/deposit mask strength applied to Vertex Color B when Surface Effect Mode is Custom.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceDampDepositStrength;

        [Tooltip("Authored standing-water or puddle-potential mask strength written to UV2.w when Surface Effect Mode is Custom.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float surfaceStandingWaterStrength;

        [Tooltip("Overall height influence of this modifier.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float strength = 1f;

        [Tooltip("Distance, in metres, over which the modifier blends back to untouched terrain.")]
        [Range(0f, 20f)]
        [SerializeField]
        private float blendDistance = 3f;

        [Tooltip("Full-strength radius before the blend region begins.")]
        [Range(0.25f, 40f)]
        [SerializeField]
        private float circleRadius = 4f;

        [Tooltip("Full-strength box dimensions before the blend region begins.")]
        [SerializeField]
        private Vector2 boxSize = new Vector2(8f, 8f);

        [Tooltip("Height added or removed by Raise and Lower modes.")]
        [Range(0f, 12f)]
        [SerializeField]
        private float heightAmount = 1f;

        [Tooltip("How much of the original small surface detail remains after flattening.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float preserveDetail = 0.15f;

        [Tooltip("Update the parent GeneratedGround when this modifier changes.")]
        [SerializeField]
        private bool autoRegenerateParent = true;

        public GroundModifierMode Mode => mode;
        public GroundModifierShape Shape => shape;
        public GroundModifierPriority Priority => priority;
        public GroundModifierSurfaceEffectMode SurfaceEffectMode => surfaceEffectMode;
        public GroundSurfaceFeatureExclusionFlags FeatureExclusions => featureExclusions;
        public float SurfaceCompactionStrength => surfaceCompactionStrength;
        public float SurfaceDampDepositStrength => surfaceDampDepositStrength;
        public float SurfaceStandingWaterStrength => surfaceStandingWaterStrength;
        public float Strength => strength;
        public float BlendDistance => blendDistance;
        public float CircleRadius => circleRadius;
        public Vector2 BoxSize => boxSize;
        public float HeightAmount => heightAmount;
        public float PreserveDetail => preserveDetail;
        public bool AutoRegenerateParent => autoRegenerateParent;

        public int PriorityValue => priority switch
        {
            GroundModifierPriority.Low => -10,
            GroundModifierPriority.Normal => 0,
            GroundModifierPriority.High => 10,
            GroundModifierPriority.Critical => 20,
            _ => 0
        };

        private void OnEnable()
        {
            transform.hasChanged = false;
            NotifyParent();
        }

        private void OnValidate()
        {
            circleRadius =
                Mathf.Max(0.25f, circleRadius);

            boxSize.x =
                Mathf.Max(0.5f, boxSize.x);

            boxSize.y =
                Mathf.Max(0.5f, boxSize.y);

            blendDistance =
                Mathf.Max(0f, blendDistance);

            surfaceCompactionStrength =
                Mathf.Clamp01(surfaceCompactionStrength);

            surfaceDampDepositStrength =
                Mathf.Clamp01(surfaceDampDepositStrength);

            surfaceStandingWaterStrength =
                Mathf.Clamp01(surfaceStandingWaterStrength);

            if (autoRegenerateParent)
            {
                NotifyParent();
            }
        }

        private void Update()
        {
            if (Application.isPlaying ||
                !transform.hasChanged)
            {
                return;
            }

            transform.hasChanged = false;

            if (autoRegenerateParent)
            {
                NotifyParent();
            }
        }

        public GroundModifierSnapshot CreateSnapshot(
            Transform groundTransform)
        {
            if (groundTransform == null)
            {
                throw new ArgumentNullException(
                    nameof(groundTransform));
            }

            Vector3 localCentre3 =
                groundTransform.InverseTransformPoint(
                    transform.position);

            Vector3 localRight3 =
                groundTransform.InverseTransformDirection(
                    transform.right);

            Vector3 localForward3 =
                groundTransform.InverseTransformDirection(
                    transform.forward);

            Vector2 localRight =
                new Vector2(
                    localRight3.x,
                    localRight3.z).normalized;

            Vector2 localForward =
                new Vector2(
                    localForward3.x,
                    localForward3.z).normalized;

            return new GroundModifierSnapshot(
                mode,
                shape,
                PriorityValue,
                surfaceEffectMode,
                featureExclusions,
                surfaceCompactionStrength,
                surfaceDampDepositStrength,
                surfaceStandingWaterStrength,
                strength,
                blendDistance,
                new Vector2(
                    localCentre3.x,
                    localCentre3.z),
                localCentre3.y,
                localRight,
                localForward,
                circleRadius,
                boxSize,
                heightAmount,
                preserveDetail);
        }

        [ContextMenu("Regenerate Parent Ground")]
        public void RegenerateParentGround()
        {
            NotifyParent();
        }

        private void NotifyParent()
        {
            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                ground.NotifyModifierChanged(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Quaternion yawRotation =
                Quaternion.Euler(
                    0f,
                    transform.eulerAngles.y,
                    0f);

            Matrix4x4 oldMatrix = Gizmos.matrix;
            Color oldColor = Gizmos.color;

            Gizmos.matrix =
                Matrix4x4.TRS(
                    transform.position,
                    yawRotation,
                    Vector3.one);

            Gizmos.color =
                ResolveGizmoColor();

            if (shape == GroundModifierShape.Circle)
            {
                DrawWireCircle(circleRadius);
                DrawWireCircle(
                    circleRadius + blendDistance);
            }
            else
            {
                Gizmos.DrawWireCube(
                    Vector3.zero,
                    new Vector3(
                        boxSize.x,
                        0.05f,
                        boxSize.y));

                Gizmos.DrawWireCube(
                    Vector3.zero,
                    new Vector3(
                        boxSize.x + blendDistance * 2f,
                        0.05f,
                        boxSize.y + blendDistance * 2f));
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;
        }

        private Color ResolveGizmoColor()
        {
            if ((featureExclusions &
                 GroundSurfaceFeatureExclusionFlags.PaintedAccentLines) != 0)
            {
                return new Color(1f, 0.28f, 0.78f, 0.95f);
            }

            if (mode == GroundModifierMode.None)
            {
                if (surfaceEffectMode == GroundModifierSurfaceEffectMode.Custom)
                {
                    if (surfaceStandingWaterStrength > 0.001f)
                    {
                        return new Color(0.20f, 0.75f, 1f, 0.9f);
                    }

                    if (surfaceDampDepositStrength > 0.001f)
                    {
                        return new Color(0.55f, 0.75f, 1f, 0.9f);
                    }

                    if (surfaceCompactionStrength > 0.001f)
                    {
                        return new Color(1f, 0.86f, 0.22f, 0.9f);
                    }
                }

                return new Color(0.82f, 0.82f, 0.82f, 0.9f);
            }

            return mode switch
            {
                GroundModifierMode.Flatten =>
                    new Color(0.25f, 0.85f, 1f, 0.9f),

                GroundModifierMode.Raise =>
                    new Color(0.35f, 1f, 0.35f, 0.9f),

                GroundModifierMode.Lower =>
                    new Color(1f, 0.45f, 0.25f, 0.9f),

                _ => Color.white
            };
        }

        private static void DrawWireCircle(float radius)
        {
            const int SegmentCount = 48;

            Vector3 previous =
                new Vector3(radius, 0f, 0f);

            for (int i = 1; i <= SegmentCount; i++)
            {
                float angle =
                    i /
                    (float)SegmentCount *
                    Mathf.PI *
                    2f;

                Vector3 current =
                    new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius);

                Gizmos.DrawLine(previous, current);
                previous = current;
            }
        }
    }

    public readonly struct GroundModifierSnapshot
    {
        public GroundModifierSnapshot(
            GroundModifierMode mode,
            GroundModifierShape shape,
            int priority,
            GroundModifierSurfaceEffectMode surfaceEffectMode,
            GroundSurfaceFeatureExclusionFlags featureExclusions,
            float surfaceCompactionStrength,
            float surfaceDampDepositStrength,
            float surfaceStandingWaterStrength,
            float strength,
            float blendDistance,
            Vector2 centre,
            float targetHeight,
            Vector2 right,
            Vector2 forward,
            float circleRadius,
            Vector2 boxSize,
            float heightAmount,
            float preserveDetail)
        {
            Mode = mode;
            Shape = shape;
            Priority = priority;
            SurfaceEffectMode = surfaceEffectMode;
            FeatureExclusions = featureExclusions;
            SurfaceCompactionStrength =
                Mathf.Clamp01(surfaceCompactionStrength);
            SurfaceDampDepositStrength =
                Mathf.Clamp01(surfaceDampDepositStrength);
            SurfaceStandingWaterStrength =
                Mathf.Clamp01(surfaceStandingWaterStrength);
            Strength = Mathf.Clamp01(strength);
            BlendDistance = Mathf.Max(0f, blendDistance);
            Centre = centre;
            TargetHeight = targetHeight;
            Right = right.sqrMagnitude > 0.0001f
                ? right.normalized
                : Vector2.right;
            Forward = forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : Vector2.up;
            CircleRadius = Mathf.Max(0.25f, circleRadius);
            BoxSize = new Vector2(
                Mathf.Max(0.5f, boxSize.x),
                Mathf.Max(0.5f, boxSize.y));
            HeightAmount = Mathf.Max(0f, heightAmount);
            PreserveDetail = Mathf.Clamp01(preserveDetail);
        }

        public GroundModifierMode Mode { get; }
        public GroundModifierShape Shape { get; }
        public int Priority { get; }
        public GroundModifierSurfaceEffectMode SurfaceEffectMode { get; }
        public GroundSurfaceFeatureExclusionFlags FeatureExclusions { get; }
        public float SurfaceCompactionStrength { get; }
        public float SurfaceDampDepositStrength { get; }
        public float SurfaceStandingWaterStrength { get; }
        public float Strength { get; }
        public float BlendDistance { get; }
        public Vector2 Centre { get; }
        public float TargetHeight { get; }
        public Vector2 Right { get; }
        public Vector2 Forward { get; }
        public float CircleRadius { get; }
        public Vector2 BoxSize { get; }
        public float HeightAmount { get; }
        public float PreserveDetail { get; }

        public bool Excludes(
            GroundSurfaceFeatureExclusionFlags feature)
        {
            return (FeatureExclusions & feature) != 0;
        }

        public float EvaluateWeight(Vector2 point)
        {
            float distanceOutside =
                Shape == GroundModifierShape.Circle
                    ? EvaluateCircleDistance(point)
                    : EvaluateBoxDistance(point);

            if (distanceOutside <= 0f)
            {
                return 1f;
            }

            if (BlendDistance <= 0f ||
                distanceOutside >= BlendDistance)
            {
                return 0f;
            }

            float t =
                Mathf.Clamp01(
                    distanceOutside /
                    BlendDistance);

            float smooth =
                t * t * (3f - 2f * t);

            return 1f - smooth;
        }

        private float EvaluateCircleDistance(
            Vector2 point)
        {
            return
                Vector2.Distance(point, Centre) -
                CircleRadius;
        }

        private float EvaluateBoxDistance(
            Vector2 point)
        {
            Vector2 delta = point - Centre;

            float localX =
                Mathf.Abs(
                    Vector2.Dot(delta, Right));

            float localZ =
                Mathf.Abs(
                    Vector2.Dot(delta, Forward));

            Vector2 halfSize = BoxSize * 0.5f;

            float outsideX =
                Mathf.Max(
                    0f,
                    localX - halfSize.x);

            float outsideZ =
                Mathf.Max(
                    0f,
                    localZ - halfSize.y);

            return
                Mathf.Sqrt(
                    outsideX * outsideX +
                    outsideZ * outsideZ);
        }
    }
}
