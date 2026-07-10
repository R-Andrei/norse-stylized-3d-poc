
struct FoamSourceEventData
{
    // x = source type, y = side sign, z = reveal progress, w = shape seed.
    float4 header;
    // x = start storage global, y = end storage global,
    // z = centre storage global, w = flow direction.
    float4 distance;
    // x = shore inset, y = width, z = inward reach, w = feather.
    float4 shore;
    // x = source amount, y = remaining life, z = material pattern seed,
    // w = source fill feature size.
    float4 material;
    // x = source fill seed, y = breakup scale,
    // z = breakup strength, w = curvature.
    float4 variation;
    // x = formation speed metres/second, y = moving-head trail metres,
    // z = source path length metres, w reserved.
    float4 kinematics;
    // x = object centre lateral metres, y = object along half length,
    // z = object across half width, w = object contact offset metres.
    float4 objectData;
};

struct FoamMetricRow
{
    // x = left surface half-width, y = right surface half-width,
    // z = longitudinal cell spacing, w = minimum lateral cell spacing.
    float4 widthsAndSpacing;
    // x = signed centreline curvature, y = width derivative,
    // z = left/right width asymmetry, w = valid-domain flag.
    float4 topologyData;
    // x = left normal visible half-width, y = right normal visible half-width,
    // z = base river surface height, w reserved.
    float4 shoreData;
};

struct FoamObstacleSample
{
    // x/y = first exact solid interval; z/w = optional second interval.
    float4 intervals;
    // x = global distance, y = lateral metres,
    // z = visible half-width, w = generated surface half-width.
    float4 waterParameters;
};

struct FoamObstacleIntervalCell
{
    // x/y = full-resolution Foam texel coordinate,
    // z = first of nine consecutive exact-mesh interval samples,
    // w = reserved.
    float4 coordinateAndOffset;
};

struct FoamMajorEvolutionData
{
    // x = local river distance, y = normalized lateral centre,
    // z = world orientation, w = metres per candidate cell.
    float4 centreAndPlacement;
    // x/y = candidate centroid cells, z = candidate principal angle,
    // w = mask-array slice.
    float4 candidateShape;
    // x/y = candidate principal half-extents, z = mask resolution,
    // w = support normalization.
    float4 candidateExtents;
    // x/y = along/across scale, z = shear, w reserved.
    float4 morph;
    // x/y = warp amplitudes, z/w = warp phases.
    float4 warp;
};

struct FoamHostedNegativeEvolutionData
{
    // x = host Major slot, y = mask-array slice,
    // z = pressure normalization, w = class flag.
    float4 hostAndMask;
    // x/y = immutable mask centre in host candidate cells,
    // z/w = bounded local offset in the same frame.
    float4 centreAndOffset;
    // x/y = local scale, z = local rotation, w reserved.
    float4 morph;
};

struct FoamFreeWaterEvolutionData
{
    // x = local river distance, y = normalized lateral centre,
    // z = orientation, w = metres per local mask cell.
    float4 centreAndPlacement;
    // x/y = local mask centre, z = mask-array slice, w = pressure scale.
    float4 maskAndStrength;
    // x/y = bounded metric offset, z/w = along/across scale.
    float4 morph;
};

struct FoamConnectorIdentityData
{
    // x = first flattened path point, y = point count,
    // z = outer radius, w = core radius.
    float4 pointRangeAndRadii;
};

struct FoamWeakSpanIdentityData
{
    // x = Connector record index, y = normalized path distance,
    // z/w = gate-safe normalized interval.
    float4 connectorAndPath;
    // x/y = physical along/across radii, z = pressure strength,
    // w = accepted identity orientation.
    float4 shape;
    // x = deterministic irregular-boundary noise seed.
    uint4 noiseAndFlags;
};
