#ifndef PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
#define PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL

            float ResolveSurfaceContractIsGround()
            {
                return step(0.5, _SurfaceContract);
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
                return saturate(input.materialMasks.y);
            }

            float ResolveGroundRockyDryMask(Varyings input)
            {
                return saturate(input.materialMasks.z);
            }

            float ResolveGroundStandingWaterPotentialMask(Varyings input)
            {
                return saturate(input.materialMasks.w);
            }

#if defined(PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES)
            float3 ResolveGroundPaintedAccentSegmentSample(
                float2 samplePoint,
                float2 startPoint,
                float2 endPoint,
                float startT,
                float endT)
            {
                float2 segment = endPoint - startPoint;
                float segmentLengthSquared = max(0.0001, dot(segment, segment));
                float t = saturate(dot(samplePoint - startPoint, segment) / segmentLengthSquared);
                float2 closestPoint = startPoint + segment * t;
                float2 delta = samplePoint - closestPoint;
                float distanceToSegment = length(delta);
                float2 tangent = segment * rsqrt(segmentLengthSquared);
                float2 normal = float2(-tangent.y, tangent.x);
                float signedSide = dot(delta, normal);
                float globalT = lerp(startT, endT, t);
                return float3(distanceToSegment, signedSide, globalT);
            }

            float3 ResolveGroundPaintedAccentCurvedStroke(
                float2 positionXZ,
                float microCellSize,
                float seed,
                float contrast,
                float densityScale,
                float directionBias,
                float2 preferredDirection,
                float layerOffset)
            {
                float safeCellSize = max(0.45, microCellSize);
                float2 cellCoordinate =
                    (positionXZ + layerOffset * safeCellSize) / safeCellSize;
                float2 cell = floor(cellCoordinate);
                float2 local = frac(cellCoordinate);
                float3 hashBase = float3(cell, seed + layerOffset * 17.73);

                float densityHash = PS3D_Hash31(hashBase + 13.17);
                if (densityHash > densityScale)
                {
                    return float3(0.0, 0.0, 0.0);
                }

                // Painted accent lines are now treated as small curved terrain
                // fold strokes. Recipe scale controls spacing; the stroke size
                // itself is capped in world units so variants cannot produce
                // giant bars or crescent objects.
                float lengthHalf = lerp(
                    0.16,
                    0.46,
                    PS3D_Hash31(hashBase + 83.19));
                lengthHalf *= lerp(0.82, 1.10, saturate(contrast));
                lengthHalf = min(lengthHalf, safeCellSize * 0.34);

                float thickness = lerp(
                    0.018,
                    0.052,
                    PS3D_Hash31(hashBase + 97.43));
                thickness *= lerp(0.88, 1.15, saturate(contrast));
                thickness = min(thickness, safeCellSize * 0.065);

                float randomAngle =
                    PS3D_Hash31(hashBase + 67.11) * 6.2831853;
                float2 randomDirection = float2(
                    cos(randomAngle),
                    sin(randomAngle));
                float2 direction = normalize(
                    lerp(randomDirection, preferredDirection, directionBias));
                float2 crossDirection = float2(-direction.y, direction.x);

                // Keep the full curve inside its cell. Curved strokes need extra
                // margin because their control points wander sideways.
                float curveReach = lengthHalf + thickness * 5.0;
                float centerMargin = saturate(curveReach / safeCellSize + 0.12);
                centerMargin = min(centerMargin, 0.40);
                float2 center = float2(
                    lerp(
                        centerMargin,
                        1.0 - centerMargin,
                        PS3D_Hash31(hashBase + 29.31)),
                    lerp(
                        centerMargin,
                        1.0 - centerMargin,
                        PS3D_Hash31(hashBase + 41.73)));

                float2 localSamplePoint = (local - center) * safeCellSize;
                float along = dot(localSamplePoint, direction);
                float across = dot(localSamplePoint, crossDirection);
                float2 localPoint = float2(along, across);

                float curveAmplitude = lengthHalf * lerp(
                    0.12,
                    0.42,
                    saturate(contrast));
                curveAmplitude *= lerp(
                    0.70,
                    1.20,
                    PS3D_Hash31(hashBase + 91.83));

                // Five local control points form a short irregular curve. This is
                // intentionally more chaotic than a single sine bend: the target
                // is a hand-painted turf/mound crease, not a mathematical arc.
                float o0 = (PS3D_Hash31(hashBase + 201.11) * 2.0 - 1.0) * curveAmplitude * 0.18;
                float o1 = (PS3D_Hash31(hashBase + 213.37) * 2.0 - 1.0) * curveAmplitude;
                float o2 = (PS3D_Hash31(hashBase + 229.71) * 2.0 - 1.0) * curveAmplitude * 1.15;
                float o3 = (PS3D_Hash31(hashBase + 241.19) * 2.0 - 1.0) * curveAmplitude;
                float o4 = (PS3D_Hash31(hashBase + 257.43) * 2.0 - 1.0) * curveAmplitude * 0.18;

                float2 p0 = float2(-lengthHalf, o0);
                float2 p1 = float2(-lengthHalf * 0.48, o1);
                float2 p2 = float2(0.0, o2);
                float2 p3 = float2(lengthHalf * 0.48, o3);
                float2 p4 = float2(lengthHalf, o4);

                float3 bestSample = ResolveGroundPaintedAccentSegmentSample(
                    localPoint,
                    p0,
                    p1,
                    0.0,
                    0.25);
                float3 sample1 = ResolveGroundPaintedAccentSegmentSample(
                    localPoint,
                    p1,
                    p2,
                    0.25,
                    0.5);
                if (sample1.x < bestSample.x)
                {
                    bestSample = sample1;
                }
                float3 sample2 = ResolveGroundPaintedAccentSegmentSample(
                    localPoint,
                    p2,
                    p3,
                    0.5,
                    0.75);
                if (sample2.x < bestSample.x)
                {
                    bestSample = sample2;
                }
                float3 sample3 = ResolveGroundPaintedAccentSegmentSample(
                    localPoint,
                    p3,
                    p4,
                    0.75,
                    1.0);
                if (sample3.x < bestSample.x)
                {
                    bestSample = sample3;
                }

                float endMask = smoothstep(0.02, 0.16, bestSample.z) *
                    (1.0 - smoothstep(0.84, 0.98, bestSample.z));
                float widthSoftness = lerp(
                    1.35,
                    0.62,
                    saturate(contrast)) * thickness;
                float lineMask = 1.0 - smoothstep(
                    thickness,
                    thickness + widthSoftness,
                    bestSample.x);

                float dashNoise = PS3D_ValueNoise31(float3(
                    bestSample.z * lerp(5.5, 11.0, saturate(contrast)) +
                        PS3D_Hash31(hashBase + 131.71) * 5.0,
                    cell.x * 0.37 + seed * 0.21,
                    cell.y * 0.41 + seed * 0.17));
                float chipNoise = PS3D_ValueNoise31(float3(
                    positionXZ / max(0.10, thickness * 6.2) + seed * 0.43,
                    seed + 151.31));
                float dashKeep = smoothstep(
                    lerp(0.18, 0.40, saturate(contrast)),
                    lerp(0.54, 0.78, saturate(contrast)),
                    dashNoise);
                float chipKeep = smoothstep(
                    lerp(0.10, 0.34, saturate(contrast)),
                    0.90,
                    chipNoise);
                float brokenMask = saturate(
                    lerp(0.38, 1.0, dashKeep) *
                    lerp(0.54, 1.0, chipKeep));

                float strokeMask = saturate(
                    lineMask * endMask * brokenMask);

                // A wider soft body gives the line a tiny implied terrain fold.
                // The signed side is later used for painted shadow/highlight; it
                // is visual relief only, not mesh displacement.
                float reliefBody = 1.0 - smoothstep(
                    thickness * 1.8,
                    thickness * 5.4,
                    bestSample.x);
                reliefBody *= endMask;
                float side = bestSample.y / max(0.0001, abs(bestSample.y));
                float signedRelief = side * reliefBody * brokenMask;

                return float3(strokeMask, reliefBody * brokenMask, signedRelief);
            }

            float3 ResolveGroundPaintedAccentLineReliefFeature(
                Varyings input,
                float exposureMask,
                float dampDepositMask,
                float vegetationMask,
                float compactionMask,
                float shoreMask,
                float rockyDryMask,
                float contractMask)
            {
                if (_GroundPaintedAccentLineStrength <= 0.0001)
                {
                    return float3(0.0, 0.0, 0.0);
                }

                float groupScale = max(1.0, _GroundPaintedAccentLineScale);
                float contrast = saturate(_GroundPaintedAccentLineContrast);
                float strength = saturate(_GroundPaintedAccentLineStrength);
                float maskInfluence = saturate(_GroundPaintedAccentLineMaskInfluence);
                float seed = _PixelSeed * 0.029 +
                    _GroundPaintedAccentLineSeed * 0.113;
                float2 positionXZ = input.positionWS.xz;

                float2 preferredDirection =
                    _GroundPaintedAccentLineDirection.xy;
                if (dot(preferredDirection, preferredDirection) < 0.0001)
                {
                    preferredDirection = float2(1.0, 0.0);
                }
                preferredDirection = normalize(preferredDirection);

                float clusterScale = max(1.0, groupScale * 1.36);
                float clusterNoise = PS3D_ValueNoise31(float3(
                    positionXZ / clusterScale + seed * 0.19,
                    seed + 211.31));
                float clusterThreshold = lerp(
                    0.58,
                    0.34,
                    saturate(maskInfluence * 0.65 + strength * 0.45));
                float clusterGate = smoothstep(
                    clusterThreshold,
                    clusterThreshold + 0.24,
                    clusterNoise);

                float semanticGate = saturate(
                    0.16 +
                    exposureMask * 0.08 +
                    dampDepositMask * 0.24 +
                    vegetationMask * 0.34 +
                    compactionMask * 0.42 +
                    shoreMask * 0.10 +
                    rockyDryMask * 0.07);
                float maskGate = lerp(
                    1.0,
                    semanticGate,
                    maskInfluence);

                // Scale controls the spacing of accent groups. Smaller derived
                // cells create several small curve candidates instead of one
                // large bar per group.
                float microCellSizeA = max(0.52, groupScale * 0.23);
                float microCellSizeB = max(0.46, groupScale * 0.18);
                float baseDensity = lerp(0.24, 0.46, contrast) *
                    lerp(0.84, 1.24, strength) *
                    lerp(0.82, 1.18, maskInfluence);
                baseDensity = saturate(baseDensity * lerp(0.42, 1.22, clusterGate));
                float directionBias = lerp(0.08, 0.28, maskInfluence);

                float3 strokeA = ResolveGroundPaintedAccentCurvedStroke(
                    positionXZ,
                    microCellSizeA,
                    seed + 17.11,
                    contrast,
                    baseDensity,
                    directionBias,
                    preferredDirection,
                    0.0);
                float3 strokeB = ResolveGroundPaintedAccentCurvedStroke(
                    positionXZ,
                    microCellSizeB,
                    seed + 73.47,
                    contrast,
                    baseDensity * 0.74,
                    directionBias * 0.78,
                    preferredDirection,
                    0.37);

                float lineMask = saturate(max(strokeA.x, strokeB.x * 0.74));
                float reliefBody = saturate(max(strokeA.y, strokeB.y * 0.70));
                float signedRelief = strokeA.z;
                if (abs(strokeB.z) * 0.70 > abs(signedRelief))
                {
                    signedRelief = strokeB.z * 0.70;
                }

                float finalGate = clusterGate * maskGate * strength * contractMask;
                return float3(
                    saturate(lineMask * finalGate),
                    saturate(reliefBody * finalGate),
                    clamp(signedRelief * finalGate, -1.0, 1.0));
            }

            float ResolveGroundPaintedAccentLinesFeature(
                Varyings input,
                float exposureMask,
                float dampDepositMask,
                float vegetationMask,
                float compactionMask,
                float shoreMask,
                float rockyDryMask,
                float contractMask)
            {
                return ResolveGroundPaintedAccentLineReliefFeature(
                    input,
                    exposureMask,
                    dampDepositMask,
                    vegetationMask,
                    compactionMask,
                    shoreMask,
                    rockyDryMask,
                    contractMask).x;
            }
#endif // PS3D_PIXELSURFACEGROUND_MATERIAL_PROPERTIES

#endif // PS3D_PIXELSURFACEGROUNDRESPONSE_HLSL
