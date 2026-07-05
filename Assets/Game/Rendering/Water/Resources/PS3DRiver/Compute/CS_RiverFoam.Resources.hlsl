// Canonical persistent and per-step source state (4.11C.4+, preserved by 4.11C.5):
// R = Presence
// G = Presence * normalized Remaining Life
// B = Presence * normalized Material Pattern
// A = reserved and always zero
// Premultiplying transported attributes by Presence preserves normalized Life
// and Pattern across interpolation with empty water. Consumers decode G/R and
// B/R only when Presence is non-zero.
RWTexture2D<float4> _FoamStateWrite;
Texture2D<float4> _FoamStateRead;
RWTexture2D<float4> _FoamBirthDebugWrite;
RWStructuredBuffer<uint> _FoamBirthDebugCounters;
Texture2D<float4> _FoamBoundary;
RWTexture2D<float> _FoamObstacleExclusionWrite;
Texture2D<float> _FoamObstacleExclusionRead;
Texture2D<float4> _FoamTopologyGeneratedRead;
Texture2D<float4> _FoamTopologyTransitionFromRead;
Texture2D<float> _FoamEvolvingMajorRead;
RWTexture2D<float> _FoamEvolvingMajorWrite;
Texture2D<float> _FoamEvolvingHostedNegativeRead;
RWTexture2D<float> _FoamEvolvingHostedNegativeWrite;
Texture2D<float> _FoamEvolvingFreeWaterNegativeRead;
RWTexture2D<float> _FoamEvolvingFreeWaterNegativeWrite;
Texture2D<float> _FoamEvolvingConnectorRead;
RWTexture2D<float> _FoamEvolvingConnectorWrite;
Texture2D<float> _FoamEvolvingWeakSpanNegativeRead;
RWTexture2D<float> _FoamEvolvingWeakSpanNegativeWrite;
Texture2DArray<float4> _FoamMajorMasks;
Texture2DArray<float4> _FoamHostedNegativeMasks;
Texture2DArray<float4> _FoamFreeWaterNegativeMasks;
RWTexture2D<float4> _FoamTopologyWrite;
RWTexture2D<float4> _FoamTopologySourcesWrite;
RWTexture2D<float4> _FoamTopologyTransitionCaptureWrite;
RWTexture2D<float2> _FoamCurrentShoreEdgesWrite;
Texture2D<float2> _FoamCurrentShoreEdgesRead;
Texture2D<float4> _FoamTopologyRead;
Texture2D<float4> _FoamTopologySourcesRead;
Texture2D<float4> _FoamRippleField;
Texture2D<float4> _FoamWakeField;
Texture2D<float4> _FoamStaticWakeField;
Texture2D<float4> _FoamStaticPressureField;
StructuredBuffer<FoamMetricRow> _FoamMetricRows;
StructuredBuffer<FoamMetricRow> _FoamTopologyTransitionMetricRows;
StructuredBuffer<FoamObstacleSample> _FoamObstacleSamples;
StructuredBuffer<FoamObstacleIntervalCell> _FoamObstacleCells;
StructuredBuffer<FoamMajorEvolutionData> _FoamMajorEvolutionRecords;
StructuredBuffer<FoamHostedNegativeEvolutionData>
    _FoamHostedNegativeEvolutionRecords;
StructuredBuffer<FoamFreeWaterEvolutionData>
    _FoamFreeWaterEvolutionRecords;
StructuredBuffer<FoamConnectorIdentityData>
    _FoamConnectorIdentityRecords;
StructuredBuffer<float4> _FoamConnectorPathPoints;
StructuredBuffer<FoamWeakSpanIdentityData>
    _FoamWeakSpanIdentityRecords;


// One compact global topology/material diagnostic record. Values are uint
// counters except integrated/core area, which store fixed-point square metres.
static const uint FoamMetricValidFluidOffset = 0u;
static const uint FoamMetricMajorSupportOffset = 4u;
static const uint FoamMetricConnectorSupportOffset = 8u;
static const uint FoamMetricNegativeAgingPressureOffset = 12u;
static const uint FoamMetricFoamInNegativeOffset = 16u;
static const uint FoamMetricVisibleMaterialOffset = 20u;
static const uint FoamMetricShoreSupportOffset = 24u;
static const uint FoamMetricFoamInShoreOffset = 28u;
static const uint FoamMetricIntegratedPresenceAreaOffset = 32u;
static const uint FoamMetricPresenceCoreAreaOffset = 36u;
static const uint FoamMetricPressureLeeSupportOffset = 40u;
static const uint FoamMetricFoamInPressureLeeOffset = 44u;
static const uint FoamMetricPerimeterVisibleOffset = 48u;
static const uint FoamMetricConnectorMajorOverlapOffset = 52u;
static const uint FoamMetricVisiblePresenceAreaOffset = 56u;
static const uint FoamMetricVisibleLifeAreaOffset = 60u;
static const uint FoamMetricVisiblePositiveSupportAreaOffset = 64u;
static const uint FoamMetricVisibleNegativeAgingAreaOffset = 68u;
static const uint FoamMetricVisibleLocalAgingRateAreaOffset = 72u;
static const uint FoamMetricFoamSupportNegativeOverlapOffset = 76u;
static const uint FoamMetricMaxPositiveSupportUnderFoamOffset = 80u;
static const uint FoamMetricMaxNegativeAgingUnderFoamOffset = 84u;
static const uint FoamMetricVisibleLifeMinFixedOffset = 88u;
static const uint FoamMetricVisibleLifeMaxFixedOffset = 92u;
static const uint FoamMetricVisibleLifeMinFixedIndex = 22u;
static const uint FoamMetricCount = 24u;
RWByteAddressBuffer _FoamTopologyMetrics;

int2 _FoamDimensions;
int2 _FoamTopologyDimensions;
int2 _FoamTopologyTransitionDimensions;
int2 _FoamMajorMaskDimensions;
int2 _FoamHostedNegativeMaskDimensions;
int2 _FoamFreeWaterMaskDimensions;
int2 _FoamRippleDimensions;
int2 _FoamWakeDimensions;
int2 _FoamStaticWakeDimensions;
int2 _FoamStaticPressureDimensions;
int _FoamRangeStart;
int _FoamRangeCount;
int _FoamObstacleCellCount;
int _FoamMajorEvolutionCount;
int _FoamHostedNegativeEvolutionCount;
int _FoamFreeWaterEvolutionCount;
int _FoamConnectorIdentityCount;
int _FoamWeakSpanIdentityCount;
int _FoamPhaseCommitCells;
float _FoamDeltaTime;
float _FoamDebugAbsoluteLifeProbeActive;
float _FoamPhaseTransportMetres;
float _FoamGlobalStart;
float _FoamFieldLength;
float _FoamValidLength;
float _FoamSimulationLength;
float _FoamFlowSpeed;
float _FoamFlowDirection;
float _FoamNeutralLifetime;
float _FoamPositiveAgeMultiplier;
float _FoamNegativeAgeMultiplier;
float _FoamTime;
float _FoamSeed;
float _FoamPresenceMetricThreshold;
float _FoamIntegratedAreaFixedPointScale;
float _FoamDisturbanceEnabled;
float _FoamSurfaceMorphStrength;
float _FoamChaoticDriftStrength;
float _FoamChaoticDriftRhythm;
float _FoamMotionFlowSpeed;
float _FoamMotionWaveHeight;
float _FoamMotionWaveLength;
float _FoamMotionWaveSteepness;
float _FoamMotionTurbulence;
float _FoamShoreMotion;
float _FoamShoreMotionWidth;
float _FoamShoreWaveHeightScale;
float _FoamShoreWaveLengthScale;
float _FoamShoreWaveReach;
float _FoamShoreWaveTransitionLength;
float _FoamShoreWaveSizeVariation;
float _FoamShoreWaveSideAsymmetry;
float _FoamShoreWaveProfileVariation;
float _FoamShoreBankCover;
float _FoamFreezeAmount;
float _FoamShoreCaptureCoreWidth;
float _FoamShoreCaptureFadeWidth;
float _FoamMajorEvolutionEnabled;
float _FoamHostedNegativeEvolutionEnabled;
float _FoamFreeWaterNegativeEvolutionEnabled;
float _FoamConnectorIdentityReconstructionEnabled;
float _FoamWeakSpanIdentityReconstructionEnabled;
float _FoamTopologyTransitionEnabled;
float _FoamTopologyTransitionBlend;
float _FoamTopologyTransitionSameMapping;
float _FoamTopologyTransitionGlobalStart;
float _FoamTopologyTransitionFieldLength;
float _FoamTopologyTransitionValidLength;

float _FoamInjectionGlobalDistance;
float _FoamInjectionAcrossNormalized;
float _FoamInjectionRadius;
float _FoamInjectionAmount;
float _FoamInjectionSourceFillSeed;
float _FoamInjectionSourceFillFeatureSize;
float _FoamInjectionRemainingLife;
float _FoamInjectionPatternSeed;
float _FoamInjectionElongation;
float _FoamInjectionShapeSeed;
float _FoamInjectionShapeVariety;
float _FoamInjectionCompound;
float _FoamInjectionSegment;
float _FoamInjectionSegmentStartGlobalDistance;
float _FoamInjectionSegmentStartAcrossNormalized;
float _FoamInjectionSegmentStartRadius;
float _FoamInjectionSegmentStartAmount;
float _FoamInjectionSegmentEndGlobalDistance;
float _FoamInjectionSegmentEndAcrossNormalized;
float _FoamInjectionSegmentEndRadius;
float _FoamInjectionSegmentEndAmount;
float _FoamBirthDebugPaintMode;
int4 _FoamLifeProbeRectA;
int4 _FoamLifeProbeRectB;
int4 _FoamLifeProbeRectC;
