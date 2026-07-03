
FoamMotionSample ResolveMotion(float2 pixelCoordinate, float2 fieldUV)
{
    FoamMotionSample result;
    result.velocity = float2(0.0, 0.0);
    result.capture = 0.0;
    result.lane = 0.0;
    result.junction = 0.0;
    result.wakeEnergy = 0.0;
    result.leeStrength = 0.0;
    result.pressureStrength = 0.0;
    result.rippleIntensity = 0.0;

    float4 guidance = SampleGuidanceBilinear(fieldUV);
    float2 directionToLane = guidance.xy;
    float laneStrength = saturate(guidance.z);
    float junctionStrength = saturate(guidance.w);
    result.lane = laneStrength;
    result.junction = junctionStrength;

    float2 boundary = SampleBoundaryBilinear(pixelCoordinate);
    float2 boundaryLeft = SampleBoundaryBilinear(
        pixelCoordinate + float2(-1.0, 0.0));
    float2 boundaryRight = SampleBoundaryBilinear(
        pixelCoordinate + float2(1.0, 0.0));
    float2 boundaryDown = SampleBoundaryBilinear(
        pixelCoordinate + float2(0.0, -1.0));
    float2 boundaryUp = SampleBoundaryBilinear(
        pixelCoordinate + float2(0.0, 1.0));
    float2 boundaryGradient = float2(
        boundaryRight.y - boundaryLeft.y,
        boundaryUp.y - boundaryDown.y) * 0.5;

    float4 wakeState = 0.0.xxxx;
    float4 rippleState = 0.0.xxxx;
    float4 staticWakeState = 0.0.xxxx;
    float4 pressureState = 0.0.xxxx;
    float2 wakeGradient = float2(0.0, 0.0);
    float2 leeGradient = float2(0.0, 0.0);
    float2 pressureGradient = float2(0.0, 0.0);

    if (_FoamDisturbanceEnabled > 0.5)
    {
        wakeState = SampleWakeBilinear(fieldUV);
        rippleState = SampleRippleBilinear(fieldUV);
        staticWakeState = SampleStaticWakeBilinear(fieldUV);
        pressureState = SampleStaticPressureBilinear(fieldUV);

        float2 wakeTexel = 1.0 /
            max(float2(2.0, 2.0), (float2)_FoamWakeDimensions);
        wakeGradient = float2(
            SampleWakeBilinear(saturate(
                fieldUV + float2(wakeTexel.x, 0.0))).x -
            SampleWakeBilinear(saturate(
                fieldUV - float2(wakeTexel.x, 0.0))).x,
            SampleWakeBilinear(saturate(
                fieldUV + float2(0.0, wakeTexel.y))).x -
            SampleWakeBilinear(saturate(
                fieldUV - float2(0.0, wakeTexel.y))).x) * 0.5;

        float2 leeTexel = 1.0 /
            max(float2(2.0, 2.0), (float2)_FoamStaticWakeDimensions);
        leeGradient = float2(
            SampleStaticWakeBilinear(saturate(
                fieldUV + float2(leeTexel.x, 0.0))).g -
            SampleStaticWakeBilinear(saturate(
                fieldUV - float2(leeTexel.x, 0.0))).g,
            SampleStaticWakeBilinear(saturate(
                fieldUV + float2(0.0, leeTexel.y))).g -
            SampleStaticWakeBilinear(saturate(
                fieldUV - float2(0.0, leeTexel.y))).g) * 0.5;

        pressureGradient = pressureState.gb;
    }

    float agitation = saturate(_FoamEvolution / 1.8);
    float guideStrength = max(0.0, _FoamGuidanceStrength);
    float phaseWander = FoamFbm(
        fieldUV * float2(
            max(6.0, (float)_FoamChunkCount * 2.3),
            11.0) +
        float2(
            _FoamTime * lerp(0.035, 0.12, agitation),
            -_FoamTime * lerp(0.028, 0.095, agitation)));
    float2 guideTangent = float2(-directionToLane.y, directionToLane.x);

    float2 guidanceVelocity =
        directionToLane *
            guideStrength *
            laneStrength *
            lerp(0.40, 1.15, agitation) +
        guideTangent *
            (phaseWander - 0.5) *
            guideStrength *
            agitation *
            0.32;

    float boundaryStrength =
        max(0.0, _FoamBoundaryAttraction);
    float2 boundaryVelocity =
        boundaryGradient *
        boundaryStrength *
        lerp(0.85, 2.20, boundary.y);

    float2 wakeVelocity =
        (wakeGradient * 0.42 + leeGradient * 1.35) *
        max(0.0, _FoamWakeReinforcement);
    float2 pressureVelocity =
        pressureGradient *
        max(0.0, _FoamBoundaryAttraction) *
        0.08;
    float impactInfluence = max(0.0, _FoamImpactReinforcement);
    float2 rippleVelocity = float2(
        -rippleState.b,
        -rippleState.a) *
        impactInfluence *
        0.055;

    float wakeEnergy = max(0.0, wakeState.x);
    float leeStrength = saturate(staticWakeState.g);
    float pressureStrength = saturate(abs(pressureState.r) / 0.18);
    float rippleIntensity = saturate(
        abs(rippleState.x) * 4.0 +
        abs(rippleState.y) * 0.12 +
        length(rippleState.ba) * 0.38) *
        saturate(impactInfluence);

    float capture = saturate(
        boundary.y * boundaryStrength * 1.35 +
        leeStrength * 1.85 +
        pressureStrength * 0.48);

    float downstreamSign = _FoamFlowSpeed < -0.0001 ? -1.0 : 1.0;
    float rawAlong = _FoamFlowSpeed +
        guidanceVelocity.x * 0.33 +
        boundaryVelocity.x * 0.26 +
        wakeVelocity.x * 0.24 +
        pressureVelocity.x * 0.18 +
        rippleVelocity.x;
    float downstreamMagnitude = max(0.0, rawAlong * downstreamSign);
    float captureSlowdown = lerp(
        1.0,
        0.045,
        pow(capture, 1.35));
    float alongVelocity =
        downstreamSign *
        downstreamMagnitude *
        captureSlowdown;

    float lateralVelocity =
        (
            guidanceVelocity.y * 0.96 +
            boundaryVelocity.y * 1.12 +
            wakeVelocity.y * 0.78 +
            pressureVelocity.y * 0.55 +
            rippleVelocity.y
        ) *
        lerp(0.72, 1.28, saturate(_FoamSpread));

    result.velocity = float2(alongVelocity, lateralVelocity);
    result.capture = capture;
    result.wakeEnergy = wakeEnergy;
    result.leeStrength = leeStrength;
    result.pressureStrength = pressureStrength;
    result.rippleIntensity = rippleIntensity;
    return result;
}


void ResolveOriginalBounds(
    float2 pixelCoordinate,
    out float3 minimumValue,
    out float3 maximumValue)
{
    int2 baseCoordinate = int2(floor(clamp(
        pixelCoordinate,
        float2(0.0, 0.0),
        float2(
            (float)(_FoamDimensions.x - 1),
            (float)(_FoamDimensions.y - 1)))));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamDimensions - int2(1, 1));

    float3 a = LoadState(baseCoordinate).xyz;
    float3 b = LoadState(int2(nextCoordinate.x, baseCoordinate.y)).xyz;
    float3 c = LoadState(int2(baseCoordinate.x, nextCoordinate.y)).xyz;
    float3 d = LoadState(nextCoordinate).xyz;
    minimumValue = min(min(a, b), min(c, d));
    maximumValue = max(max(a, b), max(c, d));
}
