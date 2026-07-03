// Build the geometry-following Pressure support envelope implicitly from`r`n// upstream-facing cells in the authoritative exact-mesh Obstacle Footprint`r`n// mask. Each solid boundary cell supports only a short region immediately`r`n// upstream of its own lateral row. Adjacent rows provide at most one cell of`r`n// penalised side support for sloped/rotated silhouettes. Unsupported Pressure`r`n// fails closed: the Stage 5 candidate survives only where an actual current-`r`n// water solid boundary explicitly supports it.`r`n

float ResolvePressureSupportEnvelope(
    int2 coordinate,
    float longitudinalSpacing,
    float lateralSpacing)
{
    const int MaximumSearchCells = 10;
    const int MaximumThicknessCells = 16;
    const float MaximumSearchMetres = 1.35;
    const int LateralProbeCount = 3;

    if (LoadObstacleExclusionCell(coordinate) >= 0.5)
    {
        return 1.0;
    }

    int downstreamStep = _FoamFlowDirection >= 0.0 ? 1 : -1;
    float support = 0.0;

    [loop]
    for (int probe = 0; probe < LateralProbeCount; probe++)
    {
        int lateralOffset = probe == 0 ? 0 :
            (probe == 1 ? -1 : 1);
        int firstSolidStep = -1;
        int2 boundaryCoordinate = coordinate;

        [loop]
        for (int stepIndex = 0;
             stepIndex < MaximumSearchCells;
             stepIndex++)
        {
            if ((float)stepIndex * longitudinalSpacing >
                MaximumSearchMetres)
            {
                break;
            }

            int2 sampleCoordinate = coordinate + int2(
                downstreamStep * stepIndex,
                lateralOffset);
            if (LoadObstacleExclusionCell(sampleCoordinate) < 0.5)
            {
                continue;
            }

            firstSolidStep = stepIndex;
            boundaryCoordinate = sampleCoordinate;
            break;
        }

        if (firstSolidStep < 0)
        {
            continue;
        }

        // The first solid reached from open upstream water must be the local
        // upstream-facing boundary. Reject any ambiguous interior hit rather
        // than letting a deeper solid row support a forward shelf.
        int2 upstreamNeighbour = boundaryCoordinate - int2(
            downstreamStep,
            0);
        if (LoadObstacleExclusionCell(upstreamNeighbour) >= 0.5)
        {
            continue;
        }

        int thicknessCells = 0;
        [loop]
        for (int thicknessIndex = 0;
             thicknessIndex < MaximumThicknessCells;
             thicknessIndex++)
        {
            int2 sampleCoordinate = boundaryCoordinate + int2(
                downstreamStep * thicknessIndex,
                0);
            if (LoadObstacleExclusionCell(sampleCoordinate) < 0.5)
            {
                break;
            }

            thicknessCells++;
        }

        float localThickness = max(
            longitudinalSpacing,
            (float)thicknessCells * longitudinalSpacing);
        float minimumReach = max(
            0.18,
            longitudinalSpacing * 0.65);
        float allowedReach = clamp(
            0.16 + localThickness * 0.16,
            minimumReach,
            0.78);

        float lateralDistance =
            abs((float)lateralOffset) * lateralSpacing;
        allowedReach = max(
            0.0,
            allowedReach - lateralDistance * 1.35);

        float frontDistance = max(
            0.0,
            ((float)firstSolidStep - 0.5) * longitudinalSpacing);
        float feather = max(
            0.05,
            longitudinalSpacing * 0.40);
        float localSupport = 1.0 - smoothstep(
            allowedReach,
            allowedReach + feather,
            frontDistance);

        // Adjacent-row support exists only to keep angled silhouettes joined;
        // it must not become an equal-strength source of flat lateral shelves.
        if (lateralOffset != 0)
        {
            localSupport *= 0.82;
        }

        support = max(support, localSupport);
    }

    return saturate(support);
}
