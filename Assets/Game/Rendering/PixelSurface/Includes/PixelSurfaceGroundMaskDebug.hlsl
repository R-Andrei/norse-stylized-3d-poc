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
                else if (mode == 28 || mode == 29 || mode == 30)
                {
                    float contractMask =
                        1.0 -
                        step(
                            0.995,
                            min(
                                min((float)input.color.r, (float)input.color.g),
                                (float)input.color.b));

                    if (mode == 28 &&
                        _GroundPaintedAccentCoverageEnabled > 0.5)
                    {
                        mask =
                            ResolveGroundPaintedAccentCoverage(input) *
                            contractMask;
                    }
                    else
                    {
                        float3 paintedAccent = ResolveGroundPaintedAccentFeature(
                            input,
                            ResolveGroundExposureMask(input) * contractMask,
                            ResolveGroundDampDepositMask(input),
                            ResolveGroundVegetationMask(input),
                            ResolveGroundCompactionMask(input),
                            ResolveGroundShoreMask(input),
                            ResolveGroundRockyDryMask(input),
                            contractMask);

                        if (mode == 28)
                        {
                            mask = paintedAccent.x;
                        }
                        else if (mode == 29)
                        {
                            mask = paintedAccent.y;
                        }
                        else
                        {
                            float signedMagnitude =
                                saturate(abs(paintedAccent.z) * 3.2);
                            float3 negativeColor = float3(0.16, 0.28, 0.95);
                            float3 positiveColor = float3(1.0, 0.86, 0.24);
                            float3 neutralColor = float3(0.025, 0.025, 0.035);
                            float3 signedColor = lerp(
                                negativeColor,
                                positiveColor,
                                step(0.0, paintedAccent.z));
                            return (half3)lerp(
                                neutralColor,
                                signedColor,
                                signedMagnitude);
                        }
                    }
                }
                else if (mode == 31)
                {
                    float contractMask =
                        1.0 -
                        step(
                            0.995,
                            min(
                                min((float)input.color.r, (float)input.color.g),
                                (float)input.color.b));

                    return (half3)ResolveGroundPaintedAccentFinalPrototypeDebugColor(
                        input,
                        ResolveGroundExposureMask(input) * contractMask,
                        ResolveGroundDampDepositMask(input),
                        ResolveGroundVegetationMask(input),
                        ResolveGroundCompactionMask(input),
                        ResolveGroundShoreMask(input),
                        ResolveGroundRockyDryMask(input),
                        contractMask);
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
