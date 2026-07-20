using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Weather.Editor
{
    [CustomEditor(typeof(WeatherWindDomain), true)]
    public sealed class WeatherWindDomainEditor : UnityEditor.Editor
    {
        private const double ReadbackIntervalSeconds = 0.1;

        private Color[] cachedResponsePixels;
        private double nextReadbackTime;
        private bool readbackPending;
        private float cachedResponseSimulationTime;

        private void OnEnable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
            SceneView.duringSceneGui += DuringSceneGui;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= DuringSceneGui;
            cachedResponsePixels = null;
            readbackPending = false;
        }

        public override void OnInspectorGUI()
        {
            var domain = (WeatherWindDomain)target;
            int simulationHashBefore = domain.SimulationConfigurationHash;
            WeatherWindDebugView debugViewBefore = domain.DebugView;
            bool changed = DrawDefaultInspector();
            if (changed)
            {
                bool simulationChanged = simulationHashBefore !=
                    domain.SimulationConfigurationHash;
                if (simulationChanged)
                {
                    domain.RequestRebuild();
                    cachedResponsePixels = null;
                    nextReadbackTime = 0.0;
                }
                else if (debugViewBefore != domain.DebugView &&
                         domain.DebugView == WeatherWindDebugView.ResponseError)
                {
                    nextReadbackTime = 0.0;
                }

                EditorUtility.SetDirty(domain);
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Weather Wind Actions", EditorStyles.boldLabel);

            if (target.GetType().FullName ==
                "ProgrammaticStylized3D.Vegetation.VegetationBenchmarkWindProvider")
            {
                EditorGUILayout.HelpBox(
                    "This is the preserved legacy scene component. Its old analytical " +
                    "test-wind implementation has been removed; it now runs the shared " +
                    "Weather Wind Domain through inheritance. Replace it with Weather " +
                    "Wind Domain later through the Unity Inspector when convenient.",
                    MessageType.Warning);
            }

            if (GUILayout.Button("Reset Weather Wind Field"))
            {
                domain.ResetField();
                cachedResponsePixels = null;
                nextReadbackTime = 0.0;
                EditorUtility.SetDirty(domain);
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Copy Weather Wind Report"))
            {
                EditorGUIUtility.systemCopyBuffer = domain.BuildComprehensiveReport();
                Debug.Log(
                    "[Weather Wind V0] XZ wind-domain report copied to clipboard.",
                    domain);
            }

            EditorGUILayout.Space();
            if (domain.ResourcesReady)
            {
                EditorGUILayout.HelpBox(
                    $"Ready: {domain.FieldResolution} × {domain.FieldResolution}, " +
                    $"{domain.CellSizeMetres:0.###} m/cell, " +
                    $"{domain.FieldWorldSizeMetres:0.###} m world coverage, " +
                    $"approximately {domain.EstimatedTextureBytes:N0} texture bytes.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    string.IsNullOrEmpty(domain.LastError)
                        ? "Weather wind resources are not ready."
                        : domain.LastError,
                    MessageType.Error);
            }

            if (domain.DebugView == WeatherWindDebugView.ResponseError &&
                domain.ResourcesReady &&
                cachedResponsePixels == null)
            {
                EditorGUILayout.HelpBox(
                    "Response Error uses a small editor-only GPU readback. Select the " +
                    "domain in Scene view and wait a moment if arrows do not appear immediately.",
                    MessageType.Info);
            }

            if (WeatherWindDomain.ActiveDomainCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple Weather Wind Domains are active. The most recently " +
                    "enabled domain publishes the global XZ field.",
                    MessageType.Warning);
            }
        }

        private void DuringSceneGui(SceneView sceneView)
        {
            var domain = target as WeatherWindDomain;
            if (domain == null ||
                domain.DebugView == WeatherWindDebugView.Off ||
                Selection.activeGameObject != domain.gameObject)
            {
                return;
            }

            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.Always;
            try
            {
                DrawDomainOutline(domain);
                DrawAnchorMarker(domain);

                switch (domain.DebugView)
                {
                    case WeatherWindDebugView.WindField:
                        DrawWindField(domain);
                        break;
                    case WeatherWindDebugView.ResponseError:
                        UpdateResponseReadback(domain);
                        DrawResponseError(domain);
                        break;
                }

                DrawSceneLegend(domain);
            }
            finally
            {
                Handles.zTest = previousZTest;
            }

            if (Application.isPlaying)
            {
                sceneView.Repaint();
            }
        }

        private void DrawWindField(WeatherWindDomain domain)
        {
            DrawVectorField(
                domain,
                domain.MaximumWindStrength,
                new Color(0.20f, 0.85f, 1.00f, 0.95f),
                new Color(1.00f, 0.55f, 0.10f, 0.95f),
                (int logicalX, int logicalY, Vector2 worldXZ, out Vector2 vector) =>
                {
                    vector = domain.SampleTargetWindXZ(worldXZ);
                    return true;
                });
        }

        private void DrawResponseError(WeatherWindDomain domain)
        {
            if (cachedResponsePixels == null)
            {
                DrawSceneLabel(
                    domain,
                    "Awaiting response-field readback...");
                return;
            }

            float errorVisualizationMaximum = Mathf.Max(
                0.05f,
                domain.MaximumVisualBendMetres * 0.35f);
            DrawVectorField(
                domain,
                errorVisualizationMaximum,
                new Color(0.65f, 0.45f, 1.00f, 0.95f),
                new Color(1.00f, 0.15f, 0.55f, 0.95f),
                (int logicalX, int logicalY, Vector2 worldXZ, out Vector2 vector) =>
                    domain.TrySampleResponseErrorDebug(
                        logicalX,
                        logicalY,
                        worldXZ,
                        cachedResponseSimulationTime,
                        cachedResponsePixels,
                        out vector));
        }

        private delegate bool VectorSampler(
            int logicalX,
            int logicalY,
            Vector2 worldXZ,
            out Vector2 vector);

        private void DrawVectorField(
            WeatherWindDomain domain,
            float normalizationMaximum,
            Color lowMagnitudeColor,
            Color highMagnitudeColor,
            VectorSampler sampler)
        {
            Rect rect = domain.GetFieldWorldRectXZ();
            int resolution = domain.FieldResolution;
            int step = Mathf.Max(1, domain.DebugSampleStepCells);
            float cell = domain.CellSizeMetres;
            float sampleY = domain.GetDebugAnchorPosition().y + domain.DebugHeightOffset;
            float referenceLength = step * cell * 0.75f * domain.DebugArrowScale;
            float arrowHeadSize = Mathf.Max(0.05f, referenceLength * 0.18f);
            float normalization = Mathf.Max(0.0001f, normalizationMaximum);

            for (int logicalY = 0; logicalY < resolution; logicalY += step)
            {
                for (int logicalX = 0; logicalX < resolution; logicalX += step)
                {
                    Vector2 worldXZ = new Vector2(
                        rect.xMin + (logicalX + 0.5f) * cell,
                        rect.yMin + (logicalY + 0.5f) * cell);
                    if (!sampler(logicalX, logicalY, worldXZ, out Vector2 vector))
                    {
                        continue;
                    }

                    float magnitude = vector.magnitude;
                    if (magnitude <= 0.0001f)
                    {
                        continue;
                    }

                    Vector2 direction = vector / magnitude;
                    float normalizedMagnitude = Mathf.Clamp01(magnitude / normalization);
                    float arrowLength = referenceLength * normalizedMagnitude;
                    if (arrowLength <= 0.0001f)
                    {
                        continue;
                    }

                    Vector3 start = new Vector3(worldXZ.x, sampleY, worldXZ.y);
                    Vector3 end = start + new Vector3(direction.x, 0f, direction.y) * arrowLength;
                    Color color = Color.Lerp(
                        lowMagnitudeColor,
                        highMagnitudeColor,
                        normalizedMagnitude);
                    Handles.color = color;
                    Handles.DrawAAPolyLine(2.5f, start, end);
                    Quaternion rotation = Quaternion.LookRotation(end - start, Vector3.up);
                    Handles.ConeHandleCap(
                        0,
                        end,
                        rotation,
                        arrowHeadSize,
                        EventType.Repaint);
                }
            }
        }

        private void DrawDomainOutline(WeatherWindDomain domain)
        {
            Rect rect = domain.GetFieldWorldRectXZ();
            float y = domain.GetDebugAnchorPosition().y + Mathf.Max(0.02f, domain.DebugHeightOffset * 0.35f);
            Vector3 a = new Vector3(rect.xMin, y, rect.yMin);
            Vector3 b = new Vector3(rect.xMax, y, rect.yMin);
            Vector3 c = new Vector3(rect.xMax, y, rect.yMax);
            Vector3 d = new Vector3(rect.xMin, y, rect.yMax);
            Handles.color = new Color(1f, 1f, 1f, 0.65f);
            Handles.DrawAAPolyLine(3f, a, b, c, d, a);
        }

        private void DrawAnchorMarker(WeatherWindDomain domain)
        {
            Vector3 anchor = domain.GetDebugAnchorPosition();
            anchor.y += domain.DebugHeightOffset * 0.5f;
            Handles.color = new Color(1f, 0.95f, 0.2f, 0.9f);
            float size = HandleUtility.GetHandleSize(anchor) * 0.12f;
            Handles.SphereHandleCap(0, anchor, Quaternion.identity, size, EventType.Repaint);
            Handles.DrawLine(anchor, anchor + Vector3.up * Mathf.Max(0.15f, domain.DebugHeightOffset));
        }

        private void DrawSceneLegend(WeatherWindDomain domain)
        {
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(12f, 12f, 360f, 72f), GUI.skin.box);
            GUILayout.Label(
                $"Weather Wind Debug — {domain.DebugView}",
                EditorStyles.boldLabel);
            string vectorMeaning = domain.DebugView == WeatherWindDebugView.WindField
                ? "Arrows = authoritative XZ wind. Length and colour = strength."
                : "Magenta arrows = actual bend minus target bend. No arrow = caught up.";
            GUILayout.Label(
                vectorMeaning,
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.Label(
                "White outline = active field. Yellow marker = resolved anchor.",
                EditorStyles.wordWrappedMiniLabel);
            GUILayout.EndArea();
            Handles.EndGUI();
        }

        private void DrawSceneLabel(WeatherWindDomain domain, string message)
        {
            Vector3 position = domain.GetDebugAnchorPosition();
            position.y += Mathf.Max(0.5f, domain.DebugHeightOffset + 0.35f);
            Handles.BeginGUI();
            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(position);
            Rect rect = new Rect(guiPoint.x + 8f, guiPoint.y - 18f, 260f, 24f);
            GUI.Label(rect, message, EditorStyles.helpBox);
            Handles.EndGUI();
        }

        private void UpdateResponseReadback(WeatherWindDomain domain)
        {
            if (!domain.ResourcesReady || domain.ResponseTexture == null)
            {
                cachedResponsePixels = null;
                return;
            }

            if (readbackPending || EditorApplication.timeSinceStartup < nextReadbackTime)
            {
                return;
            }

            readbackPending = true;
            nextReadbackTime = EditorApplication.timeSinceStartup + ReadbackIntervalSeconds;
            var texture = domain.ResponseTexture;
            float requestedSimulationTime = domain.SimulationTime;
            AsyncGPUReadback.Request(
                texture,
                0,
                TextureFormat.RGBAFloat,
                request =>
                {
                    readbackPending = false;
                    if (target == null)
                    {
                        return;
                    }

                    if (request.hasError)
                    {
                        cachedResponsePixels = null;
                        Repaint();
                        SceneView.RepaintAll();
                        return;
                    }

                    cachedResponsePixels = request.GetData<Color>().ToArray();
                    cachedResponseSimulationTime = requestedSimulationTime;
                    Repaint();
                    SceneView.RepaintAll();
                });
        }
    }
}
