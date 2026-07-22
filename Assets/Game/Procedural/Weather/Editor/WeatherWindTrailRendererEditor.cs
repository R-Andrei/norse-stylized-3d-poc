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
        private static readonly Color OutsideViewportColor =
            new Color(0.55f, 0.58f, 0.62f, 0.55f);
        private static readonly Color BelowWindFloorColor =
            new Color(0.20f, 0.55f, 1.00f, 0.90f);
        private static readonly Color TooCloseColor =
            new Color(1.00f, 0.48f, 0.10f, 0.95f);
        private static readonly Color EligibleColor =
            new Color(0.25f, 1.00f, 0.45f, 0.95f);
        private static readonly Color SelectedColor =
            new Color(1.00f, 0.92f, 0.15f, 1.00f);
        private static readonly Color TrailPathColor =
            new Color(0.72f, 0.96f, 1.00f, 0.98f);

        private bool showVisualCalibration = true;
        private bool showPlacement = true;
        private bool showAdvancedGeneration;
        private bool showSceneDiagnostics = true;
        private bool showReport = true;
        private Vector2 reportScrollPosition;

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
            EditorGUI.BeginChangeCheck();
            DrawScriptReference();
            DrawVisualCalibration(trailRenderer);
            DrawPlacementAndDensity();
            DrawAdvancedGeneration();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(trailRenderer);
                SceneView.RepaintAll();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (upgradedBaseline)
            {
                EditorGUILayout.HelpBox(
                    "Updated exact earlier baseline values to the V0.6 " +
                    "length-resolved lifecycle. Existing non-default tuning " +
                    "was preserved.",
                    MessageType.Info);
            }

            if (assignedDefaultShader)
            {
                EditorGUILayout.HelpBox(
                    "Assigned the default serialized wind-trail shader: " +
                    "SH_WeatherWindTrails.",
                    MessageType.Info);
            }

            DrawRuntimeStatus(trailRenderer);
            DrawActions(trailRenderer);
            DrawReport(trailRenderer);
            DrawSceneDiagnosticControls();
        }

        private void DrawScriptReference()
        {
            SerializedProperty scriptProperty =
                serializedObject.FindProperty("m_Script");
            if (scriptProperty == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(scriptProperty);
            }
        }

        private void DrawVisualCalibration(
            WeatherWindTrailRenderer trailRenderer)
        {
            EditorGUILayout.Space();
            showVisualCalibration = EditorGUILayout.Foldout(
                showVisualCalibration,
                "Visual Calibration",
                true);
            if (!showVisualCalibration)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawProperty("trailShader");
                DrawProperty("trailColor");
                DrawProperty("trailOpacity");
                DrawProperty("uniformBodyOpacity");
                SerializedProperty uniformBodyProperty =
                    serializedObject.FindProperty("uniformBodyOpacity");
                using (new EditorGUI.DisabledScope(
                    uniformBodyProperty != null &&
                    uniformBodyProperty.boolValue))
                {
                    DrawProperty("edgeSoftness");
                }

                DrawMinMaxProperties(
                    "Width",
                    "minimumWidthMetres",
                    "maximumWidthMetres");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Lifecycle",
                    EditorStyles.miniBoldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawMinMaxProperties(
                        "Alive Duration",
                        "minimumAliveDurationSeconds",
                        "maximumAliveDurationSeconds");
                    DrawMinMaxProperties(
                        "Travel Speed",
                        "minimumPresentationSpeed",
                        "maximumPresentationSpeed");
                    DrawMinMaxProperties(
                        "Visible Body Length",
                        "minimumVisibleBodyLengthMetres",
                        "maximumVisibleBodyLengthMetres");
                    DrawProperty("lifecycleTipSpeedAllowance");
                    DrawProperty("pointedEndLengthMetres");
                    DrawResolvedLifecycleSummary(trailRenderer);
                }

                DrawMinMaxProperties(
                    "Altitude",
                    "minimumAltitudeMetres",
                    "maximumAltitudeMetres");
                DrawProperty("maximumVerticalDeviationMetres");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Occasional Broad Waves",
                    EditorStyles.miniBoldLabel);
                using (new EditorGUI.IndentLevelScope())
                {
                    DrawProperty("occasionalBroadWaveChance");
                    DrawProperty("occasionalBroadWaveStrengthMetres");
                }

                DrawProperty("strengthOpacityInfluence");
                DrawProperty("variationOpacityInfluence");
            }
        }

        private static void DrawResolvedLifecycleSummary(
            WeatherWindTrailRenderer trailRenderer)
        {
            trailRenderer.GetResolvedLifecycleDurationRanges(
                out Vector2 spawnRange,
                out Vector2 despawnRange,
                out Vector2 totalRange);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Resolved Timing",
                EditorStyles.miniBoldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField(
                    "Spawn Duration",
                    $"{spawnRange.x:0.##}–{spawnRange.y:0.##} s");
                EditorGUILayout.TextField(
                    "Despawn Duration",
                    $"{despawnRange.x:0.##}–{despawnRange.y:0.##} s");
                EditorGUILayout.TextField(
                    "Total Lifetime",
                    $"{totalRange.x:0.##}–{totalRange.y:0.##} s");
            }

            EditorGUILayout.HelpBox(
                "Spawn and despawn durations are resolved from body length, " +
                "travel speed, and lifecycle tip-speed allowance. Accepted " +
                "trails may reduce speed, alive duration, or body length to " +
                "guarantee the complete lifecycle fits their generated path.",
                MessageType.None);
        }

        private void DrawPlacementAndDensity()
        {
            EditorGUILayout.Space();
            showPlacement = EditorGUILayout.Foldout(
                showPlacement,
                "Placement & Density",
                true);
            if (!showPlacement)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawProperty("maximumActiveTrails");
                DrawProperty("spawnAttemptsPerSecond");
                DrawProperty("minimumWindStrength");
                DrawProperty("minimumTrailSeparationMetres");
                DrawProperty("separationCooldownSeconds");
            }
        }

        private void DrawAdvancedGeneration()
        {
            EditorGUILayout.Space();
            showAdvancedGeneration = EditorGUILayout.Foldout(
                showAdvancedGeneration,
                "Advanced Generation",
                true);
            if (!showAdvancedGeneration)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.LabelField(
                    "Candidate Selection",
                    EditorStyles.boldLabel);
                DrawProperty("candidateGridResolution");
                DrawProperty("strongestCandidateSubset");
                DrawProperty("candidateCellJitter");
                DrawProperty("trailSeed");
                DrawProperty("strengthScoreExponent");
                DrawProperty("spacingScoreExponent");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Streamline Construction",
                    EditorStyles.boldLabel);
                DrawProperty("maximumCentrelinePoints");
                DrawProperty("integrationStepMetres");
                DrawProperty("minimumPathWindStrength");
                DrawProperty("minimumCompletedPathLengthMetres");
                DrawProperty("maximumTurnDegreesPerSegment");
                DrawProperty("selfApproachDistanceMetres");
                DrawProperty("minimumSegmentWindAlignment");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField(
                    "Camera Relevance",
                    EditorStyles.boldLabel);
                DrawProperty("candidateViewportMargin");
            }
        }

        private void DrawMinMaxProperties(
            string label,
            string minimumPropertyName,
            string maximumPropertyName)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                DrawProperty(minimumPropertyName);
                DrawProperty(maximumPropertyName);
            }
        }

        private void DrawProperty(string propertyName)
        {
            SerializedProperty property =
                serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, true);
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

        private void DrawRuntimeStatus(WeatherWindTrailRenderer trailRenderer)
        {
            WeatherWindDomain statusDomain = trailRenderer.WeatherDomain != null
                ? trailRenderer.WeatherDomain
                : trailRenderer.GetComponent<WeatherWindDomain>();
            Camera statusCamera = trailRenderer.TargetCamera != null
                ? trailRenderer.TargetCamera
                : statusDomain != null ? statusDomain.TargetCamera : null;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Weather Wind Trail Status",
                EditorStyles.boldLabel);

            EditorGUILayout.LabelField(
                "Runtime State",
                !Application.isPlaying
                    ? "Editor idle"
                    : trailRenderer.RuntimeReady ? "Ready" : "Not ready");
            EditorGUILayout.LabelField(
                "Weather Domain",
                statusDomain != null ? statusDomain.name : "None");
            EditorGUILayout.LabelField(
                "Target Camera",
                statusCamera != null ? statusCamera.name : "None");
            EditorGUILayout.LabelField(
                "Trail Shader",
                trailRenderer.TrailShader != null
                    ? trailRenderer.TrailShader.name
                    : "None");
            EditorGUILayout.LabelField(
                "Active / Maximum Trails",
                $"{trailRenderer.ActiveTrailCount} / " +
                trailRenderer.MaximumActiveTrails);
            EditorGUILayout.LabelField(
                "Last Visible / Eligible Candidates",
                $"{trailRenderer.LastVisibleCandidateCount} / " +
                trailRenderer.LastEligibleCandidateCount);
            EditorGUILayout.LabelField(
                "Mesh Vertex / Index Capacity",
                $"{trailRenderer.MeshVertexCapacity} / " +
                trailRenderer.MeshIndexCapacity);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Editor idle. Wind-trail resources, spawning, paths, mesh " +
                    "uploads, and rendering run only in Play Mode.",
                    MessageType.Info);
            }
            else if (!string.IsNullOrEmpty(trailRenderer.LastError))
            {
                EditorGUILayout.HelpBox(
                    trailRenderer.LastError,
                    MessageType.Error);
            }
        }

        private void DrawActions(WeatherWindTrailRenderer trailRenderer)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Weather Wind Trail Actions",
                EditorStyles.boldLabel);

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
        }

        private void DrawReport(WeatherWindTrailRenderer trailRenderer)
        {
            EditorGUILayout.Space();
            showReport = EditorGUILayout.Foldout(
                showReport,
                "Weather Wind Trail Report",
                true);
            if (!showReport)
            {
                return;
            }

            string report = trailRenderer.BuildComprehensiveReport();
            reportScrollPosition = EditorGUILayout.BeginScrollView(
                reportScrollPosition,
                GUILayout.MinHeight(220f),
                GUILayout.MaxHeight(320f));

            float availableWidth = Mathf.Max(
                100f,
                EditorGUIUtility.currentViewWidth - 44f);
            float reportHeight = Mathf.Max(
                220f,
                EditorStyles.textArea.CalcHeight(
                    new GUIContent(report),
                    availableWidth));
            EditorGUILayout.SelectableLabel(
                report,
                EditorStyles.textArea,
                GUILayout.Height(reportHeight));
            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Copy Report"))
            {
                EditorGUIUtility.systemCopyBuffer = report;
                Debug.Log(
                    "[Weather Wind Trails V0.6] Report copied to clipboard.",
                    trailRenderer);
            }
        }

        private void DrawSceneDiagnosticControls()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Scene Diagnostics",
                EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUI.BeginChangeCheck();
                showSceneDiagnostics = EditorGUILayout.ToggleLeft(
                    "Show Scene Diagnostics",
                    showSceneDiagnostics);
                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Candidate and path geometry is available only in Play Mode.",
                    MessageType.None);
            }
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
                case WeatherWindTrailCandidateStatus.OutsideViewport:
                    return OutsideViewportColor;
                case WeatherWindTrailCandidateStatus.BelowWindFloor:
                    return BelowWindFloorColor;
                case WeatherWindTrailCandidateStatus.TooClose:
                    return TooCloseColor;
                case WeatherWindTrailCandidateStatus.Eligible:
                    return EligibleColor;
                case WeatherWindTrailCandidateStatus.Selected:
                    return SelectedColor;
                default:
                    return Color.clear;
            }
        }
    }
}
