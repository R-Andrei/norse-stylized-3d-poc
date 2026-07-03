static const float FoamMaterialStateEpsilon = 0.0001;

struct FoamMaterialState
{
    float presence;
    float remainingLife;
    float materialPattern;
};

FoamMaterialState FoamDecodeMaterialState(float4 packed)
{
    FoamMaterialState state;
    state.presence = saturate(packed.x);
    if (state.presence > FoamMaterialStateEpsilon)
    {
        state.remainingLife = saturate(packed.y / state.presence);
        state.materialPattern = saturate(packed.z / state.presence);
    }
    else
    {
        state.remainingLife = 0.0;
        state.materialPattern = 0.0;
    }

    return state;
}

float4 FoamEncodeMaterialState(FoamMaterialState state)
{
    state.presence = saturate(state.presence);
    state.remainingLife = saturate(state.remainingLife);
    state.materialPattern = saturate(state.materialPattern);
    if (state.presence <= FoamMaterialStateEpsilon ||
        state.remainingLife <= 0.0)
    {
        return 0.0.xxxx;
    }

    return float4(
        state.presence,
        state.presence * state.remainingLife,
        state.presence * state.materialPattern,
        0.0);
}

float4 FoamClampPackedMaterialState(float4 packed)
{
    float presence = saturate(packed.x);
    float lifeMoment = clamp(packed.y, 0.0, presence);
    float patternMoment = clamp(packed.z, 0.0, presence);
    if (presence <= FoamMaterialStateEpsilon || lifeMoment <= 0.0)
    {
        return 0.0.xxxx;
    }

    return float4(presence, lifeMoment, patternMoment, 0.0);
}

float4 FoamMergeBornPresence(float4 existingPacked, float4 sourcePacked)
{
    FoamMaterialState existing = FoamDecodeMaterialState(existingPacked);
    FoamMaterialState source = FoamDecodeMaterialState(sourcePacked);
    float addedPresence = max(
        0.0,
        source.presence - existing.presence);
    float combinedPresence = max(
        existing.presence,
        source.presence);
    if (combinedPresence <= FoamMaterialStateEpsilon)
    {
        return 0.0.xxxx;
    }

    FoamMaterialState combined;
    combined.presence = combinedPresence;
    combined.remainingLife = saturate(
        (existing.presence * existing.remainingLife +
         addedPresence * source.remainingLife) /
        combinedPresence);
    combined.materialPattern = saturate(
        (existing.presence * existing.materialPattern +
         addedPresence * source.materialPattern) /
        combinedPresence);
    return FoamEncodeMaterialState(combined);
}
