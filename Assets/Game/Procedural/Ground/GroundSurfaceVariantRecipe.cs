using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    [Serializable]
    public sealed class GroundSurfaceVariantRecipe
    {
        [Tooltip("Stable identifier used by generated ground objects. Do not localize or rename after scenes reference it.")]
        [SerializeField]
        private string id = "ground.variant";

        [Tooltip("Human-facing variant name shown in the GeneratedGround Inspector.")]
        [SerializeField]
        private string displayName = "Variant";

        [SerializeField]
        private GroundMaterialControls materialControls =
            new GroundMaterialControls();

        [Tooltip("Optional surface-feature recipes owned by this variant. Patch M applies Directional Streaks and Patch N applies Pooled Wetness as the first shader-only features; other kinds are reserved for later modules.")]
        [SerializeField]
        private List<GroundSurfaceFeatureRecipe> features =
            new List<GroundSurfaceFeatureRecipe>();

        public string Id => id;

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? id
                : displayName;

        public GroundMaterialControls MaterialControls =>
            materialControls ??= new GroundMaterialControls();

        public IReadOnlyList<GroundSurfaceFeatureRecipe> Features =>
            features;

        public bool HasValidId => !string.IsNullOrWhiteSpace(id);


        public bool TryGetFirstShaderFeature(
            out GroundSurfaceFeatureRecipe feature)
        {
            if (features != null)
            {
                for (int index = 0; index < features.Count; index++)
                {
                    GroundSurfaceFeatureRecipe candidate = features[index];

                    if (candidate == null ||
                        !candidate.CanApplyAsShaderOnly ||
                        !IsSupportedSingleShaderFeature(candidate.Kind))
                    {
                        continue;
                    }

                    feature = candidate;
                    return true;
                }
            }

            feature = null;
            return false;
        }

        public bool TryGetFirstShaderFeature(
            GroundSurfaceFeatureKind requiredKind,
            out GroundSurfaceFeatureRecipe feature)
        {
            if (features != null)
            {
                for (int index = 0; index < features.Count; index++)
                {
                    GroundSurfaceFeatureRecipe candidate = features[index];

                    if (candidate == null ||
                        !candidate.CanApplyAsShaderOnly ||
                        candidate.Kind != requiredKind)
                    {
                        continue;
                    }

                    feature = candidate;
                    return true;
                }
            }

            feature = null;
            return false;
        }

        private static bool IsSupportedSingleShaderFeature(
            GroundSurfaceFeatureKind kind)
        {
            return kind == GroundSurfaceFeatureKind.DirectionalStreaks ||
                kind == GroundSurfaceFeatureKind.PooledWetness;
        }

        public string BuildFeatureSummary()
        {
            if (features == null || features.Count == 0)
            {
                return "Features: none";
            }

            List<string> names = new List<string>();

            for (int index = 0; index < features.Count; index++)
            {
                GroundSurfaceFeatureRecipe feature = features[index];

                if (feature == null || !feature.Enabled)
                {
                    continue;
                }

                names.Add(feature.Kind.ToString());
            }

            return names.Count == 0
                ? "Features: none enabled"
                : "Features: " + string.Join(", ", names);
        }
    }
}
