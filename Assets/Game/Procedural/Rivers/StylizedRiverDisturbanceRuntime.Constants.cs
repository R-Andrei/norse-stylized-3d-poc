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
        private const string ComputeResourcePath =
            "PS3DRiver/Compute/CS_RiverDisturbance";
        private const float ChunkLengthMetres = 32f;
        private const int ThreadGroupSize = 8;
        private const float StationarySpeedStart = 0.08f;
        private const float MovingSpeedFull = 0.45f;
        private const double SourceStaleSeconds = 0.35;
        private const float StaticOnlySimulationRate = 12f;
        public const float MaximumStaticPressureHeightMetres = 1.25f;
        private const float MaximumStaticPressureModulation = 1.16f;
        private const float RippleStabilitySafety = 0.42f;
        private const float RippleInjectionEnvelopeRadius = 1.15f;
        private const int MaximumRippleSubsteps = 32;
        private const double RippleSubstepDiagnosticWindowSeconds = 5.0;
        private const double PerformanceDiagnosticWindowSeconds = 5.0;
        private const float MinimumImpactReservationLifetime = 0.25f;
        private const float RippleReservationLookAheadSteps = 2f;
        private const float RippleReservationPaddingCells = 2f;
        private const float GoldenPhaseStep = 0.61803398875f;
        private const float AutomaticBoundsHorizontalPadding = 0.5f;
        private const float DefaultGeneratedFootprintPadding = 0.12f;
        private const float AutomaticBoundsVerticalPadding = 1.25f;
        private const int GeneratedSourcesPerFrame = 1;
        private const float StaticPressureProfileUpdateRate = 12f;
        private const float StaticPressureProfileTransitionFraction = 0.85f;
        private const float StaticWakeVariationUpdateRate = 12f;
        private const float StaticWakeVariationTransitionFraction = 0.85f;
        private const float StaticPressureMinimumProfileMultiplier = 0.58f;
        private const int MaximumStaticContourPoints =
            RiverDisturbanceFootprintResolver.MaximumContourPoints;

        private enum PerformanceDispatchCategory
        {
            RippleSimulation,
            WakeSimulation,
            ImpactInjection,
            WakeInjection,
            StaticPressureBake,
            StaticWakeBake,
            RippleBoundaryBake,
            Clear
        }

        private static uint sourcePhaseSequence = 1;

        private static readonly List<StylizedRiverDisturbanceRuntime>
            ActiveRuntimes = new();
        private static readonly Dictionary<EntityId, GeneratedRiverDisturbanceDiagnostics>
            GeneratedSourceDiagnostics = new();

        private static readonly int DisturbanceEnabledId =
            Shader.PropertyToID("_DisturbanceEnabled");
        private static readonly int DisturbancePreviousId =
            Shader.PropertyToID("_DisturbanceFieldPrevious");
        private static readonly int DisturbanceCurrentId =
            Shader.PropertyToID("_DisturbanceFieldCurrent");
        private static readonly int DisturbanceStaticTargetId =
            Shader.PropertyToID("_DisturbanceStaticTarget");
        private static readonly int DisturbanceStaticWakeSourceId =
            Shader.PropertyToID("_DisturbanceStaticWakeSource");
        private static readonly int DisturbanceRippleBoundaryId =
            Shader.PropertyToID("_DisturbanceRippleBoundary");
        private static readonly int DisturbanceStaticWakeTexelSizeId =
            Shader.PropertyToID("_DisturbanceStaticWakeTexelSize");
        private static readonly int DisturbanceWakePreviousId =
            Shader.PropertyToID("_DisturbanceWakePrevious");
        private static readonly int DisturbanceWakeCurrentId =
            Shader.PropertyToID("_DisturbanceWakeCurrent");
        private static readonly int DisturbanceWakeInterpolationId =
            Shader.PropertyToID("_DisturbanceWakeInterpolation");
        private static readonly int DisturbanceInterpolationId =
            Shader.PropertyToID("_DisturbanceInterpolation");
        private static readonly int DisturbanceGlobalStartId =
            Shader.PropertyToID("_DisturbanceGlobalStart");
        private static readonly int DisturbanceFieldLengthId =
            Shader.PropertyToID("_DisturbanceFieldLength");
        private static readonly int DisturbanceGeometryStrengthId =
            Shader.PropertyToID("_DisturbanceGeometryStrength");
        private static readonly int DisturbanceNormalStrengthId =
            Shader.PropertyToID("_DisturbanceNormalStrength");
        private static readonly int DisturbanceShoreInteractionId =
            Shader.PropertyToID("_DisturbanceShoreInteraction");
        private static readonly int DisturbanceMaximumHeightId =
            Shader.PropertyToID("_DisturbanceMaximumHeight");
        private static readonly int DisturbanceStaticMaximumHeightId =
            Shader.PropertyToID("_DisturbanceStaticMaximumHeight");
        private static readonly int DisturbanceWakeGeometryHeightId =
            Shader.PropertyToID("_DisturbanceWakeGeometryHeight");
        private static readonly int DisturbanceWakeGeometryCompactnessId =
            Shader.PropertyToID("_DisturbanceWakeGeometryCompactness");
        private static readonly int DisturbanceDebugViewId =
            Shader.PropertyToID("_DisturbanceDebugView");
        private static readonly int DisturbanceFragmentDetailId =
            Shader.PropertyToID("_DisturbanceFragmentDetail");
    }
}
