#ifndef PS3D_RIVER_WATER_REFRACTION_INCLUDED
#define PS3D_RIVER_WATER_REFRACTION_INCLUDED

struct RiverWaterRefractionInputs
{
    float2 screenUV;
    float3 positionWS;
    float3 baseNormalWS;
    float3 surfaceNormalWS;
    float3 tangentWS;
    float3 sideWS;
    float globalDistance;
    float lateralMetres;
    float visibleHalfWidth;
    float surfaceHalfWidth;
    float freezeAmount;
    float iceCloudiness;
};

struct RiverWaterRefractionResult
{
    float3 sceneColour;
    float2 refractedUV;
    float2 offset;
    float depthInfluence;
    float shoreMask;
    float sampleValidity;
    float iceDiffusion;
};

float RiverWaterRefractionInsideScreen(float2 uv)
{
    return
        step(0.0, uv.x) *
        step(uv.x, 1.0) *
        step(0.0, uv.y) *
        step(uv.y, 1.0);
}

float RiverWaterResolveRefractionShoreMask(
    float lateralMetres,
    float visibleHalfWidth,
    float surfaceHalfWidth,
    float shoreRefraction)
{
    float lateral = abs(lateralMetres);
    float visible = max(0.001, visibleHalfWidth);
    float surface = max(visible + 0.001, surfaceHalfWidth);
    float retainedAtShore = saturate(shoreRefraction);

    if (lateral <= visible)
    {
        float interiorBand = max(0.15, visible * 0.22);
        float interiorDistance = visible - lateral;
        float interiorBlend = smoothstep(
            0.0,
            interiorBand,
            interiorDistance);

        return lerp(retainedAtShore, 1.0, interiorBlend);
    }

    float hiddenWidth = max(0.001, surface - visible);
    float hiddenRemaining = saturate((surface - lateral) / hiddenWidth);

    return retainedAtShore * smoothstep(
        0.0,
        1.0,
        hiddenRemaining);
}


float3 RiverWaterEvaluateStaticIceNormal(
    TEXTURE2D_PARAM(detailTexture, detailSampler),
    RiverWaterRefractionInputs input,
    float seed)
{
    float2 uv = float2(
        input.lateralMetres / 2.7,
        input.globalDistance / 3.8);

    uv += float2(
        frac(seed * 0.017),
        frac(seed * 0.031));

    float3 tangentNormal = UnpackNormalScale(
        SAMPLE_TEXTURE2D(detailTexture, detailSampler, uv),
        0.45);

    return normalize(
        input.sideWS * tangentNormal.x +
        input.tangentWS * tangentNormal.y +
        input.baseNormalWS * tangentNormal.z);
}

float RiverWaterValidateRefractedSample(
    float2 candidateUV,
    RiverWaterRefractionInputs input,
    RiverWaterDepthData originalDepth,
    float fallbackDepth,
    float depthRange,
    float depthContrast,
    float clarity,
    float edgeProtection,
    out RiverWaterDepthData candidateDepth)
{
    float insideScreen = RiverWaterRefractionInsideScreen(candidateUV);

    candidateDepth = RiverWaterEvaluateDepth(
        saturate(candidateUV),
        input.positionWS,
        fallbackDepth,
        depthRange,
        depthContrast,
        clarity);

    float protection = saturate(edgeProtection);

    // A candidate that becomes much shallower than the original sample is
    // likely crossing a foreground object, protruding rock, or dry bank.
    // A deeper candidate is less dangerous and receives a wider allowance.
    float shallowerJump = max(
        0.0,
        originalDepth.verticalDepth -
        candidateDepth.verticalDepth);
    float deeperJump = max(
        0.0,
        candidateDepth.verticalDepth -
        originalDepth.verticalDepth);

    float protectedShallowAllowance = max(
        0.20,
        originalDepth.verticalDepth * 0.38 + depthRange * 0.10);
    float relaxedShallowAllowance = max(
        1.50,
        depthRange * 1.75);
    float shallowAllowance = lerp(
        relaxedShallowAllowance,
        protectedShallowAllowance,
        protection);

    float deepAllowance = lerp(
        max(3.0, depthRange * 3.0),
        max(0.75, originalDepth.verticalDepth * 1.35 + depthRange * 0.35),
        protection);

    float shallowContinuity = 1.0 - smoothstep(
        shallowAllowance * 0.65,
        shallowAllowance,
        shallowerJump);
    float deepContinuity = 1.0 - smoothstep(
        deepAllowance * 0.65,
        deepAllowance,
        deeperJump);

    return
        insideScreen *
        originalDepth.validSceneDepth *
        candidateDepth.validSceneDepth *
        shallowContinuity *
        deepContinuity;
}

float3 RiverWaterSampleValidatedScene(
    float2 candidateUV,
    float3 originalScene,
    RiverWaterRefractionInputs input,
    RiverWaterDepthData originalDepth,
    float fallbackDepth,
    float depthRange,
    float depthContrast,
    float clarity,
    float edgeProtection,
    out float validity)
{
    RiverWaterDepthData candidateDepth;

    validity = RiverWaterValidateRefractedSample(
        candidateUV,
        input,
        originalDepth,
        fallbackDepth,
        depthRange,
        depthContrast,
        clarity,
        edgeProtection,
        candidateDepth);

    float3 candidateScene = SampleSceneColor(
        saturate(candidateUV));

    return lerp(
        originalScene,
        candidateScene,
        validity);
}

float RiverWaterResolveSilhouettePreservation(
    RiverWaterDepthData originalDepth,
    RiverWaterDepthData candidateDepth,
    float edgeProtection,
    float preserveObjectSilhouettes)
{
    if (preserveObjectSilhouettes < 0.5)
    {
        return 1.0;
    }

    float original = max(0.0, originalDepth.verticalDepth);
    float candidate = max(0.0, candidateDepth.verticalDepth);

    // The ghosting case is specifically an original shallow object sample
    // being replaced by a substantially deeper background sample.
    float deeperJump = max(0.0, candidate - original);

    // Scale the safe allowance with the object's apparent submerged depth.
    // Deep riverbed-to-riverbed variation keeps full refraction, while a
    // shallow object edge transitioning to deep bed is progressively guarded.
    float nearSurfaceRisk = 1.0 - smoothstep(0.12, 1.25, original);
    float protection = saturate(edgeProtection);

    float relaxedAllowance = max(1.75, original * 2.0 + 0.75);
    float protectedAllowance = lerp(
        max(0.30, original * 0.75 + 0.20),
        max(0.12, original * 0.40 + 0.08),
        protection);

    float allowance = lerp(
        relaxedAllowance,
        protectedAllowance,
        nearSurfaceRisk);

    float guard = 1.0 - smoothstep(
        allowance * 0.55,
        allowance,
        deeperJump);

    // Never create a hard frozen outline. Even risky edges retain a small
    // amount of optical motion so silhouettes can still wobble subtly.
    return lerp(0.18, 1.0, guard);
}

RiverWaterRefractionResult RiverWaterEvaluateRefraction(
    TEXTURE2D_PARAM(detailTexture, detailSampler),
    RiverWaterRefractionInputs input,
    RiverWaterDepthData originalDepth,
    float liquidStrength,
    float depthInfluenceAmount,
    float normalInfluence,
    float shoreRefraction,
    float edgeProtection,
    float preserveObjectSilhouettes,
    float iceDistortionStrength,
    float iceDiffusionAmount,
    float refractionQuality,
    float fallbackDepth,
    float depthRange,
    float depthContrast,
    float clarity,
    float seed,
    float motionTime,
    float flowSpeed,
    float waveHeight,
    float waveLength,
    float waveSteepness,
    float detailStrength,
    float detailScale,
    float turbulence,
    float shoreMotion,
    float shoreMotionWidth,
    float shoreWaveHeightScale,
    float shoreWaveLengthScale,
    float shoreWaveSpacingScale,
    float shoreWaveReach,
    float shoreWaveTransitionLength,
    float shoreWaveSizeVariation,
    float shoreWaveSideAsymmetry,
    float shoreWaveProfileVariation,
    float shoreWaveProfileEvolutionStrength,
    float shoreWaveProfileEvolutionDuration)
{
    RiverWaterRefractionResult result;

    float3 originalScene = SampleSceneColor(input.screenUV);
    float freeze = saturate(input.freezeAmount);

    float depthInfluence = lerp(
        1.0,
        smoothstep(0.0, 1.0, originalDepth.normalizedDepth),
        saturate(depthInfluenceAmount));

    float shoreMask = RiverWaterResolveRefractionShoreMask(
        input.lateralMetres,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        shoreRefraction);

    float3 staticIceNormal = RiverWaterEvaluateStaticIceNormal(
        TEXTURE2D_ARGS(detailTexture, detailSampler),
        input,
        seed);

    RiverWaterMotionInputs opticalMotionInputs;
    opticalMotionInputs.positionWS = input.positionWS;
    opticalMotionInputs.baseNormalWS = input.baseNormalWS;
    opticalMotionInputs.tangentWS = input.tangentWS;
    opticalMotionInputs.sideWS = input.sideWS;
    opticalMotionInputs.globalDistance = input.globalDistance;
    opticalMotionInputs.lateralMetres = input.lateralMetres;
    opticalMotionInputs.visibleHalfWidth = input.visibleHalfWidth;
    opticalMotionInputs.surfaceHalfWidth = input.surfaceHalfWidth;
    opticalMotionInputs.time = motionTime;
    opticalMotionInputs.freezeAmount = input.freezeAmount;

    float liquidFactor = 1.0 - freeze;
    float shoreLength = max(
        0.25,
        waveLength * max(0.25, shoreWaveLengthScale));
    float shoreGap =
        waveLength * max(0.0, shoreWaveSpacingScale);
    float sideSign = input.lateralMetres < 0.0 ? -1.0 : 1.0;
    float2 shoreProfileEvolution =
        RiverWaterResolveShoreProfileEvolution(
            input.globalDistance,
            motionTime,
            flowSpeed,
            shoreLength,
            shoreGap,
            sideSign,
            shoreWaveTransitionLength,
            shoreWaveSideAsymmetry,
            shoreWaveProfileEvolutionStrength,
            shoreWaveProfileEvolutionDuration,
            seed);
    float motionBankMask;
    RiverWaterEvaluateSurfaceHeightWithEvolution(
        input.globalDistance,
        input.lateralMetres,
        input.visibleHalfWidth,
        input.surfaceHalfWidth,
        motionTime,
        flowSpeed,
        waveHeight,
        waveLength,
        waveSteepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreProfileEvolution,
        liquidFactor,
        seed,
        motionBankMask);

    float3 macroNormalWS = RiverWaterEvaluateSurfaceNormal(
        opticalMotionInputs,
        flowSpeed,
        waveHeight,
        waveLength,
        waveSteepness,
        turbulence,
        shoreMotion,
        shoreMotionWidth,
        shoreWaveHeightScale,
        shoreWaveLengthScale,
        shoreWaveSpacingScale,
        shoreWaveReach,
        shoreWaveTransitionLength,
        shoreWaveSizeVariation,
        shoreWaveSideAsymmetry,
        shoreWaveProfileVariation,
        shoreWaveProfileEvolutionStrength,
        shoreWaveProfileEvolutionDuration,
        shoreProfileEvolution,
        seed);

    float3 detailNormalWS = RiverWaterEvaluateDetailNormal(
        TEXTURE2D_ARGS(detailTexture, detailSampler),
        opticalMotionInputs,
        detailScale,
        detailStrength * motionBankMask * liquidFactor,
        flowSpeed,
        turbulence,
        seed);

    float3 baseNormalVS = mul(
        (float3x3)UNITY_MATRIX_V,
        normalize(input.baseNormalWS));
    float3 macroNormalVS = mul(
        (float3x3)UNITY_MATRIX_V,
        normalize(macroNormalWS));
    float3 detailNormalVS = mul(
        (float3x3)UNITY_MATRIX_V,
        normalize(detailNormalWS));
    float3 iceNormalVS = mul(
        (float3x3)UNITY_MATRIX_V,
        staticIceNormal);

    float2 macroDelta = macroNormalVS.xy - baseNormalVS.xy;
    float2 detailDelta = detailNormalVS.xy - baseNormalVS.xy;
    float2 iceNormalDelta = iceNormalVS.xy - baseNormalVS.xy;

    // Macro bending and fine shimmer are evaluated independently but share
    // the same Stage 3 river-space phase. This creates local deformation of
    // the entire transmitted scene instead of a nearly uniform image shift.
    float macroAuthority = saturate(
        waveHeight * 4.0 +
        waveSteepness * 0.35);
    float detailAuthority = saturate(
        detailStrength * 0.85 +
        turbulence * 0.25);

    float2 liquidOpticalDelta =
        macroDelta * lerp(0.55, 1.15, macroAuthority) +
        detailDelta * lerp(0.45, 1.05, detailAuthority);

    float deltaLength = length(liquidOpticalDelta);
    liquidOpticalDelta *=
        min(1.0, 0.85 / max(0.0001, deltaLength));

    float opticalGain = lerp(
        3.0,
        5.5,
        saturate(turbulence * 0.75 + detailStrength * 0.20));

    float2 liquidOffset =
        liquidOpticalDelta *
        saturate(normalInfluence) *
        max(0.0, liquidStrength) *
        opticalGain *
        depthInfluence *
        shoreMask;

    float2 iceOffset =
        iceNormalDelta *
        saturate(normalInfluence) *
        max(0.0, iceDistortionStrength) *
        shoreMask;

    float2 finalOffset = lerp(
        liquidOffset,
        iceOffset,
        freeze);

    float diffusion = saturate(
        freeze *
        (saturate(iceDiffusionAmount) * 0.72 +
         saturate(input.iceCloudiness) * 0.45));

    if (dot(finalOffset, finalOffset) < 0.0000000001 &&
        diffusion < 0.0001)
    {
        result.sceneColour = originalScene;
        result.refractedUV = input.screenUV;
        result.offset = 0.0;
        result.depthInfluence = depthInfluence;
        result.shoreMask = shoreMask;
        result.sampleValidity = originalDepth.validSceneDepth;
        result.iceDiffusion = 0.0;
        return result;
    }

    float2 candidateUV = input.screenUV + finalOffset;

    RiverWaterDepthData centreCandidateDepth;
    float centreValidity = RiverWaterValidateRefractedSample(
        candidateUV,
        input,
        originalDepth,
        fallbackDepth,
        depthRange,
        depthContrast,
        clarity,
        edgeProtection,
        centreCandidateDepth);

    float silhouettePreservation =
        RiverWaterResolveSilhouettePreservation(
            originalDepth,
            centreCandidateDepth,
            edgeProtection,
            preserveObjectSilhouettes);

    centreValidity *= silhouettePreservation;

    float3 centreCandidateScene = SampleSceneColor(
        saturate(candidateUV));

    float3 refractedScene = lerp(
        originalScene,
        centreCandidateScene,
        centreValidity);

    int quality = (int)round(refractionQuality);

    if (diffusion > 0.0001 && quality > 0)
    {
        float2 texel = 1.0 / max(_ScaledScreenParams.xy, 1.0.xx);
        float radiusPixels = diffusion * lerp(0.75, 2.75, diffusion);
        float2 radius = texel * radiusPixels;

        float sampleValidityA;
        float sampleValidityB;

        float3 sampleA = RiverWaterSampleValidatedScene(
            candidateUV + float2(radius.x, radius.y),
            originalScene,
            input,
            originalDepth,
            fallbackDepth,
            depthRange,
            depthContrast,
            clarity,
            edgeProtection,
            sampleValidityA);

        float3 sampleB = RiverWaterSampleValidatedScene(
            candidateUV + float2(-radius.x, -radius.y),
            originalScene,
            input,
            originalDepth,
            fallbackDepth,
            depthRange,
            depthContrast,
            clarity,
            edgeProtection,
            sampleValidityB);

        float3 diffusionColour =
            (refractedScene + sampleA + sampleB) / 3.0;

        float diffusionValidity =
            (centreValidity + sampleValidityA + sampleValidityB) / 3.0;

        if (quality > 1)
        {
            float sampleValidityC;
            float sampleValidityD;

            float3 sampleC = RiverWaterSampleValidatedScene(
                candidateUV + float2(radius.x, -radius.y),
                originalScene,
                input,
                originalDepth,
                fallbackDepth,
                depthRange,
                depthContrast,
                clarity,
                edgeProtection,
                sampleValidityC);

            float3 sampleD = RiverWaterSampleValidatedScene(
                candidateUV + float2(-radius.x, radius.y),
                originalScene,
                input,
                originalDepth,
                fallbackDepth,
                depthRange,
                depthContrast,
                clarity,
                edgeProtection,
                sampleValidityD);

            diffusionColour =
                (refractedScene + sampleA + sampleB + sampleC + sampleD) / 5.0;

            diffusionValidity =
                (centreValidity +
                 sampleValidityA +
                 sampleValidityB +
                 sampleValidityC +
                 sampleValidityD) / 5.0;
        }

        refractedScene = lerp(
            refractedScene,
            diffusionColour,
            diffusion);

        centreValidity = lerp(
            centreValidity,
            diffusionValidity,
            diffusion);
    }

    result.sceneColour = refractedScene;
    result.refractedUV = saturate(candidateUV);
    result.offset = finalOffset;
    result.depthInfluence = depthInfluence;
    result.shoreMask = shoreMask;
    result.sampleValidity = centreValidity;
    result.iceDiffusion = diffusion;
    return result;
}

#endif
