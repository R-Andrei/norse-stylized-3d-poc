using UnityEditor;
using UnityEngine;

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
            DrawPaintedAccent3DStrokeControls();
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

        private void DrawPaintedAccent3DStrokeControls()
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
            if (strokeWidth == null ||
                strokeDensity == null ||
                strokeLengthMin == null ||
                strokeLengthMax == null ||
                strokeFacingDirectionDegrees == null ||
                strokeAngleJitterDegrees == null ||
                foldHeight == null ||
                crestCrownHeight == null ||
                foldIrregularity == null ||
                foldEndTaper == null)
            {
                return;
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Painted Accent 3D Strokes",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Edits the selected surface variant's Painted Accent stroke layout and grounded crowned crest-ribbon controls. Rebuild the 3D Ridge Preview after changes. The preview is one visual-only child mesh and never modifies the base ground mesh or collider.",
                MessageType.None);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Slider(
                strokeWidth,
                0.01f,
                0.35f,
                new GUIContent(
                    "Stroke Width",
                    "Visible shoulder-to-shoulder width in metres for the crowned crest ribbon. Use 0.02 m for the focused shape proof. Legacy BodyWidth is not used by the ribbon mesh."));
            EditorGUILayout.Slider(
                strokeDensity,
                0f,
                80f,
                new GUIContent(
                    "Stroke Density",
                    "Approximate target number of generated 3D strokes per standard 40x40 ground patch."));
            EditorGUILayout.Slider(
                strokeLengthMin,
                0.20f,
                4.0f,
                new GUIContent(
                    "Stroke Length Min",
                    "Minimum generated 3D stroke length in metres."));
            EditorGUILayout.Slider(
                strokeLengthMax,
                0.25f,
                6.0f,
                new GUIContent(
                    "Stroke Length Max",
                    "Maximum generated 3D stroke length in metres."));
            EditorGUILayout.Slider(
                strokeFacingDirectionDegrees,
                0f,
                360f,
                new GUIContent(
                    "Facing Direction Degrees",
                    "Local X/Z player or camera-facing direction. Generated 3D strokes are perpendicular to this direction before signed Angle Jitter is applied."));
            EditorGUILayout.Slider(
                strokeAngleJitterDegrees,
                0f,
                30f,
                new GUIContent(
                    "Angle Jitter Degrees",
                    "Maximum signed angle offset in degrees around the perpendicular stroke angle derived from Facing Direction Degrees. Each stroke rolls independently between -value and +value."));
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Grounded Crowned Crest Ribbon",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.Slider(
                foldHeight,
                0f,
                0.50f,
                new GUIContent(
                    "Fold Height",
                    "Maximum longitudinal rise in metres. Use 0.25 m as the focused crowned-ribbon proof baseline; the extended 0.50 m range leaves room above the intended normal value without changing serialized style defaults."));
            EditorGUILayout.Slider(
                crestCrownHeight,
                0f,
                0.05f,
                new GUIContent(
                    "Crest Crown Height",
                    "Additional centreline height above the two ribbon shoulders. Test 0.01, 0.02, and 0.03 m at Stroke Width 0.02 m and Fold Height 0.25 m."));
            EditorGUILayout.Slider(
                foldIrregularity,
                0f,
                1f,
                new GUIContent(
                    "Fold Irregularity",
                    "Strength of the deterministic profile search that sets crest height along each stroke. This does not add lateral centerline squiggle."));
            EditorGUILayout.Slider(
                foldEndTaper,
                0f,
                1f,
                new GUIContent(
                    "Fold End Taper",
                    "How much of the stroke length blends the raised profile back into the ground at each end."));

            if (strokeLengthMax.floatValue < strokeLengthMin.floatValue + 0.05f)
            {
                strokeLengthMax.floatValue = strokeLengthMin.floatValue + 0.05f;
            }

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            styleObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(style);
            ApplyToTargets(
                "Tune Painted Accent Crowned Crest Ribbon",
                ground => ground.RefreshSurfaceMaterialProperties());
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

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Painted Accent Crowned Crest Ribbon Preview",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Builds one editor/debug-only crowned crest ribbon from the generated ground-following 3D strokes. Five stochastic profile samples derive each row's longitudinal crest height; three visible vertices across form two sloped crown faces while leaving the underside empty. It does not change the generated ground mesh, collision, or gameplay surface.",
                MessageType.None);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Build 3D Ridge Preview"))
            {
                ApplyToTargets(
                    "Build Painted Accent 3D Ridge Preview",
                    ground => ground.BuildPaintedAccentFoldSurfacePreview());
            }

            if (GUILayout.Button("Clear 3D Ridge Preview"))
            {
                ApplyToTargets(
                    "Clear Painted Accent 3D Ridge Preview",
                    ground => ground.ClearPaintedAccentFoldSurfacePreview());
            }

            EditorGUILayout.EndHorizontal();
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
