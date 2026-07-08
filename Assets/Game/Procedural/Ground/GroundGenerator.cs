using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Rivers;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public static class GroundGenerator
    {
        public static MeshData Generate(
            GroundRecipe recipe,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers)
        {
            return Generate(
                recipe,
                null,
                modifiers,
                rivers,
                out _);
        }

        public static MeshData Generate(
            GroundRecipe recipe,
            GroundSurfaceProfile surfaceProfile,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers)
        {
            return Generate(
                recipe,
                surfaceProfile,
                modifiers,
                rivers,
                out _);
        }

        public static MeshData Generate(
            GroundRecipe recipe,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            out GroundHeightFieldSnapshot baseSurface)
        {
            return Generate(
                recipe,
                null,
                modifiers,
                rivers,
                out baseSurface);
        }

        public static MeshData Generate(
            GroundRecipe recipe,
            GroundSurfaceProfile surfaceProfile,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            out GroundHeightFieldSnapshot baseSurface)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }

            float patchSize = ResolvePatchSize(recipe.PatchSize);
            int resolution = ResolveResolution(recipe.Resolution);
            int vertexCount = resolution * resolution;

            float[] heights = new float[vertexCount];
            float[] detailHeights = new float[vertexCount];

            float spacing = patchSize / (resolution - 1);
            float halfSize = patchSize * 0.5f;

            Vector2 noiseOrigin = new Vector2(
                recipe.PatchCoordinate.x * patchSize,
                recipe.PatchCoordinate.y * patchSize);

            for (int z = 0; z < resolution; z++)
            {
                float v = z / (float)(resolution - 1);
                float localZ = -halfSize + z * spacing;

                for (int x = 0; x < resolution; x++)
                {
                    float u = x / (float)(resolution - 1);
                    float localX = -halfSize + x * spacing;
                    int index = z * resolution + x;

                    float transition =
                        EvaluateTransition(
                            u,
                            v,
                            recipe.TransitionDirection,
                            recipe.TransitionHeight);

                    float edgeMask =
                        EvaluateEdgeMask(
                            u,
                            v,
                            recipe.EdgeBlend);

                    Vector2 samplePosition =
                        noiseOrigin +
                        new Vector2(localX, localZ);

                    float broadHeight =
                        EvaluateBroadProfile(
                            recipe,
                            samplePosition,
                            u,
                            v);

                    float detailHeight =
                        EvaluateSurfaceDetail(
                            recipe,
                            samplePosition);

                    broadHeight *= edgeMask;
                    detailHeight *= edgeMask;

                    detailHeights[index] = detailHeight;
                    heights[index] =
                        transition +
                        broadHeight +
                        detailHeight;
                }
            }

            ApplyModifiers(
                heights,
                detailHeights,
                resolution,
                spacing,
                halfSize,
                modifiers);

            float[] baseHeights = (float[])heights.Clone();

            ApplyRivers(
                heights,
                resolution,
                spacing,
                halfSize,
                rivers);

            Vector3[] baseNormals =
                BuildHeightFieldNormals(
                    baseHeights,
                    resolution,
                    spacing,
                    recipe.ShapeSeed);

            // River concealment changes broad-ground positions only where the
            // dedicated corridor renders above them. Those hidden slopes must
            // not affect the visible terrain lighting at the handoff.
            Vector3[] renderNormals =
                (Vector3[])baseNormals.Clone();

            // Surface metadata describes the visible pre-river terrain rather
            // than the hidden concealment trench.
            BuildSurfaceMetadata(
                recipe,
                surfaceProfile,
                baseHeights,
                baseNormals,
                modifiers,
                rivers,
                resolution,
                spacing,
                halfSize,
                out float[] surfaceVariations,
                out float[] exposureMasks,
                out float[] dampDepositMasks,
                out float[] vegetationSuitabilityMasks,
                out float[] materialClassifications,
                out Vector4[] secondarySurfaceMasks);

            baseSurface =
                new GroundHeightFieldSnapshot(
                    baseHeights,
                    baseNormals,
                    renderNormals,
                    surfaceVariations,
                    exposureMasks,
                    dampDepositMasks,
                    vegetationSuitabilityMasks,
                    secondarySurfaceMasks,
                    materialClassifications,
                    resolution,
                    spacing,
                    halfSize,
                    recipe.ShapeSeed);

            return BuildMeshData(
                heights,
                renderNormals,
                surfaceVariations,
                exposureMasks,
                dampDepositMasks,
                vegetationSuitabilityMasks,
                secondarySurfaceMasks,
                resolution,
                spacing,
                halfSize,
                recipe.ShapeSeed);
        }

        public static float ResolvePatchSize(
            GroundPatchSize patchSize)
        {
            return patchSize switch
            {
                GroundPatchSize.Compact20 => 20f,
                GroundPatchSize.Standard40 => 40f,
                GroundPatchSize.Large60 => 60f,
                GroundPatchSize.Huge80 => 80f,
                _ => 40f
            };
        }

        public static int ResolveResolution(
            GroundResolution resolution)
        {
            return resolution switch
            {
                GroundResolution.Low17 => 17,
                GroundResolution.Medium33 => 33,
                GroundResolution.High65 => 65,
                GroundResolution.VeryHigh129 => 129,
                _ => 33
            };
        }

        private static float EvaluateTransition(
            float u,
            float v,
            GroundTransitionDirection direction,
            float transitionHeight)
        {
            if (direction == GroundTransitionDirection.None ||
                Mathf.Approximately(transitionHeight, 0f))
            {
                return 0f;
            }

            Vector2 directionVector =
                ResolveTransitionVector(direction);

            Vector2 centred = new Vector2(
                u * 2f - 1f,
                v * 2f - 1f);

            Vector2 normalizedDirection =
                directionVector.normalized;

            float maximumProjection =
                Mathf.Abs(normalizedDirection.x) +
                Mathf.Abs(normalizedDirection.y);

            float projection =
                Vector2.Dot(
                    centred,
                    normalizedDirection);

            float normalizedProjection =
                projection /
                Mathf.Max(0.0001f, maximumProjection);

            float transitionT =
                Mathf.Clamp01(
                    normalizedProjection * 0.5f + 0.5f);

            transitionT =
                transitionT *
                transitionT *
                (3f - 2f * transitionT);

            return transitionHeight * transitionT;
        }

        private static Vector2 ResolveTransitionVector(
            GroundTransitionDirection direction)
        {
            return direction switch
            {
                GroundTransitionDirection.North => Vector2.up,
                GroundTransitionDirection.South => Vector2.down,
                GroundTransitionDirection.East => Vector2.right,
                GroundTransitionDirection.West => Vector2.left,
                GroundTransitionDirection.NorthEast => new Vector2(1f, 1f),
                GroundTransitionDirection.NorthWest => new Vector2(-1f, 1f),
                GroundTransitionDirection.SouthEast => new Vector2(1f, -1f),
                GroundTransitionDirection.SouthWest => new Vector2(-1f, -1f),
                _ => Vector2.zero
            };
        }

        private static float EvaluateBroadProfile(
            GroundRecipe recipe,
            Vector2 samplePosition,
            float u,
            float v)
        {
            if (recipe.Profile == GroundProfile.Flat ||
                recipe.BroadForm <= 0f)
            {
                return 0f;
            }

            float frequency =
                Mathf.Lerp(
                    0.018f,
                    0.075f,
                    recipe.Roughness);

            float noiseA =
                SampleCenteredPerlin(
                    samplePosition,
                    frequency,
                    recipe.ShapeSeed,
                    0x3A17);

            float noiseB =
                SampleCenteredPerlin(
                    samplePosition,
                    frequency * 0.52f,
                    recipe.ShapeSeed,
                    0x7B31);

            float normalizedX = u * 2f - 1f;
            float normalizedZ = v * 2f - 1f;
            float value;

            switch (recipe.Profile)
            {
                case GroundProfile.Rolling:
                    value = noiseA * 0.72f + noiseB * 0.28f;
                    break;

                case GroundProfile.Basin:
                {
                    float radial =
                        Mathf.Clamp01(
                            Mathf.Sqrt(
                                normalizedX * normalizedX +
                                normalizedZ * normalizedZ));

                    float basin =
                        -(1f - radial) *
                        (1f - radial);

                    value =
                        basin * 0.78f +
                        noiseA * 0.22f;
                    break;
                }

                case GroundProfile.Ridge:
                {
                    float angle =
                        Hash01(
                            recipe.ShapeSeed,
                            0x4D29) *
                        Mathf.PI;

                    Vector2 ridgeNormal =
                        new Vector2(
                            Mathf.Cos(angle),
                            Mathf.Sin(angle));

                    float distance =
                        Mathf.Abs(
                            Vector2.Dot(
                                new Vector2(
                                    normalizedX,
                                    normalizedZ),
                                ridgeNormal));

                    float ridge =
                        Mathf.Pow(
                            Mathf.Clamp01(1f - distance),
                            2.2f);

                    value =
                        ridge * 0.82f +
                        noiseA * 0.18f;
                    break;
                }

                case GroundProfile.Uneven:
                {
                    float directional =
                        Mathf.Sin(
                            (samplePosition.x + samplePosition.y) *
                            frequency *
                            3.1f +
                            recipe.ShapeSeed * 0.013f);

                    value =
                        noiseA * 0.58f +
                        noiseB * 0.24f +
                        directional * 0.18f;
                    break;
                }

                default:
                    value = 0f;
                    break;
            }

            return value * recipe.BroadForm;
        }

        private static float EvaluateSurfaceDetail(
            GroundRecipe recipe,
            Vector2 samplePosition)
        {
            if (recipe.SurfaceDetail <= 0f)
            {
                return 0f;
            }

            float frequency =
                Mathf.Lerp(
                    0.11f,
                    0.23f,
                    recipe.Roughness);

            float detail =
                SampleCenteredPerlin(
                    samplePosition,
                    frequency,
                    recipe.ShapeSeed,
                    0x5EED);

            float secondary =
                SampleCenteredPerlin(
                    samplePosition,
                    frequency * 1.83f,
                    recipe.ShapeSeed,
                    0x71C3);

            float amplitude =
                recipe.SurfaceDetail * 0.42f;

            return
                (detail * 0.72f +
                 secondary * 0.28f) *
                amplitude;
        }

        private static float EvaluateEdgeMask(
            float u,
            float v,
            GroundEdgeBlend edgeBlend)
        {
            float blendWidth = edgeBlend switch
            {
                GroundEdgeBlend.Narrow => 0.08f,
                GroundEdgeBlend.Medium => 0.16f,
                GroundEdgeBlend.Wide => 0.25f,
                _ => 0.16f
            };

            float distanceToEdge =
                Mathf.Min(
                    Mathf.Min(u, 1f - u),
                    Mathf.Min(v, 1f - v));

            float t =
                Mathf.Clamp01(
                    distanceToEdge /
                    Mathf.Max(0.0001f, blendWidth));

            return t * t * (3f - 2f * t);
        }

        private static void ApplyModifiers(
            float[] heights,
            float[] detailHeights,
            int resolution,
            float spacing,
            float halfSize,
            IReadOnlyList<GroundModifierSnapshot> modifiers)
        {
            if (modifiers == null ||
                modifiers.Count == 0)
            {
                return;
            }

            for (int modifierIndex = 0;
                 modifierIndex < modifiers.Count;
                 modifierIndex++)
            {
                GroundModifierSnapshot modifier =
                    modifiers[modifierIndex];

                for (int z = 0; z < resolution; z++)
                {
                    float localZ =
                        -halfSize + z * spacing;

                    for (int x = 0; x < resolution; x++)
                    {
                        float localX =
                            -halfSize + x * spacing;

                        int index =
                            z * resolution + x;

                        Vector2 point =
                            new Vector2(localX, localZ);

                        float weight =
                            modifier.EvaluateWeight(point);

                        if (weight <= 0f)
                        {
                            continue;
                        }

                        float influence =
                            weight * modifier.Strength;

                        switch (modifier.Mode)
                        {
                            case GroundModifierMode.Flatten:
                            {
                                float targetHeight =
                                    modifier.TargetHeight +
                                    detailHeights[index] *
                                    modifier.PreserveDetail;

                                heights[index] =
                                    Mathf.Lerp(
                                        heights[index],
                                        targetHeight,
                                        influence);
                                break;
                            }

                            case GroundModifierMode.Raise:
                                heights[index] +=
                                    modifier.HeightAmount *
                                    influence;
                                break;

                            case GroundModifierMode.Lower:
                                heights[index] -=
                                    modifier.HeightAmount *
                                    influence;
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
        }

        private static void ApplyRivers(
            float[] heights,
            int resolution,
            float spacing,
            float halfSize,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers)
        {
            if (rivers == null ||
                rivers.Count == 0)
            {
                return;
            }

            for (int riverIndex = 0;
                 riverIndex < rivers.Count;
                 riverIndex++)
            {
                StylizedRiverGroundSnapshot river =
                    rivers[riverIndex];

                if (!river.IsValid)
                {
                    continue;
                }

                for (int z = 0;
                     z < resolution;
                     z++)
                {
                    float localZ =
                        -halfSize +
                        z * spacing;

                    for (int x = 0;
                         x < resolution;
                         x++)
                    {
                        float localX =
                            -halfSize +
                            x * spacing;

                        int index =
                            z * resolution +
                            x;

                        Vector2 point =
                            new Vector2(
                                localX,
                                localZ);

                        if (!river.TryEvaluate(
                                point,
                                out float distance,
                                out float waterHeight,
                                out _,
                                out float surfaceHalfWidth) ||
                            distance >
                            river.ResolveHandoffHalfWidth(surfaceHalfWidth))
                        {
                            continue;
                        }

                        heights[index] =
                            river.EvaluateConcealedGroundHeight(
                                heights[index],
                                distance,
                                waterHeight,
                                surfaceHalfWidth);
                    }
                }
            }
        }

        private static Vector3[] BuildHeightFieldNormals(
            float[] heights,
            int resolution,
            float spacing,
            int triangulationSeed)
        {
            Vector3[] normals =
                new Vector3[heights != null ? heights.Length : 0];

            if (heights == null ||
                resolution < 2 ||
                heights.Length != resolution * resolution)
            {
                return normals;
            }

            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int a = z * resolution + x;
                    int b = a + 1;
                    int c = a + resolution;
                    int d = c + 1;

                    Vector3 pa =
                        new Vector3(
                            x * spacing,
                            heights[a],
                            z * spacing);
                    Vector3 pb =
                        new Vector3(
                            (x + 1) * spacing,
                            heights[b],
                            z * spacing);
                    Vector3 pc =
                        new Vector3(
                            x * spacing,
                            heights[c],
                            (z + 1) * spacing);
                    Vector3 pd =
                        new Vector3(
                            (x + 1) * spacing,
                            heights[d],
                            (z + 1) * spacing);

                    bool alternate =
                        ((x + z + triangulationSeed) & 1) == 0;

                    if (alternate)
                    {
                        AccumulateFaceNormal(normals, a, c, b, pa, pc, pb);
                        AccumulateFaceNormal(normals, b, c, d, pb, pc, pd);
                    }
                    else
                    {
                        AccumulateFaceNormal(normals, a, d, b, pa, pd, pb);
                        AccumulateFaceNormal(normals, a, c, d, pa, pc, pd);
                    }
                }
            }

            for (int index = 0; index < normals.Length; index++)
            {
                normals[index] =
                    normals[index].sqrMagnitude > 0.000001f
                        ? normals[index].normalized
                        : Vector3.up;
            }

            return normals;
        }

        private static void AccumulateFaceNormal(
            Vector3[] normals,
            int a,
            int b,
            int c,
            Vector3 pa,
            Vector3 pb,
            Vector3 pc)
        {
            Vector3 faceNormal =
                Vector3.Cross(pb - pa, pc - pa);

            if (faceNormal.sqrMagnitude <= 0.0000001f)
            {
                return;
            }

            normals[a] += faceNormal;
            normals[b] += faceNormal;
            normals[c] += faceNormal;
        }

        private static void BuildSurfaceMetadata(
            GroundRecipe recipe,
            GroundSurfaceProfile surfaceProfile,
            float[] finalHeights,
            Vector3[] normals,
            IReadOnlyList<GroundModifierSnapshot> modifiers,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers,
            int resolution,
            float spacing,
            float halfSize,
            out float[] surfaceVariations,
            out float[] exposureMasks,
            out float[] dampDepositMasks,
            out float[] vegetationSuitabilityMasks,
            out float[] materialClassifications,
            out Vector4[] secondarySurfaceMasks)
        {
            surfaceVariations = new float[finalHeights.Length];
            exposureMasks = new float[finalHeights.Length];
            dampDepositMasks = new float[finalHeights.Length];
            vegetationSuitabilityMasks = new float[finalHeights.Length];
            materialClassifications = new float[finalHeights.Length];
            secondarySurfaceMasks = new Vector4[finalHeights.Length];

            float minimumHeight = float.PositiveInfinity;
            float maximumHeight = float.NegativeInfinity;

            for (int index = 0; index < finalHeights.Length; index++)
            {
                minimumHeight =
                    Mathf.Min(minimumHeight, finalHeights[index]);

                maximumHeight =
                    Mathf.Max(maximumHeight, finalHeights[index]);
            }

            float heightRange =
                Mathf.Max(
                    0.001f,
                    maximumHeight - minimumHeight);

            float patchScale =
                GroundSurfaceProfile.ResolvePatchScale(surfaceProfile);

            float patchContrast =
                GroundSurfaceProfile.ResolvePatchContrast(surfaceProfile);

            float patchSoftness =
                GroundSurfaceProfile.ResolvePatchEdgeSoftness(surfaceProfile);

            float exposureBias =
                GroundSurfaceProfile.ResolveExposureBias(surfaceProfile);

            float dampBias =
                GroundSurfaceProfile.ResolveDampDepositBias(surfaceProfile);

            float vegetationBias =
                GroundSurfaceProfile.ResolveVegetationSuitability(surfaceProfile);

            float rockyDryBias =
                GroundSurfaceProfile.ResolveRockyDrySuitability(surfaceProfile);

            float snowEligibility =
                GroundSurfaceProfile.ResolveSnowEligibility(surfaceProfile);

            float rainAbsorption =
                GroundSurfaceProfile.ResolveRainAbsorption(surfaceProfile);

            float profileTonalInfluence = surfaceProfile != null ? 0.35f : 0f;

            Vector2 noiseOrigin = new Vector2(
                recipe.PatchCoordinate.x * ResolvePatchSize(recipe.PatchSize),
                recipe.PatchCoordinate.y * ResolvePatchSize(recipe.PatchSize));

            for (int z = 0; z < resolution; z++)
            {
                float localZ = -halfSize + z * spacing;

                for (int x = 0; x < resolution; x++)
                {
                    float localX = -halfSize + x * spacing;
                    int index = z * resolution + x;

                    Vector2 point = new Vector2(localX, localZ);
                    Vector2 samplePosition = noiseOrigin + point;

                    float randomValue =
                        Hash01(
                            recipe.ShapeSeed,
                            index ^ 0x2F61);

                    float heightValue =
                        Mathf.InverseLerp(
                            minimumHeight,
                            minimumHeight + heightRange,
                            finalHeights[index]);

                    float combinedVariation =
                        Mathf.Lerp(
                            randomValue,
                            heightValue,
                            0.28f);

                    float compatibleVariation =
                        Mathf.Clamp01(
                            0.5f +
                            (combinedVariation - 0.5f) *
                            recipe.SurfaceVariation);

                    float broadPatch =
                        EvaluateBroadSurfacePatch(
                            samplePosition,
                            patchScale,
                            patchSoftness,
                            recipe.ShapeSeed);

                    float profiledVariation =
                        Mathf.Clamp01(
                            0.5f +
                            (broadPatch - 0.5f) *
                            recipe.SurfaceVariation *
                            Mathf.Lerp(0.65f, 1.6f, patchContrast));

                    surfaceVariations[index] =
                        Mathf.Lerp(
                            compatibleVariation,
                            profiledVariation,
                            profileTonalInfluence);

                    Vector3 normal =
                        normals != null && index < normals.Length
                            ? normals[index]
                            : Vector3.up;

                    float upFacing =
                        Mathf.Clamp01(normal.normalized.y);

                    float slope = 1f - upFacing;

                    float compaction =
                        EvaluateCompactionInfluence(
                            point,
                            modifiers);

                    float shore =
                        EvaluateShoreInfluence(
                            point,
                            spacing,
                            rivers);

                    float dryPatch =
                        EvaluateBroadSurfacePatch(
                            samplePosition + new Vector2(37.7f, -19.3f),
                            patchScale * 0.72f,
                            patchSoftness,
                            recipe.ShapeSeed ^ 0x4D91);

                    float rockyDry =
                        Mathf.Clamp01(
                            rockyDryBias * 0.52f +
                            dryPatch * rockyDryBias * 0.30f +
                            slope * rockyDryBias * 0.18f);

                    float lowArea = 1f - heightValue;
                    float flatness = upFacing;

                    float exposure =
                        Mathf.Clamp01(
                            upFacing * 0.44f +
                            heightValue * 0.22f +
                            broadPatch * 0.16f +
                            exposureBias * 0.18f);

                    exposure *= Mathf.Lerp(0.55f, 1.08f, snowEligibility);
                    exposure *= Mathf.Lerp(1f, 0.78f, shore * rainAbsorption);
                    exposure *= Mathf.Lerp(1f, 0.88f, compaction);
                    exposureMasks[index] = Mathf.Clamp01(exposure);

                    float dampDeposit =
                        Mathf.Clamp01(
                            lowArea * 0.34f +
                            flatness * 0.12f +
                            shore * 0.28f +
                            compaction * 0.08f +
                            dampBias * 0.18f);

                    dampDeposit *= Mathf.Lerp(0.72f, 1.18f, rainAbsorption);
                    dampDepositMasks[index] = Mathf.Clamp01(dampDeposit);

                    float midMoisture =
                        1f - Mathf.Abs(dampDepositMasks[index] - 0.56f) / 0.56f;

                    float vegetation =
                        Mathf.Clamp01(
                            vegetationBias * 0.50f +
                            flatness * vegetationBias * 0.18f +
                            midMoisture * vegetationBias * 0.18f +
                            broadPatch * vegetationBias * 0.14f);

                    vegetation *= Mathf.Lerp(1f, 0.65f, compaction);
                    vegetation *= Mathf.Lerp(1f, 0.72f, shore);
                    vegetation *= Mathf.Lerp(1f, 0.58f, rockyDry);
                    vegetationSuitabilityMasks[index] = Mathf.Clamp01(vegetation);

                    // Reserved for future terrain material routing. Keeping the
                    // channel explicit now prevents another snapshot/API refactor.
                    materialClassifications[index] = 0f;

                    // UV2 contract for future shader/runtime use:
                    // X = compaction/path/flatten influence
                    // Y = river/shore influence
                    // Z = rocky/dry secondary patch
                    // W = reserved authored mask or secondary profile blend
                    secondarySurfaceMasks[index] =
                        new Vector4(
                            Mathf.Clamp01(compaction),
                            Mathf.Clamp01(shore),
                            Mathf.Clamp01(rockyDry),
                            0f);
                }
            }
        }

        private static float EvaluateBroadSurfacePatch(
            Vector2 samplePosition,
            float patchScale,
            float edgeSoftness,
            int seed)
        {
            float scale = Mathf.Max(2f, patchScale);
            float baseFrequency = 1f / scale;

            Vector2 warp = new Vector2(
                SampleCenteredPerlin(
                    samplePosition,
                    baseFrequency * 0.47f,
                    seed,
                    0x32A1),
                SampleCenteredPerlin(
                    samplePosition,
                    baseFrequency * 0.47f,
                    seed,
                    0x6A35));

            Vector2 warpedPosition =
                samplePosition + warp * scale * 0.24f;

            float broad =
                Sample01(
                    warpedPosition,
                    baseFrequency,
                    seed,
                    0x2387);

            float secondary =
                Sample01(
                    warpedPosition,
                    baseFrequency * 2.15f,
                    seed,
                    0x6B59);

            float value =
                Mathf.Clamp01(
                    broad * 0.74f +
                    secondary * 0.26f);

            float smooth = value * value * (3f - 2f * value);
            float posterized = Mathf.Round(value * 4f) * 0.25f;
            float stylized = Mathf.Lerp(posterized, smooth, edgeSoftness);

            return Mathf.Clamp01(
                Mathf.Lerp(value, stylized, 0.55f));
        }

        private static float EvaluateCompactionInfluence(
            Vector2 point,
            IReadOnlyList<GroundModifierSnapshot> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
            {
                return 0f;
            }

            float influence = 0f;

            for (int index = 0; index < modifiers.Count; index++)
            {
                GroundModifierSnapshot modifier = modifiers[index];

                if (modifier.Mode != GroundModifierMode.Flatten)
                {
                    continue;
                }

                influence =
                    Mathf.Max(
                        influence,
                        modifier.EvaluateWeight(point) * modifier.Strength);
            }

            return Mathf.Clamp01(influence);
        }

        private static float EvaluateShoreInfluence(
            Vector2 point,
            float spacing,
            IReadOnlyList<StylizedRiverGroundSnapshot> rivers)
        {
            if (rivers == null || rivers.Count == 0)
            {
                return 0f;
            }

            float influence = 0f;

            for (int index = 0; index < rivers.Count; index++)
            {
                StylizedRiverGroundSnapshot river = rivers[index];

                if (!river.IsValid ||
                    !river.TryEvaluate(
                        point,
                        out float distance,
                        out _,
                        out _,
                        out float surfaceHalfWidth))
                {
                    continue;
                }

                float handoffHalfWidth =
                    river.ResolveHandoffHalfWidth(surfaceHalfWidth);

                float outerWidth =
                    handoffHalfWidth +
                    Mathf.Max(spacing * 2f, river.BankBlend * 0.75f);

                float riverInfluence =
                    1f - SmoothStep(
                        surfaceHalfWidth,
                        outerWidth,
                        distance);

                influence = Mathf.Max(influence, riverInfluence);
            }

            return Mathf.Clamp01(influence);
        }

        private static MeshData BuildMeshData(
            float[] heights,
            Vector3[] normals,
            float[] surfaceVariations,
            float[] exposureMasks,
            float[] dampDepositMasks,
            float[] vegetationSuitabilityMasks,
            Vector4[] secondarySurfaceMasks,
            int resolution,
            float spacing,
            float halfSize,
            int triangulationSeed)
        {
            MeshData meshData = new MeshData();

            for (int z = 0; z < resolution; z++)
            {
                float v = z / (float)(resolution - 1);
                float localZ = -halfSize + z * spacing;

                for (int x = 0; x < resolution; x++)
                {
                    float u = x / (float)(resolution - 1);
                    float localX = -halfSize + x * spacing;
                    int index = z * resolution + x;

                    meshData.AddVertex(
                        new Vector3(
                            localX,
                            heights[index],
                            localZ),
                        new Vector2(u, v),
                        new Color(
                            surfaceVariations[index],
                            exposureMasks[index],
                            dampDepositMasks[index],
                            vegetationSuitabilityMasks[index]),
                        secondarySurfaceMasks[index]);

                    meshData.Normals.Add(normals[index]);
                }
            }

            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int a = z * resolution + x;
                    int b = a + 1;
                    int c = a + resolution;
                    int d = c + 1;

                    bool alternate =
                        ((x + z + triangulationSeed) & 1) == 0;

                    if (alternate)
                    {
                        meshData.AddTriangle(a, c, b);
                        meshData.AddTriangle(b, c, d);
                    }
                    else
                    {
                        meshData.AddTriangle(a, d, b);
                        meshData.AddTriangle(a, c, d);
                    }
                }
            }

            return meshData;
        }

        private static float Sample01(
            Vector2 position,
            float frequency,
            int seed,
            int salt)
        {
            return SampleCenteredPerlin(position, frequency, seed, salt) * 0.5f + 0.5f;
        }

        private static float SmoothStep(
            float edge0,
            float edge1,
            float value)
        {
            float t = Mathf.InverseLerp(edge0, edge1, value);
            return t * t * (3f - 2f * t);
        }

        private static float SampleCenteredPerlin(
            Vector2 position,
            float frequency,
            int seed,
            int salt)
        {
            float offsetX =
                Hash01(seed, salt) * 2048f + 31.7f;

            float offsetY =
                Hash01(seed, salt ^ 0x6D2B) *
                2048f +
                79.3f;

            float value =
                Mathf.PerlinNoise(
                    position.x * frequency + offsetX,
                    position.y * frequency + offsetY);

            return value * 2f - 1f;
        }

        private static float Hash01(
            int seed,
            int value)
        {
            unchecked
            {
                uint hash = (uint)seed;
                hash ^= (uint)value + 0x9E3779B9u +
                        (hash << 6) +
                        (hash >> 2);

                hash ^= hash >> 16;
                hash *= 0x7FEB352Du;
                hash ^= hash >> 15;
                hash *= 0x846CA68Bu;
                hash ^= hash >> 16;

                return
                    (hash & 0x00FFFFFFu) /
                    16777215f;
            }
        }
    }
}
