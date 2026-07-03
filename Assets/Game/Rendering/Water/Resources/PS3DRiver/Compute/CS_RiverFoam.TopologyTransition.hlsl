
float3 ResolveCurrentGeneratedTopology(int2 outputCoordinate)
{
    float4 generatedWork = _FoamTopologyGeneratedRead.Load(
        int3(outputCoordinate, 0));

    float evolvingMajorSupport = _FoamEvolvingMajorRead.Load(
        int3(outputCoordinate, 0));
    float majorSupport = lerp(
        generatedWork.r,
        evolvingMajorSupport,
        saturate(_FoamMajorEvolutionEnabled));

    float evolvingConnectorSupport = _FoamEvolvingConnectorRead.Load(
        int3(outputCoordinate, 0));
    float connectorSupport = lerp(
        generatedWork.g,
        evolvingConnectorSupport,
        saturate(_FoamConnectorIdentityReconstructionEnabled));

    float evolvingFreeWaterAgingPressure =
        _FoamEvolvingFreeWaterNegativeRead.Load(
            int3(outputCoordinate, 0));
    float evolvingWeakSpanAgingPressure =
        _FoamEvolvingWeakSpanNegativeRead.Load(
            int3(outputCoordinate, 0));
    float independentAgingPressure = generatedWork.b;
    independentAgingPressure = lerp(
        independentAgingPressure,
        max(independentAgingPressure, evolvingFreeWaterAgingPressure),
        saturate(_FoamFreeWaterNegativeEvolutionEnabled));
    independentAgingPressure = lerp(
        independentAgingPressure,
        max(independentAgingPressure, evolvingWeakSpanAgingPressure),
        saturate(_FoamWeakSpanIdentityReconstructionEnabled));

    float staticHostedAgingPressure = generatedWork.a;
    float evolvingHostedAgingPressure =
        _FoamEvolvingHostedNegativeRead.Load(
            int3(outputCoordinate, 0));
    float hostedAgingPressure = lerp(
        staticHostedAgingPressure,
        max(staticHostedAgingPressure, evolvingHostedAgingPressure),
        saturate(_FoamHostedNegativeEvolutionEnabled));

    return saturate(float3(
        majorSupport,
        connectorSupport,
        max(independentAgingPressure, hostedAgingPressure)));
}


float3 SampleTopologyTransitionBilinear(float2 uv)
{
    float2 pixelCoordinate = FoamUVToTexelCoordinate(
        uv,
        _FoamTopologyTransitionDimensions);
    int2 baseCoordinate = int2(floor(pixelCoordinate));
    int2 nextCoordinate = min(
        baseCoordinate + int2(1, 1),
        _FoamTopologyTransitionDimensions - int2(1, 1));
    float2 blend = frac(pixelCoordinate);

    float3 a = _FoamTopologyTransitionFromRead.Load(
        int3(baseCoordinate, 0)).rgb;
    float3 b = _FoamTopologyTransitionFromRead.Load(
        int3(int2(nextCoordinate.x, baseCoordinate.y), 0)).rgb;
    float3 c = _FoamTopologyTransitionFromRead.Load(
        int3(int2(baseCoordinate.x, nextCoordinate.y), 0)).rgb;
    float3 d = _FoamTopologyTransitionFromRead.Load(
        int3(nextCoordinate, 0)).rgb;
    return lerp(
        lerp(a, b, blend.x),
        lerp(c, d, blend.x),
        blend.y);
}


float3 ApplyGeneratedTopologyTransition(
    float3 currentGenerated,
    int2 outputCoordinate,
    float2 uv,
    FoamMetricRow currentMetric)
{
    if (_FoamTopologyTransitionEnabled <= 0.5)
    {
        return currentGenerated;
    }

    float3 previousGenerated = 0.0.xxx;
    bool previousAvailable = false;
    if (_FoamTopologyTransitionSameMapping > 0.5 &&
        all(outputCoordinate < _FoamTopologyTransitionDimensions))
    {
        previousGenerated = _FoamTopologyTransitionFromRead.Load(
            int3(outputCoordinate, 0)).rgb;
        previousAvailable = true;
    }
    else
    {
        float currentLocalDistance = saturate(uv.x) * _FoamFieldLength;
        float globalDistance = _FoamGlobalStart + currentLocalDistance;
        float previousLocalDistance =
            globalDistance - _FoamTopologyTransitionGlobalStart;
        if (previousLocalDistance >= 0.0 &&
            previousLocalDistance <=
                _FoamTopologyTransitionValidLength + 0.0001 &&
            _FoamTopologyTransitionFieldLength > 0.0001)
        {
            float previousU = previousLocalDistance /
                _FoamTopologyTransitionFieldLength;
            int previousMetricX = FoamUVToContainingTexel(
                previousU,
                _FoamTopologyTransitionDimensions.x);
            FoamMetricRow previousMetric =
                _FoamTopologyTransitionMetricRows[previousMetricX];
            float lateralMetres = FoamAcross01ToMetres(
                saturate(uv.y),
                currentMetric.widthsAndSpacing.x,
                currentMetric.widthsAndSpacing.y);
            float previousLeft = max(
                0.0001,
                previousMetric.widthsAndSpacing.x);
            float previousRight = max(
                0.0001,
                previousMetric.widthsAndSpacing.y);
            if (lateralMetres >= -previousLeft &&
                lateralMetres <= previousRight)
            {
                float previousV = FoamMetresToAcross01(
                    lateralMetres,
                    previousLeft,
                    previousRight);
                previousGenerated = SampleTopologyTransitionBilinear(
                    float2(previousU, previousV));
                previousAvailable = true;
            }
        }
    }

    float3 previousOrZero = previousAvailable
        ? previousGenerated
        : 0.0.xxx;
    return saturate(lerp(
        previousOrZero,
        currentGenerated,
        saturate(_FoamTopologyTransitionBlend)));
}
