using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum GroundModifierMode
    {
        Flatten,
        Raise,
        Lower,
        RiverBed
    }

    public enum GroundModifierShape
    {
        Circle,
        Box,
        Spline
    }

    public enum GroundModifierPriority
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum RiverSplineResolution
    {
        Low,
        Medium,
        High,
        VeryHigh
    }

    public enum RiverBankStyle
    {
        Gentle,
        Natural,
        Steep
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

        [Tooltip("Overall influence of this modifier.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float strength = 1f;

        [Tooltip("Distance, in metres, over which a circle or box modifier blends back to untouched terrain.")]
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

        [Header("River Bed")]

        [Tooltip("Spline whose Y position represents the water-surface level.")]
        [SerializeField]
        private SplineContainer splineContainer;

        [SerializeField]
        private RiverSplineResolution riverSplineResolution =
            RiverSplineResolution.Medium;

        [Tooltip("Full visible channel width, in metres.")]
        [Range(0.5f, 20f)]
        [SerializeField]
        private float riverWidth = 4f;

        [Tooltip("Horizontal bank blend distance on each side of the channel.")]
        [Range(0.25f, 20f)]
        [SerializeField]
        private float riverBankWidth = 3f;

        [Tooltip("Bed depth beneath the spline's water level.")]
        [Range(0.1f, 8f)]
        [SerializeField]
        private float riverDepth = 1.2f;

        [Tooltip("Zero creates a rounded cross-section. One creates a flat bed.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float riverBedFlatness = 0.75f;

        [SerializeField]
        private RiverBankStyle riverBankStyle =
            RiverBankStyle.Natural;

        [Tooltip("Update the parent GeneratedGround when this modifier changes.")]
        [SerializeField]
        private bool autoRegenerateParent = true;

        public GroundModifierMode Mode => mode;
        public GroundModifierShape Shape => shape;
        public GroundModifierPriority Priority => priority;
        public float Strength => strength;
        public float BlendDistance => blendDistance;
        public float CircleRadius => circleRadius;
        public Vector2 BoxSize => boxSize;
        public float HeightAmount => heightAmount;
        public float PreserveDetail => preserveDetail;
        public bool AutoRegenerateParent => autoRegenerateParent;
        public SplineContainer SplineContainer => ResolveSplineContainer();
        public RiverSplineResolution RiverResolution => riverSplineResolution;
        public float RiverWidth => riverWidth;
        public float RiverBankWidth => riverBankWidth;
        public float RiverDepth => riverDepth;
        public float RiverBedFlatness => riverBedFlatness;
        public RiverBankStyle RiverBankStyle => riverBankStyle;
        public bool IsRiverBed => mode == GroundModifierMode.RiverBed;

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
            ResolveRiverState();
            transform.hasChanged = false;
            NotifyDependants();
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

            riverWidth =
                Mathf.Max(0.5f, riverWidth);

            riverBankWidth =
                Mathf.Max(0.25f, riverBankWidth);

            riverDepth =
                Mathf.Max(0.1f, riverDepth);

            ResolveRiverState();

            if (autoRegenerateParent)
            {
                NotifyDependants();
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
                NotifyDependants();
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

            Vector3[] localSplinePoints =
                BuildLocalSplinePoints(
                    groundTransform);

            return new GroundModifierSnapshot(
                mode,
                shape,
                PriorityValue,
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
                preserveDetail,
                localSplinePoints,
                riverWidth,
                riverBankWidth,
                riverDepth,
                riverBedFlatness,
                riverBankStyle);
        }

        public bool UsesSpline(Spline spline)
        {
            SplineContainer container =
                ResolveSplineContainer();

            if (container == null ||
                spline == null)
            {
                return false;
            }

            for (int i = 0;
                 i < container.Splines.Count;
                 i++)
            {
                if (ReferenceEquals(
                        container.Splines[i],
                        spline))
                {
                    return true;
                }
            }

            return false;
        }

        public static float ResolveSplineSpacing(
            RiverSplineResolution resolution)
        {
            return resolution switch
            {
                RiverSplineResolution.Low => 2f,
                RiverSplineResolution.Medium => 1f,
                RiverSplineResolution.High => 0.5f,
                RiverSplineResolution.VeryHigh => 0.25f,
                _ => 1f
            };
        }

        [ContextMenu("Regenerate Parent Ground")]
        public void RegenerateParentGround()
        {
            NotifyDependants();
        }

        private void ResolveRiverState()
        {
            if (mode == GroundModifierMode.RiverBed)
            {
                shape = GroundModifierShape.Spline;

                if (splineContainer == null)
                {
                    splineContainer =
                        GetComponent<SplineContainer>();
                }
            }
            else if (shape == GroundModifierShape.Spline)
            {
                shape = GroundModifierShape.Circle;
            }
        }

        private SplineContainer ResolveSplineContainer()
        {
            if (splineContainer != null)
            {
                return splineContainer;
            }

            return GetComponent<SplineContainer>();
        }

        private Vector3[] BuildLocalSplinePoints(
            Transform groundTransform)
        {
            if (mode != GroundModifierMode.RiverBed)
            {
                return Array.Empty<Vector3>();
            }

            SplineContainer container =
                ResolveSplineContainer();

            if (container == null ||
                container.Splines.Count == 0)
            {
                return Array.Empty<Vector3>();
            }

            float length =
                Mathf.Max(
                    0.01f,
                    container.CalculateLength());

            float spacing =
                ResolveSplineSpacing(
                    riverSplineResolution);

            int sampleCount =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(
                        length / spacing) + 1);

            Vector3[] points =
                new Vector3[sampleCount];

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                float t =
                    i /
                    (float)(sampleCount - 1);

                float3 evaluated =
                    container.EvaluatePosition(t);

                Vector3 worldPosition =
                    new Vector3(
                        evaluated.x,
                        evaluated.y,
                        evaluated.z);

                points[i] =
                    groundTransform.InverseTransformPoint(
                        worldPosition);
            }

            return points;
        }

        private void NotifyDependants()
        {
            GeneratedGround ground =
                GetComponentInParent<GeneratedGround>();

            if (ground != null)
            {
                ground.NotifyModifierChanged(this);
            }

            GeneratedRiver river =
                GetComponent<GeneratedRiver>();

            if (river != null)
            {
                river.Regenerate();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (mode == GroundModifierMode.RiverBed)
            {
                DrawRiverGizmos();
                return;
            }

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
                mode switch
                {
                    GroundModifierMode.Flatten =>
                        new Color(
                            0.25f,
                            0.85f,
                            1f,
                            0.9f),

                    GroundModifierMode.Raise =>
                        new Color(
                            0.35f,
                            1f,
                            0.35f,
                            0.9f),

                    GroundModifierMode.Lower =>
                        new Color(
                            1f,
                            0.45f,
                            0.25f,
                            0.9f),

                    _ => Color.white
                };

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
                        boxSize.x +
                        blendDistance * 2f,
                        0.05f,
                        boxSize.y +
                        blendDistance * 2f));
            }

            Gizmos.matrix = oldMatrix;
            Gizmos.color = oldColor;
        }

        private void DrawRiverGizmos()
        {
            SplineContainer container =
                ResolveSplineContainer();

            if (container == null ||
                container.Splines.Count == 0)
            {
                return;
            }

            Color oldColor = Gizmos.color;

            float length =
                Mathf.Max(
                    0.01f,
                    container.CalculateLength());

            int sampleCount =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(length) + 1);

            float waterHalfWidth =
                riverWidth * 0.5f;

            float outerHalfWidth =
                waterHalfWidth +
                riverBankWidth;

            Vector3 previousCentre =
                Vector3.zero;

            Vector3 previousLeft =
                Vector3.zero;

            Vector3 previousRight =
                Vector3.zero;

            Vector3 previousOuterLeft =
                Vector3.zero;

            Vector3 previousOuterRight =
                Vector3.zero;

            for (int i = 0;
                 i < sampleCount;
                 i++)
            {
                float t =
                    i /
                    (float)(sampleCount - 1);

                float3 positionValue =
                    container.EvaluatePosition(t);

                float3 tangentValue =
                    container.EvaluateTangent(t);

                Vector3 centre =
                    new Vector3(
                        positionValue.x,
                        positionValue.y,
                        positionValue.z);

                Vector3 tangent =
                    new Vector3(
                        tangentValue.x,
                        tangentValue.y,
                        tangentValue.z);

                tangent.y = 0f;

                if (tangent.sqrMagnitude < 0.0001f)
                {
                    tangent = Vector3.forward;
                }

                tangent.Normalize();

                Vector3 side =
                    Vector3.Cross(
                        Vector3.up,
                        tangent).normalized;

                Vector3 left =
                    centre - side * waterHalfWidth;

                Vector3 right =
                    centre + side * waterHalfWidth;

                Vector3 outerLeft =
                    centre - side * outerHalfWidth;

                Vector3 outerRight =
                    centre + side * outerHalfWidth;

                if (i > 0)
                {
                    Gizmos.color =
                        new Color(
                            0.2f,
                            0.75f,
                            1f,
                            0.95f);

                    Gizmos.DrawLine(
                        previousCentre,
                        centre);

                    Gizmos.DrawLine(
                        previousLeft,
                        left);

                    Gizmos.DrawLine(
                        previousRight,
                        right);

                    Gizmos.color =
                        new Color(
                            0.2f,
                            0.75f,
                            1f,
                            0.4f);

                    Gizmos.DrawLine(
                        previousOuterLeft,
                        outerLeft);

                    Gizmos.DrawLine(
                        previousOuterRight,
                        outerRight);
                }

                previousCentre = centre;
                previousLeft = left;
                previousRight = right;
                previousOuterLeft = outerLeft;
                previousOuterRight = outerRight;
            }

            Gizmos.color = oldColor;
        }

        private static void DrawWireCircle(
            float radius)
        {
            const int SegmentCount = 48;

            Vector3 previous =
                new Vector3(
                    radius,
                    0f,
                    0f);

            for (int i = 1;
                 i <= SegmentCount;
                 i++)
            {
                float angle =
                    i /
                    (float)SegmentCount *
                    Mathf.PI *
                    2f;

                Vector3 current =
                    new Vector3(
                        Mathf.Cos(angle) *
                        radius,
                        0f,
                        Mathf.Sin(angle) *
                        radius);

                Gizmos.DrawLine(
                    previous,
                    current);

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
            float strength,
            float blendDistance,
            Vector2 centre,
            float targetHeight,
            Vector2 right,
            Vector2 forward,
            float circleRadius,
            Vector2 boxSize,
            float heightAmount,
            float preserveDetail,
            Vector3[] splinePoints,
            float riverWidth,
            float riverBankWidth,
            float riverDepth,
            float riverBedFlatness,
            RiverBankStyle riverBankStyle)
        {
            Mode = mode;
            Shape = shape;
            Priority = priority;
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
            CircleRadius = Mathf.Max(
                0.25f,
                circleRadius);
            BoxSize = new Vector2(
                Mathf.Max(
                    0.5f,
                    boxSize.x),
                Mathf.Max(
                    0.5f,
                    boxSize.y));
            HeightAmount = Mathf.Max(
                0f,
                heightAmount);
            PreserveDetail = Mathf.Clamp01(
                preserveDetail);
            SplinePoints =
                splinePoints ??
                Array.Empty<Vector3>();
            RiverWidth = Mathf.Max(
                0.5f,
                riverWidth);
            RiverBankWidth = Mathf.Max(
                0.25f,
                riverBankWidth);
            RiverDepth = Mathf.Max(
                0.1f,
                riverDepth);
            RiverBedFlatness =
                Mathf.Clamp01(
                    riverBedFlatness);
            RiverBankStyle = riverBankStyle;
        }

        public GroundModifierMode Mode { get; }
        public GroundModifierShape Shape { get; }
        public int Priority { get; }
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
        public Vector3[] SplinePoints { get; }
        public float RiverWidth { get; }
        public float RiverBankWidth { get; }
        public float RiverDepth { get; }
        public float RiverBedFlatness { get; }
        public RiverBankStyle RiverBankStyle { get; }

        public bool HasValidRiver =>
            Mode == GroundModifierMode.RiverBed &&
            SplinePoints != null &&
            SplinePoints.Length >= 2;

        public float EvaluateWeight(
            Vector2 point)
        {
            float distanceOutside =
                Shape ==
                GroundModifierShape.Circle
                    ? EvaluateCircleDistance(
                        point)
                    : EvaluateBoxDistance(
                        point);

            if (distanceOutside <= 0f)
            {
                return 1f;
            }

            if (BlendDistance <= 0f ||
                distanceOutside >=
                BlendDistance)
            {
                return 0f;
            }

            float t =
                Mathf.Clamp01(
                    distanceOutside /
                    BlendDistance);

            float smooth =
                t *
                t *
                (3f - 2f * t);

            return 1f - smooth;
        }

        public bool TryEvaluateRiver(
            Vector2 point,
            out float distance,
            out float waterHeight)
        {
            distance =
                float.PositiveInfinity;

            waterHeight = 0f;

            if (!HasValidRiver)
            {
                return false;
            }

            for (int i = 0;
                 i < SplinePoints.Length - 1;
                 i++)
            {
                Vector3 pointA3 =
                    SplinePoints[i];

                Vector3 pointB3 =
                    SplinePoints[i + 1];

                Vector2 pointA =
                    new Vector2(
                        pointA3.x,
                        pointA3.z);

                Vector2 pointB =
                    new Vector2(
                        pointB3.x,
                        pointB3.z);

                Vector2 segment =
                    pointB - pointA;

                float segmentLengthSqr =
                    segment.sqrMagnitude;

                float segmentT =
                    segmentLengthSqr > 0.000001f
                        ? Mathf.Clamp01(
                            Vector2.Dot(
                                point - pointA,
                                segment) /
                            segmentLengthSqr)
                        : 0f;

                Vector2 nearest =
                    pointA +
                    segment * segmentT;

                float candidateDistance =
                    Vector2.Distance(
                        point,
                        nearest);

                if (candidateDistance >=
                    distance)
                {
                    continue;
                }

                distance =
                    candidateDistance;

                waterHeight =
                    Mathf.Lerp(
                        pointA3.y,
                        pointB3.y,
                        segmentT);
            }

            return
                !float.IsPositiveInfinity(
                    distance);
        }

        public float EvaluateRiverTargetHeight(
            float distance,
            float waterHeight)
        {
            float halfWidth =
                RiverWidth * 0.5f;

            float normalizedDistance =
                halfWidth > 0.0001f
                    ? Mathf.Clamp01(
                        distance /
                        halfWidth)
                    : 0f;

            float roundedDepthFactor =
                Mathf.Lerp(
                    1f,
                    0.55f,
                    normalizedDistance *
                    normalizedDistance);

            float depthFactor =
                Mathf.Lerp(
                    roundedDepthFactor,
                    1f,
                    RiverBedFlatness);

            return
                waterHeight -
                RiverDepth * depthFactor;
        }

        public float EvaluateRiverInfluence(
            float distance)
        {
            float halfWidth =
                RiverWidth * 0.5f;

            if (distance <= halfWidth)
            {
                return Strength;
            }

            float bankDistance =
                distance - halfWidth;

            if (bankDistance >=
                RiverBankWidth)
            {
                return 0f;
            }

            float t =
                Mathf.Clamp01(
                    bankDistance /
                    RiverBankWidth);

            float smooth =
                t *
                t *
                (3f - 2f * t);

            float baseWeight =
                1f - smooth;

            float shapedWeight =
                RiverBankStyle switch
                {
                    RiverBankStyle.Gentle =>
                        Mathf.Pow(
                            baseWeight,
                            1.5f),

                    RiverBankStyle.Natural =>
                        baseWeight,

                    RiverBankStyle.Steep =>
                        Mathf.Pow(
                            baseWeight,
                            0.55f),

                    _ => baseWeight
                };

            return
                shapedWeight *
                Strength;
        }

        private float EvaluateCircleDistance(
            Vector2 point)
        {
            return
                Vector2.Distance(
                    point,
                    Centre) -
                CircleRadius;
        }

        private float EvaluateBoxDistance(
            Vector2 point)
        {
            Vector2 delta =
                point - Centre;

            float localX =
                Mathf.Abs(
                    Vector2.Dot(
                        delta,
                        Right));

            float localZ =
                Mathf.Abs(
                    Vector2.Dot(
                        delta,
                        Forward));

            Vector2 halfSize =
                BoxSize * 0.5f;

            float outsideX =
                Mathf.Max(
                    0f,
                    localX -
                    halfSize.x);

            float outsideZ =
                Mathf.Max(
                    0f,
                    localZ -
                    halfSize.y);

            return
                Mathf.Sqrt(
                    outsideX *
                    outsideX +
                    outsideZ *
                    outsideZ);
        }
    }
}
