#ifndef PS3D_VEGETATION_COMMON_INCLUDED
#define PS3D_VEGETATION_COMMON_INCLUDED

struct VegetationInstanceDataGpu
{
    float4 positionYaw;
    float4 scaleStiffness;
    float4 variationPhase;
};

StructuredBuffer<VegetationInstanceDataGpu> _VegetationInstances;
float4x4 _VegetationLocalToWorld;

float2 RotateVegetationXZ(float2 value, float yaw)
{
    float sineValue;
    float cosineValue;
    sincos(yaw, sineValue, cosineValue);
    return float2(
        value.x * cosineValue - value.y * sineValue,
        value.x * sineValue + value.y * cosineValue);
}

void DecodeVegetationInstance(
    uint instanceId,
    out float3 localPosition,
    out float yaw,
    out float2 scale,
    out float stiffness,
    out float phase,
    out float colorVariation,
    out float bladeVariation,
    out float macroPatch)
{
    VegetationInstanceDataGpu instanceData = _VegetationInstances[instanceId];
    localPosition = instanceData.positionYaw.xyz;
    yaw = instanceData.positionYaw.w;
    scale = instanceData.scaleStiffness.xy;
    stiffness = instanceData.scaleStiffness.z;
    phase = instanceData.variationPhase.x;
    colorVariation = instanceData.variationPhase.y;
    bladeVariation = instanceData.variationPhase.z;
    macroPatch = instanceData.variationPhase.w;
}

float3 TransformVegetationVertexToWorld(
    float3 meshPosition,
    float3 instanceLocalPosition,
    float yaw,
    float2 scale)
{
    float2 rotatedXZ = RotateVegetationXZ(meshPosition.xz * scale.x, yaw);
    float3 localPosition = instanceLocalPosition + float3(
        rotatedXZ.x,
        meshPosition.y * scale.y,
        rotatedXZ.y);
    return mul(_VegetationLocalToWorld, float4(localPosition, 1.0)).xyz;
}

float3 TransformVegetationVertexToWorldStabilized(
    float3 meshPosition,
    float2 meshCenterXZ,
    float3 instanceLocalPosition,
    float yaw,
    float2 scale,
    float stabilizationEnabled,
    float stabilizationStartDistance,
    float stabilizationMaximumMultiplier)
{
    float3 centerMeshPosition = float3(meshCenterXZ.x, meshPosition.y, meshCenterXZ.y);
    float3 centerWorldPosition = TransformVegetationVertexToWorld(
        centerMeshPosition,
        instanceLocalPosition,
        yaw,
        scale);

    float cameraDistance = distance(_WorldSpaceCameraPos, centerWorldPosition);
    float distanceRange = max(0.001, stabilizationStartDistance);
    float perspectiveStabilizationT = saturate(
        (cameraDistance - stabilizationStartDistance) / distanceRange);
    perspectiveStabilizationT *= perspectiveStabilizationT *
        (3.0 - 2.0 * perspectiveStabilizationT);
    float stabilizationT = lerp(
        perspectiveStabilizationT,
        1.0,
        saturate(unity_OrthoParams.w));
    float multiplier = lerp(
        1.0,
        max(1.0, stabilizationMaximumMultiplier),
        stabilizationT * saturate(stabilizationEnabled));

    float2 lateralOffset = meshPosition.xz - meshCenterXZ;
    float3 stabilizedMeshPosition = meshPosition;
    stabilizedMeshPosition.xz = meshCenterXZ + lateralOffset * multiplier;
    return TransformVegetationVertexToWorld(
        stabilizedMeshPosition,
        instanceLocalPosition,
        yaw,
        scale);
}

float3 TransformVegetationNormalToWorld(float3 meshNormal, float yaw)
{
    float2 rotatedXZ = RotateVegetationXZ(meshNormal.xz, yaw);
    float3 localNormal = normalize(float3(rotatedXZ.x, meshNormal.y, rotatedXZ.y));
    return normalize(mul((float3x3)_VegetationLocalToWorld, localNormal));
}

#endif
