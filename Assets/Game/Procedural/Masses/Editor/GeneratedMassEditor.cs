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
            (int)StoneSurfaceMaskDebug.BoundaryFieldDiagnostic,
            (int)StoneSurfaceMaskDebug.BoundaryModulationDiagnostic,
            (int)StoneSurfaceMaskDebug.Exposure,
            (int)StoneSurfaceMaskDebug.CreviceBase,
            (int)StoneSurfaceMaskDebug.DirtDeposit
        };

        private static readonly GUIContent[] CommonDebugLabels =
        {
            new GUIContent("None"),
            new GUIContent("Convex Edge Wear"),
            new GUIContent("Atlas Boundary Field Diagnostic", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
            new GUIContent("Atlas Boundary Modulation Diagnostic", "Legacy generated-mass atlas diagnostic. Not physical bevel geometry."),
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
        private bool showAdvancedFeatureDiagnostics;
        private bool showCreaseDebugFeature;
        private bool showPressureProfile;

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
                    mass => mass.Regenerate());
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
                "Common debug views stay in the main selector. Raw boundary " +
                "channels live in Advanced Feature Diagnostics so normal use " +
                "does not turn into a long debug-control list.",
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
                "Advanced Feature Diagnostics",
                true);

            if (!showAdvancedFeatureDiagnostics)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.HelpBox(
                    "Raw generated feature-atlas inspection. FeatureAtlas0 stores " +
                    "boundary structure fields; FeatureAtlas1 stores boundary " +
                    "coordinate/modulation fields. These views are for validation, " +
                    "not normal material authoring.",
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
                        "Raw Feature Channel",
                        "Inspects separated semantic and baked irregularity channels for generated mass features."),
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
                        "Controls generated bevel/chamfer depth on selected convex edges."));
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
            if (!showPressureProfile || !Application.isPlaying)
            {
                return;
            }

            GeneratedMass mass = target as GeneratedMass;
            if (mass == null ||
                Selection.activeGameObject != mass.gameObject ||
                !StylizedRiverDisturbanceRuntime.
                    TryGetGeneratedSourcePressureProfileDebugData(
                        mass,
                        out GeneratedRiverPressureProfileDebugData debugData))
            {
                return;
            }

            DrawPressureProfileSceneOverlay(debugData);
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
