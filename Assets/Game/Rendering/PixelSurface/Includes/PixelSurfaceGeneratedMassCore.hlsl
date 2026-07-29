#ifndef PS3D_PIXELSURFACEGENERATEDMASSCORE_HLSL
#define PS3D_PIXELSURFACEGENERATEDMASSCORE_HLSL

            float ResolveGeneratedMassHeight01(Varyings input)
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float rawHeight01 = saturate(
                    (input.positionOS.y - _GeneratedMassLocalMinY) /
                    height);
                float baseLift = saturate(_GeneratedMassMaskBaseLift);
                return saturate(
                    (rawHeight01 - baseLift) /
                    max(0.0001, 1.0 - baseLift));
            }

            float ResolveNotUpwardMask(Varyings input)
            {
                float normalY = normalize((float3)input.normalOS).y;
                return 1.0 - smoothstep(0.18, 0.78, normalY);
            }

            float3 ResolveGeneratedMassMaskCoordinate(
                Varyings input,
                float scale,
                float offset)
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float3 normalizedPosition =
                    float3(
                        input.positionOS.x / xzScale,
                        (input.positionOS.y - _GeneratedMassLocalMinY) / height,
                        input.positionOS.z / xzScale);

                return normalizedPosition * scale +
                    float3(
                        _GeneratedMassMaskSeed * 0.017 + offset,
                        _GeneratedMassMaskSeed * 0.011 - offset * 0.37,
                        _GeneratedMassMaskSeed * 0.019 + offset * 0.61);
            }

            float ResolveGeneratedMassPatchNoise(
                Varyings input,
                float scale,
                float offset)
            {
                float3 coordinate =
                    ResolveGeneratedMassMaskCoordinate(input, scale, offset);

                float broad = PS3D_ValueNoise31(coordinate);
                float detail = PS3D_ValueNoise31(coordinate * 2.23 + 17.31);
                return saturate(broad * 0.68 + detail * 0.32);
            }

            float ResolveGeneratedMassSoftPatchNoise(
                Varyings input,
                float scale,
                float offset)
            {
                float3 coordinate =
                    ResolveGeneratedMassMaskCoordinate(input, scale, offset);

                float a = PS3D_ValueNoise31(coordinate);
                float b = PS3D_ValueNoise31(coordinate * 1.71 + 9.73);
                float c = PS3D_ValueNoise31(coordinate * 3.11 + 27.19);
                return saturate(a * 0.52 + b * 0.33 + c * 0.15);
            }

            float ResolveGeneratedMassTallnessFactor()
            {
                float height = max(0.0001, _GeneratedMassLocalHeight);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                return saturate((height / xzScale - 0.65) * 0.58);
            }

            float ResolveGeneratedMassSizeFactor()
            {
                return saturate((_GeneratedMassLocalHeight - 0.75) * 0.16);
            }

            float ResolveGeneratedMassOrganicLowerFade(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float3 normalOS = normalize((float3)input.normalOS);
                float tallness = ResolveGeneratedMassTallnessFactor();
                float sizeFactor = ResolveGeneratedMassSizeFactor();
                float creviceReach = max(0.05, _GeneratedMassCreviceReach);
                float creviceSmoothness = max(0.05, _GeneratedMassCreviceSmoothness);
                float creviceBreakup = max(0.05, _GeneratedMassCreviceBreakup);
                float reach01 = saturate((creviceReach - 0.25) / 1.75);
                float smoothness01 = saturate((creviceSmoothness - 0.25) / 1.75);
                float breakup01 = saturate((creviceBreakup - 0.25) / 1.75);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float2 normalizedXZ = input.positionOS.xz / xzScale;
                float seed = _GeneratedMassMaskSeed;

                // Footprint lobes are useful for broad object-level variation, but
                // they cannot be the only field; on large side faces they can leave
                // a whole face inside one inactive footprint island.
                float footprintWarpA = PS3D_ValueNoise31(float3(
                    normalizedXZ * 0.45 + float2(seed * 0.007, seed * -0.013),
                    seed * 0.019));
                float footprintWarpB = PS3D_ValueNoise31(float3(
                    normalizedXZ.yx * 0.49 + float2(seed * -0.011, seed * 0.017),
                    seed * 0.023 + 11.7));
                float2 warpedXZ = normalizedXZ +
                    (float2(footprintWarpA, footprintWarpB) - 0.5) *
                    lerp(0.12, 0.42, breakup01);

                float footprintWaveA = sin(warpedXZ.x * 2.35 + seed * 0.071);
                float footprintWaveB = sin(warpedXZ.y * 2.05 + seed * 0.053 + 1.37);
                float footprintWaveC = sin(
                    (warpedXZ.x * 0.62 + warpedXZ.y * 0.48) * 3.35 +
                    seed * 0.093 - 0.48);
                float footprintWave =
                    (footprintWaveA * 0.36 +
                     footprintWaveB * 0.34 +
                     footprintWaveC * 0.30) * 0.5 + 0.5;

                float broadNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.42, 47.0);
                float lobeNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.72, 83.0);
                float patchNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 1.18, 119.0);
                float planeNoise =
                    ResolveGeneratedMassPatchNoise(input, 1.82, 29.0);
                float facetNoise = saturate(
                    (float)input.color.r * 0.28 +
                    broadNoise * 0.24 +
                    lobeNoise * 0.22 +
                    patchNoise * 0.16 +
                    planeNoise * 0.10);

                // Side-surface lobes: choose the useful width coordinate for the
                // current side face. Z-facing faces vary along local X; X-facing
                // faces vary along local Z. Each dominant side receives a separate
                // phase so the back/side faces do not mirror or disappear together.
                float dominantX = step(abs(normalOS.z), abs(normalOS.x));
                float sideWidthCoord = lerp(normalizedXZ.x, normalizedXZ.y, dominantX);
                float positiveSide = lerp(step(0.0, normalOS.z), step(0.0, normalOS.x), dominantX);
                float sidePhase = lerp(
                    lerp(23.7, 41.9, positiveSide),
                    lerp(67.3, 89.1, positiveSide),
                    dominantX);
                float sideWarp = PS3D_ValueNoise31(float3(
                    sideWidthCoord * 0.96 + sidePhase * 0.019,
                    height01 * 0.62 + seed * 0.017,
                    seed * 0.031 + sidePhase));
                float sideCoord = sideWidthCoord +
                    (sideWarp - 0.5) * lerp(0.18, 0.78, breakup01);

                float sideWaveA = sin(sideCoord * 5.6 + seed * 0.061 + sidePhase);
                float sideWaveB = sin(sideCoord * 10.4 + seed * 0.089 + sidePhase * 1.37);
                float sideWaveC = sin(sideCoord * 15.7 + seed * 0.047 - sidePhase * 0.73);
                float sideNoiseA = PS3D_ValueNoise31(float3(
                    sideCoord * 1.65 + sidePhase * 0.027,
                    seed * 0.021,
                    height01 * 0.42));
                float sideNoiseB = PS3D_ValueNoise31(float3(
                    sideCoord * 3.25 - sidePhase * 0.013,
                    seed * 0.037 + 9.1,
                    height01 * 0.85));
                float rawSideLobe = saturate(
                    (sideWaveA * 0.28 +
                     sideWaveB * 0.19 +
                     sideWaveC * 0.12) * 0.5 + 0.5);
                rawSideLobe = saturate(
                    rawSideLobe * 0.48 +
                    sideNoiseA * 0.32 +
                    sideNoiseB * 0.20);

                // Breakup should increase contrast and local relief, not simply
                // lower the threshold and behave like a second Reach slider.
                float sideLobe =
                    saturate((rawSideLobe - 0.5) *
                        lerp(1.35, 3.15, breakup01) + 0.5);
                float footprintLobe =
                    saturate((footprintWave * 0.44 +
                              broadNoise * 0.26 +
                              lobeNoise * 0.20 +
                              facetNoise * 0.10 - 0.5) *
                        lerp(1.15, 2.25, breakup01) + 0.5);
                float patchRelief =
                    saturate((patchNoise * 0.36 +
                              planeNoise * 0.24 +
                              facetNoise * 0.24 +
                              broadNoise * 0.16 - 0.5) *
                        lerp(1.05, 2.05, breakup01) + 0.5);

                float lobeHeightDriver = saturate(
                    sideLobe * 0.58 +
                    footprintLobe * 0.25 +
                    patchRelief * 0.17);
                float lobePresenceDriver = saturate(
                    sideLobe * 0.66 +
                    footprintLobe * 0.18 +
                    patchRelief * 0.16);

                // Every side point needs a low crawl floor. Breakup should make
                // areas crawl low or high; it should not create fully empty
                // vertical strips. Keep the floor in crawl height, not in mask
                // intensity, so it does not rebuild the old continuous skirt.
                float lobePresence = saturate(
                    lerp(0.18, 0.12, breakup01) +
                    smoothstep(0.30, 0.76, lobePresenceDriver) *
                    lerp(0.72, 0.86, breakup01));

                // Preserve approximate average height while increasing the low/high
                // spread as Breakup rises.
                float lobeHeightContrast =
                    saturate((lobeHeightDriver - 0.5) *
                        lerp(1.10, 2.80, breakup01) + 0.5);

                // Reach controls average crawl height. The default is intentionally
                // visible again, but local side lobes decide how much extra height
                // each area gets above the guaranteed low crawl floor.
                float averageCrawlHeight =
                    (0.078 + tallness * 0.030 + sizeFactor * 0.018) *
                    lerp(0.56, 1.44, reach01);
                float minimumCrawlHeight =
                    averageCrawlHeight *
                    lerp(0.20, 0.15, breakup01);
                float extraCrawlHeight =
                    averageCrawlHeight *
                    lerp(0.00, 2.24, lobeHeightContrast);
                float localCrawlHeight =
                    minimumCrawlHeight + extraCrawlHeight;

                // Smoothness controls the vertical dissolve length independently.
                float fadeLength =
                    (0.125 + tallness * 0.048 + sizeFactor * 0.028) *
                    lerp(1.10, 3.05, smoothness01) *
                    lerp(0.92, 1.32, saturate(broadNoise * 0.55 + sideNoiseA * 0.45));

                float verticalNoise = PS3D_ValueNoise31(float3(
                    sideCoord * 0.86 + sidePhase * 0.017,
                    warpedXZ.x * 0.38 + seed * 0.011,
                    height01 * 1.95 + seed * 0.029));
                float heightJitter =
                    (verticalNoise - 0.5) * lerp(0.050, 0.135, breakup01) +
                    (patchNoise - 0.5) * lerp(0.030, 0.082, breakup01) +
                    (planeNoise - 0.5) * lerp(0.018, 0.055, breakup01) +
                    (facetNoise - 0.5) * 0.028;
                float shiftedHeight = max(0.0, height01 + heightJitter);

                // Long-tail fade: avoid a single smoothstep contour. Higher local
                // crawl shifts the strongest part upward; smoothness widens the
                // dissolve without increasing crawl height.
                float fadeStart = localCrawlHeight * 0.16;
                float fadeDenominator = max(0.035, localCrawlHeight * 0.62 + fadeLength);
                float distanceT = max(0.0, shiftedHeight - fadeStart) / fadeDenominator;
                float falloffShape = lerp(1.05, 0.56, smoothness01);
                float falloffRate = lerp(4.25, 1.55, smoothness01);
                float lowerFade =
                    exp2(-pow(max(0.0, distanceT), falloffShape) * falloffRate);

                lowerFade *= lobePresence;
                lowerFade *= 1.0 - smoothstep(0.68, 0.94, height01);

                float contactAnchor =
                    (1.0 - smoothstep(0.0, 0.010 + tallness * 0.003, height01)) *
                    lerp(0.034, 0.056, lobePresence);

                return saturate(max(lowerFade, contactAnchor));
            }

            float ResolveGeneratedMassMottleNoise(Varyings input)
            {
                float scale = max(0.05, _StoneMottleScale);
                float broad =
                    ResolveGeneratedMassSoftPatchNoise(input, scale * 0.48, 151.0);
                float middle =
                    ResolveGeneratedMassSoftPatchNoise(input, scale * 0.94, 197.0);
                float small =
                    ResolveGeneratedMassPatchNoise(input, scale * 1.82, 233.0);
                float raw = saturate(
                    broad * 0.56 +
                    middle * 0.32 +
                    small * 0.12);

                // Higher softness keeps the mottle as broad material variation;
                // lower softness exaggerates the same field for validation.
                float contrast = lerp(1.90, 0.78, saturate(_StoneMottleSoftness));
                return saturate((raw - 0.5) * contrast + 0.5);
            }

            half3 ApplyGeneratedMassSurfaceMottle(
                half3 albedo,
                Varyings input,
                float generatedMassSurface,
                float exposureVisual,
                float creviceVisual,
                float baseVisual,
                float dirtDepositVisual,
                float wetness,
                float frostStrength,
                float monolithicFlatten)
            {
                float strength =
                    saturate(_StoneMottleStrength) *
                    saturate(generatedMassSurface) *
                    // Frost and monolithic profiles should reduce mottle, not
                    // erase it completely. Patch 13B keeps some broad stone
                    // breakup visible so these profiles do not collapse into
                    // smooth artificial material slabs.
                    lerp(1.0, 0.68, saturate(frostStrength)) *
                    lerp(1.0, 0.58, saturate(monolithicFlatten));

                if (strength <= 0.0001)
                {
                    return albedo;
                }

                float mottle = ResolveGeneratedMassMottleNoise(input);
                float signedMottle = (mottle - 0.5) * 2.0;
                float shelterBias = saturate(_StoneMottleShelterBias);
                float shelterMask = saturate(
                    creviceVisual * 0.36 +
                    baseVisual * 0.28 +
                    dirtDepositVisual * 0.52 +
                    (1.0 - exposureVisual) * 0.14);

                // Broad face breakup should remain value-based so neutral grey
                // rocks do not regain unwanted hue drift. Shelter bias only
                // increases the darker gathered component in existing semantic
                // dirt/base/crevice zones.
                float broadValueScale =
                    1.0 +
                    signedMottle *
                    strength *
                    lerp(0.070, 0.045, shelterBias);
                float gatheredDarkMask = saturate(
                    (1.0 - mottle) *
                    lerp(0.26, 0.54 + shelterMask * 0.78, shelterBias));
                float gatheredDarken =
                    gatheredDarkMask *
                    strength *
                    lerp(0.055, 0.185, shelterBias) *
                    lerp(1.0, 1.22, saturate(wetness));

                float valueScale = max(0.0, broadValueScale - gatheredDarken);
                return albedo * (half)valueScale;
            }

            float ResolveGeneratedMassOrganicBottomMask(Varyings input)
            {
                // Keep value shaping from rebuilding a separate horizontal band.
                // This only lightly follows the same side-aware lobe field.
                return saturate(ResolveGeneratedMassOrganicLowerFade(input) * 0.20);
            }

            float ResolveShaderCreviceBaseMask(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float normalY = normalize((float3)input.normalOS).y;
                float up = saturate(normalY);
                float downward = saturate(-normalY * 1.10);
                float sideFacing = 1.0 - smoothstep(0.16, 0.92, abs(normalY));
                float notUpward = 1.0 - smoothstep(0.10, 0.56, up);
                float tallness = ResolveGeneratedMassTallnessFactor();

                float broadNoise = ResolveGeneratedMassSoftPatchNoise(input, 0.92, 19.0);
                float patchNoise = ResolveGeneratedMassSoftPatchNoise(input, 1.42, 31.0);
                float planeNoise = ResolveGeneratedMassPatchNoise(input, 2.05, 37.0);
                float facetNoise = saturate(
                    (float)input.color.r * 0.46 +
                    broadNoise * 0.20 +
                    patchNoise * 0.17 +
                    planeNoise * 0.17);

                float lowerFade = ResolveGeneratedMassOrganicLowerFade(input);
                float shelter = saturate(sideFacing * 0.50 + notUpward * 0.14 + downward * 0.14);
                float shelterBlend = lerp(0.66, 1.00, smoothstep(0.16, 0.82, shelter));
                float facetAttenuation = lerp(0.58, 1.02, saturate(facetNoise * 0.72 + broadNoise * 0.28));
                float contactAccent =
                    (1.0 - smoothstep(0.0, 0.012 + tallness * 0.003, height01)) * 0.045;

                float mask = lowerFade * shelterBlend * facetAttenuation;
                mask = max(mask, contactAccent);
                return saturate(mask);
            }

            float ResolveShaderDirtDepositMask(Varyings input)
            {
                float height01 = ResolveGeneratedMassHeight01(input);
                float normalY = normalize((float3)input.normalOS).y;
                float up = saturate(normalY);
                float downward = saturate(-normalY * 1.22);
                float sideFacing = 1.0 - smoothstep(0.24, 0.92, abs(normalY));
                float notUpward = 1.0 - smoothstep(0.09, 0.60, up);
                float depositShelter = saturate(
                    sideFacing * 0.74 +
                    downward * 0.24 +
                    notUpward * 0.18);

                float tallness = ResolveGeneratedMassTallnessFactor();
                float dirtReach = max(0.05, _GeneratedMassDirtCrawlReach);
                float dirtCoverage = max(0.05, _GeneratedMassDirtCoverage);
                float dirtCoverageDelta = clamp(dirtCoverage - 1.0, -0.75, 1.0);
                float dirtCoverageMultiplier = clamp(dirtCoverage, 0.35, 1.45);
                float xzScale = max(0.0001, _GeneratedMassLocalXZScale);
                float2 normalizedXZ = input.positionOS.xz / xzScale;
                float seed = _GeneratedMassMaskSeed;

                float lowNoise =
                    ResolveGeneratedMassSoftPatchNoise(input, 0.88, 47.0);
                float mediumNoise =
                    ResolveGeneratedMassPatchNoise(input, 2.80, 71.0);
                float highNoise =
                    ResolveGeneratedMassPatchNoise(input, 7.60, 113.0);

                float skeletonWaveA =
                    1.0 - smoothstep(
                        0.18,
                        0.56,
                        abs(sin(normalizedXZ.x * 10.5 + lowNoise * 2.4 + seed * 0.041)));
                float skeletonWaveB =
                    1.0 - smoothstep(
                        0.14,
                        0.52,
                        abs(sin((normalizedXZ.x * 0.62 + normalizedXZ.y * 0.91) * 8.8 + mediumNoise * 1.8 + seed * 0.067)));
                float crawlSkeleton = saturate(max(skeletonWaveA * 0.82, skeletonWaveB * 0.70));
                crawlSkeleton *= smoothstep(0.30, 0.82, lowNoise * 0.56 + mediumNoise * 0.44);

                float crawlHeight =
                    (0.070 + 0.305 * pow(lowNoise, 1.42) +
                    crawlSkeleton * 0.070) *
                    dirtReach;
                crawlHeight = min(
                    crawlHeight + tallness * 0.040 * dirtReach,
                    clamp(0.48 * dirtReach, 0.12, 0.86));
                float connectedCrawl =
                    1.0 - smoothstep(
                        crawlHeight,
                        crawlHeight + 0.086,
                        height01);

                float baseConnection =
                    1.0 - smoothstep(
                        0.0,
                        0.070 + tallness * 0.010,
                        height01);
                float heightTaper =
                    1.0 - smoothstep(
                        crawlHeight * 0.62,
                        crawlHeight + 0.055,
                        height01);

                float erosion =
                    smoothstep(
                        saturate(0.34 - dirtCoverageDelta * 0.12),
                        saturate(0.72 - dirtCoverageDelta * 0.10),
                        mediumNoise * 0.50 + highNoise * 0.34 + lowNoise * 0.16);
                float fineBreakup = lerp(0.62, 1.06, highNoise);
                float skeletonCoverage = lerp(0.42, 1.00, crawlSkeleton);

                float rimCore =
                    1.0 - smoothstep(0.0, 0.050 + tallness * 0.010, height01);
                float rimBreakup = smoothstep(
                    0.30,
                    0.68,
                    mediumNoise * 0.50 + highNoise * 0.30 + lowNoise * 0.20);
                float brokenRim =
                    rimCore * rimBreakup * lerp(0.46, 0.96, depositShelter);

                float crawlDeposit =
                    connectedCrawl *
                    heightTaper *
                    erosion *
                    fineBreakup *
                    skeletonCoverage *
                    depositShelter;

                float upperSuppress = smoothstep(0.46, 0.66, height01);
                float mask = max(
                    baseConnection * rimBreakup * 0.30 * dirtCoverageMultiplier,
                    brokenRim * 0.54 * dirtCoverageMultiplier);
                mask = max(mask, crawlDeposit * 0.76 * dirtCoverageMultiplier);
                mask *= 1.0 - upperSuppress;
                return saturate(pow(mask, 1.06));
            }
            float3 ResolveGeneratedMassWholeSurfaceNormalWS(
                Varyings input,
                float3 baseNormalWS)
            {
                float strength = saturate(_GeneratedMassSurfaceNormalStrength);
                if (strength <= 0.0001)
                {
                    return normalize(baseNormalWS);
                }

                float scale = max(0.05, _GeneratedMassSurfaceNormalScale);
                float3 broadCoordinate = ResolveGeneratedMassMaskCoordinate(
                    input,
                    scale * 0.72,
                    173.0);
                float3 mediumCoordinate = ResolveGeneratedMassMaskCoordinate(
                    input,
                    scale * 1.85,
                    241.0);
                float epsilon = 0.085;

                float3 tetraA = float3( epsilon,  epsilon,  epsilon);
                float3 tetraB = float3( epsilon, -epsilon, -epsilon);
                float3 tetraC = float3(-epsilon,  epsilon, -epsilon);
                float3 tetraD = float3(-epsilon, -epsilon,  epsilon);

                float b0 = PS3D_ValueNoise31(broadCoordinate + tetraA);
                float b1 = PS3D_ValueNoise31(broadCoordinate + tetraB);
                float b2 = PS3D_ValueNoise31(broadCoordinate + tetraC);
                float b3 = PS3D_ValueNoise31(broadCoordinate + tetraD);
                float3 broadGradient = float3(
                    b0 + b1 - b2 - b3,
                    b0 - b1 + b2 - b3,
                    b0 - b1 - b2 + b3) / max(0.0001, 4.0 * epsilon);

                float m0 = PS3D_ValueNoise31(mediumCoordinate + tetraA);
                float m1 = PS3D_ValueNoise31(mediumCoordinate + tetraB);
                float m2 = PS3D_ValueNoise31(mediumCoordinate + tetraC);
                float m3 = PS3D_ValueNoise31(mediumCoordinate + tetraD);
                float3 mediumGradient = float3(
                    m0 + m1 - m2 - m3,
                    m0 - m1 + m2 - m3,
                    m0 - m1 - m2 + m3) / max(0.0001, 4.0 * epsilon);

                float3 gradientOS = broadGradient * 0.78 + mediumGradient * 0.38;
                float3 normalOS = normalize((float3)input.normalOS);
                float3 tangentGradientOS =
                    gradientOS - normalOS * dot(gradientOS, normalOS);

                // Strength is true slope amplitude. The top of the Inspector range
                // is deliberately exaggerated so 0 and 1 cannot look identical.
                float slopeAmplitude = strength * 1.85;
                float3 perturbedNormalOS = normalize(
                    normalOS - tangentGradientOS * slopeAmplitude);
                return normalize(TransformObjectToWorldNormal(perturbedNormalOS));
            }

#endif // PS3D_PIXELSURFACEGENERATEDMASSCORE_HLSL
