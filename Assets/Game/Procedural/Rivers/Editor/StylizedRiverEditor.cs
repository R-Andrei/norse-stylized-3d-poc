using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    [CustomEditor(typeof(StylizedRiver))]
    [CanEditMultipleObjects]
    internal sealed class StylizedRiverEditor : UnityEditor.Editor
    {
        private SerializedProperty splineContainer;
        private SerializedProperty liveRegeneration;

        private SerializedProperty width;
        private SerializedProperty bankBlend;
        private SerializedProperty depth;
        private SerializedProperty bedFlatness;
        private SerializedProperty bankProfile;
        private SerializedProperty bankOverlap;
        private SerializedProperty carvingStrength;

        private SerializedProperty quality;
        private SerializedProperty surfaceOffset;

        private SerializedProperty shallowColor;
        private SerializedProperty deepColor;
        private SerializedProperty flowTint;
        private SerializedProperty opacity;
        private SerializedProperty flowSpeed;
        private SerializedProperty flowScale;
        private SerializedProperty flowStrength;
        private SerializedProperty detailScale;
        private SerializedProperty detailStrength;
        private SerializedProperty waveHeight;
        private SerializedProperty bankLight;
        private SerializedProperty lightingSteps;

        private SerializedProperty enableCurrentAccents;
        private SerializedProperty currentColor;
        private SerializedProperty currentIntensity;
        private SerializedProperty currentOpacity;
        private SerializedProperty currentSpeed;
        private SerializedProperty currentDensity;
        private SerializedProperty currentLength;
        private SerializedProperty currentWidth;
        private SerializedProperty currentCurvature;
        private SerializedProperty currentSoftness;

        private SerializedProperty bodyMaterial;
        private SerializedProperty currentMaterial;
        private SerializedProperty flowTexture;
        private SerializedProperty detailTexture;
        private SerializedProperty currentVerticalOffset;
        private SerializedProperty visualSeed;

        private bool showAdvanced;

        private void OnEnable()
        {
            splineContainer = Find("splineContainer");
            liveRegeneration = Find("liveRegeneration");

            width = Find("width");
            bankBlend = Find("bankBlend");
            depth = Find("depth");
            bedFlatness = Find("bedFlatness");
            bankProfile = Find("bankProfile");
            bankOverlap = Find("bankOverlap");
            carvingStrength = Find("carvingStrength");

            quality = Find("quality");
            surfaceOffset = Find("surfaceOffset");

            shallowColor = Find("shallowColor");
            deepColor = Find("deepColor");
            flowTint = Find("flowTint");
            opacity = Find("opacity");
            flowSpeed = Find("flowSpeed");
            flowScale = Find("flowScale");
            flowStrength = Find("flowStrength");
            detailScale = Find("detailScale");
            detailStrength = Find("detailStrength");
            waveHeight = Find("waveHeight");
            bankLight = Find("bankLight");
            lightingSteps = Find("lightingSteps");

            enableCurrentAccents = Find("enableCurrentAccents");
            currentColor = Find("currentColor");
            currentIntensity = Find("currentIntensity");
            currentOpacity = Find("currentOpacity");
            currentSpeed = Find("currentSpeed");
            currentDensity = Find("currentDensity");
            currentLength = Find("currentLength");
            currentWidth = Find("currentWidth");
            currentCurvature = Find("currentCurvature");
            currentSoftness = Find("currentSoftness");

            bodyMaterial = Find("bodyMaterial");
            currentMaterial = Find("currentMaterial");
            flowTexture = Find("flowTexture");
            detailTexture = Find("detailTexture");
            currentVerticalOffset = Find("currentVerticalOffset");
            visualSeed = Find("visualSeed");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSetup();
            DrawChannel();
            DrawSurface();
            DrawWaterBody();
            DrawCurrentAccents();
            DrawAdvanced();

            serializedObject.ApplyModifiedProperties();

            DrawStatus();
            DrawButtons();
        }

        private SerializedProperty Find(string propertyName)
        {
            return serializedObject.FindProperty(propertyName);
        }

        private void DrawSetup()
        {
            EditorGUILayout.LabelField("Clean River Setup", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(splineContainer, new GUIContent("Spline Container"));
            EditorGUILayout.PropertyField(liveRegeneration, new GUIContent("Live Regeneration"));

            EditorGUILayout.HelpBox(
                "Pass 1 uses two meshes: the river body and a separate current-accent mesh. " +
                "The current accents are generated as fixed ribbons that move downstream without stretching over time.",
                MessageType.Info);
        }

        private void DrawChannel()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Channel", EditorStyles.boldLabel);
            EditorGUILayout.Slider(width, 0.5f, 20f, new GUIContent("Water Width"));
            EditorGUILayout.Slider(depth, 0.1f, 6f, new GUIContent("Bed Depth"));
            EditorGUILayout.Slider(bedFlatness, 0f, 1f, new GUIContent("Bed Flatness"));
            EditorGUILayout.Slider(bankBlend, 0.1f, 12f, new GUIContent("Bank Blend"));
            EditorGUILayout.PropertyField(bankProfile, new GUIContent("Bank Profile"));
            EditorGUILayout.Slider(bankOverlap, 0f, 0.8f, new GUIContent("Water Underlap"));
            EditorGUILayout.Slider(carvingStrength, 0f, 1f, new GUIContent("Carving Strength"));
        }

        private void DrawSurface()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Surface Mesh", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(quality, new GUIContent("Quality"));
            EditorGUILayout.Slider(surfaceOffset, 0f, 0.25f, new GUIContent("Water Level Offset"));
        }

        private void DrawWaterBody()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Water Body", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(shallowColor, new GUIContent("Shallow Color"));
            EditorGUILayout.PropertyField(deepColor, new GUIContent("Deep Color"));
            EditorGUILayout.PropertyField(flowTint, new GUIContent("Highlight Tint"));
            EditorGUILayout.Slider(opacity, 0.15f, 1f, new GUIContent("Opacity"));
            EditorGUILayout.Slider(flowSpeed, 0f, 4f, new GUIContent("Body Flow Speed"));
            EditorGUILayout.Slider(flowScale, 0.5f, 12f, new GUIContent("Body Flow Scale"));
            EditorGUILayout.Slider(flowStrength, 0f, 1f, new GUIContent("Body Flow Visibility"));
            EditorGUILayout.Slider(detailScale, 0.15f, 4f, new GUIContent("Body Detail Scale"));
            EditorGUILayout.Slider(detailStrength, 0f, 1f, new GUIContent("Body Detail Strength"));
            EditorGUILayout.Slider(waveHeight, 0f, 0.18f, new GUIContent("Surface Motion"));
            EditorGUILayout.Slider(bankLight, 0f, 1f, new GUIContent("Shallow Edge Light"));
            EditorGUILayout.Slider(lightingSteps, 1f, 6f, new GUIContent("Lighting Bands"));
        }

        private void DrawCurrentAccents()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Current Accents", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableCurrentAccents, new GUIContent("Enable Current Accents"));

            using (new EditorGUI.DisabledScope(!enableCurrentAccents.boolValue))
            {
                EditorGUILayout.PropertyField(currentColor, new GUIContent("Current Color"));
                EditorGUILayout.Slider(currentIntensity, 0f, 2f, new GUIContent("Intensity"));
                EditorGUILayout.Slider(currentOpacity, 0f, 1f, new GUIContent("Opacity"));
                EditorGUILayout.Slider(currentSpeed, 0f, 4f, new GUIContent("Speed"));
                EditorGUILayout.Slider(currentDensity, 0.05f, 4f, new GUIContent("Density"));
                EditorGUILayout.Slider(currentLength, 0.2f, 8f, new GUIContent("Length"));
                EditorGUILayout.Slider(currentWidth, 0.02f, 1f, new GUIContent("Width"));
                EditorGUILayout.Slider(currentCurvature, 0f, 1f, new GUIContent("Curvature"));
                EditorGUILayout.Slider(currentSoftness, 0f, 1f, new GUIContent("Edge Softness"));
            }
        }

        private void DrawAdvanced()
        {
            EditorGUILayout.Space(8f);
            showAdvanced = EditorGUILayout.Foldout(showAdvanced, "Advanced", true);

            if (!showAdvanced)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(bodyMaterial);
            EditorGUILayout.PropertyField(currentMaterial);
            EditorGUILayout.PropertyField(flowTexture);
            EditorGUILayout.PropertyField(detailTexture);
            EditorGUILayout.Slider(currentVerticalOffset, 0.001f, 0.08f, new GUIContent("Current Vertical Offset"));
            EditorGUILayout.IntSlider(visualSeed, 1, 9999, new GUIContent("Visual Seed"));
            EditorGUI.indentLevel--;
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
            EditorGUILayout.LabelField("Surface Triangles", river.SurfaceTriangleCount.ToString("N0"));
            EditorGUILayout.LabelField("Current Triangles", river.CurrentTriangleCount.ToString("N0"));
            EditorGUILayout.LabelField("Generated Current Accents", river.CurrentAccentCount.ToString("N0"));
        }

        private void DrawButtons()
        {
            EditorGUILayout.Space(10f);

            if (GUILayout.Button("Regenerate River and Ground"))
            {
                ApplyToTargets("Regenerate Clean River", river => river.RegenerateAll());
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Rebuild Surface Only"))
            {
                ApplyToTargets("Rebuild Clean River Surface", river => river.RebuildSurfaceOnly());
            }

            if (GUILayout.Button("Rebuild Current Accents Only"))
            {
                ApplyToTargets("Rebuild Clean River Currents", river => river.RebuildCurrentAccentsOnly());
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Clear Generated River"))
            {
                ApplyToTargets("Clear Clean River", river => river.ClearGenerated());
            }
        }

        private void ApplyToTargets(string undoName, RiverAction action)
        {
            for (int index = 0; index < targets.Length; index++)
            {
                StylizedRiver river = targets[index] as StylizedRiver;

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
            SceneView.RepaintAll();
        }

        private delegate void RiverAction(StylizedRiver river);

        [MenuItem("GameObject/PS3D/Clean Stylized River", false, 10)]
        private static void CreateCleanRiver(MenuCommand command)
        {
            GameObject riverObject = new GameObject("River_Main");
            GameObjectUtility.SetParentAndAlign(riverObject, command.context as GameObject);
            Undo.RegisterCreatedObjectUndo(riverObject, "Create Clean Stylized River");
            riverObject.AddComponent<SplineContainer>();
            riverObject.AddComponent<StylizedRiver>();
            Selection.activeGameObject = riverObject;
        }

        [MenuItem("Tools/PS3D/Rivers/Check Legacy River Files")]
        private static void CheckLegacyRiverFiles()
        {
            string[] legacyPaths =
            {
                "Assets/Game/Procedural/Ground/GeneratedRiver.cs",
                "Assets/Game/Procedural/Ground/GeneratedRiverFoam.cs",
                "Assets/Game/Procedural/Ground/Editor/GeneratedRiverFoamEditor.cs",
                "Assets/Game/Rendering/Water/Shaders/SH_StylizedRiver.shader",
                "Assets/Game/Rendering/Water/Shaders/SH_StylizedRiverFoam.shader",
                "Assets/Game/Rendering/Water/Shaders/SH_StylizedRiverShore.shader"
            };

            bool foundAny = false;

            for (int index = 0; index < legacyPaths.Length; index++)
            {
                Object asset = AssetDatabase.LoadMainAssetAtPath(legacyPaths[index]);

                if (asset == null)
                {
                    continue;
                }

                foundAny = true;
                Debug.LogWarning("Legacy river file still exists: " + legacyPaths[index], asset);
            }

            if (!foundAny)
            {
                Debug.Log("No known legacy PS3D river files were found.");
            }
        }
    }
}
