using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public enum RiverSurfaceState
    {
        RunningDeep,
        RunningShallow,
        Frozen
    }

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(GroundModifier))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public sealed class GeneratedRiver : MonoBehaviour
    {
        [SerializeField]
        private GroundModifier riverBedModifier;

        [SerializeField]
        private RiverSurfaceState surfaceState =
            RiverSurfaceState.RunningDeep;

        [SerializeField]
        private RiverSplineResolution meshResolution =
            RiverSplineResolution.Medium;

        [Tooltip("Pulls the visible surface inward from each river-bank edge.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float widthInset = 0.15f;

        [Tooltip("Raises the visible surface slightly above the spline water level.")]
        [Range(-0.25f, 0.5f)]
        [SerializeField]
        private float surfaceOffset = 0.04f;

        [Tooltip("World-space metres represented by one vertical UV repeat.")]
        [Range(0.25f, 20f)]
        [SerializeField]
        private float flowTileLength = 2f;

        [Tooltip("Generate a MeshCollider for running water. Frozen rivers always receive one.")]
        [SerializeField]
        private bool generateCollider;

        [Tooltip("Regenerate when this component changes in the Inspector.")]
        [SerializeField]
        private bool regenerateOnValidate = true;

        private MeshFilter meshFilter;
        private MeshCollider meshCollider;
        private Mesh generatedMesh;

        public GroundModifier RiverBedModifier =>
            ResolveRiverBedModifier();

        public RiverSurfaceState SurfaceState =>
            surfaceState;

        public bool IsConfigured
        {
            get
            {
                GroundModifier modifier =
                    ResolveRiverBedModifier();

                return
                    modifier != null &&
                    modifier.IsRiverBed &&
                    modifier.SplineContainer != null &&
                    modifier.SplineContainer.Splines.Count > 0;
            }
        }

        private void OnEnable()
        {
            CacheComponents();
            ResolveRiverBedModifier();
            Regenerate();
        }

        private void OnValidate()
        {
            widthInset =
                Mathf.Max(0f, widthInset);

            flowTileLength =
                Mathf.Max(0.25f, flowTileLength);

            ResolveRiverBedModifier();

            if (regenerateOnValidate)
            {
                Regenerate();
            }
        }

        [ContextMenu("Regenerate River Surface")]
        public void Regenerate()
        {
            CacheComponents();

            GroundModifier modifier =
                ResolveRiverBedModifier();

            if (modifier == null ||
                !modifier.IsRiverBed)
            {
                ClearGeneratedAssignments();
                return;
            }

            SplineContainer container =
                modifier.SplineContainer;

            if (container == null ||
                container.Splines.Count == 0)
            {
                ClearGeneratedAssignments();
                return;
            }

            EnsureGeneratedMesh();

            MeshData meshData =
                GenerateRibbon(
                    container,
                    modifier.RiverWidth);

            string meshName =
                $"GeneratedRiver_{surfaceState}_{meshResolution}";

            MeshBuilder.ApplyToMesh(
                meshData,
                generatedMesh,
                meshName);

            meshFilter.sharedMesh =
                generatedMesh;

            bool colliderRequired =
                surfaceState ==
                RiverSurfaceState.Frozen ||
                generateCollider;

            meshCollider.enabled =
                colliderRequired;

            meshCollider.sharedMesh =
                null;

            if (colliderRequired)
            {
                meshCollider.sharedMesh =
                    generatedMesh;

                meshCollider.convex =
                    false;
            }
        }

        public bool UsesSpline(
            Spline spline)
        {
            GroundModifier modifier =
                ResolveRiverBedModifier();

            return
                modifier != null &&
                modifier.UsesSpline(spline);
        }

        private MeshData GenerateRibbon(
            SplineContainer container,
            float riverWidth)
        {
            MeshData meshData =
                new MeshData();

            float length =
                Mathf.Max(
                    0.01f,
                    container.CalculateLength());

            float spacing =
                GroundModifier.ResolveSplineSpacing(
                    meshResolution);

            int sampleCount =
                Mathf.Max(
                    2,
                    Mathf.CeilToInt(
                        length / spacing) + 1);

            float halfWidth =
                Mathf.Max(
                    0.05f,
                    riverWidth * 0.5f -
                    widthInset);

            float cumulativeDistance =
                0f;

            Vector3 previousCentre =
                Vector3.zero;

            Vector3 previousSide =
                Vector3.right;

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

                Vector3 worldCentre =
                    new Vector3(
                        positionValue.x,
                        positionValue.y,
                        positionValue.z);

                Vector3 worldTangent =
                    new Vector3(
                        tangentValue.x,
                        tangentValue.y,
                        tangentValue.z);

                worldTangent.y = 0f;

                Vector3 worldSide;

                if (worldTangent.sqrMagnitude <
                    0.0001f)
                {
                    worldSide =
                        previousSide;
                }
                else
                {
                    worldTangent.Normalize();

                    worldSide =
                        Vector3.Cross(
                            Vector3.up,
                            worldTangent).normalized;
                }

                previousSide =
                    worldSide;

                if (i > 0)
                {
                    cumulativeDistance +=
                        Vector3.Distance(
                            previousCentre,
                            worldCentre);
                }

                previousCentre =
                    worldCentre;

                Vector3 worldLeft =
                    worldCentre -
                    worldSide * halfWidth +
                    Vector3.up * surfaceOffset;

                Vector3 worldRight =
                    worldCentre +
                    worldSide * halfWidth +
                    Vector3.up * surfaceOffset;

                Vector3 localLeft =
                    transform.InverseTransformPoint(
                        worldLeft);

                Vector3 localRight =
                    transform.InverseTransformPoint(
                        worldRight);

                float uvV =
                    cumulativeDistance /
                    Mathf.Max(
                        0.25f,
                        flowTileLength);

                meshData.AddVertex(
                    localLeft,
                    new Vector2(0f, uvV),
                    new Color(
                        0.5f,
                        0.5f,
                        0.5f,
                        1f));

                meshData.AddVertex(
                    localRight,
                    new Vector2(1f, uvV),
                    new Color(
                        0.5f,
                        0.5f,
                        0.5f,
                        1f));
            }

            for (int i = 0;
                 i < sampleCount - 1;
                 i++)
            {
                int leftA =
                    i * 2;

                int rightA =
                    leftA + 1;

                int leftB =
                    leftA + 2;

                int rightB =
                    leftA + 3;

                meshData.AddTriangle(
                    leftA,
                    leftB,
                    rightA);

                meshData.AddTriangle(
                    rightA,
                    leftB,
                    rightB);
            }

            return meshData;
        }

        private GroundModifier ResolveRiverBedModifier()
        {
            if (riverBedModifier == null)
            {
                riverBedModifier =
                    GetComponent<GroundModifier>();
            }

            return riverBedModifier;
        }

        private void CacheComponents()
        {
            if (meshFilter == null)
            {
                meshFilter =
                    GetComponent<MeshFilter>();
            }

            if (meshCollider == null)
            {
                meshCollider =
                    GetComponent<MeshCollider>();
            }
        }

        private void EnsureGeneratedMesh()
        {
            if (generatedMesh != null)
            {
                return;
            }

            generatedMesh =
                new Mesh
                {
                    name =
                        "GeneratedRiver_Temporary",

                    hideFlags =
                        HideFlags.DontSave
                };
        }

        private void ClearGeneratedAssignments()
        {
            if (meshFilter != null &&
                meshFilter.sharedMesh ==
                generatedMesh)
            {
                meshFilter.sharedMesh =
                    null;
            }

            if (meshCollider != null)
            {
                if (meshCollider.sharedMesh ==
                    generatedMesh)
                {
                    meshCollider.sharedMesh =
                        null;
                }

                meshCollider.enabled =
                    false;
            }
        }

        private void OnDestroy()
        {
            ClearGeneratedAssignments();

            if (generatedMesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(generatedMesh);
            }
            else
            {
                DestroyImmediate(
                    generatedMesh);
            }

            generatedMesh = null;
        }
    }
}
