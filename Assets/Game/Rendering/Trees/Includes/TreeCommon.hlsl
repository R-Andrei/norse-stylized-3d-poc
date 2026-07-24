#ifndef PS3D_TREE_COMMON_INCLUDED
#define PS3D_TREE_COMMON_INCLUDED

#define PS3D_TREE_TWO_PI 6.28318530718

float TreeHash11(float value)
{
    return frac(sin(value * 12.9898 + 78.233) * 43758.5453);
}

float TreeHash31(float3 value)
{
    return frac(
        sin(dot(value, float3(12.9898, 78.233, 37.719))) *
        43758.5453);
}

float3 TreeRotateAroundAxis(
    float3 value,
    float3 axis,
    float angle)
{
    float sine = sin(angle);
    float cosine = cos(angle);
    return value * cosine +
        cross(axis, value) * sine +
        axis * dot(axis, value) * (1.0 - cosine);
}

float TreeResolveHeightMask(
    float localY,
    float boundsMinY,
    float boundsHeight)
{
    float normalizedHeight = saturate(
        (localY - boundsMinY) /
        max(0.0001, boundsHeight));
    return normalizedHeight * normalizedHeight *
        (3.0 - 2.0 * normalizedHeight);
}

float TreeResolveWindMask(
    float heightMask,
    float4 vertexColour,
    float maskMode)
{
    float vertexColourMask = saturate(vertexColour.r);
    return maskMode >= 0.5
        ? vertexColourMask
        : heightMask;
}

float3 TreeResolveDebugColour(
    float debugMode,
    float4 vertexColour,
    float heightMask,
    float windMask,
    float flutterPhase,
    float3 normalWS)
{
    if (debugMode < 0.5)
    {
        return -1.0;
    }

    if (debugMode < 1.5)
    {
        return saturate(vertexColour.rgb);
    }

    if (debugMode < 2.5)
    {
        return heightMask.xxx;
    }

    if (debugMode < 3.5)
    {
        return windMask.xxx;
    }

    if (debugMode < 4.5)
    {
        float phase = frac(flutterPhase / PS3D_TREE_TWO_PI);
        return float3(
            phase,
            frac(phase + 0.3333333),
            frac(phase + 0.6666667));
    }

    return normalize(normalWS) * 0.5 + 0.5;
}

#endif
