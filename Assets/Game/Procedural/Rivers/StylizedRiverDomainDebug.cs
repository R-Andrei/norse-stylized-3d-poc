using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProgrammaticStylized3D.Rivers
{
    /// <summary>
    /// Optional Stage-1 proof harness. It visualizes the authoritative domain and
    /// moves one marker by metres per second through oriented downstream distance.
    /// It does not participate in rendering or gameplay.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(StylizedRiver))]
    public sealed class StylizedRiverDomainDebug : MonoBehaviour
    {
        [Header("Domain Drawing")]
        [SerializeField] private bool drawCentreline = true;
        [SerializeField] private bool drawLogicalBanks = true;
        [SerializeField] private bool drawSampleFrames = true;

        [Min(0.1f)]
        [SerializeField] private float frameIntervalMetres = 2f;

        [Min(0f)]
        [SerializeField] private float gizmoHeightOffset = 0.06f;

        [Header("Constant-Speed Transport Test")]
        [SerializeField] private bool animateTransportMarker = true;

        [Min(0f)]
        [SerializeField] private float transportSpeedMetresPerSecond = 1f;

        [Min(0.01f)]
        [SerializeField] private float markerRadius = 0.16f;

        [SerializeField] private Transform optionalMarkerTransform;

        private StylizedRiver river;
        private float orientedDistance;
        private double lastUpdateTime;

        public float OrientedDistance => orientedDistance;

        private void Reset()
        {
            river = GetComponent<StylizedRiver>();
            lastUpdateTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnEnable()
        {
            river = GetComponent<StylizedRiver>();
            lastUpdateTime = Time.realtimeSinceStartupAsDouble;
        }

        private void OnValidate()
        {
            frameIntervalMetres = Mathf.Max(0.1f, frameIntervalMetres);
            gizmoHeightOffset = Mathf.Max(0f, gizmoHeightOffset);
            transportSpeedMetresPerSecond =
                Mathf.Max(0f, transportSpeedMetresPerSecond);
            markerRadius = Mathf.Max(0.01f, markerRadius);
        }

        private void Update()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            double now = Time.realtimeSinceStartupAsDouble;
            float deltaTime =
                Application.isPlaying
                    ? Time.deltaTime
                    : Mathf.Clamp((float)(now - lastUpdateTime), 0f, 0.1f);

            lastUpdateTime = now;

            RiverDomainSnapshot domain =
                river != null
                    ? river.Domain
                    : RiverDomainSnapshot.Empty;

            if (animateTransportMarker &&
                domain.IsValid &&
                deltaTime > 0f)
            {
                orientedDistance =
                    Mathf.Repeat(
                        orientedDistance +
                        transportSpeedMetresPerSecond * deltaTime,
                        domain.LocalLength);
            }

            if (optionalMarkerTransform != null && domain.IsValid)
            {
                StylizedRiverSplineSample sample =
                    domain.SampleAtOrientedDistance(orientedDistance);

                optionalMarkerTransform.position =
                    sample.SurfacePoint +
                    sample.Up * gizmoHeightOffset;

                optionalMarkerTransform.rotation =
                    Quaternion.LookRotation(
                        river.FlowDirection >= 0f
                            ? sample.Tangent
                            : -sample.Tangent,
                        sample.Up);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && animateTransportMarker)
            {
                SceneView.RepaintAll();
            }
#endif
        }

        [ContextMenu("Reset Transport Marker")]
        public void ResetTransportMarker()
        {
            orientedDistance = 0f;
        }

        private void OnDrawGizmos()
        {
            if (river == null)
            {
                river = GetComponent<StylizedRiver>();
            }

            RiverDomainSnapshot domain =
                river != null
                    ? river.Domain
                    : RiverDomainSnapshot.Empty;

            if (!domain.IsValid)
            {
                return;
            }

            if (drawCentreline)
            {
                DrawCentreline(domain);
            }

            if (drawLogicalBanks)
            {
                DrawBanks(domain);
            }

            if (drawSampleFrames)
            {
                DrawFrames(domain);
            }

            if (animateTransportMarker || optionalMarkerTransform != null)
            {
                StylizedRiverSplineSample markerSample =
                    domain.SampleAtOrientedDistance(orientedDistance);

                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(
                    markerSample.SurfacePoint +
                    markerSample.Up * gizmoHeightOffset,
                    markerRadius);
            }
        }

        private void DrawCentreline(RiverDomainSnapshot domain)
        {
            Gizmos.color = Color.cyan;

            for (int index = 0; index < domain.SampleCount - 1; index++)
            {
                Vector3 a =
                    domain.Samples[index].SurfacePoint +
                    domain.Samples[index].Up * gizmoHeightOffset;

                Vector3 b =
                    domain.Samples[index + 1].SurfacePoint +
                    domain.Samples[index + 1].Up * gizmoHeightOffset;

                Gizmos.DrawLine(a, b);
            }
        }

        private void DrawBanks(RiverDomainSnapshot domain)
        {
            Gizmos.color = Color.white;

            for (int index = 0; index < domain.SampleCount - 1; index++)
            {
                StylizedRiverSplineSample a = domain.Samples[index];
                StylizedRiverSplineSample b = domain.Samples[index + 1];

                Vector3 aHeight = a.Up * gizmoHeightOffset;
                Vector3 bHeight = b.Up * gizmoHeightOffset;

                Gizmos.DrawLine(
                    a.SurfacePoint -
                    a.Side * a.LeftHalfWidth +
                    aHeight,
                    b.SurfacePoint -
                    b.Side * b.LeftHalfWidth +
                    bHeight);

                Gizmos.DrawLine(
                    a.SurfacePoint +
                    a.Side * a.RightHalfWidth +
                    aHeight,
                    b.SurfacePoint +
                    b.Side * b.RightHalfWidth +
                    bHeight);
            }
        }

        private void DrawFrames(RiverDomainSnapshot domain)
        {
            for (float distance = 0f;
                 distance <= domain.LocalLength;
                 distance += frameIntervalMetres)
            {
                StylizedRiverSplineSample sample =
                    domain.SampleAtLocalDistance(distance);

                Vector3 origin =
                    sample.SurfacePoint +
                    sample.Up * gizmoHeightOffset;

                Gizmos.color = Color.green;
                Gizmos.DrawLine(
                    origin,
                    origin + sample.Tangent * 0.45f);

                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    origin,
                    origin + sample.Side * 0.35f);
            }
        }
    }
}
