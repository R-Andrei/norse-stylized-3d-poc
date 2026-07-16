using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProgrammaticStylized3D.Geometry;
using ProgrammaticStylized3D.Rivers;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Masses.Editor
{
    [CustomEditor(typeof(GeneratedMass))]
    [CanEditMultipleObjects]
    public sealed class GeneratedMassEditor : UnityEditor.Editor
    {
        private static bool sourceEdgeIndexOverlayEnabled;
        private static bool sourceEdgeIndexHighlightSearchEdges = true;
        private static bool sourceEdgeIndexOverlayXRay;
        private static GeneratedMass sourceEdgeIndexOverlayTarget;

        private const string RenderMeshAuditReportFileName =
            "GeneratedMassRenderMeshAudit.txt";
        private const float RenderMeshExtremeTangentMagnitude = 8f;
        private const float RenderMeshMinimumVectorMagnitude = 0.000001f;
        private const float RenderMeshMinimumVectorMagnitudeSqr =
            RenderMeshMinimumVectorMagnitude *
            RenderMeshMinimumVectorMagnitude;
        private const float RenderMeshUnitVectorTolerance = 0.01f;
        private const float RenderMeshDegenerateRelativeArea = 0.00000001f;
        private const float RenderMeshSliverRelativeArea = 0.00001f;
        private const float RenderMeshDegenerateUvDeterminant = 0.0000000001f;
        private const float RenderMeshIllConditionedUvDeterminant = 0.000001f;
        private const int RenderMeshWorstListCapacity = 8;

        private static bool renderMeshAuditDrawWorstTriangle = true;
        private static bool renderMeshAuditXRay;
        private static int renderMeshAuditDrawTriangleOrdinal = -1;
        private static GeneratedMass renderMeshAuditTarget;
        private static RenderMeshAuditResult lastRenderMeshAudit;
        private static GameObject renderMeshProofObject;
        private static GeneratedMass renderMeshProofTarget;
        private static Mesh renderMeshProofSourceMesh;
        private static Mesh renderMeshProofMesh;
        private static Material renderMeshProofMaterial;
        private static MeshRenderer renderMeshProofSourceRenderer;
        private static bool renderMeshProofSourceForceRenderingOff;
        private static RenderMeshProofMode renderMeshProofMode;

        private static readonly int[] EdgeWearBatchShapeSeeds =
        {
            1,
            1112,
            2223,
            3334,
            4445,
            5556,
            6667,
            7778,
            8889,
            9999,
            5727
        };

        private static readonly string[] EdgeWearBatchWidthNames =
        {
            "minimum",
            "default",
            "maximum"
        };

        private static readonly float[] EdgeWearBatchWidths =
        {
            0.05f,
            1f,
            2f
        };

        private const float EdgeWearBatchMinimumWidthScale = 0.25f;

        private enum RenderMeshProofMode
        {
            None,
            NormalTangentRepair,
            Unlit
        }

        private enum EdgeWearMatrixKind
        {
            TopologyViability,
            ArtisticPreviewParity
        }

        private enum EdgeWearValidationSuiteStage
        {
            CurrentPreview,
            TopologyViability,
            ArtisticPreviewParity,
            Complete
        }

        private static EdgeWearViabilityMatrixJob
            activeEdgeWearViabilityMatrixJob;
        private static EdgeWearValidationSuiteJob
            activeEdgeWearValidationSuiteJob;
        private static string lastEdgeWearBatchSummary = string.Empty;
        private static string lastEdgeWearValidationSuiteSummary =
            string.Empty;
        private const string EdgeWearValidationSuiteReportFileName =
            "GeneratedMassEdgeWearValidationSuite.txt";
        private const string EdgeWearArtisticComprehensiveReportFileName =
            "GeneratedMassEdgeWearArtisticComprehensiveAudit.txt";
        private const string EdgeWearArtisticComprehensiveEdgesCsvFileName =
            "GeneratedMassEdgeWearArtisticComprehensiveEdges.csv";
        private const string
            EdgeWearArtisticComprehensiveScenariosCsvFileName =
                "GeneratedMassEdgeWearArtisticComprehensiveScenarios.csv";

        private const string ColdGreyStoneMaterialPath =
            "Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_ColdGrey.mat";
        private const string DarkWetRiverStoneMaterialPath =
            "Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_WetRiver.mat";
        private const string PaleFrostStoneMaterialPath =
            "Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_PaleFrost.mat";
        private const string BlackSacredStoneMaterialPath =
            "Assets/Game/Demo/Materials/Stone/M_PixelStone_HLSL_BlackSacred.mat";

        private static readonly int[] CommonDebugValues =
        {
            (int)StoneSurfaceMaskDebug.None,
            (int)StoneSurfaceMaskDebug.ConvexEdgeWear,
            (int)StoneSurfaceMaskDebug.Exposure,
            (int)StoneSurfaceMaskDebug.CreviceBase,
            (int)StoneSurfaceMaskDebug.DirtDeposit
        };

        private static readonly GUIContent[] CommonDebugLabels =
        {
            new GUIContent("None"),
            new GUIContent("Convex Edge Wear", "Physical generated bevel/chamfer geometry mask carried by UV2.z. This is the correct edge-wear validation view."),
            new GUIContent("Exposure"),
            new GUIContent("Crevice / Base"),
            new GUIContent("Dirt / Deposit")
        };

        private static readonly int[] AdvancedDebugValues =
        {
            (int)StoneSurfaceMaskDebug.None,
            (int)StoneSurfaceMaskDebug.ConvexBoundaryProximity,
            (int)StoneSurfaceMaskDebug.ConcaveBoundaryProximity,
            (int)StoneSurfaceMaskDebug.ConvexBoundarySalienceComposite,
            (int)StoneSurfaceMaskDebug.BoundarySalience,
            (int)StoneSurfaceMaskDebug.BoundaryIdentity,
            (int)StoneSurfaceMaskDebug.ConcaveBoundarySalienceComposite,
            (int)StoneSurfaceMaskDebug.BoundaryAlongCoordinate,
            (int)StoneSurfaceMaskDebug.BoundaryCrossCoordinate,
            (int)StoneSurfaceMaskDebug.BoundaryCoarseModulation,
            (int)StoneSurfaceMaskDebug.BoundaryFineModulation
        };

        private static readonly GUIContent[] AdvancedDebugLabels =
        {
            new GUIContent("None"),
            new GUIContent("Atlas Convex Boundary Proximity", "Legacy generated-mass atlas diagnostic. Use Convex Edge Wear to validate physical bevel geometry."),
            new GUIContent("Atlas Concave Boundary Proximity", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Convex Boundary + Salience", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Salience", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Identity", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Concave Boundary + Salience", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Along Coordinate", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Cross Coordinate", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Coarse Modulation", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Fine Modulation", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry.")
        };

        private sealed class RenderMeshAuditResult
        {
            public GeneratedMass Target;
            public Mesh Mesh;
            public string ObjectName = string.Empty;
            public string EntityId = string.Empty;
            public string MeshName = string.Empty;
            public string Summary = string.Empty;
            public string Report = string.Empty;
            public string WorstReason = "none";
            public int VertexCount;
            public int NormalCount;
            public int TangentCount;
            public int Uv0Count;
            public int ColorCount;
            public int Uv2Count;
            public int TriangleCount;
            public int SubMeshCount;
            public int NonFinitePositions;
            public int PositionOutliers;
            public int MissingOrPartialNormals;
            public int NonFiniteNormals;
            public int ZeroNormals;
            public int NonUnitNormals;
            public int MissingOrPartialTangents;
            public int NonFiniteTangents;
            public int ZeroTangents;
            public int ExtremeTangents;
            public int InvalidTangentHandedness;
            public int MissingOrPartialUv0;
            public int NonFiniteUv0;
            public int MissingOrPartialColors;
            public int NonFiniteColors;
            public int OutOfRangeColors;
            public int MissingOrPartialUv2;
            public int NonFiniteUv2;
            public int InvalidTriangleIndices;
            public int NonFiniteTriangleGeometry;
            public int DegenerateTriangles;
            public int SliverTriangles;
            public int UvDegenerateTriangles;
            public int UvIllConditionedTriangles;
            public int WindingFailures;
            public int NormalAgreementFailures;
            public float MaximumTangentMagnitude;
            public float MaximumPositionDistance;
            public float MedianPositionDistance;
            public float MinimumRelativeArea = float.PositiveInfinity;
            public float MinimumAbsoluteUvDeterminant = float.PositiveInfinity;
            public float MinimumStoredNormalDot = 1f;
            public float MinimumOutwardDot = 1f;
            public int WorstTriangleOrdinal = -1;
            public RenderMeshTriangleAudit WorstTriangle;
            public readonly List<RenderMeshTriangleAudit>
                Triangles = new();
            public readonly List<RenderMeshRankedTriangle>
                WorstUvTriangles = new();
            public readonly List<RenderMeshRankedTriangle>
                WorstTangentTriangles = new();

            public bool ReadFailure;

            public bool HasHardFailure =>
                ReadFailure ||
                NonFinitePositions > 0 ||
                NonFiniteNormals > 0 ||
                ZeroNormals > 0 ||
                NonFiniteTangents > 0 ||
                NonFiniteUv0 > 0 ||
                NonFiniteColors > 0 ||
                NonFiniteUv2 > 0 ||
                InvalidTriangleIndices > 0 ||
                NonFiniteTriangleGeometry > 0 ||
                DegenerateTriangles > 0;

            public bool HasWarning =>
                PositionOutliers > 0 ||
                MissingOrPartialNormals > 0 ||
                NonUnitNormals > 0 ||
                MissingOrPartialTangents > 0 ||
                ZeroTangents > 0 ||
                ExtremeTangents > 0 ||
                InvalidTangentHandedness > 0 ||
                MissingOrPartialUv0 > 0 ||
                MissingOrPartialColors > 0 ||
                OutOfRangeColors > 0 ||
                MissingOrPartialUv2 > 0 ||
                SliverTriangles > 0 ||
                UvDegenerateTriangles > 0 ||
                UvIllConditionedTriangles > 0 ||
                WindingFailures > 0 ||
                NormalAgreementFailures > 0;
        }

        private sealed class RenderMeshTriangleAudit
        {
            public int Ordinal;
            public int IndexA;
            public int IndexB;
            public int IndexC;
            public Vector3 PositionA;
            public Vector3 PositionB;
            public Vector3 PositionC;
            public Vector2 UvA;
            public Vector2 UvB;
            public Vector2 UvC;
            public Vector3 NormalA;
            public Vector3 NormalB;
            public Vector3 NormalC;
            public Vector4 TangentA;
            public Vector4 TangentB;
            public Vector4 TangentC;
            public Color ColorA;
            public Color ColorB;
            public Color ColorC;
            public Vector4 Uv2A;
            public Vector4 Uv2B;
            public Vector4 Uv2C;
            public float MinimumEdgeLength;
            public float MaximumEdgeLength;
            public float DoubleArea;
            public float RelativeArea;
            public float UvDeterminant;
            public Vector3 GeometricNormal;
            public float MinimumNormalDot;
            public float OutwardDot;
            public float MaximumTangentMagnitude;
            public bool HasNonFiniteVertexChannel;
            public bool ZeroNormal;
            public bool Degenerate;
            public bool Sliver;
            public bool UvDegenerate;
            public bool UvIllConditioned;
            public bool WindingFailure;
            public bool NormalAgreementFailure;
        }

        private readonly struct RenderMeshRankedTriangle
        {
            public RenderMeshRankedTriangle(
                int triangleOrdinal,
                float metric)
            {
                TriangleOrdinal = triangleOrdinal;
                Metric = metric;
            }

            public int TriangleOrdinal { get; }
            public float Metric { get; }
        }

        private SerializedProperty coldGreyStoneMaterial;
        private SerializedProperty darkWetRiverStoneMaterial;
        private SerializedProperty paleFrostStoneMaterial;
        private SerializedProperty blackSacredStoneMaterial;
        private SerializedProperty recipe;
        private SerializedProperty regenerateOnValidate;
        private SerializedProperty generationBudget;
        private SerializedProperty customFeatureAtlasResolution;
        private SerializedProperty featureRecipe;
        private SerializedProperty stoneSurfaceProfile;
        private SerializedProperty baseColor;
        private SerializedProperty surfaceMaskDebug;
        private SerializedProperty surfaceMaskBaseLift;
        private SerializedProperty creviceReach;
        private SerializedProperty creviceSmoothness;
        private SerializedProperty creviceBreakup;
        private SerializedProperty dirtCrawlReach;
        private SerializedProperty dirtCoverage;
        private SerializedProperty exposureResponse;
        private SerializedProperty creviceResponse;
        private SerializedProperty baseResponse;
        private SerializedProperty dirtDepositResponse;
        private SerializedProperty exposureTint;
        private SerializedProperty exposureTintStrength;
        private SerializedProperty creviceTint;
        private SerializedProperty creviceTintStrength;
        private SerializedProperty baseTint;
        private SerializedProperty baseTintStrength;
        private SerializedProperty dirtDepositTint;
        private SerializedProperty dirtDepositTintStrength;
        private SerializedProperty overallRockTint;
        private SerializedProperty overallRockTintStrength;
        private SerializedProperty lightingTintInfluence;
        private SerializedProperty edgeWearAmount;
        private SerializedProperty edgeWearWidth;
        private SerializedProperty edgeWearCoverage;
        private SerializedProperty edgeWearSoftness;
        private SerializedProperty edgeWearResponseStrength;
        private SerializedProperty edgeWearBrightnessLift;
        private SerializedProperty edgeWearTint;
        private SerializedProperty edgeWearTintStrength;
        private SerializedProperty edgeWearMacroVariation;
        private SerializedProperty edgeWearMicroVariation;
        private SerializedProperty creaseAmount;
        private SerializedProperty creaseWidth;
        private SerializedProperty creaseLength;
        private SerializedProperty creaseBranching;
        private SerializedProperty creaseSoftness;
        private SerializedProperty riverInteraction;
        private SerializedProperty participation;
        private SerializedProperty staticPressureMode;
        private SerializedProperty staticPressureStrength;
        private SerializedProperty staticPressureContactSharpness;
        private SerializedProperty staticPressureWaveResponse;
        private SerializedProperty staticPressureProfileChangeIntervalMin;
        private SerializedProperty staticPressureProfileChangeIntervalMax;
        private SerializedProperty obstructionWakeMode;
        private SerializedProperty obstructionWakeStrength;
        private SerializedProperty obstructionWakeReach;
        private SerializedProperty obstructionWakeSpread;
        private SerializedProperty obstructionWakeVariation;
        private SerializedProperty impactRippleCollisionMode;
        private bool showExposureFeature = true;
        private bool showBaseContactFeature = true;
        private bool showCreviceShelterFeature = true;
        private bool showDirtDepositFeature = true;
        private bool showEdgeWearFeature;
        private bool showSourceEdgeIndexDebug;
        private bool highlightSourceEdgeSearchEdges = true;
        private bool sourceEdgeIndexDebugXRay;
        private bool showAdvancedFeatureDiagnostics;
        private bool showCreaseDebugFeature;
        private bool showPressureProfile;

        [InitializeOnLoadMethod]
        private static void RegisterSourceEdgeIndexOverlayRenderer()
        {
            SceneView.duringSceneGui -= DrawGlobalSourceEdgeIndexOverlay;
            SceneView.duringSceneGui += DrawGlobalSourceEdgeIndexOverlay;
            AssemblyReloadEvents.beforeAssemblyReload -=
                CancelEdgeWearViabilityMatrixForDomainReload;
            AssemblyReloadEvents.beforeAssemblyReload +=
                CancelEdgeWearViabilityMatrixForDomainReload;
            EditorApplication.quitting -=
                CancelEdgeWearViabilityMatrixForDomainReload;
            EditorApplication.quitting +=
                CancelEdgeWearViabilityMatrixForDomainReload;
        }

        private static void DrawGlobalSourceEdgeIndexOverlay(
            SceneView sceneView)
        {
            if (!sourceEdgeIndexOverlayEnabled || Application.isPlaying ||
                activeEdgeWearViabilityMatrixJob != null ||
                activeEdgeWearValidationSuiteJob != null)
            {
                return;
            }

            GeneratedMass mass = sourceEdgeIndexOverlayTarget;
            if (mass == null)
            {
                sourceEdgeIndexOverlayEnabled = false;
                sourceEdgeIndexOverlayTarget = null;
                return;
            }

            if (!mass.SourceEdgeIndexDebugIsCurrent)
            {
                if (Event.current.type != EventType.Repaint ||
                    GUIUtility.hotControl != 0 ||
                    EditorGUIUtility.editingTextField)
                {
                    return;
                }
                mass.RefreshSourceEdgeIndexDebug();
            }

            DrawSourceEdgeIndexOverlay(
                mass,
                sourceEdgeIndexHighlightSearchEdges,
                sourceEdgeIndexOverlayXRay,
                sceneView);
        }

        private static void SetSourceEdgeIndexOverlayState(
            GeneratedMass mass,
            bool enabled,
            bool highlightSearchEdges,
            bool xRay)
        {
            bool nextEnabled = enabled && mass != null;
            GeneratedMass nextTarget = nextEnabled ? mass : null;
            if (sourceEdgeIndexOverlayEnabled == nextEnabled &&
                sourceEdgeIndexHighlightSearchEdges ==
                    highlightSearchEdges &&
                sourceEdgeIndexOverlayXRay == xRay &&
                sourceEdgeIndexOverlayTarget == nextTarget)
            {
                return;
            }

            sourceEdgeIndexOverlayEnabled = nextEnabled;
            sourceEdgeIndexHighlightSearchEdges =
                highlightSearchEdges;
            sourceEdgeIndexOverlayXRay = xRay;
            sourceEdgeIndexOverlayTarget = nextTarget;
            SceneView.RepaintAll();
        }

        [InitializeOnLoadMethod]
        private static void RegisterRenderMeshProofCleanup()
        {
            AssemblyReloadEvents.beforeAssemblyReload -=
                DestroyRenderMeshProofClone;
            AssemblyReloadEvents.beforeAssemblyReload +=
                DestroyRenderMeshProofClone;
            EditorApplication.quitting -= DestroyRenderMeshProofClone;
            EditorApplication.quitting += DestroyRenderMeshProofClone;
            EditorApplication.playModeStateChanged -=
                HandleRenderMeshProofPlayModeChange;
            EditorApplication.playModeStateChanged +=
                HandleRenderMeshProofPlayModeChange;
        }

        private static void HandleRenderMeshProofPlayModeChange(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode ||
                state == PlayModeStateChange.ExitingPlayMode)
            {
                DestroyRenderMeshProofClone();
            }
        }

        private void OnEnable()
        {
            coldGreyStoneMaterial = serializedObject.FindProperty(
                "coldGreyStoneMaterial");
            darkWetRiverStoneMaterial = serializedObject.FindProperty(
                "darkWetRiverStoneMaterial");
            paleFrostStoneMaterial = serializedObject.FindProperty(
                "paleFrostStoneMaterial");
            blackSacredStoneMaterial = serializedObject.FindProperty(
                "blackSacredStoneMaterial");
            recipe = serializedObject.FindProperty(
                "recipe");
            regenerateOnValidate = serializedObject.FindProperty(
                "regenerateOnValidate");
            generationBudget = serializedObject.FindProperty(
                "generationBudget");
            customFeatureAtlasResolution = serializedObject.FindProperty(
                "customFeatureAtlasResolution");
            featureRecipe = serializedObject.FindProperty(
                "featureRecipe");
            stoneSurfaceProfile = serializedObject.FindProperty(
                "stoneSurfaceProfile");
            baseColor = serializedObject.FindProperty(
                "baseColor");
            surfaceMaskDebug = serializedObject.FindProperty(
                "surfaceMaskDebug");
            surfaceMaskBaseLift = serializedObject.FindProperty(
                "surfaceMaskBaseLift");
            creviceReach = serializedObject.FindProperty(
                "creviceReach");
            creviceSmoothness = serializedObject.FindProperty(
                "creviceSmoothness");
            creviceBreakup = serializedObject.FindProperty(
                "creviceBreakup");
            dirtCrawlReach = serializedObject.FindProperty(
                "dirtCrawlReach");
            dirtCoverage = serializedObject.FindProperty(
                "dirtCoverage");
            exposureResponse = serializedObject.FindProperty(
                "exposureResponse");
            creviceResponse = serializedObject.FindProperty(
                "creviceResponse");
            baseResponse = serializedObject.FindProperty(
                "baseResponse");
            dirtDepositResponse = serializedObject.FindProperty(
                "dirtDepositResponse");
            exposureTint = serializedObject.FindProperty(
                "exposureTint");
            exposureTintStrength = serializedObject.FindProperty(
                "exposureTintStrength");
            creviceTint = serializedObject.FindProperty(
                "creviceTint");
            creviceTintStrength = serializedObject.FindProperty(
                "creviceTintStrength");
            baseTint = serializedObject.FindProperty(
                "baseTint");
            baseTintStrength = serializedObject.FindProperty(
                "baseTintStrength");
            dirtDepositTint = serializedObject.FindProperty(
                "dirtDepositTint");
            dirtDepositTintStrength = serializedObject.FindProperty(
                "dirtDepositTintStrength");
            overallRockTint = serializedObject.FindProperty(
                "overallRockTint");
            overallRockTintStrength = serializedObject.FindProperty(
                "overallRockTintStrength");
            lightingTintInfluence = serializedObject.FindProperty(
                "lightingTintInfluence");
            edgeWearAmount = serializedObject.FindProperty(
                "edgeWearAmount");
            edgeWearWidth = serializedObject.FindProperty(
                "edgeWearWidth");
            edgeWearCoverage = serializedObject.FindProperty(
                "edgeWearCoverage");
            edgeWearSoftness = serializedObject.FindProperty(
                "edgeWearSoftness");
            edgeWearResponseStrength = serializedObject.FindProperty(
                "edgeWearResponseStrength");
            edgeWearBrightnessLift = serializedObject.FindProperty(
                "edgeWearBrightnessLift");
            edgeWearTint = serializedObject.FindProperty(
                "edgeWearTint");
            edgeWearTintStrength = serializedObject.FindProperty(
                "edgeWearTintStrength");
            edgeWearMacroVariation = serializedObject.FindProperty(
                "edgeWearMacroVariation");
            edgeWearMicroVariation = serializedObject.FindProperty(
                "edgeWearMicroVariation");
            creaseAmount = serializedObject.FindProperty(
                "creaseAmount");
            creaseWidth = serializedObject.FindProperty(
                "creaseWidth");
            creaseLength = serializedObject.FindProperty(
                "creaseLength");
            creaseBranching = serializedObject.FindProperty(
                "creaseBranching");
            creaseSoftness = serializedObject.FindProperty(
                "creaseSoftness");
            riverInteraction = serializedObject.FindProperty(
                "riverInteraction");
            participation = riverInteraction?.FindPropertyRelative(
                "participation");
            staticPressureMode = riverInteraction?.FindPropertyRelative(
                "staticPressureMode");
            staticPressureStrength = riverInteraction?.FindPropertyRelative(
                "staticPressureStrength");
            staticPressureContactSharpness =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureContactSharpness");
            staticPressureWaveResponse =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureWaveResponse");
            staticPressureProfileChangeIntervalMin =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureProfileChangeIntervalMin");
            staticPressureProfileChangeIntervalMax =
                riverInteraction?.FindPropertyRelative(
                    "staticPressureProfileChangeIntervalMax");
            obstructionWakeMode = riverInteraction?.FindPropertyRelative(
                "obstructionWakeMode");
            obstructionWakeStrength = riverInteraction?.FindPropertyRelative(
                "obstructionWakeStrength");
            obstructionWakeReach = riverInteraction?.FindPropertyRelative(
                "obstructionWakeReach");
            obstructionWakeSpread = riverInteraction?.FindPropertyRelative(
                "obstructionWakeSpread");
            obstructionWakeVariation = riverInteraction?.FindPropertyRelative(
                "obstructionWakeVariation");
            impactRippleCollisionMode =
                riverInteraction?.FindPropertyRelative(
                    "impactRippleCollisionMode");
        }

        private void OnDisable()
        {
            GeneratedMass mass = target as GeneratedMass;
            if (mass != null && renderMeshProofTarget == mass)
            {
                DestroyRenderMeshProofClone();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EnsureDefaultStoneMaterials();

            DrawFeatureRecipeWorkflow();
            DrawGenerationBudget();
            DrawCoreShapeRecipe();
            DrawRenderingAndProfile();

            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "recipe",
                "regenerateOnValidate",
                "generationBudget",
                "customFeatureAtlasResolution",
                "featureRecipe",
                "stoneSurfaceProfile",
                "baseColor",
                "surfaceMaskDebug",
                "surfaceMaskBaseLift",
                "creviceReach",
                "creviceSmoothness",
                "creviceBreakup",
                "dirtCrawlReach",
                "dirtCoverage",
                "exposureResponse",
                "creviceResponse",
                "baseResponse",
                "dirtDepositResponse",
                "exposureTint",
                "exposureTintStrength",
                "creviceTint",
                "creviceTintStrength",
                "baseTint",
                "baseTintStrength",
                "dirtDepositTint",
                "dirtDepositTintStrength",
                "overallRockTint",
                "overallRockTintStrength",
                "lightingTintInfluence",
                "edgeWearAmount",
                "edgeWearWidth",
                "edgeWearCoverage",
                "edgeWearSoftness",
                "edgeWearResponseStrength",
                "edgeWearBrightnessLift",
                "edgeWearTint",
                "edgeWearTintStrength",
                "edgeWearMacroVariation",
                "edgeWearMicroVariation",
                "creaseAmount",
                "creaseWidth",
                "creaseLength",
                "creaseBranching",
                "creaseSoftness",
                "riverInteraction");

            DrawFeatureStack();
            DrawRockColourAuthority();
            DrawRiverInteraction();

            serializedObject.ApplyModifiedProperties();

            DrawEdgeWearBevelPreview();
            DrawSourceEdgeIndexDebug();
            DrawRenderMeshDiagnostics();

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Variant Controls",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("New Shape"))
            {
                ApplyToTargets(
                    "New Generated Mass Shape",
                    mass => mass.CreateNewShape());
            }

            if (GUILayout.Button("New Surface"))
            {
                ApplyToTargets(
                    "New Generated Mass Surface",
                    mass => mass.CreateNewSurface());
            }

            if (GUILayout.Button("New Variant"))
            {
                ApplyToTargets(
                    "New Generated Mass Variant",
                    mass => mass.CreateNewVariant());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Regenerate"))
            {
                ApplyToTargets(
                    "Regenerate Generated Mass",
                    mass =>
                    {
                        Debug.Log(
                            "GeneratedMass manual regeneration context. " +
                            "object=" + mass.name +
                            ", entityId=" + mass.GetEntityId(),
                            mass);
                        mass.Regenerate();
                    });
            }

            if (GUILayout.Button("Reset to Archetype"))
            {
                ApplyToTargets(
                    "Reset Generated Mass Recipe",
                    mass => mass.ResetRecipeToArchetype());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Shape Seed changes proportions, major cuts and silhouette. " +
                "Surface Seed changes surface triangulation, subtle facet relief " +
                "and vertex-colour variation.",
                MessageType.Info);
        }

        private void DrawEdgeWearBevelPreview()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Edge-Wear Bevel Evaluation",
                EditorStyles.boldLabel);

            bool anyEvaluated = false;
            int evaluatedCount = 0;
            int staleCount = 0;
            int appliedCount = 0;
            int candidates = 0;
            int railSolved = 0;
            int activeEdges = 0;
            int deferredEdges = 0;
            int rejectedEdges = 0;
            int bevelFaces = 0;
            int triangles = 0;
            string firstDiagnostic = string.Empty;

            for (int targetIndex = 0;
                 targetIndex < targets.Length;
                 targetIndex++)
            {
                GeneratedMass mass = targets[targetIndex] as GeneratedMass;
                if (mass == null || !mass.UnifiedEdgeWearPreviewEnabled)
                {
                    continue;
                }

                anyEvaluated = true;
                evaluatedCount++;
                if (mass.UnifiedEdgeWearPreviewStale)
                {
                    staleCount++;
                }
                if (mass.UnifiedEdgeWearPreviewApplied)
                {
                    appliedCount++;
                }
                candidates += mass.UnifiedEdgeWearPreviewCandidateCount;
                railSolved +=
                    mass.UnifiedEdgeWearPreviewRailSolvedEdgeCount;
                activeEdges += mass.UnifiedEdgeWearPreviewActiveEdgeCount;
                deferredEdges +=
                    mass.UnifiedEdgeWearPreviewDeferredEdgeCount;
                rejectedEdges +=
                    mass.UnifiedEdgeWearPreviewRejectedEdgeCount;
                bevelFaces += mass.UnifiedEdgeWearPreviewBevelFaceCount;
                triangles += mass.UnifiedEdgeWearPreviewTriangleCount;
                if (string.IsNullOrEmpty(firstDiagnostic) &&
                    !string.IsNullOrEmpty(
                        mass.UnifiedEdgeWearPreviewDiagnostic))
                {
                    firstDiagnostic =
                        mass.UnifiedEdgeWearPreviewDiagnostic;
                }
            }

            string message;
            MessageType messageType;
            if (!anyEvaluated)
            {
                message =
                    "One authoritative rebuild solves all selected edge widths together, rebuilds the complete rock through the certified all-edge bevel shell, and outputs one cumulative audit record. Every emitted bevel polygon uses the one-planar-surface render contract.";
                messageType = MessageType.Info;
            }
            else if (staleCount > 0)
            {
                message =
                    "The edge-wear preview is out of date for " +
                    staleCount + " object(s). Rebuild to evaluate current settings.";
                messageType = MessageType.Warning;
            }
            else if (appliedCount == evaluatedCount)
            {
                message =
                    "All-edge bevel preview active for " +
                    appliedCount + " object(s): " +
                    activeEdges + " materialized bevels from " +
                    railSolved + " active selected edges and " +
                    candidates + " selected candidates, " +
                    bevelFaces + " one-surface bevel faces, " +
                    triangles + " triangles.";
                if (deferredEdges > 0 || rejectedEdges > 0)
                {
                    message += " Deferred=" + deferredEdges +
                        ", rejected=" + rejectedEdges + ".";
                }
                messageType =
                    deferredEdges > 0 || rejectedEdges > 0
                        ? MessageType.Warning
                        : MessageType.Info;
            }
            else
            {
                message =
                    "All-edge bevel rebuild failed for " +
                    (evaluatedCount - appliedCount) +
                    " object(s). Active selected=" + railSolved +
                    ", materialized=" + activeEdges +
                    ", deferred=" + deferredEdges +
                    ", rejected=" + rejectedEdges + ".";
                if (!string.IsNullOrEmpty(firstDiagnostic))
                {
                    message += " " + firstDiagnostic + ".";
                }
                messageType = MessageType.Error;
            }

            EditorGUILayout.HelpBox(message, messageType);

            using (new EditorGUI.DisabledScope(
                Application.isPlaying ||
                activeEdgeWearViabilityMatrixJob != null))
            {
                if (GUILayout.Button(
                        "Rebuild Edge-Wear Bevel Preview"))
                {
                    for (int targetIndex = 0;
                         targetIndex < targets.Length;
                         targetIndex++)
                    {
                        GeneratedMass mass =
                            targets[targetIndex] as GeneratedMass;
                        mass?.EvaluateUnifiedEdgeWearPreview();
                    }

                    serializedObject.Update();
                    Repaint();
                    SceneView.RepaintAll();
                }
            }

            EditorGUILayout.Space(4f);
            DrawEdgeWearViabilityMatrixControls();

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Explicit edge-wear evaluation is disabled in Play Mode; production geometry is used.",
                    MessageType.None);
            }
        }

        private void DrawEdgeWearViabilityMatrixControls()
        {
            bool matrixRunning =
                activeEdgeWearViabilityMatrixJob != null;
            bool suiteRunning =
                activeEdgeWearValidationSuiteJob != null;
            int matrixCaseCount = EdgeWearBatchShapeSeeds.Length *
                EdgeWearBatchWidths.Length;

            EditorGUILayout.HelpBox(
                "One-click validation rebuilds the current preview, runs " +
                "both canonical " + matrixCaseCount + "-case matrices, " +
                "and writes one combined report. The focused matrix " +
                "buttons remain available when only one audit is needed.",
                MessageType.None);

            using (new EditorGUI.DisabledScope(
                Application.isPlaying ||
                serializedObject.isEditingMultipleObjects ||
                matrixRunning ||
                suiteRunning))
            {
                if (GUILayout.Button(
                        "Run Full Edge-Wear Validation Suite (1 Click)"))
                {
                    GeneratedMass mass = target as GeneratedMass;
                    if (mass != null)
                    {
                        StartEdgeWearValidationSuite(mass);
                    }
                }

                EditorGUILayout.Space(2f);
                if (GUILayout.Button(
                        "Run Topology Viability Matrix (" +
                        matrixCaseCount + " Exhaustive Cases)"))
                {
                    GeneratedMass mass = target as GeneratedMass;
                    if (mass != null)
                    {
                        StartEdgeWearViabilityMatrix(
                            mass,
                            EdgeWearMatrixKind.TopologyViability);
                    }
                }
                if (GUILayout.Button(
                        "Run Artistic Preview Parity Matrix (" +
                        matrixCaseCount + " Cases)"))
                {
                    GeneratedMass mass = target as GeneratedMass;
                    if (mass != null)
                    {
                        StartEdgeWearViabilityMatrix(
                            mass,
                            EdgeWearMatrixKind.ArtisticPreviewParity);
                    }
                }
            }

            if (suiteRunning)
            {
                EdgeWearValidationSuiteJob suite =
                    activeEdgeWearValidationSuiteJob;
                EdgeWearViabilityMatrixJob matrix =
                    activeEdgeWearViabilityMatrixJob;
                string progress = suite.StageDisplayName;
                if (matrix != null)
                {
                    progress += ": case " +
                        (matrix.CompletedCaseCount + 1) + "/" +
                        matrix.TotalCaseCount;
                }
                EditorGUILayout.HelpBox(
                    "Running full edge-wear validation suite — " +
                    progress + ". The current preview is rebuilt once; " +
                    "matrix cases do not modify the selected mass.",
                    MessageType.Info);
                if (GUILayout.Button("Cancel Full Validation Suite"))
                {
                    suite.CancelRequested = true;
                    if (matrix != null)
                    {
                        matrix.CancelRequested = true;
                    }
                }
            }
            else if (matrixRunning)
            {
                EdgeWearViabilityMatrixJob job =
                    activeEdgeWearViabilityMatrixJob;
                EditorGUILayout.HelpBox(
                    "Running " + job.DisplayName + ": case " +
                    (job.CompletedCaseCount + 1) + "/" +
                    job.TotalCaseCount + ". The selected mass and its " +
                    "preview are not modified.",
                    MessageType.Info);
                if (GUILayout.Button("Cancel Matrix"))
                {
                    job.CancelRequested = true;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(
                        lastEdgeWearValidationSuiteSummary))
                {
                    EditorGUILayout.HelpBox(
                        lastEdgeWearValidationSuiteSummary,
                        MessageType.None);
                    string suiteReportPath = GetEdgeWearLibraryPath(
                        EdgeWearValidationSuiteReportFileName);
                    using (new EditorGUI.DisabledScope(
                        !File.Exists(suiteReportPath)))
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (GUILayout.Button(
                                "Copy Full Validation Report"))
                        {
                            EditorGUIUtility.systemCopyBuffer =
                                File.ReadAllText(suiteReportPath);
                        }
                        if (GUILayout.Button("Reveal Full Report"))
                        {
                            EditorUtility.RevealInFinder(suiteReportPath);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
                else if (!string.IsNullOrEmpty(
                             lastEdgeWearBatchSummary))
                {
                    EditorGUILayout.HelpBox(
                        lastEdgeWearBatchSummary,
                        MessageType.None);
                }
            }

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox(
                    "Edge-wear validation requires one selected mass " +
                    "because every audit uses one immutable " +
                    "recipe/settings snapshot.",
                    MessageType.None);
            }
        }

        private static void StartEdgeWearValidationSuite(
            GeneratedMass mass)
        {
            if (mass == null || mass.Recipe == null ||
                activeEdgeWearViabilityMatrixJob != null ||
                activeEdgeWearValidationSuiteJob != null)
            {
                return;
            }

            EdgeWearValidationSuiteJob suite =
                new EdgeWearValidationSuiteJob(mass);
            activeEdgeWearValidationSuiteJob = suite;
            lastEdgeWearBatchSummary = string.Empty;
            lastEdgeWearValidationSuiteSummary = string.Empty;

            try
            {
                suite.Stage = EdgeWearValidationSuiteStage.CurrentPreview;
                suite.PrepareCurrentPreviewCapture();
                mass.EvaluateUnifiedEdgeWearPreview();
                suite.CaptureCurrentPreview();
                suite.Stage =
                    EdgeWearValidationSuiteStage.TopologyViability;
                StartEdgeWearViabilityMatrix(
                    mass,
                    EdgeWearMatrixKind.TopologyViability,
                    true);
            }
            catch (Exception exception)
            {
                FinishEdgeWearValidationSuite(
                    suite,
                    false,
                    "current preview threw " +
                    exception.GetType().Name + ":" +
                    exception.Message);
            }
        }

        private static void StartEdgeWearViabilityMatrix(
            GeneratedMass mass,
            EdgeWearMatrixKind kind,
            bool suiteOwned = false)
        {
            if (mass == null || mass.Recipe == null ||
                activeEdgeWearViabilityMatrixJob != null ||
                (!suiteOwned &&
                 activeEdgeWearValidationSuiteJob != null))
            {
                return;
            }

            activeEdgeWearViabilityMatrixJob =
                new EdgeWearViabilityMatrixJob(
                    mass,
                    kind,
                    suiteOwned);
            lastEdgeWearBatchSummary = string.Empty;
            if (!suiteOwned)
            {
                lastEdgeWearValidationSuiteSummary = string.Empty;
            }
            EditorApplication.update -= AdvanceEdgeWearViabilityMatrix;
            EditorApplication.update += AdvanceEdgeWearViabilityMatrix;
        }

        private static void AdvanceEdgeWearViabilityMatrix()
        {
            EdgeWearViabilityMatrixJob job =
                activeEdgeWearViabilityMatrixJob;
            if (job == null)
            {
                EditorApplication.update -= AdvanceEdgeWearViabilityMatrix;
                EditorUtility.ClearProgressBar();
                return;
            }

            if (job.CancelRequested || job.Target == null)
            {
                FinishEdgeWearViabilityMatrix(
                    job,
                    true,
                    job.Target == null
                        ? "target mass was destroyed"
                        : "cancelled by user");
                return;
            }

            int caseIndex = job.CompletedCaseCount;
            if (caseIndex >= job.TotalCaseCount)
            {
                FinishEdgeWearViabilityMatrix(job, false, string.Empty);
                return;
            }

            int seedIndex = caseIndex / EdgeWearBatchWidths.Length;
            int widthIndex = caseIndex % EdgeWearBatchWidths.Length;
            int shapeSeed = EdgeWearBatchShapeSeeds[seedIndex];
            float width = EdgeWearBatchWidths[widthIndex];
            string widthName = EdgeWearBatchWidthNames[widthIndex];
            float progress = (float)caseIndex / job.TotalCaseCount;
            if (EditorUtility.DisplayCancelableProgressBar(
                    job.ProgressTitle,
                    "Seed " + shapeSeed + ", " + widthName +
                    " width (case " + (caseIndex + 1) + "/" +
                    job.TotalCaseCount + ")",
                    progress))
            {
                FinishEdgeWearViabilityMatrix(
                    job,
                    true,
                    "cancelled by user");
                return;
            }

            EdgeWearViabilityMatrixCase matrixCase =
                EvaluateEdgeWearViabilityMatrixCase(
                    job,
                    shapeSeed,
                    widthName,
                    width);
            if (job.CancelRequested)
            {
                FinishEdgeWearViabilityMatrix(
                    job,
                    true,
                    "cancelled by user");
                return;
            }
            job.Cases.Add(matrixCase);
            job.CompletedCaseCount++;

            if (job.CompletedCaseCount >= job.TotalCaseCount)
            {
                FinishEdgeWearViabilityMatrix(job, false, string.Empty);
            }
        }

        private static EdgeWearViabilityMatrixCase
            EvaluateEdgeWearViabilityMatrixCase(
                EdgeWearViabilityMatrixJob job,
                int shapeSeed,
                string widthName,
                float width)
        {
            MassRecipe caseRecipe = JsonUtility.FromJson<MassRecipe>(
                job.RecipeJson);
            if (caseRecipe == null)
            {
                return new EdgeWearViabilityMatrixCase(
                    shapeSeed,
                    widthName,
                    width,
                    new MassGenerator.EdgeWearBatchAuditCaseResult
                    {
                        ShapeSeed = shapeSeed,
                        EdgeWearWidth = width,
                        PrimaryFailure =
                            "failed to clone the immutable mass recipe"
                    });
            }
            caseRecipe.SetShapeSeed(shapeSeed);

            MassSurfaceFeatureSettings settings =
                new MassSurfaceFeatureSettings(
                    caseRecipe.Archetype,
                    caseRecipe.SurfaceSeed,
                    job.EdgeWearAmount,
                    width,
                    2f,
                    job.EdgeWearSoftness,
                    job.CreaseAmount,
                    job.CreaseWidth,
                    job.CreaseLength,
                    job.CreaseBranching);
            MassGenerator.EdgeWearBatchAuditCaseResult result;
            MassGenerator.SetEditorEdgeWearAuditCancellationProbe(() =>
            {
                if (job.CancelRequested)
                {
                    return true;
                }

                float progress = (float)job.CompletedCaseCount /
                    job.TotalCaseCount;
                if (!EditorUtility.DisplayCancelableProgressBar(
                        job.ProgressTitle,
                        "Seed " + shapeSeed + ", " + widthName +
                        " width — bounded conflict search",
                        progress))
                {
                    return false;
                }

                job.CancelRequested = true;
                return true;
            });
            try
            {
                result = job.RequireAllGeometricCandidates
                    ? MassGenerator.GenerateUnifiedEdgeWearBatchAuditCase(
                        caseRecipe,
                        settings)
                    : MassGenerator
                        .GenerateUnifiedEdgeWearPreviewParityAuditCase(
                            caseRecipe,
                            settings);
            }
            finally
            {
                MassGenerator.SetEditorEdgeWearAuditCancellationProbe(null);
            }
            return new EdgeWearViabilityMatrixCase(
                shapeSeed,
                widthName,
                width,
                result);
        }

        private static void FinishEdgeWearViabilityMatrix(
            EdgeWearViabilityMatrixJob job,
            bool cancelled,
            string terminalReason)
        {
            EditorApplication.update -= AdvanceEdgeWearViabilityMatrix;
            EditorUtility.ClearProgressBar();
            activeEdgeWearViabilityMatrixJob = null;

            bool statePreserved = job.ValidateTargetStatePreserved(
                out string stateDiagnostic);
            if (!statePreserved)
            {
                terminalReason = string.IsNullOrEmpty(terminalReason)
                    ? stateDiagnostic
                    : terminalReason + "; " + stateDiagnostic;
            }

            EdgeWearViabilityMatrixAggregate aggregate =
                BuildEdgeWearViabilityMatrixAggregate(
                    job,
                    cancelled,
                    statePreserved,
                    terminalReason);
            string reportText =
                BuildEdgeWearViabilityMatrixText(job, aggregate);
            string reportCsv = BuildEdgeWearViabilityMatrixCsv(job);
            WriteEdgeWearViabilityMatrixReports(
                job,
                reportText,
                reportCsv);

            lastEdgeWearBatchSummary = job.DisplayName + " " +
                aggregate.Status + ": " + aggregate.CasesPassed + "/" +
                aggregate.CasesRun + " cases passed. Reports: Library/" +
                job.ReportTextFileName + " and " +
                job.ReportCsvFileName;

            EdgeWearValidationSuiteJob suite =
                activeEdgeWearValidationSuiteJob;
            if (job.SuiteOwned && suite != null)
            {
                suite.RecordMatrix(job, aggregate, reportText);
                if (cancelled || suite.CancelRequested ||
                    !statePreserved || suite.Target == null)
                {
                    FinishEdgeWearValidationSuite(
                        suite,
                        cancelled || suite.CancelRequested,
                        terminalReason);
                    return;
                }

                if (job.Kind == EdgeWearMatrixKind.TopologyViability)
                {
                    suite.Stage = EdgeWearValidationSuiteStage
                        .ArtisticPreviewParity;
                    StartEdgeWearViabilityMatrix(
                        suite.Target,
                        EdgeWearMatrixKind.ArtisticPreviewParity,
                        true);
                    return;
                }

                FinishEdgeWearValidationSuite(
                    suite,
                    false,
                    string.Empty);
                return;
            }

            LogEdgeWearViabilityMatrixSummary(
                job,
                aggregate,
                statePreserved);
        }

        private static void LogEdgeWearViabilityMatrixSummary(
            EdgeWearViabilityMatrixJob job,
            EdgeWearViabilityMatrixAggregate aggregate,
            bool statePreserved)
        {
            string message =
                "GeneratedMass edge-wear " + job.ConsoleName + ". " +
                "status:" + aggregate.Status +
                ",cases:" + aggregate.CasesPassed + "/" +
                    aggregate.CasesRun +
                ",coverageFailures:" +
                    aggregate.CoexistenceCoverageFailures +
                ",widthFloorFailures:" +
                    aggregate.WidthFloorFailures +
                ",missingJunctionFailures:" +
                    aggregate.MissingJunctionFailures +
                ",tJunctionFailures:" +
                    aggregate.TJunctionFailures +
                ",strictIntersectionFailures:" +
                    aggregate.StrictIntersectionFailures +
                ",planeBandFailures:" +
                    aggregate.PlaneBandFailures +
                ",terminalCandidateConservationFailures:" +
                    aggregate.CandidateConservationFailures +
                ",topologyFailures:" +
                    aggregate.TopologyFailures +
                ",faceQualityFailures:" +
                    aggregate.FaceQualityFailures +
                ",placementFailures:" +
                    aggregate.PlacementFailures +
                ",cacheFailures:" +
                    aggregate.CacheContractFailures +
                ",collateralFailures:" +
                    aggregate.CollateralPreservationFailures +
                ",minimumCertifiedRatio:" +
                    aggregate.MinimumCertifiedRatio.ToString(
                        "G9", CultureInfo.InvariantCulture) +
                ",maximumPreflightMs:" +
                    aggregate.MaximumPreflightMilliseconds.ToString(
                        "G9", CultureInfo.InvariantCulture) +
                ",maximumTotalMs:" +
                    aggregate.MaximumTotalMilliseconds.ToString(
                        "G9", CultureInfo.InvariantCulture) +
                ",statePreserved:" + (statePreserved ? "1" : "0") +
                ",reports=Library/" + job.ReportTextFileName + "|" +
                    job.ReportCsvFileName;
            Debug.LogFormat(
                aggregate.CasesFailed > 0 || !statePreserved
                    ? LogType.Warning
                    : LogType.Log,
                LogOption.NoStacktrace,
                job.Target,
                "{0}",
                message);
        }

        private static void FinishEdgeWearValidationSuite(
            EdgeWearValidationSuiteJob suite,
            bool cancelled,
            string terminalReason)
        {
            if (suite == null)
            {
                return;
            }

            activeEdgeWearValidationSuiteJob = null;
            activeEdgeWearViabilityMatrixJob = null;
            EditorApplication.update -= AdvanceEdgeWearViabilityMatrix;
            EditorUtility.ClearProgressBar();
            suite.Stage = EdgeWearValidationSuiteStage.Complete;
            suite.Cancelled = cancelled;
            suite.TerminalReason = terminalReason ?? string.Empty;

            BuildEdgeWearArtisticComprehensiveEvidence(
                suite,
                out string comprehensiveReport,
                out string comprehensiveEdgesCsv,
                out string comprehensiveScenariosCsv,
                out string comprehensiveDiagnostic);
            suite.ComprehensiveArtisticReport =
                comprehensiveReport ?? string.Empty;
            suite.ComprehensiveArtisticDiagnostic =
                comprehensiveDiagnostic ?? string.Empty;
            suite.ComprehensiveArtisticAvailable =
                string.IsNullOrEmpty(comprehensiveDiagnostic) &&
                !string.IsNullOrEmpty(comprehensiveReport);
            bool comprehensiveWritten =
                WriteEdgeWearArtisticComprehensiveReports(
                    comprehensiveReport,
                    comprehensiveEdgesCsv,
                    comprehensiveScenariosCsv,
                    out string comprehensiveWriteDiagnostic);
            if (!comprehensiveWritten)
            {
                suite.ComprehensiveArtisticAvailable = false;
                suite.ComprehensiveArtisticDiagnostic =
                    string.IsNullOrEmpty(
                        suite.ComprehensiveArtisticDiagnostic)
                        ? comprehensiveWriteDiagnostic
                        : suite.ComprehensiveArtisticDiagnostic + "; " +
                            comprehensiveWriteDiagnostic;
            }

            string status = suite.Status;
            string report = BuildEdgeWearValidationSuiteReport(suite);
            bool reportWritten = WriteEdgeWearValidationSuiteReport(
                report,
                out string reportDiagnostic);
            if (!reportWritten)
            {
                status = "failed";
                suite.TerminalReason = string.IsNullOrEmpty(
                        suite.TerminalReason)
                    ? reportDiagnostic
                    : suite.TerminalReason + "; " + reportDiagnostic;
            }

            lastEdgeWearValidationSuiteSummary =
                "Full edge-wear validation suite " + status + ": " +
                "current preview=" +
                (suite.CurrentPreviewPassed ? "passed" : "failed") +
                ", topology=" + suite.TopologyCasesPassed + "/" +
                suite.TopologyCasesRun + ", artistic preview=" +
                suite.PreviewCasesPassed + "/" +
                suite.PreviewCasesRun + ". Combined report: Library/" +
                EdgeWearValidationSuiteReportFileName;

            string message =
                "GeneratedMass edge-wear full validation suite. " +
                "status:" + status +
                ",currentPreview:" +
                    (suite.CurrentPreviewPassed ? "1" : "0") +
                ",topology:" + suite.TopologyCasesPassed + "/" +
                    suite.TopologyCasesRun +
                ",preview:" + suite.PreviewCasesPassed + "/" +
                    suite.PreviewCasesRun +
                ",outlierResolution:" +
                    suite.OutlierRecoveryChecksPassed + "/" +
                    suite.OutlierRecoveryChecksRun +
                ",negativeExclusion:" +
                    suite.NegativeExclusionChecksPassed + "/" +
                    suite.NegativeExclusionChecksRun +
                ",topologyCollateralFailures:" +
                    suite.TopologyCollateralFailures +
                ",previewCollateralFailures:" +
                    suite.PreviewCollateralFailures +
                ",artisticComprehensive:" +
                    (suite.ComprehensiveArtisticAvailable ? "1" : "0") +
                ",report=Library/" +
                    EdgeWearValidationSuiteReportFileName;
            Debug.LogFormat(
                status == "passed" ? LogType.Log : LogType.Warning,
                LogOption.NoStacktrace,
                suite.Target,
                "{0}",
                message);
        }

        private static string BuildEdgeWearValidationSuiteReport(
            EdgeWearValidationSuiteJob suite)
        {
            StringBuilder builder = new StringBuilder(262144);
            builder.AppendLine(
                "GeneratedMass edge-wear one-click validation suite");
            builder.AppendLine("contract=EW-B4.2R13A.7-suite");
            builder.Append("object=");
            builder.AppendLine(suite.TargetName);
            builder.Append("entityId=");
            builder.AppendLine(suite.TargetEntityId);
            builder.Append("currentShapeSeed=");
            builder.AppendLine(suite.CurrentShapeSeed.ToString());
            builder.Append("matrixSeeds=");
            builder.AppendLine(string.Join("/", EdgeWearBatchShapeSeeds));
            builder.Append("matrixCasesPerPolicy=");
            builder.AppendLine((
                EdgeWearBatchShapeSeeds.Length *
                EdgeWearBatchWidths.Length).ToString());
            builder.Append("status=");
            builder.AppendLine(suite.Status);
            builder.Append("currentPreviewPassed=");
            builder.AppendLine(
                suite.CurrentPreviewPassed ? "1" : "0");
            builder.Append("currentPreviewTelemetryAvailable=");
            builder.AppendLine(
                suite.CurrentPreviewTelemetryAvailable ? "1" : "0");
            builder.Append("topologyStatus=");
            builder.AppendLine(suite.TopologyStatus);
            builder.Append("topologyCases=");
            builder.Append(suite.TopologyCasesPassed);
            builder.Append('/');
            builder.AppendLine(suite.TopologyCasesRun.ToString());
            builder.Append("previewStatus=");
            builder.AppendLine(suite.PreviewStatus);
            builder.Append("previewCases=");
            builder.Append(suite.PreviewCasesPassed);
            builder.Append('/');
            builder.AppendLine(suite.PreviewCasesRun.ToString());
            builder.Append("outlierResolutionStatus=");
            builder.AppendLine(suite.OutlierRecoveryStatus);
            builder.Append("outlierResolutionChecks=");
            builder.Append(suite.OutlierRecoveryChecksPassed);
            builder.Append('/');
            builder.AppendLine(suite.OutlierRecoveryChecksRun.ToString());
            builder.Append("outlierCertifiedRecoveries=");
            builder.AppendLine(
                suite.OutlierCertifiedRecoveries.ToString());
            builder.Append("outlierProvenInfeasible=");
            builder.AppendLine(
                suite.OutlierProvenInfeasible.ToString());
            builder.Append("outlierUnresolved=");
            builder.AppendLine(suite.OutlierUnresolved.ToString());
            builder.Append("negativeExclusionStatus=");
            builder.AppendLine(suite.NegativeExclusionStatus);
            builder.Append("negativeExclusionChecks=");
            builder.Append(suite.NegativeExclusionChecksPassed);
            builder.Append('/');
            builder.AppendLine(
                suite.NegativeExclusionChecksRun.ToString());
            builder.Append("artisticComprehensiveAvailable=");
            builder.AppendLine(
                suite.ComprehensiveArtisticAvailable ? "1" : "0");
            builder.Append("artisticComprehensiveReports=");
            builder.Append("Library/");
            builder.Append(EdgeWearArtisticComprehensiveReportFileName);
            builder.Append("|Library/");
            builder.Append(EdgeWearArtisticComprehensiveEdgesCsvFileName);
            builder.Append("|Library/");
            builder.AppendLine(
                EdgeWearArtisticComprehensiveScenariosCsvFileName);
            builder.Append("cancelled=");
            builder.AppendLine(suite.Cancelled ? "1" : "0");
            builder.Append("terminalReason=");
            builder.AppendLine(string.IsNullOrEmpty(suite.TerminalReason)
                ? "none"
                : suite.TerminalReason);

            builder.AppendLine();
            builder.AppendLine("[Current Preview Summary]");
            builder.AppendLine(suite.CurrentPreviewSummary);
            builder.AppendLine();
            builder.AppendLine("[Current Preview Telemetry]");
            builder.AppendLine(suite.CurrentPreviewTelemetryAvailable
                ? suite.CurrentPreviewTelemetry
                : "unavailable: " + suite.CurrentPreviewTelemetryDiagnostic);
            builder.AppendLine();
            builder.AppendLine("[Topology Viability Matrix]");
            builder.AppendLine(string.IsNullOrEmpty(suite.TopologyReportText)
                ? "not run"
                : suite.TopologyReportText);
            builder.AppendLine();
            builder.AppendLine("[Artistic Preview Parity Matrix]");
            builder.AppendLine(string.IsNullOrEmpty(suite.PreviewReportText)
                ? "not run"
                : suite.PreviewReportText);
            builder.AppendLine();
            builder.AppendLine(
                "[Outlier Resolution and Negative Exclusion Contract]");
            builder.AppendLine(string.IsNullOrEmpty(
                    suite.OutlierRecoveryReport)
                ? "not evaluated"
                : suite.OutlierRecoveryReport);
            builder.AppendLine();
            builder.AppendLine(
                "[Comprehensive Artistic Selection Evidence]");
            builder.AppendLine(suite.ComprehensiveArtisticAvailable
                ? suite.ComprehensiveArtisticReport
                : "unavailable: " +
                    (string.IsNullOrEmpty(
                        suite.ComprehensiveArtisticDiagnostic)
                        ? "not captured"
                        : suite.ComprehensiveArtisticDiagnostic));
            return builder.ToString();
        }


        private const int EdgeWearArtisticModifierBase = 1;
        private const int EdgeWearArtisticModifierUpward = 2;
        private const int EdgeWearArtisticModifierCharacter = 4;
        private const int EdgeWearArtisticGateLength = 1;
        private const int EdgeWearArtisticGateAngle = 2;
        private const int EdgeWearArtisticGateBase = 4;
        private const float EdgeWearArtisticScoreReproductionTolerance =
            0.000002f;
        private const float EdgeWearArtisticRankScoreTolerance =
            0.0000001f;

        private sealed class EdgeWearArtisticScenario
        {
            public string Name = string.Empty;
            public string Category = string.Empty;
            public float AngleWeight;
            public float LengthWeight;
            public float RandomWeight;
            public float DihedralWeight;
            public float SilhouetteWeight;
            public float WidthWeight;
            public float IsolationWeight;
            public float LowCrowdingWeight;
            public float VerticalWeight;
            public float HorizontalWeight;
            public float StrengthWeight;
            public float DepthWeight;
            public float LocalityWeight;
            public float SeamWeight;
            public int ModifierMask;
            public int GateMask;
            public bool Named;
        }

        private sealed class EdgeWearArtisticScenarioOutcome
        {
            public EdgeWearArtisticScenario Scenario;
            public readonly List<int> RankedEdgeIds = new List<int>();
            public readonly Dictionary<int, float> ScoreByEdge =
                new Dictionary<int, float>();
            public readonly Dictionary<int, int> RankByEdge =
                new Dictionary<int, int>();
            public float ScoreMinimum;
            public float ScoreMedian;
            public float ScoreMaximum;
            public string RankHash = string.Empty;
        }

        private sealed class EdgeWearArtisticCaseAnalysis
        {
            public EdgeWearViabilityMatrixCase MatrixCase;
            public MassGenerator.EdgeWearArtisticEdgeAuditRecord[] Edges =
                Array.Empty<MassGenerator.EdgeWearArtisticEdgeAuditRecord>();
            public readonly List<EdgeWearArtisticScenarioOutcome> Outcomes =
                new List<EdgeWearArtisticScenarioOutcome>();
            public EdgeWearArtisticScenarioOutcome CurrentOutcome;
            public float CurrentScoreReproductionMaximumError;
            public int CurrentScoreReproductionFailureCount;
            public string CurrentScoreReproductionDiagnostic = string.Empty;
            public bool RecordedProductionRanksValid;
            public int RecordedProductionRankIntegrityFailureCount;
            public string RecordedProductionRankIntegrityDiagnostic =
                string.Empty;
        }

        private static void BuildEdgeWearArtisticComprehensiveEvidence(
            EdgeWearValidationSuiteJob suite,
            out string report,
            out string edgesCsv,
            out string scenariosCsv,
            out string diagnostic)
        {
            report = string.Empty;
            edgesCsv = string.Empty;
            scenariosCsv = string.Empty;
            diagnostic = string.Empty;
            if (suite == null)
            {
                diagnostic = "validation suite was null";
                return;
            }
            int expectedCaseCount = EdgeWearBatchShapeSeeds.Length *
                EdgeWearBatchWidths.Length;
            if (suite.PreviewCases.Count != expectedCaseCount)
            {
                diagnostic = "artistic preview case set was incomplete: " +
                    suite.PreviewCases.Count + "/" + expectedCaseCount;
                return;
            }

            List<EdgeWearArtisticScenario> scenarioUniverse =
                BuildEdgeWearArtisticScenarioUniverse();
            if (scenarioUniverse.Count == 0)
            {
                diagnostic = "artistic scenario universe was empty";
                return;
            }

            StringBuilder caseReportBuilder =
                new StringBuilder(1048576);
            StringBuilder edgesCsvBuilder = new StringBuilder(1048576);
            StringBuilder scenariosCsvBuilder =
                new StringBuilder(8388608);
            AppendEdgeWearArtisticCsvHeaderOnly(
                edgesCsvBuilder,
                BuildEdgeWearArtisticComprehensiveEdgesCsv(
                    new List<EdgeWearArtisticCaseAnalysis>()));
            AppendEdgeWearArtisticCsvHeaderOnly(
                scenariosCsvBuilder,
                BuildEdgeWearArtisticComprehensiveScenariosCsv(
                    new List<EdgeWearArtisticCaseAnalysis>(),
                    scenarioUniverse));

            List<EdgeWearArtisticCaseAnalysis> crossWidthAnalyses =
                new List<EdgeWearArtisticCaseAnalysis>(expectedCaseCount);
            int edgeRows = 0;
            int geometricRows = 0;
            int currentEligibleRows = 0;
            int currentSelectedRows = 0;
            float maximumScoreError = 0f;
            int recordedRankIntegrityFailures = 0;
            int randomSensitive25 = 0;
            int randomSensitive50 = 0;
            int randomSensitive75 = 0;
            int gateContradictions = 0;

            for (int caseIndex = 0;
                 caseIndex < suite.PreviewCases.Count;
                 caseIndex++)
            {
                EdgeWearViabilityMatrixCase matrixCase =
                    suite.PreviewCases[caseIndex];
                MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records =
                    matrixCase.Result.ArtisticEdges;
                if (records == null || records.Length == 0)
                {
                    diagnostic = "artistic edge records missing for seed=" +
                        matrixCase.ShapeSeed + ",width=" +
                        matrixCase.WidthName;
                    return;
                }

                EdgeWearArtisticCaseAnalysis analysis =
                    AnalyzeEdgeWearArtisticCase(
                        matrixCase,
                        records,
                        scenarioUniverse);
                maximumScoreError = Mathf.Max(
                    maximumScoreError,
                    analysis.CurrentScoreReproductionMaximumError);
                recordedRankIntegrityFailures +=
                    analysis.RecordedProductionRankIntegrityFailureCount;
                if (!analysis.RecordedProductionRanksValid)
                {
                    diagnostic =
                        "recorded production rank integrity failed for " +
                        "seed=" + matrixCase.ShapeSeed + ",width=" +
                        matrixCase.WidthName + ": " +
                        analysis.RecordedProductionRankIntegrityDiagnostic;
                    return;
                }
                if (analysis.CurrentScoreReproductionMaximumError >
                        EdgeWearArtisticScoreReproductionTolerance ||
                    analysis.CurrentScoreReproductionFailureCount != 0)
                {
                    diagnostic =
                        "current artistic score formula did not reproduce " +
                        "for seed=" + matrixCase.ShapeSeed + ",width=" +
                        matrixCase.WidthName + " (scoreError=" +
                        FormatEdgeWearArtisticFloat(
                            analysis.CurrentScoreReproductionMaximumError) +
                        ",failures=" +
                        analysis.CurrentScoreReproductionFailureCount +
                        ",diagnostic=" +
                        analysis.CurrentScoreReproductionDiagnostic + ")";
                    return;
                }

                edgeRows += records.Length;
                for (int edgeIndex = 0;
                     edgeIndex < records.Length;
                     edgeIndex++)
                {
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                        records[edgeIndex];
                    geometricRows += edge.GeometricEligible;
                    currentEligibleRows += edge.ArtisticEligible;
                    currentSelectedRows += edge.Selected;
                    if (edge.GeometricEligible != 0 &&
                        edge.ArtisticEligible == 0 &&
                        edge.Score >= matrixCase.Result
                            .ArtisticSelectionThreshold)
                    {
                        gateContradictions++;
                    }
                }
                EdgeWearArtisticScenarioOutcome noRandom =
                    FindEdgeWearArtisticOutcome(
                        analysis,
                        "current-no-random");
                int currentCount =
                    analysis.CurrentOutcome.RankedEdgeIds.Count;
                randomSensitive25 += CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    noRandom,
                    ResolveEdgeWearArtisticCoverageCount(
                        currentCount,
                        0.25f));
                randomSensitive50 += CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    noRandom,
                    ResolveEdgeWearArtisticCoverageCount(
                        currentCount,
                        0.50f));
                randomSensitive75 += CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    noRandom,
                    ResolveEdgeWearArtisticCoverageCount(
                        currentCount,
                        0.75f));

                AppendEdgeWearArtisticCaseEvidence(
                    caseReportBuilder,
                    analysis,
                    scenarioUniverse);
                AppendEdgeWearArtisticCsvBody(
                    edgesCsvBuilder,
                    BuildEdgeWearArtisticComprehensiveEdgesCsv(
                        new List<EdgeWearArtisticCaseAnalysis>
                        {
                            analysis
                        }));
                AppendEdgeWearArtisticCsvBody(
                    scenariosCsvBuilder,
                    BuildEdgeWearArtisticComprehensiveScenariosCsv(
                        new List<EdgeWearArtisticCaseAnalysis>
                        {
                            analysis
                        },
                        scenarioUniverse));

                analysis.Outcomes.Clear();
                analysis.Edges = Array.Empty<
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord>();
                crossWidthAnalyses.Add(analysis);
            }

            StringBuilder reportBuilder = new StringBuilder(
                caseReportBuilder.Length + 32768);
            reportBuilder.AppendLine(
                "GeneratedMass comprehensive artistic selection evidence");
            reportBuilder.AppendLine(
                "contract=EW-B4.2R13A.7-comprehensive");
            reportBuilder.Append("cases=");
            reportBuilder.AppendLine(expectedCaseCount.ToString());
            reportBuilder.Append("scenariosPerCase=");
            reportBuilder.AppendLine(scenarioUniverse.Count.ToString());
            reportBuilder.Append("totalScenarioEvaluations=");
            reportBuilder.AppendLine(
                (expectedCaseCount * scenarioUniverse.Count).ToString());
            reportBuilder.AppendLine(
                "behaviorChanged=0;geometryChanged=0;selectionChanged=0");
            reportBuilder.AppendLine(
                "scenarioUniverse=exact+ablations+all-0.05-angle-length-random-simplex*8-modifier-masks+gate-masks+single-metric+signed-context-sweeps+composites");
            reportBuilder.AppendLine(
                "cutoffUniverse=every fixed selected slot plus native coverage deciles 0.10-1.00");
            reportBuilder.AppendLine(
                "rawEvidence=all edge geometry,normals,gates,score components,modifiers,context,viability,effect,lifecycle");
            AppendEdgeWearArtisticGlobalEvidenceValues(
                reportBuilder,
                scenarioUniverse,
                edgeRows,
                geometricRows,
                currentEligibleRows,
                currentSelectedRows,
                maximumScoreError,
                recordedRankIntegrityFailures,
                randomSensitive25,
                randomSensitive50,
                randomSensitive75,
                gateContradictions);
            AppendEdgeWearArtisticScenarioDefinitions(
                reportBuilder,
                scenarioUniverse);
            reportBuilder.Append(caseReportBuilder);
            AppendEdgeWearArtisticCrossWidthEvidence(
                reportBuilder,
                crossWidthAnalyses);

            report = reportBuilder.ToString();
            edgesCsv = edgesCsvBuilder.ToString();
            scenariosCsv = scenariosCsvBuilder.ToString();
            if (string.IsNullOrEmpty(report) ||
                string.IsNullOrEmpty(edgesCsv) ||
                string.IsNullOrEmpty(scenariosCsv))
            {
                diagnostic = "one or more comprehensive outputs were empty";
            }
        }

        private static void AppendEdgeWearArtisticCsvHeaderOnly(
            StringBuilder target,
            string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return;
            }
            int lineEnd = csv.IndexOf('\n');
            target.Append(lineEnd >= 0
                ? csv.Substring(0, lineEnd + 1)
                : csv);
        }

        private static void AppendEdgeWearArtisticCsvBody(
            StringBuilder target,
            string csv)
        {
            if (string.IsNullOrEmpty(csv))
            {
                return;
            }
            int lineEnd = csv.IndexOf('\n');
            if (lineEnd >= 0 && lineEnd + 1 < csv.Length)
            {
                target.Append(csv, lineEnd + 1, csv.Length - lineEnd - 1);
            }
        }

        private static void AppendEdgeWearArtisticGlobalEvidenceValues(
            StringBuilder builder,
            List<EdgeWearArtisticScenario> scenarios,
            int edgeRows,
            int geometricRows,
            int currentEligibleRows,
            int currentSelectedRows,
            float maximumReproductionError,
            int recordedRankIntegrityFailures,
            int randomSensitive25,
            int randomSensitive50,
            int randomSensitive75,
            int gateContradictions)
        {
            builder.AppendLine();
            builder.AppendLine("[Global Evidence Summary]");
            builder.Append("edgeRows/geometric/currentEligible/currentSelected=");
            builder.Append(edgeRows);
            builder.Append('/');
            builder.Append(geometricRows);
            builder.Append('/');
            builder.Append(currentEligibleRows);
            builder.Append('/');
            builder.AppendLine(currentSelectedRows.ToString());
            builder.Append("currentScoreReproductionMaximumError=");
            builder.AppendLine(FormatEdgeWearArtisticFloat(
                maximumReproductionError));
            builder.AppendLine("recordedProductionRanksValid=1");
            builder.Append("recordedProductionRankIntegrityFailures=");
            builder.AppendLine(recordedRankIntegrityFailures.ToString());
            builder.Append("hardGateAboveThresholdContradictions=");
            builder.AppendLine(gateContradictions.ToString());
            builder.Append("noRandomCutoffChurn25/50/75=");
            builder.Append(randomSensitive25);
            builder.Append('/');
            builder.Append(randomSensitive50);
            builder.Append('/');
            builder.AppendLine(randomSensitive75.ToString());
            builder.Append("scenarioCategories=");
            Dictionary<string, int> categoryCounts =
                new Dictionary<string, int>();
            for (int scenarioIndex = 0;
                 scenarioIndex < scenarios.Count;
                 scenarioIndex++)
            {
                string category = scenarios[scenarioIndex].Category;
                categoryCounts.TryGetValue(category, out int count);
                categoryCounts[category] = count + 1;
            }
            List<string> categories = new List<string>(
                categoryCounts.Keys);
            categories.Sort(StringComparer.Ordinal);
            for (int categoryIndex = 0;
                 categoryIndex < categories.Count;
                 categoryIndex++)
            {
                if (categoryIndex > 0)
                {
                    builder.Append(';');
                }
                string category = categories[categoryIndex];
                builder.Append(category);
                builder.Append(':');
                builder.Append(categoryCounts[category]);
            }
            builder.AppendLine();
        }

        private static List<EdgeWearArtisticScenario>
            BuildEdgeWearArtisticScenarioUniverse()
        {
            List<EdgeWearArtisticScenario> scenarios =
                new List<EdgeWearArtisticScenario>(2048);
            AddEdgeWearArtisticScenario(
                scenarios,
                "current-exact",
                "baseline",
                0.60f,
                0.35f,
                0.05f,
                EdgeWearArtisticModifierBase |
                    EdgeWearArtisticModifierUpward,
                EdgeWearArtisticGateLength |
                    EdgeWearArtisticGateAngle |
                    EdgeWearArtisticGateBase,
                true);
            AddEdgeWearArtisticScenario(
                scenarios,
                "current-no-random",
                "ablation",
                0.63157892f,
                0.36842105f,
                0f,
                EdgeWearArtisticModifierBase |
                    EdgeWearArtisticModifierUpward,
                7,
                true);
            AddEdgeWearArtisticScenario(
                scenarios,
                "current-no-modifiers",
                "ablation",
                0.60f,
                0.35f,
                0.05f,
                0,
                7,
                true);
            AddEdgeWearArtisticScenario(
                scenarios,
                "current-no-gates",
                "ablation",
                0.60f,
                0.35f,
                0.05f,
                EdgeWearArtisticModifierBase |
                    EdgeWearArtisticModifierUpward,
                0,
                true);

            for (int modifierMask = 0;
                 modifierMask <= 7;
                 modifierMask++)
            {
                AddEdgeWearArtisticScenario(
                    scenarios,
                    "modifier-mask-" + modifierMask,
                    "modifier-ablation",
                    0.60f,
                    0.35f,
                    0.05f,
                    modifierMask,
                    7,
                    true);
            }
            for (int gateMask = 0; gateMask <= 7; gateMask++)
            {
                AddEdgeWearArtisticScenario(
                    scenarios,
                    "gate-mask-" + gateMask,
                    "gate-ablation",
                    0.60f,
                    0.35f,
                    0.05f,
                    EdgeWearArtisticModifierBase |
                        EdgeWearArtisticModifierUpward,
                    gateMask,
                    true);
            }

            AddEdgeWearArtisticSingleMetricScenarios(scenarios);
            AddEdgeWearArtisticContextScenarios(scenarios);
            AddEdgeWearArtisticCompositeScenarios(scenarios);

            for (int angleUnits = 0;
                 angleUnits <= 20;
                 angleUnits++)
            {
                for (int lengthUnits = 0;
                     lengthUnits <= 20 - angleUnits;
                     lengthUnits++)
                {
                    int randomUnits = 20 - angleUnits - lengthUnits;
                    float angleWeight = angleUnits / 20f;
                    float lengthWeight = lengthUnits / 20f;
                    float randomWeight = randomUnits / 20f;
                    for (int modifierMask = 0;
                         modifierMask <= 7;
                         modifierMask++)
                    {
                        AddEdgeWearArtisticScenario(
                            scenarios,
                            "simplex-a" + angleUnits +
                                "-l" + lengthUnits +
                                "-r" + randomUnits +
                                "-m" + modifierMask,
                            "weight-simplex",
                            angleWeight,
                            lengthWeight,
                            randomWeight,
                            modifierMask,
                            7,
                            false);
                    }
                }
            }
            return scenarios;
        }

        private static void AddEdgeWearArtisticScenario(
            List<EdgeWearArtisticScenario> scenarios,
            string name,
            string category,
            float angleWeight,
            float lengthWeight,
            float randomWeight,
            int modifierMask,
            int gateMask,
            bool named)
        {
            scenarios.Add(new EdgeWearArtisticScenario
            {
                Name = name,
                Category = category,
                AngleWeight = angleWeight,
                LengthWeight = lengthWeight,
                RandomWeight = randomWeight,
                ModifierMask = modifierMask,
                GateMask = gateMask,
                Named = named
            });
        }

        private static void AddEdgeWearArtisticSingleMetricScenarios(
            List<EdgeWearArtisticScenario> scenarios)
        {
            string[] names =
            {
                "angle",
                "length",
                "random",
                "dihedral",
                "silhouette",
                "width",
                "isolation",
                "low-crowding",
                "vertical",
                "horizontal",
                "strength",
                "depth",
                "locality",
                "seam"
            };
            for (int index = 0; index < names.Length; index++)
            {
                EdgeWearArtisticScenario scenario =
                    new EdgeWearArtisticScenario
                    {
                        Name = "single-" + names[index],
                        Category = "single-metric",
                        GateMask = 7,
                        ModifierMask = 0,
                        Named = true
                    };
                SetEdgeWearArtisticContextWeight(
                    scenario,
                    names[index],
                    1f);
                scenarios.Add(scenario);
            }
        }

        private static void AddEdgeWearArtisticContextScenarios(
            List<EdgeWearArtisticScenario> scenarios)
        {
            string[] names =
            {
                "dihedral",
                "silhouette",
                "width",
                "isolation",
                "low-crowding",
                "vertical",
                "horizontal",
                "strength",
                "depth",
                "locality",
                "seam"
            };
            float[] weights = { -0.5f, -0.25f, 0.25f, 0.5f };
            for (int nameIndex = 0;
                 nameIndex < names.Length;
                 nameIndex++)
            {
                for (int weightIndex = 0;
                     weightIndex < weights.Length;
                     weightIndex++)
                {
                    EdgeWearArtisticScenario scenario =
                        new EdgeWearArtisticScenario
                        {
                            Name = "current-plus-" + names[nameIndex] +
                                "-" + FormatEdgeWearArtisticFloat(
                                    weights[weightIndex]),
                            Category = "context-sweep",
                            AngleWeight = 0.60f,
                            LengthWeight = 0.35f,
                            RandomWeight = 0.05f,
                            ModifierMask = EdgeWearArtisticModifierBase |
                                EdgeWearArtisticModifierUpward,
                            GateMask = 7,
                            Named = true
                        };
                    SetEdgeWearArtisticContextWeight(
                        scenario,
                        names[nameIndex],
                        weights[weightIndex]);
                    scenarios.Add(scenario);
                }
            }
        }

        private static void AddEdgeWearArtisticCompositeScenarios(
            List<EdgeWearArtisticScenario> scenarios)
        {
            EdgeWearArtisticScenario quality =
                new EdgeWearArtisticScenario
                {
                    Name = "composite-quality-no-random",
                    Category = "composite",
                    AngleWeight = 0.34f,
                    LengthWeight = 0.18f,
                    DihedralWeight = 0.10f,
                    SilhouetteWeight = 0.12f,
                    WidthWeight = 0.08f,
                    IsolationWeight = 0.09f,
                    LowCrowdingWeight = 0.09f,
                    ModifierMask = 7,
                    GateMask = 7,
                    Named = true
                };
            scenarios.Add(quality);
            EdgeWearArtisticScenario strongShort =
                new EdgeWearArtisticScenario
                {
                    Name = "composite-strong-short",
                    Category = "composite",
                    AngleWeight = 0.42f,
                    LengthWeight = -0.10f,
                    DihedralWeight = 0.18f,
                    SilhouetteWeight = 0.18f,
                    WidthWeight = 0.12f,
                    IsolationWeight = 0.10f,
                    ModifierMask = 7,
                    GateMask = 7,
                    Named = true
                };
            scenarios.Add(strongShort);
            EdgeWearArtisticScenario visibleSparse =
                new EdgeWearArtisticScenario
                {
                    Name = "composite-visible-sparse",
                    Category = "composite",
                    AngleWeight = 0.25f,
                    LengthWeight = 0.15f,
                    SilhouetteWeight = 0.25f,
                    IsolationWeight = 0.20f,
                    LowCrowdingWeight = 0.15f,
                    ModifierMask = 7,
                    GateMask = 7,
                    Named = true
                };
            scenarios.Add(visibleSparse);
            EdgeWearArtisticScenario buildReliable =
                new EdgeWearArtisticScenario
                {
                    Name = "composite-build-reliable",
                    Category = "composite",
                    AngleWeight = 0.24f,
                    LengthWeight = 0.18f,
                    WidthWeight = 0.22f,
                    LocalityWeight = 0.18f,
                    IsolationWeight = 0.09f,
                    LowCrowdingWeight = 0.09f,
                    ModifierMask = 7,
                    GateMask = 7,
                    Named = true
                };
            scenarios.Add(buildReliable);
            EdgeWearArtisticScenario effectForward =
                new EdgeWearArtisticScenario
                {
                    Name = "composite-effect-forward",
                    Category = "composite",
                    AngleWeight = 0.25f,
                    LengthWeight = 0.15f,
                    SilhouetteWeight = 0.15f,
                    StrengthWeight = 0.25f,
                    DepthWeight = 0.20f,
                    ModifierMask = 7,
                    GateMask = 7,
                    Named = true
                };
            scenarios.Add(effectForward);
        }

        private static void SetEdgeWearArtisticContextWeight(
            EdgeWearArtisticScenario scenario,
            string name,
            float weight)
        {
            switch (name)
            {
                case "angle":
                    scenario.AngleWeight = weight;
                    break;
                case "length":
                    scenario.LengthWeight = weight;
                    break;
                case "random":
                    scenario.RandomWeight = weight;
                    break;
                case "dihedral":
                    scenario.DihedralWeight = weight;
                    break;
                case "silhouette":
                    scenario.SilhouetteWeight = weight;
                    break;
                case "width":
                    scenario.WidthWeight = weight;
                    break;
                case "isolation":
                    scenario.IsolationWeight = weight;
                    break;
                case "low-crowding":
                    scenario.LowCrowdingWeight = weight;
                    break;
                case "vertical":
                    scenario.VerticalWeight = weight;
                    break;
                case "horizontal":
                    scenario.HorizontalWeight = weight;
                    break;
                case "strength":
                    scenario.StrengthWeight = weight;
                    break;
                case "depth":
                    scenario.DepthWeight = weight;
                    break;
                case "locality":
                    scenario.LocalityWeight = weight;
                    break;
                case "seam":
                    scenario.SeamWeight = weight;
                    break;
            }
        }

        private static EdgeWearArtisticCaseAnalysis
            AnalyzeEdgeWearArtisticCase(
                EdgeWearViabilityMatrixCase matrixCase,
                MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records,
                List<EdgeWearArtisticScenario> scenarios)
        {
            EdgeWearArtisticCaseAnalysis analysis =
                new EdgeWearArtisticCaseAnalysis
                {
                    MatrixCase = matrixCase,
                    Edges = records
                };
            if (scenarios == null || scenarios.Count == 0)
            {
                analysis.RecordedProductionRanksValid = false;
                analysis.RecordedProductionRankIntegrityFailureCount = 1;
                analysis.RecordedProductionRankIntegrityDiagnostic =
                    "current-exact scenario was unavailable";
                return analysis;
            }

            analysis.RecordedProductionRanksValid =
                TryBuildRecordedCurrentArtisticOutcome(
                    matrixCase,
                    records,
                    scenarios[0],
                    out EdgeWearArtisticScenarioOutcome currentOutcome,
                    out int rankIntegrityFailures,
                    out string rankIntegrityDiagnostic);
            analysis.CurrentOutcome = currentOutcome;
            analysis.RecordedProductionRankIntegrityFailureCount =
                rankIntegrityFailures;
            analysis.RecordedProductionRankIntegrityDiagnostic =
                rankIntegrityDiagnostic;
            if (currentOutcome != null)
            {
                analysis.Outcomes.Add(currentOutcome);
            }

            for (int scenarioIndex = 1;
                 scenarioIndex < scenarios.Count;
                 scenarioIndex++)
            {
                analysis.Outcomes.Add(
                    EvaluateEdgeWearArtisticScenario(
                        records,
                        scenarios[scenarioIndex]));
            }

            CalculateCurrentEdgeWearArtisticScoreReproduction(
                records,
                scenarios[0],
                out float maximumError,
                out int scoreFailures,
                out string scoreDiagnostic);
            analysis.CurrentScoreReproductionMaximumError = maximumError;
            analysis.CurrentScoreReproductionFailureCount = scoreFailures;
            analysis.CurrentScoreReproductionDiagnostic = scoreDiagnostic;
            return analysis;
        }

        private static bool TryBuildRecordedCurrentArtisticOutcome(
            EdgeWearViabilityMatrixCase matrixCase,
            MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records,
            EdgeWearArtisticScenario currentScenario,
            out EdgeWearArtisticScenarioOutcome outcome,
            out int failureCount,
            out string diagnostic)
        {
            outcome = new EdgeWearArtisticScenarioOutcome
            {
                Scenario = currentScenario
            };
            failureCount = 0;
            diagnostic = "none";
            if (records == null)
            {
                failureCount = 1;
                diagnostic = "artistic edge records were null";
                return false;
            }

            int rankedCount = 0;
            int survivingCandidateCount = 0;
            for (int edgeIndex = 0;
                 edgeIndex < records.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    records[edgeIndex];
                if (edge.GeometricEligible != 0 &&
                    edge.ArtisticEligible != 0)
                {
                    rankedCount++;
                }
                if (edge.Candidate != 0)
                {
                    survivingCandidateCount++;
                }
            }

            int expectedRankedCount =
                matrixCase.Result.ArtisticEligibleCount;
            if (rankedCount != expectedRankedCount)
            {
                failureCount = 1;
                diagnostic =
                    "artistic ranking-universe count mismatch " +
                    "records/result=" + rankedCount + "/" +
                    expectedRankedCount;
                return false;
            }
            int expectedCandidateCount = matrixCase.Result.CandidateCount;
            if (survivingCandidateCount != expectedCandidateCount)
            {
                failureCount = 1;
                diagnostic = "surviving candidate count mismatch " +
                    "records/result=" + survivingCandidateCount + "/" +
                    expectedCandidateCount;
                return false;
            }

            int[] edgeIdByRank = new int[rankedCount];
            float[] scoreByRank = new float[rankedCount];
            bool[] rankSeen = new bool[rankedCount];
            bool[] survivingCandidateIndexSeen =
                new bool[rankedCount];
            HashSet<int> sourceEdgeIds = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < records.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    records[edgeIndex];
                bool belongsToRankingUniverse =
                    edge.GeometricEligible != 0 &&
                    edge.ArtisticEligible != 0;
                if (!belongsToRankingUniverse)
                {
                    if (edge.ArtisticSelectionRank > 0)
                    {
                        failureCount = 1;
                        diagnostic =
                            "edge outside artistic ranking universe " +
                            "carried rank: edge=" +
                            edge.SourceEdgeIndex + ",rank=" +
                            edge.ArtisticSelectionRank;
                        return false;
                    }
                    if (edge.Candidate != 0)
                    {
                        failureCount = 1;
                        diagnostic =
                            "surviving candidate was outside artistic " +
                            "ranking universe: edge=" +
                            edge.SourceEdgeIndex;
                        return false;
                    }
                    continue;
                }
                if (edge.SourceEdgeIndex < 0)
                {
                    failureCount = 1;
                    diagnostic =
                        "ranked edge had invalid source edge id: " +
                        "rank=" + edge.ArtisticSelectionRank;
                    return false;
                }
                if (!sourceEdgeIds.Add(edge.SourceEdgeIndex))
                {
                    failureCount = 1;
                    diagnostic = "duplicate ranked source edge id=" +
                        edge.SourceEdgeIndex;
                    return false;
                }
                if (edge.ArtisticSelectionRank < 1 ||
                    edge.ArtisticSelectionRank > rankedCount)
                {
                    failureCount = 1;
                    diagnostic = "rank outside artistic universe 1..N: " +
                        "edge=" + edge.SourceEdgeIndex + ",rank=" +
                        edge.ArtisticSelectionRank + ",count=" +
                        rankedCount;
                    return false;
                }
                int rankIndex = edge.ArtisticSelectionRank - 1;
                if (rankSeen[rankIndex])
                {
                    failureCount = 1;
                    diagnostic = "duplicate production rank=" +
                        edge.ArtisticSelectionRank + ",edge=" +
                        edge.SourceEdgeIndex;
                    return false;
                }
                if (float.IsNaN(edge.Score) ||
                    float.IsInfinity(edge.Score))
                {
                    failureCount = 1;
                    diagnostic = "ranked score was not finite: edge=" +
                        edge.SourceEdgeIndex;
                    return false;
                }
                if (edge.Candidate != 0)
                {
                    if (edge.CandidateIndex < 0 ||
                        edge.CandidateIndex >= rankedCount)
                    {
                        failureCount = 1;
                        diagnostic =
                            "surviving candidate index outside original " +
                            "ranking universe: edge=" +
                            edge.SourceEdgeIndex + ",candidateIndex=" +
                            edge.CandidateIndex + ",count=" +
                            rankedCount;
                        return false;
                    }
                    if (survivingCandidateIndexSeen[
                            edge.CandidateIndex])
                    {
                        failureCount = 1;
                        diagnostic =
                            "duplicate surviving candidate index=" +
                            edge.CandidateIndex;
                        return false;
                    }
                    survivingCandidateIndexSeen[edge.CandidateIndex] =
                        true;
                }
                else if (edge.CandidateIndex >= 0)
                {
                    failureCount = 1;
                    diagnostic =
                        "post-coexistence excluded ranked edge retained " +
                        "candidate index: edge=" +
                        edge.SourceEdgeIndex + ",candidateIndex=" +
                        edge.CandidateIndex;
                    return false;
                }

                rankSeen[rankIndex] = true;
                edgeIdByRank[rankIndex] = edge.SourceEdgeIndex;
                scoreByRank[rankIndex] = edge.Score;
                outcome.ScoreByEdge.Add(
                    edge.SourceEdgeIndex,
                    edge.Score);
                outcome.RankByEdge.Add(
                    edge.SourceEdgeIndex,
                    rankIndex);
            }

            for (int index = 0; index < rankedCount; index++)
            {
                if (!rankSeen[index])
                {
                    failureCount = 1;
                    diagnostic = "missing production rank=" +
                        (index + 1);
                    return false;
                }
                if (index > 0 &&
                    scoreByRank[index - 1] +
                        EdgeWearArtisticRankScoreTolerance <
                    scoreByRank[index])
                {
                    failureCount = 1;
                    diagnostic = "score inversion at ranks=" + index +
                        "/" + (index + 1) + ",edges=" +
                        edgeIdByRank[index - 1] + "/" +
                        edgeIdByRank[index] + ",scores=" +
                        FormatEdgeWearArtisticFloat(
                            scoreByRank[index - 1]) + "/" +
                        FormatEdgeWearArtisticFloat(scoreByRank[index]);
                    return false;
                }
            }

            List<float> scores = new List<float>(rankedCount);
            StringBuilder hashBuilder = new StringBuilder();
            for (int rankIndex = 0;
                 rankIndex < rankedCount;
                 rankIndex++)
            {
                int edgeId = edgeIdByRank[rankIndex];
                outcome.RankedEdgeIds.Add(edgeId);
                scores.Add(scoreByRank[rankIndex]);
                if (rankIndex > 0)
                {
                    hashBuilder.Append('/');
                }
                hashBuilder.Append(edgeId);
            }
            outcome.ScoreMinimum =
                ResolveEdgeWearArtisticMinimum(scores);
            outcome.ScoreMedian =
                ResolveEdgeWearArtisticMedian(scores);
            outcome.ScoreMaximum =
                ResolveEdgeWearArtisticMaximum(scores);
            outcome.RankHash = CalculateEdgeWearArtisticStableHash(
                hashBuilder.ToString());
            return true;
        }

        private static void
            CalculateCurrentEdgeWearArtisticScoreReproduction(
                MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records,
                EdgeWearArtisticScenario currentScenario,
                out float maximumError,
                out int failureCount,
                out string diagnostic)
        {
            maximumError = 0f;
            failureCount = 0;
            diagnostic = "none";
            if (records == null || currentScenario == null)
            {
                failureCount = 1;
                diagnostic = "records or current scenario were unavailable";
                return;
            }
            HashSet<int> geometricSourceEdgeIds = new HashSet<int>();
            for (int edgeIndex = 0;
                 edgeIndex < records.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    records[edgeIndex];
                if (edge.GeometricEligible == 0)
                {
                    continue;
                }
                if (edge.SourceEdgeIndex < 0 ||
                    !geometricSourceEdgeIds.Add(edge.SourceEdgeIndex))
                {
                    failureCount++;
                    if (diagnostic == "none")
                    {
                        diagnostic = "invalid or duplicate geometric source " +
                            "edge id=" + edge.SourceEdgeIndex;
                    }
                    continue;
                }
                float reproduced =
                    CalculateEdgeWearArtisticScenarioScore(
                        edge,
                        currentScenario);
                if (float.IsNaN(reproduced) ||
                    float.IsInfinity(reproduced) ||
                    float.IsNaN(edge.Score) ||
                    float.IsInfinity(edge.Score))
                {
                    failureCount++;
                    if (diagnostic == "none")
                    {
                        diagnostic = "non-finite score for edge=" +
                            edge.SourceEdgeIndex;
                    }
                    continue;
                }
                float error = Mathf.Abs(reproduced - edge.Score);
                maximumError = Mathf.Max(maximumError, error);
                if (error > EdgeWearArtisticScoreReproductionTolerance)
                {
                    failureCount++;
                    if (diagnostic == "none")
                    {
                        diagnostic = "score mismatch for edge=" +
                            edge.SourceEdgeIndex + ",recorded=" +
                            FormatEdgeWearArtisticFloat(edge.Score) +
                            ",reproduced=" +
                            FormatEdgeWearArtisticFloat(reproduced) +
                            ",error=" +
                            FormatEdgeWearArtisticFloat(error);
                    }
                }
            }
        }

        private static EdgeWearArtisticScenarioOutcome
            EvaluateEdgeWearArtisticScenario(
                MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records,
                EdgeWearArtisticScenario scenario)
        {
            EdgeWearArtisticScenarioOutcome outcome =
                new EdgeWearArtisticScenarioOutcome
                {
                    Scenario = scenario
                };
            List<KeyValuePair<int, float>> ranked =
                new List<KeyValuePair<int, float>>(records.Length);
            for (int edgeIndex = 0;
                 edgeIndex < records.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    records[edgeIndex];
                if (!IsEdgeWearArtisticScenarioEligible(edge, scenario))
                {
                    continue;
                }
                float score = CalculateEdgeWearArtisticScenarioScore(
                    edge,
                    scenario);
                ranked.Add(new KeyValuePair<int, float>(
                    edge.SourceEdgeIndex,
                    score));
                outcome.ScoreByEdge[edge.SourceEdgeIndex] = score;
            }
            ranked.Sort((left, right) =>
            {
                int score = right.Value.CompareTo(left.Value);
                return score != 0
                    ? score
                    : left.Key.CompareTo(right.Key);
            });
            List<float> scores = new List<float>(ranked.Count);
            StringBuilder hashBuilder = new StringBuilder();
            for (int rank = 0; rank < ranked.Count; rank++)
            {
                int edgeId = ranked[rank].Key;
                outcome.RankedEdgeIds.Add(edgeId);
                outcome.RankByEdge[edgeId] = rank;
                scores.Add(ranked[rank].Value);
                if (rank > 0)
                {
                    hashBuilder.Append('/');
                }
                hashBuilder.Append(edgeId);
            }
            outcome.ScoreMinimum = ResolveEdgeWearArtisticMinimum(scores);
            outcome.ScoreMedian = ResolveEdgeWearArtisticMedian(scores);
            outcome.ScoreMaximum = ResolveEdgeWearArtisticMaximum(scores);
            outcome.RankHash = CalculateEdgeWearArtisticStableHash(
                hashBuilder.ToString());
            return outcome;
        }

        private static bool IsEdgeWearArtisticScenarioEligible(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge,
            EdgeWearArtisticScenario scenario)
        {
            if (edge.GeometricEligible == 0)
            {
                return false;
            }
            if ((scenario.GateMask & EdgeWearArtisticGateLength) != 0 &&
                edge.ArtisticLengthEligible == 0)
            {
                return false;
            }
            if ((scenario.GateMask & EdgeWearArtisticGateAngle) != 0 &&
                edge.ArtisticAngleEligible == 0)
            {
                return false;
            }
            return (scenario.GateMask & EdgeWearArtisticGateBase) == 0 ||
                edge.ArtisticBaseEligible != 0;
        }

        private static float CalculateEdgeWearArtisticScenarioScore(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge,
            EdgeWearArtisticScenario scenario)
        {
            float score =
                edge.ArtisticAngleScore * scenario.AngleWeight +
                edge.ArtisticLengthScore * scenario.LengthWeight +
                edge.ArtisticRandomScore * scenario.RandomWeight +
                ResolveEdgeWearArtisticDihedral01(edge) *
                    scenario.DihedralWeight +
                edge.ArtisticSilhouettePotential *
                    scenario.SilhouetteWeight +
                ResolveEdgeWearArtisticWidth01(edge) *
                    scenario.WidthWeight +
                (1f - edge.ArtisticLocalDensity01) *
                    scenario.IsolationWeight +
                ResolveEdgeWearArtisticLowCrowding01(edge) *
                    scenario.LowCrowdingWeight +
                edge.ArtisticEdgeAxisVertical01 *
                    scenario.VerticalWeight +
                (1f - edge.ArtisticEdgeAxisVertical01) *
                    scenario.HorizontalWeight +
                edge.ArtisticStrength * scenario.StrengthWeight +
                ResolveEdgeWearArtisticDepth01(edge) *
                    scenario.DepthWeight +
                ResolveEdgeWearArtisticLocality01(edge) *
                    scenario.LocalityWeight +
                edge.CoincidentBoundarySeamReconciled *
                    scenario.SeamWeight;
            if ((scenario.ModifierMask & EdgeWearArtisticModifierBase) != 0)
            {
                score *= ResolveEdgeWearArtisticBasePriorityFactor(edge);
            }
            if ((scenario.ModifierMask &
                 EdgeWearArtisticModifierUpward) != 0)
            {
                score *= ResolveEdgeWearArtisticUpwardPriorityFactor(edge);
            }
            if ((scenario.ModifierMask &
                 EdgeWearArtisticModifierCharacter) != 0)
            {
                score *= edge.ArtisticCharacterBoost;
            }
            return score;
        }

        private static float ResolveEdgeWearArtisticBasePriorityFactor(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            return Mathf.Lerp(
                0.60f,
                1.00f,
                Mathf.InverseLerp(
                    0.06f,
                    0.20f,
                    edge.ArtisticBaseSuppression));
        }

        private static float ResolveEdgeWearArtisticUpwardPriorityFactor(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            return Mathf.Lerp(
                0.925f,
                1.075f,
                Mathf.InverseLerp(
                    0.82f,
                    1.08f,
                    edge.ArtisticUpwardEdgeBoost));
        }

        private static float ResolveEdgeWearArtisticDihedral01(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            return Mathf.Clamp01((edge.DihedralDegrees - 15f) / 75f);
        }

        private static float ResolveEdgeWearArtisticWidth01(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            return Mathf.Clamp01(Mathf.Max(
                edge.ArtisticFeasibleWidthFraction,
                edge.ArtisticSolvedWidthFraction));
        }

        private static float ResolveEdgeWearArtisticLowCrowding01(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            int degree = edge.ArtisticSharedVertexDegreeA +
                edge.ArtisticSharedVertexDegreeB;
            return 1f - Mathf.Clamp01(degree / 6f);
        }

        private static float ResolveEdgeWearArtisticDepth01(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            return Mathf.InverseLerp(
                0.78f,
                1.15f,
                edge.ArtisticDepthMultiplier);
        }

        private static float ResolveEdgeWearArtisticLocality01(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge)
        {
            float denominator = Mathf.Max(
                0.000001f,
                edge.RequestedWidth * 4f);
            return Mathf.Clamp01(edge.LocalityFeasibleMargin / denominator);
        }

        private static void AppendEdgeWearArtisticScenarioDefinitions(
            StringBuilder builder,
            List<EdgeWearArtisticScenario> scenarios)
        {
            builder.AppendLine();
            builder.AppendLine("[Scenario Definitions]");
            for (int scenarioIndex = 0;
                 scenarioIndex < scenarios.Count;
                 scenarioIndex++)
            {
                EdgeWearArtisticScenario scenario = scenarios[scenarioIndex];
                if (!scenario.Named &&
                    !string.Equals(
                        scenario.Category,
                        "weight-simplex",
                        StringComparison.Ordinal))
                {
                    continue;
                }
                if (!scenario.Named && scenarioIndex % 64 != 0)
                {
                    continue;
                }
                builder.Append("scenario=");
                builder.Append(scenario.Name);
                builder.Append(",category=");
                builder.Append(scenario.Category);
                builder.Append(",weights=");
                AppendEdgeWearArtisticScenarioWeights(builder, scenario);
                builder.Append(",modifierMask=");
                builder.Append(scenario.ModifierMask);
                builder.Append(",gateMask=");
                builder.AppendLine(scenario.GateMask.ToString());
            }
            builder.AppendLine(
                "all scenario definitions and outcomes are present in Library/GeneratedMassEdgeWearArtisticComprehensiveScenarios.csv");
        }

        private static void AppendEdgeWearArtisticScenarioWeights(
            StringBuilder builder,
            EdgeWearArtisticScenario scenario)
        {
            builder.Append("angle:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.AngleWeight));
            builder.Append("/length:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.LengthWeight));
            builder.Append("/random:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.RandomWeight));
            builder.Append("/dihedral:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.DihedralWeight));
            builder.Append("/silhouette:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.SilhouetteWeight));
            builder.Append("/width:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.WidthWeight));
            builder.Append("/isolation:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.IsolationWeight));
            builder.Append("/lowCrowding:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.LowCrowdingWeight));
            builder.Append("/vertical:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.VerticalWeight));
            builder.Append("/horizontal:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.HorizontalWeight));
            builder.Append("/strength:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.StrengthWeight));
            builder.Append("/depth:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.DepthWeight));
            builder.Append("/locality:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.LocalityWeight));
            builder.Append("/seam:");
            builder.Append(FormatEdgeWearArtisticFloat(
                scenario.SeamWeight));
        }

        private static void AppendEdgeWearArtisticCaseEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis,
            List<EdgeWearArtisticScenario> scenarios)
        {
            MassGenerator.EdgeWearBatchAuditCaseResult result =
                analysis.MatrixCase.Result;
            builder.AppendLine();
            builder.Append("[Case seed=");
            builder.Append(analysis.MatrixCase.ShapeSeed);
            builder.Append(",width=");
            builder.Append(analysis.MatrixCase.WidthName);
            builder.Append('/');
            builder.Append(FormatEdgeWearArtisticFloat(
                analysis.MatrixCase.Width));
            builder.AppendLine("]");
            builder.Append("resultPassed=");
            builder.Append(result.Passed ? "1" : "0");
            builder.Append(",source/geometric/eligible/selected/certified=");
            builder.Append(result.SourceEdgeCount);
            builder.Append('/');
            builder.Append(result.GeometricEligibleCount);
            builder.Append('/');
            builder.Append(result.ArtisticEligibleCount);
            builder.Append('/');
            builder.Append(result.SelectedCount);
            builder.Append('/');
            builder.AppendLine(result.CertifiedCount.ToString());
            builder.Append("currentRankHash=");
            builder.Append(analysis.CurrentOutcome.RankHash);
            builder.Append(",currentScoreError=");
            builder.Append(FormatEdgeWearArtisticFloat(
                analysis.CurrentScoreReproductionMaximumError));
            builder.Append(",recordedProductionRanksValid=");
            builder.AppendLine(
                analysis.RecordedProductionRanksValid ? "1" : "0");

            AppendEdgeWearArtisticMetricEvidence(builder, analysis);
            AppendEdgeWearArtisticParetoEvidence(builder, analysis);
            AppendEdgeWearArtisticScenarioSensitivity(builder, analysis);
            AppendEdgeWearArtisticNamedScenarioEvidence(builder, analysis);
            AppendEdgeWearArtisticFixedSlotEvidence(builder, analysis);
            AppendEdgeWearArtisticNativeCoverageEvidence(builder, analysis);
            AppendEdgeWearArtisticRawEdgeEvidence(builder, analysis);
        }

        private static void AppendEdgeWearArtisticMetricEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            string[] metrics =
            {
                "angle",
                "length",
                "random",
                "dihedral",
                "silhouette",
                "width",
                "isolation",
                "lowCrowding",
                "vertical",
                "horizontal",
                "strength",
                "depth",
                "locality",
                "baseSuppression",
                "upwardBoost"
            };
            builder.AppendLine("metricCorrelations(score:pearson/spearman)=");
            for (int metricIndex = 0;
                 metricIndex < metrics.Length;
                 metricIndex++)
            {
                List<float> scores = new List<float>();
                List<float> values = new List<float>();
                for (int edgeIndex = 0;
                     edgeIndex < analysis.Edges.Length;
                     edgeIndex++)
                {
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                        analysis.Edges[edgeIndex];
                    if (edge.GeometricEligible == 0)
                    {
                        continue;
                    }
                    scores.Add(edge.Score);
                    values.Add(ResolveEdgeWearArtisticMetric(
                        edge,
                        metrics[metricIndex]));
                }
                builder.Append(metrics[metricIndex]);
                builder.Append(':');
                builder.Append(FormatEdgeWearArtisticFloat(
                    CalculateEdgeWearArtisticPearson(scores, values)));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    CalculateEdgeWearArtisticSpearman(scores, values)));
                builder.Append(",range=");
                builder.Append(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticMinimum(values)));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticMedian(values)));
                builder.Append('/');
                builder.AppendLine(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticMaximum(values)));
            }
        }

        private static float ResolveEdgeWearArtisticMetric(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord edge,
            string metric)
        {
            switch (metric)
            {
                case "angle":
                    return edge.ArtisticAngleScore;
                case "length":
                    return edge.ArtisticLengthScore;
                case "random":
                    return edge.ArtisticRandomScore;
                case "dihedral":
                    return ResolveEdgeWearArtisticDihedral01(edge);
                case "silhouette":
                    return edge.ArtisticSilhouettePotential;
                case "width":
                    return ResolveEdgeWearArtisticWidth01(edge);
                case "isolation":
                    return 1f - edge.ArtisticLocalDensity01;
                case "lowCrowding":
                    return ResolveEdgeWearArtisticLowCrowding01(edge);
                case "vertical":
                    return edge.ArtisticEdgeAxisVertical01;
                case "horizontal":
                    return 1f - edge.ArtisticEdgeAxisVertical01;
                case "strength":
                    return edge.ArtisticStrength;
                case "depth":
                    return ResolveEdgeWearArtisticDepth01(edge);
                case "locality":
                    return ResolveEdgeWearArtisticLocality01(edge);
                case "baseSuppression":
                    return edge.ArtisticBaseSuppression;
                case "upwardBoost":
                    return edge.ArtisticUpwardEdgeBoost;
                default:
                    return 0f;
            }
        }

        private static void AppendEdgeWearArtisticParetoEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            List<int> frontier = new List<int>();
            int dominanceInversions = 0;
            for (int edgeIndex = 0;
                 edgeIndex < analysis.Edges.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord candidate =
                    analysis.Edges[edgeIndex];
                if (candidate.GeometricEligible == 0)
                {
                    continue;
                }
                bool dominated = false;
                for (int otherIndex = 0;
                     otherIndex < analysis.Edges.Length;
                     otherIndex++)
                {
                    if (edgeIndex == otherIndex)
                    {
                        continue;
                    }
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord other =
                        analysis.Edges[otherIndex];
                    if (other.GeometricEligible == 0 ||
                        !DoesEdgeWearArtisticDominate(other, candidate))
                    {
                        continue;
                    }
                    dominated = true;
                    if (analysis.CurrentOutcome.RankByEdge.TryGetValue(
                            other.SourceEdgeIndex,
                            out int otherRank) &&
                        analysis.CurrentOutcome.RankByEdge.TryGetValue(
                            candidate.SourceEdgeIndex,
                            out int candidateRank) &&
                        otherRank > candidateRank)
                    {
                        dominanceInversions++;
                    }
                }
                if (!dominated)
                {
                    frontier.Add(candidate.SourceEdgeIndex);
                }
            }
            frontier.Sort();
            builder.Append("paretoFrontier=");
            builder.Append(FormatEdgeWearArtisticIdList(frontier));
            builder.Append(",dominanceRankInversions=");
            builder.AppendLine(dominanceInversions.ToString());
        }

        private static bool DoesEdgeWearArtisticDominate(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord left,
            MassGenerator.EdgeWearArtisticEdgeAuditRecord right)
        {
            float[] leftValues =
            {
                left.ArtisticAngleScore,
                left.ArtisticLengthScore,
                left.ArtisticSilhouettePotential,
                ResolveEdgeWearArtisticWidth01(left),
                1f - left.ArtisticLocalDensity01,
                ResolveEdgeWearArtisticLowCrowding01(left)
            };
            float[] rightValues =
            {
                right.ArtisticAngleScore,
                right.ArtisticLengthScore,
                right.ArtisticSilhouettePotential,
                ResolveEdgeWearArtisticWidth01(right),
                1f - right.ArtisticLocalDensity01,
                ResolveEdgeWearArtisticLowCrowding01(right)
            };
            bool strictlyBetter = false;
            for (int valueIndex = 0;
                 valueIndex < leftValues.Length;
                 valueIndex++)
            {
                if (leftValues[valueIndex] + 0.000001f <
                    rightValues[valueIndex])
                {
                    return false;
                }
                if (leftValues[valueIndex] >
                    rightValues[valueIndex] + 0.000001f)
                {
                    strictlyBetter = true;
                }
            }
            return strictlyBetter;
        }

        private static void AppendEdgeWearArtisticScenarioSensitivity(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            builder.AppendLine("scenarioSensitivityPerGeometricEdge=");
            for (int edgeIndex = 0;
                 edgeIndex < analysis.Edges.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    analysis.Edges[edgeIndex];
                if (edge.GeometricEligible == 0)
                {
                    continue;
                }
                int eligible = 0;
                int rankMinimum = int.MaxValue;
                int rankMaximum = -1;
                double rankTotal = 0d;
                int top25 = 0;
                int top50 = 0;
                int top75 = 0;
                for (int outcomeIndex = 0;
                     outcomeIndex < analysis.Outcomes.Count;
                     outcomeIndex++)
                {
                    EdgeWearArtisticScenarioOutcome outcome =
                        analysis.Outcomes[outcomeIndex];
                    if (!outcome.RankByEdge.TryGetValue(
                            edge.SourceEdgeIndex,
                            out int rank))
                    {
                        continue;
                    }
                    eligible++;
                    rankMinimum = Mathf.Min(rankMinimum, rank);
                    rankMaximum = Mathf.Max(rankMaximum, rank);
                    rankTotal += rank;
                    int count = outcome.RankedEdgeIds.Count;
                    if (rank < ResolveEdgeWearArtisticCoverageCount(
                            count,
                            0.25f))
                    {
                        top25++;
                    }
                    if (rank < ResolveEdgeWearArtisticCoverageCount(
                            count,
                            0.50f))
                    {
                        top50++;
                    }
                    if (rank < ResolveEdgeWearArtisticCoverageCount(
                            count,
                            0.75f))
                    {
                        top75++;
                    }
                }
                builder.Append("edge=");
                builder.Append(edge.SourceEdgeIndex);
                builder.Append(",eligibleScenarios=");
                builder.Append(eligible);
                builder.Append('/');
                builder.Append(analysis.Outcomes.Count);
                builder.Append(",rankMin/mean/max=");
                builder.Append(rankMinimum == int.MaxValue
                    ? "none"
                    : rankMinimum.ToString());
                builder.Append('/');
                builder.Append(eligible == 0
                    ? "none"
                    : FormatEdgeWearArtisticFloat(
                        (float)(rankTotal / eligible)));
                builder.Append('/');
                builder.Append(rankMaximum < 0
                    ? "none"
                    : rankMaximum.ToString());
                builder.Append(",top25/50/75Frequency=");
                builder.Append(eligible == 0
                    ? "0"
                    : FormatEdgeWearArtisticFloat((float)top25 / eligible));
                builder.Append('/');
                builder.Append(eligible == 0
                    ? "0"
                    : FormatEdgeWearArtisticFloat((float)top50 / eligible));
                builder.Append('/');
                builder.AppendLine(eligible == 0
                    ? "0"
                    : FormatEdgeWearArtisticFloat((float)top75 / eligible));
            }
        }

        private static void AppendEdgeWearArtisticNamedScenarioEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            builder.AppendLine("namedScenarioComparisons=");
            for (int outcomeIndex = 0;
                 outcomeIndex < analysis.Outcomes.Count;
                 outcomeIndex++)
            {
                EdgeWearArtisticScenarioOutcome outcome =
                    analysis.Outcomes[outcomeIndex];
                if (!outcome.Scenario.Named)
                {
                    continue;
                }
                builder.Append("scenario=");
                builder.Append(outcome.Scenario.Name);
                builder.Append(",eligible=");
                builder.Append(outcome.RankedEdgeIds.Count);
                builder.Append(",rankCorrelation=");
                builder.Append(FormatEdgeWearArtisticFloat(
                    CalculateEdgeWearArtisticOutcomeSpearman(
                        analysis.CurrentOutcome,
                        outcome)));
                builder.Append(",churn25/50/75=");
                builder.Append(CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    outcome,
                    ResolveEdgeWearArtisticCoverageCount(
                        analysis.CurrentOutcome.RankedEdgeIds.Count,
                        0.25f)));
                builder.Append('/');
                builder.Append(CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    outcome,
                    ResolveEdgeWearArtisticCoverageCount(
                        analysis.CurrentOutcome.RankedEdgeIds.Count,
                        0.50f)));
                builder.Append('/');
                builder.Append(CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    outcome,
                    ResolveEdgeWearArtisticCoverageCount(
                        analysis.CurrentOutcome.RankedEdgeIds.Count,
                        0.75f)));
                builder.Append(",top25=");
                builder.Append(FormatEdgeWearArtisticSelectedIds(
                    outcome,
                    ResolveEdgeWearArtisticCoverageCount(
                        outcome.RankedEdgeIds.Count,
                        0.25f)));
                builder.AppendLine();
            }
        }

        private static void AppendEdgeWearArtisticFixedSlotEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            builder.AppendLine("fixedSlotEvidence=");
            int maximumSlots =
                analysis.CurrentOutcome.RankedEdgeIds.Count;
            EdgeWearArtisticScenarioOutcome noRandom =
                FindEdgeWearArtisticOutcome(
                    analysis,
                    "current-no-random");
            for (int selectedCount = 1;
                 selectedCount <= maximumSlots;
                 selectedCount++)
            {
                HashSet<int> intersection = null;
                HashSet<int> union = new HashSet<int>();
                int maximumChurn = 0;
                int[] frequency = new int[analysis.Edges.Length];
                for (int outcomeIndex = 0;
                     outcomeIndex < analysis.Outcomes.Count;
                     outcomeIndex++)
                {
                    EdgeWearArtisticScenarioOutcome outcome =
                        analysis.Outcomes[outcomeIndex];
                    HashSet<int> selected =
                        GetEdgeWearArtisticSelectedSet(
                            outcome,
                            selectedCount);
                    union.UnionWith(selected);
                    if (intersection == null)
                    {
                        intersection = new HashSet<int>(selected);
                    }
                    else
                    {
                        intersection.IntersectWith(selected);
                    }
                    maximumChurn = Mathf.Max(
                        maximumChurn,
                        CalculateEdgeWearArtisticChurn(
                            analysis.CurrentOutcome,
                            outcome,
                            selectedCount));
                    foreach (int edgeId in selected)
                    {
                        int recordIndex = FindEdgeWearArtisticRecordIndex(
                            analysis.Edges,
                            edgeId);
                        if (recordIndex >= 0)
                        {
                            frequency[recordIndex]++;
                        }
                    }
                }
                List<int> core90 = new List<int>();
                for (int recordIndex = 0;
                     recordIndex < analysis.Edges.Length;
                     recordIndex++)
                {
                    if (frequency[recordIndex] >=
                        Mathf.CeilToInt(analysis.Outcomes.Count * 0.9f))
                    {
                        core90.Add(
                            analysis.Edges[recordIndex].SourceEdgeIndex);
                    }
                }
                float threshold = ResolveEdgeWearArtisticThreshold(
                    analysis.CurrentOutcome,
                    selectedCount);
                float gap = ResolveEdgeWearArtisticThresholdGap(
                    analysis.CurrentOutcome,
                    selectedCount);
                builder.Append("slots=");
                builder.Append(selectedCount);
                builder.Append(",threshold/gap=");
                builder.Append(FormatEdgeWearArtisticFloat(threshold));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(gap));
                builder.Append(",noRandomChurn=");
                builder.Append(CalculateEdgeWearArtisticChurn(
                    analysis.CurrentOutcome,
                    noRandom,
                    selectedCount));
                builder.Append(",maximumScenarioChurn=");
                builder.Append(maximumChurn);
                builder.Append(",intersection/union=");
                builder.Append(intersection == null ? 0 : intersection.Count);
                builder.Append('/');
                builder.Append(union.Count);
                builder.Append(",core90=");
                builder.AppendLine(FormatEdgeWearArtisticIdList(core90));
            }
        }

        private static void AppendEdgeWearArtisticNativeCoverageEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            builder.AppendLine("nativeCoverageDeciles=");
            for (int decile = 1; decile <= 10; decile++)
            {
                float coverage = decile / 10f;
                int selectedCount = ResolveEdgeWearArtisticCoverageCount(
                    analysis.CurrentOutcome.RankedEdgeIds.Count,
                    coverage);
                int maximumChurn = 0;
                double churnTotal = 0d;
                int evaluated = 0;
                for (int outcomeIndex = 0;
                     outcomeIndex < analysis.Outcomes.Count;
                     outcomeIndex++)
                {
                    int churn = CalculateEdgeWearArtisticChurn(
                        analysis.CurrentOutcome,
                        analysis.Outcomes[outcomeIndex],
                        selectedCount);
                    maximumChurn = Mathf.Max(maximumChurn, churn);
                    churnTotal += churn;
                    evaluated++;
                }
                builder.Append("coverage=");
                builder.Append(FormatEdgeWearArtisticFloat(coverage));
                builder.Append(",slots=");
                builder.Append(selectedCount);
                builder.Append(",current=");
                builder.Append(FormatEdgeWearArtisticSelectedIds(
                    analysis.CurrentOutcome,
                    selectedCount));
                builder.Append(",threshold/gap=");
                builder.Append(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticThreshold(
                        analysis.CurrentOutcome,
                        selectedCount)));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticThresholdGap(
                        analysis.CurrentOutcome,
                        selectedCount)));
                builder.Append(",scenarioChurnMean/max=");
                builder.Append(FormatEdgeWearArtisticFloat(
                    evaluated == 0 ? 0f : (float)(churnTotal / evaluated)));
                builder.Append('/');
                builder.AppendLine(maximumChurn.ToString());
            }
        }

        private static void AppendEdgeWearArtisticRawEdgeEvidence(
            StringBuilder builder,
            EdgeWearArtisticCaseAnalysis analysis)
        {
            builder.AppendLine("rawPerEdgeEvidence=");
            for (int edgeIndex = 0;
                 edgeIndex < analysis.Edges.Length;
                 edgeIndex++)
            {
                MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                    analysis.Edges[edgeIndex];
                builder.Append("edge=");
                builder.Append(edge.SourceEdgeIndex);
                builder.Append(",candidateIndex=");
                builder.Append(edge.CandidateIndex);
                builder.Append(",geometry={start:");
                builder.Append(FormatEdgeWearArtisticVector(edge.Start));
                builder.Append(",end:");
                builder.Append(FormatEdgeWearArtisticVector(edge.End));
                builder.Append(",mid:");
                builder.Append(FormatEdgeWearArtisticVector(edge.Midpoint));
                builder.Append(",length:");
                builder.Append(FormatEdgeWearArtisticFloat(edge.Length));
                builder.Append(",dihedral:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.DihedralDegrees));
                builder.Append(",faces:");
                builder.Append(edge.FaceA);
                builder.Append('/');
                builder.Append(edge.FaceB);
                builder.Append('/');
                builder.Append(edge.FaceCount);
                builder.Append(",classification:");
                builder.Append(edge.Classification);
                builder.Append(",seam:");
                builder.Append(edge.CoincidentBoundarySeamReconciled);
                builder.Append("},normals={ownerA:");
                builder.Append(FormatEdgeWearArtisticVector(
                    edge.OwnerNormalA));
                builder.Append(",ownerB:");
                builder.Append(FormatEdgeWearArtisticVector(
                    edge.OwnerNormalB));
                builder.Append(",bevel:");
                builder.Append(FormatEdgeWearArtisticVector(
                    edge.BevelNormal));
                builder.Append("},eligibility={structural/geometric/coexistence/artistic:");
                builder.Append(edge.StructuralEligible);
                builder.Append('/');
                builder.Append(edge.GeometricEligible);
                builder.Append('/');
                builder.Append(edge.CoexistenceEligible);
                builder.Append('/');
                builder.Append(edge.ArtisticEligible);
                builder.Append(",gates:");
                builder.Append(edge.ArtisticLengthEligible);
                builder.Append('/');
                builder.Append(edge.ArtisticAngleEligible);
                builder.Append('/');
                builder.Append(edge.ArtisticBaseEligible);
                builder.Append(",filter:");
                builder.Append(edge.ArtisticFilterReason);
                builder.Append("},score={final:");
                builder.Append(FormatEdgeWearArtisticFloat(edge.Score));
                builder.Append(",angle/length/random:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticAngleScore));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticLengthScore));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticRandomScore));
                builder.Append(",base/upward/character:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticBaseSuppression));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticUpwardEdgeBoost));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticCharacterBoost));
                builder.Append(",rank/threshold/delta:");
                builder.Append(edge.ArtisticSelectionRank);
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticSelectionThreshold));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticSelectionDelta));
                builder.Append("},context={axisVertical/absXYZ:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticEdgeAxisVertical01));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticEdgeAxisAbsX));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticEdgeAxisAbsY));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticEdgeAxisAbsZ));
                builder.Append(",silhouette/width/density/crowding:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticSilhouettePotential));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    ResolveEdgeWearArtisticWidth01(edge)));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticLocalDensity01));
                builder.Append('/');
                builder.Append(edge.ArtisticSharedVertexDegreeA);
                builder.Append('/');
                builder.Append(edge.ArtisticSharedVertexDegreeB);
                builder.Append("},viability={requested/footprint/ratio:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.RequestedWidth));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.RequiredFootprintLength));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LengthToWidthRatio));
                builder.Append(",localityFloor/ceiling/margin/guard/minRemoval:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LocalityRetainPlaneFloor));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LocalityRemovalPlaneCeiling));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LocalityFeasibleMargin));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LocalityGuardMargin));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.LocalityMinimumRemoval));
                builder.Append(",maximumWidth/fraction:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.MaximumLocallyFeasibleWidth));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.FeasibleWidthFraction));
                builder.Append(",isolated:");
                builder.Append(edge.IsolatedSucceeded);
                builder.Append('/');
                builder.Append(edge.IsolatedWidthAttemptCount);
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.IsolatedMaximumCertifiedWidthFraction));
                builder.Append('/');
                builder.Append(edge.IsolatedOpenEdgeCount);
                builder.Append('/');
                builder.Append(edge.IsolatedNonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(edge.IsolatedTJunctionCount);
                builder.Append('/');
                builder.Append(edge.IsolatedInvalidFaceCount);
                builder.Append(",failure:");
                builder.Append(edge.ViabilityFailureReason);
                builder.Append("},effect={variation/strength/depth:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticDeterministicVariation));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticStrength));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.ArtisticDepthMultiplier));
                builder.Append("},lifecycle={candidate/selected/active/attempted/certified/deferred/rejected:");
                builder.Append(edge.Candidate);
                builder.Append('/');
                builder.Append(edge.Selected);
                builder.Append('/');
                builder.Append(edge.Active);
                builder.Append('/');
                builder.Append(edge.AttemptedBuilt);
                builder.Append('/');
                builder.Append(edge.CertifiedBuilt);
                builder.Append('/');
                builder.Append(edge.Deferred);
                builder.Append('/');
                builder.Append(edge.Rejected);
                builder.Append(",solved/materialized/scale:");
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.SolvedWidth));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.MaterializedWidth));
                builder.Append('/');
                builder.Append(FormatEdgeWearArtisticFloat(
                    edge.MaterializedWidthScale));
                builder.Append(",reason:");
                builder.Append(edge.FinalReason);
                builder.AppendLine("}");
            }
        }

        private static void AppendEdgeWearArtisticCrossWidthEvidence(
            StringBuilder builder,
            List<EdgeWearArtisticCaseAnalysis> analyses)
        {
            builder.AppendLine();
            builder.AppendLine("[Cross Width Stability]");
            for (int seedIndex = 0;
                 seedIndex < EdgeWearBatchShapeSeeds.Length;
                 seedIndex++)
            {
                int seed = EdgeWearBatchShapeSeeds[seedIndex];
                EdgeWearArtisticCaseAnalysis minimum = null;
                EdgeWearArtisticCaseAnalysis normal = null;
                EdgeWearArtisticCaseAnalysis maximum = null;
                for (int caseIndex = 0;
                     caseIndex < analyses.Count;
                     caseIndex++)
                {
                    EdgeWearArtisticCaseAnalysis analysis =
                        analyses[caseIndex];
                    if (analysis.MatrixCase.ShapeSeed != seed)
                    {
                        continue;
                    }
                    switch (analysis.MatrixCase.WidthName)
                    {
                        case "minimum":
                            minimum = analysis;
                            break;
                        case "default":
                            normal = analysis;
                            break;
                        case "maximum":
                            maximum = analysis;
                            break;
                    }
                }
                builder.Append("seed=");
                builder.Append(seed);
                AppendEdgeWearArtisticCrossWidthPair(
                    builder,
                    "min-default",
                    minimum,
                    normal);
                AppendEdgeWearArtisticCrossWidthPair(
                    builder,
                    "default-max",
                    normal,
                    maximum);
                AppendEdgeWearArtisticCrossWidthPair(
                    builder,
                    "min-max",
                    minimum,
                    maximum);
                builder.AppendLine();
            }
        }

        private static void AppendEdgeWearArtisticCrossWidthPair(
            StringBuilder builder,
            string name,
            EdgeWearArtisticCaseAnalysis left,
            EdgeWearArtisticCaseAnalysis right)
        {
            builder.Append(',');
            builder.Append(name);
            builder.Append('=');
            if (left == null || right == null)
            {
                builder.Append("missing");
                return;
            }
            builder.Append("rank:");
            builder.Append(FormatEdgeWearArtisticFloat(
                CalculateEdgeWearArtisticOutcomeSpearman(
                    left.CurrentOutcome,
                    right.CurrentOutcome)));
            builder.Append("/jaccard25:");
            builder.Append(FormatEdgeWearArtisticFloat(
                CalculateEdgeWearArtisticJaccard(
                    left.CurrentOutcome,
                    right.CurrentOutcome,
                    0.25f)));
            builder.Append("/jaccard50:");
            builder.Append(FormatEdgeWearArtisticFloat(
                CalculateEdgeWearArtisticJaccard(
                    left.CurrentOutcome,
                    right.CurrentOutcome,
                    0.50f)));
            builder.Append("/jaccard75:");
            builder.Append(FormatEdgeWearArtisticFloat(
                CalculateEdgeWearArtisticJaccard(
                    left.CurrentOutcome,
                    right.CurrentOutcome,
                    0.75f)));
        }

        private static string BuildEdgeWearArtisticComprehensiveEdgesCsv(
            List<EdgeWearArtisticCaseAnalysis> analyses)
        {
            StringBuilder builder = new StringBuilder(524288);
            builder.AppendLine(
                "seed,widthName,width,sourceEdge,candidateIndex,start,end,midpoint,ownerNormalA,ownerNormalB,bevelNormal,faceA,faceB,faceCount,length,dihedral,vertical01,classification,seamReconciled,structural,geometric,coexistence,artistic,lengthGate,angleGate,baseGate,filterReason,candidateReason,finalReason,score,minimumLength,lengthScore,angleScore,randomScore,baseSuppression,upwardBoost,characterBoost,axisVertical,axisAbsX,axisAbsY,axisAbsZ,silhouette,feasibleWidthFraction,solvedWidthFraction,localDensity,degreeA,degreeB,selectionRank,selectionThreshold,selectionDelta,deterministicVariation,strength,depthMultiplier,requestedWidth,requiredFootprint,lengthToWidthRatio,localityFloor,localityCeiling,localityMargin,localityGuard,localityMinimumRemoval,localityLimitingVertex,localityLimitingPosition,maximumLocallyFeasibleWidth,feasibleWidthFractionRaw,isolatedSucceeded,isolatedAttempts,isolatedLastWidth,isolatedMaximumWidth,isolatedMaximumFraction,endpointConsumptionA,endpointConsumptionB,remainingSpan,minimumSpan,isolatedOpen,isolatedNonManifold,isolatedTJunction,isolatedInvalidFace,isolatedDiagnostic,viabilityFailure,solvedWidth,materializedWidth,materializedScale,widthReduced,candidate,selected,widthInactive,active,attempted,certified,trialRejected,deferred,rejected");
            for (int caseIndex = 0;
                 caseIndex < analyses.Count;
                 caseIndex++)
            {
                EdgeWearArtisticCaseAnalysis analysis = analyses[caseIndex];
                for (int edgeIndex = 0;
                     edgeIndex < analysis.Edges.Length;
                     edgeIndex++)
                {
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord edge =
                        analysis.Edges[edgeIndex];
                    AppendEdgeWearArtisticCsv(builder,
                        analysis.MatrixCase.ShapeSeed.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        analysis.MatrixCase.WidthName);
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            analysis.MatrixCase.Width));
                    AppendEdgeWearArtisticCsv(builder,
                        edge.SourceEdgeIndex.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.CandidateIndex.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.Start));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.End));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.Midpoint));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.OwnerNormalA));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.OwnerNormalB));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(edge.BevelNormal));
                    AppendEdgeWearArtisticCsv(builder, edge.FaceA.ToString());
                    AppendEdgeWearArtisticCsv(builder, edge.FaceB.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.FaceCount.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(edge.Length));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            edge.DihedralDegrees));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(edge.Vertical01));
                    AppendEdgeWearArtisticCsv(builder,
                        edge.Classification);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.CoincidentBoundarySeamReconciled.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.StructuralEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.GeometricEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.CoexistenceEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticLengthEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticAngleEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticBaseEligible.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticFilterReason);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.CandidateReason);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.FinalReason);
                    float[] firstFloats =
                    {
                        edge.Score,
                        edge.ArtisticMinimumLength,
                        edge.ArtisticLengthScore,
                        edge.ArtisticAngleScore,
                        edge.ArtisticRandomScore,
                        edge.ArtisticBaseSuppression,
                        edge.ArtisticUpwardEdgeBoost,
                        edge.ArtisticCharacterBoost,
                        edge.ArtisticEdgeAxisVertical01,
                        edge.ArtisticEdgeAxisAbsX,
                        edge.ArtisticEdgeAxisAbsY,
                        edge.ArtisticEdgeAxisAbsZ,
                        edge.ArtisticSilhouettePotential,
                        edge.ArtisticFeasibleWidthFraction,
                        edge.ArtisticSolvedWidthFraction,
                        edge.ArtisticLocalDensity01
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, firstFloats);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticSharedVertexDegreeA.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticSharedVertexDegreeB.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ArtisticSelectionRank.ToString());
                    float[] secondFloats =
                    {
                        edge.ArtisticSelectionThreshold,
                        edge.ArtisticSelectionDelta,
                        edge.ArtisticDeterministicVariation,
                        edge.ArtisticStrength,
                        edge.ArtisticDepthMultiplier,
                        edge.RequestedWidth,
                        edge.RequiredFootprintLength,
                        edge.LengthToWidthRatio,
                        edge.LocalityRetainPlaneFloor,
                        edge.LocalityRemovalPlaneCeiling,
                        edge.LocalityFeasibleMargin,
                        edge.LocalityGuardMargin,
                        edge.LocalityMinimumRemoval
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, secondFloats);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.LocalityLimitingVertex.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticVector(
                            edge.LocalityLimitingPosition));
                    float[] thirdFloats =
                    {
                        edge.MaximumLocallyFeasibleWidth,
                        edge.FeasibleWidthFraction
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, thirdFloats);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedSucceeded.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedWidthAttemptCount.ToString());
                    float[] fourthFloats =
                    {
                        edge.IsolatedLastAttemptedWidth,
                        edge.IsolatedMaximumCertifiedWidth,
                        edge.IsolatedMaximumCertifiedWidthFraction,
                        edge.EndpointConsumptionA,
                        edge.EndpointConsumptionB,
                        edge.RemainingCentralSpan,
                        edge.MinimumCentralSpan
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, fourthFloats);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedOpenEdgeCount.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedNonManifoldEdgeCount.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedTJunctionCount.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedInvalidFaceCount.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        edge.IsolatedDiagnostic);
                    AppendEdgeWearArtisticCsv(builder,
                        edge.ViabilityFailureReason);
                    float[] fifthFloats =
                    {
                        edge.SolvedWidth,
                        edge.MaterializedWidth,
                        edge.MaterializedWidthScale
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, fifthFloats);
                    int[] finalInts =
                    {
                        edge.WidthReduced,
                        edge.Candidate,
                        edge.Selected,
                        edge.WidthInactive,
                        edge.Active,
                        edge.AttemptedBuilt,
                        edge.CertifiedBuilt,
                        edge.TrialRejected,
                        edge.Deferred,
                        edge.Rejected
                    };
                    for (int intIndex = 0;
                         intIndex < finalInts.Length;
                         intIndex++)
                    {
                        AppendEdgeWearArtisticCsv(
                            builder,
                            finalInts[intIndex].ToString(),
                            intIndex == finalInts.Length - 1);
                    }
                }
            }
            return builder.ToString();
        }

        private static string BuildEdgeWearArtisticComprehensiveScenariosCsv(
            List<EdgeWearArtisticCaseAnalysis> analyses,
            List<EdgeWearArtisticScenario> scenarios)
        {
            StringBuilder builder = new StringBuilder(8388608);
            builder.AppendLine(
                "seed,widthName,width,scenario,category,named,angleWeight,lengthWeight,randomWeight,dihedralWeight,silhouetteWeight,widthWeight,isolationWeight,lowCrowdingWeight,verticalWeight,horizontalWeight,strengthWeight,depthWeight,localityWeight,seamWeight,modifierMask,gateMask,eligibleCount,scoreMinimum,scoreMedian,scoreMaximum,rankHash,rankedIds,currentRankSpearman,churn25,churn50,churn75");
            for (int caseIndex = 0;
                 caseIndex < analyses.Count;
                 caseIndex++)
            {
                EdgeWearArtisticCaseAnalysis analysis = analyses[caseIndex];
                for (int outcomeIndex = 0;
                     outcomeIndex < analysis.Outcomes.Count;
                     outcomeIndex++)
                {
                    EdgeWearArtisticScenarioOutcome outcome =
                        analysis.Outcomes[outcomeIndex];
                    EdgeWearArtisticScenario scenario = outcome.Scenario;
                    AppendEdgeWearArtisticCsv(builder,
                        analysis.MatrixCase.ShapeSeed.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        analysis.MatrixCase.WidthName);
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            analysis.MatrixCase.Width));
                    AppendEdgeWearArtisticCsv(builder, scenario.Name);
                    AppendEdgeWearArtisticCsv(builder, scenario.Category);
                    AppendEdgeWearArtisticCsv(builder,
                        scenario.Named ? "1" : "0");
                    float[] weights =
                    {
                        scenario.AngleWeight,
                        scenario.LengthWeight,
                        scenario.RandomWeight,
                        scenario.DihedralWeight,
                        scenario.SilhouetteWeight,
                        scenario.WidthWeight,
                        scenario.IsolationWeight,
                        scenario.LowCrowdingWeight,
                        scenario.VerticalWeight,
                        scenario.HorizontalWeight,
                        scenario.StrengthWeight,
                        scenario.DepthWeight,
                        scenario.LocalityWeight,
                        scenario.SeamWeight
                    };
                    AppendEdgeWearArtisticCsvFloats(builder, weights);
                    AppendEdgeWearArtisticCsv(builder,
                        scenario.ModifierMask.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        scenario.GateMask.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        outcome.RankedEdgeIds.Count.ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            outcome.ScoreMinimum));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            outcome.ScoreMedian));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            outcome.ScoreMaximum));
                    AppendEdgeWearArtisticCsv(builder, outcome.RankHash);
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticIdList(
                            outcome.RankedEdgeIds));
                    AppendEdgeWearArtisticCsv(builder,
                        FormatEdgeWearArtisticFloat(
                            CalculateEdgeWearArtisticOutcomeSpearman(
                                analysis.CurrentOutcome,
                                outcome)));
                    int currentCount =
                        analysis.CurrentOutcome.RankedEdgeIds.Count;
                    AppendEdgeWearArtisticCsv(builder,
                        CalculateEdgeWearArtisticChurn(
                            analysis.CurrentOutcome,
                            outcome,
                            ResolveEdgeWearArtisticCoverageCount(
                                currentCount,
                                0.25f)).ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        CalculateEdgeWearArtisticChurn(
                            analysis.CurrentOutcome,
                            outcome,
                            ResolveEdgeWearArtisticCoverageCount(
                                currentCount,
                                0.50f)).ToString());
                    AppendEdgeWearArtisticCsv(builder,
                        CalculateEdgeWearArtisticChurn(
                            analysis.CurrentOutcome,
                            outcome,
                            ResolveEdgeWearArtisticCoverageCount(
                                currentCount,
                                0.75f)).ToString(),
                        true);
                }
            }
            return builder.ToString();
        }

        private static bool WriteEdgeWearArtisticComprehensiveReports(
            string report,
            string edgesCsv,
            string scenariosCsv,
            out string diagnostic)
        {
            try
            {
                File.WriteAllText(
                    GetEdgeWearLibraryPath(
                        EdgeWearArtisticComprehensiveReportFileName),
                    report ?? string.Empty,
                    new UTF8Encoding(false));
                File.WriteAllText(
                    GetEdgeWearLibraryPath(
                        EdgeWearArtisticComprehensiveEdgesCsvFileName),
                    edgesCsv ?? string.Empty,
                    new UTF8Encoding(false));
                File.WriteAllText(
                    GetEdgeWearLibraryPath(
                        EdgeWearArtisticComprehensiveScenariosCsvFileName),
                    scenariosCsv ?? string.Empty,
                    new UTF8Encoding(false));
                diagnostic = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                diagnostic = "comprehensive artistic report write failed: " +
                    exception.GetType().Name + ":" + exception.Message;
                return false;
            }
        }

        private static EdgeWearArtisticScenarioOutcome
            FindEdgeWearArtisticOutcome(
                EdgeWearArtisticCaseAnalysis analysis,
                string scenarioName)
        {
            if (analysis == null)
            {
                return null;
            }
            for (int outcomeIndex = 0;
                 outcomeIndex < analysis.Outcomes.Count;
                 outcomeIndex++)
            {
                EdgeWearArtisticScenarioOutcome outcome =
                    analysis.Outcomes[outcomeIndex];
                if (string.Equals(
                        outcome.Scenario.Name,
                        scenarioName,
                        StringComparison.Ordinal))
                {
                    return outcome;
                }
            }
            return null;
        }

        private static int ResolveEdgeWearArtisticCoverageCount(
            int eligibleCount,
            float coverage)
        {
            if (eligibleCount <= 0 || coverage <= 0f)
            {
                return 0;
            }
            return Mathf.Clamp(
                Mathf.CeilToInt(eligibleCount * Mathf.Clamp01(coverage)),
                1,
                eligibleCount);
        }

        private static HashSet<int> GetEdgeWearArtisticSelectedSet(
            EdgeWearArtisticScenarioOutcome outcome,
            int selectedCount)
        {
            HashSet<int> selected = new HashSet<int>();
            if (outcome == null || selectedCount <= 0)
            {
                return selected;
            }
            int count = Mathf.Min(
                selectedCount,
                outcome.RankedEdgeIds.Count);
            for (int index = 0; index < count; index++)
            {
                selected.Add(outcome.RankedEdgeIds[index]);
            }
            return selected;
        }

        private static int CalculateEdgeWearArtisticChurn(
            EdgeWearArtisticScenarioOutcome left,
            EdgeWearArtisticScenarioOutcome right,
            int selectedCount)
        {
            if (left == null || right == null)
            {
                return selectedCount;
            }
            HashSet<int> leftSet = GetEdgeWearArtisticSelectedSet(
                left,
                selectedCount);
            HashSet<int> rightSet = GetEdgeWearArtisticSelectedSet(
                right,
                selectedCount);
            leftSet.SymmetricExceptWith(rightSet);
            return leftSet.Count / 2;
        }

        private static float CalculateEdgeWearArtisticJaccard(
            EdgeWearArtisticScenarioOutcome left,
            EdgeWearArtisticScenarioOutcome right,
            float coverage)
        {
            if (left == null || right == null)
            {
                return 0f;
            }
            int selectedCount = ResolveEdgeWearArtisticCoverageCount(
                Mathf.Min(
                    left.RankedEdgeIds.Count,
                    right.RankedEdgeIds.Count),
                coverage);
            HashSet<int> leftSet = GetEdgeWearArtisticSelectedSet(
                left,
                selectedCount);
            HashSet<int> rightSet = GetEdgeWearArtisticSelectedSet(
                right,
                selectedCount);
            HashSet<int> intersection = new HashSet<int>(leftSet);
            intersection.IntersectWith(rightSet);
            leftSet.UnionWith(rightSet);
            return leftSet.Count == 0
                ? 1f
                : (float)intersection.Count / leftSet.Count;
        }

        private static float ResolveEdgeWearArtisticThreshold(
            EdgeWearArtisticScenarioOutcome outcome,
            int selectedCount)
        {
            if (outcome == null || selectedCount <= 0 ||
                selectedCount > outcome.RankedEdgeIds.Count)
            {
                return 0f;
            }
            int edgeId = outcome.RankedEdgeIds[selectedCount - 1];
            return outcome.ScoreByEdge.TryGetValue(edgeId, out float score)
                ? score
                : 0f;
        }

        private static float ResolveEdgeWearArtisticThresholdGap(
            EdgeWearArtisticScenarioOutcome outcome,
            int selectedCount)
        {
            if (outcome == null || selectedCount <= 0 ||
                selectedCount >= outcome.RankedEdgeIds.Count)
            {
                return 0f;
            }
            float selected = ResolveEdgeWearArtisticThreshold(
                outcome,
                selectedCount);
            int nextId = outcome.RankedEdgeIds[selectedCount];
            float next = outcome.ScoreByEdge.TryGetValue(
                    nextId,
                    out float nextScore)
                ? nextScore
                : 0f;
            return selected - next;
        }

        private static string FormatEdgeWearArtisticSelectedIds(
            EdgeWearArtisticScenarioOutcome outcome,
            int selectedCount)
        {
            if (outcome == null || selectedCount <= 0)
            {
                return "none";
            }
            List<int> ids = new List<int>();
            int count = Mathf.Min(
                selectedCount,
                outcome.RankedEdgeIds.Count);
            for (int index = 0; index < count; index++)
            {
                ids.Add(outcome.RankedEdgeIds[index]);
            }
            return FormatEdgeWearArtisticIdList(ids);
        }

        private static int FindEdgeWearArtisticRecordIndex(
            MassGenerator.EdgeWearArtisticEdgeAuditRecord[] edges,
            int sourceEdgeId)
        {
            for (int edgeIndex = 0;
                 edgeIndex < edges.Length;
                 edgeIndex++)
            {
                if (edges[edgeIndex].SourceEdgeIndex == sourceEdgeId)
                {
                    return edgeIndex;
                }
            }
            return -1;
        }

        private static float CalculateEdgeWearArtisticOutcomeSpearman(
            EdgeWearArtisticScenarioOutcome left,
            EdgeWearArtisticScenarioOutcome right)
        {
            if (left == null || right == null)
            {
                return 0f;
            }
            List<float> leftRanks = new List<float>();
            List<float> rightRanks = new List<float>();
            foreach (KeyValuePair<int, int> pair in left.RankByEdge)
            {
                if (!right.RankByEdge.TryGetValue(
                        pair.Key,
                        out int rightRank))
                {
                    continue;
                }
                leftRanks.Add(pair.Value);
                rightRanks.Add(rightRank);
            }
            return CalculateEdgeWearArtisticPearson(
                leftRanks,
                rightRanks);
        }

        private static float CalculateEdgeWearArtisticPearson(
            List<float> left,
            List<float> right)
        {
            if (left == null || right == null ||
                left.Count != right.Count || left.Count < 2)
            {
                return 0f;
            }
            double leftMean = 0d;
            double rightMean = 0d;
            for (int index = 0; index < left.Count; index++)
            {
                leftMean += left[index];
                rightMean += right[index];
            }
            leftMean /= left.Count;
            rightMean /= right.Count;
            double numerator = 0d;
            double leftVariance = 0d;
            double rightVariance = 0d;
            for (int index = 0; index < left.Count; index++)
            {
                double leftDelta = left[index] - leftMean;
                double rightDelta = right[index] - rightMean;
                numerator += leftDelta * rightDelta;
                leftVariance += leftDelta * leftDelta;
                rightVariance += rightDelta * rightDelta;
            }
            double denominator = Math.Sqrt(
                leftVariance * rightVariance);
            return denominator <= 0.0000000001d
                ? 0f
                : (float)(numerator / denominator);
        }

        private static float CalculateEdgeWearArtisticSpearman(
            List<float> left,
            List<float> right)
        {
            if (left == null || right == null ||
                left.Count != right.Count || left.Count < 2)
            {
                return 0f;
            }
            return CalculateEdgeWearArtisticPearson(
                CalculateEdgeWearArtisticRanks(left),
                CalculateEdgeWearArtisticRanks(right));
        }

        private static List<float> CalculateEdgeWearArtisticRanks(
            List<float> values)
        {
            List<KeyValuePair<int, float>> sorted =
                new List<KeyValuePair<int, float>>(values.Count);
            for (int index = 0; index < values.Count; index++)
            {
                sorted.Add(new KeyValuePair<int, float>(
                    index,
                    values[index]));
            }
            sorted.Sort((left, right) =>
            {
                int comparison = left.Value.CompareTo(right.Value);
                return comparison != 0
                    ? comparison
                    : left.Key.CompareTo(right.Key);
            });
            List<float> ranks = new List<float>(values.Count);
            for (int index = 0; index < values.Count; index++)
            {
                ranks.Add(0f);
            }
            int cursor = 0;
            while (cursor < sorted.Count)
            {
                int end = cursor + 1;
                while (end < sorted.Count &&
                    Mathf.Abs(sorted[end].Value -
                        sorted[cursor].Value) <= 0.0000001f)
                {
                    end++;
                }
                float averageRank = (cursor + end - 1) * 0.5f;
                for (int rankIndex = cursor;
                     rankIndex < end;
                     rankIndex++)
                {
                    ranks[sorted[rankIndex].Key] = averageRank;
                }
                cursor = end;
            }
            return ranks;
        }

        private static float ResolveEdgeWearArtisticMinimum(
            List<float> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }
            float minimum = values[0];
            for (int index = 1; index < values.Count; index++)
            {
                minimum = Mathf.Min(minimum, values[index]);
            }
            return minimum;
        }

        private static float ResolveEdgeWearArtisticMaximum(
            List<float> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }
            float maximum = values[0];
            for (int index = 1; index < values.Count; index++)
            {
                maximum = Mathf.Max(maximum, values[index]);
            }
            return maximum;
        }

        private static float ResolveEdgeWearArtisticMedian(
            List<float> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }
            List<float> sorted = new List<float>(values);
            sorted.Sort();
            int midpoint = sorted.Count / 2;
            return sorted.Count % 2 == 0
                ? (sorted[midpoint - 1] + sorted[midpoint]) * 0.5f
                : sorted[midpoint];
        }

        private static string CalculateEdgeWearArtisticStableHash(
            string value)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string text = value ?? string.Empty;
                for (int index = 0; index < text.Length; index++)
                {
                    hash ^= text[index];
                    hash *= 16777619u;
                }
                return hash.ToString("X8", CultureInfo.InvariantCulture);
            }
        }

        private static string FormatEdgeWearArtisticIdList(
            IList<int> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return "none";
            }
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < ids.Count; index++)
            {
                if (index > 0)
                {
                    builder.Append('/');
                }
                builder.Append(ids[index]);
            }
            return builder.ToString();
        }

        private static string FormatEdgeWearArtisticFloat(float value)
        {
            return value.ToString("G9", CultureInfo.InvariantCulture);
        }

        private static string FormatEdgeWearArtisticVector(Vector3 value)
        {
            return "(" +
                FormatEdgeWearArtisticFloat(value.x) + "/" +
                FormatEdgeWearArtisticFloat(value.y) + "/" +
                FormatEdgeWearArtisticFloat(value.z) + ")";
        }

        private static void AppendEdgeWearArtisticCsvFloats(
            StringBuilder builder,
            float[] values)
        {
            for (int index = 0; index < values.Length; index++)
            {
                AppendEdgeWearArtisticCsv(
                    builder,
                    FormatEdgeWearArtisticFloat(values[index]));
            }
        }

        private static void AppendEdgeWearArtisticCsv(
            StringBuilder builder,
            string value,
            bool endOfLine = false)
        {
            value ??= string.Empty;
            bool quote = value.IndexOfAny(
                new[] { ',', '"', '\r', '\n' }) >= 0;
            if (quote)
            {
                builder.Append('"');
                builder.Append(value.Replace("\"", "\"\""));
                builder.Append('"');
            }
            else
            {
                builder.Append(value);
            }
            if (endOfLine)
            {
                builder.AppendLine();
            }
            else
            {
                builder.Append(',');
            }
        }

        private static bool WriteEdgeWearValidationSuiteReport(
            string report,
            out string diagnostic)
        {
            try
            {
                File.WriteAllText(
                    GetEdgeWearLibraryPath(
                        EdgeWearValidationSuiteReportFileName),
                    report ?? string.Empty,
                    new UTF8Encoding(false));
                diagnostic = string.Empty;
                return true;
            }
            catch (Exception exception)
            {
                diagnostic =
                    "combined report write failed: " +
                    exception.GetType().Name + ":" +
                    exception.Message;
                return false;
            }
        }

        private static string GetEdgeWearLibraryPath(string fileName)
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string libraryPath = Path.Combine(projectRoot, "Library");
            Directory.CreateDirectory(libraryPath);
            return Path.Combine(libraryPath, fileName);
        }

        private static void
            CancelEdgeWearViabilityMatrixForDomainReload()
        {
            EditorApplication.update -= AdvanceEdgeWearViabilityMatrix;
            EditorUtility.ClearProgressBar();
            activeEdgeWearViabilityMatrixJob = null;
            activeEdgeWearValidationSuiteJob = null;
        }

        private static EdgeWearViabilityMatrixAggregate
            BuildEdgeWearViabilityMatrixAggregate(
                EdgeWearViabilityMatrixJob job,
                bool cancelled,
                bool statePreserved,
                string terminalReason)
        {
            EdgeWearViabilityMatrixAggregate aggregate =
                new EdgeWearViabilityMatrixAggregate
                {
                    CasesRun = job.Cases.Count,
                    Cancelled = cancelled,
                    StatePreserved = statePreserved,
                    TerminalReason = terminalReason ?? string.Empty,
                    MinimumCertifiedRatio = job.Cases.Count > 0
                        ? 1f
                        : 0f
                };

            for (int caseIndex = 0;
                 caseIndex < job.Cases.Count;
                 caseIndex++)
            {
                EdgeWearViabilityMatrixCase matrixCase =
                    job.Cases[caseIndex];
                MassGenerator.EdgeWearBatchAuditCaseResult result =
                    matrixCase.Result;
                if (result.Passed)
                {
                    aggregate.CasesPassed++;
                }
                else
                {
                    aggregate.FailureCoordinates.Add(
                        "seed=" + matrixCase.ShapeSeed +
                        "/width=" + matrixCase.WidthName +
                        "/reason=" + result.PrimaryFailure);
                }

                bool coverageFailure =
                    result.CertifiedCount !=
                        result.ExpectedCertificationCount ||
                    result.CoverageValid != 1;
                if (coverageFailure)
                {
                    aggregate.CoexistenceCoverageFailures++;
                }
                bool collateralFailure =
                    result.CollateralPreservationValid != 1 ||
                    result.CollateralLostEdgeCount != 0 ||
                    result.CollateralChangedEdgeCount != 0;
                if (collateralFailure)
                {
                    aggregate.CollateralPreservationFailures++;
                }
                bool widthFloorFailure =
                    result.SelectedCount > 0 &&
                    result.MinimumWidthScale + 0.0001f <
                        EdgeWearBatchMinimumWidthScale;
                if (widthFloorFailure)
                {
                    aggregate.WidthFloorFailures++;
                }

                string failure = result.PrimaryFailure ?? string.Empty;
                bool missingJunction = failure.IndexOf(
                        "missing-junction",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool tJunction = failure.IndexOf(
                        "category=TJunction",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    failure.IndexOf(
                        "t-junctions:",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool strictIntersection = failure.IndexOf(
                        "category=StrictIntersection",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool planeBand = failure.IndexOf(
                        "splits bevel-band edge",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    failure.IndexOf(
                        "plane-band-incompatible",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool candidateConservation = failure.IndexOf(
                        "candidate-conservation",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool faceQuality = failure.IndexOf(
                        "FaceQuality",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    failure.IndexOf(
                        "NonPlanar",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                bool topology = missingJunction || tJunction ||
                    failure.IndexOf(
                        "category=OpenEdge",
                        StringComparison.OrdinalIgnoreCase) >= 0 ||
                    failure.IndexOf(
                        "category=NonManifold",
                        StringComparison.OrdinalIgnoreCase) >= 0;
                if (missingJunction)
                {
                    aggregate.MissingJunctionFailures++;
                }
                if (tJunction)
                {
                    aggregate.TJunctionFailures++;
                }
                if (strictIntersection)
                {
                    aggregate.StrictIntersectionFailures++;
                }
                if (planeBand)
                {
                    aggregate.PlaneBandFailures++;
                }
                if (candidateConservation)
                {
                    aggregate.CandidateConservationFailures++;
                }
                if (topology)
                {
                    aggregate.TopologyFailures++;
                }
                if (faceQuality)
                {
                    aggregate.FaceQualityFailures++;
                }
                bool placementFailure = result.PreviewApplied &&
                    (result.ObjectTransformChanged != 0 ||
                     result.PreviewDerivedPlacementParameters != 0 ||
                     result.PreviewUsesCanonicalFrame != 1 ||
                     !result.PlacementCaptured);
                if (placementFailure)
                {
                    aggregate.PlacementFailures++;
                }
                if (!result.Passed &&
                    !coverageFailure &&
                    !widthFloorFailure &&
                    !topology &&
                    !strictIntersection &&
                    !planeBand &&
                    !candidateConservation &&
                    !faceQuality &&
                    !placementFailure &&
                    !collateralFailure)
                {
                    aggregate.OtherConstructionFailures++;
                }
                if (result.LocalityCacheMissCount != 0 ||
                    result.LocalitySolverRecomputationCount != 0)
                {
                    aggregate.CacheContractFailures++;
                }
                aggregate.MinimumCertifiedRatio = Mathf.Min(
                    aggregate.MinimumCertifiedRatio,
                    result.CertifiedRatio);
                aggregate.MaximumPreflightMilliseconds = Math.Max(
                    aggregate.MaximumPreflightMilliseconds,
                    result.PreflightMilliseconds);
                aggregate.MaximumTotalMilliseconds = Math.Max(
                    aggregate.MaximumTotalMilliseconds,
                    result.TotalMilliseconds);
            }

            aggregate.CasesFailed =
                aggregate.CasesRun - aggregate.CasesPassed;
            aggregate.Status = cancelled
                ? "cancelled"
                : aggregate.CasesRun == job.TotalCaseCount &&
                    aggregate.CasesFailed == 0 &&
                    statePreserved
                        ? "passed"
                        : "failed";
            return aggregate;
        }

        private static void WriteEdgeWearViabilityMatrixReports(
            EdgeWearViabilityMatrixJob job,
            string reportText,
            string reportCsv)
        {
            try
            {
                File.WriteAllText(
                    GetEdgeWearLibraryPath(job.ReportTextFileName),
                    reportText ?? string.Empty,
                    new UTF8Encoding(false));
                File.WriteAllText(
                    GetEdgeWearLibraryPath(job.ReportCsvFileName),
                    reportCsv ?? string.Empty,
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "GeneratedMass edge-wear matrix report write failed: " +
                    exception.GetType().Name + ":" + exception.Message);
            }
        }

        private static string BuildEdgeWearViabilityMatrixText(
            EdgeWearViabilityMatrixJob job,
            EdgeWearViabilityMatrixAggregate aggregate)
        {
            StringBuilder builder = new StringBuilder(16384);
            builder.AppendLine(job.ReportTitle);
            builder.Append("contract=");
            builder.AppendLine(job.Contract);
            builder.Append("candidatePolicy=");
            builder.AppendLine(job.RequireAllGeometricCandidates
                ? "all-geometric"
                : "artistic-preview");
            builder.Append("object=");
            builder.AppendLine(job.TargetName);
            builder.Append("entityId=");
            builder.AppendLine(job.TargetEntityId);
            builder.Append("status=");
            builder.AppendLine(aggregate.Status);
            builder.Append("casesRun=");
            builder.AppendLine(aggregate.CasesRun.ToString());
            builder.Append("casesPassed=");
            builder.AppendLine(aggregate.CasesPassed.ToString());
            builder.Append("casesFailed=");
            builder.AppendLine(aggregate.CasesFailed.ToString());
            builder.Append("coexistenceCoverageFailures=");
            builder.AppendLine(
                aggregate.CoexistenceCoverageFailures.ToString());
            builder.Append("widthFloorFailures=");
            builder.AppendLine(aggregate.WidthFloorFailures.ToString());
            builder.Append("missingJunctionFailures=");
            builder.AppendLine(
                aggregate.MissingJunctionFailures.ToString());
            builder.Append("tJunctionFailures=");
            builder.AppendLine(aggregate.TJunctionFailures.ToString());
            builder.Append("strictIntersectionFailures=");
            builder.AppendLine(
                aggregate.StrictIntersectionFailures.ToString());
            builder.Append("planeBandFailures=");
            builder.AppendLine(aggregate.PlaneBandFailures.ToString());
            builder.Append("terminalCandidateConservationFailures=");
            builder.AppendLine(
                aggregate.CandidateConservationFailures.ToString());
            builder.Append("otherConstructionFailures=");
            builder.AppendLine(
                aggregate.OtherConstructionFailures.ToString());
            builder.Append("topologyFailures=");
            builder.AppendLine(aggregate.TopologyFailures.ToString());
            builder.Append("faceQualityFailures=");
            builder.AppendLine(aggregate.FaceQualityFailures.ToString());
            builder.Append("placementFailures=");
            builder.AppendLine(aggregate.PlacementFailures.ToString());
            builder.Append("cacheContractFailures=");
            builder.AppendLine(
                aggregate.CacheContractFailures.ToString());
            builder.Append("collateralPreservationFailures=");
            builder.AppendLine(
                aggregate.CollateralPreservationFailures.ToString());
            builder.Append("minimumCertifiedRatio=");
            builder.AppendLine(aggregate.MinimumCertifiedRatio.ToString(
                "G9", CultureInfo.InvariantCulture));
            builder.Append("maximumPreflightMilliseconds=");
            builder.AppendLine(
                aggregate.MaximumPreflightMilliseconds.ToString(
                    "G9", CultureInfo.InvariantCulture));
            builder.Append("maximumTotalMilliseconds=");
            builder.AppendLine(aggregate.MaximumTotalMilliseconds.ToString(
                "G9", CultureInfo.InvariantCulture));
            builder.Append("statePreserved=");
            builder.AppendLine(aggregate.StatePreserved ? "1" : "0");
            builder.Append("terminalReason=");
            builder.AppendLine(string.IsNullOrEmpty(
                    aggregate.TerminalReason)
                ? "none"
                : aggregate.TerminalReason);
            builder.Append("failureCoordinates=");
            builder.AppendLine(aggregate.FailureCoordinates.Count == 0
                ? "none"
                : string.Join(";", aggregate.FailureCoordinates));
            builder.AppendLine();
            builder.AppendLine("[Cases]");

            for (int caseIndex = 0;
                 caseIndex < job.Cases.Count;
                 caseIndex++)
            {
                EdgeWearViabilityMatrixCase matrixCase =
                    job.Cases[caseIndex];
                MassGenerator.EdgeWearBatchAuditCaseResult result =
                    matrixCase.Result;
                builder.Append("case=");
                builder.Append(caseIndex + 1);
                builder.Append(",seed=");
                builder.Append(matrixCase.ShapeSeed);
                builder.Append(",widthTier=");
                builder.Append(matrixCase.WidthName);
                builder.Append(",width=");
                builder.Append(matrixCase.Width.ToString(
                    "G9", CultureInfo.InvariantCulture));
                builder.Append(",passed=");
                builder.Append(result.Passed ? '1' : '0');
                builder.Append(",rawSource/source/seamPairs/vertexAliases/graphSeamPairs=");
                builder.Append(result.RawSourceEdgeCount);
                builder.Append('/');
                builder.Append(result.SourceEdgeCount);
                builder.Append('/');
                builder.Append(result.CoincidentBoundarySeamPairCount);
                builder.Append('/');
                builder.Append(
                    result.CoincidentGraphVertexReconciliationCount);
                builder.Append('/');
                builder.Append(
                    result.CoincidentGraphBoundarySeamPairCount);
                builder.Append(",collateral=baseline/current/recovered/lost/changed/valid:");
                builder.Append(result.BaselineGeometricEligibleCount);
                builder.Append('/');
                builder.Append(result.GeometricEligibleCount);
                builder.Append('/');
                builder.Append(result.RecoveredGeometricEdgeCount);
                builder.Append('/');
                builder.Append(result.CollateralLostEdgeCount);
                builder.Append('/');
                builder.Append(result.CollateralChangedEdgeCount);
                builder.Append('/');
                builder.Append(result.CollateralPreservationValid);
                builder.Append(",collateralIds=recovered{");
                builder.Append(result.RecoveredGeometricEdgeIds);
                builder.Append("}/lost{");
                builder.Append(result.CollateralLostEdgeIds);
                builder.Append("}/changed{");
                builder.Append(result.CollateralChangedEdgeIds);
                builder.Append('}');
                builder.Append(",structural/geometric/coexistence=");
                builder.Append(result.StructuralEligibleCount);
                builder.Append('/');
                builder.Append(result.GeometricEligibleCount);
                builder.Append('/');
                builder.Append(result.CoexistenceEligibleCount);
                builder.Append(",artistic/candidates/expected=");
                builder.Append(result.ArtisticEligibleCount);
                builder.Append('/');
                builder.Append(result.CandidateCount);
                builder.Append('/');
                builder.Append(result.ExpectedCertificationCount);
                builder.Append(",artisticFilters=total/short/shallow/base/other:");
                builder.Append(result.ArtisticFilteredCount);
                builder.Append('/');
                builder.Append(result.ArtisticShortFilteredCount);
                builder.Append('/');
                builder.Append(result.ArtisticShallowFilteredCount);
                builder.Append('/');
                builder.Append(result.ArtisticBaseFilteredCount);
                builder.Append('/');
                builder.Append(result.ArtisticOtherFilteredCount);
                builder.Append(",artisticThreshold=");
                builder.Append(result.ArtisticSelectionThreshold.ToString(
                    "G9", CultureInfo.InvariantCulture));
                builder.Append(",artisticScores=all{");
                AppendArtisticScoreRange(builder,
                    result.ArtisticScoreMinimum,
                    result.ArtisticScoreMedian,
                    result.ArtisticScoreMaximum);
                builder.Append("}/selected{");
                AppendArtisticScoreRange(builder,
                    result.ArtisticSelectedScoreMinimum,
                    result.ArtisticSelectedScoreMedian,
                    result.ArtisticSelectedScoreMaximum);
                builder.Append("}/filtered{");
                AppendArtisticScoreRange(builder,
                    result.ArtisticFilteredScoreMinimum,
                    result.ArtisticFilteredScoreMedian,
                    result.ArtisticFilteredScoreMaximum);
                builder.Append('}');
                builder.Append(",artisticBins=length{");
                builder.Append(result.ArtisticLengthBins);
                builder.Append("}|dihedral{");
                builder.Append(result.ArtisticDihedralBins);
                builder.Append("}|orientation{");
                builder.Append(result.ArtisticOrientationBins);
                builder.Append("}|silhouette{");
                builder.Append(result.ArtisticSilhouetteBins);
                builder.Append("}|density{");
                builder.Append(result.ArtisticLocalDensityBins);
                builder.Append("}|crowding{");
                builder.Append(result.ArtisticCrowdingBins);
                builder.Append('}');
                builder.Append(",selected/certified/deferred/rejected=");
                builder.Append(result.SelectedCount);
                builder.Append('/');
                builder.Append(result.CertifiedCount);
                builder.Append('/');
                builder.Append(result.DeferredCount);
                builder.Append('/');
                builder.Append(result.RejectedCount);
                builder.Append(",exclusions=");
                AppendBatchExclusionCounts(builder, result);
                builder.Append(",coexistenceExclusions=");
                builder.Append(result.SourceVertexStarExclusionCount);
                builder.Append('/');
                builder.Append(result.PlanePairExclusionCount);
                builder.Append('/');
                builder.Append(result.PlaneBandExclusionCount);
                builder.Append('/');
                builder.Append(result.GlobalWidthFloorExclusionCount);
                builder.Append('/');
                builder.Append(result.CandidateConservationExclusionCount);
                builder.Append('/');
                builder.Append(result.CornerWidthMissingExclusionCount);
                builder.Append('/');
                builder.Append(result.CornerWidthInactiveExclusionCount);
                builder.Append(",coexistenceTrials/cacheUses=");
                builder.Append(result.CoexistenceTrialCount);
                builder.Append('/');
                builder.Append(result.CoexistenceCacheUseCount);
                builder.Append(",coexistenceSearch=");
                builder.Append(result.CoexistenceSearchStatesEvaluated);
                builder.Append('/');
                builder.Append(result.CoexistenceSearchStatesDeduplicated);
                builder.Append('/');
                builder.Append(result.CoexistenceSearchMaximumDepth);
                builder.Append('/');
                builder.Append(result.CoexistenceSearchFrontierRemaining);
                builder.Append('/');
                builder.Append(result.CoexistenceSearchWinningDepth);
                builder.Append(
                    ",searchStateCandidateConservationFailures=");
                builder.Append(result.CandidateConservationFailureCount);
                builder.Append(",minimumWidthScale=");
                builder.Append(result.MinimumWidthScale.ToString(
                    "G9", CultureInfo.InvariantCulture));
                builder.Append(",solverPasses/reductions=");
                builder.Append(result.SolverPassCount);
                builder.Append('/');
                builder.Append(result.WidthReductionCount);
                builder.Append(",topology=");
                builder.Append(result.OpenEdgeCount);
                builder.Append('/');
                builder.Append(result.NonManifoldEdgeCount);
                builder.Append('/');
                builder.Append(result.TJunctionCount);
                builder.Append('/');
                builder.Append(result.InvalidFaceCount);
                builder.Append('/');
                builder.Append(result.NonPlanarFaceCount);
                builder.Append(",validity=");
                builder.Append(result.GeometryValid);
                builder.Append('/');
                builder.Append(result.CoverageValid);
                builder.Append('/');
                builder.Append(result.SurfaceRenderValid);
                builder.Append('/');
                builder.Append(result.MeshValid);
                builder.Append(",fingerprintPrepared=");
                builder.Append(result.StableFingerprintPrepared);
                builder.Append(",cache=");
                builder.Append(result.LocalityEvaluationCount);
                builder.Append('/');
                builder.Append(result.LocalityConstructionUseCount);
                builder.Append('/');
                builder.Append(result.LocalityCacheMissCount);
                builder.Append('/');
                builder.Append(result.LocalitySolverRecomputationCount);
                builder.Append(",preflightMs=");
                builder.Append(result.PreflightMilliseconds.ToString(
                    "G9", CultureInfo.InvariantCulture));
                builder.Append(",totalMs=");
                builder.Append(result.TotalMilliseconds.ToString(
                    "G9", CultureInfo.InvariantCulture));
                builder.Append(",hashes=");
                builder.Append(result.ExclusionReasonHash);
                builder.Append('/');
                builder.Append(result.SelectedEdgeHash);
                builder.Append('/');
                builder.Append(result.CertifiedEdgeHash);
                builder.Append('/');
                builder.Append(result.GeometryTopologyHash);
                builder.Append('/');
                builder.Append(result.PlacementFrameHash);
                builder.Append('/');
                builder.Append(result.EvaluationHash);
                builder.Append(",primaryFailure=");
                builder.AppendLine(string.IsNullOrEmpty(
                        result.PrimaryFailure)
                    ? "none"
                    : result.PrimaryFailure);
            }

            bool wroteSearchTrace = false;
            for (int caseIndex = 0;
                 caseIndex < job.Cases.Count;
                 caseIndex++)
            {
                MassGenerator.EdgeWearBatchAuditCaseResult result =
                    job.Cases[caseIndex].Result;
                if (string.IsNullOrEmpty(result.CoexistenceSearchTrace))
                {
                    continue;
                }
                if (!wroteSearchTrace)
                {
                    builder.AppendLine();
                    wroteSearchTrace = true;
                }
                builder.Append("[Case ");
                builder.Append(caseIndex + 1);
                builder.AppendLine(" Coexistence Search]");
                builder.AppendLine(result.CoexistenceSearchTrace);
                builder.AppendLine();
            }
            return builder.ToString();
        }

        private static void AppendArtisticScoreRange(
            StringBuilder builder,
            float minimum,
            float median,
            float maximum)
        {
            builder.Append(minimum.ToString(
                "G9", CultureInfo.InvariantCulture));
            builder.Append('/');
            builder.Append(median.ToString(
                "G9", CultureInfo.InvariantCulture));
            builder.Append('/');
            builder.Append(maximum.ToString(
                "G9", CultureInfo.InvariantCulture));
        }

        private static string BuildEdgeWearViabilityMatrixCsv(
            EdgeWearViabilityMatrixJob job)
        {
            StringBuilder builder = new StringBuilder(16384);
            builder.AppendLine(
                "case,seed,widthTier,width,passed,rawSourceEdges,sourceEdges," +
                "coincidentBoundarySeamPairs,graphVertexAliases," +
                "graphBoundarySeamPairs,baselineGeometricEligible," +
                "recoveredGeometricEdges,collateralLostEdges," +
                "collateralChangedEdges,collateralPreservationValid," +
                "recoveredGeometricEdgeIds,collateralLostEdgeIds," +
                "collateralChangedEdgeIds,structuralEligible," +
                "geometricEligible,coexistenceEligible," +
                "coexistenceIneligible,artisticEligible,artisticFiltered," +
                "artisticShortFiltered,artisticShallowFiltered," +
                "artisticBaseFiltered,artisticOtherFiltered," +
                "artisticSelectionThreshold,artisticScoreMinimum," +
                "artisticScoreMedian,artisticScoreMaximum," +
                "artisticSelectedScoreMinimum,artisticSelectedScoreMedian," +
                "artisticSelectedScoreMaximum," +
                "artisticFilteredScoreMinimum,artisticFilteredScoreMedian," +
                "artisticFilteredScoreMaximum,artisticLengthBins," +
                "artisticDihedralBins,artisticOrientationBins," +
                "artisticSilhouetteBins,artisticLocalDensityBins," +
                "artisticCrowdingBins,selected,certified,deferred,rejected," +
                "trialRejected,boundaryExclusions,dihedralExclusions," +
                "footprintExclusions,localityExclusions," +
                "isolatedRailExclusions,supportExclusions," +
                "widthFractionExclusions,endpointSpanExclusions," +
                "otherExclusions,sourceVertexStarExclusions," +
                "planePairExclusions,planeBandExclusions," +
                "globalWidthFloorExclusions," +
                "candidateConservationExclusions," +
                "cornerWidthMissingExclusions," +
                "cornerWidthInactiveExclusions,coexistenceTrials," +
                "coexistenceCacheUses,coexistenceSearchStatesEvaluated," +
                "coexistenceSearchStatesDeduplicated," +
                "coexistenceSearchMaximumDepth," +
                "coexistenceSearchFrontierRemaining," +
                "coexistenceSearchWinningDepth," +
                "searchStateCandidateConservationFailures," +
                "minimumWidthScale," +
                "solverPasses,widthReductions,openEdges,nonManifoldEdges," +
                "tJunctions,invalidFaces,nonPlanarFaces,geometryValid," +
                "coverageValid,surfaceRenderValid,meshValid," +
                "fingerprintPrepared,localityEvaluations," +
                "localityConstructionUses,localityCacheMisses," +
                "localitySolverRecomputations,preflightMilliseconds," +
                "totalMilliseconds,exclusionReasonHash,selectedEdgeHash," +
                "certifiedEdgeHash,geometryTopologyHash," +
                "placementFrameHash,evaluationHash,primaryFailure");
            for (int caseIndex = 0;
                 caseIndex < job.Cases.Count;
                 caseIndex++)
            {
                EdgeWearViabilityMatrixCase matrixCase =
                    job.Cases[caseIndex];
                MassGenerator.EdgeWearBatchAuditCaseResult result =
                    matrixCase.Result;
                AppendCsvValue(builder, (caseIndex + 1).ToString());
                AppendCsvValue(builder, matrixCase.ShapeSeed.ToString());
                AppendCsvValue(builder, matrixCase.WidthName);
                AppendCsvValue(builder, matrixCase.Width.ToString(
                    "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder, result.Passed ? "1" : "0");
                AppendCsvValue(builder, result.RawSourceEdgeCount.ToString());
                AppendCsvValue(builder, result.SourceEdgeCount.ToString());
                AppendCsvValue(builder,
                    result.CoincidentBoundarySeamPairCount.ToString());
                AppendCsvValue(builder,
                    result.CoincidentGraphVertexReconciliationCount.ToString());
                AppendCsvValue(builder,
                    result.CoincidentGraphBoundarySeamPairCount.ToString());
                AppendCsvValue(builder,
                    result.BaselineGeometricEligibleCount.ToString());
                AppendCsvValue(builder,
                    result.RecoveredGeometricEdgeCount.ToString());
                AppendCsvValue(builder,
                    result.CollateralLostEdgeCount.ToString());
                AppendCsvValue(builder,
                    result.CollateralChangedEdgeCount.ToString());
                AppendCsvValue(builder,
                    result.CollateralPreservationValid.ToString());
                AppendCsvValue(builder, result.RecoveredGeometricEdgeIds);
                AppendCsvValue(builder, result.CollateralLostEdgeIds);
                AppendCsvValue(builder, result.CollateralChangedEdgeIds);
                AppendCsvValue(builder,
                    result.StructuralEligibleCount.ToString());
                AppendCsvValue(builder,
                    result.GeometricEligibleCount.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceEligibleCount.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceIneligibleCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticEligibleCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticFilteredCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticShortFilteredCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticShallowFilteredCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticBaseFilteredCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticOtherFilteredCount.ToString());
                AppendCsvValue(builder,
                    result.ArtisticSelectionThreshold.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticScoreMinimum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticScoreMedian.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticScoreMaximum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticSelectedScoreMinimum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticSelectedScoreMedian.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticSelectedScoreMaximum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticFilteredScoreMinimum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticFilteredScoreMedian.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder,
                    result.ArtisticFilteredScoreMaximum.ToString(
                        "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder, result.ArtisticLengthBins);
                AppendCsvValue(builder, result.ArtisticDihedralBins);
                AppendCsvValue(builder, result.ArtisticOrientationBins);
                AppendCsvValue(builder, result.ArtisticSilhouetteBins);
                AppendCsvValue(builder, result.ArtisticLocalDensityBins);
                AppendCsvValue(builder, result.ArtisticCrowdingBins);
                AppendCsvValue(builder, result.SelectedCount.ToString());
                AppendCsvValue(builder, result.CertifiedCount.ToString());
                AppendCsvValue(builder, result.DeferredCount.ToString());
                AppendCsvValue(builder, result.RejectedCount.ToString());
                AppendCsvValue(builder,
                    result.TrialRejectedCount.ToString());
                AppendCsvValue(builder,
                    result.BoundaryExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.DihedralExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.FootprintExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.LocalityExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.IsolatedRailExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.SupportExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.WidthFractionExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.EndpointSpanExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.OtherExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.SourceVertexStarExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.PlanePairExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.PlaneBandExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.GlobalWidthFloorExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.CandidateConservationExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.CornerWidthMissingExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.CornerWidthInactiveExclusionCount.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceTrialCount.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceCacheUseCount.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceSearchStatesEvaluated.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceSearchStatesDeduplicated.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceSearchMaximumDepth.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceSearchFrontierRemaining.ToString());
                AppendCsvValue(builder,
                    result.CoexistenceSearchWinningDepth.ToString());
                AppendCsvValue(builder,
                    result.CandidateConservationFailureCount.ToString());
                AppendCsvValue(builder, result.MinimumWidthScale.ToString(
                    "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder, result.SolverPassCount.ToString());
                AppendCsvValue(builder,
                    result.WidthReductionCount.ToString());
                AppendCsvValue(builder, result.OpenEdgeCount.ToString());
                AppendCsvValue(builder,
                    result.NonManifoldEdgeCount.ToString());
                AppendCsvValue(builder, result.TJunctionCount.ToString());
                AppendCsvValue(builder, result.InvalidFaceCount.ToString());
                AppendCsvValue(builder,
                    result.NonPlanarFaceCount.ToString());
                AppendCsvValue(builder, result.GeometryValid.ToString());
                AppendCsvValue(builder, result.CoverageValid.ToString());
                AppendCsvValue(builder,
                    result.SurfaceRenderValid.ToString());
                AppendCsvValue(builder, result.MeshValid.ToString());
                AppendCsvValue(builder,
                    result.StableFingerprintPrepared.ToString());
                AppendCsvValue(builder,
                    result.LocalityEvaluationCount.ToString());
                AppendCsvValue(builder,
                    result.LocalityConstructionUseCount.ToString());
                AppendCsvValue(builder,
                    result.LocalityCacheMissCount.ToString());
                AppendCsvValue(builder,
                    result.LocalitySolverRecomputationCount.ToString());
                AppendCsvValue(builder, result.PreflightMilliseconds.ToString(
                    "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder, result.TotalMilliseconds.ToString(
                    "G9", CultureInfo.InvariantCulture));
                AppendCsvValue(builder, result.ExclusionReasonHash);
                AppendCsvValue(builder, result.SelectedEdgeHash);
                AppendCsvValue(builder, result.CertifiedEdgeHash);
                AppendCsvValue(builder, result.GeometryTopologyHash);
                AppendCsvValue(builder, result.PlacementFrameHash);
                AppendCsvValue(builder, result.EvaluationHash);
                AppendCsvValue(
                    builder,
                    result.PrimaryFailure,
                    true);
            }
            return builder.ToString();
        }

        private static void AppendBatchExclusionCounts(
            StringBuilder builder,
            MassGenerator.EdgeWearBatchAuditCaseResult result)
        {
            builder.Append(result.BoundaryExclusionCount);
            builder.Append('/');
            builder.Append(result.DihedralExclusionCount);
            builder.Append('/');
            builder.Append(result.FootprintExclusionCount);
            builder.Append('/');
            builder.Append(result.LocalityExclusionCount);
            builder.Append('/');
            builder.Append(result.IsolatedRailExclusionCount);
            builder.Append('/');
            builder.Append(result.SupportExclusionCount);
            builder.Append('/');
            builder.Append(result.WidthFractionExclusionCount);
            builder.Append('/');
            builder.Append(result.EndpointSpanExclusionCount);
            builder.Append('/');
            builder.Append(result.OtherExclusionCount);
        }

        private static void AppendCsvValue(
            StringBuilder builder,
            string value,
            bool endOfLine = false)
        {
            value ??= string.Empty;
            bool quote = value.IndexOfAny(
                new[] { ',', '"', '\r', '\n' }) >= 0;
            if (quote)
            {
                builder.Append('"');
                builder.Append(value.Replace("\"", "\"\""));
                builder.Append('"');
            }
            else
            {
                builder.Append(value);
            }
            if (endOfLine)
            {
                builder.AppendLine();
            }
            else
            {
                builder.Append(',');
            }
        }

        private sealed class EdgeWearValidationSuiteJob
        {
            public readonly GeneratedMass Target;
            public readonly string TargetName;
            public readonly string TargetEntityId;
            public readonly int CurrentShapeSeed;
            public EdgeWearValidationSuiteStage Stage;
            public bool CancelRequested;
            public bool Cancelled;
            public string TerminalReason = string.Empty;
            public bool CurrentPreviewPassed;
            public bool CurrentPreviewTelemetryAvailable;
            public string CurrentPreviewSummary = string.Empty;
            public string CurrentPreviewTelemetry = string.Empty;
            public string CurrentPreviewTelemetryDiagnostic =
                string.Empty;
            public EdgeWearViabilityMatrixAggregate TopologyAggregate;
            public EdgeWearViabilityMatrixAggregate PreviewAggregate;
            public string TopologyReportText = string.Empty;
            public string PreviewReportText = string.Empty;
            public bool ComprehensiveArtisticAvailable;
            public string ComprehensiveArtisticReport = string.Empty;
            public string ComprehensiveArtisticDiagnostic = string.Empty;
            public readonly List<EdgeWearViabilityMatrixCase> TopologyCases =
                new List<EdgeWearViabilityMatrixCase>();
            public readonly List<EdgeWearViabilityMatrixCase> PreviewCases =
                new List<EdgeWearViabilityMatrixCase>();
            public int OutlierRecoveryChecksRun;
            public int OutlierRecoveryChecksPassed;
            public int OutlierCertifiedRecoveries;
            public int OutlierProvenInfeasible;
            public int OutlierUnresolved;
            public int NegativeExclusionChecksRun;
            public int NegativeExclusionChecksPassed;
            public string OutlierRecoveryReport = string.Empty;

            public EdgeWearValidationSuiteJob(GeneratedMass target)
            {
                Target = target;
                TargetName = target.name;
                TargetEntityId = target.GetEntityId().ToString();
                CurrentShapeSeed = target.Recipe != null
                    ? target.Recipe.ShapeSeed
                    : 0;
            }

            public string StageDisplayName
            {
                get
                {
                    return Stage switch
                    {
                        EdgeWearValidationSuiteStage.CurrentPreview =>
                            "current preview",
                        EdgeWearValidationSuiteStage.TopologyViability =>
                            "topology viability matrix",
                        EdgeWearValidationSuiteStage
                            .ArtisticPreviewParity =>
                            "artistic preview parity matrix",
                        _ => "complete"
                    };
                }
            }

            public string TopologyStatus => TopologyAggregate == null
                ? "not-run"
                : TopologyAggregate.Status;

            public string PreviewStatus => PreviewAggregate == null
                ? "not-run"
                : PreviewAggregate.Status;

            public string OutlierRecoveryStatus =>
                OutlierRecoveryChecksRun == 0
                    ? "not-run"
                    : OutlierRecoveryChecksPassed ==
                        OutlierRecoveryChecksRun &&
                        OutlierUnresolved == 0
                        ? "passed"
                        : "failed";

            public string NegativeExclusionStatus =>
                NegativeExclusionChecksRun == 0
                    ? "not-run"
                    : NegativeExclusionChecksPassed ==
                        NegativeExclusionChecksRun
                        ? "passed"
                        : "failed";

            public int TopologyCasesRun => TopologyAggregate == null
                ? 0
                : TopologyAggregate.CasesRun;

            public int TopologyCasesPassed => TopologyAggregate == null
                ? 0
                : TopologyAggregate.CasesPassed;

            public int PreviewCasesRun => PreviewAggregate == null
                ? 0
                : PreviewAggregate.CasesRun;

            public int PreviewCasesPassed => PreviewAggregate == null
                ? 0
                : PreviewAggregate.CasesPassed;

            public int TopologyCollateralFailures =>
                TopologyAggregate == null
                    ? 0
                    : TopologyAggregate
                        .CollateralPreservationFailures;

            public int PreviewCollateralFailures =>
                PreviewAggregate == null
                    ? 0
                    : PreviewAggregate
                        .CollateralPreservationFailures;

            public string Status
            {
                get
                {
                    if (Cancelled)
                    {
                        return "cancelled";
                    }
                    if (!CurrentPreviewPassed ||
                        !CurrentPreviewTelemetryAvailable ||
                        TopologyAggregate == null ||
                        PreviewAggregate == null ||
                        TopologyAggregate.Status != "passed" ||
                        PreviewAggregate.Status != "passed" ||
                        OutlierRecoveryChecksRun == 0 ||
                        OutlierRecoveryChecksPassed !=
                            OutlierRecoveryChecksRun ||
                        OutlierUnresolved != 0 ||
                        NegativeExclusionChecksRun == 0 ||
                        NegativeExclusionChecksPassed !=
                            NegativeExclusionChecksRun ||
                        !ComprehensiveArtisticAvailable ||
                        !string.IsNullOrEmpty(TerminalReason))
                    {
                        return "failed";
                    }
                    return "passed";
                }
            }

            public void PrepareCurrentPreviewCapture()
            {
                string telemetryPath = GetEdgeWearLibraryPath(
                    "GeneratedMassEdgeWearTelemetry.txt");
                try
                {
                    if (File.Exists(telemetryPath))
                    {
                        File.Delete(telemetryPath);
                    }
                }
                catch (Exception exception)
                {
                    CurrentPreviewTelemetryDiagnostic =
                        "could not clear previous telemetry: " +
                        exception.GetType().Name + ":" +
                        exception.Message;
                }
            }

            public void CaptureCurrentPreview()
            {
                if (Target == null)
                {
                    CurrentPreviewPassed = false;
                    CurrentPreviewSummary =
                        "target mass no longer exists";
                    CurrentPreviewTelemetryDiagnostic =
                        CurrentPreviewSummary;
                    return;
                }

                CurrentPreviewPassed =
                    Target.UnifiedEdgeWearPreviewApplied &&
                    !Target.UnifiedEdgeWearPreviewStale;
                CurrentPreviewSummary =
                    "seed=" + CurrentShapeSeed +
                    ",applied=" +
                        (Target.UnifiedEdgeWearPreviewApplied ? "1" : "0") +
                    ",stale=" +
                        (Target.UnifiedEdgeWearPreviewStale ? "1" : "0") +
                    ",candidates=" +
                        Target.UnifiedEdgeWearPreviewCandidateCount +
                    ",active=" +
                        Target.UnifiedEdgeWearPreviewActiveEdgeCount +
                    ",deferred=" +
                        Target.UnifiedEdgeWearPreviewDeferredEdgeCount +
                    ",rejected=" +
                        Target.UnifiedEdgeWearPreviewRejectedEdgeCount +
                    ",diagnostic=" +
                        (string.IsNullOrEmpty(
                            Target.UnifiedEdgeWearPreviewDiagnostic)
                            ? "none"
                            : Target.UnifiedEdgeWearPreviewDiagnostic);

                string telemetryPath = GetEdgeWearLibraryPath(
                    "GeneratedMassEdgeWearTelemetry.txt");
                try
                {
                    if (!File.Exists(telemetryPath))
                    {
                        CurrentPreviewTelemetryAvailable = false;
                        string missingDiagnostic =
                            "Library/GeneratedMassEdgeWearTelemetry.txt " +
                            "was not written";
                        CurrentPreviewTelemetryDiagnostic =
                            string.IsNullOrEmpty(
                                CurrentPreviewTelemetryDiagnostic)
                                ? missingDiagnostic
                                : CurrentPreviewTelemetryDiagnostic + "; " +
                                    missingDiagnostic;
                        return;
                    }
                    CurrentPreviewTelemetry =
                        File.ReadAllText(telemetryPath);
                    CurrentPreviewTelemetryAvailable =
                        !string.IsNullOrEmpty(CurrentPreviewTelemetry);
                    CurrentPreviewTelemetryDiagnostic =
                        CurrentPreviewTelemetryAvailable
                            ? string.Empty
                            : "telemetry file was empty";
                }
                catch (Exception exception)
                {
                    CurrentPreviewTelemetryAvailable = false;
                    CurrentPreviewTelemetryDiagnostic =
                        "telemetry read failed: " +
                        exception.GetType().Name + ":" +
                        exception.Message;
                }
            }

            public void RecordMatrix(
                EdgeWearViabilityMatrixJob job,
                EdgeWearViabilityMatrixAggregate aggregate,
                string reportText)
            {
                if (job.Kind == EdgeWearMatrixKind.TopologyViability)
                {
                    TopologyAggregate = aggregate;
                    TopologyReportText = reportText ?? string.Empty;
                    TopologyCases.Clear();
                    TopologyCases.AddRange(job.Cases);
                }
                else
                {
                    PreviewAggregate = aggregate;
                    PreviewReportText = reportText ?? string.Empty;
                    PreviewCases.Clear();
                    PreviewCases.AddRange(job.Cases);
                    EvaluateOutlierRecoveryContract();
                }
            }

            private void EvaluateOutlierRecoveryContract()
            {
                OutlierRecoveryChecksRun = 0;
                OutlierRecoveryChecksPassed = 0;
                OutlierCertifiedRecoveries = 0;
                OutlierProvenInfeasible = 0;
                OutlierUnresolved = 0;
                NegativeExclusionChecksRun = 0;
                NegativeExclusionChecksPassed = 0;
                StringBuilder builder = new StringBuilder(3072);
                builder.AppendLine(
                    "policy=editor-only canonical source-edge fixtures over topology cases; positive fixtures resolve as certified or complete-current-discrete-schedule infeasible");
                EvaluateOutlierRecoveryExpectation(
                    2223,
                    "maximum",
                    36,
                    builder);
                EvaluateOutlierRecoveryExpectation(
                    2223,
                    "default",
                    13,
                    builder);
                EvaluateOutlierRecoveryExpectation(
                    2223,
                    "maximum",
                    13,
                    builder);
                EvaluateOutlierRecoveryExpectation(
                    8889,
                    "maximum",
                    13,
                    builder);
                EvaluateOutlierRecoveryExpectation(
                    8889,
                    "maximum",
                    23,
                    builder);
                EvaluateNegativeEdgeExclusionExpectation(
                    8889,
                    "maximum",
                    40,
                    builder);
                OutlierRecoveryReport = builder.ToString().TrimEnd();
            }

            private void EvaluateOutlierRecoveryExpectation(
                int shapeSeed,
                string widthName,
                int sourceEdgeIndex,
                StringBuilder builder)
            {
                OutlierRecoveryChecksRun++;
                EdgeWearViabilityMatrixCase? matchingCase = null;
                for (int caseIndex = 0;
                     caseIndex < TopologyCases.Count;
                     caseIndex++)
                {
                    EdgeWearViabilityMatrixCase matrixCase =
                        TopologyCases[caseIndex];
                    if (matrixCase.ShapeSeed == shapeSeed &&
                        string.Equals(
                            matrixCase.WidthName,
                            widthName,
                            StringComparison.Ordinal))
                    {
                        matchingCase = matrixCase;
                        break;
                    }
                }

                MassGenerator.EdgeWearArtisticEdgeAuditRecord record = null;
                if (matchingCase.HasValue)
                {
                    MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records =
                        matchingCase.Value.Result.ArtisticEdges;
                    if (records != null)
                    {
                        for (int recordIndex = 0;
                             recordIndex < records.Length;
                             recordIndex++)
                        {
                            if (records[recordIndex] != null &&
                                records[recordIndex].SourceEdgeIndex ==
                                    sourceEdgeIndex)
                            {
                                record = records[recordIndex];
                                break;
                            }
                        }
                    }
                }

                bool certifiedRecovery = record != null &&
                    record.Active != 0 &&
                    record.CertifiedBuilt != 0 &&
                    record.MaterializedWidth > 0f;
                bool provenInfeasible = record != null &&
                    (string.Equals(
                         record.FinalReason,
                         "corner-recovery-proven-infeasible",
                         StringComparison.Ordinal) ||
                     (!string.IsNullOrEmpty(record.IsolatedDiagnostic) &&
                      (record.IsolatedDiagnostic.Contains(
                           "scheduleResolution:complete-infeasible") ||
                       record.IsolatedDiagnostic.Contains(
                           "scheduleResolution:complete-rail-infeasible"))));
                bool passed = certifiedRecovery || provenInfeasible;
                if (passed)
                {
                    OutlierRecoveryChecksPassed++;
                    if (certifiedRecovery)
                    {
                        OutlierCertifiedRecoveries++;
                    }
                    else
                    {
                        OutlierProvenInfeasible++;
                    }
                }
                else
                {
                    OutlierUnresolved++;
                }

                builder.Append("seed=");
                builder.Append(shapeSeed);
                builder.Append(",width=");
                builder.Append(widthName);
                builder.Append(",edge=");
                builder.Append(sourceEdgeIndex);
                builder.Append(",passed=");
                builder.Append(passed ? '1' : '0');
                builder.Append(",resolution=");
                builder.Append(certifiedRecovery
                    ? "certified-recovery"
                    : provenInfeasible
                        ? "proven-infeasible"
                        : "unresolved");
                builder.Append(",found=");
                builder.Append(record != null ? '1' : '0');
                if (record != null)
                {
                    builder.Append(",geometric=");
                    builder.Append(record.GeometricEligible);
                    builder.Append(",coexistence=");
                    builder.Append(record.CoexistenceEligible);
                    builder.Append(",candidate=");
                    builder.Append(record.Candidate);
                    builder.Append(",selected=");
                    builder.Append(record.Selected);
                    builder.Append(",active=");
                    builder.Append(record.Active);
                    builder.Append(",certified=");
                    builder.Append(record.CertifiedBuilt);
                    builder.Append(",materializedWidth=");
                    builder.Append(record.MaterializedWidth.ToString(
                        "G9",
                        CultureInfo.InvariantCulture));
                    builder.Append(",viabilityFailure=");
                    builder.Append(string.IsNullOrEmpty(
                            record.ViabilityFailureReason)
                        ? "none"
                        : record.ViabilityFailureReason);
                    builder.Append(",isolatedDiagnostic=");
                    builder.Append(string.IsNullOrEmpty(
                            record.IsolatedDiagnostic)
                        ? "none"
                        : record.IsolatedDiagnostic);
                    builder.Append(",finalReason=");
                    builder.Append(string.IsNullOrEmpty(record.FinalReason)
                        ? "none"
                        : record.FinalReason);
                }
                builder.AppendLine();
            }

            private void EvaluateNegativeEdgeExclusionExpectation(
                int shapeSeed,
                string widthName,
                int sourceEdgeIndex,
                StringBuilder builder)
            {
                NegativeExclusionChecksRun++;
                MassGenerator.EdgeWearArtisticEdgeAuditRecord record =
                    FindOutlierFixtureRecord(
                        shapeSeed,
                        widthName,
                        sourceEdgeIndex);
                bool passed = record != null &&
                    record.Active == 0 &&
                    record.CertifiedBuilt == 0 &&
                    record.MaterializedWidth <= 0f &&
                    string.Equals(
                        record.FinalReason,
                        "corner-width-inactive",
                        StringComparison.Ordinal);
                if (passed)
                {
                    NegativeExclusionChecksPassed++;
                }

                builder.Append("negative seed=");
                builder.Append(shapeSeed);
                builder.Append(",width=");
                builder.Append(widthName);
                builder.Append(",edge=");
                builder.Append(sourceEdgeIndex);
                builder.Append(",passed=");
                builder.Append(passed ? '1' : '0');
                builder.Append(",found=");
                builder.Append(record != null ? '1' : '0');
                if (record != null)
                {
                    builder.Append(",active=");
                    builder.Append(record.Active);
                    builder.Append(",certified=");
                    builder.Append(record.CertifiedBuilt);
                    builder.Append(",materializedWidth=");
                    builder.Append(record.MaterializedWidth.ToString(
                        "G9",
                        CultureInfo.InvariantCulture));
                    builder.Append(",finalReason=");
                    builder.Append(string.IsNullOrEmpty(record.FinalReason)
                        ? "none"
                        : record.FinalReason);
                }
                builder.AppendLine();
            }

            private MassGenerator.EdgeWearArtisticEdgeAuditRecord
                FindOutlierFixtureRecord(
                    int shapeSeed,
                    string widthName,
                    int sourceEdgeIndex)
            {
                for (int caseIndex = 0;
                     caseIndex < TopologyCases.Count;
                     caseIndex++)
                {
                    EdgeWearViabilityMatrixCase matrixCase =
                        TopologyCases[caseIndex];
                    if (matrixCase.ShapeSeed != shapeSeed ||
                        !string.Equals(
                            matrixCase.WidthName,
                            widthName,
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    MassGenerator.EdgeWearArtisticEdgeAuditRecord[] records =
                        matrixCase.Result.ArtisticEdges;
                    if (records == null)
                    {
                        return null;
                    }
                    for (int recordIndex = 0;
                         recordIndex < records.Length;
                         recordIndex++)
                    {
                        if (records[recordIndex] != null &&
                            records[recordIndex].SourceEdgeIndex ==
                                sourceEdgeIndex)
                        {
                            return records[recordIndex];
                        }
                    }
                    return null;
                }
                return null;
            }

        }

        private sealed class EdgeWearViabilityMatrixJob
        {
            public readonly EdgeWearMatrixKind Kind;
            public readonly GeneratedMass Target;
            public readonly string TargetName;
            public readonly string TargetEntityId;
            public readonly string RecipeJson;
            public readonly float EdgeWearAmount;
            public readonly float EdgeWearSoftness;
            public readonly float CreaseAmount;
            public readonly float CreaseWidth;
            public readonly float CreaseLength;
            public readonly float CreaseBranching;
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;
            public readonly Mesh OriginalMesh;
            public readonly bool SuiteOwned;
            public readonly List<EdgeWearViabilityMatrixCase> Cases =
                new List<EdgeWearViabilityMatrixCase>(
                    EdgeWearBatchShapeSeeds.Length *
                    EdgeWearBatchWidths.Length);
            public int CompletedCaseCount;
            public bool CancelRequested;

            public EdgeWearViabilityMatrixJob(
                GeneratedMass target,
                EdgeWearMatrixKind kind,
                bool suiteOwned)
            {
                Kind = kind;
                SuiteOwned = suiteOwned;
                Target = target;
                TargetName = target.name;
                TargetEntityId = target.GetEntityId().ToString();
                RecipeJson = JsonUtility.ToJson(target.Recipe);
                EdgeWearAmount = target.EdgeWearAmount;
                EdgeWearSoftness = target.EdgeWearSoftness;
                CreaseAmount = target.CreaseAmount;
                CreaseWidth = target.CreaseWidth;
                CreaseLength = target.CreaseLength;
                CreaseBranching = target.CreaseBranching;
                Transform targetTransform = target.transform;
                LocalPosition = targetTransform.localPosition;
                LocalRotation = targetTransform.localRotation;
                LocalScale = targetTransform.localScale;
                MeshFilter meshFilter = target.GetComponent<MeshFilter>();
                OriginalMesh = meshFilter != null
                    ? meshFilter.sharedMesh
                    : null;
            }

            public bool RequireAllGeometricCandidates =>
                Kind == EdgeWearMatrixKind.TopologyViability;

            public string DisplayName => RequireAllGeometricCandidates
                ? "topology viability matrix"
                : "artistic preview parity matrix";

            public string ConsoleName => RequireAllGeometricCandidates
                ? "topology viability matrix"
                : "artistic preview parity matrix";

            public string ProgressTitle => RequireAllGeometricCandidates
                ? "Generated Mass Topology Viability Matrix"
                : "Generated Mass Artistic Preview Parity Matrix";

            public string ReportTextFileName =>
                RequireAllGeometricCandidates
                    ? "GeneratedMassEdgeWearBatchAudit.txt"
                    : "GeneratedMassEdgeWearPreviewParityAudit.txt";

            public string ReportCsvFileName =>
                RequireAllGeometricCandidates
                    ? "GeneratedMassEdgeWearBatchAudit.csv"
                    : "GeneratedMassEdgeWearPreviewParityAudit.csv";

            public string ReportTitle => RequireAllGeometricCandidates
                ? "GeneratedMass edge-wear multi-seed topology " +
                    "viability matrix"
                : "GeneratedMass edge-wear multi-seed artistic " +
                    "preview parity matrix";

            public string Contract => RequireAllGeometricCandidates
                ? "EW-B4.2R13A.7-topology"
                : "EW-B4.2R13A.7-preview";

            public int TotalCaseCount =>
                EdgeWearBatchShapeSeeds.Length *
                EdgeWearBatchWidths.Length;

            public bool ValidateTargetStatePreserved(
                out string diagnostic)
            {
                if (Target == null)
                {
                    diagnostic = "target mass no longer exists";
                    return false;
                }
                Transform targetTransform = Target.transform;
                MeshFilter meshFilter = Target.GetComponent<MeshFilter>();
                bool transformMatches =
                    targetTransform.localPosition == LocalPosition &&
                    targetTransform.localRotation == LocalRotation &&
                    targetTransform.localScale == LocalScale;
                bool meshMatches =
                    (meshFilter == null ? null : meshFilter.sharedMesh) ==
                    OriginalMesh;
                bool recipeMatches = string.Equals(
                    JsonUtility.ToJson(Target.Recipe),
                    RecipeJson,
                    StringComparison.Ordinal);
                if (transformMatches && meshMatches && recipeMatches)
                {
                    diagnostic = string.Empty;
                    return true;
                }
                diagnostic =
                    "selected mass state changed during the batch " +
                    "(transform=" + (transformMatches ? "0" : "1") +
                    ",mesh=" + (meshMatches ? "0" : "1") +
                    ",recipe=" + (recipeMatches ? "0" : "1") + ")";
                return false;
            }
        }

        private readonly struct EdgeWearViabilityMatrixCase
        {
            public readonly int ShapeSeed;
            public readonly string WidthName;
            public readonly float Width;
            public readonly MassGenerator.EdgeWearBatchAuditCaseResult
                Result;

            public EdgeWearViabilityMatrixCase(
                int shapeSeed,
                string widthName,
                float width,
                MassGenerator.EdgeWearBatchAuditCaseResult result)
            {
                ShapeSeed = shapeSeed;
                WidthName = widthName ?? string.Empty;
                Width = width;
                Result = result ??
                    new MassGenerator.EdgeWearBatchAuditCaseResult
                    {
                        ShapeSeed = shapeSeed,
                        EdgeWearWidth = width,
                        PrimaryFailure = "batch result was null"
                    };
            }
        }

        private sealed class EdgeWearViabilityMatrixAggregate
        {
            public string Status = string.Empty;
            public int CasesRun;
            public int CasesPassed;
            public int CasesFailed;
            public int CoexistenceCoverageFailures;
            public int WidthFloorFailures;
            public int MissingJunctionFailures;
            public int TJunctionFailures;
            public int StrictIntersectionFailures;
            public int PlaneBandFailures;
            public int CandidateConservationFailures;
            public int OtherConstructionFailures;
            public int TopologyFailures;
            public int FaceQualityFailures;
            public int PlacementFailures;
            public int CacheContractFailures;
            public int CollateralPreservationFailures;
            public float MinimumCertifiedRatio;
            public double MaximumPreflightMilliseconds;
            public double MaximumTotalMilliseconds;
            public bool Cancelled;
            public bool StatePreserved;
            public string TerminalReason = string.Empty;
            public readonly List<string> FailureCoordinates =
                new List<string>();
        }

        private void DrawSourceEdgeIndexDebug()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Source Edge Index Debug",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This editor-only view rebuilds from the current shape and " +
                "edge-wear controls. Edge labels are classified in-place: " +
                "C certified, A artistically filtered, W width-floor failure, " +
                "R isolated-rail failure. The cache invalidates automatically " +
                "when generation inputs change.",
                MessageType.Info);

            if (serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox(
                    "Source-edge indexing is available for one selected mass at a time.",
                    MessageType.None);
                return;
            }

            GeneratedMass mass = target as GeneratedMass;
            if (mass == null)
            {
                return;
            }

            bool nextShow = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Show All Source Edge Numbers in Scene",
                    "Draws every authoritative source-topology edge and index, independently of bevel-preview success."),
                showSourceEdgeIndexDebug);
            if (nextShow != showSourceEdgeIndexDebug)
            {
                showSourceEdgeIndexDebug = nextShow;
                if (showSourceEdgeIndexDebug &&
                    !mass.SourceEdgeIndexDebugIsCurrent)
                {
                    mass.RefreshSourceEdgeIndexDebug();
                }
                SetSourceEdgeIndexOverlayState(
                    mass,
                    showSourceEdgeIndexDebug,
                    highlightSourceEdgeSearchEdges,
                    sourceEdgeIndexDebugXRay);
            }

            if (!showSourceEdgeIndexDebug)
            {
                SetSourceEdgeIndexOverlayState(
                    mass,
                    false,
                    highlightSourceEdgeSearchEdges,
                    sourceEdgeIndexDebugXRay);
                return;
            }

            bool nextHighlight = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "Highlight Active Bevel Search Edges",
                    "Keeps all source edges visible and only changes the colour of edges implicated by the current bevel-search telemetry."),
                highlightSourceEdgeSearchEdges);
            if (nextHighlight != highlightSourceEdgeSearchEdges)
            {
                highlightSourceEdgeSearchEdges = nextHighlight;
            }

            bool nextXRay = EditorGUILayout.ToggleLeft(
                new GUIContent(
                    "X-Ray Hidden Source Edges",
                    "When disabled, source edges are depth-tested against the visible mass. Enable only when inspecting the complete rear-side topology through the mesh."),
                sourceEdgeIndexDebugXRay);
            if (nextXRay != sourceEdgeIndexDebugXRay)
            {
                sourceEdgeIndexDebugXRay = nextXRay;
            }

            if (GUILayout.Button("Refresh Source Edge Graph"))
            {
                mass.RefreshSourceEdgeIndexDebug();
                SceneView.RepaintAll();
            }

            MassGenerator.EdgeWearDebugEdgeRecord[] records =
                mass.SourceEdgeIndexDebugEdges;
            int focusCount =
                CountCurrentSearchFocusEdges(mass);
            EditorGUILayout.LabelField(
                "Source Graph Data",
                "seed " + mass.SourceEdgeIndexDebugShapeSeed +
                    "; " + records.Length + " edges; " + focusCount +
                    " current search highlights");
            if (records.Length == 0)
            {
                string diagnostic = mass.SourceEdgeIndexDebugDiagnostic;
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(diagnostic)
                        ? "No independent source-edge graph is available. Press Refresh Source Edge Graph."
                        : diagnostic,
                    MessageType.Warning);
            }

            SetSourceEdgeIndexOverlayState(
                mass,
                true,
                highlightSourceEdgeSearchEdges,
                sourceEdgeIndexDebugXRay);
        }

        private void DrawFeatureRecipeWorkflow()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Recipe & Feature Stack",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Feature Recipe is an editable starting point for generated " +
                "mass feature controls. Changing the dropdown does not " +
                "overwrite manual edits. Use the buttons below when you " +
                "explicitly want to apply or reset those controls.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                featureRecipe,
                new GUIContent(
                    "Feature Recipe",
                    "Selects a reusable Generated Mass feature-control recipe. This selection is inert until Apply or Reset is pressed."));

            DrawFeatureRecipeStatus();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Selected Recipe"))
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Apply Generated Mass Feature Recipe",
                    mass => mass.ApplySelectedFeatureRecipe());
            }

            if (GUILayout.Button("Reset Controls to Recipe"))
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Reset Generated Mass Feature Controls",
                    mass => mass.ResetFeatureControlsToSelectedRecipe());
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Patch 14B scaffolds recipe-driven feature controls only. " +
                "The recipes currently remap existing rendering/mask/debug " +
                "controls; new feature channels such as pitting, water wear, " +
                "frost stress and sacred plane control are still future work.",
                MessageType.None);
        }

        private void DrawFeatureRecipeStatus()
        {
            if (featureRecipe == null)
            {
                return;
            }

            if (featureRecipe.hasMultipleDifferentValues ||
                serializedObject.isEditingMultipleObjects)
            {
                EditorGUILayout.HelpBox(
                    "Recipe status is unavailable while editing multiple generated masses.",
                    MessageType.None);
                return;
            }

            GeneratedMass mass = target as GeneratedMass;
            if (mass == null)
            {
                return;
            }

            GeneratedMassFeatureRecipe selectedRecipe =
                (GeneratedMassFeatureRecipe)featureRecipe.enumValueIndex;
            bool matchesRecipe =
                mass.CurrentFeatureControlsMatchRecipe(selectedRecipe);
            EditorGUILayout.HelpBox(
                matchesRecipe
                    ? "Recipe Status: current feature controls match the selected recipe."
                    : "Recipe Status: current feature controls are modified/custom relative to the selected recipe.",
                MessageType.None);
        }

        private void DrawGenerationBudget()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Generation Budget",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Budget caps generated support-data cost. Temporary atlas debug " +
                "views decide whether Atlas0 or Atlas1 exists; normal edge wear " +
                "no longer requests runtime atlases.",
                MessageType.Info);

            if (generationBudget != null)
            {
                EditorGUILayout.PropertyField(
                    generationBudget,
                    new GUIContent(
                        "Budget",
                        "Compact/Standard/Detailed/Hero cap generated data cost. Custom enables a manual atlas resolution override."));
            }

            bool isCustom = generationBudget != null &&
                !generationBudget.hasMultipleDifferentValues &&
                generationBudget.enumValueIndex ==
                (int)GeneratedMassGenerationBudget.Custom;
            if (isCustom && customFeatureAtlasResolution != null)
            {
                EditorGUILayout.PropertyField(
                    customFeatureAtlasResolution,
                    new GUIContent(
                        "Custom Atlas Resolution",
                        "Manual atlas resolution for Custom budget. Runtime generation quantizes this to 128, 256 or 512."));
            }

            DrawFeatureAtlasBudgetPreview();
        }

        private void DrawFeatureAtlasBudgetPreview()
        {
            if (surfaceMaskDebug == null ||
                edgeWearAmount == null ||
                edgeWearResponseStrength == null ||
                edgeWearMicroVariation == null ||
                generationBudget == null)
            {
                return;
            }

            if (serializedObject.isEditingMultipleObjects ||
                generationBudget.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "Debug atlas preview is unavailable while editing multiple generated masses or mixed budgets.",
                    MessageType.None);
                return;
            }

            GeneratedMassFeatureAtlasRequest request =
                ResolveInspectorFeatureAtlasRequest();
            int resolution = ResolveInspectorFeatureAtlasResolution(request);
            int atlasCount = 0;
            if ((request & GeneratedMassFeatureAtlasRequest.FeatureAtlas0) != 0)
            {
                atlasCount++;
            }

            if ((request & GeneratedMassFeatureAtlasRequest.FeatureAtlas1) != 0)
            {
                atlasCount++;
            }

            float atlasMemoryMb = resolution > 0
                ? (resolution * resolution * 4f * atlasCount) / (1024f * 1024f)
                : 0f;

            string atlasSummary = atlasCount == 0
                ? "No temporary debug atlas required by current Surface Mask Debug mode."
                : $"Temporary debug atlas request: {FormatAtlasRequest(request)} at {resolution}x{resolution}; estimated GPU pixel data {atlasMemoryMb:0.###} MB.";

            EditorGUILayout.HelpBox(
                atlasSummary +
                " CPU-readable texture copies are discarded after upload by the baker.",
                MessageType.None);
        }

        private GeneratedMassFeatureAtlasRequest ResolveInspectorFeatureAtlasRequest()
        {
            GeneratedMassFeatureAtlasRequest request =
                GeneratedMassFeatureAtlasRequest.None;

            StoneSurfaceMaskDebug debugMode =
                (StoneSurfaceMaskDebug)surfaceMaskDebug.intValue;
            if (GeneratedMass.DebugModeRequiresFeatureAtlas0(debugMode))
            {
                request |= GeneratedMassFeatureAtlasRequest.FeatureAtlas0;
            }

            if (GeneratedMass.DebugModeRequiresFeatureAtlas1(debugMode))
            {
                request |= GeneratedMassFeatureAtlasRequest.FeatureAtlas0 |
                    GeneratedMassFeatureAtlasRequest.FeatureAtlas1;
            }

            return request;
        }

        private int ResolveInspectorFeatureAtlasResolution(
            GeneratedMassFeatureAtlasRequest request)
        {
            if (request == GeneratedMassFeatureAtlasRequest.None)
            {
                return 0;
            }

            GeneratedMassGenerationBudget budget =
                (GeneratedMassGenerationBudget)generationBudget.enumValueIndex;
            switch (budget)
            {
                case GeneratedMassGenerationBudget.Compact:
                    return 128;
                case GeneratedMassGenerationBudget.Hero:
                    return 512;
                case GeneratedMassGenerationBudget.Custom:
                    return QuantizeInspectorAtlasResolution(
                        customFeatureAtlasResolution != null
                            ? customFeatureAtlasResolution.intValue
                            : 256);
                case GeneratedMassGenerationBudget.Detailed:
                case GeneratedMassGenerationBudget.Standard:
                default:
                    return 256;
            }
        }

        private static int QuantizeInspectorAtlasResolution(int requestedResolution)
        {
            if (requestedResolution <= 128)
            {
                return 128;
            }

            if (requestedResolution <= 256)
            {
                return 256;
            }

            return 512;
        }

        private static string FormatAtlasRequest(
            GeneratedMassFeatureAtlasRequest request)
        {
            bool atlas0 =
                (request & GeneratedMassFeatureAtlasRequest.FeatureAtlas0) != 0;
            bool atlas1 =
                (request & GeneratedMassFeatureAtlasRequest.FeatureAtlas1) != 0;

            if (atlas0 && atlas1)
            {
                return "Atlas0 + Atlas1";
            }

            if (atlas0)
            {
                return "Atlas0 only";
            }

            if (atlas1)
            {
                return "Atlas1 only";
            }

            return "None";
        }

        private void DrawCoreShapeRecipe()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Core Shape Recipe",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Core Shape Recipe controls the generated mesh: seeds, size, " +
                "major cuts, faceting, edge character, grounding, lean and " +
                "base surface variation. Patch 14B keeps this existing shape " +
                "recipe separate from the new Feature Recipe scaffold.",
                MessageType.Info);

            if (recipe != null)
            {
                EditorGUILayout.PropertyField(
                    recipe,
                    new GUIContent(
                        "Core Shape Recipe",
                        "Existing generated-mass shape recipe. This controls mesh generation rather than material/feature interpretation."),
                    true);
            }

            if (regenerateOnValidate != null)
            {
                EditorGUILayout.PropertyField(
                    regenerateOnValidate,
                    new GUIContent(
                        "Regenerate On Validate",
                        "When enabled, inspector edits regenerate the generated mesh immediately."));
            }
        }

        private void DrawRenderingAndProfile()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Rendering & Profile",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Base Color is the generated mass's chosen material colour before " +
                "generated mask response, optional tint controls, and " +
                "scene/PBR lighting. Use Lighting Tint Influence below to " +
                "control how strongly scene and local light colour can " +
                "hue-shift the final result.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                stoneSurfaceProfile,
                new GUIContent(
                    "Stone Surface Profile",
                    "Chooses the shared HLSL stone material profile. Renderer Material leaves the current renderer material untouched."));
            EditorGUILayout.PropertyField(
                baseColor,
                new GUIContent(
                    "Base Color",
                    "Per-object starting stone colour before generated mask response, optional tinting, and scene lighting."));
            DrawSurfaceMaskDebugControls();

            DrawActiveProfileSummary();
        }

        private void DrawSurfaceMaskDebugControls()
        {
            if (surfaceMaskDebug == null)
            {
                return;
            }

            EditorGUILayout.LabelField(
                "Surface Debug",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Common debug views stay in the main selector. Legacy atlas " +
                "diagnostics are isolated below because they do not validate " +
                "physical bevel geometry.",
                MessageType.None);

            int currentValue = surfaceMaskDebug.intValue;
            int commonIndex = IndexOfDebugValue(CommonDebugValues, currentValue);
            bool currentIsAdvanced =
                commonIndex < 0 &&
                IndexOfDebugValue(AdvancedDebugValues, currentValue) > 0;
            bool currentIsOtherDebug =
                commonIndex < 0 &&
                !currentIsAdvanced &&
                currentValue != (int)StoneSurfaceMaskDebug.None;

            int[] displayedCommonValues = CommonDebugValues;
            GUIContent[] displayedCommonLabels = CommonDebugLabels;
            if (currentIsAdvanced || currentIsOtherDebug)
            {
                displayedCommonValues = new int[CommonDebugValues.Length + 1];
                displayedCommonLabels = new GUIContent[CommonDebugLabels.Length + 1];
                displayedCommonValues[0] = currentValue;
                displayedCommonLabels[0] = currentIsAdvanced
                    ? new GUIContent("Advanced Diagnostic Active")
                    : new GUIContent("Other Debug Active");
                for (int i = 0; i < CommonDebugValues.Length; i++)
                {
                    displayedCommonValues[i + 1] = CommonDebugValues[i];
                    displayedCommonLabels[i + 1] = CommonDebugLabels[i];
                }

                commonIndex = 0;
            }
            else if (commonIndex < 0)
            {
                commonIndex = 0;
            }

            EditorGUI.showMixedValue = surfaceMaskDebug.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int nextCommonIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Surface Mask Debug",
                    "Temporarily visualizes generated material masks on this object. Leave at None for normal rendering."),
                commonIndex,
                displayedCommonLabels);
            if (EditorGUI.EndChangeCheck())
            {
                surfaceMaskDebug.intValue = displayedCommonValues[nextCommonIndex];
            }

            EditorGUI.showMixedValue = false;

            showAdvancedFeatureDiagnostics = EditorGUILayout.Foldout(
                showAdvancedFeatureDiagnostics,
                "Legacy Atlas Diagnostics",
                true);

            if (!showAdvancedFeatureDiagnostics)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Legacy generated-mass atlas inspection only. These channels " +
                    "are not final convex edge wear and are not proof of physical " +
                    "bevel geometry. Use Convex Edge Wear for UV2.z bevel masks.",
                    MessageType.None);

                int advancedIndex =
                    IndexOfDebugValue(AdvancedDebugValues, surfaceMaskDebug.intValue);
                if (advancedIndex < 0)
                {
                    advancedIndex = 0;
                }

                EditorGUI.showMixedValue = surfaceMaskDebug.hasMultipleDifferentValues;
                EditorGUI.BeginChangeCheck();
                int nextAdvancedIndex = EditorGUILayout.Popup(
                    new GUIContent(
                        "Legacy Atlas Channel",
                        "Inspects legacy temporary FeatureAtlas0/1 channels only; not normal edge-wear geometry."),
                    advancedIndex,
                    AdvancedDebugLabels);
                if (EditorGUI.EndChangeCheck())
                {
                    surfaceMaskDebug.intValue =
                        AdvancedDebugValues[nextAdvancedIndex];
                }

                EditorGUI.showMixedValue = false;
            }
        }

        private static int IndexOfDebugValue(int[] values, int value)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == value)
                {
                    return i;
                }
            }

            return -1;
        }

        private void DrawActiveProfileSummary()
        {
            if (stoneSurfaceProfile == null)
            {
                return;
            }

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Active Profile Summary",
                EditorStyles.miniBoldLabel);

            if (stoneSurfaceProfile.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "Profile summary is unavailable while selected masses use different stone profiles.",
                    MessageType.None);
                return;
            }

            StoneSurfaceProfile profile =
                (StoneSurfaceProfile)stoneSurfaceProfile.enumValueIndex;
            Material material = ResolveActiveProfileMaterial(profile);

            if (material == null)
            {
                EditorGUILayout.HelpBox(
                    "Profile summary unavailable: material reference missing or no renderer material found.",
                    MessageType.None);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Material",
                    material,
                    typeof(Material),
                    false);
                DrawMaterialFloat(material, "Wetness", "_Wetness");
                DrawMaterialFloat(material, "Frost Strength", "_FrostStrength");
                DrawMaterialFloat(
                    material,
                    "Monolithic Flatten",
                    "_MonolithicFlatten");
                DrawMaterialFloat(material, "Mottle Strength", "_StoneMottleStrength");
                DrawMaterialFloat(material, "Mottle Scale", "_StoneMottleScale");
                DrawMaterialFloat(material, "Mottle Softness", "_StoneMottleSoftness");
                DrawMaterialFloat(
                    material,
                    "Mottle Shelter Bias",
                    "_StoneMottleShelterBias");
                DrawMaterialFloat(material, "Smoothness", "_Smoothness");
                DrawMaterialFloat(
                    material,
                    "Specular Strength",
                    "_SpecularStrength");
            }
        }

        private Material ResolveActiveProfileMaterial(StoneSurfaceProfile profile)
        {
            switch (profile)
            {
                case StoneSurfaceProfile.ColdGreyStone:
                    return coldGreyStoneMaterial?.objectReferenceValue as Material;

                case StoneSurfaceProfile.DarkWetRiverStone:
                    return darkWetRiverStoneMaterial?.objectReferenceValue as Material;

                case StoneSurfaceProfile.PaleFrostStone:
                    return paleFrostStoneMaterial?.objectReferenceValue as Material;

                case StoneSurfaceProfile.BlackSacredStone:
                    return blackSacredStoneMaterial?.objectReferenceValue as Material;

                case StoneSurfaceProfile.RendererMaterial:
                default:
                    return ResolveRendererMaterial();
            }
        }

        private Material ResolveRendererMaterial()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                return null;
            }

            GeneratedMass mass = target as GeneratedMass;
            Renderer renderer =
                mass != null
                    ? mass.GetComponent<Renderer>()
                    : null;
            return renderer != null
                ? renderer.sharedMaterial
                : null;
        }

        private static void DrawMaterialFloat(
            Material material,
            string label,
            string propertyName)
        {
            if (!material.HasProperty(propertyName))
            {
                EditorGUILayout.LabelField(label, "Unavailable");
                return;
            }

            EditorGUILayout.FloatField(label, material.GetFloat(propertyName));
        }

        private void DrawFeatureStack()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Feature Stack",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Controls are grouped by generated-mass feature. Each " +
                "feature owns its own shape, strength, tint and debug " +
                "controls where those controls exist. Future feature " +
                "channels should be added here as self-contained foldouts, " +
                "not split into global Shape/Strength/Tint buckets.",
                MessageType.Info);

            DrawExposureFeature();
            DrawBaseContactFeature();
            DrawCreviceShelterFeature();
            DrawDirtDepositFeature();
            DrawEdgeWearFeature();
            DrawCreaseDebugFeature();
        }

        private void DrawExposureFeature()
        {
            showExposureFeature = EditorGUILayout.Foldout(
                showExposureFeature,
                "Exposure",
                true);

            if (!showExposureFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Upward/exposed surface response. This feature currently " +
                    "controls final-render strength and optional hue tinting; " +
                    "its generated mask placement comes from the mass surface " +
                    "data rather than a separate per-object height control.",
                    MessageType.None);

                EditorGUILayout.PropertyField(
                    exposureResponse,
                    new GUIContent(
                        "Strength",
                        "Scales how strongly the Exposure mask affects normal final rendering."));
                EditorGUILayout.PropertyField(
                    exposureTint,
                    new GUIContent(
                        "Tint",
                        "Optional hue tint for exposed surfaces."));
                EditorGUILayout.PropertyField(
                    exposureTintStrength,
                    new GUIContent(
                        "Tint Strength",
                        "How much Exposure Tint affects the final render. Default 0 keeps exposure response hue-neutral."));
            }
        }

        private void DrawBaseContactFeature()
        {
            showBaseContactFeature = EditorGUILayout.Foldout(
                showBaseContactFeature,
                "Base / Contact",
                true);

            if (!showBaseContactFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Lower/contact grounding response. Base Lift changes " +
                    "where the base/contact feature starts; the other " +
                    "controls define how strongly and with what optional tint " +
                    "the feature appears in final rendering.",
                    MessageType.None);

                EditorGUILayout.PropertyField(
                    surfaceMaskBaseLift,
                    new GUIContent(
                        "Base Lift",
                        "Moves the lower/contact mask origin upward for embedded or flat masses."));
                EditorGUILayout.PropertyField(
                    baseResponse,
                    new GUIContent(
                        "Strength",
                        "Scales how strongly the Base/contact mask affects normal final rendering."));
                EditorGUILayout.PropertyField(
                    baseTint,
                    new GUIContent(
                        "Tint",
                        "Optional hue tint for Base/contact grounding."));
                EditorGUILayout.PropertyField(
                    baseTintStrength,
                    new GUIContent(
                        "Tint Strength",
                        "How much Base / Contact Tint affects the final render. Default 0 means neutral grounding only."));
            }
        }

        private void DrawCreviceShelterFeature()
        {
            showCreviceShelterFeature = EditorGUILayout.Foldout(
                showCreviceShelterFeature,
                "Crevice / Shelter",
                true);

            if (!showCreviceShelterFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Sheltered lower-side accumulation. Height, Fade and " +
                    "Irregularity change where the CreviceBase feature " +
                    "exists; Strength and Tint control its final visual " +
                    "interpretation.",
                    MessageType.None);

                EditorGUILayout.PropertyField(
                    creviceReach,
                    new GUIContent(
                        "Height",
                        "Controls how far CreviceBase can crawl upward from lower/contact areas."));
                EditorGUILayout.PropertyField(
                    creviceSmoothness,
                    new GUIContent(
                        "Fade",
                        "Controls how softly CreviceBase fades upward."));
                EditorGUILayout.PropertyField(
                    creviceBreakup,
                    new GUIContent(
                        "Irregularity",
                        "Controls how uneven or broken the crevice crawl field is."));
                EditorGUILayout.PropertyField(
                    creviceResponse,
                    new GUIContent(
                        "Strength",
                        "Scales how strongly the CreviceBase mask affects normal final rendering."));
                EditorGUILayout.PropertyField(
                    creviceTint,
                    new GUIContent(
                        "Tint",
                        "Optional hue tint for sheltered crevice/base areas."));
                EditorGUILayout.PropertyField(
                    creviceTintStrength,
                    new GUIContent(
                        "Tint Strength",
                        "How much Crevice Tint affects the final render. Default 0 keeps the response hue-neutral."));
            }
        }

        private void DrawDirtDepositFeature()
        {
            showDirtDepositFeature = EditorGUILayout.Foldout(
                showDirtDepositFeature,
                "Dirt / Deposit",
                true);

            if (!showDirtDepositFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Dirt, mineral and gathered deposit response. Crawl " +
                    "Height and Coverage change where the DirtDeposit feature " +
                    "exists; Strength and Tint control its final visual " +
                    "interpretation.",
                    MessageType.None);

                EditorGUILayout.PropertyField(
                    dirtCrawlReach,
                    new GUIContent(
                        "Crawl Height",
                        "Controls how far DirtDeposit crawl paths may rise."));
                EditorGUILayout.PropertyField(
                    dirtCoverage,
                    new GUIContent(
                        "Coverage",
                        "Controls how full or sparse DirtDeposit is."));
                EditorGUILayout.PropertyField(
                    dirtDepositResponse,
                    new GUIContent(
                        "Strength",
                        "Scales how strongly the DirtDeposit mask affects normal final rendering."));
                EditorGUILayout.PropertyField(
                    dirtDepositTint,
                    new GUIContent(
                        "Tint",
                        "Optional hue tint for dirt, mineral or deposit accumulation."));
                EditorGUILayout.PropertyField(
                    dirtDepositTintStrength,
                    new GUIContent(
                        "Tint Strength",
                        "How much Dirt Deposit Tint affects the final render. Default 0 keeps the response hue-neutral."));
            }
        }

        private void DrawEdgeWearFeature()
        {
            showEdgeWearFeature = EditorGUILayout.Foldout(
                showEdgeWearFeature,
                "Edge Wear",
                true);

            if (!showEdgeWearFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "EW-4 geometry edge wear is active for plane-cut mass archetypes. " +
                    "These controls drive generated bevel/chamfer faces and their " +
                    "worn-edge material response. FeatureAtlas0/1 remain temporary " +
                    "boundary diagnostics only and are not sampled by normal edge wear.",
                    MessageType.Info);

                EditorGUILayout.LabelField(
                    "Geometry Edge-Wear Inputs",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(
                    edgeWearAmount,
                    new GUIContent(
                        "Amount",
                        "Controls worn-face material mask intensity. A value of zero disables generated edge-wear bevels; physical bevel width is controlled by Width."));
                EditorGUILayout.PropertyField(
                    edgeWearWidth,
                    new GUIContent(
                        "Width",
                        "Authoritative physical bevel depth for both normal edge-wear candidates and the editor plane-cut preview. Values below 0.25 provide an extra thin range without changing the established 0.25-2 mapping."));
                EditorGUILayout.PropertyField(
                    edgeWearCoverage,
                    new GUIContent(
                        "Coverage",
                        "Controls how many eligible convex edges are selected for bevel wear. Max selects all eligible structural candidates unless a cut is rejected for stability."));
                EditorGUILayout.PropertyField(
                    edgeWearSoftness,
                    new GUIContent(
                        "Softness",
                        "Controls visible material softness on marked bevel faces. EW-4A.1 does not let this change physical bevel depth."));

                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField(
                    "Visual Response",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    "Visual response is applied to mesh bevel faces marked in UV2.z. " +
                    "Response Strength must be above zero for bevel faces to visibly brighten/tint.",
                    MessageType.None);
                EditorGUILayout.PropertyField(
                    edgeWearResponseStrength,
                    new GUIContent(
                        "Response Strength",
                        "Master visible intensity for UV2.z-marked generated bevel/chamfer faces."));
                EditorGUILayout.PropertyField(
                    edgeWearBrightnessLift,
                    new GUIContent(
                        "Brightness Lift",
                        "How much visible worn ridges brighten the stone value."));
                EditorGUILayout.PropertyField(
                    edgeWearTint,
                    new GUIContent(
                        "Worn Edge Tint",
                        "Optional hue target for worn convex ridges."));
                EditorGUILayout.PropertyField(
                    edgeWearTintStrength,
                    new GUIContent(
                        "Tint Influence",
                        "How strongly Worn Edge Tint affects the visible response. Zero keeps the response value-only."));
                EditorGUILayout.PropertyField(
                    edgeWearMacroVariation,
                    new GUIContent(
                        "Macro Variation",
                        "Reserved for richer per-edge selection/material variation. First EW-4 pass uses deterministic edge scoring only."));
                EditorGUILayout.PropertyField(
                    edgeWearMicroVariation,
                    new GUIContent(
                        "Micro Variation",
                        "Reserved for future along-edge chipping/segmentation. First EW-4 pass does not segment bevel faces."));
            }
        }

        private void DrawCreaseDebugFeature()
        {
            showCreaseDebugFeature = EditorGUILayout.Foldout(
                showCreaseDebugFeature,
                "Crease / Crack Debug",
                true);

            if (!showCreaseDebugFeature)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "ConcaveCrease uses FeatureAtlas0.G as concave boundary proximity " +
                    "and the shared salience/identity channels for future interpretation. This " +
                    "remains data/debug-only; final crease darkening is deferred until " +
                    "the mask is validated.",
                    MessageType.Info);

                EditorGUILayout.PropertyField(
                    creaseAmount,
                    new GUIContent(
                        "Amount",
                        "How much ConcaveCrease debug/data is generated."));
                EditorGUILayout.PropertyField(
                    creaseWidth,
                    new GUIContent(
                        "Width",
                        "Controls the debug/data width around concave creases."));
                EditorGUILayout.PropertyField(
                    creaseLength,
                    new GUIContent(
                        "Length",
                        "Controls the generated length of crease debug/data strips."));
                EditorGUILayout.PropertyField(
                    creaseBranching,
                    new GUIContent(
                        "Branching",
                        "Controls how often crease debug/data branches are generated."));
                EditorGUILayout.PropertyField(
                    creaseSoftness,
                    new GUIContent(
                        "Softness",
                        "Controls how soft the generated crease debug/data response is."));
            }
        }

        private void DrawRockColourAuthority()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Colour / Lighting Interpretation",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Overall Rock Tint changes the generated mass's own colour identity " +
                "before lighting. Lighting Tint Influence controls how " +
                "strongly scene/local light colour can hue-shift the final " +
                "PBR result. Lights still affect brightness, shadows, " +
                "highlights and form.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                overallRockTint,
                new GUIContent(
                    "Overall Mass Tint",
                    "Optional overall colour identity tint applied before lighting."));
            EditorGUILayout.PropertyField(
                overallRockTintStrength,
                new GUIContent(
                    "Overall Tint Strength",
                    "How strongly Overall Mass Tint affects the final generated mass colour."));
            EditorGUILayout.PropertyField(
                lightingTintInfluence,
                new GUIContent(
                    "Lighting Tint Influence",
                    "Controls how strongly scene/local light colour can hue-shift the final PBR result. Lights still affect brightness, shadows, highlights and form."));

            EditorGUILayout.HelpBox(
                "Lighting Tint Influence: 0 = value-only lighting hue " +
                "influence, 0.35 = default moderate scene-light colour " +
                "influence, 1 = full RGB PBR light colour influence.",
                MessageType.None);
        }

        private void EnsureDefaultStoneMaterials()
        {
            bool changed = false;

            changed |= AssignDefaultMaterialIfMissing(
                coldGreyStoneMaterial,
                ColdGreyStoneMaterialPath);
            changed |= AssignDefaultMaterialIfMissing(
                darkWetRiverStoneMaterial,
                DarkWetRiverStoneMaterialPath);
            changed |= AssignDefaultMaterialIfMissing(
                paleFrostStoneMaterial,
                PaleFrostStoneMaterialPath);
            changed |= AssignDefaultMaterialIfMissing(
                blackSacredStoneMaterial,
                BlackSacredStoneMaterialPath);

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.Update();
            }
        }

        private static bool AssignDefaultMaterialIfMissing(
            SerializedProperty property,
            string assetPath)
        {
            if (property == null ||
                property.hasMultipleDifferentValues ||
                property.objectReferenceValue != null)
            {
                return false;
            }

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(assetPath);

            if (material == null)
            {
                return false;
            }

            property.objectReferenceValue = material;
            return true;
        }

        private void DrawRiverInteraction()
        {
            if (riverInteraction == null || participation == null)
            {
                return;
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "River Interaction",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(participation);

            bool disabled =
                participation.enumValueIndex ==
                (int)GeneratedRiverInteractionParticipation.Disabled;

            if (disabled)
            {
                EditorGUILayout.HelpBox(
                    "This generated object is ignored by automatic static river-obstruction discovery.",
                    MessageType.Info);
                DrawRuntimeDiagnostics();
                return;
            }

            DrawStaticPressureControls();
            DrawObstructionWakeControls();
            DrawImpactRippleCollisionControls();

            EditorGUILayout.HelpBox(
                "Inherit uses the defaults of the river that detects this object. Custom replaces only the selected feature's values; it does not multiply unrelated interaction systems.",
                MessageType.None);

            DrawRuntimeDiagnostics();
        }

        private void DrawStaticPressureControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Pressure",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                staticPressureMode,
                new GUIContent("Mode"));

            if (staticPressureMode == null)
            {
                return;
            }

            GeneratedRiverFeatureMode mode =
                (GeneratedRiverFeatureMode)staticPressureMode.enumValueIndex;

            if (mode == GeneratedRiverFeatureMode.Custom)
            {
                EditorGUILayout.PropertyField(
                    staticPressureStrength,
                    new GUIContent("Strength"));
                EditorGUILayout.PropertyField(
                    staticPressureContactSharpness,
                    new GUIContent("Contact Sharpness"));
                EditorGUILayout.PropertyField(
                    staticPressureWaveResponse,
                    new GUIContent(
                        "Profile Variation",
                        "Controls how strongly supported ridge height is redistributed laterally."));
                EditorGUILayout.PropertyField(
                    staticPressureProfileChangeIntervalMin,
                    new GUIContent(
                        "Minimum Change Interval",
                        "Shortest randomized time in seconds between lateral pressure-profile changes."));
                EditorGUILayout.PropertyField(
                    staticPressureProfileChangeIntervalMax,
                    new GUIContent(
                        "Maximum Change Interval",
                        "Longest randomized time in seconds between lateral pressure-profile changes. Morph duration scales automatically and completes before the next change."));
            }
            else if (mode == GeneratedRiverFeatureMode.Inherit)
            {
                EditorGUILayout.HelpBox(
                    "Uses the detected river's shared Pressure defaults.",
                    MessageType.None);
            }
        }

        private void DrawObstructionWakeControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Wake",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                obstructionWakeMode,
                new GUIContent("Mode"));

            if (obstructionWakeMode == null)
            {
                return;
            }

            GeneratedRiverFeatureMode mode =
                (GeneratedRiverFeatureMode)obstructionWakeMode.enumValueIndex;

            if (mode == GeneratedRiverFeatureMode.Custom)
            {
                EditorGUILayout.PropertyField(
                    obstructionWakeStrength,
                    new GUIContent("Strength"));
                EditorGUILayout.PropertyField(
                    obstructionWakeReach,
                    new GUIContent("Reach"));
                EditorGUILayout.PropertyField(
                    obstructionWakeSpread,
                    new GUIContent("Spread"));
                EditorGUILayout.PropertyField(
                    obstructionWakeVariation,
                    new GUIContent(
                        "Variation",
                        "Amount of spatial lee-profile variation and independent left/right release trajectory variation. Timing uses the detected river's interval settings."));
            }
            else if (mode == GeneratedRiverFeatureMode.Inherit)
            {
                EditorGUILayout.HelpBox(
                    "Uses the detected river's shared Wake defaults.",
                    MessageType.None);
            }
        }


        private void DrawImpactRippleCollisionControls()
        {
            if (impactRippleCollisionMode == null)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Impact Ripples",
                EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                impactRippleCollisionMode,
                new GUIContent(
                    "Collision",
                    "Inherit includes this registered stationary solid in the cached Impact Ripple boundary mask. Disabled lets ripples pass through this object without changing its Pressure or Wake behavior."));
        }

        private void DrawRuntimeDiagnostics()
        {
            if (serializedObject.isEditingMultipleObjects ||
                !Application.isPlaying)
            {
                return;
            }

            GeneratedMass mass = (GeneratedMass)target;
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Runtime River Contact",
                EditorStyles.boldLabel);

            if (!StylizedRiverDisturbanceRuntime.TryGetGeneratedSourceDiagnostics(
                    mass,
                    out GeneratedRiverDisturbanceDiagnostics diagnostics))
            {
                EditorGUILayout.HelpBox(
                    "No active river contact is registered for this generated object.",
                    MessageType.None);
                return;
            }

            bool hasPressureProfileDebug =
                StylizedRiverDisturbanceRuntime.
                    TryGetGeneratedSourcePressureProfileDebugData(
                        mass,
                        out GeneratedRiverPressureProfileDebugData
                            pressureProfileDebug);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Detected River",
                    diagnostics.River,
                    typeof(StylizedRiver),
                    true);
                EditorGUILayout.Toggle("Contact Active", diagnostics.Active);
                EditorGUILayout.FloatField(
                    "Across Width",
                    diagnostics.AcrossWidth);
                EditorGUILayout.FloatField(
                    "Along Length",
                    diagnostics.AlongLength);
                EditorGUILayout.FloatField(
                    "Local River Width",
                    diagnostics.LocalRiverWidth);
                EditorGUILayout.Slider(
                    "Blockage",
                    diagnostics.BlockageRatio,
                    0f,
                    1f);
                EditorGUILayout.FloatField(
                    "Effective Padding",
                    diagnostics.EffectivePadding);
                EditorGUILayout.FloatField(
                    "Representative Support",
                    diagnostics.RepresentativeSupportHeight);
                EditorGUILayout.FloatField(
                    "Wave Allowance",
                    diagnostics.WaveAllowance);
                EditorGUILayout.Toggle(
                    "Pressure Enabled",
                    diagnostics.StaticPressureEnabled);
                if (diagnostics.StaticPressureEnabled)
                {
                    EditorGUILayout.Slider(
                        "Pressure Strength",
                        diagnostics.PressureStrength,
                        0f,
                        1f);
                    EditorGUILayout.FloatField(
                        "Contact Sharpness",
                        diagnostics.ContactSharpness);
                    EditorGUILayout.FloatField(
                        "Profile Variation",
                        diagnostics.ProfileVariation);
                    EditorGUILayout.Vector2Field(
                        "Feasible Pressure Range",
                        new Vector2(
                            diagnostics.PressureMinimumHeight,
                            diagnostics.PressureMaximumHeight));
                    EditorGUILayout.FloatField(
                        "Resolved Pressure Height",
                        diagnostics.EffectiveAmplitude);
                    EditorGUILayout.Toggle(
                        "Support Clamp Reached",
                        diagnostics.HeightClampReached);

                    if (hasPressureProfileDebug)
                    {
                        DrawPressureProfileDiagnostics(
                            pressureProfileDebug);
                    }
                }

                EditorGUILayout.Toggle(
                    "Wake Enabled",
                    diagnostics.ObstructionWakeEnabled);
                if (diagnostics.ObstructionWakeEnabled)
                {
                    EditorGUILayout.FloatField(
                        "Resolved Wake Strength",
                        diagnostics.EffectiveWakeStrength);
                    EditorGUILayout.FloatField(
                        "Wake Reach",
                        diagnostics.ObstructionWakeReach);
                    EditorGUILayout.FloatField(
                        "Wake Spread",
                        diagnostics.ObstructionWakeSpread);
                    EditorGUILayout.FloatField(
                        "Wake Variation",
                        diagnostics.ObstructionWakeVariation);

                }
            }

            if (diagnostics.StaticPressureEnabled &&
                hasPressureProfileDebug)
            {
                EditorGUILayout.Space(3f);
                bool updatedShowPressureProfile =
                    EditorGUILayout.ToggleLeft(
                        "Show Pressure Profile Graph",
                        showPressureProfile);
                if (updatedShowPressureProfile != showPressureProfile)
                {
                    showPressureProfile = updatedShowPressureProfile;
                    SceneView.RepaintAll();
                }

                if (showPressureProfile)
                {
                    EditorGUILayout.HelpBox(
                        "Scene graph: row-by-row height, floor, ceiling, and contact-boundary diagnostics in a fixed screen-space panel.",
                        MessageType.None);
                }
            }

            EditorGUILayout.HelpBox(
                diagnostics.Status,
                diagnostics.Active
                    ? MessageType.Info
                    : MessageType.None);
        }

        private static void DrawPressureProfileDiagnostics(
            GeneratedRiverPressureProfileDebugData debugData)
        {
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Pressure Profile Source",
                EditorStyles.boldLabel);
            EditorGUILayout.FloatField(
                "Requested Width (Field Pixels)",
                debugData.RequestedProfileWidthPixels);
            EditorGUILayout.IntField(
                "Resolved Lateral Rows",
                debugData.LateralSampleCount);
            EditorGUILayout.IntField(
                "Vertical Support Slices",
                debugData.VerticalSupportSlices);
            EditorGUILayout.FloatField(
                "Support Inspection Height",
                debugData.SupportInspectionHeight);
            EditorGUILayout.FloatField(
                "Resolved Pressure Target",
                debugData.TargetHeight);
            EditorGUILayout.FloatField(
                "Support Modulation Reserve",
                debugData.SupportModulationReserve);
            EditorGUILayout.LabelField(
                "Valid Profile Rows",
                $"{debugData.ValidRowCount} / " +
                debugData.LateralSampleCount);
            EditorGUILayout.Vector2Field(
                "Cached Base Height Range",
                debugData.CachedBaseHeightRange);
            EditorGUILayout.Vector2Field(
                "Current Height Range",
                debugData.CurrentHeightRange);
            EditorGUILayout.Vector2Field(
                "Local Ceiling Range",
                debugData.LocalCeilingRange);
            EditorGUILayout.Vector2Field(
                "Interior Base Range",
                debugData.InteriorBaseHeightRange);
            EditorGUILayout.Vector2Field(
                "Interior Ceiling Range",
                debugData.InteriorCeilingRange);
            EditorGUILayout.Vector2Field(
                "Current Multiplier Range",
                debugData.CurrentMultiplierRange);
            EditorGUILayout.IntField(
                "Rows Support-Limited Below Target",
                debugData.SupportLimitedBelowTargetRowCount);
            EditorGUILayout.IntField(
                "Rows Affected by Endpoint Taper",
                debugData.EndpointTaperRowCount);
            EditorGUILayout.IntField(
                "Rows Reaching Target Height",
                debugData.TargetHeightRowCount);
            EditorGUILayout.Vector2Field(
                "Row Thickness Range",
                debugData.RowThicknessRange);
            EditorGUILayout.FloatField(
                "Median Row Thickness",
                debugData.MedianRowThickness);
            EditorGUILayout.FloatField(
                "Protected Rear Starts At (%)",
                debugData.ProtectedDownstreamStartPercent);
            EditorGUILayout.FloatField(
                "Max Resolved Crest Depth (%)",
                debugData.MaximumResolvedCrestDepthPercent);
            EditorGUILayout.FloatField(
                "Max Pressure-End Depth (%)",
                debugData.MaximumResolvedPressureEndDepthPercent);
            EditorGUILayout.IntField(
                "Rows Clamped by Rear Protection",
                debugData.GeometryClampedRowCount);
            EditorGUILayout.IntField(
                "Rows Entering Protected Rear Region",
                debugData.ProtectedDownstreamRegionViolationRowCount);
            EditorGUILayout.FloatField(
                "Max Adjacent Base Height Delta",
                debugData.MaximumAdjacentBaseHeightDifference);
            EditorGUILayout.FloatField(
                "Max Adjacent Current Height Delta",
                debugData.MaximumAdjacentCurrentHeightDifference);
            EditorGUILayout.FloatField(
                "Max Adjacent Base Contact Shift",
                debugData.MaximumAdjacentBaseContactShift);
            EditorGUILayout.FloatField(
                "Max Adjacent Current Contact Shift",
                debugData.MaximumAdjacentCurrentContactShift);
            EditorGUILayout.Vector2Field(
                "Applied Multiplier Bounds",
                debugData.AppliedMultiplierBounds);
        }

        private void DrawRenderMeshDiagnostics()
        {
            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField(
                "Mesh Diagnostics",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Audits the currently generated MeshFilter.sharedMesh " +
                "without regeneration or repair. Proof clones are " +
                "temporary, non-serialized, and replace only suspect " +
                "tangents or the material for causal comparison.",
                MessageType.None);

            GeneratedMass mass = target as GeneratedMass;
            bool editingMultiple =
                serializedObject.isEditingMultipleObjects;
            bool currentAudit = IsCurrentRenderMeshAudit(mass);

            if (renderMeshProofTarget != null &&
                (renderMeshProofSourceMesh == null ||
                 renderMeshProofTarget.GeometryMeshFilter == null ||
                 renderMeshProofTarget.GeometryMeshFilter.sharedMesh !=
                    renderMeshProofSourceMesh))
            {
                DestroyRenderMeshProofClone();
            }

            using (new EditorGUI.DisabledScope(
                Application.isPlaying ||
                editingMultiple ||
                mass == null))
            {
                if (GUILayout.Button("Audit Render Mesh"))
                {
                    RunRenderMeshAudit(mass);
                }
            }

            if (editingMultiple)
            {
                EditorGUILayout.HelpBox(
                    "Render-mesh diagnostics require one selected mass.",
                    MessageType.None);
                return;
            }

            if (lastRenderMeshAudit != null &&
                renderMeshAuditTarget == mass &&
                !currentAudit)
            {
                EditorGUILayout.HelpBox(
                    "The audited mesh has changed. Run Audit Render Mesh " +
                    "again before drawing or creating a proof clone.",
                    MessageType.Warning);
            }

            if (!currentAudit)
            {
                return;
            }

            MessageType auditMessageType =
                lastRenderMeshAudit.HasHardFailure
                    ? MessageType.Error
                    : lastRenderMeshAudit.HasWarning
                        ? MessageType.Warning
                        : MessageType.Info;
            EditorGUILayout.HelpBox(
                lastRenderMeshAudit.Summary,
                auditMessageType);

            EditorGUI.BeginChangeCheck();
            renderMeshAuditDrawWorstTriangle =
                EditorGUILayout.Toggle(
                    "Draw Worst Triangle",
                    renderMeshAuditDrawWorstTriangle);
            renderMeshAuditXRay = EditorGUILayout.Toggle(
                "X-Ray Audit Triangle",
                renderMeshAuditXRay);
            int maximumTriangleOrdinal = Mathf.Max(
                0,
                lastRenderMeshAudit.TriangleCount - 1);
            renderMeshAuditDrawTriangleOrdinal =
                EditorGUILayout.IntSlider(
                    "Triangle To Draw",
                    Mathf.Clamp(
                        renderMeshAuditDrawTriangleOrdinal,
                        0,
                        maximumTriangleOrdinal),
                    0,
                    maximumTriangleOrdinal);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Use Worst Overall"))
            {
                renderMeshAuditDrawTriangleOrdinal =
                    lastRenderMeshAudit.WorstTriangleOrdinal;
            }
            if (GUILayout.Button("Use Worst UV"))
            {
                renderMeshAuditDrawTriangleOrdinal =
                    lastRenderMeshAudit.WorstUvTriangles.Count > 0
                        ? lastRenderMeshAudit.WorstUvTriangles[0].
                            TriangleOrdinal
                        : lastRenderMeshAudit.WorstTriangleOrdinal;
            }
            if (GUILayout.Button("Use Worst Tangent"))
            {
                renderMeshAuditDrawTriangleOrdinal =
                    lastRenderMeshAudit.WorstTangentTriangles.Count > 0
                        ? lastRenderMeshAudit.WorstTangentTriangles[0].
                            TriangleOrdinal
                        : lastRenderMeshAudit.WorstTriangleOrdinal;
            }
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            string reportPath = GetEdgeWearLibraryPath(
                RenderMeshAuditReportFileName);
            using (new EditorGUI.DisabledScope(
                !File.Exists(reportPath)))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Render Audit"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        File.ReadAllText(reportPath);
                }
                if (GUILayout.Button("Reveal Render Audit"))
                {
                    EditorUtility.RevealInFinder(reportPath);
                }
                EditorGUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledScope(
                Application.isPlaying || mass == null))
            {
                if (GUILayout.Button(
                        "Create Normal/Tangent Repair Proof Clone"))
                {
                    CreateRenderMeshProofClone(
                        mass,
                        RenderMeshProofMode.NormalTangentRepair);
                }
                if (GUILayout.Button("Create Unlit Proof Clone"))
                {
                    CreateRenderMeshProofClone(
                        mass,
                        RenderMeshProofMode.Unlit);
                }
            }

            if (renderMeshProofTarget == mass &&
                renderMeshProofObject != null)
            {
                EditorGUILayout.HelpBox(
                    "Temporary proof active: " +
                    ResolveRenderMeshProofDisplayName(
                        renderMeshProofMode) +
                    ". The source renderer is temporarily suppressed; " +
                    "remove the proof or deselect the mass to restore it.",
                    MessageType.Info);
                if (GUILayout.Button("Remove Render Proof Clone"))
                {
                    DestroyRenderMeshProofClone();
                }
            }
        }

        private static bool IsCurrentRenderMeshAudit(
            GeneratedMass mass)
        {
            if (mass == null || lastRenderMeshAudit == null ||
                renderMeshAuditTarget != mass ||
                lastRenderMeshAudit.Target != mass)
            {
                return false;
            }

            MeshFilter meshFilter = mass.GeometryMeshFilter;
            return meshFilter != null &&
                meshFilter.sharedMesh != null &&
                meshFilter.sharedMesh == lastRenderMeshAudit.Mesh;
        }

        private static void RunRenderMeshAudit(GeneratedMass mass)
        {
            if (mass == null)
            {
                return;
            }

            DestroyRenderMeshProofClone();
            RenderMeshAuditResult audit = BuildRenderMeshAudit(mass);
            renderMeshAuditTarget = mass;
            lastRenderMeshAudit = audit;
            renderMeshAuditDrawWorstTriangle =
                audit != null && audit.WorstTriangleOrdinal >= 0;
            renderMeshAuditDrawTriangleOrdinal = audit != null
                ? audit.WorstTriangleOrdinal
                : -1;

            string report = audit != null
                ? audit.Report
                : "GeneratedMass render-mesh audit failed before a " +
                  "report could be created.";
            string reportPath = GetEdgeWearLibraryPath(
                RenderMeshAuditReportFileName);
            try
            {
                File.WriteAllText(
                    reportPath,
                    report ?? string.Empty,
                    new UTF8Encoding(false));
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "GeneratedMass render-mesh audit report write " +
                    "failed. object=" + mass.name +
                    ", entityId=" + mass.GetEntityId() +
                    ", exception=" + exception.GetType().Name +
                    ":" + exception.Message,
                    mass);
            }

            if (audit == null)
            {
                Debug.LogWarning(
                    "GeneratedMass render-mesh audit failed. object=" +
                    mass.name + ", entityId=" + mass.GetEntityId(),
                    mass);
            }
            else
            {
                Debug.LogFormat(
                    audit.HasHardFailure
                        ? LogType.Error
                        : audit.HasWarning
                            ? LogType.Warning
                            : LogType.Log,
                    LogOption.NoStacktrace,
                    mass,
                    "{0}",
                    audit.Summary);
            }

            SceneView.RepaintAll();
        }

        private static RenderMeshAuditResult BuildRenderMeshAudit(
            GeneratedMass mass)
        {
            MeshFilter meshFilter = mass.GeometryMeshFilter;
            Mesh mesh = meshFilter != null
                ? meshFilter.sharedMesh
                : null;
            RenderMeshAuditResult result = new RenderMeshAuditResult
            {
                Target = mass,
                Mesh = mesh,
                ObjectName = mass.name,
                EntityId = mass.GetEntityId().ToString(),
                MeshName = mesh != null ? mesh.name : "none"
            };

            if (mesh == null)
            {
                result.ReadFailure = true;
                result.Summary =
                    "Render mesh audit failed: the selected mass has no " +
                    "MeshFilter.sharedMesh.";
                result.Report = BuildRenderMeshAuditReport(result);
                return result;
            }

            try
            {
                List<Vector3> vertices = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector4> tangents = new List<Vector4>();
                List<Vector2> uv0 = new List<Vector2>();
                List<Vector4> uv2 = new List<Vector4>();
                List<Color> colors = new List<Color>();
                mesh.GetVertices(vertices);
                mesh.GetNormals(normals);
                mesh.GetTangents(tangents);
                mesh.GetUVs(0, uv0);
                mesh.GetUVs(2, uv2);
                mesh.GetColors(colors);
                int[] triangles = mesh.triangles;

                result.VertexCount = vertices.Count;
                result.NormalCount = normals.Count;
                result.TangentCount = tangents.Count;
                result.Uv0Count = uv0.Count;
                result.ColorCount = colors.Count;
                result.Uv2Count = uv2.Count;
                result.TriangleCount = triangles.Length / 3;
                result.SubMeshCount = mesh.subMeshCount;
                result.MissingOrPartialNormals =
                    normals.Count == vertices.Count ? 0 : 1;
                result.MissingOrPartialTangents =
                    tangents.Count == vertices.Count ? 0 : 1;
                result.MissingOrPartialUv0 =
                    uv0.Count == vertices.Count ? 0 : 1;
                result.MissingOrPartialColors =
                    colors.Count == vertices.Count ? 0 : 1;
                result.MissingOrPartialUv2 =
                    uv2.Count == 0 || uv2.Count == vertices.Count
                        ? 0
                        : 1;

                Vector3 positionCentre = Vector3.zero;
                int finitePositionCount = 0;
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    Vector3 position = vertices[vertexIndex];
                    if (!IsFinite(position))
                    {
                        result.NonFinitePositions++;
                        continue;
                    }
                    positionCentre += position;
                    finitePositionCount++;
                }
                if (finitePositionCount > 0)
                {
                    positionCentre /= finitePositionCount;
                }

                List<float> positionDistances = new List<float>(
                    finitePositionCount);
                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    Vector3 position = vertices[vertexIndex];
                    if (!IsFinite(position))
                    {
                        continue;
                    }
                    float distance =
                        (position - positionCentre).magnitude;
                    positionDistances.Add(distance);
                    result.MaximumPositionDistance = Mathf.Max(
                        result.MaximumPositionDistance,
                        distance);
                }
                positionDistances.Sort();
                result.MedianPositionDistance = CalculateMedian(
                    positionDistances);
                float positionOutlierDistance = Mathf.Max(
                    0.0001f,
                    result.MedianPositionDistance * 8f);
                for (int distanceIndex = 0;
                     distanceIndex < positionDistances.Count;
                     distanceIndex++)
                {
                    if (positionDistances[distanceIndex] >
                        positionOutlierDistance)
                    {
                        result.PositionOutliers++;
                    }
                }

                for (int vertexIndex = 0;
                     vertexIndex < vertices.Count;
                     vertexIndex++)
                {
                    if (normals.Count == vertices.Count)
                    {
                        Vector3 normal = normals[vertexIndex];
                        if (!IsFinite(normal))
                        {
                            result.NonFiniteNormals++;
                        }
                        else
                        {
                            float magnitude = normal.magnitude;
                            if (magnitude <
                                RenderMeshMinimumVectorMagnitude)
                            {
                                result.ZeroNormals++;
                            }
                            else if (Mathf.Abs(magnitude - 1f) >
                                     RenderMeshUnitVectorTolerance)
                            {
                                result.NonUnitNormals++;
                            }
                        }
                    }

                    if (tangents.Count == vertices.Count)
                    {
                        Vector4 tangent = tangents[vertexIndex];
                        if (!IsFinite(tangent))
                        {
                            result.NonFiniteTangents++;
                        }
                        else
                        {
                            float magnitude = new Vector3(
                                tangent.x,
                                tangent.y,
                                tangent.z).magnitude;
                            result.MaximumTangentMagnitude = Mathf.Max(
                                result.MaximumTangentMagnitude,
                                magnitude);
                            if (magnitude <
                                RenderMeshMinimumVectorMagnitude)
                            {
                                result.ZeroTangents++;
                            }
                            if (magnitude >
                                RenderMeshExtremeTangentMagnitude)
                            {
                                result.ExtremeTangents++;
                            }
                            if (Mathf.Abs(
                                    Mathf.Abs(tangent.w) - 1f) >
                                RenderMeshUnitVectorTolerance)
                            {
                                result.InvalidTangentHandedness++;
                            }
                        }
                    }

                    if (uv0.Count == vertices.Count &&
                        !IsFinite(uv0[vertexIndex]))
                    {
                        result.NonFiniteUv0++;
                    }
                    if (colors.Count == vertices.Count)
                    {
                        Color color = colors[vertexIndex];
                        if (!IsFinite(color))
                        {
                            result.NonFiniteColors++;
                        }
                        else if (color.r < 0f || color.r > 1f ||
                                 color.g < 0f || color.g > 1f ||
                                 color.b < 0f || color.b > 1f ||
                                 color.a < 0f || color.a > 1f)
                        {
                            result.OutOfRangeColors++;
                        }
                    }
                    if (uv2.Count == vertices.Count &&
                        !IsFinite(uv2[vertexIndex]))
                    {
                        result.NonFiniteUv2++;
                    }
                }

                RenderMeshTriangleAudit firstNonFinite = null;
                RenderMeshTriangleAudit firstZeroNormal = null;
                RenderMeshTriangleAudit firstDegenerate = null;
                RenderMeshTriangleAudit firstExtremeTangent = null;
                RenderMeshTriangleAudit minimumUv = null;
                RenderMeshTriangleAudit minimumArea = null;
                RenderMeshTriangleAudit firstWinding = null;
                RenderMeshTriangleAudit worstNormalAgreement = null;

                for (int triangleOffset = 0;
                     triangleOffset + 2 < triangles.Length;
                     triangleOffset += 3)
                {
                    int ordinal = triangleOffset / 3;
                    int indexA = triangles[triangleOffset];
                    int indexB = triangles[triangleOffset + 1];
                    int indexC = triangles[triangleOffset + 2];
                    if (!IsValidVertexIndex(indexA, vertices.Count) ||
                        !IsValidVertexIndex(indexB, vertices.Count) ||
                        !IsValidVertexIndex(indexC, vertices.Count))
                    {
                        result.InvalidTriangleIndices++;
                        continue;
                    }

                    RenderMeshTriangleAudit triangle =
                        BuildRenderMeshTriangleAudit(
                            ordinal,
                            indexA,
                            indexB,
                            indexC,
                            vertices,
                            normals,
                            tangents,
                            uv0,
                            colors,
                            uv2,
                            positionCentre);

                    result.Triangles.Add(triangle);

                    if (triangle.HasNonFiniteVertexChannel)
                    {
                        result.NonFiniteTriangleGeometry++;
                        firstNonFinite ??= triangle;
                    }
                    if (triangle.ZeroNormal)
                    {
                        firstZeroNormal ??= triangle;
                    }
                    if (triangle.Degenerate)
                    {
                        result.DegenerateTriangles++;
                        firstDegenerate ??= triangle;
                    }
                    if (triangle.Sliver)
                    {
                        result.SliverTriangles++;
                    }
                    if (triangle.UvDegenerate)
                    {
                        result.UvDegenerateTriangles++;
                    }
                    else if (triangle.UvIllConditioned)
                    {
                        result.UvIllConditionedTriangles++;
                    }
                    if (triangle.WindingFailure)
                    {
                        result.WindingFailures++;
                        firstWinding ??= triangle;
                    }
                    if (triangle.NormalAgreementFailure)
                    {
                        result.NormalAgreementFailures++;
                    }
                    if (triangle.MaximumTangentMagnitude >
                        RenderMeshExtremeTangentMagnitude)
                    {
                        firstExtremeTangent ??= triangle;
                    }

                    result.MinimumRelativeArea = Mathf.Min(
                        result.MinimumRelativeArea,
                        triangle.RelativeArea);
                    result.MinimumAbsoluteUvDeterminant = Mathf.Min(
                        result.MinimumAbsoluteUvDeterminant,
                        Mathf.Abs(triangle.UvDeterminant));
                    result.MinimumStoredNormalDot = Mathf.Min(
                        result.MinimumStoredNormalDot,
                        triangle.MinimumNormalDot);
                    result.MinimumOutwardDot = Mathf.Min(
                        result.MinimumOutwardDot,
                        triangle.OutwardDot);

                    if (minimumUv == null ||
                        Mathf.Abs(triangle.UvDeterminant) <
                        Mathf.Abs(minimumUv.UvDeterminant))
                    {
                        minimumUv = triangle;
                    }
                    if (minimumArea == null ||
                        triangle.RelativeArea < minimumArea.RelativeArea)
                    {
                        minimumArea = triangle;
                    }
                    if (worstNormalAgreement == null ||
                        triangle.MinimumNormalDot <
                        worstNormalAgreement.MinimumNormalDot)
                    {
                        worstNormalAgreement = triangle;
                    }

                    InsertRankedTriangle(
                        result.WorstUvTriangles,
                        new RenderMeshRankedTriangle(
                            ordinal,
                            Mathf.Abs(triangle.UvDeterminant)),
                        ascending: true);
                    InsertRankedTriangle(
                        result.WorstTangentTriangles,
                        new RenderMeshRankedTriangle(
                            ordinal,
                            triangle.MaximumTangentMagnitude),
                        ascending: false);
                }

                RenderMeshTriangleAudit worst =
                    firstNonFinite ??
                    firstZeroNormal ??
                    firstDegenerate ??
                    firstExtremeTangent ??
                    firstWinding ??
                    (worstNormalAgreement != null &&
                     worstNormalAgreement.NormalAgreementFailure
                        ? worstNormalAgreement
                        : null) ??
                    (minimumUv != null &&
                     minimumUv.UvIllConditioned
                        ? minimumUv
                        : null) ??
                    minimumArea ??
                    minimumUv;
                result.WorstTriangle = worst;
                result.WorstTriangleOrdinal = worst != null
                    ? worst.Ordinal
                    : -1;
                result.WorstReason = ResolveRenderMeshWorstReason(
                    worst,
                    firstNonFinite,
                    firstZeroNormal,
                    firstDegenerate,
                    firstExtremeTangent,
                    minimumUv,
                    firstWinding,
                    worstNormalAgreement);

                if (float.IsPositiveInfinity(
                        result.MinimumRelativeArea))
                {
                    result.MinimumRelativeArea = 0f;
                }
                if (float.IsPositiveInfinity(
                        result.MinimumAbsoluteUvDeterminant))
                {
                    result.MinimumAbsoluteUvDeterminant = 0f;
                }

                string status = result.HasHardFailure
                    ? "failed"
                    : result.HasWarning
                        ? "passed-with-warnings"
                        : "passed";
                result.Summary =
                    "GeneratedMass render-mesh audit " + status +
                    ". object=" + result.ObjectName +
                    ", vertices=" + result.VertexCount +
                    ", triangles=" + result.TriangleCount +
                    ", zeroNormals=" + result.ZeroNormals +
                    ", nonFinite=" +
                    (result.NonFinitePositions +
                     result.NonFiniteNormals +
                     result.NonFiniteTangents +
                     result.NonFiniteUv0 +
                     result.NonFiniteColors +
                     result.NonFiniteUv2) +
                    ", extremeTangents=" +
                    result.ExtremeTangents +
                    ", uvDegenerate=" +
                    result.UvDegenerateTriangles +
                    ", degenerate3D=" +
                    result.DegenerateTriangles +
                    ", worstTriangle=" +
                    result.WorstTriangleOrdinal +
                    ", reason=" + result.WorstReason +
                    ". Report: Library/" +
                    RenderMeshAuditReportFileName;
                result.Report = BuildRenderMeshAuditReport(result);
                return result;
            }
            catch (Exception exception)
            {
                result.ReadFailure = true;
                result.Summary =
                    "Render mesh audit failed while reading the live mesh: " +
                    exception.GetType().Name + ":" +
                    exception.Message;
                result.Report = BuildRenderMeshAuditReport(
                    result,
                    exception);
                return result;
            }
        }

        private static RenderMeshTriangleAudit
            BuildRenderMeshTriangleAudit(
                int ordinal,
                int indexA,
                int indexB,
                int indexC,
                List<Vector3> vertices,
                List<Vector3> normals,
                List<Vector4> tangents,
                List<Vector2> uv0,
                List<Color> colors,
                List<Vector4> uv2,
                Vector3 positionCentre)
        {
            RenderMeshTriangleAudit result =
                new RenderMeshTriangleAudit
                {
                    Ordinal = ordinal,
                    IndexA = indexA,
                    IndexB = indexB,
                    IndexC = indexC,
                    PositionA = vertices[indexA],
                    PositionB = vertices[indexB],
                    PositionC = vertices[indexC],
                    UvA = uv0.Count == vertices.Count
                        ? uv0[indexA]
                        : Vector2.zero,
                    UvB = uv0.Count == vertices.Count
                        ? uv0[indexB]
                        : Vector2.zero,
                    UvC = uv0.Count == vertices.Count
                        ? uv0[indexC]
                        : Vector2.zero,
                    NormalA = normals.Count == vertices.Count
                        ? normals[indexA]
                        : Vector3.zero,
                    NormalB = normals.Count == vertices.Count
                        ? normals[indexB]
                        : Vector3.zero,
                    NormalC = normals.Count == vertices.Count
                        ? normals[indexC]
                        : Vector3.zero,
                    TangentA = tangents.Count == vertices.Count
                        ? tangents[indexA]
                        : Vector4.zero,
                    TangentB = tangents.Count == vertices.Count
                        ? tangents[indexB]
                        : Vector4.zero,
                    TangentC = tangents.Count == vertices.Count
                        ? tangents[indexC]
                        : Vector4.zero,
                    ColorA = colors.Count == vertices.Count
                        ? colors[indexA]
                        : Color.clear,
                    ColorB = colors.Count == vertices.Count
                        ? colors[indexB]
                        : Color.clear,
                    ColorC = colors.Count == vertices.Count
                        ? colors[indexC]
                        : Color.clear,
                    Uv2A = uv2.Count == vertices.Count
                        ? uv2[indexA]
                        : Vector4.zero,
                    Uv2B = uv2.Count == vertices.Count
                        ? uv2[indexB]
                        : Vector4.zero,
                    Uv2C = uv2.Count == vertices.Count
                        ? uv2[indexC]
                        : Vector4.zero
                };

            result.HasNonFiniteVertexChannel =
                !IsFinite(result.PositionA) ||
                !IsFinite(result.PositionB) ||
                !IsFinite(result.PositionC) ||
                (normals.Count == vertices.Count &&
                 (!IsFinite(result.NormalA) ||
                  !IsFinite(result.NormalB) ||
                  !IsFinite(result.NormalC))) ||
                (tangents.Count == vertices.Count &&
                 (!IsFinite(result.TangentA) ||
                  !IsFinite(result.TangentB) ||
                  !IsFinite(result.TangentC))) ||
                (uv0.Count == vertices.Count &&
                 (!IsFinite(result.UvA) ||
                  !IsFinite(result.UvB) ||
                  !IsFinite(result.UvC))) ||
                (colors.Count == vertices.Count &&
                 (!IsFinite(colors[indexA]) ||
                  !IsFinite(colors[indexB]) ||
                  !IsFinite(colors[indexC]))) ||
                (uv2.Count == vertices.Count &&
                 (!IsFinite(uv2[indexA]) ||
                  !IsFinite(uv2[indexB]) ||
                  !IsFinite(uv2[indexC])));
            result.ZeroNormal = normals.Count != vertices.Count ||
                IsZeroRenderMeshVector(result.NormalA) ||
                IsZeroRenderMeshVector(result.NormalB) ||
                IsZeroRenderMeshVector(result.NormalC);

            Vector3 edgeAB = result.PositionB - result.PositionA;
            Vector3 edgeAC = result.PositionC - result.PositionA;
            Vector3 edgeBC = result.PositionC - result.PositionB;
            float lengthAB = edgeAB.magnitude;
            float lengthAC = edgeAC.magnitude;
            float lengthBC = edgeBC.magnitude;
            result.MinimumEdgeLength = Mathf.Min(
                lengthAB,
                Mathf.Min(lengthAC, lengthBC));
            result.MaximumEdgeLength = Mathf.Max(
                lengthAB,
                Mathf.Max(lengthAC, lengthBC));
            Vector3 cross = Vector3.Cross(edgeAB, edgeAC);
            result.DoubleArea = cross.magnitude;
            float maximumEdgeSqr =
                result.MaximumEdgeLength * result.MaximumEdgeLength;
            result.RelativeArea = maximumEdgeSqr > 0f
                ? result.DoubleArea / maximumEdgeSqr
                : 0f;
            result.Degenerate =
                !IsFinite(result.DoubleArea) ||
                result.RelativeArea <=
                    RenderMeshDegenerateRelativeArea;
            result.Sliver = !result.Degenerate &&
                result.RelativeArea <= RenderMeshSliverRelativeArea;
            result.GeometricNormal =
                TryNormalizeRenderMeshVector(
                    cross,
                    out Vector3 geometricNormal)
                    ? geometricNormal
                    : Vector3.zero;

            Vector2 duv1 = result.UvB - result.UvA;
            Vector2 duv2 = result.UvC - result.UvA;
            result.UvDeterminant =
                duv1.x * duv2.y - duv1.y * duv2.x;
            float absoluteUvDeterminant =
                Mathf.Abs(result.UvDeterminant);
            result.UvDegenerate =
                !IsFinite(result.UvDeterminant) ||
                absoluteUvDeterminant <=
                    RenderMeshDegenerateUvDeterminant;
            result.UvIllConditioned = result.UvDegenerate ||
                absoluteUvDeterminant <=
                    RenderMeshIllConditionedUvDeterminant;

            result.MinimumNormalDot = 1f;
            if (normals.Count == vertices.Count &&
                result.GeometricNormal.sqrMagnitude > 0f)
            {
                result.MinimumNormalDot = Mathf.Min(
                    Vector3.Dot(
                        result.GeometricNormal,
                        SafeNormalized(result.NormalA)),
                    Mathf.Min(
                        Vector3.Dot(
                            result.GeometricNormal,
                            SafeNormalized(result.NormalB)),
                        Vector3.Dot(
                            result.GeometricNormal,
                            SafeNormalized(result.NormalC))));
            }
            result.NormalAgreementFailure =
                result.MinimumNormalDot < 0.5f;

            Vector3 triangleCentre =
                (result.PositionA + result.PositionB +
                 result.PositionC) / 3f;
            result.OutwardDot = result.GeometricNormal.sqrMagnitude > 0f
                ? Vector3.Dot(
                    result.GeometricNormal,
                    SafeNormalized(triangleCentre - positionCentre))
                : 0f;
            result.WindingFailure = result.OutwardDot < -0.0001f;

            result.MaximumTangentMagnitude = Mathf.Max(
                ResolveTangentMagnitude(result.TangentA),
                Mathf.Max(
                    ResolveTangentMagnitude(result.TangentB),
                    ResolveTangentMagnitude(result.TangentC)));
            return result;
        }

        private static string BuildRenderMeshAuditReport(
            RenderMeshAuditResult result,
            Exception exception = null)
        {
            StringBuilder builder = new StringBuilder(32768);
            builder.AppendLine("GeneratedMass live render-mesh audit");
            builder.AppendLine("contract=GM-R12B.1E-render-audit-v3");
            builder.Append("status=");
            builder.AppendLine(
                result.HasHardFailure
                    ? "failed"
                    : result.HasWarning
                        ? "passed-with-warnings"
                        : "passed");
            builder.Append("warnings=uvIllConditioned:");
            builder.AppendLine(
                result.UvIllConditionedTriangles.ToString());
            builder.Append("object=");
            builder.AppendLine(result.ObjectName);
            builder.Append("entityId=");
            builder.AppendLine(result.EntityId);
            builder.Append("mesh=");
            builder.AppendLine(result.MeshName);
            builder.Append("edgeWearPreviewApplied=");
            builder.AppendLine(
                result.Target != null &&
                result.Target.UnifiedEdgeWearPreviewApplied
                    ? "1"
                    : "0");
            builder.Append("vertices=");
            builder.AppendLine(result.VertexCount.ToString());
            builder.Append("channelCounts=positions/normals/tangents/uv0/colors/uv2:");
            builder.Append(result.VertexCount);
            builder.Append("/");
            builder.Append(result.NormalCount);
            builder.Append("/");
            builder.Append(result.TangentCount);
            builder.Append("/");
            builder.Append(result.Uv0Count);
            builder.Append("/");
            builder.Append(result.ColorCount);
            builder.Append("/");
            builder.AppendLine(result.Uv2Count.ToString());
            builder.Append("triangles=");
            builder.AppendLine(result.TriangleCount.ToString());
            builder.Append("subMeshes=");
            builder.AppendLine(result.SubMeshCount.ToString());
            if (exception != null)
            {
                builder.Append("exception=");
                builder.Append(exception.GetType().Name);
                builder.Append(":");
                builder.AppendLine(exception.Message);
            }

            builder.AppendLine();
            builder.AppendLine("[Channel Summary]");
            AppendAuditMetric(
                builder,
                "positions",
                result.NonFinitePositions,
                result.PositionOutliers);
            AppendAuditMetric(
                builder,
                "normals",
                result.NonFiniteNormals,
                result.ZeroNormals,
                result.NonUnitNormals,
                result.MissingOrPartialNormals);
            AppendAuditMetric(
                builder,
                "tangents",
                result.NonFiniteTangents,
                result.ZeroTangents,
                result.ExtremeTangents,
                result.InvalidTangentHandedness,
                result.MissingOrPartialTangents);
            AppendAuditMetric(
                builder,
                "uv0",
                result.NonFiniteUv0,
                result.MissingOrPartialUv0);
            AppendAuditMetric(
                builder,
                "colors",
                result.NonFiniteColors,
                result.OutOfRangeColors,
                result.MissingOrPartialColors);
            AppendAuditMetric(
                builder,
                "uv2",
                result.NonFiniteUv2,
                result.MissingOrPartialUv2);
            builder.Append("positionDistance=max/median/outliers:");
            builder.Append(FormatAuditFloat(
                result.MaximumPositionDistance));
            builder.Append("/");
            builder.Append(FormatAuditFloat(
                result.MedianPositionDistance));
            builder.Append("/");
            builder.AppendLine(result.PositionOutliers.ToString());
            builder.Append("tangentMagnitude=max/extremeThreshold:");
            builder.Append(FormatAuditFloat(
                result.MaximumTangentMagnitude));
            builder.Append("/");
            builder.AppendLine(FormatAuditFloat(
                RenderMeshExtremeTangentMagnitude));

            builder.AppendLine();
            builder.AppendLine("[Triangle Summary]");
            builder.Append("invalidIndices=");
            builder.AppendLine(result.InvalidTriangleIndices.ToString());
            builder.Append("nonFiniteGeometry=");
            builder.AppendLine(
                result.NonFiniteTriangleGeometry.ToString());
            builder.Append("degenerate3D=");
            builder.AppendLine(result.DegenerateTriangles.ToString());
            builder.Append("slivers=");
            builder.AppendLine(result.SliverTriangles.ToString());
            builder.Append("uvDegenerate=");
            builder.AppendLine(result.UvDegenerateTriangles.ToString());
            builder.Append("uvIllConditioned=");
            builder.AppendLine(
                result.UvIllConditionedTriangles.ToString());
            builder.Append("windingFailures=");
            builder.AppendLine(result.WindingFailures.ToString());
            builder.Append("normalAgreementFailures=");
            builder.AppendLine(
                result.NormalAgreementFailures.ToString());
            builder.Append("minimumRelativeArea=");
            builder.AppendLine(FormatAuditFloat(
                result.MinimumRelativeArea));
            builder.Append("minimumAbsoluteUvDeterminant=");
            builder.AppendLine(FormatAuditFloat(
                result.MinimumAbsoluteUvDeterminant));
            builder.Append("minimumStoredNormalDot=");
            builder.AppendLine(FormatAuditFloat(
                result.MinimumStoredNormalDot));
            builder.Append("minimumOutwardDot=");
            builder.AppendLine(FormatAuditFloat(
                result.MinimumOutwardDot));
            builder.Append("worstTriangle=");
            builder.Append(result.WorstTriangleOrdinal);
            builder.Append(",reason=");
            builder.AppendLine(result.WorstReason);

            if (result.WorstTriangle != null)
            {
                builder.AppendLine();
                builder.AppendLine("[Worst Triangle]");
                AppendRenderMeshTriangleAudit(
                    builder,
                    result.WorstTriangle);
            }

            builder.AppendLine();
            builder.AppendLine("[Worst UV Determinants]");
            AppendRankedTriangleList(
                builder,
                result.WorstUvTriangles,
                "absUvDet");
            builder.AppendLine();
            builder.AppendLine("[Worst Tangent Magnitudes]");
            AppendRankedTriangleList(
                builder,
                result.WorstTangentTriangles,
                "maxTangentMagnitude");
            return builder.ToString();
        }

        private static void AppendAuditMetric(
            StringBuilder builder,
            string label,
            params int[] values)
        {
            builder.Append(label);
            builder.Append("=");
            for (int valueIndex = 0;
                 valueIndex < values.Length;
                 valueIndex++)
            {
                if (valueIndex > 0)
                {
                    builder.Append("/");
                }
                builder.Append(values[valueIndex]);
            }
            builder.AppendLine();
        }

        private static void AppendRenderMeshTriangleAudit(
            StringBuilder builder,
            RenderMeshTriangleAudit triangle)
        {
            builder.Append("ordinal=");
            builder.AppendLine(triangle.Ordinal.ToString());
            builder.Append("indices=");
            builder.Append(triangle.IndexA);
            builder.Append("/");
            builder.Append(triangle.IndexB);
            builder.Append("/");
            builder.AppendLine(triangle.IndexC.ToString());
            builder.Append("positions=");
            builder.Append(FormatAuditVector3(triangle.PositionA));
            builder.Append("|");
            builder.Append(FormatAuditVector3(triangle.PositionB));
            builder.Append("|");
            builder.AppendLine(FormatAuditVector3(
                triangle.PositionC));
            builder.Append("edgeLength=min/max:");
            builder.Append(FormatAuditFloat(
                triangle.MinimumEdgeLength));
            builder.Append("/");
            builder.AppendLine(FormatAuditFloat(
                triangle.MaximumEdgeLength));
            builder.Append("doubleArea=");
            builder.AppendLine(FormatAuditFloat(
                triangle.DoubleArea));
            builder.Append("relativeArea=");
            builder.AppendLine(FormatAuditFloat(
                triangle.RelativeArea));
            builder.Append("uv0=");
            builder.Append(FormatAuditVector2(triangle.UvA));
            builder.Append("|");
            builder.Append(FormatAuditVector2(triangle.UvB));
            builder.Append("|");
            builder.AppendLine(FormatAuditVector2(triangle.UvC));
            builder.Append("uvDeterminant=");
            builder.AppendLine(FormatAuditFloat(
                triangle.UvDeterminant));
            builder.Append("geometricNormal=");
            builder.AppendLine(FormatAuditVector3(
                triangle.GeometricNormal));
            builder.Append("storedNormals=");
            builder.Append(FormatAuditVector3(triangle.NormalA));
            builder.Append("|");
            builder.Append(FormatAuditVector3(triangle.NormalB));
            builder.Append("|");
            builder.AppendLine(FormatAuditVector3(
                triangle.NormalC));
            builder.Append("minimumStoredNormalDot=");
            builder.AppendLine(FormatAuditFloat(
                triangle.MinimumNormalDot));
            builder.Append("outwardDot=");
            builder.AppendLine(FormatAuditFloat(
                triangle.OutwardDot));
            builder.Append("tangents=");
            builder.Append(FormatAuditVector4(triangle.TangentA));
            builder.Append("|");
            builder.Append(FormatAuditVector4(triangle.TangentB));
            builder.Append("|");
            builder.AppendLine(FormatAuditVector4(
                triangle.TangentC));
            builder.Append("maximumTangentMagnitude=");
            builder.AppendLine(FormatAuditFloat(
                triangle.MaximumTangentMagnitude));
            builder.Append("colors=");
            builder.Append(FormatAuditColor(triangle.ColorA));
            builder.Append("|");
            builder.Append(FormatAuditColor(triangle.ColorB));
            builder.Append("|");
            builder.AppendLine(FormatAuditColor(triangle.ColorC));
            builder.Append("uv2=");
            builder.Append(FormatAuditVector4(triangle.Uv2A));
            builder.Append("|");
            builder.Append(FormatAuditVector4(triangle.Uv2B));
            builder.Append("|");
            builder.AppendLine(FormatAuditVector4(triangle.Uv2C));
            builder.Append("flags=nonFinite/zeroNormal/degenerate/sliver/");
            builder.Append("uvDegenerate/uvIllConditioned/winding/");
            builder.Append("normalAgreement:");
            builder.Append(triangle.HasNonFiniteVertexChannel ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.ZeroNormal ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.Degenerate ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.Sliver ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.UvDegenerate ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.UvIllConditioned ? "1" : "0");
            builder.Append("/");
            builder.Append(triangle.WindingFailure ? "1" : "0");
            builder.Append("/");
            builder.AppendLine(
                triangle.NormalAgreementFailure ? "1" : "0");
        }

        private static void AppendRankedTriangleList(
            StringBuilder builder,
            List<RenderMeshRankedTriangle> triangles,
            string metricLabel)
        {
            if (triangles == null || triangles.Count == 0)
            {
                builder.AppendLine("none");
                return;
            }
            for (int index = 0; index < triangles.Count; index++)
            {
                builder.Append("triangle=");
                builder.Append(triangles[index].TriangleOrdinal);
                builder.Append(",");
                builder.Append(metricLabel);
                builder.Append("=");
                builder.AppendLine(FormatAuditFloat(
                    triangles[index].Metric));
            }
        }

        private static void InsertRankedTriangle(
            List<RenderMeshRankedTriangle> list,
            RenderMeshRankedTriangle item,
            bool ascending)
        {
            int insertionIndex = 0;
            while (insertionIndex < list.Count)
            {
                bool insertBefore = ascending
                    ? item.Metric < list[insertionIndex].Metric
                    : item.Metric > list[insertionIndex].Metric;
                if (insertBefore)
                {
                    break;
                }
                insertionIndex++;
            }
            list.Insert(insertionIndex, item);
            if (list.Count > RenderMeshWorstListCapacity)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        private static string ResolveRenderMeshWorstReason(
            RenderMeshTriangleAudit worst,
            RenderMeshTriangleAudit firstNonFinite,
            RenderMeshTriangleAudit firstZeroNormal,
            RenderMeshTriangleAudit firstDegenerate,
            RenderMeshTriangleAudit firstExtremeTangent,
            RenderMeshTriangleAudit minimumUv,
            RenderMeshTriangleAudit firstWinding,
            RenderMeshTriangleAudit worstNormalAgreement)
        {
            if (worst == null)
            {
                return "none";
            }
            if (worst == firstNonFinite)
            {
                return "non-finite vertex channel";
            }
            if (worst == firstZeroNormal)
            {
                return "zero stored normal";
            }
            if (worst == firstDegenerate)
            {
                return "degenerate 3D triangle";
            }
            if (worst == firstExtremeTangent)
            {
                return "extreme tangent magnitude";
            }
            if (worst == firstWinding)
            {
                return "outward winding failure";
            }
            if (worst == worstNormalAgreement &&
                worst.NormalAgreementFailure)
            {
                return "stored-normal disagreement";
            }
            if (worst == minimumUv && worst.UvDegenerate)
            {
                return "UV-degenerate triangle";
            }
            if (worst == minimumUv && worst.UvIllConditioned)
            {
                return "UV-ill-conditioned triangle";
            }
            return "minimum relative 3D area";
        }

        private static void DrawRenderMeshAuditSceneOverlay(
            GeneratedMass mass,
            RenderMeshAuditResult audit)
        {
            if (mass == null || audit == null ||
                audit.WorstTriangle == null)
            {
                return;
            }

            RenderMeshTriangleAudit triangle =
                ResolveRenderMeshAuditTriangle(
                    audit,
                    renderMeshAuditDrawTriangleOrdinal) ??
                audit.WorstTriangle;
            if (triangle == null)
            {
                return;
            }
            string triangleReason = triangle.Ordinal ==
                audit.WorstTriangleOrdinal
                    ? audit.WorstReason
                    : ResolveRenderMeshTriangleFlags(triangle);
            Transform meshTransform = mass.GeometryMeshFilter.transform;
            Vector3 worldA = meshTransform.TransformPoint(
                triangle.PositionA);
            Vector3 worldB = meshTransform.TransformPoint(
                triangle.PositionB);
            Vector3 worldC = meshTransform.TransformPoint(
                triangle.PositionC);
            Vector3 worldCentre = (worldA + worldB + worldC) / 3f;

            Color previousColor = Handles.color;
            UnityEngine.Rendering.CompareFunction previousZTest =
                Handles.zTest;
            Handles.zTest = renderMeshAuditXRay
                ? UnityEngine.Rendering.CompareFunction.Always
                : UnityEngine.Rendering.CompareFunction.LessEqual;
            Handles.color = new Color(1f, 0.08f, 0.04f, 0.22f);
            Handles.DrawAAConvexPolygon(worldA, worldB, worldC);
            Handles.color = new Color(1f, 0.08f, 0.04f, 1f);
            Handles.DrawAAPolyLine(
                4f,
                worldA,
                worldB,
                worldC,
                worldA);
            float handleSize = HandleUtility.GetHandleSize(
                worldCentre) * 0.05f;
            Handles.SphereHandleCap(
                0,
                worldA,
                Quaternion.identity,
                handleSize,
                EventType.Repaint);
            Handles.SphereHandleCap(
                0,
                worldB,
                Quaternion.identity,
                handleSize,
                EventType.Repaint);
            Handles.SphereHandleCap(
                0,
                worldC,
                Quaternion.identity,
                handleSize,
                EventType.Repaint);
            Handles.Label(
                worldCentre,
                "Render audit triangle " + triangle.Ordinal +
                "\n" + triangleReason +
                "\nindices " + triangle.IndexA + "/" +
                triangle.IndexB + "/" + triangle.IndexC);
            Handles.zTest = previousZTest;
            Handles.color = previousColor;
        }

        private static RenderMeshTriangleAudit
            ResolveRenderMeshAuditTriangle(
                RenderMeshAuditResult audit,
                int ordinal)
        {
            if (audit == null || audit.Triangles == null)
            {
                return null;
            }
            for (int index = 0;
                 index < audit.Triangles.Count;
                 index++)
            {
                if (audit.Triangles[index].Ordinal == ordinal)
                {
                    return audit.Triangles[index];
                }
            }
            return null;
        }

        private static string ResolveRenderMeshTriangleFlags(
            RenderMeshTriangleAudit triangle)
        {
            if (triangle == null)
            {
                return "none";
            }
            if (triangle.HasNonFiniteVertexChannel)
            {
                return "non-finite vertex channel";
            }
            if (triangle.ZeroNormal)
            {
                return "zero stored normal";
            }
            if (triangle.Degenerate)
            {
                return "degenerate 3D triangle";
            }
            if (triangle.MaximumTangentMagnitude >
                RenderMeshExtremeTangentMagnitude)
            {
                return "extreme tangent magnitude";
            }
            if (triangle.WindingFailure)
            {
                return "outward winding failure";
            }
            if (triangle.NormalAgreementFailure)
            {
                return "stored-normal disagreement";
            }
            if (triangle.UvDegenerate)
            {
                return "UV-degenerate triangle";
            }
            if (triangle.UvIllConditioned)
            {
                return "UV-ill-conditioned triangle";
            }
            if (triangle.Sliver)
            {
                return "3D sliver";
            }
            return "audited triangle";
        }

        private static void CreateRenderMeshProofClone(
            GeneratedMass mass,
            RenderMeshProofMode mode)
        {
            if (!IsCurrentRenderMeshAudit(mass) ||
                mass.GeometryMeshFilter == null ||
                mass.GeometryMeshFilter.sharedMesh == null)
            {
                Debug.LogWarning(
                    "Run Audit Render Mesh on the current generated mesh " +
                    "before creating a proof clone.",
                    mass);
                return;
            }

            DestroyRenderMeshProofClone();
            Mesh sourceMesh = mass.GeometryMeshFilter.sharedMesh;
            MeshRenderer sourceRenderer =
                mass.GeometryMeshFilter.GetComponent<MeshRenderer>();
            if (sourceRenderer == null)
            {
                Debug.LogWarning(
                    "GeneratedMass render proof failed: source " +
                    "MeshRenderer is missing.",
                    mass);
                return;
            }

            Mesh proofMesh = UnityEngine.Object.Instantiate(sourceMesh);
            proofMesh.name = sourceMesh.name +
                (mode == RenderMeshProofMode.NormalTangentRepair
                    ? " [Normal Tangent Repair Proof]"
                    : " [Unlit Proof]");
            proofMesh.hideFlags = HideFlags.HideAndDontSave;

            int repairedNormals = 0;
            int repairedTangents = 0;
            if (mode == RenderMeshProofMode.NormalTangentRepair)
            {
                if (!RepairProofMeshNormalsAndTangents(
                        proofMesh,
                        out repairedNormals,
                        out repairedTangents,
                        out string repairDiagnostic))
                {
                    UnityEngine.Object.DestroyImmediate(proofMesh);
                    Debug.LogWarning(
                        "GeneratedMass normal/tangent proof failed: " +
                        repairDiagnostic,
                        mass);
                    return;
                }
            }

            GameObject proofObject = new GameObject(
                mass.name + " [Render Proof]");
            proofObject.hideFlags = HideFlags.HideAndDontSave;
            proofObject.layer = mass.gameObject.layer;
            Transform sourceTransform = mass.transform;
            proofObject.transform.SetParent(
                sourceTransform.parent,
                false);
            proofObject.transform.localPosition =
                sourceTransform.localPosition;
            proofObject.transform.localRotation =
                sourceTransform.localRotation;
            proofObject.transform.localScale =
                sourceTransform.localScale;

            MeshFilter proofFilter =
                proofObject.AddComponent<MeshFilter>();
            MeshRenderer proofRenderer =
                proofObject.AddComponent<MeshRenderer>();
            proofFilter.sharedMesh = proofMesh;
            proofRenderer.shadowCastingMode =
                sourceRenderer.shadowCastingMode;
            proofRenderer.receiveShadows = sourceRenderer.receiveShadows;
            proofRenderer.lightProbeUsage = sourceRenderer.lightProbeUsage;
            proofRenderer.reflectionProbeUsage =
                sourceRenderer.reflectionProbeUsage;
            proofRenderer.renderingLayerMask =
                sourceRenderer.renderingLayerMask;
            proofRenderer.enabled = sourceRenderer.enabled;
            proofRenderer.sortingLayerID =
                sourceRenderer.sortingLayerID;
            proofRenderer.sortingOrder =
                sourceRenderer.sortingOrder;

            Material proofMaterial = null;
            if (mode == RenderMeshProofMode.Unlit)
            {
                Shader unlitShader = Shader.Find(
                    "Universal Render Pipeline/Unlit");
                if (unlitShader == null)
                {
                    unlitShader = Shader.Find("Unlit/Color");
                }
                if (unlitShader == null)
                {
                    UnityEngine.Object.DestroyImmediate(proofObject);
                    UnityEngine.Object.DestroyImmediate(proofMesh);
                    Debug.LogWarning(
                        "GeneratedMass unlit proof failed: no supported " +
                        "Unlit shader was found.",
                        mass);
                    return;
                }
                proofMaterial = new Material(unlitShader)
                {
                    name = "GeneratedMass Unlit Proof Material",
                    hideFlags = HideFlags.HideAndDontSave
                };
                Color proofColor = new Color(
                    0.62f,
                    0.64f,
                    0.66f,
                    1f);
                if (proofMaterial.HasProperty("_BaseColor"))
                {
                    proofMaterial.SetColor("_BaseColor", proofColor);
                }
                if (proofMaterial.HasProperty("_Color"))
                {
                    proofMaterial.SetColor("_Color", proofColor);
                }
                Material[] sourceMaterials =
                    sourceRenderer.sharedMaterials;
                int materialCount = Mathf.Max(
                    1,
                    sourceMaterials.Length);
                Material[] proofMaterials =
                    new Material[materialCount];
                for (int materialIndex = 0;
                     materialIndex < proofMaterials.Length;
                     materialIndex++)
                {
                    proofMaterials[materialIndex] = proofMaterial;
                }
                proofRenderer.sharedMaterials = proofMaterials;
            }
            else
            {
                proofRenderer.sharedMaterials =
                    sourceRenderer.sharedMaterials;
            }

            renderMeshProofObject = proofObject;
            renderMeshProofTarget = mass;
            renderMeshProofSourceMesh = sourceMesh;
            renderMeshProofMesh = proofMesh;
            renderMeshProofMaterial = proofMaterial;
            renderMeshProofSourceRenderer = sourceRenderer;
            renderMeshProofSourceForceRenderingOff =
                sourceRenderer.forceRenderingOff;
            renderMeshProofMode = mode;
            sourceRenderer.forceRenderingOff = true;

            Debug.Log(
                "GeneratedMass render proof created. object=" +
                mass.name + ", entityId=" + mass.GetEntityId() +
                ", mode=" + ResolveRenderMeshProofDisplayName(mode) +
                ", repairedNormals=" + repairedNormals +
                ", repairedTangents=" + repairedTangents +
                ". Remove the proof clone or deselect the mass to " +
                "restore the source renderer.",
                mass);
            SceneView.RepaintAll();
        }

        private static bool RepairProofMeshNormalsAndTangents(
            Mesh mesh,
            out int repairedNormals,
            out int repairedTangents,
            out string diagnostic)
        {
            repairedNormals = 0;
            repairedTangents = 0;
            diagnostic = string.Empty;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();
            List<Vector2> uv0 = new List<Vector2>();
            mesh.GetVertices(vertices);
            mesh.GetNormals(normals);
            mesh.GetTangents(tangents);
            mesh.GetUVs(0, uv0);

            int vertexCount = vertices.Count;
            if (vertexCount < 3)
            {
                diagnostic = "the proof mesh contains fewer than three vertices";
                return false;
            }

            bool normalChannelComplete = normals.Count == vertexCount;
            if (!normalChannelComplete)
            {
                normals.Clear();
                for (int vertexIndex = 0;
                     vertexIndex < vertexCount;
                     vertexIndex++)
                {
                    normals.Add(Vector3.zero);
                }
            }

            bool[] repairedNormalVertices = new bool[vertexCount];
            int[] triangles = mesh.triangles;
            for (int triangleOffset = 0;
                 triangleOffset + 2 < triangles.Length;
                 triangleOffset += 3)
            {
                int indexA = triangles[triangleOffset];
                int indexB = triangles[triangleOffset + 1];
                int indexC = triangles[triangleOffset + 2];
                if (!IsValidVertexIndex(indexA, vertexCount) ||
                    !IsValidVertexIndex(indexB, vertexCount) ||
                    !IsValidVertexIndex(indexC, vertexCount))
                {
                    diagnostic =
                        "triangle " + (triangleOffset / 3) +
                        " contains an invalid vertex index";
                    return false;
                }

                bool repairA = IsZeroRenderMeshVector(normals[indexA]);
                bool repairB = IsZeroRenderMeshVector(normals[indexB]);
                bool repairC = IsZeroRenderMeshVector(normals[indexC]);
                if (!repairA && !repairB && !repairC)
                {
                    continue;
                }

                Vector3 cross = Vector3.Cross(
                    vertices[indexB] - vertices[indexA],
                    vertices[indexC] - vertices[indexA]);
                if (!TryNormalizeRenderMeshVector(
                        cross,
                        out Vector3 geometricNormal))
                {
                    diagnostic =
                        "triangle " + (triangleOffset / 3) +
                        " cannot produce a stable geometric normal";
                    return false;
                }

                if (repairA)
                {
                    normals[indexA] = geometricNormal;
                    repairedNormalVertices[indexA] = true;
                    repairedNormals++;
                }
                if (repairB)
                {
                    normals[indexB] = geometricNormal;
                    repairedNormalVertices[indexB] = true;
                    repairedNormals++;
                }
                if (repairC)
                {
                    normals[indexC] = geometricNormal;
                    repairedNormalVertices[indexC] = true;
                    repairedNormals++;
                }
            }

            for (int vertexIndex = 0;
                 vertexIndex < vertexCount;
                 vertexIndex++)
            {
                if (!TryNormalizeRenderMeshVector(
                        normals[vertexIndex],
                        out Vector3 normalized))
                {
                    diagnostic =
                        "vertex " + vertexIndex +
                        " still has no finite non-zero normal after repair";
                    return false;
                }
                normals[vertexIndex] = normalized;
            }
            mesh.SetNormals(normals);

            bool tangentChannelComplete =
                tangents.Count == vertexCount;
            bool[] uvConditionedVertices =
                BuildUvConditionedTangentVertexMask(
                    mesh,
                    vertexCount,
                    uv0);
            if (!tangentChannelComplete)
            {
                tangents.Clear();
                for (int vertexIndex = 0;
                     vertexIndex < vertexCount;
                     vertexIndex++)
                {
                    tangents.Add(Vector4.zero);
                }
            }

            for (int vertexIndex = 0;
                 vertexIndex < vertexCount;
                 vertexIndex++)
            {
                Vector4 tangent = tangents[vertexIndex];
                float magnitude = ResolveTangentMagnitude(tangent);
                bool repair = !tangentChannelComplete ||
                    !IsFinite(tangent) ||
                    magnitude < RenderMeshMinimumVectorMagnitude ||
                    magnitude > RenderMeshExtremeTangentMagnitude ||
                    uvConditionedVertices[vertexIndex] ||
                    repairedNormalVertices[vertexIndex];
                if (!repair)
                {
                    continue;
                }

                Vector3 stable = BuildStableTangent(
                    normals[vertexIndex]);
                if (!TryNormalizeRenderMeshVector(
                        stable,
                        out stable))
                {
                    diagnostic =
                        "vertex " + vertexIndex +
                        " cannot produce a stable tangent";
                    return false;
                }
                float handedness = IsFinite(tangent.w) &&
                    Mathf.Abs(tangent.w) > 0.5f
                        ? Mathf.Sign(tangent.w)
                        : 1f;
                tangents[vertexIndex] = new Vector4(
                    stable.x,
                    stable.y,
                    stable.z,
                    handedness);
                repairedTangents++;
            }

            mesh.SetTangents(tangents);
            return true;
        }

        private static bool TryNormalizeRenderMeshVector(
            Vector3 value,
            out Vector3 normalized)
        {
            normalized = Vector3.zero;
            if (!IsFinite(value))
            {
                return false;
            }

            double x = value.x;
            double y = value.y;
            double z = value.z;
            double magnitudeSqr = x * x + y * y + z * z;
            if (!(magnitudeSqr > 0.0) ||
                double.IsNaN(magnitudeSqr) ||
                double.IsInfinity(magnitudeSqr))
            {
                return false;
            }

            double inverseMagnitude = 1.0 / Math.Sqrt(magnitudeSqr);
            normalized = new Vector3(
                (float)(x * inverseMagnitude),
                (float)(y * inverseMagnitude),
                (float)(z * inverseMagnitude));
            float normalizedMagnitudeSqr = normalized.sqrMagnitude;
            return IsFinite(normalized) &&
                IsFinite(normalizedMagnitudeSqr) &&
                normalizedMagnitudeSqr > 0.99f &&
                normalizedMagnitudeSqr < 1.01f;
        }

        private static bool[] BuildUvConditionedTangentVertexMask(
            Mesh mesh,
            int vertexCount,
            List<Vector2> uv0)
        {
            bool[] mask = new bool[vertexCount];
            if (mesh == null || uv0 == null ||
                uv0.Count != vertexCount)
            {
                return mask;
            }

            int[] triangles = mesh.triangles;
            for (int triangleOffset = 0;
                 triangleOffset + 2 < triangles.Length;
                 triangleOffset += 3)
            {
                int indexA = triangles[triangleOffset];
                int indexB = triangles[triangleOffset + 1];
                int indexC = triangles[triangleOffset + 2];
                if (!IsValidVertexIndex(indexA, vertexCount) ||
                    !IsValidVertexIndex(indexB, vertexCount) ||
                    !IsValidVertexIndex(indexC, vertexCount))
                {
                    continue;
                }
                Vector2 duv1 = uv0[indexB] - uv0[indexA];
                Vector2 duv2 = uv0[indexC] - uv0[indexA];
                float determinant =
                    duv1.x * duv2.y - duv1.y * duv2.x;
                if (IsFinite(determinant) &&
                    Mathf.Abs(determinant) >
                        RenderMeshIllConditionedUvDeterminant)
                {
                    continue;
                }
                mask[indexA] = true;
                mask[indexB] = true;
                mask[indexC] = true;
            }
            return mask;
        }

        private static Vector3 BuildStableTangent(Vector3 normal)
        {
            Vector3 reference = Mathf.Abs(normal.y) < 0.9f
                ? Vector3.up
                : Vector3.right;
            Vector3 tangent = Vector3.Cross(reference, normal);
            if (!TryNormalizeRenderMeshVector(tangent, out _))
            {
                tangent = Vector3.Cross(Vector3.forward, normal);
            }
            return tangent;
        }

        private static void DestroyRenderMeshProofClone()
        {
            if (renderMeshProofSourceRenderer != null)
            {
                renderMeshProofSourceRenderer.forceRenderingOff =
                    renderMeshProofSourceForceRenderingOff;
            }
            if (renderMeshProofObject != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    renderMeshProofObject);
            }
            if (renderMeshProofMesh != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    renderMeshProofMesh);
            }
            if (renderMeshProofMaterial != null)
            {
                UnityEngine.Object.DestroyImmediate(
                    renderMeshProofMaterial);
            }
            renderMeshProofObject = null;
            renderMeshProofTarget = null;
            renderMeshProofSourceMesh = null;
            renderMeshProofMesh = null;
            renderMeshProofMaterial = null;
            renderMeshProofSourceRenderer = null;
            renderMeshProofSourceForceRenderingOff = false;
            renderMeshProofMode = RenderMeshProofMode.None;
            SceneView.RepaintAll();
        }

        private static string ResolveRenderMeshProofDisplayName(
            RenderMeshProofMode mode)
        {
            return mode switch
            {
                RenderMeshProofMode.NormalTangentRepair =>
                    "normal/tangent repair",
                RenderMeshProofMode.Unlit => "unlit material",
                _ => "none"
            };
        }

        private static bool IsZeroRenderMeshVector(Vector3 value)
        {
            return !IsFinite(value) ||
                value.sqrMagnitude <
                    RenderMeshMinimumVectorMagnitudeSqr;
        }

        private static bool IsValidVertexIndex(
            int index,
            int vertexCount)
        {
            return index >= 0 && index < vertexCount;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private static bool IsFinite(Vector2 value)
        {
            return IsFinite(value.x) && IsFinite(value.y);
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) &&
                IsFinite(value.z);
        }

        private static bool IsFinite(Vector4 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) &&
                IsFinite(value.z) && IsFinite(value.w);
        }

        private static bool IsFinite(Color value)
        {
            return IsFinite(value.r) && IsFinite(value.g) &&
                IsFinite(value.b) && IsFinite(value.a);
        }

        private static Vector3 SafeNormalized(Vector3 value)
        {
            return TryNormalizeRenderMeshVector(
                value,
                out Vector3 normalized)
                    ? normalized
                    : Vector3.zero;
        }

        private static float ResolveTangentMagnitude(Vector4 tangent)
        {
            if (!IsFinite(tangent))
            {
                return float.PositiveInfinity;
            }
            return new Vector3(
                tangent.x,
                tangent.y,
                tangent.z).magnitude;
        }

        private static float CalculateMedian(List<float> values)
        {
            if (values == null || values.Count == 0)
            {
                return 0f;
            }
            int middle = values.Count / 2;
            return values.Count % 2 == 0
                ? (values[middle - 1] + values[middle]) * 0.5f
                : values[middle];
        }

        private static string FormatAuditFloat(float value)
        {
            return value.ToString(
                "R",
                CultureInfo.InvariantCulture);
        }

        private static string FormatAuditVector2(Vector2 value)
        {
            return "(" + FormatAuditFloat(value.x) + "/" +
                FormatAuditFloat(value.y) + ")";
        }

        private static string FormatAuditVector3(Vector3 value)
        {
            return "(" + FormatAuditFloat(value.x) + "/" +
                FormatAuditFloat(value.y) + "/" +
                FormatAuditFloat(value.z) + ")";
        }

        private static string FormatAuditVector4(Vector4 value)
        {
            return "(" + FormatAuditFloat(value.x) + "/" +
                FormatAuditFloat(value.y) + "/" +
                FormatAuditFloat(value.z) + "/" +
                FormatAuditFloat(value.w) + ")";
        }

        private static string FormatAuditColor(Color value)
        {
            return "(" + FormatAuditFloat(value.r) + "/" +
                FormatAuditFloat(value.g) + "/" +
                FormatAuditFloat(value.b) + "/" +
                FormatAuditFloat(value.a) + ")";
        }

        private void OnSceneGUI()
        {
            GeneratedMass mass = target as GeneratedMass;
            if (mass == null)
            {
                return;
            }

            if (renderMeshAuditDrawWorstTriangle &&
                IsCurrentRenderMeshAudit(mass))
            {
                DrawRenderMeshAuditSceneOverlay(
                    mass,
                    lastRenderMeshAudit);
            }

            if (showPressureProfile && Application.isPlaying &&
                StylizedRiverDisturbanceRuntime.
                    TryGetGeneratedSourcePressureProfileDebugData(
                        mass,
                        out GeneratedRiverPressureProfileDebugData debugData))
            {
                DrawPressureProfileSceneOverlay(debugData);
            }
        }


        private static int CountCurrentSearchFocusEdges(
            GeneratedMass mass)
        {
            MassGenerator.EdgeWearDebugEdgeRecord[] records =
                mass != null
                    ? mass.UnifiedEdgeWearPreviewDebugEdges
                    : null;
            if (records == null)
            {
                return 0;
            }
            int count = 0;
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                if (records[recordIndex].Focus)
                {
                    count++;
                }
            }
            return count;
        }

        private static bool IsCurrentSearchFocusEdge(
            GeneratedMass mass,
            int edgeIndex)
        {
            MassGenerator.EdgeWearDebugEdgeRecord[] records =
                mass != null
                    ? mass.UnifiedEdgeWearPreviewDebugEdges
                    : null;
            if (records == null)
            {
                return false;
            }
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                if (records[recordIndex].EdgeIndex == edgeIndex)
                {
                    return records[recordIndex].Focus;
                }
            }
            return false;
        }

        private static string BuildCurrentSearchFocusEvidence(
            GeneratedMass mass)
        {
            MassGenerator.EdgeWearDebugEdgeRecord[] records =
                mass != null
                    ? mass.UnifiedEdgeWearPreviewDebugEdges
                    : null;
            if (records == null)
            {
                return "none";
            }
            string evidence = string.Empty;
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                if (!records[recordIndex].Focus)
                {
                    continue;
                }
                if (evidence.Length > 0)
                {
                    evidence += "/";
                }
                evidence += records[recordIndex].EdgeIndex.ToString();
            }
            return evidence.Length == 0 ? "none" : evidence;
        }

        private static Vector2 ResolveSourceEdgeLabelOffset(
            int edgeIndex)
        {
            float angle = edgeIndex * 137.50777f * Mathf.Deg2Rad;
            float radius = 0.24f + (edgeIndex % 3) * 0.08f;
            return new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius);
        }

        private static int CountSourceEdgeDebugState(
            MassGenerator.EdgeWearDebugEdgeRecord[] records,
            MassGenerator.EdgeWearDebugEdgeState state)
        {
            if (records == null)
            {
                return 0;
            }

            int count = 0;
            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                if (records[recordIndex].State == state)
                {
                    count++;
                }
            }
            return count;
        }

        private static Color ResolveSourceEdgeDebugColor(
            MassGenerator.EdgeWearDebugEdgeState state)
        {
            return state switch
            {
                MassGenerator.EdgeWearDebugEdgeState.Certified =>
                    new Color(0.82f, 0.96f, 1f, 0.95f),
                MassGenerator.EdgeWearDebugEdgeState.Selected =>
                    new Color(0.45f, 1f, 0.58f, 0.95f),
                MassGenerator.EdgeWearDebugEdgeState.EligibleUnselected =>
                    new Color(0.35f, 0.82f, 0.68f, 0.9f),
                MassGenerator.EdgeWearDebugEdgeState.ArtisticFiltered =>
                    new Color(1f, 0.35f, 0.9f, 0.95f),
                MassGenerator.EdgeWearDebugEdgeState.WidthFloorFailure =>
                    new Color(1f, 0.82f, 0.18f, 0.98f),
                MassGenerator.EdgeWearDebugEdgeState.IsolatedRailFailure =>
                    new Color(1f, 0.25f, 0.18f, 0.98f),
                MassGenerator.EdgeWearDebugEdgeState.CoexistenceExcluded =>
                    new Color(1f, 0.56f, 0.12f, 0.95f),
                MassGenerator.EdgeWearDebugEdgeState.GeometricExcluded =>
                    new Color(0.42f, 0.68f, 1f, 0.82f),
                MassGenerator.EdgeWearDebugEdgeState.StructuralExcluded =>
                    new Color(0.55f, 0.58f, 0.62f, 0.78f),
                _ => new Color(0.72f, 0.76f, 0.82f, 0.82f)
            };
        }

        private static string ResolveSourceEdgeDebugCode(
            MassGenerator.EdgeWearDebugEdgeState state)
        {
            return state switch
            {
                MassGenerator.EdgeWearDebugEdgeState.Certified => "C",
                MassGenerator.EdgeWearDebugEdgeState.Selected => "S",
                MassGenerator.EdgeWearDebugEdgeState.EligibleUnselected => "E",
                MassGenerator.EdgeWearDebugEdgeState.ArtisticFiltered => "A",
                MassGenerator.EdgeWearDebugEdgeState.WidthFloorFailure => "W",
                MassGenerator.EdgeWearDebugEdgeState.IsolatedRailFailure => "R",
                MassGenerator.EdgeWearDebugEdgeState.CoexistenceExcluded => "X",
                MassGenerator.EdgeWearDebugEdgeState.GeometricExcluded => "G",
                MassGenerator.EdgeWearDebugEdgeState.StructuralExcluded => "B",
                _ => "?"
            };
        }

        private static void DrawSourceEdgeIndexStatusPanel(
            GeneratedMass mass,
            MassGenerator.EdgeWearDebugEdgeRecord[] records,
            bool highlightSearchEdges)
        {
            int totalCount = records == null ? 0 : records.Length;
            int certifiedCount = CountSourceEdgeDebugState(
                records,
                MassGenerator.EdgeWearDebugEdgeState.Certified);
            int artisticCount = CountSourceEdgeDebugState(
                records,
                MassGenerator.EdgeWearDebugEdgeState.ArtisticFiltered);
            int widthCount = CountSourceEdgeDebugState(
                records,
                MassGenerator.EdgeWearDebugEdgeState.WidthFloorFailure);
            int railCount = CountSourceEdgeDebugState(
                records,
                MassGenerator.EdgeWearDebugEdgeState.IsolatedRailFailure);
            int otherCount = Mathf.Max(
                0,
                totalCount - certifiedCount - artisticCount -
                    widthCount - railCount);
            string focusEvidence = highlightSearchEdges
                ? BuildCurrentSearchFocusEvidence(mass)
                : "disabled";
            Handles.BeginGUI();
            Rect panel = new Rect(12f, 12f, 390f, 82f);
            GUI.Box(panel, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(
                new Rect(22f, 18f, 368f, 20f),
                "Source edges: seed " +
                    mass.SourceEdgeIndexDebugShapeSeed +
                    " / " + totalCount + " records",
                EditorStyles.boldLabel);
            GUI.Label(
                new Rect(22f, 40f, 368f, 18f),
                "C " + certifiedCount + "  A " + artisticCount +
                    "  W " + widthCount + "  R " + railCount +
                    "  Other " + otherCount,
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(22f, 60f, 368f, 18f),
                "Search highlights: {" + focusEvidence + "}",
                EditorStyles.miniLabel);
            Handles.EndGUI();
        }

        private static void DrawSourceEdgeIndexOverlay(
            GeneratedMass mass,
            bool highlightSearchEdges,
            bool xRay,
            SceneView sceneView)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            MassGenerator.EdgeWearDebugEdgeRecord[] records =
                mass.SourceEdgeIndexDebugEdges;
            DrawSourceEdgeIndexStatusPanel(
                mass,
                records,
                highlightSearchEdges);
            if (records == null || records.Length == 0)
            {
                return;
            }

            Color previousColor = Handles.color;
            UnityEngine.Rendering.CompareFunction previousZTest =
                Handles.zTest;
            Handles.zTest = xRay
                ? UnityEngine.Rendering.CompareFunction.Always
                : UnityEngine.Rendering.CompareFunction.LessEqual;

            GUIStyle normalStyle = new GUIStyle(
                EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(4, 4, 1, 1)
            };
            normalStyle.normal.textColor = Color.white;
            GUIStyle focusStyle = new GUIStyle(normalStyle);
            focusStyle.normal.textColor =
                new Color(1f, 0.72f, 0.18f, 1f);

            Transform cameraTransform = sceneView != null &&
                sceneView.camera != null
                    ? sceneView.camera.transform
                    : null;
            Vector3 labelRight = cameraTransform != null
                ? cameraTransform.right
                : Vector3.right;
            Vector3 labelUp = cameraTransform != null
                ? cameraTransform.up
                : Vector3.up;

            for (int recordIndex = 0;
                 recordIndex < records.Length;
                 recordIndex++)
            {
                MassGenerator.EdgeWearDebugEdgeRecord record =
                    records[recordIndex];
                bool focus = highlightSearchEdges &&
                    IsCurrentSearchFocusEdge(
                        mass,
                        record.EdgeIndex);
                Vector3 start =
                    mass.transform.TransformPoint(record.Start);
                Vector3 end =
                    mass.transform.TransformPoint(record.End);
                Vector3 midpoint = (start + end) * 0.5f;
                float handleSize =
                    HandleUtility.GetHandleSize(midpoint);
                Color edgeColor = focus
                    ? new Color(1f, 0.58f, 0.08f, 1f)
                    : ResolveSourceEdgeDebugColor(record.State);

                Handles.color = new Color(0f, 0f, 0f, 0.88f);
                Handles.DrawAAPolyLine(
                    focus ? 7f : 4f,
                    start,
                    end);
                Handles.color = edgeColor;
                Handles.DrawAAPolyLine(
                    focus ? 3.5f : 1.8f,
                    start,
                    end);

                Vector2 offset =
                    ResolveSourceEdgeLabelOffset(record.EdgeIndex);
                Vector3 labelPosition = midpoint +
                    (labelRight * offset.x + labelUp * offset.y) *
                    handleSize;
                Handles.color = new Color(0f, 0f, 0f, 0.76f);
                Handles.DrawAAPolyLine(
                    2.5f,
                    midpoint,
                    labelPosition);
                Handles.color = edgeColor;
                Handles.DrawAAPolyLine(
                    1.25f,
                    midpoint,
                    labelPosition);
                normalStyle.normal.textColor = edgeColor;
                Handles.Label(
                    labelPosition,
                    " " + record.EdgeIndex + " " +
                        ResolveSourceEdgeDebugCode(record.State) + " ",
                    focus ? focusStyle : normalStyle);
            }

            Handles.zTest = previousZTest;
            Handles.color = previousColor;
        }

        private static void DrawPressureProfileSceneOverlay(
            GeneratedRiverPressureProfileDebugData debugData)
        {
            if (!debugData.IsValid ||
                Event.current.type != EventType.Repaint)
            {
                return;
            }

            SceneView sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                return;
            }

            float panelWidth = Mathf.Clamp(
                sceneView.position.width - 32f,
                520f,
                760f);
            Rect panelRect = new Rect(16f, 48f, panelWidth, 466f);
            Rect heightGraph = new Rect(
                panelRect.x + 12f,
                panelRect.y + 98f,
                panelRect.width - 24f,
                178f);
            Rect contactGraph = new Rect(
                panelRect.x + 12f,
                panelRect.y + 312f,
                panelRect.width - 24f,
                110f);

            Handles.BeginGUI();
            GUI.Box(panelRect, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(
                new Rect(
                    panelRect.x + 10f,
                    panelRect.y + 8f,
                    panelRect.width - 20f,
                    20f),
                "Pressure Profile Diagnostics",
                EditorStyles.boldLabel);
            GUI.Label(
                new Rect(
                    panelRect.x + 10f,
                    panelRect.y + 29f,
                    panelRect.width - 20f,
                    18f),
                $"Rows {debugData.LateralSampleCount} " +
                $"({debugData.ValidRowCount} valid) | " +
                $"Requested {debugData.RequestedProfileWidthPixels:F1} px | " +
                $"Target {debugData.TargetHeight:F3} m",
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(
                    panelRect.x + 10f,
                    panelRect.y + 47f,
                    panelRect.width - 20f,
                    18f),
                $"Support-limited below target " +
                $"{debugData.SupportLimitedBelowTargetRowCount} | " +
                $"Endpoint taper {debugData.EndpointTaperRowCount} | " +
                $"At target {debugData.TargetHeightRowCount}",
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(
                    panelRect.x + 10f,
                    panelRect.y + 65f,
                    panelRect.width - 20f,
                    18f),
                $"Thickness {debugData.RowThicknessRange.x:F3}–" +
                $"{debugData.RowThicknessRange.y:F3} m | " +
                $"Geometry-clamped {debugData.GeometryClampedRowCount} | " +
                $"Rear-region violations " +
                $"{debugData.ProtectedDownstreamRegionViolationRowCount}",
                EditorStyles.miniLabel);

            DrawHeightProfileGraph(heightGraph, debugData);
            DrawContactProfileGraph(contactGraph, debugData);
            Handles.EndGUI();
        }

        private static void DrawHeightProfileGraph(
            Rect graphRect,
            GeneratedRiverPressureProfileDebugData debugData)
        {
            EditorGUI.DrawRect(graphRect, new Color(0.08f, 0.08f, 0.08f, 0.94f));
            DrawGraphGrid(graphRect, debugData.LateralSampleCount);

            float maximumHeight = Mathf.Max(
                debugData.TargetHeight,
                debugData.InteriorCeilingRange.y,
                debugData.LocalCeilingRange.y,
                0.001f) * 1.08f;

            DrawHorizontalGraphValue(
                graphRect,
                debugData.TargetHeight,
                0f,
                maximumHeight,
                new Color(0.30f, 1f, 0.30f, 0.85f));
            DrawHeightSeries(
                graphRect,
                debugData,
                0,
                0f,
                maximumHeight,
                new Color(1f, 0.88f, 0.18f, 1f));
            DrawHeightSeries(
                graphRect,
                debugData,
                1,
                0f,
                maximumHeight,
                new Color(1f, 0.48f, 0.12f, 1f));
            DrawHeightSeries(
                graphRect,
                debugData,
                2,
                0f,
                maximumHeight,
                new Color(0.10f, 0.90f, 1f, 1f));
            DrawHeightSeries(
                graphRect,
                debugData,
                3,
                0f,
                maximumHeight,
                new Color(1f, 0.20f, 0.85f, 1f));
            DrawHeightClassifications(
                graphRect,
                debugData,
                maximumHeight);

            GUI.Label(
                new Rect(
                    graphRect.x + 5f,
                    graphRect.y + 3f,
                    graphRect.width - 10f,
                    18f),
                "Height profile (metres)",
                EditorStyles.miniBoldLabel);
            GUI.Label(
                new Rect(
                    graphRect.x + 4f,
                    graphRect.y + 20f,
                    72f,
                    16f),
                maximumHeight.ToString("F3"),
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(
                    graphRect.x + 4f,
                    graphRect.yMax - 32f,
                    72f,
                    16f),
                "0.000",
                EditorStyles.miniLabel);
            DrawLegendItem(
                graphRect.x + 6f,
                graphRect.yMax + 4f,
                new Color(1f, 0.88f, 0.18f, 1f),
                "base before taper");
            DrawLegendItem(
                graphRect.x + 142f,
                graphRect.yMax + 4f,
                new Color(1f, 0.48f, 0.12f, 1f),
                "base after taper");
            DrawLegendItem(
                graphRect.x + 272f,
                graphRect.yMax + 4f,
                new Color(0.10f, 0.90f, 1f, 1f),
                "current");
            DrawLegendItem(
                graphRect.x + 355f,
                graphRect.yMax + 4f,
                new Color(1f, 0.20f, 0.85f, 1f),
                "ceiling before taper");
            DrawLegendItem(
                graphRect.x + 508f,
                graphRect.yMax + 4f,
                new Color(0.30f, 1f, 0.30f, 0.85f),
                "target height");
        }

        private static void DrawContactProfileGraph(
            Rect graphRect,
            GeneratedRiverPressureProfileDebugData debugData)
        {
            EditorGUI.DrawRect(graphRect, new Color(0.08f, 0.08f, 0.08f, 0.94f));
            DrawGraphGrid(graphRect, debugData.LateralSampleCount);

            float minimum = float.PositiveInfinity;
            float maximum = float.NegativeInfinity;
            for (int row = 0; row < debugData.LateralSampleCount; row++)
            {
                Vector4 baseSample = debugData.BaseSamples[row];
                Vector4 currentSample = debugData.CurrentSamples[row];
                if (baseSample.z <= 0.0001f ||
                    baseSample.w <= 0.0001f)
                {
                    continue;
                }

                float waterline = baseSample.x;
                float cachedContact =
                    baseSample.x + baseSample.y * baseSample.z;
                float currentContact =
                    currentSample.x +
                    currentSample.y * currentSample.z;
                float ceilingContact =
                    baseSample.x + baseSample.y * baseSample.w;
                float downstreamBoundary =
                    debugData.DownstreamBoundaries[row];
                float protectedDownstreamStart = Mathf.Lerp(
                    waterline,
                    downstreamBoundary,
                    debugData.ProtectedDownstreamStartPercent * 0.01f);
                minimum = Mathf.Min(minimum, waterline);
                minimum = Mathf.Min(minimum, cachedContact);
                minimum = Mathf.Min(minimum, currentContact);
                minimum = Mathf.Min(minimum, ceilingContact);
                minimum = Mathf.Min(minimum, protectedDownstreamStart);
                minimum = Mathf.Min(minimum, downstreamBoundary);
                maximum = Mathf.Max(maximum, waterline);
                maximum = Mathf.Max(maximum, cachedContact);
                maximum = Mathf.Max(maximum, currentContact);
                maximum = Mathf.Max(maximum, ceilingContact);
                maximum = Mathf.Max(maximum, protectedDownstreamStart);
                maximum = Mathf.Max(maximum, downstreamBoundary);
            }

            if (float.IsInfinity(minimum))
            {
                return;
            }

            float padding = Mathf.Max(0.01f, (maximum - minimum) * 0.10f);
            minimum -= padding;
            maximum += padding;
            DrawContactSeries(
                graphRect,
                debugData,
                0,
                minimum,
                maximum,
                new Color(0.72f, 0.72f, 0.72f, 1f));
            DrawContactSeries(
                graphRect,
                debugData,
                1,
                minimum,
                maximum,
                new Color(1f, 0.88f, 0.18f, 1f));
            DrawContactSeries(
                graphRect,
                debugData,
                2,
                minimum,
                maximum,
                new Color(0.10f, 0.90f, 1f, 1f));
            DrawContactSeries(
                graphRect,
                debugData,
                3,
                minimum,
                maximum,
                new Color(1f, 0.20f, 0.85f, 1f));
            DrawContactSeries(
                graphRect,
                debugData,
                4,
                minimum,
                maximum,
                new Color(1f, 0.55f, 0.15f, 1f));
            DrawContactSeries(
                graphRect,
                debugData,
                5,
                minimum,
                maximum,
                new Color(0.55f, 0.35f, 1f, 1f));

            GUI.Label(
                new Rect(
                    graphRect.x + 5f,
                    graphRect.y + 3f,
                    graphRect.width - 10f,
                    18f),
                "Contact and row boundaries (metres relative to source)",
                EditorStyles.miniBoldLabel);
            GUI.Label(
                new Rect(
                    graphRect.x + 4f,
                    graphRect.y + 20f,
                    72f,
                    16f),
                maximum.ToString("F3"),
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(
                    graphRect.x + 4f,
                    graphRect.yMax - 32f,
                    72f,
                    16f),
                minimum.ToString("F3"),
                EditorStyles.miniLabel);
            DrawLegendItem(
                graphRect.x + 6f,
                graphRect.yMax + 4f,
                new Color(0.72f, 0.72f, 0.72f, 1f),
                "waterline");
            DrawLegendItem(
                graphRect.x + 102f,
                graphRect.yMax + 4f,
                new Color(1f, 0.88f, 0.18f, 1f),
                "cached contact");
            DrawLegendItem(
                graphRect.x + 218f,
                graphRect.yMax + 4f,
                new Color(0.10f, 0.90f, 1f, 1f),
                "current contact");
            DrawLegendItem(
                graphRect.x + 340f,
                graphRect.yMax + 4f,
                new Color(1f, 0.20f, 0.85f, 1f),
                "ceiling contact");
            DrawLegendItem(
                graphRect.x + 6f,
                graphRect.yMax + 20f,
                new Color(1f, 0.55f, 0.15f, 1f),
                "rear protection");
            DrawLegendItem(
                graphRect.x + 132f,
                graphRect.yMax + 20f,
                new Color(0.55f, 0.35f, 1f, 1f),
                "downstream edge");
        }

        private static void DrawGraphGrid(Rect rect, int rowCount)
        {
            Color previousColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.09f);
            for (int row = 0; row < rowCount; row++)
            {
                float x = ResolveGraphX(rect, row, rowCount);
                Handles.DrawLine(
                    new Vector3(x, rect.y),
                    new Vector3(x, rect.yMax));
            }

            for (int line = 0; line <= 4; line++)
            {
                float y = Mathf.Lerp(rect.yMax, rect.y, line / 4f);
                Handles.DrawLine(
                    new Vector3(rect.x, y),
                    new Vector3(rect.xMax, y));
            }
            Handles.color = previousColor;

            GUI.Label(
                new Rect(rect.x, rect.yMax - 16f, 34f, 16f),
                "0",
                EditorStyles.miniLabel);
            GUI.Label(
                new Rect(rect.xMax - 38f, rect.yMax - 16f, 38f, 16f),
                (rowCount - 1).ToString(),
                EditorStyles.miniLabel);
        }

        private static void DrawHeightSeries(
            Rect rect,
            GeneratedRiverPressureProfileDebugData debugData,
            int series,
            float minimum,
            float maximum,
            Color color)
        {
            Color previousColor = Handles.color;
            Handles.color = color;
            bool hasPrevious = false;
            Vector3 previous = Vector3.zero;
            for (int row = 0; row < debugData.LateralSampleCount; row++)
            {
                Vector4 baseSample = debugData.BaseSamples[row];
                Vector4 currentSample = debugData.CurrentSamples[row];
                if (baseSample.z <= 0.0001f ||
                    baseSample.w <= 0.0001f)
                {
                    hasPrevious = false;
                    continue;
                }

                float taper = ResolveEndpointTaper(
                    row,
                    debugData.LateralSampleCount);
                float value = series switch
                {
                    0 => taper > 0.0001f
                        ? baseSample.z / taper
                        : 0f,
                    1 => baseSample.z,
                    2 => currentSample.z,
                    3 => taper > 0.0001f
                        ? baseSample.w / taper
                        : 0f,
                    _ => 0f
                };
                Vector3 point = new Vector3(
                    ResolveGraphX(
                        rect,
                        row,
                        debugData.LateralSampleCount),
                    ResolveGraphY(rect, value, minimum, maximum));
                if (hasPrevious)
                {
                    Handles.DrawLine(previous, point);
                }

                previous = point;
                hasPrevious = true;
            }
            Handles.color = previousColor;
        }

        private static void DrawContactSeries(
            Rect rect,
            GeneratedRiverPressureProfileDebugData debugData,
            int series,
            float minimum,
            float maximum,
            Color color)
        {
            Color previousColor = Handles.color;
            Handles.color = color;
            bool hasPrevious = false;
            Vector3 previous = Vector3.zero;
            for (int row = 0; row < debugData.LateralSampleCount; row++)
            {
                Vector4 baseSample = debugData.BaseSamples[row];
                Vector4 currentSample = debugData.CurrentSamples[row];
                if (baseSample.z <= 0.0001f ||
                    baseSample.w <= 0.0001f)
                {
                    hasPrevious = false;
                    continue;
                }

                float value = series switch
                {
                    0 => baseSample.x,
                    1 => baseSample.x + baseSample.y * baseSample.z,
                    2 => currentSample.x +
                         currentSample.y * currentSample.z,
                    3 => baseSample.x + baseSample.y * baseSample.w,
                    4 => Mathf.Lerp(
                        baseSample.x,
                        debugData.DownstreamBoundaries[row],
                        debugData.ProtectedDownstreamStartPercent * 0.01f),
                    5 => debugData.DownstreamBoundaries[row],
                    _ => 0f
                };
                Vector3 point = new Vector3(
                    ResolveGraphX(
                        rect,
                        row,
                        debugData.LateralSampleCount),
                    ResolveGraphY(rect, value, minimum, maximum));
                if (hasPrevious)
                {
                    Handles.DrawLine(previous, point);
                }

                previous = point;
                hasPrevious = true;
            }
            Handles.color = previousColor;
        }

        private static void DrawHeightClassifications(
            Rect rect,
            GeneratedRiverPressureProfileDebugData debugData,
            float maximumHeight)
        {
            for (int row = 0; row < debugData.LateralSampleCount; row++)
            {
                Vector4 baseSample = debugData.BaseSamples[row];
                if (baseSample.z <= 0.0001f ||
                    baseSample.w <= 0.0001f)
                {
                    EditorGUI.DrawRect(
                        new Rect(
                            ResolveGraphX(
                                rect,
                                row,
                                debugData.LateralSampleCount) - 2f,
                            rect.yMax - 5f,
                            4f,
                            4f),
                        new Color(1f, 0.18f, 0.18f, 1f));
                    continue;
                }

                float taper = ResolveEndpointTaper(
                    row,
                    debugData.LateralSampleCount);
                if (taper <= 0.0001f)
                {
                    continue;
                }

                float untaperedCeiling = baseSample.w / taper;
                float x = ResolveGraphX(
                    rect,
                    row,
                    debugData.LateralSampleCount);

                if (untaperedCeiling <
                    debugData.TargetHeight - 0.0005f)
                {
                    float y = ResolveGraphY(
                        rect,
                        untaperedCeiling,
                        0f,
                        maximumHeight);
                    EditorGUI.DrawRect(
                        new Rect(x - 2f, y - 2f, 4f, 4f),
                        new Color(1f, 0.18f, 0.18f, 1f));
                }

                if (taper < 0.999f)
                {
                    EditorGUI.DrawRect(
                        new Rect(x - 1f, rect.yMax - 5f, 3f, 4f),
                        new Color(1f, 0.48f, 0.12f, 1f));
                }
            }
        }

        private static void DrawHorizontalGraphValue(
            Rect rect,
            float value,
            float minimum,
            float maximum,
            Color color)
        {
            if (value <= 0f)
            {
                return;
            }

            Color previousColor = Handles.color;
            Handles.color = color;
            float y = ResolveGraphY(rect, value, minimum, maximum);
            Handles.DrawLine(
                new Vector3(rect.x, y),
                new Vector3(rect.xMax, y));
            Handles.color = previousColor;
        }

        private static void DrawLegendItem(
            float x,
            float y,
            Color color,
            string label)
        {
            EditorGUI.DrawRect(new Rect(x, y + 4f, 10f, 3f), color);
            GUI.Label(
                new Rect(x + 14f, y - 2f, 130f, 16f),
                label,
                EditorStyles.miniLabel);
        }

        private static float ResolveEndpointTaper(int row, int rowCount)
        {
            float row01 = rowCount > 1
                ? row / (float)(rowCount - 1)
                : 0.5f;
            float lateral01 = Mathf.Abs(row01 * 2f - 1f);
            return 1f - Mathf.SmoothStep(
                0f,
                1f,
                Mathf.InverseLerp(0.82f, 1f, lateral01));
        }

        private static float ResolveGraphX(
            Rect rect,
            int row,
            int rowCount)
        {
            float row01 = rowCount > 1
                ? row / (float)(rowCount - 1)
                : 0.5f;
            return Mathf.Lerp(rect.x, rect.xMax, row01);
        }

        private static float ResolveGraphY(
            Rect rect,
            float value,
            float minimum,
            float maximum)
        {
            float normalized = Mathf.InverseLerp(
                minimum,
                maximum,
                value);
            return Mathf.Lerp(rect.yMax, rect.y, normalized);
        }

        private void ApplyToTargets(
            string undoName,
            ActionForMass action)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                GeneratedMass mass = targets[i] as GeneratedMass;

                if (mass == null)
                {
                    continue;
                }

                Undo.RecordObject(mass, undoName);
                action(mass);
                EditorUtility.SetDirty(mass);
            }

            serializedObject.Update();
            Repaint();
        }

        private delegate void ActionForMass(GeneratedMass mass);
    }
}
