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

        private SerializedProperty coldGreyStoneMaterial;
        private SerializedProperty darkWetRiverStoneMaterial;
        private SerializedProperty paleFrostStoneMaterial;
        private SerializedProperty blackSacredStoneMaterial;
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
        private SerializedProperty surfaceFeatureVisibility;
        private SerializedProperty edgeWearAmount;
        private SerializedProperty edgeWearWidth;
        private SerializedProperty edgeWearCoverage;
        private SerializedProperty edgeWearSoftness;
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
            surfaceFeatureVisibility = serializedObject.FindProperty(
                "surfaceFeatureVisibility");
            edgeWearAmount = serializedObject.FindProperty(
                "edgeWearAmount");
            edgeWearWidth = serializedObject.FindProperty(
                "edgeWearWidth");
            edgeWearCoverage = serializedObject.FindProperty(
                "edgeWearCoverage");
            edgeWearSoftness = serializedObject.FindProperty(
                "edgeWearSoftness");
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

            DrawPropertiesExcluding(
                serializedObject,
                "m_Script",
                "surfaceMaskBaseLift",
                "creviceReach",
                "creviceSmoothness",
                "creviceBreakup",
                "dirtCrawlReach",
                "dirtCoverage",
                "surfaceFeatureVisibility",
                "edgeWearAmount",
                "edgeWearWidth",
                "edgeWearCoverage",
                "edgeWearSoftness",
                "creaseAmount",
                "creaseWidth",
                "creaseLength",
                "creaseBranching",
                "creaseSoftness",
                "riverInteraction");

            DrawSurfaceMaskTuning();
            DrawSurfaceFeatureLines();
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

        private void DrawSurfaceMaskTuning()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Surface Mask Tuning",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "These controls tune generated CreviceBase and DirtDeposit " +
                "surface masks per object. Crevice Reach controls crawl " +
                "height; Crevice Smoothness controls the fade length. The four Response controls affect final normal rendering only and let each accepted mask type be tuned independently.",
                MessageType.Info);

            EditorGUILayout.PropertyField(surfaceMaskBaseLift);
            EditorGUILayout.PropertyField(creviceReach);
            EditorGUILayout.PropertyField(creviceSmoothness);
            EditorGUILayout.PropertyField(creviceBreakup);
            EditorGUILayout.PropertyField(dirtCrawlReach);
            EditorGUILayout.PropertyField(dirtCoverage);
            EditorGUILayout.PropertyField(exposureResponse);
            EditorGUILayout.PropertyField(creviceResponse);
            EditorGUILayout.PropertyField(baseResponse);
            EditorGUILayout.PropertyField(dirtDepositResponse);
        }

        private void DrawSurfaceFeatureLines()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Surface Feature Lines",
                EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Generates raised overlay strips for ConvexEdgeWear and " +
                "ConcaveCrease debug validation only. These strips are not " +
                "used for normal stone rendering because their debug lift makes " +
                "them read as floating lines instead of surface-integrated " +
                "stone wear/cracks.",
                MessageType.Info);

            EditorGUILayout.PropertyField(edgeWearAmount);
            EditorGUILayout.PropertyField(edgeWearWidth);
            EditorGUILayout.PropertyField(edgeWearCoverage);
            EditorGUILayout.PropertyField(edgeWearSoftness);
            EditorGUILayout.Space(2f);
            EditorGUILayout.PropertyField(creaseAmount);
            EditorGUILayout.PropertyField(creaseWidth);
            EditorGUILayout.PropertyField(creaseLength);
            EditorGUILayout.PropertyField(creaseBranching);
            EditorGUILayout.PropertyField(creaseSoftness);
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
