using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherWindTrailRenderer), true)]
    public sealed class WeatherWindTrailRendererEditor : UnityEditor.Editor
    {
        private const string DefaultTrailShaderAssetPath =
            "Assets/Game/Rendering/Weather/Shaders/SH_WeatherWindTrails.shader";
        private static readonly Color OutsideWeatherFieldColor =
            new Color(0.55f, 0.58f, 0.62f, 0.55f);
        private static readonly Color BelowWindFloorColor =
            new Color(0.20f, 0.55f, 1.00f, 0.90f);
        private static readonly Color TooCloseColor =
            new Color(1.00f, 0.48f, 0.10f, 0.95f);
        private static readonly Color EligibleColor =
            new Color(0.25f, 1.00f, 0.45f, 0.95f);
        private static readonly Color SelectedColor =
            new Color(1.00f, 0.92f, 0.15f, 1.00f);
        private static readonly Color CameraEntryRejectedColor =
            new Color(0.45f, 0.45f, 0.45f, 0.55f);
        private static readonly Color DirectionMismatchColor =
            new Color(0.75f, 0.35f, 1.00f, 0.90f);
        private static readonly Color InsufficientRunwayColor =
            new Color(1.00f, 0.25f, 0.45f, 0.90f);
        private static readonly Color TrailPathColor =
            new Color(0.72f, 0.96f, 1.00f, 0.98f);

        private bool showAppearance;
        private bool showShapeAltitude;
        private bool showLifecycleTravel;
        private bool showWobbleShape;
        private bool showPopulationSeparation;
        private bool showCameraEntry;
        private bool showCandidateSelection;
        private bool showPathConstruction;
        private bool showDebugDiagnostics;
        private bool showActionsReports;
        private bool showLiveStatus;
        private bool showSceneDiagnostics;

        private void OnEnable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
            SceneView.duringSceneGui += DuringSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
        }

        public override bool RequiresConstantRepaint()
        {
            return Application.isPlaying;
        }

        public override void OnInspectorGUI()
        {
            var trailRenderer = (WeatherWindTrailRenderer)target;

            bool upgradedBaseline = TryUpgradeSerializedBaselineIfNeeded(
                trailRenderer);
            bool assignedDefaultShader = TryAssignDefaultShaderIfMissing();

            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);
            DrawImmediateWarnings(trailRenderer);

            DrawAppearance();
            DrawShapeAltitude();
            DrawLifecycleTravel(trailRenderer);
            DrawWobbleShape();
            DrawPopulationSeparation();
            DrawCameraEntry();
            DrawCandidateSelection();
            DrawPathConstruction();

            bool changed = serializedObject.ApplyModifiedProperties();
            if (changed)
            {
                EditorUtility.SetDirty(trailRenderer);
                SceneView.RepaintAll();
            }

            if (upgradedBaseline)
            {
                WeatherInspectorGui.Info(
                    "Updated exact earlier baseline values to the V0.9 " +
                    "direction-locked upwind-entry baseline. Existing non-default tuning was preserved.");
            }

            if (assignedDefaultShader)
            {
                WeatherInspectorGui.Info(
                    "Assigned the default serialized wind-trail shader: SH_WeatherWindTrails.");
            }

            DrawDebugDiagnostics();
            DrawActionsReports(trailRenderer);
            DrawLiveStatus(trailRenderer);
        }

        private static void DrawImmediateWarnings(
            WeatherWindTrailRenderer trailRenderer)
        {
            if (Application.isPlaying &&
                !string.IsNullOrEmpty(trailRenderer.LastError))
            {
                WeatherInspectorGui.Error(trailRenderer.LastError);
            }
        }

        private void DrawAppearance()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showAppearance,
                    "Appearance",
                    "Controls trail material, colour, opacity, and cross-width alpha response."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "trailShader",
                    "Trail Shader",
                    "Serialized shader reference retained in builds and used to create the hidden runtime material.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "trailColor",
                    "Trail Colour",
                    "HDR tint multiplied into the wind-trail shader output.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "trailOpacity",
                    "Trail Opacity",
                    "Overall alpha multiplier applied to every visible trail.");
                SerializedProperty uniformBody = WeatherInspectorGui.Property(
                    serializedObject,
                    "uniformBodyOpacity",
                    "Uniform Body Opacity",
                    "Keeps alpha spatially uniform across the visible body. Head and tail shaping then use physical width taper instead of broad alpha gradients.");
                using (new EditorGUI.DisabledScope(
                    uniformBody != null && uniformBody.boolValue))
                {
                    WeatherInspectorGui.Property(
                        serializedObject,
                        "edgeSoftness",
                        "Edge Softness",
                        "Cross-width alpha softness. Used only when Uniform Body Opacity is disabled.");
                }

                WeatherInspectorGui.Property(
                    serializedObject,
                    "strengthOpacityInfluence",
                    "Wind Strength Opacity Influence",
                    "How strongly local authoritative wind strength modulates trail opacity. Zero keeps opacity independent of wind strength.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "variationOpacityInfluence",
                    "Per-Trail Variation Opacity Influence",
                    "How strongly deterministic per-trail variation modulates opacity. Zero keeps all trails equally opaque before lifecycle shaping.");
            }
        }

        private void DrawShapeAltitude()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showShapeAltitude,
                    "Shape & Altitude",
                    "Controls physical width and vertical placement above the sampled world path."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Width",
                    "minimumWidthMetres",
                    "Minimum Width (m)",
                    "Smallest physical ribbon width selected for a spawned trail.",
                    "maximumWidthMetres",
                    "Maximum Width (m)",
                    "Largest physical ribbon width selected for a spawned trail.");
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Altitude",
                    "minimumAltitudeMetres",
                    "Minimum Altitude (m)",
                    "Minimum vertical offset above the generated world-space path.",
                    "maximumAltitudeMetres",
                    "Maximum Altitude (m)",
                    "Maximum vertical offset above the generated world-space path.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumVerticalDeviationMetres",
                    "Maximum Vertical Deviation (m)",
                    "Maximum deterministic vertical shape variation applied along a trail after its base altitude is chosen.");
            }
        }

        private void DrawLifecycleTravel(
            WeatherWindTrailRenderer trailRenderer)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLifecycleTravel,
                    "Lifecycle & Travel",
                    "Controls alive duration, motion speed, visible body length, and pointed endpoint timing."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Alive Duration",
                    "minimumAliveDurationSeconds",
                    "Minimum Alive Duration (s)",
                    "Minimum fully spawned lifetime before despawn begins.",
                    "maximumAliveDurationSeconds",
                    "Maximum Alive Duration (s)",
                    "Maximum fully spawned lifetime before despawn begins.");
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Travel Speed",
                    "minimumPresentationSpeed",
                    "Minimum Travel Speed (m/s)",
                    "Minimum presentation speed selected for an accepted trail.",
                    "maximumPresentationSpeed",
                    "Maximum Travel Speed (m/s)",
                    "Maximum presentation speed selected for an accepted trail.");
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Visible Body Length",
                    "minimumVisibleBodyLengthMetres",
                    "Minimum Visible Body Length (m)",
                    "Minimum fully visible trail-body length after spawn completes.",
                    "maximumVisibleBodyLengthMetres",
                    "Maximum Visible Body Length (m)",
                    "Maximum fully visible trail-body length after spawn completes.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "lifecycleTipSpeedAllowance",
                    "Lifecycle Tip-Speed Allowance (m/s)",
                    "Maximum extra endpoint speed used only while growing or shrinking a trail. Each trail clamps this allowance below its normal travel speed.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "pointedEndLengthMetres",
                    "Pointed End Length (m)",
                    "Physical distance over which each visible endpoint tapers to a point.");

                DrawResolvedLifecycleSummary(trailRenderer);
            }
        }

        private static void DrawResolvedLifecycleSummary(
            WeatherWindTrailRenderer trailRenderer)
        {
            trailRenderer.GetResolvedLifecycleDurationRanges(
                out Vector2 spawnRange,
                out Vector2 despawnRange,
                out Vector2 totalRange);

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Resolved Timing",
                EditorStyles.miniBoldLabel);
            WeatherInspectorGui.ReadOnlyRow(
                "Spawn Duration",
                $"{spawnRange.x:0.##}–{spawnRange.y:0.##} s");
            WeatherInspectorGui.ReadOnlyRow(
                "Despawn Duration",
                $"{despawnRange.x:0.##}–{despawnRange.y:0.##} s");
            WeatherInspectorGui.ReadOnlyRow(
                "Total Lifetime",
                $"{totalRange.x:0.##}–{totalRange.y:0.##} s");

            WeatherInspectorGui.Help(
                "Spawn and despawn durations are resolved from body length, " +
                "travel speed, and lifecycle tip-speed allowance. Accepted trails may reduce speed, alive duration, or body length so the complete lifecycle fits the generated path.");
        }

        private void DrawWobbleShape()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showWobbleShape,
                    "Wobble & Local Shape",
                    "Controls subtle lateral movement around the direction-locked Weather backbone."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Wobble Strength",
                    "minimumLateralWobbleStrengthMetres",
                    "Minimum Wobble Strength (m)",
                    "Minimum mandatory side-to-side displacement around the authoritative Weather streamline.",
                    "maximumLateralWobbleStrengthMetres",
                    "Maximum Wobble Strength (m)",
                    "Maximum mandatory side-to-side displacement around the authoritative Weather streamline.");
                WeatherInspectorGui.MinMaxProperties(
                    serializedObject,
                    "Wobble Wavelength",
                    "minimumLateralWobbleWavelengthMetres",
                    "Minimum Wobble Wavelength (m)",
                    "Minimum world distance covered by one complete lateral wobble cycle.",
                    "maximumLateralWobbleWavelengthMetres",
                    "Maximum Wobble Wavelength (m)",
                    "Maximum world distance covered by one complete lateral wobble cycle.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "occasionalLargerLoopChance",
                    "Larger Loop Chance",
                    "Probability that one normal wobble cycle receives a localized extra-amplitude boost.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "occasionalLargerLoopExtraStrengthMetres",
                    "Larger Loop Extra Strength (m)",
                    "Extra lateral displacement added only to the selected localized wobble cycle.");
            }
        }

        private void DrawPopulationSeparation()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showPopulationSeparation,
                    "Population & Separation",
                    "Controls trail capacity, spawn cadence, wind eligibility, and spacing."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumActiveTrails",
                    "Maximum Active Trails",
                    "Fixed maximum number of simultaneous wind trails and the primary mesh-capacity driver.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "spawnAttemptsPerSecond",
                    "Spawn Attempts / Second",
                    "Frequency at which the renderer evaluates the current candidate set while capacity is available.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumWindStrength",
                    "Minimum Wind Strength",
                    "Minimum authoritative Weather strength required for a candidate spawn location.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumTrailSeparationMetres",
                    "Minimum Trail Separation (m)",
                    "Minimum world-space distance from active trails and recent spawn cooldown records.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "separationCooldownSeconds",
                    "Separation Cooldown (s)",
                    "Duration for which a released trail location continues to repel new spawn candidates.");
            }
        }

        private void DrawCameraEntry()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCameraEntry,
                    "Camera Entry Placement",
                    "Biases spawns toward the upwind screen edge and requires useful visible downwind traversal."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "upwindSpawnBandDepth",
                    "Upwind Spawn Band Depth",
                    "Fraction of visible screen depth, measured from the upwind edge, that may contain on-screen spawn seeds.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "upwindEntryMarginViewport",
                    "Upwind Edge Margin",
                    "Small normalized viewport margin permitted beyond only the edge from which wind enters the screen.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "preferredVisibleRunwayMetres",
                    "Preferred Visible Runway (m)",
                    "Visible downwind travel distance that receives the maximum camera-entry score.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumAcceptedVisibleRunwayMetres",
                    "Minimum Accepted Runway (m)",
                    "Hard minimum visible downwind travel distance required before a candidate may spawn.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cameraEntryPreference",
                    "Camera Entry Preference",
                    "Strength of the score bias toward upwind positions with long visible downwind traversal.");

                WeatherInspectorGui.Help(
                    "Trails prefer the edge from which wind enters the camera and " +
                    "reject candidates that would immediately travel away from the visible view.");
            }
        }

        private void DrawCandidateSelection()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showCandidateSelection,
                    "Advanced Candidate Selection",
                    "Controls the deterministic screen candidate grid and score weighting."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "candidateGridResolution",
                    "Candidate Grid Resolution",
                    "Number of screen candidate cells per axis. Higher values evaluate more possible positions per spawn attempt.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strongestCandidateSubset",
                    "Strongest Candidate Subset",
                    "Number of highest-scoring eligible candidates retained for deterministic weighted selection.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "candidateCellJitter",
                    "Candidate Cell Jitter",
                    "Normalized deterministic offset within each candidate grid cell. Zero samples cell centres.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "trailSeed",
                    "Placement Seed",
                    "Deterministic seed for candidate jitter, trail selection, and per-trail presentation variation.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "strengthScoreExponent",
                    "Wind Strength Score Exponent",
                    "Exponent applied to normalized wind strength during candidate scoring. Higher values favor the strongest locations more aggressively.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "spacingScoreExponent",
                    "Separation Score Exponent",
                    "Exponent applied to normalized separation during candidate scoring. Higher values favor isolated locations more aggressively.");
            }
        }

        private void DrawPathConstruction()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showPathConstruction,
                    "Advanced Path Construction",
                    "Controls the authoritative Weather backbone, compatibility gates, and smooth render-curve density."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumCentrelinePoints",
                    "Maximum Backbone Points",
                    "Maximum number of authoritative Weather integration points retained per trail.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "integrationStepMetres",
                    "Backbone Step Length (m)",
                    "World distance advanced per authoritative Weather integration step. Smaller values increase Weather samples and path construction cost.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "renderCurveSubdivisionsPerBackboneSection",
                    "Render Subdivisions / Backbone Section",
                    "Smooth render samples generated between authoritative Weather points. This improves presentation smoothness without increasing Weather integration frequency.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumPathWindStrength",
                    "Minimum Path Wind Strength",
                    "Minimum authoritative Weather strength required at each accepted backbone continuation.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumCompletedPathLengthMetres",
                    "Minimum Completed Path Length (m)",
                    "Minimum generated path length required before a candidate may become an active trail.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumLocalWindDirectionMismatchDegrees",
                    "Maximum Local Direction Mismatch (°)",
                    "Maximum angular difference between the direction locked at birth and local authoritative wind sampled along the backbone.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "selfApproachDistanceMetres",
                    "Self-Approach Rejection Distance (m)",
                    "Minimum allowed distance between nonadjacent path sections before path construction rejects a self-approach.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "minimumSegmentWindAlignment",
                    "Minimum Segment / Wind Alignment",
                    "Minimum dot product between each generated segment and compatible local authoritative wind.");

                WeatherInspectorGui.Help(
                    "The dominant visible Weather direction is locked when a trail " +
                    "spawns. Local samples validate strength and compatibility but cannot cumulatively steer the backbone into a large curve.");
            }
        }

        private void DrawDebugDiagnostics()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showDebugDiagnostics,
                    "Debug & Diagnostics",
                    "Controls editor-only candidate and path geometry in Scene view."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUI.BeginChangeCheck();
                showSceneDiagnostics = EditorGUILayout.ToggleLeft(
                    new GUIContent(
                        "Show Scene Candidate Diagnostics",
                        "Displays the last candidate classifications and active generated paths while this component is selected in Play Mode."),
                    showSceneDiagnostics);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }

            if (!Application.isPlaying)
            {
                WeatherInspectorGui.Help(
                    "Candidate and path geometry is available only in Play Mode.");
            }
            else
            {
                WeatherInspectorGui.Help(
                    "Grey = outside Weather field or camera-entry region; blue = below wind floor; orange = too close; purple = incompatible direction; pink = insufficient runway; green = eligible; yellow = selected; cyan line = active path.");
            }
        }

        private void DrawActionsReports(
            WeatherWindTrailRenderer trailRenderer)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActionsReports,
                    "Actions & Reports",
                    "Manual reset and copyable comprehensive diagnostic report."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                using (new EditorGUI.DisabledScope(
                    !Application.isPlaying || !trailRenderer.ResourcesReady))
                {
                    if (GUILayout.Button("Reset Wind Trail Simulation"))
                    {
                        trailRenderer.ResetTrailSimulation();
                        Repaint();
                        SceneView.RepaintAll();
                    }
                }

                if (GUILayout.Button("Copy Comprehensive Trail Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        trailRenderer.BuildComprehensiveReport();
                    Debug.Log(
                        "[Weather Wind Trails V0.9] Report copied to clipboard.",
                        trailRenderer);
                }
            }
        }

        private void DrawLiveStatus(
            WeatherWindTrailRenderer trailRenderer)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLiveStatus,
                    "Live Status",
                    "Read-only runtime dependencies, population, candidate, and mesh-capacity state."))
            {
                return;
            }

            WeatherWindDomain statusDomain = trailRenderer.WeatherDomain != null
                ? trailRenderer.WeatherDomain
                : trailRenderer.GetComponent<WeatherWindDomain>();
            Camera statusCamera = trailRenderer.TargetCamera != null
                ? trailRenderer.TargetCamera
                : statusDomain != null ? statusDomain.TargetCamera : null;

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyRow(
                    "Runtime State",
                    !Application.isPlaying
                        ? "Editor idle"
                        : trailRenderer.RuntimeReady ? "Ready" : "Not ready");
                WeatherInspectorGui.ReadOnlyObject(
                    "Weather Domain",
                    statusDomain);
                WeatherInspectorGui.ReadOnlyObject(
                    "Target Camera",
                    statusCamera);
                WeatherInspectorGui.ReadOnlyObject(
                    "Trail Shader",
                    trailRenderer.TrailShader);
                WeatherInspectorGui.ReadOnlyRow(
                    "Active / Maximum Trails",
                    $"{trailRenderer.ActiveTrailCount} / {trailRenderer.MaximumActiveTrails}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Visible / Eligible Candidates",
                    $"{trailRenderer.LastVisibleCandidateCount} / {trailRenderer.LastEligibleCandidateCount}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Mesh Vertex / Index Capacity",
                    $"{trailRenderer.MeshVertexCapacity} / {trailRenderer.MeshIndexCapacity}");

                if (!Application.isPlaying)
                {
                    WeatherInspectorGui.Info(
                        "Editor idle. Wind-trail resources, spawning, paths, mesh uploads, and rendering run only in Play Mode.");
                }
            }
        }

        private bool TryUpgradeSerializedBaselineIfNeeded(
            WeatherWindTrailRenderer trailRenderer)
        {
            if (trailRenderer == null ||
                trailRenderer.SerializedBaselineVersion >=
                    WeatherWindTrailRenderer.CurrentBaselineVersion)
            {
                return false;
            }

            Undo.RecordObject(
                trailRenderer,
                "Upgrade Weather Wind Trail Baseline");
            bool changed = trailRenderer.UpgradeSerializedBaselineIfNeeded();
            serializedObject.UpdateIfRequiredOrScript();
            EditorUtility.SetDirty(trailRenderer);
            return changed;
        }

        private bool TryAssignDefaultShaderIfMissing()
        {
            serializedObject.UpdateIfRequiredOrScript();
            SerializedProperty shaderProperty =
                serializedObject.FindProperty("trailShader");
            if (shaderProperty == null ||
                shaderProperty.objectReferenceValue != null)
            {
                return false;
            }

            Shader defaultShader = AssetDatabase.LoadAssetAtPath<Shader>(
                DefaultTrailShaderAssetPath);
            if (defaultShader == null)
            {
                return false;
            }

            Undo.RecordObject(
                target,
                "Assign Default Weather Wind Trail Shader");
            shaderProperty.objectReferenceValue = defaultShader;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
            return true;
        }

        private void DuringSceneGui(SceneView sceneView)
        {
            var trailRenderer = target as WeatherWindTrailRenderer;
            if (!Application.isPlaying ||
                !showSceneDiagnostics ||
                trailRenderer == null ||
                Selection.activeGameObject != trailRenderer.gameObject)
            {
                return;
            }

            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            try
            {
                DrawCandidates(trailRenderer);
                DrawActiveTrails(trailRenderer);
            }
            finally
            {
                Handles.zTest = previousZTest;
            }

            sceneView.Repaint();
        }

        private static void DrawCandidates(
            WeatherWindTrailRenderer trailRenderer)
        {
            int candidateCount = trailRenderer.LastCandidateCount;
            for (int candidateIndex = 0;
                 candidateIndex < candidateCount;
                 candidateIndex++)
            {
                if (!trailRenderer.TryGetLastCandidate(
                        candidateIndex,
                        out Vector3 worldPosition,
                        out _,
                        out _,
                        out _,
                        out WeatherWindTrailCandidateStatus status) ||
                    status == WeatherWindTrailCandidateStatus.NotEvaluated)
                {
                    continue;
                }

                Handles.color = CandidateColor(status);
                float size = HandleUtility.GetHandleSize(worldPosition) *
                    (status == WeatherWindTrailCandidateStatus.Selected
                        ? 0.075f
                        : 0.045f);
                Handles.DotHandleCap(
                    0,
                    worldPosition,
                    Quaternion.identity,
                    size,
                    EventType.Repaint);
            }
        }

        private static void DrawActiveTrails(
            WeatherWindTrailRenderer trailRenderer)
        {
            Handles.color = TrailPathColor;
            for (int trailIndex = 0;
                 trailIndex < trailRenderer.MaximumActiveTrails;
                 trailIndex++)
            {
                int pointCount = trailRenderer.GetTrailPointCount(trailIndex);
                if (pointCount < 2 ||
                    !trailRenderer.TryGetTrailPoint(
                        trailIndex,
                        0,
                        out Vector3 previousPoint))
                {
                    continue;
                }

                float endpointSize =
                    HandleUtility.GetHandleSize(previousPoint) * 0.035f;
                Handles.DotHandleCap(
                    0,
                    previousPoint,
                    Quaternion.identity,
                    endpointSize,
                    EventType.Repaint);

                for (int pointIndex = 1;
                     pointIndex < pointCount;
                     pointIndex++)
                {
                    if (!trailRenderer.TryGetTrailPoint(
                            trailIndex,
                            pointIndex,
                            out Vector3 currentPoint))
                    {
                        break;
                    }

                    Handles.DrawAAPolyLine(
                        3f,
                        previousPoint,
                        currentPoint);
                    previousPoint = currentPoint;
                }

                endpointSize =
                    HandleUtility.GetHandleSize(previousPoint) * 0.035f;
                Handles.DotHandleCap(
                    0,
                    previousPoint,
                    Quaternion.identity,
                    endpointSize,
                    EventType.Repaint);
            }
        }

        private static Color CandidateColor(
            WeatherWindTrailCandidateStatus status)
        {
            switch (status)
            {
                case WeatherWindTrailCandidateStatus.OutsideWeatherField:
                    return OutsideWeatherFieldColor;
                case WeatherWindTrailCandidateStatus.BelowWindFloor:
                    return BelowWindFloorColor;
                case WeatherWindTrailCandidateStatus.TooClose:
                    return TooCloseColor;
                case WeatherWindTrailCandidateStatus.Eligible:
                    return EligibleColor;
                case WeatherWindTrailCandidateStatus.Selected:
                    return SelectedColor;
                case WeatherWindTrailCandidateStatus.OutsideCameraEntryRegion:
                    return CameraEntryRejectedColor;
                case WeatherWindTrailCandidateStatus.IncompatibleWindDirection:
                    return DirectionMismatchColor;
                case WeatherWindTrailCandidateStatus.InsufficientVisibleRunway:
                    return InsufficientRunwayColor;
                default:
                    return Color.clear;
            }
        }
    }
}
