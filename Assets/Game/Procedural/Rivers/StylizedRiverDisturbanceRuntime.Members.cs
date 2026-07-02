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
        private readonly Dictionary<EntityId, ContinuousSource> continuousSources =
            new();
        private readonly Dictionary<EntityId, EntityId>
            continuousSourceIdsByOwner = new();
        private readonly HashSet<EntityId> ownershipConflictWarningOwnerIds =
            new();
        private readonly List<EntityId> staleSourceIds = new();
        private readonly List<EntityId> staticPressureProfileSourceIds = new();
        private readonly List<EntityId> staticWakeVariationSourceIds = new();
        private readonly List<ImpactCommand> pendingImpacts = new();
        private readonly List<ImpactReservation> activeImpactReservations =
            new();
        private readonly List<IGeneratedGeometrySource>
            generatedGeometryScratch = new();
        private readonly HashSet<EntityId> automaticGeneratedSourceIds =
            new();
        private readonly HashSet<EntityId>
            refreshedAutomaticGeneratedSourceIds = new();
        private readonly Vector4[] staticContourUpload =
            new Vector4[MaximumStaticContourPoints];
        private readonly Vector4[] staticPressureProfileUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];
        private readonly Vector4[] staticPressureGeometryUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];
        private readonly Vector4[] staticWakeVariationProfileUpload =
            new Vector4[
                RiverDisturbanceFootprintResolver.
                    MaximumPressureSupportLateralSamples];

        private StylizedRiver river;
        private MeshRenderer surfaceRenderer;
        private MaterialPropertyBlock propertyBlock;
        private ComputeShader computeShader;
        private RenderTexture stateA;
        private RenderTexture stateB;
        private RenderTexture staticTarget;
        private RenderTexture staticWakeSource;
        private RenderTexture rippleBoundary;
        private RenderTexture wakeA;
        private RenderTexture wakeB;
        private RenderTexture currentWake;
        private RenderTexture previousWake;
        private RenderTexture writeWake;
        private RenderTexture currentState;
        private RenderTexture previousState;
        private RenderTexture writeState;
        private ComputeBuffer rippleMetricBuffer;
        private float[] rippleMetricMinimumAlongCell = Array.Empty<float>();
        private float[] rippleMetricMinimumLateralCell = Array.Empty<float>();
        private float[] rippleChunkMaximumInverseLength = Array.Empty<float>();
        private float[] rippleChunkMinimumCellSize = Array.Empty<float>();
        private double[] chunkActiveUntil = Array.Empty<double>();
        private bool[] chunkActive = Array.Empty<bool>();
        private bool[] chunkHasStaticSource = Array.Empty<bool>();
        private double[] wakeChunkActiveUntil = Array.Empty<double>();
        private double[] staticWakeChunkReleaseDuration = Array.Empty<double>();
        private bool[] wakeChunkActive = Array.Empty<bool>();

        private int clearKernel = -1;
        private int injectRippleKernel = -1;
        private int injectWakeKernel = -1;
        private int bakeStaticPressureKernel = -1;
        private int finalizeStaticPressureKernel = -1;
        private int bakeStaticWakeSourceKernel = -1;
        private int bakeRippleBoundaryBaseKernel = -1;
        private int bakeRippleBoundaryObstacleKernel = -1;
        private int applyRippleBoundaryKernel = -1;
        private int simulateRippleKernel = -1;
        private int simulateWakeKernel = -1;
        private int fieldWidth;
        private int fieldHeight;
        private int chunkCount;
        private int resolutionPerChunk;
        private int wakeResolutionPerChunk;
        private int wakeFieldWidth;
        private int wakeFieldHeight;
        private int domainVersion = -1;
        private float fieldLength;
        private float validFieldLength;
        private int validFieldWidth;
        private int validWakeFieldWidth;
        private float averageSurfaceHalfWidth = 1f;
        private float simulationAccumulator;
        private float staticPressureProfileAccumulator;
        private float staticWakeVariationAccumulator;
        private float simulationInterpolation = 1f;
        private float wakeInterpolation = 1f;
        private double lastRuntimeTime;
        private double lastActivityTime;
        private bool supportWarningReported;
        private bool allocationWarningReported;
        private bool resourcesDirty = true;
        private bool staticPressureTargetDirty = true;
        private bool staticWakeSourceDirty = true;
        private bool rippleBoundaryDirty = true;
        private int validStaticSourceCount;
        private int validStaticWakeSourceCount;
        private int obstacleGeometryVersion;
        private int rippleCollisionSourceCount;
        private bool generatedGeometryRegistryDirty = true;
        private bool generatedGeometryRefreshInProgress;
        private int generatedGeometryRefreshIndex;
        private Bounds generatedGeometryRefreshBounds;
        private bool wasFrozen;
        private int impactsInjectedLastStep;
        private int currentRippleSubstepCount;
        private int maximumRecentRippleSubstepCount;
        private float activeRippleMinimumCellSize;
        private bool rippleSubstepLimitReached;
        private double rippleSubstepDiagnosticWindowStart;
        private int lastUpdateComputeDispatchCount;
        private int recentPeakComputeDispatchCount;
        private long lastUpdateThreadGroupCount;
        private long recentPeakThreadGroupCount;
        private long lastUpdateCellIterationCount;
        private long recentPeakCellIterationCount;
        private int lastUpdateRippleSimulationDispatchCount;
        private int lastUpdateWakeSimulationDispatchCount;
        private int lastUpdateImpactInjectionDispatchCount;
        private int lastUpdateWakeInjectionDispatchCount;
        private int lastUpdateStaticPressureBakeDispatchCount;
        private int lastUpdateStaticWakeBakeDispatchCount;
        private int lastUpdateRippleBoundaryBakeDispatchCount;
        private int lastUpdateClearDispatchCount;
        private int lastUpdateFieldRebuildCount;
        private int recentPeakFieldRebuildCount;
        private double performanceDiagnosticWindowStart;
    }
}
