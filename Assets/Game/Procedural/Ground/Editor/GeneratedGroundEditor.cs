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
        private SerializedProperty showPaintedAccentProjectedGlyphDebug;
        private SerializedProperty showPaintedAccentContourClusterCandidateDebug;
        private SerializedProperty comparePaintedAccentProjectedGlyphAndClusterCandidate;
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
        private int paintedAccentContourClusterCandidateDebugSignature =
            int.MinValue;
        private bool
            paintedAccentContourClusterCandidateDebugSnapshotBuildFailed;
        private GroundPaintedAccentContourClusterDebugSnapshot
            paintedAccentContourClusterCandidateDebugSnapshot =
                GroundPaintedAccentContourClusterDebugSnapshot.Empty;

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

        private bool showMaterialControls;
        private bool showStyleAssetDetails;
        private bool showAdvanced;


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

            showPaintedAccentProjectedGlyphDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentProjectedGlyphDebug");

            showPaintedAccentContourClusterCandidateDebug =
                serializedObject.FindProperty(
                    "showPaintedAccentContourClusterCandidateDebug");

            comparePaintedAccentProjectedGlyphAndClusterCandidate =
                serializedObject.FindProperty(
                    "comparePaintedAccentProjectedGlyphAndClusterCandidate");

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
            EditorGUILayout.LabelField(
                "Ground Surface",
                EditorStyles.boldLabel);

            DrawSurfaceFamilyPopup();

            GroundSurfaceStyleProfile style =
                surfaceStyleProfile.objectReferenceValue as
                    GroundSurfaceStyleProfile;

            DrawSurfaceVariantPopup(style);
            DrawStyleWarnings(style);
            DrawSurfaceProfileOverride(style);
            DrawResolvedFeatureSummary();
            DrawStyleAssetDetails(style);
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
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Ground Debug",
                EditorStyles.boldLabel);

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
            SerializedProperty distributionSparseFloor =
                feature.FindPropertyRelative("paintedAccentDistributionSparseFloor");
            SerializedProperty strokeLengthMin =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMin");
            SerializedProperty strokeLengthMax =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMax");
            SerializedProperty strokeFacingDirectionDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeFacingDirectionDegrees");
            SerializedProperty strokeAngleJitterDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeAngleJitterDegrees");
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
                distributionSparseFloor == null ||
                strokeLengthMin == null ||
                strokeLengthMax == null ||
                strokeFacingDirectionDegrees == null ||
                strokeAngleJitterDegrees == null ||
                foldHeight == null ||
                crestCrownHeight == null ||
                foldIrregularity == null ||
                foldEndTaper == null ||
                inkColor == null)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Painted Accent Strokes",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Edits accepted placement descriptors, the mesh-free projected contour profile, and authored ink colour. Projected Glyph Debug applies the profile toward fixed world +Z using Scene handles only; it creates no mesh, renderer, child object, or runtime representation.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(
                strokeWidth,
                0.01f,
                0.35f,
                new GUIContent(
                    "Stroke Width",
                    "Visible authored projected-contour width in metres. BodyWidth remains texture/debug support only."));
            EditorGUILayout.Slider(
                strokeDensity,
                0f,
                240f,
                new GUIContent(
                    "Stroke Density",
                    "Approximate number of weighted stroke proposals per standard 40x40 ground patch before river, modifier, sampling, slope, and grade rejection. Final accepted count may be lower because rejected proposals are not backfilled."));
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Distribution & Placement",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.Slider(
                distributionPatchScale,
                2f,
                24f,
                new GUIContent(
                    "Distribution Patch Scale",
                    "World-space size in metres of soft continuous density patches. Larger values create broader sparse and dense regions without hard island boundaries."));
            EditorGUILayout.Slider(
                distributionPatchiness,
                0f,
                1f,
                new GUIContent(
                    "Distribution Patchiness",
                    "Strength of weighted patch placement. Zero approaches broad random coverage; one strongly prefers dense noise regions while retaining a non-zero chance elsewhere."));
            EditorGUILayout.Slider(
                distributionSparseFloor,
                0.02f,
                0.40f,
                new GUIContent(
                    "Distribution Sparse Floor",
                    "Minimum patch preference retained in cold regions before semantic weighting. Lower values create stronger sparse/dense contrast while preserving a non-zero chance outside warm patches."));
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
                    "Maximum signed angle offset in degrees around the perpendicular stroke angle derived from Facing Direction Degrees. Each stroke rolls independently between -value and +value."));
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Projected Contour Profile",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "The mesh-free projected contour applies its solved scalar profile toward fixed world +Z, which is permanent gameplay screen-up.",
                MessageType.None);
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
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Authored Ink",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                inkColor,
                new GUIContent(
                    "Ink Color",
                    "Family/variant-authored ink colour reserved for the future projected coverage bake and ground-albedo composition."));

            if (strokeLengthMax.floatValue < strokeLengthMin.floatValue + 0.05f)
            {
                strokeLengthMax.floatValue = strokeLengthMin.floatValue + 0.05f;
            }

            bool styleChanged = EditorGUI.EndChangeCheck();
            if (styleChanged)
            {
                styleObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(style);
                paintedAccentPlacementDebugSignature = int.MinValue;
                ApplyToTargets(
                    "Tune Painted Accent Distribution, Projected Profile, and Ink",
                    ground => ground.RefreshSurfaceMaterialProperties());
            }

            DrawPaintedAccentPlacementDebugControls();
        }

        private void DrawPaintedAccentPlacementDebugControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Painted Accent Placement Debug",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Editor-only Scene view overlays. Distribution uses the exact production patch-weight function; Weighted Proposals uses the exact production candidate pool and weighted selection before exclusions. Last Accepted Positions comes from the most recent placement generation.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                paintedAccentPlacementOverlayWeight,
                new GUIContent(
                    "Overlay Weight",
                    "Patch Preference displays only the continuous noise-driven patch weight. Effective Proposal Weight displays patch weight multiplied by semantic support, matching the final weight used by weighted proposal selection."));
            EditorGUILayout.PropertyField(
                showPaintedAccentDistributionOverlay,
                new GUIContent(
                    "Show Distribution Overlay",
                    "Displays a live filled-cell heatmap of the continuous patch-weight field. Cool cells are sparse preference; warm cells are dense preference."));
            EditorGUILayout.PropertyField(
                showPaintedAccentWeightedProposals,
                new GUIContent(
                    "Show Weighted Proposals",
                    "Displays the live weighted proposal centres selected before river, modifier, sampling, slope, and grade rejection."));
            EditorGUILayout.PropertyField(
                showPaintedAccentLastAcceptedPositions,
                new GUIContent(
                    "Show Last Accepted Positions",
                    "Displays accepted stroke centres from the most recent placement generation."));
            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField(
                "Painted Accent Shape Overlays",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "These three Scene-view overlays are independent and additive. Disable all three for no shape overlay, enable either true-position result by itself, enable both true-position results together, or add the paired offset comparison separately.",
                MessageType.None);
            EditorGUILayout.PropertyField(
                showPaintedAccentProjectedGlyphDebug,
                new GUIContent(
                    "Show Accepted Projected Debug",
                    "Displays only the accepted complete A6/A7 projected glyphs at their true positions. This toggle never enables the rejected A9A candidate."));
            EditorGUILayout.PropertyField(
                showPaintedAccentContourClusterCandidateDebug,
                new GUIContent(
                    "Show Rejected A9A Candidate Debug",
                    "Displays only the rejected per-descriptor A9A candidate at its true positions. It is retained temporarily for evidence and comparison while A10A is designed; cyan, green, and blue curves are primary arms, branches, and echoes."));
            EditorGUILayout.PropertyField(
                comparePaintedAccentProjectedGlyphAndClusterCandidate,
                new GUIContent(
                    "Show Paired Comparison Preview",
                    "Adds editor-offset comparison copies: the accepted glyph is placed to the visual left and the matching rejected A9A candidate to the visual right. This toggle does not force either true-position overlay on."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                paintedAccentPlacementDebugSignature = int.MinValue;
                paintedAccentPlacementDebugSnapshotBuildFailed = false;
                paintedAccentProjectedGlyphDebugSignature = int.MinValue;
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed = false;
                paintedAccentContourClusterCandidateDebugSignature =
                    int.MinValue;
                paintedAccentContourClusterCandidateDebugSnapshotBuildFailed =
                    false;
                SceneView.RepaintAll();
            }

            if ((showPaintedAccentDistributionOverlay.boolValue ||
                 showPaintedAccentWeightedProposals.boolValue) &&
                paintedAccentPlacementDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The live Painted Accent placement snapshot could not be built. Confirm that the ground has a valid generated mesh and base-surface snapshot, then regenerate the ground.",
                    MessageType.Warning);
            }

            if ((showPaintedAccentProjectedGlyphDebug.boolValue ||
                 comparePaintedAccentProjectedGlyphAndClusterCandidate.boolValue) &&
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The projected glyph snapshot could not be built. Confirm that Painted Accent Lines are enabled and that the ground has valid generated descriptors, then regenerate the ground.",
                    MessageType.Warning);
            }

            if ((showPaintedAccentContourClusterCandidateDebug.boolValue ||
                 comparePaintedAccentProjectedGlyphAndClusterCandidate.boolValue) &&
                paintedAccentContourClusterCandidateDebugSnapshotBuildFailed)
            {
                EditorGUILayout.HelpBox(
                    "The contour-cluster candidate snapshot could not be built. Confirm that Painted Accent Lines are enabled and that the ground has valid generated descriptors, then regenerate the ground.",
                    MessageType.Warning);
            }

            GeneratedGround ground = target as GeneratedGround;
            if (ground == null)
            {
                return;
            }

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Last Generated",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                ground.GetLastPaintedAccentPlacementStatistics(),
                MessageType.None);

            if (showPaintedAccentProjectedGlyphDebug.boolValue ||
                comparePaintedAccentProjectedGlyphAndClusterCandidate.boolValue)
            {
                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField(
                    "Accepted Projected Baseline",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    ground.GetLastPaintedAccentProjectedGlyphStatistics(),
                    MessageType.None);
            }

            if (showPaintedAccentContourClusterCandidateDebug.boolValue ||
                comparePaintedAccentProjectedGlyphAndClusterCandidate.boolValue)
            {
                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField(
                    "A9A Downward-Only Cluster Candidate",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.HelpBox(
                    ground.GetLastPaintedAccentContourClusterCandidateStatistics(),
                    MessageType.None);
            }
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
            EditorGUILayout.LabelField(
                "Generation",
                EditorStyles.boldLabel);

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
        }

        private void DrawPatchSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Patch",
                EditorStyles.boldLabel);

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

            float metres =
                GroundGenerator.ResolvePatchSize(
                    selectedSize);

            int verticesPerSide =
                GroundGenerator.ResolveResolution(
                    selectedResolution);

            int triangleCount =
                (verticesPerSide - 1) *
                (verticesPerSide - 1) *
                2;

            EditorGUILayout.HelpBox(
                $"{metres:0} × {metres:0} m, " +
                $"{verticesPerSide} × {verticesPerSide} vertices, " +
                $"{triangleCount:N0} triangles.",
                MessageType.None);
        }

        private void DrawTransitionSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Mountain Transition",
                EditorStyles.boldLabel);

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
        }

        private void DrawShapeSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Ground Shape",
                EditorStyles.boldLabel);

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
        }

        private void DrawSurfaceSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Surface",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Shape controls still define playable height. The selected surface family and variant resolve visual recipes at the top of the Inspector. This section controls the generated material masks: R tonal variation, G exposure, B damp/deposit, A vegetation suitability.",
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

            if (targets.Length == 1)
            {
                GeneratedGround ground =
                    target as GeneratedGround;

                if (ground != null)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.HelpBox(
                        ground.LastSurfaceMaskDiagnostics,
                        MessageType.None);
                }
            }

            DrawMaterialOverrideControls();
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
            if (targets.Length != 1)
            {
                return;
            }

            GeneratedGround ground = target as GeneratedGround;

            if (ground == null)
            {
                return;
            }

            EditorGUILayout.HelpBox(
                ground.ResolvedSurfaceFeatureSummary,
                MessageType.None);
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
            showMaterialControls =
                EditorGUILayout.Foldout(
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

            EditorGUI.BeginChangeCheck();

            DrawMaterialSubsection(
                "Palette",
                baseColor,
                frostColor,
                dampTint,
                dampTintStrength,
                rockyDryTint,
                rockyDryTintStrength,
                vegetationTint,
                vegetationTintStrength);

            DrawMaterialSubsection(
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

            DrawMaterialSubsection(
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

            DrawMaterialSubsection(
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

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                ApplyToTargets(
                    "Customize Ground Material Controls",
                    ground => ground.MarkGroundVisualControlsCustom());
            }

            EditorGUI.indentLevel--;
        }

        private static void DrawMaterialSubsection(
            string label,
            params SerializedProperty[] properties)
        {
            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                label,
                EditorStyles.miniBoldLabel);

            EditorGUI.indentLevel++;

            for (int i = 0; i < properties.Length; i++)
            {
                SerializedProperty property = properties[i];

                if (property == null)
                {
                    continue;
                }

                EditorGUILayout.PropertyField(property);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawModifierSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Modifiers",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                useModifiers,
                new GUIContent("Use Modifiers"));

            if (targets.Length == 1)
            {
                GeneratedGround ground =
                    target as GeneratedGround;

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
                "GroundModifier and StylizedRiver components are discovered " +
                "below this GeneratedGround object in the Hierarchy.",
                MessageType.Info);
        }

        private void DrawAdvancedSection()
        {
            EditorGUILayout.Space(8f);

            showAdvanced =
                EditorGUILayout.Foldout(
                    showAdvanced,
                    "Advanced",
                    true);

            if (!showAdvanced)
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
            bool showProjectedGlyphs =
                ground.ShowPaintedAccentProjectedGlyphDebug;
            bool showClusterCandidates =
                ground.ShowPaintedAccentContourClusterCandidateDebug;
            bool compareProjectedAndCandidate =
                ground.ComparePaintedAccentProjectedGlyphAndClusterCandidate;
            bool needsProjectedSnapshot =
                showProjectedGlyphs || compareProjectedAndCandidate;
            bool needsCandidateSnapshot =
                showClusterCandidates || compareProjectedAndCandidate;
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode =
                ground.PaintedAccentPlacementOverlayWeight;

            if (!showDistribution &&
                !showProposals &&
                !showAccepted &&
                !needsProjectedSnapshot &&
                !needsCandidateSnapshot)
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

            if (needsProjectedSnapshot)
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

            if (needsCandidateSnapshot)
            {
                int candidateSignature =
                    ground.CalculatePaintedAccentContourClusterCandidateDebugSignature();

                if (candidateSignature !=
                        paintedAccentContourClusterCandidateDebugSignature ||
                    paintedAccentContourClusterCandidateDebugSnapshotBuildFailed)
                {
                    paintedAccentContourClusterCandidateDebugSignature =
                        candidateSignature;
                    bool built =
                        ground.TryBuildPaintedAccentContourClusterCandidateDebugSnapshot(
                            out paintedAccentContourClusterCandidateDebugSnapshot);
                    paintedAccentContourClusterCandidateDebugSnapshotBuildFailed =
                        !built;

                    if (!built)
                    {
                        paintedAccentContourClusterCandidateDebugSnapshot =
                            GroundPaintedAccentContourClusterDebugSnapshot.Empty;
                    }

                    Repaint();
                }
            }

            Vector3[] acceptedLocalPositions =
                showAccepted
                    ? ground.GetLastPaintedAccentAcceptedLocalPositions()
                    : System.Array.Empty<Vector3>();

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

            if (showProjectedGlyphs)
            {
                DrawPaintedAccentProjectedGlyphOverlay(
                    ground,
                    paintedAccentProjectedGlyphDebugSnapshot);
            }

            if (showClusterCandidates)
            {
                DrawPaintedAccentContourClusterCandidateOverlay(
                    ground,
                    paintedAccentContourClusterCandidateDebugSnapshot,
                    Vector3.zero,
                    drawRejections: true);
            }

            if (compareProjectedAndCandidate)
            {
                DrawPaintedAccentProjectedGlyphAndClusterCandidateComparison(
                    ground,
                    paintedAccentProjectedGlyphDebugSnapshot,
                    paintedAccentContourClusterCandidateDebugSnapshot);
            }

            Handles.color = previousColor;
            Handles.zTest = previousZTest;

            DrawPaintedAccentPlacementLegend(
                showDistribution,
                showProposals,
                showAccepted,
                showProjectedGlyphs,
                showClusterCandidates,
                compareProjectedAndCandidate,
                overlayWeightMode,
                paintedAccentPlacementDebugSnapshot,
                acceptedLocalPositions,
                paintedAccentPlacementDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshotBuildFailed,
                paintedAccentContourClusterCandidateDebugSnapshotBuildFailed,
                paintedAccentProjectedGlyphDebugSnapshot,
                paintedAccentContourClusterCandidateDebugSnapshot);
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
                if (!glyph.IsValid)
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

        private static void DrawPaintedAccentContourClusterCandidateOverlay(
            GeneratedGround ground,
            GroundPaintedAccentContourClusterDebugSnapshot snapshot,
            Vector3 localOffset,
            bool drawRejections)
        {
            if (!snapshot.IsValid)
            {
                return;
            }

            Transform groundTransform = ground.transform;
            GroundPaintedAccentContourClusterCandidate[] candidates =
                snapshot.Candidates;
            for (int candidateIndex = 0;
                 candidates != null && candidateIndex < candidates.Length;
                 candidateIndex++)
            {
                GroundPaintedAccentContourClusterCandidate candidate =
                    candidates[candidateIndex];
                if (!candidate.IsValid)
                {
                    continue;
                }

                DrawPaintedAccentContourClusterCandidate(
                    groundTransform,
                    candidate,
                    localOffset);
            }

            if (!drawRejections)
            {
                return;
            }

            GroundPaintedAccentContourClusterRejectionDebugPoint[] rejections =
                snapshot.Rejections;
            for (int rejectionIndex = 0;
                 rejections != null && rejectionIndex < rejections.Length;
                 rejectionIndex++)
            {
                GroundPaintedAccentContourClusterRejectionDebugPoint rejection =
                    rejections[rejectionIndex];
                Vector3 localPosition = rejection.LocalPosition + localOffset;
                Vector3 worldPosition =
                    groundTransform.TransformPoint(localPosition);
                float size =
                    HandleUtility.GetHandleSize(worldPosition) * 0.070f;
                Vector3 right = groundTransform.right * size;
                Vector3 forward = groundTransform.forward * size;

                Handles.color = new Color(0f, 0f, 0f, 0.92f);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(
                    5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);
                Handles.color =
                    ResolvePaintedAccentContourClusterRejectionColor(
                        rejection.Reason);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - right - forward,
                    worldPosition + right + forward);
                Handles.DrawAAPolyLine(
                    2.5f,
                    worldPosition - right + forward,
                    worldPosition + right - forward);
            }
        }

        private static void DrawPaintedAccentContourClusterCandidate(
            Transform groundTransform,
            GroundPaintedAccentContourClusterCandidate candidate,
            Vector3 localOffset)
        {
            GroundPaintedAccentContourClusterChain[] chains = candidate.Chains;
            for (int chainIndex = 0;
                 chains != null && chainIndex < chains.Length;
                 chainIndex++)
            {
                GroundPaintedAccentContourClusterChain chain =
                    chains[chainIndex];
                if (!chain.IsValid)
                {
                    continue;
                }

                Vector3[] localPoints = chain.LocalSurfacePoints;
                float[] halfWidths = chain.HalfWidths;
                Vector3[] centerWorld = new Vector3[localPoints.Length];
                Vector3[] leftWorld = new Vector3[localPoints.Length];
                Vector3[] rightWorld = new Vector3[localPoints.Length];

                for (int pointIndex = 0;
                     pointIndex < localPoints.Length;
                     pointIndex++)
                {
                    Vector3 localPoint = localPoints[pointIndex] + localOffset;
                    localPoint.y += 0.045f;
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

                Handles.color = new Color(0f, 0f, 0f, 0.98f);
                Handles.DrawAAPolyLine(6.5f, centerWorld);
                Handles.color =
                    ResolvePaintedAccentContourClusterChainColor(chain.Role);
                Handles.DrawAAPolyLine(3.5f, centerWorld);

                Handles.color = new Color(0f, 0f, 0f, 0.88f);
                Handles.DrawAAPolyLine(3.25f, leftWorld);
                Handles.DrawAAPolyLine(3.25f, rightWorld);
                Handles.color = new Color(0.10f, 0.32f, 0.92f, 0.98f);
                Handles.DrawAAPolyLine(1.55f, leftWorld);
                Handles.DrawAAPolyLine(1.55f, rightWorld);

                if (chain.Role ==
                    GroundPaintedAccentContourClusterChainRole.Branch)
                {
                    Vector3 junctionWorld = centerWorld[0];
                    float radius =
                        HandleUtility.GetHandleSize(junctionWorld) * 0.026f;
                    Handles.color = new Color(0f, 0f, 0f, 0.95f);
                    Handles.DrawWireDisc(
                        junctionWorld,
                        groundTransform.up,
                        radius * 1.55f);
                    Handles.color = new Color(1f, 1f, 1f, 0.98f);
                    Handles.DrawWireDisc(
                        junctionWorld,
                        groundTransform.up,
                        radius);
                }
            }

            Vector3 highLocal = candidate.HighPointLocalPosition + localOffset;
            highLocal.y += 0.045f;
            Vector3 highWorld = groundTransform.TransformPoint(highLocal);
            float highRadius =
                HandleUtility.GetHandleSize(highWorld) * 0.052f;
            Handles.color = new Color(0f, 0f, 0f, 0.98f);
            Handles.DrawWireDisc(
                highWorld,
                groundTransform.up,
                highRadius * 1.45f);
            Handles.color = new Color(1f, 0.48f, 0.04f, 1f);
            Handles.DrawWireDisc(
                highWorld,
                groundTransform.up,
                highRadius);
        }

        private static void
            DrawPaintedAccentProjectedGlyphAndClusterCandidateComparison(
                GeneratedGround ground,
                GroundPaintedAccentProjectedGlyphDebugSnapshot projectedSnapshot,
                GroundPaintedAccentContourClusterDebugSnapshot candidateSnapshot)
        {
            if (!projectedSnapshot.IsValid || !candidateSnapshot.IsValid)
            {
                return;
            }

            System.Collections.Generic.Dictionary<int,
                GroundPaintedAccentProjectedGlyph> projectedBySeed =
                    new System.Collections.Generic.Dictionary<int,
                        GroundPaintedAccentProjectedGlyph>();
            GroundPaintedAccentProjectedGlyph[] projectedGlyphs =
                projectedSnapshot.Glyphs;
            for (int glyphIndex = 0;
                 projectedGlyphs != null && glyphIndex < projectedGlyphs.Length;
                 glyphIndex++)
            {
                GroundPaintedAccentProjectedGlyph glyph =
                    projectedGlyphs[glyphIndex];
                if (glyph.IsValid && !projectedBySeed.ContainsKey(glyph.Seed))
                {
                    projectedBySeed.Add(glyph.Seed, glyph);
                }
            }

            Transform groundTransform = ground.transform;
            Vector3 localNorth3 =
                groundTransform.InverseTransformDirection(Vector3.forward);
            Vector2 localNorth =
                new Vector2(localNorth3.x, localNorth3.z);
            localNorth =
                localNorth.sqrMagnitude > 0.000001f
                    ? localNorth.normalized
                    : Vector2.up;
            Vector2 localEast = new Vector2(localNorth.y, -localNorth.x);

            GroundPaintedAccentContourClusterCandidate[] candidates =
                candidateSnapshot.Candidates;
            for (int candidateIndex = 0;
                 candidates != null && candidateIndex < candidates.Length;
                 candidateIndex++)
            {
                GroundPaintedAccentContourClusterCandidate candidate =
                    candidates[candidateIndex];
                if (!candidate.IsValid ||
                    !projectedBySeed.TryGetValue(
                        candidate.Seed,
                        out GroundPaintedAccentProjectedGlyph projectedGlyph))
                {
                    continue;
                }

                ResolvePaintedAccentProjectedGlyphEastRange(
                    projectedGlyph,
                    localEast,
                    out _,
                    out float projectedMaximum);
                ResolvePaintedAccentContourClusterEastRange(
                    candidate,
                    localEast,
                    out float candidateMinimum,
                    out _);
                float comparisonGap =
                    Mathf.Max(0.22f, projectedGlyph.CombinedPeakHeight * 0.70f);
                float anchorEast =
                    Vector2.Dot(
                        new Vector2(
                            candidate.SourceAnchorLocalPosition.x,
                            candidate.SourceAnchorLocalPosition.z),
                        localEast);
                float baselineEastShift =
                    anchorEast - comparisonGap * 0.5f - projectedMaximum;
                float candidateEastShift =
                    anchorEast + comparisonGap * 0.5f - candidateMinimum;
                Vector3 baselineOffset =
                    new Vector3(
                        localEast.x * baselineEastShift,
                        0f,
                        localEast.y * baselineEastShift);
                Vector3 candidateOffset =
                    new Vector3(
                        localEast.x * candidateEastShift,
                        0f,
                        localEast.y * candidateEastShift);

                DrawPaintedAccentProjectedGlyphAtOffset(
                    groundTransform,
                    projectedGlyph,
                    baselineOffset);
                DrawPaintedAccentContourClusterCandidate(
                    groundTransform,
                    candidate,
                    candidateOffset);

                Vector3 baselineAnchor =
                    candidate.SourceAnchorLocalPosition + baselineOffset;
                baselineAnchor.y += 0.055f;
                Vector3 candidateAnchor =
                    candidate.SourceAnchorLocalPosition + candidateOffset;
                candidateAnchor.y += 0.055f;
                Handles.color = new Color(1f, 1f, 1f, 0.38f);
                Handles.DrawDottedLine(
                    groundTransform.TransformPoint(baselineAnchor),
                    groundTransform.TransformPoint(candidateAnchor),
                    4f);
            }
        }

        private static void DrawPaintedAccentProjectedGlyphAtOffset(
            Transform groundTransform,
            GroundPaintedAccentProjectedGlyph glyph,
            Vector3 localOffset)
        {
            if (!glyph.IsValid)
            {
                return;
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
                Vector3 localPoint = localPoints[pointIndex] + localOffset;
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

        private static void ResolvePaintedAccentProjectedGlyphEastRange(
            GroundPaintedAccentProjectedGlyph glyph,
            Vector2 localEast,
            out float minimum,
            out float maximum)
        {
            minimum = float.PositiveInfinity;
            maximum = float.NegativeInfinity;
            Vector3[] points = glyph.LocalSurfacePoints;
            for (int pointIndex = 0;
                 points != null && pointIndex < points.Length;
                 pointIndex++)
            {
                float coordinate =
                    Vector2.Dot(
                        new Vector2(points[pointIndex].x, points[pointIndex].z),
                        localEast);
                minimum = Mathf.Min(minimum, coordinate);
                maximum = Mathf.Max(maximum, coordinate);
            }

            if (float.IsPositiveInfinity(minimum) ||
                float.IsNegativeInfinity(maximum))
            {
                minimum = 0f;
                maximum = 0f;
            }
        }

        private static void ResolvePaintedAccentContourClusterEastRange(
            GroundPaintedAccentContourClusterCandidate candidate,
            Vector2 localEast,
            out float minimum,
            out float maximum)
        {
            minimum = float.PositiveInfinity;
            maximum = float.NegativeInfinity;
            GroundPaintedAccentContourClusterChain[] chains = candidate.Chains;
            for (int chainIndex = 0;
                 chains != null && chainIndex < chains.Length;
                 chainIndex++)
            {
                Vector3[] points = chains[chainIndex].LocalSurfacePoints;
                for (int pointIndex = 0;
                     points != null && pointIndex < points.Length;
                     pointIndex++)
                {
                    float coordinate =
                        Vector2.Dot(
                            new Vector2(
                                points[pointIndex].x,
                                points[pointIndex].z),
                            localEast);
                    minimum = Mathf.Min(minimum, coordinate);
                    maximum = Mathf.Max(maximum, coordinate);
                }
            }

            if (float.IsPositiveInfinity(minimum) ||
                float.IsNegativeInfinity(maximum))
            {
                minimum = 0f;
                maximum = 0f;
            }
        }

        private static Color ResolvePaintedAccentContourClusterChainColor(
            GroundPaintedAccentContourClusterChainRole role)
        {
            switch (role)
            {
                case GroundPaintedAccentContourClusterChainRole.Branch:
                    return new Color(0.10f, 1.00f, 0.42f, 1.00f);
                case GroundPaintedAccentContourClusterChainRole.Echo:
                    return new Color(0.28f, 0.64f, 1.00f, 1.00f);
                case GroundPaintedAccentContourClusterChainRole.PrimaryLeftArm:
                case GroundPaintedAccentContourClusterChainRole.PrimaryRightArm:
                default:
                    return new Color(0.00f, 0.94f, 0.94f, 1.00f);
            }
        }

        private static Color ResolvePaintedAccentContourClusterRejectionColor(
            GroundPaintedAccentContourClusterRejectionReason reason)
        {
            switch (reason)
            {
                case GroundPaintedAccentContourClusterRejectionReason.River:
                    return new Color(0.10f, 0.45f, 1.00f, 1.00f);
                case GroundPaintedAccentContourClusterRejectionReason.ModifierExclusion:
                    return new Color(1.00f, 0.90f, 0.08f, 1.00f);
                case GroundPaintedAccentContourClusterRejectionReason.BroadSlope:
                    return new Color(1.00f, 0.10f, 0.08f, 1.00f);
                case GroundPaintedAccentContourClusterRejectionReason.LocalGrade:
                    return new Color(0.75f, 0.18f, 1.00f, 1.00f);
                case GroundPaintedAccentContourClusterRejectionReason.UpwardExcursion:
                    return new Color(1.00f, 0.05f, 0.72f, 1.00f);
                case GroundPaintedAccentContourClusterRejectionReason.Sampling:
                default:
                    return new Color(1.00f, 0.45f, 0.05f, 1.00f);
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
            bool showProjectedGlyphs,
            bool showClusterCandidates,
            bool compareProjectedAndCandidate,
            PaintedAccentPlacementOverlayWeightMode overlayWeightMode,
            GroundPaintedAccentPlacementDebugSnapshot snapshot,
            Vector3[] acceptedPositions,
            bool snapshotBuildFailed,
            bool projectedSnapshotBuildFailed,
            bool candidateSnapshotBuildFailed,
            GroundPaintedAccentProjectedGlyphDebugSnapshot projectedSnapshot,
            GroundPaintedAccentContourClusterDebugSnapshot candidateSnapshot)
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
                new System.Text.StringBuilder(384);
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

            if (showProjectedGlyphs || compareProjectedAndCandidate)
            {
                text.AppendLine("Red/purple: accepted projected baseline");
                text.AppendLine("Yellow ring: accepted projected crest");
                GroundPaintedAccentProjectedGlyphDiagnostics diagnostics =
                    projectedSnapshot.Diagnostics;
                text.Append("Projected accepted/rejected: ");
                text.Append(diagnostics.ProjectedGlyphsAccepted);
                text.Append(" / ");
                text.AppendLine(
                    diagnostics.ProjectedGlyphsRejectedTotal.ToString());
            }

            if (showClusterCandidates || compareProjectedAndCandidate)
            {
                text.AppendLine("Cyan/green/blue: cluster primary/branch/echo");
                text.AppendLine("Orange root; white junction; candidate X: rejection");
                GroundPaintedAccentContourClusterDiagnostics diagnostics =
                    candidateSnapshot.Diagnostics;
                text.Append("Cluster accepted/rejected: ");
                text.Append(diagnostics.CandidatesAccepted);
                text.Append(" / ");
                text.AppendLine(diagnostics.RejectedTotal.ToString());
                text.Append("Accepted upward violations: ");
                text.AppendLine(
                    diagnostics.AcceptedUpwardViolationCount.ToString());
            }

            if (compareProjectedAndCandidate)
            {
                text.AppendLine("Paired preview copies: accepted left, A9A right");
                text.AppendLine("True-position overlays remain independently controlled");
            }

            if (snapshotBuildFailed && (showDistribution || showProposals))
            {
                text.AppendLine("PLACEMENT SNAPSHOT UNAVAILABLE");
            }

            if (projectedSnapshotBuildFailed &&
                (showProjectedGlyphs || compareProjectedAndCandidate))
            {
                text.AppendLine("PROJECTED GLYPH SNAPSHOT UNAVAILABLE");
            }

            if (candidateSnapshotBuildFailed &&
                (showClusterCandidates || compareProjectedAndCandidate))
            {
                text.AppendLine("CLUSTER CANDIDATE SNAPSHOT UNAVAILABLE");
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
            bool showAnyShape =
                showProjectedGlyphs ||
                showClusterCandidates ||
                compareProjectedAndCandidate;
            float boxHeight =
                compareProjectedAndCandidate
                    ? 292f
                    : showAnyShape
                        ? 232f
                        : 124f;
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
