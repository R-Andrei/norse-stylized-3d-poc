#ifndef PS3D_VEGETATION_INTERACTION_FIELD_INCLUDED
#define PS3D_VEGETATION_INTERACTION_FIELD_INCLUDED

TEXTURE2D(_VegetationInteractionPreviousField);
SAMPLER(sampler_VegetationInteractionPreviousField);
TEXTURE2D(_VegetationInteractionCurrentField);
SAMPLER(sampler_VegetationInteractionCurrentField);

float4 _VegetationInteractionFieldOriginCellSize;
float4 _VegetationInteractionFieldResolutionOffset;
float4 _VegetationInteractionFieldTiming;

struct VegetationInteractionSample
{
    float2 bend;
    float flatten;
    float active;
};

float2 VegetationInteractionLogicalCellToUv(float2 logicalCell)
{
    float2 resolution = max(
        _VegetationInteractionFieldResolutionOffset.xy,
        1.0);
    float2 clampedLogical = clamp(
        logicalCell,
        0.0,
        resolution - 1.0);
    float2 physicalCell = clampedLogical +
        _VegetationInteractionFieldResolutionOffset.zw;
    return frac((physicalCell + 0.5) / resolution);
}

float VegetationInteractionFieldContains(float2 logicalCell)
{
    float2 resolution =
        _VegetationInteractionFieldResolutionOffset.xy;
    float2 minimumCheck = step(0.0, logicalCell);
    float2 maximumCheck = step(logicalCell, resolution - 1.0);
    return minimumCheck.x * minimumCheck.y *
        maximumCheck.x * maximumCheck.y;
}

float2 VegetationInteractionWorldToLogicalCell(float3 worldPosition)
{
    float cellSize = max(
        0.0001,
        _VegetationInteractionFieldOriginCellSize.z);
    return (
        worldPosition.xz -
        _VegetationInteractionFieldOriginCellSize.xy) /
        cellSize - 0.5;
}

VegetationInteractionSample SampleVegetationInteraction(
    float3 worldPosition)
{
    VegetationInteractionSample sample;
    sample.bend = 0.0;
    sample.flatten = 0.0;
    sample.active = 0.0;

    if (_VegetationInteractionFieldOriginCellSize.w < 0.5)
    {
        return sample;
    }

    float2 logicalCell =
        VegetationInteractionWorldToLogicalCell(worldPosition);
    float inside = VegetationInteractionFieldContains(logicalCell);
    float2 uv = VegetationInteractionLogicalCellToUv(logicalCell);
    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        _VegetationInteractionPreviousField,
        sampler_VegetationInteractionPreviousField,
        uv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        _VegetationInteractionCurrentField,
        sampler_VegetationInteractionCurrentField,
        uv,
        0.0);
    float interpolation = saturate(
        _VegetationInteractionFieldTiming.x);
    float4 state;
    if (currentState.w > 0.001)
    {
        state = lerp(
            previousState,
            currentState,
            interpolation);
    }
    else
    {
        float fixedStep = max(
            0.0,
            _VegetationInteractionFieldTiming.y);
        float recoveryTime = max(
            0.001,
            _VegetationInteractionFieldTiming.z);
        float renderAge = interpolation * fixedStep;
        float releaseDecay = exp(-renderAge / recoveryTime);
        state = currentState;
        state.xyz *= releaseDecay;
    }
    state *= inside;

    sample.bend = state.xy;
    sample.flatten = saturate(state.z);
    sample.active = inside;
    return sample;
}

float VegetationInteractionEffectiveStrength(
    VegetationInteractionSample interaction,
    float bendResponse,
    float flattenResponse)
{
    float bendStrength = saturate(
        length(interaction.bend) * max(0.0, bendResponse));
    float flattenStrength = saturate(
        interaction.flatten * max(0.0, flattenResponse));
    return max(bendStrength, flattenStrength);
}

float3 ApplyVegetationInteractionResponse(
    float3 worldPosition,
    VegetationInteractionSample interaction,
    float rootToTipWeight,
    float vertexHeightMetres,
    float bendResponse,
    float flattenResponse,
    float heightExponent,
    float maximumBendMetres,
    out float2 fullTipDisplacementXZ,
    out float fullTipFlatten)
{
    fullTipDisplacementXZ = 0.0;
    fullTipFlatten = 0.0;
    if (interaction.active < 0.5)
    {
        return worldPosition;
    }

    float responseWeight = pow(
        saturate(rootToTipWeight),
        max(0.25, heightExponent));
    float2 normalizedLayerBend = interaction.bend *
        max(0.0, bendResponse);
    float normalizedLayerBendMagnitude = length(normalizedLayerBend);
    if (normalizedLayerBendMagnitude > 1.0)
    {
        normalizedLayerBend /= normalizedLayerBendMagnitude;
    }
    fullTipDisplacementXZ = normalizedLayerBend *
        max(0.0, maximumBendMetres);
    float2 vertexDisplacement =
        fullTipDisplacementXZ * responseWeight;
    worldPosition.xz += vertexDisplacement;

    fullTipFlatten = saturate(
        interaction.flatten * max(0.0, flattenResponse));
    float flattenDistance =
        fullTipFlatten *
        max(0.0, vertexHeightMetres) *
        responseWeight;
    worldPosition.y -= flattenDistance;
    return worldPosition;
}

#endif
