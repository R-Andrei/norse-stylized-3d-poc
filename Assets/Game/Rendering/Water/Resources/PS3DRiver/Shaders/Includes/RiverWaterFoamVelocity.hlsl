#ifndef PS3D_RIVER_WATER_FOAM_VELOCITY_INCLUDED
#define PS3D_RIVER_WATER_FOAM_VELOCITY_INCLUDED

// Patch 4.11C.5.16A canonical Foam velocity contract. Raw lane intent and
// obstacle routing remain separate inputs because the lane sample scrolls while
// obstacle routing stays fixed in river space. Every consumer resolves those
// inputs through this pure function so Layer C transport, Layer D advection,
// diagnostics, and future strain calculations share one physical velocity.
struct RiverWaterFoamResolvedVelocity
{
    // x = nonnegative downstream speed magnitude in metres/second.
    // y = signed lateral speed in metres/second.
    float2 velocityMetresPerSecond;
    float lateralIntent;
    float downstreamSpeedFactor;
    float obstacleInfluence;
    float laneIntent;
    float obstacleIntent;
};

RiverWaterFoamResolvedVelocity RiverWaterResolveFoamVelocityContract(
    float laneIntent,
    float obstacleIntent,
    float obstacleInfluence,
    float baseDownstreamSpeed,
    float maximumLateralSpeedRatio,
    float obstacleSlowdownStrength,
    float obstacleMinimumDownstreamFactor,
    float validFluid)
{
    RiverWaterFoamResolvedVelocity resolved;
    resolved.velocityMetresPerSecond = 0.0.xx;
    resolved.lateralIntent = 0.0;
    resolved.downstreamSpeedFactor = 0.0;
    resolved.obstacleInfluence = 0.0;
    resolved.laneIntent = 0.0;
    resolved.obstacleIntent = 0.0;

    float validity = saturate(validFluid);
    if (validity <= 0.0001)
    {
        return resolved;
    }

    float lane = clamp(laneIntent, -1.0, 1.0);
    float obstacle = clamp(obstacleIntent, -1.0, 1.0);
    float influence = saturate(obstacleInfluence);
    float lateral = clamp(lerp(lane, obstacle, influence), -1.0, 1.0);

    float speed = max(0.0, baseDownstreamSpeed);
    float lateralRatio = max(0.0, maximumLateralSpeedRatio);
    float slowdown = saturate(
        influence * saturate(obstacleSlowdownStrength));
    float minimumFactor = saturate(obstacleMinimumDownstreamFactor);
    float downstreamFactor = lerp(1.0, minimumFactor, slowdown);
    float speedActive = speed > 0.0001 ? 1.0 : 0.0;

    resolved.velocityMetresPerSecond = float2(
        max(0.0, speed * downstreamFactor),
        lateral * speed * lateralRatio) * validity;
    resolved.lateralIntent = lateral * validity;
    resolved.downstreamSpeedFactor =
        downstreamFactor * speedActive * validity;
    resolved.obstacleInfluence = influence * validity;
    resolved.laneIntent = lane * validity;
    resolved.obstacleIntent = obstacle * validity;
    return resolved;
}

#endif
