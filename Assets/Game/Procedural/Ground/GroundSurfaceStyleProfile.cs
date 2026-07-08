using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    [CreateAssetMenu(
        fileName = "GSSP_NewGroundSurfaceStyle",
        menuName = "PS3D/Ground/Surface Style Profile")]
    public sealed class GroundSurfaceStyleProfile : ScriptableObject
    {
        [Tooltip("Human-facing name of this visual surface family.")]
        [SerializeField]
        private string displayName = "Ground Surface";

        [Tooltip("Default semantic/mask-generation profile used by this visual family unless a GeneratedGround object explicitly overrides it.")]
        [SerializeField]
        private GroundSurfaceProfile defaultSurfaceProfile;

        [Tooltip("Variant recipes available inside this visual surface family.")]
        [SerializeField]
        private List<GroundSurfaceVariantRecipe> variants =
            new List<GroundSurfaceVariantRecipe>();

        public string DisplayName =>
            string.IsNullOrWhiteSpace(displayName)
                ? name
                : displayName;

        public GroundSurfaceProfile DefaultSurfaceProfile =>
            defaultSurfaceProfile;

        public IReadOnlyList<GroundSurfaceVariantRecipe> Variants =>
            variants;

        public bool TryGetVariant(
            string variantId,
            out GroundSurfaceVariantRecipe variant)
        {
            if (variants != null &&
                !string.IsNullOrWhiteSpace(variantId))
            {
                for (int index = 0; index < variants.Count; index++)
                {
                    GroundSurfaceVariantRecipe candidate = variants[index];

                    if (candidate == null ||
                        !candidate.HasValidId)
                    {
                        continue;
                    }

                    if (candidate.Id == variantId)
                    {
                        variant = candidate;
                        return true;
                    }
                }
            }

            variant = null;
            return false;
        }

        public bool TryGetFirstVariant(
            out GroundSurfaceVariantRecipe variant)
        {
            if (variants != null)
            {
                for (int index = 0; index < variants.Count; index++)
                {
                    GroundSurfaceVariantRecipe candidate = variants[index];

                    if (candidate == null ||
                        !candidate.HasValidId)
                    {
                        continue;
                    }

                    variant = candidate;
                    return true;
                }
            }

            variant = null;
            return false;
        }
    }
}
