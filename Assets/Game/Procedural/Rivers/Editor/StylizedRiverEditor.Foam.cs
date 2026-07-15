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

            bool hasRiver = false;
            bool allFoamEnabled = true;
            bool held = false;
            bool mixed = false;
            foreach (Object selectedTarget in targets)
            {
                if (selectedTarget is not StylizedRiver river)
                {
                    continue;
                }

                allFoamEnabled &= river.FoamEnabled;
                if (!hasRiver)
                {
                    held = river.FoamStateHeld;
                    hasRiver = true;
                }
                else if (river.FoamStateHeld != held)
                {
                    mixed = true;
                }
            }

            using (new EditorGUI.DisabledScope(
                       !Application.isPlaying || !hasRiver ||
                       !allFoamEnabled))
            {
                EditorGUI.showMixedValue = mixed;
                EditorGUI.BeginChangeCheck();
                bool nextHeld = EditorGUILayout.Toggle(
                    new GUIContent(
                        "Hold Foam State",
                        "Play Mode diagnostic. Preserves the current Layer C material and Layer D products while continuing to rebind Layer E rendering controls for exact same-state visual comparisons. Pending births and simulation time resume without catch-up when released."),
                    held);
                if (EditorGUI.EndChangeCheck())
                {
                    foreach (Object selectedTarget in targets)
                    {
                        if (selectedTarget is StylizedRiver river)
                        {
                            river.SetFoamStateHeld(nextHeld);
                        }
                    }

                    RepaintScene();
                }
                EditorGUI.showMixedValue = false;
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Hold Foam State is available in Play Mode and is not saved as authoring data.",
                    MessageType.None);
            }
        }

        private void DrawFoamLayerA()
        {
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                EditorGUILayout.PropertyField(
                    Find("foamTopologyCacheAsset"),
                    new GUIContent(
                        "Topology Cache Asset",
                        "Persistent prepared topology associated with this river. Exact caches load directly; stale-compatible caches remain session-local, while missing or incompatible caches require explicit Edit Mode preparation."));
            }

            if (targets.Length == 1 &&
                target is StylizedRiver river &&
                river.FoamTopologyCacheAsset == null)
            {
                EditorGUILayout.HelpBox(
                    Application.isPlaying
                        ? "No usable topology cache was available. Foam topology remains disabled for this session; leave Play Mode and prepare a cache explicitly."
                        : "No topology cache is assigned. Create one, then use Prepare / Rebuild Foam Topology Cache before entering Play Mode.",
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
            EditorGUILayout.LabelField(
                "Production Chipping",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Production Chipping now exposes six primary controls: Amount, Size, Spacing, Irregularity, Edge Width, and optional Interior Access. One canonical material-permission model owns edge and interior territory. Use Chip Candidate Field, Chip Eligibility Composite, and Production Chip Mask to inspect the complete handoff.",
                MessageType.Info);
            EditorGUILayout.PropertyField(
                Find("foamChipActivation"),
                new GUIContent(
                    "Chip Amount",
                    "Fraction of deterministic Chip identities active in production. Zero is an exact no-Chip result; one activates every available candidate before Edge Width and optional Interior Access permissions."));
            SerializedProperty chipSpacing = Find("foamChipCandidateSpacing");
            SerializedProperty chipSize = Find("foamChipSize");
            SerializedProperty chipIrregularity = Find(
                "foamChipIrregularity");
            EditorGUILayout.PropertyField(
                chipSize,
                new GUIContent(
                    "Chip Size",
                    "Relative mean Chip size within Chip Spacing. Zero maps to a radius of 5% of spacing; one maps to 65%. This bounded representation keeps candidate search cost predictable."));
            EditorGUILayout.PropertyField(
                chipSpacing,
                new GUIContent(
                    "Chip Spacing (m)",
                    "Average world-space spacing between possible Chip centres. Lower values create more candidates; higher values create fewer, more isolated candidates."));
            EditorGUILayout.PropertyField(
                chipIrregularity,
                new GUIContent(
                    "Chip Irregularity",
                    "One static variation control for centre jitter, candidate size variance, and connected contour asymmetry. Zero gives equal circles on a regular lattice; one uses the camera-readable 0.80×–1.40× radius range and the accepted maximum contour variation."));

            bool mixedChipShapeInputs =
                chipSpacing.hasMultipleDifferentValues ||
                chipSize.hasMultipleDifferentValues ||
                chipIrregularity.hasMultipleDifferentValues;
            if (mixedChipShapeInputs)
            {
                DrawReadOnlyRow(
                    new GUIContent("Effective Mean Radius"),
                    "Mixed");
                DrawReadOnlyRow(
                    new GUIContent("Effective Mean Diameter"),
                    "Mixed");
                DrawReadOnlyRow(
                    new GUIContent("Effective Radius Range"),
                    "Mixed");
            }
            else
            {
                float spacingMetres = Mathf.Max(
                    0.10f,
                    chipSpacing.floatValue);
                float sizeAuthority = Mathf.Clamp01(
                    chipSize.floatValue);
                float radiusRatio = Mathf.Lerp(
                    0.05f,
                    0.65f,
                    sizeAuthority);
                float meanRadius = spacingMetres * radiusRatio;
                float irregularity = Mathf.Clamp01(
                    chipIrregularity.floatValue);
                float minimumMultiplier = Mathf.Lerp(
                    1f,
                    0.80f,
                    irregularity);
                float maximumMultiplier = Mathf.Lerp(
                    1f,
                    1.40f,
                    irregularity);
                float expectedMeanMultiplier = Mathf.Lerp(
                    1f,
                    1.10f,
                    irregularity);
                float expectedMeanRadius =
                    meanRadius * expectedMeanMultiplier;
                DrawReadOnlyRow(
                    new GUIContent(
                        "Expected Mean Radius",
                        "Average candidate radius after the current Chip Irregularity size distribution."),
                    $"{expectedMeanRadius:0.###} m");
                DrawReadOnlyRow(
                    new GUIContent("Expected Mean Diameter"),
                    $"{expectedMeanRadius * 2f:0.###} m");
                DrawReadOnlyRow(
                    new GUIContent("Effective Radius Range"),
                    $"{meanRadius * minimumMultiplier:0.###}–" +
                    $"{meanRadius * maximumMultiplier:0.###} m");
            }

            DrawUnboundedNonNegativeSlider(
                Find("foamChipEdgeWidthPixels"),
                new GUIContent(
                    "Chip Edge Width (px)",
                    "Approximate inward width of the canonical visible Foam edge band in rendered pixels. Zero disables edge permission exactly. The slider covers 0–256 px; the numeric field accepts any non-negative value for deliberately extreme bands."),
                0f,
                256f);
            EditorGUILayout.PropertyField(
                Find("foamChipInteriorAccess"),
                new GUIContent(
                    "Chip Interior Access",
                    "Fraction of activated candidate identities granted permission in the established body outside Chip Edge Width. Zero is edge-only; one lets every active candidate cut the full visible body. Intermediate values admit complete deterministic candidates, not pixel noise."));
            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "View Readability LOD",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Chip identity and animation stay in River-space metres. This bounded rendering LOD enlarges undersized distant Chips first, then fades candidates that still cannot reach a useful projected size. Close Chips are never shrunk, and the decision is resolved before pulse/lifecycle so formation and death retain exact zero.",
                MessageType.Info);
            SerializedProperty chipStableScreenRadiusPixels = Find(
                "foamChipStableScreenRadiusPixels");
            SerializedProperty chipMaximumViewScale = Find(
                "foamChipMaximumViewScale");
            EditorGUILayout.PropertyField(
                chipStableScreenRadiusPixels,
                new GUIContent(
                    "Minimum Stable Radius (px)",
                    "Target readable screen radius for each fully formed Chip. Zero keeps the previous pure world-space behavior. Positive values use bounded enlargement, then fade candidates that remain below 65–100% of the target. The range extends to 16 px for deliberate isometric-camera readability tests."));
            EditorGUILayout.PropertyField(
                chipMaximumViewScale,
                new GUIContent(
                    "Maximum View Scale",
                    "Largest permitted readability enlargement. One disables enlargement; 1.75 permits at most 75% extra radius before the existing spacing-relative cap."));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Lifecycle — Always Active",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Every activated candidate owns a deterministic four-stage cycle: monotonic Formation, fully formed Stable time, monotonic Dissolve, then exact zero coverage for Dormant Time. Motion and living-variation controls cannot disable death or shorten the authored dormant wait.",
                MessageType.Info);
            SerializedProperty chipFormationTime = Find(
                "foamChipFormationTime");
            SerializedProperty chipStableTime = Find(
                "foamChipStableTime");
            SerializedProperty chipDissolveTime = Find(
                "foamChipDissolveTime");
            SerializedProperty chipDormantTime = Find(
                "foamChipDormantTime");
            EditorGUILayout.PropertyField(
                chipFormationTime,
                new GUIContent(
                    "Formation Time (s)",
                    "Seconds for one candidate to grow monotonically from zero radius to its authored living radius."));
            EditorGUILayout.PropertyField(
                chipStableTime,
                new GUIContent(
                    "Stable Time (s)",
                    "Seconds the candidate remains fully formed before dissolution. Size pulse and shape change operate only inside this stage and ease at its ends."));
            EditorGUILayout.PropertyField(
                chipDissolveTime,
                new GUIContent(
                    "Dissolve Time (s)",
                    "Seconds for one candidate to shrink monotonically from its living radius to exact zero."));
            EditorGUILayout.PropertyField(
                chipDormantTime,
                new GUIContent(
                    "Dormant Time (s)",
                    "Seconds the same deterministic candidate remains completely absent before it begins forming again."));

            bool mixedLifecycleTimes =
                chipFormationTime.hasMultipleDifferentValues ||
                chipStableTime.hasMultipleDifferentValues ||
                chipDissolveTime.hasMultipleDifferentValues ||
                chipDormantTime.hasMultipleDifferentValues;
            if (mixedLifecycleTimes)
            {
                DrawReadOnlyRow(
                    new GUIContent("Total Candidate Cycle"),
                    "Mixed");
            }
            else
            {
                float totalCycle =
                    chipFormationTime.floatValue +
                    chipStableTime.floatValue +
                    chipDissolveTime.floatValue +
                    chipDormantTime.floatValue;
                DrawReadOnlyRow(
                    new GUIContent(
                        "Total Candidate Cycle",
                        "Formation + Stable + Dissolve + Dormant."),
                    $"{totalCycle:0.##} s");
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Rigid Motion",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Downstream and lateral movement are rigid translations; rotation is an orthonormal angular transform. None of these paths deform the candidate coordinate field, so movement cannot smear or stretch an individual Chip.",
                MessageType.Info);
            EditorGUILayout.PropertyField(
                Find("foamChipFieldSpeed"),
                new GUIContent(
                    "Downstream Speed (m/s)",
                    "Rigid downstream translation speed of the complete candidate field. Zero keeps the field fixed in River space."));
            SerializedProperty chipLateralMotionAmount = Find(
                "foamChipLateralMotionAmount");
            EditorGUILayout.PropertyField(
                chipLateralMotionAmount,
                new GUIContent(
                    "Lateral Motion Amount (spacing)",
                    "Maximum plus/minus rigid lateral travel as a fraction of Chip Spacing. One means one full spacing in either direction; 2.5 means two and a half spacings. The candidate search expands laterally to preserve complete contours."));
            SerializedProperty chipLateralMotionSpeed = Find(
                "foamChipLateralMotionSpeed");
            EditorGUILayout.PropertyField(
                chipLateralMotionSpeed,
                new GUIContent(
                    "Lateral Motion Speed (cycles/s)",
                    "Independent lateral oscillation frequency. Zero returns candidates to their static lateral centres."));

            bool mixedLateralMotion =
                chipSpacing.hasMultipleDifferentValues ||
                chipLateralMotionAmount.hasMultipleDifferentValues ||
                chipLateralMotionSpeed.hasMultipleDifferentValues;
            if (mixedLateralMotion)
            {
                DrawReadOnlyRow(
                    new GUIContent("Resolved Lateral Excursion"),
                    "Mixed");
                DrawReadOnlyRow(
                    new GUIContent("Peak Lateral Speed"),
                    "Mixed");
            }
            else
            {
                float spacingMetres = Mathf.Max(
                    0.10f,
                    chipSpacing.floatValue);
                float lateralAmountSpacings = Mathf.Clamp(
                    chipLateralMotionAmount.floatValue,
                    0f,
                    2.5f);
                float lateralSpeedCycles = Mathf.Clamp(
                    chipLateralMotionSpeed.floatValue,
                    0f,
                    1f);
                float excursionMetres =
                    spacingMetres * lateralAmountSpacings;

                // RiverWaterFoamResolveChipSignedWave is a smoothstep-shaped
                // triangle wave. Its maximum slope is exactly 6 per cycle, so
                // peak physical centre speed is 6 × frequency × excursion.
                float peakSpeedMetresPerSecond =
                    6f * lateralSpeedCycles * excursionMetres;
                DrawReadOnlyRow(
                    new GUIContent(
                        "Resolved Lateral Excursion",
                        "Maximum rigid centre displacement in metres and its full peak-to-peak travel."),
                    $"±{excursionMetres:0.###} m " +
                    $"({excursionMetres * 2f:0.###} m peak-to-peak)");
                DrawReadOnlyRow(
                    new GUIContent(
                        "Peak Lateral Speed",
                        "Exact peak centre speed for the shader's smooth periodic lateral wave."),
                    $"{peakSpeedMetresPerSecond:0.###} m/s");
            }
            EditorGUILayout.PropertyField(
                Find("foamChipRotationAmountDegrees"),
                new GUIContent(
                    "Rotation Amount (deg)",
                    "Maximum plus/minus angular excursion from each candidate's static orientation. Circular candidates do not visibly rotate."));
            EditorGUILayout.PropertyField(
                Find("foamChipRotationSpeed"),
                new GUIContent(
                    "Rotation Speed (cycles/s)",
                    "Independent angular oscillation frequency. Zero restores the static orientation."));

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Living Variation",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.HelpBox(
                "Size pulse and shape change operate only while a candidate is established. Size Pulse owns radius variation. Shape Change independently redistributes silhouette geometry through a candidate-specific multi-axis harmonic trajectory and eases back to the static contour before Dissolve. Lifecycle still reaches zero and waits through Dormant Time even when every variation amount is zero.",
                MessageType.Info);
            SerializedProperty chipSizePulseAmount = Find(
                "foamChipSizePulseAmount");
            EditorGUILayout.PropertyField(
                chipSizePulseAmount,
                new GUIContent(
                    "Size Pulse Amount",
                    "Fractional living-radius excursion. 0.20 means 80%-120% of the authored living radius; it never controls birth, death, or dormancy."));
            EditorGUILayout.PropertyField(
                Find("foamChipSizePulseSpeed"),
                new GUIContent(
                    "Size Pulse Speed (cycles/s)",
                    "Independent established-stage pulse frequency. Zero keeps the living radius at its authored value."));
            SerializedProperty chipShapeChangeAmount = Find(
                "foamChipShapeChangeAmount");
            EditorGUILayout.PropertyField(
                chipShapeChangeAmount,
                new GUIContent(
                    "Shape Change Amount",
                    "Authority of multi-axis temporal silhouette morphing. Zero preserves the static contour; one blends toward the full candidate-specific sine-harmonic trajectory. Squared-radius blending preserves temporal radial area exactly, so this does not become a Size Pulse. Shape Change remains visible when Chip Irregularity is zero; redistributed lobes can reach up to 1.52x the area-equivalent mean radius and are covered by the adaptive search."));
            SerializedProperty chipShapeChangeCadence = Find(
                "foamChipShapeChangeSpeed");
            SerializedProperty chipShapeTransitionTime = Find(
                "foamChipShapeTransitionTime");
            EditorGUILayout.PropertyField(
                chipShapeChangeCadence,
                new GUIContent(
                    "Shape Change Cadence (changes/s)",
                    "How often a candidate selects its next deterministic contour target. This controls target cadence only; it does not control how quickly the geometry moves between targets. Zero preserves the current static contour."));
            EditorGUILayout.PropertyField(
                chipShapeTransitionTime,
                new GUIContent(
                    "Shape Transition Time (s)",
                    "Seconds spent morphing between consecutive contour targets. Larger values slow the actual geometric change. When this is longer than the cadence interval, the shader uses the complete interval and remains in continuous motion without an abrupt switch."));

            if (chipShapeChangeCadence.hasMultipleDifferentValues ||
                chipShapeTransitionTime.hasMultipleDifferentValues)
            {
                DrawReadOnlyRow(
                    new GUIContent("Resolved Shape Timing"),
                    "Mixed");
            }
            else
            {
                float cadence = Mathf.Max(
                    0f,
                    chipShapeChangeCadence.floatValue);
                float authoredTransition = Mathf.Max(
                    0.10f,
                    chipShapeTransitionTime.floatValue);
                if (cadence <= 0.0001f)
                {
                    DrawReadOnlyRow(
                        new GUIContent("Resolved Shape Timing"),
                        "Static");
                }
                else
                {
                    float interval = 1f / cadence;
                    float effectiveTransition = Mathf.Min(
                        authoredTransition,
                        interval);
                    float hold = Mathf.Max(
                        0f,
                        interval - effectiveTransition);
                    DrawReadOnlyRow(
                        new GUIContent("Resolved Shape Timing"),
                        $"{effectiveTransition:0.###} s transition / {hold:0.###} s hold");
                }
            }

            bool mixedSearchInputs =
                chipSize.hasMultipleDifferentValues ||
                chipIrregularity.hasMultipleDifferentValues ||
                chipSizePulseAmount.hasMultipleDifferentValues ||
                chipShapeChangeAmount.hasMultipleDifferentValues ||
                chipLateralMotionAmount.hasMultipleDifferentValues ||
                chipStableScreenRadiusPixels.hasMultipleDifferentValues ||
                chipMaximumViewScale.hasMultipleDifferentValues;
            if (mixedSearchInputs)
            {
                DrawReadOnlyRow(
                    new GUIContent("Candidate Search"),
                    "Mixed");
            }
            else
            {
                float authoredRadiusRatio = Mathf.Lerp(
                    0.05f,
                    0.65f,
                    Mathf.Clamp01(chipSize.floatValue));
                float viewScaleCeiling =
                    chipStableScreenRadiusPixels.floatValue > 0.0001f
                        ? Mathf.Clamp(
                            chipMaximumViewScale.floatValue,
                            1f,
                            2.5f)
                        : 1f;
                float stabilizedRadiusRatio = Mathf.Min(
                    0.65f,
                    authoredRadiusRatio * viewScaleCeiling);
                float maximumShapeReachScale = Mathf.Sqrt(
                    Mathf.Lerp(
                        1f,
                        1.52f * 1.52f,
                        Mathf.Clamp01(chipShapeChangeAmount.floatValue)));
                float maximumRadiusReachInSpacings =
                    stabilizedRadiusRatio *
                    Mathf.Lerp(
                        1f,
                        1.40f,
                        Mathf.Clamp01(chipIrregularity.floatValue)) *
                    (1f + Mathf.Clamp(
                        chipSizePulseAmount.floatValue,
                        0f,
                        0.45f)) *
                    maximumShapeReachScale;
                float maximumLateralReachInSpacings =
                    maximumRadiusReachInSpacings +
                    Mathf.Clamp(
                        chipLateralMotionAmount.floatValue,
                        0f,
                        2.5f);
                float cellCentreReach = 0.5f +
                    0.39f * Mathf.Clamp01(
                        chipIrregularity.floatValue);
                int downstreamOffset = Mathf.Clamp(
                    Mathf.FloorToInt(
                        maximumRadiusReachInSpacings +
                        cellCentreReach +
                        0.0001f),
                    1,
                    2);
                int lateralOffset = Mathf.Clamp(
                    Mathf.FloorToInt(
                        maximumLateralReachInSpacings +
                        cellCentreReach +
                        0.0001f),
                    1,
                    5);
                int downstreamCells = downstreamOffset * 2 + 1;
                int lateralCells = lateralOffset * 2 + 1;
                DrawReadOnlyRow(
                    new GUIContent(
                        "Candidate Search",
                        "Smallest rectangular source-cell search that encloses maximum Chip Size, multi-axis shape-lobe reach, deterministic centre jitter, bounded view scale, size pulse, and rigid lateral excursion. Maximum is 5×11."),
                    $"{downstreamCells}×{lateralCells} adaptive " +
                    $"(R {maximumRadiusReachInSpacings:0.###}, " +
                    $"Y {maximumLateralReachInSpacings:0.###} spacing)");
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(
                "Temporal Shape",
                EditorStyles.miniBoldLabel);
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
            EditorGUILayout.LabelField(
                "General Composition",
                EditorStyles.miniBoldLabel);
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

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField(
                "Foam Strands",
                EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(
                Find("foamStrandStrength"),
                new GUIContent(
                    "Strand Strength",
                    "Controls structural anisotropic lineification. Zero gives the coherent Foam body; higher values create elongated cuts and remnants."));
            EditorGUILayout.PropertyField(
                Find("foamStrandScale"),
                new GUIContent(
                    "Strand Scale",
                    "Controls the structural Strand size hierarchy. Zero retains medium subdivisions; one keeps broader, simpler structures."));
            EditorGUILayout.PropertyField(
                Find("foamStrandDensity"),
                new GUIContent(
                    "Strand Density",
                    "Controls how much of the candidate Strand field survives. Zero gives sparse selected groups; one gives denser groups without changing cut depth."));
            EditorGUILayout.PropertyField(
                Find("foamStrandReach"),
                new GUIContent(
                    "Strand Reach",
                    "Controls how deeply selected Strand regions penetrate weak-to-medium Foam. Zero stays shallow near weak edges; one permits deeper channels without changing candidate density."));
            EditorGUILayout.HelpBox(
                "Strands own structural anisotropic lineification. Scale, Density, and Reach shape elongated cuts and remnants.",
                MessageType.None);

        }

        private static void DrawUnboundedNonNegativeSlider(
            SerializedProperty property,
            GUIContent label,
            float sliderMinimum,
            float sliderMaximum)
        {
            Rect row = EditorGUILayout.GetControlRect();
            row = EditorGUI.PrefixLabel(row, label);
            const float FieldWidth = 72f;
            const float Gap = 4f;
            Rect sliderRect = new Rect(
                row.x,
                row.y,
                Mathf.Max(0f, row.width - FieldWidth - Gap),
                row.height);
            Rect fieldRect = new Rect(
                sliderRect.xMax + Gap,
                row.y,
                FieldWidth,
                row.height);

            float current = Mathf.Max(0f, property.floatValue);
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

            EditorGUI.BeginChangeCheck();
            float sliderValue = GUI.HorizontalSlider(
                sliderRect,
                Mathf.Clamp(current, sliderMinimum, sliderMaximum),
                sliderMinimum,
                sliderMaximum);
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = Mathf.Max(0f, sliderValue);
                current = property.floatValue;
            }

            EditorGUI.BeginChangeCheck();
            float fieldValue = EditorGUI.FloatField(fieldRect, current);
            if (EditorGUI.EndChangeCheck())
            {
                property.floatValue = Mathf.Max(0f, fieldValue);
            }

            EditorGUI.showMixedValue = false;
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
                        "Broad global scale selector for shore-source path dimensions. Shore Ribbon contact thickness is authored separately in cross-river cells; Inward Wash retains metre-based width and reach."));
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
                    EditorGUILayout.PropertyField(
                        Find("foamShoreRibbonThicknessCells"),
                        new GUIContent(
                            "Source Thickness",
                            "Bank-normal source thickness measured in cross-river Foam cells. One produces one contact-attached source cell; source amount and activity remain separate controls."));
                    EditorGUILayout.PropertyField(
                        Find("foamShoreRibbonOffsetMetres"),
                        new GUIContent(
                            "Source Offset",
                            "Base inward offset from the live shore edge in metres. Keep this close to zero for a contact-attached ribbon."));
                    EditorGUILayout.PropertyField(
                        Find("foamShoreRibbonOffsetVariationCells"),
                        new GUIContent(
                            "Offset Variation",
                            "Deterministic event-to-event offset variation measured in cross-river Foam cells. This should not be used to create separated parallel bands."));
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
