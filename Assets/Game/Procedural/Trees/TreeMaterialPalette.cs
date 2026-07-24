using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Trees
{
    [CreateAssetMenu(
        fileName = "TreeMaterialPalette",
        menuName = "PS3D/Trees/Tree Material Palette")]
    public sealed class TreeMaterialPalette : ScriptableObject
    {
        public const int CurrentPaletteVersion = 1;

        [Header("Identity")]
        [SerializeField]
        private string stableIdentity = "tree-material-palette";

        [SerializeField]
        private int paletteVersion = CurrentPaletteVersion;

        [Header("Shared Texture Identity")]
        [SerializeField]
        private Texture2D barkAlbedo;

        [SerializeField]
        private Texture2D barkNormal;

        [SerializeField]
        private Texture2D foliageTintable;

        [SerializeField]
        private Texture2D foliageReferenceColour;

        [Header("Bark Colour")]
        [SerializeField]
        private Color barkTint = Color.white;

        [SerializeField]
        private TreeFloatRange barkHueShift = new TreeFloatRange(-0.025f, 0.025f);

        [SerializeField]
        private TreeFloatRange barkSaturation = new TreeFloatRange(0.9f, 1.1f);

        [SerializeField]
        private TreeFloatRange barkValue = new TreeFloatRange(0.88f, 1.08f);

        [SerializeField]
        private TreeFloatRange rootDarkening = new TreeFloatRange(0.05f, 0.18f);

        [SerializeField]
        private TreeFloatRange upperTrunkVariation = new TreeFloatRange(-0.06f, 0.08f);

        [SerializeField]
        private TreeFloatRange branchOrderVariation = new TreeFloatRange(-0.05f, 0.06f);

        [Header("Foliage Colour")]
        [SerializeField]
        private Color foliageBaseColor = new Color(0.36f, 0.58f, 0.22f, 1f);

        [SerializeField]
        private Color foliageHighlightColor = new Color(0.48f, 0.7f, 0.28f, 1f);

        [SerializeField]
        private Color foliageShadowColor = new Color(0.16f, 0.28f, 0.09f, 1f);

        [SerializeField]
        private TreeFloatRange foliageHueVariation = new TreeFloatRange(-0.035f, 0.035f);

        [SerializeField]
        private TreeFloatRange foliageSaturationVariation = new TreeFloatRange(0.9f, 1.12f);

        [SerializeField]
        private TreeFloatRange foliageValueVariation = new TreeFloatRange(0.88f, 1.12f);

        [SerializeField]
        private TreeFloatRange clusterColourVariation = new TreeFloatRange(0f, 0.12f);

        [SerializeField]
        private Color foliageBottomGradient = new Color(0.28f, 0.45f, 0.16f, 1f);

        [SerializeField]
        private Color foliageTopGradient = new Color(0.42f, 0.64f, 0.24f, 1f);

        public string StableIdentity => stableIdentity;
        public int PaletteVersion => paletteVersion;
        public Texture2D BarkAlbedo => barkAlbedo;
        public Texture2D BarkNormal => barkNormal;
        public Texture2D FoliageTintable => foliageTintable;
        public Texture2D FoliageReferenceColour => foliageReferenceColour;
        public Color BarkTint => barkTint;
        public TreeFloatRange BarkHueShift => barkHueShift;
        public TreeFloatRange BarkSaturation => barkSaturation;
        public TreeFloatRange BarkValue => barkValue;
        public TreeFloatRange RootDarkening => rootDarkening;
        public TreeFloatRange UpperTrunkVariation => upperTrunkVariation;
        public TreeFloatRange BranchOrderVariation => branchOrderVariation;
        public Color FoliageBaseColor => foliageBaseColor;
        public Color FoliageHighlightColor => foliageHighlightColor;
        public Color FoliageShadowColor => foliageShadowColor;
        public TreeFloatRange FoliageHueVariation => foliageHueVariation;
        public TreeFloatRange FoliageSaturationVariation => foliageSaturationVariation;
        public TreeFloatRange FoliageValueVariation => foliageValueVariation;
        public TreeFloatRange ClusterColourVariation => clusterColourVariation;
        public Color FoliageBottomGradient => foliageBottomGradient;
        public Color FoliageTopGradient => foliageTopGradient;

        public void ResetForFamily(TreeFamily family)
        {
            stableIdentity = "tree-palette-" + family.ToString().ToLowerInvariant();
            paletteVersion = CurrentPaletteVersion;

            switch (family)
            {
                case TreeFamily.Pine:
                    barkTint = new Color(0.92f, 0.88f, 0.8f, 1f);
                    foliageBaseColor = new Color(0.24f, 0.43f, 0.13f, 1f);
                    foliageHighlightColor = new Color(0.34f, 0.56f, 0.18f, 1f);
                    foliageShadowColor = new Color(0.1f, 0.21f, 0.055f, 1f);
                    foliageBottomGradient = new Color(0.18f, 0.32f, 0.09f, 1f);
                    foliageTopGradient = new Color(0.3f, 0.5f, 0.15f, 1f);
                    break;
                case TreeFamily.Twisted:
                    barkTint = new Color(0.86f, 0.78f, 0.68f, 1f);
                    foliageBaseColor = new Color(0.38f, 0.48f, 0.16f, 1f);
                    foliageHighlightColor = new Color(0.53f, 0.62f, 0.21f, 1f);
                    foliageShadowColor = new Color(0.17f, 0.22f, 0.07f, 1f);
                    foliageBottomGradient = new Color(0.28f, 0.34f, 0.1f, 1f);
                    foliageTopGradient = new Color(0.44f, 0.54f, 0.17f, 1f);
                    break;
                case TreeFamily.Dead:
                    barkTint = new Color(0.62f, 0.58f, 0.53f, 1f);
                    foliageBaseColor = Color.black;
                    foliageHighlightColor = Color.black;
                    foliageShadowColor = Color.black;
                    foliageBottomGradient = Color.black;
                    foliageTopGradient = Color.black;
                    break;
                default:
                    barkTint = new Color(0.95f, 0.88f, 0.78f, 1f);
                    foliageBaseColor = new Color(0.36f, 0.58f, 0.22f, 1f);
                    foliageHighlightColor = new Color(0.48f, 0.7f, 0.28f, 1f);
                    foliageShadowColor = new Color(0.16f, 0.28f, 0.09f, 1f);
                    foliageBottomGradient = new Color(0.28f, 0.45f, 0.16f, 1f);
                    foliageTopGradient = new Color(0.42f, 0.64f, 0.24f, 1f);
                    break;
            }
        }

        public bool ValidatePalette(List<string> failures)
        {
            if (failures == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(stableIdentity))
            {
                failures.Add("Material palette stable identity is empty.");
            }

            if (!TreeDeterministicUtility.IsFinite(barkTint.r) ||
                !TreeDeterministicUtility.IsFinite(foliageBaseColor.r))
            {
                failures.Add("Material palette contains non-finite colours.");
            }

            return failures.Count == 0;
        }

        private void OnValidate()
        {
            paletteVersion = Mathf.Max(1, paletteVersion);
        }
    }
}
