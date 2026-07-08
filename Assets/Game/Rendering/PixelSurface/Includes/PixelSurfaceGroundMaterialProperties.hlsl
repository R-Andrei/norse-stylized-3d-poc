#ifndef PS3D_PIXELSURFACEGROUNDMATERIALPROPERTIES_HLSL
#define PS3D_PIXELSURFACEGROUNDMATERIALPROPERTIES_HLSL

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _PixelCellSize;
                float _PixelSeed;
                float _PixelToneCount;
                float _PixelClusterStrength;
                float _PixelVariation;
                float _PixelVertexVariation;
                float _PixelEffectStrength;
                float _PixelBroadVariation;
                float _PixelWarpStrength;
                float _MaskDebugMode;
                float _SurfaceContract;
                float _GroundSnowResponse;
                float _GroundDampResponse;
                float _GroundVegetationResponse;
                float _GroundRockyDryResponse;
                float _GroundShoreDampStrength;
                float _HighlightCompressStrength;
                float _HighlightCompressStart;
                float _BottomDarkenStrength;
                float _BottomDarkenHeight;
                float _EdgeDarkenStrength;
                float _EdgeDarkenPower;
                float _ProfileContrast;
                float _ProfilePixelContrast;
                float _Wetness;
                float _WetDarkenStrength;
                float _WetPixelSoftening;
                float _WetSmoothnessBoost;
                float _FrostStrength;
                float _FrostCoverage;
                float _FrostContrast;
                float _FrostCreviceDarken;
                half4 _FrostColor;
                float _MonolithicFlatten;
                float _MonolithicSmoothnessBoost;
                float _Smoothness;
                float _SpecularStrength;
                float _LightingTintInfluence;
                float _AmbientStrength;
                float _DirectStrength;
                float _DiffuseWrap;
                float _ShadowAmbientStrength;
                float _FlatNormalStrength;
                float _ReceiveShadows;
                float _Cull;
            CBUFFER_END

#endif // PS3D_PIXELSURFACEGROUNDMATERIALPROPERTIES_HLSL
