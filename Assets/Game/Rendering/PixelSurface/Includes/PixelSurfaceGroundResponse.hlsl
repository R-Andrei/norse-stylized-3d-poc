#ifndef PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
#define PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL

            float ResolveSurfaceContractIsGround()
            {
                return step(0.5, _SurfaceContract);
            }

            float ResolveGroundRiverCoupledEnabled()
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT) && \
    defined(PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES)
                return step(0.5, _GroundRiverCoupledEnabled);
#else
                return 0.0;
#endif
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
                float shoreMask = saturate(input.materialMasks.y);
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                return
                    shoreMask *
                    ResolveGroundRiverCoupledEnabled();
#else
                return shoreMask;
#endif
            }

            float ResolveGroundRockyDryMask(Varyings input)
            {
                return saturate(input.materialMasks.z);
            }

            float ResolveGroundStandingWaterPotentialMask(Varyings input)
            {
                return saturate(input.materialMasks.w);
            }

            float ResolveGroundRiverbedSupportMask(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                return
                    ResolveGroundRiverCoupledEnabled() *
                    saturate((float)input.riverCoupledMasks.x);
#else
                return 0.0;
#endif
            }

            float ResolveGroundRiverBankDistance(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                return
                    ResolveGroundRiverCoupledEnabled() *
                    max(0.0, (float)input.riverCoupledMasks.y);
#else
                return 0.0;
#endif
            }

            float ResolveGroundRiverBankDomain(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                float enabled = ResolveGroundRiverCoupledEnabled();
                float encodedDomain =
                    saturate((float)input.riverCoupledMasks.z);
                float riverbedSupport =
                    ResolveGroundRiverbedSupportMask(input);
                return saturate(
                    enabled *
                    encodedDomain *
                    (1.0 - riverbedSupport));
#else
                return 0.0;
#endif
            }

#if defined(PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES)
            float3 ResolveGroundBankZoneWeights(
                Varyings input)
            {
                float shoreMask = ResolveGroundShoreMask(input);
                float softness = saturate(_GroundBankTransitionSoftness);
                float reach = saturate(_GroundBankMaterialReach);

                float broadCenter = lerp(0.285, 0.075, reach);
                float broadHalfWidth = lerp(0.012, 0.075, softness);
                float immediateHalfWidth = lerp(0.018, 0.065, softness);
                float waterlineHalfWidth = lerp(0.018, 0.050, softness);

                float broadBank = smoothstep(
                    broadCenter - broadHalfWidth,
                    broadCenter + broadHalfWidth,
                    shoreMask);
                float immediateBank = smoothstep(
                    0.235 - immediateHalfWidth,
                    0.235 + immediateHalfWidth,
                    shoreMask);
                float waterlineCore = smoothstep(
                    0.355 - waterlineHalfWidth,
                    0.355 + waterlineHalfWidth,
                    shoreMask);

                float bankDomain =
                    ResolveGroundRiverBankDomain(input);

                return saturate(
                    float3(
                        broadBank,
                        immediateBank,
                        waterlineCore) *
                    bankDomain);
            }

            float ResolveGroundOuterBankExtensionBlend(
                Varyings input)
            {
                float extension = max(0.0, _GroundOuterBankExtension);
                float enabled =
                    step(0.0001, extension) *
                    ResolveGroundRiverBankDomain(input);
                float fade = max(0.05, _GroundOuterBankFade);
                float distance = ResolveGroundRiverBankDistance(input);
                float distanceWeight =
                    1.0 -
                    smoothstep(
                        extension,
                        extension + fade,
                        distance);

                return saturate(
                    enabled *
                    saturate(_GroundOuterBankStrength) *
                    distanceWeight);
            }

            float ResolveGroundBankMaterialBlend(
                Varyings input)
            {
                float enabled = saturate(_GroundBankLayerEnabled);
                float strength = saturate(_GroundBankMaterialStrength);
                float3 zones = ResolveGroundBankZoneWeights(input);
                float broadContribution = zones.x * 0.65;
                float immediateContribution =
                    zones.y * saturate(_GroundImmediateBankExposure);
                float waterlineContribution =
                    zones.z * saturate(_GroundWaterlineMaterialStrength);
                float outerContribution =
                    ResolveGroundOuterBankExtensionBlend(input);
                float composedZones =
                    1.0 -
                    (1.0 - broadContribution) *
                    (1.0 - immediateContribution) *
                    (1.0 - waterlineContribution) *
                    (1.0 - outerContribution);

                return saturate(
                    enabled *
                    strength *
                    composedZones);
            }

            float4 ResolveGroundBankCoverRetention(
                float bankMaterialBlend)
            {
                float4 fullRetention =
                    float4(1.0, 1.0, 1.0, 1.0);
                float4 spatialRetention = lerp(
                    fullRetention,
                    saturate(_GroundBankLayerCoverRetention),
                    saturate(bankMaterialBlend));

                return lerp(
                    fullRetention,
                    spatialRetention,
                    saturate(_GroundBankCoverRetreatStrength));
            }

            float4 ResolveGroundBankCoverRetention(
                Varyings input)
            {
                return ResolveGroundBankCoverRetention(
                    ResolveGroundBankMaterialBlend(input));
            }

            float4 ResolveGroundBankCoverRetreat(
                Varyings input)
            {
                return saturate(
                    float4(1.0, 1.0, 1.0, 1.0) -
                    ResolveGroundBankCoverRetention(input));
            }

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
                float contractMask,
                float paintedAccentRetention)
            {
                return
                    ResolveGroundPaintedAccentCoverage(input) *
                    saturate(paintedAccentRetention) *
                    contractMask *
                    saturate(_GroundPaintedAccentInkOpacity);
            }

#endif // PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES

#endif // PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
