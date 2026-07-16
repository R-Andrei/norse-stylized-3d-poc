using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ProgrammaticStylized3D.Geometry;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Rivers
{
    public sealed partial class StylizedRiverDisturbanceRuntime
    {
        private struct StaticWakeLeeVariationState
        {
            public int SampleCount;
            public float[] CurrentDepthMultipliers;
            public float[] TransitionStartDepthMultipliers;
            public float[] TargetDepthMultipliers;
            public float[] CurrentLengthMultipliers;
            public float[] TransitionStartLengthMultipliers;
            public float[] TargetLengthMultipliers;
            public float[] CurrentTrailingEdgeOffsets;
            public float[] TransitionStartTrailingEdgeOffsets;
            public float[] TargetTrailingEdgeOffsets;
            public float[] RawScratch;
            public float[] SmoothedScratch;
            public float Transition;
            public float TransitionDuration;
            public float SelectedInterval;
            public uint EventIndex;
            public double NextEventTime;
            public bool ScheduleInitialized;
            public int ProfileFamily;
        }

        private struct StaticWakeReleaseVariationState
        {
            public float CurrentLateralOffset;
            public float TransitionStartLateralOffset;
            public float TargetLateralOffset;
            public float CurrentEnergyMultiplier;
            public float TransitionStartEnergyMultiplier;
            public float TargetEnergyMultiplier;
            public float CurrentWidthMultiplier;
            public float TransitionStartWidthMultiplier;
            public float TargetWidthMultiplier;
            public float CurrentDownstreamOffset;
            public float TransitionStartDownstreamOffset;
            public float TargetDownstreamOffset;
            public float Transition;
            public float TransitionDuration;
            public float SelectedInterval;
            public uint EventIndex;
            public double NextEventTime;
            public bool ScheduleInitialized;
        }

        private readonly struct StaticWakeBakeVariationParameters
        {
            public StaticWakeBakeVariationParameters(
                StaticWakeLeeVariationState lee,
                StaticWakeReleaseVariationState left,
                StaticWakeReleaseVariationState right)
            {
                Lee = lee;
                Left = left;
                Right = right;
            }

            public StaticWakeLeeVariationState Lee { get; }
            public StaticWakeReleaseVariationState Left { get; }
            public StaticWakeReleaseVariationState Right { get; }
        }

        private struct ContinuousSource
        {
            public Vector3 WorldPosition;
            public float StartDistance;
            public float EndDistance;
            public float StartAcrossNormalized;
            public float EndAcrossNormalized;
            public float AcrossHalfWidth;
            public float AlongHalfLength;
            public float Strength;
            public float GeometryContribution;
            public float NormalContribution;
            public float StaticTargetHeightMetres;
            public float StaticPressureAcrossHalfWidth;
            public float StaticPressureAlongHalfLength;
            public Vector2[] StaticPressureContour;
            public RiverFoamStaticContactProfile FoamContactProfile;
            public RiverDisturbancePressureBakeProfile StaticPressureProfile;
            public RiverDisturbancePressureBakeProfile StaticPressureBaseProfile;
            // Exact generated mesh retained for staged pre-gameplay obstacle
            // preparation. Foam consumes cached exact solid intervals; Static
            // Pressure still has its older independent contour path and is
            // explicitly marked for a future shared-data refactor.
            public MeshFilter ObstacleExclusionMeshFilter;
            public IGeneratedGeometryStableFingerprintSource
                ObstacleExclusionFingerprintSource;
            public float[] StaticPressureCurrentMultipliers;
            public float[] StaticPressureTransitionStartMultipliers;
            public float[] StaticPressureTargetMultipliers;
            public float[] StaticPressureRawScratch;
            public float[] StaticPressureSmoothedScratch;
            public float StaticPressureProfileTransition;
            public float StaticPressureProfileTransitionDuration;
            public float StaticPressureProfileChangeIntervalMin;
            public float StaticPressureProfileChangeIntervalMax;
            public uint StaticPressureProfileEventIndex;
            public double StaticPressureNextProfileEventTime;
            public bool StaticPressureProfileScheduleInitialized;
            public float StaticWakeAmplitude;
            public float StaticContactSharpness;
            public float StaticWakeReachMultiplier;
            public float StaticWakeSpreadMultiplier;
            public float StaticWakeVariation;
            public StaticWakeLeeVariationState StaticWakeLeeVariation;
            public StaticWakeReleaseVariationState
                StaticWakeLeftReleaseVariation;
            public StaticWakeReleaseVariationState
                StaticWakeRightReleaseVariation;
            public float StaticWakeVariationIntervalMin;
            public float StaticWakeVariationIntervalMax;
            public float StaticProfileVariation;
            public Vector2[] StaticContour;
            public bool RippleCollisionEnabled;
            public float RippleCollisionAcrossHalfWidth;
            public float RippleCollisionAlongHalfLength;
            public Vector2[] RippleCollisionContour;
            public float MovementSpeed;
            public float Phase;
            public EntityId OwnerId;
            public bool IsStatic;
            public bool StationaryObstruction;
            public double LastSeen;
        }

        private struct ImpactCommand
        {
            public float Distance;
            public float AcrossNormalized;
            public Vector2 WorldPositionXZ;
            public float Radius;
            public float SignedImpulse;
            public float InitialElevation;
            public float Shape;
            public float Sharpness;
            public float GeometryContribution;
            public float NormalContribution;
        }

        private struct ImpactReservation
        {
            public double EndTime;
            public float AgeSeconds;
            public float MinimumLifetime;
            public float MaximumLifetime;
            public float CurrentDistance;
            public float CurrentRadius;
            public float CurrentMagnitude;
            public float MinimumReservedDistance;
            public float MaximumReservedDistance;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RippleMetricRowData
        {
            // Must match RippleMetricRowData in CS_RiverDisturbance.compute.
            public Vector4 CentreAndTangent;
            public Vector4 SideAndWidths;
        }
    }
}
