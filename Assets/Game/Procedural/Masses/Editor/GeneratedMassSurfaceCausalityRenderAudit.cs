using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ProgrammaticStylized3D.Geometry.Masses.Editor
{
    /// <summary>
    /// Audit-owned rendering tournament. It never writes scene or material
    /// assets: every case uses a hidden temporary renderer, camera, and cloned
    /// material; all light/renderer overrides are restored before waiting for
    /// asynchronous GPU readback.
    /// </summary>
    // GM-SURFACE.5P ACTIVE DEFECT CONTRACT:
    // This audit exists first to explain wrong per-surface directional response:
    // source faces and bevels can be brighter/darker in an order that contradicts
    // their orientation to the same light. Global darkness, average residual, F0,
    // and specular magnitude are secondary evidence only. No ownership label can
    // close the defect unless surface-orientation and parent-bevel-parent ordering
    // are coherent against the legacy reference.
    internal sealed class GeneratedMassSurfaceCausalityRenderAudit : IDisposable
    {
        internal const int CaptureSize = 384;
        private const float HighIntensityScale = 8f;
        private const float Quantization = 100000f;
        private const int MinimumValidFacetSamples = 16;
        private const string LegacyStoneMaterialPath =
            "Assets/Game/Demo/Materials/Stone/M_PixelStone.mat";
        private const float ClassMismatchThreshold = 0.10f;
        private const float OrderingTolerance = 0.08f;
        private const string TriangleIdentityShaderPath =
            "Assets/Game/Procedural/Masses/Editor/GeneratedMassTriangleIdentityAudit.shader";
        private const string TriangleIdentityPassName =
            "TriangleIdentityAudit";
        private const int TriangleIdentityRadix = 255;
        private const int TriangleIdentityMaximum =
            TriangleIdentityRadix * TriangleIdentityRadix *
            TriangleIdentityRadix - 1;
        private const int TriangleIdentityBackground = -1;
        private const int TriangleIdentityInvalid = -2;
        private const int MinimumTrianglePixels = 4;
        private const int MinimumIdentityPixels = 1024;
        private const int MinimumIdentityDistinctTriangles = 8;
        private const int MinimumIdentityExtentPixels = 16;
        private const float MinimumTriangleCoverageRatio = 0.90f;
        private const int MinimumIlluminatedTrianglesPerClass = 8;
        private const int MinimumEvaluableStageADirections = 12;
        private const int MinimumBrdfTrianglesPerClass = 8;
        private const int StageBDirectionCount = 4;
        private const int StageCDirectionCount = 2;
        private const float AlternateViewAzimuthDegrees = 35f;
        private const float SignedResidualTolerance = 0.05f;
        private const float DielectricF0 = 0.04f;
        private const float CurrentHlslF0 = 0.16f;
        private const float MinimumForegroundAlignmentIoU = 0.995f;
        private const float MaximumForegroundPixelCountDifferenceRatio = 0.01f;
        private const int MinimumLambertValidNormalPixels = 20000;
        private const int MinimumLambertPositiveExpectedPixels = 2000;
        private const int MinimumLambertPositiveObservedPixels = 2000;
        private const float MinimumLambertMeanForegroundLuma = 0.02f;
        private const float MaximumLambertConfiguredNormalizedRmse = 0.01f;
        private const float LambertNormalMinimumLength = 0.5f;
        private const float LambertPositiveResponseThreshold = 0.001f;
        private const float OrientationMinimumNdotLSeparation = 0.08f;
        private const float OrientationIntermediateNdotLTolerance = 0.025f;
        private const float OrientationOrderingLumaTolerance = 0.0005f;
        private const float OrientationCorrelationEpsilon = 0.000001f;

        private static readonly OrientationStageDefinition[] OrientationStages =
        {
            new OrientationStageDefinition("BASE", 20, 40),
            new OrientationStageDefinition("TONAL", 21, 41),
            new OrientationStageDefinition("EXPOSURE_SCALE", 22, 42),
            new OrientationStageDefinition("MOTTLE", 23, 43),
            new OrientationStageDefinition("EXPOSURE_TINT", 25, 44),
            new OrientationStageDefinition("CREVICE", 26, 45),
            new OrientationStageDefinition("BASE_LAYER", 27, 46),
            new OrientationStageDefinition("DIRT", 28, 47),
            new OrientationStageDefinition("WET_DAMP", 24, 48),
            new OrientationStageDefinition("FROST", 29, 49),
            new OrientationStageDefinition("WET_GLOBAL", 30, 50),
            new OrientationStageDefinition("FINAL_PRELIGHT", 31, 51),
            new OrientationStageDefinition("FINAL_WITH_OVERALL_TINT", 1, 54)
        };

        private static readonly string[] OrientationAblations =
        {
            "TONAL",
            "EXPOSURE",
            "MOTTLE",
            "CREVICE",
            "BASE",
            "DIRT",
            "WET",
            "FROST",
            "MONOLITHIC",
            "OVERALL_TINT",
            "SPECULAR_ZERO",
            "ALL_PRELIGHT_VALUE"
        };

        private static readonly BrdfDirectionDefinition[] BaseBrdfDirections =
        {
            new BrdfDirectionDefinition("POS_X", new Vector3(1f, 0f, 0f)),
            new BrdfDirectionDefinition("NEG_X", new Vector3(-1f, 0f, 0f)),
            new BrdfDirectionDefinition("POS_Y", new Vector3(0f, 1f, 0f)),
            new BrdfDirectionDefinition("NEG_Y", new Vector3(0f, -1f, 0f)),
            new BrdfDirectionDefinition("POS_Z", new Vector3(0f, 0f, 1f)),
            new BrdfDirectionDefinition("NEG_Z", new Vector3(0f, 0f, -1f)),
            new BrdfDirectionDefinition("XY_PP", new Vector3(1f, 1f, 0f)),
            new BrdfDirectionDefinition("XY_PN", new Vector3(1f, -1f, 0f)),
            new BrdfDirectionDefinition("XY_NP", new Vector3(-1f, 1f, 0f)),
            new BrdfDirectionDefinition("XY_NN", new Vector3(-1f, -1f, 0f)),
            new BrdfDirectionDefinition("XZ_PP", new Vector3(1f, 0f, 1f)),
            new BrdfDirectionDefinition("XZ_PN", new Vector3(1f, 0f, -1f)),
            new BrdfDirectionDefinition("XZ_NP", new Vector3(-1f, 0f, 1f)),
            new BrdfDirectionDefinition("XZ_NN", new Vector3(-1f, 0f, -1f)),
            new BrdfDirectionDefinition("YZ_PP", new Vector3(0f, 1f, 1f)),
            new BrdfDirectionDefinition("YZ_PN", new Vector3(0f, 1f, -1f)),
            new BrdfDirectionDefinition("YZ_NP", new Vector3(0f, -1f, 1f)),
            new BrdfDirectionDefinition("YZ_NN", new Vector3(0f, -1f, -1f)),
            new BrdfDirectionDefinition("XYZ_PPP", new Vector3(1f, 1f, 1f)),
            new BrdfDirectionDefinition("XYZ_PPN", new Vector3(1f, 1f, -1f)),
            new BrdfDirectionDefinition("XYZ_PNP", new Vector3(1f, -1f, 1f)),
            new BrdfDirectionDefinition("XYZ_PNN", new Vector3(1f, -1f, -1f)),
            new BrdfDirectionDefinition("XYZ_NPP", new Vector3(-1f, 1f, 1f)),
            new BrdfDirectionDefinition("XYZ_NPN", new Vector3(-1f, 1f, -1f)),
            new BrdfDirectionDefinition("XYZ_NNP", new Vector3(-1f, -1f, 1f)),
            new BrdfDirectionDefinition("XYZ_NNN", new Vector3(-1f, -1f, -1f))
        };

        private static readonly ViewDefinition[] AlternateViews =
        {
            new ViewDefinition("AZIMUTH_NEG_35", -AlternateViewAzimuthDegrees),
            new ViewDefinition("AZIMUTH_POS_35", AlternateViewAzimuthDegrees)
        };

        private readonly BrdfDirectionDefinition[] brdfDirections;


        internal enum SurfaceClass
        {
            WholeObject,
            SourceFace,
            OrdinaryBevel,
            JunctionOrEndpointCap,
            CornerDamage,
            Unclassified
        }

        private static readonly SurfaceClass[] ReportedSurfaceClasses =
        {
            SurfaceClass.WholeObject,
            SurfaceClass.SourceFace,
            SurfaceClass.OrdinaryBevel,
            SurfaceClass.JunctionOrEndpointCap,
            SurfaceClass.CornerDamage,
            SurfaceClass.Unclassified
        };

        private static readonly MethodInfo markSceneCleanMethod =
            ResolveSceneCleanMethod();

        internal sealed class Subject
        {
            internal string Role = string.Empty;
            internal GeneratedMass Target;
            internal Mesh Mesh;
            internal Material Material;
            internal MassGenerator.BevelShadingDiagnosticBuildRecord Build;
            internal readonly List<InternalEdge> InternalEdges = new();
            internal readonly Dictionary<SurfaceClass, List<int>>
                SurfaceClassIndices = new();
            internal readonly List<BevelParentGeometrySample>
                BevelParentSamples = new();
            internal readonly Dictionary<SurfaceClass, float>
                ExpectedMainDiffuse = new();
            internal SurfaceClass[] TriangleClasses =
                Array.Empty<SurfaceClass>();
            internal MassGenerator.FinalTriangleRecord[] TriangleRecords =
                Array.Empty<MassGenerator.FinalTriangleRecord>();
            internal int ClassifiedTriangleCount;
            internal int UnclassifiedTriangleCount;
            internal bool SurfaceClassContractValid;
            internal Matrix4x4 CloneLocalToWorld;
            internal Bounds LocalBounds;
        }

        internal sealed class SurfaceClassStatistics
        {
            internal SurfaceClass Class;
            internal int PixelCount;
            internal float MeanLuma;
            internal float P10Luma;
            internal float MedianLuma;
            internal float P90Luma;
            internal float RelativeToWhole;
        }

        internal sealed class BevelParentRenderSample
        {
            internal int LogicalBevelId;
            internal int SampleIndex;
            internal float ParentALuma;
            internal float BevelLuma;
            internal float ParentBLuma;
            internal float NormalizedTransition;
            internal float OutsideEnvelopeMagnitude;
            internal string Ordering = "Unavailable";
        }

        internal readonly struct BevelParentGeometrySample
        {
            internal readonly int LogicalBevelId;
            internal readonly int SampleIndex;
            internal readonly int ParentATriangleIndex;
            internal readonly int BevelTriangleIndex;
            internal readonly int ParentBTriangleIndex;
            internal readonly Vector3 ParentA;
            internal readonly Vector3 Bevel;
            internal readonly Vector3 ParentB;

            internal BevelParentGeometrySample(
                int logicalBevelId,
                int sampleIndex,
                int parentATriangleIndex,
                int bevelTriangleIndex,
                int parentBTriangleIndex,
                Vector3 parentA,
                Vector3 bevel,
                Vector3 parentB)
            {
                LogicalBevelId = logicalBevelId;
                SampleIndex = sampleIndex;
                ParentATriangleIndex = parentATriangleIndex;
                BevelTriangleIndex = bevelTriangleIndex;
                ParentBTriangleIndex = parentBTriangleIndex;
                ParentA = parentA;
                Bevel = bevel;
                ParentB = parentB;
            }
        }

        internal sealed class TriangleLuminanceStatistics
        {
            internal int TriangleIndex;
            internal SurfaceClass SurfaceClass;
            internal int LogicalBevelId;
            internal int ParentFaceA = -1;
            internal int ParentFaceB = -1;
            internal int ProvenanceKind;
            internal int ProvenanceIndex = -1;
            internal int SurfaceGroup = -1;
            internal Vector3 GeometricNormalLocal;
            internal Vector3 AuthoredNormalLocal;
            internal Vector4 MaskA;
            internal Vector4 MaskB;
            internal Vector4 MaskC;
            internal Vector4 StructuralA;
            internal Vector4 StructuralB;
            internal Vector4 StructuralC;
            internal string TriangleCondition = string.Empty;
            internal double TriangleAspectRatio;
            internal double TriangleMinimumAngleDegrees;
            internal int PixelCount;
            internal Vector3 MeanLinearRgb;
            internal float MeanLuma;
            internal float MinLuma;
            internal float P10Luma;
            internal float MedianLuma;
            internal float P90Luma;
            internal float MaxLuma;
            internal float StandardDeviationLuma;
            internal Vector3 StoredNormalLocal;
            internal float PredictedNdotL;
            internal float PredictedNdotV;
            internal float PredictedNdotH;
        }

        internal sealed class TriangleResponseComparison
        {
            internal int TriangleIndex;
            internal SurfaceClass SurfaceClass;
            internal int LogicalBevelId;
            internal int PixelCount;
            internal Vector3 LegacyLinearRgb;
            internal Vector3 CurrentHlslLinearRgb;
            internal Vector3 DielectricHlslLinearRgb;
            internal float LegacyLuma;
            internal float CurrentHlslLuma;
            internal float DielectricHlslLuma;
            internal float SignedResidualCurrent;
            internal float SignedResidualDielectric;
            internal float RgbResidualCurrent;
            internal float RgbResidualDielectric;
        }

        internal sealed class BrdfDirectionSummary
        {
            internal string DirectionName = string.Empty;
            internal Vector3 LightDirectionLocal;
            internal int ComparedTriangles;
            internal int ComparedSourceTriangles;
            internal int ComparedBevelTriangles;
            internal int CurrentOverResponseCount;
            internal int CurrentUnderResponseCount;
            internal int DielectricOverResponseCount;
            internal int DielectricUnderResponseCount;
            internal int CurrentOrderingInversionCount;
            internal int DielectricOrderingInversionCount;
            internal float CurrentMeanAbsoluteResidual;
            internal float CurrentP90AbsoluteResidual;
            internal float DielectricMeanAbsoluteResidual;
            internal float DielectricP90AbsoluteResidual;
            internal float DielectricResidualReduction;
            internal bool AdaptiveStageAvailable;
            internal float ActualCurrentMeanAbsoluteResidual;
            internal float ActualDielectricMeanAbsoluteResidual;
            internal float ActualDielectricResidualReduction;
            internal float DiffuseEnergyMatchedMeanAbsoluteResidual;
            internal bool IsEvaluable;
            internal readonly List<TriangleResponseComparison>
                TriangleComparisons = new();
        }

        internal sealed class CaseResult
        {
            internal string Name = string.Empty;
            internal string MeshRole = string.Empty;
            internal string MaterialRole = string.Empty;
            internal string Family = string.Empty;
            internal string PropertyBlockMode = "Preserved";
            internal SurfaceClass MaskClass = SurfaceClass.WholeObject;
            internal bool HighIntensity;
            internal int CausalityMode;
            internal int MaskDebugMode;
            internal bool IsAblation;
            internal bool IsTriangleIdentity;
            internal bool IsBrdfSweep;
            internal bool IsAdaptiveBrdf;
            internal string DirectionName = string.Empty;
            internal string BrdfVariant = string.Empty;
            internal string ViewName = "CURRENT";
            internal float CameraAzimuthDegrees;
            internal Vector3 LightDirectionLocal;
            internal Vector3 CameraPositionWorld;
            internal bool IsAuxiliaryIdentity;
            internal bool CountsTowardDecisionTotal = true;
            internal bool IsLambertPreflight;
            internal bool IsLambertNormalCapture;
            internal bool IsOrientationSweep;
            internal string OrientationKind = string.Empty;
            internal string OrientationStage = string.Empty;
            internal string OrientationAblation = string.Empty;
            internal int OrientationDirectProductPixelCount;
            internal float OrientationDirectProductMeanAbsoluteResidual;
            internal float OrientationDirectProductNormalizedRmse;
            internal bool TriangleIdentityContractValid;
            internal bool IdentityFlipRelativeToLighting;
            internal float ForegroundAlignmentIoU;
            internal float ForegroundPixelCountDifferenceRatio;
            internal int LightingForegroundPixelCount;
            internal int IdentityForegroundPixelCount;
            internal bool LambertContractValid;
            internal int LambertValidNormalPixelCount;
            internal int LambertPositiveExpectedPixelCount;
            internal int LambertPositiveObservedPixelCount;
            internal float LambertConfiguredNormalizedRmse;
            internal float LambertOppositeNormalizedRmse;
            internal float LambertBestFitScale;
            internal float LambertBestFitNormalizedRmse;
            internal float LambertMeanForegroundLuma;
            internal int TriangleIdentityPixelCount;
            internal int TriangleIdentityInvalidPixelCount;
            internal int TriangleIdentityDistinctTriangleCount;
            internal int TriangleIdentityForegroundWidth;
            internal int TriangleIdentityForegroundHeight;
            internal int TriangleIdentityCpuRoundTripFailures;
            internal float TriangleCoverageRatio;
            internal int NonFinitePixelCount;
            internal bool ReadbackError;
            internal string Error = string.Empty;
            internal int ValidFacetSamples;
            internal int TotalInternalEdges;
            internal int FrontFacingEdges;
            internal int ProjectedEdges;
            internal bool UsedFlippedReadback;
            internal float MeanGradientJump;
            internal float P90GradientJump;
            internal float MaximumGradientJump;
            internal float MeanValueStep;
            internal float MeanRawGradientJump;
            internal float P90RawGradientJump;
            internal float MeanColorGradientJump;
            internal float P90ColorGradientJump;
            internal float FacetScore;
            internal float MeanMaskedLuma;
            internal float SaturatedMaskedPixelFraction;
            internal float ReductionFromBaseline;
            internal bool ComparableForAblationRanking;
            internal string AblationExclusionReason = string.Empty;
            internal Matrix4x4 LocalToClip;
            internal readonly Dictionary<SurfaceClass, SurfaceClassStatistics>
                ClassStatistics = new();
            internal readonly List<BevelParentRenderSample>
                BevelParentSamples = new();
            internal readonly Dictionary<int, TriangleLuminanceStatistics>
                TriangleStatistics = new();
            internal int ValidBevelParentSamples;
            internal int BevelOutsideParentEnvelopeCount;
            internal float MeanBevelOutsideParentEnvelopeMagnitude;
            internal float MaximumBevelOutsideParentEnvelopeMagnitude;
            internal Color32[] Pixels = Array.Empty<Color32>();
            internal Color[] LinearPixels = Array.Empty<Color>();
        }

        internal sealed class Summary
        {
            internal string Ownership = "INCONCLUSIVE";
            internal float OwnershipConfidence;
            internal float MaterialEffect;
            internal float MeshEffect;
            internal float InteractionEffect;
            internal float HighIntensitySuppression;
            internal float HighIntensityNoPostSuppression;
            internal float SuspectMeshMaterialAssetEffect;
            internal float SuspectMeshPropertyBlockEffect;
            internal float ReferenceMeshMaterialAssetEffect;
            internal float ReferenceMeshPropertyBlockEffect;
            internal float SuspectBaselineScore;
            internal float ReferenceBaselineScore;
            internal string DominantContributor = "none";
            internal float DominantContributorReduction;
            internal bool LegacyControlAvailable;
            internal string LegacyMaterialName = "<missing>";
            internal string LegacyShaderName = "<missing>";
            internal float CurrentSourceRelativeResponse;
            internal float CurrentBevelRelativeResponse;
            internal float LegacySourceRelativeResponse;
            internal float LegacyBevelRelativeResponse;
            internal float SourceRelativeDeltaFromLegacy;
            internal float BevelRelativeDeltaFromLegacy;
            internal int OrderingMismatchAgainstLegacyCount;
            internal int ComparedBevelParentSamples;
            internal float MeanTransitionDeviationAgainstLegacy;
            internal string FirstDivergentStage = "none";
            internal string SurfaceLightingOwnership = "INCONCLUSIVE";
            internal float PropertyBlockMismatchReduction;
            internal float MatchedMainDirectClassMismatch;
            internal float MatchedIndirectClassMismatch;
            internal float CurrentSourceFinalToPrelightResponse;
            internal float CurrentBevelFinalToPrelightResponse;
            internal float CurrentBevelMinusSourceLightingResponse;
            internal float CurrentSourceMainDirectResponse;
            internal float CurrentBevelMainDirectResponse;
            internal float PredictedSourceMainDiffuse;
            internal float PredictedBevelMainDiffuse;
            internal float ExpectedBevelToSourceMainDiffuseRatio;
            internal float ObservedBevelToSourceMainDirectRatio;
            internal float MainDirectNormalPredictionResidual;
            internal float GeneratedNormalClassMismatchReduction;
            internal float AdditionalLightsClassMismatchReduction;
            internal float SpecularClassMismatchReduction;
            internal bool BrdfSweepAvailable;
            internal int BrdfComparedDirections;
            internal string BrdfWorstDirection = "none";
            internal float BrdfCurrentMeanAbsoluteResidual;
            internal float BrdfDielectricMeanAbsoluteResidual;
            internal float BrdfDielectricResidualReduction;
            internal int BrdfCurrentOverResponseCount;
            internal int BrdfCurrentUnderResponseCount;
            internal int BrdfDielectricOverResponseCount;
            internal int BrdfDielectricUnderResponseCount;
            internal int BrdfCurrentOrderingInversionCount;
            internal int BrdfDielectricOrderingInversionCount;
            internal int BrdfDielectricImprovedDirectionCount;
            internal int BrdfAdaptiveDirectionCount;
            internal float BrdfActualCurrentMeanAbsoluteResidual;
            internal float BrdfActualDielectricMeanAbsoluteResidual;
            internal float BrdfActualDielectricResidualReduction;
            internal float BrdfDiffuseEnergyMatchedMeanAbsoluteResidual;
            internal string BrdfWorkflowVerdict = "INCONCLUSIVE_BRDF_SWEEP";
            internal int ExpectedDecisionCases;
            internal int CompletedDecisionCases;
            internal int AuxiliaryIdentityCases;
            internal int AuxiliaryValidationCases;
            internal int ReadbackErrorCount;
            internal float MinimumCaseCoverageRatio;
            internal float NeutralDiffuseMeanAbsoluteResidual;
            internal int StageAEvaluableDirectionCount;
            internal float StageAF0ResidualReduction;
            internal int StageAF0ImprovedDirectionCount;
            internal float StageBStoredF0MinimumReduction;
            internal float StageBGeneratedNormalMeanReduction;
            internal float StageBActualStoredDielectricMeanAbsoluteResidual;
            internal bool LambertContractValid;
            internal int LambertValidNormalPixelCount;
            internal int LambertPositiveExpectedPixelCount;
            internal int LambertPositiveObservedPixelCount;
            internal float LambertConfiguredNormalizedRmse;
            internal float LambertOppositeNormalizedRmse;
            internal float LambertBestFitScale;
            internal float LambertBestFitNormalizedRmse;
            internal float LambertMeanForegroundLuma;
            internal float MinimumForegroundAlignmentIoU;
            internal float MaximumForegroundPixelCountDifferenceRatio;
            internal float StageCMinimumF0Reduction;
            internal float StageCGeneratedNormalMeanReduction;
            internal float IndirectCurrentMeanAbsoluteResidual;
            internal float IndirectDielectricMeanAbsoluteResidual;
            internal float ActualSceneCurrentMeanAbsoluteResidual;
            internal float ActualSceneDielectricMeanAbsoluteResidual;
            internal bool OrientationCaptureAvailable;
            internal int ExpectedOrientationCases;
            internal int CompletedOrientationCases;
            internal string OrientationFirstDivergentStage = "none";
            internal int OrientationFirstDivergentStageCount;
            internal string OrientationDominantAblation = "none";
            internal float OrientationDominantAblationReduction;
            internal int OrientationLegacySourcePairInversions;
            internal int OrientationHlslSourcePairInversions;
            internal int OrientationLegacyConditionalBevelViolations;
            internal int OrientationHlslConditionalBevelViolations;
            internal readonly List<OrientationStageSummary> OrientationStages = new();
            internal readonly List<OrientationAblationSummary> OrientationAblations = new();
            internal string CompletenessFailure = string.Empty;
            internal readonly List<BrdfDirectionSummary> BrdfDirections = new();
            internal readonly List<CaseResult> RankedContributors = new();
        }

        private readonly Subject suspect;
        private readonly Subject reference;
        private readonly List<RenderCase> cases = new();
        private readonly List<CaseResult> results = new();
        private readonly Dictionary<string, Color32[]> masks = new();
        private readonly Dictionary<string, Color32[]> triangleIdentityPixels =
            new();
        private sealed class OrientationPixelCapture
        {
            internal Color[] Pixels = Array.Empty<Color>();
            internal bool IdentityFlipRelativeToLighting;
            internal int LightingForegroundPixels;
        }

        private readonly Dictionary<string, OrientationPixelCapture> orientationAlbedoPixels =
            new(StringComparer.Ordinal);
        private readonly Dictionary<string, OrientationPixelCapture> orientationNdotLPixels =
            new(StringComparer.Ordinal);
        private Color[] lambertStoredNormalPixels = Array.Empty<Color>();
        private bool lambertStoredNormalIdentityFlipRelativeToLighting;
        private readonly Material legacyMaterial;
        private readonly string legacyMaterialLoadStatus;
        private readonly Shader triangleIdentityShader;
        private readonly string triangleIdentityShaderLoadStatus;
        private readonly int triangleIdentityShaderCompilerErrors;
        private readonly int triangleIdentityShaderPassIndex;
        private readonly bool floatingPointCaptureSupported;
        private readonly string floatingPointCaptureStatus;
        private readonly Quaternion cameraRotation;
        private readonly Vector3 cameraCenter;
        private readonly float cameraRadius;
        private readonly Camera sourceCamera;
        private readonly int auditLayer;
        private readonly bool requiresRendererSuppression;
        private int nextCase;
        private RenderTexture pendingTexture;
        private AsyncGPUReadbackRequest pendingRequest;
        private RenderCase pendingCase;
        private GameObject pendingCameraObject;
        private GameObject pendingRenderObject;
        private Material pendingMaterial;
        private Mesh pendingTemporaryMesh;
        private Matrix4x4 pendingLocalToClip;
        private Vector3 pendingCameraPosition;
        private bool waitingForReadback;
        private bool stageAQueued;
        private bool stageBQueued;
        private bool stageCQueued;
        private bool stageDQueued;
        private bool stageEQueued;
        private readonly List<string> stageBDirections = new();
        private readonly List<string> stageCDirections = new();
        private bool disposed;
        private int unresolvedSceneCleanRestorations;
        private string auditCameraNormalization = "not-run";
        private Summary summary;
        private bool fatalIdentityContractFailure;
        private string fatalContractReason = string.Empty;

        internal GeneratedMassSurfaceCausalityRenderAudit(
            Subject suspectSubject,
            Subject referenceSubject)
        {
            suspect = suspectSubject ?? throw new ArgumentNullException(nameof(suspectSubject));
            reference = referenceSubject;
            legacyMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                LegacyStoneMaterialPath);
            legacyMaterialLoadStatus = legacyMaterial == null
                ? "missing:" + LegacyStoneMaterialPath
                : "loaded:" + LegacyStoneMaterialPath;
            triangleIdentityShader = AssetDatabase.LoadAssetAtPath<Shader>(
                TriangleIdentityShaderPath);
            triangleIdentityShaderCompilerErrors =
                CountShaderCompilerErrors(triangleIdentityShader);
            triangleIdentityShaderPassIndex =
                ResolveShaderPassIndex(
                    triangleIdentityShader,
                    TriangleIdentityPassName);
            floatingPointCaptureSupported =
                SystemInfo.supportsAsyncGPUReadback &&
                SystemInfo.SupportsRenderTextureFormat(
                    RenderTextureFormat.ARGBFloat) &&
                SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat);
            floatingPointCaptureStatus = floatingPointCaptureSupported
                ? "ARGBFloat+RGBAFloatReadback"
                : "unsupported:async=" +
                    (SystemInfo.supportsAsyncGPUReadback ? 1 : 0) +
                    ",argbFloat=" +
                    (SystemInfo.SupportsRenderTextureFormat(
                        RenderTextureFormat.ARGBFloat) ? 1 : 0) +
                    ",rgbaFloat=" +
                    (SystemInfo.SupportsTextureFormat(
                        TextureFormat.RGBAFloat) ? 1 : 0);
            triangleIdentityShaderLoadStatus =
                triangleIdentityShader == null
                    ? "missing:" + TriangleIdentityShaderPath
                    : !triangleIdentityShader.isSupported
                        ? "unsupported:" + TriangleIdentityShaderPath
                        : triangleIdentityShaderCompilerErrors > 0
                            ? "compiler-errors:" +
                                triangleIdentityShaderCompilerErrors
                            : triangleIdentityShaderPassIndex < 0
                                ? "missing-pass:" +
                                    TriangleIdentityPassName
                                : "loaded:" + TriangleIdentityShaderPath;
            BuildSurfaceClassIndices(suspect);
            BuildBevelParentSamples(suspect);
            if (reference != null)
            {
                BuildSurfaceClassIndices(reference);
                BuildBevelParentSamples(reference);
            }
            sourceCamera = ResolveSourceCamera();
            auditLayer = ResolveAuditLayer(
                suspect.Target.gameObject.layer,
                out requiresRendererSuppression);
            cameraRotation = sourceCamera != null
                ? sourceCamera.transform.rotation
                : Quaternion.Euler(28f, -135f, 0f);
            cameraCenter = ResolveCommonCenter(suspect);
            cameraRadius = ResolveCommonRadius(
                suspect,
                reference,
                suspect.Target.transform.lossyScale);
            ConfigureSubjectTransform(suspect);
            BuildExpectedMainDiffuse(suspect);
            if (reference != null)
            {
                ConfigureSubjectTransform(reference);
                BuildExpectedMainDiffuse(reference);
            }
            brdfDirections = BuildBrdfDirections();
            if (triangleIdentityShader == null ||
                !triangleIdentityShader.isSupported ||
                triangleIdentityShaderCompilerErrors > 0 ||
                triangleIdentityShaderPassIndex < 0)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason =
                    "TRIANGLE_IDENTITY_SHADER_PREFLIGHT_FAILURE:" +
                    triangleIdentityShaderLoadStatus;
            }
            if (!floatingPointCaptureSupported)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason =
                    "FLOATING_POINT_CAPTURE_PREFLIGHT_FAILURE:" +
                    floatingPointCaptureStatus;
            }
            int cpuRoundTripFailures =
                CountTriangleIdentityCpuRoundTripFailures(suspect);
            if (cpuRoundTripFailures > 0)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason =
                    "TRIANGLE_IDENTITY_CPU_ROUNDTRIP_FAILURE:" +
                    cpuRoundTripFailures;
            }
            BuildCases();
        }

        internal IReadOnlyList<CaseResult> Results => results;
        internal Summary FinalSummary => summary;
        internal int CompletedCases => results.Count(item =>
            item.CountsTowardDecisionTotal);
        internal int AuxiliaryIdentityCases => results.Count(item =>
            item.IsTriangleIdentity);
        internal int AuxiliaryValidationCases => results.Count(item =>
            item.IsTriangleIdentity || item.IsLambertNormalCapture);
        internal int OrientationViewCount => AlternateViews.Length + 1;
        internal int OrientationStaticCasesPerView =>
            OrientationStages.Length + 6 + 3;
        internal int OrientationDirectionalCasesPerDirection =>
            OrientationStages.Length + 2 + 3 + OrientationAblations.Length * 2;
        internal int OrientationCaseCount => legacyMaterial == null
            ? 0
            : OrientationViewCount *
                (OrientationStaticCasesPerView +
                 brdfDirections.Length * OrientationDirectionalCasesPerDirection);
        internal int TotalCases => legacyMaterial == null
            ? 1
            : 1 + brdfDirections.Length * 6 +
                StageBDirectionCount * 5 +
                StageCDirectionCount * AlternateViews.Length * 5 +
                6 +
                OrientationCaseCount;
        internal int TotalRenderPasses =>
            TotalCases + AlternateViews.Length + 2;
        internal bool IsComplete =>
            stageAQueued &&
            stageBQueued &&
            stageCQueued &&
            stageDQueued &&
            stageEQueued &&
            nextCase >= cases.Count &&
            !waitingForReadback;
        internal string ProgressText => waitingForReadback
            ? "Waiting for asynchronous GPU readback: " + pendingCase.Name
            : nextCase < cases.Count
                ? "Rendering causality pass " + (results.Count + 1) + "/" +
                    TotalRenderPasses + ": " + cases[nextCase].Name
                : !stageAQueued
                    ? "Validating pixelwise GPU-normal Lambert preflight and queueing Stage A"
                    : !stageBQueued
                        ? "Selecting four worst Stage A directions"
                        : !stageCQueued
                            ? "Selecting two worst Stage B directions"
                            : !stageDQueued
                                ? "Queueing indirect and actual-scene closure"
                                : !stageEQueued
                                    ? "Queueing exhaustive surface-orientation stage attribution"
                                    : "Finalizing complete lighting and orientation-causality matrix";

        internal bool Advance()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(
                    nameof(GeneratedMassSurfaceCausalityRenderAudit));
            }
            if (waitingForReadback)
            {
                if (!pendingRequest.done)
                {
                    return false;
                }
                CompleteReadback();
                return IsComplete;
            }
            if (fatalIdentityContractFailure)
            {
                stageAQueued = true;
                stageBQueued = true;
                stageCQueued = true;
                stageDQueued = true;
                stageEQueued = true;
                nextCase = cases.Count;
                if (summary == null)
                {
                    summary = BuildSummary();
                }
                return true;
            }
            if (nextCase >= cases.Count)
            {
                summary = null;
                if (!stageAQueued)
                {
                    stageAQueued = true;
                    AddBrdfStageACases();
                    return false;
                }
                if (!stageBQueued)
                {
                    stageBQueued = true;
                    QueueStageBCases();
                    return false;
                }
                if (!stageCQueued)
                {
                    stageCQueued = true;
                    QueueStageCCases();
                    return false;
                }
                if (!stageDQueued)
                {
                    stageDQueued = true;
                    QueueStageDCases();
                    return false;
                }
                if (!stageEQueued)
                {
                    stageEQueued = true;
                    QueueStageECases();
                    return false;
                }
                summary = BuildSummary();
                return true;
            }

            Dispatch(cases[nextCase++]);
            return false;
        }

        internal string BuildMaterialDiffReport()
        {
            List<string> lines = new()
            {
                "legacyControlPath=" + LegacyStoneMaterialPath,
                "legacyControlStatus=" + legacyMaterialLoadStatus,
                "legacyControlMaterial=" +
                    (legacyMaterial == null ? "<missing>" : legacyMaterial.name),
                "legacyControlShader=" +
                    (legacyMaterial == null || legacyMaterial.shader == null
                        ? "<missing>"
                        : legacyMaterial.shader.name),
                "sameMeshLegacyParityAvailable=" +
                    (legacyMaterial == null ? 0 : 1)
            };
            if (legacyMaterial != null)
            {
                lines.Add("[Current HLSL to legacy material diff]");
                lines.Add(BuildMaterialDiff(suspect.Material, legacyMaterial));
            }
            if (reference != null)
            {
                lines.Add("[Selected reference material diff]");
                lines.Add(BuildMaterialDiff(
                    suspect.Material,
                    reference.Material));
            }
            else
            {
                lines.Add("selectedReferenceMaterial=<not supplied>");
            }
            lines.Add(BuildRendererStateReport("suspect", suspect));
            if (reference != null)
            {
                lines.Add(BuildRendererStateReport("reference", reference));
            }
            return string.Join("\n", lines);
        }

        internal string BuildEnvironmentReport()
        {
            List<string> lines = new();
            lines.Add("legacyControlStatus=" + legacyMaterialLoadStatus);
            lines.Add("triangleIdentityShaderStatus=" +
                triangleIdentityShaderLoadStatus);
            lines.Add("triangleIdentityShaderCompilerErrors=" +
                triangleIdentityShaderCompilerErrors);
            lines.Add("triangleIdentityShaderPassIndex=" +
                triangleIdentityShaderPassIndex);
            lines.Add("lightingCaptureFormat=" +
                floatingPointCaptureStatus);
            lines.Add("triangleIdentityEncoding=RGB24_BASE255_NONZERO");
            lines.Add("triangleIdentityCpuRoundTripFailures=" +
                CountTriangleIdentityCpuRoundTripFailures(suspect));
            lines.Add("brdfDirectionCount=" + brdfDirections.Length);
            lines.Add("stageACaseCount=" +
                (legacyMaterial == null ? 0 : brdfDirections.Length * 6));
            lines.Add("stageBCaseCount=" +
                (legacyMaterial == null ? 0 : StageBDirectionCount * 5));
            lines.Add("stageCCaseCount=" +
                (legacyMaterial == null
                    ? 0
                    : StageCDirectionCount * AlternateViews.Length * 5));
            lines.Add("stageDCaseCount=" +
                (legacyMaterial == null ? 0 : 6));
            lines.Add("stageEOrientationCaseCount=" + OrientationCaseCount);
            lines.Add("orientationViewCount=" + OrientationViewCount);
            lines.Add("orientationStaticCasesPerView=" +
                OrientationStaticCasesPerView);
            lines.Add("orientationDirectionalCasesPerDirection=" +
                OrientationDirectionalCasesPerDirection);
            lines.Add("decisionCaseCount=" + TotalCases);
            lines.Add("lambertStoredNormalCaptureCaseCount=" +
                (legacyMaterial == null ? 0 : 1));
            lines.Add("lambertPreflightCaseCount=" +
                (legacyMaterial == null ? 0 : 1));
            lines.Add("auxiliaryIdentityPassBudget=" +
                (AlternateViews.Length + 1));
            lines.Add("auxiliaryValidationPassBudget=" +
                (AlternateViews.Length + 2));
            lines.Add("totalRenderPassBudget=" + TotalRenderPasses);
            lines.Add("suspectClassifiedTriangles=" +
                suspect.ClassifiedTriangleCount);
            lines.Add("suspectUnclassifiedTriangles=" +
                suspect.UnclassifiedTriangleCount);
            lines.Add("suspectBevelParentGeometrySamples=" +
                suspect.BevelParentSamples.Count);
            lines.Add("suspectSurfaceClassContractValid=" +
                (suspect.SurfaceClassContractValid ? 1 : 0));
            lines.Add("suspectPredictedSourceMainDiffuse=" +
                Format(GetExpectedMainDiffuse(suspect, SurfaceClass.SourceFace)));
            lines.Add("suspectPredictedBevelMainDiffuse=" +
                Format(GetExpectedMainDiffuse(suspect, SurfaceClass.OrdinaryBevel)));
            if (reference != null)
            {
                lines.Add("referenceClassifiedTriangles=" +
                    reference.ClassifiedTriangleCount);
                lines.Add("referenceUnclassifiedTriangles=" +
                    reference.UnclassifiedTriangleCount);
                lines.Add("referenceBevelParentGeometrySamples=" +
                    reference.BevelParentSamples.Count);
                lines.Add("referenceSurfaceClassContractValid=" +
                    (reference.SurfaceClassContractValid ? 1 : 0));
            }
            lines.Add("auditLayer=" + auditLayer);
            lines.Add("sceneRendererSuppressionFallback=" +
                (requiresRendererSuppression ? 1 : 0));
            lines.Add("sourceSubjectRenderersSuppressed=1");
            lines.Add("sceneCleanRestoreApi=" +
                (markSceneCleanMethod == null
                    ? "unavailable"
                    : markSceneCleanMethod.Name));
            lines.Add("unresolvedSceneCleanRestorations=" +
                unresolvedSceneCleanRestorations);
            lines.Add("sourceCamera=" +
                (sourceCamera == null ? "<none>" : sourceCamera.name));
            lines.Add("auditCameraNormalization=" +
                auditCameraNormalization);
            if (sourceCamera != null)
            {
                lines.Add("cameraProjection=" +
                    (sourceCamera.orthographic ? "orthographic" : "perspective"));
                lines.Add("cameraFieldOfView=" + Format(sourceCamera.fieldOfView));
                lines.Add("cameraOrthographicSize=" +
                    Format(sourceCamera.orthographicSize));
                lines.Add("cameraPostProcessing=" +
                    ReadRenderPostProcessing(sourceCamera));
            }
            lines.Add("colorSpace=" + QualitySettings.activeColorSpace);
            lines.Add("ambientMode=" + RenderSettings.ambientMode);
            lines.Add("ambientIntensity=" + Format(RenderSettings.ambientIntensity));
            lines.Add("reflectionIntensity=" +
                Format(RenderSettings.reflectionIntensity));
            lines.Add("fogEnabled=" + (RenderSettings.fog ? 1 : 0));
            lines.Add("fogMode=" + RenderSettings.fogMode);
            lines.Add("renderSettingsSun=" +
                (RenderSettings.sun == null ? "<none>" : RenderSettings.sun.name));
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include);
            lines.Add("sceneLightCount=" + lights.Length);
            foreach (Light light in lights
                .Where(x => x != null)
                .OrderBy(x => x.name, StringComparer.Ordinal))
            {
                lines.Add(
                    "light=" + light.name +
                    ",enabled=" + (light.enabled ? 1 : 0) +
                    ",active=" + (light.gameObject.activeInHierarchy ? 1 : 0) +
                    ",type=" + light.type +
                    ",intensity=" + Format(light.intensity) +
                    ",color=" + Format(light.color.r) + "," +
                        Format(light.color.g) + "," +
                        Format(light.color.b) +
                    ",useColorTemperature=" +
                        (light.useColorTemperature ? 1 : 0) +
                    ",colorTemperature=" +
                        Format(light.colorTemperature) +
                    ",shadows=" + light.shadows +
                    ",cullingMask=" + light.cullingMask +
                    ",renderingLayerMask=" + light.renderingLayerMask);
            }
            return string.Join("\n", lines);
        }
        private void BuildCases()
        {
            AddTriangleIdentityCase(
                suspect,
                "CURRENT",
                0f,
                isAuxiliary: true);
            AddLambertStoredNormalCaptureCase();
            AddLambertPreflightCase();
        }


        private void AddLambertStoredNormalCaptureCase()
        {
            Vector3 cameraPosition = ResolveAuditCameraPosition(0f);
            Vector3 worldToLight = cameraPosition - cameraCenter;
            Vector3 localToLight = suspect.CloneLocalToWorld.inverse
                .MultiplyVector(worldToLight)
                .normalized;
            if (localToLight.sqrMagnitude <= 0.5f)
            {
                localToLight = Vector3.up;
            }
            BrdfDirectionDefinition direction =
                new BrdfDirectionDefinition(
                    "LAMBERT_CAMERA",
                    localToLight);
            RenderCase normalCapture = CreateBrdfCase(
                direction,
                "LAMBERT_STORED_NORMAL",
                suspect.Material,
                "HlslLambertStoredNormal",
                0f,
                constantNeutralAlbedo: true,
                family: "LambertNormalCapture",
                storedNormals: true);
            normalCapture.CausalityMode = 14;
            normalCapture.IsBrdfSweep = false;
            normalCapture.IsAdaptiveBrdf = false;
            normalCapture.IsLambertNormalCapture = true;
            normalCapture.CountsTowardDecisionTotal = false;
            cases.Add(normalCapture);
        }

        private void AddLambertPreflightCase()
        {
            Vector3 cameraPosition = ResolveAuditCameraPosition(0f);
            Vector3 worldToLight = cameraPosition - cameraCenter;
            Vector3 localToLight = suspect.CloneLocalToWorld.inverse
                .MultiplyVector(worldToLight)
                .normalized;
            if (localToLight.sqrMagnitude <= 0.5f)
            {
                localToLight = Vector3.up;
            }
            BrdfDirectionDefinition direction =
                new BrdfDirectionDefinition(
                    "LAMBERT_CAMERA",
                    localToLight);
            RenderCase preflight = CreateBrdfCase(
                direction,
                "LAMBERT_PREFLIGHT",
                suspect.Material,
                "HlslLambertPreflight",
                0f,
                constantNeutralAlbedo: true,
                family: "LambertPreflight",
                storedNormals: true);
            preflight.CausalityMode = 12;
            preflight.FloatOverrides["_Smoothness"] = 0f;
            preflight.IsLambertPreflight = true;
            preflight.IsAdaptiveBrdf = false;
            cases.Add(preflight);
        }

        private Vector3 ResolveAuditCameraPosition(float azimuthDegrees)
        {
            Quaternion rotation = Quaternion.AngleAxis(
                azimuthDegrees,
                Vector3.up) * cameraRotation;
            Vector3 forward = rotation * Vector3.forward;
            if (sourceCamera != null && sourceCamera.orthographic)
            {
                return cameraCenter - forward * (cameraRadius * 4f + 1f);
            }
            float fieldOfView = sourceCamera == null
                ? 60f
                : Mathf.Clamp(sourceCamera.fieldOfView, 20f, 70f);
            float distance =
                cameraRadius /
                Mathf.Max(
                    0.1f,
                    Mathf.Tan(fieldOfView * Mathf.Deg2Rad * 0.5f)) *
                1.30f;
            return cameraCenter - forward * distance;
        }

        private void AddTriangleIdentityCase(
            Subject subject,
            string viewName,
            float cameraAzimuthDegrees,
            bool isAuxiliary)
        {
            cases.Add(new RenderCase
            {
                Name = "TRIANGLE_IDENTITY__" +
                    subject.Role.ToUpperInvariant() + "__" + viewName,
                MeshSubject = subject,
                PropertySubject = subject,
                SourceMaterial = subject.Material,
                MaterialRole = "DedicatedTriangleIdentity",
                Family = "TriangleIdentity",
                ClearPropertyBlock = true,
                DisablePost = true,
                DisableFog = true,
                IsTriangleIdentity = true,
                IsAuxiliaryIdentity = true,
                CountsTowardDecisionTotal = false,
                ViewName = viewName,
                CameraAzimuthDegrees = cameraAzimuthDegrees
            });
        }

        private void AddFinalCase(
            string name,
            Subject meshSubject,
            Material material,
            Subject propertySubject,
            string materialRole,
            bool highIntensity,
            bool clearPropertyBlock = false)
        {
            cases.Add(new RenderCase
            {
                Name = name,
                MeshSubject = meshSubject,
                PropertySubject = propertySubject,
                SourceMaterial = material,
                MaterialRole = materialRole,
                Family = "ParityMatrix",
                HighIntensity = highIntensity,
                ClearPropertyBlock = clearPropertyBlock
            });
        }

        private BrdfDirectionDefinition[] BuildBrdfDirections()
        {
            List<BrdfDirectionDefinition> values =
                new List<BrdfDirectionDefinition>(BaseBrdfDirections);
            Light sceneMain = RenderSettings.sun;
            if (sceneMain != null)
            {
                Vector3 worldToLight = -sceneMain.transform.forward;
                Vector3 localToLight = suspect.CloneLocalToWorld.inverse
                    .MultiplyVector(worldToLight)
                    .normalized;
                if (localToLight.sqrMagnitude > 0.5f)
                {
                    values.Add(new BrdfDirectionDefinition(
                        "SCENE_MAIN",
                        localToLight));
                }
            }
            if (values.Count == BaseBrdfDirections.Length)
            {
                values.Add(new BrdfDirectionDefinition(
                    "SCENE_MAIN_FALLBACK",
                    new Vector3(0.35f, 1f, -0.25f)));
            }
            return values.ToArray();
        }
        private void AddBrdfStageACases()
        {
            if (legacyMaterial == null)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason = "LEGACY_CONTROL_MISSING";
                return;
            }

            foreach (BrdfDirectionDefinition direction in brdfDirections)
            {
                cases.Add(CreateBrdfCase(
                    direction,
                    "A_LEGACY_NEUTRAL_FULL",
                    legacyMaterial,
                    "LegacyNeutralFull",
                    DielectricF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true));

                RenderCase legacySpecular = CreateBrdfCase(
                    direction,
                    "A_LEGACY_BLACK_SPECULAR",
                    legacyMaterial,
                    "LegacyBlackSpecular",
                    DielectricF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true);
                ConfigureBlackAlbedoSpecularOnly(legacySpecular);
                cases.Add(legacySpecular);

                cases.Add(CreateBrdfCase(
                    direction,
                    "A_HLSL016_NEUTRAL_STORED",
                    suspect.Material,
                    "Hlsl016NeutralStored",
                    CurrentHlslF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true));

                RenderCase currentSpecular = CreateBrdfCase(
                    direction,
                    "A_HLSL016_BLACK_SPECULAR_STORED",
                    suspect.Material,
                    "Hlsl016BlackSpecularStored",
                    CurrentHlslF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true);
                ConfigureBlackAlbedoSpecularOnly(currentSpecular);
                cases.Add(currentSpecular);

                cases.Add(CreateBrdfCase(
                    direction,
                    "A_HLSL004_NEUTRAL_STORED",
                    suspect.Material,
                    "Hlsl004NeutralStored",
                    DielectricF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true));

                RenderCase dielectricSpecular = CreateBrdfCase(
                    direction,
                    "A_HLSL004_BLACK_SPECULAR_STORED",
                    suspect.Material,
                    "Hlsl004BlackSpecularStored",
                    DielectricF0,
                    constantNeutralAlbedo: true,
                    family: "StageA",
                    storedNormals: true);
                ConfigureBlackAlbedoSpecularOnly(dielectricSpecular);
                cases.Add(dielectricSpecular);
            }
        }
        private void QueueStageBCases()
        {
            List<string> selected = SelectWorstDirections(
                "A_LEGACY_NEUTRAL_FULL",
                "A_HLSL016_NEUTRAL_STORED",
                StageBDirectionCount);
            if (selected.Count != StageBDirectionCount)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason =
                    "STAGE_A_DIRECTION_COVERAGE_FAILURE:" + selected.Count;
                return;
            }

            stageBDirections.Clear();
            stageBDirections.AddRange(selected);
            foreach (string directionName in stageBDirections)
            {
                BrdfDirectionDefinition direction = FindDirection(directionName);
                cases.Add(CreateBrdfCase(
                    direction,
                    "B_LEGACY_ACTUAL_FULL",
                    legacyMaterial,
                    "LegacyActualFull",
                    DielectricF0,
                    constantNeutralAlbedo: false,
                    family: "StageB",
                    storedNormals: true));
                cases.Add(CreateBrdfCase(
                    direction,
                    "B_HLSL016_ACTUAL_GENERATED",
                    suspect.Material,
                    "Hlsl016ActualGenerated",
                    CurrentHlslF0,
                    constantNeutralAlbedo: false,
                    family: "StageB",
                    storedNormals: false));
                cases.Add(CreateBrdfCase(
                    direction,
                    "B_HLSL016_ACTUAL_STORED",
                    suspect.Material,
                    "Hlsl016ActualStored",
                    CurrentHlslF0,
                    constantNeutralAlbedo: false,
                    family: "StageB",
                    storedNormals: true));
                cases.Add(CreateBrdfCase(
                    direction,
                    "B_HLSL004_ACTUAL_GENERATED",
                    suspect.Material,
                    "Hlsl004ActualGenerated",
                    DielectricF0,
                    constantNeutralAlbedo: false,
                    family: "StageB",
                    storedNormals: false));
                cases.Add(CreateBrdfCase(
                    direction,
                    "B_HLSL004_ACTUAL_STORED",
                    suspect.Material,
                    "Hlsl004ActualStored",
                    DielectricF0,
                    constantNeutralAlbedo: false,
                    family: "StageB",
                    storedNormals: true));
            }
        }

        private void QueueStageCCases()
        {
            List<string> selected = SelectWorstDirections(
                "B_LEGACY_ACTUAL_FULL",
                "B_HLSL016_ACTUAL_GENERATED",
                StageCDirectionCount,
                stageBDirections);
            if (selected.Count != StageCDirectionCount)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason =
                    "STAGE_B_DIRECTION_COVERAGE_FAILURE:" + selected.Count;
                return;
            }

            stageCDirections.Clear();
            stageCDirections.AddRange(selected);
            foreach (ViewDefinition view in AlternateViews)
            {
                AddTriangleIdentityCase(
                    suspect,
                    view.Name,
                    view.AzimuthDegrees,
                    isAuxiliary: true);
            }

            foreach (string directionName in stageCDirections)
            {
                BrdfDirectionDefinition direction = FindDirection(directionName);
                foreach (ViewDefinition view in AlternateViews)
                {
                    cases.Add(CreateBrdfCase(
                        direction,
                        "C_LEGACY_ACTUAL_FULL",
                        legacyMaterial,
                        "LegacyActualFullAlternateView",
                        DielectricF0,
                        constantNeutralAlbedo: false,
                        family: "StageC",
                        storedNormals: true,
                        viewName: view.Name,
                        cameraAzimuthDegrees: view.AzimuthDegrees));
                    cases.Add(CreateBrdfCase(
                        direction,
                        "C_HLSL016_ACTUAL_GENERATED",
                        suspect.Material,
                        "Hlsl016ActualGeneratedAlternateView",
                        CurrentHlslF0,
                        constantNeutralAlbedo: false,
                        family: "StageC",
                        storedNormals: false,
                        viewName: view.Name,
                        cameraAzimuthDegrees: view.AzimuthDegrees));
                    cases.Add(CreateBrdfCase(
                        direction,
                        "C_HLSL016_ACTUAL_STORED",
                        suspect.Material,
                        "Hlsl016ActualStoredAlternateView",
                        CurrentHlslF0,
                        constantNeutralAlbedo: false,
                        family: "StageC",
                        storedNormals: true,
                        viewName: view.Name,
                        cameraAzimuthDegrees: view.AzimuthDegrees));
                    cases.Add(CreateBrdfCase(
                        direction,
                        "C_HLSL004_ACTUAL_GENERATED",
                        suspect.Material,
                        "Hlsl004ActualGeneratedAlternateView",
                        DielectricF0,
                        constantNeutralAlbedo: false,
                        family: "StageC",
                        storedNormals: false,
                        viewName: view.Name,
                        cameraAzimuthDegrees: view.AzimuthDegrees));
                    cases.Add(CreateBrdfCase(
                        direction,
                        "C_HLSL004_ACTUAL_STORED",
                        suspect.Material,
                        "Hlsl004ActualStoredAlternateView",
                        DielectricF0,
                        constantNeutralAlbedo: false,
                        family: "StageC",
                        storedNormals: true,
                        viewName: view.Name,
                        cameraAzimuthDegrees: view.AzimuthDegrees));
                }
            }
        }

        private void QueueStageDCases()
        {
            Vector3 sceneDirection = brdfDirections
                .First(item => item.Name.StartsWith(
                    "SCENE_MAIN",
                    StringComparison.Ordinal))
                .LocalDirection;
            BrdfDirectionDefinition scene =
                new BrdfDirectionDefinition("SCENE", sceneDirection);

            cases.Add(CreateClosureCase(
                scene,
                "D_LEGACY_INDIRECT_ONLY",
                legacyMaterial,
                "LegacyIndirectOnly",
                DielectricF0,
                indirectOnly: true));
            cases.Add(CreateClosureCase(
                scene,
                "D_HLSL016_INDIRECT_ONLY",
                suspect.Material,
                "Hlsl016IndirectOnly",
                CurrentHlslF0,
                indirectOnly: true));
            cases.Add(CreateClosureCase(
                scene,
                "D_HLSL004_INDIRECT_ONLY",
                suspect.Material,
                "Hlsl004IndirectOnly",
                DielectricF0,
                indirectOnly: true));
            cases.Add(CreateClosureCase(
                scene,
                "D_LEGACY_ACTUAL_SCENE",
                legacyMaterial,
                "LegacyActualScene",
                DielectricF0,
                indirectOnly: false));
            cases.Add(CreateClosureCase(
                scene,
                "D_HLSL016_ACTUAL_SCENE",
                suspect.Material,
                "Hlsl016ActualScene",
                CurrentHlslF0,
                indirectOnly: false));
            cases.Add(CreateClosureCase(
                scene,
                "D_HLSL004_ACTUAL_SCENE",
                suspect.Material,
                "Hlsl004ActualScene",
                DielectricF0,
                indirectOnly: false));
        }
        private void QueueStageECases()
        {
            if (legacyMaterial == null)
            {
                fatalIdentityContractFailure = true;
                fatalContractReason = "ORIENTATION_LEGACY_CONTROL_MISSING";
                return;
            }

            foreach (ViewDefinition view in EnumerateOrientationViews())
            {
                AddOrientationStaticCase(
                    view,
                    "TRIANGLE_NORMAL",
                    5,
                    "StaticNormal");
                AddOrientationStaticCase(
                    view,
                    "CURRENT_RESOLVED_NORMAL",
                    6,
                    "StaticNormal");
                AddOrientationStaticCase(
                    view,
                    "STORED_NORMAL",
                    14,
                    "StaticNormal");

                foreach (OrientationStageDefinition stage in OrientationStages)
                {
                    AddOrientationStaticCase(
                        view,
                        "ALBEDO_" + stage.Name,
                        stage.AlbedoMode,
                        "StaticAlbedo",
                        stage.Name);
                }

                AddOrientationStaticCase(view, "RAW_VERTEX_MASKS", 32, "StaticMask");
                AddOrientationStaticCase(view, "RAW_DIRT_HEIGHT_NORMALY", 33, "StaticMask");
                AddOrientationStaticCase(view, "RESOLVED_MASKS_A", 34, "StaticMask");
                AddOrientationStaticCase(view, "RESOLVED_MASKS_B", 35, "StaticMask");
                AddOrientationStaticCase(view, "RESOLVED_MASKS_C", 36, "StaticMask");
                AddOrientationStaticCase(view, "RESPONSE_SCALARS", 37, "StaticMask");

                foreach (BrdfDirectionDefinition direction in brdfDirections)
                {
                    // Queue NdotL before cumulative direct stages so 5Q can
                    // validate direct == capturedAlbedo * capturedNdotL *
                    // captured attenuation pixel-by-pixel without retaining
                    // every direct render in memory.
                    cases.Add(CreateOrientationHlslCase(
                        view,
                        direction,
                        "NDOTL_ATTENUATION_STORED",
                        55,
                        "NdotL",
                        "NDOTL_ATTENUATION",
                        storedNormals: true));

                    foreach (OrientationStageDefinition stage in OrientationStages)
                    {
                        RenderCase directStage = CreateOrientationHlslCase(
                            view,
                            direction,
                            "DIRECT_" + stage.Name,
                            stage.DirectMode,
                            "DirectStage",
                            stage.Name,
                            storedNormals: true);
                        cases.Add(directStage);
                    }

                    cases.Add(CreateOrientationHlslCase(
                        view,
                        direction,
                        "MAIN_LIGHT_DIRECTION",
                        56,
                        "LightVector",
                        "MAIN_LIGHT_DIRECTION",
                        storedNormals: true));

                    cases.Add(CreateOrientationLegacyCase(
                        view,
                        direction,
                        "PBR_LEGACY_ACTUAL"));
                    cases.Add(CreateOrientationHlslCase(
                        view,
                        direction,
                        "PBR_HLSL_PRODUCTION",
                        0,
                        "PBRReference",
                        "PRODUCTION",
                        storedNormals: false));
                    cases.Add(CreateOrientationHlslCase(
                        view,
                        direction,
                        "PBR_HLSL_STORED",
                        0,
                        "PBRReference",
                        "STORED",
                        storedNormals: true));

                    foreach (string ablation in OrientationAblations)
                    {
                        RenderCase directAblation = CreateOrientationHlslCase(
                            view,
                            direction,
                            "DIRECT_ABLATE_" + ablation,
                            54,
                            "AblationDirect",
                            "FINAL_WITH_OVERALL_TINT",
                            storedNormals: true);
                        directAblation.OrientationAblation = ablation;
                        ApplyOrientationAblation(directAblation, ablation);
                        cases.Add(directAblation);

                        RenderCase pbrAblation = CreateOrientationHlslCase(
                            view,
                            direction,
                            "PBR_ABLATE_" + ablation,
                            0,
                            "AblationPBR",
                            "PBR",
                            storedNormals: true);
                        pbrAblation.OrientationAblation = ablation;
                        ApplyOrientationAblation(pbrAblation, ablation);
                        cases.Add(pbrAblation);
                    }
                }
            }
        }

        private IEnumerable<ViewDefinition> EnumerateOrientationViews()
        {
            yield return new ViewDefinition("CURRENT", 0f);
            foreach (ViewDefinition view in AlternateViews)
            {
                yield return view;
            }
        }

        private void AddOrientationStaticCase(
            ViewDefinition view,
            string variant,
            int mode,
            string kind,
            string stage = "")
        {
            RenderCase renderCase = new RenderCase
            {
                Name = "STAGEE__STATIC__" + view.Name + "__" + variant,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = suspect.Material,
                MaterialRole = "HlslOrientationStatic",
                Family = "StageE",
                DisableShadows = true,
                DisableAdditionalLights = true,
                DisablePost = true,
                DisableLightProbes = true,
                DisableReflectionProbes = true,
                DisableAmbientEnvironment = true,
                DisableReflectionEnvironment = true,
                DisableLightCookies = true,
                DisableAllLights = true,
                DisableFog = true,
                CausalityMode = mode,
                IsOrientationSweep = true,
                OrientationKind = kind,
                OrientationStage = stage,
                BrdfVariant = variant,
                ViewName = view.Name,
                CameraAzimuthDegrees = view.AzimuthDegrees
            };
            cases.Add(renderCase);
        }

        private RenderCase CreateOrientationHlslCase(
            ViewDefinition view,
            BrdfDirectionDefinition direction,
            string variant,
            int mode,
            string kind,
            string stage,
            bool storedNormals)
        {
            RenderCase renderCase = new RenderCase
            {
                Name = "STAGEE__" + direction.Name + "__" +
                    view.Name + "__" + variant,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = suspect.Material,
                MaterialRole = "HlslOrientation",
                Family = "StageE",
                DisableShadows = true,
                DisableAdditionalLights = true,
                DisablePost = true,
                DisableLightProbes = true,
                DisableReflectionProbes = true,
                DisableAmbientEnvironment = true,
                DisableReflectionEnvironment = true,
                DisableLightCookies = true,
                DisableFog = true,
                UseControlledMainLight = true,
                MainLightDirectionLocal = direction.LocalDirection,
                MainLightIntensity = 1f,
                CausalityMode = mode,
                IsOrientationSweep = true,
                OrientationKind = kind,
                OrientationStage = stage,
                DirectionName = direction.Name,
                BrdfVariant = variant,
                ViewName = view.Name,
                CameraAzimuthDegrees = view.AzimuthDegrees,
                LightDirectionLocal = direction.LocalDirection
            };
            renderCase.FloatOverrides["_SpecularStrength"] = DielectricF0;
            renderCase.FloatOverrides["_SpecularHighlights"] = 1f;
            if (storedNormals)
            {
                renderCase.FloatOverrides[
                    "_GeneratedMassSurfaceNormalStrength"] = 0f;
                renderCase.FloatOverrides["_FlatNormalStrength"] = 0f;
            }
            return renderCase;
        }

        private RenderCase CreateOrientationLegacyCase(
            ViewDefinition view,
            BrdfDirectionDefinition direction,
            string variant)
        {
            return new RenderCase
            {
                Name = "STAGEE__" + direction.Name + "__" +
                    view.Name + "__" + variant,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = legacyMaterial,
                MaterialRole = "LegacyOrientationReference",
                Family = "StageE",
                DisableShadows = true,
                DisableAdditionalLights = true,
                DisablePost = true,
                DisableLightProbes = true,
                DisableReflectionProbes = true,
                DisableAmbientEnvironment = true,
                DisableReflectionEnvironment = true,
                DisableLightCookies = true,
                DisableFog = true,
                UseControlledMainLight = true,
                MainLightDirectionLocal = direction.LocalDirection,
                MainLightIntensity = 1f,
                IsOrientationSweep = true,
                OrientationKind = "PBRReference",
                OrientationStage = "LEGACY",
                DirectionName = direction.Name,
                BrdfVariant = variant,
                ViewName = view.Name,
                CameraAzimuthDegrees = view.AzimuthDegrees,
                LightDirectionLocal = direction.LocalDirection
            };
        }

        private static void ApplyOrientationAblation(
            RenderCase renderCase,
            string ablation)
        {
            void DisableTonal()
            {
                renderCase.FloatOverrides["_PixelVariation"] = 0f;
                renderCase.FloatOverrides["_PixelVertexVariation"] = 0f;
                renderCase.FloatOverrides["_PixelBroadVariation"] = 0f;
                renderCase.FloatOverrides["_PixelEffectStrength"] = 0f;
                renderCase.FloatOverrides["_PixelWarpStrength"] = 0f;
            }
            void DisableExposure()
            {
                renderCase.FloatOverrides["_ExposureTintStrength"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassExposureResponse"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassExposureTintStrength"] = 0f;
            }
            void DisableMottle()
            {
                renderCase.FloatOverrides["_StoneMottleStrength"] = 0f;
            }
            void DisableCrevice()
            {
                renderCase.FloatOverrides["_CreviceDarkenStrength"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassCreviceResponse"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassCreviceTintStrength"] = 0f;
            }
            void DisableBase()
            {
                renderCase.FloatOverrides["_BaseDarkenStrength"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassBaseResponse"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassBaseTintStrength"] = 0f;
            }
            void DisableDirt()
            {
                renderCase.FloatOverrides["_StoneDirtResponse"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassDirtDepositResponse"] = 0f;
                renderCase.FloatOverrides["_GeneratedMassDirtDepositTintStrength"] = 0f;
            }

            switch (ablation)
            {
                case "TONAL":
                    DisableTonal();
                    break;
                case "EXPOSURE":
                    DisableExposure();
                    break;
                case "MOTTLE":
                    DisableMottle();
                    break;
                case "CREVICE":
                    DisableCrevice();
                    break;
                case "BASE":
                    DisableBase();
                    break;
                case "DIRT":
                    DisableDirt();
                    break;
                case "WET":
                    renderCase.FloatOverrides["_Wetness"] = 0f;
                    break;
                case "FROST":
                    renderCase.FloatOverrides["_FrostStrength"] = 0f;
                    break;
                case "MONOLITHIC":
                    renderCase.FloatOverrides["_MonolithicFlatten"] = 0f;
                    break;
                case "OVERALL_TINT":
                    renderCase.FloatOverrides[
                        "_GeneratedMassOverallRockTintStrength"] = 0f;
                    break;
                case "SPECULAR_ZERO":
                    renderCase.FloatOverrides["_SpecularStrength"] = 0f;
                    break;
                case "ALL_PRELIGHT_VALUE":
                    DisableTonal();
                    DisableExposure();
                    DisableMottle();
                    DisableCrevice();
                    DisableBase();
                    DisableDirt();
                    renderCase.FloatOverrides["_Wetness"] = 0f;
                    renderCase.FloatOverrides["_FrostStrength"] = 0f;
                    renderCase.FloatOverrides["_MonolithicFlatten"] = 0f;
                    renderCase.FloatOverrides[
                        "_GeneratedMassOverallRockTintStrength"] = 0f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(ablation),
                        ablation,
                        "Unknown orientation ablation.");
            }
        }

        private RenderCase CreateBrdfCase(
            BrdfDirectionDefinition direction,
            string variant,
            Material sourceMaterial,
            string materialRole,
            float dielectricF0,
            bool constantNeutralAlbedo,
            string family,
            bool storedNormals,
            string viewName = "CURRENT",
            float cameraAzimuthDegrees = 0f)
        {
            RenderCase renderCase = new RenderCase
            {
                Name = family.ToUpperInvariant() + "__" +
                    direction.Name + "__" + viewName + "__" + variant,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = sourceMaterial,
                MaterialRole = materialRole,
                Family = family,
                ClearPropertyBlock = constantNeutralAlbedo,
                DisableShadows = true,
                DisableAdditionalLights = true,
                DisablePost = true,
                DisableLightProbes = true,
                DisableReflectionProbes = true,
                DisableAmbientEnvironment = true,
                DisableReflectionEnvironment = true,
                DisableLightCookies = true,
                DisableFog = true,
                UseControlledMainLight = true,
                MainLightDirectionLocal = direction.LocalDirection,
                MainLightIntensity = 1f,
                IsBrdfSweep = true,
                IsAdaptiveBrdf = family != "StageA",
                DirectionName = direction.Name,
                BrdfVariant = variant,
                ViewName = viewName,
                CameraAzimuthDegrees = cameraAzimuthDegrees,
                LightDirectionLocal = direction.LocalDirection
            };
            renderCase.FloatOverrides["_SpecularStrength"] = dielectricF0;
            renderCase.FloatOverrides["_SpecularHighlights"] = 1f;
            if (storedNormals)
            {
                renderCase.FloatOverrides[
                    "_GeneratedMassSurfaceNormalStrength"] = 0f;
                renderCase.FloatOverrides["_FlatNormalStrength"] = 0f;
            }
            if (constantNeutralAlbedo)
            {
                renderCase.FloatOverrides["_Smoothness"] = 0.2f;
                renderCase.FloatOverrides["_Metallic"] = 0f;
                ApplyConstantNeutralBrdfOverrides(renderCase);
            }
            return renderCase;
        }

        private RenderCase CreateClosureCase(
            BrdfDirectionDefinition sceneDirection,
            string variant,
            Material sourceMaterial,
            string materialRole,
            float dielectricF0,
            bool indirectOnly)
        {
            RenderCase renderCase = new RenderCase
            {
                Name = "STAGED__SCENE__CURRENT__" + variant,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = sourceMaterial,
                MaterialRole = materialRole,
                Family = "StageD",
                IsBrdfSweep = true,
                IsAdaptiveBrdf = true,
                DirectionName = sceneDirection.Name,
                BrdfVariant = variant,
                ViewName = "CURRENT",
                LightDirectionLocal = sceneDirection.LocalDirection,
                DisableAllLights = indirectOnly,
                DisablePost = true,
                DisableFog = true
            };
            renderCase.FloatOverrides["_SpecularStrength"] = dielectricF0;
            renderCase.FloatOverrides["_SpecularHighlights"] = 1f;
            return renderCase;
        }
        private static void ConfigureBlackAlbedoSpecularOnly(
            RenderCase renderCase)
        {
            renderCase.ColorOverrides["_BaseColor"] =
                new Color(0f, 0f, 0f, 1f);
            renderCase.ColorOverrides["_Color"] =
                new Color(0f, 0f, 0f, 1f);
        }


        private BrdfDirectionDefinition FindDirection(string name)
        {
            return brdfDirections.First(item => string.Equals(
                item.Name,
                name,
                StringComparison.Ordinal));
        }

        private List<string> SelectWorstDirections(
            string legacyVariant,
            string candidateVariant,
            int count,
            IEnumerable<string> allowedDirections = null)
        {
            HashSet<string> allowed = allowedDirections == null
                ? null
                : new HashSet<string>(
                    allowedDirections,
                    StringComparer.Ordinal);
            return brdfDirections
                .Where(item => allowed == null || allowed.Contains(item.Name))
                .Select(item =>
                {
                    CaseResult legacy = FindBrdfCase(
                        item.Name,
                        legacyVariant,
                        "CURRENT");
                    CaseResult candidate = FindBrdfCase(
                        item.Name,
                        candidateVariant,
                        "CURRENT");
                    return new
                    {
                        item.Name,
                        Evaluable = DirectionCaseEvaluable(legacy, candidate),
                        Residual = CalculateCaseRgbMeanAbsoluteResidual(
                            legacy,
                            candidate)
                    };
                })
                .Where(item => item.Evaluable && item.Residual >= 0f)
                .OrderByDescending(item => item.Residual)
                .ThenBy(item => item.Name, StringComparer.Ordinal)
                .Take(count)
                .Select(item => item.Name)
                .ToList();
        }

        private static void ApplyConstantNeutralBrdfOverrides(
            RenderCase renderCase)
        {
            renderCase.TextureOverrides["_BaseMap"] = Texture2D.whiteTexture;
            renderCase.ColorOverrides["_BaseColor"] =
                new Color(0.5f, 0.5f, 0.5f, 1f);
            renderCase.FloatOverrides["_PixelVariation"] = 0f;
            renderCase.FloatOverrides["_PixelVertexVariation"] = 0f;
            renderCase.FloatOverrides["_PixelBroadVariation"] = 0f;
            renderCase.FloatOverrides["_PixelEffectStrength"] = 0f;
            renderCase.FloatOverrides["_PixelWarpStrength"] = 0f;
            renderCase.FloatOverrides["_StoneMottleStrength"] = 0f;
            renderCase.FloatOverrides["_ExposureTintStrength"] = 0f;
            renderCase.FloatOverrides["_CreviceDarkenStrength"] = 0f;
            renderCase.FloatOverrides["_BaseDarkenStrength"] = 0f;
            renderCase.FloatOverrides["_StoneDirtResponse"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassExposureResponse"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassCreviceResponse"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassBaseResponse"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassDirtDepositResponse"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassExposureTintStrength"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassCreviceTintStrength"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassBaseTintStrength"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassDirtDepositTintStrength"] = 0f;
            renderCase.FloatOverrides["_GeneratedMassOverallRockTintStrength"] = 0f;
            renderCase.FloatOverrides["_Wetness"] = 0f;
            renderCase.FloatOverrides["_FrostStrength"] = 0f;
            renderCase.FloatOverrides["_MonolithicFlatten"] = 0f;
            renderCase.FloatOverrides["_ProfileContrast"] = 1f;
            renderCase.FloatOverrides["_ProfilePixelContrast"] = 1f;
            renderCase.FloatOverrides["_HighlightCompressStrength"] = 0f;
            renderCase.FloatOverrides["_BottomDarkenStrength"] = 0f;
            renderCase.FloatOverrides["_EdgeDarkenStrength"] = 0f;
            renderCase.FloatOverrides["_DirectStrength"] = 1f;
            renderCase.FloatOverrides["_DiffuseWrap"] = 0f;
            renderCase.FloatOverrides["_AmbientStrength"] = 0f;
            renderCase.FloatOverrides["_ShadowAmbientStrength"] = 0f;
        }

        private void AddModeCase(
            string name,
            int mode,
            string family,
            bool disablePost = false,
            bool isAblation = false)
        {
            cases.Add(new RenderCase
            {
                Name = name,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = suspect.Material,
                MaterialRole = "Suspect",
                Family = family,
                CausalityMode = mode,
                DisablePost = disablePost,
                IsAblation = isAblation
            });
        }

        private void AddOverrideCase(
            string name,
            string family,
            params (string Name, float Value)[] overrides)
        {
            RenderCase renderCase = CreateOverrideCase(name, family, false, false);
            renderCase.IsAblation = true;
            if (overrides != null)
            {
                foreach ((string Name, float Value) item in overrides)
                    renderCase.FloatOverrides[item.Name] = item.Value;
            }
            cases.Add(renderCase);
        }

        private void AddOverrideCase(
            string name,
            string family,
            bool disableShadows = false,
            bool disableAdditionalLights = false,
            bool disableLightProbes = false,
            bool disableReflectionProbes = false,
            bool disableAmbientEnvironment = false,
            bool disableReflectionEnvironment = false,
            bool disableLightCookies = false,
            bool disableAllLights = false)
        {
            RenderCase renderCase = CreateOverrideCase(
                name,
                family,
                disableShadows,
                disableAdditionalLights);
            renderCase.DisableLightProbes = disableLightProbes;
            renderCase.DisableReflectionProbes = disableReflectionProbes;
            renderCase.DisableAmbientEnvironment = disableAmbientEnvironment;
            renderCase.DisableReflectionEnvironment = disableReflectionEnvironment;
            renderCase.DisableLightCookies = disableLightCookies;
            renderCase.DisableAllLights = disableAllLights;
            cases.Add(renderCase);
        }

        private RenderCase CreateOverrideCase(
            string name,
            string family,
            bool disableShadows,
            bool disableAdditionalLights)
        {
            return new RenderCase
            {
                Name = name,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = suspect.Material,
                MaterialRole = "Suspect",
                Family = family,
                DisableShadows = disableShadows,
                DisableAdditionalLights = disableAdditionalLights,
                IsAblation = true
            };
        }

        private void AddMaskDebugCase(
            string name,
            int debugMode,
            string family)
        {
            cases.Add(new RenderCase
            {
                Name = name,
                MeshSubject = suspect,
                PropertySubject = suspect,
                SourceMaterial = suspect.Material,
                MaterialRole = "Suspect",
                Family = family,
                MaskDebugMode = debugMode
            });
        }

        private void Dispatch(RenderCase renderCase)
        {
            ReleasePendingRenderResources();
            pendingCase = renderCase;
            pendingTexture = new RenderTexture(
                CaptureSize,
                CaptureSize,
                24,
                renderCase.IsTriangleIdentity
                    ? RenderTextureFormat.ARGB32
                    : RenderTextureFormat.ARGBFloat,
                RenderTextureReadWrite.Linear)
            {
                name = "GeneratedMassSurfaceCausality_" + renderCase.Name,
                antiAliasing = 1,
                useMipMap = false,
                autoGenerateMips = false,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            pendingTexture.Create();

            List<RendererState> rendererStates = null;
            LightOverrideSession lightSession = null;
            EnvironmentState environmentState = default;
            bool environmentOverridden = false;
            SceneDirtySnapshot dirtySnapshot = CaptureSceneDirtySnapshot();
            try
            {
                if (!renderCase.IsTriangleIdentity)
                {
                    rendererStates = requiresRendererSuppression
                        ? SuppressSceneRenderers()
                        : SuppressSourceSubjectRenderers();
                    lightSession = ApplyLightOverrides(renderCase);
                    environmentState = ApplyEnvironmentOverrides(renderCase);
                    environmentOverridden = true;
                }
                pendingRenderObject = CreateRenderObject(
                    renderCase,
                    out pendingMaterial,
                    out pendingTemporaryMesh);
                Camera camera = CreateAuditCamera(
                    renderCase,
                    pendingTexture,
                    out pendingCameraObject);
                pendingLocalToClip =
                    GL.GetGPUProjectionMatrix(camera.projectionMatrix, true) *
                    camera.worldToCameraMatrix *
                    pendingRenderObject.transform.localToWorldMatrix;
                pendingCameraPosition = camera.transform.position;
                if (renderCase.IsTriangleIdentity)
                {
                    RenderTriangleIdentityDirect(
                        camera,
                        pendingTexture,
                        pendingTemporaryMesh,
                        pendingMaterial,
                        pendingRenderObject.transform.localToWorldMatrix);
                }
                else
                {
                    camera.Render();
                }
                pendingRequest = AsyncGPUReadback.Request(
                    pendingTexture,
                    0,
                    renderCase.IsTriangleIdentity
                        ? TextureFormat.RGBA32
                        : TextureFormat.RGBAFloat);
                waitingForReadback = true;
            }
            catch
            {
                ReleasePendingRenderResources();
                throw;
            }
            finally
            {
                if (environmentOverridden)
                {
                    RestoreEnvironment(environmentState);
                }
                RestoreLights(lightSession);
                RestoreSceneRenderers(rendererStates);
                RestoreSceneDirtySnapshot(dirtySnapshot);
            }
        }

        private void RenderTriangleIdentityDirect(
            Camera camera,
            RenderTexture target,
            Mesh mesh,
            Material material,
            Matrix4x4 localToWorld)
        {
            if (camera == null || target == null || mesh == null ||
                material == null || triangleIdentityShaderPassIndex < 0)
            {
                throw new InvalidOperationException(
                    "Triangle identity direct-render contract is incomplete.");
            }
            CommandBuffer command = new CommandBuffer
            {
                name = "Generated Mass Triangle Identity Direct Draw"
            };
            try
            {
                Matrix4x4 gpuProjection = GL.GetGPUProjectionMatrix(
                    camera.projectionMatrix,
                    true);
                command.SetRenderTarget(target);
                command.SetViewport(new Rect(0f, 0f, target.width, target.height));
                command.DisableScissorRect();
                command.ClearRenderTarget(
                    true,
                    true,
                    new Color(0f, 0f, 0f, 0f));
                command.SetViewProjectionMatrices(
                    camera.worldToCameraMatrix,
                    gpuProjection);
                command.DrawMesh(
                    mesh,
                    localToWorld,
                    material,
                    0,
                    triangleIdentityShaderPassIndex);
                Graphics.ExecuteCommandBuffer(command);
            }
            finally
            {
                command.Release();
            }
        }

        private void ReleasePendingRenderResources()
        {
            if (pendingCameraObject != null)
            {
                UnityEngine.Object.DestroyImmediate(pendingCameraObject);
                pendingCameraObject = null;
            }
            if (pendingRenderObject != null)
            {
                UnityEngine.Object.DestroyImmediate(pendingRenderObject);
                pendingRenderObject = null;
            }
            if (pendingMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(pendingMaterial);
                pendingMaterial = null;
            }
            if (pendingTemporaryMesh != null)
            {
                UnityEngine.Object.DestroyImmediate(pendingTemporaryMesh);
                pendingTemporaryMesh = null;
            }
            if (pendingTexture != null)
            {
                pendingTexture.Release();
                UnityEngine.Object.DestroyImmediate(pendingTexture);
                pendingTexture = null;
            }
        }

        private void CompleteReadback()
        {
            CaseResult result = new CaseResult
            {
                Name = pendingCase.Name,
                MeshRole = pendingCase.MeshSubject.Role,
                MaterialRole = pendingCase.MaterialRole,
                Family = pendingCase.Family,
                PropertyBlockMode = pendingCase.ClearPropertyBlock
                    ? "Cleared"
                    : "Preserved",
                MaskClass = pendingCase.MaskClass,
                HighIntensity = pendingCase.HighIntensity,
                CausalityMode = pendingCase.CausalityMode,
                MaskDebugMode = pendingCase.MaskDebugMode,
                IsAblation = pendingCase.IsAblation,
                IsTriangleIdentity = pendingCase.IsTriangleIdentity,
                IsBrdfSweep = pendingCase.IsBrdfSweep,
                IsAdaptiveBrdf = pendingCase.IsAdaptiveBrdf,
                IsAuxiliaryIdentity = pendingCase.IsAuxiliaryIdentity,
                CountsTowardDecisionTotal =
                    pendingCase.CountsTowardDecisionTotal,
                DirectionName = pendingCase.DirectionName,
                BrdfVariant = pendingCase.BrdfVariant,
                ViewName = pendingCase.ViewName,
                CameraAzimuthDegrees = pendingCase.CameraAzimuthDegrees,
                LightDirectionLocal = pendingCase.LightDirectionLocal,
                CameraPositionWorld = pendingCameraPosition,
                IsLambertPreflight = pendingCase.IsLambertPreflight,
                IsLambertNormalCapture = pendingCase.IsLambertNormalCapture,
                IsOrientationSweep = pendingCase.IsOrientationSweep,
                OrientationKind = pendingCase.OrientationKind,
                OrientationStage = pendingCase.OrientationStage,
                OrientationAblation = pendingCase.OrientationAblation,
                LocalToClip = pendingLocalToClip
            };

            if (pendingRequest.hasError)
            {
                result.ReadbackError = true;
                result.Error = "AsyncGPUReadback failed";
                fatalIdentityContractFailure = true;
                fatalContractReason = result.Error;
            }
            else if (!result.ReadbackError)
            {
                if (pendingCase.IsTriangleIdentity)
                {
                    NativeArray<Color32> identityData =
                        pendingRequest.GetData<Color32>();
                    result.Pixels = identityData.ToArray();
                    result.TriangleIdentityContractValid =
                        TryValidateTriangleIdentity(
                            pendingCase.MeshSubject,
                            result.Pixels,
                            out Color32[] wholeMask,
                            out result.TriangleIdentityPixelCount,
                            out result.TriangleIdentityInvalidPixelCount,
                            out result.TriangleIdentityDistinctTriangleCount,
                            out result.TriangleIdentityForegroundWidth,
                            out result.TriangleIdentityForegroundHeight,
                            out result.TriangleIdentityCpuRoundTripFailures);
                    if (!result.TriangleIdentityContractValid)
                    {
                        fatalIdentityContractFailure = true;
                        result.ReadbackError = true;
                        result.Error =
                            "triangle identity contract failed: validPixels=" +
                            result.TriangleIdentityPixelCount +
                            ",invalidPixels=" +
                            result.TriangleIdentityInvalidPixelCount +
                            ",distinctTriangles=" +
                            result.TriangleIdentityDistinctTriangleCount +
                            ",foreground=" +
                            result.TriangleIdentityForegroundWidth + "x" +
                            result.TriangleIdentityForegroundHeight +
                            ",cpuRoundTripFailures=" +
                            result.TriangleIdentityCpuRoundTripFailures;
                        fatalContractReason = result.Error;
                    }
                    else
                    {
                        triangleIdentityPixels[IdentityKey(
                            pendingCase.MeshSubject,
                            pendingCase.ViewName)] = result.Pixels;
                        masks[MaskKey(
                            pendingCase.MeshSubject,
                            SurfaceClass.WholeObject,
                            pendingCase.ViewName)] = wholeMask;
                    }
                    result.Pixels = Array.Empty<Color32>();
                }
                else
                {
                    NativeArray<Color> lightingData =
                        pendingRequest.GetData<Color>();
                    result.LinearPixels = lightingData.ToArray();
                    result.NonFinitePixelCount =
                        CountNonFinitePixels(result.LinearPixels);
                    if (result.NonFinitePixelCount > 0)
                    {
                        result.ReadbackError = true;
                        result.Error =
                            "non-finite floating-point lighting pixels: " +
                            result.NonFinitePixelCount;
                        fatalIdentityContractFailure = true;
                        fatalContractReason = result.Error;
                    }
                    result.Pixels = ConvertLinearPixelsToColor32(
                        result.LinearPixels);
                }
                if (!pendingCase.IsTriangleIdentity &&
                    !result.ReadbackError &&
                    triangleIdentityPixels.TryGetValue(
                        IdentityKey(
                            pendingCase.MeshSubject,
                            pendingCase.ViewName),
                        out Color32[] identityPixels))
                {
                    if (!TryResolveForegroundAlignment(
                            identityPixels,
                            result.LinearPixels,
                            out bool identityFlipRelativeToLighting,
                            out float foregroundIoU,
                            out float foregroundPixelDifferenceRatio,
                            out int lightingForegroundPixels,
                            out int identityForegroundPixels))
                    {
                        result.ReadbackError = true;
                        result.Error =
                            "identity-to-lighting alignment contract failed: " +
                            "iou=" + Format(foregroundIoU) +
                            ",pixelDifferenceRatio=" +
                            Format(foregroundPixelDifferenceRatio) +
                            ",lightingPixels=" + lightingForegroundPixels +
                            ",identityPixels=" + identityForegroundPixels;
                        fatalIdentityContractFailure = true;
                        fatalContractReason = result.Error;
                    }
                    else
                    {
                        result.IdentityFlipRelativeToLighting =
                            identityFlipRelativeToLighting;
                        result.ForegroundAlignmentIoU = foregroundIoU;
                        result.ForegroundPixelCountDifferenceRatio =
                            foregroundPixelDifferenceRatio;
                        result.LightingForegroundPixelCount =
                            lightingForegroundPixels;
                        result.IdentityForegroundPixelCount =
                            identityForegroundPixels;
                        if (pendingCase.IsLambertNormalCapture)
                        {
                            lambertStoredNormalPixels = result.LinearPixels;
                            lambertStoredNormalIdentityFlipRelativeToLighting =
                                identityFlipRelativeToLighting;
                        }
                        Color32[] alignedMask = BuildAlignedIdentityMask(
                            identityPixels,
                            identityFlipRelativeToLighting);
                        FacetScore score = ScoreFaceting(
                            pendingCase.MeshSubject,
                            result.Pixels,
                            alignedMask,
                            pendingLocalToClip,
                            pendingCameraPosition);
                        bool projectionFlipY = score.ValidSamples > 0
                            ? score.FlipY
                            : ResolveSampleFlipY(
                                pendingCase.MeshSubject,
                                alignedMask,
                                identityPixels,
                                identityFlipRelativeToLighting,
                                pendingLocalToClip);
                        result.ValidFacetSamples = score.ValidSamples;
                        result.TotalInternalEdges = score.TotalEdges;
                        result.FrontFacingEdges = score.FrontFacingEdges;
                        result.ProjectedEdges = score.ProjectedEdges;
                        result.UsedFlippedReadback = projectionFlipY;
                        result.MeanGradientJump = score.MeanGradientJump;
                        result.P90GradientJump = score.P90GradientJump;
                        result.MaximumGradientJump = score.MaximumGradientJump;
                        result.MeanValueStep = score.MeanValueStep;
                        result.MeanRawGradientJump = score.MeanRawGradientJump;
                        result.P90RawGradientJump = score.P90RawGradientJump;
                        result.MeanColorGradientJump = score.MeanColorGradientJump;
                        result.P90ColorGradientJump = score.P90ColorGradientJump;
                        result.FacetScore = score.Score;
                        CalculateMaskedStatistics(
                            result.LinearPixels,
                            alignedMask,
                            false,
                            out result.MeanMaskedLuma,
                            out result.SaturatedMaskedPixelFraction);
                        if (pendingCase.IsOrientationSweep)
                        {
                            CaptureOrientationPixelEvidence(
                                pendingCase,
                                result);
                        }
                        if (!result.ReadbackError &&
                            (pendingCase.IsBrdfSweep ||
                             pendingCase.IsOrientationSweep))
                        {
                            CalculateTriangleStatistics(
                                pendingCase.MeshSubject,
                                result,
                                identityFlipRelativeToLighting);
                            int eligibleIdentityTriangles =
                                CountEligibleIdentityTriangles(
                                    pendingCase.MeshSubject,
                                    pendingCase.ViewName);
                            result.TriangleCoverageRatio =
                                eligibleIdentityTriangles > 0
                                    ? result.TriangleStatistics.Count /
                                        (float)eligibleIdentityTriangles
                                    : 0f;
                            if (result.TriangleCoverageRatio <
                                MinimumTriangleCoverageRatio)
                            {
                                result.ReadbackError = true;
                                result.Error =
                                    "per-triangle coverage contract failed: " +
                                    result.TriangleStatistics.Count + "/" +
                                    eligibleIdentityTriangles + "=" +
                                    Format(result.TriangleCoverageRatio);
                                fatalIdentityContractFailure = true;
                                fatalContractReason = result.Error;
                            }
                        }
                        if (!result.ReadbackError &&
                            pendingCase.IsLambertPreflight)
                        {
                            EvaluateLambertPreflight(result);
                            if (!result.LambertContractValid)
                            {
                                result.ReadbackError = true;
                                result.Error =
                                    "Lambert preflight contract failed: " +
                                    "diagnosis=" +
                                    ResolveLambertFailureDiagnosis(result) +
                                    ",validNormalPixels=" +
                                    result.LambertValidNormalPixelCount +
                                    ",positiveExpectedPixels=" +
                                    result.LambertPositiveExpectedPixelCount +
                                    ",positiveObservedPixels=" +
                                    result.LambertPositiveObservedPixelCount +
                                    ",meanForegroundLuma=" +
                                    Format(result.LambertMeanForegroundLuma) +
                                    ",configuredNormalizedRmse=" +
                                    Format(result.LambertConfiguredNormalizedRmse) +
                                    ",oppositeNormalizedRmse=" +
                                    Format(result.LambertOppositeNormalizedRmse) +
                                    ",bestFitScale=" +
                                    Format(result.LambertBestFitScale) +
                                    ",bestFitNormalizedRmse=" +
                                    Format(result.LambertBestFitNormalizedRmse);
                                fatalIdentityContractFailure = true;
                                fatalContractReason = result.Error;
                            }
                        }
                        if (!result.ReadbackError)
                        {
                            CalculateSurfaceClassStatistics(
                                pendingCase.MeshSubject,
                                result,
                                identityFlipRelativeToLighting);
                            CalculateBevelParentSamples(
                                pendingCase.MeshSubject,
                                result,
                                alignedMask,
                                projectionFlipY,
                                identityFlipRelativeToLighting);
                        }
                        result.Pixels = Array.Empty<Color32>();
                        result.LinearPixels = Array.Empty<Color>();
                    }
                }
                else if (!pendingCase.IsTriangleIdentity &&
                         !result.ReadbackError)
                {
                    result.ReadbackError = true;
                    result.Error =
                        "triangle identity pixels were unavailable for view " +
                        pendingCase.ViewName;
                    fatalIdentityContractFailure = true;
                    fatalContractReason = result.Error;
                }
            }

            if (result.ReadbackError)
            {
                result.Pixels = Array.Empty<Color32>();
                result.LinearPixels = Array.Empty<Color>();
            }
            results.Add(result);
            waitingForReadback = false;
            ReleasePendingRenderResources();
            if (IsComplete)
            {
                summary = BuildSummary();
            }
        }
        private Summary BuildSummary()
        {
            List<CaseResult> lightingResults = results
                .Where(item => !item.IsTriangleIdentity)
                .ToList();
            Summary value = new Summary
            {
                LegacyControlAvailable = legacyMaterial != null,
                LegacyMaterialName = legacyMaterial == null
                    ? "<missing>"
                    : legacyMaterial.name,
                LegacyShaderName = legacyMaterial == null ||
                    legacyMaterial.shader == null
                        ? "<missing>"
                        : legacyMaterial.shader.name,
                ExpectedDecisionCases = TotalCases,
                CompletedDecisionCases = CompletedCases,
                ExpectedOrientationCases = OrientationCaseCount,
                CompletedOrientationCases = results.Count(item =>
                    item.IsOrientationSweep),
                AuxiliaryIdentityCases = AuxiliaryIdentityCases,
                AuxiliaryValidationCases = AuxiliaryValidationCases,
                ReadbackErrorCount = results.Count(item => item.ReadbackError),
                MinimumCaseCoverageRatio = results
                    .Where(item => item.IsBrdfSweep || item.IsOrientationSweep)
                    .Select(item => item.TriangleCoverageRatio)
                    .DefaultIfEmpty(0f)
                    .Min(),
                MinimumForegroundAlignmentIoU = lightingResults
                    .Select(item => item.ForegroundAlignmentIoU)
                    .DefaultIfEmpty(0f)
                    .Min(),
                MaximumForegroundPixelCountDifferenceRatio = lightingResults
                    .Select(item => item.ForegroundPixelCountDifferenceRatio)
                    .DefaultIfEmpty(1f)
                    .Max()
            };

            CaseResult lambert = results.FirstOrDefault(item =>
                item.IsLambertPreflight);
            if (lambert != null)
            {
                value.LambertContractValid = lambert.LambertContractValid;
                value.LambertValidNormalPixelCount =
                    lambert.LambertValidNormalPixelCount;
                value.LambertPositiveExpectedPixelCount =
                    lambert.LambertPositiveExpectedPixelCount;
                value.LambertPositiveObservedPixelCount =
                    lambert.LambertPositiveObservedPixelCount;
                value.LambertConfiguredNormalizedRmse =
                    lambert.LambertConfiguredNormalizedRmse;
                value.LambertOppositeNormalizedRmse =
                    lambert.LambertOppositeNormalizedRmse;
                value.LambertBestFitScale = lambert.LambertBestFitScale;
                value.LambertBestFitNormalizedRmse =
                    lambert.LambertBestFitNormalizedRmse;
                value.LambertMeanForegroundLuma =
                    lambert.LambertMeanForegroundLuma;
            }

            if (fatalIdentityContractFailure)
            {
                value.CompletenessFailure = string.IsNullOrEmpty(
                    fatalContractReason)
                        ? "VALIDATION_PREFLIGHT_FAILURE"
                        : fatalContractReason;
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }

            CaseResult currentIdentity = results.FirstOrDefault(item =>
                item.IsTriangleIdentity &&
                string.Equals(item.ViewName, "CURRENT",
                    StringComparison.Ordinal));
            int expectedIdentityCases = AlternateViews.Length + 1;
            if (currentIdentity == null ||
                !currentIdentity.TriangleIdentityContractValid)
            {
                value.CompletenessFailure =
                    "TRIANGLE_IDENTITY_CONTRACT_FAILURE";
            }
            else if (CompletedCases != TotalCases)
            {
                value.CompletenessFailure =
                    "CASE_COMPLETENESS_FAILURE:" + CompletedCases + "/" +
                    TotalCases;
            }
            else if (AuxiliaryIdentityCases != expectedIdentityCases ||
                     results.Any(item =>
                         item.IsTriangleIdentity &&
                         !item.TriangleIdentityContractValid))
            {
                value.CompletenessFailure =
                    "IDENTITY_COMPLETENESS_FAILURE:" +
                    AuxiliaryIdentityCases + "/" + expectedIdentityCases;
            }
            else if (AuxiliaryValidationCases != expectedIdentityCases + 1 ||
                     results.Count(item => item.IsLambertNormalCapture) != 1)
            {
                value.CompletenessFailure =
                    "AUXILIARY_VALIDATION_COMPLETENESS_FAILURE:" +
                    AuxiliaryValidationCases + "/" +
                    (expectedIdentityCases + 1);
            }
            else if (results.Count != TotalRenderPasses)
            {
                value.CompletenessFailure =
                    "TOTAL_RENDER_PASS_COMPLETENESS_FAILURE:" +
                    results.Count + "/" + TotalRenderPasses;
            }
            else if (value.ReadbackErrorCount > 0)
            {
                value.CompletenessFailure =
                    "READBACK_FAILURE_COUNT:" + value.ReadbackErrorCount;
            }
            else if (lambert == null || !lambert.LambertContractValid)
            {
                value.CompletenessFailure =
                    "LAMBERT_PREFLIGHT_CONTRACT_FAILURE";
            }
            else if (lightingResults.Any(item =>
                item.ForegroundAlignmentIoU < MinimumForegroundAlignmentIoU ||
                item.ForegroundPixelCountDifferenceRatio >
                    MaximumForegroundPixelCountDifferenceRatio))
            {
                value.CompletenessFailure =
                    "IDENTITY_LIGHTING_ALIGNMENT_CONTRACT_FAILURE";
            }
            else if (results.Any(item =>
                (item.IsBrdfSweep || item.IsOrientationSweep) &&
                !CaseCoverageValid(item)))
            {
                value.CompletenessFailure =
                    "PER_TRIANGLE_COVERAGE_CONTRACT_FAILURE";
            }
            else if (value.CompletedOrientationCases !=
                     value.ExpectedOrientationCases)
            {
                value.CompletenessFailure =
                    "ORIENTATION_CASE_COMPLETENESS_FAILURE:" +
                    value.CompletedOrientationCases + "/" +
                    value.ExpectedOrientationCases;
            }
            else if (results.Count(item => item.Family ==
                         "LambertPreflight") != 1 ||
                     results.Count(item => item.Family == "StageA") !=
                         brdfDirections.Length * 6 ||
                     results.Count(item => item.Family == "StageB") !=
                         StageBDirectionCount * 5 ||
                     results.Count(item => item.Family == "StageC") !=
                         StageCDirectionCount * AlternateViews.Length * 5 ||
                     results.Count(item => item.Family == "StageD") != 6 ||
                     results.Count(item => item.Family == "StageE") !=
                         OrientationCaseCount)
            {
                value.CompletenessFailure =
                    "MATRIX_CASE_FAMILY_COUNT_FAILURE";
            }

            if (!string.IsNullOrEmpty(value.CompletenessFailure))
            {
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }

            List<float> stageACurrent = new();
            List<float> stageADielectric = new();
            List<float> stageADiffuse = new();
            int improvedDirections = 0;
            foreach (BrdfDirectionDefinition direction in brdfDirections)
            {
                CaseResult legacyFull = FindBrdfCase(
                    direction.Name, "A_LEGACY_NEUTRAL_FULL");
                CaseResult legacySpecular = FindBrdfCase(
                    direction.Name, "A_LEGACY_BLACK_SPECULAR");
                CaseResult currentFull = FindBrdfCase(
                    direction.Name, "A_HLSL016_NEUTRAL_STORED");
                CaseResult currentSpecular = FindBrdfCase(
                    direction.Name,
                    "A_HLSL016_BLACK_SPECULAR_STORED");
                CaseResult dielectricFull = FindBrdfCase(
                    direction.Name, "A_HLSL004_NEUTRAL_STORED");
                CaseResult dielectricSpecular = FindBrdfCase(
                    direction.Name,
                    "A_HLSL004_BLACK_SPECULAR_STORED");
                float currentResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacyFull, currentFull);
                float dielectricResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacyFull, dielectricFull);
                float diffuseResidual =
                    CalculateDerivedDiffuseMeanAbsoluteResidual(
                        legacyFull,
                        legacySpecular,
                        dielectricFull,
                        dielectricSpecular);
                bool evaluable =
                    DirectionCaseEvaluable(legacyFull, currentFull) &&
                    CaseCoverageValid(legacySpecular) &&
                    CaseCoverageValid(dielectricFull) &&
                    CaseCoverageValid(currentSpecular) &&
                    CaseCoverageValid(dielectricSpecular) &&
                    currentResidual >= 0f &&
                    dielectricResidual >= 0f &&
                    diffuseResidual >= 0f;
                BrdfDirectionSummary directionSummary =
                    Build5NDirectionSummary(
                        direction,
                        legacyFull,
                        currentFull,
                        dielectricFull,
                        diffuseResidual);
                directionSummary.IsEvaluable = evaluable;
                value.BrdfDirections.Add(directionSummary);
                if (!evaluable)
                {
                    continue;
                }
                stageACurrent.Add(currentResidual);
                stageADielectric.Add(dielectricResidual);
                stageADiffuse.Add(diffuseResidual);
                if (dielectricResidual < currentResidual)
                {
                    improvedDirections++;
                }
            }

            value.StageAEvaluableDirectionCount = stageACurrent.Count;
            if (value.StageAEvaluableDirectionCount <
                MinimumEvaluableStageADirections)
            {
                value.CompletenessFailure =
                    "STAGE_A_EVALUABLE_DIRECTION_FAILURE:" +
                    value.StageAEvaluableDirectionCount + "/" +
                    MinimumEvaluableStageADirections;
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }

            float stageACurrentMean = stageACurrent.Average();
            float stageADielectricMean = stageADielectric.Average();
            value.NeutralDiffuseMeanAbsoluteResidual =
                stageADiffuse.Average();
            value.StageAF0ResidualReduction = CalculateMismatchReduction(
                stageACurrentMean, stageADielectricMean);
            value.StageAF0ImprovedDirectionCount = improvedDirections;
            value.BrdfSweepAvailable = true;
            value.BrdfComparedDirections =
                value.StageAEvaluableDirectionCount;
            value.BrdfCurrentMeanAbsoluteResidual = stageACurrentMean;
            value.BrdfDielectricMeanAbsoluteResidual = stageADielectricMean;
            value.BrdfDielectricResidualReduction =
                value.StageAF0ResidualReduction;
            value.BrdfDielectricImprovedDirectionCount = improvedDirections;
            List<BrdfDirectionSummary> evaluableDirections = value.BrdfDirections
                .Where(item => item.IsEvaluable)
                .ToList();
            value.BrdfWorstDirection = evaluableDirections
                .OrderByDescending(item => item.CurrentMeanAbsoluteResidual)
                .ThenBy(item => item.DirectionName, StringComparer.Ordinal)
                .First().DirectionName;
            value.BrdfCurrentOverResponseCount = evaluableDirections.Sum(
                item => item.CurrentOverResponseCount);
            value.BrdfCurrentUnderResponseCount = evaluableDirections.Sum(
                item => item.CurrentUnderResponseCount);
            value.BrdfDielectricOverResponseCount = evaluableDirections.Sum(
                item => item.DielectricOverResponseCount);
            value.BrdfDielectricUnderResponseCount = evaluableDirections.Sum(
                item => item.DielectricUnderResponseCount);
            value.BrdfCurrentOrderingInversionCount = evaluableDirections.Sum(
                item => item.CurrentOrderingInversionCount);
            value.BrdfDielectricOrderingInversionCount =
                evaluableDirections.Sum(
                    item => item.DielectricOrderingInversionCount);

            List<float> stageBStoredF0Reduction = new();
            List<float> stageBNormalReduction = new();
            List<float> stageBActualStoredDielectric = new();
            bool stageBResidualInvalid = false;
            foreach (string directionName in stageBDirections)
            {
                CaseResult legacy = FindBrdfCase(
                    directionName, "B_LEGACY_ACTUAL_FULL");
                CaseResult currentGenerated = FindBrdfCase(
                    directionName, "B_HLSL016_ACTUAL_GENERATED");
                CaseResult currentStored = FindBrdfCase(
                    directionName, "B_HLSL016_ACTUAL_STORED");
                CaseResult dielectricGenerated = FindBrdfCase(
                    directionName, "B_HLSL004_ACTUAL_GENERATED");
                CaseResult dielectricStored = FindBrdfCase(
                    directionName, "B_HLSL004_ACTUAL_STORED");
                float currentGeneratedResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacy, currentGenerated);
                float currentStoredResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacy, currentStored);
                float dielectricGeneratedResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacy, dielectricGenerated);
                float dielectricStoredResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacy, dielectricStored);
                stageBStoredF0Reduction.Add(CalculateMismatchReduction(
                    currentStoredResidual, dielectricStoredResidual));
                stageBNormalReduction.Add(Mathf.Max(
                    CalculateMismatchReduction(
                        currentGeneratedResidual, currentStoredResidual),
                    CalculateMismatchReduction(
                        dielectricGeneratedResidual,
                        dielectricStoredResidual)));
                stageBActualStoredDielectric.Add(
                    dielectricStoredResidual);
                stageBResidualInvalid |=
                    currentGeneratedResidual < 0f ||
                    currentStoredResidual < 0f ||
                    dielectricGeneratedResidual < 0f ||
                    dielectricStoredResidual < 0f;
            }
            if (stageBResidualInvalid ||
                stageBStoredF0Reduction.Count != StageBDirectionCount ||
                stageBNormalReduction.Count != StageBDirectionCount ||
                stageBActualStoredDielectric.Count != StageBDirectionCount)
            {
                value.CompletenessFailure =
                    "STAGE_B_RESIDUAL_CONTRACT_FAILURE";
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }
            value.StageBStoredF0MinimumReduction =
                stageBStoredF0Reduction.Min();
            value.StageBGeneratedNormalMeanReduction =
                stageBNormalReduction.Average();
            value.StageBActualStoredDielectricMeanAbsoluteResidual =
                stageBActualStoredDielectric.Average();
            value.BrdfAdaptiveDirectionCount = StageBDirectionCount;

            List<float> stageCF0 = new();
            List<float> stageCNormal = new();
            bool stageCResidualInvalid = false;
            foreach (string directionName in stageCDirections)
            {
                foreach (ViewDefinition view in AlternateViews)
                {
                    CaseResult legacy = FindBrdfCase(
                        directionName, "C_LEGACY_ACTUAL_FULL", view.Name);
                    CaseResult currentGenerated = FindBrdfCase(
                        directionName,
                        "C_HLSL016_ACTUAL_GENERATED", view.Name);
                    CaseResult currentStored = FindBrdfCase(
                        directionName,
                        "C_HLSL016_ACTUAL_STORED", view.Name);
                    CaseResult dielectricGenerated = FindBrdfCase(
                        directionName,
                        "C_HLSL004_ACTUAL_GENERATED", view.Name);
                    CaseResult dielectricStored = FindBrdfCase(
                        directionName,
                        "C_HLSL004_ACTUAL_STORED", view.Name);
                    float currentGeneratedResidual =
                        CalculateCaseRgbMeanAbsoluteResidual(
                            legacy, currentGenerated);
                    float currentStoredResidual =
                        CalculateCaseRgbMeanAbsoluteResidual(
                            legacy, currentStored);
                    float dielectricGeneratedResidual =
                        CalculateCaseRgbMeanAbsoluteResidual(
                            legacy, dielectricGenerated);
                    float dielectricStoredResidual =
                        CalculateCaseRgbMeanAbsoluteResidual(
                            legacy, dielectricStored);
                    stageCF0.Add(CalculateMismatchReduction(
                        currentGeneratedResidual,
                        dielectricGeneratedResidual));
                    stageCNormal.Add(Mathf.Max(
                        CalculateMismatchReduction(
                            currentGeneratedResidual, currentStoredResidual),
                        CalculateMismatchReduction(
                            dielectricGeneratedResidual,
                            dielectricStoredResidual)));
                    stageCResidualInvalid |=
                        currentGeneratedResidual < 0f ||
                        currentStoredResidual < 0f ||
                        dielectricGeneratedResidual < 0f ||
                        dielectricStoredResidual < 0f;
                }
            }
            int expectedStageCComparisons =
                StageCDirectionCount * AlternateViews.Length;
            if (stageCResidualInvalid ||
                stageCF0.Count != expectedStageCComparisons ||
                stageCNormal.Count != expectedStageCComparisons)
            {
                value.CompletenessFailure =
                    "STAGE_C_RESIDUAL_CONTRACT_FAILURE";
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }
            value.StageCMinimumF0Reduction = stageCF0.Min();
            value.StageCGeneratedNormalMeanReduction =
                stageCNormal.Average();

            CaseResult legacyIndirect = FindBrdfCase(
                "SCENE", "D_LEGACY_INDIRECT_ONLY");
            CaseResult currentIndirect = FindBrdfCase(
                "SCENE", "D_HLSL016_INDIRECT_ONLY");
            CaseResult dielectricIndirect = FindBrdfCase(
                "SCENE", "D_HLSL004_INDIRECT_ONLY");
            CaseResult legacyScene = FindBrdfCase(
                "SCENE", "D_LEGACY_ACTUAL_SCENE");
            CaseResult currentScene = FindBrdfCase(
                "SCENE", "D_HLSL016_ACTUAL_SCENE");
            CaseResult dielectricScene = FindBrdfCase(
                "SCENE", "D_HLSL004_ACTUAL_SCENE");
            value.IndirectCurrentMeanAbsoluteResidual =
                CalculateCaseRgbMeanAbsoluteResidual(
                    legacyIndirect, currentIndirect);
            value.IndirectDielectricMeanAbsoluteResidual =
                CalculateCaseRgbMeanAbsoluteResidual(
                    legacyIndirect, dielectricIndirect);
            value.ActualSceneCurrentMeanAbsoluteResidual =
                CalculateCaseRgbMeanAbsoluteResidual(
                    legacyScene, currentScene);
            value.ActualSceneDielectricMeanAbsoluteResidual =
                CalculateCaseRgbMeanAbsoluteResidual(
                    legacyScene, dielectricScene);
            if (value.IndirectCurrentMeanAbsoluteResidual < 0f ||
                value.IndirectDielectricMeanAbsoluteResidual < 0f ||
                value.ActualSceneCurrentMeanAbsoluteResidual < 0f ||
                value.ActualSceneDielectricMeanAbsoluteResidual < 0f)
            {
                value.CompletenessFailure =
                    "STAGE_D_RESIDUAL_CONTRACT_FAILURE";
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }

            PopulateOrientationSummary(value);
            if (!value.OrientationCaptureAvailable)
            {
                value.CompletenessFailure =
                    "ORIENTATION_ANALYSIS_CONTRACT_FAILURE";
                value.Ownership = value.CompletenessFailure;
                value.SurfaceLightingOwnership = value.Ownership;
                value.BrdfWorkflowVerdict = value.Ownership;
                return value;
            }

            // GM-SURFACE.5P: these aggregate ownership labels are diagnostic
            // contributors, not the acceptance criterion for the visible defect.
            // A parameter can reduce mean RGB error and still leave individual
            // source/bevel surfaces ordered incorrectly for their orientation.
            // Parent/bevel ordering evidence and per-surface light response remain
            // mandatory before any production fix may be called successful.
            bool brdfPrimary =
                value.NeutralDiffuseMeanAbsoluteResidual <= 0.05f &&
                value.StageAF0ResidualReduction >= 0.70f &&
                value.StageAF0ImprovedDirectionCount >=
                    Mathf.CeilToInt(
                        value.StageAEvaluableDirectionCount * 0.90f) &&
                value.StageBStoredF0MinimumReduction >= 0.50f &&
                value.StageCMinimumF0Reduction > 0f;
            bool normalPrimary =
                value.StageBGeneratedNormalMeanReduction >= 0.50f &&
                !brdfPrimary;
            bool mixed =
                value.StageBGeneratedNormalMeanReduction >= 0.35f &&
                value.StageBStoredF0MinimumReduction >= 0.35f;
            bool prelightPrimary =
                value.NeutralDiffuseMeanAbsoluteResidual <= 0.05f &&
                value.StageBActualStoredDielectricMeanAbsoluteResidual >
                    0.15f;
            bool controlledDirectClean =
                stageADielectricMean <= 0.05f &&
                value.StageBActualStoredDielectricMeanAbsoluteResidual <=
                    0.05f;
            bool environmentPrimary = controlledDirectClean &&
                (value.IndirectDielectricMeanAbsoluteResidual > 0.15f ||
                 value.ActualSceneDielectricMeanAbsoluteResidual > 0.15f);

            if (mixed)
            {
                value.Ownership = "MIXED_BRDF_AND_NORMAL";
            }
            else if (brdfPrimary)
            {
                value.Ownership = "BRDF_F0_016_PRIMARY";
            }
            else if (normalPrimary)
            {
                value.Ownership = "GENERATED_NORMAL_PRIMARY";
            }
            else if (prelightPrimary)
            {
                value.Ownership = "PRELIGHT_OR_ACTUAL_MATERIAL_PRIMARY";
            }
            else if (environmentPrimary)
            {
                value.Ownership =
                    "INDIRECT_OR_SCENE_ENVIRONMENT_PRIMARY";
            }
            else
            {
                value.Ownership =
                    "COMPLETE_MATRIX_NO_PRIMARY_THRESHOLD";
            }
            value.SurfaceLightingOwnership = value.Ownership;
            value.BrdfWorkflowVerdict = value.Ownership;
            value.OwnershipConfidence = 1f;
            return value;
        }

        private void PopulateOrientationSummary(Summary summaryValue)
        {
            List<CaseResult> orientationResults = results
                .Where(item => item.IsOrientationSweep)
                .ToList();
            if (orientationResults.Count != OrientationCaseCount)
            {
                return;
            }

            List<ViewDefinition> views = EnumerateOrientationViews().ToList();
            Dictionary<string, HashSet<string>> previousSourceInversions =
                new(StringComparer.Ordinal);
            Dictionary<string, HashSet<string>> previousBevelViolations =
                new(StringComparer.Ordinal);
            bool allStageCasesPresent = true;

            for (int stageIndex = 0;
                 stageIndex < OrientationStages.Length;
                 stageIndex++)
            {
                OrientationStageDefinition stage = OrientationStages[stageIndex];
                OrientationStageSummary stageSummary =
                    new OrientationStageSummary
                    {
                        StageName = stage.Name
                    };
                List<float> correlationNdotL = new();
                List<float> correlationLuma = new();
                List<float> directRatios = new();
                List<float> directProductRmses = new();

                foreach (ViewDefinition view in views)
                {
                    foreach (BrdfDirectionDefinition direction in brdfDirections)
                    {
                        CaseResult ndotl = FindOrientationCase(
                            view.Name,
                            direction.Name,
                            "NDOTL_ATTENUATION_STORED");
                        CaseResult direct = FindOrientationCase(
                            view.Name,
                            direction.Name,
                            "DIRECT_" + stage.Name);
                        if (ndotl == null || direct == null ||
                            !CaseCoverageValid(ndotl) ||
                            !CaseCoverageValid(direct))
                        {
                            allStageCasesPresent = false;
                            continue;
                        }

                        string context = view.Name + "|" + direction.Name;
                        EvaluateOrientationOrdering(
                            direct,
                            ndotl,
                            context,
                            out int sourceComparisons,
                            out HashSet<string> sourceInversions,
                            out int bevelComparisons,
                            out HashSet<string> bevelViolations,
                            correlationNdotL,
                            correlationLuma,
                            directRatios);
                        stageSummary.SourcePairComparisons += sourceComparisons;
                        stageSummary.SourcePairInversions +=
                            sourceInversions.Count;
                        stageSummary.ConditionalBevelComparisons +=
                            bevelComparisons;
                        stageSummary.ConditionalBevelEnvelopeViolations +=
                            bevelViolations.Count;
                        if (direct.OrientationDirectProductPixelCount > 0)
                        {
                            directProductRmses.Add(
                                direct.OrientationDirectProductNormalizedRmse);
                        }
                        else
                        {
                            allStageCasesPresent = false;
                        }

                        if (!previousSourceInversions.TryGetValue(
                                context,
                                out HashSet<string> previousSource))
                        {
                            previousSource = new HashSet<string>(
                                StringComparer.Ordinal);
                        }
                        if (!previousBevelViolations.TryGetValue(
                                context,
                                out HashSet<string> previousBevel))
                        {
                            previousBevel = new HashSet<string>(
                                StringComparer.Ordinal);
                        }
                        stageSummary.IntroducedSourcePairInversions +=
                            sourceInversions.Count(key =>
                                !previousSource.Contains(key));
                        stageSummary.IntroducedConditionalBevelViolations +=
                            bevelViolations.Count(key =>
                                !previousBevel.Contains(key));
                        previousSourceInversions[context] = sourceInversions;
                        previousBevelViolations[context] = bevelViolations;
                    }
                }

                stageSummary.SourceOrientationPearson =
                    Pearson(correlationNdotL, correlationLuma);
                stageSummary.SourceOrientationSpearman =
                    Spearman(correlationNdotL, correlationLuma);
                stageSummary.MeanDirectToNdotLRatio = directRatios.Count > 0
                    ? directRatios.Average()
                    : 0f;
                stageSummary.MeanDirectProductNormalizedRmse =
                    directProductRmses.Count > 0
                        ? directProductRmses.Average()
                        : 0f;
                PopulateOrientationStageMaskCorrelations(
                    stageIndex,
                    views,
                    stageSummary);
                summaryValue.OrientationStages.Add(stageSummary);
            }

            if (!allStageCasesPresent ||
                summaryValue.OrientationStages.Count !=
                    OrientationStages.Length)
            {
                return;
            }

            OrientationStageSummary firstDivergent =
                summaryValue.OrientationStages.FirstOrDefault(item =>
                    item.IntroducedSourcePairInversions > 0 ||
                    item.IntroducedConditionalBevelViolations > 0);
            if (firstDivergent != null)
            {
                summaryValue.OrientationFirstDivergentStage =
                    firstDivergent.StageName;
                summaryValue.OrientationFirstDivergentStageCount =
                    firstDivergent.IntroducedSourcePairInversions +
                    firstDivergent.IntroducedConditionalBevelViolations;
            }

            int legacySourceInversions = 0;
            int legacyBevelViolations = 0;
            int hlslSourceInversions = 0;
            int hlslBevelViolations = 0;
            bool referenceCasesPresent = true;
            foreach (ViewDefinition view in views)
            {
                foreach (BrdfDirectionDefinition direction in brdfDirections)
                {
                    CaseResult ndotl = FindOrientationCase(
                        view.Name,
                        direction.Name,
                        "NDOTL_ATTENUATION_STORED");
                    CaseResult legacy = FindOrientationCase(
                        view.Name,
                        direction.Name,
                        "PBR_LEGACY_ACTUAL");
                    CaseResult hlsl = FindOrientationCase(
                        view.Name,
                        direction.Name,
                        "PBR_HLSL_STORED");
                    if (ndotl == null || legacy == null || hlsl == null)
                    {
                        referenceCasesPresent = false;
                        continue;
                    }
                    EvaluateOrientationOrdering(
                        legacy,
                        ndotl,
                        view.Name + "|" + direction.Name + "|LEGACY",
                        out _,
                        out HashSet<string> legacySources,
                        out _,
                        out HashSet<string> legacyBevels,
                        null,
                        null,
                        null);
                    EvaluateOrientationOrdering(
                        hlsl,
                        ndotl,
                        view.Name + "|" + direction.Name + "|HLSL",
                        out _,
                        out HashSet<string> hlslSources,
                        out _,
                        out HashSet<string> hlslBevels,
                        null,
                        null,
                        null);
                    legacySourceInversions += legacySources.Count;
                    legacyBevelViolations += legacyBevels.Count;
                    hlslSourceInversions += hlslSources.Count;
                    hlslBevelViolations += hlslBevels.Count;
                }
            }
            if (!referenceCasesPresent)
            {
                return;
            }
            summaryValue.OrientationLegacySourcePairInversions =
                legacySourceInversions;
            summaryValue.OrientationLegacyConditionalBevelViolations =
                legacyBevelViolations;
            summaryValue.OrientationHlslSourcePairInversions =
                hlslSourceInversions;
            summaryValue.OrientationHlslConditionalBevelViolations =
                hlslBevelViolations;

            int baselineSource = 0;
            int baselineBevel = 0;
            foreach (ViewDefinition view in views)
            {
                foreach (BrdfDirectionDefinition direction in brdfDirections)
                {
                    CaseResult ndotl = FindOrientationCase(
                        view.Name,
                        direction.Name,
                        "NDOTL_ATTENUATION_STORED");
                    CaseResult baseline = FindOrientationCase(
                        view.Name,
                        direction.Name,
                        "DIRECT_FINAL_WITH_OVERALL_TINT");
                    if (ndotl == null || baseline == null)
                    {
                        return;
                    }
                    EvaluateOrientationOrdering(
                        baseline,
                        ndotl,
                        view.Name + "|" + direction.Name + "|BASELINE",
                        out _,
                        out HashSet<string> sourceErrors,
                        out _,
                        out HashSet<string> bevelErrors,
                        null,
                        null,
                        null);
                    baselineSource += sourceErrors.Count;
                    baselineBevel += bevelErrors.Count;
                }
            }
            float baselineCombined = baselineSource + baselineBevel;

            foreach (string ablation in OrientationAblations)
            {
                OrientationAblationSummary ablationSummary =
                    new OrientationAblationSummary
                    {
                        AblationName = ablation
                    };
                bool ablationCasesPresent = true;
                foreach (ViewDefinition view in views)
                {
                    foreach (BrdfDirectionDefinition direction in brdfDirections)
                    {
                        CaseResult ndotl = FindOrientationCase(
                            view.Name,
                            direction.Name,
                            "NDOTL_ATTENUATION_STORED");
                        CaseResult ablated = FindOrientationCase(
                            view.Name,
                            direction.Name,
                            "DIRECT_ABLATE_" + ablation);
                        if (ndotl == null || ablated == null)
                        {
                            ablationCasesPresent = false;
                            continue;
                        }
                        EvaluateOrientationOrdering(
                            ablated,
                            ndotl,
                            view.Name + "|" + direction.Name + "|" + ablation,
                            out _,
                            out HashSet<string> sourceErrors,
                            out _,
                            out HashSet<string> bevelErrors,
                            null,
                            null,
                            null);
                        ablationSummary.SourcePairInversions +=
                            sourceErrors.Count;
                        ablationSummary.ConditionalBevelViolations +=
                            bevelErrors.Count;
                    }
                }
                if (!ablationCasesPresent)
                {
                    return;
                }
                ablationSummary.CombinedError =
                    ablationSummary.SourcePairInversions +
                    ablationSummary.ConditionalBevelViolations;
                ablationSummary.ReductionFromBaseline = baselineCombined > 0f
                    ? (baselineCombined - ablationSummary.CombinedError) /
                        baselineCombined
                    : 0f;
                summaryValue.OrientationAblations.Add(ablationSummary);
            }

            OrientationAblationSummary dominant = baselineCombined > 0f
                ? summaryValue.OrientationAblations
                    .OrderByDescending(item => item.ReductionFromBaseline)
                    .ThenBy(item => item.AblationName, StringComparer.Ordinal)
                    .FirstOrDefault()
                : null;
            if (dominant != null)
            {
                summaryValue.OrientationDominantAblation =
                    dominant.AblationName;
                summaryValue.OrientationDominantAblationReduction =
                    dominant.ReductionFromBaseline;
            }
            summaryValue.OrientationCaptureAvailable = true;
        }

        private CaseResult FindOrientationCase(
            string viewName,
            string directionName,
            string variant)
        {
            return results.FirstOrDefault(item =>
                item.IsOrientationSweep &&
                string.Equals(item.ViewName, viewName,
                    StringComparison.Ordinal) &&
                string.Equals(item.DirectionName, directionName,
                    StringComparison.Ordinal) &&
                string.Equals(item.BrdfVariant, variant,
                    StringComparison.Ordinal));
        }

        private void EvaluateOrientationOrdering(
            CaseResult response,
            CaseResult ndotl,
            string context,
            out int sourceComparisons,
            out HashSet<string> sourceInversions,
            out int bevelComparisons,
            out HashSet<string> bevelViolations,
            List<float> correlationNdotL,
            List<float> correlationLuma,
            List<float> directRatios)
        {
            sourceComparisons = 0;
            sourceInversions = new HashSet<string>(StringComparer.Ordinal);
            bevelComparisons = 0;
            bevelViolations = new HashSet<string>(StringComparer.Ordinal);

            List<OrientationSurfaceMean> sourceMeans =
                BuildOrientationSourceFaceMeans(response, ndotl);
            for (int a = 0; a < sourceMeans.Count; a++)
            {
                OrientationSurfaceMean first = sourceMeans[a];
                if (correlationNdotL != null && correlationLuma != null)
                {
                    correlationNdotL.Add(first.NdotL);
                    correlationLuma.Add(first.Luma);
                }
                if (directRatios != null && first.NdotL > 0.05f)
                {
                    directRatios.Add(first.Luma / first.NdotL);
                }
                for (int b = a + 1; b < sourceMeans.Count; b++)
                {
                    OrientationSurfaceMean second = sourceMeans[b];
                    float ndotlDelta = first.NdotL - second.NdotL;
                    if (Mathf.Abs(ndotlDelta) <
                        OrientationMinimumNdotLSeparation)
                    {
                        continue;
                    }
                    sourceComparisons++;
                    float lumaDelta = first.Luma - second.Luma;
                    if (Mathf.Abs(lumaDelta) <=
                        OrientationOrderingLumaTolerance)
                    {
                        continue;
                    }
                    if (Mathf.Sign(ndotlDelta) != Mathf.Sign(lumaDelta))
                    {
                        int low = Mathf.Min(
                            first.ProvenanceIndex,
                            second.ProvenanceIndex);
                        int high = Mathf.Max(
                            first.ProvenanceIndex,
                            second.ProvenanceIndex);
                        sourceInversions.Add(
                            context + "|S|" + low + "|" + high);
                    }
                }
            }

            foreach (BevelParentGeometrySample sample in
                suspect.BevelParentSamples)
            {
                if (!TryGetTriangleValue(
                        ndotl,
                        sample.ParentATriangleIndex,
                        out float parentANdotL) ||
                    !TryGetTriangleValue(
                        ndotl,
                        sample.BevelTriangleIndex,
                        out float bevelNdotL) ||
                    !TryGetTriangleValue(
                        ndotl,
                        sample.ParentBTriangleIndex,
                        out float parentBNdotL) ||
                    !TryGetTriangleLuma(
                        response,
                        sample.ParentATriangleIndex,
                        out float parentALuma) ||
                    !TryGetTriangleLuma(
                        response,
                        sample.BevelTriangleIndex,
                        out float bevelLuma) ||
                    !TryGetTriangleLuma(
                        response,
                        sample.ParentBTriangleIndex,
                        out float parentBLuma))
                {
                    continue;
                }

                float minNdotL = Mathf.Min(parentANdotL, parentBNdotL) -
                    OrientationIntermediateNdotLTolerance;
                float maxNdotL = Mathf.Max(parentANdotL, parentBNdotL) +
                    OrientationIntermediateNdotLTolerance;
                if (bevelNdotL < minNdotL || bevelNdotL > maxNdotL)
                {
                    continue;
                }
                bevelComparisons++;
                float minLuma = Mathf.Min(parentALuma, parentBLuma) -
                    OrientationOrderingLumaTolerance;
                float maxLuma = Mathf.Max(parentALuma, parentBLuma) +
                    OrientationOrderingLumaTolerance;
                if (bevelLuma < minLuma || bevelLuma > maxLuma)
                {
                    bevelViolations.Add(
                        context + "|B|" + sample.LogicalBevelId + "|" +
                        sample.SampleIndex);
                }
            }
        }

        private List<OrientationSurfaceMean> BuildOrientationSourceFaceMeans(
            CaseResult response,
            CaseResult ndotl)
        {
            Dictionary<int, OrientationSurfaceAccumulator> accumulators = new();
            foreach (TriangleLuminanceStatistics responseTriangle in
                response.TriangleStatistics.Values)
            {
                if (responseTriangle.SurfaceClass != SurfaceClass.SourceFace ||
                    responseTriangle.ProvenanceIndex < 0 ||
                    !ndotl.TriangleStatistics.TryGetValue(
                        responseTriangle.TriangleIndex,
                        out TriangleLuminanceStatistics ndotlTriangle))
                {
                    continue;
                }
                int weight = Mathf.Max(1, responseTriangle.PixelCount);
                accumulators.TryGetValue(
                    responseTriangle.ProvenanceIndex,
                    out OrientationSurfaceAccumulator accumulator);
                accumulator.LumaSum += responseTriangle.MeanLuma * weight;
                accumulator.NdotLSum += ndotlTriangle.MeanLinearRgb.x * weight;
                accumulator.Weight += weight;
                accumulators[responseTriangle.ProvenanceIndex] = accumulator;
            }

            return accumulators
                .Where(pair => pair.Value.Weight > 0)
                .Select(pair => new OrientationSurfaceMean
                {
                    ProvenanceIndex = pair.Key,
                    NdotL = pair.Value.NdotLSum / pair.Value.Weight,
                    Luma = pair.Value.LumaSum / pair.Value.Weight
                })
                .OrderBy(item => item.ProvenanceIndex)
                .ToList();
        }

        private void PopulateOrientationStageMaskCorrelations(
            int stageIndex,
            IReadOnlyList<ViewDefinition> views,
            OrientationStageSummary stageSummary)
        {
            OrientationStageDefinition stage = OrientationStages[stageIndex];
            List<float> contributions = new();
            List<float> exposures = new();
            List<float> crevices = new();
            List<float> dirts = new();
            List<float> heights = new();
            List<float> mottles = new();

            foreach (ViewDefinition view in views)
            {
                CaseResult current = FindOrientationCase(
                    view.Name,
                    string.Empty,
                    "ALBEDO_" + stage.Name);
                CaseResult previous = stageIndex > 0
                    ? FindOrientationCase(
                        view.Name,
                        string.Empty,
                        "ALBEDO_" + OrientationStages[stageIndex - 1].Name)
                    : null;
                CaseResult rawMasks = FindOrientationCase(
                    view.Name,
                    string.Empty,
                    "RAW_VERTEX_MASKS");
                CaseResult dirtHeight = FindOrientationCase(
                    view.Name,
                    string.Empty,
                    "RAW_DIRT_HEIGHT_NORMALY");
                CaseResult resolvedC = FindOrientationCase(
                    view.Name,
                    string.Empty,
                    "RESOLVED_MASKS_C");
                if (current == null || rawMasks == null ||
                    dirtHeight == null || resolvedC == null)
                {
                    continue;
                }

                foreach (TriangleLuminanceStatistics triangle in
                    current.TriangleStatistics.Values)
                {
                    if (triangle.SurfaceClass != SurfaceClass.SourceFace ||
                        !rawMasks.TriangleStatistics.TryGetValue(
                            triangle.TriangleIndex,
                            out TriangleLuminanceStatistics raw) ||
                        !dirtHeight.TriangleStatistics.TryGetValue(
                            triangle.TriangleIndex,
                            out TriangleLuminanceStatistics dirt) ||
                        !resolvedC.TriangleStatistics.TryGetValue(
                            triangle.TriangleIndex,
                            out TriangleLuminanceStatistics resolved))
                    {
                        continue;
                    }
                    float previousLuma = OrientationCorrelationEpsilon;
                    if (previous != null &&
                        previous.TriangleStatistics.TryGetValue(
                            triangle.TriangleIndex,
                            out TriangleLuminanceStatistics previousTriangle))
                    {
                        previousLuma = Mathf.Max(
                            OrientationCorrelationEpsilon,
                            previousTriangle.MeanLuma);
                    }
                    float currentLuma = Mathf.Max(
                        OrientationCorrelationEpsilon,
                        triangle.MeanLuma);
                    float contribution = stageIndex == 0
                        ? Mathf.Log(currentLuma)
                        : Mathf.Log(currentLuma / previousLuma);
                    contributions.Add(contribution);
                    exposures.Add(raw.MeanLinearRgb.y);
                    crevices.Add(raw.MeanLinearRgb.z);
                    dirts.Add(dirt.MeanLinearRgb.x);
                    heights.Add(dirt.MeanLinearRgb.y);
                    mottles.Add(resolved.MeanLinearRgb.z);
                }
            }

            stageSummary.ExposureCorrelation = Pearson(exposures, contributions);
            stageSummary.CreviceCorrelation = Pearson(crevices, contributions);
            stageSummary.DirtCorrelation = Pearson(dirts, contributions);
            stageSummary.HeightCorrelation = Pearson(heights, contributions);
            stageSummary.MottleCorrelation = Pearson(mottles, contributions);
        }

        private static bool TryGetTriangleValue(
            CaseResult result,
            int triangleIndex,
            out float value)
        {
            value = 0f;
            if (result == null ||
                !result.TriangleStatistics.TryGetValue(
                    triangleIndex,
                    out TriangleLuminanceStatistics triangle))
            {
                return false;
            }
            value = triangle.MeanLinearRgb.x;
            return true;
        }

        private static bool TryGetTriangleLuma(
            CaseResult result,
            int triangleIndex,
            out float value)
        {
            value = 0f;
            if (result == null ||
                !result.TriangleStatistics.TryGetValue(
                    triangleIndex,
                    out TriangleLuminanceStatistics triangle))
            {
                return false;
            }
            value = triangle.MeanLuma;
            return true;
        }

        private static float Pearson(
            IReadOnlyList<float> x,
            IReadOnlyList<float> y)
        {
            if (x == null || y == null || x.Count != y.Count || x.Count < 2)
            {
                return 0f;
            }
            double meanX = x.Average(value => (double)value);
            double meanY = y.Average(value => (double)value);
            double covariance = 0d;
            double varianceX = 0d;
            double varianceY = 0d;
            for (int index = 0; index < x.Count; index++)
            {
                double dx = x[index] - meanX;
                double dy = y[index] - meanY;
                covariance += dx * dy;
                varianceX += dx * dx;
                varianceY += dy * dy;
            }
            double denominator = Math.Sqrt(varianceX * varianceY);
            return denominator <= 1e-20d
                ? 0f
                : (float)(covariance / denominator);
        }

        private static float Spearman(
            IReadOnlyList<float> x,
            IReadOnlyList<float> y)
        {
            if (x == null || y == null || x.Count != y.Count || x.Count < 2)
            {
                return 0f;
            }
            List<float> rankX = BuildAverageRanks(x);
            List<float> rankY = BuildAverageRanks(y);
            return Pearson(rankX, rankY);
        }

        private static List<float> BuildAverageRanks(IReadOnlyList<float> values)
        {
            List<int> ordered = Enumerable.Range(0, values.Count)
                .OrderBy(index => values[index])
                .ThenBy(index => index)
                .ToList();
            float[] ranks = new float[values.Count];
            int cursor = 0;
            while (cursor < ordered.Count)
            {
                int end = cursor + 1;
                while (end < ordered.Count &&
                    Mathf.Abs(
                        values[ordered[end]] -
                        values[ordered[cursor]]) <= 1e-7f)
                {
                    end++;
                }
                float averageRank = ((cursor + 1) + end) * 0.5f;
                for (int index = cursor; index < end; index++)
                {
                    ranks[ordered[index]] = averageRank;
                }
                cursor = end;
            }
            return ranks.ToList();
        }

        private struct OrientationSurfaceAccumulator
        {
            internal float NdotLSum;
            internal float LumaSum;
            internal int Weight;
        }

        private sealed class OrientationSurfaceMean
        {
            internal int ProvenanceIndex;
            internal float NdotL;
            internal float Luma;
        }

        private BrdfDirectionSummary Build5NDirectionSummary(
            BrdfDirectionDefinition direction,
            CaseResult legacy,
            CaseResult current,
            CaseResult dielectric,
            float diffuseResidual)
        {
            BrdfDirectionSummary summaryValue = new BrdfDirectionSummary
            {
                DirectionName = direction.Name,
                LightDirectionLocal = direction.LocalDirection,
                DiffuseEnergyMatchedMeanAbsoluteResidual = diffuseResidual,
                CurrentMeanAbsoluteResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(legacy, current),
                DielectricMeanAbsoluteResidual =
                    CalculateCaseRgbMeanAbsoluteResidual(
                        legacy,
                        dielectric)
            };
            summaryValue.DielectricResidualReduction =
                CalculateMismatchReduction(
                    summaryValue.CurrentMeanAbsoluteResidual,
                    summaryValue.DielectricMeanAbsoluteResidual);
            if (legacy == null || current == null || dielectric == null)
            {
                return summaryValue;
            }
            foreach (KeyValuePair<int, TriangleLuminanceStatistics> item in
                legacy.TriangleStatistics)
            {
                TriangleLuminanceStatistics legacyTriangle = item.Value;
                if ((legacyTriangle.SurfaceClass != SurfaceClass.SourceFace &&
                     legacyTriangle.SurfaceClass != SurfaceClass.OrdinaryBevel) ||
                    !current.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics currentTriangle) ||
                    !dielectric.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics dielectricTriangle))
                {
                    continue;
                }
                float lumaDenominator = Mathf.Max(
                    0.02f,
                    legacyTriangle.MeanLuma);
                float signedCurrent =
                    (currentTriangle.MeanLuma - legacyTriangle.MeanLuma) /
                    lumaDenominator;
                float signedDielectric =
                    (dielectricTriangle.MeanLuma - legacyTriangle.MeanLuma) /
                    lumaDenominator;
                float rgbDenominator = Mathf.Max(
                    0.02f,
                    legacyTriangle.MeanLinearRgb.magnitude);
                float rgbCurrent =
                    (currentTriangle.MeanLinearRgb -
                     legacyTriangle.MeanLinearRgb).magnitude /
                    rgbDenominator;
                float rgbDielectric =
                    (dielectricTriangle.MeanLinearRgb -
                     legacyTriangle.MeanLinearRgb).magnitude /
                    rgbDenominator;
                summaryValue.ComparedTriangles++;
                if (legacyTriangle.SurfaceClass == SurfaceClass.SourceFace)
                {
                    summaryValue.ComparedSourceTriangles++;
                }
                else
                {
                    summaryValue.ComparedBevelTriangles++;
                }
                if (signedCurrent > SignedResidualTolerance)
                    summaryValue.CurrentOverResponseCount++;
                else if (signedCurrent < -SignedResidualTolerance)
                    summaryValue.CurrentUnderResponseCount++;
                if (signedDielectric > SignedResidualTolerance)
                    summaryValue.DielectricOverResponseCount++;
                else if (signedDielectric < -SignedResidualTolerance)
                    summaryValue.DielectricUnderResponseCount++;
                summaryValue.TriangleComparisons.Add(
                    new TriangleResponseComparison
                    {
                        TriangleIndex = item.Key,
                        SurfaceClass = legacyTriangle.SurfaceClass,
                        LogicalBevelId = legacyTriangle.LogicalBevelId,
                        PixelCount = Mathf.Min(
                            legacyTriangle.PixelCount,
                            Mathf.Min(
                                currentTriangle.PixelCount,
                                dielectricTriangle.PixelCount)),
                        LegacyLinearRgb = legacyTriangle.MeanLinearRgb,
                        CurrentHlslLinearRgb =
                            currentTriangle.MeanLinearRgb,
                        DielectricHlslLinearRgb =
                            dielectricTriangle.MeanLinearRgb,
                        LegacyLuma = legacyTriangle.MeanLuma,
                        CurrentHlslLuma = currentTriangle.MeanLuma,
                        DielectricHlslLuma = dielectricTriangle.MeanLuma,
                        SignedResidualCurrent = signedCurrent,
                        SignedResidualDielectric = signedDielectric,
                        RgbResidualCurrent = rgbCurrent,
                        RgbResidualDielectric = rgbDielectric
                    });
            }
            summaryValue.CurrentOrderingInversionCount =
                CountOrderingInversions(current, legacy);
            summaryValue.DielectricOrderingInversionCount =
                CountOrderingInversions(dielectric, legacy);
            return summaryValue;
        }

        private static bool HasRequiredClassPixels(CaseResult result)
        {
            const int minimumClassPixels = 32;
            return result != null &&
                result.ClassStatistics.TryGetValue(
                    SurfaceClass.SourceFace,
                    out SurfaceClassStatistics source) &&
                source.PixelCount >= minimumClassPixels &&
                result.ClassStatistics.TryGetValue(
                    SurfaceClass.OrdinaryBevel,
                    out SurfaceClassStatistics bevel) &&
                bevel.PixelCount >= minimumClassPixels;
        }

        private static float GetRelativeResponse(
            CaseResult result,
            SurfaceClass surfaceClass)
        {
            return result != null &&
                result.ClassStatistics.TryGetValue(
                    surfaceClass,
                    out SurfaceClassStatistics statistics)
                        ? statistics.RelativeToWhole
                        : 0f;
        }

        private static float CalculateFinalToPrelightResponse(
            CaseResult final,
            CaseResult prelight,
            SurfaceClass surfaceClass)
        {
            if (final == null || prelight == null ||
                !final.ClassStatistics.TryGetValue(
                    surfaceClass,
                    out SurfaceClassStatistics finalStatistics) ||
                !prelight.ClassStatistics.TryGetValue(
                    surfaceClass,
                    out SurfaceClassStatistics prelightStatistics) ||
                prelightStatistics.MeanLuma <= 1e-5f)
            {
                return 0f;
            }
            return finalStatistics.MeanLuma /
                prelightStatistics.MeanLuma;
        }

        private static float CalculateClassMismatch(
            CaseResult current,
            CaseResult legacy)
        {
            if (current == null || legacy == null)
            {
                return float.PositiveInfinity;
            }
            return
                Mathf.Abs(
                    GetRelativeResponse(current, SurfaceClass.SourceFace) -
                    GetRelativeResponse(legacy, SurfaceClass.SourceFace)) +
                Mathf.Abs(
                    GetRelativeResponse(current, SurfaceClass.OrdinaryBevel) -
                    GetRelativeResponse(legacy, SurfaceClass.OrdinaryBevel));
        }

        private static float CalculateDerivedDiffuseMeanAbsoluteResidual(
            CaseResult legacyFull,
            CaseResult legacySpecular,
            CaseResult candidateFull,
            CaseResult candidateSpecular)
        {
            if (!CaseCoverageValid(legacyFull) ||
                !CaseCoverageValid(legacySpecular) ||
                !CaseCoverageValid(candidateFull) ||
                !CaseCoverageValid(candidateSpecular))
            {
                return -1f;
            }
            List<float> residuals = new();
            foreach (KeyValuePair<int, TriangleLuminanceStatistics> item in
                legacyFull.TriangleStatistics)
            {
                TriangleLuminanceStatistics legacyTriangle = item.Value;
                if ((legacyTriangle.SurfaceClass != SurfaceClass.SourceFace &&
                     legacyTriangle.SurfaceClass !=
                         SurfaceClass.OrdinaryBevel) ||
                    !legacySpecular.TriangleStatistics.TryGetValue(
                        item.Key, out TriangleLuminanceStatistics legacySpec) ||
                    !candidateFull.TriangleStatistics.TryGetValue(
                        item.Key, out TriangleLuminanceStatistics candidate) ||
                    !candidateSpecular.TriangleStatistics.TryGetValue(
                        item.Key, out TriangleLuminanceStatistics candidateSpec))
                {
                    continue;
                }
                Vector3 legacyDiffuse = MaxZero(
                    legacyTriangle.MeanLinearRgb - legacySpec.MeanLinearRgb);
                Vector3 candidateDiffuse = MaxZero(
                    candidate.MeanLinearRgb - candidateSpec.MeanLinearRgb);
                float denominator = Mathf.Max(0.02f, legacyDiffuse.magnitude);
                residuals.Add(
                    (candidateDiffuse - legacyDiffuse).magnitude /
                    denominator);
            }
            return residuals.Count == 0 ? -1f : residuals.Average();
        }

        private static Vector3 MaxZero(Vector3 value)
        {
            return new Vector3(
                Mathf.Max(0f, value.x),
                Mathf.Max(0f, value.y),
                Mathf.Max(0f, value.z));
        }

        private static float CalculateMismatchReduction(
            float baselineMismatch,
            float candidateMismatch)
        {
            if (float.IsNaN(baselineMismatch) ||
                float.IsInfinity(baselineMismatch) ||
                float.IsNaN(candidateMismatch) ||
                float.IsInfinity(candidateMismatch) ||
                baselineMismatch <= 1e-6f)
            {
                return 0f;
            }
            return (baselineMismatch - candidateMismatch) /
                baselineMismatch;
        }

        private static void CompareBevelParentSamples(
            CaseResult current,
            CaseResult legacy,
            Summary summaryValue)
        {
            Dictionary<(int Bevel, int Sample), BevelParentRenderSample>
                legacyByKey = legacy.BevelParentSamples.ToDictionary(
                    sample => (sample.LogicalBevelId, sample.SampleIndex));
            double deviationSum = 0.0;
            foreach (BevelParentRenderSample sample in
                current.BevelParentSamples)
            {
                if (!legacyByKey.TryGetValue(
                        (sample.LogicalBevelId, sample.SampleIndex),
                        out BevelParentRenderSample legacySample))
                {
                    continue;
                }
                summaryValue.ComparedBevelParentSamples++;
                float deviation = Mathf.Abs(
                    sample.NormalizedTransition -
                    legacySample.NormalizedTransition);
                deviationSum += deviation;
                bool orderingChanged = !string.Equals(
                    sample.Ordering,
                    legacySample.Ordering,
                    StringComparison.Ordinal);
                if (orderingChanged || deviation > 0.35f)
                {
                    summaryValue.OrderingMismatchAgainstLegacyCount++;
                }
            }
            summaryValue.MeanTransitionDeviationAgainstLegacy =
                summaryValue.ComparedBevelParentSamples > 0
                    ? (float)(deviationSum /
                        summaryValue.ComparedBevelParentSamples)
                    : 0f;
        }

        private void PopulateBrdfSummary(Summary summaryValue)
        {
            List<BrdfDirectionSummary> directionSummaries =
                BuildBrdfDirectionSummaries(includeAdaptive: true);
            summaryValue.BrdfDirections.AddRange(directionSummaries);
            List<BrdfDirectionSummary> valid = directionSummaries
                .Where(HasRequiredBrdfTriangleCoverage)
                .ToList();
            summaryValue.BrdfSweepAvailable = valid.Count == brdfDirections.Length;
            summaryValue.BrdfComparedDirections = valid.Count;
            if (valid.Count == 0)
            {
                summaryValue.BrdfWorkflowVerdict =
                    "INCONCLUSIVE_BRDF_SWEEP";
                return;
            }

            summaryValue.BrdfWorstDirection = valid
                .OrderByDescending(item => item.CurrentMeanAbsoluteResidual)
                .ThenBy(item => item.DirectionName, StringComparer.Ordinal)
                .First().DirectionName;
            summaryValue.BrdfCurrentMeanAbsoluteResidual =
                valid.Average(item => item.CurrentMeanAbsoluteResidual);
            summaryValue.BrdfDielectricMeanAbsoluteResidual =
                valid.Average(item => item.DielectricMeanAbsoluteResidual);
            summaryValue.BrdfDielectricResidualReduction =
                CalculateMismatchReduction(
                    summaryValue.BrdfCurrentMeanAbsoluteResidual,
                    summaryValue.BrdfDielectricMeanAbsoluteResidual);
            summaryValue.BrdfCurrentOverResponseCount =
                valid.Sum(item => item.CurrentOverResponseCount);
            summaryValue.BrdfCurrentUnderResponseCount =
                valid.Sum(item => item.CurrentUnderResponseCount);
            summaryValue.BrdfDielectricOverResponseCount =
                valid.Sum(item => item.DielectricOverResponseCount);
            summaryValue.BrdfDielectricUnderResponseCount =
                valid.Sum(item => item.DielectricUnderResponseCount);
            summaryValue.BrdfCurrentOrderingInversionCount =
                valid.Sum(item => item.CurrentOrderingInversionCount);
            summaryValue.BrdfDielectricOrderingInversionCount =
                valid.Sum(item => item.DielectricOrderingInversionCount);
            summaryValue.BrdfDielectricImprovedDirectionCount =
                valid.Count(item =>
                    item.DielectricMeanAbsoluteResidual <
                    item.CurrentMeanAbsoluteResidual);

            List<BrdfDirectionSummary> adaptive = valid
                .Where(item => item.AdaptiveStageAvailable)
                .ToList();
            summaryValue.BrdfAdaptiveDirectionCount = adaptive.Count;
            if (adaptive.Count > 0)
            {
                summaryValue.BrdfActualCurrentMeanAbsoluteResidual =
                    adaptive.Average(item =>
                        item.ActualCurrentMeanAbsoluteResidual);
                summaryValue.BrdfActualDielectricMeanAbsoluteResidual =
                    adaptive.Average(item =>
                        item.ActualDielectricMeanAbsoluteResidual);
                summaryValue.BrdfActualDielectricResidualReduction =
                    CalculateMismatchReduction(
                        summaryValue.BrdfActualCurrentMeanAbsoluteResidual,
                        summaryValue.BrdfActualDielectricMeanAbsoluteResidual);
                summaryValue.BrdfDiffuseEnergyMatchedMeanAbsoluteResidual =
                    adaptive.Average(item =>
                        item.DiffuseEnergyMatchedMeanAbsoluteResidual);
            }

            bool residualConfirmed =
                summaryValue.BrdfDielectricResidualReduction >= 0.50f &&
                summaryValue.BrdfDielectricImprovedDirectionCount >= 6;
            bool orderingConfirmed =
                summaryValue.BrdfCurrentOrderingInversionCount == 0
                    ? summaryValue.BrdfDielectricOrderingInversionCount == 0
                    : summaryValue.BrdfDielectricOrderingInversionCount <=
                        summaryValue.BrdfCurrentOrderingInversionCount * 0.5f;
            bool adaptiveComplete = adaptive.Count == 2;
            bool actualConfirmed = adaptiveComplete &&
                summaryValue.BrdfActualDielectricResidualReduction >= 0.35f &&
                adaptive.All(item =>
                    item.ActualDielectricMeanAbsoluteResidual <
                    item.ActualCurrentMeanAbsoluteResidual);
            bool diffuseParityClean = adaptiveComplete &&
                summaryValue.BrdfDiffuseEnergyMatchedMeanAbsoluteResidual <=
                    0.10f;
            bool contributionObserved =
                (summaryValue.BrdfDielectricResidualReduction >= 0.15f &&
                 summaryValue.BrdfDielectricImprovedDirectionCount >= 4) ||
                (adaptiveComplete &&
                 summaryValue.BrdfActualDielectricResidualReduction >= 0.15f);

            if (!summaryValue.BrdfSweepAvailable || !adaptiveComplete)
            {
                summaryValue.BrdfWorkflowVerdict =
                    "INCONCLUSIVE_BRDF_SWEEP";
            }
            else if (residualConfirmed &&
                orderingConfirmed &&
                actualConfirmed &&
                diffuseParityClean)
            {
                summaryValue.BrdfWorkflowVerdict =
                    "BRDF_F0_016_MISMATCH_CONFIRMED";
            }
            else if (contributionObserved)
            {
                summaryValue.BrdfWorkflowVerdict =
                    "BRDF_F0_016_CONTRIBUTES_BUT_NOT_SUFFICIENT";
            }
            else
            {
                summaryValue.BrdfWorkflowVerdict =
                    "BRDF_F0_016_NOT_PRIMARY";
            }
        }

        private static bool HasRequiredBrdfTriangleCoverage(
            BrdfDirectionSummary summaryValue)
        {
            return summaryValue != null &&
                summaryValue.ComparedSourceTriangles >=
                    MinimumBrdfTrianglesPerClass &&
                summaryValue.ComparedBevelTriangles >=
                    MinimumBrdfTrianglesPerClass;
        }

        private List<BrdfDirectionSummary> BuildBrdfDirectionSummaries(
            bool includeAdaptive)
        {
            List<BrdfDirectionSummary> summaries = new();
            foreach (BrdfDirectionDefinition direction in brdfDirections)
            {
                CaseResult legacy = FindBrdfCase(
                    direction.Name,
                    "LEGACY_F0_004");
                CaseResult current = FindBrdfCase(
                    direction.Name,
                    "HLSL_F0_016");
                CaseResult dielectric = FindBrdfCase(
                    direction.Name,
                    "HLSL_F0_004");
                BrdfDirectionSummary summaryValue =
                    BuildBrdfDirectionSummary(
                        direction,
                        legacy,
                        current,
                        dielectric);
                if (includeAdaptive)
                {
                    PopulateAdaptiveBrdfSummary(summaryValue);
                }
                summaries.Add(summaryValue);
            }
            return summaries;
        }

        private BrdfDirectionSummary BuildBrdfDirectionSummary(
            BrdfDirectionDefinition direction,
            CaseResult legacy,
            CaseResult current,
            CaseResult dielectric)
        {
            BrdfDirectionSummary summaryValue = new BrdfDirectionSummary
            {
                DirectionName = direction.Name,
                LightDirectionLocal = direction.LocalDirection
            };
            if (legacy == null || current == null || dielectric == null ||
                legacy.ReadbackError || current.ReadbackError ||
                dielectric.ReadbackError)
            {
                return summaryValue;
            }

            List<float> currentAbsolute = new();
            List<float> dielectricAbsolute = new();
            foreach (KeyValuePair<int, TriangleLuminanceStatistics> item in
                legacy.TriangleStatistics.OrderBy(pair => pair.Key))
            {
                if (!current.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics currentTriangle) ||
                    !dielectric.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics dielectricTriangle))
                {
                    continue;
                }
                TriangleLuminanceStatistics legacyTriangle = item.Value;
                if (legacyTriangle.SurfaceClass != SurfaceClass.SourceFace &&
                    legacyTriangle.SurfaceClass != SurfaceClass.OrdinaryBevel)
                {
                    continue;
                }
                float denominator = Mathf.Max(
                    0.02f,
                    legacyTriangle.MeanLuma);
                float signedCurrent =
                    (currentTriangle.MeanLuma - legacyTriangle.MeanLuma) /
                    denominator;
                float signedDielectric =
                    (dielectricTriangle.MeanLuma - legacyTriangle.MeanLuma) /
                    denominator;
                currentAbsolute.Add(Mathf.Abs(signedCurrent));
                dielectricAbsolute.Add(Mathf.Abs(signedDielectric));
                summaryValue.ComparedTriangles++;
                if (legacyTriangle.SurfaceClass == SurfaceClass.SourceFace)
                    summaryValue.ComparedSourceTriangles++;
                else
                    summaryValue.ComparedBevelTriangles++;
                if (signedCurrent > SignedResidualTolerance)
                    summaryValue.CurrentOverResponseCount++;
                else if (signedCurrent < -SignedResidualTolerance)
                    summaryValue.CurrentUnderResponseCount++;
                if (signedDielectric > SignedResidualTolerance)
                    summaryValue.DielectricOverResponseCount++;
                else if (signedDielectric < -SignedResidualTolerance)
                    summaryValue.DielectricUnderResponseCount++;
                summaryValue.TriangleComparisons.Add(
                    new TriangleResponseComparison
                    {
                        TriangleIndex = item.Key,
                        SurfaceClass = legacyTriangle.SurfaceClass,
                        LogicalBevelId = legacyTriangle.LogicalBevelId,
                        PixelCount = Mathf.Min(
                            legacyTriangle.PixelCount,
                            Mathf.Min(
                                currentTriangle.PixelCount,
                                dielectricTriangle.PixelCount)),
                        LegacyLuma = legacyTriangle.MeanLuma,
                        CurrentHlslLuma = currentTriangle.MeanLuma,
                        DielectricHlslLuma = dielectricTriangle.MeanLuma,
                        SignedResidualCurrent = signedCurrent,
                        SignedResidualDielectric = signedDielectric
                    });
            }
            currentAbsolute.Sort();
            dielectricAbsolute.Sort();
            summaryValue.CurrentMeanAbsoluteResidual =
                currentAbsolute.Count == 0 ? 0f : currentAbsolute.Average();
            summaryValue.CurrentP90AbsoluteResidual =
                Percentile(currentAbsolute, 0.90f);
            summaryValue.DielectricMeanAbsoluteResidual =
                dielectricAbsolute.Count == 0
                    ? 0f
                    : dielectricAbsolute.Average();
            summaryValue.DielectricP90AbsoluteResidual =
                Percentile(dielectricAbsolute, 0.90f);
            summaryValue.DielectricResidualReduction =
                CalculateMismatchReduction(
                    summaryValue.CurrentMeanAbsoluteResidual,
                    summaryValue.DielectricMeanAbsoluteResidual);
            summaryValue.CurrentOrderingInversionCount =
                CountOrderingInversions(current, legacy);
            summaryValue.DielectricOrderingInversionCount =
                CountOrderingInversions(dielectric, legacy);
            return summaryValue;
        }

        private void PopulateAdaptiveBrdfSummary(
            BrdfDirectionSummary summaryValue)
        {
            CaseResult legacyActual = FindBrdfCase(
                summaryValue.DirectionName,
                "LEGACY_ACTUAL");
            CaseResult currentActual = FindBrdfCase(
                summaryValue.DirectionName,
                "HLSL_F0_016_ACTUAL");
            CaseResult dielectricActual = FindBrdfCase(
                summaryValue.DirectionName,
                "HLSL_F0_004_ACTUAL");
            CaseResult legacyDiffuse = FindBrdfCase(
                summaryValue.DirectionName,
                "LEGACY_DIFFUSE_ENERGY_MATCHED");
            CaseResult hlslDiffuse = FindBrdfCase(
                summaryValue.DirectionName,
                "HLSL_DIFFUSE_ENERGY_MATCHED");
            if (legacyActual == null || currentActual == null ||
                dielectricActual == null || legacyDiffuse == null ||
                hlslDiffuse == null ||
                legacyActual.ReadbackError || currentActual.ReadbackError ||
                dielectricActual.ReadbackError || legacyDiffuse.ReadbackError ||
                hlslDiffuse.ReadbackError ||
                legacyActual.TriangleStatistics.Count == 0 ||
                currentActual.TriangleStatistics.Count == 0 ||
                dielectricActual.TriangleStatistics.Count == 0 ||
                legacyDiffuse.TriangleStatistics.Count == 0 ||
                hlslDiffuse.TriangleStatistics.Count == 0)
            {
                return;
            }
            summaryValue.AdaptiveStageAvailable = true;
            summaryValue.ActualCurrentMeanAbsoluteResidual =
                CalculateTriangleMeanAbsoluteResidual(
                    currentActual,
                    legacyActual);
            summaryValue.ActualDielectricMeanAbsoluteResidual =
                CalculateTriangleMeanAbsoluteResidual(
                    dielectricActual,
                    legacyActual);
            summaryValue.ActualDielectricResidualReduction =
                CalculateMismatchReduction(
                    summaryValue.ActualCurrentMeanAbsoluteResidual,
                    summaryValue.ActualDielectricMeanAbsoluteResidual);
            summaryValue.DiffuseEnergyMatchedMeanAbsoluteResidual =
                CalculateTriangleMeanAbsoluteResidual(
                    hlslDiffuse,
                    legacyDiffuse);
        }

        private static float CalculateTriangleMeanAbsoluteResidual(
            CaseResult candidate,
            CaseResult legacy)
        {
            if (candidate == null || legacy == null)
            {
                return 0f;
            }
            List<float> residuals = new();
            foreach (KeyValuePair<int, TriangleLuminanceStatistics> item in
                legacy.TriangleStatistics)
            {
                if ((item.Value.SurfaceClass != SurfaceClass.SourceFace &&
                     item.Value.SurfaceClass != SurfaceClass.OrdinaryBevel) ||
                    !candidate.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics candidateTriangle))
                {
                    continue;
                }
                residuals.Add(Mathf.Abs(
                    (candidateTriangle.MeanLuma - item.Value.MeanLuma) /
                    Mathf.Max(0.02f, item.Value.MeanLuma)));
            }
            return residuals.Count == 0 ? 0f : residuals.Average();
        }

        private static int CountOrderingInversions(
            CaseResult candidate,
            CaseResult legacy)
        {
            if (candidate == null || legacy == null)
            {
                return 0;
            }
            Dictionary<string, BevelParentRenderSample> legacySamples =
                legacy.BevelParentSamples.ToDictionary(
                    item => item.LogicalBevelId + ":" + item.SampleIndex,
                    item => item,
                    StringComparer.Ordinal);
            int count = 0;
            foreach (BevelParentRenderSample sample in
                candidate.BevelParentSamples)
            {
                string key = sample.LogicalBevelId + ":" + sample.SampleIndex;
                if (!legacySamples.TryGetValue(
                        key,
                        out BevelParentRenderSample legacySample))
                {
                    continue;
                }
                if (!string.Equals(
                        sample.Ordering,
                        legacySample.Ordering,
                        StringComparison.Ordinal))
                {
                    count++;
                }
            }
            return count;
        }

        private CaseResult FindBrdfCase(
            string directionName,
            string variant,
            string viewName = "CURRENT")
        {
            return results.FirstOrDefault(item =>
                item.IsBrdfSweep &&
                string.Equals(
                    item.DirectionName,
                    directionName,
                    StringComparison.Ordinal) &&
                string.Equals(
                    item.BrdfVariant,
                    variant,
                    StringComparison.Ordinal) &&
                string.Equals(
                    item.ViewName,
                    viewName,
                    StringComparison.Ordinal));
        }

        private static float CalculateCaseRgbMeanAbsoluteResidual(
            CaseResult legacy,
            CaseResult candidate)
        {
            if (!CaseCoverageValid(legacy) || !CaseCoverageValid(candidate))
            {
                return -1f;
            }
            List<float> residuals = new();
            foreach (KeyValuePair<int, TriangleLuminanceStatistics> item in
                legacy.TriangleStatistics)
            {
                TriangleLuminanceStatistics legacyTriangle = item.Value;
                if ((legacyTriangle.SurfaceClass != SurfaceClass.SourceFace &&
                     legacyTriangle.SurfaceClass != SurfaceClass.OrdinaryBevel) ||
                    !candidate.TriangleStatistics.TryGetValue(
                        item.Key,
                        out TriangleLuminanceStatistics candidateTriangle))
                {
                    continue;
                }
                float denominator = Mathf.Max(
                    0.02f,
                    legacyTriangle.MeanLinearRgb.magnitude);
                residuals.Add(
                    (candidateTriangle.MeanLinearRgb -
                     legacyTriangle.MeanLinearRgb).magnitude /
                    denominator);
            }
            return residuals.Count == 0 ? -1f : residuals.Average();
        }

        private static bool DirectionCaseEvaluable(
            CaseResult legacy,
            CaseResult candidate)
        {
            if (!CaseCoverageValid(legacy) || !CaseCoverageValid(candidate))
            {
                return false;
            }
            int requiredSource = ResolveRequiredIlluminatedTriangles(
                legacy,
                SurfaceClass.SourceFace);
            int requiredBevel = ResolveRequiredIlluminatedTriangles(
                legacy,
                SurfaceClass.OrdinaryBevel);
            return requiredSource >= 2 &&
                requiredBevel >= 2 &&
                CountIlluminatedTriangles(
                    legacy,
                    SurfaceClass.SourceFace) >= requiredSource &&
                CountIlluminatedTriangles(
                    legacy,
                    SurfaceClass.OrdinaryBevel) >= requiredBevel;
        }

        private static int ResolveRequiredIlluminatedTriangles(
            CaseResult result,
            SurfaceClass surfaceClass)
        {
            if (result == null)
            {
                return 0;
            }
            int visible = result.TriangleStatistics.Values.Count(item =>
                item.SurfaceClass == surfaceClass);
            if (visible < 2)
            {
                return visible;
            }
            return Mathf.Clamp(
                Mathf.CeilToInt(visible * 0.50f),
                2,
                MinimumIlluminatedTrianglesPerClass);
        }

        private static bool CaseCoverageValid(CaseResult result)
        {
            return result != null &&
                !result.ReadbackError &&
                result.TriangleCoverageRatio >= MinimumTriangleCoverageRatio &&
                result.TriangleStatistics.Count > 0;
        }

        private static int CountIlluminatedTriangles(
            CaseResult result,
            SurfaceClass surfaceClass)
        {
            if (result == null)
            {
                return 0;
            }
            return result.TriangleStatistics.Values.Count(item =>
                item.SurfaceClass == surfaceClass &&
                item.PredictedNdotL > 0.05f);
        }

        private string ResolveFirstDivergentStage()
        {
            CaseResult raw = Find("RAW_BASE_COLOR");
            float rawRatio = GetBevelToSourceRatio(raw);
            if (raw == null || rawRatio <= 1e-6f)
            {
                return "unavailable";
            }

            string[] stages =
            {
                "AFTER_PIXEL_VARIATION",
                "AFTER_EXPOSURE_SEMANTIC_SCALE",
                "AFTER_SURFACE_MOTTLE",
                "AFTER_CREVICE_BASE_DIRT_LAYERS",
                "UNLIT_PRELIGHT_ALBEDO"
            };
            foreach (string stageName in stages)
            {
                CaseResult stage = Find(stageName);
                float ratio = GetBevelToSourceRatio(stage);
                if (stage == null || stage.ReadbackError || ratio <= 1e-6f)
                {
                    continue;
                }
                float relativeBias = Mathf.Abs(
                    Mathf.Log(Mathf.Max(1e-6f, ratio / rawRatio)));
                if (relativeBias >= ClassMismatchThreshold)
                {
                    return stageName;
                }
            }
            return "none-before-lighting";
        }

        private static float GetBevelToSourceRatio(CaseResult result)
        {
            if (result == null)
            {
                return 0f;
            }
            float source = GetRelativeResponse(
                result,
                SurfaceClass.SourceFace);
            float bevel = GetRelativeResponse(
                result,
                SurfaceClass.OrdinaryBevel);
            return source > 1e-6f ? bevel / source : 0f;
        }

        private string ResolveSurfaceLightingOwnership(
            CaseResult current,
            CaseResult legacy,
            CaseResult cleared,
            float currentMismatch,
            float clearedMismatch,
            string firstDivergentStage,
            int orderingMismatchCount)
        {
            if (currentMismatch < ClassMismatchThreshold * 0.5f &&
                orderingMismatchCount == 0)
            {
                return "NO_SHADER_CLASS_MISMATCH_REPRODUCED";
            }
            if (cleared != null &&
                currentMismatch > ClassMismatchThreshold &&
                clearedMismatch <= currentMismatch * 0.50f)
            {
                return "MATERIAL_PROPERTY_BLOCK_OWNED";
            }

            if (!string.Equals(
                    firstDivergentStage,
                    "none-before-lighting",
                    StringComparison.Ordinal) &&
                !string.Equals(
                    firstDivergentStage,
                    "unavailable",
                    StringComparison.Ordinal))
            {
                CaseResult stage = Find(firstDivergentStage);
                CaseResult raw = Find("RAW_BASE_COLOR");
                float sourceDelta =
                    GetRelativeResponse(stage, SurfaceClass.SourceFace) -
                    GetRelativeResponse(raw, SurfaceClass.SourceFace);
                float bevelDelta =
                    GetRelativeResponse(stage, SurfaceClass.OrdinaryBevel) -
                    GetRelativeResponse(raw, SurfaceClass.OrdinaryBevel);
                if (sourceDelta < -ClassMismatchThreshold * 0.5f &&
                    Mathf.Abs(sourceDelta) > Mathf.Abs(bevelDelta) * 1.15f)
                {
                    return "SOURCE_FACE_PRELIGHT_UNDER_RESPONSE";
                }
                if (bevelDelta > ClassMismatchThreshold * 0.5f &&
                    Mathf.Abs(bevelDelta) > Mathf.Abs(sourceDelta) * 1.15f)
                {
                    return "BEVEL_PRELIGHT_OVER_RESPONSE";
                }
                if (string.Equals(
                        firstDivergentStage,
                        "AFTER_EXPOSURE_SEMANTIC_SCALE",
                        StringComparison.Ordinal) ||
                    string.Equals(
                        firstDivergentStage,
                        "AFTER_SURFACE_MOTTLE",
                        StringComparison.Ordinal) ||
                    string.Equals(
                        firstDivergentStage,
                        "AFTER_CREVICE_BASE_DIRT_LAYERS",
                        StringComparison.Ordinal))
                {
                    return "GENERATED_SEMANTIC_CLASS_MISMATCH";
                }
                return "CURRENT_HLSL_PRELIGHT_FIELD_OWNED";
            }

            CaseResult noGeneratedNormal = Find(
                "FINAL_WITHOUT_GENERATED_SURFACE_NORMAL");
            float noGeneratedNormalMismatch = CalculateClassMismatch(
                noGeneratedNormal,
                legacy);
            float generatedNormalReduction = CalculateMismatchReduction(
                currentMismatch,
                noGeneratedNormalMismatch);
            if (generatedNormalReduction >= 0.50f)
            {
                float currentSourceError = Mathf.Abs(
                    GetRelativeResponse(current, SurfaceClass.SourceFace) -
                    GetRelativeResponse(legacy, SurfaceClass.SourceFace));
                float currentBevelError = Mathf.Abs(
                    GetRelativeResponse(current, SurfaceClass.OrdinaryBevel) -
                    GetRelativeResponse(legacy, SurfaceClass.OrdinaryBevel));
                float noNormalSourceError = Mathf.Abs(
                    GetRelativeResponse(
                        noGeneratedNormal,
                        SurfaceClass.SourceFace) -
                    GetRelativeResponse(legacy, SurfaceClass.SourceFace));
                float noNormalBevelError = Mathf.Abs(
                    GetRelativeResponse(
                        noGeneratedNormal,
                        SurfaceClass.OrdinaryBevel) -
                    GetRelativeResponse(legacy, SurfaceClass.OrdinaryBevel));
                float sourceImprovement =
                    currentSourceError - noNormalSourceError;
                float bevelImprovement =
                    currentBevelError - noNormalBevelError;
                return sourceImprovement > bevelImprovement
                    ? "SOURCE_FACE_NORMAL_PATH_MISMATCH"
                    : "BEVEL_NORMAL_PATH_MISMATCH";
            }

            float additionalLightsReduction = CalculateMismatchReduction(
                currentMismatch,
                CalculateClassMismatch(
                    Find("ADDITIONAL_LIGHTS_OFF"),
                    legacy));
            if (additionalLightsReduction >= 0.50f)
            {
                return "ADDITIONAL_LIGHT_INTERACTION_OWNED";
            }
            float specularReduction = CalculateMismatchReduction(
                currentMismatch,
                CalculateClassMismatch(
                    Find("SPECULAR_AND_SMOOTHNESS_OFF"),
                    legacy));
            if (specularReduction >= 0.50f)
            {
                return "SPECULAR_RESPONSE_MISMATCH";
            }

            CaseResult currentDirect = Find(
                "SAME_MESH__CURRENT_HLSL__MAIN_DIRECT_ENVIRONMENT");
            CaseResult legacyDirect = Find(
                "SAME_MESH__LEGACY_MATERIAL__MAIN_DIRECT_ENVIRONMENT");
            CaseResult currentIndirect = Find(
                "SAME_MESH__CURRENT_HLSL__INDIRECT_ENVIRONMENT");
            CaseResult legacyIndirect = Find(
                "SAME_MESH__LEGACY_MATERIAL__INDIRECT_ENVIRONMENT");
            float directMismatch = CalculateClassMismatch(
                currentDirect,
                legacyDirect);
            float indirectMismatch = CalculateClassMismatch(
                currentIndirect,
                legacyIndirect);
            if (directMismatch >= ClassMismatchThreshold)
            {
                float sourceDelta =
                    GetRelativeResponse(
                        currentDirect,
                        SurfaceClass.SourceFace) -
                    GetRelativeResponse(
                        legacyDirect,
                        SurfaceClass.SourceFace);
                float bevelDelta =
                    GetRelativeResponse(
                        currentDirect,
                        SurfaceClass.OrdinaryBevel) -
                    GetRelativeResponse(
                        legacyDirect,
                        SurfaceClass.OrdinaryBevel);
                if (sourceDelta < -ClassMismatchThreshold * 0.5f &&
                    Mathf.Abs(sourceDelta) > Mathf.Abs(bevelDelta) * 1.15f)
                {
                    return "SOURCE_FACE_MAIN_LIGHT_UNDER_RESPONSE";
                }
                if (bevelDelta > ClassMismatchThreshold * 0.5f &&
                    Mathf.Abs(bevelDelta) > Mathf.Abs(sourceDelta) * 1.15f)
                {
                    return "BEVEL_MAIN_LIGHT_OVER_RESPONSE";
                }
                if (indirectMismatch < ClassMismatchThreshold)
                {
                    return "MAIN_LIGHT_INTEGRATION_MISMATCH";
                }
            }
            if (indirectMismatch >= ClassMismatchThreshold &&
                directMismatch < ClassMismatchThreshold)
            {
                return "INDIRECT_OR_SHADOW_RESPONSE_MISMATCH";
            }
            if (directMismatch < ClassMismatchThreshold &&
                indirectMismatch < ClassMismatchThreshold &&
                currentMismatch >= ClassMismatchThreshold)
            {
                return "SPECULAR_RESPONSE_MISMATCH";
            }
            return "MULTIPLE_SHADER_STAGE_MISMATCHES";
        }

        private static void ClassifyOwnership(Summary value, float baseline)
        {
            float material = Mathf.Max(0f, value.MaterialEffect);
            float mesh = Mathf.Max(0f, value.MeshEffect);
            float interaction = Mathf.Max(0f, value.InteractionEffect);
            float largest = Mathf.Max(material, Mathf.Max(mesh, interaction));
            float second = new[] { material, mesh, interaction }
                .OrderByDescending(x => x)
                .Skip(1)
                .FirstOrDefault();
            if (largest <= Mathf.Max(0.002f, baseline * 0.08f))
            {
                value.Ownership = value.HighIntensitySuppression >= 0.60f
                    ? "LIGHTING_ENVIRONMENT_OWNED"
                    : "INCONCLUSIVE_NO_DOMINANT_PARITY_EFFECT";
                value.OwnershipConfidence = 0f;
                return;
            }

            value.OwnershipConfidence = Mathf.Clamp01(
                (largest - second) / Mathf.Max(largest, 1e-6f));
            if (second > largest * 0.67f)
            {
                value.Ownership = "MULTIPLE_CONTRIBUTORS";
            }
            else if (largest == material)
            {
                value.Ownership = "MATERIAL_OR_SHADER_OWNED";
            }
            else if (largest == mesh)
            {
                value.Ownership = "MESH_DATA_OWNED";
            }
            else
            {
                value.Ownership = "MESH_MATERIAL_INTERACTION";
            }
        }

        private CaseResult Find(string name)
        {
            return results.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.Ordinal));
        }

        private FacetScore ScoreFaceting(
            Subject subject,
            Color32[] pixels,
            Color32[] mask,
            Matrix4x4 localToClip,
            Vector3 cameraPosition)
        {
            FacetScore normal = ScoreFaceting(
                subject,
                pixels,
                mask,
                localToClip,
                cameraPosition,
                false);
            FacetScore flipped = ScoreFaceting(
                subject,
                pixels,
                mask,
                localToClip,
                cameraPosition,
                true);
            return flipped.ValidSamples > normal.ValidSamples
                ? flipped
                : normal;
        }

        private FacetScore ScoreFaceting(
            Subject subject,
            Color32[] pixels,
            Color32[] mask,
            Matrix4x4 localToClip,
            Vector3 cameraPosition,
            bool flipY)
        {
            List<float> gradientJumps = new();
            List<float> rawGradientJumps = new();
            List<float> colorGradientJumps = new();
            List<float> valueSteps = new();
            int totalEdges = 0;
            int frontFacingEdges = 0;
            int projectedEdges = 0;
            foreach (InternalEdge edge in subject.InternalEdges)
            {
                totalEdges++;
                if (!IsPotentiallyVisible(subject, edge, cameraPosition))
                    continue;
                frontFacingEdges++;
                Vector2 a;
                Vector2 b;
                if (!Project(edge.A, localToClip, out a) ||
                    !Project(edge.B, localToClip, out b))
                {
                    continue;
                }
                Vector2 delta = b - a;
                float length = delta.magnitude;
                if (length < 10f || length > CaptureSize * 0.85f) continue;
                projectedEdges++;
                Vector2 perpendicular = new Vector2(-delta.y, delta.x) / length;
                for (int sampleIndex = 1; sampleIndex <= 4; sampleIndex++)
                {
                    float t = sampleIndex / 5f;
                    Vector2 p = Vector2.Lerp(a, b, t);
                    Vector2 negativeFar = p - perpendicular * 3.5f;
                    Vector2 negativeNear = p - perpendicular * 1.5f;
                    Vector2 positiveNear = p + perpendicular * 1.5f;
                    Vector2 positiveFar = p + perpendicular * 3.5f;
                    if (!MaskContains(mask, negativeFar, flipY) ||
                        !MaskContains(mask, negativeNear, flipY) ||
                        !MaskContains(mask, positiveNear, flipY) ||
                        !MaskContains(mask, positiveFar, flipY))
                    {
                        continue;
                    }
                    float nFar = SampleLuma(pixels, negativeFar, flipY);
                    float nNear = SampleLuma(pixels, negativeNear, flipY);
                    float pNear = SampleLuma(pixels, positiveNear, flipY);
                    float pFar = SampleLuma(pixels, positiveFar, flipY);
                    float negativeDerivative = (nNear - nFar) * 0.5f;
                    float positiveDerivative = (pFar - pNear) * 0.5f;
                    float rawGradientJump =
                        Mathf.Abs(negativeDerivative - positiveDerivative);
                    float localLuma = Mathf.Max(
                        0.05f,
                        (Mathf.Abs(nFar) + Mathf.Abs(nNear) +
                         Mathf.Abs(pNear) + Mathf.Abs(pFar)) * 0.25f);
                    rawGradientJumps.Add(rawGradientJump);
                    gradientJumps.Add(rawGradientJump / localLuma);
                    valueSteps.Add(Mathf.Abs(nNear - pNear) / localLuma);

                    Vector3 cnFar = SampleRgb(pixels, negativeFar, flipY);
                    Vector3 cnNear = SampleRgb(pixels, negativeNear, flipY);
                    Vector3 cpNear = SampleRgb(pixels, positiveNear, flipY);
                    Vector3 cpFar = SampleRgb(pixels, positiveFar, flipY);
                    Vector3 negativeColorDerivative =
                        (cnNear - cnFar) * 0.5f;
                    Vector3 positiveColorDerivative =
                        (cpFar - cpNear) * 0.5f;
                    float localColorMagnitude = Mathf.Max(
                        0.05f,
                        (cnFar.magnitude + cnNear.magnitude +
                         cpNear.magnitude + cpFar.magnitude) * 0.25f);
                    colorGradientJumps.Add(
                        (negativeColorDerivative - positiveColorDerivative)
                        .magnitude / localColorMagnitude);
                }
            }

            if (gradientJumps.Count == 0)
            {
                return new FacetScore
                {
                    FlipY = flipY,
                    TotalEdges = totalEdges,
                    FrontFacingEdges = frontFacingEdges,
                    ProjectedEdges = projectedEdges
                };
            }
            gradientJumps.Sort();
            rawGradientJumps.Sort();
            colorGradientJumps.Sort();
            valueSteps.Sort();
            float mean = gradientJumps.Average();
            float p90 = Percentile(gradientJumps, 0.90f);
            float meanColor = colorGradientJumps.Average();
            float p90Color = Percentile(colorGradientJumps, 0.90f);
            return new FacetScore
            {
                FlipY = flipY,
                TotalEdges = totalEdges,
                FrontFacingEdges = frontFacingEdges,
                ProjectedEdges = projectedEdges,
                ValidSamples = gradientJumps.Count,
                MeanGradientJump = mean,
                P90GradientJump = p90,
                MaximumGradientJump = gradientJumps[gradientJumps.Count - 1],
                MeanValueStep = valueSteps.Average(),
                MeanRawGradientJump = rawGradientJumps.Average(),
                P90RawGradientJump = Percentile(rawGradientJumps, 0.90f),
                MeanColorGradientJump = meanColor,
                P90ColorGradientJump = p90Color,
                Score =
                    mean * 0.25f +
                    p90 * 0.45f +
                    meanColor * 0.10f +
                    p90Color * 0.20f
            };
        }

        private static bool IsPotentiallyVisible(
            Subject subject,
            InternalEdge edge,
            Vector3 cameraPosition)
        {
            if (edge.NormalA == Vector3.zero || edge.NormalB == Vector3.zero)
                return false;
            Vector3 midpointLocal = (edge.A + edge.B) * 0.5f;
            Vector3 midpointWorld =
                subject.CloneLocalToWorld.MultiplyPoint3x4(midpointLocal);
            Vector3 viewDirection = cameraPosition - midpointWorld;
            if (viewDirection.sqrMagnitude <= 1e-12f) return false;
            viewDirection.Normalize();

            Matrix4x4 normalMatrix = subject.CloneLocalToWorld.inverse.transpose;
            Vector3 normalA = normalMatrix.MultiplyVector(edge.NormalA).normalized;
            Vector3 normalB = normalMatrix.MultiplyVector(edge.NormalB).normalized;
            if (normalA == Vector3.zero || normalB == Vector3.zero) return false;
            float facingA = Vector3.Dot(normalA, viewDirection);
            float facingB = Vector3.Dot(normalB, viewDirection);
            return Mathf.Max(facingA, facingB) > 0.04f &&
                   Mathf.Min(facingA, facingB) > -0.20f;
        }

        private static float Percentile(List<float> values, float percentile)
        {
            if (values.Count == 0) return 0f;
            int index = Mathf.Clamp(
                Mathf.RoundToInt((values.Count - 1) * percentile),
                0,
                values.Count - 1);
            return values[index];
        }

        private static void CalculateMaskedStatistics(
            Color[] pixels,
            Color32[] mask,
            bool flipY,
            out float meanLuma,
            out float saturatedFraction)
        {
            double lumaSum = 0.0;
            int included = 0;
            int saturated = 0;
            for (int y = 0; y < CaptureSize; y++)
            {
                int sourceY = flipY ? CaptureSize - 1 - y : y;
                int row = sourceY * CaptureSize;
                for (int x = 0; x < CaptureSize; x++)
                {
                    int index = row + x;
                    if (index < 0 ||
                        index >= pixels.Length ||
                        index >= mask.Length)
                    {
                        continue;
                    }
                    Color32 maskValue = mask[index];
                    if (maskValue.r <= 96 &&
                        maskValue.g <= 96 &&
                        maskValue.b <= 96)
                    {
                        continue;
                    }
                    Color value = pixels[index];
                    float luma = LinearLuma(value);
                    lumaSum += luma;
                    included++;
                    if (value.r >= 1f || value.g >= 1f || value.b >= 1f)
                    {
                        saturated++;
                    }
                }
            }
            meanLuma = included > 0
                ? (float)(lumaSum / included)
                : 0f;
            saturatedFraction = included > 0
                ? saturated / (float)included
                : 0f;
        }
        private void CaptureOrientationPixelEvidence(
            RenderCase renderCase,
            CaseResult result)
        {
            if (string.Equals(
                    renderCase.OrientationKind,
                    "StaticAlbedo",
                    StringComparison.Ordinal))
            {
                orientationAlbedoPixels[OrientationAlbedoPixelKey(
                    renderCase.ViewName,
                    renderCase.OrientationStage)] = new OrientationPixelCapture
                    {
                        Pixels = result.LinearPixels,
                        IdentityFlipRelativeToLighting =
                            result.IdentityFlipRelativeToLighting,
                        LightingForegroundPixels =
                            result.LightingForegroundPixelCount
                    };
                return;
            }
            if (string.Equals(
                    renderCase.OrientationKind,
                    "NdotL",
                    StringComparison.Ordinal))
            {
                orientationNdotLPixels[OrientationNdotLPixelKey(
                    renderCase.ViewName,
                    renderCase.DirectionName)] = new OrientationPixelCapture
                    {
                        Pixels = result.LinearPixels,
                        IdentityFlipRelativeToLighting =
                            result.IdentityFlipRelativeToLighting,
                        LightingForegroundPixels =
                            result.LightingForegroundPixelCount
                    };
                return;
            }
            if (!string.Equals(
                    renderCase.OrientationKind,
                    "DirectStage",
                    StringComparison.Ordinal))
            {
                return;
            }

            if (!orientationAlbedoPixels.TryGetValue(
                    OrientationAlbedoPixelKey(
                        renderCase.ViewName,
                        renderCase.OrientationStage),
                    out OrientationPixelCapture albedoCapture) ||
                !orientationNdotLPixels.TryGetValue(
                    OrientationNdotLPixelKey(
                        renderCase.ViewName,
                        renderCase.DirectionName),
                    out OrientationPixelCapture ndotlCapture))
            {
                result.ReadbackError = true;
                result.Error =
                    "orientation direct-product evidence unavailable: view=" +
                    renderCase.ViewName + ",direction=" +
                    renderCase.DirectionName + ",stage=" +
                    renderCase.OrientationStage;
                fatalIdentityContractFailure = true;
                fatalContractReason = result.Error;
                return;
            }

            if (albedoCapture.IdentityFlipRelativeToLighting !=
                    result.IdentityFlipRelativeToLighting ||
                ndotlCapture.IdentityFlipRelativeToLighting !=
                    result.IdentityFlipRelativeToLighting ||
                albedoCapture.LightingForegroundPixels !=
                    result.LightingForegroundPixelCount ||
                ndotlCapture.LightingForegroundPixels !=
                    result.LightingForegroundPixelCount)
            {
                result.ReadbackError = true;
                result.Error =
                    "orientation direct-product buffer alignment mismatch: view=" +
                    renderCase.ViewName + ",direction=" +
                    renderCase.DirectionName + ",stage=" +
                    renderCase.OrientationStage +
                    ",directFlip=" + result.IdentityFlipRelativeToLighting +
                    ",albedoFlip=" +
                    albedoCapture.IdentityFlipRelativeToLighting +
                    ",ndotlFlip=" +
                    ndotlCapture.IdentityFlipRelativeToLighting +
                    ",directForeground=" +
                    result.LightingForegroundPixelCount +
                    ",albedoForeground=" +
                    albedoCapture.LightingForegroundPixels +
                    ",ndotlForeground=" +
                    ndotlCapture.LightingForegroundPixels;
                fatalIdentityContractFailure = true;
                fatalContractReason = result.Error;
                return;
            }

            Color[] albedoPixels = albedoCapture.Pixels;
            Color[] ndotlPixels = ndotlCapture.Pixels;
            int count = Mathf.Min(
                result.LinearPixels.Length,
                Mathf.Min(albedoPixels.Length, ndotlPixels.Length));
            double sumAbsolute = 0d;
            double sumSquared = 0d;
            double expectedEnergy = 0d;
            int channels = 0;
            int foregroundPixels = 0;
            for (int index = 0; index < count; index++)
            {
                Color observed = result.LinearPixels[index];
                Color albedo = albedoPixels[index];
                Color ndotl = ndotlPixels[index];
                if (observed.a <= 0.0001f ||
                    albedo.a <= 0.0001f ||
                    ndotl.a <= 0.0001f)
                {
                    continue;
                }
                float lightFactor = Mathf.Max(0f, ndotl.r) *
                    Mathf.Max(0f, ndotl.g) *
                    Mathf.Max(0f, ndotl.b);
                Vector3 expected = new Vector3(
                    albedo.r * lightFactor,
                    albedo.g * lightFactor,
                    albedo.b * lightFactor);
                Vector3 actual = new Vector3(
                    observed.r,
                    observed.g,
                    observed.b);
                Vector3 delta = actual - expected;
                sumAbsolute +=
                    Math.Abs(delta.x) +
                    Math.Abs(delta.y) +
                    Math.Abs(delta.z);
                sumSquared +=
                    delta.x * delta.x +
                    delta.y * delta.y +
                    delta.z * delta.z;
                expectedEnergy +=
                    expected.x * expected.x +
                    expected.y * expected.y +
                    expected.z * expected.z;
                channels += 3;
                foregroundPixels++;
            }
            result.OrientationDirectProductPixelCount = foregroundPixels;
            result.OrientationDirectProductMeanAbsoluteResidual = channels > 0
                ? (float)(sumAbsolute / channels)
                : 0f;
            result.OrientationDirectProductNormalizedRmse =
                channels > 0 && expectedEnergy > 1e-20d
                    ? (float)Math.Sqrt(sumSquared / expectedEnergy)
                    : 0f;
        }

        private static string OrientationAlbedoPixelKey(
            string viewName,
            string stageName)
        {
            return (viewName ?? string.Empty) + "\u001f" +
                (stageName ?? string.Empty);
        }

        private static string OrientationNdotLPixelKey(
            string viewName,
            string directionName)
        {
            return (viewName ?? string.Empty) + "\u001f" +
                (directionName ?? string.Empty);
        }

        private void CalculateTriangleStatistics(
            Subject subject,
            CaseResult result,
            bool identityFlipRelativeToLighting)
        {
            if (!triangleIdentityPixels.TryGetValue(
                    IdentityKey(subject, result.ViewName),
                    out Color32[] identityPixels))
            {
                return;
            }

            Dictionary<int, Vector3> sums = new();
            Dictionary<int, int> counts = new();
            Dictionary<int, List<float>> lumas = new();
            int pixelCount = Mathf.Min(
                result.LinearPixels.Length,
                identityPixels.Length);
            for (int lightingIndex = 0;
                 lightingIndex < pixelCount;
                 lightingIndex++)
            {
                int identityIndex = MapLightingIndexToIdentityIndex(
                    lightingIndex,
                    identityFlipRelativeToLighting);
                int triangleIndex = DecodeTriangleIdentity(
                    identityPixels[identityIndex]);
                if (triangleIndex < 0 ||
                    triangleIndex >= subject.TriangleRecords.Length)
                {
                    continue;
                }
                Color pixel = result.LinearPixels[lightingIndex];
                Vector3 linearRgb = new Vector3(pixel.r, pixel.g, pixel.b);
                sums.TryGetValue(triangleIndex, out Vector3 sum);
                counts.TryGetValue(triangleIndex, out int count);
                sums[triangleIndex] = sum + linearRgb;
                counts[triangleIndex] = count + 1;
                if (!lumas.TryGetValue(
                        triangleIndex,
                        out List<float> triangleLumas))
                {
                    triangleLumas = new List<float>();
                    lumas[triangleIndex] = triangleLumas;
                }
                triangleLumas.Add(Vector3.Dot(
                    linearRgb,
                    new Vector3(0.2126f, 0.7152f, 0.0722f)));
            }

            Matrix4x4 worldToLocal = subject.CloneLocalToWorld.inverse;
            Vector3 lightLocal = result.LightDirectionLocal.normalized;
            foreach (KeyValuePair<int, int> item in counts)
            {
                if (item.Value < MinimumTrianglePixels)
                {
                    continue;
                }
                MassGenerator.FinalTriangleRecord triangle =
                    subject.TriangleRecords[item.Key];
                Vector3 meanRgb = sums[item.Key] / item.Value;
                Vector3 storedNormal = triangle?.RenderNormal.normalized ??
                    Vector3.up;
                Vector3 centroidLocal = triangle == null
                    ? Vector3.zero
                    : TriangleCentroid(triangle);
                Vector3 centroidWorld = subject.CloneLocalToWorld
                    .MultiplyPoint3x4(centroidLocal);
                Vector3 viewLocal = worldToLocal
                    .MultiplyVector(result.CameraPositionWorld - centroidWorld)
                    .normalized;
                Vector3 halfLocal = (lightLocal + viewLocal).normalized;
                ResolveParentFaceIds(
                    subject,
                    triangle?.LogicalBevelId ?? -1,
                    out int parentFaceA,
                    out int parentFaceB);
                List<float> triangleLumas = lumas[item.Key];
                float meanLuma = triangleLumas.Average();
                double variance = 0d;
                foreach (float luma in triangleLumas)
                {
                    double delta = luma - meanLuma;
                    variance += delta * delta;
                }
                float standardDeviation = triangleLumas.Count > 0
                    ? (float)Math.Sqrt(variance / triangleLumas.Count)
                    : 0f;
                result.TriangleStatistics[item.Key] =
                    new TriangleLuminanceStatistics
                    {
                        TriangleIndex = item.Key,
                        SurfaceClass = subject.TriangleClasses[item.Key],
                        LogicalBevelId = triangle?.LogicalBevelId ?? -1,
                        ParentFaceA = parentFaceA,
                        ParentFaceB = parentFaceB,
                        ProvenanceKind = triangle?.ProvenanceKind ?? -1,
                        ProvenanceIndex = triangle?.ProvenanceIndex ?? -1,
                        SurfaceGroup = triangle?.SurfaceGroup ?? -1,
                        GeometricNormalLocal = triangle?.GeometricNormal.normalized ??
                            Vector3.up,
                        AuthoredNormalLocal = triangle?.AuthoredNormal.normalized ??
                            Vector3.up,
                        MaskA = triangle?.MaskA ?? Vector4.zero,
                        MaskB = triangle?.MaskB ?? Vector4.zero,
                        MaskC = triangle?.MaskC ?? Vector4.zero,
                        StructuralA = triangle?.StructuralA ?? Vector4.zero,
                        StructuralB = triangle?.StructuralB ?? Vector4.zero,
                        StructuralC = triangle?.StructuralC ?? Vector4.zero,
                        TriangleCondition = triangle == null
                            ? "Missing"
                            : triangle.TriangleCondition.ToString(),
                        TriangleAspectRatio = triangle?.TriangleAspectRatio ?? 0d,
                        TriangleMinimumAngleDegrees =
                            triangle?.TriangleMinimumAngleDegrees ?? 0d,
                        PixelCount = item.Value,
                        MeanLinearRgb = meanRgb,
                        MeanLuma = meanLuma,
                        MinLuma = triangleLumas.Min(),
                        P10Luma = Percentile(triangleLumas, 0.10f),
                        MedianLuma = Percentile(triangleLumas, 0.50f),
                        P90Luma = Percentile(triangleLumas, 0.90f),
                        MaxLuma = triangleLumas.Max(),
                        StandardDeviationLuma = standardDeviation,
                        StoredNormalLocal = storedNormal,
                        PredictedNdotL = Mathf.Max(
                            0f,
                            Vector3.Dot(storedNormal, lightLocal)),
                        PredictedNdotV = Mathf.Max(
                            0f,
                            Vector3.Dot(storedNormal, viewLocal)),
                        PredictedNdotH = Mathf.Max(
                            0f,
                            Vector3.Dot(storedNormal, halfLocal))
                    };
            }
        }
        private void CalculateSurfaceClassStatistics(
            Subject subject,
            CaseResult result,
            bool identityFlipRelativeToLighting)
        {
            if (!triangleIdentityPixels.TryGetValue(
                    IdentityKey(subject, result.ViewName),
                    out Color32[] identityPixels))
            {
                return;
            }

            foreach (SurfaceClass surfaceClass in ReportedSurfaceClasses)
            {
                SurfaceClassStatistics statistics =
                    CalculateSurfaceClassStatistics(
                        subject,
                        result.LinearPixels,
                        identityPixels,
                        surfaceClass,
                        identityFlipRelativeToLighting);
                result.ClassStatistics[surfaceClass] = statistics;
            }

            if (!result.ClassStatistics.TryGetValue(
                    SurfaceClass.WholeObject,
                    out SurfaceClassStatistics whole) ||
                whole.MeanLuma <= 1e-6f)
            {
                return;
            }
            foreach (SurfaceClassStatistics statistics in
                result.ClassStatistics.Values)
            {
                statistics.RelativeToWhole =
                    statistics.MeanLuma / whole.MeanLuma;
            }
        }
        private static SurfaceClassStatistics CalculateSurfaceClassStatistics(
            Subject subject,
            Color[] pixels,
            Color32[] identityPixels,
            SurfaceClass surfaceClass,
            bool identityFlipRelativeToLighting)
        {
            List<float> lumas = new();
            int pixelCount = Mathf.Min(pixels.Length, identityPixels.Length);
            for (int lightingIndex = 0;
                 lightingIndex < pixelCount;
                 lightingIndex++)
            {
                int identityIndex = MapLightingIndexToIdentityIndex(
                    lightingIndex,
                    identityFlipRelativeToLighting);
                int triangleIndex = DecodeTriangleIdentity(
                    identityPixels[identityIndex]);
                if (triangleIndex < 0 ||
                    triangleIndex >= subject.TriangleClasses.Length)
                {
                    continue;
                }
                if (surfaceClass != SurfaceClass.WholeObject &&
                    subject.TriangleClasses[triangleIndex] != surfaceClass)
                {
                    continue;
                }
                lumas.Add(LinearLuma(pixels[lightingIndex]));
            }
            lumas.Sort();
            return new SurfaceClassStatistics
            {
                Class = surfaceClass,
                PixelCount = lumas.Count,
                MeanLuma = lumas.Count == 0 ? 0f : lumas.Average(),
                P10Luma = Percentile(lumas, 0.10f),
                MedianLuma = Percentile(lumas, 0.50f),
                P90Luma = Percentile(lumas, 0.90f)
            };
        }

        private void CalculateBevelParentSamples(
            Subject subject,
            CaseResult result,
            Color32[] wholeMask,
            bool flipY,
            bool identityFlipRelativeToLighting)
        {
            if (!triangleIdentityPixels.TryGetValue(
                    IdentityKey(subject, result.ViewName),
                    out Color32[] identityPixels))
            {
                return;
            }

            float outsideSum = 0f;
            foreach (BevelParentGeometrySample sample in
                subject.BevelParentSamples)
            {
                if (!TrySampleProjectedLuma(
                        result.LinearPixels,
                        wholeMask,
                        identityPixels,
                        sample.ParentATriangleIndex,
                        sample.ParentA,
                        result.LocalToClip,
                        flipY,
                        identityFlipRelativeToLighting,
                        out float parentA) ||
                    !TrySampleProjectedLuma(
                        result.LinearPixels,
                        wholeMask,
                        identityPixels,
                        sample.BevelTriangleIndex,
                        sample.Bevel,
                        result.LocalToClip,
                        flipY,
                        identityFlipRelativeToLighting,
                        out float bevel) ||
                    !TrySampleProjectedLuma(
                        result.LinearPixels,
                        wholeMask,
                        identityPixels,
                        sample.ParentBTriangleIndex,
                        sample.ParentB,
                        result.LocalToClip,
                        flipY,
                        identityFlipRelativeToLighting,
                        out float parentB))
                {
                    continue;
                }

                // GM-SURFACE.5P/5Q: preserve the legacy parent-envelope label as
                // descriptive evidence only. A bevel being brighter than both or
                // darker than both parents is NOT automatically an orientation
                // defect: 5Q counts it as such only when the measured bevel NdotL
                // is itself intermediate between the measured parent NdotL values.
                float minimum = Mathf.Min(parentA, parentB);
                float maximum = Mathf.Max(parentA, parentB);
                float range = maximum - minimum;
                float tolerance = Mathf.Max(
                    0.005f,
                    range * OrderingTolerance);
                float outside = bevel < minimum - tolerance
                    ? minimum - bevel
                    : bevel > maximum + tolerance
                        ? bevel - maximum
                        : 0f;
                string ordering = outside <= 0f
                    ? "WithinParentEnvelope"
                    : bevel < minimum
                        ? "DarkerThanBothParents"
                        : "BrighterThanBothParents";
                float transition = Mathf.Abs(parentB - parentA) > 0.01f
                    ? (bevel - parentA) / (parentB - parentA)
                    : 0.5f +
                        (bevel - (parentA + parentB) * 0.5f) / 0.05f;

                result.BevelParentSamples.Add(new BevelParentRenderSample
                {
                    LogicalBevelId = sample.LogicalBevelId,
                    SampleIndex = sample.SampleIndex,
                    ParentALuma = parentA,
                    BevelLuma = bevel,
                    ParentBLuma = parentB,
                    NormalizedTransition = transition,
                    OutsideEnvelopeMagnitude = outside,
                    Ordering = ordering
                });
                result.ValidBevelParentSamples++;
                if (outside > 0f)
                {
                    result.BevelOutsideParentEnvelopeCount++;
                    outsideSum += outside;
                    result.MaximumBevelOutsideParentEnvelopeMagnitude =
                        Mathf.Max(
                            result.MaximumBevelOutsideParentEnvelopeMagnitude,
                            outside);
                }
            }
            result.MeanBevelOutsideParentEnvelopeMagnitude =
                result.BevelOutsideParentEnvelopeCount > 0
                    ? outsideSum /
                        result.BevelOutsideParentEnvelopeCount
                    : 0f;
        }

        private static bool TrySampleProjectedLuma(
            Color[] pixels,
            Color32[] wholeMask,
            Color32[] identityPixels,
            int expectedTriangleIndex,
            Vector3 localPoint,
            Matrix4x4 localToClip,
            bool flipY,
            bool identityFlipRelativeToLighting,
            out float luma)
        {
            luma = 0f;
            if (!Project(localPoint, localToClip, out Vector2 pixel))
            {
                return false;
            }

            double sum = 0.0;
            int count = 0;
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    Vector2 candidate = pixel + new Vector2(x, y);
                    if (!MaskContains(wholeMask, candidate, flipY) ||
                        SampleTriangleIdentityRelative(
                            identityPixels,
                            candidate,
                            flipY,
                            identityFlipRelativeToLighting) !=
                        expectedTriangleIndex)
                    {
                        continue;
                    }
                    sum += SampleLinearLuma(pixels, candidate, flipY);
                    count++;
                }
            }
            if (count == 0)
            {
                return false;
            }
            luma = (float)(sum / count);
            return true;
        }
        private bool ResolveSampleFlipY(
            Subject subject,
            Color32[] wholeMask,
            Color32[] identityPixels,
            bool identityFlipRelativeToLighting,
            Matrix4x4 localToClip)
        {
            int normal = CountProjectedSampleHits(
                subject,
                wholeMask,
                identityPixels,
                identityFlipRelativeToLighting,
                localToClip,
                false);
            int flipped = CountProjectedSampleHits(
                subject,
                wholeMask,
                identityPixels,
                identityFlipRelativeToLighting,
                localToClip,
                true);
            return flipped > normal;
        }
        private static int CountProjectedSampleHits(
            Subject subject,
            Color32[] mask,
            Color32[] identityPixels,
            bool identityFlipRelativeToLighting,
            Matrix4x4 localToClip,
            bool flipY)
        {
            int count = 0;
            foreach (BevelParentGeometrySample sample in
                subject.BevelParentSamples)
            {
                if (Project(sample.Bevel, localToClip, out Vector2 pixel) &&
                    MaskContains(mask, pixel, flipY) &&
                    SampleTriangleIdentityRelative(
                        identityPixels,
                        pixel,
                        flipY,
                        identityFlipRelativeToLighting) ==
                    sample.BevelTriangleIndex)
                {
                    count++;
                }
            }
            return count;
        }

        private static bool TryResolveForegroundAlignment(
            Color32[] identityPixels,
            Color[] lightingPixels,
            out bool identityFlipRelativeToLighting,
            out float intersectionOverUnion,
            out float pixelCountDifferenceRatio,
            out int lightingForegroundPixelCount,
            out int identityForegroundPixelCount)
        {
            identityFlipRelativeToLighting = false;
            intersectionOverUnion = 0f;
            pixelCountDifferenceRatio = 1f;
            lightingForegroundPixelCount = 0;
            identityForegroundPixelCount = 0;
            if (identityPixels == null || lightingPixels == null ||
                identityPixels.Length != lightingPixels.Length ||
                identityPixels.Length != CaptureSize * CaptureSize)
            {
                return false;
            }

            AlignmentScore normal = ScoreForegroundAlignment(
                identityPixels,
                lightingPixels,
                false);
            AlignmentScore flipped = ScoreForegroundAlignment(
                identityPixels,
                lightingPixels,
                true);
            AlignmentScore selected = flipped.IntersectionOverUnion >
                normal.IntersectionOverUnion
                    ? flipped
                    : normal;
            identityFlipRelativeToLighting = selected.FlipIdentity;
            intersectionOverUnion = selected.IntersectionOverUnion;
            pixelCountDifferenceRatio = selected.PixelCountDifferenceRatio;
            lightingForegroundPixelCount = selected.LightingForegroundPixels;
            identityForegroundPixelCount = selected.IdentityForegroundPixels;
            return lightingForegroundPixelCount > 0 &&
                identityForegroundPixelCount > 0 &&
                intersectionOverUnion >= MinimumForegroundAlignmentIoU &&
                pixelCountDifferenceRatio <=
                    MaximumForegroundPixelCountDifferenceRatio;
        }

        private static AlignmentScore ScoreForegroundAlignment(
            Color32[] identityPixels,
            Color[] lightingPixels,
            bool flipIdentity)
        {
            int intersection = 0;
            int union = 0;
            int identityCount = 0;
            int lightingCount = 0;
            for (int lightingIndex = 0;
                 lightingIndex < lightingPixels.Length;
                 lightingIndex++)
            {
                int identityIndex = MapLightingIndexToIdentityIndex(
                    lightingIndex,
                    flipIdentity);
                bool identityForeground = DecodeTriangleIdentity(
                    identityPixels[identityIndex]) >= 0;
                bool lightingForeground =
                    lightingPixels[lightingIndex].a > 0.5f;
                if (identityForeground)
                {
                    identityCount++;
                }
                if (lightingForeground)
                {
                    lightingCount++;
                }
                if (identityForeground && lightingForeground)
                {
                    intersection++;
                }
                if (identityForeground || lightingForeground)
                {
                    union++;
                }
            }
            float iou = union > 0 ? intersection / (float)union : 0f;
            float difference = Mathf.Abs(identityCount - lightingCount) /
                (float)Mathf.Max(1, Mathf.Max(identityCount, lightingCount));
            return new AlignmentScore(
                flipIdentity,
                iou,
                difference,
                lightingCount,
                identityCount);
        }

        private static int MapLightingIndexToIdentityIndex(
            int lightingIndex,
            bool flipIdentity)
        {
            if (!flipIdentity)
            {
                return lightingIndex;
            }
            int x = lightingIndex % CaptureSize;
            int y = lightingIndex / CaptureSize;
            return (CaptureSize - 1 - y) * CaptureSize + x;
        }

        private static Color32[] BuildAlignedIdentityMask(
            Color32[] identityPixels,
            bool flipIdentity)
        {
            Color32[] mask = new Color32[identityPixels.Length];
            Color32 white = new Color32(255, 255, 255, 255);
            for (int lightingIndex = 0;
                 lightingIndex < mask.Length;
                 lightingIndex++)
            {
                int identityIndex = MapLightingIndexToIdentityIndex(
                    lightingIndex,
                    flipIdentity);
                if (DecodeTriangleIdentity(identityPixels[identityIndex]) >= 0)
                {
                    mask[lightingIndex] = white;
                }
            }
            return mask;
        }

        private static int SampleTriangleIdentityRelative(
            Color32[] pixels,
            Vector2 pixel,
            bool projectionFlipY,
            bool identityFlipRelativeToLighting)
        {
            int x = Mathf.Clamp(
                Mathf.RoundToInt(pixel.x),
                0,
                CaptureSize - 1);
            int y = Mathf.Clamp(
                Mathf.RoundToInt(pixel.y),
                0,
                CaptureSize - 1);
            if (projectionFlipY)
            {
                y = CaptureSize - 1 - y;
            }
            int lightingIndex = y * CaptureSize + x;
            int identityIndex = MapLightingIndexToIdentityIndex(
                lightingIndex,
                identityFlipRelativeToLighting);
            return identityIndex >= 0 && identityIndex < pixels.Length
                ? DecodeTriangleIdentity(pixels[identityIndex])
                : TriangleIdentityInvalid;
        }

        private void EvaluateLambertPreflight(CaseResult result)
        {
            result.LambertMeanForegroundLuma = result.MeanMaskedLuma;
            if (lambertStoredNormalPixels == null ||
                lambertStoredNormalPixels.Length != result.LinearPixels.Length ||
                result.LinearPixels.Length != CaptureSize * CaptureSize ||
                !triangleIdentityPixels.TryGetValue(
                    IdentityKey(suspect, result.ViewName),
                    out Color32[] identityPixels))
            {
                result.LambertContractValid = false;
                result.LambertConfiguredNormalizedRmse =
                    float.PositiveInfinity;
                result.LambertOppositeNormalizedRmse =
                    float.PositiveInfinity;
                result.LambertBestFitNormalizedRmse =
                    float.PositiveInfinity;
                lambertStoredNormalPixels = Array.Empty<Color>();
                return;
            }

            Vector3 configuredDirectionWorld =
                suspect.Target.transform.rotation *
                result.LightDirectionLocal.normalized;
            configuredDirectionWorld.Normalize();
            Vector3 oppositeDirectionWorld = -configuredDirectionWorld;

            double configuredSquaredError = 0.0;
            double oppositeSquaredError = 0.0;
            double bestFitNumerator = 0.0;
            double bestFitDenominator = 0.0;
            int validPixels = 0;
            int positiveExpectedPixels = 0;
            int positiveObservedPixels = 0;

            for (int lightingIndex = 0;
                 lightingIndex < result.LinearPixels.Length;
                 lightingIndex++)
            {
                int identityIndex = MapLightingIndexToIdentityIndex(
                    lightingIndex,
                    result.IdentityFlipRelativeToLighting);
                if (identityIndex < 0 ||
                    identityIndex >= identityPixels.Length ||
                    DecodeTriangleIdentity(identityPixels[identityIndex]) < 0)
                {
                    continue;
                }

                int normalLightingIndex = MapLightingIndexToIdentityIndex(
                    identityIndex,
                    lambertStoredNormalIdentityFlipRelativeToLighting);
                if (normalLightingIndex < 0 ||
                    normalLightingIndex >= lambertStoredNormalPixels.Length)
                {
                    continue;
                }

                Color encodedNormal =
                    lambertStoredNormalPixels[normalLightingIndex];
                Color observed = result.LinearPixels[lightingIndex];
                if (encodedNormal.a <= 0.5f || observed.a <= 0.5f)
                {
                    continue;
                }

                Vector3 normalWorld = new Vector3(
                    encodedNormal.r * 2f - 1f,
                    encodedNormal.g * 2f - 1f,
                    encodedNormal.b * 2f - 1f);
                float normalLength = normalWorld.magnitude;
                if (float.IsNaN(normalLength) ||
                    float.IsInfinity(normalLength) ||
                    normalLength < LambertNormalMinimumLength)
                {
                    continue;
                }
                normalWorld /= normalLength;

                float configuredNdotL = Mathf.Max(
                    0f,
                    Vector3.Dot(normalWorld, configuredDirectionWorld));
                float oppositeNdotL = Mathf.Max(
                    0f,
                    Vector3.Dot(normalWorld, oppositeDirectionWorld));
                float configuredExpected = 0.5f * configuredNdotL;
                float oppositeExpected = 0.5f * oppositeNdotL;
                float observedLuma = LinearLuma(observed);

                if (configuredExpected > LambertPositiveResponseThreshold)
                {
                    positiveExpectedPixels++;
                }
                if (observedLuma > LambertPositiveResponseThreshold)
                {
                    positiveObservedPixels++;
                }

                double configuredErrorR = observed.r - configuredExpected;
                double configuredErrorG = observed.g - configuredExpected;
                double configuredErrorB = observed.b - configuredExpected;
                configuredSquaredError +=
                    configuredErrorR * configuredErrorR +
                    configuredErrorG * configuredErrorG +
                    configuredErrorB * configuredErrorB;

                double oppositeErrorR = observed.r - oppositeExpected;
                double oppositeErrorG = observed.g - oppositeExpected;
                double oppositeErrorB = observed.b - oppositeExpected;
                oppositeSquaredError +=
                    oppositeErrorR * oppositeErrorR +
                    oppositeErrorG * oppositeErrorG +
                    oppositeErrorB * oppositeErrorB;

                bestFitNumerator += configuredNdotL * observedLuma;
                bestFitDenominator += configuredNdotL * configuredNdotL;
                validPixels++;
            }

            result.LambertValidNormalPixelCount = validPixels;
            result.LambertPositiveExpectedPixelCount = positiveExpectedPixels;
            result.LambertPositiveObservedPixelCount = positiveObservedPixels;
            result.LambertBestFitScale = bestFitDenominator > 1e-12
                ? (float)(bestFitNumerator / bestFitDenominator)
                : 0f;

            double bestFitSquaredError = 0.0;
            if (validPixels > 0)
            {
                for (int lightingIndex = 0;
                     lightingIndex < result.LinearPixels.Length;
                     lightingIndex++)
                {
                    int identityIndex = MapLightingIndexToIdentityIndex(
                        lightingIndex,
                        result.IdentityFlipRelativeToLighting);
                    if (identityIndex < 0 ||
                        identityIndex >= identityPixels.Length ||
                        DecodeTriangleIdentity(identityPixels[identityIndex]) < 0)
                    {
                        continue;
                    }
                    int normalLightingIndex = MapLightingIndexToIdentityIndex(
                        identityIndex,
                        lambertStoredNormalIdentityFlipRelativeToLighting);
                    if (normalLightingIndex < 0 ||
                        normalLightingIndex >= lambertStoredNormalPixels.Length)
                    {
                        continue;
                    }
                    Color encodedNormal =
                        lambertStoredNormalPixels[normalLightingIndex];
                    Color observed = result.LinearPixels[lightingIndex];
                    if (encodedNormal.a <= 0.5f || observed.a <= 0.5f)
                    {
                        continue;
                    }
                    Vector3 normalWorld = new Vector3(
                        encodedNormal.r * 2f - 1f,
                        encodedNormal.g * 2f - 1f,
                        encodedNormal.b * 2f - 1f);
                    float normalLength = normalWorld.magnitude;
                    if (float.IsNaN(normalLength) ||
                        float.IsInfinity(normalLength) ||
                        normalLength < LambertNormalMinimumLength)
                    {
                        continue;
                    }
                    normalWorld /= normalLength;
                    float ndotl = Mathf.Max(
                        0f,
                        Vector3.Dot(normalWorld, configuredDirectionWorld));
                    float fitted = result.LambertBestFitScale * ndotl;
                    float observedLuma = LinearLuma(observed);
                    double error = observedLuma - fitted;
                    bestFitSquaredError += error * error;
                }
            }

            float normalization = Mathf.Max(
                MinimumLambertMeanForegroundLuma,
                result.LambertMeanForegroundLuma);
            float configuredRmse = validPixels > 0
                ? Mathf.Sqrt((float)(configuredSquaredError /
                    (validPixels * 3.0)))
                : float.PositiveInfinity;
            float oppositeRmse = validPixels > 0
                ? Mathf.Sqrt((float)(oppositeSquaredError /
                    (validPixels * 3.0)))
                : float.PositiveInfinity;
            float bestFitRmse = validPixels > 0
                ? Mathf.Sqrt((float)(bestFitSquaredError / validPixels))
                : float.PositiveInfinity;
            result.LambertConfiguredNormalizedRmse =
                configuredRmse / normalization;
            result.LambertOppositeNormalizedRmse =
                oppositeRmse / normalization;
            result.LambertBestFitNormalizedRmse =
                bestFitRmse / normalization;
            result.LambertContractValid =
                result.LambertValidNormalPixelCount >=
                    MinimumLambertValidNormalPixels &&
                result.LambertPositiveExpectedPixelCount >=
                    MinimumLambertPositiveExpectedPixels &&
                result.LambertPositiveObservedPixelCount >=
                    MinimumLambertPositiveObservedPixels &&
                result.LambertMeanForegroundLuma >=
                    MinimumLambertMeanForegroundLuma &&
                result.LambertConfiguredNormalizedRmse <=
                    MaximumLambertConfiguredNormalizedRmse;
            lambertStoredNormalPixels = Array.Empty<Color>();
        }

        private static string ResolveLambertFailureDiagnosis(
            CaseResult result)
        {
            if (result.LambertOppositeNormalizedRmse <=
                    MaximumLambertConfiguredNormalizedRmse &&
                result.LambertOppositeNormalizedRmse <
                    result.LambertConfiguredNormalizedRmse)
            {
                return "CONTROLLED_LIGHT_DIRECTION_REVERSED";
            }
            if (result.LambertBestFitNormalizedRmse <=
                    MaximumLambertConfiguredNormalizedRmse &&
                Mathf.Abs(result.LambertBestFitScale - 0.5f) > 0.01f)
            {
                return "CONTROLLED_LIGHT_SCALAR_MISMATCH";
            }
            if (result.LambertValidNormalPixelCount <
                MinimumLambertValidNormalPixels)
            {
                return "STORED_NORMAL_PIXEL_COVERAGE_FAILURE";
            }
            return "DIRECT_LIGHT_SHADER_PATH_MISMATCH";
        }

        private static int CountNonFinitePixels(Color[] pixels)
        {
            if (pixels == null)
            {
                return 0;
            }
            int count = 0;
            foreach (Color pixel in pixels)
            {
                if (float.IsNaN(pixel.r) || float.IsInfinity(pixel.r) ||
                    float.IsNaN(pixel.g) || float.IsInfinity(pixel.g) ||
                    float.IsNaN(pixel.b) || float.IsInfinity(pixel.b) ||
                    float.IsNaN(pixel.a) || float.IsInfinity(pixel.a))
                {
                    count++;
                }
            }
            return count;
        }

        private static Color32[] ConvertLinearPixelsToColor32(Color[] pixels)
        {
            if (pixels == null || pixels.Length == 0)
            {
                return Array.Empty<Color32>();
            }
            Color32[] values = new Color32[pixels.Length];
            for (int index = 0; index < pixels.Length; index++)
            {
                Color pixel = pixels[index];
                values[index] = new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.r * 255f), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.g * 255f), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.b * 255f), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(pixel.a * 255f), 0, 255));
            }
            return values;
        }

        private static float LinearLuma(Color value)
        {
            return value.r * 0.2126f +
                value.g * 0.7152f +
                value.b * 0.0722f;
        }

        private static float SampleLinearLuma(
            Color[] pixels,
            Vector2 pixel,
            bool flipY)
        {
            if (pixels == null || pixels.Length == 0)
            {
                return 0f;
            }
            int x = Mathf.Clamp(Mathf.RoundToInt(pixel.x), 0, CaptureSize - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(pixel.y), 0, CaptureSize - 1);
            if (flipY)
            {
                y = CaptureSize - 1 - y;
            }
            return LinearLuma(pixels[y * CaptureSize + x]);
        }

        private static bool TryValidateTriangleIdentity(
            Subject subject,
            Color32[] identityPixels,
            out Color32[] wholeMask,
            out int validPixelCount,
            out int invalidPixelCount,
            out int distinctTriangleCount,
            out int foregroundWidth,
            out int foregroundHeight,
            out int cpuRoundTripFailures)
        {
            wholeMask = new Color32[identityPixels.Length];
            validPixelCount = 0;
            invalidPixelCount = 0;
            foregroundWidth = 0;
            foregroundHeight = 0;
            cpuRoundTripFailures =
                CountTriangleIdentityCpuRoundTripFailures(subject);
            HashSet<int> distinct = new HashSet<int>();
            Color32 white = new Color32(255, 255, 255, 255);
            int triangleCount = subject?.TriangleRecords?.Length ?? 0;
            int minX = CaptureSize;
            int minY = CaptureSize;
            int maxX = -1;
            int maxY = -1;
            for (int index = 0; index < identityPixels.Length; index++)
            {
                Color32 encodedPixel = identityPixels[index];
                int triangleIndex = DecodeTriangleIdentity(encodedPixel);
                if (triangleIndex == TriangleIdentityBackground)
                {
                    continue;
                }
                if (triangleIndex == TriangleIdentityInvalid ||
                    triangleIndex >= triangleCount)
                {
                    invalidPixelCount++;
                    continue;
                }
                wholeMask[index] = white;
                validPixelCount++;
                distinct.Add(triangleIndex);
                int x = index % CaptureSize;
                int y = index / CaptureSize;
                minX = Mathf.Min(minX, x);
                minY = Mathf.Min(minY, y);
                maxX = Mathf.Max(maxX, x);
                maxY = Mathf.Max(maxY, y);
            }
            distinctTriangleCount = distinct.Count;
            if (maxX >= minX && maxY >= minY)
            {
                foregroundWidth = maxX - minX + 1;
                foregroundHeight = maxY - minY + 1;
            }
            return triangleCount > 0 &&
                cpuRoundTripFailures == 0 &&
                validPixelCount >= MinimumIdentityPixels &&
                invalidPixelCount == 0 &&
                distinctTriangleCount >= MinimumIdentityDistinctTriangles &&
                foregroundWidth >= MinimumIdentityExtentPixels &&
                foregroundHeight >= MinimumIdentityExtentPixels;
        }

        private static int DecodeTriangleIdentity(Color32 value)
        {
            if (value.r == 0 && value.g == 0 && value.b == 0)
            {
                return TriangleIdentityBackground;
            }
            if (value.r == 0 || value.g == 0 || value.b == 0)
            {
                return TriangleIdentityInvalid;
            }
            return (value.r - 1) +
                (value.g - 1) * TriangleIdentityRadix +
                (value.b - 1) * TriangleIdentityRadix *
                    TriangleIdentityRadix;
        }

        private static int SampleTriangleIdentity(
            Color32[] pixels,
            Vector2 pixel,
            bool flipY)
        {
            return DecodeTriangleIdentity(Sample(pixels, pixel, flipY));
        }

        private static bool IsMaskPixel(Color32 value)
        {
            return value.r > 96 || value.g > 96 || value.b > 96;
        }

        private static float Luma(Color32 value)
        {
            return (
                value.r * 0.2126f +
                value.g * 0.7152f +
                value.b * 0.0722f) / 255f;
        }

        private static bool Project(
            Vector3 local,
            Matrix4x4 localToClip,
            out Vector2 pixel)
        {
            Vector4 clip = localToClip * new Vector4(local.x, local.y, local.z, 1f);
            if (clip.w <= 0.00001f)
            {
                pixel = default;
                return false;
            }
            float inverseW = 1f / clip.w;
            float x = clip.x * inverseW * 0.5f + 0.5f;
            float y = clip.y * inverseW * 0.5f + 0.5f;
            pixel = new Vector2(x * CaptureSize, y * CaptureSize);
            return x >= -0.05f && x <= 1.05f && y >= -0.05f && y <= 1.05f;
        }

        private static bool MaskContains(Color32[] mask, Vector2 pixel, bool flipY)
        {
            Color32 value = Sample(mask, pixel, flipY);
            return value.r > 96 || value.g > 96 || value.b > 96;
        }

        private static float SampleLuma(Color32[] pixels, Vector2 pixel, bool flipY)
        {
            Color32 value = Sample(pixels, pixel, flipY);
            return
                (value.r * 0.2126f +
                 value.g * 0.7152f +
                 value.b * 0.0722f) / 255f;
        }

        private static Vector3 SampleRgb(
            Color32[] pixels,
            Vector2 pixel,
            bool flipY)
        {
            Color32 value = Sample(pixels, pixel, flipY);
            return new Vector3(value.r, value.g, value.b) / 255f;
        }

        private static Color32 Sample(Color32[] pixels, Vector2 pixel, bool flipY)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt(pixel.x), 0, CaptureSize - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(pixel.y), 0, CaptureSize - 1);
            if (flipY) y = CaptureSize - 1 - y;
            int index = y * CaptureSize + x;
            return index >= 0 && index < pixels.Length
                ? pixels[index]
                : default;
        }

        private GameObject CreateRenderObject(
            RenderCase renderCase,
            out Material material,
            out Mesh temporaryMesh)
        {
            GameObject gameObject = new GameObject(
                "GeneratedMass Surface Causality Render Object")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = auditLayer
            };
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();
            temporaryMesh = null;
            if (renderCase.IsTriangleIdentity)
            {
                if (triangleIdentityShader == null ||
                    !triangleIdentityShader.isSupported ||
                    triangleIdentityShaderCompilerErrors > 0 ||
                    triangleIdentityShaderPassIndex < 0)
                {
                    throw new InvalidOperationException(
                        "Dedicated triangle identity shader failed preflight: " +
                        triangleIdentityShaderLoadStatus);
                }
                temporaryMesh = CreateTriangleIdentityMesh(
                    renderCase.MeshSubject);
                filter.sharedMesh = temporaryMesh;
                material = new Material(triangleIdentityShader)
                {
                    name = "Generated Mass Triangle Identity [" +
                        renderCase.ViewName + "]",
                    hideFlags = HideFlags.HideAndDontSave
                };
                float cull = renderCase.SourceMaterial != null &&
                    renderCase.SourceMaterial.HasProperty("_Cull")
                        ? renderCase.SourceMaterial.GetFloat("_Cull")
                        : (float)CullMode.Back;
                material.SetFloat("_Cull", cull);
            }
            else
            {
                filter.sharedMesh = renderCase.MeshSubject.Mesh;
                if (renderCase.SourceMaterial == null)
                {
                    throw new InvalidOperationException(
                        "Render case has no source material: " +
                        renderCase.Name);
                }
                material = new Material(renderCase.SourceMaterial)
                {
                    name = renderCase.SourceMaterial.name +
                        " [Surface Causality " + renderCase.Name + "]",
                    hideFlags = HideFlags.HideAndDontSave
                };
                if (renderCase.CausalityMode != 0 &&
                    !material.HasProperty("_SurfaceCausalityMode"))
                {
                    throw new InvalidOperationException(
                        "Shader does not support surface-causality modes: " +
                        material.shader.name);
                }
                if (renderCase.MaskDebugMode != 0 &&
                    !material.HasProperty("_MaskDebugMode"))
                {
                    throw new InvalidOperationException(
                        "Shader does not support mask-debug modes: " +
                        material.shader.name);
                }
                if (renderCase.CausalityMode != 0)
                {
                    material.EnableKeyword("_SURFACE_CAUSALITY_AUDIT");
                }
                SetFloat(
                    material,
                    "_SurfaceCausalityMode",
                    renderCase.CausalityMode);
                SetFloat(material, "_SurfaceCausalityLightScale", 1f);
                SetFloat(material, "_MaskDebugMode", renderCase.MaskDebugMode);
                foreach (KeyValuePair<string, float> item in
                    renderCase.FloatOverrides)
                {
                    SetFloat(material, item.Key, item.Value);
                }
                foreach (KeyValuePair<string, Color> item in
                    renderCase.ColorOverrides)
                {
                    SetColor(material, item.Key, item.Value);
                }
                foreach (KeyValuePair<string, Texture> item in
                    renderCase.TextureOverrides)
                {
                    SetTexture(material, item.Key, item.Value);
                }
            }

            Vector4 positionColumn =
                renderCase.MeshSubject.CloneLocalToWorld.GetColumn(3);
            gameObject.transform.SetPositionAndRotation(
                new Vector3(
                    positionColumn.x,
                    positionColumn.y,
                    positionColumn.z),
                suspect.Target.transform.rotation);
            gameObject.transform.localScale =
                suspect.Target.transform.lossyScale;

            int materialCount = Mathf.Max(1, filter.sharedMesh.subMeshCount);
            renderer.sharedMaterials = Enumerable
                .Repeat(material, materialCount)
                .ToArray();
            Subject rendererStateSubject =
                renderCase.PropertySubject ?? renderCase.MeshSubject;
            ApplyRendererState(
                renderer,
                rendererStateSubject,
                renderCase.DisableShadows,
                renderCase.DisableLightProbes,
                renderCase.DisableReflectionProbes);
            if (!renderCase.IsTriangleIdentity)
            {
                Subject propertySubject =
                    renderCase.PropertySubject ?? renderCase.MeshSubject;
                Renderer sourceRenderer =
                    propertySubject.Target.GeometryMeshFilter
                        .GetComponent<Renderer>();
                ApplyAuditPropertyBlocks(
                    renderer,
                    sourceRenderer,
                    materialCount,
                    renderCase);
            }
            return gameObject;
        }

        private static void ApplyAuditPropertyBlocks(
            Renderer destination,
            Renderer source,
            int materialCount,
            RenderCase renderCase)
        {
            MaterialPropertyBlock global = new MaterialPropertyBlock();
            if (!renderCase.ClearPropertyBlock && source != null)
            {
                source.GetPropertyBlock(global);
            }
            ApplyAuditOverrides(global, renderCase);
            destination.SetPropertyBlock(global);

            int sourceMaterialCount = source == null ||
                source.sharedMaterials == null
                    ? 0
                    : source.sharedMaterials.Length;
            if (renderCase.ClearPropertyBlock || source == null)
            {
                return;
            }
            for (int materialIndex = 0;
                 materialIndex < materialCount &&
                 materialIndex < sourceMaterialCount;
                 materialIndex++)
            {
                MaterialPropertyBlock indexed = new MaterialPropertyBlock();
                source.GetPropertyBlock(indexed, materialIndex);
                if (indexed.isEmpty)
                {
                    continue;
                }
                ApplyAuditOverrides(indexed, renderCase);
                destination.SetPropertyBlock(indexed, materialIndex);
            }
        }

        private static void ApplyAuditOverrides(
            MaterialPropertyBlock block,
            RenderCase renderCase)
        {
            block.SetFloat(
                Shader.PropertyToID("_SurfaceCausalityMode"),
                renderCase.CausalityMode);
            block.SetFloat(
                Shader.PropertyToID("_SurfaceCausalityLightScale"),
                1f);
            block.SetFloat(
                Shader.PropertyToID("_MaskDebugMode"),
                renderCase.MaskDebugMode);
            foreach (KeyValuePair<string, float> item in
                renderCase.FloatOverrides)
            {
                block.SetFloat(Shader.PropertyToID(item.Key), item.Value);
            }
            foreach (KeyValuePair<string, Color> item in
                renderCase.ColorOverrides)
            {
                block.SetColor(Shader.PropertyToID(item.Key), item.Value);
            }
            foreach (KeyValuePair<string, Texture> item in
                renderCase.TextureOverrides)
            {
                block.SetTexture(Shader.PropertyToID(item.Key), item.Value);
            }
        }

        private static void ApplyRendererState(
            MeshRenderer destination,
            Subject sourceSubject,
            bool disableShadows,
            bool disableLightProbes,
            bool disableReflectionProbes)
        {
            Renderer source =
                sourceSubject?.Target?.GeometryMeshFilter == null
                    ? null
                    : sourceSubject.Target.GeometryMeshFilter.GetComponent<Renderer>();
            if (source == null)
            {
                destination.shadowCastingMode = ShadowCastingMode.On;
                destination.receiveShadows = !disableShadows;
                return;
            }
            destination.shadowCastingMode = disableShadows
                ? ShadowCastingMode.Off
                : source.shadowCastingMode == ShadowCastingMode.ShadowsOnly
                    ? ShadowCastingMode.On
                    : source.shadowCastingMode;
            destination.receiveShadows = !disableShadows && source.receiveShadows;
            destination.lightProbeUsage = disableLightProbes
                ? LightProbeUsage.Off
                : source.lightProbeUsage;
            destination.reflectionProbeUsage = disableReflectionProbes
                ? ReflectionProbeUsage.Off
                : source.reflectionProbeUsage;
            destination.renderingLayerMask = source.renderingLayerMask;
            destination.probeAnchor = source.probeAnchor;
            destination.lightProbeProxyVolumeOverride =
                source.lightProbeProxyVolumeOverride;
            destination.motionVectorGenerationMode =
                source.motionVectorGenerationMode;
            destination.allowOcclusionWhenDynamic =
                source.allowOcclusionWhenDynamic;
        }

        private Camera CreateAuditCamera(
            RenderCase renderCase,
            RenderTexture target,
            out GameObject cameraObject)
        {
            cameraObject = new GameObject("GeneratedMass Surface Causality Camera")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            Camera camera = cameraObject.AddComponent<Camera>();
            if (sourceCamera != null)
            {
                camera.CopyFrom(sourceCamera);
                CopyUniversalCameraData(sourceCamera, camera);
            }
            auditCameraNormalization =
                NormalizeUniversalAuditCamera(camera);
            camera.enabled = false;
            camera.targetTexture = target;
            camera.aspect = 1f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            camera.gameObject.layer = auditLayer;
            camera.cullingMask = 1 << auditLayer;
            camera.allowMSAA = false;
            camera.allowDynamicResolution = false;
            if (renderCase.IsTriangleIdentity)
            {
                camera.allowHDR = false;
                SetRenderPostProcessing(camera, false);
            }
            else
            {
                camera.allowHDR = true;
            }
            camera.transform.rotation =
                Quaternion.AngleAxis(
                    renderCase.CameraAzimuthDegrees,
                    Vector3.up) * cameraRotation;
            if (camera.orthographic)
            {
                camera.orthographicSize = cameraRadius * 1.25f;
                camera.transform.position = cameraCenter - camera.transform.forward * (cameraRadius * 4f + 1f);
            }
            else
            {
                camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 20f, 70f);
                float distance =
                    cameraRadius /
                    Mathf.Max(0.1f, Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f)) *
                    1.30f;
                camera.transform.position = cameraCenter - camera.transform.forward * distance;
                camera.nearClipPlane = Mathf.Max(0.001f, distance - cameraRadius * 2.5f);
                camera.farClipPlane = distance + cameraRadius * 4f + 10f;
            }
            if (renderCase.DisablePost) SetRenderPostProcessing(camera, false);
            return camera;
        }

        private static List<RendererState> SuppressSceneRenderers()
        {
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include);
            List<RendererState> states = new List<RendererState>(renderers.Length);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                states.Add(new RendererState(renderer, renderer.forceRenderingOff));
                renderer.forceRenderingOff = true;
            }
            return states;
        }

        private static int ResolveAuditLayer(
            int sourceLayer,
            out bool requiresSuppression)
        {
            bool[] occupied = new bool[32];
            Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
                FindObjectsInactive.Include);
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                    occupied[Mathf.Clamp(renderer.gameObject.layer, 0, 31)] = true;
            }
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include);
            int sourceBit = 1 << Mathf.Clamp(sourceLayer, 0, 31);
            for (int candidate = 31; candidate >= 0; candidate--)
            {
                if (occupied[candidate]) continue;
                int candidateBit = 1 << candidate;
                bool sameLightSignature = true;
                foreach (Light light in lights)
                {
                    if (light == null) continue;
                    bool sourceIncluded = (light.cullingMask & sourceBit) != 0;
                    bool candidateIncluded =
                        (light.cullingMask & candidateBit) != 0;
                    if (sourceIncluded == candidateIncluded) continue;
                    sameLightSignature = false;
                    break;
                }
                if (!sameLightSignature) continue;
                requiresSuppression = false;
                return candidate;
            }
            requiresSuppression = true;
            return Mathf.Clamp(sourceLayer, 0, 31);
        }

        private static SceneDirtySnapshot CaptureSceneDirtySnapshot()
        {
            List<SceneDirtyRecord> records = new();
            for (int index = 0; index < SceneManager.sceneCount; index++)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (!scene.IsValid() || !scene.isLoaded) continue;
                records.Add(new SceneDirtyRecord(scene, scene.isDirty));
            }
            return new SceneDirtySnapshot(records);
        }

        private void RestoreSceneDirtySnapshot(SceneDirtySnapshot snapshot)
        {
            foreach (SceneDirtyRecord record in snapshot.Records)
            {
                if (!record.Scene.IsValid() ||
                    !record.Scene.isLoaded ||
                    record.WasDirty ||
                    !record.Scene.isDirty)
                {
                    continue;
                }
                if (!TryMarkSceneClean(record.Scene))
                    unresolvedSceneCleanRestorations++;
            }
        }

        private static MethodInfo ResolveSceneCleanMethod()
        {
            const BindingFlags flags =
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic;
            Type[] signature = { typeof(Scene) };
            return
                typeof(EditorSceneManager).GetMethod(
                    "MarkSceneClean",
                    flags,
                    null,
                    signature,
                    null) ??
                typeof(EditorSceneManager).GetMethod(
                    "ClearSceneDirtiness",
                    flags,
                    null,
                    signature,
                    null);
        }

        private static bool TryMarkSceneClean(Scene scene)
        {
            if (markSceneCleanMethod == null) return false;
            try
            {
                markSceneCleanMethod.Invoke(null, new object[] { scene });
                return !scene.isDirty;
            }
            catch
            {
                return false;
            }
        }

        private List<RendererState> SuppressSourceSubjectRenderers()
        {
            List<RendererState> states = new List<RendererState>(2);
            HashSet<Renderer> unique = new HashSet<Renderer>();
            AddSubjectRenderer(suspect, unique, states);
            AddSubjectRenderer(reference, unique, states);
            return states;
        }

        private static void AddSubjectRenderer(
            Subject subject,
            ISet<Renderer> unique,
            ICollection<RendererState> states)
        {
            Renderer renderer =
                subject?.Target?.GeometryMeshFilter == null
                    ? null
                    : subject.Target.GeometryMeshFilter.GetComponent<Renderer>();
            if (renderer == null || !unique.Add(renderer)) return;
            states.Add(new RendererState(
                renderer,
                renderer.forceRenderingOff));
            renderer.forceRenderingOff = true;
        }

        private static void RestoreSceneRenderers(List<RendererState> states)
        {
            if (states == null) return;
            foreach (RendererState state in states)
            {
                if (state.Renderer != null)
                    state.Renderer.forceRenderingOff = state.ForceRenderingOff;
            }
        }

        private LightOverrideSession ApplyLightOverrides(
            RenderCase renderCase)
        {
            Light[] lights = UnityEngine.Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include);
            LightOverrideSession session = new LightOverrideSession
            {
                PreviousSun = RenderSettings.sun
            };
            Light main = RenderSettings.sun;
            if (main == null ||
                !main.isActiveAndEnabled ||
                main.type != LightType.Directional)
            {
                main = lights.FirstOrDefault(x =>
                    x != null &&
                    x.isActiveAndEnabled &&
                    x.type == LightType.Directional);
            }
            foreach (Light light in lights)
            {
                if (light == null) continue;
                session.States.Add(new LightState(light));
            }

            if (renderCase.UseControlledMainLight)
            {
                foreach (Light light in lights)
                {
                    if (light != null) light.enabled = false;
                }
                if (main == null)
                {
                    session.TemporaryLightObject = new GameObject(
                        "GeneratedMass Surface Causality Controlled Light")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    main = session.TemporaryLightObject.AddComponent<Light>();
                }
                main.type = LightType.Directional;
                main.enabled = true;
                main.intensity = renderCase.MainLightIntensity;
                main.color = Color.white;
                main.useColorTemperature = false;
                main.shadows = LightShadows.None;
                main.cookie = null;
                main.cullingMask = 1 << auditLayer;
                main.renderingLayerMask = unchecked((int)uint.MaxValue);
                Vector3 directionWorld =
                    suspect.Target.transform.rotation *
                    renderCase.MainLightDirectionLocal.normalized;
                Vector3 up = Mathf.Abs(Vector3.Dot(
                    directionWorld,
                    Vector3.up)) > 0.98f
                        ? Vector3.forward
                        : Vector3.up;
                main.transform.rotation = Quaternion.LookRotation(
                    -directionWorld,
                    up);
                RenderSettings.sun = main;
                session.ControlledMainLight = main;
                return session;
            }

            foreach (Light light in lights)
            {
                if (light == null) continue;
                if (renderCase.DisableAllLights)
                {
                    light.enabled = false;
                    continue;
                }
                if (renderCase.DisableAdditionalLights && light != main)
                {
                    light.enabled = false;
                }
                if (renderCase.DisableShadows)
                {
                    light.shadows = LightShadows.None;
                }
                if (renderCase.DisableLightCookies)
                {
                    light.cookie = null;
                }
                if (renderCase.HighIntensity)
                {
                    light.intensity *= HighIntensityScale;
                }
            }
            return session;
        }

        private static void RestoreLights(LightOverrideSession session)
        {
            if (session == null) return;
            RenderSettings.sun = session.PreviousSun;
            foreach (LightState state in session.States)
            {
                state.Restore();
            }
            if (session.TemporaryLightObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    session.TemporaryLightObject);
            }
        }

        private static EnvironmentState ApplyEnvironmentOverrides(
            RenderCase renderCase)
        {
            EnvironmentState state = new EnvironmentState(
                RenderSettings.ambientMode,
                RenderSettings.ambientIntensity,
                RenderSettings.ambientLight,
                RenderSettings.ambientSkyColor,
                RenderSettings.ambientEquatorColor,
                RenderSettings.ambientGroundColor,
                RenderSettings.ambientProbe,
                RenderSettings.reflectionIntensity,
                RenderSettings.fog);
            if (renderCase.DisableAmbientEnvironment)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientIntensity = 0f;
                RenderSettings.ambientLight = Color.black;
                RenderSettings.ambientSkyColor = Color.black;
                RenderSettings.ambientEquatorColor = Color.black;
                RenderSettings.ambientGroundColor = Color.black;
                RenderSettings.ambientProbe = new SphericalHarmonicsL2();
            }
            if (renderCase.DisableReflectionEnvironment)
            {
                RenderSettings.reflectionIntensity = 0f;
            }
            if (renderCase.DisableFog)
            {
                RenderSettings.fog = false;
            }
            return state;
        }

        private static void RestoreEnvironment(EnvironmentState state)
        {
            RenderSettings.ambientMode = state.AmbientMode;
            RenderSettings.ambientIntensity = state.AmbientIntensity;
            RenderSettings.ambientLight = state.AmbientLight;
            RenderSettings.ambientSkyColor = state.AmbientSkyColor;
            RenderSettings.ambientEquatorColor = state.AmbientEquatorColor;
            RenderSettings.ambientGroundColor = state.AmbientGroundColor;
            RenderSettings.ambientProbe = state.AmbientProbe;
            RenderSettings.reflectionIntensity = state.ReflectionIntensity;
            RenderSettings.fog = state.FogEnabled;
        }

        private void ConfigureSubjectTransform(Subject subject)
        {
            subject.LocalBounds = subject.Mesh != null
                ? subject.Mesh.bounds
                : new Bounds(Vector3.zero, Vector3.one);
            Vector3 scale = suspect.Target.transform.lossyScale;
            Quaternion rotation = suspect.Target.transform.rotation;
            Vector3 scaledCenter = Vector3.Scale(subject.LocalBounds.center, scale);
            Vector3 position = cameraCenter - rotation * scaledCenter;
            subject.CloneLocalToWorld = Matrix4x4.TRS(position, rotation, scale);
        }

        private static Vector3 ResolveCommonCenter(Subject subject)
        {
            Renderer renderer = subject.Target != null && subject.Target.GeometryMeshFilter != null
                ? subject.Target.GeometryMeshFilter.GetComponent<Renderer>()
                : null;
            return renderer != null
                ? renderer.bounds.center
                : subject.Target.transform.TransformPoint(subject.Mesh.bounds.center);
        }

        private static float ResolveCommonRadius(
            Subject a,
            Subject b,
            Vector3 commonScale)
        {
            float radius = CalculateWorldRadius(a, commonScale);
            if (b != null)
                radius = Mathf.Max(
                    radius,
                    CalculateWorldRadius(b, commonScale));
            return Mathf.Max(0.1f, radius);
        }

        private static float CalculateWorldRadius(
            Subject subject,
            Vector3 commonScale)
        {
            if (subject == null || subject.Mesh == null || subject.Target == null) return 1f;
            Vector3 scaled = Vector3.Scale(
                subject.Mesh.bounds.extents,
                commonScale);
            return scaled.magnitude;
        }

        private static Camera ResolveSourceCamera()
        {
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null && sceneView.camera != null)
                return sceneView.camera;
            if (Camera.main != null) return Camera.main;
            return UnityEngine.Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Exclude).FirstOrDefault();
        }

        private static void BuildSurfaceClassIndices(Subject subject)
        {
            subject.SurfaceClassIndices.Clear();
            foreach (SurfaceClass surfaceClass in ReportedSurfaceClasses)
            {
                subject.SurfaceClassIndices[surfaceClass] = new List<int>();
            }

            if (subject.Build == null || subject.Mesh == null)
            {
                return;
            }

            int[] uploadedIndices = subject.Mesh.triangles;
            int uploadedTriangles = uploadedIndices.Length / 3;
            subject.TriangleClasses = Enumerable.Repeat(
                SurfaceClass.Unclassified,
                uploadedTriangles).ToArray();
            subject.TriangleRecords =
                new MassGenerator.FinalTriangleRecord[uploadedTriangles];
            bool triangleIndexContractValid = true;

            foreach (MassGenerator.FinalTriangleRecord triangle in
                subject.Build.FinalTriangles)
            {
                if (triangle.TriangleIndex < 0 ||
                    triangle.TriangleIndex >= uploadedTriangles ||
                    subject.TriangleRecords[triangle.TriangleIndex] != null)
                {
                    triangleIndexContractValid = false;
                    continue;
                }
                int uploadedOffset = triangle.TriangleIndex * 3;
                if (uploadedOffset < 0 ||
                    uploadedOffset + 2 >= uploadedIndices.Length ||
                    uploadedIndices[uploadedOffset] != triangle.IndexA ||
                    uploadedIndices[uploadedOffset + 1] != triangle.IndexB ||
                    uploadedIndices[uploadedOffset + 2] != triangle.IndexC)
                {
                    triangleIndexContractValid = false;
                    continue;
                }
                subject.TriangleRecords[triangle.TriangleIndex] = triangle;
                SurfaceClass surfaceClass = ClassifySurface(triangle);
                subject.TriangleClasses[triangle.TriangleIndex] = surfaceClass;
                List<int> classIndices = subject.SurfaceClassIndices[surfaceClass];
                classIndices.Add(triangle.IndexA);
                classIndices.Add(triangle.IndexB);
                classIndices.Add(triangle.IndexC);

                List<int> wholeIndices =
                    subject.SurfaceClassIndices[SurfaceClass.WholeObject];
                wholeIndices.Add(triangle.IndexA);
                wholeIndices.Add(triangle.IndexB);
                wholeIndices.Add(triangle.IndexC);
            }

            subject.ClassifiedTriangleCount =
                subject.SurfaceClassIndices
                    .Where(pair =>
                        pair.Key != SurfaceClass.WholeObject &&
                        pair.Key != SurfaceClass.Unclassified)
                    .Sum(pair => pair.Value.Count / 3);
            subject.UnclassifiedTriangleCount =
                subject.SurfaceClassIndices[SurfaceClass.Unclassified].Count / 3;
            int recordedTriangles = subject.Build.FinalTriangles.Count;
            subject.SurfaceClassContractValid =
                triangleIndexContractValid &&
                subject.TriangleRecords.All(item => item != null) &&
                subject.ClassifiedTriangleCount +
                    subject.UnclassifiedTriangleCount == recordedTriangles &&
                recordedTriangles == uploadedTriangles &&
                subject.SurfaceClassIndices[SurfaceClass.WholeObject].Count ==
                    uploadedTriangles * 3;
        }

        private static SurfaceClass ClassifySurface(
            MassGenerator.FinalTriangleRecord triangle)
        {
            if (triangle == null)
            {
                return SurfaceClass.Unclassified;
            }
            if (triangle.IsOrdinaryBevel ||
                string.Equals(
                    triangle.ProvenanceKindName,
                    "EdgeBevelPlane",
                    StringComparison.Ordinal) ||
                string.Equals(
                    triangle.ProvenanceKindName,
                    "BoundedEdgeBevel",
                    StringComparison.Ordinal))
            {
                return SurfaceClass.OrdinaryBevel;
            }
            if (string.Equals(
                triangle.ProvenanceKindName,
                "SourceFace",
                StringComparison.Ordinal))
            {
                return SurfaceClass.SourceFace;
            }
            if (string.Equals(
                    triangle.ProvenanceKindName,
                    "VertexJunctionPlane",
                    StringComparison.Ordinal) ||
                string.Equals(
                    triangle.ProvenanceKindName,
                    "BoundedEndpointCap",
                    StringComparison.Ordinal))
            {
                return SurfaceClass.JunctionOrEndpointCap;
            }
            if (string.Equals(
                triangle.ProvenanceKindName,
                "CornerDamageCap",
                StringComparison.Ordinal))
            {
                return SurfaceClass.CornerDamage;
            }
            return SurfaceClass.Unclassified;
        }

        private static void BuildBevelParentSamples(Subject subject)
        {
            subject.BevelParentSamples.Clear();
            if (subject.Build == null)
            {
                return;
            }

            List<MassGenerator.FinalTriangleRecord> sourceFaces =
                subject.Build.FinalTriangles
                    .Where(triangle =>
                        ClassifySurface(triangle) == SurfaceClass.SourceFace &&
                        !triangle.TriangleStructurallyInvalid)
                    .ToList();

            foreach (KeyValuePair<int, MassGenerator.LogicalBevelRecord> pair in
                subject.Build.LogicalBevels.OrderBy(item => item.Key))
            {
                MassGenerator.LogicalBevelRecord logical = pair.Value;
                List<MassGenerator.FinalTriangleRecord> bevelTriangles =
                    subject.Build.FinalTriangles
                        .Where(triangle =>
                            triangle.LogicalBevelId == logical.LogicalBevelId &&
                            triangle.IsOrdinaryBevel &&
                            !triangle.TriangleStructurallyInvalid)
                        .ToList();
                if (bevelTriangles.Count == 0)
                {
                    continue;
                }

                Vector3 edge = logical.SourceB - logical.SourceA;
                float edgeLengthSquared = edge.sqrMagnitude;
                List<MassGenerator.FinalTriangleRecord> ordered =
                    bevelTriangles
                        .OrderBy(triangle =>
                            edgeLengthSquared <= 1e-12f
                                ? 0f
                                : Vector3.Dot(
                                    TriangleCentroid(triangle) - logical.SourceA,
                                    edge) / edgeLengthSquared)
                        .ThenBy(triangle => triangle.TriangleIndex)
                        .ToList();
                foreach (int selectedIndex in SelectRepresentativeIndices(
                    ordered.Count))
                {
                    MassGenerator.FinalTriangleRecord bevel =
                        ordered[selectedIndex];
                    Vector3 bevelPoint = TriangleCentroid(bevel);
                    MassGenerator.FinalTriangleRecord parentA =
                        FindParentTriangle(
                            sourceFaces,
                            logical.ParentFaceA,
                            logical.ParentNormalA,
                            bevelPoint);
                    MassGenerator.FinalTriangleRecord parentB =
                        FindParentTriangle(
                            sourceFaces,
                            logical.ParentFaceB,
                            logical.ParentNormalB,
                            bevelPoint);
                    if (parentA == null || parentB == null)
                    {
                        continue;
                    }
                    subject.BevelParentSamples.Add(
                        new BevelParentGeometrySample(
                            logical.LogicalBevelId,
                            selectedIndex,
                            parentA.TriangleIndex,
                            bevel.TriangleIndex,
                            parentB.TriangleIndex,
                            TriangleCentroid(parentA),
                            bevelPoint,
                            TriangleCentroid(parentB)));
                }
            }
        }

        private static IEnumerable<int> SelectRepresentativeIndices(int count)
        {
            if (count <= 0)
            {
                yield break;
            }
            if (count <= 3)
            {
                for (int index = 0; index < count; index++)
                {
                    yield return index;
                }
                yield break;
            }

            yield return 0;
            yield return count / 2;
            yield return count - 1;
        }

        private static MassGenerator.FinalTriangleRecord FindParentTriangle(
            IReadOnlyList<MassGenerator.FinalTriangleRecord> sourceFaces,
            int parentFace,
            Vector3 parentNormal,
            Vector3 target)
        {
            MassGenerator.FinalTriangleRecord exact = sourceFaces
                .Where(triangle => triangle.ProvenanceIndex == parentFace)
                .OrderBy(triangle =>
                    (TriangleCentroid(triangle) - target).sqrMagnitude)
                .FirstOrDefault();
            if (exact != null)
            {
                return exact;
            }

            Vector3 normalizedParent = parentNormal.normalized;
            return sourceFaces
                .Where(triangle =>
                    normalizedParent == Vector3.zero ||
                    Vector3.Dot(
                        triangle.RenderNormal.normalized,
                        normalizedParent) >= 0.985f)
                .OrderBy(triangle =>
                    (TriangleCentroid(triangle) - target).sqrMagnitude)
                .FirstOrDefault();
        }

        private static Vector3 TriangleCentroid(
            MassGenerator.FinalTriangleRecord triangle)
        {
            return (triangle.A + triangle.B + triangle.C) / 3f;
        }

        private static Mesh CreateTriangleIdentityMesh(Subject subject)
        {
            Mesh source = subject?.Mesh;
            if (source == null ||
                subject.TriangleRecords.Length == 0 ||
                !subject.SurfaceClassContractValid)
            {
                throw new InvalidOperationException(
                    "Triangle identity requested without an exact " +
                    "uploaded-triangle contract.");
            }

            int[] sourceIndices = source.triangles;
            Vector3[] sourceVertices = source.vertices;
            if (sourceIndices.Length != subject.TriangleRecords.Length * 3)
            {
                throw new InvalidOperationException(
                    "Triangle identity index count does not match the " +
                    "captured final-triangle contract.");
            }

            Vector3[] vertices = new Vector3[sourceIndices.Length];
            Color32[] colors = new Color32[sourceIndices.Length];
            int[] indices = new int[sourceIndices.Length];
            for (int triangleIndex = 0;
                 triangleIndex < subject.TriangleRecords.Length;
                 triangleIndex++)
            {
                MassGenerator.FinalTriangleRecord record =
                    subject.TriangleRecords[triangleIndex];
                int offset = triangleIndex * 3;
                if (record == null ||
                    record.TriangleIndex != triangleIndex ||
                    sourceIndices[offset] != record.IndexA ||
                    sourceIndices[offset + 1] != record.IndexB ||
                    sourceIndices[offset + 2] != record.IndexC)
                {
                    throw new InvalidOperationException(
                        "Triangle identity uploaded-index contract failed at " +
                        triangleIndex + ".");
                }
                Color32 identity = EncodeTriangleIdentity(triangleIndex);
                for (int corner = 0; corner < 3; corner++)
                {
                    int destinationIndex = offset + corner;
                    int sourceIndex = sourceIndices[destinationIndex];
                    if (sourceIndex < 0 ||
                        sourceIndex >= sourceVertices.Length)
                    {
                        throw new InvalidOperationException(
                            "Triangle identity source index is out of range.");
                    }
                    vertices[destinationIndex] = sourceVertices[sourceIndex];
                    colors[destinationIndex] = identity;
                    indices[destinationIndex] = destinationIndex;
                }
            }

            Mesh identityMesh = new Mesh
            {
                name = source.name + " [Triangle Identity]",
                hideFlags = HideFlags.HideAndDontSave,
                indexFormat = sourceIndices.Length > ushort.MaxValue
                    ? IndexFormat.UInt32
                    : IndexFormat.UInt16
            };
            identityMesh.vertices = vertices;
            identityMesh.colors32 = colors;
            identityMesh.SetIndices(
                indices,
                MeshTopology.Triangles,
                0,
                false);
            identityMesh.bounds = source.bounds;
            return identityMesh;
        }

        private static Color32 EncodeTriangleIdentity(int triangleIndex)
        {
            if (triangleIndex < 0 || triangleIndex > TriangleIdentityMaximum)
            {
                throw new InvalidOperationException(
                    "Triangle identity exceeds the nonzero base-255 audit " +
                    "encoding range.");
            }
            int remaining = triangleIndex;
            byte red = (byte)(remaining % TriangleIdentityRadix + 1);
            remaining /= TriangleIdentityRadix;
            byte green = (byte)(remaining % TriangleIdentityRadix + 1);
            remaining /= TriangleIdentityRadix;
            byte blue = (byte)(remaining % TriangleIdentityRadix + 1);
            return new Color32(red, green, blue, 255);
        }

        private static int CountTriangleIdentityCpuRoundTripFailures(
            Subject subject)
        {
            int triangleCount = subject?.TriangleRecords?.Length ?? 0;
            int failures = 0;
            for (int triangleIndex = 0;
                 triangleIndex < triangleCount;
                 triangleIndex++)
            {
                if (DecodeTriangleIdentity(
                        EncodeTriangleIdentity(triangleIndex)) != triangleIndex)
                {
                    failures++;
                }
            }
            return failures;
        }

        private int CountDistinctIdentityTriangles(
            Subject subject,
            string viewName)
        {
            if (!triangleIdentityPixels.TryGetValue(
                    IdentityKey(subject, viewName),
                    out Color32[] pixels))
            {
                return 0;
            }
            HashSet<int> values = new HashSet<int>();
            foreach (Color32 pixel in pixels)
            {
                int triangleIndex = DecodeTriangleIdentity(pixel);
                if (triangleIndex >= 0 &&
                    triangleIndex < subject.TriangleRecords.Length)
                {
                    values.Add(triangleIndex);
                }
            }
            return values.Count;
        }

        private int CountEligibleIdentityTriangles(
            Subject subject,
            string viewName)
        {
            if (!triangleIdentityPixels.TryGetValue(
                    IdentityKey(subject, viewName),
                    out Color32[] pixels))
            {
                return 0;
            }
            Dictionary<int, int> counts = new Dictionary<int, int>();
            foreach (Color32 pixel in pixels)
            {
                int triangleIndex = DecodeTriangleIdentity(pixel);
                if (triangleIndex < 0 ||
                    triangleIndex >= subject.TriangleRecords.Length)
                {
                    continue;
                }
                counts.TryGetValue(triangleIndex, out int count);
                counts[triangleIndex] = count + 1;
            }
            return counts.Count(item =>
                item.Value >= MinimumTrianglePixels);
        }

        private static void ResolveParentFaceIds(
            Subject subject,
            int logicalBevelId,
            out int parentFaceA,
            out int parentFaceB)
        {
            parentFaceA = -1;
            parentFaceB = -1;
            if (subject?.Build == null || logicalBevelId < 0 ||
                !subject.Build.LogicalBevels.TryGetValue(
                    logicalBevelId,
                    out MassGenerator.LogicalBevelRecord bevel))
            {
                return;
            }
            parentFaceA = bevel.ParentFaceA;
            parentFaceB = bevel.ParentFaceB;
        }

        private static int ResolveShaderPassIndex(
            Shader shader,
            string passName)
        {
            if (shader == null)
            {
                return -1;
            }
            Material material = null;
            try
            {
                material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                return material.FindPass(passName);
            }
            finally
            {
                if (material != null)
                {
                    UnityEngine.Object.DestroyImmediate(material);
                }
            }
        }

        private static int CountShaderCompilerErrors(Shader shader)
        {
            if (shader == null)
            {
                return 1;
            }
            try
            {
                MethodInfo method = typeof(ShaderUtil).GetMethod(
                    "GetShaderMessages",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(Shader) },
                    null);
                object messages = method?.Invoke(null, new object[] { shader });
                Array array = messages as Array;
                if (array == null)
                {
                    return 0;
                }
                int errors = 0;
                foreach (object message in array)
                {
                    if (message == null)
                    {
                        continue;
                    }
                    PropertyInfo severityProperty = message.GetType()
                        .GetProperty(
                            "severity",
                            BindingFlags.Public | BindingFlags.Instance);
                    string severity = severityProperty?.GetValue(message)
                        ?.ToString();
                    if (string.Equals(
                            severity,
                            "Error",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        errors++;
                    }
                }
                return errors;
            }
            catch
            {
                return 1;
            }
        }

        internal static void BuildInternalEdges(Subject subject)
        {
            subject.InternalEdges.Clear();
            if (subject.Build == null) return;
            foreach (IGrouping<int, MassGenerator.FinalTriangleRecord> bevel in
                subject.Build.FinalTriangles
                    .Where(x =>
                        x.IsOrdinaryBevel &&
                        x.LogicalBevelId >= 0 &&
                        !x.TriangleStructurallyInvalid)
                    .GroupBy(x => x.LogicalBevelId))
            {
                Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>> edges = new();
                foreach (MassGenerator.FinalTriangleRecord triangle in bevel)
                {
                    AddEdge(edges, triangle, triangle.A, triangle.B);
                    AddEdge(edges, triangle, triangle.B, triangle.C);
                    AddEdge(edges, triangle, triangle.C, triangle.A);
                }
                foreach (KeyValuePair<EdgeKey, List<MassGenerator.FinalTriangleRecord>> pair in edges)
                {
                    if (pair.Value.Count != 2) continue;
                    subject.InternalEdges.Add(new InternalEdge(
                        pair.Key.A.Position,
                        pair.Key.B.Position,
                        pair.Value[0].GeometricNormal,
                        pair.Value[1].GeometricNormal));
                }
            }
        }

        private static void AddEdge(
            Dictionary<EdgeKey, List<MassGenerator.FinalTriangleRecord>> edges,
            MassGenerator.FinalTriangleRecord triangle,
            Vector3 a,
            Vector3 b)
        {
            EdgeKey key = new EdgeKey(a, b);
            if (!edges.TryGetValue(key, out List<MassGenerator.FinalTriangleRecord> list))
            {
                list = new List<MassGenerator.FinalTriangleRecord>(2);
                edges.Add(key, list);
            }
            list.Add(triangle);
        }

        private static void SetFloat(Material material, string name, float value)
        {
            if (material != null && material.HasProperty(name))
                material.SetFloat(name, value);
        }

        private static void SetColor(Material material, string name, Color value)
        {
            if (material != null && material.HasProperty(name))
                material.SetColor(name, value);
        }

        private static void SetTexture(
            Material material,
            string name,
            Texture value)
        {
            if (material != null && material.HasProperty(name))
                material.SetTexture(name, value);
        }

        private static void CopyUniversalCameraData(Camera source, Camera destination)
        {
            Type type = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (type == null) return;
            Component sourceData = source.GetComponent(type);
            if (sourceData == null) return;
            Component destinationData = destination.gameObject.AddComponent(type);
            EditorUtility.CopySerialized(sourceData, destinationData);
        }

        private static string NormalizeUniversalAuditCamera(Camera camera)
        {
            Type type = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (type == null || camera == null) return "urp-data-unavailable";
            Component data = camera.GetComponent(type);
            if (data == null) return "urp-data-absent";

            bool baseApplied = false;
            bool stackCleared = false;
            try
            {
                PropertyInfo renderType = type.GetProperty(
                    "renderType",
                    BindingFlags.Public | BindingFlags.Instance);
                if (renderType != null && renderType.CanWrite &&
                    renderType.PropertyType.IsEnum)
                {
                    object baseValue = Enum.Parse(
                        renderType.PropertyType,
                        "Base",
                        true);
                    renderType.SetValue(data, baseValue);
                    baseApplied = true;
                }

                PropertyInfo cameraStack = type.GetProperty(
                    "cameraStack",
                    BindingFlags.Public | BindingFlags.Instance);
                object stackValue = cameraStack?.GetValue(data);
                if (stackValue is System.Collections.IList stack)
                {
                    stack.Clear();
                    stackCleared = stack.Count == 0;
                }
                else
                {
                    stackCleared = cameraStack == null;
                }
            }
            catch (Exception exception)
            {
                return "failed:" + exception.GetType().Name;
            }

            return
                "base=" + (baseApplied ? 1 : 0) +
                ",stackCleared=" + (stackCleared ? 1 : 0);
        }

        private static void SetRenderPostProcessing(Camera camera, bool enabled)
        {
            Type type = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (type == null) return;
            Component data = camera.GetComponent(type);
            if (data == null) return;
            PropertyInfo property = type.GetProperty(
                "renderPostProcessing",
                BindingFlags.Public | BindingFlags.Instance);
            property?.SetValue(data, enabled);
        }

        private static string ReadRenderPostProcessing(Camera camera)
        {
            Type type = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (type == null || camera == null) return "unknown";
            Component data = camera.GetComponent(type);
            if (data == null) return "0";
            PropertyInfo property = type.GetProperty(
                "renderPostProcessing",
                BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return "unknown";
            object value = property.GetValue(data);
            return value is bool enabled && enabled ? "1" : "0";
        }

        private static string MaskKey(
            Subject subject,
            SurfaceClass surfaceClass,
            string viewName)
        {
            return (subject?.Role ?? "<none>") + "|" +
                (viewName ?? "CURRENT") + "|" + surfaceClass;
        }

        private static void BuildExpectedMainDiffuse(Subject subject)
        {
            subject.ExpectedMainDiffuse.Clear();
            foreach (SurfaceClass surfaceClass in ReportedSurfaceClasses)
            {
                subject.ExpectedMainDiffuse[surfaceClass] = 0f;
            }
            Light mainLight = ResolveMainDirectionalLight();
            if (subject.Build == null || mainLight == null)
            {
                return;
            }

            Dictionary<SurfaceClass, double> weightedSums = new();
            Dictionary<SurfaceClass, double> weights = new();
            foreach (SurfaceClass surfaceClass in ReportedSurfaceClasses)
            {
                weightedSums[surfaceClass] = 0.0;
                weights[surfaceClass] = 0.0;
            }

            Matrix4x4 localToWorld = subject.CloneLocalToWorld;
            Matrix4x4 normalMatrix = localToWorld.inverse.transpose;
            Vector3 lightDirection = -mainLight.transform.forward;
            foreach (MassGenerator.FinalTriangleRecord triangle in
                subject.Build.FinalTriangles)
            {
                if (triangle == null || triangle.TriangleStructurallyInvalid)
                {
                    continue;
                }
                SurfaceClass surfaceClass = ClassifySurface(triangle);
                Vector3 normalWS = normalMatrix
                    .MultiplyVector(triangle.RenderNormal)
                    .normalized;
                Vector3 a = localToWorld.MultiplyPoint3x4(triangle.A);
                Vector3 b = localToWorld.MultiplyPoint3x4(triangle.B);
                Vector3 c = localToWorld.MultiplyPoint3x4(triangle.C);
                double area = Vector3.Cross(b - a, c - a).magnitude * 0.5;
                if (area <= 1e-12 || normalWS == Vector3.zero)
                {
                    continue;
                }
                double response = Mathf.Max(
                    0f,
                    Vector3.Dot(normalWS, lightDirection));
                weightedSums[surfaceClass] += response * area;
                weights[surfaceClass] += area;
                weightedSums[SurfaceClass.WholeObject] += response * area;
                weights[SurfaceClass.WholeObject] += area;
            }

            foreach (SurfaceClass surfaceClass in ReportedSurfaceClasses)
            {
                subject.ExpectedMainDiffuse[surfaceClass] =
                    weights[surfaceClass] > 1e-12
                        ? (float)(weightedSums[surfaceClass] /
                            weights[surfaceClass])
                        : 0f;
            }
        }

        private static Light ResolveMainDirectionalLight()
        {
            Light configured = RenderSettings.sun;
            if (configured != null &&
                configured.enabled &&
                configured.gameObject.activeInHierarchy &&
                configured.type == LightType.Directional)
            {
                return configured;
            }
            return UnityEngine.Object.FindObjectsByType<Light>(
                    FindObjectsInactive.Exclude)
                .FirstOrDefault(light =>
                    light != null &&
                    light.enabled &&
                    light.type == LightType.Directional);
        }

        private static float GetExpectedMainDiffuse(
            Subject subject,
            SurfaceClass surfaceClass)
        {
            return subject != null &&
                subject.ExpectedMainDiffuse.TryGetValue(
                    surfaceClass,
                    out float value)
                        ? value
                        : 0f;
        }

        private static string IdentityKey(
            Subject subject,
            string viewName)
        {
            return subject.Role + ":TriangleIdentity:" +
                (viewName ?? "CURRENT");
        }

        private static string BuildMaterialDiff(Material suspect, Material reference)
        {
            List<string> lines = new();
            lines.Add("suspectMaterial=" + (suspect == null ? "<none>" : suspect.name));
            lines.Add("referenceMaterial=" + (reference == null ? "<none>" : reference.name));
            lines.Add("suspectShader=" + (suspect == null || suspect.shader == null ? "<none>" : suspect.shader.name));
            lines.Add("referenceShader=" + (reference == null || reference.shader == null ? "<none>" : reference.shader.name));
            if (suspect == null || reference == null)
            {
                lines.Add("materialParityAvailable=0");
                return string.Join("\n", lines);
            }
            lines.Add("materialParityAvailable=1");
            lines.Add("renderQueueDiff=" + suspect.renderQueue + "->" + reference.renderQueue);
            lines.Add("enableInstancingDiff=" + (suspect.enableInstancing ? 1 : 0) + "->" + (reference.enableInstancing ? 1 : 0));
            lines.Add("doubleSidedGIDiff=" + (suspect.doubleSidedGI ? 1 : 0) + "->" + (reference.doubleSidedGI ? 1 : 0));
            lines.Add("globalIlluminationFlagsDiff=" +
                suspect.globalIlluminationFlags + "->" +
                reference.globalIlluminationFlags);
            string suspectKeywords = string.Join("/", suspect.shaderKeywords.OrderBy(x => x));
            string referenceKeywords = string.Join("/", reference.shaderKeywords.OrderBy(x => x));
            lines.Add("suspectKeywords=" + suspectKeywords);
            lines.Add("referenceKeywords=" + referenceKeywords);
            string[] passNames =
            {
                "ForwardLit",
                "UniversalForward",
                "ShadowCaster",
                "DepthOnly",
                "DepthNormals",
                "Meta"
            };
            foreach (string passName in passNames)
            {
                lines.Add(
                    "passDiff=" + passName + ":" +
                    (suspect.GetShaderPassEnabled(passName) ? 1 : 0) +
                    "->" +
                    (reference.GetShaderPassEnabled(passName) ? 1 : 0));
            }

            SortedSet<string> properties = new();
            AddShaderProperties(suspect.shader, properties);
            AddShaderProperties(reference.shader, properties);
            foreach (string propertyName in properties)
            {
                string a = ReadMaterialProperty(suspect, propertyName);
                string b = ReadMaterialProperty(reference, propertyName);
                if (!string.Equals(a, b, StringComparison.Ordinal))
                    lines.Add("propertyDiff=" + propertyName + ":" + a + "->" + b);
            }
            return string.Join("\n", lines);
        }

        private static string BuildRendererStateReport(
            string role,
            Subject subject)
        {
            Renderer renderer =
                subject?.Target?.GeometryMeshFilter == null
                    ? null
                    : subject.Target.GeometryMeshFilter.GetComponent<Renderer>();
            if (renderer == null)
                return role + "Renderer=<none>";
            return
                role + "Renderer=" + renderer.name + "\n" +
                role + "HasPropertyBlock=" +
                    (renderer.HasPropertyBlock() ? 1 : 0) + "\n" +
                role + "ShadowCastingMode=" + renderer.shadowCastingMode + "\n" +
                role + "ReceiveShadows=" + (renderer.receiveShadows ? 1 : 0) + "\n" +
                role + "LightProbeUsage=" + renderer.lightProbeUsage + "\n" +
                role + "ReflectionProbeUsage=" + renderer.reflectionProbeUsage + "\n" +
                role + "RenderingLayerMask=" + renderer.renderingLayerMask;
        }

        private static void AddShaderProperties(Shader shader, ISet<string> output)
        {
            if (shader == null) return;
            int count = shader.GetPropertyCount();
            for (int index = 0; index < count; index++)
                output.Add(shader.GetPropertyName(index));
        }

        private static string ReadMaterialProperty(Material material, string name)
        {
            if (material == null || !material.HasProperty(name)) return "<absent>";
            int index = FindShaderProperty(material.shader, name);
            if (index < 0) return "<unknown>";
            ShaderPropertyType type = material.shader.GetPropertyType(index);
            if (type == ShaderPropertyType.Color ||
                type == ShaderPropertyType.Vector)
            {
                Vector4 value = material.GetVector(name);
                return Format(value.x) + "," + Format(value.y) + "," + Format(value.z) + "," + Format(value.w);
            }
            if (type == ShaderPropertyType.Texture)
            {
                Texture texture = material.GetTexture(name);
                string path = texture == null ? "<none>" : AssetDatabase.GetAssetPath(texture);
                if (texture != null && string.IsNullOrEmpty(path))
                {
                    path =
                        "<runtime:" + texture.name +
                        ":" + texture.width + "x" + texture.height +
                        ":" + texture.dimension + ">";
                }
                Vector2 scale = material.GetTextureScale(name);
                Vector2 offset = material.GetTextureOffset(name);
                return path + "@scale=" + Format(scale.x) + "," + Format(scale.y) + "@offset=" + Format(offset.x) + "," + Format(offset.y);
            }
            return Format(material.GetFloat(name));
        }

        private static int FindShaderProperty(Shader shader, string name)
        {
            if (shader == null) return -1;
            int count = shader.GetPropertyCount();
            for (int index = 0; index < count; index++)
                if (string.Equals(shader.GetPropertyName(index), name, StringComparison.Ordinal))
                    return index;
            return -1;
        }

        private static string Format(float value)
        {
            return value.ToString("R", CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            if (waitingForReadback && !pendingRequest.done)
            {
                DeferPendingResourceRelease(
                    pendingRequest,
                    pendingTexture,
                    pendingCameraObject,
                    pendingRenderObject,
                    pendingMaterial,
                    pendingTemporaryMesh);
                pendingTexture = null;
                pendingCameraObject = null;
                pendingRenderObject = null;
                pendingMaterial = null;
                pendingTemporaryMesh = null;
            }
            else
            {
                ReleasePendingRenderResources();
            }
            waitingForReadback = false;
            orientationAlbedoPixels.Clear();
            orientationNdotLPixels.Clear();
        }

        private static void DeferPendingResourceRelease(
            AsyncGPUReadbackRequest request,
            RenderTexture texture,
            GameObject cameraObject,
            GameObject renderObject,
            Material material,
            Mesh mesh)
        {
            EditorApplication.CallbackFunction cleanup = null;
            cleanup = () =>
            {
                if (!request.done)
                {
                    return;
                }
                EditorApplication.update -= cleanup;
                if (cameraObject != null)
                    UnityEngine.Object.DestroyImmediate(cameraObject);
                if (renderObject != null)
                    UnityEngine.Object.DestroyImmediate(renderObject);
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
                if (mesh != null)
                    UnityEngine.Object.DestroyImmediate(mesh);
                if (texture != null)
                {
                    texture.Release();
                    UnityEngine.Object.DestroyImmediate(texture);
                }
            };
            EditorApplication.update += cleanup;
        }

        private sealed class RenderCase
        {
            internal string Name = string.Empty;
            internal Subject MeshSubject;
            internal Subject PropertySubject;
            internal Material SourceMaterial;
            internal string MaterialRole = string.Empty;
            internal string Family = string.Empty;
            internal string PropertyBlockMode = "Preserved";
            internal SurfaceClass MaskClass = SurfaceClass.WholeObject;
            internal bool ClearPropertyBlock;
            internal bool HighIntensity;
            internal bool DisableShadows;
            internal bool DisableAdditionalLights;
            internal bool DisablePost;
            internal bool DisableLightProbes;
            internal bool DisableReflectionProbes;
            internal bool DisableAmbientEnvironment;
            internal bool DisableReflectionEnvironment;
            internal bool DisableLightCookies;
            internal bool DisableAllLights;
            internal bool DisableFog;
            internal bool UseControlledMainLight;
            internal float MainLightIntensity = 1f;
            internal Vector3 MainLightDirectionLocal;
            internal int CausalityMode;
            internal int MaskDebugMode;
            internal bool IsAblation;
            internal bool IsTriangleIdentity;
            internal bool IsLambertPreflight;
            internal bool IsLambertNormalCapture;
            internal bool IsOrientationSweep;
            internal string OrientationKind = string.Empty;
            internal string OrientationStage = string.Empty;
            internal string OrientationAblation = string.Empty;
            internal bool IsBrdfSweep;
            internal bool IsAdaptiveBrdf;
            internal string DirectionName = string.Empty;
            internal string BrdfVariant = string.Empty;
            internal string ViewName = "CURRENT";
            internal float CameraAzimuthDegrees;
            internal Vector3 LightDirectionLocal;
            internal bool IsAuxiliaryIdentity;
            internal bool CountsTowardDecisionTotal = true;
            internal readonly Dictionary<string, float> FloatOverrides = new();
            internal readonly Dictionary<string, Color> ColorOverrides = new();
            internal readonly Dictionary<string, Texture> TextureOverrides = new();
        }

        internal readonly struct InternalEdge
        {
            internal readonly Vector3 A;
            internal readonly Vector3 B;
            internal readonly Vector3 NormalA;
            internal readonly Vector3 NormalB;

            internal InternalEdge(Vector3 a, Vector3 b, Vector3 normalA, Vector3 normalB)
            {
                A = a;
                B = b;
                NormalA = normalA;
                NormalB = normalB;
            }
        }

        private struct FacetScore
        {
            internal bool FlipY;
            internal int TotalEdges;
            internal int FrontFacingEdges;
            internal int ProjectedEdges;
            internal int ValidSamples;
            internal float MeanGradientJump;
            internal float P90GradientJump;
            internal float MaximumGradientJump;
            internal float MeanValueStep;
            internal float MeanRawGradientJump;
            internal float P90RawGradientJump;
            internal float MeanColorGradientJump;
            internal float P90ColorGradientJump;
            internal float Score;
        }

        private readonly struct RendererState
        {
            internal readonly Renderer Renderer;
            internal readonly bool ForceRenderingOff;
            internal RendererState(Renderer renderer, bool forceRenderingOff)
            {
                Renderer = renderer;
                ForceRenderingOff = forceRenderingOff;
            }
        }

        private readonly struct SceneDirtyRecord
        {
            internal readonly Scene Scene;
            internal readonly bool WasDirty;

            internal SceneDirtyRecord(Scene scene, bool wasDirty)
            {
                Scene = scene;
                WasDirty = wasDirty;
            }
        }

        private readonly struct SceneDirtySnapshot
        {
            internal readonly IReadOnlyList<SceneDirtyRecord> Records;

            internal SceneDirtySnapshot(
                IReadOnlyList<SceneDirtyRecord> records)
            {
                Records = records ?? Array.Empty<SceneDirtyRecord>();
            }
        }

        private sealed class LightOverrideSession
        {
            internal readonly List<LightState> States = new();
            internal Light PreviousSun;
            internal Light ControlledMainLight;
            internal GameObject TemporaryLightObject;
        }

        private readonly struct LightState
        {
            internal readonly Light Light;
            internal readonly bool Enabled;
            internal readonly LightType Type;
            internal readonly float Intensity;
            internal readonly Color Color;
            internal readonly bool UseColorTemperature;
            internal readonly float ColorTemperature;
            internal readonly LightShadows Shadows;
            internal readonly Texture Cookie;
            internal readonly int CullingMask;
            internal readonly int RenderingLayerMask;
            internal readonly Quaternion Rotation;

            internal LightState(Light light)
            {
                Light = light;
                Enabled = light.enabled;
                Type = light.type;
                Intensity = light.intensity;
                Color = light.color;
                UseColorTemperature = light.useColorTemperature;
                ColorTemperature = light.colorTemperature;
                Shadows = light.shadows;
                Cookie = light.cookie;
                CullingMask = light.cullingMask;
                RenderingLayerMask = light.renderingLayerMask;
                Rotation = light.transform.rotation;
            }

            internal void Restore()
            {
                if (Light == null) return;
                Light.enabled = Enabled;
                Light.type = Type;
                Light.intensity = Intensity;
                Light.color = Color;
                Light.useColorTemperature = UseColorTemperature;
                Light.colorTemperature = ColorTemperature;
                Light.shadows = Shadows;
                Light.cookie = Cookie;
                Light.cullingMask = CullingMask;
                Light.renderingLayerMask = RenderingLayerMask;
                Light.transform.rotation = Rotation;
            }
        }

        private readonly struct EnvironmentState
        {
            internal readonly AmbientMode AmbientMode;
            internal readonly float AmbientIntensity;
            internal readonly Color AmbientLight;
            internal readonly Color AmbientSkyColor;
            internal readonly Color AmbientEquatorColor;
            internal readonly Color AmbientGroundColor;
            internal readonly SphericalHarmonicsL2 AmbientProbe;
            internal readonly float ReflectionIntensity;
            internal readonly bool FogEnabled;

            internal EnvironmentState(
                AmbientMode ambientMode,
                float ambientIntensity,
                Color ambientLight,
                Color ambientSkyColor,
                Color ambientEquatorColor,
                Color ambientGroundColor,
                SphericalHarmonicsL2 ambientProbe,
                float reflectionIntensity,
                bool fogEnabled)
            {
                AmbientMode = ambientMode;
                AmbientIntensity = ambientIntensity;
                AmbientLight = ambientLight;
                AmbientSkyColor = ambientSkyColor;
                AmbientEquatorColor = ambientEquatorColor;
                AmbientGroundColor = ambientGroundColor;
                AmbientProbe = ambientProbe;
                ReflectionIntensity = reflectionIntensity;
                FogEnabled = fogEnabled;
            }
        }

        private readonly struct AlignmentScore
        {
            internal readonly bool FlipIdentity;
            internal readonly float IntersectionOverUnion;
            internal readonly float PixelCountDifferenceRatio;
            internal readonly int LightingForegroundPixels;
            internal readonly int IdentityForegroundPixels;

            internal AlignmentScore(
                bool flipIdentity,
                float intersectionOverUnion,
                float pixelCountDifferenceRatio,
                int lightingForegroundPixels,
                int identityForegroundPixels)
            {
                FlipIdentity = flipIdentity;
                IntersectionOverUnion = intersectionOverUnion;
                PixelCountDifferenceRatio = pixelCountDifferenceRatio;
                LightingForegroundPixels = lightingForegroundPixels;
                IdentityForegroundPixels = identityForegroundPixels;
            }
        }

        private readonly struct OrientationStageDefinition
        {
            internal readonly string Name;
            internal readonly int AlbedoMode;
            internal readonly int DirectMode;

            internal OrientationStageDefinition(
                string name,
                int albedoMode,
                int directMode)
            {
                Name = name;
                AlbedoMode = albedoMode;
                DirectMode = directMode;
            }
        }

        internal sealed class OrientationStageSummary
        {
            internal string StageName = string.Empty;
            internal int SourcePairComparisons;
            internal int SourcePairInversions;
            internal int IntroducedSourcePairInversions;
            internal int ConditionalBevelComparisons;
            internal int ConditionalBevelEnvelopeViolations;
            internal int IntroducedConditionalBevelViolations;
            internal float SourceOrientationPearson;
            internal float SourceOrientationSpearman;
            internal float MeanDirectToNdotLRatio;
            internal float MeanDirectProductNormalizedRmse;
            internal float ExposureCorrelation;
            internal float CreviceCorrelation;
            internal float DirtCorrelation;
            internal float HeightCorrelation;
            internal float MottleCorrelation;
        }

        internal sealed class OrientationAblationSummary
        {
            internal string AblationName = string.Empty;
            internal int SourcePairInversions;
            internal int ConditionalBevelViolations;
            internal float CombinedError;
            internal float ReductionFromBaseline;
        }

        private readonly struct ViewDefinition
        {
            internal readonly string Name;
            internal readonly float AzimuthDegrees;

            internal ViewDefinition(string name, float azimuthDegrees)
            {
                Name = name;
                AzimuthDegrees = azimuthDegrees;
            }
        }

        private readonly struct BrdfDirectionDefinition
        {
            internal readonly string Name;
            internal readonly Vector3 LocalDirection;

            internal BrdfDirectionDefinition(
                string name,
                Vector3 localDirection)
            {
                Name = name;
                LocalDirection = localDirection.normalized;
            }
        }

        private readonly struct QuantizedPoint : IEquatable<QuantizedPoint>, IComparable<QuantizedPoint>
        {
            internal readonly int X;
            internal readonly int Y;
            internal readonly int Z;
            internal readonly Vector3 Position;
            internal QuantizedPoint(Vector3 position)
            {
                Position = position;
                X = Mathf.RoundToInt(position.x * Quantization);
                Y = Mathf.RoundToInt(position.y * Quantization);
                Z = Mathf.RoundToInt(position.z * Quantization);
            }
            public int CompareTo(QuantizedPoint other)
            {
                int result = X.CompareTo(other.X);
                if (result != 0) return result;
                result = Y.CompareTo(other.Y);
                return result != 0 ? result : Z.CompareTo(other.Z);
            }
            public bool Equals(QuantizedPoint other) => X == other.X && Y == other.Y && Z == other.Z;
            public override bool Equals(object obj) => obj is QuantizedPoint other && Equals(other);
            public override int GetHashCode() => ((X * 397) ^ Y) * 397 ^ Z;
        }

        private readonly struct EdgeKey : IEquatable<EdgeKey>
        {
            internal readonly QuantizedPoint A;
            internal readonly QuantizedPoint B;
            internal EdgeKey(Vector3 a, Vector3 b)
            {
                QuantizedPoint qa = new QuantizedPoint(a);
                QuantizedPoint qb = new QuantizedPoint(b);
                if (qa.CompareTo(qb) <= 0)
                {
                    A = qa;
                    B = qb;
                }
                else
                {
                    A = qb;
                    B = qa;
                }
            }
            public bool Equals(EdgeKey other) => A.Equals(other.A) && B.Equals(other.B);
            public override bool Equals(object obj) => obj is EdgeKey other && Equals(other);
            public override int GetHashCode() => A.GetHashCode() * 397 ^ B.GetHashCode();
        }
    }
}
