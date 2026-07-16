
struct FoamSourceEventData
{
    // x = source type, y = side sign except Object Arc/Semi-Arc phase
    // (0 Build, 1 Hold, 2 Release), z = phase/reveal progress, w = shape seed.
    float4 header;
    // x/y = start/end storage global except Object Arc/Semi-Arc point 0;
    // z = centre storage global; w = flow direction except Object Arc/Semi-Arc point 1.x.
    float4 distance;
    // x = shore inset except Object Arc/Semi-Arc point 1.y; y = width metres
    // except Shore Ribbon thickness cells and Object Arc/Semi-Arc wake-arm length;
    // z = inward reach or Arc/Semi-Arc material-step duration; w = feather or point 2.x.
    float4 shore;
    // x = source amount, y = remaining life, z = material pattern seed,
    // w = source fill feature size.
    float4 material;
    // x = source fill seed except Object Arc/Semi-Arc negative-half
    // first-segment split; y/z = breakup scale/strength except Object
    // Arc/Semi-Arc point 2.y / point 3.x; w = curvature or selected Semi-Arc side.
    float4 variation;
    // x/y = formation speed / moving-head trail except Object Arc/Semi-Arc
    // point 3.y / point 4.x; z = source path length metres; w = source fill blend
    // except Object Arc/Semi-Arc positive-half first-segment split.
    float4 kinematics;
    // x = object centre lateral metres; y/z = object half extents except
    // Object Arc/Semi-Arc point 4.y / front split; w = Fleck contact offset or
    // Arc/Semi-Arc source-local lateral cell spacing metres.
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
