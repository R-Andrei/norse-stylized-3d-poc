#ifndef PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
#define PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL

            float ResolveSurfaceContractIsGround()
            {
                return step(0.5, _SurfaceContract);
            }

            float ResolveGroundTonalMask(Varyings input)
            {
                return saturate((float)input.color.r);
            }

            float ResolveGroundExposureMask(Varyings input)
            {
                return saturate((float)input.color.g);
            }

            float ResolveGroundDampDepositMask(Varyings input)
            {
                return saturate((float)input.color.b);
            }

            float ResolveGroundVegetationMask(Varyings input)
            {
                return saturate((float)input.color.a);
            }

            float ResolveGroundCompactionMask(Varyings input)
            {
                return saturate(input.materialMasks.x);
            }

            float ResolveGroundShoreMask(Varyings input)
            {
                return saturate(input.materialMasks.y);
            }

            float ResolveGroundRockyDryMask(Varyings input)
            {
                return saturate(input.materialMasks.z);
            }

            float ResolveGroundStandingWaterPotentialMask(Varyings input)
            {
                return saturate(input.materialMasks.w);
            }
#endif // PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
