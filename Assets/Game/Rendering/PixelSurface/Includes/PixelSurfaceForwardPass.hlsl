#ifndef PS3D_PIXELSURFACEFORWARDPASS_HLSL
#define PS3D_PIXELSURFACEFORWARDPASS_HLSL

            half3 ResolvePixelSurfaceColor(Varyings input)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);

                float broadCellSize = max(_PixelCellSize * 8.0, 0.0001);
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

                float isGroundSurface = ResolveSurfaceContractIsGround();
                float exposureMask =
                    saturate((float)input.color.g) * contractMask;
                float massCreviceMask =
                    ResolveShaderCreviceBaseMask(input) * contractMask;
                float creviceMask =
                    lerp(massCreviceMask, 0.0, isGroundSurface);
                float massDirtDepositMask =
                    ResolveShaderDirtDepositMask(input) * contractMask;
                float dirtDepositMask =
                    lerp(massDirtDepositMask, 0.0, isGroundSurface);
                float baseMask = creviceMask * (1.0 - exposureMask);
                float groundDampDeposit = ResolveGroundDampDepositMask(input);
                float groundShore = ResolveGroundShoreMask(input);
                float groundRockyDry = ResolveGroundRockyDryMask(input);
                float groundVegetation = ResolveGroundVegetationMask(input);
                float groundDampVisual = saturate(
                    (groundDampDeposit * 0.78 +
                     groundShore * 0.52 * max(0.0, _GroundShoreDampStrength)) *
                    max(0.0, _GroundDampResponse));
                float groundSnowVisual = saturate(
                    exposureMask * max(0.0, _GroundSnowResponse) *
                    (1.0 - groundDampVisual * 0.42));
                float groundRockyDryVisual = saturate(
                    groundRockyDry * max(0.0, _GroundRockyDryResponse));
                float groundVegetationVisual = saturate(
                    groundVegetation * max(0.0, _GroundVegetationResponse));
                float profileContrast =
                    max(0.0, _ProfileContrast) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength));
                float generatedMassExposureResponse =
                    max(0.0, _GeneratedMassExposureResponse);
                float generatedMassCreviceResponse =
                    max(0.0, _GeneratedMassCreviceResponse);
                float generatedMassBaseResponse =
                    max(0.0, _GeneratedMassBaseResponse);
                float generatedMassDirtDepositResponse =
                    max(0.0, _GeneratedMassDirtDepositResponse);

                float wetness = saturate(_Wetness);
                float frostStrength = saturate(_FrostStrength);
                float monolithicFlatten = saturate(_MonolithicFlatten);
                float generatedMassSurface = 1.0 - isGroundSurface;

                float exposureVisual =
                    pow(saturate(exposureMask), 0.72);
                float creviceVisual =
                    pow(saturate(creviceMask), 0.58) *
                    (1.0 - exposureVisual * 0.22);
                float baseVisual =
                    pow(saturate(baseMask), 0.78) *
                    (1.0 - exposureVisual * 0.18);
                float dirtDepositVisual =
                    pow(saturate(dirtDepositMask), 0.70);

                // Exposure remains the only generated-mass mask that primarily
                // shifts the pre-layer value scale. Crevice, base, and dirt are
                // handled below as independent material layers so their response
                // controls do not collapse into one shared lower-region multiplier.
                float generatedMassSemanticScale =
                    1.0 +
                    exposureVisual *
                    _ExposureTintStrength *
                    1.72 *
                    generatedMassExposureResponse *
                    profileContrast;
                float groundSemanticScale =
                    1.0 +
                    (groundSnowVisual * 0.11 -
                     groundDampVisual * 0.18 -
                     groundRockyDryVisual * 0.035 +
                     groundVegetationVisual * 0.025) *
                    profileContrast;
                float semanticScale = lerp(
                    generatedMassSemanticScale,
                    groundSemanticScale,
                    isGroundSurface);

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, semanticScale);

                half3 groundAlbedo = albedo;
                groundAlbedo = lerp(
                    groundAlbedo,
                    _FrostColor.rgb,
                    (half)(groundSnowVisual * 0.34));
                groundAlbedo *=
                    (half)max(0.0, 1.0 - groundDampVisual * 0.24);
                groundAlbedo = lerp(
                    groundAlbedo,
                    groundAlbedo * half3(0.88h, 0.90h, 0.93h),
                    (half)(groundRockyDryVisual * 0.22));
                groundAlbedo = lerp(
                    groundAlbedo,
                    groundAlbedo * half3(0.94h, 1.00h, 0.90h),
                    (half)(groundVegetationVisual * 0.18));
                albedo = lerp(
                    albedo,
                    groundAlbedo,
                    (half)isGroundSurface);

                albedo = ApplyGeneratedMassSurfaceMottle(
                    albedo,
                    input,
                    generatedMassSurface,
                    exposureVisual,
                    creviceVisual,
                    baseVisual,
                    dirtDepositVisual,
                    wetness,
                    frostStrength,
                    monolithicFlatten);

                half3 exposureTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        albedo,
                        _GeneratedMassExposureTint.rgb,
                        _GeneratedMassExposureTintStrength);
                float exposureTintOpacity =
                    exposureVisual *
                    generatedMassExposureResponse *
                    generatedMassSurface *
                    saturate(_GeneratedMassExposureTintStrength);
                albedo = lerp(
                    albedo,
                    exposureTintTarget,
                    (half)saturate(exposureTintOpacity));

                // Dedicated crevice layer: profile-aware depth/occlusion.
                // Response 1.0 is intentionally stronger than H2L response 2.0.
                half3 creviceNeutralTarget =
                    albedo *
                    (half)lerp(0.48, 0.38, wetness);
                half3 creviceTarget =
                    PS3D_ApplyValuePreservingTint(
                        creviceNeutralTarget,
                        _GeneratedMassCreviceTint.rgb,
                        _GeneratedMassCreviceTintStrength);
                creviceTarget = lerp(
                    creviceTarget,
                    _BaseColor.rgb * (half)0.46,
                    (half)(monolithicFlatten * 0.82));
                float creviceOpacity =
                    (1.0 - exp2(
                        -creviceVisual *
                        (2.80 +
                         _CreviceDarkenStrength * 14.50 +
                         wetness * 1.05 +
                         frostStrength * 0.62) *
                        generatedMassCreviceResponse *
                        profileContrast)) *
                    generatedMassSurface *
                    lerp(1.0, 0.72, frostStrength) *
                    lerp(1.0, 0.66, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    creviceTarget,
                    (half)saturate(creviceOpacity));

                // Dedicated base/contact layer: broader grounding, less deep
                // than crevice, controlled only by Base Response.
                half3 baseNeutralTarget =
                    albedo *
                    (half)lerp(0.70, 0.62, wetness);
                half3 baseTarget =
                    PS3D_ApplyValuePreservingTint(
                        baseNeutralTarget,
                        _GeneratedMassBaseTint.rgb,
                        _GeneratedMassBaseTintStrength);
                baseTarget = lerp(
                    baseTarget,
                    _BaseColor.rgb * (half)0.62,
                    (half)(monolithicFlatten * 0.70));
                float baseOpacity =
                    (1.0 - exp2(
                        -baseVisual *
                        (1.25 +
                         _BaseDarkenStrength * 9.50 +
                         wetness * 0.42) *
                        generatedMassBaseResponse *
                        profileContrast)) *
                    generatedMassSurface *
                    lerp(1.0, 0.70, frostStrength) *
                    lerp(1.0, 0.62, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    baseTarget,
                    (half)saturate(baseOpacity));

                // Dedicated dirt/deposit layer. Response 1.0 is calibrated to
                // land near the previous H2L response 2.0 visual strength, but
                // the exponential opacity curve keeps high values from becoming
                // flat paint too quickly.
                half3 dirtNeutralTarget =
                    albedo *
                    (half)lerp(0.88, 0.70, wetness);
                half3 dirtTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        dirtNeutralTarget,
                        _GeneratedMassDirtDepositTint.rgb,
                        _GeneratedMassDirtDepositTintStrength);
                half3 dirtTarget = lerp(
                    dirtTintTarget,
                    dirtTintTarget *
                        (half)lerp(0.92, 0.62, saturate(_WetDarkenStrength)),
                    (half)wetness);
                float dirtOpacity =
                    (1.0 - exp2(
                        -dirtDepositVisual *
                        (1.75 + saturate(_StoneDirtResponse) * 2.65) *
                        generatedMassDirtDepositResponse *
                        lerp(1.0, 1.36, wetness))) *
                    generatedMassSurface *
                    lerp(1.0, 0.18, frostStrength) *
                    lerp(1.0, 0.09, monolithicFlatten);
                albedo = lerp(
                    albedo,
                    dirtTarget,
                    (half)saturate(dirtOpacity));

                float dampGatherMask =
                    saturate(
                        dirtDepositVisual * generatedMassDirtDepositResponse * 0.82 +
                        baseVisual * generatedMassBaseResponse * 0.20 +
                        creviceVisual * generatedMassCreviceResponse * 0.14 -
                        exposureVisual * generatedMassExposureResponse * 0.16);
                half3 wetDampNeutralTarget =
                    albedo *
                    (half)lerp(0.88, 0.58, saturate(_WetDarkenStrength));
                half3 wetDampTarget =
                    PS3D_ApplyValuePreservingTint(
                        wetDampNeutralTarget,
                        _GeneratedMassDirtDepositTint.rgb,
                        _GeneratedMassDirtDepositTintStrength);
                float wetDampStrength =
                    dampGatherMask *
                    wetness *
                    saturate(_WetDarkenStrength * 1.65) *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    wetDampTarget,
                    (half)saturate(wetDampStrength));


                float frostNoise =
                    PS3D_ValueNoise31(broadCoordinate * 1.7 + 71.31);
                float frostPattern =
                    saturate(
                        (frostNoise - (1.0 - saturate(_FrostCoverage))) /
                        max(0.001, saturate(_FrostCoverage)));
                float frostPatternSoft =
                    smoothstep(0.12, 0.88, frostPattern);

                // Patch 13B: frost should read as a coherent pale material
                // layer, not as a high-contrast triangle/facet visualizer.
                // Keep exposure important, but soften its authority and use the
                // procedural frost field as a low-frequency breakup term.
                float frostExposure =
                    saturate(exposureVisual * 0.72 + broadValue * 0.08);
                float frostMask =
                    saturate(
                        frostExposure * (0.84 * generatedMassExposureResponse) +
                        frostPatternSoft * 0.22 -
                        creviceVisual * (0.10 * generatedMassCreviceResponse) -
                        dirtDepositVisual *
                            (0.10 * generatedMassDirtDepositResponse)) *
                    frostStrength *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    _FrostColor.rgb,
                    (half)(frostMask * 0.62));

                float wetGlobalDarken =
                    wetness * saturate(_WetDarkenStrength) * 0.36;
                albedo *= (half)max(0.0, 1.0 - wetGlobalDarken);

                float monolithicRelief =
                    broadValue * 0.028 +
                    exposureVisual * (0.078 * generatedMassExposureResponse) -
                    creviceVisual * (0.110 * generatedMassCreviceResponse) -
                    baseVisual * (0.052 * generatedMassBaseResponse);
                half3 monolithicTarget =
                    _BaseColor.rgb * (half)max(0.0, 1.0 + monolithicRelief);
                albedo = lerp(
                    albedo,
                    monolithicTarget,
                    (half)monolithicFlatten);

                return albedo;
            }

            half3 ApplyStylizedValueShaping(
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

                float isGroundSurface = ResolveSurfaceContractIsGround();
                half generatedMassBottomMask =
                    (half)ResolveGeneratedMassOrganicBottomMask(input);
                half defaultBottomMask =
                    1.0h -
                    smoothstep(
                        0.0h,
                        max(0.001h, (half)_BottomDarkenHeight),
                        (half)max(0.0, input.positionOS.y));
                half bottomMask = lerp(
                    generatedMassBottomMask,
                    defaultBottomMask,
                    (half)isGroundSurface);
                half sideMask = pow(
                    saturate(1.0h - abs(normalWS.y)),
                    max(0.5h, (half)_EdgeDarkenPower));
                half generatedMassBaseResponse =
                    (half)max(0.0, _GeneratedMassBaseResponse);
                half bottomResponseScale =
                    lerp(generatedMassBaseResponse, 1.0h, (half)isGroundSurface);
                half bottomDarken =
                    bottomMask *
                    saturate((half)_BottomDarkenStrength) *
                    bottomResponseScale;
                half broadEdgeDarken =
                    bottomMask *
                    sideMask *
                    saturate((half)_EdgeDarkenStrength) *
                    bottomResponseScale;
                half valueScale =
                    highlightScale *
                    (1.0h - saturate(bottomDarken + broadEdgeDarken));

                return albedo * valueScale;
            }

            half ResolveProfileSmoothness()
            {
                return saturate(
                    (half)_Smoothness +
                    (half)_Wetness * (half)_WetSmoothnessBoost +
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

            SurfaceData BuildSurfaceData(half3 albedo)
            {
                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.specular =
                    (half3)_SpecularStrength *
                    // Wet and monolithic stone can have controlled highlights,
                    // but the previous amplification pushed profiles toward
                    // polished metal/glass. Keep profile identity without
                    // overwhelming stone roughness.
                    lerp(
                        1.0h,
                        1.25h,
                        saturate((half)_Wetness)) *
                    lerp(
                        1.0h,
                        1.10h,
                        saturate((half)_MonolithicFlatten));
                surfaceData.metallic = 0.0h;
                surfaceData.smoothness = ResolveProfileSmoothness();
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

                half3 albedo = ResolvePixelSurfaceColor(input);
                albedo = ApplyStylizedValueShaping(albedo, input, normalWS);
                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    _GeneratedMassOverallRockTint.rgb,
                    _GeneratedMassOverallRockTintStrength);
                albedo = ApplyGeneratedMassGeometryEdgeWearResponse(
                    albedo,
                    input);

                InputData inputData = BuildInputData(input, normalWS);
                SurfaceData surfaceData = BuildSurfaceData(albedo);
                half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);

                // Keep URP/PBR lighting, shadows, local lights and specular,
                // but reduce how much RGB light colour can override the rock's
                // chosen material hue. This preserves brightness/form from PBR
                // while letting light tint remain an adjustable influence.
                half3 safeAlbedo = max(albedo, half3(0.001h, 0.001h, 0.001h));
                half3 pbrLightingRatio = pbrColor.rgb / safeAlbedo;
                half lightingLuma =
                    dot(
                        pbrLightingRatio,
                        half3(0.2126h, 0.7152h, 0.0722h));
                half3 neutralLitColor =
                    albedo * max(0.0h, lightingLuma);

                half lightingTintInfluence =
                    saturate((half)_GeneratedMassLightingTintInfluence);
                half3 finalRgb =
                    lerp(
                        neutralLitColor,
                        pbrColor.rgb,
                        lightingTintInfluence);

                finalRgb = MixFog(finalRgb, inputData.fogCoord);
                return half4(finalRgb, pbrColor.a);
            }
#endif // PS3D_PIXELSURFACEFORWARDPASS_HLSL
