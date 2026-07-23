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
                float sourceAmount,
                float remainingLife,
                float patternSeed,
                float elongation,
                bool isManual,
                float sourceFillSeed,
                float sourceFillFeatureSize,
                float shapeSeed = 0f,
                float shapeVariety = 0f,
                bool compoundShape = false,
                bool segmentShape = false,
                float segmentStartGlobalDistance = 0f,
                float segmentStartAcrossNormalized = 0f,
                float segmentStartRadius = 0f,
                float segmentStartAmount = 0f,
                float segmentEndGlobalDistance = 0f,
                float segmentEndAcrossNormalized = 0f,
                float segmentEndRadius = 0f,
                float segmentEndAmount = 0f,
                bool usesMetricLateral = false,
                float lateralMetres = 0f,
                float segmentStartLateralMetres = 0f,
                float segmentEndLateralMetres = 0f)
            {
                GlobalDistance = globalDistance;
                AcrossNormalized = acrossNormalized;
                Radius = radius;
                SourceAmount = sourceAmount;
                RemainingLife = remainingLife;
                PatternSeed = patternSeed;
                Elongation = elongation;
                IsManual = isManual;
                SourceFillSeed = sourceFillSeed;
                SourceFillFeatureSize = sourceFillFeatureSize;
                ShapeSeed = shapeSeed;
                ShapeVariety = shapeVariety;
                CompoundShape = compoundShape;
                SegmentShape = segmentShape;
                SegmentStartGlobalDistance = segmentShape
                    ? segmentStartGlobalDistance
                    : globalDistance;
                SegmentStartAcrossNormalized = segmentShape
                    ? segmentStartAcrossNormalized
                    : acrossNormalized;
                SegmentStartRadius = segmentShape
                    ? segmentStartRadius
                    : radius;
                SegmentStartSourceAmount = segmentShape
                    ? segmentStartAmount
                    : sourceAmount;
                SegmentEndGlobalDistance = segmentShape
                    ? segmentEndGlobalDistance
                    : globalDistance;
                SegmentEndAcrossNormalized = segmentShape
                    ? segmentEndAcrossNormalized
                    : acrossNormalized;
                SegmentEndRadius = segmentShape
                    ? segmentEndRadius
                    : radius;
                SegmentEndSourceAmount = segmentShape
                    ? segmentEndAmount
                    : sourceAmount;
                UsesMetricLateral = usesMetricLateral;
                LateralMetres = usesMetricLateral
                    ? lateralMetres
                    : 0f;
                SegmentStartLateralMetres = usesMetricLateral && segmentShape
                    ? segmentStartLateralMetres
                    : LateralMetres;
                SegmentEndLateralMetres = usesMetricLateral && segmentShape
                    ? segmentEndLateralMetres
                    : LateralMetres;
            }

            public float GlobalDistance { get; }
            public float AcrossNormalized { get; }
            public float Radius { get; }
            public float SourceAmount { get; }
            public float RemainingLife { get; }
            public float PatternSeed { get; }
            public float Elongation { get; }
            public bool IsManual { get; }
            public float SourceFillSeed { get; }
            public float SourceFillFeatureSize { get; }
            public float ShapeSeed { get; }
            public float ShapeVariety { get; }
            public bool CompoundShape { get; }
            public bool SegmentShape { get; }
            public float SegmentStartGlobalDistance { get; }
            public float SegmentStartAcrossNormalized { get; }
            public float SegmentStartRadius { get; }
            public float SegmentStartSourceAmount { get; }
            public float SegmentEndGlobalDistance { get; }
            public float SegmentEndAcrossNormalized { get; }
            public float SegmentEndRadius { get; }
            public float SegmentEndSourceAmount { get; }
            public bool UsesMetricLateral { get; }
            public float LateralMetres { get; }
            public float SegmentStartLateralMetres { get; }
            public float SegmentEndLateralMetres { get; }
        }

        private enum AutomaticFoamSourceEventType
        {
            None = 0,
            ShoreRibbon = 1,
            InwardWash = 2,
            ObjectContactArc = 3,
            ObjectContactFleck = 4,
            ObjectContactSemiArc = 5,
            FreeWaterLaceConnector = 6,
            FreeWaterTornFragment = 7,
            FreeWaterCrossLaceConnector = 8
        }

        private struct AutomaticObjectSourceState
        {
            public int CycleIndex;
            public float NextStartTime;
            public AutomaticFoamSourceEventType LastEventType;
        }

        private struct AutomaticRevealTimingTelemetry
        {
            public bool HasValue;
            public int EventId;
            public AutomaticFoamSourceEventType Type;
            public float PathDistanceMetres;
            public float RequestedSpeedMetresPerSecond;
            public float RawDurationSeconds;
            public float ResolvedDurationSeconds;
            public float ActualSpeedMetresPerSecond;
            public bool CadenceLimited;
        }

        private struct AutomaticFoamSourceEvent
        {
            public bool Active;
            public int EventId;
            public AutomaticFoamSourceEventType Type;
            public EntityId ObjectSourceId;
            public float SideSign;
            public float StartGlobalDistance;
            public float EndGlobalDistance;
            public float ObjectCentreGlobalDistance;
            public float Duration;
            public float Elapsed;
            public float ObjectBuildDuration;
            public float FormationSpeedMetresPerSecond;
            public float RevealPathDistanceMetres;
            public float RawRevealDurationSeconds;
            public bool RevealCadenceLimited;
            public float HeadTrailMetres;
            public float ShoreInsetMetres;
            public float WidthMetres;
            public float ShoreRibbonThicknessCells;
            public float ShoreRibbonThicknessMetres;
            public float InwardReachMetres;
            public float FeatherMetres;
            public float SourceAmount;
            public float RemainingLife;
            public float PatternSeed;
            public float SourceFillSeed;
            public float SourceFillFeatureSize;
            public float SourceFillBlend;
            public float ShapeSeed;
            // Reserved legacy lanes retained in the fixed GPU event ABI.
            // P13B no longer evaluates generic automatic-source breakup.
            public float BreakupScaleMetres;
            public float BreakupStrength;
            public float Curvature;
            public float ObjectCentreAcrossMetres;
            public float ObjectAlongHalfLengthMetres;
            public float ObjectAcrossHalfWidthMetres;
            public float ObjectContactOffsetMetres;
            public float ObjectSourceLateralCellSpacingMetres;
            public float ObjectWakeArmLengthMetres;
            public float ObjectContactPathLengthMetres;
            public Vector2 ObjectContactPoint0;
            public Vector2 ObjectContactPoint1;
            public Vector2 ObjectContactPoint2;
            public Vector2 ObjectContactPoint3;
            public Vector2 ObjectContactPoint4;
            public float ObjectContactFrontSplit;
            public float ObjectContactNegativeFirstSegmentSplit;
            public float ObjectContactPositiveFirstSegmentSplit;
            public float CentreAcrossNormalized;
            public float LateralPaddingMetres;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamSourceEventGpuData
        {
            // x = source type, y = side sign except Object Arc/Semi-Arc
            // reserved Build code zero, z = reveal progress, w = shape seed.
            public Vector4 Header;
            // x/y = start/end storage global except Object Arc/Semi-Arc
            // contact point 0; z = centre storage global; w = flow direction
            // except Object Arc/Semi-Arc contact point 1.x.
            public Vector4 Distance;
            // x = shore inset except Object Arc/Semi-Arc contact point 1.y;
            // y = width metres except Shore Ribbon thickness cells and Object
            // Arc/Semi-Arc straight wake-arm length metres; z = inward reach or
            // Arc/Semi-Arc normalized material-step duration; w = feather metres
            // except Object Arc/Semi-Arc contact point 2.x.
            public Vector4 Shore;
            // x = authored intrinsic Presence, y = authored normalized
            // Remaining Life, z = pattern seed, w = pattern feature size.
            public Vector4 Material;
            // x = source fill seed except Object Arc/Semi-Arc negative-half
            // first-segment split; y/z = reserved zero lanes except Object
            // Arc/Semi-Arc contact point 2.y / point 3.x; w = curvature,
            // selected Semi-Arc side, or fragment rotation.
            public Vector4 Variation;
            // x/y = formation speed / moving-head trail except Object Arc/Semi-Arc
            // contact point 3.y / point 4.x; z = source path length metres;
            // w = reserved legacy source-fill blend except Object Arc/Semi-Arc
            // positive-half first-segment split. P13A no longer lets it reinterpret
            // Initial Presence as geometric probability.
            public Vector4 Kinematics;
            // x = object/free-water centre lateral metres; y/z = object half extents
            // except Object Arc/Semi-Arc contact point 4.y / front split;
            // w = Fleck contact offset, Free-Water shape parameter, or Object
            // Arc/Semi-Arc source-local lateral cell spacing metres.
            public Vector4 ObjectData;
            // x = previous deposition side/phase, y = previous deposition
            // progress, z = previous deposition state valid (0 for the first
            // source tick, 1 afterward), w reserved. Current phase/progress
            // remain Header.y/z. Positive newly revealed coverage gates
            // nonpersistent source families; Object Arc/Semi-Arc use their
            // current phase-shaped persistent emitter directly.
            public Vector4 Deposit;
        }

        private struct FoamCompositionEvent
        {
            public bool Active;
            public bool UsesMetricLateral;
            public int EventId;
            public float StartGlobalDistance;
            public float StartAcrossNormalized;
            public float StartLateralMetres;
            public float Duration;
            public float TravelDistance;
            public float FlowDirection;
            public float AcrossDrift;
            public float AcrossDriftMetres;
            public float PathWander;
            public float PathWanderMetres;
            public float BaseRadius;
            public float SourceAmount;
            public float RemainingLife;
            public float AmountEnvelopeFloor;
            public float RadiusEnvelopeFloor;
            public float PatternSeed;
            public float ShapeSeed;
            public float SourceFillSeed;
            public float SourceFillFeatureSize;
            public float BendSign;
            public float WidthPhase;
            public float StrokeAspect;
            public float WidthVariation;
            public float Elapsed;
            public float PreviousGlobalDistance;
            public float PreviousAcrossNormalized;
            public float PreviousLateralMetres;
            public float PreviousRadius;
            public float PreviousEmissionAmount;
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
