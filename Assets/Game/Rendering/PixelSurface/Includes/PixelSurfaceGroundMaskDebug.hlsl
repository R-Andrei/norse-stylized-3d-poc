#ifndef PS3D_PIXELSURFACEGROUNDMASKDEBUG_HLSL
#define PS3D_PIXELSURFACEGROUNDMASKDEBUG_HLSL

            half3 ResolveMaskDebugColor(Varyings input)
            {
                int mode = (int)round(_MaskDebugMode);

                if (mode <= 0)
                {
                    return half3(-1.0h, -1.0h, -1.0h);
                }

                float mask = 0.0;
                if (mode == 1 || mode == 7)
                {
                    mask = ResolveGroundTonalMask(input);
                }
                else if (mode == 2 || mode == 8)
                {
                    mask = ResolveGroundExposureMask(input);
                }
                else if (mode == 9)
                {
                    mask = ResolveGroundDampDepositMask(input);
                }
                else if (mode == 10)
                {
                    mask = ResolveGroundVegetationMask(input);
                }
                else if (mode == 11)
                {
                    mask = ResolveGroundCompactionMask(input);
                }
                else if (mode == 12)
                {
                    mask = ResolveGroundShoreMask(input);
                }
                else if (mode == 13)
                {
                    mask = ResolveGroundRockyDryMask(input);
                }
                else if (mode == 27)
                {
                    mask = ResolveGroundStandingWaterPotentialMask(input);
                }
                else if (mode == 28)
                {
                    float contractMask =
                        1.0 -
                        step(
                            0.995,
                            min(
                                min((float)input.color.r, (float)input.color.g),
                                (float)input.color.b));
                    mask =
                        ResolveGroundPaintedAccentCoverage(input) *
                        contractMask;
                }
                else if (mode == 14)
                {
                    float exposure = ResolveGroundExposureMask(input);
                    float damp = saturate(
                        ResolveGroundDampDepositMask(input) * 0.82 +
                        ResolveGroundShoreMask(input) * 0.24);
                    float vegetationOrDry = max(
                        ResolveGroundVegetationMask(input),
                        ResolveGroundRockyDryMask(input));

                    return (half3)float3(
                        exposure,
                        damp,
                        vegetationOrDry);
                }
                else
                {
                    return half3(-1.0h, -1.0h, -1.0h);
                }

                return (half3)lerp(
                    float3(0.025, 0.025, 0.035),
                    float3(1.0, 0.92, 0.55),
                    mask);
            }
#endif // PS3D_PIXELSURFACEGROUNDMASKDEBUG_HLSL
