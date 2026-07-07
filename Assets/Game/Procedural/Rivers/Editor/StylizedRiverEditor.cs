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
        private bool showAdvancedShoreline;
        private bool showPerformanceDiagnostics;
        private bool showFoamDiagnostics;
        private bool showFoamCacheDiagnostics;
        private bool showFoamValidationOverview = true;
        private bool showFoamViewModes;
        private bool showFoamTransportDiagnostics;
        private bool showFoamLifetimeDiagnostics;
        private bool showFoamMaterialProbe;
        private bool showFoamManualBirthSource = true;
        private bool showFoamManualSourceMotion;
        private bool showFoamShapeResidueDiagnostics;
        private bool showFoamRuntimeResourceDiagnostics;
        private bool showFoamAdvancedInternalDiagnostics;
        private StylizedRiverFoamMajorCandidate majorCandidatePreview;
        private Texture2D majorCandidatePreviewTexture;
        private Color32[] majorCandidatePreviewPixels;
        private int majorCandidatePreviewSeed = int.MinValue;
        private StylizedRiverFoamMajorCandidatePreviewStage
            majorCandidatePreviewStage =
                StylizedRiverFoamMajorCandidatePreviewStage.FinalSupport;

        public override bool RequiresConstantRepaint()
        {
            if (!Application.isPlaying || targets.Length != 1 ||
                target is not StylizedRiver river || !river.FoamEnabled)
            {
                return false;
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();
            if (runtime == null || !runtime.ShouldRepaintInspectorForFoamDebug)
            {
                return false;
            }

            return showFoamValidationOverview ||
                showFoamViewModes ||
                showFoamTransportDiagnostics ||
                showFoamLifetimeDiagnostics ||
                showFoamMaterialProbe ||
                showFoamManualBirthSource ||
                showFoamShapeResidueDiagnostics ||
                showFoamRuntimeResourceDiagnostics ||
                showFoamAdvancedInternalDiagnostics;
        }

        private void OnDisable()
        {
            if (majorCandidatePreviewTexture != null)
            {
                DestroyImmediate(majorCandidatePreviewTexture);
                majorCandidatePreviewTexture = null;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSetup();
            DrawRiverDomain();
            DrawChannel();
            DrawNaturalVariation();
            DrawAdvancedShoreline();
            DrawSurfaceMesh();
            DrawSurfaceMotion();
            DrawRefraction();
            DrawRuntimeDisturbances();
            DrawFoam();
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
            EditorGUILayout.PropertyField(
                Find("width"),
                new GUIContent(
                    "Water Width",
                    "Approximate visible open-water width. The corridor adds a small hidden shoreline cover automatically."));
            EditorGUILayout.PropertyField(Find("depth"), new GUIContent("Bed Depth"));
            EditorGUILayout.PropertyField(
                Find("bedFlatness"),
                new GUIContent(
                    "Bed Flatness",
                    "Controls how much of the river centre remains flat before the bed rises toward the shoreline."));
            EditorGUILayout.PropertyField(
                Find("bankBlend"),
                new GUIContent(
                    "Bank Blend",
                    "Actual horizontal distance, in metres, used by the visible corridor bank before it reaches the untouched ground handoff."));
            EditorGUILayout.PropertyField(Find("bankProfile"));
            EditorGUILayout.PropertyField(
                Find("terrainConformity"),
                new GUIContent(
                    "Terrain Conformity",
                    "Zero preserves the sampled ground shape as much as geometry safety permits. One strongly imposes the authored bed and bank cross-section."));

            if (targets.Length != 1 || target is not StylizedRiver river)
            {
                return;
            }

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField(
                "Visible Shoreline",
                "Dedicated spline corridor");
            EditorGUILayout.LabelField(
                "Resolved Hidden Overlap",
                $"{river.ResolvedShorelineOverlap:0.000} m / side");
            EditorGUILayout.LabelField(
                "Generated Water Width",
                $"{river.GeneratedSurfaceWidth:0.000} m");
            EditorGUILayout.LabelField(
                "Collider Handoff Width",
                river.CorridorHandoffWidth > 0f
                    ? $"{river.CorridorHandoffWidth:0.000} m"
                    : "Not generated");
            EditorGUILayout.LabelField(
                "Hidden Integration Apron",
                river.CorridorIntegrationApronWidth > 0f
                    ? $"{river.CorridorIntegrationApronWidth:0.000} m / side"
                    : "Not generated");
            EditorGUILayout.LabelField(
                "Corridor Render Width",
                river.CorridorOuterWidth > 0f
                    ? $"{river.CorridorOuterWidth:0.000} m"
                    : "Not generated");
        }

        private void DrawNaturalVariation()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Natural Channel Variation",
                EditorStyles.boldLabel);

            SerializedProperty preset = Find("channelCharacterPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Channel Character",
                    "Controls only static bed and shoreline variation. It does not change water colour, lighting, freezing, motion, foam, refraction, or reflections."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply Channel Character Preset");
                    river.ApplyChannelCharacterPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("naturalVariationSeed"),
                new GUIContent(
                    "Variation Seed",
                    "Stable seed shared by bed and shoreline variation."));
            EditorGUILayout.PropertyField(
                Find("bedRoughness"),
                new GUIContent(
                    "Bed Roughness",
                    "Maximum vertical variation in metres across the floor and the configured lower portion of the submerged bed slope."));
            EditorGUILayout.PropertyField(
                Find("bedRoughnessScale"),
                new GUIContent(
                    "Bed Feature Scale",
                    "Typical physical size of bottom depressions and raised areas, in metres."));
            EditorGUILayout.PropertyField(
                Find("bedRoughnessReach"),
                new GUIContent(
                    "Bed Roughness Reach",
                    "How far upward through the submerged bed profile roughness may extend. Zero preserves floor-only roughness. One uses the maximum safe reach while retaining a smooth upper shoreline band."));
            EditorGUILayout.PropertyField(
                Find("shorelineIrregularity"),
                new GUIContent(
                    "Shoreline Irregularity",
                    "Maximum smooth deviation of each bank from the configured water width, in metres."));
            EditorGUILayout.PropertyField(
                Find("shorelineIrregularityScale"),
                new GUIContent(
                    "Shoreline Feature Scale",
                    "Typical longitudinal size of widening and narrowing features, in metres."));
            EditorGUILayout.PropertyField(
                Find("bankAsymmetry"),
                new GUIContent(
                    "Bank Asymmetry",
                    "Zero keeps both banks correlated. One lets the left and right banks vary independently."));

            if (EditorGUI.EndChangeCheck())
            {
                Find("channelCharacterPreset").enumValueIndex =
                    (int)StylizedRiverChannelCharacterPreset.Custom;
            }

            if (targets.Length == 1 && target is StylizedRiver singleRiver)
            {
                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField(
                    "Resolved Bed Roughness",
                    $"{singleRiver.ResolvedBedRoughness:0.000} m");
                EditorGUILayout.LabelField(
                    "Resolved Visible Width",
                    $"{singleRiver.ResolvedMinimumVisibleWidth:0.00}–{singleRiver.ResolvedMaximumVisibleWidth:0.00} m");
            }
        }

        private void DrawAdvancedShoreline()
        {
            showAdvancedShoreline = EditorGUILayout.Foldout(
                showAdvancedShoreline,
                "Advanced Corridor Safety",
                true);

            if (!showAdvancedShoreline)
            {
                return;
            }

            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox(
                "The dedicated corridor guarantees wet clearance and hides the flat water-mesh edge beneath its banks. Its collider ends where the corridor meets the untouched ground; a render-only apron continues beneath the ground to hide the coarse heightfield transition.",
                MessageType.Info);
            EditorGUILayout.PropertyField(
                Find("additionalShorelineOverlap"),
                new GUIContent(
                    "Additional Overlap",
                    "Optional extra hidden overlap beyond the calculated safe minimum."));
            EditorGUILayout.PropertyField(
                Find("shorelineWetClearance"),
                new GUIContent(
                    "Wet Clearance",
                    "Minimum separation between generated terrain and the visible wet surface."));
            EditorGUILayout.PropertyField(
                Find("shorelineBankCover"),
                new GUIContent(
                    "Bank Cover",
                    "Minimum corridor-bank height above the hidden water-mesh edge."));
            EditorGUILayout.PropertyField(
                Find("reservedDownwardSurfaceDisplacement"),
                new GUIContent(
                    "Additional Downward Motion Reserve",
                    "Optional clearance beyond the automatically resolved Stage 3 wave height."));
            EditorGUI.indentLevel--;
        }

        private void DrawSurfaceMesh()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Surface Mesh", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                Find("quality"),
                new GUIContent(
                    "Quality",
                    "Controls water geometry and the shared Stage 6 material/topology resolution. Persistent Foam, topology, and the canonical obstacle footprint use 64 cells across at Low, 96 at Medium (standard), and 128 at High. The Stage 1 domain remains the authoritative coordinate source."));
            EditorGUILayout.PropertyField(
                Find("surfaceOffset"),
                new GUIContent("Water Level Offset"));
        }

        private void DrawSurfaceMotion()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Surface Motion", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 3 uses one coherent river-space field for macro displacement, animated normals, current accents, and shoreline lapping. Persistent wakes remain reserved for Stage 5.",
                MessageType.Info);

            SerializedProperty preset = Find("motionPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Motion Character",
                    "Still reproduces the accepted Stage 2 surface. Calm, Flowing, and Furious change only surface motion."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply River Motion Preset");
                    river.ApplyMotionPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(Find("flowSpeed"), new GUIContent("Flow Speed", "Downstream travel speed in metres per second."));
            EditorGUILayout.PropertyField(Find("motionWaveHeight"), new GUIContent("Wave Height", "Maximum vertical macro displacement in metres."));
            EditorGUILayout.PropertyField(Find("motionWaveLength"), new GUIContent("Wave Length", "Typical physical length of displaced waves in metres."));
            EditorGUILayout.PropertyField(Find("motionWaveSteepness"), new GUIContent("Wave Steepness", "Broad rounded waves versus sharper crest-like shapes."));
            EditorGUILayout.PropertyField(Find("motionDetailStrength"), new GUIContent("Surface Detail Strength", "Strength of small flow-aligned normal detail."));
            EditorGUILayout.PropertyField(Find("motionDetailScale"), new GUIContent("Surface Detail Scale", "Typical physical size of ripple detail in metres."));
            EditorGUILayout.PropertyField(Find("motionTurbulence"), new GUIContent("Turbulence", "How strongly the pattern evolves instead of only translating."));
            EditorGUILayout.PropertyField(Find("currentAccentStrength"), new GUIContent("Current Accent Strength", "Broad downstream modulation. This is not foam."));
            EditorGUILayout.PropertyField(Find("currentAccentScale"), new GUIContent("Current Accent Scale", "Typical longitudinal size of current accents in metres."));
            EditorGUILayout.PropertyField(Find("shoreMotion"), new GUIContent("Shore Motion", "Displacement retained where water visibly meets the bank. It fades to zero inside the hidden overlap."));
            EditorGUILayout.PropertyField(Find("shoreMotionWidth"), new GUIContent("Shore Motion Width", "Distance inside the visible shoreline over which centre motion blends toward Shore Motion."));

            EditorGUILayout.Space(3f);
            EditorGUILayout.LabelField("Shore Wave Profile", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("shoreWaveHeightScale"),
                new GUIContent(
                    "Shore Wave Height Scale",
                    "Vertical shore-wave amplitude relative to the centre-river macro wave. One preserves the former height."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveLengthScale"),
                new GUIContent(
                    "Shore Wave Length Scale",
                    "Longitudinal shore-wave length relative to the centre-river macro wave. One preserves the former wavelength."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveReach"),
                new GUIContent(
                    "Shore Wave Reach",
                    "Maximum fraction of the generated hidden shoreline allowance that a shore wave may wet."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveTransitionLength"),
                new GUIContent(
                    "Shore Wave Transition Length",
                    "World-space smoothing distance for the shore-wave profile and for transitions between neighbouring wave sizes. Larger values produce broader, rounder shoreline transitions."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveSizeVariation"),
                new GUIContent(
                    "Shore Wave Size Variation",
                    "Stable deterministic differences between successive shore waves. This changes overall height and lateral reach without live reseeding."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveSideAsymmetry"),
                new GUIContent(
                    "Shore Side Asymmetry",
                    "Makes left and right banks use increasingly independent Size Variation and Profile Variation."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveProfileVariation"),
                new GUIContent(
                    "Shore Wave Profile Variation",
                    "Varies each shore wave smoothly between its start, middle, and end, affecting both height and lateral reach. Zero preserves the former uniform repeating wave."));

            EditorGUILayout.PropertyField(Find("motionDebugView"), new GUIContent("Motion Debug View", "Bank mask, macro height, surface normal, current accents, or liquid factor."));

            if (EditorGUI.EndChangeCheck())
            {
                Find("motionPreset").enumValueIndex =
                    (int)StylizedRiverMotionPreset.Custom;
            }

            if (targets.Length == 1 && target is StylizedRiver singleRiver)
            {
                EditorGUILayout.Space(3f);
                EditorGUILayout.LabelField(
                    "Resolved Surface Row Spacing",
                    $"{singleRiver.ResolvedSurfaceLongitudinalSpacing:0.000} m");
                EditorGUILayout.LabelField(
                    "Resolved Downward Clearance",
                    $"{singleRiver.ResolvedMaximumDownwardMotion:0.000} m");
            }
        }

        private void DrawRefraction()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Refraction and Optical Distortion",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 4 distorts only the already-lit opaque scene beneath the river. It uses the Stage 3 surface normal, actual water depth, shoreline protection, and depth-discontinuity rejection. Defaults are intentionally restrained.",
                MessageType.Info);

            SerializedProperty preset = Find("refractionPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Refraction Character",
                    "None reproduces the accepted Stage 3 result. Clear, Balanced, and Distorted remain bounded screen-space styles rather than physically exact refraction."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply River Refraction Preset");
                    river.ApplyRefractionPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("liquidRefractionStrength"),
                new GUIContent(
                    "Refraction Strength",
                    "Maximum liquid screen-space distortion. Small values are recommended."));
            EditorGUILayout.PropertyField(
                Find("refractionDepthInfluence"),
                new GUIContent(
                    "Depth Influence",
                    "How strongly shallow water suppresses distortion while deeper water reaches the configured strength."));
            EditorGUILayout.PropertyField(
                Find("refractionNormalInfluence"),
                new GUIContent(
                    "Normal Influence",
                    "How strongly the completed Stage 3 surface normal drives optical distortion."));
            EditorGUILayout.PropertyField(
                Find("shoreRefraction"),
                new GUIContent(
                    "Shore Refraction",
                    "Distortion retained at the visible bank. It still fades to zero before the buried surface edge."));
            EditorGUILayout.PropertyField(
                Find("depthEdgeProtection"),
                new GUIContent(
                    "Depth-Edge Protection",
                    "Rejects displaced samples that cross rocks, banks, foreground objects, or other strong scene-depth discontinuities."));

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField(
                "Advanced",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("preserveObjectSilhouettes"),
                new GUIContent(
                    "Preserve Object Silhouettes",
                    "Uses the depth samples already required by refraction to reduce object-edge contraction and pale disocclusion ghosts. It adds shader arithmetic but no extra texture samples."));

            EditorGUILayout.PropertyField(
                Find("iceDistortionStrength"),
                new GUIContent(
                    "Ice Distortion",
                    "Static optical warping through frozen ice. It does not scroll with liquid flow."));
            EditorGUILayout.PropertyField(
                Find("iceDiffusion"),
                new GUIContent(
                    "Ice Diffusion",
                    "Quality-scaled softening of the transmitted scene beneath ice. Ice Cloudiness also contributes automatically."));
            EditorGUILayout.PropertyField(
                Find("refractionDebugView"),
                new GUIContent(
                    "Refraction Debug View",
                    "Displays the refracted scene, offset, depth influence, shoreline mask, sample validity, or ice diffusion."));

            if (EditorGUI.EndChangeCheck())
            {
                Find("refractionPreset").enumValueIndex =
                    (int)StylizedRiverRefractionPreset.Custom;
            }
        }

        private void DrawRuntimeDisturbances()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Runtime Disturbance and Interaction",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 5 exposes one shared Pressure response and one shared Wake response. Stationary geometry and dynamic emitters prepare different source data, but consume the same river-level visual rules. Impact Ripples remain a separate event system.",
                MessageType.Info);

            SerializedProperty enabledProperty = Find("runtimeDisturbances");
            SerializedProperty presetProperty = Find("disturbancePreset");
            SerializedProperty staticPressureStrengthProperty =
                Find("staticPressureStrength");
            SerializedProperty staticPressureContactSharpnessProperty =
                Find("staticPressureContactSharpness");
            SerializedProperty staticPressureWaveResponseProperty =
                Find("staticPressureWaveResponse");
            SerializedProperty staticPressureProfileChangeIntervalMinProperty =
                Find("staticPressureProfileChangeIntervalMin");
            SerializedProperty staticPressureProfileChangeIntervalMaxProperty =
                Find("staticPressureProfileChangeIntervalMax");
            SerializedProperty obstructionWakeStrengthProperty =
                Find("obstructionWakeStrength");
            SerializedProperty obstructionWakeReachProperty =
                Find("obstructionWakeReach");
            SerializedProperty obstructionWakeSpreadProperty =
                Find("obstructionWakeSpread");
            SerializedProperty obstructionWakeVariationProperty =
                Find("obstructionWakeVariation");
            SerializedProperty obstructionWakeVariationIntervalMinProperty =
                Find("obstructionWakeVariationIntervalMin");
            SerializedProperty obstructionWakeVariationIntervalMaxProperty =
                Find("obstructionWakeVariationIntervalMax");
            SerializedProperty obstructionWakeWideningProperty =
                Find("obstructionWakeWidening");
            SerializedProperty obstructionWakeSurfaceHeightProperty =
                Find("obstructionWakeSurfaceHeight");
            SerializedProperty obstructionWakeSurfaceCompactnessProperty =
                Find("obstructionWakeSurfaceCompactness");
            SerializedProperty impactRippleStrengthProperty =
                Find("impactRippleStrength");
            SerializedProperty impactRippleRidgeEmphasisProperty =
                Find("impactRippleRidgeEmphasis");
            SerializedProperty impactRipplePropagationProperty =
                Find("impactRipplePropagation");
            SerializedProperty impactRippleDecayProperty =
                Find("impactRippleDecay");
            SerializedProperty impactRippleFlowDissipationProperty =
                Find("impactRippleFlowDissipation");
            SerializedProperty impactRippleMinimumVisibleEnergyProperty =
                Find("impactRippleMinimumVisibleEnergy");
            SerializedProperty impactRippleMaximumLifetimeProperty =
                Find("impactRippleMaximumLifetime");
            SerializedProperty impactRippleShoreReflectionProperty =
                Find("impactRippleShoreReflection");
            SerializedProperty impactRippleObstacleReflectionProperty =
                Find("impactRippleObstacleReflection");
            SerializedProperty impactRippleTestDistanceProperty =
                Find("impactRippleTestDistanceNormalized");
            SerializedProperty impactRippleTestAcrossProperty =
                Find("impactRippleTestAcrossNormalized");
            SerializedProperty impactRippleTestEventProperty =
                Find("impactRippleTestEvent");
            SerializedProperty debugViewProperty =
                Find("disturbanceDebugView");

            string missingProperties = string.Empty;
            if (enabledProperty == null)
            {
                missingProperties += "runtimeDisturbances";
            }
            if (presetProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", disturbancePreset"
                    : "disturbancePreset";
            }
            if (staticPressureStrengthProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", staticPressureStrength"
                    : "staticPressureStrength";
            }
            if (staticPressureContactSharpnessProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", staticPressureContactSharpness"
                    : "staticPressureContactSharpness";
            }
            if (staticPressureWaveResponseProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", staticPressureWaveResponse"
                    : "staticPressureWaveResponse";
            }
            if (staticPressureProfileChangeIntervalMinProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", staticPressureProfileChangeIntervalMin"
                    : "staticPressureProfileChangeIntervalMin";
            }
            if (staticPressureProfileChangeIntervalMaxProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", staticPressureProfileChangeIntervalMax"
                    : "staticPressureProfileChangeIntervalMax";
            }
            if (obstructionWakeStrengthProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeStrength"
                    : "obstructionWakeStrength";
            }
            if (obstructionWakeReachProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeReach"
                    : "obstructionWakeReach";
            }
            if (obstructionWakeSpreadProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeSpread"
                    : "obstructionWakeSpread";
            }
            if (obstructionWakeVariationProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeVariation"
                    : "obstructionWakeVariation";
            }
            if (obstructionWakeVariationIntervalMinProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeVariationIntervalMin"
                    : "obstructionWakeVariationIntervalMin";
            }
            if (obstructionWakeVariationIntervalMaxProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeVariationIntervalMax"
                    : "obstructionWakeVariationIntervalMax";
            }
            if (obstructionWakeWideningProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeWidening"
                    : "obstructionWakeWidening";
            }
            if (obstructionWakeSurfaceHeightProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeSurfaceHeight"
                    : "obstructionWakeSurfaceHeight";
            }
            if (obstructionWakeSurfaceCompactnessProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", obstructionWakeSurfaceCompactness"
                    : "obstructionWakeSurfaceCompactness";
            }
            if (impactRippleStrengthProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleStrength"
                    : "impactRippleStrength";
            }
            if (impactRippleRidgeEmphasisProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleRidgeEmphasis"
                    : "impactRippleRidgeEmphasis";
            }
            if (impactRipplePropagationProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRipplePropagation"
                    : "impactRipplePropagation";
            }
            if (impactRippleDecayProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleDecay"
                    : "impactRippleDecay";
            }
            if (impactRippleFlowDissipationProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleFlowDissipation"
                    : "impactRippleFlowDissipation";
            }
            if (impactRippleMinimumVisibleEnergyProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleMinimumVisibleEnergy"
                    : "impactRippleMinimumVisibleEnergy";
            }
            if (impactRippleMaximumLifetimeProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleMaximumLifetime"
                    : "impactRippleMaximumLifetime";
            }
            if (impactRippleShoreReflectionProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleShoreReflection"
                    : "impactRippleShoreReflection";
            }
            if (impactRippleObstacleReflectionProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleObstacleReflection"
                    : "impactRippleObstacleReflection";
            }
            if (impactRippleTestDistanceProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleTestDistanceNormalized"
                    : "impactRippleTestDistanceNormalized";
            }
            if (impactRippleTestAcrossProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleTestAcrossNormalized"
                    : "impactRippleTestAcrossNormalized";
            }
            if (impactRippleTestEventProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", impactRippleTestEvent"
                    : "impactRippleTestEvent";
            }
            if (debugViewProperty == null)
            {
                missingProperties += missingProperties.Length > 0
                    ? ", disturbanceDebugView"
                    : "disturbanceDebugView";
            }

            if (missingProperties.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    "The StylizedRiver Inspector and runtime component do not match. Missing serialized properties: " +
                    missingProperties +
                    ". No missing property will be drawn.",
                    MessageType.Error);
            }

            if (enabledProperty != null)
            {
                EditorGUILayout.PropertyField(
                    enabledProperty,
                    new GUIContent(
                        "Runtime Disturbances",
                        "Master switch for Stage 5 Pressure, Wake, and Impact Ripples. Off releases or avoids disturbance textures and reproduces Stage 4 water; registered geometry remains available but contributes no disturbance."));
            }

            if (presetProperty != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    presetProperty,
                    new GUIContent(
                        "Disturbance Character",
                        "Applies a coordinated starting preset to Pressure, Wake, and Impact Ripple river-level controls. Editing any individual response control returns this field to Custom; source-specific emitter settings are not changed."));

                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();

                    foreach (Object selectedTarget in targets)
                    {
                        if (selectedTarget is not StylizedRiver river)
                        {
                            continue;
                        }

                        Undo.RecordObject(river, "Apply River Disturbance Preset");
                        river.ApplyDisturbancePreset();
                        EditorUtility.SetDirty(river);
                    }

                    serializedObject.Update();
                    RepaintScene();
                }
            }

            bool controlsDisabled =
                enabledProperty != null &&
                !enabledProperty.hasMultipleDifferentValues &&
                !enabledProperty.boolValue;

            using (new EditorGUI.DisabledScope(controlsDisabled))
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Pressure",
                    EditorStyles.miniBoldLabel);
                if (staticPressureStrengthProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureStrengthProperty,
                        new GUIContent(
                            "Strength",
                            "Selects a point inside each source's computed safe Pressure-height range. Zero removes attached buildup; one uses the maximum geometry-, support-, and flow-safe height without bypassing rear protection."));
                }
                if (staticPressureContactSharpnessProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureContactSharpnessProperty,
                        new GUIContent(
                            "Contact Sharpness",
                            "Shapes the short open-water falloff from source contact. Lower values make a broader, softer ridge; higher values make it tighter and steeper. This does not raise the crest-height ceiling."));
                }
                if (staticPressureWaveResponseProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureWaveResponseProperty,
                        new GUIContent(
                            "Profile Variation",
                            "Controls deterministic lateral reshaping of the Pressure ridge. Zero keeps the cached geometry-derived profile fixed; one gives the normal variation range; two permits the strongest bounded redistribution. It is independent from Stage 3 waves."));
                }
                if (staticPressureProfileChangeIntervalMinProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureProfileChangeIntervalMinProperty,
                        new GUIContent(
                            "Minimum Change Interval",
                            "Shortest randomized delay, in seconds, before each stationary Pressure source chooses a new lateral profile target. The profile morphs smoothly rather than switching instantly."));
                }
                if (staticPressureProfileChangeIntervalMaxProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureProfileChangeIntervalMaxProperty,
                        new GUIContent(
                            "Maximum Change Interval",
                            "Longest randomized delay, in seconds, before each stationary Pressure source chooses a new lateral profile target. Sources are independent, and each smooth morph completes before the next target."));
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Wake",
                    EditorStyles.miniBoldLabel);
                if (obstructionWakeStrengthProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeStrengthProperty,
                        new GUIContent(
                            "Strength",
                            "Shared Wake response after source preparation. Zero removes the authored lee/release response; higher values deepen the attached lee and inject more transported Wake energy. Stationary and dynamic sources use this same river-level value."));
                }
                if (obstructionWakeReachProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeReachProperty,
                        new GUIContent(
                            "Reach",
                            "Controls how far the prepared Wake source and retained energy are allowed to influence downstream water. Higher values extend persistence and active range; this does not change river Flow Speed."));
                }
                if (obstructionWakeSpreadProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSpreadProperty,
                        new GUIContent(
                            "Spread",
                            "Initial across-river Wake source width. Stationary geometry uses it for the attached lee and rear releases; dynamic emitters use it for their swept footprint. Downstream diffusion is controlled separately by Widening."));
                }
                if (obstructionWakeVariationProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationProperty,
                        new GUIContent(
                            "Variation",
                            "Allowed spatial change in Wake source shape. Zero keeps stationary lee/release geometry stable; one permits the full bounded variation range. It does not pulse or globally brighten the persistent field."));
                }
                if (obstructionWakeVariationIntervalMinProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationIntervalMinProperty,
                        new GUIContent(
                            "Minimum Variation Interval",
                            "Shortest randomized delay, in seconds, before a stationary Wake source chooses new lee and independent left/right release targets."));
                }
                if (obstructionWakeVariationIntervalMaxProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationIntervalMaxProperty,
                        new GUIContent(
                            "Maximum Variation Interval",
                            "Longest randomized delay, in seconds, before a stationary Wake source chooses new lee and independent left/right release targets. Transitions occupy about 85% of the chosen interval."));
                }
                if (obstructionWakeWideningProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeWideningProperty,
                        new GUIContent(
                            "Widening",
                            "Lateral diffusion after Wake energy enters the shared persistent field. Lower values keep trails narrow for longer; higher values broaden and merge them sooner. This does not change initial source width."));
                }
                if (obstructionWakeSurfaceHeightProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSurfaceHeightProperty,
                        new GUIContent(
                            "Wake Surface Height",
                            "Maximum positive water-surface displacement, in metres, extracted from the compact core of transported Wake energy. Zero preserves transport, normals, intensity, and the separate negative lee but adds no positive transported Wake height."));
                }
                if (obstructionWakeSurfaceCompactnessProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSurfaceCompactnessProperty,
                        new GUIContent(
                            "Wake Surface Compactness",
                            "Controls which part of the broad transported Wake field becomes positive geometry. Lower values create a broader, stronger visible rise; higher values restrict height to the strongest core. Transport, normals, intensity, and future foam data are unchanged."));
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Impact Ripples",
                    EditorStyles.miniBoldLabel);
                if (impactRippleStrengthProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleStrengthProperty,
                        new GUIContent(
                            "Strength",
                            "Master multiplier for Impact Ripple height, velocity, initial elevation, and normal detail. The nonlinear mapping makes 1 roughly equivalent to the former 2.6. Values from 0–1.5 are the normal authoring range, 2–3 are exaggerated stress settings, and 4 is an intentional override level for exceptional impacts."));
                }
                if (impactRippleRidgeEmphasisProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleRidgeEmphasisProperty,
                        new GUIContent(
                            "Ridge Emphasis",
                            "Emphasizes only the raised ripple ridge: its positive height, outward velocity, and normal-detail edge. It does not deepen the centre, change radius, propagation, decay, reflections, or initial elevation. Values above 1 make the ridge slightly sharper and more noticeable."));
                }
                if (impactRipplePropagationProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRipplePropagationProperty,
                        new GUIContent(
                            "Propagation",
                            "Approximate world-space wavefront expansion speed in metres per second. This controls radial spreading through local river metrics; river Flow Speed separately advects the ripple downstream."));
                }
                if (impactRippleDecayProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleDecayProperty,
                        new GUIContent(
                            "Decay",
                            "Base exponential loss per second. Effective Decay = Decay + abs(Flow Speed) × Flow Dissipation. Higher values shorten the legacy surface overlay lifetime in still water."));
                }
                if (impactRippleFlowDissipationProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleFlowDissipationProperty,
                        new GUIContent(
                            "Flow Dissipation",
                            "Adds decay in direct proportion to river speed: abs(Flow Speed in m/s) × this value. Example: Decay 0.85, Flow Speed 2 m/s, and Flow Dissipation 0.15 produce Effective Decay 1.15/s. Set to zero when fast flow should advect without extra suppression."));
                }
                if (impactRippleMinimumVisibleEnergyProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleMinimumVisibleEnergyProperty,
                        new GUIContent(
                            "Minimum Visible Energy",
                            "CPU reservation threshold, not a direct visual cutoff. Once a predicted event envelope falls below this value, its future chunk reservation may end. Lower values preserve faint tails longer; higher values reduce active simulation sooner."));
                }
                if (impactRippleMaximumLifetimeProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleMaximumLifetimeProperty,
                        new GUIContent(
                            "Maximum Lifetime",
                            "Hard safety cap, in seconds, on how long one event may reserve future chunks. The reservation ends at this time even if it remains above Minimum Visible Energy, so very low values can clip extreme low-decay ripples."));
                }
                if (impactRippleShoreReflectionProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleShoreReflectionProperty,
                        new GUIContent(
                            "Shore Reflection",
                            "Controls shoreline boundary hardness after the shallow absorption band. Zero uses the most absorbing outgoing-wave response; higher values approach a harder no-flux reflection and make the broad return wave clearer. This is not a literal returned-energy percentage because the absorption band and normal ripple decay still apply."));
                }
                if (impactRippleObstacleReflectionProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleObstacleReflectionProperty,
                        new GUIContent(
                            "Obstacle Reflection",
                            "Controls registered-solid boundary hardness. Zero uses the most absorbing outgoing-wave response; higher values approach a hard no-flux reflection. This is not a literal returned-energy percentage because obstacle-edge absorption and normal ripple decay still apply."));
                }

                if (EditorGUI.EndChangeCheck() && presetProperty != null)
                {
                    presetProperty.enumValueIndex =
                        (int)StylizedRiverDisturbancePreset.Custom;
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Debug",
                    EditorStyles.miniBoldLabel);
                if (debugViewProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        debugViewProperty,
                        new GUIContent(
                            "Disturbance Debug View",
                            "Selects a Stage 5 diagnostic visualization for source fields, persistent fields, or composed disturbance geometry. It changes only the debug display and does not alter simulation state or authored values."));

                    if (!debugViewProperty.hasMultipleDifferentValues)
                    {
                        DrawDisturbanceDebugLegend(
                            (StylizedRiverDisturbanceDebugView)
                            debugViewProperty.intValue);
                    }
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    "Impact Ripple Test",
                    EditorStyles.miniBoldLabel);
                if (impactRippleTestDistanceProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleTestDistanceProperty,
                        new GUIContent(
                            "Longitudinal Position",
                            "Manual test location along this river: 0 is the domain start and 1 is the domain end. This affects only the Inspector test buttons."));
                }
                if (impactRippleTestAcrossProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleTestAcrossProperty,
                        new GUIContent(
                            "Across Position",
                            "Manual test location across the local water surface: -1 is the left edge, 0 is the centreline, and +1 is the right edge. This affects only the Inspector test buttons."));
                }
                if (impactRippleTestEventProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleTestEventProperty,
                        new GUIContent(
                            "Event",
                            "Profile used by the manual test buttons: initial radius, signed impulse, immediate elevation, centre/ring shape, sharpness, and separate geometry/normal contributions."),
                        true);
                }
            }

            if (targets.Length != 1 || target is not StylizedRiver singleRiver)
            {
                return;
            }

            StylizedRiverDisturbanceRuntime runtime =
                singleRiver.GetComponent<StylizedRiverDisturbanceRuntime>();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Runtime Diagnostics",
                EditorStyles.miniBoldLabel);

            if (runtime == null)
            {
                EditorGUILayout.LabelField(
                    "Runtime",
                    singleRiver.RuntimeDisturbancesEnabled
                        ? "Will be created automatically"
                        : "Not allocated");

                if (singleRiver.RuntimeDisturbancesEnabled &&
                    GUILayout.Button(new GUIContent(
                        "Create Disturbance Runtime",
                        "Creates the hidden river-owned runtime component immediately. Normally it is created automatically when an enabled disturbance source or manual test first needs it.")))
                {
                    runtime = singleRiver.GetOrCreateDisturbanceRuntime();
                    EditorUtility.SetDirty(singleRiver);
                }

                return;
            }

            EditorGUILayout.LabelField(
                "Compute Support",
                runtime.IsSupported ? "Available" : "Unavailable");
            EditorGUILayout.LabelField(
                "Field",
                runtime.IsAllocated
                    ? $"{runtime.FieldWidth} × {runtime.FieldHeight}"
                    : "Sleeping / not allocated");
            EditorGUILayout.LabelField(
                "Logical Chunks",
                $"{runtime.ActiveChunkCount} active / {runtime.ChunkCount} total");
            EditorGUILayout.LabelField(
                "Simulation Rate",
                $"{runtime.SimulationRate:0} Hz");
            EditorGUILayout.LabelField(
                "Wake Field",
                runtime.IsAllocated
                    ? $"{runtime.WakeFieldWidth} × {runtime.WakeFieldHeight}"
                    : "Sleeping / not allocated");
            EditorGUILayout.LabelField(
                "Wake Update Rate",
                $"{runtime.WakeSimulationRate:0} Hz");
            EditorGUILayout.LabelField(
                "Wake Chunks",
                $"{runtime.ActiveWakeChunkCount} active / {runtime.ChunkCount} total");
            EditorGUILayout.LabelField(
                "Continuous Sources",
                runtime.ContinuousSourceCount.ToString());
            EditorGUILayout.LabelField(
                "Pending Impacts",
                runtime.PendingImpactCount.ToString());
            EditorGUILayout.LabelField(
                "Active Ripple Reservations",
                runtime.ActiveImpactReservationCount.ToString());
            EditorGUILayout.LabelField(
                "Longest Reservation Remaining",
                runtime.ActiveImpactReservationCount > 0
                    ? $"{runtime.LongestImpactReservationRemainingSeconds:0.00} s"
                    : "Inactive");
            EditorGUILayout.LabelField(
                "Resolved Ripple Strength",
                singleRiver.ResolvedImpactRippleStrength.ToString("0.00"));
            EditorGUILayout.LabelField(
                "Effective Ripple Decay",
                $"{singleRiver.ResolvedImpactRippleDecay:0.00} /s");
            EditorGUILayout.LabelField(
                "Impacts Injected Last Step",
                runtime.ImpactsInjectedLastStep.ToString());
            EditorGUILayout.LabelField(
                "Ripple Internal Substeps",
                runtime.CurrentRippleSubstepCount.ToString());
            EditorGUILayout.LabelField(
                "Maximum Recent Ripple Substeps",
                runtime.MaximumRecentRippleSubstepCount.ToString());
            EditorGUILayout.LabelField(
                "Ripple Metric Rows",
                runtime.RippleMetricRowCount.ToString());
            EditorGUILayout.LabelField(
                "Ripple Boundary Mask",
                runtime.IsAllocated
                    ? $"{runtime.RippleBoundaryWidth} × {runtime.RippleBoundaryHeight}"
                    : "Sleeping / not allocated");
            EditorGUILayout.LabelField(
                "Ripple Collision Sources",
                runtime.RippleCollisionSourceCount.ToString());
            EditorGUILayout.LabelField(
                "Active Ripple Minimum Cell",
                runtime.ActiveRippleMinimumCellSize > 0f
                    ? $"{runtime.ActiveRippleMinimumCellSize:0.000} m"
                    : "Inactive");
            EditorGUILayout.LabelField(
                "Ripple Substep Limit",
                runtime.RippleSubstepLimitReached
                    ? "Reached — reduce Propagation or increase local cell size"
                    : "Within limit");
            EditorGUILayout.LabelField(
                "Estimated Field Memory",
                $"{runtime.EstimatedMemoryBytes / (1024f * 1024f):0.00} MB");
            EditorGUILayout.LabelField(
                "State",
                runtime.IsSleeping ? "Sleeping" : "Active");

            EditorGUILayout.Space(3f);
            showPerformanceDiagnostics = EditorGUILayout.Foldout(
                showPerformanceDiagnostics,
                "Performance Diagnostics",
                true);
            if (showPerformanceDiagnostics)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.HelpBox(
                    "These counters estimate submitted compute workload; they are not CPU or GPU time. 'Last Update' is the latest river LateUpdate, and 'Recent Peak' is the highest value observed during the current five-second window.",
                    MessageType.Info);

                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Compute Dispatches",
                        "Number of compute-kernel submissions made by this river. Several dispatches may occur during one simulation update because active chunks can form separate ranges and Ripple may use internal stability substeps."),
                    new GUIContent($"{runtime.LastUpdateComputeDispatchCount:N0} last / {runtime.RecentPeakComputeDispatchCount:N0} peak"));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Thread Groups",
                        "Total compute thread groups submitted. This is a better rough workload comparison than dispatch count because larger dispatches contain more groups."),
                    new GUIContent($"{runtime.LastUpdateThreadGroupCount:N0} last / {runtime.RecentPeakThreadGroupCount:N0} peak"));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Estimated Cell-Iterations",
                        "Approximate field cells processed across all dispatches. Ripple stability substeps and separated active ranges are counted. This is workload, not measured GPU time."),
                    new GUIContent($"{runtime.LastUpdateCellIterationCount:N0} last / {runtime.RecentPeakCellIterationCount:N0} peak"));

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Last Update Dispatch Breakdown",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Ripple Simulation",
                        "Ripple propagation dispatches. Multiple dispatches may be submitted for separated active chunk ranges and for each internal stability substep."),
                    new GUIContent(runtime.LastUpdateRippleSimulationDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Wake Simulation",
                        "Persistent Wake transport and widening dispatches for active Wake chunk ranges."),
                    new GUIContent(runtime.LastUpdateWakeSimulationDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Impact Injection",
                        "One-shot Impact Ripple injection dispatches submitted during the latest river update."),
                    new GUIContent(runtime.LastUpdateImpactInjectionDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Dynamic Wake Injection",
                        "Continuous dynamic-emitter Wake injection dispatches. The full relative-motion dynamic source model remains deferred."),
                    new GUIContent(runtime.LastUpdateWakeInjectionDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Pressure Bakes",
                        "Cached stationary Pressure source and finalization dispatches rebuilt during the latest update. These may recur while Pressure Profile Variation is active; unexpectedly high counts without authored variation can indicate avoidable rebaking."),
                    new GUIContent(runtime.LastUpdateStaticPressureBakeDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Wake Source Bakes",
                        "Cached stationary Wake source dispatches rebuilt during the latest update. These may recur while Wake Variation is active; otherwise they should mainly follow geometry, domain, or setting changes."),
                    new GUIContent(runtime.LastUpdateStaticWakeBakeDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Boundary Bakes",
                        "Ripple shore/obstacle boundary generation and state-application dispatches. These should occur only when the domain, resources, quality, or registered collision geometry changes."),
                    new GUIContent(runtime.LastUpdateRippleBoundaryBakeDispatchCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Clear Dispatches",
                        "Texture-region clear operations, including allocation, sleeping cleanup, and source-field rebuild preparation."),
                    new GUIContent(runtime.LastUpdateClearDispatchCount.ToString("N0")));

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Source and Rebuild State",
                    EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Registered Stationary Sources",
                        "Stationary generated-geometry sources currently owned by this river runtime, regardless of whether each individual feature is enabled."),
                    new GUIContent(runtime.RegisteredStationarySourceCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Valid Pressure Sources",
                        "Stationary sources that contributed to the latest cached Pressure target rebuild."),
                    new GUIContent(runtime.ValidStaticPressureSourceCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Valid Wake Sources",
                        "Stationary sources that contributed to the latest cached Wake source rebuild."),
                    new GUIContent(runtime.ValidStaticWakeSourceCount.ToString("N0")));
                EditorGUILayout.LabelField(
                    new GUIContent(
                        "Field Rebuilds",
                        "Resource allocation or cached Pressure, Wake, and Ripple Boundary rebuilds during the latest update. The peak uses the same five-second window."),
                    new GUIContent($"{runtime.LastUpdateFieldRebuildCount:N0} last / {runtime.RecentPeakFieldRebuildCount:N0} peak"));

                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField(
                    "Allocated Memory Estimate",
                    EditorStyles.miniBoldLabel);
                DrawMemoryDiagnostic(
                    "Ripple State",
                    runtime.RippleStateMemoryBytes,
                    "Two persistent RGBA-half Ripple height/velocity/detail textures.");
                DrawMemoryDiagnostic(
                    "Static Pressure",
                    runtime.StaticPressureMemoryBytes,
                    "Cached RGBA-half stationary Pressure target texture.");
                DrawMemoryDiagnostic(
                    "Ripple Boundary",
                    runtime.RippleBoundaryMemoryBytes,
                    "Cached RG-half shore and stationary-obstacle boundary texture.");
                DrawMemoryDiagnostic(
                    "Wake State and Source",
                    runtime.WakeFieldMemoryBytes,
                    "Two persistent RGBA-half Wake textures plus the cached stationary Wake source texture.");
                DrawMemoryDiagnostic(
                    "Ripple Metrics",
                    runtime.RippleMetricMemoryBytes,
                    "Compact structured buffer containing the world-space river frame and widths for each longitudinal Ripple row.");
                DrawMemoryDiagnostic(
                    "Total",
                    runtime.EstimatedMemoryBytes,
                    "Estimated disturbance-field texture and metric-buffer memory for this river. Driver and allocation overhead are not included.");

                if (GUILayout.Button(new GUIContent(
                        "Reset Recent Peaks",
                        "Clears the five-second peak counters. Last Update values continue reporting the current workload.")))
                {
                    runtime.ResetPerformanceDiagnosticPeaks();
                }

                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button(new GUIContent(
                    "Clear Field",
                    "Immediately clears Pressure, Wake, and Impact Ripple runtime textures and pending transient state for this river. Authored settings and registered sources are not removed.")))
            {
                runtime.ClearField();
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(
                        "Emit Test Impact",
                        "In Play Mode, emits the configured Event at the selected longitudinal and across-river position.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugImpact(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }

                if (GUILayout.Button(new GUIContent(
                        "Emit Opposite Sign",
                        "In Play Mode, emits the same Event after reversing Signed Impulse and Initial Elevation. Radius, shape, sharpness, and contributions stay unchanged.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOppositeSignImpact(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent(
                        "Emit Overlapping Pair",
                        "In Play Mode, emits two nearby copies of the configured Event to test reinforcement, overlap, stability, and shared-field composition.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOverlappingPair(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }

                if (GUILayout.Button(new GUIContent(
                        "Emit Near Shore",
                        "In Play Mode, emits the configured Event close to the selected side of the river. Tests the cached shoreline absorption band and its weak reflected return wave.")))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugNearShore(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        private void ApplyImpactTestProperties()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private static void DrawDisturbanceDebugLegend(
            StylizedRiverDisturbanceDebugView debugView)
        {
            string description = debugView switch
            {
                StylizedRiverDisturbanceDebugView.StaticWakeSource =>
                    "Static Wake Source: red is rear-release energy, green is the attached geometry-aware lee, and blue is reach/persistence metadata.",
                StylizedRiverDisturbanceDebugView.WakeEnergy =>
                    "Wake Energy: red shows the shared persistent wake field after injection, transport, widening, decay, bank masking, and freeze suppression.",
                StylizedRiverDisturbanceDebugView.RippleBoundary =>
                    "Ripple Boundary: green is open water, black is fully absorbing coverage, and red shows reflection hardness. Shores appear as soft dark-red/green absorption bands; participating registered solids appear as compact brighter-red boundaries with dark interiors.",
                StylizedRiverDisturbanceDebugView.FinalWakeGeometryHeight =>
                    "Final Wake Geometry Height: mid-gray is zero, darker values are the attached lee depression, and brighter values are positive transported trail height. The fixed encoding spans -0.40 m to +0.40 m.",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.HelpBox(description, MessageType.None);
            }
        }

        private static void DrawMemoryDiagnostic(
            string label,
            long bytes,
            string tooltip)
        {
            EditorGUILayout.LabelField(
                new GUIContent(label, tooltip),
                new GUIContent($"{bytes / (1024f * 1024f):0.00} MB"));
        }

        private static string FormatPercent(float value)
        {
            return $"{Mathf.Clamp01(value) * 100f:0.0}%";
        }

        private void DrawFoam()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField(
                "Foam and Surface Tracing",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 6.2 retains Pressure Support, stationary Lee Support, dynamic Shore Support, and the water-level-aware Obstacle Footprint. Patch 4.9C.1 makes the development workflow Play-only: valid caches load directly, while missing or stale caches generate, validate, and persist automatically. Release builds remain strictly cache-only.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                Find("foamEnabled"),
                new GUIContent(
                    "Foam Enabled",
                    "Master switch for the shared persistent Foam field. Disabled Foam allocates no simulation textures and contributes nothing to the water shader."));
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(
                    Find("foamTopologyCacheAsset"),
                    new GUIContent(
                        "Topology Cache Asset",
                        "Persistent topology payload associated with this authored river. The Editor coordinator creates and assigns one automatically before Play Mode when this field is empty. Valid payloads load directly; missing or stale development payloads rebuild and save automatically."));
            }

            EditorGUILayout.PropertyField(
                Find("foamMajorSupportAmount"),
                new GUIContent(
                    "Major Support Amount",
                    "Controls the nested deterministic population of whole-river Major Support. Higher values activate later-ranked opportunities without moving or reshaping earlier accepted regions. It does not alter the separate local candidate preview."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSize"),
                new GUIContent(
                    "Major Support Size",
                    "Controls the physical scale envelope of the same stable whole-river opportunities. It preserves opportunity identity and does not enlarge the separate local candidate preview."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSizeVariation"),
                new GUIContent(
                    "Major Support Size Variation",
                    "Controls the relative size spread between stable Major opportunities. Zero makes their scale multipliers uniform, 0.5 preserves the Patch 2 distribution, and one strongly separates the smallest and largest regions without overriding river-width or placement limits."));
            EditorGUILayout.PropertyField(
                Find("foamMajorRecycleTerritoryDeviationPercent"),
                new GUIContent(
                    "Major Recycle Territory Deviation (%)",
                    "Maximum longitudinal deviation from each Major's original accepted river position when it recycles. A value of 3 permits respawn within approximately original position ±3% of valid river length. Near-egress originals are shifted upstream enough to retain a useful movement runway."));
            EditorGUILayout.PropertyField(
                Find("foamMajorLifetimeUnits"),
                new GUIContent(
                    "Major Lifetime Units",
                    "Average combined lifetime budget for one Major occurrence. Approximately one normal five-second dwell-plus-move cycle consumes one unit through both elapsed time and completed hops. Higher values delay local recycling."));
            EditorGUILayout.PropertyField(
                Find("foamMajorLifetimeUnitDeviation"),
                new GUIContent(
                    "Major Lifetime Unit Deviation",
                    "Deterministic plus-or-minus variation around Major Lifetime Units for each occurrence. A base of 6 and deviation of 2 produces approximately 4–8 allocated units, with a minimum of one."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSeed"),
                new GUIContent(
                    "Major Support Seed",
                    "Deterministic seed for the field-first candidate proof and stable whole-river opportunity identity, candidate assignment, transforms, and future evolution metadata. Identical inputs reproduce identical static Major topology."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorAmount"),
                new GUIContent(
                    "Connector Amount",
                    "Controls the accepted relationship population. Zero keeps only the strongest sparse relationships, 0.5 preserves Patch 3, and one permits more secondary connections plus bounded overlap without creating an all-to-all web."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorDirectness"),
                new GUIContent(
                    "Connector Directness",
                    "One preserves near-facing endpoints and the shortest valid route. Lower values deliberately broaden endpoint choice and force one stable broad lateral bend when valid, without random wiggle."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorLengthPreference"),
                new GUIContent(
                    "Connector Length Preference",
                    "Controls which valid connection lengths are favoured inside one fixed safe envelope. Zero strongly favours short connections, 0.5 is neutral, and one strongly favours long connections. Safety, obstacle, path-length, and amount limits remain authoritative."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorBreakStretchRatio"),
                new GUIContent(
                    "Connector Break Stretch Ratio",
                    "Maximum live length relative to the reference captured when a relationship or recycle variant becomes active. The default 1.45 permits 45% growth. Exceeding the ratio breaks the relationship and attempts an immediate prepared rebind to a different Major pair."));
            EditorGUILayout.PropertyField(
                Find("foamInteriorPocketAmount"),
                new GUIContent(
                    "Interior Pocket Amount",
                    "Controls the nested deterministic population of closed Major-hosted negative regions. Zero disables Interior Pockets, 0.5 preserves approximately the accepted Patch 4 result, and one activates additional bounded opportunities without reshuffling earlier identities."));
            EditorGUILayout.PropertyField(
                Find("foamEdgeCavityAmount"),
                new GUIContent(
                    "Edge Cavity Amount",
                    "Controls the nested deterministic population of lopsided Major-hosted negative regions that deliberately breach one selected side while preserving a useful positive remainder. Zero disables them; 0.5 is the normal baseline; one permits the maximum bounded population."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorWeakSpanAmount"),
                new GUIContent(
                    "Connector Weak Span Amount",
                    "Controls the nested deterministic population of short Connector-hosted negative regions. Weak Spans remain associated with accepted Connector identities, stay away from endpoint gates, and locally weaken rather than delete the relationship. Zero disables them; 0.5 is the normal baseline; one permits the maximum bounded population."));
            EditorGUILayout.PropertyField(
                Find("foamFreeWaterEventAmount"),
                new GUIContent(
                    "Free-Water Event Amount",
                    "Controls the nested deterministic population of sparse valid-water negative events that require no Major or Connector host. Neutral or weakly supported opportunities activate first, but positive overlap remains permitted. Zero disables them; 0.5 is the normal sparse baseline; one permits the maximum bounded population."));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Material Lifecycle", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                Find("foamNeutralLifetime"),
                new GUIContent(
                    "Neutral Lifetime (s)",
                    "Normalized Remaining Life reaches zero after approximately this many seconds in neutral water. Support slows local aging; Negative Aging Pressure suppresses support preservation and accelerates local aging."));
            EditorGUILayout.PropertyField(
                Find("foamSupportedAgingRate"),
                new GUIContent(
                    "Supported Aging Rate",
                    "Aging-rate multiplier at full positive support. Values below one extend life. At the default 0.20, fully supported Foam ages five times more slowly than neutral Foam before negative overlap is considered."));
            EditorGUILayout.PropertyField(
                Find("foamNegativeAgingRate"),
                new GUIContent(
                    "Negative Aging Rate",
                    "Aging-rate multiplier at full Negative Aging Pressure. Values above one shorten life. Negative pressure first suppresses support preservation, then applies this faster aging response."));
            EditorGUILayout.PropertyField(
                Find("foamMaterialFlowSpeedMultiplier"),
                new GUIContent(
                    "Material Flow Speed",
                    "Multiplier for persistent Foam transport relative to the river Flow Speed. One follows the water, higher values move existing Foam downstream faster, and zero freezes ordinary downstream material drift without changing birth Amount or Remaining Life."));
            EditorGUILayout.PropertyField(
                Find("foamColour"),
                new GUIContent(
                    "Foam Colour",
                    "Lit, non-emissive Foam tint. The colour alpha controls maximum Foam opacity."));

            if (targets.Length != 1 || target is not StylizedRiver river)
            {
                return;
            }

            if (river.FoamTopologyCacheAsset == null)
            {
                EditorGUILayout.HelpBox(
                    Application.isPlaying
                        ? "No persistent asset was available for this Play session. The topology can still generate automatically, but it will remain session-only. On the next Play entry from a saved scene, the Editor coordinator will create and assign the cache automatically."
                        : "No cache asset is assigned. Press Play normally; the Editor coordinator will create and assign a deterministic asset automatically for this saved scene.",
                    MessageType.Info);
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();

            DrawFoamDebugLayer(river, runtime);
        }

        private void DrawFoamDebugLayer(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Foam Debug",
                EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Compact workflow: Overview for status, View for display mode, Material Motion for persistent downstream travel, Lifetime + Topology for aging/support, Material Probe for isolated lifetime checks, Manual Birth Source for all manual source controls, Material Shape for stored/visible footprint diagnostics, Runtime for resources, and Advanced Internals only for low-level failures.",
                MessageType.None);

            DrawFoamValidationOverview(river, runtime);
            DrawFoamViewModeSection(river);
            DrawFoamTransportMotionSection(river, runtime);
            DrawFoamLifetimeSection(river, runtime);
            DrawFoamMaterialProbeSection(river, runtime);
            DrawFoamManualBirthSourceSection(river, runtime);
            DrawFoamShapeResidueSection(runtime);
            DrawFoamRuntimeResourceSection(runtime);
            DrawFoamAdvancedInternalSection(runtime);
        }

        private void DrawFoamValidationOverview(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            showFoamValidationOverview = EditorGUILayout.Foldout(
                showFoamValidationOverview,
                "Overview",
                true);
            if (!showFoamValidationOverview)
            {
                return;
            }

            EditorGUI.indentLevel++;

            bool runtimeAvailable = runtime != null;
            bool resourcesAvailable = runtimeAvailable && runtime.ResourcesAllocated;
            float integratedArea = runtimeAvailable ? runtime.IntegratedPresenceArea : 0f;
            float visibleArea = runtimeAvailable ? runtime.VisiblePresenceCoreArea : 0f;
            float hiddenArea = Mathf.Max(0f, integratedArea - visibleArea);

            EditorGUILayout.LabelField(
                "State",
                river.FoamEnabled
                    ? runtimeAvailable
                        ? resourcesAvailable
                            ? runtime.IsSleeping ? "Ready / sleeping" : "Active"
                            : "Runtime present / no resources"
                        : "No runtime"
                    : "Disabled");
            EditorGUILayout.LabelField(
                "View",
                ObjectNames.NicifyVariableName(
                    ((StylizedRiverFoamDebugView)Find("foamDebugView").intValue)
                    .ToString()));
            EditorGUILayout.LabelField(
                "Foam Area",
                runtimeAvailable
                    ? $"visible {visibleArea:0.000} m² / stored {integratedArea:0.000} m² / hidden {hiddenArea:0.000} m²"
                    : "—");
            EditorGUILayout.LabelField(
                "Motion",
                runtimeAvailable ? ResolveFoamTransportSmoothnessStatus(runtime) : "Runtime unavailable");
            EditorGUILayout.LabelField(
                "Next Debug Section",
                ResolveFoamLikelyProblem(runtime));

            if (runtimeAvailable && hiddenArea > Mathf.Max(0.05f, visibleArea * 0.25f))
            {
                EditorGUILayout.HelpBox(
                    "Stored Foam is much larger than visible Foam. Open Material Shape if the visible mask looks wrong, or Lifetime + Topology if Foam should have died but remains stored.",
                    MessageType.Warning);
            }

            DrawFoamTransportWarnings(runtime);
            EditorGUI.indentLevel--;
        }

        private void DrawFoamViewModeSection(StylizedRiver river)
        {
            showFoamViewModes = EditorGUILayout.Foldout(
                showFoamViewModes,
                "View",
                true);
            if (!showFoamViewModes)
            {
                return;
            }

            EditorGUI.indentLevel++;

            SerializedProperty foamDebugProperty = Find("foamDebugView");
            string[] foamDebugLabels =
            {
                "Final Foam",
                "Foam + Aging Topology",
                "Progressive Birth Source",
                "Material Presence",
                "Material Remaining Life",
                "Foam Motion Field",
                "Foam Motion Field + Cell Grid",
                "Foam Evaluated Shape",
                "Foam Shape Difference"
            };
            int[] foamDebugValues =
            {
                (int)StylizedRiverFoamDebugView.Final,
                (int)StylizedRiverFoamDebugView.FoamAndAgingTopology,
                (int)StylizedRiverFoamDebugView.ProgressiveBirthSource,
                (int)StylizedRiverFoamDebugView.MaterialPresence,
                (int)StylizedRiverFoamDebugView.MaterialRemainingLife,
                (int)StylizedRiverFoamDebugView.FoamMotionField,
                (int)StylizedRiverFoamDebugView.FoamMotionFieldCellGrid,
                (int)StylizedRiverFoamDebugView.FoamEvaluatedShape,
                (int)StylizedRiverFoamDebugView.FoamShapeDifference
            };
            int currentDebugIndex = System.Array.IndexOf(
                foamDebugValues,
                foamDebugProperty.intValue);
            if (currentDebugIndex < 0)
            {
                currentDebugIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            int selectedDebugIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug View",
                    "Final Foam is the normal render. Foam Motion Field views show external routing/deformation intent and raw stored Foam Presence overlay; Foam Evaluated Shape and Shape Difference show the Layer D visual product, not persistent material truth."),
                currentDebugIndex,
                foamDebugLabels);
            if (EditorGUI.EndChangeCheck())
            {
                foamDebugProperty.intValue =
                    foamDebugValues[selectedDebugIndex];
            }

            StylizedRiverFoamDebugView selectedFoamDebug =
                (StylizedRiverFoamDebugView)foamDebugProperty.intValue;
            EditorGUILayout.LabelField(
                "Use For",
                GetFoamDebugViewDescription(selectedFoamDebug));

            EditorGUI.indentLevel--;
        }

        private void DrawFoamTransportMotionSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            showFoamTransportDiagnostics = EditorGUILayout.Foldout(
                showFoamTransportDiagnostics,
                "Material Motion",
                true);
            if (!showFoamTransportDiagnostics)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                Find("foamMaterialFlowSpeedMultiplier"),
                new GUIContent(
                    "Material Flow Speed",
                    "Multiplier for persistent Foam downstream travel."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldStrength"),
                new GUIContent(
                    "Motion Field Strength",
                    "Strength of the generated dense Foam Motion Field intent/debug texture. The field is currently used for visualization and future routing/deformation work; it does not currently move persistent Foam material."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldScrollHz"),
                new GUIContent(
                    "Motion Field Scroll Hz",
                    "Complete downstream wraps per second for the generated intent/debug lane field. This is a sample-coordinate phase scroll only, not active persistent Foam material motion and not a field rebuild rate."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldNeutralCoverage"),
                new GUIContent(
                    "Motion Field Neutral Coverage",
                    "Approximate fraction of the generated intent/debug lane field that resolves to neutral/no lateral direction. Changing it regenerates the lane texture only; it does not activate lateral material transport."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldLaneScale"),
                new GUIContent(
                    "Motion Field Lane Scale",
                    "Broadness of the generated intent/debug lane pattern. Lower values produce larger lanes; higher values produce finer lanes. Changing it regenerates the lane texture only; it does not activate lateral material transport."));

            if (runtime == null)
            {
                EditorGUILayout.LabelField("Runtime", "Unavailable");
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField(
                "Transport Mode",
                "Downstream phase + integer commit; lateral field debug/future only");
            EditorGUILayout.LabelField(
                "Motion Field",
                $"scroll {runtime.FoamMotionLaneScrollCells:0.00} cells / lane sig {runtime.FoamMotionLaneSignature} / obstacle sig {runtime.FoamObstacleRoutingSignature}");
            EditorGUILayout.LabelField(
                "Phase",
                $"{runtime.FoamPhaseCellFraction:0.00} cell / {runtime.FoamPhaseTransportMetres:0.000} m");
            EditorGUILayout.LabelField(
                "Committed Cells",
                $"{runtime.PhaseCommitCellsLastFrame} last frame / {runtime.PhaseCommitCellsLastSecond} last second");
            EditorGUILayout.LabelField(
                "Material Tick",
                $"{runtime.UpdateRate:0.#} Hz / {runtime.MaterialStepsLastFrame} steps last frame");
            EditorGUILayout.LabelField(
                "Status",
                ResolveFoamTransportSmoothnessStatus(runtime));

            DrawFoamTransportWarnings(runtime);
            EditorGUI.indentLevel--;
        }

        private void DrawFoamLifetimeSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            showFoamLifetimeDiagnostics = EditorGUILayout.Foldout(
                showFoamLifetimeDiagnostics,
                "Lifetime + Topology",
                true);
            if (!showFoamLifetimeDiagnostics)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(
                Find("foamNeutralLifetime"),
                new GUIContent("Neutral Lifetime (s)"));
            EditorGUILayout.PropertyField(
                Find("foamSupportedAgingRate"),
                new GUIContent("Supported Aging Rate"));
            EditorGUILayout.PropertyField(
                Find("foamNegativeAgingRate"),
                new GUIContent("Negative Aging Rate"));

            if (runtime == null)
            {
                EditorGUILayout.LabelField("Runtime", "Unavailable");
                EditorGUI.indentLevel--;
                return;
            }

            bool hasFreshMetrics = runtime.TopologyMetricsFresh;
            bool hasCompletedMetrics = runtime.TopologyMetricsAvailable;
            bool hasVisibleFoam =
                hasCompletedMetrics &&
                runtime.VisibleFoamPresenceArea > 0.0001f;
            string metricFreshnessPrefix = hasFreshMetrics ? string.Empty : "stale ";
            float hiddenArea = Mathf.Max(
                0f,
                runtime.IntegratedPresenceArea - runtime.VisiblePresenceCoreArea);
            float inspectorNeutralLifetime =
                Find("foamNeutralLifetime").floatValue;

            EditorGUILayout.LabelField(
                "Lifetime Authority",
                runtime.MaterialLifetimeAuthorityActive
                    ? "Remaining Life / full-field direct sim"
                    : runtime.LifetimeAuthorityStatus);
            EditorGUILayout.LabelField(
                "Visible Life",
                hasVisibleFoam
                    ? $"{metricFreshnessPrefix}avg {runtime.AverageVisibleRemainingLife:0.00}"
                    : hasCompletedMetrics
                        ? "completed sample found no visible foam"
                        : "no completed sample yet");
            EditorGUILayout.LabelField(
                "Local Aging",
                hasVisibleFoam
                    ? $"{metricFreshnessPrefix}{runtime.AverageLocalAgingRateUnderVisibleFoam:0.00}× avg"
                    : hasCompletedMetrics
                        ? "completed sample found no visible foam"
                        : "no completed sample yet");
            EditorGUILayout.LabelField(
                "Topology Under Foam",
                hasVisibleFoam
                    ? $"{metricFreshnessPrefix}support {runtime.AveragePositiveSupportUnderVisibleFoam:0.00} avg / negative {runtime.AverageNegativeAgingUnderVisibleFoam:0.00} avg"
                    : hasCompletedMetrics
                        ? "completed sample found no visible foam"
                        : "no completed sample yet");
            EditorGUILayout.LabelField(
                "Strongest Sample",
                hasVisibleFoam
                    ? $"{metricFreshnessPrefix}support {runtime.StrongestPositiveSupportUnderFoam:0.00} / negative {runtime.StrongestNegativeAgingUnderFoam:0.00}"
                    : hasCompletedMetrics
                        ? "completed sample found no visible foam"
                        : "no completed sample yet");
            EditorGUILayout.LabelField(
                "Foam Area",
                $"visible {runtime.VisiblePresenceCoreArea:0.000} m² / hidden {hiddenArea:0.000} m²");
            EditorGUILayout.LabelField(
                "Sample Freshness",
                runtime.TopologyMetricsAgeSeconds < 0f
                    ? "no completed sample yet"
                    : hasFreshMetrics
                        ? $"live {runtime.TopologyMetricsAgeSeconds:0.00}s old"
                        : $"stale {runtime.TopologyMetricsAgeSeconds:0.00}s old");
            EditorGUILayout.LabelField(
                "Material Clock",
                runtime.MaterialClockStatus);
            EditorGUILayout.LabelField(
                "Runtime Aging Params",
                $"inspector {inspectorNeutralLifetime:0.00}s / " +
                runtime.RuntimeAgingParameterStatus);
            EditorGUILayout.LabelField(
                "Probe Decay Check",
                runtime.ProbeDecayCheckStatus);
            EditorGUILayout.LabelField(
                "Life Range",
                runtime.VisibleLifeRangeStatus);
            EditorGUILayout.LabelField(
                "Birth Activity",
                runtime.BirthActivityStatus);

            EditorGUILayout.HelpBox(
                "Single-authority mode: chunk/reservation timers no longer clear material. Visible death should now come from per-cell Remaining Life only.",
                MessageType.None);

            EditorGUI.indentLevel--;
        }

        private void DrawFoamMaterialProbeSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            showFoamMaterialProbe = EditorGUILayout.Foldout(
                showFoamMaterialProbe,
                "Material Probe",
                true);
            if (!showFoamMaterialProbe)
            {
                return;
            }

            EditorGUI.indentLevel++;
            SerializedProperty foamDebugProperty = Find("foamDebugView");
            string[] foamDebugLabels =
            {
                "Final Foam",
                "Foam + Aging Topology",
                "Progressive Birth Source",
                "Material Presence",
                "Material Remaining Life",
                "Foam Motion Field",
                "Foam Motion Field + Cell Grid",
                "Foam Evaluated Shape",
                "Foam Shape Difference"
            };
            int[] foamDebugValues =
            {
                (int)StylizedRiverFoamDebugView.Final,
                (int)StylizedRiverFoamDebugView.FoamAndAgingTopology,
                (int)StylizedRiverFoamDebugView.ProgressiveBirthSource,
                (int)StylizedRiverFoamDebugView.MaterialPresence,
                (int)StylizedRiverFoamDebugView.MaterialRemainingLife,
                (int)StylizedRiverFoamDebugView.FoamMotionField,
                (int)StylizedRiverFoamDebugView.FoamMotionFieldCellGrid,
                (int)StylizedRiverFoamDebugView.FoamEvaluatedShape,
                (int)StylizedRiverFoamDebugView.FoamShapeDifference
            };
            int currentDebugIndex = System.Array.IndexOf(
                foamDebugValues,
                foamDebugProperty.intValue);
            if (currentDebugIndex < 0)
            {
                currentDebugIndex = 0;
            }

            EditorGUI.BeginChangeCheck();
            int selectedDebugIndex = EditorGUILayout.Popup(
                new GUIContent(
                    "Debug View",
                    "Raw material views show the persistent foam texture directly, not the beauty mask or topology overlay."),
                currentDebugIndex,
                foamDebugLabels);
            if (EditorGUI.EndChangeCheck())
            {
                foamDebugProperty.intValue = foamDebugValues[selectedDebugIndex];
            }

            EditorGUILayout.LabelField(
                "Probe State",
                runtime != null
                    ? runtime.IsolatedLifeProbeStatus
                    : "runtime unavailable");

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button(
                        new GUIContent(
                            "Clear + Emit Configured Life Probe",
                            "Clears persistent foam material, cancels active foam births, then writes three small non-overlapping raw material patches directly. Aging uses the current Neutral Lifetime and aging-rate compute parameters.")))
                {
                    ApplyFoamSpawnProperties();
                    river.ClearAndEmitFoamIsolatedLifeProbe(false);
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Clear + Emit Absolute 1s Probe",
                            "Final lifecycle sanity check: clears material, writes the same three raw patches, then temporarily ignores topology and Neutral Lifetime for this isolated probe so Remaining Life subtracts raw deltaTime directly.")))
                {
                    ApplyFoamSpawnProperties();
                    river.ClearAndEmitFoamAbsoluteLifeProbe();
                }
            }

            EditorGUILayout.HelpBox(
                "Configured probe uses the real production lifetime parameters. Absolute 1s probe is a debug-only sanity check that bypasses topology and lifetime scaling after the write; if absolute aging works but configured aging does not, the failure is in parameter/lifetime-scale plumbing rather than the texture ping-pong path.",
                MessageType.None);

            EditorGUI.indentLevel--;
        }

        private void DrawFoamManualBirthSourceSection(
            StylizedRiver river,
            StylizedRiverFoamRuntime runtime)
        {
            showFoamManualBirthSource = EditorGUILayout.Foldout(
                showFoamManualBirthSource,
                "Manual Birth Source",
                true);
            if (!showFoamManualBirthSource)
            {
                return;
            }

            EditorGUI.indentLevel++;

            EditorGUILayout.HelpBox(
                "Manual Birth Source contains every manual foam birth control. This section creates stable source material only; macro/meso breakup belongs to material evolution and micro breakup belongs to rendering.",
                MessageType.None);

            EditorGUILayout.LabelField("Source Position", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                Find("foamSpawnDistanceNormalized"),
                new GUIContent(
                    "Longitudinal Position",
                    "Normalized position from logical upstream start (0) to downstream end (1)."));
            EditorGUILayout.PropertyField(
                Find("foamSpawnAcrossNormalized"),
                new GUIContent(
                    "Across Position",
                    "Normalized lateral position. Minus one is the left surface edge, zero is centre, and one is the right surface edge."));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Source Material", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                Find("foamSpawnAmount"),
                new GUIContent(
                    "Amount",
                    "Source-only coverage amount. Higher values fill more of the same candidate source; this is not Remaining Life, opacity, density, or fracture severity."));
            EditorGUILayout.PropertyField(
                Find("foamSpawnRemainingLife"),
                new GUIContent(
                    "Initial Remaining Life",
                    "Normalized lifetime assigned to accepted source material. One starts with a complete lifetime; lower values begin closer to expiry."));
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Source Shape", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                Find("foamSpawnScale"),
                new GUIContent(
                    "Half Width",
                    "World-space half-width of the canonical moving manual source. Final breakup and morphing are handled after birth, not by this source control."));
            EditorGUI.indentLevel--;

            showFoamManualSourceMotion = EditorGUILayout.Foldout(
                showFoamManualSourceMotion,
                "Source Path Motion",
                true);
            if (showFoamManualSourceMotion)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    Find("foamSpawnRibbonDuration"),
                    new GUIContent(
                        "Duration",
                        "Duration of the budgeted moving-head source event."));
                EditorGUILayout.PropertyField(
                    Find("foamSpawnRibbonTravelDistance"),
                    new GUIContent(
                        "Travel Distance",
                        "Net downstream travel distance of the source head."));
                EditorGUILayout.PropertyField(
                    Find("foamSpawnRibbonAcrossDrift"),
                    new GUIContent(
                        "Across Drift",
                        "Total normalized lateral displacement from source start to source end."));
                EditorGUILayout.PropertyField(
                    Find("foamSpawnRibbonPathWander"),
                    new GUIContent(
                        "Path Bend",
                        "Strength of the deterministic smooth bend added to the source path. This is path curvature, not Foam breakup."));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(
                        new GUIContent(
                            "Start Manual Source",
                            "Starts one budgeted manual source event. The source is intentionally stable so later material behavior can be tested honestly.")))
                {
                    ApplyFoamSpawnProperties();
                    river.StartFoamSpawn();
                }

                if (GUILayout.Button(
                        new GUIContent(
                            "Clear Foam",
                            "Clears persistent Foam state, pending manual injections, and active foam composition events.")))
                {
                    ApplyFoamSpawnProperties();
                    river.ClearFoam();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to start a manual source. Automatic spawning remains intentionally disabled.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(2f);
            EditorGUILayout.LabelField("State", EditorStyles.boldLabel);
            if (runtime != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(
                    "Source State",
                    runtime.LatestFoamCompositionEventId > 0
                        ? $"event {runtime.LatestFoamCompositionEventId}, active {runtime.ActiveFoamCompositionEventCount}/{runtime.FoamCompositionPoolCapacity}, budget {runtime.FoamCompositionBirthBudgetPerStep}/step"
                        : $"No active source, budget {runtime.FoamCompositionBirthBudgetPerStep}/step");
                EditorGUILayout.LabelField(
                    "Last Segment",
                    $"{runtime.LastFoamCompositionSegmentLength:0.000} m");
                EditorGUILayout.LabelField(
                    "Source Texels",
                    runtime.ProgressiveBirthDebugReadbackAvailable
                        ? $"{runtime.ProgressiveBirthDebugLatestAffectedTexels:N0} latest"
                        : "No source readback");
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("Runtime", "Unavailable");
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFoamShapeResidueSection(
            StylizedRiverFoamRuntime runtime)
        {
            showFoamShapeResidueDiagnostics = EditorGUILayout.Foldout(
                showFoamShapeResidueDiagnostics,
                "Material Shape",
                true);
            if (!showFoamShapeResidueDiagnostics)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (runtime == null)
            {
                EditorGUILayout.LabelField("Runtime", "Unavailable");
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField(
                "Stored / Visible Area",
                $"{runtime.IntegratedPresenceArea:0.000} m² / {runtime.VisiblePresenceCoreArea:0.000} m²");
            EditorGUILayout.LabelField(
                "Manual Proof Ratio",
                runtime.ManualProofReferenceAvailable
                    ? runtime.ManualProofPresenceRatio.ToString("0.00")
                    : "No manual proof");
            EditorGUILayout.LabelField(
                "Perimeter Ratio",
                FormatPercent(runtime.PerimeterRatio));
            EditorGUILayout.LabelField(
                "Known Shape Issue",
                "Not just a simple line/ribbon now; morphing, breakup, lateral drift, and obstacle interaction remain unsolved");

            if (runtime.ManualProofReferenceAvailable &&
                (runtime.ManualProofPresenceRatio > 1.25f ||
                 runtime.ManualProofPresenceRatio < 0.65f))
            {
                EditorGUILayout.HelpBox(
                    "Manual proof area changed outside tolerance. Shape/transport state needs investigation.",
                    MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFoamRuntimeResourceSection(
            StylizedRiverFoamRuntime runtime)
        {
            showFoamRuntimeResourceDiagnostics = EditorGUILayout.Foldout(
                showFoamRuntimeResourceDiagnostics,
                "Runtime",
                true);
            if (!showFoamRuntimeResourceDiagnostics)
            {
                return;
            }

            EditorGUI.indentLevel++;

            if (runtime == null)
            {
                EditorGUILayout.LabelField("Runtime", "Unavailable");
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUILayout.LabelField(
                "State",
                runtime.enabled ? runtime.IsSleeping ? "Sleeping" : "Active" : "Component disabled");
            EditorGUILayout.LabelField(
                "Texture",
                runtime.ResourcesAllocated
                    ? $"{runtime.FieldWidth} × {runtime.FieldHeight}"
                    : "Not allocated");
            EditorGUILayout.LabelField(
                "Chunks",
                $"active {runtime.ActiveChunkCount} / reserved {runtime.ActiveReservationCount}");
            EditorGUILayout.LabelField(
                "Dispatch",
                $"{runtime.LastUpdateDispatches} last / {runtime.RecentPeakDispatches} peak");
            DrawMemoryDiagnostic(
                "Memory",
                runtime.EstimatedMemoryBytes,
                "Estimated Foam runtime memory.");

            if (GUILayout.Button(
                    new GUIContent(
                        "Reset Runtime Peaks",
                        "Resets recent dispatch and cell-iteration peaks to the current update.")))
            {
                runtime.ResetRecentPeaks();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawFoamAdvancedInternalSection(
            StylizedRiverFoamRuntime runtime)
        {
            showFoamAdvancedInternalDiagnostics = EditorGUILayout.Foldout(
                showFoamAdvancedInternalDiagnostics,
                "Advanced Internals",
                true);
            if (!showFoamAdvancedInternalDiagnostics)
            {
                return;
            }

            EditorGUI.indentLevel++;

            DrawMajorCandidatePreview();

            if (runtime != null)
            {
                showFoamCacheDiagnostics = EditorGUILayout.Foldout(
                    showFoamCacheDiagnostics,
                    "Topology Cache",
                    true);
                if (showFoamCacheDiagnostics)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField(
                        "Build",
                        $"{runtime.TopologyReplacementState} / ready {(runtime.TopologyReplacementReady ? "yes" : "no")}");
                    EditorGUILayout.LabelField(
                        "Transition",
                        $"{runtime.TopologyTransitionState} / {FormatPercent(runtime.TopologyTransitionProgress)}");
                    EditorGUILayout.LabelField(
                        "Startup",
                        runtime.TopologyStartupValidationComplete
                            ? $"complete — {runtime.TopologyStartupTotalMilliseconds:0.000} ms"
                            : runtime.TopologyCacheStartupState);
                    EditorGUILayout.LabelField(
                        "Cache Hits / Misses",
                        $"{runtime.TopologyCacheStartupHitCount} / {runtime.TopologyCacheStartupMissCount}");
                    EditorGUI.indentLevel--;
                }
            }

            EditorGUI.indentLevel--;
        }

        private static string ResolveFoamTransportSmoothnessStatus(
            StylizedRiverFoamRuntime runtime)
        {
            if (runtime == null)
            {
                return "Runtime unavailable";
            }
            if (!runtime.ResourcesAllocated)
            {
                return "Resources not allocated";
            }
            if (runtime.VisiblePresenceCoreArea <= 0.0001f &&
                runtime.IntegratedPresenceArea <= 0.0001f)
            {
                return "No Foam material";
            }
            if (runtime.MaterialStepsLastFrame > 1)
            {
                return "Simulation burst detected";
            }
            if (runtime.EstimatedTransportCellsPerStep > 1.25f)
            {
                return "Phase cells per material step high";
            }
            if (runtime.PhaseCommitCellsLastFrame > 0)
            {
                return "Integer phase commit this frame";
            }
            if (runtime.FoamPhaseCellFraction > 0.01f)
            {
                return "Phase transport active";
            }

            return "No high-level transport warning";
        }

        private static string ResolveFoamLikelyProblem(
            StylizedRiverFoamRuntime runtime)
        {
            if (runtime == null)
            {
                return "Open Runtime";
            }
            if (!runtime.ResourcesAllocated)
            {
                return "Open Runtime";
            }
            if (runtime.MaterialStepsLastFrame > 1 ||
                runtime.EstimatedTransportCellsPerStep > 1.25f)
            {
                return "Open Material Motion";
            }
            float hiddenArea = Mathf.Max(
                0f,
                runtime.IntegratedPresenceArea -
                runtime.VisiblePresenceCoreArea);
            if (hiddenArea > Mathf.Max(0.05f, runtime.VisiblePresenceCoreArea * 0.25f))
            {
                return "Open Lifetime + Topology or Material Shape";
            }
            if (runtime.ManualProofReferenceAvailable &&
                (runtime.ManualProofPresenceRatio < 0.65f ||
                 runtime.ManualProofPresenceRatio > 1.25f))
            {
                return "Open Material Shape";
            }
            return "No obvious diagnostic failure";
        }

        private static void DrawFoamTransportWarnings(
            StylizedRiverFoamRuntime runtime)
        {
            if (runtime == null)
            {
                return;
            }

            if (runtime.MaterialStepsLastFrame > 1)
            {
                EditorGUILayout.HelpBox(
                    "Material simulation steps are arriving in bursts. Foam may pause and jump.",
                    MessageType.Warning);
            }
            if (runtime.EstimatedTransportCellsPerStep > 1.25f)
            {
                EditorGUILayout.HelpBox(
                    "Estimated phase cells per material step is high. Base motion should still be committed by integer shifts, but very fast authored flow may move through several cells between material lifecycle ticks.",
                    MessageType.Warning);
            }
            if (runtime.IntegratedPresenceArea > 0.0001f &&
                runtime.FoamPhaseCellFraction <= 0.0001f &&
                runtime.PhaseCommitCellsLastFrame == 0)
            {
                EditorGUILayout.HelpBox(
                    "Live Foam exists but the residual phase is currently zero. This is healthy immediately after an integer commit, but Foam Phase / Cell should ramp upward during visible downstream movement.",
                    MessageType.Info);
            }
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

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField(
                "Status",
                majorCandidatePreview.Accepted
                    ? "Accepted"
                    : "Rejected after bounded retries");
            EditorGUILayout.LabelField(
                "Primary Rejection",
                majorCandidatePreview.Accepted
                    ? "None"
                    : ObjectNames.NicifyVariableName(
                        majorCandidatePreview.RejectionReason.ToString()));
            EditorGUILayout.LabelField(
                "Occupied Area",
                $"{majorCandidatePreview.OccupiedCellCount:N0} cells · " +
                $"{majorCandidatePreview.OccupiedAreaFraction * 100f:0.0}%");
            EditorGUILayout.LabelField(
                "Minimum Neck Width",
                $"{majorCandidatePreview.MinimumNeckWidthCells} cells");
            EditorGUILayout.LabelField(
                "Compactness",
                majorCandidatePreview.Compactness.ToString("0.000"));
            EditorGUI.indentLevel--;
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

        private void DrawWaterBody()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Water Body", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Water is split into explicit layers: the river domain defines coordinates, external influence fields describe support/contact/motion context, persistent Foam material moves downstream and ages, the Layer D shape product is a visual interpretation, and the shader performs final composition. Persistent Foam does not currently perform lateral disturbance transport; motion/support fields are inputs, diagnostics, and future routing/visual data. Detached spray, droplets, caustics, reflections, and final performance closure remain later gated work.",
                MessageType.Info);

            SerializedProperty surfaceState = Find("surfaceState");
            EditorGUILayout.PropertyField(
                surfaceState,
                new GUIContent(
                    "Surface State",
                    "Liquid and Frozen are authored endpoints. Custom exposes a continuous freeze value reserved for later systems; no visible freeze/thaw transition is simulated."));

            bool mixedState = surfaceState.hasMultipleDifferentValues;
            StylizedRiverSurfaceState resolvedState =
                (StylizedRiverSurfaceState)surfaceState.enumValueIndex;

            if (!mixedState &&
                resolvedState == StylizedRiverSurfaceState.Custom)
            {
                EditorGUILayout.PropertyField(
                    Find("customFreezeAmount"),
                    new GUIContent(
                        "Freeze Amount",
                        "Zero is fully liquid and one is fully frozen."));
            }

            bool showLiquid =
                mixedState ||
                resolvedState != StylizedRiverSurfaceState.Frozen;

            bool showFrozen =
                mixedState ||
                resolvedState != StylizedRiverSurfaceState.Liquid;

            if (showLiquid)
            {
                DrawLiquidBodyControls();
            }

            if (showFrozen)
            {
                DrawFrozenBodyControls();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Lighting Response",
                EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(
                Find("lightDependence"),
                new GUIContent(
                    "Light Dependence",
                    "Zero keeps authored colours largely fixed. One makes the water or ice body fully dependent on actual scene lighting."));
            EditorGUILayout.PropertyField(
                Find("ambientResponse"),
                new GUIContent(
                    "Ambient Response",
                    "Strength of environment and ambient illumination."));
            EditorGUILayout.PropertyField(
                Find("sunResponse"),
                new GUIContent(
                    "Sun Response",
                    "Strength of the main directional sun or moon light."));
            EditorGUILayout.PropertyField(
                Find("localLightResponse"),
                new GUIContent(
                    "Local Light Response",
                    "Strength of point, spot, and additional directional lights."));
            EditorGUILayout.PropertyField(
                Find("lightColorInfluence"),
                new GUIContent(
                    "Light Colour Influence",
                    "Zero uses light brightness only. One allows sunrise, sunset, spells, hearths, and other lights to fully tint the river."));
            EditorGUILayout.PropertyField(
                Find("minimumNightVisibility"),
                new GUIContent(
                    "Minimum Night Visibility",
                    "Minimum retained body illumination when meaningful light is absent. Zero allows the river to become virtually black."));
            EditorGUILayout.PropertyField(
                Find("shadowResponse"),
                new GUIContent(
                    "Shadow Response Master",
                    "Master strength for real-time shadowing of the river's intrinsic water or ice contribution."));
            EditorGUILayout.PropertyField(
                Find("liquidSurfaceShadowResponse"),
                new GUIContent(
                    "Liquid Surface Shadow",
                    "How strongly the main-light shadow affects intrinsic liquid tint and surface lighting. Keep this subtle so the refracted underwater shadow remains dominant."));
            EditorGUILayout.PropertyField(
                Find("iceSurfaceShadowResponse"),
                new GUIContent(
                    "Ice Surface Shadow",
                    "How strongly the main-light shadow affects the more solid frozen ice body and surface."));
        }

        private void DrawLiquidBodyControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Liquid Body",
                EditorStyles.boldLabel);

            SerializedProperty preset = Find("bodyPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Liquid Preset",
                    "Applies only the liquid optical settings. It never changes motion, foam, refraction, reflection, or frozen-body controls."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply Liquid Body Preset");
                    river.ApplyWaterBodyPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("shallowColor"),
                new GUIContent("Shallow Colour"));
            EditorGUILayout.PropertyField(
                Find("deepColor"),
                new GUIContent("Deep Colour"));
            EditorGUILayout.PropertyField(
                Find("clarity"),
                new GUIContent(
                    "Clarity",
                    "How strongly the riverbed remains visible through liquid water."));
            EditorGUILayout.PropertyField(
                Find("bodyDepthRange"),
                new GUIContent(
                    "Depth Range",
                    "World-space vertical depth at which liquid water reaches its deep appearance."));
            EditorGUILayout.PropertyField(
                Find("bodyDepthContrast"),
                new GUIContent(
                    "Depth Contrast",
                    "Low values produce a gradual transition. High values separate shallow and deep water more strongly."));
            EditorGUILayout.PropertyField(
                Find("waterTintStrength"),
                new GUIContent(
                    "Water Tint Strength",
                    "How strongly liquid water colours the scene beneath it."));
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

        private void DrawFrozenBodyControls()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Frozen Body",
                EditorStyles.boldLabel);

            SerializedProperty preset = Find("iceBodyPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Ice Preset",
                    "Applies only the frozen optical settings. Motion systems will consume the shared freeze state in later stages."));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver river)
                    {
                        continue;
                    }

                    Undo.RecordObject(river, "Apply Ice Body Preset");
                    river.ApplyIceBodyPreset();
                    EditorUtility.SetDirty(river);
                }

                serializedObject.Update();
                RepaintScene();
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("iceColor"),
                new GUIContent("Ice Colour"));
            EditorGUILayout.PropertyField(
                Find("iceTransmission"),
                new GUIContent(
                    "Ice Transmission",
                    "How much of the lit scene beneath the ice remains visible."));
            EditorGUILayout.PropertyField(
                Find("iceThickness"),
                new GUIContent(
                    "Ice Thickness",
                    "Optical thickness of the frozen sheet. Higher values make the ice more opaque."));
            EditorGUILayout.PropertyField(
                Find("iceCloudiness"),
                new GUIContent(
                    "Ice Cloudiness",
                    "How cloudy and internally scattered the ice appears."));
            EditorGUILayout.PropertyField(
                Find("iceSurfacePresence"),
                new GUIContent(
                    "Ice Surface Presence",
                    "How strongly the frozen air-ice boundary remains visible."));
            EditorGUILayout.PropertyField(
                Find("iceScattering"),
                new GUIContent(
                    "Ice Scattering",
                    "How strongly cloudy ice broadens and brightens its light response."));

            if (EditorGUI.EndChangeCheck())
            {
                Find("iceBodyPreset").enumValueIndex =
                    (int)StylizedRiverIceBodyPreset.Custom;
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
                    "Displays the body inputs independently: depth, transmission, coverage, scene colour, lighting contributions, or freeze state."));
            EditorGUILayout.PropertyField(
                Find("diffuseWrap"),
                new GUIContent(
                    "Diffuse Wrap",
                    "Advanced low-angle lighting control. Higher values keep sunrise and sunset response broader instead of collapsing abruptly."));
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

            bool hasReflection =
                river.GetComponent<StylizedRiverPlanarReflection>() != null;

            if (!hasReflection)
            {
                return;
            }

            EditorGUILayout.Space(8f);

            if (hasReflection)
            {
                EditorGUILayout.HelpBox(
                    "The planar-reflection component remains a deferred Stage 8 system and is still ignored by the accepted production shader path.",
                    MessageType.Warning);
            }
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
                "Corridor Rings",
                river.CorridorRingCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "Corridor Triangles",
                river.CorridorTriangleCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "Corridor Collider Triangles",
                river.CorridorColliderTriangleCount.ToString("N0"));
            EditorGUILayout.LabelField(
                "Ground Height Source",
                river.CorridorUsesGroundHeightField
                    ? "Generated base terrain"
                    : "Fallback");
            if (river.CorridorHasTightBendWarning)
            {
                EditorGUILayout.HelpBox(
                    "The river is very wide relative to at least one bend radius. Inspect the inner bank for pinching.",
                    MessageType.Warning);
            }
            EditorGUILayout.LabelField(
                "GameObject Layer",
                LayerMask.LayerToName(river.gameObject.layer));
        }

        private static string GetFoamDebugViewDescription(
            StylizedRiverFoamDebugView view)
        {
            switch (view)
            {
                case StylizedRiverFoamDebugView.Final:
                    return
                        "The exact normal player-facing Foam result after transport, topology-adjusted Remaining Life, valid-fluid clipping, temporal interpolation, lighting, and the final transported Presence coverage. No diagnostic substitution is active.";

                case StylizedRiverFoamDebugView.FoamAndAgingTopology:
                    return
                        "One combined lifecycle-validation view. Dark water is neutral valid fluid. Green is the maximum positive lifespan support from Major, Connector, Pressure, Lee, and Shore Support. Red is Negative Aging Pressure. Yellow is their overlap. Blue is the canonical current-water Obstacle Footprint. Bright cyan/white is the exact final Foam mask used by normal rendering. Remaining Life is verified through the Material Lifetime and Topology Interaction summaries, not by broad Foam opacity.";

                case StylizedRiverFoamDebugView.ProgressiveBirthSource:
                    return
                        "Source isolation before persistent transport and aging. Blue is the complete planned accepted source, green is cumulative accepted source geometry since the latest idle start, red is source submitted during the latest material update, and yellow is the current emission head. Amount selects deterministic coherent area rather than persistent intensity.";

                case StylizedRiverFoamDebugView.MaterialPresence:
                    return
                        "Raw persistent material Presence sampled through the same storage coordinate path used by normal Foam rendering. White means stored Foam coverage exists before beauty-mask breakup.";

                case StylizedRiverFoamDebugView.MaterialRemainingLife:
                    return
                        "Raw normalized Remaining Life from the persistent material texture. This ignores beauty colour and shows whether cells actually age independently.";

                case StylizedRiverFoamDebugView.FoamMotionField:
                    return
                        "Dense Foam Motion Field intent/debug texture. Blue/cyan means leftward routing intent, red/orange means rightward routing intent, black means intentional neutral/calm field, green/yellow marks obstacle override influence, and semi-transparent white overlays raw stored Foam Presence rather than the final render mask. This view does not mean lateral material transport is currently active.";

                case StylizedRiverFoamDebugView.FoamMotionFieldCellGrid:
                    return
                        "Dense Foam Motion Field intent/debug texture plus the persistent Foam simulation cell grid. The white overlay is raw stored Foam Presence, not the final render mask. Fine dark lines show individual Foam cells; brighter pale lines show eight-cell blocks. This view does not mean lateral material transport is currently active.";

                case StylizedRiverFoamDebugView.FoamEvaluatedShape:
                    return
                        "Layer D evaluated Foam Shape product sampled from _FoamShapeMask. After 4.11C.5.10B this is intentionally reset to clipped raw Persistent Presence as a clean baseline. It is not Final Foam and not a separate material truth.";

                case StylizedRiverFoamDebugView.FoamShapeDifference:
                    return
                        "Layer D difference diagnostic. Black means _FoamShapeMask matches raw persistent Material Presence, green means evaluated shape adds visual coverage, and magenta/red means evaluated shape removes visual coverage. This exists so Stage D changes are visible without guessing between two similar masks.";

                default:
                    return
                        "The exact normal player-facing Foam result. No diagnostic substitution is active.";
            }
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
