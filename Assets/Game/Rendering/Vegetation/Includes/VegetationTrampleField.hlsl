#ifndef PS3D_VEGETATION_TRAMPLE_FIELD_INCLUDED
#define PS3D_VEGETATION_TRAMPLE_FIELD_INCLUDED

TEXTURE2D(_VegetationTramplePreviousField);
SAMPLER(sampler_VegetationTramplePreviousField);
TEXTURE2D(_VegetationTrampleCurrentField);
SAMPLER(sampler_VegetationTrampleCurrentField);

float4x4 _VegetationTrampleWorldToLocal;
float4 _VegetationTrampleDomainParameters;

struct VegetationTrampleSample
{
    float2 bend;
    float flatten;
    float active;
};

VegetationTrampleSample SampleVegetationTrample(float3 worldPosition)
{
    VegetationTrampleSample sample;
    sample.bend = 0.0;
    sample.flatten = 0.0;
    sample.active = 0.0;
    if (_VegetationTrampleDomainParameters.w < 0.5)
    {
        return sample;
    }

    float3 localPosition = mul(
        _VegetationTrampleWorldToLocal,
        float4(worldPosition, 1.0)).xyz;
    float halfSize = _VegetationTrampleDomainParameters.x;
    float domainSize = max(0.0001, _VegetationTrampleDomainParameters.y);
    float2 uv = (localPosition.xz + halfSize) / domainSize;
    float2 minimumCheck = step(0.0, uv);
    float2 maximumCheck = step(uv, 1.0);
    float inside = minimumCheck.x * minimumCheck.y *
        maximumCheck.x * maximumCheck.y;
    float2 clampedUv = saturate(uv);
    float4 previousState = SAMPLE_TEXTURE2D_LOD(
        _VegetationTramplePreviousField,
        sampler_VegetationTramplePreviousField,
        clampedUv,
        0.0);
    float4 currentState = SAMPLE_TEXTURE2D_LOD(
        _VegetationTrampleCurrentField,
        sampler_VegetationTrampleCurrentField,
        clampedUv,
        0.0);
    float4 state = lerp(
        previousState,
        currentState,
        saturate(_VegetationTrampleDomainParameters.z));
    state *= inside;
    sample.bend = state.xy;
    sample.flatten = saturate(state.z);
    sample.active = inside;
    return sample;
}

float VegetationTrampleEffectiveStrength(
    VegetationTrampleSample trample,
    float bendResponse,
    float flattenResponse)
{
    float bendStrength = saturate(
        length(trample.bend) * max(0.0, bendResponse));
    float flattenStrength = saturate(
        trample.flatten * max(0.0, flattenResponse));
    return max(bendStrength, flattenStrength);
}

float3 ApplyVegetationTrampleResponse(
    float3 worldPosition,
    VegetationTrampleSample trample,
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
    if (trample.active < 0.5)
    {
        return worldPosition;
    }

    float responseWeight = pow(
        saturate(rootToTipWeight),
        max(0.25, heightExponent));
    float2 normalizedLayerBend = trample.bend * max(0.0, bendResponse);
    float bendMagnitude = length(normalizedLayerBend);
    if (bendMagnitude > 1.0)
    {
        normalizedLayerBend /= bendMagnitude;
    }
    fullTipDisplacementXZ = normalizedLayerBend * max(0.0, maximumBendMetres);
    worldPosition.xz += fullTipDisplacementXZ * responseWeight;

    fullTipFlatten = saturate(
        trample.flatten * max(0.0, flattenResponse));
    worldPosition.y -= fullTipFlatten *
        max(0.0, vertexHeightMetres) * responseWeight;
    return worldPosition;
}

#endif
