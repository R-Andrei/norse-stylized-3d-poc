#ifndef PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL
#define PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL

            bool PS3D_IsGroundFeatureMode(float expectedMode)
            {
                return abs(_GroundFeatureMode - expectedMode) < 0.25 &&
                    _GroundFeatureStrength > 0.0001;
            }

            float ResolveGroundDirectionalStreakFeature(
                Varyings input,
                float exposureMask,
                float dampDepositMask,
                float rockyDryMask,
                float contractMask)
            {
                if (!PS3D_IsGroundFeatureMode(1.0))
                {
                    return 0.0;
                }

                float2 direction = _GroundFeatureDirection.xy;

                if (dot(direction, direction) < 0.0001)
                {
                    direction = float2(1.0, 0.0);
                }

                direction = normalize(direction);
                float2 crossDirection = float2(-direction.y, direction.x);
                float2 positionXZ = input.positionWS.xz;
                float along = dot(positionXZ, direction);
                float across = dot(positionXZ, crossDirection);
                float scale = max(0.1, _GroundFeatureScale);
                float seed = _PixelSeed * 0.017 + _GroundFeatureSeed * 0.071;

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
                float contrast = lerp(1.15, 3.8, saturate(_GroundFeatureContrast));
                float signedFeature =
                    (combined - 0.5) * contrast * saturate(_GroundFeatureStrength);
                float semanticGate = saturate(
                    exposureMask * 0.68 +
                    rockyDryMask * 0.20 +
                    dampDepositMask * 0.12);
                float maskGate = lerp(
                    1.0,
                    semanticGate,
                    saturate(_GroundFeatureMaskInfluence));

                return clamp(
                    signedFeature * maskGate * contractMask,
                    -1.0,
                    1.0);
            }

            float ResolveGroundPooledWetnessFeature(
                Varyings input,
                float dampDepositMask,
                float shoreMask,
                float rockyDryMask,
                float contractMask)
            {
                if (!PS3D_IsGroundFeatureMode(2.0))
                {
                    return 0.0;
                }

                float scale = max(0.1, _GroundFeatureScale);
                float seed = _PixelSeed * 0.019 + _GroundFeatureSeed * 0.083;
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
                float contrast = lerp(0.65, 2.40, saturate(_GroundFeatureContrast));
                float poolShape = saturate((combined - 0.48) * contrast + 0.5);
                float semanticGate = saturate(
                    dampDepositMask * 0.70 +
                    shoreMask * 0.45 +
                    (1.0 - rockyDryMask) * 0.18);
                float maskGate = lerp(
                    1.0,
                    semanticGate,
                    saturate(_GroundFeatureMaskInfluence));

                return saturate(
                    poolShape *
                    maskGate *
                    saturate(_GroundFeatureStrength) *
                    contractMask);
            }

            float ResolveGroundTrampledWearFeature(
                Varyings input,
                float compactionMask,
                float dampDepositMask,
                float rockyDryMask,
                float contractMask)
            {
                if (!PS3D_IsGroundFeatureMode(3.0))
                {
                    return 0.0;
                }

                float scale = max(0.1, _GroundFeatureScale);
                float seed = _PixelSeed * 0.023 + _GroundFeatureSeed * 0.097;
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
                float contrast = lerp(0.85, 3.20, saturate(_GroundFeatureContrast));
                float breakup = saturate((combined - 0.42) * contrast + 0.5);
                float semanticGate = saturate(
                    compactionMask * 0.90 +
                    dampDepositMask * 0.18 +
                    rockyDryMask * 0.12);
                float maskGate = lerp(
                    compactionMask,
                    semanticGate,
                    saturate(_GroundFeatureMaskInfluence));

                return saturate(
                    breakup *
                    maskGate *
                    saturate(_GroundFeatureStrength) *
                    contractMask);
            }

            half3 ResolvePixelGroundSurfaceColor(Varyings input)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                // Ground macro variation must live at terrain scale, not pixel-cell
                // scale. The previous _PixelCellSize * 8 path produced ~0.44 m
                // patches with the snowfield material, which read as repeated
                // granular mottling from the isometric camera.
                float broadCellSize = max(_GroundMacroPatchScale, 0.0001);
                float3 broadCoordinate =
                    input.positionWS / broadCellSize + _PixelSeed * 0.013;
                float3 warp =
                    float3(
                        PS3D_ValueNoise31(broadCoordinate + 11.17),
                        PS3D_ValueNoise31(broadCoordinate + 23.31),
                        PS3D_ValueNoise31(broadCoordinate + 37.47)) *
                    2.0 -
                    1.0;
                float3 pixelPositionWS =
                    input.positionWS +
                    warp * _PixelCellSize * _PixelWarpStrength;

                float pixelVariation;
                PixelCellVariation_float(
                    pixelPositionWS,
                    _PixelCellSize,
                    _PixelSeed,
                    _PixelToneCount,
                    _PixelClusterStrength,
                    pixelVariation);

                float broadValue =
                    PS3D_ValueNoise31(broadCoordinate + 53.29) * 2.0 - 1.0;
                float contractMask =
                    1.0 -
                    step(
                        0.995,
                        min(
                            min((float)input.color.r, (float)input.color.g),
                            (float)input.color.b));
                float vertexVariation =
                    ((float)input.color.r - 0.5) * 2.0 * contractMask;
                float pixelProfileContrast =
                    max(0.0, _ProfilePixelContrast) *
                    lerp(1.0, 1.0 - saturate(_WetPixelSoftening), saturate(_Wetness)) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength)) *
                    lerp(1.0, 0.25, saturate(_MonolithicFlatten));
                float tonalOffset =
                    (pixelVariation * _PixelVariation +
                     vertexVariation * _PixelVertexVariation +
                     broadValue * _PixelBroadVariation) *
                    pixelProfileContrast;
                half tonalScale =
                    (half)max(0.0, 1.0 + tonalOffset * _PixelEffectStrength);

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
                    (groundDampDeposit * 0.84 +
                     groundShore * 0.34 * max(0.0, _GroundShoreDampStrength)) *
                    max(0.0, _GroundDampResponse));
                float groundSnowVisual = saturate(
                    pow(saturate(exposureMask), 0.82) *
                    max(0.0, _GroundSnowResponse) *
                    (1.0 - groundDampVisual * 0.36));
                float groundRockyDryVisual = saturate(
                    groundRockyDry * max(0.0, _GroundRockyDryResponse));
                float groundVegetationVisual = saturate(
                    groundVegetation * max(0.0, _GroundVegetationResponse));
                float directionalFeature = ResolveGroundDirectionalStreakFeature(
                    input,
                    exposureMask,
                    groundDampDeposit,
                    groundRockyDry,
                    contractMask);
                float pooledWetnessFeature = ResolveGroundPooledWetnessFeature(
                    input,
                    groundDampDeposit,
                    groundShore,
                    groundRockyDry,
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    groundCompaction,
                    groundDampDeposit,
                    groundRockyDry,
                    contractMask);
                float profileContrast =
                    max(0.0, _ProfileContrast) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength));
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

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, groundSemanticScale);

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
                    saturate(_GroundFeatureStrength) * 0.34);
                albedo = lerp(
                    albedo,
                    featureTarget,
                    (half)(abs(directionalFeature) * 0.35));

                half3 pooledWetnessTarget =
                    albedo *
                    (half)max(
                        0.0,
                        1.0 - pooledWetnessFeature *
                        (0.08 + saturate(_GroundFeatureStrength) * 0.12));
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
                        (0.16 + saturate(_GroundFeatureContrast) * 0.12));
                trampledWearTarget = PS3D_ApplyValuePreservingTint(
                    trampledWearTarget,
                    (half3)_GroundDampTint.rgb,
                    saturate(_GroundDampTintStrength + trampledWearFeature * 0.20));
                albedo = lerp(
                    albedo,
                    trampledWearTarget,
                    (half)(trampledWearFeature * 0.55));

                float wetness = saturate(_Wetness);
                float wetGlobalDarken =
                    wetness * saturate(_WetDarkenStrength) * 0.18;
                albedo *= (half)max(0.0, 1.0 - wetGlobalDarken);

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
                half valueScale =
                    highlightScale *
                    (1.0h - saturate(bottomDarken + broadEdgeDarken));

                return albedo * valueScale;
            }

            half ResolveGroundProfileSmoothness(Varyings input)
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
                    ResolveGroundShoreMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    ResolveGroundCompactionMask(input),
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);

                return saturate(
                    (half)_Smoothness +
                    (half)_Wetness * (half)_WetSmoothnessBoost * 0.22h +
                    (half)pooledWetnessFeature * 0.025h -
                    (half)trampledWearFeature * 0.030h +
                    (half)_MonolithicFlatten *
                    (half)_MonolithicSmoothnessBoost -
                    (half)_FrostStrength * 0.06h);
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

            SurfaceData BuildSurfaceData(half3 albedo, Varyings input)
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
                    ResolveGroundShoreMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);
                float trampledWearFeature = ResolveGroundTrampledWearFeature(
                    input,
                    ResolveGroundCompactionMask(input),
                    ResolveGroundDampDepositMask(input),
                    ResolveGroundRockyDryMask(input),
                    contractMask);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.specular =
                    (half3)_SpecularStrength *
                    lerp(
                        1.0h,
                        1.025h,
                        saturate((half)_Wetness)) *
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
                        saturate((half)trampledWearFeature));
                surfaceData.metallic = 0.0h;
                surfaceData.smoothness = ResolveGroundProfileSmoothness(input);
                surfaceData.normalTS = half3(0.0h, 0.0h, 1.0h);
                surfaceData.emission = half3(0.0h, 0.0h, 0.0h);
                surfaceData.occlusion = 1.0h;
                surfaceData.alpha = _BaseColor.a;
                surfaceData.clearCoatMask = 0.0h;
                surfaceData.clearCoatSmoothness = 0.0h;
                return surfaceData;
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

                half3 albedo = ResolvePixelGroundSurfaceColor(input);
                albedo = ApplyGroundStylizedValueShaping(
                    albedo,
                    input,
                    normalWS);

                InputData inputData = BuildInputData(input, normalWS);
                SurfaceData surfaceData = BuildSurfaceData(albedo, input);
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

                finalRgb = MixFog(finalRgb, inputData.fogCoord);
                return half4(finalRgb, pbrColor.a);
            }
#endif // PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL
