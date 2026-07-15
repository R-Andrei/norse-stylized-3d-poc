#ifndef PS3D_PIXELSURFACEGROUNDMACRO_HLSL
#define PS3D_PIXELSURFACEGROUNDMACRO_HLSL

            struct PS3D_GroundMacroRegionResult
            {
                float signedRegion;
                float2 warp;
            };

            float PS3D_ResolveGroundMacroTonalAmplitude()
            {
                return max(0.0, _PixelBroadVariation) * 3.0;
            }

            PS3D_GroundMacroRegionResult PS3D_EvaluateGroundMacroRegion(
                float3 positionWS)
            {
                PS3D_GroundMacroRegionResult result;

                float macroScale = max(_GroundMacroPatchScale, 0.0001);
                float2 baseCoordinate = positionWS.xz / macroScale;
                float seedCoordinate = _PixelSeed * 0.013;
                float2 patternCoordinate =
                    baseCoordinate +
                    _GroundMacroPatchSeedScroll.xy;

                float3 warpCoordinate =
                    float3(patternCoordinate * 0.43, seedCoordinate);
                float2 warp =
                    float2(
                        PS3D_ValueNoise31(
                            warpCoordinate +
                            float3(11.17, 29.31, 7.73)),
                        PS3D_ValueNoise31(
                            warpCoordinate +
                            float3(23.31, 17.73, 5.37))) *
                    2.0 -
                    1.0;

                float2 regionCoordinate =
                    patternCoordinate + warp * 0.52;
                float primaryRegion = PS3D_ValueNoise31(
                    float3(regionCoordinate, seedCoordinate + 37.47));
                float secondaryRegion = PS3D_ValueNoise31(
                    float3(regionCoordinate * 1.65, seedCoordinate + 53.29));
                float regionalSource = saturate(
                    primaryRegion + (secondaryRegion - 0.5) * 0.14);

                float averageSeparation =
                    max(0.0, _GroundMacroPatchSeparation);
                float localSeparationVariation =
                    warp.x * warp.y * 0.08;
                float localGap = clamp(
                    averageSeparation * 0.28 +
                    localSeparationVariation,
                    0.0,
                    0.98);
                float halfGap = localGap * 0.5;
                float darkNeutralBoundary = 0.5 - halfGap;
                float lightNeutralBoundary = 0.5 + halfGap;

                float transitionWidth = lerp(
                    0.06,
                    0.35,
                    saturate(_GroundMacroPatchTransitionSoftness));
                float darkPlateauBoundary =
                    darkNeutralBoundary - transitionWidth;
                float lightPlateauBoundary =
                    lightNeutralBoundary + transitionWidth;

                float darkRegion =
                    1.0 - smoothstep(
                        darkPlateauBoundary,
                        darkNeutralBoundary,
                        regionalSource);
                float lightRegion =
                    smoothstep(
                        lightNeutralBoundary,
                        lightPlateauBoundary,
                        regionalSource);

                result.signedRegion = clamp(
                    lightRegion - darkRegion,
                    -1.0,
                    1.0);
                result.warp = warp;
                return result;
            }

#endif // PS3D_PIXELSURFACEGROUNDMACRO_HLSL
