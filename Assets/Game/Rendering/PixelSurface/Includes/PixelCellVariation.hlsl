#ifndef PS3D_PIXEL_CELL_VARIATION_INCLUDED
#define PS3D_PIXEL_CELL_VARIATION_INCLUDED

float PS3D_Hash31(float3 value)
{
    value = frac(value * 0.1031);
    value += dot(value, value.yzx + 33.33);
    return frac((value.x + value.y) * value.z);
}

void PixelCellVariation_float(
    float3 Position,
    float CellSize,
    float Seed,
    float ToneCount,
    float ClusterStrength,
    out float Variation)
{
    float safeCellSize = max(CellSize, 0.0001);
    float3 cell = floor(Position / safeCellSize);

    float detailValue = PS3D_Hash31(cell + Seed * 19.19);
    float clusterValue = PS3D_Hash31(floor(cell * 0.5) + Seed * 47.47);

    float combinedValue = lerp(
        detailValue,
        clusterValue,
        saturate(ClusterStrength));

    float levels = max(2.0, round(ToneCount));
    float quantized = floor(combinedValue * levels) / (levels - 1.0);

    Variation = saturate(quantized) * 2.0 - 1.0;
}

void PixelCellVariation_half(
    half3 Position,
    half CellSize,
    half Seed,
    half ToneCount,
    half ClusterStrength,
    out half Variation)
{
    float variationFloat;

    PixelCellVariation_float(
        (float3)Position,
        (float)CellSize,
        (float)Seed,
        (float)ToneCount,
        (float)ClusterStrength,
        variationFloat);

    Variation = (half)variationFloat;
}

#endif
