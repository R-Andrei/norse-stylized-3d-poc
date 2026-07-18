using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface
{
    public enum StylizedSurfaceDetailSourceMode
    {
        PrepackedDetail = 0,
        AuthoredMaterialSet = 1
    }

    /// <summary>
    /// Shared editor-built surface-material arrays. Stable entry IDs resolve
    /// generated packed-detail and optional authored-colour slices at material
    /// refresh time, so profiles never serialize fragile slice indices or
    /// editor-only source textures.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SSDL_NewSurfaceDetailLibrary",
        menuName = "PS3D/Pixel Surface/Surface Detail Library")]
    public sealed class StylizedSurfaceDetailLibrary : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [SerializeField]
            private string stableId = "surface-detail";

            [SerializeField]
            private string displayName = "Surface Detail";

            [SerializeField]
            private StylizedSurfaceDetailSourceMode sourceMode =
                StylizedSurfaceDetailSourceMode.PrepackedDetail;

#if UNITY_EDITOR
            [Header("Prepacked Detail Source")]
            [SerializeField]
            private Texture2D sourceTexture;

            [Header("Authored Material-Set Sources")]
            [SerializeField]
            private Texture2D authoredBaseColor;

            [SerializeField]
            private Texture2D authoredNormal;

            [SerializeField]
            private bool flipAuthoredNormalGreen;

            [SerializeField]
            private Texture2D authoredHeight;

            [SerializeField]
            private Texture2D authoredAmbientOcclusion;

            [SerializeField]
            private Texture2D authoredRoughness;

            [Tooltip("Contribution of inverted authored height to the packed cavity channel.")]
            [Range(0f, 2f)]
            [SerializeField]
            private float authoredHeightCavityWeight = 1f;

            [Tooltip("Contribution of inverted ambient occlusion to the packed cavity channel.")]
            [Range(0f, 2f)]
            [SerializeField]
            private float authoredAmbientOcclusionCavityWeight = 1f;

            [Tooltip("Removes broad low-level cavity response before the packed channel is written.")]
            [Range(0f, 0.95f)]
            [SerializeField]
            private float authoredCavityFloor = 0.05f;
#endif

            public string StableId => stableId ?? string.Empty;
            public string DisplayName =>
                string.IsNullOrWhiteSpace(displayName)
                    ? StableId
                    : displayName;
            public StylizedSurfaceDetailSourceMode SourceMode => sourceMode;
            public bool UsesAuthoredMaterialSet =>
                sourceMode == StylizedSurfaceDetailSourceMode.AuthoredMaterialSet;
#if UNITY_EDITOR
            public Texture2D SourceTexture => sourceTexture;
            public Texture2D AuthoredBaseColor => authoredBaseColor;
            public Texture2D AuthoredNormal => authoredNormal;
            public bool FlipAuthoredNormalGreen => flipAuthoredNormalGreen;
            public Texture2D AuthoredHeight => authoredHeight;
            public Texture2D AuthoredAmbientOcclusion => authoredAmbientOcclusion;
            public Texture2D AuthoredRoughness => authoredRoughness;
            public float AuthoredHeightCavityWeight =>
                Mathf.Clamp(authoredHeightCavityWeight, 0f, 2f);
            public float AuthoredAmbientOcclusionCavityWeight =>
                Mathf.Clamp(authoredAmbientOcclusionCavityWeight, 0f, 2f);
            public float AuthoredCavityFloor =>
                Mathf.Clamp(authoredCavityFloor, 0f, 0.95f);
#endif
        }

        [Tooltip("Every generated runtime slice uses this square resolution.")]
        [Min(16)]
        [SerializeField]
        private int sliceResolution = 256;

        [SerializeField]
        private List<Entry> entries = new List<Entry>();

        [HideInInspector]
        [SerializeField]
        private Texture2DArray generatedTextureArray;

        [HideInInspector]
        [SerializeField]
        private Texture2DArray generatedAuthoredColorArray;

        [HideInInspector]
        [SerializeField]
        private List<int> generatedAuthoredColorSliceIndices =
            new List<int>();

        [HideInInspector]
        [SerializeField]
        private string generatedSignature = string.Empty;

        public int SliceResolution => Mathf.Max(16, sliceResolution);
        public IReadOnlyList<Entry> Entries => entries;
        public Texture2DArray GeneratedTextureArray => generatedTextureArray;
        public Texture2DArray GeneratedAuthoredColorArray =>
            generatedAuthoredColorArray;
        public IReadOnlyList<int> GeneratedAuthoredColorSliceIndices
        {
            get
            {
                return generatedAuthoredColorSliceIndices != null
                    ? (IReadOnlyList<int>)generatedAuthoredColorSliceIndices
                    : Array.Empty<int>();
            }
        }
        public string GeneratedSignature => generatedSignature ?? string.Empty;

        public bool TryResolve(
            string stableId,
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = generatedTextureArray;
            sliceIndex = FindEntryIndex(stableId);
            return textureArray != null &&
                   sliceIndex >= 0 &&
                   sliceIndex < textureArray.depth;
        }

        public bool TryResolveAuthoredColor(
            string stableId,
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = generatedAuthoredColorArray;
            sliceIndex = -1;

            int entryIndex = FindEntryIndex(stableId);
            if (textureArray == null ||
                entryIndex < 0 ||
                generatedAuthoredColorSliceIndices == null ||
                entryIndex >= generatedAuthoredColorSliceIndices.Count)
            {
                return false;
            }

            sliceIndex = generatedAuthoredColorSliceIndices[entryIndex];
            return sliceIndex >= 0 && sliceIndex < textureArray.depth;
        }

        private int FindEntryIndex(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId))
            {
                return -1;
            }

            for (int index = 0; index < entries.Count; index++)
            {
                Entry entry = entries[index];
                if (entry != null &&
                    string.Equals(
                        entry.StableId,
                        stableId,
                        StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }

#if UNITY_EDITOR
        public void SetGeneratedArrays(
            Texture2DArray detailArray,
            Texture2DArray authoredColorArray,
            IReadOnlyList<int> authoredColorSliceIndices,
            string signature)
        {
            generatedTextureArray = detailArray;
            generatedAuthoredColorArray = authoredColorArray;
            generatedAuthoredColorSliceIndices ??= new List<int>();
            generatedAuthoredColorSliceIndices.Clear();
            if (authoredColorSliceIndices != null)
            {
                for (int index = 0;
                     index < authoredColorSliceIndices.Count;
                     index++)
                {
                    generatedAuthoredColorSliceIndices.Add(
                        authoredColorSliceIndices[index]);
                }
            }

            generatedSignature = signature ?? string.Empty;
        }
#endif
    }
}
