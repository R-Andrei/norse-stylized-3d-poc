using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
        private readonly struct PendingInjection
        {
            public PendingInjection(
                float globalDistance,
                float acrossNormalized,
                float radius,
                float amount,
                float remainingLife,
                float integrity,
                float phase,
                float elongation,
                bool isManual,
                float shapeSeed = 0f,
                float shapeVariety = 0f,
                bool compoundShape = false)
            {
                GlobalDistance = globalDistance;
                AcrossNormalized = acrossNormalized;
                Radius = radius;
                Amount = amount;
                RemainingLife = remainingLife;
                Integrity = integrity;
                Phase = phase;
                Elongation = elongation;
                IsManual = isManual;
                ShapeSeed = shapeSeed;
                ShapeVariety = shapeVariety;
                CompoundShape = compoundShape;
            }

            public float GlobalDistance { get; }
            public float AcrossNormalized { get; }
            public float Radius { get; }
            public float Amount { get; }
            public float RemainingLife { get; }
            public float Integrity { get; }
            public float Phase { get; }
            public float Elongation { get; }
            public bool IsManual { get; }
            public float ShapeSeed { get; }
            public float ShapeVariety { get; }
            public bool CompoundShape { get; }
        }

        private sealed class FoamReservation
        {
            public float CentreGlobalDistance;
            public float AlongRadius;
            public float RemainingAmount;
            public float Elapsed;
            public float MaximumLifetime;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamMetricRow
        {
            public Vector4 WidthsAndSpacing;
            public Vector4 TopologyData;
            public Vector4 ShoreData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamObstacleIntervalCellData
        {
            public Vector4 CoordinateAndOffset;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamMajorEvolutionData
        {
            public Vector4 CentreAndPlacement;
            public Vector4 CandidateShape;
            public Vector4 CandidateExtents;
            public Vector4 Morph;
            public Vector4 Warp;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamHostedNegativeEvolutionData
        {
            public Vector4 HostAndMask;
            public Vector4 CentreAndOffset;
            public Vector4 Morph;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamFreeWaterEvolutionData
        {
            public Vector4 CentreAndPlacement;
            public Vector4 MaskAndStrength;
            public Vector4 Morph;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamConnectorIdentityData
        {
            // x = first flattened path point, y = point count,
            // z = outer radius, w = core radius.
            public Vector4 PointRangeAndRadii;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamWeakSpanIdentityData
        {
            // x = Connector record index, y = normalized path distance,
            // z/w = gate-safe normalized interval.
            public Vector4 ConnectorAndPath;
            // x/y = physical along/across radii, z = pressure strength,
            // w = accepted identity orientation.
            public Vector4 Shape;
            // x = static irregular-boundary noise seed; remaining lanes reserved.
            public uint NoiseSeed;
            public uint Reserved0;
            public uint Reserved1;
            public uint Reserved2;
        }

        private enum ConnectorReleaseReason
        {
            None,
            Unavailable,
            Turnover,
            StretchBreak
        }

        private struct ConnectorRelationshipCandidate
        {
            public StylizedRiverFoamConnectorPath Path;
            public int StartHostSlotIndex;
            public int EndHostSlotIndex;
            public float BasePathLengthMetres;
            public float SelectionWeight;
        }

        private struct ConnectorEvolutionSlot
        {
            public uint StableId;
            public int OriginalCandidateIndex;
            public int AssignedCandidateIndex;
            public int ActiveCandidateIndex;
            public int LastReleasedCandidateIndex;
            public int ReleaseCooldownTicks;
            public int RelationshipRevision;
            public int PointOffset;
            public int PointCapacity;
            public int PointCount;
            public int ActiveStartAnchorIndex;
            public int ActiveEndAnchorIndex;
            public ConnectorReleaseReason PendingReleaseReason;
            public int TurnoverFallbackCandidateIndex;
            public float ReferenceLengthMetres;
            public int ReferenceCandidateIndex;
            public int ReferenceStartAnchorIndex;
            public int ReferenceEndAnchorIndex;
            public int ObservedStartRecycleCount;
            public int ObservedEndRecycleCount;
            public int StretchBlockedCandidateIndex;
            public int StretchBlockedStartRecycleCount;
            public int StretchBlockedEndRecycleCount;
            public bool IsActive;
            public bool HasRuntimeState;
        }

        private struct MajorEvolutionPose
        {
            public float LocalDistance;
            public float AcrossNormalized;
            public float OrientationRadians;
            public float MetresPerCandidateCell;
            public float ScaleAlong;
            public float ScaleAcross;
            public float Shear;
            public float WarpAlong;
            public float WarpAcross;
            public float WarpPhaseA;
            public float WarpPhaseB;
            public float SupportScale;
        }

        private struct MajorEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public float BaseMetresPerCandidateCell;
            public MajorEvolutionPose Current;
            public MajorEvolutionPose Start;
            public MajorEvolutionPose Target;
            public float DwellRemaining;
            public float LastDwellDuration;
            public float MoveElapsed;
            public float MoveDuration;
            public float OccurrenceElapsed;
            public float LifetimeUnitBudget;
            public float MaximumOccurrenceSeconds;
            public int HopIndex;
            public int RecycleCount;
            public int LastAnchorIndex;
            public bool IsMoving;
        }

        private struct HostedNegativeEvolutionPose
        {
            public Vector2 OffsetCells;
            public float RotationRadians;
            public float ScaleAlong;
            public float ScaleAcross;
            public float StrengthScale;
        }

        private struct HostedNegativeEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public int HostSlotIndex;
            public StylizedRiverFoamNegativeRegionClass RegionClass;
            public int CurrentVariantIndex;
            public int TargetVariantIndex;
            public HostedNegativeEvolutionPose Current;
            public HostedNegativeEvolutionPose Start;
            public HostedNegativeEvolutionPose Target;
        }

        private struct FreeWaterEvolutionPose
        {
            public float LocalDistance;
            public float AcrossNormalized;
            public float OrientationRadians;
            public float ScaleAlong;
            public float ScaleAcross;
            public float StrengthScale;
        }

        private struct FreeWaterEvolutionSlot
        {
            public uint StableId;
            public int PreparedIndex;
            public FreeWaterEvolutionPose Current;
            public FreeWaterEvolutionPose Start;
            public FreeWaterEvolutionPose Target;
            public float DwellRemaining;
            public float LastDwellDuration;
            public float MoveElapsed;
            public float MoveDuration;
            public float OccurrenceElapsed;
            public float LifetimeUnitBudget;
            public float MaximumOccurrenceSeconds;
            public int HopIndex;
            public int RecycleCount;
            public int LastAnchorIndex;
            public bool IsMoving;
        }
    }
}
