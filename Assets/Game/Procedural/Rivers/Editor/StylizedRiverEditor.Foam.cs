using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

namespace ProgrammaticStylized3D.Rivers.Editor
{
    internal sealed partial class StylizedRiverEditor
    {
        private void DrawFoam()
        {
            EditorGUILayout.HelpBox(
                "Foam production authoring follows the accepted Layer A–E " +
                "ownership model. Live telemetry, debug views, and test tools " +
                "are intentionally outside this authoring section.",
                MessageType.None);

            DrawNestedSection(
                InspectorSection.FoamRuntimeQuality,
                "Runtime & Quality",
                DrawFoamRuntimeQuality);
            DrawNestedSection(
                InspectorSection.FoamLayerA,
                "Layer A — Topology & Support",
                DrawFoamLayerA);
            DrawNestedSection(
                InspectorSection.FoamLayerB,
                "Layer B — Canonical Velocity",
                DrawFoamLayerB);
            DrawNestedSection(
                InspectorSection.FoamLayerC,
                "Layer C — Persistent Material & Lifecycle",
                DrawFoamLayerC);
            DrawNestedSection(
                InspectorSection.FoamLayerD,
                "Layer D — Evaluated Shape",
                DrawFoamLayerD);
            DrawNestedSection(
                InspectorSection.FoamLayerE,
                "Layer E — Rendering",
                DrawFoamLayerE);
        }

        private void DrawFoamRuntimeQuality()
        {
            EditorGUILayout.PropertyField(
                Find("foamEnabled"),
                new GUIContent(
                    "Foam Enabled",
                    "Master switch for persistent foam. Disabled foam allocates no simulation textures and contributes nothing to the water shader."));
        }

        private void DrawFoamLayerA()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(
                    Find("foamTopologyCacheAsset"),
                    new GUIContent(
                        "Topology Cache Asset",
                        "Persistent topology payload associated with this river. In development, missing or stale cache data is generated and persisted automatically; release builds remain cache-only."));
            }

            if (targets.Length == 1 &&
                target is StylizedRiver river &&
                river.FoamTopologyCacheAsset == null)
            {
                EditorGUILayout.HelpBox(
                    Application.isPlaying
                        ? "No persistent topology cache was available for this Play session. Development generation may continue session-only."
                        : "No topology cache is assigned. Normal Play entry from a saved scene can create and assign the deterministic cache.",
                    MessageType.Info);
            }

            DrawNestedSection(
                InspectorSection.FoamLayerAMajorSupport,
                "Major Support",
                DrawFoamLayerAMajorSupport);
            DrawNestedSection(
                InspectorSection.FoamLayerAConnectors,
                "Connectors",
                DrawFoamLayerAConnectors);
            DrawNestedSection(
                InspectorSection.FoamLayerANegativeTopology,
                "Negative Topology",
                DrawFoamLayerANegativeTopology);
        }

        private void DrawFoamLayerAMajorSupport()
        {
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportAmount"),
                new GUIContent(
                    "Amount",
                    "Controls the nested deterministic population of whole-river Major Support. Higher values activate later-ranked opportunities without reshaping earlier accepted regions."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSize"),
                new GUIContent(
                    "Size",
                    "Controls the physical scale envelope of stable Major opportunities."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSizeVariation"),
                new GUIContent(
                    "Size Variation",
                    "Controls relative size spread between stable Major opportunities without changing their identity."));
            EditorGUILayout.PropertyField(
                Find("foamMajorRecycleTerritoryDeviationPercent"),
                new GUIContent(
                    "Recycle Territory Deviation (%)",
                    "Maximum longitudinal deviation from a Major's original accepted river position when it recycles."));
            EditorGUILayout.PropertyField(
                Find("foamMajorLifetimeUnits"),
                new GUIContent(
                    "Lifetime Units",
                    "Average combined dwell-and-movement lifetime budget for one Major occurrence."));
            EditorGUILayout.PropertyField(
                Find("foamMajorLifetimeUnitDeviation"),
                new GUIContent(
                    "Lifetime Unit Deviation",
                    "Deterministic plus-or-minus variation around Major Lifetime Units, with a minimum of one."));
            EditorGUILayout.PropertyField(
                Find("foamMajorSupportSeed"),
                new GUIContent(
                    "Seed",
                    "Deterministic seed for stable whole-river Major opportunity identity, transforms, and evolution metadata."));
        }

        private void DrawFoamLayerAConnectors()
        {
            EditorGUILayout.PropertyField(
                Find("foamConnectorAmount"),
                new GUIContent(
                    "Amount",
                    "Controls the accepted relationship population without creating an all-to-all web."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorDirectness"),
                new GUIContent(
                    "Directness",
                    "One favours facing endpoints and short valid routes. Lower values broaden endpoint choice and permit stable broad bends."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorLengthPreference"),
                new GUIContent(
                    "Length Preference",
                    "Favours shorter or longer valid connections inside the fixed safe envelope."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorBreakStretchRatio"),
                new GUIContent(
                    "Break Stretch Ratio",
                    "Maximum live length relative to the active reference before the relationship breaks and attempts a prepared rebind."));
        }

        private void DrawFoamLayerANegativeTopology()
        {
            EditorGUILayout.PropertyField(
                Find("foamInteriorPocketAmount"),
                new GUIContent(
                    "Interior Pocket Amount",
                    "Controls closed Major-hosted negative regions without reshuffling earlier identities."));
            EditorGUILayout.PropertyField(
                Find("foamEdgeCavityAmount"),
                new GUIContent(
                    "Edge Cavity Amount",
                    "Controls lopsided Major-hosted negative regions that breach one selected side while preserving positive support."));
            EditorGUILayout.PropertyField(
                Find("foamConnectorWeakSpanAmount"),
                new GUIContent(
                    "Connector Weak Span Amount",
                    "Controls short Connector-hosted negative regions that locally weaken rather than delete relationships."));
            EditorGUILayout.PropertyField(
                Find("foamFreeWaterEventAmount"),
                new GUIContent(
                    "Free-Water Event Amount",
                    "Controls sparse valid-water negative events that require no Major or Connector host."));
        }

        private void DrawFoamLayerB()
        {
            EditorGUILayout.PropertyField(
                Find("foamMaterialFlowSpeedMultiplier"),
                new GUIContent(
                    "Downstream Speed Ratio",
                    "Base persistent-foam speed relative to authored river Flow Speed."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldStrength"),
                new GUIContent(
                    "Maximum Lateral Speed Ratio",
                    "Maximum signed lateral speed relative to base downstream foam speed."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldScrollHz"),
                new GUIContent(
                    "Lane Advection Ratio",
                    "Downstream speed of the generated lane pattern relative to base foam speed."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldLaneScale"),
                new GUIContent(
                    "Direction Change Frequency",
                    "Controls how often lateral route intent changes sign downstream."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldAcrossRiverCoherence"),
                new GUIContent(
                    "Across-River Coherence",
                    "Controls how broadly lateral route intent is shared across river width."));
            EditorGUILayout.PropertyField(
                Find("foamMotionFieldNeutralCoverage"),
                new GUIContent(
                    "Low Lateral Motion Coverage",
                    "Approximate fraction of the route field compressed toward very low lateral intent."));
            EditorGUILayout.PropertyField(
                Find("foamObstacleSlowdownStrength"),
                new GUIContent(
                    "Obstacle Slowdown Strength",
                    "How strongly obstacle-routing influence reduces local downstream foam speed."));
            EditorGUILayout.PropertyField(
                Find("foamObstacleMinimumDownstreamFactor"),
                new GUIContent(
                    "Obstacle Minimum Downstream Factor",
                    "Minimum downstream-speed factor at maximum obstacle influence. Zero permits temporary stagnation without upstream motion."));
        }

        private void DrawFoamLayerC()
        {
            DrawNestedSection(
                InspectorSection.FoamLayerCLifecycle,
                "Lifecycle",
                DrawFoamLayerCLifecycle);
            DrawNestedSection(
                InspectorSection.FoamLayerCAutomaticBirth,
                "Automatic Birth Sources",
                DrawFoamAutomaticSourcePopulationSection);
        }

        private void DrawFoamLayerCLifecycle()
        {
            EditorGUILayout.PropertyField(
                Find("foamNeutralLifetime"),
                new GUIContent(
                    "Neutral Lifetime (s)",
                    "Normalized Remaining Life reaches zero after approximately this many seconds in neutral water."));
            EditorGUILayout.PropertyField(
                Find("foamSupportedAgingRate"),
                new GUIContent(
                    "Supported Aging Rate",
                    "Aging-rate multiplier at full positive support. Values below one extend life."));
            EditorGUILayout.PropertyField(
                Find("foamNegativeAgingRate"),
                new GUIContent(
                    "Negative Aging Rate",
                    "Aging-rate multiplier at full Negative Aging Pressure. Values above one shorten life."));
        }

        private void DrawFoamLayerD()
        {
            EditorGUILayout.PropertyField(
                Find("foamVisualOccupancyBuildTime"),
                new GUIContent(
                    "Visual Occupancy Build Time",
                    "Time used by Layer D temporal occupancy to build toward the current instantaneous shape target."));
            EditorGUILayout.PropertyField(
                Find("foamVisualOccupancyReleaseTime"),
                new GUIContent(
                    "Visual Occupancy Release Time",
                    "Time used by Layer D temporal occupancy to release coverage after the instantaneous target recedes."));
        }

        private void DrawFoamLayerE()
        {
            EditorGUILayout.PropertyField(
                Find("foamFinalVisibilityMode"),
                new GUIContent(
                    "Final Foam Visibility Mode",
                    "Render-only choice between concentration-gated Final Foam and lifecycle-faithful coverage. Stored material and lifecycle remain unchanged."));
            EditorGUILayout.PropertyField(
                Find("foamColour"),
                new GUIContent(
                    "Foam Colour",
                    "Lit, non-emissive base tint resolved before water bleed-through. Alpha sets the base Foam opacity before the established-interior floor is applied."));
            EditorGUILayout.PropertyField(
                Find("foamInteriorOpacityFloor"),
                new GUIContent(
                    "Interior Opacity Floor",
                    "Absolute minimum opacity for established Foam interiors. This may exceed Foam Colour alpha, but it does not affect weak fringe or create Foam outside the incoming silhouette. Zero preserves the accepted pre-5.17A composition."));
            EditorGUILayout.PropertyField(
                Find("foamEdgeContrast"),
                new GUIContent(
                    "Edge Contrast",
                    "Controls the existing edge-versus-interior lighting contrast. Negative values suppress the bright rim, zero preserves the accepted pre-5.17A lighting, and positive values intensify the edge without expanding the silhouette."));
            EditorGUILayout.PropertyField(
                Find("foamChipStrength"),
                new GUIContent(
                    "Chip Strength",
                    "Strength of medium render-only edge bites and derived short cuts. Zero preserves the accepted pre-5.17B silhouette; this never changes stored Foam material or Layer D shape."));
            EditorGUILayout.PropertyField(
                Find("foamFrayStrength"),
                new GUIContent(
                    "Fray Strength",
                    "Strength of fine render-only shredding confined to weak Foam fringe coverage. Zero preserves the accepted pre-5.17B silhouette."));
            EditorGUILayout.PropertyField(
                Find("foamBreakupScale"),
                new GUIContent(
                    "Breakup Scale",
                    "Chooses between finer, more frequent and broader, less frequent stable breakup features in river/material space. It has no visible effect while Chip and Fray Strength are both zero."));
        }

        private void DrawNormalizedPatternWeight(
            SerializedProperty primary,
            SerializedProperty secondary,
            GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.Slider(
                label,
                primary.floatValue,
                0f,
                1f);
            if (EditorGUI.EndChangeCheck())
            {
                value = Mathf.Clamp01(value);
                primary.floatValue = value;
                secondary.floatValue = 1f - value;
            }
        }

        private void DrawNormalizedPatternWeight3(
            SerializedProperty primary,
            SerializedProperty secondary,
            SerializedProperty tertiary,
            GUIContent label)
        {
            EditorGUI.BeginChangeCheck();
            float value = EditorGUILayout.Slider(
                label,
                primary.floatValue,
                0f,
                1f);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            value = Mathf.Clamp01(value);
            float remaining = 1f - value;
            float secondaryOld = Mathf.Clamp01(secondary.floatValue);
            float tertiaryOld = Mathf.Clamp01(tertiary.floatValue);
            float otherTotal = secondaryOld + tertiaryOld;

            primary.floatValue = value;
            if (otherTotal <= 0.0001f)
            {
                secondary.floatValue = remaining * 0.5f;
                tertiary.floatValue = remaining * 0.5f;
                return;
            }

            secondary.floatValue = remaining * (secondaryOld / otherTotal);
            tertiary.floatValue = remaining * (tertiaryOld / otherTotal);
        }

        private void DrawMinMaxMetreControls(
            string label,
            SerializedProperty minimum,
            SerializedProperty maximum)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                minimum,
                new GUIContent("Min", $"Minimum {label.ToLowerInvariant()} in metres."));
            EditorGUILayout.PropertyField(
                maximum,
                new GUIContent("Max", $"Maximum {label.ToLowerInvariant()} in metres."));
            EditorGUI.indentLevel--;
        }

        private void DrawMinMaxUnitControls(
            string label,
            SerializedProperty minimum,
            SerializedProperty maximum,
            string tooltip)
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(
                minimum,
                new GUIContent("Min", tooltip));
            EditorGUILayout.PropertyField(
                maximum,
                new GUIContent("Max", tooltip));
            EditorGUI.indentLevel--;
        }

        private void DrawFoamAutomaticSourcePopulationSection()
        {
            EditorGUILayout.HelpBox(
                "Automatic birth creates real persistent FoamState material. Off disables automatic birth; otherwise each source category is controlled by its own Enabled toggle. Shore, Object, and Free Water Foam are Layer C source classes.",
                MessageType.None);

            EditorGUILayout.PropertyField(
                Find("foamAutomaticBirthEnabled"),
                new GUIContent(
                    "Automatic Foam Birth",
                    "Turns automatic Layer C material birth on or off. Support topology still only preserves or suppresses material after it exists."));
            EditorGUILayout.PropertyField(
                Find("foamSourcePopulationPreset"),
                new GUIContent(
                    "Spawn Preset",
                    "Off disables automatic birth. Other presets are authoring/validation presets; Shore and Object source categories are controlled by their own Enabled toggles."));

            EditorGUILayout.Space(4f);
            if (DrawInlineFoldout(
                    InspectorSection.FoamBirthShore,
                    "Shore Foam"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    Find("foamAutomaticShoreBirthEnabled"),
                    new GUIContent(
                        "Enabled",
                        "Enables deterministic shore/contact Layer C material birth when Automatic Foam Birth is on and Spawn Preset is not Off."));
                EditorGUILayout.PropertyField(
                    Find("foamShoreFoamCoverage"),
                    new GUIContent(
                        "Coverage",
                        "How much eligible shoreline can participate in deterministic source events over time. This does not change event opacity or patch size."));
                EditorGUILayout.PropertyField(
                    Find("foamShoreFoamActivity"),
                    new GUIContent(
                        "Activity",
                        "How often new shore source events start. Higher values start more full-strength source events per second."));
                EditorGUILayout.PropertyField(
                    Find("foamShoreFoamPatchSize"),
                    new GUIContent(
                        "Global Size Multiplier",
                        "Broad global scale selector for all shore-source pattern dimensions. Per-pattern length, width, reach, and offset controls below define the actual ranges."));
                EditorGUILayout.PropertyField(
                    Find("foamShoreFoamFormationSpeedMetresPerSecond"),
                    new GUIContent(
                        "Global Formation Speed",
                        "Base source reveal speed in metres per second. Per-pattern Formation Speed multipliers below can make individual patterns reveal faster or slower."));
                EditorGUILayout.PropertyField(
                    Find("foamShoreFoamPattern"),
                    new GUIContent(
                        "Debug Pattern Mode",
                        "Mixed uses the normalized pattern weights below. Shore Ribbons and Inward Wash force one pattern for validation."));

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Pattern Mix", EditorStyles.boldLabel);
                SerializedProperty ribbonWeight = Find("foamShoreRibbonPatternWeight");
                SerializedProperty washWeight = Find("foamInwardWashPatternWeight");
                DrawNormalizedPatternWeight(
                    ribbonWeight,
                    washWeight,
                    new GUIContent(
                        "Shore Ribbons",
                        "Normalized share of Mixed Shore Foam events assigned to Shore Ribbon sources. Editing this automatically updates Inward Wash to keep the mix sum at one."));
                DrawNormalizedPatternWeight(
                    washWeight,
                    ribbonWeight,
                    new GUIContent(
                        "Inward Wash",
                        "Normalized share of Mixed Shore Foam events assigned to Inward Wash sources. Editing this automatically updates Shore Ribbons to keep the mix sum at one."));

                EditorGUILayout.Space(4f);
                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthShoreRibbonPattern,
                        "Shore Ribbon Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamShoreRibbonFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Global Formation Speed for Shore Ribbon events only."));
                    DrawMinMaxMetreControls(
                        "Length",
                        Find("foamShoreRibbonLengthMinMetres"),
                        Find("foamShoreRibbonLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamShoreRibbonWidthMinMetres"),
                        Find("foamShoreRibbonWidthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Shore Offset",
                        Find("foamShoreRibbonOffsetMinMetres"),
                        Find("foamShoreRibbonOffsetMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamShoreRibbonInitialLifeMin"),
                        Find("foamShoreRibbonInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamShoreRibbonBreakupStrengthMin"),
                        Find("foamShoreRibbonBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthInwardWashPattern,
                        "Inward Wash Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamInwardWashFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Global Formation Speed for Inward Wash events only."));
                    DrawMinMaxMetreControls(
                        "Length",
                        Find("foamInwardWashLengthMinMetres"),
                        Find("foamInwardWashLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamInwardWashWidthMinMetres"),
                        Find("foamInwardWashWidthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Inward Reach",
                        Find("foamInwardWashReachMinMetres"),
                        Find("foamInwardWashReachMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Shore Start Offset",
                        Find("foamInwardWashOffsetMinMetres"),
                        Find("foamInwardWashOffsetMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamInwardWashInitialLifeMin"),
                        Find("foamInwardWashInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamInwardWashBreakupStrengthMin"),
                        Find("foamInwardWashBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4f);
            if (DrawInlineFoldout(
                    InspectorSection.FoamBirthObject,
                    "Object Foam"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    Find("foamAutomaticObjectBirthEnabled"),
                    new GUIContent(
                        "Enabled",
                        "Enables deterministic static object/contact Layer C material birth when Automatic Foam Birth is on and Spawn Preset is not Off."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamCoverage"),
                    new GUIContent(
                        "Coverage",
                        "How much of the registered static object/contact population can participate in deterministic source events over time."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamActivity"),
                    new GUIContent(
                        "Activity",
                        "How often new object-contact source events start."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamFormationSpeedMetresPerSecond"),
                    new GUIContent(
                        "Global Formation Speed",
                        "Base source reveal speed in metres per second for Object Foam."));
                EditorGUILayout.PropertyField(
                    Find("foamObjectFoamPattern"),
                    new GUIContent(
                        "Debug Pattern Mode",
                        "Mixed uses the normalized pattern weights below. Contact Arcs, Contact Semi-Arcs, and Contact Flecks force one pattern for validation."));

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Pattern Mix", EditorStyles.boldLabel);
                SerializedProperty arcWeight = Find("foamObjectContactArcPatternWeight");
                SerializedProperty semiArcWeight = Find("foamObjectContactSemiArcPatternWeight");
                SerializedProperty fleckWeight = Find("foamObjectContactFleckPatternWeight");
                DrawNormalizedPatternWeight3(
                    arcWeight,
                    semiArcWeight,
                    fleckWeight,
                    new GUIContent(
                        "Contact Arcs",
                        "Normalized share of Mixed Object Foam events assigned to full contact arcs. Editing this preserves the relative share of the other object patterns."));
                DrawNormalizedPatternWeight3(
                    semiArcWeight,
                    arcWeight,
                    fleckWeight,
                    new GUIContent(
                        "Contact Semi-Arcs",
                        "Normalized share of Mixed Object Foam events assigned to lopsided one-sided contact arcs. Editing this preserves the relative share of the other object patterns."));
                DrawNormalizedPatternWeight3(
                    fleckWeight,
                    arcWeight,
                    semiArcWeight,
                    new GUIContent(
                        "Contact Flecks",
                        "Normalized share of Mixed Object Foam events assigned to small contact flecks. Editing this preserves the relative share of the other object patterns."));

                EditorGUILayout.Space(4f);
                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactArcPattern,
                        "Object Contact Arc Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamObjectContactArcFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Object Foam Global Formation Speed for Contact Arc events only."));
                    DrawMinMaxMetreControls(
                        "Arc Length",
                        Find("foamObjectContactArcLengthMinMetres"),
                        Find("foamObjectContactArcLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamObjectContactArcWidthMinMetres"),
                        Find("foamObjectContactArcWidthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Contact Offset",
                        Find("foamObjectContactArcOffsetMinMetres"),
                        Find("foamObjectContactArcOffsetMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactArcInitialLifeMin"),
                        Find("foamObjectContactArcInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamObjectContactArcBreakupStrengthMin"),
                        Find("foamObjectContactArcBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactSemiArcPattern,
                        "Object Contact Semi-Arc Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamObjectContactSemiArcFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Object Foam Global Formation Speed for Contact Semi-Arc events only."));
                    DrawMinMaxMetreControls(
                        "Semi-Arc Length",
                        Find("foamObjectContactSemiArcLengthMinMetres"),
                        Find("foamObjectContactSemiArcLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamObjectContactSemiArcWidthMinMetres"),
                        Find("foamObjectContactSemiArcWidthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Contact Offset",
                        Find("foamObjectContactSemiArcOffsetMinMetres"),
                        Find("foamObjectContactSemiArcOffsetMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactSemiArcInitialLifeMin"),
                        Find("foamObjectContactSemiArcInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamObjectContactSemiArcBreakupStrengthMin"),
                        Find("foamObjectContactSemiArcBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    DrawMinMaxUnitControls(
                        "Lopsidedness",
                        Find("foamObjectContactSemiArcLopsidednessMin"),
                        Find("foamObjectContactSemiArcLopsidednessMax"),
                        "Signed by event seed at runtime. Higher values push the source farther onto one shoulder of the object contact edge.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthObjectContactFleckPattern,
                        "Object Contact Fleck Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamObjectContactFleckFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Object Foam Global Formation Speed for Contact Fleck events only."));
                    DrawMinMaxMetreControls(
                        "Fleck Length",
                        Find("foamObjectContactFleckLengthMinMetres"),
                        Find("foamObjectContactFleckLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamObjectContactFleckWidthMinMetres"),
                        Find("foamObjectContactFleckWidthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Contact Offset",
                        Find("foamObjectContactFleckOffsetMinMetres"),
                        Find("foamObjectContactFleckOffsetMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamObjectContactFleckInitialLifeMin"),
                        Find("foamObjectContactFleckInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamObjectContactFleckBreakupStrengthMin"),
                        Find("foamObjectContactFleckBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.HelpBox(
                    "Static Object Foam is Layer C birth only: contact arcs, semi-arcs, and flecks are anchored from registered static disturbance sources, then gated by obstacle exclusion and static pressure on the GPU.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }

            if (DrawInlineFoldout(
                    InspectorSection.FoamBirthFreeWater,
                    "Free Water Foam"))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(
                    Find("foamAutomaticFreeWaterBirthEnabled"),
                    new GUIContent(
                        "Enabled",
                        "Enables deterministic open-water Layer C material birth when Automatic Foam Birth is on and Spawn Preset is not Off."));
                EditorGUILayout.PropertyField(
                    Find("foamFreeWaterFoamCoverage"),
                    new GUIContent(
                        "Coverage",
                        "How much of the deterministic open-water source lattice can participate over time."));
                EditorGUILayout.PropertyField(
                    Find("foamFreeWaterFoamActivity"),
                    new GUIContent(
                        "Activity",
                        "How often new open-water source events start."));
                EditorGUILayout.PropertyField(
                    Find("foamFreeWaterFoamFormationSpeedMetresPerSecond"),
                    new GUIContent(
                        "Global Formation Speed",
                        "Base source reveal speed in metres per second for Free Water Foam."));
                EditorGUILayout.PropertyField(
                    Find("foamFreeWaterFoamPattern"),
                    new GUIContent(
                        "Debug Pattern Mode",
                        "Mixed uses the normalized pattern weights below. Lace Connectors, Cross-Lace Connectors, and Torn Fragments force one pattern for validation."));

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Pattern Mix", EditorStyles.boldLabel);
                SerializedProperty laceWeight =
                    Find("foamFreeWaterLaceConnectorPatternWeight");
                SerializedProperty crossLaceWeight =
                    Find("foamFreeWaterCrossLaceConnectorPatternWeight");
                SerializedProperty fragmentWeight =
                    Find("foamFreeWaterTornFragmentPatternWeight");
                DrawNormalizedPatternWeight3(
                    laceWeight,
                    crossLaceWeight,
                    fragmentWeight,
                    new GUIContent(
                        "Lace Connectors",
                        "Normalized share of Mixed Free Water Foam events assigned to with-flow lace connector strokes. Editing this preserves the relative share of the other free-water patterns."));
                DrawNormalizedPatternWeight3(
                    crossLaceWeight,
                    laceWeight,
                    fragmentWeight,
                    new GUIContent(
                        "Cross-Lace Connectors",
                        "Normalized share of Mixed Free Water Foam events assigned to cross-current horizontal lace strokes. Editing this preserves the relative share of the other free-water patterns."));
                DrawNormalizedPatternWeight3(
                    fragmentWeight,
                    laceWeight,
                    crossLaceWeight,
                    new GUIContent(
                        "Torn Fragments",
                        "Normalized share of Mixed Free Water Foam events assigned to progressive swept torn fragments. Editing this preserves the relative share of the other free-water patterns."));

                EditorGUILayout.Space(4f);
                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthFreeWaterLacePattern,
                        "Free Water Lace Connector Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamFreeWaterLaceFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Free Water Global Formation Speed for Lace Connector events only."));
                    DrawMinMaxMetreControls(
                        "Length",
                        Find("foamFreeWaterLaceLengthMinMetres"),
                        Find("foamFreeWaterLaceLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamFreeWaterLaceWidthMinMetres"),
                        Find("foamFreeWaterLaceWidthMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterLaceInitialLifeMin"),
                        Find("foamFreeWaterLaceInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamFreeWaterLaceBreakupStrengthMin"),
                        Find("foamFreeWaterLaceBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    DrawMinMaxUnitControls(
                        "Curvature",
                        Find("foamFreeWaterLaceCurvatureMin"),
                        Find("foamFreeWaterLaceCurvatureMax"),
                        "Signed by event seed at runtime. Higher values bend the lace connector more strongly across open water.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthFreeWaterCrossLacePattern,
                        "Free Water Cross-Lace Connector Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamFreeWaterCrossLaceFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Free Water Global Formation Speed for Cross-Lace Connector events only."));
                    DrawMinMaxMetreControls(
                        "Lateral Length",
                        Find("foamFreeWaterCrossLaceLengthMinMetres"),
                        Find("foamFreeWaterCrossLaceLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamFreeWaterCrossLaceWidthMinMetres"),
                        Find("foamFreeWaterCrossLaceWidthMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterCrossLaceInitialLifeMin"),
                        Find("foamFreeWaterCrossLaceInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamFreeWaterCrossLaceBreakupStrengthMin"),
                        Find("foamFreeWaterCrossLaceBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                if (DrawInlineFoldout(
                        InspectorSection.FoamBirthFreeWaterFragmentPattern,
                        "Free Water Torn Fragment Pattern"))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(
                        Find("foamFreeWaterFragmentFormationSpeedMultiplier"),
                        new GUIContent(
                            "Formation Speed",
                            "Multiplier applied to Free Water Global Formation Speed for Torn Fragment events only."));
                    DrawMinMaxMetreControls(
                        "Length",
                        Find("foamFreeWaterFragmentLengthMinMetres"),
                        Find("foamFreeWaterFragmentLengthMaxMetres"));
                    DrawMinMaxMetreControls(
                        "Width",
                        Find("foamFreeWaterFragmentWidthMinMetres"),
                        Find("foamFreeWaterFragmentWidthMaxMetres"));
                    DrawMinMaxUnitControls(
                        "Initial Life",
                        Find("foamFreeWaterFragmentInitialLifeMin"),
                        Find("foamFreeWaterFragmentInitialLifeMax"),
                        "Initial normalized Remaining Life assigned to spawned material. One means full authored foam lifetime; lower values die sooner under the normal aging rules.");
                    DrawMinMaxUnitControls(
                        "Breakup Strength",
                        Find("foamFreeWaterFragmentBreakupStrengthMin"),
                        Find("foamFreeWaterFragmentBreakupStrengthMax"),
                        "Deterministic edge/source breakup strength for this pattern.");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.HelpBox(
                    "Free Water Foam is Layer C birth only: Lace Connectors use a moving head+stroke along flow, Cross-Lace Connectors use a moving head+stroke across the river, and Torn Fragments use a progressive swept patch. Bright specular glints are intentionally not spawned as persistent material.",
                    MessageType.Info);
                EditorGUI.indentLevel--;
            }


        }
    }
}
