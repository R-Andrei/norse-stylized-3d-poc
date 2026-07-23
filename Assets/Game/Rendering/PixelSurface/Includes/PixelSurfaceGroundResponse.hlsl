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
                float bankInwardDistance = max(
                    0.0,
                    (float)input.riverCoupledMasks.z);
                float riverbedSupport =
                    ResolveGroundRiverbedSupportMask(input);
                return saturate(
                    enabled *
                    step(0.0001, bankInwardDistance) *
                    (1.0 - riverbedSupport));
#else
                return 0.0;
#endif
            }

            float ResolveGroundRiverBankInwardDistance(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                return
                    ResolveGroundRiverCoupledEnabled() *
                    max(0.0, (float)input.riverCoupledMasks.z);
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

            float ResolveGroundRiverbedBoundaryBankInwardDistance(
                Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                float bankInwardDistance =
                    ResolveGroundRiverBankInwardDistance(input);
                float riverbedInwardDistance =
                    ResolveGroundRiverbedInwardDistance(input);
                float bankOutwardDistance =
                    ResolveGroundRiverBankDistance(input);
                return max(
                    0.0,
                    bankInwardDistance -
                    riverbedInwardDistance +
                    bankOutwardDistance);
#else
                return 0.0;
#endif
            }

            float ResolveGroundRiverbedBoundaryShoreMask()
            {
                // StylizedRiverCorridorGeometry.ResolveCorridorShoreInfluence
                // publishes pow(0.52, 1.32) at the exact BedSlope edge.
                return 0.42181733;
            }

            float ResolveGroundRiverbedApplicationDomain(Varyings input)
            {
#if defined(PS3D_GROUND_HAS_RIVERBED_SUPPORT)
                float riverbedSupport =
                    ResolveGroundRiverbedSupportMask(input);
                float riverbedInwardDistance =
                    ResolveGroundRiverbedInwardDistance(input);
                return
                    ResolveGroundRiverCoupledEnabled() *
                    step(
                        0.0001,
                        riverbedSupport + riverbedInwardDistance);
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

            float ResolveGroundBankRiverbedSameDrySurface()
            {
                return
                    step(0.5, _GroundBankLayerEnabled) *
                    step(0.5, _GroundRiverbedLayerEnabled) *
                    step(0.5, _GroundBankRiverbedSameDrySurface);
            }

            float ResolveGroundSurfaceApplicationTransition(
                float regionWeight,
                float inwardDistance,
                float4 transitionSettings)
            {
                float region = saturate(regionWeight);
                float transitionDistance = max(0.0, transitionSettings.x);
                if (region <= 0.0001 || transitionDistance <= 0.0001)
                {
                    return region;
                }

                float distance01 = saturate(
                    max(0.0, inwardDistance) /
                    max(0.0001, transitionDistance));
                float smoothDistance =
                    distance01 *
                    distance01 *
                    (3.0 - 2.0 * distance01);
                float transitionWeight = lerp(
                    distance01,
                    smoothDistance,
                    saturate(transitionSettings.y));
                return region * saturate(transitionWeight);
            }

            float2 ResolveGroundWorldXZScalarGradient(
                float scalarValue,
                float3 positionWS,
                out float valid)
            {
                float2 positionDx = ddx(positionWS.xz);
                float2 positionDy = ddy(positionWS.xz);
                float scalarDx = ddx(scalarValue);
                float scalarDy = ddy(scalarValue);
                float determinant =
                    positionDx.x * positionDy.y -
                    positionDx.y * positionDy.x;
                float determinantMagnitude = abs(determinant);
                valid = step(1e-8, determinantMagnitude);
                float safeDeterminant =
                    determinant >= 0.0
                        ? max(determinant, 1e-8)
                        : min(determinant, -1e-8);
                return float2(
                    (scalarDx * positionDy.y -
                        scalarDy * positionDx.y) /
                        safeDeterminant,
                    (positionDx.x * scalarDy -
                        positionDy.x * scalarDx) /
                        safeDeterminant);
            }

            float ResolveGroundWholeFeatureRetention(
                float regionWeight,
                float inwardDistance,
                float3 positionWS,
                float2 featureCenterOffsetNormalized,
                float maximumSupportRadiusUv,
                float detailUvScale,
                float4 transitionSettings)
            {
                float safetyMargin = max(0.0, transitionSettings.z);
                float fadeDistance = max(0.0, transitionSettings.w);
                float safeUvScale = max(0.0001, detailUvScale);
                float conservativeSupportRadius =
                    max(0.0, maximumSupportRadiusUv) / safeUvScale;
                float metadataEnabled = step(
                    1e-5,
                    conservativeSupportRadius);
                float enabled =
                    step(0.0001, safetyMargin) *
                    step(0.0001, regionWeight) *
                    metadataEnabled;

                float2 centreOffset =
                    featureCenterOffsetNormalized *
                    conservativeSupportRadius;

                float inwardGradientValid;
                float2 inwardGradient =
                    ResolveGroundWorldXZScalarGradient(
                        inwardDistance,
                        positionWS,
                        inwardGradientValid);
                float centreInwardDistance =
                    inwardDistance - dot(inwardGradient, centreOffset);

                float anchorDistanceSquared = dot(
                    featureCenterOffsetNormalized,
                    featureCenterOffsetNormalized);
                float payloadValid =
                    inwardGradientValid *
                    step(1e-5, conservativeSupportRadius) *
                    step(anchorDistanceSquared, 1.0001);

                float reconstructedEdgeDistance =
                    centreInwardDistance - conservativeSupportRadius;
                // Invalid payload/derivative reconstruction is rejected
                // conservatively. The algorithm-10 proof requires zero invalid
                // accepted feature samples before installation.
                float featureEdgeDistance = lerp(
                    -1000000.0,
                    reconstructedEdgeDistance,
                    payloadValid);
                float hardRetention = step(
                    safetyMargin,
                    featureEdgeDistance);
                float softRetention = smoothstep(
                    safetyMargin,
                    safetyMargin + max(0.0001, fadeDistance),
                    featureEdgeDistance);
                float retention = lerp(
                    hardRetention,
                    softRetention,
                    step(0.0001, fadeDistance));
                return lerp(1.0, retention, enabled);
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
                float bankApplication = saturate(bankMaterialBlend);
                float riverbedApplication = saturate(riverbedMaterialBlend);
                float rawSecondaryTotal =
                    bankApplication + riverbedApplication;
                float secondaryCoverage = saturate(rawSecondaryTotal);
                float secondaryNormalization =
                    rawSecondaryTotal > 0.0
                        ? secondaryCoverage / rawSecondaryTotal
                        : 0.0;

                return float3(
                    1.0 - secondaryCoverage,
                    bankApplication * secondaryNormalization,
                    riverbedApplication * secondaryNormalization);
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

            float ResolveGroundOuterBankBoundaryContribution()
            {
                return
                    step(
                        0.0001,
                        max(0.0, _GroundOuterBankExtension)) *
                    saturate(_GroundOuterBankStrength);
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
                float rawBankBlend = ResolveGroundComposedBankMaterialBlend(
                    ResolveGroundBankZoneWeights(input),
                    ResolveGroundOuterBankExtensionBlend(input));
                float applicationTransition =
                    ResolveGroundSurfaceApplicationTransition(
                        ResolveGroundRiverBankDomain(input),
                        ResolveGroundRiverBankInwardDistance(input),
                        _GroundBankMaterialTransition);
                return saturate(rawBankBlend * applicationTransition);
            }

            float ResolveGroundBankEdgeMaterialBlend(
                Varyings input)
            {
                float rawBankBlend = ResolveGroundComposedBankMaterialBlend(
                    ResolveGroundBankZoneWeights(
                        ResolveGroundRiverbedBoundaryShoreMask(),
                        1.0),
                    ResolveGroundOuterBankBoundaryContribution());
                float applicationTransition =
                    ResolveGroundSurfaceApplicationTransition(
                        1.0,
                        ResolveGroundRiverbedBoundaryBankInwardDistance(
                            input),
                        _GroundBankMaterialTransition);
                return saturate(rawBankBlend * applicationTransition);
            }

            float ResolveGroundRiverbedMaterialApplicationEnabled()
            {
                return
                    step(0.5, _GroundRiverbedLayerEnabled) *
                    step(0.0001, _GroundRiverbedMaterialStrength);
            }

            float ResolveGroundRiverbedMaterialBlend(
                Varyings input,
                float riverbedSupport)
            {
                float applicationTransition =
                    ResolveGroundSurfaceApplicationTransition(
                        riverbedSupport,
                        ResolveGroundRiverbedInwardDistance(input),
                        _GroundRiverbedMaterialTransition);
                return saturate(
                    ResolveGroundRiverbedMaterialApplicationEnabled() *
                    saturate(_GroundRiverbedMaterialStrength) *
                    applicationTransition);
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
