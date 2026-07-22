using ProgrammaticStylized3D.Geometry.Ground;
using UnityEngine;

namespace ProgrammaticStylized3D.Vegetation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("PS3D/Vegetation/Vegetation Layer")]
    public sealed class VegetationLayer : VegetationRendererBase
    {
        [Header("Layer Coverage")]
        [SerializeField, HideInInspector]
        private VegetationCoverageField coverage = new VegetationCoverageField();

        [SerializeField, HideInInspector]
        private bool coveragePaintMode;

        [SerializeField, HideInInspector]
        private float coverageBrushRadius = 3f;

        [SerializeField, HideInInspector]
        private float coverageBrushStrength = 0.5f;

        [SerializeField, HideInInspector]
        private bool coverageEraseMode;

        [SerializeField, HideInInspector]
        private bool showCoverageOverlay = true;

        public VegetationCoverageField Coverage => coverage;
        public bool CoverageInitialized =>
            coverage != null && coverage.Initialized;
        public int CoverageResolution =>
            coverage != null
                ? coverage.Resolution
                : VegetationCoverageField.DefaultResolution;
        public int CoverageRevision =>
            coverage != null ? coverage.Revision : 0;
        public int CoverageByteCount =>
            coverage != null ? coverage.ByteCount : 0;
        public bool CoverageStorageValid =>
            coverage == null || coverage.StorageValid;
        public float AverageCoverage =>
            coverage != null
                ? coverage.CalculateAverageCoverage()
                : 0f;
        public bool CoveragePaintMode => coveragePaintMode;
        public float CoverageBrushRadius => Mathf.Max(0.05f, coverageBrushRadius);
        public float CoverageBrushStrength => Mathf.Clamp01(coverageBrushStrength);
        public bool CoverageEraseMode => coverageEraseMode;
        public bool ShowCoverageOverlay => showCoverageOverlay;
        protected override int AuthoredCoverageRevision =>
            coverage != null ? coverage.Revision : 0;

        protected override bool TrySampleAuthoredCoverage(
            Vector3 worldPosition,
            out float authoredCoverage)
        {
            authoredCoverage = 0f;
            return coverage != null &&
                coverage.TrySample(
                    coverageGround,
                    worldPosition,
                    out authoredCoverage);
        }

        private void Reset()
        {
            SynchronizeSurfaceGroundFromHierarchy();
            coverage ??= new VegetationCoverageField();
            coverage.Initialize(false);
        }

        protected override void OnEnable()
        {
            SynchronizeSurfaceGroundFromHierarchy();
            base.OnEnable();
        }

        protected override void OnValidate()
        {
            SynchronizeSurfaceGroundFromHierarchy();
            coverage ??= new VegetationCoverageField();
            coverageBrushRadius = Mathf.Max(0.05f, coverageBrushRadius);
            coverageBrushStrength = Mathf.Clamp01(coverageBrushStrength);
            base.OnValidate();
        }

        private void OnTransformParentChanged()
        {
            bool changed = SynchronizeSurfaceGroundFromHierarchy();
            if (changed && isActiveAndEnabled)
            {
                RebuildVegetation();
            }
        }

        protected override bool TryValidateBuildSource(out string error)
        {
            SynchronizeSurfaceGroundFromHierarchy();
            if (coverageGround != null)
            {
                error = string.Empty;
                return true;
            }

            error =
                "VegetationLayer requires a GeneratedGround ancestor. " +
                "Place the layer directly under GeneratedGround or beneath " +
                "its Vegetation child. No fallback field is rendered.";
            return false;
        }

        public void InitializeCoverage(bool full)
        {
            coverage ??= new VegetationCoverageField();
            coverage.Initialize(full);
            RebuildVegetation();
        }

        public void FillCoverage(float value)
        {
            coverage ??= new VegetationCoverageField();
            coverage.Fill(value);
            RebuildVegetation();
        }

        public void SetCoverageResolution(
            int resolution,
            bool preserveApproximateCoverage)
        {
            coverage ??= new VegetationCoverageField();
            coverage.SetResolution(
                resolution,
                preserveApproximateCoverage);
            RebuildVegetation();
        }

        public bool PaintCoverageStamp(
            Vector3 worldPosition,
            float brushRadiusMetres,
            float brushStrength,
            bool erase)
        {
            SynchronizeSurfaceGroundFromHierarchy();
            coverage ??= new VegetationCoverageField();
            return coverage.Paint(
                coverageGround,
                worldPosition,
                brushRadiusMetres,
                brushStrength,
                erase);
        }

        public void CompleteCoverageStroke(bool changed)
        {
            if (changed)
            {
                RebuildVegetation();
            }
        }

        public void SetCoverageAuthoringSettings(
            bool paintMode,
            float brushRadius,
            float brushStrength,
            bool eraseMode,
            bool showOverlay)
        {
            coveragePaintMode = paintMode;
            coverageBrushRadius = Mathf.Max(0.05f, brushRadius);
            coverageBrushStrength = Mathf.Clamp01(brushStrength);
            coverageEraseMode = eraseMode;
            showCoverageOverlay = showOverlay;
        }

        private bool SynchronizeSurfaceGroundFromHierarchy()
        {
            GeneratedGround resolvedGround =
                GetComponentInParent<GeneratedGround>(true);
            bool changed = coverageGround != resolvedGround;
            coverageGround = resolvedGround;
            return changed;
        }
    }
}
