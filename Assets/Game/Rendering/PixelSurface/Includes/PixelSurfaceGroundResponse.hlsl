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

#if defined(PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES)
            float ResolveGroundPaintedAccentCoverage(
                Varyings input)
            {
                if (_GroundPaintedAccentCoverageEnabled <= 0.5)
                {
                    return 0.0;
                }

                float3 groundLocalPosition =
                    mul(
                        _GroundPaintedAccentCoverageWorldToLocal,
                        float4(input.positionWS, 1.0)).xyz;
                float2 fieldSize =
                    max(
                        _GroundPaintedAccentCoverageOriginSize.zw,
                        float2(0.0001, 0.0001));
                float2 uv =
                    (groundLocalPosition.xz -
                     _GroundPaintedAccentCoverageOriginSize.xy) /
                    fieldSize;

                if (any(uv < 0.0) || any(uv > 1.0))
                {
                    return 0.0;
                }

                return saturate(
                    SAMPLE_TEXTURE2D(
                        _GroundPaintedAccentCoverage,
                        sampler_GroundPaintedAccentCoverage,
                        uv).r);
            }

            float ResolveGroundPaintedAccentLinesFeature(
                Varyings input,
                float exposureMask,
                float dampDepositMask,
                float vegetationMask,
                float compactionMask,
                float shoreMask,
                float rockyDryMask,
                float contractMask)
            {
                return
                    ResolveGroundPaintedAccentCoverage(input) *
                    contractMask *
                    saturate(_GroundPaintedAccentLineStrength);
            }

#endif // PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES

#endif // PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
