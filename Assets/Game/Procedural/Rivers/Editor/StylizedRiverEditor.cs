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
                    "Geometry Quality",
                    "Controls water cross-channel tessellation plus corridor cross-section detail and smooth longitudinal refinement. The Stage 1 domain remains the authoritative coordinate source."));
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
            SerializedProperty impactRipplePropagationProperty =
                Find("impactRipplePropagation");
            SerializedProperty impactRippleDecayProperty =
                Find("impactRippleDecay");
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
                        "Master allocation and simulation switch. Disabled rivers reproduce Stage 4 and allocate no disturbance fields."));
            }

            if (presetProperty != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(
                    presetProperty,
                    new GUIContent(
                        "Disturbance Character",
                        "Applies coordinated defaults to Pressure, Wake, and Impact Ripple response controls."));

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
                            "Normalized 0–1 position inside the computed flow-and-geometry-safe pressure range."));
                }
                if (staticPressureContactSharpnessProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureContactSharpnessProperty,
                        new GUIContent(
                            "Contact Sharpness",
                            "Controls how quickly the pressure ridge descends from the object toward open water."));
                }
                if (staticPressureWaveResponseProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureWaveResponseProperty,
                        new GUIContent(
                            "Profile Variation",
                            "Controls how strongly the supported ridge height is redistributed laterally. Zero keeps the cached geometry-derived profile stable."));
                }
                if (staticPressureProfileChangeIntervalMinProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureProfileChangeIntervalMinProperty,
                        new GUIContent(
                            "Minimum Change Interval",
                            "Shortest randomized time in seconds between lateral pressure-profile changes."));
                }
                if (staticPressureProfileChangeIntervalMaxProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        staticPressureProfileChangeIntervalMaxProperty,
                        new GUIContent(
                            "Maximum Change Interval",
                            "Longest randomized time in seconds between lateral pressure-profile changes. Morph duration scales automatically and completes before the next change."));
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
                            "Shared wake-energy response. Stationary geometry and dynamic emitters prepare different sources but use the same river-level Strength."));
                }
                if (obstructionWakeReachProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeReachProperty,
                        new GUIContent(
                            "Reach",
                            "Shared downstream persistence and active range for stationary and dynamic wake sources."));
                }
                if (obstructionWakeSpreadProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSpreadProperty,
                        new GUIContent(
                            "Spread",
                            "Shared source-width response. For stationary geometry this shapes the lee and rear releases; dynamic emitters apply it to their swept wake footprint."));
                }
                if (obstructionWakeVariationProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationProperty,
                        new GUIContent(
                            "Variation",
                            "Shared wake-variation envelope. Stationary sources use spatial lee/release profiles; dynamic sources derive most variation from movement and will consume the same envelope when their full source model is implemented."));
                }
                if (obstructionWakeVariationIntervalMinProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationIntervalMinProperty,
                        new GUIContent(
                            "Minimum Variation Interval",
                            "Shortest randomized time in seconds between stationary wake-source targets."));
                }
                if (obstructionWakeVariationIntervalMaxProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeVariationIntervalMaxProperty,
                        new GUIContent(
                            "Maximum Variation Interval",
                            "Longest randomized time in seconds between stationary wake-source targets. Smooth transitions complete before the next target."));
                }
                if (obstructionWakeWideningProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeWideningProperty,
                        new GUIContent(
                            "Widening",
                            "How quickly the shared transported wake field spreads laterally downstream for both stationary and dynamic sources."));
                }
                if (obstructionWakeSurfaceHeightProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSurfaceHeightProperty,
                        new GUIContent(
                            "Wake Surface Height",
                            "Maximum positive surface height produced by the compact core of the shared transported wake field. Static and dynamic wake energy use the same bounded response; the attached lee remains a separate negative envelope."));
                }
                if (obstructionWakeSurfaceCompactnessProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        obstructionWakeSurfaceCompactnessProperty,
                        new GUIContent(
                            "Wake Surface Compactness",
                            "Controls how much of the broad transported energy field becomes visible geometry. Lower values produce a broader and stronger surface response; higher values restrict height to the strongest wake core without changing transport, normals, turbulence, or future foam data."));
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
                            "Initial energy injected by impact events."));
                }
                if (impactRipplePropagationProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRipplePropagationProperty,
                        new GUIContent(
                            "Propagation",
                            "Approximate speed at which impact ripple energy spreads."));
                }
                if (impactRippleDecayProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleDecayProperty,
                        new GUIContent(
                            "Decay",
                            "Rate at which impact ripple energy dissipates."));
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
                            "Displays Stage 5 disturbance fields and composed geometry without changing simulation settings."));

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
                            "Normalized zero-to-one position along the river used by manual impact tests."));
                }
                if (impactRippleTestAcrossProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleTestAcrossProperty,
                        new GUIContent(
                            "Across Position",
                            "Normalized position across the local water surface. Negative is left and positive is right."));
                }
                if (impactRippleTestEventProperty != null)
                {
                    EditorGUILayout.PropertyField(
                        impactRippleTestEventProperty,
                        new GUIContent("Event"),
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
                    GUILayout.Button("Create Disturbance Runtime"))
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
                "Impacts Injected Last Step",
                runtime.ImpactsInjectedLastStep.ToString());
            EditorGUILayout.LabelField(
                "Ripple Internal Substeps",
                runtime.CurrentRippleSubstepCount.ToString());
            EditorGUILayout.LabelField(
                "Maximum Recent Ripple Substeps",
                runtime.MaximumRecentRippleSubstepCount.ToString());
            EditorGUILayout.LabelField(
                "Estimated Field Memory",
                $"{runtime.EstimatedMemoryBytes / (1024f * 1024f):0.00} MB");
            EditorGUILayout.LabelField(
                "State",
                runtime.IsSleeping ? "Sleeping" : "Active");

            if (GUILayout.Button("Clear Field"))
            {
                runtime.ClearField();
            }

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Emit Test Impact"))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugImpact(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }

                if (GUILayout.Button("Emit Opposite Sign"))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOppositeSignImpact(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Emit Overlapping Pair"))
                {
                    ApplyImpactTestProperties();
                    runtime.EmitDebugOverlappingPair(
                        singleRiver.ImpactRippleTestDistanceNormalized,
                        singleRiver.ImpactRippleTestAcrossNormalized,
                        singleRiver.ImpactRippleTestEvent);
                }

                if (GUILayout.Button("Emit Near Shore"))
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
                StylizedRiverDisturbanceDebugView.FinalWakeGeometryHeight =>
                    "Final Wake Geometry Height: mid-gray is zero, darker values are the attached lee depression, and brighter values are positive transported trail height. The fixed encoding spans -0.40 m to +0.40 m.",
                _ => string.Empty
            };

            if (!string.IsNullOrEmpty(description))
            {
                EditorGUILayout.HelpBox(description, MessageType.None);
            }
        }

        private void DrawWaterBody()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Water Body", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Stage 2 provides the accepted body, Stage 3 supplies coherent motion, Stage 4 distorts the transmitted scene, and Stage 5 now adds persistent shared-field disturbances. Foam, secondary effects, caustics, and reflections remain deferred.",
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
