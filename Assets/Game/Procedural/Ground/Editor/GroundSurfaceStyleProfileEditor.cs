using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    [CustomEditor(typeof(GroundSurfaceStyleProfile))]
    public sealed class GroundSurfaceStyleProfileEditor : UnityEditor.Editor
    {
        private SerializedProperty displayName;
        private SerializedProperty defaultSurfaceProfile;
        private SerializedProperty variants;

        private static readonly List<GroundSurfaceStyleProfile>
            PendingRefreshProfiles = new List<GroundSurfaceStyleProfile>();

        private static bool refreshScheduled;

        private readonly Dictionary<string, bool> foldouts =
            new Dictionary<string, bool>();

        private void OnEnable()
        {
            displayName = serializedObject.FindProperty("displayName");
            defaultSurfaceProfile =
                serializedObject.FindProperty("defaultSurfaceProfile");
            variants = serializedObject.FindProperty("variants");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(
                "Ground Surface Style Profile",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                displayName,
                new GUIContent(
                    "Display Name",
                    "Human-facing surface-family name shown by GeneratedGround."));

            EditorGUILayout.PropertyField(
                defaultSurfaceProfile,
                new GUIContent(
                    "Default Surface Profile",
                    "Semantic/mask-generation profile used unless a GeneratedGround object overrides it."));

            DrawValidationWarnings();

            DrawApplyToOpenGroundsButton();

            EditorGUILayout.Space(8f);
            DrawVariantList();

            bool changed = serializedObject.ApplyModifiedProperties();

            if (changed)
            {
                QueueRefreshOpenGeneratedGrounds();
            }
        }

        private void DrawApplyToOpenGroundsButton()
        {
            EditorGUILayout.Space(4f);

            if (!GUILayout.Button(
                    "Apply To Open Generated Grounds",
                    GUILayout.Height(24f)))
            {
                return;
            }

            serializedObject.ApplyModifiedProperties();
            RefreshOpenGeneratedGroundsForProfile(
                target as GroundSurfaceStyleProfile,
                true);
        }

        private void DrawVariantList()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"Variants ({variants.arraySize})",
                    EditorStyles.boldLabel);

                if (GUILayout.Button(
                        "Add Variant",
                        EditorStyles.miniButton,
                        GUILayout.Width(92f)))
                {
                    AddVariant();
                }
            }

            if (variants.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "This style has no variants. Add at least one variant before assigning it to GeneratedGround.",
                    MessageType.Warning);
                return;
            }

            for (int index = 0; index < variants.arraySize; index++)
            {
                SerializedProperty variant =
                    variants.GetArrayElementAtIndex(index);

                DrawVariantCard(index, variant);
            }
        }

        private void DrawVariantCard(
            int index,
            SerializedProperty variant)
        {
            SerializedProperty id =
                variant.FindPropertyRelative("id");
            SerializedProperty variantDisplayName =
                variant.FindPropertyRelative("displayName");
            SerializedProperty materialControls =
                variant.FindPropertyRelative("materialControls");
            SerializedProperty features =
                variant.FindPropertyRelative("features");

            string title = BuildVariantTitle(
                index,
                id,
                variantDisplayName);
            string foldoutKey = $"variant_{index}_{id.stringValue}";

            EditorGUILayout.Space(4f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    bool expanded = GetFoldout(foldoutKey, true);
                    expanded = EditorGUILayout.Foldout(
                        expanded,
                        title,
                        true);
                    SetFoldout(foldoutKey, expanded);

                    if (GUILayout.Button(
                            "Duplicate",
                            EditorStyles.miniButton,
                            GUILayout.Width(72f)))
                    {
                        DuplicateVariant(index);
                        return;
                    }

                    if (GUILayout.Button(
                            "Remove",
                            EditorStyles.miniButton,
                            GUILayout.Width(58f)))
                    {
                        RemoveVariant(index);
                        return;
                    }
                }

                EditorGUILayout.LabelField(
                    BuildFeatureSummary(features),
                    EditorStyles.miniLabel);

                if (!GetFoldout(foldoutKey, true))
                {
                    return;
                }

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(
                    id,
                    new GUIContent(
                        "Stable ID",
                        "Scene-safe variant identifier. Do not rename after GeneratedGround objects reference it."));

                EditorGUILayout.PropertyField(
                    variantDisplayName,
                    new GUIContent(
                        "Display Name",
                        "Human-facing variant name shown by GeneratedGround."));

                DrawMaterialControls(index, materialControls);
                DrawFeatureList(index, features);

                EditorGUI.indentLevel--;
            }
        }

        private void DrawMaterialControls(
            int variantIndex,
            SerializedProperty materialControls)
        {
            string key = $"material_{variantIndex}";
            bool expanded = GetFoldout(key, false);

            expanded = EditorGUILayout.Foldout(
                expanded,
                "Material Controls",
                true);
            SetFoldout(key, expanded);

            if (!expanded)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (materialControls == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing material-control data on this variant.",
                    MessageType.Error);
            }
            else
            {
                EditorGUILayout.PropertyField(
                    materialControls,
                    GUIContent.none,
                    true);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFeatureList(
            int variantIndex,
            SerializedProperty features)
        {
            string key = $"features_{variantIndex}";
            bool expanded = GetFoldout(key, false);

            using (new EditorGUILayout.HorizontalScope())
            {
                expanded = EditorGUILayout.Foldout(
                    expanded,
                    $"Features ({features.arraySize})",
                    true);
                SetFoldout(key, expanded);

                if (GUILayout.Button(
                        "Add Feature",
                        EditorStyles.miniButton,
                        GUILayout.Width(86f)))
                {
                    AddFeature(features);
                    expanded = true;
                    SetFoldout(key, true);
                }
            }

            if (!expanded)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (features.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No feature recipes. This variant is material-only.",
                    MessageType.None);
            }

            for (int index = 0; index < features.arraySize; index++)
            {
                DrawFeatureCard(features, index);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFeatureCard(
            SerializedProperty features,
            int index)
        {
            SerializedProperty feature =
                features.GetArrayElementAtIndex(index);
            SerializedProperty kind =
                feature.FindPropertyRelative("kind");
            SerializedProperty enabled =
                feature.FindPropertyRelative("enabled");
            SerializedProperty costClass =
                feature.FindPropertyRelative("costClass");
            SerializedProperty strength =
                feature.FindPropertyRelative("strength");
            SerializedProperty scale =
                feature.FindPropertyRelative("scale");
            SerializedProperty contrast =
                feature.FindPropertyRelative("contrast");
            SerializedProperty maskInfluence =
                feature.FindPropertyRelative("maskInfluence");
            SerializedProperty direction =
                feature.FindPropertyRelative("direction");
            SerializedProperty seedOffset =
                feature.FindPropertyRelative("seedOffset");
            SerializedProperty paintedAccentStrokeWidth =
                feature.FindPropertyRelative("paintedAccentStrokeWidth");
            SerializedProperty paintedAccentStrokeDensity =
                feature.FindPropertyRelative("paintedAccentStrokeDensity");
            SerializedProperty paintedAccentStrokeLengthMin =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMin");
            SerializedProperty paintedAccentStrokeLengthMax =
                feature.FindPropertyRelative("paintedAccentStrokeLengthMax");
            SerializedProperty paintedAccentStrokeFacingDirectionDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeFacingDirectionDegrees");
            SerializedProperty paintedAccentStrokeAngleJitterDegrees =
                feature.FindPropertyRelative("paintedAccentStrokeAngleJitterDegrees");
            SerializedProperty paintedAccentFoldHeight =
                feature.FindPropertyRelative("paintedAccentFoldHeight");
            SerializedProperty paintedAccentFoldIrregularity =
                feature.FindPropertyRelative("paintedAccentFoldIrregularity");
            SerializedProperty paintedAccentFoldEndTaper =
                feature.FindPropertyRelative("paintedAccentFoldEndTaper");

            string featureKey = $"feature_{features.propertyPath}_{index}";
            bool expanded = GetFoldout(featureKey, false);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    expanded = EditorGUILayout.Foldout(
                        expanded,
                        $"{index + 1}. {GetEnumDisplayName(kind)}",
                        true);
                    SetFoldout(featureKey, expanded);

                    enabled.boolValue = EditorGUILayout.Toggle(
                        enabled.boolValue,
                        GUILayout.Width(18f));

                    if (GUILayout.Button(
                            "Remove",
                            EditorStyles.miniButton,
                            GUILayout.Width(58f)))
                    {
                        features.DeleteArrayElementAtIndex(index);
                        return;
                    }
                }

                DrawFeatureWarning(kind, enabled, costClass);

                if (!expanded)
                {
                    return;
                }

                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(kind);
                EditorGUILayout.PropertyField(enabled);
                EditorGUILayout.PropertyField(costClass);
                EditorGUILayout.Slider(strength, 0f, 1f);
                EditorGUILayout.Slider(scale, 0.1f, 30f);
                EditorGUILayout.Slider(contrast, 0f, 1f);
                EditorGUILayout.Slider(maskInfluence, 0f, 1f);
                EditorGUILayout.PropertyField(direction);
                EditorGUILayout.PropertyField(seedOffset);

                if ((GroundSurfaceFeatureKind)kind.intValue ==
                    GroundSurfaceFeatureKind.PaintedAccentLines)
                {
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(
                        "Painted Accent 3D Strokes",
                        EditorStyles.miniBoldLabel);
                    EditorGUILayout.Slider(
                        paintedAccentStrokeWidth,
                        0.04f,
                        0.35f,
                        new GUIContent(
                            "Stroke Width",
                            "Visible width in metres for the narrow secondary ridge. Legacy BodyWidth is not used by the ridge mesh."));
                    EditorGUILayout.Slider(
                        paintedAccentStrokeDensity,
                        0f,
                        80f,
                        new GUIContent(
                            "Stroke Density",
                            "Approximate target number of generated 3D strokes per standard 40x40 ground patch."));
                    EditorGUILayout.Slider(
                        paintedAccentStrokeLengthMin,
                        0.20f,
                        4.0f,
                        new GUIContent(
                            "Stroke Length Min",
                            "Minimum generated 3D stroke length in metres."));
                    EditorGUILayout.Slider(
                        paintedAccentStrokeLengthMax,
                        0.25f,
                        6.0f,
                        new GUIContent(
                            "Stroke Length Max",
                            "Maximum generated 3D stroke length in metres."));
                    EditorGUILayout.Slider(
                        paintedAccentStrokeFacingDirectionDegrees,
                        0f,
                        360f,
                        new GUIContent(
                            "Facing Direction Degrees",
                            "Local X/Z player or camera-facing direction. Generated 3D strokes are perpendicular to this direction before signed Angle Jitter is applied."));
                    EditorGUILayout.Slider(
                        paintedAccentStrokeAngleJitterDegrees,
                        0f,
                        30f,
                        new GUIContent(
                            "Angle Jitter Degrees",
                            "Maximum signed angle offset in degrees around the perpendicular stroke angle derived from Facing Direction Degrees. Each stroke rolls independently between -value and +value."));
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField(
                        "Narrow Stochastic 3D Ridge",
                        EditorStyles.miniBoldLabel);
                    EditorGUILayout.Slider(
                        paintedAccentFoldHeight,
                        0f,
                        0.05f,
                        new GUIContent(
                            "Fold Height",
                            "Maximum raised ridge height in metres, applied along the sampled ground normal. Tiny accent marks should normally remain in the 0.006-0.025 metre range."));
                    EditorGUILayout.Slider(
                        paintedAccentFoldIrregularity,
                        0f,
                        1f,
                        new GUIContent(
                            "Fold Irregularity",
                            "Strength of deterministic smooth profile variation across and along each stroke. This does not add lateral centerline squiggle."));
                    EditorGUILayout.Slider(
                        paintedAccentFoldEndTaper,
                        0f,
                        1f,
                        new GUIContent(
                            "Fold End Taper",
                            "How much of the stroke length blends the raised profile back into the ground at each end."));

                    if (paintedAccentStrokeLengthMax.floatValue <
                        paintedAccentStrokeLengthMin.floatValue + 0.05f)
                    {
                        paintedAccentStrokeLengthMax.floatValue =
                            paintedAccentStrokeLengthMin.floatValue + 0.05f;
                    }
                }

                EditorGUI.indentLevel--;
            }
        }

        private void DrawFeatureWarning(
            SerializedProperty kind,
            SerializedProperty enabled,
            SerializedProperty costClass)
        {
            if (!enabled.boolValue)
            {
                return;
            }

            GroundSurfaceFeatureKind resolvedKind =
                (GroundSurfaceFeatureKind)kind.intValue;
            GroundSurfaceFeatureCostClass resolvedCostClass =
                (GroundSurfaceFeatureCostClass)costClass.intValue;

            if (resolvedKind == GroundSurfaceFeatureKind.None)
            {
                EditorGUILayout.HelpBox(
                    "Enabled feature has kind None and will not render.",
                    MessageType.Warning);
                return;
            }

            if (resolvedCostClass != GroundSurfaceFeatureCostClass.ShaderOnly)
            {
                EditorGUILayout.HelpBox(
                    "This feature cost class is reserved for future generated-texture/runtime modules and is not rendered by the current shader feature stack.",
                    MessageType.Info);
                return;
            }

            if (!IsCurrentlyRenderableShaderFeature(resolvedKind))
            {
                EditorGUILayout.HelpBox(
                    $"{resolvedKind} is reserved in the feature contract but is not implemented by the current ground shader feature stack.",
                    MessageType.Info);
            }
        }

        private void DrawValidationWarnings()
        {
            if (defaultSurfaceProfile.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox(
                    "Missing default GroundSurfaceProfile. GeneratedGround can still use a local override, but this style is incomplete as a standalone family.",
                    MessageType.Warning);
            }

            if (variants.arraySize == 0)
            {
                EditorGUILayout.HelpBox(
                    "No variants are defined. GeneratedGround will not have a selectable variant for this style.",
                    MessageType.Warning);
                return;
            }

            Dictionary<string, int> ids =
                new Dictionary<string, int>(StringComparer.Ordinal);

            for (int index = 0; index < variants.arraySize; index++)
            {
                SerializedProperty variant =
                    variants.GetArrayElementAtIndex(index);
                SerializedProperty id =
                    variant.FindPropertyRelative("id");
                SerializedProperty materialControls =
                    variant.FindPropertyRelative("materialControls");

                if (string.IsNullOrWhiteSpace(id.stringValue))
                {
                    EditorGUILayout.HelpBox(
                        $"Variant {index + 1} has no stable id.",
                        MessageType.Warning);
                    continue;
                }

                if (ids.TryGetValue(id.stringValue, out int firstIndex))
                {
                    EditorGUILayout.HelpBox(
                        $"Duplicate variant id '{id.stringValue}' on variants {firstIndex + 1} and {index + 1}. GeneratedGround stores variants by id, so ids must be unique.",
                        MessageType.Warning);
                }
                else
                {
                    ids.Add(id.stringValue, index);
                }

                if (materialControls == null)
                {
                    EditorGUILayout.HelpBox(
                        $"Variant '{id.stringValue}' has no material controls property.",
                        MessageType.Error);
                }

                DrawFeatureListValidation(
                    id.stringValue,
                    variant.FindPropertyRelative("features"));
            }
        }

        private void DrawFeatureListValidation(
            string variantId,
            SerializedProperty features)
        {
            if (features == null)
            {
                return;
            }

            HashSet<GroundSurfaceFeatureKind> seenShaderFeatureKinds =
                new HashSet<GroundSurfaceFeatureKind>();

            for (int index = 0; index < features.arraySize; index++)
            {
                SerializedProperty feature =
                    features.GetArrayElementAtIndex(index);
                SerializedProperty kind =
                    feature.FindPropertyRelative("kind");
                SerializedProperty enabled =
                    feature.FindPropertyRelative("enabled");
                SerializedProperty costClass =
                    feature.FindPropertyRelative("costClass");

                if (!enabled.boolValue)
                {
                    continue;
                }

                GroundSurfaceFeatureKind resolvedKind =
                    (GroundSurfaceFeatureKind)kind.intValue;
                GroundSurfaceFeatureCostClass resolvedCostClass =
                    (GroundSurfaceFeatureCostClass)costClass.intValue;

                if (resolvedKind == GroundSurfaceFeatureKind.None)
                {
                    EditorGUILayout.HelpBox(
                        $"Variant '{variantId}' has an enabled feature with kind None.",
                        MessageType.Warning);
                }
                else if (resolvedCostClass == GroundSurfaceFeatureCostClass.ShaderOnly &&
                         IsCurrentlyRenderableShaderFeature(resolvedKind))
                {
                    if (!seenShaderFeatureKinds.Add(resolvedKind))
                    {
                        EditorGUILayout.HelpBox(
                            $"Variant '{variantId}' has multiple enabled '{resolvedKind}' shader features. The shader feature stack uses the first enabled recipe of each kind.",
                            MessageType.Warning);
                    }
                }
                else if (resolvedCostClass == GroundSurfaceFeatureCostClass.ShaderOnly &&
                         !IsCurrentlyRenderableShaderFeature(resolvedKind))
                {
                    EditorGUILayout.HelpBox(
                        $"Variant '{variantId}' uses '{resolvedKind}', which is reserved but not currently rendered by the shader feature stack.",
                        MessageType.Info);
                }
                else if (resolvedCostClass != GroundSurfaceFeatureCostClass.ShaderOnly)
                {
                    EditorGUILayout.HelpBox(
                        $"Variant '{variantId}' uses a non-shader feature cost class. That is a reserved future path and currently has no shader feature stack output.",
                        MessageType.Info);
                }
            }
        }

        private void AddVariant()
        {
            int index = variants.arraySize;
            variants.InsertArrayElementAtIndex(index);

            SerializedProperty variant =
                variants.GetArrayElementAtIndex(index);

            SetVariantDefaults(
                variant,
                MakeUniqueVariantId(BuildDefaultVariantId(), index),
                "New Variant");
        }

        private void DuplicateVariant(int index)
        {
            SerializedProperty source =
                variants.GetArrayElementAtIndex(index);
            SerializedProperty sourceId =
                source.FindPropertyRelative("id");
            SerializedProperty sourceName =
                source.FindPropertyRelative("displayName");

            variants.InsertArrayElementAtIndex(index + 1);

            SerializedProperty duplicate =
                variants.GetArrayElementAtIndex(index + 1);
            SerializedProperty duplicateId =
                duplicate.FindPropertyRelative("id");
            SerializedProperty duplicateName =
                duplicate.FindPropertyRelative("displayName");

            duplicateId.stringValue = MakeUniqueVariantId(
                string.IsNullOrWhiteSpace(sourceId.stringValue)
                    ? BuildDefaultVariantId()
                    : sourceId.stringValue + "_copy",
                index + 1);

            duplicateName.stringValue =
                string.IsNullOrWhiteSpace(sourceName.stringValue)
                    ? "Variant Copy"
                    : sourceName.stringValue + " Copy";
        }

        private void RemoveVariant(int index)
        {
            variants.DeleteArrayElementAtIndex(index);
        }

        private void AddFeature(SerializedProperty features)
        {
            int index = features.arraySize;
            features.InsertArrayElementAtIndex(index);

            SerializedProperty feature =
                features.GetArrayElementAtIndex(index);

            feature.FindPropertyRelative("kind").intValue =
                (int)GroundSurfaceFeatureKind.DirectionalStreaks;
            feature.FindPropertyRelative("enabled").boolValue = true;
            feature.FindPropertyRelative("costClass").intValue =
                (int)GroundSurfaceFeatureCostClass.ShaderOnly;
            feature.FindPropertyRelative("strength").floatValue = 0.25f;
            feature.FindPropertyRelative("scale").floatValue = 5f;
            feature.FindPropertyRelative("contrast").floatValue = 0.5f;
            feature.FindPropertyRelative("maskInfluence").floatValue = 0.5f;
            feature.FindPropertyRelative("direction").vector2Value =
                new Vector2(0.82f, 0.36f);
            feature.FindPropertyRelative("seedOffset").intValue = 0;

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
            SerializedProperty foldIrregularity =
                feature.FindPropertyRelative("paintedAccentFoldIrregularity");
            SerializedProperty foldEndTaper =
                feature.FindPropertyRelative("paintedAccentFoldEndTaper");

            if (strokeWidth != null)
            {
                strokeWidth.floatValue = 0.12f;
            }

            if (strokeDensity != null)
            {
                strokeDensity.floatValue = 34f;
            }

            if (strokeLengthMin != null)
            {
                strokeLengthMin.floatValue = 0.55f;
            }

            if (strokeLengthMax != null)
            {
                strokeLengthMax.floatValue = 1.55f;
            }

            if (strokeFacingDirectionDegrees != null)
            {
                strokeFacingDirectionDegrees.floatValue = 90f;
            }

            if (strokeAngleJitterDegrees != null)
            {
                strokeAngleJitterDegrees.floatValue = 18f;
            }

            if (foldHeight != null)
            {
                foldHeight.floatValue = 0.018f;
            }

            if (foldIrregularity != null)
            {
                foldIrregularity.floatValue = 0.55f;
            }

            if (foldEndTaper != null)
            {
                foldEndTaper.floatValue = 0.65f;
            }
        }

        private void SetVariantDefaults(
            SerializedProperty variant,
            string id,
            string name)
        {
            variant.FindPropertyRelative("id").stringValue = id;
            variant.FindPropertyRelative("displayName").stringValue = name;

            SerializedProperty features =
                variant.FindPropertyRelative("features");
            features.ClearArray();
        }

        private string BuildDefaultVariantId()
        {
            string styleName = displayName.stringValue;

            if (string.IsNullOrWhiteSpace(styleName))
            {
                styleName = target != null ? target.name : "ground";
            }

            return SanitizeIdSegment(styleName) + ".variant";
        }

        private string MakeUniqueVariantId(
            string desiredId,
            int ignoredIndex)
        {
            string baseId = string.IsNullOrWhiteSpace(desiredId)
                ? BuildDefaultVariantId()
                : desiredId.Trim();

            if (!VariantIdExists(baseId, ignoredIndex))
            {
                return baseId;
            }

            int suffix = 2;

            while (VariantIdExists(baseId + "_" + suffix, ignoredIndex))
            {
                suffix++;
            }

            return baseId + "_" + suffix;
        }

        private bool VariantIdExists(
            string id,
            int ignoredIndex)
        {
            for (int index = 0; index < variants.arraySize; index++)
            {
                if (index == ignoredIndex)
                {
                    continue;
                }

                SerializedProperty variant =
                    variants.GetArrayElementAtIndex(index);
                SerializedProperty existingId =
                    variant.FindPropertyRelative("id");

                if (existingId.stringValue == id)
                {
                    return true;
                }
            }

            return false;
        }

        private static string BuildVariantTitle(
            int index,
            SerializedProperty id,
            SerializedProperty displayName)
        {
            string label = string.IsNullOrWhiteSpace(displayName.stringValue)
                ? id.stringValue
                : displayName.stringValue;

            if (string.IsNullOrWhiteSpace(label))
            {
                label = "Unnamed Variant";
            }

            return $"{index + 1}. {label}  [{id.stringValue}]";
        }

        private static string BuildFeatureSummary(SerializedProperty features)
        {
            if (features == null || features.arraySize == 0)
            {
                return "Features: none";
            }

            List<string> names = new List<string>();

            for (int index = 0; index < features.arraySize; index++)
            {
                SerializedProperty feature =
                    features.GetArrayElementAtIndex(index);
                SerializedProperty enabled =
                    feature.FindPropertyRelative("enabled");
                SerializedProperty kind =
                    feature.FindPropertyRelative("kind");

                if (!enabled.boolValue)
                {
                    continue;
                }

                names.Add(GetEnumDisplayName(kind));
            }

            return names.Count == 0
                ? "Features: none enabled"
                : "Features: " + string.Join(", ", names);
        }

        private void QueueRefreshOpenGeneratedGrounds()
        {
            GroundSurfaceStyleProfile profile =
                target as GroundSurfaceStyleProfile;

            if (profile == null)
            {
                return;
            }

            if (!PendingRefreshProfiles.Contains(profile))
            {
                PendingRefreshProfiles.Add(profile);
            }

            if (refreshScheduled)
            {
                return;
            }

            refreshScheduled = true;
            EditorApplication.delayCall += FlushPendingRefreshes;
        }

        private static void FlushPendingRefreshes()
        {
            refreshScheduled = false;

            if (PendingRefreshProfiles.Count == 0)
            {
                return;
            }

            GroundSurfaceStyleProfile[] profiles =
                PendingRefreshProfiles.ToArray();
            PendingRefreshProfiles.Clear();

            for (int index = 0; index < profiles.Length; index++)
            {
                RefreshOpenGeneratedGroundsForProfile(
                    profiles[index],
                    false);
            }
        }

        private static int RefreshOpenGeneratedGroundsForProfile(
            GroundSurfaceStyleProfile profile,
            bool logResult)
        {
            if (profile == null)
            {
                return 0;
            }

            int refreshedCount = 0;

            GeneratedGround[] grounds = UnityEngine.Object.FindObjectsByType<GeneratedGround>(
                FindObjectsInactive.Include);

            for (int index = 0; index < grounds.Length; index++)
            {
                GeneratedGround ground = grounds[index];

                if (ground == null ||
                    ground.SurfaceStyleProfile != profile)
                {
                    continue;
                }

                ground.RefreshSurfaceStyleState();
                refreshedCount++;
            }

            if (refreshedCount > 0)
            {
                SceneView.RepaintAll();
            }

            if (logResult)
            {
                Debug.Log(
                    $"Applied ground surface style '{profile.name}' to {refreshedCount} open GeneratedGround instance(s).",
                    profile);
            }

            return refreshedCount;
        }

        private static string GetEnumDisplayName(SerializedProperty property)
        {
            if (property.enumValueIndex >= 0 &&
                property.enumValueIndex < property.enumDisplayNames.Length)
            {
                return property.enumDisplayNames[property.enumValueIndex];
            }

            return property.intValue.ToString();
        }

        private static string SanitizeIdSegment(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "ground";
            }

            StringBuilder builder = new StringBuilder(value.Length);
            bool lastWasSeparator = false;

            for (int index = 0; index < value.Length; index++)
            {
                char c = char.ToLowerInvariant(value[index]);

                if ((c >= 'a' && c <= 'z') ||
                    (c >= '0' && c <= '9'))
                {
                    builder.Append(c);
                    lastWasSeparator = false;
                }
                else if (!lastWasSeparator)
                {
                    builder.Append('_');
                    lastWasSeparator = true;
                }
            }

            string result = builder.ToString().Trim('_');
            return string.IsNullOrWhiteSpace(result) ? "ground" : result;
        }

        private bool GetFoldout(string key, bool defaultValue)
        {
            if (foldouts.TryGetValue(key, out bool value))
            {
                return value;
            }

            foldouts.Add(key, defaultValue);
            return defaultValue;
        }

        private void SetFoldout(string key, bool value)
        {
            foldouts[key] = value;
        }

        private static bool IsCurrentlyRenderableShaderFeature(
            GroundSurfaceFeatureKind kind)
        {
            return kind == GroundSurfaceFeatureKind.DirectionalStreaks ||
                kind == GroundSurfaceFeatureKind.PooledWetness ||
                kind == GroundSurfaceFeatureKind.PaintedAccentLines ||
                kind == GroundSurfaceFeatureKind.TrampledWear;
        }
    }
}
