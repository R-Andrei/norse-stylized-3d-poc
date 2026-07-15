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
            MassGenerator.EdgeWearBatchAuditCaseResult result =
                job.RequireAllGeometricCandidates
                    ? MassGenerator.GenerateUnifiedEdgeWearBatchAuditCase(
                        caseRecipe,
                        settings)
                    : MassGenerator
                        .GenerateUnifiedEdgeWearPreviewParityAuditCase(
                            caseRecipe,
                            settings);
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
                ",topologyCollateralFailures:" +
                    suite.TopologyCollateralFailures +
                ",previewCollateralFailures:" +
                    suite.PreviewCollateralFailures +
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
            builder.AppendLine("contract=EW-B4.2R12A-suite");
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
            return builder.ToString();
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
                }
                else
                {
                    PreviewAggregate = aggregate;
                    PreviewReportText = reportText ?? string.Empty;
                }
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
                ? "EW-B4.2R12A-topology"
                : "EW-B4.2R12A-preview";

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

        private void OnSceneGUI()
        {
            GeneratedMass mass = target as GeneratedMass;
            if (mass == null)
            {
                return;
            }

            if (!showPressureProfile || !Application.isPlaying ||
                !StylizedRiverDisturbanceRuntime.
                    TryGetGeneratedSourcePressureProfileDebugData(
                        mass,
                        out GeneratedRiverPressureProfileDebugData debugData))
            {
                return;
            }

            DrawPressureProfileSceneOverlay(debugData);
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
