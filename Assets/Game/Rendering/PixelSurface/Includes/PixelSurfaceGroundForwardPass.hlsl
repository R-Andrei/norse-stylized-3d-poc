#ifndef PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL
#define PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL

            float ResolveGroundDirectionalStreakFeature(
                Varyings input,
                float exposureMask,
                float dampDepositMask,
                float rockyDryMask,
                float contractMask)
            {
                if (_GroundDirectionalStreakStrength <= 0.0001)
                {
                    return 0.0;
                }

                float2 direction = _GroundDirectionalStreakDirection.xy;

                if (dot(direction, direction) < 0.0001)
                {
                    direction = float2(1.0, 0.0);
                }

                direction = normalize(direction);
                float2 crossDirection = float2(-direction.y, direction.x);
                float2 positionXZ = input.positionWS.xz;
                float along = dot(positionXZ, direction);
                float across = dot(positionXZ, crossDirection);
                float scale = max(0.1, _GroundDirectionalStreakScale);
                float seed = _PixelSeed * 0.017 + _GroundDirectionalStreakSeed * 0.071;

                float lane = PS3D_ValueNoise31(
                    float3(
                        across / scale + seed,
                        along / (scale * 7.5) - seed * 0.37,
                        seed + 19.13));
                float scrape = PS3D_ValueNoise31(
                    float3(
                        across / (scale * 0.38) - seed * 0.23,
                        along / (scale * 14.0) + seed,
                        seed + 41.71));
                float combined = saturate(
                    lane * 0.78 +
                    scrape * 0.22);
                float contrast = lerp(1.15, 3.8, saturate(_GroundDirectionalStreakContrast));
                float signedFeature =
                    (combined - 0.5) * contrast * saturate(_GroundDirectionalStreakStrength);
                float semanticGate = saturate(
                    exposureMask * 0.68 +
                    rockyDryMask * 0.20 +
                    dampDepositMask * 0.12);
                float maskGate = lerp(
                    1.0,
                    semanticGate,
                    saturate(_GroundDirectionalStreakMaskInfluence));

                return clamp(
                    signedFeature * maskGate * contractMask,
                    -1.0,
                    1.0);
            }

            float ResolveGroundPooledWetnessFeature(
                Varyings input,
                float dampDepositMask,
                float rockyDryMask,
                float contractMask)
            {
                if (_GroundPooledWetnessStrength <= 0.0001)
                {
                    return 0.0;
                }

                float scale = max(0.1, _GroundPooledWetnessScale);
                float seed = _PixelSeed * 0.019 + _GroundPooledWetnessSeed * 0.083;
                float2 positionXZ = input.positionWS.xz;
                float broad = PS3D_ValueNoise31(
                    float3(
                        positionXZ.x / scale + seed,
                        positionXZ.y / scale - seed * 0.41,
                        seed + 61.37));
                float detail = PS3D_ValueNoise31(
                    float3(
                        positionXZ.x / (scale * 0.42) - seed * 0.17,
                        positionXZ.y / (scale * 0.42) + seed * 0.29,
                        seed + 83.11));
                float combined = saturate(broad * 0.74 + detail * 0.26);
                float contrast = lerp(0.65, 2.40, saturate(_GroundPooledWetnessContrast));
                float poolShape = saturate((combined - 0.48) * contrast + 0.5);
                float semanticGate = saturate(
                    dampDepositMask * 0.82 +
                    (1.0 - rockyDryMask) * 0.18);
                float maskGate = lerp(
                    1.0,
                    semanticGate,
                    saturate(_GroundPooledWetnessMaskInfluence));

                return saturate(
                    poolShape *
                    maskGate *
                    saturate(_GroundPooledWetnessStrength) *
                    contractMask);
            }

            float ResolveGroundTrampledWearFeature(
                Varyings input,
                float compactionMask,
                float dampDepositMask,
                float rockyDryMask,
                float contractMask)
            {
                if (_GroundTrampledWearStrength <= 0.0001)
                {
                    return 0.0;
                }

                float scale = max(0.1, _GroundTrampledWearScale);
                float seed = _PixelSeed * 0.023 + _GroundTrampledWearSeed * 0.097;
                float2 positionXZ = input.positionWS.xz;
                float broad = PS3D_ValueNoise31(
                    float3(
                        positionXZ.x / scale + seed,
                        positionXZ.y / (scale * 0.72) - seed * 0.37,
                        seed + 107.17));
                float scrape = PS3D_ValueNoise31(
                    float3(
                        positionXZ.x / (scale * 0.34) - seed * 0.21,
                        positionXZ.y / (scale * 1.85) + seed * 0.43,
                        seed + 139.43));
                float grit = PS3D_ValueNoise31(
                    float3(
                        positionXZ.x / (scale * 0.16) + seed * 0.61,
                        positionXZ.y / (scale * 0.20) - seed * 0.19,
                        seed + 173.89));
                float combined = saturate(
                    broad * 0.52 +
                    scrape * 0.34 +
                    grit * 0.14);
                float contrast = lerp(0.85, 3.20, saturate(_GroundTrampledWearContrast));
                float breakup = saturate((combined - 0.42) * contrast + 0.5);
                float semanticGate = saturate(
                    compactionMask * 0.90 +
                    dampDepositMask * 0.18 +
                    rockyDryMask * 0.12);
                float maskGate = lerp(
                    compactionMask,
                    semanticGate,
                    saturate(_GroundTrampledWearMaskInfluence));

                return saturate(
                    breakup *
                    maskGate *
                    saturate(_GroundTrampledWearStrength) *
                    contractMask);
            }

            PS3D_StylizedSurfaceDetail ResolveGroundBankLayerDetail(
                Varyings input,
                float substrateWeight)
            {
                PS3D_StylizedSurfaceDetail result =
                    PS3D_ZeroStylizedSurfaceDetail();
                [branch]
                if (_GroundBankLayerDetailA.x > 0.5 &&
                    substrateWeight > 0.0001)
                {
                    float2 detailUv =
                        input.positionWS.xz *
                        max(0.0001, _GroundBankLayerDetailA.z);
                    float4 packedSample = SAMPLE_TEXTURE2D_ARRAY(
                        _GroundBankLayerDetailArray,
                        sampler_GroundBankLayerDetailArray,
                        detailUv,
                        _GroundBankLayerDetailA.y);
                    result = PS3D_DecodeStylizedSurfaceDetail(
                        packedSample,
                        _GroundBankLayerDetailA,
                        _GroundBankLayerDetailB,
                        _GroundBankLayerDetailC);
                    [branch]
                    if (_GroundBankLayerAuthoredColorA.x > 0.5)
                    {
                        float4 authoredSample = SAMPLE_TEXTURE2D_ARRAY(
                            _GroundBankLayerAuthoredColorArray,
                            sampler_GroundBankLayerAuthoredColorArray,
                            detailUv,
                            _GroundBankLayerAuthoredColorA.y);
                        result = PS3D_AssignStylizedSurfaceTextureForm(
                            result,
                            authoredSample,
                            _GroundBankLayerAuthoredColorA);
                    }
                }

                // The material-uniform branch keeps the inward-distance
                // derivative coherent. The sampled B/A payload directly carries
                // the centre offset; feature-mask gating occurs afterwards.
                [branch]
                if (_GroundBankMaterialTransition.z > 0.0001 &&
                    _GroundBankLayerDetailC.z > 1.5)
                {
                    float wholeFeatureRetention =
                        ResolveGroundWholeFeatureRetention(
                            ResolveGroundRiverBankDomain(input),
                            ResolveGroundRiverBankInwardDistance(input),
                            input.positionWS,
                            result.featureCenterOffsetNormalized,
                            result.featureMaximumSupportRadiusUv,
                            _GroundBankLayerDetailA.z,
                            _GroundBankMaterialTransition);
                    result = PS3D_ApplyStylizedSurfaceFeatureRetention(
                        result,
                        lerp(
                            1.0,
                            wholeFeatureRetention,
                            step(0.001, result.featureMask)));
                }

                return result;
            }

            PS3D_StylizedSurfaceDetail ResolveGroundRiverbedLayerDetail(
                Varyings input,
                float substrateWeight)
            {
                PS3D_StylizedSurfaceDetail result =
                    PS3D_ZeroStylizedSurfaceDetail();
                [branch]
                if (_GroundRiverbedLayerDetailA.x > 0.5 &&
                    substrateWeight > 0.0001)
                {
                    float2 detailUv =
                        input.positionWS.xz *
                        max(0.0001, _GroundRiverbedLayerDetailA.z);
                    float4 packedSample = SAMPLE_TEXTURE2D_ARRAY(
                        _GroundRiverbedLayerDetailArray,
                        sampler_GroundRiverbedLayerDetailArray,
                        detailUv,
                        _GroundRiverbedLayerDetailA.y);
                    result = PS3D_DecodeStylizedSurfaceDetail(
                        packedSample,
                        _GroundRiverbedLayerDetailA,
                        _GroundRiverbedLayerDetailB,
                        _GroundRiverbedLayerDetailC);
                    [branch]
                    if (_GroundRiverbedLayerAuthoredColorA.x > 0.5)
                    {
                        float4 authoredSample = SAMPLE_TEXTURE2D_ARRAY(
                            _GroundRiverbedLayerAuthoredColorArray,
                            sampler_GroundRiverbedLayerAuthoredColorArray,
                            detailUv,
                            _GroundRiverbedLayerAuthoredColorA.y);
                        result = PS3D_AssignStylizedSurfaceTextureForm(
                            result,
                            authoredSample,
                            _GroundRiverbedLayerAuthoredColorA);
                    }
                }

                // See the Bank path: the only derivative is taken from the
                // coherent corridor distance field, not from sampled feature data.
                [branch]
                if (_GroundRiverbedMaterialTransition.z > 0.0001 &&
                    _GroundRiverbedLayerDetailC.z > 1.5)
                {
                    float wholeFeatureRetention =
                        ResolveGroundWholeFeatureRetention(
                            ResolveGroundRiverbedSupportMask(input),
                            ResolveGroundRiverbedInwardDistance(input),
                            input.positionWS,
                            result.featureCenterOffsetNormalized,
                            result.featureMaximumSupportRadiusUv,
                            _GroundRiverbedLayerDetailA.z,
                            _GroundRiverbedMaterialTransition);
                    result = PS3D_ApplyStylizedSurfaceFeatureRetention(
                        result,
                        lerp(
                            1.0,
                            wholeFeatureRetention,
                            step(0.001, result.featureMask)));
                }

                return result;
            }

            half3 ResolvePixelGroundSurfaceColor(
                Varyings input,
                float localShoreWetness,
                float riverbedWetness,
                float3 substrateWeights,
                float4 surfaceCoverRetention,
                PS3D_StylizedSurfaceDetail bankLayerDetail,
                PS3D_StylizedSurfaceDetail riverbedLayerDetail)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // One world-continuous XZ evaluator now owns both broad macro
                // composition and the low-frequency offset used by fine pixel cells.
                // It replaces the previous three-axis warp plus raw broad-noise lobe
                // without increasing the value-noise sample count.
                PS3D_GroundMacroRegionResult macroRegion =
                    PS3D_EvaluateGroundMacroRegion(input.positionWS);
                float3 pixelPositionWS =
                    input.positionWS +
                    float3(macroRegion.warp.x, 0.0, macroRegion.warp.y) *
                    _PixelCellSize *
                    _PixelWarpStrength;

                float pixelVariation;
                PixelCellVariation_float(
                    pixelPositionWS,
                    _PixelCellSize,
                    _PixelSeed,
                    _PixelToneCount,
                    _PixelClusterStrength,
                    pixelVariation);

                float broadValue = macroRegion.signedRegion;
                float contractMask =
                    1.0 -
                    step(
                        0.995,
                        min(
                            min((float)input.color.r, (float)input.color.g),
                            (float)input.color.b));
                float vertexVariation =
                    ((float)input.color.r - 0.5) * 2.0 * contractMask;
                float effectiveFrostStrength =
                    saturate(_FrostStrength) *
                    surfaceCoverRetention.z *
                    ResolveGroundFrostHydrologyRetention(
                        localShoreWetness,
                        riverbedWetness);
                float pixelProfileContrast =
                    max(0.0, _ProfilePixelContrast) *
                    (1.0 - ResolveGroundCombinedWetPixelSoftening(
                        localShoreWetness,
                        riverbedWetness)) *
                    lerp(1.0, max(0.0, _FrostContrast), effectiveFrostStrength) *
                    lerp(1.0, 0.25, saturate(_MonolithicFlatten));
                float fineTonalOffset =
                    (pixelVariation * _PixelVariation +
                     vertexVariation * _PixelVertexVariation) *
                    pixelProfileContrast *
                    _PixelEffectStrength;
                float macroTonalOffset =
                    broadValue *
                    PS3D_ResolveGroundMacroTonalAmplitude();
                half tonalScale =
                    (half)max(
                        0.0,
                        1.0 + fineTonalOffset + macroTonalOffset);

                float groundTonal = ResolveGroundTonalMask(input);
                float tonalSigned = (groundTonal - 0.5) * 2.0 * contractMask;
                float semanticPatch = saturate(
                    0.5 + tonalSigned * 0.44 + broadValue * 0.22);
                float inversePatch = 1.0 - semanticPatch;

                float exposureMask =
                    ResolveGroundExposureMask(input) * contractMask;
                float groundDampDeposit = ResolveGroundDampDepositMask(input);
                float groundShore = ResolveGroundShoreMask(input);
                float groundRockyDry = ResolveGroundRockyDryMask(input);
                float groundVegetation = ResolveGroundVegetationMask(input);
                float groundCompaction = ResolveGroundCompactionMask(input);
                float groundDampVisual = saturate(
                    groundDampDeposit *
                    0.84 *
                    max(0.0, _GroundDampResponse));
                float groundSnowVisual = saturate(
                    pow(saturate(exposureMask), 0.82) *
                    max(0.0, _GroundSnowResponse) *
                    (1.0 - groundDampVisual * 0.36) *
                    surfaceCoverRetention.y *
                    ResolveGroundSnowHydrologyRetention(
                        localShoreWetness,
                        riverbedWetness));
                float groundRockyDryVisual = saturate(
                    groundRockyDry * max(0.0, _GroundRockyDryResponse));
                float groundVegetationVisual = saturate(
                    groundVegetation *
                    max(0.0, _GroundVegetationResponse) *
                    surfaceCoverRetention.x);
                float directionalFeature = ResolveGroundDirectionalStreakFeature(
                    input,
                    exposureMask,
                    groundDampDeposit,
                    groundRockyDry,
                    contractMask);
                float pooledWetnessFeature = ResolveGroundPooledWetnessFeature(
                    input,
                    groundDampDeposit,
                    groundRockyDry,
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    groundCompaction,
                    groundDampDeposit,
                    groundRockyDry,
                    contractMask);
                float paintedAccentCoverage =
                    ResolveGroundPaintedAccentCoverage(input) *
                    surfaceCoverRetention.w *
                    contractMask *
                    saturate(_GroundPaintedAccentInkOpacity);


                float profileContrast =
                    max(0.0, _ProfileContrast) *
                    lerp(1.0, max(0.0, _FrostContrast), effectiveFrostStrength);
                float patchBlend =
                    saturate(_GroundPatchBlendStrength) * profileContrast;
                float snowPatch = saturate(
                    groundSnowVisual *
                    lerp(0.78, 1.18, semanticPatch) *
                    (1.0 - groundRockyDryVisual * 0.16));
                float dampPatch = saturate(
                    groundDampVisual * lerp(0.86, 1.18, inversePatch));

                float groundSemanticScale =
                    1.0 +
                    (snowPatch * (0.10 + _GroundSnowBrightness * 0.45) -
                     dampPatch * (0.14 + _GroundDampDarkenStrength * 0.26) -
                     groundRockyDryVisual * 0.045 +
                     groundVegetationVisual * 0.030 -
                     trampledWearFeature * 0.13 +
                     tonalSigned * 0.040 * _GroundPatchBlendStrength +
                     directionalFeature * 0.10) *
                    profileContrast;

                half3 ordinaryGroundAlbedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, groundSemanticScale);

                float bankLegacyPixelInfluence =
                    saturate(_GroundBankLayerDetailC.y);
                float bankLayerSignedVariation = clamp(
                    broadValue *
                        max(0.0, _GroundBankLayerMacroContrast) *
                        0.72 +
                    pixelVariation *
                        max(0.0, _GroundBankLayerPixelContrast) *
                        bankLegacyPixelInfluence *
                        0.20 +
                    vertexVariation *
                        max(0.0, _GroundBankLayerPixelContrast) *
                        bankLegacyPixelInfluence *
                        0.08,
                    -1.0,
                    1.0);
                half3 bankLayerPalette =
                    PS3D_ResolveStylizedSurfacePalette(
                        _GroundBankLayerBaseColor.rgb,
                        _GroundBankLayerDarkColor.rgb,
                        _GroundBankLayerLightColor.rgb,
                        _GroundBankLayerCavityColor.rgb,
                        bankLayerSignedVariation,
                        _GroundBankLayerDetailB.w,
                        bankLayerDetail);
                half3 bankLayerAlbedo =
                    baseSample.rgb * bankLayerPalette;

                float riverbedLegacyPixelInfluence =
                    saturate(_GroundRiverbedLayerDetailC.y);
                float riverbedLayerSignedVariation = clamp(
                    broadValue *
                        max(0.0, _GroundRiverbedLayerMacroContrast) *
                        0.72 +
                    pixelVariation *
                        max(0.0, _GroundRiverbedLayerPixelContrast) *
                        riverbedLegacyPixelInfluence *
                        0.20 +
                    vertexVariation *
                        max(0.0, _GroundRiverbedLayerPixelContrast) *
                        riverbedLegacyPixelInfluence *
                        0.08,
                    -1.0,
                    1.0);
                half3 riverbedLayerPalette =
                    PS3D_ResolveStylizedSurfacePalette(
                        _GroundRiverbedLayerBaseColor.rgb,
                        _GroundRiverbedLayerDarkColor.rgb,
                        _GroundRiverbedLayerLightColor.rgb,
                        _GroundRiverbedLayerCavityColor.rgb,
                        riverbedLayerSignedVariation,
                        _GroundRiverbedLayerDetailB.w,
                        riverbedLayerDetail);
                half3 riverbedLayerAlbedo =
                    baseSample.rgb * riverbedLayerPalette;
                half3 albedo =
                    ordinaryGroundAlbedo * (half)substrateWeights.x +
                    bankLayerAlbedo * (half)substrateWeights.y +
                    riverbedLayerAlbedo * (half)substrateWeights.z;

                // Separate snow value lift from snow hue. Ground Snow Brightness
                // now controls luminance, while Ground Snow Tint Strength controls
                // how much the lifted snow target adopts the frost/cold hue. This
                // makes the tint control perceptible even when the base snow color
                // is already pale.
                half3 snowValueTarget =
                    albedo *
                    (half)max(
                        0.0,
                        1.0 + saturate(_GroundSnowBrightness) * 0.78);
                half3 snowTintTarget = PS3D_ApplyValuePreservingTint(
                    snowValueTarget,
                    (half3)_FrostColor.rgb,
                    _GroundSnowTintStrength);
                albedo = lerp(
                    albedo,
                    snowTintTarget,
                    (half)(snowPatch * patchBlend * 0.76));

                half3 dampTarget = albedo * half3(0.78h, 0.76h, 0.69h);
                dampTarget = PS3D_ApplyValuePreservingTint(
                    dampTarget,
                    (half3)_GroundDampTint.rgb,
                    _GroundDampTintStrength);
                albedo = lerp(
                    albedo,
                    dampTarget,
                    (half)(dampPatch * saturate(_GroundDampDarkenStrength) * 0.92));

                half3 rockyDryTarget = albedo * half3(0.88h, 0.90h, 0.93h);
                rockyDryTarget = PS3D_ApplyValuePreservingTint(
                    rockyDryTarget,
                    (half3)_GroundRockyDryTint.rgb,
                    _GroundRockyDryTintStrength);
                albedo = lerp(
                    albedo,
                    rockyDryTarget,
                    (half)(groundRockyDryVisual * 0.18 * patchBlend));

                half3 vegetationTarget = albedo * half3(0.94h, 1.00h, 0.90h);
                vegetationTarget = PS3D_ApplyValuePreservingTint(
                    vegetationTarget,
                    (half3)_GroundVegetationTint.rgb,
                    _GroundVegetationTintStrength);
                albedo = lerp(
                    albedo,
                    vegetationTarget,
                    (half)(groundVegetationVisual * 0.14 * patchBlend));

                half3 featureTarget = PS3D_ApplyValuePreservingTint(
                    albedo * (half)max(0.0, 1.0 + directionalFeature * 0.16),
                    (half3)_FrostColor.rgb,
                    saturate(_GroundDirectionalStreakStrength) * 0.34);
                albedo = lerp(
                    albedo,
                    featureTarget,
                    (half)(abs(directionalFeature) * 0.35));

                half3 pooledWetnessTarget =
                    albedo *
                    (half)max(
                        0.0,
                        1.0 - pooledWetnessFeature *
                        (0.08 + saturate(_GroundPooledWetnessStrength) * 0.12));
                pooledWetnessTarget = PS3D_ApplyValuePreservingTint(
                    pooledWetnessTarget,
                    (half3)_GroundDampTint.rgb,
                    saturate(_GroundDampTintStrength + pooledWetnessFeature * 0.16));
                albedo = lerp(
                    albedo,
                    pooledWetnessTarget,
                    (half)(pooledWetnessFeature * 0.40));

                half3 trampledWearTarget =
                    albedo *
                    (half)max(
                        0.0,
                        1.0 - trampledWearFeature *
                        (0.16 + saturate(_GroundTrampledWearContrast) * 0.12));
                trampledWearTarget = PS3D_ApplyValuePreservingTint(
                    trampledWearTarget,
                    (half3)_GroundDampTint.rgb,
                    saturate(_GroundDampTintStrength + trampledWearFeature * 0.20));
                albedo = lerp(
                    albedo,
                    trampledWearTarget,
                    (half)(trampledWearFeature * 0.55));

                float exposureVisual =
                    pow(saturate(exposureMask), 0.72);
                float monolithicRelief =
                    broadValue * 0.028 +
                    exposureVisual * 0.078;
                half3 monolithicTarget =
                    _BaseColor.rgb * (half)max(0.0, 1.0 + monolithicRelief);
                albedo = lerp(
                    albedo,
                    monolithicTarget,
                    (half)saturate(_MonolithicFlatten));

                // V3J.4B: accepted projected glyph coverage is a flat albedo
                // layer. It introduces no normal displacement, relief, emission,
                // smoothness, or extra renderer. Ordinary ground lighting is
                // applied after this composition by the existing URP path.
                half inkBlend =
                    (half)saturate(
                        paintedAccentCoverage *
                        _GroundPaintedAccentInkColor.a);
                albedo = lerp(
                    albedo,
                    (half3)_GroundPaintedAccentInkColor.rgb,
                    inkBlend);

                float combinedWetDarkening =
                    ResolveGroundCombinedWetDarkening(
                        localShoreWetness,
                        riverbedWetness);
                albedo *=
                    (half)max(0.0, 1.0 - combinedWetDarkening);

                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    (half3)_GroundShoreHydrologyWetTintColor.rgb,
                    localShoreWetness *
                        saturate(_GroundShoreHydrologyCharacterA.x));
                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    (half3)_GroundRiverbedHydrologyWetTintColor.rgb,
                    riverbedWetness *
                        saturate(_GroundRiverbedHydrologyCharacterA.x));

                return albedo;
            }

            half3 ApplyGroundStylizedValueShaping(
                half3 albedo,
                Varyings input,
                half3 normalWS)
            {
                Light mainLight = GetMainLight();
                half litMask = saturate(dot(normalWS, mainLight.direction));
                half highlightMask = saturate(
                    (litMask - (half)_HighlightCompressStart) /
                    max(0.001h, 1.0h - (half)_HighlightCompressStart));
                half highlightScale =
                    1.0h -
                    highlightMask *
                    saturate((half)_HighlightCompressStrength);

                half bottomMask =
                    1.0h -
                    smoothstep(
                        0.0h,
                        max(0.001h, (half)_BottomDarkenHeight),
                        (half)max(0.0, input.positionOS.y));
                half sideMask = pow(
                    saturate(1.0h - abs(normalWS.y)),
                    max(0.5h, (half)_EdgeDarkenPower));
                half bottomDarken =
                    bottomMask *
                    saturate((half)_BottomDarkenStrength);
                half broadEdgeDarken =
                    bottomMask *
                    sideMask *
                    saturate((half)_EdgeDarkenStrength);

                float2 horizontalLightDirection = mainLight.direction.xz;
                float horizontalLightLengthSquared =
                    dot(
                        horizontalLightDirection,
                        horizontalLightDirection);
                horizontalLightDirection *=
                    rsqrt(max(0.0001, horizontalLightLengthSquared));

                float2 groundSlope =
                    normalWS.xz /
                    max(0.25, abs((float)normalWS.y));
                float reliefSignal =
                    clamp(
                        dot(groundSlope, horizontalLightDirection),
                        -1.0,
                        1.0) *
                    step(0.0001, horizontalLightLengthSquared);
                float reliefScale =
                    max(
                        0.0,
                        1.0 +
                        reliefSignal *
                        max(0.0, _GroundReliefShadingStrength));

                float relativeHeightScale =
                    max(
                        0.0,
                        1.0 +
                        input.positionOS.y *
                        max(0.0, _GroundRelativeHeightContrast));

                half valueScale =
                    highlightScale *
                    (1.0h - saturate(bottomDarken + broadEdgeDarken)) *
                    (half)reliefScale *
                    (half)relativeHeightScale;

                return albedo * valueScale;
            }

            half ResolveGroundProfileSmoothness(
                Varyings input,
                float localShoreWetness,
                float riverbedWetness,
                float3 substrateWeights,
                float4 surfaceCoverRetention,
                PS3D_StylizedSurfaceDetail bankLayerDetail,
                PS3D_StylizedSurfaceDetail riverbedLayerDetail)
            {
                float contractMask =
                    1.0 -
                    step(
                        0.995,
                        min(
                            min((float)input.color.r, (float)input.color.g),
                            (float)input.color.b));
                float effectiveFrostStrength =
                    saturate(_FrostStrength) *
                    surfaceCoverRetention.z *
                    ResolveGroundFrostHydrologyRetention(
                        localShoreWetness,
                        riverbedWetness);
                float pooledWetnessFeature = ResolveGroundPooledWetnessFeature(
                    input,
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    ResolveGroundCompactionMask(input),
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float paintedAccentLinesFeature = ResolveGroundPaintedAccentLinesFeature(
                    input,
                    ResolveGroundExposureMask(input) * contractMask,
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundVegetationMask(input),
                    ResolveGroundCompactionMask(input),
                    ResolveGroundShoreMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask,
                    surfaceCoverRetention.w);

                half ordinaryDrySmoothness = saturate(
                    (half)_Smoothness +
                    (half)pooledWetnessFeature * 0.025h -
                    (half)trampledWearFeature * 0.030h -
                    (half)paintedAccentLinesFeature * 0.012h +
                    (half)_MonolithicFlatten *
                    (half)_MonolithicSmoothnessBoost -
                    (half)effectiveFrostStrength * 0.06h);
                half bankDrySmoothness =
                    PS3D_ResolveStylizedSurfaceDrySmoothness(
                        (half)_GroundBankLayerDrySmoothness,
                        bankLayerDetail);
                half riverbedDrySmoothness =
                    PS3D_ResolveStylizedSurfaceDrySmoothness(
                        (half)_GroundRiverbedLayerDrySmoothness,
                        riverbedLayerDetail);
                half resolvedDrySmoothness =
                    ordinaryDrySmoothness * (half)substrateWeights.x +
                    bankDrySmoothness * (half)substrateWeights.y +
                    riverbedDrySmoothness * (half)substrateWeights.z;
                return saturate(
                    resolvedDrySmoothness +
                    (half)ResolveGroundCombinedWetSmoothnessBoost(
                        localShoreWetness,
                        riverbedWetness));
            }

            InputData BuildInputData(Varyings input, half3 normalWS)
            {
                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS =
                    SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                inputData.shadowCoord =
                    TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fogFactor;
                inputData.vertexLighting = half3(0.0h, 0.0h, 0.0h);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
                return inputData;
            }

            SurfaceData BuildSurfaceData(
                half3 albedo,
                Varyings input,
                float localShoreWetness,
                float riverbedWetness,
                float3 substrateWeights,
                float4 surfaceCoverRetention,
                PS3D_StylizedSurfaceDetail bankLayerDetail,
                PS3D_StylizedSurfaceDetail riverbedLayerDetail)
            {
                float contractMask =
                    1.0 -
                    step(
                        0.995,
                        min(
                            min((float)input.color.r, (float)input.color.g),
                            (float)input.color.b));
                float pooledWetnessFeature = ResolveGroundPooledWetnessFeature(
                    input,
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    ResolveGroundCompactionMask(input),
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float paintedAccentLinesFeature = ResolveGroundPaintedAccentLinesFeature(
                    input,
                    ResolveGroundExposureMask(input) * contractMask,
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundVegetationMask(input),
                    ResolveGroundCompactionMask(input),
                    ResolveGroundShoreMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask,
                    surfaceCoverRetention.w);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                half3 ordinaryDrySpecular =
                    (half3)_SpecularStrength *
                    lerp(
                        1.0h,
                        1.035h,
                        saturate((half)pooledWetnessFeature)) *
                    lerp(
                        1.0h,
                        1.10h,
                        saturate((half)_MonolithicFlatten)) *
                    lerp(
                        1.0h,
                        0.82h,
                        saturate((half)trampledWearFeature)) *
                    lerp(
                        1.0h,
                        0.94h,
                        saturate((half)paintedAccentLinesFeature));
                half3 bankDrySpecular =
                    (half3)_GroundBankLayerDrySpecularStrength *
                    (half)max(
                        0.0,
                        1.0 + bankLayerDetail.finishSigned * 0.5 -
                        bankLayerDetail.cavity * 0.18);
                half3 riverbedDrySpecular =
                    (half3)_GroundRiverbedLayerDrySpecularStrength *
                    (half)max(
                        0.0,
                        1.0 + riverbedLayerDetail.finishSigned * 0.5 -
                        riverbedLayerDetail.cavity * 0.18);
                half3 resolvedDrySpecular =
                    ordinaryDrySpecular * (half)substrateWeights.x +
                    bankDrySpecular * (half)substrateWeights.y +
                    riverbedDrySpecular * (half)substrateWeights.z;
                surfaceData.specular = saturate(
                    resolvedDrySpecular *
                        (half)ResolveGroundGlobalWetSpecularMultiplier() +
                    (half3)ResolveGroundLocalShoreWetSpecularBoost(
                        localShoreWetness) +
                    (half3)ResolveGroundRiverbedWetSpecularBoost(
                        riverbedWetness));
                surfaceData.metallic = 0.0h;
                surfaceData.smoothness = ResolveGroundProfileSmoothness(
                    input,
                    localShoreWetness,
                    riverbedWetness,
                    substrateWeights,
                    surfaceCoverRetention,
                    bankLayerDetail,
                    riverbedLayerDetail);
                surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
                surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
                surfaceData.occlusion = 1.0h;
                surfaceData.alpha = _BaseColor.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;
                return surfaceData;
            }

            half3 ResolveGroundStylizedShoreWetHighlight(
                Varyings input,
                InputData inputData,
                float localShoreWetness,
                half lightingLuma)
            {
                float wetness = saturate(localShoreWetness);
                float highlightWidth =
                    max(0.0, _GroundShoreWetHighlightBand.x);
                float highlightFeather =
                    max(0.005, _GroundShoreWetHighlightBand.y);
                float strength =
                    max(0.0, _GroundShoreWetHighlightShaping.x);
                float tightness =
                    saturate(_GroundShoreWetHighlightShaping.y);
                float cameraBias =
                    saturate(_GroundShoreWetHighlightShaping.z);
                float verticalFalloff =
                    saturate(_GroundShoreWetHighlightShaping.w);
                float bandMask =
                    ResolveGroundRiverBankDomain(input) *
                    (1.0 - smoothstep(
                        highlightWidth,
                        highlightWidth + highlightFeather,
                        ResolveGroundRiverBankDistance(input)));
                float activeStrength =
                    wetness * bandMask * strength * cameraBias;
                if (activeStrength <= 0.0001)
                {
                    return half3(0.0h, 0.0h, 0.0h);
                }

                Light mainLight = GetMainLight();
                float3 halfDirection = SafeNormalize(
                    (float3)mainLight.direction +
                    (float3)inputData.viewDirectionWS);
                float physicalExponent =
                    exp2(lerp(4.0, 8.0, tightness));
                float physicalLobe = pow(
                    saturate(dot(
                        (float3)inputData.normalWS,
                        halfDirection)),
                    physicalExponent);

                float verticalCenter = saturate(
                    1.0 -
                    abs(
                        inputData.normalizedScreenSpaceUV.y * 2.0 -
                        1.0));
                float cameraBand = pow(
                    verticalCenter,
                    lerp(0.75, 6.0, verticalFalloff));
                float lightFacing = saturate(dot(
                    (float3)inputData.normalWS,
                    (float3)mainLight.direction));
                float cameraLobe = cameraBand * lightFacing;
                float shapedLobe = lerp(
                    physicalLobe,
                    cameraLobe,
                    cameraBias);
                float visibility = saturate((float)lightingLuma);
                float amount =
                    activeStrength *
                    shapedLobe *
                    visibility;

                return (half3)mainLight.color * (half)amount;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half3 normalWS = normalize(input.normalWS);
                half flatNormalStrength =
                    saturate((half)_FlatNormalStrength);
                if (flatNormalStrength > 0.001h)
                {
                    half3 viewDirectionWS =
                        SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                    half3 flatNormalWS = normalize(
                        cross(
                            ddy(input.positionWS),
                            ddx(input.positionWS)));
                    flatNormalWS = faceforward(
                        flatNormalWS,
                        -viewDirectionWS,
                        flatNormalWS);
                    normalWS = normalize(
                        lerp(
                            normalWS,
                            flatNormalWS,
                            flatNormalStrength));
                }
                half3 debugColor = ResolveMaskDebugColor(input);
                if (debugColor.r >= 0.0h)
                {
                    return half4(debugColor, 1.0h);
                }

                float localShoreWetness =
                    ResolveGroundLocalShoreWetness(input);
                float bankMaterialBlend =
                    ResolveGroundBankMaterialBlend(input);
                float riverbedSupport =
                    ResolveGroundRiverbedSupportMask(input);
                float riverbedMaterialApplicationEnabled =
                    ResolveGroundRiverbedMaterialApplicationEnabled();
                float riverbedApplicationDomain =
                    ResolveGroundRiverbedApplicationDomain(input);
                float bankUnderRiverbedBlend =
                    ResolveGroundBankEdgeMaterialBlend(input) *
                    riverbedApplicationDomain *
                    riverbedMaterialApplicationEnabled;
                float resolvedBankMaterialBlend = max(
                    saturate(bankMaterialBlend),
                    saturate(bankUnderRiverbedBlend));
                float riverbedMaterialBlend =
                    ResolveGroundRiverbedMaterialBlend(
                        input,
                        riverbedSupport);
                float riverbedWetness =
                    ResolveGroundRiverbedWetness(input);
                float sameDrySurface =
                    ResolveGroundBankRiverbedSameDrySurface();
                float3 sequentialSubstrateWeights =
                    ResolveGroundSubstrateCompositionWeights(
                        resolvedBankMaterialBlend,
                        riverbedMaterialBlend);
                float sharedDrySurfaceBlend =
                    1.0 - sequentialSubstrateWeights.x;
                float bankDetailApplication = lerp(
                    resolvedBankMaterialBlend,
                    sharedDrySurfaceBlend,
                    sameDrySurface);
                PS3D_StylizedSurfaceDetail bankLayerDetail =
                    ResolveGroundBankLayerDetail(
                        input,
                        bankDetailApplication);
                PS3D_StylizedSurfaceDetail riverbedLayerDetail =
                    ResolveGroundRiverbedLayerDetail(
                        input,
                        riverbedMaterialBlend);
                float3 sharedSubstrateWeights = float3(
                    1.0 - sharedDrySurfaceBlend,
                    sharedDrySurfaceBlend,
                    0.0);
                float3 substrateWeights = lerp(
                    sequentialSubstrateWeights,
                    sharedSubstrateWeights,
                    sameDrySurface);
                float4 surfaceCoverRetention =
                    ResolveGroundBankCoverRetention(bankMaterialBlend) *
                    (1.0 - saturate(riverbedSupport));
                float2 combinedDetailSlope =
                    bankLayerDetail.slope * substrateWeights.y +
                    riverbedLayerDetail.slope * substrateWeights.z;
                if (dot(combinedDetailSlope, combinedDetailSlope) > 0.000001)
                {
                    normalWS = (half3)
                        PS3D_ApplyWorldXZStylizedSurfaceNormal(
                            (float3)normalWS,
                            combinedDetailSlope);
                }

                half3 albedo = ResolvePixelGroundSurfaceColor(
                    input,
                    localShoreWetness,
                    riverbedWetness,
                    substrateWeights,
                    surfaceCoverRetention,
                    bankLayerDetail,
                    riverbedLayerDetail);
                albedo = ApplyGroundStylizedValueShaping(
                    albedo,
                    input,
                    normalWS);

                InputData inputData = BuildInputData(input, normalWS);
                SurfaceData surfaceData = BuildSurfaceData(
                    albedo,
                    input,
                    localShoreWetness,
                    riverbedWetness,
                    substrateWeights,
                    surfaceCoverRetention,
                    bankLayerDetail,
                    riverbedLayerDetail);
                half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);

                half3 safeAlbedo = max(albedo, half3(0.001h, 0.001h, 0.001h));
                half3 pbrLightingRatio = pbrColor.rgb / safeAlbedo;
                half lightingLuma =
                    dot(
                        pbrLightingRatio,
                        half3(0.2126h, 0.7152h, 0.0722h));
                half3 neutralLitColor =
                    albedo * max(0.0h, lightingLuma);

                half lightingTintInfluence =
                    saturate((half)_LightingTintInfluence);
                half3 finalRgb =
                    lerp(
                        neutralLitColor,
                        pbrColor.rgb,
                        lightingTintInfluence);

                float textureFormBankWeight =
                    substrateWeights.y *
                    bankLayerDetail.textureFormStrength;
                float textureFormRiverbedWeight =
                    substrateWeights.z *
                    riverbedLayerDetail.textureFormStrength;
                float textureFormCoverage = saturate(
                    textureFormBankWeight + textureFormRiverbedWeight);
                float textureFormLightingResponse =
                    textureFormCoverage > 0.0001
                        ? saturate(
                            (textureFormBankWeight *
                                bankLayerDetail.sceneLightingResponse +
                             textureFormRiverbedWeight *
                                riverbedLayerDetail.sceneLightingResponse) /
                            max(0.0001,
                                textureFormBankWeight +
                                textureFormRiverbedWeight))
                        : 1.0;
                half3 positiveSpecularRemainder = max(
                    pbrColor.rgb -
                    albedo * max(0.0h, lightingLuma),
                    half3(0.0h, 0.0h, 0.0h));
                half textureFormDiffuseLighting = lerp(
                    1.0h,
                    max(0.0h, lightingLuma),
                    (half)textureFormLightingResponse);
                half3 textureFormPreservedColor =
                    albedo * textureFormDiffuseLighting +
                    positiveSpecularRemainder;
                finalRgb = lerp(
                    finalRgb,
                    textureFormPreservedColor,
                    (half)textureFormCoverage);

                finalRgb += ResolveGroundStylizedShoreWetHighlight(
                    input,
                    inputData,
                    localShoreWetness,
                    lightingLuma);

                finalRgb = MixFog(finalRgb, inputData.fogCoord);
                return half4(finalRgb, pbrColor.a);
            }
#endif // PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL
