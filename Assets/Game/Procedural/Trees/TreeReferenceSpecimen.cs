using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Trees/Tree Reference Specimen")]
    public sealed class TreeReferenceSpecimen : MonoBehaviour
    {
        [SerializeField]
        private TreeFamily family;

        [SerializeField]
        [Range(1, 5)]
        private int sourceVariantIndex = 1;

        [SerializeField]
        private TreeReferenceRole role;

        [SerializeField]
        private string sourceAssetPath = string.Empty;

        [SerializeField]
        private string sourceAssetGuid = string.Empty;

        [SerializeField]
        private Vector3 auditedBoundsCenter;

        [SerializeField]
        private Vector3 auditedBoundsSize;

        [SerializeField]
        private float lowestVisibleLocalY;

        [SerializeField]
        private float visibleHeight;

        [SerializeField]
        private float canopyWidth;

        [SerializeField]
        private float appliedGroundCorrection;

        [SerializeField]
        private Vector3 comparisonRootLocalPosition;

        [SerializeField]
        [Min(0)]
        private int rendererCount;

        [SerializeField]
        [Min(0)]
        private int submeshCount;

        [SerializeField]
        [Min(0)]
        private int vertexCount;

        [SerializeField]
        [Min(0)]
        private int triangleCount;

        [SerializeField]
        [TextArea(2, 6)]
        private string materialLayout = string.Empty;

        [SerializeField]
        [TextArea(2, 6)]
        private string assignedRendering = string.Empty;

        public TreeFamily Family => family;
        public int SourceVariantIndex => sourceVariantIndex;
        public TreeReferenceRole Role => role;
        public string SourceAssetPath => sourceAssetPath;
        public string SourceAssetGuid => sourceAssetGuid;
        public Bounds AuditedBounds =>
            new Bounds(auditedBoundsCenter, auditedBoundsSize);
        public float LowestVisibleLocalY => lowestVisibleLocalY;
        public float VisibleHeight => visibleHeight;
        public float CanopyWidth => canopyWidth;
        public float AppliedGroundCorrection => appliedGroundCorrection;
        public Vector3 ComparisonRootLocalPosition =>
            comparisonRootLocalPosition;

        public Vector3 ResolveComparisonRootWorldPosition()
        {
            return transform.TransformPoint(comparisonRootLocalPosition);
        }

        public int RendererCount => rendererCount;
        public int SubmeshCount => submeshCount;
        public int VertexCount => vertexCount;
        public int TriangleCount => triangleCount;
        public string MaterialLayout => materialLayout;
        public string AssignedRendering => assignedRendering;

        public void Configure(
            TreeFamily treeFamily,
            int variantIndex,
            TreeReferenceRole referenceRole,
            string assetPath,
            string assetGuid,
            Bounds bounds,
            float lowestLocalY,
            float groundCorrection,
            Vector3 rootLocalPosition,
            int auditedRendererCount,
            int auditedSubmeshCount,
            int auditedVertexCount,
            int auditedTriangleCount,
            string auditedMaterialLayout,
            string renderingSummary)
        {
            family = treeFamily;
            sourceVariantIndex = Mathf.Clamp(variantIndex, 1, 5);
            role = referenceRole;
            sourceAssetPath = assetPath ?? string.Empty;
            sourceAssetGuid = assetGuid ?? string.Empty;
            auditedBoundsCenter = bounds.center;
            auditedBoundsSize = bounds.size;
            lowestVisibleLocalY = lowestLocalY;
            visibleHeight = Mathf.Max(0f, bounds.size.y);
            canopyWidth = Mathf.Max(bounds.size.x, bounds.size.z);
            appliedGroundCorrection = groundCorrection;
            comparisonRootLocalPosition = rootLocalPosition;
            rendererCount = Mathf.Max(0, auditedRendererCount);
            submeshCount = Mathf.Max(0, auditedSubmeshCount);
            vertexCount = Mathf.Max(0, auditedVertexCount);
            triangleCount = Mathf.Max(0, auditedTriangleCount);
            materialLayout = auditedMaterialLayout ?? string.Empty;
            assignedRendering = renderingSummary ?? string.Empty;
        }
    }
}
