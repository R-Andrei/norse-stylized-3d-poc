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
        private bool showFoamTestTools;
        private bool showFoamDiagnostics;

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
                    "Controls water geometry and the Stage 6 structural grid. Foam/material, topology, guidance, and obstacle exclusion use 64 cells across at Low, 96 at Medium (standard), and 128 at High. The Stage 1 domain remains the authoritative coordinate source."));
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
                            "Base exponential loss per second. Effective Decay = Decay + abs(Flow Speed) × Flow Dissipation. Higher values shorten visible lifetime and chunk reservations even in still water."));
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
                "Stage 6.2 retains read-only Pressure, stationary Lee, and dynamic Shore as independent positive classes, while Pocket Exclusion and water-level-aware Obstacle Exclusion remain independent negative classes. Impact remains deliberately inactive.",
                MessageType.Info);

            EditorGUILayout.PropertyField(
                Find("foamEnabled"),
                new GUIContent(
                    "Foam Enabled",
                    "Master switch for the shared persistent Foam field. Disabled Foam allocates no simulation textures and contributes nothing to the water shader."));

            SerializedProperty preset = Find("foamPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Foam Preset",
                    "Applies one coherent stylized Foam personality using the six canonical controls. Custom preserves the current values."));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                foreach (Object selectedTarget in targets)
                {
                    if (selectedTarget is not StylizedRiver selectedRiver)
                    {
                        continue;
                    }

                    Undo.RecordObject(selectedRiver, "Apply Foam Preset");
                    selectedRiver.ApplyFoamPreset(
                        (StylizedRiverFoamPreset)preset.enumValueIndex);
                    EditorUtility.SetDirty(selectedRiver);
                }

                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(
                Find("foamAmount"),
                new GUIContent(
                    "Amount",
                    "Total supported material population and bounded supply. Zero stops new material while existing material decays normally."));
            EditorGUILayout.PropertyField(
                Find("foamWebGranularity"),
                new GUIContent(
                    "Web Granularity",
                    "Controls the metric scale hierarchy of rafts, pockets, branches, and connectors. Lower values favour fewer broad structures; higher values support finer subdivision."));
            EditorGUILayout.PropertyField(
                Find("foamNetworkEvolution"),
                new GUIContent(
                    "Network Evolution",
                    "Controls slow asynchronous evolution of free-water rafts, protected pockets, and connector candidates. Geometry-anchored structures remain stable."));
            EditorGUILayout.PropertyField(
                Find("foamBreakupFrequency"),
                new GUIContent(
                    "Breakup Frequency",
                    "Controls opportunities for hole opening, neck failure, peeling, and fragment shedding. It is not a global lifetime control."));
            EditorGUILayout.PropertyField(
                Find("foamSpeed"),
                new GUIContent(
                    "Foam Speed",
                    "Controls downstream material transport relative to the river flow. The solver still forbids coherent upstream travel."));
            EditorGUILayout.PropertyField(
                Find("foamColour"),
                new GUIContent(
                    "Foam Colour",
                    "Lit, non-emissive Foam tint. The colour alpha controls maximum Foam opacity."));

            if (targets.Length != 1 || target is not StylizedRiver river)
            {
                return;
            }

            EditorGUILayout.Space(4f);
            showFoamTestTools = EditorGUILayout.Foldout(
                showFoamTestTools,
                "Advanced Manual Foam Test Tools",
                true);
            if (showFoamTestTools)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    Find("foamTestDistanceNormalized"),
                    new GUIContent(
                        "Longitudinal Position",
                        "Normalized position from the logical upstream start (0) to downstream end (1)."));
                EditorGUILayout.PropertyField(
                    Find("foamTestAcrossNormalized"),
                    new GUIContent(
                        "Across Position",
                        "Normalized lateral position. Minus one is the left surface edge, zero is centre, and one is the right surface edge."));
                EditorGUILayout.PropertyField(
                    Find("foamTestRadius"),
                    new GUIContent(
                        "Radius",
                        "Initial across-river radius in world metres."));
                EditorGUILayout.PropertyField(
                    Find("foamTestAmount"),
                    new GUIContent(
                        "Amount",
                        "Persistent Foam coverage injected into the shared field."));
                EditorGUILayout.PropertyField(
                    Find("foamTestFreshness"),
                    new GUIContent(
                        "Freshness",
                        "Initial source youth. Freshness decays much faster than Amount, while injected Integrity begins high automatically."));
                EditorGUILayout.PropertyField(
                    Find("foamTestElongation"),
                    new GUIContent(
                        "Along-Flow Elongation",
                        "Multiplies the patch radius along the river. One is approximately circular; larger values create ribbons."));

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Foam Patch",
                                "Injects the configured manual patch into the persistent Foam field.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamTestPatch();
                    }

                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Adjacent Pair",
                                "Injects two nearby patches to test cohesion and merging.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamAdjacentPair();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Thin Ribbon",
                                "Injects a narrow elongated patch to test transport, Freshness loss, Integrity weakening, and crisp state extraction.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamThinRibbon();
                    }

                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Tongue Cluster",
                                "Injects three overlapping elongated sources to test phase carriage, overlap, structural support, and temporal interpolation.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamTongueCluster();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Fragment Chain",
                                "Injects a staggered chain of small sources to compare independent phase, Amount lifetime, Freshness lifetime, and Integrity evolution.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamFragmentChain();
                    }

                    if (GUILayout.Button(
                            new GUIContent(
                                "Emit Near Shore",
                                "Injects the configured patch close to a shore to test animated-edge attraction, retention, release, and exclusion.")))
                    {
                        ApplyFoamTestProperties();
                        river.EmitFoamNearShore();
                    }
                    EditorGUILayout.EndHorizontal();

                    if (GUILayout.Button(
                            new GUIContent(
                                "Clear Foam",
                                "Clears the four-channel Foam state and pending diagnostics. When Amount is above zero, the measured autonomous population begins rebuilding on subsequent simulation steps.")))
                    {
                        ApplyFoamTestProperties();
                        river.ClearFoam();
                    }
                }

                if (!Application.isPlaying)
                {
                    EditorGUILayout.HelpBox(
                        "Enter Play Mode to observe the autonomous network. Manual shapes may be injected to test how explicit material merges into, breaks within, and is reorganised by the same complete solver.",
                        MessageType.Info);
                }

                EditorGUI.indentLevel--;
            }

            StylizedRiverFoamRuntime runtime =
                river.GetComponent<StylizedRiverFoamRuntime>();

            showFoamDiagnostics = EditorGUILayout.Foldout(
                showFoamDiagnostics,
                "Foam Runtime Diagnostics",
                true);
            if (!showFoamDiagnostics)
            {
                return;
            }

            SerializedProperty foamDebugProperty = Find("foamDebugView");
            string[] foamDebugLabels =
            {
                "Final Foam (Debug Off)",
                "Capture Zones",
                "Positive Zone Classes",
                "Negative Zone Classes",
                "Positive and Negative Zones"
            };
            int[] foamDebugValues =
            {
                (int)StylizedRiverFoamDebugView.Final,
                (int)StylizedRiverFoamDebugView.CaptureZones,
                (int)StylizedRiverFoamDebugView.PositiveZoneClasses,
                (int)StylizedRiverFoamDebugView.NegativeZoneClasses,
                (int)StylizedRiverFoamDebugView.PositiveNegativeZones
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
                    "Final Foam disables Foam diagnostics. Only the four currently authoritative topology diagnostics remain available."),
                currentDebugIndex,
                foamDebugLabels);
            if (EditorGUI.EndChangeCheck())
            {
                foamDebugProperty.intValue =
                    foamDebugValues[selectedDebugIndex];
            }

            StylizedRiverFoamDebugView selectedFoamDebug =
                (StylizedRiverFoamDebugView)foamDebugProperty.intValue;
            EditorGUILayout.HelpBox(
                GetFoamDebugViewDescription(selectedFoamDebug),
                MessageType.None);

            if (runtime == null)
            {
                EditorGUILayout.LabelField(
                    new GUIContent("Runtime"),
                    new GUIContent(
                        river.FoamEnabled
                            ? "Created on Play/validation"
                            : "Disabled"));
                return;
            }

            EditorGUILayout.LabelField(
                new GUIContent(
                    "Stage 6 Mode",
                    "Batch 1C replaces the continuous river-length chain with finite local rafts and ribbons. Enclosed pockets are ring-validated, connectors require separate structure endpoints, branches form deliberate forks, and conservative water-level solid cross-sections remain separate negative topology instead of deforming positive classes. Capacity still does not feed material simulation."),
                new GUIContent("Batch 1C finite structures"));
            EditorGUILayout.LabelField(
                new GUIContent("Field Resolution"),
                new GUIContent(
                    runtime.ResourcesAllocated
                        ? $"{runtime.FieldWidth} × {runtime.FieldHeight}"
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Guidance Resolution",
                    "Shared Stage 6 structural grid used to organise persistent material toward branches and junctions. It now matches the material/topology resolution instead of using a coarser hidden lattice, and it is never rendered directly."),
                new GUIContent(
                    runtime.ResourcesAllocated
                        ? $"{runtime.GuidanceWidth} × {runtime.GuidanceHeight}"
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Topology Resolution",
                    "Primary RGBAHalf topology field at the same structural resolution as persistent material: red is Major Capacity, green Connector/Branch Capacity, blue Pocket Exclusion, and alpha is the structural-grid copy of water-level-aware Obstacle Exclusion. The authoritative debug mask is a dedicated point-sampled texture at that same resolution, reconstructed from one-time exact transformed-mesh solid intervals at the current Stage 3 water height. The companion source-class texture stores canonical Pressure, Lee, and Shore capture separately; alpha is reserved zero. Every positive class is computed independently, while negative classes remain separate until final composition."),
                new GUIContent(
                    runtime.ResourcesAllocated
                        ? $"{runtime.TopologyWidth} × {runtime.TopologyHeight}"
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Dynamic Shore Rows",
                    "One shared Stage 3 shoreline record per topology column. Each record stores the current left and right visible water edges after macro-wave displacement and hidden-bank-cover intersection. Shore Capture is a 0.24 m solid band plus a 0.03 m inward fade from those moving edges."),
                new GUIContent(
                    runtime.ResourcesAllocated
                        ? runtime.DynamicShoreRowCount.ToString()
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Topology Metrics",
                    "Low-rate asynchronous GPU reduction over the valid river domain. Metrics do not stall the simulation and never include padded storage."),
                new GUIContent(
                    runtime.TopologyMetricsAvailable
                        ? "Available"
                        : "Waiting for GPU readback"));
            if (runtime.TopologyMetricsAvailable)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(
                    "Major Capacity Coverage",
                    FormatPercent(runtime.MajorCapacityCoverage));
                EditorGUILayout.LabelField(
                    "Composed Topology Coverage",
                    FormatPercent(runtime.ComposedTopologyCoverage));
                EditorGUILayout.LabelField(
                    "Open Span Coverage",
                    FormatPercent(runtime.OpenSpanCoverage));
                EditorGUILayout.LabelField(
                    "Connector Capacity Coverage",
                    FormatPercent(runtime.ConnectorCapacityCoverage));
                EditorGUILayout.LabelField(
                    "Connector in Major Overlap",
                    FormatPercent(runtime.ConnectorInMajorOverlap));
                EditorGUILayout.LabelField(
                    "Pocket Interior Coverage",
                    FormatPercent(runtime.ProtectedPocketCoverage));
                EditorGUILayout.LabelField(
                    "Pocket Violation",
                    FormatPercent(runtime.ProtectedPocketViolation));
                EditorGUILayout.LabelField(
                    "Visible Material Coverage",
                    FormatPercent(runtime.VisibleMaterialCoverage));
                EditorGUILayout.LabelField(
                    "Shore Occupancy",
                    FormatPercent(runtime.ShoreOccupancy));
                EditorGUILayout.LabelField(
                    "Pressure / Lee Occupancy",
                    FormatPercent(runtime.PressureLeeOccupancy));
                EditorGUILayout.LabelField(
                    "Perimeter Ratio",
                    FormatPercent(runtime.PerimeterRatio));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.LabelField(
                new GUIContent(
                    "Fracture Resolution",
                    "Half-resolution persistent structural-damage field. Red stores accumulated damage; green stores crack coherence. It advances at a lower rate than material transport and never directly renders or deletes Foam."),
                new GUIContent(
                    runtime.ResourcesAllocated
                        ? $"{runtime.FractureWidth} × {runtime.FractureHeight}"
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent("Subsystem Rates"),
                new GUIContent(
                    $"Topology/Guidance {runtime.GuidanceUpdateRate:0} Hz · Population {runtime.PopulationUpdateRate:0} Hz · Fracture {runtime.FractureUpdateRate:0} Hz"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Transport",
                    "All quality tiers use forward/reverse bounded correction so thin strands, cracks, and tiny fragments survive transport instead of diffusing into broad sheets; quality scales resolution and cadence rather than reverting to the rejected transport. The longitudinal velocity is clamped to the authoritative downstream direction."),
                new GUIContent(
                    runtime.CorrectedAdvectionActive
                        ? "Corrected; downstream-only"
                        : "Not allocated"));
            EditorGUILayout.LabelField(
                new GUIContent("Update Rate"),
                new GUIContent($"{runtime.UpdateRate:0} Hz"));
            EditorGUILayout.LabelField(
                new GUIContent("Active Chunks"),
                new GUIContent(runtime.ActiveChunkCount.ToString()));
            EditorGUILayout.LabelField(
                new GUIContent("Pending Injections"),
                new GUIContent(runtime.PendingInjectionCount.ToString()));
            EditorGUILayout.LabelField(
                new GUIContent("Active Reservations"),
                new GUIContent(runtime.ActiveReservationCount.ToString()));
            EditorGUILayout.LabelField(
                new GUIContent("Injected Last Update"),
                new GUIContent(runtime.InjectedLastUpdate.ToString()));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Last Injection Boundary Coverage",
                    "Fluid coverage sampled at the centre of the most recent manual injection. Values near one mean open water; values near zero mean the requested centre was inside the shore/solid exclusion mask."),
                new GUIContent(
                    runtime.LastInjectionBoundaryCoverage >= 0f
                        ? runtime.LastInjectionBoundaryCoverage.ToString("0.000")
                        : "No injection yet"));
            EditorGUILayout.LabelField(
                new GUIContent(
                    "Injection Temporal State",
                    "Fresh manual injections are written into both Foam ping-pong textures so interpolation cannot hide them behind an empty previous state."),
                new GUIContent(
                    runtime.LastInjectionStateSynchronized
                        ? "Synchronized"
                        : "No injection yet"));
            EditorGUILayout.LabelField(
                new GUIContent("State"),
                new GUIContent(
                    runtime.IsSleeping
                        ? "Sleeping"
                        : "Active"));
            EditorGUILayout.LabelField(
                new GUIContent("Compute Dispatches"),
                new GUIContent(
                    $"{runtime.LastUpdateDispatches} last / {runtime.RecentPeakDispatches} recent peak"));
            EditorGUILayout.LabelField(
                new GUIContent("Estimated Cell-Iterations"),
                new GUIContent(
                    $"{runtime.LastUpdateCellIterations:N0} last / {runtime.RecentPeakCellIterations:N0} recent peak"));
            DrawMemoryDiagnostic(
                "Allocated Foam Memory",
                runtime.EstimatedMemoryBytes,
                "Estimated material state, corrected-advection scratch textures, guidance, Batch 1A topology, Batch 1B source-class topology, fracture, boundary, population/topology metrics, and local river metric buffer.");

            if (GUILayout.Button(
                    new GUIContent(
                        "Reset Foam Peaks",
                        "Resets the five-second recent dispatch and cell-iteration peaks to the current update.")))
            {
                runtime.ResetRecentPeaks();
            }
        }

        private void ApplyFoamTestProperties()
        {
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void DrawWaterBody()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Water Body", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 2 provides the accepted body, Stage 3 coherent motion, Stage 4 optical distortion, Stage 5 Pressure/Wake/Ripples, and Stage 6 now consumes all of those contracts in one persistent web-capable Foam network. Detached spray, droplets, caustics, reflections, and final performance closure remain later gated work.",
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

            bool hasLegacyFoam =
                river.GetComponent<StylizedRiverFoamSimulation>() != null;
            bool hasReflection =
                river.GetComponent<StylizedRiverPlanarReflection>() != null;

            if (!hasLegacyFoam && !hasReflection)
            {
                return;
            }

            EditorGUILayout.Space(8f);

            if (hasLegacyFoam)
            {
                EditorGUILayout.HelpBox(
                    "A legacy StylizedRiverFoamSimulation migration stub remains attached. Remove that component; Stage 6 Foam is now owned by the hidden StylizedRiverFoamRuntime and the controls above.",
                    MessageType.Warning);
            }

            if (hasReflection)
            {
                EditorGUILayout.HelpBox(
                    "The planar-reflection component remains a deferred Stage 8 system and is still ignored by the accepted production shader path.",
                    MessageType.Warning);
            }
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
                        "Normal rendered Foam result. No Foam diagnostic colour encoding is active.";

                case StylizedRiverFoamDebugView.CaptureZones:
                    return
                        "Canonical independent positive Capture Zones. Red = Pressure. Green = attached Lee. Blue = Shore. Shore is a fixed metric band measured inward from the instantaneous Stage 3 visible water edge: 0.24 m solid capture plus a 0.03 m fade. The edge is resolved from the same macro-wave and shore attenuation functions used by the water shader, not from the static shoreline allowance. No capture class or free-water class modifies another. Overlaps mix directly. These same three values are collapsed into the blue Capture class in Positive Zone Classes.";

                case StylizedRiverFoamDebugView.PositiveZoneClasses:
                    return
                        "Red = independent Major Capacity. Green = independent Connector Capacity. Blue = the maximum of the exact canonical Pressure, Lee, and Shore values shown separately in Capture Zones. No positive class is weighted against another. Overlaps mix. Black = no positive support. Negative topology is intentionally omitted.";

                case StylizedRiverFoamDebugView.NegativeZoneClasses:
                    return
                        "Independent negative topology classes. Red = Pocket Exclusion. Blue = Obstacle Exclusion from the conservative current-water-level solid cross-section. Magenta = overlap. The mask is point-sampled; each retained texel must pass a conservative 3×3 set of exact-mesh solid-interval tests across its displayed rectangle, and it is never expanded into a surrounding halo.";

                case StylizedRiverFoamDebugView.PositiveNegativeZones:
                    return
                        "Green = the unweighted maximum of Major Capacity, Connector Capacity, Pressure, Lee, and Shore before exclusion. Red = the maximum of Pocket Exclusion and Obstacle Exclusion. Yellow = overlap where combined negative topology removes positive support. Black = neither.";

                default:
                    return "Normal rendered Foam result. No Foam diagnostic colour encoding is active.";
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
