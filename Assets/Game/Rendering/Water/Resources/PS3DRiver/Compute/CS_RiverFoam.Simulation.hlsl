void ResolveFoamMaterialNeighbourhood(
    int x,
    int y,
    float globalDistance,
    float lateralMetres,
    float phase,
    float agitation,
    out float directionalAverage,
    out float broadAmount,
    out float neighbourPhase)
{
    float baseAngle = phase * 6.2831853 +
        (FoamFbm(float2(globalDistance * 0.19, lateralMetres * 0.41) + _FoamSeed * 0.0017) - 0.5) * 2.6;
    float radius = lerp(1.15, 1.70, agitation);
    float2 d0 = float2(cos(baseAngle), sin(baseAngle));
    float2 d1 = float2(cos(baseAngle + 0.7853982), sin(baseAngle + 0.7853982));
    float2 d2 = float2(cos(baseAngle + 1.5707963), sin(baseAngle + 1.5707963));
    float2 d3 = float2(cos(baseAngle + 2.3561945), sin(baseAngle + 2.3561945));

    float4 s0 = SampleAdvectedBilinear(float2(x, y) + d0 * radius);
    float4 s1 = SampleAdvectedBilinear(float2(x, y) + d1 * radius);
    float4 s2 = SampleAdvectedBilinear(float2(x, y) + d2 * radius);
    float4 s3 = SampleAdvectedBilinear(float2(x, y) + d3 * radius);
    float4 s4 = SampleAdvectedBilinear(float2(x, y) - d0 * radius);
    float4 s5 = SampleAdvectedBilinear(float2(x, y) - d1 * radius);
    float4 s6 = SampleAdvectedBilinear(float2(x, y) - d2 * radius);
    float4 s7 = SampleAdvectedBilinear(float2(x, y) - d3 * radius);

    directionalAverage =
        (s0.x + s1.x + s2.x + s3.x + s4.x + s5.x + s6.x + s7.x) * 0.125;

    float directionalWeight =
        s0.x + s1.x + s2.x + s3.x + s4.x + s5.x + s6.x + s7.x;
    neighbourPhase = phase;
    if (directionalWeight > 0.001)
    {
        float weightedSin =
            sin(s0.w * 6.2831853) * s0.x + sin(s1.w * 6.2831853) * s1.x +
            sin(s2.w * 6.2831853) * s2.x + sin(s3.w * 6.2831853) * s3.x +
            sin(s4.w * 6.2831853) * s4.x + sin(s5.w * 6.2831853) * s5.x +
            sin(s6.w * 6.2831853) * s6.x + sin(s7.w * 6.2831853) * s7.x;
        float weightedCos =
            cos(s0.w * 6.2831853) * s0.x + cos(s1.w * 6.2831853) * s1.x +
            cos(s2.w * 6.2831853) * s2.x + cos(s3.w * 6.2831853) * s3.x +
            cos(s4.w * 6.2831853) * s4.x + cos(s5.w * 6.2831853) * s5.x +
            cos(s6.w * 6.2831853) * s6.x + cos(s7.w * 6.2831853) * s7.x;
        neighbourPhase = frac(atan2(weightedSin, weightedCos) / 6.2831853 + 1.0);
    }

    float broadRadius = lerp(2.5, 3.5, agitation);
    broadAmount =
        (SampleAdvectedBilinear(float2(x, y) + d0 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) - d0 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) + d2 * broadRadius).x +
         SampleAdvectedBilinear(float2(x, y) - d2 * broadRadius).x) * 0.25;
}

