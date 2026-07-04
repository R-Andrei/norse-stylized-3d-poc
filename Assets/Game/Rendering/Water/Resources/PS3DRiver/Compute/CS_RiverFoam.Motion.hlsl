float2 ResolveMaterialVelocity(float2 fieldUV)
{
    float2 wakeGradient = float2(0.0, 0.0);
    float2 leeGradient = float2(0.0, 0.0);
    float2 pressureGradient = float2(0.0, 0.0);

    if (_FoamDisturbanceEnabled > 0.5)
    {
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

        pressureGradient = SampleStaticPressureBilinear(fieldUV).gb;
    }

    float2 wakeVelocity =
        (wakeGradient * 0.42 + leeGradient * 1.35) *
        max(0.0.xx, _FoamWakeMotionInfluence);
    float2 pressureVelocity =
        pressureGradient * max(0.0.xx, _FoamPressureMotionInfluence);
    float2 disturbanceVelocity = wakeVelocity + pressureVelocity;

    // Patch 4.11C.5.2d: base downstream travel is no longer solved by
    // fractional finite-volume advection. A persistent render phase and
    // integer texture commits own that motion. This path retains only the
    // accepted disturbance-derived material velocity so pressure / lee fields
    // may still deform material without reintroducing the deleted guidance
    // network, shore suction, or lane attraction.
    float downstreamSign = _FoamFlowDirection < 0.0 ? -1.0 : 1.0;
    float rawAlong = disturbanceVelocity.x;
    float alongVelocity = downstreamSign * max(
        0.0,
        rawAlong * downstreamSign);

    return float2(alongVelocity, disturbanceVelocity.y);
}

float2 FoamPixelCoordinateToUV(float2 pixelCoordinate)
{
    return saturate(
        (pixelCoordinate + 0.5) /
        max(float2(1.0, 1.0), (float2)_FoamDimensions));
}

float FoamClampFaceVelocity(
    float velocity,
    float minimumSpacing)
{
    float maximumMagnitude =
        max(0.0, _FoamTransportMaximumAxisCourant) *
        max(0.0001, minimumSpacing) /
        max(0.0001, _FoamDeltaTime);
    return clamp(velocity, -maximumMagnitude, maximumMagnitude);
}
