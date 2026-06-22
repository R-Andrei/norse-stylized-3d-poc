using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiver))]
    [CanEditMultipleObjects]
    internal sealed class StylizedRiverEditor : UnityEditor.Editor
    {
        private bool showAdvancedBody;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSetup();
            DrawRiverDomain();
            DrawChannel();
            DrawSurfaceMesh();
            DrawWaterBody();
            DrawAdvancedBody();

            bool riverChanged = serializedObject.ApplyModifiedProperties();

            if (riverChanged)
            {
                RepaintScene();
            }

            DrawDeferredStageStatus();
            DrawStatus();
            DrawButtons();
        }

        private SerializedProperty Find(string propertyName)
        {
            return serializedObject.FindProperty(propertyName);
        }

        private void DrawSetup()
        {
            EditorGUILayout.LabelField("Stylized River", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(Find("splineContainer"));
            EditorGUILayout.PropertyField(Find("liveRegeneration"));
        }

        private void DrawRiverDomain()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("River Domain", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                Find("domainSampleSpacing"),
                new GUIContent(
                    "Sample Spacing",
                    "Authoritative world-space spacing in metres. Mesh, terrain, projection, transport, interaction, foam, and later systems share this domain."));
            EditorGUILayout.PropertyField(
                Find("reverseFlow"),
                new GUIContent(
                    "Reverse Flow",
                    "Changes logical downstream orientation without reversing the authored spline or rebuilding independent coordinates."));
            EditorGUILayout.PropertyField(
                Find("connectedRiverDistanceOffset"),
                new GUIContent(
                    "Connected Distance Offset",
                    "Global downstream metre offset reserved for connected river segments."));

            EditorGUILayout.HelpBox(
                "UV0 stores normalized cross-river position and local geometric metres. UV1 reserves global downstream metres, signed lateral metres, oriented local metres, and surface half-width.",
                MessageType.Info);

            if (targets.Length != 1)
            {
                return;
            }

            StylizedRiver river = target as StylizedRiver;

            if (river == null)
            {
                return;
            }

            RiverDomainSnapshot domain = river.Domain;

            EditorGUILayout.LabelField(
                "Status",
                domain.IsValid
                    ? $"{domain.SampleCount:N0} samples, {domain.LocalLength:0.00} m"
                    : "No valid domain");

            if (domain.IsValid)
            {
                EditorGUILayout.LabelField(
                    "Actual Spacing",
                    $"{domain.MinimumSampleSpacing:0.000}–{domain.MaximumSampleSpacing:0.000} m");
                EditorGUILayout.LabelField(
                    "Global Range",
                    $"{domain.GlobalDistanceMinimum:0.00}–{domain.GlobalDistanceMaximum:0.00} m");
            }

            StylizedRiverDomainDebug debugger =
                river.GetComponent<StylizedRiverDomainDebug>();

            if (debugger == null)
            {
                if (GUILayout.Button("Add Domain Proof Harness"))
                {
                    Undo.AddComponent<StylizedRiverDomainDebug>(river.gameObject);
                }
            }
            else
            {
                EditorGUILayout.LabelField(
                    "Proof Harness",
                    "Active — centreline, banks, frames, and constant-speed marker");
            }

            if (GUILayout.Button("Validate Domain Contract"))
            {
                river.ValidateRiverDomainContract();
            }
        }

        private void DrawChannel()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Channel", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(Find("width"), new GUIContent("Water Width"));
            EditorGUILayout.PropertyField(Find("depth"), new GUIContent("Bed Depth"));
            EditorGUILayout.PropertyField(Find("bedFlatness"));
            EditorGUILayout.PropertyField(Find("bankBlend"));
            EditorGUILayout.PropertyField(Find("bankProfile"));
            EditorGUILayout.PropertyField(Find("bankOverlap"), new GUIContent("Water Underlap"));
            EditorGUILayout.PropertyField(Find("carvingStrength"));
        }

        private void DrawSurfaceMesh()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Surface Mesh", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                Find("quality"),
                new GUIContent(
                    "Across-River Quality",
                    "Controls cross-channel tessellation only. Along-river density is owned by the River Domain sample spacing."));
            EditorGUILayout.PropertyField(
                Find("surfaceOffset"),
                new GUIContent("Water Level Offset"));
        }

        private void DrawWaterBody()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Water Body", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 2 evaluates the still body only. Surface motion, refraction distortion, foam, and reflections are intentionally not rendered yet.",
                MessageType.Info);

            SerializedProperty preset = Find("bodyPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Body Preset",
                    "Applies only the still-water body settings. It will never change later motion, foam, refraction, or reflection controls."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply Water Body Preset");
                    river.ApplyWaterBodyPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(Find("shallowColor"), new GUIContent("Shallow Colour"));
            EditorGUILayout.PropertyField(Find("deepColor"), new GUIContent("Deep Colour"));
            EditorGUILayout.PropertyField(
                Find("clarity"),
                new GUIContent(
                    "Clarity",
                    "How strongly the riverbed remains visible through the water."));
            EditorGUILayout.PropertyField(
                Find("bodyDepthRange"),
                new GUIContent(
                    "Depth Range",
                    "World-space vertical depth at which the water reaches its deep appearance."));
            EditorGUILayout.PropertyField(
                Find("bodyDepthContrast"),
                new GUIContent(
                    "Depth Contrast",
                    "Low values produce a gradual transition. High values separate shallow and deep water more strongly."));
            EditorGUILayout.PropertyField(
                Find("waterTintStrength"),
                new GUIContent(
                    "Water Tint Strength",
                    "How strongly the water volume colours the scene beneath it."));
            EditorGUILayout.PropertyField(
                Find("surfacePresence"),
                new GUIContent(
                    "Surface Presence",
                    "How clearly the air-water boundary remains visible, even in shallow clear water."));

            if (EditorGUI.EndChangeCheck())
            {
                Find("bodyPreset").enumValueIndex =
                    (int)StylizedRiverWaterBodyPreset.Custom;
            }
        }

        private void DrawAdvancedBody()
        {
            EditorGUILayout.Space(8f);
            showAdvancedBody = EditorGUILayout.Foldout(
                showAdvancedBody,
                "Water Body Validation",
                true);

            if (!showAdvancedBody)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                Find("bodyDebugView"),
                new GUIContent(
                    "Debug View",
                    "Displays the body inputs independently: vertical depth, shaped depth, bed transmission, final body coverage, scene colour, depth validity, or surface coverage."));
            EditorGUILayout.PropertyField(
                Find("bodyMaterial"),
                new GUIContent(
                    "Body Material Override",
                    $"Leave empty for the included {StylizedRiver.CompatibleShaderName} shader."));
            EditorGUI.indentLevel--;
        }

        private void DrawDeferredStageStatus()
        {
            if (targets.Length != 1)
            {
                return;
            }

            StylizedRiver river = target as StylizedRiver;

            if (river == null)
            {
                return;
            }

            bool hasFoam = river.GetComponent<StylizedRiverFoamSimulation>() != null;
            bool hasReflection = river.GetComponent<StylizedRiverPlanarReflection>() != null;

            if (!hasFoam && !hasReflection)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Deferred-stage components are still present on this river, but the Stage 2 shader intentionally ignores foam and planar-reflection inputs. They may remain disabled until their stages are redesigned.",
                MessageType.Warning);
        }

        private void DrawStatus()
        {
            if (targets.Length != 1)
            {
                return;
            }

            StylizedRiver river = target as StylizedRiver;

            if (river == null)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Generated Status", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("River Length", $"{river.RiverLength:0.00} m");
            EditorGUILayout.LabelField("Domain Version", river.Domain.Version.ToString());
            EditorGUILayout.LabelField("Domain Samples", river.Domain.SampleCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "Global Distance",
                $"{river.GlobalDistanceMinimum:0.00}–{river.GlobalDistanceMaximum:0.00} m");
            EditorGUILayout.LabelField(
                "Average Surface Height",
                $"{river.AverageSurfaceHeight:0.00} m");
            EditorGUILayout.LabelField(
                "Surface Triangles",
                river.SurfaceTriangleCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "GameObject Layer",
                LayerMask.LayerToName(river.gameObject.layer));
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Regenerate River and Ground"))
            {
                ApplyToTargets(
                    "Regenerate Stylized River",
                    river => river.RegenerateAll());
            }

            if (GUILayout.Button("Rebuild Surface Only"))
            {
                ApplyToTargets(
                    "Rebuild Stylized River Surface",
                    river => river.RebuildSurfaceOnly());
            }

            if (GUILayout.Button("Clear Generated River"))
            {
                ApplyToTargets(
                    "Clear Stylized River",
                    river => river.ClearGenerated());
            }
        }

        private void ApplyToTargets(string undoName, RiverAction action)
        {
            foreach (Object selectedTarget in targets)
            {
                StylizedRiver river = selectedTarget as StylizedRiver;

                if (river == null)
                {
                    continue;
                }

                Undo.RecordObject(river, undoName);
                action(river);
                EditorUtility.SetDirty(river);
            }

            serializedObject.Update();
            Repaint();
            RepaintScene();
        }

        private static void RepaintScene()
        {
            SceneView.RepaintAll();
        }

        private delegate void RiverAction(StylizedRiver river);

        [MenuItem("GameObject/PS3D/Stylized River", false, 10)]
        private static void CreateStylizedRiver(MenuCommand command)
        {
            GameObject riverObject = new GameObject("River_Main");
            GameObjectUtility.SetParentAndAlign(
                riverObject,
                command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(
                riverObject,
                "Create Stylized River");
            Undo.AddComponent<SplineContainer>(riverObject);
            Undo.AddComponent<StylizedRiver>(riverObject);
            Selection.activeGameObject = riverObject;
        }
    }
}
