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

bool FoamTopologyTransitionPreviousUsesFixedMetricLattice()
{
    return (int)round(
        _FoamTopologyTransitionGridDescriptorContract.z) == 1;
}

float FoamTopologyTransitionPreviousLocalDistanceAtUV(float u)
{
    if (FoamTopologyTransitionPreviousUsesFixedMetricLattice())
    {
        return _FoamTopologyTransitionGridDescriptorLongitudinal.x +
            saturate(u) *
            _FoamTopologyTransitionGridDescriptorLongitudinal.y;
    }

    return saturate(u) * _FoamTopologyTransitionFieldLength;
}

bool TryResolveTopologyTransitionPreviousUV(
    float globalDistance,
    float lateralMetres,
    out float2 previousUV)
{
    previousUV = 0.0.xx;
    float previousLocalDistance =
        globalDistance - _FoamTopologyTransitionGlobalStart;
    if (previousLocalDistance < 0.0 ||
        previousLocalDistance >
            _FoamTopologyTransitionValidLength + 0.0001)
    {
        return false;
    }

    float previousU;
    if (FoamTopologyTransitionPreviousUsesFixedMetricLattice())
    {
        float previousStart =
            _FoamTopologyTransitionGridDescriptorLongitudinal.x;
        float previousLength = max(
            0.0001,
            _FoamTopologyTransitionGridDescriptorLongitudinal.y);
        previousU =
            (previousLocalDistance - previousStart) / previousLength;
    }
    else
    {
        previousU = previousLocalDistance /
            max(0.0001, _FoamTopologyTransitionFieldLength);
    }
    if (previousU < 0.0 || previousU > 1.0)
    {
        return false;
    }

    int previousMetricX = FoamUVToContainingTexel(
        previousU,
        _FoamTopologyTransitionDimensions.x);
    FoamMetricRow previousMetric =
        _FoamTopologyTransitionMetricRows[previousMetricX];
    float previousV;
    if (FoamTopologyTransitionPreviousUsesFixedMetricLattice())
    {
        float previousDy = max(
            0.0001,
            _FoamTopologyTransitionGridDescriptorSpacing.w);
        float previousGlobalY =
            (lateralMetres -
             _FoamTopologyTransitionGridDescriptorLateral.x) /
            previousDy;
        float previousLocalY = previousGlobalY -
            _FoamTopologyTransitionGridDescriptorLateral.y;
        previousV = (previousLocalY + 0.5) /
            max(1.0, _FoamTopologyTransitionGridDescriptorLateral.z);
        if (previousV < 0.0 || previousV > 1.0)
        {
            return false;
        }
    }
    else
    {
        float previousLeft = max(
            0.0001,
            previousMetric.widthsAndSpacing.x);
        float previousRight = max(
            0.0001,
            previousMetric.widthsAndSpacing.y);
        if (lateralMetres < -previousLeft ||
            lateralMetres > previousRight)
        {
            return false;
        }
        previousV = FoamMetresToAcross01(
            lateralMetres,
            previousLeft,
            previousRight);
    }

    previousUV = saturate(float2(previousU, previousV));
    return true;
}

bool TryResolvePersistentStateRemapCoordinate(
    int2 currentCoordinate,
    out int2 previousCoordinate)
{
    previousCoordinate = int2(0, 0);
    if (_FoamPersistentStateRemapEnabled == 0 ||
        !FoamGridUsesFixedMetricLattice() ||
        !FoamTopologyTransitionPreviousUsesFixedMetricLattice())
    {
        return false;
    }

    int currentX = clamp(
        currentCoordinate.x,
        0,
        max(0, _FoamDimensions.x - 1));
    FoamMetricRow currentMetric = _FoamMetricRows[currentX];
    float currentLocalDistance =
        FoamGridLocalDistanceAtTexel(currentCoordinate.x);
    float globalDistance = _FoamGlobalStart + currentLocalDistance;
    float previousLocalDistance =
        globalDistance - _FoamTopologyTransitionGlobalStart;
    float previousDx = max(
        0.0001,
        _FoamTopologyTransitionGridDescriptorSpacing.z);
    float previousXFloat =
        (previousLocalDistance -
         _FoamTopologyTransitionGridDescriptorLongitudinal.x) /
        previousDx - 0.5;
    int previousX = (int)round(previousXFloat);
    if (abs(previousXFloat - (float)previousX) > 0.001 ||
        previousX < 0 ||
        previousX >= _FoamTopologyTransitionDimensions.x)
    {
        return false;
    }

    float lateralMetres = FoamLateralMetresAtTexel(
        currentCoordinate.y,
        currentMetric);
    float previousDy = max(
        0.0001,
        _FoamTopologyTransitionGridDescriptorSpacing.w);
    float previousGlobalY =
        (lateralMetres -
         _FoamTopologyTransitionGridDescriptorLateral.x) /
        previousDy;
    float previousYFloat = previousGlobalY -
        _FoamTopologyTransitionGridDescriptorLateral.y;
    int previousY = (int)round(previousYFloat);
    if (abs(previousYFloat - (float)previousY) > 0.001 ||
        previousY < 0 ||
        previousY >= _FoamTopologyTransitionDimensions.y)
    {
        return false;
    }

    previousCoordinate = int2(previousX, previousY);
    return true;
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
        float currentLocalDistance = FoamGridLocalDistanceAtUV(uv.x);
        float globalDistance = _FoamGlobalStart + currentLocalDistance;
        float lateralMetres = FoamLateralMetresAtUV(
            uv.y,
            currentMetric);
        float2 previousUV;
        if (TryResolveTopologyTransitionPreviousUV(
                globalDistance,
                lateralMetres,
                previousUV))
        {
            previousGenerated = SampleTopologyTransitionBilinear(previousUV);
            previousAvailable = true;
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
