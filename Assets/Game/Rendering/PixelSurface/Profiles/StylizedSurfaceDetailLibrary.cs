using System;
using System.Collections.Generic;
using UnityEngine;

namespace ProgrammaticStylized3D.Rendering.PixelSurface
{
    /// <summary>
    /// Shared editor-built packed-detail array. Stable entry IDs are resolved
    /// at material-refresh time so profiles never serialize fragile slice indices.
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

#if UNITY_EDITOR
            [SerializeField]
            private Texture2D sourceTexture;
#endif

            public string StableId => stableId ?? string.Empty;
            public string DisplayName =>
                string.IsNullOrWhiteSpace(displayName)
                    ? StableId
                    : displayName;
#if UNITY_EDITOR
            public Texture2D SourceTexture => sourceTexture;
#endif
        }

        [Tooltip("Every source is normalized to this square resolution before it can be packed into the array.")]
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
        private string generatedSignature = string.Empty;

        public int SliceResolution => Mathf.Max(16, sliceResolution);
        public IReadOnlyList<Entry> Entries => entries;
        public Texture2DArray GeneratedTextureArray => generatedTextureArray;
        public string GeneratedSignature => generatedSignature ?? string.Empty;

        public bool TryResolve(
            string stableId,
            out Texture2DArray textureArray,
            out int sliceIndex)
        {
            textureArray = generatedTextureArray;
            sliceIndex = -1;

            if (textureArray == null ||
                string.IsNullOrWhiteSpace(stableId))
            {
                return false;
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
                    sliceIndex = index;
                    return index < textureArray.depth;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        public void SetGeneratedTextureArray(
            Texture2DArray textureArray,
            string signature)
        {
            generatedTextureArray = textureArray;
            generatedSignature = signature ?? string.Empty;
        }
#endif
    }
}
