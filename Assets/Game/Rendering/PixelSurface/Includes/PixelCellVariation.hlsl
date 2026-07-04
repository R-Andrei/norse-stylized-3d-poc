#ifndef PS3D_PIXEL_CELL_VARIATION_INCLUDED
#define PS3D_PIXEL_CELL_VARIATION_INCLUDED

float PS3D_Hash31(float3 value)
{
    value = frac(value * 0.1031);
    value += dot(value, value.yzx + 33.33);
    return frac((value.x + value.y) * value.z);
}

float PS3D_ValueNoise31(float3 value)
{
    float3 cell = floor(value);
    float3 local = frac(value);
    float3 blend = local * local * (3.0 - 2.0 * local);

    float c000 = PS3D_Hash31(cell + float3(0.0, 0.0, 0.0));
    float c100 = PS3D_Hash31(cell + float3(1.0, 0.0, 0.0));
    float c010 = PS3D_Hash31(cell + float3(0.0, 1.0, 0.0));
    float c110 = PS3D_Hash31(cell + float3(1.0, 1.0, 0.0));
    float c001 = PS3D_Hash31(cell + float3(0.0, 0.0, 1.0));
    float c101 = PS3D_Hash31(cell + float3(1.0, 0.0, 1.0));
    float c011 = PS3D_Hash31(cell + float3(0.0, 1.0, 1.0));
    float c111 = PS3D_Hash31(cell + float3(1.0, 1.0, 1.0));

    float x00 = lerp(c000, c100, blend.x);
    float x10 = lerp(c010, c110, blend.x);
    float x01 = lerp(c001, c101, blend.x);
    float x11 = lerp(c011, c111, blend.x);
    float y0 = lerp(x00, x10, blend.y);
    float y1 = lerp(x01, x11, blend.y);
    return lerp(y0, y1, blend.z);
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
