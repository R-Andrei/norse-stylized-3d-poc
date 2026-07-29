using System.Runtime.InteropServices;
using UnityEngine;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverFoamRuntime
    {
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
            public float NextPacketStartTime;
            public float NextReinforcementTime;
            public AutomaticFoamSourceEventType LastEventType;
            public AutomaticFoamSourceEventType LastContactEventType;
            public float LastContactSeed;
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

        private struct AutomaticFoamPacketReservation
        {
            public bool Active;
            public int EventId;
            public AutomaticFoamSourceEventType Type;
            public EntityId ObjectSourceId;
            public float MinimumGlobalDistance;
            public float MaximumGlobalDistance;
            public float MinimumLateralMetres;
            public float MaximumLateralMetres;
            public float ExpiresAtRealtime;
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
            public float ObjectContactStrokeDuration;
            public float ObjectContactStrokePathLengthMetres;
            public float ObjectContactStrokeRawRevealDurationSeconds;
            public bool ObjectContactStrokeRevealCadenceLimited;
            public int ObjectContactStrokeCount;
            public bool ObjectContactReinforcementOnly;
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
            // D8 cell-authoritative recipe-local geometry. These CPU fields are
            // union-packed into the existing eight-float4 GPU event record.
            public float BodyLengthCells;
            public float BodyWidthCells;
            public float HeadLengthCells;
            public float HeadWidthCells;
            public float BendAmplitudeCells;
            public float ContactSpanCells;
            public float ContactWidthCells;
            public float WakeLengthCells;
            public float WakeWidthCells;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FoamSourceEventGpuData
        {
            // x = source type, y = side sign except Object Arc/Semi-Arc
            // finite stroke phase (0 = complete packet, 1/2 = contact-only
            // reinforcement), z = per-stroke reveal progress, w = shape seed.
            public Vector4 Header;
            // x/y = start/end storage global except Object Arc/Semi-Arc
            // contact point 0; z = centre storage global; w = flow direction
            // except Object Arc/Semi-Arc contact point 1.x.
            public Vector4 Distance;
            // x = shore offset cells for D8 Shore/Inward except Object Arc/Semi-Arc contact point 1.y;
            // y = width cells for D8 Shore/Inward and Object
            // Arc/Semi-Arc straight wake-arm length metres; z = inward reach cells for D8 Inward Wash or
            // Arc/Semi-Arc normalized material-step duration; w = head width cells for D8 Shore/Inward
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
            // finite object burst uses phase changes to reset one-shot permission.
            public Vector4 Deposit;
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
