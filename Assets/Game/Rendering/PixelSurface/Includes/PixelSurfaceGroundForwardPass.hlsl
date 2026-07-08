#ifndef PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL
#define PS3D_PIXELSURFACEGROUNDFORWARDPASS_HLSL

            half3 ResolvePixelGroundSurfaceColor(Varyings input)
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

                float exposureMask =
                    ResolveGroundExposureMask(input) * contractMask;
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

                float groundSemanticScale =
                    1.0 +
                    (groundSnowVisual * 0.11 -
                     groundDampVisual * 0.18 -
                     groundRockyDryVisual * 0.035 +
                     groundVegetationVisual * 0.025) *
                    profileContrast;

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, groundSemanticScale);

                albedo = lerp(
                    albedo,
                    _FrostColor.rgb,
                    (half)(groundSnowVisual * 0.34));
                albedo *=
                    (half)max(0.0, 1.0 - groundDampVisual * 0.24);
                albedo = lerp(
                    albedo,
                    albedo * half3(0.88h, 0.90h, 0.93h),
                    (half)(groundRockyDryVisual * 0.22));
                albedo = lerp(
                    albedo,
                    albedo * half3(0.94h, 1.00h, 0.90h),
                    (half)(groundVegetationVisual * 0.18));

                float wetness = saturate(_Wetness);
                float wetGlobalDarken =
                    wetness * saturate(_WetDarkenStrength) * 0.36;
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

            half ResolveGroundProfileSmoothness()
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
                    lerp(
                        1.0h,
                        1.25h,
                        saturate((half)_Wetness)) *
                    lerp(
                        1.0h,
                        1.10h,
                        saturate((half)_MonolithicFlatten));
                surfaceData.metallic = 0.0h;
                surfaceData.smoothness = ResolveGroundProfileSmoothness();
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
                SurfaceData surfaceData = BuildSurfaceData(albedo);
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
