// Canonical Patch 4.11A persistent material state:
// R = Amount
// G = Amount * normalized Remaining Life
// B = Amount * normalized Integrity
// A = material phase / provenance
// Premultiplying the transported attributes makes advection and merging
// conservative: empty cells cannot carry life or integrity independently of
// material amount. Consumers decode G/R and B/R only when Amount is non-zero.
RWTexture2D<float4> _FoamStateWrite;
Texture2D<float4> _FoamStateRead;
RWTexture2D<float4> _FoamAdvectionWrite;
Texture2D<float4> _FoamAdvectedRead;
Texture2D<float4> _FoamReverseRead;
Texture2D<float4> _FoamBoundary;
RWTexture2D<float> _FoamObstacleExclusionWrite;
Texture2D<float> _FoamObstacleExclusionRead;
RWTexture2D<float4> _FoamGuidanceWrite;
Texture2D<float4> _FoamGuidanceRead;
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
Texture2D<float4> _FoamWakeField;
Texture2D<float4> _FoamRippleField;
Texture2D<float4> _FoamStaticWakeField;
Texture2D<float4> _FoamStaticPressureField;
RWTexture2D<float2> _FoamFractureWrite;
Texture2D<float2> _FoamFractureRead;
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

// Per chunk, two uint4 records are stored in one raw buffer:
// record 0: x = quantised Amount sum, y = visible-cell count,
//           z = valid fluid-cell count, w = guidance-lane cell count.
// record 1: x = perimeter-cell count, y = broad-interior count,
//           z = shore-support visible count, w = visible guidance-lane count.
RWByteAddressBuffer _FoamPopulationMetrics;

// One global topology diagnostic record. Values are uint counters.
//  0 valid-fluid cells                    8 shore-support cells
//  1 major-support cells                  9 foam within shore support
//  2 connector-support cells             10 reserved
//  3 negative-aging-pressure cells       11 reserved
//  4 foam within negative aging pressure 12 Pressure/Lee-support cells
//  5 visible material cells              13 foam within Pressure/Lee support
//  6 reserved                            14 perimeter-visible cells
//  7 reserved                            15 Connector/Major overlap cells
RWByteAddressBuffer _FoamTopologyMetrics;

int2 _FoamDimensions;
int2 _FoamGuidanceDimensions;
int2 _FoamTopologyDimensions;
int2 _FoamTopologyTransitionDimensions;
int2 _FoamMajorMaskDimensions;
int2 _FoamHostedNegativeMaskDimensions;
int2 _FoamFreeWaterMaskDimensions;
int2 _FoamWakeDimensions;
int2 _FoamRippleDimensions;
int2 _FoamStaticWakeDimensions;
int2 _FoamStaticPressureDimensions;
int2 _FoamFractureDimensions;
int _FoamRangeStart;
int _FoamRangeCount;
int _FoamFractureRangeStart;
int _FoamFractureRangeCount;
int _FoamObstacleCellCount;
int _FoamMajorEvolutionCount;
int _FoamHostedNegativeEvolutionCount;
int _FoamFreeWaterEvolutionCount;
int _FoamConnectorIdentityCount;
int _FoamWeakSpanIdentityCount;
int _FoamResolutionPerChunk;
int _FoamChunkCount;
float _FoamDeltaTime;
float _FoamGlobalStart;
float _FoamFieldLength;
float _FoamValidLength;
float _FoamSimulationLength;
float _FoamFlowSpeed;
float _FoamFlowDirection;
float _FoamEvolution;
float _FoamBreakup;
float _FoamSpread;
float _FoamCohesion;
float _FoamConnectivity;
float _FoamNeutralLifetime;
float _FoamPositiveAgeMultiplier;
float _FoamNegativeAgeMultiplier;
float _FoamEndOfLifeDissipationRate;
float _FoamEndOfLifeDissipationStart;
float _FoamIntegrityDamage;
float _FoamShoreRetention;
float _FoamTime;
float _FoamSeed;
float _FoamTargetCoverage;
float _FoamSupplyRate;
float _FoamVisibleThreshold;
float _FoamGuidanceStrength;
float _FoamBoundaryAttraction;
float _FoamWakeReinforcement;
float _FoamImpactReinforcement;
float _FoamDisturbanceEnabled;
float _FoamFractureDeltaTime;
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
float _FoamInjectionRemainingLife;
float _FoamInjectionIntegrity;
float _FoamInjectionPhase;
float _FoamInjectionElongation;
float _FoamInjectionShapeSeed;
float _FoamInjectionShapeVariety;
float _FoamInjectionCompound;
