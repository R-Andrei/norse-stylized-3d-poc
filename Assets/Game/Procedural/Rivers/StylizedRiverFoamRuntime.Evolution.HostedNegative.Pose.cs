using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private static HostedNegativeEvolutionPose
            CreateIdentityHostedNegativePose()
        {
            return new HostedNegativeEvolutionPose
            {
                OffsetCells = Vector2.zero,
                RotationRadians = 0f,
                ScaleAlong = 1f,
                ScaleAcross = 1f,
                StrengthScale = 1f
            };
        }

        private static HostedNegativeEvolutionPose
            ResolveHostedNegativeTarget(
                StylizedRiverFoamPreparedHostedNegativeRegion prepared,
                uint seed,
                int currentVariantIndex,
                out int targetVariantIndex)
        {
            IReadOnlyList<StylizedRiverFoamHostedNegativeVariant> variants =
                prepared.Variants;
            if (variants == null || variants.Count == 0)
            {
                targetVariantIndex = 0;
                return CreateIdentityHostedNegativePose();
            }

            int count = variants.Count;
            targetVariantIndex = Mathf.Clamp(currentVariantIndex, 0, count - 1);
            if (count > 1)
            {
                int offset = 1 + Mathf.FloorToInt(
                    HashMajorEvolution(seed, 2u) * (count - 1));
                targetVariantIndex = (targetVariantIndex + offset) % count;
            }

            StylizedRiverFoamHostedNegativeVariant variant =
                variants[targetVariantIndex];
            HostedNegativeEvolutionPose pose = new HostedNegativeEvolutionPose
            {
                OffsetCells = variant.OffsetCells,
                RotationRadians = variant.RotationRadians,
                ScaleAlong = variant.ScaleAlong,
                ScaleAcross = variant.ScaleAcross,
                StrengthScale = 1f
            };
            float areaScale = Mathf.Max(
                0.25f,
                pose.ScaleAlong * pose.ScaleAcross);
            pose.StrengthScale = Mathf.Clamp(
                1f / Mathf.Sqrt(areaScale),
                0.90f,
                1.10f);
            return pose;
        }
    }
}
