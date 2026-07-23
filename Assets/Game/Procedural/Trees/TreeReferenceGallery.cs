using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Trees/Tree Reference Gallery")]
    public sealed class TreeReferenceGallery : MonoBehaviour
    {
        public const string SourceRootPath = "Assets/References/Trees";
        public const int RequiredModelCount = 20;
        public const int RequiredTextureCount = 12;

        [Header("Reference Surface")]
        [SerializeField]
        [Tooltip("Explicit Ground used for later gallery height sampling. The gallery remains a separate sibling/root object and does not inherit Ground ownership from hierarchy.")]
        private GeneratedGround referenceGround;

        [Header("Gallery Layout Foundation")]
        [SerializeField]
        [Min(0.01f)]
        private float sourceScale = 1f;

        [SerializeField]
        private bool alignToGround = true;

        [SerializeField]
        [Min(1f)]
        private float familyRowSpacing = 18f;

        [SerializeField]
        [Min(1f)]
        private float pairColumnSpacing = 12f;

        [SerializeField]
        [Min(0.1f)]
        private float comparisonPairOffset = 4f;

        [Header("Reference Rendering Foundation")]
        [SerializeField]
        private bool windEnabled = true;

        [SerializeField]
        private bool foliageShadowCasting;

        [SerializeField]
        [HideInInspector]
        private bool sourceFolderAvailable;

        [SerializeField]
        [HideInInspector]
        private bool lastSourceAuditPassed;

        [SerializeField]
        [HideInInspector]
        private int sourceAuditRevision;

        [SerializeField]
        [HideInInspector]
        private int lastAuditedModelCount;

        [SerializeField]
        [HideInInspector]
        private int lastAuditedTextureCount;

        [SerializeField]
        [HideInInspector]
        private string lastSourceAuditTimestamp = string.Empty;

        [SerializeField]
        [HideInInspector]
        [TextArea(8, 40)]
        private string lastSourceAuditReport = string.Empty;

        public GeneratedGround ReferenceGround => referenceGround;
        public float SourceScale => sourceScale;
        public bool AlignToGround => alignToGround;
        public float FamilyRowSpacing => familyRowSpacing;
        public float PairColumnSpacing => pairColumnSpacing;
        public float ComparisonPairOffset => comparisonPairOffset;
        public bool WindEnabled => windEnabled;
        public bool FoliageShadowCasting => foliageShadowCasting;
        public bool SourceFolderAvailable => sourceFolderAvailable;
        public bool LastSourceAuditPassed => lastSourceAuditPassed;
        public int SourceAuditRevision => sourceAuditRevision;
        public int LastAuditedModelCount => lastAuditedModelCount;
        public int LastAuditedTextureCount => lastAuditedTextureCount;
        public string LastSourceAuditTimestamp => lastSourceAuditTimestamp;
        public string LastSourceAuditReport => lastSourceAuditReport;
        public bool HasSourceAuditReport =>
            !string.IsNullOrEmpty(lastSourceAuditReport);

        private void OnValidate()
        {
            sourceScale = Mathf.Max(0.01f, sourceScale);
            familyRowSpacing = Mathf.Max(1f, familyRowSpacing);
            pairColumnSpacing = Mathf.Max(1f, pairColumnSpacing);
            comparisonPairOffset = Mathf.Max(0.1f, comparisonPairOffset);
        }

        public void SetReferenceGround(GeneratedGround ground)
        {
            referenceGround = ground;
        }

        public void RecordSourceAudit(
            bool passed,
            bool folderAvailable,
            int modelCount,
            int textureCount,
            string timestamp,
            string report)
        {
            lastSourceAuditPassed = passed;
            sourceFolderAvailable = folderAvailable;
            lastAuditedModelCount = Mathf.Max(0, modelCount);
            lastAuditedTextureCount = Mathf.Max(0, textureCount);
            lastSourceAuditTimestamp = timestamp ?? string.Empty;
            lastSourceAuditReport = report ?? string.Empty;
            sourceAuditRevision++;
        }
    }
}
