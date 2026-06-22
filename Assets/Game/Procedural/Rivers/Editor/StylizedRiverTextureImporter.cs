using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed class StylizedRiverTextureImporter :
        AssetPostprocessor
    {
        private const string TextureFolderMarker =
            "/Resources/PS3DRiver/Textures/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.Contains(
                    TextureFolderMarker))
            {
                return;
            }

            TextureImporter importer =
                assetImporter as TextureImporter;

            if (importer == null)
            {
                return;
            }

            importer.textureType =
                TextureImporterType.Default;

            importer.sRGBTexture = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 1;
            importer.textureCompression =
                TextureImporterCompression.CompressedHQ;
        }
    }
}
