#ifndef PS3D_RIVER_WATER_FOAM_VELOCITY_INCLUDED
#define PS3D_RIVER_WATER_FOAM_VELOCITY_INCLUDED

// Canonical Foam velocity contract. The obstacle texture now carries signed
// lateral-routing influence in R and independent contact-slowdown influence in
// G. This keeps the one-sided collision route separate from the narrow all-side
// contact retention halo without adding a resource or sample. The resolved
// slowdown factor scales the complete routed velocity vector so full contact
// influence reduces downstream, lateral, and total speed consistently.
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
    float signedRoutingInfluence = clamp(obstacleIntent, -1.0, 1.0);
    float routingInfluence = abs(signedRoutingInfluence);
    float routingDirection = signedRoutingInfluence >= 0.0 ? 1.0 : -1.0;
    float lateral = clamp(
        lerp(lane, routingDirection, routingInfluence),
        -1.0,
        1.0);

    float speed = max(0.0, baseDownstreamSpeed);
    float lateralRatio = max(0.0, maximumLateralSpeedRatio);
    float slowdownField = saturate(obstacleInfluence);
    float slowdownFalloff = saturate(obstacleSlowdownStrength);
    float slowdownFieldSquared = slowdownField * slowdownField;
    float narrowSlowdown = slowdownFieldSquared * slowdownFieldSquared;
    float slowdown = slowdownFalloff > 0.0001
        ? lerp(narrowSlowdown, slowdownField, slowdownFalloff)
        : 0.0;
    float minimumFactor = saturate(obstacleMinimumDownstreamFactor);
    float contactSpeedFactor = lerp(1.0, minimumFactor, slowdown);
    float speedActive = speed > 0.0001 ? 1.0 : 0.0;
    float2 routedVelocity = float2(
        speed,
        lateral * speed * lateralRatio);

    resolved.velocityMetresPerSecond =
        routedVelocity * contactSpeedFactor * validity;
    resolved.lateralIntent = lateral * validity;
    resolved.downstreamSpeedFactor =
        contactSpeedFactor * speedActive * validity;
    resolved.obstacleInfluence = slowdownField * validity;
    resolved.laneIntent = lane * validity;
    resolved.obstacleIntent = signedRoutingInfluence * validity;
    return resolved;
}

#endif
