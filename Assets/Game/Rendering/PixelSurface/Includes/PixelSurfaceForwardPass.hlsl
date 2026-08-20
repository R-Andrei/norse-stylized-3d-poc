#ifndef PS3D_PIXELSURFACEFORWARDPASS_HLSL
#define PS3D_PIXELSURFACEFORWARDPASS_HLSL

// GM-SURFACE.5P ACTIVE DEFECT CONTRACT:
// Generated Mass is failing per-surface directional-light coherence. The target
// is NOT global darkness, exposure, ambient strength, or specular magnitude.
// Under the same light, each source face and bevel must respond according to its
// own measured orientation. A bevel outside both parent responses is an orientation
// defect only when its measured NdotL predicts an intermediate response between
// those parents. Do not accept average-luminance or BRDF parity as closure while
// measured orientation/response inversions remain.
// GM-SURFACE.5Q adds audit-only cumulative albedo/direct checkpoints plus raw
// mask/scalar outputs. Every executable branch added by 5Q is compiled only
// under _SURFACE_CAUSALITY_AUDIT; ordinary production variants are unchanged.

// GM-SURFACE.5S LOW-LIGHT FORM CONTRACT:
// The primary form term remains illumination-derived from the actual main-light
// direction and resolved fragment normal. GM-SURFACE.5S4 adds one explicitly
// stylized, generation-time logical-face tone carried in Generated Mass UV2.w.
// That tone is deterministic, area-centered around zero, material-independent,
// and affects existing indirect GI only. Ground owns a separate UV2.w meaning
// and never executes this branch. Face Separation zero is exact 5S2 parity;
// both low-light authoring controls at zero recover the 5R baked-GI baseline.
half3 ResolveGeneratedMassLowLightFormGI(
    half3 bakedGI,
    half3 normalWS,
    half generatedFaceTone)
{
    Light mainLight = GetMainLight();
    half sourceLuma = dot(
        mainLight.color,
        half3(0.2126h, 0.7152h, 0.0722h));
    half sourceGate = saturate(sourceLuma);

    half facing = clamp(
        dot(normalWS, mainLight.direction),
        -1.0h,
        1.0h);
    half wrap = saturate((half)_DiffuseWrap);
    half wrappedFacing = lerp(
        facing,
        max(facing, 0.0h),
        wrap);
    half targetScale = 1.0h + 0.40h * wrappedFacing;
    half formStrength = clamp(
        (half)_ShadowAmbientStrength,
        0.0h,
        2.0h);
    half formWeight = formStrength * sourceGate;
    half primaryScale = lerp(1.0h, targetScale, formWeight);

    half faceSeparationStrength =
        saturate((half)_GeneratedMassLowLightFaceSeparation);
    if (faceSeparationStrength <= 0.0001h)
    {
        return bakedGI * primaryScale;
    }

    // GM-SURFACE.5S4: final mesh generation assigns one signed tone to each
    // logical planar face. Runtime no longer tries to infer a second face
    // identity from camera direction. The small zero-mean tone redistribution
    // is clamped inside the existing 5S2 0.20..1.80 contrast envelope.
    half faceTone = clamp(generatedFaceTone, -1.0h, 1.0h);
    half faceToneScale =
        0.16h *
        sourceGate *
        faceSeparationStrength *
        faceTone;
    half finalScale = clamp(
        primaryScale + faceToneScale,
        0.20h,
        1.80h);

    return bakedGI * finalScale;
}

half3 ResolvePixelSurfaceBakedGI(
    half3 normalWS,
    half generatedFaceTone)
{
    half3 bakedGI = SampleSH(normalWS);
    if (ResolveSurfaceContractIsGround() < 0.5)
    {
        bakedGI = ResolveGeneratedMassLowLightFormGI(
            bakedGI,
            normalWS,
            generatedFaceTone);
    }
    return bakedGI;
}

#if defined(_SURFACE_CAUSALITY_AUDIT)
            int ResolveSurfaceCausalityMode()
            {
                return (int)round(_SurfaceCausalityMode);
            }

            half3 ResolveSurfaceCausalityMainDirect(
                half3 albedo,
                Varyings input,
                half3 normalWS)
            {
                Light mainLight = GetMainLight(
                    TransformWorldToShadowCoord(input.positionWS));
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half attenuation =
                    mainLight.distanceAttenuation *
                    mainLight.shadowAttenuation;
                return
                    albedo *
                    mainLight.color *
                    ndotl *
                    attenuation *
                    (half)max(0.0, _SurfaceCausalityLightScale);
            }

            half3 ResolveSurfaceCausalityAmbient(
                half3 albedo,
                Varyings input,
                half3 normalWS)
            {
                return
                    albedo *
                    ResolvePixelSurfaceBakedGI(
                        normalWS,
                        (half)input.materialMasks.w) *
                    (half)max(0.0, _SurfaceCausalityLightScale);
            }
#endif

            half3 ResolvePixelSurfaceColor(Varyings input)
            {
                half4 baseSample =
                    SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 20)
                {
                    return baseSample.rgb * _BaseColor.rgb;
                }
                if (ResolveSurfaceCausalityMode() == 40)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        baseSample.rgb * _BaseColor.rgb,
                        input,
                        normalize(input.normalWS));
                }
#endif

                float isGroundSurface = ResolveSurfaceContractIsGround();
                float generatedMassSurface = 1.0 - isGroundSurface;
                float primaryConvexBoundary =
                    step(0.5, 1.0 - abs(input.structuralFeatures.x - 1.0)) *
                    step(0.0001, input.structuralFeatures.y);
                float secondaryConvexBoundary =
                    step(0.5, 1.0 - abs(input.structuralFeatures.z - 1.0)) *
                    step(0.0001, input.structuralFeatures.w);
                float generatedMassBevelMask =
                    generatedMassSurface *
                    saturate(max(primaryConvexBoundary, secondaryConvexBoundary));

                // GM-SURFACE.5R: Stage E proved that the HLSL tonal stage is
                // harmless to source-face orientation ordering but introduces a
                // large bevel-only ordering error. Generated convex bevels must
                // therefore not consume topology/interpolation-sensitive vertex-R,
                // broad-value, or cell-warp authority. They retain the shared
                // world-position pixel-cell variation, which is independent of
                // bevel triangulation. Ground and non-bevel Generated Mass pixels
                // retain their ordinary authored controls.
                float bevelIndependentWarpStrength =
                    _PixelWarpStrength * (1.0 - generatedMassBevelMask);
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
                    warp * _PixelCellSize * bevelIndependentWarpStrength;

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
                float bevelIndependentVertexVariation =
                    vertexVariation * (1.0 - generatedMassBevelMask);
                float bevelIndependentBroadVariation =
                    broadValue * (1.0 - generatedMassBevelMask);
                float pixelProfileContrast =
                    max(0.0, _ProfilePixelContrast) *
                    lerp(1.0, 1.0 - saturate(_WetPixelSoftening), saturate(_Wetness)) *
                    lerp(1.0, max(0.0, _FrostContrast), saturate(_FrostStrength)) *
                    lerp(1.0, 0.25, saturate(_MonolithicFlatten));
                float structuralVariationStrength =
                    ResolveGeneratedMassStructuralVariationStrength(
                        input.structuralFeatures);
                float baseTonalOffset =
                    (pixelVariation * _PixelVariation +
                     bevelIndependentVertexVariation * _PixelVertexVariation +
                     bevelIndependentBroadVariation * _PixelBroadVariation) *
                    pixelProfileContrast;
                // GM-SURFACE.6A.4 reuses the already-evaluated pixel-cell
                // variation as a direct structural material term. It is not
                // attenuated by the pre-existing bevel breakup that 5R
                // deliberately suppresses on convex transition geometry.
                float structuralTonalOffset =
                    pixelVariation * structuralVariationStrength;
                float tonalOffset =
                    baseTonalOffset + structuralTonalOffset;
                half tonalScale =
                    (half)max(0.0, 1.0 + tonalOffset * _PixelEffectStrength);
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 21)
                {
                    return baseSample.rgb * _BaseColor.rgb * tonalScale;
                }
                if (ResolveSurfaceCausalityMode() == 41)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        baseSample.rgb * _BaseColor.rgb * tonalScale,
                        input,
                        normalize(input.normalWS));
                }
#endif

                float exposureMask =
                    saturate((float)input.color.g) * contractMask;

                // Generated Mass exposure, crevice/base and dirt are compiled
                // into the mesh channels. Recomputing them from generated-face
                // orientation reclassifies bevels and chip transitions before
                // lighting, which creates fixed bright/dark feature bands.
                float generatedMassMask = (1.0 - isGroundSurface) * contractMask;
                float resolvedCreviceMask = lerp(
                    ResolveShaderCreviceBaseMask(input),
                    saturate((float)input.color.b),
                    generatedMassMask);
                float creviceMask =
                    lerp(resolvedCreviceMask * contractMask, 0.0, isGroundSurface);
                float resolvedDirtDepositMask = lerp(
                    ResolveShaderDirtDepositMask(input),
                    saturate((float)input.materialMasks.y),
                    generatedMassMask);
                float dirtDepositMask =
                    lerp(resolvedDirtDepositMask * contractMask, 0.0, isGroundSurface);
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

                // GM-SURFACE.5R ORIENTATION-COHERENCE CONTRACT:
                // Exposure is a material-semantic mask, not illumination. 5Q
                // measured zero source-face inversions before this stage and 28
                // newly introduced inversions when upwardness/height-driven
                // exposure was allowed to scale albedo. Generated Mass exposure
                // therefore has no luminance authority before PBR. The separate
                // value-preserving exposure tint path remains available below.
                // Ground semantic scaling is intentionally unchanged.
                float generatedMassSemanticScale = 1.0;
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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                int stageAttributionMode = ResolveSurfaceCausalityMode();
                if (stageAttributionMode == 32)
                {
                    return half3(input.color.r, input.color.g, input.color.b);
                }
                if (stageAttributionMode == 33)
                {
                    float normalY = normalize((float3)input.normalOS).y;
                    return half3(
                        saturate((float)input.materialMasks.y),
                        ResolveGeneratedMassHeight01(input),
                        normalY * 0.5 + 0.5);
                }
                if (stageAttributionMode == 34)
                {
                    return half3(exposureMask, creviceMask, baseMask);
                }
                if (stageAttributionMode == 35)
                {
                    return half3(
                        dirtDepositMask,
                        exposureVisual,
                        creviceVisual);
                }
                if (stageAttributionMode == 36)
                {
                    return half3(
                        baseVisual,
                        dirtDepositVisual,
                        ResolveGeneratedMassMottleNoise(input));
                }
                if (stageAttributionMode == 37)
                {
                    return half3(tonalScale, semanticScale, profileContrast);
                }
#endif

                half3 albedo =
                    baseSample.rgb *
                    _BaseColor.rgb *
                    tonalScale *
                    (half)max(0.0, semanticScale);
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 22)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 42)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 23)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 43)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 25)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 44)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

                // GM-SURFACE.5R: crevice is material identity, not a second
                // shadow field. 5Q measured 38 new source-face orientation
                // inversions at this stage while the direct NdotL product remained
                // exact. Do not push direct-light albedo toward a fixed dark target.
                // Preserve only value-preserving semantic tint authority; any
                // future crevice occlusion must be authored in an indirect/AO
                // contract rather than by corrupting pre-light direct albedo.
                half3 creviceTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        albedo,
                        _GeneratedMassCreviceTint.rgb,
                        _GeneratedMassCreviceTintStrength);
                float creviceTintOpacity =
                    creviceVisual *
                    generatedMassCreviceResponse *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    creviceTintTarget,
                    (half)saturate(creviceTintOpacity));
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 26)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 45)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

                // GM-SURFACE.5R: base/contact is also semantic material data,
                // not a proxy for weaker incident light. 5Q measured 15 additional
                // source-face inversions when this layer darkened pre-light albedo.
                // Keep value-preserving tint only; grounding/occlusion must not
                // override the actual surface-orientation response to direct light.
                half3 baseTintTarget =
                    PS3D_ApplyValuePreservingTint(
                        albedo,
                        _GeneratedMassBaseTint.rgb,
                        _GeneratedMassBaseTintStrength);
                float baseTintOpacity =
                    baseVisual *
                    generatedMassBaseResponse *
                    generatedMassSurface;
                albedo = lerp(
                    albedo,
                    baseTintTarget,
                    (half)saturate(baseTintOpacity));
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 27)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 46)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 28)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 47)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 24)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 48)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 29)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 49)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

                float wetGlobalDarken =
                    wetness * saturate(_WetDarkenStrength) * 0.36;
                albedo *= (half)max(0.0, 1.0 - wetGlobalDarken);
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 30)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 50)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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
#if defined(_SURFACE_CAUSALITY_AUDIT)
                if (ResolveSurfaceCausalityMode() == 31)
                {
                    return albedo;
                }
                if (ResolveSurfaceCausalityMode() == 51)
                {
                    return ResolveSurfaceCausalityMainDirect(
                        albedo, input, normalize(input.normalWS));
                }
#endif

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

            InputData BuildInputData(
                Varyings input,
                half3 normalWS)
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
                inputData.bakedGI = ResolvePixelSurfaceBakedGI(
                    normalWS,
                    (half)input.materialMasks.w);
                inputData.normalizedScreenSpaceUV =
                    GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1.0h, 1.0h, 1.0h, 1.0h);
                return inputData;
            }

            SurfaceData BuildSurfaceData(
                half3 albedo,
                float4 structuralFeatures)
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
                surfaceData.smoothness = saturate(
                    ResolveProfileSmoothness() +
                    ResolveGeneratedMassStructuralSmoothnessOffset(
                        structuralFeatures));
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

                // GM-SURFACE.6A.2: bypass normal/material/PBR work only for the
                // two explicit structural transport diagnostics. Off mode
                // returns a negative sentinel and preserves the 6A.1 path.
                half3 structuralDiagnosticColor =
                    ResolveGeneratedMassStructuralDiagnosticColor(
                        input.structuralFeatures);
                if (structuralDiagnosticColor.r >= 0.0h)
                {
                    return half4(structuralDiagnosticColor, 1.0h);
                }

#if !defined(_SURFACE_CAUSALITY_AUDIT)
                // Production variant: intentionally identical to the validated
                // pre-audit forward path. Diagnostic derivatives, branches, and
                // isolation modes are compiled out of ordinary materials.
                half3 geometricNormalWS = normalize(input.normalWS);
                half3 normalWS = geometricNormalWS;
                float generatedMassSurface =
                    1.0 - ResolveSurfaceContractIsGround();
                if (generatedMassSurface > 0.5)
                {
                    normalWS = (half3)ResolveGeneratedMassWholeSurfaceNormalWS(
                        input,
                        normalWS);
                }
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
                if (generatedMassSurface <= 0.5)
                {
                    albedo = ApplyStylizedValueShaping(albedo, input, normalWS);
                }
                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    _GeneratedMassOverallRockTint.rgb,
                    _GeneratedMassOverallRockTintStrength);

                InputData inputData = BuildInputData(
                    input,
                    normalWS);
                SurfaceData surfaceData = BuildSurfaceData(
                    albedo,
                    input.structuralFeatures);
                half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);

                // GM-SURFACE.5E baseline parity: Generated Masses use raw URP/PBR
                // output. Do not apply bevel-specific albedo painting, directional
                // pre-light value shaping, post-PBR light-colour reconstruction, or
                // shadow-side normal readability while validating geometry lighting.
                //
                // GM-SURFACE.5P: the acceptance target is per-fragment orientation
                // response and parent-bevel-parent ordering under one light. A global
                // brightness/specular adjustment can improve averages while leaving
                // the actual defect untouched, so never close this issue from the
                // whole-object PBR magnitude alone.
                half3 finalRgb;
                if (generatedMassSurface > 0.5)
                {
                    finalRgb = pbrColor.rgb;
                }
                else
                {
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
                    finalRgb =
                        lerp(
                            neutralLitColor,
                            pbrColor.rgb,
                            lightingTintInfluence);
                }

                finalRgb = MixFog(finalRgb, inputData.fogCoord);
                return half4(finalRgb, pbrColor.a);
#else
                int causalityMode = ResolveSurfaceCausalityMode();
                half3 storedNormalWS = normalize(input.normalWS);
                half3 viewDirectionWS =
                    SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                half3 triangleNormalWS = normalize(
                    cross(
                        ddy(input.positionWS),
                        ddx(input.positionWS)));
                triangleNormalWS = faceforward(
                    triangleNormalWS,
                    -viewDirectionWS,
                    triangleNormalWS);

                bool triangleNormalMode =
                    causalityMode == 5 ||
                    (causalityMode >= 15 && causalityMode <= 19);
                bool storedNormalMode =
                    causalityMode >= 11 && causalityMode <= 14;
                bool noGeneratedSurfaceNormalMode = causalityMode == 7;

                half3 normalWS = storedNormalWS;
                float generatedMassSurface =
                    1.0 - ResolveSurfaceContractIsGround();
                if (triangleNormalMode)
                {
                    normalWS = triangleNormalWS;
                }
                else if (!storedNormalMode)
                {
                    if (generatedMassSurface > 0.5 &&
                        !noGeneratedSurfaceNormalMode)
                    {
                        normalWS = (half3)ResolveGeneratedMassWholeSurfaceNormalWS(
                            input,
                            normalWS);
                    }
                    half flatNormalStrength =
                        saturate((half)_FlatNormalStrength);
                    if (flatNormalStrength > 0.001h)
                    {
                        normalWS = normalize(
                            lerp(
                                normalWS,
                                triangleNormalWS,
                                flatNormalStrength));
                    }
                }

                half3 debugColor = ResolveMaskDebugColor(input);
                if (debugColor.r >= 0.0h)
                {
                    return half4(debugColor, 1.0h);
                }

                if (causalityMode == 55 || causalityMode == 56)
                {
                    Light auditMainLight = GetMainLight(
                        TransformWorldToShadowCoord(input.positionWS));
                    if (causalityMode == 55)
                    {
                        return half4(
                            saturate(dot(storedNormalWS, auditMainLight.direction)),
                            auditMainLight.distanceAttenuation,
                            auditMainLight.shadowAttenuation,
                            1.0h);
                    }
                    return half4(
                        auditMainLight.direction * 0.5h + 0.5h,
                        1.0h);
                }

                half3 albedo = ResolvePixelSurfaceColor(input);
                if ((causalityMode >= 20 && causalityMode <= 37) ||
                    (causalityMode >= 40 && causalityMode <= 51))
                {
                    return half4(albedo, 1.0h);
                }
                if (generatedMassSurface <= 0.5)
                {
                    albedo = ApplyStylizedValueShaping(albedo, input, normalWS);
                }
                albedo = PS3D_ApplyValuePreservingTint(
                    albedo,
                    _GeneratedMassOverallRockTint.rgb,
                    _GeneratedMassOverallRockTintStrength);

                if (causalityMode == 1)
                {
                    return half4(albedo, 1.0h);
                }
                if (causalityMode == 54)
                {
                    return half4(
                        ResolveSurfaceCausalityMainDirect(
                            albedo,
                            input,
                            storedNormalWS),
                        1.0h);
                }
                if (causalityMode == 5)
                {
                    return half4(triangleNormalWS * 0.5h + 0.5h, 1.0h);
                }
                if (causalityMode == 6)
                {
                    return half4(normalWS * 0.5h + 0.5h, 1.0h);
                }
                if (causalityMode == 14)
                {
                    return half4(storedNormalWS * 0.5h + 0.5h, 1.0h);
                }

                bool constantAlbedoMode =
                    (causalityMode >= 2 && causalityMode <= 4) ||
                    (causalityMode >= 11 && causalityMode <= 13) ||
                    (causalityMode >= 15 && causalityMode <= 17);
                if (constantAlbedoMode)
                {
                    albedo = half3(0.5h, 0.5h, 0.5h);
                }

                bool directOnlyMode =
                    causalityMode == 3 ||
                    causalityMode == 9 ||
                    causalityMode == 12 ||
                    causalityMode == 16 ||
                    causalityMode == 18;
                if (directOnlyMode)
                {
                    return half4(
                        ResolveSurfaceCausalityMainDirect(
                            albedo,
                            input,
                            normalWS),
                        1.0h);
                }

                bool ambientOnlyMode =
                    causalityMode == 4 ||
                    causalityMode == 10 ||
                    causalityMode == 13 ||
                    causalityMode == 17 ||
                    causalityMode == 19;
                if (ambientOnlyMode)
                {
                    return half4(
                        ResolveSurfaceCausalityAmbient(
                            albedo,
                            input,
                            normalWS),
                        1.0h);
                }

                InputData inputData = BuildInputData(
                    input,
                    normalWS);
                SurfaceData surfaceData = BuildSurfaceData(
                    albedo,
                    input.structuralFeatures);
                half4 pbrColor = UniversalFragmentPBR(inputData, surfaceData);

                half3 finalRgb;
                if (generatedMassSurface > 0.5)
                {
                    finalRgb = pbrColor.rgb;
                }
                else
                {
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
                    finalRgb = lerp(
                        neutralLitColor,
                        pbrColor.rgb,
                        lightingTintInfluence);
                }

                if (causalityMode != 8)
                {
                    finalRgb = MixFog(finalRgb, inputData.fogCoord);
                }
                return half4(finalRgb, pbrColor.a);
#endif
            }
#endif // PS3D_PIXELSURFACEFORWARDPASS_HLSL
