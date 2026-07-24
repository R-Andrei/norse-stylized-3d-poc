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

        private bool showDomainPlacement;
        private bool showResolutionBudget;
        private bool showBaseWind;
        private bool showBroadVariation;
        private bool showGustRegions;
        private bool showElasticResponse;
        private bool showDebugDiagnostics;
        private bool showActionsReports;
        private bool showLiveStatus;

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

            serializedObject.UpdateIfRequiredOrScript();
            WeatherInspectorGui.DrawScriptReference(serializedObject);
            DrawImmediateWarnings(domain);

            DrawDomainPlacement(domain);
            DrawResolutionBudget(domain);
            DrawBaseWind();
            DrawBroadVariation();
            DrawGustRegions();
            DrawElasticResponse();
            DrawDebugDiagnostics(domain);

            bool changed = serializedObject.ApplyModifiedProperties();
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

            DrawActionsReports(domain);
            DrawLiveStatus(domain);
        }

        private static void DrawImmediateWarnings(WeatherWindDomain domain)
        {
            if (WeatherWindDomain.ActiveDomainCount > 1)
            {
                WeatherInspectorGui.Warning(
                    "Multiple Weather Wind Domains are active. The most recently " +
                    "enabled domain publishes the global XZ field.");
            }

            if (!string.IsNullOrEmpty(domain.LastError))
            {
                WeatherInspectorGui.Error(domain.LastError);
            }
        }

        private void DrawDomainPlacement(WeatherWindDomain domain)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showDomainPlacement,
                    "Domain Placement",
                    "Controls how the moving XZ wind field resolves its world-space centre."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fieldAnchor",
                    "Gameplay Anchor",
                    "Preferred Transform followed by the XZ wind field. Assign the player or camera follow target rather than an offset isometric camera.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "targetCamera",
                    "Fallback Camera",
                    "Used only when Gameplay Anchor is unassigned. Its forward ray is projected onto the horizontal field plane. Camera.main is resolved when this reference is also unassigned.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fieldPlaneY",
                    "Fallback Projection Plane Y",
                    "World-space Y coordinate of the horizontal plane used to project the fallback camera into an XZ field centre.");

                WeatherInspectorGui.Help(
                    "Resolution order: Gameplay Anchor, assigned Fallback Camera, " +
                    "automatic Camera.main, then the controller Transform.");
                WeatherInspectorGui.ReadOnlyObject(
                    "Resolved Camera",
                    domain.TargetCamera);
                WeatherInspectorGui.ReadOnlyRow(
                    "Current Anchor Position",
                    domain.GetDebugAnchorPosition().ToString("F3"));
            }
        }

        private void DrawResolutionBudget(WeatherWindDomain domain)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showResolutionBudget,
                    "Resolution & Update Budget",
                    "Controls field coverage, texture memory, and fixed-cadence simulation work."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "fieldResolution",
                    "Field Resolution",
                    "Texel count per axis for the authoritative target field and elastic response textures. Higher values increase memory and compute cost quadratically.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "cellSizeMetres",
                    "Cell Size (m)",
                    "World-space size represented by one field texel. Resolution multiplied by Cell Size determines coverage per axis.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "updateRateHz",
                    "Simulation Rate (Hz)",
                    "Fixed update frequency for recentering and elastic response simulation. Higher values improve temporal response and increase dispatch frequency.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumStepsPerFrame",
                    "Maximum Catch-Up Steps / Frame",
                    "Maximum fixed simulation steps allowed in one rendered frame after a stall. This prevents unbounded catch-up work.");

                WeatherInspectorGui.ReadOnlyRow(
                    "Coverage Per Axis",
                    $"{domain.FieldWorldSizeMetres:0.###} m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Field Area",
                    $"{domain.FieldWorldSizeMetres * domain.FieldWorldSizeMetres:0.###} m²");
                WeatherInspectorGui.ReadOnlyRow(
                    "Estimated Texture Memory",
                    $"{domain.EstimatedTextureBytes:N0} bytes");
            }
        }

        private void DrawBaseWind()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showBaseWind,
                    "Base Wind",
                    "Defines the prevailing direction and baseline authoritative wind strength."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "prevailingDirection",
                    "Prevailing Direction (XZ)",
                    "Base horizontal wind direction. The vector is normalized internally; magnitude does not multiply strength.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "baseStrength",
                    "Base Wind Strength",
                    "Authoritative Weather strength present before broad variation and gust regions are added.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumWindStrength",
                    "Maximum Authoritative Strength",
                    "Maximum magnitude of the CPU/GPU target-wind vector. Gameplay and wind-trail consumers read these dimensionless Weather units.");

                WeatherInspectorGui.Help(
                    "Base Wind and Maximum Authoritative Strength affect gameplay " +
                    "and every Weather consumer that samples target wind.");
            }
        }

        private void DrawBroadVariation()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showBroadVariation,
                    "Broad Variation",
                    "Adds large moving XZ variation around the prevailing wind."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "broadNoiseScaleMetres",
                    "Variation Scale (m)",
                    "Approximate world-space size of broad wind features. Larger values create wider, slower-changing regions.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "broadNoiseTravelSpeed",
                    "Variation Travel Speed",
                    "World-pattern travel speed used by the broad procedural variation.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "turbulenceStrength",
                    "Turbulence Strength",
                    "Strength of directional and magnitude deviation added by broad variation.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "seed",
                    "Pattern Seed",
                    "Deterministic seed shared by the broad variation and gust-region procedural patterns.");
            }
        }

        private void DrawGustRegions()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showGustRegions,
                    "Gust Regions",
                    "Controls irregular stronger-wind regions moving through the authoritative field."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "gustNoiseScaleMetres",
                    "Gust Region Scale (m)",
                    "Approximate world-space size of gust regions. Larger values produce broader gust areas.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "gustTravelSpeed",
                    "Gust Travel Speed",
                    "World-pattern travel speed of gust regions through the field.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "gustStrength",
                    "Gust Strength",
                    "Additional authoritative wind strength contributed inside a fully active gust region.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "gustThreshold",
                    "Gust Activation Threshold",
                    "Procedural threshold defining how much of the field becomes a gust region. Higher values generally make gust regions less common.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "gustSoftness",
                    "Gust Boundary Softness",
                    "Normalized transition width around gust-region boundaries. Higher values produce broader, softer transitions.");
            }
        }

        private void DrawElasticResponse()
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showElasticResponse,
                    "Elastic Visual Response",
                    "Controls the delayed bend field sampled by Vegetation without changing authoritative target wind."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "responseFrequencyHz",
                    "Spring Frequency (Hz)",
                    "Natural frequency of the visual spring response. Higher values catch up to target wind faster.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "responseDampingRatio",
                    "Damping Ratio",
                    "Damping applied to the visual spring. Lower values oscillate more; higher values settle more directly.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "responseVariation",
                    "Response Variation",
                    "Per-cell deterministic variation applied to the visual response so large areas do not bend identically.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "maximumVisualBendMetres",
                    "Maximum Visual Bend (m)",
                    "Maximum displacement stored by the elastic response texture. Vegetation interprets this value in world metres.");

                WeatherInspectorGui.Help(
                    "These controls affect the visual response texture only. They " +
                    "do not change the authoritative target wind sampled by gameplay or wind trails.");
            }
        }

        private void DrawDebugDiagnostics(WeatherWindDomain domain)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showDebugDiagnostics,
                    "Debug & Diagnostics",
                    "Controls Scene-view visualization of the authoritative wind field or response error."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugView",
                    "Debug View",
                    "Off disables Scene diagnostics. Wind Field shows authoritative target wind. Response Error shows actual visual bend minus target bend.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugSampleStepCells",
                    "Sample Step (cells)",
                    "Number of field cells skipped between Scene-view arrows. Higher values reduce Editor diagnostic density and readback drawing cost.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugHeightOffset",
                    "Height Offset (m)",
                    "Vertical offset applied to Scene-view arrows and the field outline.");
                WeatherInspectorGui.Property(
                    serializedObject,
                    "debugArrowScale",
                    "Arrow Scale",
                    "Presentation multiplier applied to Scene-view arrow length only.");

                WeatherInspectorGui.Help(
                    "White outline = active field. Yellow marker = resolved anchor. " +
                    "Wind Field arrows show authoritative XZ wind. Response Error arrows show current bend minus target bend.");

                if (domain.DebugView == WeatherWindDebugView.ResponseError &&
                    domain.ResourcesReady &&
                    cachedResponsePixels == null)
                {
                    WeatherInspectorGui.Info(
                        "Response Error uses a small editor-only GPU readback. " +
                        "Keep the domain selected in Scene view and wait briefly if arrows are not visible yet.");
                }
            }
        }

        private void DrawActionsReports(WeatherWindDomain domain)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showActionsReports,
                    "Actions & Reports",
                    "Manual reset and copyable diagnostic report actions."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                if (GUILayout.Button("Reset Wind Simulation"))
                {
                    domain.ResetField();
                    cachedResponsePixels = null;
                    nextReadbackTime = 0.0;
                    EditorUtility.SetDirty(domain);
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Copy Comprehensive Wind Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        domain.BuildComprehensiveReport();
                    Debug.Log(
                        "[Weather Wind V0] XZ wind-domain report copied to clipboard.",
                        domain);
                }
            }
        }

        private void DrawLiveStatus(WeatherWindDomain domain)
        {
            if (!WeatherInspectorGui.Foldout(
                    ref showLiveStatus,
                    "Live Status",
                    "Read-only resource, field, and dispatch state."))
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                WeatherInspectorGui.ReadOnlyRow(
                    "Published",
                    WeatherWindDomain.PublishedDomain == domain ? "Yes" : "No");
                WeatherInspectorGui.ReadOnlyRow(
                    "Resources",
                    domain.ResourcesReady ? "Ready" : "Not ready");
                WeatherInspectorGui.ReadOnlyRow(
                    "Resolution",
                    $"{domain.FieldResolution} × {domain.FieldResolution}");
                WeatherInspectorGui.ReadOnlyRow(
                    "Cell Size",
                    $"{domain.CellSizeMetres:0.###} m");
                WeatherInspectorGui.ReadOnlyRow(
                    "Coverage",
                    $"{domain.FieldWorldSizeMetres:0.###} m per axis");
                WeatherInspectorGui.ReadOnlyRow(
                    "Texture Memory",
                    $"{domain.EstimatedTextureBytes:N0} bytes");
                WeatherInspectorGui.ReadOnlyRow(
                    "Field Origin XZ",
                    domain.FieldOriginXZ.ToString("F3"));
                WeatherInspectorGui.ReadOnlyRow(
                    "Ring Offset",
                    domain.RingOffset.ToString());
                WeatherInspectorGui.ReadOnlyRow(
                    "Last Frame Steps",
                    domain.LastFrameStepCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Last Frame Dispatches",
                    domain.LastFrameDispatchCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Total Simulation Dispatches",
                    domain.TotalSimulationDispatchCount);
                WeatherInspectorGui.ReadOnlyRow(
                    "Total Recenter Dispatches",
                    domain.TotalRecenterDispatchCount);
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
