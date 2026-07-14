using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    [CustomEditor(typeof(GeneratedGround))]
    [CanEditMultipleObjects]
    public sealed class GeneratedGroundEditor : UnityEditor.Editor
    {
        private SerializedProperty recipe;
        private SerializedProperty surfaceStyleProfile;
        private SerializedProperty surfaceVariantId;
        private SerializedProperty overrideSurfaceProfile;
        private SerializedProperty surfaceProfile;
        private SerializedProperty overrideMaterialControls;
        private SerializedProperty groundMaterialControls;
        private SerializedProperty regenerateOnValidate;
        private SerializedProperty debugView;
        private SerializedProperty showPaintedAccentDistributionOverlay;
        private SerializedProperty showPaintedAccentWeightedProposals;
        private SerializedProperty showPaintedAccentLastAcceptedPositions;
        private SerializedProperty showPaintedAccentCompositionDebug;
        private SerializedProperty showPaintedAccentProjectedGlyphDebug;
        private SerializedProperty paintedAccentGlyphFamilyPreview;
        private SerializedProperty paintedAccentPlacementOverlayWeight;

        private int paintedAccentPlacementDebugSignature = int.MinValue;
        private bool paintedAccentPlacementDebugSnapshotBuildFailed;
        private GroundPaintedAccentPlacementDebugSnapshot
            paintedAccentPlacementDebugSnapshot =
                GroundPaintedAccentPlacementDebugSnapshot.Empty;
        private int paintedAccentProjectedGlyphDebugSignature = int.MinValue;
        private bool paintedAccentProjectedGlyphDebugSnapshotBuildFailed;
        private GroundPaintedAccentProjectedGlyphDebugSnapshot
            paintedAccentProjectedGlyphDebugSnapshot =
                GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;

        private SerializedProperty shapeSeed;
        private SerializedProperty patchSize;
        private SerializedProperty resolution;
        private SerializedProperty patchCoordinate;
        private SerializedProperty transitionDirection;
        private SerializedProperty transitionHeight;
        private SerializedProperty profile;
        private SerializedProperty broadForm;
        private SerializedProperty roughness;
        private SerializedProperty surfaceDetail;
        private SerializedProperty edgeBlend;
        private SerializedProperty surfaceVariation;
        private SerializedProperty useModifiers;

        private SerializedProperty baseColor;
        private SerializedProperty frostColor;
        private SerializedProperty dampTint;
        private SerializedProperty dampTintStrength;
        private SerializedProperty rockyDryTint;
        private SerializedProperty rockyDryTintStrength;
        private SerializedProperty vegetationTint;
        private SerializedProperty vegetationTintStrength;
        private SerializedProperty pixelCellSize;
        private SerializedProperty pixelToneCount;
        private SerializedProperty pixelClusterStrength;
        private SerializedProperty pixelVariation;
        private SerializedProperty broadVariation;
        private SerializedProperty vertexVariation;
        private SerializedProperty pixelEffectStrength;
        private SerializedProperty cellWarpStrength;
        private SerializedProperty groundMacroPatchScale;
        private SerializedProperty profileContrastScale;
        private SerializedProperty profilePixelContrastScale;
        private SerializedProperty groundSnowResponseScale;
        private SerializedProperty groundDampResponseScale;
        private SerializedProperty groundVegetationResponseScale;
        private SerializedProperty groundRockyDryResponseScale;
        private SerializedProperty groundShoreDampStrengthScale;
        private SerializedProperty groundPatchBlendStrength;
        private SerializedProperty groundSnowTintStrength;
        private SerializedProperty groundSnowBrightness;
        private SerializedProperty groundDampDarkenStrength;
        private SerializedProperty wetness;
        private SerializedProperty wetDarkenStrength;
        private SerializedProperty wetPixelSoftening;
        private SerializedProperty wetSmoothnessBoost;
        private SerializedProperty frostStrength;
        private SerializedProperty frostContrast;
        private SerializedProperty monolithicFlatten;
        private SerializedProperty monolithicSmoothnessBoost;
        private SerializedProperty smoothness;
        private SerializedProperty specularStrength;

        private bool showGroundSurface = true;
        private bool showResolvedFeatureSummary;
        private bool showGroundDebug;
        private bool showRegenerationAccounting;
        private bool showPaintedAccentStrokes = true;
        private bool showPaintedAccentBasics = true;
        private bool showPaintedAccentDistribution;
        private bool showPaintedAccentHorizontalCompanions = true;
        private bool showPaintedAccentAdvancedCompanionLayoutMix;
        private bool showPaintedAccentFamilyMix;
        private bool showPaintedAccentGeometry;
        private bool showPaintedAccentProfile;
        private bool showPaintedAccentInk;
        private bool showPaintedAccentPlacementDebug;
        private bool showPaintedAccentPlacementOverlays;
        private bool showPaintedAccentShapeOverlay;
        private bool showPaintedAccentDiagnostics;
        private bool showGeneration;
        private bool showPatch;
        private bool showTransition;
        private bool showShape;
        private bool showSurface;
        private bool showSurfaceDiagnostics;
        private bool showModifiers;
        private bool showMaterialControls;
        private bool showMaterialPalette;
        private bool showMaterialPixelVariation;
        private bool showMaterialSemanticResponse;
        private bool showMaterialWeatherFinish;
        private bool showStyleAssetDetails;
        private bool showAdvanced;


        private static bool DrawSectionFoldout(
            ref bool expanded,
            string label,
            float spacing = 8f)
        {
            if (spacing > 0f)
            {
                EditorGUILayout.Space(spacing);
            }

            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);
            return expanded;
        }

        private static bool DrawSubsectionFoldout(
            ref bool expanded,
            string label)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);
            return expanded;
        }

        private void OnEnable()
        {
            recipe =
                serializedObject.FindProperty("recipe");

            surfaceStyleProfile =
                serializedObject.FindProperty("surfaceStyleProfile");

            surfaceVariantId =
                serializedObject.FindProperty("surfaceVariantId");

            overrideSurfaceProfile =
                serializedObject.FindProperty("overrideSurfaceProfile");

            surfaceProfile =
                serializedObject.FindProperty("surfaceProfile");

            overrideMaterialControls =
                serializedObject.FindProperty("overrideMaterialControls");

            groundMaterialControls =
                serializedObject.FindProperty("groundMaterialControls");

            regenerateOnValidate =
                serializedObject.FindProperty(
                    "regenerateOnValidate");

            debugView =
                serializedObject.FindProperty("debugView");

            showPaintedAccentDistributionOverlay =
                serializedObject.FindProperty(
                    "showPaintedAccentDistributionOverlay");

            showPaintedAccentWeightedProposals =
                serializedObject.FindProperty(
                    "showPaintedAccentWeightedProposals");

            showPaintedAccentLastAcceptedPositions =
                serializedObject.FindProperty(
                    "showPaintedAccentLastAcceptedPositions");

            showPaintedAccentCompositionDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentCompositionDebug");

            showPaintedAccentProjectedGlyphDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentProjectedGlyphDebug");

            paintedAccentGlyphFamilyPreview =
                serializedObject.FindProperty(
                    "paintedAccentGlyphFamilyPreview");

            paintedAccentPlacementOverlayWeight =
                serializedObject.FindProperty(
                    "paintedAccentPlacementOverlayWeight");

            shapeSeed =
                recipe.FindPropertyRelative("shapeSeed");

            patchSize =
                recipe.FindPropertyRelative("patchSize");

            resolution =
                recipe.FindPropertyRelative("resolution");

            patchCoordinate =
                recipe.FindPropertyRelative("patchCoordinate");

            transitionDirection =
                recipe.FindPropertyRelative(
                    "transitionDirection");

            transitionHeight =
                recipe.FindPropertyRelative(
                    "transitionHeight");

            profile =
                recipe.FindPropertyRelative("profile");

            broadForm =
                recipe.FindPropertyRelative("broadForm");

            roughness =
                recipe.FindPropertyRelative("roughness");

            surfaceDetail =
                recipe.FindPropertyRelative("surfaceDetail");

            edgeBlend =
                recipe.FindPropertyRelative("edgeBlend");

            surfaceVariation =
                recipe.FindPropertyRelative(
                    "surfaceVariation");

            useModifiers =
                recipe.FindPropertyRelative("useModifiers");

            baseColor =
                groundMaterialControls.FindPropertyRelative("baseColor");

            frostColor =
                groundMaterialControls.FindPropertyRelative("frostColor");

            dampTint =
                groundMaterialControls.FindPropertyRelative("dampTint");

            dampTintStrength =
                groundMaterialControls.FindPropertyRelative("dampTintStrength");

            rockyDryTint =
                groundMaterialControls.FindPropertyRelative("rockyDryTint");

            rockyDryTintStrength =
                groundMaterialControls.FindPropertyRelative("rockyDryTintStrength");

            vegetationTint =
                groundMaterialControls.FindPropertyRelative("vegetationTint");

            vegetationTintStrength =
                groundMaterialControls.FindPropertyRelative("vegetationTintStrength");

            pixelCellSize =
                groundMaterialControls.FindPropertyRelative("pixelCellSize");

            pixelToneCount =
                groundMaterialControls.FindPropertyRelative("pixelToneCount");

            pixelClusterStrength =
                groundMaterialControls.FindPropertyRelative("pixelClusterStrength");

            pixelVariation =
                groundMaterialControls.FindPropertyRelative("pixelVariation");

            broadVariation =
                groundMaterialControls.FindPropertyRelative("broadVariation");

            vertexVariation =
                groundMaterialControls.FindPropertyRelative("vertexVariation");

            pixelEffectStrength =
                groundMaterialControls.FindPropertyRelative("pixelEffectStrength");

            cellWarpStrength =
                groundMaterialControls.FindPropertyRelative("cellWarpStrength");

            groundMacroPatchScale =
                groundMaterialControls.FindPropertyRelative("groundMacroPatchScale");

            profileContrastScale =
                groundMaterialControls.FindPropertyRelative("profileContrastScale");

            profilePixelContrastScale =
                groundMaterialControls.FindPropertyRelative("profilePixelContrastScale");

            groundSnowResponseScale =
                groundMaterialControls.FindPropertyRelative("groundSnowResponseScale");

            groundDampResponseScale =
                groundMaterialControls.FindPropertyRelative("groundDampResponseScale");

            groundVegetationResponseScale =
                groundMaterialControls.FindPropertyRelative("groundVegetationResponseScale");

            groundRockyDryResponseScale =
                groundMaterialControls.FindPropertyRelative("groundRockyDryResponseScale");

            groundShoreDampStrengthScale =
                groundMaterialControls.FindPropertyRelative("groundShoreDampStrengthScale");

            groundPatchBlendStrength =
                groundMaterialControls.FindPropertyRelative("groundPatchBlendStrength");

            groundSnowTintStrength =
                groundMaterialControls.FindPropertyRelative("groundSnowTintStrength");

            groundSnowBrightness =
                groundMaterialControls.FindPropertyRelative("groundSnowBrightness");

            groundDampDarkenStrength =
                groundMaterialControls.FindPropertyRelative("groundDampDarkenStrength");

            wetness =
                groundMaterialControls.FindPropertyRelative("wetness");

            wetDarkenStrength =
                groundMaterialControls.FindPropertyRelative("wetDarkenStrength");

            wetPixelSoftening =
                groundMaterialControls.FindPropertyRelative("wetPixelSoftening");

            wetSmoothnessBoost =
                groundMaterialControls.FindPropertyRelative("wetSmoothnessBoost");

            frostStrength =
                groundMaterialControls.FindPropertyRelative("frostStrength");

            frostContrast =
                groundMaterialControls.FindPropertyRelative("frostContrast");

            monolithicFlatten =
                groundMaterialControls.FindPropertyRelative("monolithicFlatten");

            monolithicSmoothnessBoost =
                groundMaterialControls.FindPropertyRelative("monolithicSmoothnessBoost");

            smoothness =
                groundMaterialControls.FindPropertyRelative("smoothness");

            specularStrength =
                groundMaterialControls.FindPropertyRelative("specularStrength");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGroundSurfaceAuthoringSection();
            DrawGroundDebugSection();
            DrawRegenerationAccountingSection();
            DrawPaintedAccentStrokeControls();
            DrawGenerationSection();
            DrawPatchSection();
            DrawTransitionSection();
            DrawShapeSection();
            DrawSurfaceSection();
            DrawModifierSection();
            DrawAdvancedSection();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(10f);
            DrawActionButtons();
        }

        private void DrawGroundSurfaceAuthoringSection()
        {
            if (!DrawSectionFoldout(
                    ref showGroundSurface,
                    "Ground Surface",
                    0f))
            {
                return;
            }

            EditorGUI.indentLevel++;

            DrawSurfaceFamilyPopup();

            GroundSurfaceStyleProfile style =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            DrawSurfaceVariantPopup(style);
            DrawStyleWarnings(style);
            DrawSurfaceProfileOverride(style);
            DrawResolvedFeatureSummary();
            DrawStyleAssetDetails(style);

            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceFamilyPopup()
        {
            GroundSurfaceStyleProfile[] styles =
                LoadAvailableStyleProfiles();

            if (styles.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No GroundSurfaceStyleProfile assets were found. Create or assign a style profile before choosing a family.",
                    MessageType.Warning);

                DrawManualSurfaceStyleField();
                return;
            }

            GroundSurfaceStyleProfile current =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            int selectedIndex = 0;
            bool foundCurrent = false;

            for (int index = 0; index < styles.Length; index++)
            {
                if (styles[index] == current)
                {
                    selectedIndex = index;
                    foundCurrent = true;
                    break;
                }
            }

            if (current != null && !foundCurrent)
            {
                styles = AppendStyle(styles, current);
                selectedIndex = styles.Length - 1;
                foundCurrent = true;
            }

            GUIContent[] labels = new GUIContent[styles.Length];

            for (int index = 0; index < styles.Length; index++)
            {
                GroundSurfaceStyleProfile style = styles[index];
                labels[index] = new GUIContent(
                    style != null ? style.DisplayName : "Missing Style");
            }

            EditorGUI.showMixedValue =
                surfaceStyleProfile.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int newSelectedIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Surface Family",
                    "Top-level visual ground family. This assigns a GroundSurfaceStyleProfile asset without manual dragging."),
                Mathf.Clamp(selectedIndex, 0, labels.Length - 1),
                labels);

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                GroundSurfaceStyleProfile selectedStyle =
                    styles[Mathf.Clamp(
                        newSelectedIndex,
                        0,
                        styles.Length - 1)];

                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Select Ground Surface Family",
                    ground => ground.SetSurfaceStyleProfile(selectedStyle));
            }

            if (current == null && !surfaceStyleProfile.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "No style is currently assigned. GeneratedGround will use the first valid discovered family after validation.",
                    MessageType.Info);
            }
        }

        private void DrawManualSurfaceStyleField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                surfaceStyleProfile,
                new GUIContent(
                    "Surface Style Profile",
                    "Manual style asset fallback. Normal authoring should use the Surface Family dropdown when profiles are discoverable."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Ground Surface Style",
                    ground => ground.RefreshSurfaceStyleState());
            }
        }

        private void DrawStyleAssetDetails(
            GroundSurfaceStyleProfile style)
        {
            showStyleAssetDetails = EditorGUILayout.Foldout(
                showStyleAssetDetails,
                "Advanced Style Asset",
                true);

            if (!showStyleAssetDetails)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                surfaceStyleProfile,
                new GUIContent(
                    "Style Asset",
                    "Direct asset reference for custom or externally stored style profiles."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Ground Surface Style",
                    ground => ground.RefreshSurfaceStyleState());
            }

            if (style != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent(
                            "Resolved Style Asset",
                            "The asset currently driving family and variant options."),
                        style,
                        typeof(GroundSurfaceStyleProfile),
                        false);
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawStyleWarnings(
            GroundSurfaceStyleProfile style)
        {
            if (style == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing surface family. Assign or create a GroundSurfaceStyleProfile asset.",
                    MessageType.Warning);
                return;
            }

            if (style.DefaultSurfaceProfile == null)
            {
                EditorGUILayout.HelpBox(
                    "The selected surface family has no default GroundSurfaceProfile. Generation will fall back to the local override/profile if available.",
                    MessageType.Warning);
            }

            if (style.Variants == null || style.Variants.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected surface family has no variants.",
                    MessageType.Warning);
                return;
            }

            bool selectedVariantFound = false;
            string currentId = surfaceVariantId.stringValue;

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                if (variant.Id == currentId)
                {
                    selectedVariantFound = true;
                    break;
                }
            }

            if (!selectedVariantFound &&
                !surfaceVariantId.hasMultipleDifferentValues)
            {
                EditorGUILayout.HelpBox(
                    "The stored variant id is not present in the selected family. The first valid variant will be used after validation.",
                    MessageType.Warning);
            }

            string duplicateId = FindDuplicateVariantId(style);

            if (!string.IsNullOrWhiteSpace(duplicateId))
            {
                EditorGUILayout.HelpBox(
                    $"The selected family contains duplicate variant id '{duplicateId}'. Variant ids must be stable and unique.",
                    MessageType.Warning);
            }
        }

        private void DrawGroundDebugSection()
        {
            if (!DrawSectionFoldout(
                    ref showGroundDebug,
                    "Ground Debug"))
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Ground debug views are applied through this GeneratedGround object's MaterialPropertyBlock. They do not require editing shared material assets and do not regenerate terrain.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                debugView,
                new GUIContent(
                    "Debug View",
                    "Renderer-local generated-ground debug view. Use None for normal rendering."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Change Generated Ground Debug View",
                    ground => ground.RefreshSurfaceMaterialProperties());
            }

            using (new EditorGUI.DisabledScope(
                       !debugView.hasMultipleDifferentValues &&
                       debugView.enumValueIndex == 0))
            {
                if (GUILayout.Button("Clear Debug View"))
                {
                    serializedObject.ApplyModifiedProperties();
                    ApplyToTargets(
                        "Clear Generated Ground Debug View",
                        ground => ground.ClearDebugView());
                }
            }

            EditorGUI.indentLevel--;
        }

        private void DrawRegenerationAccountingSection()
        {
            if (!DrawSectionFoldout(
                    ref showRegenerationAccounting,
                    "Editor Regeneration Accounting"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            if (targets.Length != 1 || target is not GeneratedGround ground)
            {
                EditorGUILayout.HelpBox(
                    "Select one GeneratedGround to inspect or copy its latest accounting batch.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.HelpBox(
                ground.LastEditorRegenerationAccountingReport,
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Latest Batch"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    ground.LastEditorRegenerationAccountingReport;
            }
            if (GUILayout.Button("Clear"))
            {
                ground.ClearEditorRegenerationAccounting();
                Repaint();
            }
            if (GUILayout.Button("Log Next Batch Once"))
            {
                ground.LogNextEditorRegenerationBatchOnce();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Last Regeneration Stage Timing",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.LastRegenerationTimingDiagnostics,
                MessageType.None);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Copy Stage Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildRegenerationTimingClipboardReport(ground);
            }
            if (GUILayout.Button("Copy Accounting + Timing"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildCombinedGroundDiagnosticsClipboardReport(ground);
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Copy All Ground Reports"))
            {
                EditorGUIUtility.systemCopyBuffer =
                    BuildAllGroundDiagnosticsClipboardReport(ground);
            }

            EditorGUILayout.LabelField(
                "The accounting record is Editor-only and observational. The timing report retains the latest regeneration stage breakdown recorded by GeneratedGround.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUI.indentLevel--;
        }

        private static string BuildRegenerationTimingClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Full Generation Report\n" +
                ground.LastRegenerationTimingDiagnostics;
        }

        private static string BuildCombinedGroundDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround regeneration accounting\n" +
                ground.LastEditorRegenerationAccountingReport +
                "\n\n" +
                BuildRegenerationTimingClipboardReport(ground);
        }

        private static string BuildPaintedAccentPlacementClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Placement Report\n" +
                ground.GetLastPaintedAccentPlacementStatistics();
        }

        private static string BuildPaintedAccentCoverageClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Coverage Report\n" +
                ground.GetLastPaintedAccentCoverageStatistics();
        }

        private static string BuildPaintedAccentProjectedGlyphClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround Painted Accent Projected Baseline Report\n" +
                ground.GetLastPaintedAccentProjectedGlyphStatistics();
        }

        private static string BuildPaintedAccentGenerationDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                BuildRegenerationTimingClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentPlacementClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentCoverageClipboardReport(ground) +
                "\n\n" +
                BuildPaintedAccentProjectedGlyphClipboardReport(ground);
        }

        private static string BuildAllGroundDiagnosticsClipboardReport(
            GeneratedGround ground)
        {
            return
                "GeneratedGround regeneration accounting\n" +
                ground.LastEditorRegenerationAccountingReport +
                "\n\n" +
                BuildPaintedAccentGenerationDiagnosticsClipboardReport(ground);
        }

        private void DrawPaintedAccentStrokeControls()
        {
            if (targets.Length != 1)
            {
                return;
            }

            GroundSurfaceStyleProfile style =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            if (style == null ||
                string.IsNullOrWhiteSpace(surfaceVariantId.stringValue))
            {
                return;
            }

            SerializedObject styleObject = new SerializedObject(style);
            styleObject.Update();

            SerializedProperty feature =
                FindSelectedPaintedAccentFeatureProperty(
                    styleObject,
                    surfaceVariantId.stringValue);

            if (feature == null)
            {
                return;
            }

            SerializedProperty strokeWidth =
                feature.FindPropertyRelative("paintedAccentStrokeWidth");
            SerializedProperty strokeDensity =
                feature.FindPropertyRelative("paintedAccentStrokeDensity");
            SerializedProperty distributionPatchScale =
                feature.FindPropertyRelative("paintedAccentDistributionPatchScale");
            SerializedProperty distributionPatchiness =
                feature.FindPropertyRelative("paintedAccentDistributionPatchiness");
            SerializedProperty horizontalCompanionStrength =
                feature.FindPropertyRelative("paintedAccentHorizontalCompanionStrength");
            SerializedProperty companionTripletShare =
                feature.FindPropertyRelative("paintedAccentCompanionTripletShare");
            SerializedProperty companionAccentBias =
                feature.FindPropertyRelative("paintedAccentCompanionAccentBias");
            SerializedProperty companionTightness =
                feature.FindPropertyRelative("paintedAccentCompanionTightness");
            SerializedProperty companionTripletVerticality =
                feature.FindPropertyRelative("paintedAccentCompanionTripletVerticality");
            SerializedProperty companionTripletVerticalityInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionTripletVerticalityInitialized");
            SerializedProperty horizontalCompanionsInitialized =
                feature.FindPropertyRelative("paintedAccentHorizontalCompanionsInitialized");
            SerializedProperty companionQuotaControlsInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionQuotaControlsInitialized");
            SerializedProperty pairSteppedWeight =
                feature.FindPropertyRelative("paintedAccentPairSteppedWeight");
            SerializedProperty pairShoulderWeight =
                feature.FindPropertyRelative("paintedAccentPairShoulderWeight");
            SerializedProperty pairOffsetWeight =
                feature.FindPropertyRelative("paintedAccentPairOffsetWeight");
            SerializedProperty pairShallowWeight =
                feature.FindPropertyRelative("paintedAccentPairShallowWeight");
            SerializedProperty tripletSteppedRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletSteppedRunWeight");
            SerializedProperty tripletCrownRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletCrownRunWeight");
            SerializedProperty tripletBrokenTerraceWeight =
                feature.FindPropertyRelative("paintedAccentTripletBrokenTerraceWeight");
            SerializedProperty tripletShallowRunWeight =
                feature.FindPropertyRelative("paintedAccentTripletShallowRunWeight");
            SerializedProperty companionLayoutWeightsInitialized =
                feature.FindPropertyRelative("paintedAccentCompanionLayoutWeightsInitialized");
            SerializedProperty completeMoundWeight =
                feature.FindPropertyRelative("paintedAccentCompleteMoundWeight");
            SerializedProperty asymmetricMoundWeight =
                feature.FindPropertyRelative("paintedAccentAsymmetricMoundWeight");
            SerializedProperty singleShoulderWeight =
                feature.FindPropertyRelative("paintedAccentSingleShoulderWeight");
            SerializedProperty shallowCrestWeight =
                feature.FindPropertyRelative("paintedAccentShallowCrestWeight");
            SerializedProperty familyWeightsInitialized =
                feature.FindPropertyRelative("paintedAccentGlyphFamilyWeightsInitialized");
            SerializedProperty strokeLengthMin =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMin");
            SerializedProperty strokeLengthMax =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMax");
            SerializedProperty strokeFacingDirectionDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeFacingDirectionDegrees");
            SerializedProperty strokeAngleJitterDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeAngleJitterDegrees");
            SerializedProperty strokePathWiggle =
                feature.FindPropertyRelative("paintedAccentStrokePathWiggle");
            SerializedProperty strokePathWiggleInitialized =
                feature.FindPropertyRelative("paintedAccentStrokePathWiggleInitialized");
            SerializedProperty foldHeight =
                feature.FindPropertyRelative("paintedAccentFoldHeight");
            SerializedProperty crestCrownHeight =
                feature.FindPropertyRelative("paintedAccentCrestCrownHeight");
            SerializedProperty foldIrregularity =
                feature.FindPropertyRelative("paintedAccentFoldIrregularity");
            SerializedProperty foldEndTaper =
                feature.FindPropertyRelative("paintedAccentFoldEndTaper");
            SerializedProperty inkColor =
                feature.FindPropertyRelative("paintedAccentInkColor");

            if (strokeWidth == null ||
                strokeDensity == null ||
                distributionPatchScale == null ||
                distributionPatchiness == null ||
                horizontalCompanionStrength == null ||
                companionTripletShare == null ||
                companionAccentBias == null ||
                companionTightness == null ||
                companionTripletVerticality == null ||
                companionTripletVerticalityInitialized == null ||
                horizontalCompanionsInitialized == null ||
                companionQuotaControlsInitialized == null ||
                pairSteppedWeight == null ||
                pairShoulderWeight == null ||
                pairOffsetWeight == null ||
                pairShallowWeight == null ||
                tripletSteppedRunWeight == null ||
                tripletCrownRunWeight == null ||
                tripletBrokenTerraceWeight == null ||
                tripletShallowRunWeight == null ||
                companionLayoutWeightsInitialized == null ||
                completeMoundWeight == null ||
                asymmetricMoundWeight == null ||
                singleShoulderWeight == null ||
                shallowCrestWeight == null ||
                familyWeightsInitialized == null ||
                strokeLengthMin == null ||
                strokeLengthMax == null ||
                strokeFacingDirectionDegrees == null ||
                strokeAngleJitterDegrees == null ||
                strokePathWiggle == null ||
                strokePathWiggleInitialized == null ||
                foldHeight == null ||
                crestCrownHeight == null ||
                foldIrregularity == null ||
                foldEndTaper == null ||
                inkColor == null)
            {
                return;
            }

            bool styleChanged = false;

            if (!horizontalCompanionsInitialized.boolValue)
            {
                horizontalCompanionStrength.floatValue = 0f;
                companionTightness.floatValue = 0.65f;
                horizontalCompanionsInitialized.boolValue = true;
                styleChanged = true;
            }

            if (!companionTripletVerticalityInitialized.boolValue)
            {
                companionTripletVerticality.floatValue = 1f;
                companionTripletVerticalityInitialized.boolValue = true;
                styleChanged = true;
            }

            if (!companionQuotaControlsInitialized.boolValue)
            {
                companionTripletShare.floatValue = 0.45f;
                companionAccentBias.floatValue = 0.65f;
                companionQuotaControlsInitialized.boolValue = true;
                styleChanged = true;
            }

            if (!companionLayoutWeightsInitialized.boolValue)
            {
                pairSteppedWeight.floatValue = 0.45f;
                pairShoulderWeight.floatValue = 0.30f;
                pairOffsetWeight.floatValue = 0.20f;
                pairShallowWeight.floatValue = 0.05f;
                tripletSteppedRunWeight.floatValue = 0.40f;
                tripletCrownRunWeight.floatValue = 0.30f;
                tripletBrokenTerraceWeight.floatValue = 0.25f;
                tripletShallowRunWeight.floatValue = 0.05f;
                companionLayoutWeightsInitialized.boolValue = true;
                styleChanged = true;
            }

            if (!familyWeightsInitialized.boolValue)
            {
                completeMoundWeight.floatValue = 0.20f;
                asymmetricMoundWeight.floatValue = 0.30f;
                singleShoulderWeight.floatValue = 0.30f;
                shallowCrestWeight.floatValue = 0.20f;
                familyWeightsInitialized.boolValue = true;
                styleChanged = true;
            }

            if (!strokePathWiggleInitialized.boolValue)
            {
                strokePathWiggle.floatValue = 0.35f;
                strokePathWiggleInitialized.boolValue = true;
                styleChanged = true;
            }

            bool expanded = DrawSectionFoldout(
                ref showPaintedAccentStrokes,
                "Painted Accent Strokes");

            if (expanded)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.HelpBox(
                    "Edits placement descriptors, projected contour families, horizontal companion composition, and authored ink. Production glyphs remain mesh-free and bake into the shared R8 coverage texture.",
                    MessageType.None);

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentBasics,
                        "Stroke Basics"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        strokeWidth,
                        0.002f,
                        0.20f,
                        new GUIContent(
                            "Stroke Width",
                            "Visible authored projected-contour width in metres. BodyWidth remains texture/debug support only."));
                    EditorGUILayout.Slider(
                        strokeDensity,
                        0f,
                        2000f,
                        new GUIContent(
                            "Stroke Density",
                            "Approximate requested stroke proposals per standard 40x40 ground patch. Regional concentration redistributes a fixed average share of this population; physical validation may reduce the final count."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentDistribution,
                        "Distribution"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "Scale controls the size of sparse/dense structure. Contrast controls how strongly the field separates into populated and quiet areas. Cluster Region Bias only decides where the fixed companion quota is concentrated.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        distributionPatchScale,
                        2f,
                        24f,
                        new GUIContent(
                            "Distribution Scale",
                            "Lower values create smaller, more frequent variation. Higher values create broader local patches and larger coherent regions."));
                    EditorGUILayout.Slider(
                        distributionPatchiness,
                        0f,
                        1f,
                        new GUIContent(
                            "Distribution Contrast",
                            "Zero approaches an even field. One creates strong sparse-versus-dense separation while retaining a protected non-zero sparse-region floor."));
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUILayout.Slider(
                            companionAccentBias,
                            0f,
                            1f,
                            new GUIContent(
                                "Cluster Region Bias",
                                "Zero distributes clusters like the overall field. One concentrates the same fixed cluster quota into denser accent regions."));
                    }
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentHorizontalCompanions,
                        "Companion Composition"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "Participation, pair/triplet split, and layout weights resolve to deterministic whole-mark quotas after ordinary projected validation. Shape controls cannot silently reduce those quotas.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        horizontalCompanionStrength,
                        0f,
                        1f,
                        new GUIContent(
                            "Companion Participation",
                            "Authoritative target share of final valid projected marks assigned to complete pairs or triplets."));
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUILayout.Slider(
                            companionTripletShare,
                            0f,
                            1f,
                            new GUIContent(
                                "Triplet Share",
                                "Of clustered participants, the authoritative target share assigned to triplets. The remainder is assigned to pairs."));
                        EditorGUILayout.Slider(
                            companionTightness,
                            0f,
                            1f,
                            new GUIContent(
                                "Companion Tightness",
                                "Junction spacing only. One stops terminal endpoints at the visible edge of the contacted mark without overlap."));
                        EditorGUILayout.Slider(
                            companionTripletVerticality,
                            0f,
                            1f,
                            new GUIContent(
                                "Cluster Verticality",
                                "Translation-driven stepping for pairs and triplets. This does not change cluster counts or Angle Jitter."));
                    }
                    styleChanged |= EditorGUI.EndChangeCheck();
                    if (!serializedObject.isEditingMultipleObjects &&
                        target is GeneratedGround generatedGround)
                    {
                        EditorGUILayout.HelpBox(
                            generatedGround.GetLastPaintedAccentCompanionQuotaSummary(),
                            MessageType.None);
                    }
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentAdvancedCompanionLayoutMix,
                        "Advanced Companion Layout Mix"))
                {
                    EditorGUI.indentLevel++;
                    using (new EditorGUI.DisabledScope(
                               !horizontalCompanionStrength.hasMultipleDifferentValues &&
                               horizontalCompanionStrength.floatValue <= 0f))
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.LabelField("Pair Layout Weights", EditorStyles.miniBoldLabel);
                        EditorGUILayout.Slider(pairSteppedWeight, 0f, 1f, new GUIContent("Stepped", "Exact normalized quota weight for stepped pairs."));
                        EditorGUILayout.Slider(pairShoulderWeight, 0f, 1f, new GUIContent("Shoulder", "Exact normalized quota weight for shoulder/interior-contact pairs."));
                        EditorGUILayout.Slider(pairOffsetWeight, 0f, 1f, new GUIContent("Offset", "Exact normalized quota weight for offset pairs."));
                        EditorGUILayout.Slider(pairShallowWeight, 0f, 1f, new GUIContent("Shallow Offset", "Exact normalized quota weight for quieter visibly separated pairs."));
                        EditorGUILayout.LabelField("Triplet Layout Weights", EditorStyles.miniBoldLabel);
                        EditorGUILayout.Slider(tripletSteppedRunWeight, 0f, 1f, new GUIContent("Stepped Run", "Exact normalized quota weight for rising/falling stepped runs."));
                        EditorGUILayout.Slider(tripletCrownRunWeight, 0f, 1f, new GUIContent("Crown Run", "Exact normalized quota weight for centre-raised/lowered triplets."));
                        EditorGUILayout.Slider(tripletBrokenTerraceWeight, 0f, 1f, new GUIContent("Broken Terrace", "Exact normalized quota weight for alternating terrace triplets."));
                        EditorGUILayout.Slider(tripletShallowRunWeight, 0f, 1f, new GUIContent("Shallow Run", "Exact normalized quota weight for quieter non-collinear triplets."));
                        styleChanged |= EditorGUI.EndChangeCheck();
                    }
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentFamilyMix,
                        "Glyph Family Mix"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        completeMoundWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Complete Mound Weight",
                            "Relative weight for the accepted two-sided mound family. Values are normalized against the other family weights."));
                    EditorGUILayout.Slider(
                        asymmetricMoundWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Asymmetric Mound Weight",
                            "Relative weight for strongly unequal two-sided mound silhouettes. Values are normalized internally."));
                    EditorGUILayout.Slider(
                        singleShoulderWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Single Shoulder Weight",
                            "Relative weight for open one-sided shoulder silhouettes. Values are normalized internally."));
                    EditorGUILayout.Slider(
                        shallowCrestWeight,
                        0f,
                        1f,
                        new GUIContent(
                            "Shallow Crest Weight",
                            "Relative weight for low predominantly lateral crest silhouettes. Values are normalized internally."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentGeometry,
                        "Stroke Geometry"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        strokeLengthMin,
                        0.20f,
                        4.0f,
                        new GUIContent(
                            "Stroke Length Min",
                            "Minimum accepted ground-surface descriptor length in metres."));
                    EditorGUILayout.Slider(
                        strokeLengthMax,
                        0.25f,
                        6.0f,
                        new GUIContent(
                            "Stroke Length Max",
                            "Maximum accepted ground-surface descriptor length in metres."));
                    EditorGUILayout.Slider(
                        strokeFacingDirectionDegrees,
                        0f,
                        360f,
                        new GUIContent(
                            "Facing Direction Degrees",
                            "Local X/Z orientation reference. Accepted descriptor strokes are perpendicular to this direction before signed Angle Jitter is applied."));
                    EditorGUILayout.Slider(
                        strokeAngleJitterDegrees,
                        0f,
                        30f,
                        new GUIContent(
                            "Angle Jitter Degrees",
                            "Maximum signed angle offset around the perpendicular stroke angle."));
                    EditorGUILayout.Slider(
                        strokePathWiggle,
                        0f,
                        1f,
                        new GUIContent(
                            "Stroke Path Wiggle",
                            "Smooth lateral curvature of the ground-surface stroke path. This does not alter Profile Irregularity or family height."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentProfile,
                        "Projected Contour Profile"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        "The mesh-free projected contour applies its solved scalar profile toward fixed world +Z, which is permanent gameplay screen-up.",
                        MessageType.None);
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.Slider(
                        foldHeight,
                        0f,
                        0.50f,
                        new GUIContent(
                            "Profile Height",
                            "Primary projected contour amplitude in metres, applied toward fixed world +Z."));
                    EditorGUILayout.Slider(
                        crestCrownHeight,
                        0f,
                        0.05f,
                        new GUIContent(
                            "Crest Crown Height",
                            "Additional projected crest/cap amplitude added directly to fixed world +Z displacement."));
                    EditorGUILayout.Slider(
                        foldIrregularity,
                        0f,
                        1f,
                        new GUIContent(
                            "Profile Irregularity",
                            "Seeded longitudinal variation in the projected contour silhouette."));
                    EditorGUILayout.Slider(
                        foldEndTaper,
                        0f,
                        1f,
                        new GUIContent(
                            "End Taper",
                            "Projected contour and visible-width endpoint envelope."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                if (DrawSubsectionFoldout(
                        ref showPaintedAccentInk,
                        "Authored Ink"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(
                        inkColor,
                        new GUIContent(
                            "Ink Color",
                            "Family/variant-authored opaque ink colour blended through the generated projected coverage texture into ground albedo."));
                    styleChanged |= EditorGUI.EndChangeCheck();
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            if (strokeLengthMax.floatValue < strokeLengthMin.floatValue + 0.05f)
            {
                strokeLengthMax.floatValue = strokeLengthMin.floatValue + 0.05f;
                styleChanged = true;
            }

            if (styleChanged)
            {
                styleObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(style);
                paintedAccentPlacementDebugSignature = int.MinValue;
                ApplyToTargets(
                    "Tune Painted Accent Distribution, Companions, Families, Profile, and Ink",
                    ground => ground.RefreshSurfaceMaterialProperties());
            }

            DrawPaintedAccentPlacementDebugControls();
        }

        private void DrawPaintedAccentPlacementDebugControls()
        {
            if (!DrawSectionFoldout(
                    ref showPaintedAccentPlacementDebug,
                    "Painted Accent Placement Debug",
                    4f))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Editor-only Scene view overlays and read-only generation diagnostics. These controls do not change production coverage.",
                MessageType.None);

            bool debugChanged = false;

            if (DrawSubsectionFoldout(
                    ref showPaintedAccentPlacementOverlays,
                    "Placement and Composition Overlays"))
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    paintedAccentPlacementOverlayWeight,
                    new GUIContent(
                        "Overlay Weight",
                        "Patch Preference displays only continuous patch weight. Effective Proposal Weight also includes semantic support."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentDistributionOverlay,
                    new GUIContent(
                        "Show Distribution Overlay",
                        "Displays a filled-cell heatmap of the continuous patch-weight field."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentWeightedProposals,
                    new GUIContent(
                        "Show Weighted Proposals",
                        "Displays weighted proposal centres before physical rejection."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentLastAcceptedPositions,
                    new GUIContent(
                        "Show Last Accepted Positions",
                        "Displays accepted stroke centres from the most recent placement generation."));
                EditorGUILayout.PropertyField(
                    showPaintedAccentCompositionDebug,
                    new GUIContent(
                        "Show Composition Debug",
                        "Displays region modes, directions, thinning survival, mark roles, and selected glyph families."));
                debugChanged |= EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
            }

            if (DrawSubsectionFoldout(
                    ref showPaintedAccentShapeOverlay,
                    "Projected Shape Overlay"))
            {
                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    showPaintedAccentProjectedGlyphDebug,
                    new GUIContent(
                        "Show Accepted Projected Debug",
                        "Displays accepted projected glyphs at their true positions."));
                EditorGUILayout.PropertyField(
                    paintedAccentGlyphFamilyPreview,
                    new GUIContent(
                        "Family Preview",
                        "Filters only Scene debug drawing. It never changes generation or baked production coverage."));
                debugChanged |= EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;
            }

            if (debugChanged)
            {
                serializedObject.ApplyModifiedProperties();
                paintedAccentPlacementDebugSignature = int.MinValue;
                paintedAccentPlacementDebugSnapshotBuildFailed = false;
                paintedAccentProjectedGlyphDebugSignature = int.MinValue;
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed = false;
                SceneView.RepaintAll();
            }

            if (showPaintedAccentPlacementOverlays &&
                (showPaintedAccentDistributionOverlay.boolValue ||
                 showPaintedAccentWeightedProposals.boolValue) &&
                paintedAccentPlacementDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The live Painted Accent placement snapshot could not be built. Confirm that the ground has a valid generated mesh and base-surface snapshot, then regenerate the ground.",
                    MessageType.Warning);
            }

            if (showPaintedAccentShapeOverlay &&
                showPaintedAccentProjectedGlyphDebug.boolValue &&
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The projected glyph snapshot could not be built. Confirm that Painted Accent Lines are enabled and that the ground has valid generated descriptors, then regenerate the ground.",
                    MessageType.Warning);
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                EditorGUI.indentLevel--;
                return;
            }

            if (showPaintedAccentPlacementOverlays &&
                showPaintedAccentCompositionDebug.boolValue &&
                !ground.GetLastPaintedAccentCompositionDebugSnapshot().IsValid)
            {
                EditorGUILayout.HelpBox(
                    "The composition snapshot is unavailable. Regenerate Painted Accent placement before using the composition overlay.",
                    MessageType.Warning);
            }

            if (DrawSubsectionFoldout(
                    ref showPaintedAccentDiagnostics,
                    "Last Generation Diagnostics"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(
                    "Last Regeneration Timing",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    ground.LastRegenerationTimingDiagnostics,
                    MessageType.None);
                if (GUILayout.Button("Copy Regeneration Timing"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        BuildRegenerationTimingClipboardReport(ground);
                }

                EditorGUILayout.LabelField(
                    "Last Generated",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    ground.GetLastPaintedAccentPlacementStatistics(),
                    MessageType.None);
                if (GUILayout.Button("Copy Last Generated"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        BuildPaintedAccentPlacementClipboardReport(ground);
                }

                EditorGUILayout.LabelField(
                    "Projected Coverage Bake",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    ground.GetLastPaintedAccentCoverageStatistics(),
                    MessageType.None);
                if (GUILayout.Button("Copy Projected Coverage Bake"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        BuildPaintedAccentCoverageClipboardReport(ground);
                }

                if (showPaintedAccentProjectedGlyphDebug.boolValue)
                {
                    EditorGUILayout.LabelField(
                        "Accepted Projected Baseline",
                        EditorStyles.miniBoldLabel);
                    EditorGUILayout.HelpBox(
                        ground.GetLastPaintedAccentProjectedGlyphStatistics(),
                        MessageType.None);
                    if (GUILayout.Button("Copy Accepted Projected Baseline"))
                    {
                        EditorGUIUtility.systemCopyBuffer =
                            BuildPaintedAccentProjectedGlyphClipboardReport(ground);
                    }
                }

                if (GUILayout.Button("Copy All Generation Diagnostics"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        BuildPaintedAccentGenerationDiagnosticsClipboardReport(ground);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        private static SerializedProperty FindSelectedPaintedAccentFeatureProperty(
            SerializedObject styleObject,
            string variantId)
        {
            SerializedProperty variants =
                styleObject.FindProperty("variants");

            if (variants == null || !variants.isArray)
            {
                return null;
            }

            for (int variantIndex = 0;
                 variantIndex < variants.arraySize;
                 variantIndex++)
            {
                SerializedProperty variant =
                    variants.GetArrayElementAtIndex(variantIndex);
                SerializedProperty id =
                    variant.FindPropertyRelative("id");

                if (id == null || id.stringValue != variantId)
                {
                    continue;
                }

                SerializedProperty features =
                    variant.FindPropertyRelative("features");

                if (features == null || !features.isArray)
                {
                    return null;
                }

                for (int featureIndex = 0;
                     featureIndex < features.arraySize;
                     featureIndex++)
                {
                    SerializedProperty feature =
                        features.GetArrayElementAtIndex(featureIndex);
                    SerializedProperty kind =
                        feature.FindPropertyRelative("kind");

                    if (kind != null &&
                        kind.intValue ==
                        (int)GroundSurfaceFeatureKind.PaintedAccentLines)
                    {
                        return feature;
                    }
                }

                return null;
            }

            return null;
        }

        private void DrawGenerationSection()
        {
            if (!DrawSectionFoldout(
                    ref showGeneration,
                    "Generation"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.IntSlider(
                shapeSeed,
                GroundRecipe.MinimumSeed,
                GroundRecipe.MaximumSeed,
                new GUIContent(
                    "Shape Seed",
                    "Deterministic terrain variation."));

            EditorGUILayout.PropertyField(
                regenerateOnValidate,
                new GUIContent(
                    "Live Regeneration",
                    "Regenerate when recipe values change."));
            EditorGUI.indentLevel--;
        }

        private void DrawPatchSection()
        {
            if (!DrawSectionFoldout(
                    ref showPatch,
                    "Patch"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                patchSize,
                new GUIContent("Patch Size"));

            EditorGUILayout.PropertyField(
                resolution,
                new GUIContent("Resolution"));

            GroundPatchSize selectedSize =
                (GroundPatchSize)patchSize.enumValueIndex;
            GroundResolution selectedResolution =
                (GroundResolution)resolution.enumValueIndex;
            float metres = GroundGenerator.ResolvePatchSize(selectedSize);
            int verticesPerSide =
                GroundGenerator.ResolveResolution(selectedResolution);
            int triangleCount =
                (verticesPerSide - 1) *
                (verticesPerSide - 1) *
                2;

            EditorGUILayout.HelpBox(
                $"{metres:0} × {metres:0} m, " +
                $"{verticesPerSide} × {verticesPerSide} vertices, " +
                $"{triangleCount:N0} triangles.",
                MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawTransitionSection()
        {
            if (!DrawSectionFoldout(
                    ref showTransition,
                    "Mountain Transition"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                transitionDirection,
                new GUIContent(
                    "Direction",
                    "The side toward which this patch rises."));

            using (new EditorGUI.DisabledScope(
                       transitionDirection.enumValueIndex ==
                       (int)GroundTransitionDirection.None))
            {
                EditorGUILayout.Slider(
                    transitionHeight,
                    -12f,
                    12f,
                    new GUIContent(
                        "Height Change",
                        "Metres from the low side to the high side."));
            }
            EditorGUI.indentLevel--;
        }

        private void DrawShapeSection()
        {
            if (!DrawSectionFoldout(
                    ref showShape,
                    "Ground Shape"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                profile,
                new GUIContent("Profile"));

            EditorGUILayout.Slider(
                broadForm,
                0f,
                6f,
                new GUIContent(
                    "Broad Form",
                    "Height contribution in metres."));

            EditorGUILayout.Slider(
                roughness,
                0f,
                1f,
                new GUIContent(
                    "Roughness",
                    "Controls broad and detail noise frequency."));

            EditorGUILayout.PropertyField(
                edgeBlend,
                new GUIContent(
                    "Edge Blend",
                    "Fades generated variation near patch borders."));
            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceSection()
        {
            if (!DrawSectionFoldout(
                    ref showSurface,
                    "Surface"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "Shape controls define playable height. The selected surface family and variant resolve visual recipes. This section controls generated material masks and optional local material overrides.",
                MessageType.None);

            EditorGUILayout.Slider(
                surfaceDetail,
                0f,
                1f,
                new GUIContent(
                    "Surface Detail",
                    "Restrained small-scale height variation."));

            EditorGUILayout.Slider(
                surfaceVariation,
                0f,
                1f,
                new GUIContent(
                    "Material Variation",
                    "Overall strength of generated tonal variation written to vertex colour red."));

            if (targets.Length == 1 &&
                DrawSubsectionFoldout(
                    ref showSurfaceDiagnostics,
                    "Last Surface Mask Diagnostics"))
            {
                GeneratedGround ground = target as GeneratedGround;
                if (ground != null)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.HelpBox(
                        ground.LastSurfaceMaskDiagnostics,
                        MessageType.None);
                    EditorGUI.indentLevel--;
                }
            }

            DrawMaterialOverrideControls();
            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceVariantPopup(
            GroundSurfaceStyleProfile style)
        {
            if (style == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a Surface Style Profile to choose visual variants. " +
                    "GeneratedGround will attempt to assign the Snowfield style automatically if it exists in the project.",
                    MessageType.Info);
                return;
            }

            if (style.Variants == null || style.Variants.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected Surface Style Profile has no valid variants.",
                    MessageType.Warning);
                return;
            }

            string currentId = surfaceVariantId.stringValue;
            int validCount = 0;

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant != null && variant.HasValidId)
                {
                    validCount++;
                }
            }

            if (validCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "The selected Surface Style Profile contains only empty or invalid variant ids.",
                    MessageType.Warning);
                return;
            }

            string[] ids = new string[validCount];
            GUIContent[] labels = new GUIContent[validCount];
            int writeIndex = 0;
            int selectedIndex = 0;
            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                ids[writeIndex] = variant.Id;
                labels[writeIndex] = new GUIContent(variant.DisplayName);

                if (variant.Id == currentId)
                {
                    selectedIndex = writeIndex;
                }

                writeIndex++;
            }

            EditorGUI.showMixedValue =
                surfaceVariantId.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();

            int newSelectedIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Surface Variant",
                    "Variant recipe inside the selected surface style asset."),
                selectedIndex,
                labels);

            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                string selectedId = ids[Mathf.Clamp(
                    newSelectedIndex,
                    0,
                    ids.Length - 1)];

                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Select Ground Surface Variant",
                    ground => ground.SetSurfaceVariant(selectedId));
            }
        }

        private void DrawResolvedFeatureSummary()
        {
            if (targets.Length != 1 ||
                !DrawSubsectionFoldout(
                    ref showResolvedFeatureSummary,
                    "Resolved Feature Summary"))
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                ground.ResolvedSurfaceFeatureSummary,
                MessageType.None);
            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceProfileOverride(
            GroundSurfaceStyleProfile style)
        {
            EditorGUILayout.Space(3f);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                overrideSurfaceProfile,
                new GUIContent(
                    "Override Surface Profile",
                    "Use a local semantic/mask-generation profile instead of the style profile default."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Toggle Ground Surface Profile Override",
                    ground => ground.RefreshSurfaceStyleState());
            }

            if (overrideSurfaceProfile.hasMultipleDifferentValues ||
                overrideSurfaceProfile.boolValue)
            {
                EditorGUILayout.PropertyField(
                    surfaceProfile,
                    new GUIContent(
                        "Surface Profile Override",
                        "Local semantic/mask-generation profile used by this generated ground."));
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                Object defaultProfile =
                    style != null ? style.DefaultSurfaceProfile : null;

                EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Resolved Surface Profile",
                        "Semantic/mask-generation profile inherited from the selected style."),
                    defaultProfile,
                    typeof(GroundSurfaceProfile),
                    false);
            }
        }

        private void DrawMaterialOverrideControls()
        {
            showMaterialControls = EditorGUILayout.Foldout(
                showMaterialControls,
                "Advanced Material Overrides",
                true);

            if (!showMaterialControls)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (overrideMaterialControls.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(
                    overrideMaterialControls,
                    new GUIContent("Override Material Controls"));
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                bool enabled = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Override Material Controls",
                        "Use a local material-control recipe instead of the selected style variant."),
                    overrideMaterialControls.boolValue);

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    if (enabled)
                    {
                        ApplyToTargets(
                            "Enable Ground Material Override",
                            ground => ground.EnableMaterialControlOverrideFromResolved());
                    }
                    else
                    {
                        ApplyToTargets(
                            "Disable Ground Material Override",
                            ground => ground.DisableMaterialControlOverride());
                    }
                }
            }

            if (!overrideMaterialControls.hasMultipleDifferentValues &&
                !overrideMaterialControls.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Using the selected style variant recipe. Enable Override Material Controls to create a local custom copy.",
                    MessageType.None);
                EditorGUI.indentLevel--;
                return;
            }

            bool materialChanged = false;
            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPalette,
                "Palette",
                baseColor,
                frostColor,
                dampTint,
                dampTintStrength,
                rockyDryTint,
                rockyDryTintStrength,
                vegetationTint,
                vegetationTintStrength);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialPixelVariation,
                "Pixel and Macro Variation",
                pixelCellSize,
                pixelToneCount,
                pixelClusterStrength,
                pixelVariation,
                broadVariation,
                vertexVariation,
                pixelEffectStrength,
                cellWarpStrength,
                groundMacroPatchScale);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialSemanticResponse,
                "Semantic Response",
                profileContrastScale,
                profilePixelContrastScale,
                groundSnowResponseScale,
                groundDampResponseScale,
                groundVegetationResponseScale,
                groundRockyDryResponseScale,
                groundShoreDampStrengthScale,
                groundPatchBlendStrength,
                groundSnowTintStrength,
                groundSnowBrightness,
                groundDampDarkenStrength);

            materialChanged |= DrawMaterialSubsection(
                ref showMaterialWeatherFinish,
                "Weather and Finish",
                wetness,
                wetDarkenStrength,
                wetPixelSoftening,
                wetSmoothnessBoost,
                frostStrength,
                frostContrast,
                monolithicFlatten,
                monolithicSmoothnessBoost,
                smoothness,
                specularStrength);

            if (materialChanged)
            {
                serializedObject.ApplyModifiedProperties();
                ApplyToTargets(
                    "Customize Ground Material Controls",
                    ground => ground.MarkGroundVisualControlsCustom());
            }

            EditorGUI.indentLevel--;
        }

        private static bool DrawMaterialSubsection(
            ref bool expanded,
            string label,
            params SerializedProperty[] properties)
        {
            expanded = EditorGUILayout.Foldout(
                expanded,
                label,
                true);

            if (!expanded)
            {
                return false;
            }

            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();

            for (int index = 0; index < properties.Length; index++)
            {
                SerializedProperty property = properties[index];
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property);
                }
            }

            bool changed = EditorGUI.EndChangeCheck();
            EditorGUI.indentLevel--;
            return changed;
        }

        private void DrawModifierSection()
        {
            if (!DrawSectionFoldout(
                    ref showModifiers,
                    "Modifiers"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                useModifiers,
                new GUIContent("Use Modifiers"));

            if (targets.Length == 1)
            {
                GeneratedGround ground = target as GeneratedGround;
                if (ground != null)
                {
                    EditorGUILayout.LabelField(
                        "Found Ground Modifiers",
                        ground.ModifierCount.ToString());
                    EditorGUILayout.LabelField(
                        "Found River Channels",
                        ground.RiverCount.ToString());
                }
            }

            EditorGUILayout.HelpBox(
                "GroundModifier and StylizedRiver components are discovered below this GeneratedGround object in the Hierarchy.",
                MessageType.Info);
            EditorGUI.indentLevel--;
        }

        private void DrawAdvancedSection()
        {
            if (!DrawSectionFoldout(
                    ref showAdvanced,
                    "Advanced"))
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                patchCoordinate,
                new GUIContent(
                    "Patch Coordinate",
                    "Stable noise coordinate used by future chunk assembly."));
            EditorGUI.indentLevel--;
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("New Shape"))
            {
                ApplyToTargets(
                    "New Generated Ground Shape",
                    ground => ground.CreateNewShape());
            }

            if (GUILayout.Button("Regenerate"))
            {
                ApplyToTargets(
                    "Regenerate Generated Ground",
                    ground => ground.Regenerate());
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Find Modifiers and Rivers"))
            {
                ApplyToTargets(
                    "Find Generated Ground Modifiers",
                    ground =>
                    {
                        ground.RefreshModifiers();
                        ground.Regenerate();
                    });
            }

        }

        private void OnSceneGUI()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            bool showDistribution =
                ground.ShowPaintedAccentDistributionOverlay;
            bool showProposals =
                ground.ShowPaintedAccentWeightedProposals;
            bool showAccepted =
                ground.ShowPaintedAccentLastAcceptedPositions;
            bool showComposition =
                ground.ShowPaintedAccentCompositionDebug;
            bool showProjectedGlyphs =
                ground.ShowPaintedAccentProjectedGlyphDebug;
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode =
                ground.PaintedAccentPlacementOverlayWeight;

            if (!showDistribution &&
                !showProposals &&
                !showAccepted &&
                !showComposition &&
                !showProjectedGlyphs)
            {
                return;
            }

            if (showDistribution || showProposals)
            {
                int signature =
                    ground.CalculatePaintedAccentPlacementDebugSignature();

                if (signature != paintedAccentPlacementDebugSignature)
                {
                    paintedAccentPlacementDebugSignature = signature;
                    bool built =
                        ground.TryBuildPaintedAccentPlacementDebugSnapshot(
                            out paintedAccentPlacementDebugSnapshot);
                    paintedAccentPlacementDebugSnapshotBuildFailed = !built;

                    if (!built)
                    {
                        paintedAccentPlacementDebugSnapshot =
                            GroundPaintedAccentPlacementDebugSnapshot.Empty;
                    }

                    Repaint();
                }
            }

            if (showProjectedGlyphs)
            {
                int projectedSignature =
                    ground.CalculatePaintedAccentProjectedGlyphDebugSignature();

                if (projectedSignature !=
                        paintedAccentProjectedGlyphDebugSignature ||
                    paintedAccentProjectedGlyphDebugSnapshotBuildFailed)
                {
                    paintedAccentProjectedGlyphDebugSignature =
                        projectedSignature;
                    bool built =
                        ground.TryBuildPaintedAccentProjectedGlyphDebugSnapshot(
                            out paintedAccentProjectedGlyphDebugSnapshot);
                    paintedAccentProjectedGlyphDebugSnapshotBuildFailed = !built;

                    if (!built)
                    {
                        paintedAccentProjectedGlyphDebugSnapshot =
                            GroundPaintedAccentProjectedGlyphDebugSnapshot.Empty;
                    }

                    Repaint();
                }
            }

            Vector3[] acceptedLocalPositions =
                showAccepted
                    ? ground.GetLastPaintedAccentAcceptedLocalPositions()
                    : System.Array.Empty<Vector3>();
            GroundPaintedAccentCompositionDebugSnapshot compositionSnapshot =
                showComposition
                    ? ground.GetLastPaintedAccentCompositionDebugSnapshot()
                    : GroundPaintedAccentCompositionDebugSnapshot.Empty;

            Color previousColor = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;

            if (showDistribution)
            {
                DrawPaintedAccentDistributionOverlay(
                    ground,
                    paintedAccentPlacementDebugSnapshot,
                    overlayWeightMode);
            }

            if (showProposals)
            {
                DrawPaintedAccentProposalOverlay(
                    ground,
                    paintedAccentPlacementDebugSnapshot.ProposedPoints);
            }

            if (showAccepted)
            {
                DrawPaintedAccentAcceptedOverlay(
                    ground,
                    acceptedLocalPositions);
            }

            if (showComposition)
            {
                DrawPaintedAccentCompositionOverlay(
                    ground,
                    compositionSnapshot);
            }

            if (showProjectedGlyphs)
            {
                DrawPaintedAccentProjectedGlyphOverlay(
                    ground,
                    paintedAccentProjectedGlyphDebugSnapshot);
            }

            Handles.color = previousColor;
            Handles.zTest = previousZTest;

            DrawPaintedAccentPlacementLegend(
                showDistribution,
                showProposals,
                showAccepted,
                showComposition,
                showProjectedGlyphs,
                overlayWeightMode,
                paintedAccentPlacementDebugSnapshot,
                acceptedLocalPositions,
                paintedAccentPlacementDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshot,
                compositionSnapshot);
        }

        private static void DrawPaintedAccentDistributionOverlay(
            GeneratedGround ground,
            GroundPaintedAccentPlacementDebugSnapshot snapshot,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            GroundPaintedAccentDistributionDebugSample[] samples =
                snapshot.DistributionSamples;
            int resolution = snapshot.DistributionSampleResolution;
            Transform groundTransform = ground.transform;

            for (int z = 0; z < resolution - 1; z++)
            {
                for (int x = 0; x < resolution - 1; x++)
                {
                    int i00 = z * resolution + x;
                    int i10 = i00 + 1;
                    int i01 = (z + 1) * resolution + x;
                    int i11 = i01 + 1;

                    GroundPaintedAccentDistributionDebugSample s00 =
                        samples[i00];
                    GroundPaintedAccentDistributionDebugSample s10 =
                        samples[i10];
                    GroundPaintedAccentDistributionDebugSample s11 =
                        samples[i11];
                    GroundPaintedAccentDistributionDebugSample s01 =
                        samples[i01];

                    if (!s00.IsValid ||
                        !s10.IsValid ||
                        !s11.IsValid ||
                        !s01.IsValid)
                    {
                        continue;
                    }

                    float weight =
                        (ResolvePaintedAccentDebugSampleWeight(
                             s00,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s10,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s11,
                             overlayWeightMode) +
                         ResolvePaintedAccentDebugSampleWeight(
                             s01,
                             overlayWeightMode)) * 0.25f;
                    Handles.color =
                        ResolvePaintedAccentDistributionHeatmapColor(weight);

                    Handles.DrawAAConvexPolygon(
                        groundTransform.TransformPoint(s00.LocalPosition),
                        groundTransform.TransformPoint(s10.LocalPosition),
                        groundTransform.TransformPoint(s11.LocalPosition),
                        groundTransform.TransformPoint(s01.LocalPosition));
                }
            }
        }

        private static Color ResolvePaintedAccentDistributionHeatmapColor(
            float weight)
        {
            Color sparseColor =
                new Color(0.05f, 0.22f, 1.00f, 0.22f);
            Color middleColor =
                new Color(0.05f, 0.90f, 0.82f, 0.30f);
            Color denseColor =
                new Color(1.00f, 0.18f, 0.03f, 0.52f);
            float clampedWeight = Mathf.Clamp01(weight);

            return clampedWeight < 0.5f
                ? Color.Lerp(
                    sparseColor,
                    middleColor,
                    clampedWeight * 2f)
                : Color.Lerp(
                    middleColor,
                    denseColor,
                    (clampedWeight - 0.5f) * 2f);
        }

        private static void DrawPaintedAccentProposalOverlay(
            GeneratedGround ground,
            GroundPaintedAccentProposalDebugPoint[] points)
        {
            if (points == null)
            {
                return;
            }

            Transform groundTransform = ground.transform;

            for (int index = 0; index < points.Length; index++)
            {
                GroundPaintedAccentProposalDebugPoint point = points[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(point.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.075f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;

                Handles.color = new Color(0f, 0f, 0f, 0.90f);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - forward,
                    worldPosition + forward);

                Handles.color =
                    Color.Lerp(
                        new Color(0.05f, 0.90f, 1.00f, 1.00f),
                        new Color(1.00f, 0.92f, 0.05f, 1.00f),
                        point.EffectiveProposalWeight);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - forward,
                    worldPosition + forward);
            }
        }

        private static void DrawPaintedAccentAcceptedOverlay(
            GeneratedGround ground,
            Vector3[] localPositions)
        {
            if (localPositions == null)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            Vector3 normal = groundTransform.up;

            for (int index = 0; index < localPositions.Length; index++)
            {
                Vector3 worldPosition =
                    groundTransform.TransformPoint(localPositions[index]);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.090f;

                DrawPaintedAccentAcceptedRing(
                    worldPosition,
                    normal,
                    size * 0.62f);
            }
        }

        private static void DrawPaintedAccentCompositionOverlay(
            GeneratedGround ground,
            GroundPaintedAccentCompositionDebugSnapshot snapshot)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            Vector3 groundNormal = groundTransform.up;
            GroundPaintedAccentCompositionProposalDebugPoint[] proposals =
                snapshot.Proposals;

            for (int index = 0;
                 proposals != null && index < proposals.Length;
                 index++)
            {
                GroundPaintedAccentCompositionProposalDebugPoint proposal =
                    proposals[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(proposal.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.030f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;
                Color modeColor =
                    ResolvePaintedAccentCompositionRegionColor(
                        proposal.RegionMode);
                modeColor.a = 0.92f;
                Handles.color = modeColor;
                const float lineWidth = 2.5f;
                Handles.DrawAAPolyLine(
                    lineWidth,
                    worldPosition - right,
                    worldPosition + right);
                Handles.DrawAAPolyLine(
                    lineWidth,
                    worldPosition - forward,
                    worldPosition + forward);
            }

            GroundPaintedAccentCompositionRegionDebug[] regions =
                snapshot.Regions;
            for (int index = 0;
                 regions != null && index < regions.Length;
                 index++)
            {
                GroundPaintedAccentCompositionRegionDebug region =
                    regions[index];
                if (!region.IsOccupied)
                {
                    continue;
                }

                Vector3 worldPosition =
                    groundTransform.TransformPoint(region.LocalPosition);
                Vector3 worldDirection =
                    groundTransform.TransformDirection(
                        new Vector3(
                            region.LocalDirection.x,
                            0f,
                            region.LocalDirection.y));
                if (worldDirection.sqrMagnitude <= 0.000001f)
                {
                    worldDirection = groundTransform.right;
                }
                else
                {
                    worldDirection.Normalize();
                }

                float halfLength =
                    HandleUtility.GetHandleSize(worldPosition) * 0.18f;
                Vector3 start = worldPosition - worldDirection * halfLength;
                Vector3 end = worldPosition + worldDirection * halfLength;
                Handles.color = new Color(0f, 0f, 0f, 0.90f);
                Handles.DrawAAPolyLine(5f, start, end);
                Handles.color =
                    ResolvePaintedAccentCompositionRegionColor(
                        region.RegionMode);
                Handles.DrawAAPolyLine(2.5f, start, end);
                Handles.DrawWireDisc(
                    worldPosition,
                    groundNormal,
                    halfLength * 0.22f);
            }

            GroundPaintedAccentCompositionMarkDebugPoint[] marks =
                snapshot.AcceptedMarks;
            for (int index = 0;
                 marks != null && index < marks.Length;
                 index++)
            {
                GroundPaintedAccentCompositionMarkDebugPoint mark = marks[index];
                Vector3 worldPosition =
                    groundTransform.TransformPoint(mark.LocalPosition);
                float handleSize = HandleUtility.GetHandleSize(worldPosition);
                float radius;

                switch (mark.Role)
                {
                    case GroundPaintedAccentCompositionRole.Dominant:
                        radius = handleSize * 0.065f;
                        break;
                    case GroundPaintedAccentCompositionRole.Support:
                        radius = handleSize * 0.028f;
                        break;
                    case GroundPaintedAccentCompositionRole.Standard:
                    default:
                        radius = handleSize * 0.044f;
                        break;
                }

                Handles.color = new Color(0f, 0f, 0f, 0.95f);
                Handles.DrawWireDisc(
                    worldPosition,
                    groundNormal,
                    radius * 1.28f);
                Handles.color = ResolvePaintedAccentGlyphFamilyColor(mark.Family);
                Handles.DrawWireDisc(worldPosition, groundNormal, radius);
            }
        }

        private static Color ResolvePaintedAccentCompositionRegionColor(
            GroundPaintedAccentCompositionRegionMode mode)
        {
            switch (mode)
            {
                case GroundPaintedAccentCompositionRegionMode.Quiet:
                    return new Color(0.25f, 0.55f, 1.00f, 0.95f);
                case GroundPaintedAccentCompositionRegionMode.Accent:
                    return new Color(1.00f, 0.42f, 0.06f, 0.98f);
                case GroundPaintedAccentCompositionRegionMode.Supporting:
                default:
                    return new Color(0.12f, 1.00f, 0.65f, 0.95f);
            }
        }

        private static void DrawPaintedAccentProjectedGlyphOverlay(
            GeneratedGround ground,
            GroundPaintedAccentProjectedGlyphDebugSnapshot snapshot)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            GroundPaintedAccentProjectedGlyph[] glyphs = snapshot.Glyphs;

            for (int glyphIndex = 0;
                 glyphs != null && glyphIndex < glyphs.Length;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph = glyphs[glyphIndex];
                if (!glyph.IsValid ||
                    !ShouldDrawPaintedAccentGlyphFamily(
                        ground.PaintedAccentGlyphFamilyFilter,
                        glyph.Family))
                {
                    continue;
                }

                Vector3[] localPoints = glyph.LocalSurfacePoints;
                float[] halfWidths = glyph.HalfWidths;
                Vector3[] centerWorld = new Vector3[localPoints.Length];
                Vector3[] leftWorld = new Vector3[localPoints.Length];
                Vector3[] rightWorld = new Vector3[localPoints.Length];

                for (int pointIndex = 0;
                     pointIndex < localPoints.Length;
                     pointIndex++)
                {
                    Vector3 localPoint = localPoints[pointIndex];
                    localPoint.y += 0.035f;
                    Vector2 tangent =
                        ResolvePaintedAccentProjectedGlyphTangent(
                            localPoints,
                            pointIndex);
                    Vector2 side = new Vector2(-tangent.y, tangent.x);
                    float halfWidth = Mathf.Max(0f, halfWidths[pointIndex]);
                    Vector3 leftLocal =
                        localPoint +
                        new Vector3(side.x, 0f, side.y) * halfWidth;
                    Vector3 rightLocal =
                        localPoint -
                        new Vector3(side.x, 0f, side.y) * halfWidth;

                    centerWorld[pointIndex] =
                        groundTransform.TransformPoint(localPoint);
                    leftWorld[pointIndex] =
                        groundTransform.TransformPoint(leftLocal);
                    rightWorld[pointIndex] =
                        groundTransform.TransformPoint(rightLocal);
                }

                // Use deliberately high-contrast debug colours. The ground can be
                // turquoise, pale snow, dark mud, or selection-tinted, so a single
                // bright cyan pass is not reliably legible. Draw a black outline
                // beneath the projected centreline and crest marker, then use a
                // saturated red/yellow foreground. Width boundaries remain distinct
                // dark purple so they do not visually merge with the centreline.
                Handles.color = new Color(0f, 0f, 0f, 0.98f);
                Handles.DrawAAPolyLine(6.5f, centerWorld);
                Handles.color = new Color(1.00f, 0.05f, 0.04f, 1.00f);
                Handles.DrawAAPolyLine(3.5f, centerWorld);

                Handles.color = new Color(0f, 0f, 0f, 0.88f);
                Handles.DrawAAPolyLine(3.25f, leftWorld);
                Handles.DrawAAPolyLine(3.25f, rightWorld);
                Handles.color = new Color(0.48f, 0.08f, 0.72f, 0.98f);
                Handles.DrawAAPolyLine(1.55f, leftWorld);
                Handles.DrawAAPolyLine(1.55f, rightWorld);

                int crestIndex = Mathf.Clamp(
                    Mathf.RoundToInt(glyph.CrestT * (centerWorld.Length - 1)),
                    0,
                    centerWorld.Length - 1);
                Vector3 crestWorld = centerWorld[crestIndex];
                float crestRadius =
                    HandleUtility.GetHandleSize(crestWorld) * 0.050f;
                Handles.color = new Color(0f, 0f, 0f, 0.98f);
                Handles.DrawWireDisc(
                    crestWorld,
                    groundTransform.up,
                    crestRadius * 1.45f);
                Handles.color = new Color(1.00f, 0.92f, 0.05f, 1.00f);
                Handles.DrawWireDisc(
                    crestWorld,
                    groundTransform.up,
                    crestRadius);
            }

            GroundPaintedAccentProjectedGlyphRejectionDebugPoint[] rejections =
                snapshot.Rejections;

            for (int rejectionIndex = 0;
                 rejections != null && rejectionIndex < rejections.Length;
                 rejectionIndex++)
            {
                GroundPaintedAccentProjectedGlyphRejectionDebugPoint rejection =
                    rejections[rejectionIndex];
                if (!ShouldDrawPaintedAccentGlyphFamily(
                        ground.PaintedAccentGlyphFamilyFilter,
                        rejection.Family))
                {
                    continue;
                }

                Vector3 worldPosition =
                    groundTransform.TransformPoint(rejection.LocalPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.065f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;

                Handles.color = new Color(0f, 0f, 0f, 0.92f);
                Handles.DrawAAPolyLine(5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);

                Handles.color =
                    ResolvePaintedAccentProjectedGlyphRejectionColor(
                        rejection.Reason);
                Handles.DrawAAPolyLine(2.5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(2.5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);
            }
        }

        private static bool ShouldDrawPaintedAccentGlyphFamily(
            PaintedAccentGlyphFamilyPreview preview,
            GroundPaintedAccentGlyphFamily family)
        {
            switch (preview)
            {
                case PaintedAccentGlyphFamilyPreview.CompleteMound:
                    return family == GroundPaintedAccentGlyphFamily.CompleteMound;
                case PaintedAccentGlyphFamilyPreview.AsymmetricMound:
                    return family == GroundPaintedAccentGlyphFamily.AsymmetricMound;
                case PaintedAccentGlyphFamilyPreview.SingleShoulder:
                    return family == GroundPaintedAccentGlyphFamily.SingleShoulder;
                case PaintedAccentGlyphFamilyPreview.ShallowCrest:
                    return family == GroundPaintedAccentGlyphFamily.ShallowCrest;
                case PaintedAccentGlyphFamilyPreview.All:
                default:
                    return true;
            }
        }

        private static Color ResolvePaintedAccentGlyphFamilyColor(
            GroundPaintedAccentGlyphFamily family)
        {
            switch (family)
            {
                case GroundPaintedAccentGlyphFamily.AsymmetricMound:
                    return new Color(0.20f, 0.85f, 1.00f, 1.00f);
                case GroundPaintedAccentGlyphFamily.SingleShoulder:
                    return new Color(1.00f, 0.55f, 0.12f, 1.00f);
                case GroundPaintedAccentGlyphFamily.ShallowCrest:
                    return new Color(0.38f, 1.00f, 0.42f, 1.00f);
                case GroundPaintedAccentGlyphFamily.CompleteMound:
                default:
                    return new Color(1.00f, 0.25f, 0.75f, 1.00f);
            }
        }

        private static Vector2 ResolvePaintedAccentProjectedGlyphTangent(
            Vector3[] points,
            int pointIndex)
        {
            int beforeIndex = Mathf.Max(0, pointIndex - 1);
            int afterIndex = Mathf.Min(points.Length - 1, pointIndex + 1);
            Vector2 tangent =
                new Vector2(
                    points[afterIndex].x - points[beforeIndex].x,
                    points[afterIndex].z - points[beforeIndex].z);

            return tangent.sqrMagnitude > 0.000001f
                ? tangent.normalized
                : Vector2.right;
        }

        private static Color ResolvePaintedAccentProjectedGlyphRejectionColor(
            GroundPaintedAccentProjectedGlyphRejectionReason reason)
        {
            switch (reason)
            {
                case GroundPaintedAccentProjectedGlyphRejectionReason.River:
                    return new Color(0.10f, 0.45f, 1.00f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.ModifierExclusion:
                    return new Color(1.00f, 0.90f, 0.08f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.BroadSlope:
                    return new Color(1.00f, 0.10f, 0.08f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.LocalGrade:
                    return new Color(0.75f, 0.18f, 1.00f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.FamilyShape:
                    return new Color(1.00f, 0.10f, 0.75f, 1.00f);
                // Keep the editor compatible with a runtime assembly that may still expose
                // rejection value 7 without the newer symbolic enum member during Unity's
                // incremental compile pass. The projected generator owns the stable value.
                case (GroundPaintedAccentProjectedGlyphRejectionReason)7:
                    return new Color(1.00f, 0.28f, 0.28f, 1.00f);
                case GroundPaintedAccentProjectedGlyphRejectionReason.Sampling:
                default:
                    return new Color(1.00f, 0.45f, 0.05f, 1.00f);
            }
        }

        private static float ResolvePaintedAccentDebugSampleWeight(
            GroundPaintedAccentDistributionDebugSample sample,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode)
        {
            return
                overlayWeightMode ==
                    PaintedAccentPlacementOverlayWeightMode.EffectiveProposalWeight
                    ? sample.EffectiveProposalWeight
                    : sample.PatchWeight;
        }

        private static void ResolvePaintedAccentDebugWeightStatistics(
            GroundPaintedAccentDistributionDebugSample[] samples,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode,
            out float minimum,
            out float mean,
            out float maximum)
        {
            minimum = 0f;
            mean = 0f;
            maximum = 0f;

            if (samples == null || samples.Length == 0)
            {
                return;
            }

            float minimumValue = float.PositiveInfinity;
            float maximumValue = float.NegativeInfinity;
            double total = 0.0;
            int count = 0;

            for (int index = 0; index < samples.Length; index++)
            {
                GroundPaintedAccentDistributionDebugSample sample =
                    samples[index];

                if (!sample.IsValid)
                {
                    continue;
                }

                float value =
                    ResolvePaintedAccentDebugSampleWeight(
                        sample,
                        overlayWeightMode);
                minimumValue = Mathf.Min(minimumValue, value);
                maximumValue = Mathf.Max(maximumValue, value);
                total += value;
                count++;
            }

            if (count <= 0)
            {
                return;
            }

            minimum = minimumValue;
            mean = (float)(total / count);
            maximum = maximumValue;
        }

        private static void DrawPaintedAccentAcceptedRing(
            Vector3 worldPosition,
            Vector3 normal,
            float radius)
        {
            Vector3 normalizedNormal =
                normal.sqrMagnitude > 0.0001f
                    ? normal.normalized
                    : Vector3.up;

            Handles.color = new Color(0f, 0f, 0f, 0.95f);
            Handles.DrawWireDisc(
                worldPosition,
                normalizedNormal,
                radius * 1.10f);
            Handles.color = new Color(0.16f, 1.00f, 0.24f, 0.98f);
            Handles.DrawWireDisc(
                worldPosition,
                normalizedNormal,
                radius);
        }

        private static void DrawPaintedAccentPlacementLegend(
            bool showDistribution,
            bool showProposals,
            bool showAccepted,
            bool showComposition,
            bool showProjectedGlyphs,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode,
            GroundPaintedAccentPlacementDebugSnapshot snapshot,
            Vector3[] acceptedPositions,
            bool snapshotBuildFailed,
            bool projectedSnapshotBuildFailed,
            GroundPaintedAccentProjectedGlyphDebugSnapshot projectedSnapshot,
            GroundPaintedAccentCompositionDebugSnapshot compositionSnapshot)
        {
            int validSampleCount = 0;
            GroundPaintedAccentDistributionDebugSample[] samples =
                snapshot.DistributionSamples;

            if (samples != null)
            {
                for (int index = 0; index < samples.Length; index++)
                {
                    if (samples[index].IsValid)
                    {
                        validSampleCount++;
                    }
                }
            }

            ResolvePaintedAccentDebugWeightStatistics(
                samples,
                overlayWeightMode,
                out float minimumWeight,
                out float meanWeight,
                out float maximumWeight);

            int proposedCount =
                snapshot.ProposedPoints != null
                    ? snapshot.ProposedPoints.Length
                    : 0;
            int acceptedCount =
                acceptedPositions != null
                    ? acceptedPositions.Length
                    : 0;

            System.Text.StringBuilder text =
                new System.Text.StringBuilder(320);
            text.AppendLine("Painted Accent Placement");

            if (showDistribution)
            {
                text.Append("Blue → red: ");
                text.AppendLine(
                    overlayWeightMode ==
                        PaintedAccentPlacementOverlayWeightMode.EffectiveProposalWeight
                        ? "effective proposal weight"
                        : "patch preference");
                text.Append("Weight min/mean/max: ");
                text.Append(minimumWeight.ToString("F3"));
                text.Append(" / ");
                text.Append(meanWeight.ToString("F3"));
                text.Append(" / ");
                text.AppendLine(maximumWeight.ToString("F3"));
            }

            if (showProposals)
            {
                text.AppendLine("Cyan/yellow cross: weighted proposal");
            }

            if (showAccepted)
            {
                text.AppendLine("Green ring: accepted base stroke");
            }

            if (showComposition)
            {
                text.AppendLine(
                    "Blue/green/orange crosses: quiet/supporting/accent proposals");
                text.AppendLine(
                    "Region bars: occupied-region direction");
                text.AppendLine(
                    "Ring size: dominant/standard/support; ring colour: glyph family");
                int regionCount =
                    compositionSnapshot.Regions != null
                        ? compositionSnapshot.Regions.Length
                        : 0;
                int markCount =
                    compositionSnapshot.AcceptedMarks != null
                        ? compositionSnapshot.AcceptedMarks.Length
                        : 0;
                text.Append("Composition occupied regions/marks: ");
                text.Append(regionCount);
                text.Append(" / ");
                text.AppendLine(markCount.ToString());
            }

            if (showProjectedGlyphs)
            {
                text.AppendLine("Red/purple: accepted projected glyphs");
                text.AppendLine("Yellow ring: projected peak; Family Preview filters debug only");
                GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                    projectedSnapshot.Diagnostics;
                text.Append("Projected accepted/rejected: ");
                text.Append(diagnostics.ProjectedGlyphsAccepted);
                text.Append(" / ");
                text.AppendLine(
                    diagnostics.ProjectedGlyphsRejectedTotal.ToString());
            }

            if (snapshotBuildFailed && (showDistribution || showProposals))
            {
                text.AppendLine("PLACEMENT SNAPSHOT UNAVAILABLE");
            }

            if (projectedSnapshotBuildFailed && showProjectedGlyphs)
            {
                text.AppendLine("PROJECTED GLYPH SNAPSHOT UNAVAILABLE");
            }

            text.Append("Samples: ");
            text.Append(validSampleCount);
            text.Append('/');
            text.Append(samples != null ? samples.Length : 0);
            text.Append("   Proposals: ");
            text.Append(proposedCount);
            text.Append("   Accepted: ");
            text.Append(acceptedCount);

            Handles.BeginGUI();
            float boxHeight = showProjectedGlyphs ? 210f : 124f;
            Rect boxRect = new Rect(12f, 12f, 430f, boxHeight);
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);
            GUI.Label(
                new Rect(
                    boxRect.x + 9f,
                    boxRect.y + 7f,
                    boxRect.width - 18f,
                    boxRect.height - 14f),
                text.ToString(),
                EditorStyles.wordWrappedMiniLabel);
            Handles.EndGUI();
        }

        private static GroundSurfaceStyleProfile[] LoadAvailableStyleProfiles()
        {
            string[] searchFolders = { "Assets/Game/Demo/Profiles/Ground/Styles" };
            string[] guids = AssetDatabase.FindAssets(
                "t:GroundSurfaceStyleProfile",
                searchFolders);

            if (guids == null || guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets(
                    "t:GroundSurfaceStyleProfile");
            }

            if (guids == null || guids.Length == 0)
            {
                return new GroundSurfaceStyleProfile[0];
            }

            System.Collections.Generic.List<GroundSurfaceStyleProfile> styles =
                new System.Collections.Generic.List<GroundSurfaceStyleProfile>();

            for (int index = 0; index < guids.Length; index++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[index]);
                GroundSurfaceStyleProfile style =
                    AssetDatabase.LoadAssetAtPath<GroundSurfaceStyleProfile>(
                        path);

                if (style == null || styles.Contains(style))
                {
                    continue;
                }

                styles.Add(style);
            }

            styles.Sort((left, right) =>
                string.Compare(
                    left != null ? left.DisplayName : string.Empty,
                    right != null ? right.DisplayName : string.Empty,
                    System.StringComparison.OrdinalIgnoreCase));

            return styles.ToArray();
        }

        private static GroundSurfaceStyleProfile[] AppendStyle(
            GroundSurfaceStyleProfile[] styles,
            GroundSurfaceStyleProfile style)
        {
            GroundSurfaceStyleProfile[] expanded =
                new GroundSurfaceStyleProfile[styles.Length + 1];

            for (int index = 0; index < styles.Length; index++)
            {
                expanded[index] = styles[index];
            }

            expanded[styles.Length] = style;
            return expanded;
        }

        private static string FindDuplicateVariantId(
            GroundSurfaceStyleProfile style)
        {
            if (style == null || style.Variants == null)
            {
                return null;
            }

            System.Collections.Generic.HashSet<string> seen =
                new System.Collections.Generic.HashSet<string>();

            for (int index = 0; index < style.Variants.Count; index++)
            {
                GroundSurfaceVariantRecipe variant = style.Variants[index];

                if (variant == null || !variant.HasValidId)
                {
                    continue;
                }

                if (!seen.Add(variant.Id))
                {
                    return variant.Id;
                }
            }

            return null;
        }

        private void ApplyToTargets(
            string undoName,
            GroundAction action)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                GeneratedGround ground =
                    targets[i] as GeneratedGround;

                if (ground == null)
                {
                    continue;
                }

                Undo.RecordObject(
                    ground,
                    undoName);

                action(ground);

                EditorUtility.SetDirty(ground);
            }

            serializedObject.Update();
            Repaint();
            SceneView.RepaintAll();
        }

        private delegate void GroundAction(
            GeneratedGround ground);
    }

}
