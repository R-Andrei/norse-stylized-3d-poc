using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private void DrawFoamMaterialProbeSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            DrawReadOnlyRow(
                new GUIContent("Recommended Debug View"),
                "Foam / Layer C / Material Remaining Life");
            DrawReadOnlyRow(
                new GUIContent("Probe State"),
                runtime != null
                    ? runtime.IsolatedLifeProbeStatus
                    : Application.isPlaying
                        ? "Runtime unavailable"
                        : "Not in Play Mode");

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Clear + Emit Configured Life Probe",
                            "Clears persistent foam material, cancels active " +
                            "births, and writes three isolated patches using " +
                            "the current production lifetime parameters.")))
                {
                    ApplyFoamSpawnProperties();
                    river.ClearAndEmitFoamIsolatedLifeProbe(false);
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Clear + Emit Absolute 1s Probe",
                            "Clears material and writes the same patches with " +
                            "a debug-only one-second direct aging authority.")))
                {
                    ApplyFoamSpawnProperties();
                    river.ClearAndEmitFoamAbsoluteLifeProbe();
                }
            }

            EditorGUILayout.LabelField(
                "Configured probes use the production lifetime plumbing. The " +
                "absolute one-second probe isolates raw Remaining Life aging.",
                EditorStyles.wordWrappedMiniLabel);
        }

        private void DrawFoamManualBirthSourceSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            EditorGUILayout.LabelField(
                "Creates stable Layer C source material for transport and " +
                "lifecycle validation. It does not author macro fracture.",
                EditorStyles.wordWrappedMiniLabel);

            EditorGUILayout.LabelField(
                "Source Position",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("foamSpawnDistanceNormalized"),
                new GUIContent(
                    "Longitudinal Position",
                    "Normalized position from logical upstream start (0) to " +
                    "downstream end (1)."));
            EditorGUILayout.PropertyField(
                Find("foamSpawnAcrossNormalized"),
                new GUIContent(
                    "Across Position",
                    "Normalized lateral position: -1 left edge, 0 centre, " +
                    "+1 right edge."));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Source Material",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("foamSpawnAmount"),
                new GUIContent(
                    "Amount",
                    "Source coverage amount. This is not Remaining Life, " +
                    "opacity, or fracture severity."));
            EditorGUILayout.PropertyField(
                Find("foamSpawnRemainingLife"),
                new GUIContent(
                    "Initial Remaining Life",
                    "Normalized Remaining Life assigned to accepted source " +
                    "material."));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Source Shape",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("foamSpawnScale"),
                new GUIContent(
                    "Half Width",
                    "World-space half-width of the moving manual source."));

            if (DrawInlineFoldout(
                    InspectorSection.FoamManualSourceMotion,
                    "Source Path Motion"))
            {
                using (new EditorGUI.IndentLevelScope())
                {
                    EditorGUILayout.PropertyField(
                        Find("foamSpawnRibbonDuration"),
                        new GUIContent("Duration"));
                    EditorGUILayout.PropertyField(
                        Find("foamSpawnRibbonTravelDistance"),
                        new GUIContent("Travel Distance"));
                    EditorGUILayout.PropertyField(
                        Find("foamSpawnRibbonAcrossDrift"),
                        new GUIContent("Across Drift"));
                    EditorGUILayout.PropertyField(
                        Find("foamSpawnRibbonPathWander"),
                        new GUIContent(
                            "Path Bend",
                            "Deterministic smooth bend applied to the source " +
                            "path; this is not Foam breakup."));
                }
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Start Manual Source",
                            "Starts one budgeted manual Layer C source event.")))
                {
                    ApplyFoamSpawnProperties();
                    river.StartFoamSpawn();
                }
            }

            DrawReadOnlyRow(
                new GUIContent("Source State"),
                runtime != null
                    ? runtime.LatestFoamCompositionEventId > 0
                        ? $"event {runtime.LatestFoamCompositionEventId}, " +
                          $"active {runtime.ActiveFoamCompositionEventCount}/" +
                          $"{runtime.FoamCompositionPoolCapacity}, " +
                          $"budget {runtime.FoamCompositionBirthBudgetPerStep}/step"
                        : $"Idle / budget " +
                          $"{runtime.FoamCompositionBirthBudgetPerStep}/step"
                    : Application.isPlaying
                        ? "Runtime unavailable"
                        : "Not in Play Mode");
            DrawReadOnlyRow(
                new GUIContent("Last Segment"),
                runtime != null
                    ? $"{runtime.LastFoamCompositionSegmentLength:0.000} m"
                    : "—");
            DrawReadOnlyRow(
                new GUIContent("Source Texels"),
                runtime != null
                    ? runtime.ProgressiveBirthDebugReadbackAvailable
                        ? $"{runtime.ProgressiveBirthDebugLatestAffectedTexels:N0} latest"
                        : runtime.ProgressiveBirthDebugReadbackPending
                            ? "Awaiting readback"
                            : "No completed source readback"
                    : "—");
        }

        private void DrawMajorCandidatePreview()
        {
            EditorGUILayout.Space(5f);
            EditorGUILayout.LabelField(
                "Major Candidate Proof",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "This compact preview isolates one local field-first Major shape. The generated whole-river distribution must be judged on the real river through Foam + Aging Topology, where it appears as part of the green positive-support field beneath the exact final Foam mask.",
                MessageType.None);

            int seed = Mathf.Max(
                0,
                Find("foamMajorSupportSeed").intValue);
            if (majorCandidatePreview == null ||
                majorCandidatePreviewSeed != seed)
            {
                majorCandidatePreview =
                    StylizedRiverFoamMajorCandidateGenerator.Generate(seed);
                majorCandidatePreviewSeed = seed;
                RefreshMajorCandidatePreviewTexture();
            }

            string[] stageLabels =
            {
                "Raw Field",
                "Thresholded",
                "Cleaned",
                "Final Support"
            };
            EditorGUI.BeginChangeCheck();
            int selectedStage = GUILayout.Toolbar(
                (int)majorCandidatePreviewStage,
                stageLabels);
            if (EditorGUI.EndChangeCheck())
            {
                majorCandidatePreviewStage =
                    (StylizedRiverFoamMajorCandidatePreviewStage)
                    selectedStage;
                RefreshMajorCandidatePreviewTexture();
            }

            if (majorCandidatePreviewTexture != null)
            {
                float previewSize = Mathf.Clamp(
                    EditorGUIUtility.currentViewWidth - 70f,
                    160f,
                    280f);
                Rect previewRect = GUILayoutUtility.GetRect(
                    previewSize,
                    previewSize,
                    GUILayout.ExpandWidth(false));
                previewRect.x += Mathf.Max(
                    0f,
                    (EditorGUIUtility.currentViewWidth -
                        previewRect.width - 35f) * 0.5f);
                EditorGUI.DrawPreviewTexture(
                    previewRect,
                    majorCandidatePreviewTexture,
                    null,
                    ScaleMode.ScaleToFit);
            }

            using (new EditorGUI.IndentLevelScope())
            {
                DrawReadOnlyRow(
                    new GUIContent("Status"),
                    majorCandidatePreview.Accepted
                        ? "Accepted"
                        : "Rejected after bounded retries");
                DrawReadOnlyRow(
                    new GUIContent("Primary Rejection"),
                    majorCandidatePreview.Accepted
                        ? "None"
                        : ObjectNames.NicifyVariableName(
                            majorCandidatePreview.RejectionReason.ToString()));
                DrawReadOnlyRow(
                    new GUIContent("Occupied Area"),
                    $"{majorCandidatePreview.OccupiedCellCount:N0} cells · " +
                    $"{majorCandidatePreview.OccupiedAreaFraction * 100f:0.0}%");
                DrawReadOnlyRow(
                    new GUIContent("Minimum Neck Width"),
                    $"{majorCandidatePreview.MinimumNeckWidthCells} cells");
                DrawReadOnlyRow(
                    new GUIContent("Compactness"),
                    majorCandidatePreview.Compactness.ToString("0.000"));
            }
        }

        private void RefreshMajorCandidatePreviewTexture()
        {
            if (majorCandidatePreview == null)
            {
                return;
            }

            int resolution = majorCandidatePreview.Resolution;
            int cellCount = resolution * resolution;
            if (majorCandidatePreviewTexture == null ||
                majorCandidatePreviewTexture.width != resolution ||
                majorCandidatePreviewTexture.height != resolution)
            {
                if (majorCandidatePreviewTexture != null)
                {
                    DestroyImmediate(majorCandidatePreviewTexture);
                }

                majorCandidatePreviewTexture = new Texture2D(
                    resolution,
                    resolution,
                    TextureFormat.RGBA32,
                    false,
                    true)
                {
                    name = "PS3D Major Candidate Preview",
                    hideFlags = HideFlags.HideAndDontSave,
                    wrapMode = TextureWrapMode.Clamp
                };
                majorCandidatePreviewPixels = new Color32[cellCount];
            }
            else if (majorCandidatePreviewPixels == null ||
                majorCandidatePreviewPixels.Length != cellCount)
            {
                majorCandidatePreviewPixels = new Color32[cellCount];
            }

            majorCandidatePreviewTexture.filterMode =
                majorCandidatePreviewStage ==
                StylizedRiverFoamMajorCandidatePreviewStage.FinalSupport
                    ? FilterMode.Bilinear
                    : FilterMode.Point;
            majorCandidatePreview.FillPreview(
                majorCandidatePreviewStage,
                majorCandidatePreviewPixels);
            majorCandidatePreviewTexture.SetPixels32(
                majorCandidatePreviewPixels);
            majorCandidatePreviewTexture.Apply(false, false);
        }

        private void ApplyFoamSpawnProperties()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void CreateAndAssignFoamTopologyCacheAsset(
            StylizedRiver river)
        {
            if (river == null || Application.isPlaying)
            {
                return;
            }

            string safeName = string.IsNullOrWhiteSpace(river.name)
                ? "River"
                : river.name.Replace('/', '_').Replace('\\', '_');
            string path = EditorUtility.SaveFilePanelInProject(
                "Create River Foam Topology Cache",
                $"{safeName}_FoamTopologyCache",
                "asset",
                "Choose where this authored river's prepared Foam topology " +
                "payload should be stored.");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            path = AssetDatabase.GenerateUniqueAssetPath(path);
            StylizedRiverFoamTopologyCacheAsset asset =
                CreateInstance<StylizedRiverFoamTopologyCacheAsset>();
            AssetDatabase.CreateAsset(asset, path);
            Undo.RegisterCreatedObjectUndo(
                asset,
                "Create River Foam Topology Cache");
            Undo.RecordObject(
                river,
                "Assign River Foam Topology Cache");
            SerializedObject riverObject = new SerializedObject(river);
            SerializedProperty cacheProperty =
                riverObject.FindProperty("foamTopologyCacheAsset");
            cacheProperty.objectReferenceValue = asset;
            riverObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(river);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            serializedObject.Update();
            EditorGUIUtility.PingObject(asset);
        }

        private void BuildOrUpdateFoamTopologyCache(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            serializedObject.ApplyModifiedProperties();
            StylizedRiverFoamTopologyCacheAsset asset =
                river.FoamTopologyCacheAsset;
            if (asset == null)
            {
                serializedObject.Update();
                return;
            }

            if (!runtime.TryBuildTopologyCache(
                    out StylizedRiverFoamTopologyCacheBuildArtifact artifact))
            {
                serializedObject.Update();
                return;
            }

            Undo.RecordObject(
                asset,
                "Update River Foam Topology Cache");
            asset.StoreBuild(artifact);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            serializedObject.Update();
            runtime.ValidateAssignedTopologyCache();
            EditorGUIUtility.PingObject(asset);
        }

        private void DrawActions()
        {
            DrawNestedSection(
                InspectorSection.ActionsGeneration,
                "Generation",
                DrawButtons);
            DrawNestedSection(
                InspectorSection.ActionsDomainValidation,
                "Domain Validation",
                DrawDomainValidationActions);
            DrawNestedSection(
                InspectorSection.ActionsDisturbanceTests,
                "Disturbance Test Events",
                DrawDisturbanceTestActions);
            DrawNestedSection(
                InspectorSection.ActionsFoamLayerACache,
                "Foam Layer A Cache Tools",
                DrawFoamLayerACacheActions);
            DrawNestedSection(
                InspectorSection.ActionsFoamLayerCTests,
                "Foam Layer C Test Sources",
                DrawFoamLayerCTestActions);
            DrawNestedSection(
                InspectorSection.ActionsFoamLifecycleProbes,
                "Foam Lifecycle Probes",
                DrawFoamLifecycleProbeActions);
            DrawNestedSection(
                InspectorSection.ActionsRuntimeClearReset,
                "Runtime Clear & Reset",
                DrawRuntimeClearResetActions);
        }

        private void DrawDomainValidationActions()
        {
            bool anyMissingHarness = false;
            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is StylizedRiver river &&
                    river.GetComponent<StylizedRiverDomainDebug>() == null)
                {
                    anyMissingHarness = true;
                    break;
                }
            }

            DrawReadOnlyRow(
                new GUIContent("Proof Harness"),
                anyMissingHarness
                    ? "Missing on one or more selected rivers"
                    : "Present on all selected rivers");

            using (new EditorGUI.DisabledScope(!anyMissingHarness))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Add Domain Proof Harness",
                            "Adds the editor proof component to selected rivers " +
                            "that do not already have one.")))
                {
                    foreach (Object selectedTarget in targets)
                    {
                        if (selectedTarget is not StylizedRiver river ||
                            river.GetComponent<StylizedRiverDomainDebug>() != null)
                        {
                            continue;
                        }

                        Undo.AddComponent<StylizedRiverDomainDebug>(
                            river.gameObject);
                    }
                }
            }

            if (GUILayout.Button(
                    new GUIContent(
                        "Validate Domain Contract",
                        "Runs the shared-domain contract validation on every " +
                        "selected river.")))
            {
                ApplyToTargets(
                    "Validate Stylized River Domain",
                    river => river.ValidateRiverDomainContract());
            }
        }

        private void DrawDisturbanceTestActions()
        {
            EditorGUILayout.PropertyField(
                Find("impactRippleTestDistanceNormalized"),
                new GUIContent(
                    "Longitudinal Position",
                    "Manual test location along the river: 0 is the domain " +
                    "start and 1 is the domain end."));
            EditorGUILayout.PropertyField(
                Find("impactRippleTestAcrossNormalized"),
                new GUIContent(
                    "Across Position",
                    "Manual test location across the water surface: -1 is " +
                    "the left edge, 0 is centre, and +1 is the right edge."));
            EditorGUILayout.PropertyField(
                Find("impactRippleTestEvent"),
                new GUIContent(
                    "Event",
                    "Profile used by the four manual disturbance test actions."),
                true);

            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            StylizedRiverDisturbanceRuntime runtime = river != null
                ? river.GetComponent<StylizedRiverDisturbanceRuntime>()
                : null;

            DrawReadOnlyRow(
                new GUIContent("Runtime"),
                river == null
                    ? "Select one river for runtime test actions."
                    : runtime != null
                        ? runtime.IsSleeping ? "Sleeping" : "Active"
                        : river.RuntimeDisturbancesEnabled
                            ? "Will be created automatically"
                            : "Not allocated");

            using (new EditorGUI.DisabledScope(
                       river == null ||
                       runtime != null ||
                       !river.RuntimeDisturbancesEnabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Create Disturbance Runtime",
                            "Creates the hidden river-owned disturbance " +
                            "runtime immediately. Normally it is created on " +
                            "demand.")))
                {
                    runtime = river.GetOrCreateDisturbanceRuntime();
                    EditorUtility.SetDirty(river);
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       river == null ||
                       runtime == null))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(
                        new GUIContent(
                            "Emit Test Impact",
                            "Emits the configured Event at the selected " +
                            "longitudinal and across-river position.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugImpact(
                        river.ImpactRippleTestDistanceNormalized,
                        river.ImpactRippleTestAcrossNormalized,
                        river.ImpactRippleTestEvent);
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Emit Opposite Sign",
                            "Emits the same Event after reversing Signed " +
                            "Impulse and Initial Elevation.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOppositeSignImpact(
                        river.ImpactRippleTestDistanceNormalized,
                        river.ImpactRippleTestAcrossNormalized,
                        river.ImpactRippleTestEvent);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(
                        new GUIContent(
                            "Emit Overlapping Pair",
                            "Emits two nearby copies of the configured Event " +
                            "to validate overlap and reinforcement.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOverlappingPair(
                        river.ImpactRippleTestDistanceNormalized,
                        river.ImpactRippleTestAcrossNormalized,
                        river.ImpactRippleTestEvent);
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Emit Near Shore",
                            "Emits the configured Event near the selected bank " +
                            "to validate shoreline absorption and reflection.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugNearShore(
                        river.ImpactRippleTestDistanceNormalized,
                        river.ImpactRippleTestAcrossNormalized,
                        river.ImpactRippleTestEvent);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawFoamLayerACacheActions()
        {
            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            StylizedRiverFoamRuntime runtime = river != null
                ? river.GetComponent<StylizedRiverFoamRuntime>()
                : null;

            DrawReadOnlyRow(
                new GUIContent("Cache Asset"),
                river == null
                    ? "Select one river for cache tools."
                    : river.FoamTopologyCacheAsset != null
                        ? river.FoamTopologyCacheAsset.name
                        : "Not assigned");
            DrawReadOnlyRow(
                new GUIContent("Runtime Build State"),
                runtime != null
                    ? runtime.TopologyCacheBuildState
                    : river == null
                        ? "Select one river for cache tools."
                        : Application.isPlaying
                            ? "Runtime unavailable"
                            : "Enter Play Mode for runtime cache build");

            using (new EditorGUI.DisabledScope(
                       river == null ||
                       Application.isPlaying ||
                       river.FoamTopologyCacheAsset != null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Create Cache Asset",
                            "Creates and assigns a persistent topology cache " +
                            "asset for this river.")))
                {
                    CreateAndAssignFoamTopologyCacheAsset(river);
                }
            }

            using (new EditorGUI.DisabledScope(
                       river == null ||
                       !Application.isPlaying ||
                       runtime == null ||
                       river.FoamTopologyCacheAsset == null ||
                       !runtime.TopologyCacheBuildReady))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Build / Update Cache",
                            "Builds the current deterministic topology payload " +
                            "and stores it in the assigned cache asset.")))
                {
                    BuildOrUpdateFoamTopologyCache(river, runtime);
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       river.FoamTopologyCacheAsset == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Validate Assigned Cache",
                            "Validates the assigned cache against the current " +
                            "domain, obstacle, and generation fingerprints.")))
                {
                    runtime.ValidateAssignedTopologyCache();
                }
            }

            if (river != null)
            {
                DrawMajorCandidatePreview();
            }
        }

        private void DrawFoamLayerCTestActions()
        {
            if (targets.Length != 1 || target is not StylizedRiver river)
            {
                DrawReadOnlyRow(
                    new GUIContent("Runtime"),
                    "Select one river for Foam test-source actions.");
                return;
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();
            DrawFoamManualBirthSourceSection(river, runtime);
        }

        private void DrawFoamLifecycleProbeActions()
        {
            if (targets.Length != 1 || target is not StylizedRiver river)
            {
                DrawReadOnlyRow(
                    new GUIContent("Runtime"),
                    "Select one river for lifecycle probes.");
                return;
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();
            DrawFoamMaterialProbeSection(river, runtime);
        }

        private void DrawRuntimeClearResetActions()
        {
            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            StylizedRiverDisturbanceRuntime disturbanceRuntime = river != null
                ? river.GetComponent<StylizedRiverDisturbanceRuntime>()
                : null;
            StylizedRiverFoamRuntime foamRuntime = river != null
                ? river.GetComponent<StylizedRiverFoamRuntime>()
                : null;

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       disturbanceRuntime == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Clear Disturbance Field",
                            "Clears Pressure, Wake, Ripple textures, and " +
                            "pending transient disturbance state. Authored " +
                            "settings and registered sources remain.")))
                {
                    disturbanceRuntime.ClearField();
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       foamRuntime == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Clear Foam Material",
                            "Clears persistent Foam material and active Foam " +
                            "birth events without changing authored settings.")))
                {
                    river.ClearFoam();
                }
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       disturbanceRuntime == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Reset Disturbance Peaks",
                            "Resets recent disturbance dispatch, thread-group, " +
                            "cell-iteration, and rebuild peaks.")))
                {
                    disturbanceRuntime.ResetPerformanceDiagnosticPeaks();
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       foamRuntime == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Reset Foam Peaks",
                            "Resets recent Foam dispatch and cell-iteration " +
                            "peaks to the current update.")))
                {
                    foamRuntime.ResetRecentPeaks();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button(
                    new GUIContent(
                        "Reset All Debug Views",
                        "Sets all five River debug selectors to Final on every " +
                        "selected river.")))
            {
                SetExclusiveDebugView(
                    RiverDebugFeature.FinalRender,
                    0);
            }
        }

        private void DrawButtons()
        {
            if (GUILayout.Button("Regenerate River and Ground"))
            {
                ApplyToTargets(
                    "Regenerate Stylized River",
                    river => river.RegenerateAll());
            }

            if (GUILayout.Button("Rebuild Surface and Corridor"))
            {
                ApplyToTargets(
                    "Rebuild Stylized River Surface and Corridor",
                    river => river.RebuildSurfaceOnly());
            }

            if (GUILayout.Button("Clear Generated River"))
            {
                ApplyToTargets(
                    "Clear Stylized River",
                    river => river.ClearGenerated());
            }
        }
    }
}
