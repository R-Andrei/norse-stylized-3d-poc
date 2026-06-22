using UnityEditor;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Geometry.Ground.Editor
{
    [CustomEditor(typeof(GeneratedGround))]
    [CanEditMultipleObjects]
    public sealed class GeneratedGroundEditor :
        UnityEditor.Editor
    {
        private SerializedProperty recipe;
        private SerializedProperty regenerateOnValidate;

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

        private bool showAdvanced;

        private void OnEnable()
        {
            recipe =
                serializedObject.FindProperty(
                    "recipe");

            regenerateOnValidate =
                serializedObject.FindProperty(
                    "regenerateOnValidate");

            shapeSeed =
                recipe.FindPropertyRelative(
                    "shapeSeed");

            patchSize =
                recipe.FindPropertyRelative(
                    "patchSize");

            resolution =
                recipe.FindPropertyRelative(
                    "resolution");

            patchCoordinate =
                recipe.FindPropertyRelative(
                    "patchCoordinate");

            transitionDirection =
                recipe.FindPropertyRelative(
                    "transitionDirection");

            transitionHeight =
                recipe.FindPropertyRelative(
                    "transitionHeight");

            profile =
                recipe.FindPropertyRelative(
                    "profile");

            broadForm =
                recipe.FindPropertyRelative(
                    "broadForm");

            roughness =
                recipe.FindPropertyRelative(
                    "roughness");

            surfaceDetail =
                recipe.FindPropertyRelative(
                    "surfaceDetail");

            edgeBlend =
                recipe.FindPropertyRelative(
                    "edgeBlend");

            surfaceVariation =
                recipe.FindPropertyRelative(
                    "surfaceVariation");

            useModifiers =
                recipe.FindPropertyRelative(
                    "useModifiers");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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
                new GUIContent(
                    "Patch Size"));

            EditorGUILayout.PropertyField(
                resolution,
                new GUIContent(
                    "Resolution"));

            GroundPatchSize selectedSize =
                (GroundPatchSize)
                patchSize.enumValueIndex;

            GroundResolution selectedResolution =
                (GroundResolution)
                resolution.enumValueIndex;

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
                $"{verticesPerSide} × " +
                $"{verticesPerSide} vertices, " +
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
                       (int)
                       GroundTransitionDirection.None))
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
                new GUIContent(
                    "Profile"));

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
                    "Variation written to vertex colour red."));
        }

        private void DrawModifierSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Modifiers",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                useModifiers,
                new GUIContent(
                    "Use Modifiers"));

            if (targets.Length == 1)
            {
                GeneratedGround ground =
                    target as GeneratedGround;

                if (ground != null)
                {
                    EditorGUILayout.LabelField(
                        "Found Modifiers",
                        ground.ModifierCount.ToString());
                }
            }

            EditorGUILayout.HelpBox(
                "GroundModifier components are discovered below this " +
                "GeneratedGround object in the Hierarchy.",
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

            if (GUILayout.Button(
                    "New Shape"))
            {
                ApplyToTargets(
                    "New Generated Ground Shape",
                    ground =>
                        ground.CreateNewShape());
            }

            if (GUILayout.Button(
                    "Regenerate"))
            {
                ApplyToTargets(
                    "Regenerate Generated Ground",
                    ground =>
                        ground.Regenerate());
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(
                    "Find Modifiers"))
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

        private void ApplyToTargets(
            string undoName,
            GroundAction action)
        {
            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                GeneratedGround ground =
                    targets[i] as
                    GeneratedGround;

                if (ground == null)
                {
                    continue;
                }

                Undo.RecordObject(
                    ground,
                    undoName);

                action(ground);

                EditorUtility.SetDirty(
                    ground);
            }

            serializedObject.Update();
            Repaint();
            SceneView.RepaintAll();
        }

        private delegate void GroundAction(
            GeneratedGround ground);
    }

    [CustomEditor(typeof(GroundModifier))]
    [CanEditMultipleObjects]
    internal sealed class GroundModifierEditor :
        UnityEditor.Editor
    {
        private SerializedProperty mode;
        private SerializedProperty shape;
        private SerializedProperty priority;
        private SerializedProperty strength;
        private SerializedProperty blendDistance;
        private SerializedProperty circleRadius;
        private SerializedProperty boxSize;
        private SerializedProperty heightAmount;
        private SerializedProperty preserveDetail;
        private SerializedProperty splineContainer;
        private SerializedProperty riverSplineResolution;
        private SerializedProperty riverWidth;
        private SerializedProperty riverBankWidth;
        private SerializedProperty riverDepth;
        private SerializedProperty riverBedFlatness;
        private SerializedProperty riverBankStyle;
        private SerializedProperty autoRegenerateParent;

        private void OnEnable()
        {
            mode =
                serializedObject.FindProperty(
                    "mode");

            shape =
                serializedObject.FindProperty(
                    "shape");

            priority =
                serializedObject.FindProperty(
                    "priority");

            strength =
                serializedObject.FindProperty(
                    "strength");

            blendDistance =
                serializedObject.FindProperty(
                    "blendDistance");

            circleRadius =
                serializedObject.FindProperty(
                    "circleRadius");

            boxSize =
                serializedObject.FindProperty(
                    "boxSize");

            heightAmount =
                serializedObject.FindProperty(
                    "heightAmount");

            preserveDetail =
                serializedObject.FindProperty(
                    "preserveDetail");

            splineContainer =
                serializedObject.FindProperty(
                    "splineContainer");

            riverSplineResolution =
                serializedObject.FindProperty(
                    "riverSplineResolution");

            riverWidth =
                serializedObject.FindProperty(
                    "riverWidth");

            riverBankWidth =
                serializedObject.FindProperty(
                    "riverBankWidth");

            riverDepth =
                serializedObject.FindProperty(
                    "riverDepth");

            riverBedFlatness =
                serializedObject.FindProperty(
                    "riverBedFlatness");

            riverBankStyle =
                serializedObject.FindProperty(
                    "riverBankStyle");

            autoRegenerateParent =
                serializedObject.FindProperty(
                    "autoRegenerateParent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(
                "Influence",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                mode);

            EditorGUILayout.PropertyField(
                priority);

            EditorGUILayout.Slider(
                strength,
                0f,
                1f,
                new GUIContent(
                    "Strength"));

            GroundModifierMode selectedMode =
                (GroundModifierMode)
                mode.enumValueIndex;

            if (selectedMode ==
                GroundModifierMode.RiverBed)
            {
                DrawRiverSettings();
            }
            else
            {
                DrawStandardModifierSettings(
                    selectedMode);
            }

            EditorGUILayout.Space(8f);

            EditorGUILayout.PropertyField(
                autoRegenerateParent,
                new GUIContent(
                    "Live Parent Regeneration"));

            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button(
                    selectedMode ==
                    GroundModifierMode.RiverBed
                        ? "Regenerate River and Ground"
                        : "Regenerate Parent Ground"))
            {
                for (int i = 0;
                     i < targets.Length;
                     i++)
                {
                    GroundModifier modifier =
                        targets[i] as
                        GroundModifier;

                    if (modifier == null)
                    {
                        continue;
                    }

                    modifier.RegenerateParentGround();

                    EditorUtility.SetDirty(
                        modifier);
                }

                SceneView.RepaintAll();
            }
        }

        private void DrawStandardModifierSettings(
            GroundModifierMode selectedMode)
        {
            int standardShapeIndex =
                Mathf.Clamp(
                    shape.enumValueIndex,
                    0,
                    1);

            standardShapeIndex =
                EditorGUILayout.Popup(
                    new GUIContent(
                        "Shape"),
                    standardShapeIndex,
                    new[]
                    {
                        "Circle",
                        "Box"
                    });

            shape.enumValueIndex =
                standardShapeIndex;

            EditorGUILayout.Slider(
                blendDistance,
                0f,
                20f,
                new GUIContent(
                    "Blend Distance"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Shape",
                EditorStyles.boldLabel);

            GroundModifierShape selectedShape =
                (GroundModifierShape)
                shape.enumValueIndex;

            if (selectedShape ==
                GroundModifierShape.Circle)
            {
                EditorGUILayout.Slider(
                    circleRadius,
                    0.25f,
                    40f,
                    new GUIContent(
                        "Radius"));
            }
            else
            {
                EditorGUILayout.PropertyField(
                    boxSize,
                    new GUIContent(
                        "Box Size"));
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Mode Settings",
                EditorStyles.boldLabel);

            if (selectedMode ==
                GroundModifierMode.Flatten)
            {
                EditorGUILayout.Slider(
                    preserveDetail,
                    0f,
                    1f,
                    new GUIContent(
                        "Preserve Detail"));

                EditorGUILayout.HelpBox(
                    "The modifier Transform Y position is the " +
                    "flatten target height.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.Slider(
                    heightAmount,
                    0f,
                    12f,
                    new GUIContent(
                        selectedMode ==
                        GroundModifierMode.Raise
                            ? "Raise Amount"
                            : "Lower Amount"));
            }
        }

        private void DrawRiverSettings()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "River Path",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                splineContainer,
                new GUIContent(
                    "Spline Container"));

            EditorGUILayout.PropertyField(
                riverSplineResolution,
                new GUIContent(
                    "Spline Sampling"));

            EditorGUILayout.HelpBox(
                "The spline's Y position is the water-surface " +
                "level. The ground is carved beneath it.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Channel",
                EditorStyles.boldLabel);

            EditorGUILayout.Slider(
                riverWidth,
                0.5f,
                20f,
                new GUIContent(
                    "River Width"));

            EditorGUILayout.Slider(
                riverBankWidth,
                0.25f,
                20f,
                new GUIContent(
                    "Bank Width"));

            EditorGUILayout.Slider(
                riverDepth,
                0.1f,
                8f,
                new GUIContent(
                    "Bed Depth"));

            EditorGUILayout.Slider(
                riverBedFlatness,
                0f,
                1f,
                new GUIContent(
                    "Bed Flatness"));

            EditorGUILayout.PropertyField(
                riverBankStyle,
                new GUIContent(
                    "Bank Style"));
        }
    }

    [CustomEditor(typeof(GeneratedRiver))]
    [CanEditMultipleObjects]
    internal sealed class GeneratedRiverEditor :
        UnityEditor.Editor
    {
        private SerializedProperty riverBedModifier;
        private SerializedProperty surfaceState;
        private SerializedProperty meshResolution;
        private SerializedProperty widthInset;
        private SerializedProperty surfaceOffset;
        private SerializedProperty flowTileLength;
        private SerializedProperty generateCollider;
        private SerializedProperty regenerateOnValidate;

        private void OnEnable()
        {
            riverBedModifier =
                serializedObject.FindProperty(
                    "riverBedModifier");

            surfaceState =
                serializedObject.FindProperty(
                    "surfaceState");

            meshResolution =
                serializedObject.FindProperty(
                    "meshResolution");

            widthInset =
                serializedObject.FindProperty(
                    "widthInset");

            surfaceOffset =
                serializedObject.FindProperty(
                    "surfaceOffset");

            flowTileLength =
                serializedObject.FindProperty(
                    "flowTileLength");

            generateCollider =
                serializedObject.FindProperty(
                    "generateCollider");

            regenerateOnValidate =
                serializedObject.FindProperty(
                    "regenerateOnValidate");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField(
                "River Source",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                riverBedModifier,
                new GUIContent(
                    "River Bed Modifier"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Surface",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                surfaceState);

            EditorGUILayout.PropertyField(
                meshResolution,
                new GUIContent(
                    "Mesh Resolution"));

            EditorGUILayout.Slider(
                widthInset,
                0f,
                1f,
                new GUIContent(
                    "Bank Inset"));

            EditorGUILayout.Slider(
                surfaceOffset,
                -0.25f,
                0.5f,
                new GUIContent(
                    "Surface Offset"));

            EditorGUILayout.Slider(
                flowTileLength,
                0.25f,
                20f,
                new GUIContent(
                    "Flow Tile Length"));

            RiverSurfaceState selectedState =
                (RiverSurfaceState)
                surfaceState.enumValueIndex;

            using (new EditorGUI.DisabledScope(
                       selectedState ==
                       RiverSurfaceState.Frozen))
            {
                EditorGUILayout.PropertyField(
                    generateCollider,
                    new GUIContent(
                        "Running-Water Collider"));
            }

            EditorGUILayout.PropertyField(
                regenerateOnValidate,
                new GUIContent(
                    "Live Regeneration"));

            serializedObject.ApplyModifiedProperties();

            if (targets.Length == 1)
            {
                GeneratedRiver river =
                    target as GeneratedRiver;

                if (river != null &&
                    !river.IsConfigured)
                {
                    EditorGUILayout.HelpBox(
                        "Assign a GroundModifier configured as " +
                        "River Bed and a valid Spline Container.",
                        MessageType.Warning);
                }
            }

            EditorGUILayout.Space(10f);

            if (GUILayout.Button(
                    "Regenerate River Surface"))
            {
                for (int i = 0;
                     i < targets.Length;
                     i++)
                {
                    GeneratedRiver river =
                        targets[i] as
                        GeneratedRiver;

                    if (river == null)
                    {
                        continue;
                    }

                    Undo.RecordObject(
                        river,
                        "Regenerate River Surface");

                    river.Regenerate();

                    EditorUtility.SetDirty(
                        river);
                }

                SceneView.RepaintAll();
            }
        }
    }

    [InitializeOnLoad]
    internal static class GroundSplineAutoRefresh
    {
        static GroundSplineAutoRefresh()
        {
            EditorSplineUtility
                .AfterSplineWasModified +=
                HandleSplineModified;
        }

        private static void HandleSplineModified(
            Spline spline)
        {
            GroundModifier[] modifiers =
                Object.FindObjectsByType<
                    GroundModifier>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < modifiers.Length;
                 i++)
            {
                GroundModifier modifier =
                    modifiers[i];

                if (modifier == null ||
                    !modifier.UsesSpline(spline))
                {
                    continue;
                }

                modifier.RegenerateParentGround();

                EditorUtility.SetDirty(
                    modifier);
            }

            SceneView.RepaintAll();
        }
    }
}
