using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private void DrawWaterBodyAndLighting()
        {
            DrawWaterBody();
        }

        private void DrawSetup()
        {
            EditorGUILayout.PropertyField(Find("splineContainer"));
            EditorGUILayout.PropertyField(
                Find("liveRegeneration"),
                new GUIContent(
                    "Live Regeneration",
                    "Automatically rebuilds structural river authoring and spline changes. Rendering, runtime tuning, diagnostics, and Foam Layer E controls update without rebuilding the river or clearing live Foam state."));
        }

        private void DrawRiverDomain()
        {
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

        }

        private void DrawChannel()
        {
            EditorGUILayout.PropertyField(
                Find("width"),
                new GUIContent(
                    "Water Width",
                    "Approximate visible open-water width. The corridor adds a small hidden shoreline cover automatically."));
            EditorGUILayout.PropertyField(
                Find("depth"),
                new GUIContent("Bed Depth"));
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

        }

        private void DrawNaturalVariation()
        {
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

        }

        private void DrawAdvancedShoreline()
        {
            EditorGUILayout.PropertyField(
                Find("shorelineWetClearance"),
                new GUIContent(
                    "Wet Clearance",
                    "Vertical clearance retained between the deformed river surface and the visible shoreline."));
            EditorGUILayout.PropertyField(
                Find("shorelineBankCover"),
                new GUIContent(
                    "Bank Cover",
                    "Additional ground cover maintained above the river surface at the bank handoff."));
            EditorGUILayout.PropertyField(
                Find("reservedDownwardSurfaceDisplacement"),
                new GUIContent(
                    "Reserved Downward Displacement",
                    "Optional clearance beyond the automatically resolved maximum downward surface motion."));
        }

        private void DrawSurfaceMesh()
        {
            EditorGUILayout.PropertyField(
                Find("quality"),
                new GUIContent(
                    "Quality",
                    "Controls water geometry and the shared foam material/topology resolution. Persistent foam, topology, and the canonical obstacle footprint use 64 cells across at Low, 96 at Medium, and 128 at High. The river domain remains the authoritative coordinate source."));
            EditorGUILayout.PropertyField(
                Find("surfaceOffset"),
                new GUIContent("Water Level Offset"));
        }

        private void DrawSurfaceMotion()
        {
            EditorGUILayout.HelpBox(
                "Surface motion is authored as one coherent river-space field. " +
                "Macro displacement, detail normals, current accents, and " +
                "shore motion remain separate controls within that field.",
                MessageType.None);

            DrawNestedSection(
                InspectorSection.MotionGeneralFlow,
                "General Flow",
                DrawMotionGeneralFlow);
            DrawNestedSection(
                InspectorSection.MotionMacroWaves,
                "Macro Waves",
                DrawMotionMacroWaves);
            DrawNestedSection(
                InspectorSection.MotionDetail,
                "Detail Motion",
                DrawMotionDetail);
            DrawNestedSection(
                InspectorSection.MotionCurrentAccents,
                "Current Accents",
                DrawMotionCurrentAccents);
            DrawNestedSection(
                InspectorSection.MotionShoreMotion,
                "Shore Motion",
                DrawMotionShoreMotion);
            DrawNestedSection(
                InspectorSection.MotionShoreWaveProfile,
                "Shore Wave Profile",
                DrawMotionShoreWaveProfile);
        }

        private void DrawMotionGeneralFlow()
        {
            SerializedProperty preset = Find("motionPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Motion Character",
                    "Still, Calm, Flowing, and Furious apply coordinated surface-motion starting points without changing foam, refraction, or water-body settings."));

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
            EditorGUILayout.PropertyField(
                Find("flowSpeed"),
                new GUIContent(
                    "Flow Speed",
                    "Downstream travel speed in metres per second."));
            if (EditorGUI.EndChangeCheck())
            {
                Find("motionPreset").enumValueIndex =
                    (int)StylizedRiverMotionPreset.Custom;
            }
        }

        private void DrawMotionMacroWaves()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("motionWaveHeight"),
                new GUIContent(
                    "Wave Height",
                    "Maximum vertical macro displacement in metres."));
            EditorGUILayout.PropertyField(
                Find("motionWaveLength"),
                new GUIContent(
                    "Wave Length",
                    "Typical physical length of displaced waves in metres."));
            EditorGUILayout.PropertyField(
                Find("motionWaveSteepness"),
                new GUIContent(
                    "Wave Steepness",
                    "Broad rounded waves versus sharper crest-like shapes."));
            MarkMotionPresetCustomIfChanged();
        }

        private void DrawMotionDetail()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("motionDetailStrength"),
                new GUIContent(
                    "Surface Detail Strength",
                    "Strength of small flow-aligned normal detail."));
            EditorGUILayout.PropertyField(
                Find("motionDetailScale"),
                new GUIContent(
                    "Surface Detail Scale",
                    "Typical physical size of ripple detail in metres."));
            EditorGUILayout.PropertyField(
                Find("motionTurbulence"),
                new GUIContent(
                    "Turbulence",
                    "How strongly the pattern evolves instead of only translating."));
            MarkMotionPresetCustomIfChanged();
        }

        private void DrawMotionCurrentAccents()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("currentAccentStrength"),
                new GUIContent(
                    "Current Accent Strength",
                    "Broad downstream modulation. This is not foam."));
            EditorGUILayout.PropertyField(
                Find("currentAccentScale"),
                new GUIContent(
                    "Current Accent Scale",
                    "Typical longitudinal size of current accents in metres."));
            MarkMotionPresetCustomIfChanged();
        }

        private void DrawMotionShoreMotion()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("shoreMotion"),
                new GUIContent(
                    "Shore Motion",
                    "Positive displacement retained where water visibly meets the bank. Negative troughs return to the static waterline at the exact shoreline."));
            EditorGUILayout.PropertyField(
                Find("shoreMotionWidth"),
                new GUIContent(
                    "Shore Motion Width",
                    "Distance inside the visible shoreline over which centre motion blends toward Shore Motion and negative troughs return smoothly to the static waterline."));
            MarkMotionPresetCustomIfChanged();
        }

        private void DrawMotionShoreWaveProfile()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("shoreWaveHeightScale"),
                new GUIContent(
                    "Shore Wave Height Scale",
                    "Vertical shore-wave amplitude relative to the centre-river macro wave."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveShapeLengthScale"),
                new GUIContent(
                    "Shore Wave Length Scale",
                    "Longitudinal size of each individual shore wave relative to the centre-river macro wave. Lower values create shorter, tighter waves; higher values create broader waves that occupy more shoreline length."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveSpacingScale"),
                new GUIContent(
                    "Shore Wave Gap Scale",
                    "Clear longitudinal gap between successive shore-wave packets relative to the centre-river macro wave. Zero makes adjacent packets touch; higher values insert wider calm shoreline gaps without changing individual wave length."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveReach"),
                new GUIContent(
                    "Shore Wave Reach",
                    "Maximum fraction of the generated hidden shoreline allowance that a positive shore-wave crest may wet."));
            if (EditorGUI.EndChangeCheck())
            {
                Find("motionPreset").enumValueIndex =
                    (int)StylizedRiverMotionPreset.Custom;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("additionalShorelineOverlap"),
                new GUIContent(
                    "Positive Overflow Allowance (m)",
                    "Additional hidden water width per bank beyond the automatic overlap. Positive shore-wave crests may use this space according to Shore Wave Reach. Changing it structurally regenerates the river domain and corridor."));
            if (EditorGUI.EndChangeCheck())
            {
                structuralAuthoringChanged = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("shoreWaveTransitionLength"),
                new GUIContent(
                    "Shore Wave Transition Length",
                    "World-space shoulder-shaping distance inside each wave packet. It does not change packet length or the authored gap."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveSizeVariation"),
                new GUIContent(
                    "Shore Wave Size Variation",
                    "Stable deterministic differences between successive shore waves. Height and positive overflow reach remain related but are no longer identical."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveSideAsymmetry"),
                new GUIContent(
                    "Shore Side Asymmetry",
                    "Makes left and right banks use increasingly independent variation."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveProfileVariation"),
                new GUIContent(
                    "Shore Wave Profile Variation",
                    "Varies each shore wave smoothly between its start, middle, and end."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveProfileEvolutionStrength"),
                new GUIContent(
                    "Profile Evolution Strength",
                    "Changes each travelling shore wave's normalized roundness and shoulder shape over time. Zero preserves the exact current profile."));
            EditorGUILayout.PropertyField(
                Find("shoreWaveProfileEvolutionDuration"),
                new GUIContent(
                    "Profile Evolution Duration (s)",
                    "Seconds for one predictable narrow-to-broad-to-narrow evolution cycle. Neighbouring waves receive deterministic phase offsets and do not morph in lockstep."));
            MarkMotionPresetCustomIfChanged();
        }

        private void MarkMotionPresetCustomIfChanged()
        {
            if (EditorGUI.EndChangeCheck())
            {
                Find("motionPreset").enumValueIndex =
                    (int)StylizedRiverMotionPreset.Custom;
            }
        }

        private void DrawRefraction()
        {
            EditorGUILayout.HelpBox(
                "Refraction distorts the already-lit opaque scene beneath the " +
                "river while protecting shorelines, object silhouettes, and " +
                "strong depth discontinuities.",
                MessageType.None);

            DrawRefractionPreset();

            DrawNestedSection(
                InspectorSection.RefractionLiquid,
                "Liquid Refraction",
                DrawLiquidRefractionControls);
            DrawNestedSection(
                InspectorSection.RefractionShoreDepth,
                "Shore & Depth Protection",
                DrawRefractionProtectionControls);
            DrawNestedSection(
                InspectorSection.RefractionFrozen,
                "Frozen Distortion",
                DrawFrozenRefractionControls);
        }

        private void DrawRefractionPreset()
        {
            SerializedProperty preset = Find("refractionPreset");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                preset,
                new GUIContent(
                    "Refraction Character",
                    "None, Clear, Balanced, and Distorted apply bounded screen-space optical starting points."));

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

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

        private void DrawLiquidRefractionControls()
        {
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
                    "How strongly the completed surface normal drives optical distortion."));
            MarkRefractionPresetCustomIfChanged();
        }

        private void DrawRefractionProtectionControls()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("shoreRefraction"),
                new GUIContent(
                    "Shore Refraction",
                    "Distortion retained at the visible bank. It still fades to zero before the buried surface edge."));
            EditorGUILayout.PropertyField(
                Find("depthEdgeProtection"),
                new GUIContent(
                    "Depth-Edge Protection",
                    "Rejects displaced samples that cross rocks, banks, foreground objects, or strong scene-depth discontinuities."));
            EditorGUILayout.PropertyField(
                Find("preserveObjectSilhouettes"),
                new GUIContent(
                    "Preserve Object Silhouettes",
                    "Uses existing depth samples to reduce object-edge contraction and pale disocclusion ghosts."));
            MarkRefractionPresetCustomIfChanged();
        }

        private void DrawFrozenRefractionControls()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(
                Find("iceDistortionStrength"),
                new GUIContent(
                    "Ice Distortion",
                    "Static optical warping through frozen ice. It does not scroll with liquid flow."));
            EditorGUILayout.PropertyField(
                Find("iceDiffusion"),
                new GUIContent(
                    "Ice Diffusion",
                    "Quality-scaled softening of the transmitted scene beneath ice."));
            MarkRefractionPresetCustomIfChanged();
        }

        private void MarkRefractionPresetCustomIfChanged()
        {
            if (EditorGUI.EndChangeCheck())
            {
                Find("refractionPreset").enumValueIndex =
                    (int)StylizedRiverRefractionPreset.Custom;
            }
        }

        private void DrawWaterBody()
        {
            EditorGUILayout.HelpBox(
                "Water-body authoring is separated into state, liquid, frozen, " +
                "lighting, and advanced material groups. Runtime diagnostics " +
                "and debug presentation are handled elsewhere.",
                MessageType.None);

            SerializedProperty surfaceState = Find("surfaceState");
            bool mixedState = surfaceState.hasMultipleDifferentValues;
            StylizedRiverSurfaceState resolvedState =
                (StylizedRiverSurfaceState)surfaceState.enumValueIndex;

            DrawNestedSection(
                InspectorSection.WaterSurfaceState,
                "Surface State",
                DrawWaterSurfaceState);

            bool showLiquid =
                mixedState ||
                resolvedState != StylizedRiverSurfaceState.Frozen;
            if (showLiquid)
            {
                DrawNestedSection(
                    InspectorSection.WaterLiquidBody,
                    "Liquid Body",
                    DrawLiquidBodyControls);
            }

            bool showFrozen =
                mixedState ||
                resolvedState != StylizedRiverSurfaceState.Liquid;
            if (showFrozen)
            {
                DrawNestedSection(
                    InspectorSection.WaterFrozenBody,
                    "Frozen Body",
                    DrawFrozenBodyControls);
            }

            DrawNestedSection(
                InspectorSection.WaterShorelineAccent,
                "Shoreline Accent",
                DrawShorelineAccentControls);

            DrawNestedSection(
                InspectorSection.WaterLightingResponse,
                "Lighting Response",
                DrawWaterLightingControls);
            DrawNestedSection(
                InspectorSection.WaterAdvancedMaterial,
                "Advanced Material",
                DrawAdvancedBody);
        }

        private void DrawWaterSurfaceState()
        {
            SerializedProperty surfaceState = Find("surfaceState");
            EditorGUILayout.PropertyField(
                surfaceState,
                new GUIContent(
                    "Surface State",
                    "Liquid and Frozen are authored endpoints. Custom exposes a continuous freeze value."));

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
        }

        private void DrawShorelineAccentControls()
        {
            EditorGUILayout.HelpBox(
                "Draws one stylized water-side line along the complete current shoreline. " +
                "Ordinary and positive-overflow regions use the same dynamic edge; " +
                "interior rocks and other scene silhouettes are not outlined.",
                MessageType.None);

            EditorGUILayout.PropertyField(
                Find("shorelineAccentColor"),
                new GUIContent(
                    "Colour",
                    "Authored shoreline-line colour before the signed Brightness adjustment."));
            EditorGUILayout.PropertyField(
                Find("shorelineAccentStrength"),
                new GUIContent(
                    "Strength",
                    "Overall blend amount. Zero disables the accent without changing shoreline geometry or overflow."));
            EditorGUILayout.PropertyField(
                Find("shorelineAccentWidth"),
                new GUIContent(
                    "Width (m)",
                    "World-space thickness measured inward from the current shoreline on the water side."));
            EditorGUILayout.PropertyField(
                Find("shorelineEdgeBlendWidth"),
                new GUIContent(
                    "Edge Blend Width (m)",
                    "World-space width over which the completed water, Foam, and accent colour blends back to the already-rendered opaque scene at the shoreline. Zero preserves a hard edge. This remains active when Accent Strength is zero."));
            EditorGUILayout.PropertyField(
                Find("shorelineAccentBrightness"),
                new GUIContent(
                    "Brightness",
                    "Signed colour multiplier. Negative values darken the accent; positive values brighten it. Minus one reaches black and plus one doubles the authored colour."));
        }

        private void DrawWaterLightingControls()
        {
            EditorGUILayout.PropertyField(
                Find("lightDependence"),
                new GUIContent(
                    "Light Dependence",
                    "Zero keeps authored colours largely fixed. One makes the body fully dependent on actual scene lighting."));
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
                    "Zero uses light brightness only. One allows scene lights to fully tint the river."));
            EditorGUILayout.PropertyField(
                Find("minimumNightVisibility"),
                new GUIContent(
                    "Minimum Night Visibility",
                    "Minimum retained body illumination when meaningful light is absent."));
            EditorGUILayout.PropertyField(
                Find("shadowResponse"),
                new GUIContent(
                    "Shadow Response Master",
                    "Master strength for real-time shadowing of the intrinsic water or ice contribution."));
            EditorGUILayout.PropertyField(
                Find("liquidSurfaceShadowResponse"),
                new GUIContent(
                    "Liquid Surface Shadow",
                    "How strongly main-light shadow affects intrinsic liquid tint and surface lighting."));
            EditorGUILayout.PropertyField(
                Find("iceSurfaceShadowResponse"),
                new GUIContent(
                    "Ice Surface Shadow",
                    "How strongly main-light shadow affects the frozen ice body and surface."));
        }

        private void DrawLiquidBodyControls()
        {

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
        }
    }
}
