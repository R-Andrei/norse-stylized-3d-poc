using System;
using System.Collections.Generic;
using UnityEngine;
using ProgrammaticStylized3D.Geometry;

namespace ProgrammaticStylized3D.Geometry.Ground
{
    public static class GroundGenerator
    {
        public static MeshData Generate(
            GroundRecipe recipe,
            IReadOnlyList<GroundModifierSnapshot> modifiers)
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

            return BuildMeshData(
                recipe,
                heights,
                resolution,
                spacing,
                halfSize);
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

        private static MeshData BuildMeshData(
            GroundRecipe recipe,
            float[] heights,
            int resolution,
            float spacing,
            float halfSize)
        {
            MeshData meshData = new MeshData();

            float minimumHeight = float.PositiveInfinity;
            float maximumHeight = float.NegativeInfinity;

            for (int i = 0; i < heights.Length; i++)
            {
                minimumHeight =
                    Mathf.Min(minimumHeight, heights[i]);

                maximumHeight =
                    Mathf.Max(maximumHeight, heights[i]);
            }

            float heightRange =
                Mathf.Max(
                    0.001f,
                    maximumHeight - minimumHeight);

            for (int z = 0; z < resolution; z++)
            {
                float v = z / (float)(resolution - 1);
                float localZ = -halfSize + z * spacing;

                for (int x = 0; x < resolution; x++)
                {
                    float u = x / (float)(resolution - 1);
                    float localX = -halfSize + x * spacing;
                    int index = z * resolution + x;

                    float randomValue =
                        Hash01(
                            recipe.ShapeSeed,
                            index ^ 0x2F61);

                    float heightValue =
                        Mathf.InverseLerp(
                            minimumHeight,
                            minimumHeight + heightRange,
                            heights[index]);

                    float combinedVariation =
                        Mathf.Lerp(
                            randomValue,
                            heightValue,
                            0.28f);

                    float red =
                        Mathf.Clamp01(
                            0.5f +
                            (combinedVariation - 0.5f) *
                            recipe.SurfaceVariation);

                    meshData.AddVertex(
                        new Vector3(
                            localX,
                            heights[index],
                            localZ),
                        new Vector2(u, v),
                        new Color(
                            red,
                            0.5f,
                            0.5f,
                            1f));
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
                        ((x + z + recipe.ShapeSeed) & 1) == 0;

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
