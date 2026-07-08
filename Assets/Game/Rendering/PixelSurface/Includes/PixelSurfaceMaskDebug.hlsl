#ifndef PS3D_PIXELSURFACEMASKDEBUG_HLSL
#define PS3D_PIXELSURFACEMASKDEBUG_HLSL

            half3 ResolveMaskDebugColor(Varyings input)
            {
                int mode = (int)round(_MaskDebugMode);

                if (mode <= 0)
                {
                    return half3(-1.0h, -1.0h, -1.0h);
                }

                if (mode == 4)
                {
                    float mask = saturate(input.materialMasks.z);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.92, 0.55),
                        mask);
                }

                if (mode == 17)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    float mask = saturate(atlas0.r * atlas0.b);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.92, 0.55),
                        mask);
                }

                if (mode == 5)
                {
                    float mask = ResolveGeneratedMassAtlasCreaseMask(input);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.20, 0.36, 1.0),
                        mask);
                }

                if (mode == 20)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    float mask = saturate(atlas0.g * atlas0.b);

                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.20, 0.36, 1.0),
                        mask);
                }

                if (mode == 15)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.92, 0.55),
                        saturate(atlas0.r));
                }

                if (mode == 16)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.35, 1.0, 0.45),
                        saturate(atlas0.g));
                }

                if (mode == 18)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.78, 0.28),
                        saturate(atlas0.b));
                }

                if (mode == 19)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.85, 0.45, 1.0),
                        saturate(atlas0.a));
                }

                if (mode == 21)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    return (half3)float3(
                        saturate(atlas0.r),
                        saturate(atlas0.g),
                        saturate(atlas0.b));
                }

                if (mode == 22)
                {
                    float4 atlas1 = ResolveGeneratedMassFeatureAtlas1(input);
                    return (half3)float3(
                        saturate(atlas1.r),
                        saturate(atlas1.g),
                        saturate(atlas1.b));
                }

                if (mode == 23)
                {
                    float4 atlas1 = ResolveGeneratedMassFeatureAtlas1(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(1.0, 0.58, 0.24),
                        saturate(atlas1.r));
                }

                if (mode == 24)
                {
                    float4 atlas0 = ResolveGeneratedMassFeatureAtlas0(input);
                    float4 atlas1 = ResolveGeneratedMassFeatureAtlas1(input);
                    float boundaryPresence = saturate(max(atlas0.r, atlas0.g));
                    float cross = saturate(atlas1.g);
                    float sideAmount = saturate(abs(cross - 0.5) * 2.0) * boundaryPresence;
                    float3 negativeSide = float3(0.22, 0.42, 1.0);
                    float3 positiveSide = float3(1.0, 0.46, 0.22);
                    float3 sideColour = lerp(negativeSide, positiveSide, step(0.5, cross));
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        sideColour,
                        sideAmount);
                }

                if (mode == 25)
                {
                    float4 atlas1 = ResolveGeneratedMassFeatureAtlas1(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.82, 0.55, 1.0),
                        saturate(atlas1.b));
                }

                if (mode == 26)
                {
                    float4 atlas1 = ResolveGeneratedMassFeatureAtlas1(input);
                    return (half3)lerp(
                        float3(0.025, 0.025, 0.035),
                        float3(0.55, 0.82, 1.0),
                        saturate(atlas1.a));
                }

                float mask = 0.0;
                if (mode == 1)
                {
                    mask = saturate((float)input.color.r);
                }
                else if (mode == 2)
                {
                    mask = saturate((float)input.color.g);
                }
                else if (mode == 3)
                {
                    mask = ResolveShaderCreviceBaseMask(input);
                }
                else if (mode == 6)
                {
                    mask = ResolveShaderDirtDepositMask(input);
                }
                else if (mode == 7)
                {
                    mask = ResolveGroundTonalMask(input);
                }
                else if (mode == 8)
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
                else if (mode == 14)
                {
                    float exposure = ResolveGroundExposureMask(input);
                    float damp = saturate(
                        ResolveGroundDampDepositMask(input) * 0.75 +
                        ResolveGroundShoreMask(input) * 0.45);
                    float vegetationOrDry = max(
                        ResolveGroundVegetationMask(input),
                        ResolveGroundRockyDryMask(input));

                    return (half3)float3(
                        exposure,
                        damp,
                        vegetationOrDry);
                }

                return (half3)lerp(
                    float3(0.025, 0.025, 0.035),
                    float3(1.0, 0.92, 0.55),
                    mask);
            }
#endif // PS3D_PIXELSURFACEMASKDEBUG_HLSL
