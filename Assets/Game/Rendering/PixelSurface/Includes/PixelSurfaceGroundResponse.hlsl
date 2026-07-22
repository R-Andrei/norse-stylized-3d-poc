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

            float ResolveGroundRiverbedInwardDistance(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                return
                    ResolveGroundRiverCoupledEnabled() *
                    max(0.0, (float)input.riverCoupledMasks.w);
#else
                return 0.0;
#endif
            }

#if defined(PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES)
            float ResolveGroundBoundedUnion(
                float firstContribution,
                float secondContribution)
            {
                float first = saturate(firstContribution);
                float second = saturate(secondContribution);
                return 1.0 - (1.0 - first) * (1.0 - second);
            }

            float ResolveGroundComposedShoreWetness(
                float domainWeight,
                float distance,
                float shore)
            {
                float profileEnabled =
                    saturate(_GroundShoreHydrologyEnabled);
                float masterStrength =
                    saturate(_GroundShoreHydrologySpatialA.x);
                float reach =
                    max(0.0, _GroundShoreHydrologySpatialA.y);
                float fade =
                    max(0.05, _GroundShoreHydrologySpatialA.z);
                float domain = saturate(domainWeight);
                float distanceWeight =
                    (1.0 - smoothstep(
                        reach,
                        reach + fade,
                        max(0.0, distance))) *
                    domain;
                float broadContribution =
                    distanceWeight *
                    saturate(_GroundShoreHydrologySpatialA.w);
                float immediateContribution =
                    smoothstep(0.17, 0.30, shore) *
                    saturate(_GroundShoreHydrologySpatialB.x) *
                    domain;
                float waterlineContribution =
                    smoothstep(0.31, 0.40, shore) *
                    saturate(_GroundShoreHydrologySpatialB.y) *
                    domain;
                float composedWetness = ResolveGroundBoundedUnion(
                    ResolveGroundBoundedUnion(
                        broadContribution,
                        immediateContribution),
                    waterlineContribution);

                return saturate(
                    profileEnabled *
                    masterStrength *
                    composedWetness);
            }

            float ResolveGroundLocalShoreWetness(
                Varyings input)
            {
                return ResolveGroundComposedShoreWetness(
                    ResolveGroundRiverBankDomain(input),
                    ResolveGroundRiverBankDistance(input),
                    ResolveGroundShoreMask(input));
            }

            float ResolveGroundBankEdgeWetness(Varyings input)
            {
                return ResolveGroundComposedShoreWetness(
                    1.0,
                    0.0,
                    ResolveGroundShoreMask(input));
            }

            float ResolveGroundRiverbedWetness(
                Varyings input)
            {
                float support = ResolveGroundRiverbedSupportMask(input);
                float baseWetness =
                    saturate(_GroundRiverbedHydrologyEnabled) *
                    saturate(_GroundRiverbedWetnessStrength) *
                    support;
                float transitionDistance =
                    max(0.0, _GroundRiverbedWetnessTransition.x);
                if (support <= 0.0001 || transitionDistance <= 0.0001)
                {
                    return saturate(baseWetness);
                }

                float distance01 = saturate(
                    ResolveGroundRiverbedInwardDistance(input) /
                    max(0.0001, transitionDistance));
                float smoothDistance =
                    distance01 *
                    distance01 *
                    (3.0 - 2.0 * distance01);
                float interiorWeight = lerp(
                    distance01,
                    smoothDistance,
                    saturate(_GroundRiverbedWetnessTransition.y));
                float edgeWetness =
                    ResolveGroundBankEdgeWetness(input) * support;

                return saturate(
                    lerp(
                        edgeWetness,
                        baseWetness,
                        interiorWeight));
            }

            float ResolveGroundEffectiveWetness(
                float localShoreWetness,
                float riverbedWetness)
            {
                return ResolveGroundBoundedUnion(
                    ResolveGroundBoundedUnion(
                        saturate(_Wetness),
                        localShoreWetness),
                    riverbedWetness);
            }

            float ResolveGroundEffectiveWetness(
                Varyings input)
            {
                return ResolveGroundEffectiveWetness(
                    ResolveGroundLocalShoreWetness(input),
                    ResolveGroundRiverbedWetness(input));
            }

            float ResolveGroundCombinedWetPixelSoftening(
                float localShoreWetness,
                float riverbedWetness)
            {
                return ResolveGroundBoundedUnion(
                    ResolveGroundBoundedUnion(
                        saturate(_Wetness) *
                            saturate(_WetPixelSoftening),
                        localShoreWetness *
                            saturate(_GroundShoreHydrologyCharacterA.z)),
                    riverbedWetness *
                        saturate(_GroundRiverbedHydrologyCharacterA.z));
            }

            float ResolveGroundCombinedWetDarkening(
                float localShoreWetness,
                float riverbedWetness)
            {
                return ResolveGroundBoundedUnion(
                    ResolveGroundBoundedUnion(
                        saturate(_Wetness) *
                            saturate(_WetDarkenStrength) *
                            0.18,
                        localShoreWetness *
                            saturate(_GroundShoreHydrologyCharacterA.y)),
                    riverbedWetness *
                        saturate(_GroundRiverbedHydrologyCharacterA.y));
            }

            float ResolveGroundCombinedWetSmoothnessBoost(
                float localShoreWetness,
                float riverbedWetness)
            {
                return ResolveGroundBoundedUnion(
                    ResolveGroundBoundedUnion(
                        saturate(_Wetness) *
                            saturate(_WetSmoothnessBoost) *
                            0.22,
                        localShoreWetness *
                            saturate(_GroundShoreHydrologyCharacterA.w) *
                            (1.0 - saturate(
                                _GroundShoreWetHighlightShaping.z))),
                    riverbedWetness *
                        saturate(_GroundRiverbedHydrologyCharacterA.w) *
                        saturate(_GroundRiverbedWetSmoothnessResponse));
            }

            float ResolveGroundGlobalWetSpecularMultiplier()
            {
                return 1.0 + saturate(_Wetness) * 0.025;
            }

            float ResolveGroundLocalShoreWetSpecularBoost(
                float localShoreWetness)
            {
                return
                    saturate(localShoreWetness) *
                    saturate(_GroundShoreHydrologyCharacterB.x) *
                    (1.0 - saturate(
                        _GroundShoreWetHighlightShaping.z));
            }

            float ResolveGroundRiverbedWetSpecularBoost(
                float riverbedWetness)
            {
                return
                    saturate(riverbedWetness) *
                    saturate(_GroundRiverbedHydrologyCharacterB.x) *
                    saturate(_GroundRiverbedWetSpecularResponse);
            }

            float ResolveGroundSnowHydrologyRetention(
                float localShoreWetness,
                float riverbedWetness)
            {
                float shoreRetention = saturate(
                    1.0 -
                    localShoreWetness *
                    saturate(_GroundShoreHydrologyCharacterB.y));
                float riverbedRetention = saturate(
                    1.0 -
                    riverbedWetness *
                    saturate(_GroundRiverbedHydrologyCharacterB.y));
                return shoreRetention * riverbedRetention;
            }

            float ResolveGroundFrostHydrologyRetention(
                float localShoreWetness,
                float riverbedWetness)
            {
                float shoreRetention = saturate(
                    1.0 -
                    localShoreWetness *
                    saturate(_GroundShoreHydrologyCharacterB.z));
                float riverbedRetention = saturate(
                    1.0 -
                    riverbedWetness *
                    saturate(_GroundRiverbedHydrologyCharacterB.z));
                return shoreRetention * riverbedRetention;
            }

            float3 ResolveGroundSubstrateCompositionWeights(
                float bankMaterialBlend,
                float riverbedMaterialBlend)
            {
                float rawBank = saturate(bankMaterialBlend);
                float rawRiverbed = saturate(riverbedMaterialBlend);
                float rawTotal = rawBank + rawRiverbed;
                float secondaryCoverage = saturate(rawTotal);
                float normalization = rawTotal > 0.0001
                    ? secondaryCoverage / rawTotal
                    : 0.0;

                return saturate(
                    float3(
                        1.0 - secondaryCoverage,
                        rawBank * normalization,
                        rawRiverbed * normalization));
            }

            float3 ResolveGroundBankZoneWeights(
                float shoreMask,
                float bankDomain)
            {
                float softness = saturate(_GroundBankTransitionSoftness);
                float reach = saturate(_GroundBankMaterialReach);

                float broadCenter = lerp(0.285, 0.075, reach);
                float broadHalfWidth = lerp(0.012, 0.075, softness);
                float immediateHalfWidth = lerp(0.018, 0.065, softness);
                float waterlineHalfWidth = lerp(0.018, 0.050, softness);

                float broadBank = smoothstep(
                    broadCenter - broadHalfWidth,
                    broadCenter + broadHalfWidth,
                    saturate(shoreMask));
                float immediateBank = smoothstep(
                    0.235 - immediateHalfWidth,
                    0.235 + immediateHalfWidth,
                    saturate(shoreMask));
                float waterlineCore = smoothstep(
                    0.355 - waterlineHalfWidth,
                    0.355 + waterlineHalfWidth,
                    saturate(shoreMask));

                return saturate(
                    float3(
                        broadBank,
                        immediateBank,
                        waterlineCore) *
                    saturate(bankDomain));
            }

            float3 ResolveGroundBankZoneWeights(
                Varyings input)
            {
                return ResolveGroundBankZoneWeights(
                    ResolveGroundShoreMask(input),
                    ResolveGroundRiverBankDomain(input));
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

            float ResolveGroundComposedBankMaterialBlend(
                float3 zones,
                float outerContribution)
            {
                float broadContribution = zones.x * 0.65;
                float immediateContribution =
                    zones.y * saturate(_GroundImmediateBankExposure);
                float waterlineContribution =
                    zones.z * saturate(_GroundWaterlineMaterialStrength);
                float composedZones =
                    1.0 -
                    (1.0 - broadContribution) *
                    (1.0 - immediateContribution) *
                    (1.0 - waterlineContribution) *
                    (1.0 - saturate(outerContribution));

                return saturate(
                    saturate(_GroundBankLayerEnabled) *
                    saturate(_GroundBankMaterialStrength) *
                    composedZones);
            }

            float ResolveGroundBankMaterialBlend(
                Varyings input)
            {
                return ResolveGroundComposedBankMaterialBlend(
                    ResolveGroundBankZoneWeights(input),
                    ResolveGroundOuterBankExtensionBlend(input));
            }

            float ResolveGroundBankEdgeMaterialBlend(
                Varyings input)
            {
                return ResolveGroundComposedBankMaterialBlend(
                    ResolveGroundBankZoneWeights(
                        ResolveGroundShoreMask(input),
                        1.0),
                    0.0);
            }

            float ResolveGroundRiverbedMaterialTransitionWeight(
                Varyings input,
                float riverbedSupport)
            {
                float support = saturate(riverbedSupport);
                float transitionDistance =
                    max(0.0, _GroundRiverbedMaterialTransition.x);
                float transitionEnabled =
                    step(0.5, _GroundRiverbedLayerEnabled) *
                    step(0.0001, _GroundRiverbedMaterialStrength) *
                    step(0.0001, transitionDistance);
                if (support <= 0.0001 || transitionEnabled <= 0.5)
                {
                    return support;
                }

                float distance01 = saturate(
                    ResolveGroundRiverbedInwardDistance(input) /
                    max(0.0001, transitionDistance));
                float smoothDistance =
                    distance01 *
                    distance01 *
                    (3.0 - 2.0 * distance01);
                float interiorWeight = lerp(
                    distance01,
                    smoothDistance,
                    saturate(_GroundRiverbedMaterialTransition.y));
                return support * saturate(interiorWeight);
            }

            float ResolveGroundRiverbedMaterialTransitionActive(
                float riverbedSupport)
            {
                return saturate(riverbedSupport) *
                    step(0.5, _GroundRiverbedLayerEnabled) *
                    step(0.0001, _GroundRiverbedMaterialStrength) *
                    step(
                        0.0001,
                        max(0.0, _GroundRiverbedMaterialTransition.x));
            }

            float ResolveGroundRiverbedEdgeBankMaterialBlend(
                Varyings input,
                float riverbedSupport,
                float riverbedTransitionWeight)
            {
                float edgeWeight = saturate(
                    saturate(riverbedSupport) -
                    saturate(riverbedTransitionWeight));
                return saturate(
                    ResolveGroundBankEdgeMaterialBlend(input) *
                    edgeWeight);
            }

            float ResolveGroundRiverbedMaterialBlend(
                float riverbedTransitionWeight)
            {
                return saturate(
                    saturate(_GroundRiverbedLayerEnabled) *
                    saturate(_GroundRiverbedMaterialStrength) *
                    saturate(riverbedTransitionWeight));
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
