using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [CreateAssetMenu(
        fileName = "TreeReferenceCalibrationPreset",
        menuName = "PS3D/Trees/Tree Reference Calibration Preset")]
    public sealed class TreeReferenceCalibrationPreset : ScriptableObject
    {
        public const int CurrentCalibrationVersion = 2;

        [Header("Identity")]
        [SerializeField]
        private string stableIdentity = "tree-reference-calibration";

        [SerializeField]
        private int calibrationVersion = CurrentCalibrationVersion;

        [SerializeField]
        private TreeFamily family = TreeFamily.Common;

        [Header("Optional Imported Reference")]
        [SerializeField]
        private string sourceFbxPath;

        [SerializeField]
        private string sourceFbxGuid;

        [Header("Target Measurements")]
        [SerializeField, Min(0f)]
        private float targetVisibleHeight;

        [SerializeField, Min(0f)]
        private float targetVisibleWidth;

        [SerializeField, Min(0f)]
        private float targetVisibleDepth;

        [SerializeField, Range(0f, 1f)]
        private float targetCrownStart = 0.3f;

        [SerializeField, Range(0f, 1f)]
        private float dimensionTolerance = 0.15f;

        [Header("Calibration Overrides")]
        [SerializeField]
        private TreeGenerationOverrides parameterOverrides =
            new TreeGenerationOverrides();

        [SerializeField]
        private TreeMaterialPalette paletteOverride;

        public string StableIdentity => stableIdentity;
        public int CalibrationVersion => calibrationVersion;
        public TreeFamily Family => family;
        public string SourceFbxPath => sourceFbxPath;
        public string SourceFbxGuid => sourceFbxGuid;
        public float TargetVisibleHeight => Mathf.Max(0f, targetVisibleHeight);
        public float TargetVisibleWidth => Mathf.Max(0f, targetVisibleWidth);
        public float TargetVisibleDepth => Mathf.Max(0f, targetVisibleDepth);
        public float TargetCrownStart => Mathf.Clamp01(targetCrownStart);
        public float DimensionTolerance => Mathf.Clamp01(dimensionTolerance);
        public TreeGenerationOverrides ParameterOverrides => parameterOverrides;
        public TreeMaterialPalette PaletteOverride => paletteOverride;

        public void Initialize(
            TreeFamily targetFamily,
            string identity,
            string fbxPath,
            float visibleHeight,
            float visibleWidth)
        {
            Initialize(
                targetFamily,
                identity,
                fbxPath,
                string.Empty,
                visibleHeight,
                visibleWidth,
                visibleWidth);
        }

        public void Initialize(
            TreeFamily targetFamily,
            string identity,
            string fbxPath,
            string fbxGuid,
            float visibleHeight,
            float visibleWidth,
            float visibleDepth)
        {
            family = targetFamily;
            stableIdentity = string.IsNullOrWhiteSpace(identity)
                ? "tree-reference-" + targetFamily.ToString().ToLowerInvariant()
                : identity;
            sourceFbxPath = fbxPath ?? string.Empty;
            sourceFbxGuid = fbxGuid ?? string.Empty;
            targetVisibleHeight = Mathf.Max(0f, visibleHeight);
            targetVisibleWidth = Mathf.Max(0f, visibleWidth);
            targetVisibleDepth = Mathf.Max(0f, visibleDepth);
            dimensionTolerance = 0.15f;
            calibrationVersion = CurrentCalibrationVersion;
            parameterOverrides ??= new TreeGenerationOverrides();
            parameterOverrides.UpgradeTreeGen2BControls();
            parameterOverrides.ConfigureReferenceDimensions(
                targetVisibleHeight,
                targetVisibleWidth);
        }

        public void SynchronizeImportedReference(
            string fbxPath,
            string fbxGuid,
            float visibleHeight,
            float visibleWidth,
            float visibleDepth)
        {
            sourceFbxPath = fbxPath ?? string.Empty;
            sourceFbxGuid = fbxGuid ?? string.Empty;
            targetVisibleHeight = Mathf.Max(0f, visibleHeight);
            targetVisibleWidth = Mathf.Max(0f, visibleWidth);
            targetVisibleDepth = Mathf.Max(0f, visibleDepth);
            if (calibrationVersion < CurrentCalibrationVersion)
            {
                dimensionTolerance = 0.15f;
            }
            calibrationVersion = CurrentCalibrationVersion;
            parameterOverrides ??= new TreeGenerationOverrides();
            parameterOverrides.UpgradeTreeGen2BControls();
            parameterOverrides.ConfigureReferenceDimensions(
                targetVisibleHeight,
                targetVisibleWidth);
        }

        public void ApplyFamilyStructuralRanges(
            TreeFamilyProfile profile)
        {
            if (profile == null)
            {
                return;
            }

            parameterOverrides ??= new TreeGenerationOverrides();
            parameterOverrides.UpgradeTreeGen2BControls();
            parameterOverrides.ConfigureReferenceDimensions(
                targetVisibleHeight,
                targetVisibleWidth,
                profile.PrimaryBranches.LengthRatio);
        }

        public bool ValidatePreset(
            TreeFamilyProfile familyProfile,
            List<string> failures)
        {
            if (failures == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Calibration preset stable identity is empty.");
            }

            if (targetVisibleHeight <= 0f ||
                targetVisibleWidth <= 0f ||
                targetVisibleDepth <= 0f)
            {
                failures.Add("Calibration preset target dimensions must all be positive.");
            }

            if (familyProfile == null)
            {
                failures.Add("Calibration preset cannot validate without a family profile.");
            }
            else if (familyProfile.Family != family)
            {
                failures.Add(
                    "Calibration preset family " + family +
                    " does not match profile family " + familyProfile.Family + ".");
            }

            return failures.Count == 0;
        }

        private void OnValidate()
        {
            calibrationVersion = Mathf.Max(1, calibrationVersion);
            targetVisibleHeight = Mathf.Max(0f, targetVisibleHeight);
            targetVisibleWidth = Mathf.Max(0f, targetVisibleWidth);
            targetVisibleDepth = Mathf.Max(0f, targetVisibleDepth);
            targetCrownStart = Mathf.Clamp01(targetCrownStart);
            dimensionTolerance = Mathf.Clamp01(dimensionTolerance);
            parameterOverrides ??= new TreeGenerationOverrides();
        }
    }
}
