using System.Collections.Generic;
using ProgrammaticStylized3D.Geometry.Ground;
using ProgrammaticStylized3D.Weather;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace ProgrammaticStylized3D.Vegetation.Editor
{
    [CustomEditor(typeof(VegetationLayer))]
    public sealed class VegetationLayerEditor : UnityEditor.Editor
    {
        private static readonly string[] RenderingOptions =
        {
            "Enabled",
            "Disabled"
        };

        private int requestedResolution =
            VegetationCoverageField.DefaultResolution;
        private SerializedProperty coveragePaintMode;
        private SerializedProperty coverageBrushRadius;
        private SerializedProperty coverageBrushStrength;
        private SerializedProperty coverageEraseMode;
        private SerializedProperty showCoverageOverlay;

        private bool coverageStrokeActive;
        private bool coverageStrokeChanged;
        private int coverageStrokeControlId;
        private VegetationLayer coverageStrokeLayer;
        private int overlayRevision = int.MinValue;
        private int overlaySurfaceRevision = int.MinValue;
        private int overlayResolution = -1;
        private int overlayTransformHash = int.MinValue;
        private readonly List<Vector3> overlayPoints = new List<Vector3>();
        private readonly List<float> overlayValues = new List<float>();

        private void OnEnable()
        {
            if (target is VegetationLayer layer)
            {
                requestedResolution = layer.CoverageResolution;
            }
            coveragePaintMode = serializedObject.FindProperty("coveragePaintMode");
            coverageBrushRadius = serializedObject.FindProperty("coverageBrushRadius");
            coverageBrushStrength = serializedObject.FindProperty("coverageBrushStrength");
            coverageEraseMode = serializedObject.FindProperty("coverageEraseMode");
            showCoverageOverlay = serializedObject.FindProperty("showCoverageOverlay");
            Undo.undoRedoPerformed += HandleUndoRedo;
            SceneView.duringSceneGui -= HandleCoverageSceneGUI;
            SceneView.duringSceneGui += HandleCoverageSceneGUI;
        }

        private void OnDisable()
        {
            CompleteCoverageStroke();
            Undo.undoRedoPerformed -= HandleUndoRedo;
            SceneView.duringSceneGui -= HandleCoverageSceneGUI;
        }

        private void HandleUndoRedo()
        {
            serializedObject.UpdateIfRequiredOrScript();
            if (target is VegetationLayer layer)
            {
                layer.RebuildVegetation();
                InvalidateOverlay();
            }
            Repaint();
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            var layer = (VegetationLayer)target;
            DrawHierarchyOwnership(layer);

            int rebuildHashBefore = layer.ComputeRebuildConfigurationHash();
            int lightingHashBefore = layer.ComputeLightingConfigurationHash();
            bool inspectorChanged = DrawProductionProperties();

            if (inspectorChanged)
            {
                int rebuildHashAfter = layer.ComputeRebuildConfigurationHash();
                int lightingHashAfter = layer.ComputeLightingConfigurationHash();
                if (rebuildHashAfter != rebuildHashBefore)
                {
                    layer.RebuildVegetation();
                }
                else if (lightingHashAfter != lightingHashBefore)
                {
                    layer.RefreshLightingMaterialProperties();
                }

                EditorUtility.SetDirty(layer);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }

            DrawRendering(layer);
            DrawCoverage(layer);
            DrawActions(layer);
            DrawStatus(layer);
        }

        private bool DrawProductionProperties()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(
                    serializedObject.FindProperty("m_Script"));
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("densityPerSquareMetre"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("seed"));
            EditorGUILayout.PropertyField(
                serializedObject.FindProperty("minimumCoverage"));

            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                switch (iterator.propertyPath)
                {
                    case "m_Script":
                    case "densityPerSquareMetre":
                    case "seed":
                    case "coverageGround":
                    case "minimumCoverage":
                    case "coverage":
                    case "coveragePaintMode":
                    case "coverageBrushRadius":
                    case "coverageBrushStrength":
                    case "coverageEraseMode":
                    case "showCoverageOverlay":
                        continue;
                }

                EditorGUILayout.PropertyField(iterator, true);
            }

            bool changed = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();
            return changed;
        }

        private static void DrawHierarchyOwnership(VegetationLayer layer)
        {
            EditorGUILayout.LabelField(
                "Ground Ownership",
                EditorStyles.boldLabel);
            if (layer.SurfaceGround == null)
            {
                EditorGUILayout.HelpBox(
                    "Invalid hierarchy: VegetationLayer requires a " +
                    "GeneratedGround ancestor. Place it directly under the " +
                    "Ground or beneath that Ground's Vegetation child. " +
                    "The layer renders nothing until the hierarchy is valid.",
                    MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.ObjectField(
                    "Resolved Ground",
                    layer.SurfaceGround,
                    typeof(GeneratedGround),
                    true);
            }
            EditorGUILayout.LabelField(
                "Ownership",
                "Nearest GeneratedGround ancestor");
        }

        private static void DrawRendering(VegetationLayer layer)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            int current = layer.RenderingEnabled ? 0 : 1;
            EditorGUI.BeginChangeCheck();
            int selected = EditorGUILayout.Popup(
                "Rendering",
                current,
                RenderingOptions);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(layer, "Change Vegetation Layer Rendering");
            layer.SetRenderingEnabled(selected == 0);
            EditorUtility.SetDirty(layer);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private void DrawCoverage(VegetationLayer layer)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Coverage Authoring",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Initialized",
                layer.CoverageInitialized ? "Yes" : "No");
            EditorGUILayout.LabelField(
                "Resolution",
                $"{layer.CoverageResolution} × {layer.CoverageResolution}");
            EditorGUILayout.LabelField(
                "Serialized bytes",
                layer.CoverageByteCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "Revision",
                layer.CoverageRevision.ToString());
            EditorGUILayout.LabelField(
                "Average coverage",
                $"{layer.AverageCoverage * 100f:0.0}%");

            if (!layer.CoverageStorageValid)
            {
                EditorGUILayout.HelpBox(
                    "Layer coverage storage does not match its declared " +
                    "resolution. Initialize Empty or Initialize Full to repair it.",
                    MessageType.Error);
            }

            requestedResolution = EditorGUILayout.IntSlider(
                "Requested Resolution",
                requestedResolution,
                VegetationCoverageField.MinimumResolution,
                VegetationCoverageField.MaximumResolution);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Initialize Empty"))
                {
                    MutateCoverage(
                        layer,
                        "Initialize Empty Vegetation Layer Coverage",
                        () => layer.InitializeCoverage(false));
                }
                if (GUILayout.Button("Initialize Full"))
                {
                    MutateCoverage(
                        layer,
                        "Initialize Full Vegetation Layer Coverage",
                        () => layer.InitializeCoverage(true));
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear Empty"))
                {
                    MutateCoverage(
                        layer,
                        "Clear Vegetation Layer Coverage",
                        () => layer.FillCoverage(0f));
                }
                if (GUILayout.Button("Fill Full"))
                {
                    MutateCoverage(
                        layer,
                        "Fill Vegetation Layer Coverage",
                        () => layer.FillCoverage(1f));
                }
            }
            using (new EditorGUI.DisabledScope(
                       requestedResolution == layer.CoverageResolution))
            {
                if (GUILayout.Button("Apply Resolution and Preserve Coverage"))
                {
                    MutateCoverage(
                        layer,
                        "Resize Vegetation Layer Coverage",
                        () => layer.SetCoverageResolution(
                            requestedResolution,
                            true));
                }
            }

            serializedObject.UpdateIfRequiredOrScript();
            EditorGUI.BeginChangeCheck();
            using (new EditorGUI.DisabledScope(
                       !layer.CoverageInitialized ||
                       !layer.CoverageStorageValid ||
                       layer.SurfaceGround == null))
            {
                EditorGUILayout.PropertyField(
                    coveragePaintMode,
                    new GUIContent(
                        "Enable Scene Painting",
                        "Claims non-Alt left-button Scene input. Drag stamps " +
                        "mutate this layer only; one rebuild occurs when a changed stroke ends."));
                EditorGUILayout.PropertyField(
                    coverageEraseMode,
                    new GUIContent("Erase", "Subtract coverage instead of adding it."));
                float maximumRadius = layer.SurfaceGround != null
                    ? Mathf.Max(0.1f, layer.SurfaceGround.PatchSize)
                    : 40f;
                coverageBrushRadius.floatValue = EditorGUILayout.Slider(
                    new GUIContent("Brush Radius", "World-space radius in metres."),
                    Mathf.Max(0.05f, coverageBrushRadius.floatValue),
                    0.05f,
                    maximumRadius);
                coverageBrushStrength.floatValue = EditorGUILayout.Slider(
                    new GUIContent(
                        "Brush Strength",
                        "Coverage added or removed at the brush centre per stamp."),
                    Mathf.Clamp01(coverageBrushStrength.floatValue),
                    0f,
                    1f);
            }
            EditorGUILayout.PropertyField(
                showCoverageOverlay,
                new GUIContent(
                    "Show Coverage Overlay",
                    "Shows at most 32 × 32 revision-cached samples conformed to the Ground surface."));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(layer);
                InvalidateOverlay();
                SceneView.RepaintAll();
            }

            if (layer.CoveragePaintMode &&
                layer.CoverageInitialized &&
                layer.CoverageStorageValid &&
                layer.SurfaceGround != null)
            {
                EditorGUILayout.HelpBox(
                    layer.CoverageEraseMode
                        ? "Scene Painting active: ERASE. Left-drag removes coverage; hold Alt for Scene navigation."
                        : "Scene Painting active: PAINT. Left-drag adds coverage; hold Alt for Scene navigation.",
                    layer.CoverageEraseMode
                        ? MessageType.Warning
                        : MessageType.Info);
            }
        }

        private static void DrawActions(VegetationLayer layer)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(
                "Layer Actions",
                EditorStyles.boldLabel);
            if (GUILayout.Button("Rebuild Vegetation Layer"))
            {
                Undo.RecordObject(layer, "Rebuild Vegetation Layer");
                layer.RebuildVegetation();
                EditorUtility.SetDirty(layer);
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }

            GroundVegetation root = layer.transform.parent != null
                ? layer.transform.parent.GetComponent<GroundVegetation>()
                : null;
            using (new EditorGUI.DisabledScope(root == null))
            {
                if (GUILayout.Button("Duplicate Recipe as Empty Layer"))
                {
                    VegetationLayer created =
                        VegetationLayerAuthoring.DuplicateLayerAsEmpty(
                            root,
                            layer);
                    Selection.activeGameObject = created.gameObject;
                    SceneView.RepaintAll();
                }
            }
        }

        private static void DrawStatus(VegetationLayer layer)
        {
            EditorGUILayout.Space();
            MessageType statusType = layer.ResourcesReady
                ? MessageType.Info
                : MessageType.Warning;
            string status = layer.ResourcesReady
                ? $"Ready: {layer.InstanceCount:N0} instances, " +
                  $"{layer.ClusterTriangleCount:N0} triangles per cluster."
                : "Vegetation layer resources are not ready.";
            EditorGUILayout.HelpBox(status, statusType);

            if (!string.IsNullOrEmpty(layer.LastBuildError))
            {
                EditorGUILayout.HelpBox(
                    layer.LastBuildError,
                    MessageType.Error);
            }

            if (layer.SurfaceGround != null)
            {
                EditorGUILayout.HelpBox(
                    "Surface Ground: " + layer.SurfaceGround.name + "\n" +
                    "Placement domain: " + layer.PlacementDomainSummary,
                    MessageType.Info);
            }

            int domainCount = WeatherWindDomain.ActiveDomainCount;
            if (domainCount == 0)
            {
                EditorGUILayout.HelpBox(
                    "No shared Weather XZ Wind Domain is active. The layer remains static.",
                    MessageType.Warning);
            }
            else if (domainCount > 1)
            {
                EditorGUILayout.HelpBox(
                    "Multiple Weather XZ Wind Domains are active. The most recently " +
                    "enabled domain publishes the global field.",
                    MessageType.Warning);
            }
        }

        private static void MutateCoverage(
            VegetationLayer layer,
            string undoName,
            System.Action mutation)
        {
            Undo.RegisterCompleteObjectUndo(layer, undoName);
            mutation();
            EditorUtility.SetDirty(layer);
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }

        private void HandleCoverageSceneGUI(SceneView sceneView)
        {
            if (sceneView == null ||
                targets.Length != 1 ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CompleteCoverageStroke();
                return;
            }

            VegetationLayer layer = target as VegetationLayer;
            if (layer == null ||
                Selection.gameObjects.Length != 1 ||
                Selection.activeGameObject != layer.gameObject)
            {
                CompleteCoverageStroke();
                return;
            }

            Event current = Event.current;
            if (current == null)
            {
                return;
            }

            if (layer.ShowCoverageOverlay && current.type == EventType.Repaint)
            {
                DrawCoverageOverlay(layer);
            }

            if (!layer.CoverageInitialized ||
                !layer.CoverageStorageValid ||
                !layer.CoveragePaintMode ||
                layer.SurfaceGround == null)
            {
                CompleteCoverageStroke();
                return;
            }

            if (current.type == EventType.MouseMove)
            {
                sceneView.Repaint();
            }
            if (current.type == EventType.Repaint)
            {
                DrawCoveragePaintStatus(layer);
            }

            GeneratedGround ground = layer.SurfaceGround;
            Ray ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            bool hasSurfaceHit = ground.TryRaycastGeneratedSurface(
                ray,
                out Vector3 surfaceHit);
            if (hasSurfaceHit && current.type == EventType.Repaint)
            {
                Color previousColor = Handles.color;
                Handles.color = layer.CoverageEraseMode
                    ? new Color(1f, 0.25f, 0.15f, 1f)
                    : new Color(0.25f, 1f, 0.35f, 1f);
                Handles.DrawWireDisc(
                    surfaceHit,
                    ground.transform.up,
                    layer.CoverageBrushRadius);
                Handles.color = previousColor;
            }

            int controlId = GUIUtility.GetControlID(
                "VegetationLayerCoveragePaint".GetHashCode(),
                FocusType.Passive);
            if (current.type == EventType.Layout && !current.alt)
            {
                HandleUtility.AddDefaultControl(controlId);
            }

            if (current.alt)
            {
                CompleteCoverageStroke();
                return;
            }

            if (current.type == EventType.MouseDown &&
                current.button == 0 &&
                hasSurfaceHit)
            {
                Undo.RegisterCompleteObjectUndo(
                    layer,
                    layer.CoverageEraseMode
                        ? "Erase Vegetation Layer Coverage"
                        : "Paint Vegetation Layer Coverage");
                coverageStrokeActive = true;
                coverageStrokeChanged = false;
                coverageStrokeControlId = controlId;
                coverageStrokeLayer = layer;
                GUIUtility.hotControl = controlId;
                ApplyCoveragePaintStamp(layer, surfaceHit);
                current.Use();
                return;
            }

            if (current.type == EventType.MouseDrag &&
                current.button == 0 &&
                coverageStrokeActive &&
                GUIUtility.hotControl == coverageStrokeControlId)
            {
                if (hasSurfaceHit)
                {
                    ApplyCoveragePaintStamp(layer, surfaceHit);
                }
                current.Use();
                return;
            }

            if (current.type == EventType.MouseUp &&
                current.button == 0 &&
                coverageStrokeActive)
            {
                CompleteCoverageStroke();
                current.Use();
                return;
            }

            if ((current.type == EventType.KeyDown &&
                 current.keyCode == KeyCode.Escape) ||
                current.type == EventType.MouseLeaveWindow)
            {
                CompleteCoverageStroke();
            }
        }

        private void ApplyCoveragePaintStamp(
            VegetationLayer layer,
            Vector3 worldPosition)
        {
            if (layer == null ||
                !layer.PaintCoverageStamp(
                    worldPosition,
                    layer.CoverageBrushRadius,
                    layer.CoverageBrushStrength,
                    layer.CoverageEraseMode))
            {
                return;
            }

            coverageStrokeChanged = true;
            EditorUtility.SetDirty(layer);
            InvalidateOverlay();
            Repaint();
            SceneView.RepaintAll();
        }

        private void CompleteCoverageStroke()
        {
            if (!coverageStrokeActive)
            {
                return;
            }

            VegetationLayer layer = coverageStrokeLayer;
            bool changed = coverageStrokeChanged;
            int controlId = coverageStrokeControlId;
            coverageStrokeActive = false;
            coverageStrokeChanged = false;
            coverageStrokeControlId = 0;
            coverageStrokeLayer = null;

            if (GUIUtility.hotControl == controlId)
            {
                GUIUtility.hotControl = 0;
            }

            if (layer != null)
            {
                layer.CompleteCoverageStroke(changed);
                if (changed)
                {
                    EditorUtility.SetDirty(layer);
                }
            }

            serializedObject.UpdateIfRequiredOrScript();
            Repaint();
            SceneView.RepaintAll();
        }

        private static void DrawCoveragePaintStatus(VegetationLayer layer)
        {
            Handles.BeginGUI();
            string mode = layer.CoverageEraseMode ? "ERASE" : "PAINT";
            GUI.Box(
                new Rect(12f, 12f, 310f, 44f),
                $"Vegetation Layer Coverage: {mode}\n" +
                $"Radius {layer.CoverageBrushRadius:0.##} m  " +
                $"Strength {layer.CoverageBrushStrength:0.##}  Alt: navigate",
                EditorStyles.helpBox);
            Handles.EndGUI();
        }

        private void DrawCoverageOverlay(VegetationLayer layer)
        {
            EnsureCoverageOverlay(layer);
            if (overlayPoints.Count == 0)
            {
                return;
            }

            Color previousColor = Handles.color;
            CompareFunction previousZTest = Handles.zTest;
            Handles.zTest = CompareFunction.LessEqual;
            for (int index = 0; index < overlayPoints.Count; index++)
            {
                float coverage = overlayValues[index];
                Handles.color = coverage <= 0.01f
                    ? new Color(1f, 0.15f, 0.1f, 0.28f)
                    : coverage >= 0.99f
                        ? new Color(0.15f, 1f, 0.25f, 0.28f)
                        : new Color(1f, 0.72f, 0.1f, 0.34f);
                Vector3 point = overlayPoints[index];
                float size = HandleUtility.GetHandleSize(point) * 0.018f;
                Handles.DotHandleCap(
                    0,
                    point,
                    Quaternion.identity,
                    size,
                    EventType.Repaint);
            }
            Handles.zTest = previousZTest;
            Handles.color = previousColor;
        }

        private void EnsureCoverageOverlay(VegetationLayer layer)
        {
            GeneratedGround ground = layer.SurfaceGround;
            if (ground == null)
            {
                InvalidateOverlay();
                return;
            }

            int transformHash = ground.transform.localToWorldMatrix.GetHashCode();
            int resolution = layer.CoverageResolution;
            if (overlayRevision == layer.CoverageRevision &&
                overlaySurfaceRevision == ground.SurfaceGeometryRevision &&
                overlayResolution == resolution &&
                overlayTransformHash == transformHash)
            {
                return;
            }

            overlayPoints.Clear();
            overlayValues.Clear();
            int sampleCount = Mathf.Min(32, resolution);
            if (sampleCount >= 2)
            {
                for (int sampleZ = 0; sampleZ < sampleCount; sampleZ++)
                {
                    int z = Mathf.RoundToInt(
                        sampleZ * (resolution - 1f) / (sampleCount - 1f));
                    for (int sampleX = 0; sampleX < sampleCount; sampleX++)
                    {
                        int x = Mathf.RoundToInt(
                            sampleX * (resolution - 1f) / (sampleCount - 1f));
                        if (!layer.Coverage.TryGetTexelWorldPosition(
                                ground,
                                x,
                                z,
                                out Vector3 worldPosition,
                                out float coverage))
                        {
                            continue;
                        }
                        overlayPoints.Add(
                            worldPosition + ground.transform.up * 0.015f);
                        overlayValues.Add(coverage);
                    }
                }
            }

            overlayRevision = layer.CoverageRevision;
            overlaySurfaceRevision = ground.SurfaceGeometryRevision;
            overlayResolution = resolution;
            overlayTransformHash = transformHash;
        }

        private void InvalidateOverlay()
        {
            overlayRevision = int.MinValue;
            overlaySurfaceRevision = int.MinValue;
            overlayResolution = -1;
            overlayTransformHash = int.MinValue;
            overlayPoints.Clear();
            overlayValues.Clear();
        }
    }
}
