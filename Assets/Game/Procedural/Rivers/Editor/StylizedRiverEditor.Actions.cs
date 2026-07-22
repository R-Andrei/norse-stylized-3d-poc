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
                    "Compatibility Longitudinal Position",
                    "Normalized authoring control from logical upstream start " +
                    "(0) to downstream end (1). It resolves to global river " +
                    "distance when the source is submitted."));
            EditorGUILayout.PropertyField(
                Find("foamSpawnAcrossNormalized"),
                new GUIContent(
                    "Compatibility Across Position",
                    "Normalized authoring control: -1 left edge, 0 centre, +1 " +
                    "right edge. It resolves against the local river half-width " +
                    "when the source is submitted."));

            if (river.TryResolveFoamSpawnMetricPlacement(
                    out float resolvedGlobalDistance,
                    out float resolvedLateralMetres,
                    out float resolvedDriftMetres,
                    out float resolvedMaximumBendMetres))
            {
                DrawReadOnlyRow(
                    new GUIContent(
                        "Resolved Metric Placement",
                        "Physical command values represented by the current " +
                        "normalized compatibility controls at the source start."),
                    $"s={resolvedGlobalDistance:0.000} m · " +
                    $"n={resolvedLateralMetres:0.000} m · " +
                    $"drift={resolvedDriftMetres:0.000} m · " +
                    $"bend≤{resolvedMaximumBendMetres:0.000} m");
            }

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
                        new GUIContent(
                            "Compatibility Across Drift",
                            "Normalized compatibility drift. Metric API callers " +
                            "provide lateral drift directly in metres."));
                    EditorGUILayout.PropertyField(
                        Find("foamSpawnRibbonPathWander"),
                        new GUIContent(
                            "Compatibility Path Bend",
                            "Normalized compatibility bend strength. Metric API " +
                            "callers provide maximum bend directly in metres; " +
                            "this is not Foam breakup."));
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
            bool prepared =
                StylizedRiverFoamDevelopmentCacheCoordinator
                    .TryPrepareAndPersist(
                        river,
                        runtime,
                        out bool validationPassed,
                        out string result);
            serializedObject.Update();

            string message = prepared
                ? $"[River Foam P3] Prepared cache '{river.name}': {result}"
                : $"[River Foam P3] Cache preparation failed for " +
                  $"'{river.name}': {result}";
            if (prepared && validationPassed)
            {
                Debug.Log(message, river);
            }
            else
            {
                Debug.LogWarning(message, river);
            }

            if (prepared && river.FoamTopologyCacheAsset != null)
            {
                EditorGUIUtility.PingObject(river.FoamTopologyCacheAsset);
            }
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
                "Foam Cache & Validation",
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

            EditorGUILayout.LabelField(
                "Cache Lifecycle",
                EditorStyles.boldLabel);
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
                        : "Runtime unavailable");

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
                       Application.isPlaying ||
                       runtime == null ||
                       river.FoamTopologyCacheAsset == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Prepare / Rebuild Foam Topology Cache",
                            "Explicitly prepares deterministic Foam topology in " +
                            "Edit Mode and stores one validated payload in the " +
                            "assigned cache asset.")))
                {
                    BuildOrUpdateFoamTopologyCache(river, runtime);
                }
            }

            using (new EditorGUI.DisabledScope(
                       Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       river.FoamTopologyCacheAsset == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Validate Assigned Cache",
                            "Validates the assigned cache against the current " +
                            "domain, obstacle, and generation fingerprints " +
                            "without generating or mutating assets.")))
                {
                    bool valid = runtime.TryValidateAssignedTopologyCacheForRelease(
                        out string state,
                        out string summary,
                        out int payloadBytes,
                        out string payloadHash,
                        out int obstacleSourceCount);
                    string message =
                        $"[River Foam P3] Cache validation '{river.name}': " +
                        $"{state}. {summary} Payload={payloadBytes:N0} bytes, " +
                        $"hash={payloadHash}, obstacles={obstacleSourceCount:N0}.";
                    if (valid)
                    {
                        Debug.Log(message, river);
                    }
                    else
                    {
                        Debug.LogWarning(message, river);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(
                       Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       river.FoamTopologyCacheAsset == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run Exhaustive Cache Integrity Proof",
                            "Explicitly performs the expensive deterministic " +
                            "round-trip, byte-reproduction, generated-channel, " +
                            "and corruption-rejection proof. Normal cache " +
                            "preparation deliberately does not run this work.")))
                {
                    runtime.RunTopologyCacheRoundTripValidation();
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "P12 Candidate Evidence",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Fixed Metric is now the default test path. Rebuild the assigned " +
                "cache after any change that alters the active mapping or resolved " +
                "cell size. Use the " +
                "live capture for comparable runtime evidence and the P9 report " +
                "for the full migrated-consumer regression.",
                EditorStyles.wordWrappedMiniLabel);
            DrawReadOnlyRow(
                new GUIContent(
                    "Diagnostic State",
                    "Result of the latest explicit River Foam diagnostic action."),
                runtime != null
                    ? runtime.TopologyCacheDiagnosticState
                    : "Runtime unavailable");
            if (runtime != null)
            {
                EditorGUILayout.HelpBox(
                    runtime.TopologyCacheDiagnosticSummary,
                    runtime.TopologyCacheDiagnosticState == "Passed"
                        ? MessageType.Info
                        : runtime.TopologyCacheDiagnosticState == "Failed"
                            ? MessageType.Error
                            : MessageType.None);
                DrawReadOnlyRow(
                    new GUIContent(
                        "Runs / Passes",
                        "Non-serialized explicit diagnostic counters for this " +
                        "Editor runtime instance."),
                    $"{runtime.TopologyCacheDiagnosticRunCount:N0} / " +
                    $"{runtime.TopologyCacheDiagnosticPassCount:N0}");
                DrawReadOnlyRow(
                    new GUIContent(
                        "Latest Report File",
                        "Absolute path to the latest user-triggered diagnostic " +
                        "text report under Library/RiverFoamDiagnostics."),
                    string.IsNullOrEmpty(
                        runtime.TopologyCacheDiagnosticReportPath)
                        ? "None"
                        : runtime.TopologyCacheDiagnosticReportPath);
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Automatic Birth Reveal-Speed Audit",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "One Play Mode action captures the latest observed timing for " +
                "all eight automatic Layer C source recipes, current active " +
                "events, 32-slot pool occupancy, and rejected starts. The " +
                "report is written to Library and can be copied directly.",
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       runtime == null ||
                       river == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Write Automatic Birth Reveal-Speed Report",
                            "Captures requested and actual reveal speed, path " +
                            "distance, raw/resolved duration, cadence limiting, " +
                            "active event counts, pool occupancy, and rejected " +
                            "starts for the live automatic-source session.")))
                {
                    runtime.RunAutomaticBirthRevealSpeedReport();
                    Repaint();
                }
            }
            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       runtime.TopologyCacheDiagnosticSummary !=
                           "Automatic birth reveal-speed evidence captured." ||
                       string.IsNullOrEmpty(
                           runtime.TopologyCacheDiagnosticReport)))
            {
                if (GUILayout.Button("Copy Reveal-Speed Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        runtime.TopologyCacheDiagnosticReport;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Complete Candidate Sweep",
                EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "One Play Mode action runs 12 cases: 0.25/0.20/0.15/0.10 m " +
                "at lateral ratios 0, authored, and 1. Each case clears and " +
                "warms the real runtime, captures at least five seconds, then " +
                "restores the authored selection. The assigned cache is never " +
                "written or replaced. Expected duration is roughly 90–120 " +
                "seconds on the current scene.",
                EditorStyles.wordWrappedMiniLabel);
            DrawReadOnlyRow(
                new GUIContent(
                    "Sweep State",
                    "Current state of the one-button P12 candidate matrix."),
                runtime != null
                    ? runtime.P12CandidateSweepStatus
                    : "Runtime unavailable");
            DrawReadOnlyRow(
                new GUIContent(
                    "Sweep Progress",
                    "Completed and in-progress fraction of the 12-case matrix."),
                runtime != null
                    ? $"{runtime.P12CandidateSweepProgress * 100f:0.0}% — " +
                      runtime.P12CandidateSweepSummary
                    : "Runtime unavailable");

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       river.FoamGridMode !=
                           StylizedRiverFoamGridMode.FixedMetric ||
                       river.FoamStateHeld ||
                       river.FreezeAmount >= 0.999f ||
                       runtime.P12CandidateSweepActive))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run Complete P12 Candidate Sweep",
                            "Runs the complete 4-spacing × 3-lateral-ratio " +
                            "matrix through the real runtime and writes one " +
                            "combined report.")))
                {
                    runtime.StartP12CandidateSweep();
                    Repaint();
                }
            }

            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       !runtime.P12CandidateSweepActive))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Cancel P12 Sweep",
                            "Stops after the current frame, restores authored " +
                            "runtime ownership, and writes the partial report.")))
                {
                    runtime.CancelP12CandidateSweep();
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       string.IsNullOrEmpty(
                           runtime.P12CandidateSweepReport)))
            {
                if (GUILayout.Button("Copy P12 Sweep Report to Clipboard"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        runtime.P12CandidateSweepReport;
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Single Candidate Evidence",
                EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       (runtime != null && runtime.P12CandidateSweepActive)))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Start / Reset P12 Candidate Capture",
                            "Starts a clean explicit steady-state work window " +
                            "for the currently active mapping and candidate.")))
                {
                    runtime.ResetSteadyStateWorkAccounting();
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       runtime == null ||
                       river == null ||
                       (runtime != null && runtime.P12CandidateSweepActive) ||
                       !runtime.SteadyStateWorkAccountingActive))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Write P12 Candidate Snapshot",
                            "Writes one read-only live report containing the " +
                            "active descriptor, cache, initialization, CFL, " +
                            "curvature, temporal presentation, Motion Lane, " +
                            "memory, and explicit steady-state work window. " +
                            "Visual acceptance remains manual.")))
                {
                    runtime.RunP12ActiveCandidateSnapshotReport();
                    Repaint();
                }
            }
            EditorGUILayout.EndHorizontal();

            DrawLatestFoamReportCopyButton(
                runtime,
                "Copy P12 Snapshot to Clipboard");

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       Application.isPlaying ||
                       runtime == null ||
                       river == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run Fixed-Metric Consumer Regression (P9)",
                            "Runs the validated P9 endpoint report using the " +
                            "currently authored active mapping. It installs the " +
                            "assigned cache into temporary live resources and " +
                            "verifies structural-to-film grouping, represented " +
                            "area, actual GPU Film Source, visual occupancy and " +
                            "shape paths, production/debug mapping, cleanup, and " +
                            "cache immutability.")))
                {
                    runtime.RunP9ComprehensiveValidationReport();
                    Repaint();
                }
            }

            DrawLatestFoamReportCopyButton(
                runtime,
                "Copy P9 Report");
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       string.IsNullOrEmpty(
                           runtime.TopologyCacheDiagnosticReport)))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Copy Latest Diagnostic Report"))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        runtime.TopologyCacheDiagnosticReport;
                }
                if (GUILayout.Button("Log Latest Diagnostic Report"))
                {
                    runtime.LogLatestTopologyCacheDiagnosticReport();
                }
                EditorGUILayout.EndHorizontal();
            }

            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       string.IsNullOrEmpty(
                           runtime.TopologyCacheDiagnosticReportPath)))
            {
                if (GUILayout.Button("Reveal Latest Diagnostic File"))
                {
                    EditorUtility.RevealInFinder(
                        runtime.TopologyCacheDiagnosticReportPath);
                }
            }

            EditorGUILayout.Space(4f);
            DrawNestedSection(
                InspectorSection.ActionsFoamHistoricalDiagnostics,
                "Historical / Deep Diagnostics",
                DrawFoamHistoricalDiagnostics);

            if (river != null)
            {
                DrawMajorCandidatePreview();
            }
        }

        private static void DrawLatestFoamReportCopyButton(
            StylizedRiverFoamRuntime runtime,
            string buttonLabel)
        {
            using (new EditorGUI.DisabledScope(
                       runtime == null ||
                       string.IsNullOrEmpty(
                           runtime.TopologyCacheDiagnosticReport)))
            {
                if (GUILayout.Button(buttonLabel))
                {
                    EditorGUIUtility.systemCopyBuffer =
                        runtime.TopologyCacheDiagnosticReport;
                }
            }
        }

        private void DrawFoamHistoricalDiagnostics()
        {
            StylizedRiver river = targets.Length == 1
                ? target as StylizedRiver
                : null;
            StylizedRiverFoamRuntime runtime = river != null
                ? river.GetComponent<StylizedRiverFoamRuntime>()
                : null;

            EditorGUILayout.LabelField(
                "Closed phase reports and obstacle-provenance tools remain " +
                "available for targeted regressions. They are not part of the " +
                "normal cache workflow.",
                EditorStyles.wordWrappedMiniLabel);

            bool reportActionDisabled =
                Application.isPlaying || runtime == null || river == null;

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run P8 Transport / Replacement Report",
                            "Reruns the closed P8 conservative transport, CFL, " +
                            "curvature, persistent replacement, topology mapping, " +
                            "cleanup, and cache immutability proof.")))
                {
                    runtime.RunP8ComprehensiveValidationReport();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(runtime, "Copy P8 Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run P7 Source Contract Report",
                            "Reruns the closed P7 automatic/manual source-unit, " +
                            "range, evaluator, lifecycle, cleanup, and cache " +
                            "immutability proof.")))
                {
                    runtime.RunP7ComprehensiveValidationReport();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(runtime, "Copy P7 Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run P6 Routing / External-Field Report",
                            "Reruns the closed P6 routing, Motion Lane, external-" +
                            "field mapping, live transaction, cleanup, and cache " +
                            "immutability proof.")))
                {
                    runtime.RunP6ComprehensiveValidationReport();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(runtime, "Copy P6 Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run P5.3 Deterministic Topology Report",
                            "Reruns the closed P5.3 deterministic topology-phase, " +
                            "obstacle fingerprint, five-build, legacy-raster, " +
                            "publication, and assigned-cache proof.")))
                {
                    runtime.RunP53ComprehensiveValidationReport();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(runtime, "Copy P5.3 Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Run P5.1 Two-Build Audit",
                            "Runs two independent Edit Mode topology " +
                            "preparations without storing either artifact. " +
                            "Captures every obstacle source, compares complete " +
                            "input keys, section byte counts/hashes, first byte " +
                            "differences, topology counts, and the assigned " +
                            "cache. Writes and logs one exhaustive report.")))
                {
                    runtime.RunTopologyCacheDeterminismDiagnosticAudit();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(runtime, "Copy P5.1 Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Capture Obstacle Baseline",
                            "Captures exact per-source local mesh, transform, " +
                            "provider world, and independently recomputed world " +
                            "fingerprints under Library/RiverFoamDiagnostics.")))
                {
                    runtime.CaptureTopologyObstacleDiagnosticBaseline();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(
                    runtime,
                    "Copy Baseline Report");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(reportActionDisabled))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Compare Obstacles to Baseline",
                            "Recaptures every exact obstacle source and reports " +
                            "added, removed, or changed geometry, transform, " +
                            "provider, and direct-world fingerprints.")))
                {
                    runtime
                        .CompareTopologyObstaclesAgainstDiagnosticBaseline();
                    Repaint();
                }
            }
            DrawLatestFoamReportCopyButton(
                    runtime,
                    "Copy Comparison Report");
            EditorGUILayout.EndHorizontal();
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

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       foamRuntime == null))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Start / Reset P4 Accounting",
                            "Starts a clean, explicit steady-state Foam work " +
                            "accounting window after startup. The counters are " +
                            "dormant until this action is used.")))
                {
                    foamRuntime.ResetSteadyStateWorkAccounting();
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying ||
                       foamRuntime == null ||
                       !foamRuntime.SteadyStateWorkAccountingActive))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Log P4 Work Summary",
                            "Emits one compact River Foam work summary for the " +
                            "current explicit accounting window.")))
                {
                    foamRuntime.LogSteadyStateWorkSummary();
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
